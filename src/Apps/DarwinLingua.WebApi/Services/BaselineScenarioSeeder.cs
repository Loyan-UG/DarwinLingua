using System.Text.Json;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

public interface IBaselineScenarioSeeder
{
    Task SeedAsync(CancellationToken cancellationToken);
}

internal sealed class BaselineScenarioSeeder(
    IDbContextFactory<DarwinLinguaDbContext> dbContextFactory,
    IWebHostEnvironment environment,
    ILogger<BaselineScenarioSeeder> logger) : IBaselineScenarioSeeder
{
    private const string RelativeSeedPath = "SeedContent/scenarios-baseline-v1.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        string seedPath = Path.Combine(environment.ContentRootPath, RelativeSeedPath);
        if (!File.Exists(seedPath))
        {
            logger.LogWarning("Baseline scenario seed file was not found at {SeedPath}.", seedPath);
            return;
        }

        await using FileStream stream = File.OpenRead(seedPath);
        BaselineScenarioSeedDocument? document = await JsonSerializer
            .DeserializeAsync<BaselineScenarioSeedDocument>(stream, SerializerOptions, cancellationToken)
            .ConfigureAwait(false);

        if (document?.Scenarios is null || document.Scenarios.Count == 0)
        {
            return;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        string[] seedSlugs = document.Scenarios
            .Select(static scenario => NormalizeSlug(scenario.Slug))
            .Where(static slug => slug.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Dictionary<string, ScenarioLesson> existingScenariosBySlug = await dbContext.ScenarioLessons
            .Include(scenario => scenario.Topics)
            .Where(scenario => seedSlugs.Contains(scenario.Slug))
            .ToDictionaryAsync(scenario => scenario.Slug, StringComparer.OrdinalIgnoreCase, cancellationToken)
            .ConfigureAwait(false);

        string[] requestedTopicKeys = document.Scenarios
            .SelectMany(static scenario => scenario.Topics ?? [])
            .Select(static topic => NormalizeSlug(topic))
            .Where(static topic => topic.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Dictionary<string, Guid> topicIdsByKey = await dbContext.Topics
            .AsNoTracking()
            .Where(topic => requestedTopicKeys.Contains(topic.Key))
            .ToDictionaryAsync(topic => topic.Key, topic => topic.Id, StringComparer.OrdinalIgnoreCase, cancellationToken)
            .ConfigureAwait(false);

        int createdCount = 0;
        int linkedTopicCount = 0;
        foreach (BaselineScenarioSeedItem item in document.Scenarios)
        {
            string slug = NormalizeSlug(item.Slug);
            if (slug.Length == 0)
            {
                continue;
            }

            DateTime now = DateTime.UtcNow;
            if (existingScenariosBySlug.TryGetValue(slug, out ScenarioLesson? existingScenario))
            {
                linkedTopicCount += EnsureTopics(existingScenario, item.Topics, topicIdsByKey, now);
                continue;
            }

            ScenarioLesson scenario = CreateScenario(item, slug, topicIdsByKey, now);
            dbContext.ScenarioLessons.Add(scenario);
            existingScenariosBySlug.Add(slug, scenario);
            createdCount++;
        }

        if (createdCount > 0 || linkedTopicCount > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation(
                "Seeded {CreatedCount} baseline scenario lessons and linked {LinkedTopicCount} scenario topics.",
                createdCount,
                linkedTopicCount);
        }
    }

    private static ScenarioLesson CreateScenario(
        BaselineScenarioSeedItem item,
        string slug,
        IReadOnlyDictionary<string, Guid> topicIdsByKey,
        DateTime now)
    {
        ScenarioLesson scenario = new(
            Guid.NewGuid(),
            slug,
            Required(item.Title, nameof(item.Title)),
            Required(item.Description, nameof(item.Description)),
            Required(item.LearnerGoal, nameof(item.LearnerGoal)),
            ParseCefr(item.CefrLevel),
            string.IsNullOrWhiteSpace(item.Category) ? "general" : item.Category,
            ParsePublicationStatus(item.PublicationStatus),
            Math.Max(0, item.SortOrder),
            now);

        EnsureTopics(scenario, item.Topics, topicIdsByKey, now);

        foreach (BaselineScenarioDialogueTurnSeedItem turnItem in item.DialogueTurns ?? [])
        {
            ScenarioDialogueTurn turn = scenario.AddDialogueTurn(
                Guid.NewGuid(),
                turnItem.SortOrder,
                Required(turnItem.SpeakerRole, nameof(turnItem.SpeakerRole)),
                Required(turnItem.BaseText, nameof(turnItem.BaseText)),
                now);
            AddTranslations(turn.AddTranslation, turnItem.Translations, now);
        }

        foreach (BaselineScenarioPhraseSeedItem phraseItem in item.UsefulPhrases ?? [])
        {
            ScenarioPhrase phrase = scenario.AddUsefulPhrase(
                Guid.NewGuid(),
                phraseItem.SortOrder,
                Required(phraseItem.BaseText, nameof(phraseItem.BaseText)),
                phraseItem.UsageNote,
                now);
            AddTranslations(phrase.AddTranslation, phraseItem.Translations, now);
        }

        foreach (BaselineScenarioQuestionSeedItem questionItem in item.Questions ?? [])
        {
            ScenarioQuestion question = scenario.AddQuestion(
                Guid.NewGuid(),
                questionItem.SortOrder,
                Required(questionItem.Prompt, nameof(questionItem.Prompt)),
                now);
            AddTranslations(question.AddTranslation, questionItem.Translations, now);

            foreach (BaselineScenarioAnswerSeedItem answerItem in questionItem.Answers ?? [])
            {
                ScenarioAnswer answer = question.AddAnswer(
                    Guid.NewGuid(),
                    answerItem.SortOrder,
                    Required(answerItem.Text, nameof(answerItem.Text)),
                    answerItem.IsCorrect,
                    answerItem.Feedback,
                    now);
                AddTranslations(answer.AddTranslation, answerItem.Translations, now);
            }
        }

        return scenario;
    }

    private static int EnsureTopics(
        ScenarioLesson scenario,
        IReadOnlyList<string>? topicKeys,
        IReadOnlyDictionary<string, Guid> topicIdsByKey,
        DateTime now)
    {
        int linkedCount = 0;
        bool hasPrimaryTopic = scenario.Topics.Any(static topic => topic.IsPrimary);
        HashSet<Guid> existingTopicIds = scenario.Topics
            .Select(static topic => topic.TopicId)
            .ToHashSet();

        foreach (string topicKey in topicKeys ?? [])
        {
            string normalizedTopicKey = NormalizeSlug(topicKey);
            if (!topicIdsByKey.TryGetValue(normalizedTopicKey, out Guid topicId) || existingTopicIds.Contains(topicId))
            {
                continue;
            }

            scenario.AddTopic(Guid.NewGuid(), topicId, !hasPrimaryTopic, now);
            hasPrimaryTopic = true;
            existingTopicIds.Add(topicId);
            linkedCount++;
        }

        return linkedCount;
    }

    private static void AddTranslations(
        Action<Guid, LanguageCode, string, DateTime> addTranslation,
        IReadOnlyList<BaselineScenarioTranslationSeedItem>? translations,
        DateTime now)
    {
        IReadOnlyList<string> missingLanguages = ContentLanguageRequirements.FindMissingMeaningLanguages(
            (translations ?? []).Select(static translation => translation.LanguageCode ?? string.Empty));
        if (missingLanguages.Count > 0)
        {
            throw new InvalidOperationException(
                $"Baseline scenario translations are missing languages: {string.Join(", ", missingLanguages)}.");
        }

        foreach (BaselineScenarioTranslationSeedItem translation in translations ?? [])
        {
            addTranslation(
                Guid.NewGuid(),
                LanguageCode.From(Required(translation.LanguageCode, nameof(translation.LanguageCode))),
                Required(translation.Text, nameof(translation.Text)),
                now);
        }
    }

    private static string NormalizeSlug(string? slug) =>
        string.IsNullOrWhiteSpace(slug) ? string.Empty : slug.Trim().ToLowerInvariant();

    private static CefrLevel ParseCefr(string? value) =>
        Enum.TryParse(value, ignoreCase: true, out CefrLevel level) && Enum.IsDefined(level)
            ? level
            : CefrLevel.A1;

    private static PublicationStatus ParsePublicationStatus(string? value) =>
        Enum.TryParse(value, ignoreCase: true, out PublicationStatus status) && Enum.IsDefined(status)
            ? status
            : PublicationStatus.Draft;

    private static string Required(string? value, string fieldName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"Baseline scenario field '{fieldName}' is required.")
            : value.Trim();

    private sealed class BaselineScenarioSeedDocument
    {
        public List<BaselineScenarioSeedItem>? Scenarios { get; set; }
    }

    private sealed class BaselineScenarioSeedItem
    {
        public string? Slug { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? LearnerGoal { get; set; }

        public string? CefrLevel { get; set; }

        public string? Category { get; set; }

        public string? PublicationStatus { get; set; }

        public int SortOrder { get; set; }

        public List<string>? Topics { get; set; }

        public List<BaselineScenarioDialogueTurnSeedItem>? DialogueTurns { get; set; }

        public List<BaselineScenarioPhraseSeedItem>? UsefulPhrases { get; set; }

        public List<BaselineScenarioQuestionSeedItem>? Questions { get; set; }
    }

    private sealed class BaselineScenarioDialogueTurnSeedItem
    {
        public int SortOrder { get; set; }

        public string? SpeakerRole { get; set; }

        public string? BaseText { get; set; }

        public List<BaselineScenarioTranslationSeedItem>? Translations { get; set; }
    }

    private sealed class BaselineScenarioPhraseSeedItem
    {
        public int SortOrder { get; set; }

        public string? BaseText { get; set; }

        public string? UsageNote { get; set; }

        public List<BaselineScenarioTranslationSeedItem>? Translations { get; set; }
    }

    private sealed class BaselineScenarioQuestionSeedItem
    {
        public int SortOrder { get; set; }

        public string? Prompt { get; set; }

        public List<BaselineScenarioTranslationSeedItem>? Translations { get; set; }

        public List<BaselineScenarioAnswerSeedItem>? Answers { get; set; }
    }

    private sealed class BaselineScenarioAnswerSeedItem
    {
        public int SortOrder { get; set; }

        public string? Text { get; set; }

        public bool IsCorrect { get; set; }

        public string? Feedback { get; set; }

        public List<BaselineScenarioTranslationSeedItem>? Translations { get; set; }
    }

    private sealed class BaselineScenarioTranslationSeedItem
    {
        public string? LanguageCode { get; set; }

        public string? Text { get; set; }
    }
}

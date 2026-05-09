using System.Text.Json;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

public interface IBaselineDialogueSeeder
{
    Task SeedAsync(CancellationToken cancellationToken);
}

internal sealed class BaselineDialogueSeeder(
    IDbContextFactory<DarwinLinguaDbContext> dbContextFactory,
    IWebHostEnvironment environment,
    ILogger<BaselineDialogueSeeder> logger) : IBaselineDialogueSeeder
{
    private const string RelativeSeedPath = "SeedContent/dialogues-baseline-v1.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        string seedPath = Path.Combine(environment.ContentRootPath, RelativeSeedPath);
        if (!File.Exists(seedPath))
        {
            logger.LogWarning("Baseline dialogue seed file was not found at {SeedPath}.", seedPath);
            return;
        }

        await using FileStream stream = File.OpenRead(seedPath);
        BaselineDialogueSeedDocument? document = await JsonSerializer
            .DeserializeAsync<BaselineDialogueSeedDocument>(stream, SerializerOptions, cancellationToken)
            .ConfigureAwait(false);

        if (document?.Dialogues is null || document.Dialogues.Count == 0)
        {
            return;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        string[] seedSlugs = document.Dialogues
            .Select(static dialogue => NormalizeSlug(dialogue.Slug))
            .Where(static slug => slug.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Dictionary<string, DialogueLesson> existingDialoguesBySlug = await dbContext.DialogueLessons
            .Include(dialogue => dialogue.Topics)
            .Where(dialogue => seedSlugs.Contains(dialogue.Slug))
            .ToDictionaryAsync(dialogue => dialogue.Slug, StringComparer.OrdinalIgnoreCase, cancellationToken)
            .ConfigureAwait(false);

        string[] requestedTopicKeys = document.Dialogues
            .SelectMany(static dialogue => dialogue.Topics ?? [])
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
        foreach (BaselineDialogueSeedItem item in document.Dialogues)
        {
            string slug = NormalizeSlug(item.Slug);
            if (slug.Length == 0)
            {
                continue;
            }

            DateTime now = DateTime.UtcNow;
            if (existingDialoguesBySlug.TryGetValue(slug, out DialogueLesson? existingDialogue))
            {
                linkedTopicCount += EnsureTopics(existingDialogue, item.Topics, topicIdsByKey, now);
                continue;
            }

            DialogueLesson dialogue = CreateDialogue(item, slug, topicIdsByKey, now);
            dbContext.DialogueLessons.Add(dialogue);
            existingDialoguesBySlug.Add(slug, dialogue);
            createdCount++;
        }

        if (createdCount > 0 || linkedTopicCount > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation(
                "Seeded {CreatedCount} baseline dialogue lessons and linked {LinkedTopicCount} dialogue topics.",
                createdCount,
                linkedTopicCount);
        }
    }

    private static DialogueLesson CreateDialogue(
        BaselineDialogueSeedItem item,
        string slug,
        IReadOnlyDictionary<string, Guid> topicIdsByKey,
        DateTime now)
    {
        DialogueLesson dialogue = new(
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

        EnsureTopics(dialogue, item.Topics, topicIdsByKey, now);

        foreach (BaselineDialogueTurnSeedItem turnItem in item.DialogueTurns ?? [])
        {
            DialogueTurn turn = dialogue.AddDialogueTurn(
                Guid.NewGuid(),
                turnItem.SortOrder,
                Required(turnItem.SpeakerRole, nameof(turnItem.SpeakerRole)),
                Required(turnItem.BaseText, nameof(turnItem.BaseText)),
                now);
            AddTranslations(turn.AddTranslation, turnItem.Translations, now);
        }

        foreach (BaselineDialoguePhraseSeedItem phraseItem in item.UsefulPhrases ?? [])
        {
            DialoguePhrase phrase = dialogue.AddUsefulPhrase(
                Guid.NewGuid(),
                phraseItem.SortOrder,
                Required(phraseItem.BaseText, nameof(phraseItem.BaseText)),
                phraseItem.UsageNote,
                now);
            AddTranslations(phrase.AddTranslation, phraseItem.Translations, now);
        }

        foreach (BaselineDialogueQuestionSeedItem questionItem in item.Questions ?? [])
        {
            DialogueQuestion question = dialogue.AddQuestion(
                Guid.NewGuid(),
                questionItem.SortOrder,
                Required(questionItem.Prompt, nameof(questionItem.Prompt)),
                now);
            AddTranslations(question.AddTranslation, questionItem.Translations, now);

            foreach (BaselineDialogueAnswerSeedItem answerItem in questionItem.Answers ?? [])
            {
                DialogueAnswer answer = question.AddAnswer(
                    Guid.NewGuid(),
                    answerItem.SortOrder,
                    Required(answerItem.Text, nameof(answerItem.Text)),
                    answerItem.IsCorrect,
                    answerItem.Feedback,
                    now);
                AddTranslations(answer.AddTranslation, answerItem.Translations, now);
            }
        }

        return dialogue;
    }

    private static int EnsureTopics(
        DialogueLesson dialogue,
        IReadOnlyList<string>? topicKeys,
        IReadOnlyDictionary<string, Guid> topicIdsByKey,
        DateTime now)
    {
        int linkedCount = 0;
        bool hasPrimaryTopic = dialogue.Topics.Any(static topic => topic.IsPrimary);
        HashSet<Guid> existingTopicIds = dialogue.Topics
            .Select(static topic => topic.TopicId)
            .ToHashSet();

        foreach (string topicKey in topicKeys ?? [])
        {
            string normalizedTopicKey = NormalizeSlug(topicKey);
            if (!topicIdsByKey.TryGetValue(normalizedTopicKey, out Guid topicId) || existingTopicIds.Contains(topicId))
            {
                continue;
            }

            dialogue.AddTopic(Guid.NewGuid(), topicId, !hasPrimaryTopic, now);
            hasPrimaryTopic = true;
            existingTopicIds.Add(topicId);
            linkedCount++;
        }

        return linkedCount;
    }

    private static void AddTranslations(
        Action<Guid, LanguageCode, string, DateTime> addTranslation,
        IReadOnlyList<BaselineDialogueTranslationSeedItem>? translations,
        DateTime now)
    {
        IReadOnlyList<string> missingLanguages = ContentLanguageRequirements.FindMissingMeaningLanguages(
            (translations ?? []).Select(static translation => translation.LanguageCode ?? string.Empty));
        if (missingLanguages.Count > 0)
        {
            throw new InvalidOperationException(
                $"Baseline dialogue translations are missing languages: {string.Join(", ", missingLanguages)}.");
        }

        foreach (BaselineDialogueTranslationSeedItem translation in translations ?? [])
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
            ? throw new InvalidOperationException($"Baseline dialogue field '{fieldName}' is required.")
            : value.Trim();

    private sealed class BaselineDialogueSeedDocument
    {
        public List<BaselineDialogueSeedItem>? Dialogues { get; set; }
    }

    private sealed class BaselineDialogueSeedItem
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

        public List<BaselineDialogueTurnSeedItem>? DialogueTurns { get; set; }

        public List<BaselineDialoguePhraseSeedItem>? UsefulPhrases { get; set; }

        public List<BaselineDialogueQuestionSeedItem>? Questions { get; set; }
    }

    private sealed class BaselineDialogueTurnSeedItem
    {
        public int SortOrder { get; set; }

        public string? SpeakerRole { get; set; }

        public string? BaseText { get; set; }

        public List<BaselineDialogueTranslationSeedItem>? Translations { get; set; }
    }

    private sealed class BaselineDialoguePhraseSeedItem
    {
        public int SortOrder { get; set; }

        public string? BaseText { get; set; }

        public string? UsageNote { get; set; }

        public List<BaselineDialogueTranslationSeedItem>? Translations { get; set; }
    }

    private sealed class BaselineDialogueQuestionSeedItem
    {
        public int SortOrder { get; set; }

        public string? Prompt { get; set; }

        public List<BaselineDialogueTranslationSeedItem>? Translations { get; set; }

        public List<BaselineDialogueAnswerSeedItem>? Answers { get; set; }
    }

    private sealed class BaselineDialogueAnswerSeedItem
    {
        public int SortOrder { get; set; }

        public string? Text { get; set; }

        public bool IsCorrect { get; set; }

        public string? Feedback { get; set; }

        public List<BaselineDialogueTranslationSeedItem>? Translations { get; set; }
    }

    private sealed class BaselineDialogueTranslationSeedItem
    {
        public string? LanguageCode { get; set; }

        public string? Text { get; set; }
    }
}

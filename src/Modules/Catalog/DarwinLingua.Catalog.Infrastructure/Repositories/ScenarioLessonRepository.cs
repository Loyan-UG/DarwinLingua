using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class ScenarioLessonRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IScenarioLessonRepository
{
    private static readonly LanguageCode EnglishFallbackLanguage = LanguageCode.From("en");

    public async Task<IReadOnlyList<ScenarioLessonListItemModel>> GetPublishedScenariosAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        return await dbContext.ScenarioLessons
            .AsNoTracking()
            .Where(lesson => lesson.PublicationStatus == PublicationStatus.Active)
            .OrderBy(lesson => lesson.SortOrder)
            .ThenBy(lesson => lesson.Title)
            .Select(lesson => new ScenarioLessonListItemModel(
                lesson.Slug,
                lesson.Title,
                lesson.Description,
                lesson.LearnerGoal,
                lesson.CefrLevel.ToString(),
                lesson.Category,
                lesson.Topics
                    .OrderByDescending(topic => topic.IsPrimary)
                    .ThenBy(topic => topic.CreatedAtUtc)
                    .Join(dbContext.Topics, link => link.TopicId, topic => topic.Id, (link, topic) => topic.Key)
                    .ToArray()))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<ScenarioLessonDetailModel?> GetPublishedScenarioBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        string normalizedSlug = slug.Trim().ToLowerInvariant();

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        ScenarioLesson? lesson = await dbContext.ScenarioLessons
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.Topics)
            .Include(item => item.DialogueTurns).ThenInclude(turn => turn.Translations)
            .Include(item => item.UsefulPhrases).ThenInclude(phrase => phrase.Translations)
            .Include(item => item.Questions).ThenInclude(question => question.Translations)
            .Include(item => item.Questions).ThenInclude(question => question.Answers).ThenInclude(answer => answer.Translations)
            .Where(item => item.PublicationStatus == PublicationStatus.Active && item.Slug == normalizedSlug)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (lesson is null)
        {
            return null;
        }

        Dictionary<Guid, string> topicKeysById = await dbContext.Topics
            .AsNoTracking()
            .Where(topic => lesson.Topics.Select(link => link.TopicId).Contains(topic.Id))
            .ToDictionaryAsync(topic => topic.Id, topic => topic.Key, cancellationToken)
            .ConfigureAwait(false);

        LanguageCode primaryLanguage = LanguageCode.From(primaryMeaningLanguageCode);
        LanguageCode? secondaryLanguage = string.IsNullOrWhiteSpace(secondaryMeaningLanguageCode)
            ? null
            : LanguageCode.From(secondaryMeaningLanguageCode);

        return new ScenarioLessonDetailModel(
            lesson.Slug,
            lesson.Title,
            lesson.Description,
            lesson.LearnerGoal,
            lesson.CefrLevel.ToString(),
            lesson.Category,
            lesson.Topics
                .OrderByDescending(topic => topic.IsPrimary)
                .ThenBy(topic => topic.CreatedAtUtc)
                .Select(topic => topicKeysById[topic.TopicId])
                .ToArray(),
            lesson.DialogueTurns
                .OrderBy(turn => turn.SortOrder)
                .Select(turn => new ScenarioDialogueTurnModel(
                    turn.SpeakerRole,
                    turn.BaseText,
                    ResolvePrimaryMeaning(turn.Translations, primaryLanguage),
                    ResolveSecondaryMeaning(turn.Translations, secondaryLanguage)))
                .ToArray(),
            lesson.UsefulPhrases
                .OrderBy(phrase => phrase.SortOrder)
                .Select(phrase => new ScenarioPhraseModel(
                    phrase.BaseText,
                    ResolvePrimaryMeaning(phrase.Translations, primaryLanguage),
                    ResolveSecondaryMeaning(phrase.Translations, secondaryLanguage),
                    phrase.UsageNote))
                .ToArray(),
            lesson.Questions
                .OrderBy(question => question.SortOrder)
                .Select(question => new ScenarioQuestionModel(
                    question.Prompt,
                    ResolvePrimaryMeaning(question.Translations, primaryLanguage),
                    ResolveSecondaryMeaning(question.Translations, secondaryLanguage),
                    question.Answers
                        .OrderBy(answer => answer.SortOrder)
                        .Select(answer => new ScenarioAnswerModel(
                            answer.Text,
                            ResolvePrimaryMeaning(answer.Translations, primaryLanguage),
                            ResolveSecondaryMeaning(answer.Translations, secondaryLanguage),
                            answer.IsCorrect,
                            answer.Feedback))
                        .ToArray()))
                .ToArray());
    }

    private static string? ResolvePrimaryMeaning<TTranslation>(
        IReadOnlyCollection<TTranslation> translations,
        LanguageCode primaryLanguage)
        where TTranslation : ScenarioTranslationBase
    {
        return translations
            .Where(translation => translation.LanguageCode == primaryLanguage || translation.LanguageCode == EnglishFallbackLanguage)
            .OrderBy(translation => translation.LanguageCode == primaryLanguage ? 0 : 1)
            .Select(translation => translation.Text)
            .FirstOrDefault();
    }

    private static string? ResolveSecondaryMeaning<TTranslation>(
        IReadOnlyCollection<TTranslation> translations,
        LanguageCode? secondaryLanguage)
        where TTranslation : ScenarioTranslationBase
    {
        return secondaryLanguage is null
            ? null
            : translations
                .Where(translation => translation.LanguageCode == secondaryLanguage)
                .Select(translation => translation.Text)
                .FirstOrDefault();
    }
}

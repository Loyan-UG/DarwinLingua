using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class DialogueLessonRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IDialogueLessonRepository
{
    private static readonly LanguageCode EnglishFallbackLanguage = LanguageCode.From("en");

    public async Task<IReadOnlyList<DialogueLessonListItemModel>> GetPublishedDialoguesAsync(
        DialogueLessonListFilterModel filter,
        CancellationToken cancellationToken) =>
        await GetPublishedDialoguesAsync(filter, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<DialogueLessonListItemModel>> GetPublishedDialoguesAsync(
        DialogueLessonListFilterModel filter,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken)
    {
        string targetLanguageCode = NormalizeRequiredLanguageCode(targetLearningLanguageCode);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        IQueryable<DialogueLesson> query = dbContext.DialogueLessons
            .AsNoTracking()
            .Where(lesson =>
                lesson.PublicationStatus == PublicationStatus.Active &&
                lesson.TargetLearningLanguageCode == targetLanguageCode);

        query = ApplyFilter(query, dbContext, filter);

        return await query
            .OrderBy(lesson => lesson.SortOrder)
            .ThenBy(lesson => lesson.Title)
            .Select(lesson => new DialogueLessonListItemModel(
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
                    .ToArray(),
                lesson.ExamProfiles
                    .OrderBy(profile => profile.SortOrder)
                    .Select(profile => profile.ExamProfile)
                    .ToArray(),
                lesson.SkillFocus
                    .OrderBy(focus => focus.SortOrder)
                    .Select(focus => focus.SkillFocus)
                    .ToArray(),
                lesson.TaskType,
                lesson.InteractionMode,
                lesson.Register,
                lesson.EstimatedPracticeMinutes))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<DialogueLessonDetailModel?> GetPublishedDialogueBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        await GetPublishedDialogueBySlugAsync(
            slug,
            ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
            primaryMeaningLanguageCode,
            secondaryMeaningLanguageCode,
            cancellationToken).ConfigureAwait(false);

    public async Task<DialogueLessonDetailModel?> GetPublishedDialogueBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        string normalizedSlug = slug.Trim().ToLowerInvariant();
        string targetLanguageCode = NormalizeRequiredLanguageCode(targetLearningLanguageCode);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        DialogueLesson? lesson = await dbContext.DialogueLessons
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.Topics)
            .Include(item => item.ExamProfiles)
            .Include(item => item.SkillFocus)
            .Include(item => item.SpeakingFunctions)
            .Include(item => item.UsefulWords)
            .Include(item => item.SpeakingPrompts).ThenInclude(prompt => prompt.Translations)
            .Include(item => item.DialogueTurns).ThenInclude(turn => turn.Translations)
            .Include(item => item.UsefulPhrases).ThenInclude(phrase => phrase.Translations)
            .Include(item => item.Questions).ThenInclude(question => question.Translations)
            .Include(item => item.Questions).ThenInclude(question => question.Answers).ThenInclude(answer => answer.Translations)
            .Where(item =>
                item.PublicationStatus == PublicationStatus.Active &&
                item.TargetLearningLanguageCode == targetLanguageCode &&
                item.Slug == normalizedSlug)
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

        return new DialogueLessonDetailModel(
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
            lesson.ExamProfiles
                .OrderBy(profile => profile.SortOrder)
                .Select(profile => profile.ExamProfile)
                .ToArray(),
            lesson.SkillFocus
                .OrderBy(focus => focus.SortOrder)
                .Select(focus => focus.SkillFocus)
                .ToArray(),
            lesson.TaskType,
            lesson.InteractionMode,
            lesson.Register,
            lesson.SpeakingFunctions
                .OrderBy(function => function.SortOrder)
                .Select(function => function.SpeakingFunction)
                .ToArray(),
            lesson.EstimatedPracticeMinutes,
            lesson.DifficultyNote,
            lesson.ExamRelevance,
            lesson.UsefulWords
                .OrderBy(word => word.SortOrder)
                .Select(word => new DialogueUsefulWordModel(
                    word.Lemma,
                    word.WordSlug,
                    word.CefrLevel.HasValue ? word.CefrLevel.Value.ToString() : null,
                    word.SortOrder))
                .ToArray(),
            lesson.SpeakingPrompts
                .OrderBy(prompt => prompt.SortOrder)
                .Select(prompt => new DialogueSpeakingPromptModel(
                    prompt.PromptType,
                    prompt.Prompt,
                    ResolvePrimaryMeaning(prompt.Translations, primaryLanguage),
                    ResolveSecondaryMeaning(prompt.Translations, secondaryLanguage),
                    prompt.SortOrder))
                .ToArray(),
            lesson.DialogueTurns
                .OrderBy(turn => turn.SortOrder)
                .Select(turn => new DialogueTurnModel(
                    turn.SpeakerRole,
                    turn.BaseText,
                    ResolvePrimaryMeaning(turn.Translations, primaryLanguage),
                    ResolveSecondaryMeaning(turn.Translations, secondaryLanguage)))
                .ToArray(),
            lesson.UsefulPhrases
                .OrderBy(phrase => phrase.SortOrder)
                .Select(phrase => new DialoguePhraseModel(
                    phrase.BaseText,
                    ResolvePrimaryMeaning(phrase.Translations, primaryLanguage),
                    ResolveSecondaryMeaning(phrase.Translations, secondaryLanguage),
                    phrase.UsageNote))
                .ToArray(),
            lesson.Questions
                .OrderBy(question => question.SortOrder)
                .Select(question => new DialogueQuestionModel(
                    question.Prompt,
                    ResolvePrimaryMeaning(question.Translations, primaryLanguage),
                    ResolveSecondaryMeaning(question.Translations, secondaryLanguage),
                    question.Answers
                        .OrderBy(answer => answer.SortOrder)
                        .Select(answer => new DialogueAnswerModel(
                            answer.Text,
                            ResolvePrimaryMeaning(answer.Translations, primaryLanguage),
                            ResolveSecondaryMeaning(answer.Translations, secondaryLanguage),
                            answer.IsCorrect,
                            answer.Feedback))
                        .ToArray()))
                .ToArray());
    }

    private static string NormalizeRequiredLanguageCode(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? ContentLanguageRequirements.DefaultTargetLearningLanguageCode
            : value.Trim().ToLowerInvariant();

    private static IQueryable<DialogueLesson> ApplyFilter(
        IQueryable<DialogueLesson> query,
        DarwinLinguaDbContext dbContext,
        DialogueLessonListFilterModel filter)
    {
        string? cefrLevel = NormalizeOptional(filter.CefrLevel);
        if (cefrLevel is not null && Enum.TryParse(cefrLevel, true, out CefrLevel parsedCefrLevel))
        {
            query = query.Where(lesson => lesson.CefrLevel == parsedCefrLevel);
        }

        string? category = NormalizeOptional(filter.Category);
        if (category is not null)
        {
            query = query.Where(lesson => lesson.Category == category);
        }

        string? topicKey = NormalizeOptional(filter.TopicKey);
        if (topicKey is not null)
        {
            query = query.Where(lesson => lesson.Topics.Any(link =>
                dbContext.Topics.Any(topic => topic.Id == link.TopicId && topic.Key == topicKey)));
        }

        string? examProfile = NormalizeOptional(filter.ExamProfile);
        if (examProfile is not null)
        {
            query = query.Where(lesson => lesson.ExamProfiles.Any(profile => profile.ExamProfile == examProfile));
        }

        string? skillFocus = NormalizeOptional(filter.SkillFocus);
        if (skillFocus is not null)
        {
            query = query.Where(lesson => lesson.SkillFocus.Any(focus => focus.SkillFocus == skillFocus));
        }

        string? taskType = NormalizeOptional(filter.TaskType);
        if (taskType is not null)
        {
            query = query.Where(lesson => lesson.TaskType == taskType);
        }

        string? interactionMode = NormalizeOptional(filter.InteractionMode);
        if (interactionMode is not null)
        {
            query = query.Where(lesson => lesson.InteractionMode == interactionMode);
        }

        string? register = NormalizeOptional(filter.Register);
        if (register is not null)
        {
            query = query.Where(lesson => lesson.Register == register);
        }

        string? searchQuery = string.IsNullOrWhiteSpace(filter.Query) ? null : filter.Query.Trim();
        if (searchQuery is not null)
        {
            string normalizedSearchQuery = searchQuery.ToLowerInvariant();
            query = query.Where(lesson =>
                lesson.Title.ToLower().Contains(normalizedSearchQuery) ||
                lesson.Description.ToLower().Contains(normalizedSearchQuery) ||
                lesson.LearnerGoal.ToLower().Contains(normalizedSearchQuery) ||
                lesson.Category.ToLower().Contains(normalizedSearchQuery) ||
                lesson.ExamProfiles.Any(profile => profile.ExamProfile.ToLower().Contains(normalizedSearchQuery)) ||
                lesson.SkillFocus.Any(focus => focus.SkillFocus.ToLower().Contains(normalizedSearchQuery)));
        }

        return query;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static string? ResolvePrimaryMeaning<TTranslation>(
        IReadOnlyCollection<TTranslation> translations,
        LanguageCode primaryLanguage)
        where TTranslation : DialogueTranslationBase
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
        where TTranslation : DialogueTranslationBase
    {
        return secondaryLanguage is null
            ? null
            : translations
                .Where(translation => translation.LanguageCode == secondaryLanguage)
                .Select(translation => translation.Text)
                .FirstOrDefault();
    }
}

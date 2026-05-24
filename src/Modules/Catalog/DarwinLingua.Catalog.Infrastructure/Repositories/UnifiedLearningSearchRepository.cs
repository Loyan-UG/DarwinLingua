using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class UnifiedLearningSearchRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IUnifiedLearningSearchRepository
{
    private const int MaxResultsPerType = 20;

    public async Task<IReadOnlyList<UnifiedLearningSearchResultModel>> SearchAsync(UnifiedLearningSearchFilterModel filter, CancellationToken cancellationToken)
    {
        string? query = NormalizeSearch(filter.Query);
        if (query is null)
        {
            return [];
        }

        string? resultType = NormalizeKey(filter.ResultType);
        string? category = NormalizeKey(filter.Category);
        string? topicKey = NormalizeKey(filter.TopicKey);
        CefrLevel? cefrLevel = Enum.TryParse(filter.CefrLevel, true, out CefrLevel parsedCefr) ? parsedCefr : null;

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        List<UnifiedLearningSearchResultModel> results = [];

        if (ShouldSearch(resultType, "word")) results.AddRange(await SearchWordsAsync(dbContext, query, cefrLevel, category, topicKey, cancellationToken).ConfigureAwait(false));
        if (ShouldSearch(resultType, "grammar")) results.AddRange(await SearchGrammarAsync(dbContext, query, cefrLevel, category, topicKey, cancellationToken).ConfigureAwait(false));
        if (ShouldSearch(resultType, "expression")) results.AddRange(await SearchExpressionsAsync(dbContext, query, cefrLevel, category, topicKey, cancellationToken).ConfigureAwait(false));
        if (ShouldSearch(resultType, "dialogue")) results.AddRange(await SearchDialoguesAsync(dbContext, query, cefrLevel, category, topicKey, cancellationToken).ConfigureAwait(false));
        if (ShouldSearch(resultType, "talk-topic")) results.AddRange(await SearchTalkTopicsAsync(dbContext, query, cefrLevel, category, topicKey, cancellationToken).ConfigureAwait(false));
        if (ShouldSearch(resultType, "exercise")) results.AddRange(await SearchExercisesAsync(dbContext, query, cefrLevel, category, cancellationToken).ConfigureAwait(false));
        if (ShouldSearch(resultType, "course-lesson")) results.AddRange(await SearchCourseLessonsAsync(dbContext, query, cefrLevel, category, cancellationToken).ConfigureAwait(false));
        if (ShouldSearch(resultType, "exam-prep")) results.AddRange(await SearchExamPrepAsync(dbContext, query, cefrLevel, category, cancellationToken).ConfigureAwait(false));
        if (ShouldSearch(resultType, "writing-template")) results.AddRange(await SearchWritingTemplatesAsync(dbContext, query, cefrLevel, category, cancellationToken).ConfigureAwait(false));
        if (ShouldSearch(resultType, "cultural-note")) results.AddRange(await SearchCulturalNotesAsync(dbContext, query, cefrLevel, category, cancellationToken).ConfigureAwait(false));
        if (ShouldSearch(resultType, "event")) results.AddRange(await SearchEventsAsync(dbContext, query, cefrLevel, category, cancellationToken).ConfigureAwait(false));
        if (ShouldSearch(resultType, "organizer")) results.AddRange(await SearchOrganizersAsync(dbContext, query, cefrLevel, category, cancellationToken).ConfigureAwait(false));

        return results
            .OrderByDescending(result => result.RelevanceScore)
            .ThenBy(result => result.Title)
            .Take(100)
            .ToArray();
    }

    private static async Task<IReadOnlyList<UnifiedLearningSearchResultModel>> SearchWordsAsync(DarwinLinguaDbContext dbContext, string query, CefrLevel? cefrLevel, string? category, string? topicKey, CancellationToken cancellationToken)
    {
        IQueryable<WordEntry> words = dbContext.WordEntries.AsNoTracking().Where(word => word.PublicationStatus == PublicationStatus.Active);
        if (cefrLevel.HasValue) words = words.Where(word => word.PrimaryCefrLevel == cefrLevel.Value);
        if (topicKey is not null) words = words.Where(word => word.Topics.Any(link => dbContext.Topics.Any(topic => topic.Id == link.TopicId && topic.Key == topicKey)));
        words = words.Where(word => EF.Functions.ILike(word.Lemma, $"%{query}%") || EF.Functions.ILike(word.NormalizedLemma, $"%{query}%"));

        return await words
            .OrderBy(word => word.NormalizedLemma)
            .Take(MaxResultsPerType)
            .Select(word => new UnifiedLearningSearchResultModel("word", word.Lemma, word.PartOfSpeech.ToString(), word.PrimaryCefrLevel.ToString(), word.PartOfSpeech.ToString(), word.Topics.Join(dbContext.Topics, link => link.TopicId, topic => topic.Id, (link, topic) => topic.Key).ToArray(), $"/words/{LemmaUrlSlug.FromLemma(word.Lemma)}", Score(query, word.Lemma, word.NormalizedLemma, null, 0), MatchFields(query, word.Lemma, word.NormalizedLemma, null)))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<UnifiedLearningSearchResultModel>> SearchGrammarAsync(DarwinLinguaDbContext dbContext, string query, CefrLevel? cefrLevel, string? category, string? topicKey, CancellationToken cancellationToken)
    {
        IQueryable<GrammarTopic> items = dbContext.GrammarTopics.AsNoTracking().Where(item => item.PublicationStatus == PublicationStatus.Active);
        if (cefrLevel.HasValue) items = items.Where(item => item.CefrLevel == cefrLevel.Value);
        if (category is not null) items = items.Where(item => item.GrammarCategory == category);
        if (topicKey is not null) items = items.Where(item => item.Topics.Any(link => dbContext.Topics.Any(topic => topic.Id == link.TopicId && topic.Key == topicKey)));
        items = items.Where(item => EF.Functions.ILike(item.Title, $"%{query}%") || EF.Functions.ILike(item.ShortDescription, $"%{query}%") || EF.Functions.ILike(item.Slug, $"%{query}%"));
        return await items.OrderBy(item => item.SortOrder).Take(MaxResultsPerType).Select(item => new UnifiedLearningSearchResultModel("grammar", item.Title, item.ShortDescription, item.CefrLevel.ToString(), item.GrammarCategory, item.Topics.Join(dbContext.Topics, link => link.TopicId, topic => topic.Id, (link, topic) => topic.Key).ToArray(), $"/grammar/{item.Slug}", Score(query, item.Title, item.Slug, item.ShortDescription, item.SortOrder), MatchFields(query, item.Title, item.Slug, item.ShortDescription))).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<UnifiedLearningSearchResultModel>> SearchExpressionsAsync(DarwinLinguaDbContext dbContext, string query, CefrLevel? cefrLevel, string? category, string? topicKey, CancellationToken cancellationToken)
    {
        IQueryable<ExpressionEntry> items = dbContext.ExpressionEntries.AsNoTracking().Where(item => item.PublicationStatus == PublicationStatus.Active);
        if (cefrLevel.HasValue) items = items.Where(item => item.CefrLevel == cefrLevel.Value);
        if (category is not null) items = items.Where(item => item.Category == category || item.ExpressionType == category);
        if (topicKey is not null) items = items.Where(item => item.Topics.Any(link => dbContext.Topics.Any(topic => topic.Id == link.TopicId && topic.Key == topicKey)));
        string normalizedQuery = query.ToLowerInvariant();
        items = items.Where(item => item.ExpressionText.ToLower().Contains(normalizedQuery) || item.ActualMeaningText.ToLower().Contains(normalizedQuery) || item.Slug.ToLower().Contains(normalizedQuery));
        return await items.OrderBy(item => item.SortOrder).Take(MaxResultsPerType).Select(item => new UnifiedLearningSearchResultModel("expression", item.ExpressionText, item.ActualMeaningText, item.CefrLevel.ToString(), item.ExpressionType, item.Topics.Join(dbContext.Topics, link => link.TopicId, topic => topic.Id, (link, topic) => topic.Key).ToArray(), $"/expressions/{item.Slug}", Score(query, item.ExpressionText, item.Slug, item.ActualMeaningText, item.SortOrder), MatchFields(query, item.ExpressionText, item.Slug, item.ActualMeaningText))).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<UnifiedLearningSearchResultModel>> SearchDialoguesAsync(DarwinLinguaDbContext dbContext, string query, CefrLevel? cefrLevel, string? category, string? topicKey, CancellationToken cancellationToken)
    {
        IQueryable<DialogueLesson> items = dbContext.DialogueLessons.AsNoTracking().Where(item => item.PublicationStatus == PublicationStatus.Active);
        if (cefrLevel.HasValue) items = items.Where(item => item.CefrLevel == cefrLevel.Value);
        if (category is not null) items = items.Where(item => item.Category == category || item.TaskType == category);
        if (topicKey is not null) items = items.Where(item => item.Topics.Any(link => dbContext.Topics.Any(topic => topic.Id == link.TopicId && topic.Key == topicKey)));
        items = items.Where(item => EF.Functions.ILike(item.Title, $"%{query}%") || EF.Functions.ILike(item.Description, $"%{query}%") || EF.Functions.ILike(item.LearnerGoal, $"%{query}%") || EF.Functions.ILike(item.Slug, $"%{query}%"));
        return await items.OrderBy(item => item.SortOrder).Take(MaxResultsPerType).Select(item => new UnifiedLearningSearchResultModel("dialogue", item.Title, item.Description, item.CefrLevel.ToString(), item.Category, item.Topics.Join(dbContext.Topics, link => link.TopicId, topic => topic.Id, (link, topic) => topic.Key).ToArray(), $"/dialogues/{item.Slug}", Score(query, item.Title, item.Slug, item.Description, item.SortOrder), MatchFields(query, item.Title, item.Slug, item.Description))).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<UnifiedLearningSearchResultModel>> SearchTalkTopicsAsync(DarwinLinguaDbContext dbContext, string query, CefrLevel? cefrLevel, string? category, string? topicKey, CancellationToken cancellationToken)
    {
        IQueryable<TalkTopic> items = dbContext.TalkTopics.AsNoTracking().Where(item => item.PublicationStatus == PublicationStatus.Active);
        if (cefrLevel.HasValue) items = items.Where(item => item.CefrLevel == cefrLevel.Value);
        if (category is not null) items = items.Where(item => item.Category == category);
        if (topicKey is not null) items = items.Where(item => item.Topics.Any(link => dbContext.Topics.Any(topic => topic.Id == link.TopicId && topic.Key == topicKey)));
        items = items.Where(item => EF.Functions.ILike(item.Title, $"%{query}%") || EF.Functions.ILike(item.Description, $"%{query}%") || EF.Functions.ILike(item.Slug, $"%{query}%"));
        return await items.OrderBy(item => item.SortOrder).Take(MaxResultsPerType).Select(item => new UnifiedLearningSearchResultModel("talk-topic", item.Title, item.Description, item.CefrLevel.ToString(), item.Category, item.Topics.Join(dbContext.Topics, link => link.TopicId, topic => topic.Id, (link, topic) => topic.Key).ToArray(), $"/talk-topics/{item.Slug}", Score(query, item.Title, item.Slug, item.Description, item.SortOrder), MatchFields(query, item.Title, item.Slug, item.Description))).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<UnifiedLearningSearchResultModel>> SearchExercisesAsync(DarwinLinguaDbContext dbContext, string query, CefrLevel? cefrLevel, string? category, CancellationToken cancellationToken)
    {
        IQueryable<Exercise> items = dbContext.Exercises.AsNoTracking().Where(item => item.PublicationStatus == PublicationStatus.Active);
        if (cefrLevel.HasValue) items = items.Where(item => item.CefrLevel == cefrLevel.Value);
        if (category is not null) items = items.Where(item => item.ExerciseType == category || item.TargetSkill == category || item.OwnerType == category);
        items = items.Where(item => EF.Functions.ILike(item.Title, $"%{query}%") || EF.Functions.ILike(item.Instruction, $"%{query}%") || EF.Functions.ILike(item.Slug, $"%{query}%"));
        return await items.OrderBy(item => item.SortOrder).Take(MaxResultsPerType).Select(item => new UnifiedLearningSearchResultModel("exercise", item.Title, item.Instruction, item.CefrLevel.ToString(), item.ExerciseType, Array.Empty<string>(), $"/exercises/{item.Slug}", Score(query, item.Title, item.Slug, item.Instruction, item.SortOrder), MatchFields(query, item.Title, item.Slug, item.Instruction))).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<UnifiedLearningSearchResultModel>> SearchCourseLessonsAsync(DarwinLinguaDbContext dbContext, string query, CefrLevel? cefrLevel, string? category, CancellationToken cancellationToken)
    {
        IQueryable<CourseLesson> items = dbContext.CourseLessons.AsNoTracking().Where(item => item.PublicationStatus == PublicationStatus.Active);
        if (cefrLevel.HasValue) items = items.Where(item => item.CefrLevel == cefrLevel.Value);
        if (category is not null) items = items.Where(item => item.CoursePathSlug == category || item.ModuleSlug == category);
        items = items.Where(item => EF.Functions.ILike(item.Title, $"%{query}%") || EF.Functions.ILike(item.ShortDescription, $"%{query}%") || EF.Functions.ILike(item.Narrative, $"%{query}%") || EF.Functions.ILike(item.Slug, $"%{query}%"));
        return await items.OrderBy(item => item.SortOrder).Take(MaxResultsPerType).Select(item => new UnifiedLearningSearchResultModel("course-lesson", item.Title, item.ShortDescription, item.CefrLevel.ToString(), item.CoursePathSlug, Array.Empty<string>(), $"/courses/{item.CoursePathSlug}/{item.Slug}", Score(query, item.Title, item.Slug, item.ShortDescription, item.SortOrder), MatchFields(query, item.Title, item.Slug, item.ShortDescription))).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<UnifiedLearningSearchResultModel>> SearchExamPrepAsync(DarwinLinguaDbContext dbContext, string query, CefrLevel? cefrLevel, string? category, CancellationToken cancellationToken)
    {
        IQueryable<ExamPrepUnit> items = dbContext.ExamPrepUnits.AsNoTracking().Where(item => item.PublicationStatus == PublicationStatus.Active);
        if (cefrLevel.HasValue) items = items.Where(item => item.CefrLevel == cefrLevel.Value);
        if (category is not null) items = items.Where(item => item.ExamProfileKey == category || item.ExamSection == category || item.TaskType == category);
        items = items.Where(item => EF.Functions.ILike(item.Title, $"%{query}%") || EF.Functions.ILike(item.ShortDescription, $"%{query}%") || EF.Functions.ILike(item.Explanation, $"%{query}%") || EF.Functions.ILike(item.Slug, $"%{query}%"));
        return await items.OrderBy(item => item.SortOrder).Take(MaxResultsPerType).Select(item => new UnifiedLearningSearchResultModel("exam-prep", item.Title, item.ShortDescription, item.CefrLevel.ToString(), item.ExamSection, Array.Empty<string>(), $"/exam-prep/{item.Slug}", Score(query, item.Title, item.Slug, item.ShortDescription, item.SortOrder), MatchFields(query, item.Title, item.Slug, item.ShortDescription))).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<UnifiedLearningSearchResultModel>> SearchWritingTemplatesAsync(DarwinLinguaDbContext dbContext, string query, CefrLevel? cefrLevel, string? category, CancellationToken cancellationToken)
    {
        IQueryable<WritingTemplate> items = dbContext.WritingTemplates.AsNoTracking().Where(item => item.PublicationStatus == PublicationStatus.Active);
        if (cefrLevel.HasValue) items = items.Where(item => item.CefrLevel == cefrLevel.Value);
        if (category is not null) items = items.Where(item => item.Category == category || item.Register == category);
        items = items.Where(item => EF.Functions.ILike(item.Title, $"%{query}%") || EF.Functions.ILike(item.ShortDescription, $"%{query}%") || EF.Functions.ILike(item.Situation, $"%{query}%") || EF.Functions.ILike(item.Slug, $"%{query}%"));
        return await items.OrderBy(item => item.SortOrder).Take(MaxResultsPerType).Select(item => new UnifiedLearningSearchResultModel("writing-template", item.Title, item.ShortDescription, item.CefrLevel.ToString(), item.Category, Array.Empty<string>(), $"/writing-templates/{item.Slug}", Score(query, item.Title, item.Slug, item.ShortDescription, item.SortOrder), MatchFields(query, item.Title, item.Slug, item.ShortDescription))).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<UnifiedLearningSearchResultModel>> SearchCulturalNotesAsync(DarwinLinguaDbContext dbContext, string query, CefrLevel? cefrLevel, string? category, CancellationToken cancellationToken)
    {
        IQueryable<CulturalNote> items = dbContext.CulturalNotes.AsNoTracking().Where(item => item.PublicationStatus == PublicationStatus.Active);
        if (cefrLevel.HasValue) items = items.Where(item => item.CefrLevel == cefrLevel.Value);
        if (category is not null) items = items.Where(item => item.Category == category);
        items = items.Where(item => EF.Functions.ILike(item.Title, $"%{query}%") || EF.Functions.ILike(item.ShortDescription, $"%{query}%") || EF.Functions.ILike(item.Context, $"%{query}%") || EF.Functions.ILike(item.Slug, $"%{query}%"));
        return await items.OrderBy(item => item.SortOrder).Take(MaxResultsPerType).Select(item => new UnifiedLearningSearchResultModel("cultural-note", item.Title, item.ShortDescription, item.CefrLevel.ToString(), item.Category, Array.Empty<string>(), $"/cultural-notes/{item.Slug}", Score(query, item.Title, item.Slug, item.ShortDescription, item.SortOrder), MatchFields(query, item.Title, item.Slug, item.ShortDescription))).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<UnifiedLearningSearchResultModel>> SearchEventsAsync(DarwinLinguaDbContext dbContext, string query, CefrLevel? cefrLevel, string? category, CancellationToken cancellationToken)
    {
        IQueryable<ConversationEvent> items = dbContext.ConversationEvents.AsNoTracking().Where(item => item.PublicationStatus == PublicationStatus.Active);
        if (cefrLevel.HasValue) items = items.Where(item => item.SupportedLevels.Any(level => level.CefrLevel == cefrLevel.Value));
        if (category is not null) items = items.Where(item => item.Category == category);
        items = items.Where(item => EF.Functions.ILike(item.Name, $"%{query}%") || EF.Functions.ILike(item.Description, $"%{query}%") || EF.Functions.ILike(item.OrganizerName, $"%{query}%") || EF.Functions.ILike(item.Slug, $"%{query}%"));
        return await items.OrderBy(item => item.SortOrder).Take(MaxResultsPerType).Select(item => new UnifiedLearningSearchResultModel("event", item.Name, item.Description, null, item.Category, Array.Empty<string>(), $"/conversation-events/{item.Slug}", Score(query, item.Name, item.Slug, item.Description, item.SortOrder), MatchFields(query, item.Name, item.Slug, item.Description))).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<UnifiedLearningSearchResultModel>> SearchOrganizersAsync(DarwinLinguaDbContext dbContext, string query, CefrLevel? cefrLevel, string? category, CancellationToken cancellationToken)
    {
        IQueryable<OrganizerProfile> items = dbContext.OrganizerProfiles.AsNoTracking().Where(item => item.PublicationStatus == PublicationStatus.Active);
        if (cefrLevel.HasValue) items = items.Where(item => item.SupportedLevels.Any(level => level.CefrLevel == cefrLevel.Value));
        if (category is not null) items = items.Where(item => item.OrganizerType == category);
        items = items.Where(item => EF.Functions.ILike(item.DisplayName, $"%{query}%") || EF.Functions.ILike(item.Description, $"%{query}%") || EF.Functions.ILike(item.Slug, $"%{query}%"));
        return await items.OrderBy(item => item.DisplayName).Take(MaxResultsPerType).Select(item => new UnifiedLearningSearchResultModel("organizer", item.DisplayName, item.Description, null, item.OrganizerType, Array.Empty<string>(), $"/organizers/{item.Slug}", Score(query, item.DisplayName, item.Slug, item.Description, 0), MatchFields(query, item.DisplayName, item.Slug, item.Description))).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private static bool ShouldSearch(string? requestedResultType, string resultType) =>
        requestedResultType is null || string.Equals(requestedResultType, resultType, StringComparison.Ordinal);

    private static int Score(string query, string title, string slug, string? snippet, int sortOrder)
    {
        string normalizedTitle = title.Trim().ToLowerInvariant();
        string normalizedSlug = slug.Trim().ToLowerInvariant();
        string normalizedSnippet = snippet?.Trim().ToLowerInvariant() ?? string.Empty;
        int baseScore = normalizedTitle == query || normalizedSlug == query ? 600 :
            normalizedTitle.StartsWith(query, StringComparison.Ordinal) || normalizedSlug.StartsWith(query, StringComparison.Ordinal) ? 500 :
            normalizedTitle.Contains(query, StringComparison.Ordinal) || normalizedSlug.Contains(query, StringComparison.Ordinal) ? 400 :
            normalizedSnippet.Contains(query, StringComparison.Ordinal) ? 300 : 100;
        return baseScore - Math.Min(sortOrder, 99);
    }

    private static string[] MatchFields(string query, string title, string slug, string? snippet)
    {
        List<string> fields = [];
        if (title.Contains(query, StringComparison.OrdinalIgnoreCase)) fields.Add("title");
        if (slug.Contains(query, StringComparison.OrdinalIgnoreCase)) fields.Add("slug");
        if (!string.IsNullOrWhiteSpace(snippet) && snippet.Contains(query, StringComparison.OrdinalIgnoreCase)) fields.Add("snippet");
        return fields.ToArray();
    }

    private static string? NormalizeSearch(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static string? NormalizeKey(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
}

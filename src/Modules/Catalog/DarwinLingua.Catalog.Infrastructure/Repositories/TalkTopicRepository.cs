using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class TalkTopicRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : ITalkTopicRepository
{
    private static readonly LanguageCode EnglishFallbackLanguage = LanguageCode.From("en");

    public async Task<IReadOnlyList<TalkTopicListItemModel>> GetPublishedTalkTopicsAsync(
        TalkTopicListFilterModel filter,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        IQueryable<TalkTopic> query = dbContext.TalkTopics
            .AsNoTracking()
            .Include(topic => topic.Topics)
            .Include(topic => topic.SpeakingGoals)
            .Where(topic => topic.PublicationStatus == PublicationStatus.Active);

        if (Enum.TryParse(filter.CefrLevel, true, out CefrLevel cefrLevel))
        {
            query = query.Where(topic => topic.CefrLevel == cefrLevel);
        }

        string? category = NormalizeOptionalKey(filter.Category);
        if (category is not null)
        {
            query = query.Where(topic => topic.Category == category);
        }

        if (TryParseContentType(filter.ContentType, out TalkTopicContentType contentType))
        {
            query = query.Where(topic => topic.ContentType == contentType);
        }

        if (TryParseSpeakingGoal(filter.SpeakingGoal, out TalkTopicSpeakingGoal speakingGoal))
        {
            query = query.Where(topic => topic.SpeakingGoals.Any(goal => goal.SpeakingGoal == speakingGoal));
        }

        if (filter.IsSensitive.HasValue)
        {
            query = query.Where(topic => topic.IsSensitive == filter.IsSensitive.Value);
        }

        List<TalkTopic> topics = await query
            .OrderBy(topic => topic.SortOrder)
            .ThenBy(topic => topic.Title)
            .AsSplitQuery()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Guid[] topicIds = topics
            .SelectMany(item => item.Topics)
            .Select(link => link.TopicId)
            .Distinct()
            .ToArray();

        Dictionary<Guid, string> topicKeysById = await dbContext.Topics
            .AsNoTracking()
            .Where(topic => topicIds.Contains(topic.Id))
            .ToDictionaryAsync(topic => topic.Id, topic => topic.Key, cancellationToken)
            .ConfigureAwait(false);

        string? topicKey = NormalizeOptionalKey(filter.TopicKey);
        IEnumerable<TalkTopic> filteredTopics = topics;
        if (topicKey is not null)
        {
            filteredTopics = filteredTopics.Where(topic =>
                topic.Topics.Any(link =>
                    topicKeysById.TryGetValue(link.TopicId, out string? key) &&
                    string.Equals(key, topicKey, StringComparison.Ordinal)));
        }

        return filteredTopics
            .Select(topic => new TalkTopicListItemModel(
                topic.Slug,
                topic.TopicGroupKey,
                topic.Title,
                topic.Description,
                topic.CefrLevel.ToString(),
                topic.Category,
                FormatContentType(topic.ContentType),
                topic.EstimatedReadingMinutes,
                topic.EstimatedDiscussionMinutes,
                topic.IsSensitive,
                topic.Topics
                    .OrderByDescending(link => link.IsPrimary)
                    .ThenBy(link => link.CreatedAtUtc)
                    .Select(link => topicKeysById[link.TopicId])
                    .ToArray(),
                topic.SpeakingGoals
                    .OrderBy(goal => goal.SortOrder)
                    .Select(goal => FormatSpeakingGoal(goal.SpeakingGoal))
                    .ToArray()))
            .ToArray();
    }

    public async Task<TalkTopicDetailModel?> GetPublishedTalkTopicBySlugAsync(
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

        TalkTopic? topic = await dbContext.TalkTopics
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.Topics)
            .Include(item => item.Questions)
            .Include(item => item.VocabularyItems)
            .Include(item => item.SpeakingGoals)
            .Where(item => item.PublicationStatus == PublicationStatus.Active && item.Slug == normalizedSlug)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (topic is null)
        {
            return null;
        }

        Dictionary<Guid, string> topicKeysById = await dbContext.Topics
            .AsNoTracking()
            .Where(item => topic.Topics.Select(link => link.TopicId).Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.Key, cancellationToken)
            .ConfigureAwait(false);

        LanguageCode primaryLanguage = LanguageCode.From(primaryMeaningLanguageCode);
        LanguageCode? secondaryLanguage = string.IsNullOrWhiteSpace(secondaryMeaningLanguageCode)
            ? null
            : LanguageCode.From(secondaryMeaningLanguageCode);

        IReadOnlyList<TalkTopicVocabularyItemModel> vocabularyItems = await ResolveVocabularyAsync(
            dbContext,
            topic.VocabularyItems.OrderBy(item => item.SortOrder).ToArray(),
            primaryLanguage,
            secondaryLanguage,
            cancellationToken).ConfigureAwait(false);

        return new TalkTopicDetailModel(
            topic.Slug,
            topic.TopicGroupKey,
            topic.Title,
            topic.Description,
            topic.CefrLevel.ToString(),
            topic.Category,
            FormatContentType(topic.ContentType),
            topic.ArticleBaseText,
            null,
            null,
            topic.EstimatedReadingMinutes,
            topic.EstimatedDiscussionMinutes,
            topic.IsSensitive,
            topic.SensitivityNote,
            topic.RecommendedForModeratedGroupsOnly,
            topic.Topics
                .OrderByDescending(link => link.IsPrimary)
                .ThenBy(link => link.CreatedAtUtc)
                .Select(link => topicKeysById[link.TopicId])
                .ToArray(),
            topic.SpeakingGoals
                .OrderBy(goal => goal.SortOrder)
                .Select(goal => FormatSpeakingGoal(goal.SpeakingGoal))
                .ToArray(),
            topic.WarmupQuestions
                .OrderBy(question => question.SortOrder)
                .Select(question => new TalkTopicQuestionModel(
                    question.Prompt,
                    null,
                    null))
                .ToArray(),
            topic.DiscussionQuestions
                .OrderBy(question => question.SortOrder)
                .Select(question => new TalkTopicDiscussionQuestionModel(
                    question.Prompt,
                    question.QuestionType.HasValue ? FormatQuestionType(question.QuestionType.Value) : string.Empty,
                    null,
                    null))
                .ToArray(),
            vocabularyItems);
    }

    private static async Task<IReadOnlyList<TalkTopicVocabularyItemModel>> ResolveVocabularyAsync(
        DarwinLinguaDbContext dbContext,
        IReadOnlyList<TalkTopicVocabularyItem> vocabularyItems,
        LanguageCode primaryLanguage,
        LanguageCode? secondaryLanguage,
        CancellationToken cancellationToken)
    {
        if (vocabularyItems.Count == 0)
        {
            return [];
        }

        string[] normalizedLemmaCandidates = vocabularyItems
            .SelectMany(CreateVocabularyLookupCandidates)
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
            .Distinct(StringComparer.Ordinal)
            .ToArray()!;

        List<WordEntry> words = await dbContext.WordEntries
            .AsNoTracking()
            .AsSplitQuery()
            .Include(word => word.Senses).ThenInclude(sense => sense.Translations)
            .Where(word => word.PublicationStatus == PublicationStatus.Active)
            .Where(word => normalizedLemmaCandidates.Contains(word.NormalizedLemma))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return vocabularyItems
            .Select(item =>
            {
                string[] itemLookupCandidates = CreateVocabularyLookupCandidates(item).ToArray();
                WordEntry? word = words.FirstOrDefault(candidate =>
                    (item.WordSlug is not null && string.Equals(LemmaUrlSlug.FromLemma(candidate.Lemma), item.WordSlug, StringComparison.Ordinal)) ||
                    itemLookupCandidates.Contains(candidate.NormalizedLemma, StringComparer.Ordinal));

                WordSense? sense = word?.Senses
                    .OrderByDescending(candidate => candidate.IsPrimarySense)
                    .ThenBy(candidate => candidate.SenseOrder)
                    .FirstOrDefault();
                return new TalkTopicVocabularyItemModel(
                    item.Lemma,
                    word is null ? null : LemmaUrlSlug.FromLemma(word.Lemma),
                    item.CefrLevel?.ToString(),
                    sense is null ? null : ResolvePrimaryMeaning(sense.Translations, primaryLanguage),
                    sense is null ? null : ResolveSecondaryMeaning(sense.Translations, secondaryLanguage),
                    word is not null);
            })
            .ToArray();
    }

    private static IEnumerable<string> CreateVocabularyLookupCandidates(TalkTopicVocabularyItem item)
    {
        if (item.WordSlug is not null)
        {
            string slugCandidate = LemmaUrlSlug.ToNormalizedLemmaCandidate(item.WordSlug);
            yield return slugCandidate;
            string? strippedSlugCandidate = StripGermanArticle(slugCandidate);
            if (strippedSlugCandidate is not null)
            {
                yield return strippedSlugCandidate;
            }
        }

        string lemmaCandidate = NormalizeVocabularyLookupText(item.Lemma);
        yield return lemmaCandidate;
        string? strippedLemmaCandidate = StripGermanArticle(lemmaCandidate);
        if (strippedLemmaCandidate is not null)
        {
            yield return strippedLemmaCandidate;
        }
    }

    private static string NormalizeVocabularyLookupText(string value) =>
        string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();

    private static string? StripGermanArticle(string normalizedLemma)
    {
        foreach (string article in new[] { "der ", "die ", "das " })
        {
            if (normalizedLemma.StartsWith(article, StringComparison.Ordinal))
            {
                string stripped = normalizedLemma[article.Length..].Trim();
                return string.IsNullOrWhiteSpace(stripped) ? null : stripped;
            }
        }

        return null;
    }

    private static string? ResolvePrimaryMeaning<TTranslation>(
        IReadOnlyCollection<TTranslation> translations,
        LanguageCode primaryLanguage)
        where TTranslation : TalkTopicTranslationBase
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
        where TTranslation : TalkTopicTranslationBase
    {
        return secondaryLanguage is null
            ? null
            : translations
                .Where(translation => translation.LanguageCode == secondaryLanguage)
                .Select(translation => translation.Text)
                .FirstOrDefault();
    }

    private static string? ResolvePrimaryMeaning(
        IReadOnlyCollection<SenseTranslation> translations,
        LanguageCode primaryLanguage)
    {
        return translations
            .Where(translation => translation.LanguageCode == primaryLanguage || translation.LanguageCode == EnglishFallbackLanguage)
            .OrderBy(translation => translation.LanguageCode == primaryLanguage ? 0 : 1)
            .Select(translation => translation.TranslationText)
            .FirstOrDefault();
    }

    private static string? ResolveSecondaryMeaning(
        IReadOnlyCollection<SenseTranslation> translations,
        LanguageCode? secondaryLanguage)
    {
        return secondaryLanguage is null
            ? null
            : translations
                .Where(translation => translation.LanguageCode == secondaryLanguage)
                .Select(translation => translation.TranslationText)
                .FirstOrDefault();
    }

    private static string? NormalizeOptionalKey(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static bool TryParseContentType(string? value, out TalkTopicContentType contentType)
    {
        contentType = NormalizeOptionalKey(value) switch
        {
            "article" => TalkTopicContentType.Article,
            "book-summary" => TalkTopicContentType.BookSummary,
            "movie-summary" => TalkTopicContentType.MovieSummary,
            "story" => TalkTopicContentType.Story,
            "fact-sheet" => TalkTopicContentType.FactSheet,
            "opinion-text" => TalkTopicContentType.OpinionText,
            "interview" => TalkTopicContentType.Interview,
            "debate-text" => TalkTopicContentType.DebateText,
            _ => default,
        };
        return contentType != default;
    }

    private static bool TryParseSpeakingGoal(string? value, out TalkTopicSpeakingGoal speakingGoal)
    {
        speakingGoal = NormalizeOptionalKey(value) switch
        {
            "express-opinion" => TalkTopicSpeakingGoal.ExpressOpinion,
            "give-reasons" => TalkTopicSpeakingGoal.GiveReasons,
            "agree-disagree" => TalkTopicSpeakingGoal.AgreeDisagree,
            "ask-follow-up-questions" => TalkTopicSpeakingGoal.AskFollowUpQuestions,
            "compare-options" => TalkTopicSpeakingGoal.CompareOptions,
            "make-predictions" => TalkTopicSpeakingGoal.MakePredictions,
            "describe-experiences" => TalkTopicSpeakingGoal.DescribeExperiences,
            "imagine-possibilities" => TalkTopicSpeakingGoal.ImaginePossibilities,
            "debate-politely" => TalkTopicSpeakingGoal.DebatePolitely,
            "summarize-position" => TalkTopicSpeakingGoal.SummarizePosition,
            _ => default,
        };
        return speakingGoal != default;
    }

    private static string FormatContentType(TalkTopicContentType contentType) =>
        contentType switch
        {
            TalkTopicContentType.Article => "article",
            TalkTopicContentType.BookSummary => "book-summary",
            TalkTopicContentType.MovieSummary => "movie-summary",
            TalkTopicContentType.Story => "story",
            TalkTopicContentType.FactSheet => "fact-sheet",
            TalkTopicContentType.OpinionText => "opinion-text",
            TalkTopicContentType.Interview => "interview",
            TalkTopicContentType.DebateText => "debate-text",
            _ => contentType.ToString(),
        };

    private static string FormatQuestionType(TalkTopicQuestionType questionType) =>
        questionType switch
        {
            TalkTopicQuestionType.Opinion => "opinion",
            TalkTopicQuestionType.PersonalExperience => "personal-experience",
            TalkTopicQuestionType.Prediction => "prediction",
            TalkTopicQuestionType.Comparison => "comparison",
            TalkTopicQuestionType.Imagination => "imagination",
            TalkTopicQuestionType.Debate => "debate",
            TalkTopicQuestionType.Ethics => "ethics",
            TalkTopicQuestionType.Comprehension => "comprehension",
            _ => questionType.ToString(),
        };

    private static string FormatSpeakingGoal(TalkTopicSpeakingGoal speakingGoal) =>
        speakingGoal switch
        {
            TalkTopicSpeakingGoal.ExpressOpinion => "express-opinion",
            TalkTopicSpeakingGoal.GiveReasons => "give-reasons",
            TalkTopicSpeakingGoal.AgreeDisagree => "agree-disagree",
            TalkTopicSpeakingGoal.AskFollowUpQuestions => "ask-follow-up-questions",
            TalkTopicSpeakingGoal.CompareOptions => "compare-options",
            TalkTopicSpeakingGoal.MakePredictions => "make-predictions",
            TalkTopicSpeakingGoal.DescribeExperiences => "describe-experiences",
            TalkTopicSpeakingGoal.ImaginePossibilities => "imagine-possibilities",
            TalkTopicSpeakingGoal.DebatePolitely => "debate-politely",
            TalkTopicSpeakingGoal.SummarizePosition => "summarize-position",
            _ => speakingGoal.ToString(),
        };
}

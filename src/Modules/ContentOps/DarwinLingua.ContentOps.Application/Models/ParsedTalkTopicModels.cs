namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedTalkTopicModel(
    string Slug,
    string TopicGroupKey,
    string Title,
    string Description,
    string CefrLevel,
    string Category,
    IReadOnlyList<string> Topics,
    string ContentType,
    ParsedTalkTopicArticleModel Article,
    IReadOnlyList<ParsedTalkTopicQuestionModel> WarmupQuestions,
    IReadOnlyList<ParsedTalkTopicDiscussionQuestionModel> DiscussionQuestions,
    IReadOnlyList<ParsedTalkTopicVocabularyItemModel> VocabularyItems,
    IReadOnlyList<string> SpeakingGoals,
    int EstimatedReadingMinutes,
    int EstimatedDiscussionMinutes,
    bool IsSensitive,
    string? SensitivityNote,
    bool RecommendedForModeratedGroupsOnly,
    int SortOrder,
    bool IsPublished);

public sealed record ParsedTalkTopicArticleModel(
    string BaseText,
    IReadOnlyList<ParsedContentMeaningModel> Translations);

public sealed record ParsedTalkTopicQuestionModel(
    string Prompt,
    IReadOnlyList<ParsedContentMeaningModel> Translations,
    int SortOrder);

public sealed record ParsedTalkTopicDiscussionQuestionModel(
    string Prompt,
    string QuestionType,
    IReadOnlyList<ParsedContentMeaningModel> Translations,
    int SortOrder);

public sealed record ParsedTalkTopicVocabularyItemModel(
    string Lemma,
    string? WordSlug,
    string? CefrLevel,
    int SortOrder);

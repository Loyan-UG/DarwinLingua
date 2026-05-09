namespace DarwinLingua.Catalog.Application.Models;

public sealed record TalkTopicListFilterModel(
    string? CefrLevel,
    string? Category,
    string? TopicKey,
    string? ContentType,
    string? SpeakingGoal,
    bool? IsSensitive);

public sealed record TalkTopicListItemModel(
    string Slug,
    string TopicGroupKey,
    string Title,
    string Description,
    string CefrLevel,
    string Category,
    string ContentType,
    int EstimatedReadingMinutes,
    int EstimatedDiscussionMinutes,
    bool IsSensitive,
    IReadOnlyList<string> TopicKeys,
    IReadOnlyList<string> SpeakingGoals);

public sealed record TalkTopicDetailModel(
    string Slug,
    string TopicGroupKey,
    string Title,
    string Description,
    string CefrLevel,
    string Category,
    string ContentType,
    string ArticleBaseText,
    string? PrimaryArticleTranslation,
    string? SecondaryArticleTranslation,
    int EstimatedReadingMinutes,
    int EstimatedDiscussionMinutes,
    bool IsSensitive,
    string? SensitivityNote,
    bool RecommendedForModeratedGroupsOnly,
    IReadOnlyList<string> TopicKeys,
    IReadOnlyList<string> SpeakingGoals,
    IReadOnlyList<TalkTopicQuestionModel> WarmupQuestions,
    IReadOnlyList<TalkTopicDiscussionQuestionModel> DiscussionQuestions,
    IReadOnlyList<TalkTopicVocabularyItemModel> VocabularyItems);

public sealed record TalkTopicQuestionModel(
    string Prompt,
    string? PrimaryMeaning,
    string? SecondaryMeaning);

public sealed record TalkTopicDiscussionQuestionModel(
    string Prompt,
    string QuestionType,
    string? PrimaryMeaning,
    string? SecondaryMeaning);

public sealed record TalkTopicVocabularyItemModel(
    string Lemma,
    string? WordSlug,
    string? CefrLevel,
    string? PrimaryMeaning,
    string? SecondaryMeaning,
    bool IsResolved);

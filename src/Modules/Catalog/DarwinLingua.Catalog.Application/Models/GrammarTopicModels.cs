namespace DarwinLingua.Catalog.Application.Models;

public sealed record GrammarTopicListFilterModel(
    string? CefrLevel,
    string? GrammarCategory,
    string? TopicKey,
    string? Query);

public sealed record GrammarTopicListItemModel(
    string Slug,
    string Title,
    string ShortDescription,
    string CefrLevel,
    string GrammarCategory,
    IReadOnlyList<string> TopicKeys);

public sealed record GrammarTopicDetailModel(
    string Slug,
    string Title,
    string ShortDescription,
    string CefrLevel,
    string GrammarCategory,
    IReadOnlyList<string> TopicKeys,
    IReadOnlyList<GrammarSectionModel> Sections,
    IReadOnlyList<GrammarExampleModel> Examples,
    IReadOnlyList<GrammarTextItemModel> RuleSummaries,
    IReadOnlyList<GrammarCommonMistakeModel> CommonMistakes,
    IReadOnlyList<GrammarTextItemModel> ExceptionNotes,
    IReadOnlyList<GrammarLinkedWordModel> LinkedWords,
    IReadOnlyList<string> LinkedDialogueSlugs,
    IReadOnlyList<string> LinkedTalkTopicSlugs,
    IReadOnlyList<string> LinkedExerciseSlugs,
    IReadOnlyList<string> PrerequisiteSlugs,
    IReadOnlyList<string> RelatedTopicSlugs);

public sealed record GrammarSectionModel(
    string Heading,
    string Explanation,
    string? RequestedLanguageCode,
    bool UsedFallback);

public sealed record GrammarExampleModel(
    string GermanText,
    string? Note,
    string? Translation,
    string? RequestedLanguageCode,
    bool UsedFallback);

public sealed record GrammarTextItemModel(
    string Text,
    string? RequestedLanguageCode,
    bool UsedFallback);

public sealed record GrammarCommonMistakeModel(
    string WrongText,
    string CorrectedText,
    string Explanation,
    string? RequestedLanguageCode,
    bool UsedFallback);

public sealed record GrammarLinkedWordModel(
    string Lemma,
    string? WordSlug);

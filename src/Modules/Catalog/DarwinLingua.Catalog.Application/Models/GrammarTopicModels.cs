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
    string? LearnerLanguageTitle,
    string? LearnerLanguageShortDescription,
    int? ContentRevision,
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
    IReadOnlyList<string> RelatedTopicSlugs,
    IReadOnlyList<GrammarImageSlotModel> ImageSlots);

public sealed record GrammarSectionModel(
    string? SectionKey,
    string Heading,
    string Explanation,
    IReadOnlyList<GrammarContentBlockModel> Blocks,
    string? LearnerLanguageHeading,
    string? LearnerLanguageExplanation,
    IReadOnlyList<GrammarContentBlockModel> LearnerLanguageBlocks,
    string? RequestedLanguageCode,
    bool UsedFallback);

public sealed record GrammarExampleModel(
    string SourceText,
    string GermanText,
    string? Note,
    string? Translation,
    string? RequestedLanguageCode,
    bool UsedFallback);

public sealed record GrammarTextItemModel(
    string Text,
    string? LearnerLanguageText,
    string? RequestedLanguageCode,
    bool UsedFallback);

public sealed record GrammarCommonMistakeModel(
    string WrongText,
    string CorrectedText,
    string Explanation,
    string? LearnerLanguageExplanation,
    string? RequestedLanguageCode,
    bool UsedFallback);

public sealed record GrammarLinkedWordModel(
    string Lemma,
    string? WordSlug);

public sealed record GrammarImageSlotModel(
    string ImageSlotKey,
    string? AssetKey,
    string? ImageFileName,
    string? AltText);

public sealed record GrammarContentBlockModel(
    string Type,
    string? Text,
    string? Style,
    string? Caption,
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<string>> Rows,
    IReadOnlyList<string> Items,
    string? Wrong,
    string? Correct,
    string? AssetKey,
    string? ImageSlotKey);

namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedGrammarTopicModel(
    string Slug,
    int? ContentRevision,
    string Title,
    string ShortDescription,
    IReadOnlyDictionary<string, string> TitleLocalized,
    IReadOnlyDictionary<string, string> ShortDescriptionLocalized,
    string CefrLevel,
    string GrammarCategory,
    IReadOnlyList<string> Topics,
    bool IsPublished,
    int SortOrder,
    IReadOnlyList<ParsedGrammarSectionModel> Sections,
    IReadOnlyList<ParsedGrammarExampleModel> Examples,
    IReadOnlyList<ParsedGrammarTextItemModel> RuleSummaries,
    IReadOnlyList<ParsedGrammarCommonMistakeModel> CommonMistakes,
    IReadOnlyList<ParsedGrammarTextItemModel> ExceptionNotes,
    IReadOnlyList<string> PrerequisiteSlugs,
    IReadOnlyList<string> RelatedTopicSlugs,
    IReadOnlyList<ParsedGrammarLinkedWordModel> LinkedWords,
    IReadOnlyList<string> LinkedDialogueSlugs,
    IReadOnlyList<string> LinkedTalkTopicSlugs,
    IReadOnlyList<string> LinkedExerciseSlugs,
    string? ImageSlotsJson);

public sealed record ParsedGrammarSectionModel(
    string SectionKey,
    string Heading,
    string Explanation,
    IReadOnlyList<ParsedGrammarSectionTranslationModel> Translations,
    IReadOnlyDictionary<string, string> LocalizedBlocksJson,
    int SortOrder);

public sealed record ParsedGrammarSectionTranslationModel(
    string Language,
    string Heading,
    string Text);

public sealed record ParsedGrammarExampleModel(
    string GermanText,
    string? Note,
    IReadOnlyList<ParsedContentMeaningModel> Translations,
    int SortOrder);

public sealed record ParsedGrammarTextItemModel(
    string Text,
    IReadOnlyList<ParsedContentMeaningModel> Translations,
    int SortOrder);

public sealed record ParsedGrammarCommonMistakeModel(
    string WrongText,
    string CorrectedText,
    string Explanation,
    IReadOnlyList<ParsedContentMeaningModel> Translations,
    int SortOrder);

public sealed record ParsedGrammarLinkedWordModel(
    string Lemma,
    string? WordSlug,
    int SortOrder);

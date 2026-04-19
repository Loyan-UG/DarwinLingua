namespace DarwinLingua.Catalog.Application.Models;

/// <summary>
/// Represents the detail view model for a lexical entry.
/// </summary>
public sealed record WordDetailModel(
    Guid PublicId,
    string Lemma,
    string? Article,
    string? PluralForm,
    string? InfinitiveForm,
    string? PronunciationIpa,
    string? SyllableBreak,
    string PartOfSpeech,
    IReadOnlyList<WordLexicalFormDetailModel> LexicalForms,
    string CefrLevel,
    IReadOnlyList<string> UsageLabels,
    IReadOnlyList<string> ContextLabels,
    IReadOnlyList<string> GrammarNotes,
    IReadOnlyList<WordCollocationDetailModel> Collocations,
    IReadOnlyList<WordFamilyMemberDetailModel> WordFamilies,
    IReadOnlyList<WordRelationDetailModel> Synonyms,
    IReadOnlyList<WordRelationDetailModel> Antonyms,
    IReadOnlyList<string> Topics,
    IReadOnlyList<WordSenseDetailModel> Senses);

/// <summary>
/// Represents one collocation block on the word detail screen.
/// </summary>
public sealed record WordCollocationDetailModel(
    string Text,
    string? Meaning);

/// <summary>
/// Represents one word-family member block on the word detail screen.
/// </summary>
public sealed record WordFamilyMemberDetailModel(
    string Lemma,
    string RelationLabel,
    string? Note);

/// <summary>
/// Represents one lexical relation on the word detail screen.
/// </summary>
public sealed record WordRelationDetailModel(
    string Lemma,
    string? Note);

/// <summary>
/// Represents a sense block on the word detail screen.
/// </summary>
public sealed record WordSenseDetailModel(
    string? ShortDefinitionDe,
    string? PrimaryMeaning,
    string? SecondaryMeaning,
    IReadOnlyList<ExampleSentenceDetailModel> Examples);

/// <summary>
/// Represents an example sentence block on the word detail screen.
/// </summary>
public sealed record ExampleSentenceDetailModel(
    string GermanText,
    string? PrimaryMeaning,
    string? SecondaryMeaning);

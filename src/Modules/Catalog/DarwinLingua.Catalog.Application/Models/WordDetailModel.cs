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
    string PartOfSpeech,
    string CefrLevel,
    IReadOnlyList<string> UsageLabels,
    IReadOnlyList<string> ContextLabels,
    IReadOnlyList<string> Topics,
    IReadOnlyList<WordSenseDetailModel> Senses);

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

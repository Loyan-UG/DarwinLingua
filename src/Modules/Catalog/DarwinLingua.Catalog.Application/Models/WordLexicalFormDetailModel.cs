namespace DarwinLingua.Catalog.Application.Models;

/// <summary>
/// Represents one lexical-form block on the word detail screen.
/// </summary>
public sealed record WordLexicalFormDetailModel(
    string PartOfSpeech,
    string? Article,
    string? PluralForm,
    string? InfinitiveForm,
    bool IsPrimary);

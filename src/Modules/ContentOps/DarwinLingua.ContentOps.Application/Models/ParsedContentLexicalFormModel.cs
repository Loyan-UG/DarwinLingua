namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents one lexical-form block parsed from a content package entry.
/// </summary>
public sealed record ParsedContentLexicalFormModel(
    string PartOfSpeech,
    string? Article,
    string? Plural,
    string? Infinitive,
    bool IsPrimary);

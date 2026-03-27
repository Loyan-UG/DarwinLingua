namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents one parsed collocation block from an import package entry.
/// </summary>
public sealed record ParsedContentCollocationModel(
    string Text,
    string? Meaning);

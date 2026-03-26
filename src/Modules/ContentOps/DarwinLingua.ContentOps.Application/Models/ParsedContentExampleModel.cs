namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents one parsed example sentence from an import package.
/// </summary>
public sealed record ParsedContentExampleModel(
    string BaseText,
    IReadOnlyList<ParsedContentMeaningModel> Translations);

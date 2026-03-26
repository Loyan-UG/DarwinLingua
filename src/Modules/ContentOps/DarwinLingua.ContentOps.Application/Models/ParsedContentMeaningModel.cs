namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents one parsed meaning or example translation from an import package.
/// </summary>
public sealed record ParsedContentMeaningModel(string Language, string Text);

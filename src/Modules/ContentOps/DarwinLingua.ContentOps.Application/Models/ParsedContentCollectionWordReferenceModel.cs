namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents one word reference inside an imported curated collection.
/// </summary>
public sealed record ParsedContentCollectionWordReferenceModel(
    string Word,
    string? PartOfSpeech,
    string? CefrLevel);

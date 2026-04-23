namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents one curated collection declared inside a content import package.
/// </summary>
public sealed record ParsedContentCollectionModel(
    string Slug,
    string Name,
    string? Description,
    string? ImageUrl,
    int SortOrder,
    IReadOnlyList<ParsedContentCollectionWordReferenceModel> Words);

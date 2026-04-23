namespace DarwinLingua.Catalog.Application.Models;

/// <summary>
/// Represents a browse-friendly summary of a curated word collection.
/// </summary>
public sealed record WordCollectionListItemModel(
    string Slug,
    string Name,
    string? Description,
    string? ImageUrl,
    int WordCount,
    IReadOnlyList<string> CefrLevels,
    IReadOnlyList<string> PreviewWords);

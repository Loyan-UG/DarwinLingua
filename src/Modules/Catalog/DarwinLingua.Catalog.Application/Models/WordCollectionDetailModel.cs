namespace DarwinLingua.Catalog.Application.Models;

/// <summary>
/// Represents a full curated word collection with its resolved words.
/// </summary>
public sealed record WordCollectionDetailModel(
    string Slug,
    string Name,
    string? Description,
    string? ImageUrl,
    int WordCount,
    IReadOnlyList<string> CefrLevels,
    IReadOnlyList<WordListItemModel> Words);

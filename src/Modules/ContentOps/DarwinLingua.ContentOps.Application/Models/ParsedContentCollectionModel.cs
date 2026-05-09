using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents one curated collection declared inside a content import package.
/// </summary>
public sealed record ParsedContentCollectionModel(
    string Slug,
    string Name,
    string? Description,
    IReadOnlyList<ParsedLocalizedTextModel> Localizations,
    string? ImageUrl,
    int SortOrder,
    IReadOnlyList<ParsedContentCollectionWordReferenceModel> Words)
{
    public ParsedContentCollectionModel(
        string slug,
        string name,
        string? description,
        string? imageUrl,
        int sortOrder,
        IReadOnlyList<ParsedContentCollectionWordReferenceModel> words)
        : this(slug, name, description, CreateCompatibilityLocalizations(name, description), imageUrl, sortOrder, words)
    {
    }

    private static IReadOnlyList<ParsedLocalizedTextModel> CreateCompatibilityLocalizations(string name, string? description)
    {
        return ContentLanguageRequirements.RequiredLocalizationLanguageCodes
            .Select(languageCode => new ParsedLocalizedTextModel(languageCode, name, description))
            .ToArray();
    }
}

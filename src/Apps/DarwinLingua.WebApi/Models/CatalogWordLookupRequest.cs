namespace DarwinLingua.WebApi.Models;

public sealed record CatalogWordLookupRequest(
    IReadOnlyList<Guid> WordIds,
    string MeaningLanguageCode);

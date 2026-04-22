namespace DarwinLingua.WebApi.Models;

public sealed record AdminCatalogDraftWordsResponse(
    string? Query,
    IReadOnlyList<AdminCatalogDraftWordItemResponse> Words);

public sealed record AdminCatalogDraftWordItemResponse(
    Guid PublicId,
    string Lemma,
    string PartOfSpeech,
    string CefrLevel,
    string PublicationStatus);

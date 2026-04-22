namespace DarwinLingua.WebApi.Models;

public sealed record AdminCatalogHistoryViewResponse(
    string? StatusFilter,
    IReadOnlyList<AdminCatalogHistoryItemResponse> Items);

public sealed record AdminCatalogHistoryItemResponse(
    string PackageId,
    string PackageVersion,
    string Status,
    int TotalEntries,
    int InsertedEntries,
    int InvalidEntries,
    DateTime CreatedAtUtc);

namespace DarwinLingua.WebApi.Models;

public sealed record AdminCatalogImportsResponse(
    string? StatusFilter,
    IReadOnlyList<AdminCatalogImportItemResponse> Packages);

public sealed record AdminCatalogImportItemResponse(
    string PackageId,
    string PackageVersion,
    string PackageName,
    string SourceType,
    string Status,
    int TotalEntries,
    int InsertedEntries,
    int InvalidEntries,
    int WarningCount,
    DateTime CreatedAtUtc);

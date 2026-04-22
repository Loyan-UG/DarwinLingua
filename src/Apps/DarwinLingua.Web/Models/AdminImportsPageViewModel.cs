namespace DarwinLingua.Web.Models;

public sealed record AdminImportsPageViewModel(
    string? StatusFilter,
    IReadOnlyList<AdminContentPackageListItemViewModel> Packages);

public sealed record AdminContentPackageListItemViewModel(
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

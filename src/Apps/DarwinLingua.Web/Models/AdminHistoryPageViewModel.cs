namespace DarwinLingua.Web.Models;

public sealed record AdminHistoryPageViewModel(
    string? StatusFilter,
    IReadOnlyList<AdminHistoryItemViewModel> Items);

public sealed record AdminHistoryItemViewModel(
    string PackageId,
    string PackageVersion,
    string Status,
    int TotalEntries,
    int InsertedEntries,
    int InvalidEntries,
    DateTime CreatedAtUtc);

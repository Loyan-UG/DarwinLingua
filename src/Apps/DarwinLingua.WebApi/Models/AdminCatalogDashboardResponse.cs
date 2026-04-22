namespace DarwinLingua.WebApi.Models;

public sealed record AdminCatalogDashboardResponse(
    int ActiveWordCount,
    int DraftWordCount,
    int TotalTopicCount,
    int ImportedPackageCount,
    int FailedPackageCount,
    DateTime? LastImportAtUtc);

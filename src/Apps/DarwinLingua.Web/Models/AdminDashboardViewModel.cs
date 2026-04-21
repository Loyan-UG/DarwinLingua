namespace DarwinLingua.Web.Models;

public sealed record AdminDashboardViewModel(
    int ActiveWordCount,
    int DraftWordCount,
    int TotalTopicCount,
    int ImportedPackageCount,
    int FailedPackageCount,
    DateTime? LastImportAtUtc);

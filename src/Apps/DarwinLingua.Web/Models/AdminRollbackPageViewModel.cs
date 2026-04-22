namespace DarwinLingua.Web.Models;

public sealed record AdminRollbackPageViewModel(
    int DraftWordCount,
    int ImportedPackageCount,
    string WarningMessage);

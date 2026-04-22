namespace DarwinLingua.WebApi.Models;

public sealed record AdminCatalogRollbackPreviewResponse(
    int DraftWordCount,
    int ImportedPackageCount,
    string WarningMessage);

namespace DarwinLingua.ContentOps.Domain.Enums;

/// <summary>
/// Represents the processing outcome of a single attempted content-package entry.
/// </summary>
public enum ContentPackageEntryStatus
{
    Pending = 1,
    Imported = 2,
    SkippedDuplicate = 3,
    Invalid = 4,
    Failed = 5,
}

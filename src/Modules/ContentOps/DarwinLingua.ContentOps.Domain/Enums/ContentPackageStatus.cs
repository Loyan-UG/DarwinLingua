namespace DarwinLingua.ContentOps.Domain.Enums;

/// <summary>
/// Represents the lifecycle state of a content package import operation.
/// </summary>
public enum ContentPackageStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    CompletedWithWarnings = 4,
    Failed = 5,
}

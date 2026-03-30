namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Thrown when a requested content package is incompatible with the caller's schema version.
/// </summary>
public sealed class MobileContentSchemaCompatibilityException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MobileContentSchemaCompatibilityException"/> class.
    /// </summary>
    public MobileContentSchemaCompatibilityException(
        string packageId,
        int clientSchemaVersion,
        int minimumRequiredSchemaVersion,
        int packageSchemaVersion)
        : base($"Package '{packageId}' requires client schema {minimumRequiredSchemaVersion} or higher, but client schema {clientSchemaVersion} was supplied.")
    {
        PackageId = packageId;
        ClientSchemaVersion = clientSchemaVersion;
        MinimumRequiredSchemaVersion = minimumRequiredSchemaVersion;
        PackageSchemaVersion = packageSchemaVersion;
    }

    /// <summary>
    /// Gets the incompatible package identifier.
    /// </summary>
    public string PackageId { get; }

    /// <summary>
    /// Gets the client schema version supplied by the caller.
    /// </summary>
    public int ClientSchemaVersion { get; }

    /// <summary>
    /// Gets the minimum schema version required by the package.
    /// </summary>
    public int MinimumRequiredSchemaVersion { get; }

    /// <summary>
    /// Gets the schema version used to serialize the package.
    /// </summary>
    public int PackageSchemaVersion { get; }
}

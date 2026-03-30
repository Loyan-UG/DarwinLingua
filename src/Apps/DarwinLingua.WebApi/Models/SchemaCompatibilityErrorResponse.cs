namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Represents an incompatibility between a mobile client schema version and a published package.
/// </summary>
public sealed record SchemaCompatibilityErrorResponse(
    string Code,
    string Message,
    string PackageId,
    int ClientSchemaVersion,
    int MinimumRequiredSchemaVersion,
    int PackageSchemaVersion);

namespace DarwinLingua.WebApi.Persistence.Entities;

/// <summary>
/// Represents the server-side lifecycle state of one mobile content package.
/// </summary>
public enum PackagePublicationStatus
{
    Draft = 0,
    Published = 1,
    Superseded = 2,
}

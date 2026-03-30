namespace DarwinLingua.WebApi.Persistence.Entities;

/// <summary>
/// Represents one audited server-side publication event type.
/// </summary>
public enum ContentPublicationEventType
{
    Publish = 0,
    Rollback = 1,
    Cleanup = 2,
}

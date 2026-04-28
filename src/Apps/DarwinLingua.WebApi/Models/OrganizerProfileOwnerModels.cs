namespace DarwinLingua.WebApi.Models;

public sealed record AssignOrganizerProfileOwnerRequest(
    string OrganizerProfileSlug,
    string OwnerEmail,
    string AssignedBy);

public sealed record OrganizerProfileOwnerResponse(
    Guid Id,
    string OrganizerProfileSlug,
    string OwnerEmail,
    string AssignedBy,
    DateTime CreatedAtUtc);

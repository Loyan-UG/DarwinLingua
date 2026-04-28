namespace DarwinLingua.WebApi.Models;

public sealed record SubmitOrganizerClaimRequest(
    string RequesterName,
    string RequesterEmail,
    string RelationshipToOrganizer,
    string EvidenceText);

public sealed record OrganizerClaimRequestResponse(
    Guid Id,
    string OrganizerProfileSlug,
    string RequesterName,
    string RequesterEmail,
    string RelationshipToOrganizer,
    string EvidenceText,
    string Status,
    DateTime CreatedAtUtc);

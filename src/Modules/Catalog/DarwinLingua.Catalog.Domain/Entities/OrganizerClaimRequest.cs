using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed class OrganizerClaimRequest
{
    private OrganizerClaimRequest()
    {
    }

    public OrganizerClaimRequest(
        Guid id,
        string organizerProfileSlug,
        string requesterName,
        string requesterEmail,
        string relationshipToOrganizer,
        string evidenceText,
        string status,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Organizer claim request identifier cannot be empty.");
        }

        Id = id;
        OrganizerProfileSlug = ConversationEvent.NormalizeKey(organizerProfileSlug, "Organizer claim profile slug");
        RequesterName = ConversationEvent.NormalizeRequiredText(requesterName, nameof(requesterName), 256);
        RequesterEmail = ConversationEvent.NormalizeRequiredText(requesterEmail, nameof(requesterEmail), 320);
        RelationshipToOrganizer = ConversationEvent.NormalizeRequiredText(relationshipToOrganizer, nameof(relationshipToOrganizer), 256);
        EvidenceText = ConversationEvent.NormalizeRequiredText(evidenceText, nameof(evidenceText), 4000);
        Status = NormalizeStatus(status);
        CreatedAtUtc = ConversationEvent.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string OrganizerProfileSlug { get; private set; } = string.Empty;

    public string RequesterName { get; private set; } = string.Empty;

    public string RequesterEmail { get; private set; } = string.Empty;

    public string RelationshipToOrganizer { get; private set; } = string.Empty;

    public string EvidenceText { get; private set; } = string.Empty;

    public string Status { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static string NormalizeStatus(string status)
    {
        string normalized = ConversationEvent.NormalizeKey(status, "Organizer claim status");
        if (!OrganizerClaimRequestStatuses.All.Contains(normalized))
        {
            throw new DomainRuleException("Organizer claim status is not supported.");
        }

        return normalized;
    }
}

public static class OrganizerClaimRequestStatuses
{
    public const string Submitted = "submitted";

    public const string Reviewing = "reviewing";

    public const string Approved = "approved";

    public const string Rejected = "rejected";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Submitted,
        Reviewing,
        Approved,
        Rejected,
    };
}

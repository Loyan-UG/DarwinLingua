using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed class OrganizerVerification
{
    private OrganizerVerification()
    {
    }

    public OrganizerVerification(Guid id, string organizerProfileSlug, string status, string requestedByEmail, DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Organizer verification identifier cannot be empty.");
        }

        Id = id;
        OrganizerProfileSlug = ConversationEvent.NormalizeKey(organizerProfileSlug, "Organizer profile slug");
        Status = ConversationEvent.NormalizeTaxonomyKey(status, OrganizerVerificationStatuses.All, "Organizer verification status");
        RequestedByEmail = LearnerConversationProfile.NormalizeEmail(requestedByEmail);
        CreatedAtUtc = ConversationEvent.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string OrganizerProfileSlug { get; private set; } = string.Empty;

    public string Status { get; private set; } = string.Empty;

    public string RequestedByEmail { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }
}

public static class OrganizerVerificationStatuses
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        "pending",
        "verified",
        "rejected",
    };
}

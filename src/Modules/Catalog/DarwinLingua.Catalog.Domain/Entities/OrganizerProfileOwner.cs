using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed class OrganizerProfileOwner
{
    private OrganizerProfileOwner()
    {
    }

    public OrganizerProfileOwner(
        Guid id,
        string organizerProfileSlug,
        string ownerEmail,
        string assignedBy,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Organizer profile owner identifier cannot be empty.");
        }

        Id = id;
        OrganizerProfileSlug = ConversationEvent.NormalizeKey(organizerProfileSlug, "Organizer profile owner slug");
        OwnerEmail = NormalizeEmail(ownerEmail);
        AssignedBy = ConversationEvent.NormalizeRequiredText(assignedBy, nameof(assignedBy), 320);
        CreatedAtUtc = ConversationEvent.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public string OrganizerProfileSlug { get; private set; } = string.Empty;

    public string OwnerEmail { get; private set; } = string.Empty;

    public string AssignedBy { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public static string NormalizeEmail(string value)
    {
        string normalized = ConversationEvent.NormalizeRequiredText(value, "Owner email", 320).ToLowerInvariant();
        if (!normalized.Contains('@', StringComparison.Ordinal))
        {
            throw new DomainRuleException("Owner email must be a valid email-like value.");
        }

        return normalized;
    }
}

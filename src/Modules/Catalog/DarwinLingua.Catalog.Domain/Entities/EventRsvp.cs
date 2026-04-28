using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed class EventRsvp
{
    private EventRsvp()
    {
    }

    public EventRsvp(
        Guid id,
        string conversationEventSlug,
        string participantName,
        string participantEmail,
        string status,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Event RSVP identifier cannot be empty.");
        }

        Id = id;
        ConversationEventSlug = ConversationEvent.NormalizeKey(conversationEventSlug, "Event RSVP event slug");
        ParticipantName = ConversationEvent.NormalizeRequiredText(participantName, nameof(participantName), 256);
        ParticipantEmail = NormalizeEmail(participantEmail);
        Status = NormalizeStatus(status);
        CreatedAtUtc = ConversationEvent.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string ConversationEventSlug { get; private set; } = string.Empty;

    public string ParticipantName { get; private set; } = string.Empty;

    public string ParticipantEmail { get; private set; } = string.Empty;

    public string Status { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public void Update(string participantName, string status, DateTime updatedAtUtc)
    {
        ParticipantName = ConversationEvent.NormalizeRequiredText(participantName, nameof(participantName), 256);
        Status = NormalizeStatus(status);
        UpdatedAtUtc = ConversationEvent.NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    public static string NormalizeEmail(string value)
    {
        string normalized = ConversationEvent.NormalizeRequiredText(value, "Participant email", 320).ToLowerInvariant();
        if (!normalized.Contains('@', StringComparison.Ordinal))
        {
            throw new DomainRuleException("Participant email must be a valid email-like value.");
        }

        return normalized;
    }

    public static string NormalizeStatus(string status)
    {
        string normalized = ConversationEvent.NormalizeKey(status, "Event RSVP status");
        if (!EventRsvpStatuses.All.Contains(normalized))
        {
            throw new DomainRuleException("Event RSVP status is not supported.");
        }

        return normalized;
    }
}

public static class EventRsvpStatuses
{
    public const string Interested = "interested";

    public const string Going = "going";

    public const string Cancelled = "cancelled";

    public const string Attended = "attended";

    public const string NoShow = "no-show";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Interested,
        Going,
        Cancelled,
        Attended,
        NoShow,
    };
}

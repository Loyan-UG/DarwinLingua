using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed class PartnerRequest
{
    private PartnerRequest()
    {
    }

    public PartnerRequest(
        Guid id,
        string requesterEmail,
        Guid targetLearnerProfileId,
        string openerTemplateKey,
        string? note,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Partner request identifier cannot be empty.");
        }

        if (targetLearnerProfileId == Guid.Empty)
        {
            throw new DomainRuleException("Partner request target profile identifier cannot be empty.");
        }

        Id = id;
        RequesterEmail = LearnerConversationProfile.NormalizeEmail(requesterEmail);
        TargetLearnerProfileId = targetLearnerProfileId;
        OpenerTemplateKey = ConversationEvent.NormalizeTaxonomyKey(openerTemplateKey, PartnerRequestTaxonomy.OpenerTemplates, "Partner request opener template");
        Note = ConversationEvent.NormalizeOptionalText(note, 500);
        Status = PartnerRequestStatuses.Pending;
        CreatedAtUtc = ConversationEvent.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
        ExpiresAtUtc = ConversationEvent.NormalizeUtc(expiresAtUtc, nameof(expiresAtUtc));

        if (ExpiresAtUtc <= CreatedAtUtc)
        {
            throw new DomainRuleException("Partner request expiry must be after creation.");
        }
    }

    public Guid Id { get; private set; }

    public string RequesterEmail { get; private set; } = string.Empty;

    public Guid TargetLearnerProfileId { get; private set; }

    public string OpenerTemplateKey { get; private set; } = string.Empty;

    public string? Note { get; private set; }

    public string Status { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? RespondedAtUtc { get; private set; }

    public void Accept(DateTime updatedAtUtc)
    {
        EnsurePending();
        Status = PartnerRequestStatuses.Accepted;
        UpdatedAtUtc = ConversationEvent.NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
        RespondedAtUtc = UpdatedAtUtc;
    }

    public void Decline(DateTime updatedAtUtc)
    {
        EnsurePending();
        Status = PartnerRequestStatuses.Declined;
        UpdatedAtUtc = ConversationEvent.NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
        RespondedAtUtc = UpdatedAtUtc;
    }

    public void Cancel(DateTime updatedAtUtc)
    {
        EnsurePending();
        Status = PartnerRequestStatuses.Cancelled;
        UpdatedAtUtc = ConversationEvent.NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    public void Block(DateTime updatedAtUtc)
    {
        if (Status is PartnerRequestStatuses.Cancelled or PartnerRequestStatuses.Expired)
        {
            throw new DomainRuleException("Cancelled or expired partner requests cannot be blocked.");
        }

        Status = PartnerRequestStatuses.Blocked;
        UpdatedAtUtc = ConversationEvent.NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
        RespondedAtUtc = UpdatedAtUtc;
    }

    public void Expire(DateTime updatedAtUtc)
    {
        EnsurePending();
        Status = PartnerRequestStatuses.Expired;
        UpdatedAtUtc = ConversationEvent.NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    private void EnsurePending()
    {
        if (Status != PartnerRequestStatuses.Pending)
        {
            throw new DomainRuleException("Only pending partner requests can change to the requested state.");
        }
    }
}

public static class PartnerRequestStatuses
{
    public const string Pending = "pending";

    public const string Accepted = "accepted";

    public const string Declined = "declined";

    public const string Cancelled = "cancelled";

    public const string Blocked = "blocked";

    public const string Expired = "expired";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Pending,
        Accepted,
        Declined,
        Cancelled,
        Blocked,
        Expired,
    };
}

public static class PartnerRequestTaxonomy
{
    public static readonly IReadOnlySet<string> OpenerTemplates = new HashSet<string>(StringComparer.Ordinal)
    {
        "practice-goals",
        "same-city",
        "online-practice",
        "event-follow-up",
    };
}

using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed class UserBlock
{
    private UserBlock()
    {
    }

    public UserBlock(
        Guid id,
        string blockerEmail,
        string blockedEmail,
        string? reason,
        Guid? sourcePartnerRequestId,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("User block identifier cannot be empty.");
        }

        Id = id;
        BlockerEmail = LearnerConversationProfile.NormalizeEmail(blockerEmail);
        BlockedEmail = LearnerConversationProfile.NormalizeEmail(blockedEmail);
        if (BlockerEmail == BlockedEmail)
        {
            throw new DomainRuleException("A learner cannot block themselves.");
        }

        Reason = ConversationEvent.NormalizeOptionalText(reason, 500);
        SourcePartnerRequestId = sourcePartnerRequestId;
        CreatedAtUtc = ConversationEvent.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public string BlockerEmail { get; private set; } = string.Empty;

    public string BlockedEmail { get; private set; } = string.Empty;

    public string? Reason { get; private set; }

    public Guid? SourcePartnerRequestId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}

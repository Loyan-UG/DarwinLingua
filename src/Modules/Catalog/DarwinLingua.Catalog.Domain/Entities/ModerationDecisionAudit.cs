using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed class ModerationDecisionAudit
{
    private ModerationDecisionAudit()
    {
    }

    public ModerationDecisionAudit(
        Guid id,
        Guid userReportId,
        string decisionStatus,
        string decidedBy,
        string? decisionNote,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Moderation decision audit identifier cannot be empty.");
        }

        if (userReportId == Guid.Empty)
        {
            throw new DomainRuleException("Moderation decision audit report identifier cannot be empty.");
        }

        Id = id;
        UserReportId = userReportId;
        DecisionStatus = ConversationEvent.NormalizeTaxonomyKey(decisionStatus, UserReportStatuses.All, "Moderation decision status");
        DecidedBy = LearnerConversationProfile.NormalizeEmail(decidedBy);
        DecisionNote = ConversationEvent.NormalizeOptionalText(decisionNote, 1000);
        CreatedAtUtc = ConversationEvent.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid UserReportId { get; private set; }

    public string DecisionStatus { get; private set; } = string.Empty;

    public string DecidedBy { get; private set; } = string.Empty;

    public string? DecisionNote { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}

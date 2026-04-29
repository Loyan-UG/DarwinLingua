namespace DarwinLingua.Web.Services;

public static class TransactionalEmailScenarios
{
    public const string AccountEmailConfirmation = "Account.EmailConfirmation";
    public const string AccountPasswordReset = "Account.PasswordReset";
    public const string AccountPasswordResetCompleted = "Account.PasswordResetCompleted";
    public const string AccountEmailChangeConfirmation = "Account.EmailChangeConfirmation";
    public const string AccountEmailChangedNotification = "Account.EmailChangedNotification";
    public const string OrganizerClaimSubmitted = "Organizer.ClaimSubmitted";
    public const string OrganizerClaimApproved = "Organizer.ClaimApproved";
    public const string OrganizerClaimRejected = "Organizer.ClaimRejected";
    public const string AdminNewOrganizerClaim = "Admin.NewOrganizerClaim";
    public const string EventRsvpConfirmation = "Event.RsvpConfirmation";
    public const string PartnerRequestAccepted = "Partner.RequestAccepted";
    public const string ModerationHighSeverityReport = "Moderation.HighSeverityReport";
}

public sealed record RenderedEmailTemplate(
    string TemplateKey,
    string Culture,
    string Subject,
    string PlainTextBody,
    string HtmlBody);

public sealed record TransactionalEmailMessage(
    string ScenarioKey,
    string TemplateKey,
    string RecipientEmail,
    string? RecipientUserId,
    string Culture,
    string Subject,
    string PlainTextBody,
    string HtmlBody,
    string? CorrelationId);

public sealed record TransactionalEmailSendResult(
    bool Succeeded,
    string ProviderName,
    string? ProviderMessageId,
    string? FailureCode,
    string? FailureMessageSummary);

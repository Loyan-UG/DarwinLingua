using DarwinLingua.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Services;

public sealed class PolicyAcceptanceService(WebIdentityDbContext dbContext) : IPolicyAcceptanceService
{
    public const string TermsOfUsePolicyKey = "terms-of-use";
    public const string PrivacyNoticePolicyKey = "privacy-notice";
    public const string CurrentTermsOfUseVersion = "2026-05-25";
    public const string CurrentPrivacyNoticeVersion = "2026-05-25";
    public const string RegistrationSource = "registration";

    public async Task RecordRegistrationAcceptancesAsync(
        string userId,
        string? culture,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("A user id is required to record policy acceptance.", nameof(userId));
        }

        DateTime acceptedAtUtc = DateTime.UtcNow;
        string normalizedCulture = string.IsNullOrWhiteSpace(culture) ? "en" : culture.Trim();

        await UpsertAcceptanceAsync(
                userId,
                TermsOfUsePolicyKey,
                CurrentTermsOfUseVersion,
                acceptedAtUtc,
                normalizedCulture,
                cancellationToken)
            .ConfigureAwait(false);

        await UpsertAcceptanceAsync(
                userId,
                PrivacyNoticePolicyKey,
                CurrentPrivacyNoticeVersion,
                acceptedAtUtc,
                normalizedCulture,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task UpsertAcceptanceAsync(
        string userId,
        string policyKey,
        string policyVersion,
        DateTime acceptedAtUtc,
        string culture,
        CancellationToken cancellationToken)
    {
        WebPolicyAcceptance? existing = await dbContext.PolicyAcceptances
            .FirstOrDefaultAsync(
                acceptance =>
                    acceptance.UserId == userId &&
                    acceptance.PolicyKey == policyKey &&
                    acceptance.PolicyVersion == policyVersion,
                cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            existing.AcceptedAtUtc = acceptedAtUtc;
            existing.Source = RegistrationSource;
            existing.Culture = culture;
        }
        else
        {
            dbContext.PolicyAcceptances.Add(new WebPolicyAcceptance
            {
                UserId = userId,
                PolicyKey = policyKey,
                PolicyVersion = policyVersion,
                AcceptedAtUtc = acceptedAtUtc,
                Source = RegistrationSource,
                Culture = culture
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Identity;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Services;

public interface IAccountDataSelfService
{
    string DeleteConfirmationPhrase { get; }

    Task<AccountDataExportModel> ExportAsync(
        DarwinLinguaIdentityUser user,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken);

    Task<AccountDeletionResult> DeleteAsync(
        DarwinLinguaIdentityUser user,
        string? currentPassword,
        string? confirmationPhrase,
        CancellationToken cancellationToken);
}

public sealed class AccountDataSelfService(
    WebIdentityDbContext webIdentityDbContext,
    IDbContextFactory<DarwinLinguaDbContext> sharedDbContextFactory,
    UserManager<DarwinLinguaIdentityUser> userManager) : IAccountDataSelfService
{
    public string DeleteConfirmationPhrase => "DELETE";

    public async Task<AccountDataExportModel> ExportAsync(
        DarwinLinguaIdentityUser user,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(roles);

        string userId = user.Id;
        string? normalizedEmail = NormalizeEmail(user.Email);

        AccountDataExportWebSection web = await LoadWebSectionAsync(userId, cancellationToken)
            .ConfigureAwait(false);
        AccountDataExportLearningSection learning = await LoadLearningSectionAsync(userId, normalizedEmail, cancellationToken)
            .ConfigureAwait(false);

        return new AccountDataExportModel(
            ExportedAtUtc: DateTimeOffset.UtcNow,
            Account: new AccountDataExportIdentitySection(
                user.Id,
                user.UserName,
                user.Email,
                user.EmailConfirmed,
                user.LockoutEnd,
                user.AccessFailedCount,
                roles.OrderBy(static role => role, StringComparer.OrdinalIgnoreCase).ToArray()),
            Web: web,
            Learning: learning,
            RetentionNotes:
            [
                "Transactional email delivery diagnostics are exported as operational summaries; email bodies and recovery URLs are not stored.",
                "Billing and entitlement audit records may be retained where legal, accounting, fraud-prevention, or security obligations require it.",
                "Backups are restored and expired under the operational backup policy rather than edited record-by-record."
            ]);
    }

    public async Task<AccountDeletionResult> DeleteAsync(
        DarwinLinguaIdentityUser user,
        string? currentPassword,
        string? confirmationPhrase,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (!string.Equals(confirmationPhrase?.Trim(), DeleteConfirmationPhrase, StringComparison.Ordinal))
        {
            return AccountDeletionResult.Failed("Type DELETE exactly to confirm account deletion.");
        }

        if (await userManager.HasPasswordAsync(user).ConfigureAwait(false))
        {
            if (string.IsNullOrWhiteSpace(currentPassword) ||
                !await userManager.CheckPasswordAsync(user, currentPassword).ConfigureAwait(false))
            {
                return AccountDeletionResult.Failed("The current password is required before this account can be deleted.");
            }
        }

        string userId = user.Id;
        string? normalizedEmail = NormalizeEmail(user.Email);

        AccountDeletionCounts webCounts = await DeleteWebStateAsync(userId, normalizedEmail, cancellationToken)
            .ConfigureAwait(false);
        AccountDeletionCounts learningCounts = await DeleteLearningStateAsync(userId, normalizedEmail, cancellationToken)
            .ConfigureAwait(false);

        IdentityResult identityResult = await userManager.DeleteAsync(user).ConfigureAwait(false);
        if (!identityResult.Succeeded)
        {
            return AccountDeletionResult.Failed(string.Join(
                " ",
                identityResult.Errors.Select(static error => error.Description)));
        }

        return AccountDeletionResult.Completed(webCounts + learningCounts);
    }

    private async Task<AccountDataExportWebSection> LoadWebSectionAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        WebUserPreference[] preferences = await webIdentityDbContext.UserPreferences
            .AsNoTracking()
            .Where(preference => preference.ActorId == userId)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        WebUserFavoriteWord[] favoriteWords = await webIdentityDbContext.UserFavoriteWords
            .AsNoTracking()
            .Where(favorite => favorite.ActorId == userId)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        WebUserWordState[] wordStates = await webIdentityDbContext.UserWordStates
            .AsNoTracking()
            .Where(state => state.ActorId == userId)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        WebPolicyAcceptance[] policyAcceptances = await webIdentityDbContext.PolicyAcceptances
            .AsNoTracking()
            .Where(acceptance => acceptance.UserId == userId)
            .OrderBy(acceptance => acceptance.AcceptedAtUtc)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        UserEntitlementState? entitlementState = await webIdentityDbContext.UserEntitlementStates
            .AsNoTracking()
            .SingleOrDefaultAsync(entitlement => entitlement.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
        UserEntitlementAuditEvent[] entitlementAuditEvents = await webIdentityDbContext.UserEntitlementAuditEvents
            .AsNoTracking()
            .Where(audit => audit.UserId == userId)
            .OrderBy(audit => audit.CreatedAtUtc)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        WebBillingProfile? billingProfile = await webIdentityDbContext.WebBillingProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(profile => profile.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
        WebBillingEvent[] billingEvents = await webIdentityDbContext.WebBillingEvents
            .AsNoTracking()
            .Where(billingEvent => billingEvent.UserId == userId)
            .OrderBy(billingEvent => billingEvent.CreatedAtUtc)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        WebBillingNotification[] billingNotifications = await webIdentityDbContext.WebBillingNotifications
            .AsNoTracking()
            .Where(notification => notification.UserId == userId)
            .OrderBy(notification => notification.CreatedAtUtc)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        WebEmailDeliveryLog[] emailLogs = await webIdentityDbContext.EmailDeliveryLogs
            .AsNoTracking()
            .Where(log => log.RecipientUserId == userId)
            .OrderBy(log => log.CreatedAtUtc)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        WebWordSuggestion[] wordSuggestions = await webIdentityDbContext.WebWordSuggestions
            .AsNoTracking()
            .Where(suggestion => suggestion.UserId == userId)
            .OrderBy(suggestion => suggestion.CreatedAtUtc)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AccountDataExportWebSection(
            preferences,
            favoriteWords,
            wordStates,
            policyAcceptances,
            entitlementState,
            entitlementAuditEvents,
            billingProfile,
            billingEvents,
            billingNotifications,
            emailLogs.Select(static log => new AccountDataExportEmailLog(
                log.Id,
                log.ScenarioKey,
                log.RecipientEmailHash,
                log.TemplateKey,
                log.Culture,
                log.Subject,
                log.ProviderName,
                log.ProviderMessageId,
                log.ProviderLastEvent,
                log.ProviderLastEventAtUtc,
                log.Status.ToString(),
                log.FailureCode,
                log.FailureMessageSummary,
                log.CreatedAtUtc,
                log.SentAtUtc)).ToArray(),
            wordSuggestions);
    }

    private async Task<AccountDataExportLearningSection> LoadLearningSectionAsync(
        string userId,
        string? normalizedEmail,
        CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext sharedDbContext = await sharedDbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        var learningProfiles = await sharedDbContext.UserLearningProfiles
            .AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var contentProgress = await sharedDbContext.UserContentProgress
            .AsNoTracking()
            .Where(progress => progress.UserId == userId)
            .OrderBy(progress => progress.ContentOwnerType)
            .ThenBy(progress => progress.ContentOwnerSlug)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var practiceReviewStates = await sharedDbContext.PracticeReviewStates
            .AsNoTracking()
            .Where(state => state.UserId == userId)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var practiceAttempts = await sharedDbContext.PracticeAttempts
            .AsNoTracking()
            .Where(attempt => attempt.UserId == userId)
            .OrderBy(attempt => attempt.AttemptedAtUtc)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var exerciseAttempts = await sharedDbContext.UserExerciseAttempts
            .AsNoTracking()
            .Where(attempt => attempt.UserId == userId)
            .OrderBy(attempt => attempt.AttemptedAtUtc)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var favoriteWords = await sharedDbContext.UserFavoriteWords
            .AsNoTracking()
            .Where(favorite => favorite.UserId == userId)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var wordStates = await sharedDbContext.UserWordStates
            .AsNoTracking()
            .Where(state => state.UserId == userId)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        LearnerConversationProfile[] learnerProfiles = [];
        PartnerRequest[] partnerRequests = [];
        UserReport[] userReports = [];
        UserBlock[] userBlocks = [];
        EventRsvp[] eventRsvps = [];

        if (!string.IsNullOrWhiteSpace(normalizedEmail))
        {
            learnerProfiles = await sharedDbContext.LearnerConversationProfiles
                .AsNoTracking()
                .Where(profile => profile.OwnerEmail == normalizedEmail)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            Guid[] profileIds = learnerProfiles.Select(static profile => profile.Id).ToArray();
            partnerRequests = await sharedDbContext.PartnerRequests
                .AsNoTracking()
                .Where(request => request.RequesterEmail == normalizedEmail || profileIds.Contains(request.TargetLearnerProfileId))
                .OrderBy(request => request.CreatedAtUtc)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            userReports = await sharedDbContext.UserReports
                .AsNoTracking()
                .Where(report => report.ReporterEmail == normalizedEmail || report.ReportedUserEmail == normalizedEmail)
                .OrderBy(report => report.CreatedAtUtc)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            userBlocks = await sharedDbContext.UserBlocks
                .AsNoTracking()
                .Where(block => block.BlockerEmail == normalizedEmail || block.BlockedEmail == normalizedEmail)
                .OrderBy(block => block.CreatedAtUtc)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            eventRsvps = await sharedDbContext.EventRsvps
                .AsNoTracking()
                .Where(rsvp => rsvp.ParticipantEmail == normalizedEmail)
                .OrderBy(rsvp => rsvp.CreatedAtUtc)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return new AccountDataExportLearningSection(
            learningProfiles,
            favoriteWords,
            wordStates,
            contentProgress,
            practiceReviewStates,
            practiceAttempts,
            exerciseAttempts,
            learnerProfiles,
            partnerRequests,
            userReports,
            userBlocks,
            eventRsvps);
    }

    private async Task<AccountDeletionCounts> DeleteWebStateAsync(
        string userId,
        string? normalizedEmail,
        CancellationToken cancellationToken)
    {
        int removed = 0;
        int anonymized = 0;
        int detached = 0;

        removed += await RemoveRangeAsync(webIdentityDbContext, webIdentityDbContext.UserPreferences.Where(preference => preference.ActorId == userId), cancellationToken)
            .ConfigureAwait(false);
        removed += await RemoveRangeAsync(webIdentityDbContext, webIdentityDbContext.UserFavoriteWords.Where(favorite => favorite.ActorId == userId), cancellationToken)
            .ConfigureAwait(false);
        removed += await RemoveRangeAsync(webIdentityDbContext, webIdentityDbContext.UserWordStates.Where(state => state.ActorId == userId), cancellationToken)
            .ConfigureAwait(false);
        removed += await RemoveRangeAsync(webIdentityDbContext, webIdentityDbContext.WebWordSuggestions.Where(suggestion => suggestion.UserId == userId), cancellationToken)
            .ConfigureAwait(false);
        removed += await RemoveRangeAsync(webIdentityDbContext, webIdentityDbContext.UserEntitlementStates.Where(entitlement => entitlement.UserId == userId), cancellationToken)
            .ConfigureAwait(false);
        removed += await RemoveRangeAsync(webIdentityDbContext, webIdentityDbContext.WebBillingProfiles.Where(profile => profile.UserId == userId), cancellationToken)
            .ConfigureAwait(false);

        WebBillingEvent[] billingEvents = await webIdentityDbContext.WebBillingEvents
            .Where(billingEvent => billingEvent.UserId == userId)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        foreach (WebBillingEvent billingEvent in billingEvents)
        {
            billingEvent.UserId = null;
            detached++;
        }

        WebBillingNotification[] billingNotifications = await webIdentityDbContext.WebBillingNotifications
            .Where(notification => notification.UserId == userId)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        foreach (WebBillingNotification notification in billingNotifications)
        {
            notification.UserId = null;
            detached++;
        }

        WebEmailDeliveryLog[] emailLogs = await webIdentityDbContext.EmailDeliveryLogs
            .Where(log => log.RecipientUserId == userId)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        foreach (WebEmailDeliveryLog emailLog in emailLogs)
        {
            emailLog.RecipientUserId = null;
            detached++;
        }

        if (!string.IsNullOrWhiteSpace(normalizedEmail))
        {
            WebWordSuggestion[] suggestions = await webIdentityDbContext.WebWordSuggestions
                .Where(suggestion => suggestion.Email == normalizedEmail)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (WebWordSuggestion suggestion in suggestions)
            {
                suggestion.UserId = null;
                suggestion.Email = null;
                suggestion.ActorId = "deleted-user";
                suggestion.Note = null;
                anonymized++;
            }
        }

        await webIdentityDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new AccountDeletionCounts(removed, anonymized, detached);
    }

    private async Task<AccountDeletionCounts> DeleteLearningStateAsync(
        string userId,
        string? normalizedEmail,
        CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext sharedDbContext = await sharedDbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        int removed = 0;
        int anonymized = 0;

        removed += await RemoveRangeAsync(sharedDbContext, sharedDbContext.UserLearningProfiles.Where(profile => profile.UserId == userId), cancellationToken)
            .ConfigureAwait(false);
        removed += await RemoveRangeAsync(sharedDbContext, sharedDbContext.UserFavoriteWords.Where(favorite => favorite.UserId == userId), cancellationToken)
            .ConfigureAwait(false);
        removed += await RemoveRangeAsync(sharedDbContext, sharedDbContext.UserWordStates.Where(state => state.UserId == userId), cancellationToken)
            .ConfigureAwait(false);
        removed += await RemoveRangeAsync(sharedDbContext, sharedDbContext.UserContentProgress.Where(progress => progress.UserId == userId), cancellationToken)
            .ConfigureAwait(false);
        removed += await RemoveRangeAsync(sharedDbContext, sharedDbContext.PracticeReviewStates.Where(state => state.UserId == userId), cancellationToken)
            .ConfigureAwait(false);
        removed += await RemoveRangeAsync(sharedDbContext, sharedDbContext.PracticeAttempts.Where(attempt => attempt.UserId == userId), cancellationToken)
            .ConfigureAwait(false);
        removed += await RemoveRangeAsync(sharedDbContext, sharedDbContext.UserExerciseAttempts.Where(attempt => attempt.UserId == userId), cancellationToken)
            .ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(normalizedEmail))
        {
            LearnerConversationProfile[] profiles = await sharedDbContext.LearnerConversationProfiles
                .Where(profile => profile.OwnerEmail == normalizedEmail)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            Guid[] profileIds = profiles.Select(static profile => profile.Id).ToArray();

            removed += await RemoveRangeAsync(sharedDbContext, sharedDbContext.PartnerRequests
                .Where(request => request.RequesterEmail == normalizedEmail || profileIds.Contains(request.TargetLearnerProfileId)), cancellationToken)
                .ConfigureAwait(false);
            removed += await RemoveRangeAsync(sharedDbContext, sharedDbContext.UserReports
                .Where(report => report.ReporterEmail == normalizedEmail || report.ReportedUserEmail == normalizedEmail), cancellationToken)
                .ConfigureAwait(false);
            removed += await RemoveRangeAsync(sharedDbContext, sharedDbContext.UserBlocks
                .Where(block => block.BlockerEmail == normalizedEmail || block.BlockedEmail == normalizedEmail), cancellationToken)
                .ConfigureAwait(false);
            removed += await RemoveRangeAsync(sharedDbContext, sharedDbContext.EventRsvps
                .Where(rsvp => rsvp.ParticipantEmail == normalizedEmail), cancellationToken)
                .ConfigureAwait(false);

            DateTime updatedAtUtc = DateTime.UtcNow;
            foreach (LearnerConversationProfile profile in profiles)
            {
                profile.Anonymize(updatedAtUtc);
                anonymized++;
            }
        }

        await sharedDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new AccountDeletionCounts(removed, anonymized, 0);
    }

    private static async Task<int> RemoveRangeAsync<TEntity>(
        DbContext dbContext,
        IQueryable<TEntity> query,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        TEntity[] entities = await query.ToArrayAsync(cancellationToken).ConfigureAwait(false);
        if (entities.Length == 0)
        {
            return 0;
        }

        dbContext.RemoveRange(entities);
        return entities.Length;
    }

    private static string? NormalizeEmail(string? email) =>
        string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
}

public sealed record AccountDataExportModel(
    DateTimeOffset ExportedAtUtc,
    AccountDataExportIdentitySection Account,
    AccountDataExportWebSection Web,
    AccountDataExportLearningSection Learning,
    IReadOnlyList<string> RetentionNotes);

public sealed record AccountDataExportIdentitySection(
    string UserId,
    string? UserName,
    string? Email,
    bool EmailConfirmed,
    DateTimeOffset? LockoutEnd,
    int AccessFailedCount,
    IReadOnlyList<string> Roles);

public sealed record AccountDataExportWebSection(
    IReadOnlyList<WebUserPreference> Preferences,
    IReadOnlyList<WebUserFavoriteWord> FavoriteWords,
    IReadOnlyList<WebUserWordState> WordStates,
    IReadOnlyList<WebPolicyAcceptance> PolicyAcceptances,
    UserEntitlementState? EntitlementState,
    IReadOnlyList<UserEntitlementAuditEvent> EntitlementAuditEvents,
    WebBillingProfile? BillingProfile,
    IReadOnlyList<WebBillingEvent> BillingEvents,
    IReadOnlyList<WebBillingNotification> BillingNotifications,
    IReadOnlyList<AccountDataExportEmailLog> EmailDeliveryLogs,
    IReadOnlyList<WebWordSuggestion> WordSuggestions);

public sealed record AccountDataExportEmailLog(
    Guid Id,
    string ScenarioKey,
    string RecipientEmailHash,
    string TemplateKey,
    string Culture,
    string Subject,
    string ProviderName,
    string? ProviderMessageId,
    string? ProviderLastEvent,
    DateTimeOffset? ProviderLastEventAtUtc,
    string Status,
    string? FailureCode,
    string? FailureMessageSummary,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? SentAtUtc);

public sealed record AccountDataExportLearningSection(
    IReadOnlyList<object> LearningProfiles,
    IReadOnlyList<object> FavoriteWords,
    IReadOnlyList<object> WordStates,
    IReadOnlyList<object> ContentProgress,
    IReadOnlyList<object> PracticeReviewStates,
    IReadOnlyList<object> PracticeAttempts,
    IReadOnlyList<object> ExerciseAttempts,
    IReadOnlyList<LearnerConversationProfile> LearnerConversationProfiles,
    IReadOnlyList<PartnerRequest> PartnerRequests,
    IReadOnlyList<UserReport> UserReports,
    IReadOnlyList<UserBlock> UserBlocks,
    IReadOnlyList<EventRsvp> EventRsvps);

public sealed record AccountDeletionResult(
    bool Succeeded,
    string? ErrorMessage,
    AccountDeletionCounts Counts)
{
    public static AccountDeletionResult Completed(AccountDeletionCounts counts) => new(true, null, counts);

    public static AccountDeletionResult Failed(string errorMessage) => new(false, errorMessage, new AccountDeletionCounts(0, 0, 0));
}

public sealed record AccountDeletionCounts(int RemovedRows, int AnonymizedRows, int DetachedAuditRows)
{
    public static AccountDeletionCounts operator +(AccountDeletionCounts left, AccountDeletionCounts right) =>
        new(
            left.RemovedRows + right.RemovedRows,
            left.AnonymizedRows + right.AnonymizedRows,
            left.DetachedAuditRows + right.DetachedAuditRows);
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Identity;

public sealed class UserEntitlementService<TContext>(
    TContext dbContext,
    IOptions<DarwinLinguaEntitlementOptions> options) : IUserEntitlementService
    where TContext : DarwinLinguaIdentityDbContext
{
    private const string InitialTrialEventType = "initial-trial";
    private const string InitialFreeEventType = "initial-free";
    private const string TierChangedEventType = "tier-changed";
    private const string TrialExpiredEventType = "trial-expired";
    private const string PremiumExpiredEventType = "premium-expired";
    private const string InvalidTierNormalizedEventType = "invalid-tier-normalized";

    private static readonly string[] FreeFeatures =
    [
        DarwinLinguaFeatureKeys.BrowseCatalog,
        DarwinLinguaFeatureKeys.SearchCatalog,
        DarwinLinguaFeatureKeys.ViewWordDetails,
        DarwinLinguaFeatureKeys.UseCollections,
        DarwinLinguaFeatureKeys.UseTopics,
    ];

    private static readonly string[] PaidFeatures =
    [
        DarwinLinguaFeatureKeys.Favorites,
        DarwinLinguaFeatureKeys.DualMeaningLanguage,
        DarwinLinguaFeatureKeys.AdvancedPractice,
    ];

    public async Task<UserEntitlementSnapshot> GetCurrentAsync(string userId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        UserEntitlementState? state = await dbContext.UserEntitlementStates
            .SingleOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        bool requiresSave = false;

        if (state is null)
        {
            state = CreateInitialState(userId, nowUtc);
            dbContext.UserEntitlementStates.Add(state);
            AddAuditEvent(
                state,
                null,
                state.Tier,
                null,
                state.TrialEndsAtUtc,
                null,
                state.PremiumEndsAtUtc,
                "system",
                string.Equals(state.Tier, DarwinLinguaEntitlementTiers.Trial, StringComparison.Ordinal)
                    ? InitialTrialEventType
                    : InitialFreeEventType,
                nowUtc);
            requiresSave = true;
        }
        else
        {
            requiresSave = NormalizeState(state, nowUtc, addAuditEvent: true);
        }

        if (requiresSave)
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return BuildSnapshot(state);
    }

    public async Task<bool> HasFeatureAsync(string userId, string featureKey, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureKey);

        UserEntitlementSnapshot snapshot = await GetCurrentAsync(userId, cancellationToken).ConfigureAwait(false);
        return snapshot.EnabledFeatures.Contains(featureKey, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyList<UserEntitlementAuditEventModel>> GetRecentAuditEventsAsync(
        string userId,
        int take,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        int boundedTake = Math.Clamp(take, 1, 100);
        UserEntitlementAuditEvent[] auditEvents = await dbContext.UserEntitlementAuditEvents
            .AsNoTracking()
            .Where(auditEvent => auditEvent.UserId == userId)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return auditEvents
            .OrderByDescending(auditEvent => auditEvent.CreatedAtUtc)
            .Take(boundedTake)
            .Select(auditEvent => new UserEntitlementAuditEventModel(
                auditEvent.Id,
                auditEvent.UserId,
                auditEvent.EventType,
                auditEvent.PreviousTier,
                auditEvent.NewTier,
                auditEvent.PreviousTrialEndsAtUtc,
                auditEvent.NewTrialEndsAtUtc,
                auditEvent.PreviousPremiumEndsAtUtc,
                auditEvent.NewPremiumEndsAtUtc,
                auditEvent.UpdatedBy,
                auditEvent.CreatedAtUtc))
            .ToArray();
    }

    public async Task<UserEntitlementSnapshot> SetTierAsync(
        string userId,
        string tier,
        DateTimeOffset? expiresAtUtc,
        string? updatedBy,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        string normalizedTier = DarwinLinguaEntitlementTiers.All.Single(
            candidate => string.Equals(candidate, tier, StringComparison.OrdinalIgnoreCase));
        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;

        UserEntitlementState? state = await dbContext.UserEntitlementStates
            .SingleOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        if (state is null)
        {
            state = new UserEntitlementState
            {
                UserId = userId,
                CreatedAtUtc = nowUtc,
            };

            dbContext.UserEntitlementStates.Add(state);
        }

        string? previousTier = state.Tier;
        DateTimeOffset? previousTrialEndsAtUtc = state.TrialEndsAtUtc;
        DateTimeOffset? previousPremiumEndsAtUtc = state.PremiumEndsAtUtc;
        string normalizedUpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy.Trim();

        state.Tier = normalizedTier;
        state.UpdatedAtUtc = nowUtc;
        state.LastUpdatedBy = normalizedUpdatedBy;

        if (string.Equals(normalizedTier, DarwinLinguaEntitlementTiers.Trial, StringComparison.Ordinal))
        {
            state.TrialStartedAtUtc ??= nowUtc;
            state.TrialEndsAtUtc = expiresAtUtc;
            state.PremiumStartedAtUtc = null;
            state.PremiumEndsAtUtc = null;
        }
        else if (string.Equals(normalizedTier, DarwinLinguaEntitlementTiers.Premium, StringComparison.Ordinal))
        {
            state.PremiumStartedAtUtc ??= nowUtc;
            state.PremiumEndsAtUtc = expiresAtUtc;
        }
        else
        {
            state.PremiumStartedAtUtc = null;
            state.PremiumEndsAtUtc = null;
        }

        AddAuditEvent(
            state,
            previousTier,
            state.Tier,
            previousTrialEndsAtUtc,
            state.TrialEndsAtUtc,
            previousPremiumEndsAtUtc,
            state.PremiumEndsAtUtc,
            normalizedUpdatedBy,
            TierChangedEventType,
            nowUtc);

        NormalizeState(state, nowUtc, addAuditEvent: true);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return BuildSnapshot(state);
    }

    private UserEntitlementState CreateInitialState(string userId, DateTimeOffset nowUtc)
    {
        UserEntitlementState state = new()
        {
            UserId = userId,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc,
            LastUpdatedBy = "system",
        };

        if (options.Value.NewUserTrialDays > 0)
        {
            state.Tier = DarwinLinguaEntitlementTiers.Trial;
            state.TrialStartedAtUtc = nowUtc;
            state.TrialEndsAtUtc = nowUtc.AddDays(options.Value.NewUserTrialDays);
        }
        else
        {
            state.Tier = DarwinLinguaEntitlementTiers.Free;
        }

        return state;
    }

    private bool NormalizeState(UserEntitlementState state, DateTimeOffset nowUtc, bool addAuditEvent)
    {
        bool changed = false;

        if (string.Equals(state.Tier, DarwinLinguaEntitlementTiers.Trial, StringComparison.OrdinalIgnoreCase) &&
            state.TrialEndsAtUtc is not null &&
            state.TrialEndsAtUtc <= nowUtc)
        {
            string previousTier = state.Tier;
            DateTimeOffset? previousTrialEndsAtUtc = state.TrialEndsAtUtc;
            DateTimeOffset? previousPremiumEndsAtUtc = state.PremiumEndsAtUtc;
            state.Tier = DarwinLinguaEntitlementTiers.Free;
            state.UpdatedAtUtc = nowUtc;
            state.LastUpdatedBy = "system";
            if (addAuditEvent)
            {
                AddAuditEvent(
                    state,
                    previousTier,
                    state.Tier,
                    previousTrialEndsAtUtc,
                    state.TrialEndsAtUtc,
                    previousPremiumEndsAtUtc,
                    state.PremiumEndsAtUtc,
                    "system",
                    TrialExpiredEventType,
                    nowUtc);
            }

            changed = true;
        }

        if (string.Equals(state.Tier, DarwinLinguaEntitlementTiers.Premium, StringComparison.OrdinalIgnoreCase) &&
            state.PremiumEndsAtUtc is not null &&
            state.PremiumEndsAtUtc <= nowUtc)
        {
            string previousTier = state.Tier;
            DateTimeOffset? previousTrialEndsAtUtc = state.TrialEndsAtUtc;
            DateTimeOffset? previousPremiumEndsAtUtc = state.PremiumEndsAtUtc;
            state.Tier = DarwinLinguaEntitlementTiers.Free;
            state.PremiumStartedAtUtc = null;
            state.PremiumEndsAtUtc = null;
            state.UpdatedAtUtc = nowUtc;
            state.LastUpdatedBy = "system";
            if (addAuditEvent)
            {
                AddAuditEvent(
                    state,
                    previousTier,
                    state.Tier,
                    previousTrialEndsAtUtc,
                    state.TrialEndsAtUtc,
                    previousPremiumEndsAtUtc,
                    state.PremiumEndsAtUtc,
                    "system",
                    PremiumExpiredEventType,
                    nowUtc);
            }

            changed = true;
        }

        if (!DarwinLinguaEntitlementTiers.All.Contains(state.Tier, StringComparer.OrdinalIgnoreCase))
        {
            string previousTier = state.Tier;
            DateTimeOffset? previousTrialEndsAtUtc = state.TrialEndsAtUtc;
            DateTimeOffset? previousPremiumEndsAtUtc = state.PremiumEndsAtUtc;
            state.Tier = DarwinLinguaEntitlementTiers.Free;
            state.UpdatedAtUtc = nowUtc;
            state.LastUpdatedBy = "system";
            if (addAuditEvent)
            {
                AddAuditEvent(
                    state,
                    previousTier,
                    state.Tier,
                    previousTrialEndsAtUtc,
                    state.TrialEndsAtUtc,
                    previousPremiumEndsAtUtc,
                    state.PremiumEndsAtUtc,
                    "system",
                    InvalidTierNormalizedEventType,
                    nowUtc);
            }

            changed = true;
        }

        return changed;
    }

    private void AddAuditEvent(
        UserEntitlementState state,
        string? previousTier,
        string newTier,
        DateTimeOffset? previousTrialEndsAtUtc,
        DateTimeOffset? newTrialEndsAtUtc,
        DateTimeOffset? previousPremiumEndsAtUtc,
        DateTimeOffset? newPremiumEndsAtUtc,
        string updatedBy,
        string eventType,
        DateTimeOffset createdAtUtc)
    {
        dbContext.UserEntitlementAuditEvents.Add(new UserEntitlementAuditEvent
        {
            Id = Guid.NewGuid(),
            UserId = state.UserId,
            EventType = eventType,
            PreviousTier = previousTier,
            NewTier = newTier,
            PreviousTrialEndsAtUtc = previousTrialEndsAtUtc,
            NewTrialEndsAtUtc = newTrialEndsAtUtc,
            PreviousPremiumEndsAtUtc = previousPremiumEndsAtUtc,
            NewPremiumEndsAtUtc = newPremiumEndsAtUtc,
            UpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy.Trim(),
            CreatedAtUtc = createdAtUtc,
        });
    }

    private static UserEntitlementSnapshot BuildSnapshot(UserEntitlementState state)
    {
        IReadOnlyList<string> features = ResolveFeatures(state.Tier);
        return new UserEntitlementSnapshot(
            state.UserId,
            state.Tier,
            state.TrialEndsAtUtc,
            state.PremiumEndsAtUtc,
            features);
    }

    private static IReadOnlyList<string> ResolveFeatures(string tier)
    {
        if (string.Equals(tier, DarwinLinguaEntitlementTiers.Trial, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(tier, DarwinLinguaEntitlementTiers.Premium, StringComparison.OrdinalIgnoreCase))
        {
            return [.. FreeFeatures, .. PaidFeatures];
        }

        return FreeFeatures;
    }
}

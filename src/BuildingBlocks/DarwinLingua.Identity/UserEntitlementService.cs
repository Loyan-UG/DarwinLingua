using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Identity;

public sealed class UserEntitlementService<TContext>(
    TContext dbContext,
    IOptions<DarwinLinguaEntitlementOptions> options) : IUserEntitlementService
    where TContext : DarwinLinguaIdentityDbContext
{
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
            requiresSave = true;
        }
        else
        {
            requiresSave = NormalizeState(state, nowUtc);
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

        state.Tier = normalizedTier;
        state.UpdatedAtUtc = nowUtc;
        state.LastUpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy.Trim();

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

        NormalizeState(state, nowUtc);
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

    private static bool NormalizeState(UserEntitlementState state, DateTimeOffset nowUtc)
    {
        bool changed = false;

        if (string.Equals(state.Tier, DarwinLinguaEntitlementTiers.Trial, StringComparison.OrdinalIgnoreCase) &&
            state.TrialEndsAtUtc is not null &&
            state.TrialEndsAtUtc <= nowUtc)
        {
            state.Tier = DarwinLinguaEntitlementTiers.Free;
            state.UpdatedAtUtc = nowUtc;
            state.LastUpdatedBy = "system";
            changed = true;
        }

        if (string.Equals(state.Tier, DarwinLinguaEntitlementTiers.Premium, StringComparison.OrdinalIgnoreCase) &&
            state.PremiumEndsAtUtc is not null &&
            state.PremiumEndsAtUtc <= nowUtc)
        {
            state.Tier = DarwinLinguaEntitlementTiers.Free;
            state.PremiumStartedAtUtc = null;
            state.PremiumEndsAtUtc = null;
            state.UpdatedAtUtc = nowUtc;
            state.LastUpdatedBy = "system";
            changed = true;
        }

        if (!DarwinLinguaEntitlementTiers.All.Contains(state.Tier, StringComparer.OrdinalIgnoreCase))
        {
            state.Tier = DarwinLinguaEntitlementTiers.Free;
            state.UpdatedAtUtc = nowUtc;
            state.LastUpdatedBy = "system";
            changed = true;
        }

        return changed;
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

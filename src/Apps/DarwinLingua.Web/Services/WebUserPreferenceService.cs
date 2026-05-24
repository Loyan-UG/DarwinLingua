using DarwinLingua.Learning.Application.Models;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Services;

public interface IWebUserPreferenceService
{
    Task<UserLearningProfileModel> GetProfileAsync(CancellationToken cancellationToken);

    Task<UserLearningProfileModel> UpdatePreferencesAsync(
        string uiLanguageCode,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        bool allowsRudeSlangContent,
        string adultContentAccessState,
        CancellationToken cancellationToken);
}

internal sealed class WebUserPreferenceService(
    IWebActorContextAccessor actorContextAccessor,
    WebIdentityDbContext dbContext) : IWebUserPreferenceService
{
    private static readonly HashSet<string> SupportedUiLanguageCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "en",
        "de",
    };

    private static readonly HashSet<string> SupportedMeaningLanguageCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "ar",
        "ckb",
        "en",
        "fa",
        "kmr",
        "pl",
        "ro",
        "ru",
        "sq",
        "tr",
    };

    public async Task<UserLearningProfileModel> GetProfileAsync(CancellationToken cancellationToken)
    {
        WebActorContext actor = actorContextAccessor.GetCurrentActor();

        WebUserPreference? preference = await dbContext.UserPreferences
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.ActorId == actor.ActorId, cancellationToken)
            .ConfigureAwait(false);

        if (preference is not null)
        {
            return Map(actor.ActorId, preference);
        }

        if (!actor.IsAuthenticated)
        {
            return CreateDefaultProfile(actor.ActorId);
        }

        WebUserPreference createdPreference = new()
        {
            Id = Guid.NewGuid(),
            ActorId = actor.ActorId,
            UiLanguageCode = "en",
            PrimaryMeaningLanguageCode = "en",
            AllowsRudeSlangContent = false,
            AdultContentAccessState = AdultContentAccessStates.NotRequested,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        dbContext.UserPreferences.Add(createdPreference);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Map(actor.ActorId, createdPreference);
    }

    public async Task<UserLearningProfileModel> UpdatePreferencesAsync(
        string uiLanguageCode,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        bool allowsRudeSlangContent,
        string adultContentAccessState,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(uiLanguageCode, primaryMeaningLanguageCode, secondaryMeaningLanguageCode, adultContentAccessState, cancellationToken)
            .ConfigureAwait(false);

        WebActorContext actor = actorContextAccessor.GetCurrentActor();

        WebUserPreference? preference = await dbContext.UserPreferences
            .SingleOrDefaultAsync(item => item.ActorId == actor.ActorId, cancellationToken)
            .ConfigureAwait(false);

        if (preference is null)
        {
            preference = new WebUserPreference
            {
                Id = Guid.NewGuid(),
                ActorId = actor.ActorId,
                CreatedAtUtc = DateTime.UtcNow
            };

            dbContext.UserPreferences.Add(preference);
        }

        preference.UiLanguageCode = uiLanguageCode.Trim().ToLowerInvariant();
        preference.PrimaryMeaningLanguageCode = primaryMeaningLanguageCode.Trim().ToLowerInvariant();
        preference.SecondaryMeaningLanguageCode = string.IsNullOrWhiteSpace(secondaryMeaningLanguageCode)
            ? null
            : secondaryMeaningLanguageCode.Trim().ToLowerInvariant();
        preference.AllowsRudeSlangContent = allowsRudeSlangContent;
        preference.AdultContentAccessState = ResolveUserWritableAdultAccessState(adultContentAccessState, preference.AdultContentAccessState);
        preference.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Map(actor.ActorId, preference);
    }

    private async Task ValidateAsync(
        string uiLanguageCode,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        string adultContentAccessState,
        CancellationToken cancellationToken)
    {
        LanguageCode uiLanguage = LanguageCode.From(uiLanguageCode);
        LanguageCode primaryMeaningLanguage = LanguageCode.From(primaryMeaningLanguageCode);

        cancellationToken.ThrowIfCancellationRequested();

        if (!SupportedUiLanguageCodes.Contains(uiLanguage.Value))
        {
            throw new InvalidOperationException($"Unsupported UI language code '{uiLanguageCode}'.");
        }

        if (!SupportedMeaningLanguageCodes.Contains(primaryMeaningLanguage.Value))
        {
            throw new InvalidOperationException($"Unsupported meaning language code '{primaryMeaningLanguageCode}'.");
        }

        if (!string.IsNullOrWhiteSpace(secondaryMeaningLanguageCode))
        {
            LanguageCode secondaryMeaningLanguage = LanguageCode.From(secondaryMeaningLanguageCode);

            if (!SupportedMeaningLanguageCodes.Contains(secondaryMeaningLanguage.Value))
            {
                throw new InvalidOperationException($"Unsupported secondary meaning language code '{secondaryMeaningLanguageCode}'.");
            }
        }

        string normalizedAdultState = NormalizeAdultContentAccessState(adultContentAccessState);
        if (normalizedAdultState is not AdultContentAccessStates.NotRequested and not AdultContentAccessStates.SelfDeclaredAdult)
        {
            throw new InvalidOperationException("Only not-requested and self-declared-adult adult-content states can be set from learner settings.");
        }
    }

    private static UserLearningProfileModel Map(string actorId, WebUserPreference preference) =>
        new(
            actorId,
            preference.PrimaryMeaningLanguageCode,
            preference.SecondaryMeaningLanguageCode,
            preference.UiLanguageCode,
            preference.AllowsRudeSlangContent,
            NormalizeAdultContentAccessState(preference.AdultContentAccessState));

    private static UserLearningProfileModel CreateDefaultProfile(string actorId) =>
        new(
            actorId,
            "en",
            null,
            "en",
            false,
            AdultContentAccessStates.NotRequested);

    private static string ResolveUserWritableAdultAccessState(string requestedState, string currentState)
    {
        string normalizedCurrent = NormalizeAdultContentAccessState(currentState);
        if (normalizedCurrent is AdultContentAccessStates.AgeVerifiedAdult or AdultContentAccessStates.Blocked)
        {
            return normalizedCurrent;
        }

        return NormalizeAdultContentAccessState(requestedState);
    }

    private static string NormalizeAdultContentAccessState(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            AdultContentAccessStates.SelfDeclaredAdult => AdultContentAccessStates.SelfDeclaredAdult,
            AdultContentAccessStates.AgeVerifiedAdult => AdultContentAccessStates.AgeVerifiedAdult,
            AdultContentAccessStates.Blocked => AdultContentAccessStates.Blocked,
            _ => AdultContentAccessStates.NotRequested,
        };
}

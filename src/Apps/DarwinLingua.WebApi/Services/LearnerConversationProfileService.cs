using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Lexicon;
using DarwinLingua.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

public sealed class LearnerConversationProfileService(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory)
    : ILearnerConversationProfileService
{
    public async Task<LearnerConversationProfilePrivateResponse?> GetPrivateAsync(
        string ownerEmail,
        CancellationToken cancellationToken)
    {
        string normalizedEmail = LearnerConversationProfile.NormalizeEmail(ownerEmail);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        LearnerConversationProfile? profile = await dbContext.LearnerConversationProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.OwnerEmail == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);

        return profile is null ? null : ToPrivateResponse(profile);
    }

    public async Task<IReadOnlyList<LearnerConversationProfilePublicResponse>> GetPublicProfilesAsync(
        CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        LearnerConversationProfile[] profiles = await dbContext.LearnerConversationProfiles
            .AsNoTracking()
            .Where(item => item.Visibility == "public" && item.HasConfirmedAdult)
            .OrderBy(item => item.CityRegion)
            .ThenBy(item => item.DisplayName)
            .Take(100)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return profiles.Select(ToPublicResponse).ToArray();
    }

    public async Task<LearnerConversationProfilePrivateResponse> SaveAsync(
        string ownerEmail,
        SaveLearnerConversationProfileRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await SaveCoreAsync(ownerEmail, request, cancellationToken).ConfigureAwait(false);
        }
        catch (DomainRuleException exception)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    public async Task<LearnerConversationProfilePrivateResponse> SetEnabledAsync(
        string ownerEmail,
        LearnerConversationProfileVisibilityRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            string normalizedEmail = LearnerConversationProfile.NormalizeEmail(ownerEmail);

            await using DarwinLinguaDbContext dbContext = await dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            LearnerConversationProfile profile = await dbContext.LearnerConversationProfiles
                .SingleOrDefaultAsync(item => item.OwnerEmail == normalizedEmail, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new KeyNotFoundException("No learner conversation profile was found for the current learner.");

            profile.SetVisibility(request.IsEnabled ? "private" : "disabled", DateTime.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return ToPrivateResponse(profile);
        }
        catch (DomainRuleException exception)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    public async Task AnonymizeAsync(
        string ownerEmail,
        CancellationToken cancellationToken)
    {
        try
        {
            string normalizedEmail = LearnerConversationProfile.NormalizeEmail(ownerEmail);

            await using DarwinLinguaDbContext dbContext = await dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            LearnerConversationProfile? profile = await dbContext.LearnerConversationProfiles
                .SingleOrDefaultAsync(item => item.OwnerEmail == normalizedEmail, cancellationToken)
                .ConfigureAwait(false);

            if (profile is null)
            {
                return;
            }

            profile.Anonymize(DateTime.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DomainRuleException exception)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    private async Task<LearnerConversationProfilePrivateResponse> SaveCoreAsync(
        string ownerEmail,
        SaveLearnerConversationProfileRequest request,
        CancellationToken cancellationToken)
    {
        string normalizedEmail = LearnerConversationProfile.NormalizeEmail(ownerEmail);
        string learningLevel = NormalizeCefrLevel(request.LearningLevel);
        string helperLanguageCodes = NormalizeLanguageCodes(request.HelperLanguageCodes);
        ValidateSafetyBoundary(request.Visibility, request.HasConfirmedAdult);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        DateTime nowUtc = DateTime.UtcNow;
        LearnerConversationProfile? profile = await dbContext.LearnerConversationProfiles
            .SingleOrDefaultAsync(item => item.OwnerEmail == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);

        if (profile is null)
        {
            profile = new LearnerConversationProfile(
                Guid.NewGuid(),
                normalizedEmail,
                request.DisplayName,
                request.CityRegion,
                request.InteractionPreference,
                learningLevel,
                helperLanguageCodes,
                request.ConversationGoals,
                request.AvailabilityNotes,
                request.Visibility,
                request.HasConfirmedAdult,
                nowUtc);

            dbContext.LearnerConversationProfiles.Add(profile);
        }
        else
        {
            profile.Update(
                request.DisplayName,
                request.CityRegion,
                request.InteractionPreference,
                learningLevel,
                helperLanguageCodes,
                request.ConversationGoals,
                request.AvailabilityNotes,
                request.Visibility,
                request.HasConfirmedAdult,
                nowUtc);
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ToPrivateResponse(profile);
    }

    private static string NormalizeCefrLevel(string value)
    {
        if (!Enum.TryParse(value, true, out CefrLevel level))
        {
            throw new InvalidOperationException($"'{value}' is not a supported CEFR level.");
        }

        return level.ToString().ToUpperInvariant();
    }

    private static string NormalizeLanguageCodes(IReadOnlyList<string> values)
    {
        string[] normalizedValues = values
            .Select(value => ConversationEvent.NormalizeKey(value, "Learner helper language code"))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalizedValues.Length == 0)
        {
            throw new InvalidOperationException("At least one helper language code is required.");
        }

        return string.Join(',', normalizedValues);
    }

    private static void ValidateSafetyBoundary(string visibility, bool hasConfirmedAdult)
    {
        string normalizedVisibility = ConversationEvent.NormalizeKey(visibility, "Learner profile visibility");
        if (!LearnerConversationProfileTaxonomy.VisibilityStates.Contains(normalizedVisibility))
        {
            throw new InvalidOperationException("Learner profile visibility is not supported.");
        }

        if (!hasConfirmedAdult && normalizedVisibility is "public" or "request-only")
        {
            throw new InvalidOperationException("Adult confirmation is required before a learner profile can be public or request-only.");
        }
    }

    private static LearnerConversationProfilePrivateResponse ToPrivateResponse(LearnerConversationProfile profile) =>
        new(
            profile.Id,
            profile.OwnerEmail,
            profile.DisplayName,
            profile.CityRegion,
            profile.InteractionPreference,
            profile.LearningLevel,
            SplitLanguageCodes(profile.HelperLanguageCodes),
            profile.ConversationGoals,
            profile.AvailabilityNotes,
            profile.Visibility,
            profile.HasConfirmedAdult,
            profile.CreatedAtUtc,
            profile.UpdatedAtUtc);

    private static LearnerConversationProfilePublicResponse ToPublicResponse(LearnerConversationProfile profile) =>
        new(
            profile.DisplayName,
            profile.CityRegion,
            profile.InteractionPreference,
            profile.LearningLevel,
            SplitLanguageCodes(profile.HelperLanguageCodes),
            profile.ConversationGoals);

    private static IReadOnlyList<string> SplitLanguageCodes(string value) =>
        value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
}

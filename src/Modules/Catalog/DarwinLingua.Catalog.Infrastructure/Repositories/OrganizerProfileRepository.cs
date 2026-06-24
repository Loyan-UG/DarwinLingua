using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class OrganizerProfileRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IOrganizerProfileRepository
{
    public async Task<IReadOnlyList<OrganizerProfileListItemModel>> GetPublishedOrganizerProfilesAsync(
        string targetLearningLanguageCode,
        CancellationToken cancellationToken)
    {
        string normalizedTargetLearningLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        List<OrganizerProfile> profiles = await dbContext.OrganizerProfiles
            .AsNoTracking()
            .AsSplitQuery()
            .Include(profile => profile.SupportedLevels)
            .Include(profile => profile.HelperLanguages)
            .Where(profile => profile.PublicationStatus == PublicationStatus.Active)
            .OrderBy(profile => profile.DisplayName)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<string, int> activeEventCounts = await GetActiveEventCountsAsync(
                dbContext,
                profiles.Select(profile => profile.Slug),
                normalizedTargetLearningLanguageCode,
                cancellationToken)
            .ConfigureAwait(false);

        return profiles
            .Where(profile => HasTargetLanguageSupport(profile, normalizedTargetLearningLanguageCode) ||
                activeEventCounts.GetValueOrDefault(profile.Slug) > 0)
            .Select(profile => CreateListItem(profile, normalizedTargetLearningLanguageCode, activeEventCounts.GetValueOrDefault(profile.Slug)))
            .ToArray();
    }

    public async Task<OrganizerProfileDetailModel?> GetPublishedOrganizerProfileBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        string normalizedSlug = slug.Trim().ToLowerInvariant();
        string normalizedTargetLearningLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        OrganizerProfile? profile = await dbContext.OrganizerProfiles
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.SupportedLevels)
            .Include(item => item.HelperLanguages)
            .Where(item => item.PublicationStatus == PublicationStatus.Active && item.Slug == normalizedSlug)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (profile is null)
        {
            return null;
        }

        ConversationEvent[] activeEvents = await dbContext.ConversationEvents
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.SupportedLevels)
            .Include(item => item.HelperLanguages)
            .Include(item => item.PreparationPackLinks)
            .Where(item =>
                item.PublicationStatus == PublicationStatus.Active &&
                item.TargetLearningLanguageCode == normalizedTargetLearningLanguageCode &&
                item.OrganizerProfileSlug == normalizedSlug)
            .OrderBy(item => item.StartsAtUtc ?? DateTime.MaxValue)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.Name)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new OrganizerProfileDetailModel(
            profile.Slug,
            normalizedTargetLearningLanguageCode,
            profile.DisplayName,
            profile.OrganizerType,
            profile.Description,
            profile.CityRegion,
            profile.IsOnlineAvailable,
            ResolveLevels(profile, normalizedTargetLearningLanguageCode),
            ResolveHelperLanguages(profile),
            profile.WebsiteUrl,
            profile.PublicContactMethod,
            profile.VerificationStatus,
            profile.PlanKey,
            profile.HistoricalEventCount,
            activeEvents.Select(CreateEventListItem).ToArray());
    }

    private static async Task<Dictionary<string, int>> GetActiveEventCountsAsync(
        DarwinLinguaDbContext dbContext,
        IEnumerable<string> profileSlugs,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken)
    {
        string[] slugs = profileSlugs.ToArray();
        if (slugs.Length == 0)
        {
            return [];
        }

        return await dbContext.ConversationEvents
            .AsNoTracking()
            .Where(item => item.PublicationStatus == PublicationStatus.Active &&
                item.TargetLearningLanguageCode == targetLearningLanguageCode &&
                item.OrganizerProfileSlug != null &&
                slugs.Contains(item.OrganizerProfileSlug))
            .GroupBy(item => item.OrganizerProfileSlug!)
            .Select(group => new { Slug = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Slug, item => item.Count, cancellationToken)
            .ConfigureAwait(false);
    }

    private static OrganizerProfileListItemModel CreateListItem(
        OrganizerProfile profile,
        string targetLearningLanguageCode,
        int activeEventCount) =>
        new(
            profile.Slug,
            targetLearningLanguageCode,
            profile.DisplayName,
            profile.OrganizerType,
            profile.Description,
            profile.CityRegion,
            profile.IsOnlineAvailable,
            ResolveLevels(profile, targetLearningLanguageCode),
            ResolveHelperLanguages(profile),
            profile.VerificationStatus,
            profile.PlanKey,
            profile.HistoricalEventCount,
            activeEventCount);

    private static ConversationEventListItemModel CreateEventListItem(ConversationEvent conversationEvent) =>
        new(
            conversationEvent.Slug,
            conversationEvent.TargetLearningLanguageCode,
            conversationEvent.Name,
            conversationEvent.Description,
            conversationEvent.City,
            conversationEvent.CountryRegion,
            conversationEvent.IsOnline,
            conversationEvent.Category,
            conversationEvent.SupportedLevels.OrderBy(level => level.SortOrder).Select(level => level.CefrLevel.ToString()).ToArray(),
            conversationEvent.HelperLanguages.OrderBy(language => language.SortOrder).Select(language => language.LanguageCode).ToArray(),
            conversationEvent.OrganizerName,
            conversationEvent.OrganizerProfileSlug,
            conversationEvent.ScheduleText,
            conversationEvent.StartsAtUtc,
            conversationEvent.EndsAtUtc,
            conversationEvent.PriceType,
            conversationEvent.VerificationStatus,
            conversationEvent.PreparationPackLinks.OrderBy(link => link.SortOrder).Select(link => link.PreparationPackSlug).ToArray());

    private static string[] ResolveLevels(OrganizerProfile profile, string targetLearningLanguageCode) =>
        profile.SupportedLevels
            .Where(level => level.TargetLearningLanguageCode == targetLearningLanguageCode)
            .OrderBy(level => level.SortOrder)
            .Select(level => level.CefrLevel.ToString())
            .ToArray();

    private static bool HasTargetLanguageSupport(OrganizerProfile profile, string targetLearningLanguageCode) =>
        profile.SupportedLevels.Any(level => level.TargetLearningLanguageCode == targetLearningLanguageCode);

    private static string[] ResolveHelperLanguages(OrganizerProfile profile) =>
        profile.HelperLanguages
            .OrderBy(language => language.SortOrder)
            .Select(language => language.LanguageCode)
            .ToArray();
}

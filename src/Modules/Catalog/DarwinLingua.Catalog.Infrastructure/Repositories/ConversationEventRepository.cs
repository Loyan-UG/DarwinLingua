using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class ConversationEventRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IConversationEventRepository
{
    public async Task<IReadOnlyList<ConversationEventListItemModel>> GetPublishedEventsAsync(
        ConversationEventListFilterModel filter,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        IQueryable<ConversationEvent> query = dbContext.ConversationEvents
            .AsNoTracking()
            .AsSplitQuery()
            .Include(conversationEvent => conversationEvent.SupportedLevels)
            .Include(conversationEvent => conversationEvent.HelperLanguages)
            .Include(conversationEvent => conversationEvent.PreparationPackLinks)
            .Where(conversationEvent => conversationEvent.PublicationStatus == PublicationStatus.Active);

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            string city = filter.City.Trim();
            query = query.Where(conversationEvent => conversationEvent.City != null && conversationEvent.City.ToLower() == city.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            string category = filter.Category.Trim().ToLowerInvariant();
            query = query.Where(conversationEvent => conversationEvent.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(filter.PriceType))
        {
            string priceType = filter.PriceType.Trim().ToLowerInvariant();
            query = query.Where(conversationEvent => conversationEvent.PriceType == priceType);
        }

        if (filter.IsOnline.HasValue)
        {
            query = query.Where(conversationEvent => conversationEvent.IsOnline == filter.IsOnline.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.CefrLevel) &&
            Enum.TryParse(filter.CefrLevel.Trim(), true, out CefrLevel cefrLevel))
        {
            query = query.Where(conversationEvent => conversationEvent.SupportedLevels.Any(level => level.CefrLevel == cefrLevel));
        }

        if (!string.IsNullOrWhiteSpace(filter.HelperLanguageCode))
        {
            string helperLanguageCode = filter.HelperLanguageCode.Trim().ToLowerInvariant();
            query = query.Where(conversationEvent => conversationEvent.HelperLanguages.Any(language => language.LanguageCode == helperLanguageCode));
        }

        List<ConversationEvent> events = await query
            .OrderBy(conversationEvent => conversationEvent.SortOrder)
            .ThenBy(conversationEvent => conversationEvent.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return events.Select(CreateListItem).ToArray();
    }

    public async Task<ConversationEventDetailModel?> GetPublishedEventBySlugAsync(
        string slug,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        string normalizedSlug = slug.Trim().ToLowerInvariant();

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        ConversationEvent? conversationEvent = await dbContext.ConversationEvents
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.SupportedLevels)
            .Include(item => item.HelperLanguages)
            .Include(item => item.PreparationPackLinks)
            .Where(item => item.PublicationStatus == PublicationStatus.Active && item.Slug == normalizedSlug)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return conversationEvent is null
            ? null
            : new ConversationEventDetailModel(
                conversationEvent.Slug,
                conversationEvent.Name,
                conversationEvent.Description,
                conversationEvent.City,
                conversationEvent.CountryRegion,
                conversationEvent.ApproximateLocation,
                conversationEvent.IsOnline,
                conversationEvent.Category,
                ResolveLevels(conversationEvent),
                ResolveHelperLanguages(conversationEvent),
                conversationEvent.OrganizerName,
                conversationEvent.OrganizerProfileSlug,
                conversationEvent.ExternalLink,
                conversationEvent.ContactMethod,
                conversationEvent.ScheduleText,
                conversationEvent.PriceType,
                conversationEvent.VerificationStatus,
                conversationEvent.SourceName,
                conversationEvent.SourceUrl,
                conversationEvent.LastVerifiedAtUtc,
                ResolvePreparationPackSlugs(conversationEvent));
    }

    private static ConversationEventListItemModel CreateListItem(ConversationEvent conversationEvent) =>
        new(
            conversationEvent.Slug,
            conversationEvent.Name,
            conversationEvent.Description,
            conversationEvent.City,
            conversationEvent.CountryRegion,
            conversationEvent.IsOnline,
            conversationEvent.Category,
            ResolveLevels(conversationEvent),
            ResolveHelperLanguages(conversationEvent),
            conversationEvent.OrganizerName,
            conversationEvent.OrganizerProfileSlug,
            conversationEvent.ScheduleText,
            conversationEvent.PriceType,
            conversationEvent.VerificationStatus,
            ResolvePreparationPackSlugs(conversationEvent));

    private static string[] ResolveLevels(ConversationEvent conversationEvent) =>
        conversationEvent.SupportedLevels
            .OrderBy(level => level.SortOrder)
            .Select(level => level.CefrLevel.ToString())
            .ToArray();

    private static string[] ResolveHelperLanguages(ConversationEvent conversationEvent) =>
        conversationEvent.HelperLanguages
            .OrderBy(language => language.SortOrder)
            .Select(language => language.LanguageCode)
            .ToArray();

    private static string[] ResolvePreparationPackSlugs(ConversationEvent conversationEvent) =>
        conversationEvent.PreparationPackLinks
            .OrderBy(link => link.SortOrder)
            .Select(link => link.PreparationPackSlug)
            .ToArray();
}

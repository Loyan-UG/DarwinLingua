using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using DarwinLingua.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

public sealed class ConversationEventAdminService(
    IDbContextFactory<DarwinLinguaDbContext> dbContextFactory,
    IConversationEventQueryService conversationEventQueryService) : IConversationEventAdminService
{
    private static readonly TimeSpan FreshVerificationWindow = TimeSpan.FromDays(180);

    public async Task<ConversationEventDetailModel> SaveAsync(
        AdminSaveConversationEventRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await SaveCoreAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (DomainRuleException exception)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    public async Task<IReadOnlyList<OrganizerManagedConversationEventModel>> GetByOrganizerProfileSlugAsync(
        string organizerProfileSlug,
        CancellationToken cancellationToken)
    {
        string normalizedSlug = ConversationEvent.NormalizeKey(organizerProfileSlug, "Organizer profile slug");

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        ConversationEvent[] events = await dbContext.ConversationEvents
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.SupportedLevels)
            .Include(item => item.HelperLanguages)
            .Include(item => item.PreparationPackLinks)
            .Where(item => item.OrganizerProfileSlug == normalizedSlug)
            .OrderBy(item => item.PublicationStatus)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.Name)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return events.Select(CreateManagedModel).ToArray();
    }

    public async Task<OrganizerManagedConversationEventModel> SetPublicationStatusAsync(
        string slug,
        AdminSetConversationEventPublicationStatusRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentNullException.ThrowIfNull(request);

        if (!Enum.TryParse(request.PublicationStatus, true, out PublicationStatus publicationStatus))
        {
            throw new InvalidOperationException($"'{request.PublicationStatus}' is not a supported publication status.");
        }

        string normalizedSlug = ConversationEvent.NormalizeKey(slug, "Conversation event slug");

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        ConversationEvent conversationEvent = await dbContext.ConversationEvents
            .Include(item => item.SupportedLevels)
            .Include(item => item.HelperLanguages)
            .Include(item => item.PreparationPackLinks)
            .SingleOrDefaultAsync(item => item.Slug == normalizedSlug, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"No conversation event was found for '{normalizedSlug}'.");

        conversationEvent.SetPublicationStatus(publicationStatus, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return CreateManagedModel(conversationEvent);
    }

    private async Task<ConversationEventDetailModel> SaveCoreAsync(
        AdminSaveConversationEventRequest request,
        CancellationToken cancellationToken)
    {
        DateTime nowUtc = DateTime.UtcNow;
        string targetLearningLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(request.TargetLearningLanguageCode);
        ValidateVerificationFreshness(request.VerificationStatus, request.LastVerifiedAtUtc, nowUtc);
        CefrLevel[] levels = ParseLevels(request.SupportedLearnerLevels);
        string[] helperLanguageCodes = NormalizeKeys(request.HelperLanguageCodes, "helper language code");
        string[] preparationPackSlugs = NormalizeKeys(request.LinkedEventPreparationPackSlugs, "preparation pack slug");

        ConversationEvent conversationEvent = new(
            Guid.NewGuid(),
            request.Slug,
            request.Name,
            request.Description,
            request.City,
            request.CountryRegion,
            request.ApproximateLocation,
            request.IsOnline,
            request.Category,
            request.OrganizerName,
            request.OrganizerProfileSlug,
            request.ExternalLink,
            request.ContactMethod,
            request.ScheduleText,
            request.PriceType,
            request.VerificationStatus,
            PublicationStatus.Active,
            0,
            nowUtc,
            targetLearningLanguageCode: targetLearningLanguageCode);

        conversationEvent.SetSourceMetadata(request.SourceName, request.SourceUrl, request.LastVerifiedAtUtc);
        conversationEvent.SetOperationalDetails(request.RecurrenceRule, request.Capacity, nowUtc, request.StartsAtUtc, request.EndsAtUtc);

        for (int index = 0; index < levels.Length; index++)
        {
            conversationEvent.AddSupportedLevel(Guid.NewGuid(), levels[index], index + 1, nowUtc);
        }

        for (int index = 0; index < helperLanguageCodes.Length; index++)
        {
            conversationEvent.AddHelperLanguage(Guid.NewGuid(), helperLanguageCodes[index], index + 1, nowUtc);
        }

        for (int index = 0; index < preparationPackSlugs.Length; index++)
        {
            conversationEvent.AddPreparationPackLink(Guid.NewGuid(), preparationPackSlugs[index], index + 1, nowUtc);
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        string normalizedSlug = conversationEvent.Slug;
        ConversationEvent? existingEvent = await dbContext.ConversationEvents
            .Include(item => item.SupportedLevels)
            .Include(item => item.HelperLanguages)
            .Include(item => item.PreparationPackLinks)
            .SingleOrDefaultAsync(
                item => item.TargetLearningLanguageCode == targetLearningLanguageCode && item.Slug == normalizedSlug,
                cancellationToken)
            .ConfigureAwait(false);

        if (existingEvent is not null)
        {
            dbContext.ConversationEvents.Remove(existingEvent);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        dbContext.ConversationEvents.Add(conversationEvent);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await conversationEventQueryService
            .GetPublishedEventBySlugAsync(normalizedSlug, targetLearningLanguageCode, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("The saved conversation event could not be loaded.");
    }

    private static CefrLevel[] ParseLevels(IReadOnlyList<string> values)
    {
        if (values.Count == 0)
        {
            throw new InvalidOperationException("At least one learner level is required.");
        }

        List<CefrLevel> levels = [];
        foreach (string value in values.Select(value => value.Trim()).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!Enum.TryParse(value, true, out CefrLevel level))
            {
                throw new InvalidOperationException($"'{value}' is not a supported CEFR level.");
            }

            levels.Add(level);
        }

        return levels.Count == 0
            ? throw new InvalidOperationException("At least one learner level is required.")
            : levels.ToArray();
    }

    private static string[] NormalizeKeys(IReadOnlyList<string> values, string label) =>
        values
            .Select(value => ConversationEvent.NormalizeKey(value, label))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static OrganizerManagedConversationEventModel CreateManagedModel(ConversationEvent conversationEvent) =>
        new(
            conversationEvent.Slug,
            conversationEvent.TargetLearningLanguageCode,
            conversationEvent.Name,
            conversationEvent.Description,
            conversationEvent.City,
            conversationEvent.CountryRegion,
            conversationEvent.ApproximateLocation,
            conversationEvent.IsOnline,
            conversationEvent.Category,
            conversationEvent.SupportedLevels.OrderBy(level => level.SortOrder).Select(level => level.CefrLevel.ToString()).ToArray(),
            conversationEvent.HelperLanguages.OrderBy(language => language.SortOrder).Select(language => language.LanguageCode).ToArray(),
            conversationEvent.OrganizerName,
            conversationEvent.OrganizerProfileSlug,
            conversationEvent.ExternalLink,
            conversationEvent.ContactMethod,
            conversationEvent.ScheduleText,
            conversationEvent.StartsAtUtc,
            conversationEvent.EndsAtUtc,
            conversationEvent.PriceType,
            conversationEvent.VerificationStatus,
            conversationEvent.PublicationStatus.ToString(),
            conversationEvent.SourceName,
            conversationEvent.SourceUrl,
            conversationEvent.LastVerifiedAtUtc,
            conversationEvent.PreparationPackLinks.OrderBy(link => link.SortOrder).Select(link => link.PreparationPackSlug).ToArray())
        {
            RecurrenceRule = conversationEvent.RecurrenceRule,
            Capacity = conversationEvent.Capacity,
        };

    private static void ValidateVerificationFreshness(
        string verificationStatus,
        DateTime? lastVerifiedAtUtc,
        DateTime nowUtc)
    {
        string normalizedStatus = verificationStatus.Trim().ToLowerInvariant();
        if (normalizedStatus is not ("reviewed" or "verified"))
        {
            return;
        }

        if (lastVerifiedAtUtc is null)
        {
            throw new InvalidOperationException("Reviewed or verified events require a last verified UTC value.");
        }

        DateTime normalizedLastVerifiedAtUtc = lastVerifiedAtUtc.Value.Kind == DateTimeKind.Utc
            ? lastVerifiedAtUtc.Value
            : lastVerifiedAtUtc.Value.ToUniversalTime();

        if (normalizedLastVerifiedAtUtc > nowUtc.AddMinutes(5))
        {
            throw new InvalidOperationException("Last verified UTC cannot be in the future.");
        }

        if (normalizedLastVerifiedAtUtc < nowUtc.Subtract(FreshVerificationWindow))
        {
            throw new InvalidOperationException("Reviewed or verified events must be rechecked within the last 180 days.");
        }
    }
}

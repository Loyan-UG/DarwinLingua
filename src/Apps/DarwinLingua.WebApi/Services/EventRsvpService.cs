using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

public sealed class EventRsvpService(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IEventRsvpService
{
    public async Task<EventRsvpResponse> SubmitAsync(
        string eventSlug,
        string targetLearningLanguageCode,
        SubmitEventRsvpRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventSlug);
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            await using DarwinLinguaDbContext dbContext = await dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            string normalizedEventSlug = ConversationEvent.NormalizeKey(eventSlug, "Conversation event slug");
            string normalizedTargetLearningLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode);
            bool eventExists = await dbContext.ConversationEvents
                .AsNoTracking()
                .AnyAsync(
                    item =>
                        item.TargetLearningLanguageCode == normalizedTargetLearningLanguageCode &&
                        item.Slug == normalizedEventSlug &&
                        item.PublicationStatus == PublicationStatus.Active,
                    cancellationToken)
                .ConfigureAwait(false);

            if (!eventExists)
            {
                throw new KeyNotFoundException($"No active conversation event was found for '{normalizedEventSlug}'.");
            }

            string participantEmail = EventRsvp.NormalizeEmail(request.ParticipantEmail);
            EventRsvp? existingRsvp = await dbContext.EventRsvps
                .SingleOrDefaultAsync(
                    item =>
                        item.TargetLearningLanguageCode == normalizedTargetLearningLanguageCode &&
                        item.ConversationEventSlug == normalizedEventSlug &&
                        item.ParticipantEmail == participantEmail,
                    cancellationToken)
                .ConfigureAwait(false);

            DateTime nowUtc = DateTime.UtcNow;
            await EnsureCapacityAllowsStatusChangeAsync(
                    dbContext,
                    normalizedEventSlug,
                    normalizedTargetLearningLanguageCode,
                    existingRsvp?.Id,
                    request.Status,
                    cancellationToken)
                .ConfigureAwait(false);

            if (existingRsvp is null)
            {
                existingRsvp = new EventRsvp(
                    Guid.NewGuid(),
                    normalizedEventSlug,
                    normalizedTargetLearningLanguageCode,
                    request.ParticipantName,
                    participantEmail,
                    request.Status,
                    nowUtc);

                dbContext.EventRsvps.Add(existingRsvp);
            }
            else
            {
                existingRsvp.Update(request.ParticipantName, request.Status, nowUtc);
            }

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return CreateResponse(existingRsvp);
        }
        catch (DomainRuleException exception)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    public async Task<EventRsvpSummaryResponse> GetSummaryAsync(
        string eventSlug,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken)
    {
        string normalizedEventSlug = ConversationEvent.NormalizeKey(eventSlug, "Conversation event slug");
        string normalizedTargetLearningLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        ConversationEvent? conversationEvent = await dbContext.ConversationEvents
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.TargetLearningLanguageCode == normalizedTargetLearningLanguageCode && item.Slug == normalizedEventSlug,
                cancellationToken)
            .ConfigureAwait(false);

        if (conversationEvent is null)
        {
            throw new KeyNotFoundException($"No conversation event was found for '{normalizedEventSlug}'.");
        }

        EventRsvpStatusCount[] statusCounts = await dbContext.EventRsvps
            .AsNoTracking()
            .Where(item =>
                item.TargetLearningLanguageCode == normalizedTargetLearningLanguageCode &&
                item.ConversationEventSlug == normalizedEventSlug)
            .GroupBy(item => item.Status)
            .Select(group => new EventRsvpStatusCount(group.Key, group.Count()))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        int goingCount = GetStatusCount(statusCounts, EventRsvpStatuses.Going);
        int capacity = conversationEvent.Capacity ?? 0;
        int? remainingCapacity = capacity > 0
            ? Math.Max(0, capacity - goingCount)
            : null;

        return new EventRsvpSummaryResponse(
            normalizedEventSlug,
            normalizedTargetLearningLanguageCode,
            GetStatusCount(statusCounts, EventRsvpStatuses.Interested),
            goingCount,
            GetStatusCount(statusCounts, EventRsvpStatuses.Cancelled),
            capacity,
            remainingCapacity);
    }

    public async Task<IReadOnlyList<EventRsvpResponse>> GetByEventAsync(
        string eventSlug,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken)
    {
        string normalizedEventSlug = ConversationEvent.NormalizeKey(eventSlug, "Conversation event slug");
        string normalizedTargetLearningLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        EventRsvp[] rsvps = await dbContext.EventRsvps
            .AsNoTracking()
            .Where(item =>
                item.TargetLearningLanguageCode == normalizedTargetLearningLanguageCode &&
                item.ConversationEventSlug == normalizedEventSlug)
            .OrderBy(item => item.Status)
            .ThenBy(item => item.ParticipantName)
            .Take(500)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return rsvps.Select(CreateResponse).ToArray();
    }

    public async Task<EventRsvpResponse> SetStatusAsync(
        string eventSlug,
        string targetLearningLanguageCode,
        Guid rsvpId,
        AdminSetEventRsvpStatusRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventSlug);
        ArgumentNullException.ThrowIfNull(request);
        if (rsvpId == Guid.Empty)
        {
            throw new InvalidOperationException("Event RSVP identifier cannot be empty.");
        }

        try
        {
            string normalizedEventSlug = ConversationEvent.NormalizeKey(eventSlug, "Conversation event slug");
            string normalizedTargetLearningLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode);

            await using DarwinLinguaDbContext dbContext = await dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            EventRsvp rsvp = await dbContext.EventRsvps
                .SingleOrDefaultAsync(
                    item =>
                        item.Id == rsvpId &&
                        item.TargetLearningLanguageCode == normalizedTargetLearningLanguageCode &&
                        item.ConversationEventSlug == normalizedEventSlug,
                    cancellationToken)
                .ConfigureAwait(false)
                ?? throw new KeyNotFoundException($"No RSVP was found for event '{normalizedEventSlug}'.");

            await EnsureCapacityAllowsStatusChangeAsync(
                    dbContext,
                    normalizedEventSlug,
                    normalizedTargetLearningLanguageCode,
                    rsvp.Id,
                    request.Status,
                    cancellationToken)
                .ConfigureAwait(false);

            rsvp.Update(rsvp.ParticipantName, request.Status, DateTime.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return CreateResponse(rsvp);
        }
        catch (DomainRuleException exception)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    private static EventRsvpResponse CreateResponse(EventRsvp rsvp) =>
        new(
            rsvp.Id,
            rsvp.ConversationEventSlug,
            rsvp.TargetLearningLanguageCode,
            rsvp.ParticipantName,
            rsvp.ParticipantEmail,
            rsvp.Status,
            rsvp.CreatedAtUtc,
            rsvp.UpdatedAtUtc);

    private static int GetStatusCount(EventRsvpStatusCount[] statusCounts, string status) =>
        statusCounts.FirstOrDefault(item => item.Status == status)?.Count ?? 0;

    private static async Task EnsureCapacityAllowsStatusChangeAsync(
        DarwinLinguaDbContext dbContext,
        string normalizedEventSlug,
        string normalizedTargetLearningLanguageCode,
        Guid? currentRsvpId,
        string requestedStatus,
        CancellationToken cancellationToken)
    {
        string normalizedStatus = EventRsvp.NormalizeStatus(requestedStatus);
        if (normalizedStatus != EventRsvpStatuses.Going)
        {
            return;
        }

        int? capacity = await dbContext.ConversationEvents
            .AsNoTracking()
            .Where(item =>
                item.TargetLearningLanguageCode == normalizedTargetLearningLanguageCode &&
                item.Slug == normalizedEventSlug)
            .Select(item => item.Capacity)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (capacity is null or <= 0)
        {
            return;
        }

        int currentGoingCount = await dbContext.EventRsvps
            .AsNoTracking()
            .CountAsync(
                item => item.ConversationEventSlug == normalizedEventSlug &&
                    item.TargetLearningLanguageCode == normalizedTargetLearningLanguageCode &&
                    item.Status == EventRsvpStatuses.Going &&
                    (!currentRsvpId.HasValue || item.Id != currentRsvpId.Value),
                cancellationToken)
            .ConfigureAwait(false);

        if (currentGoingCount >= capacity.Value)
        {
            throw new InvalidOperationException("This event has reached its RSVP capacity.");
        }
    }

    private sealed record EventRsvpStatusCount(string Status, int Count);
}

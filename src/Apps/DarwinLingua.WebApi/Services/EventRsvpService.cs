using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

public sealed class EventRsvpService(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IEventRsvpService
{
    public async Task<EventRsvpResponse> SubmitAsync(
        string eventSlug,
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
            bool eventExists = await dbContext.ConversationEvents
                .AsNoTracking()
                .AnyAsync(item => item.Slug == normalizedEventSlug && item.PublicationStatus == PublicationStatus.Active, cancellationToken)
                .ConfigureAwait(false);

            if (!eventExists)
            {
                throw new KeyNotFoundException($"No active conversation event was found for '{normalizedEventSlug}'.");
            }

            string participantEmail = EventRsvp.NormalizeEmail(request.ParticipantEmail);
            EventRsvp? existingRsvp = await dbContext.EventRsvps
                .SingleOrDefaultAsync(
                    item => item.ConversationEventSlug == normalizedEventSlug && item.ParticipantEmail == participantEmail,
                    cancellationToken)
                .ConfigureAwait(false);

            DateTime nowUtc = DateTime.UtcNow;
            if (existingRsvp is null)
            {
                existingRsvp = new EventRsvp(
                    Guid.NewGuid(),
                    normalizedEventSlug,
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
        CancellationToken cancellationToken)
    {
        string normalizedEventSlug = ConversationEvent.NormalizeKey(eventSlug, "Conversation event slug");

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        ConversationEvent? conversationEvent = await dbContext.ConversationEvents
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Slug == normalizedEventSlug, cancellationToken)
            .ConfigureAwait(false);

        if (conversationEvent is null)
        {
            throw new KeyNotFoundException($"No conversation event was found for '{normalizedEventSlug}'.");
        }

        EventRsvp[] rsvps = await dbContext.EventRsvps
            .AsNoTracking()
            .Where(item => item.ConversationEventSlug == normalizedEventSlug)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        int goingCount = rsvps.Count(item => item.Status == EventRsvpStatuses.Going);
        int capacity = conversationEvent.Capacity ?? 0;
        int? remainingCapacity = capacity > 0
            ? Math.Max(0, capacity - goingCount)
            : null;

        return new EventRsvpSummaryResponse(
            normalizedEventSlug,
            rsvps.Count(item => item.Status == EventRsvpStatuses.Interested),
            goingCount,
            rsvps.Count(item => item.Status == EventRsvpStatuses.Cancelled),
            capacity,
            remainingCapacity);
    }

    public async Task<IReadOnlyList<EventRsvpResponse>> GetByEventAsync(
        string eventSlug,
        CancellationToken cancellationToken)
    {
        string normalizedEventSlug = ConversationEvent.NormalizeKey(eventSlug, "Conversation event slug");

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        EventRsvp[] rsvps = await dbContext.EventRsvps
            .AsNoTracking()
            .Where(item => item.ConversationEventSlug == normalizedEventSlug)
            .OrderBy(item => item.Status)
            .ThenBy(item => item.ParticipantName)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return rsvps.Select(CreateResponse).ToArray();
    }

    private static EventRsvpResponse CreateResponse(EventRsvp rsvp) =>
        new(
            rsvp.Id,
            rsvp.ConversationEventSlug,
            rsvp.ParticipantName,
            rsvp.ParticipantEmail,
            rsvp.Status,
            rsvp.CreatedAtUtc,
            rsvp.UpdatedAtUtc);
}

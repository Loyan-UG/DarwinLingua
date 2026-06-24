using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class EventRsvpServiceTests
{
    [Fact]
    public async Task SubmitAsync_ShouldCreateAndUpdateOneRsvpPerParticipantEmail()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_event_rsvp");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);
            await SeedEventAsync(serviceProvider, "berlin-cafe-a1", capacity: 10, PublicationStatus.Active);

            IEventRsvpService rsvpService = serviceProvider.GetRequiredService<IEventRsvpService>();

            EventRsvpResponse first = await rsvpService.SubmitAsync(
                "berlin-cafe-a1",
                ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
                new SubmitEventRsvpRequest("Sara Test", "SARA@example.com", "interested"),
                CancellationToken.None);
            EventRsvpResponse updated = await rsvpService.SubmitAsync(
                "berlin-cafe-a1",
                ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
                new SubmitEventRsvpRequest("Sara Updated", "sara@example.com", "going"),
                CancellationToken.None);

            Assert.Equal(first.Id, updated.Id);
            Assert.Equal("sara@example.com", updated.ParticipantEmail);
            Assert.Equal(ContentLanguageRequirements.DefaultTargetLearningLanguageCode, updated.TargetLearningLanguageCode);
            Assert.Equal("Sara Updated", updated.ParticipantName);
            Assert.Equal(EventRsvpStatuses.Going, updated.Status);

            EventRsvpSummaryResponse summary = await rsvpService.GetSummaryAsync("berlin-cafe-a1", ContentLanguageRequirements.DefaultTargetLearningLanguageCode, CancellationToken.None);
            Assert.Equal(ContentLanguageRequirements.DefaultTargetLearningLanguageCode, summary.TargetLearningLanguageCode);
            Assert.Equal(0, summary.InterestedCount);
            Assert.Equal(1, summary.GoingCount);
            Assert.Equal(9, summary.RemainingCapacity);

            IReadOnlyList<EventRsvpResponse> eventRsvps = await rsvpService.GetByEventAsync("berlin-cafe-a1", ContentLanguageRequirements.DefaultTargetLearningLanguageCode, CancellationToken.None);
            Assert.Single(eventRsvps);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }
        }
    }

    [Fact]
    public async Task SubmitAsync_ShouldEnforceCapacityForGoingStatusButAllowCancellation()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_event_rsvp");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);
            await SeedEventAsync(serviceProvider, "full-cafe-a1", capacity: 1, PublicationStatus.Active);

            IEventRsvpService rsvpService = serviceProvider.GetRequiredService<IEventRsvpService>();

            await rsvpService.SubmitAsync(
                "full-cafe-a1",
                ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
                new SubmitEventRsvpRequest("First Learner", "first@example.com", "going"),
                CancellationToken.None);

            InvalidOperationException capacityException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                rsvpService.SubmitAsync(
                    "full-cafe-a1",
                    ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
                    new SubmitEventRsvpRequest("Second Learner", "second@example.com", "going"),
                    CancellationToken.None));
            Assert.Contains("capacity", capacityException.Message, StringComparison.OrdinalIgnoreCase);

            EventRsvpResponse cancelled = await rsvpService.SubmitAsync(
                "full-cafe-a1",
                ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
                new SubmitEventRsvpRequest("Second Learner", "second@example.com", "cancelled"),
                CancellationToken.None);
            Assert.Equal(EventRsvpStatuses.Cancelled, cancelled.Status);

            EventRsvpSummaryResponse summary = await rsvpService.GetSummaryAsync("full-cafe-a1", ContentLanguageRequirements.DefaultTargetLearningLanguageCode, CancellationToken.None);
            Assert.Equal(1, summary.GoingCount);
            Assert.Equal(1, summary.CancelledCount);
            Assert.Equal(0, summary.RemainingCapacity);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }
        }
    }

    [Fact]
    public async Task SetStatusAsync_ShouldRespectCapacityAndPermitExistingGoingRsvpUpdate()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_event_rsvp");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);
            await SeedEventAsync(serviceProvider, "admin-cafe-a1", capacity: 1, PublicationStatus.Active);

            IEventRsvpService rsvpService = serviceProvider.GetRequiredService<IEventRsvpService>();
            EventRsvpResponse going = await rsvpService.SubmitAsync(
                "admin-cafe-a1",
                ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
                new SubmitEventRsvpRequest("Going Learner", "going@example.com", "going"),
                CancellationToken.None);
            EventRsvpResponse interested = await rsvpService.SubmitAsync(
                "admin-cafe-a1",
                ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
                new SubmitEventRsvpRequest("Interested Learner", "interested@example.com", "interested"),
                CancellationToken.None);

            EventRsvpResponse attended = await rsvpService.SetStatusAsync(
                "admin-cafe-a1",
                ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
                going.Id,
                new AdminSetEventRsvpStatusRequest("attended"),
                CancellationToken.None);
            Assert.Equal(EventRsvpStatuses.Attended, attended.Status);

            EventRsvpResponse promoted = await rsvpService.SetStatusAsync(
                "admin-cafe-a1",
                ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
                interested.Id,
                new AdminSetEventRsvpStatusRequest("going"),
                CancellationToken.None);
            Assert.Equal(EventRsvpStatuses.Going, promoted.Status);

            InvalidOperationException capacityException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                rsvpService.SetStatusAsync(
                    "admin-cafe-a1",
                    ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
                    going.Id,
                    new AdminSetEventRsvpStatusRequest("going"),
                    CancellationToken.None));
            Assert.Contains("capacity", capacityException.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }
        }
    }

    [Fact]
    public async Task SubmitAsync_ShouldRejectDraftOrUnknownEvents()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_event_rsvp");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);
            await SeedEventAsync(serviceProvider, "draft-cafe-a1", capacity: null, PublicationStatus.Draft);

            IEventRsvpService rsvpService = serviceProvider.GetRequiredService<IEventRsvpService>();

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                rsvpService.SubmitAsync(
                    "draft-cafe-a1",
                    ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
                    new SubmitEventRsvpRequest("Draft Learner", "draft@example.com", "interested"),
                    CancellationToken.None));
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                rsvpService.SubmitAsync(
                    "missing-cafe-a1",
                    ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
                    new SubmitEventRsvpRequest("Missing Learner", "missing@example.com", "interested"),
                    CancellationToken.None));
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }
        }
    }

    private static ServiceProvider BuildServiceProvider(string connectionString)
    {
        ServiceCollection services = new();
        services.AddDarwinLinguaInfrastructureForPostgres(connectionString);
        services.AddScoped<IEventRsvpService, EventRsvpService>();
        return services.BuildServiceProvider();
    }

    private static async Task SeedEventAsync(
        ServiceProvider serviceProvider,
        string slug,
        int? capacity,
        PublicationStatus publicationStatus)
    {
        IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
            serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None);
        DateTime nowUtc = DateTime.UtcNow;
        ConversationEvent conversationEvent = new(
            Guid.NewGuid(),
            slug,
            "Berlin cafe A1",
            "A learner-friendly conversation event.",
            "Berlin",
            "DE-BE",
            "Central area",
            false,
            "conversation-cafe",
            "Darwin Test Organizer",
            "berlin-language-club",
            "https://example.local/events",
            "events@example.local",
            "Every Tuesday evening",
            "free",
            "reviewed",
            publicationStatus,
            10,
            nowUtc);
        conversationEvent.SetOperationalDetails(null, capacity, nowUtc);
        conversationEvent.AddSupportedLevel(Guid.NewGuid(), CefrLevel.A1, 1, nowUtc);
        conversationEvent.AddHelperLanguage(Guid.NewGuid(), "en", 1, nowUtc);

        dbContext.ConversationEvents.Add(conversationEvent);
        await dbContext.SaveChangesAsync(CancellationToken.None);
    }
}

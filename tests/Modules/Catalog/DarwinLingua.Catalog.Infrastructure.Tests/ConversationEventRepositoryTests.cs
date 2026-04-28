using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Catalog.Infrastructure.Tests;

public sealed class ConversationEventRepositoryTests
{
    [Fact]
    public async Task GetPublishedEventsAsync_ShouldFilterVisibleEvents()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-events-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using (DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                dbContext.ConversationEvents.Add(CreateEvent(
                    "berlin-cafe-a1",
                    "Berlin cafe A1",
                    "Berlin",
                    false,
                    "free",
                    PublicationStatus.Active,
                    CefrLevel.A1,
                    "en",
                    "a1-cafe-first-meeting-prep"));
                dbContext.ConversationEvents.Add(CreateEvent(
                    "online-club-a2",
                    "Online club A2",
                    null,
                    true,
                    "paid",
                    PublicationStatus.Active,
                    CefrLevel.A2,
                    "en",
                    "a2-online-practice-meeting-prep"));
                dbContext.ConversationEvents.Add(CreateEvent(
                    "draft-berlin-cafe",
                    "Draft Berlin cafe",
                    "Berlin",
                    false,
                    "free",
                    PublicationStatus.Draft,
                    CefrLevel.A1,
                    "en",
                    "a1-cafe-first-meeting-prep"));

                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IConversationEventRepository repository = serviceProvider.GetRequiredService<IConversationEventRepository>();
            IReadOnlyList<ConversationEventListItemModel> events = await repository.GetPublishedEventsAsync(
                new ConversationEventListFilterModel("Berlin", "A1", "en", false, "free", "conversation-cafe"),
                CancellationToken.None);

            ConversationEventListItemModel conversationEvent = Assert.Single(events);
            Assert.Equal("berlin-cafe-a1", conversationEvent.Slug);
            Assert.Equal(["A1"], conversationEvent.SupportedLearnerLevels);
            Assert.Equal(["en"], conversationEvent.HelperLanguageCodes);
            Assert.Equal(["a1-cafe-first-meeting-prep"], conversationEvent.LinkedEventPreparationPackSlugs);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            TryDeleteFile(databasePath);
        }
    }

    [Fact]
    public async Task GetPublishedEventBySlugAsync_ShouldReturnDetailWithPreparationLinks()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-event-detail-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using (DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                dbContext.ConversationEvents.Add(CreateEvent(
                    "berlin-cafe-a1",
                    "Berlin cafe A1",
                    "Berlin",
                    false,
                    "donation",
                    PublicationStatus.Active,
                    CefrLevel.A1,
                    "fa",
                    "a1-cafe-first-meeting-prep"));

                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IConversationEventRepository repository = serviceProvider.GetRequiredService<IConversationEventRepository>();
            ConversationEventDetailModel? detail = await repository.GetPublishedEventBySlugAsync("berlin-cafe-a1", CancellationToken.None);

            Assert.NotNull(detail);
            Assert.Equal("Berlin cafe A1", detail.Name);
            Assert.Equal("Berlin", detail.City);
            Assert.False(detail.IsOnline);
            Assert.Equal(["A1"], detail.SupportedLearnerLevels);
            Assert.Equal(["fa"], detail.HelperLanguageCodes);
            Assert.Equal(["a1-cafe-first-meeting-prep"], detail.LinkedEventPreparationPackSlugs);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            TryDeleteFile(databasePath);
        }
    }

    private static ServiceProvider BuildServiceProvider(string databasePath)
    {
        ServiceCollection services = new();
        services
            .AddDarwinLinguaInfrastructure(options => options.DatabasePath = databasePath)
            .AddCatalogInfrastructure();

        return services.BuildServiceProvider();
    }

    private static ConversationEvent CreateEvent(
        string slug,
        string name,
        string? city,
        bool isOnline,
        string priceType,
        PublicationStatus publicationStatus,
        CefrLevel level,
        string helperLanguageCode,
        string preparationPackSlug)
    {
        DateTime nowUtc = DateTime.UtcNow;
        ConversationEvent conversationEvent = new(
            Guid.NewGuid(),
            slug,
            name,
            "A reviewed learner-friendly conversation event.",
            city,
            "DE-BE",
            city is null ? "Online meeting" : "Central area",
            isOnline,
            "conversation-cafe",
            "Darwin Test Organizer",
            null,
            "https://example.local/events",
            "events@example.local",
            "Every Tuesday evening",
            priceType,
            "reviewed",
            publicationStatus,
            1,
            nowUtc);

        conversationEvent.SetSourceMetadata("test-source", "https://example.local/source", nowUtc);
        conversationEvent.AddSupportedLevel(Guid.NewGuid(), level, 1, nowUtc);
        conversationEvent.AddHelperLanguage(Guid.NewGuid(), helperLanguageCode, 1, nowUtc);
        conversationEvent.AddPreparationPackLink(Guid.NewGuid(), preparationPackSlug, 1, nowUtc);
        return conversationEvent;
    }

    private static void TryDeleteFile(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
        }
    }
}

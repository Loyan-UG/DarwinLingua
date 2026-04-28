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

public sealed class OrganizerProfileRepositoryTests
{
    [Fact]
    public async Task GetPublishedOrganizerProfilesAsync_ShouldReturnVisibleProfilesWithActiveEventCounts()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-organizers-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using (DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                dbContext.OrganizerProfiles.Add(CreateProfile("berlin-language-club", "Berlin Language Club", PublicationStatus.Active));
                dbContext.OrganizerProfiles.Add(CreateProfile("draft-club", "Draft Club", PublicationStatus.Draft));
                dbContext.ConversationEvents.Add(CreateEvent("berlin-cafe-a1", "Berlin Cafe A1", "berlin-language-club", PublicationStatus.Active));
                dbContext.ConversationEvents.Add(CreateEvent("draft-event", "Draft Event", "berlin-language-club", PublicationStatus.Draft));

                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IOrganizerProfileRepository repository = serviceProvider.GetRequiredService<IOrganizerProfileRepository>();
            IReadOnlyList<OrganizerProfileListItemModel> profiles = await repository.GetPublishedOrganizerProfilesAsync(CancellationToken.None);

            OrganizerProfileListItemModel profile = Assert.Single(profiles);
            Assert.Equal("berlin-language-club", profile.Slug);
            Assert.Equal(1, profile.ActiveEventCount);
            Assert.Equal(["A1"], profile.SupportedLearnerLevels);
            Assert.Equal(["en"], profile.HelperLanguageCodes);
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
    public async Task GetPublishedOrganizerProfileBySlugAsync_ShouldReturnLinkedActiveEvents()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-organizer-detail-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using (DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                dbContext.OrganizerProfiles.Add(CreateProfile("berlin-language-club", "Berlin Language Club", PublicationStatus.Active));
                dbContext.ConversationEvents.Add(CreateEvent("berlin-cafe-a1", "Berlin Cafe A1", "berlin-language-club", PublicationStatus.Active));
                dbContext.ConversationEvents.Add(CreateEvent("unlinked-cafe", "Unlinked Cafe", null, PublicationStatus.Active));

                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IOrganizerProfileRepository repository = serviceProvider.GetRequiredService<IOrganizerProfileRepository>();
            OrganizerProfileDetailModel? detail = await repository.GetPublishedOrganizerProfileBySlugAsync("berlin-language-club", CancellationToken.None);

            Assert.NotNull(detail);
            ConversationEventListItemModel linkedEvent = Assert.Single(detail.ActiveEvents);
            Assert.Equal("berlin-cafe-a1", linkedEvent.Slug);
            Assert.Equal("berlin-language-club", linkedEvent.OrganizerProfileSlug);
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

    private static OrganizerProfile CreateProfile(string slug, string displayName, PublicationStatus publicationStatus)
    {
        DateTime nowUtc = DateTime.UtcNow;
        OrganizerProfile profile = new(
            Guid.NewGuid(),
            slug,
            displayName,
            "club",
            "A reviewed German practice organizer.",
            "Berlin",
            true,
            "https://example.local",
            "organizer@example.local",
            "verified",
            "free-organizer",
            publicationStatus,
            3,
            nowUtc);

        profile.AddSupportedLevel(Guid.NewGuid(), CefrLevel.A1, 1, nowUtc);
        profile.AddHelperLanguage(Guid.NewGuid(), "en", 1, nowUtc);
        return profile;
    }

    private static ConversationEvent CreateEvent(
        string slug,
        string name,
        string? organizerProfileSlug,
        PublicationStatus publicationStatus)
    {
        DateTime nowUtc = DateTime.UtcNow;
        ConversationEvent conversationEvent = new(
            Guid.NewGuid(),
            slug,
            name,
            "A reviewed learner-friendly conversation event.",
            "Berlin",
            "DE-BE",
            "Central area",
            false,
            "conversation-cafe",
            "Berlin Language Club",
            organizerProfileSlug,
            "https://example.local/events",
            "events@example.local",
            "Every Tuesday evening",
            "free",
            "reviewed",
            publicationStatus,
            1,
            nowUtc);

        conversationEvent.SetSourceMetadata("test-source", "https://example.local/source", nowUtc);
        conversationEvent.AddSupportedLevel(Guid.NewGuid(), CefrLevel.A1, 1, nowUtc);
        conversationEvent.AddHelperLanguage(Guid.NewGuid(), "en", 1, nowUtc);
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

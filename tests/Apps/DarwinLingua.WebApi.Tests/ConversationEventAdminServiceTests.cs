using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class ConversationEventAdminServiceTests
{
    [Fact]
    public async Task SaveAsync_ShouldCreatePublishedConversationEvent()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_admin_events");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IConversationEventAdminService adminService = serviceProvider.GetRequiredService<IConversationEventAdminService>();
            ConversationEventDetailModel savedEvent = await adminService.SaveAsync(
                CreateRequest("berlin-cafe-a1", "Berlin cafe A1", "reviewed"),
                CancellationToken.None);

            Assert.Equal("berlin-cafe-a1", savedEvent.Slug);
            Assert.Equal(["A1", "A2"], savedEvent.SupportedLearnerLevels);
            Assert.Equal(["en", "fa"], savedEvent.HelperLanguageCodes);
            Assert.Equal(["a1-cafe-first-meeting-prep"], savedEvent.LinkedEventPreparationPackSlugs);

            IConversationEventQueryService queryService = serviceProvider.GetRequiredService<IConversationEventQueryService>();
            IReadOnlyList<ConversationEventListItemModel> events = await queryService.GetPublishedEventsAsync(
                new ConversationEventListFilterModel("Berlin", "A1", "en", false, "free", "conversation-cafe"),
                CancellationToken.None);

            Assert.Single(events);
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
    public async Task SaveAsync_ShouldRejectUnsupportedVerificationStatus()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_admin_events");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IConversationEventAdminService adminService = serviceProvider.GetRequiredService<IConversationEventAdminService>();

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                adminService.SaveAsync(CreateRequest("berlin-cafe-a1", "Berlin cafe A1", "private"), CancellationToken.None));

            Assert.Contains("verification status", exception.Message, StringComparison.OrdinalIgnoreCase);
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
    public async Task SaveAsync_ShouldRejectStaleReviewedEvents()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_admin_events");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IConversationEventAdminService adminService = serviceProvider.GetRequiredService<IConversationEventAdminService>();
            AdminSaveConversationEventRequest staleRequest = CreateRequest("berlin-cafe-a1", "Berlin cafe A1", "reviewed") with
            {
                LastVerifiedAtUtc = DateTime.UtcNow.AddDays(-181),
            };

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                adminService.SaveAsync(staleRequest, CancellationToken.None));

            Assert.Contains("180 days", exception.Message, StringComparison.OrdinalIgnoreCase);
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
        services
            .AddDarwinLinguaInfrastructureForPostgres(connectionString)
            .AddCatalogApplication()
            .AddCatalogInfrastructure();
        services.AddScoped<IConversationEventAdminService, ConversationEventAdminService>();

        return services.BuildServiceProvider();
    }

    private static AdminSaveConversationEventRequest CreateRequest(
        string slug,
        string name,
        string verificationStatus) =>
        new(
            slug,
            name,
            "A manually reviewed language-practice event.",
            "Berlin",
            "DE-BE",
            "Central Berlin",
            false,
            "conversation-cafe",
            ["A1", "A2"],
            ["en", "fa"],
            "Darwin Test Organizer",
            null,
            "https://example.local/events",
            "events@example.local",
            "Every Tuesday evening",
            "free",
            verificationStatus,
            "manual-admin",
            "https://example.local/source",
            DateTime.UtcNow,
            ["a1-cafe-first-meeting-prep"]);

}

using System.Diagnostics;
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
using Npgsql;

namespace DarwinLingua.Catalog.Infrastructure.Tests;

public sealed class ConversationEventRepositoryTests
{
    private const string DefaultDockerContainerName = "darwinlingua-postgres";

    [Fact]
    public async Task GetPublishedEventsAsync_ShouldFilterVisibleEvents()
    {
        string databaseName = $"darwin_events_test_{Guid.NewGuid():N}"[..48];
        string connectionString = BuildAppConnectionString(databaseName);
        ServiceProvider? serviceProvider = null;
        await CreateDatabaseAsync(databaseName, CancellationToken.None);

        try
        {
            serviceProvider = BuildServiceProvider(connectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using (DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                DateTime startUtc = new(2026, 6, 18, 18, 0, 0, DateTimeKind.Utc);
                dbContext.ConversationEvents.Add(CreateEvent(
                    "berlin-cafe-a1",
                    "Berlin cafe A1",
                    "Berlin",
                    false,
                    "free",
                    PublicationStatus.Active,
                    CefrLevel.A1,
                    "en",
                    "a1-cafe-first-meeting-prep",
                    startUtc));
                dbContext.ConversationEvents.Add(CreateEvent(
                    "online-club-a2",
                    "Online club A2",
                    null,
                    true,
                    "paid",
                    PublicationStatus.Active,
                    CefrLevel.A2,
                    "en",
                    "a2-online-practice-meeting-prep",
                    startUtc.AddDays(2)));
                dbContext.ConversationEvents.Add(CreateEvent(
                    "berlin-cafe-paid-a1",
                    "Berlin cafe paid A1",
                    "Berlin",
                    false,
                    "paid",
                    PublicationStatus.Active,
                    CefrLevel.A1,
                    "en",
                    "a1-cafe-first-meeting-prep"));
                dbContext.ConversationEvents.Add(CreateEvent(
                    "berlin-cafe-a1-de",
                    "Berlin cafe A1 German helper",
                    "Berlin",
                    false,
                    "free",
                    PublicationStatus.Active,
                    CefrLevel.A1,
                    "de",
                    "a1-cafe-first-meeting-prep"));
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

            IReadOnlyList<ConversationEventListItemModel> onlineEvents = await repository.GetPublishedEventsAsync(
                new ConversationEventListFilterModel(null, "A2", "en", true, "paid", null),
                CancellationToken.None);
            Assert.Equal("online-club-a2", Assert.Single(onlineEvents).Slug);

            IReadOnlyList<ConversationEventListItemModel> germanHelperEvents = await repository.GetPublishedEventsAsync(
                new ConversationEventListFilterModel("berlin", "a1", "de", false, "free", "conversation-cafe"),
                CancellationToken.None);
            Assert.Equal("berlin-cafe-a1-de", Assert.Single(germanHelperEvents).Slug);

            IReadOnlyList<ConversationEventListItemModel> dateFilteredEvents = await repository.GetPublishedEventsAsync(
                new ConversationEventListFilterModel(null, null, null, null, null, null, new DateTime(2026, 6, 18, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 18, 23, 59, 59, DateTimeKind.Utc)),
                CancellationToken.None);
            ConversationEventListItemModel dateFilteredEvent = Assert.Single(dateFilteredEvents);
            Assert.Equal("berlin-cafe-a1", dateFilteredEvent.Slug);
            Assert.Equal(new DateTime(2026, 6, 18, 18, 0, 0, DateTimeKind.Utc), dateFilteredEvent.StartsAtUtc);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            await DropDatabaseAsync(databaseName, CancellationToken.None);
        }
    }

    [Fact]
    public async Task GetPublishedEventBySlugAsync_ShouldReturnDetailWithPreparationLinks()
    {
        string databaseName = $"darwin_event_detail_{Guid.NewGuid():N}"[..48];
        string connectionString = BuildAppConnectionString(databaseName);
        ServiceProvider? serviceProvider = null;
        await CreateDatabaseAsync(databaseName, CancellationToken.None);

        try
        {
            serviceProvider = BuildServiceProvider(connectionString);
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

            ConversationEventDetailModel? draftDetail = await repository.GetPublishedEventBySlugAsync("missing-event", CancellationToken.None);
            Assert.Null(draftDetail);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            await DropDatabaseAsync(databaseName, CancellationToken.None);
        }
    }

    private static ServiceProvider BuildServiceProvider(string connectionString)
    {
        ServiceCollection services = new();
        services
            .AddDarwinLinguaInfrastructureForPostgres(connectionString)
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
        string preparationPackSlug,
        DateTime? startsAtUtc = null)
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
            nowUtc,
            startsAtUtc,
            startsAtUtc?.AddHours(1));

        conversationEvent.SetSourceMetadata("test-source", "https://example.local/source", nowUtc);
        conversationEvent.AddSupportedLevel(Guid.NewGuid(), level, 1, nowUtc);
        conversationEvent.AddHelperLanguage(Guid.NewGuid(), helperLanguageCode, 1, nowUtc);
        conversationEvent.AddPreparationPackLink(Guid.NewGuid(), preparationPackSlug, 1, nowUtc);
        return conversationEvent;
    }

    private static string BuildAppConnectionString(string databaseName)
    {
        string? configuredTemplate = Environment.GetEnvironmentVariable("DARWINLINGUA_TEST_POSTGRES_APP_CONNECTION_TEMPLATE");
        if (!string.IsNullOrWhiteSpace(configuredTemplate))
        {
            return string.Format(configuredTemplate, databaseName);
        }

        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = "localhost",
            Port = 5432,
            Database = databaseName,
            Username = "darwinlingua_app",
            Password = "@pP@sS!13;X"
        };

        return builder.ConnectionString;
    }

    private static async Task CreateDatabaseAsync(string databaseName, CancellationToken cancellationToken) =>
        await RunDockerPsqlAsync($"""CREATE DATABASE "{databaseName}" OWNER darwinlingua_app;""", cancellationToken);

    private static async Task DropDatabaseAsync(string databaseName, CancellationToken cancellationToken)
    {
        await RunDockerPsqlAsync(
            $"""
            SELECT pg_terminate_backend(pid)
            FROM pg_stat_activity
            WHERE datname = '{databaseName}'
              AND pid <> pg_backend_pid();
            """,
            cancellationToken);
        await RunDockerPsqlAsync($"""DROP DATABASE IF EXISTS "{databaseName}";""", cancellationToken);
    }

    private static async Task RunDockerPsqlAsync(string sql, CancellationToken cancellationToken)
    {
        string containerName = Environment.GetEnvironmentVariable("DARWINLINGUA_TEST_POSTGRES_CONTAINER") ?? DefaultDockerContainerName;
        ProcessStartInfo startInfo = new()
        {
            FileName = "docker",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        startInfo.ArgumentList.Add("exec");
        startInfo.ArgumentList.Add(containerName);
        startInfo.ArgumentList.Add("psql");
        startInfo.ArgumentList.Add("-U");
        startInfo.ArgumentList.Add("postgres");
        startInfo.ArgumentList.Add("-d");
        startInfo.ArgumentList.Add("postgres");
        startInfo.ArgumentList.Add("-v");
        startInfo.ArgumentList.Add("ON_ERROR_STOP=1");
        startInfo.ArgumentList.Add("-c");
        startInfo.ArgumentList.Add(sql);

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start Docker PostgreSQL helper process.");
        string standardOutput = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        string standardError = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"PostgreSQL test database command failed with exit code {process.ExitCode}.{Environment.NewLine}{standardOutput}{Environment.NewLine}{standardError}");
        }
    }
}

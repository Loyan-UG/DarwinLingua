using System.Diagnostics;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace DarwinLingua.Catalog.Infrastructure.Tests;

public sealed class OrganizerProfileRepositoryTests
{
    private const string DefaultDockerContainerName = "darwinlingua-postgres";

    [Fact]
    public async Task GetPublishedOrganizerProfilesAsync_ShouldReturnVisibleProfilesWithActiveEventCounts()
    {
        string databaseName = $"darwin_organizer_test_{Guid.NewGuid():N}"[..48];
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
                dbContext.OrganizerProfiles.Add(CreateProfile("berlin-language-club", "Berlin Language Club", PublicationStatus.Active));
                dbContext.OrganizerProfiles.Add(CreateProfile("draft-club", "Draft Club", PublicationStatus.Draft));
                dbContext.ConversationEvents.Add(CreateEvent("berlin-cafe-a1", "Berlin Cafe A1", "berlin-language-club", PublicationStatus.Active));
                dbContext.ConversationEvents.Add(CreateEvent("draft-event", "Draft Event", "berlin-language-club", PublicationStatus.Draft));

                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IOrganizerProfileRepository repository = serviceProvider.GetRequiredService<IOrganizerProfileRepository>();
            IReadOnlyList<OrganizerProfileListItemModel> profiles = await repository.GetPublishedOrganizerProfilesAsync(
                ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
                CancellationToken.None);

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

            await DropDatabaseAsync(databaseName, CancellationToken.None);
        }
    }

    [Fact]
    public async Task GetPublishedOrganizerProfileBySlugAsync_ShouldReturnLinkedActiveEvents()
    {
        string databaseName = $"darwin_organizer_detail_{Guid.NewGuid():N}"[..48];
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
                dbContext.OrganizerProfiles.Add(CreateProfile("berlin-language-club", "Berlin Language Club", PublicationStatus.Active));
                dbContext.ConversationEvents.Add(CreateEvent("berlin-cafe-a1", "Berlin Cafe A1", "berlin-language-club", PublicationStatus.Active));
                dbContext.ConversationEvents.Add(CreateEvent("unlinked-cafe", "Unlinked Cafe", null, PublicationStatus.Active));

                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IOrganizerProfileRepository repository = serviceProvider.GetRequiredService<IOrganizerProfileRepository>();
            OrganizerProfileDetailModel? detail = await repository.GetPublishedOrganizerProfileBySlugAsync(
                "berlin-language-club",
                ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
                CancellationToken.None);

            Assert.NotNull(detail);
            ConversationEventListItemModel linkedEvent = Assert.Single(detail.ActiveEvents);
            Assert.Equal("berlin-cafe-a1", linkedEvent.Slug);
            Assert.Equal("berlin-language-club", linkedEvent.OrganizerProfileSlug);

            OrganizerProfileDetailModel? missingDetail = await repository.GetPublishedOrganizerProfileBySlugAsync(
                "missing-club",
                ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
                CancellationToken.None);
            Assert.Null(missingDetail);
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

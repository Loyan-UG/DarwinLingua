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

public sealed class TalkTopicPostgresRepositoryTests
{
    private const string DefaultDockerContainerName = "darwinlingua-postgres";

    [Fact]
    public async Task TalkTopicRepository_ShouldResolveKnownVocabularyAndLeaveUnknownVocabularyUnlinked()
    {
        string databaseName = $"darwin_talk_topic_test_{Guid.NewGuid():N}"[..46];
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
                DateTime nowUtc = DateTime.UtcNow;
                dbContext.WordEntries.Add(CreateWord("Bahnhof", "station"));
                dbContext.TalkTopics.Add(CreateTalkTopic(
                    "a1-bahnhof",
                    "Bahnhof",
                    "der Bahnhof",
                    "der-bahnhof",
                    nowUtc));
                dbContext.TalkTopics.Add(CreateTalkTopic(
                    "a1-unbekanntes-wort",
                    "Unbekanntes Wort",
                    "das Testwort",
                    "das-testwort",
                    nowUtc));

                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            ITalkTopicRepository repository = serviceProvider.GetRequiredService<ITalkTopicRepository>();
            TalkTopicDetailModel? resolvedDetail = await repository.GetPublishedTalkTopicBySlugAsync(
                "a1-bahnhof",
                "en",
                null,
                CancellationToken.None);

            Assert.NotNull(resolvedDetail);
            TalkTopicVocabularyItemModel resolvedItem = Assert.Single(resolvedDetail.VocabularyItems);
            Assert.True(resolvedItem.IsResolved);
            Assert.Equal("bahnhof", resolvedItem.WordSlug);
            Assert.Equal("station", resolvedItem.PrimaryMeaning);

            TalkTopicDetailModel? unresolvedDetail = await repository.GetPublishedTalkTopicBySlugAsync(
                "a1-unbekanntes-wort",
                "en",
                null,
                CancellationToken.None);

            Assert.NotNull(unresolvedDetail);
            TalkTopicVocabularyItemModel unresolvedItem = Assert.Single(unresolvedDetail.VocabularyItems);
            Assert.False(unresolvedItem.IsResolved);
            Assert.Null(unresolvedItem.WordSlug);
            Assert.Null(unresolvedItem.PrimaryMeaning);
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

    private static WordEntry CreateWord(string lemma, string translationText)
    {
        DateTime nowUtc = DateTime.UtcNow;
        WordEntry word = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            lemma,
            LanguageCode.From("de"),
            CefrLevel.A1,
            PartOfSpeech.Noun,
            PublicationStatus.Active,
            ContentSourceType.Manual,
            nowUtc,
            article: "der");

        WordSense sense = word.AddSense(Guid.NewGuid(), 1, true, PublicationStatus.Active, nowUtc);
        sense.AddTranslation(Guid.NewGuid(), LanguageCode.From("en"), translationText, true, nowUtc);

        return word;
    }

    private static TalkTopic CreateTalkTopic(
        string slug,
        string title,
        string vocabularyLemma,
        string vocabularyWordSlug,
        DateTime nowUtc)
    {
        TalkTopic talkTopic = new(
            Guid.NewGuid(),
            slug,
            slug.Replace("a1-", string.Empty, StringComparison.Ordinal),
            title,
            "Ein kurzer Talk Topic fuer Repository-Tests.",
            CefrLevel.A1,
            "daily-life",
            TalkTopicContentType.Article,
            "Dieser kurze Text prueft sichere Vokabellinks.",
            3,
            20,
            false,
            null,
            false,
            PublicationStatus.Active,
            10,
            nowUtc);
        talkTopic.AddVocabularyItem(Guid.NewGuid(), vocabularyLemma, vocabularyWordSlug, CefrLevel.A1, 10, nowUtc);

        return talkTopic;
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

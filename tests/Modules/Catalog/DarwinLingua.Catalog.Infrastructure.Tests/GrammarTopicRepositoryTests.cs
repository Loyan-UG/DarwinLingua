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

public sealed class GrammarTopicRepositoryTests
{
    private const string DefaultDockerContainerName = "darwinlingua-postgres";

    [Fact]
    public async Task GetPublishedGrammarTopicsAsync_ShouldFilterByCefrCategoryTopicAndSearch()
    {
        string databaseName = $"darwin_grammar_test_{Guid.NewGuid():N}"[..48];
        string connectionString = BuildAppConnectionString(databaseName);
        ServiceProvider? serviceProvider = null;
        await CreateDatabaseAsync(databaseName, CancellationToken.None);

        try
        {
            serviceProvider = BuildServiceProvider(connectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            Guid identityTopicId = Guid.NewGuid();
            Guid workTopicId = Guid.NewGuid();
            DateTime nowUtc = DateTime.UtcNow;
            await using (DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                dbContext.Topics.Add(new Topic(identityTopicId, "personal-identity", 1, true, nowUtc));
                dbContext.Topics.Add(new Topic(workTopicId, "workplace", 2, true, nowUtc));

                dbContext.GrammarTopics.Add(CreateGrammarTopic(
                    "a1-personal-pronouns",
                    "Personal pronouns",
                    "Use ich, du, er and sie in short sentences.",
                    CefrLevel.A1,
                    "pronouns",
                    PublicationStatus.Active,
                    identityTopicId,
                    nowUtc));
                dbContext.GrammarTopics.Add(CreateGrammarTopic(
                    "a2-word-order",
                    "Word order after time phrases",
                    "Place the verb in position two after a short time phrase.",
                    CefrLevel.A2,
                    "sentence-structure",
                    PublicationStatus.Active,
                    workTopicId,
                    nowUtc));
                dbContext.GrammarTopics.Add(CreateGrammarTopic(
                    "draft-pronouns",
                    "Draft pronouns",
                    "This draft should never appear in public grammar results.",
                    CefrLevel.A1,
                    "pronouns",
                    PublicationStatus.Draft,
                    identityTopicId,
                    nowUtc));

                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IGrammarTopicRepository repository = serviceProvider.GetRequiredService<IGrammarTopicRepository>();

            IReadOnlyList<GrammarTopicListItemModel> filtered = await repository.GetPublishedGrammarTopicsAsync(
                new GrammarTopicListFilterModel("A1", "pronouns", "personal-identity", "pronouns"),
                CancellationToken.None);

            GrammarTopicListItemModel grammarTopic = Assert.Single(filtered);
            Assert.Equal("a1-personal-pronouns", grammarTopic.Slug);
            Assert.Equal("A1", grammarTopic.CefrLevel);
            Assert.Equal("pronouns", grammarTopic.GrammarCategory);
            Assert.Equal(["personal-identity"], grammarTopic.TopicKeys);

            IReadOnlyList<GrammarTopicListItemModel> caseInsensitiveSearch = await repository.GetPublishedGrammarTopicsAsync(
                new GrammarTopicListFilterModel(null, null, null, "TIME PHRASES"),
                CancellationToken.None);
            Assert.Equal("a2-word-order", Assert.Single(caseInsensitiveSearch).Slug);

            IReadOnlyList<GrammarTopicListItemModel> hiddenDrafts = await repository.GetPublishedGrammarTopicsAsync(
                new GrammarTopicListFilterModel("A1", "pronouns", "personal-identity", "draft"),
                CancellationToken.None);
            Assert.Empty(hiddenDrafts);
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
    public async Task GetPublishedGrammarTopicBySlugAsync_ShouldProjectLocalizedTextAndLinksSafely()
    {
        string databaseName = $"darwin_grammar_detail_{Guid.NewGuid():N}"[..48];
        string connectionString = BuildAppConnectionString(databaseName);
        ServiceProvider? serviceProvider = null;
        await CreateDatabaseAsync(databaseName, CancellationToken.None);

        try
        {
            serviceProvider = BuildServiceProvider(connectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            Guid identityTopicId = Guid.NewGuid();
            DateTime nowUtc = DateTime.UtcNow;
            await using (DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                dbContext.Topics.Add(new Topic(identityTopicId, "personal-identity", 1, true, nowUtc));

                GrammarTopic grammarTopic = CreateGrammarTopic(
                    "a1-personal-pronouns",
                    "Personal pronouns",
                    "Use ich, du, er and sie in short sentences.",
                    CefrLevel.A1,
                    "pronouns",
                    PublicationStatus.Active,
                    identityTopicId,
                    nowUtc);
                GrammarSection section = grammarTopic.Sections.Single();
                section.AddTranslation(Guid.NewGuid(), LanguageCode.From("fa"), "قاعده", "از ضمیرهای شخصی برای گفتن اینکه چه کسی کاری را انجام می‌دهد استفاده کن.", nowUtc);
                GrammarExample example = grammarTopic.AddExample(Guid.NewGuid(), 1, "Ich lerne Deutsch.", "Keep the verb second.", nowUtc);
                example.AddTranslation(Guid.NewGuid(), LanguageCode.From("fa"), "من آلمانی یاد می‌گیرم.", nowUtc);
                GrammarRuleSummary rule = grammarTopic.AddRuleSummary(Guid.NewGuid(), 1, "Pronoun plus verb creates a simple sentence.", nowUtc);
                rule.AddTranslation(Guid.NewGuid(), LanguageCode.From("fa"), "ضمیر همراه با فعل یک جمله ساده می‌سازد.", nowUtc);
                GrammarCommonMistake mistake = grammarTopic.AddCommonMistake(Guid.NewGuid(), 1, "Ich Deutsch lerne.", "Ich lerne Deutsch.", "Keep the finite verb in position two.", nowUtc);
                mistake.AddTranslation(Guid.NewGuid(), LanguageCode.From("fa"), "فعل صرف‌شده را در جایگاه دوم نگه دار.", nowUtc);
                GrammarExceptionNote note = grammarTopic.AddExceptionNote(Guid.NewGuid(), 1, "In questions, the verb can come first.", nowUtc);
                note.AddTranslation(Guid.NewGuid(), LanguageCode.From("fa"), "در پرسش‌ها فعل می‌تواند اول بیاید.", nowUtc);
                grammarTopic.AddLinkedWord(Guid.NewGuid(), "ich", "ich", 1, nowUtc);
                grammarTopic.AddLinkedWord(Guid.NewGuid(), "du", null, 2, nowUtc);
                grammarTopic.AddLinkedDialogue(Guid.NewGuid(), "a1-greeting-dialogue", 1, nowUtc);
                grammarTopic.AddLinkedTalkTopic(Guid.NewGuid(), "a1-sich-vorstellen", 1, nowUtc);
                grammarTopic.AddLinkedExercise(Guid.NewGuid(), "a1-pronouns-practice", 1, nowUtc);

                dbContext.GrammarTopics.Add(grammarTopic);
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IGrammarTopicRepository repository = serviceProvider.GetRequiredService<IGrammarTopicRepository>();
            GrammarTopicDetailModel? detail = await repository.GetPublishedGrammarTopicBySlugAsync(
                "a1-personal-pronouns",
                "fa",
                CancellationToken.None);

            Assert.NotNull(detail);
            Assert.Equal("قاعده", Assert.Single(detail.Sections).Heading);
            Assert.Equal("از ضمیرهای شخصی برای گفتن اینکه چه کسی کاری را انجام می‌دهد استفاده کن.", Assert.Single(detail.Sections).Explanation);
            Assert.False(Assert.Single(detail.Sections).UsedFallback);
            Assert.Equal("من آلمانی یاد می‌گیرم.", Assert.Single(detail.Examples).Translation);
            Assert.Equal("ضمیر همراه با فعل یک جمله ساده می‌سازد.", Assert.Single(detail.RuleSummaries).Text);
            Assert.Equal("فعل صرف‌شده را در جایگاه دوم نگه دار.", Assert.Single(detail.CommonMistakes).Explanation);
            Assert.Equal("در پرسش‌ها فعل می‌تواند اول بیاید.", Assert.Single(detail.ExceptionNotes).Text);
            Assert.Contains(detail.LinkedWords, word => word.Lemma == "ich" && word.WordSlug == "ich");
            Assert.Contains(detail.LinkedWords, word => word.Lemma == "du" && word.WordSlug is null);
            Assert.Equal(["a1-greeting-dialogue"], detail.LinkedDialogueSlugs);
            Assert.Equal(["a1-sich-vorstellen"], detail.LinkedTalkTopicSlugs);
            Assert.Equal(["a1-pronouns-practice"], detail.LinkedExerciseSlugs);
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

    private static GrammarTopic CreateGrammarTopic(
        string slug,
        string title,
        string shortDescription,
        CefrLevel cefrLevel,
        string grammarCategory,
        PublicationStatus publicationStatus,
        Guid topicId,
        DateTime nowUtc)
    {
        GrammarTopic grammarTopic = new(
            Guid.NewGuid(),
            slug,
            title,
            shortDescription,
            cefrLevel,
            grammarCategory,
            publicationStatus,
            1,
            nowUtc);
        grammarTopic.AddTopic(Guid.NewGuid(), topicId, isPrimary: true, nowUtc);
        grammarTopic.AddSection(Guid.NewGuid(), 1, "Rule", "Use the pattern in short examples.", nowUtc);
        return grammarTopic;
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

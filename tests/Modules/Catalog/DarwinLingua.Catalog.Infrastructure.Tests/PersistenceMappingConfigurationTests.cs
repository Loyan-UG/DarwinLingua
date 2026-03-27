using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Learning.Domain.Entities;
using DarwinLingua.Localization.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Catalog.Infrastructure.Tests;

/// <summary>
/// Verifies critical EF Core persistence mappings for Phase 1 entities.
/// </summary>
public sealed class PersistenceMappingConfigurationTests
{
    /// <summary>
    /// Verifies the lexical-entry indexes needed by browse and search flows.
    /// </summary>
    [Fact]
    public async Task Model_ShouldConfigureWordEntrySearchAndUniquenessIndexes()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-mapping-word-entry-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);
            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using DarwinLinguaDbContext dbContext = await dbContextFactory
                .CreateDbContextAsync(CancellationToken.None);

            IEntityType wordEntryEntity = dbContext.Model.FindEntityType(typeof(WordEntry))
                ?? throw new Xunit.Sdk.XunitException("WordEntry mapping is missing from the model.");

            IReadOnlyList<IIndex> indexes = wordEntryEntity.GetIndexes().ToArray();

            Assert.Contains(indexes, index => index.GetDatabaseName() == "IX_WordEntries_Search_NormalizedLemma");
            Assert.Contains(indexes, index => index.GetDatabaseName() == "IX_WordEntries_Search_ActiveNormalizedLemma");
            Assert.Contains(indexes, index => index.GetDatabaseName() == "IX_WordEntries_Browse_Cefr_NormalizedLemma");
            Assert.Contains(
                indexes,
                index =>
                    index.IsUnique &&
                    index.Properties.Select(property => property.Name).SequenceEqual(
                        [nameof(WordEntry.NormalizedLemma), nameof(WordEntry.PartOfSpeech), nameof(WordEntry.PrimaryCefrLevel)]));
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

    /// <summary>
    /// Verifies unique constraints for localization and translation child entities.
    /// </summary>
    [Fact]
    public async Task Model_ShouldConfigureUniqueCompositeIndexesForLocalizedChildren()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-mapping-children-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);
            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using DarwinLinguaDbContext dbContext = await dbContextFactory
                .CreateDbContextAsync(CancellationToken.None);

            IEntityType translationEntity = dbContext.Model.FindEntityType(typeof(SenseTranslation))
                ?? throw new Xunit.Sdk.XunitException("SenseTranslation mapping is missing from the model.");
            IEntityType exampleEntity = dbContext.Model.FindEntityType(typeof(ExampleSentence))
                ?? throw new Xunit.Sdk.XunitException("ExampleSentence mapping is missing from the model.");
            IEntityType topicLocalizationEntity = dbContext.Model.FindEntityType(typeof(TopicLocalization))
                ?? throw new Xunit.Sdk.XunitException("TopicLocalization mapping is missing from the model.");
            IEntityType wordSenseEntity = dbContext.Model.FindEntityType(typeof(WordSense))
                ?? throw new Xunit.Sdk.XunitException("WordSense mapping is missing from the model.");
            IEntityType wordTopicEntity = dbContext.Model.FindEntityType(typeof(WordTopic))
                ?? throw new Xunit.Sdk.XunitException("WordTopic mapping is missing from the model.");
            IEntityType wordLabelEntity = dbContext.Model.FindEntityType(typeof(WordLabel))
                ?? throw new Xunit.Sdk.XunitException("WordLabel mapping is missing from the model.");
            IEntityType wordGrammarNoteEntity = dbContext.Model.FindEntityType(typeof(WordGrammarNote))
                ?? throw new Xunit.Sdk.XunitException("WordGrammarNote mapping is missing from the model.");
            IEntityType wordCollocationEntity = dbContext.Model.FindEntityType(typeof(WordCollocation))
                ?? throw new Xunit.Sdk.XunitException("WordCollocation mapping is missing from the model.");

            Assert.Contains(
                translationEntity.GetIndexes(),
                index =>
                    index.IsUnique &&
                    index.Properties.Select(property => property.Name).SequenceEqual(
                        [nameof(SenseTranslation.WordSenseId), nameof(SenseTranslation.LanguageCode)]));

            Assert.Contains(
                topicLocalizationEntity.GetIndexes(),
                index =>
                    index.IsUnique &&
                    index.Properties.Select(property => property.Name).SequenceEqual(
                        [nameof(TopicLocalization.TopicId), nameof(TopicLocalization.LanguageCode)]));

            Assert.Contains(
                wordSenseEntity.GetIndexes(),
                index =>
                    index.GetDatabaseName() == "IX_WordSenses_PrimaryPerWordEntry" &&
                    index.IsUnique &&
                    index.Properties.Select(property => property.Name).SequenceEqual([nameof(WordSense.WordEntryId)]));

            Assert.Contains(
                translationEntity.GetIndexes(),
                index =>
                    index.GetDatabaseName() == "IX_SenseTranslations_PrimaryPerSense" &&
                    index.IsUnique &&
                    index.Properties.Select(property => property.Name).SequenceEqual([nameof(SenseTranslation.WordSenseId)]));

            Assert.Contains(
                exampleEntity.GetIndexes(),
                index =>
                    index.GetDatabaseName() == "IX_ExampleSentences_PrimaryPerSense" &&
                    index.IsUnique &&
                    index.Properties.Select(property => property.Name).SequenceEqual([nameof(ExampleSentence.WordSenseId)]));

            Assert.Contains(
                wordTopicEntity.GetIndexes(),
                index =>
                    index.GetDatabaseName() == "IX_WordTopics_PrimaryPerWordEntry" &&
                    index.IsUnique &&
                    index.Properties.Select(property => property.Name).SequenceEqual([nameof(WordTopic.WordEntryId)]));

            Assert.Contains(
                wordLabelEntity.GetIndexes(),
                index =>
                    index.IsUnique &&
                    index.Properties.Select(property => property.Name).SequenceEqual(
                        [nameof(WordLabel.WordEntryId), nameof(WordLabel.Kind), nameof(WordLabel.Key)]));

            Assert.Contains(
                wordGrammarNoteEntity.GetIndexes(),
                index =>
                    index.IsUnique &&
                    index.Properties.Select(property => property.Name).SequenceEqual(
                        [nameof(WordGrammarNote.WordEntryId), nameof(WordGrammarNote.Text)]));

            Assert.Contains(
                wordCollocationEntity.GetIndexes(),
                index =>
                    index.IsUnique &&
                    index.Properties.Select(property => property.Name).SequenceEqual(
                        [nameof(WordCollocation.WordEntryId), nameof(WordCollocation.Text)]));
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

    /// <summary>
    /// Verifies unique constraints for language reference rows and user favorites.
    /// </summary>
    [Fact]
    public async Task Model_ShouldConfigureUniqueIndexesForLanguageCodeAndFavorites()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-mapping-identity-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);
            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using DarwinLinguaDbContext dbContext = await dbContextFactory
                .CreateDbContextAsync(CancellationToken.None);

            IEntityType languageEntity = dbContext.Model.FindEntityType(typeof(Language))
                ?? throw new Xunit.Sdk.XunitException("Language mapping is missing from the model.");
            IEntityType favoriteEntity = dbContext.Model.FindEntityType(typeof(UserFavoriteWord))
                ?? throw new Xunit.Sdk.XunitException("UserFavoriteWord mapping is missing from the model.");
            IEntityType userWordStateEntity = dbContext.Model.FindEntityType(typeof(UserWordState))
                ?? throw new Xunit.Sdk.XunitException("UserWordState mapping is missing from the model.");

            Assert.Contains(
                languageEntity.GetIndexes(),
                index =>
                    index.IsUnique &&
                    index.Properties.Select(property => property.Name).SequenceEqual([nameof(Language.Code)]));

            Assert.Contains(
                favoriteEntity.GetIndexes(),
                index =>
                    index.IsUnique &&
                    index.Properties.Select(property => property.Name).SequenceEqual(
                        [nameof(UserFavoriteWord.UserId), nameof(UserFavoriteWord.WordEntryPublicId)]));

            Assert.Contains(
                userWordStateEntity.GetIndexes(),
                index =>
                    index.IsUnique &&
                    index.Properties.Select(property => property.Name).SequenceEqual(
                        [nameof(UserWordState.UserId), nameof(UserWordState.WordEntryPublicId)]));
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

    /// <summary>
    /// Verifies relationship constraints for word-topic links.
    /// </summary>
    [Fact]
    public async Task Model_ShouldConfigureWordTopicForeignKeysForWordEntryAndTopic()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-mapping-word-topic-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);
            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using DarwinLinguaDbContext dbContext = await dbContextFactory
                .CreateDbContextAsync(CancellationToken.None);

            IEntityType wordTopicEntity = dbContext.Model.FindEntityType(typeof(WordTopic))
                ?? throw new Xunit.Sdk.XunitException("WordTopic mapping is missing from the model.");

            Assert.Contains(
                wordTopicEntity.GetForeignKeys(),
                foreignKey =>
                    foreignKey.PrincipalEntityType.ClrType == typeof(WordEntry) &&
                    foreignKey.Properties.Select(property => property.Name).SequenceEqual([nameof(WordTopic.WordEntryId)]));

            Assert.Contains(
                wordTopicEntity.GetForeignKeys(),
                foreignKey =>
                    foreignKey.PrincipalEntityType.ClrType == typeof(Topic) &&
                    foreignKey.Properties.Select(property => property.Name).SequenceEqual([nameof(WordTopic.TopicId)]));
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

    /// <summary>
    /// Verifies that user-state tables remain decoupled from catalog-content foreign keys.
    /// </summary>
    [Fact]
    public async Task Model_ShouldKeepUserStateTablesSeparateFromCatalogContentForeignKeys()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-mapping-user-state-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);
            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using DarwinLinguaDbContext dbContext = await dbContextFactory
                .CreateDbContextAsync(CancellationToken.None);

            IEntityType favoriteEntity = dbContext.Model.FindEntityType(typeof(UserFavoriteWord))
                ?? throw new Xunit.Sdk.XunitException("UserFavoriteWord mapping is missing from the model.");
            IEntityType userWordStateEntity = dbContext.Model.FindEntityType(typeof(UserWordState))
                ?? throw new Xunit.Sdk.XunitException("UserWordState mapping is missing from the model.");

            Assert.DoesNotContain(
                favoriteEntity.GetForeignKeys(),
                foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(WordEntry));

            Assert.DoesNotContain(
                userWordStateEntity.GetForeignKeys(),
                foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(WordEntry));
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

    /// <summary>
    /// Builds the service provider used by persistence mapping tests.
    /// </summary>
    private static ServiceProvider BuildServiceProvider(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        ServiceCollection services = new();
        services.AddDarwinLinguaInfrastructure(options => options.DatabasePath = databasePath);

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Deletes a temporary test database file while safely ignoring lock races.
    /// </summary>
    private static void TryDeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (IOException)
        {
            // Ignore transient file-lock races during test cleanup.
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore cleanup failures caused by external process locks.
        }
    }
}

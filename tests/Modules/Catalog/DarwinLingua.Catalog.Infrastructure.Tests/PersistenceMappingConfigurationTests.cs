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

            Assert.Contains(indexes, index => index.Name == "IX_WordEntries_Search_NormalizedLemma");
            Assert.Contains(indexes, index => index.Name == "IX_WordEntries_Search_ActiveNormalizedLemma");
            Assert.Contains(indexes, index => index.Name == "IX_WordEntries_Browse_Cefr_NormalizedLemma");
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
            IEntityType topicLocalizationEntity = dbContext.Model.FindEntityType(typeof(TopicLocalization))
                ?? throw new Xunit.Sdk.XunitException("TopicLocalization mapping is missing from the model.");

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

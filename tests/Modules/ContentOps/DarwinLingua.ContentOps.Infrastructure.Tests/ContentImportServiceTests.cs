using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.DependencyInjection;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.Localization.Application.DependencyInjection;
using DarwinLingua.Localization.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

/// <summary>
/// Verifies the end-to-end Phase 1 content import workflow against a temporary SQLite database.
/// </summary>
public sealed class ContentImportServiceTests
{
    /// <summary>
    /// Verifies that a valid package imports one new word and that the imported word becomes queryable.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldImportValidPackage()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreateValidPackageJson("a1-shopping-import-test"));

            ServiceCollection services = new();
            services
                .AddDarwinLinguaInfrastructure(options => options.DatabasePath = databasePath)
                .AddCatalogApplication()
                .AddCatalogInfrastructure()
                .AddContentOpsApplication()
                .AddContentOpsInfrastructure()
                .AddLocalizationApplication()
                .AddLocalizationInfrastructure();

            serviceProvider = services.BuildServiceProvider();

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal("Completed", result.Status);
            Assert.Equal(1, result.TotalEntries);
            Assert.Equal(1, result.ImportedEntries);
            Assert.Equal(0, result.SkippedDuplicateEntries);
            Assert.Equal(0, result.InvalidEntries);

            IWordQueryService wordQueryService = serviceProvider.GetRequiredService<IWordQueryService>();
            IReadOnlyList<DarwinLingua.Catalog.Application.Models.WordListItemModel> words = await wordQueryService
                .GetWordsByTopicAsync("shopping", "en", CancellationToken.None);

            DarwinLingua.Catalog.Application.Models.WordListItemModel importedWord = Assert.Single(words);
            Assert.Equal("Brot", importedWord.Lemma);
            Assert.Equal("bread", importedWord.PrimaryMeaning);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    /// <summary>
    /// Verifies that the same package identifier cannot be imported twice.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldRejectDuplicatePackageId()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreateValidPackageJson("a1-shopping-import-duplicate-package"));

            ServiceCollection services = new();
            services
                .AddDarwinLinguaInfrastructure(options => options.DatabasePath = databasePath)
                .AddCatalogApplication()
                .AddCatalogInfrastructure()
                .AddContentOpsApplication()
                .AddContentOpsInfrastructure()
                .AddLocalizationApplication()
                .AddLocalizationInfrastructure();

            serviceProvider = services.BuildServiceProvider();

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();

            ImportContentPackageResult firstResult = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);
            ImportContentPackageResult secondResult = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(firstResult.IsSuccess);
            Assert.False(secondResult.IsSuccess);
            Assert.Equal("Failed", secondResult.Status);
            Assert.Contains(secondResult.Issues, issue => issue.Message.Contains("already exists", StringComparison.Ordinal));
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    private static string CreateValidPackageJson(string packageId)
    {
        return $$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "A1 Shopping Import Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "entries": [
                {
                  "word": "Brot",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "das",
                  "plural": "Brote",
                  "topics": ["shopping"],
                  "meanings": [
                    {
                      "language": "en",
                      "text": "bread"
                    }
                  ],
                  "examples": [
                    {
                      "baseText": "Ich kaufe Brot.",
                      "translations": [
                        {
                          "language": "en",
                          "text": "I buy bread."
                        }
                      ]
                    }
                  ]
                }
              ]
            }
            """;
    }
}

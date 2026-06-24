using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using DarwinLingua.ContentOps.Infrastructure.Services;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

public sealed class ContentImportParserExpressionEntryTests
{
    [Fact]
    public async Task ParseAsync_ShouldParseOfficialExpressionPilotPackage()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection()
            .AddContentOpsInfrastructure()
            .BuildServiceProvider();

        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();
        string packagePath = Path.Combine(
            ResolveRepositoryRoot(),
            "content",
            "learning-portal",
            "expressions",
            "packages",
            "expressions-a1-a2-core-pilot-v1.json");

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            await File.ReadAllTextAsync(packagePath, CancellationToken.None),
            CancellationToken.None);

        Assert.Equal("expressions-a1-a2-core-pilot-v1", parsedPackage.PackageId);
        Assert.Equal(12, parsedPackage.ExpressionEntries.Count);

        string[] requiredLanguages = ["en", "fa", "ar", "tr", "ru", "ckb", "kmr", "pl", "ro", "sq"];
        foreach (ParsedExpressionEntryModel expression in parsedPackage.ExpressionEntries)
        {
            Assert.Equal(requiredLanguages, expression.Meanings.Select(meaning => meaning.Language).ToArray());
            Assert.NotEmpty(expression.Examples);
            Assert.False(string.IsNullOrWhiteSpace(expression.SensitiveContentKind));
            Assert.False(string.IsNullOrWhiteSpace(expression.UsagePolicy));
            Assert.All(expression.Examples, example =>
                Assert.Equal(requiredLanguages, example.Translations.Select(translation => translation.Language).ToArray()));
            Assert.All(expression.LinkedWords, word =>
            {
                Assert.False(string.IsNullOrWhiteSpace(word.Lemma));
                Assert.Null(word.WordSlug);
            });
        }

        ParsedExpressionEntryModel riskyExpression = Assert.Single(parsedPackage.ExpressionEntries, expression => expression.IsRisky);
        Assert.Equal("na-ja", riskyExpression.Slug);
        Assert.NotEmpty(riskyExpression.Warnings);
        Assert.Equal(requiredLanguages, Assert.Single(riskyExpression.Warnings).Translations.Select(translation => translation.Language).ToArray());
    }

    [Fact]
    public async Task ParseAsync_ShouldParseAllOfficialExpressionPackages()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection()
            .AddContentOpsInfrastructure()
            .BuildServiceProvider();

        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();
        string packagesPath = Path.Combine(
            ResolveRepositoryRoot(),
            "content",
            "learning-portal",
            "expressions",
            "packages");

        string[] requiredLanguages = ["en", "fa", "ar", "tr", "ru", "ckb", "kmr", "pl", "ro", "sq"];
        string[] packagePaths = Directory.GetFiles(packagesPath, "*.json", SearchOption.TopDirectoryOnly);

        Assert.NotEmpty(packagePaths);

        foreach (string packagePath in packagePaths)
        {
            ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
                await File.ReadAllTextAsync(packagePath, CancellationToken.None),
                CancellationToken.None);

            Assert.NotEmpty(parsedPackage.ExpressionEntries);
            Assert.All(parsedPackage.ExpressionEntries, expression =>
            {
                Assert.Equal(requiredLanguages, expression.Meanings.Select(meaning => meaning.Language).ToArray());
                Assert.NotEmpty(expression.Examples);
                Assert.False(string.IsNullOrWhiteSpace(expression.SensitiveContentKind));
                Assert.False(string.IsNullOrWhiteSpace(expression.UsagePolicy));
                Assert.All(expression.Examples, example =>
                    Assert.Equal(requiredLanguages, example.Translations.Select(translation => translation.Language).ToArray()));
                Assert.All(expression.LinkedWords, word => Assert.False(string.IsNullOrWhiteSpace(word.Lemma)));
            });
        }
    }

    [Fact]
    public async Task ParseAsync_ShouldParseExpressionEntryContract()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection()
            .AddContentOpsInfrastructure()
            .BuildServiceProvider();

        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            """
            {
              "packageVersion": "1.0",
              "packageId": "expression-contract-test",
              "packageName": "Expression Contract Test",
              "targetLearningLanguageCode": "de",
              "levelSystemCode": "cefr",
              "defaultMeaningLanguages": ["en"],
              "entries": [],
              "expressionEntries": [
                {
                  "slug": "a2-alles-klar",
                  "expressionText": "Alles klar.",
                  "literalMeaningText": "Everything clear.",
                  "actualMeaningText": "All good or understood.",
                  "usageExplanation": "Used to confirm understanding or agreement.",
                  "meaningTransparency": "pragmatic-formula",
                  "teachingReason": "It is a conventional confirmation formula.",
                  "safetyRating": "general",
                  "minimumAge": 0,
                  "requiresAdultAccess": false,
                  "sensitiveContentKind": "none",
                  "requiresSensitiveOptIn": false,
                  "requiresVerifiedAdult": false,
                  "usagePolicy": "safe-to-use",
                  "cefrLevel": "A2",
                  "expressionType": "fixed-expression",
                  "register": "neutral",
                  "category": "daily-life",
                  "region": "de",
                  "topics": ["daily-life"],
                  "isPublished": true,
                  "sortOrder": 10,
                  "meanings": [
                    {
                      "language": "en",
                      "actualMeaningText": "All good or understood.",
                      "literalMeaningText": "Everything clear.",
                      "usageExplanation": "A neutral confirmation phrase."
                    }
                  ],
                  "examples": [
                    {
                      "germanText": "Alles klar, wir treffen uns um acht.",
                      "translations": [
                        { "language": "en", "text": "All good, we meet at eight." }
                      ],
                      "sortOrder": 10
                    }
                  ],
                  "warnings": [
                    {
                      "warningType": "tone",
                      "text": "Avoid in very formal letters.",
                      "translations": [
                        { "language": "en", "text": "Avoid in very formal letters." }
                      ]
                    }
                  ],
                  "linkedWords": [
                    { "lemma": "klar", "wordSlug": "klar", "sortOrder": 10 }
                  ],
                  "relatedExpressionSlugs": ["verstanden"],
                  "linkedExerciseSlugs": ["a2-confirmation-phrases"]
                }
              ]
            }
            """,
            CancellationToken.None);

        ParsedExpressionEntryModel expression = Assert.Single(parsedPackage.ExpressionEntries);
        Assert.Equal("a2-alles-klar", expression.Slug);
        Assert.Equal("fixed-expression", expression.ExpressionType);
        Assert.Equal("neutral", expression.Register);
        Assert.Equal("pragmatic-formula", expression.MeaningTransparency);
        Assert.Equal("It is a conventional confirmation formula.", expression.TeachingReason);
        Assert.Equal("general", expression.SafetyRating);
        Assert.Equal(0, expression.MinimumAge);
        Assert.False(expression.RequiresAdultAccess);
        Assert.Equal("none", expression.SensitiveContentKind);
        Assert.False(expression.RequiresSensitiveOptIn);
        Assert.False(expression.RequiresVerifiedAdult);
        Assert.Equal("safe-to-use", expression.UsagePolicy);
        Assert.Equal("All good or understood.", Assert.Single(expression.Meanings).ActualMeaningText);
        Assert.Equal("Alles klar, wir treffen uns um acht.", Assert.Single(expression.Examples).GermanText);
        Assert.Equal("klar", Assert.Single(expression.LinkedWords).WordSlug);
        Assert.Equal("verstanden", Assert.Single(expression.RelatedExpressionSlugs));
    }

    private static string ResolveRepositoryRoot()
    {
        DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string candidateSolutionPath = Path.Combine(currentDirectory.FullName, "DarwinLingua.slnx");
            if (File.Exists(candidateSolutionPath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to resolve the repository root.");
    }
}

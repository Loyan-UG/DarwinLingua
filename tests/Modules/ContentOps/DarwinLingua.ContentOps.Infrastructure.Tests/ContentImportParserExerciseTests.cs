using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

public sealed class ContentImportParserExerciseTests
{
    [Fact]
    public async Task ParseAsync_ShouldParseExerciseContract()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection().AddContentOpsInfrastructure().BuildServiceProvider();
        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            """
            {
              "packageVersion": "1.0",
              "packageId": "exercise-contract-test",
              "packageName": "Exercise Contract Test",
              "defaultMeaningLanguages": ["en"],
              "entries": [],
              "exercises": [
                {
                  "slug": "a1-article-der",
                  "title": "Choose the article",
                  "instruction": "Choose the correct article.",
                  "cefrLevel": "A1",
                  "exerciseType": "article-selection",
                  "targetSkill": "grammar",
                  "ownerType": "grammar-topic",
                  "ownerSlug": "a1-definite-articles",
                  "prompt": { "options": [{ "id": "der", "text": "der" }] },
                  "answerKey": { "correctOptionIds": ["der"] },
                  "correctExplanation": "Der is correct.",
                  "incorrectExplanation": "Review masculine nouns.",
                  "hint": "Kaffee is masculine.",
                  "sortOrder": 10
                }
              ],
              "exerciseSets": [
                {
                  "slug": "a1-articles-practice",
                  "title": "Articles Practice",
                  "description": "Practice definite articles.",
                  "cefrLevel": "A1",
                  "ownerType": "grammar-topic",
                  "ownerSlug": "a1-definite-articles",
                  "exerciseSlugs": ["a1-article-der"],
                  "sortOrder": 10
                }
              ]
            }
            """,
            CancellationToken.None);

        ParsedExerciseModel exercise = Assert.Single(parsedPackage.Exercises);
        Assert.Equal("a1-article-der", exercise.Slug);
        Assert.Contains("correctOptionIds", exercise.AnswerKeyJson, StringComparison.Ordinal);
        Assert.Equal("a1-article-der", Assert.Single(Assert.Single(parsedPackage.ExerciseSets).ExerciseSlugs));
    }
}

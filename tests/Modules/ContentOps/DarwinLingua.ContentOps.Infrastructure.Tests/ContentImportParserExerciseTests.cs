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
              "targetLearningLanguageCode": "de",
              "levelSystemCode": "cefr",
              "defaultMeaningLanguages": ["en"],
              "entries": [],
              "exercises": [
                {
                  "slug": "a1-article-der",
                  "title": "Choose the article",
                  "titleTranslations": [{ "language": "fa", "text": "انتخاب حرف تعریف" }],
                  "instruction": "Choose the correct article.",
                  "instructionTranslations": [{ "language": "fa", "text": "حرف تعریف درست را انتخاب کن." }],
                  "cefrLevel": "A1",
                  "exerciseType": "article-selection",
                  "targetSkill": "grammar",
                  "ownerType": "grammar-topic",
                  "ownerSlug": "a1-definite-articles",
                  "prompt": { "options": [{ "id": "der", "text": "der" }] },
                  "answerKey": { "correctOptionIds": ["der"] },
                  "correctExplanation": "Der is correct.",
                  "correctExplanationTranslations": [{ "language": "fa", "text": "گزینه der درست است." }],
                  "incorrectExplanation": "Review masculine nouns.",
                  "incorrectExplanationTranslations": [{ "language": "fa", "text": "اسم‌های مذکر را مرور کن." }],
                  "hint": "Kaffee is masculine.",
                  "hintTranslations": [{ "language": "fa", "text": "Kaffee مذکر است." }],
                  "sortOrder": 10
                }
              ],
              "exerciseSets": [
                {
                  "slug": "a1-articles-practice",
                  "title": "Articles Practice",
                  "titleTranslations": [{ "language": "fa", "text": "تمرین حرف تعریف" }],
                  "description": "Practice definite articles.",
                  "descriptionTranslations": [{ "language": "fa", "text": "حرف‌های تعریف معین را تمرین کن." }],
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
        Assert.Equal("fa", Assert.Single(exercise.TitleTranslations).Language);
        Assert.Equal("fa", Assert.Single(exercise.InstructionTranslations).Language);
        Assert.Equal("fa", Assert.Single(exercise.CorrectExplanationTranslations).Language);
        Assert.Equal("fa", Assert.Single(exercise.IncorrectExplanationTranslations).Language);
        Assert.Equal("fa", Assert.Single(exercise.HintTranslations).Language);
        Assert.Contains("correctOptionIds", exercise.AnswerKeyJson, StringComparison.Ordinal);
        ParsedExerciseSetModel set = Assert.Single(parsedPackage.ExerciseSets);
        Assert.Equal("fa", Assert.Single(set.TitleTranslations).Language);
        Assert.Equal("fa", Assert.Single(set.DescriptionTranslations).Language);
        Assert.Equal("a1-article-der", Assert.Single(set.ExerciseSlugs));
    }
}

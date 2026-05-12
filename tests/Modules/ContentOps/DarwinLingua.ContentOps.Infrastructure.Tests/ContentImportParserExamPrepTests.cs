using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

public sealed class ContentImportParserExamPrepTests
{
    [Fact]
    public async Task ParseAsync_ShouldParseExamPrepContract()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection().AddContentOpsInfrastructure().BuildServiceProvider();
        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            """
            {
              "packageVersion": "1.0",
              "packageId": "exam-prep-contract-test",
              "packageName": "Exam Prep Contract Test",
              "defaultMeaningLanguages": ["en"],
              "entries": [],
              "examProfiles": [
                {
                  "key": "goethe-a2",
                  "displayName": "Goethe A2",
                  "cefrRange": "A2",
                  "description": "Goethe A2 preparation.",
                  "sortOrder": 10
                }
              ],
              "examPrepUnits": [
                {
                  "slug": "a2-goethe-speaking-roleplay",
                  "examProfileKey": "goethe-a2",
                  "title": "Speaking roleplay strategy",
                  "shortDescription": "Prepare a short A2 roleplay.",
                  "cefrLevel": "A2",
                  "examSection": "speaking",
                  "taskType": "roleplay",
                  "skillFocus": "exam-preparation",
                  "explanation": "Use short, clear questions and answers.",
                  "strategyNotes": ["Ask for clarification when needed."],
                  "checklist": ["Answer the prompt directly."],
                  "linkedDialogueSlugs": ["a2-appointment-roleplay"],
                  "linkedTalkTopicSlugs": ["a2-appointments"],
                  "linkedGrammarTopicSlugs": ["a2-question-word-order"],
                  "linkedExpressionSlugs": ["koennten-sie-bitte"],
                  "linkedWritingTemplateSlugs": ["a2-appointment-reschedule"],
                  "linkedExerciseSlugs": ["a2-speaking-roleplay-practice"],
                  "linkedCourseLessonSlugs": ["a2-appointments-lesson"],
                  "sortOrder": 20
                }
              ]
            }
            """,
            CancellationToken.None);

        ParsedExamProfileModel profile = Assert.Single(parsedPackage.ExamProfiles);
        ParsedExamPrepUnitModel unit = Assert.Single(parsedPackage.ExamPrepUnits);
        Assert.Equal("goethe-a2", profile.Key);
        Assert.Equal("a2-goethe-speaking-roleplay", unit.Slug);
        Assert.Equal("a2-speaking-roleplay-practice", Assert.Single(unit.LinkedExerciseSlugs));
    }
}

using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

public sealed class ContentImportParserCourseTests
{
    [Fact]
    public async Task ParseAsync_ShouldParseCourseContract()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection().AddContentOpsInfrastructure().BuildServiceProvider();
        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            """
            {
              "packageVersion": "1.0",
              "packageId": "course-contract-test",
              "packageName": "Course Contract Test",
              "defaultMeaningLanguages": ["en"],
              "entries": [],
              "coursePaths": [
                {
                  "slug": "a1-german",
                  "title": "A1 German",
                  "description": "Structured A1 learning path.",
                  "cefrLevel": "A1",
                  "sortOrder": 10
                }
              ],
              "courseModules": [
                {
                  "slug": "a1-1",
                  "coursePathSlug": "a1-german",
                  "title": "A1.1",
                  "description": "First A1 module.",
                  "moduleNumber": 1,
                  "cefrLevel": "A1",
                  "sortOrder": 10
                }
              ],
              "courseLessons": [
                {
                  "slug": "a1-first-steps",
                  "coursePathSlug": "a1-german",
                  "moduleSlug": "a1-1",
                  "lessonNumber": 1,
                  "title": "First steps",
                  "shortDescription": "Start with greetings and first words.",
                  "narrative": "This lesson links to existing learning content instead of duplicating it.",
                  "cefrLevel": "A1",
                  "estimatedMinutes": 25,
                  "learningGoals": ["Greet someone", "Find beginner words"],
                  "linkedGrammarTopicSlugs": ["a1-word-order"],
                  "linkedWordSlugs": ["hallo"],
                  "linkedExpressionSlugs": ["guten-morgen"],
                  "linkedDialogueSlugs": ["a1-introductions"],
                  "linkedTalkTopicSlugs": ["a1-greetings"],
                  "linkedExerciseSetSlugs": ["a1-greetings-practice"],
                  "sortOrder": 10
                }
              ]
            }
            """,
            CancellationToken.None);

        ParsedCoursePathModel course = Assert.Single(parsedPackage.CoursePaths);
        Assert.Equal("a1-german", course.Slug);
        Assert.Equal("a1-1", Assert.Single(parsedPackage.CourseModules).Slug);
        ParsedCourseLessonModel lesson = Assert.Single(parsedPackage.CourseLessons);
        Assert.Equal("a1-first-steps", lesson.Slug);
        Assert.Equal("Greet someone", lesson.LearningGoals[0]);
    }
}

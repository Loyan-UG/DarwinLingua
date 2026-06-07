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
                  "title": "A1 Deutsch",
                  "titleTranslations": [{ "language": "en", "text": "A1 German" }],
                  "description": "Strukturierter A1-Lernpfad.",
                  "descriptionTranslations": [{ "language": "en", "text": "Structured A1 learning path." }],
                  "cefrLevel": "A1",
                  "sortOrder": 10
                }
              ],
              "courseModules": [
                {
                  "slug": "a1-1",
                  "coursePathSlug": "a1-german",
                  "title": "A1.1",
                  "titleTranslations": [{ "language": "en", "text": "A1.1" }],
                  "description": "Erstes A1-Modul.",
                  "descriptionTranslations": [{ "language": "en", "text": "First A1 module." }],
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
                  "title": "Erste Schritte",
                  "titleTranslations": [{ "language": "en", "text": "First steps" }],
                  "shortDescription": "Starte mit Begruessungen und ersten Woertern.",
                  "shortDescriptionTranslations": [{ "language": "en", "text": "Start with greetings and first words." }],
                  "narrative": "Diese Lektion verlinkt vorhandene Lerninhalte, statt sie zu duplizieren.",
                  "narrativeTranslations": [{ "language": "en", "text": "This lesson links to existing learning content instead of duplicating it." }],
                  "cefrLevel": "A1",
                  "estimatedMinutes": 25,
                  "learningGoals": ["Jemanden begruessen", "Anfaengerwoerter finden"],
                  "learningGoalsTranslations": [{ "language": "en", "texts": ["Greet someone", "Find beginner words"] }],
                  "linkedGrammarTopicSlugs": ["a1-word-order"],
                  "linkedWordSlugs": ["hallo"],
                  "linkedExpressionSlugs": ["guten-morgen"],
                  "linkedDialogueSlugs": ["a1-introductions"],
                  "linkedTalkTopicSlugs": ["a1-greetings"],
                  "linkedExerciseSetSlugs": ["a1-greetings-practice"],
                  "reviewSummary": "Wiederhole die Begruessung.",
                  "reviewSummaryTranslations": [{ "language": "en", "text": "Repeat the greeting." }],
                  "homeworkTask": "Schreibe zwei kurze Saetze.",
                  "homeworkTaskTranslations": [{ "language": "en", "text": "Write two short sentences." }],
                  "sortOrder": 10
                }
              ]
            }
            """,
            CancellationToken.None);

        ParsedCoursePathModel course = Assert.Single(parsedPackage.CoursePaths);
        Assert.Equal("a1-german", course.Slug);
        Assert.Equal("A1 German", Assert.Single(course.TitleTranslations).Text);
        Assert.Equal("a1-1", Assert.Single(parsedPackage.CourseModules).Slug);
        ParsedCourseLessonModel lesson = Assert.Single(parsedPackage.CourseLessons);
        Assert.Equal("a1-first-steps", lesson.Slug);
        Assert.Equal("Jemanden begruessen", lesson.LearningGoals[0]);
        Assert.Equal("Greet someone", Assert.Single(lesson.LearningGoalsTranslations).Texts[0]);
    }
}

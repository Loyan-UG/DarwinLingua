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
                  "displayNameTranslations": [
                    { "language": "en", "text": "Goethe A2 exam profile" },
                    { "language": "fa", "text": "پروفایل آزمون گوته A2" }
                  ],
                  "cefrRange": "A2",
                  "description": "Vorbereitung auf die Goethe-A2-Pruefung.",
                  "descriptionTranslations": [
                    { "language": "en", "text": "Goethe A2 exam preparation" },
                    { "language": "fa", "text": "آمادگی آزمون گوته A2" }
                  ],
                  "sortOrder": 10
                }
              ],
              "examPrepUnits": [
                {
                  "slug": "a2-goethe-speaking-roleplay",
                  "examProfileKey": "goethe-a2",
                  "title": "Strategie fuer das Sprechrollenspiel",
                  "titleTranslations": [
                    { "language": "en", "text": "Speaking roleplay strategy" },
                    { "language": "fa", "text": "راهبرد نقش‌آفرینی شفاهی" }
                  ],
                  "shortDescription": "Bereite ein kurzes A2-Rollenspiel vor.",
                  "shortDescriptionTranslations": [
                    { "language": "en", "text": "Prepare a short A2 roleplay" },
                    { "language": "fa", "text": "یک نقش‌آفرینی کوتاه A2 را آماده کن" }
                  ],
                  "cefrLevel": "A2",
                  "examSection": "speaking",
                  "taskType": "roleplay",
                  "skillFocus": "exam-preparation",
                  "explanation": "Nutze kurze, klare Fragen und Antworten.",
                  "explanationTranslations": [
                    { "language": "en", "text": "Use short, clear questions and answers" },
                    { "language": "fa", "text": "از پرسش‌ها و پاسخ‌های کوتاه و روشن استفاده کن" }
                  ],
                  "strategyNotes": ["Ask for clarification when needed."],
                  "strategyNotesTranslations": [
                    { "language": "en", "texts": ["Ask for clarification when needed"] },
                    { "language": "fa", "texts": ["وقتی لازم است، درخواست توضیح بیشتر کن"] }
                  ],
                  "checklist": ["Answer the prompt directly."],
                  "checklistTranslations": [
                    { "language": "en", "texts": ["Answer the prompt directly"] },
                    { "language": "fa", "texts": ["به موضوع خواسته‌شده مستقیم پاسخ بده"] }
                  ],
                  "linkedDialogueSlugs": ["a2-appointment-roleplay"],
                  "linkedTalkTopicSlugs": ["a2-appointments"],
                  "linkedGrammarTopicSlugs": ["a2-question-word-order"],
                  "linkedExpressionSlugs": ["koennten-sie-bitte"],
                  "linkedWritingTemplateSlugs": ["a2-appointment-reschedule"],
                  "linkedExerciseSlugs": ["a2-speaking-roleplay-practice"],
                  "linkedRoleplaySlugs": ["a2-termin-verschieben-am-telefon"],
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
        Assert.Equal("fa", profile.DisplayNameTranslations[1].Language);
        Assert.Equal("a2-goethe-speaking-roleplay", unit.Slug);
        Assert.Equal("fa", unit.ExplanationTranslations[1].Language);
        Assert.Equal("وقتی لازم است، درخواست توضیح بیشتر کن", unit.StrategyNotesTranslations[1].Texts[0]);
        Assert.Equal("a2-speaking-roleplay-practice", Assert.Single(unit.LinkedExerciseSlugs));
        Assert.Equal("a2-termin-verschieben-am-telefon", Assert.Single(unit.LinkedRoleplaySlugs));
    }
}

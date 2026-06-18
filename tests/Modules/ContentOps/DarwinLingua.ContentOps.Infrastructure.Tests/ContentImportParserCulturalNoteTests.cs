using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

public sealed class ContentImportParserCulturalNoteTests
{
    [Fact]
    public async Task ParseAsync_ShouldParseCulturalNoteContract()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection().AddContentOpsInfrastructure().BuildServiceProvider();
        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            """
            {
              "packageVersion": "1.0",
              "packageId": "cultural-note-contract-test",
              "packageName": "Cultural Note Contract Test",
              "defaultMeaningLanguages": ["en"],
              "entries": [],
              "culturalNotes": [
                {
                  "slug": "a2-du-vs-sie-at-work",
                  "title": "Du vs. Sie at work",
                  "titleTranslations": [
                    { "language": "fa", "text": "تو یا شما در محیط کار" }
                  ],
                  "shortDescription": "A practical note about choosing address forms.",
                  "shortDescriptionTranslations": [
                    { "language": "fa", "text": "یادداشتی کاربردی درباره انتخاب شکل خطاب کردن." }
                  ],
                  "cefrLevel": "A2",
                  "category": "du-vs-sie",
                  "context": "Workplace introductions",
                  "contextTranslations": [
                    { "language": "fa", "text": "معرفی در محیط کار" }
                  ],
                  "sections": ["Use Sie until a colleague offers du."],
                  "sectionsTranslations": [
                    { "language": "fa", "items": ["تا وقتی همکار خودش du را پیشنهاد نکرده، از Sie استفاده کن."] }
                  ],
                  "examples": [
                    {
                      "germanText": "Sollen wir uns duzen?",
                      "explanation": "A polite way to ask about switching to du.",
                      "explanationTranslations": [
                        { "language": "fa", "text": "راهی محترمانه برای پرسیدن درباره تغییر خطاب به du." }
                      ]
                    }
                  ],
                  "doNotes": ["Start with Sie in formal settings."],
                  "doNotesTranslations": [
                    { "language": "fa", "items": ["در موقعیت رسمی با Sie شروع کن."] }
                  ],
                  "dontNotes": ["Do not switch to du automatically."],
                  "dontNotesTranslations": [
                    { "language": "fa", "items": ["خودکار به du تغییر نده."] }
                  ],
                  "sensitivityWarning": "Address forms can feel personal in hierarchical contexts.",
                  "sensitivityWarningTranslations": [
                    { "language": "fa", "text": "در موقعیت‌های سلسله‌مراتبی، شکل خطاب کردن می‌تواند جنبه شخصی پیدا کند." }
                  ],
                  "linkedDialogueSlugs": ["a2-workplace-introduction"],
                  "linkedExpressionSlugs": ["sollen-wir-uns-duzen"],
                  "linkedWritingTemplateSlugs": ["a2-formal-work-email"],
                  "linkedTalkTopicSlugs": ["a2-workplace-small-talk"],
                  "linkedCourseLessonSlugs": ["a2-workplace-communication"],
                  "sortOrder": 10
                }
              ]
            }
            """,
            CancellationToken.None);

        ParsedCulturalNoteModel note = Assert.Single(parsedPackage.CulturalNotes);
        Assert.Equal("a2-du-vs-sie-at-work", note.Slug);
        Assert.Equal("du-vs-sie", note.Category);
        ParsedCulturalNoteExampleModel example = Assert.Single(note.Examples);
        Assert.Equal("Sollen wir uns duzen?", example.GermanText);
        Assert.Equal("تو یا شما در محیط کار", Assert.Single(note.TitleTranslations).Text);
        Assert.Equal("در موقعیت رسمی با Sie شروع کن.", Assert.Single(note.DoNotesTranslations).Items[0]);
        Assert.Equal("راهی محترمانه برای پرسیدن درباره تغییر خطاب به du.", Assert.Single(example.ExplanationTranslations).Text);
        Assert.Equal("a2-formal-work-email", Assert.Single(note.LinkedWritingTemplateSlugs));
    }
}

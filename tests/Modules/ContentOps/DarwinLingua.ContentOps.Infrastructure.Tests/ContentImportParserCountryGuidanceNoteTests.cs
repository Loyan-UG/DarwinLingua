using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

public sealed class ContentImportParserCountryGuidanceNoteTests
{
    [Fact]
    public async Task ParseAsync_ShouldParseCountryGuidanceNoteContract()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection().AddContentOpsInfrastructure().BuildServiceProvider();
        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            """
            {
              "packageVersion": "1.0",
              "packageId": "country-guidance-note-contract-test",
              "packageName": "Country Guidance Note Contract Test",
              "targetLearningLanguageCode": "de",
              "levelSystemCode": "cefr",
              "countryContextCode": "DE",
              "defaultMeaningLanguages": ["en"],
              "entries": [],
              "countryGuidanceNotes": [
                {
                  "slug": "a2-du-vs-sie-at-work",
                  "title": "Du vs. Sie am Arbeitsplatz",
                  "titleTranslations": [
                    { "language": "fa", "text": "خطاب کردن با du یا Sie در محیط کار" }
                  ],
                  "shortDescription": "Eine praktische Orientierung zur passenden Anrede.",
                  "shortDescriptionTranslations": [
                    { "language": "fa", "text": "راهنمایی کاربردی برای انتخاب شکل خطاب مناسب." }
                  ],
                  "cefrLevel": "A2",
                  "category": "du-vs-sie",
                  "context": "Vorstellung im Arbeitsumfeld",
                  "contextTranslations": [
                    { "language": "fa", "text": "معرفی در فضای کاری" }
                  ],
                  "sections": ["Nutze Sie, bis eine Kollegin oder ein Kollege das du anbietet."],
                  "sectionsTranslations": [
                    { "language": "fa", "items": ["تا وقتی همکار خودش du را پیشنهاد نکرده، از Sie استفاده کن."] }
                  ],
                  "examples": [
                    {
                      "germanText": "Sollen wir uns duzen?",
                      "explanation": "Eine höfliche Frage, wenn du zur informellen Anrede wechseln möchtest.",
                      "explanationTranslations": [
                        { "language": "fa", "text": "پرسشی محترمانه وقتی می‌خواهی به شکل خطاب غیررسمی بروی." }
                      ]
                    }
                  ],
                  "doNotes": ["Beginne in formellen Situationen mit Sie."],
                  "doNotesTranslations": [
                    { "language": "fa", "items": ["در موقعیت رسمی با Sie شروع کن."] }
                  ],
                  "dontNotes": ["Wechsle nicht automatisch zu du."],
                  "dontNotesTranslations": [
                    { "language": "fa", "items": ["خودکار به du تغییر نده."] }
                  ],
                  "sensitivityWarning": "Anredeformen können in hierarchischen Situationen persönlich wirken.",
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

        Assert.Equal("de", parsedPackage.TargetLearningLanguageCode);
        Assert.Equal("cefr", parsedPackage.LevelSystemCode);
        Assert.Equal("DE", parsedPackage.CountryContextCode);

        ParsedCountryGuidanceNoteModel note = Assert.Single(parsedPackage.CountryGuidanceNotes);
        Assert.Equal("a2-du-vs-sie-at-work", note.Slug);
        Assert.Equal("du-vs-sie", note.Category);
        ParsedCountryGuidanceNoteExampleModel example = Assert.Single(note.Examples);
        Assert.Equal("Sollen wir uns duzen?", example.GermanText);
        Assert.Equal("خطاب کردن با du یا Sie در محیط کار", Assert.Single(note.TitleTranslations).Text);
        Assert.Equal("در موقعیت رسمی با Sie شروع کن.", Assert.Single(note.DoNotesTranslations).Items[0]);
        Assert.Equal("پرسشی محترمانه وقتی می‌خواهی به شکل خطاب غیررسمی بروی.", Assert.Single(example.ExplanationTranslations).Text);
        Assert.Equal("a2-formal-work-email", Assert.Single(note.LinkedWritingTemplateSlugs));
    }
}

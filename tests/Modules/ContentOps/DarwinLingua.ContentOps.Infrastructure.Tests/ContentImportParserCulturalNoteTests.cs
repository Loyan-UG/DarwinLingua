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
                  "shortDescription": "A practical note about choosing address forms.",
                  "cefrLevel": "A2",
                  "category": "du-vs-sie",
                  "context": "Workplace introductions",
                  "sections": ["Use Sie until a colleague offers du."],
                  "examples": [
                    {
                      "germanText": "Sollen wir uns duzen?",
                      "explanation": "A polite way to ask about switching to du."
                    }
                  ],
                  "doNotes": ["Start with Sie in formal settings."],
                  "dontNotes": ["Do not switch to du automatically."],
                  "sensitivityWarning": "Address forms can feel personal in hierarchical contexts.",
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
        Assert.Equal("Sollen wir uns duzen?", Assert.Single(note.Examples).GermanText);
        Assert.Equal("a2-formal-work-email", Assert.Single(note.LinkedWritingTemplateSlugs));
    }
}

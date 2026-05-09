using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

public sealed class ContentImportParserEventPreparationTests
{
    [Fact]
    public async Task ParseAsync_ShouldParseEventPreparationPackContracts()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection()
            .AddContentOpsInfrastructure()
            .BuildServiceProvider();

        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            """
            {
              "packageVersion": "1.0",
              "packageId": "event-preparation-contract-test",
              "packageName": "Event Preparation Contract Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en", "fa"],
              "entries": [],
              "eventPreparationPacks": [
                {
                  "slug": "a1-conversation-cafe-first-visit",
                  "title": "First Conversation Cafe Visit",
                  "description": "Prepare for a friendly beginner conversation cafe.",
                  "cefrLevel": "A1",
                  "category": "conversation-cafe",
                  "eventType": "conversation-cafe",
                  "topics": ["everyday-life"],
                  "sortOrder": 10,
                  "LinkedDialogueSlugs": ["a1-buy-bread-at-bakery"],
                  "linkedConversationStarterPackSlugs": ["a1-cafe-first-meeting-starters"],
                  "linkedVocabulary": [
                    { "word": "Hilfe", "partOfSpeech": "Noun", "cefrLevel": "A1" }
                  ],
                  "openingPrompts": ["Hallo, ich heiße ..."],
                  "roleplayPrompts": ["Introduce yourself and ask one simple question."],
                  "reviewPrompts": ["Which phrase did you use successfully?"]
                }
              ]
            }
            """,
            CancellationToken.None);

        ParsedEventPreparationPackModel pack = Assert.Single(parsedPackage.EventPreparationPacks);
        Assert.Equal("a1-conversation-cafe-first-visit", pack.Slug);
        Assert.Equal("A1", pack.CefrLevel);
        Assert.Equal("conversation-cafe", pack.Category);
        Assert.Equal("conversation-cafe", pack.EventType);
        Assert.Equal(["everyday-life"], pack.Topics);
        Assert.Equal(["a1-buy-bread-at-bakery"], pack.LinkedDialogueSlugs);
        Assert.Equal(["a1-cafe-first-meeting-starters"], pack.LinkedConversationStarterPackSlugs);
        Assert.Equal("Hilfe", Assert.Single(pack.LinkedVocabulary).Word);
        Assert.Equal(["Hallo, ich heiße ..."], pack.OpeningPrompts);
        Assert.Equal(["Introduce yourself and ask one simple question."], pack.RoleplayPrompts);
        Assert.Equal(["Which phrase did you use successfully?"], pack.ReviewPrompts);
    }
}

using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

public sealed class ContentImportParserConversationStarterTests
{
    [Fact]
    public async Task ParseAsync_ShouldParseConversationStarterPackContracts()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection()
            .AddContentOpsInfrastructure()
            .BuildServiceProvider();

        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            """
            {
              "packageVersion": "1.0",
              "packageId": "conversation-starter-contract-test",
              "packageName": "Conversation Starter Contract Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en", "fa"],
              "entries": [],
              "conversationStarterPacks": [
                {
                  "slug": "a1-cafe-first-meeting",
                  "title": "Cafe First Meeting",
                  "description": "Simple phrases for a first cafe conversation.",
                  "cefrLevel": "A1",
                  "category": "first-meetings",
                  "situation": "cafe",
                  "tone": "friendly",
                  "conversationGoal": "introduction",
                  "topics": ["everyday-life"],
                  "sortOrder": 10,
                  "linkedScenarioSlugs": ["a1-buy-bread-at-bakery"],
                  "linkedEventPreparationPackSlugs": ["a1-cafe-evening-prep"],
                  "phrases": [
                    {
                      "baseText": "Hallo, ich heiße Sara.",
                      "function": "opening",
                      "register": "neutral",
                      "usageNote": "Use this as a simple first introduction.",
                      "sortOrder": 1,
                      "alternativeBaseTexts": ["Guten Tag, ich heiße Sara."],
                      "commonMistake": "Do not use 'Ich bin heiße'.",
                      "translations": [
                        { "language": "en", "text": "Hello, my name is Sara." },
                        { "language": "fa", "text": "سلام، اسم من سارا است." }
                      ]
                    }
                  ]
                }
              ]
            }
            """,
            CancellationToken.None);

        ParsedConversationStarterPackModel pack = Assert.Single(parsedPackage.ConversationStarterPacks);
        Assert.Equal("a1-cafe-first-meeting", pack.Slug);
        Assert.Equal("A1", pack.CefrLevel);
        Assert.Equal("first-meetings", pack.Category);
        Assert.Equal("cafe", pack.Situation);
        Assert.Equal("friendly", pack.Tone);
        Assert.Equal("introduction", pack.ConversationGoal);
        Assert.Equal(["everyday-life"], pack.Topics);
        Assert.Equal(["a1-buy-bread-at-bakery"], pack.LinkedScenarioSlugs);
        Assert.Equal(["a1-cafe-evening-prep"], pack.LinkedEventPreparationPackSlugs);

        ParsedConversationStarterPhraseModel phrase = Assert.Single(pack.Phrases);
        Assert.Equal("Hallo, ich heiße Sara.", phrase.BaseText);
        Assert.Equal("opening", phrase.Function);
        Assert.Equal("neutral", phrase.Register);
        Assert.Equal(1, phrase.SortOrder);
        Assert.Equal(["Guten Tag, ich heiße Sara."], phrase.AlternativeBaseTexts);
        Assert.Contains(phrase.Translations, translation => translation.Language == "fa");
    }
}

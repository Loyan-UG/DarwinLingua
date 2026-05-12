using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

public sealed class ContentImportParserGrammarTopicTests
{
    [Fact]
    public async Task ParseAsync_ShouldParseGrammarTopicContract()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection()
            .AddContentOpsInfrastructure()
            .BuildServiceProvider();

        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            """
            {
              "packageVersion": "1.0",
              "packageId": "grammar-contract-test",
              "packageName": "Grammar Contract Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "entries": [],
              "grammarTopics": [
                {
                  "slug": "a1-definite-articles",
                  "title": "Definite Articles",
                  "shortDescription": "Learn how der, die, and das mark German noun gender.",
                  "cefrLevel": "A1",
                  "grammarCategory": "articles",
                  "topics": ["daily-life"],
                  "sortOrder": 10,
                  "isPublished": true,
                  "sections": [
                    {
                      "heading": "When to use definite articles",
                      "explanation": "German uses definite articles before known nouns.",
                      "translations": [
                        { "language": "en", "heading": "When to use definite articles", "text": "Use definite articles for known nouns." }
                      ],
                      "sortOrder": 10
                    }
                  ],
                  "examples": [
                    {
                      "germanText": "Der Kaffee ist heiss.",
                      "translations": [{ "language": "en", "text": "The coffee is hot." }],
                      "sortOrder": 10
                    }
                  ],
                  "ruleSummaries": [
                    { "text": "der, die, and das agree with noun gender.", "translations": [{ "language": "en", "text": "Articles agree with noun gender." }], "sortOrder": 10 }
                  ],
                  "commonMistakes": [
                    { "wrongText": "die Kaffee", "correctedText": "der Kaffee", "explanation": "Kaffee is masculine.", "translations": [{ "language": "en", "text": "Kaffee is masculine." }], "sortOrder": 10 }
                  ],
                  "exceptionNotes": [
                    { "text": "Compound nouns take the gender of the last noun.", "sortOrder": 10 }
                  ],
                  "linkedWords": [{ "lemma": "der Kaffee", "wordSlug": "der-kaffee", "sortOrder": 10 }],
                  "linkedDialogueSlugs": ["a1-cafe-order"],
                  "linkedTalkTopicSlugs": ["a1-food-and-drinks"],
                  "linkedExerciseSlugs": ["a1-articles-practice"],
                  "prerequisiteSlugs": [],
                  "relatedTopicSlugs": ["a1-indefinite-articles"]
                }
              ]
            }
            """,
            CancellationToken.None);

        ParsedGrammarTopicModel grammarTopic = Assert.Single(parsedPackage.GrammarTopics);
        Assert.Equal("a1-definite-articles", grammarTopic.Slug);
        Assert.Equal("articles", grammarTopic.GrammarCategory);
        Assert.Equal("daily-life", Assert.Single(grammarTopic.Topics));
        Assert.Equal("When to use definite articles", Assert.Single(grammarTopic.Sections).Heading);
        Assert.Equal("Der Kaffee ist heiss.", Assert.Single(grammarTopic.Examples).GermanText);
        Assert.Equal("der Kaffee", Assert.Single(grammarTopic.LinkedWords).Lemma);
        Assert.Equal("a1-indefinite-articles", Assert.Single(grammarTopic.RelatedTopicSlugs));
    }
}

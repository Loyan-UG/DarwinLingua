using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

public sealed class ContentImportParserTalkTopicTests
{
    [Fact]
    public async Task ParseAsync_ShouldParseTalkTopicContracts()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection()
            .AddContentOpsInfrastructure()
            .BuildServiceProvider();

        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            """
            {
              "packageVersion": "1.0",
              "packageId": "talk-topic-contract-test",
              "packageName": "Talk Topic Contract Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "entries": [],
              "talkTopics": [
                {
                  "slug": "a1-do-aliens-exist",
                  "topicGroupKey": "do-aliens-exist",
                  "title": "Gibt es Ausserirdische?",
                  "description": "A simple Talk Topic about aliens and space.",
                  "cefrLevel": "A1",
                  "category": "science-and-imagination",
                  "topics": ["technology-and-it"],
                  "contentType": "article",
                  "estimatedReadingMinutes": 5,
                  "estimatedDiscussionMinutes": 20,
                  "isSensitive": false,
                  "recommendedForModeratedGroupsOnly": false,
                  "article": {
                    "baseText": "Ein langer deutscher Beispieltext.",
                    "translations": [
                      { "languageCode": "en", "text": "A long German sample text." }
                    ]
                  },
                  "warmupQuestions": [
                    {
                      "prompt": "Schaust du gern in den Nachthimmel?",
                      "translations": [
                        { "languageCode": "en", "text": "Do you like looking at the night sky?" }
                      ],
                      "sortOrder": 10
                    }
                  ],
                  "discussionQuestions": [
                    {
                      "prompt": "Glaubst du, dass es Ausserirdische gibt?",
                      "questionType": "opinion",
                      "translations": [
                        { "languageCode": "en", "text": "Do you believe aliens exist?" }
                      ],
                      "sortOrder": 10
                    }
                  ],
                  "vocabularyItems": [
                    { "lemma": "das Weltall", "wordSlug": "das-weltall", "cefrLevel": "A2", "sortOrder": 10 }
                  ],
                  "speakingGoals": ["express-opinion", "give-reasons"],
                  "sortOrder": 10,
                  "isPublished": true
                }
              ]
            }
            """,
            CancellationToken.None);

        ParsedTalkTopicModel talkTopic = Assert.Single(parsedPackage.TalkTopics);
        Assert.Equal("a1-do-aliens-exist", talkTopic.Slug);
        Assert.Equal("do-aliens-exist", talkTopic.TopicGroupKey);
        Assert.Equal("article", talkTopic.ContentType);
        Assert.Equal(["technology-and-it"], talkTopic.Topics);
        Assert.Equal("Schaust du gern in den Nachthimmel?", Assert.Single(talkTopic.WarmupQuestions).Prompt);
        Assert.Equal("opinion", Assert.Single(talkTopic.DiscussionQuestions).QuestionType);
        Assert.Equal("das Weltall", Assert.Single(talkTopic.VocabularyItems).Lemma);
        Assert.Equal(["express-opinion", "give-reasons"], talkTopic.SpeakingGoals);
    }
}

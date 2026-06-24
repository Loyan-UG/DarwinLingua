using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

public sealed class ContentImportParserRoleplayScenarioTests
{
    [Fact]
    public async Task ParseAsync_ShouldParseRoleplayScenarioContracts()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection()
            .AddContentOpsInfrastructure()
            .BuildServiceProvider();

        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            """
            {
              "packageVersion": "1.0",
              "packageId": "roleplay-contract-test",
              "packageName": "Roleplay Contract Test",
              "targetLearningLanguageCode": "de",
              "levelSystemCode": "cefr",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en", "fa"],
              "entries": [],
              "roleplayScenarios": [
                {
                  "slug": "a1-conversation-cafe-introduction",
                  "linkedDialogueSlug": "cafe-first-meeting-a1",
                  "title": "Sich im Sprachcafe vorstellen",
                  "titleTranslations": [{ "language": "en", "text": "Introduce yourself at a conversation cafe" }],
                  "description": "Uebe eine kurze, gesteuerte Vorstellung im Sprachcafe.",
                  "descriptionTranslations": [{ "language": "en", "text": "Practice a short deterministic conversation cafe introduction." }],
                  "learnerGoal": "Sage deinen Namen und stelle eine einfache Rueckfrage.",
                  "learnerGoalTranslations": [{ "language": "en", "text": "Say your name and ask one simple follow-up question." }],
                  "cefrLevel": "A1",
                  "category": "conversation-cafe",
                  "topics": ["everyday-life"],
                  "examProfiles": ["goethe-a1"],
                  "skillFocus": ["speaking", "roleplay"],
                  "taskType": "introduce-yourself",
                  "interactionMode": "face-to-face",
                  "register": "neutral",
                  "estimatedPracticeMinutes": 8,
                  "roles": [
                    { "roleKey": "learner", "displayName": "Learner", "translations": [{ "language": "en", "text": "Learner" }] },
                    { "roleKey": "partner", "displayName": "Partner", "translations": [{ "language": "en", "text": "Partner" }] }
                  ],
                  "turns": [
                    { "sortOrder": 1, "speakerRole": "partner", "baseText": "Hallo, ich bin Anna.", "translations": [{ "language": "en", "text": "Hello, I am Anna." }] },
                    { "sortOrder": 2, "speakerRole": "learner", "baseText": "Hallo, ich heiße Sam.", "translations": [{ "language": "en", "text": "Hello, my name is Sam." }], "expectedLearnerAction": "introduce-yourself" }
                  ],
                  "answerChoices": [
                    {
                      "turnSortOrder": 2,
                      "choices": [
                        { "id": "a", "text": "Hallo, ich heiße Sam.", "translations": [{ "language": "en", "text": "Hello, my name is Sam." }], "isCorrect": true, "feedback": "Good simple introduction.", "feedbackTranslations": [{ "language": "en", "text": "Good simple introduction." }] },
                        { "id": "b", "text": "Tschüss.", "translations": [{ "language": "en", "text": "Bye." }], "isCorrect": false, "feedback": "This ends the talk too early.", "feedbackTranslations": [{ "language": "en", "text": "This ends the talk too early." }] }
                      ]
                    }
                  ],
                  "staticFeedback": [
                    { "turnSortOrder": 2, "feedbackType": "politeness", "text": "A short greeting is enough at A1.", "translations": [{ "language": "en", "text": "A short greeting is enough at A1." }] }
                  ],
                  "imageSlots": [
                    { "slotKey": "scene", "placement": "header", "purpose": "Show the social setting.", "altText": "People sitting in a conversation cafe.", "altTextTranslations": [{ "language": "en", "text": "People sitting in a conversation cafe." }], "imagePrompt": "Clean educational illustration of adults in a German conversation cafe, no logos, no text.", "assetPath": null, "isRequired": false }
                  ],
                  "isPublished": true,
                  "sortOrder": 10
                }
              ]
            }
            """,
            CancellationToken.None);

        ParsedRoleplayScenarioModel scenario = Assert.Single(parsedPackage.RoleplayScenarios);
        Assert.Equal("a1-conversation-cafe-introduction", scenario.Slug);
        Assert.Equal("cafe-first-meeting-a1", scenario.LinkedDialogueSlug);
        Assert.Equal("Sich im Sprachcafe vorstellen", scenario.Title);
        Assert.Equal("Introduce yourself at a conversation cafe", Assert.Single(scenario.TitleTranslations).Text);
        Assert.Equal(["speaking", "roleplay"], scenario.SkillFocus);
        Assert.Equal(2, scenario.Turns.Count);
        Assert.Equal(2, Assert.Single(scenario.AnswerChoices).Choices.Count);
        Assert.Equal("scene", Assert.Single(scenario.ImageSlots).SlotKey);
    }
}

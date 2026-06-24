using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

public sealed class ContentImportParserDialogueTests
{
    [Fact]
    public async Task ParseAsync_ShouldParseDialogueLessonContracts()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection()
            .AddContentOpsInfrastructure()
            .BuildServiceProvider();

        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            """
            {
              "packageVersion": "1.0",
              "packageId": "Dialogue-contract-test",
              "packageName": "Dialogue Contract Test",
              "targetLearningLanguageCode": "de",
              "levelSystemCode": "cefr",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en", "fa"],
              "entries": [],
              "dialogues": [
                {
                  "slug": "doctor-appointment-a1",
                  "title": "Doctor Appointment",
                  "description": "Prepare for a simple appointment conversation.",
                  "learnerGoal": "Explain that you need an appointment.",
                  "cefrLevel": "A1",
                  "category": "doctor-and-healthcare",
                  "topics": ["appointments-and-health"],
                  "sortOrder": 10,
                  "dialogueTurns": [
                    {
                      "speakerRole": "learner",
                      "baseText": "Ich brauche einen Termin.",
                      "translations": [
                        { "language": "en", "text": "I need an appointment." },
                        { "language": "fa", "text": "من یک وقت ملاقات لازم دارم." }
                      ]
                    }
                  ],
                  "usefulPhrases": [
                    {
                      "baseText": "Können Sie das bitte wiederholen?",
                      "usageNote": "Use when you did not understand.",
                      "translations": [
                        { "language": "en", "text": "Could you please repeat that?" }
                      ]
                    }
                  ],
                  "questions": [
                    {
                      "prompt": "Was braucht die Person?",
                      "translations": [
                        { "language": "en", "text": "What does the person need?" }
                      ],
                      "answers": [
                        {
                          "text": "Einen Termin.",
                          "isCorrect": true,
                          "feedback": "Correct.",
                          "translations": [
                            { "language": "en", "text": "An appointment." }
                          ]
                        },
                        {
                          "text": "Ein Brot.",
                          "isCorrect": false,
                          "feedback": "This belongs to shopping.",
                          "translations": [
                            { "language": "en", "text": "A bread." }
                          ]
                        }
                      ]
                    }
                  ]
                }
              ]
            }
            """,
            CancellationToken.None);

        ParsedDialogueLessonModel Dialogue = Assert.Single(parsedPackage.Dialogues);
        Assert.Equal("doctor-appointment-a1", Dialogue.Slug);
        Assert.Equal("Doctor Appointment", Dialogue.Title);
        Assert.Equal("Explain that you need an appointment.", Dialogue.LearnerGoal);
        Assert.Equal("A1", Dialogue.CefrLevel);
        Assert.Equal("doctor-and-healthcare", Dialogue.Category);
        Assert.Equal(["appointments-and-health"], Dialogue.Topics);

        ParsedDialogueTurnModel turn = Assert.Single(Dialogue.DialogueTurns);
        Assert.Equal("learner", turn.SpeakerRole);
        Assert.Equal("Ich brauche einen Termin.", turn.BaseText);
        Assert.Contains(turn.Translations, translation => translation.Language == "fa");

        ParsedDialoguePhraseModel phrase = Assert.Single(Dialogue.UsefulPhrases);
        Assert.Equal("Use when you did not understand.", phrase.UsageNote);

        ParsedDialogueQuestionModel question = Assert.Single(Dialogue.Questions);
        Assert.Equal(2, question.Answers.Count);
        Assert.Contains(question.Answers, answer => answer.IsCorrect && answer.Text == "Einen Termin.");
    }
}

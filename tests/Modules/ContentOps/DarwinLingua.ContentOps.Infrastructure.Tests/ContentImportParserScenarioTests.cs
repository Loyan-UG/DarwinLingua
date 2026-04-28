using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

public sealed class ContentImportParserScenarioTests
{
    [Fact]
    public async Task ParseAsync_ShouldParseScenarioLessonContracts()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection()
            .AddContentOpsInfrastructure()
            .BuildServiceProvider();

        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            """
            {
              "packageVersion": "1.0",
              "packageId": "scenario-contract-test",
              "packageName": "Scenario Contract Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en", "fa"],
              "entries": [],
              "scenarios": [
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

        ParsedScenarioLessonModel scenario = Assert.Single(parsedPackage.Scenarios);
        Assert.Equal("doctor-appointment-a1", scenario.Slug);
        Assert.Equal("Doctor Appointment", scenario.Title);
        Assert.Equal("Explain that you need an appointment.", scenario.LearnerGoal);
        Assert.Equal("A1", scenario.CefrLevel);
        Assert.Equal("doctor-and-healthcare", scenario.Category);
        Assert.Equal(["appointments-and-health"], scenario.Topics);

        ParsedScenarioDialogueTurnModel turn = Assert.Single(scenario.DialogueTurns);
        Assert.Equal("learner", turn.SpeakerRole);
        Assert.Equal("Ich brauche einen Termin.", turn.BaseText);
        Assert.Contains(turn.Translations, translation => translation.Language == "fa");

        ParsedScenarioPhraseModel phrase = Assert.Single(scenario.UsefulPhrases);
        Assert.Equal("Use when you did not understand.", phrase.UsageNote);

        ParsedScenarioQuestionModel question = Assert.Single(scenario.Questions);
        Assert.Equal(2, question.Answers.Count);
        Assert.Contains(question.Answers, answer => answer.IsCorrect && answer.Text == "Einen Termin.");
    }
}

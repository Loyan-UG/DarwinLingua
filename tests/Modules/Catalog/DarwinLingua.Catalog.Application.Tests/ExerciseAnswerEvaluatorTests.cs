using DarwinLingua.Catalog.Application.Services;

namespace DarwinLingua.Catalog.Application.Tests;

public sealed class ExerciseAnswerEvaluatorTests
{
    public static TheoryData<string, string, string, string> SupportedExerciseTypes => new()
    {
        { "multiple-choice", """{ "correctOptionIds": ["a"] }""", """{ "selectedOptionIds": ["a"] }""", """{ "selectedOptionIds": ["b"] }""" },
        { "fill-in-the-blank", """{ "acceptedAnswers": ["gehe"] }""", """{ "answer": "gehe" }""", """{ "answer": "geht" }""" },
        { "matching", """{ "pairs": ["hallo = hello", "danke = thanks"] }""", """{ "pairs": ["danke = thanks", "hallo = hello"] }""", """{ "pairs": ["hallo = bye"] }""" },
        { "sentence-ordering", """{ "orderedSegments": ["Ich", "heiße", "Maria."] }""", """{ "orderedSegments": ["Ich", "heiße", "Maria."] }""", """{ "orderedSegments": ["Maria.", "heiße", "Ich"] }""" },
        { "error-correction", """{ "correctedText": "Ich heiße Maria." }""", """{ "correctedText": "Ich heiße Maria." }""", """{ "correctedText": "Ich heißt Maria." }""" },
        { "article-selection", """{ "correctOptionIds": ["der"] }""", """{ "selectedOptionIds": ["der"] }""", """{ "selectedOptionIds": ["die"] }""" },
        { "case-selection", """{ "correctOptionIds": ["der-frau"] }""", """{ "selectedOptionIds": ["der-frau"] }""", """{ "selectedOptionIds": ["die-frau"] }""" },
        { "conjugation", """{ "acceptedAnswers": ["komme"] }""", """{ "answer": "komme" }""", """{ "answer": "kommt" }""" },
        { "translation-controlled", """{ "acceptedAnswers": ["Ich wohne in Berlin."] }""", """{ "answer": "Ich wohne in Berlin." }""", """{ "answer": "Ich bin Berlin." }""" },
        { "dialogue-completion", """{ "correctOptionIds": ["bitte"] }""", """{ "selectedOptionIds": ["bitte"] }""", """{ "selectedOptionIds": ["morgen"] }""" },
        { "vocabulary-choice", """{ "correctOptionIds": ["trinken"] }""", """{ "selectedOptionIds": ["trinken"] }""", """{ "selectedOptionIds": ["schlafen"] }""" },
        { "grammar-choice", """{ "correctOptionIds": ["wo"] }""", """{ "selectedOptionIds": ["wo"] }""", """{ "selectedOptionIds": ["wer"] }""" },
    };

    [Theory]
    [MemberData(nameof(SupportedExerciseTypes))]
    public void Evaluate_ShouldReturnTrue_ForCorrectAnswerShapes(
        string exerciseType,
        string answerKeyJson,
        string correctSubmittedAnswerJson,
        string wrongSubmittedAnswerJson)
    {
        Assert.False(string.IsNullOrWhiteSpace(wrongSubmittedAnswerJson));
        bool result = ExerciseAnswerEvaluator.Evaluate(exerciseType, answerKeyJson, correctSubmittedAnswerJson);

        Assert.True(result);
    }

    [Theory]
    [MemberData(nameof(SupportedExerciseTypes))]
    public void Evaluate_ShouldReturnFalse_ForPlausibleWrongAnswerShapes(
        string exerciseType,
        string answerKeyJson,
        string correctSubmittedAnswerJson,
        string wrongSubmittedAnswerJson)
    {
        Assert.False(string.IsNullOrWhiteSpace(correctSubmittedAnswerJson));
        bool result = ExerciseAnswerEvaluator.Evaluate(exerciseType, answerKeyJson, wrongSubmittedAnswerJson);

        Assert.False(result);
    }
}

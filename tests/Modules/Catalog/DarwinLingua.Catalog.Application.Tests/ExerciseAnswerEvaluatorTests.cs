using DarwinLingua.Catalog.Application.Services;

namespace DarwinLingua.Catalog.Application.Tests;

public sealed class ExerciseAnswerEvaluatorTests
{
    [Fact]
    public void Evaluate_ShouldReturnTrue_ForCorrectMultipleChoiceAnswer()
    {
        bool result = ExerciseAnswerEvaluator.Evaluate(
            "multiple-choice",
            """{ "correctOptionIds": ["a"] }""",
            """{ "selectedOptionIds": ["a"] }""");

        Assert.True(result);
    }

    [Fact]
    public void Evaluate_ShouldReturnFalse_ForWrongFillInTheBlankAnswer()
    {
        bool result = ExerciseAnswerEvaluator.Evaluate(
            "fill-in-the-blank",
            """{ "acceptedAnswers": ["gehe"] }""",
            """{ "answer": "geht" }""");

        Assert.False(result);
    }
}

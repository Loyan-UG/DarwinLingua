using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class ExerciseRunnerViewModelTests
{
    [Fact]
    public void ExercisePromptViewModel_ShouldParseChoiceOptionsWithoutAnswerKey()
    {
        ExerciseRunnerPageViewModel model = new(CreateExercise("""{ "stem": "___ Kaffee", "options": [{ "id": "der", "text": "der" }, { "id": "die", "text": "die" }] }"""), null, null, "en");

        Assert.Equal("___ Kaffee", model.Prompt.Stem);
        Assert.True(model.Prompt.HasOptions);
        Assert.Equal(["der", "die"], model.Prompt.Options.Select(option => option.Id).ToArray());
    }

    [Fact]
    public void ExercisePromptViewModel_ShouldFailSafelyForMalformedPrompt()
    {
        ExerciseRunnerPageViewModel model = new(CreateExercise("{ nope"), null, null, "en");

        Assert.Null(model.Prompt.Stem);
        Assert.Empty(model.Prompt.Options);
    }

    private static ExerciseDetailModel CreateExercise(string promptJson) =>
        new(
            "a1-article-choice",
            "Choose the article",
            "Artikel wählen",
            "Choose the correct article.",
            "Wählen Sie den richtigen Artikel.",
            "A1",
            "article-selection",
            "grammar",
            "grammar-topic",
            "a1-articles",
            promptJson,
            "Kaffee is masculine.",
            "Kaffee ist maskulin.");
}

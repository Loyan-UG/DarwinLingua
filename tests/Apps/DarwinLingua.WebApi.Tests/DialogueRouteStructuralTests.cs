namespace DarwinLingua.WebApi.Tests;

using Xunit;

public sealed class DialogueRouteStructuralTests
{
    [Fact]
    public void WebDialogueRoutesAndRoleplayView_ShouldRenderDeterministicNoAiPractice()
    {
        string controllerSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Controllers", "DialoguesController.cs"));
        string roleplaySource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "Dialogues", "Roleplay.cshtml"));

        Assert.Contains("[Route(\"dialogues\")", controllerSource, StringComparison.Ordinal);
        Assert.Contains("[HttpGet(\"{slug}/roleplay\", Name = \"Dialogues_Roleplay\")]", controllerSource, StringComparison.Ordinal);
        Assert.Contains("BuildRoleplaySteps(dialogue)", controllerSource, StringComparison.Ordinal);
        Assert.Contains("static turn => IsLearnerRole(turn.SpeakerRole)", controllerSource, StringComparison.Ordinal);
        Assert.Contains("Compare your answer with the model answer, then replay the line aloud.", controllerSource, StringComparison.Ordinal);

        Assert.Contains("Model.Steps.Count == 0", roleplaySource, StringComparison.Ordinal);
        Assert.Contains("step.ModelAnswerText", roleplaySource, StringComparison.Ordinal);
        Assert.Contains("step.ModelAnswerPrimaryMeaning", roleplaySource, StringComparison.Ordinal);
        Assert.Contains("step.ModelAnswerSecondaryMeaning", roleplaySource, StringComparison.Ordinal);
        Assert.Contains("step.StaticFeedback", roleplaySource, StringComparison.Ordinal);
        Assert.Contains("data-speak-text=\"@step.ModelAnswerText\"", roleplaySource, StringComparison.Ordinal);
        Assert.DoesNotContain("<form", roleplaySource, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<textarea", roleplaySource, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("submittedAnswer", roleplaySource, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("OpenAI", roleplaySource, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ChatGPT", roleplaySource, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("aiFeedback", roleplaySource, StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveRepositoryPath(params string[] segments)
    {
        DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string candidateSolutionPath = Path.Combine(currentDirectory.FullName, "DarwinLingua.slnx");
            if (File.Exists(candidateSolutionPath))
            {
                return Path.Combine([currentDirectory.FullName, .. segments]);
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new InvalidOperationException("Repository root with DarwinLingua.slnx was not found.");
    }
}

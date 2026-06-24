namespace DarwinLingua.WebApi.Tests;

using Xunit;

public sealed class ConversationStarterRouteStructuralTests
{
    [Fact]
    public void WebConversationStarterRoutesAndViews_ShouldRenderFiltersAndDualLanguageDetail()
    {
        string apiProgramSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.WebApi", "Program.cs"));
        string controllerSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Controllers", "ConversationStartersController.cs"));
        string indexSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "ConversationStarters", "Index.cshtml"));
        string detailSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "ConversationStarters", "Detail.cshtml"));

        Assert.Contains("\"/api/catalog/conversation-starters\"", apiProgramSource, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/conversation-starters/{slug}\"", apiProgramSource, StringComparison.Ordinal);
        Assert.Contains("[Route(DarwinLingua.Web.Services.LearningRouteConventions.ConversationStarters)]", controllerSource, StringComparison.Ordinal);
        Assert.Contains("ResolveTargetLearningLanguageCode(HttpContext)", controllerSource, StringComparison.Ordinal);
        Assert.Contains("targetLearningLanguageCode", apiProgramSource, StringComparison.Ordinal);
        Assert.Contains("GetConversationStarterPacksAsync", controllerSource, StringComparison.Ordinal);
        Assert.Contains("GetConversationStarterPackBySlugAsync", controllerSource, StringComparison.Ordinal);
        Assert.Contains("ResolveSecondaryMeaningLanguageAsync", controllerSource, StringComparison.Ordinal);

        Assert.Contains("name=\"cefrLevel\"", indexSource, StringComparison.Ordinal);
        Assert.Contains("name=\"situation\"", indexSource, StringComparison.Ordinal);
        Assert.Contains("name=\"tone\"", indexSource, StringComparison.Ordinal);
        Assert.Contains("name=\"conversationGoal\"", indexSource, StringComparison.Ordinal);
        Assert.Contains("name=\"topicKey\"", indexSource, StringComparison.Ordinal);

        Assert.Contains("TextDirection.FromLanguageCode(Model.PrimaryMeaningLanguageCode)", detailSource, StringComparison.Ordinal);
        Assert.Contains("TextDirection.FromLanguageCode(Model.SecondaryMeaningLanguageCode)", detailSource, StringComparison.Ordinal);
        Assert.Contains("dir=\"@primaryMeaningDirection\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("dir=\"@secondaryMeaningDirection\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("phrase.AlternativeBaseTexts", detailSource, StringComparison.Ordinal);
        Assert.Contains("phrase.UsageNote", detailSource, StringComparison.Ordinal);
        Assert.Contains("phrase.Register", detailSource, StringComparison.Ordinal);
        Assert.Contains("phrase.CommonMistake", detailSource, StringComparison.Ordinal);
        Assert.Contains("phrase.PrimaryMeaning", detailSource, StringComparison.Ordinal);
        Assert.Contains("phrase.SecondaryMeaning", detailSource, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Dialogues\"", detailSource, StringComparison.Ordinal);
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

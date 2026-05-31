namespace DarwinLingua.WebApi.Tests;

using Xunit;

public sealed class RoleplayScenarioRouteStructuralTests
{
    [Fact]
    public void WebApiProgram_ShouldExposeRoleplayScenarioListDetailAndSearchRoutes()
    {
        string programSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.WebApi", "Program.cs"));

        Assert.Contains("\"/api/catalog/roleplays\"", programSource, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/roleplays/{slug}\"", programSource, StringComparison.Ordinal);
        Assert.Contains("IRoleplayScenarioQueryService", programSource, StringComparison.Ordinal);
        Assert.Contains("RoleplayScenarioListFilterModel", programSource, StringComparison.Ordinal);
        Assert.Contains("resultType", programSource, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/search\"", programSource, StringComparison.Ordinal);
    }

    [Fact]
    public void WebRoleplayRoutesAndViews_ShouldRenderDeterministicRunnerWithoutLeakingImagePrompts()
    {
        string controllerSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Controllers", "RoleplaysController.cs"));
        string indexSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "Roleplays", "Index.cshtml"));
        string detailSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "Roleplays", "Detail.cshtml"));

        Assert.Contains("[Route(\"roleplays\")", controllerSource, StringComparison.Ordinal);
        Assert.Contains("IWebLearningProfileAccessor", controllerSource, StringComparison.Ordinal);
        Assert.Contains("profile.PreferredMeaningLanguage1", controllerSource, StringComparison.Ordinal);
        Assert.Contains("[OutputCache(NoStore = true)]", controllerSource, StringComparison.Ordinal);
        Assert.Contains("GetRoleplaysAsync", controllerSource, StringComparison.Ordinal);
        Assert.Contains("GetRoleplayBySlugAsync", controllerSource, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Roleplays\"", indexSource, StringComparison.Ordinal);
        Assert.Contains("panel stack-lg", indexSource, StringComparison.Ordinal);
        Assert.Contains("grid-two-col", indexSource, StringComparison.Ordinal);
        Assert.Contains("scenario-translation-line", detailSource, StringComparison.Ordinal);
        Assert.Contains("PrimaryMeaningLanguageCode", detailSource, StringComparison.Ordinal);
        Assert.Contains("AnswerChoices", detailSource, StringComparison.Ordinal);
        Assert.Contains("StaticFeedback", detailSource, StringComparison.Ordinal);
        Assert.Contains("ImageSlots", detailSource, StringComparison.Ordinal);
        Assert.Contains("string.IsNullOrWhiteSpace(imageSlot.AssetPath)", detailSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ImagePrompt", detailSource, StringComparison.Ordinal);
        Assert.DoesNotContain("textarea", detailSource, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("free-text", detailSource, StringComparison.OrdinalIgnoreCase);
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

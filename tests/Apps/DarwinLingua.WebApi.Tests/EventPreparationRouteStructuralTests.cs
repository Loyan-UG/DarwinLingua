namespace DarwinLingua.WebApi.Tests;

using Xunit;

public sealed class EventPreparationRouteStructuralTests
{
    [Fact]
    public void WebAndApiEventPreparationRoutes_ShouldRenderEntitledPreparationPackDetail()
    {
        string apiProgramSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.WebApi", "Program.cs"));
        string controllerSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Controllers", "EventPreparationPacksController.cs"));
        string detailSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "EventPreparationPacks", "Detail.cshtml"));
        string eventDetailSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "ConversationEvents", "Detail.cshtml"));

        Assert.Contains("\"/api/catalog/event-preparation-packs\"", apiProgramSource, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/dialogues/{slug}/event-preparation-packs\"", apiProgramSource, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/event-preparation-packs/{slug}\"", apiProgramSource, StringComparison.Ordinal);

        Assert.Contains("[Route(DarwinLingua.Web.Services.LearningRouteConventions.EventPreparationPacks)]", controllerSource, StringComparison.Ordinal);
        Assert.Contains("ResolveTargetLearningLanguageCode(HttpContext)", controllerSource, StringComparison.Ordinal);
        Assert.Contains("targetLearningLanguageCode", apiProgramSource, StringComparison.Ordinal);
        Assert.Contains("CanUseEventPreparationPacksAsync", controllerSource, StringComparison.Ordinal);
        Assert.Contains("GetEventPreparationPackBySlugAsync", controllerSource, StringComparison.Ordinal);
        Assert.Contains("WebProductAnalyticsEvents.EventPreparationPackCompleted", controllerSource, StringComparison.Ordinal);
        Assert.Contains("RedirectToAction(nameof(Detail)", controllerSource, StringComparison.Ordinal);

        Assert.Contains("asp-action=\"Complete\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("Mark prepared", detailSource, StringComparison.Ordinal);
        Assert.Contains("Preparation prompts", detailSource, StringComparison.Ordinal);
        Assert.Contains("PreparationPack.Prompts", detailSource, StringComparison.Ordinal);
        Assert.Contains("Useful vocabulary", detailSource, StringComparison.Ordinal);
        Assert.Contains("PreparationPack.LinkedVocabulary", detailSource, StringComparison.Ordinal);
        Assert.Contains("Related practice", detailSource, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Dialogues\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"ConversationStarters\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("CompletionRecorded", detailSource, StringComparison.Ordinal);

        Assert.Contains("Model.PreparationPacks.Count > 0", eventDetailSource, StringComparison.Ordinal);
        Assert.Contains("foreach (var preparationPack in Model.PreparationPacks)", eventDetailSource, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"EventPreparationPacks\"", eventDetailSource, StringComparison.Ordinal);
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

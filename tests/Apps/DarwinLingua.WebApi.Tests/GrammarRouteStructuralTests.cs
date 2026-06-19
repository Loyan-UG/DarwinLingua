namespace DarwinLingua.WebApi.Tests;

using Xunit;

public sealed class GrammarRouteStructuralTests
{
    [Fact]
    public void GrammarApi_ShouldExposeCanonicalTopicRoutesAndLocalizedDetailFallback()
    {
        string apiSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.WebApi", "Program.cs"));
        string webClientSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Services", "WebCatalogApiClient.cs"));

        Assert.Contains("\"/api/catalog/grammar-topics\"", apiSource, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/grammar-topics/{slug}\"", apiSource, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/grammar\"", apiSource, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/grammar/{slug}\"", apiSource, StringComparison.Ordinal);
        Assert.Contains("primaryMeaningLanguageCode ?? \"en\"", apiSource, StringComparison.Ordinal);

        Assert.Contains("\"/api/catalog/grammar-topics\"", webClientSource, StringComparison.Ordinal);
        Assert.Contains("$\"/api/catalog/grammar-topics/{Uri.EscapeDataString(slug)}\"", webClientSource, StringComparison.Ordinal);
    }

    [Fact]
    public void GrammarDetail_ShouldRenderLocalizedContentAndLinkedPracticeSafely()
    {
        string controllerSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Controllers", "GrammarController.cs"));
        string detailSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "Grammar", "Detail.cshtml"));
        string indexSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "Grammar", "Index.cshtml"));

        Assert.Contains("GetGrammarTopicBySlugAsync(normalizedSlug, profile.PreferredMeaningLanguage1", controllerSource, StringComparison.Ordinal);
        Assert.Contains("dir=\"@primaryMeaningDirection\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("grammarSection.Blocks", detailSource, StringComparison.Ordinal);
        Assert.Contains("example.Translation", detailSource, StringComparison.Ordinal);
        Assert.Contains("mistake.Explanation", detailSource, StringComparison.Ordinal);

        Assert.Contains("Model.GrammarTopic.LinkedWords.Count > 0", detailSource, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Words\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("asp-route-slug=\"@word.WordSlug\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("<span>@word.Lemma</span>", detailSource, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Dialogues\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"TalkTopics\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Exercises\"", detailSource, StringComparison.Ordinal);

        Assert.Contains("name=\"grammarCategory\"", indexSource, StringComparison.Ordinal);
        Assert.Contains("name=\"topic\"", indexSource, StringComparison.Ordinal);
        Assert.Contains("name=\"q\"", indexSource, StringComparison.Ordinal);
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

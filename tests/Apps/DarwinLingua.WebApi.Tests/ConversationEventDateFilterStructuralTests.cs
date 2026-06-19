namespace DarwinLingua.WebApi.Tests;

using Xunit;

public sealed class ConversationEventDateFilterStructuralTests
{
    [Fact]
    public void ConversationEventRoutes_ShouldExposeStructuredDateFiltering()
    {
        string apiProgramSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.WebApi", "Program.cs"));
        string webControllerSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Controllers", "ConversationEventsController.cs"));
        string webClientSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Services", "WebCatalogApiClient.cs"));
        string indexViewSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "ConversationEvents", "Index.cshtml"));
        string detailViewSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "ConversationEvents", "Detail.cshtml"));

        Assert.Contains("DateTime? dateFromUtc", apiProgramSource, StringComparison.Ordinal);
        Assert.Contains("DateTime? dateToUtc", apiProgramSource, StringComparison.Ordinal);
        Assert.Contains("new ConversationEventListFilterModel(city, cefrLevel, helperLanguageCode, isOnline, priceType, category, dateFromUtc, dateToUtc)", apiProgramSource, StringComparison.Ordinal);

        Assert.Contains("string? dateFromUtc", webControllerSource, StringComparison.Ordinal);
        Assert.Contains("string? dateToUtc", webControllerSource, StringComparison.Ordinal);
        Assert.Contains("NormalizeDateFilter(dateFromUtc, endOfDay: false)", webControllerSource, StringComparison.Ordinal);
        Assert.Contains("NormalizeDateFilter(dateToUtc, endOfDay: true)", webControllerSource, StringComparison.Ordinal);

        Assert.Contains("new(\"dateFromUtc\", FormatUtc(filter.DateFromUtc))", webClientSource, StringComparison.Ordinal);
        Assert.Contains("new(\"dateToUtc\", FormatUtc(filter.DateToUtc))", webClientSource, StringComparison.Ordinal);

        Assert.Contains("name=\"dateFromUtc\"", indexViewSource, StringComparison.Ordinal);
        Assert.Contains("name=\"dateToUtc\"", indexViewSource, StringComparison.Ordinal);
        Assert.Contains("conversationEvent.StartsAtUtc", indexViewSource, StringComparison.Ordinal);
        Assert.Contains("Model.Event.StartsAtUtc", detailViewSource, StringComparison.Ordinal);
        Assert.Contains("Model.Event.EndsAtUtc", detailViewSource, StringComparison.Ordinal);
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

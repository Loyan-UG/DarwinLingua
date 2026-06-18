using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class LearningProgressRouteStructuralTests
{
    [Fact]
    public void Program_ShouldRequireAuthenticatedUser_ForLearningProgressEndpoints()
    {
        string program = File.ReadAllText(GetProgramPath());
        string normalized = NormalizeWhitespace(program);

        Assert.Contains("\"/api/learning/progress/summary\"", program, StringComparison.Ordinal);
        Assert.Contains("\"/api/learning/progress/content\"", program, StringComparison.Ordinal);
        Assert.Contains("\"/api/learning/recommendations\"", program, StringComparison.Ordinal);
        Assert.Contains("GetSummaryAsync(GetRequiredUserId(principal), cancellationToken)", normalized, StringComparison.Ordinal);
        Assert.Contains("UpdateContentProgressAsync(GetRequiredUserId(principal), request, cancellationToken)", normalized, StringComparison.Ordinal);
        Assert.Contains("GetRecommendationsAsync(GetRequiredUserId(principal), Math.Clamp(take ?? 6, 1, 20), cancellationToken)", normalized, StringComparison.Ordinal);
        Assert.DoesNotContain("Identity?.Name ?? \"anonymous\"", program, StringComparison.Ordinal);
    }

    [Fact]
    public void Program_ShouldAttachAuthorizationToLearningProgressEndpointGroup()
    {
        string program = File.ReadAllText(GetProgramPath());
        string normalized = NormalizeWhitespace(program);

        Assert.Contains("\"/api/learning/progress/summary\"", normalized, StringComparison.Ordinal);
        Assert.Contains("\"/api/learning/progress/content\"", normalized, StringComparison.Ordinal);
        Assert.Contains("\"/api/learning/recommendations\"", normalized, StringComparison.Ordinal);
        Assert.True(
            CountOccurrences(normalized, ".RequireAuthorization()") >= 3,
            "Expected progress summary, content update, and recommendation endpoints to require authorization.");
    }

    [Fact]
    public void WebViews_ShouldRenderCourseAndRecentProgressIndicators()
    {
        string courseLessonView = File.ReadAllText(GetCourseLessonViewPath());
        string recentView = File.ReadAllText(GetRecentViewPath());
        string recentController = File.ReadAllText(GetRecentControllerPath());

        Assert.Contains("Learning progress", courseLessonView, StringComparison.Ordinal);
        Assert.Contains("Model.Progress.State", courseLessonView, StringComparison.Ordinal);
        Assert.Contains("Learning progress", recentView, StringComparison.Ordinal);
        Assert.Contains("Model.LearningProgress", recentView, StringComparison.Ordinal);
        Assert.Contains("Completed", recentView, StringComparison.Ordinal);
        Assert.Contains("Needs review", recentView, StringComparison.Ordinal);
        Assert.Contains("Personalized practice", recentView, StringComparison.Ordinal);
        Assert.Contains("Model.Recommendations", recentView, StringComparison.Ordinal);
        Assert.Contains("GetRecommendationsAsync(userId, 6, cancellationToken)", NormalizeWhitespace(recentController), StringComparison.Ordinal);
        Assert.Contains("Review this word because you marked it as difficult.", recentController, StringComparison.Ordinal);
    }

    [Fact]
    public void RecommendationReader_ShouldUseWeakExerciseAndDifficultWordSignals()
    {
        string reader = File.ReadAllText(GetRecommendationReaderPath());

        Assert.Contains("UserExerciseAttempts", reader, StringComparison.Ordinal);
        Assert.Contains("UserWordStates", reader, StringComparison.Ordinal);
        Assert.Contains("\"weak-exercise\"", reader, StringComparison.Ordinal);
        Assert.Contains("\"difficult-word\"", reader, StringComparison.Ordinal);
        Assert.Contains("latest saved attempt was not correct", reader, StringComparison.Ordinal);
        Assert.Contains("marked it as difficult", reader, StringComparison.Ordinal);
    }

    private static string GetProgramPath() =>
        Path.Combine(FindRepositoryRoot(), "src", "Apps", "DarwinLingua.WebApi", "Program.cs");

    private static string GetCourseLessonViewPath() =>
        Path.Combine(FindRepositoryRoot(), "src", "Apps", "DarwinLingua.Web", "Views", "Courses", "Lesson.cshtml");

    private static string GetRecentViewPath() =>
        Path.Combine(FindRepositoryRoot(), "src", "Apps", "DarwinLingua.Web", "Views", "Recent", "Index.cshtml");

    private static string GetRecentControllerPath() =>
        Path.Combine(FindRepositoryRoot(), "src", "Apps", "DarwinLingua.Web", "Controllers", "RecentController.cs");

    private static string GetRecommendationReaderPath() =>
        Path.Combine(FindRepositoryRoot(), "src", "Modules", "Learning", "DarwinLingua.Learning.Infrastructure", "Services", "LearningRecommendationCatalogReader.cs");

    private static string FindRepositoryRoot()
    {
        foreach (string startPath in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            DirectoryInfo? directory = new(startPath);

            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "DarwinLingua.slnx")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        throw new DirectoryNotFoundException("Could not find the DarwinLingua repository root.");
    }

    private static string NormalizeWhitespace(string value) =>
        string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static int CountOccurrences(string value, string expected)
    {
        int count = 0;
        int index = 0;

        while ((index = value.IndexOf(expected, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += expected.Length;
        }

        return count;
    }
}

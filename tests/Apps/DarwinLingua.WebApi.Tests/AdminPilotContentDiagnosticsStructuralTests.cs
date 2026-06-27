using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class AdminPilotContentDiagnosticsStructuralTests
{
    [Fact]
    public void Program_ShouldExposeAdminOnlyPilotSearchDiagnostics()
    {
        string program = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.WebApi", "Program.cs"));

        Assert.Contains("\"/api/admin/catalog/learning-search\"", program, StringComparison.Ordinal);
        Assert.Contains("ResolveDiagnosticTargetLearningLanguageCode(targetLearningLanguageCode)", program, StringComparison.Ordinal);
        Assert.Contains("new UnifiedLearningSearchFilterModel(", program, StringComparison.Ordinal);
        Assert.Contains("IUnifiedLearningSearchService searchService", program, StringComparison.Ordinal);
    }

    [Fact]
    public void Program_ShouldExposeAdminOnlyPilotContentPreviewDiagnostics()
    {
        string program = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.WebApi", "Program.cs"));

        Assert.Contains("\"/api/admin/catalog/learning-content-preview/{contentType}/{slug}\"", program, StringComparison.Ordinal);
        Assert.Contains("ResolveDiagnosticTargetLearningLanguageCode(targetLearningLanguageCode)", program, StringComparison.Ordinal);
        Assert.Contains("\"course-lesson\" => await courseQueryService.GetPublishedCourseLessonBySlugAsync", program, StringComparison.Ordinal);
        Assert.Contains("\"grammar-topic\" or \"grammar\" => await grammarTopicQueryService.GetPublishedGrammarTopicBySlugAsync", program, StringComparison.Ordinal);
        Assert.Contains("\"expression\" => await expressionQueryService.GetPublishedExpressionBySlugAsync", program, StringComparison.Ordinal);
        Assert.Contains("\"exercise\" => await exerciseQueryService.GetPublishedExerciseBySlugAsync", program, StringComparison.Ordinal);
        Assert.Contains("\"exercise-set\" => await exerciseQueryService.GetPublishedExerciseSetBySlugAsync", program, StringComparison.Ordinal);
        Assert.Contains("\"writing-template\" => await writingTemplateQueryService.GetPublishedWritingTemplateBySlugAsync", program, StringComparison.Ordinal);
        Assert.Contains("Unsupported diagnostic content type", program, StringComparison.Ordinal);
    }

    [Fact]
    public void Program_ShouldKeepPublicLearningEndpointsActiveTargetOnly()
    {
        string program = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.WebApi", "Program.cs"));

        Assert.Contains("\"/api/catalog/course-lessons/{slug}\"", program, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/search\"", program, StringComparison.Ordinal);
        Assert.Contains("ResolveTargetLearningLanguageCode(targetLearningLanguageCode)", program, StringComparison.Ordinal);
    }

    private static string ResolveRepositoryPath(params string[] relativeSegments) =>
        Path.Combine(FindRepositoryRoot(), Path.Combine(relativeSegments));

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

        throw new InvalidOperationException("Could not locate repository root.");
    }
}

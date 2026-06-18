namespace DarwinLingua.WebApi.Tests;

using DarwinLingua.Web.Models;
using Xunit;

public sealed class AdminImportsModuleSummaryTests
{
    [Fact]
    public void ModuleSummaries_ShouldGroupPhaseSevenPackageCounters()
    {
        DateTime firstImport = new(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc);
        DateTime secondImport = new(2026, 6, 11, 12, 0, 0, DateTimeKind.Utc);
        AdminImportsPageViewModel model = new(
            null,
            [
                new("course-a1-foundation-v1", "v1", "Course A1", "Json", "Completed", 10, 10, 0, 0, firstImport),
                new("course-a2-foundation-v1", "v1", "Course A2", "Json", "CompletedWithWarnings", 8, 7, 1, 2, secondImport),
                new("writing-templates-a1-foundation-v1", "v1", "Writing Templates A1", "Json", "Completed", 5, 5, 0, 0, firstImport),
                new("exam-prep-c1-foundation-v1", "v1", "Exam Prep C1", "Json", "Failed", 4, 0, 4, 0, secondImport),
            ]);

        AdminImportModuleSummaryViewModel courseSummary = Assert.Single(
            model.ModuleSummaries,
            summary => summary.ModuleKey == "courses");
        Assert.Equal("Courses", courseSummary.DisplayName);
        Assert.Equal(2, courseSummary.PackageCount);
        Assert.Equal(1, courseSummary.CompletedPackageCount);
        Assert.Equal(1, courseSummary.WarningPackageCount);
        Assert.Equal(0, courseSummary.FailedPackageCount);
        Assert.Equal(18, courseSummary.TotalEntries);
        Assert.Equal(17, courseSummary.InsertedEntries);
        Assert.Equal(1, courseSummary.InvalidEntries);
        Assert.Equal(2, courseSummary.WarningCount);
        Assert.Equal(secondImport, courseSummary.LastImportAtUtc);

        Assert.Contains(model.ModuleSummaries, summary => summary.ModuleKey == "writing-templates" && summary.PackageCount == 1);
        Assert.Contains(model.ModuleSummaries, summary => summary.ModuleKey == "exam-prep" && summary.FailedPackageCount == 1);
    }

    [Fact]
    public void ImportsView_ShouldRenderModuleValidationSummaryTable()
    {
        string viewSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Areas", "Admin", "Views", "Imports", "Index.cshtml"));

        Assert.Contains("imports-module-summary", viewSource, StringComparison.Ordinal);
        Assert.Contains("Import validation summary by module", viewSource, StringComparison.Ordinal);
        Assert.Contains("Model.ModuleSummaries", viewSource, StringComparison.Ordinal);
        Assert.Contains("summary.WarningPackageCount", viewSource, StringComparison.Ordinal);
        Assert.Contains("summary.InvalidEntries", viewSource, StringComparison.Ordinal);
        Assert.Contains("summary.InsertedEntries", viewSource, StringComparison.Ordinal);
    }

    private static string ResolveRepositoryPath(params string[] segments)
    {
        string? directory = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(directory))
        {
            string candidate = Path.Combine(new[] { directory }.Concat(segments).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new FileNotFoundException($"Could not resolve repository file '{string.Join(Path.DirectorySeparatorChar, segments)}'.");
    }
}

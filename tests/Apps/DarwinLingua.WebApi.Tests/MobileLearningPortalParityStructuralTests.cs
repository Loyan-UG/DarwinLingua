using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class MobileLearningPortalParityStructuralTests
{
    [Fact]
    public void CatalogPackagePublisher_ExportsPhase7LearningPortalArrays()
    {
        string source = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.WebApi", "Services", "CatalogPackagePublisher.cs"));

        Assert.Contains("GrammarTopics", source);
        Assert.Contains("ExpressionEntries", source);
        Assert.Contains("ExerciseSets", source);
        Assert.Contains("CourseLessons", source);
        Assert.Contains("WritingTemplates", source);
        Assert.Contains("CulturalNotes", source);
        Assert.Contains("ExamPrepUnits", source);
    }

    [Fact]
    public void MobileFullReplaceScript_CopiesPhase7Tables()
    {
        string source = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinDeutsch.Maui", "Services", "Updates", "RemoteContentUpdateService.cs"));

        Assert.Contains("INSERT INTO GrammarTopics SELECT * FROM remote.GrammarTopics;", source);
        Assert.Contains("INSERT INTO ExpressionEntries SELECT * FROM remote.ExpressionEntries;", source);
        Assert.Contains("INSERT INTO ExerciseSets SELECT * FROM remote.ExerciseSets;", source);
        Assert.Contains("INSERT INTO CourseLessons SELECT * FROM remote.CourseLessons;", source);
        Assert.Contains("INSERT INTO WritingTemplates SELECT * FROM remote.WritingTemplates;", source);
        Assert.Contains("INSERT INTO CulturalNotes SELECT * FROM remote.CulturalNotes;", source);
        Assert.Contains("INSERT INTO ExamPrepUnits SELECT * FROM remote.ExamPrepUnits;", source);
    }

    [Fact]
    public void MauiLearningPortal_RegistersMobileRoutesAndLocalizedNavigation()
    {
        string shellSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinDeutsch.Maui", "AppShell.xaml.cs"));
        string resourcesSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinDeutsch.Maui", "Resources", "Strings", "AppStrings.resx"));

        Assert.Contains("LearningPortalListPage", shellSource);
        Assert.Contains("LearningPortalDetailPage", shellSource);
        Assert.Contains("LearningPortalSearchPage", shellSource);
        Assert.Contains("LearningPortalGroupLearn", resourcesSource);
        Assert.Contains("LearningPortalGroupPractice", resourcesSource);
        Assert.Contains("LearningPortalGroupSpeak", resourcesSource);
        Assert.Contains("LearningPortalGroupPrepare", resourcesSource);
        Assert.Contains("LearningPortalGroupResources", resourcesSource);
    }

    private static string ResolveRepositoryPath(params string[] segments)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            string candidate = Path.Combine(new[] { directory.FullName }.Concat(segments).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}

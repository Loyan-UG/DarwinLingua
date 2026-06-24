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
        Assert.Contains("CountryGuidanceNotes", source);
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
        Assert.Contains("INSERT INTO CountryGuidanceNotes SELECT * FROM remote.CountryGuidanceNotes;", source);
        Assert.Contains("INSERT INTO ExamPrepUnits SELECT * FROM remote.ExamPrepUnits;", source);
    }

    [Fact]
    public void MobileRemoteUpdate_SupportsModuleScopedPackageApply()
    {
        string source = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinDeutsch.Maui", "Services", "Updates", "RemoteContentUpdateService.cs"));

        Assert.Contains("ReplaceMode.Module", source);
        Assert.Contains("ForModule", source);
        Assert.Contains("/api/mobile/content/areas/catalog/modules/", source);
        Assert.Contains("ReplaceModuleContentAsync", source);
    }

    [Fact]
    public void MauiStartup_DoesNotApplyPackagedSeedAutomatically()
    {
        string startupSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinDeutsch.Maui", "Services", "Startup", "AppStartupInitializationService.cs"));
        string deferredSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinDeutsch.Maui", "Services", "Startup", "DeferredStartupMaintenanceService.cs"));

        Assert.DoesNotContain("EnsureSeedDatabaseAsync", startupSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ApplySeedUpdateAsync", deferredSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ISeedDatabaseProvisioningService", deferredSource, StringComparison.Ordinal);
    }

    [Fact]
    public void WebStartup_DoesNotRegisterLocalSqliteLearningDatabase()
    {
        string programSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Program.cs"));
        string projectSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "DarwinLingua.Web.csproj"));
        string bootstrapperSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Services", "WebUserStateDatabaseBootstrapper.cs"));

        Assert.Contains("AddDarwinLinguaInfrastructureForPostgres", programSource, StringComparison.Ordinal);
        Assert.DoesNotContain("AddDarwinLinguaInfrastructure(", programSource, StringComparison.Ordinal);
        Assert.DoesNotContain("IDatabaseInitializer", programSource, StringComparison.Ordinal);
        Assert.DoesNotContain("UseSqlite", programSource, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Microsoft.EntityFrameworkCore.Sqlite", projectSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Sqlite", bootstrapperSource, StringComparison.OrdinalIgnoreCase);
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

    [Fact]
    public void MobileValidationWorksheet_ShouldTrackPhase7OfflineAndModuleUpdateChecks()
    {
        string worksheet = File.ReadAllText(ResolveRepositoryPath("artifacts", "validation", "phase7-mobile-validation-worksheet.md"));
        string roadmap = File.ReadAllText(ResolveRepositoryPath("docs", "76-Learning-Portal-Roadmap-And-Backlog.md"));
        string testBacklog = File.ReadAllText(ResolveRepositoryPath("docs", "71-Web-Test-Backlog.md"));
        string releaseChecklist = File.ReadAllText(ResolveRepositoryPath("docs", "61-Web-Release-Checklist.md"));

        Assert.Contains("First-Run Module Selection", worksheet, StringComparison.Ordinal);
        Assert.Contains("Module-Scoped Package Update", worksheet, StringComparison.Ordinal);
        Assert.Contains("Offline Behavior", worksheet, StringComparison.Ordinal);
        Assert.Contains("Phase 7 Content Surfaces", worksheet, StringComparison.Ordinal);
        Assert.Contains("Mobile exercise runner implementation.", worksheet, StringComparison.Ordinal);
        Assert.Contains("phase7-mobile-validation-worksheet.md", roadmap, StringComparison.Ordinal);
        Assert.Contains("phase7-mobile-validation-worksheet.md", testBacklog, StringComparison.Ordinal);
        Assert.Contains("phase7-mobile-validation-worksheet.md", releaseChecklist, StringComparison.Ordinal);
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

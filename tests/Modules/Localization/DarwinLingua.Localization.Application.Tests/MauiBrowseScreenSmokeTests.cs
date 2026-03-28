namespace DarwinLingua.Localization.Application.Tests;

/// <summary>
/// Provides structural smoke coverage for the MAUI home and browse surfaces.
/// </summary>
public sealed class MauiBrowseScreenSmokeTests
{
    /// <summary>
    /// Verifies that the home screen keeps the main dashboard browse entry points wired in XAML.
    /// </summary>
    [Fact]
    public void HomePage_ShouldExposeDashboardBrowseSections()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string homePagePath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/HomePage.xaml");
        string welcomePagePath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/WelcomePage.xaml");
        string appPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/App.xaml.cs");
        string mauiProgramPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/MauiProgram.cs");
        string seedProvisioningServicePath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Services/Storage/SeedDatabaseProvisioningService.cs");
        string seedDatabaseAssetPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Resources/Raw/darwin-lingua.seed.db");

        Assert.True(File.Exists(homePagePath), $"Home page XAML file not found: {homePagePath}");
        Assert.True(File.Exists(welcomePagePath), $"Welcome page XAML file not found: {welcomePagePath}");
        Assert.True(File.Exists(appPath), $"App code-behind file not found: {appPath}");
        Assert.True(File.Exists(mauiProgramPath), $"Maui program file not found: {mauiProgramPath}");
        Assert.True(File.Exists(seedProvisioningServicePath), $"Seed provisioning service file not found: {seedProvisioningServicePath}");
        Assert.True(File.Exists(seedDatabaseAssetPath), $"Seed database asset file not found: {seedDatabaseAssetPath}");

        string sourceCode = File.ReadAllText(homePagePath);
        string welcomeSource = File.ReadAllText(welcomePagePath);
        string appSource = File.ReadAllText(appPath);
        string mauiProgramSource = File.ReadAllText(mauiProgramPath);
        string seedProvisioningServiceSource = File.ReadAllText(seedProvisioningServicePath);

        Assert.Contains("CefrQuickFilterView", sourceCode, StringComparison.Ordinal);
        Assert.Contains("LogoPlaceholderLabel", sourceCode, StringComparison.Ordinal);
        Assert.Contains("AppNameLabel", sourceCode, StringComparison.Ordinal);
        Assert.Contains("AppSubtitleLabel", sourceCode, StringComparison.Ordinal);
        Assert.Contains("ExploreSectionLabel", sourceCode, StringComparison.Ordinal);
        Assert.Contains("PracticeActionBlockView", sourceCode, StringComparison.Ordinal);
        Assert.Contains("SearchActionBlockView", sourceCode, StringComparison.Ordinal);
        Assert.Contains("BrowseTopicsActionBlockView", sourceCode, StringComparison.Ordinal);
        Assert.Contains("FavoritesActionBlockView", sourceCode, StringComparison.Ordinal);
        Assert.Contains("LanguagePicker", welcomeSource, StringComparison.Ordinal);
        Assert.Contains("CurrentFeaturesBodyLabel", welcomeSource, StringComparison.Ordinal);
        Assert.Contains("LearnWithLanguagesBodyLabel", welcomeSource, StringComparison.Ordinal);
        Assert.Contains("InterfaceLanguagesBodyLabel", welcomeSource, StringComparison.Ordinal);
        Assert.Contains("StartButton", welcomeSource, StringComparison.Ordinal);
        Assert.Contains("ShouldShowWelcomeExperience", appSource, StringComparison.Ordinal);
        Assert.Contains("ISeedDatabaseProvisioningService", mauiProgramSource, StringComparison.Ordinal);
        Assert.Contains("EnsureSeedDatabaseAsync", mauiProgramSource, StringComparison.Ordinal);
        Assert.Contains("darwin-lingua.seed.db", seedProvisioningServiceSource, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that the settings screen hosts the app-information content removed from home.
    /// </summary>
    [Fact]
    public void SettingsPage_ShouldExposeAppInformationSections()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string settingsPagePath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/SettingsPage.xaml");
        string settingsCodeBehindPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/SettingsPage.xaml.cs");

        Assert.True(File.Exists(settingsPagePath), $"Settings page XAML file not found: {settingsPagePath}");
        Assert.True(File.Exists(settingsCodeBehindPath), $"Settings page code-behind file not found: {settingsCodeBehindPath}");

        string xamlSource = File.ReadAllText(settingsPagePath);
        string codeBehindSource = File.ReadAllText(settingsCodeBehindPath);

        Assert.Contains("AppInfoSectionLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("CurrentLanguageSectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("MeaningLanguagesSectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("SupportedLanguagesSectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("CurrentFeaturesSectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("FutureFeaturesSectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ContentUpdatesSectionLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ContentUpdateStatusSectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ContentUpdateDetailsSectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ApplySeedUpdateButton", xamlSource, StringComparison.Ordinal);
        Assert.Contains("SettingsAppInfoSectionLabel", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("WelcomeCurrentFeaturesBody", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("WelcomeFutureFeaturesBody", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("ISeedDatabaseProvisioningService", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("OnApplySeedUpdateButtonClicked", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("BuildContentUpdateDetails", codeBehindSource, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that the browse screen combines CEFR shortcuts, topic browsing, and navigation actions.
    /// </summary>
    [Fact]
    public void TopicsPage_ShouldActAsBrowseHub()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string topicsPagePath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/TopicsPage.xaml");
        string topicsCodeBehindPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/TopicsPage.xaml.cs");

        Assert.True(File.Exists(topicsPagePath), $"Topics page XAML file not found: {topicsPagePath}");
        Assert.True(File.Exists(topicsCodeBehindPath), $"Topics page code-behind file not found: {topicsCodeBehindPath}");

        string xamlSource = File.ReadAllText(topicsPagePath);
        string codeBehindSource = File.ReadAllText(topicsCodeBehindPath);

        Assert.Contains("CefrQuickFilterView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("StatusBadgeLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ShortcutsSectionLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("TopicsSectionLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("SearchActionBlockView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("FavoritesActionBlockView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("TopicListItemView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("nameof(CefrWordsPage)", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("nameof(SearchWordsPage)", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("\"//favorites\"", codeBehindSource, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that the practice screen is wired as a first-class learner-facing surface.
    /// </summary>
    [Fact]
    public void PracticePage_ShouldExposeOverviewAndReviewSections()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string practicePagePath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/PracticePage.xaml");
        string practiceCodeBehindPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/PracticePage.xaml.cs");
        string practiceSessionPagePath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/PracticeSessionPage.xaml");
        string practiceSessionCodeBehindPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/PracticeSessionPage.xaml.cs");
        string shellPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/AppShell.xaml.cs");

        Assert.True(File.Exists(practicePagePath), $"Practice page XAML file not found: {practicePagePath}");
        Assert.True(File.Exists(practiceCodeBehindPath), $"Practice page code-behind file not found: {practiceCodeBehindPath}");
        Assert.True(File.Exists(practiceSessionPagePath), $"Practice session page XAML file not found: {practiceSessionPagePath}");
        Assert.True(File.Exists(practiceSessionCodeBehindPath), $"Practice session page code-behind file not found: {practiceSessionCodeBehindPath}");
        Assert.True(File.Exists(shellPath), $"App shell code-behind file not found: {shellPath}");

        string xamlSource = File.ReadAllText(practicePagePath);
        string codeBehindSource = File.ReadAllText(practiceCodeBehindPath);
        string practiceSessionXamlSource = File.ReadAllText(practiceSessionPagePath);
        string practiceSessionCodeBehindSource = File.ReadAllText(practiceSessionCodeBehindPath);
        string shellSource = File.ReadAllText(shellPath);

        Assert.Contains("StartFlashcardsActionBlockView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("StartQuizActionBlockView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("RefreshPracticeActionBlockView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("StatusBadgeLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ProgressSectionLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ActionsSectionLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ReviewSessionCollectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("RecentActivityCollectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("IPracticeLearningProgressSnapshotService", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("IPracticeRecentActivityService", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("IPracticeReviewSessionService", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("OutcomeButtonsGrid", practiceSessionXamlSource, StringComparison.Ordinal);
        Assert.Contains("SummaryBorder", practiceSessionXamlSource, StringComparison.Ordinal);
        Assert.Contains("IPracticeFlashcardAnswerService", practiceSessionCodeBehindSource, StringComparison.Ordinal);
        Assert.Contains("IPracticeQuizAnswerService", practiceSessionCodeBehindSource, StringComparison.Ordinal);
        Assert.Contains("\"practice\"", shellSource, StringComparison.Ordinal);
        Assert.Contains("PracticeSessionPage", shellSource, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that the word-detail screen exposes the richer lexical metadata sections.
    /// </summary>
    [Fact]
    public void WordDetailPage_ShouldExposeLexicalMetadataChips()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string wordDetailXamlPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/WordDetailPage.xaml");
        string wordDetailCodeBehindPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/WordDetailPage.xaml.cs");

        Assert.True(File.Exists(wordDetailXamlPath), $"Word detail XAML file not found: {wordDetailXamlPath}");
        Assert.True(File.Exists(wordDetailCodeBehindPath), $"Word detail code-behind file not found: {wordDetailCodeBehindPath}");

        string xamlSource = File.ReadAllText(wordDetailXamlPath);
        string codeBehindSource = File.ReadAllText(wordDetailCodeBehindPath);

        Assert.Contains("UsageLabelsBorder", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ContextLabelsBorder", xamlSource, StringComparison.Ordinal);
        Assert.Contains("GrammarNotesBorder", xamlSource, StringComparison.Ordinal);
        Assert.Contains("CollocationsBorder", xamlSource, StringComparison.Ordinal);
        Assert.Contains("WordFamiliesBorder", xamlSource, StringComparison.Ordinal);
        Assert.Contains("LexicalRelationsBorder", xamlSource, StringComparison.Ordinal);
        Assert.Contains("UsageLabelsFlexLayout", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ContextLabelsFlexLayout", xamlSource, StringComparison.Ordinal);
        Assert.Contains("GrammarNotesStackLayout", xamlSource, StringComparison.Ordinal);
        Assert.Contains("CollocationsStackLayout", xamlSource, StringComparison.Ordinal);
        Assert.Contains("WordFamiliesStackLayout", xamlSource, StringComparison.Ordinal);
        Assert.Contains("SynonymsStackLayout", xamlSource, StringComparison.Ordinal);
        Assert.Contains("AntonymsStackLayout", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ApplyWordLabels", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("ApplyGrammarNotes", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("ApplyCollocations", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("ApplyWordFamilies", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("ApplyLexicalRelations", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("LexiconTagDisplayText", codeBehindSource, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that the search page keeps the richer search-panel and results-panel layout.
    /// </summary>
    [Fact]
    public void SearchWordsPage_ShouldExposeSearchAndResultsPanels()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string searchPagePath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/SearchWordsPage.xaml");
        string searchCodeBehindPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/SearchWordsPage.xaml.cs");

        Assert.True(File.Exists(searchPagePath), $"Search page XAML file not found: {searchPagePath}");
        Assert.True(File.Exists(searchCodeBehindPath), $"Search page code-behind file not found: {searchCodeBehindPath}");

        string xamlSource = File.ReadAllText(searchPagePath);
        string codeBehindSource = File.ReadAllText(searchCodeBehindPath);

        Assert.Contains("StatusBadgeLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("SearchSectionLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("SearchHintLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ResultsSectionLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("SearchBarControl", xamlSource, StringComparison.Ordinal);
        Assert.Contains("WordsCollectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("SearchWordsPageSearchHint", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("SearchWordsPageResultsLabel", codeBehindSource, StringComparison.Ordinal);
    }

    /// <summary>
    /// Resolves the repository root path by walking parent directories.
    /// </summary>
    private static string ResolveRepositoryRoot()
    {
        DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string candidateSolutionPath = Path.Combine(currentDirectory.FullName, "DarwinLingua.slnx");

            if (File.Exists(candidateSolutionPath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new InvalidOperationException("Unable to resolve repository root from test execution directory.");
    }
}

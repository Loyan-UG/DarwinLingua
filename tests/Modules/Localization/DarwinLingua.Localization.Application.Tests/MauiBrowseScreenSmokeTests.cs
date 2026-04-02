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
        string startupPagePath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/StartupPage.xaml");
        string startupPageCodeBehindPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/StartupPage.xaml.cs");
        string welcomePagePath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/WelcomePage.xaml");
        string appPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/App.xaml.cs");
        string mauiProgramPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/MauiProgram.cs");
        string startupInitializationServicePath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Services/Startup/AppStartupInitializationService.cs");
        string backgroundRemoteUpdateCoordinatorPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Services/Updates/BackgroundRemoteUpdateCoordinator.cs");
        string seedProvisioningServicePath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Services/Storage/SeedDatabaseProvisioningService.cs");
        string seedDatabaseAssetPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Resources/Raw/darwin-lingua.seed.db");

        Assert.True(File.Exists(homePagePath), $"Home page XAML file not found: {homePagePath}");
        Assert.True(File.Exists(startupPagePath), $"Startup page XAML file not found: {startupPagePath}");
        Assert.True(File.Exists(startupPageCodeBehindPath), $"Startup page code-behind file not found: {startupPageCodeBehindPath}");
        Assert.True(File.Exists(welcomePagePath), $"Welcome page XAML file not found: {welcomePagePath}");
        Assert.True(File.Exists(appPath), $"App code-behind file not found: {appPath}");
        Assert.True(File.Exists(mauiProgramPath), $"Maui program file not found: {mauiProgramPath}");
        Assert.True(File.Exists(startupInitializationServicePath), $"Startup initialization service file not found: {startupInitializationServicePath}");
        Assert.True(File.Exists(backgroundRemoteUpdateCoordinatorPath), $"Background remote update coordinator file not found: {backgroundRemoteUpdateCoordinatorPath}");
        Assert.True(File.Exists(seedProvisioningServicePath), $"Seed provisioning service file not found: {seedProvisioningServicePath}");
        Assert.True(File.Exists(seedDatabaseAssetPath), $"Seed database asset file not found: {seedDatabaseAssetPath}");

        string sourceCode = File.ReadAllText(homePagePath);
        string startupSource = File.ReadAllText(startupPagePath);
        string startupCodeBehindSource = File.ReadAllText(startupPageCodeBehindPath);
        string welcomeSource = File.ReadAllText(welcomePagePath);
        string appSource = File.ReadAllText(appPath);
        string mauiProgramSource = File.ReadAllText(mauiProgramPath);
        string startupInitializationServiceSource = File.ReadAllText(startupInitializationServicePath);
        string backgroundRemoteUpdateCoordinatorSource = File.ReadAllText(backgroundRemoteUpdateCoordinatorPath);
        string seedProvisioningServiceSource = File.ReadAllText(seedProvisioningServicePath);
        string deferredStartupMaintenanceServicePath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Services/Startup/DeferredStartupMaintenanceService.cs");
        Assert.True(File.Exists(deferredStartupMaintenanceServicePath), $"Deferred startup maintenance service file not found: {deferredStartupMaintenanceServicePath}");
        string deferredStartupMaintenanceServiceSource = File.ReadAllText(deferredStartupMaintenanceServicePath);

        Assert.Contains("CefrQuickFilterView", sourceCode, StringComparison.Ordinal);
        Assert.Contains("LogoPlaceholderLabel", sourceCode, StringComparison.Ordinal);
        Assert.Contains("AppNameLabel", sourceCode, StringComparison.Ordinal);
        Assert.Contains("AppSubtitleLabel", sourceCode, StringComparison.Ordinal);
        Assert.Contains("ExploreSectionLabel", sourceCode, StringComparison.Ordinal);
        Assert.Contains("PracticeActionBlockView", sourceCode, StringComparison.Ordinal);
        Assert.Contains("SearchActionBlockView", sourceCode, StringComparison.Ordinal);
        Assert.Contains("BrowseTopicsActionBlockView", sourceCode, StringComparison.Ordinal);
        Assert.Contains("FavoritesActionBlockView", sourceCode, StringComparison.Ordinal);
        Assert.Contains("ActivityIndicator", startupSource, StringComparison.Ordinal);
        Assert.Contains("RetryButton", startupSource, StringComparison.Ordinal);
        Assert.Contains("RunInitializationAsync", startupCodeBehindSource, StringComparison.Ordinal);
        Assert.Contains("StartupCompleted", startupCodeBehindSource, StringComparison.Ordinal);
        Assert.Contains("LanguagePicker", welcomeSource, StringComparison.Ordinal);
        Assert.Contains("CurrentFeaturesBodyLabel", welcomeSource, StringComparison.Ordinal);
        Assert.Contains("LearnWithLanguagesBodyLabel", welcomeSource, StringComparison.Ordinal);
        Assert.Contains("InterfaceLanguagesBodyLabel", welcomeSource, StringComparison.Ordinal);
        Assert.Contains("StartButton", welcomeSource, StringComparison.Ordinal);
        Assert.Contains("GetStartupPage()", appSource, StringComparison.Ordinal);
        Assert.Contains("GetAppShell()", appSource, StringComparison.Ordinal);
        Assert.Contains("GetWelcomePage()", appSource, StringComparison.Ordinal);
        Assert.Contains("OnStartupCompleted", appSource, StringComparison.Ordinal);
        Assert.Contains("EnsureScheduled", appSource, StringComparison.Ordinal);
        Assert.Contains("Schedule()", appSource, StringComparison.Ordinal);
        Assert.Contains("ScheduleInitialCheck", appSource, StringComparison.Ordinal);
        Assert.Contains("OnWindowResumed", appSource, StringComparison.Ordinal);
        Assert.Contains("ShouldShowWelcomeExperience", appSource, StringComparison.Ordinal);
        Assert.Contains("IAppStartupInitializationService", mauiProgramSource, StringComparison.Ordinal);
        Assert.Contains("IDeferredStartupMaintenanceService", mauiProgramSource, StringComparison.Ordinal);
        Assert.Contains("IBackgroundRemoteUpdateCoordinator", mauiProgramSource, StringComparison.Ordinal);
        Assert.Contains("IPlatformBackgroundUpdateScheduler", mauiProgramSource, StringComparison.Ordinal);
        Assert.Contains("AddSingleton<StartupPage>()", mauiProgramSource, StringComparison.Ordinal);
        Assert.Contains("ISeedDatabaseProvisioningService", mauiProgramSource, StringComparison.Ordinal);
        Assert.Contains("ICefrBrowseStateService", mauiProgramSource, StringComparison.Ordinal);
        Assert.Contains("InitializeCoreAsync(cancellationToken)", startupInitializationServiceSource, StringComparison.Ordinal);
        Assert.Contains("EnsureSeedDatabaseAsync", startupInitializationServiceSource, StringComparison.Ordinal);
        Assert.Contains("InitializeAsync", startupInitializationServiceSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ApplySeedUpdateAsync", startupInitializationServiceSource, StringComparison.Ordinal);
        Assert.Contains("ApplySeedUpdateAsync", deferredStartupMaintenanceServiceSource, StringComparison.Ordinal);
        Assert.Contains("ScheduleInitialWarmup", deferredStartupMaintenanceServiceSource, StringComparison.Ordinal);
        Assert.Contains("ScheduleResumeCheck", backgroundRemoteUpdateCoordinatorSource, StringComparison.Ordinal);
        Assert.Contains("DisplayAlert", backgroundRemoteUpdateCoordinatorSource, StringComparison.Ordinal);
        Assert.Contains("ApplyFullUpdateAsync", backgroundRemoteUpdateCoordinatorSource, StringComparison.Ordinal);
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
        Assert.Contains("RemoteContentUpdatesSectionLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("PackagedSeedUpdatesSectionLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("RemoteContentUpdateStatusSectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("RemoteContentUpdateDetailsSectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("RemoteContentUpdateDiagnosticsSectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("RemoteContentUpdateHistorySectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ApplyRemoteUpdateButton", xamlSource, StringComparison.Ordinal);
        Assert.Contains("CatalogAreaUpdatesSectionLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("CatalogAreaUpdateSectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ApplyCatalogAreaUpdateButton", xamlSource, StringComparison.Ordinal);
        Assert.Contains("CefrLevelUpdatesSectionLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("CefrA1UpdateSectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ApplyCefrA1UpdateButton", xamlSource, StringComparison.Ordinal);
        Assert.Contains("CefrC2UpdateSectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ApplyCefrC2UpdateButton", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ContentUpdateStatusSectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ContentUpdateDetailsSectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ContentUpdateDiagnosticsSectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ApplySeedUpdateButton", xamlSource, StringComparison.Ordinal);
        Assert.Contains("SettingsAppInfoSectionLabel", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("WelcomeCurrentFeaturesBody", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("WelcomeFutureFeaturesBody", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("ISeedDatabaseProvisioningService", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("IRemoteContentUpdateService", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("OnApplyRemoteUpdateButtonClicked", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("OnApplyCatalogAreaUpdateButtonClicked", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("OnApplyCefrLevelUpdateButtonClicked", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("GetAreaUpdateStatusAsync", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("GetCefrUpdateStatusAsync", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("BuildRemoteContentUpdateStatus", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("BuildRemoteContentUpdateDiagnostics", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("BuildRemoteContentUpdateHistory", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("GetRecentUpdateHistoryAsync", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("SettingsRemoteContentUpdatesScopeFormat", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("SettingsRemoteContentUpdatesRemoteChecksumFormat", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("SettingsRemoteContentUpdatesManifestGeneratedAtFormat", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("BuildRemoteScopeSummary", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("OnApplySeedUpdateButtonClicked", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("BuildContentUpdateDetails", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("BuildContentUpdateDiagnostics", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("SettingsContentUpdatesDiagnosticsLabel", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("CancelPageStateRequest", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("LoadCefrUpdateStatusesAsync(localDatabasePath, cancellationToken)", codeBehindSource, StringComparison.Ordinal);
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
        Assert.Contains("GetStartingWordPublicIdAsync", codeBehindSource, StringComparison.Ordinal);
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
        Assert.Contains("CancelRefreshRequest", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("Task.WhenAll", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("OutcomeButtonsGrid", practiceSessionXamlSource, StringComparison.Ordinal);
        Assert.Contains("SummaryBorder", practiceSessionXamlSource, StringComparison.Ordinal);
        Assert.Contains("IPracticeFlashcardAnswerService", practiceSessionCodeBehindSource, StringComparison.Ordinal);
        Assert.Contains("IPracticeQuizAnswerService", practiceSessionCodeBehindSource, StringComparison.Ordinal);
        Assert.Contains("CancelSessionRequest", practiceSessionCodeBehindSource, StringComparison.Ordinal);
        Assert.Contains("_isSubmittingOutcome", practiceSessionCodeBehindSource, StringComparison.Ordinal);
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
        Assert.Contains("CefrNavigationTopGrid", xamlSource, StringComparison.Ordinal);
        Assert.Contains("PreviousWordButtonTop", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ShowWordListButtonTop", xamlSource, StringComparison.Ordinal);
        Assert.Contains("NextWordButtonTop", xamlSource, StringComparison.Ordinal);
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
        Assert.Contains("ApplyCefrNavigationStateAsync", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("GetWordStateAsync", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("CancelRefreshRequest", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("OnPreviousWordButtonClicked", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("OnShowWordListButtonClicked", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("LexiconTagDisplayText", codeBehindSource, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that the search page keeps the richer search-panel and results-panel layout.
    /// </summary>
    [Fact]
    public void SearchWordsPage_ShouldExposeSearchAndResultsPanels()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string cefrPagePath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/CefrWordsPage.xaml");
        string searchPagePath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/SearchWordsPage.xaml");
        string searchCodeBehindPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/SearchWordsPage.xaml.cs");

        Assert.True(File.Exists(cefrPagePath), $"CEFR page XAML file not found: {cefrPagePath}");
        Assert.True(File.Exists(searchPagePath), $"Search page XAML file not found: {searchPagePath}");
        Assert.True(File.Exists(searchCodeBehindPath), $"Search page code-behind file not found: {searchCodeBehindPath}");

        string cefrXamlSource = File.ReadAllText(cefrPagePath);
        string xamlSource = File.ReadAllText(searchPagePath);
        string codeBehindSource = File.ReadAllText(searchCodeBehindPath);

        Assert.Contains("RemainingItemsThresholdReached", cefrXamlSource, StringComparison.Ordinal);
        Assert.Contains("StatusBadgeLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("SearchSectionLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("SearchHintLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("ResultsSectionLabel", xamlSource, StringComparison.Ordinal);
        Assert.Contains("SearchBarControl", xamlSource, StringComparison.Ordinal);
        Assert.Contains("WordsCollectionView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("SearchWordsPageSearchHint", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("SearchWordsPageResultsLabel", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("CancelSearchRequest", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("GetCurrentProfileAsync(cancellationToken)", codeBehindSource, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that CEFR-scoped remote updates clean out dependent rows before reinserting content.
    /// </summary>
    [Fact]
    public void RemoteContentUpdateService_ShouldDeleteDependentRowsBeforeCefrReplace()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string remoteUpdateServicePath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Services/Updates/RemoteContentUpdateService.cs");

        Assert.True(File.Exists(remoteUpdateServicePath), $"Remote content update service file not found: {remoteUpdateServicePath}");

        string source = File.ReadAllText(remoteUpdateServicePath);

        Assert.Contains("DELETE FROM ExampleTranslations", source, StringComparison.Ordinal);
        Assert.Contains("DELETE FROM ExampleSentences", source, StringComparison.Ordinal);
        Assert.Contains("DELETE FROM SenseTranslations", source, StringComparison.Ordinal);
        Assert.Contains("DELETE FROM WordTopics", source, StringComparison.Ordinal);
        Assert.Contains("DELETE FROM WordSenses", source, StringComparison.Ordinal);
        Assert.Contains("DELETE FROM ContentPackageEntries", source, StringComparison.Ordinal);
        Assert.Contains("DELETE FROM ContentPackages", source, StringComparison.Ordinal);
        Assert.Contains("DELETE FROM WordEntries WHERE PrimaryCefrLevel = $cefrLevel", source, StringComparison.Ordinal);
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

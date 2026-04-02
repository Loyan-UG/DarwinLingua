namespace DarwinLingua.Localization.Application.Tests;

/// <summary>
/// Provides practical smoke checks for the MAUI startup orchestration path.
/// </summary>
public sealed class MauiStartupSmokeTests
{
    /// <summary>
    /// Verifies that MAUI startup still invokes database initialization and localization initialization.
    /// </summary>
    [Fact]
    public void MauiProgram_ShouldInvokeRequiredStartupInitializationCalls()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string mauiProgramPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/MauiProgram.cs");
        string startupServicePath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Services/Startup/AppStartupInitializationService.cs");
        string backgroundRemoteUpdateCoordinatorPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Services/Updates/BackgroundRemoteUpdateCoordinator.cs");
        string startupPageCodeBehindPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/StartupPage.xaml.cs");
        string appPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/App.xaml.cs");

        Assert.True(File.Exists(mauiProgramPath), $"Startup source file not found: {mauiProgramPath}");
        Assert.True(File.Exists(startupServicePath), $"Startup initialization service file not found: {startupServicePath}");
        Assert.True(File.Exists(backgroundRemoteUpdateCoordinatorPath), $"Background remote update coordinator file not found: {backgroundRemoteUpdateCoordinatorPath}");
        Assert.True(File.Exists(startupPageCodeBehindPath), $"Startup page code-behind file not found: {startupPageCodeBehindPath}");
        Assert.True(File.Exists(appPath), $"App source file not found: {appPath}");

        string mauiProgramSource = File.ReadAllText(mauiProgramPath);
        string startupServiceSource = File.ReadAllText(startupServicePath);
        string backgroundRemoteUpdateCoordinatorSource = File.ReadAllText(backgroundRemoteUpdateCoordinatorPath);
        string startupPageCodeBehindSource = File.ReadAllText(startupPageCodeBehindPath);
        string appSource = File.ReadAllText(appPath);

        Assert.Contains("IAppStartupInitializationService", mauiProgramSource, StringComparison.Ordinal);
        Assert.Contains("IBackgroundRemoteUpdateCoordinator", mauiProgramSource, StringComparison.Ordinal);
        Assert.Contains("IPlatformBackgroundUpdateScheduler", mauiProgramSource, StringComparison.Ordinal);
        Assert.Contains("AddSingleton<StartupPage>()", mauiProgramSource, StringComparison.Ordinal);
        Assert.Contains("IDatabaseInitializer", startupServiceSource, StringComparison.Ordinal);
        Assert.Contains("_databaseInitializer.InitializeAsync(cancellationToken)", startupServiceSource, StringComparison.Ordinal);
        Assert.Contains("IAppLocalizationService", startupServiceSource, StringComparison.Ordinal);
        Assert.Contains("_appLocalizationService.InitializeAsync(cancellationToken)", startupServiceSource, StringComparison.Ordinal);
        Assert.Contains("InitializeCoreAsync(cancellationToken)", startupServiceSource, StringComparison.Ordinal);
        Assert.Contains("ScheduleInitialCheck", backgroundRemoteUpdateCoordinatorSource, StringComparison.Ordinal);
        Assert.Contains("ScheduleResumeCheck", backgroundRemoteUpdateCoordinatorSource, StringComparison.Ordinal);
        Assert.Contains("ApplyFullUpdateAsync", backgroundRemoteUpdateCoordinatorSource, StringComparison.Ordinal);
        Assert.Contains("StartupCompleted?.Invoke", startupPageCodeBehindSource, StringComparison.Ordinal);
        Assert.Contains("GetStartupPage()", appSource, StringComparison.Ordinal);
        Assert.Contains("GetAppShell()", appSource, StringComparison.Ordinal);
        Assert.Contains("GetWelcomePage()", appSource, StringComparison.Ordinal);
        Assert.Contains("OnWindowResumed", appSource, StringComparison.Ordinal);
        Assert.Contains("EnsureScheduled", appSource, StringComparison.Ordinal);
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

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

        Assert.True(File.Exists(mauiProgramPath), $"Startup source file not found: {mauiProgramPath}");

        string sourceCode = File.ReadAllText(mauiProgramPath);

        Assert.Contains("IDatabaseInitializer", sourceCode, StringComparison.Ordinal);
        Assert.Contains(".InitializeAsync(CancellationToken.None)", sourceCode, StringComparison.Ordinal);
        Assert.Contains("IAppLocalizationService", sourceCode, StringComparison.Ordinal);
        Assert.Contains("localizationService.InitializeAsync(CancellationToken.None)", sourceCode, StringComparison.Ordinal);
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

namespace DarwinDeutsch.Maui.Services.Startup;

/// <summary>
/// Performs application startup initialization without blocking the first window.
/// </summary>
public interface IAppStartupInitializationService
{
    /// <summary>
    /// Ensures startup initialization has completed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The initialization result.</returns>
    Task<AppStartupInitializationResult> InitializeAsync(CancellationToken cancellationToken);
}

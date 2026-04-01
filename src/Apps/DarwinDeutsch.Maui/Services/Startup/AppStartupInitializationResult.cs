namespace DarwinDeutsch.Maui.Services.Startup;

/// <summary>
/// Represents the outcome of the startup initialization flow.
/// </summary>
public sealed record AppStartupInitializationResult(
    bool IsSuccess,
    string? ErrorMessage)
{
    /// <summary>
    /// Gets the successful startup result.
    /// </summary>
    public static AppStartupInitializationResult Success { get; } = new(true, null);
}

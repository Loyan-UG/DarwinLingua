namespace DarwinLingua.Web.Services;

/// <summary>
/// Initializes the web-specific authenticated-user state tables that live beside Identity.
/// </summary>
public interface IWebUserStateDatabaseBootstrapper
{
    /// <summary>
    /// Creates the web user preference, favorite, and word-state tables when they are missing.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken);
}

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Initializes and bootstraps the shared-content PostgreSQL database.
/// </summary>
public interface IServerContentDatabaseBootstrapper
{
    /// <summary>
    /// Ensures the database exists and seeds the configured baseline data.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken);
}

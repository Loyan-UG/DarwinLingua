namespace DarwinLingua.Infrastructure.Persistence.Abstractions;

/// <summary>
/// Defines the startup workflow responsible for creating and preparing the local database.
/// </summary>
public interface IDatabaseInitializer
{
    /// <summary>
    /// Ensures that the local database exists and its schema is prepared for Phase 1 workflows.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel schema initialization.</param>
    /// <returns>A task that completes when schema preparation finishes.</returns>
    Task EnsureDatabaseSchemaAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Executes all registered reference-data seeders.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel seeding.</param>
    /// <returns>A task that completes when seeding finishes.</returns>
    Task SeedReferenceDataAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Ensures that the local database exists and all registered seeders have run.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the initialization flow.</param>
    /// <returns>A task that completes when initialization finishes.</returns>
    Task InitializeAsync(CancellationToken cancellationToken);
}

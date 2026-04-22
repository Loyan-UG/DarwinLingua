namespace DarwinLingua.Infrastructure.Persistence.Options;

/// <summary>
/// Holds the SQLite database path configured by the current host.
/// </summary>
public sealed class SqliteDatabaseOptions
{
    /// <summary>
    /// Gets or sets the absolute path of the SQLite database file.
    /// </summary>
    public string DatabasePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional path of a packaged seed database whose content packages
    /// should be merged into the runtime database during initialization.
    /// </summary>
    public string? SeedDatabasePath { get; set; }
}

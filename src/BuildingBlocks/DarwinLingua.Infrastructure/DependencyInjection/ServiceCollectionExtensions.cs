using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.Infrastructure.Persistence.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Infrastructure.DependencyInjection;

/// <summary>
/// Registers the shared infrastructure services used by the current hosts.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the shared SQLite-backed infrastructure services.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <param name="configureOptions">The callback that provides the database path.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddDarwinLinguaInfrastructure(
        this IServiceCollection services,
        Action<SqliteDatabaseOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        SqliteDatabaseOptions sqliteDatabaseOptions = new();
        configureOptions(sqliteDatabaseOptions);

        if (string.IsNullOrWhiteSpace(sqliteDatabaseOptions.DatabasePath))
        {
            throw new InvalidOperationException("The SQLite database path must be configured.");
        }

        string databasePath = Path.GetFullPath(sqliteDatabaseOptions.DatabasePath);
        string? directoryPath = Path.GetDirectoryName(databasePath);

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new InvalidOperationException("The SQLite database path must include a directory.");
        }

        Directory.CreateDirectory(directoryPath);

        services.AddSingleton(new SqliteDatabaseOptions
        {
            DatabasePath = databasePath,
            SeedDatabasePath = string.IsNullOrWhiteSpace(sqliteDatabaseOptions.SeedDatabasePath)
                ? null
                : Path.GetFullPath(sqliteDatabaseOptions.SeedDatabasePath),
        });

        services.AddDbContextFactory<DarwinLinguaDbContext>(options =>
        {
            options.UseSqlite($"Data Source={databasePath}");
        });

        services.AddSingleton<IDatabaseInitializer, DarwinLinguaDatabaseInitializer>();
        services.AddSingleton<ITransactionalExecutionService, TransactionalExecutionService>();

        return services;
    }

    /// <summary>
    /// Adds the shared PostgreSQL-backed infrastructure services used by the server-side hosts.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddDarwinLinguaInfrastructureForPostgres(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContextFactory<DarwinLinguaDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddSingleton<IDatabaseInitializer, DarwinLinguaDatabaseInitializer>();
        services.AddSingleton<ITransactionalExecutionService, TransactionalExecutionService>();

        return services;
    }
}

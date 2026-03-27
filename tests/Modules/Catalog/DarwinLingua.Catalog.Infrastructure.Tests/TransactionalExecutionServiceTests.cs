using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.Localization.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Catalog.Infrastructure.Tests;

/// <summary>
/// Verifies shared transactional execution support for write workflows.
/// </summary>
public sealed class TransactionalExecutionServiceTests
{
    /// <summary>
    /// Verifies that successful transactional execution commits inserted rows.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCommitChanges_WhenOperationSucceeds()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-transaction-commit-{Guid.NewGuid():N}.db");
        await using ServiceProvider serviceProvider = BuildServiceProvider(databasePath);

        IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.EnsureDatabaseSchemaAsync(CancellationToken.None);

        ITransactionalExecutionService transactionalExecutionService =
            serviceProvider.GetRequiredService<ITransactionalExecutionService>();

        await transactionalExecutionService.ExecuteAsync(
            async (dbContext, cancellationToken) =>
            {
                dbContext.Languages.Add(new Language(
                    Guid.NewGuid(),
                    LanguageCode.From("fr"),
                    "French",
                    "Français",
                    true,
                    false,
                    false));

                await Task.CompletedTask;
            },
            CancellationToken.None);

        IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
            serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();
        await using DarwinLinguaDbContext verificationContext = await dbContextFactory
            .CreateDbContextAsync(CancellationToken.None);

        Assert.Equal(1, await verificationContext.Languages.CountAsync(cancellationToken: CancellationToken.None));
    }

    /// <summary>
    /// Verifies that failed transactional execution rolls back inserted rows.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldRollbackChanges_WhenOperationFails()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-transaction-rollback-{Guid.NewGuid():N}.db");
        await using ServiceProvider serviceProvider = BuildServiceProvider(databasePath);

        IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.EnsureDatabaseSchemaAsync(CancellationToken.None);

        ITransactionalExecutionService transactionalExecutionService =
            serviceProvider.GetRequiredService<ITransactionalExecutionService>();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            transactionalExecutionService.ExecuteAsync(
                (dbContext, _) =>
                {
                    dbContext.Languages.Add(new Language(
                        Guid.NewGuid(),
                        LanguageCode.From("it"),
                        "Italian",
                        "Italiano",
                        true,
                        false,
                        false));

                    return Task.FromException(new InvalidOperationException("Force rollback."));
                },
                CancellationToken.None));

        IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
            serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();
        await using DarwinLinguaDbContext verificationContext = await dbContextFactory
            .CreateDbContextAsync(CancellationToken.None);

        Assert.Equal(0, await verificationContext.Languages.CountAsync(cancellationToken: CancellationToken.None));
    }

    /// <summary>
    /// Builds the service provider used by transaction tests.
    /// </summary>
    private static ServiceProvider BuildServiceProvider(string databasePath)
    {
        ServiceCollection services = new();
        services.AddDarwinLinguaInfrastructure(options => options.DatabasePath = databasePath);

        return services.BuildServiceProvider();
    }
}

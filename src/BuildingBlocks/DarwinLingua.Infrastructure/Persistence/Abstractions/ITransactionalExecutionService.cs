using DarwinLingua.Infrastructure.Persistence;

namespace DarwinLingua.Infrastructure.Persistence.Abstractions;

/// <summary>
/// Provides a shared transactional execution boundary for write workflows.
/// </summary>
public interface ITransactionalExecutionService
{
    /// <summary>
    /// Executes a write workflow inside a single database transaction.
    /// </summary>
    Task ExecuteAsync(
        Func<DarwinLinguaDbContext, CancellationToken, Task> writeOperation,
        CancellationToken cancellationToken);

    /// <summary>
    /// Executes a write workflow inside a single database transaction and returns a result.
    /// </summary>
    Task<TResult> ExecuteAsync<TResult>(
        Func<DarwinLinguaDbContext, CancellationToken, Task<TResult>> writeOperation,
        CancellationToken cancellationToken);
}

using DarwinLingua.Infrastructure.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Infrastructure.Persistence;

/// <summary>
/// Implements a reusable transactional boundary for write workflows.
/// </summary>
internal sealed class TransactionalExecutionService : ITransactionalExecutionService
{
    private readonly IDbContextFactory<DarwinLinguaDbContext> _dbContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionalExecutionService"/> class.
    /// </summary>
    public TransactionalExecutionService(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory)
    {
        ArgumentNullException.ThrowIfNull(dbContextFactory);

        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(
        Func<DarwinLinguaDbContext, CancellationToken, Task> writeOperation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(writeOperation);

        await ExecuteAsync<object?>(
            async (dbContext, operationCancellationToken) =>
            {
                await writeOperation(dbContext, operationCancellationToken).ConfigureAwait(false);
                return null;
            },
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TResult> ExecuteAsync<TResult>(
        Func<DarwinLinguaDbContext, CancellationToken, Task<TResult>> writeOperation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(writeOperation);

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            TResult result = await writeOperation(dbContext, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }
}

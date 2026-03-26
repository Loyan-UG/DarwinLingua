using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Learning.Infrastructure.Repositories;

/// <summary>
/// Reads and writes the local user's lightweight word state in the shared SQLite database.
/// </summary>
internal sealed class UserWordStateRepository : IUserWordStateRepository
{
    private readonly IDbContextFactory<DarwinLinguaDbContext> _dbContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserWordStateRepository"/> class.
    /// </summary>
    public UserWordStateRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory)
    {
        ArgumentNullException.ThrowIfNull(dbContextFactory);

        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public async Task<UserWordState?> GetByUserIdAndWordPublicIdAsync(
        string userId,
        Guid wordEntryPublicId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        if (wordEntryPublicId == Guid.Empty)
        {
            throw new ArgumentException("Word public identifier cannot be empty.", nameof(wordEntryPublicId));
        }

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        return await dbContext.UserWordStates
            .SingleOrDefaultAsync(
                userWordState => userWordState.UserId == userId && userWordState.WordEntryPublicId == wordEntryPublicId,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AddAsync(UserWordState userWordState, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userWordState);

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        dbContext.UserWordStates.Add(userWordState);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(UserWordState userWordState, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userWordState);

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        dbContext.UserWordStates.Update(userWordState);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

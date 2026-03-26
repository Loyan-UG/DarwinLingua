using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.SharedKernel.Content;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Learning.Infrastructure.Services;

/// <summary>
/// Reads lexical catalog existence data required for user-word-state workflows.
/// </summary>
internal sealed class UserWordStateCatalogReader : IUserWordStateCatalogReader
{
    private readonly IDbContextFactory<DarwinLinguaDbContext> _dbContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserWordStateCatalogReader"/> class.
    /// </summary>
    public UserWordStateCatalogReader(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory)
    {
        ArgumentNullException.ThrowIfNull(dbContextFactory);

        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(Guid wordEntryPublicId, CancellationToken cancellationToken)
    {
        if (wordEntryPublicId == Guid.Empty)
        {
            throw new ArgumentException("Word public identifier cannot be empty.", nameof(wordEntryPublicId));
        }

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        return await dbContext.WordEntries
            .AsNoTracking()
            .AnyAsync(
                word => word.PublicId == wordEntryPublicId && word.PublicationStatus == PublicationStatus.Active,
                cancellationToken)
            .ConfigureAwait(false);
    }
}

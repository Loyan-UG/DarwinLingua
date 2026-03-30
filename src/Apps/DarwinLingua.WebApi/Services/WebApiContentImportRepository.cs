using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Provides content-import persistence against the shared PostgreSQL-backed catalog database.
/// </summary>
public sealed class WebApiContentImportRepository : IContentImportRepository
{
    private readonly IDbContextFactory<DarwinLinguaDbContext> _dbContextFactory;

    public WebApiContentImportRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory)
    {
        ArgumentNullException.ThrowIfNull(dbContextFactory);

        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyDictionary<string, Topic>> GetActiveTopicsByKeyAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await dbContext.Topics
            .AsNoTracking()
            .ToDictionaryAsync(topic => topic.Key, StringComparer.Ordinal, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlySet<LanguageCode>> GetActiveMeaningLanguagesAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        LanguageCode[] languageCodes = await dbContext.Languages
            .AsNoTracking()
            .Where(language => language.IsActive && language.SupportsMeanings)
            .Select(language => language.Code)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new HashSet<LanguageCode>(languageCodes);
    }

    public async Task<bool> PackageExistsAsync(string packageId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await dbContext.ContentPackages
            .AsNoTracking()
            .AnyAsync(contentPackage => contentPackage.PackageId == packageId.Trim(), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> WordExistsAsync(string normalizedLemma, PartOfSpeech partOfSpeech, CefrLevel cefrLevel, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedLemma);

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await dbContext.WordEntries
            .AsNoTracking()
            .AnyAsync(
                word => word.NormalizedLemma == normalizedLemma &&
                    word.PartOfSpeech == partOfSpeech &&
                    word.PrimaryCefrLevel == cefrLevel,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task PersistImportAsync(ContentPackage contentPackage, IReadOnlyList<WordEntry> importedWords, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(contentPackage);
        ArgumentNullException.ThrowIfNull(importedWords);

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        dbContext.AddRange(importedWords);
        dbContext.Add(contentPackage);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}

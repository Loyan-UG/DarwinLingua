using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
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

    public async Task<IReadOnlyList<WordEntry>> GetActiveWordsByNormalizedLemmasAsync(
        IReadOnlyCollection<string> normalizedLemmas,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(normalizedLemmas);

        if (normalizedLemmas.Count == 0)
        {
            return [];
        }

        string[] normalizedLemmaArray = normalizedLemmas
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim().ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalizedLemmaArray.Length == 0)
        {
            return [];
        }

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await dbContext.WordEntries
            .AsNoTracking()
            .Where(word => word.PublicationStatus == PublicationStatus.Active)
            .Where(word => normalizedLemmaArray.Contains(word.NormalizedLemma))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task PersistImportAsync(
        ContentPackage contentPackage,
        IReadOnlyList<WordEntry> importedWords,
        IReadOnlyList<WordCollection> importedCollections,
        IReadOnlyList<ScenarioLesson> importedScenarios,
        IReadOnlyList<ConversationStarterPack> importedConversationStarterPacks,
        IReadOnlyList<EventPreparationPack> importedEventPreparationPacks,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(contentPackage);
        ArgumentNullException.ThrowIfNull(importedWords);
        ArgumentNullException.ThrowIfNull(importedCollections);
        ArgumentNullException.ThrowIfNull(importedScenarios);
        ArgumentNullException.ThrowIfNull(importedConversationStarterPacks);
        ArgumentNullException.ThrowIfNull(importedEventPreparationPacks);

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        dbContext.AddRange(importedWords);
        dbContext.Add(contentPackage);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (importedCollections.Count > 0)
        {
            string[] importedSlugs = importedCollections
                .Select(item => item.Slug)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            List<WordCollection> existingCollections = await dbContext.WordCollections
                .Include(collection => collection.Entries)
                .Where(collection => importedSlugs.Contains(collection.Slug))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (WordCollection importedCollection in importedCollections)
            {
                WordCollection? existingCollection = existingCollections
                    .SingleOrDefault(collection => string.Equals(collection.Slug, importedCollection.Slug, StringComparison.OrdinalIgnoreCase));

                if (existingCollection is null)
                {
                    dbContext.WordCollections.Add(importedCollection);
                    continue;
                }

                existingCollection.UpdateMetadata(
                    importedCollection.Name,
                    importedCollection.Description,
                    importedCollection.ImageUrl,
                    importedCollection.PublicationStatus,
                    importedCollection.SortOrder,
                    importedCollection.UpdatedAtUtc);

                existingCollection.ReplaceEntries(
                    importedCollection.Entries
                        .Select(entry => (entry.WordEntryId, entry.SortOrder))
                        .ToArray(),
                    importedCollection.UpdatedAtUtc);
            }

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (importedScenarios.Count > 0)
        {
            string[] importedSlugs = importedScenarios
                .Select(item => item.Slug)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            List<ScenarioLesson> existingScenarios = await dbContext.ScenarioLessons
                .Where(scenario => importedSlugs.Contains(scenario.Slug))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (existingScenarios.Count > 0)
            {
                dbContext.ScenarioLessons.RemoveRange(existingScenarios);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            dbContext.ScenarioLessons.AddRange(importedScenarios);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (importedConversationStarterPacks.Count > 0)
        {
            string[] importedSlugs = importedConversationStarterPacks
                .Select(item => item.Slug)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            List<ConversationStarterPack> existingStarterPacks = await dbContext.ConversationStarterPacks
                .Where(pack => importedSlugs.Contains(pack.Slug))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (existingStarterPacks.Count > 0)
            {
                dbContext.ConversationStarterPacks.RemoveRange(existingStarterPacks);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            dbContext.ConversationStarterPacks.AddRange(importedConversationStarterPacks);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (importedEventPreparationPacks.Count > 0)
        {
            string[] importedSlugs = importedEventPreparationPacks
                .Select(item => item.Slug)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            List<EventPreparationPack> existingEventPreparationPacks = await dbContext.EventPreparationPacks
                .Where(pack => importedSlugs.Contains(pack.Slug))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (existingEventPreparationPacks.Count > 0)
            {
                dbContext.EventPreparationPacks.RemoveRange(existingEventPreparationPacks);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            dbContext.EventPreparationPacks.AddRange(importedEventPreparationPacks);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}

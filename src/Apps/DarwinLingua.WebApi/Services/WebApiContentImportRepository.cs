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

    public async Task<bool> WordExistsAsync(string normalizedLemma, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedLemma);

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await dbContext.WordEntries
            .AsNoTracking()
            .AnyAsync(word => word.NormalizedLemma == normalizedLemma, cancellationToken)
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
        IReadOnlyList<LabelDefinition> importedLabelDefinitions,
        IReadOnlyList<WordEntry> importedWords,
        IReadOnlyList<WordCollection> importedCollections,
        IReadOnlyList<DialogueLesson> importedDialogues,
        IReadOnlyList<TalkTopic> importedTalkTopics,
        IReadOnlyList<ConversationStarterPack> importedConversationStarterPacks,
        IReadOnlyList<EventPreparationPack> importedEventPreparationPacks,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(contentPackage);
        ArgumentNullException.ThrowIfNull(importedLabelDefinitions);
        ArgumentNullException.ThrowIfNull(importedWords);
        ArgumentNullException.ThrowIfNull(importedCollections);
        ArgumentNullException.ThrowIfNull(importedDialogues);
        ArgumentNullException.ThrowIfNull(importedTalkTopics);
        ArgumentNullException.ThrowIfNull(importedConversationStarterPacks);
        ArgumentNullException.ThrowIfNull(importedEventPreparationPacks);

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        if (importedLabelDefinitions.Count > 0)
        {
            string[] importedLabelDefinitionKeys = importedLabelDefinitions
                .Select(label => label.Key)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            List<LabelDefinition> existingLabels = await dbContext.LabelDefinitions
                .Include(label => label.Localizations)
                .Where(label => importedLabelDefinitionKeys.Contains(label.Key))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (LabelDefinition importedLabel in importedLabelDefinitions)
            {
                LabelDefinition? existingLabel = existingLabels.SingleOrDefault(label =>
                    label.Kind == importedLabel.Kind &&
                    string.Equals(label.Key, importedLabel.Key, StringComparison.Ordinal));

                if (existingLabel is null)
                {
                    dbContext.LabelDefinitions.Add(importedLabel);
                    continue;
                }

                existingLabel.UpdateMetadata(
                    importedLabel.DisplayName,
                    importedLabel.SortOrder,
                    importedLabel.IsSystem,
                    importedLabel.UpdatedAtUtc);

                foreach (LabelDefinitionLocalization localization in importedLabel.Localizations)
                {
                    Guid localizationId = existingLabel.Localizations.Any(item => item.LanguageCode == localization.LanguageCode)
                        ? Guid.Empty
                        : Guid.NewGuid();

                    existingLabel.AddOrUpdateLocalization(
                        localizationId,
                        localization.LanguageCode,
                        localization.DisplayName,
                        importedLabel.UpdatedAtUtc);
                }
            }
        }

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
                .Include(collection => collection.Localizations)
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

                foreach (WordCollectionLocalization localization in importedCollection.Localizations)
                {
                    Guid localizationId = existingCollection.Localizations.Any(item => item.LanguageCode == localization.LanguageCode)
                        ? Guid.Empty
                        : Guid.NewGuid();

                    existingCollection.AddOrUpdateLocalization(
                        localizationId,
                        localization.LanguageCode,
                        localization.Name,
                        localization.Description,
                        importedCollection.UpdatedAtUtc);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (importedDialogues.Count > 0)
        {
            string[] importedSlugs = importedDialogues
                .Select(item => item.Slug)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            List<DialogueLesson> existingDialogues = await dbContext.DialogueLessons
                .Where(dialogue => importedSlugs.Contains(dialogue.Slug))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (existingDialogues.Count > 0)
            {
                dbContext.DialogueLessons.RemoveRange(existingDialogues);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            dbContext.DialogueLessons.AddRange(importedDialogues);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (importedTalkTopics.Count > 0)
        {
            string[] importedSlugs = importedTalkTopics
                .Select(item => item.Slug)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            List<TalkTopic> existingTalkTopics = await dbContext.TalkTopics
                .Where(topic => importedSlugs.Contains(topic.Slug))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (existingTalkTopics.Count > 0)
            {
                dbContext.TalkTopics.RemoveRange(existingTalkTopics);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            dbContext.TalkTopics.AddRange(importedTalkTopics);
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

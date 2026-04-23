using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class WordCollectionRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IWordCollectionRepository
{
    public async Task<IReadOnlyList<WordCollectionListItemModel>> GetPublishedCollectionsAsync(
        string meaningLanguageCode,
        CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        List<WordCollectionProjection> collections = await dbContext.WordCollections
            .AsNoTracking()
            .Where(collection => collection.PublicationStatus == PublicationStatus.Active)
            .OrderBy(collection => collection.SortOrder)
            .ThenBy(collection => collection.Name)
            .Select(collection => new WordCollectionProjection(
                collection.Slug,
                collection.Name,
                collection.Description,
                collection.ImageUrl,
                collection.Entries
                    .OrderBy(entry => entry.SortOrder)
                    .Select(entry => new WordCollectionWordProjection(
                        entry.WordEntry!.Id,
                        entry.WordEntry.PublicId,
                        entry.WordEntry.Lemma,
                        entry.WordEntry.Article,
                        entry.WordEntry.PluralForm,
                        entry.WordEntry.PartOfSpeech.ToString(),
                        entry.WordEntry.PrimaryCefrLevel.ToString(),
                        entry.SortOrder))
                    .ToArray()))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<Guid, string?> meaningsByWordEntryId = await LoadPrimaryMeaningsAsync(
                dbContext,
                collections.SelectMany(collection => collection.Words).Select(word => word.WordEntryId).Distinct().ToArray(),
                meaningLanguageCode,
                cancellationToken)
            .ConfigureAwait(false);

        return collections
            .Select(collection => new WordCollectionListItemModel(
                collection.Slug,
                collection.Name,
                collection.Description,
                collection.ImageUrl,
                collection.Words.Count,
                collection.Words
                    .Select(word => word.CefrLevel)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(level => level, StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
                collection.Words
                    .OrderBy(word => word.SortOrder)
                    .Take(4)
                    .Select(word =>
                    {
                        string? meaning = meaningsByWordEntryId.GetValueOrDefault(word.WordEntryId);
                        return string.IsNullOrWhiteSpace(meaning)
                            ? word.Lemma
                            : $"{word.Lemma} ({meaning})";
                    })
                    .ToArray()))
            .ToArray();
    }

    public async Task<WordCollectionDetailModel?> GetPublishedCollectionBySlugAsync(
        string slug,
        string meaningLanguageCode,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        string normalizedSlug = slug.Trim().ToLowerInvariant();

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        WordCollectionProjection? collection = await dbContext.WordCollections
            .AsNoTracking()
            .Where(item => item.PublicationStatus == PublicationStatus.Active && item.Slug == normalizedSlug)
            .Select(item => new WordCollectionProjection(
                item.Slug,
                item.Name,
                item.Description,
                item.ImageUrl,
                item.Entries
                    .OrderBy(entry => entry.SortOrder)
                    .Select(entry => new WordCollectionWordProjection(
                        entry.WordEntry!.Id,
                        entry.WordEntry.PublicId,
                        entry.WordEntry.Lemma,
                        entry.WordEntry.Article,
                        entry.WordEntry.PluralForm,
                        entry.WordEntry.PartOfSpeech.ToString(),
                        entry.WordEntry.PrimaryCefrLevel.ToString(),
                        entry.SortOrder))
                    .ToArray()))
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (collection is null)
        {
            return null;
        }

        Dictionary<Guid, string?> meaningsByWordEntryId = await LoadPrimaryMeaningsAsync(
                dbContext,
                collection.Words.Select(word => word.WordEntryId).Distinct().ToArray(),
                meaningLanguageCode,
                cancellationToken)
            .ConfigureAwait(false);

        WordListItemModel[] words = collection.Words
            .OrderBy(word => word.SortOrder)
            .Select(word => new WordListItemModel(
                word.PublicId,
                word.Lemma,
                word.Article,
                word.PluralForm,
                word.PartOfSpeech,
                word.CefrLevel,
                meaningsByWordEntryId.GetValueOrDefault(word.WordEntryId)))
            .ToArray();

        return new WordCollectionDetailModel(
            collection.Slug,
            collection.Name,
            collection.Description,
            collection.ImageUrl,
            words.Length,
            words
                .Select(word => word.CefrLevel)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(level => level, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            words);
    }

    private static async Task<Dictionary<Guid, string?>> LoadPrimaryMeaningsAsync(
        DarwinLinguaDbContext dbContext,
        IReadOnlyCollection<Guid> wordEntryIds,
        string meaningLanguageCode,
        CancellationToken cancellationToken)
    {
        if (wordEntryIds.Count == 0)
        {
            return [];
        }

        LanguageCode resolvedMeaningLanguageCode = LanguageCode.From(meaningLanguageCode);

        List<CollectionPrimarySenseRow> primarySenses = await dbContext.WordSenses
            .AsNoTracking()
            .Where(sense => wordEntryIds.Contains(sense.WordEntryId))
            .Where(sense => sense.PublicationStatus == PublicationStatus.Active)
            .Select(sense => new CollectionPrimarySenseRow(
                sense.Id,
                sense.WordEntryId,
                sense.IsPrimarySense,
                sense.SenseOrder))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (primarySenses.Count == 0)
        {
            return [];
        }

        Guid[] senseIds = primarySenses
            .Select(sense => sense.SenseId)
            .ToArray();

        List<CollectionPrimaryMeaningTranslationRow> translations = await dbContext.SenseTranslations
            .AsNoTracking()
            .Where(translation => translation.LanguageCode == resolvedMeaningLanguageCode)
            .Where(translation => senseIds.Contains(translation.WordSenseId))
            .Select(translation => new CollectionPrimaryMeaningTranslationRow(
                translation.WordSenseId,
                translation.TranslationText,
                translation.IsPrimary))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<CollectionPrimaryMeaningRow> meanings = translations
            .Join(
                primarySenses,
                translation => translation.WordSenseId,
                sense => sense.SenseId,
                (translation, sense) => new CollectionPrimaryMeaningRow(
                    sense.WordEntryId,
                    sense.IsPrimarySense,
                    sense.SenseOrder,
                    translation.IsPrimary,
                    translation.TranslationText))
            .ToList();

        return meanings
            .GroupBy(item => item.WordEntryId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(item => item.IsPrimarySense)
                    .ThenBy(item => item.SenseOrder)
                    .ThenByDescending(item => item.IsPrimary)
                    .Select(item => (string?)item.TranslationText)
                    .FirstOrDefault());
    }

    private sealed record WordCollectionProjection(
        string Slug,
        string Name,
        string? Description,
        string? ImageUrl,
        IReadOnlyList<WordCollectionWordProjection> Words);

    private sealed record WordCollectionWordProjection(
        Guid WordEntryId,
        Guid PublicId,
        string Lemma,
        string? Article,
        string? PluralForm,
        string PartOfSpeech,
        string CefrLevel,
        int SortOrder);

    private sealed record CollectionPrimarySenseRow(
        Guid SenseId,
        Guid WordEntryId,
        bool IsPrimarySense,
        int SenseOrder);

    private sealed record CollectionPrimaryMeaningTranslationRow(
        Guid WordSenseId,
        string TranslationText,
        bool IsPrimary);

    private sealed record CollectionPrimaryMeaningRow(
        Guid WordEntryId,
        bool IsPrimarySense,
        int SenseOrder,
        bool IsPrimary,
        string TranslationText);
}

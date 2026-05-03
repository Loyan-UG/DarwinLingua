using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

public interface IAdminCollectionsService
{
    Task<AdminCollectionsResponse> GetCollectionsAsync(CancellationToken cancellationToken);

    Task<AdminCollectionDetailResponse?> GetCollectionAsync(Guid collectionId, CancellationToken cancellationToken);

    Task<AdminCollectionDetailResponse> CreateCollectionAsync(AdminSaveCollectionRequest request, CancellationToken cancellationToken);

    Task<AdminCollectionDetailResponse?> UpdateCollectionAsync(Guid collectionId, AdminSaveCollectionRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteCollectionAsync(Guid collectionId, CancellationToken cancellationToken);

    Task<AdminCollectionDetailResponse?> AddWordAsync(Guid collectionId, AdminAddCollectionWordRequest request, CancellationToken cancellationToken);

    Task<AdminCollectionDetailResponse?> DeleteWordAsync(Guid collectionId, Guid entryId, CancellationToken cancellationToken);

    Task<AdminBulkCollectionImportResponse> ImportCollectionsAsync(AdminBulkCollectionImportRequest request, CancellationToken cancellationToken);
}

internal sealed class AdminCollectionsService(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IAdminCollectionsService
{
    public async Task<AdminCollectionsResponse> GetCollectionsAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        AdminCollectionItemResponse[] collections = await dbContext.WordCollections
            .AsNoTracking()
            .Include(collection => collection.Localizations)
            .OrderBy(collection => collection.SortOrder)
            .ThenBy(collection => collection.Name)
            .Select(collection => new AdminCollectionItemResponse(
                collection.Id,
                collection.Slug,
                collection.Name,
                collection.Description,
                collection.Localizations
                    .OrderBy(localization => localization.LanguageCode.Value)
                    .Select(localization => new AdminCollectionLocalizationResponse(
                        localization.Id,
                        localization.LanguageCode.Value,
                        localization.Name,
                        localization.Description))
                    .ToArray(),
                collection.ImageUrl,
                collection.PublicationStatus.ToString(),
                collection.SortOrder,
                collection.Entries.Count,
                collection.UpdatedAtUtc))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminCollectionsResponse(collections);
    }

    public async Task<AdminCollectionDetailResponse?> GetCollectionAsync(Guid collectionId, CancellationToken cancellationToken)
    {
        if (collectionId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        WordCollection? collection = await dbContext.WordCollections
            .AsNoTracking()
            .Include(item => item.Entries)
                .ThenInclude(entry => entry.WordEntry)
            .Include(item => item.Localizations)
            .SingleOrDefaultAsync(item => item.Id == collectionId, cancellationToken)
            .ConfigureAwait(false);

        return collection is null ? null : MapDetail(collection);
    }

    public async Task<AdminCollectionDetailResponse> CreateCollectionAsync(AdminSaveCollectionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        PublicationStatus status = ParseStatus(request.PublicationStatus);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        DateTime now = DateTime.UtcNow;
        WordCollection collection = new(
            Guid.NewGuid(),
            request.Slug,
            request.Name,
            request.Description,
            request.ImageUrl,
            status,
            request.SortOrder,
            now);
        ApplyCollectionLocalizations(collection, request.Localizations, now, requireCompleteCoverage: false);

        dbContext.WordCollections.Add(collection);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MapDetail(collection);
    }

    public async Task<AdminCollectionDetailResponse?> UpdateCollectionAsync(Guid collectionId, AdminSaveCollectionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (collectionId == Guid.Empty)
        {
            return null;
        }

        PublicationStatus status = ParseStatus(request.PublicationStatus);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        WordCollection? collection = await dbContext.WordCollections
            .Include(item => item.Entries)
                .ThenInclude(entry => entry.WordEntry)
            .Include(item => item.Localizations)
            .SingleOrDefaultAsync(item => item.Id == collectionId, cancellationToken)
            .ConfigureAwait(false);

        if (collection is null)
        {
            return null;
        }

        collection.UpdateMetadata(request.Name, request.Description, request.ImageUrl, status, request.SortOrder, DateTime.UtcNow);
        ApplyCollectionLocalizations(collection, request.Localizations, DateTime.UtcNow, requireCompleteCoverage: false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MapDetail(collection);
    }

    public async Task<bool> DeleteCollectionAsync(Guid collectionId, CancellationToken cancellationToken)
    {
        if (collectionId == Guid.Empty)
        {
            return false;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        WordCollection? collection = await dbContext.WordCollections
            .SingleOrDefaultAsync(item => item.Id == collectionId, cancellationToken)
            .ConfigureAwait(false);

        if (collection is null)
        {
            return false;
        }

        dbContext.WordCollections.Remove(collection);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<AdminCollectionDetailResponse?> AddWordAsync(Guid collectionId, AdminAddCollectionWordRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (collectionId == Guid.Empty || request.WordPublicId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        WordCollection? collection = await dbContext.WordCollections
            .Include(item => item.Entries)
            .SingleOrDefaultAsync(item => item.Id == collectionId, cancellationToken)
            .ConfigureAwait(false);

        WordEntry? word = await dbContext.WordEntries
            .SingleOrDefaultAsync(item => item.PublicId == request.WordPublicId, cancellationToken)
            .ConfigureAwait(false);

        if (collection is null || word is null)
        {
            return null;
        }

        int sortOrder = request.SortOrder <= 0
            ? (collection.Entries.Count == 0 ? 1 : collection.Entries.Max(entry => entry.SortOrder) + 1)
            : request.SortOrder;

        if (!collection.Entries.Any(entry => entry.WordEntryId == word.Id))
        {
            collection.AddWord(Guid.NewGuid(), word.Id, sortOrder, DateTime.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return await GetCollectionAsync(collectionId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminCollectionDetailResponse?> DeleteWordAsync(Guid collectionId, Guid entryId, CancellationToken cancellationToken)
    {
        if (collectionId == Guid.Empty || entryId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        WordCollectionEntry? entry = await dbContext.WordCollectionEntries
            .SingleOrDefaultAsync(item => item.WordCollectionId == collectionId && item.Id == entryId, cancellationToken)
            .ConfigureAwait(false);

        if (entry is not null)
        {
            dbContext.WordCollectionEntries.Remove(entry);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return await GetCollectionAsync(collectionId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminBulkCollectionImportResponse> ImportCollectionsAsync(AdminBulkCollectionImportRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Collections.Count == 0)
        {
            throw new InvalidOperationException("The import file must contain at least one collection.");
        }

        List<AdminBulkCollectionImportItemResult> results = [];
        int importedCount = 0;
        int skippedCount = 0;
        int failedCount = 0;

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        for (int index = 0; index < request.Collections.Count; index++)
        {
            AdminBulkCollectionImportItemRequest item = request.Collections[index];
            int rowNumber = index + 1;

            try
            {
                PublicationStatus status = ParseStatus(string.IsNullOrWhiteSpace(item.PublicationStatus) ? "Draft" : item.PublicationStatus);
                ValidateCompleteLocalizations(item.Localizations, "Collection localizations");
                WordCollection? collection = await dbContext.WordCollections
                    .Include(existing => existing.Entries)
                    .Include(existing => existing.Localizations)
                    .SingleOrDefaultAsync(existing => existing.Slug == item.Slug.Trim().ToLowerInvariant(), cancellationToken)
                    .ConfigureAwait(false);

                DateTime now = DateTime.UtcNow;
                bool created = false;
                if (collection is null)
                {
                    collection = new WordCollection(
                        Guid.NewGuid(),
                        item.Slug,
                        item.Name,
                        item.Description,
                        item.ImageUrl,
                        status,
                        item.SortOrder,
                        now);
                    dbContext.WordCollections.Add(collection);
                    created = true;
                }
                else
                {
                    collection.UpdateMetadata(item.Name, item.Description, item.ImageUrl, status, item.SortOrder, now);
                }

                ApplyCollectionLocalizations(collection, item.Localizations, now, requireCompleteCoverage: true);

                IReadOnlyList<AdminBulkCollectionWordImportRequest> words = item.Words ?? [];
                if (words.Count > 0)
                {
                    Guid[] publicIds = words.Select(word => word.WordPublicId).Distinct().ToArray();
                    Dictionary<Guid, Guid> wordIds = await dbContext.WordEntries
                        .Where(word => publicIds.Contains(word.PublicId))
                        .ToDictionaryAsync(word => word.PublicId, word => word.Id, cancellationToken)
                        .ConfigureAwait(false);

                    Guid[] missing = publicIds.Where(publicId => !wordIds.ContainsKey(publicId)).ToArray();
                    if (missing.Length > 0)
                    {
                        throw new InvalidOperationException($"Unknown word public IDs: {string.Join(", ", missing.Select(id => id.ToString("D")))}");
                    }

                    collection.ReplaceEntries(
                        words.Select((word, wordIndex) => (
                            WordEntryId: wordIds[word.WordPublicId],
                            SortOrder: word.SortOrder <= 0 ? wordIndex + 1 : word.SortOrder)),
                        now);
                }

                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                importedCount++;
                results.Add(new AdminBulkCollectionImportItemResult(
                    rowNumber,
                    item.Slug,
                    collection.Id,
                    created ? "Imported" : "Updated",
                    created ? "Collection was created." : "Collection was updated."));
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or DomainRuleException)
            {
                failedCount++;
                results.Add(new AdminBulkCollectionImportItemResult(rowNumber, item.Slug, null, "Failed", ex.Message));
            }
            catch (DbUpdateException ex)
            {
                failedCount++;
                results.Add(new AdminBulkCollectionImportItemResult(rowNumber, item.Slug, null, "Failed", ex.InnerException?.Message ?? ex.Message));
            }
        }

        return new AdminBulkCollectionImportResponse(
            request.Collections.Count,
            importedCount,
            skippedCount,
            failedCount,
            results);
    }

    private static PublicationStatus ParseStatus(string value)
    {
        if (!Enum.TryParse(value, ignoreCase: true, out PublicationStatus status) || !Enum.IsDefined(status))
        {
            throw new InvalidOperationException($"'{value}' is not a supported publication status.");
        }

        return status;
    }

    private static AdminCollectionDetailResponse MapDetail(WordCollection collection) =>
        new(
            collection.Id,
            collection.Slug,
            collection.Name,
            collection.Description,
            MapLocalizations(collection.Localizations),
            collection.ImageUrl,
            collection.PublicationStatus.ToString(),
            collection.SortOrder,
            collection.CreatedAtUtc,
            collection.UpdatedAtUtc,
            collection.Entries
                .OrderBy(entry => entry.SortOrder)
                .Select(entry => new AdminCollectionEntryResponse(
                    entry.Id,
                    entry.WordEntry?.PublicId ?? Guid.Empty,
                    entry.WordEntry?.Lemma ?? "Unknown word",
                    entry.WordEntry?.PartOfSpeech.ToString() ?? "-",
                    entry.WordEntry?.PrimaryCefrLevel.ToString() ?? "-",
                    entry.SortOrder))
                .ToArray());

    private static AdminCollectionLocalizationResponse[] MapLocalizations(IEnumerable<WordCollectionLocalization> localizations) =>
        localizations
            .OrderBy(localization => localization.LanguageCode.Value)
            .Select(localization => new AdminCollectionLocalizationResponse(
                localization.Id,
                localization.LanguageCode.Value,
                localization.Name,
                localization.Description))
            .ToArray();

    private static void ApplyCollectionLocalizations(
        WordCollection collection,
        IReadOnlyList<AdminSaveCollectionLocalizationRequest>? localizations,
        DateTime now,
        bool requireCompleteCoverage)
    {
        if (localizations is null || localizations.Count == 0)
        {
            if (requireCompleteCoverage)
            {
                throw new InvalidOperationException(
                    $"Collection localizations are required for every language: {ContentLanguageRequirements.FormatRequiredLocalizationLanguages()}.");
            }

            return;
        }

        if (requireCompleteCoverage)
        {
            ValidateCompleteLocalizations(localizations, "Collection localizations");
        }

        foreach (AdminSaveCollectionLocalizationRequest localization in localizations)
        {
            LanguageCode languageCode = LanguageCode.From(localization.LanguageCode);
            Guid localizationId = collection.Localizations.Any(item => item.LanguageCode == languageCode)
                ? Guid.Empty
                : Guid.NewGuid();

            collection.AddOrUpdateLocalization(
                localizationId,
                languageCode,
                localization.Name,
                localization.Description,
                now);
        }
    }

    private static void ValidateCompleteLocalizations(
        IReadOnlyList<AdminSaveCollectionLocalizationRequest>? localizations,
        string label)
    {
        if (localizations is null || localizations.Count == 0)
        {
            throw new InvalidOperationException(
                $"{label} are required for every language: {ContentLanguageRequirements.FormatRequiredLocalizationLanguages()}.");
        }

        IReadOnlyList<string> missing = ContentLanguageRequirements.FindMissingLocalizationLanguages(
            localizations.Select(localization => localization.LanguageCode));
        if (missing.Count > 0)
        {
            throw new InvalidOperationException($"{label} are missing languages: {string.Join(", ", missing)}.");
        }

        string[] duplicates = localizations
            .Where(localization => !string.IsNullOrWhiteSpace(localization.LanguageCode))
            .GroupBy(localization => localization.LanguageCode.Trim().ToLowerInvariant())
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        if (duplicates.Length > 0)
        {
            throw new InvalidOperationException($"{label} contain duplicate languages: {string.Join(", ", duplicates)}.");
        }
    }
}

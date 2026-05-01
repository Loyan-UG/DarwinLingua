using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
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
            .OrderBy(collection => collection.SortOrder)
            .ThenBy(collection => collection.Name)
            .Select(collection => new AdminCollectionItemResponse(
                collection.Id,
                collection.Slug,
                collection.Name,
                collection.Description,
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
            .SingleOrDefaultAsync(item => item.Id == collectionId, cancellationToken)
            .ConfigureAwait(false);

        if (collection is null)
        {
            return null;
        }

        collection.UpdateMetadata(request.Name, request.Description, request.ImageUrl, status, request.SortOrder, DateTime.UtcNow);
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
                WordCollection? collection = await dbContext.WordCollections
                    .Include(existing => existing.Entries)
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
}

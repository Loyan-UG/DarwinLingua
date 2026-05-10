using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.SharedKernel.Content;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Seed;

internal sealed class CatalogWordCollectionSeeder(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IDatabaseSeeder
{
    private static readonly CollectionSeedDefinition[] SeedDefinitions =
    [
        new(
            new Guid("A7E59E70-2E9D-4B04-B2E7-5F2D6A110001"),
            "erp-core-b1",
            "ERP Core B1",
            "Essential workplace and ERP vocabulary for learners who need the basic language of tasks, users, records, forms, meetings, and daily system work.",
            "/images/collections/erp-core-b1.svg",
            10,
            ["Aufgabe", "Benutzer", "Datensatz", "Formular", "speichern", "prüfen", "dokumentieren", "Freigabeworkflow"]),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        List<WordCollection> existingCollections = await dbContext.WordCollections
            .Include(collection => collection.Entries)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<WordEntry> activeWords = await dbContext.WordEntries
            .Where(word => word.PublicationStatus == PublicationStatus.Active)
            .OrderBy(word => word.Lemma)
            .ThenBy(word => word.PartOfSpeech)
            .ThenBy(word => word.PrimaryCefrLevel)
            .ThenBy(word => word.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<string, WordEntry> activeWordsByLemma = activeWords
            .GroupBy(word => word.Lemma, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.First(),
                StringComparer.OrdinalIgnoreCase);

        if (activeWordsByLemma.Count == 0)
        {
            return;
        }

        DateTime timestampUtc = DateTime.UtcNow;

        foreach (CollectionSeedDefinition seedDefinition in SeedDefinitions)
        {
            WordCollection? existingCollection = existingCollections
                .SingleOrDefault(collection => collection.Id == seedDefinition.Id) ??
                existingCollections.SingleOrDefault(collection => string.Equals(collection.Slug, seedDefinition.Slug, StringComparison.OrdinalIgnoreCase));

            List<(Guid WordEntryId, int SortOrder)> resolvedWords = seedDefinition.Lemmas
                .Select((lemma, index) => activeWordsByLemma.TryGetValue(lemma, out WordEntry? word)
                    ? (Found: true, WordEntryId: word.Id, SortOrder: index + 1)
                    : (Found: false, WordEntryId: Guid.Empty, SortOrder: 0))
                .Where(item => item.Found)
                .Select(item => (item.WordEntryId, item.SortOrder))
                .ToList();

            if (resolvedWords.Count == 0)
            {
                continue;
            }

            if (existingCollection is not null)
            {
                continue;
            }

            WordCollection newCollection = new(
                seedDefinition.Id,
                seedDefinition.Slug,
                seedDefinition.Name,
                seedDefinition.Description,
                seedDefinition.ImageUrl,
                PublicationStatus.Active,
                seedDefinition.SortOrder,
                timestampUtc);

            newCollection.ReplaceEntries(resolvedWords, timestampUtc);
            dbContext.WordCollections.Add(newCollection);
        }

        if (dbContext.ChangeTracker.HasChanges())
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed record CollectionSeedDefinition(
        Guid Id,
        string Slug,
        string Name,
        string Description,
        string ImageUrl,
        int SortOrder,
        IReadOnlyList<string> Lemmas);
}

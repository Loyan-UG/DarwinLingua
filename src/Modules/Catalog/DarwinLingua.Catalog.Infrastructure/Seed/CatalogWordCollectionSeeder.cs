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
        new(
            new Guid("A7E59E70-2E9D-4B04-B2E7-5F2D6A110002"),
            "crm-sales-playlist",
            "CRM and Sales Playlist",
            "Focused vocabulary for demos, quotations, leads, contract conversations, objections, follow-up calls, and customer-facing discussions.",
            "/images/collections/crm-sales-playlist.svg",
            20,
            ["Angebot", "Kunde", "Vertrieb", "Anfrage", "Lead-Quelle", "Verkaufschance", "Preisverhandlung", "nachfassen", "argumentieren", "überzeugend"]),
        new(
            new Guid("A7E59E70-2E9D-4B04-B2E7-5F2D6A110003"),
            "warehouse-procurement",
            "Warehouse and Procurement",
            "A practical collection for stock, purchasing, supplier communication, goods receipt, transfers, picking, and inventory operations.",
            "/images/collections/warehouse-procurement.svg",
            30,
            ["Lieferant", "Bestellung", "Lieferung", "Lagerbestand", "Beschaffung", "Kommissionierung", "Umlagerung", "Wareneingang", "Bestellanforderung", "nachbestellen"]),
        new(
            new Guid("A7E59E70-2E9D-4B04-B2E7-5F2D6A110004"),
            "project-meetings-b2",
            "Projects and Meetings B2",
            "Useful words for requirement workshops, alignment meetings, status updates, rollout planning, and technical coordination with German-speaking colleagues.",
            "/images/collections/project-meetings-b2.svg",
            40,
            ["Besprechung", "Anforderung", "Schnittstelle", "abstimmen", "priorisieren", "Präsentation", "Projektplan", "Statusbericht", "Risikobewertung", "zusammenfassen"]),
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
                .SingleOrDefault(collection => string.Equals(collection.Slug, seedDefinition.Slug, StringComparison.OrdinalIgnoreCase));

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

            if (existingCollection is null)
            {
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
                continue;
            }

            existingCollection.UpdateMetadata(
                seedDefinition.Name,
                seedDefinition.Description,
                seedDefinition.ImageUrl,
                PublicationStatus.Active,
                seedDefinition.SortOrder,
                timestampUtc);

            existingCollection.ReplaceEntries(resolvedWords, timestampUtc);
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

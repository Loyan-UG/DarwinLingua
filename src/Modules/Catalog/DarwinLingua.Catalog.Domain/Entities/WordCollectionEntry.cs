using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents one word inside a curated collection.
/// </summary>
public sealed class WordCollectionEntry
{
    private WordCollectionEntry()
    {
    }

    internal WordCollectionEntry(
        Guid id,
        Guid wordCollectionId,
        Guid wordEntryId,
        int sortOrder,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Word collection entry identifier cannot be empty.");
        }

        if (wordCollectionId == Guid.Empty)
        {
            throw new DomainRuleException("Word collection identifier cannot be empty.");
        }

        if (wordEntryId == Guid.Empty)
        {
            throw new DomainRuleException("Word entry identifier cannot be empty.");
        }

        if (sortOrder < 0)
        {
            throw new DomainRuleException("Word collection entry sort order cannot be negative.");
        }

        Id = id;
        WordCollectionId = wordCollectionId;
        WordEntryId = wordEntryId;
        SortOrder = sortOrder;
        CreatedAtUtc = createdAtUtc.Kind == DateTimeKind.Utc ? createdAtUtc : createdAtUtc.ToUniversalTime();
    }

    public Guid Id { get; private set; }

    public Guid WordCollectionId { get; private set; }

    public Guid WordEntryId { get; private set; }

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public WordEntry? WordEntry { get; private set; }
}

using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents a curated collection of words such as a book list, study playlist, or topical pack.
/// </summary>
public sealed partial class WordCollection
{
    private readonly List<WordCollectionEntry> _entries = [];

    private WordCollection()
    {
    }

    public WordCollection(
        Guid id,
        string slug,
        string name,
        string? description,
        string? imageUrl,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Word collection identifier cannot be empty.");
        }

        Id = id;
        Slug = NormalizeSlug(slug);
        Name = NormalizeRequiredText(name, nameof(name));
        Description = NormalizeOptionalText(description);
        ImageUrl = NormalizeOptionalText(imageUrl);
        PublicationStatus = publicationStatus;
        SortOrder = NormalizeSortOrder(sortOrder);
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string Slug { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? ImageUrl { get; private set; }

    public PublicationStatus PublicationStatus { get; private set; }

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<WordCollectionEntry> Entries => _entries.AsReadOnly();

    public void UpdateMetadata(
        string name,
        string? description,
        string? imageUrl,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime updatedAtUtc)
    {
        Name = NormalizeRequiredText(name, nameof(name));
        Description = NormalizeOptionalText(description);
        ImageUrl = NormalizeOptionalText(imageUrl);
        PublicationStatus = publicationStatus;
        SortOrder = NormalizeSortOrder(sortOrder);
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    public void AddWord(
        Guid entryId,
        Guid wordEntryId,
        int sortOrder,
        DateTime createdAtUtc)
    {
        if (entryId == Guid.Empty)
        {
            throw new DomainRuleException("Word collection entry identifier cannot be empty.");
        }

        if (wordEntryId == Guid.Empty)
        {
            throw new DomainRuleException("Word collection entry word identifier cannot be empty.");
        }

        if (_entries.Any(existingEntry => existingEntry.WordEntryId == wordEntryId))
        {
            throw new DomainRuleException("Duplicate words are not allowed inside the same word collection.");
        }

        if (_entries.Any(existingEntry => existingEntry.SortOrder == sortOrder))
        {
            throw new DomainRuleException("Duplicate collection sort orders are not allowed.");
        }

        WordCollectionEntry entry = new(
            entryId,
            Id,
            wordEntryId,
            sortOrder,
            createdAtUtc);

        _entries.Add(entry);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public void ReplaceEntries(IEnumerable<(Guid WordEntryId, int SortOrder)> words, DateTime updatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(words);

        _entries.Clear();
        DateTime normalizedUpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));

        foreach ((Guid wordEntryId, int sortOrder) in words.OrderBy(item => item.SortOrder))
        {
            AddWord(Guid.NewGuid(), wordEntryId, sortOrder, normalizedUpdatedAtUtc);
        }

        UpdatedAtUtc = normalizedUpdatedAtUtc;
    }

    private static string NormalizeRequiredText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException($"{parameterName} cannot be empty.");
        }

        return value.Trim();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string NormalizeSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("Word collection slug cannot be empty.");
        }

        string normalized = value.Trim().ToLowerInvariant();
        if (!SlugPattern().IsMatch(normalized))
        {
            throw new DomainRuleException("Word collection slug must use lowercase kebab-case characters only.");
        }

        return normalized;
    }

    private static int NormalizeSortOrder(int value)
    {
        if (value < 0)
        {
            throw new DomainRuleException("Word collection sort order cannot be negative.");
        }

        return value;
    }

    private static DateTime NormalizeUtc(DateTime value, string parameterName)
    {
        if (value == default)
        {
            throw new DomainRuleException($"{parameterName} cannot be empty.");
        }

        return value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled)]
    private static partial Regex SlugPattern();
}

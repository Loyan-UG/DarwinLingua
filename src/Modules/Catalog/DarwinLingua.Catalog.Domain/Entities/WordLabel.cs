using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents a normalized lexical label attached to a word entry.
/// </summary>
public sealed partial class WordLabel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WordLabel"/> class for EF Core materialization.
    /// </summary>
    private WordLabel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WordLabel"/> class.
    /// </summary>
    internal WordLabel(
        Guid id,
        Guid wordEntryId,
        WordLabelKind kind,
        string key,
        int sortOrder,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Word label identifier cannot be empty.");
        }

        if (wordEntryId == Guid.Empty)
        {
            throw new DomainRuleException("Word entry identifier cannot be empty for a label.");
        }

        if (sortOrder <= 0)
        {
            throw new DomainRuleException("Word label sort order must be greater than zero.");
        }

        Id = id;
        WordEntryId = wordEntryId;
        Kind = kind;
        Key = NormalizeKey(key);
        SortOrder = sortOrder;
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    /// <summary>
    /// Gets the internal identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the owning word-entry identifier.
    /// </summary>
    public Guid WordEntryId { get; private set; }

    /// <summary>
    /// Gets the label kind.
    /// </summary>
    public WordLabelKind Kind { get; private set; }

    /// <summary>
    /// Gets the normalized label key.
    /// </summary>
    public string Key { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the stable display ordering within the same label kind.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Gets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    private static string NormalizeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("Word label key cannot be empty.");
        }

        string normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > 64)
        {
            throw new DomainRuleException("Word label key cannot be longer than 64 characters.");
        }

        if (!WordLabelKeyPattern().IsMatch(normalized))
        {
            throw new DomainRuleException("Word label key must use lowercase kebab-case characters only.");
        }

        return normalized;
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
    private static partial Regex WordLabelKeyPattern();
}

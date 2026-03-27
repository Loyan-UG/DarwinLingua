using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents a learner-facing collocation attached to a word entry.
/// </summary>
public sealed class WordCollocation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WordCollocation"/> class for EF Core materialization.
    /// </summary>
    private WordCollocation()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WordCollocation"/> class.
    /// </summary>
    internal WordCollocation(
        Guid id,
        Guid wordEntryId,
        string text,
        string? meaning,
        int sortOrder,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Word collocation identifier cannot be empty.");
        }

        if (wordEntryId == Guid.Empty)
        {
            throw new DomainRuleException("Word entry identifier cannot be empty for a collocation.");
        }

        if (sortOrder <= 0)
        {
            throw new DomainRuleException("Word collocation sort order must be greater than zero.");
        }

        Id = id;
        WordEntryId = wordEntryId;
        Text = NormalizeRequiredText(text, "Word collocation text cannot be empty.", 256);
        Meaning = NormalizeOptionalText(meaning, 256);
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
    /// Gets the learner-facing collocation text.
    /// </summary>
    public string Text { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the optional collocation meaning hint.
    /// </summary>
    public string? Meaning { get; private set; }

    /// <summary>
    /// Gets the stable display ordering.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Gets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    private static string NormalizeRequiredText(string value, string errorMessage, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException(errorMessage);
        }

        string normalized = string.Join(" ", value.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        if (normalized.Length > maxLength)
        {
            throw new DomainRuleException($"Word collocation text cannot be longer than {maxLength} characters.");
        }

        return normalized;
    }

    private static string? NormalizeOptionalText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string normalized = string.Join(" ", value.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        if (normalized.Length > maxLength)
        {
            throw new DomainRuleException($"Word collocation meaning cannot be longer than {maxLength} characters.");
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
}

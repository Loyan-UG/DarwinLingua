using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents a learner-facing grammar note attached to a word entry.
/// </summary>
public sealed class WordGrammarNote
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WordGrammarNote"/> class for EF Core materialization.
    /// </summary>
    private WordGrammarNote()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WordGrammarNote"/> class.
    /// </summary>
    internal WordGrammarNote(
        Guid id,
        Guid wordEntryId,
        string text,
        int sortOrder,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Word grammar-note identifier cannot be empty.");
        }

        if (wordEntryId == Guid.Empty)
        {
            throw new DomainRuleException("Word entry identifier cannot be empty for a grammar note.");
        }

        if (sortOrder <= 0)
        {
            throw new DomainRuleException("Word grammar-note sort order must be greater than zero.");
        }

        Id = id;
        WordEntryId = wordEntryId;
        Text = NormalizeText(text);
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
    /// Gets the learner-facing grammar note text.
    /// </summary>
    public string Text { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the stable display ordering.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Gets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    private static string NormalizeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("Word grammar-note text cannot be empty.");
        }

        string normalized = value.Trim();

        if (normalized.Length > 512)
        {
            throw new DomainRuleException("Word grammar-note text cannot be longer than 512 characters.");
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

using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents a learner-facing word-family member attached to a word entry.
/// </summary>
public sealed class WordFamilyMember
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WordFamilyMember"/> class for EF Core materialization.
    /// </summary>
    private WordFamilyMember()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WordFamilyMember"/> class.
    /// </summary>
    internal WordFamilyMember(
        Guid id,
        Guid wordEntryId,
        string lemma,
        string relationLabel,
        string? note,
        int sortOrder,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Word family-member identifier cannot be empty.");
        }

        if (wordEntryId == Guid.Empty)
        {
            throw new DomainRuleException("Word entry identifier cannot be empty for a family member.");
        }

        if (sortOrder <= 0)
        {
            throw new DomainRuleException("Word family-member sort order must be greater than zero.");
        }

        Id = id;
        WordEntryId = wordEntryId;
        Lemma = NormalizeRequiredText(lemma, "Word family-member lemma cannot be empty.", 128);
        RelationLabel = NormalizeRequiredText(relationLabel, "Word family-member relation label cannot be empty.", 64);
        Note = NormalizeOptionalText(note, 256);
        SortOrder = sortOrder;
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid WordEntryId { get; private set; }

    public string Lemma { get; private set; } = string.Empty;

    public string RelationLabel { get; private set; } = string.Empty;

    public string? Note { get; private set; }

    public int SortOrder { get; private set; }

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
            throw new DomainRuleException($"Word family-member text cannot be longer than {maxLength} characters.");
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
            throw new DomainRuleException($"Word family-member note cannot be longer than {maxLength} characters.");
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

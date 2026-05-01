using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed partial class LabelDefinition
{
    private LabelDefinition()
    {
    }

    public LabelDefinition(
        Guid id,
        WordLabelKind kind,
        string key,
        string displayName,
        int sortOrder,
        bool isSystem,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Label definition identifier cannot be empty.");
        }

        Id = id;
        Kind = kind;
        Key = NormalizeKey(key);
        DisplayName = NormalizeDisplayName(displayName);
        SortOrder = NormalizeSortOrder(sortOrder);
        IsSystem = isSystem;
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public WordLabelKind Kind { get; private set; }

    public string Key { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public bool IsSystem { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public void UpdateMetadata(string displayName, int sortOrder, bool isSystem, DateTime updatedAtUtc)
    {
        DisplayName = NormalizeDisplayName(displayName);
        SortOrder = NormalizeSortOrder(sortOrder);
        IsSystem = isSystem;
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    public void Rename(WordLabelKind kind, string key, string displayName, int sortOrder, bool isSystem, DateTime updatedAtUtc)
    {
        Kind = kind;
        Key = NormalizeKey(key);
        UpdateMetadata(displayName, sortOrder, isSystem, updatedAtUtc);
    }

    private static string NormalizeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("Label key cannot be empty.");
        }

        string normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > 64)
        {
            throw new DomainRuleException("Label key cannot exceed 64 characters.");
        }

        if (!LabelKeyPattern().IsMatch(normalized))
        {
            throw new DomainRuleException("Label key must use lowercase kebab-case characters only.");
        }

        return normalized;
    }

    private static string NormalizeDisplayName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("Label display name cannot be empty.");
        }

        string normalized = value.Trim();
        if (normalized.Length > 128)
        {
            throw new DomainRuleException("Label display name cannot exceed 128 characters.");
        }

        return normalized;
    }

    private static int NormalizeSortOrder(int value)
    {
        if (value < 0)
        {
            throw new DomainRuleException("Label sort order cannot be negative.");
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
    private static partial Regex LabelKeyPattern();
}

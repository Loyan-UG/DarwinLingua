using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents a stable topic used for browsing and filtering vocabulary content.
/// </summary>
public sealed partial class Topic
{
    private readonly List<TopicLocalization> _localizations = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Topic"/> class for EF Core materialization.
    /// </summary>
    private Topic()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Topic"/> class.
    /// </summary>
    public Topic(
        Guid id,
        string key,
        int sortOrder,
        bool isSystem,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Topic identifier cannot be empty.");
        }

        Id = id;
        Key = NormalizeKey(key);
        SortOrder = NormalizeSortOrder(sortOrder);
        IsSystem = isSystem;
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string Key { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public bool IsSystem { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<TopicLocalization> Localizations => _localizations.AsReadOnly();

    /// <summary>
    /// Adds or updates a localized display name for the topic.
    /// </summary>
    public void AddOrUpdateLocalization(
        Guid localizationId,
        LanguageCode languageCode,
        string displayName,
        DateTime timestampUtc)
    {
        DateTime normalizedTimestamp = NormalizeUtc(timestampUtc, nameof(timestampUtc));

        TopicLocalization? existingLocalization = _localizations
            .SingleOrDefault(localization => localization.LanguageCode == languageCode);

        if (existingLocalization is null)
        {
            _localizations.Add(new TopicLocalization(
                localizationId,
                Id,
                languageCode,
                displayName,
                normalizedTimestamp,
                normalizedTimestamp));
        }
        else
        {
            if (localizationId != Guid.Empty && existingLocalization.Id != localizationId)
            {
                throw new DomainRuleException("Topic localization identifier does not match the existing language row.");
            }

            existingLocalization.UpdateDisplayName(displayName, normalizedTimestamp);
        }

        UpdatedAtUtc = normalizedTimestamp;
    }

    /// <summary>
    /// Returns the localized display name for the requested language when available.
    /// </summary>
    public TopicLocalization? FindLocalization(LanguageCode languageCode)
    {
        return _localizations.SingleOrDefault(localization => localization.LanguageCode == languageCode);
    }

    /// <summary>
    /// Updates the editable topic metadata.
    /// </summary>
    public void UpdateMetadata(string key, int sortOrder, bool isSystem, DateTime updatedAtUtc)
    {
        Key = NormalizeKey(key);
        SortOrder = NormalizeSortOrder(sortOrder);
        IsSystem = isSystem;
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    /// <summary>
     /// Updates the sort order of the topic.
     /// </summary>
    public void UpdateSortOrder(int sortOrder, DateTime updatedAtUtc)
    {
        SortOrder = NormalizeSortOrder(sortOrder);
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    private static string NormalizeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("Topic key cannot be empty.");
        }

        string normalized = value.Trim().ToLowerInvariant();

        if (!TopicKeyPattern().IsMatch(normalized))
        {
            throw new DomainRuleException("Topic key must use lowercase kebab-case characters only.");
        }

        return normalized;
    }

    private static int NormalizeSortOrder(int value)
    {
        if (value < 0)
        {
            throw new DomainRuleException("Topic sort order cannot be negative.");
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
    private static partial Regex TopicKeyPattern();
}

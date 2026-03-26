using DarwinLingua.ContentOps.Domain.Enums;
using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.ContentOps.Domain.Entities;

/// <summary>
/// Represents a single attempted entry inside a content package import.
/// </summary>
public sealed class ContentPackageEntry
{
    private ContentPackageEntry()
    {
    }

    internal ContentPackageEntry(
        Guid id,
        Guid contentPackageId,
        string rawLemma,
        string normalizedLemma,
        string? cefrLevel,
        string? partOfSpeech,
        ContentPackageEntryStatus processingStatus,
        string? errorMessage,
        string? warningMessage,
        Guid? importedWordEntryPublicId,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Content package entry identifier cannot be empty.");
        }

        if (contentPackageId == Guid.Empty)
        {
            throw new DomainRuleException("Content package identifier cannot be empty for a package entry.");
        }

        Id = id;
        ContentPackageId = contentPackageId;
        RawLemma = NormalizeRequiredText(rawLemma, nameof(rawLemma));
        NormalizedLemma = NormalizeRequiredText(normalizedLemma, nameof(normalizedLemma));
        CefrLevel = NormalizeOptionalText(cefrLevel);
        PartOfSpeech = NormalizeOptionalText(partOfSpeech);
        ProcessingStatus = processingStatus;
        ErrorMessage = NormalizeOptionalText(errorMessage);
        WarningMessage = NormalizeOptionalText(warningMessage);
        ImportedWordEntryPublicId = importedWordEntryPublicId;
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid ContentPackageId { get; private set; }

    public string RawLemma { get; private set; } = string.Empty;

    public string NormalizedLemma { get; private set; } = string.Empty;

    public string? CefrLevel { get; private set; }

    public string? PartOfSpeech { get; private set; }

    public ContentPackageEntryStatus ProcessingStatus { get; private set; }

    public string? ErrorMessage { get; private set; }

    public string? WarningMessage { get; private set; }

    public Guid? ImportedWordEntryPublicId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

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

    private static DateTime NormalizeUtc(DateTime value, string parameterName)
    {
        if (value == default)
        {
            throw new DomainRuleException($"{parameterName} cannot be empty.");
        }

        return value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }
}

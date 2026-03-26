using DarwinLingua.ContentOps.Domain.Enums;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.ContentOps.Domain.Entities;

/// <summary>
/// Represents a single imported content package and its processing summary.
/// </summary>
public sealed class ContentPackage
{
    private readonly List<ContentPackageEntry> _entries = [];

    private ContentPackage()
    {
    }

    public ContentPackage(
        Guid id,
        string packageId,
        string packageVersion,
        string packageName,
        ContentSourceType sourceType,
        string inputFileName,
        int totalEntries,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Content package identifier cannot be empty.");
        }

        if (totalEntries < 0)
        {
            throw new DomainRuleException("Content package total entries cannot be negative.");
        }

        Id = id;
        PackageId = NormalizeRequiredText(packageId, nameof(packageId));
        PackageVersion = NormalizeRequiredText(packageVersion, nameof(packageVersion));
        PackageName = NormalizeRequiredText(packageName, nameof(packageName));
        SourceType = sourceType;
        InputFileName = NormalizeRequiredText(inputFileName, nameof(inputFileName));
        TotalEntries = totalEntries;
        Status = ContentPackageStatus.Pending;
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string PackageId { get; private set; } = string.Empty;

    public string PackageVersion { get; private set; } = string.Empty;

    public string PackageName { get; private set; } = string.Empty;

    public ContentSourceType SourceType { get; private set; }

    public string InputFileName { get; private set; } = string.Empty;

    public int TotalEntries { get; private set; }

    public int InsertedEntries { get; private set; }

    public int SkippedDuplicateEntries { get; private set; }

    public int InvalidEntries { get; private set; }

    public int WarningCount { get; private set; }

    public ContentPackageStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<ContentPackageEntry> Entries => _entries.AsReadOnly();

    public void MarkProcessing(DateTime updatedAtUtc)
    {
        Status = ContentPackageStatus.Processing;
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    public ContentPackageEntry AddEntry(
        Guid id,
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
        ContentPackageEntry entry = new(
            id,
            Id,
            rawLemma,
            normalizedLemma,
            cefrLevel,
            partOfSpeech,
            processingStatus,
            errorMessage,
            warningMessage,
            importedWordEntryPublicId,
            createdAtUtc);

        _entries.Add(entry);

        if (processingStatus == ContentPackageEntryStatus.Imported)
        {
            InsertedEntries++;
        }

        if (processingStatus == ContentPackageEntryStatus.SkippedDuplicate)
        {
            SkippedDuplicateEntries++;
        }

        if (processingStatus is ContentPackageEntryStatus.Invalid or ContentPackageEntryStatus.Failed)
        {
            InvalidEntries++;
        }

        if (!string.IsNullOrWhiteSpace(warningMessage))
        {
            WarningCount++;
        }

        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));

        return entry;
    }

    public void Complete(DateTime updatedAtUtc)
    {
        Status = WarningCount > 0 || InvalidEntries > 0 || SkippedDuplicateEntries > 0
            ? ContentPackageStatus.CompletedWithWarnings
            : ContentPackageStatus.Completed;
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    public void Fail(DateTime updatedAtUtc)
    {
        Status = ContentPackageStatus.Failed;
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    private static string NormalizeRequiredText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException($"{parameterName} cannot be empty.");
        }

        return value.Trim();
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

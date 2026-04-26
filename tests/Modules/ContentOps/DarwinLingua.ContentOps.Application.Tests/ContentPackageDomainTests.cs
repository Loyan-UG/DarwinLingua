using DarwinLingua.ContentOps.Domain.Entities;
using DarwinLingua.ContentOps.Domain.Enums;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.ContentOps.Application.Tests;

/// <summary>
/// Verifies the <see cref="ContentPackage"/> aggregate behavior.
/// </summary>
public sealed class ContentPackageDomainTests
{
    /// <summary>
    /// Verifies that a valid content package is created with expected initial property values.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreatePackageWithExpectedProperties()
    {
        Guid id = Guid.NewGuid();
        DateTime createdAt = DateTime.UtcNow;

        ContentPackage package = new(
            id,
            "darwin-deutsch-v1",
            "1.0",
            "Darwin Deutsch",
            ContentSourceType.Manual,
            "darwin-deutsch-v1.json",
            50,
            createdAt);

        Assert.Equal(id, package.Id);
        Assert.Equal("darwin-deutsch-v1", package.PackageId);
        Assert.Equal("1.0", package.PackageVersion);
        Assert.Equal("Darwin Deutsch", package.PackageName);
        Assert.Equal(ContentPackageStatus.Pending, package.Status);
        Assert.Equal(50, package.TotalEntries);
        Assert.Equal(0, package.InsertedEntries);
        Assert.Equal(0, package.SkippedDuplicateEntries);
        Assert.Equal(0, package.InvalidEntries);
        Assert.Equal(0, package.WarningCount);
        Assert.Empty(package.Entries);
    }

    /// <summary>
    /// Verifies that an empty identifier is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyIdentifier()
    {
        Assert.Throws<DomainRuleException>(() => new ContentPackage(
            Guid.Empty,
            "my-package",
            "1.0",
            "My Package",
            ContentSourceType.Manual,
            "input.json",
            10,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that a negative total entry count is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectNegativeTotalEntries()
    {
        Assert.Throws<DomainRuleException>(() => new ContentPackage(
            Guid.NewGuid(),
            "my-package",
            "1.0",
            "My Package",
            ContentSourceType.Manual,
            "input.json",
            -1,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that a blank package identifier is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyPackageId()
    {
        Assert.Throws<DomainRuleException>(() => new ContentPackage(
            Guid.NewGuid(),
            "   ",
            "1.0",
            "My Package",
            ContentSourceType.Manual,
            "input.json",
            10,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that a blank package version is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyPackageVersion()
    {
        Assert.Throws<DomainRuleException>(() => new ContentPackage(
            Guid.NewGuid(),
            "my-package",
            "   ",
            "My Package",
            ContentSourceType.Manual,
            "input.json",
            10,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that a blank package name is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyPackageName()
    {
        Assert.Throws<DomainRuleException>(() => new ContentPackage(
            Guid.NewGuid(),
            "my-package",
            "1.0",
            "   ",
            ContentSourceType.Manual,
            "input.json",
            10,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that a default (uninitialized) creation timestamp is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectDefaultCreatedAtUtc()
    {
        Assert.Throws<DomainRuleException>(() => new ContentPackage(
            Guid.NewGuid(),
            "my-package",
            "1.0",
            "My Package",
            ContentSourceType.Manual,
            "input.json",
            10,
            default));
    }

    /// <summary>
    /// Verifies that <see cref="ContentPackage.MarkProcessing"/> sets the status to Processing.
    /// </summary>
    [Fact]
    public void MarkProcessing_ShouldSetStatusToProcessingAndUpdateTimestamp()
    {
        ContentPackage package = CreatePackage();
        DateTime updatedAt = DateTime.UtcNow.AddSeconds(1);

        package.MarkProcessing(updatedAt);

        Assert.Equal(ContentPackageStatus.Processing, package.Status);
        Assert.Equal(updatedAt, package.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that adding an imported entry increments the inserted count.
    /// </summary>
    [Fact]
    public void AddEntry_ShouldIncrementInsertedEntriesForImportedStatus()
    {
        ContentPackage package = CreatePackage();

        package.AddEntry(
            Guid.NewGuid(),
            "Brot",
            "brot",
            "A1",
            "Noun",
            ContentPackageEntryStatus.Imported,
            null,
            null,
            Guid.NewGuid(),
            DateTime.UtcNow);

        Assert.Equal(1, package.InsertedEntries);
        Assert.Equal(0, package.SkippedDuplicateEntries);
        Assert.Equal(0, package.InvalidEntries);
        Assert.Single(package.Entries);
    }

    /// <summary>
    /// Verifies that adding a skipped-duplicate entry increments the skipped count.
    /// </summary>
    [Fact]
    public void AddEntry_ShouldIncrementSkippedDuplicateEntriesForSkippedDuplicateStatus()
    {
        ContentPackage package = CreatePackage();

        package.AddEntry(
            Guid.NewGuid(),
            "Brot",
            "brot",
            "A1",
            "Noun",
            ContentPackageEntryStatus.SkippedDuplicate,
            null,
            null,
            null,
            DateTime.UtcNow);

        Assert.Equal(0, package.InsertedEntries);
        Assert.Equal(1, package.SkippedDuplicateEntries);
        Assert.Equal(0, package.InvalidEntries);
    }

    /// <summary>
    /// Verifies that adding an invalid entry increments the invalid count.
    /// </summary>
    [Fact]
    public void AddEntry_ShouldIncrementInvalidEntriesForInvalidStatus()
    {
        ContentPackage package = CreatePackage();

        package.AddEntry(
            Guid.NewGuid(),
            "bad-word",
            "bad-word",
            null,
            null,
            ContentPackageEntryStatus.Invalid,
            "Missing required fields.",
            null,
            null,
            DateTime.UtcNow);

        Assert.Equal(0, package.InsertedEntries);
        Assert.Equal(0, package.SkippedDuplicateEntries);
        Assert.Equal(1, package.InvalidEntries);
    }

    /// <summary>
    /// Verifies that adding a failed entry also increments the invalid count.
    /// </summary>
    [Fact]
    public void AddEntry_ShouldIncrementInvalidEntriesForFailedStatus()
    {
        ContentPackage package = CreatePackage();

        package.AddEntry(
            Guid.NewGuid(),
            "crash-word",
            "crash-word",
            null,
            null,
            ContentPackageEntryStatus.Failed,
            "Unexpected error occurred.",
            null,
            null,
            DateTime.UtcNow);

        Assert.Equal(1, package.InvalidEntries);
    }

    /// <summary>
    /// Verifies that adding an entry with a warning message increments the warning count.
    /// </summary>
    [Fact]
    public void AddEntry_ShouldIncrementWarningCountWhenWarningMessageIsPresent()
    {
        ContentPackage package = CreatePackage();

        package.AddEntry(
            Guid.NewGuid(),
            "Bahnhof",
            "bahnhof",
            "A1",
            "Noun",
            ContentPackageEntryStatus.Imported,
            null,
            "Translation missing for one language.",
            Guid.NewGuid(),
            DateTime.UtcNow);

        Assert.Equal(1, package.WarningCount);
        Assert.Equal(1, package.InsertedEntries);
    }

    /// <summary>
    /// Verifies that <see cref="ContentPackage.Complete"/> sets Completed status when no issues were recorded.
    /// </summary>
    [Fact]
    public void Complete_ShouldSetCompletedStatusWhenNoIssues()
    {
        ContentPackage package = CreatePackage();
        package.AddEntry(
            Guid.NewGuid(),
            "Brot",
            "brot",
            "A1",
            "Noun",
            ContentPackageEntryStatus.Imported,
            null,
            null,
            Guid.NewGuid(),
            DateTime.UtcNow);

        DateTime completedAt = DateTime.UtcNow.AddSeconds(5);
        package.Complete(completedAt);

        Assert.Equal(ContentPackageStatus.Completed, package.Status);
        Assert.Equal(completedAt, package.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that <see cref="ContentPackage.Complete"/> sets CompletedWithWarnings when warnings exist.
    /// </summary>
    [Fact]
    public void Complete_ShouldSetCompletedWithWarningsWhenWarningsPresent()
    {
        ContentPackage package = CreatePackage();
        package.AddEntry(
            Guid.NewGuid(),
            "Brot",
            "brot",
            "A1",
            "Noun",
            ContentPackageEntryStatus.Imported,
            null,
            "Missing secondary translation.",
            Guid.NewGuid(),
            DateTime.UtcNow);

        package.Complete(DateTime.UtcNow.AddSeconds(5));

        Assert.Equal(ContentPackageStatus.CompletedWithWarnings, package.Status);
    }

    /// <summary>
    /// Verifies that <see cref="ContentPackage.Complete"/> sets CompletedWithWarnings when invalid entries exist.
    /// </summary>
    [Fact]
    public void Complete_ShouldSetCompletedWithWarningsWhenInvalidEntriesPresent()
    {
        ContentPackage package = CreatePackage();
        package.AddEntry(
            Guid.NewGuid(),
            "broken",
            "broken",
            null,
            null,
            ContentPackageEntryStatus.Invalid,
            "Invalid entry.",
            null,
            null,
            DateTime.UtcNow);

        package.Complete(DateTime.UtcNow.AddSeconds(5));

        Assert.Equal(ContentPackageStatus.CompletedWithWarnings, package.Status);
    }

    /// <summary>
    /// Verifies that <see cref="ContentPackage.Complete"/> sets CompletedWithWarnings when skipped duplicates exist.
    /// </summary>
    [Fact]
    public void Complete_ShouldSetCompletedWithWarningsWhenSkippedDuplicatesPresent()
    {
        ContentPackage package = CreatePackage();
        package.AddEntry(
            Guid.NewGuid(),
            "Brot",
            "brot",
            "A1",
            "Noun",
            ContentPackageEntryStatus.SkippedDuplicate,
            null,
            null,
            null,
            DateTime.UtcNow);

        package.Complete(DateTime.UtcNow.AddSeconds(5));

        Assert.Equal(ContentPackageStatus.CompletedWithWarnings, package.Status);
    }

    /// <summary>
    /// Verifies that <see cref="ContentPackage.Fail"/> sets the status to Failed.
    /// </summary>
    [Fact]
    public void Fail_ShouldSetStatusToFailed()
    {
        ContentPackage package = CreatePackage();
        DateTime failedAt = DateTime.UtcNow.AddSeconds(2);

        package.Fail(failedAt);

        Assert.Equal(ContentPackageStatus.Failed, package.Status);
        Assert.Equal(failedAt, package.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that multiple entries are tracked independently.
    /// </summary>
    [Fact]
    public void AddEntry_ShouldTrackMultipleEntriesCumulatively()
    {
        ContentPackage package = CreatePackage();

        package.AddEntry(Guid.NewGuid(), "Brot", "brot", "A1", "Noun", ContentPackageEntryStatus.Imported, null, null, Guid.NewGuid(), DateTime.UtcNow);
        package.AddEntry(Guid.NewGuid(), "Brot", "brot", "A1", "Noun", ContentPackageEntryStatus.SkippedDuplicate, null, null, null, DateTime.UtcNow);
        package.AddEntry(Guid.NewGuid(), "bad", "bad", null, null, ContentPackageEntryStatus.Invalid, "Bad entry.", null, null, DateTime.UtcNow);

        Assert.Equal(3, package.Entries.Count);
        Assert.Equal(1, package.InsertedEntries);
        Assert.Equal(1, package.SkippedDuplicateEntries);
        Assert.Equal(1, package.InvalidEntries);
    }

    /// <summary>
    /// Verifies that a blank input file name is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyInputFileName()
    {
        Assert.Throws<DomainRuleException>(() => new ContentPackage(
            Guid.NewGuid(),
            "my-package",
            "1.0",
            "My Package",
            ContentSourceType.Manual,
            "   ",
            10,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that a local (non-UTC) creation timestamp is converted to UTC.
    /// </summary>
    [Fact]
    public void Constructor_ShouldConvertLocalCreatedAtToUtc()
    {
        DateTime localTime = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Local);

        ContentPackage package = new(
            Guid.NewGuid(),
            "my-package",
            "1.0",
            "My Package",
            ContentSourceType.Manual,
            "input.json",
            5,
            localTime);

        Assert.Equal(DateTimeKind.Utc, package.CreatedAtUtc.Kind);
    }

    /// <summary>
    /// Verifies that adding a failed entry with a warning message increments both
    /// <see cref="ContentPackage.InvalidEntries"/> and <see cref="ContentPackage.WarningCount"/>.
    /// </summary>
    [Fact]
    public void AddEntry_FailedStatusWithWarning_ShouldIncrementBothInvalidAndWarningCounts()
    {
        ContentPackage package = CreatePackage();

        package.AddEntry(
            Guid.NewGuid(),
            "crash-word",
            "crash-word",
            null,
            null,
            ContentPackageEntryStatus.Failed,
            "Unexpected error occurred.",
            "Partial data written.",
            null,
            DateTime.UtcNow);

        Assert.Equal(1, package.InvalidEntries);
        Assert.Equal(1, package.WarningCount);
    }

    /// <summary>
    /// Verifies that adding an entry updates the <see cref="ContentPackage.UpdatedAtUtc"/> timestamp.
    /// </summary>
    [Fact]
    public void AddEntry_ShouldUpdateUpdatedAtUtcTimestamp()
    {
        ContentPackage package = CreatePackage();
        DateTime entryCreatedAt = DateTime.UtcNow.AddSeconds(2);

        package.AddEntry(
            Guid.NewGuid(),
            "Brot",
            "brot",
            "A1",
            "Noun",
            ContentPackageEntryStatus.Imported,
            null,
            null,
            Guid.NewGuid(),
            entryCreatedAt);

        Assert.Equal(entryCreatedAt, package.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that zero total entries is accepted (empty package).
    /// </summary>
    [Fact]
    public void Constructor_ShouldAcceptZeroTotalEntries()
    {
        ContentPackage package = new(
            Guid.NewGuid(),
            "empty-package",
            "1.0",
            "Empty Package",
            ContentSourceType.Manual,
            "empty.json",
            0,
            DateTime.UtcNow);

        Assert.Equal(0, package.TotalEntries);
        Assert.Equal(ContentPackageStatus.Pending, package.Status);
    }

    private static ContentPackage CreatePackage()
    {
        return new ContentPackage(
            Guid.NewGuid(),
            "test-package-v1",
            "1.0",
            "Test Package",
            ContentSourceType.Manual,
            "test-package-v1.json",
            10,
            DateTime.UtcNow);
    }
}

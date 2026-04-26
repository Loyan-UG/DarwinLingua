using DarwinLingua.Practice.Domain.Entities;
using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Practice.Domain.Tests;

/// <summary>
/// Verifies the <see cref="PracticeAttempt"/> entity invariants.
/// </summary>
public sealed class PracticeAttemptTests
{
    /// <summary>
    /// Verifies that a valid attempt is created with the expected property values.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateAttemptWithExpectedProperties()
    {
        Guid id = Guid.NewGuid();
        Guid wordEntryPublicId = Guid.NewGuid();
        DateTime attemptedAt = DateTime.UtcNow;
        DateTime dueAtBefore = DateTime.UtcNow.AddDays(-1);
        DateTime dueAtAfter = DateTime.UtcNow.AddDays(3);

        PracticeAttempt attempt = new(
            id,
            "local-installation-user",
            wordEntryPublicId,
            PracticeSessionType.Flashcard,
            PracticeAttemptOutcome.Correct,
            attemptedAt,
            dueAtBefore,
            dueAtAfter,
            responseMilliseconds: 850);

        Assert.Equal(id, attempt.Id);
        Assert.Equal("local-installation-user", attempt.UserId);
        Assert.Equal(wordEntryPublicId, attempt.WordEntryPublicId);
        Assert.Equal(PracticeSessionType.Flashcard, attempt.SessionType);
        Assert.Equal(PracticeAttemptOutcome.Correct, attempt.Outcome);
        Assert.Equal(attemptedAt, attempt.AttemptedAtUtc);
        Assert.Equal(dueAtBefore, attempt.DueAtUtcBeforeAttempt);
        Assert.Equal(dueAtAfter, attempt.DueAtUtcAfterAttempt);
        Assert.Equal(850, attempt.ResponseMilliseconds);
        Assert.Equal(attemptedAt, attempt.CreatedAtUtc);
    }

    /// <summary>
    /// Verifies that an empty internal identifier is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyIdentifier()
    {
        Assert.Throws<DomainRuleException>(() => new PracticeAttempt(
            Guid.Empty,
            "local-installation-user",
            Guid.NewGuid(),
            PracticeSessionType.Quiz,
            PracticeAttemptOutcome.Correct,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that an empty lexical-entry identifier is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyWordEntryPublicId()
    {
        Assert.Throws<DomainRuleException>(() => new PracticeAttempt(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.Empty,
            PracticeSessionType.Quiz,
            PracticeAttemptOutcome.Correct,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that a blank user identifier is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectBlankUserId()
    {
        Assert.Throws<DomainRuleException>(() => new PracticeAttempt(
            Guid.NewGuid(),
            "   ",
            Guid.NewGuid(),
            PracticeSessionType.Quiz,
            PracticeAttemptOutcome.Correct,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that the user identifier is trimmed on construction.
    /// </summary>
    [Fact]
    public void Constructor_ShouldTrimUserId()
    {
        PracticeAttempt attempt = new(
            Guid.NewGuid(),
            "  local-installation-user  ",
            Guid.NewGuid(),
            PracticeSessionType.Flashcard,
            PracticeAttemptOutcome.Incorrect,
            DateTime.UtcNow);

        Assert.Equal("local-installation-user", attempt.UserId);
    }

    /// <summary>
    /// Verifies that a default (uninitialized) attempt timestamp is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectDefaultAttemptedAtUtc()
    {
        Assert.Throws<DomainRuleException>(() => new PracticeAttempt(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.NewGuid(),
            PracticeSessionType.Flashcard,
            PracticeAttemptOutcome.Correct,
            default));
    }

    /// <summary>
    /// Verifies that a zero response time is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectZeroResponseMilliseconds()
    {
        Assert.Throws<DomainRuleException>(() => new PracticeAttempt(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.NewGuid(),
            PracticeSessionType.Flashcard,
            PracticeAttemptOutcome.Correct,
            DateTime.UtcNow,
            responseMilliseconds: 0));
    }

    /// <summary>
    /// Verifies that a negative response time is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectNegativeResponseMilliseconds()
    {
        Assert.Throws<DomainRuleException>(() => new PracticeAttempt(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.NewGuid(),
            PracticeSessionType.Flashcard,
            PracticeAttemptOutcome.Correct,
            DateTime.UtcNow,
            responseMilliseconds: -1));
    }

    /// <summary>
    /// Verifies that null optional fields are stored as null.
    /// </summary>
    [Fact]
    public void Constructor_ShouldAcceptNullOptionalFields()
    {
        PracticeAttempt attempt = new(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.NewGuid(),
            PracticeSessionType.Quiz,
            PracticeAttemptOutcome.Hard,
            DateTime.UtcNow);

        Assert.Null(attempt.DueAtUtcBeforeAttempt);
        Assert.Null(attempt.DueAtUtcAfterAttempt);
        Assert.Null(attempt.ResponseMilliseconds);
    }

    /// <summary>
    /// Verifies that a local (non-UTC) attempt timestamp is converted to UTC.
    /// </summary>
    [Fact]
    public void Constructor_ShouldConvertLocalAttemptTimestampToUtc()
    {
        DateTime localTime = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Local);

        PracticeAttempt attempt = new(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.NewGuid(),
            PracticeSessionType.Flashcard,
            PracticeAttemptOutcome.Easy,
            localTime);

        Assert.Equal(DateTimeKind.Utc, attempt.AttemptedAtUtc.Kind);
    }

    /// <summary>
    /// Verifies that local (non-UTC) optional due-at timestamps are converted to UTC.
    /// </summary>
    [Fact]
    public void Constructor_ShouldConvertLocalOptionalDueAtTimestampsToUtc()
    {
        DateTime localDueBefore = new(2025, 5, 31, 9, 0, 0, DateTimeKind.Local);
        DateTime localDueAfter = new(2025, 6, 4, 9, 0, 0, DateTimeKind.Local);

        PracticeAttempt attempt = new(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.NewGuid(),
            PracticeSessionType.Flashcard,
            PracticeAttemptOutcome.Correct,
            DateTime.UtcNow,
            localDueBefore,
            localDueAfter);

        Assert.Equal(DateTimeKind.Utc, attempt.DueAtUtcBeforeAttempt!.Value.Kind);
        Assert.Equal(DateTimeKind.Utc, attempt.DueAtUtcAfterAttempt!.Value.Kind);
    }

    /// <summary>
    /// Verifies that <see cref="PracticeAttempt.CreatedAtUtc"/> is set to the same value as the attempt timestamp.
    /// </summary>
    [Fact]
    public void Constructor_ShouldSetCreatedAtUtcToAttemptTimestamp()
    {
        DateTime attemptedAt = new(2025, 6, 1, 10, 0, 0, DateTimeKind.Utc);

        PracticeAttempt attempt = new(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.NewGuid(),
            PracticeSessionType.Quiz,
            PracticeAttemptOutcome.Correct,
            attemptedAt);

        Assert.Equal(attemptedAt, attempt.CreatedAtUtc);
    }
}

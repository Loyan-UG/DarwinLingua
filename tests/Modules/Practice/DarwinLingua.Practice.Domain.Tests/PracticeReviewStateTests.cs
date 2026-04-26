using DarwinLingua.Practice.Domain.Entities;
using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Practice.Domain.Tests;

/// <summary>
/// Verifies the <see cref="PracticeReviewState"/> aggregate behavior.
/// </summary>
public sealed class PracticeReviewStateTests
{
    /// <summary>
    /// Verifies that a valid review state is created with the expected initial property values.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateReviewStateWithExpectedProperties()
    {
        Guid id = Guid.NewGuid();
        Guid wordEntryPublicId = Guid.NewGuid();
        DateTime createdAt = DateTime.UtcNow;

        PracticeReviewState state = new(id, "local-installation-user", wordEntryPublicId, createdAt);

        Assert.Equal(id, state.Id);
        Assert.Equal("local-installation-user", state.UserId);
        Assert.Equal(wordEntryPublicId, state.WordEntryPublicId);
        Assert.Equal(createdAt, state.CreatedAtUtc);
        Assert.Equal(createdAt, state.UpdatedAtUtc);
        Assert.Null(state.DueAtUtc);
        Assert.Null(state.LastAttemptedAtUtc);
        Assert.Null(state.LastSuccessfulAttemptedAtUtc);
        Assert.Null(state.LastSessionType);
        Assert.Null(state.LastOutcome);
        Assert.Equal(0, state.ConsecutiveSuccessCount);
        Assert.Equal(0, state.ConsecutiveFailureCount);
        Assert.Equal(0, state.TotalAttemptCount);
    }

    /// <summary>
    /// Verifies that an empty internal identifier is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyIdentifier()
    {
        Assert.Throws<DomainRuleException>(() => new PracticeReviewState(
            Guid.Empty,
            "local-installation-user",
            Guid.NewGuid(),
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that an empty lexical-entry identifier is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyWordEntryPublicId()
    {
        Assert.Throws<DomainRuleException>(() => new PracticeReviewState(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.Empty,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that a blank user identifier is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectBlankUserId()
    {
        Assert.Throws<DomainRuleException>(() => new PracticeReviewState(
            Guid.NewGuid(),
            "   ",
            Guid.NewGuid(),
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that the user identifier is trimmed on construction.
    /// </summary>
    [Fact]
    public void Constructor_ShouldTrimUserId()
    {
        PracticeReviewState state = new(
            Guid.NewGuid(),
            "  local-installation-user  ",
            Guid.NewGuid(),
            DateTime.UtcNow);

        Assert.Equal("local-installation-user", state.UserId);
    }

    /// <summary>
    /// Verifies that a default (uninitialized) creation timestamp is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectDefaultCreatedAtUtc()
    {
        Assert.Throws<DomainRuleException>(() => new PracticeReviewState(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.NewGuid(),
            default));
    }

    /// <summary>
    /// Verifies that an incorrect outcome increments <see cref="PracticeReviewState.ConsecutiveFailureCount"/>
    /// and resets <see cref="PracticeReviewState.ConsecutiveSuccessCount"/> to zero.
    /// </summary>
    [Fact]
    public void RecordAttempt_IncorrectOutcome_ShouldIncrementFailureCountAndResetSuccessCount()
    {
        PracticeReviewState state = CreateReviewState();
        DateTime attemptedAt = DateTime.UtcNow;

        state.RecordAttempt(PracticeSessionType.Flashcard, PracticeAttemptOutcome.Incorrect, attemptedAt, null);

        Assert.Equal(1, state.ConsecutiveFailureCount);
        Assert.Equal(0, state.ConsecutiveSuccessCount);
        Assert.Equal(1, state.TotalAttemptCount);
    }

    /// <summary>
    /// Verifies that a correct outcome increments <see cref="PracticeReviewState.ConsecutiveSuccessCount"/>
    /// and resets <see cref="PracticeReviewState.ConsecutiveFailureCount"/> to zero.
    /// </summary>
    [Fact]
    public void RecordAttempt_CorrectOutcome_ShouldIncrementSuccessCountAndResetFailureCount()
    {
        PracticeReviewState state = CreateReviewState();

        state.RecordAttempt(PracticeSessionType.Quiz, PracticeAttemptOutcome.Incorrect, DateTime.UtcNow, null);
        state.RecordAttempt(PracticeSessionType.Quiz, PracticeAttemptOutcome.Correct, DateTime.UtcNow, null);

        Assert.Equal(1, state.ConsecutiveSuccessCount);
        Assert.Equal(0, state.ConsecutiveFailureCount);
    }

    /// <summary>
    /// Verifies that an easy outcome is treated as a success, incrementing the consecutive success counter.
    /// </summary>
    [Fact]
    public void RecordAttempt_EasyOutcome_ShouldIncrementSuccessCount()
    {
        PracticeReviewState state = CreateReviewState();

        state.RecordAttempt(PracticeSessionType.Flashcard, PracticeAttemptOutcome.Easy, DateTime.UtcNow, null);

        Assert.Equal(1, state.ConsecutiveSuccessCount);
        Assert.Equal(0, state.ConsecutiveFailureCount);
    }

    /// <summary>
    /// Verifies that a hard outcome is treated as a success, incrementing the consecutive success counter.
    /// </summary>
    [Fact]
    public void RecordAttempt_HardOutcome_ShouldIncrementSuccessCount()
    {
        PracticeReviewState state = CreateReviewState();

        state.RecordAttempt(PracticeSessionType.Flashcard, PracticeAttemptOutcome.Hard, DateTime.UtcNow, null);

        Assert.Equal(1, state.ConsecutiveSuccessCount);
        Assert.Equal(0, state.ConsecutiveFailureCount);
    }

    /// <summary>
    /// Verifies that <see cref="PracticeReviewState.RecordAttempt"/> updates the session type, outcome, and
    /// last-attempted timestamp.
    /// </summary>
    [Fact]
    public void RecordAttempt_ShouldUpdateLastSessionTypeOutcomeAndTimestamp()
    {
        PracticeReviewState state = CreateReviewState();
        DateTime attemptedAt = DateTime.UtcNow;

        state.RecordAttempt(PracticeSessionType.Review, PracticeAttemptOutcome.Hard, attemptedAt, null);

        Assert.Equal(PracticeSessionType.Review, state.LastSessionType);
        Assert.Equal(PracticeAttemptOutcome.Hard, state.LastOutcome);
        Assert.Equal(attemptedAt, state.LastAttemptedAtUtc);
        Assert.Equal(attemptedAt, state.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that <see cref="PracticeReviewState.TotalAttemptCount"/> increments on each recorded attempt.
    /// </summary>
    [Fact]
    public void RecordAttempt_ShouldIncrementTotalAttemptCountOnEachAttempt()
    {
        PracticeReviewState state = CreateReviewState();

        state.RecordAttempt(PracticeSessionType.Flashcard, PracticeAttemptOutcome.Correct, DateTime.UtcNow, null);
        state.RecordAttempt(PracticeSessionType.Flashcard, PracticeAttemptOutcome.Incorrect, DateTime.UtcNow, null);
        state.RecordAttempt(PracticeSessionType.Flashcard, PracticeAttemptOutcome.Easy, DateTime.UtcNow, null);

        Assert.Equal(3, state.TotalAttemptCount);
    }

    /// <summary>
    /// Verifies that a successful attempt updates <see cref="PracticeReviewState.LastSuccessfulAttemptedAtUtc"/>.
    /// </summary>
    [Fact]
    public void RecordAttempt_SuccessfulOutcome_ShouldSetLastSuccessfulAttemptedAtUtc()
    {
        PracticeReviewState state = CreateReviewState();
        DateTime attemptedAt = DateTime.UtcNow;

        state.RecordAttempt(PracticeSessionType.Flashcard, PracticeAttemptOutcome.Correct, attemptedAt, null);

        Assert.Equal(attemptedAt, state.LastSuccessfulAttemptedAtUtc);
    }

    /// <summary>
    /// Verifies that an incorrect attempt does not set <see cref="PracticeReviewState.LastSuccessfulAttemptedAtUtc"/>.
    /// </summary>
    [Fact]
    public void RecordAttempt_IncorrectOutcome_ShouldNotSetLastSuccessfulAttemptedAtUtc()
    {
        PracticeReviewState state = CreateReviewState();

        state.RecordAttempt(PracticeSessionType.Flashcard, PracticeAttemptOutcome.Incorrect, DateTime.UtcNow, null);

        Assert.Null(state.LastSuccessfulAttemptedAtUtc);
    }

    /// <summary>
    /// Verifies that the due date after an attempt is persisted when provided.
    /// </summary>
    [Fact]
    public void RecordAttempt_ShouldPersistDueAtUtcWhenProvided()
    {
        PracticeReviewState state = CreateReviewState();
        DateTime dueAt = DateTime.UtcNow.AddDays(3);

        state.RecordAttempt(PracticeSessionType.Flashcard, PracticeAttemptOutcome.Correct, DateTime.UtcNow, dueAt);

        Assert.Equal(dueAt, state.DueAtUtc);
    }

    /// <summary>
    /// Verifies that <see cref="PracticeReviewState.SetDueAt"/> updates the due timestamp and the updated
    /// timestamp without recording an attempt.
    /// </summary>
    [Fact]
    public void SetDueAt_ShouldUpdateDueAtAndUpdatedAtUtc()
    {
        PracticeReviewState state = CreateReviewState();
        DateTime dueAt = DateTime.UtcNow.AddDays(7);
        DateTime updatedAt = DateTime.UtcNow;

        state.SetDueAt(dueAt, updatedAt);

        Assert.Equal(dueAt, state.DueAtUtc);
        Assert.Equal(updatedAt, state.UpdatedAtUtc);
        Assert.Equal(0, state.TotalAttemptCount);
    }

    /// <summary>
    /// Verifies that <see cref="PracticeReviewState.SetDueAt"/> clears the due date when null is provided.
    /// </summary>
    [Fact]
    public void SetDueAt_ShouldClearDueAtWhenNullIsProvided()
    {
        PracticeReviewState state = CreateReviewState();
        state.SetDueAt(DateTime.UtcNow.AddDays(3), DateTime.UtcNow);

        state.SetDueAt(null, DateTime.UtcNow.AddMinutes(1));

        Assert.Null(state.DueAtUtc);
    }

    /// <summary>
    /// Verifies that a default (uninitialized) attempt timestamp is rejected by <see cref="PracticeReviewState.RecordAttempt"/>.
    /// </summary>
    [Fact]
    public void RecordAttempt_ShouldRejectDefaultAttemptedAtUtc()
    {
        PracticeReviewState state = CreateReviewState();

        Assert.Throws<DomainRuleException>(() =>
            state.RecordAttempt(PracticeSessionType.Flashcard, PracticeAttemptOutcome.Correct, default, null));
    }

    /// <summary>
    /// Verifies that a default (uninitialized) updated timestamp is rejected by <see cref="PracticeReviewState.SetDueAt"/>.
    /// </summary>
    [Fact]
    public void SetDueAt_ShouldRejectDefaultUpdatedAtUtc()
    {
        PracticeReviewState state = CreateReviewState();

        Assert.Throws<DomainRuleException>(() => state.SetDueAt(null, default));
    }

    /// <summary>
    /// Verifies that a local (non-UTC) attempt timestamp passed to <see cref="PracticeReviewState.RecordAttempt"/>
    /// is converted to UTC before being stored.
    /// </summary>
    [Fact]
    public void RecordAttempt_ShouldConvertLocalTimestampToUtc()
    {
        PracticeReviewState state = CreateReviewState();
        DateTime localTime = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Local);

        state.RecordAttempt(PracticeSessionType.Flashcard, PracticeAttemptOutcome.Correct, localTime, null);

        Assert.Equal(DateTimeKind.Utc, state.LastAttemptedAtUtc!.Value.Kind);
        Assert.Equal(DateTimeKind.Utc, state.UpdatedAtUtc.Kind);
    }

    /// <summary>
    /// Verifies that a local (non-UTC) updated timestamp passed to <see cref="PracticeReviewState.SetDueAt"/>
    /// is converted to UTC before being stored.
    /// </summary>
    [Fact]
    public void SetDueAt_ShouldConvertLocalUpdatedAtToUtc()
    {
        PracticeReviewState state = CreateReviewState();
        DateTime localTime = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Local);

        state.SetDueAt(null, localTime);

        Assert.Equal(DateTimeKind.Utc, state.UpdatedAtUtc.Kind);
    }

    /// <summary>
    /// Verifies that <see cref="PracticeReviewState.RecordAttempt"/> with a null due date
    /// leaves <see cref="PracticeReviewState.DueAtUtc"/> as null.
    /// </summary>
    [Fact]
    public void RecordAttempt_NullDueAtUtc_ShouldLeaveDueAtUtcNull()
    {
        PracticeReviewState state = CreateReviewState();

        state.RecordAttempt(PracticeSessionType.Flashcard, PracticeAttemptOutcome.Correct, DateTime.UtcNow, null);

        Assert.Null(state.DueAtUtc);
    }

    /// <summary>
    /// Verifies that a local (non-UTC) creation timestamp is converted to UTC.
    /// </summary>
    [Fact]
    public void Constructor_ShouldConvertLocalCreatedAtToUtc()
    {
        DateTime localTime = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Local);

        PracticeReviewState state = new(Guid.NewGuid(), "local-installation-user", Guid.NewGuid(), localTime);

        Assert.Equal(DateTimeKind.Utc, state.CreatedAtUtc.Kind);
    }

    private static PracticeReviewState CreateReviewState()
    {
        return new PracticeReviewState(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.NewGuid(),
            DateTime.UtcNow);
    }
}

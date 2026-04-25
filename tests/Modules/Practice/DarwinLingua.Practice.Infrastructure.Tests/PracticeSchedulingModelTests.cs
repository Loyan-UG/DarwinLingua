using DarwinLingua.Practice.Domain.Entities;
using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Practice.Infrastructure.Tests;

/// <summary>
/// Verifies core Practice scheduling-domain invariants and state transitions.
/// </summary>
public sealed class PracticeSchedulingModelTests
{
    /// <summary>
    /// Verifies that review state tracks success and failure streaks consistently.
    /// </summary>
    [Fact]
    public void PracticeReviewState_RecordAttempt_ShouldUpdateCountersAndDueDate()
    {
        PracticeReviewState reviewState = new(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(-5));

        DateTime firstAttemptAtUtc = DateTime.UtcNow.AddMinutes(-2);
        DateTime secondAttemptAtUtc = DateTime.UtcNow.AddMinutes(-1);

        reviewState.RecordAttempt(
            PracticeSessionType.Flashcard,
            PracticeAttemptOutcome.Correct,
            firstAttemptAtUtc,
            firstAttemptAtUtc.AddHours(1));

        reviewState.RecordAttempt(
            PracticeSessionType.Quiz,
            PracticeAttemptOutcome.Incorrect,
            secondAttemptAtUtc,
            secondAttemptAtUtc.AddMinutes(10));

        Assert.Equal(2, reviewState.TotalAttemptCount);
        Assert.Equal(0, reviewState.ConsecutiveSuccessCount);
        Assert.Equal(1, reviewState.ConsecutiveFailureCount);
        Assert.Equal(PracticeSessionType.Quiz, reviewState.LastSessionType);
        Assert.Equal(PracticeAttemptOutcome.Incorrect, reviewState.LastOutcome);
        Assert.Equal(secondAttemptAtUtc.AddMinutes(10), reviewState.DueAtUtc);
        Assert.Equal(firstAttemptAtUtc, reviewState.LastSuccessfulAttemptedAtUtc);
    }

    /// <summary>
    /// Verifies that practice attempts reject invalid response-duration values.
    /// </summary>
    [Fact]
    public void PracticeAttempt_ShouldRejectNonPositiveResponseMilliseconds()
    {
        Assert.Throws<DomainRuleException>(() => new PracticeAttempt(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.NewGuid(),
            PracticeSessionType.Review,
            PracticeAttemptOutcome.Hard,
            DateTime.UtcNow,
            responseMilliseconds: 0));
    }

    /// <summary>
    /// Verifies that a null response-duration value is allowed.
    /// </summary>
    [Fact]
    public void PracticeAttempt_ShouldAllowNullResponseMilliseconds()
    {
        PracticeAttempt attempt = new(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.NewGuid(),
            PracticeSessionType.Flashcard,
            PracticeAttemptOutcome.Correct,
            DateTime.UtcNow,
            responseMilliseconds: null);

        Assert.Null(attempt.ResponseMilliseconds);
    }

    /// <summary>
    /// Verifies that an empty attempt identifier is rejected.
    /// </summary>
    [Fact]
    public void PracticeAttempt_ShouldRejectEmptyIdentifier()
    {
        Assert.Throws<DomainRuleException>(() => new PracticeAttempt(
            Guid.Empty,
            "local-installation-user",
            Guid.NewGuid(),
            PracticeSessionType.Review,
            PracticeAttemptOutcome.Correct,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that an empty lexical-entry identifier is rejected.
    /// </summary>
    [Fact]
    public void PracticeAttempt_ShouldRejectEmptyWordEntryPublicId()
    {
        Assert.Throws<DomainRuleException>(() => new PracticeAttempt(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.Empty,
            PracticeSessionType.Review,
            PracticeAttemptOutcome.Correct,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that a blank user identifier is rejected.
    /// </summary>
    [Fact]
    public void PracticeAttempt_ShouldRejectEmptyUserId()
    {
        Assert.Throws<DomainRuleException>(() => new PracticeAttempt(
            Guid.NewGuid(),
            "   ",
            Guid.NewGuid(),
            PracticeSessionType.Review,
            PracticeAttemptOutcome.Correct,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that an empty review-state identifier is rejected.
    /// </summary>
    [Fact]
    public void PracticeReviewState_ShouldRejectEmptyIdentifier()
    {
        Assert.Throws<DomainRuleException>(() => new PracticeReviewState(
            Guid.Empty,
            "local-installation-user",
            Guid.NewGuid(),
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that an empty lexical-entry identifier is rejected in review state.
    /// </summary>
    [Fact]
    public void PracticeReviewState_ShouldRejectEmptyWordEntryPublicId()
    {
        Assert.Throws<DomainRuleException>(() => new PracticeReviewState(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.Empty,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that a blank user identifier is rejected in review state.
    /// </summary>
    [Fact]
    public void PracticeReviewState_ShouldRejectEmptyUserId()
    {
        Assert.Throws<DomainRuleException>(() => new PracticeReviewState(
            Guid.NewGuid(),
            "   ",
            Guid.NewGuid(),
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that <see cref="PracticeReviewState.SetDueAt"/> persists the due date and updates the timestamp.
    /// </summary>
    [Fact]
    public void PracticeReviewState_SetDueAt_ShouldPersistDueDateAndTimestamp()
    {
        PracticeReviewState reviewState = new(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(-5));

        DateTime dueAt = DateTime.UtcNow.AddDays(1);
        DateTime updatedAt = DateTime.UtcNow;

        reviewState.SetDueAt(dueAt, updatedAt);

        Assert.Equal(dueAt, reviewState.DueAtUtc);
        Assert.Equal(updatedAt, reviewState.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that <see cref="PracticeReviewState.SetDueAt"/> with a null value clears the due date.
    /// </summary>
    [Fact]
    public void PracticeReviewState_SetDueAt_ShouldClearDueDateWhenNull()
    {
        PracticeReviewState reviewState = new(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(-5));

        reviewState.SetDueAt(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddMinutes(-3));
        reviewState.SetDueAt(null, DateTime.UtcNow);

        Assert.Null(reviewState.DueAtUtc);
    }
}

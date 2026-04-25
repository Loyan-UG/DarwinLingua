using DarwinLingua.Practice.Application.Services;
using DarwinLingua.Practice.Domain.Entities;

namespace DarwinLingua.Practice.Application.Tests;

/// <summary>
/// Verifies all branching paths in <see cref="PracticeSchedulingPolicy"/> deterministically.
/// </summary>
public sealed class PracticeSchedulingPolicyTests
{
    private static readonly DateTime BaseUtc = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    // ── Incorrect outcome ─────────────────────────────────────────────────────

    /// <summary>
    /// First failure (no prior failures) yields the shortest retry interval of 10 minutes.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Incorrect_FirstFailure_Returns10Minutes()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Incorrect,
            consecutiveSuccessCountBeforeAttempt: 0,
            consecutiveFailureCountBeforeAttempt: 0,
            attemptedAtUtc: BaseUtc);

        Assert.Equal(BaseUtc.AddMinutes(10), result);
    }

    /// <summary>
    /// Second consecutive failure (one prior failure) yields a 30-minute retry interval.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Incorrect_SecondFailure_Returns30Minutes()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Incorrect,
            consecutiveSuccessCountBeforeAttempt: 0,
            consecutiveFailureCountBeforeAttempt: 1,
            attemptedAtUtc: BaseUtc);

        Assert.Equal(BaseUtc.AddMinutes(30), result);
    }

    /// <summary>
    /// Three or more consecutive failures yield a 2-hour retry interval.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Incorrect_ThirdOrMoreFailure_Returns2Hours()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Incorrect,
            consecutiveSuccessCountBeforeAttempt: 0,
            consecutiveFailureCountBeforeAttempt: 2,
            attemptedAtUtc: BaseUtc);

        Assert.Equal(BaseUtc.AddHours(2), result);
    }

    /// <summary>
    /// Many consecutive failures still yield the 2-hour cap.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Incorrect_ManyFailures_StillReturns2Hours()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Incorrect,
            consecutiveSuccessCountBeforeAttempt: 0,
            consecutiveFailureCountBeforeAttempt: 10,
            attemptedAtUtc: BaseUtc);

        Assert.Equal(BaseUtc.AddHours(2), result);
    }

    // ── Hard outcome ──────────────────────────────────────────────────────────

    /// <summary>
    /// First hard answer (no prior successes) yields an 8-hour interval.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Hard_FirstAttempt_Returns8Hours()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Hard,
            consecutiveSuccessCountBeforeAttempt: 0,
            consecutiveFailureCountBeforeAttempt: 0,
            attemptedAtUtc: BaseUtc);

        Assert.Equal(BaseUtc.AddHours(8), result);
    }

    /// <summary>
    /// Hard answer after 1 prior consecutive success yields 1 day.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Hard_OneSuccess_Returns1Day()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Hard,
            consecutiveSuccessCountBeforeAttempt: 1,
            consecutiveFailureCountBeforeAttempt: 0,
            attemptedAtUtc: BaseUtc);

        Assert.Equal(BaseUtc.AddDays(1), result);
    }

    /// <summary>
    /// Hard answer after 2 prior consecutive successes yields 3 days.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Hard_TwoSuccesses_Returns3Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Hard,
            consecutiveSuccessCountBeforeAttempt: 2,
            consecutiveFailureCountBeforeAttempt: 0,
            attemptedAtUtc: BaseUtc);

        Assert.Equal(BaseUtc.AddDays(3), result);
    }

    /// <summary>
    /// Hard answer after 3 or more prior consecutive successes yields 7 days.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Hard_ThreeOrMoreSuccesses_Returns7Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Hard,
            consecutiveSuccessCountBeforeAttempt: 3,
            consecutiveFailureCountBeforeAttempt: 0,
            attemptedAtUtc: BaseUtc);

        Assert.Equal(BaseUtc.AddDays(7), result);
    }

    // ── Correct outcome ───────────────────────────────────────────────────────

    /// <summary>
    /// First correct answer yields 1 day.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Correct_FirstAttempt_Returns1Day()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Correct,
            consecutiveSuccessCountBeforeAttempt: 0,
            consecutiveFailureCountBeforeAttempt: 0,
            attemptedAtUtc: BaseUtc);

        Assert.Equal(BaseUtc.AddDays(1), result);
    }

    /// <summary>
    /// Correct answer after 1 prior consecutive success yields 3 days.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Correct_OneSuccess_Returns3Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Correct,
            consecutiveSuccessCountBeforeAttempt: 1,
            consecutiveFailureCountBeforeAttempt: 0,
            attemptedAtUtc: BaseUtc);

        Assert.Equal(BaseUtc.AddDays(3), result);
    }

    /// <summary>
    /// Correct answer after 2 prior consecutive successes yields 7 days.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Correct_TwoSuccesses_Returns7Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Correct,
            consecutiveSuccessCountBeforeAttempt: 2,
            consecutiveFailureCountBeforeAttempt: 0,
            attemptedAtUtc: BaseUtc);

        Assert.Equal(BaseUtc.AddDays(7), result);
    }

    /// <summary>
    /// Correct answer after 3 prior consecutive successes yields 14 days.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Correct_ThreeSuccesses_Returns14Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Correct,
            consecutiveSuccessCountBeforeAttempt: 3,
            consecutiveFailureCountBeforeAttempt: 0,
            attemptedAtUtc: BaseUtc);

        Assert.Equal(BaseUtc.AddDays(14), result);
    }

    /// <summary>
    /// Correct answer after 4 or more prior consecutive successes yields 30 days.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Correct_FourOrMoreSuccesses_Returns30Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Correct,
            consecutiveSuccessCountBeforeAttempt: 4,
            consecutiveFailureCountBeforeAttempt: 0,
            attemptedAtUtc: BaseUtc);

        Assert.Equal(BaseUtc.AddDays(30), result);
    }

    // ── Easy outcome ──────────────────────────────────────────────────────────

    /// <summary>
    /// First easy answer yields 3 days.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Easy_FirstAttempt_Returns3Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Easy,
            consecutiveSuccessCountBeforeAttempt: 0,
            consecutiveFailureCountBeforeAttempt: 0,
            attemptedAtUtc: BaseUtc);

        Assert.Equal(BaseUtc.AddDays(3), result);
    }

    /// <summary>
    /// Easy answer after 1 prior consecutive success yields 7 days.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Easy_OneSuccess_Returns7Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Easy,
            consecutiveSuccessCountBeforeAttempt: 1,
            consecutiveFailureCountBeforeAttempt: 0,
            attemptedAtUtc: BaseUtc);

        Assert.Equal(BaseUtc.AddDays(7), result);
    }

    /// <summary>
    /// Easy answer after 2 prior consecutive successes yields 14 days.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Easy_TwoSuccesses_Returns14Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Easy,
            consecutiveSuccessCountBeforeAttempt: 2,
            consecutiveFailureCountBeforeAttempt: 0,
            attemptedAtUtc: BaseUtc);

        Assert.Equal(BaseUtc.AddDays(14), result);
    }

    /// <summary>
    /// Easy answer after 3 prior consecutive successes yields 30 days.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Easy_ThreeSuccesses_Returns30Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Easy,
            consecutiveSuccessCountBeforeAttempt: 3,
            consecutiveFailureCountBeforeAttempt: 0,
            attemptedAtUtc: BaseUtc);

        Assert.Equal(BaseUtc.AddDays(30), result);
    }

    /// <summary>
    /// Easy answer after 4 or more prior consecutive successes yields 45 days.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Easy_FourOrMoreSuccesses_Returns45Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Easy,
            consecutiveSuccessCountBeforeAttempt: 4,
            consecutiveFailureCountBeforeAttempt: 0,
            attemptedAtUtc: BaseUtc);

        Assert.Equal(BaseUtc.AddDays(45), result);
    }

    // ── Timestamp normalization ───────────────────────────────────────────────

    /// <summary>
    /// A local-time <paramref name="attemptedAtUtc"/> is converted to UTC before computing the due date.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_LocalTime_IsNormalizedToUtcBeforeComputing()
    {
        DateTime localTime = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Local);
        DateTime expectedUtcBase = localTime.ToUniversalTime();

        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Correct,
            consecutiveSuccessCountBeforeAttempt: 0,
            consecutiveFailureCountBeforeAttempt: 0,
            attemptedAtUtc: localTime);

        Assert.Equal(expectedUtcBase.AddDays(1), result);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    /// <summary>
    /// A UTC <paramref name="attemptedAtUtc"/> is passed through unchanged.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_UtcTime_IsPassedThroughUnchanged()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Correct,
            consecutiveSuccessCountBeforeAttempt: 0,
            consecutiveFailureCountBeforeAttempt: 0,
            attemptedAtUtc: BaseUtc);

        Assert.Equal(BaseUtc.AddDays(1), result);
    }

    // ── Unsupported outcome ───────────────────────────────────────────────────

    /// <summary>
    /// An unrecognized outcome value throws <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_UnrecognizedOutcome_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PracticeSchedulingPolicy.GetNextDueAtUtc(
            (PracticeAttemptOutcome)999,
            consecutiveSuccessCountBeforeAttempt: 0,
            consecutiveFailureCountBeforeAttempt: 0,
            attemptedAtUtc: BaseUtc));
    }
}

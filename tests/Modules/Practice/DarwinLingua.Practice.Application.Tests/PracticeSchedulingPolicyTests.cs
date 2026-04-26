using DarwinLingua.Practice.Application.Services;
using DarwinLingua.Practice.Domain.Entities;

namespace DarwinLingua.Practice.Application.Tests;

/// <summary>
/// Verifies the deterministic interval table of <see cref="PracticeSchedulingPolicy"/>.
/// </summary>
public sealed class PracticeSchedulingPolicyTests
{
    private static readonly DateTime BaseTime = new(2025, 6, 1, 10, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Verifies the Incorrect-outcome interval for zero previous failures (first failure → 10 minutes).
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Incorrect_ZeroFailures_Returns10Minutes()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Incorrect,
            consecutiveSuccessCountBeforeAttempt: 0,
            consecutiveFailureCountBeforeAttempt: 0,
            BaseTime);

        Assert.Equal(BaseTime.AddMinutes(10), result);
    }

    /// <summary>
    /// Verifies the Incorrect-outcome interval for one previous failure (second failure → 30 minutes).
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Incorrect_OneFailure_Returns30Minutes()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Incorrect,
            consecutiveSuccessCountBeforeAttempt: 0,
            consecutiveFailureCountBeforeAttempt: 1,
            BaseTime);

        Assert.Equal(BaseTime.AddMinutes(30), result);
    }

    /// <summary>
    /// Verifies the Incorrect-outcome interval for two or more previous failures (repeated failure → 2 hours).
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Incorrect_TwoPlusFailures_Returns2Hours()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Incorrect,
            consecutiveSuccessCountBeforeAttempt: 0,
            consecutiveFailureCountBeforeAttempt: 2,
            BaseTime);

        Assert.Equal(BaseTime.AddHours(2), result);
    }

    /// <summary>
    /// Verifies the Hard-outcome interval with no prior successes (first hard → 8 hours).
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Hard_ZeroSuccesses_Returns8Hours()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Hard,
            consecutiveSuccessCountBeforeAttempt: 0,
            consecutiveFailureCountBeforeAttempt: 0,
            BaseTime);

        Assert.Equal(BaseTime.AddHours(8), result);
    }

    /// <summary>
    /// Verifies the Hard-outcome interval after one consecutive success (→ 1 day).
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Hard_OneSuccess_Returns1Day()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Hard,
            consecutiveSuccessCountBeforeAttempt: 1,
            consecutiveFailureCountBeforeAttempt: 0,
            BaseTime);

        Assert.Equal(BaseTime.AddDays(1), result);
    }

    /// <summary>
    /// Verifies the Hard-outcome interval after two consecutive successes (→ 3 days).
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Hard_TwoSuccesses_Returns3Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Hard,
            consecutiveSuccessCountBeforeAttempt: 2,
            consecutiveFailureCountBeforeAttempt: 0,
            BaseTime);

        Assert.Equal(BaseTime.AddDays(3), result);
    }

    /// <summary>
    /// Verifies the Hard-outcome interval after three or more consecutive successes (→ 7 days).
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Hard_ThreePlusSuccesses_Returns7Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Hard,
            consecutiveSuccessCountBeforeAttempt: 3,
            consecutiveFailureCountBeforeAttempt: 0,
            BaseTime);

        Assert.Equal(BaseTime.AddDays(7), result);
    }

    /// <summary>
    /// Verifies the Correct-outcome interval with no prior successes (first correct → 1 day).
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Correct_ZeroSuccesses_Returns1Day()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Correct,
            consecutiveSuccessCountBeforeAttempt: 0,
            consecutiveFailureCountBeforeAttempt: 0,
            BaseTime);

        Assert.Equal(BaseTime.AddDays(1), result);
    }

    /// <summary>
    /// Verifies the Correct-outcome interval after one consecutive success (→ 3 days).
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Correct_OneSuccess_Returns3Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Correct,
            consecutiveSuccessCountBeforeAttempt: 1,
            consecutiveFailureCountBeforeAttempt: 0,
            BaseTime);

        Assert.Equal(BaseTime.AddDays(3), result);
    }

    /// <summary>
    /// Verifies the Correct-outcome interval after two consecutive successes (→ 7 days).
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Correct_TwoSuccesses_Returns7Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Correct,
            consecutiveSuccessCountBeforeAttempt: 2,
            consecutiveFailureCountBeforeAttempt: 0,
            BaseTime);

        Assert.Equal(BaseTime.AddDays(7), result);
    }

    /// <summary>
    /// Verifies the Correct-outcome interval after three consecutive successes (→ 14 days).
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Correct_ThreeSuccesses_Returns14Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Correct,
            consecutiveSuccessCountBeforeAttempt: 3,
            consecutiveFailureCountBeforeAttempt: 0,
            BaseTime);

        Assert.Equal(BaseTime.AddDays(14), result);
    }

    /// <summary>
    /// Verifies the Correct-outcome interval after four or more consecutive successes (→ 30 days).
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Correct_FourPlusSuccesses_Returns30Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Correct,
            consecutiveSuccessCountBeforeAttempt: 4,
            consecutiveFailureCountBeforeAttempt: 0,
            BaseTime);

        Assert.Equal(BaseTime.AddDays(30), result);
    }

    /// <summary>
    /// Verifies the Easy-outcome interval with no prior successes (first easy → 3 days).
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Easy_ZeroSuccesses_Returns3Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Easy,
            consecutiveSuccessCountBeforeAttempt: 0,
            consecutiveFailureCountBeforeAttempt: 0,
            BaseTime);

        Assert.Equal(BaseTime.AddDays(3), result);
    }

    /// <summary>
    /// Verifies the Easy-outcome interval after one consecutive success (→ 7 days).
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Easy_OneSuccess_Returns7Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Easy,
            consecutiveSuccessCountBeforeAttempt: 1,
            consecutiveFailureCountBeforeAttempt: 0,
            BaseTime);

        Assert.Equal(BaseTime.AddDays(7), result);
    }

    /// <summary>
    /// Verifies the Easy-outcome interval after two consecutive successes (→ 14 days).
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Easy_TwoSuccesses_Returns14Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Easy,
            consecutiveSuccessCountBeforeAttempt: 2,
            consecutiveFailureCountBeforeAttempt: 0,
            BaseTime);

        Assert.Equal(BaseTime.AddDays(14), result);
    }

    /// <summary>
    /// Verifies the Easy-outcome interval after three consecutive successes (→ 30 days).
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Easy_ThreeSuccesses_Returns30Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Easy,
            consecutiveSuccessCountBeforeAttempt: 3,
            consecutiveFailureCountBeforeAttempt: 0,
            BaseTime);

        Assert.Equal(BaseTime.AddDays(30), result);
    }

    /// <summary>
    /// Verifies the Easy-outcome interval after four or more consecutive successes (→ 45 days).
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_Easy_FourPlusSuccesses_Returns45Days()
    {
        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Easy,
            consecutiveSuccessCountBeforeAttempt: 4,
            consecutiveFailureCountBeforeAttempt: 0,
            BaseTime);

        Assert.Equal(BaseTime.AddDays(45), result);
    }

    /// <summary>
    /// Verifies that a local (non-UTC) attempt timestamp is converted to UTC before adding the interval.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_ShouldConvertLocalTimestampToUtcBeforeApplyingInterval()
    {
        DateTime localTime = new(2025, 6, 1, 10, 0, 0, DateTimeKind.Local);

        DateTime result = PracticeSchedulingPolicy.GetNextDueAtUtc(
            PracticeAttemptOutcome.Correct,
            consecutiveSuccessCountBeforeAttempt: 0,
            consecutiveFailureCountBeforeAttempt: 0,
            localTime);

        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    /// <summary>
    /// Verifies that an unsupported outcome value throws <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void GetNextDueAtUtc_ShouldThrowForUnsupportedOutcome()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PracticeSchedulingPolicy.GetNextDueAtUtc(
            (PracticeAttemptOutcome)999,
            consecutiveSuccessCountBeforeAttempt: 0,
            consecutiveFailureCountBeforeAttempt: 0,
            BaseTime));
    }
}

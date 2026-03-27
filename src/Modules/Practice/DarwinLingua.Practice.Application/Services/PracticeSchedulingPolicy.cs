using DarwinLingua.Practice.Domain.Entities;

namespace DarwinLingua.Practice.Application.Services;

/// <summary>
/// Defines the deterministic Phase 2 scheduling policy for local practice answers.
/// </summary>
internal static class PracticeSchedulingPolicy
{
    /// <summary>
    /// Calculates the next due timestamp after one learner attempt.
    /// </summary>
    internal static DateTime GetNextDueAtUtc(
        PracticeAttemptOutcome outcome,
        int consecutiveSuccessCountBeforeAttempt,
        int consecutiveFailureCountBeforeAttempt,
        DateTime attemptedAtUtc)
    {
        attemptedAtUtc = attemptedAtUtc.Kind == DateTimeKind.Utc
            ? attemptedAtUtc
            : attemptedAtUtc.ToUniversalTime();

        TimeSpan interval = outcome switch
        {
            PracticeAttemptOutcome.Incorrect => GetFailureInterval(consecutiveFailureCountBeforeAttempt),
            PracticeAttemptOutcome.Hard => GetHardInterval(consecutiveSuccessCountBeforeAttempt),
            PracticeAttemptOutcome.Correct => GetCorrectInterval(consecutiveSuccessCountBeforeAttempt),
            PracticeAttemptOutcome.Easy => GetEasyInterval(consecutiveSuccessCountBeforeAttempt),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Unsupported practice attempt outcome."),
        };

        return attemptedAtUtc.Add(interval);
    }

    private static TimeSpan GetFailureInterval(int consecutiveFailureCountBeforeAttempt)
    {
        return consecutiveFailureCountBeforeAttempt switch
        {
            <= 0 => TimeSpan.FromMinutes(10),
            1 => TimeSpan.FromMinutes(30),
            _ => TimeSpan.FromHours(2),
        };
    }

    private static TimeSpan GetHardInterval(int consecutiveSuccessCountBeforeAttempt)
    {
        return consecutiveSuccessCountBeforeAttempt switch
        {
            <= 0 => TimeSpan.FromHours(8),
            1 => TimeSpan.FromDays(1),
            2 => TimeSpan.FromDays(3),
            _ => TimeSpan.FromDays(7),
        };
    }

    private static TimeSpan GetCorrectInterval(int consecutiveSuccessCountBeforeAttempt)
    {
        return consecutiveSuccessCountBeforeAttempt switch
        {
            <= 0 => TimeSpan.FromDays(1),
            1 => TimeSpan.FromDays(3),
            2 => TimeSpan.FromDays(7),
            3 => TimeSpan.FromDays(14),
            _ => TimeSpan.FromDays(30),
        };
    }

    private static TimeSpan GetEasyInterval(int consecutiveSuccessCountBeforeAttempt)
    {
        return consecutiveSuccessCountBeforeAttempt switch
        {
            <= 0 => TimeSpan.FromDays(3),
            1 => TimeSpan.FromDays(7),
            2 => TimeSpan.FromDays(14),
            3 => TimeSpan.FromDays(30),
            _ => TimeSpan.FromDays(45),
        };
    }
}

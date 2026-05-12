using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Learning.Domain.Entities;

/// <summary>
/// Represents user-specific progress for any Learning Portal content item.
/// </summary>
public sealed class UserContentProgress
{
    public static readonly IReadOnlySet<string> ValidOwnerTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "word",
        "grammar-topic",
        "expression",
        "dialogue",
        "talk-topic",
        "exercise",
        "exercise-set",
        "course",
        "course-module",
        "course-lesson",
        "exam-prep-unit",
        "writing-template",
        "cultural-note",
    };

    public static readonly IReadOnlySet<string> ValidStates = new HashSet<string>(StringComparer.Ordinal)
    {
        "not-started",
        "viewed",
        "in-progress",
        "completed",
        "needs-review",
        "skipped",
    };

    private UserContentProgress()
    {
        UserId = string.Empty;
        ContentOwnerType = string.Empty;
        ContentOwnerSlug = string.Empty;
        State = "not-started";
    }

    public UserContentProgress(
        Guid id,
        string userId,
        string contentOwnerType,
        string contentOwnerSlug,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("User-content-progress identifier cannot be empty.");
        }

        Id = id;
        UserId = NormalizeRequiredText(userId, nameof(userId), 256);
        ContentOwnerType = NormalizeControlledValue(contentOwnerType, ValidOwnerTypes, nameof(contentOwnerType));
        ContentOwnerSlug = NormalizeRequiredText(contentOwnerSlug, nameof(contentOwnerSlug), 256).ToLowerInvariant();
        State = "not-started";
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string UserId { get; private set; }

    public string ContentOwnerType { get; private set; }

    public string ContentOwnerSlug { get; private set; }

    public string State { get; private set; }

    public DateTime? FirstViewedAtUtc { get; private set; }

    public DateTime? LastViewedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public int ViewCount { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public void ApplyState(string state, DateTime updatedAtUtc)
    {
        string normalizedState = NormalizeControlledValue(state, ValidStates, nameof(state));
        DateTime normalizedUpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));

        if (normalizedState is "viewed" or "in-progress" or "completed")
        {
            FirstViewedAtUtc ??= normalizedUpdatedAtUtc;
            LastViewedAtUtc = normalizedUpdatedAtUtc;
        }

        if (normalizedState == "viewed")
        {
            ViewCount++;
        }

        CompletedAtUtc = normalizedState == "completed"
            ? normalizedUpdatedAtUtc
            : CompletedAtUtc;
        State = normalizedState;
        UpdatedAtUtc = normalizedUpdatedAtUtc;
    }

    private static string NormalizeControlledValue(
        string value,
        IReadOnlySet<string> validValues,
        string parameterName)
    {
        string normalized = NormalizeRequiredText(value, parameterName, 128).ToLowerInvariant();
        if (!validValues.Contains(normalized))
        {
            throw new DomainRuleException($"{parameterName} has unsupported value '{value}'.");
        }

        return normalized;
    }

    private static string NormalizeRequiredText(string value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException($"{parameterName} cannot be empty.");
        }

        string normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainRuleException($"{parameterName} cannot exceed {maxLength} characters.");
        }

        return normalized;
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

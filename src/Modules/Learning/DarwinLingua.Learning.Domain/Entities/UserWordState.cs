using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Learning.Domain.Entities;

/// <summary>
/// Represents lightweight user-specific learning state for a lexical entry.
/// </summary>
public sealed class UserWordState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserWordState"/> class for EF Core materialization.
    /// </summary>
    private UserWordState()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserWordState"/> class.
    /// </summary>
    public UserWordState(Guid id, string userId, Guid wordEntryPublicId, DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("User-word-state identifier cannot be empty.");
        }

        if (wordEntryPublicId == Guid.Empty)
        {
            throw new DomainRuleException("User-word-state lexical entry identifier cannot be empty.");
        }

        Id = id;
        UserId = NormalizeRequiredText(userId, nameof(userId));
        WordEntryPublicId = wordEntryPublicId;
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
        ViewCount = 0;
    }

    /// <summary>
    /// Gets the stable internal identifier of the user-word-state row.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the stable local user identifier.
    /// </summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the public lexical-entry identifier referenced by the user state.
    /// </summary>
    public Guid WordEntryPublicId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the user marked the word as known.
    /// </summary>
    public bool IsKnown { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the user marked the word as difficult.
    /// </summary>
    public bool IsDifficult { get; private set; }

    /// <summary>
    /// Gets the first UTC timestamp when the word detail was viewed.
    /// </summary>
    public DateTime? FirstViewedAtUtc { get; private set; }

    /// <summary>
    /// Gets the most recent UTC timestamp when the word detail was viewed.
    /// </summary>
    public DateTime? LastViewedAtUtc { get; private set; }

    /// <summary>
    /// Gets the number of recorded word-detail views.
    /// </summary>
    public int ViewCount { get; private set; }

    /// <summary>
    /// Gets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC last update timestamp.
    /// </summary>
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Records a word-detail view for the user.
    /// </summary>
    public void TrackViewed(DateTime viewedAtUtc)
    {
        DateTime normalizedViewedAtUtc = NormalizeUtc(viewedAtUtc, nameof(viewedAtUtc));

        FirstViewedAtUtc ??= normalizedViewedAtUtc;
        LastViewedAtUtc = normalizedViewedAtUtc;
        ViewCount++;
        UpdatedAtUtc = normalizedViewedAtUtc;
    }

    /// <summary>
    /// Marks the word as known.
    /// </summary>
    public void MarkKnown(DateTime updatedAtUtc)
    {
        IsKnown = true;
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    /// <summary>
    /// Clears the known marker from the word.
    /// </summary>
    public void ClearKnown(DateTime updatedAtUtc)
    {
        IsKnown = false;
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    /// <summary>
    /// Marks the word as difficult.
    /// </summary>
    public void MarkDifficult(DateTime updatedAtUtc)
    {
        IsDifficult = true;
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    /// <summary>
    /// Clears the difficult marker from the word.
    /// </summary>
    public void ClearDifficult(DateTime updatedAtUtc)
    {
        IsDifficult = false;
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

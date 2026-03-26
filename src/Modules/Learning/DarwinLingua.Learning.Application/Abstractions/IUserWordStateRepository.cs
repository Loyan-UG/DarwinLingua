using DarwinLingua.Learning.Domain.Entities;

namespace DarwinLingua.Learning.Application.Abstractions;

/// <summary>
/// Provides persistence operations for local user word state.
/// </summary>
public interface IUserWordStateRepository
{
    /// <summary>
    /// Loads the user-word state row for the specified user and lexical entry.
    /// </summary>
    Task<UserWordState?> GetByUserIdAndWordPublicIdAsync(
        string userId,
        Guid wordEntryPublicId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Persists a newly created user-word state row.
    /// </summary>
    Task AddAsync(UserWordState userWordState, CancellationToken cancellationToken);

    /// <summary>
    /// Persists updates made to an existing user-word state row.
    /// </summary>
    Task UpdateAsync(UserWordState userWordState, CancellationToken cancellationToken);
}

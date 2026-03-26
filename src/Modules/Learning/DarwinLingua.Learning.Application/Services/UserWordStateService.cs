using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Learning.Domain.Entities;
using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Learning.Application.Services;

/// <summary>
/// Implements the Phase 1 lightweight user-word-state workflows.
/// </summary>
internal sealed class UserWordStateService : IUserWordStateService
{
    private readonly IUserWordStateRepository _userWordStateRepository;
    private readonly IUserWordStateCatalogReader _userWordStateCatalogReader;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserWordStateService"/> class.
    /// </summary>
    public UserWordStateService(
        IUserWordStateRepository userWordStateRepository,
        IUserWordStateCatalogReader userWordStateCatalogReader)
    {
        ArgumentNullException.ThrowIfNull(userWordStateRepository);
        ArgumentNullException.ThrowIfNull(userWordStateCatalogReader);

        _userWordStateRepository = userWordStateRepository;
        _userWordStateCatalogReader = userWordStateCatalogReader;
    }

    /// <inheritdoc />
    public async Task<UserWordStateModel?> GetWordStateAsync(Guid wordEntryPublicId, CancellationToken cancellationToken)
    {
        ValidateWordPublicId(wordEntryPublicId);

        UserWordState? userWordState = await _userWordStateRepository
            .GetByUserIdAndWordPublicIdAsync(LocalInstallationUser.UserId, wordEntryPublicId, cancellationToken)
            .ConfigureAwait(false);

        return userWordState is null ? null : Map(userWordState);
    }

    /// <inheritdoc />
    public async Task<UserWordStateModel> TrackWordViewedAsync(Guid wordEntryPublicId, CancellationToken cancellationToken)
    {
        UserWordState userWordState = await GetRequiredUserWordStateAsync(wordEntryPublicId, cancellationToken)
            .ConfigureAwait(false);

        userWordState.TrackViewed(DateTime.UtcNow);
        await SaveAsync(userWordState, cancellationToken).ConfigureAwait(false);

        return Map(userWordState);
    }

    /// <inheritdoc />
    public async Task<UserWordStateModel> MarkWordKnownAsync(Guid wordEntryPublicId, CancellationToken cancellationToken)
    {
        UserWordState userWordState = await GetRequiredUserWordStateAsync(wordEntryPublicId, cancellationToken)
            .ConfigureAwait(false);

        userWordState.MarkKnown(DateTime.UtcNow);
        await SaveAsync(userWordState, cancellationToken).ConfigureAwait(false);

        return Map(userWordState);
    }

    /// <inheritdoc />
    public async Task<UserWordStateModel> MarkWordDifficultAsync(Guid wordEntryPublicId, CancellationToken cancellationToken)
    {
        UserWordState userWordState = await GetRequiredUserWordStateAsync(wordEntryPublicId, cancellationToken)
            .ConfigureAwait(false);

        userWordState.MarkDifficult(DateTime.UtcNow);
        await SaveAsync(userWordState, cancellationToken).ConfigureAwait(false);

        return Map(userWordState);
    }

    /// <inheritdoc />
    public async Task<UserWordStateModel> ClearWordKnownStateAsync(Guid wordEntryPublicId, CancellationToken cancellationToken)
    {
        UserWordState userWordState = await GetRequiredUserWordStateAsync(wordEntryPublicId, cancellationToken)
            .ConfigureAwait(false);

        userWordState.ClearKnown(DateTime.UtcNow);
        await SaveAsync(userWordState, cancellationToken).ConfigureAwait(false);

        return Map(userWordState);
    }

    /// <inheritdoc />
    public async Task<UserWordStateModel> ClearWordDifficultStateAsync(Guid wordEntryPublicId, CancellationToken cancellationToken)
    {
        UserWordState userWordState = await GetRequiredUserWordStateAsync(wordEntryPublicId, cancellationToken)
            .ConfigureAwait(false);

        userWordState.ClearDifficult(DateTime.UtcNow);
        await SaveAsync(userWordState, cancellationToken).ConfigureAwait(false);

        return Map(userWordState);
    }

    /// <summary>
    /// Loads an existing user-word-state row or creates a new one when the lexical entry exists.
    /// </summary>
    private async Task<UserWordState> GetRequiredUserWordStateAsync(Guid wordEntryPublicId, CancellationToken cancellationToken)
    {
        ValidateWordPublicId(wordEntryPublicId);

        UserWordState? existingUserWordState = await _userWordStateRepository
            .GetByUserIdAndWordPublicIdAsync(LocalInstallationUser.UserId, wordEntryPublicId, cancellationToken)
            .ConfigureAwait(false);

        if (existingUserWordState is not null)
        {
            return existingUserWordState;
        }

        if (!await _userWordStateCatalogReader.ExistsAsync(wordEntryPublicId, cancellationToken).ConfigureAwait(false))
        {
            throw new DomainRuleException("Only active lexical entries can be tracked in user word state.");
        }

        UserWordState userWordState = new(
            Guid.NewGuid(),
            LocalInstallationUser.UserId,
            wordEntryPublicId,
            DateTime.UtcNow);

        await _userWordStateRepository.AddAsync(userWordState, cancellationToken).ConfigureAwait(false);

        return userWordState;
    }

    /// <summary>
    /// Persists the current user-word-state instance.
    /// </summary>
    private async Task SaveAsync(UserWordState userWordState, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userWordState);

        await _userWordStateRepository.UpdateAsync(userWordState, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Validates the lexical-entry public identifier.
    /// </summary>
    private static void ValidateWordPublicId(Guid wordEntryPublicId)
    {
        if (wordEntryPublicId == Guid.Empty)
        {
            throw new ArgumentException("Word public identifier cannot be empty.", nameof(wordEntryPublicId));
        }
    }

    /// <summary>
    /// Maps the domain entity to a presentation-safe model.
    /// </summary>
    private static UserWordStateModel Map(UserWordState userWordState)
    {
        ArgumentNullException.ThrowIfNull(userWordState);

        return new UserWordStateModel(
            userWordState.WordEntryPublicId,
            userWordState.IsKnown,
            userWordState.IsDifficult,
            userWordState.FirstViewedAtUtc,
            userWordState.LastViewedAtUtc,
            userWordState.ViewCount);
    }
}

using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.DependencyInjection;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Learning.Domain.Entities;
using DarwinLingua.SharedKernel.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Learning.Application.Tests;

/// <summary>
/// Verifies the lightweight user-word-state application workflows.
/// </summary>
public sealed class UserWordStateServiceTests
{
    /// <summary>
    /// Verifies that tracking a viewed word creates state and increments the view counter.
    /// </summary>
    [Fact]
    public async Task TrackWordViewedAsync_ShouldCreateAndIncrementState()
    {
        Guid wordPublicId = Guid.NewGuid();
        InMemoryUserWordStateRepository repository = new();
        FakeUserWordStateCatalogReader catalogReader = new(new HashSet<Guid> { wordPublicId });
        ServiceCollection services = new();
        services.AddLearningApplication();
        services.AddSingleton<IUserWordStateRepository>(repository);
        services.AddSingleton<IUserWordStateCatalogReader>(catalogReader);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IUserWordStateService service = serviceProvider.GetRequiredService<IUserWordStateService>();

        UserWordStateModel firstResult = await service.TrackWordViewedAsync(wordPublicId, CancellationToken.None);
        UserWordStateModel secondResult = await service.TrackWordViewedAsync(wordPublicId, CancellationToken.None);

        Assert.Equal(1, firstResult.ViewCount);
        Assert.Equal(2, secondResult.ViewCount);
        Assert.NotNull(secondResult.FirstViewedAtUtc);
        Assert.NotNull(secondResult.LastViewedAtUtc);
    }

    /// <summary>
    /// Verifies that a word can be marked as known.
    /// </summary>
    [Fact]
    public async Task MarkWordKnownAsync_ShouldPersistKnownState()
    {
        Guid wordPublicId = Guid.NewGuid();
        InMemoryUserWordStateRepository repository = new();
        FakeUserWordStateCatalogReader catalogReader = new(new HashSet<Guid> { wordPublicId });
        ServiceCollection services = new();
        services.AddLearningApplication();
        services.AddSingleton<IUserWordStateRepository>(repository);
        services.AddSingleton<IUserWordStateCatalogReader>(catalogReader);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IUserWordStateService service = serviceProvider.GetRequiredService<IUserWordStateService>();

        UserWordStateModel result = await service.MarkWordKnownAsync(wordPublicId, CancellationToken.None);

        Assert.True(result.IsKnown);
        Assert.False(result.IsDifficult);
    }

    /// <summary>
    /// Verifies that a word can be marked as difficult.
    /// </summary>
    [Fact]
    public async Task MarkWordDifficultAsync_ShouldPersistDifficultState()
    {
        Guid wordPublicId = Guid.NewGuid();
        InMemoryUserWordStateRepository repository = new();
        FakeUserWordStateCatalogReader catalogReader = new(new HashSet<Guid> { wordPublicId });
        ServiceCollection services = new();
        services.AddLearningApplication();
        services.AddSingleton<IUserWordStateRepository>(repository);
        services.AddSingleton<IUserWordStateCatalogReader>(catalogReader);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IUserWordStateService service = serviceProvider.GetRequiredService<IUserWordStateService>();

        UserWordStateModel result = await service.MarkWordDifficultAsync(wordPublicId, CancellationToken.None);

        Assert.False(result.IsKnown);
        Assert.True(result.IsDifficult);
    }

    /// <summary>
    /// Verifies that the known marker can be cleared after it is set.
    /// </summary>
    [Fact]
    public async Task ClearWordKnownStateAsync_ShouldClearKnownMarker()
    {
        Guid wordPublicId = Guid.NewGuid();
        InMemoryUserWordStateRepository repository = new();
        FakeUserWordStateCatalogReader catalogReader = new(new HashSet<Guid> { wordPublicId });
        ServiceCollection services = new();
        services.AddLearningApplication();
        services.AddSingleton<IUserWordStateRepository>(repository);
        services.AddSingleton<IUserWordStateCatalogReader>(catalogReader);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IUserWordStateService service = serviceProvider.GetRequiredService<IUserWordStateService>();

        await service.MarkWordKnownAsync(wordPublicId, CancellationToken.None);
        UserWordStateModel result = await service.ClearWordKnownStateAsync(wordPublicId, CancellationToken.None);

        Assert.False(result.IsKnown);
    }

    /// <summary>
    /// Verifies that the difficult marker can be cleared after it is set.
    /// </summary>
    [Fact]
    public async Task ClearWordDifficultStateAsync_ShouldClearDifficultMarker()
    {
        Guid wordPublicId = Guid.NewGuid();
        InMemoryUserWordStateRepository repository = new();
        FakeUserWordStateCatalogReader catalogReader = new(new HashSet<Guid> { wordPublicId });
        ServiceCollection services = new();
        services.AddLearningApplication();
        services.AddSingleton<IUserWordStateRepository>(repository);
        services.AddSingleton<IUserWordStateCatalogReader>(catalogReader);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IUserWordStateService service = serviceProvider.GetRequiredService<IUserWordStateService>();

        await service.MarkWordDifficultAsync(wordPublicId, CancellationToken.None);
        UserWordStateModel result = await service.ClearWordDifficultStateAsync(wordPublicId, CancellationToken.None);

        Assert.False(result.IsDifficult);
    }

    /// <summary>
    /// Verifies that non-existent words cannot be tracked in user word state.
    /// </summary>
    [Fact]
    public async Task TrackWordViewedAsync_ShouldRejectMissingWords()
    {
        InMemoryUserWordStateRepository repository = new();
        FakeUserWordStateCatalogReader catalogReader = new(new HashSet<Guid>());
        ServiceCollection services = new();
        services.AddLearningApplication();
        services.AddSingleton<IUserWordStateRepository>(repository);
        services.AddSingleton<IUserWordStateCatalogReader>(catalogReader);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IUserWordStateService service = serviceProvider.GetRequiredService<IUserWordStateService>();

        await Assert.ThrowsAsync<DomainRuleException>(() => service.TrackWordViewedAsync(Guid.NewGuid(), CancellationToken.None));
    }

    /// <summary>
    /// Stores user-word-state rows for application tests.
    /// </summary>
    private sealed class InMemoryUserWordStateRepository : IUserWordStateRepository
    {
        private readonly List<UserWordState> _userWordStates = [];

        /// <inheritdoc />
        public Task<UserWordState?> GetByUserIdAndWordPublicIdAsync(
            string userId,
            Guid wordEntryPublicId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_userWordStates.SingleOrDefault(
                userWordState => userWordState.UserId == userId && userWordState.WordEntryPublicId == wordEntryPublicId));
        }

        /// <inheritdoc />
        public Task AddAsync(UserWordState userWordState, CancellationToken cancellationToken)
        {
            _userWordStates.Add(userWordState);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task UpdateAsync(UserWordState userWordState, CancellationToken cancellationToken)
        {
            UserWordState? existingUserWordState = _userWordStates.SingleOrDefault(
                existing => existing.Id == userWordState.Id);

            if (existingUserWordState is null)
            {
                _userWordStates.Add(userWordState);
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Supplies predictable lexical existence rules for user-word-state application tests.
    /// </summary>
    private sealed class FakeUserWordStateCatalogReader(IReadOnlySet<Guid> availableWordPublicIds)
        : IUserWordStateCatalogReader
    {
        /// <inheritdoc />
        public Task<bool> ExistsAsync(Guid wordEntryPublicId, CancellationToken cancellationToken)
        {
            return Task.FromResult(availableWordPublicIds.Contains(wordEntryPublicId));
        }
    }
}

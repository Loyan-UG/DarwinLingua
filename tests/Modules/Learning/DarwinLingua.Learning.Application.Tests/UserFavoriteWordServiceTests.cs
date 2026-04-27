using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.DependencyInjection;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Learning.Domain.Entities;
using DarwinLingua.SharedKernel.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Learning.Application.Tests;

/// <summary>
/// Verifies the favorite-word application workflows.
/// </summary>
public sealed class UserFavoriteWordServiceTests
{
    /// <summary>
    /// Verifies that toggling a non-favorited active word adds it to the user's favorites.
    /// </summary>
    [Fact]
    public async Task ToggleFavoriteAsync_ShouldAddFavoriteWhenWordExists()
    {
        Guid wordPublicId = Guid.NewGuid();
        InMemoryUserFavoriteWordRepository repository = new();
        FakeUserFavoriteWordCatalogReader catalogReader = new(new HashSet<Guid> { wordPublicId });
        ServiceCollection services = new();
        services.AddLearningApplication();
        services.AddSingleton<IUserFavoriteWordRepository>(repository);
        services.AddSingleton<IUserFavoriteWordCatalogReader>(catalogReader);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IUserFavoriteWordService service = serviceProvider.GetRequiredService<IUserFavoriteWordService>();

        bool result = await service.ToggleFavoriteAsync(wordPublicId, CancellationToken.None);

        Assert.True(result);
        Assert.True(await service.IsFavoriteAsync(wordPublicId, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that toggling an already favorited word removes it from the user's favorites.
    /// </summary>
    [Fact]
    public async Task ToggleFavoriteAsync_ShouldRemoveFavoriteWhenWordIsAlreadyFavorited()
    {
        Guid wordPublicId = Guid.NewGuid();
        InMemoryUserFavoriteWordRepository repository = new();
        FakeUserFavoriteWordCatalogReader catalogReader = new(new HashSet<Guid> { wordPublicId });
        ServiceCollection services = new();
        services.AddLearningApplication();
        services.AddSingleton<IUserFavoriteWordRepository>(repository);
        services.AddSingleton<IUserFavoriteWordCatalogReader>(catalogReader);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IUserFavoriteWordService service = serviceProvider.GetRequiredService<IUserFavoriteWordService>();

        await service.ToggleFavoriteAsync(wordPublicId, CancellationToken.None);
        bool result = await service.ToggleFavoriteAsync(wordPublicId, CancellationToken.None);

        Assert.False(result);
        Assert.False(await service.IsFavoriteAsync(wordPublicId, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that non-existent words cannot be favorited.
    /// </summary>
    [Fact]
    public async Task ToggleFavoriteAsync_ShouldRejectMissingWords()
    {
        InMemoryUserFavoriteWordRepository repository = new();
        FakeUserFavoriteWordCatalogReader catalogReader = new(new HashSet<Guid>());
        ServiceCollection services = new();
        services.AddLearningApplication();
        services.AddSingleton<IUserFavoriteWordRepository>(repository);
        services.AddSingleton<IUserFavoriteWordCatalogReader>(catalogReader);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IUserFavoriteWordService service = serviceProvider.GetRequiredService<IUserFavoriteWordService>();

        await Assert.ThrowsAsync<DomainRuleException>(() => service.ToggleFavoriteAsync(Guid.NewGuid(), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that IsFavoriteAsync returns false when the word has not been favorited.
    /// </summary>
    [Fact]
    public async Task IsFavoriteAsync_ShouldReturnFalseWhenWordIsNotFavorited()
    {
        Guid wordPublicId = Guid.NewGuid();
        InMemoryUserFavoriteWordRepository repository = new();
        FakeUserFavoriteWordCatalogReader catalogReader = new(new HashSet<Guid> { wordPublicId });
        ServiceCollection services = new();
        services.AddLearningApplication();
        services.AddSingleton<IUserFavoriteWordRepository>(repository);
        services.AddSingleton<IUserFavoriteWordCatalogReader>(catalogReader);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IUserFavoriteWordService service = serviceProvider.GetRequiredService<IUserFavoriteWordService>();

        bool result = await service.IsFavoriteAsync(wordPublicId, CancellationToken.None);

        Assert.False(result);
    }

    /// <summary>
    /// Verifies that IsFavoriteAsync rejects an empty word public identifier.
    /// </summary>
    [Fact]
    public async Task IsFavoriteAsync_ShouldRejectEmptyWordPublicId()
    {
        InMemoryUserFavoriteWordRepository repository = new();
        FakeUserFavoriteWordCatalogReader catalogReader = new(new HashSet<Guid>());
        ServiceCollection services = new();
        services.AddLearningApplication();
        services.AddSingleton<IUserFavoriteWordRepository>(repository);
        services.AddSingleton<IUserFavoriteWordCatalogReader>(catalogReader);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IUserFavoriteWordService service = serviceProvider.GetRequiredService<IUserFavoriteWordService>();

        await Assert.ThrowsAsync<ArgumentException>(() => service.IsFavoriteAsync(Guid.Empty, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that the favorite query returns the lexical data supplied by the catalog reader.
    /// </summary>
    [Fact]
    public async Task GetFavoriteWordsAsync_ShouldReturnCatalogData()
    {
        Guid wordPublicId = Guid.NewGuid();
        InMemoryUserFavoriteWordRepository repository = new();
        FakeUserFavoriteWordCatalogReader catalogReader = new(new HashSet<Guid> { wordPublicId });
        ServiceCollection services = new();
        services.AddLearningApplication();
        services.AddSingleton<IUserFavoriteWordRepository>(repository);
        services.AddSingleton<IUserFavoriteWordCatalogReader>(catalogReader);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IUserFavoriteWordService service = serviceProvider.GetRequiredService<IUserFavoriteWordService>();

        await service.ToggleFavoriteAsync(wordPublicId, CancellationToken.None);
        IReadOnlyList<FavoriteWordListItemModel> favorites = await service.GetFavoriteWordsAsync("en", CancellationToken.None);

        FavoriteWordListItemModel result = Assert.Single(favorites);
        Assert.Equal(wordPublicId, result.PublicId);
        Assert.Equal("Bahnhof", result.Lemma);
    }

    /// <summary>
    /// Stores favorite-word rows for application tests.
    /// </summary>
    private sealed class InMemoryUserFavoriteWordRepository : IUserFavoriteWordRepository
    {
        private readonly List<UserFavoriteWord> _favoriteWords = [];

        /// <inheritdoc />
        public Task<UserFavoriteWord?> GetByUserIdAndWordPublicIdAsync(
            string userId,
            Guid wordEntryPublicId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_favoriteWords.SingleOrDefault(
                favoriteWord => favoriteWord.UserId == userId && favoriteWord.WordEntryPublicId == wordEntryPublicId));
        }

        /// <inheritdoc />
        public Task AddAsync(UserFavoriteWord favoriteWord, CancellationToken cancellationToken)
        {
            _favoriteWords.Add(favoriteWord);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteAsync(UserFavoriteWord favoriteWord, CancellationToken cancellationToken)
        {
            _favoriteWords.Remove(favoriteWord);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Supplies predictable lexical responses for favorite-word application tests.
    /// </summary>
    private sealed class FakeUserFavoriteWordCatalogReader(IReadOnlySet<Guid> availableWordPublicIds)
        : IUserFavoriteWordCatalogReader
    {
        /// <inheritdoc />
        public Task<bool> ExistsAsync(Guid wordEntryPublicId, CancellationToken cancellationToken)
        {
            return Task.FromResult(availableWordPublicIds.Contains(wordEntryPublicId));
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<FavoriteWordListItemModel>> GetFavoriteWordsAsync(
            string userId,
            string meaningLanguageCode,
            CancellationToken cancellationToken)
        {
            FavoriteWordListItemModel[] favoriteWords = availableWordPublicIds
                .Select(wordPublicId => new FavoriteWordListItemModel(
                    wordPublicId,
                    "Bahnhof",
                    "der",
                    "Noun",
                    "A1",
                    "station"))
                .ToArray();

            return Task.FromResult<IReadOnlyList<FavoriteWordListItemModel>>(favoriteWords);
        }
    }
}

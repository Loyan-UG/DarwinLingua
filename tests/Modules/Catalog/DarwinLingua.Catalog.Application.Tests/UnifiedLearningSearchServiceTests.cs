using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.SharedKernel.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Catalog.Application.Tests;

public sealed class UnifiedLearningSearchServiceTests
{
    [Fact]
    public async Task SearchAsync_ShouldReturnEmpty_ForEmptyQuery()
    {
        FakeUnifiedSearchRepository repository = new();
        await using ServiceProvider serviceProvider = BuildServiceProvider(repository);
        IUnifiedLearningSearchService service = serviceProvider.GetRequiredService<IUnifiedLearningSearchService>();

        IReadOnlyList<UnifiedLearningSearchResultModel> results = await service.SearchAsync(
            new UnifiedLearningSearchFilterModel(" ", null, null, null, null),
            CancellationToken.None);

        Assert.Empty(results);
        Assert.False(repository.WasCalled);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnEmpty_ForOneCharacterQuery()
    {
        FakeUnifiedSearchRepository repository = new();
        await using ServiceProvider serviceProvider = BuildServiceProvider(repository);
        IUnifiedLearningSearchService service = serviceProvider.GetRequiredService<IUnifiedLearningSearchService>();

        IReadOnlyList<UnifiedLearningSearchResultModel> results = await service.SearchAsync(
            new UnifiedLearningSearchFilterModel("a", null, null, null, null),
            CancellationToken.None);

        Assert.Empty(results);
        Assert.False(repository.WasCalled);
    }

    [Fact]
    public async Task SearchAsync_ShouldRejectLongQuery()
    {
        FakeUnifiedSearchRepository repository = new();
        await using ServiceProvider serviceProvider = BuildServiceProvider(repository);
        IUnifiedLearningSearchService service = serviceProvider.GetRequiredService<IUnifiedLearningSearchService>();

        await Assert.ThrowsAsync<DomainRuleException>(() => service.SearchAsync(
            new UnifiedLearningSearchFilterModel(new string('a', 101), null, null, null, null),
            CancellationToken.None));
        Assert.False(repository.WasCalled);
    }

    [Fact]
    public async Task SearchAsync_ShouldRejectUnsupportedResultType()
    {
        FakeUnifiedSearchRepository repository = new();
        await using ServiceProvider serviceProvider = BuildServiceProvider(repository);
        IUnifiedLearningSearchService service = serviceProvider.GetRequiredService<IUnifiedLearningSearchService>();

        await Assert.ThrowsAsync<DomainRuleException>(() => service.SearchAsync(
            new UnifiedLearningSearchFilterModel("word", null, "unsupported", null, null),
            CancellationToken.None));
        Assert.False(repository.WasCalled);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnProjectedResults_FromRepository()
    {
        FakeUnifiedSearchRepository repository = new()
        {
            Results =
            [
                new UnifiedLearningSearchResultModel("grammar", "Word order", "Sentence structure", "A2", "word-order", [], "/grammar/word-order", 500, ["title"])
            ]
        };
        await using ServiceProvider serviceProvider = BuildServiceProvider(repository);
        IUnifiedLearningSearchService service = serviceProvider.GetRequiredService<IUnifiedLearningSearchService>();

        IReadOnlyList<UnifiedLearningSearchResultModel> results = await service.SearchAsync(
            new UnifiedLearningSearchFilterModel(" word ", "A2", "GRAMMAR", " Word-Order ", " Sentence "),
            CancellationToken.None);

        UnifiedLearningSearchResultModel result = Assert.Single(results);
        Assert.Equal("grammar", result.ResultType);
        Assert.Equal("Word order", result.Title);
        Assert.Equal("word", repository.LastFilter?.Query);
        Assert.Equal("a2", repository.LastFilter?.CefrLevel);
        Assert.Equal("grammar", repository.LastFilter?.ResultType);
        Assert.Equal("word-order", repository.LastFilter?.Category);
        Assert.Equal("sentence", repository.LastFilter?.TopicKey);
        Assert.True(repository.WasCalled);
    }

    private sealed class FakeUnifiedSearchRepository : IUnifiedLearningSearchRepository
    {
        public bool WasCalled { get; private set; }

        public UnifiedLearningSearchFilterModel? LastFilter { get; private set; }

        public IReadOnlyList<UnifiedLearningSearchResultModel> Results { get; init; } = [];

        public Task<IReadOnlyList<UnifiedLearningSearchResultModel>> SearchAsync(UnifiedLearningSearchFilterModel filter, CancellationToken cancellationToken)
        {
            WasCalled = true;
            LastFilter = filter;
            return Task.FromResult(Results);
        }
    }

    private static ServiceProvider BuildServiceProvider(IUnifiedLearningSearchRepository repository)
    {
        ServiceCollection services = new();
        services.AddCatalogApplication();
        services.AddSingleton(repository);
        return services.BuildServiceProvider();
    }
}

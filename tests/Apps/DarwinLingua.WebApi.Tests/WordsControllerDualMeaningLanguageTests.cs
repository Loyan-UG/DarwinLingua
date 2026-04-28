using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Web.Controllers;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class WordsControllerDualMeaningLanguageTests
{
    [Fact]
    public async Task Detail_ShouldPassResolvedSecondaryMeaningLanguageToApiAndViewModel()
    {
        Guid wordId = Guid.NewGuid();
        CapturingCatalogApiClient catalogApiClient = new(CreateWordDetail(wordId));
        StaticFeatureAccessService featureAccessService = new("fa");
        WordsController controller = new(
            catalogApiClient,
            new StaticFavoriteWordService(false),
            new StaticWordStateService(wordId),
            new StaticLearningProfileAccessor(new UserLearningProfileModel("local", "en", "fa", "en")),
            featureAccessService)
        {
            Url = new StaticUrlHelper($"/words/{wordId:D}"),
        };

        IActionResult actionResult = await controller.Detail(wordId, CancellationToken.None);

        ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);
        WordDetailPageViewModel viewModel = Assert.IsType<WordDetailPageViewModel>(viewResult.Model);
        Assert.Equal("en", catalogApiClient.PrimaryMeaningLanguageCode);
        Assert.Equal("fa", catalogApiClient.SecondaryMeaningLanguageCode);
        Assert.Equal("fa", viewModel.SecondaryMeaningLanguageCode);
    }

    [Fact]
    public async Task Detail_ShouldOmitSecondaryMeaningLanguage_WhenFeatureIsUnavailable()
    {
        Guid wordId = Guid.NewGuid();
        CapturingCatalogApiClient catalogApiClient = new(CreateWordDetail(wordId));
        WordsController controller = new(
            catalogApiClient,
            new StaticFavoriteWordService(false),
            new StaticWordStateService(wordId),
            new StaticLearningProfileAccessor(new UserLearningProfileModel("local", "en", "fa", "en")),
            new StaticFeatureAccessService(null))
        {
            Url = new StaticUrlHelper($"/words/{wordId:D}"),
        };

        IActionResult actionResult = await controller.Detail(wordId, CancellationToken.None);

        ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);
        WordDetailPageViewModel viewModel = Assert.IsType<WordDetailPageViewModel>(viewResult.Model);
        Assert.Equal("en", catalogApiClient.PrimaryMeaningLanguageCode);
        Assert.Null(catalogApiClient.SecondaryMeaningLanguageCode);
        Assert.Null(viewModel.SecondaryMeaningLanguageCode);
    }

    private static WordDetailModel CreateWordDetail(Guid publicId)
    {
        return new WordDetailModel(
            publicId,
            "Brot",
            "das",
            "Brote",
            null,
            null,
            null,
            "Noun",
            [new WordLexicalFormDetailModel("Noun", "das", "Brote", null, true)],
            "A1",
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            ["shopping"],
            [new WordSenseDetailModel(null, "bread", "نان", [new ExampleSentenceDetailModel("Ich kaufe Brot.", "I buy bread.", "من نان می خرم.")])]);
    }

    private sealed class CapturingCatalogApiClient(WordDetailModel wordDetail) : IWebCatalogApiClient
    {
        public string? PrimaryMeaningLanguageCode { get; private set; }

        public string? SecondaryMeaningLanguageCode { get; private set; }

        public Task<WordDetailModel?> GetWordDetailsAsync(
            Guid publicId,
            string primaryMeaningLanguageCode,
            string? secondaryMeaningLanguageCode,
            string uiLanguageCode,
            CancellationToken cancellationToken)
        {
            PrimaryMeaningLanguageCode = primaryMeaningLanguageCode;
            SecondaryMeaningLanguageCode = secondaryMeaningLanguageCode;
            return Task.FromResult<WordDetailModel?>(wordDetail);
        }

        public Task<IReadOnlyList<TopicListItemModel>> GetTopicsAsync(string uiLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<WordCollectionListItemModel>> GetCollectionsAsync(string meaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<WordCollectionDetailModel?> GetCollectionBySlugAsync(string slug, string meaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<ScenarioLessonListItemModel>> GetScenariosAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<ScenarioLessonDetailModel?> GetScenarioBySlugAsync(string slug, string primaryMeaningLanguageCode, string? secondaryMeaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetConversationStarterPacksAsync(ConversationStarterListFilterModel filter, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetConversationStarterPacksForScenarioAsync(string scenarioSlug, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<ConversationStarterPackDetailModel?> GetConversationStarterPackBySlugAsync(string slug, string primaryMeaningLanguageCode, string? secondaryMeaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<EventPreparationPackListItemModel>> GetEventPreparationPacksForScenarioAsync(string scenarioSlug, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<EventPreparationPackDetailModel?> GetEventPreparationPackBySlugAsync(string slug, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<WordListItemModel>> GetWordsByTopicPageAsync(string topicKey, string meaningLanguageCode, int skip, int take, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<WordListItemModel>> GetWordsByCefrPageAsync(string cefrLevel, string meaningLanguageCode, int skip, int take, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<WordListItemModel>> SearchWordsAsync(string query, string meaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<WordListItemModel>> GetWordsByIdsAsync(IReadOnlyList<Guid> wordIds, string meaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<AdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<AdminImportsPageViewModel> GetAdminImportsAsync(string? statusFilter, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<AdminDraftWordsPageViewModel> GetAdminDraftWordsAsync(string? query, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<AdminHistoryPageViewModel> GetAdminHistoryAsync(string? statusFilter, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<AdminRollbackPageViewModel> GetAdminRollbackPreviewAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class StaticLearningProfileAccessor(UserLearningProfileModel profile) : IWebLearningProfileAccessor
    {
        public Task<UserLearningProfileModel> GetProfileAsync(CancellationToken cancellationToken) => Task.FromResult(profile);
    }

    private sealed class StaticFeatureAccessService(string? resolvedSecondaryLanguage) : IWebEntitledFeatureAccessService
    {
        public Task<bool> CanUseFavoritesAsync(CancellationToken cancellationToken) => Task.FromResult(true);

        public Task EnsureCanUseFavoritesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<bool> CanUseDualMeaningLanguageAsync(CancellationToken cancellationToken) => Task.FromResult(resolvedSecondaryLanguage is not null);

        public Task<bool> CanUseEventPreparationPacksAsync(CancellationToken cancellationToken) => Task.FromResult(true);

        public Task EnsureCanUseEventPreparationPacksAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<string?> ResolveSecondaryMeaningLanguageAsync(string? requestedSecondaryMeaningLanguageCode, CancellationToken cancellationToken) =>
            Task.FromResult(resolvedSecondaryLanguage);
    }

    private sealed class StaticFavoriteWordService(bool isFavorite) : IWebFavoriteWordService
    {
        public Task<bool> IsFavoriteAsync(Guid wordPublicId, CancellationToken cancellationToken) => Task.FromResult(isFavorite);

        public Task ToggleFavoriteAsync(Guid wordPublicId, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IReadOnlyList<FavoriteWordListItemModel>> GetFavoriteWordsAsync(string meaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class StaticWordStateService(Guid wordId) : IWebUserWordStateService
    {
        private UserWordStateModel State => new(wordId, false, false, DateTime.UtcNow, DateTime.UtcNow, 1);

        public Task<UserWordStateModel?> GetWordStateAsync(Guid wordPublicId, CancellationToken cancellationToken) => Task.FromResult<UserWordStateModel?>(State);

        public Task<UserWordStateModel> TrackWordViewedAsync(Guid wordPublicId, CancellationToken cancellationToken) => Task.FromResult(State);

        public Task<UserWordStateModel> MarkWordKnownAsync(Guid wordPublicId, CancellationToken cancellationToken) => Task.FromResult(State);

        public Task<UserWordStateModel> MarkWordDifficultAsync(Guid wordPublicId, CancellationToken cancellationToken) => Task.FromResult(State);

        public Task<UserWordStateModel> ClearWordKnownStateAsync(Guid wordPublicId, CancellationToken cancellationToken) => Task.FromResult(State);

        public Task<UserWordStateModel> ClearWordDifficultStateAsync(Guid wordPublicId, CancellationToken cancellationToken) => Task.FromResult(State);
    }

    private sealed class StaticUrlHelper(string detailUrl) : IUrlHelper
    {
        public ActionContext ActionContext => new();

        public string? Action(UrlActionContext actionContext) => detailUrl;

        public string? Content(string? contentPath) => contentPath;

        public bool IsLocalUrl(string? url) => true;

        public string? Link(string? routeName, object? values) => detailUrl;

        public string? RouteUrl(UrlRouteContext routeContext) => detailUrl;
    }
}

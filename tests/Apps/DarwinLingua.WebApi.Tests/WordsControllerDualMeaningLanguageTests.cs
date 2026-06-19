using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Identity;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Web.Controllers;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class WordsControllerDualMeaningLanguageTests
{
    [Fact]
    public async Task Detail_ShouldPassResolvedSecondaryMeaningLanguageToApiAndViewModel()
    {
        CapturingCatalogApiClient catalogApiClient = new(CreateWordDetail(Guid.NewGuid()));
        StaticFeatureAccessService featureAccessService = new("fa");
        WordsController controller = new(
            catalogApiClient,
            new StaticFavoriteWordService(false),
            new StaticWordStateService(catalogApiClient.WordPublicId),
            new StaticLearningProfileAccessor(new UserLearningProfileModel("local", "en", "fa", "en")),
            featureAccessService,
            new TestStringLocalizer(),
            NullLogger<WordsController>.Instance)
        {
            Url = new StaticUrlHelper("/words/brot"),
        };

        IActionResult actionResult = await controller.Detail("brot", CancellationToken.None);

        ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);
        WordDetailPageViewModel viewModel = Assert.IsType<WordDetailPageViewModel>(viewResult.Model);
        Assert.Equal("en", catalogApiClient.PrimaryMeaningLanguageCode);
        Assert.Equal("fa", catalogApiClient.SecondaryMeaningLanguageCode);
        Assert.Equal("fa", viewModel.SecondaryMeaningLanguageCode);
    }

    [Fact]
    public async Task Detail_ShouldOmitSecondaryMeaningLanguage_WhenFeatureIsUnavailable()
    {
        CapturingCatalogApiClient catalogApiClient = new(CreateWordDetail(Guid.NewGuid()));
        WordsController controller = new(
            catalogApiClient,
            new StaticFavoriteWordService(false),
            new StaticWordStateService(catalogApiClient.WordPublicId),
            new StaticLearningProfileAccessor(new UserLearningProfileModel("local", "en", "fa", "en")),
            new StaticFeatureAccessService(null),
            new TestStringLocalizer(),
            NullLogger<WordsController>.Instance)
        {
            Url = new StaticUrlHelper("/words/brot"),
        };

        IActionResult actionResult = await controller.Detail("brot", CancellationToken.None);

        ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);
        WordDetailPageViewModel viewModel = Assert.IsType<WordDetailPageViewModel>(viewResult.Model);
        Assert.Equal("en", catalogApiClient.PrimaryMeaningLanguageCode);
        Assert.Null(catalogApiClient.SecondaryMeaningLanguageCode);
        Assert.Null(viewModel.SecondaryMeaningLanguageCode);
    }

    [Fact]
    public async Task ToggleFavorite_ShouldToggleAndRedirectToSafeReturnUrl()
    {
        Guid wordId = Guid.NewGuid();
        StaticFavoriteWordService favoriteWordService = new(false);
        WordsController controller = new(
            new CapturingCatalogApiClient(CreateWordDetail(wordId)),
            favoriteWordService,
            new StaticWordStateService(wordId),
            new StaticLearningProfileAccessor(new UserLearningProfileModel("local", "en", "fa", "en")),
            new StaticFeatureAccessService("fa"),
            new TestStringLocalizer(),
            NullLogger<WordsController>.Instance)
        {
            Url = new StaticUrlHelper("/words/brot"),
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        IActionResult actionResult = await controller.ToggleFavorite(wordId, "/words/brot", CancellationToken.None);

        LocalRedirectResult redirect = Assert.IsType<LocalRedirectResult>(actionResult);
        Assert.Equal("/words/brot", redirect.Url);
        Assert.Equal(1, favoriteWordService.ToggleCount);
    }

    [Fact]
    public async Task ToggleFavorite_ShouldRedirectToFavorites_WhenFeatureIsLocked()
    {
        Guid wordId = Guid.NewGuid();
        StaticFavoriteWordService favoriteWordService = new(false, throwFeatureDeniedOnToggle: true);
        WordsController controller = new(
            new CapturingCatalogApiClient(CreateWordDetail(wordId)),
            favoriteWordService,
            new StaticWordStateService(wordId),
            new StaticLearningProfileAccessor(new UserLearningProfileModel("local", "en", "fa", "en")),
            new StaticFeatureAccessService(null, canUseFavorites: false),
            new TestStringLocalizer(),
            NullLogger<WordsController>.Instance)
        {
            Url = new StaticUrlHelper("/words/brot"),
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        IActionResult actionResult = await controller.ToggleFavorite(wordId, "/words/brot", CancellationToken.None);

        RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(actionResult);
        Assert.Equal("Favorites", redirect.ControllerName);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal(1, favoriteWordService.ToggleCount);
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

    private sealed class CapturingCatalogApiClient(WordDetailModel wordDetail) : UnsupportedWebCatalogApiClient
    {
        public Guid WordPublicId => wordDetail.PublicId;

        public string? PrimaryMeaningLanguageCode { get; private set; }

        public string? SecondaryMeaningLanguageCode { get; private set; }

        public override Task<WordDetailModel?> GetWordDetailsBySlugAsync(
            string slug,
            string primaryMeaningLanguageCode,
            string? secondaryMeaningLanguageCode,
            string uiLanguageCode,
            CancellationToken cancellationToken)
        {
            PrimaryMeaningLanguageCode = primaryMeaningLanguageCode;
            SecondaryMeaningLanguageCode = secondaryMeaningLanguageCode;
            return Task.FromResult<WordDetailModel?>(wordDetail);
        }
    }

    private sealed class StaticLearningProfileAccessor(UserLearningProfileModel profile) : IWebLearningProfileAccessor
    {
        public Task<UserLearningProfileModel> GetProfileAsync(CancellationToken cancellationToken) => Task.FromResult(profile);
    }

    private sealed class StaticFeatureAccessService(string? resolvedSecondaryLanguage, bool canUseFavorites = true) : IWebEntitledFeatureAccessService
    {
        public Task<bool> CanUseFavoritesAsync(CancellationToken cancellationToken) => Task.FromResult(canUseFavorites);

        public Task EnsureCanUseFavoritesAsync(CancellationToken cancellationToken)
        {
            if (canUseFavorites)
            {
                return Task.CompletedTask;
            }

            throw new FeatureAccessDeniedException(DarwinLinguaFeatureKeys.Favorites, "Favorites are locked.");
        }

        public Task<bool> CanUseDualMeaningLanguageAsync(CancellationToken cancellationToken) => Task.FromResult(resolvedSecondaryLanguage is not null);

        public Task<bool> CanUseEventPreparationPacksAsync(CancellationToken cancellationToken) => Task.FromResult(true);

        public Task EnsureCanUseEventPreparationPacksAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<string?> ResolveSecondaryMeaningLanguageAsync(string? requestedSecondaryMeaningLanguageCode, CancellationToken cancellationToken) =>
            Task.FromResult(resolvedSecondaryLanguage);
    }

    private sealed class StaticFavoriteWordService(bool isFavorite, bool throwFeatureDeniedOnToggle = false) : IWebFavoriteWordService
    {
        public int ToggleCount { get; private set; }

        public Task<bool> IsFavoriteAsync(Guid wordPublicId, CancellationToken cancellationToken) => Task.FromResult(isFavorite);

        public Task ToggleFavoriteAsync(Guid wordPublicId, CancellationToken cancellationToken)
        {
            ToggleCount++;
            if (throwFeatureDeniedOnToggle)
            {
                throw new FeatureAccessDeniedException(DarwinLinguaFeatureKeys.Favorites, "Favorites are locked.");
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlySet<Guid>> GetFavoriteWordIdsAsync(IReadOnlyCollection<Guid> wordPublicIds, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlySet<Guid>>(isFavorite ? wordPublicIds.ToHashSet() : new HashSet<Guid>());

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

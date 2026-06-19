using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Controllers;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class EventPreparationPacksControllerFeatureGateTests
{
    [Fact]
    public async Task Detail_ShouldForbid_WhenFeatureIsUnavailable()
    {
        EventPreparationPacksController controller = new(
            new StaticCatalogApiClient(),
            new StaticFeatureAccessService(canUseEventPreparationPacks: false),
            NullLogger<EventPreparationPacksController>.Instance,
            new TestStringLocalizer());

        IActionResult actionResult = await controller.Detail("a1-cafe-first-meeting-prep", CancellationToken.None);

        Assert.IsType<ForbidResult>(actionResult);
    }

    [Fact]
    public async Task Detail_ShouldReturnView_WhenFeatureIsAvailable()
    {
        StaticCatalogApiClient catalogApiClient = new(CreatePreparationPack());
        EventPreparationPacksController controller = new(
            catalogApiClient,
            new StaticFeatureAccessService(canUseEventPreparationPacks: true),
            NullLogger<EventPreparationPacksController>.Instance,
            new TestStringLocalizer())
        {
            ControllerContext = CreateAuthenticatedControllerContext()
        };

        IActionResult actionResult = await controller.Detail("a1-cafe-first-meeting-prep", CancellationToken.None);

        ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);
        EventPreparationDetailPageViewModel viewModel = Assert.IsType<EventPreparationDetailPageViewModel>(viewResult.Model);
        Assert.Equal("a1-cafe-first-meeting-prep", catalogApiClient.RequestedSlug);
        Assert.Equal("learner@example.test", catalogApiClient.RequestedActorEmail);
        Assert.Equal("Cafe first meeting prep", viewModel.PreparationPack.Title);
    }

    [Fact]
    public async Task Complete_ShouldRecordCompletionAndRedirectToDetail()
    {
        StaticCatalogApiClient catalogApiClient = new(CreatePreparationPack());
        CapturingAnalyticsService analyticsService = new();
        EventPreparationPacksController controller = new(
            catalogApiClient,
            new StaticFeatureAccessService(canUseEventPreparationPacks: true),
            NullLogger<EventPreparationPacksController>.Instance,
            new TestStringLocalizer(),
            analyticsService)
        {
            ControllerContext = CreateAuthenticatedControllerContext(),
            TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                new TestTempDataProvider())
        };

        IActionResult actionResult = await controller.Complete("a1-cafe-first-meeting-prep", CancellationToken.None);

        RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(actionResult);
        Assert.Equal(nameof(EventPreparationPacksController.Detail), redirect.ActionName);
        Assert.Equal("a1-cafe-first-meeting-prep", redirect.RouteValues?["slug"]);
        Assert.Equal("a1-cafe-first-meeting-prep", controller.TempData["PreparationPackCompleted"]);
        Assert.Contains(analyticsService.Events, item =>
            item.EventName == WebProductAnalyticsEvents.EventPreparationPackCompleted &&
            item.ScopeKey == "pack:a1-cafe-first-meeting-prep");
    }

    [Fact]
    public async Task Complete_ShouldForbid_WhenFeatureIsUnavailable()
    {
        CapturingAnalyticsService analyticsService = new();
        EventPreparationPacksController controller = new(
            new StaticCatalogApiClient(CreatePreparationPack()),
            new StaticFeatureAccessService(canUseEventPreparationPacks: false),
            NullLogger<EventPreparationPacksController>.Instance,
            new TestStringLocalizer(),
            analyticsService);

        IActionResult actionResult = await controller.Complete("a1-cafe-first-meeting-prep", CancellationToken.None);

        Assert.IsType<ForbidResult>(actionResult);
        Assert.Contains(analyticsService.Events, item =>
            item.EventName == WebProductAnalyticsEvents.PremiumFeatureDenied &&
            item.ScopeKey == "feature:event-preparation-packs");
    }

    [Fact]
    public async Task Detail_ShouldReturnNotFound_WhenPackIsUnknown()
    {
        EventPreparationPacksController controller = new(
            new StaticCatalogApiClient(null),
            new StaticFeatureAccessService(canUseEventPreparationPacks: true),
            NullLogger<EventPreparationPacksController>.Instance,
            new TestStringLocalizer())
        {
            ControllerContext = CreateAuthenticatedControllerContext()
        };

        IActionResult actionResult = await controller.Detail("missing-prep-pack", CancellationToken.None);

        Assert.IsType<NotFoundResult>(actionResult);
    }

    private static EventPreparationPackDetailModel CreatePreparationPack() =>
        new(
            "a1-cafe-first-meeting-prep",
            "Cafe first meeting prep",
            "Prepare for a first cafe meeting.",
            "A1",
            "social",
            "conversation-cafe",
            ["social"],
            ["a1-cafe-first-meeting"],
            ["a1-cafe-first-meeting-starters"],
            [new EventPreparationVocabularyReferenceModel("Hallo", null, "A1")],
            [new EventPreparationPromptModel("opening", "Say hello and introduce yourself.")]);

    private static ControllerContext CreateAuthenticatedControllerContext()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(
            [new Claim(ClaimTypes.Email, "learner@example.test")],
            authenticationType: "Test"));

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            }
        };
    }

    private sealed class StaticCatalogApiClient(EventPreparationPackDetailModel? preparationPack = null) : UnsupportedWebCatalogApiClient
    {
        public string? RequestedSlug { get; private set; }
        public string? RequestedActorEmail { get; private set; }

        public override Task<EventPreparationPackDetailModel?> GetEventPreparationPackBySlugAsync(string slug, string actorEmail, CancellationToken cancellationToken)
        {
            RequestedSlug = slug;
            RequestedActorEmail = actorEmail;
            return Task.FromResult(preparationPack);
        }
    }

    private sealed class StaticFeatureAccessService(bool canUseEventPreparationPacks) : IWebEntitledFeatureAccessService
    {
        public Task<bool> CanUseFavoritesAsync(CancellationToken cancellationToken) => Task.FromResult(true);

        public Task EnsureCanUseFavoritesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<bool> CanUseDualMeaningLanguageAsync(CancellationToken cancellationToken) => Task.FromResult(true);

        public Task<bool> CanUseEventPreparationPacksAsync(CancellationToken cancellationToken) => Task.FromResult(canUseEventPreparationPacks);

        public Task EnsureCanUseEventPreparationPacksAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<string?> ResolveSecondaryMeaningLanguageAsync(string? requestedSecondaryMeaningLanguageCode, CancellationToken cancellationToken) =>
            Task.FromResult(requestedSecondaryMeaningLanguageCode);
    }

    private sealed class CapturingAnalyticsService : IWebProductAnalyticsService
    {
        private readonly List<WebProductAnalyticsSummaryItem> events = [];

        public IReadOnlyList<WebProductAnalyticsSummaryItem> Events => events;

        public void Record(string eventName, string? scopeKey = null, int count = 1) =>
            events.Add(new WebProductAnalyticsSummaryItem(
                eventName,
                scopeKey ?? "all",
                count,
                DateTime.UtcNow,
                DateTime.UtcNow));

        public IReadOnlyList<WebProductAnalyticsSummaryItem> GetSummary() => events;
    }

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}

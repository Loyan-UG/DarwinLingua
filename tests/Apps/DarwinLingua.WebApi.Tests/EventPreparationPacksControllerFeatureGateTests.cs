using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Controllers;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
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
            new TestStringLocalizer());

        IActionResult actionResult = await controller.Detail("a1-cafe-first-meeting-prep", CancellationToken.None);

        ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);
        EventPreparationDetailPageViewModel viewModel = Assert.IsType<EventPreparationDetailPageViewModel>(viewResult.Model);
        Assert.Equal("a1-cafe-first-meeting-prep", catalogApiClient.RequestedSlug);
        Assert.Equal("Cafe first meeting prep", viewModel.PreparationPack.Title);
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

    private sealed class StaticCatalogApiClient(EventPreparationPackDetailModel? preparationPack = null) : UnsupportedWebCatalogApiClient
    {
        public string? RequestedSlug { get; private set; }

        public override Task<EventPreparationPackDetailModel?> GetEventPreparationPackBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            RequestedSlug = slug;
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
}

using System.Security.Claims;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.Web.Controllers;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class OrganizerDashboardControllerTests
{
    [Fact]
    public async Task EditProfile_ShouldForbid_WhenAuthenticatedUserDoesNotOwnProfile()
    {
        StaticOrganizerCatalogApiClient catalogApiClient = new(
            [CreateOwner("owned-club", "owner@example.com")],
            profiles: [CreateProfile("other-club")],
            eventsByProfile: new Dictionary<string, IReadOnlyList<OrganizerManagedConversationEventModel>>(StringComparer.OrdinalIgnoreCase));
        OrganizerDashboardController controller = CreateController(catalogApiClient, "owner@example.com");

        IActionResult actionResult = await controller.EditProfile("other-club", CancellationToken.None);

        Assert.IsType<ForbidResult>(actionResult);
    }

    [Fact]
    public async Task EditEvent_ShouldReturnNotFound_WhenEventDoesNotBelongToOwnedProfile()
    {
        StaticOrganizerCatalogApiClient catalogApiClient = new(
            [CreateOwner("owned-club", "owner@example.com")],
            profiles: [CreateProfile("owned-club")],
            eventsByProfile: new Dictionary<string, IReadOnlyList<OrganizerManagedConversationEventModel>>(StringComparer.OrdinalIgnoreCase)
            {
                ["owned-club"] = [CreateManagedEvent("owned-event", "owned-club", "Active")],
                ["other-club"] = [CreateManagedEvent("other-event", "other-club", "Active")],
            });
        OrganizerDashboardController controller = CreateController(catalogApiClient, "owner@example.com");

        IActionResult actionResult = await controller.EditEvent("owned-club", "other-event", CancellationToken.None);

        Assert.IsType<NotFoundResult>(actionResult);
    }

    [Fact]
    public async Task NewEvent_ShouldRedirectAndExplain_WhenOrganizerPlanActiveEventLimitIsReached()
    {
        StaticOrganizerCatalogApiClient catalogApiClient = new(
            [CreateOwner("free-club", "owner@example.com")],
            profiles: [CreateProfile("free-club", planKey: "free")],
            eventsByProfile: new Dictionary<string, IReadOnlyList<OrganizerManagedConversationEventModel>>(StringComparer.OrdinalIgnoreCase)
            {
                ["free-club"] = [CreateManagedEvent("active-event", "free-club", "Active")],
            });
        OrganizerDashboardController controller = CreateController(catalogApiClient, "owner@example.com");

        IActionResult actionResult = await controller.NewEvent("free-club", CancellationToken.None);

        RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(actionResult);
        Assert.Equal(nameof(OrganizerDashboardController.Index), redirect.ActionName);
        Assert.Contains("allows 1 active event", controller.TempData["ErrorMessage"]?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task NewEvent_ShouldReturnEditView_WhenOrganizerOwnsProfileAndPlanAllowsMoreEvents()
    {
        StaticOrganizerCatalogApiClient catalogApiClient = new(
            [CreateOwner("lite-club", "owner@example.com")],
            profiles: [CreateProfile("lite-club", planKey: "lite")],
            eventsByProfile: new Dictionary<string, IReadOnlyList<OrganizerManagedConversationEventModel>>(StringComparer.OrdinalIgnoreCase)
            {
                ["lite-club"] = [CreateManagedEvent("active-event", "lite-club", "Active")],
            });
        OrganizerDashboardController controller = CreateController(catalogApiClient, "owner@example.com");

        IActionResult actionResult = await controller.NewEvent("lite-club", CancellationToken.None);

        ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);
        Assert.Equal("EditEvent", viewResult.ViewName);
        OrganizerEventEditPageViewModel viewModel = Assert.IsType<OrganizerEventEditPageViewModel>(viewResult.Model);
        Assert.True(viewModel.IsNew);
        Assert.Equal("lite-club", viewModel.Profile.Slug);
    }

    private static OrganizerDashboardController CreateController(
        StaticOrganizerCatalogApiClient catalogApiClient,
        string userEmail)
    {
        DefaultHttpContext httpContext = new()
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.Email, userEmail)],
                authenticationType: "TestAuth"))
        };

        return new OrganizerDashboardController(
            catalogApiClient,
            new TestStringLocalizer(),
            NullLogger<OrganizerDashboardController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            },
            TempData = new TempDataDictionary(httpContext, new TestTempDataProvider()),
        };
    }

    private static OrganizerProfileOwnerModel CreateOwner(string organizerProfileSlug, string ownerEmail) =>
        new(Guid.NewGuid(), organizerProfileSlug, ownerEmail, "admin@example.com", DateTime.UtcNow);

    private static OrganizerProfileDetailModel CreateProfile(string slug, string planKey = "free") =>
        new(
            slug,
            ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
            $"Organizer {slug}",
            "club",
            "A reviewed organizer profile.",
            "Berlin",
            false,
            ["A1", "A2"],
            ["en", "fa"],
            "https://example.local",
            "events@example.local",
            "reviewed",
            planKey,
            0,
            []);

    private static OrganizerManagedConversationEventModel CreateManagedEvent(
        string slug,
        string organizerProfileSlug,
        string publicationStatus) =>
        new(
            slug,
            ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
            $"Event {slug}",
            "A reviewed conversation event.",
            "Berlin",
            "DE-BE",
            "Central area",
            false,
            "conversation-cafe",
            ["A1"],
            ["en"],
            $"Organizer {organizerProfileSlug}",
            organizerProfileSlug,
            "https://example.local/events",
            "events@example.local",
            "Every Tuesday evening",
            null,
            null,
            "free",
            "reviewed",
            publicationStatus,
            "manual",
            "https://example.local/source",
            DateTime.UtcNow,
            [])
        {
            Capacity = 12,
        };

    private sealed class StaticOrganizerCatalogApiClient(
        IReadOnlyList<OrganizerProfileOwnerModel> ownerships,
        IReadOnlyList<OrganizerProfileDetailModel> profiles,
        IReadOnlyDictionary<string, IReadOnlyList<OrganizerManagedConversationEventModel>> eventsByProfile) : UnsupportedWebCatalogApiClient
    {
        public override Task<IReadOnlyList<OrganizerProfileOwnerModel>> GetOrganizerProfileOwnersByEmailAsync(
            string ownerEmail,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<OrganizerProfileOwnerModel>>(
                ownerships
                    .Where(ownership => string.Equals(ownership.OwnerEmail, ownerEmail, StringComparison.OrdinalIgnoreCase))
                    .ToArray());

        public override Task<OrganizerProfileDetailModel?> GetOrganizerProfileBySlugAsync(
            string slug,
            string targetLearningLanguageCode,
            CancellationToken cancellationToken) =>
            Task.FromResult(profiles.FirstOrDefault(profile => string.Equals(profile.Slug, slug, StringComparison.OrdinalIgnoreCase)));

        public override Task<IReadOnlyList<OrganizerManagedConversationEventModel>> GetAdminConversationEventsByOrganizerAsync(
            string organizerProfileSlug,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                eventsByProfile.TryGetValue(organizerProfileSlug, out IReadOnlyList<OrganizerManagedConversationEventModel>? events)
                    ? events
                    : []);
    }

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}

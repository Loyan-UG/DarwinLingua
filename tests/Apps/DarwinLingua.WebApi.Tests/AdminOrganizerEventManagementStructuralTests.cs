using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class AdminOrganizerEventManagementStructuralTests
{
    [Fact]
    public void AdminOrganizerProfiles_ShouldExposeReviewedProfileOwnerAndClaimManagement()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string controller = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Controllers/OrganizerProfilesController.cs");
        string view = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/OrganizerProfiles/Index.cshtml");
        string viewModel = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Models/AdminOrganizerProfilesPageViewModel.cs");
        string webClient = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Services/WebCatalogApiClient.cs");
        string apiProgram = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.WebApi/Program.cs");

        Assert.Contains("[Route(\"admin/organizer-profiles\")]", controller, StringComparison.Ordinal);
        Assert.Contains("[Authorize(Policy = \"Operator\")]", controller, StringComparison.Ordinal);
        Assert.Contains("[HttpPost(\"\", Name = \"Admin_OrganizerProfiles_Save\")]", controller, StringComparison.Ordinal);
        Assert.Contains("[HttpPost(\"owners\", Name = \"Admin_OrganizerProfiles_AssignOwner\")]", controller, StringComparison.Ordinal);
        Assert.Contains("[HttpPost(\"claims/{claimRequestId:guid}/status\", Name = \"Admin_OrganizerProfiles_SetClaimStatus\")]", controller, StringComparison.Ordinal);
        Assert.Contains("IsAllowedOrganizerVerificationStatus", controller, StringComparison.Ordinal);
        Assert.Contains("IsAllowedOrganizerPlan", controller, StringComparison.Ordinal);
        Assert.Contains("HasAllowedCefrLevels", controller, StringComparison.Ordinal);
        Assert.Contains("HasAllowedLanguageCodes", controller, StringComparison.Ordinal);
        Assert.Contains("AssignAdminOrganizerProfileOwnerAsync", controller, StringComparison.Ordinal);
        Assert.Contains("SendOrganizerProfileOwnershipChangedAsync", controller, StringComparison.Ordinal);
        Assert.Contains("SetAdminOrganizerClaimRequestStatusAsync", controller, StringComparison.Ordinal);
        Assert.Contains("SendOrganizerClaimDecisionAsync", controller, StringComparison.Ordinal);

        Assert.Contains("[RegularExpression(\"^[a-z0-9]+(?:-[a-z0-9]+)*$\")]", viewModel, StringComparison.Ordinal);
        Assert.Contains("[EmailAddress]", viewModel, StringComparison.Ordinal);
        Assert.Contains("[Range(0, 100000)]", viewModel, StringComparison.Ordinal);

        Assert.Contains("id=\"organizer-edit\"", view, StringComparison.Ordinal);
        Assert.Contains("id=\"organizer-owner-assign\"", view, StringComparison.Ordinal);
        Assert.Contains("id=\"organizer-claims\"", view, StringComparison.Ordinal);
        Assert.Contains("id=\"organizer-profiles-list\"", view, StringComparison.Ordinal);
        Assert.Contains("data-confirm-submit=\"@T[\"Save this public organizer profile?\"]\"", view, StringComparison.Ordinal);
        Assert.Contains("data-confirm-submit=\"@T[\"Assign this owner email to the organizer profile?\"]\"", view, StringComparison.Ordinal);
        Assert.Contains("Approve", view, StringComparison.Ordinal);
        Assert.Contains("Reject", view, StringComparison.Ordinal);

        Assert.Contains("SaveAdminOrganizerProfileAsync", webClient, StringComparison.Ordinal);
        Assert.Contains("GetAdminOrganizerClaimRequestsAsync", webClient, StringComparison.Ordinal);
        Assert.Contains("SetAdminOrganizerClaimRequestStatusAsync", webClient, StringComparison.Ordinal);
        Assert.Contains("GetAdminOrganizerProfileOwnersAsync", webClient, StringComparison.Ordinal);
        Assert.Contains("AssignAdminOrganizerProfileOwnerAsync", webClient, StringComparison.Ordinal);

        Assert.Contains("\"/api/admin/catalog/organizer-profiles\"", apiProgram, StringComparison.Ordinal);
        Assert.Contains("\"/api/admin/catalog/organizer-claim-requests\"", apiProgram, StringComparison.Ordinal);
        Assert.Contains("\"/api/admin/catalog/organizer-claim-requests/{claimRequestId:guid}/status\"", apiProgram, StringComparison.Ordinal);
        Assert.Contains("\"/api/admin/catalog/organizer-profile-owners\"", apiProgram, StringComparison.Ordinal);
        Assert.Contains("IOrganizerProfileAdminService", apiProgram, StringComparison.Ordinal);
        Assert.Contains("IOrganizerClaimRequestService", apiProgram, StringComparison.Ordinal);
        Assert.Contains("IOrganizerProfileOwnerService", apiProgram, StringComparison.Ordinal);
    }

    [Fact]
    public void AdminConversationEvents_ShouldExposeReviewedEventListingRsvpAndPublicationManagement()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string controller = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Controllers/ConversationEventsController.cs");
        string view = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/ConversationEvents/Index.cshtml");
        string viewModel = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Models/AdminConversationEventsPageViewModel.cs");
        string webClient = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Services/WebCatalogApiClient.cs");
        string apiProgram = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.WebApi/Program.cs");

        Assert.Contains("[Route(\"admin/conversation-events\")]", controller, StringComparison.Ordinal);
        Assert.Contains("[Authorize(Policy = \"Operator\")]", controller, StringComparison.Ordinal);
        Assert.Contains("[HttpPost(\"\", Name = \"Admin_ConversationEvents_Save\")]", controller, StringComparison.Ordinal);
        Assert.Contains("IsAllowedVerificationStatus", controller, StringComparison.Ordinal);
        Assert.Contains("IsAllowedPriceType", controller, StringComparison.Ordinal);
        Assert.Contains("TryParseOptionalUtc(input.StartsAtUtc", controller, StringComparison.Ordinal);
        Assert.Contains("TryParseOptionalUtc(input.EndsAtUtc", controller, StringComparison.Ordinal);
        Assert.Contains("HasAllowedSlugs(linkedPreparationPackSlugs)", controller, StringComparison.Ordinal);
        Assert.Contains("RecurrenceRule = TrimToNull(input.RecurrenceRule)", controller, StringComparison.Ordinal);
        Assert.Contains("Capacity = input.Capacity", controller, StringComparison.Ordinal);
        Assert.Contains("SaveAdminConversationEventAsync", controller, StringComparison.Ordinal);

        Assert.Contains("[RegularExpression(\"^[a-z0-9]+(?:-[a-z0-9]+)*$\")]", viewModel, StringComparison.Ordinal);
        Assert.Contains("[Range(1, 100000)]", viewModel, StringComparison.Ordinal);
        Assert.Contains("[Url]", viewModel, StringComparison.Ordinal);

        Assert.Contains("id=\"conversation-event-edit\"", view, StringComparison.Ordinal);
        Assert.Contains("id=\"conversation-events-list\"", view, StringComparison.Ordinal);
        Assert.Contains("data-confirm-submit=\"Save this public conversation event listing?\"", view, StringComparison.Ordinal);
        Assert.Contains("name=\"StartsAtUtc\"", view, StringComparison.Ordinal);
        Assert.Contains("name=\"EndsAtUtc\"", view, StringComparison.Ordinal);
        Assert.Contains("name=\"RecurrenceRule\"", view, StringComparison.Ordinal);
        Assert.Contains("name=\"Capacity\"", view, StringComparison.Ordinal);
        Assert.Contains("name=\"LinkedEventPreparationPackSlugs\"", view, StringComparison.Ordinal);

        Assert.Contains("SaveAdminConversationEventAsync", webClient, StringComparison.Ordinal);
        Assert.Contains("GetAdminConversationEventsByOrganizerAsync", webClient, StringComparison.Ordinal);
        Assert.Contains("SetAdminConversationEventPublicationStatusAsync", webClient, StringComparison.Ordinal);
        Assert.Contains("GetAdminEventRsvpsAsync", webClient, StringComparison.Ordinal);
        Assert.Contains("SetAdminEventRsvpStatusAsync", webClient, StringComparison.Ordinal);

        Assert.Contains("\"/api/admin/catalog/conversation-events\"", apiProgram, StringComparison.Ordinal);
        Assert.Contains("\"/api/admin/catalog/conversation-events/by-organizer/{organizerProfileSlug}\"", apiProgram, StringComparison.Ordinal);
        Assert.Contains("\"/api/admin/catalog/conversation-events/{slug}/publication-status\"", apiProgram, StringComparison.Ordinal);
        Assert.Contains("\"/api/admin/catalog/conversation-events/{slug}/rsvps\"", apiProgram, StringComparison.Ordinal);
        Assert.Contains("\"/api/admin/catalog/conversation-events/{slug}/rsvps/{rsvpId:guid}/status\"", apiProgram, StringComparison.Ordinal);
        Assert.Contains("IConversationEventAdminService", apiProgram, StringComparison.Ordinal);
        Assert.Contains("IEventRsvpService", apiProgram, StringComparison.Ordinal);
    }

    private static string ReadSource(string repositoryRoot, string relativePath) =>
        File.ReadAllText(Path.Combine(repositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string ResolveRepositoryRoot()
    {
        DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string candidateSolutionPath = Path.Combine(currentDirectory.FullName, "DarwinLingua.slnx");

            if (File.Exists(candidateSolutionPath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new InvalidOperationException("Unable to resolve repository root from test execution directory.");
    }
}

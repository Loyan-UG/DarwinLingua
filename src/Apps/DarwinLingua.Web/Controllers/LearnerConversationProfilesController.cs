using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[Authorize]
[Route("learner-profile")]
public sealed class LearnerConversationProfilesController(IWebCatalogApiClient catalogApiClient) : Controller
{
    [HttpGet("", Name = "LearnerConversationProfiles_Edit")]
    public async Task<IActionResult> Edit(CancellationToken cancellationToken)
    {
        string ownerEmail = GetOwnerEmail();
        LearnerConversationProfileModel? profile = await catalogApiClient
            .GetLearnerConversationProfileAsync(ownerEmail, cancellationToken)
            .ConfigureAwait(false);

        return View("Edit", new LearnerConversationProfilePageViewModel(
            CreateInput(profile, ownerEmail),
            profile,
            TempData["StatusMessage"] as string,
            TempData["ErrorMessage"] as string));
    }

    [HttpPost("", Name = "LearnerConversationProfiles_Save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        [Bind(Prefix = "Input")] LearnerConversationProfileInputModel input,
        CancellationToken cancellationToken)
    {
        string ownerEmail = GetOwnerEmail();
        if (!ModelState.IsValid)
        {
            LearnerConversationProfileModel? existingProfile = await catalogApiClient
                .GetLearnerConversationProfileAsync(ownerEmail, cancellationToken)
                .ConfigureAwait(false);

            return View("Edit", new LearnerConversationProfilePageViewModel(
                input,
                existingProfile,
                null,
                "Required profile fields are missing or invalid."));
        }

        try
        {
            await catalogApiClient.SaveLearnerConversationProfileAsync(
                    ownerEmail,
                    new SaveLearnerConversationProfileRequest(
                        input.DisplayName,
                        input.CityRegion,
                        input.InteractionPreference,
                        input.GermanLevel,
                        SplitLanguageCodes(input.HelperLanguageCodesText),
                        input.ConversationGoals,
                        input.AvailabilityNotes,
                        input.Visibility,
                        input.HasConfirmedAdult),
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = "Learner profile saved.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Edit));
    }

    [HttpPost("enabled", Name = "LearnerConversationProfiles_SetEnabled")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetEnabled(bool isEnabled, CancellationToken cancellationToken)
    {
        try
        {
            await catalogApiClient.SetLearnerConversationProfileEnabledAsync(
                    GetOwnerEmail(),
                    new LearnerConversationProfileVisibilityRequest(isEnabled),
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = isEnabled
                ? "Learner profile enabled as private."
                : "Learner profile disabled.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Edit));
    }

    [HttpPost("delete", Name = "LearnerConversationProfiles_Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken)
    {
        try
        {
            await catalogApiClient
                .DeleteLearnerConversationProfileAsync(GetOwnerEmail(), cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = "Learner profile deleted.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Edit));
    }

    private string GetOwnerEmail() =>
        WebUserIdentity.GetRequiredEmail(User, "The authenticated learner does not have an email address.");

    private static LearnerConversationProfileInputModel CreateInput(
        LearnerConversationProfileModel? profile,
        string ownerEmail)
    {
        if (profile is null)
        {
            return new LearnerConversationProfileInputModel
            {
                DisplayName = ownerEmail,
            };
        }

        return new LearnerConversationProfileInputModel
        {
            DisplayName = profile.DisplayName,
            CityRegion = profile.CityRegion,
            InteractionPreference = profile.InteractionPreference,
            GermanLevel = profile.GermanLevel,
            HelperLanguageCodesText = string.Join(", ", profile.HelperLanguageCodes),
            ConversationGoals = profile.ConversationGoals,
            AvailabilityNotes = profile.AvailabilityNotes,
            Visibility = profile.Visibility,
            HasConfirmedAdult = profile.HasConfirmedAdult,
        };
    }

    private static IReadOnlyList<string> SplitLanguageCodes(string value) =>
        value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
}

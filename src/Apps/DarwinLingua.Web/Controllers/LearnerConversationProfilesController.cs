using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Controllers;

[Authorize]
[Route("learner-profile")]
public sealed class LearnerConversationProfilesController(
    IWebCatalogApiClient catalogApiClient,
    IStringLocalizer<SharedResource> localizer) : Controller
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
        IReadOnlyList<string> helperLanguageCodes = SplitLanguageCodes(input.HelperLanguageCodesText);
        if (!ModelState.IsValid ||
            !IsAllowedInteractionPreference(input.InteractionPreference) ||
            !IsAllowedGermanLevel(input.GermanLevel) ||
            !IsAllowedVisibility(input.Visibility) ||
            !HasAllowedLanguageCodes(helperLanguageCodes))
        {
            LearnerConversationProfileModel? existingProfile = await catalogApiClient
                .GetLearnerConversationProfileAsync(ownerEmail, cancellationToken)
                .ConfigureAwait(false);

            return View("Edit", new LearnerConversationProfilePageViewModel(
                input,
                existingProfile,
                null,
                localizer["Required profile fields are missing or invalid."].Value));
        }

        try
        {
            await catalogApiClient.SaveLearnerConversationProfileAsync(
                    ownerEmail,
                    new SaveLearnerConversationProfileRequest(
                        input.DisplayName.Trim(),
                        TrimToNull(input.CityRegion),
                        input.InteractionPreference,
                        input.GermanLevel,
                        helperLanguageCodes,
                        input.ConversationGoals.Trim(),
                        TrimToNull(input.AvailabilityNotes),
                        input.Visibility,
                        input.HasConfirmedAdult),
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = localizer["Learner profile saved."].Value;
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = BuildProfileOperationErrorMessage(exception);
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
                ? localizer["Learner profile enabled as private."].Value
                : localizer["Learner profile disabled."].Value;
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = BuildProfileOperationErrorMessage(exception);
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

            TempData["StatusMessage"] = localizer["Learner profile deleted."].Value;
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = BuildProfileOperationErrorMessage(exception);
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

    private static string? TrimToNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static bool IsAllowedInteractionPreference(string value) =>
        string.Equals(value, "online", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, "in-person", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, "both", StringComparison.OrdinalIgnoreCase);

    private static bool IsAllowedGermanLevel(string value) =>
        string.Equals(value, "A1", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, "A2", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, "B1", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, "B2", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, "C1", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, "C2", StringComparison.OrdinalIgnoreCase);

    private static bool IsAllowedVisibility(string value) =>
        string.Equals(value, "private", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, "request-only", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, "public", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, "disabled", StringComparison.OrdinalIgnoreCase);

    private static bool HasAllowedLanguageCodes(IReadOnlyCollection<string> languageCodes) =>
        languageCodes.Count > 0 && languageCodes.All(static code =>
            code.Length is >= 2 and <= 8 &&
            code.All(static character =>
                (character >= 'a' && character <= 'z') ||
                (character >= 'A' && character <= 'Z') ||
                character == '-'));

    private string BuildProfileOperationErrorMessage(Exception exception) =>
        exception.Message.Contains("409", StringComparison.OrdinalIgnoreCase)
            ? localizer["The learner profile could not be saved because it conflicts with existing data."].Value
            : localizer["The learner profile operation could not be completed. Review the fields and try again."].Value;
}

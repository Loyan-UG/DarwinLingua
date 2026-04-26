using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Localization.Application.Abstractions;
using DarwinLingua.Localization.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DarwinLingua.Web.Controllers;

[Route("settings")]
public sealed class SettingsController(
    IWebUserPreferenceService webUserPreferenceService,
    ILanguageQueryService languageQueryService,
    IWebEntitledFeatureAccessService featureAccessService) : Controller
{
    private const string DualMeaningLanguageLockedMessage = "A trial or premium plan is required to enable a second meaning language.";

    [HttpGet("", Name = "Settings_Index")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        SettingsPageViewModel viewModel = await BuildViewModelAsync(
            TempData["StatusMessage"] as string,
            cancellationToken);

        return View(viewModel);
    }

    [HttpPost("update", Name = "Settings_Update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(SettingsUpdateInputModel input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SettingsPageViewModel invalidViewModel = await BuildViewModelAsync(null, cancellationToken, input);
            return View("Index", invalidViewModel);
        }

        string? secondaryMeaningLanguageCode = string.Equals(
            input.PrimaryMeaningLanguageCode,
            input.SecondaryMeaningLanguageCode,
            StringComparison.OrdinalIgnoreCase)
            ? null
            : input.SecondaryMeaningLanguageCode;

        secondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(secondaryMeaningLanguageCode, cancellationToken)
            .ConfigureAwait(false);

        await webUserPreferenceService.UpdatePreferencesAsync(
            input.UiLanguageCode,
            input.PrimaryMeaningLanguageCode,
            secondaryMeaningLanguageCode,
            cancellationToken);

        TempData["StatusMessage"] = "Language preferences updated.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<SettingsPageViewModel> BuildViewModelAsync(
        string? statusMessage,
        CancellationToken cancellationToken,
        SettingsUpdateInputModel? input = null)
    {
        var profile = await webUserPreferenceService.GetProfileAsync(cancellationToken);
        bool canUseDualMeaningLanguage = await featureAccessService
            .CanUseDualMeaningLanguageAsync(cancellationToken)
            .ConfigureAwait(false);
        string? effectiveSecondaryMeaningLanguageCode = canUseDualMeaningLanguage
            ? profile.PreferredMeaningLanguage2
            : null;
        IReadOnlyList<SupportedLanguageModel> languages = await languageQueryService.GetActiveLanguagesAsync(cancellationToken);

        SettingsUpdateInputModel formInput = input ?? new SettingsUpdateInputModel
        {
            UiLanguageCode = profile.UiLanguageCode,
            PrimaryMeaningLanguageCode = profile.PreferredMeaningLanguage1,
            SecondaryMeaningLanguageCode = effectiveSecondaryMeaningLanguageCode
        };

        IReadOnlyList<SelectListItem> uiLanguageOptions = languages
            .Where(static language => language.SupportsUserInterface)
            .Select(language => new SelectListItem(
                $"{language.EnglishName} ({language.NativeName})",
                language.Code,
                string.Equals(language.Code, formInput.UiLanguageCode, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        IReadOnlyList<SelectListItem> meaningLanguageOptions = languages
            .Where(static language => language.SupportsMeanings)
            .Select(language => new SelectListItem(
                $"{language.EnglishName} ({language.NativeName})",
                language.Code,
                string.Equals(language.Code, formInput.PrimaryMeaningLanguageCode, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        List<SelectListItem> secondaryMeaningOptions =
        [
            new("None", string.Empty, string.IsNullOrWhiteSpace(formInput.SecondaryMeaningLanguageCode))
        ];

        secondaryMeaningOptions.AddRange(languages
            .Where(static language => language.SupportsMeanings)
            .Select(language => new SelectListItem(
                $"{language.EnglishName} ({language.NativeName})",
                language.Code,
                string.Equals(language.Code, formInput.SecondaryMeaningLanguageCode, StringComparison.OrdinalIgnoreCase))));

        return new SettingsPageViewModel(
            formInput,
            uiLanguageOptions,
            meaningLanguageOptions,
            secondaryMeaningOptions,
            statusMessage,
            canUseDualMeaningLanguage,
            canUseDualMeaningLanguage ? null : DualMeaningLanguageLockedMessage,
            User.Identity?.IsAuthenticated == true);
    }
}

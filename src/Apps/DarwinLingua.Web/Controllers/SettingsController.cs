using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Localization.Application.Abstractions;
using DarwinLingua.Localization.Application.Models;
using DarwinLingua.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DarwinLingua.Web.Controllers;

public sealed class SettingsController(
    IUserLearningProfileService userLearningProfileService,
    ILanguageQueryService languageQueryService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        SettingsPageViewModel viewModel = await BuildViewModelAsync(
            TempData["StatusMessage"] as string,
            cancellationToken);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(SettingsUpdateInputModel input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SettingsPageViewModel invalidViewModel = await BuildViewModelAsync(null, cancellationToken, input);
            return View("Index", invalidViewModel);
        }

        await userLearningProfileService.UpdateUiLanguagePreferenceAsync(input.UiLanguageCode, cancellationToken);

        string? secondaryMeaningLanguageCode = string.Equals(
            input.PrimaryMeaningLanguageCode,
            input.SecondaryMeaningLanguageCode,
            StringComparison.OrdinalIgnoreCase)
            ? null
            : input.SecondaryMeaningLanguageCode;

        await userLearningProfileService.UpdateMeaningLanguagePreferencesAsync(
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
        var profile = await userLearningProfileService.GetCurrentProfileAsync(cancellationToken);
        IReadOnlyList<SupportedLanguageModel> languages = await languageQueryService.GetActiveLanguagesAsync(cancellationToken);

        SettingsUpdateInputModel formInput = input ?? new SettingsUpdateInputModel
        {
            UiLanguageCode = profile.UiLanguageCode,
            PrimaryMeaningLanguageCode = profile.PreferredMeaningLanguage1,
            SecondaryMeaningLanguageCode = profile.PreferredMeaningLanguage2
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
            statusMessage);
    }
}

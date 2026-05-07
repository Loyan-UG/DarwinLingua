using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Localization.Application.Abstractions;
using DarwinLingua.Localization.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using DarwinLingua.Web.Localization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Controllers;

[Route("settings")]
public sealed class SettingsController(
    IWebUserPreferenceService webUserPreferenceService,
    ILanguageQueryService languageQueryService,
    IWebEntitledFeatureAccessService featureAccessService,
    IStringLocalizer<SharedResource> localizer) : Controller
{
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
        IReadOnlyList<SupportedLanguageModel> languages = await languageQueryService
            .GetActiveLanguagesAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!ModelState.IsValid || !HasSupportedLanguageSelection(input, languages))
        {
            SettingsPageViewModel invalidViewModel = await BuildViewModelAsync(null, cancellationToken, input);
            return View("Index", invalidViewModel);
        }

        string? secondaryMeaningLanguageCode = string.Equals(
            input.PrimaryMeaningLanguageCode,
            input.SecondaryMeaningLanguageCode,
            StringComparison.OrdinalIgnoreCase)
            ? null
            : TrimToNull(input.SecondaryMeaningLanguageCode);

        secondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(secondaryMeaningLanguageCode, cancellationToken)
            .ConfigureAwait(false);

        await webUserPreferenceService.UpdatePreferencesAsync(
            input.UiLanguageCode.Trim(),
            input.PrimaryMeaningLanguageCode.Trim(),
            secondaryMeaningLanguageCode,
            cancellationToken);

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(input.UiLanguageCode.Trim())),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = false,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps
            });

        TempData["StatusMessage"] = localizer["Language preferences updated."].Value;
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
            canUseDualMeaningLanguage ? null : localizer["A trial or premium plan is required to enable a second meaning language."].Value,
            User.Identity?.IsAuthenticated == true);
    }

    private static bool HasSupportedLanguageSelection(
        SettingsUpdateInputModel input,
        IReadOnlyList<SupportedLanguageModel> languages)
    {
        bool hasUiLanguage = languages.Any(language =>
            language.SupportsUserInterface &&
            string.Equals(language.Code, input.UiLanguageCode, StringComparison.OrdinalIgnoreCase));
        bool hasPrimaryMeaningLanguage = languages.Any(language =>
            language.SupportsMeanings &&
            string.Equals(language.Code, input.PrimaryMeaningLanguageCode, StringComparison.OrdinalIgnoreCase));
        bool hasSecondaryMeaningLanguage = string.IsNullOrWhiteSpace(input.SecondaryMeaningLanguageCode) ||
            languages.Any(language =>
                language.SupportsMeanings &&
                string.Equals(language.Code, input.SecondaryMeaningLanguageCode, StringComparison.OrdinalIgnoreCase));

        return hasUiLanguage && hasPrimaryMeaningLanguage && hasSecondaryMeaningLanguage;
    }

    private static string? TrimToNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}

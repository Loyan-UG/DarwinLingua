using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Data;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Controllers;

[Route("settings")]
public sealed class SettingsController(
    IWebUserPreferenceService webUserPreferenceService,
    IWebEntitledFeatureAccessService featureAccessService,
    IStringLocalizer<SharedResource> localizer) : Controller
{
    private static readonly IReadOnlyList<SupportedWebLanguage> SupportedLanguages =
    [
        new("en", "English", "English", SupportsUserInterface: true, SupportsMeanings: true),
        new("de", "German", "Deutsch", SupportsUserInterface: true, SupportsMeanings: false),
        new("fa", "Persian", "فارسی", SupportsUserInterface: false, SupportsMeanings: true),
        new("ar", "Arabic", "العربية", SupportsUserInterface: false, SupportsMeanings: true),
        new("tr", "Turkish", "Türkçe", SupportsUserInterface: false, SupportsMeanings: true),
        new("ru", "Russian", "Русский", SupportsUserInterface: false, SupportsMeanings: true),
        new("ckb", "Kurdish Sorani", "کوردیی ناوەندی", SupportsUserInterface: false, SupportsMeanings: true),
        new("kmr", "Kurdish Kurmanji", "Kurmancî", SupportsUserInterface: false, SupportsMeanings: true),
        new("pl", "Polish", "Polski", SupportsUserInterface: false, SupportsMeanings: true),
        new("ro", "Romanian", "Română", SupportsUserInterface: false, SupportsMeanings: true),
        new("sq", "Albanian", "Shqip", SupportsUserInterface: false, SupportsMeanings: true)
    ];

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
        IReadOnlyList<SupportedWebLanguage> languages = SupportedLanguages;

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
            input.TargetLearningLanguageCode.Trim(),
            input.UiLanguageCode.Trim(),
            input.PrimaryMeaningLanguageCode.Trim(),
            secondaryMeaningLanguageCode,
            input.AllowsRudeSlangContent,
            input.AdultContentAccessState,
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
        IReadOnlyList<SupportedWebLanguage> languages = SupportedLanguages;

        SettingsUpdateInputModel formInput = input ?? new SettingsUpdateInputModel
        {
            UiLanguageCode = profile.UiLanguageCode,
            TargetLearningLanguageCode = profile.TargetLearningLanguageCode,
            PrimaryMeaningLanguageCode = profile.PreferredMeaningLanguage1,
            SecondaryMeaningLanguageCode = effectiveSecondaryMeaningLanguageCode,
            AllowsRudeSlangContent = profile.AllowsRudeSlangContent,
            AdultContentAccessState = profile.AdultContentAccessState
        };

        IReadOnlyList<SelectListItem> targetLearningLanguageOptions = TargetLearningLanguageCatalog.All
            .OrderBy(static language => language.SortOrder)
            .Select(language => new SelectListItem(
                language.IsActive
                    ? $"{language.NativeName} ({language.EnglishName})"
                    : $"{language.NativeName} ({language.EnglishName}) - {(language.IsPilot ? localizer["Pilot"].Value : localizer["Planned"].Value)}",
                language.Code,
                string.Equals(language.Code, formInput.TargetLearningLanguageCode, StringComparison.OrdinalIgnoreCase),
                disabled: !language.IsActive))
            .ToArray();

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

        IReadOnlyList<SelectListItem> adultAccessOptions =
        [
            new(localizer["Do not request adult content access"].Value, AdultContentAccessStates.NotRequested, string.Equals(formInput.AdultContentAccessState, AdultContentAccessStates.NotRequested, StringComparison.OrdinalIgnoreCase)),
            new(localizer["I self-declare that I am 18 or older"].Value, AdultContentAccessStates.SelfDeclaredAdult, string.Equals(formInput.AdultContentAccessState, AdultContentAccessStates.SelfDeclaredAdult, StringComparison.OrdinalIgnoreCase)),
        ];

        return new SettingsPageViewModel(
            formInput,
            targetLearningLanguageOptions,
            uiLanguageOptions,
            meaningLanguageOptions,
            secondaryMeaningOptions,
            adultAccessOptions,
            statusMessage,
            canUseDualMeaningLanguage,
            canUseDualMeaningLanguage ? null : localizer["A trial or premium plan is required to enable a second meaning language."].Value,
            User.Identity?.IsAuthenticated == true);
    }

    private static bool HasSupportedLanguageSelection(
        SettingsUpdateInputModel input,
        IReadOnlyList<SupportedWebLanguage> languages)
    {
        bool hasUiLanguage = languages.Any(language =>
            language.SupportsUserInterface &&
            string.Equals(language.Code, input.UiLanguageCode, StringComparison.OrdinalIgnoreCase));
        bool hasTargetLearningLanguage = TargetLearningLanguageCatalog.Active.Any(language =>
            string.Equals(language.Code, input.TargetLearningLanguageCode, StringComparison.OrdinalIgnoreCase));
        bool hasPrimaryMeaningLanguage = languages.Any(language =>
            language.SupportsMeanings &&
            string.Equals(language.Code, input.PrimaryMeaningLanguageCode, StringComparison.OrdinalIgnoreCase));
        bool hasSecondaryMeaningLanguage = string.IsNullOrWhiteSpace(input.SecondaryMeaningLanguageCode) ||
            languages.Any(language =>
                language.SupportsMeanings &&
                string.Equals(language.Code, input.SecondaryMeaningLanguageCode, StringComparison.OrdinalIgnoreCase));
        bool hasAdultAccessState = input.AdultContentAccessState is not null &&
            (string.Equals(input.AdultContentAccessState, AdultContentAccessStates.NotRequested, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(input.AdultContentAccessState, AdultContentAccessStates.SelfDeclaredAdult, StringComparison.OrdinalIgnoreCase));

        return hasTargetLearningLanguage && hasUiLanguage && hasPrimaryMeaningLanguage && hasSecondaryMeaningLanguage && hasAdultAccessState;
    }

    private static string? TrimToNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private sealed record SupportedWebLanguage(
        string Code,
        string EnglishName,
        string NativeName,
        bool SupportsUserInterface,
        bool SupportsMeanings);
}

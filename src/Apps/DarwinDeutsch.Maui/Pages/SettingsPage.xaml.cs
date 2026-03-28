using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Localization.Application.Abstractions;
using DarwinLingua.Localization.Application.Models;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Allows the user to manage persisted UI and meaning-language preferences.
/// </summary>
public partial class SettingsPage : ContentPage
{
    private readonly IAppLocalizationService _appLocalizationService;
    private readonly IUserLearningProfileService _userLearningProfileService;
    private readonly ILanguageQueryService _languageQueryService;
    private bool _isUpdatingSelection;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsPage"/> class.
    /// </summary>
    /// <param name="appLocalizationService">The service that manages UI localization.</param>
    /// <param name="userLearningProfileService">The service that manages the persisted learning profile.</param>
    /// <param name="languageQueryService">The service that loads active language reference data.</param>
    public SettingsPage(
        IAppLocalizationService appLocalizationService,
        IUserLearningProfileService userLearningProfileService,
        ILanguageQueryService languageQueryService)
    {
        ArgumentNullException.ThrowIfNull(appLocalizationService);
        ArgumentNullException.ThrowIfNull(userLearningProfileService);
        ArgumentNullException.ThrowIfNull(languageQueryService);

        InitializeComponent();

        _appLocalizationService = appLocalizationService;
        _userLearningProfileService = userLearningProfileService;
        _languageQueryService = languageQueryService;
        _appLocalizationService.CultureChanged += OnCultureChanged;

        ApplyStaticLocalizedText();
    }

    /// <summary>
    /// Rebuilds the localized page text when the page becomes visible.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await RebuildPageStateAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Releases event subscriptions when the page handler is detached.
    /// </summary>
    /// <param name="args">The handler-changing event arguments.</param>
    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        if (args.NewHandler is null)
        {
            _appLocalizationService.CultureChanged -= OnCultureChanged;
        }

        base.OnHandlerChanging(args);
    }

    /// <summary>
    /// Handles UI culture changes raised by the localization service.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OnCultureChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await RebuildPageStateAsync().ConfigureAwait(true);
        });
    }

    /// <summary>
    /// Applies the current localized copy that does not require database reads.
    /// </summary>
    private void ApplyStaticLocalizedText()
    {
        Title = AppStrings.SettingsTabTitle;
        HeadlineLabel.Text = AppStrings.SettingsHeadline;
        DescriptionLabel.Text = AppStrings.SettingsDescription;
        AppInfoSectionLabel.Text = AppStrings.SettingsAppInfoSectionLabel;
        LanguagePickerCaptionLabel.Text = AppStrings.SettingsUiLanguageLabel;
        PrimaryMeaningLanguageCaptionLabel.Text = AppStrings.SettingsPrimaryMeaningLanguageLabel;
        SecondaryMeaningLanguageCaptionLabel.Text = AppStrings.SettingsSecondaryMeaningLanguageLabel;
        CurrentLanguageSectionView.SectionTitle = AppStrings.HomeCurrentUiLanguageLabel;
        MeaningLanguagesSectionView.SectionTitle = AppStrings.HomeMeaningLanguagesLabel;
        SupportedLanguagesSectionView.SectionTitle = AppStrings.HomeSupportedLanguagesLabel;
        CurrentFeaturesSectionView.SectionTitle = AppStrings.WelcomeCurrentFeaturesTitle;
        FutureFeaturesSectionView.SectionTitle = AppStrings.WelcomeFutureFeaturesTitle;
    }

    /// <summary>
    /// Rebuilds the page state using the current profile and supported language reference data.
    /// </summary>
    private async Task RebuildPageStateAsync()
    {
        ApplyStaticLocalizedText();

        IReadOnlyList<UiLanguageOption> supportedUiLanguages = _appLocalizationService.GetSupportedLanguages();
        IReadOnlyList<SupportedLanguageModel> supportedMeaningLanguages = (await _languageQueryService
                .GetActiveLanguagesAsync(CancellationToken.None)
                .ConfigureAwait(true))
            .Where(language => language.SupportsMeanings)
            .OrderBy(language => language.EnglishName)
            .ToArray();
        UserLearningProfileModel profile = await _userLearningProfileService
            .GetCurrentProfileAsync(CancellationToken.None)
            .ConfigureAwait(true);

        List<MeaningLanguageOption> primaryMeaningOptions = supportedMeaningLanguages
            .Select(CreateMeaningLanguageOption)
            .ToList();
        List<MeaningLanguageOption> secondaryMeaningOptions = BuildSecondaryMeaningOptions(
            supportedMeaningLanguages,
            profile.PreferredMeaningLanguage1);

        _isUpdatingSelection = true;

        LanguagePicker.ItemsSource = supportedUiLanguages.ToList();
        LanguagePicker.SelectedItem = supportedUiLanguages.Single(option => string.Equals(
            option.CultureName,
            profile.UiLanguageCode,
            StringComparison.OrdinalIgnoreCase));

        PrimaryMeaningLanguagePicker.ItemsSource = primaryMeaningOptions;
        PrimaryMeaningLanguagePicker.SelectedItem = primaryMeaningOptions.Single(option => string.Equals(
            option.LanguageCode,
            profile.PreferredMeaningLanguage1,
            StringComparison.OrdinalIgnoreCase));

        SecondaryMeaningLanguagePicker.ItemsSource = secondaryMeaningOptions;
        SecondaryMeaningLanguagePicker.SelectedItem = secondaryMeaningOptions.Single(option => string.Equals(
            option.LanguageCode ?? string.Empty,
            profile.PreferredMeaningLanguage2 ?? string.Empty,
            StringComparison.OrdinalIgnoreCase));

        CurrentLanguageSectionView.SectionValue = supportedUiLanguages
            .Single(option => string.Equals(option.CultureName, profile.UiLanguageCode, StringComparison.OrdinalIgnoreCase))
            .DisplayName;
        MeaningLanguagesSectionView.SectionValue = BuildMeaningLanguageSummary(
            supportedMeaningLanguages,
            profile.PreferredMeaningLanguage1,
            profile.PreferredMeaningLanguage2);
        SupportedLanguagesSectionView.SectionValue = supportedMeaningLanguages.Count == 0
            ? AppStrings.HomeNoLanguages
            : string.Join(Environment.NewLine, supportedMeaningLanguages.Select(language => $"{language.NativeName} ({language.Code})"));
        CurrentFeaturesSectionView.SectionValue = AppStrings.WelcomeCurrentFeaturesBody;
        FutureFeaturesSectionView.SectionValue = AppStrings.WelcomeFutureFeaturesBody;

        _isUpdatingSelection = false;
    }

    private static string BuildMeaningLanguageSummary(
        IReadOnlyList<SupportedLanguageModel> supportedMeaningLanguages,
        string primaryLanguageCode,
        string? secondaryLanguageCode)
    {
        string primaryMeaningLanguage = ResolveLanguageDisplayName(supportedMeaningLanguages, primaryLanguageCode);
        string? secondaryMeaningLanguage = string.IsNullOrWhiteSpace(secondaryLanguageCode)
            ? null
            : ResolveLanguageDisplayName(supportedMeaningLanguages, secondaryLanguageCode);

        return secondaryMeaningLanguage is null
            ? primaryMeaningLanguage
            : $"{primaryMeaningLanguage}, {secondaryMeaningLanguage}";
    }

    private static string ResolveLanguageDisplayName(
        IReadOnlyList<SupportedLanguageModel> supportedLanguages,
        string languageCode)
    {
        ArgumentNullException.ThrowIfNull(supportedLanguages);
        ArgumentException.ThrowIfNullOrWhiteSpace(languageCode);

        SupportedLanguageModel? language = supportedLanguages.SingleOrDefault(candidate => string.Equals(
            candidate.Code,
            languageCode,
            StringComparison.OrdinalIgnoreCase));

        return language is null
            ? languageCode
            : $"{language.NativeName} ({language.Code})";
    }

    /// <summary>
    /// Persists the selected UI language when the picker value changes.
    /// </summary>
    private async void OnLanguagePickerSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_isUpdatingSelection || LanguagePicker.SelectedItem is not UiLanguageOption selectedLanguage)
        {
            return;
        }

        await _appLocalizationService
            .SetCultureAsync(selectedLanguage.CultureName, CancellationToken.None)
            .ConfigureAwait(true);
    }

    /// <summary>
    /// Persists the selected primary meaning language when the picker value changes.
    /// </summary>
    private async void OnPrimaryMeaningLanguagePickerSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_isUpdatingSelection || PrimaryMeaningLanguagePicker.SelectedItem is not MeaningLanguageOption selectedLanguage)
        {
            return;
        }

        string? secondaryMeaningLanguage = (SecondaryMeaningLanguagePicker.SelectedItem as MeaningLanguageOption)?.LanguageCode;

        if (string.Equals(selectedLanguage.LanguageCode, secondaryMeaningLanguage, StringComparison.OrdinalIgnoreCase))
        {
            secondaryMeaningLanguage = null;
        }

        await _userLearningProfileService
            .UpdateMeaningLanguagePreferencesAsync(
                selectedLanguage.LanguageCode!,
                secondaryMeaningLanguage,
                CancellationToken.None)
            .ConfigureAwait(true);

        await RebuildPageStateAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Persists the selected secondary meaning language when the picker value changes.
    /// </summary>
    private async void OnSecondaryMeaningLanguagePickerSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_isUpdatingSelection || SecondaryMeaningLanguagePicker.SelectedItem is not MeaningLanguageOption selectedLanguage)
        {
            return;
        }

        if (PrimaryMeaningLanguagePicker.SelectedItem is not MeaningLanguageOption primaryMeaningLanguage)
        {
            return;
        }

        await _userLearningProfileService
            .UpdateMeaningLanguagePreferencesAsync(
                primaryMeaningLanguage.LanguageCode!,
                selectedLanguage.LanguageCode,
                CancellationToken.None)
            .ConfigureAwait(true);

        await RebuildPageStateAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Creates a presentation option for the meaning-language pickers.
    /// </summary>
    private static MeaningLanguageOption CreateMeaningLanguageOption(SupportedLanguageModel language)
    {
        ArgumentNullException.ThrowIfNull(language);

        return new MeaningLanguageOption(language.Code, $"{language.NativeName} ({language.Code})");
    }

    /// <summary>
    /// Builds the valid secondary meaning-language options for the selected primary language.
    /// </summary>
    private static List<MeaningLanguageOption> BuildSecondaryMeaningOptions(
        IReadOnlyList<SupportedLanguageModel> supportedMeaningLanguages,
        string selectedPrimaryMeaningLanguage)
    {
        ArgumentNullException.ThrowIfNull(supportedMeaningLanguages);
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedPrimaryMeaningLanguage);

        List<MeaningLanguageOption> options =
        [
            new MeaningLanguageOption(null, AppStrings.SettingsSecondaryMeaningLanguageNone),
        ];

        options.AddRange(supportedMeaningLanguages
            .Where(language => !string.Equals(language.Code, selectedPrimaryMeaningLanguage, StringComparison.OrdinalIgnoreCase))
            .Select(CreateMeaningLanguageOption));

        return options;
    }

    /// <summary>
    /// Represents a picker option for meaning-language selection.
    /// </summary>
    private sealed record MeaningLanguageOption(string? LanguageCode, string DisplayName);
}

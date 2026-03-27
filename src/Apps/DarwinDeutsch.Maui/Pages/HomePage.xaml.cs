using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Localization.Application.Abstractions;
using DarwinLingua.Localization.Application.Models;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays the current foundation state backed by localized strings, seeded reference data, and persisted user preferences.
/// </summary>
public partial class HomePage : ContentPage
{
    private readonly IAppLocalizationService _appLocalizationService;
    private readonly ITopicQueryService _topicQueryService;
    private readonly ILanguageQueryService _languageQueryService;
    private readonly IUserLearningProfileService _userLearningProfileService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomePage"/> class.
    /// </summary>
    /// <param name="appLocalizationService">The service that manages UI localization.</param>
    /// <param name="languageQueryService">The service that reads seeded language reference data.</param>
    public HomePage(
        IAppLocalizationService appLocalizationService,
        ITopicQueryService topicQueryService,
        ILanguageQueryService languageQueryService,
        IUserLearningProfileService userLearningProfileService)
    {
        ArgumentNullException.ThrowIfNull(appLocalizationService);
        ArgumentNullException.ThrowIfNull(topicQueryService);
        ArgumentNullException.ThrowIfNull(languageQueryService);
        ArgumentNullException.ThrowIfNull(userLearningProfileService);

        InitializeComponent();

        _appLocalizationService = appLocalizationService;
        _topicQueryService = topicQueryService;
        _languageQueryService = languageQueryService;
        _userLearningProfileService = userLearningProfileService;

        _appLocalizationService.CultureChanged += OnCultureChanged;

        ApplyLocalizedText();
    }

    /// <summary>
    /// Refreshes localized copy and seeded data whenever the page becomes visible.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        ApplyLocalizedText();
        await RefreshReferenceDataAsync().ConfigureAwait(true);
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
    /// Handles culture changes raised by the localization service.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OnCultureChanged(object? sender, EventArgs e)
    {
        ApplyLocalizedText();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await RefreshReferenceDataAsync().ConfigureAwait(true);
        });
    }

    /// <summary>
    /// Applies localized text to the current visual elements.
    /// </summary>
    private void ApplyLocalizedText()
    {
        Title = AppStrings.HomeTabTitle;
        HeadlineLabel.Text = AppStrings.HomeHeadline;
        IntroLabel.Text = AppStrings.HomeIntro;
        CurrentLanguageSectionView.SectionTitle = AppStrings.HomeCurrentUiLanguageLabel;
        CurrentLanguageSectionView.SectionValue = _appLocalizationService.CurrentCulture.NativeName;
        SupportedLanguagesSectionView.SectionTitle = AppStrings.HomeSupportedLanguagesLabel;
        MeaningLanguagesSectionView.SectionTitle = AppStrings.HomeMeaningLanguagesLabel;
        TopicsSectionView.SectionTitle = AppStrings.HomeTopicsLabel;
        CefrQuickFilterView.Caption = AppStrings.HomeCefrBrowseLabel;
        SearchActionBlockView.Caption = AppStrings.HomeSearchLabel;
        SearchActionBlockView.ButtonText = AppStrings.HomeSearchButton;
        BrowseTopicsActionBlockView.Caption = AppStrings.HomeTopicsLabel;
        BrowseTopicsActionBlockView.ButtonText = AppStrings.BrowseTabTitle;
        FavoritesActionBlockView.Caption = AppStrings.FavoritesPageTitle;
        FavoritesActionBlockView.ButtonText = AppStrings.FavoritesTabTitle;
    }

    /// <summary>
    /// Loads the active language rows that were seeded into the local database.
    /// </summary>
    /// <returns>A task that completes when the page values are refreshed.</returns>
    private async Task RefreshReferenceDataAsync()
    {
        IReadOnlyList<SupportedLanguageModel> supportedLanguages = await _languageQueryService
            .GetActiveLanguagesAsync(CancellationToken.None)
            .ConfigureAwait(true);
        UserLearningProfileModel profile = await _userLearningProfileService
            .GetCurrentProfileAsync(CancellationToken.None)
            .ConfigureAwait(true);

        IReadOnlyList<TopicListItemModel> topics = await _topicQueryService
            .GetTopicsAsync(_appLocalizationService.CurrentCulture.TwoLetterISOLanguageName, CancellationToken.None)
            .ConfigureAwait(true);

        SupportedLanguagesSectionView.SectionValue = supportedLanguages.Count == 0
            ? AppStrings.HomeNoLanguages
            : string.Join(Environment.NewLine, supportedLanguages.Select(language =>
                $"{language.NativeName} ({language.Code})"));

        string primaryMeaningLanguage = ResolveLanguageDisplayName(
            supportedLanguages,
            profile.PreferredMeaningLanguage1);
        string? secondaryMeaningLanguage = profile.PreferredMeaningLanguage2 is null
            ? null
            : ResolveLanguageDisplayName(supportedLanguages, profile.PreferredMeaningLanguage2);

        MeaningLanguagesSectionView.SectionValue = secondaryMeaningLanguage is null
            ? primaryMeaningLanguage
            : $"{primaryMeaningLanguage}, {secondaryMeaningLanguage}";

        TopicsSectionView.SectionValue = topics.Count == 0
            ? AppStrings.HomeNoTopics
            : string.Join(Environment.NewLine, topics.Select(topic => topic.DisplayName));
    }

    /// <summary>
    /// Resolves a human-readable language name from the active language reference data.
    /// </summary>
    private static string ResolveLanguageDisplayName(
        IReadOnlyList<SupportedLanguageModel> supportedLanguages,
        string languageCode)
    {
        ArgumentNullException.ThrowIfNull(supportedLanguages);
        ArgumentException.ThrowIfNullOrWhiteSpace(languageCode);

        SupportedLanguageModel? language = supportedLanguages
            .SingleOrDefault(candidate => string.Equals(
                candidate.Code,
                languageCode,
                StringComparison.OrdinalIgnoreCase));

        return language is null
            ? languageCode
            : $"{language.NativeName} ({language.Code})";
    }

    /// <summary>
    /// Navigates to the selected CEFR browse page.
    /// </summary>
    private async void OnCefrLevelSelected(object? sender, EventArgs e)
    {
        string cefrLevel = CefrQuickFilterView.SelectedLevel;
        if (string.IsNullOrWhiteSpace(cefrLevel))
        {
            return;
        }

        string escapedCefrLevel = Uri.EscapeDataString(cefrLevel);
        await Shell.Current.GoToAsync($"{nameof(CefrWordsPage)}?cefrLevel={escapedCefrLevel}")
            .ConfigureAwait(true);
    }

    /// <summary>
    /// Navigates to the lexical search page.
    /// </summary>
    private async void OnSearchActionInvoked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SearchWordsPage)).ConfigureAwait(true);
    }

    /// <summary>
    /// Navigates to the topics browse tab from the home dashboard.
    /// </summary>
    private async void OnBrowseTopicsActionInvoked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//browse").ConfigureAwait(true);
    }

    /// <summary>
    /// Navigates to the favorites tab from the home dashboard.
    /// </summary>
    private async void OnFavoritesActionInvoked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//favorites").ConfigureAwait(true);
    }
}

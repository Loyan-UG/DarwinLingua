using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Localization.Application.Abstractions;
using DarwinLingua.Localization.Application.Models;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Presents the one-time welcome flow, language choice, and product overview.
/// </summary>
public partial class WelcomePage : ContentPage
{
    private static readonly string[] PreferredLanguageOrder = ["en", "fa", "ru", "ar", "pl", "tr", "ro", "sq", "ckb", "kmr"];

    private readonly IAppLocalizationService _appLocalizationService;
    private readonly ILanguageQueryService _languageQueryService;
    private CancellationTokenSource? _refreshCancellationTokenSource;
    private bool _isUpdatingSelection;

    /// <summary>
    /// Initializes a new instance of the <see cref="WelcomePage"/> class.
    /// </summary>
    public WelcomePage(
        IAppLocalizationService appLocalizationService,
        ILanguageQueryService languageQueryService)
    {
        ArgumentNullException.ThrowIfNull(appLocalizationService);
        ArgumentNullException.ThrowIfNull(languageQueryService);

        InitializeComponent();

        _appLocalizationService = appLocalizationService;
        _languageQueryService = languageQueryService;
        _appLocalizationService.CultureChanged += OnCultureChanged;

        ApplyStaticLocalizedText();
    }

    /// <summary>
    /// Raised when the user completes the welcome flow.
    /// </summary>
    public event EventHandler? StartRequested;

    /// <summary>
    /// Refreshes the page whenever it becomes visible.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    protected override void OnDisappearing()
    {
        CancelRefreshRequest();
        base.OnDisappearing();
    }

    /// <summary>
    /// Releases event subscriptions when the page handler is detached.
    /// </summary>
    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        if (args.NewHandler is null)
        {
            _appLocalizationService.CultureChanged -= OnCultureChanged;
        }

        base.OnHandlerChanging(args);
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await RefreshAsync().ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
            }
        });
    }

    private void ApplyStaticLocalizedText()
    {
        Title = AppStrings.AppTitle;
        LanguageQuestionLabel.Text = AppStrings.WelcomeLanguageQuestion;
        HeadlineLabel.Text = AppStrings.WelcomeHeadline;
        DescriptionLabel.Text = AppStrings.WelcomeDescription;
        CurrentFeaturesLabel.Text = AppStrings.WelcomeCurrentFeaturesTitle;
        CurrentFeaturesBodyLabel.Text = AppStrings.WelcomeCurrentFeaturesBody;
        LearnWithLanguagesLabel.Text = AppStrings.WelcomeLearnWithLanguagesTitle;
        InterfaceLanguagesLabel.Text = AppStrings.WelcomeInterfaceLanguagesTitle;
        FutureFeaturesLabel.Text = AppStrings.WelcomeFutureFeaturesTitle;
        FutureFeaturesBodyLabel.Text = AppStrings.WelcomeFutureFeaturesBody;
        StartButton.Text = AppStrings.WelcomeStartButton;
    }

    private async Task RefreshAsync()
    {
        ResetRefreshRequest();
        CancellationToken cancellationToken = _refreshCancellationTokenSource!.Token;

        ApplyStaticLocalizedText();

        IReadOnlyList<UiLanguageOption> supportedUiLanguages = _appLocalizationService.GetSupportedLanguages();
        IReadOnlyList<SupportedLanguageModel> activeLanguages = await _languageQueryService
            .GetActiveLanguagesAsync(cancellationToken)
            .ConfigureAwait(true);

        IReadOnlyList<SupportedLanguageModel> orderedLanguages = activeLanguages
            .OrderBy(language =>
            {
                int index = Array.FindIndex(
                    PreferredLanguageOrder,
                    code => string.Equals(code, language.Code, StringComparison.OrdinalIgnoreCase));
                return index < 0 ? int.MaxValue : index;
            })
            .ThenBy(language => language.Code, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        IReadOnlyList<SupportedLanguageModel> meaningLanguages = orderedLanguages
            .Where(language => language.SupportsMeanings)
            .ToArray();
        IReadOnlyList<SupportedLanguageModel> interfaceLanguages = orderedLanguages
            .Where(language => language.SupportsUserInterface)
            .ToArray();
        IReadOnlyList<SupportedLanguageModel> plannedInterfaceLanguages = meaningLanguages
            .Where(language => !language.SupportsUserInterface)
            .ToArray();

        _isUpdatingSelection = true;
        LanguagePicker.ItemsSource = supportedUiLanguages.ToList();
        LanguagePicker.SelectedItem = supportedUiLanguages.FirstOrDefault(option => string.Equals(
            option.CultureName,
            _appLocalizationService.CurrentCulture.TwoLetterISOLanguageName,
            StringComparison.OrdinalIgnoreCase))
            ?? supportedUiLanguages.FirstOrDefault();
        _isUpdatingSelection = false;

        LearnWithLanguagesBodyLabel.Text = string.Format(
            AppStrings.WelcomeLearnWithLanguagesBodyFormat,
            BuildLanguageList(meaningLanguages));
        InterfaceLanguagesBodyLabel.Text = string.Format(
            AppStrings.WelcomeInterfaceLanguagesBodyFormat,
            BuildLanguageList(interfaceLanguages),
            plannedInterfaceLanguages.Count == 0
                ? AppStrings.WelcomeNoPlannedUiLanguages
                : BuildLanguageList(plannedInterfaceLanguages));
    }

    private async void OnLanguagePickerSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_isUpdatingSelection || LanguagePicker.SelectedItem is not UiLanguageOption selectedLanguage)
        {
            return;
        }

        try
        {
            await _appLocalizationService
                .SetCultureAsync(selectedLanguage.CultureName, CancellationToken.None)
                .ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void OnStartButtonClicked(object? sender, EventArgs e)
    {
        StartRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ResetRefreshRequest()
    {
        CancelRefreshRequest();
        _refreshCancellationTokenSource = new CancellationTokenSource();
    }

    private void CancelRefreshRequest()
    {
        if (_refreshCancellationTokenSource is null)
        {
            return;
        }

        _refreshCancellationTokenSource.Cancel();
        _refreshCancellationTokenSource.Dispose();
        _refreshCancellationTokenSource = null;
    }

    private static string BuildLanguageList(IReadOnlyList<SupportedLanguageModel> languages)
    {
        ArgumentNullException.ThrowIfNull(languages);

        if (languages.Count == 0)
        {
            return AppStrings.HomeNoLanguages;
        }

        return string.Join(", ", languages.Select(language => $"{language.NativeName} ({language.Code})"));
    }
}

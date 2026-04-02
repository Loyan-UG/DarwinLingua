using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Controls;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinDeutsch.Maui.Services.Browse;
using DarwinDeutsch.Maui.Services.Storage;
using DarwinDeutsch.Maui.Services.Updates;
#if ANDROID
using DarwinDeutsch.Maui.Platforms.Android.Updates;
#endif
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Localization.Application.Abstractions;
using DarwinLingua.Localization.Application.Models;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Allows the user to manage persisted UI and meaning-language preferences.
/// </summary>
public partial class SettingsPage : ContentPage
{
    private static readonly string[] CefrLevels = ["A1", "A2", "B1", "B2", "C1", "C2"];

    private readonly IAppLocalizationService _appLocalizationService;
    private readonly IBrowseAccelerationService _browseAccelerationService;
    private readonly IRemoteContentUpdateService _remoteContentUpdateService;
    private readonly ISeedDatabaseProvisioningService _seedDatabaseProvisioningService;
    private readonly IActiveLearningProfileCacheService _activeLearningProfileCacheService;
    private readonly ILanguageQueryService _languageQueryService;
    private CancellationTokenSource? _pageStateCancellationTokenSource;
    private bool _isUpdatingSelection;
    private bool _isApplyingRemoteUpdate;
    private bool _isApplyingSeedUpdate;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsPage"/> class.
    /// </summary>
    /// <param name="appLocalizationService">The service that manages UI localization.</param>
    /// <param name="browseAccelerationService">The service that manages browse warm-up and cache invalidation.</param>
    /// <param name="remoteContentUpdateService">The service that applies remote content updates.</param>
    /// <param name="seedDatabaseProvisioningService">The service that provisions and merges packaged seed content.</param>
    /// <param name="activeLearningProfileCacheService">The service that caches the active learning profile for the current session.</param>
    /// <param name="languageQueryService">The service that loads active language reference data.</param>
    public SettingsPage(
        IAppLocalizationService appLocalizationService,
        IBrowseAccelerationService browseAccelerationService,
        IRemoteContentUpdateService remoteContentUpdateService,
        ISeedDatabaseProvisioningService seedDatabaseProvisioningService,
        IActiveLearningProfileCacheService activeLearningProfileCacheService,
        ILanguageQueryService languageQueryService)
    {
        ArgumentNullException.ThrowIfNull(appLocalizationService);
        ArgumentNullException.ThrowIfNull(browseAccelerationService);
        ArgumentNullException.ThrowIfNull(remoteContentUpdateService);
        ArgumentNullException.ThrowIfNull(seedDatabaseProvisioningService);
        ArgumentNullException.ThrowIfNull(activeLearningProfileCacheService);
        ArgumentNullException.ThrowIfNull(languageQueryService);

        InitializeComponent();

        _appLocalizationService = appLocalizationService;
        _browseAccelerationService = browseAccelerationService;
        _remoteContentUpdateService = remoteContentUpdateService;
        _seedDatabaseProvisioningService = seedDatabaseProvisioningService;
        _activeLearningProfileCacheService = activeLearningProfileCacheService;
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

        try
        {
            await RebuildPageStateAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            return;
        }
    }

    protected override void OnDisappearing()
    {
        CancelPageStateRequest();

        base.OnDisappearing();
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
            try
            {
                await RebuildPageStateAsync().ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                return;
            }
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
        ContentUpdatesSectionLabel.Text = AppStrings.SettingsContentUpdatesSectionLabel;
        RemoteContentUpdatesSectionLabel.Text = AppStrings.SettingsRemoteContentUpdatesSectionLabel;
        CatalogAreaUpdatesSectionLabel.Text = AppStrings.SettingsRemoteCatalogAreaSectionLabel;
        CefrLevelUpdatesSectionLabel.Text = AppStrings.SettingsRemoteCefrLevelsSectionLabel;
        PackagedSeedUpdatesSectionLabel.Text = AppStrings.SettingsPackagedSeedUpdatesSectionLabel;
        ContentUpdateStatusSectionView.SectionTitle = AppStrings.SettingsContentUpdatesStatusLabel;
        ContentUpdateDetailsSectionView.SectionTitle = AppStrings.SettingsContentUpdatesDetailsLabel;
        ContentUpdateDiagnosticsSectionView.SectionTitle = AppStrings.SettingsContentUpdatesDiagnosticsLabel;
        RemoteContentUpdateStatusSectionView.SectionTitle = AppStrings.SettingsContentUpdatesStatusLabel;
        RemoteContentUpdateDetailsSectionView.SectionTitle = AppStrings.SettingsContentUpdatesDetailsLabel;
        RemoteContentUpdateDiagnosticsSectionView.SectionTitle = AppStrings.SettingsContentUpdatesDiagnosticsLabel;
        RemoteContentUpdateHistorySectionView.SectionTitle = AppStrings.SettingsRemoteContentUpdatesHistoryLabel;
        CatalogAreaUpdateSectionView.SectionTitle = AppStrings.SettingsRemoteCatalogAreaTitle;
        CefrA1UpdateSectionView.SectionTitle = "A1";
        CefrA2UpdateSectionView.SectionTitle = "A2";
        CefrB1UpdateSectionView.SectionTitle = "B1";
        CefrB2UpdateSectionView.SectionTitle = "B2";
        CefrC1UpdateSectionView.SectionTitle = "C1";
        CefrC2UpdateSectionView.SectionTitle = "C2";
        ApplyRemoteUpdateButton.Text = AppStrings.SettingsRemoteContentUpdatesApplyButton;
        ApplyCatalogAreaUpdateButton.Text = string.Format(AppStrings.SettingsRemoteContentScopeApplyButtonFormat, AppStrings.SettingsRemoteCatalogAreaTitle);
        ApplyCefrA1UpdateButton.Text = string.Format(AppStrings.SettingsRemoteContentScopeApplyButtonFormat, "A1");
        ApplyCefrA2UpdateButton.Text = string.Format(AppStrings.SettingsRemoteContentScopeApplyButtonFormat, "A2");
        ApplyCefrB1UpdateButton.Text = string.Format(AppStrings.SettingsRemoteContentScopeApplyButtonFormat, "B1");
        ApplyCefrB2UpdateButton.Text = string.Format(AppStrings.SettingsRemoteContentScopeApplyButtonFormat, "B2");
        ApplyCefrC1UpdateButton.Text = string.Format(AppStrings.SettingsRemoteContentScopeApplyButtonFormat, "C1");
        ApplyCefrC2UpdateButton.Text = string.Format(AppStrings.SettingsRemoteContentScopeApplyButtonFormat, "C2");
        ApplySeedUpdateButton.Text = AppStrings.SettingsContentUpdatesApplyButton;
    }

    /// <summary>
    /// Rebuilds the page state using the current profile and supported language reference data.
    /// </summary>
    private async Task RebuildPageStateAsync()
    {
        ResetPageStateRequest();
        CancellationToken cancellationToken = _pageStateCancellationTokenSource!.Token;

        ApplyStaticLocalizedText();

        IReadOnlyList<UiLanguageOption> supportedUiLanguages = _appLocalizationService.GetSupportedLanguages();
        Task<IReadOnlyList<SupportedLanguageModel>> supportedMeaningLanguagesTask = LoadSupportedMeaningLanguagesAsync(cancellationToken);
        Task<UserLearningProfileModel> profileTask = _activeLearningProfileCacheService
            .GetCurrentProfileAsync(cancellationToken);
        Task<SeedDatabaseUpdateStatus> seedStatusTask = _seedDatabaseProvisioningService.GetUpdateStatusAsync(GetLocalDatabasePath(), cancellationToken);

        await Task.WhenAll(supportedMeaningLanguagesTask, profileTask, seedStatusTask).ConfigureAwait(true);

        IReadOnlyList<SupportedLanguageModel> supportedMeaningLanguages = await supportedMeaningLanguagesTask.ConfigureAwait(true);
        UserLearningProfileModel profile = await profileTask.ConfigureAwait(true);
        SeedDatabaseUpdateStatus seedDatabaseUpdateStatus = await seedStatusTask.ConfigureAwait(true);

        List<MeaningLanguageOption> primaryMeaningOptions = supportedMeaningLanguages
            .Select(CreateMeaningLanguageOption)
            .ToList();
        List<MeaningLanguageOption> secondaryMeaningOptions = BuildSecondaryMeaningOptions(
            supportedMeaningLanguages,
            profile.PreferredMeaningLanguage1);

        _isUpdatingSelection = true;

        try
        {
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

            ContentUpdateStatusSectionView.SectionValue = BuildContentUpdateStatus(seedDatabaseUpdateStatus);
            ContentUpdateDetailsSectionView.SectionValue = BuildContentUpdateDetails(seedDatabaseUpdateStatus);
            ContentUpdateDiagnosticsSectionView.SectionValue = BuildContentUpdateDiagnostics(seedDatabaseUpdateStatus, GetLocalDatabasePath());
            ApplySeedUpdateButton.IsEnabled = seedDatabaseUpdateStatus.IsSeedAvailable && !_isApplyingSeedUpdate;
            ApplySeedUpdateButton.Text = seedDatabaseUpdateStatus.IsUpdateAvailable
                ? AppStrings.SettingsContentUpdatesApplyButton
                : AppStrings.SettingsContentUpdatesAppliedButton;

            ApplyRemoteLoadingState();
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            await LoadAndApplyRemoteUpdateSectionsAsync(GetLocalDatabasePath(), cancellationToken).ConfigureAwait(true);
        }
        finally
        {
            _isUpdatingSelection = false;
        }
    }

    private async Task<IReadOnlyList<SupportedLanguageModel>> LoadSupportedMeaningLanguagesAsync(CancellationToken cancellationToken)
    {
        return (await _languageQueryService
                .GetActiveLanguagesAsync(cancellationToken)
                .ConfigureAwait(true))
            .Where(language => language.SupportsMeanings)
            .OrderBy(language => language.EnglishName)
            .ToArray();
    }

    private void ApplyRemoteLoadingState()
    {
        string loadingText = AppStrings.CommonStateLoading;

        RemoteContentUpdateStatusSectionView.SectionValue = loadingText;
        RemoteContentUpdateDetailsSectionView.SectionValue = loadingText;
        RemoteContentUpdateDiagnosticsSectionView.SectionValue = loadingText;
        RemoteContentUpdateHistorySectionView.SectionValue = loadingText;
        CatalogAreaUpdateSectionView.SectionValue = loadingText;
        CefrA1UpdateSectionView.SectionValue = loadingText;
        CefrA2UpdateSectionView.SectionValue = loadingText;
        CefrB1UpdateSectionView.SectionValue = loadingText;
        CefrB2UpdateSectionView.SectionValue = loadingText;
        CefrC1UpdateSectionView.SectionValue = loadingText;
        CefrC2UpdateSectionView.SectionValue = loadingText;

        ApplyRemoteUpdateButton.IsEnabled = false;
        ApplyCatalogAreaUpdateButton.IsEnabled = false;
        ApplyCefrA1UpdateButton.IsEnabled = false;
        ApplyCefrA2UpdateButton.IsEnabled = false;
        ApplyCefrB1UpdateButton.IsEnabled = false;
        ApplyCefrB2UpdateButton.IsEnabled = false;
        ApplyCefrC1UpdateButton.IsEnabled = false;
        ApplyCefrC2UpdateButton.IsEnabled = false;
    }

    private async Task LoadAndApplyRemoteUpdateSectionsAsync(string localDatabasePath, CancellationToken cancellationToken)
    {
        Task<RemoteContentUpdateStatus> remoteContentStatusTask = _remoteContentUpdateService.GetUpdateStatusAsync(localDatabasePath, cancellationToken);
        Task<RemoteContentUpdateStatus> catalogAreaStatusTask = _remoteContentUpdateService.GetAreaUpdateStatusAsync(localDatabasePath, "catalog", cancellationToken);
        Task<IReadOnlyList<RemoteContentUpdateHistoryEntry>> remoteUpdateHistoryTask = _remoteContentUpdateService.GetRecentUpdateHistoryAsync(cancellationToken);

        await Task.WhenAll(remoteContentStatusTask, catalogAreaStatusTask, remoteUpdateHistoryTask).ConfigureAwait(true);

        RemoteContentUpdateStatus remoteContentUpdateStatus = await remoteContentStatusTask.ConfigureAwait(true);
        RemoteContentUpdateStatus catalogAreaUpdateStatus = await catalogAreaStatusTask.ConfigureAwait(true);
        IReadOnlyList<RemoteContentUpdateHistoryEntry> remoteUpdateHistory = await remoteUpdateHistoryTask.ConfigureAwait(true);

        RemoteContentUpdateStatusSectionView.SectionValue = BuildRemoteContentUpdateStatus(remoteContentUpdateStatus);
        RemoteContentUpdateDetailsSectionView.SectionValue = BuildRemoteContentUpdateDetails(remoteContentUpdateStatus);
        RemoteContentUpdateDiagnosticsSectionView.SectionValue = BuildRemoteContentUpdateDiagnostics(remoteContentUpdateStatus);
        RemoteContentUpdateHistorySectionView.SectionValue = BuildRemoteContentUpdateHistory(remoteUpdateHistory);
        ApplyRemoteUpdateButton.IsEnabled = remoteContentUpdateStatus.IsRemoteConfigured && remoteContentUpdateStatus.IsServerReachable && !_isApplyingRemoteUpdate;
        ApplyRemoteUpdateButton.Text = remoteContentUpdateStatus.IsUpdateAvailable
            ? AppStrings.SettingsRemoteContentUpdatesApplyButton
            : AppStrings.SettingsRemoteContentUpdatesAppliedButton;

        CatalogAreaUpdateSectionView.SectionValue = BuildRemoteScopeSummary(catalogAreaUpdateStatus);
        ApplyCatalogAreaUpdateButton.IsEnabled = catalogAreaUpdateStatus.IsRemoteConfigured && catalogAreaUpdateStatus.IsServerReachable && catalogAreaUpdateStatus.IsUpdateAvailable && !_isApplyingRemoteUpdate;
        ApplyCatalogAreaUpdateButton.Text = BuildRemoteScopeButtonText(catalogAreaUpdateStatus, AppStrings.SettingsRemoteCatalogAreaTitle);

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        Dictionary<string, RemoteContentUpdateStatus> cefrUpdateStatuses = await LoadCefrUpdateStatusesAsync(localDatabasePath, cancellationToken).ConfigureAwait(true);
        BindCefrUpdateSection(CefrA1UpdateSectionView, ApplyCefrA1UpdateButton, "A1", cefrUpdateStatuses["A1"]);
        BindCefrUpdateSection(CefrA2UpdateSectionView, ApplyCefrA2UpdateButton, "A2", cefrUpdateStatuses["A2"]);
        BindCefrUpdateSection(CefrB1UpdateSectionView, ApplyCefrB1UpdateButton, "B1", cefrUpdateStatuses["B1"]);
        BindCefrUpdateSection(CefrB2UpdateSectionView, ApplyCefrB2UpdateButton, "B2", cefrUpdateStatuses["B2"]);
        BindCefrUpdateSection(CefrC1UpdateSectionView, ApplyCefrC1UpdateButton, "C1", cefrUpdateStatuses["C1"]);
        BindCefrUpdateSection(CefrC2UpdateSectionView, ApplyCefrC2UpdateButton, "C2", cefrUpdateStatuses["C2"]);
    }

    private async void OnApplyRemoteUpdateButtonClicked(object? sender, EventArgs e)
    {
        await ApplyScopedRemoteUpdateAsync(
                AppStrings.SettingsRemoteFullDatabaseTitle,
                () => _remoteContentUpdateService.ApplyFullUpdateAsync(GetLocalDatabasePath(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    private async void OnApplyCatalogAreaUpdateButtonClicked(object? sender, EventArgs e)
    {
        await ApplyScopedRemoteUpdateAsync(
                AppStrings.SettingsRemoteCatalogAreaTitle,
                () => _remoteContentUpdateService.ApplyAreaUpdateAsync(GetLocalDatabasePath(), "catalog", CancellationToken.None))
            .ConfigureAwait(true);
    }

    private async void OnApplyCefrLevelUpdateButtonClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not string cefrLevel || string.IsNullOrWhiteSpace(cefrLevel))
        {
            return;
        }

        await ApplyScopedRemoteUpdateAsync(
                cefrLevel,
                () => _remoteContentUpdateService.ApplyCefrUpdateAsync(GetLocalDatabasePath(), cefrLevel, CancellationToken.None))
            .ConfigureAwait(true);
    }

    private async void OnApplySeedUpdateButtonClicked(object? sender, EventArgs e)
    {
        if (_isApplyingSeedUpdate)
        {
            return;
        }

        _isApplyingSeedUpdate = true;
        ApplySeedUpdateButton.IsEnabled = false;
        ApplySeedUpdateButton.Text = AppStrings.SettingsContentUpdatesApplyingButton;

        try
        {
            SeedDatabaseUpdateResult result = await _seedDatabaseProvisioningService
                .ApplySeedUpdateAsync(GetLocalDatabasePath(), CancellationToken.None)
                .ConfigureAwait(true);

            if (!result.IsSuccess)
            {
                await DisplayAlertAsync(
                        AppStrings.SettingsContentUpdatesFailedTitle,
                        string.Format(AppStrings.SettingsContentUpdatesFailedMessageFormat, result.ErrorMessage ?? AppStrings.SettingsContentUpdatesUnavailableStatus),
                        AppStrings.SettingsContentUpdatesDismissButton)
                    .ConfigureAwait(true);
            }
            else if (result.AppliedChanges)
            {
                _browseAccelerationService.ResetCaches();

                await DisplayAlertAsync(
                        AppStrings.SettingsContentUpdatesCompletedTitle,
                        string.Format(AppStrings.SettingsContentUpdatesCompletedMessageFormat, result.ImportedPackages, result.ImportedWords),
                        AppStrings.SettingsContentUpdatesDismissButton)
                    .ConfigureAwait(true);
            }
            else
            {
                await DisplayAlertAsync(
                        AppStrings.SettingsContentUpdatesUpToDateTitle,
                        AppStrings.SettingsContentUpdatesUpToDateMessage,
                        AppStrings.SettingsContentUpdatesDismissButton)
                    .ConfigureAwait(true);
            }

            await RebuildPageStateAsync().ConfigureAwait(true);
        }
        finally
        {
            _isApplyingSeedUpdate = false;
        }
    }

    private async Task<Dictionary<string, RemoteContentUpdateStatus>> LoadCefrUpdateStatusesAsync(string localDatabasePath, CancellationToken cancellationToken)
    {
        Task<RemoteContentUpdateStatus>[] statusTasks = CefrLevels
            .Select(cefrLevel => _remoteContentUpdateService.GetCefrUpdateStatusAsync(localDatabasePath, cefrLevel, cancellationToken))
            .ToArray();

        RemoteContentUpdateStatus[] statuses = await Task.WhenAll(statusTasks).ConfigureAwait(true);
        return CefrLevels.Zip(statuses, static (cefrLevel, status) => new KeyValuePair<string, RemoteContentUpdateStatus>(cefrLevel, status))
            .ToDictionary();
    }

    private void BindCefrUpdateSection(
        DetailSectionView detailSectionView,
        Button button,
        string cefrLevel,
        RemoteContentUpdateStatus status)
    {
        detailSectionView.SectionValue = BuildRemoteScopeSummary(status);
        button.IsEnabled = status.IsRemoteConfigured && status.IsServerReachable && status.IsUpdateAvailable && !_isApplyingRemoteUpdate;
        button.Text = BuildRemoteScopeButtonText(status, cefrLevel);
    }

    private async Task ApplyScopedRemoteUpdateAsync(
        string scopeDisplayName,
        Func<Task<RemoteContentUpdateResult>> applyAction)
    {
        if (_isApplyingRemoteUpdate)
        {
            return;
        }

        _isApplyingRemoteUpdate = true;
        ApplyRemoteUpdateButton.IsEnabled = false;
        ApplyCatalogAreaUpdateButton.IsEnabled = false;
        ApplyCefrA1UpdateButton.IsEnabled = false;
        ApplyCefrA2UpdateButton.IsEnabled = false;
        ApplyCefrB1UpdateButton.IsEnabled = false;
        ApplyCefrB2UpdateButton.IsEnabled = false;
        ApplyCefrC1UpdateButton.IsEnabled = false;
        ApplyCefrC2UpdateButton.IsEnabled = false;

        try
        {
            RemoteContentUpdateResult result = await applyAction().ConfigureAwait(true);

            if (!result.IsSuccess)
            {
                await DisplayAlertAsync(
                        AppStrings.SettingsRemoteContentUpdatesFailedTitle,
                        string.Format(AppStrings.SettingsRemoteContentScopeFailedMessageFormat, scopeDisplayName, result.ErrorMessage ?? AppStrings.SettingsRemoteContentUpdatesUnavailableStatus),
                        AppStrings.SettingsContentUpdatesDismissButton)
                    .ConfigureAwait(true);
            }
            else if (result.AppliedChanges)
            {
                _browseAccelerationService.ResetCaches();

                await DisplayAlertAsync(
                        AppStrings.SettingsRemoteContentUpdatesCompletedTitle,
                        string.Format(AppStrings.SettingsRemoteContentScopeCompletedMessageFormat, scopeDisplayName, result.AppliedVersion, result.ImportedWords),
                        AppStrings.SettingsContentUpdatesDismissButton)
                    .ConfigureAwait(true);
            }
            else
            {
                await DisplayAlertAsync(
                        AppStrings.SettingsRemoteContentUpdatesUpToDateTitle,
                        string.Format(AppStrings.SettingsRemoteContentScopeUpToDateMessageFormat, scopeDisplayName),
                        AppStrings.SettingsContentUpdatesDismissButton)
                    .ConfigureAwait(true);
            }

            await RebuildPageStateAsync().ConfigureAwait(true);
        }
        finally
        {
            _isApplyingRemoteUpdate = false;
        }
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

        await _activeLearningProfileCacheService
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

        await _activeLearningProfileCacheService
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

    private static string GetLocalDatabasePath()
    {
        return Path.Combine(FileSystem.Current.AppDataDirectory, "darwin-lingua.db");
    }

    private void ResetPageStateRequest()
    {
        CancelPageStateRequest();
        _pageStateCancellationTokenSource = new CancellationTokenSource();
    }

    private void CancelPageStateRequest()
    {
        if (_pageStateCancellationTokenSource is null)
        {
            return;
        }

        _pageStateCancellationTokenSource.Cancel();
        _pageStateCancellationTokenSource.Dispose();
        _pageStateCancellationTokenSource = null;
    }

    private static string BuildContentUpdateStatus(SeedDatabaseUpdateStatus seedDatabaseUpdateStatus)
    {
        ArgumentNullException.ThrowIfNull(seedDatabaseUpdateStatus);

        if (!seedDatabaseUpdateStatus.IsSeedAvailable)
        {
            return AppStrings.SettingsContentUpdatesUnavailableStatus;
        }

        return seedDatabaseUpdateStatus.IsUpdateAvailable
            ? AppStrings.SettingsContentUpdatesAvailableStatus
            : AppStrings.SettingsContentUpdatesCurrentStatus;
    }

    private static string BuildContentUpdateDetails(SeedDatabaseUpdateStatus seedDatabaseUpdateStatus)
    {
        ArgumentNullException.ThrowIfNull(seedDatabaseUpdateStatus);

        if (!seedDatabaseUpdateStatus.IsSeedAvailable)
        {
            return AppStrings.SettingsContentUpdatesUnavailableDetails;
        }

        return seedDatabaseUpdateStatus.IsUpdateAvailable
            ? string.Format(
                AppStrings.SettingsContentUpdatesAvailableDetailsFormat,
                seedDatabaseUpdateStatus.PendingPackageCount,
                seedDatabaseUpdateStatus.PendingWordCount)
            : AppStrings.SettingsContentUpdatesCurrentDetails;
    }

    private static string BuildContentUpdateDiagnostics(SeedDatabaseUpdateStatus seedDatabaseUpdateStatus, string databasePath)
    {
        ArgumentNullException.ThrowIfNull(seedDatabaseUpdateStatus);
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        if (!seedDatabaseUpdateStatus.IsSeedAvailable)
        {
            return AppStrings.SettingsContentUpdatesUnavailableDiagnostics;
        }

        string seedSignature = BuildSignatureDisplay(seedDatabaseUpdateStatus.SeedSignature);
        string appliedSignature = BuildSignatureDisplay(seedDatabaseUpdateStatus.AppliedSignature);
        string lastAppliedAt = seedDatabaseUpdateStatus.LastAppliedAtUtc?.ToLocalTime().ToString("g")
            ?? AppStrings.SettingsContentUpdatesNeverAppliedValue;
        string lastAppliedSummary = string.Format(
            AppStrings.SettingsContentUpdatesLastAppliedSummaryFormat,
            seedDatabaseUpdateStatus.LastAppliedPackageCount,
            seedDatabaseUpdateStatus.LastAppliedWordCount);

        return string.Join(
            Environment.NewLine,
            string.Format(AppStrings.SettingsContentUpdatesDatabasePathFormat, databasePath),
            string.Format(AppStrings.SettingsContentUpdatesSeedSignatureFormat, seedSignature),
            string.Format(AppStrings.SettingsContentUpdatesAppliedSignatureFormat, appliedSignature),
            string.Format(AppStrings.SettingsContentUpdatesLastAppliedAtFormat, lastAppliedAt),
            lastAppliedSummary);
    }

    private static string BuildSignatureDisplay(string? signature)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            return AppStrings.SettingsContentUpdatesUnknownSignatureValue;
        }

        return signature.Length <= 12
            ? signature
            : signature[..12];
    }

    private static string BuildRemoteContentUpdateStatus(RemoteContentUpdateStatus remoteContentUpdateStatus)
    {
        ArgumentNullException.ThrowIfNull(remoteContentUpdateStatus);

        if (!remoteContentUpdateStatus.IsRemoteConfigured)
        {
            return AppStrings.SettingsRemoteContentUpdatesUnavailableStatus;
        }

        if (!remoteContentUpdateStatus.IsServerReachable)
        {
            return AppStrings.SettingsRemoteContentUpdatesUnreachableStatus;
        }

        return remoteContentUpdateStatus.IsUpdateAvailable
            ? AppStrings.SettingsRemoteContentUpdatesAvailableStatus
            : AppStrings.SettingsRemoteContentUpdatesCurrentStatus;
    }

    private static string BuildRemoteContentUpdateDetails(RemoteContentUpdateStatus remoteContentUpdateStatus)
    {
        ArgumentNullException.ThrowIfNull(remoteContentUpdateStatus);

        if (!remoteContentUpdateStatus.IsRemoteConfigured)
        {
            return AppStrings.SettingsRemoteContentUpdatesUnavailableDetails;
        }

        if (!remoteContentUpdateStatus.IsServerReachable)
        {
            return AppStrings.SettingsRemoteContentUpdatesUnreachableDetails;
        }

        return remoteContentUpdateStatus.IsUpdateAvailable
            ? string.Format(
                AppStrings.SettingsRemoteContentUpdatesAvailableDetailsFormat,
                remoteContentUpdateStatus.RemoteVersion,
                remoteContentUpdateStatus.PendingWordCount)
            : string.Format(
                AppStrings.SettingsRemoteContentUpdatesCurrentDetailsFormat,
                string.IsNullOrWhiteSpace(remoteContentUpdateStatus.LocalVersion)
                    ? AppStrings.SettingsContentUpdatesNeverAppliedValue
                    : remoteContentUpdateStatus.LocalVersion);
    }

    private static string BuildRemoteContentUpdateDiagnostics(RemoteContentUpdateStatus remoteContentUpdateStatus)
    {
        ArgumentNullException.ThrowIfNull(remoteContentUpdateStatus);

        string scopeKey = string.IsNullOrWhiteSpace(remoteContentUpdateStatus.ScopeKey)
            ? AppStrings.SettingsContentUpdatesUnknownSignatureValue
            : remoteContentUpdateStatus.ScopeKey;
        string contentArea = string.IsNullOrWhiteSpace(remoteContentUpdateStatus.ContentAreaKey)
            ? AppStrings.SettingsContentUpdatesUnknownSignatureValue
            : remoteContentUpdateStatus.ContentAreaKey;
        string sliceKey = string.IsNullOrWhiteSpace(remoteContentUpdateStatus.SliceKey)
            ? AppStrings.SettingsContentUpdatesUnknownSignatureValue
            : remoteContentUpdateStatus.SliceKey;
        string packageType = string.IsNullOrWhiteSpace(remoteContentUpdateStatus.PackageType)
            ? AppStrings.SettingsContentUpdatesUnknownSignatureValue
            : remoteContentUpdateStatus.PackageType;
        string localPackage = string.IsNullOrWhiteSpace(remoteContentUpdateStatus.LocalPackageId)
            ? AppStrings.SettingsContentUpdatesNeverAppliedValue
            : remoteContentUpdateStatus.LocalPackageId;
        string remotePackage = string.IsNullOrWhiteSpace(remoteContentUpdateStatus.RemotePackageId)
            ? AppStrings.SettingsContentUpdatesUnknownSignatureValue
            : remoteContentUpdateStatus.RemotePackageId;
        string localChecksum = BuildSignatureDisplay(remoteContentUpdateStatus.LocalChecksum);
        string remoteChecksum = BuildSignatureDisplay(remoteContentUpdateStatus.RemoteChecksum);
        string localSchemaVersion = remoteContentUpdateStatus.LocalSchemaVersion <= 0
            ? AppStrings.SettingsContentUpdatesUnknownSignatureValue
            : remoteContentUpdateStatus.LocalSchemaVersion.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string remoteSchemaVersion = remoteContentUpdateStatus.RemoteSchemaVersion <= 0
            ? AppStrings.SettingsContentUpdatesUnknownSignatureValue
            : remoteContentUpdateStatus.RemoteSchemaVersion.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string remoteManifestGeneratedAt = remoteContentUpdateStatus.RemoteManifestGeneratedAtUtc?.ToLocalTime().ToString("g")
            ?? AppStrings.SettingsContentUpdatesUnknownSignatureValue;
        string lastUpdatedAt = remoteContentUpdateStatus.LastSuccessfulUpdateAtUtc?.ToLocalTime().ToString("g")
            ?? AppStrings.SettingsContentUpdatesNeverAppliedValue;
        string lastFailure = string.IsNullOrWhiteSpace(remoteContentUpdateStatus.LastFailureMessage)
            ? AppStrings.SettingsRemoteContentUpdatesNoFailureValue
            : remoteContentUpdateStatus.LastFailureMessage;

        List<string> diagnosticsLines =
        [
            string.Format(AppStrings.SettingsRemoteContentUpdatesScopeFormat, scopeKey),
            string.Format(AppStrings.SettingsRemoteContentUpdatesContentAreaFormat, contentArea),
            string.Format(AppStrings.SettingsRemoteContentUpdatesSliceFormat, sliceKey),
            string.Format(AppStrings.SettingsRemoteContentUpdatesPackageTypeFormat, packageType),
            string.Format(AppStrings.SettingsRemoteContentUpdatesLocalPackageFormat, localPackage),
            string.Format(AppStrings.SettingsRemoteContentUpdatesRemotePackageFormat, remotePackage),
            string.Format(AppStrings.SettingsRemoteContentUpdatesLocalChecksumFormat, localChecksum),
            string.Format(AppStrings.SettingsRemoteContentUpdatesRemoteChecksumFormat, remoteChecksum),
            string.Format(AppStrings.SettingsRemoteContentUpdatesLocalVersionFormat, string.IsNullOrWhiteSpace(remoteContentUpdateStatus.LocalVersion) ? AppStrings.SettingsContentUpdatesNeverAppliedValue : remoteContentUpdateStatus.LocalVersion),
            string.Format(AppStrings.SettingsRemoteContentUpdatesRemoteVersionFormat, string.IsNullOrWhiteSpace(remoteContentUpdateStatus.RemoteVersion) ? AppStrings.SettingsContentUpdatesUnknownSignatureValue : remoteContentUpdateStatus.RemoteVersion),
            string.Format(AppStrings.SettingsRemoteContentUpdatesLocalSchemaVersionFormat, localSchemaVersion),
            string.Format(AppStrings.SettingsRemoteContentUpdatesRemoteSchemaVersionFormat, remoteSchemaVersion),
            string.Format(AppStrings.SettingsRemoteContentUpdatesManifestGeneratedAtFormat, remoteManifestGeneratedAt),
            string.Format(AppStrings.SettingsRemoteContentUpdatesLastUpdatedAtFormat, lastUpdatedAt),
            string.Format(AppStrings.SettingsRemoteContentUpdatesLastFailureFormat, lastFailure),
        ];

#if ANDROID
        diagnosticsLines.AddRange(BuildAndroidWorkerDiagnostics());
#endif

        return string.Join(
            Environment.NewLine,
            diagnosticsLines);
    }

#if ANDROID
    private static IEnumerable<string> BuildAndroidWorkerDiagnostics()
    {
        string workerLastProbeAt = Preferences.Default.Get<string?>(RemoteContentUpdateProbeWorker.LastWorkerProbeAtPreferenceKey, null)
            is string rawProbeAt && DateTimeOffset.TryParse(rawProbeAt, out DateTimeOffset parsedProbeAt)
                ? parsedProbeAt.ToLocalTime().ToString("g")
                : AppStrings.SettingsContentUpdatesNeverAppliedValue;
        string workerOutcome = Preferences.Default.Get(RemoteContentUpdateProbeWorker.LastWorkerOutcomePreferenceKey, AppStrings.SettingsContentUpdatesUnknownSignatureValue);
        string workerPendingPackage = Preferences.Default.Get(RemoteContentUpdateProbeWorker.LastWorkerPendingPackageIdPreferenceKey, string.Empty);
        string workerFailure = Preferences.Default.Get(RemoteContentUpdateProbeWorker.LastWorkerFailureMessagePreferenceKey, string.Empty);
        bool workerServerReachable = Preferences.Default.Get(RemoteContentUpdateProbeWorker.LastWorkerServerReachablePreferenceKey, false);
        bool workerUpdateAvailable = Preferences.Default.Get(RemoteContentUpdateProbeWorker.LastWorkerUpdateAvailablePreferenceKey, false);

        yield return $"Android worker last probe: {workerLastProbeAt}";
        yield return $"Android worker outcome: {workerOutcome}";
        yield return $"Android worker reachable: {workerServerReachable}";
        yield return $"Android worker update available: {workerUpdateAvailable}";

        if (!string.IsNullOrWhiteSpace(workerPendingPackage))
        {
            yield return $"Android worker pending package: {workerPendingPackage}";
        }

        if (!string.IsNullOrWhiteSpace(workerFailure))
        {
            yield return $"Android worker failure: {workerFailure}";
        }
    }
#endif

    private static string BuildRemoteContentUpdateHistory(IReadOnlyList<RemoteContentUpdateHistoryEntry> historyEntries)
    {
        ArgumentNullException.ThrowIfNull(historyEntries);

        if (historyEntries.Count == 0)
        {
            return AppStrings.SettingsRemoteContentUpdatesHistoryEmpty;
        }

        return string.Join(
            Environment.NewLine,
            historyEntries
                .Take(5)
                .Select(BuildRemoteContentUpdateHistoryLine));
    }

    private static string BuildRemoteContentUpdateHistoryLine(RemoteContentUpdateHistoryEntry entry)
    {
        string occurredAt = entry.OccurredAtUtc.ToLocalTime().ToString("g");
        string scopeName = BuildRemoteScopeDisplayName(entry.ScopeKey);

        if (!entry.IsSuccess)
        {
            return string.Format(
                AppStrings.SettingsRemoteContentUpdatesHistoryFailedFormat,
                occurredAt,
                scopeName,
                string.IsNullOrWhiteSpace(entry.ErrorMessage) ? AppStrings.SettingsRemoteContentUpdatesUnavailableStatus : entry.ErrorMessage);
        }

        if (!entry.AppliedChanges)
        {
            return string.Format(
                AppStrings.SettingsRemoteContentUpdatesHistoryCurrentFormat,
                occurredAt,
                scopeName);
        }

        return string.Format(
            AppStrings.SettingsRemoteContentUpdatesHistoryAppliedFormat,
            occurredAt,
            scopeName,
            string.IsNullOrWhiteSpace(entry.Version) ? AppStrings.SettingsContentUpdatesUnknownSignatureValue : entry.Version,
            entry.ImportedWords);
    }

    private static string BuildRemoteScopeDisplayName(string scopeKey)
    {
        if (string.Equals(scopeKey, "all-full", StringComparison.OrdinalIgnoreCase))
        {
            return AppStrings.SettingsRemoteFullDatabaseTitle;
        }

        if (string.Equals(scopeKey, "catalog-full", StringComparison.OrdinalIgnoreCase))
        {
            return AppStrings.SettingsRemoteCatalogAreaTitle;
        }

        if (scopeKey.StartsWith("catalog-cefr-", StringComparison.OrdinalIgnoreCase))
        {
            return scopeKey["catalog-cefr-".Length..].ToUpperInvariant();
        }

        return scopeKey;
    }

    private static string BuildRemoteScopeSummary(RemoteContentUpdateStatus remoteContentUpdateStatus)
    {
        return string.Join(
            Environment.NewLine,
            BuildRemoteContentUpdateStatus(remoteContentUpdateStatus),
            BuildRemoteContentUpdateDetails(remoteContentUpdateStatus));
    }

    private static string BuildRemoteScopeButtonText(RemoteContentUpdateStatus remoteContentUpdateStatus, string scopeDisplayName)
    {
        ArgumentNullException.ThrowIfNull(remoteContentUpdateStatus);
        ArgumentException.ThrowIfNullOrWhiteSpace(scopeDisplayName);

        if (!remoteContentUpdateStatus.IsRemoteConfigured || !remoteContentUpdateStatus.IsServerReachable)
        {
            return string.Format(AppStrings.SettingsRemoteContentScopeApplyButtonFormat, scopeDisplayName);
        }

        return remoteContentUpdateStatus.IsUpdateAvailable
            ? string.Format(AppStrings.SettingsRemoteContentScopeApplyButtonFormat, scopeDisplayName)
            : string.Format(AppStrings.SettingsRemoteContentScopeCurrentButtonFormat, scopeDisplayName);
    }

    /// <summary>
    /// Represents a picker option for meaning-language selection.
    /// </summary>
    private sealed record MeaningLanguageOption(string? LanguageCode, string DisplayName);
}

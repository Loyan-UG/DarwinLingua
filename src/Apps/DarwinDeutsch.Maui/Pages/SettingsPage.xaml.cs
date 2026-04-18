using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Controls;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinDeutsch.Maui.Services.Browse;
using DarwinDeutsch.Maui.Services.Storage;
using DarwinDeutsch.Maui.Services.UI;
using DarwinDeutsch.Maui.Services.Updates;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Localization.Application.Abstractions;
using DarwinLingua.Localization.Application.Models;
using Syncfusion.Maui.Toolkit.Buttons;

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
    private readonly IPopupDialogService _popupDialogService;
    private readonly Dictionary<string, RemoteContentUpdateStatus> _cefrUpdateStatuses = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource? _pageStateCancellationTokenSource;
    private bool _isUpdatingSelection;
    private bool _isApplyingRemoteUpdate;
    private bool _isApplyingSeedUpdate;
    private string _selectedCefrLevel = "A1";

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
        ILanguageQueryService languageQueryService,
        IPopupDialogService popupDialogService)
    {
        ArgumentNullException.ThrowIfNull(appLocalizationService);
        ArgumentNullException.ThrowIfNull(browseAccelerationService);
        ArgumentNullException.ThrowIfNull(remoteContentUpdateService);
        ArgumentNullException.ThrowIfNull(seedDatabaseProvisioningService);
        ArgumentNullException.ThrowIfNull(activeLearningProfileCacheService);
        ArgumentNullException.ThrowIfNull(languageQueryService);
        ArgumentNullException.ThrowIfNull(popupDialogService);

        InitializeComponent();

        _appLocalizationService = appLocalizationService;
        _browseAccelerationService = browseAccelerationService;
        _remoteContentUpdateService = remoteContentUpdateService;
        _seedDatabaseProvisioningService = seedDatabaseProvisioningService;
        _activeLearningProfileCacheService = activeLearningProfileCacheService;
        _languageQueryService = languageQueryService;
        _popupDialogService = popupDialogService;
        _appLocalizationService.CultureChanged += OnCultureChanged;
        CefrLevelChipGroup.ItemsSource = CefrLevels.Select(static level => new CefrLevelChipItem(level)).ToArray();
        CefrLevelChipGroup.SelectedItem = ((IEnumerable<CefrLevelChipItem>)CefrLevelChipGroup.ItemsSource)
            .First(item => string.Equals(item.Level, _selectedCefrLevel, StringComparison.OrdinalIgnoreCase));

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
        catch (Exception)
        {
            ApplyPageLoadFailureState();
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
            catch (Exception)
            {
                ApplyPageLoadFailureState();
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
        AboutSectionLabel.Text = AppStrings.SettingsAppInfoSectionLabel;
        AboutSummaryLabel.Text = AppStrings.SettingsAboutSummary;
        OpenAboutButton.Text = AppStrings.SettingsAboutButton;
        LanguageInputLayout.Hint = AppStrings.SettingsUiLanguageLabel;
        PrimaryMeaningLanguageInputLayout.Hint = AppStrings.SettingsPrimaryMeaningLanguageLabel;
        SecondaryMeaningLanguageInputLayout.Hint = AppStrings.SettingsSecondaryMeaningLanguageLabel;
        ContentUpdatesSectionLabel.Text = AppStrings.SettingsContentUpdatesSectionLabel;
        RemoteContentUpdatesSectionLabel.Text = AppStrings.SettingsRemoteContentUpdatesSectionLabel;
        CatalogAreaUpdatesSectionLabel.Text = AppStrings.SettingsRemoteCatalogAreaSectionLabel;
        CefrLevelUpdatesSectionLabel.Text = AppStrings.SettingsRemoteCefrLevelsSectionLabel;
        PackagedSeedUpdatesSectionLabel.Text = AppStrings.SettingsPackagedSeedUpdatesSectionLabel;
        ContentUpdateStatusSectionView.SectionTitle = AppStrings.SettingsContentUpdatesStatusLabel;
        ContentUpdateDetailsSectionView.SectionTitle = AppStrings.SettingsContentUpdatesDetailsLabel;
        RemoteContentUpdateStatusSectionView.SectionTitle = AppStrings.SettingsContentUpdatesStatusLabel;
        RemoteContentUpdateDetailsSectionView.SectionTitle = AppStrings.SettingsContentUpdatesDetailsLabel;
        CatalogAreaUpdateSectionView.SectionTitle = AppStrings.SettingsRemoteCatalogAreaTitle;
        SelectedCefrUpdateSectionView.SectionTitle = _selectedCefrLevel;
        SetButtonPresentation(ApplyRemoteUpdateButton, AppStrings.SettingsRemoteContentUpdatesApplyButton);
        SetButtonPresentation(ApplyCatalogAreaUpdateButton, string.Format(AppStrings.SettingsRemoteContentScopeApplyButtonFormat, AppStrings.SettingsRemoteCatalogAreaTitle));
        SetButtonPresentation(ApplySelectedCefrLevelUpdateButton, string.Format(AppStrings.SettingsRemoteContentScopeApplyButtonFormat, _selectedCefrLevel));
        SetButtonPresentation(ApplySeedUpdateButton, AppStrings.SettingsContentUpdatesApplyButton);
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

            ContentUpdateStatusSectionView.SectionValue = BuildContentUpdateStatus(seedDatabaseUpdateStatus);
            ContentUpdateDetailsSectionView.SectionValue = BuildContentUpdateDetails(seedDatabaseUpdateStatus);
            ApplySeedUpdateButton.IsEnabled = seedDatabaseUpdateStatus.IsSeedAvailable && !_isApplyingSeedUpdate;
            SetButtonPresentation(
                ApplySeedUpdateButton,
                seedDatabaseUpdateStatus.IsUpdateAvailable
                    ? AppStrings.SettingsContentUpdatesApplyButton
                    : AppStrings.SettingsContentUpdatesAppliedButton);

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
        CatalogAreaUpdateSectionView.SectionValue = loadingText;
        SelectedCefrUpdateSectionView.SectionValue = loadingText;
        ApplyRemoteUpdateButton.IsEnabled = false;
        ApplyCatalogAreaUpdateButton.IsEnabled = false;
        ApplySelectedCefrLevelUpdateButton.IsEnabled = false;
    }

    private void ApplyPageLoadFailureState()
    {
        string errorText = AppStrings.CommonStateError;

        ContentUpdateStatusSectionView.SectionValue = errorText;
        ContentUpdateDetailsSectionView.SectionValue = errorText;
        RemoteContentUpdateStatusSectionView.SectionValue = errorText;
        RemoteContentUpdateDetailsSectionView.SectionValue = errorText;
        CatalogAreaUpdateSectionView.SectionValue = errorText;
        SelectedCefrUpdateSectionView.SectionValue = errorText;

        ApplyRemoteUpdateButton.IsEnabled = false;
        ApplyCatalogAreaUpdateButton.IsEnabled = false;
        ApplySelectedCefrLevelUpdateButton.IsEnabled = false;
        ApplySeedUpdateButton.IsEnabled = false;
    }

    private async Task LoadAndApplyRemoteUpdateSectionsAsync(string localDatabasePath, CancellationToken cancellationToken)
    {
        Task<RemoteContentUpdateStatus> remoteContentStatusTask = _remoteContentUpdateService.GetUpdateStatusAsync(localDatabasePath, cancellationToken);
        Task<RemoteContentUpdateStatus> catalogAreaStatusTask = _remoteContentUpdateService.GetAreaUpdateStatusAsync(localDatabasePath, "catalog", cancellationToken);

        await Task.WhenAll(remoteContentStatusTask, catalogAreaStatusTask).ConfigureAwait(true);

        RemoteContentUpdateStatus remoteContentUpdateStatus = await remoteContentStatusTask.ConfigureAwait(true);
        RemoteContentUpdateStatus catalogAreaUpdateStatus = await catalogAreaStatusTask.ConfigureAwait(true);

        RemoteContentUpdateStatusSectionView.SectionValue = BuildRemoteContentUpdateStatus(remoteContentUpdateStatus);
        RemoteContentUpdateDetailsSectionView.SectionValue = BuildRemoteContentUpdateDetails(remoteContentUpdateStatus);
        ApplyRemoteUpdateButton.IsEnabled = remoteContentUpdateStatus.IsRemoteConfigured && remoteContentUpdateStatus.IsServerReachable && !_isApplyingRemoteUpdate;
        SetButtonPresentation(
            ApplyRemoteUpdateButton,
            remoteContentUpdateStatus.IsUpdateAvailable
                ? AppStrings.SettingsRemoteContentUpdatesApplyButton
                : AppStrings.SettingsRemoteContentUpdatesAppliedButton);

        CatalogAreaUpdateSectionView.SectionValue = BuildRemoteScopeSummary(catalogAreaUpdateStatus);
        ApplyCatalogAreaUpdateButton.IsEnabled = catalogAreaUpdateStatus.IsRemoteConfigured && catalogAreaUpdateStatus.IsServerReachable && catalogAreaUpdateStatus.IsUpdateAvailable && !_isApplyingRemoteUpdate;
        SetButtonPresentation(
            ApplyCatalogAreaUpdateButton,
            BuildRemoteScopeButtonText(catalogAreaUpdateStatus, AppStrings.SettingsRemoteCatalogAreaTitle));

        if (!catalogAreaUpdateStatus.IsRemoteConfigured || !catalogAreaUpdateStatus.IsServerReachable)
        {
            ApplyUnavailableCefrUpdateSections(catalogAreaUpdateStatus);
            return;
        }

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        Dictionary<string, RemoteContentUpdateStatus> cefrUpdateStatuses = await LoadCefrUpdateStatusesAsync(localDatabasePath, cancellationToken).ConfigureAwait(true);
        ApplyCefrUpdateStatuses(cefrUpdateStatuses);
    }

    private void ApplyUnavailableCefrUpdateSections(RemoteContentUpdateStatus baseStatus)
    {
        RemoteContentUpdateStatus unavailableStatus = baseStatus with
        {
            ScopeKey = "catalog-cefr-unavailable",
            SliceKey = string.Empty,
            PackageType = "catalog-cefr",
            IsUpdateAvailable = false,
            PendingWordCount = 0,
        };

        Dictionary<string, RemoteContentUpdateStatus> unavailableStatuses = CefrLevels
            .ToDictionary(static level => level, _ => unavailableStatus, StringComparer.OrdinalIgnoreCase);
        ApplyCefrUpdateStatuses(unavailableStatuses);
    }

    private async void OnApplyRemoteUpdateButtonClicked(object? sender, EventArgs e)
    {
        await ApplyScopedRemoteUpdateAsync(
                AppStrings.SettingsRemoteFullDatabaseTitle,
                () => _remoteContentUpdateService.ApplyFullUpdateAsync(GetLocalDatabasePath(), CancellationToken.None),
                ApplyRemoteUpdateButton,
                AppStrings.SettingsRemoteContentUpdatesApplyingButton)
            .ConfigureAwait(true);
    }

    private async void OnApplyCatalogAreaUpdateButtonClicked(object? sender, EventArgs e)
    {
        await ApplyScopedRemoteUpdateAsync(
                AppStrings.SettingsRemoteCatalogAreaTitle,
                () => _remoteContentUpdateService.ApplyAreaUpdateAsync(GetLocalDatabasePath(), "catalog", CancellationToken.None),
                ApplyCatalogAreaUpdateButton,
                string.Format(AppStrings.SettingsRemoteContentScopeApplyingButtonFormat, AppStrings.SettingsRemoteCatalogAreaTitle))
            .ConfigureAwait(true);
    }

    private async void OnApplySelectedCefrLevelUpdateButtonClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_selectedCefrLevel))
        {
            return;
        }

        await ApplyScopedRemoteUpdateAsync(
                _selectedCefrLevel,
                () => _remoteContentUpdateService.ApplyCefrUpdateAsync(GetLocalDatabasePath(), _selectedCefrLevel, CancellationToken.None),
                ApplySelectedCefrLevelUpdateButton,
                string.Format(AppStrings.SettingsRemoteContentScopeApplyingButtonFormat, _selectedCefrLevel))
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
        SetButtonPresentation(ApplySeedUpdateButton, AppStrings.SettingsContentUpdatesApplyingButton);

        try
        {
            SeedDatabaseUpdateResult result = await _seedDatabaseProvisioningService
                .ApplySeedUpdateAsync(GetLocalDatabasePath(), CancellationToken.None)
                .ConfigureAwait(true);

            if (!result.IsSuccess)
            {
                await _popupDialogService.ShowMessageAsync(
                        AppStrings.SettingsContentUpdatesFailedTitle,
                        string.Format(AppStrings.SettingsContentUpdatesFailedMessageFormat, result.ErrorMessage ?? AppStrings.SettingsContentUpdatesUnavailableStatus),
                        AppStrings.SettingsContentUpdatesDismissButton,
                        PopupDialogKind.Error)
                    .ConfigureAwait(true);
            }
            else if (result.AppliedChanges)
            {
                _browseAccelerationService.ResetCaches();

                await _popupDialogService.ShowMessageAsync(
                        AppStrings.SettingsContentUpdatesCompletedTitle,
                        string.Format(AppStrings.SettingsContentUpdatesCompletedMessageFormat, result.ImportedPackages, result.ImportedWords),
                        AppStrings.SettingsContentUpdatesDismissButton,
                        PopupDialogKind.Success)
                    .ConfigureAwait(true);
            }
            else
            {
                await _popupDialogService.ShowMessageAsync(
                        AppStrings.SettingsContentUpdatesUpToDateTitle,
                        AppStrings.SettingsContentUpdatesUpToDateMessage,
                        AppStrings.SettingsContentUpdatesDismissButton,
                        PopupDialogKind.Info)
                    .ConfigureAwait(true);
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception exception)
        {
            await _popupDialogService.ShowMessageAsync(
                    AppStrings.SettingsContentUpdatesFailedTitle,
                    string.Format(AppStrings.SettingsContentUpdatesFailedMessageFormat, exception.Message),
                    AppStrings.SettingsContentUpdatesDismissButton,
                    PopupDialogKind.Error)
                .ConfigureAwait(true);
        }
        finally
        {
            _isApplyingSeedUpdate = false;
        }

        try
        {
            await RefreshUpdateSectionsAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            await _popupDialogService.ShowMessageAsync(
                    AppStrings.SettingsContentUpdatesFailedTitle,
                    string.Format(AppStrings.SettingsContentUpdatesFailedMessageFormat, exception.Message),
                    AppStrings.SettingsContentUpdatesDismissButton,
                    PopupDialogKind.Error)
                .ConfigureAwait(true);
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

    private void ApplyCefrUpdateStatuses(IReadOnlyDictionary<string, RemoteContentUpdateStatus> cefrUpdateStatuses)
    {
        _cefrUpdateStatuses.Clear();

        foreach ((string cefrLevel, RemoteContentUpdateStatus status) in cefrUpdateStatuses)
        {
            _cefrUpdateStatuses[cefrLevel] = status;
        }

        BindSelectedCefrUpdateSection();
    }

    private void BindSelectedCefrUpdateSection()
    {
        SelectedCefrUpdateSectionView.SectionTitle = _selectedCefrLevel;

        if (!_cefrUpdateStatuses.TryGetValue(_selectedCefrLevel, out RemoteContentUpdateStatus? status))
        {
            SelectedCefrUpdateSectionView.SectionValue = AppStrings.CommonStateLoading;
            ApplySelectedCefrLevelUpdateButton.IsEnabled = false;
            SetButtonPresentation(
                ApplySelectedCefrLevelUpdateButton,
                string.Format(AppStrings.SettingsRemoteContentScopeApplyButtonFormat, _selectedCefrLevel));
            return;
        }

        SelectedCefrUpdateSectionView.SectionValue = BuildRemoteScopeSummary(status);
        ApplySelectedCefrLevelUpdateButton.IsEnabled = status.IsRemoteConfigured && status.IsServerReachable && status.IsUpdateAvailable && !_isApplyingRemoteUpdate;
        SetButtonPresentation(
            ApplySelectedCefrLevelUpdateButton,
            BuildRemoteScopeButtonText(status, _selectedCefrLevel));
    }

    private async Task ApplyScopedRemoteUpdateAsync(
        string scopeDisplayName,
        Func<Task<RemoteContentUpdateResult>> applyAction,
        SfButton targetButton,
        string busyButtonText)
    {
        if (_isApplyingRemoteUpdate)
        {
            return;
        }

        _isApplyingRemoteUpdate = true;
        ApplyRemoteUpdateButton.IsEnabled = false;
        ApplyCatalogAreaUpdateButton.IsEnabled = false;
        ApplySelectedCefrLevelUpdateButton.IsEnabled = false;
        SetButtonPresentation(targetButton, busyButtonText);

        try
        {
            RemoteContentUpdateResult result = await applyAction().ConfigureAwait(true);

            if (!result.IsSuccess)
            {
                await _popupDialogService.ShowMessageAsync(
                        AppStrings.SettingsRemoteContentUpdatesFailedTitle,
                        string.Format(AppStrings.SettingsRemoteContentScopeFailedMessageFormat, scopeDisplayName, result.ErrorMessage ?? AppStrings.SettingsRemoteContentUpdatesUnavailableStatus),
                        AppStrings.SettingsContentUpdatesDismissButton,
                        PopupDialogKind.Error)
                    .ConfigureAwait(true);
            }
            else if (result.AppliedChanges)
            {
                _browseAccelerationService.ResetCaches();

                await _popupDialogService.ShowMessageAsync(
                        AppStrings.SettingsRemoteContentUpdatesCompletedTitle,
                        string.Format(AppStrings.SettingsRemoteContentScopeCompletedMessageFormat, scopeDisplayName, result.AppliedVersion, result.ImportedWords),
                        AppStrings.SettingsContentUpdatesDismissButton,
                        PopupDialogKind.Success)
                    .ConfigureAwait(true);
            }
            else
            {
                await _popupDialogService.ShowMessageAsync(
                        AppStrings.SettingsRemoteContentUpdatesUpToDateTitle,
                        string.Format(AppStrings.SettingsRemoteContentScopeUpToDateMessageFormat, scopeDisplayName),
                        AppStrings.SettingsContentUpdatesDismissButton,
                        PopupDialogKind.Info)
                    .ConfigureAwait(true);
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception exception)
        {
            await _popupDialogService.ShowMessageAsync(
                    AppStrings.SettingsRemoteContentUpdatesFailedTitle,
                    string.Format(AppStrings.SettingsRemoteContentScopeFailedMessageFormat, scopeDisplayName, exception.Message),
                    AppStrings.SettingsContentUpdatesDismissButton,
                    PopupDialogKind.Error)
                .ConfigureAwait(true);
        }
        finally
        {
            _isApplyingRemoteUpdate = false;
            SetButtonPresentation(targetButton, busyButtonText);
        }

        try
        {
            await RefreshUpdateSectionsAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            await _popupDialogService.ShowMessageAsync(
                    AppStrings.SettingsRemoteContentUpdatesFailedTitle,
                    string.Format(AppStrings.SettingsRemoteContentScopeFailedMessageFormat, scopeDisplayName, exception.Message),
                    AppStrings.SettingsContentUpdatesDismissButton,
                    PopupDialogKind.Error)
                .ConfigureAwait(true);
        }
    }

    private async Task RefreshUpdateSectionsAsync()
    {
        string localDatabasePath = GetLocalDatabasePath();
        CancellationToken cancellationToken = _pageStateCancellationTokenSource?.Token ?? CancellationToken.None;

        SeedDatabaseUpdateStatus seedDatabaseUpdateStatus = await _seedDatabaseProvisioningService
            .GetUpdateStatusAsync(localDatabasePath, cancellationToken)
            .ConfigureAwait(true);

        ContentUpdateStatusSectionView.SectionValue = BuildContentUpdateStatus(seedDatabaseUpdateStatus);
        ContentUpdateDetailsSectionView.SectionValue = BuildContentUpdateDetails(seedDatabaseUpdateStatus);
        ApplySeedUpdateButton.IsEnabled = seedDatabaseUpdateStatus.IsSeedAvailable && !_isApplyingSeedUpdate;
        SetButtonPresentation(
            ApplySeedUpdateButton,
            seedDatabaseUpdateStatus.IsUpdateAvailable
                ? AppStrings.SettingsContentUpdatesApplyButton
                : AppStrings.SettingsContentUpdatesAppliedButton);

        ApplyRemoteLoadingState();
        await LoadAndApplyRemoteUpdateSectionsAsync(localDatabasePath, cancellationToken).ConfigureAwait(true);
    }

    private async void OnOpenAboutButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(AboutPage)).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void OnCefrLevelChipGroupSelectionChanged(object? sender, Syncfusion.Maui.Toolkit.Chips.SelectionChangedEventArgs e)
    {
        string? selectedLevel = e.AddedItem switch
        {
            CefrLevelChipItem chipItem => chipItem.Level,
            string level => level,
            _ => null,
        };

        if (string.IsNullOrWhiteSpace(selectedLevel))
        {
            return;
        }

        _selectedCefrLevel = selectedLevel;
        BindSelectedCefrUpdateSection();
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

    /// <summary>
    /// Represents a chip option for CEFR update selection.
    /// </summary>
    private sealed record CefrLevelChipItem(string Level);

    private static void SetButtonPresentation(SfButton button, string text)
    {
        button.Text = text;
    }
}

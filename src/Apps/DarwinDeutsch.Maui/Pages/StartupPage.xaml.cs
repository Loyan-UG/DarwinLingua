using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinDeutsch.Maui.Services.Startup;
using DarwinDeutsch.Maui.Services.UI;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Shows a lightweight startup surface while local initialization completes.
/// </summary>
public partial class StartupPage : ContentPage
{
    private readonly IAppStartupInitializationService _appStartupInitializationService;
    private readonly IAppLocalizationService _appLocalizationService;
    private readonly IPopupDialogService _popupDialogService;
    private bool _hasAttemptedInitialization;
    private bool _isFailurePopupVisible;
    private bool _isInitializing;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartupPage"/> class.
    /// </summary>
    public StartupPage(
        IAppStartupInitializationService appStartupInitializationService,
        IAppLocalizationService appLocalizationService,
        IPopupDialogService popupDialogService)
    {
        ArgumentNullException.ThrowIfNull(appStartupInitializationService);
        ArgumentNullException.ThrowIfNull(appLocalizationService);
        ArgumentNullException.ThrowIfNull(popupDialogService);

        InitializeComponent();

        _appStartupInitializationService = appStartupInitializationService;
        _appLocalizationService = appLocalizationService;
        _popupDialogService = popupDialogService;
        _appLocalizationService.CultureChanged += OnCultureChanged;

        ApplyLocalizedText();
        ApplyLoadingState();
    }

    /// <summary>
    /// Raised after startup initialization succeeds.
    /// </summary>
    public event EventHandler? StartupCompleted;

    /// <inheritdoc />
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_hasAttemptedInitialization)
        {
            return;
        }

        _hasAttemptedInitialization = true;
        _ = RunInitializationAsync();
    }

    /// <inheritdoc />
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
        MainThread.BeginInvokeOnMainThread(ApplyLocalizedText);
    }

    private async void OnRetryButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            await RunInitializationAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task RunInitializationAsync()
    {
        if (_isInitializing)
        {
            return;
        }

        _isInitializing = true;
        ApplyLoadingState();
        bool shouldRetry = false;

        try
        {
            AppStartupInitializationResult result = await _appStartupInitializationService
                .InitializeAsync(CancellationToken.None)
                .ConfigureAwait(true);

            if (result.IsSuccess)
            {
                StartupCompleted?.Invoke(this, EventArgs.Empty);
                return;
            }

            ApplyFailureState(result.ErrorMessage);
            shouldRetry = await ShowFailurePopupAsync(result.ErrorMessage).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception exception)
        {
            ApplyFailureState(exception.Message);
            shouldRetry = await ShowFailurePopupAsync(exception.Message).ConfigureAwait(true);
        }
        finally
        {
            _isInitializing = false;
            RetryButton.IsEnabled = true;
        }

        if (shouldRetry)
        {
            await RunInitializationAsync().ConfigureAwait(true);
        }
    }

    private void ApplyLocalizedText()
    {
        Title = AppStrings.AppTitle;
        RetryButton.Text = AppStrings.StartupRetryButton;

        if (ErrorLabel.IsVisible)
        {
            HeadlineLabel.Text = AppStrings.StartupFailedHeadline;
            DescriptionLabel.Text = AppStrings.StartupFailedDescription;
        }
        else
        {
            HeadlineLabel.Text = AppStrings.StartupLoadingHeadline;
            DescriptionLabel.Text = AppStrings.StartupLoadingDescription;
        }
    }

    private void ApplyLoadingState()
    {
        StartupIndicator.IsVisible = true;
        StartupIndicator.IsRunning = true;
        ErrorLabel.IsVisible = false;
        ErrorLabel.Text = string.Empty;
        RetryButton.IsVisible = false;
        RetryButton.IsEnabled = false;
        HeadlineLabel.Text = AppStrings.StartupLoadingHeadline;
        DescriptionLabel.Text = AppStrings.StartupLoadingDescription;
    }

    private void ApplyFailureState(string? errorMessage)
    {
        StartupIndicator.IsRunning = false;
        StartupIndicator.IsVisible = false;
        HeadlineLabel.Text = AppStrings.StartupFailedHeadline;
        DescriptionLabel.Text = AppStrings.StartupFailedDescription;
        ErrorLabel.Text = string.IsNullOrWhiteSpace(errorMessage)
            ? AppStrings.StartupFailedGenericDetails
            : string.Format(AppStrings.StartupFailedDetailsFormat, errorMessage);
        ErrorLabel.IsVisible = true;
        RetryButton.IsVisible = true;
        RetryButton.IsEnabled = true;
    }

    private async Task<bool> ShowFailurePopupAsync(string? errorMessage)
    {
        if (_isFailurePopupVisible)
        {
            return false;
        }

        _isFailurePopupVisible = true;

        try
        {
            string detailText = string.IsNullOrWhiteSpace(errorMessage)
                ? AppStrings.StartupFailedGenericDetails
                : string.Format(AppStrings.StartupFailedDetailsFormat, errorMessage);

            return await _popupDialogService
                .ShowConfirmationAsync(
                    AppStrings.StartupFailedHeadline,
                    string.Join(Environment.NewLine + Environment.NewLine, AppStrings.StartupFailedDescription, detailText),
                    AppStrings.StartupRetryButton,
                    AppStrings.StartupStayOnPageButton,
                    PopupDialogKind.Error)
                .ConfigureAwait(true);
        }
        finally
        {
            _isFailurePopupVisible = false;
        }
    }
}

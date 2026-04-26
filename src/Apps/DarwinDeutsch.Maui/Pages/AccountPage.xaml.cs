using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Auth;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinDeutsch.Maui.Services.UI;

namespace DarwinDeutsch.Maui.Pages;

public partial class AccountPage : ContentPage
{
    private readonly IAppLocalizationService _appLocalizationService;
    private readonly IMobileAuthService _mobileAuthService;
    private readonly IPopupDialogService _popupDialogService;
    private CancellationTokenSource? _requestCancellationTokenSource;

    public AccountPage(
        IAppLocalizationService appLocalizationService,
        IMobileAuthService mobileAuthService,
        IPopupDialogService popupDialogService)
    {
        ArgumentNullException.ThrowIfNull(appLocalizationService);
        ArgumentNullException.ThrowIfNull(mobileAuthService);
        ArgumentNullException.ThrowIfNull(popupDialogService);

        InitializeComponent();
        _appLocalizationService = appLocalizationService;
        _mobileAuthService = mobileAuthService;
        _popupDialogService = popupDialogService;
        _appLocalizationService.CultureChanged += OnCultureChanged;
        ApplyLocalizedText();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await RefreshSessionAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    protected override void OnDisappearing()
    {
        CancelRequest();
        base.OnDisappearing();
    }

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
        ApplyLocalizedText();
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await RefreshSessionAsync().ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
            }
        });
    }

    private void ApplyLocalizedText()
    {
        Title = AppStrings.AccountTitle;
        HeadlineLabel.Text = AppStrings.AccountHeadline;
        DescriptionLabel.Text = AppStrings.AccountDescription;
        SessionSectionLabel.Text = AppStrings.AccountSessionSectionLabel;
        SignInSectionLabel.Text = AppStrings.AccountSignInSectionLabel;
        RegisterSectionLabel.Text = AppStrings.AccountRegisterSectionLabel;
        EmailInputLayout.Hint = AppStrings.AccountEmailLabel;
        PasswordInputLayout.Hint = AppStrings.AccountPasswordLabel;
        RegisterEmailInputLayout.Hint = AppStrings.AccountEmailLabel;
        RegisterPasswordInputLayout.Hint = AppStrings.AccountPasswordLabel;
        RegisterConfirmPasswordInputLayout.Hint = AppStrings.AccountConfirmPasswordLabel;
        SignInButton.Text = AppStrings.AccountSignInButton;
        RegisterButton.Text = AppStrings.AccountRegisterButton;
        RefreshSessionButton.Text = AppStrings.AccountRefreshButton;
        SignOutButton.Text = AppStrings.AccountSignOutButton;
    }

    private async Task RefreshSessionAsync()
    {
        ResetRequest();
        MobileAuthSession? session = await _mobileAuthService
            .GetCurrentSessionAsync(_requestCancellationTokenSource!.Token)
            .ConfigureAwait(true);

        SessionSummaryLabel.Text = session is null
            ? AppStrings.AccountSignedOutSummary
            : string.Format(AppStrings.AccountSignedInSummaryFormat, session.Email, string.Join(", ", session.Roles));

        SignOutButton.IsEnabled = session is not null;
        RefreshSessionButton.IsEnabled = true;
    }

    private async void OnSignInButtonClicked(object? sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim() ?? string.Empty;
        string password = PasswordEntry.Text ?? string.Empty;

        if (!ValidateCredentials(email, password))
        {
            return;
        }

        try
        {
            ResetRequest();
            MobileAuthSession session = await _mobileAuthService
                .SignInAsync(email, password, _requestCancellationTokenSource!.Token)
                .ConfigureAwait(true);

            PasswordEntry.Text = string.Empty;
            await _popupDialogService.ShowMessageAsync(
                    AppStrings.AccountSignInSuccessTitle,
                    string.Format(AppStrings.AccountSignInSuccessMessageFormat, session.Email),
                    AppStrings.SettingsContentUpdatesDismissButton,
                    PopupDialogKind.Success,
                    _requestCancellationTokenSource.Token)
                .ConfigureAwait(true);

            await RefreshSessionAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            await _popupDialogService.ShowMessageAsync(
                    AppStrings.AccountSignInFailedTitle,
                    string.Format(AppStrings.AccountSignInFailedMessageFormat, exception.Message),
                    AppStrings.SettingsContentUpdatesDismissButton,
                    PopupDialogKind.Error)
                .ConfigureAwait(true);
        }
    }

    private async void OnRegisterButtonClicked(object? sender, EventArgs e)
    {
        string email = RegisterEmailEntry.Text?.Trim() ?? string.Empty;
        string password = RegisterPasswordEntry.Text ?? string.Empty;
        string confirmPassword = RegisterConfirmPasswordEntry.Text ?? string.Empty;

        if (!ValidateCredentials(email, password))
        {
            return;
        }

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            await _popupDialogService.ShowMessageAsync(
                    AppStrings.AccountRegisterFailedTitle,
                    AppStrings.AccountPasswordMismatchMessage,
                    AppStrings.SettingsContentUpdatesDismissButton,
                    PopupDialogKind.Warning)
                .ConfigureAwait(true);
            return;
        }

        try
        {
            ResetRequest();
            MobileAuthSession session = await _mobileAuthService
                .RegisterAsync(email, password, _requestCancellationTokenSource!.Token)
                .ConfigureAwait(true);

            RegisterPasswordEntry.Text = string.Empty;
            RegisterConfirmPasswordEntry.Text = string.Empty;
            await _popupDialogService.ShowMessageAsync(
                    AppStrings.AccountRegisterSuccessTitle,
                    string.Format(AppStrings.AccountRegisterSuccessMessageFormat, session.Email),
                    AppStrings.SettingsContentUpdatesDismissButton,
                    PopupDialogKind.Success,
                    _requestCancellationTokenSource.Token)
                .ConfigureAwait(true);

            await RefreshSessionAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            await _popupDialogService.ShowMessageAsync(
                    AppStrings.AccountRegisterFailedTitle,
                    string.Format(AppStrings.AccountRegisterFailedMessageFormat, exception.Message),
                    AppStrings.SettingsContentUpdatesDismissButton,
                    PopupDialogKind.Error)
                .ConfigureAwait(true);
        }
    }

    private async void OnRefreshSessionButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            await RefreshSessionAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            await _popupDialogService.ShowMessageAsync(
                    AppStrings.AccountRefreshFailedTitle,
                    string.Format(AppStrings.AccountRefreshFailedMessageFormat, exception.Message),
                    AppStrings.SettingsContentUpdatesDismissButton,
                    PopupDialogKind.Error)
                .ConfigureAwait(true);
        }
    }

    private async void OnSignOutButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            ResetRequest();
            await _mobileAuthService.SignOutAsync(_requestCancellationTokenSource!.Token).ConfigureAwait(true);
            await RefreshSessionAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private bool ValidateCredentials(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _ = _popupDialogService.ShowMessageAsync(
                AppStrings.AccountValidationFailedTitle,
                AppStrings.AccountCredentialsRequiredMessage,
                AppStrings.SettingsContentUpdatesDismissButton,
                PopupDialogKind.Warning);

            return false;
        }

        return true;
    }

    private void ResetRequest()
    {
        CancelRequest();
        _requestCancellationTokenSource = new CancellationTokenSource();
    }

    private void CancelRequest()
    {
        _requestCancellationTokenSource?.Cancel();
        _requestCancellationTokenSource?.Dispose();
        _requestCancellationTokenSource = null;
    }
}

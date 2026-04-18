using Syncfusion.Maui.Toolkit.Popup;

namespace DarwinDeutsch.Maui.Services.UI;

/// <summary>
/// Shows application dialogs through Syncfusion popups with a centralized visual contract.
/// </summary>
internal sealed class SyncfusionPopupDialogService : IPopupDialogService
{
    private static readonly Thickness PopupPadding = new(20);

    /// <inheritdoc />
    public Task ShowMessageAsync(string title, string message, string acceptText, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(acceptText);

        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            SfPopup popup = CreatePopup(title, message, acceptText, null);
            _ = await popup.ShowAsync().ConfigureAwait(true);
        });
    }

    /// <inheritdoc />
    public Task<bool> ShowConfirmationAsync(
        string title,
        string message,
        string acceptText,
        string declineText,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(acceptText);
        ArgumentException.ThrowIfNullOrWhiteSpace(declineText);

        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            SfPopup popup = CreatePopup(title, message, acceptText, declineText);
            return await popup.ShowAsync().ConfigureAwait(true);
        });
    }

    private static SfPopup CreatePopup(string title, string message, string acceptText, string? declineText)
    {
        bool hasDeclineAction = !string.IsNullOrWhiteSpace(declineText);

        return new SfPopup
        {
            HeaderTitle = title,
            Message = message,
            AcceptButtonText = acceptText,
            DeclineButtonText = declineText ?? string.Empty,
            ShowHeader = true,
            ShowFooter = true,
            ShowCloseButton = false,
            ShowOverlayAlways = true,
            IgnoreActionBar = true,
            Padding = PopupPadding,
            AppearanceMode = hasDeclineAction
                ? PopupButtonAppearanceMode.TwoButton
                : PopupButtonAppearanceMode.OneButton,
            OverlayMode = PopupOverlayMode.Blur,
            PopupStyle = new PopupStyle
            {
                CornerRadius = 4,
                HasShadow = true,
                BlurIntensity = PopupBlurIntensity.Light,
                HeaderFontSize = 18,
                FooterFontSize = 15,
            },
        };
    }
}

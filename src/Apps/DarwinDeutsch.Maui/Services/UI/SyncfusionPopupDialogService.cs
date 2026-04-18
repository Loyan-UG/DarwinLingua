using Syncfusion.Maui.Toolkit.Popup;

namespace DarwinDeutsch.Maui.Services.UI;

/// <summary>
/// Shows application dialogs through Syncfusion popups with a centralized visual contract.
/// </summary>
internal sealed class SyncfusionPopupDialogService : IPopupDialogService
{
    private static readonly Thickness PopupPadding = new(20);
    private static readonly Color SurfaceColor = Color.FromArgb("#FFFCF8");
    private static readonly Color SurfaceDarkColor = Color.FromArgb("#16212B");
    private static readonly Color StrokeColor = Color.FromArgb("#D8E0E5");
    private static readonly Color HeaderTextColor = Color.FromArgb("#10212B");
    private static readonly Color MessageTextColor = Color.FromArgb("#42505C");
    private static readonly Color SuccessAccentColor = Color.FromArgb("#1A6B5B");
    private static readonly Color SuccessSurfaceColor = Color.FromArgb("#E6F4EE");
    private static readonly Color WarningAccentColor = Color.FromArgb("#A9581A");
    private static readonly Color WarningSurfaceColor = Color.FromArgb("#FFF4E8");
    private static readonly Color ErrorAccentColor = Color.FromArgb("#9F2D23");
    private static readonly Color ErrorSurfaceColor = Color.FromArgb("#FDECEA");
    private static readonly Color InfoAccentColor = Color.FromArgb("#155E75");
    private static readonly Color InfoSurfaceColor = Color.FromArgb("#E9F5F9");
    private static readonly Color WhiteTextColor = Colors.White;

    /// <inheritdoc />
    public Task ShowMessageAsync(
        string title,
        string message,
        string acceptText,
        PopupDialogKind kind = PopupDialogKind.Info,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(acceptText);

        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            SfPopup popup = CreatePopup(title, message, acceptText, null, kind);
            _ = await popup.ShowAsync().ConfigureAwait(true);
        });
    }

    /// <inheritdoc />
    public Task<bool> ShowConfirmationAsync(
        string title,
        string message,
        string acceptText,
        string declineText,
        PopupDialogKind kind = PopupDialogKind.Warning,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(acceptText);
        ArgumentException.ThrowIfNullOrWhiteSpace(declineText);

        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            SfPopup popup = CreatePopup(title, message, acceptText, declineText, kind);
            return await popup.ShowAsync().ConfigureAwait(true);
        });
    }

    private static SfPopup CreatePopup(
        string title,
        string message,
        string acceptText,
        string? declineText,
        PopupDialogKind kind)
    {
        bool hasDeclineAction = !string.IsNullOrWhiteSpace(declineText);
        PopupVisualState visualState = GetVisualState(kind);

        return new SfPopup
        {
            HeaderTitle = title,
            Message = message,
            AcceptButtonText = acceptText,
            DeclineButtonText = declineText ?? string.Empty,
            ShowHeader = true,
            ShowFooter = true,
            ShowCloseButton = !hasDeclineAction,
            ShowOverlayAlways = true,
            IgnoreActionBar = true,
            StaysOpen = false,
            Padding = PopupPadding,
            AnimationMode = PopupAnimationMode.Fade,
            AnimationDuration = 160,
            AppearanceMode = hasDeclineAction
                ? PopupButtonAppearanceMode.TwoButton
                : PopupButtonAppearanceMode.OneButton,
            OverlayMode = PopupOverlayMode.Blur,
            PopupStyle = new PopupStyle
            {
                CornerRadius = 6,
                HasShadow = true,
                BlurIntensity = PopupBlurIntensity.Light,
                OverlayColor = visualState.OverlayColor,
                Stroke = visualState.BorderColor,
                StrokeThickness = 1,
                HeaderBackground = visualState.HeaderBackground,
                HeaderTextColor = visualState.HeaderTextColor,
                MessageBackground = SurfaceColor,
                MessageTextColor = MessageTextColor,
                FooterBackground = SurfaceColor,
                HeaderFontSize = 18,
                HeaderFontAttribute = FontAttributes.Bold,
                MessageFontSize = 15,
                FooterFontSize = 15,
                FooterButtonCornerRadius = 18,
                AcceptButtonBackground = visualState.AcceptButtonBackground,
                AcceptButtonTextColor = visualState.AcceptButtonTextColor,
                DeclineButtonBackground = hasDeclineAction ? SurfaceDarkColor : visualState.AcceptButtonBackground,
                DeclineButtonTextColor = hasDeclineAction ? WhiteTextColor : visualState.AcceptButtonTextColor,
                CloseIconColor = visualState.CloseIconColor,
            },
        };
    }

    private static PopupVisualState GetVisualState(PopupDialogKind kind)
    {
        return kind switch
        {
            PopupDialogKind.Success => new PopupVisualState(
                HeaderBackground: SuccessSurfaceColor,
                HeaderTextColor: SuccessAccentColor,
                AcceptButtonBackground: SuccessAccentColor,
                AcceptButtonTextColor: WhiteTextColor,
                BorderColor: SuccessAccentColor.WithAlpha(0.25f),
                OverlayColor: SuccessAccentColor.WithAlpha(0.10f),
                CloseIconColor: SuccessAccentColor),
            PopupDialogKind.Warning => new PopupVisualState(
                HeaderBackground: WarningSurfaceColor,
                HeaderTextColor: WarningAccentColor,
                AcceptButtonBackground: WarningAccentColor,
                AcceptButtonTextColor: WhiteTextColor,
                BorderColor: WarningAccentColor.WithAlpha(0.25f),
                OverlayColor: WarningAccentColor.WithAlpha(0.10f),
                CloseIconColor: WarningAccentColor),
            PopupDialogKind.Error => new PopupVisualState(
                HeaderBackground: ErrorSurfaceColor,
                HeaderTextColor: ErrorAccentColor,
                AcceptButtonBackground: ErrorAccentColor,
                AcceptButtonTextColor: WhiteTextColor,
                BorderColor: ErrorAccentColor.WithAlpha(0.25f),
                OverlayColor: ErrorAccentColor.WithAlpha(0.12f),
                CloseIconColor: ErrorAccentColor),
            _ => new PopupVisualState(
                HeaderBackground: InfoSurfaceColor,
                HeaderTextColor: InfoAccentColor,
                AcceptButtonBackground: InfoAccentColor,
                AcceptButtonTextColor: WhiteTextColor,
                BorderColor: StrokeColor,
                OverlayColor: InfoAccentColor.WithAlpha(0.08f),
                CloseIconColor: InfoAccentColor),
        };
    }

    private sealed record PopupVisualState(
        Color HeaderBackground,
        Color HeaderTextColor,
        Color AcceptButtonBackground,
        Color AcceptButtonTextColor,
        Color BorderColor,
        Color OverlayColor,
        Color CloseIconColor);
}

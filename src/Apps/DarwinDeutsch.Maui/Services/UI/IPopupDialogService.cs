namespace DarwinDeutsch.Maui.Services.UI;

/// <summary>
/// Provides a consistent application-level popup dialog surface.
/// </summary>
public interface IPopupDialogService
{
    /// <summary>
    /// Shows a single-action informational popup.
    /// </summary>
    Task ShowMessageAsync(
        string title,
        string message,
        string acceptText,
        PopupDialogKind kind = PopupDialogKind.Info,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Shows a two-action confirmation popup and returns <c>true</c> when the accept action was chosen.
    /// </summary>
    Task<bool> ShowConfirmationAsync(
        string title,
        string message,
        string acceptText,
        string declineText,
        PopupDialogKind kind = PopupDialogKind.Warning,
        CancellationToken cancellationToken = default);
}

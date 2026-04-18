namespace DarwinDeutsch.Maui.Services.UI;

/// <summary>
/// Represents the visual intent of an application popup dialog.
/// </summary>
public enum PopupDialogKind
{
    /// <summary>
    /// A neutral informational popup.
    /// </summary>
    Info,

    /// <summary>
    /// A success popup shown after a completed action.
    /// </summary>
    Success,

    /// <summary>
    /// A warning popup shown before a potentially disruptive action.
    /// </summary>
    Warning,

    /// <summary>
    /// An error popup shown after a failed action.
    /// </summary>
    Error,
}

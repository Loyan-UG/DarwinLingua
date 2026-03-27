using System.Windows.Input;

namespace DarwinDeutsch.Maui.Controls;

/// <summary>
/// Reusable action block with a localized caption and one action button.
/// </summary>
public partial class ActionBlockView : ContentView
{
    /// <summary>
    /// Backing bindable property for <see cref="Caption"/>.
    /// </summary>
    public static readonly BindableProperty CaptionProperty = BindableProperty.Create(
        nameof(Caption),
        typeof(string),
        typeof(ActionBlockView),
        string.Empty,
        propertyChanged: static (bindable, _, newValue) =>
        {
            ((ActionBlockView)bindable).CaptionLabel.Text = (string?)newValue ?? string.Empty;
        });

    /// <summary>
    /// Backing bindable property for <see cref="ButtonText"/>.
    /// </summary>
    public static readonly BindableProperty ButtonTextProperty = BindableProperty.Create(
        nameof(ButtonText),
        typeof(string),
        typeof(ActionBlockView),
        string.Empty,
        propertyChanged: static (bindable, _, newValue) =>
        {
            ((ActionBlockView)bindable).ActionButton.Text = (string?)newValue ?? string.Empty;
        });

    /// <summary>
    /// Backing bindable property for <see cref="ActionCommand"/>.
    /// </summary>
    public static readonly BindableProperty ActionCommandProperty = BindableProperty.Create(
        nameof(ActionCommand),
        typeof(ICommand),
        typeof(ActionBlockView));

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionBlockView"/> class.
    /// </summary>
    public ActionBlockView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Raised when the action button is tapped.
    /// </summary>
    public event EventHandler? ActionInvoked;

    /// <summary>
    /// Gets or sets the action block caption.
    /// </summary>
    public string Caption
    {
        get => (string)GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    /// <summary>
    /// Gets or sets the action button text.
    /// </summary>
    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the optional command executed when the action button is tapped.
    /// </summary>
    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    /// <summary>
    /// Handles action button taps.
    /// </summary>
    private void OnActionButtonClicked(object? sender, EventArgs e)
    {
        if (ActionCommand?.CanExecute(null) == true)
        {
            ActionCommand.Execute(null);
        }

        ActionInvoked?.Invoke(this, EventArgs.Empty);
    }
}

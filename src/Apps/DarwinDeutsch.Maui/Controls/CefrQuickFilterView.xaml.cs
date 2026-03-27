using System.Collections.Generic;
using System.Windows.Input;

namespace DarwinDeutsch.Maui.Controls;

/// <summary>
/// Reusable CEFR quick-filter control with predefined level buttons.
/// </summary>
public partial class CefrQuickFilterView : ContentView
{
    /// <summary>
    /// Backing bindable property for <see cref="Caption"/>.
    /// </summary>
    public static readonly BindableProperty CaptionProperty = BindableProperty.Create(
        nameof(Caption),
        typeof(string),
        typeof(CefrQuickFilterView),
        string.Empty,
        propertyChanged: static (bindable, _, newValue) =>
        {
            ((CefrQuickFilterView)bindable).CaptionLabel.Text = (string?)newValue ?? string.Empty;
        });

    /// <summary>
    /// Backing bindable property for <see cref="SelectedLevel"/>.
    /// </summary>
    public static readonly BindableProperty SelectedLevelProperty = BindableProperty.Create(
        nameof(SelectedLevel),
        typeof(string),
        typeof(CefrQuickFilterView),
        string.Empty,
        propertyChanged: static (bindable, _, _) =>
        {
            ((CefrQuickFilterView)bindable).ApplySelectedState();
        });

    /// <summary>
    /// Backing bindable property for <see cref="LevelSelectedCommand"/>.
    /// </summary>
    public static readonly BindableProperty LevelSelectedCommandProperty = BindableProperty.Create(
        nameof(LevelSelectedCommand),
        typeof(ICommand),
        typeof(CefrQuickFilterView));

    /// <summary>
    /// Initializes a new instance of the <see cref="CefrQuickFilterView"/> class.
    /// </summary>
    public CefrQuickFilterView()
    {
        InitializeComponent();
        ApplySelectedState();
    }

    /// <summary>
    /// Raised when a CEFR level button is selected.
    /// </summary>
    public event EventHandler? LevelSelected;

    /// <summary>
    /// Gets or sets the section caption displayed above the CEFR buttons.
    /// </summary>
    public string Caption
    {
        get => (string)GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    /// <summary>
    /// Gets or sets the currently selected CEFR level.
    /// </summary>
    public string SelectedLevel
    {
        get => (string)GetValue(SelectedLevelProperty);
        set => SetValue(SelectedLevelProperty, value);
    }

    /// <summary>
    /// Gets or sets the optional command executed when a CEFR level is selected.
    /// </summary>
    public ICommand? LevelSelectedCommand
    {
        get => (ICommand?)GetValue(LevelSelectedCommandProperty);
        set => SetValue(LevelSelectedCommandProperty, value);
    }

    /// <summary>
    /// Handles quick-filter button taps.
    /// </summary>
    private void OnCefrButtonClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || string.IsNullOrWhiteSpace(button.Text))
        {
            return;
        }

        SelectedLevel = button.Text;

        if (LevelSelectedCommand?.CanExecute(SelectedLevel) == true)
        {
            LevelSelectedCommand.Execute(SelectedLevel);
        }

        LevelSelected?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Applies visual selection state for the active CEFR button.
    /// </summary>
    private void ApplySelectedState()
    {
        foreach (Button button in GetCefrButtons())
        {
            ApplyButtonSelection(button);
        }
    }

    /// <summary>
    /// Applies selection colors to a single CEFR button.
    /// </summary>
    private void ApplyButtonSelection(Button button)
    {
        bool isSelected = string.Equals(button.Text, SelectedLevel, StringComparison.OrdinalIgnoreCase);
        button.BackgroundColor = isSelected
            ? (Color?)Application.Current?.Resources["Primary"] ?? Colors.DarkSlateBlue
            : null;
        button.TextColor = isSelected
            ? (Color?)Application.Current?.Resources["White"] ?? Colors.White
            : null;
    }

    /// <summary>
    /// Returns the fixed CEFR buttons used by the control.
    /// </summary>
    private IEnumerable<Button> GetCefrButtons()
    {
        yield return A1Button;
        yield return A2Button;
        yield return B1Button;
        yield return B2Button;
        yield return C1Button;
        yield return C2Button;
    }
}

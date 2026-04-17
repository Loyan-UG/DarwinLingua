using System.Collections.Generic;
using System.Windows.Input;
using Syncfusion.Maui.Toolkit.SegmentedControl;

namespace DarwinDeutsch.Maui.Controls;

/// <summary>
/// Reusable CEFR quick-filter control with predefined level buttons.
/// </summary>
public partial class CefrQuickFilterView : ContentView
{
    private static readonly string[] Levels = ["A1", "A2", "B1", "B2", "C1", "C2"];

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
        SegmentedControl.ItemsSource = Levels;
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
    private void OnSegmentedControlSelectionChanged(
        object? sender,
        Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
    {
        if (e.NewValue is null)
        {
            return;
        }

        string? selectedLevel = e.NewValue.ToString();
        if (string.IsNullOrWhiteSpace(selectedLevel))
        {
            return;
        }

        SelectedLevel = selectedLevel;

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
        int selectedIndex = Array.FindIndex(
            Levels,
            level => string.Equals(level, SelectedLevel, StringComparison.OrdinalIgnoreCase));

        if (selectedIndex < 0)
        {
            return;
        }

        if (SegmentedControl.SelectedIndex != selectedIndex)
        {
            SegmentedControl.SelectedIndex = selectedIndex;
        }
    }
}

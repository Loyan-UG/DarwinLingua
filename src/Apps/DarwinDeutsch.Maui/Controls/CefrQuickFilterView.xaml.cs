using System.Collections.Generic;
using System.Windows.Input;

namespace DarwinDeutsch.Maui.Controls;

/// <summary>
/// Reusable CEFR quick-filter control with predefined level buttons.
/// </summary>
public partial class CefrQuickFilterView : ContentView
{
    private static readonly IReadOnlyDictionary<string, CefrButtonPalette> ButtonPalettes =
        new Dictionary<string, CefrButtonPalette>(StringComparer.OrdinalIgnoreCase)
        {
            ["A1"] = new(Colors.White, Color.FromArgb("#2B7FFF"), Color.FromArgb("#E4F0FF")),
            ["A2"] = new(Colors.White, Color.FromArgb("#1FA3A3"), Color.FromArgb("#DCF7F4")),
            ["B1"] = new(Colors.White, Color.FromArgb("#2F9D57"), Color.FromArgb("#E1F6E7")),
            ["B2"] = new(Colors.White, Color.FromArgb("#D0A019"), Color.FromArgb("#FFF5D7")),
            ["C1"] = new(Colors.White, Color.FromArgb("#E07A1F"), Color.FromArgb("#FFE8D3")),
            ["C2"] = new(Colors.White, Color.FromArgb("#D64545"), Color.FromArgb("#FFE0E0")),
        };

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
        CefrButtonPalette palette = ResolvePalette(button.Text);

        button.FontSize = 19;
        button.FontAttributes = FontAttributes.Bold;
        button.HeightRequest = 58;
        button.Padding = new Thickness(0);
        button.BorderWidth = 1;
        button.BorderColor = palette.AccentColor;
        button.BackgroundColor = isSelected ? palette.AccentColor : palette.SurfaceColor;
        button.TextColor = isSelected ? palette.SelectedTextColor : palette.AccentColor;
    }

    private static CefrButtonPalette ResolvePalette(string? level)
    {
        if (!string.IsNullOrWhiteSpace(level) && ButtonPalettes.TryGetValue(level, out CefrButtonPalette? palette))
        {
            return palette;
        }

        return new CefrButtonPalette(Colors.White, ResolveAppColor("Primary", Colors.DarkSlateBlue), ResolveAppColor("Secondary", Colors.LightGray));
    }

    /// <summary>
    /// Resolves an application-scoped color with a safe fallback for early initialization timing.
    /// </summary>
    private static Color ResolveAppColor(string resourceKey, Color fallbackColor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceKey);

        if (Application.Current?.Resources.TryGetValue(resourceKey, out object? color) == true &&
            color is Color resolvedColor)
        {
            return resolvedColor;
        }

        return fallbackColor;
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

    private sealed record CefrButtonPalette(Color SelectedTextColor, Color AccentColor, Color SurfaceColor);
}

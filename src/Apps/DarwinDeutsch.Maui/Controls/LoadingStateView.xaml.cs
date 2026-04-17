namespace DarwinDeutsch.Maui.Controls;

/// <summary>
/// Reusable loading state view backed by a Syncfusion shimmer indicator.
/// </summary>
public partial class LoadingStateView : ContentView
{
    /// <summary>
    /// Backing bindable property for <see cref="Message"/>.
    /// </summary>
    public static readonly BindableProperty MessageProperty = BindableProperty.Create(
        nameof(Message),
        typeof(string),
        typeof(LoadingStateView),
        string.Empty,
        propertyChanged: static (bindable, _, newValue) =>
        {
            ((LoadingStateView)bindable).MessageLabel.Text = (string?)newValue ?? string.Empty;
        });

    /// <summary>
    /// Backing bindable property for <see cref="IsLoading"/>.
    /// </summary>
    public static readonly BindableProperty IsLoadingProperty = BindableProperty.Create(
        nameof(IsLoading),
        typeof(bool),
        typeof(LoadingStateView),
        false,
        propertyChanged: static (bindable, _, newValue) =>
        {
            LoadingStateView view = (LoadingStateView)bindable;
            bool isLoading = (bool)newValue;
            view.LoadingLayout.IsVisible = isLoading;
            view.LoadingShimmer.IsActive = isLoading;
        });

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadingStateView"/> class.
    /// </summary>
    public LoadingStateView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the loading message shown next to the shimmer.
    /// </summary>
    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the loading state is active.
    /// </summary>
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
}

namespace DarwinDeutsch.Maui.Controls;

/// <summary>
/// Reusable loading state view backed by a Syncfusion shimmer indicator.
/// </summary>
public partial class LoadingStateView : ContentView
{
    /// <summary>
    /// Backing bindable property for <see cref="LoadingMode"/>.
    /// </summary>
    public static readonly BindableProperty LoadingModeProperty = BindableProperty.Create(
        nameof(LoadingMode),
        typeof(LoadingStateViewMode),
        typeof(LoadingStateView),
        LoadingStateViewMode.Inline,
        propertyChanged: static (bindable, _, _) => ((LoadingStateView)bindable).ApplyState());

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
            _ = (bool)newValue;
            view.ApplyState();
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
    /// Gets or sets the visual loading mode to render.
    /// </summary>
    public LoadingStateViewMode LoadingMode
    {
        get => (LoadingStateViewMode)GetValue(LoadingModeProperty);
        set => SetValue(LoadingModeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the loading state is active.
    /// </summary>
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    private void ApplyState()
    {
        bool isLoading = IsLoading;
        RootLayout.IsVisible = isLoading;

        bool isInline = LoadingMode == LoadingStateViewMode.Inline;
        InlineLoadingLayout.IsVisible = isLoading && isInline;
        InlineLoadingShimmer.IsActive = isLoading && isInline;

        bool isListSkeleton = LoadingMode == LoadingStateViewMode.ListSkeleton;
        ListSkeletonLayout.IsVisible = isLoading && isListSkeleton;
        ListSkeletonShimmer.IsActive = isLoading && isListSkeleton;

        bool isDetailSkeleton = LoadingMode == LoadingStateViewMode.DetailSkeleton;
        DetailSkeletonLayout.IsVisible = isLoading && isDetailSkeleton;
        DetailSkeletonShimmer.IsActive = isLoading && isDetailSkeleton;
    }
}

/// <summary>
/// Defines the visual mode used by <see cref="LoadingStateView"/>.
/// </summary>
public enum LoadingStateViewMode
{
    /// <summary>
    /// Uses the compact inline loading indicator with a short message.
    /// </summary>
    Inline,

    /// <summary>
    /// Uses a repeated list skeleton suited for browse and search pages.
    /// </summary>
    ListSkeleton,

    /// <summary>
    /// Uses a richer detail-page skeleton suited for lexical detail screens.
    /// </summary>
    DetailSkeleton,
}

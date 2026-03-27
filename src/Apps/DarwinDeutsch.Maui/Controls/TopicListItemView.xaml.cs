namespace DarwinDeutsch.Maui.Controls;

/// <summary>
/// Reusable list item view for localized topic summaries.
/// </summary>
public partial class TopicListItemView : ContentView
{
    /// <summary>
    /// Backing bindable property for <see cref="DisplayName"/>.
    /// </summary>
    public static readonly BindableProperty DisplayNameProperty = BindableProperty.Create(
        nameof(DisplayName),
        typeof(string),
        typeof(TopicListItemView),
        string.Empty,
        propertyChanged: static (bindable, _, newValue) =>
        {
            ((TopicListItemView)bindable).DisplayNameLabel.Text = (string?)newValue ?? string.Empty;
        });

    /// <summary>
    /// Backing bindable property for <see cref="TopicKey"/>.
    /// </summary>
    public static readonly BindableProperty TopicKeyProperty = BindableProperty.Create(
        nameof(TopicKey),
        typeof(string),
        typeof(TopicListItemView),
        string.Empty,
        propertyChanged: static (bindable, _, newValue) =>
        {
            ((TopicListItemView)bindable).TopicKeyLabel.Text = (string?)newValue ?? string.Empty;
        });

    /// <summary>
    /// Initializes a new instance of the <see cref="TopicListItemView"/> class.
    /// </summary>
    public TopicListItemView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the localized topic display name.
    /// </summary>
    public string DisplayName
    {
        get => (string)GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }

    /// <summary>
    /// Gets or sets the stable topic key.
    /// </summary>
    public string TopicKey
    {
        get => (string)GetValue(TopicKeyProperty);
        set => SetValue(TopicKeyProperty, value);
    }
}

using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays the seeded browse topics in the currently active UI language.
/// </summary>
public partial class TopicsPage : ContentPage
{
    private readonly IAppLocalizationService _appLocalizationService;
    private readonly ITopicQueryService _topicQueryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TopicsPage"/> class.
    /// </summary>
    public TopicsPage(
        IAppLocalizationService appLocalizationService,
        ITopicQueryService topicQueryService)
    {
        ArgumentNullException.ThrowIfNull(appLocalizationService);
        ArgumentNullException.ThrowIfNull(topicQueryService);

        InitializeComponent();

        _appLocalizationService = appLocalizationService;
        _topicQueryService = topicQueryService;

        _appLocalizationService.CultureChanged += OnCultureChanged;

        ApplyLocalizedText();
    }

    /// <summary>
    /// Refreshes the localized topic list whenever the page becomes visible.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        ApplyLocalizedText();
        await RefreshTopicsAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Releases event subscriptions when the page handler is detached.
    /// </summary>
    /// <param name="args">The handler-changing event arguments.</param>
    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        if (args.NewHandler is null)
        {
            _appLocalizationService.CultureChanged -= OnCultureChanged;
        }

        base.OnHandlerChanging(args);
    }

    /// <summary>
    /// Handles UI culture changes raised by the localization service.
    /// </summary>
    private void OnCultureChanged(object? sender, EventArgs e)
    {
        ApplyLocalizedText();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await RefreshTopicsAsync().ConfigureAwait(true);
        });
    }

    /// <summary>
    /// Applies localized text to the page.
    /// </summary>
    private void ApplyLocalizedText()
    {
        Title = AppStrings.BrowseTabTitle;
        HeadlineLabel.Text = AppStrings.TopicsPageHeadline;
        DescriptionLabel.Text = AppStrings.TopicsPageDescription;
        EmptyStateLabel.Text = AppStrings.TopicsPageEmpty;
        LoadingStateLabel.Text = AppStrings.CommonStateLoading;
        ErrorStateLabel.Text = AppStrings.CommonStateError;
    }

    /// <summary>
    /// Loads the localized topic list for the current UI language.
    /// </summary>
    private async Task RefreshTopicsAsync()
    {
        ShowLoadingState();

        try
        {
            IReadOnlyList<TopicListItemModel> topics = await _topicQueryService
                .GetTopicsAsync(_appLocalizationService.CurrentCulture.TwoLetterISOLanguageName, CancellationToken.None)
                .ConfigureAwait(true);

            TopicsCollectionView.ItemsSource = topics;
            EmptyStateLabel.IsVisible = topics.Count == 0;
            TopicsCollectionView.IsVisible = topics.Count > 0;
            ErrorStateLabel.IsVisible = false;
        }
        catch
        {
            TopicsCollectionView.ItemsSource = Array.Empty<TopicListItemModel>();
            TopicsCollectionView.IsVisible = false;
            EmptyStateLabel.IsVisible = false;
            ErrorStateLabel.IsVisible = true;
        }
        finally
        {
            LoadingStateLabel.IsVisible = false;
        }
    }

    /// <summary>
    /// Shows the loading state while topic data is being fetched.
    /// </summary>
    private void ShowLoadingState()
    {
        LoadingStateLabel.IsVisible = true;
        ErrorStateLabel.IsVisible = false;
        EmptyStateLabel.IsVisible = false;
        TopicsCollectionView.IsVisible = false;
    }

    /// <summary>
    /// Navigates to the selected topic browse page.
    /// </summary>
    private async void OnTopicsSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not TopicListItemModel selectedTopic)
        {
            return;
        }

        TopicsCollectionView.SelectedItem = null;

        string topicKey = Uri.EscapeDataString(selectedTopic.Key);
        string topicTitle = Uri.EscapeDataString(selectedTopic.DisplayName);
        await Shell.Current
            .GoToAsync($"{nameof(TopicWordsPage)}?topicKey={topicKey}&topicTitle={topicTitle}")
            .ConfigureAwait(true);
    }
}

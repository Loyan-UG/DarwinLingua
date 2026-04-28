using DarwinDeutsch.Maui.Resources.Strings;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using System.Collections.ObjectModel;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays conversation starter packs available on the device.
/// </summary>
public partial class ConversationStartersPage : ContentPage
{
    private readonly IConversationStarterQueryService _conversationStarterQueryService;
    private readonly ObservableCollection<StarterPackCardViewModel> _starterPacks = [];
    private CancellationTokenSource? _refreshCancellationTokenSource;

    public ConversationStartersPage(IConversationStarterQueryService conversationStarterQueryService)
    {
        ArgumentNullException.ThrowIfNull(conversationStarterQueryService);

        InitializeComponent();
        _conversationStarterQueryService = conversationStarterQueryService;
        StarterPacksCollectionView.ItemsSource = _starterPacks;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    protected override void OnDisappearing()
    {
        CancelRefreshRequest();
        base.OnDisappearing();
    }

    private async Task RefreshAsync()
    {
        ResetRefreshRequest();
        CancellationToken cancellationToken = _refreshCancellationTokenSource!.Token;

        Title = "Starters";
        HeadlineLabel.Text = "Conversation starters";
        DescriptionLabel.Text = "Practice short German openers by situation, tone, goal, and topic.";
        EmptyStateLabel.Text = "No conversation starter packs are available yet.";
        LoadingStateView.Message = AppStrings.CommonStateLoading;
        LoadingStateView.IsLoading = true;

        IReadOnlyList<ConversationStarterPackListItemModel> starterPacks = await _conversationStarterQueryService
            .GetPublishedStarterPacksAsync(new ConversationStarterListFilterModel(null, null, null, null, null), cancellationToken)
            .ConfigureAwait(true);

        _starterPacks.Clear();
        foreach (ConversationStarterPackListItemModel starterPack in starterPacks)
        {
            _starterPacks.Add(new StarterPackCardViewModel(
                starterPack.Slug,
                starterPack.Title,
                starterPack.Description,
                starterPack.ConversationGoal,
                $"{starterPack.CefrLevel} • {starterPack.Situation} • {starterPack.Tone} • {string.Join(", ", starterPack.TopicKeys)}"));
        }

        LoadingStateView.IsLoading = false;
        EmptyStateLabel.IsVisible = _starterPacks.Count == 0;
        StarterPacksCollectionView.IsVisible = _starterPacks.Count > 0;
    }

    private async void OnStarterPacksSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not StarterPackCardViewModel selectedStarterPack)
        {
            return;
        }

        StarterPacksCollectionView.SelectedItem = null;
        string escapedSlug = Uri.EscapeDataString(selectedStarterPack.Slug);

        try
        {
            await Shell.Current.GoToAsync($"{nameof(ConversationStarterDetailPage)}?starterPackSlug={escapedSlug}").ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void ResetRefreshRequest()
    {
        CancelRefreshRequest();
        _refreshCancellationTokenSource = new CancellationTokenSource();
    }

    private void CancelRefreshRequest()
    {
        if (_refreshCancellationTokenSource is null)
        {
            return;
        }

        _refreshCancellationTokenSource.Cancel();
        _refreshCancellationTokenSource.Dispose();
        _refreshCancellationTokenSource = null;
    }

    private sealed record StarterPackCardViewModel(
        string Slug,
        string Title,
        string Description,
        string ConversationGoal,
        string MetadataLine);
}

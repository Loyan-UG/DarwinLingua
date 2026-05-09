using DarwinDeutsch.Maui.Resources.Strings;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using System.Collections.ObjectModel;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays practical dialogue lessons available on the device.
/// </summary>
public partial class DialoguesPage : ContentPage
{
    private readonly IDialogueLessonQueryService _dialogueLessonQueryService;
    private readonly ObservableCollection<DialogueCardViewModel> _dialogues = [];
    private CancellationTokenSource? _refreshCancellationTokenSource;

    public DialoguesPage(IDialogueLessonQueryService dialogueLessonQueryService)
    {
        ArgumentNullException.ThrowIfNull(dialogueLessonQueryService);

        InitializeComponent();
        _dialogueLessonQueryService = dialogueLessonQueryService;
        DialoguesCollectionView.ItemsSource = _dialogues;
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

        Title = "Dialogues";
        HeadlineLabel.Text = "Practical German dialogues";
        DescriptionLabel.Text = "Practice short dialogues, useful phrases, and quick checks for real situations.";
        EmptyStateLabel.Text = "No dialogues are available yet.";
        LoadingStateView.Message = AppStrings.CommonStateLoading;
        LoadingStateView.IsLoading = true;

        IReadOnlyList<DialogueLessonListItemModel> dialogues = await _dialogueLessonQueryService
            .GetPublishedDialoguesAsync(cancellationToken)
            .ConfigureAwait(true);

        _dialogues.Clear();
        foreach (DialogueLessonListItemModel dialogue in dialogues)
        {
            _dialogues.Add(new DialogueCardViewModel(
                dialogue.Slug,
                dialogue.Title,
                dialogue.Description,
                dialogue.LearnerGoal,
                $"{dialogue.CefrLevel} • {dialogue.Category} • {string.Join(", ", dialogue.TopicKeys)}"));
        }

        LoadingStateView.IsLoading = false;
        EmptyStateLabel.IsVisible = _dialogues.Count == 0;
        DialoguesCollectionView.IsVisible = _dialogues.Count > 0;
    }

    private async void OnDialoguesSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not DialogueCardViewModel selectedDialogue)
        {
            return;
        }

        DialoguesCollectionView.SelectedItem = null;
        string escapedSlug = Uri.EscapeDataString(selectedDialogue.Slug);

        try
        {
            await Shell.Current.GoToAsync($"{nameof(DialogueDetailPage)}?dialogueSlug={escapedSlug}").ConfigureAwait(true);
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

    private sealed record DialogueCardViewModel(
        string Slug,
        string Title,
        string Description,
        string Goal,
        string MetadataLine);
}

using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Auth;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using System.Collections.ObjectModel;

namespace DarwinDeutsch.Maui.Pages;

[QueryProperty(nameof(PreparationPackSlug), "preparationPackSlug")]
public partial class EventPreparationPackDetailPage : ContentPage
{
    private readonly IEventPreparationQueryService _eventPreparationQueryService;
    private readonly IMobileEntitledFeatureAccessService _featureAccessService;
    private readonly ObservableCollection<EventPreparationPromptItemViewModel> _prompts = [];
    private readonly ObservableCollection<EventPreparationVocabularyItemViewModel> _vocabulary = [];
    private CancellationTokenSource? _refreshCancellationTokenSource;
    private string _preparationPackSlug = string.Empty;

    public EventPreparationPackDetailPage(
        IEventPreparationQueryService eventPreparationQueryService,
        IMobileEntitledFeatureAccessService featureAccessService)
    {
        ArgumentNullException.ThrowIfNull(eventPreparationQueryService);
        ArgumentNullException.ThrowIfNull(featureAccessService);

        InitializeComponent();
        _eventPreparationQueryService = eventPreparationQueryService;
        _featureAccessService = featureAccessService;
        PromptsCollectionView.ItemsSource = _prompts;
        VocabularyCollectionView.ItemsSource = _vocabulary;
    }

    public string PreparationPackSlug
    {
        get => _preparationPackSlug;
        set => _preparationPackSlug = Uri.UnescapeDataString(value ?? string.Empty);
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

        Title = "Preparation";
        EmptyStateLabel.Text = "Preparation pack not found.";
        LoadingStateView.Message = AppStrings.CommonStateLoading;
        LoadingStateView.IsLoading = true;
        ContentScrollView.IsVisible = false;
        EmptyStateLabel.IsVisible = false;

        if (string.IsNullOrWhiteSpace(PreparationPackSlug))
        {
            ShowPreparationPack(null);
            return;
        }

        if (!await _featureAccessService.CanUseEventPreparationPacksAsync(cancellationToken).ConfigureAwait(true))
        {
            EmptyStateLabel.Text = "Preparation packs require an active trial or premium plan.";
            ShowPreparationPack(null);
            return;
        }

        EventPreparationPackDetailModel? preparationPack = await _eventPreparationQueryService
            .GetPublishedEventPreparationPackBySlugAsync(PreparationPackSlug, cancellationToken)
            .ConfigureAwait(true);

        ShowPreparationPack(preparationPack);
    }

    private void ShowPreparationPack(EventPreparationPackDetailModel? preparationPack)
    {
        _prompts.Clear();
        _vocabulary.Clear();

        if (preparationPack is null)
        {
            LoadingStateView.IsLoading = false;
            ContentScrollView.IsVisible = true;
            EmptyStateLabel.IsVisible = true;
            PromptsSection.IsVisible = false;
            VocabularySection.IsVisible = false;
            return;
        }

        Title = preparationPack.Title;
        MetadataLabel.Text = $"{preparationPack.CefrLevel} • {preparationPack.EventType} • {preparationPack.Category}";
        HeadlineLabel.Text = preparationPack.Title;
        DescriptionLabel.Text = preparationPack.Description;

        foreach (EventPreparationPromptModel prompt in preparationPack.Prompts)
        {
            _prompts.Add(new EventPreparationPromptItemViewModel(prompt.PromptType, prompt.Text));
        }

        foreach (EventPreparationVocabularyReferenceModel reference in preparationPack.LinkedVocabulary)
        {
            _vocabulary.Add(new EventPreparationVocabularyItemViewModel(
                reference.Word,
                string.Join(" • ", new[] { reference.PartOfSpeech, reference.CefrLevel }.Where(value => !string.IsNullOrWhiteSpace(value)))));
        }

        PromptsSection.IsVisible = _prompts.Count > 0;
        VocabularySection.IsVisible = _vocabulary.Count > 0;
        EmptyStateLabel.IsVisible = false;
        ContentScrollView.IsVisible = true;
        LoadingStateView.IsLoading = false;
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

    private sealed record EventPreparationPromptItemViewModel(string PromptType, string Text);

    private sealed record EventPreparationVocabularyItemViewModel(string Word, string MetadataLine);
}

using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Audio;
using DarwinDeutsch.Maui.Services.Auth;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Models;
using System.Collections.ObjectModel;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays dialogue, phrases, and quick checks for one dialogue lesson.
/// </summary>
[QueryProperty(nameof(DialogueSlug), "dialogueSlug")]
public partial class DialogueDetailPage : ContentPage
{
    private readonly IDialogueLessonQueryService _dialogueLessonQueryService;
    private readonly IConversationStarterQueryService _conversationStarterQueryService;
    private readonly IEventPreparationQueryService _eventPreparationQueryService;
    private readonly IActiveLearningProfileCacheService _activeLearningProfileCacheService;
    private readonly IMobileEntitledFeatureAccessService _featureAccessService;
    private readonly ISpeechPlaybackService _speechPlaybackService;
    private readonly ObservableCollection<DialogueStarterPackItemViewModel> _starterPacks = [];
    private readonly ObservableCollection<DialogueEventPreparationPackItemViewModel> _eventPreparationPacks = [];
    private readonly ObservableCollection<DialogueTextItemViewModel> _dialogueTurns = [];
    private readonly ObservableCollection<DialoguePhraseItemViewModel> _phrases = [];
    private readonly ObservableCollection<DialogueQuestionItemViewModel> _questions = [];
    private CancellationTokenSource? _refreshCancellationTokenSource;
    private CancellationTokenSource? _speechCancellationTokenSource;
    private string _dialogueSlug = string.Empty;

    public DialogueDetailPage(
        IDialogueLessonQueryService dialogueLessonQueryService,
        IConversationStarterQueryService conversationStarterQueryService,
        IEventPreparationQueryService eventPreparationQueryService,
        IActiveLearningProfileCacheService activeLearningProfileCacheService,
        IMobileEntitledFeatureAccessService featureAccessService,
        ISpeechPlaybackService speechPlaybackService)
    {
        ArgumentNullException.ThrowIfNull(dialogueLessonQueryService);
        ArgumentNullException.ThrowIfNull(conversationStarterQueryService);
        ArgumentNullException.ThrowIfNull(eventPreparationQueryService);
        ArgumentNullException.ThrowIfNull(activeLearningProfileCacheService);
        ArgumentNullException.ThrowIfNull(featureAccessService);
        ArgumentNullException.ThrowIfNull(speechPlaybackService);

        InitializeComponent();
        _dialogueLessonQueryService = dialogueLessonQueryService;
        _conversationStarterQueryService = conversationStarterQueryService;
        _eventPreparationQueryService = eventPreparationQueryService;
        _activeLearningProfileCacheService = activeLearningProfileCacheService;
        _featureAccessService = featureAccessService;
        _speechPlaybackService = speechPlaybackService;

        StarterPacksCollectionView.ItemsSource = _starterPacks;
        EventPreparationPacksCollectionView.ItemsSource = _eventPreparationPacks;
        DialogueTurnsCollectionView.ItemsSource = _dialogueTurns;
        PhrasesCollectionView.ItemsSource = _phrases;
        QuestionsCollectionView.ItemsSource = _questions;
    }

    public string DialogueSlug
    {
        get => _dialogueSlug;
        set => _dialogueSlug = Uri.UnescapeDataString(value ?? string.Empty);
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
        CancelSpeechRequest();
        base.OnDisappearing();
    }

    private async Task RefreshAsync()
    {
        ResetRefreshRequest();
        CancellationToken cancellationToken = _refreshCancellationTokenSource!.Token;

        Title = "Dialogue";
        EmptyStateLabel.Text = "Dialogue not found.";
        LoadingStateView.Message = AppStrings.CommonStateLoading;
        LoadingStateView.IsLoading = true;
        ContentScrollView.IsVisible = false;
        EmptyStateLabel.IsVisible = false;

        if (string.IsNullOrWhiteSpace(DialogueSlug))
        {
            ShowDialogue(null);
            return;
        }

        UserLearningProfileModel profile = await _activeLearningProfileCacheService
            .GetCurrentProfileAsync(cancellationToken)
            .ConfigureAwait(true);
        string? secondaryMeaningLanguageCode = await _featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(true);

        DialogueLessonDetailModel? dialogue = await _dialogueLessonQueryService
            .GetPublishedDialogueBySlugAsync(
                DialogueSlug,
                profile.PreferredMeaningLanguage1,
                secondaryMeaningLanguageCode,
                cancellationToken)
            .ConfigureAwait(true);

        IReadOnlyList<ConversationStarterPackListItemModel> relatedStarterPacks = dialogue is null
            ? []
            : await _conversationStarterQueryService
                .GetPublishedStarterPacksForDialogueAsync(DialogueSlug, cancellationToken)
                .ConfigureAwait(true);

        IReadOnlyList<EventPreparationPackListItemModel> relatedEventPreparationPacks =
            dialogue is null || !await _featureAccessService.CanUseEventPreparationPacksAsync(cancellationToken).ConfigureAwait(true)
                ? []
                : await _eventPreparationQueryService
                    .GetPublishedEventPreparationPacksForDialogueAsync(DialogueSlug, cancellationToken)
                    .ConfigureAwait(true);

        ShowDialogue(dialogue, relatedStarterPacks, relatedEventPreparationPacks);
    }

    private void ShowDialogue(
        DialogueLessonDetailModel? dialogue,
        IReadOnlyList<ConversationStarterPackListItemModel>? relatedStarterPacks = null,
        IReadOnlyList<EventPreparationPackListItemModel>? relatedEventPreparationPacks = null)
    {
        _starterPacks.Clear();
        _eventPreparationPacks.Clear();
        _dialogueTurns.Clear();
        _phrases.Clear();
        _questions.Clear();

        if (dialogue is null)
        {
            LoadingStateView.IsLoading = false;
            ContentScrollView.IsVisible = true;
            EmptyStateLabel.IsVisible = true;
            StarterPacksSection.IsVisible = false;
            EventPreparationPacksSection.IsVisible = false;
            DialogueSection.IsVisible = false;
            PhrasesSection.IsVisible = false;
            QuestionsSection.IsVisible = false;
            return;
        }

        Title = dialogue.Title;
        MetadataLabel.Text = $"{dialogue.CefrLevel} • {dialogue.Category} • {string.Join(", ", dialogue.TopicKeys)}";
        HeadlineLabel.Text = dialogue.Title;
        DescriptionLabel.Text = dialogue.Description;
        GoalLabel.Text = dialogue.LearnerGoal;

        foreach (ConversationStarterPackListItemModel starterPack in relatedStarterPacks ?? [])
        {
            _starterPacks.Add(new DialogueStarterPackItemViewModel(
                starterPack.Slug,
                starterPack.Title,
                starterPack.Description,
                $"{starterPack.CefrLevel} • {starterPack.Situation} • {starterPack.Tone}"));
        }

        foreach (EventPreparationPackListItemModel preparationPack in relatedEventPreparationPacks ?? [])
        {
            _eventPreparationPacks.Add(new DialogueEventPreparationPackItemViewModel(
                preparationPack.Slug,
                preparationPack.Title,
                preparationPack.Description,
                $"{preparationPack.CefrLevel} • {preparationPack.EventType} • {preparationPack.Category}",
                preparationPack.LinkedConversationStarterPackSlugs.Count == 0
                    ? string.Empty
                    : $"Starter packs: {string.Join(", ", preparationPack.LinkedConversationStarterPackSlugs)}",
                preparationPack.LinkedConversationStarterPackSlugs.Count > 0));
        }

        foreach (DialogueTurnModel turn in dialogue.DialogueTurns)
        {
            _dialogueTurns.Add(new DialogueTextItemViewModel(
                turn.SpeakerRole,
                turn.BaseText,
                turn.PrimaryMeaning ?? string.Empty,
                !string.IsNullOrWhiteSpace(turn.PrimaryMeaning),
                turn.SecondaryMeaning ?? string.Empty,
                !string.IsNullOrWhiteSpace(turn.SecondaryMeaning)));
        }

        foreach (DialoguePhraseModel phrase in dialogue.UsefulPhrases)
        {
            _phrases.Add(new DialoguePhraseItemViewModel(
                phrase.BaseText,
                phrase.PrimaryMeaning ?? string.Empty,
                !string.IsNullOrWhiteSpace(phrase.PrimaryMeaning),
                phrase.SecondaryMeaning ?? string.Empty,
                !string.IsNullOrWhiteSpace(phrase.SecondaryMeaning),
                phrase.UsageNote ?? string.Empty,
                !string.IsNullOrWhiteSpace(phrase.UsageNote)));
        }

        foreach (DialogueQuestionModel question in dialogue.Questions)
        {
            _questions.Add(new DialogueQuestionItemViewModel(
                question.Prompt,
                question.PrimaryMeaning ?? string.Empty,
                !string.IsNullOrWhiteSpace(question.PrimaryMeaning),
                question.SecondaryMeaning ?? string.Empty,
                !string.IsNullOrWhiteSpace(question.SecondaryMeaning),
                string.Join(Environment.NewLine, question.Answers.Select(BuildAnswerLine))));
        }

        StarterPacksSection.IsVisible = _starterPacks.Count > 0;
        EventPreparationPacksSection.IsVisible = _eventPreparationPacks.Count > 0;
        DialogueSection.IsVisible = _dialogueTurns.Count > 0;
        PhrasesSection.IsVisible = _phrases.Count > 0;
        QuestionsSection.IsVisible = _questions.Count > 0;
        EmptyStateLabel.IsVisible = false;
        ContentScrollView.IsVisible = true;
        LoadingStateView.IsLoading = false;
    }

    private async void OnStarterPacksSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not DialogueStarterPackItemViewModel selectedStarterPack)
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

    private async void OnEventPreparationPacksSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not DialogueEventPreparationPackItemViewModel selectedPreparationPack)
        {
            return;
        }

        EventPreparationPacksCollectionView.SelectedItem = null;
        string escapedSlug = Uri.EscapeDataString(selectedPreparationPack.Slug);

        try
        {
            await Shell.Current.GoToAsync($"{nameof(EventPreparationPackDetailPage)}?preparationPackSlug={escapedSlug}").ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async void OnSpeakItemClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        string? text = button.BindingContext switch
        {
            DialogueTextItemViewModel turn => turn.BaseText,
            DialoguePhraseItemViewModel phrase => phrase.BaseText,
            _ => null,
        };

        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        CancelSpeechRequest();
        _speechCancellationTokenSource = new CancellationTokenSource();

        try
        {
            await _speechPlaybackService
                .SpeakAsync(text, "de-DE", _speechCancellationTokenSource.Token)
                .ConfigureAwait(true);
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

    private void CancelSpeechRequest()
    {
        if (_speechCancellationTokenSource is null)
        {
            return;
        }

        _speechCancellationTokenSource.Cancel();
        _speechCancellationTokenSource.Dispose();
        _speechCancellationTokenSource = null;
    }

    private static string BuildAnswerLine(DialogueAnswerModel answer)
    {
        string marker = answer.IsCorrect ? "✓ " : string.Empty;
        string meaning = answer.PrimaryMeaning ?? answer.SecondaryMeaning ?? string.Empty;
        return string.IsNullOrWhiteSpace(meaning)
            ? $"{marker}{answer.Text}"
            : $"{marker}{answer.Text} - {meaning}";
    }

    private sealed record DialogueTextItemViewModel(
        string SpeakerRole,
        string BaseText,
        string PrimaryMeaning,
        bool HasPrimaryMeaning,
        string SecondaryMeaning,
        bool HasSecondaryMeaning);

    private sealed record DialogueStarterPackItemViewModel(
        string Slug,
        string Title,
        string Description,
        string MetadataLine);

    private sealed record DialogueEventPreparationPackItemViewModel(
        string Slug,
        string Title,
        string Description,
        string MetadataLine,
        string LinkedStarterPacksLine,
        bool HasLinkedStarterPacks);

    private sealed record DialoguePhraseItemViewModel(
        string BaseText,
        string PrimaryMeaning,
        bool HasPrimaryMeaning,
        string SecondaryMeaning,
        bool HasSecondaryMeaning,
        string UsageNote,
        bool HasUsageNote);

    private sealed record DialogueQuestionItemViewModel(
        string Prompt,
        string PrimaryMeaning,
        bool HasPrimaryMeaning,
        string SecondaryMeaning,
        bool HasSecondaryMeaning,
        string AnswerSummary);
}

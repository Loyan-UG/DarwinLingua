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
/// Displays dialogue, phrases, and quick checks for one scenario lesson.
/// </summary>
[QueryProperty(nameof(ScenarioSlug), "scenarioSlug")]
public partial class ScenarioDetailPage : ContentPage
{
    private readonly IScenarioLessonQueryService _scenarioLessonQueryService;
    private readonly IConversationStarterQueryService _conversationStarterQueryService;
    private readonly IActiveLearningProfileCacheService _activeLearningProfileCacheService;
    private readonly IMobileEntitledFeatureAccessService _featureAccessService;
    private readonly ISpeechPlaybackService _speechPlaybackService;
    private readonly ObservableCollection<ScenarioStarterPackItemViewModel> _starterPacks = [];
    private readonly ObservableCollection<ScenarioTextItemViewModel> _dialogueTurns = [];
    private readonly ObservableCollection<ScenarioPhraseItemViewModel> _phrases = [];
    private readonly ObservableCollection<ScenarioQuestionItemViewModel> _questions = [];
    private CancellationTokenSource? _refreshCancellationTokenSource;
    private CancellationTokenSource? _speechCancellationTokenSource;
    private string _scenarioSlug = string.Empty;

    public ScenarioDetailPage(
        IScenarioLessonQueryService scenarioLessonQueryService,
        IConversationStarterQueryService conversationStarterQueryService,
        IActiveLearningProfileCacheService activeLearningProfileCacheService,
        IMobileEntitledFeatureAccessService featureAccessService,
        ISpeechPlaybackService speechPlaybackService)
    {
        ArgumentNullException.ThrowIfNull(scenarioLessonQueryService);
        ArgumentNullException.ThrowIfNull(conversationStarterQueryService);
        ArgumentNullException.ThrowIfNull(activeLearningProfileCacheService);
        ArgumentNullException.ThrowIfNull(featureAccessService);
        ArgumentNullException.ThrowIfNull(speechPlaybackService);

        InitializeComponent();
        _scenarioLessonQueryService = scenarioLessonQueryService;
        _conversationStarterQueryService = conversationStarterQueryService;
        _activeLearningProfileCacheService = activeLearningProfileCacheService;
        _featureAccessService = featureAccessService;
        _speechPlaybackService = speechPlaybackService;

        StarterPacksCollectionView.ItemsSource = _starterPacks;
        DialogueTurnsCollectionView.ItemsSource = _dialogueTurns;
        PhrasesCollectionView.ItemsSource = _phrases;
        QuestionsCollectionView.ItemsSource = _questions;
    }

    public string ScenarioSlug
    {
        get => _scenarioSlug;
        set => _scenarioSlug = Uri.UnescapeDataString(value ?? string.Empty);
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

        Title = "Scenario";
        EmptyStateLabel.Text = "Scenario not found.";
        LoadingStateView.Message = AppStrings.CommonStateLoading;
        LoadingStateView.IsLoading = true;
        ContentScrollView.IsVisible = false;
        EmptyStateLabel.IsVisible = false;

        if (string.IsNullOrWhiteSpace(ScenarioSlug))
        {
            ShowScenario(null);
            return;
        }

        UserLearningProfileModel profile = await _activeLearningProfileCacheService
            .GetCurrentProfileAsync(cancellationToken)
            .ConfigureAwait(true);
        string? secondaryMeaningLanguageCode = await _featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(true);

        ScenarioLessonDetailModel? scenario = await _scenarioLessonQueryService
            .GetPublishedScenarioBySlugAsync(
                ScenarioSlug,
                profile.PreferredMeaningLanguage1,
                secondaryMeaningLanguageCode,
                cancellationToken)
            .ConfigureAwait(true);

        IReadOnlyList<ConversationStarterPackListItemModel> relatedStarterPacks = scenario is null
            ? []
            : await _conversationStarterQueryService
                .GetPublishedStarterPacksForScenarioAsync(ScenarioSlug, cancellationToken)
                .ConfigureAwait(true);

        ShowScenario(scenario, relatedStarterPacks);
    }

    private void ShowScenario(
        ScenarioLessonDetailModel? scenario,
        IReadOnlyList<ConversationStarterPackListItemModel>? relatedStarterPacks = null)
    {
        _starterPacks.Clear();
        _dialogueTurns.Clear();
        _phrases.Clear();
        _questions.Clear();

        if (scenario is null)
        {
            LoadingStateView.IsLoading = false;
            ContentScrollView.IsVisible = true;
            EmptyStateLabel.IsVisible = true;
            StarterPacksSection.IsVisible = false;
            DialogueSection.IsVisible = false;
            PhrasesSection.IsVisible = false;
            QuestionsSection.IsVisible = false;
            return;
        }

        Title = scenario.Title;
        MetadataLabel.Text = $"{scenario.CefrLevel} • {scenario.Category} • {string.Join(", ", scenario.TopicKeys)}";
        HeadlineLabel.Text = scenario.Title;
        DescriptionLabel.Text = scenario.Description;
        GoalLabel.Text = scenario.LearnerGoal;

        foreach (ConversationStarterPackListItemModel starterPack in relatedStarterPacks ?? [])
        {
            _starterPacks.Add(new ScenarioStarterPackItemViewModel(
                starterPack.Slug,
                starterPack.Title,
                starterPack.Description,
                $"{starterPack.CefrLevel} • {starterPack.Situation} • {starterPack.Tone}"));
        }

        foreach (ScenarioDialogueTurnModel turn in scenario.DialogueTurns)
        {
            _dialogueTurns.Add(new ScenarioTextItemViewModel(
                turn.SpeakerRole,
                turn.BaseText,
                turn.PrimaryMeaning ?? string.Empty,
                !string.IsNullOrWhiteSpace(turn.PrimaryMeaning),
                turn.SecondaryMeaning ?? string.Empty,
                !string.IsNullOrWhiteSpace(turn.SecondaryMeaning)));
        }

        foreach (ScenarioPhraseModel phrase in scenario.UsefulPhrases)
        {
            _phrases.Add(new ScenarioPhraseItemViewModel(
                phrase.BaseText,
                phrase.PrimaryMeaning ?? string.Empty,
                !string.IsNullOrWhiteSpace(phrase.PrimaryMeaning),
                phrase.SecondaryMeaning ?? string.Empty,
                !string.IsNullOrWhiteSpace(phrase.SecondaryMeaning),
                phrase.UsageNote ?? string.Empty,
                !string.IsNullOrWhiteSpace(phrase.UsageNote)));
        }

        foreach (ScenarioQuestionModel question in scenario.Questions)
        {
            _questions.Add(new ScenarioQuestionItemViewModel(
                question.Prompt,
                question.PrimaryMeaning ?? string.Empty,
                !string.IsNullOrWhiteSpace(question.PrimaryMeaning),
                question.SecondaryMeaning ?? string.Empty,
                !string.IsNullOrWhiteSpace(question.SecondaryMeaning),
                string.Join(Environment.NewLine, question.Answers.Select(BuildAnswerLine))));
        }

        StarterPacksSection.IsVisible = _starterPacks.Count > 0;
        DialogueSection.IsVisible = _dialogueTurns.Count > 0;
        PhrasesSection.IsVisible = _phrases.Count > 0;
        QuestionsSection.IsVisible = _questions.Count > 0;
        EmptyStateLabel.IsVisible = false;
        ContentScrollView.IsVisible = true;
        LoadingStateView.IsLoading = false;
    }

    private async void OnStarterPacksSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ScenarioStarterPackItemViewModel selectedStarterPack)
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

    private async void OnSpeakItemClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        string? text = button.BindingContext switch
        {
            ScenarioTextItemViewModel turn => turn.BaseText,
            ScenarioPhraseItemViewModel phrase => phrase.BaseText,
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

    private static string BuildAnswerLine(ScenarioAnswerModel answer)
    {
        string marker = answer.IsCorrect ? "✓ " : string.Empty;
        string meaning = answer.PrimaryMeaning ?? answer.SecondaryMeaning ?? string.Empty;
        return string.IsNullOrWhiteSpace(meaning)
            ? $"{marker}{answer.Text}"
            : $"{marker}{answer.Text} - {meaning}";
    }

    private sealed record ScenarioTextItemViewModel(
        string SpeakerRole,
        string BaseText,
        string PrimaryMeaning,
        bool HasPrimaryMeaning,
        string SecondaryMeaning,
        bool HasSecondaryMeaning);

    private sealed record ScenarioStarterPackItemViewModel(
        string Slug,
        string Title,
        string Description,
        string MetadataLine);

    private sealed record ScenarioPhraseItemViewModel(
        string BaseText,
        string PrimaryMeaning,
        bool HasPrimaryMeaning,
        string SecondaryMeaning,
        bool HasSecondaryMeaning,
        string UsageNote,
        bool HasUsageNote);

    private sealed record ScenarioQuestionItemViewModel(
        string Prompt,
        string PrimaryMeaning,
        bool HasPrimaryMeaning,
        string SecondaryMeaning,
        bool HasSecondaryMeaning,
        string AnswerSummary);
}

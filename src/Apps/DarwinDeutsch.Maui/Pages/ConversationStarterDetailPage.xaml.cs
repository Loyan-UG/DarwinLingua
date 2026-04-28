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
/// Displays one conversation starter pack with dual meaning-language phrases.
/// </summary>
[QueryProperty(nameof(StarterPackSlug), "starterPackSlug")]
public partial class ConversationStarterDetailPage : ContentPage
{
    private readonly IConversationStarterQueryService _conversationStarterQueryService;
    private readonly IActiveLearningProfileCacheService _activeLearningProfileCacheService;
    private readonly IMobileEntitledFeatureAccessService _featureAccessService;
    private readonly ISpeechPlaybackService _speechPlaybackService;
    private readonly ObservableCollection<StarterPhraseItemViewModel> _phrases = [];
    private CancellationTokenSource? _refreshCancellationTokenSource;
    private CancellationTokenSource? _speechCancellationTokenSource;
    private string _starterPackSlug = string.Empty;

    public ConversationStarterDetailPage(
        IConversationStarterQueryService conversationStarterQueryService,
        IActiveLearningProfileCacheService activeLearningProfileCacheService,
        IMobileEntitledFeatureAccessService featureAccessService,
        ISpeechPlaybackService speechPlaybackService)
    {
        ArgumentNullException.ThrowIfNull(conversationStarterQueryService);
        ArgumentNullException.ThrowIfNull(activeLearningProfileCacheService);
        ArgumentNullException.ThrowIfNull(featureAccessService);
        ArgumentNullException.ThrowIfNull(speechPlaybackService);

        InitializeComponent();
        _conversationStarterQueryService = conversationStarterQueryService;
        _activeLearningProfileCacheService = activeLearningProfileCacheService;
        _featureAccessService = featureAccessService;
        _speechPlaybackService = speechPlaybackService;

        PhrasesCollectionView.ItemsSource = _phrases;
    }

    public string StarterPackSlug
    {
        get => _starterPackSlug;
        set => _starterPackSlug = Uri.UnescapeDataString(value ?? string.Empty);
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

        Title = "Starter";
        EmptyStateLabel.Text = "Conversation starter pack not found.";
        LoadingStateView.Message = AppStrings.CommonStateLoading;
        LoadingStateView.IsLoading = true;
        ContentScrollView.IsVisible = false;
        EmptyStateLabel.IsVisible = false;

        if (string.IsNullOrWhiteSpace(StarterPackSlug))
        {
            ShowStarterPack(null);
            return;
        }

        UserLearningProfileModel profile = await _activeLearningProfileCacheService
            .GetCurrentProfileAsync(cancellationToken)
            .ConfigureAwait(true);
        string? secondaryMeaningLanguageCode = await _featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(true);

        ConversationStarterPackDetailModel? starterPack = await _conversationStarterQueryService
            .GetPublishedStarterPackBySlugAsync(
                StarterPackSlug,
                profile.PreferredMeaningLanguage1,
                secondaryMeaningLanguageCode,
                cancellationToken)
            .ConfigureAwait(true);

        ShowStarterPack(starterPack);
    }

    private void ShowStarterPack(ConversationStarterPackDetailModel? starterPack)
    {
        _phrases.Clear();

        if (starterPack is null)
        {
            LoadingStateView.IsLoading = false;
            ContentScrollView.IsVisible = true;
            EmptyStateLabel.IsVisible = true;
            PhrasesSection.IsVisible = false;
            return;
        }

        Title = starterPack.Title;
        MetadataLabel.Text = $"{starterPack.CefrLevel} • {starterPack.Situation} • {starterPack.Tone} • {string.Join(", ", starterPack.TopicKeys)}";
        HeadlineLabel.Text = starterPack.Title;
        DescriptionLabel.Text = starterPack.Description;
        GoalLabel.Text = starterPack.ConversationGoal;

        foreach (ConversationStarterPhraseModel phrase in starterPack.Phrases)
        {
            string alternativesLine = phrase.AlternativeBaseTexts.Count == 0
                ? string.Empty
                : string.Join(Environment.NewLine, phrase.AlternativeBaseTexts);

            _phrases.Add(new StarterPhraseItemViewModel(
                phrase.BaseText,
                phrase.Function,
                phrase.PrimaryMeaning ?? string.Empty,
                !string.IsNullOrWhiteSpace(phrase.PrimaryMeaning),
                phrase.SecondaryMeaning ?? string.Empty,
                !string.IsNullOrWhiteSpace(phrase.SecondaryMeaning),
                alternativesLine,
                !string.IsNullOrWhiteSpace(alternativesLine),
                phrase.UsageNote ?? string.Empty,
                !string.IsNullOrWhiteSpace(phrase.UsageNote),
                phrase.CommonMistake ?? string.Empty,
                !string.IsNullOrWhiteSpace(phrase.CommonMistake)));
        }

        PhrasesSection.IsVisible = _phrases.Count > 0;
        EmptyStateLabel.IsVisible = false;
        ContentScrollView.IsVisible = true;
        LoadingStateView.IsLoading = false;
    }

    private async void OnSpeakItemClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: StarterPhraseItemViewModel phrase })
        {
            return;
        }

        CancelSpeechRequest();
        _speechCancellationTokenSource = new CancellationTokenSource();

        try
        {
            await _speechPlaybackService
                .SpeakAsync(phrase.BaseText, "de-DE", _speechCancellationTokenSource.Token)
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

    private sealed record StarterPhraseItemViewModel(
        string BaseText,
        string Function,
        string PrimaryMeaning,
        bool HasPrimaryMeaning,
        string SecondaryMeaning,
        bool HasSecondaryMeaning,
        string AlternativesLine,
        bool HasAlternatives,
        string UsageNote,
        bool HasUsageNote,
        string CommonMistake,
        bool HasCommonMistake);
}

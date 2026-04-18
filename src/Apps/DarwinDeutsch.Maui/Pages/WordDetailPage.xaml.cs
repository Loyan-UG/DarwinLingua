using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Audio;
using DarwinDeutsch.Maui.Services.Browse;
using DarwinDeutsch.Maui.Services.Browse.Models;
using DarwinDeutsch.Maui.Services.Diagnostics;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Buttons;
using Syncfusion.Maui.Toolkit.Chips;
using System.Diagnostics;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays the detail view of a selected lexical entry.
/// </summary>
[QueryProperty(nameof(WordPublicId), "wordPublicId")]
[QueryProperty(nameof(CefrLevel), "cefrLevel")]
public partial class WordDetailPage : ContentPage
{
    private const int DeferredSenseBatchSize = 2;
    private readonly IWordDetailCacheService _wordDetailCacheService;
    private readonly ICefrBrowseStateService _cefrBrowseStateService;
    private readonly IActiveLearningProfileCacheService _activeLearningProfileCacheService;
    private readonly IUserFavoriteWordService _userFavoriteWordService;
    private readonly IUserWordStateService _userWordStateService;
    private readonly ISpeechPlaybackService _speechPlaybackService;
    private readonly IPerformanceTelemetryService _performanceTelemetryService;
    private readonly ILogger<WordDetailPage> _logger;
    private string _wordPublicId = string.Empty;
    private string _cefrLevel = string.Empty;
    private bool _isFavorite;
    private UserWordStateModel? _userWordState;
    private CefrBrowseNavigationState? _cefrBrowseNavigationState;
    private CancellationTokenSource? _speechCancellationTokenSource;
    private CancellationTokenSource? _refreshCancellationTokenSource;
    private Guid? _lastTrackedWordPublicId;
    private Guid? _loadedWordPublicId;
    private string _loadedPrimaryMeaningLanguageCode = string.Empty;
    private string _loadedSecondaryMeaningLanguageCode = string.Empty;
    private string _loadedUiLanguageCode = string.Empty;
    private bool _isNavigatingBetweenWords;

    /// <summary>
    /// Initializes a new instance of the <see cref="WordDetailPage"/> class.
    /// </summary>
    public WordDetailPage(
        IWordDetailCacheService wordDetailCacheService,
        ICefrBrowseStateService cefrBrowseStateService,
        IActiveLearningProfileCacheService activeLearningProfileCacheService,
        IUserFavoriteWordService userFavoriteWordService,
        IUserWordStateService userWordStateService,
        ISpeechPlaybackService speechPlaybackService,
        IPerformanceTelemetryService performanceTelemetryService,
        ILogger<WordDetailPage> logger)
    {
        ArgumentNullException.ThrowIfNull(wordDetailCacheService);
        ArgumentNullException.ThrowIfNull(cefrBrowseStateService);
        ArgumentNullException.ThrowIfNull(activeLearningProfileCacheService);
        ArgumentNullException.ThrowIfNull(userFavoriteWordService);
        ArgumentNullException.ThrowIfNull(userWordStateService);
        ArgumentNullException.ThrowIfNull(speechPlaybackService);
        ArgumentNullException.ThrowIfNull(performanceTelemetryService);
        ArgumentNullException.ThrowIfNull(logger);

        InitializeComponent();

        _wordDetailCacheService = wordDetailCacheService;
        _cefrBrowseStateService = cefrBrowseStateService;
        _activeLearningProfileCacheService = activeLearningProfileCacheService;
        _userFavoriteWordService = userFavoriteWordService;
        _userWordStateService = userWordStateService;
        _speechPlaybackService = speechPlaybackService;
        _performanceTelemetryService = performanceTelemetryService;
        _logger = logger;
    }

    /// <summary>
    /// Gets or sets the public identifier of the selected lexical entry.
    /// </summary>
    public string WordPublicId
    {
        get => _wordPublicId;
        set => _wordPublicId = Uri.UnescapeDataString(value ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets the optional CEFR level that enables previous/next navigation within one level.
    /// </summary>
    public string CefrLevel
    {
        get => _cefrLevel;
        set => _cefrLevel = Uri.UnescapeDataString(value ?? string.Empty);
    }

    /// <summary>
    /// Refreshes the detail page whenever it becomes visible.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch
        {
            ShowLoadFailureState(AppStrings.WordDetailLoadFailed);
        }
    }

    /// <summary>
    /// Cancels any in-flight pronunciation request when the page is no longer visible.
    /// </summary>
    protected override void OnDisappearing()
    {
        CancelSpeechRequest();
        CancelRefreshRequest();

        base.OnDisappearing();
    }

    /// <summary>
    /// Loads the selected lexical entry using the current profile language preferences.
    /// </summary>
    private async Task RefreshAsync()
    {
        ResetRefreshRequest();
        CancellationToken cancellationToken = _refreshCancellationTokenSource!.Token;
        Stopwatch stopwatch = Stopwatch.StartNew();
        bool shouldShowSkeleton = _loadedWordPublicId is null || !ContentStackLayout.IsVisible || _isNavigatingBetweenWords;
        SetLoadingState(shouldShowSkeleton);

        Title = AppStrings.WordDetailTitle;
        TopicsSectionView.SectionTitle = AppStrings.WordDetailTopicsLabel;
        LearningStateSectionView.SectionTitle = AppStrings.WordDetailLearningStateLabel;
        UsageLabelsHeadingLabel.Text = AppStrings.WordDetailUsageLabelsLabel;
        ContextLabelsHeadingLabel.Text = AppStrings.WordDetailContextLabelsLabel;
        GrammarNotesHeadingLabel.Text = AppStrings.WordDetailGrammarNotesLabel;
        CollocationsHeadingLabel.Text = AppStrings.WordDetailCollocationsLabel;
        WordFamiliesHeadingLabel.Text = AppStrings.WordDetailWordFamiliesLabel;
        LexicalRelationsHeadingLabel.Text = AppStrings.WordDetailLexicalRelationsLabel;
        SynonymsHeadingLabel.Text = AppStrings.WordDetailSynonymsLabel;
        AntonymsHeadingLabel.Text = AppStrings.WordDetailAntonymsLabel;
        EmptyStateLabel.Text = AppStrings.WordDetailNotFound;
        PreviousWordButtonTop.Text = AppStrings.WordDetailPreviousWordButton;
        PreviousWordButtonBottom.Text = AppStrings.WordDetailPreviousWordButton;
        ShowWordListButtonTop.Text = AppStrings.WordDetailWordListButton;
        ShowWordListButtonBottom.Text = AppStrings.WordDetailWordListButton;
        NextWordButtonTop.Text = AppStrings.WordDetailNextWordButton;
        NextWordButtonBottom.Text = AppStrings.WordDetailNextWordButton;
        ClearAudioStatus();

        if (!Guid.TryParse(WordPublicId, out Guid publicId))
        {
            ShowNotFoundState();
            SetLoadingState(false);
            _performanceTelemetryService.Record("word-detail.refresh", stopwatch.Elapsed, PerformanceTelemetryOutcome.Failed);
            return;
        }

        UserLearningProfileModel profile = await _activeLearningProfileCacheService
            .GetCurrentProfileAsync(cancellationToken)
            .ConfigureAwait(true);

        if (IsLoadedDetailContext(publicId, profile))
        {
            await RefreshInteractiveStateAsync(publicId, cancellationToken).ConfigureAwait(true);
            await ApplyCefrNavigationStateAsync(publicId, cancellationToken).ConfigureAwait(true);

            if (!string.IsNullOrWhiteSpace(CefrLevel))
            {
                _ = PrefetchNavigationAsync(publicId);
            }

            ScheduleWordDetailPrefetch(profile);
            SetLoadingState(false);
            _performanceTelemetryService.Record("word-detail.refresh", stopwatch.Elapsed, PerformanceTelemetryOutcome.Success);
            return;
        }

        WordDetailModel? word = await _wordDetailCacheService
            .GetWordDetailsAsync(
                publicId,
                profile.PreferredMeaningLanguage1,
                profile.PreferredMeaningLanguage2,
                profile.UiLanguageCode,
                cancellationToken)
            .ConfigureAwait(true);

        if (word is null)
        {
            ShowNotFoundState();
            SetLoadingState(false);
            _performanceTelemetryService.Record("word-detail.refresh", stopwatch.Elapsed, PerformanceTelemetryOutcome.Failed);
            return;
        }

        PrepareForWordRender();
        await RefreshInteractiveStateAsync(publicId, cancellationToken).ConfigureAwait(true);

        Title = BuildHeadline(word);
        HeadlineLabel.Text = BuildHeadline(word);
        ConfigureSpeakWordButton();
        SpeakWordButton.IsVisible = true;
        MetadataLabel.Text = LexiconDisplayText.FormatMetadata(word.PartOfSpeech, word.CefrLevel);
        TopicsSectionView.SectionValue = word.Topics.Count == 0
            ? AppStrings.WordDetailNoTopics
            : string.Join(", ", word.Topics);
        EmptyStateLabel.IsVisible = false;
        RememberLoadedDetailContext(publicId, profile);
        ScheduleAutoSpeakWord(HeadlineLabel.Text ?? string.Empty);

        await ApplyCefrNavigationStateAsync(publicId, cancellationToken).ConfigureAwait(true);

        // Yield once so the headline and primary actions can render before heavier sections are materialized.
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        ApplyWordLabels(UsageLabelsChipGroup, UsageLabelsBorder, word.UsageLabels);
        ApplyWordLabels(ContextLabelsChipGroup, ContextLabelsBorder, word.ContextLabels);
        ApplyGrammarNotes(word.GrammarNotes);
        ApplyCollocations(word.Collocations);
        ApplyWordFamilies(word.WordFamilies);
        ApplyLexicalRelations(word.Synonyms, word.Antonyms);
        await RenderSensesAsync(word.Senses, cancellationToken).ConfigureAwait(true);

        if (!string.IsNullOrWhiteSpace(CefrLevel))
        {
            _ = PrefetchNavigationAsync(publicId);
        }

        ScheduleWordDetailPrefetch(profile);
        SetLoadingState(false);
        _performanceTelemetryService.Record("word-detail.refresh", stopwatch.Elapsed, PerformanceTelemetryOutcome.Success, word.Senses.Count);
    }

    /// <summary>
    /// Renders the first sense immediately and hydrates the rest in small UI-friendly batches.
    /// </summary>
    private async Task RenderSensesAsync(IReadOnlyList<WordSenseDetailModel> senses, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(senses);

        if (senses.Count == 0)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        SensesContainer.Children.Add(BuildSenseView(senses[0]));

        if (senses.Count == 1)
        {
            return;
        }

        for (int index = 1; index < senses.Count; index += DeferredSenseBatchSize)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();

            int batchEnd = Math.Min(index + DeferredSenseBatchSize, senses.Count);
            for (int batchIndex = index; batchIndex < batchEnd; batchIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                SensesContainer.Children.Add(BuildSenseView(senses[batchIndex]));
            }
        }
    }

    /// <summary>
    /// Shows the not-found state when the selected word is unavailable.
    /// </summary>
    private void ShowNotFoundState()
    {
        SetLoadingState(false);
        Title = AppStrings.WordDetailTitle;
        HeadlineLabel.Text = AppStrings.WordDetailTitle;
        SpeakWordButton.IsVisible = false;
        MetadataLabel.Text = string.Empty;
        FavoriteButton.IsVisible = false;
        KnownButton.IsVisible = false;
        DifficultButton.IsVisible = false;
        CefrNavigationTopGrid.IsVisible = false;
        CefrNavigationBottomGrid.IsVisible = false;
        UsageLabelsChipGroup.ItemsSource = Array.Empty<string>();
        ContextLabelsChipGroup.ItemsSource = Array.Empty<string>();
        GrammarNotesStackLayout.Children.Clear();
        CollocationsStackLayout.Children.Clear();
        WordFamiliesStackLayout.Children.Clear();
        SynonymsStackLayout.Children.Clear();
        AntonymsStackLayout.Children.Clear();
        UsageLabelsBorder.IsVisible = false;
        ContextLabelsBorder.IsVisible = false;
        GrammarNotesBorder.IsVisible = false;
        CollocationsBorder.IsVisible = false;
        WordFamiliesBorder.IsVisible = false;
        LexicalRelationsBorder.IsVisible = false;
        SynonymsSectionStackLayout.IsVisible = false;
        AntonymsSectionStackLayout.IsVisible = false;
        LearningStateSectionView.SectionValue = string.Empty;
        TopicsSectionView.SectionValue = string.Empty;
        ClearAudioStatus();
        EmptyStateLabel.IsVisible = true;
        ClearLoadedDetailContext();
    }

    /// <summary>
    /// Shows a non-crashing failure state when the detail view cannot be loaded.
    /// </summary>
    private void ShowLoadFailureState(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        ShowNotFoundState();
        EmptyStateLabel.Text = message;
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingStateView.IsLoading = isLoading;
        ContentStackLayout.IsVisible = !isLoading;
    }

    /// <summary>
    /// Toggles the favorite state of the current lexical entry.
    /// </summary>
    private async void OnFavoriteButtonClicked(object? sender, EventArgs e)
    {
        if (!Guid.TryParse(WordPublicId, out Guid publicId))
        {
            return;
        }

        try
        {
            _isFavorite = await _userFavoriteWordService
                .ToggleFavoriteAsync(publicId, _refreshCancellationTokenSource?.Token ?? CancellationToken.None)
                .ConfigureAwait(true);

            ApplyFavoriteButtonState();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to toggle favorite state for word {WordPublicId}.", publicId);
        }
    }

    /// <summary>
    /// Marks the current lexical entry as known.
    /// </summary>
    private async void OnKnownButtonClicked(object? sender, EventArgs e)
    {
        if (!Guid.TryParse(WordPublicId, out Guid publicId))
        {
            return;
        }

        try
        {
            _userWordState = _userWordState?.IsKnown == true
                ? await _userWordStateService.ClearWordKnownStateAsync(publicId, _refreshCancellationTokenSource?.Token ?? CancellationToken.None).ConfigureAwait(true)
                : await _userWordStateService.MarkWordKnownAsync(publicId, _refreshCancellationTokenSource?.Token ?? CancellationToken.None).ConfigureAwait(true);

            ApplyUserWordState();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to update known state for word {WordPublicId}.", publicId);
        }
    }

    /// <summary>
    /// Marks the current lexical entry as difficult.
    /// </summary>
    private async void OnDifficultButtonClicked(object? sender, EventArgs e)
    {
        if (!Guid.TryParse(WordPublicId, out Guid publicId))
        {
            return;
        }

        try
        {
            _userWordState = _userWordState?.IsDifficult == true
                ? await _userWordStateService.ClearWordDifficultStateAsync(publicId, _refreshCancellationTokenSource?.Token ?? CancellationToken.None).ConfigureAwait(true)
                : await _userWordStateService.MarkWordDifficultAsync(publicId, _refreshCancellationTokenSource?.Token ?? CancellationToken.None).ConfigureAwait(true);

            ApplyUserWordState();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to update difficult state for word {WordPublicId}.", publicId);
        }
    }

    /// <summary>
    /// Speaks the currently selected lexical headline using the platform TTS service.
    /// </summary>
    private async void OnSpeakWordButtonClicked(object? sender, EventArgs e)
    {
        string headline = HeadlineLabel.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(headline))
        {
            return;
        }

        try
        {
            await SpeakGermanTextAsync(headline, showFailureStatus: true).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Manual speech playback failed for word detail '{Text}'.", headline);
            AudioStatusLabel.Text = AppStrings.WordDetailAudioFailed;
            AudioStatusLabel.IsVisible = true;
        }
    }

    /// <summary>
    /// Builds the visual block for a single sense.
    /// </summary>
    private View BuildSenseView(WordSenseDetailModel sense)
    {
        ArgumentNullException.ThrowIfNull(sense);

        VerticalStackLayout senseLayout = new()
        {
            Spacing = 8,
        };

        if (!string.IsNullOrWhiteSpace(sense.ShortDefinitionDe))
        {
            senseLayout.Children.Add(new Label
            {
                Text = sense.ShortDefinitionDe,
                Style = ResolveAppTextStyle("Title2"),
            });
        }

        if (!string.IsNullOrWhiteSpace(sense.PrimaryMeaning))
        {
            senseLayout.Children.Add(new Label
            {
                Text = sense.PrimaryMeaning,
                Style = ResolveAppTextStyle("Body"),
            });
        }

        if (!string.IsNullOrWhiteSpace(sense.SecondaryMeaning))
        {
            senseLayout.Children.Add(new Label
            {
                Text = sense.SecondaryMeaning,
                Style = ResolveAppTextStyle("Body"),
            });
        }

        foreach (ExampleSentenceDetailModel example in sense.Examples)
        {
            VerticalStackLayout exampleLayout = new()
            {
                Spacing = 2,
                Margin = new Thickness(8, 0, 0, 0),
            };

            Grid exampleHeaderLayout = new()
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
                ColumnSpacing = 12,
            };

            Label exampleTextLabel = new()
            {
                Text = example.GermanText,
                Style = ResolveAppTextStyle("Body"),
            };
            SfButton speakExampleButton = BuildSpeechButton(example.GermanText);

            exampleHeaderLayout.Add(exampleTextLabel);
            exampleHeaderLayout.Add(speakExampleButton, 1, 0);
            exampleLayout.Children.Add(exampleHeaderLayout);

            if (!string.IsNullOrWhiteSpace(example.PrimaryMeaning))
            {
                exampleLayout.Children.Add(new Label
                {
                    Text = example.PrimaryMeaning,
                    Style = ResolveAppTextStyle("Body"),
                });
            }

            if (!string.IsNullOrWhiteSpace(example.SecondaryMeaning))
            {
                exampleLayout.Children.Add(new Label
                {
                    Text = example.SecondaryMeaning,
                    Style = ResolveAppTextStyle("Body"),
                });
            }

            senseLayout.Children.Add(exampleLayout);
        }

        return new Border
        {
            Padding = 16,
            BackgroundColor = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#14202B")
                : Color.FromArgb("#FFFDF9"),
            Content = senseLayout,
        };
    }

    /// <summary>
    /// Creates a speech action button for a German text fragment.
    /// </summary>
    /// <param name="spokenText">The German text to pronounce.</param>
    /// <returns>A configured button instance.</returns>
    private SfButton BuildSpeechButton(string spokenText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(spokenText);

        SfButton button = new()
        {
            Text = "\U0001F50A",
            Style = ResolveAppStyle("SyncfusionIconActionButtonStyle"),
            WidthRequest = 44,
        };
        SemanticProperties.SetDescription(button, AppStrings.WordDetailSpeakExampleButton);

        button.Clicked += async (_, _) => await SpeakGermanTextAsync(spokenText, showFailureStatus: true).ConfigureAwait(true);

        return button;
    }

    /// <summary>
    /// Builds the page headline for the current lexical entry.
    /// </summary>
    private static string BuildHeadline(WordDetailModel word)
    {
        ArgumentNullException.ThrowIfNull(word);

        return string.IsNullOrWhiteSpace(word.Article)
            ? word.Lemma
            : $"{word.Article} {word.Lemma}";
    }

    /// <summary>
    /// Resolves an application-scoped text style while tolerating early resource-initialization timing.
    /// </summary>
    private static Style? ResolveAppTextStyle(string resourceKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceKey);

        if (Application.Current?.Resources.TryGetValue(resourceKey, out object? style) == true)
        {
            return style as Style;
        }

        return null;
    }

    /// <summary>
    /// Resolves an application-scoped style while tolerating early resource-initialization timing.
    /// </summary>
    private static Style? ResolveAppStyle(string resourceKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceKey);

        if (Application.Current?.Resources.TryGetValue(resourceKey, out object? style) == true)
        {
            return style as Style;
        }

        return null;
    }

    /// <summary>
    /// Renders the supplied lexical labels as wrapped chips.
    /// </summary>
    private static void ApplyWordLabels(SfChipGroup container, Border hostBorder, IReadOnlyList<string> labelKeys)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(hostBorder);
        ArgumentNullException.ThrowIfNull(labelKeys);

        container.ItemsSource = Array.Empty<string>();
        hostBorder.IsVisible = labelKeys.Count > 0;

        if (labelKeys.Count == 0)
        {
            return;
        }

        container.ItemsSource = labelKeys
            .Select(LexiconTagDisplayText.GetDisplayName)
            .ToList();
    }

    /// <summary>
    /// Renders learner-facing grammar notes as stacked callouts.
    /// </summary>
    private void ApplyGrammarNotes(IReadOnlyList<string> grammarNotes)
    {
        ArgumentNullException.ThrowIfNull(grammarNotes);

        GrammarNotesStackLayout.Children.Clear();
        GrammarNotesBorder.IsVisible = grammarNotes.Count > 0;

        foreach (string grammarNote in grammarNotes)
        {
            GrammarNotesStackLayout.Children.Add(new Border
            {
                Padding = new Thickness(14, 12),
                StrokeThickness = 0,
                BackgroundColor = Application.Current?.RequestedTheme == AppTheme.Dark
                    ? Color.FromArgb("#1B2732")
                    : Color.FromArgb("#FFFDF9"),
                Content = new Label
                {
                    Text = grammarNote,
                    Style = ResolveAppTextStyle("Body"),
                },
            });
        }
    }

    /// <summary>
    /// Renders collocations as compact phrase cards with optional meaning hints.
    /// </summary>
    private void ApplyCollocations(IReadOnlyList<WordCollocationDetailModel> collocations)
    {
        ArgumentNullException.ThrowIfNull(collocations);

        CollocationsStackLayout.Children.Clear();
        CollocationsBorder.IsVisible = collocations.Count > 0;

        foreach (WordCollocationDetailModel collocation in collocations)
        {
            VerticalStackLayout content = new()
            {
                Spacing = 4,
            };

            content.Children.Add(new Label
            {
                Text = collocation.Text,
                Style = ResolveAppTextStyle("Title2"),
            });

            if (!string.IsNullOrWhiteSpace(collocation.Meaning))
            {
                content.Children.Add(new Label
                {
                    Text = collocation.Meaning,
                    Style = ResolveAppTextStyle("Body"),
                });
            }

            CollocationsStackLayout.Children.Add(new Border
            {
                Padding = new Thickness(14, 12),
                StrokeThickness = 0,
                BackgroundColor = Application.Current?.RequestedTheme == AppTheme.Dark
                    ? Color.FromArgb("#192A30")
                    : Color.FromArgb("#EEF6F3"),
                Content = content,
            });
        }
    }

    /// <summary>
    /// Renders word-family members as compact derivation cards.
    /// </summary>
    private void ApplyWordFamilies(IReadOnlyList<WordFamilyMemberDetailModel> wordFamilies)
    {
        ArgumentNullException.ThrowIfNull(wordFamilies);

        WordFamiliesStackLayout.Children.Clear();
        WordFamiliesBorder.IsVisible = wordFamilies.Count > 0;

        foreach (WordFamilyMemberDetailModel familyMember in wordFamilies)
        {
            Grid header = new()
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
                ColumnSpacing = 12,
            };

            header.Add(new Label
            {
                Text = familyMember.Lemma,
                Style = ResolveAppTextStyle("Title2"),
            });

            header.Add(new Label
            {
                Text = familyMember.RelationLabel,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Application.Current?.RequestedTheme == AppTheme.Dark
                    ? Color.FromArgb("#F7D6A2")
                    : Color.FromArgb("#9A5A09"),
                VerticalTextAlignment = TextAlignment.Center,
            }, 1, 0);

            VerticalStackLayout content = new()
            {
                Spacing = 4,
            };
            content.Children.Add(header);

            if (!string.IsNullOrWhiteSpace(familyMember.Note))
            {
                content.Children.Add(new Label
                {
                    Text = familyMember.Note,
                    Style = ResolveAppTextStyle("Body"),
                });
            }

            WordFamiliesStackLayout.Children.Add(new Border
            {
                Padding = new Thickness(14, 12),
                StrokeThickness = 0,
                BackgroundColor = Application.Current?.RequestedTheme == AppTheme.Dark
                    ? Color.FromArgb("#202922")
                    : Color.FromArgb("#FFF6E8"),
                Content = content,
            });
        }
    }

    /// <summary>
    /// Renders lexical relations in separate synonym and antonym groups.
    /// </summary>
    private void ApplyLexicalRelations(
        IReadOnlyList<WordRelationDetailModel> synonyms,
        IReadOnlyList<WordRelationDetailModel> antonyms)
    {
        ArgumentNullException.ThrowIfNull(synonyms);
        ArgumentNullException.ThrowIfNull(antonyms);

        SynonymsStackLayout.Children.Clear();
        AntonymsStackLayout.Children.Clear();

        ApplyRelationGroup(SynonymsStackLayout, SynonymsSectionStackLayout, synonyms, false);
        ApplyRelationGroup(AntonymsStackLayout, AntonymsSectionStackLayout, antonyms, true);

        LexicalRelationsBorder.IsVisible = synonyms.Count > 0 || antonyms.Count > 0;
    }

    /// <summary>
    /// Renders one lexical-relation group.
    /// </summary>
    private void ApplyRelationGroup(
        VerticalStackLayout host,
        VerticalStackLayout section,
        IReadOnlyList<WordRelationDetailModel> relations,
        bool emphasizeContrast)
    {
        section.IsVisible = relations.Count > 0;

        foreach (WordRelationDetailModel relation in relations)
        {
            VerticalStackLayout content = new()
            {
                Spacing = 4,
            };

            content.Children.Add(new Label
            {
                Text = relation.Lemma,
                Style = ResolveAppTextStyle("Title2"),
            });

            if (!string.IsNullOrWhiteSpace(relation.Note))
            {
                content.Children.Add(new Label
                {
                    Text = relation.Note,
                    Style = ResolveAppTextStyle("Body"),
                });
            }

            host.Children.Add(new Border
            {
                Padding = new Thickness(14, 12),
                StrokeThickness = 0,
                BackgroundColor = emphasizeContrast
                    ? (Application.Current?.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#30231D") : Color.FromArgb("#FFF1E7"))
                    : (Application.Current?.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#19302C") : Color.FromArgb("#EAF7F3")),
                Content = content,
            });
        }
    }

    /// <summary>
    /// Applies the current lightweight user-word-state snapshot to the page controls.
    /// </summary>
    private void ApplyUserWordState()
    {
        if (_userWordState is null)
        {
            LearningStateSectionView.SectionValue = AppStrings.WordDetailLearningStateUnknown;
            KnownButton.IsVisible = false;
            DifficultButton.IsVisible = false;
            return;
        }

        LearningStateSectionView.SectionValue = string.Format(AppStrings.WordDetailViewCountFormat, _userWordState.ViewCount);

        KnownButton.Text = _userWordState.IsKnown
            ? AppStrings.WordDetailClearKnownButton
            : AppStrings.WordDetailMarkKnownButton;
        KnownButton.IsVisible = true;
        KnownButton.IsEnabled = true;
        KnownButton.IsChecked = _userWordState.IsKnown;

        DifficultButton.Text = _userWordState.IsDifficult
            ? AppStrings.WordDetailClearDifficultButton
            : AppStrings.WordDetailMarkDifficultButton;
        DifficultButton.IsVisible = true;
        DifficultButton.IsEnabled = true;
        DifficultButton.IsChecked = _userWordState.IsDifficult;
    }

    /// <summary>
    /// Attempts to pronounce German content and surfaces any non-success outcome as a localized UI message.
    /// </summary>
    /// <param name="text">The German text to pronounce.</param>
    private void ScheduleAutoSpeakWord(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        _ = AutoSpeakWordAsync(text);
    }

    private async Task AutoSpeakWordAsync(string text)
    {
        try
        {
            await SpeakGermanTextAsync(text, showFailureStatus: false).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            _logger.LogDebug(exception, "Automatic speech playback failed for word detail '{Text}'.", text);
        }
    }

    private async Task SpeakGermanTextAsync(string text, bool showFailureStatus)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        ClearAudioStatus();
        ResetSpeechRequest();
        Stopwatch stopwatch = Stopwatch.StartNew();

        SpeechPlaybackResult result = await _speechPlaybackService
            .SpeakAsync(text, "de", _speechCancellationTokenSource!.Token)
            .ConfigureAwait(true);

        if (result.IsSuccess || result.Status == SpeechPlaybackStatus.Cancelled)
        {
            _performanceTelemetryService.Record(
                showFailureStatus ? "speech.playback" : "speech.playback.auto",
                stopwatch.Elapsed,
                result.Status == SpeechPlaybackStatus.Cancelled ? PerformanceTelemetryOutcome.Cancelled : PerformanceTelemetryOutcome.Success);
            return;
        }

        _performanceTelemetryService.Record(
            showFailureStatus ? "speech.playback" : "speech.playback.auto",
            stopwatch.Elapsed,
            PerformanceTelemetryOutcome.Failed);

        if (!showFailureStatus)
        {
            return;
        }

        AudioStatusLabel.Text = result.Status switch
        {
            SpeechPlaybackStatus.Unsupported => AppStrings.WordDetailAudioNotSupported,
            SpeechPlaybackStatus.LocaleUnavailable => AppStrings.WordDetailAudioLocaleUnavailable,
            _ => AppStrings.WordDetailAudioFailed,
        };
        AudioStatusLabel.IsVisible = true;
    }

    /// <summary>
    /// Clears the current audio status message from the page.
    /// </summary>
    private void ClearAudioStatus()
    {
        AudioStatusLabel.Text = string.Empty;
        AudioStatusLabel.IsVisible = false;
    }

    /// <summary>
    /// Replaces any active pronunciation request token with a fresh one.
    /// </summary>
    private void ResetSpeechRequest()
    {
        CancelSpeechRequest();
        _speechCancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Cancels and disposes the active pronunciation request token when one exists.
    /// </summary>
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

    private void ConfigureSpeakWordButton()
    {
        SpeakWordButton.Text = "\U0001F50A";
        SemanticProperties.SetDescription(SpeakWordButton, AppStrings.WordDetailSpeakWordButton);
    }

    private void ApplyFavoriteButtonState()
    {
        FavoriteButton.Text = _isFavorite ? "\u2665" : "\u2661";
        FavoriteButton.IsChecked = _isFavorite;
        SemanticProperties.SetDescription(
            FavoriteButton,
            _isFavorite ? AppStrings.WordDetailRemoveFavoriteButton : AppStrings.WordDetailAddFavoriteButton);
    }

    private async Task ApplyCefrNavigationStateAsync(Guid currentWordPublicId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(CefrLevel))
        {
            _cefrBrowseNavigationState = null;
            CefrNavigationTopGrid.IsVisible = false;
            CefrNavigationBottomGrid.IsVisible = false;
            return;
        }

        _cefrBrowseStateService.RememberLastViewedWord(CefrLevel, currentWordPublicId);
        _cefrBrowseNavigationState = await _cefrBrowseStateService
            .GetNavigationStateAsync(CefrLevel, currentWordPublicId, cancellationToken)
            .ConfigureAwait(true);

        bool isVisible = _cefrBrowseNavigationState.TotalCount > 0;
        CefrNavigationTopGrid.IsVisible = isVisible;
        CefrNavigationBottomGrid.IsVisible = isVisible;
        ApplyNavigationButtonState();
    }

    private async void OnPreviousWordButtonClicked(object? sender, EventArgs e)
    {
        if (_cefrBrowseNavigationState?.PreviousWordPublicId is not Guid previousWordPublicId)
        {
            return;
        }

        try
        {
            await NavigateToAdjacentWordAsync(previousWordPublicId, "previous").ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async void OnNextWordButtonClicked(object? sender, EventArgs e)
    {
        if (_cefrBrowseNavigationState?.NextWordPublicId is not Guid nextWordPublicId)
        {
            return;
        }

        try
        {
            await NavigateToAdjacentWordAsync(nextWordPublicId, "next").ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async void OnDetailSwipeLeft(object? sender, SwipedEventArgs e)
    {
        if (_cefrBrowseNavigationState?.NextWordPublicId is not Guid nextWordPublicId)
        {
            return;
        }

        try
        {
            await NavigateToAdjacentWordAsync(nextWordPublicId, "next").ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async void OnDetailSwipeRight(object? sender, SwipedEventArgs e)
    {
        if (_cefrBrowseNavigationState?.PreviousWordPublicId is not Guid previousWordPublicId)
        {
            return;
        }

        try
        {
            await NavigateToAdjacentWordAsync(previousWordPublicId, "previous").ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async void OnShowWordListButtonClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CefrLevel))
        {
            return;
        }

        string escapedCefrLevel = Uri.EscapeDataString(CefrLevel);
        try
        {
            await Shell.Current.GoToAsync($"{nameof(CefrWordsPage)}?cefrLevel={escapedCefrLevel}")
                .ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to navigate back to CEFR list for level {CefrLevel}.", CefrLevel);
            ShowLoadFailureState(AppStrings.WordDetailLoadFailed);
        }
    }

    private async Task<UserWordStateModel?> TrackWordViewedAsync(Guid publicId, CancellationToken cancellationToken)
    {
        UserWordStateModel userWordState = await _userWordStateService
            .TrackWordViewedAsync(publicId, cancellationToken)
            .ConfigureAwait(true);

        _lastTrackedWordPublicId = publicId;
        return userWordState;
    }

    private async Task RefreshInteractiveStateAsync(Guid publicId, CancellationToken cancellationToken)
    {
        Task<bool> favoriteTask = _userFavoriteWordService
            .IsFavoriteAsync(publicId, cancellationToken);
        Task<UserWordStateModel?> userWordStateTask = _lastTrackedWordPublicId == publicId
            ? _userWordStateService.GetWordStateAsync(publicId, cancellationToken)
            : TrackWordViewedAsync(publicId, cancellationToken);

        await Task.WhenAll(favoriteTask, userWordStateTask).ConfigureAwait(true);

        _isFavorite = favoriteTask.Result;
        _userWordState = userWordStateTask.Result;
        _lastTrackedWordPublicId = publicId;
        ApplyFavoriteButtonState();
        FavoriteButton.IsVisible = true;
        ApplyUserWordState();
    }

    private void ScheduleWordDetailPrefetch(UserLearningProfileModel profile)
    {
        if (_cefrBrowseNavigationState is null)
        {
            return;
        }

        Guid[] candidateIds =
        [
            _cefrBrowseNavigationState.PreviousWordPublicId ?? Guid.Empty,
            _cefrBrowseNavigationState.NextWordPublicId ?? Guid.Empty,
        ];

        foreach (Guid candidateId in candidateIds.Where(id => id != Guid.Empty).Distinct())
        {
            _ = PrefetchWordDetailAsync(candidateId, profile);
        }
    }

    private async Task PrefetchNavigationAsync(Guid publicId)
    {
        try
        {
            await _cefrBrowseStateService.PrefetchNavigationAsync(CefrLevel, publicId, CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task PrefetchWordDetailAsync(Guid candidateId, UserLearningProfileModel profile)
    {
        try
        {
            await _wordDetailCacheService.PrefetchWordDetailsAsync(
                    candidateId,
                    profile.PreferredMeaningLanguage1,
                    profile.PreferredMeaningLanguage2,
                    profile.UiLanguageCode,
                    CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogDebug(exception, "Word detail prefetch failed for word {WordPublicId}.", candidateId);
        }
    }

    private bool IsLoadedDetailContext(Guid publicId, UserLearningProfileModel profile)
    {
        return _loadedWordPublicId == publicId &&
               string.Equals(_loadedPrimaryMeaningLanguageCode, profile.PreferredMeaningLanguage1, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(_loadedSecondaryMeaningLanguageCode, profile.PreferredMeaningLanguage2 ?? string.Empty, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(_loadedUiLanguageCode, profile.UiLanguageCode, StringComparison.OrdinalIgnoreCase) &&
               !EmptyStateLabel.IsVisible;
    }

    private void RememberLoadedDetailContext(Guid publicId, UserLearningProfileModel profile)
    {
        _loadedWordPublicId = publicId;
        _loadedPrimaryMeaningLanguageCode = profile.PreferredMeaningLanguage1;
        _loadedSecondaryMeaningLanguageCode = profile.PreferredMeaningLanguage2 ?? string.Empty;
        _loadedUiLanguageCode = profile.UiLanguageCode;
    }

    private void ClearLoadedDetailContext()
    {
        _loadedWordPublicId = null;
        _loadedPrimaryMeaningLanguageCode = string.Empty;
        _loadedSecondaryMeaningLanguageCode = string.Empty;
        _loadedUiLanguageCode = string.Empty;
    }

    private void PrepareForWordRender()
    {
        SensesContainer.Children.Clear();
        UsageLabelsChipGroup.ItemsSource = Array.Empty<string>();
        ContextLabelsChipGroup.ItemsSource = Array.Empty<string>();
        GrammarNotesStackLayout.Children.Clear();
        CollocationsStackLayout.Children.Clear();
        WordFamiliesStackLayout.Children.Clear();
        SynonymsStackLayout.Children.Clear();
        AntonymsStackLayout.Children.Clear();
        UsageLabelsBorder.IsVisible = false;
        ContextLabelsBorder.IsVisible = false;
        GrammarNotesBorder.IsVisible = false;
        CollocationsBorder.IsVisible = false;
        WordFamiliesBorder.IsVisible = false;
        LexicalRelationsBorder.IsVisible = false;
        SynonymsSectionStackLayout.IsVisible = false;
        AntonymsSectionStackLayout.IsVisible = false;
        SpeakWordButton.IsVisible = false;
        FavoriteButton.IsVisible = false;
        KnownButton.IsVisible = false;
        DifficultButton.IsVisible = false;
        CefrNavigationTopGrid.IsVisible = false;
        CefrNavigationBottomGrid.IsVisible = false;
        LearningStateSectionView.SectionValue = string.Empty;
    }

    private void ResetRefreshRequest()
    {
        CancelRefreshRequest();
        _refreshCancellationTokenSource = new CancellationTokenSource();
    }

    private void ApplyNavigationButtonState()
    {
        bool hasPrevious = !_isNavigatingBetweenWords && _cefrBrowseNavigationState?.PreviousWordPublicId.HasValue == true;
        bool hasNext = !_isNavigatingBetweenWords && _cefrBrowseNavigationState?.NextWordPublicId.HasValue == true;
        bool canShowList = !_isNavigatingBetweenWords && !string.IsNullOrWhiteSpace(CefrLevel);

        PreviousWordButtonTop.IsEnabled = hasPrevious;
        PreviousWordButtonBottom.IsEnabled = hasPrevious;
        NextWordButtonTop.IsEnabled = hasNext;
        NextWordButtonBottom.IsEnabled = hasNext;
        ShowWordListButtonTop.IsEnabled = canShowList;
        ShowWordListButtonBottom.IsEnabled = canShowList;
    }

    private async Task NavigateToAdjacentWordAsync(Guid targetWordPublicId, string direction)
    {
        if (_isNavigatingBetweenWords)
        {
            return;
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        _isNavigatingBetweenWords = true;
        ApplyNavigationButtonState();

        try
        {
            WordPublicId = targetWordPublicId.ToString("D");
            await RefreshAsync().ConfigureAwait(true);
            _performanceTelemetryService.Record("word-detail.cefr-navigation", stopwatch.Elapsed, PerformanceTelemetryOutcome.Success);
        }
        catch (OperationCanceledException)
        {
            _performanceTelemetryService.Record("word-detail.cefr-navigation", stopwatch.Elapsed, PerformanceTelemetryOutcome.Cancelled);
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to navigate to {Direction} word {WordPublicId}.", direction, targetWordPublicId);
            _performanceTelemetryService.Record("word-detail.cefr-navigation", stopwatch.Elapsed, PerformanceTelemetryOutcome.Failed);
            ShowLoadFailureState(AppStrings.WordDetailLoadFailed);
        }
        finally
        {
            _isNavigatingBetweenWords = false;
            ApplyNavigationButtonState();
        }
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
}

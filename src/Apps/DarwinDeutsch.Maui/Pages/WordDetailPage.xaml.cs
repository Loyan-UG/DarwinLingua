using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Audio;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays the detail view of a selected lexical entry.
/// </summary>
[QueryProperty(nameof(WordPublicId), "wordPublicId")]
public partial class WordDetailPage : ContentPage
{
    private readonly IWordDetailQueryService _wordDetailQueryService;
    private readonly IUserLearningProfileService _userLearningProfileService;
    private readonly IUserFavoriteWordService _userFavoriteWordService;
    private readonly IUserWordStateService _userWordStateService;
    private readonly ISpeechPlaybackService _speechPlaybackService;
    private string _wordPublicId = string.Empty;
    private bool _isFavorite;
    private UserWordStateModel? _userWordState;
    private CancellationTokenSource? _speechCancellationTokenSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="WordDetailPage"/> class.
    /// </summary>
    public WordDetailPage(
        IWordDetailQueryService wordDetailQueryService,
        IUserLearningProfileService userLearningProfileService,
        IUserFavoriteWordService userFavoriteWordService,
        IUserWordStateService userWordStateService,
        ISpeechPlaybackService speechPlaybackService)
    {
        ArgumentNullException.ThrowIfNull(wordDetailQueryService);
        ArgumentNullException.ThrowIfNull(userLearningProfileService);
        ArgumentNullException.ThrowIfNull(userFavoriteWordService);
        ArgumentNullException.ThrowIfNull(userWordStateService);
        ArgumentNullException.ThrowIfNull(speechPlaybackService);

        InitializeComponent();

        _wordDetailQueryService = wordDetailQueryService;
        _userLearningProfileService = userLearningProfileService;
        _userFavoriteWordService = userFavoriteWordService;
        _userWordStateService = userWordStateService;
        _speechPlaybackService = speechPlaybackService;
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
    /// Refreshes the detail page whenever it becomes visible.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await RefreshAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Cancels any in-flight pronunciation request when the page is no longer visible.
    /// </summary>
    protected override void OnDisappearing()
    {
        CancelSpeechRequest();

        base.OnDisappearing();
    }

    /// <summary>
    /// Loads the selected lexical entry using the current profile language preferences.
    /// </summary>
    private async Task RefreshAsync()
    {
        Title = AppStrings.WordDetailTitle;
        TopicsSectionView.SectionTitle = AppStrings.WordDetailTopicsLabel;
        LearningStateSectionView.SectionTitle = AppStrings.WordDetailLearningStateLabel;
        UsageLabelsHeadingLabel.Text = AppStrings.WordDetailUsageLabelsLabel;
        ContextLabelsHeadingLabel.Text = AppStrings.WordDetailContextLabelsLabel;
        GrammarNotesHeadingLabel.Text = AppStrings.WordDetailGrammarNotesLabel;
        EmptyStateLabel.Text = AppStrings.WordDetailNotFound;
        SensesContainer.Children.Clear();
        UsageLabelsFlexLayout.Children.Clear();
        ContextLabelsFlexLayout.Children.Clear();
        GrammarNotesStackLayout.Children.Clear();
        UsageLabelsBorder.IsVisible = false;
        ContextLabelsBorder.IsVisible = false;
        GrammarNotesBorder.IsVisible = false;
        SpeakWordButton.IsVisible = false;
        FavoriteButton.IsVisible = false;
        KnownButton.IsVisible = false;
        DifficultButton.IsVisible = false;
        LearningStateSectionView.SectionValue = string.Empty;
        ClearAudioStatus();

        if (!Guid.TryParse(WordPublicId, out Guid publicId))
        {
            ShowNotFoundState();
            return;
        }

        UserLearningProfileModel profile = await _userLearningProfileService
            .GetCurrentProfileAsync(CancellationToken.None)
            .ConfigureAwait(true);

        WordDetailModel? word = await _wordDetailQueryService
            .GetWordDetailsAsync(
                publicId,
                profile.PreferredMeaningLanguage1,
                profile.PreferredMeaningLanguage2,
                profile.UiLanguageCode,
                CancellationToken.None)
            .ConfigureAwait(true);

        if (word is null)
        {
            ShowNotFoundState();
            return;
        }

        _isFavorite = await _userFavoriteWordService
            .IsFavoriteAsync(publicId, CancellationToken.None)
            .ConfigureAwait(true);
        _userWordState = await _userWordStateService
            .TrackWordViewedAsync(publicId, CancellationToken.None)
            .ConfigureAwait(true);

        Title = BuildHeadline(word);
        HeadlineLabel.Text = BuildHeadline(word);
        SpeakWordButton.Text = AppStrings.WordDetailSpeakWordButton;
        SpeakWordButton.IsVisible = true;
        MetadataLabel.Text = LexiconDisplayText.FormatMetadata(word.PartOfSpeech, word.CefrLevel);
        FavoriteButton.Text = _isFavorite
            ? AppStrings.WordDetailRemoveFavoriteButton
            : AppStrings.WordDetailAddFavoriteButton;
        FavoriteButton.IsVisible = true;
        ApplyUserWordState();
        ApplyWordLabels(UsageLabelsFlexLayout, UsageLabelsBorder, word.UsageLabels);
        ApplyWordLabels(ContextLabelsFlexLayout, ContextLabelsBorder, word.ContextLabels);
        ApplyGrammarNotes(word.GrammarNotes);
        TopicsSectionView.SectionValue = word.Topics.Count == 0
            ? AppStrings.WordDetailNoTopics
            : string.Join(", ", word.Topics);
        EmptyStateLabel.IsVisible = false;

        foreach (WordSenseDetailModel sense in word.Senses)
        {
            SensesContainer.Children.Add(BuildSenseView(sense));
        }
    }

    /// <summary>
    /// Shows the not-found state when the selected word is unavailable.
    /// </summary>
    private void ShowNotFoundState()
    {
        Title = AppStrings.WordDetailTitle;
        HeadlineLabel.Text = AppStrings.WordDetailTitle;
        SpeakWordButton.IsVisible = false;
        MetadataLabel.Text = string.Empty;
        FavoriteButton.IsVisible = false;
        KnownButton.IsVisible = false;
        DifficultButton.IsVisible = false;
        UsageLabelsFlexLayout.Children.Clear();
        ContextLabelsFlexLayout.Children.Clear();
        GrammarNotesStackLayout.Children.Clear();
        UsageLabelsBorder.IsVisible = false;
        ContextLabelsBorder.IsVisible = false;
        GrammarNotesBorder.IsVisible = false;
        LearningStateSectionView.SectionValue = string.Empty;
        TopicsSectionView.SectionValue = string.Empty;
        ClearAudioStatus();
        EmptyStateLabel.IsVisible = true;
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

        _isFavorite = await _userFavoriteWordService
            .ToggleFavoriteAsync(publicId, CancellationToken.None)
            .ConfigureAwait(true);

        FavoriteButton.Text = _isFavorite
            ? AppStrings.WordDetailRemoveFavoriteButton
            : AppStrings.WordDetailAddFavoriteButton;
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

        _userWordState = _userWordState?.IsKnown == true
            ? await _userWordStateService.ClearWordKnownStateAsync(publicId, CancellationToken.None).ConfigureAwait(true)
            : await _userWordStateService.MarkWordKnownAsync(publicId, CancellationToken.None).ConfigureAwait(true);

        ApplyUserWordState();
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

        _userWordState = _userWordState?.IsDifficult == true
            ? await _userWordStateService.ClearWordDifficultStateAsync(publicId, CancellationToken.None).ConfigureAwait(true)
            : await _userWordStateService.MarkWordDifficultAsync(publicId, CancellationToken.None).ConfigureAwait(true);

        ApplyUserWordState();
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

        await SpeakGermanTextAsync(headline).ConfigureAwait(true);
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
            Button speakExampleButton = BuildSpeechButton(AppStrings.WordDetailSpeakExampleButton, example.GermanText);

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
                ? Color.FromArgb("#111827")
                : Color.FromArgb("#F8FAFC"),
            Content = senseLayout,
        };
    }

    /// <summary>
    /// Creates a speech action button for a German text fragment.
    /// </summary>
    /// <param name="buttonText">The localized button text.</param>
    /// <param name="spokenText">The German text to pronounce.</param>
    /// <returns>A configured button instance.</returns>
    private Button BuildSpeechButton(string buttonText, string spokenText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buttonText);
        ArgumentException.ThrowIfNullOrWhiteSpace(spokenText);

        Button button = new()
        {
            Text = buttonText,
        };

        button.Clicked += async (_, _) => await SpeakGermanTextAsync(spokenText).ConfigureAwait(true);

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
    /// Renders the supplied lexical labels as wrapped chips.
    /// </summary>
    private static void ApplyWordLabels(FlexLayout container, Border hostBorder, IReadOnlyList<string> labelKeys)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(hostBorder);
        ArgumentNullException.ThrowIfNull(labelKeys);

        container.Children.Clear();
        hostBorder.IsVisible = labelKeys.Count > 0;

        foreach (string labelKey in labelKeys)
        {
            container.Children.Add(BuildWordLabelChip(labelKey));
        }
    }

    /// <summary>
    /// Creates one wrapped metadata chip for a lexical label key.
    /// </summary>
    private static View BuildWordLabelChip(string labelKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(labelKey);

        return new Border
        {
            Padding = new Thickness(12, 7),
            Margin = new Thickness(0, 0, 8, 8),
            StrokeThickness = 0,
            BackgroundColor = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#223047")
                : Color.FromArgb("#DCEAFE"),
            Content = new Label
            {
                Text = LexiconTagDisplayText.GetDisplayName(labelKey),
                TextColor = Application.Current?.RequestedTheme == AppTheme.Dark
                    ? Color.FromArgb("#F8FAFC")
                    : Color.FromArgb("#1E3A8A"),
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
            },
        };
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
                    ? Color.FromArgb("#1F2937")
                    : Color.FromArgb("#F8FAFC"),
                Content = new Label
                {
                    Text = grammarNote,
                    Style = ResolveAppTextStyle("Body"),
                },
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

        List<string> statusParts = [$"{string.Format(AppStrings.WordDetailViewCountFormat, _userWordState.ViewCount)}"];

        if (_userWordState.IsKnown)
        {
            statusParts.Add(AppStrings.WordDetailStateKnown);
        }

        if (_userWordState.IsDifficult)
        {
            statusParts.Add(AppStrings.WordDetailStateDifficult);
        }

        LearningStateSectionView.SectionValue = string.Join(Environment.NewLine, statusParts);

        KnownButton.Text = _userWordState.IsKnown
            ? AppStrings.WordDetailClearKnownButton
            : AppStrings.WordDetailMarkKnownButton;
        KnownButton.IsVisible = true;
        KnownButton.IsEnabled = true;

        DifficultButton.Text = _userWordState.IsDifficult
            ? AppStrings.WordDetailClearDifficultButton
            : AppStrings.WordDetailMarkDifficultButton;
        DifficultButton.IsVisible = true;
        DifficultButton.IsEnabled = true;
    }

    /// <summary>
    /// Attempts to pronounce German content and surfaces any non-success outcome as a localized UI message.
    /// </summary>
    /// <param name="text">The German text to pronounce.</param>
    private async Task SpeakGermanTextAsync(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        ClearAudioStatus();
        ResetSpeechRequest();

        SpeechPlaybackResult result = await _speechPlaybackService
            .SpeakAsync(text, "de", _speechCancellationTokenSource!.Token)
            .ConfigureAwait(true);

        if (result.IsSuccess || result.Status == SpeechPlaybackStatus.Cancelled)
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
}

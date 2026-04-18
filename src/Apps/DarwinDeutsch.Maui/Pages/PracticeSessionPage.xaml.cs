using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Diagnostics;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Practice.Application.Abstractions;
using DarwinLingua.Practice.Application.Models;
using DarwinLingua.Practice.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Runs one learner-facing practice session for either flashcards or quiz recall.
/// </summary>
[QueryProperty(nameof(Mode), "mode")]
public partial class PracticeSessionPage : ContentPage
{
    private const int SessionItemCount = 10;

    private readonly IAppLocalizationService _appLocalizationService;
    private readonly IActiveLearningProfileCacheService _activeLearningProfileCacheService;
    private readonly IPracticeReviewSessionService _practiceReviewSessionService;
    private readonly IPracticeFlashcardAnswerService _practiceFlashcardAnswerService;
    private readonly IPracticeQuizAnswerService _practiceQuizAnswerService;
    private readonly IPerformanceTelemetryService _performanceTelemetryService;
    private readonly ILogger<PracticeSessionPage> _logger;
    private readonly List<PracticeAttemptOutcome> _submittedOutcomes = [];
    private IReadOnlyList<PracticeReviewSessionItemModel> _sessionItems = [];
    private string _mode = "flashcard";
    private int _currentIndex;
    private bool _isAnswerRevealed;
    private bool _isSubmittingOutcome;
    private bool _isSessionLoaded;
    private DateTime _currentItemShownAtUtc;
    private CancellationTokenSource? _sessionCancellationTokenSource;

    public PracticeSessionPage(
        IAppLocalizationService appLocalizationService,
        IActiveLearningProfileCacheService activeLearningProfileCacheService,
        IPracticeReviewSessionService practiceReviewSessionService,
        IPracticeFlashcardAnswerService practiceFlashcardAnswerService,
        IPracticeQuizAnswerService practiceQuizAnswerService,
        IPerformanceTelemetryService performanceTelemetryService,
        ILogger<PracticeSessionPage> logger)
    {
        ArgumentNullException.ThrowIfNull(appLocalizationService);
        ArgumentNullException.ThrowIfNull(activeLearningProfileCacheService);
        ArgumentNullException.ThrowIfNull(practiceReviewSessionService);
        ArgumentNullException.ThrowIfNull(practiceFlashcardAnswerService);
        ArgumentNullException.ThrowIfNull(practiceQuizAnswerService);
        ArgumentNullException.ThrowIfNull(performanceTelemetryService);
        ArgumentNullException.ThrowIfNull(logger);

        InitializeComponent();

        _appLocalizationService = appLocalizationService;
        _activeLearningProfileCacheService = activeLearningProfileCacheService;
        _practiceReviewSessionService = practiceReviewSessionService;
        _practiceFlashcardAnswerService = practiceFlashcardAnswerService;
        _practiceQuizAnswerService = practiceQuizAnswerService;
        _performanceTelemetryService = performanceTelemetryService;
        _logger = logger;

        _appLocalizationService.CultureChanged += OnCultureChanged;

        ApplyLocalizedText();
    }

    /// <summary>
    /// Gets or sets the routed session mode.
    /// </summary>
    public string Mode
    {
        get => _mode;
        set
        {
            _mode = Uri.UnescapeDataString(value ?? string.Empty);
            _isSessionLoaded = false;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        ApplyLocalizedText();

        if (_isSessionLoaded)
        {
            RenderCurrentState();
            return;
        }

        try
        {
            await LoadSessionAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            return;
        }
    }

    protected override void OnDisappearing()
    {
        CancelSessionRequest();

        base.OnDisappearing();
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        if (args.NewHandler is null)
        {
            _appLocalizationService.CultureChanged -= OnCultureChanged;
        }

        base.OnHandlerChanging(args);
    }

    private bool IsQuizMode =>
        string.Equals(_mode, "quiz", StringComparison.OrdinalIgnoreCase);

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        ApplyLocalizedText();
        RenderCurrentState();
    }

    private void ApplyLocalizedText()
    {
        SessionHeadlineLabel.Text = IsQuizMode
            ? AppStrings.PracticeQuizPageHeadline
            : AppStrings.PracticeFlashcardPageHeadline;
        SessionDescriptionLabel.Text = IsQuizMode
            ? AppStrings.PracticeQuizPageDescription
            : AppStrings.PracticeFlashcardPageDescription;
        RevealButton.Text = AppStrings.PracticeSessionRevealButton;
        IncorrectButton.Text = AppStrings.PracticePageOutcomeIncorrect;
        HardButton.Text = AppStrings.PracticePageOutcomeHard;
        CorrectButton.Text = AppStrings.PracticePageOutcomeCorrect;
        EasyButton.Text = AppStrings.PracticePageOutcomeEasy;
        NextButton.Text = AppStrings.PracticeSessionNextButton;
        FinishButton.Text = AppStrings.PracticeSessionFinishButton;
        FeedbackHeadlineLabel.Text = AppStrings.PracticeSessionFeedbackHeadline;
        SummaryHeadlineLabel.Text = AppStrings.PracticeSessionSummaryHeadline;
        ReturnToPracticeButton.Text = AppStrings.PracticeSessionReturnButton;
        Title = IsQuizMode
            ? AppStrings.PracticeQuizPageTitle
            : AppStrings.PracticeFlashcardPageTitle;
    }

    private async Task LoadSessionAsync()
    {
        ResetSessionRequest();
        CancellationToken cancellationToken = _sessionCancellationTokenSource!.Token;
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            SetLoadingState();

            UserLearningProfileModel profile = await _activeLearningProfileCacheService
                .GetCurrentProfileAsync(cancellationToken)
                .ConfigureAwait(true);
            PracticeReviewSessionModel session = await _practiceReviewSessionService
                .StartAsync(profile.PreferredMeaningLanguage1, SessionItemCount, cancellationToken)
                .ConfigureAwait(true);

            _sessionItems = session.Items;
            _submittedOutcomes.Clear();
            _currentIndex = 0;
            _isAnswerRevealed = false;
            _isSubmittingOutcome = false;
            _isSessionLoaded = true;
            _currentItemShownAtUtc = DateTime.UtcNow;

            RenderCurrentState();
            _logger.LogInformation(
                "Practice session '{Mode}' loaded {ItemCount} items in {ElapsedMs} ms.",
                IsQuizMode ? "quiz" : "flashcard",
                _sessionItems.Count,
                stopwatch.ElapsedMilliseconds);
            _performanceTelemetryService.Record("practice.session-load", stopwatch.Elapsed, PerformanceTelemetryOutcome.Success, _sessionItems.Count);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Practice session load cancelled after {ElapsedMs} ms.", stopwatch.ElapsedMilliseconds);
            _performanceTelemetryService.Record("practice.session-load", stopwatch.Elapsed, PerformanceTelemetryOutcome.Cancelled);
            throw;
        }
        catch
        {
            _logger.LogWarning("Practice session load failed after {ElapsedMs} ms.", stopwatch.ElapsedMilliseconds);
            _performanceTelemetryService.Record("practice.session-load", stopwatch.Elapsed, PerformanceTelemetryOutcome.Failed);
            SessionStateLabel.Text = AppStrings.CommonStateError;
            SessionProgressLabel.Text = string.Empty;
            PromptCardBorder.IsVisible = false;
            PrimaryActionsLayout.IsVisible = false;
            FeedbackBorder.IsVisible = false;
            SummaryBorder.IsVisible = false;
        }
    }

    private void SetLoadingState()
    {
        SessionProgressLabel.Text = string.Empty;
        SessionProgressBar.Progress = 0d;
        SessionStateLabel.Text = AppStrings.CommonStateLoading;
        PromptCardBorder.IsVisible = false;
        PrimaryActionsLayout.IsVisible = false;
        FeedbackBorder.IsVisible = false;
        SummaryBorder.IsVisible = false;
        ApplyActionButtonState();
    }

    private void RenderCurrentState()
    {
        if (_sessionItems.Count == 0)
        {
            SessionProgressLabel.Text = string.Empty;
            SessionProgressBar.Progress = 0d;
            SessionStateLabel.Text = AppStrings.PracticeSessionEmpty;
            PromptCardBorder.IsVisible = false;
            PrimaryActionsLayout.IsVisible = false;
            FeedbackBorder.IsVisible = false;
            SummaryBorder.IsVisible = false;
            ApplyActionButtonState();
            return;
        }

        if (_currentIndex >= _sessionItems.Count)
        {
            RenderSummaryState();
            return;
        }

        PracticeReviewSessionItemModel currentItem = _sessionItems[_currentIndex];

        SessionProgressLabel.Text = string.Format(
            AppStrings.PracticeSessionProgressFormat,
            _currentIndex + 1,
            _sessionItems.Count);
        SessionProgressBar.Progress = CalculateSessionProgress();
        SessionStateLabel.Text = IsQuizMode
            ? AppStrings.PracticeQuizPromptHint
            : AppStrings.PracticeFlashcardPromptHint;
        PromptCardBorder.IsVisible = true;
        PrimaryActionsLayout.IsVisible = true;

        PromptTitleLabel.Text = IsQuizMode
            ? AppStrings.PracticeQuizPromptLabel
            : AppStrings.PracticeFlashcardPromptLabel;
        PromptValueLabel.Text = IsQuizMode
            ? currentItem.PrimaryMeaning ?? AppStrings.TopicWordsPageMeaningUnavailable
            : currentItem.Lemma;
        PromptMetadataLabel.Text = BuildItemMetadata(currentItem);

        AnswerTitleLabel.Text = IsQuizMode
            ? AppStrings.PracticeQuizAnswerLabel
            : AppStrings.PracticeFlashcardAnswerLabel;
        AnswerValueLabel.Text = IsQuizMode
            ? currentItem.Lemma
            : currentItem.PrimaryMeaning ?? AppStrings.TopicWordsPageMeaningUnavailable;
        AnswerTitleLabel.IsVisible = _isAnswerRevealed;
        AnswerValueLabel.IsVisible = _isAnswerRevealed;
        AnswerHintLabel.Text = _isAnswerRevealed
            ? AppStrings.PracticeSessionAnswerRevealedHint
            : AppStrings.PracticeSessionAnswerHiddenHint;

        RevealButton.IsVisible = !_isAnswerRevealed;
        OutcomeButtonsGrid.IsVisible = _isAnswerRevealed && !FeedbackBorder.IsVisible;

        if (!FeedbackBorder.IsVisible)
        {
            NextButton.IsVisible = false;
            FinishButton.IsVisible = false;
        }

        ApplyActionButtonState();
    }

    private void RenderSummaryState()
    {
        int incorrectCount = _submittedOutcomes.Count(static outcome => outcome == PracticeAttemptOutcome.Incorrect);
        int hardCount = _submittedOutcomes.Count(static outcome => outcome == PracticeAttemptOutcome.Hard);
        int correctCount = _submittedOutcomes.Count(static outcome => outcome == PracticeAttemptOutcome.Correct);
        int easyCount = _submittedOutcomes.Count(static outcome => outcome == PracticeAttemptOutcome.Easy);

        SessionProgressLabel.Text = AppStrings.PracticeSessionSummaryState;
        SessionProgressBar.Progress = 1d;
        SessionStateLabel.Text = IsQuizMode
            ? AppStrings.PracticeQuizSessionComplete
            : AppStrings.PracticeFlashcardSessionComplete;
        PromptCardBorder.IsVisible = false;
        PrimaryActionsLayout.IsVisible = false;
        FeedbackBorder.IsVisible = false;
        SummaryBorder.IsVisible = true;
        SummaryBodyLabel.Text = string.Format(
            AppStrings.PracticeSessionSummaryBodyFormat,
            _submittedOutcomes.Count,
            correctCount + easyCount,
            hardCount,
            incorrectCount);
        ApplyActionButtonState();
    }

    private async Task SubmitOutcomeAsync(PracticeAttemptOutcome outcome)
    {
        if (_currentIndex >= _sessionItems.Count || _isSubmittingOutcome)
        {
            return;
        }

        _isSubmittingOutcome = true;
        ApplyActionButtonState();
        Stopwatch stopwatch = Stopwatch.StartNew();

        PracticeReviewSessionItemModel currentItem = _sessionItems[_currentIndex];
        int responseMilliseconds = (int)Math.Max(
            1,
            DateTime.UtcNow.Subtract(_currentItemShownAtUtc).TotalMilliseconds);

        try
        {
            PracticeAnswerResultViewModel result = IsQuizMode
                ? await SubmitQuizOutcomeAsync(currentItem, outcome, responseMilliseconds).ConfigureAwait(true)
                : await SubmitFlashcardOutcomeAsync(currentItem, outcome, responseMilliseconds).ConfigureAwait(true);

            _submittedOutcomes.Add(outcome);

            FeedbackBodyLabel.Text = string.Format(
                AppStrings.PracticeSessionFeedbackBodyFormat,
                GetOutcomeLabel(outcome),
                FormatDueAt(result.DueAtUtcAfterAttempt),
                result.TotalAttemptCount,
                result.ConsecutiveSuccessCount,
                result.ConsecutiveFailureCount);
            FeedbackBorder.IsVisible = true;
            OutcomeButtonsGrid.IsVisible = false;
            RevealButton.IsVisible = false;
            NextButton.IsVisible = _currentIndex < _sessionItems.Count - 1;
            FinishButton.IsVisible = _currentIndex >= _sessionItems.Count - 1;
            ApplyActionButtonState();
            _logger.LogInformation(
                "Practice session '{Mode}' submitted outcome '{Outcome}' for item {ItemIndex}/{ItemCount} in {ElapsedMs} ms.",
                IsQuizMode ? "quiz" : "flashcard",
                outcome,
                _currentIndex + 1,
                _sessionItems.Count,
                stopwatch.ElapsedMilliseconds);
            _performanceTelemetryService.Record("practice.session-submit", stopwatch.Elapsed, PerformanceTelemetryOutcome.Success, _currentIndex + 1);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Practice session outcome submission cancelled after {ElapsedMs} ms.", stopwatch.ElapsedMilliseconds);
            _performanceTelemetryService.Record("practice.session-submit", stopwatch.Elapsed, PerformanceTelemetryOutcome.Cancelled);
            return;
        }
        catch
        {
            _logger.LogWarning("Practice session outcome submission failed after {ElapsedMs} ms.", stopwatch.ElapsedMilliseconds);
            _performanceTelemetryService.Record("practice.session-submit", stopwatch.Elapsed, PerformanceTelemetryOutcome.Failed);
            FeedbackBodyLabel.Text = AppStrings.CommonStateError;
            FeedbackBorder.IsVisible = true;
            OutcomeButtonsGrid.IsVisible = true;
            RevealButton.IsVisible = false;
            NextButton.IsVisible = false;
            FinishButton.IsVisible = false;
            ApplyActionButtonState();
        }
        finally
        {
            _isSubmittingOutcome = false;
            ApplyActionButtonState();
        }
    }

    private async Task<PracticeAnswerResultViewModel> SubmitFlashcardOutcomeAsync(
        PracticeReviewSessionItemModel currentItem,
        PracticeAttemptOutcome outcome,
        int responseMilliseconds)
    {
        PracticeFlashcardAnswerResultModel result = await _practiceFlashcardAnswerService
            .SubmitAsync(
                new PracticeFlashcardAnswerRequestModel(currentItem.WordEntryPublicId, outcome, responseMilliseconds),
                _sessionCancellationTokenSource?.Token ?? CancellationToken.None)
            .ConfigureAwait(true);

        return new PracticeAnswerResultViewModel(
            result.DueAtUtcAfterAttempt,
            result.TotalAttemptCount,
            result.ConsecutiveSuccessCount,
            result.ConsecutiveFailureCount);
    }

    private async Task<PracticeAnswerResultViewModel> SubmitQuizOutcomeAsync(
        PracticeReviewSessionItemModel currentItem,
        PracticeAttemptOutcome outcome,
        int responseMilliseconds)
    {
        PracticeQuizAnswerResultModel result = await _practiceQuizAnswerService
            .SubmitAsync(
                new PracticeQuizAnswerRequestModel(currentItem.WordEntryPublicId, outcome, responseMilliseconds),
                _sessionCancellationTokenSource?.Token ?? CancellationToken.None)
            .ConfigureAwait(true);

        return new PracticeAnswerResultViewModel(
            result.DueAtUtcAfterAttempt,
            result.TotalAttemptCount,
            result.ConsecutiveSuccessCount,
            result.ConsecutiveFailureCount);
    }

    private void AdvanceToNextItem()
    {
        _currentIndex++;
        _isAnswerRevealed = false;
        _currentItemShownAtUtc = DateTime.UtcNow;
        FeedbackBorder.IsVisible = false;
        NextButton.IsVisible = false;
        FinishButton.IsVisible = false;
        RenderCurrentState();
    }

    private void ApplyActionButtonState()
    {
        bool isBusy = _isSubmittingOutcome;

        RevealButton.IsEnabled = !isBusy && RevealButton.IsVisible;
        IncorrectButton.IsEnabled = !isBusy && OutcomeButtonsGrid.IsVisible;
        HardButton.IsEnabled = !isBusy && OutcomeButtonsGrid.IsVisible;
        CorrectButton.IsEnabled = !isBusy && OutcomeButtonsGrid.IsVisible;
        EasyButton.IsEnabled = !isBusy && OutcomeButtonsGrid.IsVisible;
        NextButton.IsEnabled = !isBusy && NextButton.IsVisible;
        FinishButton.IsEnabled = !isBusy && FinishButton.IsVisible;
        ReturnToPracticeButton.IsEnabled = !isBusy && SummaryBorder.IsVisible;
    }

    private static string BuildItemMetadata(PracticeReviewSessionItemModel item)
    {
        string dueLabel = item.IsDueNow
            ? AppStrings.PracticePageDueBadge
            : AppStrings.PracticePageQueuedBadge;
        string statusLabel = item.IsDifficult
            ? AppStrings.PracticePageDifficultBadge
            : item.IsKnown
                ? AppStrings.PracticePageKnownBadge
                : AppStrings.PracticePageLearningBadge;

        return string.Format(
            AppStrings.PracticeSessionMetadataFormat,
            item.CefrLevel,
            dueLabel,
            statusLabel,
            item.TotalAttemptCount);
    }

    private static string FormatDueAt(DateTime dueAtUtc)
    {
        DateTime localDueAt = dueAtUtc.ToLocalTime();
        return string.Format(
            AppStrings.PracticeSessionDueAtFormat,
            localDueAt.ToString("g"));
    }

    private static string GetOutcomeLabel(PracticeAttemptOutcome outcome)
    {
        return outcome switch
        {
            PracticeAttemptOutcome.Incorrect => AppStrings.PracticePageOutcomeIncorrect,
            PracticeAttemptOutcome.Hard => AppStrings.PracticePageOutcomeHard,
            PracticeAttemptOutcome.Correct => AppStrings.PracticePageOutcomeCorrect,
            PracticeAttemptOutcome.Easy => AppStrings.PracticePageOutcomeEasy,
            _ => outcome.ToString(),
        };
    }

    private double CalculateSessionProgress()
    {
        if (_sessionItems.Count == 0)
        {
            return 0d;
        }

        return Math.Clamp((double)(_currentIndex + 1) / _sessionItems.Count, 0d, 1d);
    }

    private void OnRevealButtonClicked(object? sender, EventArgs e)
    {
        _isAnswerRevealed = true;
        FeedbackBorder.IsVisible = false;
        RenderCurrentState();
    }

    private async void OnIncorrectButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            await SubmitOutcomeAsync(PracticeAttemptOutcome.Incorrect).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async void OnHardButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            await SubmitOutcomeAsync(PracticeAttemptOutcome.Hard).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async void OnCorrectButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            await SubmitOutcomeAsync(PracticeAttemptOutcome.Correct).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async void OnEasyButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            await SubmitOutcomeAsync(PracticeAttemptOutcome.Easy).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void OnNextButtonClicked(object? sender, EventArgs e)
    {
        AdvanceToNextItem();
    }

    private void OnFinishButtonClicked(object? sender, EventArgs e)
    {
        _currentIndex = _sessionItems.Count;
        RenderSummaryState();
    }

    private async void OnReturnToPracticeButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("//practice").ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void ResetSessionRequest()
    {
        CancelSessionRequest();
        _sessionCancellationTokenSource = new CancellationTokenSource();
    }

    private void CancelSessionRequest()
    {
        if (_sessionCancellationTokenSource is null)
        {
            return;
        }

        _sessionCancellationTokenSource.Cancel();
        _sessionCancellationTokenSource.Dispose();
        _sessionCancellationTokenSource = null;
    }

    private sealed record PracticeAnswerResultViewModel(
        DateTime DueAtUtcAfterAttempt,
        int TotalAttemptCount,
        int ConsecutiveSuccessCount,
        int ConsecutiveFailureCount);
}

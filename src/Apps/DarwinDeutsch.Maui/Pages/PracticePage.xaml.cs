using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Practice.Application.Abstractions;
using DarwinLingua.Practice.Application.Models;
using DarwinLingua.Practice.Domain.Entities;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays the learner's current practice overview, review-session preview, and recent activity.
/// </summary>
public partial class PracticePage : ContentPage
{
    private readonly IAppLocalizationService _appLocalizationService;
    private readonly IUserLearningProfileService _userLearningProfileService;
    private readonly IPracticeLearningProgressSnapshotService _practiceLearningProgressSnapshotService;
    private readonly IPracticeRecentActivityService _practiceRecentActivityService;
    private readonly IPracticeReviewSessionService _practiceReviewSessionService;
    private CancellationTokenSource? _refreshCancellationTokenSource;

    public PracticePage(
        IAppLocalizationService appLocalizationService,
        IUserLearningProfileService userLearningProfileService,
        IPracticeLearningProgressSnapshotService practiceLearningProgressSnapshotService,
        IPracticeRecentActivityService practiceRecentActivityService,
        IPracticeReviewSessionService practiceReviewSessionService)
    {
        ArgumentNullException.ThrowIfNull(appLocalizationService);
        ArgumentNullException.ThrowIfNull(userLearningProfileService);
        ArgumentNullException.ThrowIfNull(practiceLearningProgressSnapshotService);
        ArgumentNullException.ThrowIfNull(practiceRecentActivityService);
        ArgumentNullException.ThrowIfNull(practiceReviewSessionService);

        InitializeComponent();

        _appLocalizationService = appLocalizationService;
        _userLearningProfileService = userLearningProfileService;
        _practiceLearningProgressSnapshotService = practiceLearningProgressSnapshotService;
        _practiceRecentActivityService = practiceRecentActivityService;
        _practiceReviewSessionService = practiceReviewSessionService;

        _appLocalizationService.CultureChanged += OnCultureChanged;

        ApplyLocalizedText();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        ApplyLocalizedText();

        try
        {
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            return;
        }
    }

    protected override void OnDisappearing()
    {
        CancelRefreshRequest();

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

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        ApplyLocalizedText();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await RefreshAsync().ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        });
    }

    private void ApplyLocalizedText()
    {
        Title = AppStrings.PracticeTabTitle;
        StatusBadgeLabel.Text = AppStrings.PracticePageStatusBadge;
        HeadlineLabel.Text = AppStrings.PracticePageHeadline;
        DescriptionLabel.Text = AppStrings.PracticePageDescription;
        ProgressSectionLabel.Text = AppStrings.PracticePageProgressSectionLabel;
        ActionsSectionLabel.Text = AppStrings.PracticePageActionsSectionLabel;
        DueNowSectionView.SectionTitle = AppStrings.PracticePageDueNowLabel;
        SuccessRateSectionView.SectionTitle = AppStrings.PracticePageSuccessRateLabel;
        MasteredSectionView.SectionTitle = AppStrings.PracticePageMasteredLabel;
        StrugglingSectionView.SectionTitle = AppStrings.PracticePageStrugglingLabel;
        StartFlashcardsActionBlockView.Caption = AppStrings.PracticePageStartFlashcardsLabel;
        StartFlashcardsActionBlockView.ButtonText = AppStrings.PracticePageStartFlashcardsButton;
        StartQuizActionBlockView.Caption = AppStrings.PracticePageStartQuizLabel;
        StartQuizActionBlockView.ButtonText = AppStrings.PracticePageStartQuizButton;
        RefreshPracticeActionBlockView.Caption = AppStrings.PracticePageRefreshLabel;
        RefreshPracticeActionBlockView.ButtonText = AppStrings.PracticePageRefreshButton;
        ReviewSessionSectionLabel.Text = AppStrings.PracticePageReviewSessionHeading;
        RecentActivitySectionLabel.Text = AppStrings.PracticePageRecentActivityHeading;
    }

    private async Task RefreshAsync()
    {
        ResetRefreshRequest();
        CancellationToken cancellationToken = _refreshCancellationTokenSource!.Token;

        try
        {
            Task<UserLearningProfileModel> profileTask = _userLearningProfileService
                .GetCurrentProfileAsync(cancellationToken);
            Task<PracticeLearningProgressSnapshotModel> snapshotTask = _practiceLearningProgressSnapshotService
                .GetSnapshotAsync(cancellationToken);

            await Task.WhenAll(profileTask, snapshotTask).ConfigureAwait(true);

            UserLearningProfileModel profile = profileTask.Result;
            PracticeLearningProgressSnapshotModel snapshot = snapshotTask.Result;

            Task<PracticeRecentActivityModel> recentActivityTask = _practiceRecentActivityService
                .GetRecentActivityAsync(profile.PreferredMeaningLanguage1, 6, cancellationToken);
            Task<PracticeReviewSessionModel> reviewPreviewTask = _practiceReviewSessionService
                .StartAsync(profile.PreferredMeaningLanguage1, 5, cancellationToken);

            await Task.WhenAll(recentActivityTask, reviewPreviewTask).ConfigureAwait(true);

            ApplySnapshot(snapshot);
            ApplyRecentActivity(recentActivityTask.Result);
            ApplyReviewPreview(reviewPreviewTask.Result);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            DueNowSectionView.SectionValue = AppStrings.CommonStateError;
            SuccessRateSectionView.SectionValue = AppStrings.CommonStateError;
            MasteredSectionView.SectionValue = AppStrings.CommonStateError;
            StrugglingSectionView.SectionValue = AppStrings.CommonStateError;
            ReviewSessionStateLabel.Text = AppStrings.CommonStateError;
            ReviewSessionStateLabel.IsVisible = true;
            ReviewSessionCollectionView.ItemsSource = Array.Empty<PracticeWordItemViewModel>();
            ReviewSessionCollectionView.IsVisible = false;
            RecentActivityStateLabel.Text = AppStrings.CommonStateError;
            RecentActivityStateLabel.IsVisible = true;
            RecentActivityCollectionView.ItemsSource = Array.Empty<PracticeWordItemViewModel>();
            RecentActivityCollectionView.IsVisible = false;
        }
    }

    private void ApplySnapshot(PracticeLearningProgressSnapshotModel snapshot)
    {
        DueNowSectionView.SectionValue = string.Format(
            AppStrings.PracticePageDueNowValueFormat,
            snapshot.DueNowCount,
            snapshot.ScheduledWordCount);
        SuccessRateSectionView.SectionValue = string.Format(
            AppStrings.PracticePageSuccessRateValueFormat,
            snapshot.SuccessRate.ToString("P0"),
            snapshot.TotalAttemptCount);
        MasteredSectionView.SectionValue = string.Format(
            AppStrings.PracticePageMasteredValueFormat,
            snapshot.MasteredWordCount,
            snapshot.AttemptedWordCount);
        StrugglingSectionView.SectionValue = string.Format(
            AppStrings.PracticePageStrugglingValueFormat,
            snapshot.StrugglingWordCount,
            snapshot.TrackedWordCount);
    }

    private void ApplyRecentActivity(PracticeRecentActivityModel recentActivity)
    {
        PracticeWordItemViewModel[] items = recentActivity.Items
            .Select(item => new PracticeWordItemViewModel(
                item.WordEntryPublicId,
                item.Lemma,
                item.PrimaryMeaning ?? AppStrings.TopicWordsPageMeaningUnavailable,
                BuildRecentActivityMetadata(item)))
            .ToArray();

        RecentActivityCollectionView.ItemsSource = items;
        RecentActivityCollectionView.IsVisible = items.Length > 0;
        RecentActivityStateLabel.Text = items.Length == 0
            ? AppStrings.PracticePageRecentActivityEmpty
            : string.Format(AppStrings.PracticePageRecentActivitySummaryFormat, recentActivity.TotalAttempts);
        RecentActivityStateLabel.IsVisible = true;
    }

    private void ApplyReviewPreview(PracticeReviewSessionModel session)
    {
        PracticeWordItemViewModel[] items = session.Items
            .Select(item => new PracticeWordItemViewModel(
                item.WordEntryPublicId,
                item.Lemma,
                item.PrimaryMeaning ?? AppStrings.TopicWordsPageMeaningUnavailable,
                BuildReviewSessionMetadata(item)))
            .ToArray();

        ReviewSessionCollectionView.ItemsSource = items;
        ReviewSessionCollectionView.IsVisible = items.Length > 0;
        ReviewSessionStateLabel.Text = items.Length == 0
            ? AppStrings.PracticePageReviewSessionEmpty
            : string.Format(AppStrings.PracticePageReviewSessionSummaryFormat, session.TotalCandidates);
        ReviewSessionStateLabel.IsVisible = true;
    }

    private async void OnStartFlashcardsActionInvoked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(PracticeSessionPage)}?mode=flashcard")
            .ConfigureAwait(true);
    }

    private async void OnStartQuizActionInvoked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(PracticeSessionPage)}?mode=quiz")
            .ConfigureAwait(true);
    }

    private async void OnRefreshPracticeActionInvoked(object? sender, EventArgs e)
    {
        try
        {
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            return;
        }
    }

    private async void OnReviewSessionSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not PracticeWordItemViewModel selectedWord)
        {
            return;
        }

        ReviewSessionCollectionView.SelectedItem = null;
        string wordPublicId = Uri.EscapeDataString(selectedWord.PublicId.ToString());
        await Shell.Current.GoToAsync($"{nameof(WordDetailPage)}?wordPublicId={wordPublicId}")
            .ConfigureAwait(true);
    }

    private async void OnRecentActivitySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not PracticeWordItemViewModel selectedWord)
        {
            return;
        }

        RecentActivityCollectionView.SelectedItem = null;
        string wordPublicId = Uri.EscapeDataString(selectedWord.PublicId.ToString());
        await Shell.Current.GoToAsync($"{nameof(WordDetailPage)}?wordPublicId={wordPublicId}")
            .ConfigureAwait(true);
    }

    private static string BuildReviewSessionMetadata(PracticeReviewSessionItemModel item)
    {
        string dueLabel = item.IsDueNow
            ? AppStrings.PracticePageDueBadge
            : AppStrings.PracticePageQueuedBadge;
        string statusLabel = item.IsDifficult
            ? AppStrings.PracticePageDifficultBadge
            : item.IsKnown
                ? AppStrings.PracticePageKnownBadge
                : AppStrings.PracticePageLearningBadge;

        return $"{item.CefrLevel} · {dueLabel} · {statusLabel}";
    }

    private static string BuildRecentActivityMetadata(PracticeRecentActivityItemModel item)
    {
        string sessionLabel = item.SessionType == PracticeSessionType.Quiz
            ? AppStrings.PracticePageSessionQuiz
            : AppStrings.PracticePageSessionFlashcard;
        string outcomeLabel = item.Outcome switch
        {
            PracticeAttemptOutcome.Incorrect => AppStrings.PracticePageOutcomeIncorrect,
            PracticeAttemptOutcome.Hard => AppStrings.PracticePageOutcomeHard,
            PracticeAttemptOutcome.Correct => AppStrings.PracticePageOutcomeCorrect,
            PracticeAttemptOutcome.Easy => AppStrings.PracticePageOutcomeEasy,
            _ => item.Outcome.ToString(),
        };

        return $"{sessionLabel} · {outcomeLabel} · {item.CefrLevel}";
    }

    private sealed record PracticeWordItemViewModel(Guid PublicId, string Lemma, string PrimaryMeaning, string MetadataLine);

    private void ResetRefreshRequest()
    {
        CancelRefreshRequest();
        _refreshCancellationTokenSource = new CancellationTokenSource();
        ReviewSessionStateLabel.Text = AppStrings.CommonStateLoading;
        ReviewSessionStateLabel.IsVisible = true;
        ReviewSessionCollectionView.IsVisible = false;
        RecentActivityStateLabel.Text = AppStrings.CommonStateLoading;
        RecentActivityStateLabel.IsVisible = true;
        RecentActivityCollectionView.IsVisible = false;
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

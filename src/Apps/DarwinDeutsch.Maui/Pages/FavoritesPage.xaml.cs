using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Auth;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays the current user's favorited lexical entries.
/// </summary>
public partial class FavoritesPage : ContentPage
{
    private readonly IMobileEntitledFeatureAccessService _featureAccessService;
    private readonly IUserFavoriteWordService _userFavoriteWordService;
    private readonly IUserLearningProfileService _userLearningProfileService;
    private CancellationTokenSource? _refreshCancellationTokenSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="FavoritesPage"/> class.
    /// </summary>
    public FavoritesPage(
        IMobileEntitledFeatureAccessService featureAccessService,
        IUserFavoriteWordService userFavoriteWordService,
        IUserLearningProfileService userLearningProfileService)
    {
        ArgumentNullException.ThrowIfNull(featureAccessService);
        ArgumentNullException.ThrowIfNull(userFavoriteWordService);
        ArgumentNullException.ThrowIfNull(userLearningProfileService);

        InitializeComponent();

        _featureAccessService = featureAccessService;
        _userFavoriteWordService = userFavoriteWordService;
        _userLearningProfileService = userLearningProfileService;
    }

    /// <summary>
    /// Refreshes the page when it becomes visible.
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
        }
    }

    protected override void OnDisappearing()
    {
        CancelRefreshRequest();
        base.OnDisappearing();
    }

    /// <summary>
    /// Loads the current favorites using the active meaning-language preference.
    /// </summary>
    private async Task RefreshAsync()
    {
        ResetRefreshRequest();
        CancellationToken cancellationToken = _refreshCancellationTokenSource!.Token;

        Title = AppStrings.FavoritesPageTitle;
        HeadlineLabel.Text = AppStrings.FavoritesPageHeadline;
        DescriptionLabel.Text = AppStrings.FavoritesPageDescription;
        EmptyStateLabel.Text = AppStrings.FavoritesPageEmpty;
        LoadingStateView.Message = AppStrings.CommonStateLoading;
        ErrorStateLabel.Text = AppStrings.CommonStateError;

        ShowLoadingState();

        try
        {
            if (!await _featureAccessService.CanUseFavoritesAsync(cancellationToken).ConfigureAwait(true))
            {
                ShowFavoritesLockedState();
                return;
            }

            UserLearningProfileModel profile = await _userLearningProfileService
                .GetCurrentProfileAsync(cancellationToken)
                .ConfigureAwait(true);

            IReadOnlyList<FavoriteWordListItemModel> favoriteWords = await _userFavoriteWordService
                .GetFavoriteWordsAsync(profile.PreferredMeaningLanguage1, cancellationToken)
                .ConfigureAwait(true);

            FavoritesCollectionView.ItemsSource = favoriteWords
                .Select(word => new FavoriteWordItemViewModel(
                    word.PublicId,
                    BuildLemmaLine(word),
                    word.PrimaryMeaning ?? AppStrings.TopicWordsPageMeaningUnavailable,
                    LexiconDisplayText.FormatMetadata(word.PartOfSpeech, word.CefrLevel)))
                .ToArray();

            EmptyStateLabel.IsVisible = favoriteWords.Count == 0;
            FavoritesCollectionView.IsVisible = favoriteWords.Count > 0;
            ErrorStateLabel.IsVisible = false;
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch
        {
            FavoritesCollectionView.ItemsSource = Array.Empty<FavoriteWordItemViewModel>();
            FavoritesCollectionView.IsVisible = false;
            EmptyStateLabel.IsVisible = false;
            ErrorStateLabel.IsVisible = true;
        }
        finally
        {
            LoadingStateView.IsLoading = false;
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

    /// <summary>
    /// Shows the loading state while favorites are being loaded.
    /// </summary>
    private void ShowLoadingState()
    {
        LoadingStateView.IsLoading = true;
        ErrorStateLabel.IsVisible = false;
        EmptyStateLabel.IsVisible = false;
        FavoritesCollectionView.IsVisible = false;
    }

    private void ShowFavoritesLockedState()
    {
        LoadingStateView.IsLoading = false;
        ErrorStateLabel.IsVisible = false;
        EmptyStateLabel.IsVisible = true;
        EmptyStateLabel.Text = AppStrings.FavoritesLockedMessage;
        FavoritesCollectionView.IsVisible = false;
    }

    /// <summary>
    /// Navigates to the selected lexical-entry detail page.
    /// </summary>
    private async void OnFavoritesSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not FavoriteWordItemViewModel selectedWord)
        {
            return;
        }

        FavoritesCollectionView.SelectedItem = null;

        string wordPublicId = Uri.EscapeDataString(selectedWord.PublicId.ToString());
        try
        {
            await Shell.Current.GoToAsync($"{nameof(WordDetailPage)}?wordPublicId={wordPublicId}")
                .ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// Builds the browse headline shown for a lexical entry.
    /// </summary>
    private static string BuildLemmaLine(FavoriteWordListItemModel word)
    {
        ArgumentNullException.ThrowIfNull(word);

        return string.IsNullOrWhiteSpace(word.Article)
            ? word.Lemma
            : $"{word.Article} {word.Lemma}";
    }

    /// <summary>
    /// Represents the UI model used by the favorites collection view.
    /// </summary>
    private sealed record FavoriteWordItemViewModel(Guid PublicId, string Lemma, string PrimaryMeaning, string MetadataLine);
}

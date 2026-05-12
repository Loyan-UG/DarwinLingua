using DarwinDeutsch.Maui.Resources.Strings;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using System.Collections.ObjectModel;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays mobile list surfaces for Learning Portal modules backed by local package content.
/// </summary>
[QueryProperty(nameof(Module), "module")]
public partial class LearningPortalListPage : ContentPage
{
    private readonly IGrammarTopicQueryService _grammarTopics;
    private readonly IExpressionQueryService _expressions;
    private readonly IExerciseQueryService _exercises;
    private readonly ICourseQueryService _courses;
    private readonly IWritingTemplateQueryService _writingTemplates;
    private readonly ICulturalNoteQueryService _culturalNotes;
    private readonly IExamPrepQueryService _examPrep;
    private readonly ITalkTopicQueryService _talkTopics;
    private readonly ObservableCollection<LearningPortalListItemViewModel> _items = [];
    private CancellationTokenSource? _refreshCancellationTokenSource;

    public LearningPortalListPage(
        IGrammarTopicQueryService grammarTopics,
        IExpressionQueryService expressions,
        IExerciseQueryService exercises,
        ICourseQueryService courses,
        IWritingTemplateQueryService writingTemplates,
        ICulturalNoteQueryService culturalNotes,
        IExamPrepQueryService examPrep,
        ITalkTopicQueryService talkTopics)
    {
        InitializeComponent();
        _grammarTopics = grammarTopics;
        _expressions = expressions;
        _exercises = exercises;
        _courses = courses;
        _writingTemplates = writingTemplates;
        _culturalNotes = culturalNotes;
        _examPrep = examPrep;
        _talkTopics = talkTopics;
        ItemsCollectionView.ItemsSource = _items;
    }

    public string Module { get; set; } = string.Empty;

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
        LearningPortalModuleDescriptor descriptor = LearningPortalModuleDescriptor.Resolve(Module);

        Title = descriptor.Title;
        HeadlineLabel.Text = descriptor.Title;
        DescriptionLabel.Text = descriptor.Description;
        EmptyStateLabel.Text = AppStrings.LearningPortalEmptyState;
        LoadingStateView.Message = AppStrings.CommonStateLoading;
        LoadingStateView.IsLoading = true;

        IReadOnlyList<LearningPortalListItemViewModel> items = await LoadItemsAsync(descriptor.Key, cancellationToken).ConfigureAwait(true);

        _items.Clear();
        foreach (LearningPortalListItemViewModel item in items)
        {
            _items.Add(item);
        }

        LoadingStateView.IsLoading = false;
        EmptyStateLabel.IsVisible = _items.Count == 0;
        ItemsCollectionView.IsVisible = _items.Count > 0;
    }

    private async Task<IReadOnlyList<LearningPortalListItemViewModel>> LoadItemsAsync(string module, CancellationToken cancellationToken)
    {
        switch (module)
        {
            case "grammar":
                return (await _grammarTopics.GetPublishedGrammarTopicsAsync(new GrammarTopicListFilterModel(null, null, null, null), cancellationToken).ConfigureAwait(false))
                    .Select(item => new LearningPortalListItemViewModel(item.Slug, item.Title, item.ShortDescription, $"{item.CefrLevel} - {item.GrammarCategory}"))
                    .ToArray();
            case "expressions":
                return (await _expressions.GetPublishedExpressionsAsync(new ExpressionListFilterModel(null, null, null, null, null, null, null), cancellationToken).ConfigureAwait(false))
                    .Select(item => new LearningPortalListItemViewModel(item.Slug, item.ExpressionText, item.ActualMeaning, $"{item.CefrLevel} - {item.ExpressionType} - {item.Register}"))
                    .ToArray();
            case "exercises":
                return (await _exercises.GetPublishedExerciseSetsAsync(new ExerciseSetListFilterModel(null, null, null, null), cancellationToken).ConfigureAwait(false))
                    .Select(item => new LearningPortalListItemViewModel(item.Slug, item.Title, item.Description, $"{item.CefrLevel} - {item.OwnerType} - {item.ExerciseCount}"))
                    .ToArray();
            case "courses":
                return (await _courses.GetPublishedCoursePathsAsync(new CoursePathListFilterModel(null, null), cancellationToken).ConfigureAwait(false))
                    .Select(item => new LearningPortalListItemViewModel(item.Slug, item.Title, item.Description, $"{item.CefrLevel ?? item.CefrRange} - {item.ModuleCount} - {item.LessonCount}"))
                    .ToArray();
            case "writing-templates":
                return (await _writingTemplates.GetPublishedWritingTemplatesAsync(new WritingTemplateListFilterModel(null, null, null, null, null), cancellationToken).ConfigureAwait(false))
                    .Select(item => new LearningPortalListItemViewModel(item.Slug, item.Title, item.ShortDescription, $"{item.CefrLevel} - {item.Category} - {item.Register}"))
                    .ToArray();
            case "cultural-notes":
                return (await _culturalNotes.GetPublishedCulturalNotesAsync(new CulturalNoteListFilterModel(null, null, null, null), cancellationToken).ConfigureAwait(false))
                    .Select(item => new LearningPortalListItemViewModel(item.Slug, item.Title, item.ShortDescription, $"{item.CefrLevel} - {item.Category} - {item.Context}"))
                    .ToArray();
            case "exam-prep":
                return (await _examPrep.GetPublishedExamPrepUnitsAsync(new ExamPrepListFilterModel(null, null, null, null, null, null), cancellationToken).ConfigureAwait(false))
                    .Select(item => new LearningPortalListItemViewModel(item.Slug, item.Title, item.ShortDescription, $"{item.CefrLevel} - {item.ExamProfileKey} - {item.SkillFocus}"))
                    .ToArray();
            case "talk-topics":
                return (await _talkTopics.GetPublishedTalkTopicsAsync(new TalkTopicListFilterModel(null, null, null, null, null, null), cancellationToken).ConfigureAwait(false))
                    .Select(item => new LearningPortalListItemViewModel(item.Slug, item.Title, item.Description, $"{item.CefrLevel} - {item.Category} - {item.ContentType}"))
                    .ToArray();
            default:
                return [];
        }
    }

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not LearningPortalListItemViewModel selectedItem)
        {
            return;
        }

        ItemsCollectionView.SelectedItem = null;
        string module = Uri.EscapeDataString(LearningPortalModuleDescriptor.Resolve(Module).Key);
        string slug = Uri.EscapeDataString(selectedItem.Slug);
        await Shell.Current.GoToAsync($"{nameof(LearningPortalDetailPage)}?module={module}&slug={slug}").ConfigureAwait(true);
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

    private sealed record LearningPortalListItemViewModel(string Slug, string Title, string Description, string MetadataLine);
}

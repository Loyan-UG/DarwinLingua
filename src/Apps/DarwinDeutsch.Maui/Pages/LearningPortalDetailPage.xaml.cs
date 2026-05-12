using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays a compact dynamic detail view for Learning Portal content available in the local catalog.
/// </summary>
[QueryProperty(nameof(Module), "module")]
[QueryProperty(nameof(Slug), "slug")]
public partial class LearningPortalDetailPage : ContentPage
{
    private readonly IGrammarTopicQueryService _grammarTopics;
    private readonly IExpressionQueryService _expressions;
    private readonly IExerciseQueryService _exercises;
    private readonly ICourseQueryService _courses;
    private readonly IWritingTemplateQueryService _writingTemplates;
    private readonly ICulturalNoteQueryService _culturalNotes;
    private readonly IExamPrepQueryService _examPrep;
    private readonly ITalkTopicQueryService _talkTopics;
    private readonly IActiveLearningProfileCacheService _profileCache;
    private CancellationTokenSource? _refreshCancellationTokenSource;

    public LearningPortalDetailPage(
        IGrammarTopicQueryService grammarTopics,
        IExpressionQueryService expressions,
        IExerciseQueryService exercises,
        ICourseQueryService courses,
        IWritingTemplateQueryService writingTemplates,
        ICulturalNoteQueryService culturalNotes,
        IExamPrepQueryService examPrep,
        ITalkTopicQueryService talkTopics,
        IActiveLearningProfileCacheService profileCache)
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
        _profileCache = profileCache;
    }

    public string Module { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

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
        LoadingStateView.Message = AppStrings.CommonStateLoading;
        LoadingStateView.IsLoading = true;
        ContentLayout.Clear();

        string module = LearningPortalModuleDescriptor.Resolve(Module).Key;
        string primaryLanguage = "en";
        string? secondaryLanguage = null;

        try
        {
            var profile = await _profileCache.GetCurrentProfileAsync(cancellationToken).ConfigureAwait(true);
            primaryLanguage = profile.PreferredMeaningLanguage1;
            secondaryLanguage = profile.PreferredMeaningLanguage2;
        }
        catch (InvalidOperationException)
        {
        }

        bool found = await LoadDetailAsync(module, Slug, primaryLanguage, secondaryLanguage, cancellationToken).ConfigureAwait(true);
        LoadingStateView.IsLoading = false;

        if (!found)
        {
            Title = AppStrings.LearningPortalNotFoundTitle;
            AddHeadline(AppStrings.LearningPortalNotFoundTitle);
            AddBody(AppStrings.LearningPortalNotFoundDescription);
        }
    }

    private async Task<bool> LoadDetailAsync(string module, string slug, string primaryLanguage, string? secondaryLanguage, CancellationToken cancellationToken)
    {
        switch (module)
        {
            case "grammar":
                GrammarTopicDetailModel? grammar = await _grammarTopics.GetPublishedGrammarTopicBySlugAsync(slug, primaryLanguage, cancellationToken).ConfigureAwait(false);
                if (grammar is null) { return false; }
                AddHeader(grammar.Title, $"{grammar.CefrLevel} - {grammar.GrammarCategory}", grammar.ShortDescription);
                AddSection(AppStrings.LearningPortalSectionExplanation, grammar.Sections.Select(section => $"{section.Heading}\n{section.Explanation}"));
                AddSection(AppStrings.LearningPortalSectionExamples, grammar.Examples.Select(example => string.IsNullOrWhiteSpace(example.Translation) ? example.GermanText : $"{example.GermanText}\n{example.Translation}"));
                AddSection(AppStrings.LearningPortalSectionLinkedContent, grammar.LinkedWords.Select(word => word.WordSlug ?? word.Lemma).Concat(grammar.LinkedDialogueSlugs).Concat(grammar.LinkedTalkTopicSlugs).Concat(grammar.LinkedExerciseSlugs));
                return true;
            case "expressions":
                ExpressionDetailModel? expression = await _expressions.GetPublishedExpressionBySlugAsync(slug, primaryLanguage, cancellationToken).ConfigureAwait(false);
                if (expression is null) { return false; }
                AddHeader(expression.ExpressionText, $"{expression.CefrLevel} - {expression.ExpressionType} - {expression.Register}", expression.ActualMeaning);
                AddSection(AppStrings.LearningPortalSectionExplanation, new[] { expression.UsageExplanation, expression.LiteralMeaning }.Where(text => !string.IsNullOrWhiteSpace(text)));
                AddSection(AppStrings.LearningPortalSectionWarnings, expression.Warnings.Select(warning => $"{warning.WarningType}: {warning.Text}"));
                AddSection(AppStrings.LearningPortalSectionExamples, expression.Examples.Select(example => string.IsNullOrWhiteSpace(example.Translation) ? example.GermanText : $"{example.GermanText}\n{example.Translation}"));
                AddSection(AppStrings.LearningPortalSectionLinkedContent, expression.LinkedWords.Select(word => word.WordSlug ?? word.Lemma).Concat(expression.RelatedExpressionSlugs).Concat(expression.LinkedExerciseSlugs));
                return true;
            case "exercises":
                ExerciseSetDetailModel? set = await _exercises.GetPublishedExerciseSetBySlugAsync(slug, cancellationToken).ConfigureAwait(false);
                if (set is null) { return false; }
                AddHeader(set.Title, $"{set.CefrLevel} - {set.OwnerType}", set.Description);
                AddSection(AppStrings.LearningPortalSectionExercises, set.Exercises.Select(exercise => $"{exercise.Title} - {exercise.ExerciseType}"));
                return true;
            case "courses":
                CoursePathDetailModel? course = await _courses.GetPublishedCoursePathBySlugAsync(slug, cancellationToken).ConfigureAwait(false);
                if (course is null) { return false; }
                AddHeader(course.Title, course.CefrLevel ?? course.CefrRange, course.Description);
                AddSection(AppStrings.LearningPortalSectionLessons, course.Modules.SelectMany(moduleItem => moduleItem.Lessons.Select(lesson => $"{moduleItem.Title}: {lesson.LessonNumber}. {lesson.Title}")));
                return true;
            case "writing-templates":
                WritingTemplateDetailModel? template = await _writingTemplates.GetPublishedWritingTemplateBySlugAsync(slug, cancellationToken).ConfigureAwait(false);
                if (template is null) { return false; }
                AddHeader(template.Title, $"{template.CefrLevel} - {template.Category} - {template.Register}", template.ShortDescription);
                AddSection(AppStrings.LearningPortalSectionTemplate, new[] { template.TemplateText });
                AddSection(AppStrings.LearningPortalSectionSample, new[] { template.SampleFilledVersion });
                AddSection(AppStrings.LearningPortalSectionLinkedContent, template.LinkedGrammarTopicSlugs.Concat(template.LinkedWordSlugs).Concat(template.LinkedExpressionSlugs).Concat(template.LinkedExerciseSlugs));
                return true;
            case "cultural-notes":
                CulturalNoteDetailModel? note = await _culturalNotes.GetPublishedCulturalNoteBySlugAsync(slug, cancellationToken).ConfigureAwait(false);
                if (note is null) { return false; }
                AddHeader(note.Title, $"{note.CefrLevel} - {note.Category}", note.ShortDescription);
                AddSection(AppStrings.LearningPortalSectionExplanation, note.Sections);
                AddSection(AppStrings.LearningPortalSectionExamples, note.Examples.Select(example => $"{example.GermanText}\n{example.Explanation}"));
                AddSection(AppStrings.LearningPortalSectionLinkedContent, note.LinkedDialogueSlugs.Concat(note.LinkedExpressionSlugs).Concat(note.LinkedWritingTemplateSlugs).Concat(note.LinkedTalkTopicSlugs).Concat(note.LinkedCourseLessonSlugs));
                return true;
            case "exam-prep":
                ExamPrepUnitDetailModel? exam = await _examPrep.GetPublishedExamPrepUnitBySlugAsync(slug, cancellationToken).ConfigureAwait(false);
                if (exam is null) { return false; }
                AddHeader(exam.Title, $"{exam.CefrLevel} - {exam.ExamProfileKey} - {exam.SkillFocus}", exam.ShortDescription);
                AddSection(AppStrings.LearningPortalSectionExplanation, new[] { exam.Explanation });
                AddSection(AppStrings.LearningPortalSectionStrategy, exam.StrategyNotes.Concat(exam.Checklist));
                AddSection(AppStrings.LearningPortalSectionLinkedContent, exam.LinkedDialogueSlugs.Concat(exam.LinkedTalkTopicSlugs).Concat(exam.LinkedGrammarTopicSlugs).Concat(exam.LinkedExpressionSlugs).Concat(exam.LinkedWritingTemplateSlugs).Concat(exam.LinkedExerciseSlugs).Concat(exam.LinkedCourseLessonSlugs));
                return true;
            case "talk-topics":
                TalkTopicDetailModel? talkTopic = await _talkTopics.GetPublishedTalkTopicBySlugAsync(slug, primaryLanguage, secondaryLanguage, cancellationToken).ConfigureAwait(false);
                if (talkTopic is null) { return false; }
                AddHeader(talkTopic.Title, $"{talkTopic.CefrLevel} - {talkTopic.Category} - {talkTopic.ContentType}", talkTopic.Description);
                AddSection(AppStrings.LearningPortalSectionExplanation, new[] { talkTopic.ArticleBaseText, talkTopic.PrimaryArticleTranslation }.Where(text => !string.IsNullOrWhiteSpace(text)));
                AddSection(AppStrings.LearningPortalSectionQuestions, talkTopic.WarmupQuestions.Select(question => question.Prompt).Concat(talkTopic.DiscussionQuestions.Select(question => question.Prompt)));
                return true;
            default:
                return false;
        }
    }

    private void AddHeader(string title, string metadata, string description)
    {
        Title = title;
        AddHeadline(title);
        AddCaption(metadata);
        AddBody(description);
    }

    private void AddSection(string title, IEnumerable<string?> lines)
    {
        string[] values = lines.Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line!.Trim()).ToArray();
        if (values.Length == 0)
        {
            return;
        }

        AddCaption(title);
        foreach (string value in values)
        {
            AddBody(value);
        }
    }

    private void AddHeadline(string text) =>
        ContentLayout.Add(new Label { Text = text, Style = (Style)Application.Current!.Resources["Headline"], LineBreakMode = LineBreakMode.WordWrap });

    private void AddCaption(string text) =>
        ContentLayout.Add(new Label { Text = text, Style = (Style)Application.Current!.Resources["Caption"], LineBreakMode = LineBreakMode.WordWrap });

    private void AddBody(string text) =>
        ContentLayout.Add(new Label { Text = text, Style = (Style)Application.Current!.Resources["Body"], LineBreakMode = LineBreakMode.WordWrap });

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
}

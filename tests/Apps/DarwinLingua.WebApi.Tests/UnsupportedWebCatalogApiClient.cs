using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;

namespace DarwinLingua.WebApi.Tests;

internal abstract class UnsupportedWebCatalogApiClient : IWebCatalogApiClient
{
    public virtual Task<IReadOnlyList<TopicListItemModel>> GetTopicsAsync(string uiLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<WordCollectionListItemModel>> GetCollectionsAsync(string meaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<WordCollectionDetailModel?> GetCollectionBySlugAsync(string slug, string meaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<DialogueLessonListItemModel>> GetDialoguesAsync(DialogueLessonListFilterModel filter, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<DialogueLessonDetailModel?> GetDialogueBySlugAsync(string slug, string primaryMeaningLanguageCode, string? secondaryMeaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<RoleplayScenarioListItemModel>> GetRoleplaysAsync(RoleplayScenarioListFilterModel filter, string primaryMeaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<RoleplayScenarioDetailModel?> GetRoleplayBySlugAsync(string slug, string primaryMeaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<TalkTopicListItemModel>> GetTalkTopicsAsync(TalkTopicListFilterModel filter, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<TalkTopicDetailModel?> GetTalkTopicBySlugAsync(string slug, string primaryMeaningLanguageCode, string? secondaryMeaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<GrammarTopicListItemModel>> GetGrammarTopicsAsync(GrammarTopicListFilterModel filter, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<GrammarTopicDetailModel?> GetGrammarTopicBySlugAsync(string slug, string primaryMeaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<ExpressionListItemModel>> GetExpressionsAsync(ExpressionListFilterModel filter, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<ExpressionDetailModel?> GetExpressionBySlugAsync(string slug, string primaryMeaningLanguageCode, bool includeSensitiveEducationalLanguage, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<ExerciseSetListItemModel>> GetExerciseSetsAsync(ExerciseSetListFilterModel filter, string primaryMeaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<ExerciseSetDetailModel?> GetExerciseSetBySlugAsync(string slug, string primaryMeaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<ExerciseDetailModel?> GetExerciseBySlugAsync(string slug, string primaryMeaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<ExerciseAttemptResultModel?> SubmitExerciseAttemptAsync(string slug, ExerciseAttemptRequestModel request, string primaryMeaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<CoursePathListItemModel>> GetCoursesAsync(CoursePathListFilterModel filter, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<CoursePathDetailModel?> GetCourseBySlugAsync(string slug, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<CourseLessonDetailModel?> GetCourseLessonBySlugAsync(string slug, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<WritingTemplateListItemModel>> GetWritingTemplatesAsync(WritingTemplateListFilterModel filter, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<WritingTemplateDetailModel?> GetWritingTemplateBySlugAsync(string slug, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<CulturalNoteListItemModel>> GetCulturalNotesAsync(CulturalNoteListFilterModel filter, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<CulturalNoteDetailModel?> GetCulturalNoteBySlugAsync(string slug, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<ExamProfileModel>> GetExamProfilesAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<ExamPrepUnitListItemModel>> GetExamPrepUnitsAsync(ExamPrepListFilterModel filter, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<ExamPrepUnitDetailModel?> GetExamPrepUnitBySlugAsync(string slug, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<UnifiedLearningSearchResultModel>> SearchLearningContentAsync(UnifiedLearningSearchFilterModel filter, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetConversationStarterPacksAsync(ConversationStarterListFilterModel filter, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetConversationStarterPacksForDialogueAsync(string dialogueSlug, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<ConversationStarterPackDetailModel?> GetConversationStarterPackBySlugAsync(string slug, string primaryMeaningLanguageCode, string? secondaryMeaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<EventPreparationPackListItemModel>> GetEventPreparationPacksForDialogueAsync(string dialogueSlug, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<EventPreparationPackDetailModel?> GetEventPreparationPackBySlugAsync(string slug, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<ConversationEventListItemModel>> GetConversationEventsAsync(ConversationEventListFilterModel filter, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<ConversationEventDetailModel?> GetConversationEventBySlugAsync(string slug, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<OrganizerProfileListItemModel>> GetOrganizerProfilesAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<OrganizerProfileDetailModel?> GetOrganizerProfileBySlugAsync(string slug, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<WordListItemModel>> GetWordsByTopicPageAsync(string topicKey, string meaningLanguageCode, int skip, int take, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<WordListItemModel>> GetWordsByCefrPageAsync(string cefrLevel, string meaningLanguageCode, int skip, int take, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<WordListItemModel>> SearchWordsAsync(string query, string meaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<WordDetailModel?> GetWordDetailsAsync(Guid publicId, string primaryMeaningLanguageCode, string? secondaryMeaningLanguageCode, string uiLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<WordDetailModel?> GetWordDetailsBySlugAsync(string slug, string primaryMeaningLanguageCode, string? secondaryMeaningLanguageCode, string uiLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<IReadOnlyList<WordListItemModel>> GetWordsByIdsAsync(IReadOnlyList<Guid> wordIds, string meaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminImportsPageViewModel> GetAdminImportsAsync(string? statusFilter, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminWordsPageViewModel> GetAdminWordsAsync(string? query, string? statusFilter, string? sort, int skip, int take, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminWordDetailViewModel?> GetAdminWordAsync(Guid publicId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminWordDetailViewModel> UpdateAdminWordMetadataAsync(Guid publicId, AdminUpdateWordMetadataRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminWordDetailViewModel> CreateAdminWordAsync(AdminUpdateWordMetadataRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminBulkWordImportResponse> ImportAdminWordsAsync(AdminBulkWordImportRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminWordDetailViewModel> AddAdminWordSenseAsync(Guid publicId, AdminAddWordSenseRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminWordDetailViewModel> AddAdminWordSenseTranslationAsync(Guid publicId, Guid senseId, AdminAddWordSenseTranslationRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminWordDetailViewModel> AddAdminWordSenseExampleAsync(Guid publicId, Guid senseId, AdminAddWordSenseExampleRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminWordDetailViewModel> UpdateAdminWordSenseTranslationAsync(Guid publicId, Guid senseId, Guid translationId, AdminUpdateWordSenseTranslationRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminWordDetailViewModel> DeleteAdminWordSenseTranslationAsync(Guid publicId, Guid senseId, Guid translationId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminWordDetailViewModel> UpdateAdminWordSenseExampleAsync(Guid publicId, Guid senseId, Guid exampleId, AdminUpdateWordSenseExampleRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminWordDetailViewModel> DeleteAdminWordSenseExampleAsync(Guid publicId, Guid senseId, Guid exampleId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminWordDetailViewModel> AddAdminWordSenseExampleTranslationAsync(Guid publicId, Guid senseId, Guid exampleId, AdminAddWordSenseExampleTranslationRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminWordDetailViewModel> UpdateAdminWordSenseExampleTranslationAsync(Guid publicId, Guid senseId, Guid exampleId, Guid translationId, AdminUpdateWordSenseExampleTranslationRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminWordDetailViewModel> DeleteAdminWordSenseExampleTranslationAsync(Guid publicId, Guid senseId, Guid exampleId, Guid translationId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminWordDetailViewModel> AddAdminWordTopicAsync(Guid publicId, AdminAddWordTopicRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminWordDetailViewModel> DeleteAdminWordTopicAsync(Guid publicId, Guid topicId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminWordDetailViewModel> AddAdminWordLabelAsync(Guid publicId, AdminAddWordLabelRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminWordDetailViewModel> DeleteAdminWordLabelAsync(Guid publicId, string kind, string key, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDraftWordsPageViewModel> GetAdminDraftWordsAsync(string? query, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminHistoryPageViewModel> GetAdminHistoryAsync(string? statusFilter, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminRollbackPageViewModel> GetAdminRollbackPreviewAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminTopicsPageViewModel> GetAdminTopicsAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminTopicItemViewModel?> GetAdminTopicAsync(Guid topicId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminTopicItemViewModel> CreateAdminTopicAsync(AdminSaveTopicRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminTopicItemViewModel> UpdateAdminTopicAsync(Guid topicId, AdminSaveTopicRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminTopicItemViewModel> MergeAdminTopicAsync(Guid topicId, AdminMergeTopicRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminLabelsPageViewModel> GetAdminLabelsAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminLabelItemViewModel?> GetAdminLabelAsync(Guid labelId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminLabelItemViewModel> CreateAdminLabelAsync(AdminSaveLabelRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminLabelItemViewModel> UpdateAdminLabelAsync(Guid labelId, AdminSaveLabelRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminLabelItemViewModel> RenameAdminLabelAsync(Guid labelId, AdminSaveLabelRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminLabelItemViewModel> MergeAdminLabelAsync(Guid labelId, AdminMergeLabelRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminCollectionsPageViewModel> GetAdminCollectionsAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminCollectionDetailViewModel?> GetAdminCollectionAsync(Guid collectionId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminCollectionDetailViewModel> CreateAdminCollectionAsync(AdminSaveCollectionRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminCollectionDetailViewModel> UpdateAdminCollectionAsync(Guid collectionId, AdminSaveCollectionRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<bool> DeleteAdminCollectionAsync(Guid collectionId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminCollectionDetailViewModel> AddAdminCollectionWordAsync(Guid collectionId, AdminAddCollectionWordRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminCollectionDetailViewModel> DeleteAdminCollectionWordAsync(Guid collectionId, Guid entryId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminBulkCollectionImportResponse> ImportAdminCollectionsAsync(AdminBulkCollectionImportRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialoguesPageViewModel> GetAdminDialoguesAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialogueDetailViewModel?> GetAdminDialogueAsync(Guid dialogueId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialogueDetailViewModel> CreateAdminDialogueAsync(AdminSaveDialogueRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialogueDetailViewModel> UpdateAdminDialogueAsync(Guid dialogueId, AdminSaveDialogueRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<bool> DeleteAdminDialogueAsync(Guid dialogueId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminBulkDialogueImportResponse> ImportAdminDialoguesAsync(AdminBulkDialogueImportRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialogueDetailViewModel> AddAdminDialogueTurnAsync(Guid dialogueId, AdminAddDialogueTurnRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialogueDetailViewModel> DeleteAdminDialogueTurnAsync(Guid dialogueId, Guid turnId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialogueDetailViewModel> AddAdminDialoguePhraseAsync(Guid dialogueId, AdminAddDialoguePhraseRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialogueDetailViewModel> DeleteAdminDialoguePhraseAsync(Guid dialogueId, Guid phraseId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialogueDetailViewModel> AddAdminDialogueQuestionAsync(Guid dialogueId, AdminAddDialogueQuestionRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialogueDetailViewModel> DeleteAdminDialogueQuestionAsync(Guid dialogueId, Guid questionId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialogueDetailViewModel> AddAdminDialogueAnswerAsync(Guid dialogueId, Guid questionId, AdminAddDialogueAnswerRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialogueDetailViewModel> DeleteAdminDialogueAnswerAsync(Guid dialogueId, Guid questionId, Guid answerId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialogueDetailViewModel> AddAdminDialogueTurnTranslationAsync(Guid dialogueId, Guid turnId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialogueDetailViewModel> DeleteAdminDialogueTurnTranslationAsync(Guid dialogueId, Guid turnId, Guid translationId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialogueDetailViewModel> AddAdminDialoguePhraseTranslationAsync(Guid dialogueId, Guid phraseId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialogueDetailViewModel> DeleteAdminDialoguePhraseTranslationAsync(Guid dialogueId, Guid phraseId, Guid translationId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialogueDetailViewModel> AddAdminDialogueQuestionTranslationAsync(Guid dialogueId, Guid questionId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialogueDetailViewModel> DeleteAdminDialogueQuestionTranslationAsync(Guid dialogueId, Guid questionId, Guid translationId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialogueDetailViewModel> AddAdminDialogueAnswerTranslationAsync(Guid dialogueId, Guid questionId, Guid answerId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<AdminDialogueDetailViewModel> DeleteAdminDialogueAnswerTranslationAsync(Guid dialogueId, Guid questionId, Guid answerId, Guid translationId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<ConversationEventDetailModel> SaveAdminConversationEventAsync(AdminSaveConversationEventRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();

    public virtual Task<OrganizerProfileDetailModel> SaveAdminOrganizerProfileAsync(AdminSaveOrganizerProfileRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();
}

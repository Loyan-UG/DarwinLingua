using DarwinLingua.Web.Models;

namespace DarwinLingua.Web.Services;

public interface IWebAdminOperationsQueryService
{
    Task<AdminImportsPageViewModel> GetImportsAsync(string? statusFilter, CancellationToken cancellationToken);

    Task<AdminLearningPortalIssuesPageViewModel> GetLearningPortalIssuesAsync(
        string? areaFilter,
        string? query,
        int take,
        CancellationToken cancellationToken);

    Task<AdminWordsPageViewModel> GetWordsAsync(
        string? query,
        string? statusFilter,
        string? sort,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel?> GetWordAsync(Guid publicId, CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> UpdateWordMetadataAsync(
        Guid publicId,
        AdminUpdateWordMetadataRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> CreateWordAsync(
        AdminUpdateWordMetadataRequest request,
        CancellationToken cancellationToken);

    Task<AdminBulkWordImportResponse> ImportWordsAsync(
        AdminBulkWordImportRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> AddWordSenseAsync(
        Guid publicId,
        AdminAddWordSenseRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> AddWordSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        AdminAddWordSenseTranslationRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> AddWordSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        AdminAddWordSenseExampleRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> UpdateWordSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid translationId,
        AdminUpdateWordSenseTranslationRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> DeleteWordSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid translationId,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> UpdateWordSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        AdminUpdateWordSenseExampleRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> DeleteWordSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> AddWordSenseExampleTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        AdminAddWordSenseExampleTranslationRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> UpdateWordSenseExampleTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        Guid translationId,
        AdminUpdateWordSenseExampleTranslationRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> DeleteWordSenseExampleTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        Guid translationId,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> AddWordTopicAsync(
        Guid publicId,
        AdminAddWordTopicRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> DeleteWordTopicAsync(
        Guid publicId,
        Guid topicId,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> AddWordLabelAsync(
        Guid publicId,
        AdminAddWordLabelRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> DeleteWordLabelAsync(
        Guid publicId,
        string kind,
        string key,
        CancellationToken cancellationToken);

    Task<AdminDraftWordsPageViewModel> GetDraftWordsAsync(string? query, CancellationToken cancellationToken);

    Task<AdminHistoryPageViewModel> GetHistoryAsync(string? statusFilter, CancellationToken cancellationToken);

    Task<AdminRollbackPageViewModel> GetRollbackPreviewAsync(CancellationToken cancellationToken);

    Task<AdminTopicsPageViewModel> GetTopicsAsync(CancellationToken cancellationToken);

    Task<AdminTopicItemViewModel?> GetTopicAsync(Guid topicId, CancellationToken cancellationToken);

    Task<AdminTopicItemViewModel> CreateTopicAsync(AdminSaveTopicRequest request, CancellationToken cancellationToken);

    Task<AdminTopicItemViewModel> UpdateTopicAsync(Guid topicId, AdminSaveTopicRequest request, CancellationToken cancellationToken);

    Task<AdminTopicItemViewModel> MergeTopicAsync(Guid topicId, AdminMergeTopicRequest request, CancellationToken cancellationToken);

    Task<AdminLabelsPageViewModel> GetLabelsAsync(CancellationToken cancellationToken);

    Task<AdminLabelItemViewModel?> GetLabelAsync(Guid labelId, CancellationToken cancellationToken);

    Task<AdminLabelItemViewModel> CreateLabelAsync(AdminSaveLabelRequest request, CancellationToken cancellationToken);

    Task<AdminLabelItemViewModel> UpdateLabelAsync(Guid labelId, AdminSaveLabelRequest request, CancellationToken cancellationToken);

    Task<AdminLabelItemViewModel> RenameLabelAsync(Guid labelId, AdminSaveLabelRequest request, CancellationToken cancellationToken);

    Task<AdminLabelItemViewModel> MergeLabelAsync(Guid labelId, AdminMergeLabelRequest request, CancellationToken cancellationToken);

    Task<AdminCollectionsPageViewModel> GetCollectionsAsync(CancellationToken cancellationToken);

    Task<AdminCollectionDetailViewModel?> GetCollectionAsync(Guid collectionId, CancellationToken cancellationToken);

    Task<AdminCollectionDetailViewModel> CreateCollectionAsync(AdminSaveCollectionRequest request, CancellationToken cancellationToken);

    Task<AdminCollectionDetailViewModel> UpdateCollectionAsync(Guid collectionId, AdminSaveCollectionRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteCollectionAsync(Guid collectionId, CancellationToken cancellationToken);

    Task<AdminCollectionDetailViewModel> AddCollectionWordAsync(Guid collectionId, AdminAddCollectionWordRequest request, CancellationToken cancellationToken);

    Task<AdminCollectionDetailViewModel> DeleteCollectionWordAsync(Guid collectionId, Guid entryId, CancellationToken cancellationToken);

    Task<AdminBulkCollectionImportResponse> ImportCollectionsAsync(AdminBulkCollectionImportRequest request, CancellationToken cancellationToken);

    Task<AdminDialoguesPageViewModel> GetDialoguesAsync(CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel?> GetDialogueAsync(Guid dialogueId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> CreateDialogueAsync(AdminSaveDialogueRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> UpdateDialogueAsync(Guid dialogueId, AdminSaveDialogueRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteDialogueAsync(Guid dialogueId, CancellationToken cancellationToken);

    Task<AdminBulkDialogueImportResponse> ImportDialoguesAsync(AdminBulkDialogueImportRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> AddDialogueTurnAsync(Guid dialogueId, AdminAddDialogueTurnRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> DeleteDialogueTurnAsync(Guid dialogueId, Guid turnId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> AddDialoguePhraseAsync(Guid dialogueId, AdminAddDialoguePhraseRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> DeleteDialoguePhraseAsync(Guid dialogueId, Guid phraseId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> AddDialogueQuestionAsync(Guid dialogueId, AdminAddDialogueQuestionRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> DeleteDialogueQuestionAsync(Guid dialogueId, Guid questionId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> AddDialogueAnswerAsync(Guid dialogueId, Guid questionId, AdminAddDialogueAnswerRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> DeleteDialogueAnswerAsync(Guid dialogueId, Guid questionId, Guid answerId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> AddDialogueTurnTranslationAsync(Guid dialogueId, Guid turnId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> DeleteDialogueTurnTranslationAsync(Guid dialogueId, Guid turnId, Guid translationId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> AddDialoguePhraseTranslationAsync(Guid dialogueId, Guid phraseId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> DeleteDialoguePhraseTranslationAsync(Guid dialogueId, Guid phraseId, Guid translationId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> AddDialogueQuestionTranslationAsync(Guid dialogueId, Guid questionId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> DeleteDialogueQuestionTranslationAsync(Guid dialogueId, Guid questionId, Guid translationId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> AddDialogueAnswerTranslationAsync(Guid dialogueId, Guid questionId, Guid answerId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> DeleteDialogueAnswerTranslationAsync(Guid dialogueId, Guid questionId, Guid answerId, Guid translationId, CancellationToken cancellationToken);
}

internal sealed class WebAdminOperationsQueryService(IWebCatalogApiClient catalogApiClient) : IWebAdminOperationsQueryService
{
    public Task<AdminImportsPageViewModel> GetImportsAsync(string? statusFilter, CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminImportsAsync(statusFilter, cancellationToken);

    public Task<AdminLearningPortalIssuesPageViewModel> GetLearningPortalIssuesAsync(
        string? areaFilter,
        string? query,
        int take,
        CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminLearningPortalIssuesAsync(areaFilter, query, take, cancellationToken);

    public Task<AdminWordsPageViewModel> GetWordsAsync(
        string? query,
        string? statusFilter,
        string? sort,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminWordsAsync(query, statusFilter, sort, skip, take, cancellationToken);

    public Task<AdminWordDetailViewModel?> GetWordAsync(Guid publicId, CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminWordAsync(publicId, cancellationToken);

    public Task<AdminWordDetailViewModel> UpdateWordMetadataAsync(
        Guid publicId,
        AdminUpdateWordMetadataRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.UpdateAdminWordMetadataAsync(publicId, request, cancellationToken);

    public Task<AdminWordDetailViewModel> CreateWordAsync(
        AdminUpdateWordMetadataRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.CreateAdminWordAsync(request, cancellationToken);

    public Task<AdminBulkWordImportResponse> ImportWordsAsync(
        AdminBulkWordImportRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.ImportAdminWordsAsync(request, cancellationToken);

    public Task<AdminWordDetailViewModel> AddWordSenseAsync(
        Guid publicId,
        AdminAddWordSenseRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminWordSenseAsync(publicId, request, cancellationToken);

    public Task<AdminWordDetailViewModel> AddWordSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        AdminAddWordSenseTranslationRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminWordSenseTranslationAsync(publicId, senseId, request, cancellationToken);

    public Task<AdminWordDetailViewModel> AddWordSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        AdminAddWordSenseExampleRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminWordSenseExampleAsync(publicId, senseId, request, cancellationToken);

    public Task<AdminWordDetailViewModel> UpdateWordSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid translationId,
        AdminUpdateWordSenseTranslationRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.UpdateAdminWordSenseTranslationAsync(publicId, senseId, translationId, request, cancellationToken);

    public Task<AdminWordDetailViewModel> DeleteWordSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid translationId,
        CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminWordSenseTranslationAsync(publicId, senseId, translationId, cancellationToken);

    public Task<AdminWordDetailViewModel> UpdateWordSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        AdminUpdateWordSenseExampleRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.UpdateAdminWordSenseExampleAsync(publicId, senseId, exampleId, request, cancellationToken);

    public Task<AdminWordDetailViewModel> DeleteWordSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminWordSenseExampleAsync(publicId, senseId, exampleId, cancellationToken);

    public Task<AdminWordDetailViewModel> AddWordSenseExampleTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        AdminAddWordSenseExampleTranslationRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminWordSenseExampleTranslationAsync(publicId, senseId, exampleId, request, cancellationToken);

    public Task<AdminWordDetailViewModel> UpdateWordSenseExampleTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        Guid translationId,
        AdminUpdateWordSenseExampleTranslationRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.UpdateAdminWordSenseExampleTranslationAsync(publicId, senseId, exampleId, translationId, request, cancellationToken);

    public Task<AdminWordDetailViewModel> DeleteWordSenseExampleTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        Guid translationId,
        CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminWordSenseExampleTranslationAsync(publicId, senseId, exampleId, translationId, cancellationToken);

    public Task<AdminWordDetailViewModel> AddWordTopicAsync(
        Guid publicId,
        AdminAddWordTopicRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminWordTopicAsync(publicId, request, cancellationToken);

    public Task<AdminWordDetailViewModel> DeleteWordTopicAsync(
        Guid publicId,
        Guid topicId,
        CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminWordTopicAsync(publicId, topicId, cancellationToken);

    public Task<AdminWordDetailViewModel> AddWordLabelAsync(
        Guid publicId,
        AdminAddWordLabelRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminWordLabelAsync(publicId, request, cancellationToken);

    public Task<AdminWordDetailViewModel> DeleteWordLabelAsync(
        Guid publicId,
        string kind,
        string key,
        CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminWordLabelAsync(publicId, kind, key, cancellationToken);

    public Task<AdminDraftWordsPageViewModel> GetDraftWordsAsync(string? query, CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminDraftWordsAsync(query, cancellationToken);

    public Task<AdminHistoryPageViewModel> GetHistoryAsync(string? statusFilter, CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminHistoryAsync(statusFilter, cancellationToken);

    public Task<AdminRollbackPageViewModel> GetRollbackPreviewAsync(CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminRollbackPreviewAsync(cancellationToken);

    public Task<AdminTopicsPageViewModel> GetTopicsAsync(CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminTopicsAsync(cancellationToken);

    public Task<AdminTopicItemViewModel?> GetTopicAsync(Guid topicId, CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminTopicAsync(topicId, cancellationToken);

    public Task<AdminTopicItemViewModel> CreateTopicAsync(AdminSaveTopicRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.CreateAdminTopicAsync(request, cancellationToken);

    public Task<AdminTopicItemViewModel> UpdateTopicAsync(Guid topicId, AdminSaveTopicRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.UpdateAdminTopicAsync(topicId, request, cancellationToken);

    public Task<AdminTopicItemViewModel> MergeTopicAsync(Guid topicId, AdminMergeTopicRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.MergeAdminTopicAsync(topicId, request, cancellationToken);

    public Task<AdminLabelsPageViewModel> GetLabelsAsync(CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminLabelsAsync(cancellationToken);

    public Task<AdminLabelItemViewModel?> GetLabelAsync(Guid labelId, CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminLabelAsync(labelId, cancellationToken);

    public Task<AdminLabelItemViewModel> CreateLabelAsync(AdminSaveLabelRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.CreateAdminLabelAsync(request, cancellationToken);

    public Task<AdminLabelItemViewModel> UpdateLabelAsync(Guid labelId, AdminSaveLabelRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.UpdateAdminLabelAsync(labelId, request, cancellationToken);

    public Task<AdminLabelItemViewModel> RenameLabelAsync(Guid labelId, AdminSaveLabelRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.RenameAdminLabelAsync(labelId, request, cancellationToken);

    public Task<AdminLabelItemViewModel> MergeLabelAsync(Guid labelId, AdminMergeLabelRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.MergeAdminLabelAsync(labelId, request, cancellationToken);

    public Task<AdminCollectionsPageViewModel> GetCollectionsAsync(CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminCollectionsAsync(cancellationToken);

    public Task<AdminCollectionDetailViewModel?> GetCollectionAsync(Guid collectionId, CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminCollectionAsync(collectionId, cancellationToken);

    public Task<AdminCollectionDetailViewModel> CreateCollectionAsync(AdminSaveCollectionRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.CreateAdminCollectionAsync(request, cancellationToken);

    public Task<AdminCollectionDetailViewModel> UpdateCollectionAsync(Guid collectionId, AdminSaveCollectionRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.UpdateAdminCollectionAsync(collectionId, request, cancellationToken);

    public Task<bool> DeleteCollectionAsync(Guid collectionId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminCollectionAsync(collectionId, cancellationToken);

    public Task<AdminCollectionDetailViewModel> AddCollectionWordAsync(Guid collectionId, AdminAddCollectionWordRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminCollectionWordAsync(collectionId, request, cancellationToken);

    public Task<AdminCollectionDetailViewModel> DeleteCollectionWordAsync(Guid collectionId, Guid entryId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminCollectionWordAsync(collectionId, entryId, cancellationToken);

    public Task<AdminBulkCollectionImportResponse> ImportCollectionsAsync(AdminBulkCollectionImportRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.ImportAdminCollectionsAsync(request, cancellationToken);

    public Task<AdminDialoguesPageViewModel> GetDialoguesAsync(CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminDialoguesAsync(cancellationToken);

    public Task<AdminDialogueDetailViewModel?> GetDialogueAsync(Guid dialogueId, CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminDialogueAsync(dialogueId, cancellationToken);

    public Task<AdminDialogueDetailViewModel> CreateDialogueAsync(AdminSaveDialogueRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.CreateAdminDialogueAsync(request, cancellationToken);

    public Task<AdminDialogueDetailViewModel> UpdateDialogueAsync(Guid dialogueId, AdminSaveDialogueRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.UpdateAdminDialogueAsync(dialogueId, request, cancellationToken);

    public Task<bool> DeleteDialogueAsync(Guid dialogueId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminDialogueAsync(dialogueId, cancellationToken);

    public Task<AdminBulkDialogueImportResponse> ImportDialoguesAsync(AdminBulkDialogueImportRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.ImportAdminDialoguesAsync(request, cancellationToken);

    public Task<AdminDialogueDetailViewModel> AddDialogueTurnAsync(Guid dialogueId, AdminAddDialogueTurnRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminDialogueTurnAsync(dialogueId, request, cancellationToken);

    public Task<AdminDialogueDetailViewModel> DeleteDialogueTurnAsync(Guid dialogueId, Guid turnId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminDialogueTurnAsync(dialogueId, turnId, cancellationToken);

    public Task<AdminDialogueDetailViewModel> AddDialoguePhraseAsync(Guid dialogueId, AdminAddDialoguePhraseRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminDialoguePhraseAsync(dialogueId, request, cancellationToken);

    public Task<AdminDialogueDetailViewModel> DeleteDialoguePhraseAsync(Guid dialogueId, Guid phraseId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminDialoguePhraseAsync(dialogueId, phraseId, cancellationToken);

    public Task<AdminDialogueDetailViewModel> AddDialogueQuestionAsync(Guid dialogueId, AdminAddDialogueQuestionRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminDialogueQuestionAsync(dialogueId, request, cancellationToken);

    public Task<AdminDialogueDetailViewModel> DeleteDialogueQuestionAsync(Guid dialogueId, Guid questionId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminDialogueQuestionAsync(dialogueId, questionId, cancellationToken);

    public Task<AdminDialogueDetailViewModel> AddDialogueAnswerAsync(Guid dialogueId, Guid questionId, AdminAddDialogueAnswerRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminDialogueAnswerAsync(dialogueId, questionId, request, cancellationToken);

    public Task<AdminDialogueDetailViewModel> DeleteDialogueAnswerAsync(Guid dialogueId, Guid questionId, Guid answerId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminDialogueAnswerAsync(dialogueId, questionId, answerId, cancellationToken);

    public Task<AdminDialogueDetailViewModel> AddDialogueTurnTranslationAsync(Guid dialogueId, Guid turnId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminDialogueTurnTranslationAsync(dialogueId, turnId, request, cancellationToken);

    public Task<AdminDialogueDetailViewModel> DeleteDialogueTurnTranslationAsync(Guid dialogueId, Guid turnId, Guid translationId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminDialogueTurnTranslationAsync(dialogueId, turnId, translationId, cancellationToken);

    public Task<AdminDialogueDetailViewModel> AddDialoguePhraseTranslationAsync(Guid dialogueId, Guid phraseId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminDialoguePhraseTranslationAsync(dialogueId, phraseId, request, cancellationToken);

    public Task<AdminDialogueDetailViewModel> DeleteDialoguePhraseTranslationAsync(Guid dialogueId, Guid phraseId, Guid translationId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminDialoguePhraseTranslationAsync(dialogueId, phraseId, translationId, cancellationToken);

    public Task<AdminDialogueDetailViewModel> AddDialogueQuestionTranslationAsync(Guid dialogueId, Guid questionId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminDialogueQuestionTranslationAsync(dialogueId, questionId, request, cancellationToken);

    public Task<AdminDialogueDetailViewModel> DeleteDialogueQuestionTranslationAsync(Guid dialogueId, Guid questionId, Guid translationId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminDialogueQuestionTranslationAsync(dialogueId, questionId, translationId, cancellationToken);

    public Task<AdminDialogueDetailViewModel> AddDialogueAnswerTranslationAsync(Guid dialogueId, Guid questionId, Guid answerId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminDialogueAnswerTranslationAsync(dialogueId, questionId, answerId, request, cancellationToken);

    public Task<AdminDialogueDetailViewModel> DeleteDialogueAnswerTranslationAsync(Guid dialogueId, Guid questionId, Guid answerId, Guid translationId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminDialogueAnswerTranslationAsync(dialogueId, questionId, answerId, translationId, cancellationToken);
}

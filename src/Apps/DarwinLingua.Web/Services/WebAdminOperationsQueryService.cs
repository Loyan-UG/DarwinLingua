using DarwinLingua.Web.Models;

namespace DarwinLingua.Web.Services;

public interface IWebAdminOperationsQueryService
{
    Task<AdminImportsPageViewModel> GetImportsAsync(string? statusFilter, CancellationToken cancellationToken);

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

    Task<AdminScenariosPageViewModel> GetScenariosAsync(CancellationToken cancellationToken);

    Task<AdminScenarioDetailViewModel?> GetScenarioAsync(Guid scenarioId, CancellationToken cancellationToken);

    Task<AdminScenarioDetailViewModel> CreateScenarioAsync(AdminSaveScenarioRequest request, CancellationToken cancellationToken);

    Task<AdminScenarioDetailViewModel> UpdateScenarioAsync(Guid scenarioId, AdminSaveScenarioRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteScenarioAsync(Guid scenarioId, CancellationToken cancellationToken);

    Task<AdminBulkScenarioImportResponse> ImportScenariosAsync(AdminBulkScenarioImportRequest request, CancellationToken cancellationToken);

    Task<AdminScenarioDetailViewModel> AddScenarioDialogueTurnAsync(Guid scenarioId, AdminAddScenarioDialogueTurnRequest request, CancellationToken cancellationToken);

    Task<AdminScenarioDetailViewModel> DeleteScenarioDialogueTurnAsync(Guid scenarioId, Guid turnId, CancellationToken cancellationToken);

    Task<AdminScenarioDetailViewModel> AddScenarioPhraseAsync(Guid scenarioId, AdminAddScenarioPhraseRequest request, CancellationToken cancellationToken);

    Task<AdminScenarioDetailViewModel> DeleteScenarioPhraseAsync(Guid scenarioId, Guid phraseId, CancellationToken cancellationToken);

    Task<AdminScenarioDetailViewModel> AddScenarioQuestionAsync(Guid scenarioId, AdminAddScenarioQuestionRequest request, CancellationToken cancellationToken);

    Task<AdminScenarioDetailViewModel> DeleteScenarioQuestionAsync(Guid scenarioId, Guid questionId, CancellationToken cancellationToken);

    Task<AdminScenarioDetailViewModel> AddScenarioAnswerAsync(Guid scenarioId, Guid questionId, AdminAddScenarioAnswerRequest request, CancellationToken cancellationToken);

    Task<AdminScenarioDetailViewModel> DeleteScenarioAnswerAsync(Guid scenarioId, Guid questionId, Guid answerId, CancellationToken cancellationToken);

    Task<AdminScenarioDetailViewModel> AddScenarioDialogueTurnTranslationAsync(Guid scenarioId, Guid turnId, AdminAddScenarioTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminScenarioDetailViewModel> DeleteScenarioDialogueTurnTranslationAsync(Guid scenarioId, Guid turnId, Guid translationId, CancellationToken cancellationToken);

    Task<AdminScenarioDetailViewModel> AddScenarioPhraseTranslationAsync(Guid scenarioId, Guid phraseId, AdminAddScenarioTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminScenarioDetailViewModel> DeleteScenarioPhraseTranslationAsync(Guid scenarioId, Guid phraseId, Guid translationId, CancellationToken cancellationToken);

    Task<AdminScenarioDetailViewModel> AddScenarioQuestionTranslationAsync(Guid scenarioId, Guid questionId, AdminAddScenarioTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminScenarioDetailViewModel> DeleteScenarioQuestionTranslationAsync(Guid scenarioId, Guid questionId, Guid translationId, CancellationToken cancellationToken);

    Task<AdminScenarioDetailViewModel> AddScenarioAnswerTranslationAsync(Guid scenarioId, Guid questionId, Guid answerId, AdminAddScenarioTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminScenarioDetailViewModel> DeleteScenarioAnswerTranslationAsync(Guid scenarioId, Guid questionId, Guid answerId, Guid translationId, CancellationToken cancellationToken);
}

internal sealed class WebAdminOperationsQueryService(IWebCatalogApiClient catalogApiClient) : IWebAdminOperationsQueryService
{
    public Task<AdminImportsPageViewModel> GetImportsAsync(string? statusFilter, CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminImportsAsync(statusFilter, cancellationToken);

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

    public Task<AdminScenariosPageViewModel> GetScenariosAsync(CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminScenariosAsync(cancellationToken);

    public Task<AdminScenarioDetailViewModel?> GetScenarioAsync(Guid scenarioId, CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminScenarioAsync(scenarioId, cancellationToken);

    public Task<AdminScenarioDetailViewModel> CreateScenarioAsync(AdminSaveScenarioRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.CreateAdminScenarioAsync(request, cancellationToken);

    public Task<AdminScenarioDetailViewModel> UpdateScenarioAsync(Guid scenarioId, AdminSaveScenarioRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.UpdateAdminScenarioAsync(scenarioId, request, cancellationToken);

    public Task<bool> DeleteScenarioAsync(Guid scenarioId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminScenarioAsync(scenarioId, cancellationToken);

    public Task<AdminBulkScenarioImportResponse> ImportScenariosAsync(AdminBulkScenarioImportRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.ImportAdminScenariosAsync(request, cancellationToken);

    public Task<AdminScenarioDetailViewModel> AddScenarioDialogueTurnAsync(Guid scenarioId, AdminAddScenarioDialogueTurnRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminScenarioDialogueTurnAsync(scenarioId, request, cancellationToken);

    public Task<AdminScenarioDetailViewModel> DeleteScenarioDialogueTurnAsync(Guid scenarioId, Guid turnId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminScenarioDialogueTurnAsync(scenarioId, turnId, cancellationToken);

    public Task<AdminScenarioDetailViewModel> AddScenarioPhraseAsync(Guid scenarioId, AdminAddScenarioPhraseRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminScenarioPhraseAsync(scenarioId, request, cancellationToken);

    public Task<AdminScenarioDetailViewModel> DeleteScenarioPhraseAsync(Guid scenarioId, Guid phraseId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminScenarioPhraseAsync(scenarioId, phraseId, cancellationToken);

    public Task<AdminScenarioDetailViewModel> AddScenarioQuestionAsync(Guid scenarioId, AdminAddScenarioQuestionRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminScenarioQuestionAsync(scenarioId, request, cancellationToken);

    public Task<AdminScenarioDetailViewModel> DeleteScenarioQuestionAsync(Guid scenarioId, Guid questionId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminScenarioQuestionAsync(scenarioId, questionId, cancellationToken);

    public Task<AdminScenarioDetailViewModel> AddScenarioAnswerAsync(Guid scenarioId, Guid questionId, AdminAddScenarioAnswerRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminScenarioAnswerAsync(scenarioId, questionId, request, cancellationToken);

    public Task<AdminScenarioDetailViewModel> DeleteScenarioAnswerAsync(Guid scenarioId, Guid questionId, Guid answerId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminScenarioAnswerAsync(scenarioId, questionId, answerId, cancellationToken);

    public Task<AdminScenarioDetailViewModel> AddScenarioDialogueTurnTranslationAsync(Guid scenarioId, Guid turnId, AdminAddScenarioTranslationRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminScenarioDialogueTurnTranslationAsync(scenarioId, turnId, request, cancellationToken);

    public Task<AdminScenarioDetailViewModel> DeleteScenarioDialogueTurnTranslationAsync(Guid scenarioId, Guid turnId, Guid translationId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminScenarioDialogueTurnTranslationAsync(scenarioId, turnId, translationId, cancellationToken);

    public Task<AdminScenarioDetailViewModel> AddScenarioPhraseTranslationAsync(Guid scenarioId, Guid phraseId, AdminAddScenarioTranslationRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminScenarioPhraseTranslationAsync(scenarioId, phraseId, request, cancellationToken);

    public Task<AdminScenarioDetailViewModel> DeleteScenarioPhraseTranslationAsync(Guid scenarioId, Guid phraseId, Guid translationId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminScenarioPhraseTranslationAsync(scenarioId, phraseId, translationId, cancellationToken);

    public Task<AdminScenarioDetailViewModel> AddScenarioQuestionTranslationAsync(Guid scenarioId, Guid questionId, AdminAddScenarioTranslationRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminScenarioQuestionTranslationAsync(scenarioId, questionId, request, cancellationToken);

    public Task<AdminScenarioDetailViewModel> DeleteScenarioQuestionTranslationAsync(Guid scenarioId, Guid questionId, Guid translationId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminScenarioQuestionTranslationAsync(scenarioId, questionId, translationId, cancellationToken);

    public Task<AdminScenarioDetailViewModel> AddScenarioAnswerTranslationAsync(Guid scenarioId, Guid questionId, Guid answerId, AdminAddScenarioTranslationRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminScenarioAnswerTranslationAsync(scenarioId, questionId, answerId, request, cancellationToken);

    public Task<AdminScenarioDetailViewModel> DeleteScenarioAnswerTranslationAsync(Guid scenarioId, Guid questionId, Guid answerId, Guid translationId, CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminScenarioAnswerTranslationAsync(scenarioId, questionId, answerId, translationId, cancellationToken);
}

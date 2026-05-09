using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using System.Collections;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Configuration;
using DarwinLingua.Web.Models;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Services;

public interface IWebCatalogApiClient
{
    Task<IReadOnlyList<TopicListItemModel>> GetTopicsAsync(string uiLanguageCode, CancellationToken cancellationToken);

    Task<IReadOnlyList<WordCollectionListItemModel>> GetCollectionsAsync(
        string meaningLanguageCode,
        CancellationToken cancellationToken);

    Task<WordCollectionDetailModel?> GetCollectionBySlugAsync(
        string slug,
        string meaningLanguageCode,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DialogueLessonListItemModel>> GetDialoguesAsync(CancellationToken cancellationToken);

    Task<DialogueLessonDetailModel?> GetDialogueBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<TalkTopicListItemModel>> GetTalkTopicsAsync(
        TalkTopicListFilterModel filter,
        CancellationToken cancellationToken);

    Task<TalkTopicDetailModel?> GetTalkTopicBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetConversationStarterPacksAsync(
        ConversationStarterListFilterModel filter,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetConversationStarterPacksForDialogueAsync(
        string dialogueSlug,
        CancellationToken cancellationToken);

    Task<ConversationStarterPackDetailModel?> GetConversationStarterPackBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<EventPreparationPackListItemModel>> GetEventPreparationPacksForDialogueAsync(
        string dialogueSlug,
        CancellationToken cancellationToken);

    Task<EventPreparationPackDetailModel?> GetEventPreparationPackBySlugAsync(
        string slug,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ConversationEventListItemModel>> GetConversationEventsAsync(
        ConversationEventListFilterModel filter,
        CancellationToken cancellationToken);

    Task<ConversationEventDetailModel?> GetConversationEventBySlugAsync(
        string slug,
        CancellationToken cancellationToken);

    Task<EventRsvpSummaryModel> GetEventRsvpSummaryAsync(
        string eventSlug,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<EventRsvpModel> SubmitEventRsvpAsync(
        string eventSlug,
        SubmitEventRsvpRequest request,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<IReadOnlyList<EventRsvpModel>> GetAdminEventRsvpsAsync(
        string eventSlug,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<EventRsvpModel> SetAdminEventRsvpStatusAsync(
        string eventSlug,
        Guid rsvpId,
        AdminSetEventRsvpStatusRequest request,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<IReadOnlyList<OrganizerProfileListItemModel>> GetOrganizerProfilesAsync(CancellationToken cancellationToken);

    Task<OrganizerProfileDetailModel?> GetOrganizerProfileBySlugAsync(
        string slug,
        CancellationToken cancellationToken);

    Task<OrganizerClaimRequestModel> SubmitOrganizerClaimRequestAsync(
        string organizerProfileSlug,
        SubmitOrganizerClaimRequest request,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<IReadOnlyList<OrganizerProfileOwnerModel>> GetOrganizerProfileOwnersByEmailAsync(
        string ownerEmail,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<LearnerConversationProfileModel?> GetLearnerConversationProfileAsync(
        string ownerEmail,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<IReadOnlyList<LearnerConversationProfilePublicModel>> GetPublicLearnerConversationProfilesAsync(
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<LearnerConversationProfileModel> SaveLearnerConversationProfileAsync(
        string ownerEmail,
        SaveLearnerConversationProfileRequest request,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<LearnerConversationProfileModel> SetLearnerConversationProfileEnabledAsync(
        string ownerEmail,
        LearnerConversationProfileVisibilityRequest request,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task DeleteLearnerConversationProfileAsync(
        string ownerEmail,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<IReadOnlyList<PartnerMatchProfileModel>> SearchPartnerMatchesAsync(
        string ownerEmail,
        PartnerMatchSearchRequest request,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<PartnerRequestModel> SubmitPartnerRequestAsync(
        string ownerEmail,
        SubmitPartnerRequestRequest request,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<IReadOnlyList<PartnerRequestModel>> GetPartnerRequestsAsync(
        string ownerEmail,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<PartnerRequestModel> UpdatePartnerRequestStateAsync(
        string ownerEmail,
        Guid requestId,
        PartnerRequestStateUpdateRequest request,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<UserReportModel> SubmitUserReportAsync(
        string reporterEmail,
        SubmitUserReportRequest request,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<UserBlockModel> BlockUserAsync(
        string blockerEmail,
        BlockUserRequest request,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<IReadOnlyList<UserReportModel>> GetAdminUserReportsAsync(
        string? status,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<IReadOnlyList<ModerationDecisionAuditModel>> GetAdminModerationDecisionAuditsAsync(
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<UserReportModel> DecideAdminUserReportAsync(
        Guid reportId,
        ModerationDecisionRequest request,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<IReadOnlyList<WordListItemModel>> GetWordsByTopicPageAsync(
        string topicKey,
        string meaningLanguageCode,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WordListItemModel>> GetWordsByCefrPageAsync(
        string cefrLevel,
        string meaningLanguageCode,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WordListItemModel>> SearchWordsAsync(
        string query,
        string meaningLanguageCode,
        CancellationToken cancellationToken);

    Task<WordDetailModel?> GetWordDetailsAsync(
        Guid publicId,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        string uiLanguageCode,
        CancellationToken cancellationToken);

    Task<WordDetailModel?> GetWordDetailsBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        string uiLanguageCode,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WordListItemModel>> GetWordsByIdsAsync(
        IReadOnlyList<Guid> wordIds,
        string meaningLanguageCode,
        CancellationToken cancellationToken);

    Task<AdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken cancellationToken);

    Task<AdminSystemReportResponse> GetAdminSystemReportAsync(CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<AdminImportsPageViewModel> GetAdminImportsAsync(string? statusFilter, CancellationToken cancellationToken);

    Task<AdminWordsPageViewModel> GetAdminWordsAsync(
        string? query,
        string? statusFilter,
        string? sort,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel?> GetAdminWordAsync(Guid publicId, CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> UpdateAdminWordMetadataAsync(
        Guid publicId,
        AdminUpdateWordMetadataRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> CreateAdminWordAsync(
        AdminUpdateWordMetadataRequest request,
        CancellationToken cancellationToken);

    Task<AdminBulkWordImportResponse> ImportAdminWordsAsync(
        AdminBulkWordImportRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> AddAdminWordSenseAsync(
        Guid publicId,
        AdminAddWordSenseRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> AddAdminWordSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        AdminAddWordSenseTranslationRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> AddAdminWordSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        AdminAddWordSenseExampleRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> UpdateAdminWordSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid translationId,
        AdminUpdateWordSenseTranslationRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> DeleteAdminWordSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid translationId,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> UpdateAdminWordSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        AdminUpdateWordSenseExampleRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> DeleteAdminWordSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> AddAdminWordSenseExampleTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        AdminAddWordSenseExampleTranslationRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> UpdateAdminWordSenseExampleTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        Guid translationId,
        AdminUpdateWordSenseExampleTranslationRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> DeleteAdminWordSenseExampleTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        Guid translationId,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> AddAdminWordTopicAsync(
        Guid publicId,
        AdminAddWordTopicRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> DeleteAdminWordTopicAsync(
        Guid publicId,
        Guid topicId,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> AddAdminWordLabelAsync(
        Guid publicId,
        AdminAddWordLabelRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> DeleteAdminWordLabelAsync(
        Guid publicId,
        string kind,
        string key,
        CancellationToken cancellationToken);

    Task<AdminDraftWordsPageViewModel> GetAdminDraftWordsAsync(string? query, CancellationToken cancellationToken);

    Task<AdminHistoryPageViewModel> GetAdminHistoryAsync(string? statusFilter, CancellationToken cancellationToken);

    Task<AdminRollbackPageViewModel> GetAdminRollbackPreviewAsync(CancellationToken cancellationToken);

    Task<AdminTopicsPageViewModel> GetAdminTopicsAsync(CancellationToken cancellationToken);

    Task<AdminTopicItemViewModel?> GetAdminTopicAsync(Guid topicId, CancellationToken cancellationToken);

    Task<AdminTopicItemViewModel> CreateAdminTopicAsync(AdminSaveTopicRequest request, CancellationToken cancellationToken);

    Task<AdminTopicItemViewModel> UpdateAdminTopicAsync(Guid topicId, AdminSaveTopicRequest request, CancellationToken cancellationToken);

    Task<AdminTopicItemViewModel> MergeAdminTopicAsync(Guid topicId, AdminMergeTopicRequest request, CancellationToken cancellationToken);

    Task<AdminLabelsPageViewModel> GetAdminLabelsAsync(CancellationToken cancellationToken);

    Task<AdminLabelItemViewModel?> GetAdminLabelAsync(Guid labelId, CancellationToken cancellationToken);

    Task<AdminLabelItemViewModel> CreateAdminLabelAsync(AdminSaveLabelRequest request, CancellationToken cancellationToken);

    Task<AdminLabelItemViewModel> UpdateAdminLabelAsync(Guid labelId, AdminSaveLabelRequest request, CancellationToken cancellationToken);

    Task<AdminLabelItemViewModel> RenameAdminLabelAsync(Guid labelId, AdminSaveLabelRequest request, CancellationToken cancellationToken);

    Task<AdminLabelItemViewModel> MergeAdminLabelAsync(Guid labelId, AdminMergeLabelRequest request, CancellationToken cancellationToken);

    Task<AdminCollectionsPageViewModel> GetAdminCollectionsAsync(CancellationToken cancellationToken);

    Task<AdminCollectionDetailViewModel?> GetAdminCollectionAsync(Guid collectionId, CancellationToken cancellationToken);

    Task<AdminCollectionDetailViewModel> CreateAdminCollectionAsync(AdminSaveCollectionRequest request, CancellationToken cancellationToken);

    Task<AdminCollectionDetailViewModel> UpdateAdminCollectionAsync(Guid collectionId, AdminSaveCollectionRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteAdminCollectionAsync(Guid collectionId, CancellationToken cancellationToken);

    Task<AdminCollectionDetailViewModel> AddAdminCollectionWordAsync(Guid collectionId, AdminAddCollectionWordRequest request, CancellationToken cancellationToken);

    Task<AdminCollectionDetailViewModel> DeleteAdminCollectionWordAsync(Guid collectionId, Guid entryId, CancellationToken cancellationToken);

    Task<AdminBulkCollectionImportResponse> ImportAdminCollectionsAsync(AdminBulkCollectionImportRequest request, CancellationToken cancellationToken);

    Task<AdminDialoguesPageViewModel> GetAdminDialoguesAsync(CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel?> GetAdminDialogueAsync(Guid dialogueId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> CreateAdminDialogueAsync(AdminSaveDialogueRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> UpdateAdminDialogueAsync(Guid dialogueId, AdminSaveDialogueRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteAdminDialogueAsync(Guid dialogueId, CancellationToken cancellationToken);

    Task<AdminBulkDialogueImportResponse> ImportAdminDialoguesAsync(AdminBulkDialogueImportRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> AddAdminDialogueTurnAsync(Guid dialogueId, AdminAddDialogueTurnRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> DeleteAdminDialogueTurnAsync(Guid dialogueId, Guid turnId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> AddAdminDialoguePhraseAsync(Guid dialogueId, AdminAddDialoguePhraseRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> DeleteAdminDialoguePhraseAsync(Guid dialogueId, Guid phraseId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> AddAdminDialogueQuestionAsync(Guid dialogueId, AdminAddDialogueQuestionRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> DeleteAdminDialogueQuestionAsync(Guid dialogueId, Guid questionId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> AddAdminDialogueAnswerAsync(Guid dialogueId, Guid questionId, AdminAddDialogueAnswerRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> DeleteAdminDialogueAnswerAsync(Guid dialogueId, Guid questionId, Guid answerId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> AddAdminDialogueTurnTranslationAsync(Guid dialogueId, Guid turnId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> DeleteAdminDialogueTurnTranslationAsync(Guid dialogueId, Guid turnId, Guid translationId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> AddAdminDialoguePhraseTranslationAsync(Guid dialogueId, Guid phraseId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> DeleteAdminDialoguePhraseTranslationAsync(Guid dialogueId, Guid phraseId, Guid translationId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> AddAdminDialogueQuestionTranslationAsync(Guid dialogueId, Guid questionId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> DeleteAdminDialogueQuestionTranslationAsync(Guid dialogueId, Guid questionId, Guid translationId, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> AddAdminDialogueAnswerTranslationAsync(Guid dialogueId, Guid questionId, Guid answerId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken);

    Task<AdminDialogueDetailViewModel> DeleteAdminDialogueAnswerTranslationAsync(Guid dialogueId, Guid questionId, Guid answerId, Guid translationId, CancellationToken cancellationToken);

    Task<ConversationEventDetailModel> SaveAdminConversationEventAsync(
        AdminSaveConversationEventRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OrganizerManagedConversationEventModel>> GetAdminConversationEventsByOrganizerAsync(
        string organizerProfileSlug,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<OrganizerManagedConversationEventModel> SetAdminConversationEventPublicationStatusAsync(
        string slug,
        AdminSetConversationEventPublicationStatusRequest request,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<OrganizerProfileDetailModel> SaveAdminOrganizerProfileAsync(
        AdminSaveOrganizerProfileRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OrganizerClaimRequestModel>> GetAdminOrganizerClaimRequestsAsync(CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<OrganizerClaimRequestModel> SetAdminOrganizerClaimRequestStatusAsync(
        Guid claimRequestId,
        OrganizerClaimDecisionRequest request,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<IReadOnlyList<OrganizerProfileOwnerModel>> GetAdminOrganizerProfileOwnersAsync(CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<OrganizerProfileOwnerModel> AssignAdminOrganizerProfileOwnerAsync(
        AssignOrganizerProfileOwnerRequest request,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();
}

internal sealed class WebCatalogApiClient(
    HttpClient httpClient,
    IWebPerformanceTelemetryService telemetryService) : IWebCatalogApiClient
{
    private const string ActorEmailHeaderName = "X-DarwinLingua-Actor-Email";
    private const int MaxErrorResponseBodyLength = 500;

    public Task<IReadOnlyList<TopicListItemModel>> GetTopicsAsync(string uiLanguageCode, CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<TopicListItemModel>>(
            BuildPath("/api/catalog/topics", [new("uiLanguageCode", uiLanguageCode)]),
            cancellationToken);

    public Task<IReadOnlyList<WordCollectionListItemModel>> GetCollectionsAsync(
        string meaningLanguageCode,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<WordCollectionListItemModel>>(
            BuildPath("/api/catalog/collections", [new("meaningLanguageCode", meaningLanguageCode)]),
            cancellationToken);

    public Task<WordCollectionDetailModel?> GetCollectionBySlugAsync(
        string slug,
        string meaningLanguageCode,
        CancellationToken cancellationToken) =>
        GetAsync<WordCollectionDetailModel>(
            BuildPath($"/api/catalog/collections/{Uri.EscapeDataString(slug)}", [new("meaningLanguageCode", meaningLanguageCode)]),
            cancellationToken);

    public Task<IReadOnlyList<DialogueLessonListItemModel>> GetDialoguesAsync(CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<DialogueLessonListItemModel>>(
            "/api/catalog/dialogues",
            cancellationToken);

    public Task<DialogueLessonDetailModel?> GetDialogueBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        GetAsync<DialogueLessonDetailModel>(
            BuildPath(
                $"/api/catalog/dialogues/{Uri.EscapeDataString(slug)}",
                [
                    new("primaryMeaningLanguageCode", primaryMeaningLanguageCode),
                    new("secondaryMeaningLanguageCode", secondaryMeaningLanguageCode)
                ]),
            cancellationToken);

    public Task<IReadOnlyList<TalkTopicListItemModel>> GetTalkTopicsAsync(
        TalkTopicListFilterModel filter,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<TalkTopicListItemModel>>(
            BuildPath(
                "/api/catalog/talk-topics",
                [
                    new("cefrLevel", filter.CefrLevel),
                    new("category", filter.Category),
                    new("topicKey", filter.TopicKey),
                    new("contentType", filter.ContentType),
                    new("speakingGoal", filter.SpeakingGoal),
                    new("isSensitive", filter.IsSensitive?.ToString())
                ]),
            cancellationToken);

    public Task<TalkTopicDetailModel?> GetTalkTopicBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        GetAsync<TalkTopicDetailModel>(
            BuildPath(
                $"/api/catalog/talk-topics/{Uri.EscapeDataString(slug)}",
                [
                    new("primaryMeaningLanguageCode", primaryMeaningLanguageCode),
                    new("secondaryMeaningLanguageCode", secondaryMeaningLanguageCode)
                ]),
            cancellationToken);

    public Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetConversationStarterPacksAsync(
        ConversationStarterListFilterModel filter,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<ConversationStarterPackListItemModel>>(
            BuildPath(
                "/api/catalog/conversation-starters",
                [
                    new("cefrLevel", filter.CefrLevel),
                    new("situation", filter.Situation),
                    new("tone", filter.Tone),
                    new("conversationGoal", filter.ConversationGoal),
                    new("topicKey", filter.TopicKey)
                ]),
            cancellationToken);

    public Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetConversationStarterPacksForDialogueAsync(
        string dialogueSlug,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<ConversationStarterPackListItemModel>>(
            $"/api/catalog/dialogues/{Uri.EscapeDataString(dialogueSlug)}/conversation-starters",
            cancellationToken);

    public Task<ConversationStarterPackDetailModel?> GetConversationStarterPackBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        GetAsync<ConversationStarterPackDetailModel>(
            BuildPath(
                $"/api/catalog/conversation-starters/{Uri.EscapeDataString(slug)}",
                [
                    new("primaryMeaningLanguageCode", primaryMeaningLanguageCode),
                    new("secondaryMeaningLanguageCode", secondaryMeaningLanguageCode)
                ]),
            cancellationToken);

    public Task<IReadOnlyList<EventPreparationPackListItemModel>> GetEventPreparationPacksForDialogueAsync(
        string dialogueSlug,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<EventPreparationPackListItemModel>>(
            $"/api/catalog/dialogues/{Uri.EscapeDataString(dialogueSlug)}/event-preparation-packs",
            cancellationToken);

    public Task<EventPreparationPackDetailModel?> GetEventPreparationPackBySlugAsync(
        string slug,
        CancellationToken cancellationToken) =>
        GetAsync<EventPreparationPackDetailModel>(
            $"/api/catalog/event-preparation-packs/{Uri.EscapeDataString(slug)}",
            cancellationToken);

    public Task<IReadOnlyList<ConversationEventListItemModel>> GetConversationEventsAsync(
        ConversationEventListFilterModel filter,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<ConversationEventListItemModel>>(
            BuildPath(
                "/api/catalog/conversation-events",
                [
                    new("city", filter.City),
                    new("cefrLevel", filter.CefrLevel),
                    new("helperLanguageCode", filter.HelperLanguageCode),
                    new("isOnline", filter.IsOnline?.ToString()),
                    new("priceType", filter.PriceType),
                    new("category", filter.Category)
                ]),
            cancellationToken);

    public Task<ConversationEventDetailModel?> GetConversationEventBySlugAsync(
        string slug,
        CancellationToken cancellationToken) =>
        GetAsync<ConversationEventDetailModel>(
            $"/api/catalog/conversation-events/{Uri.EscapeDataString(slug)}",
            cancellationToken);

    public Task<EventRsvpSummaryModel> GetEventRsvpSummaryAsync(
        string eventSlug,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<EventRsvpSummaryModel>(
            $"/api/catalog/conversation-events/{Uri.EscapeDataString(eventSlug)}/rsvp-summary",
            cancellationToken);

    public Task<EventRsvpModel> SubmitEventRsvpAsync(
        string eventSlug,
        SubmitEventRsvpRequest request,
        CancellationToken cancellationToken) =>
        PostRequiredAsync<SubmitEventRsvpRequest, EventRsvpModel>(
            $"/api/catalog/conversation-events/{Uri.EscapeDataString(eventSlug)}/rsvps",
            request,
            cancellationToken);

    public Task<IReadOnlyList<EventRsvpModel>> GetAdminEventRsvpsAsync(
        string eventSlug,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<EventRsvpModel>>(
            $"/api/admin/catalog/conversation-events/{Uri.EscapeDataString(eventSlug)}/rsvps",
            cancellationToken);

    public Task<EventRsvpModel> SetAdminEventRsvpStatusAsync(
        string eventSlug,
        Guid rsvpId,
        AdminSetEventRsvpStatusRequest request,
        CancellationToken cancellationToken) =>
        PostRequiredAsync<AdminSetEventRsvpStatusRequest, EventRsvpModel>(
            $"/api/admin/catalog/conversation-events/{Uri.EscapeDataString(eventSlug)}/rsvps/{rsvpId}/status",
            request,
            cancellationToken);

    public Task<IReadOnlyList<OrganizerProfileListItemModel>> GetOrganizerProfilesAsync(CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<OrganizerProfileListItemModel>>(
            "/api/catalog/organizer-profiles",
            cancellationToken);

    public Task<OrganizerProfileDetailModel?> GetOrganizerProfileBySlugAsync(
        string slug,
        CancellationToken cancellationToken) =>
        GetAsync<OrganizerProfileDetailModel>(
            $"/api/catalog/organizer-profiles/{Uri.EscapeDataString(slug)}",
            cancellationToken);

    public Task<OrganizerClaimRequestModel> SubmitOrganizerClaimRequestAsync(
        string organizerProfileSlug,
        SubmitOrganizerClaimRequest request,
        CancellationToken cancellationToken) =>
        PostRequiredAsync<SubmitOrganizerClaimRequest, OrganizerClaimRequestModel>(
            $"/api/catalog/organizer-profiles/{Uri.EscapeDataString(organizerProfileSlug)}/claim",
            request,
            cancellationToken);

    public Task<IReadOnlyList<OrganizerProfileOwnerModel>> GetOrganizerProfileOwnersByEmailAsync(
        string ownerEmail,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<OrganizerProfileOwnerModel>>(
            "/api/catalog/organizer-profile-owners/by-email",
            ownerEmail,
            cancellationToken);

    public Task<LearnerConversationProfileModel?> GetLearnerConversationProfileAsync(
        string ownerEmail,
        CancellationToken cancellationToken) =>
        GetAsync<LearnerConversationProfileModel>(
            "/api/catalog/learner-conversation-profiles/me",
            ownerEmail,
            cancellationToken);

    public Task<IReadOnlyList<LearnerConversationProfilePublicModel>> GetPublicLearnerConversationProfilesAsync(
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<LearnerConversationProfilePublicModel>>(
            "/api/catalog/learner-conversation-profiles/public",
            cancellationToken);

    public Task<LearnerConversationProfileModel> SaveLearnerConversationProfileAsync(
        string ownerEmail,
        SaveLearnerConversationProfileRequest request,
        CancellationToken cancellationToken) =>
        PostRequiredAsync<SaveLearnerConversationProfileRequest, LearnerConversationProfileModel>(
            "/api/catalog/learner-conversation-profiles/me",
            ownerEmail,
            request,
            cancellationToken);

    public Task<LearnerConversationProfileModel> SetLearnerConversationProfileEnabledAsync(
        string ownerEmail,
        LearnerConversationProfileVisibilityRequest request,
        CancellationToken cancellationToken) =>
        PostRequiredAsync<LearnerConversationProfileVisibilityRequest, LearnerConversationProfileModel>(
            "/api/catalog/learner-conversation-profiles/me/enabled",
            ownerEmail,
            request,
            cancellationToken);

    public async Task DeleteLearnerConversationProfileAsync(
        string ownerEmail,
        CancellationToken cancellationToken)
    {
        const string relativeUri = "/api/catalog/learner-conversation-profiles/me";
        using HttpRequestMessage request = new(HttpMethod.Delete, relativeUri);
        AddActorEmailHeader(request, ownerEmail);
        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, relativeUri, cancellationToken).ConfigureAwait(false);
    }

    public Task<IReadOnlyList<PartnerMatchProfileModel>> SearchPartnerMatchesAsync(
        string ownerEmail,
        PartnerMatchSearchRequest request,
        CancellationToken cancellationToken) =>
        PostRequiredAsync<PartnerMatchSearchRequest, IReadOnlyList<PartnerMatchProfileModel>>(
            "/api/catalog/partner-matches/search",
            ownerEmail,
            request,
            cancellationToken);

    public Task<PartnerRequestModel> SubmitPartnerRequestAsync(
        string ownerEmail,
        SubmitPartnerRequestRequest request,
        CancellationToken cancellationToken) =>
        PostRequiredAsync<SubmitPartnerRequestRequest, PartnerRequestModel>(
            "/api/catalog/partner-requests",
            ownerEmail,
            request,
            cancellationToken);

    public Task<IReadOnlyList<PartnerRequestModel>> GetPartnerRequestsAsync(
        string ownerEmail,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<PartnerRequestModel>>(
            "/api/catalog/partner-requests",
            ownerEmail,
            cancellationToken);

    public Task<PartnerRequestModel> UpdatePartnerRequestStateAsync(
        string ownerEmail,
        Guid requestId,
        PartnerRequestStateUpdateRequest request,
        CancellationToken cancellationToken) =>
        PostRequiredAsync<PartnerRequestStateUpdateRequest, PartnerRequestModel>(
            $"/api/catalog/partner-requests/{requestId:D}/state",
            ownerEmail,
            request,
            cancellationToken);

    public Task<UserReportModel> SubmitUserReportAsync(
        string reporterEmail,
        SubmitUserReportRequest request,
        CancellationToken cancellationToken) =>
        PostRequiredAsync<SubmitUserReportRequest, UserReportModel>(
            "/api/catalog/moderation/reports",
            reporterEmail,
            request,
            cancellationToken);

    public Task<UserBlockModel> BlockUserAsync(
        string blockerEmail,
        BlockUserRequest request,
        CancellationToken cancellationToken) =>
        PostRequiredAsync<BlockUserRequest, UserBlockModel>(
            "/api/catalog/moderation/blocks",
            blockerEmail,
            request,
            cancellationToken);

    public Task<IReadOnlyList<UserReportModel>> GetAdminUserReportsAsync(
        string? status,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<UserReportModel>>(
            BuildPath("/api/admin/catalog/moderation/reports", [new("status", status)]),
            cancellationToken);

    public Task<IReadOnlyList<ModerationDecisionAuditModel>> GetAdminModerationDecisionAuditsAsync(
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<ModerationDecisionAuditModel>>(
            "/api/admin/catalog/moderation/audits",
            cancellationToken);

    public Task<UserReportModel> DecideAdminUserReportAsync(
        Guid reportId,
        ModerationDecisionRequest request,
        CancellationToken cancellationToken) =>
        PostRequiredAsync<ModerationDecisionRequest, UserReportModel>(
            $"/api/admin/catalog/moderation/reports/{reportId:D}/decision",
            request,
            cancellationToken);

    public Task<IReadOnlyList<WordListItemModel>> GetWordsByTopicPageAsync(
        string topicKey,
        string meaningLanguageCode,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<WordListItemModel>>(
            BuildPath(
                $"/api/catalog/words/topic/{Uri.EscapeDataString(topicKey)}",
                [
                    new("meaningLanguageCode", meaningLanguageCode),
                    new("skip", skip.ToString()),
                    new("take", take.ToString())
                ]),
            cancellationToken);

    public Task<IReadOnlyList<WordListItemModel>> GetWordsByCefrPageAsync(
        string cefrLevel,
        string meaningLanguageCode,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<WordListItemModel>>(
            BuildPath(
                $"/api/catalog/words/cefr/{Uri.EscapeDataString(cefrLevel)}",
                [
                    new("meaningLanguageCode", meaningLanguageCode),
                    new("skip", skip.ToString()),
                    new("take", take.ToString())
                ]),
            cancellationToken);

    public Task<IReadOnlyList<WordListItemModel>> SearchWordsAsync(
        string query,
        string meaningLanguageCode,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<WordListItemModel>>(
            BuildPath(
                "/api/catalog/words/search",
                [
                    new("q", query),
                    new("meaningLanguageCode", meaningLanguageCode)
                ]),
            cancellationToken);

    public Task<WordDetailModel?> GetWordDetailsAsync(
        Guid publicId,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        string uiLanguageCode,
        CancellationToken cancellationToken) =>
        GetAsync<WordDetailModel>(
            BuildPath(
                $"/api/catalog/words/{publicId:D}",
                [
                    new("primaryMeaningLanguageCode", primaryMeaningLanguageCode),
                    new("secondaryMeaningLanguageCode", secondaryMeaningLanguageCode),
                    new("uiLanguageCode", uiLanguageCode)
                ]),
            cancellationToken);

    public Task<WordDetailModel?> GetWordDetailsBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        string uiLanguageCode,
        CancellationToken cancellationToken) =>
        GetAsync<WordDetailModel>(
            BuildPath(
                $"/api/catalog/words/by-slug/{Uri.EscapeDataString(slug)}",
                [
                    new("primaryMeaningLanguageCode", primaryMeaningLanguageCode),
                    new("secondaryMeaningLanguageCode", secondaryMeaningLanguageCode),
                    new("uiLanguageCode", uiLanguageCode)
                ]),
            cancellationToken);

    public Task<IReadOnlyList<WordListItemModel>> GetWordsByIdsAsync(
        IReadOnlyList<Guid> wordIds,
        string meaningLanguageCode,
        CancellationToken cancellationToken) =>
        PostRequiredAsync<CatalogWordLookupRequest, IReadOnlyList<WordListItemModel>>(
            "/api/catalog/words/by-ids",
            new CatalogWordLookupRequest(wordIds, meaningLanguageCode),
            cancellationToken);

    public async Task<AdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken cancellationToken)
    {
        AdminCatalogDashboardResponse response = await GetRequiredAsync<AdminCatalogDashboardResponse>(
            "/api/admin/catalog/dashboard",
            cancellationToken).ConfigureAwait(false);

        return new AdminDashboardViewModel(
            response.ActiveWordCount,
            response.DraftWordCount,
            response.TotalTopicCount,
            response.ImportedPackageCount,
            response.FailedPackageCount,
            response.LastImportAtUtc);
    }

    public Task<AdminSystemReportResponse> GetAdminSystemReportAsync(CancellationToken cancellationToken) =>
        GetRequiredAsync<AdminSystemReportResponse>(
            "/api/admin/catalog/system-report",
            cancellationToken);

    public async Task<AdminImportsPageViewModel> GetAdminImportsAsync(string? statusFilter, CancellationToken cancellationToken)
    {
        AdminCatalogImportsResponse response = await GetRequiredAsync<AdminCatalogImportsResponse>(
            BuildPath("/api/admin/catalog/imports", [new("status", statusFilter)]),
            cancellationToken).ConfigureAwait(false);

        return new AdminImportsPageViewModel(
            response.StatusFilter,
            response.Packages
                .Select(package => new AdminContentPackageListItemViewModel(
                    package.PackageId,
                    package.PackageVersion,
                    package.PackageName,
                    package.SourceType,
                    package.Status,
                    package.TotalEntries,
                    package.InsertedEntries,
                    package.InvalidEntries,
                    package.WarningCount,
                    package.CreatedAtUtc))
                .ToArray());
    }

    public async Task<AdminWordsPageViewModel> GetAdminWordsAsync(
        string? query,
        string? statusFilter,
        string? sort,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        AdminCatalogWordsResponse response = await GetRequiredAsync<AdminCatalogWordsResponse>(
            BuildPath(
                "/api/admin/catalog/words",
                [
                    new("q", query),
                    new("status", statusFilter),
                    new("sort", sort),
                    new("skip", skip.ToString()),
                    new("take", take.ToString())
                ]),
            cancellationToken).ConfigureAwait(false);

        return new AdminWordsPageViewModel(
            response.Query,
            response.StatusFilter,
            response.Sort,
            response.Skip,
            response.Take,
            response.TotalCount,
            response.Words
                .Select(word => new AdminWordListItemViewModel(
                    word.PublicId,
                    word.Lemma,
                    word.Article,
                    word.PartOfSpeech,
                    word.CefrLevel,
                    word.PublicationStatus,
                    word.ContentSourceType,
                    word.SenseCount,
                    word.TopicCount,
                    word.UpdatedAtUtc))
                .ToArray());
    }

    public async Task<AdminWordDetailViewModel?> GetAdminWordAsync(Guid publicId, CancellationToken cancellationToken)
    {
        AdminCatalogWordDetailResponse? response = await GetAsync<AdminCatalogWordDetailResponse>(
            $"/api/admin/catalog/words/{publicId:D}",
            cancellationToken).ConfigureAwait(false);

        return response is null
            ? null
            : new AdminWordDetailViewModel(
                response.PublicId,
                response.Lemma,
                response.NormalizedLemma,
                response.LanguageCode,
                response.Article,
                response.PluralForm,
                response.InfinitiveForm,
                response.PronunciationIpa,
                response.SyllableBreak,
                response.PartOfSpeech,
                response.CefrLevel,
                response.PublicationStatus,
                response.ContentSourceType,
                response.SourceReference,
                response.CreatedAtUtc,
                response.UpdatedAtUtc,
                response.LexicalForms
                    .Select(form => new AdminWordLexicalFormViewModel(
                        form.PartOfSpeech,
                        form.Article,
                        form.PluralForm,
                        form.InfinitiveForm,
                        form.IsPrimary,
                        form.SortOrder))
                    .ToArray(),
                response.Senses
                    .Select(sense => new AdminWordSenseViewModel(
                        sense.SenseId,
                        sense.SenseOrder,
                        sense.IsPrimarySense,
                        sense.PublicationStatus,
                        sense.ShortDefinitionDe,
                        sense.ShortGloss,
                        sense.Translations
                            .Select(translation => new AdminWordTranslationViewModel(
                                translation.TranslationId,
                                translation.LanguageCode,
                                translation.TranslationText,
                                translation.IsPrimary))
                            .ToArray(),
                        sense.Examples
                            .Select(example => new AdminWordExampleViewModel(
                                example.ExampleId,
                                example.SentenceOrder,
                                example.GermanText,
                                example.IsPrimaryExample,
                                example.Translations
                                    .Select(translation => new AdminWordExampleTranslationViewModel(
                                        translation.TranslationId,
                                        translation.LanguageCode,
                                        translation.TranslationText))
                                    .ToArray()))
                            .ToArray()))
                    .ToArray(),
                response.Topics
                    .Select(topic => new AdminWordTopicViewModel(topic.TopicId, topic.Key, topic.IsPrimaryTopic))
                    .ToArray(),
                response.Labels
                    .Select(label => new AdminWordLabelViewModel(label.Kind, label.Key, label.DisplayName, label.SortOrder))
                    .ToArray(),
                response.GrammarNotes
                    .Select(note => new AdminWordTextItemViewModel(note.Text, note.SortOrder))
                    .ToArray(),
                response.Collocations
                    .Select(collocation => new AdminWordCollocationViewModel(
                        collocation.Text,
                        collocation.Meaning,
                        collocation.SortOrder))
                    .ToArray());
    }

    public async Task<AdminWordDetailViewModel> UpdateAdminWordMetadataAsync(
        Guid publicId,
        AdminUpdateWordMetadataRequest request,
        CancellationToken cancellationToken)
    {
        AdminCatalogWordDetailResponse response = await PostRequiredAsync<AdminUpdateWordMetadataRequest, AdminCatalogWordDetailResponse>(
            $"/api/admin/catalog/words/{publicId:D}/metadata",
            request,
            cancellationToken).ConfigureAwait(false);

        return new AdminWordDetailViewModel(
            response.PublicId,
            response.Lemma,
            response.NormalizedLemma,
            response.LanguageCode,
            response.Article,
            response.PluralForm,
            response.InfinitiveForm,
            response.PronunciationIpa,
            response.SyllableBreak,
            response.PartOfSpeech,
            response.CefrLevel,
            response.PublicationStatus,
            response.ContentSourceType,
            response.SourceReference,
            response.CreatedAtUtc,
            response.UpdatedAtUtc,
            response.LexicalForms
                .Select(form => new AdminWordLexicalFormViewModel(
                    form.PartOfSpeech,
                    form.Article,
                    form.PluralForm,
                    form.InfinitiveForm,
                    form.IsPrimary,
                    form.SortOrder))
                .ToArray(),
            response.Senses
                .Select(sense => new AdminWordSenseViewModel(
                    sense.SenseId,
                    sense.SenseOrder,
                    sense.IsPrimarySense,
                    sense.PublicationStatus,
                    sense.ShortDefinitionDe,
                    sense.ShortGloss,
                    sense.Translations
                        .Select(translation => new AdminWordTranslationViewModel(
                            translation.TranslationId,
                            translation.LanguageCode,
                            translation.TranslationText,
                            translation.IsPrimary))
                        .ToArray(),
                    sense.Examples
                            .Select(example => new AdminWordExampleViewModel(
                                example.ExampleId,
                                example.SentenceOrder,
                                example.GermanText,
                                example.IsPrimaryExample,
                                example.Translations
                                    .Select(translation => new AdminWordExampleTranslationViewModel(
                                        translation.TranslationId,
                                        translation.LanguageCode,
                                        translation.TranslationText))
                                    .ToArray()))
                        .ToArray()))
                .ToArray(),
            response.Topics
                .Select(topic => new AdminWordTopicViewModel(topic.TopicId, topic.Key, topic.IsPrimaryTopic))
                .ToArray(),
            response.Labels
                .Select(label => new AdminWordLabelViewModel(label.Kind, label.Key, label.DisplayName, label.SortOrder))
                .ToArray(),
            response.GrammarNotes
                .Select(note => new AdminWordTextItemViewModel(note.Text, note.SortOrder))
                .ToArray(),
            response.Collocations
                .Select(collocation => new AdminWordCollocationViewModel(
                    collocation.Text,
                    collocation.Meaning,
                    collocation.SortOrder))
                .ToArray());
    }

    public async Task<AdminWordDetailViewModel> CreateAdminWordAsync(
        AdminUpdateWordMetadataRequest request,
        CancellationToken cancellationToken)
    {
        AdminCatalogWordDetailResponse response = await PostRequiredAsync<AdminUpdateWordMetadataRequest, AdminCatalogWordDetailResponse>(
            "/api/admin/catalog/words",
            request,
            cancellationToken).ConfigureAwait(false);

        return new AdminWordDetailViewModel(
            response.PublicId,
            response.Lemma,
            response.NormalizedLemma,
            response.LanguageCode,
            response.Article,
            response.PluralForm,
            response.InfinitiveForm,
            response.PronunciationIpa,
            response.SyllableBreak,
            response.PartOfSpeech,
            response.CefrLevel,
            response.PublicationStatus,
            response.ContentSourceType,
            response.SourceReference,
            response.CreatedAtUtc,
            response.UpdatedAtUtc,
            response.LexicalForms
                .Select(form => new AdminWordLexicalFormViewModel(
                    form.PartOfSpeech,
                    form.Article,
                    form.PluralForm,
                    form.InfinitiveForm,
                    form.IsPrimary,
                    form.SortOrder))
                .ToArray(),
            response.Senses
                .Select(sense => new AdminWordSenseViewModel(
                    sense.SenseId,
                    sense.SenseOrder,
                    sense.IsPrimarySense,
                    sense.PublicationStatus,
                    sense.ShortDefinitionDe,
                    sense.ShortGloss,
                    sense.Translations
                        .Select(translation => new AdminWordTranslationViewModel(
                            translation.TranslationId,
                            translation.LanguageCode,
                            translation.TranslationText,
                            translation.IsPrimary))
                        .ToArray(),
                    sense.Examples
                            .Select(example => new AdminWordExampleViewModel(
                                example.ExampleId,
                                example.SentenceOrder,
                                example.GermanText,
                                example.IsPrimaryExample,
                                example.Translations
                                    .Select(translation => new AdminWordExampleTranslationViewModel(
                                        translation.TranslationId,
                                        translation.LanguageCode,
                                        translation.TranslationText))
                                    .ToArray()))
                        .ToArray()))
                .ToArray(),
            response.Topics
                .Select(topic => new AdminWordTopicViewModel(topic.TopicId, topic.Key, topic.IsPrimaryTopic))
                .ToArray(),
            response.Labels
                .Select(label => new AdminWordLabelViewModel(label.Kind, label.Key, label.DisplayName, label.SortOrder))
                .ToArray(),
            response.GrammarNotes
                .Select(note => new AdminWordTextItemViewModel(note.Text, note.SortOrder))
                .ToArray(),
            response.Collocations
                .Select(collocation => new AdminWordCollocationViewModel(
                    collocation.Text,
                    collocation.Meaning,
                    collocation.SortOrder))
                .ToArray());
    }

    public Task<AdminBulkWordImportResponse> ImportAdminWordsAsync(
        AdminBulkWordImportRequest request,
        CancellationToken cancellationToken) =>
        PostRequiredAsync<AdminBulkWordImportRequest, AdminBulkWordImportResponse>(
            "/api/admin/catalog/words/import",
            request,
            cancellationToken);

    public async Task<AdminWordDetailViewModel> AddAdminWordSenseAsync(
        Guid publicId,
        AdminAddWordSenseRequest request,
        CancellationToken cancellationToken)
    {
        AdminCatalogWordDetailResponse response = await PostRequiredAsync<AdminAddWordSenseRequest, AdminCatalogWordDetailResponse>(
            $"/api/admin/catalog/words/{publicId:D}/senses",
            request,
            cancellationToken).ConfigureAwait(false);

        return new AdminWordDetailViewModel(
            response.PublicId,
            response.Lemma,
            response.NormalizedLemma,
            response.LanguageCode,
            response.Article,
            response.PluralForm,
            response.InfinitiveForm,
            response.PronunciationIpa,
            response.SyllableBreak,
            response.PartOfSpeech,
            response.CefrLevel,
            response.PublicationStatus,
            response.ContentSourceType,
            response.SourceReference,
            response.CreatedAtUtc,
            response.UpdatedAtUtc,
            response.LexicalForms
                .Select(form => new AdminWordLexicalFormViewModel(
                    form.PartOfSpeech,
                    form.Article,
                    form.PluralForm,
                    form.InfinitiveForm,
                    form.IsPrimary,
                    form.SortOrder))
                .ToArray(),
            response.Senses
                .Select(sense => new AdminWordSenseViewModel(
                    sense.SenseId,
                    sense.SenseOrder,
                    sense.IsPrimarySense,
                    sense.PublicationStatus,
                    sense.ShortDefinitionDe,
                    sense.ShortGloss,
                    sense.Translations
                        .Select(translation => new AdminWordTranslationViewModel(
                            translation.TranslationId,
                            translation.LanguageCode,
                            translation.TranslationText,
                            translation.IsPrimary))
                        .ToArray(),
                    sense.Examples
                            .Select(example => new AdminWordExampleViewModel(
                                example.ExampleId,
                                example.SentenceOrder,
                                example.GermanText,
                                example.IsPrimaryExample,
                                example.Translations
                                    .Select(translation => new AdminWordExampleTranslationViewModel(
                                        translation.TranslationId,
                                        translation.LanguageCode,
                                        translation.TranslationText))
                                    .ToArray()))
                        .ToArray()))
                .ToArray(),
            response.Topics
                .Select(topic => new AdminWordTopicViewModel(topic.TopicId, topic.Key, topic.IsPrimaryTopic))
                .ToArray(),
            response.Labels
                .Select(label => new AdminWordLabelViewModel(label.Kind, label.Key, label.DisplayName, label.SortOrder))
                .ToArray(),
            response.GrammarNotes
                .Select(note => new AdminWordTextItemViewModel(note.Text, note.SortOrder))
                .ToArray(),
            response.Collocations
                .Select(collocation => new AdminWordCollocationViewModel(
                    collocation.Text,
                    collocation.Meaning,
                    collocation.SortOrder))
                .ToArray());
    }

    public async Task<AdminWordDetailViewModel> AddAdminWordSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        AdminAddWordSenseTranslationRequest request,
        CancellationToken cancellationToken)
    {
        AdminCatalogWordDetailResponse response = await PostRequiredAsync<AdminAddWordSenseTranslationRequest, AdminCatalogWordDetailResponse>(
            $"/api/admin/catalog/words/{publicId:D}/senses/{senseId:D}/translations",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminWordDetail(response);
    }

    public async Task<AdminWordDetailViewModel> AddAdminWordSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        AdminAddWordSenseExampleRequest request,
        CancellationToken cancellationToken)
    {
        AdminCatalogWordDetailResponse response = await PostRequiredAsync<AdminAddWordSenseExampleRequest, AdminCatalogWordDetailResponse>(
            $"/api/admin/catalog/words/{publicId:D}/senses/{senseId:D}/examples",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminWordDetail(response);
    }

    public async Task<AdminWordDetailViewModel> UpdateAdminWordSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid translationId,
        AdminUpdateWordSenseTranslationRequest request,
        CancellationToken cancellationToken)
    {
        AdminCatalogWordDetailResponse response = await PostRequiredAsync<AdminUpdateWordSenseTranslationRequest, AdminCatalogWordDetailResponse>(
            $"/api/admin/catalog/words/{publicId:D}/senses/{senseId:D}/translations/{translationId:D}",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminWordDetail(response);
    }

    public async Task<AdminWordDetailViewModel> DeleteAdminWordSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid translationId,
        CancellationToken cancellationToken)
    {
        AdminCatalogWordDetailResponse response = await PostRequiredAsync<AdminEmptyRequest, AdminCatalogWordDetailResponse>(
            $"/api/admin/catalog/words/{publicId:D}/senses/{senseId:D}/translations/{translationId:D}/delete",
            new AdminEmptyRequest(),
            cancellationToken).ConfigureAwait(false);

        return MapAdminWordDetail(response);
    }

    public async Task<AdminWordDetailViewModel> UpdateAdminWordSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        AdminUpdateWordSenseExampleRequest request,
        CancellationToken cancellationToken)
    {
        AdminCatalogWordDetailResponse response = await PostRequiredAsync<AdminUpdateWordSenseExampleRequest, AdminCatalogWordDetailResponse>(
            $"/api/admin/catalog/words/{publicId:D}/senses/{senseId:D}/examples/{exampleId:D}",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminWordDetail(response);
    }

    public async Task<AdminWordDetailViewModel> DeleteAdminWordSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        CancellationToken cancellationToken)
    {
        AdminCatalogWordDetailResponse response = await PostRequiredAsync<AdminEmptyRequest, AdminCatalogWordDetailResponse>(
            $"/api/admin/catalog/words/{publicId:D}/senses/{senseId:D}/examples/{exampleId:D}/delete",
            new AdminEmptyRequest(),
            cancellationToken).ConfigureAwait(false);

        return MapAdminWordDetail(response);
    }

    public async Task<AdminWordDetailViewModel> AddAdminWordSenseExampleTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        AdminAddWordSenseExampleTranslationRequest request,
        CancellationToken cancellationToken)
    {
        AdminCatalogWordDetailResponse response = await PostRequiredAsync<AdminAddWordSenseExampleTranslationRequest, AdminCatalogWordDetailResponse>(
            $"/api/admin/catalog/words/{publicId:D}/senses/{senseId:D}/examples/{exampleId:D}/translations",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminWordDetail(response);
    }

    public async Task<AdminWordDetailViewModel> UpdateAdminWordSenseExampleTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        Guid translationId,
        AdminUpdateWordSenseExampleTranslationRequest request,
        CancellationToken cancellationToken)
    {
        AdminCatalogWordDetailResponse response = await PostRequiredAsync<AdminUpdateWordSenseExampleTranslationRequest, AdminCatalogWordDetailResponse>(
            $"/api/admin/catalog/words/{publicId:D}/senses/{senseId:D}/examples/{exampleId:D}/translations/{translationId:D}",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminWordDetail(response);
    }

    public async Task<AdminWordDetailViewModel> DeleteAdminWordSenseExampleTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        Guid translationId,
        CancellationToken cancellationToken)
    {
        AdminCatalogWordDetailResponse response = await PostRequiredAsync<AdminEmptyRequest, AdminCatalogWordDetailResponse>(
            $"/api/admin/catalog/words/{publicId:D}/senses/{senseId:D}/examples/{exampleId:D}/translations/{translationId:D}/delete",
            new AdminEmptyRequest(),
            cancellationToken).ConfigureAwait(false);

        return MapAdminWordDetail(response);
    }

    public async Task<AdminWordDetailViewModel> AddAdminWordTopicAsync(
        Guid publicId,
        AdminAddWordTopicRequest request,
        CancellationToken cancellationToken)
    {
        AdminCatalogWordDetailResponse response = await PostRequiredAsync<AdminAddWordTopicRequest, AdminCatalogWordDetailResponse>(
            $"/api/admin/catalog/words/{publicId:D}/topics",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminWordDetail(response);
    }

    public async Task<AdminWordDetailViewModel> DeleteAdminWordTopicAsync(
        Guid publicId,
        Guid topicId,
        CancellationToken cancellationToken)
    {
        AdminCatalogWordDetailResponse response = await PostRequiredAsync<AdminEmptyRequest, AdminCatalogWordDetailResponse>(
            $"/api/admin/catalog/words/{publicId:D}/topics/{topicId:D}/delete",
            new AdminEmptyRequest(),
            cancellationToken).ConfigureAwait(false);

        return MapAdminWordDetail(response);
    }

    public async Task<AdminWordDetailViewModel> AddAdminWordLabelAsync(
        Guid publicId,
        AdminAddWordLabelRequest request,
        CancellationToken cancellationToken)
    {
        AdminCatalogWordDetailResponse response = await PostRequiredAsync<AdminAddWordLabelRequest, AdminCatalogWordDetailResponse>(
            $"/api/admin/catalog/words/{publicId:D}/labels",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminWordDetail(response);
    }

    public async Task<AdminWordDetailViewModel> DeleteAdminWordLabelAsync(
        Guid publicId,
        string kind,
        string key,
        CancellationToken cancellationToken)
    {
        AdminCatalogWordDetailResponse response = await PostRequiredAsync<AdminEmptyRequest, AdminCatalogWordDetailResponse>(
            $"/api/admin/catalog/words/{publicId:D}/labels/{Uri.EscapeDataString(kind)}/{Uri.EscapeDataString(key)}/delete",
            new AdminEmptyRequest(),
            cancellationToken).ConfigureAwait(false);

        return MapAdminWordDetail(response);
    }

    private static AdminWordDetailViewModel MapAdminWordDetail(AdminCatalogWordDetailResponse response) =>
        new(
            response.PublicId,
            response.Lemma,
            response.NormalizedLemma,
            response.LanguageCode,
            response.Article,
            response.PluralForm,
            response.InfinitiveForm,
            response.PronunciationIpa,
            response.SyllableBreak,
            response.PartOfSpeech,
            response.CefrLevel,
            response.PublicationStatus,
            response.ContentSourceType,
            response.SourceReference,
            response.CreatedAtUtc,
            response.UpdatedAtUtc,
            response.LexicalForms
                .Select(form => new AdminWordLexicalFormViewModel(
                    form.PartOfSpeech,
                    form.Article,
                    form.PluralForm,
                    form.InfinitiveForm,
                    form.IsPrimary,
                    form.SortOrder))
                .ToArray(),
            response.Senses
                .Select(sense => new AdminWordSenseViewModel(
                    sense.SenseId,
                    sense.SenseOrder,
                    sense.IsPrimarySense,
                    sense.PublicationStatus,
                    sense.ShortDefinitionDe,
                    sense.ShortGloss,
                    sense.Translations
                        .Select(translation => new AdminWordTranslationViewModel(
                            translation.TranslationId,
                            translation.LanguageCode,
                            translation.TranslationText,
                            translation.IsPrimary))
                        .ToArray(),
                    sense.Examples
                            .Select(example => new AdminWordExampleViewModel(
                                example.ExampleId,
                                example.SentenceOrder,
                                example.GermanText,
                                example.IsPrimaryExample,
                                example.Translations
                                    .Select(translation => new AdminWordExampleTranslationViewModel(
                                        translation.TranslationId,
                                        translation.LanguageCode,
                                        translation.TranslationText))
                                    .ToArray()))
                        .ToArray()))
                .ToArray(),
            response.Topics
                .Select(topic => new AdminWordTopicViewModel(topic.TopicId, topic.Key, topic.IsPrimaryTopic))
                .ToArray(),
            response.Labels
                .Select(label => new AdminWordLabelViewModel(label.Kind, label.Key, label.DisplayName, label.SortOrder))
                .ToArray(),
            response.GrammarNotes
                .Select(note => new AdminWordTextItemViewModel(note.Text, note.SortOrder))
                .ToArray(),
            response.Collocations
                .Select(collocation => new AdminWordCollocationViewModel(
                    collocation.Text,
                    collocation.Meaning,
                    collocation.SortOrder))
                .ToArray());

    public async Task<AdminDraftWordsPageViewModel> GetAdminDraftWordsAsync(string? query, CancellationToken cancellationToken)
    {
        AdminCatalogDraftWordsResponse response = await GetRequiredAsync<AdminCatalogDraftWordsResponse>(
            BuildPath("/api/admin/catalog/draft-words", [new("q", query)]),
            cancellationToken).ConfigureAwait(false);

        return new AdminDraftWordsPageViewModel(
            response.Query,
            response.Words
                .Select(word => new AdminDraftWordListItemViewModel(
                    word.PublicId,
                    word.Lemma,
                    word.PartOfSpeech,
                    word.CefrLevel,
                    word.PublicationStatus))
                .ToArray());
    }

    public async Task<AdminHistoryPageViewModel> GetAdminHistoryAsync(string? statusFilter, CancellationToken cancellationToken)
    {
        AdminCatalogHistoryViewResponse response = await GetRequiredAsync<AdminCatalogHistoryViewResponse>(
            BuildPath("/api/admin/catalog/history-view", [new("status", statusFilter)]),
            cancellationToken).ConfigureAwait(false);

        return new AdminHistoryPageViewModel(
            response.StatusFilter,
            response.Items
                .Select(item => new AdminHistoryItemViewModel(
                    item.PackageId,
                    item.PackageVersion,
                    item.Status,
                    item.TotalEntries,
                    item.InsertedEntries,
                    item.InvalidEntries,
                    item.CreatedAtUtc))
                .ToArray());
    }

    public async Task<AdminRollbackPageViewModel> GetAdminRollbackPreviewAsync(CancellationToken cancellationToken)
    {
        AdminCatalogRollbackPreviewResponse response = await GetRequiredAsync<AdminCatalogRollbackPreviewResponse>(
            "/api/admin/catalog/rollback-preview",
            cancellationToken).ConfigureAwait(false);

        return new AdminRollbackPageViewModel(
            response.DraftWordCount,
            response.ImportedPackageCount,
            response.WarningMessage);
    }

    public async Task<AdminTopicsPageViewModel> GetAdminTopicsAsync(CancellationToken cancellationToken)
    {
        AdminTopicsResponse response = await GetRequiredAsync<AdminTopicsResponse>(
            "/api/admin/catalog/topics",
            cancellationToken).ConfigureAwait(false);

        return new AdminTopicsPageViewModel(response.Topics.Select(MapAdminTopic).ToArray());
    }

    public async Task<AdminTopicItemViewModel?> GetAdminTopicAsync(Guid topicId, CancellationToken cancellationToken)
    {
        AdminTopicItemResponse? response = await GetAsync<AdminTopicItemResponse>(
            $"/api/admin/catalog/topics/{topicId:D}",
            cancellationToken).ConfigureAwait(false);

        return response is null ? null : MapAdminTopic(response);
    }

    public async Task<AdminTopicItemViewModel> CreateAdminTopicAsync(AdminSaveTopicRequest request, CancellationToken cancellationToken)
    {
        AdminTopicItemResponse response = await PostRequiredAsync<AdminSaveTopicRequest, AdminTopicItemResponse>(
            "/api/admin/catalog/topics",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminTopic(response);
    }

    public async Task<AdminTopicItemViewModel> UpdateAdminTopicAsync(Guid topicId, AdminSaveTopicRequest request, CancellationToken cancellationToken)
    {
        AdminTopicItemResponse response = await PostRequiredAsync<AdminSaveTopicRequest, AdminTopicItemResponse>(
            $"/api/admin/catalog/topics/{topicId:D}",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminTopic(response);
    }

    public async Task<AdminTopicItemViewModel> MergeAdminTopicAsync(Guid topicId, AdminMergeTopicRequest request, CancellationToken cancellationToken)
    {
        AdminTopicItemResponse response = await PostRequiredAsync<AdminMergeTopicRequest, AdminTopicItemResponse>(
            $"/api/admin/catalog/topics/{topicId:D}/merge",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminTopic(response);
    }

    public async Task<AdminLabelsPageViewModel> GetAdminLabelsAsync(CancellationToken cancellationToken)
    {
        AdminLabelsResponse response = await GetRequiredAsync<AdminLabelsResponse>(
            "/api/admin/catalog/labels",
            cancellationToken).ConfigureAwait(false);

        return new AdminLabelsPageViewModel(response.Labels
            .Select(MapAdminLabel)
            .ToArray());
    }

    public async Task<AdminLabelItemViewModel?> GetAdminLabelAsync(Guid labelId, CancellationToken cancellationToken)
    {
        AdminLabelItemResponse? response = await GetAsync<AdminLabelItemResponse>(
            $"/api/admin/catalog/labels/{labelId:D}",
            cancellationToken).ConfigureAwait(false);

        return response is null ? null : MapAdminLabel(response);
    }

    public async Task<AdminLabelItemViewModel> CreateAdminLabelAsync(AdminSaveLabelRequest request, CancellationToken cancellationToken)
    {
        AdminLabelItemResponse response = await PostRequiredAsync<AdminSaveLabelRequest, AdminLabelItemResponse>(
            "/api/admin/catalog/labels",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminLabel(response);
    }

    public async Task<AdminLabelItemViewModel> UpdateAdminLabelAsync(Guid labelId, AdminSaveLabelRequest request, CancellationToken cancellationToken)
    {
        AdminLabelItemResponse response = await PostRequiredAsync<AdminSaveLabelRequest, AdminLabelItemResponse>(
            $"/api/admin/catalog/labels/{labelId:D}",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminLabel(response);
    }

    public async Task<AdminLabelItemViewModel> RenameAdminLabelAsync(Guid labelId, AdminSaveLabelRequest request, CancellationToken cancellationToken)
    {
        AdminLabelItemResponse response = await PostRequiredAsync<AdminSaveLabelRequest, AdminLabelItemResponse>(
            $"/api/admin/catalog/labels/{labelId:D}/rename",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminLabel(response);
    }

    public async Task<AdminLabelItemViewModel> MergeAdminLabelAsync(Guid labelId, AdminMergeLabelRequest request, CancellationToken cancellationToken)
    {
        AdminLabelItemResponse response = await PostRequiredAsync<AdminMergeLabelRequest, AdminLabelItemResponse>(
            $"/api/admin/catalog/labels/{labelId:D}/merge",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminLabel(response);
    }

    public async Task<AdminCollectionsPageViewModel> GetAdminCollectionsAsync(CancellationToken cancellationToken)
    {
        AdminCollectionsResponse response = await GetRequiredAsync<AdminCollectionsResponse>(
            "/api/admin/catalog/collections",
            cancellationToken).ConfigureAwait(false);

        return new AdminCollectionsPageViewModel(response.Collections.Select(MapAdminCollectionItem).ToArray());
    }

    public async Task<AdminCollectionDetailViewModel?> GetAdminCollectionAsync(Guid collectionId, CancellationToken cancellationToken)
    {
        AdminCollectionDetailResponse? response = await GetAsync<AdminCollectionDetailResponse>(
            $"/api/admin/catalog/collections/{collectionId:D}",
            cancellationToken).ConfigureAwait(false);

        return response is null ? null : MapAdminCollectionDetail(response);
    }

    public async Task<AdminCollectionDetailViewModel> CreateAdminCollectionAsync(AdminSaveCollectionRequest request, CancellationToken cancellationToken)
    {
        AdminCollectionDetailResponse response = await PostRequiredAsync<AdminSaveCollectionRequest, AdminCollectionDetailResponse>(
            "/api/admin/catalog/collections",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminCollectionDetail(response);
    }

    public async Task<AdminCollectionDetailViewModel> UpdateAdminCollectionAsync(Guid collectionId, AdminSaveCollectionRequest request, CancellationToken cancellationToken)
    {
        AdminCollectionDetailResponse response = await PostRequiredAsync<AdminSaveCollectionRequest, AdminCollectionDetailResponse>(
            $"/api/admin/catalog/collections/{collectionId:D}",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminCollectionDetail(response);
    }

    public async Task<bool> DeleteAdminCollectionAsync(Guid collectionId, CancellationToken cancellationToken)
    {
        bool response = await PostRequiredAsync<AdminEmptyRequest, bool>(
            $"/api/admin/catalog/collections/{collectionId:D}/delete",
            new AdminEmptyRequest(),
            cancellationToken).ConfigureAwait(false);

        return response;
    }

    public async Task<AdminCollectionDetailViewModel> AddAdminCollectionWordAsync(Guid collectionId, AdminAddCollectionWordRequest request, CancellationToken cancellationToken)
    {
        AdminCollectionDetailResponse response = await PostRequiredAsync<AdminAddCollectionWordRequest, AdminCollectionDetailResponse>(
            $"/api/admin/catalog/collections/{collectionId:D}/words",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminCollectionDetail(response);
    }

    public async Task<AdminCollectionDetailViewModel> DeleteAdminCollectionWordAsync(Guid collectionId, Guid entryId, CancellationToken cancellationToken)
    {
        AdminCollectionDetailResponse response = await PostRequiredAsync<AdminEmptyRequest, AdminCollectionDetailResponse>(
            $"/api/admin/catalog/collections/{collectionId:D}/words/{entryId:D}/delete",
            new AdminEmptyRequest(),
            cancellationToken).ConfigureAwait(false);

        return MapAdminCollectionDetail(response);
    }

    public Task<AdminBulkCollectionImportResponse> ImportAdminCollectionsAsync(AdminBulkCollectionImportRequest request, CancellationToken cancellationToken) =>
        PostRequiredAsync<AdminBulkCollectionImportRequest, AdminBulkCollectionImportResponse>(
            "/api/admin/catalog/collections/import",
            request,
            cancellationToken);

    public async Task<AdminDialoguesPageViewModel> GetAdminDialoguesAsync(CancellationToken cancellationToken)
    {
        AdminDialoguesResponse response = await GetRequiredAsync<AdminDialoguesResponse>(
            "/api/admin/catalog/dialogues",
            cancellationToken).ConfigureAwait(false);

        return new AdminDialoguesPageViewModel(response.Dialogues.Select(MapAdminDialogueItem).ToArray());
    }

    public async Task<AdminDialogueDetailViewModel?> GetAdminDialogueAsync(Guid dialogueId, CancellationToken cancellationToken)
    {
        AdminDialogueDetailResponse? response = await GetAsync<AdminDialogueDetailResponse>(
            $"/api/admin/catalog/dialogues/{dialogueId:D}",
            cancellationToken).ConfigureAwait(false);

        return response is null ? null : MapAdminDialogueDetail(response);
    }

    public async Task<AdminDialogueDetailViewModel> CreateAdminDialogueAsync(AdminSaveDialogueRequest request, CancellationToken cancellationToken)
    {
        AdminDialogueDetailResponse response = await PostRequiredAsync<AdminSaveDialogueRequest, AdminDialogueDetailResponse>(
            "/api/admin/catalog/dialogues",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminDialogueDetail(response);
    }

    public async Task<AdminDialogueDetailViewModel> UpdateAdminDialogueAsync(Guid dialogueId, AdminSaveDialogueRequest request, CancellationToken cancellationToken)
    {
        AdminDialogueDetailResponse response = await PostRequiredAsync<AdminSaveDialogueRequest, AdminDialogueDetailResponse>(
            $"/api/admin/catalog/dialogues/{dialogueId:D}",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminDialogueDetail(response);
    }

    public async Task<bool> DeleteAdminDialogueAsync(Guid dialogueId, CancellationToken cancellationToken)
    {
        return await PostRequiredAsync<AdminEmptyRequest, bool>(
            $"/api/admin/catalog/dialogues/{dialogueId:D}/delete",
            new AdminEmptyRequest(),
            cancellationToken).ConfigureAwait(false);
    }

    public Task<AdminBulkDialogueImportResponse> ImportAdminDialoguesAsync(AdminBulkDialogueImportRequest request, CancellationToken cancellationToken) =>
        PostRequiredAsync<AdminBulkDialogueImportRequest, AdminBulkDialogueImportResponse>(
            "/api/admin/catalog/dialogues/import",
            request,
            cancellationToken);

    public async Task<AdminDialogueDetailViewModel> AddAdminDialogueTurnAsync(Guid dialogueId, AdminAddDialogueTurnRequest request, CancellationToken cancellationToken)
    {
        AdminDialogueDetailResponse response = await PostRequiredAsync<AdminAddDialogueTurnRequest, AdminDialogueDetailResponse>(
            $"/api/admin/catalog/dialogues/{dialogueId:D}/dialogue-turns",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminDialogueDetail(response);
    }

    public async Task<AdminDialogueDetailViewModel> DeleteAdminDialogueTurnAsync(Guid dialogueId, Guid turnId, CancellationToken cancellationToken)
    {
        AdminDialogueDetailResponse response = await PostRequiredAsync<AdminEmptyRequest, AdminDialogueDetailResponse>(
            $"/api/admin/catalog/dialogues/{dialogueId:D}/dialogue-turns/{turnId:D}/delete",
            new AdminEmptyRequest(),
            cancellationToken).ConfigureAwait(false);

        return MapAdminDialogueDetail(response);
    }

    public async Task<AdminDialogueDetailViewModel> AddAdminDialoguePhraseAsync(Guid dialogueId, AdminAddDialoguePhraseRequest request, CancellationToken cancellationToken)
    {
        AdminDialogueDetailResponse response = await PostRequiredAsync<AdminAddDialoguePhraseRequest, AdminDialogueDetailResponse>(
            $"/api/admin/catalog/dialogues/{dialogueId:D}/phrases",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminDialogueDetail(response);
    }

    public async Task<AdminDialogueDetailViewModel> DeleteAdminDialoguePhraseAsync(Guid dialogueId, Guid phraseId, CancellationToken cancellationToken)
    {
        AdminDialogueDetailResponse response = await PostRequiredAsync<AdminEmptyRequest, AdminDialogueDetailResponse>(
            $"/api/admin/catalog/dialogues/{dialogueId:D}/phrases/{phraseId:D}/delete",
            new AdminEmptyRequest(),
            cancellationToken).ConfigureAwait(false);

        return MapAdminDialogueDetail(response);
    }

    public async Task<AdminDialogueDetailViewModel> AddAdminDialogueQuestionAsync(Guid dialogueId, AdminAddDialogueQuestionRequest request, CancellationToken cancellationToken)
    {
        AdminDialogueDetailResponse response = await PostRequiredAsync<AdminAddDialogueQuestionRequest, AdminDialogueDetailResponse>(
            $"/api/admin/catalog/dialogues/{dialogueId:D}/questions",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminDialogueDetail(response);
    }

    public async Task<AdminDialogueDetailViewModel> DeleteAdminDialogueQuestionAsync(Guid dialogueId, Guid questionId, CancellationToken cancellationToken)
    {
        AdminDialogueDetailResponse response = await PostRequiredAsync<AdminEmptyRequest, AdminDialogueDetailResponse>(
            $"/api/admin/catalog/dialogues/{dialogueId:D}/questions/{questionId:D}/delete",
            new AdminEmptyRequest(),
            cancellationToken).ConfigureAwait(false);

        return MapAdminDialogueDetail(response);
    }

    public async Task<AdminDialogueDetailViewModel> AddAdminDialogueAnswerAsync(Guid dialogueId, Guid questionId, AdminAddDialogueAnswerRequest request, CancellationToken cancellationToken)
    {
        AdminDialogueDetailResponse response = await PostRequiredAsync<AdminAddDialogueAnswerRequest, AdminDialogueDetailResponse>(
            $"/api/admin/catalog/dialogues/{dialogueId:D}/questions/{questionId:D}/answers",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminDialogueDetail(response);
    }

    public async Task<AdminDialogueDetailViewModel> DeleteAdminDialogueAnswerAsync(Guid dialogueId, Guid questionId, Guid answerId, CancellationToken cancellationToken)
    {
        AdminDialogueDetailResponse response = await PostRequiredAsync<AdminEmptyRequest, AdminDialogueDetailResponse>(
            $"/api/admin/catalog/dialogues/{dialogueId:D}/questions/{questionId:D}/answers/{answerId:D}/delete",
            new AdminEmptyRequest(),
            cancellationToken).ConfigureAwait(false);

        return MapAdminDialogueDetail(response);
    }

    public async Task<AdminDialogueDetailViewModel> AddAdminDialogueTurnTranslationAsync(Guid dialogueId, Guid turnId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken)
    {
        AdminDialogueDetailResponse response = await PostRequiredAsync<AdminAddDialogueTranslationRequest, AdminDialogueDetailResponse>(
            $"/api/admin/catalog/dialogues/{dialogueId:D}/dialogue-turns/{turnId:D}/translations",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminDialogueDetail(response);
    }

    public async Task<AdminDialogueDetailViewModel> DeleteAdminDialogueTurnTranslationAsync(Guid dialogueId, Guid turnId, Guid translationId, CancellationToken cancellationToken)
    {
        AdminDialogueDetailResponse response = await PostRequiredAsync<AdminEmptyRequest, AdminDialogueDetailResponse>(
            $"/api/admin/catalog/dialogues/{dialogueId:D}/dialogue-turns/{turnId:D}/translations/{translationId:D}/delete",
            new AdminEmptyRequest(),
            cancellationToken).ConfigureAwait(false);

        return MapAdminDialogueDetail(response);
    }

    public async Task<AdminDialogueDetailViewModel> AddAdminDialoguePhraseTranslationAsync(Guid dialogueId, Guid phraseId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken)
    {
        AdminDialogueDetailResponse response = await PostRequiredAsync<AdminAddDialogueTranslationRequest, AdminDialogueDetailResponse>(
            $"/api/admin/catalog/dialogues/{dialogueId:D}/phrases/{phraseId:D}/translations",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminDialogueDetail(response);
    }

    public async Task<AdminDialogueDetailViewModel> DeleteAdminDialoguePhraseTranslationAsync(Guid dialogueId, Guid phraseId, Guid translationId, CancellationToken cancellationToken)
    {
        AdminDialogueDetailResponse response = await PostRequiredAsync<AdminEmptyRequest, AdminDialogueDetailResponse>(
            $"/api/admin/catalog/dialogues/{dialogueId:D}/phrases/{phraseId:D}/translations/{translationId:D}/delete",
            new AdminEmptyRequest(),
            cancellationToken).ConfigureAwait(false);

        return MapAdminDialogueDetail(response);
    }

    public async Task<AdminDialogueDetailViewModel> AddAdminDialogueQuestionTranslationAsync(Guid dialogueId, Guid questionId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken)
    {
        AdminDialogueDetailResponse response = await PostRequiredAsync<AdminAddDialogueTranslationRequest, AdminDialogueDetailResponse>(
            $"/api/admin/catalog/dialogues/{dialogueId:D}/questions/{questionId:D}/translations",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminDialogueDetail(response);
    }

    public async Task<AdminDialogueDetailViewModel> DeleteAdminDialogueQuestionTranslationAsync(Guid dialogueId, Guid questionId, Guid translationId, CancellationToken cancellationToken)
    {
        AdminDialogueDetailResponse response = await PostRequiredAsync<AdminEmptyRequest, AdminDialogueDetailResponse>(
            $"/api/admin/catalog/dialogues/{dialogueId:D}/questions/{questionId:D}/translations/{translationId:D}/delete",
            new AdminEmptyRequest(),
            cancellationToken).ConfigureAwait(false);

        return MapAdminDialogueDetail(response);
    }

    public async Task<AdminDialogueDetailViewModel> AddAdminDialogueAnswerTranslationAsync(Guid dialogueId, Guid questionId, Guid answerId, AdminAddDialogueTranslationRequest request, CancellationToken cancellationToken)
    {
        AdminDialogueDetailResponse response = await PostRequiredAsync<AdminAddDialogueTranslationRequest, AdminDialogueDetailResponse>(
            $"/api/admin/catalog/dialogues/{dialogueId:D}/questions/{questionId:D}/answers/{answerId:D}/translations",
            request,
            cancellationToken).ConfigureAwait(false);

        return MapAdminDialogueDetail(response);
    }

    public async Task<AdminDialogueDetailViewModel> DeleteAdminDialogueAnswerTranslationAsync(Guid dialogueId, Guid questionId, Guid answerId, Guid translationId, CancellationToken cancellationToken)
    {
        AdminDialogueDetailResponse response = await PostRequiredAsync<AdminEmptyRequest, AdminDialogueDetailResponse>(
            $"/api/admin/catalog/dialogues/{dialogueId:D}/questions/{questionId:D}/answers/{answerId:D}/translations/{translationId:D}/delete",
            new AdminEmptyRequest(),
            cancellationToken).ConfigureAwait(false);

        return MapAdminDialogueDetail(response);
    }

    public Task<ConversationEventDetailModel> SaveAdminConversationEventAsync(
        AdminSaveConversationEventRequest request,
        CancellationToken cancellationToken) =>
        PostRequiredAsync<AdminSaveConversationEventRequest, ConversationEventDetailModel>(
            "/api/admin/catalog/conversation-events",
            request,
            cancellationToken);

    public Task<IReadOnlyList<OrganizerManagedConversationEventModel>> GetAdminConversationEventsByOrganizerAsync(
        string organizerProfileSlug,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<OrganizerManagedConversationEventModel>>(
            $"/api/admin/catalog/conversation-events/by-organizer/{Uri.EscapeDataString(organizerProfileSlug)}",
            cancellationToken);

    public Task<OrganizerManagedConversationEventModel> SetAdminConversationEventPublicationStatusAsync(
        string slug,
        AdminSetConversationEventPublicationStatusRequest request,
        CancellationToken cancellationToken) =>
        PostRequiredAsync<AdminSetConversationEventPublicationStatusRequest, OrganizerManagedConversationEventModel>(
            $"/api/admin/catalog/conversation-events/{Uri.EscapeDataString(slug)}/publication-status",
            request,
            cancellationToken);

    public Task<OrganizerProfileDetailModel> SaveAdminOrganizerProfileAsync(
        AdminSaveOrganizerProfileRequest request,
        CancellationToken cancellationToken) =>
        PostRequiredAsync<AdminSaveOrganizerProfileRequest, OrganizerProfileDetailModel>(
            "/api/admin/catalog/organizer-profiles",
            request,
            cancellationToken);

    public Task<IReadOnlyList<OrganizerClaimRequestModel>> GetAdminOrganizerClaimRequestsAsync(CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<OrganizerClaimRequestModel>>(
            "/api/admin/catalog/organizer-claim-requests",
            cancellationToken);

    public Task<OrganizerClaimRequestModel> SetAdminOrganizerClaimRequestStatusAsync(
        Guid claimRequestId,
        OrganizerClaimDecisionRequest request,
        CancellationToken cancellationToken) =>
        PostRequiredAsync<OrganizerClaimDecisionRequest, OrganizerClaimRequestModel>(
            $"/api/admin/catalog/organizer-claim-requests/{claimRequestId:D}/status",
            request,
            cancellationToken);

    public Task<IReadOnlyList<OrganizerProfileOwnerModel>> GetAdminOrganizerProfileOwnersAsync(CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<OrganizerProfileOwnerModel>>(
            "/api/admin/catalog/organizer-profile-owners",
            cancellationToken);

    public Task<OrganizerProfileOwnerModel> AssignAdminOrganizerProfileOwnerAsync(
        AssignOrganizerProfileOwnerRequest request,
        CancellationToken cancellationToken) =>
        PostRequiredAsync<AssignOrganizerProfileOwnerRequest, OrganizerProfileOwnerModel>(
            "/api/admin/catalog/organizer-profile-owners",
            request,
            cancellationToken);

    private async Task<T?> GetAsync<T>(string relativeUri, CancellationToken cancellationToken)
    {
        return await GetAsync<T>(relativeUri, actorEmail: null, cancellationToken).ConfigureAwait(false);
    }

    private async Task<T?> GetAsync<T>(
        string relativeUri,
        string? actorEmail,
        CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        WebTelemetryOutcome outcome = WebTelemetryOutcome.Success;
        int itemCount = 0;
        using HttpRequestMessage request = new(HttpMethod.Get, relativeUri);
        try
        {
            AddActorEmailHeader(request, actorEmail);
            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }

            await EnsureSuccessAsync(response, relativeUri, cancellationToken).ConfigureAwait(false);
            string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(body))
            {
                return default;
            }

            T? payload = JsonSerializer.Deserialize<T>(body, JsonSerializerOptions.Web);
            itemCount = CountItems(payload);
            return payload;
        }
        catch
        {
            outcome = WebTelemetryOutcome.Failure;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            telemetryService.Record(
                BuildWebApiOperationKey(HttpMethod.Get, relativeUri),
                stopwatch.Elapsed,
                outcome,
                itemCount);
        }
    }

    private async Task<T> GetRequiredAsync<T>(string relativeUri, CancellationToken cancellationToken)
    {
        T? response = await GetAsync<T>(relativeUri, cancellationToken).ConfigureAwait(false);
        return response ?? throw new InvalidOperationException($"The Web API returned an empty payload for '{relativeUri}'.");
    }

    private async Task<T> GetRequiredAsync<T>(
        string relativeUri,
        string actorEmail,
        CancellationToken cancellationToken)
    {
        T? response = await GetAsync<T>(relativeUri, actorEmail, cancellationToken).ConfigureAwait(false);
        return response ?? throw new InvalidOperationException($"The Web API returned an empty payload for '{relativeUri}'.");
    }

    private async Task<TResponse> PostRequiredAsync<TRequest, TResponse>(
        string relativeUri,
        TRequest request,
        CancellationToken cancellationToken)
    {
        return await PostRequiredAsync<TRequest, TResponse>(
                relativeUri,
                actorEmail: null,
                request,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<TResponse> PostRequiredAsync<TRequest, TResponse>(
        string relativeUri,
        string? actorEmail,
        TRequest request,
        CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        WebTelemetryOutcome outcome = WebTelemetryOutcome.Success;
        int itemCount = 0;
        using HttpRequestMessage httpRequest = new(HttpMethod.Post, relativeUri);
        try
        {
            AddActorEmailHeader(httpRequest, actorEmail);
            httpRequest.Content = JsonContent.Create(request, options: JsonSerializerOptions.Web);
            using HttpResponseMessage response = await httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            await EnsureSuccessAsync(response, relativeUri, cancellationToken).ConfigureAwait(false);

            TResponse? payload = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken).ConfigureAwait(false);
            itemCount = CountItems(payload);
            return payload ?? throw new InvalidOperationException($"The Web API returned an empty payload for '{relativeUri}'.");
        }
        catch
        {
            outcome = WebTelemetryOutcome.Failure;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            telemetryService.Record(
                BuildWebApiOperationKey(HttpMethod.Post, relativeUri),
                stopwatch.Elapsed,
                outcome,
                itemCount);
        }
    }

    private static void AddActorEmailHeader(HttpRequestMessage request, string? actorEmail)
    {
        if (!string.IsNullOrWhiteSpace(actorEmail))
        {
            request.Headers.TryAddWithoutValidation(ActorEmailHeaderName, actorEmail);
        }
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string relativeUri,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        string responseSummary = SummarizeErrorResponseBody(body);
        throw new InvalidOperationException(
            $"The Web API call to '{relativeUri}' failed with status {(int)response.StatusCode}. Response: {responseSummary}");
    }

    private static string SummarizeErrorResponseBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return "(empty)";
        }

        string normalized = body.ReplaceLineEndings(" ").Trim();
        return normalized.Length <= MaxErrorResponseBodyLength
            ? normalized
            : string.Concat(normalized.AsSpan(0, MaxErrorResponseBodyLength), "...");
    }

    private static int CountItems<T>(T? payload)
    {
        if (payload is null)
        {
            return 0;
        }

        if (payload is string)
        {
            return 1;
        }

        return payload is ICollection collection ? collection.Count : 1;
    }

    private static string BuildWebApiOperationKey(HttpMethod method, string relativeUri) =>
        $"webapi:{method.Method}:{NormalizePathShape(relativeUri)}".ToLowerInvariant();

    private static AdminTopicItemViewModel MapAdminTopic(AdminTopicItemResponse topic) =>
        new(
            topic.TopicId,
            topic.Key,
            topic.SortOrder,
            topic.IsSystem,
            topic.WordCount,
            topic.UpdatedAtUtc,
            topic.Localizations
                .Select(localization => new AdminTopicLocalizationViewModel(
                    localization.LanguageCode,
                    localization.DisplayName))
                .ToArray());

    private static AdminLabelItemViewModel MapAdminLabel(AdminLabelItemResponse label) =>
        new(
            label.LabelId,
            label.Kind,
            label.Key,
            label.DisplayName,
            label.Localizations?
                .Select(localization => new AdminLabelLocalizationViewModel(
                    localization.LanguageCode,
                    localization.DisplayName))
                .ToArray(),
            label.SortOrder,
            label.IsSystem,
            label.WordCount,
            label.UpdatedAtUtc);

    private static AdminCollectionItemViewModel MapAdminCollectionItem(AdminCollectionItemResponse collection) =>
        new(
            collection.CollectionId,
            collection.Slug,
            collection.Name,
            collection.Description,
            collection.Localizations?
                .Select(localization => new AdminCollectionLocalizationViewModel(
                    localization.LocalizationId,
                    localization.LanguageCode,
                    localization.Name,
                    localization.Description))
                .ToArray(),
            collection.ImageUrl,
            collection.PublicationStatus,
            collection.SortOrder,
            collection.WordCount,
            collection.UpdatedAtUtc);

    private static AdminCollectionDetailViewModel MapAdminCollectionDetail(AdminCollectionDetailResponse collection) =>
        new(
            collection.CollectionId,
            collection.Slug,
            collection.Name,
            collection.Description,
            collection.Localizations?
                .Select(localization => new AdminCollectionLocalizationViewModel(
                    localization.LocalizationId,
                    localization.LanguageCode,
                    localization.Name,
                    localization.Description))
                .ToArray(),
            collection.ImageUrl,
            collection.PublicationStatus,
            collection.SortOrder,
            collection.CreatedAtUtc,
            collection.UpdatedAtUtc,
            collection.Entries
                .Select(entry => new AdminCollectionEntryViewModel(
                    entry.EntryId,
                    entry.WordPublicId,
                    entry.Lemma,
                    entry.PartOfSpeech,
                    entry.CefrLevel,
                    entry.SortOrder))
                .ToArray());

    private static AdminDialogueItemViewModel MapAdminDialogueItem(AdminDialogueItemResponse dialogue) =>
        new(
            dialogue.DialogueId,
            dialogue.Slug,
            dialogue.Title,
            dialogue.CefrLevel,
            dialogue.Category,
            dialogue.PublicationStatus,
            dialogue.SortOrder,
            dialogue.DialogueTurnCount,
            dialogue.PhraseCount,
            dialogue.QuestionCount,
            dialogue.UpdatedAtUtc);

    private static AdminDialogueDetailViewModel MapAdminDialogueDetail(AdminDialogueDetailResponse dialogue) =>
        new(
            dialogue.DialogueId,
            dialogue.Slug,
            dialogue.Title,
            dialogue.Description,
            dialogue.LearnerGoal,
            dialogue.CefrLevel,
            dialogue.Category,
            dialogue.PublicationStatus,
            dialogue.SortOrder,
            dialogue.CreatedAtUtc,
            dialogue.UpdatedAtUtc,
            dialogue.DialogueTurns
                .Select(turn => new AdminDialogueTurnViewModel(
                    turn.TurnId,
                    turn.SortOrder,
                    turn.SpeakerRole,
                    turn.BaseText,
                    MapAdminDialogueTranslations(turn.Translations)))
                .ToArray(),
            dialogue.UsefulPhrases
                .Select(phrase => new AdminDialoguePhraseViewModel(
                    phrase.PhraseId,
                    phrase.SortOrder,
                    phrase.BaseText,
                    phrase.UsageNote,
                    MapAdminDialogueTranslations(phrase.Translations)))
                .ToArray(),
            dialogue.Questions
                .Select(question => new AdminDialogueQuestionViewModel(
                    question.QuestionId,
                    question.SortOrder,
                    question.Prompt,
                    MapAdminDialogueTranslations(question.Translations),
                    question.Answers
                        .Select(answer => new AdminDialogueAnswerViewModel(
                            answer.AnswerId,
                            answer.SortOrder,
                            answer.Text,
                            answer.IsCorrect,
                            answer.Feedback,
                            MapAdminDialogueTranslations(answer.Translations)))
                        .ToArray()))
                .ToArray());

    private static AdminDialogueTranslationViewModel[] MapAdminDialogueTranslations(
        IReadOnlyList<AdminDialogueTranslationResponse> translations) =>
        translations
            .Select(translation => new AdminDialogueTranslationViewModel(
                translation.TranslationId,
                translation.LanguageCode,
                translation.Text))
            .ToArray();

    private static string NormalizePathShape(string relativeUri)
    {
        string path = relativeUri.Split('?', 2)[0].Trim('/');
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        string[] segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        for (int index = 0; index < segments.Length; index++)
        {
            if (Guid.TryParse(segments[index], out _))
            {
                segments[index] = "{guid}";
                continue;
            }

            if (ShouldRedactRouteSegment(segments, index))
            {
                segments[index] = "{key}";
            }
        }

        return string.Concat("/", string.Join('/', segments));
    }

    private static bool ShouldRedactRouteSegment(string[] segments, int index)
    {
        if (index == 0)
        {
            return false;
        }

        string previous = segments[index - 1];
        return string.Equals(previous, "collections", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(previous, "dialogues", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(previous, "conversation-starters", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(previous, "event-preparation-packs", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(previous, "conversation-events", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(previous, "organizer-profiles", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(previous, "by-organizer", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(previous, "organizer-claim-requests", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(previous, "partner-requests", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(previous, "reports", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildPath(string path, IEnumerable<KeyValuePair<string, string?>> queryParameters)
    {
        StringBuilder builder = new(path);
        bool isFirst = true;

        foreach ((string key, string? value) in queryParameters)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            builder.Append(isFirst ? '?' : '&');
            builder.Append(Uri.EscapeDataString(key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(value));
            isFirst = false;
        }

        return builder.ToString();
    }
}

internal static class WebCatalogApiClientRegistration
{
    public static IServiceCollection AddWebCatalogApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<WebApiOptions>()
            .Bind(configuration.GetSection(WebApiOptions.SectionName))
            .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _), "WebApi:BaseUrl must be an absolute URL.")
            .ValidateOnStart();

        services.AddHttpClient<IWebCatalogApiClient, WebCatalogApiClient>((serviceProvider, client) =>
        {
            WebApiOptions options = serviceProvider.GetRequiredService<IOptions<WebApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(30);

            if (!string.IsNullOrWhiteSpace(options.AdminApiKey))
            {
                client.DefaultRequestHeaders.Add(options.AdminApiHeaderName, options.AdminApiKey);
            }
        })
        .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
        {
            WebApiOptions options = serviceProvider.GetRequiredService<IOptions<WebApiOptions>>().Value;
            HttpClientHandler handler = new();

            if (options.IgnoreSslErrors)
            {
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            return handler;
        });

        return services;
    }
}

internal sealed record CatalogWordLookupRequest(
    IReadOnlyList<Guid> WordIds,
    string MeaningLanguageCode);

internal sealed record AdminCatalogDashboardResponse(
    int ActiveWordCount,
    int DraftWordCount,
    int TotalTopicCount,
    int ImportedPackageCount,
    int FailedPackageCount,
    DateTime? LastImportAtUtc);

public sealed record AdminSystemReportResponse(
    DateTime GeneratedAtUtc,
    AdminCatalogSystemReportResponse Catalog,
    AdminSocialSystemReportResponse Social,
    AdminModerationSystemReportResponse Moderation,
    AdminOperationsSystemReportResponse Operations);

public sealed record AdminCatalogSystemReportResponse(
    int ActiveWordCount,
    int DraftWordCount,
    int TopicCount,
    int DialogueLessonCount,
    int ConversationStarterPackCount,
    int EventPreparationPackCount);

public sealed record AdminSocialSystemReportResponse(
    int OrganizerProfileCount,
    int ConversationEventCount,
    int OnlineConversationEventCount,
    int EventRsvpCount,
    int OrganizerClaimRequestCount,
    int PendingOrganizerClaimRequestCount,
    int OrganizerProfileOwnerCount,
    int LearnerConversationProfileCount,
    int PublicLearnerConversationProfileCount,
    int PartnerRequestCount,
    int PendingPartnerRequestCount);

public sealed record AdminModerationSystemReportResponse(
    int UserReportCount,
    int PendingUserReportCount,
    int UserBlockCount,
    int ModerationDecisionAuditCount);

public sealed record AdminOperationsSystemReportResponse(
    int ImportedPackageCount,
    int FailedPackageCount,
    DateTime? LastImportAtUtc);

internal sealed record AdminCatalogImportsResponse(
    string? StatusFilter,
    IReadOnlyList<AdminCatalogImportItemResponse> Packages);

internal sealed record AdminCatalogImportItemResponse(
    string PackageId,
    string PackageVersion,
    string PackageName,
    string SourceType,
    string Status,
    int TotalEntries,
    int InsertedEntries,
    int InvalidEntries,
    int WarningCount,
    DateTime CreatedAtUtc);

internal sealed record AdminCatalogWordsResponse(
    string? Query,
    string? StatusFilter,
    string Sort,
    int Skip,
    int Take,
    int TotalCount,
    IReadOnlyList<AdminCatalogWordItemResponse> Words);

internal sealed record AdminCatalogWordItemResponse(
    Guid PublicId,
    string Lemma,
    string? Article,
    string PartOfSpeech,
    string CefrLevel,
    string PublicationStatus,
    string ContentSourceType,
    int SenseCount,
    int TopicCount,
    DateTime UpdatedAtUtc);

internal sealed record AdminCatalogWordDetailResponse(
    Guid PublicId,
    string Lemma,
    string NormalizedLemma,
    string LanguageCode,
    string? Article,
    string? PluralForm,
    string? InfinitiveForm,
    string? PronunciationIpa,
    string? SyllableBreak,
    string PartOfSpeech,
    string CefrLevel,
    string PublicationStatus,
    string ContentSourceType,
    string? SourceReference,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<AdminCatalogWordLexicalFormResponse> LexicalForms,
    IReadOnlyList<AdminCatalogWordSenseResponse> Senses,
    IReadOnlyList<AdminCatalogWordTopicResponse> Topics,
    IReadOnlyList<AdminCatalogWordLabelResponse> Labels,
    IReadOnlyList<AdminCatalogWordTextItemResponse> GrammarNotes,
    IReadOnlyList<AdminCatalogWordCollocationResponse> Collocations);

internal sealed record AdminCatalogWordLexicalFormResponse(
    string PartOfSpeech,
    string? Article,
    string? PluralForm,
    string? InfinitiveForm,
    bool IsPrimary,
    int SortOrder);

internal sealed record AdminCatalogWordSenseResponse(
    Guid SenseId,
    int SenseOrder,
    bool IsPrimarySense,
    string PublicationStatus,
    string? ShortDefinitionDe,
    string? ShortGloss,
    IReadOnlyList<AdminCatalogWordTranslationResponse> Translations,
    IReadOnlyList<AdminCatalogWordExampleResponse> Examples);

internal sealed record AdminCatalogWordTranslationResponse(
    Guid TranslationId,
    string LanguageCode,
    string TranslationText,
    bool IsPrimary);

internal sealed record AdminCatalogWordExampleResponse(
    Guid ExampleId,
    int SentenceOrder,
    string GermanText,
    bool IsPrimaryExample,
    IReadOnlyList<AdminCatalogWordExampleTranslationResponse> Translations);

internal sealed record AdminCatalogWordExampleTranslationResponse(
    Guid TranslationId,
    string LanguageCode,
    string TranslationText);

internal sealed record AdminCatalogWordTopicResponse(
    Guid TopicId,
    string Key,
    bool IsPrimaryTopic);

internal sealed record AdminCatalogWordLabelResponse(
    string Kind,
    string Key,
    string DisplayName,
    int SortOrder);

internal sealed record AdminCatalogWordTextItemResponse(
    string Text,
    int SortOrder);

internal sealed record AdminCatalogWordCollocationResponse(
    string Text,
    string? Meaning,
    int SortOrder);

internal sealed record AdminTopicsResponse(
    IReadOnlyList<AdminTopicItemResponse> Topics);

internal sealed record AdminTopicItemResponse(
    Guid TopicId,
    string Key,
    int SortOrder,
    bool IsSystem,
    int WordCount,
    DateTime UpdatedAtUtc,
    IReadOnlyList<AdminTopicLocalizationResponse> Localizations);

internal sealed record AdminTopicLocalizationResponse(
    string LanguageCode,
    string DisplayName);

internal sealed record AdminLabelsResponse(
    IReadOnlyList<AdminLabelItemResponse> Labels);

internal sealed record AdminLabelItemResponse(
    Guid LabelId,
    string Kind,
    string Key,
    string DisplayName,
    IReadOnlyList<AdminLabelLocalizationResponse>? Localizations,
    int SortOrder,
    bool IsSystem,
    int WordCount,
    DateTime UpdatedAtUtc);

internal sealed record AdminLabelLocalizationResponse(
    string LanguageCode,
    string DisplayName);

internal sealed record AdminCollectionsResponse(
    IReadOnlyList<AdminCollectionItemResponse> Collections);

internal sealed record AdminCollectionItemResponse(
    Guid CollectionId,
    string Slug,
    string Name,
    string? Description,
    IReadOnlyList<AdminCollectionLocalizationResponse>? Localizations,
    string? ImageUrl,
    string PublicationStatus,
    int SortOrder,
    int WordCount,
    DateTime UpdatedAtUtc);

internal sealed record AdminCollectionDetailResponse(
    Guid CollectionId,
    string Slug,
    string Name,
    string? Description,
    IReadOnlyList<AdminCollectionLocalizationResponse>? Localizations,
    string? ImageUrl,
    string PublicationStatus,
    int SortOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<AdminCollectionEntryResponse> Entries);

internal sealed record AdminCollectionLocalizationResponse(
    Guid LocalizationId,
    string LanguageCode,
    string Name,
    string? Description);

internal sealed record AdminCollectionEntryResponse(
    Guid EntryId,
    Guid WordPublicId,
    string Lemma,
    string PartOfSpeech,
    string CefrLevel,
    int SortOrder);

internal sealed record AdminDialoguesResponse(
    IReadOnlyList<AdminDialogueItemResponse> Dialogues);

internal sealed record AdminDialogueItemResponse(
    Guid DialogueId,
    string Slug,
    string Title,
    string CefrLevel,
    string Category,
    string PublicationStatus,
    int SortOrder,
    int DialogueTurnCount,
    int PhraseCount,
    int QuestionCount,
    DateTime UpdatedAtUtc);

internal sealed record AdminDialogueDetailResponse(
    Guid DialogueId,
    string Slug,
    string Title,
    string Description,
    string LearnerGoal,
    string CefrLevel,
    string Category,
    string PublicationStatus,
    int SortOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<AdminDialogueTurnResponse> DialogueTurns,
    IReadOnlyList<AdminDialoguePhraseResponse> UsefulPhrases,
    IReadOnlyList<AdminDialogueQuestionResponse> Questions);

internal sealed record AdminDialogueTurnResponse(
    Guid TurnId,
    int SortOrder,
    string SpeakerRole,
    string BaseText,
    IReadOnlyList<AdminDialogueTranslationResponse> Translations);

internal sealed record AdminDialoguePhraseResponse(
    Guid PhraseId,
    int SortOrder,
    string BaseText,
    string? UsageNote,
    IReadOnlyList<AdminDialogueTranslationResponse> Translations);

internal sealed record AdminDialogueQuestionResponse(
    Guid QuestionId,
    int SortOrder,
    string Prompt,
    IReadOnlyList<AdminDialogueTranslationResponse> Translations,
    IReadOnlyList<AdminDialogueAnswerResponse> Answers);

internal sealed record AdminDialogueAnswerResponse(
    Guid AnswerId,
    int SortOrder,
    string Text,
    bool IsCorrect,
    string? Feedback,
    IReadOnlyList<AdminDialogueTranslationResponse> Translations);

internal sealed record AdminDialogueTranslationResponse(
    Guid TranslationId,
    string LanguageCode,
    string Text);

internal sealed record AdminEmptyRequest;

internal sealed record AdminCatalogDraftWordsResponse(
    string? Query,
    IReadOnlyList<AdminCatalogDraftWordItemResponse> Words);

internal sealed record AdminCatalogDraftWordItemResponse(
    Guid PublicId,
    string Lemma,
    string PartOfSpeech,
    string CefrLevel,
    string PublicationStatus);

internal sealed record AdminCatalogHistoryViewResponse(
    string? StatusFilter,
    IReadOnlyList<AdminCatalogHistoryItemResponse> Items);

internal sealed record AdminCatalogHistoryItemResponse(
    string PackageId,
    string PackageVersion,
    string Status,
    int TotalEntries,
    int InsertedEntries,
    int InvalidEntries,
    DateTime CreatedAtUtc);

internal sealed record AdminCatalogRollbackPreviewResponse(
    int DraftWordCount,
    int ImportedPackageCount,
    string WarningMessage);

public sealed record AdminSaveConversationEventRequest(
    string Slug,
    string Name,
    string Description,
    string? City,
    string CountryRegion,
    string? ApproximateLocation,
    bool IsOnline,
    string Category,
    IReadOnlyList<string> SupportedLearnerLevels,
    IReadOnlyList<string> HelperLanguageCodes,
    string OrganizerName,
    string? OrganizerProfileSlug,
    string? ExternalLink,
    string? ContactMethod,
    string ScheduleText,
    string PriceType,
    string VerificationStatus,
    string? SourceName,
    string? SourceUrl,
    DateTime? LastVerifiedAtUtc,
    IReadOnlyList<string> LinkedEventPreparationPackSlugs)
{
    public string? RecurrenceRule { get; init; }

    public int? Capacity { get; init; }
}

public sealed record AdminSetConversationEventPublicationStatusRequest(
    string PublicationStatus);

public sealed record OrganizerManagedConversationEventModel(
    string Slug,
    string Name,
    string Description,
    string? City,
    string CountryRegion,
    string? ApproximateLocation,
    bool IsOnline,
    string Category,
    IReadOnlyList<string> SupportedLearnerLevels,
    IReadOnlyList<string> HelperLanguageCodes,
    string OrganizerName,
    string? OrganizerProfileSlug,
    string? ExternalLink,
    string? ContactMethod,
    string ScheduleText,
    string PriceType,
    string VerificationStatus,
    string PublicationStatus,
    string? SourceName,
    string? SourceUrl,
    DateTime? LastVerifiedAtUtc,
    IReadOnlyList<string> LinkedEventPreparationPackSlugs)
{
    public string? RecurrenceRule { get; init; }

    public int? Capacity { get; init; }
}

public sealed record SubmitEventRsvpRequest(
    string ParticipantName,
    string ParticipantEmail,
    string Status);

public sealed record AdminSetEventRsvpStatusRequest(
    string Status);

public sealed record EventRsvpModel(
    Guid Id,
    string ConversationEventSlug,
    string ParticipantName,
    string ParticipantEmail,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record EventRsvpSummaryModel(
    string ConversationEventSlug,
    int InterestedCount,
    int GoingCount,
    int CancelledCount,
    int Capacity,
    int? RemainingCapacity);

public sealed record AdminSaveOrganizerProfileRequest(
    string Slug,
    string DisplayName,
    string OrganizerType,
    string Description,
    string? CityRegion,
    bool IsOnlineAvailable,
    IReadOnlyList<string> SupportedLearnerLevels,
    IReadOnlyList<string> HelperLanguageCodes,
    string? WebsiteUrl,
    string? PublicContactMethod,
    string VerificationStatus,
    string PlanKey,
    int HistoricalEventCount);

public sealed record SubmitOrganizerClaimRequest(
    string RequesterName,
    string RequesterEmail,
    string RelationshipToOrganizer,
    string EvidenceText);

public sealed record OrganizerClaimRequestModel(
    Guid Id,
    string OrganizerProfileSlug,
    string RequesterName,
    string RequesterEmail,
    string RelationshipToOrganizer,
    string EvidenceText,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record OrganizerClaimDecisionRequest(
    string Status);

public sealed record AssignOrganizerProfileOwnerRequest(
    string OrganizerProfileSlug,
    string OwnerEmail,
    string AssignedBy);

public sealed record OrganizerProfileOwnerModel(
    Guid Id,
    string OrganizerProfileSlug,
    string OwnerEmail,
    string AssignedBy,
    DateTime CreatedAtUtc);

public sealed record SaveLearnerConversationProfileRequest(
    string DisplayName,
    string? CityRegion,
    string InteractionPreference,
    string GermanLevel,
    IReadOnlyList<string> HelperLanguageCodes,
    string ConversationGoals,
    string? AvailabilityNotes,
    string Visibility,
    bool HasConfirmedAdult);

public sealed record LearnerConversationProfileVisibilityRequest(
    bool IsEnabled);

public sealed record LearnerConversationProfileModel(
    Guid Id,
    string OwnerEmail,
    string DisplayName,
    string? CityRegion,
    string InteractionPreference,
    string GermanLevel,
    IReadOnlyList<string> HelperLanguageCodes,
    string ConversationGoals,
    string? AvailabilityNotes,
    string Visibility,
    bool HasConfirmedAdult,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record LearnerConversationProfilePublicModel(
    string DisplayName,
    string? CityRegion,
    string InteractionPreference,
    string GermanLevel,
    IReadOnlyList<string> HelperLanguageCodes,
    string ConversationGoals);

public sealed record PartnerMatchSearchRequest(
    string? CityRegion,
    string? InteractionPreference,
    string? GermanLevel,
    string? HelperLanguageCode,
    string? GoalKeyword);

public sealed record PartnerMatchProfileModel(
    Guid ProfileId,
    string DisplayName,
    string? CityRegion,
    string InteractionPreference,
    string GermanLevel,
    IReadOnlyList<string> HelperLanguageCodes,
    string ConversationGoals,
    string Visibility);

public sealed record SubmitPartnerRequestRequest(
    Guid TargetLearnerProfileId,
    string OpenerTemplateKey,
    string? Note);

public sealed record PartnerRequestStateUpdateRequest(
    string Action);

public sealed record PartnerRequestModel(
    Guid Id,
    string Direction,
    Guid TargetLearnerProfileId,
    string OtherDisplayName,
    string? OtherCityRegion,
    string OpenerTemplateKey,
    string? Note,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime ExpiresAtUtc,
    DateTime? RespondedAtUtc,
    string? ContactEmail);

public sealed record SubmitUserReportRequest(
    string TargetType,
    string TargetKey,
    string? ReportedUserEmail,
    string Reason,
    string Details);

public sealed record BlockUserRequest(
    string? BlockedEmail,
    string? Reason,
    Guid? SourcePartnerRequestId,
    Guid? TargetLearnerProfileId = null);

public sealed record ModerationDecisionRequest(
    string Status,
    string? DecisionNote,
    string DecidedBy);

public sealed record UserReportModel(
    Guid Id,
    string ReporterEmail,
    string TargetType,
    string TargetKey,
    string? ReportedUserEmail,
    string Reason,
    string Details,
    string Status,
    string? DecisionNote,
    string? DecidedBy,
    DateTime? DecidedAtUtc,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record UserBlockModel(
    Guid Id,
    string BlockerEmail,
    string BlockedEmail,
    string? Reason,
    Guid? SourcePartnerRequestId,
    DateTime CreatedAtUtc);

public sealed record ModerationDecisionAuditModel(
    Guid Id,
    Guid UserReportId,
    string DecisionStatus,
    string DecidedBy,
    string? DecisionNote,
    DateTime CreatedAtUtc);

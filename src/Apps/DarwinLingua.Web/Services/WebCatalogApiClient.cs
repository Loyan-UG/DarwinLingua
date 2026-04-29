using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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

    Task<IReadOnlyList<ScenarioLessonListItemModel>> GetScenariosAsync(CancellationToken cancellationToken);

    Task<ScenarioLessonDetailModel?> GetScenarioBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetConversationStarterPacksAsync(
        ConversationStarterListFilterModel filter,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetConversationStarterPacksForScenarioAsync(
        string scenarioSlug,
        CancellationToken cancellationToken);

    Task<ConversationStarterPackDetailModel?> GetConversationStarterPackBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<EventPreparationPackListItemModel>> GetEventPreparationPacksForScenarioAsync(
        string scenarioSlug,
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

    Task<IReadOnlyList<WordListItemModel>> GetWordsByIdsAsync(
        IReadOnlyList<Guid> wordIds,
        string meaningLanguageCode,
        CancellationToken cancellationToken);

    Task<AdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken cancellationToken);

    Task<AdminSystemReportResponse> GetAdminSystemReportAsync(CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<AdminImportsPageViewModel> GetAdminImportsAsync(string? statusFilter, CancellationToken cancellationToken);

    Task<AdminDraftWordsPageViewModel> GetAdminDraftWordsAsync(string? query, CancellationToken cancellationToken);

    Task<AdminHistoryPageViewModel> GetAdminHistoryAsync(string? statusFilter, CancellationToken cancellationToken);

    Task<AdminRollbackPageViewModel> GetAdminRollbackPreviewAsync(CancellationToken cancellationToken);

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

    Task<IReadOnlyList<OrganizerProfileOwnerModel>> GetAdminOrganizerProfileOwnersAsync(CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<OrganizerProfileOwnerModel> AssignAdminOrganizerProfileOwnerAsync(
        AssignOrganizerProfileOwnerRequest request,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();
}

internal sealed class WebCatalogApiClient(HttpClient httpClient) : IWebCatalogApiClient
{
    private const string ActorEmailHeaderName = "X-DarwinLingua-Actor-Email";

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

    public Task<IReadOnlyList<ScenarioLessonListItemModel>> GetScenariosAsync(CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<ScenarioLessonListItemModel>>(
            "/api/catalog/scenarios",
            cancellationToken);

    public Task<ScenarioLessonDetailModel?> GetScenarioBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        GetAsync<ScenarioLessonDetailModel>(
            BuildPath(
                $"/api/catalog/scenarios/{Uri.EscapeDataString(slug)}",
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

    public Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetConversationStarterPacksForScenarioAsync(
        string scenarioSlug,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<ConversationStarterPackListItemModel>>(
            $"/api/catalog/scenarios/{Uri.EscapeDataString(scenarioSlug)}/conversation-starters",
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

    public Task<IReadOnlyList<EventPreparationPackListItemModel>> GetEventPreparationPacksForScenarioAsync(
        string scenarioSlug,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<EventPreparationPackListItemModel>>(
            $"/api/catalog/scenarios/{Uri.EscapeDataString(scenarioSlug)}/event-preparation-packs",
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
        using HttpRequestMessage request = new(HttpMethod.Get, relativeUri);
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

        return JsonSerializer.Deserialize<T>(body, JsonSerializerOptions.Web);
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
        using HttpRequestMessage httpRequest = new(HttpMethod.Post, relativeUri);
        AddActorEmailHeader(httpRequest, actorEmail);
        httpRequest.Content = JsonContent.Create(request, options: JsonSerializerOptions.Web);
        using HttpResponseMessage response = await httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, relativeUri, cancellationToken).ConfigureAwait(false);

        TResponse? payload = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken).ConfigureAwait(false);
        return payload ?? throw new InvalidOperationException($"The Web API returned an empty payload for '{relativeUri}'.");
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
        throw new InvalidOperationException(
            $"The Web API call to '{relativeUri}' failed with status {(int)response.StatusCode}. Response: {body}");
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
    int ScenarioLessonCount,
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
    DateTime CreatedAtUtc);

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

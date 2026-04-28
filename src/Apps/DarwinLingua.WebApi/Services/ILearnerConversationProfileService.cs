using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

public interface ILearnerConversationProfileService
{
    Task<LearnerConversationProfilePrivateResponse?> GetPrivateAsync(
        string ownerEmail,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<LearnerConversationProfilePublicResponse>> GetPublicProfilesAsync(
        CancellationToken cancellationToken);

    Task<LearnerConversationProfilePrivateResponse> SaveAsync(
        string ownerEmail,
        SaveLearnerConversationProfileRequest request,
        CancellationToken cancellationToken);

    Task<LearnerConversationProfilePrivateResponse> SetEnabledAsync(
        string ownerEmail,
        LearnerConversationProfileVisibilityRequest request,
        CancellationToken cancellationToken);

    Task AnonymizeAsync(
        string ownerEmail,
        CancellationToken cancellationToken);
}

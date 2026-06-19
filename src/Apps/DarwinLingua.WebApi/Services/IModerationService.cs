using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

public interface IModerationService
{
    Task<UserReportResponse> SubmitReportAsync(
        string reporterEmail,
        SubmitUserReportRequest request,
        CancellationToken cancellationToken);

    Task<UserBlockResponse> BlockUserAsync(
        string blockerEmail,
        BlockUserRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<UserReportResponse>> GetReportsAsync(
        string? status,
        string? reason,
        string? targetType,
        string? assignedState,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ModerationDecisionAuditResponse>> GetDecisionAuditsAsync(
        CancellationToken cancellationToken);

    Task<UserReportResponse> DecideReportAsync(
        Guid reportId,
        ModerationDecisionRequest request,
        CancellationToken cancellationToken);
}

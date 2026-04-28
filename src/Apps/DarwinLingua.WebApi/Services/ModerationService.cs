using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

public sealed class ModerationService(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IModerationService
{
    public async Task<UserReportResponse> SubmitReportAsync(
        string reporterEmail,
        SubmitUserReportRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            await using DarwinLinguaDbContext dbContext = await dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            UserReport report = new(
                Guid.NewGuid(),
                reporterEmail,
                request.TargetType,
                request.TargetKey,
                request.ReportedUserEmail,
                request.Reason,
                request.Details,
                DateTime.UtcNow);

            dbContext.UserReports.Add(report);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return ToResponse(report);
        }
        catch (DomainRuleException exception)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    public async Task<UserBlockResponse> BlockUserAsync(
        string blockerEmail,
        BlockUserRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            string normalizedBlockerEmail = LearnerConversationProfile.NormalizeEmail(blockerEmail);

            await using DarwinLinguaDbContext dbContext = await dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            string normalizedBlockedEmail = await ResolveBlockedEmailAsync(
                    dbContext,
                    normalizedBlockerEmail,
                    request,
                    cancellationToken)
                .ConfigureAwait(false);

            UserBlock? existingBlock = await dbContext.UserBlocks
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    block => block.BlockerEmail == normalizedBlockerEmail && block.BlockedEmail == normalizedBlockedEmail,
                    cancellationToken)
                .ConfigureAwait(false);

            if (existingBlock is not null)
            {
                return ToResponse(existingBlock);
            }

            UserBlock block = new(
                Guid.NewGuid(),
                normalizedBlockerEmail,
                normalizedBlockedEmail,
                request.Reason,
                request.SourcePartnerRequestId,
                DateTime.UtcNow);

            dbContext.UserBlocks.Add(block);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return ToResponse(block);
        }
        catch (DomainRuleException exception)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    public async Task<IReadOnlyList<UserReportResponse>> GetReportsAsync(
        string? status,
        CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        IQueryable<UserReport> query = dbContext.UserReports.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status))
        {
            string normalizedStatus = ConversationEvent.NormalizeKey(status, "User report status");
            query = query.Where(report => report.Status == normalizedStatus);
        }

        UserReport[] reports = await query
            .OrderBy(report => report.Status)
            .ThenByDescending(report => report.CreatedAtUtc)
            .Take(200)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return reports.Select(ToResponse).ToArray();
    }

    public async Task<IReadOnlyList<ModerationDecisionAuditResponse>> GetDecisionAuditsAsync(
        CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        ModerationDecisionAudit[] audits = await dbContext.ModerationDecisionAudits
            .AsNoTracking()
            .OrderByDescending(audit => audit.CreatedAtUtc)
            .Take(200)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return audits.Select(ToResponse).ToArray();
    }

    public async Task<UserReportResponse> DecideReportAsync(
        Guid reportId,
        ModerationDecisionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            await using DarwinLinguaDbContext dbContext = await dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            UserReport report = await dbContext.UserReports
                .SingleOrDefaultAsync(item => item.Id == reportId, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new KeyNotFoundException("The selected user report could not be found.");

            DateTime nowUtc = DateTime.UtcNow;
            report.RecordDecision(request.Status, request.DecisionNote, request.DecidedBy, nowUtc);
            dbContext.ModerationDecisionAudits.Add(new ModerationDecisionAudit(
                Guid.NewGuid(),
                report.Id,
                report.Status,
                request.DecidedBy,
                request.DecisionNote,
                nowUtc));

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return ToResponse(report);
        }
        catch (DomainRuleException exception)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    private static UserReportResponse ToResponse(UserReport report) =>
        new(
            report.Id,
            report.ReporterEmail,
            report.TargetType,
            report.TargetKey,
            report.ReportedUserEmail,
            report.Reason,
            report.Details,
            report.Status,
            report.DecisionNote,
            report.DecidedBy,
            report.DecidedAtUtc,
            report.CreatedAtUtc,
            report.UpdatedAtUtc);

    private static UserBlockResponse ToResponse(UserBlock block) =>
        new(
            block.Id,
            block.BlockerEmail,
            block.BlockedEmail,
            block.Reason,
            block.SourcePartnerRequestId,
            block.CreatedAtUtc);

    private static ModerationDecisionAuditResponse ToResponse(ModerationDecisionAudit audit) =>
        new(
            audit.Id,
            audit.UserReportId,
            audit.DecisionStatus,
            audit.DecidedBy,
            audit.DecisionNote,
            audit.CreatedAtUtc);

    private static async Task<string> ResolveBlockedEmailAsync(
        DarwinLinguaDbContext dbContext,
        string blockerEmail,
        BlockUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.BlockedEmail))
        {
            return LearnerConversationProfile.NormalizeEmail(request.BlockedEmail);
        }

        if (request.TargetLearnerProfileId.HasValue)
        {
            LearnerConversationProfile profile = await dbContext.LearnerConversationProfiles
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.Id == request.TargetLearnerProfileId.Value, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new KeyNotFoundException("The selected learner profile could not be found.");

            return profile.OwnerEmail;
        }

        if (request.SourcePartnerRequestId.HasValue)
        {
            PartnerRequest partnerRequest = await dbContext.PartnerRequests
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.Id == request.SourcePartnerRequestId.Value, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new KeyNotFoundException("The selected partner request could not be found.");

            LearnerConversationProfile targetProfile = await dbContext.LearnerConversationProfiles
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.Id == partnerRequest.TargetLearnerProfileId, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new KeyNotFoundException("The selected partner request target profile could not be found.");

            if (partnerRequest.RequesterEmail == blockerEmail)
            {
                return targetProfile.OwnerEmail;
            }

            if (targetProfile.OwnerEmail == blockerEmail)
            {
                return partnerRequest.RequesterEmail;
            }
        }

        throw new InvalidOperationException("A blocked email, learner profile, or partner request is required.");
    }
}

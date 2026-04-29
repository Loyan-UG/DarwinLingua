using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Lexicon;
using DarwinLingua.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

public sealed class PartnerMatchingService(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IPartnerMatchingService
{
    private const int MaxNewRequestsPerDay = 5;

    public async Task<IReadOnlyList<PartnerMatchProfileResponse>> SearchAsync(
        string ownerEmail,
        PartnerMatchSearchRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        string normalizedEmail = LearnerConversationProfile.NormalizeEmail(ownerEmail);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        IQueryable<LearnerConversationProfile> query = dbContext.LearnerConversationProfiles
            .AsNoTracking()
            .Where(profile =>
                profile.OwnerEmail != normalizedEmail &&
                !dbContext.UserBlocks.AsNoTracking().Any(block =>
                    (block.BlockerEmail == normalizedEmail && block.BlockedEmail == profile.OwnerEmail) ||
                    (block.BlockerEmail == profile.OwnerEmail && block.BlockedEmail == normalizedEmail)) &&
                profile.HasConfirmedAdult &&
                (profile.Visibility == "public" || profile.Visibility == "request-only"));

        if (!string.IsNullOrWhiteSpace(request.CityRegion))
        {
            string cityRegion = request.CityRegion.Trim();
            query = query.Where(profile => profile.CityRegion != null && profile.CityRegion.Contains(cityRegion));
        }

        if (!string.IsNullOrWhiteSpace(request.InteractionPreference))
        {
            string interactionPreference = ConversationEvent.NormalizeKey(request.InteractionPreference, "Interaction preference");
            query = query.Where(profile => profile.InteractionPreference == interactionPreference || profile.InteractionPreference == "both");
        }

        if (!string.IsNullOrWhiteSpace(request.GermanLevel))
        {
            string germanLevel = NormalizeCefrLevel(request.GermanLevel);
            query = query.Where(profile => profile.GermanLevel == germanLevel);
        }

        if (!string.IsNullOrWhiteSpace(request.HelperLanguageCode))
        {
            string helperLanguageCode = ConversationEvent.NormalizeKey(request.HelperLanguageCode, "Helper language code");
            query = query.Where(profile => ("," + profile.HelperLanguageCodes + ",").Contains("," + helperLanguageCode + ","));
        }

        if (!string.IsNullOrWhiteSpace(request.GoalKeyword))
        {
            string goalKeyword = request.GoalKeyword.Trim();
            query = query.Where(profile => profile.ConversationGoals.Contains(goalKeyword));
        }

        LearnerConversationProfile[] profiles = await query
            .OrderBy(profile => profile.CityRegion)
            .ThenBy(profile => profile.GermanLevel)
            .ThenBy(profile => profile.DisplayName)
            .Take(50)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return profiles.Select(ToMatchProfile).ToArray();
    }

    public async Task<PartnerRequestResponse> SubmitRequestAsync(
        string requesterEmail,
        SubmitPartnerRequestRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await SubmitRequestCoreAsync(requesterEmail, request, cancellationToken).ConfigureAwait(false);
        }
        catch (DomainRuleException exception)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    public async Task<IReadOnlyList<PartnerRequestResponse>> GetRequestsAsync(
        string ownerEmail,
        CancellationToken cancellationToken)
    {
        string normalizedEmail = LearnerConversationProfile.NormalizeEmail(ownerEmail);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        LearnerConversationProfile? ownerProfile = await dbContext.LearnerConversationProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(profile => profile.OwnerEmail == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);

        IQueryable<PartnerRequest> query = dbContext.PartnerRequests.AsNoTracking()
            .Where(request => request.RequesterEmail == normalizedEmail);

        if (ownerProfile is not null)
        {
            query = query.Concat(dbContext.PartnerRequests.AsNoTracking()
                .Where(request => request.TargetLearnerProfileId == ownerProfile.Id));
        }

        PartnerRequest[] requests = await query
            .OrderByDescending(request => request.CreatedAtUtc)
            .Take(100)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        await ExpirePendingRequestsAsync(dbContext, requests, cancellationToken).ConfigureAwait(false);
        return await CreateResponsesAsync(dbContext, normalizedEmail, requests, cancellationToken).ConfigureAwait(false);
    }

    public async Task<PartnerRequestResponse> UpdateRequestStateAsync(
        string ownerEmail,
        Guid requestId,
        PartnerRequestStateUpdateRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            string normalizedEmail = LearnerConversationProfile.NormalizeEmail(ownerEmail);

            await using DarwinLinguaDbContext dbContext = await dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            PartnerRequest partnerRequest = await dbContext.PartnerRequests
                .SingleOrDefaultAsync(item => item.Id == requestId, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new KeyNotFoundException("The selected partner request could not be found.");

            LearnerConversationProfile targetProfile = await dbContext.LearnerConversationProfiles
                .AsNoTracking()
                .SingleOrDefaultAsync(profile => profile.Id == partnerRequest.TargetLearnerProfileId, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new KeyNotFoundException("The selected partner request target profile could not be found.");

            EnsureRequestParticipant(partnerRequest, targetProfile, normalizedEmail);

            DateTime nowUtc = DateTime.UtcNow;
            if (partnerRequest.Status == PartnerRequestStatuses.Pending && partnerRequest.ExpiresAtUtc <= nowUtc)
            {
                partnerRequest.Expire(nowUtc);
            }
            else
            {
                ApplyStateChange(partnerRequest, targetProfile, normalizedEmail, request.Action, nowUtc);
                if (ConversationEvent.NormalizeKey(request.Action, "Partner request action") == "block")
                {
                    bool alreadyBlocked = await dbContext.UserBlocks
                        .AsNoTracking()
                        .AnyAsync(
                            block => block.BlockerEmail == normalizedEmail && block.BlockedEmail == partnerRequest.RequesterEmail,
                            cancellationToken)
                        .ConfigureAwait(false);

                    if (!alreadyBlocked)
                    {
                        dbContext.UserBlocks.Add(new UserBlock(
                            Guid.NewGuid(),
                            normalizedEmail,
                            partnerRequest.RequesterEmail,
                            "Blocked from partner request.",
                            partnerRequest.Id,
                            nowUtc));
                    }
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return (await CreateResponsesAsync(dbContext, normalizedEmail, [partnerRequest], cancellationToken).ConfigureAwait(false))[0];
        }
        catch (DomainRuleException exception)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    private async Task<PartnerRequestResponse> SubmitRequestCoreAsync(
        string requesterEmail,
        SubmitPartnerRequestRequest request,
        CancellationToken cancellationToken)
    {
        string normalizedEmail = LearnerConversationProfile.NormalizeEmail(requesterEmail);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        LearnerConversationProfile requesterProfile = await dbContext.LearnerConversationProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(profile => profile.OwnerEmail == normalizedEmail, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Create a learner profile before sending partner requests.");

        if (requesterProfile.Visibility == "disabled" || !requesterProfile.HasConfirmedAdult)
        {
            throw new InvalidOperationException("Enable an adult learner profile before sending partner requests.");
        }

        LearnerConversationProfile targetProfile = await dbContext.LearnerConversationProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(profile => profile.Id == request.TargetLearnerProfileId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException("The selected learner profile could not be found.");

        if (targetProfile.OwnerEmail == normalizedEmail)
        {
            throw new InvalidOperationException("You cannot send a partner request to your own profile.");
        }

        if (!targetProfile.HasConfirmedAdult || targetProfile.Visibility is not ("public" or "request-only"))
        {
            throw new InvalidOperationException("The selected learner profile is not accepting partner requests.");
        }

        bool isSuppressed = await dbContext.UserBlocks
            .AsNoTracking()
            .AnyAsync(
                block =>
                    (block.BlockerEmail == normalizedEmail && block.BlockedEmail == targetProfile.OwnerEmail) ||
                    (block.BlockerEmail == targetProfile.OwnerEmail && block.BlockedEmail == normalizedEmail),
                cancellationToken)
            .ConfigureAwait(false);

        if (isSuppressed)
        {
            throw new InvalidOperationException("Partner requests are not available for this learner profile.");
        }

        DateTime nowUtc = DateTime.UtcNow;
        DateTime sinceUtc = nowUtc.AddDays(-1);
        int recentRequestCount = await dbContext.PartnerRequests
            .AsNoTracking()
            .CountAsync(item => item.RequesterEmail == normalizedEmail && item.CreatedAtUtc >= sinceUtc, cancellationToken)
            .ConfigureAwait(false);

        if (recentRequestCount >= MaxNewRequestsPerDay)
        {
            throw new InvalidOperationException("Daily partner request limit reached.");
        }

        bool duplicatePending = await dbContext.PartnerRequests
            .AsNoTracking()
            .AnyAsync(
                item =>
                    item.RequesterEmail == normalizedEmail &&
                    item.TargetLearnerProfileId == targetProfile.Id &&
                    item.Status == PartnerRequestStatuses.Pending,
                cancellationToken)
            .ConfigureAwait(false);

        if (duplicatePending)
        {
            throw new InvalidOperationException("A pending partner request already exists for this learner.");
        }

        PartnerRequest partnerRequest = new(
            Guid.NewGuid(),
            normalizedEmail,
            targetProfile.Id,
            request.OpenerTemplateKey,
            request.Note,
            nowUtc,
            nowUtc.AddDays(14));

        dbContext.PartnerRequests.Add(partnerRequest);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return (await CreateResponsesAsync(dbContext, normalizedEmail, [partnerRequest], cancellationToken).ConfigureAwait(false))[0];
    }

    private static void ApplyStateChange(
        PartnerRequest partnerRequest,
        LearnerConversationProfile targetProfile,
        string ownerEmail,
        string action,
        DateTime nowUtc)
    {
        string normalizedAction = ConversationEvent.NormalizeKey(action, "Partner request action");

        if (normalizedAction == "cancel")
        {
            if (partnerRequest.RequesterEmail != ownerEmail)
            {
                throw new InvalidOperationException("Only the requester can cancel a partner request.");
            }

            partnerRequest.Cancel(nowUtc);
            return;
        }

        if (targetProfile.OwnerEmail != ownerEmail)
        {
            throw new InvalidOperationException("Only the request recipient can update this partner request.");
        }

        switch (normalizedAction)
        {
            case "accept":
                partnerRequest.Accept(nowUtc);
                break;
            case "decline":
                partnerRequest.Decline(nowUtc);
                break;
            case "block":
                partnerRequest.Block(nowUtc);
                break;
            default:
                throw new InvalidOperationException("Partner request action is not supported.");
        }
    }

    private static void EnsureRequestParticipant(
        PartnerRequest partnerRequest,
        LearnerConversationProfile targetProfile,
        string ownerEmail)
    {
        if (partnerRequest.RequesterEmail == ownerEmail || targetProfile.OwnerEmail == ownerEmail)
        {
            return;
        }

        throw new InvalidOperationException("Only the requester or recipient can update this partner request.");
    }

    private static async Task ExpirePendingRequestsAsync(
        DarwinLinguaDbContext dbContext,
        IReadOnlyList<PartnerRequest> requests,
        CancellationToken cancellationToken)
    {
        DateTime nowUtc = DateTime.UtcNow;
        PartnerRequest[] expiredRequests = requests
            .Where(request => request.Status == PartnerRequestStatuses.Pending && request.ExpiresAtUtc <= nowUtc)
            .ToArray();

        if (expiredRequests.Length == 0)
        {
            return;
        }

        foreach (PartnerRequest request in expiredRequests)
        {
            dbContext.Attach(request);
            request.Expire(nowUtc);
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<PartnerRequestResponse>> CreateResponsesAsync(
        DarwinLinguaDbContext dbContext,
        string ownerEmail,
        IReadOnlyList<PartnerRequest> requests,
        CancellationToken cancellationToken)
    {
        Guid[] targetProfileIds = requests.Select(request => request.TargetLearnerProfileId).Distinct().ToArray();
        LearnerConversationProfile[] targetProfiles = await dbContext.LearnerConversationProfiles
            .AsNoTracking()
            .Where(profile => targetProfileIds.Contains(profile.Id))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        string[] requesterEmails = requests.Select(request => request.RequesterEmail).Distinct(StringComparer.Ordinal).ToArray();
        LearnerConversationProfile[] requesterProfiles = await dbContext.LearnerConversationProfiles
            .AsNoTracking()
            .Where(profile => requesterEmails.Contains(profile.OwnerEmail))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<Guid, LearnerConversationProfile> targetById = targetProfiles.ToDictionary(profile => profile.Id);
        Dictionary<string, LearnerConversationProfile> requesterByEmail = requesterProfiles.ToDictionary(profile => profile.OwnerEmail, StringComparer.Ordinal);

        return requests.Select(request =>
        {
            LearnerConversationProfile targetProfile = targetById[request.TargetLearnerProfileId];
            bool isIncoming = targetProfile.OwnerEmail == ownerEmail;
            LearnerConversationProfile? otherProfile = isIncoming
                ? requesterByEmail.GetValueOrDefault(request.RequesterEmail)
                : targetProfile;
            string otherEmail = isIncoming ? request.RequesterEmail : targetProfile.OwnerEmail;
            string? contactEmail = request.Status == PartnerRequestStatuses.Accepted ? otherEmail : null;

            return new PartnerRequestResponse(
                request.Id,
                isIncoming ? "incoming" : "outgoing",
                request.TargetLearnerProfileId,
                otherProfile?.DisplayName ?? otherEmail,
                otherProfile?.CityRegion,
                request.OpenerTemplateKey,
                request.Note,
                request.Status,
                request.CreatedAtUtc,
                request.UpdatedAtUtc,
                request.ExpiresAtUtc,
                request.RespondedAtUtc,
                contactEmail);
        }).ToArray();
    }

    private static PartnerMatchProfileResponse ToMatchProfile(LearnerConversationProfile profile) =>
        new(
            profile.Id,
            profile.DisplayName,
            profile.CityRegion,
            profile.InteractionPreference,
            profile.GermanLevel,
            SplitLanguageCodes(profile.HelperLanguageCodes),
            profile.ConversationGoals,
            profile.Visibility);

    private static string NormalizeCefrLevel(string value)
    {
        if (!Enum.TryParse(value, true, out CefrLevel level))
        {
            throw new InvalidOperationException($"'{value}' is not a supported CEFR level.");
        }

        return level.ToString().ToUpperInvariant();
    }

    private static IReadOnlyList<string> SplitLanguageCodes(string value) =>
        value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
}

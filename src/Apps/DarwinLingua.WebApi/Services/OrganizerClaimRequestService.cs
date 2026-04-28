using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

public sealed class OrganizerClaimRequestService(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IOrganizerClaimRequestService
{
    public async Task<OrganizerClaimRequestResponse> SubmitAsync(
        string organizerProfileSlug,
        SubmitOrganizerClaimRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(organizerProfileSlug);
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            await using DarwinLinguaDbContext dbContext = await dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            string normalizedSlug = ConversationEvent.NormalizeKey(organizerProfileSlug, "Organizer profile slug");
            bool organizerExists = await dbContext.OrganizerProfiles
                .AsNoTracking()
                .AnyAsync(profile => profile.Slug == normalizedSlug && profile.PublicationStatus == PublicationStatus.Active, cancellationToken)
                .ConfigureAwait(false);

            if (!organizerExists)
            {
                throw new KeyNotFoundException($"No published organizer profile was found for '{normalizedSlug}'.");
            }

            OrganizerClaimRequest claimRequest = new(
                Guid.NewGuid(),
                normalizedSlug,
                request.RequesterName,
                request.RequesterEmail,
                request.RelationshipToOrganizer,
                request.EvidenceText,
                OrganizerClaimRequestStatuses.Submitted,
                DateTime.UtcNow);

            dbContext.OrganizerClaimRequests.Add(claimRequest);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return CreateResponse(claimRequest);
        }
        catch (DomainRuleException exception)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    public async Task<IReadOnlyList<OrganizerClaimRequestResponse>> GetRecentAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        OrganizerClaimRequest[] claimRequests = await dbContext.OrganizerClaimRequests
            .AsNoTracking()
            .OrderByDescending(claimRequest => claimRequest.CreatedAtUtc)
            .Take(100)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return claimRequests.Select(CreateResponse).ToArray();
    }

    private static OrganizerClaimRequestResponse CreateResponse(OrganizerClaimRequest claimRequest) =>
        new(
            claimRequest.Id,
            claimRequest.OrganizerProfileSlug,
            claimRequest.RequesterName,
            claimRequest.RequesterEmail,
            claimRequest.RelationshipToOrganizer,
            claimRequest.EvidenceText,
            claimRequest.Status,
            claimRequest.CreatedAtUtc);
}

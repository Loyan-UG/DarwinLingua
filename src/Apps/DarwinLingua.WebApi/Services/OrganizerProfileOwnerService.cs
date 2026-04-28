using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

public sealed class OrganizerProfileOwnerService(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IOrganizerProfileOwnerService
{
    public async Task<OrganizerProfileOwnerResponse> AssignAsync(
        AssignOrganizerProfileOwnerRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            await using DarwinLinguaDbContext dbContext = await dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            string normalizedSlug = ConversationEvent.NormalizeKey(request.OrganizerProfileSlug, "Organizer profile slug");
            bool organizerExists = await dbContext.OrganizerProfiles
                .AsNoTracking()
                .AnyAsync(profile => profile.Slug == normalizedSlug && profile.PublicationStatus == PublicationStatus.Active, cancellationToken)
                .ConfigureAwait(false);

            if (!organizerExists)
            {
                throw new KeyNotFoundException($"No published organizer profile was found for '{normalizedSlug}'.");
            }

            string normalizedOwnerEmail = OrganizerProfileOwner.NormalizeEmail(request.OwnerEmail);
            OrganizerProfileOwner? existingOwner = await dbContext.OrganizerProfileOwners
                .SingleOrDefaultAsync(
                    owner => owner.OrganizerProfileSlug == normalizedSlug && owner.OwnerEmail == normalizedOwnerEmail,
                    cancellationToken)
                .ConfigureAwait(false);

            if (existingOwner is not null)
            {
                return CreateResponse(existingOwner);
            }

            OrganizerProfileOwner owner = new(
                Guid.NewGuid(),
                normalizedSlug,
                normalizedOwnerEmail,
                request.AssignedBy,
                DateTime.UtcNow);

            dbContext.OrganizerProfileOwners.Add(owner);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return CreateResponse(owner);
        }
        catch (DomainRuleException exception)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    public async Task<IReadOnlyList<OrganizerProfileOwnerResponse>> GetByOwnerEmailAsync(
        string ownerEmail,
        CancellationToken cancellationToken)
    {
        string normalizedOwnerEmail = OrganizerProfileOwner.NormalizeEmail(ownerEmail);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        OrganizerProfileOwner[] owners = await dbContext.OrganizerProfileOwners
            .AsNoTracking()
            .Where(owner => owner.OwnerEmail == normalizedOwnerEmail)
            .OrderBy(owner => owner.OrganizerProfileSlug)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return owners.Select(CreateResponse).ToArray();
    }

    public async Task<IReadOnlyList<OrganizerProfileOwnerResponse>> GetRecentAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        OrganizerProfileOwner[] owners = await dbContext.OrganizerProfileOwners
            .AsNoTracking()
            .OrderByDescending(owner => owner.CreatedAtUtc)
            .Take(100)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return owners.Select(CreateResponse).ToArray();
    }

    private static OrganizerProfileOwnerResponse CreateResponse(OrganizerProfileOwner owner) =>
        new(owner.Id, owner.OrganizerProfileSlug, owner.OwnerEmail, owner.AssignedBy, owner.CreatedAtUtc);
}

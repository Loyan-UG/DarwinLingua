using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Lexicon;
using DarwinLingua.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

public sealed class OrganizerProfileAdminService(
    IDbContextFactory<DarwinLinguaDbContext> dbContextFactory,
    IOrganizerProfileQueryService organizerProfileQueryService) : IOrganizerProfileAdminService
{
    public async Task<OrganizerProfileDetailModel> SaveAsync(
        AdminSaveOrganizerProfileRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await SaveCoreAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (DomainRuleException exception)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    private async Task<OrganizerProfileDetailModel> SaveCoreAsync(
        AdminSaveOrganizerProfileRequest request,
        CancellationToken cancellationToken)
    {
        DateTime nowUtc = DateTime.UtcNow;
        CefrLevel[] levels = ParseLevels(request.SupportedLearnerLevels);
        string[] helperLanguageCodes = NormalizeKeys(request.HelperLanguageCodes, "helper language code");

        OrganizerProfile organizerProfile = new(
            Guid.NewGuid(),
            request.Slug,
            request.DisplayName,
            request.OrganizerType,
            request.Description,
            request.CityRegion,
            request.IsOnlineAvailable,
            request.WebsiteUrl,
            request.PublicContactMethod,
            request.VerificationStatus,
            request.PlanKey,
            PublicationStatus.Active,
            request.HistoricalEventCount,
            nowUtc);

        for (int index = 0; index < levels.Length; index++)
        {
            organizerProfile.AddSupportedLevel(Guid.NewGuid(), levels[index], index + 1, nowUtc);
        }

        for (int index = 0; index < helperLanguageCodes.Length; index++)
        {
            organizerProfile.AddHelperLanguage(Guid.NewGuid(), helperLanguageCodes[index], index + 1, nowUtc);
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        string normalizedSlug = organizerProfile.Slug;
        OrganizerProfile? existingProfile = await dbContext.OrganizerProfiles
            .Include(item => item.SupportedLevels)
            .Include(item => item.HelperLanguages)
            .SingleOrDefaultAsync(item => item.Slug == normalizedSlug, cancellationToken)
            .ConfigureAwait(false);

        if (existingProfile is not null)
        {
            dbContext.OrganizerProfiles.Remove(existingProfile);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        dbContext.OrganizerProfiles.Add(organizerProfile);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await organizerProfileQueryService
            .GetPublishedOrganizerProfileBySlugAsync(normalizedSlug, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("The saved organizer profile could not be loaded.");
    }

    private static CefrLevel[] ParseLevels(IReadOnlyList<string> values)
    {
        if (values.Count == 0)
        {
            throw new InvalidOperationException("At least one learner level is required.");
        }

        List<CefrLevel> levels = [];
        foreach (string value in values.Select(value => value.Trim()).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!Enum.TryParse(value, true, out CefrLevel level))
            {
                throw new InvalidOperationException($"'{value}' is not a supported CEFR level.");
            }

            levels.Add(level);
        }

        return levels.Count == 0
            ? throw new InvalidOperationException("At least one learner level is required.")
            : levels.ToArray();
    }

    private static string[] NormalizeKeys(IReadOnlyList<string> values, string label) =>
        values
            .Select(value => ConversationEvent.NormalizeKey(value, label))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
}

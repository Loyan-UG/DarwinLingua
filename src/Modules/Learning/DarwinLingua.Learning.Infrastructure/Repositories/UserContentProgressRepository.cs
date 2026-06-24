using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Learning.Infrastructure.Repositories;

internal sealed class UserContentProgressRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory)
    : IUserContentProgressRepository
{
    public async Task<UserContentProgress?> GetByUserAndContentAsync(
        string userId,
        string targetLearningLanguageCode,
        string contentOwnerType,
        string contentOwnerSlug,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetLearningLanguageCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentOwnerType);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentOwnerSlug);

        string normalizedTargetLearningLanguageCode = targetLearningLanguageCode.Trim().ToLowerInvariant();
        string normalizedOwnerType = contentOwnerType.Trim().ToLowerInvariant();
        string normalizedOwnerSlug = contentOwnerSlug.Trim().ToLowerInvariant();

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        return await dbContext.UserContentProgress
            .SingleOrDefaultAsync(
                item =>
                    item.UserId == userId &&
                    item.TargetLearningLanguageCode == normalizedTargetLearningLanguageCode &&
                    item.ContentOwnerType == normalizedOwnerType &&
                    item.ContentOwnerSlug == normalizedOwnerSlug,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task AddAsync(UserContentProgress progress, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(progress);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        dbContext.UserContentProgress.Add(progress);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(UserContentProgress progress, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(progress);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        dbContext.UserContentProgress.Update(progress);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<UserContentProgress>> GetUserProgressAsync(
        string userId,
        string targetLearningLanguageCode,
        int recentItemCount,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetLearningLanguageCode);
        if (recentItemCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(recentItemCount), "Recent item count must be positive.");
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        string normalizedTargetLearningLanguageCode = targetLearningLanguageCode.Trim().ToLowerInvariant();
        return await dbContext.UserContentProgress
            .AsNoTracking()
            .Where(item =>
                item.UserId == userId &&
                item.TargetLearningLanguageCode == normalizedTargetLearningLanguageCode)
            .OrderByDescending(item => item.UpdatedAtUtc)
            .Take(recentItemCount)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Services;

public interface IWebActivityQueryService
{
    Task<IReadOnlyList<RecentWordActivityItemViewModel>> GetRecentWordActivityAsync(
        string userId,
        int take,
        CancellationToken cancellationToken);
}

internal sealed class WebActivityQueryService(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IWebActivityQueryService
{
    public async Task<IReadOnlyList<RecentWordActivityItemViewModel>> GetRecentWordActivityAsync(
        string userId,
        int take,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        return await (
            from state in dbContext.UserWordStates.AsNoTracking()
            join word in dbContext.WordEntries.AsNoTracking() on state.WordEntryPublicId equals word.PublicId
            where state.UserId == userId && state.LastViewedAtUtc != null
            orderby state.LastViewedAtUtc descending
            select new RecentWordActivityItemViewModel(
                word.PublicId,
                word.Lemma,
                word.Article,
                word.PluralForm,
                word.PartOfSpeech.ToString(),
                word.PrimaryCefrLevel.ToString(),
                state.ViewCount,
                state.IsKnown,
                state.IsDifficult,
                state.LastViewedAtUtc!.Value))
            .Take(take)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

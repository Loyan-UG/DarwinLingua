using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.Web.Data;
using DarwinLingua.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Services;

public interface IWebActivityQueryService
{
    Task<IReadOnlyList<RecentWordActivityItemViewModel>> GetRecentWordActivityAsync(
        string actorId,
        int take,
        CancellationToken cancellationToken);
}

internal sealed class WebActivityQueryService(
    WebIdentityDbContext identityDbContext,
    IDbContextFactory<DarwinLinguaDbContext> catalogDbContextFactory) : IWebActivityQueryService
{
    public async Task<IReadOnlyList<RecentWordActivityItemViewModel>> GetRecentWordActivityAsync(
        string actorId,
        int take,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actorId);

        RecentWordStateProjection[] recentStates = await identityDbContext.UserWordStates
            .AsNoTracking()
            .Where(state => state.ActorId == actorId && state.LastViewedAtUtc != null)
            .OrderByDescending(state => state.LastViewedAtUtc)
            .Take(take)
            .Select(state => new RecentWordStateProjection(
                state.WordPublicId,
                state.ViewCount,
                state.IsKnown,
                state.IsDifficult,
                state.LastViewedAtUtc!.Value))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        if (recentStates.Length == 0)
        {
            return [];
        }

        Guid[] wordIds = recentStates.Select(item => item.WordPublicId).ToArray();

        await using DarwinLinguaDbContext catalogDbContext = await catalogDbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        CatalogWordProjection[] words = await catalogDbContext.WordEntries
            .AsNoTracking()
            .Where(word => wordIds.Contains(word.PublicId) && word.PublicationStatus == PublicationStatus.Active)
            .Select(word => new CatalogWordProjection(
                word.PublicId,
                word.Lemma,
                word.Article,
                word.PluralForm,
                word.PartOfSpeech.ToString(),
                word.PrimaryCefrLevel.ToString()))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<Guid, CatalogWordProjection> wordMap = words.ToDictionary(item => item.PublicId);

        return recentStates
            .Where(state => wordMap.ContainsKey(state.WordPublicId))
            .Select(state =>
            {
                CatalogWordProjection word = wordMap[state.WordPublicId];

                return new RecentWordActivityItemViewModel(
                    word.PublicId,
                    word.Lemma,
                    word.Article,
                    word.PluralForm,
                    word.PartOfSpeech,
                    word.CefrLevel,
                    state.ViewCount,
                    state.IsKnown,
                    state.IsDifficult,
                    state.LastViewedAtUtc);
            })
            .ToArray();
    }

    private sealed record RecentWordStateProjection(
        Guid WordPublicId,
        int ViewCount,
        bool IsKnown,
        bool IsDifficult,
        DateTime LastViewedAtUtc);

    private sealed record CatalogWordProjection(
        Guid PublicId,
        string Lemma,
        string? Article,
        string? PluralForm,
        string PartOfSpeech,
        string CefrLevel);
}

using DarwinLingua.Web.Data;
using DarwinLingua.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Services;

public interface IWebActivityQueryService
{
    Task<IReadOnlyList<RecentWordActivityItemViewModel>> GetRecentWordActivityAsync(
        string actorId,
        string meaningLanguageCode,
        int take,
        CancellationToken cancellationToken);
}

internal sealed class WebActivityQueryService(
    WebIdentityDbContext identityDbContext,
    IWebCatalogApiClient catalogApiClient) : IWebActivityQueryService
{
    public async Task<IReadOnlyList<RecentWordActivityItemViewModel>> GetRecentWordActivityAsync(
        string actorId,
        string meaningLanguageCode,
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

        IReadOnlyList<DarwinLingua.Catalog.Application.Models.WordListItemModel> words = await catalogApiClient
            .GetWordsByIdsAsync(wordIds, meaningLanguageCode, cancellationToken)
            .ConfigureAwait(false);

        Dictionary<Guid, DarwinLingua.Catalog.Application.Models.WordListItemModel> wordMap = words.ToDictionary(item => item.PublicId);

        return recentStates
            .Where(state => wordMap.ContainsKey(state.WordPublicId))
            .Select(state =>
            {
                DarwinLingua.Catalog.Application.Models.WordListItemModel word = wordMap[state.WordPublicId];

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
}

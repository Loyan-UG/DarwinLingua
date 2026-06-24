using DarwinLingua.Web.Data;
using DarwinLingua.Web.Models;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Services;

public interface IWebActivityQueryService
{
    Task<IReadOnlyList<RecentWordActivityItemViewModel>> GetRecentWordActivityAsync(
        string actorId,
        string targetLearningLanguageCode,
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
        string targetLearningLanguageCode,
        string meaningLanguageCode,
        int take,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actorId);
        string normalizedTargetLearningLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(
            targetLearningLanguageCode,
            "Recent word activity target learning language");

        RecentWordStateProjection[] recentStates = await identityDbContext.UserWordStates
            .AsNoTracking()
            .Where(state =>
                state.ActorId == actorId &&
                state.TargetLearningLanguageCode == normalizedTargetLearningLanguageCode &&
                state.LastViewedAtUtc != null)
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
            .GetWordsByIdsAsync(wordIds, targetLearningLanguageCode, meaningLanguageCode, cancellationToken)
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

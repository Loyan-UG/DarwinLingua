using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Services;

public interface IWebFavoriteWordService
{
    Task<bool> IsFavoriteAsync(Guid wordPublicId, CancellationToken cancellationToken);

    Task ToggleFavoriteAsync(Guid wordPublicId, CancellationToken cancellationToken);

    Task<IReadOnlyList<FavoriteWordListItemModel>> GetFavoriteWordsAsync(
        string meaningLanguageCode,
        CancellationToken cancellationToken);
}

internal sealed class WebFavoriteWordService(
    IWebActorContextAccessor actorContextAccessor,
    WebIdentityDbContext identityDbContext,
    IWebCatalogApiClient catalogApiClient,
    IWebEntitledFeatureAccessService featureAccessService) : IWebFavoriteWordService
{
    public async Task<bool> IsFavoriteAsync(Guid wordPublicId, CancellationToken cancellationToken)
    {
        if (!await featureAccessService.CanUseFavoritesAsync(cancellationToken).ConfigureAwait(false))
        {
            return false;
        }

        WebActorContext actor = actorContextAccessor.GetCurrentActor();

        return await identityDbContext.UserFavoriteWords
            .AsNoTracking()
            .AnyAsync(item => item.ActorId == actor.ActorId && item.WordPublicId == wordPublicId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task ToggleFavoriteAsync(Guid wordPublicId, CancellationToken cancellationToken)
    {
        await featureAccessService.EnsureCanUseFavoritesAsync(cancellationToken).ConfigureAwait(false);

        WebActorContext actor = actorContextAccessor.GetCurrentActor();

        WebUserFavoriteWord? existing = await identityDbContext.UserFavoriteWords
            .SingleOrDefaultAsync(item => item.ActorId == actor.ActorId && item.WordPublicId == wordPublicId, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            identityDbContext.UserFavoriteWords.Add(new WebUserFavoriteWord
            {
                Id = Guid.NewGuid(),
                ActorId = actor.ActorId,
                WordPublicId = wordPublicId,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
        else
        {
            identityDbContext.UserFavoriteWords.Remove(existing);
        }

        await identityDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<FavoriteWordListItemModel>> GetFavoriteWordsAsync(
        string meaningLanguageCode,
        CancellationToken cancellationToken)
    {
        await featureAccessService.EnsureCanUseFavoritesAsync(cancellationToken).ConfigureAwait(false);

        WebActorContext actor = actorContextAccessor.GetCurrentActor();

        Guid[] favoriteWordIds = await identityDbContext.UserFavoriteWords
            .AsNoTracking()
            .Where(item => item.ActorId == actor.ActorId)
            .OrderByDescending(item => item.CreatedAtUtc)
            .Select(item => item.WordPublicId)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        if (favoriteWordIds.Length == 0)
        {
            return [];
        }

        IReadOnlyList<DarwinLingua.Catalog.Application.Models.WordListItemModel> projections = await catalogApiClient
            .GetWordsByIdsAsync(favoriteWordIds, meaningLanguageCode, cancellationToken)
            .ConfigureAwait(false);

        Dictionary<Guid, DarwinLingua.Catalog.Application.Models.WordListItemModel> map = projections.ToDictionary(item => item.PublicId);

        return favoriteWordIds
            .Where(map.ContainsKey)
            .Select(id => map[id])
            .Select(item => new FavoriteWordListItemModel(
                item.PublicId,
                item.Lemma,
                item.Article,
                item.PartOfSpeech,
                item.CefrLevel,
                item.PrimaryMeaning))
            .ToArray();
    }
}

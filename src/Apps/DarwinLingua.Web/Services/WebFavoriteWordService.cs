using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.SharedKernel.Content;
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
    IDbContextFactory<DarwinLinguaDbContext> catalogDbContextFactory) : IWebFavoriteWordService
{
    public async Task<bool> IsFavoriteAsync(Guid wordPublicId, CancellationToken cancellationToken)
    {
        WebActorContext actor = actorContextAccessor.GetCurrentActor();

        return await identityDbContext.UserFavoriteWords
            .AsNoTracking()
            .AnyAsync(item => item.ActorId == actor.ActorId && item.WordPublicId == wordPublicId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task ToggleFavoriteAsync(Guid wordPublicId, CancellationToken cancellationToken)
    {
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

        await using DarwinLinguaDbContext catalogDbContext = await catalogDbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        LanguageAwareFavoriteProjection[] projections = await (
            from word in catalogDbContext.WordEntries.AsNoTracking()
            where favoriteWordIds.Contains(word.PublicId) && word.PublicationStatus == PublicationStatus.Active
            select new LanguageAwareFavoriteProjection(
                word.PublicId,
                word.Lemma,
                word.Article,
                word.PartOfSpeech.ToString(),
                word.PrimaryCefrLevel.ToString(),
                word.Senses
                    .OrderByDescending(sense => sense.IsPrimarySense)
                    .ThenBy(sense => sense.SenseOrder)
                    .SelectMany(sense => sense.Translations)
                    .Where(translation => translation.LanguageCode.Value == meaningLanguageCode)
                    .OrderByDescending(translation => translation.IsPrimary)
                    .Select(translation => translation.TranslationText)
                    .FirstOrDefault()))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<Guid, LanguageAwareFavoriteProjection> map = projections.ToDictionary(item => item.PublicId);

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

    private sealed record LanguageAwareFavoriteProjection(
        Guid PublicId,
        string Lemma,
        string? Article,
        string PartOfSpeech,
        string CefrLevel,
        string? PrimaryMeaning);
}

using DarwinLingua.Learning.Application.Models;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Services;

public interface IWebUserWordStateService
{
    Task<UserWordStateModel?> GetWordStateAsync(Guid wordPublicId, string targetLearningLanguageCode, CancellationToken cancellationToken);

    Task<UserWordStateModel> TrackWordViewedAsync(Guid wordPublicId, string targetLearningLanguageCode, CancellationToken cancellationToken);

    Task<UserWordStateModel> MarkWordKnownAsync(Guid wordPublicId, string targetLearningLanguageCode, CancellationToken cancellationToken);

    Task<UserWordStateModel> MarkWordDifficultAsync(Guid wordPublicId, string targetLearningLanguageCode, CancellationToken cancellationToken);

    Task<UserWordStateModel> ClearWordKnownStateAsync(Guid wordPublicId, string targetLearningLanguageCode, CancellationToken cancellationToken);

    Task<UserWordStateModel> ClearWordDifficultStateAsync(Guid wordPublicId, string targetLearningLanguageCode, CancellationToken cancellationToken);
}

internal sealed class WebUserWordStateService(
    IWebActorContextAccessor actorContextAccessor,
    WebIdentityDbContext dbContext) : IWebUserWordStateService
{
    public async Task<UserWordStateModel?> GetWordStateAsync(Guid wordPublicId, string targetLearningLanguageCode, CancellationToken cancellationToken)
    {
        WebActorContext actor = actorContextAccessor.GetCurrentActor();
        string normalizedTargetLearningLanguageCode = NormalizeTargetLearningLanguageCode(targetLearningLanguageCode);

        WebUserWordState? state = await dbContext.UserWordStates
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item =>
                    item.ActorId == actor.ActorId &&
                    item.TargetLearningLanguageCode == normalizedTargetLearningLanguageCode &&
                    item.WordPublicId == wordPublicId,
                cancellationToken)
            .ConfigureAwait(false);

        return state is null ? null : Map(state);
    }

    public Task<UserWordStateModel> TrackWordViewedAsync(Guid wordPublicId, string targetLearningLanguageCode, CancellationToken cancellationToken) =>
        UpdateAsync(
            wordPublicId,
            targetLearningLanguageCode,
            static state =>
            {
                DateTime now = DateTime.UtcNow;
                state.FirstViewedAtUtc ??= now;
                state.LastViewedAtUtc = now;
                state.ViewCount++;
                state.UpdatedAtUtc = now;
            },
            cancellationToken);

    public Task<UserWordStateModel> MarkWordKnownAsync(Guid wordPublicId, string targetLearningLanguageCode, CancellationToken cancellationToken) =>
        UpdateAsync(
            wordPublicId,
            targetLearningLanguageCode,
            static state =>
            {
                state.IsKnown = true;
                state.UpdatedAtUtc = DateTime.UtcNow;
            },
            cancellationToken);

    public Task<UserWordStateModel> MarkWordDifficultAsync(Guid wordPublicId, string targetLearningLanguageCode, CancellationToken cancellationToken) =>
        UpdateAsync(
            wordPublicId,
            targetLearningLanguageCode,
            static state =>
            {
                state.IsDifficult = true;
                state.UpdatedAtUtc = DateTime.UtcNow;
            },
            cancellationToken);

    public Task<UserWordStateModel> ClearWordKnownStateAsync(Guid wordPublicId, string targetLearningLanguageCode, CancellationToken cancellationToken) =>
        UpdateAsync(
            wordPublicId,
            targetLearningLanguageCode,
            static state =>
            {
                state.IsKnown = false;
                state.UpdatedAtUtc = DateTime.UtcNow;
            },
            cancellationToken);

    public Task<UserWordStateModel> ClearWordDifficultStateAsync(Guid wordPublicId, string targetLearningLanguageCode, CancellationToken cancellationToken) =>
        UpdateAsync(
            wordPublicId,
            targetLearningLanguageCode,
            static state =>
            {
                state.IsDifficult = false;
                state.UpdatedAtUtc = DateTime.UtcNow;
            },
            cancellationToken);

    private async Task<UserWordStateModel> UpdateAsync(
        Guid wordPublicId,
        string targetLearningLanguageCode,
        Action<WebUserWordState> update,
        CancellationToken cancellationToken)
    {
        WebActorContext actor = actorContextAccessor.GetCurrentActor();
        string normalizedTargetLearningLanguageCode = NormalizeTargetLearningLanguageCode(targetLearningLanguageCode);

        WebUserWordState? state = await dbContext.UserWordStates
            .SingleOrDefaultAsync(
                item =>
                    item.ActorId == actor.ActorId &&
                    item.TargetLearningLanguageCode == normalizedTargetLearningLanguageCode &&
                    item.WordPublicId == wordPublicId,
                cancellationToken)
            .ConfigureAwait(false);

        if (state is null)
        {
            state = new WebUserWordState
            {
                Id = Guid.NewGuid(),
                ActorId = actor.ActorId,
                TargetLearningLanguageCode = normalizedTargetLearningLanguageCode,
                WordPublicId = wordPublicId,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            dbContext.UserWordStates.Add(state);
        }

        update(state);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Map(state);
    }

    private static string NormalizeTargetLearningLanguageCode(string targetLearningLanguageCode) =>
        TargetLearningLanguageScope.NormalizeOrDefault(
            targetLearningLanguageCode,
            "Web user word state target learning language");

    private static UserWordStateModel Map(WebUserWordState state) =>
        new(
            state.WordPublicId,
            state.IsKnown,
            state.IsDifficult,
            state.FirstViewedAtUtc,
            state.LastViewedAtUtc,
            state.ViewCount);
}

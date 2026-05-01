using System.Text.RegularExpressions;
using DarwinLingua.Web.Data;
using DarwinLingua.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Services;

public interface IWebWordSuggestionService
{
    Task<WebWordSuggestion> SuggestWordAsync(WordSuggestionInputModel input, CancellationToken cancellationToken);

    Task<IReadOnlyList<WordSuggestionListItemViewModel>> GetSuggestionsAsync(
        WebWordSuggestionStatus? status,
        CancellationToken cancellationToken);

    Task<bool> UpdateSuggestionStatusAsync(
        Guid suggestionId,
        WebWordSuggestionStatus status,
        string? adminNote,
        string decidedBy,
        CancellationToken cancellationToken);
}

internal sealed partial class WebWordSuggestionService(
    WebIdentityDbContext dbContext,
    IWebActorContextAccessor actorContextAccessor) : IWebWordSuggestionService
{
    private const int MaxSuggestedWordLength = 128;
    private const int MaxNoteLength = 1000;

    public async Task<WebWordSuggestion> SuggestWordAsync(WordSuggestionInputModel input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        string suggestedWord = NormalizeSubmittedWord(input.SuggestedWord);
        if (string.IsNullOrWhiteSpace(suggestedWord))
        {
            throw new ArgumentException("A suggested word is required.", nameof(input));
        }

        string normalizedSuggestedWord = NormalizeLookupWord(suggestedWord);
        WebActorContext actor = actorContextAccessor.GetCurrentActor();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        string? note = TrimToNull(input.Note, MaxNoteLength);
        string? sourceQuery = TrimToNull(input.SourceQuery, MaxSuggestedWordLength);

        WebWordSuggestion? existing = await dbContext.WebWordSuggestions
            .Where(suggestion => suggestion.ActorId == actor.ActorId)
            .Where(suggestion => suggestion.NormalizedSuggestedWord == normalizedSuggestedWord)
            .OrderByDescending(suggestion => suggestion.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            if (!string.IsNullOrWhiteSpace(note) && string.IsNullOrWhiteSpace(existing.Note))
            {
                existing.Note = note;
            }

            existing.SourceQuery ??= sourceQuery;
            existing.UpdatedAtUtc = now;
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return existing;
        }

        WebWordSuggestion suggestion = new()
        {
            Id = Guid.NewGuid(),
            SuggestedWord = suggestedWord,
            NormalizedSuggestedWord = normalizedSuggestedWord,
            Note = note,
            SourceQuery = sourceQuery,
            ActorId = actor.ActorId,
            UserId = actor.UserId,
            Email = actor.Email,
            Status = WebWordSuggestionStatus.Pending,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        dbContext.WebWordSuggestions.Add(suggestion);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return suggestion;
    }

    public async Task<IReadOnlyList<WordSuggestionListItemViewModel>> GetSuggestionsAsync(
        WebWordSuggestionStatus? status,
        CancellationToken cancellationToken)
    {
        IQueryable<WebWordSuggestion> query = dbContext.WebWordSuggestions.AsNoTracking();
        if (status.HasValue)
        {
            query = query.Where(suggestion => suggestion.Status == status.Value);
        }

        return await query
            .OrderBy(suggestion => suggestion.Status == WebWordSuggestionStatus.Pending ? 0 : 1)
            .ThenByDescending(suggestion => suggestion.CreatedAtUtc)
            .Take(250)
            .Select(suggestion => new WordSuggestionListItemViewModel(
                suggestion.Id,
                suggestion.SuggestedWord,
                suggestion.Note,
                suggestion.SourceQuery,
                suggestion.ActorId,
                suggestion.Email,
                suggestion.Status,
                suggestion.AdminNote,
                suggestion.DecidedBy,
                suggestion.CreatedAtUtc,
                suggestion.UpdatedAtUtc,
                suggestion.DecidedAtUtc))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> UpdateSuggestionStatusAsync(
        Guid suggestionId,
        WebWordSuggestionStatus status,
        string? adminNote,
        string decidedBy,
        CancellationToken cancellationToken)
    {
        WebWordSuggestion? suggestion = await dbContext.WebWordSuggestions
            .SingleOrDefaultAsync(item => item.Id == suggestionId, cancellationToken)
            .ConfigureAwait(false);

        if (suggestion is null)
        {
            return false;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;
        suggestion.Status = status;
        suggestion.AdminNote = TrimToNull(adminNote, MaxNoteLength);
        suggestion.DecidedBy = TrimToNull(decidedBy, 256);
        suggestion.DecidedAtUtc = now;
        suggestion.UpdatedAtUtc = now;

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private static string NormalizeSubmittedWord(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string collapsed = CollapseWhitespace().Replace(value.Trim(), " ");
        return collapsed.Length <= MaxSuggestedWordLength
            ? collapsed
            : collapsed[..MaxSuggestedWordLength];
    }

    private static string NormalizeLookupWord(string value) =>
        NormalizeSubmittedWord(value).ToLowerInvariant();

    private static string? TrimToNull(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    [GeneratedRegex("\\s+", RegexOptions.Compiled)]
    private static partial Regex CollapseWhitespace();
}

using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Practice.Application.Abstractions;
using DarwinLingua.Practice.Application.Models;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Practice.Infrastructure.Queries;

/// <summary>
/// Reads practice-overview projections from the shared local SQLite database.
/// </summary>
internal sealed class PracticeOverviewReader : IPracticeOverviewReader
{
    private const int PreviewSize = 5;

    private readonly IDbContextFactory<DarwinLinguaDbContext> _dbContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PracticeOverviewReader"/> class.
    /// </summary>
    public PracticeOverviewReader(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory)
    {
        ArgumentNullException.ThrowIfNull(dbContextFactory);

        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public async Task<PracticeOverviewModel> GetOverviewAsync(LanguageCode meaningLanguageCode, CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        DateTime nowUtc = DateTime.UtcNow;
        List<PracticeStateRow> trackedRows = await LoadTrackedRowsAsync(dbContext, cancellationToken).ConfigureAwait(false);
        List<PracticeStateRow> reviewCandidates = GetEligibleReviewCandidates(trackedRows, nowUtc)
            .OrderByDescending(row => IsDueNow(row, nowUtc))
            .ThenByDescending(row => row.IsDifficult)
            .ThenBy(row => row.DueAtUtc ?? row.LastViewedAtUtc)
            .ThenBy(row => row.LastViewedAtUtc)
            .ThenByDescending(row => row.ViewCount)
            .ThenByDescending(row => GetCefrSortWeight(row.CefrLevel))
            .ThenBy(row => row.Lemma, StringComparer.OrdinalIgnoreCase)
            .Take(PreviewSize)
            .ToList();

        Dictionary<Guid, string?> meaningsByWordEntryId = await LoadMeaningsByWordEntryIdAsync(
            dbContext,
            reviewCandidates.Select(row => row.WordEntryId).Distinct().ToArray(),
            meaningLanguageCode,
            cancellationToken).ConfigureAwait(false);

        return new PracticeOverviewModel(
            trackedRows.Count,
            GetEligibleReviewCandidates(trackedRows, nowUtc).Count(),
            trackedRows.Count(row => row.IsDifficult),
            trackedRows.Count(row => row.IsKnown),
            trackedRows.Count(row => row.LastViewedAtUtc is not null),
            trackedRows
                .OrderByDescending(row => row.UpdatedAtUtc)
                .Select(row => (DateTime?)row.UpdatedAtUtc)
                .FirstOrDefault(),
            reviewCandidates
                .Select(row => new PracticeWordPreviewModel(
                    row.WordEntryPublicId,
                    row.Lemma,
                    row.CefrLevel,
                    meaningsByWordEntryId.GetValueOrDefault(row.WordEntryId),
                    row.IsKnown,
                    row.IsDifficult,
                    row.ViewCount,
                    row.LastViewedAtUtc))
                .ToArray());
    }

    /// <inheritdoc />
    public async Task<PracticeReviewQueueModel> GetReviewQueueAsync(
        LanguageCode meaningLanguageCode,
        CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        DateTime nowUtc = DateTime.UtcNow;
        List<PracticeStateRow> trackedRows = await LoadTrackedRowsAsync(dbContext, cancellationToken).ConfigureAwait(false);
        List<PracticeStateRow> orderedCandidates = GetEligibleReviewCandidates(trackedRows, nowUtc)
            .OrderByDescending(row => IsDueNow(row, nowUtc))
            .ThenByDescending(row => row.IsDifficult)
            .ThenBy(row => row.DueAtUtc ?? row.LastViewedAtUtc)
            .ThenBy(row => row.LastViewedAtUtc)
            .ThenByDescending(row => row.ViewCount)
            .ThenByDescending(row => GetCefrSortWeight(row.CefrLevel))
            .ThenBy(row => row.Lemma, StringComparer.OrdinalIgnoreCase)
            .ToList();

        Dictionary<Guid, string?> meaningsByWordEntryId = await LoadMeaningsByWordEntryIdAsync(
            dbContext,
            orderedCandidates.Select(row => row.WordEntryId).Distinct().ToArray(),
            meaningLanguageCode,
            cancellationToken).ConfigureAwait(false);

        return new PracticeReviewQueueModel(
            orderedCandidates.Count,
            orderedCandidates
                .Select((row, index) => new PracticeReviewQueueItemModel(
                    index + 1,
                    row.WordEntryPublicId,
                    row.Lemma,
                    row.CefrLevel,
                    meaningsByWordEntryId.GetValueOrDefault(row.WordEntryId),
                    row.DueAtUtc,
                    IsDueNow(row, nowUtc),
                    row.IsDifficult,
                    row.IsKnown,
                    row.ViewCount,
                    row.LastViewedAtUtc!.Value))
                .ToArray());
    }

    private static async Task<List<PracticeStateRow>> LoadTrackedRowsAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return await (
            from userWordState in dbContext.UserWordStates.AsNoTracking()
            join wordEntry in dbContext.WordEntries.AsNoTracking()
                on userWordState.WordEntryPublicId equals wordEntry.PublicId
            join reviewStateJoin in dbContext.PracticeReviewStates.AsNoTracking()
                on new { userWordState.UserId, userWordState.WordEntryPublicId }
                equals new { reviewStateJoin.UserId, reviewStateJoin.WordEntryPublicId } into reviewStates
            from reviewState in reviewStates.DefaultIfEmpty()
            where userWordState.UserId == LocalInstallationUser.UserId &&
                wordEntry.PublicationStatus == PublicationStatus.Active
            select new PracticeStateRow(
                wordEntry.Id,
                wordEntry.PublicId,
                wordEntry.Lemma,
                wordEntry.PrimaryCefrLevel.ToString(),
                userWordState.IsKnown,
                userWordState.IsDifficult,
                userWordState.ViewCount,
                userWordState.LastViewedAtUtc,
                userWordState.UpdatedAtUtc,
                reviewState == null ? null : reviewState.DueAtUtc,
                reviewState == null ? null : reviewState.LastAttemptedAtUtc,
                reviewState == null ? 0 : reviewState.TotalAttemptCount))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Applies the current deterministic review rule:
    /// currently due scheduled words first, then remaining eligible tracked words,
    /// with difficult words, older schedule/view timestamps, higher view counts, higher CEFR levels,
    /// and alphabetical lemma as deterministic tie-breakers.
    /// </summary>
    private static IEnumerable<PracticeStateRow> GetEligibleReviewCandidates(
        IEnumerable<PracticeStateRow> trackedRows,
        DateTime nowUtc)
    {
        return trackedRows.Where(row =>
            row.LastViewedAtUtc is not null &&
            (!row.IsKnown || row.IsDifficult) &&
            (row.TotalAttemptCount == 0 || row.DueAtUtc is null || row.DueAtUtc <= nowUtc));
    }

    private static bool IsDueNow(PracticeStateRow row, DateTime nowUtc)
    {
        return row.TotalAttemptCount > 0 && row.DueAtUtc is not null && row.DueAtUtc <= nowUtc;
    }

    private static int GetCefrSortWeight(string cefrLevel)
    {
        return cefrLevel switch
        {
            "A1" => 1,
            "A2" => 2,
            "B1" => 3,
            "B2" => 4,
            "C1" => 5,
            "C2" => 6,
            _ => 0,
        };
    }

    private static async Task<Dictionary<Guid, string?>> LoadMeaningsByWordEntryIdAsync(
        DarwinLinguaDbContext dbContext,
        Guid[] wordEntryIds,
        LanguageCode meaningLanguageCode,
        CancellationToken cancellationToken)
    {
        if (wordEntryIds.Length == 0)
        {
            return [];
        }

        List<MeaningRow> meaningRows = await (
            from wordSense in dbContext.WordSenses.AsNoTracking()
            join translation in dbContext.SenseTranslations.AsNoTracking()
                on wordSense.Id equals translation.WordSenseId
            where wordEntryIds.Contains(wordSense.WordEntryId) &&
                wordSense.PublicationStatus == PublicationStatus.Active &&
                translation.LanguageCode == meaningLanguageCode
            orderby wordSense.WordEntryId,
                wordSense.IsPrimarySense descending,
                wordSense.SenseOrder,
                translation.IsPrimary descending,
                translation.TranslationText
            select new MeaningRow(wordSense.WordEntryId, translation.TranslationText))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return meaningRows
            .GroupBy(row => row.WordEntryId)
            .ToDictionary(group => group.Key, group => (string?)group.First().TranslationText);
    }

    private sealed record PracticeStateRow(
        Guid WordEntryId,
        Guid WordEntryPublicId,
        string Lemma,
        string CefrLevel,
        bool IsKnown,
        bool IsDifficult,
        int ViewCount,
        DateTime? LastViewedAtUtc,
        DateTime UpdatedAtUtc,
        DateTime? DueAtUtc,
        DateTime? LastAttemptedAtUtc,
        int TotalAttemptCount);

    private sealed record MeaningRow(Guid WordEntryId, string TranslationText);
}

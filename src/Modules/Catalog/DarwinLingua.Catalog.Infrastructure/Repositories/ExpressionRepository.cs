using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class ExpressionRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IExpressionRepository
{
    private static readonly LanguageCode EnglishFallbackLanguage = LanguageCode.From("en");

    public async Task<IReadOnlyList<ExpressionListItemModel>> GetPublishedExpressionsAsync(
        ExpressionListFilterModel filter,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        IQueryable<ExpressionEntry> query = dbContext.ExpressionEntries
            .AsNoTracking()
            .Include(expression => expression.Topics)
            .Include(expression => expression.Meanings)
            .Include(expression => expression.Warnings)
            .Where(expression => expression.PublicationStatus == PublicationStatus.Active)
            .Where(expression => !expression.RequiresAdultAccess && expression.SafetyRating != "explicit-adult");

        LanguageCode primaryLanguage = ResolveRequestedLanguage(filter.PrimaryMeaningLanguageCode);

        if (Enum.TryParse(filter.CefrLevel, true, out CefrLevel cefrLevel))
        {
            query = query.Where(expression => expression.CefrLevel == cefrLevel);
        }

        string? expressionType = NormalizeOptionalKey(filter.ExpressionType);
        if (expressionType is not null)
        {
            query = query.Where(expression => expression.ExpressionType == expressionType);
        }

        string? register = NormalizeOptionalKey(filter.Register);
        if (register is not null)
        {
            query = query.Where(expression => expression.Register == register);
        }

        string? category = NormalizeOptionalKey(filter.Category);
        if (category is not null)
        {
            query = query.Where(expression => expression.Category == category);
        }

        if (filter.IsRisky.HasValue)
        {
            query = query.Where(expression => expression.IsRisky == filter.IsRisky.Value);
        }

        string? search = NormalizeOptionalSearch(filter.Query);
        if (search is not null)
        {
            string normalizedSearch = search.ToLowerInvariant();
            query = query.Where(expression =>
                expression.ExpressionText.ToLower().Contains(normalizedSearch) ||
                expression.ActualMeaningText.ToLower().Contains(normalizedSearch) ||
                expression.Slug.ToLower().Contains(normalizedSearch));
        }

        List<ExpressionEntry> expressions = await query
            .OrderBy(expression => expression.SortOrder)
            .ThenBy(expression => expression.ExpressionText)
            .AsSplitQuery()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<Guid, string> topicKeysById = await LoadTopicKeysAsync(dbContext, expressions.SelectMany(item => item.Topics).Select(item => item.TopicId), cancellationToken)
            .ConfigureAwait(false);

        string? topicKey = NormalizeOptionalKey(filter.TopicKey);
        IEnumerable<ExpressionEntry> filtered = expressions;
        if (topicKey is not null)
        {
            filtered = filtered.Where(expression =>
                expression.Topics.Any(link =>
                    topicKeysById.TryGetValue(link.TopicId, out string? key) &&
                    string.Equals(key, topicKey, StringComparison.Ordinal)));
        }

        return filtered
            .Select(expression =>
            {
                ExpressionMeaning? meaning = ResolveMeaning(expression.Meanings, primaryLanguage);

                return new ExpressionListItemModel(
                    expression.Slug,
                    expression.ExpressionText,
                    meaning?.ActualMeaningText ?? expression.ActualMeaningText,
                    meaning?.LiteralMeaningText ?? expression.LiteralMeaningText,
                    expression.CefrLevel.ToString(),
                    expression.ExpressionType,
                    expression.Register,
                    expression.Category,
                    expression.Region,
                    expression.IsRisky,
                    expression.MeaningTransparency,
                    expression.TeachingReason,
                    expression.SafetyRating,
                    expression.MinimumAge,
                    expression.RequiresAdultAccess,
                    expression.AdultContentCategory,
                    GetTopicKeys(expression.Topics, topicKeysById),
                    expression.Warnings.OrderBy(warning => warning.WarningType).Select(warning => warning.WarningType).ToArray());
            })
            .ToArray();
    }

    public async Task<ExpressionDetailModel?> GetPublishedExpressionBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        string normalizedSlug = slug.Trim().ToLowerInvariant();
        LanguageCode primaryLanguage = LanguageCode.From(primaryMeaningLanguageCode);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        ExpressionEntry? expression = await dbContext.ExpressionEntries
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.Topics)
            .Include(item => item.Meanings)
            .Include(item => item.Examples).ThenInclude(example => example.Translations)
            .Include(item => item.Warnings).ThenInclude(warning => warning.Translations)
            .Include(item => item.LinkedWords)
            .Include(item => item.RelatedExpressions)
            .Include(item => item.LinkedExercises)
            .Where(item => item.PublicationStatus == PublicationStatus.Active && item.Slug == normalizedSlug)
            .Where(item => !item.RequiresAdultAccess && item.SafetyRating != "explicit-adult")
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (expression is null)
        {
            return null;
        }

        Dictionary<Guid, string> topicKeysById = await LoadTopicKeysAsync(dbContext, expression.Topics.Select(link => link.TopicId), cancellationToken)
            .ConfigureAwait(false);
        ExpressionMeaning? meaning = ResolveMeaning(expression.Meanings, primaryLanguage);

        return new ExpressionDetailModel(
            expression.Slug,
            expression.ExpressionText,
            meaning?.ActualMeaningText ?? expression.ActualMeaningText,
            meaning?.LiteralMeaningText ?? expression.LiteralMeaningText,
            meaning?.UsageExplanation ?? expression.UsageExplanation,
            expression.CefrLevel.ToString(),
            expression.ExpressionType,
            expression.Register,
            expression.Category,
            expression.Region,
            expression.IsRisky,
            expression.MeaningTransparency,
            expression.TeachingReason,
            expression.SafetyRating,
            expression.MinimumAge,
            expression.RequiresAdultAccess,
            expression.AdultContentCategory,
            GetTopicKeys(expression.Topics, topicKeysById),
            expression.Examples.OrderBy(item => item.SortOrder).Select(item => MapExample(item, primaryLanguage)).ToArray(),
            expression.Warnings.OrderBy(item => item.WarningType).Select(item => MapWarning(item, primaryLanguage)).ToArray(),
            expression.LinkedWords.OrderBy(item => item.SortOrder).Select(item => new ExpressionLinkedWordModel(item.Lemma, item.WordSlug)).ToArray(),
            expression.RelatedExpressions.OrderBy(item => item.SortOrder).Select(item => item.TargetSlug).ToArray(),
            expression.LinkedExercises.OrderBy(item => item.SortOrder).Select(item => item.TargetSlug).ToArray());
    }

    private static ExpressionMeaning? ResolveMeaning(IReadOnlyCollection<ExpressionMeaning> meanings, LanguageCode primaryLanguage) =>
        meanings
            .Where(meaning => meaning.LanguageCode == primaryLanguage || meaning.LanguageCode == EnglishFallbackLanguage)
            .OrderBy(meaning => meaning.LanguageCode == primaryLanguage ? 0 : 1)
            .FirstOrDefault();

    private static LanguageCode ResolveRequestedLanguage(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return EnglishFallbackLanguage;
        }

        try
        {
            return LanguageCode.From(languageCode.Trim().ToLowerInvariant());
        }
        catch
        {
            return EnglishFallbackLanguage;
        }
    }

    private static ExpressionExampleModel MapExample(ExpressionExample example, LanguageCode primaryLanguage)
    {
        ExpressionExampleTranslation? translation = ResolveTranslation(example.Translations, primaryLanguage);
        return new ExpressionExampleModel(example.GermanText, example.Note, translation?.Text, primaryLanguage.Value, translation is null || translation.LanguageCode != primaryLanguage);
    }

    private static ExpressionWarningModel MapWarning(ExpressionWarning warning, LanguageCode primaryLanguage)
    {
        ExpressionWarningTranslation? translation = ResolveTranslation(warning.Translations, primaryLanguage);
        return new ExpressionWarningModel(warning.WarningType, translation?.Text ?? warning.Text, primaryLanguage.Value, translation is null || translation.LanguageCode != primaryLanguage);
    }

    private static TTranslation? ResolveTranslation<TTranslation>(
        IReadOnlyCollection<TTranslation> translations,
        LanguageCode primaryLanguage)
        where TTranslation : ExpressionTranslationBase =>
        translations
            .Where(translation => translation.LanguageCode == primaryLanguage || translation.LanguageCode == EnglishFallbackLanguage)
            .OrderBy(translation => translation.LanguageCode == primaryLanguage ? 0 : 1)
            .FirstOrDefault();

    private static async Task<Dictionary<Guid, string>> LoadTopicKeysAsync(DarwinLinguaDbContext dbContext, IEnumerable<Guid> topicIds, CancellationToken cancellationToken)
    {
        Guid[] ids = topicIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return [];
        }

        return await dbContext.Topics
            .AsNoTracking()
            .Where(topic => ids.Contains(topic.Id))
            .ToDictionaryAsync(topic => topic.Id, topic => topic.Key, cancellationToken)
            .ConfigureAwait(false);
    }

    private static string[] GetTopicKeys(IEnumerable<ExpressionTopic> links, IReadOnlyDictionary<Guid, string> topicKeysById) =>
        links
            .OrderByDescending(link => link.IsPrimary)
            .ThenBy(link => link.CreatedAtUtc)
            .Where(link => topicKeysById.ContainsKey(link.TopicId))
            .Select(link => topicKeysById[link.TopicId])
            .ToArray();

    private static string? NormalizeOptionalKey(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static string? NormalizeOptionalSearch(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

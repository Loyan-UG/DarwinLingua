using System.Text.Json;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class ExamPrepRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IExamPrepRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<ExamProfileModel>> GetPublishedExamProfilesAsync(string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken)
    {
        string targetLanguageCode = NormalizeRequiredLanguageCode(targetLearningLanguageCode);
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        List<ExamProfile> profiles = await dbContext.ExamProfiles
            .AsNoTracking()
            .Where(profile =>
                profile.PublicationStatus == PublicationStatus.Active &&
                profile.TargetLearningLanguageCode == targetLanguageCode)
            .OrderBy(profile => profile.SortOrder)
            .ThenBy(profile => profile.DisplayName)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return profiles
            .Select(profile => new ExamProfileModel(
                profile.Key,
                profile.DisplayName,
                ResolveTranslation(profile.DisplayNameTranslationsJson, primaryMeaningLanguageCode),
                profile.CefrRange,
                profile.Description,
                ResolveTranslation(profile.DescriptionTranslationsJson, primaryMeaningLanguageCode)))
            .ToArray();
    }

    public async Task<IReadOnlyList<ExamPrepUnitListItemModel>> GetPublishedExamPrepUnitsAsync(ExamPrepListFilterModel filter, string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken)
    {
        string targetLanguageCode = NormalizeRequiredLanguageCode(targetLearningLanguageCode);
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        IQueryable<ExamPrepUnit> query = dbContext.ExamPrepUnits
            .AsNoTracking()
            .Where(unit =>
                unit.PublicationStatus == PublicationStatus.Active &&
                unit.TargetLearningLanguageCode == targetLanguageCode);
        query = ApplyFilters(query, filter);

        List<ExamPrepUnit> units = await query
            .OrderBy(unit => unit.SortOrder)
            .ThenBy(unit => unit.Title)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return units
            .Select(unit => new ExamPrepUnitListItemModel(
                unit.Slug,
                unit.ExamProfileKey,
                unit.Title,
                ResolveTranslation(unit.TitleTranslationsJson, primaryMeaningLanguageCode),
                unit.ShortDescription,
                ResolveTranslation(unit.ShortDescriptionTranslationsJson, primaryMeaningLanguageCode),
                unit.CefrLevel.ToString(),
                unit.ExamSection,
                unit.TaskType,
                unit.SkillFocus))
            .ToArray();
    }

    public async Task<ExamPrepUnitDetailModel?> GetPublishedExamPrepUnitBySlugAsync(string slug, string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken)
    {
        string normalizedSlug = Normalize(slug) ?? string.Empty;
        string targetLanguageCode = NormalizeRequiredLanguageCode(targetLearningLanguageCode);
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        ExamPrepUnit? unit = await dbContext.ExamPrepUnits
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item =>
                    item.PublicationStatus == PublicationStatus.Active &&
                    item.TargetLearningLanguageCode == targetLanguageCode &&
                    item.Slug == normalizedSlug,
                cancellationToken)
            .ConfigureAwait(false);

        return unit is null
            ? null
            : new ExamPrepUnitDetailModel(
                unit.Slug,
                unit.ExamProfileKey,
                unit.Title,
                unit.ShortDescription,
                unit.CefrLevel.ToString(),
                unit.ExamSection,
                unit.TaskType,
                unit.SkillFocus,
                unit.Explanation,
                ResolveTranslation(unit.TitleTranslationsJson, primaryMeaningLanguageCode),
                ResolveTranslation(unit.ShortDescriptionTranslationsJson, primaryMeaningLanguageCode),
                ResolveTranslation(unit.ExplanationTranslationsJson, primaryMeaningLanguageCode),
                DeserializeStringArray(unit.StrategyNotesJson),
                ResolveTextListTranslation(unit.StrategyNotesTranslationsJson, primaryMeaningLanguageCode),
                DeserializeStringArray(unit.ChecklistJson),
                ResolveTextListTranslation(unit.ChecklistTranslationsJson, primaryMeaningLanguageCode),
                DeserializeStringArray(unit.LinkedDialogueSlugsJson),
                DeserializeStringArray(unit.LinkedTalkTopicSlugsJson),
                DeserializeStringArray(unit.LinkedGrammarTopicSlugsJson),
                DeserializeStringArray(unit.LinkedExpressionSlugsJson),
                DeserializeStringArray(unit.LinkedWritingTemplateSlugsJson),
                DeserializeStringArray(unit.LinkedExerciseSlugsJson),
                DeserializeStringArray(unit.LinkedRoleplaySlugsJson),
                DeserializeStringArray(unit.LinkedCourseLessonSlugsJson));
    }

    private static IQueryable<ExamPrepUnit> ApplyFilters(IQueryable<ExamPrepUnit> query, ExamPrepListFilterModel filter)
    {
        string? profile = Normalize(filter.ExamProfile);
        if (profile is not null) query = query.Where(unit => unit.ExamProfileKey == profile);
        if (Enum.TryParse(filter.CefrLevel, true, out CefrLevel cefrLevel)) query = query.Where(unit => unit.CefrLevel == cefrLevel);
        string? skill = Normalize(filter.SkillFocus);
        if (skill is not null) query = query.Where(unit => unit.SkillFocus == skill);
        string? taskType = Normalize(filter.TaskType);
        if (taskType is not null) query = query.Where(unit => unit.TaskType == taskType);
        string? section = Normalize(filter.Section);
        if (section is not null) query = query.Where(unit => unit.ExamSection == section);
        string? search = NormalizeSearch(filter.Query);
        if (search is not null)
        {
            query = query.Where(unit =>
                EF.Functions.ILike(unit.Title, $"%{search}%") ||
                EF.Functions.ILike(unit.ShortDescription, $"%{search}%") ||
                EF.Functions.ILike(unit.Explanation, $"%{search}%") ||
                EF.Functions.ILike(unit.Slug, $"%{search}%"));
        }

        return query;
    }

    private static string[] DeserializeStringArray(string json) => JsonSerializer.Deserialize<string[]>(json) ?? [];

    private static string? ResolveTranslation(string json, string? languageCode)
    {
        string? normalizedLanguage = Normalize(languageCode);
        if (normalizedLanguage is null || string.IsNullOrWhiteSpace(json) || json == "[]")
        {
            return null;
        }

        try
        {
            TranslationRow[] rows = JsonSerializer.Deserialize<TranslationRow[]>(json, JsonOptions) ?? [];
            return rows.FirstOrDefault(row => string.Equals(row.Language, normalizedLanguage, StringComparison.OrdinalIgnoreCase))?.Text;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string[] ResolveTextListTranslation(string json, string? languageCode)
    {
        string? normalizedLanguage = Normalize(languageCode);
        if (normalizedLanguage is null || string.IsNullOrWhiteSpace(json) || json == "[]")
        {
            return [];
        }

        try
        {
            TextListTranslationRow[] rows = JsonSerializer.Deserialize<TextListTranslationRow[]>(json, JsonOptions) ?? [];
            return rows.FirstOrDefault(row => string.Equals(row.Language, normalizedLanguage, StringComparison.OrdinalIgnoreCase))?.Texts ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static string NormalizeRequiredLanguageCode(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? ContentLanguageRequirements.DefaultTargetLearningLanguageCode
            : value.Trim().ToLowerInvariant();

    private static string? NormalizeSearch(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record TranslationRow(string Language, string Text);

    private sealed record TextListTranslationRow(string Language, string[] Texts);
}

using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Provides content-import persistence against the shared PostgreSQL-backed catalog database.
/// </summary>
public sealed class WebApiContentImportRepository : IContentImportRepository
{
    private readonly IDbContextFactory<DarwinLinguaDbContext> _dbContextFactory;

    public WebApiContentImportRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory)
    {
        ArgumentNullException.ThrowIfNull(dbContextFactory);

        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyDictionary<string, Topic>> GetActiveTopicsByKeyAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await dbContext.Topics
            .AsNoTracking()
            .ToDictionaryAsync(topic => topic.Key, StringComparer.Ordinal, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlySet<LanguageCode>> GetActiveMeaningLanguagesAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        LanguageCode[] languageCodes = await dbContext.Languages
            .AsNoTracking()
            .Where(language => language.IsActive && language.SupportsMeanings)
            .Select(language => language.Code)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new HashSet<LanguageCode>(languageCodes);
    }

    public async Task<bool> PackageExistsAsync(string packageId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await dbContext.ContentPackages
            .AsNoTracking()
            .AnyAsync(contentPackage => contentPackage.PackageId == packageId.Trim(), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> WordExistsAsync(string normalizedLemma, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedLemma);

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await dbContext.WordEntries
            .AsNoTracking()
            .AnyAsync(word => word.NormalizedLemma == normalizedLemma, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<WordEntry>> GetActiveWordsByNormalizedLemmasAsync(
        IReadOnlyCollection<string> normalizedLemmas,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(normalizedLemmas);

        if (normalizedLemmas.Count == 0)
        {
            return [];
        }

        string[] normalizedLemmaArray = normalizedLemmas
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim().ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalizedLemmaArray.Length == 0)
        {
            return [];
        }

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await dbContext.WordEntries
            .AsNoTracking()
            .Where(word => word.PublicationStatus == PublicationStatus.Active)
            .Where(word => normalizedLemmaArray.Contains(word.NormalizedLemma))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task PersistImportAsync(
        ContentPackage contentPackage,
        IReadOnlyList<LabelDefinition> importedLabelDefinitions,
        IReadOnlyList<WordEntry> importedWords,
        IReadOnlyList<WordCollection> importedCollections,
        IReadOnlyList<DialogueLesson> importedDialogues,
        IReadOnlyList<TalkTopic> importedTalkTopics,
        IReadOnlyList<GrammarTopic> importedGrammarTopics,
        IReadOnlyList<ExpressionEntry> importedExpressions,
        IReadOnlyList<Exercise> importedExercises,
        IReadOnlyList<ExerciseSet> importedExerciseSets,
        IReadOnlyList<CoursePath> importedCoursePaths,
        IReadOnlyList<CourseModule> importedCourseModules,
        IReadOnlyList<CourseLesson> importedCourseLessons,
        IReadOnlyList<WritingTemplate> importedWritingTemplates,
        IReadOnlyList<CulturalNote> importedCulturalNotes,
        IReadOnlyList<ExamProfile> importedExamProfiles,
        IReadOnlyList<ExamPrepUnit> importedExamPrepUnits,
        IReadOnlyList<ConversationStarterPack> importedConversationStarterPacks,
        IReadOnlyList<EventPreparationPack> importedEventPreparationPacks,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(contentPackage);
        ArgumentNullException.ThrowIfNull(importedLabelDefinitions);
        ArgumentNullException.ThrowIfNull(importedWords);
        ArgumentNullException.ThrowIfNull(importedCollections);
        ArgumentNullException.ThrowIfNull(importedDialogues);
        ArgumentNullException.ThrowIfNull(importedTalkTopics);
        ArgumentNullException.ThrowIfNull(importedGrammarTopics);
        ArgumentNullException.ThrowIfNull(importedExpressions);
        ArgumentNullException.ThrowIfNull(importedExercises);
        ArgumentNullException.ThrowIfNull(importedExerciseSets);
        ArgumentNullException.ThrowIfNull(importedCoursePaths);
        ArgumentNullException.ThrowIfNull(importedCourseModules);
        ArgumentNullException.ThrowIfNull(importedCourseLessons);
        ArgumentNullException.ThrowIfNull(importedWritingTemplates);
        ArgumentNullException.ThrowIfNull(importedCulturalNotes);
        ArgumentNullException.ThrowIfNull(importedExamProfiles);
        ArgumentNullException.ThrowIfNull(importedExamPrepUnits);
        ArgumentNullException.ThrowIfNull(importedConversationStarterPacks);
        ArgumentNullException.ThrowIfNull(importedEventPreparationPacks);

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        if (importedLabelDefinitions.Count > 0)
        {
            string[] importedLabelDefinitionKeys = importedLabelDefinitions
                .Select(label => label.Key)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            List<LabelDefinition> existingLabels = await dbContext.LabelDefinitions
                .Include(label => label.Localizations)
                .Where(label => importedLabelDefinitionKeys.Contains(label.Key))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (LabelDefinition importedLabel in importedLabelDefinitions)
            {
                LabelDefinition? existingLabel = existingLabels.SingleOrDefault(label =>
                    label.Kind == importedLabel.Kind &&
                    string.Equals(label.Key, importedLabel.Key, StringComparison.Ordinal));

                if (existingLabel is null)
                {
                    dbContext.LabelDefinitions.Add(importedLabel);
                    continue;
                }

                existingLabel.UpdateMetadata(
                    importedLabel.DisplayName,
                    importedLabel.SortOrder,
                    importedLabel.IsSystem,
                    importedLabel.UpdatedAtUtc);

                foreach (LabelDefinitionLocalization localization in importedLabel.Localizations)
                {
                    Guid localizationId = existingLabel.Localizations.Any(item => item.LanguageCode == localization.LanguageCode)
                        ? Guid.Empty
                        : Guid.NewGuid();

                    existingLabel.AddOrUpdateLocalization(
                        localizationId,
                        localization.LanguageCode,
                        localization.DisplayName,
                        importedLabel.UpdatedAtUtc);
                }
            }
        }

        dbContext.AddRange(importedWords);
        dbContext.Add(contentPackage);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (importedCollections.Count > 0)
        {
            string[] importedSlugs = importedCollections
                .Select(item => item.Slug)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            List<WordCollection> existingCollections = await dbContext.WordCollections
                .Include(collection => collection.Entries)
                .Include(collection => collection.Localizations)
                .Where(collection => importedSlugs.Contains(collection.Slug))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (WordCollection importedCollection in importedCollections)
            {
                WordCollection? existingCollection = existingCollections
                    .SingleOrDefault(collection => string.Equals(collection.Slug, importedCollection.Slug, StringComparison.OrdinalIgnoreCase));

                if (existingCollection is null)
                {
                    dbContext.WordCollections.Add(importedCollection);
                    continue;
                }

                existingCollection.UpdateMetadata(
                    importedCollection.Name,
                    importedCollection.Description,
                    importedCollection.ImageUrl,
                    importedCollection.PublicationStatus,
                    importedCollection.SortOrder,
                    importedCollection.UpdatedAtUtc);

                existingCollection.ReplaceEntries(
                    importedCollection.Entries
                        .Select(entry => (entry.WordEntryId, entry.SortOrder))
                        .ToArray(),
                    importedCollection.UpdatedAtUtc);

                foreach (WordCollectionLocalization localization in importedCollection.Localizations)
                {
                    Guid localizationId = existingCollection.Localizations.Any(item => item.LanguageCode == localization.LanguageCode)
                        ? Guid.Empty
                        : Guid.NewGuid();

                    existingCollection.AddOrUpdateLocalization(
                        localizationId,
                        localization.LanguageCode,
                        localization.Name,
                        localization.Description,
                        importedCollection.UpdatedAtUtc);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (importedDialogues.Count > 0)
        {
            string[] importedSlugs = importedDialogues
                .Select(item => item.Slug)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            List<DialogueLesson> existingDialogues = await dbContext.DialogueLessons
                .Where(dialogue => importedSlugs.Contains(dialogue.Slug))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (existingDialogues.Count > 0)
            {
                dbContext.DialogueLessons.RemoveRange(existingDialogues);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            dbContext.DialogueLessons.AddRange(importedDialogues);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (importedTalkTopics.Count > 0)
        {
            string[] importedSlugs = importedTalkTopics
                .Select(item => item.Slug)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            List<TalkTopic> existingTalkTopics = await dbContext.TalkTopics
                .Where(topic => importedSlugs.Contains(topic.Slug))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (existingTalkTopics.Count > 0)
            {
                dbContext.TalkTopics.RemoveRange(existingTalkTopics);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            dbContext.TalkTopics.AddRange(importedTalkTopics);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (importedGrammarTopics.Count > 0)
        {
            string[] importedSlugs = importedGrammarTopics
                .Select(item => item.Slug)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            List<GrammarTopic> existingGrammarTopics = await dbContext.GrammarTopics
                .Where(topic => importedSlugs.Contains(topic.Slug))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (existingGrammarTopics.Count > 0)
            {
                dbContext.GrammarTopics.RemoveRange(existingGrammarTopics);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            dbContext.GrammarTopics.AddRange(importedGrammarTopics);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (importedExpressions.Count > 0)
        {
            string[] importedSlugs = importedExpressions
                .Select(item => item.Slug)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            List<ExpressionEntry> existingExpressions = await dbContext.ExpressionEntries
                .Where(expression => importedSlugs.Contains(expression.Slug))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (existingExpressions.Count > 0)
            {
                dbContext.ExpressionEntries.RemoveRange(existingExpressions);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            dbContext.ExpressionEntries.AddRange(importedExpressions);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (importedExercises.Count > 0)
        {
            string[] importedSlugs = importedExercises.Select(item => item.Slug).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            List<Exercise> existingExercises = await dbContext.Exercises.Where(exercise => importedSlugs.Contains(exercise.Slug)).ToListAsync(cancellationToken).ConfigureAwait(false);
            if (existingExercises.Count > 0)
            {
                dbContext.Exercises.RemoveRange(existingExercises);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            dbContext.Exercises.AddRange(importedExercises);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (importedExerciseSets.Count > 0)
        {
            string[] importedSlugs = importedExerciseSets.Select(item => item.Slug).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            List<ExerciseSet> existingSets = await dbContext.ExerciseSets.Where(set => importedSlugs.Contains(set.Slug)).ToListAsync(cancellationToken).ConfigureAwait(false);
            if (existingSets.Count > 0)
            {
                dbContext.ExerciseSets.RemoveRange(existingSets);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            dbContext.ExerciseSets.AddRange(importedExerciseSets);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (importedCoursePaths.Count > 0)
        {
            string[] importedSlugs = importedCoursePaths.Select(item => item.Slug).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            List<CoursePath> existingCourses = await dbContext.CoursePaths.Where(course => importedSlugs.Contains(course.Slug)).ToListAsync(cancellationToken).ConfigureAwait(false);
            if (existingCourses.Count > 0)
            {
                dbContext.CoursePaths.RemoveRange(existingCourses);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            dbContext.CoursePaths.AddRange(importedCoursePaths);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            Dictionary<string, Guid> courseIdsBySlug = importedCoursePaths.ToDictionary(course => course.Slug, course => course.Id, StringComparer.OrdinalIgnoreCase);
            foreach (CourseModule module in importedCourseModules)
            {
                module.AttachToCoursePath(courseIdsBySlug[module.CoursePathSlug]);
            }

            dbContext.CourseModules.AddRange(importedCourseModules);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            Dictionary<string, Guid> moduleIdsBySlug = importedCourseModules.ToDictionary(module => module.Slug, module => module.Id, StringComparer.OrdinalIgnoreCase);
            foreach (CourseLesson lesson in importedCourseLessons)
            {
                lesson.AttachToCourseModule(moduleIdsBySlug[lesson.ModuleSlug]);
            }

            dbContext.CourseLessons.AddRange(importedCourseLessons);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (importedWritingTemplates.Count > 0)
        {
            string[] importedSlugs = importedWritingTemplates.Select(item => item.Slug).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            List<WritingTemplate> existingTemplates = await dbContext.WritingTemplates.Where(template => importedSlugs.Contains(template.Slug)).ToListAsync(cancellationToken).ConfigureAwait(false);
            if (existingTemplates.Count > 0)
            {
                dbContext.WritingTemplates.RemoveRange(existingTemplates);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            dbContext.WritingTemplates.AddRange(importedWritingTemplates);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (importedCulturalNotes.Count > 0)
        {
            string[] importedSlugs = importedCulturalNotes.Select(item => item.Slug).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            List<CulturalNote> existingNotes = await dbContext.CulturalNotes.Where(note => importedSlugs.Contains(note.Slug)).ToListAsync(cancellationToken).ConfigureAwait(false);
            if (existingNotes.Count > 0)
            {
                dbContext.CulturalNotes.RemoveRange(existingNotes);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            dbContext.CulturalNotes.AddRange(importedCulturalNotes);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (importedExamProfiles.Count > 0)
        {
            string[] importedKeys = importedExamProfiles.Select(item => item.Key).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            List<ExamProfile> existingProfiles = await dbContext.ExamProfiles.Where(profile => importedKeys.Contains(profile.Key)).ToListAsync(cancellationToken).ConfigureAwait(false);
            if (existingProfiles.Count > 0)
            {
                dbContext.ExamProfiles.RemoveRange(existingProfiles);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            dbContext.ExamProfiles.AddRange(importedExamProfiles);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (importedExamPrepUnits.Count > 0)
        {
            string[] importedSlugs = importedExamPrepUnits.Select(item => item.Slug).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            List<ExamPrepUnit> existingUnits = await dbContext.ExamPrepUnits.Where(unit => importedSlugs.Contains(unit.Slug)).ToListAsync(cancellationToken).ConfigureAwait(false);
            if (existingUnits.Count > 0)
            {
                dbContext.ExamPrepUnits.RemoveRange(existingUnits);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            dbContext.ExamPrepUnits.AddRange(importedExamPrepUnits);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (importedConversationStarterPacks.Count > 0)
        {
            string[] importedSlugs = importedConversationStarterPacks
                .Select(item => item.Slug)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            List<ConversationStarterPack> existingStarterPacks = await dbContext.ConversationStarterPacks
                .Where(pack => importedSlugs.Contains(pack.Slug))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (existingStarterPacks.Count > 0)
            {
                dbContext.ConversationStarterPacks.RemoveRange(existingStarterPacks);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            dbContext.ConversationStarterPacks.AddRange(importedConversationStarterPacks);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (importedEventPreparationPacks.Count > 0)
        {
            string[] importedSlugs = importedEventPreparationPacks
                .Select(item => item.Slug)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            List<EventPreparationPack> existingEventPreparationPacks = await dbContext.EventPreparationPacks
                .Where(pack => importedSlugs.Contains(pack.Slug))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (existingEventPreparationPacks.Count > 0)
            {
                dbContext.EventPreparationPacks.RemoveRange(existingEventPreparationPacks);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            dbContext.EventPreparationPacks.AddRange(importedEventPreparationPacks);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}

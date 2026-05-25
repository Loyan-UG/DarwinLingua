using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.WebApi.Configuration;
using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Stages versioned mobile content packages from the shared catalog database.
/// </summary>
public sealed class CatalogPackagePublisher(
    IDbContextFactory<DarwinLinguaDbContext> catalogDbContextFactory,
    ServerContentDbContext serverContentDbContext,
    IOptions<ServerContentOptions> options,
    IWebHostEnvironment hostEnvironment) : ICatalogPackagePublisher
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    /// <inheritdoc />
    public async Task<CatalogPackagePublicationResult> StageDraftAsync(string clientProductKey, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientProductKey);

        ClientProductEntity product = await serverContentDbContext.ClientProducts
            .AsSplitQuery()
            .Include(existingProduct => existingProduct.ContentStreams)
            .ThenInclude(stream => stream.PublishedPackages)
            .SingleOrDefaultAsync(
                existingProduct => existingProduct.Key == clientProductKey.Trim() && existingProduct.IsActive,
                cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"No active client product was found for '{clientProductKey}'.");

        await using DarwinLinguaDbContext catalogDbContext = await catalogDbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<Guid, string> topicKeys = await catalogDbContext.Topics
            .AsNoTracking()
            .ToDictionaryAsync(topic => topic.Id, topic => topic.Key, cancellationToken)
            .ConfigureAwait(false);

        string[] defaultMeaningLanguages = (await catalogDbContext.Languages
            .AsNoTracking()
            .Where(language => language.IsActive && language.SupportsMeanings)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false))
            .OrderBy(language => language.Code.Value, StringComparer.Ordinal)
            .Select(language => language.Code.Value)
            .ToArray();

        List<WordEntry> words = (await catalogDbContext.WordEntries
            .AsNoTracking()
            .AsSplitQuery()
            .Where(word => word.PublicationStatus == PublicationStatus.Active)
            .Include(word => word.Senses)
                .ThenInclude(sense => sense.Translations)
            .Include(word => word.Senses)
                .ThenInclude(sense => sense.Examples)
                    .ThenInclude(example => example.Translations)
            .Include(word => word.Topics)
            .Include(word => word.LexicalForms)
            .Include(word => word.Labels)
            .Include(word => word.GrammarNotes)
            .Include(word => word.Collocations)
            .Include(word => word.FamilyMembers)
            .Include(word => word.Relations)
            .OrderBy(word => word.PrimaryCefrLevel)
            .ThenBy(word => word.NormalizedLemma)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false))
            .Where(word => word.LanguageCode.Value == product.LearningLanguageCode)
            .ToList();

        List<WordCollection> activeCollections = await catalogDbContext.WordCollections
            .AsNoTracking()
            .Where(collection => collection.PublicationStatus == PublicationStatus.Active)
            .Include(collection => collection.Entries)
                .ThenInclude(entry => entry.WordEntry)
            .OrderBy(collection => collection.SortOrder)
            .ThenBy(collection => collection.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<DialogueLesson> activeDialogues = await catalogDbContext.DialogueLessons
            .AsNoTracking()
            .AsSplitQuery()
            .Where(dialogue => dialogue.PublicationStatus == PublicationStatus.Active)
            .Include(dialogue => dialogue.Topics)
            .Include(dialogue => dialogue.DialogueTurns)
                .ThenInclude(turn => turn.Translations)
            .Include(dialogue => dialogue.UsefulPhrases)
                .ThenInclude(phrase => phrase.Translations)
            .Include(dialogue => dialogue.Questions)
                .ThenInclude(question => question.Translations)
            .Include(dialogue => dialogue.Questions)
                .ThenInclude(question => question.Answers)
                    .ThenInclude(answer => answer.Translations)
            .OrderBy(dialogue => dialogue.SortOrder)
            .ThenBy(dialogue => dialogue.Slug)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<ConversationStarterPack> activeConversationStarterPacks = await catalogDbContext.ConversationStarterPacks
            .AsNoTracking()
            .AsSplitQuery()
            .Where(pack => pack.PublicationStatus == PublicationStatus.Active)
            .Include(pack => pack.Topics)
            .Include(pack => pack.LinkedDialogues)
            .Include(pack => pack.LinkedEventPreparationPacks)
            .Include(pack => pack.Phrases)
                .ThenInclude(phrase => phrase.Translations)
            .Include(pack => pack.Phrases)
                .ThenInclude(phrase => phrase.AlternativeBaseTexts)
            .OrderBy(pack => pack.SortOrder)
            .ThenBy(pack => pack.Slug)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<EventPreparationPack> activeEventPreparationPacks = await catalogDbContext.EventPreparationPacks
            .AsNoTracking()
            .AsSplitQuery()
            .Where(pack => pack.PublicationStatus == PublicationStatus.Active)
            .Include(pack => pack.Topics)
            .Include(pack => pack.LinkedDialogues)
            .Include(pack => pack.LinkedConversationStarterPacks)
            .Include(pack => pack.LinkedVocabulary)
            .Include(pack => pack.Prompts)
            .OrderBy(pack => pack.SortOrder)
            .ThenBy(pack => pack.Slug)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<TalkTopic> activeTalkTopics = await catalogDbContext.TalkTopics
            .AsNoTracking()
            .AsSplitQuery()
            .Where(topic => topic.PublicationStatus == PublicationStatus.Active)
            .Include(topic => topic.Topics)
            .Include(topic => topic.ArticleTranslations)
            .Include(topic => topic.Questions)
                .ThenInclude(question => question.Translations)
            .Include(topic => topic.VocabularyItems)
            .Include(topic => topic.SpeakingGoals)
            .OrderBy(topic => topic.SortOrder)
            .ThenBy(topic => topic.Slug)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<GrammarTopic> activeGrammarTopics = await catalogDbContext.GrammarTopics
            .AsNoTracking()
            .AsSplitQuery()
            .Where(topic => topic.PublicationStatus == PublicationStatus.Active)
            .Include(topic => topic.Topics)
            .Include(topic => topic.Sections)
                .ThenInclude(section => section.Translations)
            .Include(topic => topic.Examples)
                .ThenInclude(example => example.Translations)
            .Include(topic => topic.CommonMistakes)
                .ThenInclude(mistake => mistake.Translations)
            .Include(topic => topic.RuleSummaries)
                .ThenInclude(rule => rule.Translations)
            .Include(topic => topic.ExceptionNotes)
                .ThenInclude(note => note.Translations)
            .Include(topic => topic.Prerequisites)
            .Include(topic => topic.RelatedTopics)
            .Include(topic => topic.LinkedWords)
            .Include(topic => topic.LinkedDialogues)
            .Include(topic => topic.LinkedTalkTopics)
            .Include(topic => topic.LinkedExercises)
            .OrderBy(topic => topic.SortOrder)
            .ThenBy(topic => topic.Slug)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<ExpressionEntry> activeExpressions = await catalogDbContext.ExpressionEntries
            .AsNoTracking()
            .AsSplitQuery()
            .Where(expression => expression.PublicationStatus == PublicationStatus.Active)
            .Where(expression =>
                !expression.RequiresAdultAccess &&
                !expression.RequiresVerifiedAdult &&
                !expression.RequiresSensitiveOptIn &&
                expression.SafetyRating == ExpressionSensitivityPolicy.SafetyGeneral &&
                expression.SensitiveContentKind == ExpressionSensitivityPolicy.SensitiveNone &&
                expression.MinimumAge == 0 &&
                expression.UsagePolicy == ExpressionSensitivityPolicy.UsageSafeToUse)
            .Include(expression => expression.Topics)
            .Include(expression => expression.Meanings)
            .Include(expression => expression.Examples)
                .ThenInclude(example => example.Translations)
            .Include(expression => expression.Warnings)
                .ThenInclude(warning => warning.Translations)
            .Include(expression => expression.LinkedWords)
            .Include(expression => expression.RelatedExpressions)
            .Include(expression => expression.LinkedExercises)
            .OrderBy(expression => expression.SortOrder)
            .ThenBy(expression => expression.Slug)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<Exercise> activeExercises = await catalogDbContext.Exercises
            .AsNoTracking()
            .Where(exercise => exercise.PublicationStatus == PublicationStatus.Active)
            .OrderBy(exercise => exercise.SortOrder)
            .ThenBy(exercise => exercise.Slug)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<ExerciseSet> activeExerciseSets = await catalogDbContext.ExerciseSets
            .AsNoTracking()
            .AsSplitQuery()
            .Where(set => set.PublicationStatus == PublicationStatus.Active)
            .Include(set => set.Items)
            .OrderBy(set => set.SortOrder)
            .ThenBy(set => set.Slug)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<CoursePath> activeCoursePaths = await catalogDbContext.CoursePaths
            .AsNoTracking()
            .AsSplitQuery()
            .Where(course => course.PublicationStatus == PublicationStatus.Active)
            .Include(course => course.Modules)
                .ThenInclude(module => module.Lessons)
            .OrderBy(course => course.SortOrder)
            .ThenBy(course => course.Slug)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<CourseModule> activeCourseModules = activeCoursePaths
            .SelectMany(course => course.Modules)
            .Where(module => module.PublicationStatus == PublicationStatus.Active)
            .OrderBy(module => module.SortOrder)
            .ThenBy(module => module.Slug)
            .ToList();

        List<CourseLesson> activeCourseLessons = activeCourseModules
            .SelectMany(module => module.Lessons)
            .Where(lesson => lesson.PublicationStatus == PublicationStatus.Active)
            .OrderBy(lesson => lesson.SortOrder)
            .ThenBy(lesson => lesson.Slug)
            .ToList();

        List<WritingTemplate> activeWritingTemplates = await catalogDbContext.WritingTemplates
            .AsNoTracking()
            .Where(template => template.PublicationStatus == PublicationStatus.Active)
            .OrderBy(template => template.SortOrder)
            .ThenBy(template => template.Slug)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<CulturalNote> activeCulturalNotes = await catalogDbContext.CulturalNotes
            .AsNoTracking()
            .Where(note => note.PublicationStatus == PublicationStatus.Active)
            .OrderBy(note => note.SortOrder)
            .ThenBy(note => note.Slug)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<ExamProfile> activeExamProfiles = await catalogDbContext.ExamProfiles
            .AsNoTracking()
            .Where(profile => profile.PublicationStatus == PublicationStatus.Active)
            .OrderBy(profile => profile.SortOrder)
            .ThenBy(profile => profile.Key)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<ExamPrepUnit> activeExamPrepUnits = await catalogDbContext.ExamPrepUnits
            .AsNoTracking()
            .Where(unit => unit.PublicationStatus == PublicationStatus.Active)
            .OrderBy(unit => unit.SortOrder)
            .ThenBy(unit => unit.Slug)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        string version = now.ToString("yyyy.MM.dd.HHmmss.fff");
        string versionToken = now.ToString("yyyyMMddHHmmssfff");
        string outputRootPath = ResolveOutputRootPath();

        string publicationBatchId = $"{product.Key}-{versionToken}";
        List<PackagePublicationDefinition> definitions = BuildDefinitions(
            product.Key,
            versionToken,
            words,
            activeDialogues,
            activeConversationStarterPacks,
            activeEventPreparationPacks,
            activeTalkTopics,
            activeGrammarTopics,
            activeExpressions,
            activeExercises,
            activeExerciseSets,
            activeCoursePaths,
            activeCourseModules,
            activeCourseLessons,
            activeWritingTemplates,
            activeCulturalNotes,
            activeExamProfiles,
            activeExamPrepUnits);
        List<string> publishedPackageIds = [];

        foreach (PackagePublicationDefinition definition in definitions)
        {
            string packageId = definition.PackageId;
            string relativePath = Path.Combine(product.Key, $"{packageId}.json");
            string fullPath = Path.Combine(outputRootPath, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            ExportedContentPackage packagePayload = CreatePackagePayload(
                packageId,
                definition.PackageName,
                definition.PackageType,
                definition.ContentAreaKey,
                definition.SliceKey,
                product.LearningLanguageCode,
                defaultMeaningLanguages,
                definition.Words,
                definition.Dialogues,
                definition.ConversationStarterPacks,
                definition.EventPreparationPacks,
                definition.TalkTopics,
                definition.GrammarTopics,
                definition.ExpressionEntries,
                definition.Exercises,
                definition.ExerciseSets,
                definition.CoursePaths,
                definition.CourseModules,
                definition.CourseLessons,
                definition.WritingTemplates,
                definition.CulturalNotes,
                definition.ExamProfiles,
                definition.ExamPrepUnits,
                activeCollections,
                topicKeys);

            string json = JsonSerializer.Serialize(packagePayload, SerializerOptions);
            await File.WriteAllTextAsync(fullPath, json, Encoding.UTF8, cancellationToken).ConfigureAwait(false);

            string checksum = ComputeSha256(json);
            ContentStreamEntity stream = ResolveOrCreateStream(product, definition.ContentAreaKey, definition.SliceKey, now);

            PublishedPackageEntity packageEntity = new()
            {
                Id = Guid.NewGuid(),
                PackageId = packageId,
                ContentStreamId = stream.Id,
                ContentStream = stream,
                PackageType = definition.PackageType,
                Version = version,
                PublicationBatchId = publicationBatchId,
                PublicationStatus = PackagePublicationStatus.Draft,
                SchemaVersion = options.Value.DefaultSchemaVersion,
                MinimumAppSchemaVersion = options.Value.DefaultSchemaVersion,
                Checksum = checksum,
                EntryCount = definition.TotalEntryCount,
                WordCount = definition.Words.Count,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                RelativeDownloadPath = relativePath.Replace('\\', '/'),
            };

            serverContentDbContext.PublishedPackages.Add(packageEntity);
            stream.PublishedPackages.Add(packageEntity);
            publishedPackageIds.Add(packageId);
        }

        await serverContentDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CatalogPackagePublicationResult(product.Key, version, publicationBatchId, publishedPackageIds);
    }

    private static List<PackagePublicationDefinition> BuildDefinitions(
        string clientProductKey,
        string versionToken,
        IReadOnlyList<WordEntry> words,
        IReadOnlyList<DialogueLesson> dialogues,
        IReadOnlyList<ConversationStarterPack> conversationStarterPacks,
        IReadOnlyList<EventPreparationPack> eventPreparationPacks,
        IReadOnlyList<TalkTopic> talkTopics,
        IReadOnlyList<GrammarTopic> grammarTopics,
        IReadOnlyList<ExpressionEntry> expressionEntries,
        IReadOnlyList<Exercise> exercises,
        IReadOnlyList<ExerciseSet> exerciseSets,
        IReadOnlyList<CoursePath> coursePaths,
        IReadOnlyList<CourseModule> courseModules,
        IReadOnlyList<CourseLesson> courseLessons,
        IReadOnlyList<WritingTemplate> writingTemplates,
        IReadOnlyList<CulturalNote> culturalNotes,
        IReadOnlyList<ExamProfile> examProfiles,
        IReadOnlyList<ExamPrepUnit> examPrepUnits)
    {
        List<PackagePublicationDefinition> definitions =
        [
            new(
                $"{clientProductKey}-all-full-{versionToken}",
                "Darwin Lingua Full Database",
                "full-database",
                "all",
                "full",
                words,
                dialogues,
                conversationStarterPacks,
                eventPreparationPacks,
                talkTopics,
                grammarTopics,
                expressionEntries,
                exercises,
                exerciseSets,
                coursePaths,
                courseModules,
                courseLessons,
                writingTemplates,
                culturalNotes,
                examProfiles,
                examPrepUnits),
            new(
                $"{clientProductKey}-catalog-full-{versionToken}",
                "Darwin Lingua Catalog",
                "full-catalog",
                "catalog",
                "full",
                words,
                dialogues,
                conversationStarterPacks,
                eventPreparationPacks,
                talkTopics,
                grammarTopics,
                expressionEntries,
                exercises,
                exerciseSets,
                coursePaths,
                courseModules,
                courseLessons,
                writingTemplates,
                culturalNotes,
                examProfiles,
                examPrepUnits),
        ];

        foreach (IGrouping<string, WordEntry> group in words
                     .GroupBy(word => word.PrimaryCefrLevel.ToString().ToLowerInvariant())
                     .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            List<WordEntry> levelWords = group.ToList();
            if (levelWords.Count == 0)
            {
                continue;
            }

            definitions.Add(new PackagePublicationDefinition(
                $"{clientProductKey}-catalog-{group.Key}-{versionToken}",
                $"Darwin Lingua Catalog {group.Key.ToUpperInvariant()}",
                "catalog-cefr",
                "catalog",
                $"cefr:{group.Key}",
                levelWords,
                dialogues
                    .Where(dialogue => string.Equals(dialogue.CefrLevel.ToString(), group.Key, StringComparison.OrdinalIgnoreCase))
                    .ToArray(),
                conversationStarterPacks
                    .Where(pack => string.Equals(pack.CefrLevel.ToString(), group.Key, StringComparison.OrdinalIgnoreCase))
                    .ToArray(),
                eventPreparationPacks
                    .Where(pack => string.Equals(pack.CefrLevel.ToString(), group.Key, StringComparison.OrdinalIgnoreCase))
                    .ToArray(),
                talkTopics
                    .Where(topic => string.Equals(topic.CefrLevel.ToString(), group.Key, StringComparison.OrdinalIgnoreCase))
                    .ToArray(),
                [],
                [],
                [],
                [],
                [],
                [],
                [],
                [],
                [],
                [],
                []));
        }

        AddModuleDefinitions(
            definitions,
            clientProductKey,
            versionToken,
            words,
            dialogues,
            conversationStarterPacks,
            eventPreparationPacks,
            talkTopics,
            grammarTopics,
            expressionEntries,
            exercises,
            exerciseSets,
            coursePaths,
            courseModules,
            courseLessons,
            writingTemplates,
            culturalNotes,
            examProfiles,
            examPrepUnits);

        return definitions;
    }

    private static void AddModuleDefinitions(
        List<PackagePublicationDefinition> definitions,
        string clientProductKey,
        string versionToken,
        IReadOnlyList<WordEntry> words,
        IReadOnlyList<DialogueLesson> dialogues,
        IReadOnlyList<ConversationStarterPack> conversationStarterPacks,
        IReadOnlyList<EventPreparationPack> eventPreparationPacks,
        IReadOnlyList<TalkTopic> talkTopics,
        IReadOnlyList<GrammarTopic> grammarTopics,
        IReadOnlyList<ExpressionEntry> expressionEntries,
        IReadOnlyList<Exercise> exercises,
        IReadOnlyList<ExerciseSet> exerciseSets,
        IReadOnlyList<CoursePath> coursePaths,
        IReadOnlyList<CourseModule> courseModules,
        IReadOnlyList<CourseLesson> courseLessons,
        IReadOnlyList<WritingTemplate> writingTemplates,
        IReadOnlyList<CulturalNote> culturalNotes,
        IReadOnlyList<ExamProfile> examProfiles,
        IReadOnlyList<ExamPrepUnit> examPrepUnits)
    {
        AddModuleDefinition(definitions, clientProductKey, versionToken, "words", "Words", words: words);
        AddModuleDefinition(definitions, clientProductKey, versionToken, "dialogues", "Dialogues", dialogues: dialogues);
        AddModuleDefinition(definitions, clientProductKey, versionToken, "talk-topics", "Talk Topics", talkTopics: talkTopics);
        AddModuleDefinition(definitions, clientProductKey, versionToken, "grammar", "Grammar Guide", grammarTopics: grammarTopics);
        AddModuleDefinition(definitions, clientProductKey, versionToken, "expressions", "Everyday Expressions", expressionEntries: expressionEntries);
        AddModuleDefinition(definitions, clientProductKey, versionToken, "exercises", "Exercises", exercises: exercises, exerciseSets: exerciseSets);
        AddModuleDefinition(definitions, clientProductKey, versionToken, "courses", "Courses", coursePaths: coursePaths, courseModules: courseModules, courseLessons: courseLessons);
        AddModuleDefinition(definitions, clientProductKey, versionToken, "exam-prep", "Exam Preparation", examProfiles: examProfiles, examPrepUnits: examPrepUnits);
        AddModuleDefinition(definitions, clientProductKey, versionToken, "writing-templates", "Writing Templates", writingTemplates: writingTemplates);
        AddModuleDefinition(definitions, clientProductKey, versionToken, "cultural-notes", "Cultural Notes", culturalNotes: culturalNotes);
        AddModuleDefinition(definitions, clientProductKey, versionToken, "events", "Events", eventPreparationPacks: eventPreparationPacks);
        AddModuleDefinition(definitions, clientProductKey, versionToken, "organizers", "Organizers");
        AddModuleDefinition(definitions, clientProductKey, versionToken, "conversation-starters", "Conversation Starters", conversationStarterPacks: conversationStarterPacks);
    }

    private static void AddModuleDefinition(
        List<PackagePublicationDefinition> definitions,
        string clientProductKey,
        string versionToken,
        string moduleKey,
        string moduleName,
        IReadOnlyList<WordEntry>? words = null,
        IReadOnlyList<DialogueLesson>? dialogues = null,
        IReadOnlyList<ConversationStarterPack>? conversationStarterPacks = null,
        IReadOnlyList<EventPreparationPack>? eventPreparationPacks = null,
        IReadOnlyList<TalkTopic>? talkTopics = null,
        IReadOnlyList<GrammarTopic>? grammarTopics = null,
        IReadOnlyList<ExpressionEntry>? expressionEntries = null,
        IReadOnlyList<Exercise>? exercises = null,
        IReadOnlyList<ExerciseSet>? exerciseSets = null,
        IReadOnlyList<CoursePath>? coursePaths = null,
        IReadOnlyList<CourseModule>? courseModules = null,
        IReadOnlyList<CourseLesson>? courseLessons = null,
        IReadOnlyList<WritingTemplate>? writingTemplates = null,
        IReadOnlyList<CulturalNote>? culturalNotes = null,
        IReadOnlyList<ExamProfile>? examProfiles = null,
        IReadOnlyList<ExamPrepUnit>? examPrepUnits = null)
    {
        definitions.Add(new PackagePublicationDefinition(
            $"{clientProductKey}-catalog-module-{moduleKey}-{versionToken}",
            $"Darwin Lingua {moduleName}",
            "catalog-module",
            "catalog",
            $"module:{moduleKey}",
            words ?? [],
            dialogues ?? [],
            conversationStarterPacks ?? [],
            eventPreparationPacks ?? [],
            talkTopics ?? [],
            grammarTopics ?? [],
            expressionEntries ?? [],
            exercises ?? [],
            exerciseSets ?? [],
            coursePaths ?? [],
            courseModules ?? [],
            courseLessons ?? [],
            writingTemplates ?? [],
            culturalNotes ?? [],
            examProfiles ?? [],
            examPrepUnits ?? []));
    }

    private static ExportedContentPackage CreatePackagePayload(
        string packageId,
        string packageName,
        string packageType,
        string contentAreaKey,
        string sliceKey,
        string learningLanguageCode,
        IReadOnlyList<string> defaultMeaningLanguages,
        IReadOnlyList<WordEntry> words,
        IReadOnlyList<DialogueLesson> dialogues,
        IReadOnlyList<ConversationStarterPack> conversationStarterPacks,
        IReadOnlyList<EventPreparationPack> eventPreparationPacks,
        IReadOnlyList<TalkTopic> talkTopics,
        IReadOnlyList<GrammarTopic> grammarTopics,
        IReadOnlyList<ExpressionEntry> expressionEntries,
        IReadOnlyList<Exercise> exercises,
        IReadOnlyList<ExerciseSet> exerciseSets,
        IReadOnlyList<CoursePath> coursePaths,
        IReadOnlyList<CourseModule> courseModules,
        IReadOnlyList<CourseLesson> courseLessons,
        IReadOnlyList<WritingTemplate> writingTemplates,
        IReadOnlyList<CulturalNote> culturalNotes,
        IReadOnlyList<ExamProfile> examProfiles,
        IReadOnlyList<ExamPrepUnit> examPrepUnits,
        IReadOnlyList<WordCollection> activeCollections,
        IReadOnlyDictionary<Guid, string> topicKeys)
    {
        IReadOnlyList<ExportedContentCollection> collections = ShouldExportCollections(packageType)
            ? CreateCollections(activeCollections, words)
            : [];

        return new ExportedContentPackage(
            "1.0",
            packageId,
            packageName,
            "Hybrid",
            packageType,
            contentAreaKey,
            sliceKey,
            learningLanguageCode,
            defaultMeaningLanguages,
            collections,
            dialogues.Select(dialogue => CreateDialogue(dialogue, topicKeys)).ToArray(),
            conversationStarterPacks.Select(pack => CreateConversationStarterPack(pack, topicKeys)).ToArray(),
            eventPreparationPacks.Select(pack => CreateEventPreparationPack(pack, topicKeys)).ToArray(),
            talkTopics.Select(topic => CreateTalkTopic(topic, topicKeys)).ToArray(),
            grammarTopics.Select(topic => CreateGrammarTopic(topic, topicKeys)).ToArray(),
            expressionEntries.Select(entry => CreateExpressionEntry(entry, topicKeys)).ToArray(),
            exercises.Select(CreateExercise).ToArray(),
            exerciseSets.Select(CreateExerciseSet).ToArray(),
            coursePaths.Select(CreateCoursePath).ToArray(),
            courseModules.Select(CreateCourseModule).ToArray(),
            courseLessons.Select(CreateCourseLesson).ToArray(),
            writingTemplates.Select(CreateWritingTemplate).ToArray(),
            culturalNotes.Select(CreateCulturalNote).ToArray(),
            examProfiles.Select(CreateExamProfile).ToArray(),
            examPrepUnits.Select(CreateExamPrepUnit).ToArray(),
            words.Select(word => CreateEntry(word, topicKeys)).ToArray());
    }

    private static bool ShouldExportCollections(string packageType)
    {
        return string.Equals(packageType, "full-database", StringComparison.OrdinalIgnoreCase)
            || string.Equals(packageType, "full-catalog", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<ExportedContentCollection> CreateCollections(
        IReadOnlyList<WordCollection> activeCollections,
        IReadOnlyList<WordEntry> words)
    {
        HashSet<Guid> exportedWordIds = words
            .Select(word => word.Id)
            .ToHashSet();

        return activeCollections
            .Select(collection => new
            {
                Collection = collection,
                Entries = collection.Entries
                    .Where(entry => entry.WordEntry is not null && exportedWordIds.Contains(entry.WordEntryId))
                    .OrderBy(entry => entry.SortOrder)
                    .ToArray(),
            })
            .Where(item => item.Entries.Length > 0)
            .Select(item => new ExportedContentCollection(
                item.Collection.Slug,
                item.Collection.Name,
                item.Collection.Description,
                item.Collection.ImageUrl,
                item.Collection.SortOrder,
                item.Entries
                    .Select(entry => new ExportedContentCollectionWordReference(
                        entry.WordEntry!.Lemma,
                        entry.WordEntry.PartOfSpeech.ToString(),
                        entry.WordEntry.PrimaryCefrLevel.ToString()))
                    .ToArray()))
            .ToArray();
    }

    private static ExportedContentEntry CreateEntry(WordEntry word, IReadOnlyDictionary<Guid, string> topicKeys)
    {
        WordSense? primarySense = word.Senses
            .OrderByDescending(sense => sense.IsPrimarySense)
            .ThenBy(sense => sense.SenseOrder)
            .FirstOrDefault();

        ExportedMeaning[] meanings = primarySense?.Translations
            .OrderByDescending(translation => translation.IsPrimary)
            .ThenBy(translation => translation.LanguageCode.Value, StringComparer.Ordinal)
            .Select(translation => new ExportedMeaning(
                translation.LanguageCode.Value,
                translation.TranslationText))
            .ToArray()
            ?? [];

        ExportedExample[] examples = primarySense?.Examples
            .OrderByDescending(example => example.IsPrimaryExample)
            .ThenBy(example => example.SentenceOrder)
            .Select(example => new ExportedExample(
                example.GermanText,
                example.Translations
                    .OrderBy(translation => translation.LanguageCode.Value, StringComparer.Ordinal)
                    .Select(translation => new ExportedMeaning(
                        translation.LanguageCode.Value,
                        translation.TranslationText))
                    .ToArray()))
            .ToArray()
            ?? [];

        string[] topics = word.Topics
            .OrderByDescending(topic => topic.IsPrimaryTopic)
            .ThenBy(topic => topic.TopicId)
            .Select(topic => topicKeys[topic.TopicId])
            .ToArray();

        string[] usageLabels = word.Labels
            .Where(label => label.Kind == WordLabelKind.Usage)
            .OrderBy(label => label.SortOrder)
            .Select(label => label.Key)
            .ToArray();

        string[] contextLabels = word.Labels
            .Where(label => label.Kind == WordLabelKind.Context)
            .OrderBy(label => label.SortOrder)
            .Select(label => label.Key)
            .ToArray();

        string[] grammarNotes = word.GrammarNotes
            .OrderBy(note => note.SortOrder)
            .Select(note => note.Text)
            .ToArray();

        ExportedCollocation[] collocations = word.Collocations
            .OrderBy(collocation => collocation.SortOrder)
            .Select(collocation => new ExportedCollocation(collocation.Text, collocation.Meaning))
            .ToArray();

        ExportedWordFamilyMember[] wordFamilies = word.FamilyMembers
            .OrderBy(member => member.SortOrder)
            .Select(member => new ExportedWordFamilyMember(member.Lemma, member.RelationLabel, member.Note))
            .ToArray();

        ExportedWordRelation[] relations = word.Relations
            .OrderBy(relation => relation.SortOrder)
            .Select(relation => new ExportedWordRelation(relation.Kind.ToString().ToLowerInvariant(), relation.Lemma, relation.Note))
            .ToArray();

        return new ExportedContentEntry(
            word.Lemma,
            word.LanguageCode.Value,
            word.PrimaryCefrLevel.ToString(),
            word.PartOfSpeech.ToString(),
            word.LexicalForms.Count == 0
                ? [new ExportedLexicalForm(word.PartOfSpeech.ToString(), word.Article, word.PluralForm, word.InfinitiveForm, true)]
                : word.LexicalForms
                    .OrderByDescending(form => form.IsPrimary)
                    .ThenBy(form => form.SortOrder)
                    .Select(form => new ExportedLexicalForm(
                        form.PartOfSpeech.ToString(),
                        form.Article,
                        form.PluralForm,
                        form.InfinitiveForm,
                        form.IsPrimary))
                    .ToArray(),
            word.Article,
            word.PluralForm,
            word.InfinitiveForm,
            word.PronunciationIpa,
            word.SyllableBreak,
            topics,
            usageLabels,
            contextLabels,
            grammarNotes,
            collocations,
            wordFamilies,
            relations,
            meanings,
            examples);
    }

    private static ExportedDialogueLesson CreateDialogue(DialogueLesson dialogue, IReadOnlyDictionary<Guid, string> topicKeys)
    {
        return new ExportedDialogueLesson(
            dialogue.Slug,
            dialogue.Title,
            dialogue.Description,
            dialogue.LearnerGoal,
            dialogue.CefrLevel.ToString(),
            dialogue.Category,
            dialogue.Topics
                .OrderByDescending(topic => topic.IsPrimary)
                .ThenBy(topic => topic.TopicId)
                .Select(topic => topicKeys[topic.TopicId])
                .ToArray(),
            dialogue.SortOrder,
            dialogue.DialogueTurns
                .OrderBy(turn => turn.SortOrder)
                .Select(turn => new ExportedDialogueTurn(
                    turn.SpeakerRole,
                    turn.BaseText,
                    CreateTranslations(turn.Translations)))
                .ToArray(),
            dialogue.UsefulPhrases
                .OrderBy(phrase => phrase.SortOrder)
                .Select(phrase => new ExportedDialoguePhrase(
                    phrase.BaseText,
                    phrase.UsageNote,
                    CreateTranslations(phrase.Translations)))
                .ToArray(),
            dialogue.Questions
                .OrderBy(question => question.SortOrder)
                .Select(question => new ExportedDialogueQuestion(
                    question.Prompt,
                    CreateTranslations(question.Translations),
                    question.Answers
                        .OrderBy(answer => answer.SortOrder)
                        .Select(answer => new ExportedDialogueAnswer(
                            answer.Text,
                            answer.IsCorrect,
                            answer.Feedback,
                            CreateTranslations(answer.Translations)))
                        .ToArray()))
                .ToArray());
    }

    private static ExportedConversationStarterPack CreateConversationStarterPack(
        ConversationStarterPack pack,
        IReadOnlyDictionary<Guid, string> topicKeys)
    {
        return new ExportedConversationStarterPack(
            pack.Slug,
            pack.Title,
            pack.Description,
            pack.CefrLevel.ToString(),
            pack.Category,
            pack.Situation,
            pack.Tone,
            pack.ConversationGoal,
            pack.Topics
                .OrderByDescending(topic => topic.IsPrimary)
                .ThenBy(topic => topic.TopicId)
                .Select(topic => topicKeys[topic.TopicId])
                .ToArray(),
            pack.SortOrder,
            pack.LinkedDialogues
                .OrderBy(link => link.SortOrder)
                .Select(link => link.DialogueSlug)
                .ToArray(),
            pack.LinkedEventPreparationPacks
                .OrderBy(link => link.SortOrder)
                .Select(link => link.EventPreparationPackSlug)
                .ToArray(),
            pack.Phrases
                .OrderBy(phrase => phrase.SortOrder)
                .Select(phrase => new ExportedConversationStarterPhrase(
                    phrase.BaseText,
                    phrase.Function,
                    CreateTranslations(phrase.Translations),
                    phrase.UsageNote,
                    phrase.Register,
                    phrase.SortOrder,
                    phrase.AlternativeBaseTexts
                        .OrderBy(alternative => alternative.SortOrder)
                        .Select(alternative => alternative.BaseText)
                        .ToArray(),
                    phrase.CommonMistake))
                .ToArray());
    }

    private static ExportedEventPreparationPack CreateEventPreparationPack(
        EventPreparationPack pack,
        IReadOnlyDictionary<Guid, string> topicKeys)
    {
        return new ExportedEventPreparationPack(
            pack.Slug,
            pack.Title,
            pack.Description,
            pack.CefrLevel.ToString(),
            pack.Category,
            pack.EventType,
            pack.Topics
                .OrderByDescending(topic => topic.IsPrimary)
                .ThenBy(topic => topic.TopicId)
                .Select(topic => topicKeys[topic.TopicId])
                .ToArray(),
            pack.SortOrder,
            pack.LinkedDialogues
                .OrderBy(link => link.SortOrder)
                .Select(link => link.DialogueSlug)
                .ToArray(),
            pack.LinkedVocabulary
                .OrderBy(reference => reference.SortOrder)
                .Select(reference => new ExportedEventPreparationVocabularyReference(
                    reference.Word,
                    reference.PartOfSpeech?.ToString(),
                    reference.CefrLevel?.ToString()))
                .ToArray(),
            pack.LinkedConversationStarterPacks
                .OrderBy(link => link.SortOrder)
                .Select(link => link.ConversationStarterPackSlug)
                .ToArray(),
            pack.Prompts
                .Where(prompt => prompt.PromptType == "opening")
                .OrderBy(prompt => prompt.SortOrder)
                .Select(prompt => prompt.Text)
                .ToArray(),
            pack.Prompts
                .Where(prompt => prompt.PromptType == "roleplay")
                .OrderBy(prompt => prompt.SortOrder)
                .Select(prompt => prompt.Text)
                .ToArray(),
            pack.Prompts
                .Where(prompt => prompt.PromptType == "review")
                .OrderBy(prompt => prompt.SortOrder)
                .Select(prompt => prompt.Text)
                .ToArray());
    }

    private static ExportedTalkTopic CreateTalkTopic(TalkTopic topic, IReadOnlyDictionary<Guid, string> topicKeys)
    {
        return new ExportedTalkTopic(
            topic.Slug,
            topic.TopicGroupKey,
            topic.Title,
            topic.Description,
            topic.CefrLevel.ToString(),
            topic.Category,
            FormatTalkTopicContentType(topic.ContentType),
            topic.Topics
                .OrderByDescending(link => link.IsPrimary)
                .ThenBy(link => link.CreatedAtUtc)
                .Select(link => topicKeys[link.TopicId])
                .ToArray(),
            topic.ArticleBaseText,
            CreateTranslations(topic.ArticleTranslations),
            topic.WarmupQuestions
                .OrderBy(question => question.SortOrder)
                .Select(question => new ExportedTalkTopicWarmupQuestion(
                    question.Prompt,
                    CreateTranslations(question.Translations),
                    question.SortOrder))
                .ToArray(),
            topic.DiscussionQuestions
                .OrderBy(question => question.SortOrder)
                .Select(question => new ExportedTalkTopicDiscussionQuestion(
                    question.Prompt,
                    question.QuestionType.HasValue ? FormatTalkTopicQuestionType(question.QuestionType.Value) : string.Empty,
                    CreateTranslations(question.Translations),
                    question.SortOrder))
                .ToArray(),
            topic.VocabularyItems
                .OrderBy(item => item.SortOrder)
                .Select(item => new ExportedTalkTopicVocabularyItem(
                    item.Lemma,
                    item.WordSlug,
                    item.CefrLevel?.ToString(),
                    item.SortOrder))
                .ToArray(),
            topic.SpeakingGoals
                .OrderBy(goal => goal.SortOrder)
                .Select(goal => FormatTalkTopicSpeakingGoal(goal.SpeakingGoal))
                .ToArray(),
            topic.IsSensitive,
            topic.SensitivityNote,
            topic.RecommendedForModeratedGroupsOnly,
            topic.EstimatedReadingMinutes,
            topic.EstimatedDiscussionMinutes,
            topic.SortOrder);
    }

    private static ExportedGrammarTopic CreateGrammarTopic(GrammarTopic topic, IReadOnlyDictionary<Guid, string> topicKeys)
    {
        return new ExportedGrammarTopic(
            topic.Slug,
            topic.Title,
            topic.ShortDescription,
            topic.CefrLevel.ToString(),
            topic.GrammarCategory,
            topic.Topics
                .OrderByDescending(link => link.IsPrimary)
                .ThenBy(link => link.CreatedAtUtc)
                .Select(link => topicKeys[link.TopicId])
                .ToArray(),
            true,
            topic.SortOrder,
            topic.Sections
                .OrderBy(section => section.SortOrder)
                .Select(section => new ExportedGrammarSection(
                    section.Heading,
                    section.Explanation,
                    section.Translations
                        .OrderBy(translation => translation.LanguageCode.Value, StringComparer.Ordinal)
                        .Select(translation => new ExportedGrammarSectionTranslation(
                            translation.LanguageCode.Value,
                            translation.Heading,
                            translation.Text))
                        .ToArray(),
                    section.SortOrder))
                .ToArray(),
            topic.Examples
                .OrderBy(example => example.SortOrder)
                .Select(example => new ExportedGrammarExample(
                    example.GermanText,
                    example.Note,
                    CreateTranslations(example.Translations),
                    example.SortOrder))
                .ToArray(),
            topic.RuleSummaries
                .OrderBy(item => item.SortOrder)
                .Select(item => new ExportedGrammarTextItem(
                    item.Text,
                    CreateTranslations(item.Translations),
                    item.SortOrder))
                .ToArray(),
            topic.CommonMistakes
                .OrderBy(item => item.SortOrder)
                .Select(item => new ExportedGrammarCommonMistake(
                    item.WrongText,
                    item.CorrectedText,
                    item.Explanation,
                    CreateTranslations(item.Translations),
                    item.SortOrder))
                .ToArray(),
            topic.ExceptionNotes
                .OrderBy(item => item.SortOrder)
                .Select(item => new ExportedGrammarTextItem(
                    item.Text,
                    CreateTranslations(item.Translations),
                    item.SortOrder))
                .ToArray(),
            topic.Prerequisites.OrderBy(link => link.SortOrder).Select(link => link.TargetSlug).ToArray(),
            topic.RelatedTopics.OrderBy(link => link.SortOrder).Select(link => link.TargetSlug).ToArray(),
            topic.LinkedWords
                .OrderBy(link => link.SortOrder)
                .Select(link => new ExportedLinkedWord(link.Lemma, link.WordSlug, link.SortOrder))
                .ToArray(),
            topic.LinkedDialogues.OrderBy(link => link.SortOrder).Select(link => link.TargetSlug).ToArray(),
            topic.LinkedTalkTopics.OrderBy(link => link.SortOrder).Select(link => link.TargetSlug).ToArray(),
            topic.LinkedExercises.OrderBy(link => link.SortOrder).Select(link => link.TargetSlug).ToArray());
    }

    private static ExportedExpressionEntry CreateExpressionEntry(ExpressionEntry entry, IReadOnlyDictionary<Guid, string> topicKeys)
    {
        return new ExportedExpressionEntry(
            entry.Slug,
            entry.ExpressionText,
            entry.LiteralMeaningText,
            entry.ActualMeaningText,
            entry.UsageExplanation,
            entry.CefrLevel.ToString(),
            entry.ExpressionType,
            entry.Register,
            entry.Category,
            null,
            entry.Region,
            entry.IsRisky,
            entry.MeaningTransparency,
            entry.TeachingReason,
            entry.SafetyRating,
            entry.MinimumAge,
            entry.RequiresAdultAccess,
            entry.AdultContentCategory,
            entry.SensitiveContentKind,
            entry.RequiresSensitiveOptIn,
            entry.RequiresVerifiedAdult,
            entry.UsagePolicy,
            entry.Topics
                .OrderByDescending(link => link.IsPrimary)
                .ThenBy(link => link.CreatedAtUtc)
                .Select(link => topicKeys[link.TopicId])
                .ToArray(),
            true,
            entry.SortOrder,
            entry.Meanings
                .OrderBy(meaning => meaning.LanguageCode.Value, StringComparer.Ordinal)
                .Select(meaning => new ExportedExpressionMeaning(
                    meaning.LanguageCode.Value,
                    meaning.ActualMeaningText,
                    meaning.ActualMeaningText,
                    meaning.LiteralMeaningText,
                    meaning.UsageExplanation))
                .ToArray(),
            entry.Examples
                .OrderBy(example => example.SortOrder)
                .Select(example => new ExportedExpressionExample(
                    example.GermanText,
                    example.Note,
                    CreateTranslations(example.Translations),
                    example.SortOrder))
                .ToArray(),
            entry.Warnings
                .OrderBy(warning => warning.WarningType, StringComparer.Ordinal)
                .Select(warning => new ExportedExpressionWarning(
                    warning.WarningType,
                    warning.Text,
                    CreateTranslations(warning.Translations)))
                .ToArray(),
            entry.LinkedWords
                .OrderBy(link => link.SortOrder)
                .Select(link => new ExportedLinkedWord(link.Lemma, link.WordSlug, link.SortOrder))
                .ToArray(),
            entry.RelatedExpressions.OrderBy(link => link.SortOrder).Select(link => link.TargetSlug).ToArray(),
            entry.LinkedExercises.OrderBy(link => link.SortOrder).Select(link => link.TargetSlug).ToArray());
    }

    private static ExportedExercise CreateExercise(Exercise exercise) =>
        new(
            exercise.Slug,
            exercise.Title,
            exercise.Instruction,
            exercise.CefrLevel.ToString(),
            exercise.ExerciseType,
            exercise.TargetSkill,
            exercise.OwnerType,
            exercise.OwnerSlug,
            ParseJsonElement(exercise.PromptJson),
            ParseJsonElement(exercise.AnswerKeyJson),
            exercise.CorrectExplanation,
            exercise.IncorrectExplanation,
            exercise.Hint,
            exercise.CommonMistakeNote,
            true,
            exercise.SortOrder);

    private static ExportedExerciseSet CreateExerciseSet(ExerciseSet set) =>
        new(
            set.Slug,
            set.Title,
            set.Description,
            set.CefrLevel.ToString(),
            set.OwnerType,
            set.OwnerSlug,
            set.Items.OrderBy(item => item.SortOrder).Select(item => item.ExerciseSlug).ToArray(),
            true,
            set.SortOrder);

    private static ExportedCoursePath CreateCoursePath(CoursePath course) =>
        new(
            course.Slug,
            course.Title,
            course.Description,
            course.CefrLevel?.ToString(),
            course.CefrRange,
            true,
            course.SortOrder);

    private static ExportedCourseModule CreateCourseModule(CourseModule module) =>
        new(
            module.Slug,
            module.CoursePathSlug,
            module.Title,
            module.Description,
            module.ModuleNumber,
            module.CefrLevel.ToString(),
            true,
            module.SortOrder);

    private static ExportedCourseLesson CreateCourseLesson(CourseLesson lesson) =>
        new(
            lesson.Slug,
            lesson.CoursePathSlug,
            lesson.ModuleSlug,
            lesson.LessonNumber,
            lesson.Title,
            lesson.ShortDescription,
            lesson.Narrative,
            lesson.CefrLevel.ToString(),
            lesson.EstimatedMinutes,
            ParseStringArray(lesson.LearningGoalsJson),
            ParseStringArray(lesson.PrerequisiteLessonSlugsJson),
            lesson.NextLessonSlug,
            ParseStringArray(lesson.LinkedGrammarTopicSlugsJson),
            ParseStringArray(lesson.LinkedWordSlugsJson),
            ParseStringArray(lesson.LinkedExpressionSlugsJson),
            ParseStringArray(lesson.LinkedDialogueSlugsJson),
            ParseStringArray(lesson.LinkedTalkTopicSlugsJson),
            ParseStringArray(lesson.LinkedExerciseSetSlugsJson),
            ParseStringArray(lesson.LinkedExamPrepSlugsJson),
            lesson.ReviewSummary,
            lesson.HomeworkTask,
            true,
            lesson.SortOrder);

    private static ExportedWritingTemplate CreateWritingTemplate(WritingTemplate template) =>
        new(
            template.Slug,
            template.Title,
            template.ShortDescription,
            template.CefrLevel.ToString(),
            template.Category,
            template.Situation,
            template.Register,
            template.TemplateText,
            template.Explanation,
            ParseStringArray(template.VariablesJson),
            template.SampleFilledVersion,
            ParseStringArray(template.LinkedGrammarTopicSlugsJson),
            ParseStringArray(template.LinkedWordSlugsJson),
            ParseStringArray(template.LinkedExpressionSlugsJson),
            ParseStringArray(template.LinkedExerciseSlugsJson),
            true,
            template.SortOrder);

    private static ExportedCulturalNote CreateCulturalNote(CulturalNote note) =>
        new(
            note.Slug,
            note.Title,
            note.ShortDescription,
            note.CefrLevel.ToString(),
            note.Category,
            note.Context,
            ParseStringArray(note.SectionsJson),
            ParseJsonArray<ExportedCulturalNoteExample>(note.ExamplesJson),
            ParseStringArray(note.DoNotesJson),
            null,
            ParseStringArray(note.DontNotesJson),
            null,
            note.SensitivityWarning,
            ParseStringArray(note.LinkedDialogueSlugsJson),
            ParseStringArray(note.LinkedExpressionSlugsJson),
            ParseStringArray(note.LinkedWritingTemplateSlugsJson),
            ParseStringArray(note.LinkedTalkTopicSlugsJson),
            ParseStringArray(note.LinkedCourseLessonSlugsJson),
            true,
            note.SortOrder);

    private static ExportedExamProfile CreateExamProfile(ExamProfile profile) =>
        new(profile.Key, profile.DisplayName, profile.CefrRange, profile.Description, true, profile.SortOrder);

    private static ExportedExamPrepUnit CreateExamPrepUnit(ExamPrepUnit unit) =>
        new(
            unit.Slug,
            unit.ExamProfileKey,
            unit.Title,
            unit.ShortDescription,
            unit.CefrLevel.ToString(),
            unit.ExamSection,
            null,
            unit.TaskType,
            unit.SkillFocus,
            unit.Explanation,
            ParseStringArray(unit.StrategyNotesJson),
            ParseStringArray(unit.ChecklistJson),
            ParseStringArray(unit.LinkedDialogueSlugsJson),
            ParseStringArray(unit.LinkedTalkTopicSlugsJson),
            ParseStringArray(unit.LinkedGrammarTopicSlugsJson),
            ParseStringArray(unit.LinkedExpressionSlugsJson),
            ParseStringArray(unit.LinkedWritingTemplateSlugsJson),
            ParseStringArray(unit.LinkedExerciseSlugsJson),
            ParseStringArray(unit.LinkedCourseLessonSlugsJson),
            true,
            unit.SortOrder);

    private static IReadOnlyList<ExportedMeaning> CreateTranslations(IEnumerable<DialogueTranslationBase> translations)
    {
        return translations
            .OrderBy(translation => translation.LanguageCode.Value, StringComparer.Ordinal)
            .Select(translation => new ExportedMeaning(
                translation.LanguageCode.Value,
                translation.Text))
            .ToArray();
    }

    private static IReadOnlyList<ExportedMeaning> CreateTranslations(IEnumerable<ConversationStarterPhraseTranslation> translations)
    {
        return translations
            .OrderBy(translation => translation.LanguageCode.Value, StringComparer.Ordinal)
            .Select(translation => new ExportedMeaning(
                translation.LanguageCode.Value,
                translation.Text))
            .ToArray();
    }

    private static IReadOnlyList<ExportedMeaning> CreateTranslations(IEnumerable<TalkTopicTranslationBase> translations)
    {
        return translations
            .OrderBy(translation => translation.LanguageCode.Value, StringComparer.Ordinal)
            .Select(translation => new ExportedMeaning(
                translation.LanguageCode.Value,
                translation.Text))
            .ToArray();
    }

    private static IReadOnlyList<ExportedMeaning> CreateTranslations(IEnumerable<GrammarTranslationBase> translations)
    {
        return translations
            .OrderBy(translation => translation.LanguageCode.Value, StringComparer.Ordinal)
            .Select(translation => new ExportedMeaning(
                translation.LanguageCode.Value,
                translation.Text))
            .ToArray();
    }

    private static IReadOnlyList<ExportedMeaning> CreateTranslations(IEnumerable<ExpressionTranslationBase> translations)
    {
        return translations
            .OrderBy(translation => translation.LanguageCode.Value, StringComparer.Ordinal)
            .Select(translation => new ExportedMeaning(
                translation.LanguageCode.Value,
                translation.Text))
            .ToArray();
    }

    private static IReadOnlyList<string> ParseStringArray(string json) => ParseJsonArray<string>(json);

    private static IReadOnlyList<T> ParseJsonArray<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<T[]>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static JsonElement ParseJsonElement(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static string FormatTalkTopicContentType(TalkTopicContentType contentType) =>
        contentType switch
        {
            TalkTopicContentType.Article => "article",
            TalkTopicContentType.BookSummary => "book-summary",
            TalkTopicContentType.MovieSummary => "movie-summary",
            TalkTopicContentType.Story => "story",
            TalkTopicContentType.FactSheet => "fact-sheet",
            TalkTopicContentType.OpinionText => "opinion-text",
            TalkTopicContentType.Interview => "interview",
            TalkTopicContentType.DebateText => "debate-text",
            _ => contentType.ToString(),
        };

    private static string FormatTalkTopicQuestionType(TalkTopicQuestionType questionType) =>
        questionType switch
        {
            TalkTopicQuestionType.Opinion => "opinion",
            TalkTopicQuestionType.PersonalExperience => "personal-experience",
            TalkTopicQuestionType.Prediction => "prediction",
            TalkTopicQuestionType.Comparison => "comparison",
            TalkTopicQuestionType.Imagination => "imagination",
            TalkTopicQuestionType.Debate => "debate",
            TalkTopicQuestionType.Ethics => "ethics",
            TalkTopicQuestionType.Comprehension => "comprehension",
            _ => questionType.ToString(),
        };

    private static string FormatTalkTopicSpeakingGoal(TalkTopicSpeakingGoal speakingGoal) =>
        speakingGoal switch
        {
            TalkTopicSpeakingGoal.ExpressOpinion => "express-opinion",
            TalkTopicSpeakingGoal.GiveReasons => "give-reasons",
            TalkTopicSpeakingGoal.AgreeDisagree => "agree-disagree",
            TalkTopicSpeakingGoal.AskFollowUpQuestions => "ask-follow-up-questions",
            TalkTopicSpeakingGoal.CompareOptions => "compare-options",
            TalkTopicSpeakingGoal.MakePredictions => "make-predictions",
            TalkTopicSpeakingGoal.DescribeExperiences => "describe-experiences",
            TalkTopicSpeakingGoal.ImaginePossibilities => "imagine-possibilities",
            TalkTopicSpeakingGoal.DebatePolitely => "debate-politely",
            TalkTopicSpeakingGoal.SummarizePosition => "summarize-position",
            _ => speakingGoal.ToString(),
        };

    private static string ComputeSha256(string content)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    private string ResolveOutputRootPath()
    {
        string configuredRootPath = options.Value.PackageStorage.RootPath;
        string outputRootPath = Path.IsPathRooted(configuredRootPath)
            ? configuredRootPath
            : Path.Combine(hostEnvironment.ContentRootPath, configuredRootPath);

        Directory.CreateDirectory(outputRootPath);
        return outputRootPath;
    }

    private static ContentStreamEntity ResolveOrCreateStream(
        ClientProductEntity product,
        string contentAreaKey,
        string sliceKey,
        DateTimeOffset now)
    {
        ContentStreamEntity? stream = product.ContentStreams.FirstOrDefault(existingStream =>
            existingStream.ContentAreaKey.Equals(contentAreaKey, StringComparison.OrdinalIgnoreCase) &&
            existingStream.SliceKey.Equals(sliceKey, StringComparison.OrdinalIgnoreCase));

        if (stream is not null)
        {
            stream.IsActive = true;
            stream.UpdatedAtUtc = now;
            return stream;
        }

        stream = new ContentStreamEntity
        {
            Id = Guid.NewGuid(),
            ClientProductId = product.Id,
            ClientProduct = product,
            ContentAreaKey = contentAreaKey,
            SliceKey = sliceKey,
            LearningLanguageCode = product.LearningLanguageCode,
            SchemaVersion = 1,
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        product.ContentStreams.Add(stream);
        return stream;
    }

    private sealed record PackagePublicationDefinition(
        string PackageId,
        string PackageName,
        string PackageType,
        string ContentAreaKey,
        string SliceKey,
        IReadOnlyList<WordEntry> Words,
        IReadOnlyList<DialogueLesson> Dialogues,
        IReadOnlyList<ConversationStarterPack> ConversationStarterPacks,
        IReadOnlyList<EventPreparationPack> EventPreparationPacks,
        IReadOnlyList<TalkTopic> TalkTopics,
        IReadOnlyList<GrammarTopic> GrammarTopics,
        IReadOnlyList<ExpressionEntry> ExpressionEntries,
        IReadOnlyList<Exercise> Exercises,
        IReadOnlyList<ExerciseSet> ExerciseSets,
        IReadOnlyList<CoursePath> CoursePaths,
        IReadOnlyList<CourseModule> CourseModules,
        IReadOnlyList<CourseLesson> CourseLessons,
        IReadOnlyList<WritingTemplate> WritingTemplates,
        IReadOnlyList<CulturalNote> CulturalNotes,
        IReadOnlyList<ExamProfile> ExamProfiles,
        IReadOnlyList<ExamPrepUnit> ExamPrepUnits)
    {
        public int TotalEntryCount =>
            Words.Count +
            Dialogues.Count +
            ConversationStarterPacks.Count +
            EventPreparationPacks.Count +
            TalkTopics.Count +
            GrammarTopics.Count +
            ExpressionEntries.Count +
            Exercises.Count +
            ExerciseSets.Count +
            CoursePaths.Count +
            CourseModules.Count +
            CourseLessons.Count +
            WritingTemplates.Count +
            CulturalNotes.Count +
            ExamProfiles.Count +
            ExamPrepUnits.Count;
    }

    private sealed record ExportedContentPackage(
        string PackageVersion,
        string PackageId,
        string PackageName,
        string Source,
        string PackageType,
        string ContentAreaKey,
        string SliceKey,
        string LearningLanguageCode,
        IReadOnlyList<string> DefaultMeaningLanguages,
        IReadOnlyList<ExportedContentCollection> Collections,
        IReadOnlyList<ExportedDialogueLesson> Dialogues,
        IReadOnlyList<ExportedConversationStarterPack> ConversationStarterPacks,
        IReadOnlyList<ExportedEventPreparationPack> EventPreparationPacks,
        IReadOnlyList<ExportedTalkTopic> TalkTopics,
        IReadOnlyList<ExportedGrammarTopic> GrammarTopics,
        IReadOnlyList<ExportedExpressionEntry> ExpressionEntries,
        IReadOnlyList<ExportedExercise> Exercises,
        IReadOnlyList<ExportedExerciseSet> ExerciseSets,
        IReadOnlyList<ExportedCoursePath> CoursePaths,
        IReadOnlyList<ExportedCourseModule> CourseModules,
        IReadOnlyList<ExportedCourseLesson> CourseLessons,
        IReadOnlyList<ExportedWritingTemplate> WritingTemplates,
        IReadOnlyList<ExportedCulturalNote> CulturalNotes,
        IReadOnlyList<ExportedExamProfile> ExamProfiles,
        IReadOnlyList<ExportedExamPrepUnit> ExamPrepUnits,
        IReadOnlyList<ExportedContentEntry> Entries);

    private sealed record ExportedContentCollection(
        string Slug,
        string Name,
        string? Description,
        string? ImageUrl,
        int SortOrder,
        IReadOnlyList<ExportedContentCollectionWordReference> Words);

    private sealed record ExportedContentCollectionWordReference(
        string Word,
        string? PartOfSpeech,
        string? CefrLevel);

    private sealed record ExportedContentEntry(
        string Word,
        string Language,
        string CefrLevel,
        string PartOfSpeech,
        IReadOnlyList<ExportedLexicalForm> LexicalForms,
        string? Article,
        string? Plural,
        string? Infinitive,
        string? PronunciationIpa,
        string? SyllableBreak,
        IReadOnlyList<string> Topics,
        IReadOnlyList<string> UsageLabels,
        IReadOnlyList<string> ContextLabels,
        IReadOnlyList<string> GrammarNotes,
        IReadOnlyList<ExportedCollocation> Collocations,
        IReadOnlyList<ExportedWordFamilyMember> WordFamilies,
        IReadOnlyList<ExportedWordRelation> Relations,
        IReadOnlyList<ExportedMeaning> Meanings,
        IReadOnlyList<ExportedExample> Examples);

    private sealed record ExportedMeaning(string Language, string Text);

    private sealed record ExportedLexicalForm(
        string PartOfSpeech,
        string? Article,
        string? Plural,
        string? Infinitive,
        bool IsPrimary);

    private sealed record ExportedExample(string BaseText, IReadOnlyList<ExportedMeaning> Translations);

    private sealed record ExportedCollocation(string Text, string? Meaning);

    private sealed record ExportedWordFamilyMember(string Lemma, string RelationLabel, string? Note);

    private sealed record ExportedWordRelation(string Kind, string Lemma, string? Note);

    private sealed record ExportedDialogueLesson(
        string Slug,
        string Title,
        string Description,
        string LearnerGoal,
        string CefrLevel,
        string Category,
        IReadOnlyList<string> Topics,
        int SortOrder,
        IReadOnlyList<ExportedDialogueTurn> DialogueTurns,
        IReadOnlyList<ExportedDialoguePhrase> UsefulPhrases,
        IReadOnlyList<ExportedDialogueQuestion> Questions);

    private sealed record ExportedDialogueTurn(
        string SpeakerRole,
        string BaseText,
        IReadOnlyList<ExportedMeaning> Translations);

    private sealed record ExportedDialoguePhrase(
        string BaseText,
        string? UsageNote,
        IReadOnlyList<ExportedMeaning> Translations);

    private sealed record ExportedDialogueQuestion(
        string Prompt,
        IReadOnlyList<ExportedMeaning> Translations,
        IReadOnlyList<ExportedDialogueAnswer> Answers);

    private sealed record ExportedDialogueAnswer(
        string Text,
        bool IsCorrect,
        string? Feedback,
        IReadOnlyList<ExportedMeaning> Translations);

    private sealed record ExportedConversationStarterPack(
        string Slug,
        string Title,
        string Description,
        string CefrLevel,
        string Category,
        string Situation,
        string Tone,
        string ConversationGoal,
        IReadOnlyList<string> Topics,
        int SortOrder,
        IReadOnlyList<string> LinkedDialogueSlugs,
        IReadOnlyList<string> LinkedEventPreparationPackSlugs,
        IReadOnlyList<ExportedConversationStarterPhrase> Phrases);

    private sealed record ExportedConversationStarterPhrase(
        string BaseText,
        string Function,
        IReadOnlyList<ExportedMeaning> Translations,
        string? UsageNote,
        string? Register,
        int SortOrder,
        IReadOnlyList<string> AlternativeBaseTexts,
        string? CommonMistake);

    private sealed record ExportedEventPreparationPack(
        string Slug,
        string Title,
        string Description,
        string CefrLevel,
        string Category,
        string EventType,
        IReadOnlyList<string> Topics,
        int SortOrder,
        IReadOnlyList<string> LinkedDialogueSlugs,
        IReadOnlyList<ExportedEventPreparationVocabularyReference> LinkedVocabulary,
        IReadOnlyList<string> LinkedConversationStarterPackSlugs,
        IReadOnlyList<string> OpeningPrompts,
        IReadOnlyList<string> RoleplayPrompts,
        IReadOnlyList<string> ReviewPrompts);

    private sealed record ExportedEventPreparationVocabularyReference(
        string Word,
        string? PartOfSpeech,
        string? CefrLevel);

    private sealed record ExportedTalkTopic(
        string Slug,
        string TopicGroupKey,
        string Title,
        string Description,
        string CefrLevel,
        string Category,
        string ContentType,
        IReadOnlyList<string> Topics,
        string ArticleBaseText,
        IReadOnlyList<ExportedMeaning> ArticleTranslations,
        IReadOnlyList<ExportedTalkTopicWarmupQuestion> WarmupQuestions,
        IReadOnlyList<ExportedTalkTopicDiscussionQuestion> DiscussionQuestions,
        IReadOnlyList<ExportedTalkTopicVocabularyItem> VocabularyItems,
        IReadOnlyList<string> SpeakingGoals,
        bool IsSensitive,
        string? SensitivityNote,
        bool RecommendedForModeratedGroupsOnly,
        int EstimatedReadingMinutes,
        int EstimatedDiscussionMinutes,
        int SortOrder);

    private sealed record ExportedTalkTopicWarmupQuestion(
        string Prompt,
        IReadOnlyList<ExportedMeaning> Translations,
        int SortOrder);

    private sealed record ExportedTalkTopicDiscussionQuestion(
        string Prompt,
        string QuestionType,
        IReadOnlyList<ExportedMeaning> Translations,
        int SortOrder);

    private sealed record ExportedTalkTopicVocabularyItem(
        string Lemma,
        string? WordSlug,
        string? CefrLevel,
        int SortOrder);

    private sealed record ExportedGrammarTopic(
        string Slug,
        string Title,
        string ShortDescription,
        string CefrLevel,
        string GrammarCategory,
        IReadOnlyList<string> Topics,
        bool IsPublished,
        int SortOrder,
        IReadOnlyList<ExportedGrammarSection> Sections,
        IReadOnlyList<ExportedGrammarExample> Examples,
        IReadOnlyList<ExportedGrammarTextItem> RuleSummaries,
        IReadOnlyList<ExportedGrammarCommonMistake> CommonMistakes,
        IReadOnlyList<ExportedGrammarTextItem> ExceptionNotes,
        IReadOnlyList<string> PrerequisiteSlugs,
        IReadOnlyList<string> RelatedTopicSlugs,
        IReadOnlyList<ExportedLinkedWord> LinkedWords,
        IReadOnlyList<string> LinkedDialogueSlugs,
        IReadOnlyList<string> LinkedTalkTopicSlugs,
        IReadOnlyList<string> LinkedExerciseSlugs);

    private sealed record ExportedGrammarSection(
        string Heading,
        string Explanation,
        IReadOnlyList<ExportedGrammarSectionTranslation> Translations,
        int SortOrder);

    private sealed record ExportedGrammarSectionTranslation(string Language, string Heading, string Text);

    private sealed record ExportedGrammarExample(
        string GermanText,
        string? Note,
        IReadOnlyList<ExportedMeaning> Translations,
        int SortOrder);

    private sealed record ExportedGrammarTextItem(
        string Text,
        IReadOnlyList<ExportedMeaning> Translations,
        int SortOrder);

    private sealed record ExportedGrammarCommonMistake(
        string WrongText,
        string CorrectedText,
        string Explanation,
        IReadOnlyList<ExportedMeaning> Translations,
        int SortOrder);

    private sealed record ExportedExpressionEntry(
        string Slug,
        string ExpressionText,
        string? LiteralMeaningText,
        string ActualMeaningText,
        string? UsageExplanation,
        string CefrLevel,
        string ExpressionType,
        string Register,
        string Category,
        string? Context,
        string? Region,
        bool IsRisky,
        string? MeaningTransparency,
        string? TeachingReason,
        string SafetyRating,
        int MinimumAge,
        bool RequiresAdultAccess,
        string? AdultContentCategory,
        string SensitiveContentKind,
        bool RequiresSensitiveOptIn,
        bool RequiresVerifiedAdult,
        string UsagePolicy,
        IReadOnlyList<string> Topics,
        bool IsPublished,
        int SortOrder,
        IReadOnlyList<ExportedExpressionMeaning> Meanings,
        IReadOnlyList<ExportedExpressionExample> Examples,
        IReadOnlyList<ExportedExpressionWarning> Warnings,
        IReadOnlyList<ExportedLinkedWord> LinkedWords,
        IReadOnlyList<string> RelatedExpressionSlugs,
        IReadOnlyList<string> LinkedExerciseSlugs);

    private sealed record ExportedExpressionMeaning(
        string Language,
        string Text,
        string ActualMeaningText,
        string? LiteralMeaningText,
        string? UsageExplanation);

    private sealed record ExportedExpressionExample(
        string GermanText,
        string? Note,
        IReadOnlyList<ExportedMeaning> Translations,
        int SortOrder);

    private sealed record ExportedExpressionWarning(
        string WarningType,
        string Text,
        IReadOnlyList<ExportedMeaning> Translations);

    private sealed record ExportedLinkedWord(string Lemma, string? WordSlug, int SortOrder);

    private sealed record ExportedExercise(
        string Slug,
        string Title,
        string Instruction,
        string CefrLevel,
        string ExerciseType,
        string TargetSkill,
        string OwnerType,
        string? OwnerSlug,
        JsonElement Prompt,
        JsonElement AnswerKey,
        string CorrectExplanation,
        string IncorrectExplanation,
        string? Hint,
        string? CommonMistakeNote,
        bool IsPublished,
        int SortOrder);

    private sealed record ExportedExerciseSet(
        string Slug,
        string Title,
        string Description,
        string CefrLevel,
        string OwnerType,
        string? OwnerSlug,
        IReadOnlyList<string> ExerciseSlugs,
        bool IsPublished,
        int SortOrder);

    private sealed record ExportedCoursePath(
        string Slug,
        string Title,
        string Description,
        string? CefrLevel,
        string CefrRange,
        bool IsPublished,
        int SortOrder);

    private sealed record ExportedCourseModule(
        string Slug,
        string CoursePathSlug,
        string Title,
        string Description,
        int ModuleNumber,
        string CefrLevel,
        bool IsPublished,
        int SortOrder);

    private sealed record ExportedCourseLesson(
        string Slug,
        string CoursePathSlug,
        string ModuleSlug,
        int LessonNumber,
        string Title,
        string ShortDescription,
        string Narrative,
        string CefrLevel,
        int EstimatedMinutes,
        IReadOnlyList<string> LearningGoals,
        IReadOnlyList<string> PrerequisiteLessonSlugs,
        string? NextLessonSlug,
        IReadOnlyList<string> LinkedGrammarTopicSlugs,
        IReadOnlyList<string> LinkedWordSlugs,
        IReadOnlyList<string> LinkedExpressionSlugs,
        IReadOnlyList<string> LinkedDialogueSlugs,
        IReadOnlyList<string> LinkedTalkTopicSlugs,
        IReadOnlyList<string> LinkedExerciseSetSlugs,
        IReadOnlyList<string> LinkedExamPrepSlugs,
        string? ReviewSummary,
        string? HomeworkTask,
        bool IsPublished,
        int SortOrder);

    private sealed record ExportedWritingTemplate(
        string Slug,
        string Title,
        string ShortDescription,
        string CefrLevel,
        string Category,
        string Situation,
        string Register,
        string TemplateText,
        string Explanation,
        IReadOnlyList<string> ReplaceableVariables,
        string SampleFilledVersion,
        IReadOnlyList<string> LinkedGrammarTopicSlugs,
        IReadOnlyList<string> LinkedWordSlugs,
        IReadOnlyList<string> LinkedExpressionSlugs,
        IReadOnlyList<string> LinkedExerciseSlugs,
        bool IsPublished,
        int SortOrder);

    private sealed record ExportedCulturalNote(
        string Slug,
        string Title,
        string ShortDescription,
        string CefrLevel,
        string Category,
        string Context,
        IReadOnlyList<string> Sections,
        IReadOnlyList<ExportedCulturalNoteExample> Examples,
        IReadOnlyList<string> DoNotes,
        IReadOnlyList<string>? Dos,
        IReadOnlyList<string> DontNotes,
        IReadOnlyList<string>? Donts,
        string? SensitivityWarning,
        IReadOnlyList<string> LinkedDialogueSlugs,
        IReadOnlyList<string> LinkedExpressionSlugs,
        IReadOnlyList<string> LinkedWritingTemplateSlugs,
        IReadOnlyList<string> LinkedTalkTopicSlugs,
        IReadOnlyList<string> LinkedCourseLessonSlugs,
        bool IsPublished,
        int SortOrder);

    private sealed record ExportedCulturalNoteExample(string? GermanText, string? Explanation);

    private sealed record ExportedExamProfile(
        string Key,
        string DisplayName,
        string CefrRange,
        string Description,
        bool IsPublished,
        int SortOrder);

    private sealed record ExportedExamPrepUnit(
        string Slug,
        string ExamProfileKey,
        string Title,
        string ShortDescription,
        string CefrLevel,
        string ExamSection,
        string? Section,
        string TaskType,
        string SkillFocus,
        string Explanation,
        IReadOnlyList<string> StrategyNotes,
        IReadOnlyList<string> Checklist,
        IReadOnlyList<string> LinkedDialogueSlugs,
        IReadOnlyList<string> LinkedTalkTopicSlugs,
        IReadOnlyList<string> LinkedGrammarTopicSlugs,
        IReadOnlyList<string> LinkedExpressionSlugs,
        IReadOnlyList<string> LinkedWritingTemplateSlugs,
        IReadOnlyList<string> LinkedExerciseSlugs,
        IReadOnlyList<string> LinkedCourseLessonSlugs,
        bool IsPublished,
        int SortOrder);
}

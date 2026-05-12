using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.ContentOps.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.ContentOps.Application.Abstractions;

/// <summary>
/// Provides persistence and lookup operations required by the content import workflow.
/// </summary>
public interface IContentImportRepository
{
    /// <summary>
    /// Loads the active topic reference rows keyed by their normalized topic keys.
    /// </summary>
    Task<IReadOnlyDictionary<string, Topic>> GetActiveTopicsByKeyAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Loads the active meaning-language codes supported by the platform.
    /// </summary>
    Task<IReadOnlySet<LanguageCode>> GetActiveMeaningLanguagesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether a content package with the same package identifier already exists.
    /// </summary>
    Task<bool> PackageExistsAsync(string packageId, CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether a lexical entry already exists for the normalized lemma.
    /// </summary>
    Task<bool> WordExistsAsync(
        string normalizedLemma,
        CancellationToken cancellationToken);

    /// <summary>
    /// Loads active word entries for the specified normalized lemmas so collection references can be resolved.
    /// </summary>
    Task<IReadOnlyList<WordEntry>> GetActiveWordsByNormalizedLemmasAsync(
        IReadOnlyCollection<string> normalizedLemmas,
        CancellationToken cancellationToken);

    /// <summary>
    /// Persists the completed package audit rows and any imported lexical aggregates in one operation.
    /// </summary>
    Task PersistImportAsync(
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
        CancellationToken cancellationToken);
}

using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Domain.Entities;
using DarwinLingua.ContentOps.Domain.Enums;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DarwinLingua.ContentOps.Application.Services;

/// <summary>
/// Implements the Phase 1 conservative JSON content-package import workflow.
/// </summary>
internal sealed class ContentImportService : IContentImportService
{
    private const string SupportedPackageVersion = "1.0";

    private static readonly HashSet<string> DialogueExamProfiles = new(StringComparer.Ordinal)
    {
        "goethe-a1", "goethe-a2", "goethe-b1", "goethe-b2", "goethe-c1", "goethe-c2",
        "telc-a1", "telc-a2", "telc-b1", "telc-b2", "telc-c1",
        "oeso-a1", "oeso-a2", "oeso-b1", "oeso-b2", "oeso-c1",
        "dtz-a2-b1", "testdaf-b2-c1", "c1-hochschule",
        "berufssprache-b1", "berufssprache-b2", "berufssprache-c1"
    };

    private static readonly HashSet<string> DialogueSkillFocus = new(StringComparer.Ordinal)
    {
        "speaking", "listening-support", "roleplay", "exam-speaking", "phone-call",
        "formal-conversation", "informal-conversation", "workplace-communication",
        "service-interaction", "appointment-management", "complaint-handling",
        "discussion", "negotiation", "presentation-support"
    };

    private static readonly HashSet<string> DialogueTaskTypes = new(StringComparer.Ordinal)
    {
        "introduce-yourself", "ask-for-information", "make-appointment", "reschedule-appointment",
        "explain-problem", "ask-for-help", "order-and-pay", "ask-for-directions",
        "complain-politely", "give-opinion", "agree-disagree", "compare-options",
        "make-suggestion", "refuse-politely", "negotiate-solution", "discuss-plan",
        "describe-experience", "handle-misunderstanding", "workplace-meeting",
        "job-interview", "exam-roleplay", "exam-discussion"
    };

    private static readonly HashSet<string> DialogueInteractionModes = new(StringComparer.Ordinal)
    {
        "face-to-face", "phone", "video-call", "workplace", "classroom", "service-counter",
        "doctor-office", "government-office", "school-kindergarten", "exam-room",
        "pair-work", "group-work"
    };

    private static readonly HashSet<string> DialogueRegisters = new(StringComparer.Ordinal)
    {
        "formal", "informal", "neutral", "mixed"
    };

    private static readonly HashSet<string> DialogueSpeakingFunctions = new(StringComparer.Ordinal)
    {
        "greet", "introduce", "ask-question", "answer-question", "request", "clarify",
        "confirm", "correct", "apologize", "explain", "describe", "suggest", "agree",
        "disagree", "compare", "justify", "complain", "negotiate", "summarize",
        "close-conversation"
    };

    private static readonly HashSet<string> DialoguePromptTypes = new(StringComparer.Ordinal)
    {
        "comprehension", "speaking-prompt", "roleplay-task", "exam-prompt",
        "follow-up-question", "self-correction", "vocabulary-check"
    };

    private static readonly HashSet<string> GrammarCategories = new(StringComparer.Ordinal)
    {
        "articles", "nouns", "gender", "plural", "pronouns", "verbs", "modal-verbs",
        "tenses", "separable-verbs", "reflexive-verbs", "cases", "nominative",
        "accusative", "dative", "genitive", "adjective-declension", "prepositions",
        "word-order", "subordinate-clauses", "connectors", "negation", "questions",
        "imperative", "passive", "konjunktiv", "reported-speech", "punctuation"
    };

    private static readonly HashSet<string> GrammarRichBlockTypes = new(StringComparer.Ordinal)
    {
        "paragraph", "table", "callout", "rule-list", "example-list", "mistake-pair", "image-slot"
    };

    private static readonly HashSet<string> ExpressionTypes = new(StringComparer.Ordinal)
    {
        "idiom", "colloquial-phrase", "proverb", "fixed-expression", "slang",
        "cultural-phrase", "false-friend", "regional-expression", "polite-formula",
        "warning-phrase"
    };

    private static readonly HashSet<string> ExpressionRegisters = new(StringComparer.Ordinal)
    {
        "formal", "informal", "neutral", "colloquial", "slang", "rude", "polite",
        "workplace-safe", "friends-only", "regional"
    };

    private static readonly HashSet<string> RiskyExpressionRegisters = new(StringComparer.Ordinal)
    {
        "slang", "rude", "friends-only"
    };

    private static readonly HashSet<string> RiskyExpressionTypes = new(StringComparer.Ordinal)
    {
        "slang", "warning-phrase"
    };

    private static readonly HashSet<string> ExerciseTypes = new(StringComparer.Ordinal)
    {
        "multiple-choice", "fill-in-the-blank", "matching", "sentence-ordering",
        "error-correction", "article-selection", "case-selection", "conjugation",
        "translation-controlled", "dialogue-completion", "vocabulary-choice", "grammar-choice"
    };

    private static readonly HashSet<string> ExerciseTargetSkills = new(StringComparer.Ordinal)
    {
        "grammar", "vocabulary", "reading", "listening", "speaking", "writing",
        "pronunciation", "exam-preparation"
    };

    private static readonly HashSet<string> ExerciseOwnerTypes = new(StringComparer.Ordinal)
    {
        "word", "grammar-topic", "expression", "dialogue", "talk-topic",
        "course-lesson", "exam-prep-unit"
    };

    private static readonly HashSet<string> WritingTemplateCategories = new(StringComparer.Ordinal)
    {
        "email-to-school", "email-to-kindergarten", "message-to-landlord",
        "doctor-appointment-request", "appointment-reschedule", "sick-note-to-employer",
        "complaint", "application-email", "cancellation", "insurance-message",
        "government-office-message", "exam-email", "exam-opinion-text"
    };

    private static readonly HashSet<string> WritingTemplateRegisters = new(StringComparer.Ordinal)
    {
        "formal", "informal", "neutral", "official", "workplace", "exam"
    };

    private static readonly HashSet<string> CulturalNoteCategories = new(StringComparer.Ordinal)
    {
        "du-vs-sie", "politeness", "directness", "small-talk", "workplace-culture",
        "office-communication", "school-kindergarten", "doctor-visit", "appointments",
        "punctuality", "complaints", "bureaucracy", "conversation-cafe-etiquette"
    };

    private static readonly HashSet<string> ExamPrepProfiles = new(StringComparer.Ordinal)
    {
        "goethe-a1", "goethe-a2", "goethe-b1", "goethe-b2",
        "telc-a1", "telc-a2", "telc-b1", "telc-b2",
        "dtz-a2-b1", "berufssprache-b1", "berufssprache-b2",
        "c1-hochschule", "testdaf"
    };

    private static readonly HashSet<string> ExamPrepSections = new(StringComparer.Ordinal)
    {
        "speaking", "writing", "reading", "listening", "grammar-vocabulary",
        "overview", "strategy", "mock-task"
    };

    private static readonly HashSet<string> ExamPrepTaskTypes = new(StringComparer.Ordinal)
    {
        "overview", "strategy", "roleplay", "discussion", "presentation", "email",
        "opinion-text", "form-filling", "reading-task", "listening-task",
        "grammar-vocabulary", "mock-task", "scoring-checklist"
    };

    private readonly IContentImportFileReader _contentImportFileReader;
    private readonly IContentImportParser _contentImportParser;
    private readonly IContentImportRepository _contentImportRepository;

    public ContentImportService(
        IContentImportFileReader contentImportFileReader,
        IContentImportParser contentImportParser,
        IContentImportRepository contentImportRepository)
    {
        ArgumentNullException.ThrowIfNull(contentImportFileReader);
        ArgumentNullException.ThrowIfNull(contentImportParser);
        ArgumentNullException.ThrowIfNull(contentImportRepository);

        _contentImportFileReader = contentImportFileReader;
        _contentImportParser = contentImportParser;
        _contentImportRepository = contentImportRepository;
    }

    public async Task<ImportContentPackageResult> ImportAsync(
        ImportContentPackageRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FilePath);

        List<ImportIssueModel> issues = [];
        string fileName = Path.GetFileName(request.FilePath);

        string rawContent;

        try
        {
            rawContent = await _contentImportFileReader
                .ReadAllTextAsync(request.FilePath, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DirectoryNotFoundException or FileNotFoundException)
        {
            issues.Add(new ImportIssueModel(null, "Error", $"The package file could not be read: {exception.Message}"));
            return CreateFatalFailureResult(fileName, issues);
        }

        ParsedContentPackageModel parsedPackage;

        try
        {
            parsedPackage = await _contentImportParser.ParseAsync(rawContent, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is InvalidDataException or FormatException)
        {
            issues.Add(new ImportIssueModel(null, "Error", exception.Message));
            return CreateFatalFailureResult(fileName, issues);
        }

        ValidatePackage(parsedPackage, issues);

        if (issues.Any(issue => issue.EntryIndex is null && string.Equals(issue.Severity, "Error", StringComparison.Ordinal)))
        {
            return CreateFatalFailureResult(parsedPackage.PackageId, issues, parsedPackage.PackageName, parsedPackage.Entries.Count);
        }

        bool packageExists = await _contentImportRepository.PackageExistsAsync(parsedPackage.PackageId, cancellationToken).ConfigureAwait(false);
        if (packageExists && !CanReimportPackageByContentSlug(parsedPackage))
        {
            issues.Add(new ImportIssueModel(null, "Error", $"A content package with id '{parsedPackage.PackageId}' already exists."));
            return CreateFatalFailureResult(parsedPackage.PackageId, issues, parsedPackage.PackageName, parsedPackage.Entries.Count);
        }

        IReadOnlyDictionary<string, Topic> topicsByKey = await _contentImportRepository
            .GetActiveTopicsByKeyAsync(cancellationToken)
            .ConfigureAwait(false);
        IReadOnlySet<LanguageCode> meaningLanguages = await _contentImportRepository
            .GetActiveMeaningLanguagesAsync(cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<string> missingActiveMeaningLanguages = ContentLanguageRequirements.FindMissingMeaningLanguages(
            meaningLanguages.Select(language => language.Value));
        if (missingActiveMeaningLanguages.Count > 0)
        {
            issues.Add(new ImportIssueModel(
                null,
                "Error",
                $"The database is missing required active meaning languages: {string.Join(", ", missingActiveMeaningLanguages)}."));
            return CreateFatalFailureResult(parsedPackage.PackageId, issues, parsedPackage.PackageName, parsedPackage.Entries.Count);
        }

        LanguageCode[] expectedMeaningLanguages = ContentLanguageRequirements.RequiredMeaningLanguageCodes
            .Select(LanguageCode.From)
            .ToArray();

        ValidateDialogues(parsedPackage.Dialogues, topicsByKey, meaningLanguages, issues);
        ValidateTalkTopics(parsedPackage.TalkTopics, topicsByKey, meaningLanguages, issues);
        ValidateGrammarTopics(parsedPackage.GrammarTopics, topicsByKey, meaningLanguages, issues);
        ValidateExpressionEntries(parsedPackage.ExpressionEntries, topicsByKey, meaningLanguages, issues);
        ValidateExercises(parsedPackage.Exercises, parsedPackage.ExerciseSets, issues);
        ValidateCourses(parsedPackage.CoursePaths, parsedPackage.CourseModules, parsedPackage.CourseLessons, issues);
        ValidateWritingTemplates(parsedPackage.WritingTemplates, issues);
        ValidateCulturalNotes(parsedPackage.CulturalNotes, issues);
        ValidateExamPrep(parsedPackage.ExamProfiles, parsedPackage.ExamPrepUnits, issues);
        ValidateConversationStarterPacks(parsedPackage.ConversationStarterPacks, topicsByKey, meaningLanguages, issues);
        ValidateEventPreparationPacks(parsedPackage.EventPreparationPacks, topicsByKey, issues);
        Dictionary<WordLabelKind, HashSet<string>> allowedLabelsByKind = ValidateLabels(
            parsedPackage.Labels,
            parsedPackage.Entries.Any(EntryReferencesLabels),
            issues);

        if (issues.Any(issue => issue.EntryIndex is null && string.Equals(issue.Severity, "Error", StringComparison.Ordinal)))
        {
            return CreateFatalFailureResult(parsedPackage.PackageId, issues, parsedPackage.PackageName, parsedPackage.Entries.Count);
        }

        ContentPackage contentPackage = new(
            Guid.NewGuid(),
            packageExists ? CreateReimportPackageId(parsedPackage.PackageId, DateTime.UtcNow) : parsedPackage.PackageId,
            parsedPackage.PackageVersion,
            parsedPackage.PackageName,
            ResolveSourceType(parsedPackage.Source),
            fileName,
            parsedPackage.Entries.Count,
            DateTime.UtcNow);

        contentPackage.MarkProcessing(DateTime.UtcNow);

        List<WordEntry> importedWords = [];
        List<LabelDefinition> importedLabelDefinitions = [];
        List<WordCollection> importedCollections = [];
        List<DialogueLesson> importedDialogues = [];
        List<TalkTopic> importedTalkTopics = [];
        List<GrammarTopic> importedGrammarTopics = [];
        List<ExpressionEntry> importedExpressions = [];
        List<Exercise> importedExercises = [];
        List<ExerciseSet> importedExerciseSets = [];
        List<CoursePath> importedCoursePaths = [];
        List<CourseModule> importedCourseModules = [];
        List<CourseLesson> importedCourseLessons = [];
        List<WritingTemplate> importedWritingTemplates = [];
        List<CulturalNote> importedCulturalNotes = [];
        List<ExamProfile> importedExamProfiles = [];
        List<ExamPrepUnit> importedExamPrepUnits = [];
        List<ConversationStarterPack> importedConversationStarterPacks = [];
        List<EventPreparationPack> importedEventPreparationPacks = [];

        ProcessLabelDefinitions(parsedPackage.Labels, importedLabelDefinitions);

        for (int entryIndex = 0; entryIndex < parsedPackage.Entries.Count; entryIndex++)
        {
            ParsedContentEntryModel entry = parsedPackage.Entries[entryIndex];
            await ProcessEntryAsync(
                entryIndex,
                entry,
                topicsByKey,
                meaningLanguages,
                expectedMeaningLanguages,
                allowedLabelsByKind,
                contentPackage,
                importedWords,
                issues,
                cancellationToken).ConfigureAwait(false);
        }

        await ProcessCollectionsAsync(
            parsedPackage.Collections,
            importedWords,
            importedCollections,
            issues,
            cancellationToken).ConfigureAwait(false);

        ProcessDialogues(parsedPackage.Dialogues, topicsByKey, importedDialogues);
        ProcessTalkTopics(parsedPackage.TalkTopics, topicsByKey, importedTalkTopics);
        ProcessGrammarTopics(parsedPackage.GrammarTopics, topicsByKey, importedGrammarTopics);
        ProcessExpressionEntries(parsedPackage.ExpressionEntries, topicsByKey, importedExpressions);
        ProcessExercises(parsedPackage.Exercises, importedExercises);
        ProcessExerciseSets(parsedPackage.ExerciseSets, importedExerciseSets);
        ProcessCourses(parsedPackage.CoursePaths, parsedPackage.CourseModules, parsedPackage.CourseLessons, importedCoursePaths, importedCourseModules, importedCourseLessons);
        ProcessWritingTemplates(parsedPackage.WritingTemplates, importedWritingTemplates);
        ProcessCulturalNotes(parsedPackage.CulturalNotes, importedCulturalNotes);
        ProcessExamPrep(parsedPackage.ExamProfiles, parsedPackage.ExamPrepUnits, importedExamProfiles, importedExamPrepUnits);
        ProcessConversationStarterPacks(parsedPackage.ConversationStarterPacks, topicsByKey, importedConversationStarterPacks);
        ProcessEventPreparationPacks(parsedPackage.EventPreparationPacks, topicsByKey, importedEventPreparationPacks);

        contentPackage.Complete(DateTime.UtcNow);

        await _contentImportRepository
            .PersistImportAsync(contentPackage, importedLabelDefinitions, importedWords, importedCollections, importedDialogues, importedTalkTopics, importedGrammarTopics, importedExpressions, importedExercises, importedExerciseSets, importedCoursePaths, importedCourseModules, importedCourseLessons, importedWritingTemplates, importedCulturalNotes, importedExamProfiles, importedExamPrepUnits, importedConversationStarterPacks, importedEventPreparationPacks, cancellationToken)
            .ConfigureAwait(false);

        return new ImportContentPackageResult(
            true,
            contentPackage.PackageId,
            contentPackage.PackageName,
            contentPackage.Status.ToString(),
            contentPackage.TotalEntries,
            contentPackage.InsertedEntries,
            contentPackage.SkippedDuplicateEntries,
            contentPackage.InvalidEntries,
            contentPackage.WarningCount,
            issues,
            importedWords
                .Select(word => word.Lemma)
                .ToArray());
    }

    private static void ProcessDialogues(
        IReadOnlyList<ParsedDialogueLessonModel> dialogues,
        IReadOnlyDictionary<string, Topic> topicsByKey,
        ICollection<DialogueLesson> importedDialogues)
    {
        DateTime timestampUtc = DateTime.UtcNow;

        foreach (ParsedDialogueLessonModel dialogue in dialogues)
        {
            DialogueLesson lesson = new(
                Guid.NewGuid(),
                NormalizeText(dialogue.Slug),
                NormalizeText(dialogue.Title),
                NormalizeText(dialogue.Description),
                NormalizeText(dialogue.LearnerGoal),
                Enum.Parse<CefrLevel>(NormalizeText(dialogue.CefrLevel), true),
                NormalizeText(dialogue.Category),
                PublicationStatus.Active,
                dialogue.SortOrder < 0 ? 0 : dialogue.SortOrder,
                timestampUtc);

            lesson.UpdateExamMetadata(
                NormalizeText(dialogue.TaskType),
                NormalizeText(dialogue.InteractionMode),
                NormalizeText(dialogue.Register),
                dialogue.EstimatedPracticeMinutes <= 0 ? 15 : dialogue.EstimatedPracticeMinutes,
                dialogue.DifficultyNote,
                dialogue.ExamRelevance,
                timestampUtc);

            string[] topicKeys = dialogue.Topics
                .Select(topic => NormalizeText(topic).ToLowerInvariant())
                .Where(topic => !string.IsNullOrWhiteSpace(topic))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int topicIndex = 0; topicIndex < topicKeys.Length; topicIndex++)
            {
                lesson.AddTopic(Guid.NewGuid(), topicsByKey[topicKeys[topicIndex]].Id, topicIndex == 0, timestampUtc);
            }

            string[] examProfiles = dialogue.ExamProfiles
                .Select(profile => NormalizeText(profile).ToLowerInvariant())
                .Where(profile => !string.IsNullOrWhiteSpace(profile))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int profileIndex = 0; profileIndex < examProfiles.Length; profileIndex++)
            {
                lesson.AddExamProfile(Guid.NewGuid(), examProfiles[profileIndex], (profileIndex + 1) * 10, timestampUtc);
            }

            string[] skillFocus = dialogue.SkillFocus
                .Select(focus => NormalizeText(focus).ToLowerInvariant())
                .Where(focus => !string.IsNullOrWhiteSpace(focus))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int focusIndex = 0; focusIndex < skillFocus.Length; focusIndex++)
            {
                lesson.AddSkillFocus(Guid.NewGuid(), skillFocus[focusIndex], (focusIndex + 1) * 10, timestampUtc);
            }

            string[] speakingFunctions = dialogue.SpeakingFunctions
                .Select(function => NormalizeText(function).ToLowerInvariant())
                .Where(function => !string.IsNullOrWhiteSpace(function))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int functionIndex = 0; functionIndex < speakingFunctions.Length; functionIndex++)
            {
                lesson.AddSpeakingFunction(Guid.NewGuid(), speakingFunctions[functionIndex], (functionIndex + 1) * 10, timestampUtc);
            }

            for (int wordIndex = 0; wordIndex < dialogue.UsefulWords.Count; wordIndex++)
            {
                ParsedDialogueUsefulWordModel word = dialogue.UsefulWords[wordIndex];
                CefrLevel? cefrLevel = Enum.TryParse(NormalizeText(word.CefrLevel), true, out CefrLevel parsedWordCefr)
                    ? parsedWordCefr
                    : null;
                lesson.AddUsefulWord(
                    Guid.NewGuid(),
                    word.Lemma,
                    word.WordSlug,
                    cefrLevel,
                    word.SortOrder <= 0 ? (wordIndex + 1) * 10 : word.SortOrder,
                    timestampUtc);
            }

            for (int promptIndex = 0; promptIndex < dialogue.SpeakingPrompts.Count; promptIndex++)
            {
                ParsedDialogueSpeakingPromptModel parsedPrompt = dialogue.SpeakingPrompts[promptIndex];
                DialogueSpeakingPrompt prompt = lesson.AddSpeakingPrompt(
                    Guid.NewGuid(),
                    parsedPrompt.SortOrder <= 0 ? (promptIndex + 1) * 10 : parsedPrompt.SortOrder,
                    parsedPrompt.PromptType,
                    parsedPrompt.Prompt,
                    timestampUtc);

                AddDialogueTranslations(prompt.AddTranslation, parsedPrompt.Translations, timestampUtc);
            }

            for (int turnIndex = 0; turnIndex < dialogue.DialogueTurns.Count; turnIndex++)
            {
                ParsedDialogueTurnModel parsedTurn = dialogue.DialogueTurns[turnIndex];
                DialogueTurn turn = lesson.AddDialogueTurn(
                    Guid.NewGuid(),
                    turnIndex + 1,
                    parsedTurn.SpeakerRole,
                    parsedTurn.BaseText,
                    timestampUtc);

                AddDialogueTranslations(turn.AddTranslation, parsedTurn.Translations, timestampUtc);
            }

            for (int phraseIndex = 0; phraseIndex < dialogue.UsefulPhrases.Count; phraseIndex++)
            {
                ParsedDialoguePhraseModel parsedPhrase = dialogue.UsefulPhrases[phraseIndex];
                DialoguePhrase phrase = lesson.AddUsefulPhrase(
                    Guid.NewGuid(),
                    phraseIndex + 1,
                    parsedPhrase.BaseText,
                    parsedPhrase.UsageNote,
                    timestampUtc);

                AddDialogueTranslations(phrase.AddTranslation, parsedPhrase.Translations, timestampUtc);
            }

            for (int questionIndex = 0; questionIndex < dialogue.Questions.Count; questionIndex++)
            {
                ParsedDialogueQuestionModel parsedQuestion = dialogue.Questions[questionIndex];
                DialogueQuestion question = lesson.AddQuestion(
                    Guid.NewGuid(),
                    questionIndex + 1,
                    parsedQuestion.Prompt,
                    timestampUtc);

                AddDialogueTranslations(question.AddTranslation, parsedQuestion.Translations, timestampUtc);

                for (int answerIndex = 0; answerIndex < parsedQuestion.Answers.Count; answerIndex++)
                {
                    ParsedDialogueAnswerModel parsedAnswer = parsedQuestion.Answers[answerIndex];
                    DialogueAnswer answer = question.AddAnswer(
                        Guid.NewGuid(),
                        answerIndex + 1,
                        parsedAnswer.Text,
                        parsedAnswer.IsCorrect,
                        parsedAnswer.Feedback,
                        timestampUtc);

                    AddDialogueTranslations(answer.AddTranslation, parsedAnswer.Translations, timestampUtc);
                }
            }

            importedDialogues.Add(lesson);
        }
    }

    private static void ProcessConversationStarterPacks(
        IReadOnlyList<ParsedConversationStarterPackModel> starterPacks,
        IReadOnlyDictionary<string, Topic> topicsByKey,
        ICollection<ConversationStarterPack> importedStarterPacks)
    {
        DateTime timestampUtc = DateTime.UtcNow;

        foreach (ParsedConversationStarterPackModel starterPack in starterPacks)
        {
            ConversationStarterPack pack = new(
                Guid.NewGuid(),
                NormalizeText(starterPack.Slug),
                NormalizeText(starterPack.Title),
                NormalizeText(starterPack.Description),
                Enum.Parse<CefrLevel>(NormalizeText(starterPack.CefrLevel), true),
                NormalizeText(starterPack.Category),
                NormalizeText(starterPack.Situation),
                NormalizeText(starterPack.Tone),
                NormalizeText(starterPack.ConversationGoal),
                PublicationStatus.Active,
                starterPack.SortOrder < 0 ? 0 : starterPack.SortOrder,
                timestampUtc);

            string[] topicKeys = starterPack.Topics
                .Select(topic => NormalizeText(topic).ToLowerInvariant())
                .Where(topic => !string.IsNullOrWhiteSpace(topic))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int topicIndex = 0; topicIndex < topicKeys.Length; topicIndex++)
            {
                pack.AddTopic(Guid.NewGuid(), topicsByKey[topicKeys[topicIndex]].Id, topicIndex == 0, timestampUtc);
            }

            string[] dialogueSlugs = starterPack.LinkedDialogueSlugs
                .Select(slug => NormalizeText(slug).ToLowerInvariant())
                .Where(slug => !string.IsNullOrWhiteSpace(slug))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int dialogueIndex = 0; dialogueIndex < dialogueSlugs.Length; dialogueIndex++)
            {
                pack.AddLinkedDialogue(Guid.NewGuid(), dialogueSlugs[dialogueIndex], dialogueIndex + 1, timestampUtc);
            }

            string[] eventPreparationPackSlugs = starterPack.LinkedEventPreparationPackSlugs
                .Select(slug => NormalizeText(slug).ToLowerInvariant())
                .Where(slug => !string.IsNullOrWhiteSpace(slug))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int eventPackIndex = 0; eventPackIndex < eventPreparationPackSlugs.Length; eventPackIndex++)
            {
                pack.AddLinkedEventPreparationPack(Guid.NewGuid(), eventPreparationPackSlugs[eventPackIndex], eventPackIndex + 1, timestampUtc);
            }

            for (int phraseIndex = 0; phraseIndex < starterPack.Phrases.Count; phraseIndex++)
            {
                ParsedConversationStarterPhraseModel parsedPhrase = starterPack.Phrases[phraseIndex];
                ConversationStarterPhrase phrase = pack.AddPhrase(
                    Guid.NewGuid(),
                    parsedPhrase.SortOrder <= 0 ? phraseIndex + 1 : parsedPhrase.SortOrder,
                    parsedPhrase.BaseText,
                    parsedPhrase.Function,
                    parsedPhrase.UsageNote,
                    parsedPhrase.Register,
                    parsedPhrase.CommonMistake,
                    timestampUtc);

                AddConversationStarterTranslations(phrase.AddTranslation, parsedPhrase.Translations, timestampUtc);

                for (int alternativeIndex = 0; alternativeIndex < parsedPhrase.AlternativeBaseTexts.Count; alternativeIndex++)
                {
                    phrase.AddAlternativeBaseText(
                        Guid.NewGuid(),
                        alternativeIndex + 1,
                        parsedPhrase.AlternativeBaseTexts[alternativeIndex],
                        timestampUtc);
                }
            }

            importedStarterPacks.Add(pack);
        }
    }

    private static void ProcessTalkTopics(
        IReadOnlyList<ParsedTalkTopicModel> talkTopics,
        IReadOnlyDictionary<string, Topic> topicsByKey,
        ICollection<TalkTopic> importedTalkTopics)
    {
        DateTime timestampUtc = DateTime.UtcNow;

        foreach (ParsedTalkTopicModel parsedTopic in talkTopics)
        {
            TalkTopic topic = new(
                Guid.NewGuid(),
                NormalizeText(parsedTopic.Slug),
                NormalizeText(parsedTopic.TopicGroupKey),
                NormalizeText(parsedTopic.Title),
                NormalizeText(parsedTopic.Description),
                Enum.Parse<CefrLevel>(NormalizeText(parsedTopic.CefrLevel), true),
                NormalizeText(parsedTopic.Category),
                ParseTalkTopicContentType(parsedTopic.ContentType),
                NormalizeText(parsedTopic.Article.BaseText),
                parsedTopic.EstimatedReadingMinutes <= 0 ? 1 : parsedTopic.EstimatedReadingMinutes,
                parsedTopic.EstimatedDiscussionMinutes <= 0 ? 1 : parsedTopic.EstimatedDiscussionMinutes,
                parsedTopic.IsSensitive,
                parsedTopic.SensitivityNote,
                parsedTopic.RecommendedForModeratedGroupsOnly,
                parsedTopic.IsPublished ? PublicationStatus.Active : PublicationStatus.Draft,
                parsedTopic.SortOrder < 0 ? 0 : parsedTopic.SortOrder,
                timestampUtc);

            string[] topicKeys = parsedTopic.Topics
                .Select(item => NormalizeText(item).ToLowerInvariant())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int topicIndex = 0; topicIndex < topicKeys.Length; topicIndex++)
            {
                if (topicsByKey.TryGetValue(topicKeys[topicIndex], out Topic? linkedTopic))
                {
                    topic.AddTopic(Guid.NewGuid(), linkedTopic.Id, topicIndex == 0, timestampUtc);
                }
            }

            for (int questionIndex = 0; questionIndex < parsedTopic.WarmupQuestions.Count; questionIndex++)
            {
                ParsedTalkTopicQuestionModel parsedQuestion = parsedTopic.WarmupQuestions[questionIndex];
                topic.AddWarmupQuestion(
                    Guid.NewGuid(),
                    parsedQuestion.SortOrder <= 0 ? questionIndex + 1 : parsedQuestion.SortOrder,
                    parsedQuestion.Prompt,
                    timestampUtc);
            }

            for (int questionIndex = 0; questionIndex < parsedTopic.DiscussionQuestions.Count; questionIndex++)
            {
                ParsedTalkTopicDiscussionQuestionModel parsedQuestion = parsedTopic.DiscussionQuestions[questionIndex];
                topic.AddDiscussionQuestion(
                    Guid.NewGuid(),
                    ParseTalkTopicQuestionType(parsedQuestion.QuestionType),
                    parsedQuestion.SortOrder <= 0 ? questionIndex + 1 : parsedQuestion.SortOrder,
                    parsedQuestion.Prompt,
                    timestampUtc);
            }

            for (int vocabularyIndex = 0; vocabularyIndex < parsedTopic.VocabularyItems.Count; vocabularyIndex++)
            {
                ParsedTalkTopicVocabularyItemModel item = parsedTopic.VocabularyItems[vocabularyIndex];
                CefrLevel? cefrLevel = Enum.TryParse(NormalizeText(item.CefrLevel), true, out CefrLevel parsedCefrLevel)
                    ? parsedCefrLevel
                    : null;
                topic.AddVocabularyItem(
                    Guid.NewGuid(),
                    item.Lemma,
                    NormalizeOptionalText(item.WordSlug),
                    cefrLevel,
                    item.SortOrder <= 0 ? vocabularyIndex + 1 : item.SortOrder,
                    timestampUtc);
            }

            string[] speakingGoals = parsedTopic.SpeakingGoals
                .Select(goal => NormalizeText(goal).ToLowerInvariant())
                .Where(goal => !string.IsNullOrWhiteSpace(goal))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int goalIndex = 0; goalIndex < speakingGoals.Length; goalIndex++)
            {
                topic.AddSpeakingGoal(
                    Guid.NewGuid(),
                    ParseTalkTopicSpeakingGoal(speakingGoals[goalIndex]),
                    goalIndex + 1,
                    timestampUtc);
            }

            importedTalkTopics.Add(topic);
        }
    }

    private static void AddConversationStarterTranslations(
        Action<Guid, LanguageCode, string, DateTime> addTranslation,
        IReadOnlyList<ParsedContentMeaningModel> translations,
        DateTime timestampUtc)
    {
        foreach (ParsedContentMeaningModel translation in translations)
        {
            addTranslation(
                Guid.NewGuid(),
                LanguageCode.From(NormalizeText(translation.Language).ToLowerInvariant()),
                NormalizeText(translation.Text),
                timestampUtc);
        }
    }

    private static void ProcessGrammarTopics(
        IReadOnlyList<ParsedGrammarTopicModel> grammarTopics,
        IReadOnlyDictionary<string, Topic> topicsByKey,
        ICollection<GrammarTopic> importedGrammarTopics)
    {
        DateTime timestampUtc = DateTime.UtcNow;

        foreach (ParsedGrammarTopicModel parsedTopic in grammarTopics)
        {
            GrammarTopic topic = new(
                Guid.NewGuid(),
                NormalizeText(parsedTopic.Slug),
                NormalizeText(parsedTopic.Title),
                NormalizeText(parsedTopic.ShortDescription),
                Enum.Parse<CefrLevel>(NormalizeText(parsedTopic.CefrLevel), true),
                NormalizeText(parsedTopic.GrammarCategory),
                parsedTopic.IsPublished ? PublicationStatus.Active : PublicationStatus.Draft,
                parsedTopic.SortOrder < 0 ? 0 : parsedTopic.SortOrder,
                timestampUtc);
            topic.SetRichContentMetadata(
                parsedTopic.ContentRevision,
                SerializeLocalizedTextDictionary(parsedTopic.TitleLocalized),
                SerializeLocalizedTextDictionary(parsedTopic.ShortDescriptionLocalized),
                parsedTopic.ImageSlotsJson,
                timestampUtc);

            string[] topicKeys = parsedTopic.Topics
                .Select(item => NormalizeText(item).ToLowerInvariant())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int topicIndex = 0; topicIndex < topicKeys.Length; topicIndex++)
            {
                if (topicsByKey.TryGetValue(topicKeys[topicIndex], out Topic? linkedTopic))
                {
                    topic.AddTopic(Guid.NewGuid(), linkedTopic.Id, topicIndex == 0, timestampUtc);
                }
            }

            for (int sectionIndex = 0; sectionIndex < parsedTopic.Sections.Count; sectionIndex++)
            {
                ParsedGrammarSectionModel parsedSection = parsedTopic.Sections[sectionIndex];
                GrammarSection section = topic.AddSection(
                    Guid.NewGuid(),
                    parsedSection.SortOrder <= 0 ? (sectionIndex + 1) * 10 : parsedSection.SortOrder,
                    NormalizeText(parsedSection.Heading),
                    NormalizeText(parsedSection.Explanation),
                    timestampUtc,
                    NormalizeOptionalText(parsedSection.SectionKey),
                    SerializeLocalizedBlocks(parsedSection.LocalizedBlocksJson));

                foreach (ParsedGrammarSectionTranslationModel translation in parsedSection.Translations)
                {
                    section.AddTranslation(
                        Guid.NewGuid(),
                        LanguageCode.From(NormalizeText(translation.Language).ToLowerInvariant()),
                        NormalizeText(translation.Heading),
                        NormalizeText(translation.Text),
                        timestampUtc);
                }
            }

            for (int exampleIndex = 0; exampleIndex < parsedTopic.Examples.Count; exampleIndex++)
            {
                ParsedGrammarExampleModel parsedExample = parsedTopic.Examples[exampleIndex];
                GrammarExample example = topic.AddExample(
                    Guid.NewGuid(),
                    parsedExample.SortOrder <= 0 ? (exampleIndex + 1) * 10 : parsedExample.SortOrder,
                    NormalizeText(parsedExample.GermanText),
                    NormalizeOptionalText(parsedExample.Note),
                    timestampUtc);
                AddGrammarTranslations(example.AddTranslation, parsedExample.Translations, timestampUtc);
            }

            for (int ruleIndex = 0; ruleIndex < parsedTopic.RuleSummaries.Count; ruleIndex++)
            {
                ParsedGrammarTextItemModel parsedRule = parsedTopic.RuleSummaries[ruleIndex];
                GrammarRuleSummary rule = topic.AddRuleSummary(
                    Guid.NewGuid(),
                    parsedRule.SortOrder <= 0 ? (ruleIndex + 1) * 10 : parsedRule.SortOrder,
                    NormalizeText(parsedRule.Text),
                    timestampUtc);
                AddGrammarTranslations(rule.AddTranslation, parsedRule.Translations, timestampUtc);
            }

            for (int mistakeIndex = 0; mistakeIndex < parsedTopic.CommonMistakes.Count; mistakeIndex++)
            {
                ParsedGrammarCommonMistakeModel parsedMistake = parsedTopic.CommonMistakes[mistakeIndex];
                GrammarCommonMistake mistake = topic.AddCommonMistake(
                    Guid.NewGuid(),
                    parsedMistake.SortOrder <= 0 ? (mistakeIndex + 1) * 10 : parsedMistake.SortOrder,
                    NormalizeText(parsedMistake.WrongText),
                    NormalizeText(parsedMistake.CorrectedText),
                    NormalizeText(parsedMistake.Explanation),
                    timestampUtc);
                AddGrammarTranslations(mistake.AddTranslation, parsedMistake.Translations, timestampUtc);
            }

            for (int noteIndex = 0; noteIndex < parsedTopic.ExceptionNotes.Count; noteIndex++)
            {
                ParsedGrammarTextItemModel parsedNote = parsedTopic.ExceptionNotes[noteIndex];
                GrammarExceptionNote note = topic.AddExceptionNote(
                    Guid.NewGuid(),
                    parsedNote.SortOrder <= 0 ? (noteIndex + 1) * 10 : parsedNote.SortOrder,
                    NormalizeText(parsedNote.Text),
                    timestampUtc);
                AddGrammarTranslations(note.AddTranslation, parsedNote.Translations, timestampUtc);
            }

            AddGrammarSlugLinks(parsedTopic.PrerequisiteSlugs, topic.AddPrerequisite, timestampUtc);
            AddGrammarSlugLinks(parsedTopic.RelatedTopicSlugs, topic.AddRelatedTopic, timestampUtc);
            AddGrammarSlugLinks(parsedTopic.LinkedDialogueSlugs, topic.AddLinkedDialogue, timestampUtc);
            AddGrammarSlugLinks(parsedTopic.LinkedTalkTopicSlugs, topic.AddLinkedTalkTopic, timestampUtc);
            AddGrammarSlugLinks(parsedTopic.LinkedExerciseSlugs, topic.AddLinkedExercise, timestampUtc);

            for (int wordIndex = 0; wordIndex < parsedTopic.LinkedWords.Count; wordIndex++)
            {
                ParsedGrammarLinkedWordModel word = parsedTopic.LinkedWords[wordIndex];
                topic.AddLinkedWord(
                    Guid.NewGuid(),
                    NormalizeText(word.Lemma),
                    NormalizeOptionalText(word.WordSlug),
                    word.SortOrder <= 0 ? (wordIndex + 1) * 10 : word.SortOrder,
                    timestampUtc);
            }

            importedGrammarTopics.Add(topic);
        }
    }

    private static void AddGrammarSlugLinks(
        IReadOnlyList<string> slugs,
        Action<Guid, string, int, DateTime> addLink,
        DateTime timestampUtc)
    {
        for (int index = 0; index < slugs.Count; index++)
        {
            addLink(Guid.NewGuid(), NormalizeText(slugs[index]), (index + 1) * 10, timestampUtc);
        }
    }

    private static void AddGrammarTranslations(
        Action<Guid, LanguageCode, string, DateTime> addTranslation,
        IReadOnlyList<ParsedContentMeaningModel> translations,
        DateTime timestampUtc)
    {
        foreach (ParsedContentMeaningModel translation in translations)
        {
            addTranslation(
                Guid.NewGuid(),
                LanguageCode.From(NormalizeText(translation.Language).ToLowerInvariant()),
                NormalizeText(translation.Text),
                timestampUtc);
        }
    }

    private static string? SerializeLocalizedTextDictionary(IReadOnlyDictionary<string, string> values)
    {
        Dictionary<string, string> normalized = values
            .Select(pair => new KeyValuePair<string, string>(
                NormalizeText(pair.Key).ToLowerInvariant(),
                NormalizeText(pair.Value)))
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
            .GroupBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().Value, StringComparer.Ordinal);

        return normalized.Count == 0 ? null : JsonSerializer.Serialize(normalized);
    }

    private static string? SerializeLocalizedBlocks(IReadOnlyDictionary<string, string> blocksByLanguage)
    {
        if (blocksByLanguage.Count == 0)
        {
            return null;
        }

        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStartObject();
            foreach ((string language, string rawBlocks) in blocksByLanguage.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                string normalizedLanguage = NormalizeText(language).ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(normalizedLanguage) || string.IsNullOrWhiteSpace(rawBlocks))
                {
                    continue;
                }

                writer.WritePropertyName(normalizedLanguage);
                using JsonDocument document = JsonDocument.Parse(rawBlocks);
                document.RootElement.WriteTo(writer);
            }

            writer.WriteEndObject();
        }

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void ProcessExpressionEntries(
        IReadOnlyList<ParsedExpressionEntryModel> expressions,
        IReadOnlyDictionary<string, Topic> topicsByKey,
        ICollection<ExpressionEntry> importedExpressions)
    {
        DateTime timestampUtc = DateTime.UtcNow;

        foreach (ParsedExpressionEntryModel parsedExpression in expressions)
        {
            ExpressionEntry expression = new(
                Guid.NewGuid(),
                NormalizeText(parsedExpression.Slug),
                NormalizeText(parsedExpression.ExpressionText),
                NormalizeOptionalText(parsedExpression.LiteralMeaningText),
                NormalizeText(parsedExpression.ActualMeaningText),
                NormalizeOptionalText(parsedExpression.UsageExplanation),
                Enum.Parse<CefrLevel>(NormalizeText(parsedExpression.CefrLevel), true),
                NormalizeText(parsedExpression.ExpressionType).ToLowerInvariant(),
                NormalizeText(parsedExpression.Register).ToLowerInvariant(),
                NormalizeText(parsedExpression.Category).ToLowerInvariant(),
                NormalizeOptionalText(parsedExpression.Region)?.ToLowerInvariant(),
                parsedExpression.IsRisky,
                parsedExpression.IsPublished ? PublicationStatus.Active : PublicationStatus.Draft,
                parsedExpression.SortOrder < 0 ? 0 : parsedExpression.SortOrder,
                timestampUtc);

            string[] topicKeys = parsedExpression.Topics
                .Select(item => NormalizeText(item).ToLowerInvariant())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int topicIndex = 0; topicIndex < topicKeys.Length; topicIndex++)
            {
                expression.AddTopic(Guid.NewGuid(), topicsByKey[topicKeys[topicIndex]].Id, topicIndex == 0, timestampUtc);
            }

            foreach (ParsedExpressionMeaningModel parsedMeaning in parsedExpression.Meanings)
            {
                expression.AddMeaning(
                    Guid.NewGuid(),
                    LanguageCode.From(NormalizeText(parsedMeaning.Language).ToLowerInvariant()),
                    NormalizeText(parsedMeaning.ActualMeaningText),
                    NormalizeOptionalText(parsedMeaning.LiteralMeaningText),
                    NormalizeOptionalText(parsedMeaning.UsageExplanation),
                    timestampUtc);
            }

            for (int exampleIndex = 0; exampleIndex < parsedExpression.Examples.Count; exampleIndex++)
            {
                ParsedExpressionExampleModel parsedExample = parsedExpression.Examples[exampleIndex];
                ExpressionExample example = expression.AddExample(
                    Guid.NewGuid(),
                    parsedExample.SortOrder <= 0 ? (exampleIndex + 1) * 10 : parsedExample.SortOrder,
                    NormalizeText(parsedExample.GermanText),
                    NormalizeOptionalText(parsedExample.Note),
                    timestampUtc);

                AddExpressionTranslations(example.AddTranslation, parsedExample.Translations, timestampUtc);
            }

            foreach (ParsedExpressionWarningModel parsedWarning in parsedExpression.Warnings)
            {
                ExpressionWarning warning = expression.AddWarning(
                    Guid.NewGuid(),
                    NormalizeText(parsedWarning.WarningType).ToLowerInvariant(),
                    NormalizeText(parsedWarning.Text),
                    timestampUtc);

                AddExpressionTranslations(warning.AddTranslation, parsedWarning.Translations, timestampUtc);
            }

            for (int wordIndex = 0; wordIndex < parsedExpression.LinkedWords.Count; wordIndex++)
            {
                ParsedExpressionLinkedWordModel word = parsedExpression.LinkedWords[wordIndex];
                expression.AddLinkedWord(
                    Guid.NewGuid(),
                    NormalizeText(word.Lemma),
                    NormalizeOptionalText(word.WordSlug)?.ToLowerInvariant(),
                    word.SortOrder <= 0 ? (wordIndex + 1) * 10 : word.SortOrder,
                    timestampUtc);
            }

            AddExpressionSlugLinks(parsedExpression.RelatedExpressionSlugs, expression.AddRelatedExpression, timestampUtc);
            AddExpressionSlugLinks(parsedExpression.LinkedExerciseSlugs, expression.AddLinkedExercise, timestampUtc);

            importedExpressions.Add(expression);
        }
    }

    private static void AddExpressionSlugLinks(
        IReadOnlyList<string> slugs,
        Action<Guid, string, int, DateTime> addLink,
        DateTime timestampUtc)
    {
        for (int index = 0; index < slugs.Count; index++)
        {
            addLink(Guid.NewGuid(), NormalizeText(slugs[index]).ToLowerInvariant(), (index + 1) * 10, timestampUtc);
        }
    }

    private static void AddExpressionTranslations(
        Action<Guid, LanguageCode, string, DateTime> addTranslation,
        IReadOnlyList<ParsedContentMeaningModel> translations,
        DateTime timestampUtc)
    {
        foreach (ParsedContentMeaningModel translation in translations)
        {
            addTranslation(
                Guid.NewGuid(),
                LanguageCode.From(NormalizeText(translation.Language).ToLowerInvariant()),
                NormalizeText(translation.Text),
                timestampUtc);
        }
    }

    private static void ProcessEventPreparationPacks(
        IReadOnlyList<ParsedEventPreparationPackModel> eventPreparationPacks,
        IReadOnlyDictionary<string, Topic> topicsByKey,
        ICollection<EventPreparationPack> importedEventPreparationPacks)
    {
        DateTime timestampUtc = DateTime.UtcNow;

        foreach (ParsedEventPreparationPackModel parsedPack in eventPreparationPacks)
        {
            EventPreparationPack pack = new(
                Guid.NewGuid(),
                NormalizeText(parsedPack.Slug),
                NormalizeText(parsedPack.Title),
                NormalizeText(parsedPack.Description),
                Enum.Parse<CefrLevel>(NormalizeText(parsedPack.CefrLevel), true),
                NormalizeText(parsedPack.Category),
                NormalizeText(parsedPack.EventType),
                PublicationStatus.Active,
                parsedPack.SortOrder < 0 ? 0 : parsedPack.SortOrder,
                timestampUtc);

            string[] topicKeys = parsedPack.Topics
                .Select(topic => NormalizeText(topic).ToLowerInvariant())
                .Where(topic => !string.IsNullOrWhiteSpace(topic))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int topicIndex = 0; topicIndex < topicKeys.Length; topicIndex++)
            {
                pack.AddTopic(Guid.NewGuid(), topicsByKey[topicKeys[topicIndex]].Id, topicIndex == 0, timestampUtc);
            }

            string[] dialogueSlugs = parsedPack.LinkedDialogueSlugs
                .Select(slug => NormalizeText(slug).ToLowerInvariant())
                .Where(slug => !string.IsNullOrWhiteSpace(slug))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int dialogueIndex = 0; dialogueIndex < dialogueSlugs.Length; dialogueIndex++)
            {
                pack.AddLinkedDialogue(Guid.NewGuid(), dialogueSlugs[dialogueIndex], dialogueIndex + 1, timestampUtc);
            }

            string[] starterPackSlugs = parsedPack.LinkedConversationStarterPackSlugs
                .Select(slug => NormalizeText(slug).ToLowerInvariant())
                .Where(slug => !string.IsNullOrWhiteSpace(slug))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int starterPackIndex = 0; starterPackIndex < starterPackSlugs.Length; starterPackIndex++)
            {
                pack.AddLinkedConversationStarterPack(Guid.NewGuid(), starterPackSlugs[starterPackIndex], starterPackIndex + 1, timestampUtc);
            }

            for (int vocabularyIndex = 0; vocabularyIndex < parsedPack.LinkedVocabulary.Count; vocabularyIndex++)
            {
                ParsedEventPreparationVocabularyReferenceModel reference = parsedPack.LinkedVocabulary[vocabularyIndex];
                PartOfSpeech? partOfSpeech = NormalizeOptionalText(reference.PartOfSpeech) is { } normalizedPartOfSpeech
                    ? Enum.Parse<PartOfSpeech>(normalizedPartOfSpeech, true)
                    : null;
                CefrLevel? cefrLevel = NormalizeOptionalText(reference.CefrLevel) is { } normalizedCefrLevel
                    ? Enum.Parse<CefrLevel>(normalizedCefrLevel, true)
                    : null;

                pack.AddLinkedVocabulary(
                    Guid.NewGuid(),
                    NormalizeText(reference.Word),
                    partOfSpeech,
                    cefrLevel,
                    vocabularyIndex + 1,
                    timestampUtc);
            }

            AddEventPreparationPrompts(pack, "opening", parsedPack.OpeningPrompts, timestampUtc);
            AddEventPreparationPrompts(pack, "roleplay", parsedPack.RoleplayPrompts, timestampUtc);
            AddEventPreparationPrompts(pack, "review", parsedPack.ReviewPrompts, timestampUtc);

            importedEventPreparationPacks.Add(pack);
        }
    }

    private static void AddEventPreparationPrompts(
        EventPreparationPack pack,
        string promptType,
        IReadOnlyList<string> prompts,
        DateTime timestampUtc)
    {
        for (int promptIndex = 0; promptIndex < prompts.Count; promptIndex++)
        {
            pack.AddPrompt(
                Guid.NewGuid(),
                promptType,
                promptIndex + 1,
                NormalizeText(prompts[promptIndex]),
                timestampUtc);
        }
    }

    private static void ProcessExercises(IReadOnlyList<ParsedExerciseModel> exercises, ICollection<Exercise> importedExercises)
    {
        DateTime timestampUtc = DateTime.UtcNow;
        foreach (ParsedExerciseModel parsedExercise in exercises)
        {
            importedExercises.Add(new Exercise(
                Guid.NewGuid(),
                NormalizeText(parsedExercise.Slug),
                NormalizeText(parsedExercise.Title),
                NormalizeText(parsedExercise.Instruction),
                Enum.Parse<CefrLevel>(NormalizeText(parsedExercise.CefrLevel), true),
                NormalizeText(parsedExercise.ExerciseType).ToLowerInvariant(),
                NormalizeText(parsedExercise.TargetSkill).ToLowerInvariant(),
                NormalizeText(parsedExercise.OwnerType).ToLowerInvariant(),
                NormalizeOptionalText(parsedExercise.OwnerSlug)?.ToLowerInvariant(),
                NormalizeText(parsedExercise.PromptJson),
                NormalizeText(parsedExercise.AnswerKeyJson),
                NormalizeText(parsedExercise.CorrectExplanation),
                NormalizeText(parsedExercise.IncorrectExplanation),
                NormalizeOptionalText(parsedExercise.Hint),
                NormalizeOptionalText(parsedExercise.CommonMistakeNote),
                parsedExercise.IsPublished ? PublicationStatus.Active : PublicationStatus.Draft,
                parsedExercise.SortOrder,
                timestampUtc));
        }
    }

    private static void ProcessExerciseSets(IReadOnlyList<ParsedExerciseSetModel> exerciseSets, ICollection<ExerciseSet> importedExerciseSets)
    {
        DateTime timestampUtc = DateTime.UtcNow;
        foreach (ParsedExerciseSetModel parsedSet in exerciseSets)
        {
            ExerciseSet set = new(
                Guid.NewGuid(),
                NormalizeText(parsedSet.Slug),
                NormalizeText(parsedSet.Title),
                NormalizeText(parsedSet.Description),
                Enum.Parse<CefrLevel>(NormalizeText(parsedSet.CefrLevel), true),
                NormalizeText(parsedSet.OwnerType).ToLowerInvariant(),
                NormalizeOptionalText(parsedSet.OwnerSlug)?.ToLowerInvariant(),
                parsedSet.IsPublished ? PublicationStatus.Active : PublicationStatus.Draft,
                parsedSet.SortOrder,
                timestampUtc);

            for (int index = 0; index < parsedSet.ExerciseSlugs.Count; index++)
            {
                set.AddExercise(Guid.NewGuid(), NormalizeText(parsedSet.ExerciseSlugs[index]), (index + 1) * 10, timestampUtc);
            }

            importedExerciseSets.Add(set);
        }
    }

    private static void ProcessCourses(
        IReadOnlyList<ParsedCoursePathModel> coursePaths,
        IReadOnlyList<ParsedCourseModuleModel> courseModules,
        IReadOnlyList<ParsedCourseLessonModel> courseLessons,
        ICollection<CoursePath> importedCoursePaths,
        ICollection<CourseModule> importedCourseModules,
        ICollection<CourseLesson> importedCourseLessons)
    {
        DateTime timestampUtc = DateTime.UtcNow;
        foreach (ParsedCoursePathModel parsedCourse in coursePaths)
        {
            CefrLevel? cefrLevel = Enum.TryParse(NormalizeText(parsedCourse.CefrLevel), true, out CefrLevel parsedCefr)
                ? parsedCefr
                : null;
            importedCoursePaths.Add(new CoursePath(
                Guid.NewGuid(),
                NormalizeText(parsedCourse.Slug),
                NormalizeText(parsedCourse.Title),
                NormalizeText(parsedCourse.Description),
                cefrLevel,
                NormalizeOptionalText(parsedCourse.CefrRange),
                parsedCourse.IsPublished ? PublicationStatus.Active : PublicationStatus.Draft,
                parsedCourse.SortOrder,
                timestampUtc));
        }

        foreach (ParsedCourseModuleModel parsedModule in courseModules)
        {
            importedCourseModules.Add(new CourseModule(
                Guid.NewGuid(),
                NormalizeText(parsedModule.CoursePathSlug),
                NormalizeText(parsedModule.Slug),
                NormalizeText(parsedModule.Title),
                NormalizeText(parsedModule.Description),
                parsedModule.ModuleNumber,
                Enum.Parse<CefrLevel>(NormalizeText(parsedModule.CefrLevel), true),
                parsedModule.IsPublished ? PublicationStatus.Active : PublicationStatus.Draft,
                parsedModule.SortOrder,
                timestampUtc));
        }

        foreach (ParsedCourseLessonModel parsedLesson in courseLessons)
        {
            importedCourseLessons.Add(new CourseLesson(
                Guid.NewGuid(),
                NormalizeText(parsedLesson.CoursePathSlug),
                NormalizeText(parsedLesson.ModuleSlug),
                NormalizeText(parsedLesson.Slug),
                parsedLesson.LessonNumber,
                NormalizeText(parsedLesson.Title),
                NormalizeText(parsedLesson.ShortDescription),
                NormalizeText(parsedLesson.Narrative),
                Enum.Parse<CefrLevel>(NormalizeText(parsedLesson.CefrLevel), true),
                parsedLesson.EstimatedMinutes,
                SerializeStringArray(parsedLesson.LearningGoals),
                SerializeSlugArray(parsedLesson.PrerequisiteLessonSlugs),
                NormalizeOptionalText(parsedLesson.NextLessonSlug)?.ToLowerInvariant(),
                SerializeSlugArray(parsedLesson.LinkedGrammarTopicSlugs),
                SerializeSlugArray(parsedLesson.LinkedWordSlugs),
                SerializeSlugArray(parsedLesson.LinkedExpressionSlugs),
                SerializeSlugArray(parsedLesson.LinkedDialogueSlugs),
                SerializeSlugArray(parsedLesson.LinkedTalkTopicSlugs),
                SerializeSlugArray(parsedLesson.LinkedExerciseSetSlugs),
                SerializeSlugArray(parsedLesson.LinkedExamPrepSlugs),
                NormalizeOptionalText(parsedLesson.ReviewSummary),
                NormalizeOptionalText(parsedLesson.HomeworkTask),
                parsedLesson.IsPublished ? PublicationStatus.Active : PublicationStatus.Draft,
                parsedLesson.SortOrder,
                timestampUtc));
        }
    }

    private static void ProcessWritingTemplates(
        IReadOnlyList<ParsedWritingTemplateModel> writingTemplates,
        ICollection<WritingTemplate> importedWritingTemplates)
    {
        DateTime timestampUtc = DateTime.UtcNow;
        foreach (ParsedWritingTemplateModel parsedTemplate in writingTemplates)
        {
            importedWritingTemplates.Add(new WritingTemplate(
                Guid.NewGuid(),
                NormalizeText(parsedTemplate.Slug),
                NormalizeText(parsedTemplate.Title),
                NormalizeText(parsedTemplate.ShortDescription),
                Enum.Parse<CefrLevel>(NormalizeText(parsedTemplate.CefrLevel), true),
                NormalizeText(parsedTemplate.Category).ToLowerInvariant(),
                NormalizeText(parsedTemplate.Situation),
                NormalizeText(parsedTemplate.Register).ToLowerInvariant(),
                NormalizeText(parsedTemplate.TemplateText),
                NormalizeText(parsedTemplate.Explanation),
                SerializeStringArray(parsedTemplate.ReplaceableVariables),
                NormalizeText(parsedTemplate.SampleFilledVersion),
                SerializeSlugArray(parsedTemplate.LinkedGrammarTopicSlugs),
                SerializeSlugArray(parsedTemplate.LinkedWordSlugs),
                SerializeSlugArray(parsedTemplate.LinkedExpressionSlugs),
                SerializeSlugArray(parsedTemplate.LinkedExerciseSlugs),
                parsedTemplate.IsPublished ? PublicationStatus.Active : PublicationStatus.Draft,
                parsedTemplate.SortOrder,
                timestampUtc));
        }
    }

    private static void ProcessCulturalNotes(
        IReadOnlyList<ParsedCulturalNoteModel> culturalNotes,
        ICollection<CulturalNote> importedCulturalNotes)
    {
        DateTime timestampUtc = DateTime.UtcNow;
        foreach (ParsedCulturalNoteModel parsedNote in culturalNotes)
        {
            importedCulturalNotes.Add(new CulturalNote(
                Guid.NewGuid(),
                NormalizeText(parsedNote.Slug),
                NormalizeText(parsedNote.Title),
                NormalizeText(parsedNote.ShortDescription),
                Enum.Parse<CefrLevel>(NormalizeText(parsedNote.CefrLevel), true),
                NormalizeText(parsedNote.Category).ToLowerInvariant(),
                NormalizeText(parsedNote.Context),
                SerializeStringArray(parsedNote.Sections),
                SerializeCulturalNoteExamples(parsedNote.Examples),
                SerializeStringArray(parsedNote.DoNotes),
                SerializeStringArray(parsedNote.DontNotes),
                NormalizeOptionalText(parsedNote.SensitivityWarning),
                SerializeSlugArray(parsedNote.LinkedDialogueSlugs),
                SerializeSlugArray(parsedNote.LinkedExpressionSlugs),
                SerializeSlugArray(parsedNote.LinkedWritingTemplateSlugs),
                SerializeSlugArray(parsedNote.LinkedTalkTopicSlugs),
                SerializeSlugArray(parsedNote.LinkedCourseLessonSlugs),
                parsedNote.IsPublished ? PublicationStatus.Active : PublicationStatus.Draft,
                parsedNote.SortOrder,
                timestampUtc));
        }
    }

    private static void ProcessExamPrep(
        IReadOnlyList<ParsedExamProfileModel> examProfiles,
        IReadOnlyList<ParsedExamPrepUnitModel> examPrepUnits,
        ICollection<ExamProfile> importedExamProfiles,
        ICollection<ExamPrepUnit> importedExamPrepUnits)
    {
        DateTime timestampUtc = DateTime.UtcNow;
        foreach (ParsedExamProfileModel parsedProfile in examProfiles)
        {
            importedExamProfiles.Add(new ExamProfile(
                Guid.NewGuid(),
                NormalizeText(parsedProfile.Key),
                NormalizeText(parsedProfile.DisplayName),
                NormalizeText(parsedProfile.CefrRange),
                NormalizeText(parsedProfile.Description),
                parsedProfile.IsPublished ? PublicationStatus.Active : PublicationStatus.Draft,
                parsedProfile.SortOrder,
                timestampUtc));
        }

        foreach (ParsedExamPrepUnitModel parsedUnit in examPrepUnits)
        {
            importedExamPrepUnits.Add(new ExamPrepUnit(
                Guid.NewGuid(),
                NormalizeText(parsedUnit.Slug),
                NormalizeText(parsedUnit.ExamProfileKey),
                NormalizeText(parsedUnit.Title),
                NormalizeText(parsedUnit.ShortDescription),
                Enum.Parse<CefrLevel>(NormalizeText(parsedUnit.CefrLevel), true),
                NormalizeText(parsedUnit.ExamSection).ToLowerInvariant(),
                NormalizeText(parsedUnit.TaskType).ToLowerInvariant(),
                NormalizeText(parsedUnit.SkillFocus).ToLowerInvariant(),
                NormalizeText(parsedUnit.Explanation),
                SerializeStringArray(parsedUnit.StrategyNotes),
                SerializeStringArray(parsedUnit.Checklist),
                SerializeSlugArray(parsedUnit.LinkedDialogueSlugs),
                SerializeSlugArray(parsedUnit.LinkedTalkTopicSlugs),
                SerializeSlugArray(parsedUnit.LinkedGrammarTopicSlugs),
                SerializeSlugArray(parsedUnit.LinkedExpressionSlugs),
                SerializeSlugArray(parsedUnit.LinkedWritingTemplateSlugs),
                SerializeSlugArray(parsedUnit.LinkedExerciseSlugs),
                SerializeSlugArray(parsedUnit.LinkedCourseLessonSlugs),
                parsedUnit.IsPublished ? PublicationStatus.Active : PublicationStatus.Draft,
                parsedUnit.SortOrder,
                timestampUtc));
        }
    }

    private static void AddDialogueTranslations(
        Action<Guid, LanguageCode, string, DateTime> addTranslation,
        IReadOnlyList<ParsedContentMeaningModel> translations,
        DateTime timestampUtc)
    {
        foreach (ParsedContentMeaningModel translation in translations)
        {
            addTranslation(
                Guid.NewGuid(),
                LanguageCode.From(NormalizeText(translation.Language).ToLowerInvariant()),
                NormalizeText(translation.Text),
                timestampUtc);
        }
    }

    private static void ProcessLabelDefinitions(
        IReadOnlyList<ParsedContentLabelDefinitionModel> labels,
        ICollection<LabelDefinition> importedLabels)
    {
        DateTime timestampUtc = DateTime.UtcNow;

        foreach (ParsedContentLabelDefinitionModel label in labels)
        {
            WordLabelKind kind = Enum.Parse<WordLabelKind>(NormalizeText(label.Kind), true);
            string key = NormalizeText(label.Key).ToLowerInvariant();
            string displayName = NormalizeText(label.DisplayName);

            LabelDefinition importedLabel = new(
                Guid.NewGuid(),
                kind,
                key,
                displayName,
                label.SortOrder < 0 ? 0 : label.SortOrder,
                true,
                timestampUtc);

            foreach (ParsedLocalizedTextModel localization in label.Localizations)
            {
                importedLabel.AddOrUpdateLocalization(
                    Guid.NewGuid(),
                    LanguageCode.From(NormalizeText(localization.Language).ToLowerInvariant()),
                    NormalizeText(localization.Name),
                    timestampUtc);
            }

            importedLabels.Add(importedLabel);
        }
    }

    private async Task ProcessCollectionsAsync(
        IReadOnlyList<ParsedContentCollectionModel> collections,
        IReadOnlyList<WordEntry> importedWords,
        ICollection<WordCollection> importedCollections,
        ICollection<ImportIssueModel> issues,
        CancellationToken cancellationToken)
    {
        if (collections.Count == 0)
        {
            return;
        }

        string[] normalizedLemmas = collections
            .SelectMany(collection => collection.Words)
            .Select(reference => NormalizeText(reference.Word).ToLowerInvariant())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        IReadOnlyList<WordEntry> existingWords = await _contentImportRepository
            .GetActiveWordsByNormalizedLemmasAsync(normalizedLemmas, cancellationToken)
            .ConfigureAwait(false);

        List<WordEntry> resolvableWords = importedWords
            .Concat(existingWords)
            .GroupBy(word => word.Id)
            .Select(group => group.First())
            .ToList();

        DateTime timestampUtc = DateTime.UtcNow;

        for (int index = 0; index < collections.Count; index++)
        {
            ParsedContentCollectionModel collection = collections[index];
            List<string> errors = [];

            string normalizedSlug = NormalizeCollectionSlug(collection.Slug, errors);
            string normalizedName = NormalizeText(collection.Name);
            string? normalizedDescription = NormalizeOptionalText(collection.Description);
            string? normalizedImageUrl = NormalizeOptionalText(collection.ImageUrl);
            int sortOrder = collection.SortOrder < 0 ? 0 : collection.SortOrder;
            ParsedLocalizedTextModel[] localizations = ValidateLocalizedCollectionText(
                collection.Localizations,
                "Collection localizations",
                errors);

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                errors.Add("Collection name is required.");
            }

            if (collection.Words.Count == 0)
            {
                errors.Add("Collection words must contain at least one item.");
            }

            if (errors.Count > 0)
            {
                issues.Add(new ImportIssueModel(null, "Error", $"Collection {index + 1}: {string.Join(" ", errors)}"));
                continue;
            }

            List<(Guid WordEntryId, int SortOrder)> collectionEntries = [];
            HashSet<(string Word, string? PartOfSpeech, string? CefrLevel)> uniqueness = [];

            for (int wordIndex = 0; wordIndex < collection.Words.Count; wordIndex++)
            {
                ParsedContentCollectionWordReferenceModel wordReference = collection.Words[wordIndex];

                string normalizedWord = NormalizeText(wordReference.Word).ToLowerInvariant();
                string? normalizedPartOfSpeech = NormalizeOptionalText(wordReference.PartOfSpeech);
                string? normalizedCefrLevel = NormalizeOptionalText(wordReference.CefrLevel)?.ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(normalizedWord))
                {
                    errors.Add($"Collection word {wordIndex + 1} is missing a word value.");
                    continue;
                }

                if (normalizedPartOfSpeech is not null &&
                    !Enum.TryParse(normalizedPartOfSpeech, true, out PartOfSpeech partOfSpeech))
                {
                    errors.Add($"Collection word '{wordReference.Word}' has an invalid partOfSpeech value.");
                    continue;
                }

                PartOfSpeech? requestedPartOfSpeech = normalizedPartOfSpeech is null
                    ? null
                    : Enum.Parse<PartOfSpeech>(normalizedPartOfSpeech, true);

                CefrLevel? requestedCefrLevel = null;
                if (normalizedCefrLevel is not null)
                {
                    if (!Enum.TryParse(normalizedCefrLevel, true, out CefrLevel cefrLevel))
                    {
                        errors.Add($"Collection word '{wordReference.Word}' has an invalid cefrLevel value.");
                        continue;
                    }

                    requestedCefrLevel = cefrLevel;
                }

                if (!uniqueness.Add((normalizedWord, requestedPartOfSpeech?.ToString(), requestedCefrLevel?.ToString())))
                {
                    errors.Add($"Collection '{normalizedSlug}' contains a duplicate word reference for '{wordReference.Word}'.");
                    continue;
                }

                List<WordEntry> matches = resolvableWords
                    .Where(word => string.Equals(word.NormalizedLemma, normalizedWord, StringComparison.Ordinal))
                    .Where(word => !requestedPartOfSpeech.HasValue || word.PartOfSpeech == requestedPartOfSpeech.Value)
                    .Where(word => !requestedCefrLevel.HasValue || word.PrimaryCefrLevel == requestedCefrLevel.Value)
                    .OrderBy(word => word.PrimaryCefrLevel)
                    .ThenBy(word => word.PartOfSpeech)
                    .ToList();

                if (matches.Count == 0)
                {
                    errors.Add($"Collection '{normalizedSlug}' references '{wordReference.Word}', but no matching active word exists.");
                    continue;
                }

                if (matches.Count > 1)
                {
                    errors.Add($"Collection '{normalizedSlug}' references '{wordReference.Word}' ambiguously. Add partOfSpeech or cefrLevel.");
                    continue;
                }

                collectionEntries.Add((matches[0].Id, wordIndex + 1));
            }

            if (errors.Count > 0)
            {
                issues.Add(new ImportIssueModel(null, "Error", $"Collection '{normalizedSlug}': {string.Join(" ", errors)}"));
                continue;
            }

            try
            {
                WordCollection importedCollection = new(
                    Guid.NewGuid(),
                    normalizedSlug,
                    normalizedName,
                    normalizedDescription,
                    normalizedImageUrl,
                    PublicationStatus.Active,
                    sortOrder,
                    timestampUtc);

                importedCollection.ReplaceEntries(collectionEntries, timestampUtc);
                foreach (ParsedLocalizedTextModel localization in localizations)
                {
                    importedCollection.AddOrUpdateLocalization(
                        Guid.NewGuid(),
                        LanguageCode.From(NormalizeText(localization.Language).ToLowerInvariant()),
                        NormalizeText(localization.Name),
                        NormalizeOptionalText(localization.Description),
                        timestampUtc);
                }

                importedCollections.Add(importedCollection);
            }
            catch (DomainRuleException exception)
            {
                issues.Add(new ImportIssueModel(null, "Error", $"Collection '{normalizedSlug}': {exception.Message}"));
            }
        }
    }

    private async Task ProcessEntryAsync(
        int entryIndex,
        ParsedContentEntryModel entry,
        IReadOnlyDictionary<string, Topic> topicsByKey,
        IReadOnlySet<LanguageCode> meaningLanguages,
        IReadOnlyList<LanguageCode> expectedMeaningLanguages,
        IReadOnlyDictionary<WordLabelKind, HashSet<string>> allowedLabelsByKind,
        ContentPackage contentPackage,
        ICollection<WordEntry> importedWords,
        ICollection<ImportIssueModel> issues,
        CancellationToken cancellationToken)
    {
        List<string> entryErrors = [];

        string rawLemma = entry.Word ?? string.Empty;
        string normalizedLemma = NormalizeText(entry.Word).ToLowerInvariant();
        string normalizedLanguage = NormalizeText(entry.Language).ToLowerInvariant();
        string normalizedCefrLevelText = NormalizeText(entry.CefrLevel).ToUpperInvariant();
        string[] normalizedTopicKeys = entry.Topics
            .Where(topic => !string.IsNullOrWhiteSpace(topic))
            .Select(topic => NormalizeText(topic).ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        string[] usageLabels = ValidateLabelKeys(entry.UsageLabels, WordLabelKind.Usage, "usageLabels", allowedLabelsByKind, entryErrors);
        string[] contextLabels = ValidateLabelKeys(entry.ContextLabels, WordLabelKind.Context, "contextLabels", allowedLabelsByKind, entryErrors);
        string[] grammarNotes = ValidateGrammarNotes(entry.GrammarNotes, entryErrors);
        ParsedContentCollocationModel[] collocations = ValidateCollocations(entry.Collocations, entryErrors);
        ParsedContentWordFamilyMemberModel[] wordFamilies = ValidateWordFamilies(entry.WordFamilies, entryErrors);
        ParsedContentWordRelationModel[] relations = ValidateRelations(entry.Relations, entryErrors);

        if (string.IsNullOrWhiteSpace(normalizedLemma))
        {
            entryErrors.Add("Entry word is required.");
        }

        if (!string.Equals(normalizedLanguage, "de", StringComparison.Ordinal))
        {
            entryErrors.Add("Entry language must be 'de' in Phase 1.");
        }

        if (!Enum.TryParse(normalizedCefrLevelText, true, out CefrLevel cefrLevel))
        {
            entryErrors.Add("Entry CEFR level is invalid.");
        }

        if (normalizedTopicKeys.Length == 0)
        {
            entryErrors.Add("Entry topics must contain at least one topic key.");
        }

        if (entry.Meanings.Count == 0)
        {
            entryErrors.Add("Entry meanings must contain at least one item.");
        }

        if (entry.Examples.Count == 0)
        {
            entryErrors.Add("Entry examples must contain at least one item.");
        }

        Dictionary<LanguageCode, string> meaningTranslations = ValidateMeaningTranslations(entry.Meanings, meaningLanguages, entryErrors);
        List<(string GermanText, Dictionary<LanguageCode, string> Translations)> examples = ValidateExamples(entry.Examples, meaningLanguages, entryErrors);
        NormalizedLexicalForm[] lexicalForms = ValidateLexicalForms(entry, entryErrors);
        NormalizedLexicalForm? primaryLexicalForm = lexicalForms
            .OrderByDescending(form => form.IsPrimary)
            .ThenBy(form => form.SortOrder)
            .FirstOrDefault();
        string normalizedPartOfSpeechText = primaryLexicalForm?.PartOfSpeech.ToString() ?? string.Empty;

        foreach (string topicKey in normalizedTopicKeys)
        {
            if (!topicsByKey.ContainsKey(topicKey))
            {
                entryErrors.Add($"Unknown topic key '{topicKey}'.");
            }
        }

        if (entryErrors.Count > 0)
        {
            string errorMessage = string.Join(" ", entryErrors);
            issues.Add(new ImportIssueModel(entryIndex + 1, "Error", errorMessage));
            contentPackage.AddEntry(
                Guid.NewGuid(),
                string.IsNullOrWhiteSpace(rawLemma) ? $"entry-{entryIndex + 1}" : rawLemma,
                string.IsNullOrWhiteSpace(normalizedLemma) ? $"entry-{entryIndex + 1}" : normalizedLemma,
                string.IsNullOrWhiteSpace(normalizedCefrLevelText) ? null : normalizedCefrLevelText,
                string.IsNullOrWhiteSpace(normalizedPartOfSpeechText) ? null : normalizedPartOfSpeechText,
                ContentPackageEntryStatus.Invalid,
                errorMessage,
                null,
                null,
                DateTime.UtcNow);
            return;
        }

        if (importedWords.Any(word => word.NormalizedLemma == normalizedLemma) ||
            await _contentImportRepository
                .WordExistsAsync(normalizedLemma, cancellationToken)
                .ConfigureAwait(false))
        {
            string warningMessage = $"Duplicate entry skipped for lemma '{rawLemma}'.";
            issues.Add(new ImportIssueModel(entryIndex + 1, "Warning", warningMessage));
            contentPackage.AddEntry(
                Guid.NewGuid(),
                rawLemma,
                normalizedLemma,
                normalizedCefrLevelText,
                normalizedPartOfSpeechText,
                ContentPackageEntryStatus.SkippedDuplicate,
                null,
                warningMessage,
                null,
                DateTime.UtcNow);
            return;
        }

        WordEntry wordEntry = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            NormalizeText(entry.Word),
            LanguageCode.From("de"),
            cefrLevel,
            primaryLexicalForm!.PartOfSpeech,
            PublicationStatus.Active,
            contentPackage.SourceType,
            DateTime.UtcNow,
            article: primaryLexicalForm.Article,
            pluralForm: primaryLexicalForm.PluralForm,
            infinitiveForm: primaryLexicalForm.InfinitiveForm,
            pronunciationIpa: NormalizeOptionalText(entry.PronunciationIpa),
            syllableBreak: NormalizeOptionalText(entry.SyllableBreak));

        foreach (NormalizedLexicalForm lexicalForm in lexicalForms.Where(form => !form.IsPrimary))
        {
            wordEntry.AddLexicalForm(
                Guid.NewGuid(),
                lexicalForm.PartOfSpeech,
                false,
                DateTime.UtcNow,
                lexicalForm.Article,
                lexicalForm.PluralForm,
                lexicalForm.InfinitiveForm);
        }

        WordSense sense = wordEntry.AddSense(
            Guid.NewGuid(),
            1,
            true,
            PublicationStatus.Active,
            DateTime.UtcNow);

        int translationIndex = 0;

        foreach ((LanguageCode languageCode, string translationText) in meaningTranslations)
        {
            translationIndex++;
            sense.AddTranslation(
                Guid.NewGuid(),
                languageCode,
                translationText,
                translationIndex == 1,
                DateTime.UtcNow);
        }

        for (int exampleIndex = 0; exampleIndex < examples.Count; exampleIndex++)
        {
            (string germanText, Dictionary<LanguageCode, string> translations) = examples[exampleIndex];

            ExampleSentence example = sense.AddExample(
                Guid.NewGuid(),
                exampleIndex + 1,
                germanText,
                exampleIndex == 0,
                DateTime.UtcNow);

            foreach ((LanguageCode languageCode, string translationText) in translations)
            {
                example.AddTranslation(Guid.NewGuid(), languageCode, translationText, DateTime.UtcNow);
            }
        }

        for (int topicIndex = 0; topicIndex < normalizedTopicKeys.Length; topicIndex++)
        {
            Topic topic = topicsByKey[normalizedTopicKeys[topicIndex]];
            wordEntry.AddTopic(Guid.NewGuid(), topic.Id, topicIndex == 0, DateTime.UtcNow);
        }

        foreach (string usageLabel in usageLabels)
        {
            wordEntry.AddLabel(Guid.NewGuid(), WordLabelKind.Usage, usageLabel, DateTime.UtcNow);
        }

        foreach (string contextLabel in contextLabels)
        {
            wordEntry.AddLabel(Guid.NewGuid(), WordLabelKind.Context, contextLabel, DateTime.UtcNow);
        }

        foreach (string grammarNote in grammarNotes)
        {
            wordEntry.AddGrammarNote(Guid.NewGuid(), grammarNote, DateTime.UtcNow);
        }

        foreach (ParsedContentCollocationModel collocation in collocations)
        {
            wordEntry.AddCollocation(Guid.NewGuid(), collocation.Text, collocation.Meaning, DateTime.UtcNow);
        }

        foreach (ParsedContentWordFamilyMemberModel familyMember in wordFamilies)
        {
            wordEntry.AddFamilyMember(Guid.NewGuid(), familyMember.Lemma, familyMember.RelationLabel, familyMember.Note, DateTime.UtcNow);
        }

        foreach (ParsedContentWordRelationModel relation in relations)
        {
            wordEntry.AddRelation(
                Guid.NewGuid(),
                ParseRelationKind(relation.Kind),
                relation.Lemma,
                relation.Note,
                DateTime.UtcNow);
        }

        importedWords.Add(wordEntry);

        contentPackage.AddEntry(
            Guid.NewGuid(),
            rawLemma,
            normalizedLemma,
            normalizedCefrLevelText,
            normalizedPartOfSpeechText,
            ContentPackageEntryStatus.Imported,
            null,
            null,
            wordEntry.PublicId,
            DateTime.UtcNow);
    }

    private static string[] BuildTranslationCoverageWarnings(
        string rawLemma,
        IReadOnlyDictionary<LanguageCode, string> meaningTranslations,
        IReadOnlyList<(string GermanText, Dictionary<LanguageCode, string> Translations)> examples,
        IReadOnlyList<LanguageCode> expectedMeaningLanguages)
    {
        if (expectedMeaningLanguages.Count < 2)
        {
            return [];
        }

        List<string> warnings = [];
        string lemma = NormalizeText(rawLemma);

        LanguageCode[] missingMeaningLanguages = expectedMeaningLanguages
            .Where(languageCode => !meaningTranslations.ContainsKey(languageCode))
            .ToArray();

        if (missingMeaningLanguages.Length > 0)
        {
            warnings.Add(
                $"Entry '{lemma}' is missing meaning translations for: {FormatLanguageList(missingMeaningLanguages)}.");
        }

        foreach ((string germanText, Dictionary<LanguageCode, string> translations) in examples)
        {
            LanguageCode[] missingExampleLanguages = expectedMeaningLanguages
                .Where(languageCode => !translations.ContainsKey(languageCode))
                .ToArray();

            if (missingExampleLanguages.Length > 0)
            {
                warnings.Add(
                    $"Entry '{lemma}' example '{germanText}' is missing translations for: {FormatLanguageList(missingExampleLanguages)}.");
            }
        }

        return warnings.ToArray();
    }

    private static string FormatLanguageList(IEnumerable<LanguageCode> languageCodes)
    {
        return string.Join(", ", languageCodes.Select(languageCode => languageCode.Value));
    }

    private static void ValidateDialogues(
        IReadOnlyList<ParsedDialogueLessonModel> dialogues,
        IReadOnlyDictionary<string, Topic> topicsByKey,
        IReadOnlySet<LanguageCode> meaningLanguages,
        ICollection<ImportIssueModel> issues)
    {
        HashSet<string> slugs = [];

        for (int index = 0; index < dialogues.Count; index++)
        {
            ParsedDialogueLessonModel dialogue = dialogues[index];
            List<string> errors = [];
            string slug = NormalizeText(dialogue.Slug).ToLowerInvariant();

            if (!ValidateKebabKey(slug))
            {
                errors.Add("Dialogue slug is required and must use lowercase kebab-case.");
            }
            else if (!slugs.Add(slug))
            {
                errors.Add($"Duplicate dialogue slug '{slug}' is not allowed inside one package.");
            }

            if (string.IsNullOrWhiteSpace(NormalizeText(dialogue.Title)))
            {
                errors.Add("Dialogue title is required.");
            }

            if (string.IsNullOrWhiteSpace(NormalizeText(dialogue.Description)))
            {
                errors.Add("Dialogue description is required.");
            }

            if (string.IsNullOrWhiteSpace(NormalizeText(dialogue.LearnerGoal)))
            {
                errors.Add("Dialogue learnerGoal is required.");
            }

            if (!Enum.TryParse(NormalizeText(dialogue.CefrLevel), true, out CefrLevel parsedDialogueCefrLevel))
            {
                errors.Add("Dialogue CEFR level is invalid.");
            }

            string category = NormalizeText(dialogue.Category).ToLowerInvariant();
            if (!ValidateKebabKey(category))
            {
                errors.Add("Dialogue category is required and must use lowercase kebab-case.");
            }

            string[] topicKeys = dialogue.Topics
                .Select(topic => NormalizeText(topic).ToLowerInvariant())
                .Where(topic => !string.IsNullOrWhiteSpace(topic))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (topicKeys.Length == 0)
            {
                errors.Add("Dialogue topics must contain at least one topic key.");
            }

            foreach (string topicKey in topicKeys)
            {
                if (!topicsByKey.ContainsKey(topicKey))
                {
                    errors.Add($"Dialogue references unknown topic key '{topicKey}'.");
                }
            }

            ValidateDialogueStringList(dialogue.ExamProfiles, DialogueExamProfiles, "examProfiles", true, errors);
            ValidateDialogueStringList(dialogue.SkillFocus, DialogueSkillFocus, "skillFocus", true, errors);
            ValidateDialogueStringList(dialogue.SpeakingFunctions, DialogueSpeakingFunctions, "speakingFunctions", true, errors);

            string taskType = NormalizeText(dialogue.TaskType).ToLowerInvariant();
            if (!DialogueTaskTypes.Contains(taskType))
            {
                errors.Add("Dialogue taskType is required and must be one of the supported dialogue task types.");
            }

            string interactionMode = NormalizeText(dialogue.InteractionMode).ToLowerInvariant();
            if (!DialogueInteractionModes.Contains(interactionMode))
            {
                errors.Add("Dialogue interactionMode is required and must be one of the supported dialogue interaction modes.");
            }

            string register = NormalizeText(dialogue.Register).ToLowerInvariant();
            if (!DialogueRegisters.Contains(register))
            {
                errors.Add("Dialogue register is required and must be formal, informal, neutral, or mixed.");
            }

            if (dialogue.EstimatedPracticeMinutes <= 0)
            {
                errors.Add("Dialogue estimatedPracticeMinutes must be greater than zero.");
            }

            if (dialogue.UsefulWords.Count == 0)
            {
                errors.Add("Dialogue usefulWords must contain at least one item.");
            }

            for (int wordIndex = 0; wordIndex < dialogue.UsefulWords.Count; wordIndex++)
            {
                ParsedDialogueUsefulWordModel word = dialogue.UsefulWords[wordIndex];
                if (string.IsNullOrWhiteSpace(NormalizeText(word.Lemma)))
                {
                    errors.Add($"Dialogue usefulWords[{wordIndex + 1}] lemma is required.");
                }

                string? wordSlug = NormalizeOptionalText(word.WordSlug);
                if (wordSlug is not null && !ValidateKebabKey(wordSlug.ToLowerInvariant()))
                {
                    errors.Add($"Dialogue usefulWords[{wordIndex + 1}] wordSlug must use lowercase kebab-case.");
                }

                string? wordCefrLevel = NormalizeOptionalText(word.CefrLevel);
                if (wordCefrLevel is not null && !Enum.TryParse(wordCefrLevel, true, out CefrLevel _))
                {
                    errors.Add($"Dialogue usefulWords[{wordIndex + 1}] cefrLevel is invalid.");
                }
            }

            if (dialogue.SpeakingPrompts.Count == 0)
            {
                errors.Add("Dialogue speakingPrompts must contain at least one item.");
            }

            for (int promptIndex = 0; promptIndex < dialogue.SpeakingPrompts.Count; promptIndex++)
            {
                ParsedDialogueSpeakingPromptModel prompt = dialogue.SpeakingPrompts[promptIndex];
                string promptType = NormalizeText(prompt.PromptType).ToLowerInvariant();
                if (!DialoguePromptTypes.Contains(promptType))
                {
                    errors.Add($"Dialogue speakingPrompts[{promptIndex + 1}] promptType is invalid.");
                }

                if (string.IsNullOrWhiteSpace(NormalizeText(prompt.Prompt)))
                {
                    errors.Add($"Dialogue speakingPrompts[{promptIndex + 1}] prompt is required.");
                }

                ValidateDialogueTranslations(
                    prompt.Translations,
                    meaningLanguages,
                    $"Dialogue speakingPrompts[{promptIndex + 1}] translations",
                    errors);
            }

            if (dialogue.DialogueTurns.Count == 0)
            {
                errors.Add("Dialogue dialogueTurns must contain at least one item.");
            }

            for (int turnIndex = 0; turnIndex < dialogue.DialogueTurns.Count; turnIndex++)
            {
                ParsedDialogueTurnModel turn = dialogue.DialogueTurns[turnIndex];
                string speakerRole = NormalizeText(turn.SpeakerRole).ToLowerInvariant();

                if (!ValidateKebabKey(speakerRole))
                {
                    errors.Add($"Dialogue dialogueTurns[{turnIndex + 1}] speakerRole is required and must use lowercase kebab-case.");
                }

                if (string.IsNullOrWhiteSpace(NormalizeText(turn.BaseText)))
                {
                    errors.Add($"Dialogue dialogueTurns[{turnIndex + 1}] baseText is required.");
                }

                ValidateDialogueTranslations(
                    turn.Translations,
                    meaningLanguages,
                    $"Dialogue dialogueTurns[{turnIndex + 1}] translations",
                    errors);
            }

            if (Enum.TryParse(NormalizeText(dialogue.CefrLevel), true, out parsedDialogueCefrLevel))
            {
                ValidateDialogueSentenceCounts(dialogue, parsedDialogueCefrLevel, errors);
            }

            if (dialogue.UsefulPhrases.Count == 0)
            {
                errors.Add("Dialogue usefulPhrases must contain at least one item.");
            }

            for (int phraseIndex = 0; phraseIndex < dialogue.UsefulPhrases.Count; phraseIndex++)
            {
                ParsedDialoguePhraseModel phrase = dialogue.UsefulPhrases[phraseIndex];

                if (string.IsNullOrWhiteSpace(NormalizeText(phrase.BaseText)))
                {
                    errors.Add($"Dialogue usefulPhrases[{phraseIndex + 1}] baseText is required.");
                }

                ValidateDialogueTranslations(
                    phrase.Translations,
                    meaningLanguages,
                    $"Dialogue usefulPhrases[{phraseIndex + 1}] translations",
                    errors);
            }

            if (dialogue.Questions.Count == 0)
            {
                errors.Add("Dialogue questions must contain at least one item.");
            }

            for (int questionIndex = 0; questionIndex < dialogue.Questions.Count; questionIndex++)
            {
                ParsedDialogueQuestionModel question = dialogue.Questions[questionIndex];

                if (string.IsNullOrWhiteSpace(NormalizeText(question.Prompt)))
                {
                    errors.Add($"Dialogue questions[{questionIndex + 1}] prompt is required.");
                }

                ValidateDialogueTranslations(
                    question.Translations,
                    meaningLanguages,
                    $"Dialogue questions[{questionIndex + 1}] translations",
                    errors);

                if (question.Answers.Count < 2)
                {
                    errors.Add($"Dialogue questions[{questionIndex + 1}] must contain at least two answers.");
                }

                int correctAnswerCount = question.Answers.Count(answer => answer.IsCorrect);
                if (correctAnswerCount != 1)
                {
                    errors.Add($"Dialogue questions[{questionIndex + 1}] must contain exactly one correct answer.");
                }

                for (int answerIndex = 0; answerIndex < question.Answers.Count; answerIndex++)
                {
                    ParsedDialogueAnswerModel answer = question.Answers[answerIndex];

                    if (string.IsNullOrWhiteSpace(NormalizeText(answer.Text)))
                    {
                        errors.Add($"Dialogue questions[{questionIndex + 1}] answers[{answerIndex + 1}] text is required.");
                    }

                    ValidateDialogueTranslations(
                        answer.Translations,
                        meaningLanguages,
                        $"Dialogue questions[{questionIndex + 1}] answers[{answerIndex + 1}] translations",
                        errors);
                }
            }

            if (errors.Count > 0)
            {
                issues.Add(new ImportIssueModel(null, "Error", $"Dialogue {index + 1} '{slug}': {string.Join(" ", errors)}"));
            }
        }
    }

    private static void ValidateDialogueTranslations(
        IReadOnlyList<ParsedContentMeaningModel> translations,
        IReadOnlySet<LanguageCode> meaningLanguages,
        string fieldName,
        ICollection<string> errors)
    {
        List<string> translationErrors = [];
        Dictionary<LanguageCode, string> normalizedTranslations = ValidateMeaningTranslations(
            translations,
            meaningLanguages,
            translationErrors);

        if (normalizedTranslations.Count == 0)
        {
            errors.Add($"{fieldName} must contain at least one valid translation.");
        }

        foreach (string translationError in translationErrors)
        {
            errors.Add($"{fieldName}: {translationError}");
        }
    }

    private static void ValidateDialogueStringList(
        IReadOnlyList<string> values,
        IReadOnlySet<string> allowedValues,
        string fieldName,
        bool requireAtLeastOne,
        ICollection<string> errors)
    {
        string[] normalizedValues = values
            .Select(value => NormalizeText(value).ToLowerInvariant())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

        if (requireAtLeastOne && normalizedValues.Length == 0)
        {
            errors.Add($"Dialogue {fieldName} must contain at least one item.");
        }

        foreach (string value in normalizedValues)
        {
            if (!ValidateKebabKey(value) || !allowedValues.Contains(value))
            {
                errors.Add($"Dialogue {fieldName} contains unsupported value '{value}'.");
            }
        }
    }

    private static void ValidateDialogueSentenceCounts(
        ParsedDialogueLessonModel dialogue,
        CefrLevel cefrLevel,
        ICollection<string> errors)
    {
        int minimumSentenceCount = cefrLevel switch
        {
            CefrLevel.A1 => 5,
            CefrLevel.A2 => 6,
            CefrLevel.B1 => 7,
            CefrLevel.B2 => 8,
            CefrLevel.C1 => 9,
            CefrLevel.C2 => 10,
            _ => 7
        };

        Dictionary<string, int> sentenceCountsByRole = dialogue.DialogueTurns
            .GroupBy(turn => NormalizeText(turn.SpeakerRole).ToLowerInvariant())
            .Where(group => !string.IsNullOrWhiteSpace(group.Key))
            .ToDictionary(
                group => group.Key,
                group => group.Sum(turn => CountDialogueSentences(turn.BaseText)),
                StringComparer.Ordinal);

        if (!sentenceCountsByRole.TryGetValue("learner", out int learnerSentenceCount) ||
            learnerSentenceCount < minimumSentenceCount)
        {
            errors.Add($"Dialogue learner side must contain at least {minimumSentenceCount} meaningful sentences for {cefrLevel}.");
        }

        int strongestPartnerSentenceCount = sentenceCountsByRole
            .Where(item => !string.Equals(item.Key, "learner", StringComparison.Ordinal))
            .Select(item => item.Value)
            .DefaultIfEmpty(0)
            .Max();

        if (strongestPartnerSentenceCount < minimumSentenceCount)
        {
            errors.Add($"Dialogue partner side must contain at least {minimumSentenceCount} meaningful sentences for {cefrLevel}.");
        }
    }

    private static int CountDialogueSentences(string text)
    {
        string normalized = NormalizeText(text);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return 0;
        }

        int sentenceCount = Regex.Matches(normalized, @"[.!?]+").Count;
        return sentenceCount == 0 ? 1 : sentenceCount;
    }

    private static void ValidateConversationStarterPacks(
        IReadOnlyList<ParsedConversationStarterPackModel> packs,
        IReadOnlyDictionary<string, Topic> topicsByKey,
        IReadOnlySet<LanguageCode> meaningLanguages,
        ICollection<ImportIssueModel> issues)
    {
        HashSet<string> slugs = [];

        for (int index = 0; index < packs.Count; index++)
        {
            ParsedConversationStarterPackModel pack = packs[index];
            List<string> errors = [];
            string slug = NormalizeText(pack.Slug).ToLowerInvariant();

            if (!ValidateKebabKey(slug))
            {
                errors.Add("Conversation starter pack slug is required and must use lowercase kebab-case.");
            }
            else if (!slugs.Add(slug))
            {
                errors.Add($"Duplicate conversation starter pack slug '{slug}' is not allowed inside one package.");
            }

            if (string.IsNullOrWhiteSpace(NormalizeText(pack.Title)))
            {
                errors.Add("Conversation starter pack title is required.");
            }

            if (string.IsNullOrWhiteSpace(NormalizeText(pack.Description)))
            {
                errors.Add("Conversation starter pack description is required.");
            }

            if (!Enum.TryParse(NormalizeText(pack.CefrLevel), true, out CefrLevel _))
            {
                errors.Add("Conversation starter pack CEFR level is invalid.");
            }

            ValidateConversationStarterKebabField(pack.Category, "category", errors);
            ValidateConversationStarterKebabField(pack.Situation, "situation", errors);
            ValidateConversationStarterKebabField(pack.Tone, "tone", errors);
            ValidateConversationStarterKebabField(pack.ConversationGoal, "conversationGoal", errors);

            string[] topicKeys = pack.Topics
                .Select(topic => NormalizeText(topic).ToLowerInvariant())
                .Where(topic => !string.IsNullOrWhiteSpace(topic))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (topicKeys.Length == 0)
            {
                errors.Add("Conversation starter pack topics must contain at least one topic key.");
            }

            foreach (string topicKey in topicKeys)
            {
                if (!topicsByKey.ContainsKey(topicKey))
                {
                    errors.Add($"Conversation starter pack references unknown topic key '{topicKey}'.");
                }
            }

            foreach (string linkedDialogueSlug in pack.LinkedDialogueSlugs.Select(slug => NormalizeText(slug).ToLowerInvariant()))
            {
                if (!ValidateKebabKey(linkedDialogueSlug))
                {
                    errors.Add("Conversation starter pack linkedDialogueSlugs must use lowercase kebab-case.");
                }
            }

            foreach (string linkedEventPreparationPackSlug in pack.LinkedEventPreparationPackSlugs.Select(slug => NormalizeText(slug).ToLowerInvariant()))
            {
                if (!ValidateKebabKey(linkedEventPreparationPackSlug))
                {
                    errors.Add("Conversation starter pack linkedEventPreparationPackSlugs must use lowercase kebab-case.");
                }
            }

            if (pack.Phrases.Count == 0)
            {
                errors.Add("Conversation starter pack phrases must contain at least one item.");
            }

            for (int phraseIndex = 0; phraseIndex < pack.Phrases.Count; phraseIndex++)
            {
                ParsedConversationStarterPhraseModel phrase = pack.Phrases[phraseIndex];

                if (string.IsNullOrWhiteSpace(NormalizeText(phrase.BaseText)))
                {
                    errors.Add($"Conversation starter pack phrases[{phraseIndex + 1}] baseText is required.");
                }

                string function = NormalizeText(phrase.Function).ToLowerInvariant();
                if (!ValidateKebabKey(function))
                {
                    errors.Add($"Conversation starter pack phrases[{phraseIndex + 1}] function is required and must use lowercase kebab-case.");
                }

                string? register = NormalizeOptionalText(phrase.Register)?.ToLowerInvariant();
                if (register is not null && !ValidateKebabKey(register))
                {
                    errors.Add($"Conversation starter pack phrases[{phraseIndex + 1}] register must use lowercase kebab-case.");
                }

                foreach (string alternative in phrase.AlternativeBaseTexts)
                {
                    if (string.IsNullOrWhiteSpace(NormalizeText(alternative)))
                    {
                        errors.Add($"Conversation starter pack phrases[{phraseIndex + 1}] alternativeBaseTexts cannot contain empty items.");
                    }
                }

                ValidateDialogueTranslations(
                    phrase.Translations,
                    meaningLanguages,
                    $"Conversation starter pack phrases[{phraseIndex + 1}] translations",
                    errors);
            }

            if (errors.Count > 0)
            {
                issues.Add(new ImportIssueModel(null, "Error", $"Conversation starter pack {index + 1} '{slug}': {string.Join(" ", errors)}"));
            }
        }
    }

    private static void ValidateConversationStarterKebabField(string value, string fieldName, ICollection<string> errors)
    {
        if (!ValidateKebabKey(NormalizeText(value).ToLowerInvariant()))
        {
            errors.Add($"Conversation starter pack {fieldName} is required and must use lowercase kebab-case.");
        }
    }

    private static void ValidateEventPreparationPacks(
        IReadOnlyList<ParsedEventPreparationPackModel> packs,
        IReadOnlyDictionary<string, Topic> topicsByKey,
        ICollection<ImportIssueModel> issues)
    {
        HashSet<string> slugs = [];

        for (int index = 0; index < packs.Count; index++)
        {
            ParsedEventPreparationPackModel pack = packs[index];
            List<string> errors = [];
            string slug = NormalizeText(pack.Slug).ToLowerInvariant();

            if (!ValidateKebabKey(slug))
            {
                errors.Add("Event preparation pack slug is required and must use lowercase kebab-case.");
            }
            else if (!slugs.Add(slug))
            {
                errors.Add($"Duplicate event preparation pack slug '{slug}' is not allowed inside one package.");
            }

            if (string.IsNullOrWhiteSpace(NormalizeText(pack.Title)))
            {
                errors.Add("Event preparation pack title is required.");
            }

            if (string.IsNullOrWhiteSpace(NormalizeText(pack.Description)))
            {
                errors.Add("Event preparation pack description is required.");
            }

            if (!Enum.TryParse(NormalizeText(pack.CefrLevel), true, out CefrLevel _))
            {
                errors.Add("Event preparation pack CEFR level is invalid.");
            }

            ValidateEventPreparationKebabField(pack.Category, "category", errors);
            ValidateEventPreparationKebabField(pack.EventType, "eventType", errors);

            string[] topicKeys = pack.Topics
                .Select(topic => NormalizeText(topic).ToLowerInvariant())
                .Where(topic => !string.IsNullOrWhiteSpace(topic))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (topicKeys.Length == 0)
            {
                errors.Add("Event preparation pack topics must contain at least one topic key.");
            }

            foreach (string topicKey in topicKeys)
            {
                if (!topicsByKey.ContainsKey(topicKey))
                {
                    errors.Add($"Event preparation pack references unknown topic key '{topicKey}'.");
                }
            }

            ValidateKebabReferences(pack.LinkedDialogueSlugs, "linkedDialogueSlugs", "Event preparation pack", errors);
            ValidateKebabReferences(pack.LinkedConversationStarterPackSlugs, "linkedConversationStarterPackSlugs", "Event preparation pack", errors);
            ValidatePromptList(pack.OpeningPrompts, "openingPrompts", errors);
            ValidatePromptList(pack.RoleplayPrompts, "roleplayPrompts", errors);
            ValidatePromptList(pack.ReviewPrompts, "reviewPrompts", errors);

            for (int referenceIndex = 0; referenceIndex < pack.LinkedVocabulary.Count; referenceIndex++)
            {
                ParsedEventPreparationVocabularyReferenceModel reference = pack.LinkedVocabulary[referenceIndex];
                if (string.IsNullOrWhiteSpace(NormalizeText(reference.Word)))
                {
                    errors.Add($"Event preparation pack linkedVocabulary[{referenceIndex + 1}] word is required.");
                }

                string? partOfSpeech = NormalizeOptionalText(reference.PartOfSpeech);
                if (partOfSpeech is not null && !Enum.TryParse(partOfSpeech, true, out PartOfSpeech _))
                {
                    errors.Add($"Event preparation pack linkedVocabulary[{referenceIndex + 1}] partOfSpeech is invalid.");
                }

                string? cefrLevel = NormalizeOptionalText(reference.CefrLevel);
                if (cefrLevel is not null && !Enum.TryParse(cefrLevel, true, out CefrLevel _))
                {
                    errors.Add($"Event preparation pack linkedVocabulary[{referenceIndex + 1}] cefrLevel is invalid.");
                }
            }

            if (errors.Count > 0)
            {
                issues.Add(new ImportIssueModel(null, "Error", $"Event preparation pack {index + 1} '{slug}': {string.Join(" ", errors)}"));
            }
        }
    }

    private static void ValidateEventPreparationKebabField(string value, string fieldName, ICollection<string> errors)
    {
        if (!ValidateKebabKey(NormalizeText(value).ToLowerInvariant()))
        {
            errors.Add($"Event preparation pack {fieldName} is required and must use lowercase kebab-case.");
        }
    }

    private static Dictionary<WordLabelKind, HashSet<string>> ValidateLabels(
        IReadOnlyList<ParsedContentLabelDefinitionModel> labels,
        bool labelsRequired,
        ICollection<ImportIssueModel> issues)
    {
        Dictionary<WordLabelKind, HashSet<string>> allowedLabelsByKind = new()
        {
            [WordLabelKind.Usage] = new HashSet<string>(StringComparer.Ordinal),
            [WordLabelKind.Context] = new HashSet<string>(StringComparer.Ordinal),
        };

        if (labels.Count == 0)
        {
            if (labelsRequired)
            {
                issues.Add(new ImportIssueModel(null, "Error", "The package labels array is required and must define localized label taxonomy before entries reference labels."));
            }

            return allowedLabelsByKind;
        }

        HashSet<string> uniqueLabels = new(StringComparer.Ordinal);

        for (int index = 0; index < labels.Count; index++)
        {
            ParsedContentLabelDefinitionModel label = labels[index];
            List<string> errors = [];
            string key = NormalizeText(label.Key).ToLowerInvariant();

            if (!Enum.TryParse(NormalizeText(label.Kind), true, out WordLabelKind kind) || !Enum.IsDefined(kind))
            {
                errors.Add("Label kind is invalid.");
            }

            if (!ValidateKebabKey(key))
            {
                errors.Add("Label key is required and must use lowercase kebab-case.");
            }

            if (string.IsNullOrWhiteSpace(NormalizeText(label.DisplayName)))
            {
                errors.Add("Label displayName is required.");
            }

            if (errors.Count == 0 && !uniqueLabels.Add($"{kind}::{key}"))
            {
                errors.Add($"Duplicate label '{kind}:{key}' is not allowed.");
            }

            ValidateLocalizedCollectionText(label.Localizations, "Label localizations", errors);

            if (errors.Count > 0)
            {
                issues.Add(new ImportIssueModel(null, "Error", $"Label {index + 1} '{key}': {string.Join(" ", errors)}"));
                continue;
            }

            if (!allowedLabelsByKind.TryGetValue(kind, out HashSet<string>? labelsForKind))
            {
                labelsForKind = new HashSet<string>(StringComparer.Ordinal);
                allowedLabelsByKind[kind] = labelsForKind;
            }

            labelsForKind.Add(key);
        }

        return allowedLabelsByKind;
    }

    private static bool EntryReferencesLabels(ParsedContentEntryModel entry)
    {
        return entry.UsageLabels.Any(label => !string.IsNullOrWhiteSpace(label)) ||
               entry.ContextLabels.Any(label => !string.IsNullOrWhiteSpace(label));
    }

    private static void ValidateKebabReferences(
        IReadOnlyList<string> references,
        string fieldName,
        string ownerLabel,
        ICollection<string> errors)
    {
        foreach (string reference in references.Select(value => NormalizeText(value).ToLowerInvariant()))
        {
            if (!ValidateKebabKey(reference))
            {
                errors.Add($"{ownerLabel} {fieldName} must use lowercase kebab-case.");
            }
        }
    }

    private static void ValidatePromptList(
        IReadOnlyList<string> prompts,
        string fieldName,
        ICollection<string> errors)
    {
        foreach (string prompt in prompts)
        {
            if (string.IsNullOrWhiteSpace(NormalizeText(prompt)))
            {
                errors.Add($"Event preparation pack {fieldName} cannot contain empty items.");
            }
        }
    }

    private static bool ValidateKebabKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        bool isValid = value.All(character =>
            (character >= 'a' && character <= 'z') ||
            (character >= '0' && character <= '9') ||
            character == '-');

        return isValid &&
            !value.StartsWith("-", StringComparison.Ordinal) &&
            !value.EndsWith("-", StringComparison.Ordinal);
    }

    private static bool CanReimportPackageByContentSlug(ParsedContentPackageModel parsedPackage)
    {
        if (parsedPackage.Entries.Count > 0 ||
            parsedPackage.Labels.Count > 0 ||
            parsedPackage.Collections.Count > 0)
        {
            return false;
        }

        return parsedPackage.Dialogues.Count > 0 ||
            parsedPackage.TalkTopics.Count > 0 ||
            parsedPackage.GrammarTopics.Count > 0 ||
            parsedPackage.ExpressionEntries.Count > 0 ||
            parsedPackage.Exercises.Count > 0 ||
            parsedPackage.ExerciseSets.Count > 0 ||
            parsedPackage.CoursePaths.Count > 0 ||
            parsedPackage.CourseModules.Count > 0 ||
            parsedPackage.CourseLessons.Count > 0 ||
            parsedPackage.WritingTemplates.Count > 0 ||
            parsedPackage.CulturalNotes.Count > 0 ||
            parsedPackage.ExamProfiles.Count > 0 ||
            parsedPackage.ExamPrepUnits.Count > 0 ||
            parsedPackage.ConversationStarterPacks.Count > 0 ||
            parsedPackage.EventPreparationPacks.Count > 0;
    }

    private static string CreateReimportPackageId(string packageId, DateTime timestampUtc)
    {
        string normalizedPackageId = packageId.Trim();
        string suffix = $"-reimport-{timestampUtc:yyyyMMddHHmmssfff}";
        int maximumPrefixLength = Math.Max(1, 128 - suffix.Length);

        if (normalizedPackageId.Length > maximumPrefixLength)
        {
            normalizedPackageId = normalizedPackageId[..maximumPrefixLength].TrimEnd('-');
        }

        return normalizedPackageId + suffix;
    }

    private static void ValidatePackage(ParsedContentPackageModel parsedPackage, ICollection<ImportIssueModel> issues)
    {
        if (string.IsNullOrWhiteSpace(parsedPackage.PackageVersion))
        {
            issues.Add(new ImportIssueModel(null, "Error", "Package version is required."));
        }
        else if (!string.Equals(parsedPackage.PackageVersion.Trim(), SupportedPackageVersion, StringComparison.Ordinal))
        {
            issues.Add(new ImportIssueModel(null, "Error", $"Unsupported package version '{parsedPackage.PackageVersion}'."));
        }

        if (string.IsNullOrWhiteSpace(parsedPackage.PackageId))
        {
            issues.Add(new ImportIssueModel(null, "Error", "Package identifier is required."));
        }

        if (string.IsNullOrWhiteSpace(parsedPackage.PackageName))
        {
            issues.Add(new ImportIssueModel(null, "Error", "Package name is required."));
        }

        if (parsedPackage.Entries.Count == 0 &&
            parsedPackage.Collections.Count == 0 &&
            parsedPackage.Dialogues.Count == 0 &&
            parsedPackage.TalkTopics.Count == 0 &&
            parsedPackage.GrammarTopics.Count == 0 &&
            parsedPackage.ExpressionEntries.Count == 0 &&
            parsedPackage.Exercises.Count == 0 &&
            parsedPackage.ExerciseSets.Count == 0 &&
            parsedPackage.CoursePaths.Count == 0 &&
            parsedPackage.CourseModules.Count == 0 &&
            parsedPackage.CourseLessons.Count == 0 &&
            parsedPackage.WritingTemplates.Count == 0 &&
            parsedPackage.CulturalNotes.Count == 0 &&
            parsedPackage.ExamProfiles.Count == 0 &&
            parsedPackage.ExamPrepUnits.Count == 0 &&
            parsedPackage.ConversationStarterPacks.Count == 0 &&
            parsedPackage.EventPreparationPacks.Count == 0)
        {
            issues.Add(new ImportIssueModel(null, "Error", "The package must contain at least one content item."));
        }

        HashSet<string> collectionSlugs = [];
        for (int index = 0; index < parsedPackage.Collections.Count; index++)
        {
            ParsedContentCollectionModel collection = parsedPackage.Collections[index];
            string normalizedSlug = NormalizeText(collection.Slug).ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(normalizedSlug))
            {
                issues.Add(new ImportIssueModel(null, "Error", $"Collection {index + 1} slug is required."));
                continue;
            }

            if (!collectionSlugs.Add(normalizedSlug))
            {
                issues.Add(new ImportIssueModel(null, "Error", $"Duplicate collection slug '{normalizedSlug}' is not allowed inside one package."));
            }
        }
    }

    private static Dictionary<LanguageCode, string> ValidateMeaningTranslations(
        IReadOnlyList<ParsedContentMeaningModel> meanings,
        IReadOnlySet<LanguageCode> meaningLanguages,
        ICollection<string> entryErrors)
    {
        Dictionary<LanguageCode, string> translations = [];

        foreach (ParsedContentMeaningModel meaning in meanings)
        {
            string normalizedLanguage = NormalizeText(meaning.Language).ToLowerInvariant();
            string normalizedText = NormalizeText(meaning.Text);

            if (string.IsNullOrWhiteSpace(normalizedLanguage))
            {
                entryErrors.Add("Meaning language is required.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(normalizedText))
            {
                entryErrors.Add("Meaning text is required.");
                continue;
            }

            LanguageCode languageCode = LanguageCode.From(normalizedLanguage);

            if (!meaningLanguages.Contains(languageCode))
            {
                entryErrors.Add($"Meaning language '{normalizedLanguage}' is not supported.");
                continue;
            }

            if (!translations.TryAdd(languageCode, normalizedText))
            {
                entryErrors.Add($"Duplicate meaning language '{normalizedLanguage}' is not allowed.");
            }
        }

        IReadOnlyList<string> missing = ContentLanguageRequirements.FindMissingMeaningLanguages(
            translations.Keys.Select(language => language.Value));
        if (missing.Count > 0)
        {
            entryErrors.Add($"Missing required meaning languages: {string.Join(", ", missing)}.");
        }

        return translations;
    }

    private static ParsedLocalizedTextModel[] ValidateLocalizedCollectionText(
        IReadOnlyList<ParsedLocalizedTextModel> localizations,
        string fieldName,
        ICollection<string> errors)
    {
        if (localizations.Count == 0)
        {
            errors.Add($"{fieldName} must contain all required languages: {ContentLanguageRequirements.FormatRequiredLocalizationLanguages()}.");
            return [];
        }

        List<ParsedLocalizedTextModel> normalized = [];
        HashSet<string> seenLanguages = new(StringComparer.Ordinal);

        for (int index = 0; index < localizations.Count; index++)
        {
            ParsedLocalizedTextModel localization = localizations[index];
            string language = NormalizeText(localization.Language).ToLowerInvariant();
            string name = NormalizeText(localization.Name);

            if (string.IsNullOrWhiteSpace(language))
            {
                errors.Add($"{fieldName}[{index + 1}] language is required.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add($"{fieldName}[{index + 1}] name is required.");
                continue;
            }

            try
            {
                LanguageCode.From(language);
            }
            catch (DomainRuleException exception)
            {
                errors.Add($"{fieldName}[{index + 1}] language is invalid: {exception.Message}");
                continue;
            }

            if (!seenLanguages.Add(language))
            {
                errors.Add($"{fieldName} contains duplicate language '{language}'.");
                continue;
            }

            normalized.Add(new ParsedLocalizedTextModel(language, name, NormalizeOptionalText(localization.Description)));
        }

        IReadOnlyList<string> missing = ContentLanguageRequirements.FindMissingLocalizationLanguages(seenLanguages);
        if (missing.Count > 0)
        {
            errors.Add($"{fieldName} are missing languages: {string.Join(", ", missing)}.");
        }

        return normalized.ToArray();
    }

    private static void ValidateTalkTopics(
        IReadOnlyList<ParsedTalkTopicModel> talkTopics,
        IReadOnlyDictionary<string, Topic> topicsByKey,
        IReadOnlySet<LanguageCode> meaningLanguages,
        ICollection<ImportIssueModel> issues)
    {
        HashSet<string> slugs = [];

        for (int index = 0; index < talkTopics.Count; index++)
        {
            ParsedTalkTopicModel topic = talkTopics[index];
            List<string> errors = [];
            List<string> warnings = [];
            string slug = NormalizeText(topic.Slug).ToLowerInvariant();

            if (!ValidateKebabKey(slug))
            {
                errors.Add("Talk topic slug is required and must use lowercase kebab-case.");
            }
            else if (!slugs.Add(slug))
            {
                errors.Add($"Duplicate talk topic slug '{slug}' is not allowed inside one package.");
            }

            if (!ValidateKebabKey(NormalizeText(topic.TopicGroupKey).ToLowerInvariant()))
            {
                errors.Add("Talk topic topicGroupKey is required and must use lowercase kebab-case.");
            }

            if (string.IsNullOrWhiteSpace(NormalizeText(topic.Title)))
            {
                errors.Add("Talk topic title is required.");
            }

            if (string.IsNullOrWhiteSpace(NormalizeText(topic.Description)))
            {
                errors.Add("Talk topic description is required.");
            }

            if (!Enum.TryParse(NormalizeText(topic.CefrLevel), true, out CefrLevel cefrLevel))
            {
                errors.Add("Talk topic CEFR level is invalid.");
            }

            if (!ValidateKebabKey(NormalizeText(topic.Category).ToLowerInvariant()))
            {
                errors.Add("Talk topic category is required and must use lowercase kebab-case.");
            }

            if (!TryParseTalkTopicContentType(topic.ContentType, out _))
            {
                errors.Add($"Talk topic contentType '{NormalizeText(topic.ContentType)}' is not supported.");
            }

            string[] topicKeys = topic.Topics
                .Select(item => NormalizeText(item).ToLowerInvariant())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (topicKeys.Length == 0)
            {
                errors.Add("Talk topic topics must contain at least one topic key.");
            }

            foreach (string topicKey in topicKeys)
            {
                if (!topicsByKey.ContainsKey(topicKey))
                {
                    errors.Add($"Talk topic references unknown topic key '{topicKey}'.");
                }
            }

            string articleText = NormalizeText(topic.Article.BaseText);
            if (string.IsNullOrWhiteSpace(articleText))
            {
                errors.Add("Talk topic article.baseText is required.");
            }
            else if (Enum.TryParse(NormalizeText(topic.CefrLevel), true, out CefrLevel articleCefrLevel))
            {
                (int minimumLength, int maximumLength) = GetTalkTopicArticleLengthRange(articleCefrLevel);
                if (articleText.Length < minimumLength || articleText.Length > maximumLength)
                {
                    errors.Add($"Talk topic article.baseText for {articleCefrLevel} must contain {minimumLength}-{maximumLength} normalized German characters; found {articleText.Length}.");
                }
            }

            if (topic.Article.Translations.Count > 0)
            {
                errors.Add("Talk topic article.translations is not supported; Talk Topic articles are German-only for now.");
            }

            int minimumWarmupQuestionCount = IsUpperIntermediateOrHigher(cefrLevel) ? 4 : 3;
            if (topic.WarmupQuestions.Count < minimumWarmupQuestionCount)
            {
                errors.Add($"Talk topic warmupQuestions must contain at least {minimumWarmupQuestionCount} questions for {cefrLevel}.");
            }

            ValidateTalkTopicQuestions(topic.WarmupQuestions, meaningLanguages, "Talk topic warmupQuestions", errors);

            if (topic.DiscussionQuestions.Count == 0)
            {
                errors.Add("Talk topic discussionQuestions must contain at least one question.");
            }

            for (int questionIndex = 0; questionIndex < topic.DiscussionQuestions.Count; questionIndex++)
            {
                ParsedTalkTopicDiscussionQuestionModel question = topic.DiscussionQuestions[questionIndex];
                if (string.IsNullOrWhiteSpace(NormalizeText(question.Prompt)))
                {
                    errors.Add($"Talk topic discussionQuestions[{questionIndex + 1}] prompt is required.");
                }

                if (!TryParseTalkTopicQuestionType(question.QuestionType, out _))
                {
                    errors.Add($"Talk topic discussionQuestions[{questionIndex + 1}] questionType is invalid.");
                }

                if (question.Translations.Count > 0)
                {
                    errors.Add($"Talk topic discussionQuestions[{questionIndex + 1}].translations is not supported; Talk Topic questions are German-only for now.");
                }
            }

            if (Enum.TryParse(NormalizeText(topic.CefrLevel), true, out CefrLevel questionCefrLevel))
            {
                int minimumPerQuestionType = IsUpperIntermediateOrHigher(questionCefrLevel) ? 3 : 2;
                string[] requiredQuestionTypes = ["opinion", "imagination", "prediction", "comparison"];
                foreach (string requiredQuestionType in requiredQuestionTypes)
                {
                    int count = topic.DiscussionQuestions.Count(question =>
                        string.Equals(NormalizeText(question.QuestionType).ToLowerInvariant(), requiredQuestionType, StringComparison.Ordinal));
                    if (count < minimumPerQuestionType)
                    {
                        errors.Add($"Talk topic discussionQuestions must contain at least {minimumPerQuestionType} '{requiredQuestionType}' questions for {questionCefrLevel}; found {count}.");
                    }
                }
            }

            if (topic.VocabularyItems.Count == 0)
            {
                errors.Add("Talk topic vocabularyItems must contain at least one item.");
            }
            else if (Enum.TryParse(NormalizeText(topic.CefrLevel), true, out CefrLevel vocabularyCefrLevel))
            {
                (int minimumVocabularyCount, int maximumVocabularyCount) = GetTalkTopicVocabularyCountRange(vocabularyCefrLevel);
                if (topic.VocabularyItems.Count < minimumVocabularyCount || topic.VocabularyItems.Count > maximumVocabularyCount)
                {
                    errors.Add($"Talk topic vocabularyItems must contain {minimumVocabularyCount}-{maximumVocabularyCount} items for {vocabularyCefrLevel}; found {topic.VocabularyItems.Count}.");
                }
            }

            for (int vocabularyIndex = 0; vocabularyIndex < topic.VocabularyItems.Count; vocabularyIndex++)
            {
                ParsedTalkTopicVocabularyItemModel item = topic.VocabularyItems[vocabularyIndex];
                if (string.IsNullOrWhiteSpace(NormalizeText(item.Lemma)) && string.IsNullOrWhiteSpace(NormalizeText(item.WordSlug)))
                {
                    errors.Add($"Talk topic vocabularyItems[{vocabularyIndex + 1}] must include lemma or wordSlug.");
                }

                string? normalizedWordSlug = NormalizeOptionalText(item.WordSlug)?.ToLowerInvariant();
                if (normalizedWordSlug is not null && !ValidateKebabKey(normalizedWordSlug))
                {
                    errors.Add($"Talk topic vocabularyItems[{vocabularyIndex + 1}] wordSlug must use lowercase kebab-case.");
                }

                if (!string.IsNullOrWhiteSpace(item.CefrLevel) && !Enum.TryParse(NormalizeText(item.CefrLevel), true, out CefrLevel _))
                {
                    errors.Add($"Talk topic vocabularyItems[{vocabularyIndex + 1}] cefrLevel is invalid.");
                }
            }

            string[] speakingGoals = topic.SpeakingGoals
                .Select(goal => NormalizeText(goal).ToLowerInvariant())
                .Where(goal => !string.IsNullOrWhiteSpace(goal))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (speakingGoals.Length == 0)
            {
                errors.Add("Talk topic speakingGoals must contain at least one value.");
            }
            else if (speakingGoals.Length < 2 || speakingGoals.Length > 5)
            {
                errors.Add($"Talk topic speakingGoals must contain 2-5 values; found {speakingGoals.Length}.");
            }

            foreach (string speakingGoal in speakingGoals)
            {
                if (!TryParseTalkTopicSpeakingGoal(speakingGoal, out _))
                {
                    errors.Add($"Talk topic speakingGoal '{speakingGoal}' is invalid.");
                }
            }

            if (topic.EstimatedReadingMinutes <= 0)
            {
                errors.Add("Talk topic estimatedReadingMinutes must be greater than zero.");
            }

            if (topic.EstimatedDiscussionMinutes <= 0)
            {
                errors.Add("Talk topic estimatedDiscussionMinutes must be greater than zero.");
            }

            if (topic.RecommendedForModeratedGroupsOnly && !topic.IsSensitive)
            {
                warnings.Add("Talk topic recommendedForModeratedGroupsOnly is true while isSensitive is false.");
            }

            foreach (string error in errors)
            {
                issues.Add(new ImportIssueModel(null, "Error", $"talkTopics[{index + 1}]: {error}"));
            }

            foreach (string warning in warnings)
            {
                issues.Add(new ImportIssueModel(null, "Warning", $"talkTopics[{index + 1}]: {warning}"));
            }
        }
    }

    private static void ValidateGrammarTopics(
        IReadOnlyList<ParsedGrammarTopicModel> grammarTopics,
        IReadOnlyDictionary<string, Topic> topicsByKey,
        IReadOnlySet<LanguageCode> meaningLanguages,
        ICollection<ImportIssueModel> issues)
    {
        HashSet<string> slugs = [];

        for (int index = 0; index < grammarTopics.Count; index++)
        {
            ParsedGrammarTopicModel topic = grammarTopics[index];
            List<string> errors = [];
            List<string> warnings = [];
            string slug = NormalizeText(topic.Slug).ToLowerInvariant();

            if (!ValidateKebabKey(slug))
            {
                errors.Add("Grammar topic slug is required and must use lowercase kebab-case.");
            }
            else if (!slugs.Add(slug))
            {
                errors.Add($"Duplicate grammar topic slug '{slug}' is not allowed inside one package.");
            }

            if (string.IsNullOrWhiteSpace(NormalizeText(topic.Title)))
            {
                errors.Add("Grammar topic title is required.");
            }

            if (string.IsNullOrWhiteSpace(NormalizeText(topic.ShortDescription)))
            {
                errors.Add("Grammar topic shortDescription is required.");
            }

            ValidateLocalizedTextDictionary(topic.TitleLocalized, meaningLanguages, "Grammar topic titleLocalized", errors);
            ValidateLocalizedTextDictionary(topic.ShortDescriptionLocalized, meaningLanguages, "Grammar topic shortDescriptionLocalized", errors);

            if (!Enum.TryParse(NormalizeText(topic.CefrLevel), true, out CefrLevel _))
            {
                errors.Add("Grammar topic CEFR level is invalid.");
            }

            string grammarCategory = NormalizeText(topic.GrammarCategory).ToLowerInvariant();
            if (!GrammarCategories.Contains(grammarCategory))
            {
                errors.Add($"Grammar topic grammarCategory '{grammarCategory}' is not supported.");
            }

            string[] topicKeys = topic.Topics
                .Select(item => NormalizeText(item).ToLowerInvariant())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            foreach (string topicKey in topicKeys)
            {
                if (!topicsByKey.ContainsKey(topicKey))
                {
                    warnings.Add($"Grammar topic references unknown topic key '{topicKey}'.");
                }
            }

            if (topic.Sections.Count == 0)
            {
                errors.Add("Grammar topic sections must contain at least one explanation section.");
            }

            HashSet<string> richSectionKeys = new(StringComparer.Ordinal);
            for (int sectionIndex = 0; sectionIndex < topic.Sections.Count; sectionIndex++)
            {
                ParsedGrammarSectionModel section = topic.Sections[sectionIndex];
                bool hasRichBlocks = section.LocalizedBlocksJson.Count > 0;
                string sectionKey = NormalizeText(section.SectionKey).ToLowerInvariant();
                if (hasRichBlocks && !ValidateKebabKey(sectionKey))
                {
                    errors.Add($"Grammar topic sections[{sectionIndex + 1}] sectionKey is required and must use lowercase kebab-case.");
                }
                else if (hasRichBlocks && !richSectionKeys.Add(sectionKey))
                {
                    errors.Add($"Grammar topic sections[{sectionIndex + 1}] sectionKey '{sectionKey}' is duplicated.");
                }

                if (string.IsNullOrWhiteSpace(NormalizeText(section.Heading)))
                {
                    errors.Add($"Grammar topic sections[{sectionIndex + 1}] heading is required.");
                }

                if (string.IsNullOrWhiteSpace(NormalizeText(section.Explanation)))
                {
                    errors.Add($"Grammar topic sections[{sectionIndex + 1}] explanation is required.");
                }

                ValidateGrammarSectionTranslations(section.Translations, meaningLanguages, $"Grammar topic sections[{sectionIndex + 1}].translations", errors);
                ValidateGrammarLocalizedBlocks(section.LocalizedBlocksJson, meaningLanguages, $"Grammar topic sections[{sectionIndex + 1}].localizedBlocks", errors);
            }

            for (int exampleIndex = 0; exampleIndex < topic.Examples.Count; exampleIndex++)
            {
                ParsedGrammarExampleModel example = topic.Examples[exampleIndex];
                if (string.IsNullOrWhiteSpace(NormalizeText(example.GermanText)))
                {
                    errors.Add($"Grammar topic examples[{exampleIndex + 1}] germanText is required.");
                }

                ValidateOptionalMeaningTranslations(example.Translations, meaningLanguages, $"Grammar topic examples[{exampleIndex + 1}].translations", errors);
            }

            ValidateGrammarTextItems(topic.RuleSummaries, meaningLanguages, "Grammar topic ruleSummaries", errors);
            ValidateGrammarTextItems(topic.ExceptionNotes, meaningLanguages, "Grammar topic exceptionNotes", errors);

            for (int mistakeIndex = 0; mistakeIndex < topic.CommonMistakes.Count; mistakeIndex++)
            {
                ParsedGrammarCommonMistakeModel mistake = topic.CommonMistakes[mistakeIndex];
                if (string.IsNullOrWhiteSpace(NormalizeText(mistake.WrongText)))
                {
                    errors.Add($"Grammar topic commonMistakes[{mistakeIndex + 1}] wrongText is required.");
                }

                if (string.IsNullOrWhiteSpace(NormalizeText(mistake.CorrectedText)))
                {
                    errors.Add($"Grammar topic commonMistakes[{mistakeIndex + 1}] correctedText is required.");
                }

                if (string.IsNullOrWhiteSpace(NormalizeText(mistake.Explanation)))
                {
                    errors.Add($"Grammar topic commonMistakes[{mistakeIndex + 1}] explanation is required.");
                }

                ValidateOptionalMeaningTranslations(mistake.Translations, meaningLanguages, $"Grammar topic commonMistakes[{mistakeIndex + 1}].translations", errors);
            }

            ValidateGrammarSlugList(topic.PrerequisiteSlugs, "Grammar topic prerequisiteSlugs", errors);
            ValidateGrammarSlugList(topic.RelatedTopicSlugs, "Grammar topic relatedTopicSlugs", errors);
            ValidateGrammarSlugList(topic.LinkedDialogueSlugs, "Grammar topic linkedDialogueSlugs", errors);
            ValidateGrammarSlugList(topic.LinkedTalkTopicSlugs, "Grammar topic linkedTalkTopicSlugs", errors);
            ValidateGrammarSlugList(topic.LinkedExerciseSlugs, "Grammar topic linkedExerciseSlugs", warnings);

            foreach (ParsedGrammarLinkedWordModel word in topic.LinkedWords)
            {
                if (string.IsNullOrWhiteSpace(NormalizeText(word.Lemma)))
                {
                    errors.Add("Grammar topic linkedWords cannot contain an empty lemma.");
                }

                string? wordSlug = NormalizeOptionalText(word.WordSlug)?.ToLowerInvariant();
                if (wordSlug is not null && !ValidateKebabKey(wordSlug))
                {
                    errors.Add($"Grammar topic linkedWords wordSlug '{wordSlug}' must use lowercase kebab-case.");
                }
            }

            foreach (string error in errors)
            {
                issues.Add(new ImportIssueModel(null, "Error", $"grammarTopics[{index + 1}]: {error}"));
            }

            foreach (string warning in warnings)
            {
                issues.Add(new ImportIssueModel(null, "Warning", $"grammarTopics[{index + 1}]: {warning}"));
            }
        }
    }

    private static void ValidateExpressionEntries(
        IReadOnlyList<ParsedExpressionEntryModel> expressions,
        IReadOnlyDictionary<string, Topic> topicsByKey,
        IReadOnlySet<LanguageCode> meaningLanguages,
        ICollection<ImportIssueModel> issues)
    {
        HashSet<string> slugs = [];

        for (int index = 0; index < expressions.Count; index++)
        {
            ParsedExpressionEntryModel expression = expressions[index];
            List<string> errors = [];
            List<string> warnings = [];
            string slug = NormalizeText(expression.Slug).ToLowerInvariant();

            if (!ValidateKebabKey(slug))
            {
                errors.Add("Expression slug is required and must use lowercase kebab-case.");
            }
            else if (!slugs.Add(slug))
            {
                errors.Add($"Duplicate expression slug '{slug}' is not allowed inside one package.");
            }

            if (string.IsNullOrWhiteSpace(NormalizeText(expression.ExpressionText)))
            {
                errors.Add("Expression expressionText is required.");
            }

            if (string.IsNullOrWhiteSpace(NormalizeText(expression.ActualMeaningText)))
            {
                errors.Add("Expression actualMeaningText is required.");
            }

            if (!Enum.TryParse(NormalizeText(expression.CefrLevel), true, out CefrLevel _))
            {
                errors.Add("Expression CEFR level is invalid.");
            }

            string expressionType = NormalizeText(expression.ExpressionType).ToLowerInvariant();
            if (!ExpressionTypes.Contains(expressionType))
            {
                errors.Add($"Expression expressionType '{expressionType}' is not supported.");
            }

            string register = NormalizeText(expression.Register).ToLowerInvariant();
            if (!ExpressionRegisters.Contains(register))
            {
                errors.Add($"Expression register '{register}' is not supported.");
            }

            string category = NormalizeText(expression.Category).ToLowerInvariant();
            if (!ValidateKebabKey(category))
            {
                errors.Add("Expression category is required and must use lowercase kebab-case.");
            }

            string[] topicKeys = expression.Topics
                .Select(item => NormalizeText(item).ToLowerInvariant())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            foreach (string topicKey in topicKeys)
            {
                if (!topicsByKey.ContainsKey(topicKey))
                {
                    errors.Add($"Expression references unknown topic key '{topicKey}'.");
                }
            }

            ValidateExpressionMeanings(expression.Meanings, meaningLanguages, errors);

            for (int exampleIndex = 0; exampleIndex < expression.Examples.Count; exampleIndex++)
            {
                ParsedExpressionExampleModel example = expression.Examples[exampleIndex];
                if (string.IsNullOrWhiteSpace(NormalizeText(example.GermanText)))
                {
                    errors.Add($"Expression examples[{exampleIndex + 1}] germanText is required.");
                }

                ValidateOptionalMeaningTranslations(example.Translations, meaningLanguages, $"Expression examples[{exampleIndex + 1}].translations", errors);
            }

            for (int warningIndex = 0; warningIndex < expression.Warnings.Count; warningIndex++)
            {
                ParsedExpressionWarningModel warning = expression.Warnings[warningIndex];
                if (string.IsNullOrWhiteSpace(NormalizeText(warning.WarningType)))
                {
                    errors.Add($"Expression warnings[{warningIndex + 1}] warningType is required.");
                }

                if (string.IsNullOrWhiteSpace(NormalizeText(warning.Text)))
                {
                    errors.Add($"Expression warnings[{warningIndex + 1}] text is required.");
                }

                ValidateOptionalMeaningTranslations(warning.Translations, meaningLanguages, $"Expression warnings[{warningIndex + 1}].translations", errors);
            }

            foreach (ParsedExpressionLinkedWordModel word in expression.LinkedWords)
            {
                if (string.IsNullOrWhiteSpace(NormalizeText(word.Lemma)))
                {
                    errors.Add("Expression linkedWords cannot contain an empty lemma.");
                }

                string? wordSlug = NormalizeOptionalText(word.WordSlug)?.ToLowerInvariant();
                if (wordSlug is not null && !ValidateKebabKey(wordSlug))
                {
                    errors.Add($"Expression linkedWords wordSlug '{wordSlug}' must use lowercase kebab-case.");
                }
            }

            ValidateGrammarSlugList(expression.RelatedExpressionSlugs, "Expression relatedExpressionSlugs", errors);
            ValidateGrammarSlugList(expression.LinkedExerciseSlugs, "Expression linkedExerciseSlugs", warnings);

            bool requiresWarning = expression.IsRisky ||
                RiskyExpressionRegisters.Contains(register) ||
                RiskyExpressionTypes.Contains(expressionType);

            if (requiresWarning && expression.Warnings.All(warning => string.IsNullOrWhiteSpace(NormalizeText(warning.Text))))
            {
                errors.Add("Risky expressions require at least one warning with text.");
            }

            foreach (string error in errors)
            {
                issues.Add(new ImportIssueModel(null, "Error", $"expressionEntries[{index + 1}]: {error}"));
            }

            foreach (string warning in warnings)
            {
                issues.Add(new ImportIssueModel(null, "Warning", $"expressionEntries[{index + 1}]: {warning}"));
            }
        }
    }

    private static void ValidateExpressionMeanings(
        IReadOnlyList<ParsedExpressionMeaningModel> meanings,
        IReadOnlySet<LanguageCode> meaningLanguages,
        ICollection<string> errors)
    {
        HashSet<LanguageCode> seenLanguages = [];

        for (int index = 0; index < meanings.Count; index++)
        {
            ParsedExpressionMeaningModel meaning = meanings[index];
            string language = NormalizeText(meaning.Language).ToLowerInvariant();
            string actualMeaning = NormalizeText(meaning.ActualMeaningText);

            if (string.IsNullOrWhiteSpace(language) || string.IsNullOrWhiteSpace(actualMeaning))
            {
                errors.Add($"Expression meanings[{index + 1}] language and actualMeaningText are required when meanings are provided.");
                continue;
            }

            LanguageCode languageCode;
            try
            {
                languageCode = LanguageCode.From(language);
            }
            catch (DomainRuleException exception)
            {
                errors.Add($"Expression meanings[{index + 1}] language is invalid: {exception.Message}");
                continue;
            }

            if (!meaningLanguages.Contains(languageCode))
            {
                errors.Add($"Expression meanings[{index + 1}] language '{language}' is not an active meaning language.");
                continue;
            }

            if (!seenLanguages.Add(languageCode))
            {
                errors.Add($"Expression meanings contains duplicate language '{language}'.");
            }
        }
    }

    private static void ValidateGrammarSectionTranslations(
        IReadOnlyList<ParsedGrammarSectionTranslationModel> translations,
        IReadOnlySet<LanguageCode> meaningLanguages,
        string fieldName,
        ICollection<string> errors)
    {
        HashSet<LanguageCode> seenLanguages = [];
        for (int index = 0; index < translations.Count; index++)
        {
            ParsedGrammarSectionTranslationModel translation = translations[index];
            string language = NormalizeText(translation.Language).ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(language) ||
                string.IsNullOrWhiteSpace(NormalizeText(translation.Heading)) ||
                string.IsNullOrWhiteSpace(NormalizeText(translation.Text)))
            {
                errors.Add($"{fieldName}[{index + 1}] language, heading, and text are required when translations are provided.");
                continue;
            }

            LanguageCode languageCode;
            try
            {
                languageCode = LanguageCode.From(language);
            }
            catch (DomainRuleException exception)
            {
                errors.Add($"{fieldName}[{index + 1}] language is invalid: {exception.Message}");
                continue;
            }

            if (!meaningLanguages.Contains(languageCode))
            {
                errors.Add($"{fieldName}[{index + 1}] language '{language}' is not an active meaning language.");
                continue;
            }

            if (!seenLanguages.Add(languageCode))
            {
                errors.Add($"{fieldName} contains duplicate language '{language}'.");
            }
        }
    }

    private static void ValidateExercises(
        IReadOnlyList<ParsedExerciseModel> exercises,
        IReadOnlyList<ParsedExerciseSetModel> exerciseSets,
        ICollection<ImportIssueModel> issues)
    {
        HashSet<string> exerciseSlugs = [];
        for (int index = 0; index < exercises.Count; index++)
        {
            ParsedExerciseModel exercise = exercises[index];
            List<string> errors = [];
            string slug = NormalizeText(exercise.Slug).ToLowerInvariant();
            if (!ValidateKebabKey(slug)) errors.Add("Exercise slug is required and must use lowercase kebab-case.");
            else if (!exerciseSlugs.Add(slug)) errors.Add($"Duplicate exercise slug '{slug}' is not allowed inside one package.");
            if (string.IsNullOrWhiteSpace(NormalizeText(exercise.Title))) errors.Add("Exercise title is required.");
            if (string.IsNullOrWhiteSpace(NormalizeText(exercise.Instruction))) errors.Add("Exercise instruction is required.");
            if (!Enum.TryParse(NormalizeText(exercise.CefrLevel), true, out CefrLevel _)) errors.Add("Exercise CEFR level is invalid.");
            string exerciseType = NormalizeText(exercise.ExerciseType).ToLowerInvariant();
            if (!ExerciseTypes.Contains(exerciseType)) errors.Add($"Exercise exerciseType '{exerciseType}' is not supported.");
            string targetSkill = NormalizeText(exercise.TargetSkill).ToLowerInvariant();
            if (!ExerciseTargetSkills.Contains(targetSkill)) errors.Add($"Exercise targetSkill '{targetSkill}' is not supported.");
            string ownerType = NormalizeText(exercise.OwnerType).ToLowerInvariant();
            if (!ExerciseOwnerTypes.Contains(ownerType)) errors.Add($"Exercise ownerType '{ownerType}' is not supported.");
            string? ownerSlug = NormalizeOptionalText(exercise.OwnerSlug)?.ToLowerInvariant();
            if (ownerSlug is not null && !ValidateKebabKey(ownerSlug)) errors.Add("Exercise ownerSlug must use lowercase kebab-case.");
            ValidateJsonObject(exercise.PromptJson, "Exercise prompt", errors);
            ValidateJsonObject(exercise.AnswerKeyJson, "Exercise answerKey", errors);
            ValidateExerciseAnswerKey(exerciseType, exercise.AnswerKeyJson, errors);
            if (string.IsNullOrWhiteSpace(NormalizeText(exercise.CorrectExplanation))) errors.Add("Exercise correctExplanation is required.");
            if (string.IsNullOrWhiteSpace(NormalizeText(exercise.IncorrectExplanation))) errors.Add("Exercise incorrectExplanation is required.");
            foreach (string error in errors) issues.Add(new ImportIssueModel(null, "Error", $"exercises[{index + 1}]: {error}"));
        }

        HashSet<string> setSlugs = [];
        for (int index = 0; index < exerciseSets.Count; index++)
        {
            ParsedExerciseSetModel set = exerciseSets[index];
            List<string> errors = [];
            string slug = NormalizeText(set.Slug).ToLowerInvariant();
            if (!ValidateKebabKey(slug)) errors.Add("Exercise set slug is required and must use lowercase kebab-case.");
            else if (!setSlugs.Add(slug)) errors.Add($"Duplicate exercise set slug '{slug}' is not allowed inside one package.");
            if (string.IsNullOrWhiteSpace(NormalizeText(set.Title))) errors.Add("Exercise set title is required.");
            if (string.IsNullOrWhiteSpace(NormalizeText(set.Description))) errors.Add("Exercise set description is required.");
            if (!Enum.TryParse(NormalizeText(set.CefrLevel), true, out CefrLevel _)) errors.Add("Exercise set CEFR level is invalid.");
            string ownerType = NormalizeText(set.OwnerType).ToLowerInvariant();
            if (!ExerciseOwnerTypes.Contains(ownerType)) errors.Add($"Exercise set ownerType '{ownerType}' is not supported.");
            if (set.ExerciseSlugs.Count == 0) errors.Add("Exercise set exerciseSlugs must contain at least one exercise slug.");
            ValidateGrammarSlugList(set.ExerciseSlugs, "Exercise set exerciseSlugs", errors);
            foreach (string exerciseSlug in set.ExerciseSlugs.Select(item => NormalizeText(item).ToLowerInvariant()))
            {
                if (ValidateKebabKey(exerciseSlug) && !exerciseSlugs.Contains(exerciseSlug))
                {
                    errors.Add($"Exercise set references unknown exercise slug '{exerciseSlug}'.");
                }
            }
            foreach (string error in errors) issues.Add(new ImportIssueModel(null, "Error", $"exerciseSets[{index + 1}]: {error}"));
        }
    }

    private static void ValidateCourses(
        IReadOnlyList<ParsedCoursePathModel> coursePaths,
        IReadOnlyList<ParsedCourseModuleModel> courseModules,
        IReadOnlyList<ParsedCourseLessonModel> courseLessons,
        ICollection<ImportIssueModel> issues)
    {
        HashSet<string> courseSlugs = [];
        for (int index = 0; index < coursePaths.Count; index++)
        {
            ParsedCoursePathModel course = coursePaths[index];
            List<string> errors = [];
            string slug = NormalizeText(course.Slug).ToLowerInvariant();
            if (!ValidateKebabKey(slug)) errors.Add("Course slug is required and must use lowercase kebab-case.");
            else if (!courseSlugs.Add(slug)) errors.Add($"Duplicate course slug '{slug}' is not allowed inside one package.");
            if (string.IsNullOrWhiteSpace(NormalizeText(course.Title))) errors.Add("Course title is required.");
            if (string.IsNullOrWhiteSpace(NormalizeText(course.Description))) errors.Add("Course description is required.");
            string cefrLevel = NormalizeText(course.CefrLevel);
            string cefrRange = NormalizeText(course.CefrRange);
            if (string.IsNullOrWhiteSpace(cefrLevel) && string.IsNullOrWhiteSpace(cefrRange)) errors.Add("Course cefrLevel or cefrRange is required.");
            if (!string.IsNullOrWhiteSpace(cefrLevel) && !Enum.TryParse(cefrLevel, true, out CefrLevel _)) errors.Add("Course CEFR level is invalid.");
            foreach (string error in errors) issues.Add(new ImportIssueModel(null, "Error", $"coursePaths[{index + 1}]: {error}"));
        }

        HashSet<string> moduleSlugs = [];
        for (int index = 0; index < courseModules.Count; index++)
        {
            ParsedCourseModuleModel module = courseModules[index];
            List<string> errors = [];
            string slug = NormalizeText(module.Slug).ToLowerInvariant();
            string courseSlug = NormalizeText(module.CoursePathSlug).ToLowerInvariant();
            if (!ValidateKebabKey(slug)) errors.Add("Course module slug is required and must use lowercase kebab-case.");
            else if (!moduleSlugs.Add(slug)) errors.Add($"Duplicate course module slug '{slug}' is not allowed inside one package.");
            if (!ValidateKebabKey(courseSlug)) errors.Add("Course module coursePathSlug is required and must use lowercase kebab-case.");
            else if (!courseSlugs.Contains(courseSlug)) errors.Add($"Course module references unknown course path slug '{courseSlug}'.");
            if (string.IsNullOrWhiteSpace(NormalizeText(module.Title))) errors.Add("Course module title is required.");
            if (string.IsNullOrWhiteSpace(NormalizeText(module.Description))) errors.Add("Course module description is required.");
            if (module.ModuleNumber <= 0) errors.Add("Course module moduleNumber must be positive.");
            if (!Enum.TryParse(NormalizeText(module.CefrLevel), true, out CefrLevel _)) errors.Add("Course module CEFR level is invalid.");
            foreach (string error in errors) issues.Add(new ImportIssueModel(null, "Error", $"courseModules[{index + 1}]: {error}"));
        }

        HashSet<string> lessonSlugs = [];
        foreach (ParsedCourseLessonModel lesson in courseLessons)
        {
            string lessonSlug = NormalizeText(lesson.Slug).ToLowerInvariant();
            if (ValidateKebabKey(lessonSlug)) lessonSlugs.Add(lessonSlug);
        }

        HashSet<string> seenLessonSlugs = [];
        for (int index = 0; index < courseLessons.Count; index++)
        {
            ParsedCourseLessonModel lesson = courseLessons[index];
            List<string> errors = [];
            string slug = NormalizeText(lesson.Slug).ToLowerInvariant();
            string courseSlug = NormalizeText(lesson.CoursePathSlug).ToLowerInvariant();
            string moduleSlug = NormalizeText(lesson.ModuleSlug).ToLowerInvariant();
            if (!ValidateKebabKey(slug)) errors.Add("Course lesson slug is required and must use lowercase kebab-case.");
            else if (!seenLessonSlugs.Add(slug)) errors.Add($"Duplicate course lesson slug '{slug}' is not allowed inside one package.");
            if (!ValidateKebabKey(courseSlug)) errors.Add("Course lesson coursePathSlug is required and must use lowercase kebab-case.");
            else if (!courseSlugs.Contains(courseSlug)) errors.Add($"Course lesson references unknown course path slug '{courseSlug}'.");
            if (!ValidateKebabKey(moduleSlug)) errors.Add("Course lesson moduleSlug is required and must use lowercase kebab-case.");
            else if (!moduleSlugs.Contains(moduleSlug)) errors.Add($"Course lesson references unknown module slug '{moduleSlug}'.");
            if (lesson.LessonNumber <= 0) errors.Add("Course lesson lessonNumber must be positive.");
            if (string.IsNullOrWhiteSpace(NormalizeText(lesson.Title))) errors.Add("Course lesson title is required.");
            if (string.IsNullOrWhiteSpace(NormalizeText(lesson.ShortDescription))) errors.Add("Course lesson shortDescription is required.");
            if (string.IsNullOrWhiteSpace(NormalizeText(lesson.Narrative))) errors.Add("Course lesson narrative is required.");
            if (!Enum.TryParse(NormalizeText(lesson.CefrLevel), true, out CefrLevel _)) errors.Add("Course lesson CEFR level is invalid.");
            if (lesson.EstimatedMinutes <= 0) errors.Add("Course lesson estimatedMinutes must be positive.");
            ValidateLearningGoals(lesson.LearningGoals, errors);
            ValidateGrammarSlugList(lesson.PrerequisiteLessonSlugs, "Course lesson prerequisiteLessonSlugs", errors);
            ValidateGrammarSlugList(lesson.LinkedGrammarTopicSlugs, "Course lesson linkedGrammarTopicSlugs", errors);
            ValidateGrammarSlugList(lesson.LinkedWordSlugs, "Course lesson linkedWordSlugs", errors);
            ValidateGrammarSlugList(lesson.LinkedExpressionSlugs, "Course lesson linkedExpressionSlugs", errors);
            ValidateGrammarSlugList(lesson.LinkedDialogueSlugs, "Course lesson linkedDialogueSlugs", errors);
            ValidateGrammarSlugList(lesson.LinkedTalkTopicSlugs, "Course lesson linkedTalkTopicSlugs", errors);
            ValidateGrammarSlugList(lesson.LinkedExerciseSetSlugs, "Course lesson linkedExerciseSetSlugs", errors);
            ValidateGrammarSlugList(lesson.LinkedExamPrepSlugs, "Course lesson linkedExamPrepSlugs", errors);
            ValidateOptionalLessonSlug(lesson.NextLessonSlug, "Course lesson nextLessonSlug", errors);
            foreach (string prerequisiteSlug in lesson.PrerequisiteLessonSlugs.Select(item => NormalizeText(item).ToLowerInvariant()))
            {
                if (ValidateKebabKey(prerequisiteSlug) && !lessonSlugs.Contains(prerequisiteSlug)) errors.Add($"Course lesson references unknown prerequisite lesson slug '{prerequisiteSlug}'.");
                if (string.Equals(prerequisiteSlug, slug, StringComparison.Ordinal)) errors.Add("Course lesson cannot list itself as a prerequisite.");
            }
            string? nextLessonSlug = NormalizeOptionalText(lesson.NextLessonSlug)?.ToLowerInvariant();
            if (nextLessonSlug is not null && !lessonSlugs.Contains(nextLessonSlug)) errors.Add($"Course lesson references unknown next lesson slug '{nextLessonSlug}'.");
            if (string.Equals(nextLessonSlug, slug, StringComparison.Ordinal)) errors.Add("Course lesson cannot list itself as next lesson.");
            foreach (string error in errors) issues.Add(new ImportIssueModel(null, "Error", $"courseLessons[{index + 1}]: {error}"));
        }
    }

    private static void ValidateLearningGoals(IReadOnlyList<string> learningGoals, ICollection<string> errors)
    {
        if (learningGoals.Count == 0)
        {
            errors.Add("Course lesson learningGoals must contain at least one item.");
            return;
        }

        HashSet<string> seen = new(StringComparer.Ordinal);
        foreach (string goal in learningGoals)
        {
            string normalizedGoal = NormalizeText(goal);
            if (string.IsNullOrWhiteSpace(normalizedGoal))
            {
                errors.Add("Course lesson learningGoals cannot contain empty items.");
                continue;
            }

            if (!seen.Add(normalizedGoal))
            {
                errors.Add($"Course lesson learningGoals contains duplicate item '{normalizedGoal}'.");
            }
        }
    }

    private static void ValidateWritingTemplates(
        IReadOnlyList<ParsedWritingTemplateModel> writingTemplates,
        ICollection<ImportIssueModel> issues)
    {
        HashSet<string> templateSlugs = [];
        for (int index = 0; index < writingTemplates.Count; index++)
        {
            ParsedWritingTemplateModel template = writingTemplates[index];
            List<string> errors = [];
            string slug = NormalizeText(template.Slug).ToLowerInvariant();
            if (!ValidateKebabKey(slug)) errors.Add("Writing template slug is required and must use lowercase kebab-case.");
            else if (!templateSlugs.Add(slug)) errors.Add($"Duplicate writing template slug '{slug}' is not allowed inside one package.");
            if (string.IsNullOrWhiteSpace(NormalizeText(template.Title))) errors.Add("Writing template title is required.");
            if (string.IsNullOrWhiteSpace(NormalizeText(template.ShortDescription))) errors.Add("Writing template shortDescription is required.");
            if (!Enum.TryParse(NormalizeText(template.CefrLevel), true, out CefrLevel _)) errors.Add("Writing template CEFR level is invalid.");
            string category = NormalizeText(template.Category).ToLowerInvariant();
            if (!WritingTemplateCategories.Contains(category)) errors.Add($"Writing template category '{category}' is not supported.");
            if (string.IsNullOrWhiteSpace(NormalizeText(template.Situation))) errors.Add("Writing template situation is required.");
            string register = NormalizeText(template.Register).ToLowerInvariant();
            if (!WritingTemplateRegisters.Contains(register)) errors.Add($"Writing template register '{register}' is not supported.");
            string templateText = NormalizeText(template.TemplateText);
            if (string.IsNullOrWhiteSpace(templateText)) errors.Add("Writing template templateText is required.");
            if (string.IsNullOrWhiteSpace(NormalizeText(template.Explanation))) errors.Add("Writing template explanation is required.");
            ValidateTemplateVariables(template.ReplaceableVariables, templateText, errors);
            if (string.IsNullOrWhiteSpace(NormalizeText(template.SampleFilledVersion))) errors.Add("Writing template sampleFilledVersion is required.");
            ValidateGrammarSlugList(template.LinkedGrammarTopicSlugs, "Writing template linkedGrammarTopicSlugs", errors);
            ValidateGrammarSlugList(template.LinkedWordSlugs, "Writing template linkedWordSlugs", errors);
            ValidateGrammarSlugList(template.LinkedExpressionSlugs, "Writing template linkedExpressionSlugs", errors);
            ValidateGrammarSlugList(template.LinkedExerciseSlugs, "Writing template linkedExerciseSlugs", errors);
            foreach (string error in errors) issues.Add(new ImportIssueModel(null, "Error", $"writingTemplates[{index + 1}]: {error}"));
        }
    }

    private static void ValidateTemplateVariables(IReadOnlyList<string> variables, string templateText, ICollection<string> errors)
    {
        HashSet<string> seen = new(StringComparer.Ordinal);
        foreach (string variable in variables)
        {
            string normalized = NormalizeText(variable);
            if (!ValidateKebabKey(normalized.ToLowerInvariant()))
            {
                errors.Add($"Writing template variable '{normalized}' must use lowercase kebab-case.");
                continue;
            }

            if (!seen.Add(normalized))
            {
                errors.Add($"Writing template variables contains duplicate item '{normalized}'.");
                continue;
            }

            if (!templateText.Contains($"{{{{{normalized}}}}}", StringComparison.Ordinal))
            {
                errors.Add($"Writing template variable '{normalized}' is not used in templateText.");
            }
        }
    }

    private static void ValidateCulturalNotes(
        IReadOnlyList<ParsedCulturalNoteModel> culturalNotes,
        ICollection<ImportIssueModel> issues)
    {
        HashSet<string> noteSlugs = [];
        for (int index = 0; index < culturalNotes.Count; index++)
        {
            ParsedCulturalNoteModel note = culturalNotes[index];
            List<string> errors = [];
            string slug = NormalizeText(note.Slug).ToLowerInvariant();
            if (!ValidateKebabKey(slug)) errors.Add("Cultural note slug is required and must use lowercase kebab-case.");
            else if (!noteSlugs.Add(slug)) errors.Add($"Duplicate cultural note slug '{slug}' is not allowed inside one package.");
            if (string.IsNullOrWhiteSpace(NormalizeText(note.Title))) errors.Add("Cultural note title is required.");
            if (string.IsNullOrWhiteSpace(NormalizeText(note.ShortDescription))) errors.Add("Cultural note shortDescription is required.");
            if (!Enum.TryParse(NormalizeText(note.CefrLevel), true, out CefrLevel _)) errors.Add("Cultural note CEFR level is invalid.");
            string category = NormalizeText(note.Category).ToLowerInvariant();
            if (!CulturalNoteCategories.Contains(category)) errors.Add($"Cultural note category '{category}' is not supported.");
            if (string.IsNullOrWhiteSpace(NormalizeText(note.Context))) errors.Add("Cultural note context is required.");
            ValidateRequiredTextList(note.Sections, "Cultural note sections", errors);
            ValidateCulturalNoteExamples(note.Examples, errors);
            ValidateOptionalTextList(note.DoNotes, "Cultural note doNotes", errors);
            ValidateOptionalTextList(note.DontNotes, "Cultural note dontNotes", errors);
            ValidateGrammarSlugList(note.LinkedDialogueSlugs, "Cultural note linkedDialogueSlugs", errors);
            ValidateGrammarSlugList(note.LinkedExpressionSlugs, "Cultural note linkedExpressionSlugs", errors);
            ValidateGrammarSlugList(note.LinkedWritingTemplateSlugs, "Cultural note linkedWritingTemplateSlugs", errors);
            ValidateGrammarSlugList(note.LinkedTalkTopicSlugs, "Cultural note linkedTalkTopicSlugs", errors);
            ValidateGrammarSlugList(note.LinkedCourseLessonSlugs, "Cultural note linkedCourseLessonSlugs", errors);
            foreach (string error in errors) issues.Add(new ImportIssueModel(null, "Error", $"culturalNotes[{index + 1}]: {error}"));
        }
    }

    private static void ValidateCulturalNoteExamples(IReadOnlyList<ParsedCulturalNoteExampleModel> examples, ICollection<string> errors)
    {
        for (int index = 0; index < examples.Count; index++)
        {
            ParsedCulturalNoteExampleModel example = examples[index];
            if (string.IsNullOrWhiteSpace(NormalizeText(example.GermanText)))
            {
                errors.Add($"Cultural note examples[{index + 1}] germanText is required.");
            }
        }
    }

    private static void ValidateRequiredTextList(IReadOnlyList<string> values, string fieldName, ICollection<string> errors)
    {
        if (values.Count == 0)
        {
            errors.Add($"{fieldName} must contain at least one item.");
            return;
        }

        ValidateOptionalTextList(values, fieldName, errors);
    }

    private static void ValidateOptionalTextList(IReadOnlyList<string> values, string fieldName, ICollection<string> errors)
    {
        for (int index = 0; index < values.Count; index++)
        {
            if (string.IsNullOrWhiteSpace(NormalizeText(values[index])))
            {
                errors.Add($"{fieldName}[{index + 1}] cannot be empty.");
            }
        }
    }

    private static void ValidateExamPrep(
        IReadOnlyList<ParsedExamProfileModel> examProfiles,
        IReadOnlyList<ParsedExamPrepUnitModel> examPrepUnits,
        ICollection<ImportIssueModel> issues)
    {
        HashSet<string> profilesInPackage = [];
        for (int index = 0; index < examProfiles.Count; index++)
        {
            ParsedExamProfileModel profile = examProfiles[index];
            List<string> errors = [];
            string key = NormalizeText(profile.Key).ToLowerInvariant();
            if (!ExamPrepProfiles.Contains(key)) errors.Add($"Exam profile key '{key}' is not supported.");
            else if (!profilesInPackage.Add(key)) errors.Add($"Duplicate exam profile key '{key}' is not allowed inside one package.");
            if (string.IsNullOrWhiteSpace(NormalizeText(profile.DisplayName))) errors.Add("Exam profile displayName is required.");
            if (string.IsNullOrWhiteSpace(NormalizeText(profile.CefrRange))) errors.Add("Exam profile cefrRange is required.");
            if (string.IsNullOrWhiteSpace(NormalizeText(profile.Description))) errors.Add("Exam profile description is required.");
            foreach (string error in errors) issues.Add(new ImportIssueModel(null, "Error", $"examProfiles[{index + 1}]: {error}"));
        }

        HashSet<string> unitSlugs = [];
        HashSet<string> validProfiles = new(ExamPrepProfiles, StringComparer.Ordinal);
        foreach (string packageProfile in profilesInPackage) validProfiles.Add(packageProfile);
        for (int index = 0; index < examPrepUnits.Count; index++)
        {
            ParsedExamPrepUnitModel unit = examPrepUnits[index];
            List<string> errors = [];
            string slug = NormalizeText(unit.Slug).ToLowerInvariant();
            if (!ValidateKebabKey(slug)) errors.Add("Exam prep unit slug is required and must use lowercase kebab-case.");
            else if (!unitSlugs.Add(slug)) errors.Add($"Duplicate exam prep unit slug '{slug}' is not allowed inside one package.");
            string profileKey = NormalizeText(unit.ExamProfileKey).ToLowerInvariant();
            if (!validProfiles.Contains(profileKey)) errors.Add($"Exam prep unit examProfileKey '{profileKey}' is not supported.");
            if (string.IsNullOrWhiteSpace(NormalizeText(unit.Title))) errors.Add("Exam prep unit title is required.");
            if (string.IsNullOrWhiteSpace(NormalizeText(unit.ShortDescription))) errors.Add("Exam prep unit shortDescription is required.");
            if (!Enum.TryParse(NormalizeText(unit.CefrLevel), true, out CefrLevel _)) errors.Add("Exam prep unit CEFR level is invalid.");
            string section = NormalizeText(unit.ExamSection).ToLowerInvariant();
            if (!ExamPrepSections.Contains(section)) errors.Add($"Exam prep unit examSection '{section}' is not supported.");
            string taskType = NormalizeText(unit.TaskType).ToLowerInvariant();
            if (!ExamPrepTaskTypes.Contains(taskType)) errors.Add($"Exam prep unit taskType '{taskType}' is not supported.");
            string skillFocus = NormalizeText(unit.SkillFocus).ToLowerInvariant();
            if (!ExerciseTargetSkills.Contains(skillFocus)) errors.Add($"Exam prep unit skillFocus '{skillFocus}' is not supported.");
            if (string.IsNullOrWhiteSpace(NormalizeText(unit.Explanation))) errors.Add("Exam prep unit explanation is required.");
            ValidateOptionalTextList(unit.StrategyNotes, "Exam prep unit strategyNotes", errors);
            ValidateOptionalTextList(unit.Checklist, "Exam prep unit checklist", errors);
            ValidateGrammarSlugList(unit.LinkedDialogueSlugs, "Exam prep unit linkedDialogueSlugs", errors);
            ValidateGrammarSlugList(unit.LinkedTalkTopicSlugs, "Exam prep unit linkedTalkTopicSlugs", errors);
            ValidateGrammarSlugList(unit.LinkedGrammarTopicSlugs, "Exam prep unit linkedGrammarTopicSlugs", errors);
            ValidateGrammarSlugList(unit.LinkedExpressionSlugs, "Exam prep unit linkedExpressionSlugs", errors);
            ValidateGrammarSlugList(unit.LinkedWritingTemplateSlugs, "Exam prep unit linkedWritingTemplateSlugs", errors);
            ValidateGrammarSlugList(unit.LinkedExerciseSlugs, "Exam prep unit linkedExerciseSlugs", errors);
            ValidateGrammarSlugList(unit.LinkedCourseLessonSlugs, "Exam prep unit linkedCourseLessonSlugs", errors);
            foreach (string error in errors) issues.Add(new ImportIssueModel(null, "Error", $"examPrepUnits[{index + 1}]: {error}"));
        }
    }

    private static void ValidateOptionalLessonSlug(string? slug, string fieldName, ICollection<string> errors)
    {
        string? normalized = NormalizeOptionalText(slug)?.ToLowerInvariant();
        if (normalized is not null && !ValidateKebabKey(normalized))
        {
            errors.Add($"{fieldName} must use lowercase kebab-case.");
        }
    }

    private static void ValidateExerciseAnswerKey(string exerciseType, string answerKeyJson, ICollection<string> errors)
    {
        try
        {
            using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(answerKeyJson);
            System.Text.Json.JsonElement root = document.RootElement;
            switch (exerciseType)
            {
                case "multiple-choice":
                case "article-selection":
                case "case-selection":
                case "vocabulary-choice":
                case "grammar-choice":
                case "dialogue-completion":
                    RequireJsonArray(root, "correctOptionIds", "Exercise answerKey correctOptionIds is required.", errors);
                    break;
                case "fill-in-the-blank":
                case "conjugation":
                case "translation-controlled":
                    RequireJsonArray(root, "acceptedAnswers", "Exercise answerKey acceptedAnswers is required.", errors);
                    break;
                case "matching":
                    RequireJsonArray(root, "pairs", "Exercise answerKey pairs is required.", errors);
                    break;
                case "sentence-ordering":
                    RequireJsonArray(root, "orderedSegments", "Exercise answerKey orderedSegments is required.", errors);
                    break;
                case "error-correction":
                    if (!root.TryGetProperty("correctedText", out System.Text.Json.JsonElement correctedText) || correctedText.ValueKind != System.Text.Json.JsonValueKind.String || string.IsNullOrWhiteSpace(correctedText.GetString()))
                    {
                        errors.Add("Exercise answerKey correctedText is required.");
                    }
                    break;
            }
        }
        catch (System.Text.Json.JsonException exception)
        {
            errors.Add($"Exercise answerKey is not valid JSON: {exception.Message}");
        }
    }

    private static void ValidateJsonObject(string json, string fieldName, ICollection<string> errors)
    {
        try
        {
            using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != System.Text.Json.JsonValueKind.Object)
            {
                errors.Add($"{fieldName} must be a JSON object.");
            }
        }
        catch (System.Text.Json.JsonException exception)
        {
            errors.Add($"{fieldName} is not valid JSON: {exception.Message}");
        }
    }

    private static void RequireJsonArray(System.Text.Json.JsonElement root, string propertyName, string message, ICollection<string> errors)
    {
        if (!root.TryGetProperty(propertyName, out System.Text.Json.JsonElement value) ||
            value.ValueKind != System.Text.Json.JsonValueKind.Array ||
            value.GetArrayLength() == 0)
        {
            errors.Add(message);
        }
    }

    private static void ValidateGrammarTextItems(
        IReadOnlyList<ParsedGrammarTextItemModel> items,
        IReadOnlySet<LanguageCode> meaningLanguages,
        string fieldName,
        ICollection<string> errors)
    {
        for (int index = 0; index < items.Count; index++)
        {
            ParsedGrammarTextItemModel item = items[index];
            if (string.IsNullOrWhiteSpace(NormalizeText(item.Text)))
            {
                errors.Add($"{fieldName}[{index + 1}] text is required.");
            }

            ValidateOptionalMeaningTranslations(item.Translations, meaningLanguages, $"{fieldName}[{index + 1}].translations", errors);
        }
    }

    private static void ValidateGrammarSlugList(IReadOnlyList<string> slugs, string fieldName, ICollection<string> errors)
    {
        HashSet<string> seen = new(StringComparer.Ordinal);
        foreach (string value in slugs)
        {
            string slug = NormalizeText(value).ToLowerInvariant();
            if (!ValidateKebabKey(slug))
            {
                errors.Add($"{fieldName} contains invalid slug '{slug}'.");
                continue;
            }

            if (!seen.Add(slug))
            {
                errors.Add($"{fieldName} contains duplicate slug '{slug}'.");
            }
        }
    }

    private static string SerializeStringArray(IReadOnlyList<string> values) =>
        System.Text.Json.JsonSerializer.Serialize(values.Select(NormalizeText).Where(value => !string.IsNullOrWhiteSpace(value)).ToArray());

    private static string SerializeCulturalNoteExamples(IReadOnlyList<ParsedCulturalNoteExampleModel> examples) =>
        System.Text.Json.JsonSerializer.Serialize(examples
            .Where(example => !string.IsNullOrWhiteSpace(NormalizeText(example.GermanText)))
            .Select(example => new { GermanText = NormalizeText(example.GermanText), Explanation = NormalizeOptionalText(example.Explanation) })
            .ToArray());

    private static string SerializeSlugArray(IReadOnlyList<string> values) =>
        System.Text.Json.JsonSerializer.Serialize(values.Select(value => NormalizeText(value).ToLowerInvariant()).Where(ValidateKebabKey).ToArray());

    private static void ValidateTalkTopicQuestions(
        IReadOnlyList<ParsedTalkTopicQuestionModel> questions,
        IReadOnlySet<LanguageCode> meaningLanguages,
        string fieldName,
        ICollection<string> errors)
    {
        for (int questionIndex = 0; questionIndex < questions.Count; questionIndex++)
        {
            ParsedTalkTopicQuestionModel question = questions[questionIndex];
            if (string.IsNullOrWhiteSpace(NormalizeText(question.Prompt)))
            {
                errors.Add($"{fieldName}[{questionIndex + 1}] prompt is required.");
            }

            if (question.Translations.Count > 0)
            {
                errors.Add($"{fieldName}[{questionIndex + 1}].translations is not supported; Talk Topic questions are German-only for now.");
            }
        }
    }

    private static void ValidateLocalizedTextDictionary(
        IReadOnlyDictionary<string, string> values,
        IReadOnlySet<LanguageCode> meaningLanguages,
        string fieldName,
        ICollection<string> errors)
    {
        foreach ((string language, string text) in values)
        {
            if (string.IsNullOrWhiteSpace(NormalizeText(text)))
            {
                errors.Add($"{fieldName} contains an empty value for language '{language}'.");
                continue;
            }

            ValidateMeaningLanguage(language, meaningLanguages, fieldName, errors);
        }
    }

    private static void ValidateGrammarLocalizedBlocks(
        IReadOnlyDictionary<string, string> blocksByLanguage,
        IReadOnlySet<LanguageCode> meaningLanguages,
        string fieldName,
        ICollection<string> errors)
    {
        foreach ((string language, string rawBlocks) in blocksByLanguage)
        {
            if (!ValidateMeaningLanguage(language, meaningLanguages, fieldName, errors))
            {
                continue;
            }

            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(rawBlocks);
            }
            catch (JsonException exception)
            {
                errors.Add($"{fieldName}.{language} is not valid JSON: {exception.Message}");
                continue;
            }

            using (document)
            {
                if (document.RootElement.ValueKind != JsonValueKind.Array)
                {
                    errors.Add($"{fieldName}.{language} must be an array of blocks.");
                    continue;
                }

                int blockIndex = 0;
                foreach (JsonElement block in document.RootElement.EnumerateArray())
                {
                    blockIndex++;
                    ValidateGrammarBlock(block, $"{fieldName}.{language}[{blockIndex}]", errors);
                }
            }
        }
    }

    private static void ValidateGrammarBlock(JsonElement block, string fieldName, ICollection<string> errors)
    {
        if (block.ValueKind != JsonValueKind.Object)
        {
            errors.Add($"{fieldName} must be an object.");
            return;
        }

        string type = ReadStringProperty(block, "type");
        if (!GrammarRichBlockTypes.Contains(type))
        {
            errors.Add($"{fieldName} has unsupported block type '{type}'.");
            return;
        }

        switch (type)
        {
            case "paragraph":
                RequireBlockString(block, "text", fieldName, errors);
                break;
            case "table":
                RequireBlockString(block, "caption", fieldName, errors);
                RequireNonEmptyArray(block, "columns", fieldName, errors);
                RequireNonEmptyArray(block, "rows", fieldName, errors);
                break;
            case "callout":
                RequireBlockString(block, "style", fieldName, errors);
                RequireBlockString(block, "text", fieldName, errors);
                break;
            case "rule-list":
            case "example-list":
                RequireNonEmptyArray(block, "items", fieldName, errors);
                break;
            case "mistake-pair":
                RequireBlockString(block, "wrong", fieldName, errors);
                RequireBlockString(block, "correct", fieldName, errors);
                break;
            case "image-slot":
                if (string.IsNullOrWhiteSpace(ReadStringProperty(block, "assetKey")) &&
                    string.IsNullOrWhiteSpace(ReadStringProperty(block, "imageSlotKey")))
                {
                    errors.Add($"{fieldName} requires assetKey or imageSlotKey.");
                }
                break;
        }
    }

    private static string ReadStringProperty(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.String
            ? NormalizeText(property.GetString())
            : string.Empty;

    private static void RequireBlockString(JsonElement block, string propertyName, string fieldName, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(ReadStringProperty(block, propertyName)))
        {
            errors.Add($"{fieldName}.{propertyName} is required.");
        }
    }

    private static void RequireNonEmptyArray(JsonElement block, string propertyName, string fieldName, ICollection<string> errors)
    {
        if (!block.TryGetProperty(propertyName, out JsonElement property) ||
            property.ValueKind != JsonValueKind.Array ||
            property.GetArrayLength() == 0)
        {
            errors.Add($"{fieldName}.{propertyName} must be a non-empty array.");
        }
    }

    private static bool ValidateMeaningLanguage(
        string language,
        IReadOnlySet<LanguageCode> meaningLanguages,
        string fieldName,
        ICollection<string> errors)
    {
        string normalizedLanguage = NormalizeText(language).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedLanguage))
        {
            errors.Add($"{fieldName} contains an empty language code.");
            return false;
        }

        try
        {
            LanguageCode languageCode = LanguageCode.From(normalizedLanguage);
            if (!meaningLanguages.Contains(languageCode))
            {
                errors.Add($"{fieldName} language '{normalizedLanguage}' is not an active meaning language.");
                return false;
            }
        }
        catch (DomainRuleException exception)
        {
            errors.Add($"{fieldName} language is invalid: {exception.Message}");
            return false;
        }

        return true;
    }

    private static void ValidateOptionalMeaningTranslations(
        IReadOnlyList<ParsedContentMeaningModel> translations,
        IReadOnlySet<LanguageCode> meaningLanguages,
        string fieldName,
        ICollection<string> errors)
    {
        HashSet<LanguageCode> seenLanguages = [];
        for (int index = 0; index < translations.Count; index++)
        {
            ParsedContentMeaningModel translation = translations[index];
            string language = NormalizeText(translation.Language).ToLowerInvariant();
            string text = NormalizeText(translation.Text);

            if (string.IsNullOrWhiteSpace(language) || string.IsNullOrWhiteSpace(text))
            {
                errors.Add($"{fieldName}[{index + 1}] language and text are required when translations are provided.");
                continue;
            }

            LanguageCode languageCode;
            try
            {
                languageCode = LanguageCode.From(language);
            }
            catch (DomainRuleException exception)
            {
                errors.Add($"{fieldName}[{index + 1}] language is invalid: {exception.Message}");
                continue;
            }

            if (!meaningLanguages.Contains(languageCode))
            {
                errors.Add($"{fieldName}[{index + 1}] language '{language}' is not an active meaning language.");
                continue;
            }

            if (!seenLanguages.Add(languageCode))
            {
                errors.Add($"{fieldName} contains duplicate language '{language}'.");
            }
        }
    }

    private static List<(string GermanText, Dictionary<LanguageCode, string> Translations)> ValidateExamples(
        IReadOnlyList<ParsedContentExampleModel> examples,
        IReadOnlySet<LanguageCode> meaningLanguages,
        ICollection<string> entryErrors)
    {
        List<(string GermanText, Dictionary<LanguageCode, string> Translations)> normalizedExamples = [];

        foreach (ParsedContentExampleModel example in examples)
        {
            string normalizedBaseText = NormalizeText(example.BaseText);

            if (string.IsNullOrWhiteSpace(normalizedBaseText))
            {
                entryErrors.Add("Example baseText is required.");
                continue;
            }

            Dictionary<LanguageCode, string> translations = ValidateMeaningTranslations(
                example.Translations,
                meaningLanguages,
                entryErrors);

            if (translations.Count == 0)
            {
                entryErrors.Add($"Example '{normalizedBaseText}' must contain at least one valid translation.");
                continue;
            }

            normalizedExamples.Add((normalizedBaseText, translations));
        }

        return normalizedExamples;
    }

    private static ContentSourceType ResolveSourceType(string? source)
    {
        string normalizedSource = NormalizeText(source).Replace(" ", string.Empty, StringComparison.Ordinal).ToLowerInvariant();

        return normalizedSource switch
        {
            "manual" => ContentSourceType.Manual,
            "aiassisted" => ContentSourceType.AiAssisted,
            "hybrid" => ContentSourceType.Hybrid,
            "externalcurated" => ContentSourceType.ExternalCurated,
            _ => ContentSourceType.Hybrid,
        };
    }

    private static TalkTopicContentType ParseTalkTopicContentType(string value) =>
        TryParseTalkTopicContentType(value, out TalkTopicContentType contentType)
            ? contentType
            : throw new InvalidOperationException($"Unsupported talk topic content type '{value}'.");

    private static bool TryParseTalkTopicContentType(string value, out TalkTopicContentType contentType)
    {
        contentType = value.Trim().ToLowerInvariant() switch
        {
            "article" => TalkTopicContentType.Article,
            "book-summary" => TalkTopicContentType.BookSummary,
            "movie-summary" => TalkTopicContentType.MovieSummary,
            "story" => TalkTopicContentType.Story,
            "fact-sheet" => TalkTopicContentType.FactSheet,
            "opinion-text" => TalkTopicContentType.OpinionText,
            "interview" => TalkTopicContentType.Interview,
            "debate-text" => TalkTopicContentType.DebateText,
            _ => default,
        };

        return contentType != default;
    }

    private static TalkTopicQuestionType ParseTalkTopicQuestionType(string value) =>
        TryParseTalkTopicQuestionType(value, out TalkTopicQuestionType questionType)
            ? questionType
            : throw new InvalidOperationException($"Unsupported talk topic question type '{value}'.");

    private static bool TryParseTalkTopicQuestionType(string value, out TalkTopicQuestionType questionType)
    {
        questionType = value.Trim().ToLowerInvariant() switch
        {
            "opinion" => TalkTopicQuestionType.Opinion,
            "imagination" => TalkTopicQuestionType.Imagination,
            "prediction" => TalkTopicQuestionType.Prediction,
            "comparison" => TalkTopicQuestionType.Comparison,
            _ => default,
        };

        return questionType != default;
    }

    private static TalkTopicSpeakingGoal ParseTalkTopicSpeakingGoal(string value) =>
        TryParseTalkTopicSpeakingGoal(value, out TalkTopicSpeakingGoal speakingGoal)
            ? speakingGoal
            : throw new InvalidOperationException($"Unsupported talk topic speaking goal '{value}'.");

    private static bool TryParseTalkTopicSpeakingGoal(string value, out TalkTopicSpeakingGoal speakingGoal)
    {
        speakingGoal = value.Trim().ToLowerInvariant() switch
        {
            "express-opinion" => TalkTopicSpeakingGoal.ExpressOpinion,
            "give-reasons" => TalkTopicSpeakingGoal.GiveReasons,
            "agree-disagree" => TalkTopicSpeakingGoal.AgreeDisagree,
            "ask-follow-up-questions" => TalkTopicSpeakingGoal.AskFollowUpQuestions,
            "compare-options" => TalkTopicSpeakingGoal.CompareOptions,
            "make-predictions" => TalkTopicSpeakingGoal.MakePredictions,
            "describe-experiences" => TalkTopicSpeakingGoal.DescribeExperiences,
            "imagine-possibilities" => TalkTopicSpeakingGoal.ImaginePossibilities,
            "debate-politely" => TalkTopicSpeakingGoal.DebatePolitely,
            "summarize-position" => TalkTopicSpeakingGoal.SummarizePosition,
            _ => default,
        };

        return speakingGoal != default;
    }

    private static (int Minimum, int Maximum) GetTalkTopicArticleLengthRange(CefrLevel cefrLevel) =>
        cefrLevel switch
        {
            CefrLevel.A1 => (900, 1100),
            CefrLevel.A2 => (1400, 1600),
            CefrLevel.B1 => (1900, 2100),
            CefrLevel.B2 => (2400, 2600),
            CefrLevel.C1 => (2900, 3100),
            CefrLevel.C2 => (3400, 3600),
            _ => (1900, 2100),
        };

    private static (int Minimum, int Maximum) GetTalkTopicVocabularyCountRange(CefrLevel cefrLevel) =>
        cefrLevel switch
        {
            CefrLevel.A1 => (12, 18),
            CefrLevel.A2 => (15, 22),
            CefrLevel.B1 => (18, 26),
            CefrLevel.B2 => (22, 32),
            CefrLevel.C1 => (26, 38),
            CefrLevel.C2 => (30, 45),
            _ => (18, 26),
        };

    private static bool IsUpperIntermediateOrHigher(CefrLevel cefrLevel) =>
        cefrLevel is CefrLevel.B2 or CefrLevel.C1 or CefrLevel.C2;

    private static NormalizedLexicalForm[] ValidateLexicalForms(
        ParsedContentEntryModel entry,
        ICollection<string> entryErrors)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(entryErrors);

        List<NormalizedLexicalForm> normalizedForms = [];

        if (entry.LexicalForms.Count == 0)
        {
            string normalizedPartOfSpeechText = NormalizeText(entry.PartOfSpeech);

            if (!Enum.TryParse(normalizedPartOfSpeechText, true, out PartOfSpeech legacyPartOfSpeech))
            {
                entryErrors.Add("Entry part of speech is invalid.");
                return [];
            }

            return
            [
                new NormalizedLexicalForm(
                    legacyPartOfSpeech,
                    NormalizeOptionalText(entry.Article),
                    NormalizeOptionalText(entry.Plural),
                    NormalizeOptionalText(entry.Infinitive),
                    true,
                    1),
            ];
        }

        int explicitPrimaryCount = entry.LexicalForms.Count(form => form.IsPrimary);
        if (explicitPrimaryCount > 1)
        {
            entryErrors.Add("Entry lexicalForms can contain at most one primary item.");
        }

        for (int index = 0; index < entry.LexicalForms.Count; index++)
        {
            ParsedContentLexicalFormModel form = entry.LexicalForms[index];
            string normalizedPartOfSpeechText = NormalizeText(form.PartOfSpeech);

            if (!Enum.TryParse(normalizedPartOfSpeechText, true, out PartOfSpeech partOfSpeech))
            {
                entryErrors.Add($"Entry lexicalForms[{index + 1}] partOfSpeech is invalid.");
                continue;
            }

            if (normalizedForms.Any(existingForm => existingForm.PartOfSpeech == partOfSpeech))
            {
                entryErrors.Add($"Entry lexicalForms contains duplicate partOfSpeech '{normalizedPartOfSpeechText}'.");
                continue;
            }

            bool isPrimary = explicitPrimaryCount == 0 ? index == 0 : form.IsPrimary;

            normalizedForms.Add(new NormalizedLexicalForm(
                partOfSpeech,
                NormalizeOptionalText(form.Article),
                NormalizeOptionalText(form.Plural),
                NormalizeOptionalText(form.Infinitive),
                isPrimary,
                index + 1));
        }

        if (normalizedForms.Count == 0)
        {
            entryErrors.Add("Entry lexicalForms must contain at least one valid item.");
            return [];
        }

        NormalizedLexicalForm primaryForm = normalizedForms
            .OrderByDescending(form => form.IsPrimary)
            .ThenBy(form => form.SortOrder)
            .First();

        if (!string.IsNullOrWhiteSpace(entry.PartOfSpeech) &&
            !string.Equals(primaryForm.PartOfSpeech.ToString(), NormalizeText(entry.PartOfSpeech), StringComparison.OrdinalIgnoreCase))
        {
            entryErrors.Add("Entry partOfSpeech must match the primary lexical form when lexicalForms is provided.");
        }

        if (!OptionalTextEquals(entry.Article, primaryForm.Article))
        {
            entryErrors.Add("Entry article must match the primary lexical form when lexicalForms is provided.");
        }

        if (!OptionalTextEquals(entry.Plural, primaryForm.PluralForm))
        {
            entryErrors.Add("Entry plural must match the primary lexical form when lexicalForms is provided.");
        }

        if (!OptionalTextEquals(entry.Infinitive, primaryForm.InfinitiveForm))
        {
            entryErrors.Add("Entry infinitive must match the primary lexical form when lexicalForms is provided.");
        }

        return normalizedForms.ToArray();
    }

    private static bool OptionalTextEquals(string? left, string? right)
    {
        return string.Equals(NormalizeOptionalText(left), NormalizeOptionalText(right), StringComparison.Ordinal);
    }

    private static string NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private static string NormalizeCollectionSlug(string? value, ICollection<string> errors)
    {
        string normalized = NormalizeText(value).ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            errors.Add("Collection slug is required.");
            return string.Empty;
        }

        bool isValid = normalized.All(character =>
            (character >= 'a' && character <= 'z') ||
            (character >= '0' && character <= '9') ||
            character == '-');

        if (!isValid || normalized.StartsWith("-", StringComparison.Ordinal) || normalized.EndsWith("-", StringComparison.Ordinal))
        {
            errors.Add("Collection slug must use lowercase kebab-case.");
        }

        return normalized;
    }

    private static string[] ValidateLabelKeys(
        IReadOnlyList<string> labels,
        WordLabelKind kind,
        string fieldName,
        IReadOnlyDictionary<WordLabelKind, HashSet<string>> allowedLabelsByKind,
        ICollection<string> entryErrors)
    {
        List<string> normalized = [];

        foreach (string? label in labels)
        {
            string normalizedLabel = NormalizeText(label).ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(normalizedLabel))
            {
                entryErrors.Add($"Entry {fieldName} cannot contain empty items.");
                continue;
            }

            if (normalizedLabel.Length > 64)
            {
                entryErrors.Add($"Entry {fieldName} items must not exceed 64 characters.");
                continue;
            }

            bool isValid = normalizedLabel.All(character =>
                (character >= 'a' && character <= 'z') ||
                (character >= '0' && character <= '9') ||
                character == '-');

            if (!isValid || normalizedLabel.StartsWith("-", StringComparison.Ordinal) || normalizedLabel.EndsWith("-", StringComparison.Ordinal))
            {
                entryErrors.Add($"Entry {fieldName} items must use lowercase kebab-case keys.");
                continue;
            }

            if (normalized.Contains(normalizedLabel, StringComparer.Ordinal))
            {
                entryErrors.Add($"Duplicate {fieldName} item '{normalizedLabel}' is not allowed.");
                continue;
            }

            if (!allowedLabelsByKind.TryGetValue(kind, out HashSet<string>? allowedLabels) ||
                !allowedLabels.Contains(normalizedLabel))
            {
                entryErrors.Add($"Entry {fieldName} item '{normalizedLabel}' must be defined in the package labels taxonomy with complete localizations.");
                continue;
            }

            normalized.Add(normalizedLabel);
        }

        return normalized.ToArray();
    }

    private static string[] ValidateGrammarNotes(
        IReadOnlyList<string> grammarNotes,
        ICollection<string> entryErrors)
    {
        List<string> normalized = [];

        foreach (string? grammarNote in grammarNotes)
        {
            string normalizedGrammarNote = NormalizeText(grammarNote);

            if (string.IsNullOrWhiteSpace(normalizedGrammarNote))
            {
                entryErrors.Add("Entry grammarNotes cannot contain empty items.");
                continue;
            }

            if (normalizedGrammarNote.Length > 512)
            {
                entryErrors.Add("Entry grammarNotes items must not exceed 512 characters.");
                continue;
            }

            if (normalized.Contains(normalizedGrammarNote, StringComparer.Ordinal))
            {
                entryErrors.Add($"Duplicate grammarNotes item '{normalizedGrammarNote}' is not allowed.");
                continue;
            }

            normalized.Add(normalizedGrammarNote);
        }

        return normalized.ToArray();
    }

    private static ParsedContentCollocationModel[] ValidateCollocations(
        IReadOnlyList<ParsedContentCollocationModel> collocations,
        ICollection<string> entryErrors)
    {
        List<ParsedContentCollocationModel> normalized = [];

        foreach (ParsedContentCollocationModel collocation in collocations)
        {
            string normalizedText = NormalizeText(collocation.Text);

            if (string.IsNullOrWhiteSpace(normalizedText))
            {
                entryErrors.Add("Entry collocations cannot contain empty text.");
                continue;
            }

            if (normalizedText.Length > 256)
            {
                entryErrors.Add("Entry collocation text must not exceed 256 characters.");
                continue;
            }

            string? normalizedMeaning = NormalizeOptionalText(collocation.Meaning);

            if (normalizedMeaning is not null && normalizedMeaning.Length > 256)
            {
                entryErrors.Add("Entry collocation meaning must not exceed 256 characters.");
                continue;
            }

            if (normalized.Any(existing => string.Equals(existing.Text, normalizedText, StringComparison.Ordinal)))
            {
                entryErrors.Add($"Duplicate collocation '{normalizedText}' is not allowed.");
                continue;
            }

            normalized.Add(new ParsedContentCollocationModel(normalizedText, normalizedMeaning));
        }

        return normalized.ToArray();
    }

    private static ParsedContentWordFamilyMemberModel[] ValidateWordFamilies(
        IReadOnlyList<ParsedContentWordFamilyMemberModel> wordFamilies,
        ICollection<string> entryErrors)
    {
        List<ParsedContentWordFamilyMemberModel> normalized = [];

        foreach (ParsedContentWordFamilyMemberModel familyMember in wordFamilies)
        {
            string normalizedLemma = NormalizeText(familyMember.Lemma);
            string normalizedRelationLabel = NormalizeText(familyMember.RelationLabel);

            if (string.IsNullOrWhiteSpace(normalizedLemma))
            {
                entryErrors.Add("Entry wordFamilies cannot contain empty lemma values.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(normalizedRelationLabel))
            {
                entryErrors.Add("Entry wordFamilies cannot contain empty relationLabel values.");
                continue;
            }

            if (normalizedLemma.Length > 128)
            {
                entryErrors.Add("Entry wordFamilies lemma must not exceed 128 characters.");
                continue;
            }

            if (normalizedRelationLabel.Length > 64)
            {
                entryErrors.Add("Entry wordFamilies relationLabel must not exceed 64 characters.");
                continue;
            }

            string? normalizedNote = NormalizeOptionalText(familyMember.Note);

            if (normalizedNote is not null && normalizedNote.Length > 256)
            {
                entryErrors.Add("Entry wordFamilies note must not exceed 256 characters.");
                continue;
            }

            if (normalized.Any(existing =>
                    string.Equals(existing.Lemma, normalizedLemma, StringComparison.Ordinal) &&
                    string.Equals(existing.RelationLabel, normalizedRelationLabel, StringComparison.Ordinal)))
            {
                entryErrors.Add($"Duplicate wordFamilies member '{normalizedLemma}' with relation '{normalizedRelationLabel}' is not allowed.");
                continue;
            }

            normalized.Add(new ParsedContentWordFamilyMemberModel(normalizedLemma, normalizedRelationLabel, normalizedNote));
        }

        return normalized.ToArray();
    }

    private static ParsedContentWordRelationModel[] ValidateRelations(
        IReadOnlyList<ParsedContentWordRelationModel> relations,
        ICollection<string> entryErrors)
    {
        List<ParsedContentWordRelationModel> normalized = [];

        foreach (ParsedContentWordRelationModel relation in relations)
        {
            string normalizedKind = NormalizeText(relation.Kind).ToLowerInvariant();
            string normalizedLemma = NormalizeText(relation.Lemma);

            if (string.IsNullOrWhiteSpace(normalizedKind))
            {
                entryErrors.Add("Entry relations cannot contain empty kind values.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(normalizedLemma))
            {
                entryErrors.Add("Entry relations cannot contain empty lemma values.");
                continue;
            }

            if (normalizedLemma.Length > 128)
            {
                entryErrors.Add("Entry relations lemma must not exceed 128 characters.");
                continue;
            }

            if (normalizedKind is not ("synonym" or "antonym"))
            {
                entryErrors.Add($"Entry relation kind '{normalizedKind}' is not supported.");
                continue;
            }

            string? normalizedNote = NormalizeOptionalText(relation.Note);

            if (normalizedNote is not null && normalizedNote.Length > 256)
            {
                entryErrors.Add("Entry relations note must not exceed 256 characters.");
                continue;
            }

            if (normalized.Any(existing =>
                    string.Equals(existing.Kind, normalizedKind, StringComparison.Ordinal) &&
                    string.Equals(existing.Lemma, normalizedLemma, StringComparison.Ordinal)))
            {
                entryErrors.Add($"Duplicate relation '{normalizedKind}:{normalizedLemma}' is not allowed.");
                continue;
            }

            normalized.Add(new ParsedContentWordRelationModel(normalizedKind, normalizedLemma, normalizedNote));
        }

        return normalized.ToArray();
    }

    private static WordRelationKind ParseRelationKind(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "synonym" => WordRelationKind.Synonym,
            "antonym" => WordRelationKind.Antonym,
            _ => throw new InvalidOperationException($"Unsupported relation kind '{value}'."),
        };
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static ImportContentPackageResult CreateFatalFailureResult(
        string? packageId,
        IReadOnlyList<ImportIssueModel> issues,
        string? packageName = null,
        int totalEntries = 0)
    {
        return new ImportContentPackageResult(
            false,
            packageId,
            packageName,
            ContentPackageStatus.Failed.ToString(),
            totalEntries,
            0,
            0,
            totalEntries,
            issues.Count(issue => string.Equals(issue.Severity, "Warning", StringComparison.Ordinal)),
            issues,
            Array.Empty<string>());
    }

    private sealed record NormalizedLexicalForm(
        PartOfSpeech PartOfSpeech,
        string? Article,
        string? PluralForm,
        string? InfinitiveForm,
        bool IsPrimary,
        int SortOrder);
}

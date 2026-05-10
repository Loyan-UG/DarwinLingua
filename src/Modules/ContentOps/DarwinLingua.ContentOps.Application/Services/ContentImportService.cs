using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Domain.Entities;
using DarwinLingua.ContentOps.Domain.Enums;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.ContentOps.Application.Services;

/// <summary>
/// Implements the Phase 1 conservative JSON content-package import workflow.
/// </summary>
internal sealed class ContentImportService : IContentImportService
{
    private const string SupportedPackageVersion = "1.0";

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

        if (await _contentImportRepository.PackageExistsAsync(parsedPackage.PackageId, cancellationToken).ConfigureAwait(false))
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
            parsedPackage.PackageId,
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
        ProcessConversationStarterPacks(parsedPackage.ConversationStarterPacks, topicsByKey, importedConversationStarterPacks);
        ProcessEventPreparationPacks(parsedPackage.EventPreparationPacks, topicsByKey, importedEventPreparationPacks);

        contentPackage.Complete(DateTime.UtcNow);

        await _contentImportRepository
            .PersistImportAsync(contentPackage, importedLabelDefinitions, importedWords, importedCollections, importedDialogues, importedTalkTopics, importedConversationStarterPacks, importedEventPreparationPacks, cancellationToken)
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

            string[] topicKeys = dialogue.Topics
                .Select(topic => NormalizeText(topic).ToLowerInvariant())
                .Where(topic => !string.IsNullOrWhiteSpace(topic))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int topicIndex = 0; topicIndex < topicKeys.Length; topicIndex++)
            {
                lesson.AddTopic(Guid.NewGuid(), topicsByKey[topicKeys[topicIndex]].Id, topicIndex == 0, timestampUtc);
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
                topic.AddTopic(Guid.NewGuid(), topicsByKey[topicKeys[topicIndex]].Id, topicIndex == 0, timestampUtc);
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

            if (!Enum.TryParse(NormalizeText(dialogue.CefrLevel), true, out CefrLevel _))
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

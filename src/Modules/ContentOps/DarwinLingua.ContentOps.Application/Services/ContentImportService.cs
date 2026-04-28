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
        LanguageCode[] expectedMeaningLanguages = ResolveExpectedMeaningLanguages(
            parsedPackage.DefaultMeaningLanguages,
            meaningLanguages);

        ValidateScenarios(parsedPackage.Scenarios, topicsByKey, meaningLanguages, issues);
        ValidateConversationStarterPacks(parsedPackage.ConversationStarterPacks, topicsByKey, meaningLanguages, issues);
        ValidateEventPreparationPacks(parsedPackage.EventPreparationPacks, topicsByKey, issues);

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
        List<WordCollection> importedCollections = [];
        List<ScenarioLesson> importedScenarios = [];
        List<ConversationStarterPack> importedConversationStarterPacks = [];
        List<EventPreparationPack> importedEventPreparationPacks = [];

        for (int entryIndex = 0; entryIndex < parsedPackage.Entries.Count; entryIndex++)
        {
            ParsedContentEntryModel entry = parsedPackage.Entries[entryIndex];
            await ProcessEntryAsync(
                entryIndex,
                entry,
                topicsByKey,
                meaningLanguages,
                expectedMeaningLanguages,
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

        ProcessScenarios(parsedPackage.Scenarios, topicsByKey, importedScenarios);
        ProcessConversationStarterPacks(parsedPackage.ConversationStarterPacks, topicsByKey, importedConversationStarterPacks);
        ProcessEventPreparationPacks(parsedPackage.EventPreparationPacks, topicsByKey, importedEventPreparationPacks);

        contentPackage.Complete(DateTime.UtcNow);

        await _contentImportRepository
            .PersistImportAsync(contentPackage, importedWords, importedCollections, importedScenarios, importedConversationStarterPacks, importedEventPreparationPacks, cancellationToken)
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

    private static void ProcessScenarios(
        IReadOnlyList<ParsedScenarioLessonModel> scenarios,
        IReadOnlyDictionary<string, Topic> topicsByKey,
        ICollection<ScenarioLesson> importedScenarios)
    {
        DateTime timestampUtc = DateTime.UtcNow;

        foreach (ParsedScenarioLessonModel scenario in scenarios)
        {
            ScenarioLesson lesson = new(
                Guid.NewGuid(),
                NormalizeText(scenario.Slug),
                NormalizeText(scenario.Title),
                NormalizeText(scenario.Description),
                NormalizeText(scenario.LearnerGoal),
                Enum.Parse<CefrLevel>(NormalizeText(scenario.CefrLevel), true),
                NormalizeText(scenario.Category),
                PublicationStatus.Active,
                scenario.SortOrder < 0 ? 0 : scenario.SortOrder,
                timestampUtc);

            string[] topicKeys = scenario.Topics
                .Select(topic => NormalizeText(topic).ToLowerInvariant())
                .Where(topic => !string.IsNullOrWhiteSpace(topic))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int topicIndex = 0; topicIndex < topicKeys.Length; topicIndex++)
            {
                lesson.AddTopic(Guid.NewGuid(), topicsByKey[topicKeys[topicIndex]].Id, topicIndex == 0, timestampUtc);
            }

            for (int turnIndex = 0; turnIndex < scenario.DialogueTurns.Count; turnIndex++)
            {
                ParsedScenarioDialogueTurnModel parsedTurn = scenario.DialogueTurns[turnIndex];
                ScenarioDialogueTurn turn = lesson.AddDialogueTurn(
                    Guid.NewGuid(),
                    turnIndex + 1,
                    parsedTurn.SpeakerRole,
                    parsedTurn.BaseText,
                    timestampUtc);

                AddScenarioTranslations(turn.AddTranslation, parsedTurn.Translations, timestampUtc);
            }

            for (int phraseIndex = 0; phraseIndex < scenario.UsefulPhrases.Count; phraseIndex++)
            {
                ParsedScenarioPhraseModel parsedPhrase = scenario.UsefulPhrases[phraseIndex];
                ScenarioPhrase phrase = lesson.AddUsefulPhrase(
                    Guid.NewGuid(),
                    phraseIndex + 1,
                    parsedPhrase.BaseText,
                    parsedPhrase.UsageNote,
                    timestampUtc);

                AddScenarioTranslations(phrase.AddTranslation, parsedPhrase.Translations, timestampUtc);
            }

            for (int questionIndex = 0; questionIndex < scenario.Questions.Count; questionIndex++)
            {
                ParsedScenarioQuestionModel parsedQuestion = scenario.Questions[questionIndex];
                ScenarioQuestion question = lesson.AddQuestion(
                    Guid.NewGuid(),
                    questionIndex + 1,
                    parsedQuestion.Prompt,
                    timestampUtc);

                AddScenarioTranslations(question.AddTranslation, parsedQuestion.Translations, timestampUtc);

                for (int answerIndex = 0; answerIndex < parsedQuestion.Answers.Count; answerIndex++)
                {
                    ParsedScenarioAnswerModel parsedAnswer = parsedQuestion.Answers[answerIndex];
                    ScenarioAnswer answer = question.AddAnswer(
                        Guid.NewGuid(),
                        answerIndex + 1,
                        parsedAnswer.Text,
                        parsedAnswer.IsCorrect,
                        parsedAnswer.Feedback,
                        timestampUtc);

                    AddScenarioTranslations(answer.AddTranslation, parsedAnswer.Translations, timestampUtc);
                }
            }

            importedScenarios.Add(lesson);
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

            string[] scenarioSlugs = starterPack.LinkedScenarioSlugs
                .Select(slug => NormalizeText(slug).ToLowerInvariant())
                .Where(slug => !string.IsNullOrWhiteSpace(slug))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int scenarioIndex = 0; scenarioIndex < scenarioSlugs.Length; scenarioIndex++)
            {
                pack.AddLinkedScenario(Guid.NewGuid(), scenarioSlugs[scenarioIndex], scenarioIndex + 1, timestampUtc);
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

            string[] scenarioSlugs = parsedPack.LinkedScenarioSlugs
                .Select(slug => NormalizeText(slug).ToLowerInvariant())
                .Where(slug => !string.IsNullOrWhiteSpace(slug))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int scenarioIndex = 0; scenarioIndex < scenarioSlugs.Length; scenarioIndex++)
            {
                pack.AddLinkedScenario(Guid.NewGuid(), scenarioSlugs[scenarioIndex], scenarioIndex + 1, timestampUtc);
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

    private static void AddScenarioTranslations(
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
            .GroupBy(
                word => (word.NormalizedLemma, word.PartOfSpeech, word.PrimaryCefrLevel),
                EqualityComparer<(string, PartOfSpeech, CefrLevel)>.Default)
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
        string[] usageLabels = ValidateLabelKeys(entry.UsageLabels, "usageLabels", entryErrors);
        string[] contextLabels = ValidateLabelKeys(entry.ContextLabels, "contextLabels", entryErrors);
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

        if (importedWords.Any(word =>
                word.NormalizedLemma == normalizedLemma &&
                word.PartOfSpeech == primaryLexicalForm!.PartOfSpeech &&
                word.PrimaryCefrLevel == cefrLevel) ||
            await _contentImportRepository
                .WordExistsAsync(normalizedLemma, primaryLexicalForm!.PartOfSpeech, cefrLevel, cancellationToken)
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

        string[] coverageWarnings = BuildTranslationCoverageWarnings(
            rawLemma,
            meaningTranslations,
            examples,
            expectedMeaningLanguages);

        foreach (string coverageWarning in coverageWarnings)
        {
            issues.Add(new ImportIssueModel(entryIndex + 1, "Warning", coverageWarning));
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
            coverageWarnings.Length == 0 ? null : string.Join(" ", coverageWarnings),
            wordEntry.PublicId,
            DateTime.UtcNow);
    }

    private static LanguageCode[] ResolveExpectedMeaningLanguages(
        IReadOnlyList<string> defaultMeaningLanguages,
        IReadOnlySet<LanguageCode> activeMeaningLanguages)
    {
        return defaultMeaningLanguages
            .Select(language => NormalizeText(language).ToLowerInvariant())
            .Where(language => !string.IsNullOrWhiteSpace(language))
            .Select(LanguageCode.From)
            .Where(activeMeaningLanguages.Contains)
            .Distinct()
            .ToArray();
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

    private static void ValidateScenarios(
        IReadOnlyList<ParsedScenarioLessonModel> scenarios,
        IReadOnlyDictionary<string, Topic> topicsByKey,
        IReadOnlySet<LanguageCode> meaningLanguages,
        ICollection<ImportIssueModel> issues)
    {
        HashSet<string> slugs = [];

        for (int index = 0; index < scenarios.Count; index++)
        {
            ParsedScenarioLessonModel scenario = scenarios[index];
            List<string> errors = [];
            string slug = NormalizeText(scenario.Slug).ToLowerInvariant();

            if (!ValidateKebabKey(slug))
            {
                errors.Add("Scenario slug is required and must use lowercase kebab-case.");
            }
            else if (!slugs.Add(slug))
            {
                errors.Add($"Duplicate scenario slug '{slug}' is not allowed inside one package.");
            }

            if (string.IsNullOrWhiteSpace(NormalizeText(scenario.Title)))
            {
                errors.Add("Scenario title is required.");
            }

            if (string.IsNullOrWhiteSpace(NormalizeText(scenario.Description)))
            {
                errors.Add("Scenario description is required.");
            }

            if (string.IsNullOrWhiteSpace(NormalizeText(scenario.LearnerGoal)))
            {
                errors.Add("Scenario learnerGoal is required.");
            }

            if (!Enum.TryParse(NormalizeText(scenario.CefrLevel), true, out CefrLevel _))
            {
                errors.Add("Scenario CEFR level is invalid.");
            }

            string category = NormalizeText(scenario.Category).ToLowerInvariant();
            if (!ValidateKebabKey(category))
            {
                errors.Add("Scenario category is required and must use lowercase kebab-case.");
            }

            string[] topicKeys = scenario.Topics
                .Select(topic => NormalizeText(topic).ToLowerInvariant())
                .Where(topic => !string.IsNullOrWhiteSpace(topic))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (topicKeys.Length == 0)
            {
                errors.Add("Scenario topics must contain at least one topic key.");
            }

            foreach (string topicKey in topicKeys)
            {
                if (!topicsByKey.ContainsKey(topicKey))
                {
                    errors.Add($"Scenario references unknown topic key '{topicKey}'.");
                }
            }

            if (scenario.DialogueTurns.Count == 0)
            {
                errors.Add("Scenario dialogueTurns must contain at least one item.");
            }

            for (int turnIndex = 0; turnIndex < scenario.DialogueTurns.Count; turnIndex++)
            {
                ParsedScenarioDialogueTurnModel turn = scenario.DialogueTurns[turnIndex];
                string speakerRole = NormalizeText(turn.SpeakerRole).ToLowerInvariant();

                if (!ValidateKebabKey(speakerRole))
                {
                    errors.Add($"Scenario dialogueTurns[{turnIndex + 1}] speakerRole is required and must use lowercase kebab-case.");
                }

                if (string.IsNullOrWhiteSpace(NormalizeText(turn.BaseText)))
                {
                    errors.Add($"Scenario dialogueTurns[{turnIndex + 1}] baseText is required.");
                }

                ValidateScenarioTranslations(
                    turn.Translations,
                    meaningLanguages,
                    $"Scenario dialogueTurns[{turnIndex + 1}] translations",
                    errors);
            }

            if (scenario.UsefulPhrases.Count == 0)
            {
                errors.Add("Scenario usefulPhrases must contain at least one item.");
            }

            for (int phraseIndex = 0; phraseIndex < scenario.UsefulPhrases.Count; phraseIndex++)
            {
                ParsedScenarioPhraseModel phrase = scenario.UsefulPhrases[phraseIndex];

                if (string.IsNullOrWhiteSpace(NormalizeText(phrase.BaseText)))
                {
                    errors.Add($"Scenario usefulPhrases[{phraseIndex + 1}] baseText is required.");
                }

                ValidateScenarioTranslations(
                    phrase.Translations,
                    meaningLanguages,
                    $"Scenario usefulPhrases[{phraseIndex + 1}] translations",
                    errors);
            }

            if (scenario.Questions.Count == 0)
            {
                errors.Add("Scenario questions must contain at least one item.");
            }

            for (int questionIndex = 0; questionIndex < scenario.Questions.Count; questionIndex++)
            {
                ParsedScenarioQuestionModel question = scenario.Questions[questionIndex];

                if (string.IsNullOrWhiteSpace(NormalizeText(question.Prompt)))
                {
                    errors.Add($"Scenario questions[{questionIndex + 1}] prompt is required.");
                }

                ValidateScenarioTranslations(
                    question.Translations,
                    meaningLanguages,
                    $"Scenario questions[{questionIndex + 1}] translations",
                    errors);

                if (question.Answers.Count < 2)
                {
                    errors.Add($"Scenario questions[{questionIndex + 1}] must contain at least two answers.");
                }

                int correctAnswerCount = question.Answers.Count(answer => answer.IsCorrect);
                if (correctAnswerCount != 1)
                {
                    errors.Add($"Scenario questions[{questionIndex + 1}] must contain exactly one correct answer.");
                }

                for (int answerIndex = 0; answerIndex < question.Answers.Count; answerIndex++)
                {
                    ParsedScenarioAnswerModel answer = question.Answers[answerIndex];

                    if (string.IsNullOrWhiteSpace(NormalizeText(answer.Text)))
                    {
                        errors.Add($"Scenario questions[{questionIndex + 1}] answers[{answerIndex + 1}] text is required.");
                    }

                    ValidateScenarioTranslations(
                        answer.Translations,
                        meaningLanguages,
                        $"Scenario questions[{questionIndex + 1}] answers[{answerIndex + 1}] translations",
                        errors);
                }
            }

            if (errors.Count > 0)
            {
                issues.Add(new ImportIssueModel(null, "Error", $"Scenario {index + 1} '{slug}': {string.Join(" ", errors)}"));
            }
        }
    }

    private static void ValidateScenarioTranslations(
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

            foreach (string linkedScenarioSlug in pack.LinkedScenarioSlugs.Select(slug => NormalizeText(slug).ToLowerInvariant()))
            {
                if (!ValidateKebabKey(linkedScenarioSlug))
                {
                    errors.Add("Conversation starter pack linkedScenarioSlugs must use lowercase kebab-case.");
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

                ValidateScenarioTranslations(
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

            ValidateKebabReferences(pack.LinkedScenarioSlugs, "linkedScenarioSlugs", "Event preparation pack", errors);
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

        if (parsedPackage.Entries.Count == 0)
        {
            issues.Add(new ImportIssueModel(null, "Error", "The package must contain at least one entry."));
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

        return translations;
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
        string fieldName,
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

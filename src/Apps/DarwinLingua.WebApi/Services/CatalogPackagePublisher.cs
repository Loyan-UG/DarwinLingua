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
            activeEventPreparationPacks);
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
                EntryCount = definition.Words.Count,
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
        IReadOnlyList<EventPreparationPack> eventPreparationPacks)
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
                eventPreparationPacks),
            new(
                $"{clientProductKey}-catalog-full-{versionToken}",
                "Darwin Lingua Catalog",
                "full-catalog",
                "catalog",
                "full",
                words,
                dialogues,
                conversationStarterPacks,
                eventPreparationPacks),
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
                    .ToArray()));
        }

        return definitions;
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
        IReadOnlyList<EventPreparationPack> EventPreparationPacks);

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
}

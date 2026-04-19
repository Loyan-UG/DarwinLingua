using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Catalog.Application.Services;

/// <summary>
/// Implements the lexical-entry detail query workflow.
/// </summary>
internal sealed class WordDetailQueryService : IWordDetailQueryService
{
    private readonly IWordEntryRepository _wordEntryRepository;
    private readonly ITopicRepository _topicRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="WordDetailQueryService"/> class.
    /// </summary>
    public WordDetailQueryService(IWordEntryRepository wordEntryRepository, ITopicRepository topicRepository)
    {
        ArgumentNullException.ThrowIfNull(wordEntryRepository);
        ArgumentNullException.ThrowIfNull(topicRepository);

        _wordEntryRepository = wordEntryRepository;
        _topicRepository = topicRepository;
    }

    /// <inheritdoc />
    public async Task<WordDetailModel?> GetWordDetailsAsync(
        Guid publicId,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        string uiLanguageCode,
        CancellationToken cancellationToken)
    {
        if (publicId == Guid.Empty)
        {
            throw new ArgumentException("Public identifier cannot be empty.", nameof(publicId));
        }

        LanguageCode primaryMeaningLanguage = LanguageCode.From(primaryMeaningLanguageCode);
        LanguageCode? secondaryMeaningLanguage = string.IsNullOrWhiteSpace(secondaryMeaningLanguageCode)
            ? null
            : LanguageCode.From(secondaryMeaningLanguageCode);
        LanguageCode uiLanguage = LanguageCode.From(uiLanguageCode);

        WordEntry? word = await _wordEntryRepository
            .GetByPublicIdAsync(publicId, cancellationToken)
            .ConfigureAwait(false);

        if (word is null)
        {
            return null;
        }

        Guid[] topicIds = word.Topics
            .Select(link => link.TopicId)
            .Distinct()
            .ToArray();
        IReadOnlyDictionary<Guid, string> topicNamesById = topicIds.Length == 0
            ? new Dictionary<Guid, string>()
            : await _topicRepository
                .GetDisplayNamesByIdsAsync(topicIds, uiLanguage, LanguageCode.From("en"), cancellationToken)
                .ConfigureAwait(false);

        IReadOnlyList<string> topicNames = word.Topics
            .OrderByDescending(link => link.IsPrimaryTopic)
            .ThenBy(link => topicNamesById.TryGetValue(link.TopicId, out string? topicName) ? topicName : string.Empty)
            .Select(link => topicNamesById.TryGetValue(link.TopicId, out string? topicName) ? topicName : link.TopicId.ToString("D"))
            .ToArray();

        IReadOnlyList<string> usageLabels = word.Labels
            .Where(label => label.Kind == WordLabelKind.Usage)
            .OrderBy(label => label.SortOrder)
            .Select(label => label.Key)
            .ToArray();

        IReadOnlyList<string> contextLabels = word.Labels
            .Where(label => label.Kind == WordLabelKind.Context)
            .OrderBy(label => label.SortOrder)
            .Select(label => label.Key)
            .ToArray();

        IReadOnlyList<string> grammarNotes = word.GrammarNotes
            .OrderBy(note => note.SortOrder)
            .Select(note => note.Text)
            .ToArray();

        IReadOnlyList<WordCollocationDetailModel> collocations = word.Collocations
            .OrderBy(collocation => collocation.SortOrder)
            .Select(collocation => new WordCollocationDetailModel(collocation.Text, collocation.Meaning))
            .ToArray();

        IReadOnlyList<WordFamilyMemberDetailModel> wordFamilies = word.FamilyMembers
            .OrderBy(member => member.SortOrder)
            .Select(member => new WordFamilyMemberDetailModel(member.Lemma, member.RelationLabel, member.Note))
            .ToArray();

        IReadOnlyList<WordRelationDetailModel> synonyms = word.Relations
            .Where(relation => relation.Kind == WordRelationKind.Synonym)
            .OrderBy(relation => relation.SortOrder)
            .Select(relation => new WordRelationDetailModel(relation.Lemma, relation.Note))
            .ToArray();

        IReadOnlyList<WordRelationDetailModel> antonyms = word.Relations
            .Where(relation => relation.Kind == WordRelationKind.Antonym)
            .OrderBy(relation => relation.SortOrder)
            .Select(relation => new WordRelationDetailModel(relation.Lemma, relation.Note))
            .ToArray();

        IReadOnlyList<WordSenseDetailModel> senses = word.Senses
            .OrderByDescending(sense => sense.IsPrimarySense)
            .ThenBy(sense => sense.SenseOrder)
            .Select(sense => new WordSenseDetailModel(
                sense.ShortDefinitionDe,
                ResolveSenseTranslation(sense, primaryMeaningLanguage),
                secondaryMeaningLanguage is null ? null : ResolveSenseTranslation(sense, secondaryMeaningLanguage.Value),
                sense.Examples
                    .OrderByDescending(example => example.IsPrimaryExample)
                    .ThenBy(example => example.SentenceOrder)
                    .Select(example => new ExampleSentenceDetailModel(
                        example.GermanText,
                        ResolveExampleTranslation(example, primaryMeaningLanguage),
                        secondaryMeaningLanguage is null ? null : ResolveExampleTranslation(example, secondaryMeaningLanguage.Value)))
                    .ToArray()))
            .ToArray();

        IReadOnlyList<WordLexicalFormDetailModel> lexicalForms = word.LexicalForms.Count > 0
            ? word.LexicalForms
                .OrderByDescending(form => form.IsPrimary)
                .ThenBy(form => form.SortOrder)
                .Select(form => new WordLexicalFormDetailModel(
                    form.PartOfSpeech.ToString(),
                    form.Article,
                    form.PluralForm,
                    form.InfinitiveForm,
                    form.IsPrimary))
                .ToArray()
            : [new WordLexicalFormDetailModel(
                word.PartOfSpeech.ToString(),
                word.Article,
                word.PluralForm,
                word.InfinitiveForm,
                true)];

        return new WordDetailModel(
            word.PublicId,
            word.Lemma,
            word.Article,
            word.PluralForm,
            word.InfinitiveForm,
            word.PronunciationIpa,
            word.SyllableBreak,
            word.PartOfSpeech.ToString(),
            lexicalForms,
            word.PrimaryCefrLevel.ToString(),
            usageLabels,
            contextLabels,
            grammarNotes,
            collocations,
            wordFamilies,
            synonyms,
            antonyms,
            topicNames,
            senses);
    }

    private static string? ResolveSenseTranslation(WordSense sense, LanguageCode languageCode)
    {
        return sense.Translations
            .Where(translation => translation.LanguageCode == languageCode)
            .OrderByDescending(translation => translation.IsPrimary)
            .ThenBy(translation => translation.TranslationText)
            .Select(translation => translation.TranslationText)
            .FirstOrDefault();
    }

    private static string? ResolveExampleTranslation(ExampleSentence example, LanguageCode languageCode)
    {
        return example.Translations
            .Where(translation => translation.LanguageCode == languageCode)
            .OrderBy(translation => translation.TranslationText)
            .Select(translation => translation.TranslationText)
            .FirstOrDefault();
    }
}

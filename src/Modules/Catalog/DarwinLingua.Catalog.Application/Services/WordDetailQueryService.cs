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

        IReadOnlyList<Topic> topics = await _topicRepository
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        IReadOnlyList<string> topicNames = word.Topics
            .Select(link => topics.SingleOrDefault(topic => topic.Id == link.TopicId))
            .Where(topic => topic is not null)
            .OrderByDescending(topic => word.Topics.Single(link => link.TopicId == topic!.Id).IsPrimaryTopic)
            .ThenBy(topic => topic!.SortOrder)
            .Select(topic =>
            {
                TopicLocalization? localization = topic!.FindLocalization(uiLanguage)
                    ?? topic.FindLocalization(LanguageCode.From("en"));

                return localization?.DisplayName ?? topic.Key;
            })
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

        return new WordDetailModel(
            word.PublicId,
            word.Lemma,
            word.Article,
            word.PluralForm,
            word.InfinitiveForm,
            word.PartOfSpeech.ToString(),
            word.PrimaryCefrLevel.ToString(),
            usageLabels,
            contextLabels,
            grammarNotes,
            collocations,
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

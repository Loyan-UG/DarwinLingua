using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Catalog.Application.Tests;

/// <summary>
/// Verifies the lexical detail query behavior.
/// </summary>
public sealed class WordDetailQueryServiceTests
{
    /// <summary>
    /// Verifies that the detail query maps senses, examples, and localized topic names.
    /// </summary>
    [Fact]
    public async Task GetWordDetailsAsync_ShouldReturnMappedDetailModel()
    {
        Guid topicId = Guid.NewGuid();
        Topic topic = new(topicId, "shopping", 10, true, DateTime.UtcNow);
        topic.AddOrUpdateLocalization(Guid.NewGuid(), LanguageCode.From("en"), "Shopping", DateTime.UtcNow);
        topic.AddOrUpdateLocalization(Guid.NewGuid(), LanguageCode.From("de"), "Einkaufen", DateTime.UtcNow);

        WordEntry word = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Kasse",
            LanguageCode.From("de"),
            CefrLevel.A1,
            PartOfSpeech.Noun,
            PublicationStatus.Active,
            ContentSourceType.Manual,
            DateTime.UtcNow,
            article: "die",
            pluralForm: "Kassen");
        word.AddLexicalForm(Guid.NewGuid(), PartOfSpeech.Verb, false, DateTime.UtcNow, infinitiveForm: "kassieren");

        WordSense sense = word.AddSense(
            Guid.NewGuid(),
            1,
            true,
            PublicationStatus.Active,
            DateTime.UtcNow,
            shortDefinitionDe: "Ort zum Bezahlen");
        sense.AddTranslation(Guid.NewGuid(), LanguageCode.From("en"), "checkout", true, DateTime.UtcNow);
        sense.AddTranslation(Guid.NewGuid(), LanguageCode.From("fa"), "صندوق", false, DateTime.UtcNow);
        ExampleSentence example = sense.AddExample(
            Guid.NewGuid(),
            1,
            "Ich bezahle an der Kasse.",
            true,
            DateTime.UtcNow);
        example.AddTranslation(Guid.NewGuid(), LanguageCode.From("en"), "I pay at the checkout.", DateTime.UtcNow);
        example.AddTranslation(Guid.NewGuid(), LanguageCode.From("fa"), "من در صندوق پرداخت می‌کنم.", DateTime.UtcNow);
        word.AddTopic(Guid.NewGuid(), topicId, true, DateTime.UtcNow);
        word.AddLabel(Guid.NewGuid(), WordLabelKind.Usage, "formal", DateTime.UtcNow);
        word.AddLabel(Guid.NewGuid(), WordLabelKind.Usage, "spoken", DateTime.UtcNow);
        word.AddLabel(Guid.NewGuid(), WordLabelKind.Context, "shopping", DateTime.UtcNow);
        word.AddGrammarNote(Guid.NewGuid(), "Usually used with \"an der\" in shopping contexts.", DateTime.UtcNow);
        word.AddCollocation(Guid.NewGuid(), "an der Kasse bezahlen", "to pay at the checkout", DateTime.UtcNow);
        word.AddFamilyMember(Guid.NewGuid(), "Kassierer", "Profession", "person working at the checkout", DateTime.UtcNow);
        word.AddRelation(Guid.NewGuid(), WordRelationKind.Synonym, "Zahlstelle", "formal alternative", DateTime.UtcNow);
        word.AddRelation(Guid.NewGuid(), WordRelationKind.Antonym, "Ausgang", "opposite direction in a store", DateTime.UtcNow);

        ServiceCollection services = new();
        services.AddCatalogApplication();
        services.AddSingleton<IWordEntryRepository>(new FakeWordEntryRepository(word));
        services.AddSingleton<ITopicRepository>(new FakeTopicRepository([topic]));

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IWordDetailQueryService queryService = serviceProvider.GetRequiredService<IWordDetailQueryService>();

        WordDetailModel? result = await queryService.GetWordDetailsAsync(
            word.PublicId,
            "en",
            "fa",
            "de",
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Kasse", result!.Lemma);
        Assert.Equal("die", result.Article);
        Assert.Equal(2, result.LexicalForms.Count);
        Assert.Contains(result.LexicalForms, form => form.PartOfSpeech == "Noun" && form.IsPrimary && form.Article == "die");
        Assert.Contains(result.LexicalForms, form => form.PartOfSpeech == "Verb" && !form.IsPrimary && form.InfinitiveForm == "kassieren");
        Assert.Contains("formal", result.UsageLabels);
        Assert.Contains("spoken", result.UsageLabels);
        Assert.Equal(2, result.UsageLabels.Count);
        Assert.Contains("shopping", result.ContextLabels);
        Assert.Contains("Usually used with \"an der\" in shopping contexts.", result.GrammarNotes);
        Assert.Contains(result.Collocations, collocation => collocation.Text == "an der Kasse bezahlen" && collocation.Meaning == "to pay at the checkout");
        Assert.Contains(result.WordFamilies, member => member.Lemma == "Kassierer" && member.RelationLabel == "Profession");
        Assert.Contains(result.Synonyms, relation => relation.Lemma == "Zahlstelle");
        Assert.Contains(result.Antonyms, relation => relation.Lemma == "Ausgang");
        Assert.Contains("Einkaufen", result.Topics);

        WordSenseDetailModel senseResult = Assert.Single(result.Senses);
        Assert.Equal("checkout", senseResult.PrimaryMeaning);
        Assert.Equal("صندوق", senseResult.SecondaryMeaning);

        ExampleSentenceDetailModel exampleResult = Assert.Single(senseResult.Examples);
        Assert.Equal("I pay at the checkout.", exampleResult.PrimaryMeaning);
        Assert.Equal("من در صندوق پرداخت می‌کنم.", exampleResult.SecondaryMeaning);
    }

    private sealed class FakeWordEntryRepository(WordEntry word) : IWordEntryRepository
    {
        public Task<IReadOnlyList<WordListItemModel>> GetActiveByTopicKeyAsync(
            string topicKey,
            string meaningLanguageCode,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<WordListItemModel>>([]);
        }

        public Task<IReadOnlyList<WordListItemModel>> GetActiveByTopicPageAsync(
            string topicKey,
            string meaningLanguageCode,
            int skip,
            int take,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<WordListItemModel>>([]);
        }

        public Task<WordEntry?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
        {
            return Task.FromResult(word.PublicId == publicId ? word : null);
        }

        public Task<IReadOnlyList<WordListItemModel>> GetActiveByCefrAsync(
            CefrLevel cefrLevel,
            string meaningLanguageCode,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<WordListItemModel>>([]);
        }

        public Task<IReadOnlyList<WordListItemModel>> GetActiveByCefrPageAsync(
            CefrLevel cefrLevel,
            string meaningLanguageCode,
            int skip,
            int take,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<WordListItemModel>>([]);
        }

        public Task<IReadOnlyList<WordListItemModel>> SearchActiveByLemmaAsync(
            string normalizedLemmaQuery,
            string meaningLanguageCode,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<WordListItemModel>>([]);
        }
    }

    private sealed class FakeTopicRepository(IReadOnlyList<Topic> topics) : ITopicRepository
    {
        public Task<IReadOnlyList<Topic>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(topics);
        }

        public Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesByIdsAsync(
            IReadOnlyCollection<Guid> topicIds,
            LanguageCode preferredLanguageCode,
            LanguageCode fallbackLanguageCode,
            CancellationToken cancellationToken)
        {
            IReadOnlyDictionary<Guid, string> values = topics
                .Where(topic => topicIds.Contains(topic.Id))
                .ToDictionary(
                    topic => topic.Id,
                    topic => topic.FindLocalization(preferredLanguageCode)?.DisplayName
                        ?? topic.FindLocalization(fallbackLanguageCode)?.DisplayName
                        ?? topic.Key);
            return Task.FromResult(values);
        }
    }
}

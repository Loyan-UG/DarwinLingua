using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Tests;

/// <summary>
/// Verifies the <see cref="WordEntry"/> aggregate behavior.
/// </summary>
public sealed class WordEntryTests
{
    /// <summary>
    /// Verifies that duplicate sense orders are rejected.
    /// </summary>
    [Fact]
    public void AddSense_ShouldRejectDuplicateSenseOrder()
    {
        WordEntry word = CreateWordEntry();
        word.AddSense(Guid.NewGuid(), 1, true, PublicationStatus.Active, DateTime.UtcNow);

        Assert.Throws<DomainRuleException>(() => word.AddSense(
            Guid.NewGuid(),
            1,
            false,
            PublicationStatus.Active,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that duplicate sense identifiers are rejected.
    /// </summary>
    [Fact]
    public void AddSense_ShouldRejectDuplicateSenseIdentifier()
    {
        WordEntry word = CreateWordEntry();
        Guid senseId = Guid.NewGuid();
        word.AddSense(senseId, 1, true, PublicationStatus.Active, DateTime.UtcNow);

        Assert.Throws<DomainRuleException>(() => word.AddSense(
            senseId,
            2,
            false,
            PublicationStatus.Active,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that duplicate topic links are rejected.
    /// </summary>
    [Fact]
    public void AddTopic_ShouldRejectDuplicateTopic()
    {
        WordEntry word = CreateWordEntry();
        Guid topicId = Guid.NewGuid();
        word.AddTopic(Guid.NewGuid(), topicId, true, DateTime.UtcNow);

        Assert.Throws<DomainRuleException>(() => word.AddTopic(
            Guid.NewGuid(),
            topicId,
            false,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that duplicate topic-link identifiers are rejected.
    /// </summary>
    [Fact]
    public void AddTopic_ShouldRejectDuplicateTopicLinkIdentifier()
    {
        WordEntry word = CreateWordEntry();
        Guid topicLinkId = Guid.NewGuid();
        word.AddTopic(topicLinkId, Guid.NewGuid(), true, DateTime.UtcNow);

        Assert.Throws<DomainRuleException>(() => word.AddTopic(
            topicLinkId,
            Guid.NewGuid(),
            false,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that duplicate translation languages are rejected within a single sense.
    /// </summary>
    [Fact]
    public void AddTranslation_ShouldRejectDuplicateLanguagePerSense()
    {
        WordEntry word = CreateWordEntry();
        WordSense sense = word.AddSense(Guid.NewGuid(), 1, true, PublicationStatus.Active, DateTime.UtcNow);
        sense.AddTranslation(Guid.NewGuid(), LanguageCode.From("en"), "station", true, DateTime.UtcNow);

        Assert.Throws<DomainRuleException>(() => sense.AddTranslation(
            Guid.NewGuid(),
            LanguageCode.From("en"),
            "train station",
            false,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that duplicate translation identifiers are rejected within a single sense.
    /// </summary>
    [Fact]
    public void AddTranslation_ShouldRejectDuplicateIdentifierPerSense()
    {
        WordEntry word = CreateWordEntry();
        WordSense sense = word.AddSense(Guid.NewGuid(), 1, true, PublicationStatus.Active, DateTime.UtcNow);
        Guid translationId = Guid.NewGuid();
        sense.AddTranslation(translationId, LanguageCode.From("en"), "station", true, DateTime.UtcNow);

        Assert.Throws<DomainRuleException>(() => sense.AddTranslation(
            translationId,
            LanguageCode.From("tr"),
            "istasyon",
            false,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that setting a new primary sense demotes the previous primary sense.
    /// </summary>
    [Fact]
    public void AddSense_ShouldSwitchPrimarySenseWhenNewPrimaryIsAdded()
    {
        WordEntry word = CreateWordEntry();
        WordSense firstSense = word.AddSense(Guid.NewGuid(), 1, true, PublicationStatus.Active, DateTime.UtcNow);
        WordSense secondSense = word.AddSense(Guid.NewGuid(), 2, true, PublicationStatus.Active, DateTime.UtcNow);

        Assert.False(firstSense.IsPrimarySense);
        Assert.True(secondSense.IsPrimarySense);
        Assert.Equal(secondSense, word.GetPrimarySense());
    }

    /// <summary>
    /// Verifies that duplicate example orders are rejected within one sense.
    /// </summary>
    [Fact]
    public void AddExample_ShouldRejectDuplicateSentenceOrderPerSense()
    {
        WordEntry word = CreateWordEntry();
        WordSense sense = word.AddSense(Guid.NewGuid(), 1, true, PublicationStatus.Active, DateTime.UtcNow);
        sense.AddExample(Guid.NewGuid(), 1, "Ich gehe zum Bahnhof.", true, DateTime.UtcNow);

        Assert.Throws<DomainRuleException>(() => sense.AddExample(
            Guid.NewGuid(),
            1,
            "Wir treffen uns am Bahnhof.",
            false,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that duplicate example identifiers are rejected within one sense.
    /// </summary>
    [Fact]
    public void AddExample_ShouldRejectDuplicateIdentifierPerSense()
    {
        WordEntry word = CreateWordEntry();
        WordSense sense = word.AddSense(Guid.NewGuid(), 1, true, PublicationStatus.Active, DateTime.UtcNow);
        Guid exampleId = Guid.NewGuid();
        sense.AddExample(exampleId, 1, "Ich gehe zum Bahnhof.", true, DateTime.UtcNow);

        Assert.Throws<DomainRuleException>(() => sense.AddExample(
            exampleId,
            2,
            "Wir treffen uns am Bahnhof.",
            false,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that duplicate example-translation identifiers are rejected within one example sentence.
    /// </summary>
    [Fact]
    public void AddExampleTranslation_ShouldRejectDuplicateIdentifierPerExample()
    {
        WordEntry word = CreateWordEntry();
        WordSense sense = word.AddSense(Guid.NewGuid(), 1, true, PublicationStatus.Active, DateTime.UtcNow);
        ExampleSentence example = sense.AddExample(Guid.NewGuid(), 1, "Ich gehe zum Bahnhof.", true, DateTime.UtcNow);
        Guid translationId = Guid.NewGuid();
        example.AddTranslation(translationId, LanguageCode.From("en"), "I am going to the station.", DateTime.UtcNow);

        Assert.Throws<DomainRuleException>(() => example.AddTranslation(
            translationId,
            LanguageCode.From("tr"),
            "İstasyona gidiyorum.",
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that duplicate example-translation languages are rejected within one example sentence.
    /// </summary>
    [Fact]
    public void AddExampleTranslation_ShouldRejectDuplicateLanguagePerExample()
    {
        WordEntry word = CreateWordEntry();
        WordSense sense = word.AddSense(Guid.NewGuid(), 1, true, PublicationStatus.Active, DateTime.UtcNow);
        ExampleSentence example = sense.AddExample(Guid.NewGuid(), 1, "Ich gehe zum Bahnhof.", true, DateTime.UtcNow);
        example.AddTranslation(Guid.NewGuid(), LanguageCode.From("en"), "I am going to the station.", DateTime.UtcNow);

        Assert.Throws<DomainRuleException>(() => example.AddTranslation(
            Guid.NewGuid(),
            LanguageCode.From("en"),
            "We are meeting at the station.",
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that setting a new primary translation demotes the previous primary translation.
    /// </summary>
    [Fact]
    public void AddTranslation_ShouldSwitchPrimaryTranslationWhenNewPrimaryIsAdded()
    {
        WordEntry word = CreateWordEntry();
        WordSense sense = word.AddSense(Guid.NewGuid(), 1, true, PublicationStatus.Active, DateTime.UtcNow);

        SenseTranslation firstTranslation = sense.AddTranslation(
            Guid.NewGuid(),
            LanguageCode.From("en"),
            "station",
            true,
            DateTime.UtcNow);
        SenseTranslation secondTranslation = sense.AddTranslation(
            Guid.NewGuid(),
            LanguageCode.From("tr"),
            "istasyon",
            true,
            DateTime.UtcNow);

        Assert.False(firstTranslation.IsPrimary);
        Assert.True(secondTranslation.IsPrimary);
        Assert.Single(sense.Translations, translation => translation.IsPrimary);
    }

    /// <summary>
    /// Verifies that setting a new primary example demotes the previous primary example.
    /// </summary>
    [Fact]
    public void AddExample_ShouldSwitchPrimaryExampleWhenNewPrimaryIsAdded()
    {
        WordEntry word = CreateWordEntry();
        WordSense sense = word.AddSense(Guid.NewGuid(), 1, true, PublicationStatus.Active, DateTime.UtcNow);

        ExampleSentence firstExample = sense.AddExample(
            Guid.NewGuid(),
            1,
            "Ich gehe zum Bahnhof.",
            true,
            DateTime.UtcNow);
        ExampleSentence secondExample = sense.AddExample(
            Guid.NewGuid(),
            2,
            "Wir treffen uns am Bahnhof.",
            true,
            DateTime.UtcNow);

        Assert.False(firstExample.IsPrimaryExample);
        Assert.True(secondExample.IsPrimaryExample);
        Assert.Single(sense.Examples, example => example.IsPrimaryExample);
    }

    /// <summary>
    /// Verifies that setting a new primary topic demotes the previous primary topic.
    /// </summary>
    [Fact]
    public void AddTopic_ShouldSwitchPrimaryTopicWhenNewPrimaryIsAdded()
    {
        WordEntry word = CreateWordEntry();

        WordTopic firstTopic = word.AddTopic(Guid.NewGuid(), Guid.NewGuid(), true, DateTime.UtcNow);
        WordTopic secondTopic = word.AddTopic(Guid.NewGuid(), Guid.NewGuid(), true, DateTime.UtcNow);

        Assert.False(firstTopic.IsPrimaryTopic);
        Assert.True(secondTopic.IsPrimaryTopic);
        Assert.Single(word.Topics, topic => topic.IsPrimaryTopic);
    }

    /// <summary>
    /// Verifies that duplicate lexical labels are rejected within the same label kind.
    /// </summary>
    [Fact]
    public void AddLabel_ShouldRejectDuplicateKeyPerKind()
    {
        WordEntry word = CreateWordEntry();
        word.AddLabel(Guid.NewGuid(), WordLabelKind.Usage, "formal", DateTime.UtcNow);

        Assert.Throws<DomainRuleException>(() => word.AddLabel(
            Guid.NewGuid(),
            WordLabelKind.Usage,
            "formal",
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that the same key can be reused across different lexical label kinds.
    /// </summary>
    [Fact]
    public void AddLabel_ShouldAllowSameKeyAcrossDifferentKinds()
    {
        WordEntry word = CreateWordEntry();

        WordLabel usageLabel = word.AddLabel(Guid.NewGuid(), WordLabelKind.Usage, "formal", DateTime.UtcNow);
        WordLabel contextLabel = word.AddLabel(Guid.NewGuid(), WordLabelKind.Context, "formal", DateTime.UtcNow);

        Assert.Equal(WordLabelKind.Usage, usageLabel.Kind);
        Assert.Equal(WordLabelKind.Context, contextLabel.Kind);
        Assert.Equal(2, word.Labels.Count);
    }

    /// <summary>
    /// Verifies that duplicate grammar-note text is rejected within a single entry.
    /// </summary>
    [Fact]
    public void AddGrammarNote_ShouldRejectDuplicateText()
    {
        WordEntry word = CreateWordEntry();
        word.AddGrammarNote(Guid.NewGuid(), "Often used with the definite article.", DateTime.UtcNow);

        Assert.Throws<DomainRuleException>(() => word.AddGrammarNote(
            Guid.NewGuid(),
            "Often used with the definite article.",
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that duplicate collocation text is rejected within a single entry.
    /// </summary>
    [Fact]
    public void AddCollocation_ShouldRejectDuplicateText()
    {
        WordEntry word = CreateWordEntry();
        word.AddCollocation(Guid.NewGuid(), "Brot kaufen", "to buy bread", DateTime.UtcNow);

        Assert.Throws<DomainRuleException>(() => word.AddCollocation(
            Guid.NewGuid(),
            "Brot kaufen",
            "buy bread",
            DateTime.UtcNow));
    }

    private static WordEntry CreateWordEntry()
    {
        return new WordEntry(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Bahnhof",
            LanguageCode.From("de"),
            CefrLevel.A1,
            PartOfSpeech.Noun,
            PublicationStatus.Active,
            ContentSourceType.Manual,
            DateTime.UtcNow,
            article: "der");
    }
}

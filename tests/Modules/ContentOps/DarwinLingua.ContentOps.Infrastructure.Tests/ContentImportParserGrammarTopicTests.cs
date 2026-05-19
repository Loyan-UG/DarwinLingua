using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

public sealed class ContentImportParserGrammarTopicTests
{
    [Fact]
    public async Task ParseAsync_ShouldParseGrammarTopicContract()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection()
            .AddContentOpsInfrastructure()
            .BuildServiceProvider();

        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            """
            {
              "packageVersion": "1.0",
              "packageId": "grammar-contract-test",
              "packageName": "Grammar Contract Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "entries": [],
              "grammarTopics": [
                {
                  "slug": "a1-definite-articles",
                  "title": "Definite Articles",
                  "shortDescription": "Learn how der, die, and das mark German noun gender.",
                  "cefrLevel": "A1",
                  "grammarCategory": "articles",
                  "topics": ["daily-life"],
                  "sortOrder": 10,
                  "isPublished": true,
                  "sections": [
                    {
                      "heading": "When to use definite articles",
                      "explanation": "German uses definite articles before known nouns.",
                      "translations": [
                        { "language": "en", "heading": "When to use definite articles", "text": "Use definite articles for known nouns." }
                      ],
                      "sortOrder": 10
                    }
                  ],
                  "examples": [
                    {
                      "germanText": "Der Kaffee ist heiss.",
                      "translations": [{ "language": "en", "text": "The coffee is hot." }],
                      "sortOrder": 10
                    }
                  ],
                  "ruleSummaries": [
                    { "text": "der, die, and das agree with noun gender.", "translations": [{ "language": "en", "text": "Articles agree with noun gender." }], "sortOrder": 10 }
                  ],
                  "commonMistakes": [
                    { "wrongText": "die Kaffee", "correctedText": "der Kaffee", "explanation": "Kaffee is masculine.", "translations": [{ "language": "en", "text": "Kaffee is masculine." }], "sortOrder": 10 }
                  ],
                  "exceptionNotes": [
                    { "text": "Compound nouns take the gender of the last noun.", "sortOrder": 10 }
                  ],
                  "linkedWords": [{ "lemma": "der Kaffee", "wordSlug": "der-kaffee", "sortOrder": 10 }],
                  "linkedDialogueSlugs": ["a1-cafe-order"],
                  "linkedTalkTopicSlugs": ["a1-food-and-drinks"],
                  "linkedExerciseSlugs": ["a1-articles-practice"],
                  "prerequisiteSlugs": [],
                  "relatedTopicSlugs": ["a1-indefinite-articles"]
                }
              ]
            }
            """,
            CancellationToken.None);

        ParsedGrammarTopicModel grammarTopic = Assert.Single(parsedPackage.GrammarTopics);
        Assert.Equal("a1-definite-articles", grammarTopic.Slug);
        Assert.Equal("articles", grammarTopic.GrammarCategory);
        Assert.Equal("daily-life", Assert.Single(grammarTopic.Topics));
        Assert.Equal("When to use definite articles", Assert.Single(grammarTopic.Sections).Heading);
        Assert.Equal("Der Kaffee ist heiss.", Assert.Single(grammarTopic.Examples).GermanText);
        Assert.Equal("der Kaffee", Assert.Single(grammarTopic.LinkedWords).Lemma);
        Assert.Equal("a1-indefinite-articles", Assert.Single(grammarTopic.RelatedTopicSlugs));
    }

    [Fact]
    public async Task ParseAsync_ShouldParseGrammarRuleSummaryLocalizedTextObject()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection()
            .AddContentOpsInfrastructure()
            .BuildServiceProvider();

        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            """
            {
              "packageVersion": "1.0",
              "packageId": "grammar-localized-rule-test",
              "packageName": "Grammar Localized Rule Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en", "fa"],
              "entries": [],
              "grammarTopics": [
                {
                  "slug": "a1-localized-rule-test",
                  "title": "Localized rule test",
                  "shortDescription": "Checks localized rule parsing.",
                  "cefrLevel": "A1",
                  "grammarCategory": "articles",
                  "sections": [
                    {
                      "sectionKey": "intro",
                      "heading": "Intro",
                      "explanation": "Intro text.",
                      "localizedBlocks": {
                        "en": [{ "type": "paragraph", "text": "Intro text." }],
                        "fa": [{ "type": "paragraph", "text": "متن معرفی." }]
                      },
                      "sortOrder": 10
                    }
                  ],
                  "ruleSummaries": [
                    {
                      "localizedText": {
                        "en": "Learn the article with the noun.",
                        "fa": "آرتیکل را همراه اسم یاد بگیر."
                      },
                      "sortOrder": 10
                    }
                  ]
                }
              ]
            }
            """,
            CancellationToken.None);

        ParsedGrammarTopicModel grammarTopic = Assert.Single(parsedPackage.GrammarTopics);
        ParsedGrammarTextItemModel rule = Assert.Single(grammarTopic.RuleSummaries);

        Assert.Equal("Learn the article with the noun.", rule.Text);
        Assert.Contains(rule.Translations, translation =>
            translation.Language == "fa" &&
            translation.Text == "آرتیکل را همراه اسم یاد بگیر.");
    }

    [Fact]
    public async Task ParseAsync_ShouldParseOfficialA1GrammarCoreContract()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection()
            .AddContentOpsInfrastructure()
            .BuildServiceProvider();

        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();
        string repositoryRoot = ResolveRepositoryRoot();
        string packagePath = Path.Combine(repositoryRoot, "content", "learning-portal", "grammar", "packages", "grammar-a1-core-v1.json");

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            await File.ReadAllTextAsync(packagePath),
            CancellationToken.None);

        Assert.Equal(40, parsedPackage.GrammarTopics.Count);
        ParsedGrammarTopicModel grammarTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-personal-pronouns-ich-du-er-sie-es");
        Assert.Equal("a1-personal-pronouns-ich-du-er-sie-es", grammarTopic.Slug);
        Assert.Equal(1, grammarTopic.ContentRevision);
        Assert.Empty(grammarTopic.Topics);
        Assert.Equal(10, grammarTopic.Sections.Count);
        Assert.Equal(50, grammarTopic.Examples.Count);
        Assert.Equal(18, grammarTopic.RuleSummaries.Count);
        Assert.Equal(20, grammarTopic.CommonMistakes.Count);
        Assert.Equal(60, grammarTopic.LinkedWords.Count);
        Assert.Contains("fa", grammarTopic.TitleLocalized.Keys);
        Assert.Contains("ar", grammarTopic.ShortDescriptionLocalized.Keys);
        ParsedGrammarSectionModel tableSection = Assert.Single(grammarTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        Assert.Contains("en", tableSection.LocalizedBlocksJson.Keys);
        Assert.Contains("\"table\"", tableSection.LocalizedBlocksJson["en"], StringComparison.Ordinal);
        Assert.True(
            grammarTopic.Sections.Count(section => section.LocalizedBlocksJson.Values.Any(blocks => blocks.Contains("\"table\"", StringComparison.Ordinal))) >= 5);
        Assert.Contains(grammarTopic.RuleSummaries, rule => rule.Translations.Any(translation => translation.Language == "fa"));
        Assert.DoesNotContain(grammarTopic.LinkedWords, word => word.Lemma.Contains("meaning", StringComparison.OrdinalIgnoreCase));
        string serializedGrammarTopic = JsonSerializer.Serialize(grammarTopic);
        Assert.DoesNotContain("A simple A1 sentence", serializedGrammarTopic, StringComparison.Ordinal);
        Assert.DoesNotContain("برای فارسی‌زبان", serializedGrammarTopic, StringComparison.Ordinal);

        string[] languages = ["en", "fa", "ar", "tr", "ru", "ckb", "kmr", "pl", "ro", "sq"];
        Assert.All(languages, language =>
        {
            Assert.True(grammarTopic.TitleLocalized.ContainsKey(language));
            Assert.True(grammarTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(grammarTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(grammarTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(grammarTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(grammarTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(grammarTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        ParsedGrammarSectionTranslationModel faFirstSectionTranslation = Assert.Single(
            grammarTopic.Sections[0].Translations,
            translation => translation.Language == "fa");
        Assert.Equal("این موضوع چیست", faFirstSectionTranslation.Heading);

        Assert.All(grammarTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel seinTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-sein-in-praesens");
        Assert.Equal(1, seinTopic.ContentRevision);
        Assert.Equal("A1", seinTopic.CefrLevel);
        Assert.Equal("verbs", seinTopic.GrammarCategory);
        Assert.Equal(10, seinTopic.Sections.Count);
        Assert.Equal(46, seinTopic.Examples.Count);
        Assert.Equal(18, seinTopic.RuleSummaries.Count);
        Assert.Equal(20, seinTopic.CommonMistakes.Count);
        Assert.Equal(61, seinTopic.LinkedWords.Count);
        Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", seinTopic.PrerequisiteSlugs);
        Assert.Contains("a1-haben-in-praesens", seinTopic.RelatedTopicSlugs);
        ParsedGrammarSectionModel conjugationSection = Assert.Single(seinTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionTranslationModel faSeinFirstSectionTranslation = Assert.Single(
            seinTopic.Sections[0].Translations,
            translation => translation.Language == "fa");
        Assert.Equal("این موضوع چیست", faSeinFirstSectionTranslation.Heading);
        Assert.All(languages, language =>
        {
            Assert.True(seinTopic.TitleLocalized.ContainsKey(language));
            Assert.True(seinTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(seinTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(seinTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.True(conjugationSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", conjugationSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(seinTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(seinTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(seinTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.All(seinTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel habenTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-haben-in-praesens");
        Assert.Equal(1, habenTopic.ContentRevision);
        Assert.Equal("A1", habenTopic.CefrLevel);
        Assert.Equal("verbs", habenTopic.GrammarCategory);
        Assert.Equal(10, habenTopic.Sections.Count);
        Assert.True(habenTopic.Examples.Count >= 45);
        Assert.Equal(18, habenTopic.RuleSummaries.Count);
        Assert.True(habenTopic.CommonMistakes.Count >= 20);
        Assert.True(habenTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", habenTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", habenTopic.PrerequisiteSlugs);
        Assert.Contains("a1-simple-accusative-introduction", habenTopic.RelatedTopicSlugs);
        ParsedGrammarSectionModel habenConjugationSection = Assert.Single(habenTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        Assert.Contains(
            habenTopic.Sections,
            section => section.SectionKey == "word-order-or-case-focus" && section.LocalizedBlocksJson.Values.Any(json => json.Contains("\"callout\"", StringComparison.Ordinal)));
        Assert.All(languages, language =>
        {
            Assert.True(habenTopic.TitleLocalized.ContainsKey(language));
            Assert.True(habenTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(habenTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.True(habenConjugationSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", habenConjugationSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(habenTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(habenTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(habenTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.All(habenTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel regularVerbsTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-regular-verbs-in-praesens");
        Assert.Equal(1, regularVerbsTopic.ContentRevision);
        Assert.Equal("A1", regularVerbsTopic.CefrLevel);
        Assert.Equal("verbs", regularVerbsTopic.GrammarCategory);
        Assert.Equal(10, regularVerbsTopic.Sections.Count);
        Assert.True(regularVerbsTopic.Examples.Count >= 45);
        Assert.Equal(18, regularVerbsTopic.RuleSummaries.Count);
        Assert.True(regularVerbsTopic.CommonMistakes.Count >= 20);
        Assert.True(regularVerbsTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", regularVerbsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", regularVerbsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-haben-in-praesens", regularVerbsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-verb-position-in-simple-sentences", regularVerbsTopic.RelatedTopicSlugs);

        string[] regularVerbTableSections =
        [
            "form-or-structure-table",
            "word-order-or-case-focus",
            "comparison-table",
            "common-patterns",
        ];

        foreach (string sectionKey in regularVerbTableSections)
        {
            ParsedGrammarSectionModel tableTopicSection = Assert.Single(regularVerbsTopic.Sections, section => section.SectionKey == sectionKey);
            Assert.All(languages, language =>
            {
                Assert.True(tableTopicSection.LocalizedBlocksJson.ContainsKey(language));
                Assert.Contains("\"table\"", tableTopicSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            });
        }

        Assert.All(languages, language =>
        {
            Assert.True(regularVerbsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(regularVerbsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(regularVerbsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(regularVerbsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(regularVerbsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(regularVerbsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.All(regularVerbsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel verbPositionTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-verb-position-in-simple-sentences");
        Assert.Equal(1, verbPositionTopic.ContentRevision);
        Assert.Equal("A1", verbPositionTopic.CefrLevel);
        Assert.Equal("word-order", verbPositionTopic.GrammarCategory);
        Assert.Equal(10, verbPositionTopic.Sections.Count);
        Assert.True(verbPositionTopic.Examples.Count >= 45);
        Assert.Equal(16, verbPositionTopic.RuleSummaries.Count);
        Assert.True(verbPositionTopic.CommonMistakes.Count >= 20);
        Assert.True(verbPositionTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", verbPositionTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", verbPositionTopic.PrerequisiteSlugs);
        Assert.Contains("a1-haben-in-praesens", verbPositionTopic.PrerequisiteSlugs);
        Assert.Contains("a1-regular-verbs-in-praesens", verbPositionTopic.PrerequisiteSlugs);
        Assert.Contains("a1-word-order-with-time-and-place", verbPositionTopic.RelatedTopicSlugs);

        string[] verbPositionTableSections =
        [
            "form-or-structure-table",
            "word-order-or-case-focus",
            "comparison-table",
            "common-patterns",
        ];

        foreach (string sectionKey in verbPositionTableSections)
        {
            ParsedGrammarSectionModel tableTopicSection = Assert.Single(verbPositionTopic.Sections, section => section.SectionKey == sectionKey);
            Assert.All(languages, language =>
            {
                Assert.True(tableTopicSection.LocalizedBlocksJson.ContainsKey(language));
                Assert.Contains("\"table\"", tableTopicSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            });
        }

        Assert.All(languages, language =>
        {
            Assert.True(verbPositionTopic.TitleLocalized.ContainsKey(language));
            Assert.True(verbPositionTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(verbPositionTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(verbPositionTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(verbPositionTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(verbPositionTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.All(verbPositionTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel yesNoQuestionsTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-yes-no-questions");
        Assert.Equal(1, yesNoQuestionsTopic.ContentRevision);
        Assert.Equal("A1", yesNoQuestionsTopic.CefrLevel);
        Assert.Equal("questions", yesNoQuestionsTopic.GrammarCategory);
        Assert.Equal(10, yesNoQuestionsTopic.Sections.Count);
        Assert.True(yesNoQuestionsTopic.Examples.Count >= 45);
        Assert.Equal(16, yesNoQuestionsTopic.RuleSummaries.Count);
        Assert.True(yesNoQuestionsTopic.CommonMistakes.Count >= 20);
        Assert.True(yesNoQuestionsTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", yesNoQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", yesNoQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-haben-in-praesens", yesNoQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-regular-verbs-in-praesens", yesNoQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-verb-position-in-simple-sentences", yesNoQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-w-questions-wer-was-wo-wann-wie", yesNoQuestionsTopic.RelatedTopicSlugs);
        Assert.Contains("a1-question-answer-sentence-patterns", yesNoQuestionsTopic.RelatedTopicSlugs);
        Assert.Contains("a1-formal-sie", yesNoQuestionsTopic.RelatedTopicSlugs);
        Assert.DoesNotContain("a1-du-vs-sie-grammar-basics", yesNoQuestionsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel statementQuestionSection = Assert.Single(yesNoQuestionsTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        Assert.All(languages, language =>
        {
            Assert.True(yesNoQuestionsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(yesNoQuestionsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(statementQuestionSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", statementQuestionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(yesNoQuestionsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(yesNoQuestionsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(yesNoQuestionsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(yesNoQuestionsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(yesNoQuestionsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.All(yesNoQuestionsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel wQuestionsTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-w-questions-wer-was-wo-wann-wie");
        Assert.Equal(1, wQuestionsTopic.ContentRevision);
        Assert.Equal("A1", wQuestionsTopic.CefrLevel);
        Assert.Equal("questions", wQuestionsTopic.GrammarCategory);
        Assert.Equal(10, wQuestionsTopic.Sections.Count);
        Assert.True(wQuestionsTopic.Examples.Count >= 45);
        Assert.Equal(16, wQuestionsTopic.RuleSummaries.Count);
        Assert.True(wQuestionsTopic.CommonMistakes.Count >= 20);
        Assert.True(wQuestionsTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", wQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", wQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-haben-in-praesens", wQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-regular-verbs-in-praesens", wQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-verb-position-in-simple-sentences", wQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-yes-no-questions", wQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-question-answer-sentence-patterns", wQuestionsTopic.RelatedTopicSlugs);
        Assert.Contains("a1-basic-location-phrases", wQuestionsTopic.RelatedTopicSlugs);
        Assert.Contains("a1-basic-appointment-phrases", wQuestionsTopic.RelatedTopicSlugs);
        Assert.Contains("a1-formal-sie", wQuestionsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel questionWordsSection = Assert.Single(wQuestionsTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        Assert.All(languages, language =>
        {
            Assert.True(wQuestionsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(wQuestionsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(questionWordsSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", questionWordsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(wQuestionsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(wQuestionsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(wQuestionsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(wQuestionsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(wQuestionsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.All(wQuestionsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel definiteArticlesTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-definite-articles-der-die-das");
        Assert.Equal(1, definiteArticlesTopic.ContentRevision);
        Assert.Equal("A1", definiteArticlesTopic.CefrLevel);
        Assert.Equal("articles", definiteArticlesTopic.GrammarCategory);
        Assert.Equal(10, definiteArticlesTopic.Sections.Count);
        Assert.True(definiteArticlesTopic.Examples.Count >= 45);
        Assert.Equal(18, definiteArticlesTopic.RuleSummaries.Count);
        Assert.True(definiteArticlesTopic.CommonMistakes.Count >= 20);
        Assert.True(definiteArticlesTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", definiteArticlesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", definiteArticlesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-verb-position-in-simple-sentences", definiteArticlesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-indefinite-articles-ein-eine", definiteArticlesTopic.RelatedTopicSlugs);
        Assert.Contains("a1-noun-gender-basics", definiteArticlesTopic.RelatedTopicSlugs);
        Assert.Contains("a1-plural-basics", definiteArticlesTopic.RelatedTopicSlugs);
        Assert.Contains("a1-nominative-case", definiteArticlesTopic.RelatedTopicSlugs);
        Assert.Contains("a1-simple-accusative-introduction", definiteArticlesTopic.RelatedTopicSlugs);
        Assert.Contains("a1-articles-with-food-drinks-and-shopping-nouns", definiteArticlesTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel articleTableSection = Assert.Single(definiteArticlesTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel caseFocusSection = Assert.Single(definiteArticlesTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel comparisonSection = Assert.Single(definiteArticlesTopic.Sections, section => section.SectionKey == "comparison-table");
        Assert.All(languages, language =>
        {
            Assert.True(definiteArticlesTopic.TitleLocalized.ContainsKey(language));
            Assert.True(definiteArticlesTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(articleTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", articleTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", caseFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", comparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(definiteArticlesTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(definiteArticlesTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(definiteArticlesTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(definiteArticlesTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(definiteArticlesTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(definiteArticlesTopic.Examples, example => example.GermanText == "Der Kaffee ist neu.");
        Assert.Contains(definiteArticlesTopic.Examples, example => example.GermanText == "Ich sehe den Tisch.");
        Assert.Contains(definiteArticlesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich sehe der Mann.");
        Assert.Contains(definiteArticlesTopic.CommonMistakes, mistake => mistake.WrongText == "das Bücher");

        Assert.All(definiteArticlesTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel indefiniteArticlesTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-indefinite-articles-ein-eine");
        Assert.Equal(1, indefiniteArticlesTopic.ContentRevision);
        Assert.Equal("A1", indefiniteArticlesTopic.CefrLevel);
        Assert.Equal("articles", indefiniteArticlesTopic.GrammarCategory);
        Assert.Equal(10, indefiniteArticlesTopic.Sections.Count);
        Assert.True(indefiniteArticlesTopic.Examples.Count >= 45);
        Assert.Equal(18, indefiniteArticlesTopic.RuleSummaries.Count);
        Assert.True(indefiniteArticlesTopic.CommonMistakes.Count >= 20);
        Assert.True(indefiniteArticlesTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-definite-articles-der-die-das", indefiniteArticlesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", indefiniteArticlesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-verb-position-in-simple-sentences", indefiniteArticlesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-noun-gender-basics", indefiniteArticlesTopic.RelatedTopicSlugs);
        Assert.Contains("a1-plural-basics", indefiniteArticlesTopic.RelatedTopicSlugs);
        Assert.Contains("a1-nominative-case", indefiniteArticlesTopic.RelatedTopicSlugs);
        Assert.Contains("a1-simple-accusative-introduction", indefiniteArticlesTopic.RelatedTopicSlugs);
        Assert.Contains("a1-articles-with-food-drinks-and-shopping-nouns", indefiniteArticlesTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel einEineTableSection = Assert.Single(indefiniteArticlesTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel indefiniteCaseFocusSection = Assert.Single(indefiniteArticlesTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel comparisonTableSection = Assert.Single(indefiniteArticlesTopic.Sections, section => section.SectionKey == "comparison-table");
        Assert.All(languages, language =>
        {
            Assert.True(indefiniteArticlesTopic.TitleLocalized.ContainsKey(language));
            Assert.True(indefiniteArticlesTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(einEineTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(indefiniteCaseFocusSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(comparisonTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", einEineTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", indefiniteCaseFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", comparisonTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(indefiniteArticlesTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(indefiniteArticlesTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(indefiniteArticlesTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(indefiniteArticlesTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(indefiniteArticlesTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(indefiniteArticlesTopic.Examples, example => example.GermanText == "Das ist ein Mann.");
        Assert.Contains(indefiniteArticlesTopic.Examples, example => example.GermanText == "Ich brauche einen Termin.");
        Assert.Contains(indefiniteArticlesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich brauche ein Termin.");
        Assert.Contains(indefiniteArticlesTopic.CommonMistakes, mistake => mistake.WrongText == "Das sind eine Kinder.");

        Assert.All(indefiniteArticlesTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel nounGenderTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-noun-gender-basics");
        Assert.Equal(1, nounGenderTopic.ContentRevision);
        Assert.Equal("A1", nounGenderTopic.CefrLevel);
        Assert.Equal("gender", nounGenderTopic.GrammarCategory);
        Assert.Equal(10, nounGenderTopic.Sections.Count);
        Assert.True(nounGenderTopic.Examples.Count >= 45);
        Assert.Equal(18, nounGenderTopic.RuleSummaries.Count);
        Assert.True(nounGenderTopic.CommonMistakes.Count >= 20);
        Assert.True(nounGenderTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-definite-articles-der-die-das", nounGenderTopic.PrerequisiteSlugs);
        Assert.Contains("a1-indefinite-articles-ein-eine", nounGenderTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", nounGenderTopic.PrerequisiteSlugs);
        Assert.Contains("a1-plural-basics", nounGenderTopic.RelatedTopicSlugs);
        Assert.Contains("a1-nominative-case", nounGenderTopic.RelatedTopicSlugs);
        Assert.Contains("a1-simple-accusative-introduction", nounGenderTopic.RelatedTopicSlugs);
        Assert.Contains("a1-articles-with-food-drinks-and-shopping-nouns", nounGenderTopic.RelatedTopicSlugs);
        Assert.Contains("a1-basic-adjective-position", nounGenderTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel genderTableSection = Assert.Single(nounGenderTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel nounCaseFocusSection = Assert.Single(nounGenderTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel genderComparisonSection = Assert.Single(nounGenderTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel nounPatternsSection = Assert.Single(nounGenderTopic.Sections, section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(nounGenderTopic.TitleLocalized.ContainsKey(language));
            Assert.True(nounGenderTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(genderTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(nounCaseFocusSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(genderComparisonSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(nounPatternsSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", genderTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", nounCaseFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", genderComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", nounPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(nounGenderTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(nounGenderTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(nounGenderTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(nounGenderTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(nounGenderTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(nounGenderTopic.Examples, example => example.GermanText == "Der Mann ist hier.");
        Assert.Contains(nounGenderTopic.Examples, example => example.GermanText == "Ich sehe den Mann.");
        Assert.Contains(nounGenderTopic.CommonMistakes, mistake => mistake.WrongText == "die Mann");
        Assert.Contains(nounGenderTopic.CommonMistakes, mistake => mistake.WrongText == "Ich brauche eine Termin.");

        Assert.All(nounGenderTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel pluralBasicsTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-plural-basics");
        Assert.Equal(1, pluralBasicsTopic.ContentRevision);
        Assert.Equal("A1", pluralBasicsTopic.CefrLevel);
        Assert.Equal("plural", pluralBasicsTopic.GrammarCategory);
        Assert.Equal(10, pluralBasicsTopic.Sections.Count);
        Assert.True(pluralBasicsTopic.Examples.Count >= 45);
        Assert.Equal(18, pluralBasicsTopic.RuleSummaries.Count);
        Assert.True(pluralBasicsTopic.CommonMistakes.Count >= 20);
        Assert.True(pluralBasicsTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-definite-articles-der-die-das", pluralBasicsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-indefinite-articles-ein-eine", pluralBasicsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-noun-gender-basics", pluralBasicsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-nominative-case", pluralBasicsTopic.RelatedTopicSlugs);
        Assert.Contains("a1-simple-accusative-introduction", pluralBasicsTopic.RelatedTopicSlugs);
        Assert.Contains("a1-articles-with-food-drinks-and-shopping-nouns", pluralBasicsTopic.RelatedTopicSlugs);
        Assert.Contains("a1-numbers-and-grammar-use", pluralBasicsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel pluralPatternSection = Assert.Single(pluralBasicsTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel pluralCaseFocusSection = Assert.Single(pluralBasicsTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel pluralComparisonSection = Assert.Single(pluralBasicsTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel pluralCommonPatternsSection = Assert.Single(pluralBasicsTopic.Sections, section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(pluralBasicsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(pluralBasicsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(pluralPatternSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(pluralCaseFocusSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(pluralComparisonSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(pluralCommonPatternsSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", pluralPatternSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", pluralCaseFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", pluralComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", pluralCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(pluralBasicsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(pluralBasicsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(pluralBasicsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(pluralBasicsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(pluralBasicsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(pluralBasicsTopic.Examples, example => example.GermanText == "Die Kinder sind hier.");
        Assert.Contains(pluralBasicsTopic.Examples, example => example.GermanText == "Ich habe zwei Bücher.");
        Assert.Contains(pluralBasicsTopic.CommonMistakes, mistake => mistake.WrongText == "die Buch");
        Assert.Contains(pluralBasicsTopic.CommonMistakes, mistake => mistake.WrongText == "Die Kinder ist hier.");

        Assert.All(pluralBasicsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel nominativeTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-nominative-case");
        Assert.Equal(1, nominativeTopic.ContentRevision);
        Assert.Equal("A1", nominativeTopic.CefrLevel);
        Assert.Equal("nominative", nominativeTopic.GrammarCategory);
        Assert.Equal(10, nominativeTopic.Sections.Count);
        Assert.True(nominativeTopic.Examples.Count >= 45);
        Assert.Equal(18, nominativeTopic.RuleSummaries.Count);
        Assert.True(nominativeTopic.CommonMistakes.Count >= 20);
        Assert.True(nominativeTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", nominativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", nominativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-definite-articles-der-die-das", nominativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-indefinite-articles-ein-eine", nominativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-noun-gender-basics", nominativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-plural-basics", nominativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-simple-accusative-introduction", nominativeTopic.RelatedTopicSlugs);
        Assert.Contains("a1-basic-adjective-position", nominativeTopic.RelatedTopicSlugs);
        Assert.Contains("a1-pronoun-and-verb-agreement", nominativeTopic.RelatedTopicSlugs);
        Assert.Contains("a1-question-answer-sentence-patterns", nominativeTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel nominativeArticleTableSection = Assert.Single(nominativeTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel nominativeCaseFocusSection = Assert.Single(nominativeTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel nominativeComparisonSection = Assert.Single(nominativeTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel nominativePatternsSection = Assert.Single(nominativeTopic.Sections, section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(nominativeTopic.TitleLocalized.ContainsKey(language));
            Assert.True(nominativeTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(nominativeArticleTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", nominativeArticleTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", nominativeCaseFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", nominativeComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", nominativePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(nominativeTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(nominativeTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(nominativeTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(nominativeTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(nominativeTopic.Examples, example => example.GermanText == "Der Mann ist hier.");
        Assert.Contains(nominativeTopic.Examples, example => example.GermanText == "Morgen kommt der Lehrer.");
        Assert.Contains(nominativeTopic.CommonMistakes, mistake => mistake.WrongText == "Den Mann kommt.");
        Assert.Contains(nominativeTopic.CommonMistakes, mistake => mistake.WrongText == "Die Bücher ist neu.");

        Assert.All(nominativeTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel accusativeTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-simple-accusative-introduction");
        Assert.Equal(1, accusativeTopic.ContentRevision);
        Assert.Equal("A1", accusativeTopic.CefrLevel);
        Assert.Equal("accusative", accusativeTopic.GrammarCategory);
        Assert.Equal(10, accusativeTopic.Sections.Count);
        Assert.True(accusativeTopic.Examples.Count >= 45);
        Assert.Equal(18, accusativeTopic.RuleSummaries.Count);
        Assert.True(accusativeTopic.CommonMistakes.Count >= 20);
        Assert.True(accusativeTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-nominative-case", accusativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-definite-articles-der-die-das", accusativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-indefinite-articles-ein-eine", accusativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-noun-gender-basics", accusativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-haben-in-praesens", accusativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-kein-versus-nicht-basics", accusativeTopic.RelatedTopicSlugs);
        Assert.Contains("a1-articles-with-food-drinks-and-shopping-nouns", accusativeTopic.RelatedTopicSlugs);
        Assert.Contains("a1-basic-adjective-position", accusativeTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel articleChangeSection = Assert.Single(accusativeTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel accusativeCaseFocusSection = Assert.Single(accusativeTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel accusativeComparisonSection = Assert.Single(accusativeTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel accusativePatternsSection = Assert.Single(accusativeTopic.Sections, section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(accusativeTopic.TitleLocalized.ContainsKey(language));
            Assert.True(accusativeTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(accusativeComparisonSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(articleChangeSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", accusativeComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", articleChangeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", accusativeCaseFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", accusativePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(accusativeTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(accusativeTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(accusativeTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(accusativeTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(accusativeTopic.Examples, example => example.GermanText == "Ich sehe den Mann.");
        Assert.Contains(accusativeTopic.Examples, example => example.GermanText == "Ich brauche einen Termin.");
        Assert.Contains(accusativeTopic.CommonMistakes, mistake => mistake.WrongText == "Ich sehe der Mann.");
        Assert.Contains(accusativeTopic.CommonMistakes, mistake => mistake.WrongText == "Ich kaufe ein Kaffee.");

        Assert.All(accusativeTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel keinNichtTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-kein-versus-nicht-basics");
        Assert.Equal(1, keinNichtTopic.ContentRevision);
        Assert.Equal("A1", keinNichtTopic.CefrLevel);
        Assert.Equal("negation", keinNichtTopic.GrammarCategory);
        Assert.Equal(10, keinNichtTopic.Sections.Count);
        Assert.True(keinNichtTopic.Examples.Count >= 50);
        Assert.Equal(18, keinNichtTopic.RuleSummaries.Count);
        Assert.True(keinNichtTopic.CommonMistakes.Count >= 22);
        Assert.True(keinNichtTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-definite-articles-der-die-das", keinNichtTopic.PrerequisiteSlugs);
        Assert.Contains("a1-indefinite-articles-ein-eine", keinNichtTopic.PrerequisiteSlugs);
        Assert.Contains("a1-simple-accusative-introduction", keinNichtTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", keinNichtTopic.PrerequisiteSlugs);
        Assert.Contains("a1-haben-in-praesens", keinNichtTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-sentence-negation", keinNichtTopic.RelatedTopicSlugs);
        Assert.Contains("a2-possessive-pronouns-in-cases", keinNichtTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel keinTableSection = Assert.Single(keinNichtTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel nichtPositionSection = Assert.Single(keinNichtTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel keinComparisonSection = Assert.Single(keinNichtTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel commonPatternsSection = Assert.Single(keinNichtTopic.Sections, section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(keinNichtTopic.TitleLocalized.ContainsKey(language));
            Assert.True(keinNichtTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(keinTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(nichtPositionSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(keinComparisonSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(commonPatternsSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", keinTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", nichtPositionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", keinComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", commonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(keinNichtTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(keinNichtTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(keinNichtTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(keinNichtTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(keinNichtTopic.Examples, example => example.GermanText == "Ich habe keine Zeit.");
        Assert.Contains(keinNichtTopic.Examples, example => example.GermanText == "Das ist nicht gut.");
        Assert.Contains(keinNichtTopic.CommonMistakes, mistake => mistake.WrongText == "Ich habe nicht Zeit.");
        Assert.Contains(keinNichtTopic.CommonMistakes, mistake => mistake.WrongText == "Das Zimmer ist kein frei.");

        Assert.All(keinNichtTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel possessivePronounsTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-possessive-pronouns-mein-dein");
        Assert.Equal(1, possessivePronounsTopic.ContentRevision);
        Assert.Equal("A1", possessivePronounsTopic.CefrLevel);
        Assert.Equal("pronouns", possessivePronounsTopic.GrammarCategory);
        Assert.Equal(10, possessivePronounsTopic.Sections.Count);
        Assert.True(possessivePronounsTopic.Examples.Count >= 53);
        Assert.Equal(18, possessivePronounsTopic.RuleSummaries.Count);
        Assert.True(possessivePronounsTopic.CommonMistakes.Count >= 22);
        Assert.True(possessivePronounsTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", possessivePronounsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-definite-articles-der-die-das", possessivePronounsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-indefinite-articles-ein-eine", possessivePronounsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-noun-gender-basics", possessivePronounsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-plural-basics", possessivePronounsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-formal-sie", possessivePronounsTopic.RelatedTopicSlugs);
        Assert.Contains("a2-possessive-pronouns-in-cases", possessivePronounsTopic.RelatedTopicSlugs);
        Assert.Contains("a1-basic-adjective-position", possessivePronounsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel possessiveBasicFormsSection = Assert.Single(possessivePronounsTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel possessiveGenderSection = Assert.Single(possessivePronounsTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel possessiveComparisonSection = Assert.Single(possessivePronounsTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel possessivePatternsSection = Assert.Single(possessivePronounsTopic.Sections, section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(possessivePronounsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(possessivePronounsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(possessiveBasicFormsSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(possessiveGenderSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(possessiveComparisonSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(possessivePatternsSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", possessiveBasicFormsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", possessiveGenderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", possessiveComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", possessivePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(possessivePronounsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(possessivePronounsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(possessivePronounsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(possessivePronounsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(possessivePronounsTopic.Examples, example => example.GermanText == "Das ist mein Vater.");
        Assert.Contains(possessivePronounsTopic.Examples, example => example.GermanText == "Wie ist Ihre Adresse?");
        Assert.Contains(possessivePronounsTopic.CommonMistakes, mistake => mistake.WrongText == "Das ist meine Vater.");
        Assert.Contains(possessivePronounsTopic.CommonMistakes, mistake => mistake.WrongText == "Das ist kein mein Buch.");

        Assert.All(possessivePronounsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel basicAdjectiveTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-basic-adjective-position");
        Assert.Equal(1, basicAdjectiveTopic.ContentRevision);
        Assert.Equal("A1", basicAdjectiveTopic.CefrLevel);
        Assert.Equal("adjective-declension", basicAdjectiveTopic.GrammarCategory);
        Assert.Equal(10, basicAdjectiveTopic.Sections.Count);
        Assert.True(basicAdjectiveTopic.Examples.Count >= 55);
        Assert.Equal(18, basicAdjectiveTopic.RuleSummaries.Count);
        Assert.True(basicAdjectiveTopic.CommonMistakes.Count >= 25);
        Assert.True(basicAdjectiveTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-sein-in-praesens", basicAdjectiveTopic.PrerequisiteSlugs);
        Assert.Contains("a1-definite-articles-der-die-das", basicAdjectiveTopic.PrerequisiteSlugs);
        Assert.Contains("a1-indefinite-articles-ein-eine", basicAdjectiveTopic.PrerequisiteSlugs);
        Assert.Contains("a1-noun-gender-basics", basicAdjectiveTopic.PrerequisiteSlugs);
        Assert.Contains("a2-adjective-endings-introduction", basicAdjectiveTopic.RelatedTopicSlugs);
        Assert.Contains("b1-adjective-declension-after-definite-article", basicAdjectiveTopic.RelatedTopicSlugs);
        Assert.Contains("b1-adjective-declension-after-indefinite-article", basicAdjectiveTopic.RelatedTopicSlugs);
        Assert.Contains("a1-basic-sentence-negation", basicAdjectiveTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel adjectiveFormSection = Assert.Single(
            basicAdjectiveTopic.Sections,
            section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel adjectiveFocusSection = Assert.Single(
            basicAdjectiveTopic.Sections,
            section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel adjectiveComparisonSection = Assert.Single(
            basicAdjectiveTopic.Sections,
            section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel adjectivePatternsSection = Assert.Single(
            basicAdjectiveTopic.Sections,
            section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(basicAdjectiveTopic.TitleLocalized.ContainsKey(language));
            Assert.True(basicAdjectiveTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(adjectiveFormSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(adjectiveFocusSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(adjectiveComparisonSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(adjectivePatternsSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", adjectiveFormSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", adjectiveFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", adjectiveComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", adjectivePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(basicAdjectiveTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(basicAdjectiveTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(basicAdjectiveTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(basicAdjectiveTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(basicAdjectiveTopic.Examples, example => example.GermanText == "Das Zimmer ist klein.");
        Assert.Contains(basicAdjectiveTopic.Examples, example => example.GermanText == "Das ist ein kleines Zimmer.");
        Assert.Contains(basicAdjectiveTopic.CommonMistakes, mistake => mistake.WrongText == "Das Zimmer ist kleines.");
        Assert.Contains(basicAdjectiveTopic.CommonMistakes, mistake => mistake.WrongText == "Das ist ein klein Zimmer.");

        Assert.All(basicAdjectiveTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel basicPrepositionsTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-basic-prepositions-in-aus-nach-bei");
        Assert.Equal(1, basicPrepositionsTopic.ContentRevision);
        Assert.Equal("A1", basicPrepositionsTopic.CefrLevel);
        Assert.Equal("prepositions", basicPrepositionsTopic.GrammarCategory);
        Assert.Equal(10, basicPrepositionsTopic.Sections.Count);
        Assert.True(basicPrepositionsTopic.Examples.Count >= 50);
        Assert.Equal(19, basicPrepositionsTopic.RuleSummaries.Count);
        Assert.True(basicPrepositionsTopic.CommonMistakes.Count >= 23);
        Assert.True(basicPrepositionsTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-sein-in-praesens", basicPrepositionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-regular-verbs-in-praesens", basicPrepositionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-location-phrases", basicPrepositionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-location-phrases", basicPrepositionsTopic.RelatedTopicSlugs);
        Assert.Contains("a1-word-order-with-time-and-place", basicPrepositionsTopic.RelatedTopicSlugs);
        Assert.Contains("a2-prepositions-with-dative", basicPrepositionsTopic.RelatedTopicSlugs);
        Assert.Contains("a2-prepositions-with-accusative", basicPrepositionsTopic.RelatedTopicSlugs);
        Assert.Contains("a2-wechselpraepositionen-introduction", basicPrepositionsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel prepositionFormSection = Assert.Single(
            basicPrepositionsTopic.Sections,
            section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel prepositionFocusSection = Assert.Single(
            basicPrepositionsTopic.Sections,
            section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel prepositionComparisonSection = Assert.Single(
            basicPrepositionsTopic.Sections,
            section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel prepositionPatternsSection = Assert.Single(
            basicPrepositionsTopic.Sections,
            section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(basicPrepositionsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(basicPrepositionsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(prepositionFormSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(prepositionFocusSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(prepositionComparisonSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(prepositionPatternsSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", prepositionFormSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", prepositionFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", prepositionComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", prepositionPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(basicPrepositionsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(basicPrepositionsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(basicPrepositionsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(basicPrepositionsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(basicPrepositionsTopic.Examples, example => example.GermanText == "Ich bin in Berlin.");
        Assert.Contains(basicPrepositionsTopic.Examples, example => example.GermanText == "Ich gehe nach Hause.");
        Assert.Contains(basicPrepositionsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich bin nach Berlin.");
        Assert.Contains(basicPrepositionsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich gehe zu Hause.");

        Assert.All(basicPrepositionsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel numbersTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-numbers-and-grammar-use");
        Assert.Equal(1, numbersTopic.ContentRevision);
        Assert.Equal("A1", numbersTopic.CefrLevel);
        Assert.Equal("nouns", numbersTopic.GrammarCategory);
        Assert.Equal(10, numbersTopic.Sections.Count);
        Assert.True(numbersTopic.Examples.Count >= 55);
        Assert.Equal(18, numbersTopic.RuleSummaries.Count);
        Assert.True(numbersTopic.CommonMistakes.Count >= 20);
        Assert.True(numbersTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-sein-in-praesens", numbersTopic.PrerequisiteSlugs);
        Assert.Contains("a1-haben-in-praesens", numbersTopic.PrerequisiteSlugs);
        Assert.Contains("a1-w-questions-wer-was-wo-wann-wie", numbersTopic.PrerequisiteSlugs);
        Assert.Contains("a1-time-expressions-heute-morgen-gestern", numbersTopic.RelatedTopicSlugs);
        Assert.Contains("a1-basic-appointment-phrases", numbersTopic.RelatedTopicSlugs);
        Assert.Contains("a1-word-order-with-time-and-place", numbersTopic.RelatedTopicSlugs);
        Assert.Contains("a2-grammar-for-appointments", numbersTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel numbersFormSection = Assert.Single(
            numbersTopic.Sections,
            section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel numbersFocusSection = Assert.Single(
            numbersTopic.Sections,
            section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel numbersComparisonSection = Assert.Single(
            numbersTopic.Sections,
            section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel numbersCommonPatternsSection = Assert.Single(
            numbersTopic.Sections,
            section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(numbersTopic.TitleLocalized.ContainsKey(language));
            Assert.True(numbersTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(numbersFormSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(numbersFocusSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(numbersComparisonSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(numbersCommonPatternsSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", numbersFormSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", numbersFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", numbersComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", numbersCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(numbersTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(numbersTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(numbersTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(numbersTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(numbersTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(numbersTopic.Examples, example => example.GermanText == "Ich bin 25 Jahre alt.");
        Assert.Contains(numbersTopic.Examples, example => example.GermanText == "Ich habe zwei Kinder.");
        Assert.Contains(numbersTopic.CommonMistakes, mistake => mistake.WrongText == "Ich bin 25 Jahr alt.");
        Assert.Contains(numbersTopic.CommonMistakes, mistake => mistake.WrongText == "Ich habe zwei Kind.");

        Assert.All(numbersTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel timeExpressionsTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-time-expressions-heute-morgen-gestern");
        Assert.Equal(1, timeExpressionsTopic.ContentRevision);
        Assert.Equal("A1", timeExpressionsTopic.CefrLevel);
        Assert.Equal("tenses", timeExpressionsTopic.GrammarCategory);
        Assert.Equal(10, timeExpressionsTopic.Sections.Count);
        Assert.True(timeExpressionsTopic.Examples.Count >= 55);
        Assert.Equal(18, timeExpressionsTopic.RuleSummaries.Count);
        Assert.True(timeExpressionsTopic.CommonMistakes.Count >= 20);
        Assert.True(timeExpressionsTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-verb-position-in-simple-sentences", timeExpressionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-numbers-and-grammar-use", timeExpressionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-haben-in-praesens", timeExpressionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-regular-verbs-in-praesens", timeExpressionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-word-order-with-time-and-place", timeExpressionsTopic.RelatedTopicSlugs);
        Assert.Contains("a1-basic-appointment-phrases", timeExpressionsTopic.RelatedTopicSlugs);
        Assert.Contains("a2-perfekt-with-haben", timeExpressionsTopic.RelatedTopicSlugs);
        Assert.Contains("a2-praeteritum-of-sein-and-haben", timeExpressionsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel timeFormSection = Assert.Single(
            timeExpressionsTopic.Sections,
            section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel timeFocusSection = Assert.Single(
            timeExpressionsTopic.Sections,
            section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel timeComparisonSection = Assert.Single(
            timeExpressionsTopic.Sections,
            section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel timeCommonPatternsSection = Assert.Single(
            timeExpressionsTopic.Sections,
            section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(timeExpressionsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(timeExpressionsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(timeFormSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(timeFocusSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(timeComparisonSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(timeCommonPatternsSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", timeFormSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", timeFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", timeComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", timeCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(timeExpressionsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(timeExpressionsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(timeExpressionsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(timeExpressionsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(timeExpressionsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(timeExpressionsTopic.Examples, example => example.GermanText == "Heute komme ich.");
        Assert.Contains(timeExpressionsTopic.Examples, example => example.GermanText == "Morgen komme ich.");
        Assert.Contains(timeExpressionsTopic.Examples, example => example.GermanText == "Gestern war ich krank.");
        Assert.Contains(timeExpressionsTopic.CommonMistakes, mistake => mistake.WrongText == "Heute ich komme.");
        Assert.Contains(timeExpressionsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich bin gestern krank.");

        Assert.All(timeExpressionsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel timePlaceWordOrderTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-word-order-with-time-and-place");
        Assert.Equal(1, timePlaceWordOrderTopic.ContentRevision);
        Assert.Equal("A1", timePlaceWordOrderTopic.CefrLevel);
        Assert.Equal("word-order", timePlaceWordOrderTopic.GrammarCategory);
        Assert.Equal(10, timePlaceWordOrderTopic.Sections.Count);
        Assert.True(timePlaceWordOrderTopic.Examples.Count >= 60);
        Assert.Equal(18, timePlaceWordOrderTopic.RuleSummaries.Count);
        Assert.True(timePlaceWordOrderTopic.CommonMistakes.Count >= 20);
        Assert.True(timePlaceWordOrderTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-verb-position-in-simple-sentences", timePlaceWordOrderTopic.PrerequisiteSlugs);
        Assert.Contains("a1-time-expressions-heute-morgen-gestern", timePlaceWordOrderTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-prepositions-in-aus-nach-bei", timePlaceWordOrderTopic.PrerequisiteSlugs);
        Assert.Contains("a1-regular-verbs-in-praesens", timePlaceWordOrderTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-location-phrases", timePlaceWordOrderTopic.RelatedTopicSlugs);
        Assert.Contains("a1-basic-appointment-phrases", timePlaceWordOrderTopic.RelatedTopicSlugs);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", timePlaceWordOrderTopic.RelatedTopicSlugs);
        Assert.Contains("b1-sentence-order-with-multiple-clauses", timePlaceWordOrderTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel timePlaceFormSection = Assert.Single(
            timePlaceWordOrderTopic.Sections,
            section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel timePlaceFocusSection = Assert.Single(
            timePlaceWordOrderTopic.Sections,
            section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel timePlaceComparisonSection = Assert.Single(
            timePlaceWordOrderTopic.Sections,
            section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel timePlaceCommonPatternsSection = Assert.Single(
            timePlaceWordOrderTopic.Sections,
            section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(timePlaceWordOrderTopic.TitleLocalized.ContainsKey(language));
            Assert.True(timePlaceWordOrderTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(timePlaceFormSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(timePlaceFocusSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(timePlaceComparisonSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(timePlaceCommonPatternsSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", timePlaceFormSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", timePlaceFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", timePlaceComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", timePlaceCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(timePlaceWordOrderTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(timePlaceWordOrderTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(timePlaceWordOrderTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(timePlaceWordOrderTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(timePlaceWordOrderTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(timePlaceWordOrderTopic.Examples, example => example.GermanText == "Ich gehe heute in den Kurs.");
        Assert.Contains(timePlaceWordOrderTopic.Examples, example => example.GermanText == "Heute gehe ich in den Kurs.");
        Assert.Contains(timePlaceWordOrderTopic.Examples, example => example.GermanText == "Morgen lernen wir in der Schule.");
        Assert.Contains(timePlaceWordOrderTopic.CommonMistakes, mistake => mistake.WrongText == "Heute ich gehe in den Kurs.");
        Assert.Contains(timePlaceWordOrderTopic.CommonMistakes, mistake => mistake.WrongText == "Morgen wir lernen in der Schule.");

        Assert.All(timePlaceWordOrderTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel simpleModalsTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-simple-modal-verbs-koennen-muessen-wollen");
        Assert.Equal(1, simpleModalsTopic.ContentRevision);
        Assert.Equal("A1", simpleModalsTopic.CefrLevel);
        Assert.Equal("modal-verbs", simpleModalsTopic.GrammarCategory);
        Assert.Equal(10, simpleModalsTopic.Sections.Count);
        Assert.True(simpleModalsTopic.Examples.Count >= 55);
        Assert.Equal(18, simpleModalsTopic.RuleSummaries.Count);
        Assert.True(simpleModalsTopic.CommonMistakes.Count >= 22);
        Assert.True(simpleModalsTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", simpleModalsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-regular-verbs-in-praesens", simpleModalsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-verb-position-in-simple-sentences", simpleModalsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-yes-no-questions", simpleModalsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-polite-requests-with-moechte", simpleModalsTopic.RelatedTopicSlugs);
        Assert.Contains("a2-modal-verbs-in-more-detail", simpleModalsTopic.RelatedTopicSlugs);
        Assert.Contains("a2-polite-forms-with-wuerde", simpleModalsTopic.RelatedTopicSlugs);
        Assert.Contains("b1-modal-verbs-in-the-past", simpleModalsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel modalFormSection = Assert.Single(
            simpleModalsTopic.Sections,
            section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel modalFocusSection = Assert.Single(
            simpleModalsTopic.Sections,
            section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel modalComparisonSection = Assert.Single(
            simpleModalsTopic.Sections,
            section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel modalCommonPatternsSection = Assert.Single(
            simpleModalsTopic.Sections,
            section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(simpleModalsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(simpleModalsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(modalFormSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(modalFocusSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(modalComparisonSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(modalCommonPatternsSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", modalFormSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", modalFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", modalComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", modalCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(simpleModalsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(simpleModalsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(simpleModalsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(simpleModalsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(simpleModalsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(simpleModalsTopic.Examples, example => example.GermanText == "Ich kann Deutsch sprechen.");
        Assert.Contains(simpleModalsTopic.Examples, example => example.GermanText == "Ich muss heute arbeiten.");
        Assert.Contains(simpleModalsTopic.Examples, example => example.GermanText == "Ich will Wasser trinken.");
        Assert.Contains(simpleModalsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich kann Deutsch spreche.");
        Assert.Contains(simpleModalsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich muss arbeite heute.");

        Assert.All(simpleModalsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel politeMoechteTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-polite-requests-with-moechte");
        Assert.Equal(1, politeMoechteTopic.ContentRevision);
        Assert.Equal("A1", politeMoechteTopic.CefrLevel);
        Assert.Equal("modal-verbs", politeMoechteTopic.GrammarCategory);
        Assert.Equal(10, politeMoechteTopic.Sections.Count);
        Assert.True(politeMoechteTopic.Examples.Count >= 55);
        Assert.Equal(18, politeMoechteTopic.RuleSummaries.Count);
        Assert.True(politeMoechteTopic.CommonMistakes.Count >= 22);
        Assert.True(politeMoechteTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-simple-modal-verbs-koennen-muessen-wollen", politeMoechteTopic.PrerequisiteSlugs);
        Assert.Contains("a1-simple-accusative-introduction", politeMoechteTopic.PrerequisiteSlugs);
        Assert.Contains("a1-yes-no-questions", politeMoechteTopic.PrerequisiteSlugs);
        Assert.Contains("a1-w-questions-wer-was-wo-wann-wie", politeMoechteTopic.PrerequisiteSlugs);
        Assert.Contains("a1-imperative-basics", politeMoechteTopic.RelatedTopicSlugs);
        Assert.Contains("a1-articles-with-food-drinks-and-shopping-nouns", politeMoechteTopic.RelatedTopicSlugs);
        Assert.Contains("a1-basic-appointment-phrases", politeMoechteTopic.RelatedTopicSlugs);
        Assert.Contains("a2-polite-forms-with-wuerde", politeMoechteTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel moechteFormSection = Assert.Single(
            politeMoechteTopic.Sections,
            section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel moechteFocusSection = Assert.Single(
            politeMoechteTopic.Sections,
            section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel moechteComparisonSection = Assert.Single(
            politeMoechteTopic.Sections,
            section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel moechteCommonPatternsSection = Assert.Single(
            politeMoechteTopic.Sections,
            section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(politeMoechteTopic.TitleLocalized.ContainsKey(language));
            Assert.True(politeMoechteTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(moechteFormSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(moechteFocusSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(moechteComparisonSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(moechteCommonPatternsSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", moechteFormSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", moechteFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", moechteComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", moechteCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(politeMoechteTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(politeMoechteTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(politeMoechteTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(politeMoechteTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(politeMoechteTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(politeMoechteTopic.Examples, example => example.GermanText == "Ich möchte einen Kaffee.");
        Assert.Contains(politeMoechteTopic.Examples, example => example.GermanText == "Ich möchte bezahlen.");
        Assert.Contains(politeMoechteTopic.Examples, example => example.GermanText == "Möchtest du Wasser?");
        Assert.Contains(politeMoechteTopic.CommonMistakes, mistake => mistake.WrongText == "Ich möchte zu bezahlen.");
        Assert.Contains(politeMoechteTopic.CommonMistakes, mistake => mistake.WrongText == "Ich möchte Deutsch lerne.");

        Assert.All(politeMoechteTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel imperativeTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-imperative-basics");
        Assert.Equal(1, imperativeTopic.ContentRevision);
        Assert.Equal("A1", imperativeTopic.CefrLevel);
        Assert.Equal("imperative", imperativeTopic.GrammarCategory);
        Assert.Equal(10, imperativeTopic.Sections.Count);
        Assert.True(imperativeTopic.Examples.Count >= 50);
        Assert.Equal(18, imperativeTopic.RuleSummaries.Count);
        Assert.True(imperativeTopic.CommonMistakes.Count >= 20);
        Assert.True(imperativeTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", imperativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-formal-sie", imperativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-regular-verbs-in-praesens", imperativeTopic.PrerequisiteSlugs);
        Assert.DoesNotContain("a1-du-vs-sie-grammar-basics", imperativeTopic.PrerequisiteSlugs);
        Assert.Contains("a2-imperative-formal-and-informal", imperativeTopic.RelatedTopicSlugs);
        Assert.Contains("a1-polite-requests-with-moechte", imperativeTopic.RelatedTopicSlugs);
        Assert.Contains("a1-yes-no-questions", imperativeTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel imperativeTableSection = Assert.Single(
            imperativeTopic.Sections,
            section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel imperativeWordOrderSection = Assert.Single(
            imperativeTopic.Sections,
            section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel imperativeComparisonSection = Assert.Single(
            imperativeTopic.Sections,
            section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel imperativePatternsSection = Assert.Single(
            imperativeTopic.Sections,
            section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(imperativeTopic.TitleLocalized.ContainsKey(language));
            Assert.True(imperativeTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(imperativeTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", imperativeTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", imperativeWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", imperativeComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", imperativePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(imperativeTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(imperativeTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(imperativeTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.Contains(imperativeTopic.Sections, section => section.LocalizedBlocksJson[language].Contains("\"warning\"", StringComparison.Ordinal));
            Assert.All(imperativeTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(imperativeTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(imperativeTopic.Examples, example => example.GermanText == "Komm bitte.");
        Assert.Contains(imperativeTopic.Examples, example => example.GermanText == "Warten Sie bitte.");
        Assert.Contains(imperativeTopic.CommonMistakes, mistake => mistake.WrongText == "Du kommst bitte.");
        Assert.Contains(imperativeTopic.CommonMistakes, mistake => mistake.WrongText == "Sie kommen bitte.");

        Assert.All(imperativeTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel separableVerbsTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-separable-verbs-introduction");
        Assert.Equal(1, separableVerbsTopic.ContentRevision);
        Assert.Equal("A1", separableVerbsTopic.CefrLevel);
        Assert.Equal("separable-verbs", separableVerbsTopic.GrammarCategory);
        Assert.Equal(10, separableVerbsTopic.Sections.Count);
        Assert.True(separableVerbsTopic.Examples.Count >= 55);
        Assert.Equal(18, separableVerbsTopic.RuleSummaries.Count);
        Assert.True(separableVerbsTopic.CommonMistakes.Count >= 22);
        Assert.True(separableVerbsTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-regular-verbs-in-praesens", separableVerbsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-verb-position-in-simple-sentences", separableVerbsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-yes-no-questions", separableVerbsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-simple-modal-verbs-koennen-muessen-wollen", separableVerbsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-separable-verbs-in-perfekt", separableVerbsTopic.RelatedTopicSlugs);
        Assert.Contains("a2-common-irregular-participles", separableVerbsTopic.RelatedTopicSlugs);
        Assert.Contains("a1-word-order-with-time-and-place", separableVerbsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel separableStructureSection = Assert.Single(
            separableVerbsTopic.Sections,
            section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel separablePositionSection = Assert.Single(
            separableVerbsTopic.Sections,
            section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel separableComparisonSection = Assert.Single(
            separableVerbsTopic.Sections,
            section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel separablePatternsSection = Assert.Single(
            separableVerbsTopic.Sections,
            section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(separableVerbsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(separableVerbsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(separableStructureSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(separablePositionSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", separableStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", separablePositionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", separableComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", separablePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(separableVerbsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(separableVerbsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(separableVerbsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(separableVerbsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(separableVerbsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(separableVerbsTopic.Examples, example => example.GermanText == "Ich stehe früh auf.");
        Assert.Contains(separableVerbsTopic.Examples, example => example.GermanText == "Ich muss heute einkaufen.");
        Assert.Contains(separableVerbsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich aufstehe um sieben Uhr.");
        Assert.Contains(separableVerbsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich muss kaufe ein.");

        Assert.All(separableVerbsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel simpleConjunctionsTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-simple-conjunctions-und-aber");
        Assert.Equal(1, simpleConjunctionsTopic.ContentRevision);
        Assert.Equal("A1", simpleConjunctionsTopic.CefrLevel);
        Assert.Equal("connectors", simpleConjunctionsTopic.GrammarCategory);
        Assert.Equal(10, simpleConjunctionsTopic.Sections.Count);
        Assert.True(simpleConjunctionsTopic.Examples.Count >= 50);
        Assert.Equal(18, simpleConjunctionsTopic.RuleSummaries.Count);
        Assert.True(simpleConjunctionsTopic.CommonMistakes.Count >= 20);
        Assert.True(simpleConjunctionsTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-verb-position-in-simple-sentences", simpleConjunctionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-regular-verbs-in-praesens", simpleConjunctionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-adjective-position", simpleConjunctionsTopic.PrerequisiteSlugs);
        Assert.DoesNotContain("a2-denn-vs-weil", simpleConjunctionsTopic.RelatedTopicSlugs);
        Assert.Contains("a2-dass-clauses", simpleConjunctionsTopic.RelatedTopicSlugs);
        Assert.Contains("a2-weil-clauses", simpleConjunctionsTopic.RelatedTopicSlugs);
        Assert.Contains("b1-connectors-for-opinion", simpleConjunctionsTopic.RelatedTopicSlugs);
        Assert.Contains("b1-connectors-for-contrast", simpleConjunctionsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel conjunctionStructureSection = Assert.Single(
            simpleConjunctionsTopic.Sections,
            section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel conjunctionWordOrderSection = Assert.Single(
            simpleConjunctionsTopic.Sections,
            section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel conjunctionComparisonSection = Assert.Single(
            simpleConjunctionsTopic.Sections,
            section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel conjunctionPatternsSection = Assert.Single(
            simpleConjunctionsTopic.Sections,
            section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(simpleConjunctionsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(simpleConjunctionsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(conjunctionStructureSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(conjunctionWordOrderSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", conjunctionStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", conjunctionWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", conjunctionComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", conjunctionPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(simpleConjunctionsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(simpleConjunctionsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(simpleConjunctionsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(simpleConjunctionsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(simpleConjunctionsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(simpleConjunctionsTopic.Examples, example => example.GermanText == "Ich lerne Deutsch und Englisch.");
        Assert.Contains(simpleConjunctionsTopic.Examples, example => example.GermanText == "Ich lerne Deutsch, aber es ist schwer.");
        Assert.Contains(simpleConjunctionsTopic.CommonMistakes, mistake => mistake.WrongText == "Der Kaffee ist gut und teuer.");
        Assert.Contains(simpleConjunctionsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich lerne Deutsch, aber ist schwer.");

        Assert.All(simpleConjunctionsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel pronounVerbAgreementTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-pronoun-and-verb-agreement");
        Assert.Equal(1, pronounVerbAgreementTopic.ContentRevision);
        Assert.Equal("A1", pronounVerbAgreementTopic.CefrLevel);
        Assert.Equal("verbs", pronounVerbAgreementTopic.GrammarCategory);
        Assert.Equal(10, pronounVerbAgreementTopic.Sections.Count);
        Assert.True(pronounVerbAgreementTopic.Examples.Count >= 55);
        Assert.Equal(18, pronounVerbAgreementTopic.RuleSummaries.Count);
        Assert.True(pronounVerbAgreementTopic.CommonMistakes.Count >= 22);
        Assert.True(pronounVerbAgreementTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", pronounVerbAgreementTopic.PrerequisiteSlugs);
        Assert.Contains("a1-regular-verbs-in-praesens", pronounVerbAgreementTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", pronounVerbAgreementTopic.PrerequisiteSlugs);
        Assert.Contains("a1-haben-in-praesens", pronounVerbAgreementTopic.PrerequisiteSlugs);
        Assert.Contains("a1-yes-no-questions", pronounVerbAgreementTopic.PrerequisiteSlugs);
        Assert.Contains("a1-verb-position-in-simple-sentences", pronounVerbAgreementTopic.RelatedTopicSlugs);
        Assert.Contains("a1-w-questions-wer-was-wo-wann-wie", pronounVerbAgreementTopic.RelatedTopicSlugs);
        Assert.Contains("a1-formal-sie", pronounVerbAgreementTopic.RelatedTopicSlugs);
        Assert.Contains("a1-du-versus-sie-grammar-basics", pronounVerbAgreementTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel regularVerbEndingSection = Assert.Single(
            pronounVerbAgreementTopic.Sections,
            section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel wordOrderAgreementSection = Assert.Single(
            pronounVerbAgreementTopic.Sections,
            section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel comparisonAgreementSection = Assert.Single(
            pronounVerbAgreementTopic.Sections,
            section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel patternsAgreementSection = Assert.Single(
            pronounVerbAgreementTopic.Sections,
            section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(pronounVerbAgreementTopic.TitleLocalized.ContainsKey(language));
            Assert.True(pronounVerbAgreementTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(regularVerbEndingSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(wordOrderAgreementSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(comparisonAgreementSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", regularVerbEndingSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", wordOrderAgreementSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", comparisonAgreementSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", patternsAgreementSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(pronounVerbAgreementTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(pronounVerbAgreementTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(pronounVerbAgreementTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(pronounVerbAgreementTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(pronounVerbAgreementTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(pronounVerbAgreementTopic.Examples, example => example.GermanText == "Ich lerne Deutsch.");
        Assert.Contains(pronounVerbAgreementTopic.Examples, example => example.GermanText == "Lernst du Deutsch?");
        Assert.Contains(pronounVerbAgreementTopic.CommonMistakes, mistake => mistake.WrongText == "Ich lernst Deutsch.");
        Assert.Contains(pronounVerbAgreementTopic.CommonMistakes, mistake => mistake.WrongText == "Du lerne Deutsch.");

        Assert.All(pronounVerbAgreementTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel formalSieTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-formal-sie");
        Assert.Equal(1, formalSieTopic.ContentRevision);
        Assert.Equal("A1", formalSieTopic.CefrLevel);
        Assert.Equal("pronouns", formalSieTopic.GrammarCategory);
        Assert.Equal(10, formalSieTopic.Sections.Count);
        Assert.True(formalSieTopic.Examples.Count >= 55);
        Assert.Equal(18, formalSieTopic.RuleSummaries.Count);
        Assert.True(formalSieTopic.CommonMistakes.Count >= 22);
        Assert.True(formalSieTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", formalSieTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", formalSieTopic.PrerequisiteSlugs);
        Assert.Contains("a1-haben-in-praesens", formalSieTopic.PrerequisiteSlugs);
        Assert.Contains("a1-pronoun-and-verb-agreement", formalSieTopic.PrerequisiteSlugs);
        Assert.Contains("a1-yes-no-questions", formalSieTopic.PrerequisiteSlugs);
        Assert.Contains("a1-du-versus-sie-grammar-basics", formalSieTopic.RelatedTopicSlugs);
        Assert.Contains("a1-imperative-basics", formalSieTopic.RelatedTopicSlugs);
        Assert.Contains("a1-polite-requests-with-moechte", formalSieTopic.RelatedTopicSlugs);
        Assert.Contains("a2-imperative-formal-and-informal", formalSieTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel sieVersusSieTableSection = Assert.Single(
            formalSieTopic.Sections,
            section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel formalSieFormSection = Assert.Single(
            formalSieTopic.Sections,
            section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel formalSieWordOrderSection = Assert.Single(
            formalSieTopic.Sections,
            section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel formalSiePatternsSection = Assert.Single(
            formalSieTopic.Sections,
            section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(formalSieTopic.TitleLocalized.ContainsKey(language));
            Assert.True(formalSieTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(sieVersusSieTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(formalSieFormSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", sieVersusSieTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", formalSieFormSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", formalSieWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"callout\"", formalSieWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", formalSiePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(formalSieTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(formalSieTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(formalSieTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(formalSieTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(formalSieTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(formalSieTopic.Examples, example => example.GermanText == "Haben Sie Zeit?");
        Assert.Contains(formalSieTopic.Examples, example => example.GermanText == "Darf ich Ihnen helfen?");
        Assert.Contains(formalSieTopic.CommonMistakes, mistake => mistake.WrongText == "Sie bist Herr Weber?");
        Assert.Contains(formalSieTopic.CommonMistakes, mistake => mistake.WrongText == "Kann ich Sie helfen?");

        Assert.All(formalSieTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel duVersusSieTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-du-versus-sie-grammar-basics");
        Assert.Equal(1, duVersusSieTopic.ContentRevision);
        Assert.Equal("A1", duVersusSieTopic.CefrLevel);
        Assert.Equal("pronouns", duVersusSieTopic.GrammarCategory);
        Assert.Equal(10, duVersusSieTopic.Sections.Count);
        Assert.True(duVersusSieTopic.Examples.Count >= 60);
        Assert.Equal(18, duVersusSieTopic.RuleSummaries.Count);
        Assert.True(duVersusSieTopic.CommonMistakes.Count >= 25);
        Assert.True(duVersusSieTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", duVersusSieTopic.PrerequisiteSlugs);
        Assert.Contains("a1-formal-sie", duVersusSieTopic.PrerequisiteSlugs);
        Assert.Contains("a1-pronoun-and-verb-agreement", duVersusSieTopic.PrerequisiteSlugs);
        Assert.Contains("a1-yes-no-questions", duVersusSieTopic.PrerequisiteSlugs);
        Assert.Contains("a1-polite-requests-with-moechte", duVersusSieTopic.PrerequisiteSlugs);
        Assert.Contains("a1-imperative-basics", duVersusSieTopic.PrerequisiteSlugs);
        Assert.Contains("a2-imperative-formal-and-informal", duVersusSieTopic.RelatedTopicSlugs);
        Assert.Contains("a2-indirect-questions-introduction", duVersusSieTopic.RelatedTopicSlugs);
        Assert.Contains("a1-question-answer-sentence-patterns", duVersusSieTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel duSieComparisonTableSection = Assert.Single(
            duVersusSieTopic.Sections,
            section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel duSieStructureTableSection = Assert.Single(
            duVersusSieTopic.Sections,
            section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel duSieWordOrderTableSection = Assert.Single(
            duVersusSieTopic.Sections,
            section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel duSiePatternsTableSection = Assert.Single(
            duVersusSieTopic.Sections,
            section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(duVersusSieTopic.TitleLocalized.ContainsKey(language));
            Assert.True(duVersusSieTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(duSieComparisonTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(duSieStructureTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(duSieWordOrderTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(duSiePatternsTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", duSieComparisonTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", duSieStructureTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", duSieWordOrderTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", duSiePatternsTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(duVersusSieTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(duVersusSieTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(duVersusSieTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(duVersusSieTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(duVersusSieTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(duVersusSieTopic.Examples, example => example.GermanText == "Du bist mein Freund.");
        Assert.Contains(duVersusSieTopic.Examples, example => example.GermanText == "Wohnen Sie hier?");
        Assert.Contains(duVersusSieTopic.CommonMistakes, mistake => mistake.WrongText == "Du sind Herr Weber?");
        Assert.Contains(duVersusSieTopic.CommonMistakes, mistake => mistake.WrongText == "Kommst Sie morgen?");

        Assert.All(duVersusSieTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel basicSentenceNegationTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-basic-sentence-negation");
        Assert.Equal(1, basicSentenceNegationTopic.ContentRevision);
        Assert.Equal("A1", basicSentenceNegationTopic.CefrLevel);
        Assert.Equal("negation", basicSentenceNegationTopic.GrammarCategory);
        Assert.Equal(10, basicSentenceNegationTopic.Sections.Count);
        Assert.True(basicSentenceNegationTopic.Examples.Count >= 60);
        Assert.Equal(18, basicSentenceNegationTopic.RuleSummaries.Count);
        Assert.True(basicSentenceNegationTopic.CommonMistakes.Count >= 25);
        Assert.True(basicSentenceNegationTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-kein-versus-nicht-basics", basicSentenceNegationTopic.PrerequisiteSlugs);
        Assert.Contains("a1-simple-accusative-introduction", basicSentenceNegationTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", basicSentenceNegationTopic.PrerequisiteSlugs);
        Assert.Contains("a1-haben-in-praesens", basicSentenceNegationTopic.PrerequisiteSlugs);
        Assert.Contains("a1-regular-verbs-in-praesens", basicSentenceNegationTopic.PrerequisiteSlugs);
        Assert.Contains("a1-word-order-with-time-and-place", basicSentenceNegationTopic.PrerequisiteSlugs);
        Assert.Contains("a2-dass-clauses", basicSentenceNegationTopic.RelatedTopicSlugs);
        Assert.Contains("a2-denn-versus-weil", basicSentenceNegationTopic.RelatedTopicSlugs);
        Assert.Contains("a2-accusative-versus-dative-basics", basicSentenceNegationTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel negationReviewTableSection = Assert.Single(
            basicSentenceNegationTopic.Sections,
            section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel negationFormTableSection = Assert.Single(
            basicSentenceNegationTopic.Sections,
            section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel negationWordOrderTableSection = Assert.Single(
            basicSentenceNegationTopic.Sections,
            section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel negationPatternsTableSection = Assert.Single(
            basicSentenceNegationTopic.Sections,
            section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(basicSentenceNegationTopic.TitleLocalized.ContainsKey(language));
            Assert.True(basicSentenceNegationTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(negationReviewTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(negationFormTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(negationWordOrderTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(negationPatternsTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", negationReviewTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", negationFormTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", negationWordOrderTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", negationPatternsTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(basicSentenceNegationTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(basicSentenceNegationTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(basicSentenceNegationTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(basicSentenceNegationTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(basicSentenceNegationTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(basicSentenceNegationTopic.Examples, example => example.GermanText == "Ich bin nicht müde.");
        Assert.Contains(basicSentenceNegationTopic.Examples, example => example.GermanText == "Ich habe keinen Termin.");
        Assert.Contains(basicSentenceNegationTopic.CommonMistakes, mistake => mistake.WrongText == "Ich nicht komme.");
        Assert.Contains(basicSentenceNegationTopic.CommonMistakes, mistake => mistake.WrongText == "Ich habe nicht Termin.");

        Assert.All(basicSentenceNegationTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel questionAnswerPatternsTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-question-answer-sentence-patterns");
        Assert.Equal(1, questionAnswerPatternsTopic.ContentRevision);
        Assert.Equal("A1", questionAnswerPatternsTopic.CefrLevel);
        Assert.Equal("questions", questionAnswerPatternsTopic.GrammarCategory);
        Assert.Equal(10, questionAnswerPatternsTopic.Sections.Count);
        Assert.True(questionAnswerPatternsTopic.Examples.Count >= 70);
        Assert.Equal(18, questionAnswerPatternsTopic.RuleSummaries.Count);
        Assert.True(questionAnswerPatternsTopic.CommonMistakes.Count >= 25);
        Assert.True(questionAnswerPatternsTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-yes-no-questions", questionAnswerPatternsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-w-questions-wer-was-wo-wann-wie", questionAnswerPatternsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-sentence-negation", questionAnswerPatternsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-pronoun-and-verb-agreement", questionAnswerPatternsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-formal-sie", questionAnswerPatternsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-du-versus-sie-grammar-basics", questionAnswerPatternsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-appointment-phrases", questionAnswerPatternsTopic.RelatedTopicSlugs);
        Assert.Contains("a1-basic-location-phrases", questionAnswerPatternsTopic.RelatedTopicSlugs);
        Assert.Contains("a2-indirect-questions-introduction", questionAnswerPatternsTopic.RelatedTopicSlugs);
        Assert.Contains("b1-grammar-for-b1-speaking-exam", questionAnswerPatternsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel yesNoAnswerTableSection = Assert.Single(
            questionAnswerPatternsTopic.Sections,
            section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel wQuestionAnswerTableSection = Assert.Single(
            questionAnswerPatternsTopic.Sections,
            section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel formalSieAnswerTableSection = Assert.Single(
            questionAnswerPatternsTopic.Sections,
            section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel answerPatternsTableSection = Assert.Single(
            questionAnswerPatternsTopic.Sections,
            section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(questionAnswerPatternsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(questionAnswerPatternsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(yesNoAnswerTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(wQuestionAnswerTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(formalSieAnswerTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(answerPatternsTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", yesNoAnswerTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", wQuestionAnswerTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", formalSieAnswerTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", answerPatternsTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(questionAnswerPatternsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(questionAnswerPatternsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(questionAnswerPatternsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(questionAnswerPatternsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(questionAnswerPatternsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(questionAnswerPatternsTopic.Examples, example => example.GermanText == "Wohnst du hier? Ja, ich wohne hier.");
        Assert.Contains(questionAnswerPatternsTopic.Examples, example => example.GermanText == "Hast du Zeit? Nein, ich habe keine Zeit.");
        Assert.Contains(questionAnswerPatternsTopic.CommonMistakes, mistake => mistake.WrongText == "Wohnst du hier? Ja.");
        Assert.Contains(questionAnswerPatternsTopic.CommonMistakes, mistake => mistake.WrongText == "Wo wohnst du? Wo ich wohne in Berlin.");

        Assert.All(questionAnswerPatternsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel foodShoppingArticlesTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-articles-with-food-drinks-and-shopping-nouns");
        Assert.Equal(1, foodShoppingArticlesTopic.ContentRevision);
        Assert.Equal("A1", foodShoppingArticlesTopic.CefrLevel);
        Assert.Equal("articles", foodShoppingArticlesTopic.GrammarCategory);
        Assert.Equal(10, foodShoppingArticlesTopic.Sections.Count);
        Assert.True(foodShoppingArticlesTopic.Examples.Count >= 60);
        Assert.Equal(19, foodShoppingArticlesTopic.RuleSummaries.Count);
        Assert.True(foodShoppingArticlesTopic.CommonMistakes.Count >= 25);
        Assert.True(foodShoppingArticlesTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-definite-articles-der-die-das", foodShoppingArticlesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-indefinite-articles-ein-eine", foodShoppingArticlesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-noun-gender-basics", foodShoppingArticlesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-plural-basics", foodShoppingArticlesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-simple-accusative-introduction", foodShoppingArticlesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-polite-requests-with-moechte", foodShoppingArticlesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-appointment-phrases", foodShoppingArticlesTopic.RelatedTopicSlugs);
        Assert.Contains("a2-accusative-versus-dative-basics", foodShoppingArticlesTopic.RelatedTopicSlugs);
        Assert.Contains("a1-kein-versus-nicht-basics", foodShoppingArticlesTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel foodNounTableSection = Assert.Single(
            foodShoppingArticlesTopic.Sections,
            section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel drinkNounTableSection = Assert.Single(
            foodShoppingArticlesTopic.Sections,
            section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel shoppingAccusativeTableSection = Assert.Single(
            foodShoppingArticlesTopic.Sections,
            section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel shoppingPatternsTableSection = Assert.Single(
            foodShoppingArticlesTopic.Sections,
            section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(foodShoppingArticlesTopic.TitleLocalized.ContainsKey(language));
            Assert.True(foodShoppingArticlesTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(foodNounTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(drinkNounTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(shoppingAccusativeTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(shoppingPatternsTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", foodNounTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", drinkNounTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", shoppingAccusativeTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", shoppingPatternsTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(foodShoppingArticlesTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(foodShoppingArticlesTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(foodShoppingArticlesTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(foodShoppingArticlesTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(foodShoppingArticlesTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(foodShoppingArticlesTopic.Examples, example => example.GermanText == "Ich möchte einen Kaffee.");
        Assert.Contains(foodShoppingArticlesTopic.Examples, example => example.GermanText == "Ich kaufe ein Brot.");
        Assert.Contains(foodShoppingArticlesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich möchte ein Kaffee.");
        Assert.Contains(foodShoppingArticlesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich kaufe der Kaffee.");

        Assert.All(foodShoppingArticlesTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel basicLocationPhrasesTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-basic-location-phrases");
        Assert.Equal(1, basicLocationPhrasesTopic.ContentRevision);
        Assert.Equal("A1", basicLocationPhrasesTopic.CefrLevel);
        Assert.Equal("prepositions", basicLocationPhrasesTopic.GrammarCategory);
        Assert.Equal(10, basicLocationPhrasesTopic.Sections.Count);
        Assert.True(basicLocationPhrasesTopic.Examples.Count >= 60);
        Assert.Equal(18, basicLocationPhrasesTopic.RuleSummaries.Count);
        Assert.True(basicLocationPhrasesTopic.CommonMistakes.Count >= 22);
        Assert.True(basicLocationPhrasesTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-basic-prepositions-in-aus-nach-bei", basicLocationPhrasesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-word-order-with-time-and-place", basicLocationPhrasesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-w-questions-wer-was-wo-wann-wie", basicLocationPhrasesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", basicLocationPhrasesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-regular-verbs-in-praesens", basicLocationPhrasesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-appointment-phrases", basicLocationPhrasesTopic.RelatedTopicSlugs);
        Assert.Contains("a2-wechselpraepositionen-introduction", basicLocationPhrasesTopic.RelatedTopicSlugs);
        Assert.Contains("a2-prepositions-with-dative", basicLocationPhrasesTopic.RelatedTopicSlugs);
        Assert.Contains("a2-prepositions-with-accusative", basicLocationPhrasesTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel imInDerTableSection = Assert.Single(
            basicLocationPhrasesTopic.Sections,
            section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel locationCaseTableSection = Assert.Single(
            basicLocationPhrasesTopic.Sections,
            section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel locationComparisonTableSection = Assert.Single(
            basicLocationPhrasesTopic.Sections,
            section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel locationPatternTableSection = Assert.Single(
            basicLocationPhrasesTopic.Sections,
            section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(basicLocationPhrasesTopic.TitleLocalized.ContainsKey(language));
            Assert.True(basicLocationPhrasesTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(imInDerTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(locationCaseTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(locationComparisonTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(locationPatternTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", imInDerTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", locationCaseTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", locationComparisonTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", locationPatternTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(basicLocationPhrasesTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(basicLocationPhrasesTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(basicLocationPhrasesTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(basicLocationPhrasesTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(basicLocationPhrasesTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(basicLocationPhrasesTopic.Examples, example => example.GermanText == "Ich wohne in Berlin.");
        Assert.Contains(basicLocationPhrasesTopic.Examples, example => example.GermanText == "Ich bin zu Hause.");
        Assert.Contains(basicLocationPhrasesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich bin nach Hause.");
        Assert.Contains(basicLocationPhrasesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich bin in Schule.");

        Assert.All(basicLocationPhrasesTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel basicAppointmentPhrasesTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-basic-appointment-phrases");
        Assert.Equal(1, basicAppointmentPhrasesTopic.ContentRevision);
        Assert.Equal("A1", basicAppointmentPhrasesTopic.CefrLevel);
        Assert.Equal("tenses", basicAppointmentPhrasesTopic.GrammarCategory);
        Assert.Equal(10, basicAppointmentPhrasesTopic.Sections.Count);
        Assert.True(basicAppointmentPhrasesTopic.Examples.Count >= 70);
        Assert.Equal(18, basicAppointmentPhrasesTopic.RuleSummaries.Count);
        Assert.True(basicAppointmentPhrasesTopic.CommonMistakes.Count >= 25);
        Assert.True(basicAppointmentPhrasesTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-haben-in-praesens", basicAppointmentPhrasesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-polite-requests-with-moechte", basicAppointmentPhrasesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-simple-accusative-introduction", basicAppointmentPhrasesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-numbers-and-grammar-use", basicAppointmentPhrasesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-time-expressions-heute-morgen-gestern", basicAppointmentPhrasesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-word-order-with-time-and-place", basicAppointmentPhrasesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-formal-sie", basicAppointmentPhrasesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-grammar-for-appointments", basicAppointmentPhrasesTopic.RelatedTopicSlugs);
        Assert.Contains("a2-indirect-questions-introduction", basicAppointmentPhrasesTopic.RelatedTopicSlugs);
        Assert.Contains("a2-polite-forms-with-wuerde", basicAppointmentPhrasesTopic.RelatedTopicSlugs);
        Assert.Contains("a1-question-answer-sentence-patterns", basicAppointmentPhrasesTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel appointmentStructureTableSection = Assert.Single(
            basicAppointmentPhrasesTopic.Sections,
            section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel appointmentWordOrderCaseTableSection = Assert.Single(
            basicAppointmentPhrasesTopic.Sections,
            section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel appointmentComparisonTableSection = Assert.Single(
            basicAppointmentPhrasesTopic.Sections,
            section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel appointmentPhraseTableSection = Assert.Single(
            basicAppointmentPhrasesTopic.Sections,
            section => section.SectionKey == "common-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(basicAppointmentPhrasesTopic.TitleLocalized.ContainsKey(language));
            Assert.True(basicAppointmentPhrasesTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(appointmentStructureTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(appointmentWordOrderCaseTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(appointmentComparisonTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(appointmentPhraseTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", appointmentStructureTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", appointmentWordOrderCaseTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", appointmentComparisonTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", appointmentPhraseTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(basicAppointmentPhrasesTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(basicAppointmentPhrasesTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(basicAppointmentPhrasesTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(basicAppointmentPhrasesTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(basicAppointmentPhrasesTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(basicAppointmentPhrasesTopic.Examples, example => example.GermanText == "Ich habe einen Termin.");
        Assert.Contains(basicAppointmentPhrasesTopic.Examples, example => example.GermanText == "Der Termin ist am Montag um 10 Uhr.");
        Assert.Contains(basicAppointmentPhrasesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich habe ein Termin.");
        Assert.Contains(basicAppointmentPhrasesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich komme am 10 Uhr.");

        Assert.All(basicAppointmentPhrasesTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel commonA1MistakesTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-common-a1-grammar-mistakes");
        Assert.Equal(1, commonA1MistakesTopic.ContentRevision);
        Assert.Equal("A1", commonA1MistakesTopic.CefrLevel);
        Assert.Equal("verbs", commonA1MistakesTopic.GrammarCategory);
        Assert.Equal(10, commonA1MistakesTopic.Sections.Count);
        Assert.True(commonA1MistakesTopic.Examples.Count >= 45);
        Assert.True(commonA1MistakesTopic.CommonMistakes.Count >= 50);
        Assert.True(commonA1MistakesTopic.CommonMistakes.Count >= 80);
        Assert.True(commonA1MistakesTopic.LinkedWords.Count >= 50);
        Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", commonA1MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", commonA1MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-haben-in-praesens", commonA1MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-regular-verbs-in-praesens", commonA1MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-verb-position-in-simple-sentences", commonA1MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-yes-no-questions", commonA1MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-w-questions-wer-was-wo-wann-wie", commonA1MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-definite-articles-der-die-das", commonA1MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-indefinite-articles-ein-eine", commonA1MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-noun-gender-basics", commonA1MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-plural-basics", commonA1MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-nominative-case", commonA1MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-simple-accusative-introduction", commonA1MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-kein-versus-nicht-basics", commonA1MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-sentence-negation", commonA1MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-a1-grammar-review-map", commonA1MistakesTopic.RelatedTopicSlugs);
        Assert.Contains("a2-common-a2-mistakes", commonA1MistakesTopic.RelatedTopicSlugs);
        Assert.Contains("a2-a2-grammar-review-map", commonA1MistakesTopic.RelatedTopicSlugs);
        Assert.True(commonA1MistakesTopic.RuleSummaries.Count >= 18);

        ParsedGrammarSectionModel mistakeCategoryTableSection = Assert.Single(
            commonA1MistakesTopic.Sections,
            section => section.SectionKey == "why-mistakes-are-normal");
        ParsedGrammarSectionModel wrongCorrectTableSection = Assert.Single(
            commonA1MistakesTopic.Sections,
            section => section.SectionKey == "mistake-type-verb-endings");
        ParsedGrammarSectionModel correctionChecklistTableSection = Assert.Single(
            commonA1MistakesTopic.Sections,
            section => section.SectionKey == "correction-strategy");
        Assert.All(languages, language =>
        {
            Assert.True(commonA1MistakesTopic.TitleLocalized.ContainsKey(language));
            Assert.True(commonA1MistakesTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(mistakeCategoryTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(wrongCorrectTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(correctionChecklistTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", mistakeCategoryTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", wrongCorrectTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", correctionChecklistTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(commonA1MistakesTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(commonA1MistakesTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(commonA1MistakesTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(commonA1MistakesTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(commonA1MistakesTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(commonA1MistakesTopic.Examples, example => example.GermanText == "Heute lerne ich Deutsch.");
        Assert.Contains(commonA1MistakesTopic.Examples, example => example.GermanText == "Ich habe einen Termin.");
        Assert.Contains(commonA1MistakesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich lernst Deutsch.");
        Assert.Contains(commonA1MistakesTopic.CommonMistakes, mistake => mistake.WrongText == "Heute ich lerne Deutsch.");

        Assert.All(commonA1MistakesTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel a1ReviewMapTopic = Assert.Single(
            parsedPackage.GrammarTopics,
            topic => topic.Slug == "a1-a1-grammar-review-map");
        Assert.Equal(1, a1ReviewMapTopic.ContentRevision);
        Assert.Equal("A1", a1ReviewMapTopic.CefrLevel);
        Assert.Equal("word-order", a1ReviewMapTopic.GrammarCategory);
        Assert.Equal(12, a1ReviewMapTopic.Sections.Count);
        Assert.True(a1ReviewMapTopic.Examples.Count >= 80);
        Assert.True(a1ReviewMapTopic.CommonMistakes.Count >= 40);
        Assert.True(a1ReviewMapTopic.RuleSummaries.Count >= 25);
        Assert.Equal(77, a1ReviewMapTopic.LinkedWords.Count);
        Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", a1ReviewMapTopic.PrerequisiteSlugs);
        Assert.Contains("a1-common-a1-grammar-mistakes", a1ReviewMapTopic.PrerequisiteSlugs);
        Assert.Contains("a2-a2-grammar-review-map", a1ReviewMapTopic.RelatedTopicSlugs);
        Assert.Contains("a2-perfekt-with-haben", a1ReviewMapTopic.RelatedTopicSlugs);
        Assert.Contains("a2-dative-case-basics", a1ReviewMapTopic.RelatedTopicSlugs);
        Assert.Contains("a2-dass-clauses", a1ReviewMapTopic.RelatedTopicSlugs);
        Assert.Contains("a2-weil-clauses", a1ReviewMapTopic.RelatedTopicSlugs);
        Assert.Contains("a2-adjective-endings-introduction", a1ReviewMapTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel reviewOrderTableSection = Assert.Single(
            a1ReviewMapTopic.Sections,
            section => section.SectionKey == "a1-core-path");
        ParsedGrammarSectionModel sentenceMapTableSection = Assert.Single(
            a1ReviewMapTopic.Sections,
            section => section.SectionKey == "sentence-building-map");
        ParsedGrammarSectionModel questionMapTableSection = Assert.Single(
            a1ReviewMapTopic.Sections,
            section => section.SectionKey == "question-map");
        ParsedGrammarSectionModel selfChecklistTableSection = Assert.Single(
            a1ReviewMapTopic.Sections,
            section => section.SectionKey == "self-checklist");
        Assert.All(languages, language =>
        {
            Assert.True(a1ReviewMapTopic.TitleLocalized.ContainsKey(language));
            Assert.True(a1ReviewMapTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.True(reviewOrderTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(sentenceMapTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(questionMapTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.True(selfChecklistTableSection.LocalizedBlocksJson.ContainsKey(language));
            Assert.Contains("\"table\"", reviewOrderTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", sentenceMapTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", questionMapTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", selfChecklistTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(a1ReviewMapTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(a1ReviewMapTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(a1ReviewMapTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(a1ReviewMapTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(a1ReviewMapTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(a1ReviewMapTopic.Examples, example => example.GermanText == "Ich heiße Sara.");
        Assert.Contains(a1ReviewMapTopic.Examples, example => example.GermanText == "Heute lerne ich Deutsch.");
        Assert.Contains(a1ReviewMapTopic.CommonMistakes, mistake => mistake.WrongText == "Ich bist neu.");
        Assert.Contains(a1ReviewMapTopic.CommonMistakes, mistake => mistake.WrongText == "Heute ich lerne Deutsch.");

        Assert.All(a1ReviewMapTopic.PrerequisiteSlugs, prerequisiteSlug =>
            Assert.Contains(parsedPackage.GrammarTopics, topic => topic.Slug == prerequisiteSlug && topic.CefrLevel == "A1"));

        Assert.All(a1ReviewMapTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public async Task ParseAsync_ShouldParseOfficialA2GrammarCoreContract()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection()
            .AddContentOpsInfrastructure()
            .BuildServiceProvider();

        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();
        string repositoryRoot = ResolveRepositoryRoot();
        string packagePath = Path.Combine(repositoryRoot, "content", "learning-portal", "grammar", "packages", "grammar-a2-core-v1.json");

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            await File.ReadAllTextAsync(packagePath),
            CancellationToken.None);

        Assert.Equal(40, parsedPackage.GrammarTopics.Count);
        ParsedGrammarTopicModel topic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-perfekt-with-haben");
        Assert.Equal("a2-perfekt-with-haben", topic.Slug);
        Assert.Equal(1, topic.ContentRevision);
        Assert.Equal("A2", topic.CefrLevel);
        Assert.Equal("tenses", topic.GrammarCategory);
        Assert.Equal(10, topic.Sections.Count);
        Assert.True(topic.Examples.Count >= 90);
        Assert.True(topic.CommonMistakes.Count >= 35);
        Assert.True(topic.RuleSummaries.Count >= 20);
        Assert.True(topic.LinkedWords.Count >= 70);
        Assert.Contains("a1-haben-in-praesens", topic.PrerequisiteSlugs);
        Assert.Contains("a1-regular-verbs-in-praesens", topic.PrerequisiteSlugs);
        Assert.Contains("a1-verb-position-in-simple-sentences", topic.PrerequisiteSlugs);
        Assert.Contains("a1-time-expressions-heute-morgen-gestern", topic.PrerequisiteSlugs);
        Assert.Contains("a1-question-answer-sentence-patterns", topic.PrerequisiteSlugs);
        Assert.Contains("a1-separable-verbs-introduction", topic.PrerequisiteSlugs);
        Assert.Contains("a2-perfekt-with-sein", topic.RelatedTopicSlugs);
        Assert.Contains("a2-common-irregular-participles", topic.RelatedTopicSlugs);
        Assert.Contains("a2-praeteritum-of-sein-and-haben", topic.RelatedTopicSlugs);
        Assert.Contains("a2-separable-verbs-in-perfekt", topic.RelatedTopicSlugs);
        Assert.Contains("b1-describing-experiences-in-the-past", topic.RelatedTopicSlugs);

        string[] languages = ["en", "fa", "ar", "tr", "ru", "ckb", "kmr", "pl", "ro", "sq"];
        ParsedGrammarSectionModel structureSection = Assert.Single(topic.Sections, section => section.SectionKey == "two-part-structure");
        ParsedGrammarSectionModel habenTableSection = Assert.Single(topic.Sections, section => section.SectionKey == "haben-conjugation-review");
        ParsedGrammarSectionModel participleTableSection = Assert.Single(topic.Sections, section => section.SectionKey == "regular-participles");
        ParsedGrammarSectionModel positionSection = Assert.Single(topic.Sections, section => section.SectionKey == "sentence-position");
        Assert.All(languages, language =>
        {
            Assert.True(topic.TitleLocalized.ContainsKey(language));
            Assert.True(topic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(topic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(topic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.Contains("\"table\"", structureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", habenTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", participleTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", positionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(topic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(topic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(topic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(topic.Examples, example => example.GermanText == "Ich habe Deutsch gelernt.");
        Assert.Contains(topic.Examples, example => example.GermanText == "Wir haben den Termin bestätigt.");
        Assert.Contains(topic.CommonMistakes, mistake => mistake.WrongText == "Ich habe gelernt Deutsch.");
        Assert.Contains(topic.CommonMistakes, mistake => mistake.WrongText == "Ich habe gestern lernen.");
        Assert.DoesNotContain(topic.Examples, example => example.Translations.Any(translation => translation.Text.StartsWith("Review example:", StringComparison.Ordinal)));
        Assert.All(topic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel seinTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-perfekt-with-sein");
        Assert.Equal(1, seinTopic.ContentRevision);
        Assert.Equal("A2", seinTopic.CefrLevel);
        Assert.Equal("tenses", seinTopic.GrammarCategory);
        Assert.Equal(10, seinTopic.Sections.Count);
        Assert.True(seinTopic.Examples.Count >= 90);
        Assert.True(seinTopic.CommonMistakes.Count >= 35);
        Assert.True(seinTopic.RuleSummaries.Count >= 20);
        Assert.True(seinTopic.LinkedWords.Count >= 70);
        Assert.Contains("a2-perfekt-with-haben", seinTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", seinTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-prepositions-in-aus-nach-bei", seinTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-location-phrases", seinTopic.PrerequisiteSlugs);
        Assert.Contains("a1-time-expressions-heute-morgen-gestern", seinTopic.PrerequisiteSlugs);
        Assert.Contains("a1-separable-verbs-introduction", seinTopic.PrerequisiteSlugs);
        Assert.Contains("a2-common-irregular-participles", seinTopic.RelatedTopicSlugs);
        Assert.Contains("a2-separable-verbs-in-perfekt", seinTopic.RelatedTopicSlugs);
        Assert.Contains("a2-praeteritum-of-sein-and-haben", seinTopic.RelatedTopicSlugs);
        Assert.Contains("b1-describing-experiences-in-the-past", seinTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel seinConjugationSection = Assert.Single(seinTopic.Sections, section => section.SectionKey == "sein-conjugation-review");
        ParsedGrammarSectionModel movementVerbsSection = Assert.Single(seinTopic.Sections, section => section.SectionKey == "movement-verbs");
        ParsedGrammarSectionModel changeOfStateSection = Assert.Single(seinTopic.Sections, section => section.SectionKey == "change-of-state-verbs");
        ParsedGrammarSectionModel seinPositionSection = Assert.Single(seinTopic.Sections, section => section.SectionKey == "sentence-position");
        ParsedGrammarSectionModel habenSeinComparisonSection = Assert.Single(seinTopic.Sections, section => section.SectionKey == "haben-vs-sein-comparison");
        ParsedGrammarSectionModel seinVerbsTableSection = Assert.Single(seinTopic.Sections, section => section.SectionKey == "common-sein-verbs-table");
        ParsedGrammarSectionModel questionsSection = Assert.Single(seinTopic.Sections, section => section.SectionKey == "questions-with-sein-perfekt");
        Assert.All(languages, language =>
        {
            Assert.True(seinTopic.TitleLocalized.ContainsKey(language));
            Assert.True(seinTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(seinTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(seinTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.Contains("\"table\"", seinConjugationSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", movementVerbsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", changeOfStateSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", seinPositionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", habenSeinComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", seinVerbsTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", questionsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(seinTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(seinTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(seinTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(seinTopic.Examples, example => example.GermanText == "Ich bin nach Hause gegangen.");
        Assert.Contains(seinTopic.Examples, example => example.GermanText == "Das Wetter ist besser geworden.");
        Assert.Contains(seinTopic.CommonMistakes, mistake => mistake.WrongText == "Ich habe nach Hause gegangen.");
        Assert.Contains(seinTopic.CommonMistakes, mistake => mistake.WrongText == "Ich bin Deutsch gelernt.");

        Assert.All(seinTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel irregularTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-common-irregular-participles");
        Assert.Equal(1, irregularTopic.ContentRevision);
        Assert.Equal("A2", irregularTopic.CefrLevel);
        Assert.Equal("tenses", irregularTopic.GrammarCategory);
        Assert.Equal(10, irregularTopic.Sections.Count);
        Assert.True(irregularTopic.Examples.Count >= 90);
        Assert.True(irregularTopic.CommonMistakes.Count >= 35);
        Assert.True(irregularTopic.RuleSummaries.Count >= 20);
        Assert.True(irregularTopic.LinkedWords.Count >= 70);
        Assert.Contains("a2-perfekt-with-haben", irregularTopic.PrerequisiteSlugs);
        Assert.Contains("a2-perfekt-with-sein", irregularTopic.PrerequisiteSlugs);
        Assert.Contains("a1-haben-in-praesens", irregularTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", irregularTopic.PrerequisiteSlugs);
        Assert.Contains("a2-separable-verbs-in-perfekt", irregularTopic.RelatedTopicSlugs);
        Assert.Contains("a2-praeteritum-of-sein-and-haben", irregularTopic.RelatedTopicSlugs);
        Assert.Contains("b1-describing-experiences-in-the-past", irregularTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel regularVsIrregularSection = Assert.Single(irregularTopic.Sections, section => section.SectionKey == "regular-vs-irregular-review");
        ParsedGrammarSectionModel habenIrregularsSection = Assert.Single(irregularTopic.Sections, section => section.SectionKey == "common-haben-irregulars");
        ParsedGrammarSectionModel seinIrregularsSection = Assert.Single(irregularTopic.Sections, section => section.SectionKey == "common-sein-irregulars");
        ParsedGrammarSectionModel auxiliarySection = Assert.Single(irregularTopic.Sections, section => section.SectionKey == "learn-with-auxiliary");
        ParsedGrammarSectionModel everydayPatternsSection = Assert.Single(irregularTopic.Sections, section => section.SectionKey == "useful-everyday-patterns");
        ParsedGrammarSectionModel questionPatternsSection = Assert.Single(irregularTopic.Sections, section => section.SectionKey == "question-patterns");
        Assert.All(languages, language =>
        {
            Assert.True(irregularTopic.TitleLocalized.ContainsKey(language));
            Assert.True(irregularTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(irregularTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(irregularTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.Contains("\"table\"", regularVsIrregularSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", habenIrregularsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", seinIrregularsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", auxiliarySection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", everydayPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", questionPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(irregularTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(irregularTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(irregularTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(irregularTopic.Examples, example => example.GermanText == "Ich habe Brot gegessen.");
        Assert.Contains(irregularTopic.Examples, example => example.GermanText == "Das Wetter ist besser geworden.");
        Assert.Contains(irregularTopic.Examples, example => example.GermanText == "Hast du schon gegessen?");
        Assert.Contains(irregularTopic.CommonMistakes, mistake => mistake.WrongText == "Ich habe getrinkt.");
        Assert.Contains(irregularTopic.CommonMistakes, mistake => mistake.WrongText == "Ich habe gegangen.");
        Assert.Contains(irregularTopic.CommonMistakes, mistake => mistake.WrongText == "Ich habe gelest.");

        Assert.All(irregularTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel praeteritumTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-praeteritum-of-sein-and-haben");
        Assert.Equal(1, praeteritumTopic.ContentRevision);
        Assert.Equal("A2", praeteritumTopic.CefrLevel);
        Assert.Equal("tenses", praeteritumTopic.GrammarCategory);
        Assert.Equal(10, praeteritumTopic.Sections.Count);
        Assert.True(praeteritumTopic.Examples.Count >= 90);
        Assert.True(praeteritumTopic.CommonMistakes.Count >= 35);
        Assert.True(praeteritumTopic.RuleSummaries.Count >= 20);
        Assert.True(praeteritumTopic.LinkedWords.Count >= 70);
        Assert.Contains("a1-sein-in-praesens", praeteritumTopic.PrerequisiteSlugs);
        Assert.Contains("a1-haben-in-praesens", praeteritumTopic.PrerequisiteSlugs);
        Assert.Contains("a2-perfekt-with-haben", praeteritumTopic.PrerequisiteSlugs);
        Assert.Contains("a2-perfekt-with-sein", praeteritumTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-sentence-negation", praeteritumTopic.PrerequisiteSlugs);
        Assert.Contains("a2-common-irregular-participles", praeteritumTopic.RelatedTopicSlugs);
        Assert.Contains("b1-describing-experiences-in-the-past", praeteritumTopic.RelatedTopicSlugs);
        Assert.Contains("b1-modal-verbs-in-the-past", praeteritumTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel seinPastSection = Assert.Single(praeteritumTopic.Sections, section => section.SectionKey == "sein-present-vs-past");
        ParsedGrammarSectionModel habenPastSection = Assert.Single(praeteritumTopic.Sections, section => section.SectionKey == "haben-present-vs-past");
        ParsedGrammarSectionModel questionSection = Assert.Single(praeteritumTopic.Sections, section => section.SectionKey == "questions-with-war-and-hatte");
        ParsedGrammarSectionModel perfektComparisonSection = Assert.Single(praeteritumTopic.Sections, section => section.SectionKey == "war-hatte-vs-perfekt");
        ParsedGrammarSectionModel negationSection = Assert.Single(praeteritumTopic.Sections, section => section.SectionKey == "negative-sentences");
        Assert.All(languages, language =>
        {
            Assert.True(praeteritumTopic.TitleLocalized.ContainsKey(language));
            Assert.True(praeteritumTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(praeteritumTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(praeteritumTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.Contains("\"table\"", seinPastSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", habenPastSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", questionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", perfektComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", negationSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(praeteritumTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(praeteritumTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(praeteritumTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(praeteritumTopic.Examples, example => example.GermanText == "Ich war krank.");
        Assert.Contains(praeteritumTopic.Examples, example => example.GermanText == "Gestern war ich krank.");
        Assert.Contains(praeteritumTopic.Examples, example => example.GermanText == "Ich hatte einen Termin.");
        Assert.Contains(praeteritumTopic.Examples, example => example.GermanText == "Hattest du gestern Zeit?");
        Assert.Contains(praeteritumTopic.CommonMistakes, mistake => mistake.WrongText == "Ich bin gestern krank.");
        Assert.Contains(praeteritumTopic.CommonMistakes, mistake => mistake.WrongText == "Ich war einen Termin.");
        Assert.Contains(praeteritumTopic.CommonMistakes, mistake => mistake.WrongText == "Gestern ich war krank.");

        Assert.All(praeteritumTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel modalTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-modal-verbs-in-more-detail");
        Assert.Equal(1, modalTopic.ContentRevision);
        Assert.Equal("A2", modalTopic.CefrLevel);
        Assert.Equal("modal-verbs", modalTopic.GrammarCategory);
        Assert.Equal(11, modalTopic.Sections.Count);
        Assert.True(modalTopic.Examples.Count >= 90);
        Assert.True(modalTopic.CommonMistakes.Count >= 35);
        Assert.True(modalTopic.RuleSummaries.Count >= 20);
        Assert.True(modalTopic.LinkedWords.Count >= 70);
        Assert.Contains("a1-simple-modal-verbs-koennen-muessen-wollen", modalTopic.PrerequisiteSlugs);
        Assert.Contains("a1-polite-requests-with-moechte", modalTopic.PrerequisiteSlugs);
        Assert.Contains("a1-verb-position-in-simple-sentences", modalTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-sentence-negation", modalTopic.PrerequisiteSlugs);
        Assert.Contains("a1-yes-no-questions", modalTopic.PrerequisiteSlugs);
        Assert.Contains("a2-polite-forms-with-wuerde", modalTopic.RelatedTopicSlugs);
        Assert.Contains("b1-modal-verbs-in-the-past", modalTopic.RelatedTopicSlugs);
        Assert.Contains("b2-expressing-probability", modalTopic.RelatedTopicSlugs);
        Assert.Contains("b2-expressing-doubt", modalTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel modalOverviewSection = Assert.Single(modalTopic.Sections, section => section.SectionKey == "modal-verbs-overview-table");
        ParsedGrammarSectionModel modalQuestionsSection = Assert.Single(modalTopic.Sections, section => section.SectionKey == "modal-questions");
        ParsedGrammarSectionModel modalWordOrderSection = Assert.Single(modalTopic.Sections, section => section.SectionKey == "word-order-with-modals");
        Assert.All(languages, language =>
        {
            Assert.True(modalTopic.TitleLocalized.ContainsKey(language));
            Assert.True(modalTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(modalTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(modalTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.Contains("\"table\"", modalOverviewSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", modalQuestionsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", modalWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(modalTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(modalTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(modalTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(modalTopic.Examples, example => example.GermanText == "Ich kann Deutsch sprechen.");
        Assert.Contains(modalTopic.Examples, example => example.GermanText == "Heute muss ich arbeiten.");
        Assert.Contains(modalTopic.Examples, example => example.GermanText == "Können Sie mir bitte helfen?");
        Assert.Contains(modalTopic.CommonMistakes, mistake => mistake.WrongText == "Ich kann Deutsch spreche.");
        Assert.Contains(modalTopic.CommonMistakes, mistake => mistake.WrongText == "Ich muss zu arbeiten.");
        Assert.Contains(modalTopic.CommonMistakes, mistake => mistake.WrongText == "Heute ich muss arbeiten.");

        Assert.All(modalTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel dativeTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-dative-case-basics");
        Assert.Equal(1, dativeTopic.ContentRevision);
        Assert.Equal("A2", dativeTopic.CefrLevel);
        Assert.Equal("dative", dativeTopic.GrammarCategory);
        Assert.Equal(10, dativeTopic.Sections.Count);
        Assert.True(dativeTopic.Examples.Count >= 90);
        Assert.True(dativeTopic.CommonMistakes.Count >= 35);
        Assert.True(dativeTopic.RuleSummaries.Count >= 20);
        Assert.True(dativeTopic.LinkedWords.Count >= 70);
        Assert.Contains("a1-nominative-case", dativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-simple-accusative-introduction", dativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-definite-articles-der-die-das", dativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-indefinite-articles-ein-eine", dativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-noun-gender-basics", dativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-plural-basics", dativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-prepositions-in-aus-nach-bei", dativeTopic.PrerequisiteSlugs);
        Assert.Contains("a2-accusative-versus-dative-basics", dativeTopic.RelatedTopicSlugs);
        Assert.Contains("a2-dative-pronouns", dativeTopic.RelatedTopicSlugs);
        Assert.Contains("a2-prepositions-with-dative", dativeTopic.RelatedTopicSlugs);
        Assert.Contains("a2-wechselpraepositionen-introduction", dativeTopic.RelatedTopicSlugs);
        Assert.Contains("b1-b1-case-review", dativeTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel dativeArticleSection = Assert.Single(dativeTopic.Sections, section => section.SectionKey == "dative-article-table");
        ParsedGrammarSectionModel dativeGivingSection = Assert.Single(dativeTopic.Sections, section => section.SectionKey == "dative-with-giving");
        ParsedGrammarSectionModel dativePrepositionSection = Assert.Single(dativeTopic.Sections, section => section.SectionKey == "dative-after-common-prepositions");
        ParsedGrammarSectionModel dativeComparisonSection = Assert.Single(dativeTopic.Sections, section => section.SectionKey == "dative-vs-nominative-preview");
        Assert.All(languages, language =>
        {
            Assert.True(dativeTopic.TitleLocalized.ContainsKey(language));
            Assert.True(dativeTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(dativeTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(dativeTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.Contains("\"table\"", dativeArticleSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", dativeGivingSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", dativePrepositionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", dativeComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(dativeTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(dativeTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(dativeTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(dativeTopic.CommonMistakes, mistake => mistake.WrongText == "Ich helfe der Mann.");
        Assert.Contains(dativeTopic.CommonMistakes, mistake => mistake.WrongText == "Ich fahre mit der Bus.");
        Assert.Contains(dativeTopic.Examples, example => example.GermanText == "Ich gebe dem Mann das Buch.");
        Assert.Contains(dativeTopic.Examples, example => example.GermanText == "Wir helfen den Kindern.");
        Assert.Contains(dativeTopic.Examples, example => example.GermanText == "Kannst du mir helfen?");
        Assert.All(dativeTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel accusativeDativeTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-accusative-versus-dative-basics");
        Assert.Equal(1, accusativeDativeTopic.ContentRevision);
        Assert.Equal("A2", accusativeDativeTopic.CefrLevel);
        Assert.Equal("cases", accusativeDativeTopic.GrammarCategory);
        Assert.Equal(11, accusativeDativeTopic.Sections.Count);
        Assert.True(accusativeDativeTopic.Examples.Count >= 90);
        Assert.True(accusativeDativeTopic.CommonMistakes.Count >= 35);
        Assert.True(accusativeDativeTopic.RuleSummaries.Count >= 20);
        Assert.True(accusativeDativeTopic.LinkedWords.Count >= 70);
        Assert.Contains("a1-nominative-case", accusativeDativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-simple-accusative-introduction", accusativeDativeTopic.PrerequisiteSlugs);
        Assert.Contains("a2-dative-case-basics", accusativeDativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-definite-articles-der-die-das", accusativeDativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-indefinite-articles-ein-eine", accusativeDativeTopic.PrerequisiteSlugs);
        Assert.Contains("a2-dative-pronouns", accusativeDativeTopic.RelatedTopicSlugs);
        Assert.Contains("a2-accusative-pronouns", accusativeDativeTopic.RelatedTopicSlugs);
        Assert.Contains("a2-possessive-pronouns-in-cases", accusativeDativeTopic.RelatedTopicSlugs);
        Assert.Contains("a2-prepositions-with-dative", accusativeDativeTopic.RelatedTopicSlugs);
        Assert.Contains("a2-prepositions-with-accusative", accusativeDativeTopic.RelatedTopicSlugs);
        Assert.Contains("b1-b1-case-review", accusativeDativeTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel helperQuestionsSection = Assert.Single(accusativeDativeTopic.Sections, section => section.SectionKey == "helper-questions-table");
        ParsedGrammarSectionModel articleComparisonSection = Assert.Single(accusativeDativeTopic.Sections, section => section.SectionKey == "article-comparison-table");
        ParsedGrammarSectionModel bothCasesSection = Assert.Single(accusativeDativeTopic.Sections, section => section.SectionKey == "sentence-with-both-cases");
        ParsedGrammarSectionModel practiceAdviceSection = Assert.Single(accusativeDativeTopic.Sections, section => section.SectionKey == "practice-advice");
        Assert.All(languages, language =>
        {
            Assert.True(accusativeDativeTopic.TitleLocalized.ContainsKey(language));
            Assert.True(accusativeDativeTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(accusativeDativeTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(accusativeDativeTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.Contains("\"table\"", helperQuestionsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", articleComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", bothCasesSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", practiceAdviceSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(accusativeDativeTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(accusativeDativeTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(accusativeDativeTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(accusativeDativeTopic.Examples, example => example.GermanText == "Ich gebe dem Mann den Schlüssel.");
        Assert.Contains(accusativeDativeTopic.Examples, example => example.GermanText == "Wir zeigen dem Kind das Buch.");
        Assert.Contains(accusativeDativeTopic.CommonMistakes, mistake => mistake.WrongText == "Ich gebe den Mann den Schlüssel.");
        Assert.Contains(accusativeDativeTopic.CommonMistakes, mistake => mistake.WrongText == "Ich fahre mit den Bus.");
        Assert.All(accusativeDativeTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel dativePronounsTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-dative-pronouns");
        Assert.Equal(1, dativePronounsTopic.ContentRevision);
        Assert.Equal("A2", dativePronounsTopic.CefrLevel);
        Assert.Equal("pronouns", dativePronounsTopic.GrammarCategory);
        Assert.Equal(12, dativePronounsTopic.Sections.Count);
        Assert.True(dativePronounsTopic.Examples.Count >= 90);
        Assert.True(dativePronounsTopic.CommonMistakes.Count >= 35);
        Assert.True(dativePronounsTopic.RuleSummaries.Count >= 20);
        Assert.Equal(75, dativePronounsTopic.LinkedWords.Count);
        Assert.Contains("a2-dative-case-basics", dativePronounsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-accusative-versus-dative-basics", dativePronounsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", dativePronounsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-formal-sie", dativePronounsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-du-versus-sie-grammar-basics", dativePronounsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-accusative-pronouns", dativePronounsTopic.RelatedTopicSlugs);
        Assert.Contains("a2-possessive-pronouns-in-cases", dativePronounsTopic.RelatedTopicSlugs);
        Assert.Contains("a2-prepositions-with-dative", dativePronounsTopic.RelatedTopicSlugs);
        Assert.Contains("b1-reflexive-verbs-with-dative-and-accusative", dativePronounsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel dativePronounTableSection = Assert.Single(dativePronounsTopic.Sections, section => section.SectionKey == "dative-pronoun-table");
        ParsedGrammarSectionModel replacingDativeNounsSection = Assert.Single(dativePronounsTopic.Sections, section => section.SectionKey == "replacing-dative-nouns");
        ParsedGrammarSectionModel formalIhnenSection = Assert.Single(dativePronounsTopic.Sections, section => section.SectionKey == "formal-ihnen");
        ParsedGrammarSectionModel dativePronounVerbSection = Assert.Single(dativePronounsTopic.Sections, section => section.SectionKey == "common-verbs-with-dative-pronouns");
        Assert.All(languages, language =>
        {
            Assert.True(dativePronounsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(dativePronounsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(dativePronounsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(dativePronounsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.Contains("\"table\"", dativePronounTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", replacingDativeNounsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", formalIhnenSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", dativePronounVerbSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(dativePronounsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(dativePronounsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(dativePronounsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(dativePronounsTopic.Examples, example => example.GermanText == "Kann ich Ihnen helfen?");
        Assert.Contains(dativePronounsTopic.Examples, example => example.GermanText == "Ich danke Ihnen.");
        Assert.Contains(dativePronounsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich danke Sie.");
        Assert.Contains(dativePronounsTopic.CommonMistakes, mistake => mistake.WrongText == "Kann ich Sie helfen?");
        Assert.All(dativePronounsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel accusativePronounsTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-accusative-pronouns");
        Assert.Equal(1, accusativePronounsTopic.ContentRevision);
        Assert.Equal("A2", accusativePronounsTopic.CefrLevel);
        Assert.Equal("pronouns", accusativePronounsTopic.GrammarCategory);
        Assert.Equal(13, accusativePronounsTopic.Sections.Count);
        Assert.True(accusativePronounsTopic.Examples.Count >= 90);
        Assert.True(accusativePronounsTopic.CommonMistakes.Count >= 35);
        Assert.True(accusativePronounsTopic.RuleSummaries.Count >= 20);
        Assert.Equal(75, accusativePronounsTopic.LinkedWords.Count);
        Assert.Contains("a1-simple-accusative-introduction", accusativePronounsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-accusative-versus-dative-basics", accusativePronounsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", accusativePronounsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-formal-sie", accusativePronounsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-du-versus-sie-grammar-basics", accusativePronounsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-dative-pronouns", accusativePronounsTopic.RelatedTopicSlugs);
        Assert.Contains("a2-possessive-pronouns-in-cases", accusativePronounsTopic.RelatedTopicSlugs);
        Assert.Contains("b1-reflexive-verbs-with-dative-and-accusative", accusativePronounsTopic.RelatedTopicSlugs);
        Assert.Contains("a1-separable-verbs-introduction", accusativePronounsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel accusativePronounTableSection = Assert.Single(accusativePronounsTopic.Sections, section => section.SectionKey == "accusative-pronoun-table");
        ParsedGrammarSectionModel replacingAccusativeNounsSection = Assert.Single(accusativePronounsTopic.Sections, section => section.SectionKey == "replacing-accusative-nouns");
        ParsedGrammarSectionModel accusativeVsDativePronounsSection = Assert.Single(accusativePronounsTopic.Sections, section => section.SectionKey == "accusative-vs-dative-pronouns-preview");
        Assert.All(languages, language =>
        {
            Assert.True(accusativePronounsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(accusativePronounsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(accusativePronounsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(accusativePronounsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.Contains("\"table\"", accusativePronounTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", replacingAccusativeNounsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", accusativeVsDativePronounsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(accusativePronounsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(accusativePronounsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(accusativePronounsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(accusativePronounsTopic.Examples, example => example.GermanText == "Ich rufe Sie morgen an.");
        Assert.Contains(accusativePronounsTopic.Examples, example => example.GermanText == "Ich sehe dich.");
        Assert.Contains(accusativePronounsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich rufe Ihnen an.");
        Assert.Contains(accusativePronounsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich sehe du.");
        Assert.All(accusativePronounsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel possessiveCasesTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-possessive-pronouns-in-cases");
        Assert.Equal(1, possessiveCasesTopic.ContentRevision);
        Assert.Equal("A2", possessiveCasesTopic.CefrLevel);
        Assert.Equal("pronouns", possessiveCasesTopic.GrammarCategory);
        Assert.Equal(12, possessiveCasesTopic.Sections.Count);
        Assert.True(possessiveCasesTopic.Examples.Count >= 100);
        Assert.True(possessiveCasesTopic.CommonMistakes.Count >= 35);
        Assert.True(possessiveCasesTopic.RuleSummaries.Count >= 20);
        Assert.Equal(77, possessiveCasesTopic.LinkedWords.Count);
        Assert.Contains("a1-possessive-pronouns-mein-dein", possessiveCasesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-dative-case-basics", possessiveCasesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-accusative-versus-dative-basics", possessiveCasesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-dative-pronouns", possessiveCasesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-accusative-pronouns", possessiveCasesTopic.PrerequisiteSlugs);
        Assert.Contains("b1-adjective-declension-after-indefinite-article", possessiveCasesTopic.RelatedTopicSlugs);
        Assert.Contains("b1-adjective-declension-after-definite-article", possessiveCasesTopic.RelatedTopicSlugs);
        Assert.Contains("b1-b1-case-review", possessiveCasesTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel nominativePossessiveSection = Assert.Single(possessiveCasesTopic.Sections, section => section.SectionKey == "nominative-forms");
        ParsedGrammarSectionModel accusativePossessiveSection = Assert.Single(possessiveCasesTopic.Sections, section => section.SectionKey == "accusative-forms");
        ParsedGrammarSectionModel dativePossessiveSection = Assert.Single(possessiveCasesTopic.Sections, section => section.SectionKey == "dative-forms");
        ParsedGrammarSectionModel comparisonPossessiveSection = Assert.Single(possessiveCasesTopic.Sections, section => section.SectionKey == "mein-and-dein-comparison");
        ParsedGrammarSectionModel caseCheckPossessiveSection = Assert.Single(possessiveCasesTopic.Sections, section => section.SectionKey == "case-check-strategy");
        Assert.All(languages, language =>
        {
            Assert.True(possessiveCasesTopic.TitleLocalized.ContainsKey(language));
            Assert.True(possessiveCasesTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(possessiveCasesTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(possessiveCasesTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(possessiveCasesTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.Contains("\"table\"", nominativePossessiveSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", accusativePossessiveSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", dativePossessiveSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", comparisonPossessiveSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", caseCheckPossessiveSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(possessiveCasesTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(possessiveCasesTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(possessiveCasesTopic.Examples, example => example.GermanText == "Ich helfe deinem Vater.");
        Assert.Contains(possessiveCasesTopic.Examples, example => example.GermanText == "Ihr Termin ist morgen.");
        Assert.Contains(possessiveCasesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich helfe dein Vater.");
        Assert.All(possessiveCasesTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel wechselTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-wechselpraepositionen-introduction");
        Assert.Equal(1, wechselTopic.ContentRevision);
        Assert.Equal("A2", wechselTopic.CefrLevel);
        Assert.Equal("prepositions", wechselTopic.GrammarCategory);
        Assert.Equal(11, wechselTopic.Sections.Count);
        Assert.True(wechselTopic.Examples.Count >= 90);
        Assert.True(wechselTopic.CommonMistakes.Count >= 35);
        Assert.True(wechselTopic.RuleSummaries.Count >= 20);
        Assert.Equal(81, wechselTopic.LinkedWords.Count);
        Assert.Contains("a1-basic-prepositions-in-aus-nach-bei", wechselTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-location-phrases", wechselTopic.PrerequisiteSlugs);
        Assert.Contains("a1-word-order-with-time-and-place", wechselTopic.PrerequisiteSlugs);
        Assert.Contains("a2-dative-case-basics", wechselTopic.PrerequisiteSlugs);
        Assert.Contains("a2-accusative-versus-dative-basics", wechselTopic.PrerequisiteSlugs);
        Assert.Contains("a2-prepositions-with-dative", wechselTopic.RelatedTopicSlugs);
        Assert.Contains("a2-prepositions-with-accusative", wechselTopic.RelatedTopicSlugs);
        Assert.Contains("b1-prepositional-verbs-introduction", wechselTopic.RelatedTopicSlugs);
        Assert.Contains("b1-b1-case-review", wechselTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel formStructureSection = Assert.Single(wechselTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel caseFocusSection = Assert.Single(wechselTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel comparisonSection = Assert.Single(wechselTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel questionFormsSection = Assert.Single(wechselTopic.Sections, section => section.SectionKey == "question-forms");
        ParsedGrammarSectionModel commonPatternsSection = Assert.Single(wechselTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel wechselPracticeAdviceSection = Assert.Single(wechselTopic.Sections, section => section.SectionKey == "practice-advice");
        Assert.All(languages, language =>
        {
            Assert.True(wechselTopic.TitleLocalized.ContainsKey(language));
            Assert.True(wechselTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(wechselTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(wechselTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(wechselTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.Contains("\"table\"", formStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", caseFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", comparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", questionFormsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", commonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", wechselPracticeAdviceSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(wechselTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(wechselTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(wechselTopic.Examples, example => example.GermanText == "Ich bin in der Schule.");
        Assert.Contains(wechselTopic.Examples, example => example.GermanText == "Ich gehe in die Schule.");
        Assert.Contains(wechselTopic.Examples, example => example.GermanText == "Das Buch liegt auf dem Tisch.");
        Assert.Contains(wechselTopic.Examples, example => example.GermanText == "Ich lege das Buch auf den Tisch.");
        Assert.Contains(wechselTopic.CommonMistakes, mistake => mistake.WrongText == "Ich bin in die Schule.");
        Assert.All(wechselTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel dativePrepositionsTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-prepositions-with-dative");
        Assert.Equal(1, dativePrepositionsTopic.ContentRevision);
        Assert.Equal("A2", dativePrepositionsTopic.CefrLevel);
        Assert.Equal("prepositions", dativePrepositionsTopic.GrammarCategory);
        Assert.Equal(11, dativePrepositionsTopic.Sections.Count);
        Assert.True(dativePrepositionsTopic.Examples.Count >= 90);
        Assert.True(dativePrepositionsTopic.CommonMistakes.Count >= 35);
        Assert.True(dativePrepositionsTopic.RuleSummaries.Count >= 20);
        Assert.Equal(82, dativePrepositionsTopic.LinkedWords.Count);
        Assert.Contains("a2-dative-case-basics", dativePrepositionsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-dative-pronouns", dativePrepositionsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-possessive-pronouns-in-cases", dativePrepositionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-prepositions-in-aus-nach-bei", dativePrepositionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-location-phrases", dativePrepositionsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-prepositions-with-accusative", dativePrepositionsTopic.RelatedTopicSlugs);
        Assert.Contains("a2-wechselpraepositionen-introduction", dativePrepositionsTopic.RelatedTopicSlugs);
        Assert.Contains("b1-verb-plus-preposition-combinations", dativePrepositionsTopic.RelatedTopicSlugs);
        Assert.Contains("b1-prepositional-verbs-introduction", dativePrepositionsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel dativePrepositionListSection = Assert.Single(dativePrepositionsTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel dativePrepositionArticleSection = Assert.Single(dativePrepositionsTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel dativePrepositionComparisonSection = Assert.Single(dativePrepositionsTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel dativePrepositionQuestionSection = Assert.Single(dativePrepositionsTopic.Sections, section => section.SectionKey == "question-forms");
        ParsedGrammarSectionModel dativePrepositionPracticeSection = Assert.Single(dativePrepositionsTopic.Sections, section => section.SectionKey == "practice-advice");
        Assert.All(languages, language =>
        {
            Assert.True(dativePrepositionsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(dativePrepositionsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(dativePrepositionsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(dativePrepositionsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(dativePrepositionsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.Contains("\"table\"", dativePrepositionListSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", dativePrepositionArticleSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", dativePrepositionComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", dativePrepositionQuestionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", dativePrepositionPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(dativePrepositionsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(dativePrepositionsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(dativePrepositionsTopic.Examples, example => example.GermanText == "Ich fahre mit dem Bus.");
        Assert.Contains(dativePrepositionsTopic.Examples, example => example.GermanText == "Ich gehe zum Arzt.");
        Assert.Contains(dativePrepositionsTopic.Examples, example => example.GermanText == "Sie kommt aus der Türkei.");
        Assert.Contains(dativePrepositionsTopic.CommonMistakes, mistake => mistake.WrongText == "mit der Bus");
        Assert.Contains(dativePrepositionsTopic.CommonMistakes, mistake => mistake.WrongText == "seit ein Jahr");
        Assert.All(dativePrepositionsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel accusativePrepositionsTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-prepositions-with-accusative");
        Assert.Equal(1, accusativePrepositionsTopic.ContentRevision);
        Assert.Equal("A2", accusativePrepositionsTopic.CefrLevel);
        Assert.Equal("prepositions", accusativePrepositionsTopic.GrammarCategory);
        Assert.Equal(11, accusativePrepositionsTopic.Sections.Count);
        Assert.True(accusativePrepositionsTopic.Examples.Count >= 90);
        Assert.True(accusativePrepositionsTopic.CommonMistakes.Count >= 35);
        Assert.True(accusativePrepositionsTopic.RuleSummaries.Count >= 20);
        Assert.Equal(77, accusativePrepositionsTopic.LinkedWords.Count);
        Assert.Contains("a1-simple-accusative-introduction", accusativePrepositionsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-accusative-pronouns", accusativePrepositionsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-accusative-versus-dative-basics", accusativePrepositionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-noun-gender-basics", accusativePrepositionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-definite-articles-der-die-das", accusativePrepositionsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-prepositions-with-dative", accusativePrepositionsTopic.RelatedTopicSlugs);
        Assert.Contains("a2-wechselpraepositionen-introduction", accusativePrepositionsTopic.RelatedTopicSlugs);
        Assert.Contains("b1-verb-plus-preposition-combinations", accusativePrepositionsTopic.RelatedTopicSlugs);
        Assert.Contains("b1-prepositional-verbs-introduction", accusativePrepositionsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel accusativePrepositionListSection = Assert.Single(accusativePrepositionsTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel accusativeArticleSection = Assert.Single(accusativePrepositionsTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel accusativePrepositionComparisonSection = Assert.Single(accusativePrepositionsTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel accusativePrepositionQuestionSection = Assert.Single(accusativePrepositionsTopic.Sections, section => section.SectionKey == "question-forms");
        ParsedGrammarSectionModel accusativePrepositionPracticeSection = Assert.Single(accusativePrepositionsTopic.Sections, section => section.SectionKey == "practice-advice");
        Assert.All(languages, language =>
        {
            Assert.True(accusativePrepositionsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(accusativePrepositionsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(accusativePrepositionsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(accusativePrepositionsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(accusativePrepositionsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.Contains("\"table\"", accusativePrepositionListSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", accusativeArticleSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", accusativePrepositionComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", accusativePrepositionQuestionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", accusativePrepositionPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(accusativePrepositionsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(accusativePrepositionsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(accusativePrepositionsTopic.Examples, example => example.GermanText == "Das ist für dich.");
        Assert.Contains(accusativePrepositionsTopic.Examples, example => example.GermanText == "Ich gehe durch den Park.");
        Assert.Contains(accusativePrepositionsTopic.Examples, example => example.GermanText == "Der Termin ist um 10 Uhr.");
        Assert.Contains(accusativePrepositionsTopic.CommonMistakes, mistake => mistake.WrongText == "für der Mann");
        Assert.Contains(accusativePrepositionsTopic.CommonMistakes, mistake => mistake.WrongText == "ohne mein Pass");
        Assert.All(accusativePrepositionsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel separablePerfektTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-separable-verbs-in-perfekt");
        Assert.Equal(1, separablePerfektTopic.ContentRevision);
        Assert.Equal("A2", separablePerfektTopic.CefrLevel);
        Assert.Equal("separable-verbs", separablePerfektTopic.GrammarCategory);
        Assert.Equal(11, separablePerfektTopic.Sections.Count);
        Assert.True(separablePerfektTopic.Examples.Count >= 90);
        Assert.True(separablePerfektTopic.CommonMistakes.Count >= 35);
        Assert.True(separablePerfektTopic.RuleSummaries.Count >= 20);
        Assert.Equal(72, separablePerfektTopic.LinkedWords.Count);
        Assert.Contains("a1-separable-verbs-introduction", separablePerfektTopic.PrerequisiteSlugs);
        Assert.Contains("a2-perfekt-with-haben", separablePerfektTopic.PrerequisiteSlugs);
        Assert.Contains("a2-perfekt-with-sein", separablePerfektTopic.PrerequisiteSlugs);
        Assert.Contains("a2-common-irregular-participles", separablePerfektTopic.PrerequisiteSlugs);
        Assert.Contains("a1-word-order-with-time-and-place", separablePerfektTopic.PrerequisiteSlugs);
        Assert.Contains("b1-describing-experiences-in-the-past", separablePerfektTopic.RelatedTopicSlugs);
        Assert.Contains("b1-verb-tense-review", separablePerfektTopic.RelatedTopicSlugs);
        Assert.Contains("a2-praeteritum-of-sein-and-haben", separablePerfektTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel regularSeparablePerfektSection = Assert.Single(separablePerfektTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel separableSentencePositionSection = Assert.Single(separablePerfektTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel presentPerfektComparisonSection = Assert.Single(separablePerfektTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel separableCommonPatternSection = Assert.Single(separablePerfektTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel separableQuestionSection = Assert.Single(separablePerfektTopic.Sections, section => section.SectionKey == "question-forms");
        ParsedGrammarSectionModel separablePracticeSection = Assert.Single(separablePerfektTopic.Sections, section => section.SectionKey == "practice-advice");
        Assert.All(languages, language =>
        {
            Assert.True(separablePerfektTopic.TitleLocalized.ContainsKey(language));
            Assert.True(separablePerfektTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(separablePerfektTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(separablePerfektTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(separablePerfektTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.Contains("\"table\"", regularSeparablePerfektSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", separableSentencePositionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", presentPerfektComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", separableCommonPatternSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", separableQuestionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", separablePracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(separablePerfektTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(separablePerfektTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(separablePerfektTopic.Examples, example => example.GermanText == "Ich habe gestern eingekauft.");
        Assert.Contains(separablePerfektTopic.Examples, example => example.GermanText == "Ich bin früh aufgestanden.");
        Assert.Contains(separablePerfektTopic.Examples, example => example.GermanText == "Wir sind um acht Uhr losgegangen.");
        Assert.Contains(separablePerfektTopic.CommonMistakes, mistake => mistake.WrongText == "Ich habe geeinkauft.");
        Assert.Contains(separablePerfektTopic.CommonMistakes, mistake => mistake.WrongText == "Ich habe aufgestanden.");
        Assert.All(separablePerfektTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel reflexiveVerbsTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-reflexive-verbs-introduction");
        Assert.Equal(1, reflexiveVerbsTopic.ContentRevision);
        Assert.Equal("A2", reflexiveVerbsTopic.CefrLevel);
        Assert.Equal("reflexive-verbs", reflexiveVerbsTopic.GrammarCategory);
        Assert.Equal(11, reflexiveVerbsTopic.Sections.Count);
        Assert.True(reflexiveVerbsTopic.Examples.Count >= 90);
        Assert.True(reflexiveVerbsTopic.CommonMistakes.Count >= 35);
        Assert.True(reflexiveVerbsTopic.RuleSummaries.Count >= 20);
        Assert.Equal(88, reflexiveVerbsTopic.LinkedWords.Count);
        Assert.Contains("a2-accusative-pronouns", reflexiveVerbsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-separable-verbs-introduction", reflexiveVerbsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-separable-verbs-in-perfekt", reflexiveVerbsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-perfekt-with-haben", reflexiveVerbsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-pronoun-and-verb-agreement", reflexiveVerbsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-reflexive-verbs-with-dative-and-accusative", reflexiveVerbsTopic.RelatedTopicSlugs);
        Assert.Contains("a2-modal-verbs-in-more-detail", reflexiveVerbsTopic.RelatedTopicSlugs);
        Assert.Contains("b1-describing-experiences-in-the-past", reflexiveVerbsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel reflexivePronounTableSection = Assert.Single(reflexiveVerbsTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel reflexiveOrderSection = Assert.Single(reflexiveVerbsTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel normalVsReflexiveSection = Assert.Single(reflexiveVerbsTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel reflexivePatternSection = Assert.Single(reflexiveVerbsTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel reflexiveQuestionSection = Assert.Single(reflexiveVerbsTopic.Sections, section => section.SectionKey == "question-forms");
        ParsedGrammarSectionModel reflexivePracticeSection = Assert.Single(reflexiveVerbsTopic.Sections, section => section.SectionKey == "practice-advice");
        Assert.All(languages, language =>
        {
            Assert.True(reflexiveVerbsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(reflexiveVerbsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(reflexiveVerbsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(reflexiveVerbsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(reflexiveVerbsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.Contains("\"table\"", reflexivePronounTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", reflexiveOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", normalVsReflexiveSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", reflexivePatternSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", reflexiveQuestionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", reflexivePracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(reflexiveVerbsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(reflexiveVerbsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(reflexiveVerbsTopic.Examples, example => example.GermanText == "Ich wasche mich.");
        Assert.Contains(reflexiveVerbsTopic.Examples, example => example.GermanText == "Wir treffen uns morgen.");
        Assert.Contains(reflexiveVerbsTopic.Examples, example => example.GermanText == "Ich habe mich gewaschen.");
        Assert.Contains(reflexiveVerbsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich wasche sich.");
        Assert.Contains(reflexiveVerbsTopic.CommonMistakes, mistake => mistake.WrongText == "Wir treffen sich.");
        Assert.All(reflexiveVerbsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel dassClausesTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-dass-clauses");
        Assert.Equal(1, dassClausesTopic.ContentRevision);
        Assert.Equal("A2", dassClausesTopic.CefrLevel);
        Assert.Equal("subordinate-clauses", dassClausesTopic.GrammarCategory);
        Assert.Equal(10, dassClausesTopic.Sections.Count);
        Assert.True(dassClausesTopic.Examples.Count >= 90);
        Assert.True(dassClausesTopic.CommonMistakes.Count >= 35);
        Assert.True(dassClausesTopic.RuleSummaries.Count >= 20);
        Assert.Equal(78, dassClausesTopic.LinkedWords.Count);
        Assert.Contains("a1-verb-position-in-simple-sentences", dassClausesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-word-order-with-time-and-place", dassClausesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-sentence-negation", dassClausesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-simple-modal-verbs-koennen-muessen-wollen", dassClausesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-modal-verbs-in-more-detail", dassClausesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-weil-clauses", dassClausesTopic.RelatedTopicSlugs);
        Assert.Contains("a2-wenn-for-conditions", dassClausesTopic.RelatedTopicSlugs);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", dassClausesTopic.RelatedTopicSlugs);
        Assert.Contains("b1-indirect-questions", dassClausesTopic.RelatedTopicSlugs);
        Assert.Contains("b1-reported-requests-and-polite-questions", dassClausesTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel dassStructureSection = Assert.Single(dassClausesTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel dassVerbFinalSection = Assert.Single(dassClausesTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel simpleVsDassSection = Assert.Single(dassClausesTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel dassPatternSection = Assert.Single(dassClausesTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel dassPracticeSection = Assert.Single(dassClausesTopic.Sections, section => section.SectionKey == "practice-advice");
        Assert.All(languages, language =>
        {
            Assert.True(dassClausesTopic.TitleLocalized.ContainsKey(language));
            Assert.True(dassClausesTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(dassClausesTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(dassClausesTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(dassClausesTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.Contains("\"table\"", dassStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", dassVerbFinalSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", simpleVsDassSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", dassPatternSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", dassPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(dassClausesTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(dassClausesTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(dassClausesTopic.Examples, example => example.GermanText == "Ich glaube, dass er heute kommt.");
        Assert.Contains(dassClausesTopic.Examples, example => example.GermanText == "Sie sagt, dass sie keine Zeit hat.");
        Assert.Contains(dassClausesTopic.Examples, example => example.GermanText == "Ich glaube, dass wir morgen arbeiten müssen.");
        Assert.Contains(dassClausesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich glaube, dass er kommt heute.");
        Assert.Contains(dassClausesTopic.CommonMistakes, mistake => mistake.WrongText == "Sie sagt, dass sie hat keine Zeit.");
        Assert.All(dassClausesTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel weilClausesTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-weil-clauses");
        Assert.Equal(1, weilClausesTopic.ContentRevision);
        Assert.Equal("A2", weilClausesTopic.CefrLevel);
        Assert.Equal("subordinate-clauses", weilClausesTopic.GrammarCategory);
        Assert.Equal(10, weilClausesTopic.Sections.Count);
        Assert.True(weilClausesTopic.Examples.Count >= 95);
        Assert.True(weilClausesTopic.CommonMistakes.Count >= 35);
        Assert.True(weilClausesTopic.RuleSummaries.Count >= 20);
        Assert.Equal(77, weilClausesTopic.LinkedWords.Count);
        Assert.Contains("a2-dass-clauses", weilClausesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-word-order-with-time-and-place", weilClausesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-sentence-negation", weilClausesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-modal-verbs-in-more-detail", weilClausesTopic.PrerequisiteSlugs);
        Assert.Contains("a1-question-answer-sentence-patterns", weilClausesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-denn-versus-weil", weilClausesTopic.RelatedTopicSlugs);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", weilClausesTopic.RelatedTopicSlugs);
        Assert.Contains("b1-weil-obwohl-trotzdem", weilClausesTopic.RelatedTopicSlugs);
        Assert.Contains("b1-connectors-for-cause-and-effect", weilClausesTopic.RelatedTopicSlugs);
        Assert.Contains("b1-giving-reasons-clearly", weilClausesTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel weilStructureTableSection = Assert.Single(weilClausesTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel weilWordOrderSection = Assert.Single(weilClausesTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel weilComparisonSection = Assert.Single(weilClausesTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel weilCommonPatternsSection = Assert.Single(weilClausesTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel weilPracticeSection = Assert.Single(weilClausesTopic.Sections, section => section.SectionKey == "practice-advice");
        Assert.All(languages, language =>
        {
            Assert.True(weilClausesTopic.TitleLocalized.ContainsKey(language));
            Assert.True(weilClausesTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(weilClausesTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(weilClausesTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", weilStructureTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", weilWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", weilComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", weilCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", weilPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(weilClausesTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(weilClausesTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(weilClausesTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(weilClausesTopic.Examples, example => example.GermanText == "Ich komme nicht, weil ich krank bin.");
        Assert.Contains(weilClausesTopic.Examples, example => example.GermanText == "Sie lernt Deutsch, weil sie in Deutschland wohnt.");
        Assert.Contains(weilClausesTopic.Examples, example => example.GermanText == "Er kommt später, weil er arbeiten muss.");
        Assert.Contains(weilClausesTopic.Examples, example => example.GermanText == "Weil ich krank bin, komme ich nicht.");
        Assert.Contains(weilClausesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich komme nicht, weil ich bin krank.");
        Assert.Contains(weilClausesTopic.CommonMistakes, mistake => mistake.WrongText == "Weil ich krank bin, ich bleibe zu Hause.");
        Assert.All(weilClausesTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel wennTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-wenn-for-conditions");
        Assert.Equal(1, wennTopic.ContentRevision);
        Assert.Equal("A2", wennTopic.CefrLevel);
        Assert.Equal("subordinate-clauses", wennTopic.GrammarCategory);
        Assert.Equal(10, wennTopic.Sections.Count);
        Assert.True(wennTopic.Examples.Count >= 95);
        Assert.True(wennTopic.CommonMistakes.Count >= 35);
        Assert.True(wennTopic.RuleSummaries.Count >= 20);
        Assert.Equal(77, wennTopic.LinkedWords.Count);
        Assert.Contains("a2-dass-clauses", wennTopic.PrerequisiteSlugs);
        Assert.Contains("a2-weil-clauses", wennTopic.PrerequisiteSlugs);
        Assert.Contains("a1-word-order-with-time-and-place", wennTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-sentence-negation", wennTopic.PrerequisiteSlugs);
        Assert.Contains("a2-modal-verbs-in-more-detail", wennTopic.PrerequisiteSlugs);
        Assert.Contains("a1-w-questions-wer-was-wo-wann-wie", wennTopic.PrerequisiteSlugs);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", wennTopic.RelatedTopicSlugs);
        Assert.Contains("b1-als-versus-wenn", wennTopic.RelatedTopicSlugs);
        Assert.Contains("b1-talking-about-plans-and-conditions", wennTopic.RelatedTopicSlugs);
        Assert.Contains("b1-giving-reasons-clearly", wennTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel wennStructureTableSection = Assert.Single(wennTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel wennWordOrderSection = Assert.Single(wennTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel wennComparisonSection = Assert.Single(wennTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel wennCommonPatternsSection = Assert.Single(wennTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel wennPracticeSection = Assert.Single(wennTopic.Sections, section => section.SectionKey == "practice-advice");
        Assert.All(languages, language =>
        {
            Assert.True(wennTopic.TitleLocalized.ContainsKey(language));
            Assert.True(wennTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(wennTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(wennTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", wennStructureTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", wennWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", wennComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", wennCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", wennPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(wennTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(wennTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(wennTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(wennTopic.Examples, example => example.GermanText == "Wenn ich Zeit habe, komme ich.");
        Assert.Contains(wennTopic.Examples, example => example.GermanText == "Ich komme, wenn ich Zeit habe.");
        Assert.Contains(wennTopic.Examples, example => example.GermanText == "Wenn ich arbeiten muss, komme ich später.");
        Assert.Contains(wennTopic.CommonMistakes, mistake => mistake.WrongText == "Wenn ich habe Zeit, komme ich.");
        Assert.Contains(wennTopic.CommonMistakes, mistake => mistake.WrongText == "Wann ich Zeit habe, komme ich.");
        Assert.All(wennTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel dennWeilTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-denn-versus-weil");
        Assert.Equal(1, dennWeilTopic.ContentRevision);
        Assert.Equal("A2", dennWeilTopic.CefrLevel);
        Assert.Equal("connectors", dennWeilTopic.GrammarCategory);
        Assert.Equal(10, dennWeilTopic.Sections.Count);
        Assert.True(dennWeilTopic.Examples.Count >= 90);
        Assert.True(dennWeilTopic.CommonMistakes.Count >= 35);
        Assert.True(dennWeilTopic.RuleSummaries.Count >= 20);
        Assert.Equal(75, dennWeilTopic.LinkedWords.Count);
        Assert.Contains("a2-weil-clauses", dennWeilTopic.PrerequisiteSlugs);
        Assert.Contains("a2-dass-clauses", dennWeilTopic.PrerequisiteSlugs);
        Assert.Contains("a1-verb-position-in-simple-sentences", dennWeilTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-sentence-negation", dennWeilTopic.PrerequisiteSlugs);
        Assert.Contains("a2-modal-verbs-in-more-detail", dennWeilTopic.PrerequisiteSlugs);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", dennWeilTopic.RelatedTopicSlugs);
        Assert.Contains("b1-weil-obwohl-trotzdem", dennWeilTopic.RelatedTopicSlugs);
        Assert.Contains("b1-connectors-for-cause-and-effect", dennWeilTopic.RelatedTopicSlugs);
        Assert.Contains("b1-giving-reasons-clearly", dennWeilTopic.RelatedTopicSlugs);
        Assert.Contains("b2-cause-connectors-aufgrund-wegen-da", dennWeilTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel dennWeilStructureSection = Assert.Single(dennWeilTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel dennWeilWordOrderSection = Assert.Single(dennWeilTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel dennWeilComparisonSection = Assert.Single(dennWeilTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel dennWeilPatternSection = Assert.Single(dennWeilTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel dennWeilPracticeSection = Assert.Single(dennWeilTopic.Sections, section => section.SectionKey == "practice-advice");
        Assert.All(languages, language =>
        {
            Assert.True(dennWeilTopic.TitleLocalized.ContainsKey(language));
            Assert.True(dennWeilTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(dennWeilTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(dennWeilTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", dennWeilStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", dennWeilWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", dennWeilComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", dennWeilPatternSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", dennWeilPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(dennWeilTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(dennWeilTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(dennWeilTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(dennWeilTopic.Examples, example => example.GermanText == "Ich bleibe zu Hause, weil ich krank bin.");
        Assert.Contains(dennWeilTopic.Examples, example => example.GermanText == "Ich bleibe zu Hause, denn ich bin krank.");
        Assert.Contains(dennWeilTopic.CommonMistakes, mistake => mistake.WrongText == "Ich komme nicht, weil ich bin krank.");
        Assert.Contains(dennWeilTopic.CommonMistakes, mistake => mistake.WrongText == "Ich komme nicht, denn ich krank bin.");
        Assert.All(dennWeilTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel subordinateOrderTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-sentence-order-in-subordinate-clauses");
        Assert.Equal(1, subordinateOrderTopic.ContentRevision);
        Assert.Equal("A2", subordinateOrderTopic.CefrLevel);
        Assert.Equal("word-order", subordinateOrderTopic.GrammarCategory);
        Assert.Equal(10, subordinateOrderTopic.Sections.Count);
        Assert.True(subordinateOrderTopic.Examples.Count >= 110);
        Assert.True(subordinateOrderTopic.CommonMistakes.Count >= 40);
        Assert.True(subordinateOrderTopic.RuleSummaries.Count >= 20);
        Assert.Equal(70, subordinateOrderTopic.LinkedWords.Count);
        Assert.Contains("a2-dass-clauses", subordinateOrderTopic.PrerequisiteSlugs);
        Assert.Contains("a2-weil-clauses", subordinateOrderTopic.PrerequisiteSlugs);
        Assert.Contains("a2-wenn-for-conditions", subordinateOrderTopic.PrerequisiteSlugs);
        Assert.Contains("a2-denn-versus-weil", subordinateOrderTopic.PrerequisiteSlugs);
        Assert.Contains("a2-modal-verbs-in-more-detail", subordinateOrderTopic.PrerequisiteSlugs);
        Assert.Contains("a2-perfekt-with-haben", subordinateOrderTopic.PrerequisiteSlugs);
        Assert.Contains("a2-perfekt-with-sein", subordinateOrderTopic.PrerequisiteSlugs);
        Assert.Contains("b1-sentence-order-with-multiple-clauses", subordinateOrderTopic.RelatedTopicSlugs);
        Assert.Contains("b1-indirect-questions", subordinateOrderTopic.RelatedTopicSlugs);
        Assert.Contains("b1-reported-requests-and-polite-questions", subordinateOrderTopic.RelatedTopicSlugs);
        Assert.Contains("b1-connectors-for-cause-and-effect", subordinateOrderTopic.RelatedTopicSlugs);
        Assert.Contains("b2-complex-sentence-order", subordinateOrderTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel subordinateStructureSection = Assert.Single(subordinateOrderTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel subordinateWordOrderSection = Assert.Single(subordinateOrderTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel subordinateComparisonSection = Assert.Single(subordinateOrderTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel subordinatePatternsSection = Assert.Single(subordinateOrderTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel subordinatePracticeSection = Assert.Single(subordinateOrderTopic.Sections, section => section.SectionKey == "practice-advice");
        Assert.All(languages, language =>
        {
            Assert.True(subordinateOrderTopic.TitleLocalized.ContainsKey(language));
            Assert.True(subordinateOrderTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(subordinateOrderTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(subordinateOrderTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", subordinateStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", subordinateWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", subordinateComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", subordinatePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", subordinatePracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(subordinateOrderTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(subordinateOrderTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(subordinateOrderTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(subordinateOrderTopic.Examples, example => example.GermanText == "Ich glaube, dass er morgen kommt.");
        Assert.Contains(subordinateOrderTopic.Examples, example => example.GermanText == "Ich bleibe zu Hause, weil ich krank bin.");
        Assert.Contains(subordinateOrderTopic.Examples, example => example.GermanText == "Wenn ich Zeit habe, komme ich.");
        Assert.Contains(subordinateOrderTopic.Examples, example => example.GermanText == "Ich komme nicht, denn ich bin krank.");
        Assert.Contains(subordinateOrderTopic.CommonMistakes, mistake => mistake.WrongText == "Ich glaube, dass er kommt morgen.");
        Assert.Contains(subordinateOrderTopic.CommonMistakes, mistake => mistake.WrongText == "Ich weiß, dass er hat gearbeitet.");
        Assert.All(subordinateOrderTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel comparativeTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-comparative-forms");
        Assert.Equal(1, comparativeTopic.ContentRevision);
        Assert.Equal("A2", comparativeTopic.CefrLevel);
        Assert.Equal("adjective-declension", comparativeTopic.GrammarCategory);
        Assert.Equal(10, comparativeTopic.Sections.Count);
        Assert.True(comparativeTopic.Examples.Count >= 90);
        Assert.True(comparativeTopic.CommonMistakes.Count >= 35);
        Assert.True(comparativeTopic.RuleSummaries.Count >= 20);
        Assert.Equal(70, comparativeTopic.LinkedWords.Count);
        Assert.Contains("a1-basic-adjective-position", comparativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", comparativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-articles-with-food-drinks-and-shopping-nouns", comparativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-numbers-and-grammar-use", comparativeTopic.PrerequisiteSlugs);
        Assert.Contains("a2-superlative-basics", comparativeTopic.RelatedTopicSlugs);
        Assert.Contains("a2-adjective-endings-introduction", comparativeTopic.RelatedTopicSlugs);
        Assert.Contains("b2-comparing-options-grammatically", comparativeTopic.RelatedTopicSlugs);
        Assert.Contains("b1-adjective-declension-after-definite-article", comparativeTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel formTableSection = Assert.Single(comparativeTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel comparativePositionSection = Assert.Single(comparativeTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel comparativeComparisonSection = Assert.Single(comparativeTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel patternsSection = Assert.Single(comparativeTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel practiceSection = Assert.Single(comparativeTopic.Sections, section => section.SectionKey == "practice-advice");
        Assert.All(languages, language =>
        {
            Assert.True(comparativeTopic.TitleLocalized.ContainsKey(language));
            Assert.True(comparativeTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(comparativeTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(comparativeTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.Contains("\"table\"", formTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", comparativePositionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", comparativeComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", patternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", practiceSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(comparativeTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(comparativeTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(comparativeTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(comparativeTopic.Examples, example => example.GermanText == "Berlin ist größer als Bonn.");
        Assert.Contains(comparativeTopic.Examples, example => example.GermanText == "Der Kaffee ist teurer als der Tee.");
        Assert.Contains(comparativeTopic.Examples, example => example.GermanText == "Ich brauche mehr Zeit.");
        Assert.Contains(comparativeTopic.CommonMistakes, mistake => mistake.WrongText == "Berlin ist groß als Bonn.");
        Assert.Contains(comparativeTopic.CommonMistakes, mistake => mistake.WrongText == "Der Kaffee ist teuerer.");
        Assert.All(comparativeTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel superlativeTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-superlative-basics");
        Assert.Equal(1, superlativeTopic.ContentRevision);
        Assert.Equal("A2", superlativeTopic.CefrLevel);
        Assert.Equal("adjective-declension", superlativeTopic.GrammarCategory);
        Assert.Equal(10, superlativeTopic.Sections.Count);
        Assert.True(superlativeTopic.Examples.Count >= 90);
        Assert.True(superlativeTopic.CommonMistakes.Count >= 35);
        Assert.True(superlativeTopic.RuleSummaries.Count >= 20);
        Assert.Equal(75, superlativeTopic.LinkedWords.Count);
        Assert.Contains("a2-comparative-forms", superlativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-adjective-position", superlativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", superlativeTopic.PrerequisiteSlugs);
        Assert.Contains("a2-adjective-endings-introduction", superlativeTopic.RelatedTopicSlugs);
        Assert.Contains("b1-adjective-declension-after-definite-article", superlativeTopic.RelatedTopicSlugs);
        Assert.Contains("b2-comparing-options-grammatically", superlativeTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel superlativeFormTableSection = Assert.Single(superlativeTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel superlativePositionSection = Assert.Single(superlativeTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel superlativeComparisonSection = Assert.Single(superlativeTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel superlativePatternsSection = Assert.Single(superlativeTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel superlativePracticeSection = Assert.Single(superlativeTopic.Sections, section => section.SectionKey == "practice-advice");
        Assert.All(languages, language =>
        {
            Assert.True(superlativeTopic.TitleLocalized.ContainsKey(language));
            Assert.True(superlativeTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(superlativeTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(superlativeTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.Contains("\"table\"", superlativeFormTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", superlativePositionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", superlativeComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", superlativePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", superlativePracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(superlativeTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(superlativeTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(superlativeTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(superlativeTopic.Examples, example => example.GermanText == "Dieses Zimmer ist am größten.");
        Assert.Contains(superlativeTopic.Examples, example => example.GermanText == "Der Kurs ist am besten.");
        Assert.Contains(superlativeTopic.Examples, example => example.GermanText == "Ich trinke am liebsten Tee.");
        Assert.Contains(superlativeTopic.CommonMistakes, mistake => mistake.WrongText == "am gutesten");
        Assert.Contains(superlativeTopic.CommonMistakes, mistake => mistake.WrongText == "Sie lernt am vielsten.");
        Assert.All(superlativeTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel adjectiveEndingsTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-adjective-endings-introduction");
        Assert.Equal(1, adjectiveEndingsTopic.ContentRevision);
        Assert.Equal("A2", adjectiveEndingsTopic.CefrLevel);
        Assert.Equal("adjective-declension", adjectiveEndingsTopic.GrammarCategory);
        Assert.Equal(10, adjectiveEndingsTopic.Sections.Count);
        Assert.True(adjectiveEndingsTopic.Examples.Count >= 90);
        Assert.True(adjectiveEndingsTopic.CommonMistakes.Count >= 35);
        Assert.True(adjectiveEndingsTopic.RuleSummaries.Count >= 20);
        Assert.Equal(77, adjectiveEndingsTopic.LinkedWords.Count);
        Assert.Contains("a1-basic-adjective-position", adjectiveEndingsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-comparative-forms", adjectiveEndingsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-superlative-basics", adjectiveEndingsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-definite-articles-der-die-das", adjectiveEndingsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-indefinite-articles-ein-eine", adjectiveEndingsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-noun-gender-basics", adjectiveEndingsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-simple-accusative-introduction", adjectiveEndingsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-adjective-declension-after-definite-article", adjectiveEndingsTopic.RelatedTopicSlugs);
        Assert.Contains("b1-adjective-declension-after-indefinite-article", adjectiveEndingsTopic.RelatedTopicSlugs);
        Assert.Contains("b1-adjective-declension-without-article", adjectiveEndingsTopic.RelatedTopicSlugs);
        Assert.Contains("b2-adjective-declension-advanced-review", adjectiveEndingsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel adjectiveEndingsFormTableSection = Assert.Single(adjectiveEndingsTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel adjectiveEndingsCaseFocusSection = Assert.Single(adjectiveEndingsTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel adjectiveEndingsComparisonSection = Assert.Single(adjectiveEndingsTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel adjectiveEndingsCommonPatternsSection = Assert.Single(adjectiveEndingsTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel adjectiveEndingsPracticeAdviceSection = Assert.Single(adjectiveEndingsTopic.Sections, section => section.SectionKey == "practice-advice");
        Assert.All(languages, language =>
        {
            Assert.True(adjectiveEndingsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(adjectiveEndingsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(adjectiveEndingsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(adjectiveEndingsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.Contains("\"table\"", adjectiveEndingsFormTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", adjectiveEndingsCaseFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", adjectiveEndingsComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", adjectiveEndingsCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", adjectiveEndingsPracticeAdviceSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(adjectiveEndingsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(adjectiveEndingsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(adjectiveEndingsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(adjectiveEndingsTopic.Examples, example => example.GermanText == "Das Zimmer ist klein.");
        Assert.Contains(adjectiveEndingsTopic.Examples, example => example.GermanText == "ein kleines Zimmer");
        Assert.Contains(adjectiveEndingsTopic.Examples, example => example.GermanText == "Ich möchte einen heißen Kaffee.");
        Assert.Contains(adjectiveEndingsTopic.CommonMistakes, mistake => mistake.WrongText == "Das Zimmer ist kleines.");
        Assert.Contains(adjectiveEndingsTopic.CommonMistakes, mistake => mistake.WrongText == "ein klein Zimmer");
        Assert.Contains(adjectiveEndingsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich möchte einen heiß Kaffee.");
        Assert.All(adjectiveEndingsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel indirectQuestionsTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-indirect-questions-introduction");
        Assert.Equal(1, indirectQuestionsTopic.ContentRevision);
        Assert.Equal("A2", indirectQuestionsTopic.CefrLevel);
        Assert.Equal("questions", indirectQuestionsTopic.GrammarCategory);
        Assert.Equal(10, indirectQuestionsTopic.Sections.Count);
        Assert.True(indirectQuestionsTopic.Examples.Count >= 90);
        Assert.True(indirectQuestionsTopic.CommonMistakes.Count >= 35);
        Assert.True(indirectQuestionsTopic.RuleSummaries.Count >= 20);
        Assert.Equal(75, indirectQuestionsTopic.LinkedWords.Count);
        Assert.Contains("a1-w-questions-wer-was-wo-wann-wie", indirectQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-yes-no-questions", indirectQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-formal-sie", indirectQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("a1-polite-requests-with-moechte", indirectQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-dass-clauses", indirectQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", indirectQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-modal-verbs-in-more-detail", indirectQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-indirect-questions", indirectQuestionsTopic.RelatedTopicSlugs);
        Assert.Contains("b1-reported-requests-and-polite-questions", indirectQuestionsTopic.RelatedTopicSlugs);
        Assert.Contains("a2-polite-forms-with-wuerde", indirectQuestionsTopic.RelatedTopicSlugs);
        Assert.Contains("b1-formal-email-sentence-structure", indirectQuestionsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel indirectQuestionsCorePatternsSection = Assert.Single(indirectQuestionsTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel indirectQuestionsStructureSection = Assert.Single(indirectQuestionsTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel indirectQuestionsWordOrderSection = Assert.Single(indirectQuestionsTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel indirectQuestionsComparisonSection = Assert.Single(indirectQuestionsTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel indirectQuestionsCommonPatternsSection = Assert.Single(indirectQuestionsTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel indirectQuestionsPracticeSection = Assert.Single(indirectQuestionsTopic.Sections, section => section.SectionKey == "practice-advice");
        Assert.All(languages, language =>
        {
            Assert.True(indirectQuestionsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(indirectQuestionsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(indirectQuestionsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(indirectQuestionsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.Contains("\"rule-list\"", indirectQuestionsCorePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", indirectQuestionsStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", indirectQuestionsWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", indirectQuestionsComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", indirectQuestionsCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", indirectQuestionsPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(indirectQuestionsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(indirectQuestionsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(indirectQuestionsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(indirectQuestionsTopic.Examples, example => example.GermanText == "Können Sie mir sagen, wo der Bahnhof ist?");
        Assert.Contains(indirectQuestionsTopic.Examples, example => example.GermanText == "Wissen Sie, ob der Bus kommt?");
        Assert.Contains(indirectQuestionsTopic.Examples, example => example.GermanText == "Ich weiß nicht, ob ich kommen kann.");
        Assert.Contains(indirectQuestionsTopic.CommonMistakes, mistake => mistake.WrongText == "Können Sie mir sagen, wo ist der Bahnhof?");
        Assert.Contains(indirectQuestionsTopic.CommonMistakes, mistake => mistake.WrongText == "Wissen Sie, ob kommt der Bus?");
        Assert.Contains(indirectQuestionsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich möchte wissen, haben Sie Zeit.");
        Assert.All(indirectQuestionsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel a2ImperativeTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-imperative-formal-and-informal");
        Assert.Equal(1, a2ImperativeTopic.ContentRevision);
        Assert.Equal("A2", a2ImperativeTopic.CefrLevel);
        Assert.Equal("imperative", a2ImperativeTopic.GrammarCategory);
        Assert.Equal(10, a2ImperativeTopic.Sections.Count);
        Assert.True(a2ImperativeTopic.Examples.Count >= 90);
        Assert.True(a2ImperativeTopic.CommonMistakes.Count >= 35);
        Assert.True(a2ImperativeTopic.RuleSummaries.Count >= 20);
        Assert.Equal(80, a2ImperativeTopic.LinkedWords.Count);
        Assert.Contains("a1-imperative-basics", a2ImperativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-formal-sie", a2ImperativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-du-versus-sie-grammar-basics", a2ImperativeTopic.PrerequisiteSlugs);
        Assert.Contains("a1-separable-verbs-introduction", a2ImperativeTopic.PrerequisiteSlugs);
        Assert.Contains("a2-modal-verbs-in-more-detail", a2ImperativeTopic.PrerequisiteSlugs);
        Assert.Contains("a2-indirect-questions-introduction", a2ImperativeTopic.RelatedTopicSlugs);
        Assert.Contains("a2-polite-forms-with-wuerde", a2ImperativeTopic.RelatedTopicSlugs);
        Assert.Contains("b1-konjunktiv-ii-for-polite-requests", a2ImperativeTopic.RelatedTopicSlugs);
        Assert.Contains("b1-reported-requests-and-polite-questions", a2ImperativeTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel imperativeCorePatternsSection = Assert.Single(a2ImperativeTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel imperativeStructureSection = Assert.Single(a2ImperativeTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel imperativeWordOrderSection = Assert.Single(a2ImperativeTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel imperativeComparisonSection = Assert.Single(a2ImperativeTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel imperativeCommonPatternsSection = Assert.Single(a2ImperativeTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel imperativePracticeSection = Assert.Single(a2ImperativeTopic.Sections, section => section.SectionKey == "practice-advice");
        Assert.All(languages, language =>
        {
            Assert.True(a2ImperativeTopic.TitleLocalized.ContainsKey(language));
            Assert.True(a2ImperativeTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(a2ImperativeTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(a2ImperativeTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.Contains("\"rule-list\"", imperativeCorePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", imperativeStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", imperativeWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", imperativeComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", imperativeCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", imperativePracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(a2ImperativeTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(a2ImperativeTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(a2ImperativeTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(a2ImperativeTopic.Examples, example => example.GermanText == "Kommen Sie bitte.");
        Assert.Contains(a2ImperativeTopic.Examples, example => example.GermanText == "Ruf mich bitte an.");
        Assert.Contains(a2ImperativeTopic.Examples, example => example.GermanText == "Können Sie bitte warten?");
        Assert.Contains(a2ImperativeTopic.CommonMistakes, mistake => mistake.WrongText == "Komm Sie bitte.");
        Assert.Contains(a2ImperativeTopic.CommonMistakes, mistake => mistake.WrongText == "Ruf an mich.");
        Assert.Contains(a2ImperativeTopic.CommonMistakes, mistake => mistake.WrongText == "Mach zu die Tür.");
        Assert.All(a2ImperativeTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel timeClausesTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-time-clauses-bevor-and-nachdem-introduction");
        Assert.Equal(1, timeClausesTopic.ContentRevision);
        Assert.Equal("A2", timeClausesTopic.CefrLevel);
        Assert.Equal("subordinate-clauses", timeClausesTopic.GrammarCategory);
        Assert.Equal(10, timeClausesTopic.Sections.Count);
        Assert.True(timeClausesTopic.Examples.Count >= 90);
        Assert.True(timeClausesTopic.CommonMistakes.Count >= 35);
        Assert.True(timeClausesTopic.RuleSummaries.Count >= 20);
        Assert.Equal(70, timeClausesTopic.LinkedWords.Count);
        Assert.Contains("a2-dass-clauses", timeClausesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-weil-clauses", timeClausesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-wenn-for-conditions", timeClausesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", timeClausesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-perfekt-with-haben", timeClausesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-perfekt-with-sein", timeClausesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-common-irregular-participles", timeClausesTopic.PrerequisiteSlugs);
        Assert.Contains("b1-nachdem-bevor-waehrend", timeClausesTopic.RelatedTopicSlugs);
        Assert.Contains("b1-describing-experiences-in-the-past", timeClausesTopic.RelatedTopicSlugs);
        Assert.Contains("b1-sentence-order-with-multiple-clauses", timeClausesTopic.RelatedTopicSlugs);
        Assert.Contains("b1-connectors-for-cause-and-effect", timeClausesTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel timeClauseCorePatternsSection = Assert.Single(timeClausesTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel timeClauseStructureSection = Assert.Single(timeClausesTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel timeClauseWordOrderSection = Assert.Single(timeClausesTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel timeClauseComparisonSection = Assert.Single(timeClausesTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel timeClauseCommonPatternsSection = Assert.Single(timeClausesTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel timeClausePracticeSection = Assert.Single(timeClausesTopic.Sections, section => section.SectionKey == "practice-advice");
        Assert.All(languages, language =>
        {
            Assert.True(timeClausesTopic.TitleLocalized.ContainsKey(language));
            Assert.True(timeClausesTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(timeClausesTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(timeClausesTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.Contains("\"rule-list\"", timeClauseCorePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", timeClauseStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", timeClauseWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", timeClauseComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", timeClauseCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", timeClausePracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(timeClausesTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(timeClausesTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(timeClausesTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(timeClausesTopic.Examples, example => example.GermanText == "Ich frühstücke, bevor ich zur Arbeit gehe.");
        Assert.Contains(timeClausesTopic.Examples, example => example.GermanText == "Nachdem ich gegessen habe, lerne ich Deutsch.");
        Assert.Contains(timeClausesTopic.Examples, example => example.GermanText == "Bevor ich gehe, rufe ich dich an.");
        Assert.Contains(timeClausesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich frühstücke, bevor ich gehe zur Arbeit.");
        Assert.Contains(timeClausesTopic.CommonMistakes, mistake => mistake.WrongText == "Nachdem ich habe gegessen, lerne ich.");
        Assert.Contains(timeClausesTopic.CommonMistakes, mistake => mistake.WrongText == "Bevor ich zur Arbeit gehe, ich frühstücke.");
        Assert.All(timeClausesTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel zuInfinitiveTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-zu-plus-infinitive-introduction");
        Assert.Equal(1, zuInfinitiveTopic.ContentRevision);
        Assert.Equal("A2", zuInfinitiveTopic.CefrLevel);
        Assert.Equal("verbs", zuInfinitiveTopic.GrammarCategory);
        Assert.Equal(10, zuInfinitiveTopic.Sections.Count);
        Assert.True(zuInfinitiveTopic.Examples.Count >= 90);
        Assert.True(zuInfinitiveTopic.CommonMistakes.Count >= 35);
        Assert.True(zuInfinitiveTopic.RuleSummaries.Count >= 20);
        Assert.Equal(71, zuInfinitiveTopic.LinkedWords.Count);
        Assert.Contains("a1-regular-verbs-in-praesens", zuInfinitiveTopic.PrerequisiteSlugs);
        Assert.Contains("a1-simple-modal-verbs-koennen-muessen-wollen", zuInfinitiveTopic.PrerequisiteSlugs);
        Assert.Contains("a1-separable-verbs-introduction", zuInfinitiveTopic.PrerequisiteSlugs);
        Assert.Contains("a2-separable-verbs-in-perfekt", zuInfinitiveTopic.PrerequisiteSlugs);
        Assert.Contains("a2-dass-clauses", zuInfinitiveTopic.PrerequisiteSlugs);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", zuInfinitiveTopic.PrerequisiteSlugs);
        Assert.Contains("b1-infinitive-with-zu", zuInfinitiveTopic.RelatedTopicSlugs);
        Assert.Contains("b1-um-zu", zuInfinitiveTopic.RelatedTopicSlugs);
        Assert.Contains("b1-damit-versus-um-zu", zuInfinitiveTopic.RelatedTopicSlugs);
        Assert.Contains("b2-infinitive-clauses-advanced", zuInfinitiveTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel zuCorePatternsSection = Assert.Single(zuInfinitiveTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel zuStructureSection = Assert.Single(zuInfinitiveTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel zuWordOrderSection = Assert.Single(zuInfinitiveTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel zuComparisonSection = Assert.Single(zuInfinitiveTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel zuCommonPatternsSection = Assert.Single(zuInfinitiveTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel zuPracticeSection = Assert.Single(zuInfinitiveTopic.Sections, section => section.SectionKey == "practice-advice");
        Assert.All(languages, language =>
        {
            Assert.True(zuInfinitiveTopic.TitleLocalized.ContainsKey(language));
            Assert.True(zuInfinitiveTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(zuInfinitiveTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(zuInfinitiveTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.Contains("\"rule-list\"", zuCorePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", zuStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", zuWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", zuComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", zuCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", zuPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(zuInfinitiveTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(zuInfinitiveTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(zuInfinitiveTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(zuInfinitiveTopic.Examples, example => example.GermanText == "Ich versuche, Deutsch zu lernen.");
        Assert.Contains(zuInfinitiveTopic.Examples, example => example.GermanText == "Ich habe Zeit, Deutsch zu lernen.");
        Assert.Contains(zuInfinitiveTopic.Examples, example => example.GermanText == "Ich muss arbeiten.");
        Assert.Contains(zuInfinitiveTopic.CommonMistakes, mistake => mistake.WrongText == "Ich versuche Deutsch lernen.");
        Assert.Contains(zuInfinitiveTopic.CommonMistakes, mistake => mistake.WrongText == "Ich muss zu arbeiten.");
        Assert.Contains(zuInfinitiveTopic.CommonMistakes, mistake => mistake.WrongText == "Ich versuche, zu einkaufen.");
        Assert.All(zuInfinitiveTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel manGeneralSubjectTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-man-as-general-subject");
        Assert.Equal(1, manGeneralSubjectTopic.ContentRevision);
        Assert.Equal("A2", manGeneralSubjectTopic.CefrLevel);
        Assert.Equal("pronouns", manGeneralSubjectTopic.GrammarCategory);
        Assert.Equal(10, manGeneralSubjectTopic.Sections.Count);
        Assert.True(manGeneralSubjectTopic.Examples.Count >= 90);
        Assert.True(manGeneralSubjectTopic.CommonMistakes.Count >= 35);
        Assert.True(manGeneralSubjectTopic.RuleSummaries.Count >= 20);
        Assert.Equal(71, manGeneralSubjectTopic.LinkedWords.Count);
        Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", manGeneralSubjectTopic.PrerequisiteSlugs);
        Assert.Contains("a1-pronoun-and-verb-agreement", manGeneralSubjectTopic.PrerequisiteSlugs);
        Assert.Contains("a2-modal-verbs-in-more-detail", manGeneralSubjectTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-sentence-negation", manGeneralSubjectTopic.PrerequisiteSlugs);
        Assert.Contains("a1-formal-sie", manGeneralSubjectTopic.PrerequisiteSlugs);
        Assert.Contains("b2-passive-with-modal-verbs", manGeneralSubjectTopic.RelatedTopicSlugs);
        Assert.Contains("b1-reported-requests-and-polite-questions", manGeneralSubjectTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel manCorePatternsSection = Assert.Single(manGeneralSubjectTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel manStructureSection = Assert.Single(manGeneralSubjectTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel manWordOrderSection = Assert.Single(manGeneralSubjectTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel manComparisonSection = Assert.Single(manGeneralSubjectTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel manCommonPatternsSection = Assert.Single(manGeneralSubjectTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel manPracticeSection = Assert.Single(manGeneralSubjectTopic.Sections, section => section.SectionKey == "practice-advice");
        Assert.All(languages, language =>
        {
            Assert.True(manGeneralSubjectTopic.TitleLocalized.ContainsKey(language));
            Assert.True(manGeneralSubjectTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(manGeneralSubjectTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(manGeneralSubjectTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.Contains("\"rule-list\"", manCorePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", manStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", manWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", manComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", manCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", manPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(manGeneralSubjectTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(manGeneralSubjectTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(manGeneralSubjectTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(manGeneralSubjectTopic.Examples, example => example.GermanText == "Man sagt in Deutschland oft Hallo.");
        Assert.Contains(manGeneralSubjectTopic.Examples, example => example.GermanText == "Man kann hier parken.");
        Assert.Contains(manGeneralSubjectTopic.Examples, example => example.GermanText == "Man spricht hier Deutsch.");
        Assert.Contains(manGeneralSubjectTopic.CommonMistakes, mistake => mistake.WrongText == "Man bin müde.");
        Assert.Contains(manGeneralSubjectTopic.CommonMistakes, mistake => mistake.WrongText == "Man kannst hier parken.");
        Assert.Contains(manGeneralSubjectTopic.CommonMistakes, mistake => mistake.WrongText == "man = ich");
        Assert.All(manGeneralSubjectTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel esGibtTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-es-gibt");
        Assert.Equal(1, esGibtTopic.ContentRevision);
        Assert.Equal("A2", esGibtTopic.CefrLevel);
        Assert.Equal("verbs", esGibtTopic.GrammarCategory);
        Assert.Equal(10, esGibtTopic.Sections.Count);
        Assert.True(esGibtTopic.Examples.Count >= 90);
        Assert.True(esGibtTopic.CommonMistakes.Count >= 35);
        Assert.True(esGibtTopic.RuleSummaries.Count >= 20);
        Assert.Equal(72, esGibtTopic.LinkedWords.Count);
        Assert.Contains("a1-simple-accusative-introduction", esGibtTopic.PrerequisiteSlugs);
        Assert.Contains("a1-kein-versus-nicht-basics", esGibtTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-location-phrases", esGibtTopic.PrerequisiteSlugs);
        Assert.Contains("a2-accusative-versus-dative-basics", esGibtTopic.PrerequisiteSlugs);
        Assert.Contains("a2-grammar-for-appointments", esGibtTopic.RelatedTopicSlugs);
        Assert.Contains("a2-prepositions-with-accusative", esGibtTopic.RelatedTopicSlugs);
        Assert.Contains("a1-basic-appointment-phrases", esGibtTopic.RelatedTopicSlugs);
        Assert.Contains("b1-formal-email-sentence-structure", esGibtTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel esGibtCorePatternsSection = Assert.Single(esGibtTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel esGibtStructureSection = Assert.Single(esGibtTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel esGibtWordOrderSection = Assert.Single(esGibtTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel esGibtComparisonSection = Assert.Single(esGibtTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel esGibtCommonPatternsSection = Assert.Single(esGibtTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel esGibtPracticeSection = Assert.Single(esGibtTopic.Sections, section => section.SectionKey == "practice-advice");

        Assert.All(languages, language =>
        {
            Assert.True(esGibtTopic.TitleLocalized.ContainsKey(language));
            Assert.True(esGibtTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(esGibtTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.All(esGibtTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.Contains("\"rule-list\"", esGibtCorePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", esGibtStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", esGibtWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", esGibtComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", esGibtCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", esGibtPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(esGibtTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(esGibtTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(esGibtTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(esGibtTopic.Examples, example => example.GermanText == "Es gibt einen Park.");
        Assert.Contains(esGibtTopic.Examples, example => example.GermanText == "Gibt es hier einen Arzt?");
        Assert.Contains(esGibtTopic.Examples, example => example.GermanText == "Es gibt keinen Termin.");
        Assert.Contains(esGibtTopic.CommonMistakes, mistake => mistake.WrongText == "Es gibt ein Park.");
        Assert.Contains(esGibtTopic.CommonMistakes, mistake => mistake.WrongText == "Gibt einen Park?");
        Assert.Contains(esGibtTopic.CommonMistakes, mistake => mistake.WrongText == "Es ist einen Park.");
        Assert.All(esGibtTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel wuerdeTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-polite-forms-with-wuerde");
        Assert.Equal(1, wuerdeTopic.ContentRevision);
        Assert.Equal("A2", wuerdeTopic.CefrLevel);
        Assert.Equal("konjunktiv", wuerdeTopic.GrammarCategory);
        Assert.Equal(10, wuerdeTopic.Sections.Count);
        Assert.True(wuerdeTopic.Examples.Count >= 90);
        Assert.True(wuerdeTopic.CommonMistakes.Count >= 35);
        Assert.True(wuerdeTopic.RuleSummaries.Count >= 20);
        Assert.Equal(85, wuerdeTopic.LinkedWords.Count);
        Assert.Contains("a1-polite-requests-with-moechte", wuerdeTopic.PrerequisiteSlugs);
        Assert.Contains("a2-modal-verbs-in-more-detail", wuerdeTopic.PrerequisiteSlugs);
        Assert.Contains("a2-indirect-questions-introduction", wuerdeTopic.PrerequisiteSlugs);
        Assert.Contains("a2-zu-plus-infinitive-introduction", wuerdeTopic.PrerequisiteSlugs);
        Assert.Contains("a2-imperative-formal-and-informal", wuerdeTopic.PrerequisiteSlugs);
        Assert.Contains("b1-konjunktiv-ii-for-polite-requests", wuerdeTopic.RelatedTopicSlugs);
        Assert.Contains("b1-konjunktiv-ii-with-waere-haette-wuerde", wuerdeTopic.RelatedTopicSlugs);
        Assert.Contains("b1-formal-email-sentence-structure", wuerdeTopic.RelatedTopicSlugs);
        Assert.Contains("b1-complaint-sentence-patterns", wuerdeTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel wuerdePatternsSection = Assert.Single(wuerdeTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel wuerdeFormsSection = Assert.Single(wuerdeTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel wuerdeWordOrderSection = Assert.Single(wuerdeTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel wuerdeComparisonSection = Assert.Single(wuerdeTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel wuerdeCommonPatternsSection = Assert.Single(wuerdeTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel wuerdePracticeSection = Assert.Single(wuerdeTopic.Sections, section => section.SectionKey == "practice-advice");

        Assert.All(languages, language =>
        {
            Assert.True(wuerdeTopic.TitleLocalized.ContainsKey(language));
            Assert.True(wuerdeTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(wuerdeTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"rule-list\"", wuerdePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", wuerdeFormsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", wuerdeWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", wuerdeComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", wuerdeCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", wuerdePracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(wuerdeTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(wuerdeTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(wuerdeTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(wuerdeTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(wuerdeTopic.Examples, example => example.GermanText == "Ich würde gern kommen.");
        Assert.Contains(wuerdeTopic.Examples, example => example.GermanText == "Würden Sie bitte warten?");
        Assert.Contains(wuerdeTopic.Examples, example => example.GermanText == "Ich würde gern einen Termin machen.");
        Assert.Contains(wuerdeTopic.CommonMistakes, mistake => mistake.WrongText == "Ich würde gern komme.");
        Assert.Contains(wuerdeTopic.CommonMistakes, mistake => mistake.WrongText == "Würde Sie bitte warten?");
        Assert.Contains(wuerdeTopic.CommonMistakes, mistake => mistake.WrongText == "Ich würde einen Kaffee.");
        Assert.All(wuerdeTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel emailTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-simple-email-grammar");
        Assert.Equal(1, emailTopic.ContentRevision);
        Assert.Equal("A2", emailTopic.CefrLevel);
        Assert.Equal("word-order", emailTopic.GrammarCategory);
        Assert.Equal(10, emailTopic.Sections.Count);
        Assert.True(emailTopic.Examples.Count >= 90);
        Assert.True(emailTopic.CommonMistakes.Count >= 35);
        Assert.True(emailTopic.RuleSummaries.Count >= 20);
        Assert.Equal(80, emailTopic.LinkedWords.Count);
        Assert.Contains("a1-formal-sie", emailTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-appointment-phrases", emailTopic.PrerequisiteSlugs);
        Assert.Contains("a2-dass-clauses", emailTopic.PrerequisiteSlugs);
        Assert.Contains("a2-weil-clauses", emailTopic.PrerequisiteSlugs);
        Assert.Contains("a2-indirect-questions-introduction", emailTopic.PrerequisiteSlugs);
        Assert.Contains("a2-polite-forms-with-wuerde", emailTopic.PrerequisiteSlugs);
        Assert.Contains("b1-formal-email-sentence-structure", emailTopic.RelatedTopicSlugs);
        Assert.Contains("b1-grammar-for-b1-writing-exam", emailTopic.RelatedTopicSlugs);
        Assert.Contains("b1-complaint-sentence-patterns", emailTopic.RelatedTopicSlugs);
        Assert.Contains("b2-formal-complaint-grammar", emailTopic.RelatedTopicSlugs);
        Assert.Contains("b2-b2-exam-writing-structures", emailTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel emailPatternsSection = Assert.Single(emailTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel emailStructureSection = Assert.Single(emailTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel emailWordOrderSection = Assert.Single(emailTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel emailContextsSection = Assert.Single(emailTopic.Sections, section => section.SectionKey == "common-contexts");
        ParsedGrammarSectionModel emailComparisonSection = Assert.Single(emailTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel emailCommonPatternsSection = Assert.Single(emailTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel emailPracticeSection = Assert.Single(emailTopic.Sections, section => section.SectionKey == "practice-advice");

        Assert.All(languages, language =>
        {
            Assert.True(emailTopic.TitleLocalized.ContainsKey(language));
            Assert.True(emailTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(emailTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"rule-list\"", emailPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", emailStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", emailWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", emailContextsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", emailComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", emailCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", emailPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(emailTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(emailTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(emailTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(emailTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(emailTopic.Examples, example => example.GermanText == "Sehr geehrte Frau Müller,");
        Assert.Contains(emailTopic.Examples, example => example.GermanText == "Ich schreibe Ihnen, weil ich einen Termin brauche.");
        Assert.Contains(emailTopic.Examples, example => example.GermanText == "Mit freundlichen Grüßen");
        Assert.Contains(emailTopic.CommonMistakes, mistake => mistake.WrongText == "Ich schreibe Sie.");
        Assert.Contains(emailTopic.CommonMistakes, mistake => mistake.WrongText == "Ich kann morgen nicht kommen, weil ich bin krank.");
        Assert.Contains(emailTopic.CommonMistakes, mistake => mistake.WrongText == "Könnten Sie bitte helfen mir?");
        Assert.All(emailTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel phoneTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-grammar-for-phone-calls");
        Assert.Equal(1, phoneTopic.ContentRevision);
        Assert.Equal("A2", phoneTopic.CefrLevel);
        Assert.Equal("word-order", phoneTopic.GrammarCategory);
        Assert.Equal(10, phoneTopic.Sections.Count);
        Assert.True(phoneTopic.Examples.Count >= 90);
        Assert.True(phoneTopic.CommonMistakes.Count >= 35);
        Assert.True(phoneTopic.RuleSummaries.Count >= 20);
        Assert.Equal(79, phoneTopic.LinkedWords.Count);
        Assert.Contains("a1-formal-sie", phoneTopic.PrerequisiteSlugs);
        Assert.Contains("a1-basic-appointment-phrases", phoneTopic.PrerequisiteSlugs);
        Assert.Contains("a1-question-answer-sentence-patterns", phoneTopic.PrerequisiteSlugs);
        Assert.Contains("a2-weil-clauses", phoneTopic.PrerequisiteSlugs);
        Assert.Contains("a2-indirect-questions-introduction", phoneTopic.PrerequisiteSlugs);
        Assert.Contains("a2-polite-forms-with-wuerde", phoneTopic.PrerequisiteSlugs);
        Assert.Contains("a2-simple-email-grammar", phoneTopic.PrerequisiteSlugs);
        Assert.Contains("b1-formal-email-sentence-structure", phoneTopic.RelatedTopicSlugs);
        Assert.Contains("b1-complaint-sentence-patterns", phoneTopic.RelatedTopicSlugs);
        Assert.Contains("b1-grammar-for-b1-speaking-exam", phoneTopic.RelatedTopicSlugs);
        Assert.Contains("b2-job-interview-grammar", phoneTopic.RelatedTopicSlugs);
        Assert.DoesNotContain("b2-formal-phone-call", phoneTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel phonePatternsSection = Assert.Single(phoneTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel phoneStructureSection = Assert.Single(phoneTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel phoneWordOrderSection = Assert.Single(phoneTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel phoneContextsSection = Assert.Single(phoneTopic.Sections, section => section.SectionKey == "common-contexts");
        ParsedGrammarSectionModel phoneComparisonSection = Assert.Single(phoneTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel phoneCommonPatternsSection = Assert.Single(phoneTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel phonePracticeSection = Assert.Single(phoneTopic.Sections, section => section.SectionKey == "practice-advice");

        Assert.All(languages, language =>
        {
            Assert.True(phoneTopic.TitleLocalized.ContainsKey(language));
            Assert.True(phoneTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(phoneTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"rule-list\"", phonePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", phoneStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", phoneWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", phoneContextsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", phoneComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", phoneCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", phonePracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(phoneTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(phoneTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(phoneTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(phoneTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(phoneTopic.Examples, example => example.GermanText == "Guten Tag, mein Name ist Ali Rahimi.");
        Assert.Contains(phoneTopic.Examples, example => example.GermanText == "Können Sie das bitte wiederholen?");
        Assert.Contains(phoneTopic.Examples, example => example.GermanText == "Also, der Termin ist am Montag um 10 Uhr.");
        Assert.Contains(phoneTopic.CommonMistakes, mistake => mistake.WrongText == "Ich rufe, weil ich einen Termin brauche.");
        Assert.Contains(phoneTopic.CommonMistakes, mistake => mistake.WrongText == "Können Sie sagen mir, wann der Termin ist?");
        Assert.Contains(phoneTopic.CommonMistakes, mistake => mistake.WrongText == "Auf Wiedersehen.");
        Assert.All(phoneTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel appointmentTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-grammar-for-appointments");
        Assert.Equal(1, appointmentTopic.ContentRevision);
        Assert.Equal("A2", appointmentTopic.CefrLevel);
        Assert.Equal("tenses", appointmentTopic.GrammarCategory);
        Assert.Equal(10, appointmentTopic.Sections.Count);
        Assert.True(appointmentTopic.Examples.Count >= 90);
        Assert.True(appointmentTopic.CommonMistakes.Count >= 35);
        Assert.True(appointmentTopic.RuleSummaries.Count >= 20);
        Assert.Equal(82, appointmentTopic.LinkedWords.Count);
        Assert.Contains("a1-basic-appointment-phrases", appointmentTopic.PrerequisiteSlugs);
        Assert.Contains("a2-simple-email-grammar", appointmentTopic.PrerequisiteSlugs);
        Assert.Contains("a2-grammar-for-phone-calls", appointmentTopic.PrerequisiteSlugs);
        Assert.Contains("a2-polite-forms-with-wuerde", appointmentTopic.PrerequisiteSlugs);
        Assert.Contains("a2-weil-clauses", appointmentTopic.PrerequisiteSlugs);
        Assert.Contains("a2-dass-clauses", appointmentTopic.PrerequisiteSlugs);
        Assert.Contains("a1-simple-accusative-introduction", appointmentTopic.PrerequisiteSlugs);
        Assert.Contains("a2-accusative-versus-dative-basics", appointmentTopic.PrerequisiteSlugs);
        Assert.Contains("a2-grammar-for-doctor-visits", appointmentTopic.RelatedTopicSlugs);
        Assert.Contains("a2-grammar-for-school-and-kindergarten-communication", appointmentTopic.RelatedTopicSlugs);
        Assert.Contains("b1-formal-email-sentence-structure", appointmentTopic.RelatedTopicSlugs);
        Assert.Contains("b1-complaint-sentence-patterns", appointmentTopic.RelatedTopicSlugs);
        Assert.Contains("b1-grammar-for-b1-speaking-exam", appointmentTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel appointmentPatternsSection = Assert.Single(appointmentTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel appointmentStructureSection = Assert.Single(appointmentTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel appointmentWordOrderSection = Assert.Single(appointmentTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel appointmentContextsSection = Assert.Single(appointmentTopic.Sections, section => section.SectionKey == "common-contexts");
        ParsedGrammarSectionModel appointmentComparisonSection = Assert.Single(appointmentTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel appointmentCommonPatternsSection = Assert.Single(appointmentTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel appointmentPracticeSection = Assert.Single(appointmentTopic.Sections, section => section.SectionKey == "practice-advice");

        Assert.All(languages, language =>
        {
            Assert.True(appointmentTopic.TitleLocalized.ContainsKey(language));
            Assert.True(appointmentTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(appointmentTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"rule-list\"", appointmentPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", appointmentStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", appointmentWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", appointmentContextsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", appointmentComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", appointmentCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", appointmentPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(appointmentTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(appointmentTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(appointmentTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(appointmentTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(appointmentTopic.Examples, example => example.GermanText == "Ich brauche einen Termin.");
        Assert.Contains(appointmentTopic.Examples, example => example.GermanText == "Ich möchte den Termin verschieben.");
        Assert.Contains(appointmentTopic.Examples, example => example.GermanText == "Ich kann heute nicht kommen, weil ich krank bin.");
        Assert.Contains(appointmentTopic.CommonMistakes, mistake => mistake.WrongText == "Ich brauche ein Termin.");
        Assert.Contains(appointmentTopic.CommonMistakes, mistake => mistake.WrongText == "Ich kann nicht kommen, weil ich bin krank.");
        Assert.Contains(appointmentTopic.CommonMistakes, mistake => mistake.WrongText == "Ich komme am 10 Uhr.");
        Assert.All(appointmentTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel doctorTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-grammar-for-doctor-visits");
        Assert.Equal(1, doctorTopic.ContentRevision);
        Assert.Equal("A2", doctorTopic.CefrLevel);
        Assert.Equal("cases", doctorTopic.GrammarCategory);
        Assert.Equal(10, doctorTopic.Sections.Count);
        Assert.True(doctorTopic.Examples.Count >= 90);
        Assert.True(doctorTopic.CommonMistakes.Count >= 35);
        Assert.True(doctorTopic.RuleSummaries.Count >= 20);
        Assert.Equal(88, doctorTopic.LinkedWords.Count);
        Assert.Contains("a2-grammar-for-appointments", doctorTopic.PrerequisiteSlugs);
        Assert.Contains("a1-haben-in-praesens", doctorTopic.PrerequisiteSlugs);
        Assert.Contains("a1-sein-in-praesens", doctorTopic.PrerequisiteSlugs);
        Assert.Contains("a2-possessive-pronouns-in-cases", doctorTopic.PrerequisiteSlugs);
        Assert.Contains("a2-prepositions-with-dative", doctorTopic.PrerequisiteSlugs);
        Assert.Contains("a1-formal-sie", doctorTopic.PrerequisiteSlugs);
        Assert.Contains("a2-simple-email-grammar", doctorTopic.RelatedTopicSlugs);
        Assert.Contains("a2-grammar-for-phone-calls", doctorTopic.RelatedTopicSlugs);
        Assert.Contains("b1-grammar-for-b1-speaking-exam", doctorTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel doctorSafetySection = Assert.Single(doctorTopic.Sections, section => section.SectionKey == "what-this-topic-is");
        ParsedGrammarSectionModel doctorPatternsSection = Assert.Single(doctorTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel doctorStructureSection = Assert.Single(doctorTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel doctorWordOrderSection = Assert.Single(doctorTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel doctorContextsSection = Assert.Single(doctorTopic.Sections, section => section.SectionKey == "common-contexts");
        ParsedGrammarSectionModel doctorComparisonSection = Assert.Single(doctorTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel doctorCommonPatternsSection = Assert.Single(doctorTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel doctorPracticeSection = Assert.Single(doctorTopic.Sections, section => section.SectionKey == "practice-advice");

        Assert.All(languages, language =>
        {
            Assert.True(doctorTopic.TitleLocalized.ContainsKey(language));
            Assert.True(doctorTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(doctorTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"callout\"", doctorSafetySection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"rule-list\"", doctorPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", doctorStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", doctorWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", doctorContextsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", doctorComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", doctorCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", doctorPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(doctorTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(doctorTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(doctorTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(doctorTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(doctorTopic.Examples, example => example.GermanText == "Ich habe Kopfschmerzen.");
        Assert.Contains(doctorTopic.Examples, example => example.GermanText == "Mein Kopf tut weh.");
        Assert.Contains(doctorTopic.Examples, example => example.GermanText == "Seit wann haben Sie das?");
        Assert.Contains(doctorTopic.Examples, example => example.GermanText == "Ich brauche eine Krankmeldung.");
        Assert.Contains(doctorTopic.CommonMistakes, mistake => mistake.WrongText == "Ich bin Kopfschmerzen.");
        Assert.Contains(doctorTopic.CommonMistakes, mistake => mistake.WrongText == "Mein Kopf tut weht.");
        Assert.Contains(doctorTopic.CommonMistakes, mistake => mistake.WrongText == "Seit wann Sie haben das?");
        Assert.All(doctorTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel schoolTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-grammar-for-school-and-kindergarten-communication");
        Assert.Equal(1, schoolTopic.ContentRevision);
        Assert.Equal("A2", schoolTopic.CefrLevel);
        Assert.Equal("word-order", schoolTopic.GrammarCategory);
        Assert.Equal(10, schoolTopic.Sections.Count);
        Assert.True(schoolTopic.Examples.Count >= 90);
        Assert.True(schoolTopic.CommonMistakes.Count >= 35);
        Assert.True(schoolTopic.RuleSummaries.Count >= 20);
        Assert.Equal(89, schoolTopic.LinkedWords.Count);
        Assert.Contains("a2-grammar-for-appointments", schoolTopic.PrerequisiteSlugs);
        Assert.Contains("a2-simple-email-grammar", schoolTopic.PrerequisiteSlugs);
        Assert.Contains("a2-grammar-for-phone-calls", schoolTopic.PrerequisiteSlugs);
        Assert.Contains("a2-weil-clauses", schoolTopic.PrerequisiteSlugs);
        Assert.Contains("a2-indirect-questions-introduction", schoolTopic.PrerequisiteSlugs);
        Assert.Contains("a1-formal-sie", schoolTopic.PrerequisiteSlugs);
        Assert.Contains("a2-grammar-for-doctor-visits", schoolTopic.RelatedTopicSlugs);
        Assert.Contains("b1-formal-email-sentence-structure", schoolTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel schoolPatternsSection = Assert.Single(schoolTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel schoolStructureSection = Assert.Single(schoolTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel schoolWordOrderSection = Assert.Single(schoolTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel schoolContextsSection = Assert.Single(schoolTopic.Sections, section => section.SectionKey == "common-contexts");
        ParsedGrammarSectionModel schoolComparisonSection = Assert.Single(schoolTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel schoolCommonPatternsSection = Assert.Single(schoolTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel schoolPracticeSection = Assert.Single(schoolTopic.Sections, section => section.SectionKey == "practice-advice");

        Assert.All(languages, language =>
        {
            Assert.True(schoolTopic.TitleLocalized.ContainsKey(language));
            Assert.True(schoolTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(schoolTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"rule-list\"", schoolPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", schoolStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", schoolWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", schoolContextsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", schoolComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", schoolCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", schoolPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(schoolTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(schoolTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(schoolTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(schoolTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(schoolTopic.Examples, example => example.GermanText == "Mein Kind kommt heute nicht.");
        Assert.Contains(schoolTopic.Examples, example => example.GermanText == "Ich hole mein Kind um 15 Uhr ab.");
        Assert.Contains(schoolTopic.Examples, example => example.GermanText == "Ich möchte einen Termin mit Ihnen vereinbaren.");
        Assert.Contains(schoolTopic.CommonMistakes, mistake => mistake.WrongText == "Mein Kind kommen nicht.");
        Assert.Contains(schoolTopic.CommonMistakes, mistake => mistake.WrongText == "Ich hole mein Kind um 15 Uhr.");
        Assert.Contains(schoolTopic.CommonMistakes, mistake => mistake.WrongText == "Können Sie sagen mir?");
        Assert.All(schoolTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel a2MistakesTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-common-a2-mistakes");
        Assert.Equal(1, a2MistakesTopic.ContentRevision);
        Assert.Equal("A2", a2MistakesTopic.CefrLevel);
        Assert.Equal("cases", a2MistakesTopic.GrammarCategory);
        Assert.Equal(10, a2MistakesTopic.Sections.Count);
        Assert.True(a2MistakesTopic.Examples.Count >= 90);
        Assert.True(a2MistakesTopic.CommonMistakes.Count >= 70);
        Assert.True(a2MistakesTopic.RuleSummaries.Count >= 25);
        Assert.Equal(98, a2MistakesTopic.LinkedWords.Count);
        Assert.Contains("a2-perfekt-with-haben", a2MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-perfekt-with-sein", a2MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-dative-case-basics", a2MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-accusative-versus-dative-basics", a2MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", a2MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-grammar-for-school-and-kindergarten-communication", a2MistakesTopic.PrerequisiteSlugs);
        Assert.Contains("a2-a2-grammar-review-map", a2MistakesTopic.RelatedTopicSlugs);
        Assert.Contains("b1-b1-mistake-patterns", a2MistakesTopic.RelatedTopicSlugs);
        Assert.Contains("b1-grammar-for-b1-writing-exam", a2MistakesTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel a2MistakePatternsSection = Assert.Single(a2MistakesTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel a2MistakeStructureSection = Assert.Single(a2MistakesTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel a2MistakeWordOrderSection = Assert.Single(a2MistakesTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel a2MistakeContextsSection = Assert.Single(a2MistakesTopic.Sections, section => section.SectionKey == "common-contexts");
        ParsedGrammarSectionModel a2MistakeComparisonSection = Assert.Single(a2MistakesTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel a2MistakeCommonPatternsSection = Assert.Single(a2MistakesTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel a2MistakePracticeSection = Assert.Single(a2MistakesTopic.Sections, section => section.SectionKey == "practice-advice");

        Assert.All(languages, language =>
        {
            Assert.True(a2MistakesTopic.TitleLocalized.ContainsKey(language));
            Assert.True(a2MistakesTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(a2MistakesTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"rule-list\"", a2MistakePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", a2MistakeStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", a2MistakeWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", a2MistakeContextsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", a2MistakeComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", a2MistakeCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", a2MistakePracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(a2MistakesTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(a2MistakesTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(a2MistakesTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(a2MistakesTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(a2MistakesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich gelernt Deutsch.");
        Assert.Contains(a2MistakesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich helfe der Mann.");
        Assert.Contains(a2MistakesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich glaube, dass er kommt morgen.");
        Assert.Contains(a2MistakesTopic.CommonMistakes, mistake => mistake.WrongText == "Können Sie sagen mir?");
        Assert.All(a2MistakesTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel connectorTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-a2-connectors-overview");
        Assert.Equal(1, connectorTopic.ContentRevision);
        Assert.Equal("A2", connectorTopic.CefrLevel);
        Assert.Equal("connectors", connectorTopic.GrammarCategory);
        Assert.Equal(10, connectorTopic.Sections.Count);
        Assert.True(connectorTopic.Examples.Count >= 90);
        Assert.True(connectorTopic.CommonMistakes.Count >= 45);
        Assert.True(connectorTopic.RuleSummaries.Count >= 22);
        Assert.Equal(76, connectorTopic.LinkedWords.Count);
        Assert.Contains("a1-simple-conjunctions-und-aber", connectorTopic.PrerequisiteSlugs);
        Assert.Contains("a2-dass-clauses", connectorTopic.PrerequisiteSlugs);
        Assert.Contains("a2-weil-clauses", connectorTopic.PrerequisiteSlugs);
        Assert.Contains("a2-wenn-for-conditions", connectorTopic.PrerequisiteSlugs);
        Assert.Contains("a2-denn-versus-weil", connectorTopic.PrerequisiteSlugs);
        Assert.Contains("a2-time-clauses-bevor-and-nachdem-introduction", connectorTopic.PrerequisiteSlugs);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", connectorTopic.PrerequisiteSlugs);
        Assert.Contains("b1-connectors-for-opinion", connectorTopic.RelatedTopicSlugs);
        Assert.Contains("b1-connectors-for-cause-and-effect", connectorTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel connectorPatternsSection = Assert.Single(connectorTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel connectorStructureSection = Assert.Single(connectorTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel connectorWordOrderSection = Assert.Single(connectorTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel connectorContextsSection = Assert.Single(connectorTopic.Sections, section => section.SectionKey == "common-contexts");
        ParsedGrammarSectionModel connectorComparisonSection = Assert.Single(connectorTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel connectorCommonPatternsSection = Assert.Single(connectorTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel connectorPracticeSection = Assert.Single(connectorTopic.Sections, section => section.SectionKey == "practice-advice");

        Assert.All(languages, language =>
        {
            Assert.True(connectorTopic.TitleLocalized.ContainsKey(language));
            Assert.True(connectorTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(connectorTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"rule-list\"", connectorPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", connectorStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", connectorWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", connectorContextsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", connectorComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", connectorCommonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", connectorPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(connectorTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(connectorTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(connectorTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(connectorTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(connectorTopic.Examples, example => example.GermanText == "Ich komme nicht, denn ich bin krank.");
        Assert.Contains(connectorTopic.Examples, example => example.GermanText == "Ich komme nicht, weil ich krank bin.");
        Assert.Contains(connectorTopic.Examples, example => example.GermanText == "Wenn ich Zeit habe, komme ich.");
        Assert.Contains(connectorTopic.CommonMistakes, mistake => mistake.WrongText == "Ich komme nicht, denn ich krank bin.");
        Assert.Contains(connectorTopic.CommonMistakes, mistake => mistake.WrongText == "Ich komme nicht, weil ich bin krank.");
        Assert.Contains(connectorTopic.CommonMistakes, mistake => mistake.WrongText == "Wenn ich Zeit habe, ich komme.");
        Assert.All(connectorTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel caseReviewTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-a2-case-review");
        Assert.Equal(1, caseReviewTopic.ContentRevision);
        Assert.Equal("A2", caseReviewTopic.CefrLevel);
        Assert.Equal("cases", caseReviewTopic.GrammarCategory);
        Assert.Equal(10, caseReviewTopic.Sections.Count);
        Assert.True(caseReviewTopic.Examples.Count >= 90);
        Assert.True(caseReviewTopic.CommonMistakes.Count >= 35);
        Assert.True(caseReviewTopic.RuleSummaries.Count >= 20);
        Assert.Equal(90, caseReviewTopic.LinkedWords.Count);
        Assert.Contains("a1-nominative-case", caseReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a1-simple-accusative-introduction", caseReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a2-dative-case-basics", caseReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a2-accusative-versus-dative-basics", caseReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a2-dative-pronouns", caseReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a2-accusative-pronouns", caseReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a2-possessive-pronouns-in-cases", caseReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a2-prepositions-with-dative", caseReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a2-prepositions-with-accusative", caseReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a2-wechselpraepositionen-introduction", caseReviewTopic.PrerequisiteSlugs);
        Assert.Contains("b1-b1-case-review", caseReviewTopic.RelatedTopicSlugs);
        Assert.Contains("b1-genitive-introduction", caseReviewTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel caseReviewCoreSection = Assert.Single(caseReviewTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel caseReviewFormSection = Assert.Single(caseReviewTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel caseReviewFocusSection = Assert.Single(caseReviewTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel caseReviewMeaningSection = Assert.Single(caseReviewTopic.Sections, section => section.SectionKey == "meaning-and-use");
        ParsedGrammarSectionModel caseReviewComparisonSection = Assert.Single(caseReviewTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel caseReviewPatternsSection = Assert.Single(caseReviewTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel caseReviewPracticeSection = Assert.Single(caseReviewTopic.Sections, section => section.SectionKey == "practice-advice");

        Assert.All(languages, language =>
        {
            Assert.True(caseReviewTopic.TitleLocalized.ContainsKey(language));
            Assert.True(caseReviewTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(caseReviewTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", caseReviewCoreSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", caseReviewFormSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", caseReviewFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", caseReviewMeaningSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", caseReviewComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", caseReviewPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", caseReviewPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(caseReviewTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(caseReviewTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(caseReviewTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(caseReviewTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(caseReviewTopic.Examples, example => example.GermanText == "Der Lehrer gibt dem Schüler das Buch.");
        Assert.Contains(caseReviewTopic.Examples, example => example.GermanText == "Ich danke Ihnen.");
        Assert.Contains(caseReviewTopic.Examples, example => example.GermanText == "Ich fahre mit dem Bus.");
        Assert.Contains(caseReviewTopic.CommonMistakes, mistake => mistake.WrongText == "Ich helfe den Lehrer.");
        Assert.Contains(caseReviewTopic.CommonMistakes, mistake => mistake.WrongText == "Kannst du mich helfen?");
        Assert.Contains(caseReviewTopic.CommonMistakes, mistake => mistake.WrongText == "Ich fahre mit der Bus.");
        Assert.All(caseReviewTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel verbReviewTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-a2-verb-review");
        Assert.Equal(1, verbReviewTopic.ContentRevision);
        Assert.Equal("A2", verbReviewTopic.CefrLevel);
        Assert.Equal("verbs", verbReviewTopic.GrammarCategory);
        Assert.Equal(10, verbReviewTopic.Sections.Count);
        Assert.True(verbReviewTopic.Examples.Count >= 90);
        Assert.True(verbReviewTopic.CommonMistakes.Count >= 35);
        Assert.True(verbReviewTopic.RuleSummaries.Count >= 20);
        Assert.Equal(90, verbReviewTopic.LinkedWords.Count);
        Assert.Contains("a2-perfekt-with-haben", verbReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a2-perfekt-with-sein", verbReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a2-common-irregular-participles", verbReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a2-modal-verbs-in-more-detail", verbReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a2-separable-verbs-in-perfekt", verbReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a2-reflexive-verbs-introduction", verbReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a2-zu-plus-infinitive-introduction", verbReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a2-polite-forms-with-wuerde", verbReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", verbReviewTopic.PrerequisiteSlugs);
        Assert.Contains("b1-b1-verb-tense-review", verbReviewTopic.RelatedTopicSlugs);
        Assert.Contains("b1-infinitive-with-zu", verbReviewTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel verbReviewCoreSection = Assert.Single(verbReviewTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel verbReviewFormSection = Assert.Single(verbReviewTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel verbReviewFocusSection = Assert.Single(verbReviewTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel verbReviewComparisonSection = Assert.Single(verbReviewTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel verbReviewPatternsSection = Assert.Single(verbReviewTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel verbReviewPracticeSection = Assert.Single(verbReviewTopic.Sections, section => section.SectionKey == "practice-advice");

        Assert.All(languages, language =>
        {
            Assert.True(verbReviewTopic.TitleLocalized.ContainsKey(language));
            Assert.True(verbReviewTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(verbReviewTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", verbReviewCoreSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", verbReviewFormSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", verbReviewFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", verbReviewComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", verbReviewPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", verbReviewPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(verbReviewTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(verbReviewTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(verbReviewTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(verbReviewTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(verbReviewTopic.Examples, example => example.GermanText == "Ich habe Deutsch gelernt.");
        Assert.Contains(verbReviewTopic.Examples, example => example.GermanText == "Ich bin nach Hause gegangen.");
        Assert.Contains(verbReviewTopic.Examples, example => example.GermanText == "Ich würde gern kommen.");
        Assert.Contains(verbReviewTopic.CommonMistakes, mistake => mistake.WrongText == "Ich muss arbeite.");
        Assert.Contains(verbReviewTopic.CommonMistakes, mistake => mistake.WrongText == "Ich habe nach Hause gegangen.");
        Assert.Contains(verbReviewTopic.CommonMistakes, mistake => mistake.WrongText == "Ich wasche sich.");
        Assert.All(verbReviewTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel a2ReviewMapTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "a2-a2-grammar-review-map");
        Assert.Equal(1, a2ReviewMapTopic.ContentRevision);
        Assert.Equal("A2", a2ReviewMapTopic.CefrLevel);
        Assert.Equal("word-order", a2ReviewMapTopic.GrammarCategory);
        Assert.Equal(10, a2ReviewMapTopic.Sections.Count);
        Assert.True(a2ReviewMapTopic.Examples.Count >= 90);
        Assert.True(a2ReviewMapTopic.CommonMistakes.Count >= 35);
        Assert.True(a2ReviewMapTopic.RuleSummaries.Count >= 20);
        Assert.Equal(90, a2ReviewMapTopic.LinkedWords.Count);
        Assert.Contains("a2-perfekt-with-haben", a2ReviewMapTopic.PrerequisiteSlugs);
        Assert.Contains("a2-a2-verb-review", a2ReviewMapTopic.PrerequisiteSlugs);
        Assert.Contains("a2-a2-case-review", a2ReviewMapTopic.PrerequisiteSlugs);
        Assert.Contains("a2-a2-connectors-overview", a2ReviewMapTopic.PrerequisiteSlugs);
        Assert.Contains("b1-b1-grammar-review-map", a2ReviewMapTopic.RelatedTopicSlugs);
        Assert.Contains("b1-relative-clauses-basics", a2ReviewMapTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel a2ReviewCoreSection = Assert.Single(a2ReviewMapTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel a2ReviewFormSection = Assert.Single(a2ReviewMapTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel a2ReviewFocusSection = Assert.Single(a2ReviewMapTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel a2ReviewComparisonSection = Assert.Single(a2ReviewMapTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel a2ReviewPatternsSection = Assert.Single(a2ReviewMapTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel a2ReviewPracticeSection = Assert.Single(a2ReviewMapTopic.Sections, section => section.SectionKey == "practice-advice");

        Assert.All(languages, language =>
        {
            Assert.True(a2ReviewMapTopic.TitleLocalized.ContainsKey(language));
            Assert.True(a2ReviewMapTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(a2ReviewMapTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", a2ReviewCoreSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", a2ReviewFormSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", a2ReviewFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", a2ReviewComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", a2ReviewPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", a2ReviewPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(a2ReviewMapTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(a2ReviewMapTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(a2ReviewMapTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(a2ReviewMapTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(a2ReviewMapTopic.Examples, example => example.GermanText == "Ich habe Deutsch gelernt.");
        Assert.Contains(a2ReviewMapTopic.Examples, example => example.GermanText == "Ich gebe dem Kind das Buch.");
        Assert.Contains(a2ReviewMapTopic.Examples, example => example.GermanText == "Können Sie mir sagen, wo der Bahnhof ist?");
        Assert.Contains(a2ReviewMapTopic.CommonMistakes, mistake => mistake.WrongText == "Ich habe nach Hause gegangen.");
        Assert.Contains(a2ReviewMapTopic.CommonMistakes, mistake => mistake.WrongText == "Ich bleibe zu Hause, weil ich bin krank.");
        Assert.Contains(a2ReviewMapTopic.CommonMistakes, mistake => mistake.WrongText == "Es gibt ein Park.");
        Assert.All(a2ReviewMapTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public async Task ParseAsync_ShouldParseOfficialB1GrammarCoreContract()
    {
        await using ServiceProvider serviceProvider = new ServiceCollection()
            .AddContentOpsInfrastructure()
            .BuildServiceProvider();

        IContentImportParser parser = serviceProvider.GetRequiredService<IContentImportParser>();
        string packagePath = Path.Combine(ResolveRepositoryRoot(), "content", "learning-portal", "grammar", "packages", "grammar-b1-core-v1.json");

        ParsedContentPackageModel parsedPackage = await parser.ParseAsync(
            await File.ReadAllTextAsync(packagePath),
            CancellationToken.None);

        string[] languages = ["en", "fa", "ar", "tr", "ru", "ckb", "kmr", "pl", "ro", "sq"];

        Assert.Equal(45, parsedPackage.GrammarTopics.Count);

        ParsedGrammarTopicModel topic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-relative-clauses-basics");
        Assert.Equal(1, topic.ContentRevision);
        Assert.Equal("B1", topic.CefrLevel);
        Assert.Equal("subordinate-clauses", topic.GrammarCategory);
        Assert.Equal(12, topic.Sections.Count);
        Assert.True(topic.Examples.Count >= 130);
        Assert.True(topic.CommonMistakes.Count >= 50);
        Assert.True(topic.RuleSummaries.Count >= 24);
        Assert.Equal(93, topic.LinkedWords.Count);
        Assert.Contains("a2-dass-clauses", topic.PrerequisiteSlugs);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", topic.PrerequisiteSlugs);
        Assert.Contains("a2-a2-case-review", topic.PrerequisiteSlugs);
        Assert.Contains("b1-relative-pronouns-in-nominative-and-accusative", topic.RelatedTopicSlugs);
        Assert.Contains("b2-advanced-relative-clauses", topic.RelatedTopicSlugs);

        ParsedGrammarSectionModel relativeBasicsCoreSection = Assert.Single(topic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel relativeBasicsFormSection = Assert.Single(topic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel relativeBasicsPronounSection = Assert.Single(topic.Sections, section => section.SectionKey == "relative-pronouns-look-like-articles");
        ParsedGrammarSectionModel relativeBasicsFocusSection = Assert.Single(topic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel relativeBasicsComparisonSection = Assert.Single(topic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel relativeBasicsPatternsSection = Assert.Single(topic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel relativeBasicsPracticeSection = Assert.Single(topic.Sections, section => section.SectionKey == "practice-advice");

        Assert.All(languages, language =>
        {
            Assert.True(topic.TitleLocalized.ContainsKey(language));
            Assert.True(topic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(topic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", relativeBasicsCoreSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", relativeBasicsFormSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", relativeBasicsPronounSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", relativeBasicsFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", relativeBasicsComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", relativeBasicsPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", relativeBasicsPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(topic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(topic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(topic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(topic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(topic.Examples, example => example.GermanText == "Das ist der Mann, der hier arbeitet.");
        Assert.Contains(topic.Examples, example => example.GermanText == "Das ist die Frau, die im Büro arbeitet.");
        Assert.Contains(topic.Examples, example => example.GermanText == "Ich lese das Buch, das du empfohlen hast.");
        Assert.Contains(topic.CommonMistakes, mistake => mistake.WrongText == "Das ist der Mann, der arbeitet hier.");
        Assert.Contains(topic.CommonMistakes, mistake => mistake.WrongText == "Das ist das Buch, das ich lese es.");
        Assert.All(topic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel nomAccTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-relative-pronouns-in-nominative-and-accusative");
        Assert.Equal(1, nomAccTopic.ContentRevision);
        Assert.Equal("B1", nomAccTopic.CefrLevel);
        Assert.Equal("subordinate-clauses", nomAccTopic.GrammarCategory);
        Assert.Equal(13, nomAccTopic.Sections.Count);
        Assert.True(nomAccTopic.Examples.Count >= 130);
        Assert.True(nomAccTopic.CommonMistakes.Count >= 50);
        Assert.True(nomAccTopic.RuleSummaries.Count >= 24);
        Assert.Equal(92, nomAccTopic.LinkedWords.Count);
        Assert.Contains("b1-relative-clauses-basics", nomAccTopic.PrerequisiteSlugs);
        Assert.Contains("a2-accusative-versus-dative-basics", nomAccTopic.PrerequisiteSlugs);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", nomAccTopic.PrerequisiteSlugs);
        Assert.Contains("b1-relative-pronouns-in-dative", nomAccTopic.RelatedTopicSlugs);
        Assert.Contains("b1-b1-case-review", nomAccTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel nomAccCoreSection = Assert.Single(nomAccTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel nomAccFormSection = Assert.Single(nomAccTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel nomAccRoleSection = Assert.Single(nomAccTopic.Sections, section => section.SectionKey == "relative-pronoun-role-table");
        ParsedGrammarSectionModel nomAccFocusSection = Assert.Single(nomAccTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel nomAccComparisonSection = Assert.Single(nomAccTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel sameNounSection = Assert.Single(nomAccTopic.Sections, section => section.SectionKey == "same-noun-different-case");
        ParsedGrammarSectionModel nomAccPatternsSection = Assert.Single(nomAccTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel relativeVerbFinalSection = Assert.Single(nomAccTopic.Sections, section => section.SectionKey == "verb-final-reminder");

        Assert.All(languages, language =>
        {
            Assert.True(nomAccTopic.TitleLocalized.ContainsKey(language));
            Assert.True(nomAccTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(nomAccTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", nomAccCoreSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", nomAccFormSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", nomAccRoleSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", nomAccFocusSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", nomAccComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", sameNounSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", nomAccPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", relativeVerbFinalSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(nomAccTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(nomAccTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(nomAccTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(nomAccTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(nomAccTopic.Examples, example => example.GermanText == "Das ist der Mann, der hier arbeitet.");
        Assert.Contains(nomAccTopic.Examples, example => example.GermanText == "Das ist der Mann, den ich kenne.");
        Assert.Contains(nomAccTopic.Examples, example => example.GermanText == "Der Mann, den ich kenne, ist nett.");
        Assert.Contains(nomAccTopic.CommonMistakes, mistake => mistake.WrongText == "Das ist der Mann, den hier arbeitet.");
        Assert.Contains(nomAccTopic.CommonMistakes, mistake => mistake.WrongText == "Das ist der Mann, der ich kenne.");
        Assert.Contains(nomAccTopic.CommonMistakes, mistake => mistake.WrongText == "Das ist das Buch, den ich lese.");
        Assert.All(nomAccTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel dativeTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-relative-pronouns-in-dative");
        Assert.Equal(1, dativeTopic.ContentRevision);
        Assert.Equal("B1", dativeTopic.CefrLevel);
        Assert.Equal("subordinate-clauses", dativeTopic.GrammarCategory);
      Assert.Equal(13, dativeTopic.Sections.Count);
      Assert.True(dativeTopic.Examples.Count >= 130);
      Assert.True(dativeTopic.CommonMistakes.Count >= 50);
      Assert.True(dativeTopic.RuleSummaries.Count >= 24);
      Assert.Equal(104, dativeTopic.LinkedWords.Count);
        Assert.Contains("b1-relative-clauses-basics", dativeTopic.PrerequisiteSlugs);
        Assert.Contains("b1-relative-pronouns-in-nominative-and-accusative", dativeTopic.PrerequisiteSlugs);
        Assert.Contains("a2-prepositions-with-dative", dativeTopic.PrerequisiteSlugs);
        Assert.Contains("b1-b1-case-review", dativeTopic.RelatedTopicSlugs);
        Assert.Contains("b1-verb-plus-preposition-combinations", dativeTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel dativePronounTableSection = Assert.Single(dativeTopic.Sections, section => section.SectionKey == "dative-relative-pronoun-table");
        ParsedGrammarSectionModel dativeDecisionSection = Assert.Single(dativeTopic.Sections, section => section.SectionKey == "how-to-decide-dative");
        ParsedGrammarSectionModel dativeComparisonSection = Assert.Single(dativeTopic.Sections, section => section.SectionKey == "dem-der-dem-denen-comparison");
        ParsedGrammarSectionModel accVsDativeSection = Assert.Single(dativeTopic.Sections, section => section.SectionKey == "common-confusion-with-accusative");

        Assert.All(languages, language =>
        {
            Assert.True(dativeTopic.TitleLocalized.ContainsKey(language));
            Assert.True(dativeTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(dativeTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", dativePronounTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
          Assert.Contains("\"table\"", dativeDecisionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
          Assert.Contains("\"table\"", dativeComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
          Assert.Contains("\"table\"", accVsDativeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
          Assert.All(dativeTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
          Assert.All(dativeTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
          Assert.All(dativeTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
      });

        Assert.Contains(dativeTopic.Examples, example => example.GermanText == "Das ist der Mann, dem ich helfe.");
        Assert.Contains(dativeTopic.Examples, example => example.GermanText == "Das ist die Frau, der ich danke.");
        Assert.Contains(dativeTopic.Examples, example => example.GermanText == "Das sind die Kinder, denen ich helfe.");
        Assert.Contains(dativeTopic.CommonMistakes, mistake => mistake.WrongText == "Der Mann, den ich helfe.");
        Assert.Contains(dativeTopic.CommonMistakes, mistake => mistake.WrongText == "Die Leute, mit die ich spreche.");
        Assert.Contains(dativeTopic.CommonMistakes, mistake => mistake.WrongText == "Die Kinder, den ich helfe.");
        Assert.All(dativeTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel politeTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-konjunktiv-ii-for-polite-requests");
        Assert.Equal(1, politeTopic.ContentRevision);
        Assert.Equal("B1", politeTopic.CefrLevel);
        Assert.Equal("konjunktiv", politeTopic.GrammarCategory);
        Assert.Equal(13, politeTopic.Sections.Count);
        Assert.True(politeTopic.Examples.Count >= 130);
        Assert.True(politeTopic.CommonMistakes.Count >= 50);
        Assert.True(politeTopic.RuleSummaries.Count >= 24);
        Assert.Equal(100, politeTopic.LinkedWords.Count);
        Assert.Contains("a2-polite-forms-with-wuerde", politeTopic.PrerequisiteSlugs);
        Assert.Contains("a2-indirect-questions-introduction", politeTopic.PrerequisiteSlugs);
        Assert.Contains("b1-konjunktiv-ii-with-waere-haette-wuerde", politeTopic.RelatedTopicSlugs);
        Assert.Contains("b1-formal-email-sentence-structure", politeTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel directVsPoliteSection = Assert.Single(politeTopic.Sections, section => section.SectionKey == "why-direct-requests-can-sound-strong");
        ParsedGrammarSectionModel politeRequestTableSection = Assert.Single(politeTopic.Sections, section => section.SectionKey == "polite-request-table");
        ParsedGrammarSectionModel politeWordOrderSection = Assert.Single(politeTopic.Sections, section => section.SectionKey == "word-order-with-konjunktiv-ii");
        ParsedGrammarSectionModel formalInformalSection = Assert.Single(politeTopic.Sections, section => section.SectionKey == "formal-vs-informal");
        ParsedGrammarSectionModel emailPhoneSection = Assert.Single(politeTopic.Sections, section => section.SectionKey == "email-and-phone-requests");

        Assert.All(languages, language =>
        {
            Assert.True(politeTopic.TitleLocalized.ContainsKey(language));
            Assert.True(politeTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(politeTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", directVsPoliteSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", politeRequestTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", politeWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", formalInformalSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", emailPhoneSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(politeTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(politeTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(politeTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(politeTopic.Examples, example => example.GermanText == "Könnten Sie mir bitte helfen?");
        Assert.Contains(politeTopic.Examples, example => example.GermanText == "Ich würde gern einen Termin machen.");
        Assert.Contains(politeTopic.Examples, example => example.GermanText == "Wäre es möglich, den Termin zu verschieben?");
        Assert.Contains(politeTopic.CommonMistakes, mistake => mistake.WrongText == "Könnten Sie helfen mir?");
        Assert.Contains(politeTopic.CommonMistakes, mistake => mistake.WrongText == "Ich würde gern einen Termin.");
        Assert.Contains(politeTopic.CommonMistakes, mistake => mistake.WrongText == "Dürfte ich fragen Sie?");
        Assert.All(politeTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel coreKonjunktivTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-konjunktiv-ii-with-waere-haette-wuerde");
        Assert.Equal(1, coreKonjunktivTopic.ContentRevision);
        Assert.Equal("B1", coreKonjunktivTopic.CefrLevel);
        Assert.Equal("konjunktiv", coreKonjunktivTopic.GrammarCategory);
        Assert.Equal(14, coreKonjunktivTopic.Sections.Count);
        Assert.True(coreKonjunktivTopic.Examples.Count >= 140);
        Assert.True(coreKonjunktivTopic.CommonMistakes.Count >= 50);
        Assert.True(coreKonjunktivTopic.RuleSummaries.Count >= 24);
        Assert.Equal(100, coreKonjunktivTopic.LinkedWords.Count);
        Assert.Contains("b1-konjunktiv-ii-for-polite-requests", coreKonjunktivTopic.PrerequisiteSlugs);
        Assert.Contains("a2-wenn-for-conditions", coreKonjunktivTopic.PrerequisiteSlugs);
        Assert.Contains("b1-talking-about-plans-and-conditions", coreKonjunktivTopic.RelatedTopicSlugs);
        Assert.Contains("b2-hypothetical-statements", coreKonjunktivTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel waereSection = Assert.Single(coreKonjunktivTopic.Sections, section => section.SectionKey == "waere");
        ParsedGrammarSectionModel haetteSection = Assert.Single(coreKonjunktivTopic.Sections, section => section.SectionKey == "haette");
        ParsedGrammarSectionModel wuerdeSection = Assert.Single(coreKonjunktivTopic.Sections, section => section.SectionKey == "wuerde");
        ParsedGrammarSectionModel formTableSection = Assert.Single(coreKonjunktivTopic.Sections, section => section.SectionKey == "form-table");
        ParsedGrammarSectionModel wennKonjunktivSection = Assert.Single(coreKonjunktivTopic.Sections, section => section.SectionKey == "wenn-plus-konjunktiv-ii");
        ParsedGrammarSectionModel politenessUseSection = Assert.Single(coreKonjunktivTopic.Sections, section => section.SectionKey == "politeness-use");
        ParsedGrammarSectionModel commonPatternsSection = Assert.Single(coreKonjunktivTopic.Sections, section => section.SectionKey == "common-patterns-table");

        Assert.All(languages, language =>
        {
            Assert.True(coreKonjunktivTopic.TitleLocalized.ContainsKey(language));
            Assert.True(coreKonjunktivTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(coreKonjunktivTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", waereSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", haetteSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", wuerdeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", formTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", wennKonjunktivSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", politenessUseSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", commonPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(coreKonjunktivTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(coreKonjunktivTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(coreKonjunktivTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(coreKonjunktivTopic.Examples, example => example.GermanText == "Wenn ich Zeit hätte, würde ich kommen.");
        Assert.Contains(coreKonjunktivTopic.Examples, example => example.GermanText == "Ich hätte gern einen Termin.");
        Assert.Contains(coreKonjunktivTopic.Examples, example => example.GermanText == "Würden Sie bitte helfen?");
        Assert.Contains(coreKonjunktivTopic.CommonMistakes, mistake => mistake.WrongText == "Wenn ich hätte Zeit, würde ich kommen.");
        Assert.Contains(coreKonjunktivTopic.CommonMistakes, mistake => mistake.WrongText == "Ich würde gern einen Termin.");
        Assert.Contains(coreKonjunktivTopic.CommonMistakes, mistake => mistake.WrongText == "Wäre es möglich, Sie helfen mir?");
        Assert.All(coreKonjunktivTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel passiveTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-passive-voice-introduction");
        Assert.Equal(1, passiveTopic.ContentRevision);
        Assert.Equal("B1", passiveTopic.CefrLevel);
        Assert.Equal("passive", passiveTopic.GrammarCategory);
        Assert.Equal(13, passiveTopic.Sections.Count);
        Assert.True(passiveTopic.Examples.Count >= 120);
        Assert.True(passiveTopic.CommonMistakes.Count >= 50);
        Assert.True(passiveTopic.RuleSummaries.Count >= 24);
        Assert.Equal(100, passiveTopic.LinkedWords.Count);
        Assert.Contains("a2-common-irregular-participles", passiveTopic.PrerequisiteSlugs);
        Assert.Contains("a2-man-as-general-subject", passiveTopic.PrerequisiteSlugs);
        Assert.Contains("b1-werden-as-auxiliary", passiveTopic.RelatedTopicSlugs);
        Assert.Contains("b2-passive-with-modal-verbs", passiveTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel activePassiveSection = Assert.Single(passiveTopic.Sections, section => section.SectionKey == "active-vs-passive-basic");
        ParsedGrammarSectionModel passiveStructureSection = Assert.Single(passiveTopic.Sections, section => section.SectionKey == "passive-structure");
        ParsedGrammarSectionModel werdenConjugationSection = Assert.Single(passiveTopic.Sections, section => section.SectionKey == "werden-conjugation-present");
        ParsedGrammarSectionModel actorVonSection = Assert.Single(passiveTopic.Sections, section => section.SectionKey == "actor-with-von");
        ParsedGrammarSectionModel officialLanguageSection = Assert.Single(passiveTopic.Sections, section => section.SectionKey == "passive-in-official-language");
        ParsedGrammarSectionModel passiveVsManSection = Assert.Single(passiveTopic.Sections, section => section.SectionKey == "passive-vs-man");

        Assert.All(languages, language =>
        {
            Assert.True(passiveTopic.TitleLocalized.ContainsKey(language));
            Assert.True(passiveTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(passiveTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", activePassiveSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", passiveStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", werdenConjugationSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", actorVonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", officialLanguageSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", passiveVsManSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(passiveTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(passiveTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(passiveTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(passiveTopic.Examples, example => example.GermanText == "Das Auto wird repariert.");
        Assert.Contains(passiveTopic.Examples, example => example.GermanText == "Der Antrag wird geprüft.");
        Assert.Contains(passiveTopic.Examples, example => example.GermanText == "Man prüft den Antrag.");
        Assert.Contains(passiveTopic.CommonMistakes, mistake => mistake.WrongText == "Das Auto wird reparieren.");
        Assert.Contains(passiveTopic.CommonMistakes, mistake => mistake.WrongText == "Der Antrag wird geprüft heute.");
        Assert.Contains(passiveTopic.CommonMistakes, mistake => mistake.WrongText == "Das Formular werden geprüft.");
        Assert.All(passiveTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel werdenTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-werden-as-auxiliary");
        Assert.Equal(1, werdenTopic.ContentRevision);
        Assert.Equal("B1", werdenTopic.CefrLevel);
        Assert.Equal("verbs", werdenTopic.GrammarCategory);
        Assert.Equal(13, werdenTopic.Sections.Count);
        Assert.True(werdenTopic.Examples.Count >= 130);
        Assert.True(werdenTopic.CommonMistakes.Count >= 50);
        Assert.True(werdenTopic.RuleSummaries.Count >= 24);
        Assert.Equal(100, werdenTopic.LinkedWords.Count);
        Assert.Contains("b1-passive-voice-introduction", werdenTopic.PrerequisiteSlugs);
        Assert.Contains("a2-common-irregular-participles", werdenTopic.PrerequisiteSlugs);
        Assert.Contains("b2-passive-with-modal-verbs", werdenTopic.RelatedTopicSlugs);
        Assert.Contains("b2-zustandspassiv-versus-vorgangspassiv", werdenTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel werdenConjugationAuxSection = Assert.Single(werdenTopic.Sections, section => section.SectionKey == "werden-conjugation");
        ParsedGrammarSectionModel passiveVsFutureSection = Assert.Single(werdenTopic.Sections, section => section.SectionKey == "passive-vs-future-pattern");
        ParsedGrammarSectionModel participleVsInfinitiveSection = Assert.Single(werdenTopic.Sections, section => section.SectionKey == "participle-vs-infinitive");
        ParsedGrammarSectionModel werdenWordOrderSection = Assert.Single(werdenTopic.Sections, section => section.SectionKey == "word-order-with-werden");
        ParsedGrammarSectionModel werdenConfusionSection = Assert.Single(werdenTopic.Sections, section => section.SectionKey == "common-confusions");

        Assert.All(languages, language =>
        {
            Assert.True(werdenTopic.TitleLocalized.ContainsKey(language));
            Assert.True(werdenTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(werdenTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", werdenConjugationAuxSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", passiveVsFutureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", participleVsInfinitiveSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", werdenWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", werdenConfusionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(werdenTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(werdenTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(werdenTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(werdenTopic.Examples, example => example.GermanText == "Ich werde müde.");
        Assert.Contains(werdenTopic.Examples, example => example.GermanText == "Das Formular wird geprüft.");
        Assert.Contains(werdenTopic.Examples, example => example.GermanText == "Ich werde morgen kommen.");
        Assert.Contains(werdenTopic.CommonMistakes, mistake => mistake.WrongText == "Ich werde müde gehen.");
        Assert.Contains(werdenTopic.CommonMistakes, mistake => mistake.WrongText == "Das Formular wird prüfen.");
        Assert.Contains(werdenTopic.CommonMistakes, mistake => mistake.WrongText == "Ich werde morgen gekommen.");
        Assert.All(werdenTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel infinitiveZuTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-infinitive-with-zu");
        Assert.Equal(1, infinitiveZuTopic.ContentRevision);
        Assert.Equal("B1", infinitiveZuTopic.CefrLevel);
        Assert.Equal("verbs", infinitiveZuTopic.GrammarCategory);
        Assert.Equal(14, infinitiveZuTopic.Sections.Count);
        Assert.True(infinitiveZuTopic.Examples.Count >= 130);
        Assert.True(infinitiveZuTopic.CommonMistakes.Count >= 50);
        Assert.True(infinitiveZuTopic.RuleSummaries.Count >= 24);
        Assert.Equal(98, infinitiveZuTopic.LinkedWords.Count);
        Assert.Contains("a2-zu-plus-infinitive-introduction", infinitiveZuTopic.PrerequisiteSlugs);
        Assert.Contains("a2-modal-verbs-in-more-detail", infinitiveZuTopic.PrerequisiteSlugs);
        Assert.Contains("b1-um-zu", infinitiveZuTopic.RelatedTopicSlugs);
        Assert.Contains("b1-damit-versus-um-zu", infinitiveZuTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel triggerVerbSection = Assert.Single(infinitiveZuTopic.Sections, section => section.SectionKey == "common-trigger-verbs");
        ParsedGrammarSectionModel triggerPhraseSection = Assert.Single(infinitiveZuTopic.Sections, section => section.SectionKey == "common-trigger-phrases");
        ParsedGrammarSectionModel separableZuSection = Assert.Single(infinitiveZuTopic.Sections, section => section.SectionKey == "separable-verbs-with-zu");
        ParsedGrammarSectionModel modalWithoutZuSection = Assert.Single(infinitiveZuTopic.Sections, section => section.SectionKey == "modal-verbs-without-zu");
        ParsedGrammarSectionModel adjectiveZuSection = Assert.Single(infinitiveZuTopic.Sections, section => section.SectionKey == "zu-infinitive-after-adjectives");
        ParsedGrammarSectionModel zuVsDassSection = Assert.Single(infinitiveZuTopic.Sections, section => section.SectionKey == "zu-infinitive-vs-dass");

        Assert.All(languages, language =>
        {
            Assert.True(infinitiveZuTopic.TitleLocalized.ContainsKey(language));
            Assert.True(infinitiveZuTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(infinitiveZuTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", triggerVerbSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", triggerPhraseSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", separableZuSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", modalWithoutZuSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", adjectiveZuSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", zuVsDassSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(infinitiveZuTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(infinitiveZuTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(infinitiveZuTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(infinitiveZuTopic.Examples, example => example.GermanText == "Ich versuche, Deutsch zu lernen.");
        Assert.Contains(infinitiveZuTopic.Examples, example => example.GermanText == "Es ist wichtig, pünktlich zu sein.");
        Assert.Contains(infinitiveZuTopic.Examples, example => example.GermanText == "Ich muss arbeiten.");
        Assert.Contains(infinitiveZuTopic.CommonMistakes, mistake => mistake.WrongText == "Ich versuche Deutsch lernen.");
        Assert.Contains(infinitiveZuTopic.CommonMistakes, mistake => mistake.WrongText == "Ich muss zu arbeiten.");
        Assert.Contains(infinitiveZuTopic.CommonMistakes, mistake => mistake.WrongText == "Ich versuche, zu anrufen.");
        Assert.All(infinitiveZuTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel umZuTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-um-zu");
        Assert.Equal(1, umZuTopic.ContentRevision);
        Assert.Equal("B1", umZuTopic.CefrLevel);
        Assert.Equal("subordinate-clauses", umZuTopic.GrammarCategory);
        Assert.Equal(13, umZuTopic.Sections.Count);
        Assert.True(umZuTopic.Examples.Count >= 130);
        Assert.True(umZuTopic.CommonMistakes.Count >= 50);
        Assert.True(umZuTopic.RuleSummaries.Count >= 24);
        Assert.Equal(95, umZuTopic.LinkedWords.Count);
        Assert.Contains("b1-infinitive-with-zu", umZuTopic.PrerequisiteSlugs);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", umZuTopic.PrerequisiteSlugs);
        Assert.Contains("b1-damit-versus-um-zu", umZuTopic.RelatedTopicSlugs);
        Assert.Contains("b1-talking-about-plans-and-conditions", umZuTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel sameSubjectSection = Assert.Single(umZuTopic.Sections, section => section.SectionKey == "same-subject-rule");
        ParsedGrammarSectionModel basicUmZuPatternSection = Assert.Single(umZuTopic.Sections, section => section.SectionKey == "basic-pattern");
        ParsedGrammarSectionModel separableUmZuSection = Assert.Single(umZuTopic.Sections, section => section.SectionKey == "separable-verbs-with-um-zu");
        ParsedGrammarSectionModel normalZuContrastSection = Assert.Single(umZuTopic.Sections, section => section.SectionKey == "um-zu-vs-normal-zu-infinitive");
        ParsedGrammarSectionModel damitWarningSection = Assert.Single(umZuTopic.Sections, section => section.SectionKey == "when-not-to-use-um-zu");
        ParsedGrammarSectionModel commonUmZuPatternSection = Assert.Single(umZuTopic.Sections, section => section.SectionKey == "common-pattern-table");

        Assert.All(languages, language =>
        {
            Assert.True(umZuTopic.TitleLocalized.ContainsKey(language));
            Assert.True(umZuTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(umZuTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", sameSubjectSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", basicUmZuPatternSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", separableUmZuSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", normalZuContrastSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", damitWarningSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", commonUmZuPatternSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(umZuTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(umZuTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(umZuTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(umZuTopic.Examples, example => example.GermanText == "Ich lerne Deutsch, um in Deutschland zu arbeiten.");
        Assert.Contains(umZuTopic.Examples, example => example.GermanText == "Ich spare Geld, um ein Auto zu kaufen.");
        Assert.Contains(umZuTopic.Examples, example => example.GermanText == "Ich lerne Deutsch, damit mein Sohn besser Deutsch spricht.");
        Assert.Contains(umZuTopic.CommonMistakes, mistake => mistake.WrongText == "Ich lerne Deutsch, um ich arbeite in Deutschland.");
        Assert.Contains(umZuTopic.CommonMistakes, mistake => mistake.WrongText == "Ich lerne Deutsch, um mein Sohn besser Deutsch spricht.");
        Assert.Contains(umZuTopic.CommonMistakes, mistake => mistake.WrongText == "Ich fahre in die Stadt, um zu einkaufen.");
        Assert.All(umZuTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel damitUmZuTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-damit-versus-um-zu");
        Assert.Equal(1, damitUmZuTopic.ContentRevision);
        Assert.Equal("B1", damitUmZuTopic.CefrLevel);
        Assert.Equal("subordinate-clauses", damitUmZuTopic.GrammarCategory);
        Assert.Equal(14, damitUmZuTopic.Sections.Count);
        Assert.True(damitUmZuTopic.Examples.Count >= 150);
        Assert.True(damitUmZuTopic.CommonMistakes.Count >= 55);
        Assert.True(damitUmZuTopic.RuleSummaries.Count >= 24);
        Assert.Equal(100, damitUmZuTopic.LinkedWords.Count);
        Assert.Contains("b1-um-zu", damitUmZuTopic.PrerequisiteSlugs);
        Assert.Contains("b1-infinitive-with-zu", damitUmZuTopic.PrerequisiteSlugs);
        Assert.Contains("b1-connectors-for-cause-and-effect", damitUmZuTopic.RelatedTopicSlugs);
        Assert.Contains("b2-infinitive-clauses-advanced", damitUmZuTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel sameDifferentSection = Assert.Single(damitUmZuTopic.Sections, section => section.SectionKey == "same-subject-vs-different-subject-table");
        ParsedGrammarSectionModel damitWordOrderSection = Assert.Single(damitUmZuTopic.Sections, section => section.SectionKey == "damit-word-order");
        ParsedGrammarSectionModel umZuWordOrderSection = Assert.Single(damitUmZuTopic.Sections, section => section.SectionKey == "um-zu-word-order");
        ParsedGrammarSectionModel pairedPurposeSection = Assert.Single(damitUmZuTopic.Sections, section => section.SectionKey == "paired-examples");
        ParsedGrammarSectionModel damitWeilSection = Assert.Single(damitUmZuTopic.Sections, section => section.SectionKey == "damit-vs-weil");
        ParsedGrammarSectionModel decisionChecklistSection = Assert.Single(damitUmZuTopic.Sections, section => section.SectionKey == "decision-checklist");

        Assert.All(languages, language =>
        {
            Assert.True(damitUmZuTopic.TitleLocalized.ContainsKey(language));
            Assert.True(damitUmZuTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(damitUmZuTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", sameDifferentSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", damitWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", umZuWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", pairedPurposeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", damitWeilSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", decisionChecklistSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(damitUmZuTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(damitUmZuTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(damitUmZuTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(damitUmZuTopic.Examples, example => example.GermanText == "Ich lerne Deutsch, um in Deutschland zu arbeiten.");
        Assert.Contains(damitUmZuTopic.Examples, example => example.GermanText == "Ich helfe meinem Sohn, damit er Deutsch lernt.");
        Assert.Contains(damitUmZuTopic.Examples, example => example.GermanText == "Ich bleibe zu Hause, damit ich gesund werde.");
        Assert.Contains(damitUmZuTopic.CommonMistakes, mistake => mistake.WrongText == "Ich lerne Deutsch, damit ich in Deutschland zu arbeiten.");
        Assert.Contains(damitUmZuTopic.CommonMistakes, mistake => mistake.WrongText == "Ich helfe meinem Sohn, um Deutsch zu lernen.");
        Assert.Contains(damitUmZuTopic.CommonMistakes, mistake => mistake.WrongText == "Damit er Deutsch lernt, ich helfe ihm.");
        Assert.All(damitUmZuTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel weilObwohlTrotzdemTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-weil-obwohl-trotzdem");
        Assert.Equal(1, weilObwohlTrotzdemTopic.ContentRevision);
        Assert.Equal("B1", weilObwohlTrotzdemTopic.CefrLevel);
        Assert.Equal("connectors", weilObwohlTrotzdemTopic.GrammarCategory);
        Assert.Equal(14, weilObwohlTrotzdemTopic.Sections.Count);
        Assert.True(weilObwohlTrotzdemTopic.Examples.Count >= 140);
        Assert.True(weilObwohlTrotzdemTopic.CommonMistakes.Count >= 50);
        Assert.True(weilObwohlTrotzdemTopic.RuleSummaries.Count >= 22);
        Assert.Equal(93, weilObwohlTrotzdemTopic.LinkedWords.Count);
        Assert.Contains("a2-weil-clauses", weilObwohlTrotzdemTopic.PrerequisiteSlugs);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", weilObwohlTrotzdemTopic.PrerequisiteSlugs);
        Assert.Contains("b1-connectors-for-contrast", weilObwohlTrotzdemTopic.RelatedTopicSlugs);
        Assert.Contains("b1-connectors-for-cause-and-effect", weilObwohlTrotzdemTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel meaningComparisonSection = Assert.Single(weilObwohlTrotzdemTopic.Sections, section => section.SectionKey == "meaning-comparison-table");
        ParsedGrammarSectionModel wordOrderComparisonSection = Assert.Single(weilObwohlTrotzdemTopic.Sections, section => section.SectionKey == "word-order-comparison-table");
        ParsedGrammarSectionModel weilVsObwohlSection = Assert.Single(weilObwohlTrotzdemTopic.Sections, section => section.SectionKey == "weil-vs-obwohl");
        ParsedGrammarSectionModel obwohlVsTrotzdemSection = Assert.Single(weilObwohlTrotzdemTopic.Sections, section => section.SectionKey == "obwohl-vs-trotzdem");
        ParsedGrammarSectionModel trotzdemOrderSection = Assert.Single(weilObwohlTrotzdemTopic.Sections, section => section.SectionKey == "trotzdem-main-clause-order");
        ParsedGrammarSectionModel modalConnectorSection = Assert.Single(weilObwohlTrotzdemTopic.Sections, section => section.SectionKey == "connectors-with-modal-verbs");
        ParsedGrammarSectionModel commonB1PatternSection = Assert.Single(weilObwohlTrotzdemTopic.Sections, section => section.SectionKey == "common-b1-patterns");

        Assert.All(languages, language =>
        {
            Assert.True(weilObwohlTrotzdemTopic.TitleLocalized.ContainsKey(language));
            Assert.True(weilObwohlTrotzdemTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(weilObwohlTrotzdemTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", meaningComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", wordOrderComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", weilVsObwohlSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", obwohlVsTrotzdemSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", trotzdemOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", modalConnectorSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", commonB1PatternSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(weilObwohlTrotzdemTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(weilObwohlTrotzdemTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(weilObwohlTrotzdemTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(weilObwohlTrotzdemTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(weilObwohlTrotzdemTopic.Examples, example => example.GermanText == "Ich bleibe zu Hause, weil ich krank bin.");
        Assert.Contains(weilObwohlTrotzdemTopic.Examples, example => example.GermanText == "Ich gehe zur Arbeit, obwohl ich krank bin.");
        Assert.Contains(weilObwohlTrotzdemTopic.Examples, example => example.GermanText == "Ich bin krank. Trotzdem gehe ich zur Arbeit.");
        Assert.Contains(weilObwohlTrotzdemTopic.CommonMistakes, mistake => mistake.WrongText == "Ich komme nicht, weil ich bin krank.");
        Assert.Contains(weilObwohlTrotzdemTopic.CommonMistakes, mistake => mistake.WrongText == "Trotzdem ich gehe zur Arbeit.");
        Assert.Contains(weilObwohlTrotzdemTopic.CommonMistakes, mistake => mistake.WrongText == "Obwohl ich krank bin, ich gehe zur Arbeit.");
        Assert.All(weilObwohlTrotzdemTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel alsWennTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-als-versus-wenn");
        Assert.Equal(1, alsWennTopic.ContentRevision);
        Assert.Equal("B1", alsWennTopic.CefrLevel);
        Assert.Equal("subordinate-clauses", alsWennTopic.GrammarCategory);
        Assert.Equal(13, alsWennTopic.Sections.Count);
        Assert.True(alsWennTopic.Examples.Count >= 140);
        Assert.True(alsWennTopic.CommonMistakes.Count >= 50);
        Assert.True(alsWennTopic.RuleSummaries.Count >= 22);
        Assert.Equal(86, alsWennTopic.LinkedWords.Count);
        Assert.Contains("a2-wenn-for-conditions", alsWennTopic.PrerequisiteSlugs);
        Assert.Contains("b1-describing-experiences-in-the-past", alsWennTopic.PrerequisiteSlugs);
        Assert.Contains("b1-nachdem-bevor-waehrend", alsWennTopic.RelatedTopicSlugs);
        Assert.Contains("b1-b1-verb-tense-review", alsWennTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel timelineSection = Assert.Single(alsWennTopic.Sections, section => section.SectionKey == "timeline-table");
        ParsedGrammarSectionModel pairedAlsWennSection = Assert.Single(alsWennTopic.Sections, section => section.SectionKey == "als-vs-wenn-paired-examples");
        ParsedGrammarSectionModel alsTenseSection = Assert.Single(alsWennTopic.Sections, section => section.SectionKey == "als-with-perfekt-and-praeteritum");
        ParsedGrammarSectionModel clauseFirstSection = Assert.Single(alsWennTopic.Sections, section => section.SectionKey == "wenn-clause-first");
        ParsedGrammarSectionModel alsWennDecisionSection = Assert.Single(alsWennTopic.Sections, section => section.SectionKey == "practice-advice");

        Assert.All(languages, language =>
        {
            Assert.True(alsWennTopic.TitleLocalized.ContainsKey(language));
            Assert.True(alsWennTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(alsWennTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", timelineSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", pairedAlsWennSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", alsTenseSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", clauseFirstSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", alsWennDecisionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(alsWennTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(alsWennTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(alsWennTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(alsWennTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(alsWennTopic.Examples, example => example.GermanText == "Als ich ein Kind war, wohnte ich in meinem Heimatland.");
        Assert.Contains(alsWennTopic.Examples, example => example.GermanText == "Wenn ich krank war, blieb ich zu Hause.");
        Assert.Contains(alsWennTopic.Examples, example => example.GermanText == "Wenn ich Zeit habe, komme ich zum Kurs.");
        Assert.Contains(alsWennTopic.CommonMistakes, mistake => mistake.WrongText == "Wenn ich nach Deutschland gekommen bin, war ich nervös.");
        Assert.Contains(alsWennTopic.CommonMistakes, mistake => mistake.WrongText == "Als ich Zeit habe, komme ich.");
        Assert.Contains(alsWennTopic.CommonMistakes, mistake => mistake.WrongText == "Wenn ich Zeit habe, ich komme.");
        Assert.All(alsWennTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel nachdemBevorWaehrendTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-nachdem-bevor-waehrend");
        Assert.Equal(1, nachdemBevorWaehrendTopic.ContentRevision);
        Assert.Equal("B1", nachdemBevorWaehrendTopic.CefrLevel);
        Assert.Equal("subordinate-clauses", nachdemBevorWaehrendTopic.GrammarCategory);
        Assert.Equal(14, nachdemBevorWaehrendTopic.Sections.Count);
        Assert.True(nachdemBevorWaehrendTopic.Examples.Count >= 150);
        Assert.True(nachdemBevorWaehrendTopic.CommonMistakes.Count >= 55);
        Assert.True(nachdemBevorWaehrendTopic.RuleSummaries.Count >= 24);
        Assert.Equal(92, nachdemBevorWaehrendTopic.LinkedWords.Count);
        Assert.Contains("a2-time-clauses-bevor-and-nachdem-introduction", nachdemBevorWaehrendTopic.PrerequisiteSlugs);
        Assert.Contains("b1-als-versus-wenn", nachdemBevorWaehrendTopic.PrerequisiteSlugs);
        Assert.Contains("b1-sentence-order-with-multiple-clauses", nachdemBevorWaehrendTopic.RelatedTopicSlugs);
        Assert.Contains("b1-describing-experiences-in-the-past", nachdemBevorWaehrendTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel temporalTimelineSection = Assert.Single(nachdemBevorWaehrendTopic.Sections, section => section.SectionKey == "timeline-table");
        ParsedGrammarSectionModel temporalVerbFinalSection = Assert.Single(nachdemBevorWaehrendTopic.Sections, section => section.SectionKey == "verb-final-rule");
        ParsedGrammarSectionModel nachdemPerfektSection = Assert.Single(nachdemBevorWaehrendTopic.Sections, section => section.SectionKey == "nachdem-and-perfekt");
        ParsedGrammarSectionModel temporalClausePositionSection = Assert.Single(nachdemBevorWaehrendTopic.Sections, section => section.SectionKey == "subordinate-clause-first-and-second");
        ParsedGrammarSectionModel nachdemDanachSection = Assert.Single(nachdemBevorWaehrendTopic.Sections, section => section.SectionKey == "nachdem-vs-danach");
        ParsedGrammarSectionModel temporalPracticeSection = Assert.Single(nachdemBevorWaehrendTopic.Sections, section => section.SectionKey == "practice-advice");

        Assert.All(languages, language =>
        {
            Assert.True(nachdemBevorWaehrendTopic.TitleLocalized.ContainsKey(language));
            Assert.True(nachdemBevorWaehrendTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(nachdemBevorWaehrendTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", temporalTimelineSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", temporalVerbFinalSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", nachdemPerfektSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", temporalClausePositionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", nachdemDanachSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", temporalPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(nachdemBevorWaehrendTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(nachdemBevorWaehrendTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(nachdemBevorWaehrendTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(nachdemBevorWaehrendTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(nachdemBevorWaehrendTopic.Examples, example => example.GermanText == "Nachdem ich gefrühstückt habe, gehe ich zur Arbeit.");
        Assert.Contains(nachdemBevorWaehrendTopic.Examples, example => example.GermanText == "Bevor ich zur Arbeit gehe, frühstücke ich.");
        Assert.Contains(nachdemBevorWaehrendTopic.Examples, example => example.GermanText == "Während ich arbeite, hört mein Sohn Musik.");
        Assert.Contains(nachdemBevorWaehrendTopic.CommonMistakes, mistake => mistake.WrongText == "Nachdem ich habe gegessen, lerne ich.");
        Assert.Contains(nachdemBevorWaehrendTopic.CommonMistakes, mistake => mistake.WrongText == "Bevor ich gehe zur Arbeit, frühstücke ich.");
        Assert.Contains(nachdemBevorWaehrendTopic.CommonMistakes, mistake => mistake.WrongText == "Während ich arbeite, mein Sohn hört Musik.");
        Assert.All(nachdemBevorWaehrendTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel indirectQuestionsTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-indirect-questions");
        Assert.Equal(1, indirectQuestionsTopic.ContentRevision);
        Assert.Equal("B1", indirectQuestionsTopic.CefrLevel);
        Assert.Equal("questions", indirectQuestionsTopic.GrammarCategory);
        Assert.Equal(14, indirectQuestionsTopic.Sections.Count);
        Assert.True(indirectQuestionsTopic.Examples.Count >= 150);
        Assert.True(indirectQuestionsTopic.CommonMistakes.Count >= 55);
        Assert.True(indirectQuestionsTopic.RuleSummaries.Count >= 24);
        Assert.Equal(98, indirectQuestionsTopic.LinkedWords.Count);
        Assert.Contains("a2-indirect-questions-introduction", indirectQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-konjunktiv-ii-for-polite-requests", indirectQuestionsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-reported-requests-and-polite-questions", indirectQuestionsTopic.RelatedTopicSlugs);
        Assert.Contains("b1-formal-email-sentence-structure", indirectQuestionsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel politeFrameSection = Assert.Single(indirectQuestionsTopic.Sections, section => section.SectionKey == "polite-frame-table");
        ParsedGrammarSectionModel directIndirectSection = Assert.Single(indirectQuestionsTopic.Sections, section => section.SectionKey == "direct-to-indirect-table");
        ParsedGrammarSectionModel modalIndirectSection = Assert.Single(indirectQuestionsTopic.Sections, section => section.SectionKey == "indirect-questions-with-modal-verbs");
        ParsedGrammarSectionModel perfektIndirectSection = Assert.Single(indirectQuestionsTopic.Sections, section => section.SectionKey == "indirect-questions-with-perfekt");
        ParsedGrammarSectionModel dassVsIndirectSection = Assert.Single(indirectQuestionsTopic.Sections, section => section.SectionKey == "indirect-question-vs-dass-clause");

        Assert.All(languages, language =>
        {
            Assert.True(indirectQuestionsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(indirectQuestionsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(indirectQuestionsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", politeFrameSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", directIndirectSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", modalIndirectSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", perfektIndirectSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", dassVsIndirectSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(indirectQuestionsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(indirectQuestionsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(indirectQuestionsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(indirectQuestionsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(indirectQuestionsTopic.Examples, example => example.GermanText == "Können Sie mir sagen, wo der Bahnhof ist?");
        Assert.Contains(indirectQuestionsTopic.Examples, example => example.GermanText == "Kommt der Bus? Wissen Sie, ob der Bus kommt?");
        Assert.Contains(indirectQuestionsTopic.Examples, example => example.GermanText == "Ich weiß nicht, ob ich einen Termin brauche.");
        Assert.Contains(indirectQuestionsTopic.CommonMistakes, mistake => mistake.WrongText == "Können Sie mir sagen, wo ist der Bahnhof?");
        Assert.Contains(indirectQuestionsTopic.CommonMistakes, mistake => mistake.WrongText == "Wissen Sie, ob kommt der Bus?");
        Assert.Contains(indirectQuestionsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich möchte wissen, haben Sie Zeit.");
        Assert.All(indirectQuestionsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel reportedRequestsTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-reported-requests-and-polite-questions");
        Assert.Equal(1, reportedRequestsTopic.ContentRevision);
        Assert.Equal("B1", reportedRequestsTopic.CefrLevel);
        Assert.Equal("questions", reportedRequestsTopic.GrammarCategory);
        Assert.Equal(14, reportedRequestsTopic.Sections.Count);
        Assert.True(reportedRequestsTopic.Examples.Count >= 150);
        Assert.True(reportedRequestsTopic.CommonMistakes.Count >= 55);
        Assert.True(reportedRequestsTopic.RuleSummaries.Count >= 24);
        Assert.Equal(95, reportedRequestsTopic.LinkedWords.Count);
        Assert.Contains("b1-indirect-questions", reportedRequestsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-infinitive-with-zu", reportedRequestsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-formal-email-sentence-structure", reportedRequestsTopic.RelatedTopicSlugs);
        Assert.Contains("c1-konjunktiv-i-for-reported-speech", reportedRequestsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel reportedObSection = Assert.Single(reportedRequestsTopic.Sections, section => section.SectionKey == "ob-for-reported-yes-no-questions");
        ParsedGrammarSectionModel reportedWSection = Assert.Single(reportedRequestsTopic.Sections, section => section.SectionKey == "w-question-reported");
        ParsedGrammarSectionModel bittenZuSection = Assert.Single(reportedRequestsTopic.Sections, section => section.SectionKey == "bitten-plus-zu-infinitive");
        ParsedGrammarSectionModel sagenSollenSection = Assert.Single(reportedRequestsTopic.Sections, section => section.SectionKey == "sagen-plus-sollen");
        ParsedGrammarSectionModel politeReportedSection = Assert.Single(reportedRequestsTopic.Sections, section => section.SectionKey == "polite-request-vs-reported-request-table");
        ParsedGrammarSectionModel reportedWordOrderSection = Assert.Single(reportedRequestsTopic.Sections, section => section.SectionKey == "word-order-rules");

        Assert.All(languages, language =>
        {
            Assert.True(reportedRequestsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(reportedRequestsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(reportedRequestsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", reportedObSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", reportedWSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", bittenZuSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", sagenSollenSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", politeReportedSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", reportedWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(reportedRequestsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(reportedRequestsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(reportedRequestsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
            Assert.All(reportedRequestsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
        });

        Assert.Contains(reportedRequestsTopic.Examples, example => example.GermanText == "Sie fragt, ob ich morgen kommen kann.");
        Assert.Contains(reportedRequestsTopic.Examples, example => example.GermanText == "Sie bittet mich, morgen zu kommen.");
        Assert.Contains(reportedRequestsTopic.Examples, example => example.GermanText == "Der Arzt sagt, dass ich zu Hause bleiben soll.");
        Assert.Contains(reportedRequestsTopic.CommonMistakes, mistake => mistake.WrongText == "Sie fragt, komme ich morgen.");
        Assert.Contains(reportedRequestsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich bitte Sie, füllen das Formular aus.");
        Assert.Contains(reportedRequestsTopic.CommonMistakes, mistake => mistake.WrongText == "Sie sagt, dass ich soll zu Hause bleiben.");
        Assert.All(reportedRequestsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel definiteAdjectiveTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-adjective-declension-after-definite-article");
        Assert.Equal(1, definiteAdjectiveTopic.ContentRevision);
        Assert.Equal("B1", definiteAdjectiveTopic.CefrLevel);
        Assert.Equal("adjective-declension", definiteAdjectiveTopic.GrammarCategory);
        Assert.Equal(14, definiteAdjectiveTopic.Sections.Count);
        Assert.True(definiteAdjectiveTopic.Examples.Count >= 150);
        Assert.True(definiteAdjectiveTopic.CommonMistakes.Count >= 55);
        Assert.True(definiteAdjectiveTopic.RuleSummaries.Count >= 25);
        Assert.Equal(103, definiteAdjectiveTopic.LinkedWords.Count);
        Assert.Contains("a2-adjective-endings-introduction", definiteAdjectiveTopic.PrerequisiteSlugs);
        Assert.Contains("a2-a2-case-review", definiteAdjectiveTopic.PrerequisiteSlugs);
        Assert.Contains("b1-adjective-declension-after-indefinite-article", definiteAdjectiveTopic.RelatedTopicSlugs);
        Assert.Contains("b1-b1-case-review", definiteAdjectiveTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel definiteNominativeSection = Assert.Single(definiteAdjectiveTopic.Sections, section => section.SectionKey == "nominative-table");
        ParsedGrammarSectionModel definiteAccusativeSection = Assert.Single(definiteAdjectiveTopic.Sections, section => section.SectionKey == "accusative-table");
        ParsedGrammarSectionModel definiteDativeSection = Assert.Single(definiteAdjectiveTopic.Sections, section => section.SectionKey == "dative-table");
        ParsedGrammarSectionModel seinBeforeNounSection = Assert.Single(definiteAdjectiveTopic.Sections, section => section.SectionKey == "after-sein-vs-before-noun");
        ParsedGrammarSectionModel formalWritingAdjectiveSection = Assert.Single(definiteAdjectiveTopic.Sections, section => section.SectionKey == "formal-writing-use");
        ParsedGrammarSectionModel patternMemorySection = Assert.Single(definiteAdjectiveTopic.Sections, section => section.SectionKey == "pattern-memory-table");

        Assert.All(languages, language =>
        {
            Assert.True(definiteAdjectiveTopic.TitleLocalized.ContainsKey(language));
            Assert.True(definiteAdjectiveTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(definiteAdjectiveTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(definiteAdjectiveTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", definiteNominativeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", definiteAccusativeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", definiteDativeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", seinBeforeNounSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", formalWritingAdjectiveSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", patternMemorySection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(definiteAdjectiveTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(definiteAdjectiveTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(definiteAdjectiveTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(definiteAdjectiveTopic.Examples, example => example.GermanText == "Der gute Kaffee ist teuer.");
        Assert.Contains(definiteAdjectiveTopic.Examples, example => example.GermanText == "Ich trinke den guten Kaffee.");
        Assert.Contains(definiteAdjectiveTopic.Examples, example => example.GermanText == "Mit den alten Büchern lerne ich.");
        Assert.Contains(definiteAdjectiveTopic.CommonMistakes, mistake => mistake.WrongText == "der gut Kaffee");
        Assert.Contains(definiteAdjectiveTopic.CommonMistakes, mistake => mistake.WrongText == "den gute Kaffee");
        Assert.Contains(definiteAdjectiveTopic.CommonMistakes, mistake => mistake.WrongText == "den alten Bücher");
        Assert.All(definiteAdjectiveTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel indefiniteAdjectiveTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-adjective-declension-after-indefinite-article");
        Assert.Equal(1, indefiniteAdjectiveTopic.ContentRevision);
        Assert.Equal("B1", indefiniteAdjectiveTopic.CefrLevel);
        Assert.Equal("adjective-declension", indefiniteAdjectiveTopic.GrammarCategory);
        Assert.Equal(15, indefiniteAdjectiveTopic.Sections.Count);
        Assert.True(indefiniteAdjectiveTopic.Examples.Count >= 160);
        Assert.True(indefiniteAdjectiveTopic.CommonMistakes.Count >= 60);
        Assert.True(indefiniteAdjectiveTopic.RuleSummaries.Count >= 28);
        Assert.Equal(99, indefiniteAdjectiveTopic.LinkedWords.Count);
        Assert.Contains("b1-adjective-declension-after-definite-article", indefiniteAdjectiveTopic.PrerequisiteSlugs);
        Assert.Contains("a2-possessive-pronouns-in-cases", indefiniteAdjectiveTopic.PrerequisiteSlugs);
        Assert.Contains("b1-adjective-declension-without-article", indefiniteAdjectiveTopic.RelatedTopicSlugs);
        Assert.Contains("b1-formal-email-sentence-structure", indefiniteAdjectiveTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel indefiniteNominativeSection = Assert.Single(indefiniteAdjectiveTopic.Sections, section => section.SectionKey == "nominative-table");
        ParsedGrammarSectionModel indefiniteAccusativeSection = Assert.Single(indefiniteAdjectiveTopic.Sections, section => section.SectionKey == "accusative-table");
        ParsedGrammarSectionModel indefiniteDativeSection = Assert.Single(indefiniteAdjectiveTopic.Sections, section => section.SectionKey == "dative-table");
        ParsedGrammarSectionModel einVsDerSection = Assert.Single(indefiniteAdjectiveTopic.Sections, section => section.SectionKey == "ein-vs-der-comparison");
        ParsedGrammarSectionModel possessiveEinWordSection = Assert.Single(indefiniteAdjectiveTopic.Sections, section => section.SectionKey == "possessives-as-ein-words");
        ParsedGrammarSectionModel keinEinWordSection = Assert.Single(indefiniteAdjectiveTopic.Sections, section => section.SectionKey == "kein-as-ein-word");
        ParsedGrammarSectionModel definiteComparisonSection = Assert.Single(indefiniteAdjectiveTopic.Sections, section => section.SectionKey == "comparison-with-definite-article-pattern");

        Assert.All(languages, language =>
        {
            Assert.True(indefiniteAdjectiveTopic.TitleLocalized.ContainsKey(language));
            Assert.True(indefiniteAdjectiveTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(indefiniteAdjectiveTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(indefiniteAdjectiveTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", indefiniteNominativeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", indefiniteAccusativeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", indefiniteDativeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", einVsDerSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", possessiveEinWordSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", keinEinWordSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", definiteComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(indefiniteAdjectiveTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(indefiniteAdjectiveTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(indefiniteAdjectiveTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(indefiniteAdjectiveTopic.Examples, example => example.GermanText == "Ein guter Kaffee hilft mir.");
        Assert.Contains(indefiniteAdjectiveTopic.Examples, example => example.GermanText == "Ich trinke einen guten Kaffee.");
        Assert.Contains(indefiniteAdjectiveTopic.Examples, example => example.GermanText == "Mein alter Vertrag endet bald.");
        Assert.Contains(indefiniteAdjectiveTopic.CommonMistakes, mistake => mistake.WrongText == "ein gute Kaffee");
        Assert.Contains(indefiniteAdjectiveTopic.CommonMistakes, mistake => mistake.WrongText == "ein klein Zimmer");
        Assert.Contains(indefiniteAdjectiveTopic.CommonMistakes, mistake => mistake.WrongText == "einen wichtige Termin");
        Assert.Contains(indefiniteAdjectiveTopic.CommonMistakes, mistake => mistake.WrongText == "mit keine alten Bücher");
        Assert.All(indefiniteAdjectiveTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel noArticleAdjectiveTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-adjective-declension-without-article");
        Assert.Equal(1, noArticleAdjectiveTopic.ContentRevision);
        Assert.Equal("B1", noArticleAdjectiveTopic.CefrLevel);
        Assert.Equal("adjective-declension", noArticleAdjectiveTopic.GrammarCategory);
        Assert.Equal(14, noArticleAdjectiveTopic.Sections.Count);
        Assert.True(noArticleAdjectiveTopic.Examples.Count >= 150);
        Assert.True(noArticleAdjectiveTopic.CommonMistakes.Count >= 55);
        Assert.True(noArticleAdjectiveTopic.RuleSummaries.Count >= 25);
        Assert.Equal(95, noArticleAdjectiveTopic.LinkedWords.Count);
        Assert.Contains("b1-adjective-declension-after-definite-article", noArticleAdjectiveTopic.PrerequisiteSlugs);
        Assert.Contains("b1-adjective-declension-after-indefinite-article", noArticleAdjectiveTopic.PrerequisiteSlugs);
        Assert.Contains("a2-prepositions-with-dative", noArticleAdjectiveTopic.PrerequisiteSlugs);
        Assert.Contains("b2-adjective-declension-advanced-review", noArticleAdjectiveTopic.RelatedTopicSlugs);
        Assert.Contains("b1-formal-email-sentence-structure", noArticleAdjectiveTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel noArticleNominativeSection = Assert.Single(noArticleAdjectiveTopic.Sections, section => section.SectionKey == "nominative-table");
        ParsedGrammarSectionModel noArticleAccusativeSection = Assert.Single(noArticleAdjectiveTopic.Sections, section => section.SectionKey == "accusative-table");
        ParsedGrammarSectionModel noArticleDativeSection = Assert.Single(noArticleAdjectiveTopic.Sections, section => section.SectionKey == "dative-table");
        ParsedGrammarSectionModel noArticleComparisonSection = Assert.Single(noArticleAdjectiveTopic.Sections, section => section.SectionKey == "comparison-with-definite-and-indefinite");
        ParsedGrammarSectionModel noArticlePrepositionSection = Assert.Single(noArticleAdjectiveTopic.Sections, section => section.SectionKey == "dative-with-prepositions");
        ParsedGrammarSectionModel noArticleFormalSection = Assert.Single(noArticleAdjectiveTopic.Sections, section => section.SectionKey == "formal-and-written-use");

        Assert.All(languages, language =>
        {
            Assert.True(noArticleAdjectiveTopic.TitleLocalized.ContainsKey(language));
            Assert.True(noArticleAdjectiveTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(noArticleAdjectiveTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(noArticleAdjectiveTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", noArticleNominativeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", noArticleAccusativeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", noArticleDativeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", noArticleComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", noArticlePrepositionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", noArticleFormalSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(noArticleAdjectiveTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(noArticleAdjectiveTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(noArticleAdjectiveTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(noArticleAdjectiveTopic.Examples, example => example.GermanText == "Guter Kaffee hilft mir am Morgen.");
        Assert.Contains(noArticleAdjectiveTopic.Examples, example => example.GermanText == "Ich trinke guten Kaffee.");
        Assert.Contains(noArticleAdjectiveTopic.Examples, example => example.GermanText == "Mit gutem Kaffee beginnt der Tag besser.");
        Assert.Contains(noArticleAdjectiveTopic.Examples, example => example.GermanText == "Bitte schicken Sie wichtige Unterlagen.");
        Assert.Contains(noArticleAdjectiveTopic.CommonMistakes, mistake => mistake.WrongText == "gut Kaffee");
        Assert.Contains(noArticleAdjectiveTopic.CommonMistakes, mistake => mistake.WrongText == "frische Brot");
        Assert.Contains(noArticleAdjectiveTopic.CommonMistakes, mistake => mistake.WrongText == "mit guter Kaffee");
        Assert.Contains(noArticleAdjectiveTopic.CommonMistakes, mistake => mistake.WrongText == "mit gut Kaffee");
        Assert.All(noArticleAdjectiveTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel genitiveTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-genitive-introduction");
        Assert.Equal(1, genitiveTopic.ContentRevision);
        Assert.Equal("B1", genitiveTopic.CefrLevel);
        Assert.Equal("genitive", genitiveTopic.GrammarCategory);
        Assert.Equal(14, genitiveTopic.Sections.Count);
        Assert.True(genitiveTopic.Examples.Count >= 130);
        Assert.True(genitiveTopic.CommonMistakes.Count >= 45);
        Assert.True(genitiveTopic.RuleSummaries.Count >= 22);
        Assert.Equal(93, genitiveTopic.LinkedWords.Count);
        Assert.Contains("a2-a2-case-review", genitiveTopic.PrerequisiteSlugs);
        Assert.Contains("a2-dative-case-basics", genitiveTopic.PrerequisiteSlugs);
        Assert.Contains("b1-b1-case-review", genitiveTopic.PrerequisiteSlugs);
        Assert.Contains("b2-genitive-in-formal-german", genitiveTopic.RelatedTopicSlugs);
        Assert.Contains("b2-dative-and-genitive-prepositions", genitiveTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel genitiveArticleSection = Assert.Single(genitiveTopic.Sections, section => section.SectionKey == "genitive-article-table");
        ParsedGrammarSectionModel genitiveNounEndingSection = Assert.Single(genitiveTopic.Sections, section => section.SectionKey == "noun-ending-s");
        ParsedGrammarSectionModel genitivePrepositionSection = Assert.Single(genitiveTopic.Sections, section => section.SectionKey == "genitive-prepositions-intro");
        ParsedGrammarSectionModel genitiveVsVonSection = Assert.Single(genitiveTopic.Sections, section => section.SectionKey == "genitive-vs-von");
        ParsedGrammarSectionModel genitiveContextSection = Assert.Single(genitiveTopic.Sections, section => section.SectionKey == "where-learners-see-genitive");

        Assert.All(languages, language =>
        {
            Assert.True(genitiveTopic.TitleLocalized.ContainsKey(language));
            Assert.True(genitiveTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(genitiveTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(genitiveTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", genitiveArticleSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", genitiveNounEndingSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", genitivePrepositionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", genitiveVsVonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", genitiveContextSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(genitiveTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(genitiveTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(genitiveTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(genitiveTopic.Examples, example => example.GermanText == "Das Auto des Mannes ist alt.");
        Assert.Contains(genitiveTopic.Examples, example => example.GermanText == "Wegen des Termins rufe ich an.");
        Assert.Contains(genitiveTopic.Examples, example => example.GermanText == "Aufgrund des Wetters fällt der Termin aus.");
        Assert.Contains(genitiveTopic.CommonMistakes, mistake => mistake.WrongText == "das Auto der Mann");
        Assert.Contains(genitiveTopic.CommonMistakes, mistake => mistake.WrongText == "des Frau");
        Assert.Contains(genitiveTopic.CommonMistakes, mistake => mistake.WrongText == "wegen der Termin");
        Assert.Contains(genitiveTopic.CommonMistakes, mistake => mistake.WrongText == "aufgrund dem Wetter");
        Assert.All(genitiveTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel prepositionalVerbsTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-prepositional-verbs-introduction");
        Assert.Equal(1, prepositionalVerbsTopic.ContentRevision);
        Assert.Equal("B1", prepositionalVerbsTopic.CefrLevel);
        Assert.Equal("prepositions", prepositionalVerbsTopic.GrammarCategory);
        Assert.Equal(15, prepositionalVerbsTopic.Sections.Count);
        Assert.True(prepositionalVerbsTopic.Examples.Count >= 150);
        Assert.True(prepositionalVerbsTopic.CommonMistakes.Count >= 55);
        Assert.True(prepositionalVerbsTopic.RuleSummaries.Count >= 24);
        Assert.Equal(105, prepositionalVerbsTopic.LinkedWords.Count);
        Assert.Contains("a2-prepositions-with-dative", prepositionalVerbsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-prepositions-with-accusative", prepositionalVerbsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-indirect-questions", prepositionalVerbsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-verb-plus-preposition-combinations", prepositionalVerbsTopic.RelatedTopicSlugs);
        Assert.Contains("b2-prepositional-phrases-in-formal-writing", prepositionalVerbsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel accusativeCombinationSection = Assert.Single(prepositionalVerbsTopic.Sections, section => section.SectionKey == "common-accusative-combinations");
        ParsedGrammarSectionModel dativeCombinationSection = Assert.Single(prepositionalVerbsTopic.Sections, section => section.SectionKey == "common-dative-combinations");
        ParsedGrammarSectionModel thingQuestionSection = Assert.Single(prepositionalVerbsTopic.Sections, section => section.SectionKey == "questions-with-worauf-worueber-woran");
        ParsedGrammarSectionModel personQuestionSection = Assert.Single(prepositionalVerbsTopic.Sections, section => section.SectionKey == "questions-with-auf-wen-mit-wem");
        ParsedGrammarSectionModel emailChunkSection = Assert.Single(prepositionalVerbsTopic.Sections, section => section.SectionKey == "prepositional-verbs-in-emails");
        ParsedGrammarSectionModel miniDictionarySection = Assert.Single(prepositionalVerbsTopic.Sections, section => section.SectionKey == "mini-dictionary-table");

        Assert.All(languages, language =>
        {
            Assert.True(prepositionalVerbsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(prepositionalVerbsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(prepositionalVerbsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(prepositionalVerbsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", accusativeCombinationSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", dativeCombinationSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", thingQuestionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", personQuestionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", emailChunkSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", miniDictionarySection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(prepositionalVerbsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(prepositionalVerbsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(prepositionalVerbsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(prepositionalVerbsTopic.Examples, example => example.GermanText == "Ich warte auf den Bus.");
        Assert.Contains(prepositionalVerbsTopic.Examples, example => example.GermanText == "Worüber sprecht ihr?");
        Assert.Contains(prepositionalVerbsTopic.Examples, example => example.GermanText == "Ich bedanke mich für Ihre Hilfe.");
        Assert.Contains(prepositionalVerbsTopic.Examples, example => example.GermanText == "Ich nehme an dem Kurs teil.");
        Assert.Contains(prepositionalVerbsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich warte für den Bus.");
        Assert.Contains(prepositionalVerbsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich spreche auf das Thema.");
        Assert.Contains(prepositionalVerbsTopic.CommonMistakes, mistake => mistake.WrongText == "Was wartest du auf?");
        Assert.Contains(prepositionalVerbsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich freue auf den Kurs.");
        Assert.All(prepositionalVerbsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel verbPrepositionTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-verb-plus-preposition-combinations");
        Assert.Equal(1, verbPrepositionTopic.ContentRevision);
        Assert.Equal("B1", verbPrepositionTopic.CefrLevel);
        Assert.Equal("prepositions", verbPrepositionTopic.GrammarCategory);
        Assert.Equal(15, verbPrepositionTopic.Sections.Count);
        Assert.True(verbPrepositionTopic.Examples.Count >= 160);
        Assert.True(verbPrepositionTopic.CommonMistakes.Count >= 60);
        Assert.True(verbPrepositionTopic.RuleSummaries.Count >= 24);
        Assert.Equal(116, verbPrepositionTopic.LinkedWords.Count);
        Assert.Contains("b1-prepositional-verbs-introduction", verbPrepositionTopic.PrerequisiteSlugs);
        Assert.Contains("a2-prepositions-with-dative", verbPrepositionTopic.PrerequisiteSlugs);
        Assert.Contains("b1-indirect-questions", verbPrepositionTopic.PrerequisiteSlugs);
        Assert.Contains("b1-noun-verb-phrases", verbPrepositionTopic.RelatedTopicSlugs);
        Assert.Contains("b1-formal-email-sentence-structure", verbPrepositionTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel verbPrepAccusativeSection = Assert.Single(verbPrepositionTopic.Sections, section => section.SectionKey == "accusative-combinations-table");
        ParsedGrammarSectionModel verbPrepDativeSection = Assert.Single(verbPrepositionTopic.Sections, section => section.SectionKey == "dative-combinations-table");
        ParsedGrammarSectionModel verbPrepPersonQuestionSection = Assert.Single(verbPrepositionTopic.Sections, section => section.SectionKey == "person-question-forms");
        ParsedGrammarSectionModel verbPrepThingQuestionSection = Assert.Single(verbPrepositionTopic.Sections, section => section.SectionKey == "thing-question-forms");
        ParsedGrammarSectionModel verbPrepCaseSection = Assert.Single(verbPrepositionTopic.Sections, section => section.SectionKey == "preposition-case-table");
        ParsedGrammarSectionModel verbPrepEmailSection = Assert.Single(verbPrepositionTopic.Sections, section => section.SectionKey == "prepositional-verbs-in-emails");
        ParsedGrammarSectionModel verbPrepPracticeSection = Assert.Single(verbPrepositionTopic.Sections, section => section.SectionKey == "practice-advice");

        Assert.All(languages, language =>
        {
            Assert.True(verbPrepositionTopic.TitleLocalized.ContainsKey(language));
            Assert.True(verbPrepositionTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(verbPrepositionTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(verbPrepositionTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", verbPrepAccusativeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", verbPrepDativeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", verbPrepPersonQuestionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", verbPrepThingQuestionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", verbPrepCaseSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", verbPrepEmailSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", verbPrepPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(verbPrepositionTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(verbPrepositionTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(verbPrepositionTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(verbPrepositionTopic.Examples, example => example.GermanText == "Ich warte auf den Bus.");
        Assert.Contains(verbPrepositionTopic.Examples, example => example.GermanText == "Ich freue mich auf den Urlaub nächste Woche.");
        Assert.Contains(verbPrepositionTopic.Examples, example => example.GermanText == "Ich bedanke mich für Ihre Hilfe.");
        Assert.Contains(verbPrepositionTopic.Examples, example => example.GermanText == "Mit wem sprichst du?");
        Assert.Contains(verbPrepositionTopic.CommonMistakes, mistake => mistake.WrongText == "Ich warte für den Bus.");
        Assert.Contains(verbPrepositionTopic.CommonMistakes, mistake => mistake.WrongText == "Ich freue mich über den Urlaub nächste Woche.");
        Assert.Contains(verbPrepositionTopic.CommonMistakes, mistake => mistake.WrongText == "Ich bedanke mich bei die Hilfe.");
        Assert.Contains(verbPrepositionTopic.CommonMistakes, mistake => mistake.WrongText == "Was wartest du auf?");
        Assert.All(verbPrepositionTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel nounVerbPhrasesTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-noun-verb-phrases");
        Assert.Equal(1, nounVerbPhrasesTopic.ContentRevision);
        Assert.Equal("B1", nounVerbPhrasesTopic.CefrLevel);
        Assert.Equal("verbs", nounVerbPhrasesTopic.GrammarCategory);
        Assert.Equal(15, nounVerbPhrasesTopic.Sections.Count);
        Assert.True(nounVerbPhrasesTopic.Examples.Count >= 170);
        Assert.True(nounVerbPhrasesTopic.CommonMistakes.Count >= 60);
        Assert.True(nounVerbPhrasesTopic.RuleSummaries.Count >= 24);
        Assert.Equal(120, nounVerbPhrasesTopic.LinkedWords.Count);
        Assert.Contains("a2-grammar-for-appointments", nounVerbPhrasesTopic.PrerequisiteSlugs);
        Assert.Contains("b1-prepositional-verbs-introduction", nounVerbPhrasesTopic.PrerequisiteSlugs);
        Assert.Contains("b1-verb-plus-preposition-combinations", nounVerbPhrasesTopic.PrerequisiteSlugs);
        Assert.Contains("b1-formal-email-sentence-structure", nounVerbPhrasesTopic.RelatedTopicSlugs);
        Assert.Contains("b1-complaint-sentence-patterns", nounVerbPhrasesTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel appointmentPhraseSection = Assert.Single(nounVerbPhrasesTopic.Sections, section => section.SectionKey == "appointment-phrases");
        ParsedGrammarSectionModel applicationOfficePhraseSection = Assert.Single(nounVerbPhrasesTopic.Sections, section => section.SectionKey == "application-and-office-phrases");
        ParsedGrammarSectionModel decisionPhraseSection = Assert.Single(nounVerbPhrasesTopic.Sections, section => section.SectionKey == "decision-and-plan-phrases");
        ParsedGrammarSectionModel helpPhraseSection = Assert.Single(nounVerbPhrasesTopic.Sections, section => section.SectionKey == "help-and-support-phrases");
        ParsedGrammarSectionModel complaintPhraseSection = Assert.Single(nounVerbPhrasesTopic.Sections, section => section.SectionKey == "complaint-and-problem-phrases");
        ParsedGrammarSectionModel accusativePhraseSection = Assert.Single(nounVerbPhrasesTopic.Sections, section => section.SectionKey == "accusative-in-noun-verb-phrases");
        ParsedGrammarSectionModel formalEverydaySection = Assert.Single(nounVerbPhrasesTopic.Sections, section => section.SectionKey == "formal-vs-everyday-style");
        ParsedGrammarSectionModel chunkLearningSection = Assert.Single(nounVerbPhrasesTopic.Sections, section => section.SectionKey == "chunk-learning-table");

        Assert.All(languages, language =>
        {
            Assert.True(nounVerbPhrasesTopic.TitleLocalized.ContainsKey(language));
            Assert.True(nounVerbPhrasesTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(nounVerbPhrasesTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(nounVerbPhrasesTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", appointmentPhraseSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", applicationOfficePhraseSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", decisionPhraseSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", helpPhraseSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", complaintPhraseSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", accusativePhraseSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", formalEverydaySection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", chunkLearningSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(nounVerbPhrasesTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(nounVerbPhrasesTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(nounVerbPhrasesTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(nounVerbPhrasesTopic.Examples, example => example.GermanText == "Ich möchte einen Termin vereinbaren.");
        Assert.Contains(nounVerbPhrasesTopic.Examples, example => example.GermanText == "Darf ich eine Frage stellen?");
        Assert.Contains(nounVerbPhrasesTopic.Examples, example => example.GermanText == "Ich muss einen Antrag stellen.");
        Assert.Contains(nounVerbPhrasesTopic.Examples, example => example.GermanText == "Wir müssen eine Entscheidung treffen.");
        Assert.Contains(nounVerbPhrasesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich mache einen Antrag.");
        Assert.Contains(nounVerbPhrasesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich stelle einen Termin.");
        Assert.Contains(nounVerbPhrasesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich mache eine Entscheidung.");
        Assert.Contains(nounVerbPhrasesTopic.CommonMistakes, mistake => mistake.WrongText == "Ich gebe eine Frage.");
        Assert.All(nounVerbPhrasesTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel opinionConnectorsTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-connectors-for-opinion");
        Assert.Equal(1, opinionConnectorsTopic.ContentRevision);
        Assert.Equal("B1", opinionConnectorsTopic.CefrLevel);
        Assert.Equal("connectors", opinionConnectorsTopic.GrammarCategory);
        Assert.Equal(14, opinionConnectorsTopic.Sections.Count);
        Assert.True(opinionConnectorsTopic.Examples.Count >= 160);
        Assert.True(opinionConnectorsTopic.CommonMistakes.Count >= 55);
        Assert.True(opinionConnectorsTopic.RuleSummaries.Count >= 24);
        Assert.Equal(107, opinionConnectorsTopic.LinkedWords.Count);
        Assert.Contains("a2-a2-connectors-overview", opinionConnectorsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-weil-obwohl-trotzdem", opinionConnectorsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-giving-reasons-clearly", opinionConnectorsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-connectors-for-contrast", opinionConnectorsTopic.RelatedTopicSlugs);
        Assert.Contains("b1-grammar-for-b1-speaking-exam", opinionConnectorsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel opinionFrameSection = Assert.Single(opinionConnectorsTopic.Sections, section => section.SectionKey == "basic-opinion-frames");
        ParsedGrammarSectionModel reasonConnectorSection = Assert.Single(opinionConnectorsTopic.Sections, section => section.SectionKey == "giving-reasons");
        ParsedGrammarSectionModel agreementSection = Assert.Single(opinionConnectorsTopic.Sections, section => section.SectionKey == "agreeing");
        ParsedGrammarSectionModel disagreementSection = Assert.Single(opinionConnectorsTopic.Sections, section => section.SectionKey == "disagreeing-politely");
        ParsedGrammarSectionModel partialAgreementSection = Assert.Single(opinionConnectorsTopic.Sections, section => section.SectionKey == "partial-agreement");
        ParsedGrammarSectionModel answerStructureSection = Assert.Single(opinionConnectorsTopic.Sections, section => section.SectionKey == "structuring-an-answer");
        ParsedGrammarSectionModel connectorWordOrderSection = Assert.Single(opinionConnectorsTopic.Sections, section => section.SectionKey == "connector-word-order");
        ParsedGrammarSectionModel opinionPatternsSection = Assert.Single(opinionConnectorsTopic.Sections, section => section.SectionKey == "useful-opinion-patterns-table");

        Assert.All(languages, language =>
        {
            Assert.True(opinionConnectorsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(opinionConnectorsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(opinionConnectorsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(opinionConnectorsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", opinionFrameSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", reasonConnectorSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", agreementSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", disagreementSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", partialAgreementSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", answerStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", connectorWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", opinionPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(opinionConnectorsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(opinionConnectorsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(opinionConnectorsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(opinionConnectorsTopic.Examples, example => example.GermanText == "Ich finde, dass Deutsch wichtig ist.");
        Assert.Contains(opinionConnectorsTopic.Examples, example => example.GermanText == "Meiner Meinung nach ist Sport gesund.");
        Assert.Contains(opinionConnectorsTopic.Examples, example => example.GermanText == "Ich stimme Ihnen zu.");
        Assert.Contains(opinionConnectorsTopic.Examples, example => example.GermanText == "Teilweise stimme ich zu, aber es ist teuer.");
        Assert.Contains(opinionConnectorsTopic.CommonMistakes, mistake => mistake.WrongText == "Meiner Meinung nach, es ist wichtig.");
        Assert.Contains(opinionConnectorsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich bin Meinung.");
        Assert.Contains(opinionConnectorsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich stimme zu dir.");
        Assert.Contains(opinionConnectorsTopic.CommonMistakes, mistake => mistake.WrongText == "Deshalb ich finde das gut.");
        Assert.All(opinionConnectorsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel contrastConnectorsTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-connectors-for-contrast");
        Assert.Equal(1, contrastConnectorsTopic.ContentRevision);
        Assert.Equal("B1", contrastConnectorsTopic.CefrLevel);
        Assert.Equal("connectors", contrastConnectorsTopic.GrammarCategory);
        Assert.Equal(14, contrastConnectorsTopic.Sections.Count);
        Assert.True(contrastConnectorsTopic.Examples.Count >= 160);
        Assert.True(contrastConnectorsTopic.CommonMistakes.Count >= 60);
        Assert.True(contrastConnectorsTopic.RuleSummaries.Count >= 24);
        Assert.Equal(103, contrastConnectorsTopic.LinkedWords.Count);
        Assert.Contains("b1-weil-obwohl-trotzdem", contrastConnectorsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-connectors-for-opinion", contrastConnectorsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-connectors-for-cause-and-effect", contrastConnectorsTopic.RelatedTopicSlugs);
        Assert.Contains("b2-zwar-aber", contrastConnectorsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel contrastMeaningSection = Assert.Single(contrastConnectorsTopic.Sections, section => section.SectionKey == "meaning-comparison-table");
        ParsedGrammarSectionModel contrastWordOrderSection = Assert.Single(contrastConnectorsTopic.Sections, section => section.SectionKey == "word-order-table");
        ParsedGrammarSectionModel contrastModalSection = Assert.Single(contrastConnectorsTopic.Sections, section => section.SectionKey == "contrast-with-modal-verbs");
        ParsedGrammarSectionModel contrastDecisionSection = Assert.Single(contrastConnectorsTopic.Sections, section => section.SectionKey == "choosing-the-right-connector");

        Assert.All(languages, language =>
        {
            Assert.True(contrastConnectorsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(contrastConnectorsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(contrastConnectorsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(contrastConnectorsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", contrastMeaningSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", contrastWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", contrastModalSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", contrastDecisionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(contrastConnectorsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(contrastConnectorsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(contrastConnectorsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(contrastConnectorsTopic.Examples, example => example.GermanText == "Der Kurs ist gut, aber teuer.");
        Assert.Contains(contrastConnectorsTopic.Examples, example => example.GermanText == "Ich gehe zur Arbeit, obwohl ich krank bin.");
        Assert.Contains(contrastConnectorsTopic.Examples, example => example.GermanText == "Ich bin krank. Trotzdem gehe ich zur Arbeit.");
        Assert.Contains(contrastConnectorsTopic.Examples, example => example.GermanText == "Ich trinke keinen Kaffee, sondern Tee.");
        Assert.Contains(contrastConnectorsTopic.CommonMistakes, mistake => mistake.WrongText == "Obwohl ich krank bin, ich gehe zur Arbeit.");
        Assert.Contains(contrastConnectorsTopic.CommonMistakes, mistake => mistake.WrongText == "Trotzdem ich gehe zur Arbeit.");
        Assert.Contains(contrastConnectorsTopic.CommonMistakes, mistake => mistake.WrongText == "Der Kurs ist gut, obwohl teuer.");
        Assert.Contains(contrastConnectorsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich trinke Kaffee, sondern Tee.");
        Assert.All(contrastConnectorsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel causeEffectConnectorsTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-connectors-for-cause-and-effect");
        Assert.Equal(1, causeEffectConnectorsTopic.ContentRevision);
        Assert.Equal("B1", causeEffectConnectorsTopic.CefrLevel);
        Assert.Equal("connectors", causeEffectConnectorsTopic.GrammarCategory);
        Assert.Equal(15, causeEffectConnectorsTopic.Sections.Count);
        Assert.True(causeEffectConnectorsTopic.Examples.Count >= 170);
        Assert.True(causeEffectConnectorsTopic.CommonMistakes.Count >= 65);
        Assert.True(causeEffectConnectorsTopic.RuleSummaries.Count >= 26);
        Assert.Equal(126, causeEffectConnectorsTopic.LinkedWords.Count);
        Assert.Contains("a2-weil-clauses", causeEffectConnectorsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-denn-versus-weil", causeEffectConnectorsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-connectors-for-opinion", causeEffectConnectorsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-weil-obwohl-trotzdem", causeEffectConnectorsTopic.PrerequisiteSlugs);
        Assert.Contains("b2-argumentation-grammar", causeEffectConnectorsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel wegenSection = Assert.Single(causeEffectConnectorsTopic.Sections, section => section.SectionKey == "wegen");
        ParsedGrammarSectionModel directionSection = Assert.Single(causeEffectConnectorsTopic.Sections, section => section.SectionKey == "cause-vs-effect-direction");
        ParsedGrammarSectionModel causeEffectWordOrderSection = Assert.Single(causeEffectConnectorsTopic.Sections, section => section.SectionKey == "word-order-comparison-table");
        ParsedGrammarSectionModel registerSection = Assert.Single(causeEffectConnectorsTopic.Sections, section => section.SectionKey == "formal-vs-spoken-register");
        ParsedGrammarSectionModel modalCauseEffectSection = Assert.Single(causeEffectConnectorsTopic.Sections, section => section.SectionKey == "cause-effect-with-modal-verbs");
        ParsedGrammarSectionModel causeEffectPracticeSection = Assert.Single(causeEffectConnectorsTopic.Sections, section => section.SectionKey == "practice-advice");

        Assert.All(languages, language =>
        {
            Assert.True(causeEffectConnectorsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(causeEffectConnectorsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(causeEffectConnectorsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(causeEffectConnectorsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", wegenSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", directionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", causeEffectWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", registerSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", modalCauseEffectSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", causeEffectPracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(causeEffectConnectorsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(causeEffectConnectorsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(causeEffectConnectorsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(causeEffectConnectorsTopic.Examples, example => example.GermanText == "Ich komme nicht, weil ich krank bin.");
        Assert.Contains(causeEffectConnectorsTopic.Examples, example => example.GermanText == "Ich bin krank. Deshalb komme ich nicht.");
        Assert.Contains(causeEffectConnectorsTopic.Examples, example => example.GermanText == "Aus diesem Grund möchte ich den Termin verschieben.");
        Assert.Contains(causeEffectConnectorsTopic.Examples, example => example.GermanText == "Wegen der Krankheit bleibe ich zu Hause.");
        Assert.Contains(causeEffectConnectorsTopic.CommonMistakes, mistake => mistake.WrongText == "Deshalb ich komme nicht.");
        Assert.Contains(causeEffectConnectorsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich komme nicht, weil ich bin krank.");
        Assert.Contains(causeEffectConnectorsTopic.CommonMistakes, mistake => mistake.WrongText == "Wegen ich bin krank.");
        Assert.Contains(causeEffectConnectorsTopic.CommonMistakes, mistake => mistake.WrongText == "Darum ich kann nicht kommen.");
        Assert.All(causeEffectConnectorsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel multipleClauseOrderTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-sentence-order-with-multiple-clauses");
        Assert.Equal(1, multipleClauseOrderTopic.ContentRevision);
        Assert.Equal("B1", multipleClauseOrderTopic.CefrLevel);
        Assert.Equal("word-order", multipleClauseOrderTopic.GrammarCategory);
        Assert.Equal(15, multipleClauseOrderTopic.Sections.Count);
        Assert.True(multipleClauseOrderTopic.Examples.Count >= 170);
        Assert.True(multipleClauseOrderTopic.CommonMistakes.Count >= 65);
        Assert.True(multipleClauseOrderTopic.RuleSummaries.Count >= 28);
        Assert.Equal(129, multipleClauseOrderTopic.LinkedWords.Count);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", multipleClauseOrderTopic.PrerequisiteSlugs);
        Assert.Contains("b1-relative-clauses-basics", multipleClauseOrderTopic.PrerequisiteSlugs);
        Assert.Contains("b1-connectors-for-cause-and-effect", multipleClauseOrderTopic.PrerequisiteSlugs);
        Assert.Contains("b1-connectors-for-contrast", multipleClauseOrderTopic.PrerequisiteSlugs);
        Assert.Contains("b2-complex-sentence-order", multipleClauseOrderTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel multipleClauseMainReviewSection = Assert.Single(multipleClauseOrderTopic.Sections, section => section.SectionKey == "main-clause-review");
        ParsedGrammarSectionModel multipleClauseSubordinateFirstSection = Assert.Single(multipleClauseOrderTopic.Sections, section => section.SectionKey == "subordinate-clause-first");
        ParsedGrammarSectionModel multipleClauseRelativeSection = Assert.Single(multipleClauseOrderTopic.Sections, section => section.SectionKey == "relative-clause-in-sentence");
        ParsedGrammarSectionModel multipleClauseConnectorTypeSection = Assert.Single(multipleClauseOrderTopic.Sections, section => section.SectionKey == "connector-type-table");
        ParsedGrammarSectionModel multipleClausePatternsSection = Assert.Single(multipleClauseOrderTopic.Sections, section => section.SectionKey == "common-sentence-patterns");
        ParsedGrammarSectionModel multipleClausePracticeSection = Assert.Single(multipleClauseOrderTopic.Sections, section => section.SectionKey == "practice-advice");

        Assert.All(languages, language =>
        {
            Assert.True(multipleClauseOrderTopic.TitleLocalized.ContainsKey(language));
            Assert.True(multipleClauseOrderTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(multipleClauseOrderTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(multipleClauseOrderTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", multipleClauseMainReviewSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", multipleClauseSubordinateFirstSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", multipleClauseRelativeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", multipleClauseConnectorTypeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", multipleClausePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", multipleClausePracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(multipleClauseOrderTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(multipleClauseOrderTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(multipleClauseOrderTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(multipleClauseOrderTopic.Examples, example => example.GermanText == "Weil ich krank bin, bleibe ich zu Hause.");
        Assert.Contains(multipleClauseOrderTopic.Examples, example => example.GermanText == "Der Mann, der hier arbeitet, ist mein Nachbar.");
        Assert.Contains(multipleClauseOrderTopic.Examples, example => example.GermanText == "Ich kenne eine Frau, die Deutsch lernt, weil sie in Deutschland arbeitet.");
        Assert.Contains(multipleClauseOrderTopic.Examples, example => example.GermanText == "Ich weiß, dass er gekommen ist.");
        Assert.Contains(multipleClauseOrderTopic.CommonMistakes, mistake => mistake.WrongText == "Weil ich krank bin, ich bleibe zu Hause.");
        Assert.Contains(multipleClauseOrderTopic.CommonMistakes, mistake => mistake.WrongText == "Ich glaube, dass er kommt morgen.");
        Assert.Contains(multipleClauseOrderTopic.CommonMistakes, mistake => mistake.WrongText == "Ich kenne eine Frau, die lernt Deutsch.");
        Assert.Contains(multipleClauseOrderTopic.CommonMistakes, mistake => mistake.WrongText == "Trotzdem ich gehe zur Arbeit.");
        Assert.All(multipleClauseOrderTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel formalEmailTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-formal-email-sentence-structure");
        Assert.Equal(1, formalEmailTopic.ContentRevision);
        Assert.Equal("B1", formalEmailTopic.CefrLevel);
        Assert.Equal("word-order", formalEmailTopic.GrammarCategory);
        Assert.Equal(16, formalEmailTopic.Sections.Count);
        Assert.True(formalEmailTopic.Examples.Count >= 180);
        Assert.True(formalEmailTopic.CommonMistakes.Count >= 70);
        Assert.True(formalEmailTopic.RuleSummaries.Count >= 28);
        Assert.Equal(145, formalEmailTopic.LinkedWords.Count);
        Assert.Contains("a2-simple-email-grammar", formalEmailTopic.PrerequisiteSlugs);
        Assert.Contains("b1-konjunktiv-ii-for-polite-requests", formalEmailTopic.PrerequisiteSlugs);
        Assert.Contains("b1-sentence-order-with-multiple-clauses", formalEmailTopic.PrerequisiteSlugs);
        Assert.Contains("b1-connectors-for-cause-and-effect", formalEmailTopic.PrerequisiteSlugs);
        Assert.Contains("b1-complaint-sentence-patterns", formalEmailTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel emailStructureSection = Assert.Single(formalEmailTopic.Sections, section => section.SectionKey == "email-structure-map");
        ParsedGrammarSectionModel formalOpeningSection = Assert.Single(formalEmailTopic.Sections, section => section.SectionKey == "opening-sentence");
        ParsedGrammarSectionModel politeRequestEmailSection = Assert.Single(formalEmailTopic.Sections, section => section.SectionKey == "making-a-request");
        ParsedGrammarSectionModel attachmentSection = Assert.Single(formalEmailTopic.Sections, section => section.SectionKey == "attachments-and-documents");
        ParsedGrammarSectionModel formalPronounSection = Assert.Single(formalEmailTopic.Sections, section => section.SectionKey == "sie-ihnen-ihr-ihre");
        ParsedGrammarSectionModel formalEmailWordOrderSection = Assert.Single(formalEmailTopic.Sections, section => section.SectionKey == "word-order-in-formal-email");
        ParsedGrammarSectionModel sampleEmailAnalysisSection = Assert.Single(formalEmailTopic.Sections, section => section.SectionKey == "sample-email-analysis");

        Assert.All(languages, language =>
        {
            Assert.True(formalEmailTopic.TitleLocalized.ContainsKey(language));
            Assert.True(formalEmailTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(formalEmailTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(formalEmailTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", emailStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", formalOpeningSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", politeRequestEmailSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", attachmentSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", formalPronounSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", formalEmailWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", sampleEmailAnalysisSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(formalEmailTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(formalEmailTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(formalEmailTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(formalEmailTopic.Examples, example => example.GermanText == "Sehr geehrte Damen und Herren,");
        Assert.Contains(formalEmailTopic.Examples, example => example.GermanText == "Ich schreibe Ihnen, weil ich eine Frage habe.");
        Assert.Contains(formalEmailTopic.Examples, example => example.GermanText == "Könnten Sie mir bitte die Unterlagen schicken?");
        Assert.Contains(formalEmailTopic.Examples, example => example.GermanText == "Im Anhang finden Sie das Formular.");
        Assert.Contains(formalEmailTopic.Examples, example => example.GermanText == "Mit freundlichen Grüßen");
        Assert.Contains(formalEmailTopic.CommonMistakes, mistake => mistake.WrongText == "Ich schreibe Sie wegen des Termins.");
        Assert.Contains(formalEmailTopic.CommonMistakes, mistake => mistake.WrongText == "Können Sie sagen mir, wann der Termin ist?");
        Assert.Contains(formalEmailTopic.CommonMistakes, mistake => mistake.WrongText == "Ich bitte Ihnen, mir zu antworten.");
        Assert.Contains(formalEmailTopic.CommonMistakes, mistake => mistake.WrongText == "Ich freue mich für Ihre Antwort.");
        Assert.All(formalEmailTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel complaintPatternsTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-complaint-sentence-patterns");
        Assert.Equal(1, complaintPatternsTopic.ContentRevision);
        Assert.Equal("B1", complaintPatternsTopic.CefrLevel);
        Assert.Equal("word-order", complaintPatternsTopic.GrammarCategory);
        Assert.Equal(15, complaintPatternsTopic.Sections.Count);
        Assert.True(complaintPatternsTopic.Examples.Count >= 180);
        Assert.True(complaintPatternsTopic.CommonMistakes.Count >= 70);
        Assert.True(complaintPatternsTopic.RuleSummaries.Count >= 28);
        Assert.Equal(136, complaintPatternsTopic.LinkedWords.Count);
        Assert.Contains("b1-formal-email-sentence-structure", complaintPatternsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-connectors-for-cause-and-effect", complaintPatternsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-sentence-order-with-multiple-clauses", complaintPatternsTopic.PrerequisiteSlugs);
        Assert.Contains("b2-formal-complaint-grammar", complaintPatternsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel complaintOverviewSection = Assert.Single(complaintPatternsTopic.Sections, section => section.SectionKey == "complaint-structure-overview");
        ParsedGrammarSectionModel problemDescriptionSection = Assert.Single(complaintPatternsTopic.Sections, section => section.SectionKey == "describing-the-problem");
        ParsedGrammarSectionModel consequenceSection = Assert.Single(complaintPatternsTopic.Sections, section => section.SectionKey == "explaining-consequences");
        ParsedGrammarSectionModel complaintRequestSection = Assert.Single(complaintPatternsTopic.Sections, section => section.SectionKey == "polite-request-for-solution");
        ParsedGrammarSectionModel appointmentComplaintSection = Assert.Single(complaintPatternsTopic.Sections, section => section.SectionKey == "complaint-about-appointment");
        ParsedGrammarSectionModel complaintEmailPatternSection = Assert.Single(complaintPatternsTopic.Sections, section => section.SectionKey == "complaint-email-pattern-table");

        Assert.All(languages, language =>
        {
            Assert.True(complaintPatternsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(complaintPatternsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(complaintPatternsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(complaintPatternsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"callout\"", complaintPatternsTopic.Sections[0].LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", complaintOverviewSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", problemDescriptionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", consequenceSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", complaintRequestSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", appointmentComplaintSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", complaintEmailPatternSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(complaintPatternsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(complaintPatternsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(complaintPatternsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(complaintPatternsTopic.Examples, example => example.GermanText == "Leider gibt es ein Problem mit der Heizung.");
        Assert.Contains(complaintPatternsTopic.Examples, example => example.GermanText == "Ich habe festgestellt, dass die Heizung nicht funktioniert.");
        Assert.Contains(complaintPatternsTopic.Examples, example => example.GermanText == "Könnten Sie bitte die Heizung reparieren?");
        Assert.Contains(complaintPatternsTopic.Examples, example => example.GermanText == "Bitte geben Sie mir eine Rückmeldung.");
        Assert.Contains(complaintPatternsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich will, dass Sie sofort antworten.");
        Assert.Contains(complaintPatternsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich habe Problem.");
        Assert.Contains(complaintPatternsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich habe festgestellt, dass die Heizung funktioniert nicht.");
        Assert.Contains(complaintPatternsTopic.CommonMistakes, mistake => mistake.WrongText == "Können Sie bitte reparieren die Heizung?");
        Assert.All(complaintPatternsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel givingReasonsTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-giving-reasons-clearly");
        Assert.Equal(1, givingReasonsTopic.ContentRevision);
        Assert.Equal("B1", givingReasonsTopic.CefrLevel);
        Assert.Equal("connectors", givingReasonsTopic.GrammarCategory);
        Assert.Equal(14, givingReasonsTopic.Sections.Count);
        Assert.True(givingReasonsTopic.Examples.Count >= 170);
        Assert.True(givingReasonsTopic.CommonMistakes.Count >= 65);
        Assert.True(givingReasonsTopic.RuleSummaries.Count >= 26);
        Assert.Equal(117, givingReasonsTopic.LinkedWords.Count);
        Assert.Contains("b1-connectors-for-cause-and-effect", givingReasonsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-connectors-for-opinion", givingReasonsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-sentence-order-with-multiple-clauses", givingReasonsTopic.PrerequisiteSlugs);
        Assert.Contains("b2-argumentation-grammar", givingReasonsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel formalReasonSection = Assert.Single(givingReasonsTopic.Sections, section => section.SectionKey == "formal-reason-phrases");
        ParsedGrammarSectionModel reasonChainSection = Assert.Single(givingReasonsTopic.Sections, section => section.SectionKey == "reason-chain");
        ParsedGrammarSectionModel givingReasonsWordOrderSection = Assert.Single(givingReasonsTopic.Sections, section => section.SectionKey == "word-order-table");
        ParsedGrammarSectionModel givingReasonsPatternSection = Assert.Single(givingReasonsTopic.Sections, section => section.SectionKey == "common-pattern-table");

        Assert.All(languages, language =>
        {
            Assert.True(givingReasonsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(givingReasonsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(givingReasonsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(givingReasonsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", formalReasonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", reasonChainSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", givingReasonsWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", givingReasonsPatternSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(givingReasonsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(givingReasonsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(givingReasonsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(givingReasonsTopic.Examples, example => example.GermanText == "Ich lerne Deutsch, weil ich in Deutschland arbeite.");
        Assert.Contains(givingReasonsTopic.Examples, example => example.GermanText == "Ich lerne Deutsch, denn ich arbeite in Deutschland.");
        Assert.Contains(givingReasonsTopic.Examples, example => example.GermanText == "Ich arbeite in Deutschland. Deshalb lerne ich Deutsch.");
        Assert.Contains(givingReasonsTopic.Examples, example => example.GermanText == "Der Grund ist, dass ich morgen arbeiten muss.");
        Assert.Contains(givingReasonsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich denke das, weil es ist wichtig.");
        Assert.Contains(givingReasonsTopic.CommonMistakes, mistake => mistake.WrongText == "Deshalb ich lerne Deutsch.");
        Assert.Contains(givingReasonsTopic.CommonMistakes, mistake => mistake.WrongText == "Wegen ich krank bin.");
        Assert.Contains(givingReasonsTopic.CommonMistakes, mistake => mistake.WrongText == "Aus diesem Grund ich kann nicht kommen.");
        Assert.All(givingReasonsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel agreeDisagreeTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-agreeing-and-disagreeing-grammatically");
        Assert.Equal(1, agreeDisagreeTopic.ContentRevision);
        Assert.Equal("B1", agreeDisagreeTopic.CefrLevel);
        Assert.Equal("connectors", agreeDisagreeTopic.GrammarCategory);
        Assert.Equal(15, agreeDisagreeTopic.Sections.Count);
        Assert.True(agreeDisagreeTopic.Examples.Count >= 170);
        Assert.True(agreeDisagreeTopic.CommonMistakes.Count >= 65);
        Assert.True(agreeDisagreeTopic.RuleSummaries.Count >= 26);
        Assert.Equal(133, agreeDisagreeTopic.LinkedWords.Count);
        Assert.Contains("b1-connectors-for-opinion", agreeDisagreeTopic.PrerequisiteSlugs);
        Assert.Contains("b1-connectors-for-contrast", agreeDisagreeTopic.PrerequisiteSlugs);
        Assert.Contains("b1-giving-reasons-clearly", agreeDisagreeTopic.PrerequisiteSlugs);
        Assert.Contains("b1-grammar-for-b1-speaking-exam", agreeDisagreeTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel simpleAgreementSection = Assert.Single(agreeDisagreeTopic.Sections, section => section.SectionKey == "simple-agreement");
        ParsedGrammarSectionModel simpleDisagreementSection = Assert.Single(agreeDisagreeTopic.Sections, section => section.SectionKey == "simple-disagreement");
        ParsedGrammarSectionModel partialAgreementTopicSection = Assert.Single(agreeDisagreeTopic.Sections, section => section.SectionKey == "partial-agreement");
        ParsedGrammarSectionModel followUpQuestionSection = Assert.Single(agreeDisagreeTopic.Sections, section => section.SectionKey == "asking-follow-up-questions");
        ParsedGrammarSectionModel agreementDisagreementTableSection = Assert.Single(agreeDisagreeTopic.Sections, section => section.SectionKey == "agreement-disagreement-table");
        ParsedGrammarSectionModel discussionDuSieSection = Assert.Single(agreeDisagreeTopic.Sections, section => section.SectionKey == "du-vs-sie-in-discussion");
        ParsedGrammarSectionModel speakingPatternSection = Assert.Single(agreeDisagreeTopic.Sections, section => section.SectionKey == "b1-speaking-exam-pattern");

        Assert.All(languages, language =>
        {
            Assert.True(agreeDisagreeTopic.TitleLocalized.ContainsKey(language));
            Assert.True(agreeDisagreeTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(agreeDisagreeTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(agreeDisagreeTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", simpleAgreementSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", simpleDisagreementSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", partialAgreementTopicSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", followUpQuestionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", agreementDisagreementTableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", discussionDuSieSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", speakingPatternSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(agreeDisagreeTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(agreeDisagreeTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(agreeDisagreeTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(agreeDisagreeTopic.Examples, example => example.GermanText == "Ich stimme dir zu.");
        Assert.Contains(agreeDisagreeTopic.Examples, example => example.GermanText == "Ich sehe das anders.");
        Assert.Contains(agreeDisagreeTopic.Examples, example => example.GermanText == "Teilweise stimme ich zu, aber es gibt auch Nachteile.");
        Assert.Contains(agreeDisagreeTopic.Examples, example => example.GermanText == "Obwohl ich dich verstehe, sehe ich das anders.");
        Assert.Contains(agreeDisagreeTopic.CommonMistakes, mistake => mistake.WrongText == "Ich stimme mit dir zu.");
        Assert.Contains(agreeDisagreeTopic.CommonMistakes, mistake => mistake.WrongText == "Ich bin nicht agree.");
        Assert.Contains(agreeDisagreeTopic.CommonMistakes, mistake => mistake.WrongText == "Ich sehe anders.");
        Assert.Contains(agreeDisagreeTopic.CommonMistakes, mistake => mistake.WrongText == "Damit ich bin nicht einverstanden.");
        Assert.All(agreeDisagreeTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel pastExperienceTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-describing-experiences-in-the-past");
        Assert.Equal(1, pastExperienceTopic.ContentRevision);
        Assert.Equal("B1", pastExperienceTopic.CefrLevel);
        Assert.Equal("tenses", pastExperienceTopic.GrammarCategory);
        Assert.Equal(15, pastExperienceTopic.Sections.Count);
        Assert.True(pastExperienceTopic.Examples.Count >= 170);
        Assert.True(pastExperienceTopic.CommonMistakes.Count >= 65);
        Assert.True(pastExperienceTopic.RuleSummaries.Count >= 26);
        Assert.Equal(124, pastExperienceTopic.LinkedWords.Count);
        Assert.Contains("a2-perfekt-with-haben", pastExperienceTopic.PrerequisiteSlugs);
        Assert.Contains("a2-perfekt-with-sein", pastExperienceTopic.PrerequisiteSlugs);
        Assert.Contains("b1-als-versus-wenn", pastExperienceTopic.PrerequisiteSlugs);
        Assert.Contains("b1-nachdem-bevor-waehrend", pastExperienceTopic.PrerequisiteSlugs);
        Assert.Contains("b1-b1-verb-tense-review", pastExperienceTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel storyStructureSection = Assert.Single(pastExperienceTopic.Sections, section => section.SectionKey == "core-story-structure");
        ParsedGrammarSectionModel perfektReviewSection = Assert.Single(pastExperienceTopic.Sections, section => section.SectionKey == "perfekt-review");
        ParsedGrammarSectionModel warHatteSection = Assert.Single(pastExperienceTopic.Sections, section => section.SectionKey == "war-and-hatte");
        ParsedGrammarSectionModel timeExpressionSection = Assert.Single(pastExperienceTopic.Sections, section => section.SectionKey == "time-expressions");
        ParsedGrammarSectionModel sequenceConnectorSection = Assert.Single(pastExperienceTopic.Sections, section => section.SectionKey == "sequence-connectors");
        ParsedGrammarSectionModel speakingExperienceSection = Assert.Single(pastExperienceTopic.Sections, section => section.SectionKey == "experience-in-speaking-exam");
        ParsedGrammarSectionModel storyPatternSection = Assert.Single(pastExperienceTopic.Sections, section => section.SectionKey == "common-story-pattern-table");

        Assert.All(languages, language =>
        {
            Assert.True(pastExperienceTopic.TitleLocalized.ContainsKey(language));
            Assert.True(pastExperienceTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(pastExperienceTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(pastExperienceTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", storyStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", perfektReviewSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", warHatteSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", timeExpressionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", sequenceConnectorSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", speakingExperienceSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", storyPatternSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(pastExperienceTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(pastExperienceTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(pastExperienceTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(pastExperienceTopic.Examples, example => example.GermanText == "Ich habe Deutsch gelernt.");
        Assert.Contains(pastExperienceTopic.Examples, example => example.GermanText == "Ich bin in Deutschland angekommen.");
        Assert.Contains(pastExperienceTopic.Examples, example => example.GermanText == "Ich war nervös.");
        Assert.Contains(pastExperienceTopic.Examples, example => example.GermanText == "Als ich nach Deutschland gekommen bin, war ich nervös.");
        Assert.Contains(pastExperienceTopic.CommonMistakes, mistake => mistake.WrongText == "Ich habe nach Deutschland gekommen.");
        Assert.Contains(pastExperienceTopic.CommonMistakes, mistake => mistake.WrongText == "Ich bin Deutsch gelernt.");
        Assert.Contains(pastExperienceTopic.CommonMistakes, mistake => mistake.WrongText == "Ich war einen Termin.");
        Assert.Contains(pastExperienceTopic.CommonMistakes, mistake => mistake.WrongText == "Nachdem ich habe gearbeitet, war ich müde.");
        Assert.All(pastExperienceTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel plansConditionsTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-talking-about-plans-and-conditions");
        Assert.Equal(1, plansConditionsTopic.ContentRevision);
        Assert.Equal("B1", plansConditionsTopic.CefrLevel);
        Assert.Equal("subordinate-clauses", plansConditionsTopic.GrammarCategory);
        Assert.Equal(15, plansConditionsTopic.Sections.Count);
        Assert.True(plansConditionsTopic.Examples.Count >= 170);
        Assert.True(plansConditionsTopic.CommonMistakes.Count >= 65);
        Assert.True(plansConditionsTopic.RuleSummaries.Count >= 26);
        Assert.Equal(124, plansConditionsTopic.LinkedWords.Count);
        Assert.Contains("a2-wenn-for-conditions", plansConditionsTopic.PrerequisiteSlugs);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", plansConditionsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-konjunktiv-ii-with-waere-haette-wuerde", plansConditionsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-giving-reasons-clearly", plansConditionsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-grammar-for-b1-speaking-exam", plansConditionsTopic.RelatedTopicSlugs);
        Assert.Contains("b2-hypothetical-statements", plansConditionsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel wennFallsSection = Assert.Single(plansConditionsTopic.Sections, section => section.SectionKey == "wenn-vs-falls");
        ParsedGrammarSectionModel conditionFirstSection = Assert.Single(plansConditionsTopic.Sections, section => section.SectionKey == "subordinate-clause-first");
        ParsedGrammarSectionModel politeConditionalSection = Assert.Single(plansConditionsTopic.Sections, section => section.SectionKey == "polite-conditional-with-wuerde");
        ParsedGrammarSectionModel realHypotheticalSection = Assert.Single(plansConditionsTopic.Sections, section => section.SectionKey == "real-vs-hypothetical-condition");
        ParsedGrammarSectionModel werdenPreviewSection = Assert.Single(plansConditionsTopic.Sections, section => section.SectionKey == "plans-with-werden-preview");
        ParsedGrammarSectionModel consequenceConnectorSection = Assert.Single(plansConditionsTopic.Sections, section => section.SectionKey == "consequence-connectors");
        ParsedGrammarSectionModel planningPatternSection = Assert.Single(plansConditionsTopic.Sections, section => section.SectionKey == "common-pattern-table");

        Assert.All(languages, language =>
        {
            Assert.True(plansConditionsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(plansConditionsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(plansConditionsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(plansConditionsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", wennFallsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", conditionFirstSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", politeConditionalSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", realHypotheticalSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", werdenPreviewSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", consequenceConnectorSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", planningPatternSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(plansConditionsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(plansConditionsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(plansConditionsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(plansConditionsTopic.Examples, example => example.GermanText == "Morgen gehe ich zum Kurs.");
        Assert.Contains(plansConditionsTopic.Examples, example => example.GermanText == "Wenn ich Zeit habe, komme ich.");
        Assert.Contains(plansConditionsTopic.Examples, example => example.GermanText == "Falls Sie Fragen haben, schreiben Sie mir bitte.");
        Assert.Contains(plansConditionsTopic.Examples, example => example.GermanText == "Wenn ich Zeit hätte, würde ich kommen.");
        Assert.Contains(plansConditionsTopic.CommonMistakes, mistake => mistake.WrongText == "Wenn ich Zeit habe, ich komme.");
        Assert.Contains(plansConditionsTopic.CommonMistakes, mistake => mistake.WrongText == "Falls Sie Fragen haben, Sie schreiben mir.");
        Assert.Contains(plansConditionsTopic.CommonMistakes, mistake => mistake.WrongText == "Wenn ich Zeit hätte, ich würde kommen.");
        Assert.Contains(plansConditionsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich werde morgen anrufen.");
        Assert.All(plansConditionsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel pastModalTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-modal-verbs-in-the-past");
        Assert.Equal(1, pastModalTopic.ContentRevision);
        Assert.Equal("B1", pastModalTopic.CefrLevel);
        Assert.Equal("modal-verbs", pastModalTopic.GrammarCategory);
        Assert.Equal(16, pastModalTopic.Sections.Count);
        Assert.True(pastModalTopic.Examples.Count >= 130);
        Assert.True(pastModalTopic.CommonMistakes.Count >= 50);
        Assert.True(pastModalTopic.RuleSummaries.Count >= 24);
        Assert.Equal(95, pastModalTopic.LinkedWords.Count);
        Assert.Contains("a2-modal-verbs-in-more-detail", pastModalTopic.PrerequisiteSlugs);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", pastModalTopic.PrerequisiteSlugs);
        Assert.Contains("b1-describing-experiences-in-the-past", pastModalTopic.PrerequisiteSlugs);
        Assert.Contains("b1-verb-tense-review", pastModalTopic.RelatedTopicSlugs);
        Assert.Contains("b1-konjunktiv-ii-with-waere-haette-wuerde", pastModalTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel modalFormsSection = Assert.Single(pastModalTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel modalWordOrderSection = Assert.Single(pastModalTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel modalConjugationSection = Assert.Single(pastModalTopic.Sections, section => section.SectionKey == "praeteritum-forms-table");
        ParsedGrammarSectionModel modalComparisonSection = Assert.Single(pastModalTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel modalPatternsSection = Assert.Single(pastModalTopic.Sections, section => section.SectionKey == "common-patterns");

        Assert.All(languages, language =>
        {
            Assert.True(pastModalTopic.TitleLocalized.ContainsKey(language));
            Assert.True(pastModalTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(pastModalTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(pastModalTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", modalFormsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", modalWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", modalConjugationSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", modalComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", modalPatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(pastModalTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(pastModalTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(pastModalTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(pastModalTopic.Examples, example => example.GermanText == "Ich konnte gestern nicht kommen.");
        Assert.Contains(pastModalTopic.Examples, example => example.GermanText == "Ich musste lange arbeiten.");
        Assert.Contains(pastModalTopic.Examples, example => example.GermanText == "Weil ich arbeiten musste, kam ich später.");
        Assert.Contains(pastModalTopic.Examples, example => example.GermanText == "Konntest du mir gestern helfen?");
        Assert.Contains(pastModalTopic.CommonMistakes, mistake => mistake.WrongText == "Ich musste gearbeitet.");
        Assert.Contains(pastModalTopic.CommonMistakes, mistake => mistake.WrongText == "Gestern ich musste arbeiten.");
        Assert.Contains(pastModalTopic.CommonMistakes, mistake => mistake.WrongText == "Weil ich musste arbeiten.");
        Assert.Contains(pastModalTopic.CommonMistakes, mistake => mistake.WrongText == "Ich musste zu arbeiten.");
        Assert.All(pastModalTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel reflexiveCaseTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-reflexive-verbs-with-dative-and-accusative");
        Assert.Equal(1, reflexiveCaseTopic.ContentRevision);
        Assert.Equal("B1", reflexiveCaseTopic.CefrLevel);
        Assert.Equal("reflexive-verbs", reflexiveCaseTopic.GrammarCategory);
        Assert.Equal(15, reflexiveCaseTopic.Sections.Count);
        Assert.True(reflexiveCaseTopic.Examples.Count >= 130);
        Assert.True(reflexiveCaseTopic.CommonMistakes.Count >= 50);
        Assert.True(reflexiveCaseTopic.RuleSummaries.Count >= 24);
        Assert.Equal(92, reflexiveCaseTopic.LinkedWords.Count);
        Assert.Contains("a2-reflexive-verbs-introduction", reflexiveCaseTopic.PrerequisiteSlugs);
        Assert.Contains("a2-accusative-pronouns", reflexiveCaseTopic.PrerequisiteSlugs);
        Assert.Contains("a2-dative-pronouns", reflexiveCaseTopic.PrerequisiteSlugs);
        Assert.Contains("a2-accusative-versus-dative-basics", reflexiveCaseTopic.PrerequisiteSlugs);
        Assert.Contains("b1-prepositional-verbs-introduction", reflexiveCaseTopic.RelatedTopicSlugs);
        Assert.Contains("b1-verb-plus-preposition-combinations", reflexiveCaseTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel reflexivePronounSection = Assert.Single(reflexiveCaseTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel reflexiveCoreSection = Assert.Single(reflexiveCaseTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel reflexiveCaseSection = Assert.Single(reflexiveCaseTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel reflexiveComparisonSection = Assert.Single(reflexiveCaseTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel reflexivePatternSection = Assert.Single(reflexiveCaseTopic.Sections, section => section.SectionKey == "common-patterns");
        ParsedGrammarSectionModel reflexivePracticeSection = Assert.Single(reflexiveCaseTopic.Sections, section => section.SectionKey == "practice-advice");

        Assert.All(languages, language =>
        {
            Assert.True(reflexiveCaseTopic.TitleLocalized.ContainsKey(language));
            Assert.True(reflexiveCaseTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(reflexiveCaseTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(reflexiveCaseTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", reflexivePronounSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", reflexiveCoreSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", reflexiveCaseSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", reflexiveComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", reflexivePatternSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", reflexivePracticeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(reflexiveCaseTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(reflexiveCaseTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(reflexiveCaseTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(reflexiveCaseTopic.Examples, example => example.GermanText == "Ich wasche mich.");
        Assert.Contains(reflexiveCaseTopic.Examples, example => example.GermanText == "Ich wasche mir die Hände.");
        Assert.Contains(reflexiveCaseTopic.Examples, example => example.GermanText == "Ich freue mich auf den Kurs.");
        Assert.Contains(reflexiveCaseTopic.Examples, example => example.GermanText == "Ich merke mir den Termin.");
        Assert.Contains(reflexiveCaseTopic.CommonMistakes, mistake => mistake.WrongText == "Ich wasche mir.");
        Assert.Contains(reflexiveCaseTopic.CommonMistakes, mistake => mistake.WrongText == "Ich wasche mich die Hände.");
        Assert.Contains(reflexiveCaseTopic.CommonMistakes, mistake => mistake.WrongText == "Ich freue mir auf den Kurs.");
        Assert.Contains(reflexiveCaseTopic.CommonMistakes, mistake => mistake.WrongText == "Ich merke mich den Termin.");
        Assert.All(reflexiveCaseTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel lassenTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-lassen-basics");
        Assert.Equal(1, lassenTopic.ContentRevision);
        Assert.Equal("B1", lassenTopic.CefrLevel);
        Assert.Equal("verbs", lassenTopic.GrammarCategory);
        Assert.Equal(16, lassenTopic.Sections.Count);
        Assert.True(lassenTopic.Examples.Count >= 130);
        Assert.True(lassenTopic.CommonMistakes.Count >= 50);
        Assert.True(lassenTopic.RuleSummaries.Count >= 24);
        Assert.Equal(95, lassenTopic.LinkedWords.Count);
        Assert.Contains("a2-modal-verbs-in-more-detail", lassenTopic.PrerequisiteSlugs);
        Assert.Contains("b1-infinitive-with-zu", lassenTopic.PrerequisiteSlugs);
        Assert.Contains("b1-passive-voice-introduction", lassenTopic.PrerequisiteSlugs);
        Assert.Contains("b1-werden-as-auxiliary", lassenTopic.RelatedTopicSlugs);
        Assert.Contains("b1-modal-verbs-in-the-past", lassenTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel lassenFormsSection = Assert.Single(lassenTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel lassenCoreSection = Assert.Single(lassenTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel lassenWordOrderSection = Assert.Single(lassenTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel lassenInfinitiveSection = Assert.Single(lassenTopic.Sections, section => section.SectionKey == "lassen-and-infinitive");
        ParsedGrammarSectionModel lassenPerfectSection = Assert.Single(lassenTopic.Sections, section => section.SectionKey == "perfect-with-lassen");
        ParsedGrammarSectionModel lassenPassiveSection = Assert.Single(lassenTopic.Sections, section => section.SectionKey == "lassen-vs-passive");
        ParsedGrammarSectionModel lassenComparisonSection = Assert.Single(lassenTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel lassenPatternSection = Assert.Single(lassenTopic.Sections, section => section.SectionKey == "common-patterns");

        Assert.All(languages, language =>
        {
            Assert.True(lassenTopic.TitleLocalized.ContainsKey(language));
            Assert.True(lassenTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(lassenTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(lassenTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", lassenFormsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", lassenCoreSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", lassenWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", lassenInfinitiveSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", lassenPerfectSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", lassenPassiveSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", lassenComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", lassenPatternSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(lassenTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(lassenTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(lassenTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(lassenTopic.Examples, example => example.GermanText == "Ich lasse das Auto reparieren.");
        Assert.Contains(lassenTopic.Examples, example => example.GermanText == "Ich lasse mein Kind spielen.");
        Assert.Contains(lassenTopic.Examples, example => example.GermanText == "Ich habe das Auto reparieren lassen.");
        Assert.Contains(lassenTopic.Examples, example => example.GermanText == "Ich möchte mich beraten lassen.");
        Assert.Contains(lassenTopic.CommonMistakes, mistake => mistake.WrongText == "Ich lasse das Auto zu reparieren.");
        Assert.Contains(lassenTopic.CommonMistakes, mistake => mistake.WrongText == "Ich lasse das Auto repariert.");
        Assert.Contains(lassenTopic.CommonMistakes, mistake => mistake.WrongText == "Ich habe das Auto reparieren gelassen.");
        Assert.Contains(lassenTopic.CommonMistakes, mistake => mistake.WrongText == "Du lasst das prüfen.");
        Assert.All(lassenTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel brauchenZuTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-brauchen-plus-zu");
        Assert.Equal(1, brauchenZuTopic.ContentRevision);
        Assert.Equal("B1", brauchenZuTopic.CefrLevel);
        Assert.Equal("verbs", brauchenZuTopic.GrammarCategory);
        Assert.Equal(15, brauchenZuTopic.Sections.Count);
        Assert.True(brauchenZuTopic.Examples.Count >= 130);
        Assert.True(brauchenZuTopic.CommonMistakes.Count >= 50);
        Assert.True(brauchenZuTopic.RuleSummaries.Count >= 24);
        Assert.Equal(92, brauchenZuTopic.LinkedWords.Count);
        Assert.Contains("a2-zu-plus-infinitive-introduction", brauchenZuTopic.PrerequisiteSlugs);
        Assert.Contains("b1-infinitive-with-zu", brauchenZuTopic.PrerequisiteSlugs);
        Assert.Contains("a2-modal-verbs-in-more-detail", brauchenZuTopic.PrerequisiteSlugs);
        Assert.Contains("b1-lassen-basics", brauchenZuTopic.RelatedTopicSlugs);
        Assert.Contains("b1-modal-verbs-in-the-past", brauchenZuTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel brauchenCoreSection = Assert.Single(brauchenZuTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel brauchenStructureSection = Assert.Single(brauchenZuTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel brauchenWordOrderSection = Assert.Single(brauchenZuTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel brauchenSeparableSection = Assert.Single(brauchenZuTopic.Sections, section => section.SectionKey == "separable-verbs");
        ParsedGrammarSectionModel brauchenMainVerbSection = Assert.Single(brauchenZuTopic.Sections, section => section.SectionKey == "brauchen-as-main-verb");
        ParsedGrammarSectionModel brauchenComparisonSection = Assert.Single(brauchenZuTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel brauchenPatternSection = Assert.Single(brauchenZuTopic.Sections, section => section.SectionKey == "common-patterns");

        Assert.All(languages, language =>
        {
            Assert.True(brauchenZuTopic.TitleLocalized.ContainsKey(language));
            Assert.True(brauchenZuTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(brauchenZuTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(brauchenZuTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", brauchenCoreSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", brauchenStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", brauchenWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", brauchenSeparableSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", brauchenMainVerbSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", brauchenComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", brauchenPatternSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(brauchenZuTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(brauchenZuTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(brauchenZuTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(brauchenZuTopic.Examples, example => example.GermanText == "Du brauchst nicht zu kommen.");
        Assert.Contains(brauchenZuTopic.Examples, example => example.GermanText == "Du brauchst nur anzurufen.");
        Assert.Contains(brauchenZuTopic.Examples, example => example.GermanText == "Wir brauchen kein Formular auszufüllen.");
        Assert.Contains(brauchenZuTopic.Examples, example => example.GermanText == "Ich brauche Hilfe.");
        Assert.Contains(brauchenZuTopic.CommonMistakes, mistake => mistake.WrongText == "Du brauchst nicht kommen.");
        Assert.Contains(brauchenZuTopic.CommonMistakes, mistake => mistake.WrongText == "Sie brauchen nicht zu anrufen.");
        Assert.Contains(brauchenZuTopic.CommonMistakes, mistake => mistake.WrongText == "Ich brauche zu Hilfe.");
        Assert.Contains(brauchenZuTopic.CommonMistakes, mistake => mistake.WrongText == "Ich muss nicht zu kommen.");
        Assert.All(brauchenZuTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel jeDestoTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-je-desto-introduction");
        Assert.Equal(1, jeDestoTopic.ContentRevision);
        Assert.Equal("B1", jeDestoTopic.CefrLevel);
        Assert.Equal("connectors", jeDestoTopic.GrammarCategory);
        Assert.Equal(15, jeDestoTopic.Sections.Count);
        Assert.True(jeDestoTopic.Examples.Count >= 130);
        Assert.True(jeDestoTopic.CommonMistakes.Count >= 50);
        Assert.True(jeDestoTopic.RuleSummaries.Count >= 24);
        Assert.Equal(96, jeDestoTopic.LinkedWords.Count);
        Assert.Contains("a2-comparative-forms", jeDestoTopic.PrerequisiteSlugs);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", jeDestoTopic.PrerequisiteSlugs);
        Assert.Contains("b1-connectors-for-cause-and-effect", jeDestoTopic.PrerequisiteSlugs);
        Assert.Contains("b1-sentence-order-with-multiple-clauses", jeDestoTopic.PrerequisiteSlugs);
        Assert.Contains("b1-b1-connector-review", jeDestoTopic.RelatedTopicSlugs);
        Assert.Contains("b2-je-desto-advanced", jeDestoTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel jeDestoCoreSection = Assert.Single(jeDestoTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel jeDestoStructureSection = Assert.Single(jeDestoTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel jeDestoWordOrderSection = Assert.Single(jeDestoTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel jeDestoComparativeSection = Assert.Single(jeDestoTopic.Sections, section => section.SectionKey == "comparative-forms-review");
        ParsedGrammarSectionModel jeDestoDestoOrderSection = Assert.Single(jeDestoTopic.Sections, section => section.SectionKey == "desto-main-clause-order");
        ParsedGrammarSectionModel jeDestoComparisonSection = Assert.Single(jeDestoTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel jeDestoPatternSection = Assert.Single(jeDestoTopic.Sections, section => section.SectionKey == "common-patterns");

        Assert.All(languages, language =>
        {
            Assert.True(jeDestoTopic.TitleLocalized.ContainsKey(language));
            Assert.True(jeDestoTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(jeDestoTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(jeDestoTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", jeDestoCoreSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", jeDestoStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", jeDestoWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", jeDestoComparativeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", jeDestoDestoOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", jeDestoComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", jeDestoPatternSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(jeDestoTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(jeDestoTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(jeDestoTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(jeDestoTopic.Examples, example => example.GermanText == "Je mehr ich lerne, desto besser spreche ich Deutsch.");
        Assert.Contains(jeDestoTopic.Examples, example => example.GermanText == "Je früher du anfängst, desto leichter wird es.");
        Assert.Contains(jeDestoTopic.Examples, example => example.GermanText == "Je länger ich warte, desto nervöser werde ich.");
        Assert.Contains(jeDestoTopic.Examples, example => example.GermanText == "Je mehr wir üben, desto sicherer werden wir.");
        Assert.Contains(jeDestoTopic.CommonMistakes, mistake => mistake.WrongText == "Je mehr ich lerne, desto ich spreche besser Deutsch.");
        Assert.Contains(jeDestoTopic.CommonMistakes, mistake => mistake.WrongText == "Je mehr lerne ich, desto besser spreche ich Deutsch.");
        Assert.Contains(jeDestoTopic.CommonMistakes, mistake => mistake.WrongText == "Je mehr ich lerne, besser spreche ich Deutsch.");
        Assert.Contains(jeDestoTopic.CommonMistakes, mistake => mistake.WrongText == "Je länger ich warte, desto nervöser ich werde.");
        Assert.All(jeDestoTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel speakingGrammarTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-grammar-for-b1-speaking-exam");
        Assert.Equal(1, speakingGrammarTopic.ContentRevision);
        Assert.Equal("B1", speakingGrammarTopic.CefrLevel);
        Assert.Equal("questions", speakingGrammarTopic.GrammarCategory);
        Assert.Equal(15, speakingGrammarTopic.Sections.Count);
        Assert.True(speakingGrammarTopic.Examples.Count >= 130);
        Assert.True(speakingGrammarTopic.CommonMistakes.Count >= 50);
        Assert.True(speakingGrammarTopic.RuleSummaries.Count >= 24);
        Assert.Equal(90, speakingGrammarTopic.LinkedWords.Count);
        Assert.Contains("b1-indirect-questions", speakingGrammarTopic.PrerequisiteSlugs);
        Assert.Contains("b1-konjunktiv-ii-for-polite-requests", speakingGrammarTopic.PrerequisiteSlugs);
        Assert.Contains("b1-connectors-for-opinion", speakingGrammarTopic.PrerequisiteSlugs);
        Assert.Contains("b1-giving-reasons-clearly", speakingGrammarTopic.PrerequisiteSlugs);
        Assert.Contains("b1-grammar-for-b1-writing-exam", speakingGrammarTopic.RelatedTopicSlugs);
        Assert.Contains("b1-reported-requests-and-polite-questions", speakingGrammarTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel speakingCoreSection = Assert.Single(speakingGrammarTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel speakingStructureSection = Assert.Single(speakingGrammarTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel speakingWordOrderSection = Assert.Single(speakingGrammarTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel speakingDirectQuestionSection = Assert.Single(speakingGrammarTopic.Sections, section => section.SectionKey == "direct-questions");
        ParsedGrammarSectionModel speakingIndirectQuestionSection = Assert.Single(speakingGrammarTopic.Sections, section => section.SectionKey == "indirect-questions-and-ob");
        ParsedGrammarSectionModel speakingPoliteRequestSection = Assert.Single(speakingGrammarTopic.Sections, section => section.SectionKey == "polite-requests");
        ParsedGrammarSectionModel speakingOpinionSection = Assert.Single(speakingGrammarTopic.Sections, section => section.SectionKey == "opinion-and-reason");
        ParsedGrammarSectionModel speakingComparisonSection = Assert.Single(speakingGrammarTopic.Sections, section => section.SectionKey == "comparison-table");

        Assert.All(languages, language =>
        {
            Assert.True(speakingGrammarTopic.TitleLocalized.ContainsKey(language));
            Assert.True(speakingGrammarTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(speakingGrammarTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(speakingGrammarTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", speakingCoreSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", speakingStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", speakingWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", speakingDirectQuestionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", speakingIndirectQuestionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", speakingPoliteRequestSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", speakingOpinionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", speakingComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(speakingGrammarTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(speakingGrammarTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(speakingGrammarTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(speakingGrammarTopic.Examples, example => example.GermanText == "Ich denke, dass Deutsch im Alltag wichtig ist.");
        Assert.Contains(speakingGrammarTopic.Examples, example => example.GermanText == "Könnten Sie bitte die Frage wiederholen?");
        Assert.Contains(speakingGrammarTopic.Examples, example => example.GermanText == "Können Sie mir sagen, wann der Kurs beginnt?");
        Assert.Contains(speakingGrammarTopic.Examples, example => example.GermanText == "Wissen Sie, ob der Termin frei ist?");
        Assert.Contains(speakingGrammarTopic.CommonMistakes, mistake => mistake.WrongText == "Können Sie sagen mir, wann der Kurs beginnt?");
        Assert.Contains(speakingGrammarTopic.CommonMistakes, mistake => mistake.WrongText == "Wissen Sie, kommt der Bus?");
        Assert.Contains(speakingGrammarTopic.CommonMistakes, mistake => mistake.WrongText == "Deshalb ich finde das gut.");
        Assert.Contains(speakingGrammarTopic.CommonMistakes, mistake => mistake.WrongText == "Wenn ich Zeit habe, ich komme.");
        Assert.All(speakingGrammarTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel writingGrammarTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-grammar-for-b1-writing-exam");
        Assert.Equal(1, writingGrammarTopic.ContentRevision);
        Assert.Equal("B1", writingGrammarTopic.CefrLevel);
        Assert.Equal("word-order", writingGrammarTopic.GrammarCategory);
        Assert.Equal(15, writingGrammarTopic.Sections.Count);
        Assert.True(writingGrammarTopic.Examples.Count >= 130);
        Assert.True(writingGrammarTopic.CommonMistakes.Count >= 50);
        Assert.True(writingGrammarTopic.RuleSummaries.Count >= 24);
        Assert.Equal(90, writingGrammarTopic.LinkedWords.Count);
        Assert.Contains("b1-formal-email-sentence-structure", writingGrammarTopic.PrerequisiteSlugs);
        Assert.Contains("b1-complaint-sentence-patterns", writingGrammarTopic.PrerequisiteSlugs);
        Assert.Contains("b1-sentence-order-with-multiple-clauses", writingGrammarTopic.PrerequisiteSlugs);
        Assert.Contains("b1-grammar-for-b1-speaking-exam", writingGrammarTopic.RelatedTopicSlugs);
        Assert.Contains("b2-b2-exam-writing-structures", writingGrammarTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel writingCoreSection = Assert.Single(writingGrammarTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel writingStructureSection = Assert.Single(writingGrammarTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel writingWordOrderSection = Assert.Single(writingGrammarTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel writingBracketSection = Assert.Single(writingGrammarTopic.Sections, section => section.SectionKey == "sentence-bracket");
        ParsedGrammarSectionModel writingSubordinateSection = Assert.Single(writingGrammarTopic.Sections, section => section.SectionKey == "subordinate-clauses");
        ParsedGrammarSectionModel writingCommaSection = Assert.Single(writingGrammarTopic.Sections, section => section.SectionKey == "comma-basics");
        ParsedGrammarSectionModel writingEmailSection = Assert.Single(writingGrammarTopic.Sections, section => section.SectionKey == "formal-email-sentences");
        ParsedGrammarSectionModel writingComparisonSection = Assert.Single(writingGrammarTopic.Sections, section => section.SectionKey == "comparison-table");

        Assert.All(languages, language =>
        {
            Assert.True(writingGrammarTopic.TitleLocalized.ContainsKey(language));
            Assert.True(writingGrammarTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(writingGrammarTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(writingGrammarTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", writingCoreSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", writingStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", writingWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", writingBracketSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", writingSubordinateSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", writingCommaSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", writingEmailSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", writingComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(writingGrammarTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(writingGrammarTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(writingGrammarTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(writingGrammarTopic.Examples, example => example.GermanText == "Ich schreibe Ihnen, weil ich eine Frage habe.");
        Assert.Contains(writingGrammarTopic.Examples, example => example.GermanText == "Aus diesem Grund bitte ich Sie um einen neuen Termin.");
        Assert.Contains(writingGrammarTopic.Examples, example => example.GermanText == "Ich habe gesehen, dass die Rechnung falsch ist.");
        Assert.Contains(writingGrammarTopic.Examples, example => example.GermanText == "Ich möchte wissen, ob der Termin frei ist.");
        Assert.Contains(writingGrammarTopic.CommonMistakes, mistake => mistake.WrongText == "Ich schreibe Ihnen, weil ich habe eine Frage.");
        Assert.Contains(writingGrammarTopic.CommonMistakes, mistake => mistake.WrongText == "Ich habe gesehen, dass die Rechnung ist falsch.");
        Assert.Contains(writingGrammarTopic.CommonMistakes, mistake => mistake.WrongText == "Aus diesem Grund ich bitte Sie um Hilfe.");
        Assert.Contains(writingGrammarTopic.CommonMistakes, mistake => mistake.WrongText == "Wenn es möglich ist, ich komme morgen.");
        Assert.All(writingGrammarTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel b1MistakePatternsTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-b1-mistake-patterns");
        Assert.Equal(1, b1MistakePatternsTopic.ContentRevision);
        Assert.Equal("B1", b1MistakePatternsTopic.CefrLevel);
        Assert.Equal("cases", b1MistakePatternsTopic.GrammarCategory);
        Assert.Equal(16, b1MistakePatternsTopic.Sections.Count);
        Assert.True(b1MistakePatternsTopic.Examples.Count >= 130);
        Assert.True(b1MistakePatternsTopic.CommonMistakes.Count >= 50);
        Assert.True(b1MistakePatternsTopic.RuleSummaries.Count >= 24);
        Assert.Equal(100, b1MistakePatternsTopic.LinkedWords.Count);
        Assert.Contains("a2-accusative-versus-dative-basics", b1MistakePatternsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-sentence-order-with-multiple-clauses", b1MistakePatternsTopic.PrerequisiteSlugs);
        Assert.Contains("b1-b1-case-review", b1MistakePatternsTopic.RelatedTopicSlugs);
        Assert.Contains("b1-b1-connector-review", b1MistakePatternsTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel b1MistakeCoreSection = Assert.Single(b1MistakePatternsTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel b1MistakeStructureSection = Assert.Single(b1MistakePatternsTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel b1MistakeCaseSection = Assert.Single(b1MistakePatternsTopic.Sections, section => section.SectionKey == "case-role-decision");
        ParsedGrammarSectionModel b1MistakeArticleSection = Assert.Single(b1MistakePatternsTopic.Sections, section => section.SectionKey == "article-change-patterns");
        ParsedGrammarSectionModel b1MistakePrepositionSection = Assert.Single(b1MistakePatternsTopic.Sections, section => section.SectionKey == "preposition-mistakes");
        ParsedGrammarSectionModel b1MistakeWordOrderSection = Assert.Single(b1MistakePatternsTopic.Sections, section => section.SectionKey == "word-order-mistakes");
        ParsedGrammarSectionModel b1MistakeConnectorSection = Assert.Single(b1MistakePatternsTopic.Sections, section => section.SectionKey == "connector-and-comma-mistakes");
        ParsedGrammarSectionModel b1MistakeComparisonSection = Assert.Single(b1MistakePatternsTopic.Sections, section => section.SectionKey == "comparison-table");
        ParsedGrammarSectionModel b1MistakePatternsSection = Assert.Single(b1MistakePatternsTopic.Sections, section => section.SectionKey == "common-patterns");

        Assert.All(languages, language =>
        {
            Assert.True(b1MistakePatternsTopic.TitleLocalized.ContainsKey(language));
            Assert.True(b1MistakePatternsTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(b1MistakePatternsTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(b1MistakePatternsTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", b1MistakeCoreSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1MistakeStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1MistakeCaseSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1MistakeArticleSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1MistakePrepositionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1MistakeWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1MistakeConnectorSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1MistakeComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1MistakePatternsSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(b1MistakePatternsTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(b1MistakePatternsTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(b1MistakePatternsTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(b1MistakePatternsTopic.Examples, example => example.GermanText == "Ich sehe den Mann.");
        Assert.Contains(b1MistakePatternsTopic.Examples, example => example.GermanText == "Ich helfe dem Mann.");
        Assert.Contains(b1MistakePatternsTopic.Examples, example => example.GermanText == "Ich warte auf den Bus.");
        Assert.Contains(b1MistakePatternsTopic.Examples, example => example.GermanText == "Weil ich krank bin, bleibe ich zu Hause.");
        Assert.Contains(b1MistakePatternsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich sehe der Mann.");
        Assert.Contains(b1MistakePatternsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich helfe den Mann.");
        Assert.Contains(b1MistakePatternsTopic.CommonMistakes, mistake => mistake.WrongText == "Ich warte für den Bus.");
        Assert.Contains(b1MistakePatternsTopic.CommonMistakes, mistake => mistake.WrongText == "Weil ich krank bin, ich bleibe zu Hause.");
        Assert.All(b1MistakePatternsTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel b1CaseReviewTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-b1-case-review");
        Assert.Equal(1, b1CaseReviewTopic.ContentRevision);
        Assert.Equal("B1", b1CaseReviewTopic.CefrLevel);
        Assert.Equal("cases", b1CaseReviewTopic.GrammarCategory);
        Assert.Equal(16, b1CaseReviewTopic.Sections.Count);
        Assert.True(b1CaseReviewTopic.Examples.Count >= 130);
        Assert.True(b1CaseReviewTopic.CommonMistakes.Count >= 50);
        Assert.True(b1CaseReviewTopic.RuleSummaries.Count >= 24);
        Assert.Equal(106, b1CaseReviewTopic.LinkedWords.Count);
        Assert.Contains("a2-a2-case-review", b1CaseReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a2-accusative-versus-dative-basics", b1CaseReviewTopic.PrerequisiteSlugs);
        Assert.Contains("b1-genitive-introduction", b1CaseReviewTopic.PrerequisiteSlugs);
        Assert.Contains("b1-b1-mistake-patterns", b1CaseReviewTopic.RelatedTopicSlugs);
        Assert.Contains("b1-adjective-declension-after-definite-article", b1CaseReviewTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel b1CaseCoreSection = Assert.Single(b1CaseReviewTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel b1CaseStructureSection = Assert.Single(b1CaseReviewTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel b1CaseDecisionSection = Assert.Single(b1CaseReviewTopic.Sections, section => section.SectionKey == "case-decision-questions");
        ParsedGrammarSectionModel b1CaseNominativeSection = Assert.Single(b1CaseReviewTopic.Sections, section => section.SectionKey == "nominative-review");
        ParsedGrammarSectionModel b1CaseAccusativeSection = Assert.Single(b1CaseReviewTopic.Sections, section => section.SectionKey == "accusative-review");
        ParsedGrammarSectionModel b1CaseDativeSection = Assert.Single(b1CaseReviewTopic.Sections, section => section.SectionKey == "dative-review");
        ParsedGrammarSectionModel b1CaseGenitiveSection = Assert.Single(b1CaseReviewTopic.Sections, section => section.SectionKey == "genitive-recognition");
        ParsedGrammarSectionModel b1CaseSameNounSection = Assert.Single(b1CaseReviewTopic.Sections, section => section.SectionKey == "same-noun-different-case");
        ParsedGrammarSectionModel b1CasePrepositionSection = Assert.Single(b1CaseReviewTopic.Sections, section => section.SectionKey == "prepositions-and-case");
        ParsedGrammarSectionModel b1CasePronounSection = Assert.Single(b1CaseReviewTopic.Sections, section => section.SectionKey == "pronouns-and-case");
        ParsedGrammarSectionModel b1CaseComparisonSection = Assert.Single(b1CaseReviewTopic.Sections, section => section.SectionKey == "comparison-table");

        Assert.All(languages, language =>
        {
            Assert.True(b1CaseReviewTopic.TitleLocalized.ContainsKey(language));
            Assert.True(b1CaseReviewTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(b1CaseReviewTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(b1CaseReviewTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", b1CaseCoreSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1CaseStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1CaseDecisionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1CaseNominativeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1CaseAccusativeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1CaseDativeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1CaseGenitiveSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1CaseSameNounSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1CasePrepositionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1CasePronounSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1CaseComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(b1CaseReviewTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(b1CaseReviewTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(b1CaseReviewTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(b1CaseReviewTopic.Examples, example => example.GermanText == "Ich sehe den Mann.");
        Assert.Contains(b1CaseReviewTopic.Examples, example => example.GermanText == "Ich helfe dem Mann.");
        Assert.Contains(b1CaseReviewTopic.Examples, example => example.GermanText == "Wegen des Mannes komme ich später.");
        Assert.Contains(b1CaseReviewTopic.Examples, example => example.GermanText == "Ich gehe in die Schule.");
        Assert.Contains(b1CaseReviewTopic.CommonMistakes, mistake => mistake.WrongText == "Ich sehe der Mann.");
        Assert.Contains(b1CaseReviewTopic.CommonMistakes, mistake => mistake.WrongText == "Ich helfe den Mann.");
        Assert.Contains(b1CaseReviewTopic.CommonMistakes, mistake => mistake.WrongText == "Wegen der Termin komme ich später.");
        Assert.Contains(b1CaseReviewTopic.CommonMistakes, mistake => mistake.WrongText == "Können Sie mich helfen?");
        Assert.All(b1CaseReviewTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel b1ConnectorReviewTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-b1-connector-review");
        Assert.Equal(1, b1ConnectorReviewTopic.ContentRevision);
        Assert.Equal("B1", b1ConnectorReviewTopic.CefrLevel);
        Assert.Equal("connectors", b1ConnectorReviewTopic.GrammarCategory);
        Assert.Equal(16, b1ConnectorReviewTopic.Sections.Count);
        Assert.True(b1ConnectorReviewTopic.Examples.Count >= 130);
        Assert.True(b1ConnectorReviewTopic.CommonMistakes.Count >= 50);
        Assert.True(b1ConnectorReviewTopic.RuleSummaries.Count >= 24);
        Assert.Equal(96, b1ConnectorReviewTopic.LinkedWords.Count);
        Assert.Contains("a2-a2-connectors-overview", b1ConnectorReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a2-sentence-order-in-subordinate-clauses", b1ConnectorReviewTopic.PrerequisiteSlugs);
        Assert.Contains("b1-weil-obwohl-trotzdem", b1ConnectorReviewTopic.PrerequisiteSlugs);
        Assert.Contains("b1-connectors-for-cause-and-effect", b1ConnectorReviewTopic.RelatedTopicSlugs);
        Assert.Contains("b1-sentence-order-with-multiple-clauses", b1ConnectorReviewTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel b1ConnectorCoreSection = Assert.Single(b1ConnectorReviewTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel b1ConnectorStructureSection = Assert.Single(b1ConnectorReviewTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel b1ConnectorWordOrderSection = Assert.Single(b1ConnectorReviewTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel b1ConnectorCauseSection = Assert.Single(b1ConnectorReviewTopic.Sections, section => section.SectionKey == "cause-connectors");
        ParsedGrammarSectionModel b1ConnectorResultSection = Assert.Single(b1ConnectorReviewTopic.Sections, section => section.SectionKey == "result-connectors");
        ParsedGrammarSectionModel b1ConnectorContrastSection = Assert.Single(b1ConnectorReviewTopic.Sections, section => section.SectionKey == "contrast-connectors");
        ParsedGrammarSectionModel b1ConnectorConditionSection = Assert.Single(b1ConnectorReviewTopic.Sections, section => section.SectionKey == "condition-connectors");
        ParsedGrammarSectionModel b1ConnectorTimeSection = Assert.Single(b1ConnectorReviewTopic.Sections, section => section.SectionKey == "time-connectors");
        ParsedGrammarSectionModel b1ConnectorPurposeSection = Assert.Single(b1ConnectorReviewTopic.Sections, section => section.SectionKey == "purpose-connectors");
        ParsedGrammarSectionModel b1ConnectorComparisonSection = Assert.Single(b1ConnectorReviewTopic.Sections, section => section.SectionKey == "comparison-table");

        Assert.All(languages, language =>
        {
            Assert.True(b1ConnectorReviewTopic.TitleLocalized.ContainsKey(language));
            Assert.True(b1ConnectorReviewTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(b1ConnectorReviewTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(b1ConnectorReviewTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", b1ConnectorCoreSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1ConnectorStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1ConnectorWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1ConnectorCauseSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1ConnectorResultSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1ConnectorContrastSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1ConnectorConditionSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1ConnectorTimeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1ConnectorPurposeSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1ConnectorComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(b1ConnectorReviewTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(b1ConnectorReviewTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(b1ConnectorReviewTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(b1ConnectorReviewTopic.Examples, example => example.GermanText == "Ich komme nicht, weil ich krank bin.");
        Assert.Contains(b1ConnectorReviewTopic.Examples, example => example.GermanText == "Ich bin krank. Deshalb komme ich nicht.");
        Assert.Contains(b1ConnectorReviewTopic.Examples, example => example.GermanText == "Ich gehe zur Arbeit, obwohl ich krank bin.");
        Assert.Contains(b1ConnectorReviewTopic.Examples, example => example.GermanText == "Ich bin krank. Trotzdem gehe ich zur Arbeit.");
        Assert.Contains(b1ConnectorReviewTopic.CommonMistakes, mistake => mistake.WrongText == "Ich komme nicht, weil ich bin krank.");
        Assert.Contains(b1ConnectorReviewTopic.CommonMistakes, mistake => mistake.WrongText == "Deshalb ich komme nicht.");
        Assert.Contains(b1ConnectorReviewTopic.CommonMistakes, mistake => mistake.WrongText == "Trotzdem ich gehe zur Arbeit.");
        Assert.Contains(b1ConnectorReviewTopic.CommonMistakes, mistake => mistake.WrongText == "Ich lerne Deutsch, um in Deutschland arbeiten.");
        Assert.All(b1ConnectorReviewTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel b1VerbTenseReviewTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-b1-verb-tense-review");
        Assert.Equal(1, b1VerbTenseReviewTopic.ContentRevision);
        Assert.Equal("B1", b1VerbTenseReviewTopic.CefrLevel);
        Assert.Equal("tenses", b1VerbTenseReviewTopic.GrammarCategory);
        Assert.Equal(16, b1VerbTenseReviewTopic.Sections.Count);
        Assert.True(b1VerbTenseReviewTopic.Examples.Count >= 130);
        Assert.True(b1VerbTenseReviewTopic.CommonMistakes.Count >= 50);
        Assert.True(b1VerbTenseReviewTopic.RuleSummaries.Count >= 24);
        Assert.Equal(102, b1VerbTenseReviewTopic.LinkedWords.Count);
        Assert.Contains("a2-perfekt-with-haben", b1VerbTenseReviewTopic.PrerequisiteSlugs);
        Assert.Contains("a2-perfekt-with-sein", b1VerbTenseReviewTopic.PrerequisiteSlugs);
        Assert.Contains("b1-modal-verbs-in-the-past", b1VerbTenseReviewTopic.PrerequisiteSlugs);
        Assert.Contains("b1-describing-experiences-in-the-past", b1VerbTenseReviewTopic.RelatedTopicSlugs);
        Assert.Contains("b1-talking-about-plans-and-conditions", b1VerbTenseReviewTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel b1VerbTenseCoreSection = Assert.Single(b1VerbTenseReviewTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel b1VerbTenseStructureSection = Assert.Single(b1VerbTenseReviewTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel b1VerbTenseWordOrderSection = Assert.Single(b1VerbTenseReviewTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel b1VerbTensePraesensSection = Assert.Single(b1VerbTenseReviewTopic.Sections, section => section.SectionKey == "praesens-review");
        ParsedGrammarSectionModel b1VerbTenseHabenSection = Assert.Single(b1VerbTenseReviewTopic.Sections, section => section.SectionKey == "perfekt-with-haben");
        ParsedGrammarSectionModel b1VerbTenseSeinSection = Assert.Single(b1VerbTenseReviewTopic.Sections, section => section.SectionKey == "perfekt-with-sein");
        ParsedGrammarSectionModel b1VerbTenseWarHatteSection = Assert.Single(b1VerbTenseReviewTopic.Sections, section => section.SectionKey == "war-and-hatte");
        ParsedGrammarSectionModel b1VerbTensePraeteritumSection = Assert.Single(b1VerbTenseReviewTopic.Sections, section => section.SectionKey == "praeteritum-basics");
        ParsedGrammarSectionModel b1VerbTenseModalSection = Assert.Single(b1VerbTenseReviewTopic.Sections, section => section.SectionKey == "modal-past-review");
        ParsedGrammarSectionModel b1VerbTenseSequenceSection = Assert.Single(b1VerbTenseReviewTopic.Sections, section => section.SectionKey == "time-expressions-and-sequence");
        ParsedGrammarSectionModel b1VerbTenseComparisonSection = Assert.Single(b1VerbTenseReviewTopic.Sections, section => section.SectionKey == "comparison-table");

        Assert.All(languages, language =>
        {
            Assert.True(b1VerbTenseReviewTopic.TitleLocalized.ContainsKey(language));
            Assert.True(b1VerbTenseReviewTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(b1VerbTenseReviewTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(b1VerbTenseReviewTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", b1VerbTenseCoreSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1VerbTenseStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1VerbTenseWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1VerbTensePraesensSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1VerbTenseHabenSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1VerbTenseSeinSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1VerbTenseWarHatteSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1VerbTensePraeteritumSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1VerbTenseModalSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1VerbTenseSequenceSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1VerbTenseComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(b1VerbTenseReviewTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(b1VerbTenseReviewTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(b1VerbTenseReviewTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(b1VerbTenseReviewTopic.Examples, example => example.GermanText == "Ich habe Deutsch gelernt.");
        Assert.Contains(b1VerbTenseReviewTopic.Examples, example => example.GermanText == "Ich bin zur Schule gegangen.");
        Assert.Contains(b1VerbTenseReviewTopic.Examples, example => example.GermanText == "Gestern war ich krank.");
        Assert.Contains(b1VerbTenseReviewTopic.Examples, example => example.GermanText == "Ich konnte gestern nicht kommen.");
        Assert.Contains(b1VerbTenseReviewTopic.CommonMistakes, mistake => mistake.WrongText == "Ich habe nach Hause gegangen.");
        Assert.Contains(b1VerbTenseReviewTopic.CommonMistakes, mistake => mistake.WrongText == "Ich bin Deutsch gelernt.");
        Assert.Contains(b1VerbTenseReviewTopic.CommonMistakes, mistake => mistake.WrongText == "Ich war einen Termin.");
        Assert.Contains(b1VerbTenseReviewTopic.CommonMistakes, mistake => mistake.WrongText == "Ich kann gestern kommen.");
        Assert.All(b1VerbTenseReviewTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel b1FormalInformalTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-b1-formal-versus-informal-grammar");
        Assert.Equal(1, b1FormalInformalTopic.ContentRevision);
        Assert.Equal("B1", b1FormalInformalTopic.CefrLevel);
        Assert.Equal("word-order", b1FormalInformalTopic.GrammarCategory);
        Assert.Equal(16, b1FormalInformalTopic.Sections.Count);
        Assert.True(b1FormalInformalTopic.Examples.Count >= 130);
        Assert.True(b1FormalInformalTopic.CommonMistakes.Count >= 50);
        Assert.True(b1FormalInformalTopic.RuleSummaries.Count >= 24);
        Assert.Equal(99, b1FormalInformalTopic.LinkedWords.Count);
        Assert.Contains("a1-formal-sie", b1FormalInformalTopic.PrerequisiteSlugs);
        Assert.Contains("a1-du-versus-sie-grammar-basics", b1FormalInformalTopic.PrerequisiteSlugs);
        Assert.Contains("b1-konjunktiv-ii-for-polite-requests", b1FormalInformalTopic.PrerequisiteSlugs);
        Assert.Contains("b1-formal-email-sentence-structure", b1FormalInformalTopic.PrerequisiteSlugs);
        Assert.Contains("b1-complaint-sentence-patterns", b1FormalInformalTopic.RelatedTopicSlugs);
        Assert.Contains("b1-reported-requests-and-polite-questions", b1FormalInformalTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel b1FormalInformalCoreSection = Assert.Single(b1FormalInformalTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel b1FormalInformalStructureSection = Assert.Single(b1FormalInformalTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel b1FormalInformalWordOrderSection = Assert.Single(b1FormalInformalTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel b1FormalInformalDuSieSection = Assert.Single(b1FormalInformalTopic.Sections, section => section.SectionKey == "du-and-sie");
        ParsedGrammarSectionModel b1FormalInformalPronounSection = Assert.Single(b1FormalInformalTopic.Sections, section => section.SectionKey == "dir-dich-ihnen-sie");
        ParsedGrammarSectionModel b1FormalInformalRequestSection = Assert.Single(b1FormalInformalTopic.Sections, section => section.SectionKey == "direct-vs-polite-requests");
        ParsedGrammarSectionModel b1FormalInformalEmailSection = Assert.Single(b1FormalInformalTopic.Sections, section => section.SectionKey == "formal-email-patterns");
        ParsedGrammarSectionModel b1FormalInformalPhoneSection = Assert.Single(b1FormalInformalTopic.Sections, section => section.SectionKey == "phone-and-office-patterns");
        ParsedGrammarSectionModel b1FormalInformalSofteningSection = Assert.Single(b1FormalInformalTopic.Sections, section => section.SectionKey == "softening-and-distance");
        ParsedGrammarSectionModel b1FormalInformalComparisonSection = Assert.Single(b1FormalInformalTopic.Sections, section => section.SectionKey == "comparison-table");

        Assert.All(languages, language =>
        {
            Assert.True(b1FormalInformalTopic.TitleLocalized.ContainsKey(language));
            Assert.True(b1FormalInformalTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(b1FormalInformalTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(b1FormalInformalTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", b1FormalInformalCoreSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1FormalInformalStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1FormalInformalWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1FormalInformalDuSieSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1FormalInformalPronounSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1FormalInformalRequestSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1FormalInformalEmailSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1FormalInformalPhoneSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1FormalInformalSofteningSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1FormalInformalComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(b1FormalInformalTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(b1FormalInformalTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(b1FormalInformalTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(b1FormalInformalTopic.Examples, example => example.GermanText == "Kannst du mir helfen?");
        Assert.Contains(b1FormalInformalTopic.Examples, example => example.GermanText == "Könnten Sie mir bitte helfen?");
        Assert.Contains(b1FormalInformalTopic.Examples, example => example.GermanText == "Ich schreibe Ihnen, weil ich eine Frage habe.");
        Assert.Contains(b1FormalInformalTopic.Examples, example => example.GermanText == "Ich freue mich auf Ihre Antwort.");
        Assert.Contains(b1FormalInformalTopic.CommonMistakes, mistake => mistake.WrongText == "Kannst Sie mir helfen?");
        Assert.Contains(b1FormalInformalTopic.CommonMistakes, mistake => mistake.WrongText == "Ich helfe Sie.");
        Assert.Contains(b1FormalInformalTopic.CommonMistakes, mistake => mistake.WrongText == "Könnten Sie helfen mir?");
        Assert.Contains(b1FormalInformalTopic.CommonMistakes, mistake => mistake.WrongText == "Geben Sie mir sofort einen Termin.");
        Assert.All(b1FormalInformalTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });

        ParsedGrammarTopicModel b1GrammarReviewMapTopic = Assert.Single(parsedPackage.GrammarTopics, topic => topic.Slug == "b1-b1-grammar-review-map");
        Assert.Equal(1, b1GrammarReviewMapTopic.ContentRevision);
        Assert.Equal("B1", b1GrammarReviewMapTopic.CefrLevel);
        Assert.Equal("word-order", b1GrammarReviewMapTopic.GrammarCategory);
        Assert.Equal(16, b1GrammarReviewMapTopic.Sections.Count);
        Assert.True(b1GrammarReviewMapTopic.Examples.Count >= 130);
        Assert.True(b1GrammarReviewMapTopic.CommonMistakes.Count >= 50);
        Assert.True(b1GrammarReviewMapTopic.RuleSummaries.Count >= 24);
        Assert.Equal(98, b1GrammarReviewMapTopic.LinkedWords.Count);
        Assert.Contains("b1-sentence-order-with-multiple-clauses", b1GrammarReviewMapTopic.PrerequisiteSlugs);
        Assert.Contains("b1-b1-case-review", b1GrammarReviewMapTopic.PrerequisiteSlugs);
        Assert.Contains("b1-b1-connector-review", b1GrammarReviewMapTopic.PrerequisiteSlugs);
        Assert.Contains("b1-b1-verb-tense-review", b1GrammarReviewMapTopic.PrerequisiteSlugs);
        Assert.Contains("b1-b1-mistake-patterns", b1GrammarReviewMapTopic.RelatedTopicSlugs);
        Assert.Contains("b1-grammar-for-b1-writing-exam", b1GrammarReviewMapTopic.RelatedTopicSlugs);

        ParsedGrammarSectionModel b1GrammarReviewCoreSection = Assert.Single(b1GrammarReviewMapTopic.Sections, section => section.SectionKey == "core-patterns");
        ParsedGrammarSectionModel b1GrammarReviewStructureSection = Assert.Single(b1GrammarReviewMapTopic.Sections, section => section.SectionKey == "form-or-structure-table");
        ParsedGrammarSectionModel b1GrammarReviewWordOrderSection = Assert.Single(b1GrammarReviewMapTopic.Sections, section => section.SectionKey == "word-order-or-case-focus");
        ParsedGrammarSectionModel b1GrammarReviewMainSection = Assert.Single(b1GrammarReviewMapTopic.Sections, section => section.SectionKey == "main-clause-map");
        ParsedGrammarSectionModel b1GrammarReviewSubordinateSection = Assert.Single(b1GrammarReviewMapTopic.Sections, section => section.SectionKey == "subordinate-clause-map");
        ParsedGrammarSectionModel b1GrammarReviewCaseSection = Assert.Single(b1GrammarReviewMapTopic.Sections, section => section.SectionKey == "case-map");
        ParsedGrammarSectionModel b1GrammarReviewTenseSection = Assert.Single(b1GrammarReviewMapTopic.Sections, section => section.SectionKey == "tense-map");
        ParsedGrammarSectionModel b1GrammarReviewConnectorSection = Assert.Single(b1GrammarReviewMapTopic.Sections, section => section.SectionKey == "connector-map");
        ParsedGrammarSectionModel b1GrammarReviewLongSentenceSection = Assert.Single(b1GrammarReviewMapTopic.Sections, section => section.SectionKey == "long-sentence-map");
        ParsedGrammarSectionModel b1GrammarReviewFormalSection = Assert.Single(b1GrammarReviewMapTopic.Sections, section => section.SectionKey == "formal-style-map");
        ParsedGrammarSectionModel b1GrammarReviewComparisonSection = Assert.Single(b1GrammarReviewMapTopic.Sections, section => section.SectionKey == "comparison-table");

        Assert.All(languages, language =>
        {
            Assert.True(b1GrammarReviewMapTopic.TitleLocalized.ContainsKey(language));
            Assert.True(b1GrammarReviewMapTopic.ShortDescriptionLocalized.ContainsKey(language));
            Assert.All(b1GrammarReviewMapTopic.Sections, section => Assert.Contains(section.Translations, translation => translation.Language == language));
            Assert.All(b1GrammarReviewMapTopic.Sections, section => Assert.True(section.LocalizedBlocksJson.ContainsKey(language)));
            Assert.Contains("\"table\"", b1GrammarReviewCoreSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1GrammarReviewStructureSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1GrammarReviewWordOrderSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1GrammarReviewMainSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1GrammarReviewSubordinateSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1GrammarReviewCaseSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1GrammarReviewTenseSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1GrammarReviewConnectorSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1GrammarReviewLongSentenceSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1GrammarReviewFormalSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.Contains("\"table\"", b1GrammarReviewComparisonSection.LocalizedBlocksJson[language], StringComparison.Ordinal);
            Assert.All(b1GrammarReviewMapTopic.Examples, example => Assert.Contains(example.Translations, translation => translation.Language == language));
            Assert.All(b1GrammarReviewMapTopic.CommonMistakes, mistake => Assert.Contains(mistake.Translations, translation => translation.Language == language));
            Assert.All(b1GrammarReviewMapTopic.RuleSummaries, rule => Assert.Contains(rule.Translations, translation => translation.Language == language));
        });

        Assert.Contains(b1GrammarReviewMapTopic.Examples, example => example.GermanText == "Ich schreibe Ihnen, weil ich eine Frage habe.");
        Assert.Contains(b1GrammarReviewMapTopic.Examples, example => example.GermanText == "Weil ich krank bin, bleibe ich zu Hause.");
        Assert.Contains(b1GrammarReviewMapTopic.Examples, example => example.GermanText == "Ich sehe den Mann.");
        Assert.Contains(b1GrammarReviewMapTopic.Examples, example => example.GermanText == "Könnten Sie mir bitte helfen?");
        Assert.Contains(b1GrammarReviewMapTopic.CommonMistakes, mistake => mistake.WrongText == "Weil ich krank bin, ich bleibe zu Hause.");
        Assert.Contains(b1GrammarReviewMapTopic.CommonMistakes, mistake => mistake.WrongText == "Ich sehe der Mann.");
        Assert.Contains(b1GrammarReviewMapTopic.CommonMistakes, mistake => mistake.WrongText == "Ich habe nach Hause gegangen.");
        Assert.Contains(b1GrammarReviewMapTopic.CommonMistakes, mistake => mistake.WrongText == "Kannst Sie mir helfen?");
        Assert.All(b1GrammarReviewMapTopic.LinkedWords, word =>
        {
            string serialized = JsonSerializer.Serialize(word);
            Assert.DoesNotContain("meaning", serialized, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static string ResolveRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "DarwinLingua.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root could not be resolved.");
    }
}

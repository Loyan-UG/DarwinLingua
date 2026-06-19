using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.DependencyInjection;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.Localization.Application.DependencyInjection;
using DarwinLingua.Localization.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

/// <summary>
/// Verifies the end-to-end Phase 1 content import workflow against a temporary SQLite database.
/// </summary>
public sealed class ContentImportServiceTests
{
    private const string DefaultDockerContainerName = "darwinlingua-postgres";

    /// <summary>
    /// Verifies that a valid package imports one new word and that the imported word becomes queryable.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldImportValidPackage()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreateValidPackageJson("a1-shopping-import-test"));

            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
            Assert.Equal("Completed", result.Status);
            Assert.Equal(1, result.TotalEntries);
            Assert.Equal(1, result.ImportedEntries);
            Assert.Equal(0, result.SkippedDuplicateEntries);
            Assert.Equal(0, result.InvalidEntries);
            Assert.Equal(["Brot"], result.ImportedLemmas);

            IWordQueryService wordQueryService = serviceProvider.GetRequiredService<IWordQueryService>();
            IReadOnlyList<DarwinLingua.Catalog.Application.Models.WordListItemModel> words = await wordQueryService
                .GetWordsByTopicAsync("shopping", "en", CancellationToken.None);

            DarwinLingua.Catalog.Application.Models.WordListItemModel importedWord = Assert.Single(words);
            Assert.Equal("Brot", importedWord.Lemma);
            Assert.Equal("bread", importedWord.PrimaryMeaning);

            IWordDetailQueryService detailQueryService = serviceProvider.GetRequiredService<IWordDetailQueryService>();
            DarwinLingua.Catalog.Application.Models.WordDetailModel? detail = await detailQueryService
                .GetWordDetailsAsync(importedWord.PublicId, "en", null, "en", CancellationToken.None);

            Assert.NotNull(detail);
            Assert.Contains("informal", detail!.UsageLabels);
            Assert.Contains("shopping", detail.ContextLabels);
            Assert.Contains("Plural form is mostly used when talking about different bread types.", detail.GrammarNotes);
            Assert.Contains(detail.Collocations, collocation => collocation.Text == "frisches Brot kaufen" && collocation.Meaning == "to buy fresh bread");
            Assert.Contains(detail.WordFamilies, member => member.Lemma == "Bäcker" && member.RelationLabel == "Profession");
            Assert.Contains(detail.Synonyms, relation => relation.Lemma == "Laib");
            Assert.Contains(detail.Antonyms, relation => relation.Lemma == "Fasten");
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    /// <summary>
    /// Verifies that the same package identifier cannot be imported twice.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldRejectDuplicatePackageId()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreateValidPackageJson("a1-shopping-import-duplicate-package"));

            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();

            ImportContentPackageResult firstResult = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);
            ImportContentPackageResult secondResult = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(firstResult.IsSuccess);
            Assert.False(secondResult.IsSuccess);
            Assert.Equal("Failed", secondResult.Status);
            Assert.Contains(secondResult.Issues, issue => issue.Message.Contains("already exists", StringComparison.Ordinal));
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    [Fact]
    public async Task ImportAsync_ShouldImportOfficialA2PerfektWithHabenGrammarTopic()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-grammar-a2-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(ResolveRepositoryRoot(), "content", "learning-portal", "grammar", "packages", "grammar-a2-core-v1.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));

            IGrammarTopicQueryService grammarQueryService = serviceProvider.GetRequiredService<IGrammarTopicQueryService>();
            GrammarTopicDetailModel? faDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-perfekt-with-haben",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faDetail);
            Assert.Contains("Perfekt", faDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("A2", faDetail.CefrLevel);
            Assert.Equal("tenses", faDetail.GrammarCategory);
            Assert.Equal(10, faDetail.Sections.Count);
            Assert.True(faDetail.Examples.Count >= 90);
            Assert.True(faDetail.CommonMistakes.Count >= 35);
            Assert.True(faDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a1-haben-in-praesens", faDetail.PrerequisiteSlugs);
            Assert.Contains("a2-perfekt-with-sein", faDetail.RelatedTopicSlugs);
            Assert.Contains(
                faDetail.Sections,
                section => section.SectionKey == "two-part-structure" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDetail.Sections,
                section => section.SectionKey == "haben-conjugation-review" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDetail.Sections,
                section => section.SectionKey == "regular-participles" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDetail.Sections,
                section => section.SectionKey == "sentence-position" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faDetail.Examples, example => example.GermanText == "Wir haben den Termin bestätigt.");
            Assert.Contains(faDetail.CommonMistakes, mistake => mistake.WrongText == "Ich habe gestern lernen.");

            GrammarTopicDetailModel? faSeinDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-perfekt-with-sein",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faSeinDetail);
            Assert.Contains("sein", faSeinDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("A2", faSeinDetail.CefrLevel);
            Assert.Equal("tenses", faSeinDetail.GrammarCategory);
            Assert.Equal(10, faSeinDetail.Sections.Count);
            Assert.True(faSeinDetail.Examples.Count >= 90);
            Assert.True(faSeinDetail.CommonMistakes.Count >= 35);
            Assert.True(faSeinDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-perfekt-with-haben", faSeinDetail.PrerequisiteSlugs);
            Assert.Contains("a2-common-irregular-participles", faSeinDetail.RelatedTopicSlugs);
            Assert.Contains(
                faSeinDetail.Sections,
                section => section.SectionKey == "sein-conjugation-review" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSeinDetail.Sections,
                section => section.SectionKey == "movement-verbs" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSeinDetail.Sections,
                section => section.SectionKey == "sentence-position" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSeinDetail.Sections,
                section => section.SectionKey == "haben-vs-sein-comparison" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSeinDetail.Sections,
                section => section.SectionKey == "common-sein-verbs-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faSeinDetail.Examples, example => example.GermanText == "Das Wetter ist besser geworden.");
            Assert.Contains(faSeinDetail.CommonMistakes, mistake => mistake.WrongText == "Ich bin Deutsch gelernt.");

            GrammarTopicDetailModel? faIrregularDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-common-irregular-participles",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faIrregularDetail);
            Assert.Contains("Partizip", faIrregularDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("A2", faIrregularDetail.CefrLevel);
            Assert.Equal("tenses", faIrregularDetail.GrammarCategory);
            Assert.Equal(10, faIrregularDetail.Sections.Count);
            Assert.True(faIrregularDetail.Examples.Count >= 90);
            Assert.True(faIrregularDetail.CommonMistakes.Count >= 35);
            Assert.True(faIrregularDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-perfekt-with-haben", faIrregularDetail.PrerequisiteSlugs);
            Assert.Contains("a2-perfekt-with-sein", faIrregularDetail.PrerequisiteSlugs);
            Assert.Contains("a2-separable-verbs-in-perfekt", faIrregularDetail.RelatedTopicSlugs);
            Assert.Contains(
                faIrregularDetail.Sections,
                section => section.SectionKey == "regular-vs-irregular-review" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faIrregularDetail.Sections,
                section => section.SectionKey == "common-haben-irregulars" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faIrregularDetail.Sections,
                section => section.SectionKey == "common-sein-irregulars" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faIrregularDetail.Sections,
                section => section.SectionKey == "learn-with-auxiliary" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faIrregularDetail.Sections,
                section => section.SectionKey == "useful-everyday-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faIrregularDetail.Sections,
                section => section.SectionKey == "question-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faIrregularDetail.Examples, example => example.GermanText == "Das Wetter ist besser geworden.");
            Assert.Contains(faIrregularDetail.Examples, example => example.GermanText == "Hast du schon gegessen?");
            Assert.Contains(faIrregularDetail.CommonMistakes, mistake => mistake.WrongText == "Ich habe getrinkt.");
            Assert.Contains(faIrregularDetail.CommonMistakes, mistake => mistake.WrongText == "Ich habe gegangen.");

            GrammarTopicDetailModel? faPraeteritumDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-praeteritum-of-sein-and-haben",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faPraeteritumDetail);
            Assert.Contains("Präteritum", faPraeteritumDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("A2", faPraeteritumDetail.CefrLevel);
            Assert.Equal("tenses", faPraeteritumDetail.GrammarCategory);
            Assert.Equal(10, faPraeteritumDetail.Sections.Count);
            Assert.True(faPraeteritumDetail.Examples.Count >= 90);
            Assert.True(faPraeteritumDetail.CommonMistakes.Count >= 35);
            Assert.True(faPraeteritumDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a1-sein-in-praesens", faPraeteritumDetail.PrerequisiteSlugs);
            Assert.Contains("a1-haben-in-praesens", faPraeteritumDetail.PrerequisiteSlugs);
            Assert.Contains("a2-common-irregular-participles", faPraeteritumDetail.RelatedTopicSlugs);
            Assert.Contains(
                faPraeteritumDetail.Sections,
                section => section.SectionKey == "sein-present-vs-past" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faPraeteritumDetail.Sections,
                section => section.SectionKey == "haben-present-vs-past" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faPraeteritumDetail.Sections,
                section => section.SectionKey == "war-hatte-vs-perfekt" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faPraeteritumDetail.Sections,
                section => section.SectionKey == "questions-with-war-and-hatte" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faPraeteritumDetail.Sections,
                section => section.SectionKey == "negative-sentences" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faPraeteritumDetail.Examples, example => example.GermanText == "Gestern war ich krank.");
            Assert.Contains(faPraeteritumDetail.Examples, example => example.GermanText == "Ich hatte einen Termin.");
            Assert.Contains(faPraeteritumDetail.CommonMistakes, mistake => mistake.WrongText == "Ich bin gestern krank.");
            Assert.Contains(faPraeteritumDetail.CommonMistakes, mistake => mistake.WrongText == "Ich war einen Termin.");

            GrammarTopicDetailModel? faModalVerbsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-modal-verbs-in-more-detail",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faModalVerbsDetail);
            Assert.Contains("Modal", faModalVerbsDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("A2", faModalVerbsDetail.CefrLevel);
            Assert.Equal("modal-verbs", faModalVerbsDetail.GrammarCategory);
            Assert.Equal(11, faModalVerbsDetail.Sections.Count);
            Assert.True(faModalVerbsDetail.Examples.Count >= 90);
            Assert.True(faModalVerbsDetail.CommonMistakes.Count >= 35);
            Assert.True(faModalVerbsDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a1-simple-modal-verbs-koennen-muessen-wollen", faModalVerbsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-polite-requests-with-moechte", faModalVerbsDetail.PrerequisiteSlugs);
            Assert.Contains("a2-polite-forms-with-wuerde", faModalVerbsDetail.RelatedTopicSlugs);
            Assert.Contains("b1-modal-verbs-in-the-past", faModalVerbsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faModalVerbsDetail.Sections,
                section => section.SectionKey == "modal-verbs-overview-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faModalVerbsDetail.Sections,
                section => section.SectionKey == "modal-questions" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faModalVerbsDetail.Sections,
                section => section.SectionKey == "word-order-with-modals" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faModalVerbsDetail.Examples, example => example.GermanText == "Ich kann Deutsch sprechen.");
            Assert.Contains(faModalVerbsDetail.Examples, example => example.GermanText == "Heute muss ich arbeiten.");
            Assert.Contains(faModalVerbsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich kann Deutsch spreche.");
            Assert.Contains(faModalVerbsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich muss zu arbeiten.");

            GrammarTopicDetailModel? faDativeDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-dative-case-basics",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faDativeDetail);
            Assert.Contains("dative", faDativeDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("A2", faDativeDetail.CefrLevel);
            Assert.Equal("dative", faDativeDetail.GrammarCategory);
            Assert.Equal(10, faDativeDetail.Sections.Count);
            Assert.True(faDativeDetail.Examples.Count >= 90);
            Assert.True(faDativeDetail.CommonMistakes.Count >= 35);
            Assert.True(faDativeDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a1-nominative-case", faDativeDetail.PrerequisiteSlugs);
            Assert.Contains("a1-simple-accusative-introduction", faDativeDetail.PrerequisiteSlugs);
            Assert.Contains("a2-accusative-versus-dative-basics", faDativeDetail.RelatedTopicSlugs);
            Assert.Contains("a2-dative-pronouns", faDativeDetail.RelatedTopicSlugs);
            Assert.Contains("a2-prepositions-with-dative", faDativeDetail.RelatedTopicSlugs);
            Assert.Contains(
                faDativeDetail.Sections,
                section => section.SectionKey == "dative-article-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDativeDetail.Sections,
                section => section.SectionKey == "dative-with-giving" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDativeDetail.Sections,
                section => section.SectionKey == "dative-after-common-prepositions" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDativeDetail.Sections,
                section => section.SectionKey == "dative-vs-nominative-preview" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faDativeDetail.Examples, example => example.GermanText == "Ich gebe dem Mann das Buch.");
            Assert.Contains(faDativeDetail.Examples, example => example.GermanText == "Kannst du mir helfen?");
            Assert.Contains(faDativeDetail.CommonMistakes, mistake => mistake.WrongText == "Ich helfe der Mann.");
            Assert.Contains(faDativeDetail.CommonMistakes, mistake => mistake.WrongText == "Ich fahre mit der Bus.");

            GrammarTopicDetailModel? faAccusativeDativeDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-accusative-versus-dative-basics",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faAccusativeDativeDetail);
            Assert.Contains("accusative", faAccusativeDativeDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("A2", faAccusativeDativeDetail.CefrLevel);
            Assert.Equal("cases", faAccusativeDativeDetail.GrammarCategory);
            Assert.Equal(11, faAccusativeDativeDetail.Sections.Count);
            Assert.True(faAccusativeDativeDetail.Examples.Count >= 90);
            Assert.True(faAccusativeDativeDetail.CommonMistakes.Count >= 35);
            Assert.True(faAccusativeDativeDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a1-nominative-case", faAccusativeDativeDetail.PrerequisiteSlugs);
            Assert.Contains("a1-simple-accusative-introduction", faAccusativeDativeDetail.PrerequisiteSlugs);
            Assert.Contains("a2-dative-case-basics", faAccusativeDativeDetail.PrerequisiteSlugs);
            Assert.Contains("a2-dative-pronouns", faAccusativeDativeDetail.RelatedTopicSlugs);
            Assert.Contains("a2-accusative-pronouns", faAccusativeDativeDetail.RelatedTopicSlugs);
            Assert.Contains("a2-possessive-pronouns-in-cases", faAccusativeDativeDetail.RelatedTopicSlugs);
            Assert.Contains(
                faAccusativeDativeDetail.Sections,
                section => section.SectionKey == "helper-questions-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faAccusativeDativeDetail.Sections,
                section => section.SectionKey == "article-comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faAccusativeDativeDetail.Sections,
                section => section.SectionKey == "sentence-with-both-cases" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faAccusativeDativeDetail.Sections,
                section => section.SectionKey == "practice-advice" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faAccusativeDativeDetail.Examples, example => example.GermanText == "Ich gebe dem Mann den Schlüssel.");
            Assert.Contains(faAccusativeDativeDetail.CommonMistakes, mistake => mistake.WrongText == "Ich gebe den Mann den Schlüssel.");
            Assert.Contains(faAccusativeDativeDetail.CommonMistakes, mistake => mistake.WrongText == "Ich fahre mit den Bus.");

            GrammarTopicDetailModel? faDativePronounsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-dative-pronouns",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faDativePronounsDetail);
            Assert.Contains("dative", faDativePronounsDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("A2", faDativePronounsDetail.CefrLevel);
            Assert.Equal("pronouns", faDativePronounsDetail.GrammarCategory);
            Assert.Equal(12, faDativePronounsDetail.Sections.Count);
            Assert.True(faDativePronounsDetail.Examples.Count >= 90);
            Assert.True(faDativePronounsDetail.CommonMistakes.Count >= 35);
            Assert.True(faDativePronounsDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-dative-case-basics", faDativePronounsDetail.PrerequisiteSlugs);
            Assert.Contains("a2-accusative-versus-dative-basics", faDativePronounsDetail.PrerequisiteSlugs);
            Assert.Contains("a2-accusative-pronouns", faDativePronounsDetail.RelatedTopicSlugs);
            Assert.Contains("a2-possessive-pronouns-in-cases", faDativePronounsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faDativePronounsDetail.Sections,
                section => section.SectionKey == "dative-pronoun-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDativePronounsDetail.Sections,
                section => section.SectionKey == "formal-ihnen" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDativePronounsDetail.Sections,
                section => section.SectionKey == "common-verbs-with-dative-pronouns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faDativePronounsDetail.Examples, example => example.GermanText == "Kann ich Ihnen helfen?");
            Assert.Contains(faDativePronounsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich danke Sie.");
            Assert.Contains(faDativePronounsDetail.CommonMistakes, mistake => mistake.WrongText == "Kann ich Sie helfen?");

            GrammarTopicDetailModel? faAccusativePronounsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-accusative-pronouns",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faAccusativePronounsDetail);
            Assert.Contains("accusative", faAccusativePronounsDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("A2", faAccusativePronounsDetail.CefrLevel);
            Assert.Equal("pronouns", faAccusativePronounsDetail.GrammarCategory);
            Assert.Equal(13, faAccusativePronounsDetail.Sections.Count);
            Assert.True(faAccusativePronounsDetail.Examples.Count >= 90);
            Assert.True(faAccusativePronounsDetail.CommonMistakes.Count >= 35);
            Assert.True(faAccusativePronounsDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a1-simple-accusative-introduction", faAccusativePronounsDetail.PrerequisiteSlugs);
            Assert.Contains("a2-accusative-versus-dative-basics", faAccusativePronounsDetail.PrerequisiteSlugs);
            Assert.Contains("a2-dative-pronouns", faAccusativePronounsDetail.RelatedTopicSlugs);
            Assert.Contains("a2-possessive-pronouns-in-cases", faAccusativePronounsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faAccusativePronounsDetail.Sections,
                section => section.SectionKey == "accusative-pronoun-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faAccusativePronounsDetail.Sections,
                section => section.SectionKey == "replacing-accusative-nouns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faAccusativePronounsDetail.Sections,
                section => section.SectionKey == "accusative-vs-dative-pronouns-preview" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faAccusativePronounsDetail.Examples, example => example.GermanText == "Ich rufe Sie morgen an.");
            Assert.Contains(faAccusativePronounsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich rufe Ihnen an.");
            Assert.Contains(faAccusativePronounsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich sehe du.");

            GrammarTopicDetailModel? faPossessiveCasesDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-possessive-pronouns-in-cases",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faPossessiveCasesDetail);
            Assert.Contains("حالت", faPossessiveCasesDetail!.Title, StringComparison.Ordinal);
            Assert.Equal("A2", faPossessiveCasesDetail.CefrLevel);
            Assert.Equal("pronouns", faPossessiveCasesDetail.GrammarCategory);
            Assert.Equal(12, faPossessiveCasesDetail.Sections.Count);
            Assert.True(faPossessiveCasesDetail.Examples.Count >= 100);
            Assert.True(faPossessiveCasesDetail.CommonMistakes.Count >= 35);
            Assert.True(faPossessiveCasesDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a1-possessive-pronouns-mein-dein", faPossessiveCasesDetail.PrerequisiteSlugs);
            Assert.Contains("a2-dative-case-basics", faPossessiveCasesDetail.PrerequisiteSlugs);
            Assert.Contains("a2-accusative-pronouns", faPossessiveCasesDetail.PrerequisiteSlugs);
            Assert.Contains("b1-b1-case-review", faPossessiveCasesDetail.RelatedTopicSlugs);
            Assert.Contains(
                faPossessiveCasesDetail.Sections,
                section => section.SectionKey == "nominative-forms" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faPossessiveCasesDetail.Sections,
                section => section.SectionKey == "accusative-forms" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faPossessiveCasesDetail.Sections,
                section => section.SectionKey == "dative-forms" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faPossessiveCasesDetail.Sections,
                section => section.SectionKey == "case-check-strategy" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faPossessiveCasesDetail.Examples, example => example.GermanText == "Ich helfe deinem Vater.");
            Assert.Contains(faPossessiveCasesDetail.CommonMistakes, mistake => mistake.WrongText == "Ich helfe dein Vater.");

            GrammarTopicDetailModel? faWechselDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-wechselpraepositionen-introduction",
                "fa",
            CancellationToken.None);

            Assert.NotNull(faWechselDetail);
            Assert.Contains("حرف اضافه", faWechselDetail!.Title, StringComparison.Ordinal);
            Assert.Equal("A2", faWechselDetail.CefrLevel);
            Assert.Equal("prepositions", faWechselDetail.GrammarCategory);
            Assert.Equal(11, faWechselDetail.Sections.Count);
            Assert.True(faWechselDetail.Examples.Count >= 90);
            Assert.True(faWechselDetail.CommonMistakes.Count >= 35);
            Assert.True(faWechselDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-dative-case-basics", faWechselDetail.PrerequisiteSlugs);
            Assert.Contains("a2-accusative-versus-dative-basics", faWechselDetail.PrerequisiteSlugs);
            Assert.Contains("a2-prepositions-with-dative", faWechselDetail.RelatedTopicSlugs);
            Assert.Contains("a2-prepositions-with-accusative", faWechselDetail.RelatedTopicSlugs);
            Assert.Contains(
                faWechselDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faWechselDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faWechselDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faWechselDetail.Sections,
                section => section.SectionKey == "question-forms" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faWechselDetail.Examples, example => example.GermanText == "Ich bin in der Schule.");
            Assert.Contains(faWechselDetail.Examples, example => example.GermanText == "Ich gehe in die Schule.");
            Assert.Contains(faWechselDetail.CommonMistakes, mistake => mistake.WrongText == "Ich bin in die Schule.");

            GrammarTopicDetailModel? faDativePrepositionsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-prepositions-with-dative",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faDativePrepositionsDetail);
            Assert.Contains("حرف اضافه", faDativePrepositionsDetail!.Title, StringComparison.Ordinal);
            Assert.Equal("A2", faDativePrepositionsDetail.CefrLevel);
            Assert.Equal("prepositions", faDativePrepositionsDetail.GrammarCategory);
            Assert.Equal(11, faDativePrepositionsDetail.Sections.Count);
            Assert.True(faDativePrepositionsDetail.Examples.Count >= 90);
            Assert.True(faDativePrepositionsDetail.CommonMistakes.Count >= 35);
            Assert.True(faDativePrepositionsDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-dative-case-basics", faDativePrepositionsDetail.PrerequisiteSlugs);
            Assert.Contains("a2-dative-pronouns", faDativePrepositionsDetail.PrerequisiteSlugs);
            Assert.Contains("a2-prepositions-with-accusative", faDativePrepositionsDetail.RelatedTopicSlugs);
            Assert.Contains("a2-wechselpraepositionen-introduction", faDativePrepositionsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faDativePrepositionsDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDativePrepositionsDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDativePrepositionsDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDativePrepositionsDetail.Sections,
                section => section.SectionKey == "question-forms" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faDativePrepositionsDetail.Examples, example => example.GermanText == "Ich fahre mit dem Bus.");
            Assert.Contains(faDativePrepositionsDetail.Examples, example => example.GermanText == "Ich gehe zum Arzt.");
            Assert.Contains(faDativePrepositionsDetail.CommonMistakes, mistake => mistake.WrongText == "mit der Bus");

            GrammarTopicDetailModel? faAccusativePrepositionsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-prepositions-with-accusative",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faAccusativePrepositionsDetail);
            Assert.Contains("حرف اضافه", faAccusativePrepositionsDetail!.Title, StringComparison.Ordinal);
            Assert.Equal("A2", faAccusativePrepositionsDetail.CefrLevel);
            Assert.Equal("prepositions", faAccusativePrepositionsDetail.GrammarCategory);
            Assert.Equal(11, faAccusativePrepositionsDetail.Sections.Count);
            Assert.True(faAccusativePrepositionsDetail.Examples.Count >= 90);
            Assert.True(faAccusativePrepositionsDetail.CommonMistakes.Count >= 35);
            Assert.True(faAccusativePrepositionsDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a1-simple-accusative-introduction", faAccusativePrepositionsDetail.PrerequisiteSlugs);
            Assert.Contains("a2-accusative-pronouns", faAccusativePrepositionsDetail.PrerequisiteSlugs);
            Assert.Contains("a2-prepositions-with-dative", faAccusativePrepositionsDetail.RelatedTopicSlugs);
            Assert.Contains("a2-wechselpraepositionen-introduction", faAccusativePrepositionsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faAccusativePrepositionsDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faAccusativePrepositionsDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faAccusativePrepositionsDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faAccusativePrepositionsDetail.Sections,
                section => section.SectionKey == "question-forms" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faAccusativePrepositionsDetail.Examples, example => example.GermanText == "Das ist für dich.");
            Assert.Contains(faAccusativePrepositionsDetail.Examples, example => example.GermanText == "Ich gehe durch den Park.");
            Assert.Contains(faAccusativePrepositionsDetail.CommonMistakes, mistake => mistake.WrongText == "für der Mann");

            GrammarTopicDetailModel? faSeparablePerfektDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-separable-verbs-in-perfekt",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faSeparablePerfektDetail);
            Assert.Contains("Perfekt", faSeparablePerfektDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("A2", faSeparablePerfektDetail.CefrLevel);
            Assert.Equal("separable-verbs", faSeparablePerfektDetail.GrammarCategory);
            Assert.Equal(11, faSeparablePerfektDetail.Sections.Count);
            Assert.True(faSeparablePerfektDetail.Examples.Count >= 90);
            Assert.True(faSeparablePerfektDetail.CommonMistakes.Count >= 35);
            Assert.True(faSeparablePerfektDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a1-separable-verbs-introduction", faSeparablePerfektDetail.PrerequisiteSlugs);
            Assert.Contains("a2-perfekt-with-haben", faSeparablePerfektDetail.PrerequisiteSlugs);
            Assert.Contains("a2-perfekt-with-sein", faSeparablePerfektDetail.PrerequisiteSlugs);
            Assert.Contains("b1-describing-experiences-in-the-past", faSeparablePerfektDetail.RelatedTopicSlugs);
            Assert.Contains(
                faSeparablePerfektDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSeparablePerfektDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSeparablePerfektDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSeparablePerfektDetail.Sections,
                section => section.SectionKey == "question-forms" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faSeparablePerfektDetail.Examples, example => example.GermanText == "Ich habe gestern eingekauft.");
            Assert.Contains(faSeparablePerfektDetail.Examples, example => example.GermanText == "Ich bin früh aufgestanden.");
            Assert.Contains(faSeparablePerfektDetail.CommonMistakes, mistake => mistake.WrongText == "Ich habe geeinkauft.");

            GrammarTopicDetailModel? faReflexiveVerbsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-reflexive-verbs-introduction",
                "fa",
            CancellationToken.None);

            Assert.NotNull(faReflexiveVerbsDetail);
            Assert.Contains("بازتابی", faReflexiveVerbsDetail!.Title, StringComparison.Ordinal);
            Assert.Equal("A2", faReflexiveVerbsDetail.CefrLevel);
            Assert.Equal("reflexive-verbs", faReflexiveVerbsDetail.GrammarCategory);
            Assert.Equal(11, faReflexiveVerbsDetail.Sections.Count);
            Assert.True(faReflexiveVerbsDetail.Examples.Count >= 90);
            Assert.True(faReflexiveVerbsDetail.CommonMistakes.Count >= 35);
            Assert.True(faReflexiveVerbsDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-accusative-pronouns", faReflexiveVerbsDetail.PrerequisiteSlugs);
            Assert.Contains("a2-separable-verbs-in-perfekt", faReflexiveVerbsDetail.PrerequisiteSlugs);
            Assert.Contains("b1-reflexive-verbs-with-dative-and-accusative", faReflexiveVerbsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faReflexiveVerbsDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faReflexiveVerbsDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faReflexiveVerbsDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faReflexiveVerbsDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faReflexiveVerbsDetail.Sections,
                section => section.SectionKey == "question-forms" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faReflexiveVerbsDetail.Examples, example => example.GermanText == "Ich wasche mich.");
            Assert.Contains(faReflexiveVerbsDetail.Examples, example => example.GermanText == "Wir treffen uns morgen.");
            Assert.Contains(faReflexiveVerbsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich wasche sich.");

            GrammarTopicDetailModel? faDassClausesDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-dass-clauses",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faDassClausesDetail);
            Assert.Contains("dass", faDassClausesDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("A2", faDassClausesDetail.CefrLevel);
            Assert.Equal("subordinate-clauses", faDassClausesDetail.GrammarCategory);
            Assert.Equal(10, faDassClausesDetail.Sections.Count);
            Assert.True(faDassClausesDetail.Examples.Count >= 90);
            Assert.True(faDassClausesDetail.CommonMistakes.Count >= 35);
            Assert.True(faDassClausesDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a1-verb-position-in-simple-sentences", faDassClausesDetail.PrerequisiteSlugs);
            Assert.Contains("a2-modal-verbs-in-more-detail", faDassClausesDetail.PrerequisiteSlugs);
            Assert.Contains("a2-weil-clauses", faDassClausesDetail.RelatedTopicSlugs);
            Assert.Contains("a2-sentence-order-in-subordinate-clauses", faDassClausesDetail.RelatedTopicSlugs);
            Assert.Contains(
                faDassClausesDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDassClausesDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDassClausesDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDassClausesDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDassClausesDetail.Sections,
                section => section.SectionKey == "practice-advice" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faDassClausesDetail.Examples, example => example.GermanText == "Ich glaube, dass er heute kommt.");
            Assert.Contains(faDassClausesDetail.Examples, example => example.GermanText == "Sie sagt, dass sie keine Zeit hat.");
            Assert.Contains(faDassClausesDetail.CommonMistakes, mistake => mistake.WrongText == "Ich glaube, dass er kommt heute.");

            GrammarTopicDetailModel? faWeilClausesDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-weil-clauses",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faWeilClausesDetail);
            Assert.Contains("weil", faWeilClausesDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("A2", faWeilClausesDetail.CefrLevel);
            Assert.Equal("subordinate-clauses", faWeilClausesDetail.GrammarCategory);
            Assert.Equal(10, faWeilClausesDetail.Sections.Count);
            Assert.True(faWeilClausesDetail.Examples.Count >= 95);
            Assert.True(faWeilClausesDetail.CommonMistakes.Count >= 35);
            Assert.True(faWeilClausesDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-dass-clauses", faWeilClausesDetail.PrerequisiteSlugs);
            Assert.Contains("a2-modal-verbs-in-more-detail", faWeilClausesDetail.PrerequisiteSlugs);
            Assert.Contains("a2-denn-versus-weil", faWeilClausesDetail.RelatedTopicSlugs);
            Assert.Contains("a2-sentence-order-in-subordinate-clauses", faWeilClausesDetail.RelatedTopicSlugs);
            Assert.Contains(
                faWeilClausesDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faWeilClausesDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faWeilClausesDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faWeilClausesDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faWeilClausesDetail.Examples, example => example.GermanText == "Ich komme nicht, weil ich krank bin.");
            Assert.Contains(faWeilClausesDetail.Examples, example => example.GermanText == "Sie lernt Deutsch, weil sie in Deutschland wohnt.");
            Assert.Contains(faWeilClausesDetail.CommonMistakes, mistake => mistake.WrongText == "Ich komme nicht, weil ich bin krank.");

            GrammarTopicDetailModel? faWennDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-wenn-for-conditions",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faWennDetail);
            Assert.Contains("wenn", faWennDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("A2", faWennDetail.CefrLevel);
            Assert.Equal("subordinate-clauses", faWennDetail.GrammarCategory);
            Assert.Equal(10, faWennDetail.Sections.Count);
            Assert.True(faWennDetail.Examples.Count >= 95);
            Assert.True(faWennDetail.CommonMistakes.Count >= 35);
            Assert.True(faWennDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-dass-clauses", faWennDetail.PrerequisiteSlugs);
            Assert.Contains("a2-weil-clauses", faWennDetail.PrerequisiteSlugs);
            Assert.Contains("a2-sentence-order-in-subordinate-clauses", faWennDetail.RelatedTopicSlugs);
            Assert.Contains("b1-als-versus-wenn", faWennDetail.RelatedTopicSlugs);
            Assert.Contains(
                faWennDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faWennDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faWennDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faWennDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faWennDetail.Examples, example => example.GermanText == "Wenn ich Zeit habe, komme ich.");
            Assert.Contains(faWennDetail.Examples, example => example.GermanText == "Ich komme, wenn ich Zeit habe.");
            Assert.Contains(faWennDetail.CommonMistakes, mistake => mistake.WrongText == "Wenn ich habe Zeit, komme ich.");

            GrammarTopicDetailModel? faDennWeilDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-denn-versus-weil",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faDennWeilDetail);
            Assert.Contains("denn", faDennWeilDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("A2", faDennWeilDetail.CefrLevel);
            Assert.Equal("connectors", faDennWeilDetail.GrammarCategory);
            Assert.Equal(10, faDennWeilDetail.Sections.Count);
            Assert.True(faDennWeilDetail.Examples.Count >= 90);
            Assert.True(faDennWeilDetail.CommonMistakes.Count >= 35);
            Assert.True(faDennWeilDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-weil-clauses", faDennWeilDetail.PrerequisiteSlugs);
            Assert.Contains("a2-dass-clauses", faDennWeilDetail.PrerequisiteSlugs);
            Assert.Contains("a2-sentence-order-in-subordinate-clauses", faDennWeilDetail.RelatedTopicSlugs);
            Assert.Contains("b1-connectors-for-cause-and-effect", faDennWeilDetail.RelatedTopicSlugs);
            Assert.Contains(
                faDennWeilDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDennWeilDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDennWeilDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDennWeilDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faDennWeilDetail.Examples, example => example.GermanText == "Ich bleibe zu Hause, weil ich krank bin.");
            Assert.Contains(faDennWeilDetail.Examples, example => example.GermanText == "Ich bleibe zu Hause, denn ich bin krank.");
            Assert.Contains(faDennWeilDetail.CommonMistakes, mistake => mistake.WrongText == "Ich komme nicht, denn ich krank bin.");

            GrammarTopicDetailModel? faSubordinateOrderDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-sentence-order-in-subordinate-clauses",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faSubordinateOrderDetail);
            Assert.Equal("a2-sentence-order-in-subordinate-clauses", faSubordinateOrderDetail!.Slug);
            Assert.Equal("A2", faSubordinateOrderDetail.CefrLevel);
            Assert.Equal("word-order", faSubordinateOrderDetail.GrammarCategory);
            Assert.Equal(10, faSubordinateOrderDetail.Sections.Count);
            Assert.True(faSubordinateOrderDetail.Examples.Count >= 110);
            Assert.True(faSubordinateOrderDetail.CommonMistakes.Count >= 40);
            Assert.True(faSubordinateOrderDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-dass-clauses", faSubordinateOrderDetail.PrerequisiteSlugs);
            Assert.Contains("a2-weil-clauses", faSubordinateOrderDetail.PrerequisiteSlugs);
            Assert.Contains("a2-wenn-for-conditions", faSubordinateOrderDetail.PrerequisiteSlugs);
            Assert.Contains("a2-denn-versus-weil", faSubordinateOrderDetail.PrerequisiteSlugs);
            Assert.Contains("b1-sentence-order-with-multiple-clauses", faSubordinateOrderDetail.RelatedTopicSlugs);
            Assert.Contains(
                faSubordinateOrderDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSubordinateOrderDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSubordinateOrderDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSubordinateOrderDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSubordinateOrderDetail.Sections,
                section => section.SectionKey == "practice-advice" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faSubordinateOrderDetail.Examples, example => example.GermanText == "Ich glaube, dass er morgen kommt.");
            Assert.Contains(faSubordinateOrderDetail.Examples, example => example.GermanText == "Wenn ich Zeit habe, komme ich.");
            Assert.Contains(faSubordinateOrderDetail.CommonMistakes, mistake => mistake.WrongText == "Ich glaube, dass er kommt morgen.");

            GrammarTopicDetailModel? faComparativeDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-comparative-forms",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faComparativeDetail);
            Assert.Equal("a2-comparative-forms", faComparativeDetail!.Slug);
            Assert.Equal("A2", faComparativeDetail.CefrLevel);
            Assert.Equal("adjective-declension", faComparativeDetail.GrammarCategory);
            Assert.Equal(10, faComparativeDetail.Sections.Count);
            Assert.True(faComparativeDetail.Examples.Count >= 90);
            Assert.True(faComparativeDetail.CommonMistakes.Count >= 35);
            Assert.True(faComparativeDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a1-basic-adjective-position", faComparativeDetail.PrerequisiteSlugs);
            Assert.Contains("a2-superlative-basics", faComparativeDetail.RelatedTopicSlugs);
            Assert.Contains("a2-adjective-endings-introduction", faComparativeDetail.RelatedTopicSlugs);
            Assert.Contains(
                faComparativeDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faComparativeDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faComparativeDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faComparativeDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faComparativeDetail.Examples, example => example.GermanText == "Berlin ist größer als Bonn.");
            Assert.Contains(faComparativeDetail.Examples, example => example.GermanText == "Ich brauche mehr Zeit.");
            Assert.Contains(faComparativeDetail.CommonMistakes, mistake => mistake.WrongText == "Berlin ist groß als Bonn.");

            GrammarTopicDetailModel? faSuperlativeDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-superlative-basics",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faSuperlativeDetail);
            Assert.Equal("a2-superlative-basics", faSuperlativeDetail!.Slug);
            Assert.Equal("A2", faSuperlativeDetail.CefrLevel);
            Assert.Equal("adjective-declension", faSuperlativeDetail.GrammarCategory);
            Assert.Equal(10, faSuperlativeDetail.Sections.Count);
            Assert.True(faSuperlativeDetail.Examples.Count >= 90);
            Assert.True(faSuperlativeDetail.CommonMistakes.Count >= 35);
            Assert.True(faSuperlativeDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-comparative-forms", faSuperlativeDetail.PrerequisiteSlugs);
            Assert.Contains("a2-adjective-endings-introduction", faSuperlativeDetail.RelatedTopicSlugs);
            Assert.Contains(
                faSuperlativeDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSuperlativeDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSuperlativeDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSuperlativeDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faSuperlativeDetail.Examples, example => example.GermanText == "Dieses Zimmer ist am größten.");
            Assert.Contains(faSuperlativeDetail.Examples, example => example.GermanText == "Der Kurs ist am besten.");
            Assert.Contains(faSuperlativeDetail.CommonMistakes, mistake => mistake.WrongText == "am gutesten");

            GrammarTopicDetailModel? faAdjectiveEndingsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-adjective-endings-introduction",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faAdjectiveEndingsDetail);
            Assert.Equal("a2-adjective-endings-introduction", faAdjectiveEndingsDetail!.Slug);
            Assert.Equal("A2", faAdjectiveEndingsDetail.CefrLevel);
            Assert.Equal("adjective-declension", faAdjectiveEndingsDetail.GrammarCategory);
            Assert.Equal(10, faAdjectiveEndingsDetail.Sections.Count);
            Assert.True(faAdjectiveEndingsDetail.Examples.Count >= 90);
            Assert.True(faAdjectiveEndingsDetail.CommonMistakes.Count >= 35);
            Assert.True(faAdjectiveEndingsDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-superlative-basics", faAdjectiveEndingsDetail.PrerequisiteSlugs);
            Assert.Contains("b1-adjective-declension-after-definite-article", faAdjectiveEndingsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faAdjectiveEndingsDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faAdjectiveEndingsDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faAdjectiveEndingsDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faAdjectiveEndingsDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faAdjectiveEndingsDetail.Examples, example => example.GermanText == "Das Zimmer ist klein.");
            Assert.Contains(faAdjectiveEndingsDetail.Examples, example => example.GermanText == "Ich möchte einen heißen Kaffee.");
            Assert.Contains(faAdjectiveEndingsDetail.CommonMistakes, mistake => mistake.WrongText == "Das Zimmer ist kleines.");

            GrammarTopicDetailModel? faIndirectQuestionsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-indirect-questions-introduction",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faIndirectQuestionsDetail);
            Assert.Equal("a2-indirect-questions-introduction", faIndirectQuestionsDetail!.Slug);
            Assert.Equal("A2", faIndirectQuestionsDetail.CefrLevel);
            Assert.Equal("questions", faIndirectQuestionsDetail.GrammarCategory);
            Assert.Equal(10, faIndirectQuestionsDetail.Sections.Count);
            Assert.True(faIndirectQuestionsDetail.Examples.Count >= 90);
            Assert.True(faIndirectQuestionsDetail.CommonMistakes.Count >= 35);
            Assert.True(faIndirectQuestionsDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-sentence-order-in-subordinate-clauses", faIndirectQuestionsDetail.PrerequisiteSlugs);
            Assert.Contains("b1-indirect-questions", faIndirectQuestionsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faIndirectQuestionsDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faIndirectQuestionsDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faIndirectQuestionsDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faIndirectQuestionsDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faIndirectQuestionsDetail.Examples, example => example.GermanText == "Können Sie mir sagen, wo der Bahnhof ist?");
            Assert.Contains(faIndirectQuestionsDetail.Examples, example => example.GermanText == "Wissen Sie, ob der Bus kommt?");
            Assert.Contains(faIndirectQuestionsDetail.CommonMistakes, mistake => mistake.WrongText == "Können Sie mir sagen, wo ist der Bahnhof?");

            GrammarTopicDetailModel? faA2ImperativeDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-imperative-formal-and-informal",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faA2ImperativeDetail);
            Assert.Equal("a2-imperative-formal-and-informal", faA2ImperativeDetail!.Slug);
            Assert.Equal("A2", faA2ImperativeDetail.CefrLevel);
            Assert.Equal("imperative", faA2ImperativeDetail.GrammarCategory);
            Assert.Equal(10, faA2ImperativeDetail.Sections.Count);
            Assert.True(faA2ImperativeDetail.Examples.Count >= 90);
            Assert.True(faA2ImperativeDetail.CommonMistakes.Count >= 35);
            Assert.True(faA2ImperativeDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a1-imperative-basics", faA2ImperativeDetail.PrerequisiteSlugs);
            Assert.Contains("a2-indirect-questions-introduction", faA2ImperativeDetail.RelatedTopicSlugs);
            Assert.Contains(
                faA2ImperativeDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faA2ImperativeDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faA2ImperativeDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faA2ImperativeDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faA2ImperativeDetail.Examples, example => example.GermanText == "Kommen Sie bitte.");
            Assert.Contains(faA2ImperativeDetail.Examples, example => example.GermanText == "Ruf mich bitte an.");
            Assert.Contains(faA2ImperativeDetail.CommonMistakes, mistake => mistake.WrongText == "Komm Sie bitte.");

            GrammarTopicDetailModel? faTimeClausesDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-time-clauses-bevor-and-nachdem-introduction",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faTimeClausesDetail);
            Assert.Equal("a2-time-clauses-bevor-and-nachdem-introduction", faTimeClausesDetail!.Slug);
            Assert.Equal("A2", faTimeClausesDetail.CefrLevel);
            Assert.Equal("subordinate-clauses", faTimeClausesDetail.GrammarCategory);
            Assert.Equal(10, faTimeClausesDetail.Sections.Count);
            Assert.True(faTimeClausesDetail.Examples.Count >= 90);
            Assert.True(faTimeClausesDetail.CommonMistakes.Count >= 35);
            Assert.True(faTimeClausesDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-sentence-order-in-subordinate-clauses", faTimeClausesDetail.PrerequisiteSlugs);
            Assert.Contains("b1-nachdem-bevor-waehrend", faTimeClausesDetail.RelatedTopicSlugs);
            Assert.Contains(
                faTimeClausesDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faTimeClausesDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faTimeClausesDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faTimeClausesDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faTimeClausesDetail.Examples, example => example.GermanText == "Ich frühstücke, bevor ich zur Arbeit gehe.");
            Assert.Contains(faTimeClausesDetail.Examples, example => example.GermanText == "Nachdem ich gegessen habe, lerne ich Deutsch.");
            Assert.Contains(faTimeClausesDetail.CommonMistakes, mistake => mistake.WrongText == "Ich frühstücke, bevor ich gehe zur Arbeit.");

            GrammarTopicDetailModel? faZuInfinitiveDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-zu-plus-infinitive-introduction",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faZuInfinitiveDetail);
            Assert.Equal("a2-zu-plus-infinitive-introduction", faZuInfinitiveDetail!.Slug);
            Assert.Equal("A2", faZuInfinitiveDetail.CefrLevel);
            Assert.Equal("verbs", faZuInfinitiveDetail.GrammarCategory);
            Assert.Equal(10, faZuInfinitiveDetail.Sections.Count);
            Assert.True(faZuInfinitiveDetail.Examples.Count >= 90);
            Assert.True(faZuInfinitiveDetail.CommonMistakes.Count >= 35);
            Assert.True(faZuInfinitiveDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-sentence-order-in-subordinate-clauses", faZuInfinitiveDetail.PrerequisiteSlugs);
            Assert.Contains("b1-infinitive-with-zu", faZuInfinitiveDetail.RelatedTopicSlugs);
            Assert.Contains(
                faZuInfinitiveDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faZuInfinitiveDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faZuInfinitiveDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faZuInfinitiveDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faZuInfinitiveDetail.Examples, example => example.GermanText == "Ich versuche, Deutsch zu lernen.");
            Assert.Contains(faZuInfinitiveDetail.Examples, example => example.GermanText == "Ich muss arbeiten.");
            Assert.Contains(faZuInfinitiveDetail.CommonMistakes, mistake => mistake.WrongText == "Ich muss zu arbeiten.");

            GrammarTopicDetailModel? faManGeneralSubjectDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-man-as-general-subject",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faManGeneralSubjectDetail);
            Assert.Equal("a2-man-as-general-subject", faManGeneralSubjectDetail!.Slug);
            Assert.Equal("A2", faManGeneralSubjectDetail.CefrLevel);
            Assert.Equal("pronouns", faManGeneralSubjectDetail.GrammarCategory);
            Assert.Equal(10, faManGeneralSubjectDetail.Sections.Count);
            Assert.True(faManGeneralSubjectDetail.Examples.Count >= 90);
            Assert.True(faManGeneralSubjectDetail.CommonMistakes.Count >= 35);
            Assert.True(faManGeneralSubjectDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-modal-verbs-in-more-detail", faManGeneralSubjectDetail.PrerequisiteSlugs);
            Assert.Contains("b2-passive-with-modal-verbs", faManGeneralSubjectDetail.RelatedTopicSlugs);
            Assert.Contains(
                faManGeneralSubjectDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faManGeneralSubjectDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faManGeneralSubjectDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faManGeneralSubjectDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faManGeneralSubjectDetail.Examples, example => example.GermanText == "Man sagt in Deutschland oft Hallo.");
            Assert.Contains(faManGeneralSubjectDetail.Examples, example => example.GermanText == "Man kann hier parken.");
            Assert.Contains(faManGeneralSubjectDetail.CommonMistakes, mistake => mistake.WrongText == "Man bin müde.");

            GrammarTopicDetailModel? faEsGibtDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-es-gibt",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faEsGibtDetail);
            Assert.Equal("a2-es-gibt", faEsGibtDetail!.Slug);
            Assert.Equal("A2", faEsGibtDetail.CefrLevel);
            Assert.Equal("verbs", faEsGibtDetail.GrammarCategory);
            Assert.Equal(10, faEsGibtDetail.Sections.Count);
            Assert.True(faEsGibtDetail.Examples.Count >= 90);
            Assert.True(faEsGibtDetail.CommonMistakes.Count >= 35);
            Assert.True(faEsGibtDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a1-simple-accusative-introduction", faEsGibtDetail.PrerequisiteSlugs);
            Assert.Contains("a2-grammar-for-appointments", faEsGibtDetail.RelatedTopicSlugs);
            Assert.Contains(
                faEsGibtDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faEsGibtDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faEsGibtDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faEsGibtDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faEsGibtDetail.Examples, example => example.GermanText == "Es gibt einen Park.");
            Assert.Contains(faEsGibtDetail.Examples, example => example.GermanText == "Gibt es hier einen Arzt?");
            Assert.Contains(faEsGibtDetail.CommonMistakes, mistake => mistake.WrongText == "Es gibt ein Park.");

            GrammarTopicDetailModel? faWuerdeDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-polite-forms-with-wuerde",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faWuerdeDetail);
            Assert.Equal("a2-polite-forms-with-wuerde", faWuerdeDetail!.Slug);
            Assert.Equal("A2", faWuerdeDetail.CefrLevel);
            Assert.Equal("konjunktiv", faWuerdeDetail.GrammarCategory);
            Assert.Equal(10, faWuerdeDetail.Sections.Count);
            Assert.True(faWuerdeDetail.Examples.Count >= 90);
            Assert.True(faWuerdeDetail.CommonMistakes.Count >= 35);
            Assert.True(faWuerdeDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-indirect-questions-introduction", faWuerdeDetail.PrerequisiteSlugs);
            Assert.Contains("b1-konjunktiv-ii-for-polite-requests", faWuerdeDetail.RelatedTopicSlugs);
            Assert.Contains(
                faWuerdeDetail.Sections,
                section => section.SectionKey == "core-patterns" && section.Blocks.Any(block => block.Type == "rule-list"));
            Assert.Contains(
                faWuerdeDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faWuerdeDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faWuerdeDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faWuerdeDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faWuerdeDetail.Sections,
                section => section.SectionKey == "practice-advice" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faWuerdeDetail.Examples, example => example.GermanText == "Ich würde gern kommen.");
            Assert.Contains(faWuerdeDetail.Examples, example => example.GermanText == "Würden Sie bitte warten?");
            Assert.Contains(faWuerdeDetail.CommonMistakes, mistake => mistake.WrongText == "Ich würde gern komme.");

            GrammarTopicDetailModel? faEmailDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-simple-email-grammar",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faEmailDetail);
            Assert.Equal("a2-simple-email-grammar", faEmailDetail!.Slug);
            Assert.Equal("A2", faEmailDetail.CefrLevel);
            Assert.Equal("word-order", faEmailDetail.GrammarCategory);
            Assert.Equal(10, faEmailDetail.Sections.Count);
            Assert.True(faEmailDetail.Examples.Count >= 90);
            Assert.True(faEmailDetail.CommonMistakes.Count >= 35);
            Assert.True(faEmailDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-polite-forms-with-wuerde", faEmailDetail.PrerequisiteSlugs);
            Assert.Contains("b1-formal-email-sentence-structure", faEmailDetail.RelatedTopicSlugs);
            Assert.Contains(
                faEmailDetail.Sections,
                section => section.SectionKey == "core-patterns" && section.Blocks.Any(block => block.Type == "rule-list"));
            Assert.Contains(
                faEmailDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faEmailDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faEmailDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faEmailDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faEmailDetail.Examples, example => example.GermanText == "Ich schreibe Ihnen, weil ich einen Termin brauche.");
            Assert.Contains(faEmailDetail.Examples, example => example.GermanText == "Mit freundlichen Grüßen");
            Assert.Contains(faEmailDetail.CommonMistakes, mistake => mistake.WrongText == "Ich schreibe Sie.");

            GrammarTopicDetailModel? faPhoneDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-grammar-for-phone-calls",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faPhoneDetail);
            Assert.Equal("a2-grammar-for-phone-calls", faPhoneDetail!.Slug);
            Assert.Equal("A2", faPhoneDetail.CefrLevel);
            Assert.Equal("word-order", faPhoneDetail.GrammarCategory);
            Assert.Equal(10, faPhoneDetail.Sections.Count);
            Assert.True(faPhoneDetail.Examples.Count >= 90);
            Assert.True(faPhoneDetail.CommonMistakes.Count >= 35);
            Assert.True(faPhoneDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-simple-email-grammar", faPhoneDetail.PrerequisiteSlugs);
            Assert.Contains("b1-grammar-for-b1-speaking-exam", faPhoneDetail.RelatedTopicSlugs);
            Assert.Contains(
                faPhoneDetail.Sections,
                section => section.SectionKey == "core-patterns" && section.Blocks.Any(block => block.Type == "rule-list"));
            Assert.Contains(
                faPhoneDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faPhoneDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faPhoneDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faPhoneDetail.Examples, example => example.GermanText == "Guten Tag, mein Name ist Ali Rahimi.");
            Assert.Contains(faPhoneDetail.Examples, example => example.GermanText == "Können Sie das bitte wiederholen?");
            Assert.Contains(faPhoneDetail.CommonMistakes, mistake => mistake.WrongText == "Ich rufe, weil ich einen Termin brauche.");

            GrammarTopicDetailModel? faAppointmentDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-grammar-for-appointments",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faAppointmentDetail);
            Assert.Equal("a2-grammar-for-appointments", faAppointmentDetail!.Slug);
            Assert.Equal("A2", faAppointmentDetail.CefrLevel);
            Assert.Equal("tenses", faAppointmentDetail.GrammarCategory);
            Assert.Equal(10, faAppointmentDetail.Sections.Count);
            Assert.True(faAppointmentDetail.Examples.Count >= 90);
            Assert.True(faAppointmentDetail.CommonMistakes.Count >= 35);
            Assert.True(faAppointmentDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-grammar-for-phone-calls", faAppointmentDetail.PrerequisiteSlugs);
            Assert.Contains("a2-grammar-for-doctor-visits", faAppointmentDetail.RelatedTopicSlugs);
            Assert.Contains(
                faAppointmentDetail.Sections,
                section => section.SectionKey == "core-patterns" && section.Blocks.Any(block => block.Type == "rule-list"));
            Assert.Contains(
                faAppointmentDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faAppointmentDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faAppointmentDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faAppointmentDetail.Examples, example => example.GermanText == "Ich brauche einen Termin.");
            Assert.Contains(faAppointmentDetail.Examples, example => example.GermanText == "Ich möchte den Termin verschieben.");
            Assert.Contains(faAppointmentDetail.CommonMistakes, mistake => mistake.WrongText == "Ich brauche ein Termin.");

            GrammarTopicDetailModel? faDoctorDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-grammar-for-doctor-visits",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faDoctorDetail);
            Assert.Equal("a2-grammar-for-doctor-visits", faDoctorDetail!.Slug);
            Assert.Equal("A2", faDoctorDetail.CefrLevel);
            Assert.Equal("cases", faDoctorDetail.GrammarCategory);
            Assert.Equal(10, faDoctorDetail.Sections.Count);
            Assert.True(faDoctorDetail.Examples.Count >= 90);
            Assert.True(faDoctorDetail.CommonMistakes.Count >= 35);
            Assert.True(faDoctorDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-grammar-for-appointments", faDoctorDetail.PrerequisiteSlugs);
            Assert.Contains("a2-grammar-for-phone-calls", faDoctorDetail.RelatedTopicSlugs);
            Assert.Contains(
                faDoctorDetail.Sections,
                section => section.SectionKey == "what-this-topic-is" && section.Blocks.Any(block => block.Type == "callout"));
            Assert.Contains(
                faDoctorDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faDoctorDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faDoctorDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faDoctorDetail.Examples, example => example.GermanText == "Ich habe Kopfschmerzen.");
            Assert.Contains(faDoctorDetail.Examples, example => example.GermanText == "Mein Kopf tut weh.");
            Assert.Contains(faDoctorDetail.CommonMistakes, mistake => mistake.WrongText == "Ich bin Kopfschmerzen.");

            GrammarTopicDetailModel? faSchoolDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-grammar-for-school-and-kindergarten-communication",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faSchoolDetail);
            Assert.Equal("a2-grammar-for-school-and-kindergarten-communication", faSchoolDetail!.Slug);
            Assert.Equal("A2", faSchoolDetail.CefrLevel);
            Assert.Equal("word-order", faSchoolDetail.GrammarCategory);
            Assert.Equal(10, faSchoolDetail.Sections.Count);
            Assert.True(faSchoolDetail.Examples.Count >= 90);
            Assert.True(faSchoolDetail.CommonMistakes.Count >= 35);
            Assert.True(faSchoolDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-grammar-for-appointments", faSchoolDetail.PrerequisiteSlugs);
            Assert.Contains("a2-grammar-for-doctor-visits", faSchoolDetail.RelatedTopicSlugs);
            Assert.Contains(
                faSchoolDetail.Sections,
                section => section.SectionKey == "core-patterns" && section.Blocks.Any(block => block.Type == "rule-list"));
            Assert.Contains(
                faSchoolDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faSchoolDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faSchoolDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faSchoolDetail.Examples, example => example.GermanText == "Mein Kind kommt heute nicht.");
            Assert.Contains(faSchoolDetail.Examples, example => example.GermanText == "Ich hole mein Kind um 15 Uhr ab.");
            Assert.Contains(faSchoolDetail.CommonMistakes, mistake => mistake.WrongText == "Mein Kind kommen nicht.");

            GrammarTopicDetailModel? faA2MistakesDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-common-a2-mistakes",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faA2MistakesDetail);
            Assert.Equal("a2-common-a2-mistakes", faA2MistakesDetail!.Slug);
            Assert.Equal("A2", faA2MistakesDetail.CefrLevel);
            Assert.Equal("cases", faA2MistakesDetail.GrammarCategory);
            Assert.Equal(10, faA2MistakesDetail.Sections.Count);
            Assert.True(faA2MistakesDetail.Examples.Count >= 90);
            Assert.True(faA2MistakesDetail.CommonMistakes.Count >= 70);
            Assert.True(faA2MistakesDetail.RuleSummaries.Count >= 25);
            Assert.Contains("a2-grammar-for-school-and-kindergarten-communication", faA2MistakesDetail.PrerequisiteSlugs);
            Assert.Contains("a2-a2-grammar-review-map", faA2MistakesDetail.RelatedTopicSlugs);
            Assert.Contains(
                faA2MistakesDetail.Sections,
                section => section.SectionKey == "core-patterns" && section.Blocks.Any(block => block.Type == "rule-list"));
            Assert.Contains(
                faA2MistakesDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faA2MistakesDetail.Sections,
                section => section.SectionKey == "practice-advice" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faA2MistakesDetail.CommonMistakes, mistake => mistake.WrongText == "Ich gelernt Deutsch.");
            Assert.Contains(faA2MistakesDetail.CommonMistakes, mistake => mistake.WrongText == "Ich helfe der Mann.");

            GrammarTopicDetailModel? faConnectorDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-a2-connectors-overview",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faConnectorDetail);
            Assert.Equal("a2-a2-connectors-overview", faConnectorDetail!.Slug);
            Assert.Equal("A2", faConnectorDetail.CefrLevel);
            Assert.Equal("connectors", faConnectorDetail.GrammarCategory);
            Assert.Equal(10, faConnectorDetail.Sections.Count);
            Assert.True(faConnectorDetail.Examples.Count >= 90);
            Assert.True(faConnectorDetail.CommonMistakes.Count >= 45);
            Assert.True(faConnectorDetail.RuleSummaries.Count >= 22);
            Assert.Contains("a2-denn-versus-weil", faConnectorDetail.PrerequisiteSlugs);
            Assert.Contains("b1-connectors-for-cause-and-effect", faConnectorDetail.RelatedTopicSlugs);
            Assert.Contains(
                faConnectorDetail.Sections,
                section => section.SectionKey == "core-patterns" && section.Blocks.Any(block => block.Type == "rule-list"));
            Assert.Contains(
                faConnectorDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faConnectorDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faConnectorDetail.Examples, example => example.GermanText == "Ich komme nicht, denn ich bin krank.");
            Assert.Contains(faConnectorDetail.Examples, example => example.GermanText == "Ich komme nicht, weil ich krank bin.");
            Assert.Contains(faConnectorDetail.CommonMistakes, mistake => mistake.WrongText == "Ich komme nicht, denn ich krank bin.");

            GrammarTopicDetailModel? faCaseReviewDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-a2-case-review",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faCaseReviewDetail);
            Assert.Equal("a2-a2-case-review", faCaseReviewDetail!.Slug);
            Assert.Equal("A2", faCaseReviewDetail.CefrLevel);
            Assert.Equal("cases", faCaseReviewDetail.GrammarCategory);
            Assert.Equal(10, faCaseReviewDetail.Sections.Count);
            Assert.True(faCaseReviewDetail.Examples.Count >= 90);
            Assert.True(faCaseReviewDetail.CommonMistakes.Count >= 35);
            Assert.True(faCaseReviewDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-dative-case-basics", faCaseReviewDetail.PrerequisiteSlugs);
            Assert.Contains("b1-b1-case-review", faCaseReviewDetail.RelatedTopicSlugs);
            Assert.Contains(
                faCaseReviewDetail.Sections,
                section => section.SectionKey == "core-patterns" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faCaseReviewDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faCaseReviewDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faCaseReviewDetail.Examples, example => example.GermanText == "Der Lehrer gibt dem Schüler das Buch.");
            Assert.Contains(faCaseReviewDetail.Examples, example => example.GermanText == "Ich danke Ihnen.");
            Assert.Contains(faCaseReviewDetail.CommonMistakes, mistake => mistake.WrongText == "Ich helfe den Lehrer.");

            GrammarTopicDetailModel? faVerbReviewDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-a2-verb-review",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faVerbReviewDetail);
            Assert.Equal("a2-a2-verb-review", faVerbReviewDetail!.Slug);
            Assert.Equal("A2", faVerbReviewDetail.CefrLevel);
            Assert.Equal("verbs", faVerbReviewDetail.GrammarCategory);
            Assert.Equal(10, faVerbReviewDetail.Sections.Count);
            Assert.True(faVerbReviewDetail.Examples.Count >= 90);
            Assert.True(faVerbReviewDetail.CommonMistakes.Count >= 35);
            Assert.True(faVerbReviewDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-perfekt-with-haben", faVerbReviewDetail.PrerequisiteSlugs);
            Assert.Contains("b1-b1-verb-tense-review", faVerbReviewDetail.RelatedTopicSlugs);
            Assert.Contains(
                faVerbReviewDetail.Sections,
                section => section.SectionKey == "core-patterns" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faVerbReviewDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faVerbReviewDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faVerbReviewDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faVerbReviewDetail.Examples, example => example.GermanText == "Ich habe Deutsch gelernt.");
            Assert.Contains(faVerbReviewDetail.Examples, example => example.GermanText == "Ich würde gern kommen.");
            Assert.Contains(faVerbReviewDetail.CommonMistakes, mistake => mistake.WrongText == "Ich muss arbeite.");

            GrammarTopicDetailModel? faA2ReviewMapDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a2-a2-grammar-review-map",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faA2ReviewMapDetail);
            Assert.Equal("a2-a2-grammar-review-map", faA2ReviewMapDetail!.Slug);
            Assert.Equal("A2", faA2ReviewMapDetail.CefrLevel);
            Assert.Equal("word-order", faA2ReviewMapDetail.GrammarCategory);
            Assert.Equal(10, faA2ReviewMapDetail.Sections.Count);
            Assert.True(faA2ReviewMapDetail.Examples.Count >= 90);
            Assert.True(faA2ReviewMapDetail.CommonMistakes.Count >= 35);
            Assert.True(faA2ReviewMapDetail.RuleSummaries.Count >= 20);
            Assert.Contains("a2-perfekt-with-haben", faA2ReviewMapDetail.PrerequisiteSlugs);
            Assert.Contains("a2-a2-verb-review", faA2ReviewMapDetail.PrerequisiteSlugs);
            Assert.Contains("b1-b1-grammar-review-map", faA2ReviewMapDetail.RelatedTopicSlugs);
            Assert.Contains(
                faA2ReviewMapDetail.Sections,
                section => section.SectionKey == "core-patterns" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faA2ReviewMapDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faA2ReviewMapDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faA2ReviewMapDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faA2ReviewMapDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faA2ReviewMapDetail.Sections,
                section => section.SectionKey == "practice-advice" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faA2ReviewMapDetail.Examples, example => example.GermanText == "Ich habe Deutsch gelernt.");
            Assert.Contains(faA2ReviewMapDetail.Examples, example => example.GermanText == "Können Sie mir sagen, wo der Bahnhof ist?");
            Assert.Contains(faA2ReviewMapDetail.CommonMistakes, mistake => mistake.WrongText == "Ich habe nach Hause gegangen.");

            await using DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext dbContext = serviceProvider
                .GetRequiredService<IDbContextFactory<DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext>>()
                .CreateDbContext();
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-perfekt-with-haben"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-perfekt-with-sein"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-common-irregular-participles"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-praeteritum-of-sein-and-haben"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-modal-verbs-in-more-detail"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-dative-case-basics"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-accusative-versus-dative-basics"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-dative-pronouns"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-accusative-pronouns"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-possessive-pronouns-in-cases"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-wechselpraepositionen-introduction"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-prepositions-with-dative"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-prepositions-with-accusative"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-separable-verbs-in-perfekt"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-reflexive-verbs-introduction"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-dass-clauses"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-weil-clauses"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-wenn-for-conditions"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-denn-versus-weil"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-sentence-order-in-subordinate-clauses"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-comparative-forms"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-superlative-basics"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-adjective-endings-introduction"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-indirect-questions-introduction"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-imperative-formal-and-informal"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-time-clauses-bevor-and-nachdem-introduction"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-zu-plus-infinitive-introduction"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-man-as-general-subject"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-es-gibt"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-polite-forms-with-wuerde"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-simple-email-grammar"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-grammar-for-phone-calls"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-grammar-for-appointments"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-grammar-for-doctor-visits"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-grammar-for-school-and-kindergarten-communication"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-common-a2-mistakes"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-a2-connectors-overview"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-a2-case-review"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-a2-verb-review"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a2-a2-grammar-review-map"));
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(databasePath))
            {
                TryDeleteFile(databasePath);
            }
        }
    }

    [Fact]
    public async Task ImportAsync_ShouldReplaceSelectedCourseModuleWithoutRemovingUnrelatedModules()
    {
        string databaseName = $"darwin_course_slice_{Guid.NewGuid():N}"[..46];
        string connectionString = BuildPostgresConnectionString(databaseName);
        string fullPackagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-course-full-{Guid.NewGuid():N}.json");
        string moduleSlicePackagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-course-module-slice-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        await CreatePostgresDatabaseAsync(databaseName, CancellationToken.None);

        try
        {
            await File.WriteAllTextAsync(
                fullPackagePath,
                CreateCourseSlicePackageJson(
                    "course-module-slice-full",
                    includeSecondModule: true,
                    firstModuleTitle: "Erste Kontakte",
                    firstLessonTitle: "Begruessung und Name",
                    secondModuleTitle: "Alltag und Termine",
                    secondLessonTitle: "Einen Termin nennen"));
            await File.WriteAllTextAsync(
                moduleSlicePackagePath,
                CreateCourseSlicePackageJson(
                    "course-module-slice-update",
                    includeSecondModule: false,
                    firstModuleTitle: "Erste Kontakte aktualisiert",
                    firstLessonTitle: "Begruessung klar aufbauen",
                    secondModuleTitle: "Alltag und Termine",
                    secondLessonTitle: "Einen Termin nennen"));

            serviceProvider = BuildPostgresServiceProvider(connectionString);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult fullImportResult = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(fullPackagePath), CancellationToken.None);
            Assert.True(fullImportResult.IsSuccess, string.Join(Environment.NewLine, fullImportResult.Issues.Select(issue => issue.Message)));

            ImportContentPackageResult sliceImportResult = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(moduleSlicePackagePath), CancellationToken.None);
            Assert.True(sliceImportResult.IsSuccess, string.Join(Environment.NewLine, sliceImportResult.Issues.Select(issue => issue.Message)));

            await using DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext dbContext = serviceProvider
                .GetRequiredService<IDbContextFactory<DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext>>()
                .CreateDbContext();

            Assert.Equal(1, await dbContext.CoursePaths.CountAsync(course => course.Slug == "a1-slice-safe-course"));
            Assert.Equal(2, await dbContext.CourseModules.CountAsync(module => module.CoursePathSlug == "a1-slice-safe-course"));
            Assert.Equal(2, await dbContext.CourseLessons.CountAsync(lesson => lesson.CoursePathSlug == "a1-slice-safe-course"));

            Assert.Equal(
                "Erste Kontakte aktualisiert",
                await dbContext.CourseModules
                    .Where(module => module.Slug == "a1-slice-module-one")
                    .Select(module => module.Title)
                    .SingleAsync());
            Assert.Equal(
                "Begruessung klar aufbauen",
                await dbContext.CourseLessons
                    .Where(lesson => lesson.Slug == "a1-slice-lesson-one")
                    .Select(lesson => lesson.Title)
                    .SingleAsync());

            Assert.Equal(
                "Alltag und Termine",
                await dbContext.CourseModules
                    .Where(module => module.Slug == "a1-slice-module-two")
                    .Select(module => module.Title)
                    .SingleAsync());
            Assert.Equal(
                "Einen Termin nennen",
                await dbContext.CourseLessons
                    .Where(lesson => lesson.Slug == "a1-slice-lesson-two")
                    .Select(lesson => lesson.Title)
                    .SingleAsync());
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(fullPackagePath))
            {
                File.Delete(fullPackagePath);
            }

            if (File.Exists(moduleSlicePackagePath))
            {
                File.Delete(moduleSlicePackagePath);
            }

            await DropPostgresDatabaseAsync(databaseName, CancellationToken.None);
        }
    }

    [Fact]
    public async Task ImportAsync_ShouldImportOfficialB1RelativeClausesBasicsGrammarTopic()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-grammar-b1-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(ResolveRepositoryRoot(), "content", "learning-portal", "grammar", "packages", "grammar-b1-core-v1.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));

            IGrammarTopicQueryService grammarQueryService = serviceProvider.GetRequiredService<IGrammarTopicQueryService>();
            GrammarTopicDetailModel? faDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-relative-clauses-basics",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faDetail);
            Assert.Equal("b1-relative-clauses-basics", faDetail!.Slug);
            Assert.Equal("B1", faDetail.CefrLevel);
            Assert.Equal("subordinate-clauses", faDetail.GrammarCategory);
            Assert.Equal(12, faDetail.Sections.Count);
            Assert.True(faDetail.Examples.Count >= 130);
            Assert.True(faDetail.CommonMistakes.Count >= 50);
            Assert.True(faDetail.RuleSummaries.Count >= 24);
            Assert.Contains("a2-dass-clauses", faDetail.PrerequisiteSlugs);
            Assert.Contains("b1-relative-pronouns-in-nominative-and-accusative", faDetail.RelatedTopicSlugs);
            Assert.Contains(
                faDetail.Sections,
                section => section.SectionKey == "core-patterns" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faDetail.Examples, example => example.GermanText == "Das ist der Mann, der hier arbeitet.");
            Assert.Contains(faDetail.Examples, example => example.GermanText == "Das ist die Frau, die im Büro arbeitet.");
            Assert.Contains(faDetail.CommonMistakes, mistake => mistake.WrongText == "Das ist der Mann, der arbeitet hier.");
            Assert.Contains(faDetail.CommonMistakes, mistake => mistake.WrongText == "Das ist das Buch, das ich lese es.");

            GrammarTopicDetailModel? faNomAccDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-relative-pronouns-in-nominative-and-accusative",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faNomAccDetail);
            Assert.Equal("b1-relative-pronouns-in-nominative-and-accusative", faNomAccDetail!.Slug);
            Assert.Equal("B1", faNomAccDetail.CefrLevel);
            Assert.Equal("subordinate-clauses", faNomAccDetail.GrammarCategory);
            Assert.Equal(13, faNomAccDetail.Sections.Count);
            Assert.True(faNomAccDetail.Examples.Count >= 130);
            Assert.True(faNomAccDetail.CommonMistakes.Count >= 50);
            Assert.True(faNomAccDetail.RuleSummaries.Count >= 24);
            Assert.Contains("b1-relative-clauses-basics", faNomAccDetail.PrerequisiteSlugs);
            Assert.Contains("b1-relative-pronouns-in-dative", faNomAccDetail.RelatedTopicSlugs);
            Assert.Contains(
                faNomAccDetail.Sections,
                section => section.SectionKey == "core-patterns" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faNomAccDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faNomAccDetail.Sections,
                section => section.SectionKey == "relative-pronoun-role-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faNomAccDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faNomAccDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faNomAccDetail.Sections,
                section => section.SectionKey == "same-noun-different-case" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faNomAccDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faNomAccDetail.Sections,
                section => section.SectionKey == "verb-final-reminder" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faNomAccDetail.Examples, example => example.GermanText == "Das ist der Mann, der hier arbeitet.");
            Assert.Contains(faNomAccDetail.Examples, example => example.GermanText == "Das ist der Mann, den ich kenne.");
            Assert.Contains(faNomAccDetail.CommonMistakes, mistake => mistake.WrongText == "Das ist der Mann, den hier arbeitet.");
            Assert.Contains(faNomAccDetail.CommonMistakes, mistake => mistake.WrongText == "Das ist der Mann, der ich kenne.");

            GrammarTopicDetailModel? faDativeDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-relative-pronouns-in-dative",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faDativeDetail);
            Assert.Equal("b1-relative-pronouns-in-dative", faDativeDetail!.Slug);
            Assert.Equal("B1", faDativeDetail.CefrLevel);
            Assert.Equal("subordinate-clauses", faDativeDetail.GrammarCategory);
            Assert.Equal(13, faDativeDetail.Sections.Count);
            Assert.True(faDativeDetail.Examples.Count >= 130);
            Assert.True(faDativeDetail.CommonMistakes.Count >= 50);
            Assert.True(faDativeDetail.RuleSummaries.Count >= 24);
            Assert.Contains("b1-relative-pronouns-in-nominative-and-accusative", faDativeDetail.PrerequisiteSlugs);
            Assert.Contains("b1-verb-plus-preposition-combinations", faDativeDetail.RelatedTopicSlugs);
            Assert.Contains(
                faDativeDetail.Sections,
                section => section.SectionKey == "dative-relative-pronoun-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faDativeDetail.Sections,
                section => section.SectionKey == "how-to-decide-dative" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faDativeDetail.Sections,
                section => section.SectionKey == "dem-der-dem-denen-comparison" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faDativeDetail.Sections,
                section => section.SectionKey == "common-confusion-with-accusative" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faDativeDetail.Examples, example => example.GermanText == "Das ist der Mann, dem ich helfe.");
            Assert.Contains(faDativeDetail.Examples, example => example.GermanText == "Das sind die Kinder, denen ich helfe.");
            Assert.Contains(faDativeDetail.CommonMistakes, mistake => mistake.WrongText == "Der Mann, den ich helfe.");
            Assert.Contains(faDativeDetail.CommonMistakes, mistake => mistake.WrongText == "Die Leute, mit die ich spreche.");

            GrammarTopicDetailModel? faPoliteDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-konjunktiv-ii-for-polite-requests",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faPoliteDetail);
            Assert.Equal("b1-konjunktiv-ii-for-polite-requests", faPoliteDetail!.Slug);
            Assert.Equal("B1", faPoliteDetail.CefrLevel);
            Assert.Equal("konjunktiv", faPoliteDetail.GrammarCategory);
            Assert.Equal(13, faPoliteDetail.Sections.Count);
            Assert.True(faPoliteDetail.Examples.Count >= 130);
            Assert.True(faPoliteDetail.CommonMistakes.Count >= 50);
            Assert.True(faPoliteDetail.RuleSummaries.Count >= 24);
            Assert.Contains("a2-polite-forms-with-wuerde", faPoliteDetail.PrerequisiteSlugs);
            Assert.Contains("b1-konjunktiv-ii-with-waere-haette-wuerde", faPoliteDetail.RelatedTopicSlugs);
            Assert.Contains(
                faPoliteDetail.Sections,
                section => section.SectionKey == "polite-request-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faPoliteDetail.Sections,
                section => section.SectionKey == "word-order-with-konjunktiv-ii" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faPoliteDetail.Sections,
                section => section.SectionKey == "formal-vs-informal" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faPoliteDetail.Sections,
                section => section.SectionKey == "email-and-phone-requests" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faPoliteDetail.Examples, example => example.GermanText == "Könnten Sie mir bitte helfen?");
            Assert.Contains(faPoliteDetail.Examples, example => example.GermanText == "Wäre es möglich, den Termin zu verschieben?");
            Assert.Contains(faPoliteDetail.CommonMistakes, mistake => mistake.WrongText == "Könnten Sie helfen mir?");
            Assert.Contains(faPoliteDetail.CommonMistakes, mistake => mistake.WrongText == "Dürfte ich fragen Sie?");

            GrammarTopicDetailModel? faCoreKonjunktivDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-konjunktiv-ii-with-waere-haette-wuerde",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faCoreKonjunktivDetail);
            Assert.Equal("b1-konjunktiv-ii-with-waere-haette-wuerde", faCoreKonjunktivDetail!.Slug);
            Assert.Equal("B1", faCoreKonjunktivDetail.CefrLevel);
            Assert.Equal("konjunktiv", faCoreKonjunktivDetail.GrammarCategory);
            Assert.Equal(14, faCoreKonjunktivDetail.Sections.Count);
            Assert.True(faCoreKonjunktivDetail.Examples.Count >= 140);
            Assert.True(faCoreKonjunktivDetail.CommonMistakes.Count >= 50);
            Assert.True(faCoreKonjunktivDetail.RuleSummaries.Count >= 24);
            Assert.Contains("b1-konjunktiv-ii-for-polite-requests", faCoreKonjunktivDetail.PrerequisiteSlugs);
            Assert.Contains("b1-talking-about-plans-and-conditions", faCoreKonjunktivDetail.RelatedTopicSlugs);
            Assert.Contains(
                faCoreKonjunktivDetail.Sections,
                section => section.SectionKey == "waere" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faCoreKonjunktivDetail.Sections,
                section => section.SectionKey == "haette" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faCoreKonjunktivDetail.Sections,
                section => section.SectionKey == "wuerde" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faCoreKonjunktivDetail.Sections,
                section => section.SectionKey == "form-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faCoreKonjunktivDetail.Sections,
                section => section.SectionKey == "wenn-plus-konjunktiv-ii" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faCoreKonjunktivDetail.Sections,
                section => section.SectionKey == "common-patterns-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faCoreKonjunktivDetail.Examples, example => example.GermanText == "Wenn ich Zeit hätte, würde ich kommen.");
            Assert.Contains(faCoreKonjunktivDetail.Examples, example => example.GermanText == "Ich hätte gern einen Termin.");
            Assert.Contains(faCoreKonjunktivDetail.Examples, example => example.GermanText == "Würden Sie bitte helfen?");
            Assert.Contains(faCoreKonjunktivDetail.CommonMistakes, mistake => mistake.WrongText == "Wenn ich hätte Zeit, würde ich kommen.");
            Assert.Contains(faCoreKonjunktivDetail.CommonMistakes, mistake => mistake.WrongText == "Ich würde gern einen Termin.");
            Assert.Contains(faCoreKonjunktivDetail.CommonMistakes, mistake => mistake.WrongText == "Wäre es möglich, Sie helfen mir?");

            GrammarTopicDetailModel? faPassiveDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-passive-voice-introduction",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faPassiveDetail);
            Assert.Equal("b1-passive-voice-introduction", faPassiveDetail!.Slug);
            Assert.Equal("B1", faPassiveDetail.CefrLevel);
            Assert.Equal("passive", faPassiveDetail.GrammarCategory);
            Assert.Equal(13, faPassiveDetail.Sections.Count);
            Assert.True(faPassiveDetail.Examples.Count >= 120);
            Assert.True(faPassiveDetail.CommonMistakes.Count >= 50);
            Assert.True(faPassiveDetail.RuleSummaries.Count >= 24);
            Assert.Contains("a2-man-as-general-subject", faPassiveDetail.PrerequisiteSlugs);
            Assert.Contains("b1-werden-as-auxiliary", faPassiveDetail.RelatedTopicSlugs);
            Assert.Contains(
                faPassiveDetail.Sections,
                section => section.SectionKey == "active-vs-passive-basic" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faPassiveDetail.Sections,
                section => section.SectionKey == "passive-structure" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faPassiveDetail.Sections,
                section => section.SectionKey == "werden-conjugation-present" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faPassiveDetail.Sections,
                section => section.SectionKey == "passive-vs-man" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faPassiveDetail.Examples, example => example.GermanText == "Das Auto wird repariert.");
            Assert.Contains(faPassiveDetail.Examples, example => example.GermanText == "Der Antrag wird geprüft.");
            Assert.Contains(faPassiveDetail.CommonMistakes, mistake => mistake.WrongText == "Das Auto wird reparieren.");
            Assert.Contains(faPassiveDetail.CommonMistakes, mistake => mistake.WrongText == "Das Formular werden geprüft.");

            GrammarTopicDetailModel? faWerdenDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-werden-as-auxiliary",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faWerdenDetail);
            Assert.Equal("b1-werden-as-auxiliary", faWerdenDetail!.Slug);
            Assert.Equal("B1", faWerdenDetail.CefrLevel);
            Assert.Equal("verbs", faWerdenDetail.GrammarCategory);
            Assert.Equal(13, faWerdenDetail.Sections.Count);
            Assert.True(faWerdenDetail.Examples.Count >= 130);
            Assert.True(faWerdenDetail.CommonMistakes.Count >= 50);
            Assert.True(faWerdenDetail.RuleSummaries.Count >= 24);
            Assert.Contains("b1-passive-voice-introduction", faWerdenDetail.PrerequisiteSlugs);
            Assert.Contains("b2-passive-with-modal-verbs", faWerdenDetail.RelatedTopicSlugs);
            Assert.Contains(
                faWerdenDetail.Sections,
                section => section.SectionKey == "werden-conjugation" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faWerdenDetail.Sections,
                section => section.SectionKey == "passive-vs-future-pattern" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faWerdenDetail.Sections,
                section => section.SectionKey == "participle-vs-infinitive" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faWerdenDetail.Sections,
                section => section.SectionKey == "word-order-with-werden" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faWerdenDetail.Examples, example => example.GermanText == "Ich werde müde.");
            Assert.Contains(faWerdenDetail.Examples, example => example.GermanText == "Das Formular wird geprüft.");
            Assert.Contains(faWerdenDetail.Examples, example => example.GermanText == "Ich werde morgen kommen.");
            Assert.Contains(faWerdenDetail.CommonMistakes, mistake => mistake.WrongText == "Ich werde müde gehen.");
            Assert.Contains(faWerdenDetail.CommonMistakes, mistake => mistake.WrongText == "Das Formular wird prüfen.");

            GrammarTopicDetailModel? faInfinitiveZuDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-infinitive-with-zu",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faInfinitiveZuDetail);
            Assert.Equal("b1-infinitive-with-zu", faInfinitiveZuDetail!.Slug);
            Assert.Equal("B1", faInfinitiveZuDetail.CefrLevel);
            Assert.Equal("verbs", faInfinitiveZuDetail.GrammarCategory);
            Assert.Equal(14, faInfinitiveZuDetail.Sections.Count);
            Assert.True(faInfinitiveZuDetail.Examples.Count >= 130);
            Assert.True(faInfinitiveZuDetail.CommonMistakes.Count >= 50);
            Assert.True(faInfinitiveZuDetail.RuleSummaries.Count >= 24);
            Assert.Contains("a2-zu-plus-infinitive-introduction", faInfinitiveZuDetail.PrerequisiteSlugs);
            Assert.Contains("b1-um-zu", faInfinitiveZuDetail.RelatedTopicSlugs);
            Assert.Contains(
                faInfinitiveZuDetail.Sections,
                section => section.SectionKey == "common-trigger-verbs" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faInfinitiveZuDetail.Sections,
                section => section.SectionKey == "common-trigger-phrases" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faInfinitiveZuDetail.Sections,
                section => section.SectionKey == "separable-verbs-with-zu" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faInfinitiveZuDetail.Sections,
                section => section.SectionKey == "modal-verbs-without-zu" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faInfinitiveZuDetail.Examples, example => example.GermanText == "Ich versuche, Deutsch zu lernen.");
            Assert.Contains(faInfinitiveZuDetail.Examples, example => example.GermanText == "Es ist wichtig, pünktlich zu sein.");
            Assert.Contains(faInfinitiveZuDetail.Examples, example => example.GermanText == "Ich muss arbeiten.");
            Assert.Contains(faInfinitiveZuDetail.CommonMistakes, mistake => mistake.WrongText == "Ich versuche Deutsch lernen.");
            Assert.Contains(faInfinitiveZuDetail.CommonMistakes, mistake => mistake.WrongText == "Ich muss zu arbeiten.");

            GrammarTopicDetailModel? faUmZuDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-um-zu",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faUmZuDetail);
            Assert.Equal("b1-um-zu", faUmZuDetail!.Slug);
            Assert.Equal("B1", faUmZuDetail.CefrLevel);
            Assert.Equal("subordinate-clauses", faUmZuDetail.GrammarCategory);
            Assert.Equal(13, faUmZuDetail.Sections.Count);
            Assert.True(faUmZuDetail.Examples.Count >= 130);
            Assert.True(faUmZuDetail.CommonMistakes.Count >= 50);
            Assert.True(faUmZuDetail.RuleSummaries.Count >= 24);
            Assert.Contains("b1-infinitive-with-zu", faUmZuDetail.PrerequisiteSlugs);
            Assert.Contains("b1-damit-versus-um-zu", faUmZuDetail.RelatedTopicSlugs);
            Assert.Contains(
                faUmZuDetail.Sections,
                section => section.SectionKey == "same-subject-rule" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faUmZuDetail.Sections,
                section => section.SectionKey == "basic-pattern" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faUmZuDetail.Sections,
                section => section.SectionKey == "separable-verbs-with-um-zu" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faUmZuDetail.Sections,
                section => section.SectionKey == "when-not-to-use-um-zu" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faUmZuDetail.Examples, example => example.GermanText == "Ich lerne Deutsch, um in Deutschland zu arbeiten.");
            Assert.Contains(faUmZuDetail.Examples, example => example.GermanText == "Ich spare Geld, um ein Auto zu kaufen.");
            Assert.Contains(faUmZuDetail.CommonMistakes, mistake => mistake.WrongText == "Ich lerne Deutsch, um ich arbeite in Deutschland.");
            Assert.Contains(faUmZuDetail.CommonMistakes, mistake => mistake.WrongText == "Ich lerne Deutsch, um mein Sohn besser Deutsch spricht.");

            GrammarTopicDetailModel? faDamitUmZuDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-damit-versus-um-zu",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faDamitUmZuDetail);
            Assert.Equal("b1-damit-versus-um-zu", faDamitUmZuDetail!.Slug);
            Assert.Equal("B1", faDamitUmZuDetail.CefrLevel);
            Assert.Equal("subordinate-clauses", faDamitUmZuDetail.GrammarCategory);
            Assert.Equal(14, faDamitUmZuDetail.Sections.Count);
            Assert.True(faDamitUmZuDetail.Examples.Count >= 150);
            Assert.True(faDamitUmZuDetail.CommonMistakes.Count >= 55);
            Assert.True(faDamitUmZuDetail.RuleSummaries.Count >= 24);
            Assert.Contains("b1-um-zu", faDamitUmZuDetail.PrerequisiteSlugs);
            Assert.Contains("b1-connectors-for-cause-and-effect", faDamitUmZuDetail.RelatedTopicSlugs);
            Assert.Contains(
                faDamitUmZuDetail.Sections,
                section => section.SectionKey == "same-subject-vs-different-subject-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faDamitUmZuDetail.Sections,
                section => section.SectionKey == "paired-examples" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faDamitUmZuDetail.Sections,
                section => section.SectionKey == "damit-vs-weil" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faDamitUmZuDetail.Sections,
                section => section.SectionKey == "decision-checklist" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faDamitUmZuDetail.Examples, example => example.GermanText == "Ich helfe meinem Sohn, damit er Deutsch lernt.");
            Assert.Contains(faDamitUmZuDetail.Examples, example => example.GermanText == "Ich lerne Deutsch, um in Deutschland zu arbeiten.");
            Assert.Contains(faDamitUmZuDetail.CommonMistakes, mistake => mistake.WrongText == "Ich lerne Deutsch, damit ich in Deutschland zu arbeiten.");
            Assert.Contains(faDamitUmZuDetail.CommonMistakes, mistake => mistake.WrongText == "Damit er Deutsch lernt, ich helfe ihm.");

            GrammarTopicDetailModel? faWeilObwohlTrotzdemDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-weil-obwohl-trotzdem",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faWeilObwohlTrotzdemDetail);
            Assert.Equal("b1-weil-obwohl-trotzdem", faWeilObwohlTrotzdemDetail!.Slug);
            Assert.Equal("B1", faWeilObwohlTrotzdemDetail.CefrLevel);
            Assert.Equal("connectors", faWeilObwohlTrotzdemDetail.GrammarCategory);
            Assert.Equal(14, faWeilObwohlTrotzdemDetail.Sections.Count);
            Assert.True(faWeilObwohlTrotzdemDetail.Examples.Count >= 140);
            Assert.True(faWeilObwohlTrotzdemDetail.CommonMistakes.Count >= 50);
            Assert.True(faWeilObwohlTrotzdemDetail.RuleSummaries.Count >= 22);
            Assert.Contains("a2-weil-clauses", faWeilObwohlTrotzdemDetail.PrerequisiteSlugs);
            Assert.Contains("b1-connectors-for-contrast", faWeilObwohlTrotzdemDetail.RelatedTopicSlugs);
            Assert.Contains(
                faWeilObwohlTrotzdemDetail.Sections,
                section => section.SectionKey == "meaning-comparison-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faWeilObwohlTrotzdemDetail.Sections,
                section => section.SectionKey == "word-order-comparison-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faWeilObwohlTrotzdemDetail.Sections,
                section => section.SectionKey == "obwohl-vs-trotzdem" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faWeilObwohlTrotzdemDetail.Sections,
                section => section.SectionKey == "connectors-with-modal-verbs" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faWeilObwohlTrotzdemDetail.Examples, example => example.GermanText == "Ich bleibe zu Hause, weil ich krank bin.");
            Assert.Contains(faWeilObwohlTrotzdemDetail.Examples, example => example.GermanText == "Ich bin krank. Trotzdem gehe ich zur Arbeit.");
            Assert.Contains(faWeilObwohlTrotzdemDetail.CommonMistakes, mistake => mistake.WrongText == "Ich komme nicht, weil ich bin krank.");
            Assert.Contains(faWeilObwohlTrotzdemDetail.CommonMistakes, mistake => mistake.WrongText == "Trotzdem ich gehe zur Arbeit.");

            GrammarTopicDetailModel? faAlsWennDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-als-versus-wenn",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faAlsWennDetail);
            Assert.Equal("b1-als-versus-wenn", faAlsWennDetail!.Slug);
            Assert.Equal("B1", faAlsWennDetail.CefrLevel);
            Assert.Equal("subordinate-clauses", faAlsWennDetail.GrammarCategory);
            Assert.Equal(13, faAlsWennDetail.Sections.Count);
            Assert.True(faAlsWennDetail.Examples.Count >= 140);
            Assert.True(faAlsWennDetail.CommonMistakes.Count >= 50);
            Assert.True(faAlsWennDetail.RuleSummaries.Count >= 22);
            Assert.Contains("a2-wenn-for-conditions", faAlsWennDetail.PrerequisiteSlugs);
            Assert.Contains("b1-nachdem-bevor-waehrend", faAlsWennDetail.RelatedTopicSlugs);
            Assert.Contains(
                faAlsWennDetail.Sections,
                section => section.SectionKey == "timeline-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faAlsWennDetail.Sections,
                section => section.SectionKey == "als-vs-wenn-paired-examples" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faAlsWennDetail.Sections,
                section => section.SectionKey == "practice-advice" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faAlsWennDetail.Examples, example => example.GermanText == "Als ich ein Kind war, wohnte ich in meinem Heimatland.");
            Assert.Contains(faAlsWennDetail.Examples, example => example.GermanText == "Wenn ich krank war, blieb ich zu Hause.");
            Assert.Contains(faAlsWennDetail.CommonMistakes, mistake => mistake.WrongText == "Wenn ich nach Deutschland gekommen bin, war ich nervös.");
            Assert.Contains(faAlsWennDetail.CommonMistakes, mistake => mistake.WrongText == "Wenn ich Zeit habe, ich komme.");

            GrammarTopicDetailModel? faNachdemBevorWaehrendDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-nachdem-bevor-waehrend",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faNachdemBevorWaehrendDetail);
            Assert.Equal("b1-nachdem-bevor-waehrend", faNachdemBevorWaehrendDetail!.Slug);
            Assert.Equal("B1", faNachdemBevorWaehrendDetail.CefrLevel);
            Assert.Equal("subordinate-clauses", faNachdemBevorWaehrendDetail.GrammarCategory);
            Assert.Equal(14, faNachdemBevorWaehrendDetail.Sections.Count);
            Assert.True(faNachdemBevorWaehrendDetail.Examples.Count >= 150);
            Assert.True(faNachdemBevorWaehrendDetail.CommonMistakes.Count >= 55);
            Assert.True(faNachdemBevorWaehrendDetail.RuleSummaries.Count >= 24);
            Assert.Contains("a2-time-clauses-bevor-and-nachdem-introduction", faNachdemBevorWaehrendDetail.PrerequisiteSlugs);
            Assert.Contains("b1-als-versus-wenn", faNachdemBevorWaehrendDetail.PrerequisiteSlugs);
            Assert.Contains("b1-sentence-order-with-multiple-clauses", faNachdemBevorWaehrendDetail.RelatedTopicSlugs);
            Assert.Contains(
                faNachdemBevorWaehrendDetail.Sections,
                section => section.SectionKey == "timeline-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faNachdemBevorWaehrendDetail.Sections,
                section => section.SectionKey == "verb-final-rule" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faNachdemBevorWaehrendDetail.Sections,
                section => section.SectionKey == "nachdem-vs-danach" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faNachdemBevorWaehrendDetail.Examples, example => example.GermanText == "Nachdem ich gefrühstückt habe, gehe ich zur Arbeit.");
            Assert.Contains(faNachdemBevorWaehrendDetail.Examples, example => example.GermanText == "Bevor ich zur Arbeit gehe, frühstücke ich.");
            Assert.Contains(faNachdemBevorWaehrendDetail.Examples, example => example.GermanText == "Während ich arbeite, hört mein Sohn Musik.");
            Assert.Contains(faNachdemBevorWaehrendDetail.CommonMistakes, mistake => mistake.WrongText == "Nachdem ich habe gegessen, lerne ich.");
            Assert.Contains(faNachdemBevorWaehrendDetail.CommonMistakes, mistake => mistake.WrongText == "Während ich arbeite, mein Sohn hört Musik.");

            GrammarTopicDetailModel? faIndirectQuestionsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-indirect-questions",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faIndirectQuestionsDetail);
            Assert.Equal("b1-indirect-questions", faIndirectQuestionsDetail!.Slug);
            Assert.Equal("B1", faIndirectQuestionsDetail.CefrLevel);
            Assert.Equal("questions", faIndirectQuestionsDetail.GrammarCategory);
            Assert.Equal(14, faIndirectQuestionsDetail.Sections.Count);
            Assert.True(faIndirectQuestionsDetail.Examples.Count >= 150);
            Assert.True(faIndirectQuestionsDetail.CommonMistakes.Count >= 55);
            Assert.True(faIndirectQuestionsDetail.RuleSummaries.Count >= 24);
            Assert.Contains("a2-indirect-questions-introduction", faIndirectQuestionsDetail.PrerequisiteSlugs);
            Assert.Contains("b1-konjunktiv-ii-for-polite-requests", faIndirectQuestionsDetail.PrerequisiteSlugs);
            Assert.Contains("b1-reported-requests-and-polite-questions", faIndirectQuestionsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faIndirectQuestionsDetail.Sections,
                section => section.SectionKey == "polite-frame-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faIndirectQuestionsDetail.Sections,
                section => section.SectionKey == "direct-to-indirect-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faIndirectQuestionsDetail.Sections,
                section => section.SectionKey == "indirect-questions-with-modal-verbs" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faIndirectQuestionsDetail.Sections,
                section => section.SectionKey == "indirect-questions-with-perfekt" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faIndirectQuestionsDetail.Sections,
                section => section.SectionKey == "indirect-question-vs-dass-clause" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faIndirectQuestionsDetail.Examples, example => example.GermanText == "Können Sie mir sagen, wo der Bahnhof ist?");
            Assert.Contains(faIndirectQuestionsDetail.Examples, example => example.GermanText == "Kommt der Bus? Wissen Sie, ob der Bus kommt?");
            Assert.Contains(faIndirectQuestionsDetail.CommonMistakes, mistake => mistake.WrongText == "Können Sie mir sagen, wo ist der Bahnhof?");
            Assert.Contains(faIndirectQuestionsDetail.CommonMistakes, mistake => mistake.WrongText == "Wissen Sie, ob kommt der Bus?");

            GrammarTopicDetailModel? faReportedRequestsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-reported-requests-and-polite-questions",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faReportedRequestsDetail);
            Assert.Equal("b1-reported-requests-and-polite-questions", faReportedRequestsDetail!.Slug);
            Assert.Equal("B1", faReportedRequestsDetail.CefrLevel);
            Assert.Equal("questions", faReportedRequestsDetail.GrammarCategory);
            Assert.Equal(14, faReportedRequestsDetail.Sections.Count);
            Assert.True(faReportedRequestsDetail.Examples.Count >= 150);
            Assert.True(faReportedRequestsDetail.CommonMistakes.Count >= 55);
            Assert.True(faReportedRequestsDetail.RuleSummaries.Count >= 24);
            Assert.Contains("b1-indirect-questions", faReportedRequestsDetail.PrerequisiteSlugs);
            Assert.Contains("b1-formal-email-sentence-structure", faReportedRequestsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faReportedRequestsDetail.Sections,
                section => section.SectionKey == "ob-for-reported-yes-no-questions" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faReportedRequestsDetail.Sections,
                section => section.SectionKey == "w-question-reported" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faReportedRequestsDetail.Sections,
                section => section.SectionKey == "bitten-plus-zu-infinitive" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faReportedRequestsDetail.Sections,
                section => section.SectionKey == "sagen-plus-sollen" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faReportedRequestsDetail.Sections,
                section => section.SectionKey == "polite-request-vs-reported-request-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faReportedRequestsDetail.Examples, example => example.GermanText == "Sie fragt, ob ich morgen kommen kann.");
            Assert.Contains(faReportedRequestsDetail.Examples, example => example.GermanText == "Sie bittet mich, morgen zu kommen.");
            Assert.Contains(faReportedRequestsDetail.CommonMistakes, mistake => mistake.WrongText == "Sie fragt, komme ich morgen.");
            Assert.Contains(faReportedRequestsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich bitte Sie, füllen das Formular aus.");

            GrammarTopicDetailModel? faDefiniteAdjectiveDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-adjective-declension-after-definite-article",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faDefiniteAdjectiveDetail);
            Assert.Equal("b1-adjective-declension-after-definite-article", faDefiniteAdjectiveDetail!.Slug);
            Assert.Equal("B1", faDefiniteAdjectiveDetail.CefrLevel);
            Assert.Equal("adjective-declension", faDefiniteAdjectiveDetail.GrammarCategory);
            Assert.Equal(14, faDefiniteAdjectiveDetail.Sections.Count);
            Assert.True(faDefiniteAdjectiveDetail.Examples.Count >= 150);
            Assert.True(faDefiniteAdjectiveDetail.CommonMistakes.Count >= 55);
            Assert.True(faDefiniteAdjectiveDetail.RuleSummaries.Count >= 25);
            Assert.Contains("a2-adjective-endings-introduction", faDefiniteAdjectiveDetail.PrerequisiteSlugs);
            Assert.Contains("b1-adjective-declension-after-indefinite-article", faDefiniteAdjectiveDetail.RelatedTopicSlugs);
            Assert.Contains(
                faDefiniteAdjectiveDetail.Sections,
                section => section.SectionKey == "nominative-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faDefiniteAdjectiveDetail.Sections,
                section => section.SectionKey == "accusative-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faDefiniteAdjectiveDetail.Sections,
                section => section.SectionKey == "dative-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faDefiniteAdjectiveDetail.Sections,
                section => section.SectionKey == "formal-writing-use" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faDefiniteAdjectiveDetail.Sections,
                section => section.SectionKey == "pattern-memory-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faDefiniteAdjectiveDetail.Examples, example => example.GermanText == "Der gute Kaffee ist teuer.");
            Assert.Contains(faDefiniteAdjectiveDetail.Examples, example => example.GermanText == "Ich trinke den guten Kaffee.");
            Assert.Contains(faDefiniteAdjectiveDetail.CommonMistakes, mistake => mistake.WrongText == "der gut Kaffee");
            Assert.Contains(faDefiniteAdjectiveDetail.CommonMistakes, mistake => mistake.WrongText == "den alten Bücher");

            GrammarTopicDetailModel? faIndefiniteAdjectiveDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-adjective-declension-after-indefinite-article",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faIndefiniteAdjectiveDetail);
            Assert.Equal("b1-adjective-declension-after-indefinite-article", faIndefiniteAdjectiveDetail!.Slug);
            Assert.Equal("B1", faIndefiniteAdjectiveDetail.CefrLevel);
            Assert.Equal("adjective-declension", faIndefiniteAdjectiveDetail.GrammarCategory);
            Assert.Equal(15, faIndefiniteAdjectiveDetail.Sections.Count);
            Assert.True(faIndefiniteAdjectiveDetail.Examples.Count >= 160);
            Assert.True(faIndefiniteAdjectiveDetail.CommonMistakes.Count >= 60);
            Assert.True(faIndefiniteAdjectiveDetail.RuleSummaries.Count >= 28);
            Assert.Contains("b1-adjective-declension-after-definite-article", faIndefiniteAdjectiveDetail.PrerequisiteSlugs);
            Assert.Contains("a2-possessive-pronouns-in-cases", faIndefiniteAdjectiveDetail.PrerequisiteSlugs);
            Assert.Contains("b1-adjective-declension-without-article", faIndefiniteAdjectiveDetail.RelatedTopicSlugs);
            Assert.Contains(
                faIndefiniteAdjectiveDetail.Sections,
                section => section.SectionKey == "nominative-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faIndefiniteAdjectiveDetail.Sections,
                section => section.SectionKey == "accusative-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faIndefiniteAdjectiveDetail.Sections,
                section => section.SectionKey == "dative-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faIndefiniteAdjectiveDetail.Sections,
                section => section.SectionKey == "ein-vs-der-comparison" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faIndefiniteAdjectiveDetail.Sections,
                section => section.SectionKey == "possessives-as-ein-words" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faIndefiniteAdjectiveDetail.Sections,
                section => section.SectionKey == "kein-as-ein-word" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faIndefiniteAdjectiveDetail.Sections,
                section => section.SectionKey == "comparison-with-definite-article-pattern" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faIndefiniteAdjectiveDetail.Examples, example => example.GermanText == "Ein guter Kaffee hilft mir.");
            Assert.Contains(faIndefiniteAdjectiveDetail.Examples, example => example.GermanText == "Ich trinke einen guten Kaffee.");
            Assert.Contains(faIndefiniteAdjectiveDetail.Examples, example => example.GermanText == "Mein alter Vertrag endet bald.");
            Assert.Contains(faIndefiniteAdjectiveDetail.CommonMistakes, mistake => mistake.WrongText == "ein gute Kaffee");
            Assert.Contains(faIndefiniteAdjectiveDetail.CommonMistakes, mistake => mistake.WrongText == "einen wichtige Termin");
            Assert.Contains(faIndefiniteAdjectiveDetail.CommonMistakes, mistake => mistake.WrongText == "mit keine alten Bücher");

            GrammarTopicDetailModel? faNoArticleAdjectiveDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-adjective-declension-without-article",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faNoArticleAdjectiveDetail);
            Assert.Equal("b1-adjective-declension-without-article", faNoArticleAdjectiveDetail!.Slug);
            Assert.Equal("B1", faNoArticleAdjectiveDetail.CefrLevel);
            Assert.Equal("adjective-declension", faNoArticleAdjectiveDetail.GrammarCategory);
            Assert.Equal(14, faNoArticleAdjectiveDetail.Sections.Count);
            Assert.True(faNoArticleAdjectiveDetail.Examples.Count >= 150);
            Assert.True(faNoArticleAdjectiveDetail.CommonMistakes.Count >= 55);
            Assert.True(faNoArticleAdjectiveDetail.RuleSummaries.Count >= 25);
            Assert.Contains("b1-adjective-declension-after-definite-article", faNoArticleAdjectiveDetail.PrerequisiteSlugs);
            Assert.Contains("b1-adjective-declension-after-indefinite-article", faNoArticleAdjectiveDetail.PrerequisiteSlugs);
            Assert.Contains("b2-adjective-declension-advanced-review", faNoArticleAdjectiveDetail.RelatedTopicSlugs);
            Assert.Contains(
                faNoArticleAdjectiveDetail.Sections,
                section => section.SectionKey == "nominative-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faNoArticleAdjectiveDetail.Sections,
                section => section.SectionKey == "accusative-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faNoArticleAdjectiveDetail.Sections,
                section => section.SectionKey == "dative-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faNoArticleAdjectiveDetail.Sections,
                section => section.SectionKey == "comparison-with-definite-and-indefinite" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faNoArticleAdjectiveDetail.Sections,
                section => section.SectionKey == "dative-with-prepositions" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faNoArticleAdjectiveDetail.Sections,
                section => section.SectionKey == "formal-and-written-use" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faNoArticleAdjectiveDetail.Examples, example => example.GermanText == "Guter Kaffee hilft mir am Morgen.");
            Assert.Contains(faNoArticleAdjectiveDetail.Examples, example => example.GermanText == "Ich trinke guten Kaffee.");
            Assert.Contains(faNoArticleAdjectiveDetail.Examples, example => example.GermanText == "Mit gutem Kaffee beginnt der Tag besser.");
            Assert.Contains(faNoArticleAdjectiveDetail.CommonMistakes, mistake => mistake.WrongText == "gut Kaffee");
            Assert.Contains(faNoArticleAdjectiveDetail.CommonMistakes, mistake => mistake.WrongText == "frische Brot");
            Assert.Contains(faNoArticleAdjectiveDetail.CommonMistakes, mistake => mistake.WrongText == "mit guter Kaffee");

            GrammarTopicDetailModel? faGenitiveDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-genitive-introduction",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faGenitiveDetail);
            Assert.Equal("b1-genitive-introduction", faGenitiveDetail!.Slug);
            Assert.Equal("B1", faGenitiveDetail.CefrLevel);
            Assert.Equal("genitive", faGenitiveDetail.GrammarCategory);
            Assert.Equal(14, faGenitiveDetail.Sections.Count);
            Assert.True(faGenitiveDetail.Examples.Count >= 130);
            Assert.True(faGenitiveDetail.CommonMistakes.Count >= 45);
            Assert.True(faGenitiveDetail.RuleSummaries.Count >= 22);
            Assert.Contains("a2-a2-case-review", faGenitiveDetail.PrerequisiteSlugs);
            Assert.Contains("b1-b1-case-review", faGenitiveDetail.PrerequisiteSlugs);
            Assert.Contains("b2-dative-and-genitive-prepositions", faGenitiveDetail.RelatedTopicSlugs);
            Assert.Contains(
                faGenitiveDetail.Sections,
                section => section.SectionKey == "genitive-article-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faGenitiveDetail.Sections,
                section => section.SectionKey == "noun-ending-s" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faGenitiveDetail.Sections,
                section => section.SectionKey == "genitive-prepositions-intro" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faGenitiveDetail.Sections,
                section => section.SectionKey == "genitive-vs-von" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faGenitiveDetail.Examples, example => example.GermanText == "Das Auto des Mannes ist alt.");
            Assert.Contains(faGenitiveDetail.Examples, example => example.GermanText == "Wegen des Termins rufe ich an.");
            Assert.Contains(faGenitiveDetail.Examples, example => example.GermanText == "Aufgrund des Wetters fällt der Termin aus.");
            Assert.Contains(faGenitiveDetail.CommonMistakes, mistake => mistake.WrongText == "das Auto der Mann");
            Assert.Contains(faGenitiveDetail.CommonMistakes, mistake => mistake.WrongText == "des Frau");
            Assert.Contains(faGenitiveDetail.CommonMistakes, mistake => mistake.WrongText == "wegen der Termin");

            GrammarTopicDetailModel? faPrepositionalVerbsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-prepositional-verbs-introduction",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faPrepositionalVerbsDetail);
            Assert.Equal("b1-prepositional-verbs-introduction", faPrepositionalVerbsDetail!.Slug);
            Assert.Equal("B1", faPrepositionalVerbsDetail.CefrLevel);
            Assert.Equal("prepositions", faPrepositionalVerbsDetail.GrammarCategory);
            Assert.Equal(15, faPrepositionalVerbsDetail.Sections.Count);
            Assert.True(faPrepositionalVerbsDetail.Examples.Count >= 150);
            Assert.True(faPrepositionalVerbsDetail.CommonMistakes.Count >= 55);
            Assert.True(faPrepositionalVerbsDetail.RuleSummaries.Count >= 24);
            Assert.Contains("a2-prepositions-with-dative", faPrepositionalVerbsDetail.PrerequisiteSlugs);
            Assert.Contains("b1-indirect-questions", faPrepositionalVerbsDetail.PrerequisiteSlugs);
            Assert.Contains("b1-verb-plus-preposition-combinations", faPrepositionalVerbsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faPrepositionalVerbsDetail.Sections,
                section => section.SectionKey == "common-accusative-combinations" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faPrepositionalVerbsDetail.Sections,
                section => section.SectionKey == "common-dative-combinations" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faPrepositionalVerbsDetail.Sections,
                section => section.SectionKey == "questions-with-worauf-worueber-woran" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faPrepositionalVerbsDetail.Sections,
                section => section.SectionKey == "questions-with-auf-wen-mit-wem" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faPrepositionalVerbsDetail.Sections,
                section => section.SectionKey == "mini-dictionary-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faPrepositionalVerbsDetail.Examples, example => example.GermanText == "Ich warte auf den Bus.");
            Assert.Contains(faPrepositionalVerbsDetail.Examples, example => example.GermanText == "Worüber sprecht ihr?");
            Assert.Contains(faPrepositionalVerbsDetail.Examples, example => example.GermanText == "Ich bedanke mich für Ihre Hilfe.");
            Assert.Contains(faPrepositionalVerbsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich warte für den Bus.");
            Assert.Contains(faPrepositionalVerbsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich spreche auf das Thema.");
            Assert.Contains(faPrepositionalVerbsDetail.CommonMistakes, mistake => mistake.WrongText == "Was wartest du auf?");

            GrammarTopicDetailModel? faVerbPrepositionDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-verb-plus-preposition-combinations",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faVerbPrepositionDetail);
            Assert.Equal("b1-verb-plus-preposition-combinations", faVerbPrepositionDetail!.Slug);
            Assert.Equal("B1", faVerbPrepositionDetail.CefrLevel);
            Assert.Equal("prepositions", faVerbPrepositionDetail.GrammarCategory);
            Assert.Equal(15, faVerbPrepositionDetail.Sections.Count);
            Assert.True(faVerbPrepositionDetail.Examples.Count >= 160);
            Assert.True(faVerbPrepositionDetail.CommonMistakes.Count >= 60);
            Assert.True(faVerbPrepositionDetail.RuleSummaries.Count >= 24);
            Assert.Contains("b1-prepositional-verbs-introduction", faVerbPrepositionDetail.PrerequisiteSlugs);
            Assert.Contains("a2-prepositions-with-accusative", faVerbPrepositionDetail.PrerequisiteSlugs);
            Assert.Contains("b1-formal-email-sentence-structure", faVerbPrepositionDetail.RelatedTopicSlugs);
            Assert.Contains(
                faVerbPrepositionDetail.Sections,
                section => section.SectionKey == "accusative-combinations-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faVerbPrepositionDetail.Sections,
                section => section.SectionKey == "dative-combinations-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faVerbPrepositionDetail.Sections,
                section => section.SectionKey == "person-question-forms" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faVerbPrepositionDetail.Sections,
                section => section.SectionKey == "thing-question-forms" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faVerbPrepositionDetail.Sections,
                section => section.SectionKey == "preposition-case-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faVerbPrepositionDetail.Examples, example => example.GermanText == "Ich warte auf den Bus.");
            Assert.Contains(faVerbPrepositionDetail.Examples, example => example.GermanText == "Ich freue mich auf den Urlaub nächste Woche.");
            Assert.Contains(faVerbPrepositionDetail.Examples, example => example.GermanText == "Ich bedanke mich für Ihre Hilfe.");
            Assert.Contains(faVerbPrepositionDetail.CommonMistakes, mistake => mistake.WrongText == "Ich warte für den Bus.");
            Assert.Contains(faVerbPrepositionDetail.CommonMistakes, mistake => mistake.WrongText == "Ich freue mich über den Urlaub nächste Woche.");
            Assert.Contains(faVerbPrepositionDetail.CommonMistakes, mistake => mistake.WrongText == "Ich bedanke mich bei die Hilfe.");

            GrammarTopicDetailModel? faNounVerbPhrasesDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-noun-verb-phrases",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faNounVerbPhrasesDetail);
            Assert.Equal("b1-noun-verb-phrases", faNounVerbPhrasesDetail!.Slug);
            Assert.Equal("B1", faNounVerbPhrasesDetail.CefrLevel);
            Assert.Equal("verbs", faNounVerbPhrasesDetail.GrammarCategory);
            Assert.Equal(15, faNounVerbPhrasesDetail.Sections.Count);
            Assert.True(faNounVerbPhrasesDetail.Examples.Count >= 170);
            Assert.True(faNounVerbPhrasesDetail.CommonMistakes.Count >= 60);
            Assert.True(faNounVerbPhrasesDetail.RuleSummaries.Count >= 24);
            Assert.Contains("a2-grammar-for-appointments", faNounVerbPhrasesDetail.PrerequisiteSlugs);
            Assert.Contains("b1-verb-plus-preposition-combinations", faNounVerbPhrasesDetail.PrerequisiteSlugs);
            Assert.Contains("b1-formal-email-sentence-structure", faNounVerbPhrasesDetail.RelatedTopicSlugs);
            Assert.Contains(
                faNounVerbPhrasesDetail.Sections,
                section => section.SectionKey == "appointment-phrases" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faNounVerbPhrasesDetail.Sections,
                section => section.SectionKey == "application-and-office-phrases" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faNounVerbPhrasesDetail.Sections,
                section => section.SectionKey == "decision-and-plan-phrases" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faNounVerbPhrasesDetail.Sections,
                section => section.SectionKey == "help-and-support-phrases" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faNounVerbPhrasesDetail.Sections,
                section => section.SectionKey == "complaint-and-problem-phrases" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faNounVerbPhrasesDetail.Sections,
                section => section.SectionKey == "chunk-learning-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faNounVerbPhrasesDetail.Examples, example => example.GermanText == "Ich möchte einen Termin vereinbaren.");
            Assert.Contains(faNounVerbPhrasesDetail.Examples, example => example.GermanText == "Darf ich eine Frage stellen?");
            Assert.Contains(faNounVerbPhrasesDetail.Examples, example => example.GermanText == "Ich muss einen Antrag stellen.");
            Assert.Contains(faNounVerbPhrasesDetail.CommonMistakes, mistake => mistake.WrongText == "Ich mache einen Antrag.");
            Assert.Contains(faNounVerbPhrasesDetail.CommonMistakes, mistake => mistake.WrongText == "Ich stelle einen Termin.");
            Assert.Contains(faNounVerbPhrasesDetail.CommonMistakes, mistake => mistake.WrongText == "Ich mache eine Entscheidung.");

            GrammarTopicDetailModel? faOpinionConnectorsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-connectors-for-opinion",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faOpinionConnectorsDetail);
            Assert.Equal("b1-connectors-for-opinion", faOpinionConnectorsDetail!.Slug);
            Assert.Equal("B1", faOpinionConnectorsDetail.CefrLevel);
            Assert.Equal("connectors", faOpinionConnectorsDetail.GrammarCategory);
            Assert.Equal(14, faOpinionConnectorsDetail.Sections.Count);
            Assert.True(faOpinionConnectorsDetail.Examples.Count >= 160);
            Assert.True(faOpinionConnectorsDetail.CommonMistakes.Count >= 55);
            Assert.True(faOpinionConnectorsDetail.RuleSummaries.Count >= 24);
            Assert.Contains("a2-a2-connectors-overview", faOpinionConnectorsDetail.PrerequisiteSlugs);
            Assert.Contains("b1-weil-obwohl-trotzdem", faOpinionConnectorsDetail.PrerequisiteSlugs);
            Assert.Contains("b1-connectors-for-contrast", faOpinionConnectorsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faOpinionConnectorsDetail.Sections,
                section => section.SectionKey == "basic-opinion-frames" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faOpinionConnectorsDetail.Sections,
                section => section.SectionKey == "agreeing" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faOpinionConnectorsDetail.Sections,
                section => section.SectionKey == "disagreeing-politely" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faOpinionConnectorsDetail.Sections,
                section => section.SectionKey == "structuring-an-answer" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faOpinionConnectorsDetail.Sections,
                section => section.SectionKey == "connector-word-order" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faOpinionConnectorsDetail.Examples, example => example.GermanText == "Ich finde, dass Deutsch wichtig ist.");
            Assert.Contains(faOpinionConnectorsDetail.Examples, example => example.GermanText == "Ich stimme Ihnen zu.");
            Assert.Contains(faOpinionConnectorsDetail.Examples, example => example.GermanText == "Teilweise stimme ich zu, aber es ist teuer.");
            Assert.Contains(faOpinionConnectorsDetail.CommonMistakes, mistake => mistake.WrongText == "Meiner Meinung nach, es ist wichtig.");
            Assert.Contains(faOpinionConnectorsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich stimme zu dir.");
            Assert.Contains(faOpinionConnectorsDetail.CommonMistakes, mistake => mistake.WrongText == "Deshalb ich finde das gut.");

            GrammarTopicDetailModel? faContrastConnectorsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-connectors-for-contrast",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faContrastConnectorsDetail);
            Assert.Equal("b1-connectors-for-contrast", faContrastConnectorsDetail!.Slug);
            Assert.Equal("B1", faContrastConnectorsDetail.CefrLevel);
            Assert.Equal("connectors", faContrastConnectorsDetail.GrammarCategory);
            Assert.Equal(14, faContrastConnectorsDetail.Sections.Count);
            Assert.True(faContrastConnectorsDetail.Examples.Count >= 160);
            Assert.True(faContrastConnectorsDetail.CommonMistakes.Count >= 60);
            Assert.True(faContrastConnectorsDetail.RuleSummaries.Count >= 24);
            Assert.Contains("b1-connectors-for-opinion", faContrastConnectorsDetail.PrerequisiteSlugs);
            Assert.Contains("b1-connectors-for-cause-and-effect", faContrastConnectorsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faContrastConnectorsDetail.Sections,
                section => section.SectionKey == "meaning-comparison-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faContrastConnectorsDetail.Sections,
                section => section.SectionKey == "word-order-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faContrastConnectorsDetail.Sections,
                section => section.SectionKey == "contrast-with-modal-verbs" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faContrastConnectorsDetail.Sections,
                section => section.SectionKey == "choosing-the-right-connector" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faContrastConnectorsDetail.Examples, example => example.GermanText == "Der Kurs ist gut, aber teuer.");
            Assert.Contains(faContrastConnectorsDetail.Examples, example => example.GermanText == "Ich gehe zur Arbeit, obwohl ich krank bin.");
            Assert.Contains(faContrastConnectorsDetail.Examples, example => example.GermanText == "Ich bin krank. Trotzdem gehe ich zur Arbeit.");
            Assert.Contains(faContrastConnectorsDetail.CommonMistakes, mistake => mistake.WrongText == "Obwohl ich krank bin, ich gehe zur Arbeit.");
            Assert.Contains(faContrastConnectorsDetail.CommonMistakes, mistake => mistake.WrongText == "Trotzdem ich gehe zur Arbeit.");
            Assert.Contains(faContrastConnectorsDetail.CommonMistakes, mistake => mistake.WrongText == "Der Kurs ist gut, obwohl teuer.");

            GrammarTopicDetailModel? faCauseEffectConnectorsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-connectors-for-cause-and-effect",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faCauseEffectConnectorsDetail);
            Assert.Equal("b1-connectors-for-cause-and-effect", faCauseEffectConnectorsDetail!.Slug);
            Assert.Equal("B1", faCauseEffectConnectorsDetail.CefrLevel);
            Assert.Equal("connectors", faCauseEffectConnectorsDetail.GrammarCategory);
            Assert.Equal(15, faCauseEffectConnectorsDetail.Sections.Count);
            Assert.True(faCauseEffectConnectorsDetail.Examples.Count >= 170);
            Assert.True(faCauseEffectConnectorsDetail.CommonMistakes.Count >= 65);
            Assert.True(faCauseEffectConnectorsDetail.RuleSummaries.Count >= 26);
            Assert.Contains("a2-weil-clauses", faCauseEffectConnectorsDetail.PrerequisiteSlugs);
            Assert.Contains("b1-connectors-for-opinion", faCauseEffectConnectorsDetail.PrerequisiteSlugs);
            Assert.Contains("b1-weil-obwohl-trotzdem", faCauseEffectConnectorsDetail.PrerequisiteSlugs);
            Assert.Contains(
                faCauseEffectConnectorsDetail.Sections,
                section => section.SectionKey == "cause-vs-effect-direction" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faCauseEffectConnectorsDetail.Sections,
                section => section.SectionKey == "word-order-comparison-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faCauseEffectConnectorsDetail.Sections,
                section => section.SectionKey == "formal-vs-spoken-register" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faCauseEffectConnectorsDetail.Sections,
                section => section.SectionKey == "cause-effect-with-modal-verbs" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faCauseEffectConnectorsDetail.Examples, example => example.GermanText == "Ich komme nicht, weil ich krank bin.");
            Assert.Contains(faCauseEffectConnectorsDetail.Examples, example => example.GermanText == "Ich bin krank. Deshalb komme ich nicht.");
            Assert.Contains(faCauseEffectConnectorsDetail.Examples, example => example.GermanText == "Aus diesem Grund möchte ich den Termin verschieben.");
            Assert.Contains(faCauseEffectConnectorsDetail.CommonMistakes, mistake => mistake.WrongText == "Deshalb ich komme nicht.");
            Assert.Contains(faCauseEffectConnectorsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich komme nicht, weil ich bin krank.");
            Assert.Contains(faCauseEffectConnectorsDetail.CommonMistakes, mistake => mistake.WrongText == "Wegen ich bin krank.");

            GrammarTopicDetailModel? faMultipleClauseOrderDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-sentence-order-with-multiple-clauses",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faMultipleClauseOrderDetail);
            Assert.Equal("b1-sentence-order-with-multiple-clauses", faMultipleClauseOrderDetail!.Slug);
            Assert.Equal("B1", faMultipleClauseOrderDetail.CefrLevel);
            Assert.Equal("word-order", faMultipleClauseOrderDetail.GrammarCategory);
            Assert.Equal(15, faMultipleClauseOrderDetail.Sections.Count);
            Assert.True(faMultipleClauseOrderDetail.Examples.Count >= 170);
            Assert.True(faMultipleClauseOrderDetail.CommonMistakes.Count >= 65);
            Assert.True(faMultipleClauseOrderDetail.RuleSummaries.Count >= 28);
            Assert.Contains("a2-sentence-order-in-subordinate-clauses", faMultipleClauseOrderDetail.PrerequisiteSlugs);
            Assert.Contains("b1-relative-clauses-basics", faMultipleClauseOrderDetail.PrerequisiteSlugs);
            Assert.Contains("b1-connectors-for-cause-and-effect", faMultipleClauseOrderDetail.PrerequisiteSlugs);
            Assert.Contains(
                faMultipleClauseOrderDetail.Sections,
                section => section.SectionKey == "connector-type-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faMultipleClauseOrderDetail.Sections,
                section => section.SectionKey == "subordinate-clause-first" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faMultipleClauseOrderDetail.Sections,
                section => section.SectionKey == "relative-clause-in-sentence" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faMultipleClauseOrderDetail.Sections,
                section => section.SectionKey == "common-sentence-patterns" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faMultipleClauseOrderDetail.Examples, example => example.GermanText == "Weil ich krank bin, bleibe ich zu Hause.");
            Assert.Contains(faMultipleClauseOrderDetail.Examples, example => example.GermanText == "Der Mann, der hier arbeitet, ist mein Nachbar.");
            Assert.Contains(faMultipleClauseOrderDetail.Examples, example => example.GermanText == "Ich kenne eine Frau, die Deutsch lernt, weil sie in Deutschland arbeitet.");
            Assert.Contains(faMultipleClauseOrderDetail.CommonMistakes, mistake => mistake.WrongText == "Weil ich krank bin, ich bleibe zu Hause.");
            Assert.Contains(faMultipleClauseOrderDetail.CommonMistakes, mistake => mistake.WrongText == "Ich glaube, dass er kommt morgen.");
            Assert.Contains(faMultipleClauseOrderDetail.CommonMistakes, mistake => mistake.WrongText == "Ich kenne eine Frau, die lernt Deutsch.");

            GrammarTopicDetailModel? faFormalEmailDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-formal-email-sentence-structure",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faFormalEmailDetail);
            Assert.Equal("b1-formal-email-sentence-structure", faFormalEmailDetail!.Slug);
            Assert.Equal("B1", faFormalEmailDetail.CefrLevel);
            Assert.Equal("word-order", faFormalEmailDetail.GrammarCategory);
            Assert.Equal(16, faFormalEmailDetail.Sections.Count);
            Assert.True(faFormalEmailDetail.Examples.Count >= 180);
            Assert.True(faFormalEmailDetail.CommonMistakes.Count >= 70);
            Assert.True(faFormalEmailDetail.RuleSummaries.Count >= 28);
            Assert.Contains("a2-simple-email-grammar", faFormalEmailDetail.PrerequisiteSlugs);
            Assert.Contains("b1-sentence-order-with-multiple-clauses", faFormalEmailDetail.PrerequisiteSlugs);
            Assert.Contains("b1-connectors-for-cause-and-effect", faFormalEmailDetail.PrerequisiteSlugs);
            Assert.Contains(
                faFormalEmailDetail.Sections,
                section => section.SectionKey == "email-structure-map" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faFormalEmailDetail.Sections,
                section => section.SectionKey == "making-a-request" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faFormalEmailDetail.Sections,
                section => section.SectionKey == "sie-ihnen-ihr-ihre" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faFormalEmailDetail.Sections,
                section => section.SectionKey == "sample-email-analysis" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faFormalEmailDetail.Examples, example => example.GermanText == "Sehr geehrte Damen und Herren,");
            Assert.Contains(faFormalEmailDetail.Examples, example => example.GermanText == "Ich schreibe Ihnen, weil ich eine Frage habe.");
            Assert.Contains(faFormalEmailDetail.Examples, example => example.GermanText == "Könnten Sie mir bitte die Unterlagen schicken?");
            Assert.Contains(faFormalEmailDetail.Examples, example => example.GermanText == "Im Anhang finden Sie das Formular.");
            Assert.Contains(faFormalEmailDetail.CommonMistakes, mistake => mistake.WrongText == "Ich schreibe Sie wegen des Termins.");
            Assert.Contains(faFormalEmailDetail.CommonMistakes, mistake => mistake.WrongText == "Können Sie sagen mir, wann der Termin ist?");
            Assert.Contains(faFormalEmailDetail.CommonMistakes, mistake => mistake.WrongText == "Ich bitte Ihnen, mir zu antworten.");

            GrammarTopicDetailModel? faComplaintPatternsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-complaint-sentence-patterns",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faComplaintPatternsDetail);
            Assert.Equal("b1-complaint-sentence-patterns", faComplaintPatternsDetail!.Slug);
            Assert.Equal("B1", faComplaintPatternsDetail.CefrLevel);
            Assert.Equal("word-order", faComplaintPatternsDetail.GrammarCategory);
            Assert.Equal(15, faComplaintPatternsDetail.Sections.Count);
            Assert.True(faComplaintPatternsDetail.Examples.Count >= 180);
            Assert.True(faComplaintPatternsDetail.CommonMistakes.Count >= 70);
            Assert.True(faComplaintPatternsDetail.RuleSummaries.Count >= 28);
            Assert.Contains("b1-formal-email-sentence-structure", faComplaintPatternsDetail.PrerequisiteSlugs);
            Assert.Contains("b1-connectors-for-cause-and-effect", faComplaintPatternsDetail.PrerequisiteSlugs);
            Assert.Contains("b1-sentence-order-with-multiple-clauses", faComplaintPatternsDetail.PrerequisiteSlugs);
            Assert.Contains(
                faComplaintPatternsDetail.Sections,
                section => section.SectionKey == "why-complaint-patterns-matter" && section.Blocks.Any(block => block.Type == "callout"));
            Assert.Contains(
                faComplaintPatternsDetail.Sections,
                section => section.SectionKey == "complaint-structure-overview" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faComplaintPatternsDetail.Sections,
                section => section.SectionKey == "polite-request-for-solution" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faComplaintPatternsDetail.Sections,
                section => section.SectionKey == "complaint-email-pattern-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faComplaintPatternsDetail.Examples, example => example.GermanText == "Leider gibt es ein Problem mit der Heizung.");
            Assert.Contains(faComplaintPatternsDetail.Examples, example => example.GermanText == "Ich habe festgestellt, dass die Heizung nicht funktioniert.");
            Assert.Contains(faComplaintPatternsDetail.Examples, example => example.GermanText == "Könnten Sie bitte die Heizung reparieren?");
            Assert.Contains(faComplaintPatternsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich will, dass Sie sofort antworten.");
            Assert.Contains(faComplaintPatternsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich habe Problem.");
            Assert.Contains(faComplaintPatternsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich habe festgestellt, dass die Heizung funktioniert nicht.");

            GrammarTopicDetailModel? faGivingReasonsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-giving-reasons-clearly",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faGivingReasonsDetail);
            Assert.Equal("b1-giving-reasons-clearly", faGivingReasonsDetail!.Slug);
            Assert.Equal("B1", faGivingReasonsDetail.CefrLevel);
            Assert.Equal("connectors", faGivingReasonsDetail.GrammarCategory);
            Assert.Equal(14, faGivingReasonsDetail.Sections.Count);
            Assert.True(faGivingReasonsDetail.Examples.Count >= 170);
            Assert.True(faGivingReasonsDetail.CommonMistakes.Count >= 65);
            Assert.True(faGivingReasonsDetail.RuleSummaries.Count >= 26);
            Assert.Contains("b1-connectors-for-cause-and-effect", faGivingReasonsDetail.PrerequisiteSlugs);
            Assert.Contains("b1-connectors-for-opinion", faGivingReasonsDetail.PrerequisiteSlugs);
            Assert.Contains("b1-sentence-order-with-multiple-clauses", faGivingReasonsDetail.PrerequisiteSlugs);
            Assert.Contains(
                faGivingReasonsDetail.Sections,
                section => section.SectionKey == "reason-chain" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faGivingReasonsDetail.Sections,
                section => section.SectionKey == "word-order-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faGivingReasonsDetail.Sections,
                section => section.SectionKey == "common-pattern-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faGivingReasonsDetail.Examples, example => example.GermanText == "Ich lerne Deutsch, weil ich in Deutschland arbeite.");
            Assert.Contains(faGivingReasonsDetail.Examples, example => example.GermanText == "Ich arbeite in Deutschland. Deshalb lerne ich Deutsch.");
            Assert.Contains(faGivingReasonsDetail.Examples, example => example.GermanText == "Der Grund ist, dass ich morgen arbeiten muss.");
            Assert.Contains(faGivingReasonsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich denke das, weil es ist wichtig.");
            Assert.Contains(faGivingReasonsDetail.CommonMistakes, mistake => mistake.WrongText == "Deshalb ich lerne Deutsch.");
            Assert.Contains(faGivingReasonsDetail.CommonMistakes, mistake => mistake.WrongText == "Wegen ich krank bin.");

            GrammarTopicDetailModel? faAgreeDisagreeDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-agreeing-and-disagreeing-grammatically",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faAgreeDisagreeDetail);
            Assert.Equal("b1-agreeing-and-disagreeing-grammatically", faAgreeDisagreeDetail!.Slug);
            Assert.Equal("B1", faAgreeDisagreeDetail.CefrLevel);
            Assert.Equal("connectors", faAgreeDisagreeDetail.GrammarCategory);
            Assert.Equal(15, faAgreeDisagreeDetail.Sections.Count);
            Assert.True(faAgreeDisagreeDetail.Examples.Count >= 170);
            Assert.True(faAgreeDisagreeDetail.CommonMistakes.Count >= 65);
            Assert.True(faAgreeDisagreeDetail.RuleSummaries.Count >= 26);
            Assert.Contains("b1-connectors-for-opinion", faAgreeDisagreeDetail.PrerequisiteSlugs);
            Assert.Contains("b1-connectors-for-contrast", faAgreeDisagreeDetail.PrerequisiteSlugs);
            Assert.Contains("b1-giving-reasons-clearly", faAgreeDisagreeDetail.PrerequisiteSlugs);
            Assert.Contains(
                faAgreeDisagreeDetail.Sections,
                section => section.SectionKey == "simple-agreement" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faAgreeDisagreeDetail.Sections,
                section => section.SectionKey == "simple-disagreement" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faAgreeDisagreeDetail.Sections,
                section => section.SectionKey == "partial-agreement" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faAgreeDisagreeDetail.Sections,
                section => section.SectionKey == "du-vs-sie-in-discussion" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faAgreeDisagreeDetail.Sections,
                section => section.SectionKey == "b1-speaking-exam-pattern" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faAgreeDisagreeDetail.Examples, example => example.GermanText == "Ich stimme dir zu.");
            Assert.Contains(faAgreeDisagreeDetail.Examples, example => example.GermanText == "Ich sehe das anders.");
            Assert.Contains(faAgreeDisagreeDetail.Examples, example => example.GermanText == "Teilweise stimme ich zu, aber es gibt auch Nachteile.");
            Assert.Contains(faAgreeDisagreeDetail.CommonMistakes, mistake => mistake.WrongText == "Ich stimme mit dir zu.");
            Assert.Contains(faAgreeDisagreeDetail.CommonMistakes, mistake => mistake.WrongText == "Ich bin nicht agree.");
            Assert.Contains(faAgreeDisagreeDetail.CommonMistakes, mistake => mistake.WrongText == "Ich sehe anders.");

            GrammarTopicDetailModel? faPastExperienceDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-describing-experiences-in-the-past",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faPastExperienceDetail);
            Assert.Equal("b1-describing-experiences-in-the-past", faPastExperienceDetail!.Slug);
            Assert.Equal("B1", faPastExperienceDetail.CefrLevel);
            Assert.Equal("tenses", faPastExperienceDetail.GrammarCategory);
            Assert.Equal(15, faPastExperienceDetail.Sections.Count);
            Assert.True(faPastExperienceDetail.Examples.Count >= 170);
            Assert.True(faPastExperienceDetail.CommonMistakes.Count >= 65);
            Assert.True(faPastExperienceDetail.RuleSummaries.Count >= 26);
            Assert.Contains("a2-perfekt-with-haben", faPastExperienceDetail.PrerequisiteSlugs);
            Assert.Contains("a2-perfekt-with-sein", faPastExperienceDetail.PrerequisiteSlugs);
            Assert.Contains("b1-als-versus-wenn", faPastExperienceDetail.PrerequisiteSlugs);
            Assert.Contains(
                faPastExperienceDetail.Sections,
                section => section.SectionKey == "core-story-structure" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faPastExperienceDetail.Sections,
                section => section.SectionKey == "perfekt-review" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faPastExperienceDetail.Sections,
                section => section.SectionKey == "war-and-hatte" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faPastExperienceDetail.Sections,
                section => section.SectionKey == "sequence-connectors" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faPastExperienceDetail.Examples, example => example.GermanText == "Ich habe Deutsch gelernt.");
            Assert.Contains(faPastExperienceDetail.Examples, example => example.GermanText == "Ich bin in Deutschland angekommen.");
            Assert.Contains(faPastExperienceDetail.Examples, example => example.GermanText == "Als ich nach Deutschland gekommen bin, war ich nervös.");
            Assert.Contains(faPastExperienceDetail.CommonMistakes, mistake => mistake.WrongText == "Ich habe nach Deutschland gekommen.");
            Assert.Contains(faPastExperienceDetail.CommonMistakes, mistake => mistake.WrongText == "Ich bin Deutsch gelernt.");
            Assert.Contains(faPastExperienceDetail.CommonMistakes, mistake => mistake.WrongText == "Ich war einen Termin.");

            GrammarTopicDetailModel? faPlansConditionsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "b1-talking-about-plans-and-conditions",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faPlansConditionsDetail);
            Assert.Equal("b1-talking-about-plans-and-conditions", faPlansConditionsDetail!.Slug);
            Assert.Equal("B1", faPlansConditionsDetail.CefrLevel);
            Assert.Equal("subordinate-clauses", faPlansConditionsDetail.GrammarCategory);
            Assert.Equal(15, faPlansConditionsDetail.Sections.Count);
            Assert.True(faPlansConditionsDetail.Examples.Count >= 170);
            Assert.True(faPlansConditionsDetail.CommonMistakes.Count >= 65);
            Assert.True(faPlansConditionsDetail.RuleSummaries.Count >= 26);
            Assert.Contains("a2-wenn-for-conditions", faPlansConditionsDetail.PrerequisiteSlugs);
            Assert.Contains("b1-konjunktiv-ii-with-waere-haette-wuerde", faPlansConditionsDetail.PrerequisiteSlugs);
            Assert.Contains("b1-giving-reasons-clearly", faPlansConditionsDetail.PrerequisiteSlugs);
            Assert.Contains(
                faPlansConditionsDetail.Sections,
                section => section.SectionKey == "wenn-vs-falls" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faPlansConditionsDetail.Sections,
                section => section.SectionKey == "subordinate-clause-first" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faPlansConditionsDetail.Sections,
                section => section.SectionKey == "real-vs-hypothetical-condition" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(
                faPlansConditionsDetail.Sections,
                section => section.SectionKey == "common-pattern-table" && section.Blocks.Any(block => block.Type == "table"));
            Assert.Contains(faPlansConditionsDetail.Examples, example => example.GermanText == "Morgen gehe ich zum Kurs.");
            Assert.Contains(faPlansConditionsDetail.Examples, example => example.GermanText == "Wenn ich Zeit habe, komme ich.");
            Assert.Contains(faPlansConditionsDetail.Examples, example => example.GermanText == "Falls Sie Fragen haben, schreiben Sie mir bitte.");
            Assert.Contains(faPlansConditionsDetail.Examples, example => example.GermanText == "Wenn ich Zeit hätte, würde ich kommen.");
            Assert.Contains(faPlansConditionsDetail.CommonMistakes, mistake => mistake.WrongText == "Wenn ich Zeit habe, ich komme.");
            Assert.Contains(faPlansConditionsDetail.CommonMistakes, mistake => mistake.WrongText == "Falls Sie Fragen haben, Sie schreiben mir.");
            Assert.Contains(faPlansConditionsDetail.CommonMistakes, mistake => mistake.WrongText == "Wenn ich Zeit hätte, ich würde kommen.");

            await using DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext dbContext = serviceProvider
                .GetRequiredService<IDbContextFactory<DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext>>()
                .CreateDbContext();
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-relative-clauses-basics"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-relative-pronouns-in-nominative-and-accusative"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-relative-pronouns-in-dative"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-konjunktiv-ii-for-polite-requests"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-konjunktiv-ii-with-waere-haette-wuerde"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-passive-voice-introduction"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-werden-as-auxiliary"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-infinitive-with-zu"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-um-zu"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-damit-versus-um-zu"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-weil-obwohl-trotzdem"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-als-versus-wenn"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-nachdem-bevor-waehrend"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-indirect-questions"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-reported-requests-and-polite-questions"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-adjective-declension-after-definite-article"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-adjective-declension-after-indefinite-article"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-adjective-declension-without-article"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-genitive-introduction"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-prepositional-verbs-introduction"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-verb-plus-preposition-combinations"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-noun-verb-phrases"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-connectors-for-opinion"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-connectors-for-contrast"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-connectors-for-cause-and-effect"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-sentence-order-with-multiple-clauses"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-formal-email-sentence-structure"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-complaint-sentence-patterns"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-giving-reasons-clearly"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-agreeing-and-disagreeing-grammatically"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-describing-experiences-in-the-past"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "b1-talking-about-plans-and-conditions"));
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(databasePath))
            {
                TryDeleteFile(databasePath);
            }
        }
    }

    /// <summary>
    /// Verifies that invalid entries are reported while valid entries in the same package are still imported.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldReportInvalidEntriesAndImportValidEntries()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreatePackageWithInvalidEntryJson("a1-shopping-import-invalid-mixed"));
            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
            Assert.Equal("CompletedWithWarnings", result.Status);
            Assert.Equal(2, result.TotalEntries);
            Assert.Equal(1, result.ImportedEntries);
            Assert.Equal(1, result.InvalidEntries);
            Assert.Equal(0, result.SkippedDuplicateEntries);
            Assert.Contains(result.Issues, issue => issue.EntryIndex == 2 && issue.Severity == "Error");
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    /// <summary>
    /// Verifies that duplicate entries inside one package are skipped with warning accounting.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldSkipDuplicateEntriesWithinSinglePackage()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreatePackageWithDuplicateEntriesJson("a1-shopping-import-duplicate-entry"));
            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
            Assert.Equal("CompletedWithWarnings", result.Status);
            Assert.Equal(2, result.TotalEntries);
            Assert.Equal(1, result.ImportedEntries);
            Assert.Equal(1, result.SkippedDuplicateEntries);
            Assert.Equal(0, result.InvalidEntries);
            Assert.Equal(1, result.WarningCount);
            Assert.Contains(result.Issues, issue => issue.EntryIndex == 2 && issue.Severity == "Warning");
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    [Fact]
    public async Task ImportAsync_ShouldImportOfficialA1GrammarCoreAndUpsertBySlug()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-grammar-rich-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(ResolveRepositoryRoot(), "content", "learning-portal", "grammar", "packages", "grammar-a1-core-v1.json");
        string updatePackagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-grammar-rich-update-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));

            IGrammarTopicQueryService grammarQueryService = serviceProvider.GetRequiredService<IGrammarTopicQueryService>();
            GrammarTopicDetailModel? faDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-personal-pronouns-ich-du-er-sie-es",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faDetail);
            Assert.Contains("ضمیر", faDetail!.Title, StringComparison.Ordinal);
            Assert.Contains("ضمیر", faDetail.ShortDescription, StringComparison.Ordinal);
            Assert.Equal(10, faDetail.Sections.Count);
            Assert.Equal(50, faDetail.Examples.Count);
            Assert.Equal(20, faDetail.CommonMistakes.Count);
            Assert.Equal("این موضوع چیست", faDetail.Sections[0].Heading);
            Assert.DoesNotContain(
                faDetail.Sections,
                section => section.Heading == "What this topic is");
            Assert.DoesNotContain(
                "برای فارسی‌زبان",
                JsonSerializer.Serialize(faDetail),
                StringComparison.Ordinal);
            GrammarSectionModel tableSection = Assert.Single(faDetail.Sections, section => section.SectionKey == "form-or-structure-table");
            Assert.Contains(tableSection.Blocks, block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0);
            Assert.All(faDetail.LinkedWords, word => Assert.True(string.IsNullOrWhiteSpace(word.WordSlug) || !word.WordSlug.Contains(' ', StringComparison.Ordinal)));

            GrammarTopicDetailModel? trSeinDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-sein-in-praesens",
                "tr",
                CancellationToken.None);

            Assert.NotNull(trSeinDetail);
            Assert.Contains("sein", trSeinDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, trSeinDetail.Sections.Count);
            Assert.Equal(46, trSeinDetail.Examples.Count);
            Assert.Equal(20, trSeinDetail.CommonMistakes.Count);
            Assert.Equal("Bu konu nedir", trSeinDetail.Sections[0].Heading);
            Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", trSeinDetail.PrerequisiteSlugs);
            Assert.Contains("a1-haben-in-praesens", trSeinDetail.RelatedTopicSlugs);
            GrammarSectionModel conjugationSection = Assert.Single(trSeinDetail.Sections, section => section.SectionKey == "form-or-structure-table");
            Assert.Contains(conjugationSection.Blocks, block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0);

            GrammarTopicDetailModel? arHabenDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-haben-in-praesens",
                "ar",
                CancellationToken.None);

            Assert.NotNull(arHabenDetail);
            Assert.Contains("haben", arHabenDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, arHabenDetail.Sections.Count);
            Assert.True(arHabenDetail.Examples.Count >= 45);
            Assert.Equal(18, arHabenDetail.RuleSummaries.Count);
            Assert.True(arHabenDetail.CommonMistakes.Count >= 20);
            Assert.Equal("ما هذا الموضوع", arHabenDetail.Sections[0].Heading);
            Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", arHabenDetail.PrerequisiteSlugs);
            Assert.Contains("a1-sein-in-praesens", arHabenDetail.PrerequisiteSlugs);
            Assert.Contains("a1-simple-accusative-introduction", arHabenDetail.RelatedTopicSlugs);
            GrammarSectionModel habenConjugationSection = Assert.Single(arHabenDetail.Sections, section => section.SectionKey == "form-or-structure-table");
            Assert.Contains(habenConjugationSection.Blocks, block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0);
            Assert.Contains(
                arHabenDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "callout"));

            GrammarTopicDetailModel? plRegularVerbsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-regular-verbs-in-praesens",
                "pl",
                CancellationToken.None);

            Assert.NotNull(plRegularVerbsDetail);
            Assert.Contains("regularne", plRegularVerbsDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, plRegularVerbsDetail.Sections.Count);
            Assert.True(plRegularVerbsDetail.Examples.Count >= 45);
            Assert.Equal(18, plRegularVerbsDetail.RuleSummaries.Count);
            Assert.True(plRegularVerbsDetail.CommonMistakes.Count >= 20);
            Assert.Equal("Czym jest ten temat", plRegularVerbsDetail.Sections[0].Heading);
            Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", plRegularVerbsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-sein-in-praesens", plRegularVerbsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-haben-in-praesens", plRegularVerbsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-verb-position-in-simple-sentences", plRegularVerbsDetail.RelatedTopicSlugs);
            Assert.Contains(
                plRegularVerbsDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                plRegularVerbsDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));

            GrammarTopicDetailModel? faVerbPositionDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-verb-position-in-simple-sentences",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faVerbPositionDetail);
            Assert.Contains("جایگاه", faVerbPositionDetail!.Title, StringComparison.Ordinal);
            Assert.Equal(10, faVerbPositionDetail.Sections.Count);
            Assert.True(faVerbPositionDetail.Examples.Count >= 45);
            Assert.Equal(16, faVerbPositionDetail.RuleSummaries.Count);
            Assert.True(faVerbPositionDetail.CommonMistakes.Count >= 20);
            Assert.Equal("این موضوع چیست", faVerbPositionDetail.Sections[0].Heading);
            Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", faVerbPositionDetail.PrerequisiteSlugs);
            Assert.Contains("a1-regular-verbs-in-praesens", faVerbPositionDetail.PrerequisiteSlugs);
            Assert.Contains("a1-word-order-with-time-and-place", faVerbPositionDetail.RelatedTopicSlugs);
            Assert.Contains(
                faVerbPositionDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faVerbPositionDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));

            GrammarTopicDetailModel? faYesNoQuestionsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-yes-no-questions",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faYesNoQuestionsDetail);
            Assert.Contains("سؤال", faYesNoQuestionsDetail!.Title, StringComparison.Ordinal);
            Assert.Equal(10, faYesNoQuestionsDetail.Sections.Count);
            Assert.True(faYesNoQuestionsDetail.Examples.Count >= 45);
            Assert.Equal(16, faYesNoQuestionsDetail.RuleSummaries.Count);
            Assert.True(faYesNoQuestionsDetail.CommonMistakes.Count >= 20);
            Assert.Equal("این موضوع چیست", faYesNoQuestionsDetail.Sections[0].Heading);
            Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", faYesNoQuestionsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-verb-position-in-simple-sentences", faYesNoQuestionsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-w-questions-wer-was-wo-wann-wie", faYesNoQuestionsDetail.RelatedTopicSlugs);
            Assert.Contains("a1-question-answer-sentence-patterns", faYesNoQuestionsDetail.RelatedTopicSlugs);
            Assert.Contains("a1-formal-sie", faYesNoQuestionsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faYesNoQuestionsDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));

            GrammarTopicDetailModel? trWQuestionsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-w-questions-wer-was-wo-wann-wie",
                "tr",
                CancellationToken.None);

            Assert.NotNull(trWQuestionsDetail);
            Assert.Contains("W", trWQuestionsDetail!.Title, StringComparison.Ordinal);
            Assert.Equal(10, trWQuestionsDetail.Sections.Count);
            Assert.True(trWQuestionsDetail.Examples.Count >= 45);
            Assert.Equal(16, trWQuestionsDetail.RuleSummaries.Count);
            Assert.True(trWQuestionsDetail.CommonMistakes.Count >= 20);
            Assert.Equal("Bu konu nedir", trWQuestionsDetail.Sections[0].Heading);
            Assert.Contains("a1-yes-no-questions", trWQuestionsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-question-answer-sentence-patterns", trWQuestionsDetail.RelatedTopicSlugs);
            Assert.Contains("a1-basic-location-phrases", trWQuestionsDetail.RelatedTopicSlugs);
            Assert.Contains("a1-basic-appointment-phrases", trWQuestionsDetail.RelatedTopicSlugs);
            Assert.Contains("a1-formal-sie", trWQuestionsDetail.RelatedTopicSlugs);
            Assert.Contains(
                trWQuestionsDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));

            GrammarTopicDetailModel? faDefiniteArticlesDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-definite-articles-der-die-das",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faDefiniteArticlesDetail);
            Assert.Contains("حرف تعریف", faDefiniteArticlesDetail!.Title, StringComparison.Ordinal);
            Assert.Equal(10, faDefiniteArticlesDetail.Sections.Count);
            Assert.Equal("این موضوع چیست", faDefiniteArticlesDetail.Sections[0].Heading);
            Assert.True(faDefiniteArticlesDetail.Examples.Count >= 45);
            Assert.Equal(18, faDefiniteArticlesDetail.RuleSummaries.Count);
            Assert.True(faDefiniteArticlesDetail.CommonMistakes.Count >= 20);
            Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", faDefiniteArticlesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-sein-in-praesens", faDefiniteArticlesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-verb-position-in-simple-sentences", faDefiniteArticlesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-indefinite-articles-ein-eine", faDefiniteArticlesDetail.RelatedTopicSlugs);
            Assert.Contains("a1-noun-gender-basics", faDefiniteArticlesDetail.RelatedTopicSlugs);
            Assert.Contains("a1-plural-basics", faDefiniteArticlesDetail.RelatedTopicSlugs);
            Assert.Contains("a1-nominative-case", faDefiniteArticlesDetail.RelatedTopicSlugs);
            Assert.Contains("a1-simple-accusative-introduction", faDefiniteArticlesDetail.RelatedTopicSlugs);
            Assert.Contains("a1-articles-with-food-drinks-and-shopping-nouns", faDefiniteArticlesDetail.RelatedTopicSlugs);
            Assert.Contains(
                faDefiniteArticlesDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDefiniteArticlesDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDefiniteArticlesDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faDefiniteArticlesDetail.Examples, example => example.GermanText == "Der Kaffee ist neu.");
            Assert.Contains(faDefiniteArticlesDetail.Examples, example => example.GermanText == "Ich sehe den Tisch.");
            Assert.Contains(faDefiniteArticlesDetail.CommonMistakes, mistake => mistake.WrongText == "Ich sehe der Mann.");
            Assert.Contains(faDefiniteArticlesDetail.CommonMistakes, mistake => mistake.WrongText == "das Bücher");

            GrammarTopicDetailModel? trIndefiniteArticlesDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-indefinite-articles-ein-eine",
                "tr",
                CancellationToken.None);

            Assert.NotNull(trIndefiniteArticlesDetail);
            Assert.Contains("artikeller", trIndefiniteArticlesDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, trIndefiniteArticlesDetail.Sections.Count);
            Assert.True(trIndefiniteArticlesDetail.Examples.Count >= 45);
            Assert.Equal(18, trIndefiniteArticlesDetail.RuleSummaries.Count);
            Assert.True(trIndefiniteArticlesDetail.CommonMistakes.Count >= 20);
            Assert.Contains("a1-definite-articles-der-die-das", trIndefiniteArticlesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-sein-in-praesens", trIndefiniteArticlesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-verb-position-in-simple-sentences", trIndefiniteArticlesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-noun-gender-basics", trIndefiniteArticlesDetail.RelatedTopicSlugs);
            Assert.Contains("a1-plural-basics", trIndefiniteArticlesDetail.RelatedTopicSlugs);
            Assert.Contains("a1-nominative-case", trIndefiniteArticlesDetail.RelatedTopicSlugs);
            Assert.Contains("a1-simple-accusative-introduction", trIndefiniteArticlesDetail.RelatedTopicSlugs);
            Assert.Contains(
                trIndefiniteArticlesDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                trIndefiniteArticlesDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                trIndefiniteArticlesDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(trIndefiniteArticlesDetail.Examples, example => example.GermanText == "Das ist ein Mann.");
            Assert.Contains(trIndefiniteArticlesDetail.Examples, example => example.GermanText == "Ich brauche einen Termin.");
            Assert.Contains(trIndefiniteArticlesDetail.CommonMistakes, mistake => mistake.WrongText == "Ich brauche ein Termin.");
            Assert.Contains(trIndefiniteArticlesDetail.CommonMistakes, mistake => mistake.WrongText == "Das sind eine Kinder.");

            GrammarTopicDetailModel? faNounGenderDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-noun-gender-basics",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faNounGenderDetail);
            Assert.Contains("جنسیت", faNounGenderDetail!.Title, StringComparison.Ordinal);
            Assert.Equal(10, faNounGenderDetail.Sections.Count);
            Assert.True(faNounGenderDetail.Examples.Count >= 45);
            Assert.Equal(18, faNounGenderDetail.RuleSummaries.Count);
            Assert.True(faNounGenderDetail.CommonMistakes.Count >= 20);
            Assert.Contains("a1-definite-articles-der-die-das", faNounGenderDetail.PrerequisiteSlugs);
            Assert.Contains("a1-indefinite-articles-ein-eine", faNounGenderDetail.PrerequisiteSlugs);
            Assert.Contains("a1-sein-in-praesens", faNounGenderDetail.PrerequisiteSlugs);
            Assert.Contains("a1-plural-basics", faNounGenderDetail.RelatedTopicSlugs);
            Assert.Contains("a1-nominative-case", faNounGenderDetail.RelatedTopicSlugs);
            Assert.Contains("a1-simple-accusative-introduction", faNounGenderDetail.RelatedTopicSlugs);
            Assert.Contains("a1-basic-adjective-position", faNounGenderDetail.RelatedTopicSlugs);
            Assert.Contains(
                faNounGenderDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faNounGenderDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faNounGenderDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faNounGenderDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faNounGenderDetail.Examples, example => example.GermanText == "Der Mann ist hier.");
            Assert.Contains(faNounGenderDetail.Examples, example => example.GermanText == "Ich sehe den Mann.");
            Assert.Contains(faNounGenderDetail.CommonMistakes, mistake => mistake.WrongText == "die Mann");
            Assert.Contains(faNounGenderDetail.CommonMistakes, mistake => mistake.WrongText == "Ich brauche eine Termin.");

            GrammarTopicDetailModel? faPluralBasicsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-plural-basics",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faPluralBasicsDetail);
            Assert.Contains("جمع", faPluralBasicsDetail!.Title, StringComparison.Ordinal);
            Assert.Equal(10, faPluralBasicsDetail.Sections.Count);
            Assert.True(faPluralBasicsDetail.Examples.Count >= 45);
            Assert.Equal(18, faPluralBasicsDetail.RuleSummaries.Count);
            Assert.True(faPluralBasicsDetail.CommonMistakes.Count >= 20);
            Assert.Contains("a1-definite-articles-der-die-das", faPluralBasicsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-indefinite-articles-ein-eine", faPluralBasicsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-noun-gender-basics", faPluralBasicsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-nominative-case", faPluralBasicsDetail.RelatedTopicSlugs);
            Assert.Contains("a1-simple-accusative-introduction", faPluralBasicsDetail.RelatedTopicSlugs);
            Assert.Contains("a1-numbers-and-grammar-use", faPluralBasicsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faPluralBasicsDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faPluralBasicsDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faPluralBasicsDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faPluralBasicsDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faPluralBasicsDetail.Examples, example => example.GermanText == "Die Kinder sind hier.");
            Assert.Contains(faPluralBasicsDetail.Examples, example => example.GermanText == "Ich habe zwei Bücher.");
            Assert.Contains(faPluralBasicsDetail.CommonMistakes, mistake => mistake.WrongText == "die Buch");
            Assert.Contains(faPluralBasicsDetail.CommonMistakes, mistake => mistake.WrongText == "Die Kinder ist hier.");

            GrammarTopicDetailModel? faNominativeDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-nominative-case",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faNominativeDetail);
            Assert.Contains("nominative", faNominativeDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, faNominativeDetail.Sections.Count);
            Assert.True(faNominativeDetail.Examples.Count >= 45);
            Assert.True(faNominativeDetail.RuleSummaries.Count >= 18);
            Assert.True(faNominativeDetail.CommonMistakes.Count >= 20);
            Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", faNominativeDetail.PrerequisiteSlugs);
            Assert.Contains("a1-sein-in-praesens", faNominativeDetail.PrerequisiteSlugs);
            Assert.Contains("a1-definite-articles-der-die-das", faNominativeDetail.PrerequisiteSlugs);
            Assert.Contains("a1-indefinite-articles-ein-eine", faNominativeDetail.PrerequisiteSlugs);
            Assert.Contains("a1-noun-gender-basics", faNominativeDetail.PrerequisiteSlugs);
            Assert.Contains("a1-plural-basics", faNominativeDetail.PrerequisiteSlugs);
            Assert.Contains("a1-simple-accusative-introduction", faNominativeDetail.RelatedTopicSlugs);
            Assert.Contains("a1-basic-adjective-position", faNominativeDetail.RelatedTopicSlugs);
            Assert.Contains("a1-pronoun-and-verb-agreement", faNominativeDetail.RelatedTopicSlugs);
            Assert.Contains("a1-question-answer-sentence-patterns", faNominativeDetail.RelatedTopicSlugs);
            Assert.Contains(
                faNominativeDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faNominativeDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faNominativeDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faNominativeDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faNominativeDetail.Examples, example => example.GermanText == "Der Mann ist hier.");
            Assert.Contains(faNominativeDetail.Examples, example => example.GermanText == "Morgen kommt der Lehrer.");
            Assert.Contains(faNominativeDetail.CommonMistakes, mistake => mistake.WrongText == "Den Mann kommt.");
            Assert.Contains(faNominativeDetail.CommonMistakes, mistake => mistake.WrongText == "Die Bücher ist neu.");

            GrammarTopicDetailModel? faAccusativeDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-simple-accusative-introduction",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faAccusativeDetail);
            Assert.Contains("accusative", faAccusativeDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, faAccusativeDetail.Sections.Count);
            Assert.True(faAccusativeDetail.Examples.Count >= 45);
            Assert.True(faAccusativeDetail.RuleSummaries.Count >= 18);
            Assert.True(faAccusativeDetail.CommonMistakes.Count >= 20);
            Assert.Contains("a1-nominative-case", faAccusativeDetail.PrerequisiteSlugs);
            Assert.Contains("a1-definite-articles-der-die-das", faAccusativeDetail.PrerequisiteSlugs);
            Assert.Contains("a1-indefinite-articles-ein-eine", faAccusativeDetail.PrerequisiteSlugs);
            Assert.Contains("a1-noun-gender-basics", faAccusativeDetail.PrerequisiteSlugs);
            Assert.Contains("a1-haben-in-praesens", faAccusativeDetail.PrerequisiteSlugs);
            Assert.Contains("a1-kein-versus-nicht-basics", faAccusativeDetail.RelatedTopicSlugs);
            Assert.Contains("a1-articles-with-food-drinks-and-shopping-nouns", faAccusativeDetail.RelatedTopicSlugs);
            Assert.Contains("a1-basic-adjective-position", faAccusativeDetail.RelatedTopicSlugs);
            Assert.Contains(
                faAccusativeDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faAccusativeDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faAccusativeDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faAccusativeDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faAccusativeDetail.Examples, example => example.GermanText == "Ich sehe den Mann.");
            Assert.Contains(faAccusativeDetail.Examples, example => example.GermanText == "Ich brauche einen Termin.");
            Assert.Contains(faAccusativeDetail.CommonMistakes, mistake => mistake.WrongText == "Ich sehe der Mann.");
            Assert.Contains(faAccusativeDetail.CommonMistakes, mistake => mistake.WrongText == "Ich kaufe ein Kaffee.");

            GrammarTopicDetailModel? faKeinNichtDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-kein-versus-nicht-basics",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faKeinNichtDetail);
            Assert.Contains("kein", faKeinNichtDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, faKeinNichtDetail.Sections.Count);
            Assert.True(faKeinNichtDetail.Examples.Count >= 50);
            Assert.True(faKeinNichtDetail.RuleSummaries.Count >= 18);
            Assert.True(faKeinNichtDetail.CommonMistakes.Count >= 22);
            Assert.Contains("a1-definite-articles-der-die-das", faKeinNichtDetail.PrerequisiteSlugs);
            Assert.Contains("a1-indefinite-articles-ein-eine", faKeinNichtDetail.PrerequisiteSlugs);
            Assert.Contains("a1-simple-accusative-introduction", faKeinNichtDetail.PrerequisiteSlugs);
            Assert.Contains("a1-sein-in-praesens", faKeinNichtDetail.PrerequisiteSlugs);
            Assert.Contains("a1-haben-in-praesens", faKeinNichtDetail.PrerequisiteSlugs);
            Assert.Contains("a1-basic-sentence-negation", faKeinNichtDetail.RelatedTopicSlugs);
            Assert.Contains("a2-possessive-pronouns-in-cases", faKeinNichtDetail.RelatedTopicSlugs);
            Assert.Contains(
                faKeinNichtDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faKeinNichtDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faKeinNichtDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faKeinNichtDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faKeinNichtDetail.Examples, example => example.GermanText == "Ich habe keine Zeit.");
            Assert.Contains(faKeinNichtDetail.Examples, example => example.GermanText == "Das ist nicht gut.");
            Assert.Contains(faKeinNichtDetail.CommonMistakes, mistake => mistake.WrongText == "Ich habe nicht Zeit.");
            Assert.Contains(faKeinNichtDetail.CommonMistakes, mistake => mistake.WrongText == "Das Zimmer ist kein frei.");

            GrammarTopicDetailModel? faPossessivePronounsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-possessive-pronouns-mein-dein",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faPossessivePronounsDetail);
            Assert.Contains("mein", faPossessivePronounsDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, faPossessivePronounsDetail.Sections.Count);
            Assert.True(faPossessivePronounsDetail.Examples.Count >= 53);
            Assert.True(faPossessivePronounsDetail.RuleSummaries.Count >= 18);
            Assert.True(faPossessivePronounsDetail.CommonMistakes.Count >= 22);
            Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", faPossessivePronounsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-definite-articles-der-die-das", faPossessivePronounsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-indefinite-articles-ein-eine", faPossessivePronounsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-noun-gender-basics", faPossessivePronounsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-plural-basics", faPossessivePronounsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-formal-sie", faPossessivePronounsDetail.RelatedTopicSlugs);
            Assert.Contains("a2-possessive-pronouns-in-cases", faPossessivePronounsDetail.RelatedTopicSlugs);
            Assert.Contains("a1-basic-adjective-position", faPossessivePronounsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faPossessivePronounsDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faPossessivePronounsDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faPossessivePronounsDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faPossessivePronounsDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faPossessivePronounsDetail.Examples, example => example.GermanText == "Das ist mein Vater.");
            Assert.Contains(faPossessivePronounsDetail.Examples, example => example.GermanText == "Wie ist Ihre Adresse?");
            Assert.Contains(faPossessivePronounsDetail.CommonMistakes, mistake => mistake.WrongText == "Das ist meine Vater.");
            Assert.Contains(faPossessivePronounsDetail.CommonMistakes, mistake => mistake.WrongText == "Das ist kein mein Buch.");

            GrammarTopicDetailModel? faBasicAdjectiveDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-basic-adjective-position",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faBasicAdjectiveDetail);
            Assert.Contains("صفت", faBasicAdjectiveDetail!.Title, StringComparison.Ordinal);
            Assert.Equal(10, faBasicAdjectiveDetail.Sections.Count);
            Assert.True(faBasicAdjectiveDetail.Examples.Count >= 55);
            Assert.True(faBasicAdjectiveDetail.RuleSummaries.Count >= 18);
            Assert.True(faBasicAdjectiveDetail.CommonMistakes.Count >= 25);
            Assert.Contains("a1-sein-in-praesens", faBasicAdjectiveDetail.PrerequisiteSlugs);
            Assert.Contains("a1-definite-articles-der-die-das", faBasicAdjectiveDetail.PrerequisiteSlugs);
            Assert.Contains("a1-indefinite-articles-ein-eine", faBasicAdjectiveDetail.PrerequisiteSlugs);
            Assert.Contains("a1-noun-gender-basics", faBasicAdjectiveDetail.PrerequisiteSlugs);
            Assert.Contains("a2-adjective-endings-introduction", faBasicAdjectiveDetail.RelatedTopicSlugs);
            Assert.Contains("b1-adjective-declension-after-definite-article", faBasicAdjectiveDetail.RelatedTopicSlugs);
            Assert.Contains("b1-adjective-declension-after-indefinite-article", faBasicAdjectiveDetail.RelatedTopicSlugs);
            Assert.Contains("a1-basic-sentence-negation", faBasicAdjectiveDetail.RelatedTopicSlugs);
            Assert.Contains(
                faBasicAdjectiveDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faBasicAdjectiveDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faBasicAdjectiveDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faBasicAdjectiveDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faBasicAdjectiveDetail.Examples, example => example.GermanText == "Das Zimmer ist klein.");
            Assert.Contains(faBasicAdjectiveDetail.Examples, example => example.GermanText == "Das ist ein kleines Zimmer.");
            Assert.Contains(faBasicAdjectiveDetail.CommonMistakes, mistake => mistake.WrongText == "Das Zimmer ist kleines.");
            Assert.Contains(faBasicAdjectiveDetail.CommonMistakes, mistake => mistake.WrongText == "Das ist ein klein Zimmer.");

            GrammarTopicDetailModel? faBasicPrepositionsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-basic-prepositions-in-aus-nach-bei",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faBasicPrepositionsDetail);
            Assert.Contains("in", faBasicPrepositionsDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, faBasicPrepositionsDetail.Sections.Count);
            Assert.True(faBasicPrepositionsDetail.Examples.Count >= 50);
            Assert.True(faBasicPrepositionsDetail.RuleSummaries.Count >= 19);
            Assert.True(faBasicPrepositionsDetail.CommonMistakes.Count >= 23);
            Assert.Contains("a1-sein-in-praesens", faBasicPrepositionsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-regular-verbs-in-praesens", faBasicPrepositionsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-basic-location-phrases", faBasicPrepositionsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-basic-location-phrases", faBasicPrepositionsDetail.RelatedTopicSlugs);
            Assert.Contains("a1-word-order-with-time-and-place", faBasicPrepositionsDetail.RelatedTopicSlugs);
            Assert.Contains("a2-prepositions-with-dative", faBasicPrepositionsDetail.RelatedTopicSlugs);
            Assert.Contains("a2-prepositions-with-accusative", faBasicPrepositionsDetail.RelatedTopicSlugs);
            Assert.Contains("a2-wechselpraepositionen-introduction", faBasicPrepositionsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faBasicPrepositionsDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faBasicPrepositionsDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faBasicPrepositionsDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faBasicPrepositionsDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faBasicPrepositionsDetail.Examples, example => example.GermanText == "Ich bin in Berlin.");
            Assert.Contains(faBasicPrepositionsDetail.Examples, example => example.GermanText == "Ich gehe nach Hause.");
            Assert.Contains(faBasicPrepositionsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich bin nach Berlin.");
            Assert.Contains(faBasicPrepositionsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich gehe zu Hause.");

            GrammarTopicDetailModel? faNumbersDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-numbers-and-grammar-use",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faNumbersDetail);
            Assert.Contains("عدد", faNumbersDetail!.Title, StringComparison.Ordinal);
            Assert.Equal(10, faNumbersDetail.Sections.Count);
            Assert.True(faNumbersDetail.Examples.Count >= 55);
            Assert.True(faNumbersDetail.RuleSummaries.Count >= 18);
            Assert.True(faNumbersDetail.CommonMistakes.Count >= 20);
            Assert.Contains("a1-sein-in-praesens", faNumbersDetail.PrerequisiteSlugs);
            Assert.Contains("a1-haben-in-praesens", faNumbersDetail.PrerequisiteSlugs);
            Assert.Contains("a1-w-questions-wer-was-wo-wann-wie", faNumbersDetail.PrerequisiteSlugs);
            Assert.Contains("a1-time-expressions-heute-morgen-gestern", faNumbersDetail.RelatedTopicSlugs);
            Assert.Contains("a1-basic-appointment-phrases", faNumbersDetail.RelatedTopicSlugs);
            Assert.Contains("a1-word-order-with-time-and-place", faNumbersDetail.RelatedTopicSlugs);
            Assert.Contains("a2-grammar-for-appointments", faNumbersDetail.RelatedTopicSlugs);
            Assert.Contains(
                faNumbersDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faNumbersDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faNumbersDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faNumbersDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faNumbersDetail.Examples, example => example.GermanText == "Ich bin 25 Jahre alt.");
            Assert.Contains(faNumbersDetail.Examples, example => example.GermanText == "Ich habe zwei Kinder.");
            Assert.Contains(faNumbersDetail.CommonMistakes, mistake => mistake.WrongText == "Ich bin 25 Jahr alt.");
            Assert.Contains(faNumbersDetail.CommonMistakes, mistake => mistake.WrongText == "Ich habe zwei Kind.");

            GrammarTopicDetailModel? faTimeExpressionsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-time-expressions-heute-morgen-gestern",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faTimeExpressionsDetail);
            Assert.Contains("زمان", faTimeExpressionsDetail!.Title, StringComparison.Ordinal);
            Assert.Equal(10, faTimeExpressionsDetail.Sections.Count);
            Assert.True(faTimeExpressionsDetail.Examples.Count >= 55);
            Assert.True(faTimeExpressionsDetail.RuleSummaries.Count >= 18);
            Assert.True(faTimeExpressionsDetail.CommonMistakes.Count >= 20);
            Assert.Contains("a1-verb-position-in-simple-sentences", faTimeExpressionsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-numbers-and-grammar-use", faTimeExpressionsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-haben-in-praesens", faTimeExpressionsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-regular-verbs-in-praesens", faTimeExpressionsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-word-order-with-time-and-place", faTimeExpressionsDetail.RelatedTopicSlugs);
            Assert.Contains("a1-basic-appointment-phrases", faTimeExpressionsDetail.RelatedTopicSlugs);
            Assert.Contains("a2-perfekt-with-haben", faTimeExpressionsDetail.RelatedTopicSlugs);
            Assert.Contains("a2-praeteritum-of-sein-and-haben", faTimeExpressionsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faTimeExpressionsDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faTimeExpressionsDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faTimeExpressionsDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faTimeExpressionsDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faTimeExpressionsDetail.Examples, example => example.GermanText == "Heute komme ich.");
            Assert.Contains(faTimeExpressionsDetail.Examples, example => example.GermanText == "Morgen komme ich.");
            Assert.Contains(faTimeExpressionsDetail.Examples, example => example.GermanText == "Gestern war ich krank.");
            Assert.Contains(faTimeExpressionsDetail.CommonMistakes, mistake => mistake.WrongText == "Heute ich komme.");
            Assert.Contains(faTimeExpressionsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich bin gestern krank.");

            GrammarTopicDetailModel? faTimePlaceWordOrderDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-word-order-with-time-and-place",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faTimePlaceWordOrderDetail);
            Assert.Contains("ترتیب", faTimePlaceWordOrderDetail!.Title, StringComparison.Ordinal);
            Assert.Equal(10, faTimePlaceWordOrderDetail.Sections.Count);
            Assert.True(faTimePlaceWordOrderDetail.Examples.Count >= 60);
            Assert.True(faTimePlaceWordOrderDetail.RuleSummaries.Count >= 18);
            Assert.True(faTimePlaceWordOrderDetail.CommonMistakes.Count >= 20);
            Assert.Contains("a1-verb-position-in-simple-sentences", faTimePlaceWordOrderDetail.PrerequisiteSlugs);
            Assert.Contains("a1-time-expressions-heute-morgen-gestern", faTimePlaceWordOrderDetail.PrerequisiteSlugs);
            Assert.Contains("a1-basic-prepositions-in-aus-nach-bei", faTimePlaceWordOrderDetail.PrerequisiteSlugs);
            Assert.Contains("a1-regular-verbs-in-praesens", faTimePlaceWordOrderDetail.PrerequisiteSlugs);
            Assert.Contains("a1-basic-location-phrases", faTimePlaceWordOrderDetail.RelatedTopicSlugs);
            Assert.Contains("a1-basic-appointment-phrases", faTimePlaceWordOrderDetail.RelatedTopicSlugs);
            Assert.Contains("a2-sentence-order-in-subordinate-clauses", faTimePlaceWordOrderDetail.RelatedTopicSlugs);
            Assert.Contains("b1-sentence-order-with-multiple-clauses", faTimePlaceWordOrderDetail.RelatedTopicSlugs);
            Assert.Contains(
                faTimePlaceWordOrderDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faTimePlaceWordOrderDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faTimePlaceWordOrderDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faTimePlaceWordOrderDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faTimePlaceWordOrderDetail.Examples, example => example.GermanText == "Ich gehe heute in den Kurs.");
            Assert.Contains(faTimePlaceWordOrderDetail.Examples, example => example.GermanText == "Heute gehe ich in den Kurs.");
            Assert.Contains(faTimePlaceWordOrderDetail.Examples, example => example.GermanText == "Morgen lernen wir in der Schule.");
            Assert.Contains(faTimePlaceWordOrderDetail.CommonMistakes, mistake => mistake.WrongText == "Heute ich gehe in den Kurs.");
            Assert.Contains(faTimePlaceWordOrderDetail.CommonMistakes, mistake => mistake.WrongText == "Morgen wir lernen in der Schule.");

            GrammarTopicDetailModel? faSimpleModalsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-simple-modal-verbs-koennen-muessen-wollen",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faSimpleModalsDetail);
            Assert.Contains("modal", faSimpleModalsDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, faSimpleModalsDetail.Sections.Count);
            Assert.True(faSimpleModalsDetail.Examples.Count >= 55);
            Assert.True(faSimpleModalsDetail.RuleSummaries.Count >= 18);
            Assert.True(faSimpleModalsDetail.CommonMistakes.Count >= 22);
            Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", faSimpleModalsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-regular-verbs-in-praesens", faSimpleModalsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-verb-position-in-simple-sentences", faSimpleModalsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-yes-no-questions", faSimpleModalsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-polite-requests-with-moechte", faSimpleModalsDetail.RelatedTopicSlugs);
            Assert.Contains("a2-modal-verbs-in-more-detail", faSimpleModalsDetail.RelatedTopicSlugs);
            Assert.Contains("a2-polite-forms-with-wuerde", faSimpleModalsDetail.RelatedTopicSlugs);
            Assert.Contains("b1-modal-verbs-in-the-past", faSimpleModalsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faSimpleModalsDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSimpleModalsDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSimpleModalsDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSimpleModalsDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faSimpleModalsDetail.Examples, example => example.GermanText == "Ich kann Deutsch sprechen.");
            Assert.Contains(faSimpleModalsDetail.Examples, example => example.GermanText == "Ich muss heute arbeiten.");
            Assert.Contains(faSimpleModalsDetail.Examples, example => example.GermanText == "Ich will Wasser trinken.");
            Assert.Contains(faSimpleModalsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich kann Deutsch spreche.");
            Assert.Contains(faSimpleModalsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich muss arbeite heute.");

            GrammarTopicDetailModel? faPoliteMoechteDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-polite-requests-with-moechte",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faPoliteMoechteDetail);
            Assert.Contains("möchte", faPoliteMoechteDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, faPoliteMoechteDetail.Sections.Count);
            Assert.True(faPoliteMoechteDetail.Examples.Count >= 55);
            Assert.True(faPoliteMoechteDetail.RuleSummaries.Count >= 18);
            Assert.True(faPoliteMoechteDetail.CommonMistakes.Count >= 22);
            Assert.Contains("a1-simple-modal-verbs-koennen-muessen-wollen", faPoliteMoechteDetail.PrerequisiteSlugs);
            Assert.Contains("a1-simple-accusative-introduction", faPoliteMoechteDetail.PrerequisiteSlugs);
            Assert.Contains("a1-yes-no-questions", faPoliteMoechteDetail.PrerequisiteSlugs);
            Assert.Contains("a1-w-questions-wer-was-wo-wann-wie", faPoliteMoechteDetail.PrerequisiteSlugs);
            Assert.Contains("a1-imperative-basics", faPoliteMoechteDetail.RelatedTopicSlugs);
            Assert.Contains("a1-articles-with-food-drinks-and-shopping-nouns", faPoliteMoechteDetail.RelatedTopicSlugs);
            Assert.Contains("a1-basic-appointment-phrases", faPoliteMoechteDetail.RelatedTopicSlugs);
            Assert.Contains("a2-polite-forms-with-wuerde", faPoliteMoechteDetail.RelatedTopicSlugs);
            Assert.Contains(
                faPoliteMoechteDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faPoliteMoechteDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faPoliteMoechteDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faPoliteMoechteDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faPoliteMoechteDetail.Examples, example => example.GermanText == "Ich möchte einen Kaffee.");
            Assert.Contains(faPoliteMoechteDetail.Examples, example => example.GermanText == "Ich möchte bezahlen.");
            Assert.Contains(faPoliteMoechteDetail.Examples, example => example.GermanText == "Möchtest du Wasser?");
            Assert.Contains(faPoliteMoechteDetail.CommonMistakes, mistake => mistake.WrongText == "Ich möchte zu bezahlen.");
            Assert.Contains(faPoliteMoechteDetail.CommonMistakes, mistake => mistake.WrongText == "Ich möchte Deutsch lerne.");

            GrammarTopicDetailModel? faImperativeDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-imperative-basics",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faImperativeDetail);
            Assert.Contains("imperative", faImperativeDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, faImperativeDetail.Sections.Count);
            Assert.True(faImperativeDetail.Examples.Count >= 50);
            Assert.True(faImperativeDetail.RuleSummaries.Count >= 18);
            Assert.True(faImperativeDetail.CommonMistakes.Count >= 20);
            Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", faImperativeDetail.PrerequisiteSlugs);
            Assert.Contains("a1-formal-sie", faImperativeDetail.PrerequisiteSlugs);
            Assert.Contains("a1-regular-verbs-in-praesens", faImperativeDetail.PrerequisiteSlugs);
            Assert.DoesNotContain("a1-du-vs-sie-grammar-basics", faImperativeDetail.PrerequisiteSlugs);
            Assert.Contains("a2-imperative-formal-and-informal", faImperativeDetail.RelatedTopicSlugs);
            Assert.Contains("a1-polite-requests-with-moechte", faImperativeDetail.RelatedTopicSlugs);
            Assert.Contains("a1-yes-no-questions", faImperativeDetail.RelatedTopicSlugs);
            Assert.Contains(
                faImperativeDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faImperativeDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faImperativeDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faImperativeDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faImperativeDetail.Sections,
                section => section.Blocks.Any(block => block.Type == "callout" && string.Equals(block.Style, "warning", StringComparison.OrdinalIgnoreCase)));
            Assert.Contains(faImperativeDetail.Examples, example => example.GermanText == "Komm bitte.");
            Assert.Contains(faImperativeDetail.Examples, example => example.GermanText == "Warten Sie bitte.");
            Assert.Contains(faImperativeDetail.CommonMistakes, mistake => mistake.WrongText == "Du kommst bitte.");
            Assert.Contains(faImperativeDetail.CommonMistakes, mistake => mistake.WrongText == "Sie kommen bitte.");

            GrammarTopicDetailModel? faSeparableVerbsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-separable-verbs-introduction",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faSeparableVerbsDetail);
            Assert.Contains("separable", faSeparableVerbsDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, faSeparableVerbsDetail.Sections.Count);
            Assert.True(faSeparableVerbsDetail.Examples.Count >= 55);
            Assert.True(faSeparableVerbsDetail.RuleSummaries.Count >= 18);
            Assert.True(faSeparableVerbsDetail.CommonMistakes.Count >= 22);
            Assert.Contains("a1-regular-verbs-in-praesens", faSeparableVerbsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-verb-position-in-simple-sentences", faSeparableVerbsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-yes-no-questions", faSeparableVerbsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-simple-modal-verbs-koennen-muessen-wollen", faSeparableVerbsDetail.PrerequisiteSlugs);
            Assert.Contains("a2-separable-verbs-in-perfekt", faSeparableVerbsDetail.RelatedTopicSlugs);
            Assert.Contains("a2-common-irregular-participles", faSeparableVerbsDetail.RelatedTopicSlugs);
            Assert.Contains("a1-word-order-with-time-and-place", faSeparableVerbsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faSeparableVerbsDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSeparableVerbsDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSeparableVerbsDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSeparableVerbsDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faSeparableVerbsDetail.Examples, example => example.GermanText == "Ich stehe früh auf.");
            Assert.Contains(faSeparableVerbsDetail.Examples, example => example.GermanText == "Ich muss heute einkaufen.");
            Assert.Contains(faSeparableVerbsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich aufstehe um sieben Uhr.");
            Assert.Contains(faSeparableVerbsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich muss kaufe ein.");

            GrammarTopicDetailModel? faSimpleConjunctionsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-simple-conjunctions-und-aber",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faSimpleConjunctionsDetail);
            Assert.Contains("und", faSimpleConjunctionsDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, faSimpleConjunctionsDetail.Sections.Count);
            Assert.True(faSimpleConjunctionsDetail.Examples.Count >= 50);
            Assert.True(faSimpleConjunctionsDetail.RuleSummaries.Count >= 18);
            Assert.True(faSimpleConjunctionsDetail.CommonMistakes.Count >= 20);
            Assert.Contains("a1-verb-position-in-simple-sentences", faSimpleConjunctionsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-regular-verbs-in-praesens", faSimpleConjunctionsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-basic-adjective-position", faSimpleConjunctionsDetail.PrerequisiteSlugs);
            Assert.DoesNotContain("a2-denn-vs-weil", faSimpleConjunctionsDetail.RelatedTopicSlugs);
            Assert.Contains("a2-dass-clauses", faSimpleConjunctionsDetail.RelatedTopicSlugs);
            Assert.Contains("a2-weil-clauses", faSimpleConjunctionsDetail.RelatedTopicSlugs);
            Assert.Contains("b1-connectors-for-opinion", faSimpleConjunctionsDetail.RelatedTopicSlugs);
            Assert.Contains("b1-connectors-for-contrast", faSimpleConjunctionsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faSimpleConjunctionsDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSimpleConjunctionsDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSimpleConjunctionsDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faSimpleConjunctionsDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faSimpleConjunctionsDetail.Examples, example => example.GermanText == "Ich lerne Deutsch und Englisch.");
            Assert.Contains(faSimpleConjunctionsDetail.Examples, example => example.GermanText == "Ich lerne Deutsch, aber es ist schwer.");
            Assert.Contains(faSimpleConjunctionsDetail.CommonMistakes, mistake => mistake.WrongText == "Der Kaffee ist gut und teuer.");
            Assert.Contains(faSimpleConjunctionsDetail.CommonMistakes, mistake => mistake.WrongText == "Ich lerne Deutsch, aber ist schwer.");

            GrammarTopicDetailModel? faPronounVerbAgreementDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-pronoun-and-verb-agreement",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faPronounVerbAgreementDetail);
            Assert.Contains("فعل", faPronounVerbAgreementDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, faPronounVerbAgreementDetail.Sections.Count);
            Assert.True(faPronounVerbAgreementDetail.Examples.Count >= 55);
            Assert.True(faPronounVerbAgreementDetail.RuleSummaries.Count >= 18);
            Assert.True(faPronounVerbAgreementDetail.CommonMistakes.Count >= 22);
            Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", faPronounVerbAgreementDetail.PrerequisiteSlugs);
            Assert.Contains("a1-regular-verbs-in-praesens", faPronounVerbAgreementDetail.PrerequisiteSlugs);
            Assert.Contains("a1-sein-in-praesens", faPronounVerbAgreementDetail.PrerequisiteSlugs);
            Assert.Contains("a1-haben-in-praesens", faPronounVerbAgreementDetail.PrerequisiteSlugs);
            Assert.Contains("a1-yes-no-questions", faPronounVerbAgreementDetail.PrerequisiteSlugs);
            Assert.Contains("a1-verb-position-in-simple-sentences", faPronounVerbAgreementDetail.RelatedTopicSlugs);
            Assert.Contains("a1-w-questions-wer-was-wo-wann-wie", faPronounVerbAgreementDetail.RelatedTopicSlugs);
            Assert.Contains("a1-formal-sie", faPronounVerbAgreementDetail.RelatedTopicSlugs);
            Assert.Contains("a1-du-versus-sie-grammar-basics", faPronounVerbAgreementDetail.RelatedTopicSlugs);
            Assert.Contains(
                faPronounVerbAgreementDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faPronounVerbAgreementDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faPronounVerbAgreementDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faPronounVerbAgreementDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faPronounVerbAgreementDetail.Examples, example => example.GermanText == "Ich lerne Deutsch.");
            Assert.Contains(faPronounVerbAgreementDetail.Examples, example => example.GermanText == "Lernst du Deutsch?");
            Assert.Contains(faPronounVerbAgreementDetail.CommonMistakes, mistake => mistake.WrongText == "Ich lernst Deutsch.");
            Assert.Contains(faPronounVerbAgreementDetail.CommonMistakes, mistake => mistake.WrongText == "Du lerne Deutsch.");

            GrammarTopicDetailModel? faFormalSieDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-formal-sie",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faFormalSieDetail);
            Assert.Contains("Sie", faFormalSieDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, faFormalSieDetail.Sections.Count);
            Assert.True(faFormalSieDetail.Examples.Count >= 55);
            Assert.True(faFormalSieDetail.RuleSummaries.Count >= 18);
            Assert.True(faFormalSieDetail.CommonMistakes.Count >= 22);
            Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", faFormalSieDetail.PrerequisiteSlugs);
            Assert.Contains("a1-sein-in-praesens", faFormalSieDetail.PrerequisiteSlugs);
            Assert.Contains("a1-haben-in-praesens", faFormalSieDetail.PrerequisiteSlugs);
            Assert.Contains("a1-pronoun-and-verb-agreement", faFormalSieDetail.PrerequisiteSlugs);
            Assert.Contains("a1-yes-no-questions", faFormalSieDetail.PrerequisiteSlugs);
            Assert.Contains("a1-du-versus-sie-grammar-basics", faFormalSieDetail.RelatedTopicSlugs);
            Assert.Contains("a1-imperative-basics", faFormalSieDetail.RelatedTopicSlugs);
            Assert.Contains("a1-polite-requests-with-moechte", faFormalSieDetail.RelatedTopicSlugs);
            Assert.Contains("a2-imperative-formal-and-informal", faFormalSieDetail.RelatedTopicSlugs);
            Assert.Contains(
                faFormalSieDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faFormalSieDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faFormalSieDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faFormalSieDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "callout"));
            Assert.Contains(
                faFormalSieDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faFormalSieDetail.Examples, example => example.GermanText == "Haben Sie Zeit?");
            Assert.Contains(faFormalSieDetail.Examples, example => example.GermanText == "Darf ich Ihnen helfen?");
            Assert.Contains(faFormalSieDetail.CommonMistakes, mistake => mistake.WrongText == "Sie bist Herr Weber?");
            Assert.Contains(faFormalSieDetail.CommonMistakes, mistake => mistake.WrongText == "Kann ich Sie helfen?");

            GrammarTopicDetailModel? faDuVersusSieDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-du-versus-sie-grammar-basics",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faDuVersusSieDetail);
            Assert.Contains("du", faDuVersusSieDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Sie", faDuVersusSieDetail.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, faDuVersusSieDetail.Sections.Count);
            Assert.True(faDuVersusSieDetail.Examples.Count >= 60);
            Assert.Equal(18, faDuVersusSieDetail.RuleSummaries.Count);
            Assert.True(faDuVersusSieDetail.CommonMistakes.Count >= 25);
            Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", faDuVersusSieDetail.PrerequisiteSlugs);
            Assert.Contains("a1-formal-sie", faDuVersusSieDetail.PrerequisiteSlugs);
            Assert.Contains("a1-pronoun-and-verb-agreement", faDuVersusSieDetail.PrerequisiteSlugs);
            Assert.Contains("a1-yes-no-questions", faDuVersusSieDetail.PrerequisiteSlugs);
            Assert.Contains("a1-polite-requests-with-moechte", faDuVersusSieDetail.PrerequisiteSlugs);
            Assert.Contains("a1-imperative-basics", faDuVersusSieDetail.PrerequisiteSlugs);
            Assert.Contains("a2-imperative-formal-and-informal", faDuVersusSieDetail.RelatedTopicSlugs);
            Assert.Contains("a2-indirect-questions-introduction", faDuVersusSieDetail.RelatedTopicSlugs);
            Assert.Contains("a1-question-answer-sentence-patterns", faDuVersusSieDetail.RelatedTopicSlugs);
            Assert.Contains(
                faDuVersusSieDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDuVersusSieDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDuVersusSieDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faDuVersusSieDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faDuVersusSieDetail.Examples, example => example.GermanText == "Du bist mein Freund.");
            Assert.Contains(faDuVersusSieDetail.Examples, example => example.GermanText == "Wohnen Sie hier?");
            Assert.Contains(faDuVersusSieDetail.CommonMistakes, mistake => mistake.WrongText == "Du sind Herr Weber?");
            Assert.Contains(faDuVersusSieDetail.CommonMistakes, mistake => mistake.WrongText == "Kommst Sie morgen?");

            GrammarTopicDetailModel? faBasicSentenceNegationDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-basic-sentence-negation",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faBasicSentenceNegationDetail);
            Assert.Contains("نفی", faBasicSentenceNegationDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, faBasicSentenceNegationDetail.Sections.Count);
            Assert.True(faBasicSentenceNegationDetail.Examples.Count >= 60);
            Assert.Equal(18, faBasicSentenceNegationDetail.RuleSummaries.Count);
            Assert.True(faBasicSentenceNegationDetail.CommonMistakes.Count >= 25);
            Assert.Contains("a1-kein-versus-nicht-basics", faBasicSentenceNegationDetail.PrerequisiteSlugs);
            Assert.Contains("a1-simple-accusative-introduction", faBasicSentenceNegationDetail.PrerequisiteSlugs);
            Assert.Contains("a1-sein-in-praesens", faBasicSentenceNegationDetail.PrerequisiteSlugs);
            Assert.Contains("a1-haben-in-praesens", faBasicSentenceNegationDetail.PrerequisiteSlugs);
            Assert.Contains("a1-regular-verbs-in-praesens", faBasicSentenceNegationDetail.PrerequisiteSlugs);
            Assert.Contains("a1-word-order-with-time-and-place", faBasicSentenceNegationDetail.PrerequisiteSlugs);
            Assert.Contains("a2-dass-clauses", faBasicSentenceNegationDetail.RelatedTopicSlugs);
            Assert.Contains("a2-denn-versus-weil", faBasicSentenceNegationDetail.RelatedTopicSlugs);
            Assert.Contains("a2-accusative-versus-dative-basics", faBasicSentenceNegationDetail.RelatedTopicSlugs);
            Assert.Contains(
                faBasicSentenceNegationDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faBasicSentenceNegationDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faBasicSentenceNegationDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faBasicSentenceNegationDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faBasicSentenceNegationDetail.Examples, example => example.GermanText == "Ich bin nicht müde.");
            Assert.Contains(faBasicSentenceNegationDetail.Examples, example => example.GermanText == "Ich habe keinen Termin.");
            Assert.Contains(faBasicSentenceNegationDetail.CommonMistakes, mistake => mistake.WrongText == "Ich nicht komme.");
            Assert.Contains(faBasicSentenceNegationDetail.CommonMistakes, mistake => mistake.WrongText == "Ich habe nicht Termin.");

            GrammarTopicDetailModel? faQuestionAnswerPatternsDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-question-answer-sentence-patterns",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faQuestionAnswerPatternsDetail);
            Assert.Contains("پرسش", faQuestionAnswerPatternsDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, faQuestionAnswerPatternsDetail.Sections.Count);
            Assert.True(faQuestionAnswerPatternsDetail.Examples.Count >= 70);
            Assert.Equal(18, faQuestionAnswerPatternsDetail.RuleSummaries.Count);
            Assert.True(faQuestionAnswerPatternsDetail.CommonMistakes.Count >= 25);
            Assert.Contains("a1-yes-no-questions", faQuestionAnswerPatternsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-w-questions-wer-was-wo-wann-wie", faQuestionAnswerPatternsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-basic-sentence-negation", faQuestionAnswerPatternsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-pronoun-and-verb-agreement", faQuestionAnswerPatternsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-formal-sie", faQuestionAnswerPatternsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-du-versus-sie-grammar-basics", faQuestionAnswerPatternsDetail.PrerequisiteSlugs);
            Assert.Contains("a1-basic-appointment-phrases", faQuestionAnswerPatternsDetail.RelatedTopicSlugs);
            Assert.Contains("a1-basic-location-phrases", faQuestionAnswerPatternsDetail.RelatedTopicSlugs);
            Assert.Contains("a2-indirect-questions-introduction", faQuestionAnswerPatternsDetail.RelatedTopicSlugs);
            Assert.Contains("b1-grammar-for-b1-speaking-exam", faQuestionAnswerPatternsDetail.RelatedTopicSlugs);
            Assert.Contains(
                faQuestionAnswerPatternsDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faQuestionAnswerPatternsDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faQuestionAnswerPatternsDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faQuestionAnswerPatternsDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faQuestionAnswerPatternsDetail.Examples, example => example.GermanText == "Wohnst du hier? Ja, ich wohne hier.");
            Assert.Contains(faQuestionAnswerPatternsDetail.Examples, example => example.GermanText == "Hast du Zeit? Nein, ich habe keine Zeit.");
            Assert.Contains(faQuestionAnswerPatternsDetail.CommonMistakes, mistake => mistake.WrongText == "Wohnst du hier? Ja.");
            Assert.Contains(faQuestionAnswerPatternsDetail.CommonMistakes, mistake => mistake.WrongText == "Wo wohnst du? Wo ich wohne in Berlin.");

            GrammarTopicDetailModel? faFoodShoppingArticlesDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-articles-with-food-drinks-and-shopping-nouns",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faFoodShoppingArticlesDetail);
            Assert.Contains("غذا", faFoodShoppingArticlesDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, faFoodShoppingArticlesDetail.Sections.Count);
            Assert.True(faFoodShoppingArticlesDetail.Examples.Count >= 60);
            Assert.Equal(19, faFoodShoppingArticlesDetail.RuleSummaries.Count);
            Assert.True(faFoodShoppingArticlesDetail.CommonMistakes.Count >= 25);
            Assert.Contains("a1-definite-articles-der-die-das", faFoodShoppingArticlesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-indefinite-articles-ein-eine", faFoodShoppingArticlesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-noun-gender-basics", faFoodShoppingArticlesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-plural-basics", faFoodShoppingArticlesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-simple-accusative-introduction", faFoodShoppingArticlesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-polite-requests-with-moechte", faFoodShoppingArticlesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-basic-appointment-phrases", faFoodShoppingArticlesDetail.RelatedTopicSlugs);
            Assert.Contains("a2-accusative-versus-dative-basics", faFoodShoppingArticlesDetail.RelatedTopicSlugs);
            Assert.Contains("a1-kein-versus-nicht-basics", faFoodShoppingArticlesDetail.RelatedTopicSlugs);
            Assert.Contains(
                faFoodShoppingArticlesDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faFoodShoppingArticlesDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faFoodShoppingArticlesDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faFoodShoppingArticlesDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faFoodShoppingArticlesDetail.Examples, example => example.GermanText == "Ich möchte einen Kaffee.");
            Assert.Contains(faFoodShoppingArticlesDetail.Examples, example => example.GermanText == "Ich kaufe ein Brot.");
            Assert.Contains(faFoodShoppingArticlesDetail.CommonMistakes, mistake => mistake.WrongText == "Ich möchte ein Kaffee.");
            Assert.Contains(faFoodShoppingArticlesDetail.CommonMistakes, mistake => mistake.WrongText == "Ich kaufe der Kaffee.");

            GrammarTopicDetailModel? faBasicLocationPhrasesDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-basic-location-phrases",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faBasicLocationPhrasesDetail);
            Assert.Contains("مکان", faBasicLocationPhrasesDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, faBasicLocationPhrasesDetail.Sections.Count);
            Assert.True(faBasicLocationPhrasesDetail.Examples.Count >= 60);
            Assert.Equal(18, faBasicLocationPhrasesDetail.RuleSummaries.Count);
            Assert.True(faBasicLocationPhrasesDetail.CommonMistakes.Count >= 22);
            Assert.Contains("a1-basic-prepositions-in-aus-nach-bei", faBasicLocationPhrasesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-word-order-with-time-and-place", faBasicLocationPhrasesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-w-questions-wer-was-wo-wann-wie", faBasicLocationPhrasesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-sein-in-praesens", faBasicLocationPhrasesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-regular-verbs-in-praesens", faBasicLocationPhrasesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-basic-appointment-phrases", faBasicLocationPhrasesDetail.RelatedTopicSlugs);
            Assert.Contains("a2-wechselpraepositionen-introduction", faBasicLocationPhrasesDetail.RelatedTopicSlugs);
            Assert.Contains("a2-prepositions-with-dative", faBasicLocationPhrasesDetail.RelatedTopicSlugs);
            Assert.Contains("a2-prepositions-with-accusative", faBasicLocationPhrasesDetail.RelatedTopicSlugs);
            Assert.Contains(
                faBasicLocationPhrasesDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faBasicLocationPhrasesDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faBasicLocationPhrasesDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faBasicLocationPhrasesDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faBasicLocationPhrasesDetail.Examples, example => example.GermanText == "Ich wohne in Berlin.");
            Assert.Contains(faBasicLocationPhrasesDetail.Examples, example => example.GermanText == "Ich bin zu Hause.");
            Assert.Contains(faBasicLocationPhrasesDetail.CommonMistakes, mistake => mistake.WrongText == "Ich bin nach Hause.");
            Assert.Contains(faBasicLocationPhrasesDetail.CommonMistakes, mistake => mistake.WrongText == "Ich bin in Schule.");

            GrammarTopicDetailModel? faBasicAppointmentPhrasesDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-basic-appointment-phrases",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faBasicAppointmentPhrasesDetail);
            Assert.Contains("وقت", faBasicAppointmentPhrasesDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, faBasicAppointmentPhrasesDetail.Sections.Count);
            Assert.True(faBasicAppointmentPhrasesDetail.Examples.Count >= 70);
            Assert.Equal(18, faBasicAppointmentPhrasesDetail.RuleSummaries.Count);
            Assert.True(faBasicAppointmentPhrasesDetail.CommonMistakes.Count >= 25);
            Assert.Contains("a1-haben-in-praesens", faBasicAppointmentPhrasesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-polite-requests-with-moechte", faBasicAppointmentPhrasesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-simple-accusative-introduction", faBasicAppointmentPhrasesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-numbers-and-grammar-use", faBasicAppointmentPhrasesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-time-expressions-heute-morgen-gestern", faBasicAppointmentPhrasesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-word-order-with-time-and-place", faBasicAppointmentPhrasesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-formal-sie", faBasicAppointmentPhrasesDetail.PrerequisiteSlugs);
            Assert.Contains("a2-grammar-for-appointments", faBasicAppointmentPhrasesDetail.RelatedTopicSlugs);
            Assert.Contains("a2-indirect-questions-introduction", faBasicAppointmentPhrasesDetail.RelatedTopicSlugs);
            Assert.Contains("a2-polite-forms-with-wuerde", faBasicAppointmentPhrasesDetail.RelatedTopicSlugs);
            Assert.Contains("a1-question-answer-sentence-patterns", faBasicAppointmentPhrasesDetail.RelatedTopicSlugs);
            Assert.Contains(
                faBasicAppointmentPhrasesDetail.Sections,
                section => section.SectionKey == "form-or-structure-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faBasicAppointmentPhrasesDetail.Sections,
                section => section.SectionKey == "word-order-or-case-focus" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faBasicAppointmentPhrasesDetail.Sections,
                section => section.SectionKey == "comparison-table" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faBasicAppointmentPhrasesDetail.Sections,
                section => section.SectionKey == "common-patterns" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faBasicAppointmentPhrasesDetail.Examples, example => example.GermanText == "Ich habe einen Termin.");
            Assert.Contains(faBasicAppointmentPhrasesDetail.Examples, example => example.GermanText == "Der Termin ist am Montag um 10 Uhr.");
            Assert.Contains(faBasicAppointmentPhrasesDetail.CommonMistakes, mistake => mistake.WrongText == "Ich habe ein Termin.");
            Assert.Contains(faBasicAppointmentPhrasesDetail.CommonMistakes, mistake => mistake.WrongText == "Ich komme am 10 Uhr.");

            GrammarTopicDetailModel? faCommonA1MistakesDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-common-a1-grammar-mistakes",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faCommonA1MistakesDetail);
            Assert.Contains("اشتباه", faCommonA1MistakesDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(10, faCommonA1MistakesDetail.Sections.Count);
            Assert.True(faCommonA1MistakesDetail.Examples.Count >= 45);
            Assert.True(faCommonA1MistakesDetail.CommonMistakes.Count >= 50);
            Assert.True(faCommonA1MistakesDetail.CommonMistakes.Count >= 80);
            Assert.Equal(20, faCommonA1MistakesDetail.RuleSummaries.Count);
            Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", faCommonA1MistakesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-sein-in-praesens", faCommonA1MistakesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-haben-in-praesens", faCommonA1MistakesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-regular-verbs-in-praesens", faCommonA1MistakesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-verb-position-in-simple-sentences", faCommonA1MistakesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-yes-no-questions", faCommonA1MistakesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-w-questions-wer-was-wo-wann-wie", faCommonA1MistakesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-definite-articles-der-die-das", faCommonA1MistakesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-indefinite-articles-ein-eine", faCommonA1MistakesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-noun-gender-basics", faCommonA1MistakesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-plural-basics", faCommonA1MistakesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-nominative-case", faCommonA1MistakesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-simple-accusative-introduction", faCommonA1MistakesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-kein-versus-nicht-basics", faCommonA1MistakesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-basic-sentence-negation", faCommonA1MistakesDetail.PrerequisiteSlugs);
            Assert.Contains("a1-a1-grammar-review-map", faCommonA1MistakesDetail.RelatedTopicSlugs);
            Assert.Contains("a2-common-a2-mistakes", faCommonA1MistakesDetail.RelatedTopicSlugs);
            Assert.Contains("a2-a2-grammar-review-map", faCommonA1MistakesDetail.RelatedTopicSlugs);
            Assert.Contains(
                faCommonA1MistakesDetail.Sections,
                section => section.SectionKey == "why-mistakes-are-normal" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faCommonA1MistakesDetail.Sections,
                section => section.SectionKey == "mistake-type-verb-endings" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faCommonA1MistakesDetail.Sections,
                section => section.SectionKey == "correction-strategy" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faCommonA1MistakesDetail.Examples, example => example.GermanText == "Heute lerne ich Deutsch.");
            Assert.Contains(faCommonA1MistakesDetail.Examples, example => example.GermanText == "Ich habe einen Termin.");
            Assert.Contains(faCommonA1MistakesDetail.CommonMistakes, mistake => mistake.WrongText == "Ich lernst Deutsch.");
            Assert.Contains(faCommonA1MistakesDetail.CommonMistakes, mistake => mistake.WrongText == "Heute ich lerne Deutsch.");

            GrammarTopicDetailModel? faA1ReviewMapDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-a1-grammar-review-map",
                "fa",
                CancellationToken.None);

            Assert.NotNull(faA1ReviewMapDetail);
            Assert.Contains("مرور", faA1ReviewMapDetail!.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(12, faA1ReviewMapDetail.Sections.Count);
            Assert.True(faA1ReviewMapDetail.Examples.Count >= 80);
            Assert.True(faA1ReviewMapDetail.CommonMistakes.Count >= 40);
            Assert.True(faA1ReviewMapDetail.RuleSummaries.Count >= 25);
            Assert.Contains("a1-personal-pronouns-ich-du-er-sie-es", faA1ReviewMapDetail.PrerequisiteSlugs);
            Assert.Contains("a1-common-a1-grammar-mistakes", faA1ReviewMapDetail.PrerequisiteSlugs);
            Assert.Contains("a2-a2-grammar-review-map", faA1ReviewMapDetail.RelatedTopicSlugs);
            Assert.Contains("a2-perfekt-with-haben", faA1ReviewMapDetail.RelatedTopicSlugs);
            Assert.Contains("a2-dative-case-basics", faA1ReviewMapDetail.RelatedTopicSlugs);
            Assert.Contains("a2-dass-clauses", faA1ReviewMapDetail.RelatedTopicSlugs);
            Assert.Contains("a2-weil-clauses", faA1ReviewMapDetail.RelatedTopicSlugs);
            Assert.Contains("a2-adjective-endings-introduction", faA1ReviewMapDetail.RelatedTopicSlugs);
            Assert.Contains(
                faA1ReviewMapDetail.Sections,
                section => section.SectionKey == "a1-core-path" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faA1ReviewMapDetail.Sections,
                section => section.SectionKey == "sentence-building-map" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faA1ReviewMapDetail.Sections,
                section => section.SectionKey == "question-map" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faA1ReviewMapDetail.Sections,
                section => section.SectionKey == "noun-and-article-map" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(
                faA1ReviewMapDetail.Sections,
                section => section.SectionKey == "self-checklist" && section.Blocks.Any(block => block.Type == "table" && block.Columns.Count > 0 && block.Rows.Count > 0));
            Assert.Contains(faA1ReviewMapDetail.Examples, example => example.GermanText == "Ich heiße Sara.");
            Assert.Contains(faA1ReviewMapDetail.Examples, example => example.GermanText == "Heute lerne ich Deutsch.");
            Assert.Contains(faA1ReviewMapDetail.CommonMistakes, mistake => mistake.WrongText == "Ich bist neu.");
            Assert.Contains(faA1ReviewMapDetail.CommonMistakes, mistake => mistake.WrongText == "Heute ich lerne Deutsch.");

            JsonNode updatePackage = JsonNode.Parse(await File.ReadAllTextAsync(packagePath))!;
            updatePackage["grammarTopics"]![0]!["contentRevision"] = 2;
            updatePackage["grammarTopics"]![0]!["titleLocalized"]!["en"] = "Updated personal pronouns";
            await File.WriteAllTextAsync(updatePackagePath, updatePackage.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

            ImportContentPackageResult updateResult = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(updatePackagePath), CancellationToken.None);

            Assert.True(updateResult.IsSuccess, string.Join(Environment.NewLine, updateResult.Issues.Select(issue => issue.Message)));
            Assert.StartsWith("grammar-a1-core-v1-reimport-", updateResult.PackageId, StringComparison.Ordinal);

            await using DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext dbContext = serviceProvider
                .GetRequiredService<IDbContextFactory<DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext>>()
                .CreateDbContext();
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-personal-pronouns-ich-du-er-sie-es"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-sein-in-praesens"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-haben-in-praesens"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-regular-verbs-in-praesens"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-verb-position-in-simple-sentences"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-yes-no-questions"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-w-questions-wer-was-wo-wann-wie"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-definite-articles-der-die-das"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-indefinite-articles-ein-eine"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-noun-gender-basics"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-plural-basics"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-nominative-case"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-simple-accusative-introduction"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-kein-versus-nicht-basics"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-possessive-pronouns-mein-dein"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-basic-adjective-position"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-basic-prepositions-in-aus-nach-bei"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-numbers-and-grammar-use"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-time-expressions-heute-morgen-gestern"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-word-order-with-time-and-place"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-simple-modal-verbs-koennen-muessen-wollen"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-polite-requests-with-moechte"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-imperative-basics"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-separable-verbs-introduction"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-simple-conjunctions-und-aber"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-pronoun-and-verb-agreement"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-formal-sie"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-du-versus-sie-grammar-basics"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-basic-sentence-negation"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-question-answer-sentence-patterns"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-articles-with-food-drinks-and-shopping-nouns"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-basic-location-phrases"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-basic-appointment-phrases"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-common-a1-grammar-mistakes"));
            Assert.Equal(1, await dbContext.GrammarTopics.CountAsync(topic => topic.Slug == "a1-a1-grammar-review-map"));

            GrammarTopicDetailModel? enDetail = await grammarQueryService.GetPublishedGrammarTopicBySlugAsync(
                "a1-personal-pronouns-ich-du-er-sie-es",
                "en",
                CancellationToken.None);
            Assert.Equal("Updated personal pronouns", enDetail?.Title);
            Assert.Equal(2, enDetail?.ContentRevision);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(databasePath))
            {
                TryDeleteFile(databasePath);
            }

            if (File.Exists(updatePackagePath))
            {
                File.Delete(updatePackagePath);
            }
        }
    }

    [Fact]
    public async Task ImportAsync_ShouldRejectInvalidRichGrammarBlocks()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-grammar-rich-invalid-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(ResolveRepositoryRoot(), "content", "learning-portal", "grammar", "packages", "grammar-a1-core-v1.json");
        string invalidPackagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-grammar-rich-invalid-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            JsonNode package = JsonNode.Parse(await File.ReadAllTextAsync(packagePath))!;
            package["packageId"] = "grammar-a1-core-invalid-rich-blocks-test";
            JsonNode topic = package["grammarTopics"]![0]!;
            topic["titleLocalized"]!["zz"] = "Unsupported language";
            JsonNode section = topic["sections"]![1]!;
            section["sectionKey"] = "";
            section["localizedBlocks"]!["en"]![0]!["type"] = "unknown-block";
            section["localizedBlocks"]!["en"]![0]!["rows"] = new JsonArray();
            JsonNode duplicateSectionPackage = JsonNode.Parse(await File.ReadAllTextAsync(packagePath))!;
            string duplicatedSectionKey = duplicateSectionPackage["grammarTopics"]![0]!["sections"]![0]!["sectionKey"]!.GetValue<string>();
            duplicateSectionPackage["grammarTopics"]![0]!["sections"]![1]!["sectionKey"] = duplicatedSectionKey;
            await File.WriteAllTextAsync(invalidPackagePath, package.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

            serviceProvider = BuildServiceProvider(databasePath);
            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(invalidPackagePath), CancellationToken.None);

            Assert.False(result.IsSuccess);
            string issueText = string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message));
            Assert.Contains("sectionKey is required", issueText, StringComparison.Ordinal);
            Assert.Contains("unsupported block type 'unknown-block'", issueText, StringComparison.Ordinal);
            Assert.Contains("language 'zz' is not an active meaning language", issueText, StringComparison.Ordinal);

            string duplicatePackagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-grammar-rich-duplicate-section-{Guid.NewGuid():N}.json");
            try
            {
                duplicateSectionPackage["packageId"] = "grammar-a1-core-duplicate-section-test";
                await File.WriteAllTextAsync(duplicatePackagePath, duplicateSectionPackage.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

                ImportContentPackageResult duplicateResult = await contentImportService
                    .ImportAsync(new ImportContentPackageRequest(duplicatePackagePath), CancellationToken.None);

                Assert.False(duplicateResult.IsSuccess);
                Assert.Contains($"sectionKey '{duplicatedSectionKey}' is duplicated", string.Join(Environment.NewLine, duplicateResult.Issues.Select(issue => issue.Message)), StringComparison.Ordinal);
            }
            finally
            {
                if (File.Exists(duplicatePackagePath))
                {
                    File.Delete(duplicatePackagePath);
                }
            }
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(databasePath))
            {
                TryDeleteFile(databasePath);
            }

            if (File.Exists(invalidPackagePath))
            {
                File.Delete(invalidPackagePath);
            }
        }
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
        }
    }

    /// <summary>
    /// Verifies that lexicalForms import populates additional lexical roles while preserving the primary role.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldImportMultipleLexicalForms()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreatePackageWithMultipleLexicalFormsJson("a1-shopping-multi-lexical-forms"));

            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
            Assert.Equal(1, result.ImportedEntries);

            IWordQueryService wordQueryService = serviceProvider.GetRequiredService<IWordQueryService>();
            DarwinLingua.Catalog.Application.Models.WordListItemModel importedWord = Assert.Single(await wordQueryService
                .GetWordsByTopicAsync("shopping", "en", CancellationToken.None));

            IWordDetailQueryService detailQueryService = serviceProvider.GetRequiredService<IWordDetailQueryService>();
            DarwinLingua.Catalog.Application.Models.WordDetailModel? detail = await detailQueryService
                .GetWordDetailsAsync(importedWord.PublicId, "en", null, "en", CancellationToken.None);

            Assert.NotNull(detail);
            Assert.Equal(2, detail!.LexicalForms.Count);
            Assert.Contains(detail.LexicalForms, form => form.PartOfSpeech == "Noun" && form.IsPrimary && form.Article == "die");
            Assert.Contains(detail.LexicalForms, form => form.PartOfSpeech == "Verb" && !form.IsPrimary && form.InfinitiveForm == "Kasse machen");
            Assert.Equal("/ˈkasə/", detail.PronunciationIpa);
            Assert.Equal("Kas-se", detail.SyllableBreak);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    /// <summary>
    /// Verifies that the compact collection wordKeys format is accepted for AI-generated content packages.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldImportCollectionWordKeys()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreatePackageWithCollectionWordKeysJson("a1-shopping-collection-word-keys"));

            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
            Assert.Equal(2, result.ImportedEntries);

            IWordCollectionQueryService collectionQueryService = serviceProvider.GetRequiredService<IWordCollectionQueryService>();
            DarwinLingua.Catalog.Application.Models.WordCollectionDetailModel? collection = await collectionQueryService
                .GetPublishedCollectionBySlugAsync("a1-shopping-word-keys", "en", CancellationToken.None);

            Assert.NotNull(collection);
            Assert.Equal("collections/a1-shopping-word-keys.png", collection!.ImageUrl);
            Assert.Equal(2, collection.Words.Count);
            Assert.Contains(collection.Words, word => word.Lemma == "Brot");
            Assert.Contains(collection.Words, word => word.Lemma == "Milch");
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    /// <summary>
    /// Verifies that collection-only packages can attach existing imported words without carrying a duplicate anchor entry.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldImportCollectionOnlyPackage()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string wordsPackagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-words-{Guid.NewGuid():N}.json");
        string collectionPackagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-collection-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(wordsPackagePath, CreatePackageWithCollectionWordKeysJson("a1-shopping-collection-only-words"));
            await File.WriteAllTextAsync(collectionPackagePath, CreateCollectionOnlyPackageJson("a1-shopping-collection-only-package"));

            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult wordsResult = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(wordsPackagePath), CancellationToken.None);
            ImportContentPackageResult collectionResult = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(collectionPackagePath), CancellationToken.None);

            Assert.True(wordsResult.IsSuccess, string.Join(Environment.NewLine, wordsResult.Issues.Select(issue => issue.Message)));
            Assert.True(collectionResult.IsSuccess, string.Join(Environment.NewLine, collectionResult.Issues.Select(issue => issue.Message)));
            Assert.Equal(0, collectionResult.TotalEntries);
            Assert.Equal(0, collectionResult.ImportedEntries);

            IWordCollectionQueryService collectionQueryService = serviceProvider.GetRequiredService<IWordCollectionQueryService>();
            DarwinLingua.Catalog.Application.Models.WordCollectionDetailModel? collection = await collectionQueryService
                .GetPublishedCollectionBySlugAsync("a1-shopping-collection-only", "en", CancellationToken.None);

            Assert.NotNull(collection);
            Assert.Equal(2, collection!.Words.Count);
            Assert.Contains(collection.Words, word => word.Lemma == "Brot");
            Assert.Contains(collection.Words, word => word.Lemma == "Milch");
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(wordsPackagePath))
            {
                File.Delete(wordsPackagePath);
            }

            if (File.Exists(collectionPackagePath))
            {
                File.Delete(collectionPackagePath);
            }
        }
    }

    /// <summary>
    /// Verifies that valid Dialogue lessons from a content package are persisted with their nested content.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldPersistDialogueLessons()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreatePackageWithDialogueJson("a1-Dialogue-import-test"));

            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
            Assert.Equal(1, result.ImportedEntries);

            await using DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext dbContext = serviceProvider
                .GetRequiredService<Microsoft.EntityFrameworkCore.IDbContextFactory<DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext>>()
                .CreateDbContext();

            DarwinLingua.Catalog.Domain.Entities.DialogueLesson lesson = Assert.Single(dbContext.DialogueLessons
                .Include(Dialogue => Dialogue.Topics)
                .Include(Dialogue => Dialogue.DialogueTurns).ThenInclude(turn => turn.Translations)
                .Include(Dialogue => Dialogue.UsefulPhrases).ThenInclude(phrase => phrase.Translations)
                .Include(Dialogue => Dialogue.Questions).ThenInclude(question => question.Translations)
                .Include(Dialogue => Dialogue.Questions).ThenInclude(question => question.Answers).ThenInclude(answer => answer.Translations));

            Assert.Equal("doctor-appointment-a1", lesson.Slug);
            Assert.Single(lesson.Topics);
            Assert.Equal(10, lesson.DialogueTurns.Count);
            Assert.Single(lesson.UsefulPhrases);
            DarwinLingua.Catalog.Domain.Entities.DialogueQuestion question = Assert.Single(lesson.Questions);
            Assert.Equal(2, question.Answers.Count);
            Assert.Contains(question.Answers, answer => answer.IsCorrect);

            IDialogueLessonQueryService DialogueLessonQueryService = serviceProvider.GetRequiredService<IDialogueLessonQueryService>();
            IReadOnlyList<DarwinLingua.Catalog.Application.Models.DialogueLessonListItemModel> Dialogues =
                await DialogueLessonQueryService.GetPublishedDialoguesAsync(new DialogueLessonListFilterModel(null, null, null, null, null, null, null, null, null), CancellationToken.None);
            DarwinLingua.Catalog.Application.Models.DialogueLessonListItemModel DialogueListItem = Assert.Single(Dialogues);
            Assert.Equal("doctor-appointment-a1", DialogueListItem.Slug);

            DarwinLingua.Catalog.Application.Models.DialogueLessonDetailModel? DialogueDetail =
                await DialogueLessonQueryService.GetPublishedDialogueBySlugAsync(
                    "doctor-appointment-a1",
                    "fa",
                    "en",
                    CancellationToken.None);

            Assert.NotNull(DialogueDetail);
            DarwinLingua.Catalog.Application.Models.DialogueTurnModel firstLearnerTurn = DialogueDetail!.DialogueTurns
                .First(turn => turn.SpeakerRole == "learner");
            Assert.Equal("I need an appointment.", firstLearnerTurn.PrimaryMeaning);
            Assert.Equal("I need an appointment.", firstLearnerTurn.SecondaryMeaning);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    /// <summary>
    /// Verifies that valid conversation starter packs are persisted and queryable with dual meaning-language fallback.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldPersistConversationStarterPacks()
    {
        string databaseName = $"darwin_starter_import_{Guid.NewGuid():N}"[..46];
        string connectionString = BuildPostgresConnectionString(databaseName);
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        await CreatePostgresDatabaseAsync(databaseName, CancellationToken.None);

        try
        {
            await File.WriteAllTextAsync(packagePath, CreatePackageWithConversationStarterJson("a1-starter-import-test"));

            serviceProvider = BuildPostgresServiceProvider(connectionString);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
            Assert.Equal(1, result.ImportedEntries);

            await using DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext dbContext = serviceProvider
                .GetRequiredService<Microsoft.EntityFrameworkCore.IDbContextFactory<DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext>>()
                .CreateDbContext();

            DarwinLingua.Catalog.Domain.Entities.ConversationStarterPack pack = Assert.Single(dbContext.ConversationStarterPacks
                .Include(starter => starter.Topics)
                .Include(starter => starter.LinkedDialogues)
                .Include(starter => starter.LinkedEventPreparationPacks)
                .Include(starter => starter.Phrases).ThenInclude(phrase => phrase.Translations)
                .Include(starter => starter.Phrases).ThenInclude(phrase => phrase.AlternativeBaseTexts));

            Assert.Equal("a1-cafe-first-meeting", pack.Slug);
            Assert.Single(pack.Topics);
            Assert.Equal("doctor-appointment-a1", Assert.Single(pack.LinkedDialogues).DialogueSlug);
            DarwinLingua.Catalog.Domain.Entities.ConversationStarterPhrase phrase = Assert.Single(pack.Phrases);
            Assert.Equal("opening", phrase.Function);
            Assert.Equal("Hallo, ich heisse Sara.", Assert.Single(phrase.AlternativeBaseTexts).BaseText);
            Assert.Equal(10, phrase.Translations.Count);

            IConversationStarterQueryService queryService = serviceProvider.GetRequiredService<IConversationStarterQueryService>();
            IReadOnlyList<DarwinLingua.Catalog.Application.Models.ConversationStarterPackListItemModel> starterPacks =
                await queryService.GetPublishedStarterPacksAsync(
                    new DarwinLingua.Catalog.Application.Models.ConversationStarterListFilterModel("A1", "cafe", "friendly", "introduction", "everyday-life"),
                    CancellationToken.None);

            DarwinLingua.Catalog.Application.Models.ConversationStarterPackListItemModel listItem = Assert.Single(starterPacks);
            Assert.Equal("a1-cafe-first-meeting", listItem.Slug);
            Assert.Equal(["doctor-appointment-a1"], listItem.LinkedDialogueSlugs);

            IReadOnlyList<DarwinLingua.Catalog.Application.Models.ConversationStarterPackListItemModel> DialogueStarterPacks =
                await queryService.GetPublishedStarterPacksForDialogueAsync("doctor-appointment-a1", CancellationToken.None);
            Assert.Equal("a1-cafe-first-meeting", Assert.Single(DialogueStarterPacks).Slug);

            DarwinLingua.Catalog.Application.Models.ConversationStarterPackDetailModel? detail =
                await queryService.GetPublishedStarterPackBySlugAsync(
                    "a1-cafe-first-meeting",
                    "fa",
                    "en",
                    CancellationToken.None);

            Assert.NotNull(detail);
            DarwinLingua.Catalog.Application.Models.ConversationStarterPhraseModel detailPhrase = Assert.Single(detail!.Phrases);
            Assert.Equal("سلام، اسم من سارا است.", detailPhrase.PrimaryMeaning);
            Assert.Equal("Hello, my name is Sara.", detailPhrase.SecondaryMeaning);
            Assert.Equal(["Hallo, ich heisse Sara."], detailPhrase.AlternativeBaseTexts);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }

            await DropPostgresDatabaseAsync(databaseName, CancellationToken.None);
        }
    }

    /// <summary>
    /// Verifies that valid event preparation packs are persisted with links and prompt groups.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldPersistEventPreparationPacks()
    {
        string databaseName = $"darwin_event_prep_import_{Guid.NewGuid():N}"[..46];
        string connectionString = BuildPostgresConnectionString(databaseName);
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        await CreatePostgresDatabaseAsync(databaseName, CancellationToken.None);

        try
        {
            await File.WriteAllTextAsync(packagePath, CreatePackageWithEventPreparationJson("a1-event-preparation-import-test"));

            serviceProvider = BuildPostgresServiceProvider(connectionString);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
            Assert.Equal(1, result.ImportedEntries);

            await using DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext dbContext = serviceProvider
                .GetRequiredService<Microsoft.EntityFrameworkCore.IDbContextFactory<DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext>>()
                .CreateDbContext();

            DarwinLingua.Catalog.Domain.Entities.EventPreparationPack pack = Assert.Single(dbContext.EventPreparationPacks
                .Include(item => item.Topics)
                .Include(item => item.LinkedDialogues)
                .Include(item => item.LinkedConversationStarterPacks)
                .Include(item => item.LinkedVocabulary)
                .Include(item => item.Prompts));

            Assert.Equal("a1-first-cafe-event", pack.Slug);
            Assert.Single(pack.Topics);
            Assert.Equal("cafe-first-meeting-a1", Assert.Single(pack.LinkedDialogues).DialogueSlug);
            Assert.Equal("a1-cafe-first-meeting", Assert.Single(pack.LinkedConversationStarterPacks).ConversationStarterPackSlug);
            Assert.Equal("Name", Assert.Single(pack.LinkedVocabulary).Word);
            Assert.Equal(3, pack.Prompts.Count);
            Assert.Contains(pack.Prompts, prompt => prompt.PromptType == "opening" && prompt.Text == "Say your name and ask for the other person's name.");
            Assert.Contains(pack.Prompts, prompt => prompt.PromptType == "roleplay" && prompt.Text == "Start a two-minute cafe introduction.");
            Assert.Contains(pack.Prompts, prompt => prompt.PromptType == "review" && prompt.Text == "Write one phrase you want to reuse.");

            IEventPreparationQueryService queryService = serviceProvider.GetRequiredService<IEventPreparationQueryService>();
            IReadOnlyList<DarwinLingua.Catalog.Application.Models.EventPreparationPackListItemModel> eventPreparationPacks =
                await queryService.GetPublishedEventPreparationPacksAsync(
                    new DarwinLingua.Catalog.Application.Models.EventPreparationListFilterModel("A1", "social-event", "conversation-cafe", "everyday-life"),
                    CancellationToken.None);

            DarwinLingua.Catalog.Application.Models.EventPreparationPackListItemModel listItem = Assert.Single(eventPreparationPacks);
            Assert.Equal("a1-first-cafe-event", listItem.Slug);
            Assert.Equal(["cafe-first-meeting-a1"], listItem.LinkedDialogueSlugs);
            Assert.Equal(["a1-cafe-first-meeting"], listItem.LinkedConversationStarterPackSlugs);

            IReadOnlyList<DarwinLingua.Catalog.Application.Models.EventPreparationPackListItemModel> DialoguePreparationPacks =
                await queryService.GetPublishedEventPreparationPacksForDialogueAsync("cafe-first-meeting-a1", CancellationToken.None);
            Assert.Equal("a1-first-cafe-event", Assert.Single(DialoguePreparationPacks).Slug);

            DarwinLingua.Catalog.Application.Models.EventPreparationPackDetailModel? detail =
                await queryService.GetPublishedEventPreparationPackBySlugAsync("a1-first-cafe-event", CancellationToken.None);

            Assert.NotNull(detail);
            Assert.Equal("Name", Assert.Single(detail!.LinkedVocabulary).Word);
            Assert.Equal(3, detail.Prompts.Count);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }

            await DropPostgresDatabaseAsync(databaseName, CancellationToken.None);
        }
    }

    /// <summary>
    /// Verifies that the Phase 1 sample package imports successfully into a freshly initialized database.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldImportPhase1SampleContentPackageIntoFreshDatabase()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            File.Copy(GetSamplePackagePath(), packagePath, overwrite: true);

            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
            Assert.Equal("Completed", result.Status);
            Assert.Equal(12, result.TotalEntries);
            Assert.Equal(12, result.ImportedEntries);
            Assert.Equal(0, result.SkippedDuplicateEntries);
            Assert.Equal(0, result.InvalidEntries);
            Assert.Equal(0, result.WarningCount);
            Assert.Equal(12, result.ImportedLemmas.Count);
            Assert.Contains("Brot", result.ImportedLemmas);
            Assert.Contains("Unabdingbarkeit", result.ImportedLemmas);

            IWordQueryService wordQueryService = serviceProvider.GetRequiredService<IWordQueryService>();

            IReadOnlyList<DarwinLingua.Catalog.Application.Models.WordListItemModel> shoppingWords = await wordQueryService
                .GetWordsByTopicAsync("shopping", "en", CancellationToken.None);
            IReadOnlyList<DarwinLingua.Catalog.Application.Models.WordListItemModel> workWords = await wordQueryService
                .GetWordsByTopicAsync("work-and-jobs", "en", CancellationToken.None);
            IReadOnlyList<DarwinLingua.Catalog.Application.Models.WordListItemModel> c2Words = await wordQueryService
                .GetWordsByCefrAsync("C2", "en", CancellationToken.None);

            DarwinLingua.Catalog.Application.Models.WordListItemModel breadWord = Assert.Single(shoppingWords);

            Assert.Equal("Brot", breadWord.Lemma);
            Assert.Equal("bread", breadWord.PrimaryMeaning);
            Assert.Contains(workWords, word => word.Lemma == "Bewerbung" && word.PrimaryMeaning == "job application");
            Assert.Equal(2, c2Words.Count);
            Assert.Contains(c2Words, word => word.Lemma == "Unabdingbarkeit");
            Assert.Contains(c2Words, word => word.Lemma == "niederschmettern");
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    private static string CreateCourseSlicePackageJson(
        string packageId,
        bool includeSecondModule,
        string firstModuleTitle,
        string firstLessonTitle,
        string secondModuleTitle,
        string secondLessonTitle)
    {
        string secondModuleJson = includeSecondModule
            ? $$"""
                ,
                {
                  "slug": "a1-slice-module-two",
                  "coursePathSlug": "a1-slice-safe-course",
                  "title": "{{secondModuleTitle}}",
                  "titleTranslations": [{ "language": "en", "text": "Everyday life and appointments" }],
                  "description": "Zweites Modul, das bei einem module-slice Import erhalten bleiben muss.",
                  "descriptionTranslations": [{ "language": "en", "text": "Second module that must survive a module-slice import." }],
                  "moduleNumber": 2,
                  "cefrLevel": "A1",
                  "sortOrder": 20
                }
              """
            : string.Empty;
        string secondLessonJson = includeSecondModule
            ? $$"""
                ,
                {
                  "slug": "a1-slice-lesson-two",
                  "coursePathSlug": "a1-slice-safe-course",
                  "moduleSlug": "a1-slice-module-two",
                  "lessonNumber": 2,
                  "title": "{{secondLessonTitle}}",
                  "titleTranslations": [{ "language": "en", "text": "Name an appointment" }],
                  "shortDescription": "Du nennst eine einfache Uhrzeit.",
                  "shortDescriptionTranslations": [{ "language": "en", "text": "You name a simple time." }],
                  "narrative": "Diese zweite Lektion prueft, dass ein spaeterer module-slice Import sie nicht entfernt.",
                  "narrativeTranslations": [{ "language": "en", "text": "This second lesson proves that a later module-slice import does not remove it." }],
                  "cefrLevel": "A1",
                  "estimatedMinutes": 15,
                  "learningGoals": ["Eine Uhrzeit nennen"],
                  "learningGoalsTranslations": [{ "language": "en", "texts": ["Name a time"] }],
                  "reviewSummary": "Wiederhole die Uhrzeit laut.",
                  "reviewSummaryTranslations": [{ "language": "en", "text": "Repeat the time aloud." }],
                  "homeworkTask": "Schreibe zwei Uhrzeiten.",
                  "homeworkTaskTranslations": [{ "language": "en", "text": "Write two times." }],
                  "sortOrder": 20
                }
              """
            : string.Empty;

        return $$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "Course Module Slice Test",
              "source": "Automated test fixture",
              "defaultMeaningLanguages": ["en"],
              "entries": [],
              "coursePaths": [
                {
                  "slug": "a1-slice-safe-course",
                  "title": "A1 Slice Safe Course",
                  "titleTranslations": [{ "language": "en", "text": "A1 slice-safe course" }],
                  "description": "Ein kleiner Kurs fuer module-slice Importtests.",
                  "descriptionTranslations": [{ "language": "en", "text": "A small course for module-slice import tests." }],
                  "cefrLevel": "A1",
                  "sortOrder": 10
                }
              ],
              "courseModules": [
                {
                  "slug": "a1-slice-module-one",
                  "coursePathSlug": "a1-slice-safe-course",
                  "title": "{{firstModuleTitle}}",
                  "titleTranslations": [{ "language": "en", "text": "First contacts" }],
                  "description": "Erstes Modul, das durch den module-slice Import ersetzt werden soll.",
                  "descriptionTranslations": [{ "language": "en", "text": "First module that should be replaced by the module-slice import." }],
                  "moduleNumber": 1,
                  "cefrLevel": "A1",
                  "sortOrder": 10
                }{{secondModuleJson}}
              ],
              "courseLessons": [
                {
                  "slug": "a1-slice-lesson-one",
                  "coursePathSlug": "a1-slice-safe-course",
                  "moduleSlug": "a1-slice-module-one",
                  "lessonNumber": 1,
                  "title": "{{firstLessonTitle}}",
                  "titleTranslations": [{ "language": "en", "text": "Build a greeting clearly" }],
                  "shortDescription": "Du begruesst jemanden und sagst deinen Namen.",
                  "shortDescriptionTranslations": [{ "language": "en", "text": "You greet someone and say your name." }],
                  "narrative": "Diese Lektion wird durch den spaeteren module-slice Import aktualisiert.",
                  "narrativeTranslations": [{ "language": "en", "text": "This lesson is updated by the later module-slice import." }],
                  "cefrLevel": "A1",
                  "estimatedMinutes": 15,
                  "learningGoals": ["Jemanden begruessen"],
                  "learningGoalsTranslations": [{ "language": "en", "texts": ["Greet someone"] }],
                  "reviewSummary": "Wiederhole die Begruessung.",
                  "reviewSummaryTranslations": [{ "language": "en", "text": "Repeat the greeting." }],
                  "homeworkTask": "Schreibe eine kurze Begruessung.",
                  "homeworkTaskTranslations": [{ "language": "en", "text": "Write a short greeting." }],
                  "sortOrder": 10
                }{{secondLessonJson}}
              ]
            }
            """;
    }

    private static ServiceProvider BuildServiceProvider(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        ServiceCollection services = new();
        services
            .AddDarwinLinguaInfrastructure(options => options.DatabasePath = databasePath)
            .AddCatalogApplication()
            .AddCatalogInfrastructure()
            .AddContentOpsApplication()
            .AddContentOpsInfrastructure()
            .AddLocalizationApplication()
            .AddLocalizationInfrastructure();

        return services.BuildServiceProvider();
    }

    private static ServiceProvider BuildPostgresServiceProvider(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        ServiceCollection services = new();
        services
            .AddDarwinLinguaInfrastructureForPostgres(connectionString)
            .AddCatalogApplication()
            .AddCatalogInfrastructure()
            .AddContentOpsApplication()
            .AddContentOpsInfrastructure()
            .AddLocalizationApplication()
            .AddLocalizationInfrastructure();

        return services.BuildServiceProvider();
    }

    private static string BuildPostgresConnectionString(string databaseName)
    {
        string? configuredTemplate = Environment.GetEnvironmentVariable("DARWINLINGUA_TEST_POSTGRES_APP_CONNECTION_TEMPLATE");
        if (!string.IsNullOrWhiteSpace(configuredTemplate))
        {
            return string.Format(configuredTemplate, databaseName);
        }

        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = "localhost",
            Port = 5432,
            Database = databaseName,
            Username = "darwinlingua_app",
            Password = "@pP@sS!13;X"
        };

        return builder.ConnectionString;
    }

    private static async Task CreatePostgresDatabaseAsync(string databaseName, CancellationToken cancellationToken) =>
        await RunDockerPsqlAsync($"""CREATE DATABASE "{databaseName}" OWNER darwinlingua_app;""", cancellationToken);

    private static async Task DropPostgresDatabaseAsync(string databaseName, CancellationToken cancellationToken)
    {
        await RunDockerPsqlAsync(
            $"""
            SELECT pg_terminate_backend(pid)
            FROM pg_stat_activity
            WHERE datname = '{databaseName}'
              AND pid <> pg_backend_pid();
            """,
            cancellationToken);
        await RunDockerPsqlAsync($"""DROP DATABASE IF EXISTS "{databaseName}";""", cancellationToken);
    }

    private static async Task RunDockerPsqlAsync(string sql, CancellationToken cancellationToken)
    {
        string containerName = Environment.GetEnvironmentVariable("DARWINLINGUA_TEST_POSTGRES_CONTAINER") ?? DefaultDockerContainerName;
        ProcessStartInfo startInfo = new()
        {
            FileName = "docker",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        startInfo.ArgumentList.Add("exec");
        startInfo.ArgumentList.Add(containerName);
        startInfo.ArgumentList.Add("psql");
        startInfo.ArgumentList.Add("-U");
        startInfo.ArgumentList.Add("postgres");
        startInfo.ArgumentList.Add("-d");
        startInfo.ArgumentList.Add("postgres");
        startInfo.ArgumentList.Add("-v");
        startInfo.ArgumentList.Add("ON_ERROR_STOP=1");
        startInfo.ArgumentList.Add("-c");
        startInfo.ArgumentList.Add(sql);

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start Docker PostgreSQL helper process.");

        string standardOutput = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        string standardError = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"PostgreSQL test database command failed with exit code {process.ExitCode}.{Environment.NewLine}{standardOutput}{Environment.NewLine}{standardError}");
        }
    }

    private static string GetSamplePackagePath()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string samplePackagePath = Path.Combine(
            repositoryRoot,
            "tests/Modules/ContentOps/DarwinLingua.ContentOps.Infrastructure.Tests/Fixtures/phase1-sample-content-package.json");

        Assert.True(File.Exists(samplePackagePath), $"Sample package fixture was not found: {samplePackagePath}");
        return samplePackagePath;
    }

    private static string ResolveRepositoryRoot()
    {
        DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string candidateSolutionPath = Path.Combine(currentDirectory.FullName, "DarwinLingua.slnx");

            if (File.Exists(candidateSolutionPath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new InvalidOperationException("Unable to resolve repository root from test execution directory.");
    }

    private static string NormalizePackageJson(string json)
    {
        string[] requiredMeaningLanguages = ["ar", "ckb", "en", "fa", "kmr", "pl", "ro", "ru", "sq", "tr"];
        string[] requiredLocalizationLanguages = ["de", .. requiredMeaningLanguages];
        JsonObject document = JsonNode.Parse(json)!.AsObject();

        document["defaultMeaningLanguages"] = new JsonArray(requiredMeaningLanguages.Select(language => JsonValue.Create(language)).ToArray<JsonNode?>());

        HashSet<string> labelKeys = new(StringComparer.OrdinalIgnoreCase);
        foreach (JsonObject entry in ReadObjects(document["entries"]))
        {
            foreach (string key in ReadStrings(entry["usageLabels"]))
            {
                labelKeys.Add(key);
            }

            foreach (string key in ReadStrings(entry["contextLabels"]))
            {
                labelKeys.Add(key);
            }

            FillMeaningTranslations(entry["meanings"], requiredMeaningLanguages, "text");
            foreach (JsonObject example in ReadObjects(entry["examples"]))
            {
                FillMeaningTranslations(example["translations"], requiredMeaningLanguages, "text");
            }
        }

        if (labelKeys.Count == 0)
        {
            labelKeys.Add("general");
        }

        document["labels"] = new JsonArray(labelKeys
            .Order(StringComparer.OrdinalIgnoreCase)
            .Select((key, index) => CreateLabelDefinition(key, index, requiredLocalizationLanguages))
            .ToArray<JsonNode?>());

        foreach (JsonObject collection in ReadObjects(document["collections"]))
        {
            string name = collection["name"]?.GetValue<string>() ?? string.Empty;
            string? description = collection["description"]?.GetValue<string>();
            collection["localizations"] = new JsonArray(requiredLocalizationLanguages
                .Select(language => CreateLocalization(language, name, description))
                .ToArray<JsonNode?>());
        }

        foreach (JsonObject dialogue in ReadObjects(document["dialogues"]))
        {
            foreach (JsonObject turn in ReadObjects(dialogue["dialogueTurns"]))
            {
                FillMeaningTranslations(turn["translations"], requiredMeaningLanguages, "text");
            }

            foreach (JsonObject phrase in ReadObjects(dialogue["usefulPhrases"]))
            {
                FillMeaningTranslations(phrase["translations"], requiredMeaningLanguages, "text");
            }

            foreach (JsonObject question in ReadObjects(dialogue["questions"]))
            {
                FillMeaningTranslations(question["translations"], requiredMeaningLanguages, "text");
                foreach (JsonObject answer in ReadObjects(question["answers"]))
                {
                    FillMeaningTranslations(answer["translations"], requiredMeaningLanguages, "text");
                }
            }
        }

        foreach (JsonObject pack in ReadObjects(document["conversationStarterPacks"]))
        {
            foreach (JsonObject phrase in ReadObjects(pack["phrases"]))
            {
                FillMeaningTranslations(phrase["translations"], requiredMeaningLanguages, "text");
            }
        }

        return document.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    private static JsonObject CreateLabelDefinition(string key, int index, IReadOnlyList<string> requiredLocalizationLanguages)
    {
        string displayName = string.Join(' ', key.Split('-', StringSplitOptions.RemoveEmptyEntries).Select(part => string.Concat(char.ToUpperInvariant(part[0]), part[1..])));
        string kind = key is "formal" or "informal" or "written" ? "usage" : "context";

        return new JsonObject
        {
            ["kind"] = kind,
            ["key"] = key,
            ["displayName"] = displayName,
            ["localizations"] = new JsonArray(requiredLocalizationLanguages
                .Select(language => CreateLocalization(language, displayName, $"{displayName} label"))
                .ToArray<JsonNode?>()),
            ["sortOrder"] = (index + 1) * 10,
        };
    }

    private static JsonObject CreateLocalization(string language, string name, string? description) =>
        new()
        {
            ["language"] = language,
            ["name"] = name,
            ["description"] = description,
        };

    private static IEnumerable<JsonObject> ReadObjects(JsonNode? node) =>
        node is JsonArray array
            ? array.OfType<JsonObject>()
            : [];

    private static IEnumerable<string> ReadStrings(JsonNode? node) =>
        node is JsonArray array
            ? array.Select(item => item?.GetValue<string>()).Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!)
            : [];

    private static void FillMeaningTranslations(JsonNode? node, IReadOnlyList<string> requiredMeaningLanguages, string valuePropertyName)
    {
        if (node is not JsonArray translations || translations.Count == 0)
        {
            return;
        }

        Dictionary<string, string> existingTranslations = translations
            .OfType<JsonObject>()
            .Where(item => item["language"] is not null)
            .ToDictionary(
                item => item["language"]!.GetValue<string>().Trim().ToLowerInvariant(),
                item => item[valuePropertyName]?.GetValue<string>() ?? string.Empty,
                StringComparer.OrdinalIgnoreCase);

        if (!existingTranslations.TryGetValue("en", out string? fallback) || string.IsNullOrWhiteSpace(fallback))
        {
            fallback = existingTranslations.Values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        }

        if (string.IsNullOrWhiteSpace(fallback))
        {
            return;
        }

        translations.Clear();
        foreach (string language in requiredMeaningLanguages)
        {
            translations.Add(new JsonObject
            {
                ["language"] = language,
                [valuePropertyName] = existingTranslations.GetValueOrDefault(language, fallback),
            });
        }
    }

    private static string CreateValidPackageJson(string packageId)
    {
        return NormalizePackageJson($$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "A1 Shopping Import Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "entries": [
                {
                  "word": "Brot",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "das",
                  "plural": "Brote",
                  "topics": ["shopping"],
                  "usageLabels": ["informal"],
                  "contextLabels": ["shopping"],
                  "grammarNotes": ["Plural form is mostly used when talking about different bread types."],
                  "collocations": [
                    {
                      "text": "frisches Brot kaufen",
                      "meaning": "to buy fresh bread"
                    }
                  ],
                  "wordFamilies": [
                    {
                      "lemma": "Bäcker",
                      "relationLabel": "Profession",
                      "note": "person who bakes or sells bread"
                    }
                  ],
                  "relations": [
                    {
                      "kind": "synonym",
                      "lemma": "Laib",
                      "note": "used for a loaf of bread"
                    },
                    {
                      "kind": "antonym",
                      "lemma": "Fasten",
                      "note": "going without food"
                    }
                  ],
                  "meanings": [
                    {
                      "language": "en",
                      "text": "bread"
                    }
                  ],
                  "examples": [
                    {
                      "baseText": "Ich kaufe Brot.",
                      "translations": [
                        {
                          "language": "en",
                          "text": "I buy bread."
                        }
                      ]
                    }
                  ]
                }
              ]
            }
            """);
    }

    private static string CreatePackageWithInvalidEntryJson(string packageId)
    {
        return NormalizePackageJson($$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "A1 Shopping Import Mixed Validity Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "entries": [
                {
                  "word": "Milch",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "die",
                  "plural": "Milch",
                  "topics": ["shopping"],
                  "meanings": [
                    {
                      "language": "en",
                      "text": "milk"
                    }
                  ],
                  "examples": [
                    {
                      "baseText": "Ich brauche Milch.",
                      "translations": [
                        {
                          "language": "en",
                          "text": "I need milk."
                        }
                      ]
                    }
                  ]
                },
                {
                  "word": "Falscheintrag",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "topics": ["missing-topic"],
                  "meanings": [],
                  "examples": []
                }
              ]
            }
            """);
    }

    private static string CreatePackageWithMultipleLexicalFormsJson(string packageId)
    {
        return NormalizePackageJson($$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "A1 Shopping Multi Lexical Forms Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "entries": [
                {
                  "word": "Kasse",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "die",
                  "plural": "Kassen",
                  "pronunciationIpa": "/ˈkasə/",
                  "syllableBreak": "Kas-se",
                  "lexicalForms": [
                    {
                      "partOfSpeech": "Noun",
                      "article": "die",
                      "plural": "Kassen",
                      "isPrimary": true
                    },
                    {
                      "partOfSpeech": "Verb",
                      "infinitive": "Kasse machen"
                    }
                  ],
                  "topics": ["shopping"],
                  "meanings": [
                    {
                      "language": "en",
                      "text": "checkout"
                    }
                  ],
                  "examples": [
                    {
                      "baseText": "Ich gehe zur Kasse.",
                      "translations": [
                        {
                          "language": "en",
                          "text": "I am going to the checkout."
                        }
                      ]
                    }
                  ]
                }
              ]
            }
            """);
    }

    private static string CreatePackageWithCollectionWordKeysJson(string packageId)
    {
        return NormalizePackageJson($$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "A1 Shopping Collection Word Keys Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "entries": [
                {
                  "word": "Brot",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "das",
                  "plural": "Brote",
                  "topics": ["shopping"],
                  "meanings": [
                    {
                      "language": "en",
                      "text": "bread"
                    }
                  ],
                  "examples": [
                    {
                      "baseText": "Ich kaufe Brot.",
                      "translations": [
                        {
                          "language": "en",
                          "text": "I buy bread."
                        }
                      ]
                    }
                  ]
                },
                {
                  "word": "Milch",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "die",
                  "topics": ["shopping"],
                  "meanings": [
                    {
                      "language": "en",
                      "text": "milk"
                    }
                  ],
                  "examples": [
                    {
                      "baseText": "Ich trinke Milch.",
                      "translations": [
                        {
                          "language": "en",
                          "text": "I drink milk."
                        }
                      ]
                    }
                  ]
                }
              ],
              "collections": [
                {
                  "slug": "a1-shopping-word-keys",
                  "name": "A1 Shopping Word Keys",
                  "description": "Compact collection reference test.",
                  "image": "collections/a1-shopping-word-keys.png",
                  "wordKeys": ["Brot", "Milch"]
                }
              ]
            }
            """);
    }

    private static string CreateCollectionOnlyPackageJson(string packageId)
    {
        return NormalizePackageJson($$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "A1 Shopping Collection Only Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "labels": [],
              "entries": [],
              "collections": [
                {
                  "slug": "a1-shopping-collection-only",
                  "name": "A1 Shopping Collection Only",
                  "description": "Collection-only package reference test.",
                  "image": "collections/a1-shopping-collection-only.png",
                  "wordKeys": ["Brot", "Milch"]
                }
              ]
            }
            """);
    }

    private static string CreatePackageWithDialogueJson(string packageId)
    {
        return NormalizePackageJson($$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "A1 Dialogue Import Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "entries": [
                {
                  "word": "Termin",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "der",
                  "topics": ["appointments-and-health"],
                  "meanings": [
                    { "language": "en", "text": "appointment" }
                  ],
                  "examples": [
                    {
                      "baseText": "Ich brauche einen Termin.",
                      "translations": [
                        { "language": "en", "text": "I need an appointment." }
                      ]
                    }
                  ]
                }
              ],
              "dialogues": [
                {
                  "slug": "doctor-appointment-a1",
                  "title": "Doctor Appointment",
                  "description": "Prepare for a simple appointment conversation.",
                  "learnerGoal": "Ask for an appointment.",
                  "cefrLevel": "A1",
                  "category": "doctor-and-healthcare",
                  "topics": ["appointments-and-health"],
                  "examProfiles": ["goethe-a1", "telc-a1"],
                  "skillFocus": ["speaking", "phone-call", "exam-speaking"],
                  "taskType": "make-appointment",
                  "interactionMode": "phone",
                  "register": "formal",
                  "speakingFunctions": ["greet", "request", "confirm", "close-conversation"],
                  "estimatedPracticeMinutes": 10,
                  "difficultyNote": "Short A1 phone dialogue with polite appointment phrases.",
                  "examRelevance": "Useful for A1 appointment roleplay tasks.",
                  "sortOrder": 1,
                  "dialogueTurns": [
                    {
                      "speakerRole": "receptionist",
                      "baseText": "Guten Morgen, Praxis Müller.",
                      "translations": [
                        { "language": "en", "text": "Good morning, Müller practice." }
                      ]
                    },
                    {
                      "speakerRole": "learner",
                      "baseText": "Ich brauche einen Termin.",
                      "translations": [
                        { "language": "en", "text": "I need an appointment." }
                      ]
                    },
                    {
                      "speakerRole": "receptionist",
                      "baseText": "Haben Sie heute Schmerzen?",
                      "translations": [
                        { "language": "en", "text": "Do you have pain today?" }
                      ]
                    },
                    {
                      "speakerRole": "learner",
                      "baseText": "Ja, mein Kopf tut weh.",
                      "translations": [
                        { "language": "en", "text": "Yes, my head hurts." }
                      ]
                    },
                    {
                      "speakerRole": "receptionist",
                      "baseText": "Morgen um neun Uhr ist ein Termin frei.",
                      "translations": [
                        { "language": "en", "text": "Tomorrow at nine o'clock an appointment is available." }
                      ]
                    },
                    {
                      "speakerRole": "learner",
                      "baseText": "Morgen um neun Uhr passt gut.",
                      "translations": [
                        { "language": "en", "text": "Tomorrow at nine o'clock works well." }
                      ]
                    },
                    {
                      "speakerRole": "receptionist",
                      "baseText": "Wie ist Ihr Name?",
                      "translations": [
                        { "language": "en", "text": "What is your name?" }
                      ]
                    },
                    {
                      "speakerRole": "learner",
                      "baseText": "Mein Name ist Sara Ali.",
                      "translations": [
                        { "language": "en", "text": "My name is Sara Ali." }
                      ]
                    },
                    {
                      "speakerRole": "receptionist",
                      "baseText": "Bitte bringen Sie Ihre Karte mit.",
                      "translations": [
                        { "language": "en", "text": "Please bring your card with you." }
                      ]
                    },
                    {
                      "speakerRole": "learner",
                      "baseText": "Danke, auf Wiederhören.",
                      "translations": [
                        { "language": "en", "text": "Thank you, goodbye on the phone." }
                      ]
                    }
                  ],
                  "usefulWords": [
                    { "lemma": "der Termin", "wordSlug": "der-termin", "cefrLevel": "A1", "sortOrder": 10 },
                    { "lemma": "die Praxis", "wordSlug": "die-praxis", "cefrLevel": "A1", "sortOrder": 20 },
                    { "lemma": "der Schmerz", "wordSlug": "der-schmerz", "cefrLevel": "A1", "sortOrder": 30 },
                    { "lemma": "morgen", "wordSlug": "morgen", "cefrLevel": "A1", "sortOrder": 40 },
                    { "lemma": "passen", "wordSlug": "passen", "cefrLevel": "A1", "sortOrder": 50 },
                    { "lemma": "der Name", "wordSlug": "der-name", "cefrLevel": "A1", "sortOrder": 60 },
                    { "lemma": "die Karte", "wordSlug": "die-karte", "cefrLevel": "A1", "sortOrder": 70 },
                    { "lemma": "mitbringen", "wordSlug": "mitbringen", "cefrLevel": "A1", "sortOrder": 80 },
                    { "lemma": "danke", "wordSlug": "danke", "cefrLevel": "A1", "sortOrder": 90 },
                    { "lemma": "auf Wiederhören", "wordSlug": "auf-wiederhoeren", "cefrLevel": "A1", "sortOrder": 100 }
                  ],
                  "speakingPrompts": [
                    {
                      "promptType": "speaking-prompt",
                      "prompt": "Rufen Sie in einer Praxis an und fragen Sie nach einem Termin.",
                      "sortOrder": 10,
                      "translations": [
                        { "language": "ar", "text": "Call a practice and ask for an appointment." },
                        { "language": "ckb", "text": "Call a practice and ask for an appointment." },
                        { "language": "en", "text": "Call a practice and ask for an appointment." },
                        { "language": "fa", "text": "با یک مطب تماس بگیرید و درخواست وقت ملاقات کنید." },
                        { "language": "kmr", "text": "Call a practice and ask for an appointment." },
                        { "language": "pl", "text": "Call a practice and ask for an appointment." },
                        { "language": "ro", "text": "Call a practice and ask for an appointment." },
                        { "language": "ru", "text": "Call a practice and ask for an appointment." },
                        { "language": "sq", "text": "Call a practice and ask for an appointment." },
                        { "language": "tr", "text": "Call a practice and ask for an appointment." }
                      ]
                    },
                    {
                      "promptType": "roleplay-task",
                      "prompt": "Sagen Sie Ihren Namen und bestätigen Sie die Uhrzeit.",
                      "sortOrder": 20,
                      "translations": [
                        { "language": "ar", "text": "Say your name and confirm the time." },
                        { "language": "ckb", "text": "Say your name and confirm the time." },
                        { "language": "en", "text": "Say your name and confirm the time." },
                        { "language": "fa", "text": "نام خود را بگویید و ساعت را تأیید کنید." },
                        { "language": "kmr", "text": "Say your name and confirm the time." },
                        { "language": "pl", "text": "Say your name and confirm the time." },
                        { "language": "ro", "text": "Say your name and confirm the time." },
                        { "language": "ru", "text": "Say your name and confirm the time." },
                        { "language": "sq", "text": "Say your name and confirm the time." },
                        { "language": "tr", "text": "Say your name and confirm the time." }
                      ]
                    }
                  ],
                  "usefulPhrases": [
                    {
                      "baseText": "Können Sie das bitte wiederholen?",
                      "usageNote": "Use when you did not understand.",
                      "translations": [
                        { "language": "en", "text": "Could you please repeat that?" }
                      ]
                    }
                  ],
                  "questions": [
                    {
                      "prompt": "Was braucht die Person?",
                      "translations": [
                        { "language": "en", "text": "What does the person need?" }
                      ],
                      "answers": [
                        {
                          "text": "Einen Termin.",
                          "isCorrect": true,
                          "feedback": "Correct.",
                          "translations": [
                            { "language": "en", "text": "An appointment." }
                          ]
                        },
                        {
                          "text": "Ein Brot.",
                          "isCorrect": false,
                          "feedback": "This belongs to shopping.",
                          "translations": [
                            { "language": "en", "text": "A bread." }
                          ]
                        }
                      ]
                    }
                  ]
                }
              ]
            }
            """);
    }

    private static string CreatePackageWithConversationStarterJson(string packageId)
    {
        return NormalizePackageJson($$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "A1 Conversation Starter Import Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en", "fa"],
              "entries": [
                {
                  "word": "Name",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "der",
                  "topics": ["everyday-life"],
                  "meanings": [
                    { "language": "en", "text": "name" },
                    { "language": "fa", "text": "اسم" }
                  ],
                  "examples": [
                    {
                      "baseText": "Mein Name ist Sara.",
                      "translations": [
                        { "language": "en", "text": "My name is Sara." },
                        { "language": "fa", "text": "اسم من سارا است." }
                      ]
                    }
                  ]
                }
              ],
              "conversationStarterPacks": [
                {
                  "slug": "a1-cafe-first-meeting",
                  "title": "Cafe First Meeting",
                  "description": "Simple phrases for starting a friendly first conversation in a cafe.",
                  "cefrLevel": "A1",
                  "category": "first-meetings",
                  "situation": "cafe",
                  "tone": "friendly",
                  "conversationGoal": "introduction",
                  "topics": ["everyday-life"],
                  "sortOrder": 1,
                  "LinkedDialogueSlugs": ["doctor-appointment-a1"],
                  "linkedEventPreparationPackSlugs": ["a1-first-cafe-event"],
                  "phrases": [
                    {
                      "baseText": "Hallo, ich heisse Sara.",
                      "function": "opening",
                      "register": "neutral",
                      "usageNote": "Use this as a simple first introduction.",
                      "sortOrder": 1,
                      "alternativeBaseTexts": ["Hallo, ich heisse Sara."],
                      "commonMistake": "Do not omit ich in a full sentence.",
                      "translations": [
                        { "language": "en", "text": "Hello, my name is Sara." },
                        { "language": "fa", "text": "سلام، اسم من سارا است." }
                      ]
                    }
                  ]
                }
              ]
            }
            """);
    }

    private static string CreatePackageWithEventPreparationJson(string packageId)
    {
        return NormalizePackageJson($$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "A1 Event Preparation Import Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "entries": [
                {
                  "word": "Name",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "der",
                  "topics": ["everyday-life"],
                  "meanings": [
                    { "language": "en", "text": "name" }
                  ],
                  "examples": [
                    {
                      "baseText": "Mein Name ist Sara.",
                      "translations": [
                        { "language": "en", "text": "My name is Sara." }
                      ]
                    }
                  ]
                }
              ],
              "eventPreparationPacks": [
                {
                  "slug": "a1-first-cafe-event",
                  "title": "First Cafe Event",
                  "description": "Prepare for a short first conversation at a cafe event.",
                  "cefrLevel": "A1",
                  "category": "social-event",
                  "eventType": "conversation-cafe",
                  "topics": ["everyday-life"],
                  "sortOrder": 1,
                  "LinkedDialogueSlugs": ["cafe-first-meeting-a1"],
                  "linkedVocabulary": [
                    { "word": "Name", "partOfSpeech": "Noun", "cefrLevel": "A1" }
                  ],
                  "linkedConversationStarterPackSlugs": ["a1-cafe-first-meeting"],
                  "openingPrompts": ["Say your name and ask for the other person's name."],
                  "roleplayPrompts": ["Start a two-minute cafe introduction."],
                  "reviewPrompts": ["Write one phrase you want to reuse."]
                }
              ]
            }
            """);
    }

    private static string CreatePackageWithDuplicateEntriesJson(string packageId)
    {
        return NormalizePackageJson($$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "A1 Shopping Duplicate Entry Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "entries": [
                {
                  "word": "Apfel",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "der",
                  "plural": "Äpfel",
                  "topics": ["shopping"],
                  "meanings": [
                    {
                      "language": "en",
                      "text": "apple"
                    }
                  ],
                  "examples": [
                    {
                      "baseText": "Der Apfel ist frisch.",
                      "translations": [
                        {
                          "language": "en",
                          "text": "The apple is fresh."
                        }
                      ]
                    }
                  ]
                },
                {
                  "word": "Apfel",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "der",
                  "plural": "Äpfel",
                  "topics": ["shopping"],
                  "meanings": [
                    {
                      "language": "en",
                      "text": "apple"
                    }
                  ],
                  "examples": [
                    {
                      "baseText": "Ich esse einen Apfel.",
                      "translations": [
                        {
                          "language": "en",
                          "text": "I eat an apple."
                        }
                      ]
                    }
                  ]
                }
              ]
            }
            """);
    }
}




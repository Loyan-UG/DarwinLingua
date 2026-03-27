using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Domain.Entities;
using DarwinLingua.ContentOps.Domain.Enums;
using DarwinLingua.SharedKernel.Content;
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

        for (int entryIndex = 0; entryIndex < parsedPackage.Entries.Count; entryIndex++)
        {
            ParsedContentEntryModel entry = parsedPackage.Entries[entryIndex];
            await ProcessEntryAsync(
                entryIndex,
                entry,
                topicsByKey,
                meaningLanguages,
                contentPackage,
                importedWords,
                issues,
                cancellationToken).ConfigureAwait(false);
        }

        contentPackage.Complete(DateTime.UtcNow);

        await _contentImportRepository
            .PersistImportAsync(contentPackage, importedWords, cancellationToken)
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
            issues);
    }

    private async Task ProcessEntryAsync(
        int entryIndex,
        ParsedContentEntryModel entry,
        IReadOnlyDictionary<string, Topic> topicsByKey,
        IReadOnlySet<LanguageCode> meaningLanguages,
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
        string normalizedPartOfSpeechText = NormalizeText(entry.PartOfSpeech);
        string[] normalizedTopicKeys = entry.Topics
            .Where(topic => !string.IsNullOrWhiteSpace(topic))
            .Select(topic => NormalizeText(topic).ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        string[] usageLabels = ValidateLabelKeys(entry.UsageLabels, "usageLabels", entryErrors);
        string[] contextLabels = ValidateLabelKeys(entry.ContextLabels, "contextLabels", entryErrors);

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

        if (!Enum.TryParse(normalizedPartOfSpeechText, true, out PartOfSpeech partOfSpeech))
        {
            entryErrors.Add("Entry part of speech is invalid.");
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
                word.PartOfSpeech == partOfSpeech &&
                word.PrimaryCefrLevel == cefrLevel) ||
            await _contentImportRepository
                .WordExistsAsync(normalizedLemma, partOfSpeech, cefrLevel, cancellationToken)
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
            partOfSpeech,
            PublicationStatus.Active,
            contentPackage.SourceType,
            DateTime.UtcNow,
            article: NormalizeOptionalText(entry.Article),
            pluralForm: NormalizeOptionalText(entry.Plural));

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

    private static string NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
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
            issues);
    }
}

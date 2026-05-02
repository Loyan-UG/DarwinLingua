using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using DarwinLingua.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

internal sealed class WordAdminService(
    IDbContextFactory<DarwinLinguaDbContext> dbContextFactory,
    IWebsiteAdminQueryService adminQueryService) : IWordAdminService
{
    private const int MaxBulkImportWords = 500;

    public async Task<AdminCatalogWordDetailResponse> CreateAsync(
        AdminUpdateWordMetadataRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        ParsedWordMetadata parsed = ParseMetadata(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        DateTime now = DateTime.UtcNow;
        WordEntry word = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            request.Lemma,
            LanguageCode.From(request.LanguageCode),
            parsed.CefrLevel,
            parsed.PartOfSpeech,
            parsed.PublicationStatus,
            parsed.ContentSourceType,
            now,
            request.Article,
            request.PluralForm,
            request.InfinitiveForm,
            request.PronunciationIpa,
            request.SyllableBreak,
            request.SourceReference);

        dbContext.WordEntries.Add(word);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await adminQueryService.GetWordAsync(word.PublicId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The new word could not be loaded after creation.");
    }

    public async Task<AdminCatalogWordDetailResponse?> UpdateMetadataAsync(
        Guid publicId,
        AdminUpdateWordMetadataRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (publicId == Guid.Empty)
        {
            return null;
        }

        ParsedWordMetadata parsed = ParseMetadata(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        WordEntry? word = await dbContext.WordEntries
            .Include(entry => entry.LexicalForms)
            .SingleOrDefaultAsync(entry => entry.PublicId == publicId, cancellationToken)
            .ConfigureAwait(false);

        if (word is null)
        {
            return null;
        }

        word.UpdateCoreMetadata(
            request.Lemma,
            LanguageCode.From(request.LanguageCode),
            parsed.CefrLevel,
            parsed.PartOfSpeech,
            parsed.PublicationStatus,
            parsed.ContentSourceType,
            DateTime.UtcNow,
            request.Article,
            request.PluralForm,
            request.InfinitiveForm,
            request.PronunciationIpa,
            request.SyllableBreak,
            request.SourceReference);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await adminQueryService.GetWordAsync(publicId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminCatalogWordDetailResponse?> AddSenseAsync(
        Guid publicId,
        AdminAddWordSenseRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (publicId == Guid.Empty)
        {
            return null;
        }

        if (!Enum.TryParse(request.PublicationStatus, ignoreCase: true, out PublicationStatus publicationStatus) ||
            !Enum.IsDefined(publicationStatus))
        {
            throw new InvalidOperationException($"'{request.PublicationStatus}' is not a supported publication status.");
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        WordEntry? word = await dbContext.WordEntries
            .Include(entry => entry.Senses)
                .ThenInclude(sense => sense.Translations)
            .Include(entry => entry.Senses)
                .ThenInclude(sense => sense.Examples)
                    .ThenInclude(example => example.Translations)
            .SingleOrDefaultAsync(entry => entry.PublicId == publicId, cancellationToken)
            .ConfigureAwait(false);

        if (word is null)
        {
            return null;
        }

        DateTime now = DateTime.UtcNow;
        int senseOrder = word.Senses.Count == 0 ? 1 : word.Senses.Max(sense => sense.SenseOrder) + 1;
        WordSense sense = word.AddSense(
            Guid.NewGuid(),
            senseOrder,
            request.IsPrimarySense,
            publicationStatus,
            now,
            request.ShortDefinitionDe,
            request.ShortGloss);

        if (!string.IsNullOrWhiteSpace(request.TranslationLanguageCode) &&
            !string.IsNullOrWhiteSpace(request.TranslationText))
        {
            sense.AddTranslation(
                Guid.NewGuid(),
                LanguageCode.From(request.TranslationLanguageCode),
                request.TranslationText,
                request.IsPrimaryTranslation,
                now);
        }

        if (!string.IsNullOrWhiteSpace(request.ExampleGermanText))
        {
            ExampleSentence example = sense.AddExample(
                Guid.NewGuid(),
                1,
                request.ExampleGermanText,
                request.IsPrimaryExample,
                now);

            if (!string.IsNullOrWhiteSpace(request.ExampleTranslationLanguageCode) &&
                !string.IsNullOrWhiteSpace(request.ExampleTranslationText))
            {
                example.AddTranslation(
                    Guid.NewGuid(),
                    LanguageCode.From(request.ExampleTranslationLanguageCode),
                    request.ExampleTranslationText,
                    now);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await adminQueryService.GetWordAsync(publicId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminCatalogWordDetailResponse?> AddSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        AdminAddWordSenseTranslationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (publicId == Guid.Empty || senseId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        WordEntry? word = await dbContext.WordEntries
            .Include(entry => entry.Senses)
                .ThenInclude(sense => sense.Translations)
            .SingleOrDefaultAsync(entry => entry.PublicId == publicId, cancellationToken)
            .ConfigureAwait(false);

        WordSense? sense = word?.Senses.SingleOrDefault(item => item.Id == senseId);
        if (word is null || sense is null)
        {
            return null;
        }

        sense.AddTranslation(
            Guid.NewGuid(),
            LanguageCode.From(request.LanguageCode),
            request.TranslationText,
            request.IsPrimary,
            DateTime.UtcNow);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await adminQueryService.GetWordAsync(publicId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminCatalogWordDetailResponse?> AddSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        AdminAddWordSenseExampleRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (publicId == Guid.Empty || senseId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        WordEntry? word = await dbContext.WordEntries
            .Include(entry => entry.Senses)
                .ThenInclude(sense => sense.Examples)
                    .ThenInclude(example => example.Translations)
            .SingleOrDefaultAsync(entry => entry.PublicId == publicId, cancellationToken)
            .ConfigureAwait(false);

        WordSense? sense = word?.Senses.SingleOrDefault(item => item.Id == senseId);
        if (word is null || sense is null)
        {
            return null;
        }

        DateTime now = DateTime.UtcNow;
        int sentenceOrder = sense.Examples.Count == 0 ? 1 : sense.Examples.Max(example => example.SentenceOrder) + 1;
        ExampleSentence example = sense.AddExample(
            Guid.NewGuid(),
            sentenceOrder,
            request.GermanText,
            request.IsPrimaryExample,
            now);

        if (!string.IsNullOrWhiteSpace(request.TranslationLanguageCode) &&
            !string.IsNullOrWhiteSpace(request.TranslationText))
        {
            example.AddTranslation(
                Guid.NewGuid(),
                LanguageCode.From(request.TranslationLanguageCode),
                request.TranslationText,
                now);
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await adminQueryService.GetWordAsync(publicId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminCatalogWordDetailResponse?> UpdateSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid translationId,
        AdminUpdateWordSenseTranslationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (publicId == Guid.Empty || senseId == Guid.Empty || translationId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        WordEntry? word = await dbContext.WordEntries
            .Include(entry => entry.Senses)
                .ThenInclude(sense => sense.Translations)
            .SingleOrDefaultAsync(entry => entry.PublicId == publicId, cancellationToken)
            .ConfigureAwait(false);

        WordSense? sense = word?.Senses.SingleOrDefault(item => item.Id == senseId);
        if (word is null || sense is null)
        {
            return null;
        }

        bool updated = sense.UpdateTranslation(
            translationId,
            LanguageCode.From(request.LanguageCode),
            request.TranslationText,
            request.IsPrimary,
            DateTime.UtcNow);

        if (!updated)
        {
            return null;
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await adminQueryService.GetWordAsync(publicId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminCatalogWordDetailResponse?> DeleteSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid translationId,
        CancellationToken cancellationToken)
    {
        if (publicId == Guid.Empty || senseId == Guid.Empty || translationId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        WordEntry? word = await dbContext.WordEntries
            .Include(entry => entry.Senses)
                .ThenInclude(sense => sense.Translations)
            .SingleOrDefaultAsync(entry => entry.PublicId == publicId, cancellationToken)
            .ConfigureAwait(false);

        WordSense? sense = word?.Senses.SingleOrDefault(item => item.Id == senseId);
        if (word is null || sense is null)
        {
            return null;
        }

        SenseTranslation? removed = sense.RemoveTranslation(translationId, DateTime.UtcNow);
        if (removed is null)
        {
            return null;
        }

        dbContext.SenseTranslations.Remove(removed);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await adminQueryService.GetWordAsync(publicId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminCatalogWordDetailResponse?> UpdateSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        AdminUpdateWordSenseExampleRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (publicId == Guid.Empty || senseId == Guid.Empty || exampleId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        WordEntry? word = await dbContext.WordEntries
            .Include(entry => entry.Senses)
                .ThenInclude(sense => sense.Examples)
            .SingleOrDefaultAsync(entry => entry.PublicId == publicId, cancellationToken)
            .ConfigureAwait(false);

        WordSense? sense = word?.Senses.SingleOrDefault(item => item.Id == senseId);
        if (word is null || sense is null)
        {
            return null;
        }

        bool updated = sense.UpdateExample(exampleId, request.GermanText, request.IsPrimaryExample, DateTime.UtcNow);
        if (!updated)
        {
            return null;
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await adminQueryService.GetWordAsync(publicId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminCatalogWordDetailResponse?> DeleteSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        CancellationToken cancellationToken)
    {
        if (publicId == Guid.Empty || senseId == Guid.Empty || exampleId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        WordEntry? word = await dbContext.WordEntries
            .Include(entry => entry.Senses)
                .ThenInclude(sense => sense.Examples)
                    .ThenInclude(example => example.Translations)
            .SingleOrDefaultAsync(entry => entry.PublicId == publicId, cancellationToken)
            .ConfigureAwait(false);

        WordSense? sense = word?.Senses.SingleOrDefault(item => item.Id == senseId);
        if (word is null || sense is null)
        {
            return null;
        }

        ExampleSentence? removed = sense.RemoveExample(exampleId, DateTime.UtcNow);
        if (removed is null)
        {
            return null;
        }

        dbContext.ExampleSentences.Remove(removed);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await adminQueryService.GetWordAsync(publicId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminCatalogWordDetailResponse?> AddSenseExampleTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        AdminAddWordSenseExampleTranslationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (publicId == Guid.Empty || senseId == Guid.Empty || exampleId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        WordEntry? word = await dbContext.WordEntries
            .Include(entry => entry.Senses)
                .ThenInclude(sense => sense.Examples)
                    .ThenInclude(example => example.Translations)
            .SingleOrDefaultAsync(entry => entry.PublicId == publicId, cancellationToken)
            .ConfigureAwait(false);

        WordSense? sense = word?.Senses.SingleOrDefault(item => item.Id == senseId);
        ExampleSentence? example = sense?.Examples.SingleOrDefault(item => item.Id == exampleId);
        if (word is null || sense is null || example is null)
        {
            return null;
        }

        example.AddTranslation(
            Guid.NewGuid(),
            LanguageCode.From(request.LanguageCode),
            request.TranslationText,
            DateTime.UtcNow);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await adminQueryService.GetWordAsync(publicId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminCatalogWordDetailResponse?> UpdateSenseExampleTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        Guid translationId,
        AdminUpdateWordSenseExampleTranslationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (publicId == Guid.Empty || senseId == Guid.Empty || exampleId == Guid.Empty || translationId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        WordEntry? word = await dbContext.WordEntries
            .Include(entry => entry.Senses)
                .ThenInclude(sense => sense.Examples)
                    .ThenInclude(example => example.Translations)
            .SingleOrDefaultAsync(entry => entry.PublicId == publicId, cancellationToken)
            .ConfigureAwait(false);

        WordSense? sense = word?.Senses.SingleOrDefault(item => item.Id == senseId);
        ExampleSentence? example = sense?.Examples.SingleOrDefault(item => item.Id == exampleId);
        if (word is null || sense is null || example is null)
        {
            return null;
        }

        bool updated = example.UpdateTranslation(
            translationId,
            LanguageCode.From(request.LanguageCode),
            request.TranslationText,
            DateTime.UtcNow);

        if (!updated)
        {
            return null;
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await adminQueryService.GetWordAsync(publicId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminCatalogWordDetailResponse?> DeleteSenseExampleTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        Guid translationId,
        CancellationToken cancellationToken)
    {
        if (publicId == Guid.Empty || senseId == Guid.Empty || exampleId == Guid.Empty || translationId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        WordEntry? word = await dbContext.WordEntries
            .Include(entry => entry.Senses)
                .ThenInclude(sense => sense.Examples)
                    .ThenInclude(example => example.Translations)
            .SingleOrDefaultAsync(entry => entry.PublicId == publicId, cancellationToken)
            .ConfigureAwait(false);

        WordSense? sense = word?.Senses.SingleOrDefault(item => item.Id == senseId);
        ExampleSentence? example = sense?.Examples.SingleOrDefault(item => item.Id == exampleId);
        if (word is null || sense is null || example is null)
        {
            return null;
        }

        ExampleTranslation? removed = example.RemoveTranslation(translationId, DateTime.UtcNow);
        if (removed is null)
        {
            return null;
        }

        dbContext.ExampleTranslations.Remove(removed);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await adminQueryService.GetWordAsync(publicId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminCatalogWordDetailResponse?> AddTopicAsync(
        Guid publicId,
        AdminAddWordTopicRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (publicId == Guid.Empty || request.TopicId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        bool topicExists = await dbContext.Topics
            .AnyAsync(topic => topic.Id == request.TopicId, cancellationToken)
            .ConfigureAwait(false);

        if (!topicExists)
        {
            return null;
        }

        WordEntry? word = await dbContext.WordEntries
            .Include(entry => entry.Topics)
            .SingleOrDefaultAsync(entry => entry.PublicId == publicId, cancellationToken)
            .ConfigureAwait(false);

        if (word is null)
        {
            return null;
        }

        if (!word.Topics.Any(topic => topic.TopicId == request.TopicId))
        {
            word.AddTopic(Guid.NewGuid(), request.TopicId, request.IsPrimaryTopic, DateTime.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return await adminQueryService.GetWordAsync(publicId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminCatalogWordDetailResponse?> DeleteTopicAsync(
        Guid publicId,
        Guid topicId,
        CancellationToken cancellationToken)
    {
        if (publicId == Guid.Empty || topicId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        WordEntry? word = await dbContext.WordEntries
            .AsNoTracking()
            .SingleOrDefaultAsync(entry => entry.PublicId == publicId, cancellationToken)
            .ConfigureAwait(false);

        if (word is null)
        {
            return null;
        }

        WordTopic? topicLink = await dbContext.WordTopics
            .SingleOrDefaultAsync(topic => topic.WordEntryId == word.Id && topic.TopicId == topicId, cancellationToken)
            .ConfigureAwait(false);

        if (topicLink is not null)
        {
            dbContext.WordTopics.Remove(topicLink);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return await adminQueryService.GetWordAsync(publicId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminCatalogWordDetailResponse?> AddLabelAsync(
        Guid publicId,
        AdminAddWordLabelRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (publicId == Guid.Empty)
        {
            return null;
        }

        if (!Enum.TryParse(request.Kind, ignoreCase: true, out WordLabelKind kind) || !Enum.IsDefined(kind))
        {
            throw new InvalidOperationException($"'{request.Kind}' is not a supported word-label kind.");
        }

        if (string.IsNullOrWhiteSpace(request.Key))
        {
            throw new InvalidOperationException("Word label key is required.");
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        string normalizedKey = request.Key.Trim().ToLowerInvariant();
        bool labelExists = await dbContext.LabelDefinitions
            .AsNoTracking()
            .AnyAsync(label => label.Kind == kind && label.Key == normalizedKey, cancellationToken)
            .ConfigureAwait(false);

        if (!labelExists)
        {
            throw new InvalidOperationException("Create this label in the label taxonomy before attaching it to a word.");
        }

        WordEntry? word = await dbContext.WordEntries
            .Include(entry => entry.Labels)
            .SingleOrDefaultAsync(entry => entry.PublicId == publicId, cancellationToken)
            .ConfigureAwait(false);

        if (word is null)
        {
            return null;
        }

        if (!word.Labels.Any(label =>
                label.Kind == kind &&
                string.Equals(label.Key, normalizedKey, StringComparison.Ordinal)))
        {
            word.AddLabel(Guid.NewGuid(), kind, normalizedKey, DateTime.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return await adminQueryService.GetWordAsync(publicId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminCatalogWordDetailResponse?> DeleteLabelAsync(
        Guid publicId,
        string kind,
        string key,
        CancellationToken cancellationToken)
    {
        if (publicId == Guid.Empty || string.IsNullOrWhiteSpace(kind) || string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        if (!Enum.TryParse(kind, ignoreCase: true, out WordLabelKind labelKind) || !Enum.IsDefined(labelKind))
        {
            throw new InvalidOperationException($"'{kind}' is not a supported word-label kind.");
        }

        string normalizedKey = key.Trim().ToLowerInvariant();

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        WordEntry? word = await dbContext.WordEntries
            .AsNoTracking()
            .SingleOrDefaultAsync(entry => entry.PublicId == publicId, cancellationToken)
            .ConfigureAwait(false);

        if (word is null)
        {
            return null;
        }

        WordLabel? label = await dbContext.WordLabels
            .SingleOrDefaultAsync(item =>
                item.WordEntryId == word.Id &&
                item.Kind == labelKind &&
                item.Key == normalizedKey,
                cancellationToken)
            .ConfigureAwait(false);

        if (label is not null)
        {
            dbContext.WordLabels.Remove(label);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return await adminQueryService.GetWordAsync(publicId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminBulkWordImportResponse> ImportWordsAsync(
        AdminBulkWordImportRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Words.Count > MaxBulkImportWords)
        {
            throw new InvalidOperationException($"A single import can contain at most {MaxBulkImportWords} words.");
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        List<AdminBulkWordImportItemResult> results = [];
        HashSet<string> importKeys = new(StringComparer.OrdinalIgnoreCase);
        DateTime now = DateTime.UtcNow;

        for (int index = 0; index < request.Words.Count; index++)
        {
            AdminBulkWordImportItemRequest item = request.Words[index];
            int rowNumber = index + 1;

            try
            {
                if (string.IsNullOrWhiteSpace(item.Lemma))
                {
                    results.Add(new(rowNumber, item.Lemma, null, "Failed", "Lemma is required."));
                    continue;
                }

                ParsedWordMetadata parsed = ParseMetadata(new AdminUpdateWordMetadataRequest(
                    item.Lemma,
                    DefaultIfBlank(item.LanguageCode, "de"),
                    item.Article,
                    item.PluralForm,
                    item.InfinitiveForm,
                    item.PronunciationIpa,
                    item.SyllableBreak,
                    DefaultIfBlank(item.PartOfSpeech, "Other"),
                    DefaultIfBlank(item.CefrLevel, "A1"),
                    DefaultIfBlank(item.PublicationStatus, "Draft"),
                    DefaultIfBlank(item.ContentSourceType, "Manual"),
                    item.SourceReference));

                ValidateCompleteWordImport(item);

                string normalizedLemma = NormalizeLemmaForLookup(item.Lemma);
                string importKey = $"{normalizedLemma}|{parsed.PartOfSpeech}|{parsed.CefrLevel}";
                if (!importKeys.Add(importKey))
                {
                    results.Add(new(rowNumber, item.Lemma, null, "Skipped", "A previous row in this file has the same lemma, part of speech, and CEFR level."));
                    continue;
                }

                bool exists = await dbContext.WordEntries.AnyAsync(
                        word => word.NormalizedLemma == normalizedLemma &&
                            word.PartOfSpeech == parsed.PartOfSpeech &&
                            word.PrimaryCefrLevel == parsed.CefrLevel,
                        cancellationToken)
                    .ConfigureAwait(false);

                if (exists)
                {
                    results.Add(new(rowNumber, item.Lemma, null, "Skipped", "A matching word already exists."));
                    continue;
                }

                WordEntry word = new(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    item.Lemma,
                    LanguageCode.From(DefaultIfBlank(item.LanguageCode, "de")),
                    parsed.CefrLevel,
                    parsed.PartOfSpeech,
                    parsed.PublicationStatus,
                    parsed.ContentSourceType,
                    now,
                    item.Article,
                    item.PluralForm,
                    item.InfinitiveForm,
                    item.PronunciationIpa,
                    item.SyllableBreak,
                    item.SourceReference);

                AddImportedSenses(word, item.Senses, now);
                dbContext.WordEntries.Add(word);
                results.Add(new(rowNumber, item.Lemma, word.PublicId, "Imported", "Word was imported."));
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or DarwinLingua.SharedKernel.Exceptions.DomainRuleException)
            {
                results.Add(new(rowNumber, item.Lemma, null, "Failed", ex.Message));
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AdminBulkWordImportResponse(
            request.Words.Count,
            results.Count(item => item.Status == "Imported"),
            results.Count(item => item.Status == "Skipped"),
            results.Count(item => item.Status == "Failed"),
            results);
    }

    private static ParsedWordMetadata ParseMetadata(AdminUpdateWordMetadataRequest request)
    {
        if (!Enum.TryParse(request.CefrLevel, ignoreCase: true, out CefrLevel cefrLevel) ||
            !Enum.IsDefined(cefrLevel))
        {
            throw new InvalidOperationException($"'{request.CefrLevel}' is not a supported CEFR level.");
        }

        if (!Enum.TryParse(request.PartOfSpeech, ignoreCase: true, out PartOfSpeech partOfSpeech) ||
            !Enum.IsDefined(partOfSpeech))
        {
            throw new InvalidOperationException($"'{request.PartOfSpeech}' is not a supported part of speech.");
        }

        if (!Enum.TryParse(request.PublicationStatus, ignoreCase: true, out PublicationStatus publicationStatus) ||
            !Enum.IsDefined(publicationStatus))
        {
            throw new InvalidOperationException($"'{request.PublicationStatus}' is not a supported publication status.");
        }

        if (!Enum.TryParse(request.ContentSourceType, ignoreCase: true, out ContentSourceType contentSourceType) ||
            !Enum.IsDefined(contentSourceType))
        {
            throw new InvalidOperationException($"'{request.ContentSourceType}' is not a supported content source type.");
        }

        return new ParsedWordMetadata(cefrLevel, partOfSpeech, publicationStatus, contentSourceType);
    }

    private sealed record ParsedWordMetadata(
        CefrLevel CefrLevel,
        PartOfSpeech PartOfSpeech,
        PublicationStatus PublicationStatus,
        ContentSourceType ContentSourceType);

    private static void AddImportedSenses(
        WordEntry word,
        IReadOnlyList<AdminBulkWordSenseImportRequest>? senses,
        DateTime now)
    {
        if (senses is null)
        {
            return;
        }

        for (int index = 0; index < senses.Count; index++)
        {
            AdminBulkWordSenseImportRequest item = senses[index];
            if (!Enum.TryParse(DefaultIfBlank(item.PublicationStatus, "Draft"), ignoreCase: true, out PublicationStatus publicationStatus) ||
                !Enum.IsDefined(publicationStatus))
            {
                throw new InvalidOperationException($"'{item.PublicationStatus}' is not a supported sense publication status.");
            }

            WordSense sense = word.AddSense(
                Guid.NewGuid(),
                index + 1,
                item.IsPrimarySense,
                publicationStatus,
                now,
                item.ShortDefinitionDe,
                item.ShortGloss);

            foreach (AdminBulkWordTranslationImportRequest translation in item.Translations ?? [])
            {
                sense.AddTranslation(
                    Guid.NewGuid(),
                    LanguageCode.From(translation.LanguageCode),
                    translation.TranslationText,
                    translation.IsPrimary,
                    now);
            }

            if (item.Examples is null)
            {
                continue;
            }

            for (int exampleIndex = 0; exampleIndex < item.Examples.Count; exampleIndex++)
            {
                AdminBulkWordExampleImportRequest exampleItem = item.Examples[exampleIndex];
                ExampleSentence example = sense.AddExample(
                    Guid.NewGuid(),
                    exampleIndex + 1,
                    exampleItem.GermanText,
                    exampleItem.IsPrimaryExample,
                    now);

                foreach (AdminBulkWordTranslationImportRequest translation in exampleItem.Translations ?? [])
                {
                    example.AddTranslation(
                        Guid.NewGuid(),
                        LanguageCode.From(translation.LanguageCode),
                        translation.TranslationText,
                        now);
                }
            }
        }
    }

    private static void ValidateCompleteWordImport(AdminBulkWordImportItemRequest item)
    {
        if (item.Senses is null || item.Senses.Count == 0)
        {
            throw new InvalidOperationException("Each imported word must include at least one sense.");
        }

        if (!item.Senses.Any(sense => sense.IsPrimarySense))
        {
            throw new InvalidOperationException("Each imported word must include one primary sense.");
        }

        for (int senseIndex = 0; senseIndex < item.Senses.Count; senseIndex++)
        {
            AdminBulkWordSenseImportRequest sense = item.Senses[senseIndex];
            string senseLabel = $"Sense {senseIndex + 1}";

            ValidateCompleteTranslations(sense.Translations, $"{senseLabel} translations");

            if (string.IsNullOrWhiteSpace(sense.ShortDefinitionDe))
            {
                throw new InvalidOperationException($"{senseLabel} must include a German short definition.");
            }

            if (sense.Examples is null || sense.Examples.Count == 0)
            {
                throw new InvalidOperationException($"{senseLabel} must include at least one German example sentence.");
            }

            for (int exampleIndex = 0; exampleIndex < sense.Examples.Count; exampleIndex++)
            {
                AdminBulkWordExampleImportRequest example = sense.Examples[exampleIndex];
                if (string.IsNullOrWhiteSpace(example.GermanText))
                {
                    throw new InvalidOperationException($"{senseLabel} example {exampleIndex + 1} must include German text.");
                }

                ValidateCompleteTranslations(
                    example.Translations,
                    $"{senseLabel} example {exampleIndex + 1} translations");
            }
        }
    }

    private static void ValidateCompleteTranslations(
        IReadOnlyList<AdminBulkWordTranslationImportRequest>? translations,
        string label)
    {
        if (translations is null || translations.Count == 0)
        {
            throw new InvalidOperationException(
                $"{label} are required for every meaning language: {ContentLanguageRequirements.FormatRequiredMeaningLanguages()}.");
        }

        IReadOnlyList<string> missing = ContentLanguageRequirements.FindMissingMeaningLanguages(
            translations.Select(translation => translation.LanguageCode));
        if (missing.Count > 0)
        {
            throw new InvalidOperationException($"{label} are missing languages: {string.Join(", ", missing)}.");
        }

        string[] duplicates = translations
            .Where(translation => !string.IsNullOrWhiteSpace(translation.LanguageCode))
            .GroupBy(translation => translation.LanguageCode.Trim().ToLowerInvariant())
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        if (duplicates.Length > 0)
        {
            throw new InvalidOperationException($"{label} contain duplicate languages: {string.Join(", ", duplicates)}.");
        }
    }

    private static string DefaultIfBlank(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string NormalizeLemmaForLookup(string lemma) =>
        string.Join(' ', lemma.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();
}

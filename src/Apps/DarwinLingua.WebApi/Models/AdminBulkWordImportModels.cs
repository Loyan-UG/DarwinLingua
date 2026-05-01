namespace DarwinLingua.WebApi.Models;

public sealed record AdminBulkWordImportRequest(
    IReadOnlyList<AdminBulkWordImportItemRequest> Words);

public sealed record AdminBulkWordImportItemRequest(
    string Lemma,
    string? LanguageCode,
    string? Article,
    string? PluralForm,
    string? InfinitiveForm,
    string? PronunciationIpa,
    string? SyllableBreak,
    string? PartOfSpeech,
    string? CefrLevel,
    string? PublicationStatus,
    string? ContentSourceType,
    string? SourceReference,
    IReadOnlyList<AdminBulkWordSenseImportRequest>? Senses);

public sealed record AdminBulkWordSenseImportRequest(
    string? PublicationStatus,
    bool IsPrimarySense,
    string? ShortDefinitionDe,
    string? ShortGloss,
    IReadOnlyList<AdminBulkWordTranslationImportRequest>? Translations,
    IReadOnlyList<AdminBulkWordExampleImportRequest>? Examples);

public sealed record AdminBulkWordTranslationImportRequest(
    string LanguageCode,
    string TranslationText,
    bool IsPrimary);

public sealed record AdminBulkWordExampleImportRequest(
    string GermanText,
    bool IsPrimaryExample,
    IReadOnlyList<AdminBulkWordTranslationImportRequest>? Translations);

public sealed record AdminBulkWordImportResponse(
    int TotalCount,
    int ImportedCount,
    int SkippedCount,
    int FailedCount,
    IReadOnlyList<AdminBulkWordImportItemResult> Items);

public sealed record AdminBulkWordImportItemResult(
    int RowNumber,
    string? Lemma,
    Guid? PublicId,
    string Status,
    string Message);

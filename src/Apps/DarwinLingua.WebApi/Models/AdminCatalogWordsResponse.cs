namespace DarwinLingua.WebApi.Models;

public sealed record AdminCatalogWordsResponse(
    string? Query,
    string? StatusFilter,
    string Sort,
    int Skip,
    int Take,
    int TotalCount,
    IReadOnlyList<AdminCatalogWordItemResponse> Words);

public sealed record AdminCatalogWordItemResponse(
    Guid PublicId,
    string Lemma,
    string? Article,
    string PartOfSpeech,
    string CefrLevel,
    string PublicationStatus,
    string ContentSourceType,
    int SenseCount,
    int TopicCount,
    DateTime UpdatedAtUtc);

public sealed record AdminCatalogWordDetailResponse(
    Guid PublicId,
    string Lemma,
    string NormalizedLemma,
    string LanguageCode,
    string? Article,
    string? PluralForm,
    string? InfinitiveForm,
    string? PronunciationIpa,
    string? SyllableBreak,
    string PartOfSpeech,
    string CefrLevel,
    string PublicationStatus,
    string ContentSourceType,
    string? SourceReference,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<AdminCatalogWordLexicalFormResponse> LexicalForms,
    IReadOnlyList<AdminCatalogWordSenseResponse> Senses,
    IReadOnlyList<AdminCatalogWordTopicResponse> Topics,
    IReadOnlyList<AdminCatalogWordLabelResponse> Labels,
    IReadOnlyList<AdminCatalogWordTextItemResponse> GrammarNotes,
    IReadOnlyList<AdminCatalogWordCollocationResponse> Collocations);

public sealed record AdminCatalogWordLexicalFormResponse(
    string PartOfSpeech,
    string? Article,
    string? PluralForm,
    string? InfinitiveForm,
    bool IsPrimary,
    int SortOrder);

public sealed record AdminCatalogWordSenseResponse(
    Guid SenseId,
    int SenseOrder,
    bool IsPrimarySense,
    string PublicationStatus,
    string? ShortDefinitionDe,
    string? ShortGloss,
    IReadOnlyList<AdminCatalogWordTranslationResponse> Translations,
    IReadOnlyList<AdminCatalogWordExampleResponse> Examples);

public sealed record AdminCatalogWordTranslationResponse(
    Guid TranslationId,
    string LanguageCode,
    string TranslationText,
    bool IsPrimary);

public sealed record AdminCatalogWordExampleResponse(
    Guid ExampleId,
    int SentenceOrder,
    string GermanText,
    bool IsPrimaryExample,
    IReadOnlyList<AdminCatalogWordExampleTranslationResponse> Translations);

public sealed record AdminCatalogWordExampleTranslationResponse(
    string LanguageCode,
    string TranslationText);

public sealed record AdminCatalogWordTopicResponse(
    Guid TopicId,
    string Key,
    bool IsPrimaryTopic);

public sealed record AdminCatalogWordLabelResponse(
    string Kind,
    string Key,
    string DisplayName,
    int SortOrder);

public sealed record AdminCatalogWordTextItemResponse(
    string Text,
    int SortOrder);

public sealed record AdminCatalogWordCollocationResponse(
    string Text,
    string? Meaning,
    int SortOrder);

public sealed record AdminAddWordTopicRequest(
    Guid TopicId,
    bool IsPrimaryTopic);

public sealed record AdminAddWordLabelRequest(
    string Kind,
    string Key);

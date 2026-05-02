namespace DarwinLingua.Web.Models;

public sealed record AdminWordsPageViewModel(
    string? Query,
    string? StatusFilter,
    string Sort,
    int Skip,
    int Take,
    int TotalCount,
    IReadOnlyList<AdminWordListItemViewModel> Words)
{
    public int PageNumber => Take <= 0 ? 1 : (Skip / Take) + 1;

    public int TotalPages => TotalCount <= 0 || Take <= 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)Take);

    public bool HasPreviousPage => Skip > 0;

    public bool HasNextPage => Skip + Take < TotalCount;

    public int PreviousSkip => Math.Max(0, Skip - Take);

    public int NextSkip => Skip + Take;
}

public sealed record AdminWordListItemViewModel(
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

public sealed record AdminWordDetailViewModel(
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
    IReadOnlyList<AdminWordLexicalFormViewModel> LexicalForms,
    IReadOnlyList<AdminWordSenseViewModel> Senses,
    IReadOnlyList<AdminWordTopicViewModel> Topics,
    IReadOnlyList<AdminWordLabelViewModel> Labels,
    IReadOnlyList<AdminWordTextItemViewModel> GrammarNotes,
    IReadOnlyList<AdminWordCollocationViewModel> Collocations);

public sealed record AdminWordLexicalFormViewModel(
    string PartOfSpeech,
    string? Article,
    string? PluralForm,
    string? InfinitiveForm,
    bool IsPrimary,
    int SortOrder);

public sealed record AdminWordSenseViewModel(
    Guid SenseId,
    int SenseOrder,
    bool IsPrimarySense,
    string PublicationStatus,
    string? ShortDefinitionDe,
    string? ShortGloss,
    IReadOnlyList<AdminWordTranslationViewModel> Translations,
    IReadOnlyList<AdminWordExampleViewModel> Examples);

public sealed record AdminWordTranslationViewModel(
    Guid TranslationId,
    string LanguageCode,
    string TranslationText,
    bool IsPrimary);

public sealed record AdminWordExampleViewModel(
    Guid ExampleId,
    int SentenceOrder,
    string GermanText,
    bool IsPrimaryExample,
    IReadOnlyList<AdminWordExampleTranslationViewModel> Translations);

public sealed record AdminWordExampleTranslationViewModel(
    Guid TranslationId,
    string LanguageCode,
    string TranslationText);

public sealed record AdminWordTopicViewModel(
    Guid TopicId,
    string Key,
    bool IsPrimaryTopic);

public sealed record AdminWordLabelViewModel(
    string Kind,
    string Key,
    string DisplayName,
    int SortOrder);

public sealed record AdminWordTextItemViewModel(
    string Text,
    int SortOrder);

public sealed record AdminWordCollocationViewModel(
    string Text,
    string? Meaning,
    int SortOrder);

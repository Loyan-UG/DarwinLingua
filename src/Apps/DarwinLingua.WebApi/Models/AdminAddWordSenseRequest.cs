namespace DarwinLingua.WebApi.Models;

public sealed record AdminAddWordSenseRequest(
    string PublicationStatus,
    bool IsPrimarySense,
    string? ShortDefinitionDe,
    string? ShortGloss,
    string? TranslationLanguageCode,
    string? TranslationText,
    bool IsPrimaryTranslation,
    string? ExampleGermanText,
    string? ExampleTranslationLanguageCode,
    string? ExampleTranslationText,
    bool IsPrimaryExample);

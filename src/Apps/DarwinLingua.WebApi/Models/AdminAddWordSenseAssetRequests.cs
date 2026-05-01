namespace DarwinLingua.WebApi.Models;

public sealed record AdminAddWordSenseTranslationRequest(
    string LanguageCode,
    string TranslationText,
    bool IsPrimary);

public sealed record AdminAddWordSenseExampleRequest(
    string GermanText,
    bool IsPrimaryExample,
    string? TranslationLanguageCode,
    string? TranslationText);

public sealed record AdminUpdateWordSenseTranslationRequest(
    string LanguageCode,
    string TranslationText,
    bool IsPrimary);

public sealed record AdminUpdateWordSenseExampleRequest(
    string GermanText,
    bool IsPrimaryExample);

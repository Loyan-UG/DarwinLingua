using System.ComponentModel.DataAnnotations;

namespace DarwinLingua.Web.Models;

public sealed class AdminWordSenseTranslationFormModel
{
    public Guid SenseId { get; set; }

    [Required]
    [StringLength(16)]
    public string LanguageCode { get; set; } = "en";

    [Required]
    [StringLength(512)]
    public string TranslationText { get; set; } = string.Empty;

    public bool IsPrimary { get; set; } = true;
}

public sealed class AdminWordSenseExampleFormModel
{
    public Guid SenseId { get; set; }

    [Required]
    [StringLength(2000)]
    public string GermanText { get; set; } = string.Empty;

    public bool IsPrimaryExample { get; set; } = true;

    [StringLength(16)]
    public string? TranslationLanguageCode { get; set; }

    [StringLength(2000)]
    public string? TranslationText { get; set; }
}

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

public sealed record AdminAddWordSenseExampleTranslationRequest(
    string LanguageCode,
    string TranslationText);

public sealed record AdminUpdateWordSenseExampleTranslationRequest(
    string LanguageCode,
    string TranslationText);

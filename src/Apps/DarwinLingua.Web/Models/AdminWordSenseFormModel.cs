using System.ComponentModel.DataAnnotations;

namespace DarwinLingua.Web.Models;

public sealed class AdminWordSenseFormModel
{
    [Required]
    public string PublicationStatus { get; set; } = "Draft";

    public bool IsPrimarySense { get; set; }

    [StringLength(512)]
    public string? ShortDefinitionDe { get; set; }

    [StringLength(512)]
    public string? ShortGloss { get; set; }

    [StringLength(16)]
    public string? TranslationLanguageCode { get; set; }

    [StringLength(512)]
    public string? TranslationText { get; set; }

    public bool IsPrimaryTranslation { get; set; } = true;

    [StringLength(2000)]
    public string? ExampleGermanText { get; set; }

    [StringLength(16)]
    public string? ExampleTranslationLanguageCode { get; set; }

    [StringLength(2000)]
    public string? ExampleTranslationText { get; set; }

    public bool IsPrimaryExample { get; set; } = true;
}

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

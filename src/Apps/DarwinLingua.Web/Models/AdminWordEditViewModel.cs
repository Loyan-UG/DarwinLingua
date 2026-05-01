using System.ComponentModel.DataAnnotations;

namespace DarwinLingua.Web.Models;

public sealed class AdminWordEditViewModel
{
    public Guid PublicId { get; set; }

    public bool IsNew => PublicId == Guid.Empty;

    [Required]
    [StringLength(256)]
    public string Lemma { get; set; } = string.Empty;

    [Required]
    [StringLength(16)]
    public string LanguageCode { get; set; } = "de";

    [StringLength(32)]
    public string? Article { get; set; }

    [StringLength(256)]
    public string? PluralForm { get; set; }

    [StringLength(256)]
    public string? InfinitiveForm { get; set; }

    [StringLength(256)]
    public string? PronunciationIpa { get; set; }

    [StringLength(256)]
    public string? SyllableBreak { get; set; }

    [Required]
    public string PartOfSpeech { get; set; } = "Other";

    [Required]
    public string CefrLevel { get; set; } = "A1";

    [Required]
    public string PublicationStatus { get; set; } = "Draft";

    [Required]
    public string ContentSourceType { get; set; } = "Manual";

    [StringLength(512)]
    public string? SourceReference { get; set; }

    public static AdminWordEditViewModel CreateNew() =>
        new()
        {
            PublicId = Guid.Empty,
            LanguageCode = "de",
            PartOfSpeech = "Other",
            CefrLevel = "A1",
            PublicationStatus = "Draft",
            ContentSourceType = "Manual",
        };

    public static AdminWordEditViewModel FromDetail(AdminWordDetailViewModel word) =>
        new()
        {
            PublicId = word.PublicId,
            Lemma = word.Lemma,
            LanguageCode = word.LanguageCode,
            Article = word.Article,
            PluralForm = word.PluralForm,
            InfinitiveForm = word.InfinitiveForm,
            PronunciationIpa = word.PronunciationIpa,
            SyllableBreak = word.SyllableBreak,
            PartOfSpeech = word.PartOfSpeech,
            CefrLevel = word.CefrLevel,
            PublicationStatus = word.PublicationStatus,
            ContentSourceType = word.ContentSourceType,
            SourceReference = word.SourceReference,
        };
}

public sealed record AdminUpdateWordMetadataRequest(
    string Lemma,
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
    string? SourceReference);

public sealed record AdminAddWordTopicRequest(
    Guid TopicId,
    bool IsPrimaryTopic);

public sealed record AdminAddWordLabelRequest(
    string Kind,
    string Key);

namespace DarwinLingua.WebApi.Models;

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

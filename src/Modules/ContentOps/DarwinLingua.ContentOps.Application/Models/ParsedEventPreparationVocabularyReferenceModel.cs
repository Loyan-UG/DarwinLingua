namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedEventPreparationVocabularyReferenceModel(
    string Word,
    string? PartOfSpeech,
    string? CefrLevel);

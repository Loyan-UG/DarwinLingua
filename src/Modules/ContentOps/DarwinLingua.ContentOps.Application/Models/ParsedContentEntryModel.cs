namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents one parsed lexical entry from an import package.
/// </summary>
public sealed record ParsedContentEntryModel(
    string Word,
    string Language,
    string CefrLevel,
    string PartOfSpeech,
    IReadOnlyList<string> Topics,
    IReadOnlyList<string> UsageLabels,
    IReadOnlyList<string> ContextLabels,
    IReadOnlyList<string> GrammarNotes,
    IReadOnlyList<ParsedContentCollocationModel> Collocations,
    IReadOnlyList<ParsedContentWordFamilyMemberModel> WordFamilies,
    IReadOnlyList<ParsedContentMeaningModel> Meanings,
    IReadOnlyList<ParsedContentExampleModel> Examples,
    string? Article,
    string? Plural);

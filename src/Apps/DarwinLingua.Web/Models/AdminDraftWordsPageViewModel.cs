namespace DarwinLingua.Web.Models;

public sealed record AdminDraftWordsPageViewModel(
    string? Query,
    IReadOnlyList<AdminDraftWordListItemViewModel> Words);

public sealed record AdminDraftWordListItemViewModel(
    Guid PublicId,
    string Lemma,
    string PartOfSpeech,
    string CefrLevel,
    string PublicationStatus);

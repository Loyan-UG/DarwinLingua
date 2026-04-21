namespace DarwinLingua.Web.Models;

public sealed record RecentActivityPageViewModel(
    IReadOnlyList<RecentWordActivityItemViewModel> Items);

public sealed record RecentWordActivityItemViewModel(
    Guid PublicId,
    string Lemma,
    string? Article,
    string? PluralForm,
    string PartOfSpeech,
    string CefrLevel,
    int ViewCount,
    bool IsKnown,
    bool IsDifficult,
    DateTime LastViewedAtUtc);

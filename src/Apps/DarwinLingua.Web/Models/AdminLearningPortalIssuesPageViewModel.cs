namespace DarwinLingua.Web.Models;

public sealed record AdminLearningPortalIssuesPageViewModel(
    string? AreaFilter,
    string? Query,
    int Take,
    int FilteredCount,
    int TotalCount,
    IReadOnlyList<string> Areas,
    IReadOnlyList<AdminLearningPortalIssueRowViewModel> Issues);

public sealed record AdminLearningPortalIssueRowViewModel(
    string Area,
    string Owner,
    string Issue,
    string? Target);

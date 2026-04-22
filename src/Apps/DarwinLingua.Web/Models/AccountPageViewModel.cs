using DarwinLingua.Learning.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record AccountPageViewModel(
    string DisplayName,
    string? Email,
    IReadOnlyList<string> Roles,
    UserLearningProfileModel Profile);

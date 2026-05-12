using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record ExamPrepIndexPageViewModel(
    IReadOnlyList<ExamProfileModel> Profiles,
    IReadOnlyList<ExamPrepUnitListItemModel> Units,
    IReadOnlyList<string> CefrLevels,
    IReadOnlyList<string> Sections,
    IReadOnlyList<string> TaskTypes,
    IReadOnlyList<string> SkillFocuses,
    string? SelectedProfile,
    string? SelectedCefrLevel,
    string? SelectedSection,
    string? SelectedTaskType,
    string? SelectedSkillFocus,
    string? Query);

public sealed record ExamPrepDetailPageViewModel(
    ExamPrepUnitDetailModel Unit);

public sealed record AdminExamPrepPageViewModel(
    IReadOnlyList<ExamProfileModel> Profiles,
    IReadOnlyList<ExamPrepUnitListItemModel> Units);

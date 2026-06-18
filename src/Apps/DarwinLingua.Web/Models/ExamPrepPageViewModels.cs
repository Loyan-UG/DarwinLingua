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
    string? Query,
    string PrimaryMeaningLanguageCode);

public sealed record ExamPrepDetailPageViewModel(
    ExamPrepUnitDetailModel Unit,
    string PrimaryMeaningLanguageCode);

public sealed record AdminExamPrepPageViewModel(
    IReadOnlyList<ExamProfileModel> Profiles,
    IReadOnlyList<ExamPrepUnitListItemModel> Units);

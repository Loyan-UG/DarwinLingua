using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record WritingTemplateIndexPageViewModel(
    IReadOnlyList<WritingTemplateListItemModel> Templates,
    IReadOnlyList<string> CefrLevels,
    IReadOnlyList<string> Categories,
    IReadOnlyList<string> Registers,
    string? SelectedCefrLevel,
    string? SelectedCategory,
    string? SelectedRegister,
    string? Query);

public sealed record WritingTemplateDetailPageViewModel(
    WritingTemplateDetailModel Template);

public sealed record AdminWritingTemplatesPageViewModel(
    IReadOnlyList<WritingTemplateListItemModel> Templates);

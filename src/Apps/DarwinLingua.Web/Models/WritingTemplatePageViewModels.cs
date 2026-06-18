using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record WritingTemplateIndexPageViewModel(
    IReadOnlyList<WritingTemplateListItemPageViewModel> Templates,
    IReadOnlyList<string> CefrLevels,
    IReadOnlyList<string> Categories,
    IReadOnlyList<string> Registers,
    string? SelectedCefrLevel,
    string? SelectedCategory,
    string? SelectedRegister,
    string? Query,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode);

public sealed record WritingTemplateDetailPageViewModel(
    WritingTemplateDetailModel Template,
    WritingTemplateDetailModel? SecondaryLanguageTemplate,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode);

public sealed record WritingTemplateListItemPageViewModel(
    WritingTemplateListItemModel Template,
    WritingTemplateListItemModel? SecondaryLanguageTemplate);

public sealed record AdminWritingTemplatesPageViewModel(
    IReadOnlyList<WritingTemplateListItemModel> Templates);

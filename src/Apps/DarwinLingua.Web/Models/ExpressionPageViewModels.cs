using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record ExpressionIndexPageViewModel(
    IReadOnlyList<ExpressionListItemModel> Expressions,
    IReadOnlyList<string> CefrLevels,
    IReadOnlyList<string> ExpressionTypes,
    IReadOnlyList<string> Registers,
    IReadOnlyList<string> Categories,
    IReadOnlyList<string> TopicKeys,
    string? SelectedCefrLevel,
    string? SelectedExpressionType,
    string? SelectedRegister,
    string? SelectedCategory,
    string? SelectedTopicKey,
    bool IncludeRisky,
    string? Query);

public sealed record ExpressionDetailPageViewModel(
    ExpressionDetailModel Expression,
    string PrimaryMeaningLanguageCode);


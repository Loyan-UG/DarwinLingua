namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedContentLabelDefinitionModel(
    string Kind,
    string Key,
    string DisplayName,
    IReadOnlyList<ParsedLocalizedTextModel> Localizations,
    int SortOrder);

namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents the parsed structure of a content-package JSON file.
/// </summary>
public sealed record ParsedContentPackageModel(
    string PackageVersion,
    string PackageId,
    string PackageName,
    string? Source,
    IReadOnlyList<string> DefaultMeaningLanguages,
    IReadOnlyList<ParsedContentEntryModel> Entries,
    IReadOnlyList<ParsedContentCollectionModel> Collections);

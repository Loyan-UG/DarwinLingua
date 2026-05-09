using DarwinLingua.SharedKernel.Globalization;

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
    IReadOnlyList<ParsedContentLabelDefinitionModel> Labels,
    IReadOnlyList<ParsedContentCollectionModel> Collections)
{
    public ParsedContentPackageModel(
        string packageVersion,
        string packageId,
        string packageName,
        string? source,
        IReadOnlyList<string> defaultMeaningLanguages,
        IReadOnlyList<ParsedContentEntryModel> entries,
        IReadOnlyList<ParsedContentCollectionModel> collections)
        : this(packageVersion, packageId, packageName, source, defaultMeaningLanguages, entries, CreateCompatibilityLabels(), collections)
    {
    }

    private static IReadOnlyList<ParsedContentLabelDefinitionModel> CreateCompatibilityLabels()
    {
        ParsedLocalizedTextModel[] localizations = ContentLanguageRequirements.RequiredLocalizationLanguageCodes
            .Select(languageCode => new ParsedLocalizedTextModel(languageCode, "General", null))
            .ToArray();

        return [new ParsedContentLabelDefinitionModel("Usage", "general", "General", localizations, 10)];
    }

    /// <summary>
    /// Gets parsed dialogue lessons included in the package.
    /// </summary>
    public IReadOnlyList<ParsedDialogueLessonModel> Dialogues { get; init; } = [];

    /// <summary>
    /// Gets parsed talk topics included in the package.
    /// </summary>
    public IReadOnlyList<ParsedTalkTopicModel> TalkTopics { get; init; } = [];

    /// <summary>
    /// Gets parsed conversation starter packs included in the package.
    /// </summary>
    public IReadOnlyList<ParsedConversationStarterPackModel> ConversationStarterPacks { get; init; } = [];

    /// <summary>
    /// Gets parsed event preparation packs included in the package.
    /// </summary>
    public IReadOnlyList<ParsedEventPreparationPackModel> EventPreparationPacks { get; init; } = [];
}

namespace DarwinLingua.Web.Models;

public sealed record AdminImportsPageViewModel(
    string? StatusFilter,
    IReadOnlyList<AdminContentPackageListItemViewModel> Packages)
{
    public IReadOnlyList<AdminImportModuleSummaryViewModel> ModuleSummaries =>
        Packages
            .GroupBy(static package => ResolveModuleKey(package), StringComparer.OrdinalIgnoreCase)
            .Select(static group => new AdminImportModuleSummaryViewModel(
                group.Key,
                ResolveModuleDisplayName(group.Key),
                group.Count(),
                group.Count(static package => string.Equals(package.Status, "Completed", StringComparison.OrdinalIgnoreCase)),
                group.Count(static package => package.WarningCount > 0 || package.InvalidEntries > 0 || string.Equals(package.Status, "CompletedWithWarnings", StringComparison.OrdinalIgnoreCase)),
                group.Count(static package => string.Equals(package.Status, "Failed", StringComparison.OrdinalIgnoreCase)),
                group.Sum(static package => package.TotalEntries),
                group.Sum(static package => package.InsertedEntries),
                group.Sum(static package => package.InvalidEntries),
                group.Sum(static package => package.WarningCount),
                group.Max(static package => package.CreatedAtUtc)))
            .OrderBy(static summary => summary.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string ResolveModuleKey(AdminContentPackageListItemViewModel package)
    {
        string packageText = $"{package.PackageId} {package.PackageName} {package.SourceType}".ToLowerInvariant();

        if (packageText.Contains("grammar", StringComparison.Ordinal))
        {
            return "grammar";
        }

        if (packageText.Contains("expression", StringComparison.Ordinal))
        {
            return "expressions";
        }

        if (packageText.Contains("exercise", StringComparison.Ordinal))
        {
            return "exercises";
        }

        if (packageText.Contains("course", StringComparison.Ordinal))
        {
            return "courses";
        }

        if (packageText.Contains("exam-prep", StringComparison.Ordinal) || packageText.Contains("exam prep", StringComparison.Ordinal))
        {
            return "exam-prep";
        }

        if (packageText.Contains("writing-template", StringComparison.Ordinal) || packageText.Contains("writing template", StringComparison.Ordinal))
        {
            return "writing-templates";
        }

        if (packageText.Contains("country-guidance", StringComparison.Ordinal))
        {
            return "country-guidance";
        }

        if (packageText.Contains("roleplay", StringComparison.Ordinal))
        {
            return "roleplays";
        }

        if (packageText.Contains("dialogue", StringComparison.Ordinal))
        {
            return "dialogues";
        }

        if (packageText.Contains("talk-topic", StringComparison.Ordinal) || packageText.Contains("talk topic", StringComparison.Ordinal))
        {
            return "talk-topics";
        }

        if (packageText.Contains("word", StringComparison.Ordinal) || packageText.Contains("vocabulary", StringComparison.Ordinal))
        {
            return "vocabulary";
        }

        return "other";
    }

    private static string ResolveModuleDisplayName(string moduleKey) =>
        moduleKey switch
        {
            "grammar" => "Grammar",
            "expressions" => "Expressions",
            "exercises" => "Exercises",
            "courses" => "Courses",
            "exam-prep" => "Exam Prep",
            "writing-templates" => "Writing Templates",
            "country-guidance" => "Country Guidance",
            "roleplays" => "Roleplays",
            "dialogues" => "Dialogues",
            "talk-topics" => "Talk Topics",
            "vocabulary" => "Vocabulary",
            _ => "Other",
        };
}

public sealed record AdminImportModuleSummaryViewModel(
    string ModuleKey,
    string DisplayName,
    int PackageCount,
    int CompletedPackageCount,
    int WarningPackageCount,
    int FailedPackageCount,
    int TotalEntries,
    int InsertedEntries,
    int InvalidEntries,
    int WarningCount,
    DateTime LastImportAtUtc);

public sealed record AdminContentPackageListItemViewModel(
    string PackageId,
    string PackageVersion,
    string PackageName,
    string SourceType,
    string Status,
    int TotalEntries,
    int InsertedEntries,
    int InvalidEntries,
    int WarningCount,
    DateTime CreatedAtUtc);

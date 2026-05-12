using DarwinLingua.Catalog.Application.Models;
using System.Text.Json;

namespace DarwinLingua.Web.Models;

public sealed record ExerciseIndexPageViewModel(
    IReadOnlyList<ExerciseSetListItemModel> ExerciseSets,
    IReadOnlyList<string> CefrLevels,
    string? SelectedCefrLevel,
    string? Query);

public sealed record ExerciseSetPageViewModel(
    ExerciseSetDetailModel ExerciseSet);

public sealed record ExerciseRunnerPageViewModel(
    ExerciseDetailModel Exercise,
    ExerciseAttemptResultModel? Result,
    string? SubmittedAnswerJson)
{
    public ExercisePromptViewModel Prompt { get; } = ExercisePromptViewModel.FromJson(Exercise.PromptJson);
}

public sealed record ExercisePromptViewModel(
    string? Stem,
    IReadOnlyList<ExercisePromptOptionViewModel> Options)
{
    public bool HasOptions => Options.Count > 0;

    public static ExercisePromptViewModel FromJson(string promptJson)
    {
        if (string.IsNullOrWhiteSpace(promptJson))
        {
            return new ExercisePromptViewModel(null, []);
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(promptJson);
            JsonElement root = document.RootElement;
            string? stem = ReadString(root, "stem") ?? ReadString(root, "text") ?? ReadString(root, "prompt");
            IReadOnlyList<ExercisePromptOptionViewModel> options = ReadOptions(root);
            return new ExercisePromptViewModel(stem, options);
        }
        catch (JsonException)
        {
            return new ExercisePromptViewModel(null, []);
        }
    }

    private static string? ReadString(JsonElement root, string propertyName) =>
        root.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static IReadOnlyList<ExercisePromptOptionViewModel> ReadOptions(JsonElement root)
    {
        if (!root.TryGetProperty("options", out JsonElement options) || options.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return options.EnumerateArray()
            .Select(ReadOption)
            .Where(option => !string.IsNullOrWhiteSpace(option.Id) && !string.IsNullOrWhiteSpace(option.Text))
            .ToArray();
    }

    private static ExercisePromptOptionViewModel ReadOption(JsonElement option)
    {
        if (option.ValueKind == JsonValueKind.String)
        {
            string value = option.GetString() ?? string.Empty;
            return new ExercisePromptOptionViewModel(value, value);
        }

        string id = ReadString(option, "id") ?? ReadString(option, "value") ?? string.Empty;
        string text = ReadString(option, "text") ?? ReadString(option, "label") ?? id;
        return new ExercisePromptOptionViewModel(id, text);
    }
}

public sealed record ExercisePromptOptionViewModel(
    string Id,
    string Text);

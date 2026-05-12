using System.Text.Json;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class ExerciseQueryService(IExerciseRepository repository) : IExerciseQueryService
{
    public Task<IReadOnlyList<ExerciseSetListItemModel>> GetPublishedExerciseSetsAsync(ExerciseSetListFilterModel filter, CancellationToken cancellationToken) =>
        repository.GetPublishedExerciseSetsAsync(filter, cancellationToken);

    public Task<ExerciseSetDetailModel?> GetPublishedExerciseSetBySlugAsync(string slug, CancellationToken cancellationToken) =>
        repository.GetPublishedExerciseSetBySlugAsync(slug, cancellationToken);

    public Task<IReadOnlyList<ExerciseListItemModel>> GetPublishedExercisesAsync(ExerciseListFilterModel filter, CancellationToken cancellationToken) =>
        repository.GetPublishedExercisesAsync(filter, cancellationToken);

    public Task<ExerciseDetailModel?> GetPublishedExerciseBySlugAsync(string slug, CancellationToken cancellationToken) =>
        repository.GetPublishedExerciseBySlugAsync(slug, cancellationToken);
}

internal sealed class ExerciseAttemptService(IExerciseRepository repository) : IExerciseAttemptService
{
    private const int MaxSubmittedAnswerJsonLength = 4096;

    public async Task<ExerciseAttemptResultModel?> SubmitAttemptAsync(
        string slug,
        ExerciseAttemptRequestModel request,
        string userId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        string normalizedUserId = NormalizeRequiredUserId(userId);
        string submittedAnswerJson = NormalizeSubmittedAnswerJson(request.SubmittedAnswerJson);
        Exercise? exercise = await repository.GetPublishedExerciseEntityBySlugAsync(slug, cancellationToken).ConfigureAwait(false);
        if (exercise is null)
        {
            return null;
        }

        bool isCorrect = ExerciseAnswerEvaluator.Evaluate(exercise.ExerciseType, exercise.AnswerKeyJson, submittedAnswerJson);
        string explanation = isCorrect ? exercise.CorrectExplanation : exercise.IncorrectExplanation;
        DateTime attemptedAtUtc = DateTime.UtcNow;

        UserExerciseAttempt attempt = new(
            Guid.NewGuid(),
            normalizedUserId,
            exercise.Slug,
            submittedAnswerJson,
            isCorrect,
            explanation,
            attemptedAtUtc);

        await repository.SaveAttemptAsync(attempt, cancellationToken).ConfigureAwait(false);

        return new ExerciseAttemptResultModel(
            exercise.Slug,
            isCorrect,
            explanation,
            exercise.Hint,
            exercise.CommonMistakeNote,
            attemptedAtUtc);
    }

    public async Task<ExerciseAttemptResultModel?> EvaluateAttemptAsync(
        string slug,
        ExerciseAttemptRequestModel request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        string submittedAnswerJson = NormalizeSubmittedAnswerJson(request.SubmittedAnswerJson);
        Exercise? exercise = await repository.GetPublishedExerciseEntityBySlugAsync(slug, cancellationToken).ConfigureAwait(false);
        if (exercise is null)
        {
            return null;
        }

        bool isCorrect = ExerciseAnswerEvaluator.Evaluate(exercise.ExerciseType, exercise.AnswerKeyJson, submittedAnswerJson);
        string explanation = isCorrect ? exercise.CorrectExplanation : exercise.IncorrectExplanation;

        return new ExerciseAttemptResultModel(
            exercise.Slug,
            isCorrect,
            explanation,
            exercise.Hint,
            exercise.CommonMistakeNote,
            DateTime.UtcNow);
    }

    private static string NormalizeRequiredUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainRuleException("Authenticated user id is required for persisted exercise attempts.");
        }

        return userId.Trim();
    }

    private static string NormalizeSubmittedAnswerJson(string submittedAnswerJson)
    {
        if (string.IsNullOrWhiteSpace(submittedAnswerJson))
        {
            throw new DomainRuleException("Submitted answer JSON is required.");
        }

        string trimmed = submittedAnswerJson.Trim();
        if (trimmed.Length > MaxSubmittedAnswerJsonLength)
        {
            throw new DomainRuleException($"Submitted answer JSON must be {MaxSubmittedAnswerJsonLength} characters or fewer.");
        }

        try
        {
            using JsonDocument submitted = JsonDocument.Parse(trimmed);
            JsonValueKind kind = submitted.RootElement.ValueKind;
            if (kind is not (JsonValueKind.Object or JsonValueKind.Array))
            {
                throw new DomainRuleException("Submitted answer JSON must be an object or array.");
            }

            return JsonSerializer.Serialize(submitted.RootElement);
        }
        catch (JsonException)
        {
            throw new DomainRuleException("Submitted answer JSON is malformed.");
        }
    }
}

public static class ExerciseAnswerEvaluator
{
    public static bool Evaluate(string exerciseType, string answerKeyJson, string submittedAnswerJson)
    {
        using JsonDocument answerKey = JsonDocument.Parse(answerKeyJson);
        using JsonDocument submitted = JsonDocument.Parse(submittedAnswerJson);

        JsonElement answerRoot = answerKey.RootElement;
        JsonElement submittedRoot = submitted.RootElement;
        string type = exerciseType.Trim().ToLowerInvariant();

        return type switch
        {
            "multiple-choice" or "article-selection" or "case-selection" or "vocabulary-choice" or "grammar-choice" or "dialogue-completion"
                => SetEquals(ReadStringArray(answerRoot, "correctOptionIds"), ReadStringArray(submittedRoot, "selectedOptionIds")),
            "fill-in-the-blank" or "conjugation" or "translation-controlled"
                => CompareAcceptedAnswers(answerRoot, submittedRoot),
            "matching"
                => SetEquals(ReadStringArray(answerRoot, "pairs"), ReadStringArray(submittedRoot, "pairs")),
            "sentence-ordering"
                => SequenceEquals(ReadStringArray(answerRoot, "orderedSegments"), ReadStringArray(submittedRoot, "orderedSegments")),
            "error-correction"
                => Normalize(ReadString(answerRoot, "correctedText")) == Normalize(ReadString(submittedRoot, "correctedText")),
            _ => false,
        };
    }

    private static bool CompareAcceptedAnswers(JsonElement answerRoot, JsonElement submittedRoot)
    {
        string[] accepted = ReadStringArray(answerRoot, "acceptedAnswers").Select(Normalize).ToArray();
        string[] submitted = ReadStringArray(submittedRoot, "answers").Select(Normalize).ToArray();
        if (submitted.Length == 0)
        {
            string singleAnswer = ReadString(submittedRoot, "answer");
            submitted = string.IsNullOrWhiteSpace(singleAnswer) ? [] : [Normalize(singleAnswer)];
        }

        return submitted.Length > 0 && submitted.All(answer => accepted.Contains(answer, StringComparer.Ordinal));
    }

    private static string ReadString(JsonElement root, string propertyName) =>
        root.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static string[] ReadStringArray(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out JsonElement value) || value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return value.EnumerateArray()
            .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() ?? string.Empty : item.GetRawText())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(Normalize)
            .ToArray();
    }

    private static bool SetEquals(string[] expected, string[] actual) =>
        expected.Length > 0 && expected.ToHashSet(StringComparer.Ordinal).SetEquals(actual);

    private static bool SequenceEquals(string[] expected, string[] actual) =>
        expected.Length == actual.Length && expected.SequenceEqual(actual, StringComparer.Ordinal);

    private static string Normalize(string value) =>
        string.Join(' ', value.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
}

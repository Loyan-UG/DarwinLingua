namespace DarwinLingua.WebApi.Models;

public sealed record AdminScenariosResponse(
    IReadOnlyList<AdminScenarioItemResponse> Scenarios);

public sealed record AdminScenarioItemResponse(
    Guid ScenarioId,
    string Slug,
    string Title,
    string CefrLevel,
    string Category,
    string PublicationStatus,
    int SortOrder,
    int DialogueTurnCount,
    int PhraseCount,
    int QuestionCount,
    DateTime UpdatedAtUtc);

public sealed record AdminScenarioDetailResponse(
    Guid ScenarioId,
    string Slug,
    string Title,
    string Description,
    string LearnerGoal,
    string CefrLevel,
    string Category,
    string PublicationStatus,
    int SortOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<AdminScenarioDialogueTurnResponse> DialogueTurns,
    IReadOnlyList<AdminScenarioPhraseResponse> UsefulPhrases,
    IReadOnlyList<AdminScenarioQuestionResponse> Questions);

public sealed record AdminScenarioDialogueTurnResponse(
    Guid TurnId,
    int SortOrder,
    string SpeakerRole,
    string BaseText,
    IReadOnlyList<AdminScenarioTranslationResponse> Translations);

public sealed record AdminScenarioPhraseResponse(
    Guid PhraseId,
    int SortOrder,
    string BaseText,
    string? UsageNote,
    IReadOnlyList<AdminScenarioTranslationResponse> Translations);

public sealed record AdminScenarioQuestionResponse(
    Guid QuestionId,
    int SortOrder,
    string Prompt,
    IReadOnlyList<AdminScenarioTranslationResponse> Translations,
    IReadOnlyList<AdminScenarioAnswerResponse> Answers);

public sealed record AdminScenarioAnswerResponse(
    Guid AnswerId,
    int SortOrder,
    string Text,
    bool IsCorrect,
    string? Feedback,
    IReadOnlyList<AdminScenarioTranslationResponse> Translations);

public sealed record AdminScenarioTranslationResponse(
    Guid TranslationId,
    string LanguageCode,
    string Text);

public sealed record AdminSaveScenarioRequest(
    string Slug,
    string Title,
    string Description,
    string LearnerGoal,
    string CefrLevel,
    string Category,
    string PublicationStatus,
    int SortOrder);

public sealed record AdminAddScenarioDialogueTurnRequest(
    int SortOrder,
    string SpeakerRole,
    string BaseText);

public sealed record AdminAddScenarioPhraseRequest(
    int SortOrder,
    string BaseText,
    string? UsageNote);

public sealed record AdminAddScenarioQuestionRequest(
    int SortOrder,
    string Prompt);

public sealed record AdminAddScenarioAnswerRequest(
    int SortOrder,
    string Text,
    bool IsCorrect,
    string? Feedback);

public sealed record AdminAddScenarioTranslationRequest(
    string LanguageCode,
    string Text);

public sealed record AdminBulkScenarioImportRequest(
    IReadOnlyList<AdminBulkScenarioImportItemRequest> Scenarios);

public sealed record AdminBulkScenarioImportItemRequest(
    string Slug,
    string Title,
    string Description,
    string LearnerGoal,
    string? CefrLevel,
    string? Category,
    string? PublicationStatus,
    int SortOrder,
    IReadOnlyList<AdminBulkScenarioDialogueTurnImportRequest>? DialogueTurns,
    IReadOnlyList<AdminBulkScenarioPhraseImportRequest>? UsefulPhrases,
    IReadOnlyList<AdminBulkScenarioQuestionImportRequest>? Questions);

public sealed record AdminBulkScenarioDialogueTurnImportRequest(
    int SortOrder,
    string SpeakerRole,
    string BaseText,
    IReadOnlyList<AdminAddScenarioTranslationRequest>? Translations);

public sealed record AdminBulkScenarioPhraseImportRequest(
    int SortOrder,
    string BaseText,
    string? UsageNote,
    IReadOnlyList<AdminAddScenarioTranslationRequest>? Translations);

public sealed record AdminBulkScenarioQuestionImportRequest(
    int SortOrder,
    string Prompt,
    IReadOnlyList<AdminAddScenarioTranslationRequest>? Translations,
    IReadOnlyList<AdminBulkScenarioAnswerImportRequest>? Answers);

public sealed record AdminBulkScenarioAnswerImportRequest(
    int SortOrder,
    string Text,
    bool IsCorrect,
    string? Feedback,
    IReadOnlyList<AdminAddScenarioTranslationRequest>? Translations);

public sealed record AdminBulkScenarioImportResponse(
    int TotalCount,
    int ImportedCount,
    int SkippedCount,
    int FailedCount,
    IReadOnlyList<AdminBulkScenarioImportItemResult> Items);

public sealed record AdminBulkScenarioImportItemResult(
    int RowNumber,
    string? Slug,
    Guid? ScenarioId,
    string Status,
    string Message);

namespace DarwinLingua.WebApi.Models;

public sealed record AdminDialoguesResponse(
    IReadOnlyList<AdminDialogueItemResponse> Dialogues);

public sealed record AdminDialogueItemResponse(
    Guid DialogueId,
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

public sealed record AdminDialogueDetailResponse(
    Guid DialogueId,
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
    IReadOnlyList<AdminDialogueTurnResponse> DialogueTurns,
    IReadOnlyList<AdminDialoguePhraseResponse> UsefulPhrases,
    IReadOnlyList<AdminDialogueQuestionResponse> Questions);

public sealed record AdminDialogueTurnResponse(
    Guid TurnId,
    int SortOrder,
    string SpeakerRole,
    string BaseText,
    IReadOnlyList<AdminDialogueTranslationResponse> Translations);

public sealed record AdminDialoguePhraseResponse(
    Guid PhraseId,
    int SortOrder,
    string BaseText,
    string? UsageNote,
    IReadOnlyList<AdminDialogueTranslationResponse> Translations);

public sealed record AdminDialogueQuestionResponse(
    Guid QuestionId,
    int SortOrder,
    string Prompt,
    IReadOnlyList<AdminDialogueTranslationResponse> Translations,
    IReadOnlyList<AdminDialogueAnswerResponse> Answers);

public sealed record AdminDialogueAnswerResponse(
    Guid AnswerId,
    int SortOrder,
    string Text,
    bool IsCorrect,
    string? Feedback,
    IReadOnlyList<AdminDialogueTranslationResponse> Translations);

public sealed record AdminDialogueTranslationResponse(
    Guid TranslationId,
    string LanguageCode,
    string Text);

public sealed record AdminSaveDialogueRequest(
    string Slug,
    string Title,
    string Description,
    string LearnerGoal,
    string CefrLevel,
    string Category,
    string PublicationStatus,
    int SortOrder);

public sealed record AdminAddDialogueTurnRequest(
    int SortOrder,
    string SpeakerRole,
    string BaseText);

public sealed record AdminAddDialoguePhraseRequest(
    int SortOrder,
    string BaseText,
    string? UsageNote);

public sealed record AdminAddDialogueQuestionRequest(
    int SortOrder,
    string Prompt);

public sealed record AdminAddDialogueAnswerRequest(
    int SortOrder,
    string Text,
    bool IsCorrect,
    string? Feedback);

public sealed record AdminAddDialogueTranslationRequest(
    string LanguageCode,
    string Text);

public sealed record AdminBulkDialogueImportRequest(
    IReadOnlyList<AdminBulkDialogueImportItemRequest> Dialogues);

public sealed record AdminBulkDialogueImportItemRequest(
    string Slug,
    string Title,
    string Description,
    string LearnerGoal,
    string? CefrLevel,
    string? Category,
    string? PublicationStatus,
    int SortOrder,
    IReadOnlyList<AdminBulkDialogueTurnImportRequest>? DialogueTurns,
    IReadOnlyList<AdminBulkDialoguePhraseImportRequest>? UsefulPhrases,
    IReadOnlyList<AdminBulkDialogueQuestionImportRequest>? Questions);

public sealed record AdminBulkDialogueTurnImportRequest(
    int SortOrder,
    string SpeakerRole,
    string BaseText,
    IReadOnlyList<AdminAddDialogueTranslationRequest>? Translations);

public sealed record AdminBulkDialoguePhraseImportRequest(
    int SortOrder,
    string BaseText,
    string? UsageNote,
    IReadOnlyList<AdminAddDialogueTranslationRequest>? Translations);

public sealed record AdminBulkDialogueQuestionImportRequest(
    int SortOrder,
    string Prompt,
    IReadOnlyList<AdminAddDialogueTranslationRequest>? Translations,
    IReadOnlyList<AdminBulkDialogueAnswerImportRequest>? Answers);

public sealed record AdminBulkDialogueAnswerImportRequest(
    int SortOrder,
    string Text,
    bool IsCorrect,
    string? Feedback,
    IReadOnlyList<AdminAddDialogueTranslationRequest>? Translations);

public sealed record AdminBulkDialogueImportResponse(
    int TotalCount,
    int ImportedCount,
    int SkippedCount,
    int FailedCount,
    IReadOnlyList<AdminBulkDialogueImportItemResult> Items);

public sealed record AdminBulkDialogueImportItemResult(
    int RowNumber,
    string? Slug,
    Guid? DialogueId,
    string Status,
    string Message);

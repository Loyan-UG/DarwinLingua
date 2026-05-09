using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DarwinLingua.Web.Models;

public sealed record AdminDialoguesPageViewModel(
    IReadOnlyList<AdminDialogueItemViewModel> Dialogues);

public sealed record AdminDialogueItemViewModel(
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

public sealed record AdminDialogueDetailViewModel(
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
    IReadOnlyList<AdminDialogueTurnViewModel> DialogueTurns,
    IReadOnlyList<AdminDialoguePhraseViewModel> UsefulPhrases,
    IReadOnlyList<AdminDialogueQuestionViewModel> Questions);

public sealed record AdminDialogueTurnViewModel(
    Guid TurnId,
    int SortOrder,
    string SpeakerRole,
    string BaseText,
    IReadOnlyList<AdminDialogueTranslationViewModel> Translations);

public sealed record AdminDialoguePhraseViewModel(
    Guid PhraseId,
    int SortOrder,
    string BaseText,
    string? UsageNote,
    IReadOnlyList<AdminDialogueTranslationViewModel> Translations);

public sealed record AdminDialogueQuestionViewModel(
    Guid QuestionId,
    int SortOrder,
    string Prompt,
    IReadOnlyList<AdminDialogueTranslationViewModel> Translations,
    IReadOnlyList<AdminDialogueAnswerViewModel> Answers);

public sealed record AdminDialogueAnswerViewModel(
    Guid AnswerId,
    int SortOrder,
    string Text,
    bool IsCorrect,
    string? Feedback,
    IReadOnlyList<AdminDialogueTranslationViewModel> Translations);

public sealed record AdminDialogueTranslationViewModel(
    Guid TranslationId,
    string LanguageCode,
    string Text);

public sealed record AdminDialogueTranslationListViewModel(
    Guid DialogueId,
    string DeleteAction,
    string ConfirmationMessage,
    IReadOnlyList<AdminDialogueTranslationViewModel> Translations,
    Guid? QuestionId = null,
    Guid? AnswerId = null,
    Guid? TurnId = null,
    Guid? PhraseId = null)
{
    public RouteValueDictionary GetDeleteRouteValues(Guid translationId)
    {
        RouteValueDictionary values = new()
        {
            ["dialogueId"] = DialogueId,
            ["translationId"] = translationId,
        };

        if (QuestionId.HasValue)
        {
            values["questionId"] = QuestionId.Value;
        }

        if (AnswerId.HasValue)
        {
            values["answerId"] = AnswerId.Value;
        }

        if (TurnId.HasValue)
        {
            values["turnId"] = TurnId.Value;
        }

        if (PhraseId.HasValue)
        {
            values["phraseId"] = PhraseId.Value;
        }

        return values;
    }
}

public sealed record AdminDialogueAddTranslationFormViewModel(
    Guid DialogueId,
    string AddAction,
    string Summary,
    string TextLabel,
    string ConfirmationMessage,
    Guid? QuestionId = null,
    Guid? AnswerId = null,
    Guid? TurnId = null,
    Guid? PhraseId = null)
{
    public RouteValueDictionary GetAddRouteValues()
    {
        RouteValueDictionary values = new()
        {
            ["dialogueId"] = DialogueId,
        };

        if (QuestionId.HasValue)
        {
            values["questionId"] = QuestionId.Value;
        }

        if (AnswerId.HasValue)
        {
            values["answerId"] = AnswerId.Value;
        }

        if (TurnId.HasValue)
        {
            values["turnId"] = TurnId.Value;
        }

        if (PhraseId.HasValue)
        {
            values["phraseId"] = PhraseId.Value;
        }

        return values;
    }
}

public sealed class AdminDialogueEditViewModel
{
    public Guid DialogueId { get; set; }

    public bool IsNew => DialogueId == Guid.Empty;

    [Required]
    [StringLength(128)]
    [RegularExpression("^[a-z0-9]+(-[a-z0-9]+)*$")]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(1024)]
    public string LearnerGoal { get; set; } = string.Empty;

    [Required]
    public string CefrLevel { get; set; } = "A1";

    [Required]
    [StringLength(128)]
    [RegularExpression("^[a-z0-9]+(-[a-z0-9]+)*$")]
    public string Category { get; set; } = "general";

    [Required]
    public string PublicationStatus { get; set; } = "Draft";

    public int SortOrder { get; set; }

    public static AdminDialogueEditViewModel CreateNew() => new()
    {
        DialogueId = Guid.Empty,
        CefrLevel = "A1",
        Category = "general",
        PublicationStatus = "Draft",
    };

    public static AdminDialogueEditViewModel FromDetail(AdminDialogueDetailViewModel dialogue) => new()
    {
        DialogueId = dialogue.DialogueId,
        Slug = dialogue.Slug,
        Title = dialogue.Title,
        Description = dialogue.Description,
        LearnerGoal = dialogue.LearnerGoal,
        CefrLevel = dialogue.CefrLevel,
        Category = dialogue.Category,
        PublicationStatus = dialogue.PublicationStatus,
        SortOrder = dialogue.SortOrder,
    };
}

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

public sealed class AdminBulkDialogueImportFormModel
{
    public IFormFile? JsonFile { get; set; }
}

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

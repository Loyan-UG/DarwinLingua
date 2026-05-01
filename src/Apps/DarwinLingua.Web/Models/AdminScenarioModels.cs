using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DarwinLingua.Web.Models;

public sealed record AdminScenariosPageViewModel(
    IReadOnlyList<AdminScenarioItemViewModel> Scenarios);

public sealed record AdminScenarioItemViewModel(
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

public sealed record AdminScenarioDetailViewModel(
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
    IReadOnlyList<AdminScenarioDialogueTurnViewModel> DialogueTurns,
    IReadOnlyList<AdminScenarioPhraseViewModel> UsefulPhrases,
    IReadOnlyList<AdminScenarioQuestionViewModel> Questions);

public sealed record AdminScenarioDialogueTurnViewModel(
    Guid TurnId,
    int SortOrder,
    string SpeakerRole,
    string BaseText,
    IReadOnlyList<AdminScenarioTranslationViewModel> Translations);

public sealed record AdminScenarioPhraseViewModel(
    Guid PhraseId,
    int SortOrder,
    string BaseText,
    string? UsageNote,
    IReadOnlyList<AdminScenarioTranslationViewModel> Translations);

public sealed record AdminScenarioQuestionViewModel(
    Guid QuestionId,
    int SortOrder,
    string Prompt,
    IReadOnlyList<AdminScenarioTranslationViewModel> Translations,
    IReadOnlyList<AdminScenarioAnswerViewModel> Answers);

public sealed record AdminScenarioAnswerViewModel(
    Guid AnswerId,
    int SortOrder,
    string Text,
    bool IsCorrect,
    string? Feedback,
    IReadOnlyList<AdminScenarioTranslationViewModel> Translations);

public sealed record AdminScenarioTranslationViewModel(
    Guid TranslationId,
    string LanguageCode,
    string Text);

public sealed record AdminScenarioTranslationListViewModel(
    Guid ScenarioId,
    string DeleteAction,
    string ConfirmationMessage,
    IReadOnlyList<AdminScenarioTranslationViewModel> Translations,
    Guid? QuestionId = null,
    Guid? AnswerId = null,
    Guid? TurnId = null,
    Guid? PhraseId = null)
{
    public RouteValueDictionary GetDeleteRouteValues(Guid translationId)
    {
        RouteValueDictionary values = new()
        {
            ["scenarioId"] = ScenarioId,
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

public sealed record AdminScenarioAddTranslationFormViewModel(
    Guid ScenarioId,
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
            ["scenarioId"] = ScenarioId,
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

public sealed class AdminScenarioEditViewModel
{
    public Guid ScenarioId { get; set; }

    public bool IsNew => ScenarioId == Guid.Empty;

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

    public static AdminScenarioEditViewModel CreateNew() => new()
    {
        ScenarioId = Guid.Empty,
        CefrLevel = "A1",
        Category = "general",
        PublicationStatus = "Draft",
    };

    public static AdminScenarioEditViewModel FromDetail(AdminScenarioDetailViewModel scenario) => new()
    {
        ScenarioId = scenario.ScenarioId,
        Slug = scenario.Slug,
        Title = scenario.Title,
        Description = scenario.Description,
        LearnerGoal = scenario.LearnerGoal,
        CefrLevel = scenario.CefrLevel,
        Category = scenario.Category,
        PublicationStatus = scenario.PublicationStatus,
        SortOrder = scenario.SortOrder,
    };
}

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

public sealed class AdminBulkScenarioImportFormModel
{
    public IFormFile? JsonFile { get; set; }
}

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

using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Controllers;

[Route("dialogues")]
public sealed class DialoguesController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor,
    IWebEntitledFeatureAccessService featureAccessService,
    IStringLocalizer<SharedResource> localizer,
    ILogger<DialoguesController> logger,
    IWebProductAnalyticsService? analyticsService = null) : Controller
{
    [HttpGet("", Name = "Dialogues_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(string? topic, string? q, CancellationToken cancellationToken)
    {
        IReadOnlyList<DialogueLessonListItemModel> dialogues;

        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            dialogues = await catalogApiClient
                .GetDialoguesAsync(catalogTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Dialogues could not be loaded.");
            dialogues = [];
        }

        string? normalizedTopic = WebRouteInput.NormalizeSlug(topic ?? string.Empty);
        string? normalizedQuery = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        IReadOnlyList<string> topicKeys = dialogues
            .SelectMany(static dialogue => dialogue.TopicKeys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static topicKey => topicKey)
            .ToArray();

        IEnumerable<DialogueLessonListItemModel> filteredDialogues = dialogues;
        if (!string.IsNullOrWhiteSpace(normalizedTopic))
        {
            filteredDialogues = filteredDialogues.Where(dialogue =>
                dialogue.TopicKeys.Any(topicKey => topicKey.Equals(normalizedTopic, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            filteredDialogues = filteredDialogues.Where(dialogue =>
                dialogue.Title.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                dialogue.Description.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                dialogue.LearnerGoal.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                dialogue.Category.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                dialogue.TopicKeys.Any(topicKey => topicKey.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)));
        }

        return View(new DialogueIndexPageViewModel(
            filteredDialogues.ToArray(),
            topicKeys,
            normalizedTopic,
            normalizedQuery));
    }

    [HttpGet("{slug}", Name = "Dialogues_Detail")]
    [OutputCache(NoStore = true)]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        string? normalizedSlug = WebRouteInput.NormalizeSlug(slug);
        if (normalizedSlug is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string? effectiveSecondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);

        DialogueLessonDetailModel? dialogue;

        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            dialogue = await catalogApiClient
                .GetDialogueBySlugAsync(
                    normalizedSlug,
                    profile.PreferredMeaningLanguage1,
                    effectiveSecondaryMeaningLanguageCode,
                    catalogTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Dialogue detail could not be loaded for {Slug}.", normalizedSlug);
            return ServiceUnavailableView(
                localizer["Dialogue is temporarily unavailable"].Value,
                localizer["This practice dialogue could not be loaded right now. Please return to dialogues and try again."].Value);
        }

        if (dialogue is null)
        {
            return NotFound();
        }

        analyticsService?.Record(WebProductAnalyticsEvents.DialogueViewed, $"dialogue:{dialogue.Slug}");

        IReadOnlyList<ConversationStarterPackListItemModel> relatedStarterPacks = [];
        IReadOnlyList<EventPreparationPackListItemModel> relatedEventPreparationPacks = [];

        try
        {
            Task<IReadOnlyList<ConversationStarterPackListItemModel>> relatedStarterPacksTask = catalogApiClient
                .GetConversationStarterPacksForDialogueAsync(normalizedSlug, cancellationToken);
            Task<IReadOnlyList<EventPreparationPackListItemModel>> relatedEventPreparationPacksTask =
                LoadRelatedEventPreparationPacksAsync(normalizedSlug, cancellationToken);
            await Task.WhenAll(relatedStarterPacksTask, relatedEventPreparationPacksTask).ConfigureAwait(false);
            relatedStarterPacks = await relatedStarterPacksTask.ConfigureAwait(false);
            relatedEventPreparationPacks = await relatedEventPreparationPacksTask.ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Dialogue related content could not be loaded for {Slug}.", normalizedSlug);
        }

        return View(new DialogueDetailPageViewModel(
            dialogue,
            relatedStarterPacks,
            relatedEventPreparationPacks,
            profile.PreferredMeaningLanguage1,
            effectiveSecondaryMeaningLanguageCode));
    }

    [HttpGet("{slug}/roleplay", Name = "Dialogues_Roleplay")]
    [OutputCache(NoStore = true)]
    public async Task<IActionResult> Roleplay(string slug, CancellationToken cancellationToken)
    {
        string? normalizedSlug = WebRouteInput.NormalizeSlug(slug);
        if (normalizedSlug is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string? effectiveSecondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);

        DialogueLessonDetailModel? dialogue;

        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            dialogue = await catalogApiClient
                .GetDialogueBySlugAsync(
                    normalizedSlug,
                    profile.PreferredMeaningLanguage1,
                    effectiveSecondaryMeaningLanguageCode,
                    catalogTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Dialogue roleplay could not be loaded for {Slug}.", normalizedSlug);
            return ServiceUnavailableView(
                localizer["Roleplay is temporarily unavailable"].Value,
                localizer["This guided roleplay could not be loaded right now. Please return to dialogues and try again."].Value);
        }

        if (dialogue is null)
        {
            return NotFound();
        }

        analyticsService?.Record(WebProductAnalyticsEvents.RoleplayViewed, $"dialogue:{dialogue.Slug}");

        return View(new DialogueRoleplayPageViewModel(
            dialogue,
            BuildRoleplaySteps(dialogue),
            profile.PreferredMeaningLanguage1,
            effectiveSecondaryMeaningLanguageCode));
    }

    private IReadOnlyList<DialogueRoleplayStepViewModel> BuildRoleplaySteps(DialogueLessonDetailModel dialogue)
    {
        List<DialogueRoleplayStepViewModel> steps = [];
        for (int index = 0; index < dialogue.DialogueTurns.Count; index++)
        {
            DialogueTurnModel prompt = dialogue.DialogueTurns[index];
            if (IsLearnerRole(prompt.SpeakerRole))
            {
                continue;
            }

            DialogueTurnModel? answer = dialogue.DialogueTurns
                .Skip(index + 1)
                .FirstOrDefault(static turn => IsLearnerRole(turn.SpeakerRole));

            if (answer is null)
            {
                continue;
            }

            steps.Add(new DialogueRoleplayStepViewModel(
                steps.Count + 1,
                prompt.SpeakerRole,
                prompt.BaseText,
                prompt.PrimaryMeaning,
                prompt.SecondaryMeaning,
                answer.SpeakerRole,
                answer.BaseText,
                answer.PrimaryMeaning,
                answer.SecondaryMeaning,
                localizer["Compare your answer with the model answer, then replay the line aloud."].Value));
        }

        return steps;
    }

    private static bool IsLearnerRole(string speakerRole) =>
        string.Equals(speakerRole, "learner", StringComparison.OrdinalIgnoreCase);

    private ViewResult ServiceUnavailableView(string title, string message)
    {
        Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
        {
            Title = title,
            Message = message,
            RequestId = HttpContext.TraceIdentifier
        });
    }

    private async Task<IReadOnlyList<EventPreparationPackListItemModel>> LoadRelatedEventPreparationPacksAsync(
        string dialogueSlug,
        CancellationToken cancellationToken)
    {
        return await featureAccessService.CanUseEventPreparationPacksAsync(cancellationToken).ConfigureAwait(false)
            ? await catalogApiClient.GetEventPreparationPacksForDialogueAsync(dialogueSlug, cancellationToken).ConfigureAwait(false)
            : [];
    }
}

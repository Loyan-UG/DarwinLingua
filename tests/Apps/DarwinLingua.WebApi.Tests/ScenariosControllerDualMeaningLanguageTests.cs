using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Web.Controllers;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class DialoguesControllerDualMeaningLanguageTests
{
    [Fact]
    public async Task Detail_ShouldPassResolvedMeaningLanguagesToApiAndViewModel()
    {
        CapturingCatalogApiClient catalogApiClient = new(CreateDialogue());
        DialoguesController controller = new(
            catalogApiClient,
            new StaticLearningProfileAccessor(new UserLearningProfileModel("local", "en", "fa", "en")),
            new StaticFeatureAccessService("fa"),
            new TestStringLocalizer(),
            NullLogger<DialoguesController>.Instance);

        IActionResult actionResult = await controller.Detail("at-the-pharmacy", CancellationToken.None);

        ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);
        DialogueDetailPageViewModel viewModel = Assert.IsType<DialogueDetailPageViewModel>(viewResult.Model);
        Assert.Equal("at-the-pharmacy", catalogApiClient.Slug);
        Assert.Equal("en", catalogApiClient.PrimaryMeaningLanguageCode);
        Assert.Equal("fa", catalogApiClient.SecondaryMeaningLanguageCode);
        Assert.Equal("en", viewModel.PrimaryMeaningLanguageCode);
        Assert.Equal("fa", viewModel.SecondaryMeaningLanguageCode);
    }

    [Fact]
    public async Task Detail_ShouldOmitSecondaryMeaningLanguage_WhenFeatureIsUnavailable()
    {
        CapturingCatalogApiClient catalogApiClient = new(CreateDialogue());
        DialoguesController controller = new(
            catalogApiClient,
            new StaticLearningProfileAccessor(new UserLearningProfileModel("local", "en", "fa", "en")),
            new StaticFeatureAccessService(null),
            new TestStringLocalizer(),
            NullLogger<DialoguesController>.Instance);

        IActionResult actionResult = await controller.Detail("at-the-pharmacy", CancellationToken.None);

        ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);
        DialogueDetailPageViewModel viewModel = Assert.IsType<DialogueDetailPageViewModel>(viewResult.Model);
        Assert.Equal("en", catalogApiClient.PrimaryMeaningLanguageCode);
        Assert.Null(catalogApiClient.SecondaryMeaningLanguageCode);
        Assert.Null(viewModel.SecondaryMeaningLanguageCode);
    }

    [Fact]
    public async Task Detail_ShouldNotLoadPreparationPacks_WhenFeatureIsUnavailable()
    {
        CapturingCatalogApiClient catalogApiClient = new(CreateDialogue());
        DialoguesController controller = new(
            catalogApiClient,
            new StaticLearningProfileAccessor(new UserLearningProfileModel("local", "en", "fa", "en")),
            new StaticFeatureAccessService(null, canUseEventPreparationPacks: false),
            new TestStringLocalizer(),
            NullLogger<DialoguesController>.Instance);

        IActionResult actionResult = await controller.Detail("at-the-pharmacy", CancellationToken.None);

        ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);
        DialogueDetailPageViewModel viewModel = Assert.IsType<DialogueDetailPageViewModel>(viewResult.Model);
        Assert.Empty(viewModel.RelatedEventPreparationPacks);
        Assert.False(catalogApiClient.EventPreparationPacksWereRequested);
    }

    [Fact]
    public async Task Roleplay_ShouldSkipLearnerPromptsAndPairNonLearnerPromptsWithNextLearnerAnswer()
    {
        CapturingCatalogApiClient catalogApiClient = new(CreateDialogueWithRoleplaySequence());
        DialoguesController controller = new(
            catalogApiClient,
            new StaticLearningProfileAccessor(new UserLearningProfileModel("local", "en", "fa", "en")),
            new StaticFeatureAccessService("fa"),
            new TestStringLocalizer(),
            NullLogger<DialoguesController>.Instance);

        IActionResult actionResult = await controller.Roleplay("at-the-pharmacy", CancellationToken.None);

        ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);
        DialogueRoleplayPageViewModel viewModel = Assert.IsType<DialogueRoleplayPageViewModel>(viewResult.Model);
        Assert.Equal("en", catalogApiClient.PrimaryMeaningLanguageCode);
        Assert.Equal("fa", catalogApiClient.SecondaryMeaningLanguageCode);
        Assert.Equal(2, viewModel.Steps.Count);

        DialogueRoleplayStepViewModel firstStep = viewModel.Steps[0];
        Assert.Equal(1, firstStep.SortOrder);
        Assert.Equal("staff", firstStep.PromptRole);
        Assert.Equal("Wie kann ich Ihnen helfen?", firstStep.PromptText);
        Assert.Equal("learner", firstStep.LearnerRole);
        Assert.Equal("Ich brauche Hilfe.", firstStep.ModelAnswerText);

        DialogueRoleplayStepViewModel secondStep = viewModel.Steps[1];
        Assert.Equal(2, secondStep.SortOrder);
        Assert.Equal("pharmacist", secondStep.PromptRole);
        Assert.Equal("Haben Sie ein Rezept?", secondStep.PromptText);
        Assert.Equal("learner", secondStep.LearnerRole);
        Assert.Equal("Nein, ich habe kein Rezept.", secondStep.ModelAnswerText);
    }

    [Fact]
    public async Task Detail_ShouldReturnNotFound_WhenDialogueIsUnknown()
    {
        DialoguesController controller = new(
            new CapturingCatalogApiClient(null),
            new StaticLearningProfileAccessor(new UserLearningProfileModel("local", "en", "fa", "en")),
            new StaticFeatureAccessService("fa"),
            new TestStringLocalizer(),
            NullLogger<DialoguesController>.Instance);

        IActionResult actionResult = await controller.Detail("missing-dialogue", CancellationToken.None);

        Assert.IsType<NotFoundResult>(actionResult);
    }

    [Fact]
    public async Task Roleplay_ShouldReturnNotFound_WhenDialogueIsUnknown()
    {
        DialoguesController controller = new(
            new CapturingCatalogApiClient(null),
            new StaticLearningProfileAccessor(new UserLearningProfileModel("local", "en", "fa", "en")),
            new StaticFeatureAccessService("fa"),
            new TestStringLocalizer(),
            NullLogger<DialoguesController>.Instance);

        IActionResult actionResult = await controller.Roleplay("missing-dialogue", CancellationToken.None);

        Assert.IsType<NotFoundResult>(actionResult);
    }

    private static DialogueLessonDetailModel CreateDialogue() =>
        new(
            "at-the-pharmacy",
            "At the pharmacy",
            "Buy medicine and ask simple questions.",
            "Ask for help at a pharmacy.",
            "A1",
            "health",
            ["health"],
            ["goethe-a1"],
            ["speaking"],
            "ask-for-help",
            "service-counter",
            "formal",
            ["request"],
            10,
            null,
            null,
            [new DialogueUsefulWordModel("die Hilfe", null, "A1", 10)],
            [new DialogueSpeakingPromptModel("speaking-prompt", "Bitten Sie um Hilfe.", "Ask for help.", "کمک بخواهید.", 10)],
            [new DialogueTurnModel("learner", "Ich brauche Hilfe.", "I need help.", "من کمک لازم دارم.")],
            [new DialoguePhraseModel("Ich habe Kopfschmerzen.", "I have a headache.", "من سردرد دارم.", null)],
            [new DialogueQuestionModel("What do you need?", "What do you need?", "چه چیزی لازم داری؟", [new DialogueAnswerModel("Hilfe", "help", "کمک", true, null)])]);

    private static DialogueLessonDetailModel CreateDialogueWithRoleplaySequence() =>
        CreateDialogue() with
        {
            DialogueTurns =
            [
                new DialogueTurnModel("learner", "Guten Tag.", "Good day.", "روز بخیر."),
                new DialogueTurnModel("staff", "Wie kann ich Ihnen helfen?", "How can I help you?", "چطور می‌توانم کمک کنم؟"),
                new DialogueTurnModel("learner", "Ich brauche Hilfe.", "I need help.", "من کمک لازم دارم."),
                new DialogueTurnModel("pharmacist", "Haben Sie ein Rezept?", "Do you have a prescription?", "آیا نسخه دارید؟"),
                new DialogueTurnModel("learner", "Nein, ich habe kein Rezept.", "No, I do not have a prescription.", "نه، نسخه ندارم."),
                new DialogueTurnModel("staff", "Noch eine Frage?", "One more question?", "یک سؤال دیگر؟")
            ]
        };

    private sealed class CapturingCatalogApiClient(DialogueLessonDetailModel? dialogue) : UnsupportedWebCatalogApiClient
    {
        public string? Slug { get; private set; }

        public string? PrimaryMeaningLanguageCode { get; private set; }

        public string? SecondaryMeaningLanguageCode { get; private set; }

        public string? TargetLearningLanguageCode { get; private set; }

        public bool EventPreparationPacksWereRequested { get; private set; }

        public override Task<DialogueLessonDetailModel?> GetDialogueBySlugAsync(
            string slug,
            string targetLearningLanguageCode,
            string primaryMeaningLanguageCode,
            string? secondaryMeaningLanguageCode,
            CancellationToken cancellationToken)
        {
            Slug = slug;
            TargetLearningLanguageCode = targetLearningLanguageCode;
            PrimaryMeaningLanguageCode = primaryMeaningLanguageCode;
            SecondaryMeaningLanguageCode = secondaryMeaningLanguageCode;
            return Task.FromResult<DialogueLessonDetailModel?>(dialogue);
        }

        public override Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetConversationStarterPacksForDialogueAsync(string dialogueSlug, string targetLearningLanguageCode, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<ConversationStarterPackListItemModel>>([]);

        public override Task<IReadOnlyList<EventPreparationPackListItemModel>> GetEventPreparationPacksForDialogueAsync(string dialogueSlug, string targetLearningLanguageCode, string actorEmail, CancellationToken cancellationToken)
        {
            EventPreparationPacksWereRequested = true;
            return Task.FromResult<IReadOnlyList<EventPreparationPackListItemModel>>([]);
        }
    }

    private sealed class StaticLearningProfileAccessor(UserLearningProfileModel profile) : IWebLearningProfileAccessor
    {
        public Task<UserLearningProfileModel> GetProfileAsync(CancellationToken cancellationToken) => Task.FromResult(profile);
    }

    private sealed class StaticFeatureAccessService(
        string? resolvedSecondaryLanguage,
        bool canUseEventPreparationPacks = true) : IWebEntitledFeatureAccessService
    {
        public Task<bool> CanUseFavoritesAsync(CancellationToken cancellationToken) => Task.FromResult(true);

        public Task EnsureCanUseFavoritesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<bool> CanUseDualMeaningLanguageAsync(CancellationToken cancellationToken) => Task.FromResult(resolvedSecondaryLanguage is not null);

        public Task<bool> CanUseEventPreparationPacksAsync(CancellationToken cancellationToken) => Task.FromResult(canUseEventPreparationPacks);

        public Task EnsureCanUseEventPreparationPacksAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<string?> ResolveSecondaryMeaningLanguageAsync(string? requestedSecondaryMeaningLanguageCode, CancellationToken cancellationToken) =>
            Task.FromResult(resolvedSecondaryLanguage);
    }
}

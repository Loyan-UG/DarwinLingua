using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Web.Controllers;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class ScenariosControllerDualMeaningLanguageTests
{
    [Fact]
    public async Task Detail_ShouldPassResolvedMeaningLanguagesToApiAndViewModel()
    {
        CapturingCatalogApiClient catalogApiClient = new(CreateScenario());
        ScenariosController controller = new(
            catalogApiClient,
            new StaticLearningProfileAccessor(new UserLearningProfileModel("local", "en", "fa", "en")),
            new StaticFeatureAccessService("fa"));

        IActionResult actionResult = await controller.Detail("at-the-pharmacy", CancellationToken.None);

        ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);
        ScenarioDetailPageViewModel viewModel = Assert.IsType<ScenarioDetailPageViewModel>(viewResult.Model);
        Assert.Equal("at-the-pharmacy", catalogApiClient.Slug);
        Assert.Equal("en", catalogApiClient.PrimaryMeaningLanguageCode);
        Assert.Equal("fa", catalogApiClient.SecondaryMeaningLanguageCode);
        Assert.Equal("en", viewModel.PrimaryMeaningLanguageCode);
        Assert.Equal("fa", viewModel.SecondaryMeaningLanguageCode);
    }

    [Fact]
    public async Task Detail_ShouldOmitSecondaryMeaningLanguage_WhenFeatureIsUnavailable()
    {
        CapturingCatalogApiClient catalogApiClient = new(CreateScenario());
        ScenariosController controller = new(
            catalogApiClient,
            new StaticLearningProfileAccessor(new UserLearningProfileModel("local", "en", "fa", "en")),
            new StaticFeatureAccessService(null));

        IActionResult actionResult = await controller.Detail("at-the-pharmacy", CancellationToken.None);

        ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);
        ScenarioDetailPageViewModel viewModel = Assert.IsType<ScenarioDetailPageViewModel>(viewResult.Model);
        Assert.Equal("en", catalogApiClient.PrimaryMeaningLanguageCode);
        Assert.Null(catalogApiClient.SecondaryMeaningLanguageCode);
        Assert.Null(viewModel.SecondaryMeaningLanguageCode);
    }

    private static ScenarioLessonDetailModel CreateScenario() =>
        new(
            "at-the-pharmacy",
            "At the pharmacy",
            "Buy medicine and ask simple questions.",
            "Ask for help at a pharmacy.",
            "A1",
            "health",
            ["health"],
            [new ScenarioDialogueTurnModel("learner", "Ich brauche Hilfe.", "I need help.", "من کمک لازم دارم.")],
            [new ScenarioPhraseModel("Ich habe Kopfschmerzen.", "I have a headache.", "من سردرد دارم.", null)],
            [new ScenarioQuestionModel("What do you need?", "What do you need?", "چه چیزی لازم داری؟", [new ScenarioAnswerModel("Hilfe", "help", "کمک", true, null)])]);

    private sealed class CapturingCatalogApiClient(ScenarioLessonDetailModel scenario) : IWebCatalogApiClient
    {
        public string? Slug { get; private set; }

        public string? PrimaryMeaningLanguageCode { get; private set; }

        public string? SecondaryMeaningLanguageCode { get; private set; }

        public Task<IReadOnlyList<ScenarioLessonListItemModel>> GetScenariosAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<ScenarioLessonDetailModel?> GetScenarioBySlugAsync(
            string slug,
            string primaryMeaningLanguageCode,
            string? secondaryMeaningLanguageCode,
            CancellationToken cancellationToken)
        {
            Slug = slug;
            PrimaryMeaningLanguageCode = primaryMeaningLanguageCode;
            SecondaryMeaningLanguageCode = secondaryMeaningLanguageCode;
            return Task.FromResult<ScenarioLessonDetailModel?>(scenario);
        }

        public Task<IReadOnlyList<TopicListItemModel>> GetTopicsAsync(string uiLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<WordCollectionListItemModel>> GetCollectionsAsync(string meaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<WordCollectionDetailModel?> GetCollectionBySlugAsync(string slug, string meaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetConversationStarterPacksAsync(ConversationStarterListFilterModel filter, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetConversationStarterPacksForScenarioAsync(string scenarioSlug, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<ConversationStarterPackListItemModel>>([]);

        public Task<ConversationStarterPackDetailModel?> GetConversationStarterPackBySlugAsync(string slug, string primaryMeaningLanguageCode, string? secondaryMeaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<WordListItemModel>> GetWordsByTopicPageAsync(string topicKey, string meaningLanguageCode, int skip, int take, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<WordListItemModel>> GetWordsByCefrPageAsync(string cefrLevel, string meaningLanguageCode, int skip, int take, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<WordListItemModel>> SearchWordsAsync(string query, string meaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<WordDetailModel?> GetWordDetailsAsync(Guid publicId, string primaryMeaningLanguageCode, string? secondaryMeaningLanguageCode, string uiLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<WordListItemModel>> GetWordsByIdsAsync(IReadOnlyList<Guid> wordIds, string meaningLanguageCode, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<AdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<AdminImportsPageViewModel> GetAdminImportsAsync(string? statusFilter, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<AdminDraftWordsPageViewModel> GetAdminDraftWordsAsync(string? query, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<AdminHistoryPageViewModel> GetAdminHistoryAsync(string? statusFilter, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<AdminRollbackPageViewModel> GetAdminRollbackPreviewAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class StaticLearningProfileAccessor(UserLearningProfileModel profile) : IWebLearningProfileAccessor
    {
        public Task<UserLearningProfileModel> GetProfileAsync(CancellationToken cancellationToken) => Task.FromResult(profile);
    }

    private sealed class StaticFeatureAccessService(string? resolvedSecondaryLanguage) : IWebEntitledFeatureAccessService
    {
        public Task<bool> CanUseFavoritesAsync(CancellationToken cancellationToken) => Task.FromResult(true);

        public Task EnsureCanUseFavoritesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<bool> CanUseDualMeaningLanguageAsync(CancellationToken cancellationToken) => Task.FromResult(resolvedSecondaryLanguage is not null);

        public Task<string?> ResolveSecondaryMeaningLanguageAsync(string? requestedSecondaryMeaningLanguageCode, CancellationToken cancellationToken) =>
            Task.FromResult(resolvedSecondaryLanguage);
    }
}

using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Web.Controllers;
using DarwinLingua.Web.Data;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class SettingsControllerLanguagePreferenceTests
{
    [Fact]
    public async Task Update_ShouldPersistSecondaryMeaningLanguage_WhenDualMeaningFeatureIsAvailable()
    {
        CapturingUserPreferenceService preferenceService = new();
        SettingsController controller = CreateController(preferenceService, new StaticFeatureAccessService(canUseDualMeaningLanguage: true));

        IActionResult result = await controller.Update(
            new()
            {
                UiLanguageCode = "en",
                PrimaryMeaningLanguageCode = "fa",
                SecondaryMeaningLanguageCode = "en",
                AdultContentAccessState = AdultContentAccessStates.NotRequested
            },
            CancellationToken.None);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("fa", preferenceService.PrimaryMeaningLanguageCode);
        Assert.Equal("en", preferenceService.SecondaryMeaningLanguageCode);
    }

    [Fact]
    public async Task Update_ShouldOmitSecondaryMeaningLanguage_WhenDualMeaningFeatureIsUnavailable()
    {
        CapturingUserPreferenceService preferenceService = new();
        SettingsController controller = CreateController(preferenceService, new StaticFeatureAccessService(canUseDualMeaningLanguage: false));

        IActionResult result = await controller.Update(
            new()
            {
                UiLanguageCode = "en",
                PrimaryMeaningLanguageCode = "fa",
                SecondaryMeaningLanguageCode = "en",
                AdultContentAccessState = AdultContentAccessStates.NotRequested
            },
            CancellationToken.None);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("fa", preferenceService.PrimaryMeaningLanguageCode);
        Assert.Null(preferenceService.SecondaryMeaningLanguageCode);
    }

    [Fact]
    public void SettingsView_ShouldExplainWhereTheSecondaryMeaningLanguageAppears()
    {
        string viewSource = File.ReadAllText(ResolveRepositoryPath(
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Views",
            "Settings",
            "Index.cshtml"));

        Assert.Contains("It appears beside the primary helper language", viewSource, StringComparison.Ordinal);
        Assert.Contains("disabled=\"@(!Model.CanUseDualMeaningLanguage)\"", viewSource, StringComparison.Ordinal);
    }

    private static SettingsController CreateController(
        CapturingUserPreferenceService preferenceService,
        StaticFeatureAccessService featureAccessService)
    {
        DefaultHttpContext httpContext = new();

        return new SettingsController(
            preferenceService,
            featureAccessService,
            new TestStringLocalizer())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, new InMemoryTempDataProvider())
        };
    }

    private static string ResolveRepositoryPath(params string[] segments)
    {
        string? directory = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(directory))
        {
            string candidate = Path.Combine([directory, .. segments]);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new FileNotFoundException($"Could not resolve repository path for {string.Join(Path.DirectorySeparatorChar, segments)}.");
    }

    private sealed class CapturingUserPreferenceService : IWebUserPreferenceService
    {
        public string? UiLanguageCode { get; private set; }

        public string? PrimaryMeaningLanguageCode { get; private set; }

        public string? SecondaryMeaningLanguageCode { get; private set; }

        public Task<UserLearningProfileModel> GetProfileAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new UserLearningProfileModel("local", "en", null, "en"));

        public Task<UserLearningProfileModel> UpdatePreferencesAsync(
            string uiLanguageCode,
            string primaryMeaningLanguageCode,
            string? secondaryMeaningLanguageCode,
            bool allowsRudeSlangContent,
            string adultContentAccessState,
            CancellationToken cancellationToken)
        {
            UiLanguageCode = uiLanguageCode;
            PrimaryMeaningLanguageCode = primaryMeaningLanguageCode;
            SecondaryMeaningLanguageCode = secondaryMeaningLanguageCode;

            return Task.FromResult(new UserLearningProfileModel(
                "local",
                primaryMeaningLanguageCode,
                secondaryMeaningLanguageCode,
                uiLanguageCode,
                allowsRudeSlangContent,
                adultContentAccessState));
        }
    }

    private sealed class StaticFeatureAccessService(bool canUseDualMeaningLanguage) : IWebEntitledFeatureAccessService
    {
        public Task<bool> CanUseFavoritesAsync(CancellationToken cancellationToken) => Task.FromResult(true);

        public Task EnsureCanUseFavoritesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<bool> CanUseDualMeaningLanguageAsync(CancellationToken cancellationToken) =>
            Task.FromResult(canUseDualMeaningLanguage);

        public Task<bool> CanUseEventPreparationPacksAsync(CancellationToken cancellationToken) => Task.FromResult(true);

        public Task EnsureCanUseEventPreparationPacksAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<string?> ResolveSecondaryMeaningLanguageAsync(
            string? requestedSecondaryMeaningLanguageCode,
            CancellationToken cancellationToken) =>
            Task.FromResult(canUseDualMeaningLanguage ? requestedSecondaryMeaningLanguageCode : null);
    }

    private sealed class InMemoryTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) =>
            new Dictionary<string, object>(StringComparer.Ordinal);

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}

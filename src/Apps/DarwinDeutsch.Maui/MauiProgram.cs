using DarwinDeutsch.Maui.Pages;
using DarwinDeutsch.Maui.Services.Audio;
using DarwinDeutsch.Maui.Services.Browse;
using DarwinDeutsch.Maui.Services.Diagnostics;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinDeutsch.Maui.Services.Onboarding;
using DarwinDeutsch.Maui.Services.Storage;
using DarwinDeutsch.Maui.Services.Startup;
using DarwinDeutsch.Maui.Services.UI;
using DarwinDeutsch.Maui.Services.Updates;
#if ANDROID
using DarwinDeutsch.Maui.Platforms.Android.Updates;
#endif
using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.ContentOps.Application.DependencyInjection;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Learning.Application.DependencyInjection;
using DarwinLingua.Learning.Infrastructure.DependencyInjection;
using DarwinLingua.Localization.Application.DependencyInjection;
using DarwinLingua.Localization.Infrastructure.DependencyInjection;
using DarwinLingua.Practice.Application.DependencyInjection;
using DarwinLingua.Practice.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using System.Net.Security;

namespace DarwinDeutsch.Maui;

/// <summary>
/// Configures the MAUI application host and module registrations.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates and configures the MAUI application instance.
    /// </summary>
    /// <returns>The configured MAUI application.</returns>
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        string databasePath = Path.Combine(FileSystem.Current.AppDataDirectory, "darwin-lingua.db");
        RemoteContentUpdateOptions remoteContentUpdateOptions = CreateRemoteContentUpdateOptions();

        builder.Services
            .AddDarwinLinguaInfrastructure(options => options.DatabasePath = databasePath)
            .AddCatalogApplication()
            .AddCatalogInfrastructure()
            .AddContentOpsApplication()
            .AddContentOpsInfrastructure()
            .AddLearningApplication()
            .AddLearningInfrastructure()
            .AddLocalizationApplication()
            .AddLocalizationInfrastructure()
            .AddPracticeApplication()
            .AddPracticeInfrastructure()
            .AddSingleton<IPerformanceTelemetryService, PerformanceTelemetryService>()
            .AddSingleton<ISpeechPlaybackService, SpeechPlaybackService>()
            .AddSingleton<ICefrBrowseStateService, CefrBrowseStateService>()
            .AddSingleton<ITopicCatalogCacheService, TopicCatalogCacheService>()
            .AddSingleton<ITopicBrowseStateService, TopicBrowseStateService>()
            .AddSingleton<IWordDetailCacheService, WordDetailCacheService>()
            .AddSingleton<IWordSearchCacheService, WordSearchCacheService>()
            .AddSingleton<IBrowseAccelerationService, BrowseAccelerationService>()
            .AddSingleton<IAppLocalizationService, AppLocalizationService>()
            .AddSingleton<IActiveLearningProfileCacheService, ActiveLearningProfileCacheService>()
            .AddSingleton<IAppOnboardingService, AppOnboardingService>()
            .AddSingleton<IPopupDialogService, SyncfusionPopupDialogService>()
            .AddSingleton<ISeedDatabaseProvisioningService, SeedDatabaseProvisioningService>()
            .AddSingleton<IAppStartupInitializationService, AppStartupInitializationService>()
            .AddSingleton<IDeferredStartupMaintenanceService, DeferredStartupMaintenanceService>()
            .AddSingleton(remoteContentUpdateOptions)
            .AddSingleton(_ => CreateRemoteUpdateHttpClient(remoteContentUpdateOptions))
            .AddSingleton<IRemoteContentUpdateService, RemoteContentUpdateService>()
            .AddSingleton<IBackgroundRemoteUpdateCoordinator, BackgroundRemoteUpdateCoordinator>()
#if ANDROID
            .AddSingleton<IPlatformBackgroundUpdateScheduler, AndroidBackgroundUpdateScheduler>()
#else
            .AddSingleton<IPlatformBackgroundUpdateScheduler, NoOpPlatformBackgroundUpdateScheduler>()
#endif
            .AddSingleton<StartupPage>()
            .AddSingleton<AppShell>()
            .AddSingleton<WelcomePage>()
            .AddTransient<AboutPage>()
            .AddSingleton<HomePage>()
            .AddSingleton<PracticePage>()
            .AddTransient<PracticeSessionPage>()
            .AddSingleton<TopicsPage>()
            .AddSingleton<FavoritesPage>()
            .AddTransient<TopicWordsPage>()
            .AddTransient<CefrWordsPage>()
            .AddTransient<SearchWordsPage>()
            .AddTransient<WordDetailPage>()
            .AddSingleton<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static string GetDefaultRemoteContentBaseUrl()
    {
        return "https://gently-purifier-amnesty.ngrok-free.dev";
    }

    private static RemoteContentUpdateOptions CreateRemoteContentUpdateOptions()
    {
        return new RemoteContentUpdateOptions
        {
            BaseUrl = GetDefaultRemoteContentBaseUrl(),
            IgnoreTlsCertificateErrors = true,
            BrowserWarningBypassHeaderName = "ngrok-skip-browser-warning",
            BrowserWarningBypassHeaderValue = "1",
            ClientProductKey = "darwin-deutsch",
            ClientSchemaVersion = 1,
            StatusRequestTimeoutSeconds = 1,
            ManifestRequestTimeoutSeconds = 4,
        };
    }

    private static HttpClient CreateRemoteUpdateHttpClient(RemoteContentUpdateOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        string configuredHost = string.Empty;
        if (Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out Uri? baseUri))
        {
            configuredHost = baseUri.Host;
        }

        HttpClientHandler handler = new()
        {
            ServerCertificateCustomValidationCallback = (requestMessage, _, _, sslPolicyErrors) =>
            {
                if (sslPolicyErrors == SslPolicyErrors.None)
                {
                    return true;
                }

                return options.IgnoreTlsCertificateErrors &&
                       !string.IsNullOrWhiteSpace(configuredHost) &&
                       string.Equals(requestMessage?.RequestUri?.Host, configuredHost, StringComparison.OrdinalIgnoreCase);
            },
        };

        HttpClient client = new(handler)
        {
            Timeout = TimeSpan.FromSeconds(10),
        };

        if (!string.IsNullOrWhiteSpace(options.BrowserWarningBypassHeaderName))
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(
                options.BrowserWarningBypassHeaderName,
                string.IsNullOrWhiteSpace(options.BrowserWarningBypassHeaderValue) ? "1" : options.BrowserWarningBypassHeaderValue);
        }

        return client;
    }
}

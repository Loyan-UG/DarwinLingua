using DarwinDeutsch.Maui.Pages;
using DarwinDeutsch.Maui.Services.Audio;
using DarwinDeutsch.Maui.Services.Browse;
using DarwinDeutsch.Maui.Services.Diagnostics;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinDeutsch.Maui.Services.Onboarding;
using DarwinDeutsch.Maui.Services.Storage;
using DarwinDeutsch.Maui.Services.Startup;
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
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        string databasePath = Path.Combine(FileSystem.Current.AppDataDirectory, "darwin-lingua.db");

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
            .AddSingleton<IBrowseAccelerationService, BrowseAccelerationService>()
            .AddSingleton<IAppLocalizationService, AppLocalizationService>()
            .AddSingleton<IAppOnboardingService, AppOnboardingService>()
            .AddSingleton<ISeedDatabaseProvisioningService, SeedDatabaseProvisioningService>()
            .AddSingleton<IAppStartupInitializationService, AppStartupInitializationService>()
            .AddSingleton(new RemoteContentUpdateOptions
            {
                BaseUrl = GetDefaultRemoteContentBaseUrl(),
                ClientProductKey = "darwin-deutsch",
                ClientSchemaVersion = 1,
            })
            .AddSingleton(_ => new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10),
            })
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
#if ANDROID
        return "http://10.0.2.2:5099";
#else
        return "http://localhost:5099";
#endif
    }
}

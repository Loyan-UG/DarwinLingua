using DarwinDeutsch.Maui.Pages;
using DarwinDeutsch.Maui.Services.Audio;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinDeutsch.Maui.Services.Onboarding;
using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.ContentOps.Application.DependencyInjection;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
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
            .AddSingleton<ISpeechPlaybackService, SpeechPlaybackService>()
            .AddSingleton<IAppLocalizationService, AppLocalizationService>()
            .AddSingleton<IAppOnboardingService, AppOnboardingService>()
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

        MauiApp app = builder.Build();

        InitializeStartupState(app);

        return app;
    }

    /// <summary>
    /// Executes the application startup tasks that must complete before the first window opens.
    /// </summary>
    /// <param name="app">The built MAUI application.</param>
    private static void InitializeStartupState(MauiApp app)
    {
        ArgumentNullException.ThrowIfNull(app);

        IDatabaseInitializer databaseInitializer = app.Services.GetRequiredService<IDatabaseInitializer>();
        databaseInitializer.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

        IAppLocalizationService localizationService = app.Services.GetRequiredService<IAppLocalizationService>();
        localizationService.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}

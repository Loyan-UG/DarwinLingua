using DarwinDeutsch.Maui.Pages;
using DarwinDeutsch.Maui.Services.Onboarding;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinDeutsch.Maui;

/// <summary>
/// Represents the MAUI application entry point.
/// </summary>
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppOnboardingService _appOnboardingService;
    private AppShell? _appShell;
    private StartupPage? _startupPage;
    private WelcomePage? _welcomePage;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="serviceProvider">The root service provider used to resolve pages lazily.</param>
    /// <param name="appOnboardingService">The service that tracks onboarding completion.</param>
    public App(
        IServiceProvider serviceProvider,
        IAppOnboardingService appOnboardingService)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(appOnboardingService);

        InitializeComponent();

        _serviceProvider = serviceProvider;
        _appOnboardingService = appOnboardingService;
    }

    /// <summary>
    /// Creates the main application window.
    /// </summary>
    /// <param name="activationState">The optional platform activation state.</param>
    /// <returns>The configured application window.</returns>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        StartupPage startupPage = GetStartupPage();
        return new Window(startupPage);
    }

    private void OnStartupCompleted(object? sender, EventArgs e)
    {
        Window? activeWindow = Windows.FirstOrDefault();
        if (activeWindow is null)
        {
            return;
        }

        activeWindow.Page = _appOnboardingService.ShouldShowWelcomeExperience()
            ? GetWelcomePage()
            : GetAppShell();
    }

    private void OnWelcomeStartRequested(object? sender, EventArgs e)
    {
        _appOnboardingService.MarkWelcomeExperienceCompleted();

        Window? activeWindow = Windows.FirstOrDefault();
        if (activeWindow is not null)
        {
            activeWindow.Page = GetAppShell();
        }
    }

    private StartupPage GetStartupPage()
    {
        if (_startupPage is not null)
        {
            return _startupPage;
        }

        _startupPage = _serviceProvider.GetRequiredService<StartupPage>();
        _startupPage.StartupCompleted += OnStartupCompleted;
        return _startupPage;
    }

    private AppShell GetAppShell()
    {
        return _appShell ??= _serviceProvider.GetRequiredService<AppShell>();
    }

    private WelcomePage GetWelcomePage()
    {
        if (_welcomePage is not null)
        {
            return _welcomePage;
        }

        _welcomePage = _serviceProvider.GetRequiredService<WelcomePage>();
        _welcomePage.StartRequested += OnWelcomeStartRequested;
        return _welcomePage;
    }
}

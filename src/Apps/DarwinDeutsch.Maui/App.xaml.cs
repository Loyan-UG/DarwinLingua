using DarwinDeutsch.Maui.Pages;
using DarwinDeutsch.Maui.Services.Onboarding;

namespace DarwinDeutsch.Maui;

/// <summary>
/// Represents the MAUI application entry point.
/// </summary>
public partial class App : Application
{
    private readonly AppShell _appShell;
    private readonly WelcomePage _welcomePage;
    private readonly IAppOnboardingService _appOnboardingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="appShell">The application shell resolved from dependency injection.</param>
    public App(
        AppShell appShell,
        WelcomePage welcomePage,
        IAppOnboardingService appOnboardingService)
    {
        ArgumentNullException.ThrowIfNull(appShell);
        ArgumentNullException.ThrowIfNull(welcomePage);
        ArgumentNullException.ThrowIfNull(appOnboardingService);

        InitializeComponent();

        _appShell = appShell;
        _welcomePage = welcomePage;
        _appOnboardingService = appOnboardingService;

        _welcomePage.StartRequested += OnWelcomeStartRequested;
    }

    /// <summary>
    /// Creates the main application window.
    /// </summary>
    /// <param name="activationState">The optional platform activation state.</param>
    /// <returns>The configured application window.</returns>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        Page startupPage = _appOnboardingService.ShouldShowWelcomeExperience()
            ? _welcomePage
            : _appShell;

        return new Window(startupPage);
    }

    private void OnWelcomeStartRequested(object? sender, EventArgs e)
    {
        _appOnboardingService.MarkWelcomeExperienceCompleted();

        Window? activeWindow = Windows.FirstOrDefault();
        if (activeWindow is not null)
        {
            activeWindow.Page = _appShell;
        }
    }
}

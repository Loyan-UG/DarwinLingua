using DarwinDeutsch.Maui.Resources.Strings;

namespace DarwinDeutsch.Maui.Pages;

public partial class AboutPage : ContentPage
{
    private const string GitHubUrl = "https://github.com/shahramvafadar/DarwinLingua";
    private const string ContactEmail = "shahram@vafadar.pro";

    public AboutPage()
    {
        InitializeComponent();
        ApplyLocalizedText();
    }

    private void ApplyLocalizedText()
    {
        Title = AppStrings.AboutTitle;
        HeadlineLabel.Text = AppStrings.AboutHeadline;
        SummaryLabel.Text = AppStrings.AboutSummary;
        FeaturesSectionLabel.Text = AppStrings.AboutFeaturesTitle;
        FeaturesBodyLabel.Text = AppStrings.AboutFeaturesBody;
        DeveloperSectionLabel.Text = AppStrings.AboutDeveloperTitle;
        DeveloperValueLabel.Text = AppStrings.AboutDeveloperValue;
        OpenGitHubButton.Text = AppStrings.AboutGitHubButton;
        ContactButton.Text = string.Format(AppStrings.AboutContactButtonFormat, ContactEmail);
    }

    private async void OnOpenGitHubButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            await Launcher.Default.OpenAsync(new Uri(GitHubUrl)).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async void OnContactButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            await Launcher.Default.OpenAsync(new Uri($"mailto:{ContactEmail}")).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }
}

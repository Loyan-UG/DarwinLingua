using DarwinDeutsch.Maui.Resources.Strings;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Provides the mobile Learning Portal navigation hub.
/// </summary>
public partial class LearningPortalPage : ContentPage
{
    private readonly IReadOnlyList<LearningPortalGroup> _groups =
    [
        new(
            () => AppStrings.LearningPortalGroupLearn,
            [
                new(() => AppStrings.LearningPortalItemWords, nameof(TopicsPage)),
                new(() => AppStrings.LearningPortalItemGrammar, $"{nameof(LearningPortalListPage)}?module=grammar"),
                new(() => AppStrings.LearningPortalItemExpressions, $"{nameof(LearningPortalListPage)}?module=expressions"),
                new(() => AppStrings.LearningPortalItemCourses, $"{nameof(LearningPortalListPage)}?module=courses"),
            ]),
        new(
            () => AppStrings.LearningPortalGroupPractice,
            [
                new(() => AppStrings.LearningPortalItemPracticeReview, "practice"),
                new(() => AppStrings.LearningPortalItemExercises, $"{nameof(LearningPortalListPage)}?module=exercises"),
            ]),
        new(
            () => AppStrings.LearningPortalGroupSpeak,
            [
                new(() => AppStrings.LearningPortalItemDialogues, nameof(DialoguesPage)),
                new(() => AppStrings.LearningPortalItemTalkTopics, $"{nameof(LearningPortalListPage)}?module=talk-topics"),
                new(() => AppStrings.LearningPortalItemConversationStarters, nameof(ConversationStartersPage)),
            ]),
        new(
            () => AppStrings.LearningPortalGroupPrepare,
            [
                new(() => AppStrings.LearningPortalItemExamPrep, $"{nameof(LearningPortalListPage)}?module=exam-prep"),
                new(() => AppStrings.LearningPortalItemWritingTemplates, $"{nameof(LearningPortalListPage)}?module=writing-templates"),
            ]),
        new(
            () => AppStrings.LearningPortalGroupResources,
            [
                new(() => AppStrings.LearningPortalItemCulturalNotes, $"{nameof(LearningPortalListPage)}?module=cultural-notes"),
                new(() => AppStrings.LearningPortalItemSearch, nameof(LearningPortalSearchPage)),
                new(() => AppStrings.LearningPortalItemSettings, nameof(SettingsPage)),
            ]),
    ];

    public LearningPortalPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyLocalizedText();
    }

    private void ApplyLocalizedText()
    {
        Title = AppStrings.LearningPortalTitle;
        HeadlineLabel.Text = AppStrings.LearningPortalHeadline;
        DescriptionLabel.Text = AppStrings.LearningPortalDescription;
        GroupsLayout.Clear();

        foreach (LearningPortalGroup group in _groups)
        {
            VerticalStackLayout section = new() { Spacing = 8 };
            section.Add(new Label
            {
                Text = group.Title(),
                Style = (Style)Application.Current!.Resources["Title2"],
            });

            foreach (LearningPortalItem item in group.Items)
            {
                Button button = new()
                {
                    Text = item.Title(),
                    HorizontalOptions = LayoutOptions.Fill,
                };
                button.Clicked += async (_, _) => await NavigateAsync(item.Route).ConfigureAwait(true);
                section.Add(button);
            }

            Border border = new()
            {
                Padding = 16,
                Style = (Style)Application.Current!.Resources["AppSectionCardBorderStyle"],
                Content = section,
            };
            GroupsLayout.Add(border);
        }
    }

    private static async Task NavigateAsync(string route)
    {
        if (string.Equals(route, "practice", StringComparison.OrdinalIgnoreCase))
        {
            await Shell.Current.GoToAsync("//practice").ConfigureAwait(true);
            return;
        }

        await Shell.Current.GoToAsync(route).ConfigureAwait(true);
    }

    private sealed record LearningPortalGroup(Func<string> Title, IReadOnlyList<LearningPortalItem> Items);

    private sealed record LearningPortalItem(Func<string> Title, string Route);
}

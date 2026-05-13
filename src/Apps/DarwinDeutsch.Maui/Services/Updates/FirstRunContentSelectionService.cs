using DarwinDeutsch.Maui.Resources.Strings;

namespace DarwinDeutsch.Maui.Services.Updates;

public interface IFirstRunContentSelectionService
{
    Task EnsureCompletedAsync(Page hostPage, CancellationToken cancellationToken);
}

internal sealed class FirstRunContentSelectionService(IRemoteContentUpdateService remoteContentUpdateService) : IFirstRunContentSelectionService
{
    private const string SelectionCompletedPreferenceKey = "mobile-content-selection-completed-v1";
    private const string SelectedModulesPreferenceKey = "mobile-content-selected-modules-v1";

    private static readonly ContentModuleOption[] ModuleOptions =
    [
        new("words", "Words"),
        new("grammar", "Grammar Guide"),
        new("expressions", "Everyday Expressions"),
        new("dialogues", "Dialogues"),
        new("talk-topics", "Talk Topics"),
        new("exercises", "Exercises"),
        new("courses", "Courses"),
        new("exam-prep", "Exam Preparation"),
        new("writing-templates", "Writing Templates"),
        new("cultural-notes", "Cultural Notes"),
        new("events", "Events"),
        new("conversation-starters", "Conversation Starters"),
    ];

    public async Task EnsureCompletedAsync(Page hostPage, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(hostPage);

        if (Preferences.Default.Get(SelectionCompletedPreferenceKey, false))
        {
            return;
        }

        bool wantsContent = await hostPage.DisplayAlertAsync(
                AppStrings.FirstRunContentSelectionTitle,
                AppStrings.FirstRunContentSelectionMessage,
                AppStrings.FirstRunContentSelectionChooseButton,
                AppStrings.FirstRunContentSelectionSkipButton)
            .ConfigureAwait(true);

        if (!wantsContent)
        {
            MarkCompleted([]);
            return;
        }

        List<ContentModuleOption> selectedModules = [];
        while (true)
        {
            string cancelText = selectedModules.Count == 0
                ? AppStrings.FirstRunContentSelectionSkipButton
                : AppStrings.FirstRunContentSelectionDoneButton;
            string? selectedDisplayName = await hostPage.DisplayActionSheetAsync(
                    AppStrings.FirstRunContentSelectionActionTitle,
                    cancelText,
                    null,
                    ModuleOptions
                        .Where(option => selectedModules.All(selected => selected.Key != option.Key))
                        .Select(option => option.DisplayName)
                        .ToArray())
                .ConfigureAwait(true);

            if (string.IsNullOrWhiteSpace(selectedDisplayName) ||
                string.Equals(selectedDisplayName, cancelText, StringComparison.Ordinal))
            {
                break;
            }

            ContentModuleOption? selected = ModuleOptions.FirstOrDefault(option => option.DisplayName == selectedDisplayName);
            if (selected is not null)
            {
                selectedModules.Add(selected);
            }

            if (selectedModules.Count == ModuleOptions.Length)
            {
                break;
            }
        }

        if (selectedModules.Count == 0)
        {
            MarkCompleted([]);
            return;
        }

        await hostPage.DisplayAlertAsync(
                AppStrings.FirstRunContentSelectionDownloadTitle,
                string.Format(AppStrings.FirstRunContentSelectionDownloadMessageFormat, selectedModules.Count),
                AppStrings.SettingsContentUpdatesDismissButton)
            .ConfigureAwait(true);

        string databasePath = Path.Combine(FileSystem.Current.AppDataDirectory, "darwin-lingua.db");
        List<string> failedModules = [];

        foreach (ContentModuleOption module in selectedModules)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RemoteContentUpdateResult result = await remoteContentUpdateService
                .ApplyModuleUpdateAsync(databasePath, module.Key, cancellationToken)
                .ConfigureAwait(true);

            if (!result.IsSuccess)
            {
                failedModules.Add(module.DisplayName);
            }
        }

        MarkCompleted(selectedModules.Select(module => module.Key));

        if (failedModules.Count > 0)
        {
            await hostPage.DisplayAlertAsync(
                    AppStrings.FirstRunContentSelectionPartialFailureTitle,
                    string.Format(AppStrings.FirstRunContentSelectionPartialFailureMessageFormat, string.Join(", ", failedModules)),
                    AppStrings.SettingsContentUpdatesDismissButton)
                .ConfigureAwait(true);
        }
    }

    private static void MarkCompleted(IEnumerable<string> selectedModuleKeys)
    {
        Preferences.Default.Set(SelectionCompletedPreferenceKey, true);
        Preferences.Default.Set(SelectedModulesPreferenceKey, string.Join(",", selectedModuleKeys.OrderBy(key => key, StringComparer.Ordinal)));
    }

    private sealed record ContentModuleOption(string Key, string DisplayName);
}

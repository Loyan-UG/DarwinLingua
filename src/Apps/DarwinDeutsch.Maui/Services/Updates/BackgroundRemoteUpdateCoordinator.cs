using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Browse;
using Microsoft.Extensions.Logging;

namespace DarwinDeutsch.Maui.Services.Updates;

/// <summary>
/// Runs lightweight remote-update checks after startup and when the app resumes.
/// </summary>
internal sealed class BackgroundRemoteUpdateCoordinator : IBackgroundRemoteUpdateCoordinator
{
    private const string LastBackgroundCheckAtPreferenceKey = "remote-content-background-last-check-at-utc";
    private const string LastPromptedRemotePackageIdPreferenceKey = "remote-content-background-last-prompted-package-id";
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan ResumeDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan MinimumCheckInterval = TimeSpan.FromMinutes(30);

    private readonly IRemoteContentUpdateService _remoteContentUpdateService;
    private readonly IBrowseAccelerationService _browseAccelerationService;
    private readonly ILogger<BackgroundRemoteUpdateCoordinator> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundRemoteUpdateCoordinator"/> class.
    /// </summary>
    public BackgroundRemoteUpdateCoordinator(
        IRemoteContentUpdateService remoteContentUpdateService,
        IBrowseAccelerationService browseAccelerationService,
        ILogger<BackgroundRemoteUpdateCoordinator> logger)
    {
        ArgumentNullException.ThrowIfNull(remoteContentUpdateService);
        ArgumentNullException.ThrowIfNull(browseAccelerationService);
        ArgumentNullException.ThrowIfNull(logger);

        _remoteContentUpdateService = remoteContentUpdateService;
        _browseAccelerationService = browseAccelerationService;
        _logger = logger;
    }

    /// <inheritdoc />
    public void ScheduleInitialCheck(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        _ = RunDeferredCheckAsync(window, InitialDelay, force: true);
    }

    /// <inheritdoc />
    public void ScheduleResumeCheck(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        _ = RunDeferredCheckAsync(window, ResumeDelay, force: false);
    }

    private async Task RunDeferredCheckAsync(Window window, TimeSpan delay, bool force)
    {
        try
        {
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay).ConfigureAwait(false);
            }

            if (!force && !ShouldRunCheckNow())
            {
                return;
            }

            if (!await _gate.WaitAsync(0).ConfigureAwait(false))
            {
                return;
            }

            try
            {
                if (!force && !ShouldRunCheckNow())
                {
                    return;
                }

                string databasePath = Path.Combine(FileSystem.Current.AppDataDirectory, "darwin-lingua.db");
                RemoteContentUpdateStatus status = await _remoteContentUpdateService
                    .GetUpdateStatusAsync(databasePath, CancellationToken.None)
                    .ConfigureAwait(false);

                PersistLastBackgroundCheckAt(DateTimeOffset.UtcNow);

                if (!status.IsRemoteConfigured || !status.IsServerReachable || !status.IsUpdateAvailable)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(status.RemotePackageId))
                {
                    return;
                }

                string lastPromptedPackageId = Preferences.Default.Get(LastPromptedRemotePackageIdPreferenceKey, string.Empty);
                if (string.Equals(lastPromptedPackageId, status.RemotePackageId, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                await PromptAndApplyUpdateAsync(window, databasePath, status).ConfigureAwait(false);
            }
            finally
            {
                _gate.Release();
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Background remote update check failed.");
        }
    }

    private async Task PromptAndApplyUpdateAsync(Window window, string databasePath, RemoteContentUpdateStatus status)
    {
        Page? promptPage = await MainThread.InvokeOnMainThreadAsync(() => ResolvePromptPage(window)).ConfigureAwait(false);
        if (promptPage is null)
        {
            return;
        }

        bool shouldApplyNow = await MainThread.InvokeOnMainThreadAsync(() => promptPage.DisplayAlertAsync(
                AppStrings.BackgroundRemoteUpdatePromptTitle,
                string.Format(
                    AppStrings.BackgroundRemoteUpdatePromptMessageFormat,
                    string.IsNullOrWhiteSpace(status.RemoteVersion)
                        ? AppStrings.SettingsContentUpdatesUnknownSignatureValue
                        : status.RemoteVersion,
                    status.PendingWordCount),
                AppStrings.BackgroundRemoteUpdateApplyNowButton,
                AppStrings.BackgroundRemoteUpdateLaterButton))
            .ConfigureAwait(false);

        if (!shouldApplyNow)
        {
            Preferences.Default.Set(LastPromptedRemotePackageIdPreferenceKey, status.RemotePackageId);
            return;
        }

        RemoteContentUpdateResult result = await _remoteContentUpdateService
            .ApplyFullUpdateAsync(databasePath, CancellationToken.None)
            .ConfigureAwait(false);

        if (result.IsSuccess && result.AppliedChanges)
        {
            Preferences.Default.Set(LastPromptedRemotePackageIdPreferenceKey, status.RemotePackageId);
            _browseAccelerationService.ResetCaches();
        }
        else if (!result.IsSuccess)
        {
            Preferences.Default.Remove(LastPromptedRemotePackageIdPreferenceKey);
        }

        await MainThread.InvokeOnMainThreadAsync(() => ShowApplyResultAsync(promptPage, result)).ConfigureAwait(false);
    }

    private static async Task ShowApplyResultAsync(Page promptPage, RemoteContentUpdateResult result)
    {
        if (!result.IsSuccess)
        {
            await promptPage.DisplayAlertAsync(
                    AppStrings.SettingsRemoteContentUpdatesFailedTitle,
                    string.Format(
                        AppStrings.SettingsRemoteContentUpdatesFailedMessageFormat,
                        result.ErrorMessage ?? AppStrings.SettingsRemoteContentUpdatesUnavailableStatus),
                    AppStrings.SettingsContentUpdatesDismissButton)
                .ConfigureAwait(true);
            return;
        }

        if (result.AppliedChanges)
        {
            await promptPage.DisplayAlertAsync(
                    AppStrings.SettingsRemoteContentUpdatesCompletedTitle,
                    string.Format(AppStrings.SettingsRemoteContentUpdatesCompletedMessageFormat, result.AppliedVersion, result.ImportedWords),
                    AppStrings.SettingsContentUpdatesDismissButton)
                .ConfigureAwait(true);
            return;
        }

        await promptPage.DisplayAlertAsync(
                AppStrings.SettingsRemoteContentUpdatesUpToDateTitle,
                AppStrings.SettingsRemoteContentUpdatesUpToDateMessage,
                AppStrings.SettingsContentUpdatesDismissButton)
            .ConfigureAwait(true);
    }

    private static Page? ResolvePromptPage(Window window)
    {
        if (window.Page is not Shell shell)
        {
            return null;
        }

        return shell.CurrentPage;
    }

    private static bool ShouldRunCheckNow()
    {
        string? rawValue = Preferences.Default.Get<string?>(LastBackgroundCheckAtPreferenceKey, null);
        if (!DateTimeOffset.TryParse(rawValue, out DateTimeOffset lastCheckedAtUtc))
        {
            return true;
        }

        return DateTimeOffset.UtcNow - lastCheckedAtUtc >= MinimumCheckInterval;
    }

    private static void PersistLastBackgroundCheckAt(DateTimeOffset occurredAtUtc)
    {
        Preferences.Default.Set(LastBackgroundCheckAtPreferenceKey, occurredAtUtc.ToString("O"));
    }
}

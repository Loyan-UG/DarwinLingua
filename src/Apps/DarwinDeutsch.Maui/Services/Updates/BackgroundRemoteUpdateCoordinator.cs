using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Pages;
using DarwinDeutsch.Maui.Services.Browse;
using DarwinDeutsch.Maui.Services.UI;
using Microsoft.Extensions.Logging;

namespace DarwinDeutsch.Maui.Services.Updates;

/// <summary>
/// Runs lightweight remote-update checks after startup and when the app resumes.
/// </summary>
internal sealed class BackgroundRemoteUpdateCoordinator : IBackgroundRemoteUpdateCoordinator
{
    private const string LastBackgroundCheckAtPreferenceKey = "remote-content-background-last-check-at-utc";
    private const string LastPromptedRemotePackageIdPreferenceKey = "remote-content-background-last-prompted-package-id";
    private const string LastBackgroundFailureAtPreferenceKey = "remote-content-background-last-failure-at-utc";
    private const string ConsecutiveBackgroundFailureCountPreferenceKey = "remote-content-background-consecutive-failure-count";
    private const string LastFailedApplyPackageIdPreferenceKey = "remote-content-background-last-failed-apply-package-id";
    private const string LastFailedApplyAtPreferenceKey = "remote-content-background-last-failed-apply-at-utc";
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan ResumeDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan PendingCheckCoalescingWindow = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ResumeAfterStartupSuppressionWindow = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan MinimumCheckInterval = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan FailedApplyPromptCooldown = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan[] FailureBackoffSchedule =
    [
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(10),
        TimeSpan.FromMinutes(20)
    ];

    private readonly IRemoteContentUpdateService _remoteContentUpdateService;
    private readonly IBrowseAccelerationService _browseAccelerationService;
    private readonly IPopupDialogService _popupDialogService;
    private readonly ILogger<BackgroundRemoteUpdateCoordinator> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly object _scheduleStateLock = new();

    private ScheduledCheckState? _scheduledCheck;
    private DateTimeOffset? _lastInitialCheckScheduledAtUtc;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundRemoteUpdateCoordinator"/> class.
    /// </summary>
    public BackgroundRemoteUpdateCoordinator(
        IRemoteContentUpdateService remoteContentUpdateService,
        IBrowseAccelerationService browseAccelerationService,
        IPopupDialogService popupDialogService,
        ILogger<BackgroundRemoteUpdateCoordinator> logger)
    {
        ArgumentNullException.ThrowIfNull(remoteContentUpdateService);
        ArgumentNullException.ThrowIfNull(browseAccelerationService);
        ArgumentNullException.ThrowIfNull(popupDialogService);
        ArgumentNullException.ThrowIfNull(logger);

        _remoteContentUpdateService = remoteContentUpdateService;
        _browseAccelerationService = browseAccelerationService;
        _popupDialogService = popupDialogService;
        _logger = logger;
    }

    /// <inheritdoc />
    public void ScheduleInitialCheck(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        ScheduleCheck(window, InitialDelay, force: false, CheckScheduleReason.Initial);
    }

    /// <inheritdoc />
    public void ScheduleResumeCheck(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        ScheduleCheck(window, ResumeDelay, force: false, CheckScheduleReason.Resume);
    }

    private void ScheduleCheck(Window window, TimeSpan delay, bool force, CheckScheduleReason reason)
    {
        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        DateTimeOffset dueAtUtc = nowUtc.Add(delay);
        long scheduledCheckId;

        lock (_scheduleStateLock)
        {
            if (reason is CheckScheduleReason.Resume &&
                _lastInitialCheckScheduledAtUtc is DateTimeOffset lastInitialCheckScheduledAtUtc &&
                nowUtc - lastInitialCheckScheduledAtUtc < ResumeAfterStartupSuppressionWindow)
            {
                return;
            }

            if (_scheduledCheck is { } scheduledCheck &&
                scheduledCheck.DueAtUtc <= dueAtUtc.Add(PendingCheckCoalescingWindow))
            {
                return;
            }

            scheduledCheckId = (_scheduledCheck?.Id ?? 0L) + 1L;
            _scheduledCheck = new ScheduledCheckState(scheduledCheckId, dueAtUtc);

            if (reason is CheckScheduleReason.Initial)
            {
                _lastInitialCheckScheduledAtUtc = nowUtc;
            }
        }

        _ = RunDeferredCheckAsync(window, delay, force, scheduledCheckId);
    }

    private async Task RunDeferredCheckAsync(Window window, TimeSpan delay, bool force, long scheduledCheckId)
    {
        try
        {
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay).ConfigureAwait(false);
            }

            ClearScheduledCheck(scheduledCheckId);

            if (!HasInternetConnectivity())
            {
                return;
            }

            if (!force && IsWithinFailureBackoffWindow())
            {
                return;
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

                if (!HasInternetConnectivity())
                {
                    return;
                }

                string databasePath = Path.Combine(FileSystem.Current.AppDataDirectory, "darwin-lingua.db");
                RemoteContentUpdateStatus status = await _remoteContentUpdateService
                    .GetUpdateStatusAsync(databasePath, CancellationToken.None)
                    .ConfigureAwait(false);

                PersistLastBackgroundCheckAt(DateTimeOffset.UtcNow);
                ResetBackgroundFailureState();

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

                if (IsWithinFailedApplyCooldown(status.RemotePackageId))
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
            ClearScheduledCheck(scheduledCheckId);
            PersistBackgroundFailure(DateTimeOffset.UtcNow);
            _logger.LogWarning(exception, "Background remote update check failed.");
        }
    }

    private void ClearScheduledCheck(long scheduledCheckId)
    {
        lock (_scheduleStateLock)
        {
            if (_scheduledCheck?.Id == scheduledCheckId)
            {
                _scheduledCheck = null;
            }
        }
    }

    private async Task PromptAndApplyUpdateAsync(Window window, string databasePath, RemoteContentUpdateStatus status)
    {
        Page? promptPage = await MainThread.InvokeOnMainThreadAsync(() => ResolvePromptPage(window)).ConfigureAwait(false);
        if (promptPage is null || !CanShowPrompt(promptPage) || IsMeteredConnection())
        {
            return;
        }

        bool shouldApplyNow = await _popupDialogService
            .ShowConfirmationAsync(
                AppStrings.BackgroundRemoteUpdatePromptTitle,
                string.Format(
                    AppStrings.BackgroundRemoteUpdatePromptMessageFormat,
                    string.IsNullOrWhiteSpace(status.RemoteVersion)
                        ? AppStrings.SettingsContentUpdatesUnknownSignatureValue
                        : status.RemoteVersion,
                    status.PendingWordCount),
                AppStrings.BackgroundRemoteUpdateApplyNowButton,
                AppStrings.BackgroundRemoteUpdateLaterButton)
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
            ClearFailedApplyState();
            _browseAccelerationService.ResetCaches();
        }
        else if (result.IsSuccess)
        {
            ClearFailedApplyState();
        }
        else if (!result.IsSuccess)
        {
            Preferences.Default.Remove(LastPromptedRemotePackageIdPreferenceKey);
            PersistFailedApply(status.RemotePackageId, DateTimeOffset.UtcNow);
        }

        await ShowApplyResultAsync(result).ConfigureAwait(false);
    }

    private async Task ShowApplyResultAsync(RemoteContentUpdateResult result)
    {
        if (!result.IsSuccess)
        {
            await _popupDialogService.ShowMessageAsync(
                    AppStrings.SettingsRemoteContentUpdatesFailedTitle,
                    string.Format(
                        AppStrings.SettingsRemoteContentUpdatesFailedMessageFormat,
                        result.ErrorMessage ?? AppStrings.SettingsRemoteContentUpdatesUnavailableStatus),
                    AppStrings.SettingsContentUpdatesDismissButton)
                .ConfigureAwait(false);
            return;
        }

        if (result.AppliedChanges)
        {
            await _popupDialogService.ShowMessageAsync(
                    AppStrings.SettingsRemoteContentUpdatesCompletedTitle,
                    string.Format(AppStrings.SettingsRemoteContentUpdatesCompletedMessageFormat, result.AppliedVersion, result.ImportedWords),
                    AppStrings.SettingsContentUpdatesDismissButton)
                .ConfigureAwait(false);
            return;
        }

        await _popupDialogService.ShowMessageAsync(
                AppStrings.SettingsRemoteContentUpdatesUpToDateTitle,
                AppStrings.SettingsRemoteContentUpdatesUpToDateMessage,
                AppStrings.SettingsContentUpdatesDismissButton)
            .ConfigureAwait(false);
    }

    private static Page? ResolvePromptPage(Window window)
    {
        if (window.Page is not Shell shell)
        {
            return null;
        }

        return shell.CurrentPage;
    }

    private static bool CanShowPrompt(Page promptPage) =>
        promptPage is not StartupPage &&
        promptPage is not WelcomePage &&
        promptPage is not SettingsPage;

    private static bool HasInternetConnectivity() =>
        Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    private static bool IsMeteredConnection() =>
        Connectivity.Current.ConnectionProfiles.Contains(ConnectionProfile.Cellular);

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

    private static bool IsWithinFailureBackoffWindow()
    {
        int consecutiveFailures = Preferences.Default.Get(ConsecutiveBackgroundFailureCountPreferenceKey, 0);
        if (consecutiveFailures <= 0)
        {
            return false;
        }

        string? rawValue = Preferences.Default.Get<string?>(LastBackgroundFailureAtPreferenceKey, null);
        if (!DateTimeOffset.TryParse(rawValue, out DateTimeOffset lastFailureAtUtc))
        {
            return false;
        }

        int backoffIndex = Math.Min(consecutiveFailures, FailureBackoffSchedule.Length) - 1;
        TimeSpan cooldown = FailureBackoffSchedule[backoffIndex];
        return DateTimeOffset.UtcNow - lastFailureAtUtc < cooldown;
    }

    private static void PersistBackgroundFailure(DateTimeOffset occurredAtUtc)
    {
        Preferences.Default.Set(LastBackgroundFailureAtPreferenceKey, occurredAtUtc.ToString("O"));
        int consecutiveFailures = Preferences.Default.Get(ConsecutiveBackgroundFailureCountPreferenceKey, 0);
        Preferences.Default.Set(ConsecutiveBackgroundFailureCountPreferenceKey, consecutiveFailures + 1);
    }

    private static void ResetBackgroundFailureState()
    {
        Preferences.Default.Remove(LastBackgroundFailureAtPreferenceKey);
        Preferences.Default.Remove(ConsecutiveBackgroundFailureCountPreferenceKey);
    }

    private static bool IsWithinFailedApplyCooldown(string remotePackageId)
    {
        if (string.IsNullOrWhiteSpace(remotePackageId))
        {
            return false;
        }

        string failedPackageId = Preferences.Default.Get(LastFailedApplyPackageIdPreferenceKey, string.Empty);
        if (!string.Equals(failedPackageId, remotePackageId, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string? rawValue = Preferences.Default.Get<string?>(LastFailedApplyAtPreferenceKey, null);
        return DateTimeOffset.TryParse(rawValue, out DateTimeOffset lastFailedApplyAtUtc) &&
               DateTimeOffset.UtcNow - lastFailedApplyAtUtc < FailedApplyPromptCooldown;
    }

    private static void PersistFailedApply(string remotePackageId, DateTimeOffset occurredAtUtc)
    {
        Preferences.Default.Set(LastFailedApplyPackageIdPreferenceKey, remotePackageId);
        Preferences.Default.Set(LastFailedApplyAtPreferenceKey, occurredAtUtc.ToString("O"));
    }

    private static void ClearFailedApplyState()
    {
        Preferences.Default.Remove(LastFailedApplyPackageIdPreferenceKey);
        Preferences.Default.Remove(LastFailedApplyAtPreferenceKey);
    }

    private sealed record ScheduledCheckState(long Id, DateTimeOffset DueAtUtc);

    private enum CheckScheduleReason
    {
        Initial,
        Resume
    }
}

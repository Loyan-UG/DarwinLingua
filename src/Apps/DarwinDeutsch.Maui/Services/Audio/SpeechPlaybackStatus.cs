namespace DarwinDeutsch.Maui.Services.Audio;

/// <summary>
/// Represents the outcome category of a pronunciation playback request.
/// </summary>
public enum SpeechPlaybackStatus
{
    Succeeded = 0,
    Unsupported = 1,
    LocaleUnavailable = 2,
    InvalidRequest = 3,
    Failed = 4,
    Cancelled = 5,
}

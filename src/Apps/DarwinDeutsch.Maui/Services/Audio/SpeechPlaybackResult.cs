namespace DarwinDeutsch.Maui.Services.Audio;

/// <summary>
/// Carries the normalized outcome of a pronunciation playback request.
/// </summary>
/// <param name="Status">The normalized playback status.</param>
public readonly record struct SpeechPlaybackResult(SpeechPlaybackStatus Status)
{
    /// <summary>
    /// Gets a value indicating whether the playback request completed successfully.
    /// </summary>
    public bool IsSuccess => Status == SpeechPlaybackStatus.Succeeded;
}

namespace DarwinDeutsch.Maui.Services.Audio;

/// <summary>
/// Defines the app-level text-to-speech abstraction for learner-facing pronunciation playback.
/// </summary>
public interface ISpeechPlaybackService
{
    /// <summary>
    /// Preloads platform text-to-speech capabilities so the first playback starts faster.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the warm-up request.</param>
    /// <returns>A task that completes when the warm-up attempt finishes.</returns>
    Task WarmUpAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Attempts to speak the supplied text using a voice compatible with the requested language.
    /// </summary>
    /// <param name="text">The text to pronounce.</param>
    /// <param name="languageCode">The language code that should be matched to a compatible voice.</param>
    /// <param name="cancellationToken">The cancellation token for the playback request.</param>
    /// <returns>The outcome of the pronunciation request.</returns>
    Task<SpeechPlaybackResult> SpeakAsync(string text, string languageCode, CancellationToken cancellationToken);
}

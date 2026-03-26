using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Media;

namespace DarwinDeutsch.Maui.Services.Audio;

/// <summary>
/// Wraps platform text-to-speech APIs behind an application-facing abstraction.
/// </summary>
internal sealed class SpeechPlaybackService : ISpeechPlaybackService
{
    /// <inheritdoc />
    public async Task<SpeechPlaybackResult> SpeakAsync(string text, string languageCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(languageCode))
        {
            return new SpeechPlaybackResult(SpeechPlaybackStatus.InvalidRequest);
        }

        string normalizedText = text.Trim();
        string normalizedLanguageCode = languageCode.Trim().ToLowerInvariant();

        try
        {
            IEnumerable<Locale> locales = await TextToSpeech.Default
                .GetLocalesAsync()
                .ConfigureAwait(false);
            Locale? locale = ResolveLocale(locales, normalizedLanguageCode);

            if (locale is null)
            {
                return new SpeechPlaybackResult(SpeechPlaybackStatus.LocaleUnavailable);
            }

            SpeechOptions options = new()
            {
                Locale = locale,
            };

            await TextToSpeech.Default
                .SpeakAsync(normalizedText, options, cancellationToken)
                .ConfigureAwait(false);

            return new SpeechPlaybackResult(SpeechPlaybackStatus.Succeeded);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new SpeechPlaybackResult(SpeechPlaybackStatus.Cancelled);
        }
        catch (FeatureNotSupportedException)
        {
            return new SpeechPlaybackResult(SpeechPlaybackStatus.Unsupported);
        }
        catch (Exception)
        {
            return new SpeechPlaybackResult(SpeechPlaybackStatus.Failed);
        }
    }

    /// <summary>
    /// Resolves the best available locale for the requested language code.
    /// </summary>
    /// <param name="locales">The available platform locales.</param>
    /// <param name="languageCode">The requested two-letter language code.</param>
    /// <returns>The matched locale when one exists; otherwise <see langword="null"/>.</returns>
    private static Locale? ResolveLocale(IEnumerable<Locale> locales, string languageCode)
    {
        ArgumentNullException.ThrowIfNull(locales);
        ArgumentException.ThrowIfNullOrWhiteSpace(languageCode);

        return locales.FirstOrDefault(locale =>
                   string.Equals(locale.Language, languageCode, StringComparison.OrdinalIgnoreCase))
               ?? locales.FirstOrDefault(locale =>
                   locale.Language.StartsWith(languageCode, StringComparison.OrdinalIgnoreCase));
    }
}

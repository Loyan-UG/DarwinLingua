using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Media;
using System.Collections.Concurrent;

namespace DarwinDeutsch.Maui.Services.Audio;

/// <summary>
/// Wraps platform text-to-speech APIs behind an application-facing abstraction.
/// </summary>
internal sealed class SpeechPlaybackService : ISpeechPlaybackService
{
    private readonly SemaphoreSlim _localeGate = new(1, 1);
    private readonly ConcurrentDictionary<string, Locale?> _resolvedLocales = new(StringComparer.OrdinalIgnoreCase);
    private Locale[]? _cachedLocales;

    /// <inheritdoc />
    public async Task WarmUpAsync(CancellationToken cancellationToken)
    {
        try
        {
            await ResolveLocaleAsync("de", cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            // Warm-up is opportunistic. Playback still resolves voices lazily if this fails.
        }
    }

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
            Locale? locale = await ResolveLocaleAsync(normalizedLanguageCode, cancellationToken).ConfigureAwait(false);

            if (locale is null)
            {
                await MainThread.InvokeOnMainThreadAsync(
                        () => TextToSpeech.Default.SpeakAsync(normalizedText, cancelToken: cancellationToken))
                    .ConfigureAwait(false);

                return new SpeechPlaybackResult(SpeechPlaybackStatus.Succeeded);
            }

            SpeechOptions options = new()
            {
                Locale = locale,
            };

            await MainThread.InvokeOnMainThreadAsync(
                    () => TextToSpeech.Default.SpeakAsync(normalizedText, options, cancellationToken))
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
        catch (ArgumentException)
        {
            return new SpeechPlaybackResult(SpeechPlaybackStatus.LocaleUnavailable);
        }
        catch (Exception)
        {
            return new SpeechPlaybackResult(SpeechPlaybackStatus.Failed);
        }
    }

    private async Task<Locale[]?> GetCachedLocalesAsync(CancellationToken cancellationToken)
    {
        Locale[]? cachedLocales = Volatile.Read(ref _cachedLocales);
        if (cachedLocales is not null)
        {
            return cachedLocales;
        }

        await _localeGate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            cachedLocales = _cachedLocales;
            if (cachedLocales is not null)
            {
                return cachedLocales;
            }

            cachedLocales = (await TextToSpeech.Default.GetLocalesAsync().ConfigureAwait(false)).ToArray();
            Volatile.Write(ref _cachedLocales, cachedLocales);
            _resolvedLocales.Clear();
            return cachedLocales;
        }
        finally
        {
            _localeGate.Release();
        }
    }

    private async Task<Locale?> ResolveLocaleAsync(string languageCode, CancellationToken cancellationToken)
    {
        if (_resolvedLocales.TryGetValue(languageCode, out Locale? cachedLocale))
        {
            return cachedLocale;
        }

        Locale[]? locales = await GetCachedLocalesAsync(cancellationToken).ConfigureAwait(false);
        Locale? resolvedLocale = locales is null ? null : ResolveLocale(locales, languageCode);
        _resolvedLocales[languageCode] = resolvedLocale;
        return resolvedLocale;
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

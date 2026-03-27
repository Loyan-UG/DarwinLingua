using System.Text.Json;
using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;

namespace DarwinLingua.ContentOps.Infrastructure.Services;

/// <summary>
/// Parses canonical JSON content-package files into application models.
/// </summary>
internal sealed class ContentImportParser : IContentImportParser
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <inheritdoc />
    public Task<ParsedContentPackageModel> ParseAsync(string rawContent, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawContent);

        cancellationToken.ThrowIfCancellationRequested();

        ContentPackageDocument? document;

        try
        {
            document = JsonSerializer.Deserialize<ContentPackageDocument>(rawContent, SerializerOptions);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"The package file is not valid JSON: {exception.Message}", exception);
        }

        if (document is null)
        {
            throw new InvalidDataException("The package file does not contain a valid root object.");
        }

        if (document.Entries is null)
        {
            throw new InvalidDataException("The package file must contain an entries array.");
        }

        ParsedContentPackageModel parsedPackage = new(
            document.PackageVersion ?? string.Empty,
            document.PackageId ?? string.Empty,
            document.PackageName ?? string.Empty,
            document.Source,
            (document.DefaultMeaningLanguages ?? []).Select(language => language ?? string.Empty).ToArray(),
            document.Entries.Select(Map).ToArray());

        return Task.FromResult(parsedPackage);
    }

    private static ParsedContentEntryModel Map(ContentEntryDocument entry)
    {
        return new ParsedContentEntryModel(
            entry.Word ?? string.Empty,
            entry.Language ?? string.Empty,
            entry.CefrLevel ?? string.Empty,
            entry.PartOfSpeech ?? string.Empty,
            (entry.Topics ?? []).Select(topic => topic ?? string.Empty).ToArray(),
            (entry.UsageLabels ?? []).Select(label => label ?? string.Empty).ToArray(),
            (entry.ContextLabels ?? []).Select(label => label ?? string.Empty).ToArray(),
            (entry.Meanings ?? []).Select(meaning => new ParsedContentMeaningModel(
                meaning.Language ?? string.Empty,
                meaning.Text ?? string.Empty)).ToArray(),
            (entry.Examples ?? []).Select(example => new ParsedContentExampleModel(
                example.BaseText ?? string.Empty,
                (example.Translations ?? []).Select(translation => new ParsedContentMeaningModel(
                    translation.Language ?? string.Empty,
                    translation.Text ?? string.Empty)).ToArray())).ToArray(),
            entry.Article,
            entry.Plural);
    }

    private sealed class ContentPackageDocument
    {
        public string? PackageVersion { get; set; }

        public string? PackageId { get; set; }

        public string? PackageName { get; set; }

        public string? Source { get; set; }

        public string?[]? DefaultMeaningLanguages { get; set; }

        public ContentEntryDocument[]? Entries { get; set; }
    }

    private sealed class ContentEntryDocument
    {
        public string? Word { get; set; }

        public string? Language { get; set; }

        public string? CefrLevel { get; set; }

        public string? PartOfSpeech { get; set; }

        public string? Article { get; set; }

        public string? Plural { get; set; }

        public string?[]? Topics { get; set; }

        public string?[]? UsageLabels { get; set; }

        public string?[]? ContextLabels { get; set; }

        public ContentMeaningDocument[]? Meanings { get; set; }

        public ContentExampleDocument[]? Examples { get; set; }
    }

    private sealed class ContentMeaningDocument
    {
        public string? Language { get; set; }

        public string? Text { get; set; }
    }

    private sealed class ContentExampleDocument
    {
        public string? BaseText { get; set; }

        public ContentMeaningDocument[]? Translations { get; set; }
    }
}

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
            document.Entries.Select(Map).ToArray(),
            (document.Collections ?? []).Select(Map).ToArray());

        return Task.FromResult(parsedPackage);
    }

    private static ParsedContentEntryModel Map(ContentEntryDocument entry)
    {
        return new ParsedContentEntryModel(
            entry.Word ?? string.Empty,
            entry.Language ?? string.Empty,
            entry.CefrLevel ?? string.Empty,
            entry.PartOfSpeech ?? string.Empty,
            (entry.LexicalForms ?? []).Select(form => new ParsedContentLexicalFormModel(
                form.PartOfSpeech ?? string.Empty,
                form.Article,
                form.Plural,
                form.Infinitive,
                form.IsPrimary ?? false)).ToArray(),
            (entry.Topics ?? []).Select(topic => topic ?? string.Empty).ToArray(),
            (entry.UsageLabels ?? []).Select(label => label ?? string.Empty).ToArray(),
            (entry.ContextLabels ?? []).Select(label => label ?? string.Empty).ToArray(),
            (entry.GrammarNotes ?? []).Select(note => note ?? string.Empty).ToArray(),
            (entry.Collocations ?? []).Select(collocation => new ParsedContentCollocationModel(
                collocation.Text ?? string.Empty,
                collocation.Meaning)).ToArray(),
            (entry.WordFamilies ?? []).Select(member => new ParsedContentWordFamilyMemberModel(
                member.Lemma ?? string.Empty,
                member.RelationLabel ?? string.Empty,
                member.Note)).ToArray(),
            (entry.Relations ?? []).Select(relation => new ParsedContentWordRelationModel(
                relation.Kind ?? string.Empty,
                relation.Lemma ?? string.Empty,
                relation.Note)).ToArray(),
            (entry.Meanings ?? []).Select(meaning => new ParsedContentMeaningModel(
                meaning.Language ?? string.Empty,
                meaning.Text ?? string.Empty)).ToArray(),
            (entry.Examples ?? []).Select(example => new ParsedContentExampleModel(
                example.BaseText ?? string.Empty,
                (example.Translations ?? []).Select(translation => new ParsedContentMeaningModel(
                    translation.Language ?? string.Empty,
                    translation.Text ?? string.Empty)).ToArray())).ToArray(),
            entry.Article,
            entry.Plural,
            entry.Infinitive,
            entry.PronunciationIpa,
            entry.SyllableBreak);
    }

    private static ParsedContentCollectionModel Map(ContentCollectionDocument collection)
    {
        ParsedContentCollectionWordReferenceModel[] explicitWords = (collection.Words ?? [])
            .Select(word => new ParsedContentCollectionWordReferenceModel(
                word.Word ?? string.Empty,
                word.PartOfSpeech,
                word.CefrLevel))
            .ToArray();

        ParsedContentCollectionWordReferenceModel[] wordKeyReferences = (collection.WordKeys ?? [])
            .Select(wordKey => new ParsedContentCollectionWordReferenceModel(
                wordKey ?? string.Empty,
                null,
                null))
            .ToArray();

        return new ParsedContentCollectionModel(
            collection.Slug ?? string.Empty,
            collection.Name ?? string.Empty,
            collection.Description,
            collection.ImageUrl ?? collection.Image,
            collection.SortOrder ?? 0,
            explicitWords.Concat(wordKeyReferences).ToArray());
    }

    private sealed class ContentPackageDocument
    {
        public string? PackageVersion { get; set; }

        public string? PackageId { get; set; }

        public string? PackageName { get; set; }

        public string? Source { get; set; }

        public string?[]? DefaultMeaningLanguages { get; set; }

        public ContentEntryDocument[]? Entries { get; set; }

        public ContentCollectionDocument[]? Collections { get; set; }
    }

    private sealed class ContentEntryDocument
    {
        public string? Word { get; set; }

        public string? Language { get; set; }

        public string? CefrLevel { get; set; }

        public string? PartOfSpeech { get; set; }

        public string? Article { get; set; }

        public string? Plural { get; set; }

        public string? Infinitive { get; set; }

        public string? PronunciationIpa { get; set; }

        public string? SyllableBreak { get; set; }

        public ContentLexicalFormDocument[]? LexicalForms { get; set; }

        public string?[]? Topics { get; set; }

        public string?[]? UsageLabels { get; set; }

        public string?[]? ContextLabels { get; set; }

        public string?[]? GrammarNotes { get; set; }

        public ContentCollocationDocument[]? Collocations { get; set; }

        public ContentWordFamilyMemberDocument[]? WordFamilies { get; set; }

        public ContentWordRelationDocument[]? Relations { get; set; }

        public ContentMeaningDocument[]? Meanings { get; set; }

        public ContentExampleDocument[]? Examples { get; set; }
    }

    private sealed class ContentLexicalFormDocument
    {
        public string? PartOfSpeech { get; set; }

        public string? Article { get; set; }

        public string? Plural { get; set; }

        public string? Infinitive { get; set; }

        public bool? IsPrimary { get; set; }
    }

    private sealed class ContentMeaningDocument
    {
        public string? Language { get; set; }

        public string? Text { get; set; }
    }

    private sealed class ContentCollocationDocument
    {
        public string? Text { get; set; }

        public string? Meaning { get; set; }
    }

    private sealed class ContentWordFamilyMemberDocument
    {
        public string? Lemma { get; set; }

        public string? RelationLabel { get; set; }

        public string? Note { get; set; }
    }

    private sealed class ContentWordRelationDocument
    {
        public string? Kind { get; set; }

        public string? Lemma { get; set; }

        public string? Note { get; set; }
    }

    private sealed class ContentExampleDocument
    {
        public string? BaseText { get; set; }

        public ContentMeaningDocument[]? Translations { get; set; }
    }

    private sealed class ContentCollectionDocument
    {
        public string? Slug { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public string? Image { get; set; }

        public int? SortOrder { get; set; }

        public ContentCollectionWordReferenceDocument[]? Words { get; set; }

        public string?[]? WordKeys { get; set; }
    }

    private sealed class ContentCollectionWordReferenceDocument
    {
        public string? Word { get; set; }

        public string? PartOfSpeech { get; set; }

        public string? CefrLevel { get; set; }
    }
}

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
            (document.Labels ?? []).Select(Map).ToArray(),
            (document.Collections ?? []).Select(Map).ToArray())
        {
            Scenarios = (document.Scenarios ?? []).Select(Map).ToArray(),
            ConversationStarterPacks = (document.ConversationStarterPacks ?? []).Select(Map).ToArray(),
            EventPreparationPacks = (document.EventPreparationPacks ?? []).Select(Map).ToArray(),
        };

        return Task.FromResult(parsedPackage);
    }

    private static ParsedContentLabelDefinitionModel Map(ContentLabelDefinitionDocument label)
    {
        return new ParsedContentLabelDefinitionModel(
            label.Kind ?? string.Empty,
            label.Key ?? string.Empty,
            label.DisplayName ?? string.Empty,
            (label.Localizations ?? []).Select(localization => new ParsedLocalizedTextModel(
                localization.Language ?? string.Empty,
                localization.Name ?? string.Empty,
                localization.Description)).ToArray(),
            label.SortOrder ?? 0);
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
            (collection.Localizations ?? []).Select(localization => new ParsedLocalizedTextModel(
                localization.Language ?? string.Empty,
                localization.Name ?? string.Empty,
                localization.Description)).ToArray(),
            collection.ImageUrl ?? collection.Image,
            collection.SortOrder ?? 0,
            explicitWords.Concat(wordKeyReferences).ToArray());
    }

    private static ParsedScenarioLessonModel Map(ScenarioLessonDocument scenario)
    {
        return new ParsedScenarioLessonModel(
            scenario.Slug ?? string.Empty,
            scenario.Title ?? string.Empty,
            scenario.Description ?? string.Empty,
            scenario.LearnerGoal ?? string.Empty,
            scenario.CefrLevel ?? string.Empty,
            scenario.Category ?? string.Empty,
            (scenario.Topics ?? []).Select(topic => topic ?? string.Empty).ToArray(),
            scenario.SortOrder ?? 0,
            (scenario.DialogueTurns ?? []).Select(turn => new ParsedScenarioDialogueTurnModel(
                turn.SpeakerRole ?? string.Empty,
                turn.BaseText ?? string.Empty,
                MapTranslations(turn.Translations))).ToArray(),
            (scenario.UsefulPhrases ?? []).Select(phrase => new ParsedScenarioPhraseModel(
                phrase.BaseText ?? string.Empty,
                MapTranslations(phrase.Translations),
                phrase.UsageNote)).ToArray(),
            (scenario.Questions ?? []).Select(question => new ParsedScenarioQuestionModel(
                question.Prompt ?? string.Empty,
                MapTranslations(question.Translations),
                (question.Answers ?? []).Select(answer => new ParsedScenarioAnswerModel(
                    answer.Text ?? string.Empty,
                    MapTranslations(answer.Translations),
                    answer.IsCorrect ?? false,
                    answer.Feedback)).ToArray())).ToArray());
    }

    private static ParsedConversationStarterPackModel Map(ConversationStarterPackDocument pack)
    {
        return new ParsedConversationStarterPackModel(
            pack.Slug ?? string.Empty,
            pack.Title ?? string.Empty,
            pack.Description ?? string.Empty,
            pack.CefrLevel ?? string.Empty,
            pack.Category ?? string.Empty,
            pack.Situation ?? string.Empty,
            pack.Tone ?? string.Empty,
            pack.ConversationGoal ?? string.Empty,
            (pack.Topics ?? []).Select(topic => topic ?? string.Empty).ToArray(),
            pack.SortOrder ?? 0,
            (pack.LinkedScenarioSlugs ?? []).Select(slug => slug ?? string.Empty).ToArray(),
            (pack.LinkedEventPreparationPackSlugs ?? []).Select(slug => slug ?? string.Empty).ToArray(),
            (pack.Phrases ?? []).Select(phrase => new ParsedConversationStarterPhraseModel(
                phrase.BaseText ?? string.Empty,
                phrase.Function ?? string.Empty,
                MapTranslations(phrase.Translations),
                phrase.UsageNote,
                phrase.Register,
                phrase.SortOrder ?? 0,
            (phrase.AlternativeBaseTexts ?? []).Select(text => text ?? string.Empty).ToArray(),
            phrase.CommonMistake)).ToArray());
    }

    private static ParsedEventPreparationPackModel Map(EventPreparationPackDocument pack)
    {
        return new ParsedEventPreparationPackModel(
            pack.Slug ?? string.Empty,
            pack.Title ?? string.Empty,
            pack.Description ?? string.Empty,
            pack.CefrLevel ?? string.Empty,
            pack.Category ?? string.Empty,
            pack.EventType ?? string.Empty,
            (pack.Topics ?? []).Select(topic => topic ?? string.Empty).ToArray(),
            pack.SortOrder ?? 0,
            (pack.LinkedScenarioSlugs ?? []).Select(slug => slug ?? string.Empty).ToArray(),
            (pack.LinkedVocabulary ?? []).Select(reference => new ParsedEventPreparationVocabularyReferenceModel(
                reference.Word ?? string.Empty,
                reference.PartOfSpeech,
                reference.CefrLevel)).ToArray(),
            (pack.LinkedConversationStarterPackSlugs ?? []).Select(slug => slug ?? string.Empty).ToArray(),
            (pack.OpeningPrompts ?? []).Select(prompt => prompt ?? string.Empty).ToArray(),
            (pack.RoleplayPrompts ?? []).Select(prompt => prompt ?? string.Empty).ToArray(),
            (pack.ReviewPrompts ?? []).Select(prompt => prompt ?? string.Empty).ToArray());
    }

    private static ParsedContentMeaningModel[] MapTranslations(ContentMeaningDocument[]? translations)
    {
        return (translations ?? [])
            .Select(translation => new ParsedContentMeaningModel(
                translation.Language ?? string.Empty,
                translation.Text ?? string.Empty))
            .ToArray();
    }

    private sealed class ContentPackageDocument
    {
        public string? PackageVersion { get; set; }

        public string? PackageId { get; set; }

        public string? PackageName { get; set; }

        public string? Source { get; set; }

        public string?[]? DefaultMeaningLanguages { get; set; }

        public ContentEntryDocument[]? Entries { get; set; }

        public ContentLabelDefinitionDocument[]? Labels { get; set; }

        public ContentCollectionDocument[]? Collections { get; set; }

        public ScenarioLessonDocument[]? Scenarios { get; set; }

        public ConversationStarterPackDocument[]? ConversationStarterPacks { get; set; }

        public EventPreparationPackDocument[]? EventPreparationPacks { get; set; }
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

        public LocalizedTextDocument[]? Localizations { get; set; }

        public string? ImageUrl { get; set; }

        public string? Image { get; set; }

        public int? SortOrder { get; set; }

        public ContentCollectionWordReferenceDocument[]? Words { get; set; }

        public string?[]? WordKeys { get; set; }
    }

    private sealed class ContentLabelDefinitionDocument
    {
        public string? Kind { get; set; }

        public string? Key { get; set; }

        public string? DisplayName { get; set; }

        public LocalizedTextDocument[]? Localizations { get; set; }

        public int? SortOrder { get; set; }
    }

    private sealed class LocalizedTextDocument
    {
        public string? Language { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }
    }

    private sealed class ContentCollectionWordReferenceDocument
    {
        public string? Word { get; set; }

        public string? PartOfSpeech { get; set; }

        public string? CefrLevel { get; set; }
    }

    private sealed class ScenarioLessonDocument
    {
        public string? Slug { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? LearnerGoal { get; set; }

        public string? CefrLevel { get; set; }

        public string? Category { get; set; }

        public string?[]? Topics { get; set; }

        public int? SortOrder { get; set; }

        public ScenarioDialogueTurnDocument[]? DialogueTurns { get; set; }

        public ScenarioPhraseDocument[]? UsefulPhrases { get; set; }

        public ScenarioQuestionDocument[]? Questions { get; set; }
    }

    private sealed class ScenarioDialogueTurnDocument
    {
        public string? SpeakerRole { get; set; }

        public string? BaseText { get; set; }

        public ContentMeaningDocument[]? Translations { get; set; }
    }

    private sealed class ScenarioPhraseDocument
    {
        public string? BaseText { get; set; }

        public ContentMeaningDocument[]? Translations { get; set; }

        public string? UsageNote { get; set; }
    }

    private sealed class ScenarioQuestionDocument
    {
        public string? Prompt { get; set; }

        public ContentMeaningDocument[]? Translations { get; set; }

        public ScenarioAnswerDocument[]? Answers { get; set; }
    }

    private sealed class ScenarioAnswerDocument
    {
        public string? Text { get; set; }

        public ContentMeaningDocument[]? Translations { get; set; }

        public bool? IsCorrect { get; set; }

        public string? Feedback { get; set; }
    }

    private sealed class ConversationStarterPackDocument
    {
        public string? Slug { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? CefrLevel { get; set; }

        public string? Category { get; set; }

        public string? Situation { get; set; }

        public string? Tone { get; set; }

        public string? ConversationGoal { get; set; }

        public string?[]? Topics { get; set; }

        public int? SortOrder { get; set; }

        public string?[]? LinkedScenarioSlugs { get; set; }

        public string?[]? LinkedEventPreparationPackSlugs { get; set; }

        public ConversationStarterPhraseDocument[]? Phrases { get; set; }
    }

    private sealed class ConversationStarterPhraseDocument
    {
        public string? BaseText { get; set; }

        public string? Function { get; set; }

        public ContentMeaningDocument[]? Translations { get; set; }

        public string? UsageNote { get; set; }

        public string? Register { get; set; }

        public int? SortOrder { get; set; }

        public string?[]? AlternativeBaseTexts { get; set; }

        public string? CommonMistake { get; set; }
    }

    private sealed class EventPreparationPackDocument
    {
        public string? Slug { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? CefrLevel { get; set; }

        public string? Category { get; set; }

        public string? EventType { get; set; }

        public string?[]? Topics { get; set; }

        public int? SortOrder { get; set; }

        public string?[]? LinkedScenarioSlugs { get; set; }

        public EventPreparationVocabularyReferenceDocument[]? LinkedVocabulary { get; set; }

        public string?[]? LinkedConversationStarterPackSlugs { get; set; }

        public string?[]? OpeningPrompts { get; set; }

        public string?[]? RoleplayPrompts { get; set; }

        public string?[]? ReviewPrompts { get; set; }
    }

    private sealed class EventPreparationVocabularyReferenceDocument
    {
        public string? Word { get; set; }

        public string? PartOfSpeech { get; set; }

        public string? CefrLevel { get; set; }
    }
}

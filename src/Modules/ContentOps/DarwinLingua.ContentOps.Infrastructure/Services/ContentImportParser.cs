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

        if (document.Entries is null && document.GrammarTopics is null)
        {
            throw new InvalidDataException("The package file must contain an entries array.");
        }

        ParsedContentPackageModel parsedPackage = new(
            document.PackageVersion ?? string.Empty,
            document.PackageId ?? string.Empty,
            document.PackageName ?? string.Empty,
            document.Source,
            (document.DefaultMeaningLanguages ?? document.TargetLanguages ?? []).Select(language => language ?? string.Empty).ToArray(),
            (document.Entries ?? []).Select(Map).ToArray(),
            (document.Labels ?? []).Select(Map).ToArray(),
            (document.Collections ?? []).Select(Map).ToArray())
        {
            Dialogues = (document.Dialogues ?? []).Select(Map).ToArray(),
            TalkTopics = (document.TalkTopics ?? []).Select(Map).ToArray(),
            GrammarTopics = (document.GrammarTopics ?? []).Select(Map).ToArray(),
            ExpressionEntries = (document.ExpressionEntries ?? []).Select(Map).ToArray(),
            Exercises = (document.Exercises ?? []).Select(Map).ToArray(),
            ExerciseSets = (document.ExerciseSets ?? []).Select(Map).ToArray(),
            CoursePaths = (document.CoursePaths ?? []).Select(Map).ToArray(),
            CourseModules = (document.CourseModules ?? []).Select(Map).ToArray(),
            CourseLessons = (document.CourseLessons ?? []).Select(Map).ToArray(),
            WritingTemplates = (document.WritingTemplates ?? []).Select(Map).ToArray(),
            CulturalNotes = (document.CulturalNotes ?? []).Select(Map).ToArray(),
            ExamProfiles = (document.ExamProfiles ?? []).Select(Map).ToArray(),
            ExamPrepUnits = (document.ExamPrepUnits ?? []).Select(Map).ToArray(),
            ConversationStarterPacks = (document.ConversationStarterPacks ?? []).Select(Map).ToArray(),
            EventPreparationPacks = (document.EventPreparationPacks ?? []).Select(Map).ToArray(),
            RoleplayScenarios = (document.RoleplayScenarios ?? []).Select(Map).ToArray(),
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

    private static ParsedDialogueLessonModel Map(DialogueLessonDocument dialogue)
    {
        return new ParsedDialogueLessonModel(
            dialogue.Slug ?? string.Empty,
            dialogue.Title ?? string.Empty,
            dialogue.Description ?? string.Empty,
            dialogue.LearnerGoal ?? string.Empty,
            dialogue.CefrLevel ?? string.Empty,
            dialogue.Category ?? string.Empty,
            (dialogue.Topics ?? []).Select(topic => topic ?? string.Empty).ToArray(),
            (dialogue.ExamProfiles ?? []).Select(profile => profile ?? string.Empty).ToArray(),
            (dialogue.SkillFocus ?? []).Select(focus => focus ?? string.Empty).ToArray(),
            dialogue.TaskType ?? string.Empty,
            dialogue.InteractionMode ?? string.Empty,
            dialogue.Register ?? string.Empty,
            (dialogue.SpeakingFunctions ?? []).Select(function => function ?? string.Empty).ToArray(),
            dialogue.EstimatedPracticeMinutes ?? 15,
            dialogue.DifficultyNote,
            dialogue.ExamRelevance,
            (dialogue.UsefulWords ?? []).Select(word => new ParsedDialogueUsefulWordModel(
                word.Lemma ?? string.Empty,
                word.WordSlug,
                word.CefrLevel,
                word.SortOrder ?? 0)).ToArray(),
            (dialogue.SpeakingPrompts ?? []).Select(prompt => new ParsedDialogueSpeakingPromptModel(
                prompt.PromptType ?? string.Empty,
                prompt.Prompt ?? string.Empty,
                MapTranslations(prompt.Translations),
                prompt.SortOrder ?? 0)).ToArray(),
            dialogue.SortOrder ?? 0,
            (dialogue.DialogueTurns ?? []).Select(turn => new ParsedDialogueTurnModel(
                turn.SpeakerRole ?? string.Empty,
                turn.BaseText ?? string.Empty,
                MapTranslations(turn.Translations))).ToArray(),
            (dialogue.UsefulPhrases ?? []).Select(phrase => new ParsedDialoguePhraseModel(
                phrase.BaseText ?? string.Empty,
                MapTranslations(phrase.Translations),
                phrase.UsageNote)).ToArray(),
            (dialogue.Questions ?? []).Select(question => new ParsedDialogueQuestionModel(
                question.Prompt ?? string.Empty,
                MapTranslations(question.Translations),
                (question.Answers ?? []).Select(answer => new ParsedDialogueAnswerModel(
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
            (pack.LinkedDialogueSlugs ?? []).Select(slug => slug ?? string.Empty).ToArray(),
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

    private static ParsedTalkTopicModel Map(TalkTopicDocument topic)
    {
        TalkTopicArticleDocument article = topic.Article ?? new TalkTopicArticleDocument();

        return new ParsedTalkTopicModel(
            topic.Slug ?? string.Empty,
            topic.TopicGroupKey ?? string.Empty,
            topic.Title ?? string.Empty,
            topic.Description ?? string.Empty,
            topic.CefrLevel ?? string.Empty,
            topic.Category ?? string.Empty,
            (topic.Topics ?? []).Select(item => item ?? string.Empty).ToArray(),
            topic.ContentType ?? string.Empty,
            new ParsedTalkTopicArticleModel(
                article.BaseText ?? string.Empty,
                MapTranslations(article.Translations)),
            (topic.WarmupQuestions ?? []).Select(question => new ParsedTalkTopicQuestionModel(
                question.Prompt ?? string.Empty,
                MapTranslations(question.Translations),
                question.SortOrder ?? 0)).ToArray(),
            (topic.DiscussionQuestions ?? []).Select(question => new ParsedTalkTopicDiscussionQuestionModel(
                question.Prompt ?? string.Empty,
                question.QuestionType ?? string.Empty,
                MapTranslations(question.Translations),
                question.SortOrder ?? 0)).ToArray(),
            (topic.VocabularyItems ?? []).Select(item => new ParsedTalkTopicVocabularyItemModel(
                item.Lemma ?? string.Empty,
                item.WordSlug,
                item.CefrLevel,
                item.SortOrder ?? 0)).ToArray(),
            (topic.SpeakingGoals ?? []).Select(goal => goal ?? string.Empty).ToArray(),
            topic.EstimatedReadingMinutes ?? 0,
            topic.EstimatedDiscussionMinutes ?? 0,
            topic.IsSensitive ?? false,
            topic.SensitivityNote,
            topic.RecommendedForModeratedGroupsOnly ?? false,
            topic.SortOrder ?? 0,
            topic.IsPublished ?? true);
    }

    private static ParsedGrammarTopicModel Map(GrammarTopicDocument topic)
    {
        IReadOnlyDictionary<string, string> titleLocalized = MapLocalizedTextObject(topic.TitleLocalized);
        IReadOnlyDictionary<string, string> shortDescriptionLocalized = MapLocalizedTextObject(topic.ShortDescriptionLocalized);
        string fallbackTitle = topic.Title ?? GetFallbackLocalizedText(titleLocalized);
        string fallbackShortDescription = topic.ShortDescription ?? GetFallbackLocalizedText(shortDescriptionLocalized);

        return new ParsedGrammarTopicModel(
            topic.Slug ?? string.Empty,
            topic.ContentRevision,
            fallbackTitle,
            fallbackShortDescription,
            titleLocalized,
            shortDescriptionLocalized,
            topic.CefrLevel ?? string.Empty,
            topic.GrammarCategory ?? string.Empty,
            (topic.Topics ?? []).Select(item => item ?? string.Empty).ToArray(),
            topic.IsPublished ?? true,
            topic.SortOrder ?? 0,
            (topic.Sections ?? []).Select(section => new ParsedGrammarSectionModel(
                section.SectionKey ?? string.Empty,
                section.Heading ?? ResolveSectionHeading(section),
                section.Explanation ?? ResolveSectionExplanation(section),
                (section.Translations ?? []).Select(translation => new ParsedGrammarSectionTranslationModel(
                    translation.Language ?? string.Empty,
                    translation.Heading ?? string.Empty,
                    translation.Text ?? string.Empty)).ToArray(),
                MapLocalizedBlocks(section.LocalizedBlocks),
                section.SortOrder ?? 0)).ToArray(),
            (topic.Examples ?? []).Select(example => new ParsedGrammarExampleModel(
                example.GermanText ?? string.Empty,
                example.Note,
                MapFlexibleTranslations(example.Translations),
                example.SortOrder ?? 0)).ToArray(),
            MapGrammarTextItems(topic.RuleSummaries, topic.RuleSummariesLocalized),
            (topic.CommonMistakes ?? []).Select(item => new ParsedGrammarCommonMistakeModel(
                item.WrongText ?? item.WrongGerman ?? string.Empty,
                item.CorrectedText ?? item.CorrectGerman ?? string.Empty,
                item.Explanation ?? GetFallbackLocalizedText(MapLocalizedTextObject(item.ExplanationLocalized)),
                MapFlexibleTranslations(item.Translations, item.ExplanationLocalized),
                item.SortOrder ?? 0)).ToArray(),
            (topic.ExceptionNotes ?? []).Select(item => new ParsedGrammarTextItemModel(
                item.Text ?? string.Empty,
                MapTranslations(item.Translations),
                item.SortOrder ?? 0)).ToArray(),
            (topic.PrerequisiteSlugs ?? []).Select(slug => slug ?? string.Empty).ToArray(),
            (topic.RelatedTopicSlugs ?? []).Select(slug => slug ?? string.Empty).ToArray(),
            (topic.LinkedWords ?? []).Select(word => new ParsedGrammarLinkedWordModel(
                word.Lemma ?? string.Empty,
                word.WordSlug,
                word.SortOrder ?? 0)).ToArray(),
            (topic.LinkedDialogueSlugs ?? []).Select(slug => slug ?? string.Empty).ToArray(),
            (topic.LinkedTalkTopicSlugs ?? []).Select(slug => slug ?? string.Empty).ToArray(),
            (topic.LinkedExerciseSlugs ?? []).Select(slug => slug ?? string.Empty).ToArray(),
            SerializeOptionalArray(topic.ImageSlots));
    }

    private static ParsedExpressionEntryModel Map(ExpressionEntryDocument expression)
    {
        return new ParsedExpressionEntryModel(
            expression.Slug ?? string.Empty,
            expression.ExpressionText ?? string.Empty,
            expression.LiteralMeaningText,
            expression.ActualMeaningText ?? string.Empty,
            expression.UsageExplanation,
            expression.CefrLevel ?? string.Empty,
            expression.ExpressionType ?? string.Empty,
            expression.Register ?? string.Empty,
            expression.Category ?? expression.Context ?? string.Empty,
            expression.Region,
            expression.IsRisky ?? false,
            (expression.Topics ?? []).Select(topic => topic ?? string.Empty).ToArray(),
            expression.IsPublished ?? true,
            expression.SortOrder ?? 0,
            (expression.Meanings ?? []).Select(meaning => new ParsedExpressionMeaningModel(
                meaning.Language ?? string.Empty,
                meaning.ActualMeaningText ?? meaning.Text ?? string.Empty,
                meaning.LiteralMeaningText,
                meaning.UsageExplanation)).ToArray(),
            (expression.Examples ?? []).Select(example => new ParsedExpressionExampleModel(
                example.GermanText ?? string.Empty,
                example.Note,
                MapTranslations(example.Translations),
                example.SortOrder ?? 0)).ToArray(),
            ((expression.Warnings ?? []).Concat(expression.ContentWarnings ?? [])).Select(warning => new ParsedExpressionWarningModel(
                warning.WarningType ?? string.Empty,
                warning.Text ?? string.Empty,
                MapTranslations(warning.Translations))).ToArray(),
            (expression.LinkedWords ?? []).Select(word => new ParsedExpressionLinkedWordModel(
                word.Lemma ?? string.Empty,
                word.WordSlug,
                word.SortOrder ?? 0)).ToArray(),
            (expression.RelatedExpressionSlugs ?? []).Select(slug => slug ?? string.Empty).ToArray(),
            (expression.LinkedExerciseSlugs ?? []).Select(slug => slug ?? string.Empty).ToArray(),
            expression.MeaningTransparency,
            expression.TeachingReason,
            expression.SafetyRating ?? "general",
            expression.MinimumAge ?? 0,
            expression.RequiresAdultAccess ?? false,
            expression.AdultContentCategory,
            expression.SensitiveContentKind ?? "none",
            expression.RequiresSensitiveOptIn ?? false,
            expression.RequiresVerifiedAdult ?? false,
            expression.UsagePolicy ?? "safe-to-use");
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
            (pack.LinkedDialogueSlugs ?? []).Select(slug => slug ?? string.Empty).ToArray(),
            (pack.LinkedVocabulary ?? []).Select(reference => new ParsedEventPreparationVocabularyReferenceModel(
                reference.Word ?? string.Empty,
                reference.PartOfSpeech,
                reference.CefrLevel)).ToArray(),
            (pack.LinkedConversationStarterPackSlugs ?? []).Select(slug => slug ?? string.Empty).ToArray(),
            (pack.OpeningPrompts ?? []).Select(prompt => prompt ?? string.Empty).ToArray(),
            (pack.RoleplayPrompts ?? []).Select(prompt => prompt ?? string.Empty).ToArray(),
            (pack.ReviewPrompts ?? []).Select(prompt => prompt ?? string.Empty).ToArray());
    }

    private static ParsedRoleplayScenarioModel Map(RoleplayScenarioDocument scenario)
    {
        return new ParsedRoleplayScenarioModel(
            scenario.Slug ?? string.Empty,
            scenario.LinkedDialogueSlug ?? scenario.ScenarioSlug,
            scenario.Title ?? string.Empty,
            MapTranslations(scenario.TitleTranslations),
            scenario.Description ?? string.Empty,
            MapTranslations(scenario.DescriptionTranslations),
            scenario.LearnerGoal ?? string.Empty,
            MapTranslations(scenario.LearnerGoalTranslations),
            scenario.CefrLevel ?? string.Empty,
            scenario.Category ?? string.Empty,
            (scenario.Topics ?? []).Select(topic => topic ?? string.Empty).ToArray(),
            (scenario.ExamProfiles ?? []).Select(profile => profile ?? string.Empty).ToArray(),
            (scenario.SkillFocus ?? []).Select(focus => focus ?? string.Empty).ToArray(),
            scenario.TaskType ?? string.Empty,
            scenario.InteractionMode ?? string.Empty,
            scenario.Register ?? string.Empty,
            scenario.EstimatedPracticeMinutes ?? 10,
            (scenario.Roles ?? []).Select(role => new ParsedRoleplayRoleModel(
                role.RoleKey ?? string.Empty,
                role.DisplayName ?? string.Empty,
                MapTranslations(role.Translations))).ToArray(),
            (scenario.Turns ?? []).Select(turn => new ParsedRoleplayTurnModel(
                turn.SortOrder ?? 0,
                turn.SpeakerRole ?? string.Empty,
                turn.BaseText ?? string.Empty,
                MapTranslations(turn.Translations),
                turn.Function,
                turn.ToneNote,
                turn.ExpectedLearnerAction)).ToArray(),
            (scenario.AnswerChoices ?? []).Select(group => new ParsedRoleplayAnswerChoiceGroupModel(
                group.TurnSortOrder ?? 0,
                (group.Choices ?? []).Select(choice => new ParsedRoleplayAnswerChoiceModel(
                    choice.Id ?? string.Empty,
                    choice.Text ?? string.Empty,
                    MapTranslations(choice.Translations),
                    choice.IsCorrect ?? false,
                    choice.Feedback ?? string.Empty,
                    MapTranslations(choice.FeedbackTranslations),
                    choice.ExplanationKey)).ToArray())).ToArray(),
            (scenario.StaticFeedback ?? []).Select(feedback => new ParsedRoleplayStaticFeedbackModel(
                feedback.TurnSortOrder ?? 0,
                feedback.FeedbackType ?? string.Empty,
                feedback.Text ?? string.Empty,
                MapTranslations(feedback.Translations))).ToArray(),
            (scenario.ImageSlots ?? []).Select(slot => new ParsedRoleplayImageSlotModel(
                slot.SlotKey ?? string.Empty,
                slot.Placement ?? string.Empty,
                slot.Purpose ?? string.Empty,
                slot.AltText ?? string.Empty,
                MapTranslations(slot.AltTextTranslations),
                slot.ImagePrompt ?? string.Empty,
                slot.AssetPath,
                slot.IsRequired ?? false)).ToArray(),
            scenario.IsPublished ?? true,
            scenario.SortOrder ?? 0);
    }

    private static ParsedExerciseModel Map(ExerciseDocument exercise)
    {
        return new ParsedExerciseModel(
            exercise.Slug ?? string.Empty,
            exercise.Title ?? string.Empty,
            exercise.Instruction ?? string.Empty,
            exercise.CefrLevel ?? string.Empty,
            exercise.ExerciseType ?? string.Empty,
            exercise.TargetSkill ?? string.Empty,
            exercise.OwnerType ?? string.Empty,
            exercise.OwnerSlug,
            SerializeRaw(exercise.Prompt),
            SerializeRaw(exercise.AnswerKey),
            exercise.CorrectExplanation ?? string.Empty,
            exercise.IncorrectExplanation ?? string.Empty,
            exercise.Hint,
            exercise.CommonMistakeNote,
            exercise.IsPublished ?? true,
            exercise.SortOrder ?? 0);
    }

    private static ParsedExerciseSetModel Map(ExerciseSetDocument set)
    {
        return new ParsedExerciseSetModel(
            set.Slug ?? string.Empty,
            set.Title ?? string.Empty,
            set.Description ?? string.Empty,
            set.CefrLevel ?? string.Empty,
            set.OwnerType ?? string.Empty,
            set.OwnerSlug,
            (set.ExerciseSlugs ?? []).Select(slug => slug ?? string.Empty).ToArray(),
            set.IsPublished ?? true,
            set.SortOrder ?? 0);
    }

    private static ParsedCoursePathModel Map(CoursePathDocument course) =>
        new(
            course.Slug ?? string.Empty,
            course.Title ?? string.Empty,
            course.Description ?? string.Empty,
            course.CefrLevel,
            course.CefrRange,
            course.IsPublished ?? true,
            course.SortOrder ?? 0);

    private static ParsedCourseModuleModel Map(CourseModuleDocument module) =>
        new(
            module.Slug ?? string.Empty,
            module.CoursePathSlug ?? string.Empty,
            module.Title ?? string.Empty,
            module.Description ?? string.Empty,
            module.ModuleNumber ?? 0,
            module.CefrLevel ?? string.Empty,
            module.IsPublished ?? true,
            module.SortOrder ?? 0);

    private static ParsedCourseLessonModel Map(CourseLessonDocument lesson) =>
        new(
            lesson.Slug ?? string.Empty,
            lesson.CoursePathSlug ?? string.Empty,
            lesson.ModuleSlug ?? string.Empty,
            lesson.LessonNumber ?? 0,
            lesson.Title ?? string.Empty,
            lesson.ShortDescription ?? string.Empty,
            lesson.Narrative ?? string.Empty,
            lesson.CefrLevel ?? string.Empty,
            lesson.EstimatedMinutes ?? 0,
            (lesson.LearningGoals ?? []).Select(item => item ?? string.Empty).ToArray(),
            (lesson.PrerequisiteLessonSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            lesson.NextLessonSlug,
            (lesson.LinkedGrammarTopicSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            (lesson.LinkedWordSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            (lesson.LinkedExpressionSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            (lesson.LinkedDialogueSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            (lesson.LinkedTalkTopicSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            (lesson.LinkedExerciseSetSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            (lesson.LinkedExamPrepSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            lesson.ReviewSummary,
            lesson.HomeworkTask,
            lesson.IsPublished ?? true,
            lesson.SortOrder ?? 0);

    private static ParsedWritingTemplateModel Map(WritingTemplateDocument template) =>
        new(
            template.Slug ?? string.Empty,
            template.Title ?? string.Empty,
            template.ShortDescription ?? string.Empty,
            template.CefrLevel ?? string.Empty,
            template.Category ?? string.Empty,
            template.Situation ?? string.Empty,
            template.Register ?? string.Empty,
            template.TemplateText ?? string.Empty,
            template.Explanation ?? string.Empty,
            (template.ReplaceableVariables ?? template.Variables ?? []).Select(item => item ?? string.Empty).ToArray(),
            template.SampleFilledVersion ?? string.Empty,
            (template.LinkedGrammarTopicSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            (template.LinkedWordSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            (template.LinkedExpressionSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            (template.LinkedExerciseSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            template.IsPublished ?? true,
            template.SortOrder ?? 0);

    private static ParsedCulturalNoteModel Map(CulturalNoteDocument note) =>
        new(
            note.Slug ?? string.Empty,
            note.Title ?? string.Empty,
            note.ShortDescription ?? string.Empty,
            note.CefrLevel ?? string.Empty,
            note.Category ?? string.Empty,
            note.Context ?? string.Empty,
            (note.Sections ?? []).Select(item => item ?? string.Empty).ToArray(),
            (note.Examples ?? []).Select(example => new ParsedCulturalNoteExampleModel(
                example.GermanText ?? string.Empty,
                example.Explanation)).ToArray(),
            (note.DoNotes ?? note.Dos ?? []).Select(item => item ?? string.Empty).ToArray(),
            (note.DontNotes ?? note.Donts ?? []).Select(item => item ?? string.Empty).ToArray(),
            note.SensitivityWarning,
            (note.LinkedDialogueSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            (note.LinkedExpressionSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            (note.LinkedWritingTemplateSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            (note.LinkedTalkTopicSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            (note.LinkedCourseLessonSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            note.IsPublished ?? true,
            note.SortOrder ?? 0);

    private static ParsedExamProfileModel Map(ExamProfileDocument profile) =>
        new(
            profile.Key ?? string.Empty,
            profile.DisplayName ?? string.Empty,
            profile.CefrRange ?? string.Empty,
            profile.Description ?? string.Empty,
            profile.IsPublished ?? true,
            profile.SortOrder ?? 0);

    private static ParsedExamPrepUnitModel Map(ExamPrepUnitDocument unit) =>
        new(
            unit.Slug ?? string.Empty,
            unit.ExamProfileKey ?? string.Empty,
            unit.Title ?? string.Empty,
            unit.ShortDescription ?? string.Empty,
            unit.CefrLevel ?? string.Empty,
            unit.ExamSection ?? unit.Section ?? string.Empty,
            unit.TaskType ?? string.Empty,
            unit.SkillFocus ?? string.Empty,
            unit.Explanation ?? string.Empty,
            (unit.StrategyNotes ?? []).Select(item => item ?? string.Empty).ToArray(),
            (unit.Checklist ?? []).Select(item => item ?? string.Empty).ToArray(),
            (unit.LinkedDialogueSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            (unit.LinkedTalkTopicSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            (unit.LinkedGrammarTopicSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            (unit.LinkedExpressionSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            (unit.LinkedWritingTemplateSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            (unit.LinkedExerciseSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            (unit.LinkedCourseLessonSlugs ?? []).Select(item => item ?? string.Empty).ToArray(),
            unit.IsPublished ?? true,
            unit.SortOrder ?? 0);

    private static string SerializeRaw(JsonElement? element) =>
        element.HasValue && element.Value.ValueKind is not JsonValueKind.Undefined and not JsonValueKind.Null
            ? element.Value.GetRawText()
            : "{}";

    private static string? SerializeOptionalArray(JsonElement? element) =>
        element.HasValue && element.Value.ValueKind is JsonValueKind.Array
            ? element.Value.GetRawText()
            : null;

    private static IReadOnlyDictionary<string, string> MapLocalizedTextObject(JsonElement? element)
    {
        if (!element.HasValue || element.Value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        if (element.Value.ValueKind is not JsonValueKind.Object)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase);
        foreach (JsonProperty property in element.Value.EnumerateObject())
        {
            values[property.Name] = property.Value.ValueKind == JsonValueKind.String
                ? property.Value.GetString() ?? string.Empty
                : property.Value.GetRawText();
        }

        return values;
    }

    private static IReadOnlyDictionary<string, string> MapLocalizedBlocks(JsonElement? element)
    {
        if (!element.HasValue || element.Value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        if (element.Value.ValueKind is not JsonValueKind.Object)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase);
        foreach (JsonProperty property in element.Value.EnumerateObject())
        {
            values[property.Name] = property.Value.GetRawText();
        }

        return values;
    }

    private static string GetFallbackLocalizedText(IReadOnlyDictionary<string, string> values)
    {
        if (values.TryGetValue("en", out string? english) && !string.IsNullOrWhiteSpace(english))
        {
            return english;
        }

        return values.Values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }

    private static string ResolveSectionHeading(GrammarSectionDocument section)
    {
        IReadOnlyDictionary<string, string> blocks = MapLocalizedBlocks(section.LocalizedBlocks);
        return string.IsNullOrWhiteSpace(section.SectionKey)
            ? "Grammar section"
            : section.SectionKey.Replace('-', ' ');
    }

    private static string ResolveSectionExplanation(GrammarSectionDocument section)
    {
        IReadOnlyDictionary<string, string> blocks = MapLocalizedBlocks(section.LocalizedBlocks);
        foreach (string rawBlocks in blocks.Values)
        {
            using JsonDocument document = JsonDocument.Parse(rawBlocks);
            foreach (JsonElement block in document.RootElement.EnumerateArray())
            {
                if (block.TryGetProperty("text", out JsonElement text) && text.ValueKind == JsonValueKind.String)
                {
                    string? value = text.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }
        }

        return "Grammar section";
    }

    private static ParsedGrammarTextItemModel[] MapGrammarTextItems(
        GrammarTextItemDocument[]? textItems,
        JsonElement? localizedItems)
    {
        ParsedGrammarTextItemModel[] oldShape = (textItems ?? []).Select(item => new ParsedGrammarTextItemModel(
            item.Text
                ?? GetFallbackLocalizedText(MapLocalizedTextObject(item.LocalizedText))
                ?? string.Empty,
            item.LocalizedText.HasValue
                ? MapFlexibleTranslations(null, item.LocalizedText)
                : MapTranslations(item.Translations),
            item.SortOrder ?? 0)).ToArray();

        IReadOnlyDictionary<string, string[]> localized = MapLocalizedStringArrays(localizedItems);
        if (localized.Count == 0)
        {
            return oldShape;
        }

        int count = localized.Values.Select(values => values.Length).DefaultIfEmpty(0).Max();
        ParsedGrammarTextItemModel[] result = new ParsedGrammarTextItemModel[count];
        for (int index = 0; index < count; index++)
        {
            ParsedContentMeaningModel[] translations = localized
                .Where(pair => index < pair.Value.Length)
                .Select(pair => new ParsedContentMeaningModel(pair.Key, pair.Value[index]))
                .ToArray();

            string text = translations.FirstOrDefault(item => string.Equals(item.Language, "en", StringComparison.OrdinalIgnoreCase))?.Text
                ?? translations.FirstOrDefault()?.Text
                ?? string.Empty;
            result[index] = new ParsedGrammarTextItemModel(text, translations, (index + 1) * 10);
        }

        return result;
    }

    private static IReadOnlyDictionary<string, string[]> MapLocalizedStringArrays(JsonElement? element)
    {
        if (!element.HasValue || element.Value.ValueKind is not JsonValueKind.Object)
        {
            return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        }

        Dictionary<string, string[]> values = new(StringComparer.OrdinalIgnoreCase);
        foreach (JsonProperty property in element.Value.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Array)
            {
                values[property.Name] = property.Value.EnumerateArray()
                    .Where(item => item.ValueKind == JsonValueKind.String)
                    .Select(item => item.GetString() ?? string.Empty)
                    .ToArray();
            }
        }

        return values;
    }

    private static ParsedContentMeaningModel[] MapFlexibleTranslations(JsonElement? translations, JsonElement? localizedObject = null)
    {
        if (localizedObject.HasValue && localizedObject.Value.ValueKind == JsonValueKind.Object)
        {
            return MapLocalizedTextObject(localizedObject)
                .Select(pair => new ParsedContentMeaningModel(pair.Key, pair.Value))
                .ToArray();
        }

        if (!translations.HasValue || translations.Value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return [];
        }

        if (translations.Value.ValueKind == JsonValueKind.Object)
        {
            return MapLocalizedTextObject(translations)
                .Select(pair => new ParsedContentMeaningModel(pair.Key, pair.Value))
                .ToArray();
        }

        if (translations.Value.ValueKind == JsonValueKind.Array)
        {
            return translations.Value.Deserialize<ContentMeaningDocument[]>(SerializerOptions) is { } items
                ? MapTranslations(items)
                : [];
        }

        return [];
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

        public string?[]? TargetLanguages { get; set; }

        public ContentEntryDocument[]? Entries { get; set; }

        public ContentLabelDefinitionDocument[]? Labels { get; set; }

        public ContentCollectionDocument[]? Collections { get; set; }

        public DialogueLessonDocument[]? Dialogues { get; set; }

        public TalkTopicDocument[]? TalkTopics { get; set; }

        public GrammarTopicDocument[]? GrammarTopics { get; set; }

        public ExpressionEntryDocument[]? ExpressionEntries { get; set; }

        public ExerciseDocument[]? Exercises { get; set; }

        public ExerciseSetDocument[]? ExerciseSets { get; set; }

        public CoursePathDocument[]? CoursePaths { get; set; }

        public CourseModuleDocument[]? CourseModules { get; set; }

        public CourseLessonDocument[]? CourseLessons { get; set; }

        public WritingTemplateDocument[]? WritingTemplates { get; set; }

        public CulturalNoteDocument[]? CulturalNotes { get; set; }

        public ExamProfileDocument[]? ExamProfiles { get; set; }

        public ExamPrepUnitDocument[]? ExamPrepUnits { get; set; }

        public ConversationStarterPackDocument[]? ConversationStarterPacks { get; set; }

        public EventPreparationPackDocument[]? EventPreparationPacks { get; set; }

        public RoleplayScenarioDocument[]? RoleplayScenarios { get; set; }
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

    private sealed class DialogueLessonDocument
    {
        public string? Slug { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? LearnerGoal { get; set; }

        public string? CefrLevel { get; set; }

        public string? Category { get; set; }

        public string?[]? Topics { get; set; }

        public string?[]? ExamProfiles { get; set; }

        public string?[]? SkillFocus { get; set; }

        public string? TaskType { get; set; }

        public string? InteractionMode { get; set; }

        public string? Register { get; set; }

        public string?[]? SpeakingFunctions { get; set; }

        public int? EstimatedPracticeMinutes { get; set; }

        public string? DifficultyNote { get; set; }

        public string? ExamRelevance { get; set; }

        public DialogueUsefulWordDocument[]? UsefulWords { get; set; }

        public DialogueSpeakingPromptDocument[]? SpeakingPrompts { get; set; }

        public int? SortOrder { get; set; }

        public DialogueTurnDocument[]? DialogueTurns { get; set; }

        public DialoguePhraseDocument[]? UsefulPhrases { get; set; }

        public DialogueQuestionDocument[]? Questions { get; set; }
    }

    private sealed class DialogueUsefulWordDocument
    {
        public string? Lemma { get; set; }

        public string? WordSlug { get; set; }

        public string? CefrLevel { get; set; }

        public int? SortOrder { get; set; }
    }

    private sealed class DialogueSpeakingPromptDocument
    {
        public string? PromptType { get; set; }

        public string? Prompt { get; set; }

        public ContentMeaningDocument[]? Translations { get; set; }

        public int? SortOrder { get; set; }
    }

    private sealed class DialogueTurnDocument
    {
        public string? SpeakerRole { get; set; }

        public string? BaseText { get; set; }

        public ContentMeaningDocument[]? Translations { get; set; }
    }

    private sealed class DialoguePhraseDocument
    {
        public string? BaseText { get; set; }

        public ContentMeaningDocument[]? Translations { get; set; }

        public string? UsageNote { get; set; }
    }

    private sealed class DialogueQuestionDocument
    {
        public string? Prompt { get; set; }

        public ContentMeaningDocument[]? Translations { get; set; }

        public DialogueAnswerDocument[]? Answers { get; set; }
    }

    private sealed class DialogueAnswerDocument
    {
        public string? Text { get; set; }

        public ContentMeaningDocument[]? Translations { get; set; }

        public bool? IsCorrect { get; set; }

        public string? Feedback { get; set; }
    }

    private sealed class TalkTopicDocument
    {
        public string? Slug { get; set; }

        public string? TopicGroupKey { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? CefrLevel { get; set; }

        public string? Category { get; set; }

        public string?[]? Topics { get; set; }

        public string? ContentType { get; set; }

        public int? EstimatedReadingMinutes { get; set; }

        public int? EstimatedDiscussionMinutes { get; set; }

        public bool? IsSensitive { get; set; }

        public bool? RecommendedForModeratedGroupsOnly { get; set; }

        public string? SensitivityNote { get; set; }

        public TalkTopicArticleDocument? Article { get; set; }

        public TalkTopicQuestionDocument[]? WarmupQuestions { get; set; }

        public TalkTopicDiscussionQuestionDocument[]? DiscussionQuestions { get; set; }

        public TalkTopicVocabularyItemDocument[]? VocabularyItems { get; set; }

        public string?[]? SpeakingGoals { get; set; }

        public int? SortOrder { get; set; }

        public bool? IsPublished { get; set; }
    }

    private sealed class TalkTopicArticleDocument
    {
        public string? BaseText { get; set; }

        public ContentMeaningDocument[]? Translations { get; set; }
    }

    private sealed class TalkTopicQuestionDocument
    {
        public string? Prompt { get; set; }

        public ContentMeaningDocument[]? Translations { get; set; }

        public int? SortOrder { get; set; }
    }

    private sealed class TalkTopicDiscussionQuestionDocument
    {
        public string? Prompt { get; set; }

        public string? QuestionType { get; set; }

        public ContentMeaningDocument[]? Translations { get; set; }

        public int? SortOrder { get; set; }
    }

    private sealed class TalkTopicVocabularyItemDocument
    {
        public string? Lemma { get; set; }

        public string? WordSlug { get; set; }

        public string? CefrLevel { get; set; }

        public int? SortOrder { get; set; }
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

        public string?[]? LinkedDialogueSlugs { get; set; }

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

        public string?[]? LinkedDialogueSlugs { get; set; }

        public EventPreparationVocabularyReferenceDocument[]? LinkedVocabulary { get; set; }

        public string?[]? LinkedConversationStarterPackSlugs { get; set; }

        public string?[]? OpeningPrompts { get; set; }

        public string?[]? RoleplayPrompts { get; set; }

        public string?[]? ReviewPrompts { get; set; }
    }

    private sealed class GrammarTopicDocument
    {
        public string? Slug { get; set; }
        public int? ContentRevision { get; set; }
        public string? Title { get; set; }
        public JsonElement? TitleLocalized { get; set; }
        public string? ShortDescription { get; set; }
        public JsonElement? ShortDescriptionLocalized { get; set; }
        public string? CefrLevel { get; set; }
        public string? GrammarCategory { get; set; }
        public string?[]? Topics { get; set; }
        public bool? IsPublished { get; set; }
        public int? SortOrder { get; set; }
        public GrammarSectionDocument[]? Sections { get; set; }
        public GrammarExampleDocument[]? Examples { get; set; }
        public GrammarTextItemDocument[]? RuleSummaries { get; set; }
        public GrammarCommonMistakeDocument[]? CommonMistakes { get; set; }
        public GrammarTextItemDocument[]? ExceptionNotes { get; set; }
        public string?[]? PrerequisiteSlugs { get; set; }
        public string?[]? RelatedTopicSlugs { get; set; }
        public GrammarLinkedWordDocument[]? LinkedWords { get; set; }
        public string?[]? LinkedDialogueSlugs { get; set; }
        public string?[]? LinkedTalkTopicSlugs { get; set; }
        public string?[]? LinkedExerciseSlugs { get; set; }
        public JsonElement? RuleSummariesLocalized { get; set; }
        public JsonElement? ImageSlots { get; set; }
    }

    private sealed class GrammarSectionDocument
    {
        public string? SectionKey { get; set; }
        public string? Heading { get; set; }
        public string? Explanation { get; set; }
        public GrammarSectionTranslationDocument[]? Translations { get; set; }
        public JsonElement? LocalizedBlocks { get; set; }
        public int? SortOrder { get; set; }
    }

    private sealed class GrammarSectionTranslationDocument
    {
        public string? Language { get; set; }
        public string? Heading { get; set; }
        public string? Text { get; set; }
    }

    private sealed class GrammarExampleDocument
    {
        public string? GermanText { get; set; }
        public string? Note { get; set; }
        public JsonElement? Translations { get; set; }
        public int? SortOrder { get; set; }
    }

    private sealed class GrammarTextItemDocument
    {
        public string? Text { get; set; }
        public JsonElement? LocalizedText { get; set; }
        public ContentMeaningDocument[]? Translations { get; set; }
        public int? SortOrder { get; set; }
    }

    private sealed class GrammarCommonMistakeDocument
    {
        public string? WrongText { get; set; }
        public string? WrongGerman { get; set; }
        public string? CorrectedText { get; set; }
        public string? CorrectGerman { get; set; }
        public string? Explanation { get; set; }
        public JsonElement? ExplanationLocalized { get; set; }
        public JsonElement? Translations { get; set; }
        public int? SortOrder { get; set; }
    }

    private sealed class GrammarLinkedWordDocument
    {
        public string? Lemma { get; set; }
        public string? WordSlug { get; set; }
        public int? SortOrder { get; set; }
    }

    private sealed class ExpressionEntryDocument
    {
        public string? Slug { get; set; }
        public string? ExpressionText { get; set; }
        public string? LiteralMeaningText { get; set; }
        public string? ActualMeaningText { get; set; }
        public string? UsageExplanation { get; set; }
        public string? CefrLevel { get; set; }
        public string? ExpressionType { get; set; }
        public string? Register { get; set; }
        public string? Category { get; set; }
        public string? Context { get; set; }
        public string? Region { get; set; }
        public bool? IsRisky { get; set; }
        public string? MeaningTransparency { get; set; }
        public string? TeachingReason { get; set; }
        public string? SafetyRating { get; set; }
        public int? MinimumAge { get; set; }
        public bool? RequiresAdultAccess { get; set; }
        public string? AdultContentCategory { get; set; }
        public string? SensitiveContentKind { get; set; }
        public bool? RequiresSensitiveOptIn { get; set; }
        public bool? RequiresVerifiedAdult { get; set; }
        public string? UsagePolicy { get; set; }
        public string?[]? Topics { get; set; }
        public bool? IsPublished { get; set; }
        public int? SortOrder { get; set; }
        public ExpressionMeaningDocument[]? Meanings { get; set; }
        public ExpressionExampleDocument[]? Examples { get; set; }
        public ExpressionWarningDocument[]? Warnings { get; set; }
        public ExpressionWarningDocument[]? ContentWarnings { get; set; }
        public ExpressionLinkedWordDocument[]? LinkedWords { get; set; }
        public string?[]? RelatedExpressionSlugs { get; set; }
        public string?[]? LinkedExerciseSlugs { get; set; }
    }

    private sealed class ExpressionMeaningDocument
    {
        public string? Language { get; set; }
        public string? Text { get; set; }
        public string? ActualMeaningText { get; set; }
        public string? LiteralMeaningText { get; set; }
        public string? UsageExplanation { get; set; }
    }

    private sealed class ExpressionExampleDocument
    {
        public string? GermanText { get; set; }
        public string? Note { get; set; }
        public ContentMeaningDocument[]? Translations { get; set; }
        public int? SortOrder { get; set; }
    }

    private sealed class ExpressionWarningDocument
    {
        public string? WarningType { get; set; }
        public string? Text { get; set; }
        public ContentMeaningDocument[]? Translations { get; set; }
    }

    private sealed class ExpressionLinkedWordDocument
    {
        public string? Lemma { get; set; }
        public string? WordSlug { get; set; }
        public int? SortOrder { get; set; }
    }

    private sealed class EventPreparationVocabularyReferenceDocument
    {
        public string? Word { get; set; }

        public string? PartOfSpeech { get; set; }

        public string? CefrLevel { get; set; }
    }

    private sealed class RoleplayScenarioDocument
    {
        public string? Slug { get; set; }
        public string? LinkedDialogueSlug { get; set; }
        public string? ScenarioSlug { get; set; }
        public string? Title { get; set; }
        public ContentMeaningDocument[]? TitleTranslations { get; set; }
        public string? Description { get; set; }
        public ContentMeaningDocument[]? DescriptionTranslations { get; set; }
        public string? LearnerGoal { get; set; }
        public ContentMeaningDocument[]? LearnerGoalTranslations { get; set; }
        public string? CefrLevel { get; set; }
        public string? Category { get; set; }
        public string?[]? Topics { get; set; }
        public string?[]? ExamProfiles { get; set; }
        public string?[]? SkillFocus { get; set; }
        public string? TaskType { get; set; }
        public string? InteractionMode { get; set; }
        public string? Register { get; set; }
        public int? EstimatedPracticeMinutes { get; set; }
        public RoleplayRoleDocument[]? Roles { get; set; }
        public RoleplayTurnDocument[]? Turns { get; set; }
        public RoleplayAnswerChoiceGroupDocument[]? AnswerChoices { get; set; }
        public RoleplayStaticFeedbackDocument[]? StaticFeedback { get; set; }
        public RoleplayImageSlotDocument[]? ImageSlots { get; set; }
        public bool? IsPublished { get; set; }
        public int? SortOrder { get; set; }
    }

    private sealed class RoleplayRoleDocument
    {
        public string? RoleKey { get; set; }
        public string? DisplayName { get; set; }
        public ContentMeaningDocument[]? Translations { get; set; }
    }

    private sealed class RoleplayTurnDocument
    {
        public int? SortOrder { get; set; }
        public string? SpeakerRole { get; set; }
        public string? BaseText { get; set; }
        public ContentMeaningDocument[]? Translations { get; set; }
        public string? Function { get; set; }
        public string? ToneNote { get; set; }
        public string? ExpectedLearnerAction { get; set; }
    }

    private sealed class RoleplayAnswerChoiceGroupDocument
    {
        public int? TurnSortOrder { get; set; }
        public RoleplayAnswerChoiceDocument[]? Choices { get; set; }
    }

    private sealed class RoleplayAnswerChoiceDocument
    {
        public string? Id { get; set; }
        public string? Text { get; set; }
        public ContentMeaningDocument[]? Translations { get; set; }
        public bool? IsCorrect { get; set; }
        public string? Feedback { get; set; }
        public ContentMeaningDocument[]? FeedbackTranslations { get; set; }
        public string? ExplanationKey { get; set; }
    }

    private sealed class RoleplayStaticFeedbackDocument
    {
        public int? TurnSortOrder { get; set; }
        public string? FeedbackType { get; set; }
        public string? Text { get; set; }
        public ContentMeaningDocument[]? Translations { get; set; }
    }

    private sealed class RoleplayImageSlotDocument
    {
        public string? SlotKey { get; set; }
        public string? Placement { get; set; }
        public string? Purpose { get; set; }
        public string? AltText { get; set; }
        public ContentMeaningDocument[]? AltTextTranslations { get; set; }
        public string? ImagePrompt { get; set; }
        public string? AssetPath { get; set; }
        public bool? IsRequired { get; set; }
    }

    private sealed class ExerciseDocument
    {
        public string? Slug { get; set; }
        public string? Title { get; set; }
        public string? Instruction { get; set; }
        public string? CefrLevel { get; set; }
        public string? ExerciseType { get; set; }
        public string? TargetSkill { get; set; }
        public string? OwnerType { get; set; }
        public string? OwnerSlug { get; set; }
        public JsonElement? Prompt { get; set; }
        public JsonElement? AnswerKey { get; set; }
        public string? CorrectExplanation { get; set; }
        public string? IncorrectExplanation { get; set; }
        public string? Hint { get; set; }
        public string? CommonMistakeNote { get; set; }
        public bool? IsPublished { get; set; }
        public int? SortOrder { get; set; }
    }

    private sealed class ExerciseSetDocument
    {
        public string? Slug { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? CefrLevel { get; set; }
        public string? OwnerType { get; set; }
        public string? OwnerSlug { get; set; }
        public string?[]? ExerciseSlugs { get; set; }
        public bool? IsPublished { get; set; }
        public int? SortOrder { get; set; }
    }

    private sealed class CoursePathDocument
    {
        public string? Slug { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? CefrLevel { get; set; }
        public string? CefrRange { get; set; }
        public bool? IsPublished { get; set; }
        public int? SortOrder { get; set; }
    }

    private sealed class CourseModuleDocument
    {
        public string? Slug { get; set; }
        public string? CoursePathSlug { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? ModuleNumber { get; set; }
        public string? CefrLevel { get; set; }
        public bool? IsPublished { get; set; }
        public int? SortOrder { get; set; }
    }

    private sealed class CourseLessonDocument
    {
        public string? Slug { get; set; }
        public string? CoursePathSlug { get; set; }
        public string? ModuleSlug { get; set; }
        public int? LessonNumber { get; set; }
        public string? Title { get; set; }
        public string? ShortDescription { get; set; }
        public string? Narrative { get; set; }
        public string? CefrLevel { get; set; }
        public int? EstimatedMinutes { get; set; }
        public string?[]? LearningGoals { get; set; }
        public string?[]? PrerequisiteLessonSlugs { get; set; }
        public string? NextLessonSlug { get; set; }
        public string?[]? LinkedGrammarTopicSlugs { get; set; }
        public string?[]? LinkedWordSlugs { get; set; }
        public string?[]? LinkedExpressionSlugs { get; set; }
        public string?[]? LinkedDialogueSlugs { get; set; }
        public string?[]? LinkedTalkTopicSlugs { get; set; }
        public string?[]? LinkedExerciseSetSlugs { get; set; }
        public string?[]? LinkedExamPrepSlugs { get; set; }
        public string? ReviewSummary { get; set; }
        public string? HomeworkTask { get; set; }
        public bool? IsPublished { get; set; }
        public int? SortOrder { get; set; }
    }

    private sealed class WritingTemplateDocument
    {
        public string? Slug { get; set; }
        public string? Title { get; set; }
        public string? ShortDescription { get; set; }
        public string? CefrLevel { get; set; }
        public string? Category { get; set; }
        public string? Situation { get; set; }
        public string? Register { get; set; }
        public string? TemplateText { get; set; }
        public string? Explanation { get; set; }
        public string?[]? ReplaceableVariables { get; set; }
        public string?[]? Variables { get; set; }
        public string? SampleFilledVersion { get; set; }
        public string?[]? LinkedGrammarTopicSlugs { get; set; }
        public string?[]? LinkedWordSlugs { get; set; }
        public string?[]? LinkedExpressionSlugs { get; set; }
        public string?[]? LinkedExerciseSlugs { get; set; }
        public bool? IsPublished { get; set; }
        public int? SortOrder { get; set; }
    }

    private sealed class CulturalNoteDocument
    {
        public string? Slug { get; set; }
        public string? Title { get; set; }
        public string? ShortDescription { get; set; }
        public string? CefrLevel { get; set; }
        public string? Category { get; set; }
        public string? Context { get; set; }
        public string?[]? Sections { get; set; }
        public CulturalNoteExampleDocument[]? Examples { get; set; }
        public string?[]? DoNotes { get; set; }
        public string?[]? Dos { get; set; }
        public string?[]? DontNotes { get; set; }
        public string?[]? Donts { get; set; }
        public string? SensitivityWarning { get; set; }
        public string?[]? LinkedDialogueSlugs { get; set; }
        public string?[]? LinkedExpressionSlugs { get; set; }
        public string?[]? LinkedWritingTemplateSlugs { get; set; }
        public string?[]? LinkedTalkTopicSlugs { get; set; }
        public string?[]? LinkedCourseLessonSlugs { get; set; }
        public bool? IsPublished { get; set; }
        public int? SortOrder { get; set; }
    }

    private sealed class CulturalNoteExampleDocument
    {
        public string? GermanText { get; set; }
        public string? Explanation { get; set; }
    }

    private sealed class ExamProfileDocument
    {
        public string? Key { get; set; }
        public string? DisplayName { get; set; }
        public string? CefrRange { get; set; }
        public string? Description { get; set; }
        public bool? IsPublished { get; set; }
        public int? SortOrder { get; set; }
    }

    private sealed class ExamPrepUnitDocument
    {
        public string? Slug { get; set; }
        public string? ExamProfileKey { get; set; }
        public string? Title { get; set; }
        public string? ShortDescription { get; set; }
        public string? CefrLevel { get; set; }
        public string? ExamSection { get; set; }
        public string? Section { get; set; }
        public string? TaskType { get; set; }
        public string? SkillFocus { get; set; }
        public string? Explanation { get; set; }
        public string?[]? StrategyNotes { get; set; }
        public string?[]? Checklist { get; set; }
        public string?[]? LinkedDialogueSlugs { get; set; }
        public string?[]? LinkedTalkTopicSlugs { get; set; }
        public string?[]? LinkedGrammarTopicSlugs { get; set; }
        public string?[]? LinkedExpressionSlugs { get; set; }
        public string?[]? LinkedWritingTemplateSlugs { get; set; }
        public string?[]? LinkedExerciseSlugs { get; set; }
        public string?[]? LinkedCourseLessonSlugs { get; set; }
        public bool? IsPublished { get; set; }
        public int? SortOrder { get; set; }
    }
}

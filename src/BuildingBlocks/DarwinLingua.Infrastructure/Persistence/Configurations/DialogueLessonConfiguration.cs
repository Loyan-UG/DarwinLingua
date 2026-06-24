using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class DialogueLessonConfiguration : IEntityTypeConfiguration<DialogueLesson>
{
    public void Configure(EntityTypeBuilder<DialogueLesson> builder)
    {
        builder.ToTable("DialogueLessons");
        builder.HasKey(lesson => lesson.Id);
        builder.Property(lesson => lesson.Id).ValueGeneratedNever();
        builder.Property(lesson => lesson.TargetLearningLanguageCode)
            .HasMaxLength(16)
            .HasDefaultValue(ContentLanguageRequirements.DefaultTargetLearningLanguageCode)
            .IsRequired();
        builder.Property(lesson => lesson.Slug).HasMaxLength(128).IsRequired();
        builder.Property(lesson => lesson.Title).HasMaxLength(256).IsRequired();
        builder.Property(lesson => lesson.Description).HasMaxLength(4000).IsRequired();
        builder.Property(lesson => lesson.LearnerGoal).HasMaxLength(1024).IsRequired();
        builder.Property(lesson => lesson.CefrLevel).HasConversion<string>().HasMaxLength(8).IsRequired();
        builder.Property(lesson => lesson.Category).HasMaxLength(128).IsRequired();
        builder.Property(lesson => lesson.TaskType).HasMaxLength(128).IsRequired().HasDefaultValue("exam-roleplay");
        builder.Property(lesson => lesson.InteractionMode).HasMaxLength(128).IsRequired().HasDefaultValue("face-to-face");
        builder.Property(lesson => lesson.Register).HasMaxLength(64).IsRequired().HasDefaultValue("neutral");
        builder.Property(lesson => lesson.EstimatedPracticeMinutes).IsRequired().HasDefaultValue(15);
        builder.Property(lesson => lesson.DifficultyNote).HasMaxLength(1024);
        builder.Property(lesson => lesson.ExamRelevance).HasMaxLength(1024);
        builder.Property(lesson => lesson.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(lesson => lesson.SortOrder).IsRequired();
        builder.Property(lesson => lesson.CreatedAtUtc).IsRequired();
        builder.Property(lesson => lesson.UpdatedAtUtc).IsRequired();
        builder.HasIndex(lesson => new { lesson.TargetLearningLanguageCode, lesson.Slug }).IsUnique();
        builder.HasIndex(lesson => new { lesson.TargetLearningLanguageCode, lesson.CefrLevel }).HasDatabaseName("IX_DialogueLessons_TargetLanguage_CefrLevel");
        builder.HasIndex(lesson => new { lesson.TargetLearningLanguageCode, lesson.Category }).HasDatabaseName("IX_DialogueLessons_TargetLanguage_Category");
        builder.HasIndex(lesson => new { lesson.TargetLearningLanguageCode, lesson.TaskType }).HasDatabaseName("IX_DialogueLessons_TargetLanguage_TaskType");
        builder.HasIndex(lesson => new { lesson.TargetLearningLanguageCode, lesson.InteractionMode }).HasDatabaseName("IX_DialogueLessons_TargetLanguage_InteractionMode");
        builder.HasIndex(lesson => new { lesson.TargetLearningLanguageCode, lesson.Register }).HasDatabaseName("IX_DialogueLessons_TargetLanguage_Register");

        builder.HasMany(lesson => lesson.Topics).WithOne().HasForeignKey(topic => topic.DialogueLessonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(lesson => lesson.ExamProfiles).WithOne().HasForeignKey(profile => profile.DialogueLessonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(lesson => lesson.SkillFocus).WithOne().HasForeignKey(focus => focus.DialogueLessonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(lesson => lesson.SpeakingFunctions).WithOne().HasForeignKey(function => function.DialogueLessonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(lesson => lesson.UsefulWords).WithOne().HasForeignKey(word => word.DialogueLessonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(lesson => lesson.SpeakingPrompts).WithOne().HasForeignKey(prompt => prompt.DialogueLessonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(lesson => lesson.DialogueTurns).WithOne().HasForeignKey(turn => turn.DialogueLessonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(lesson => lesson.UsefulPhrases).WithOne().HasForeignKey(phrase => phrase.DialogueLessonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(lesson => lesson.Questions).WithOne().HasForeignKey(question => question.DialogueLessonId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class DialogueExamProfileConfiguration : IEntityTypeConfiguration<DialogueExamProfile>
{
    public void Configure(EntityTypeBuilder<DialogueExamProfile> builder)
    {
        builder.ToTable("DialogueExamProfiles");
        builder.HasKey(profile => profile.Id);
        builder.Property(profile => profile.Id).ValueGeneratedNever();
        builder.Property(profile => profile.DialogueLessonId).IsRequired();
        builder.Property(profile => profile.ExamProfile).HasMaxLength(128).IsRequired();
        builder.Property(profile => profile.SortOrder).IsRequired();
        builder.Property(profile => profile.CreatedAtUtc).IsRequired();
        builder.HasIndex(profile => new { profile.DialogueLessonId, profile.ExamProfile }).IsUnique();
        builder.HasIndex(profile => profile.ExamProfile).HasDatabaseName("IX_DialogueExamProfiles_ExamProfile");
    }
}

internal sealed class DialogueSkillFocusConfiguration : IEntityTypeConfiguration<DialogueSkillFocus>
{
    public void Configure(EntityTypeBuilder<DialogueSkillFocus> builder)
    {
        builder.ToTable("DialogueSkillFocus");
        builder.HasKey(focus => focus.Id);
        builder.Property(focus => focus.Id).ValueGeneratedNever();
        builder.Property(focus => focus.DialogueLessonId).IsRequired();
        builder.Property(focus => focus.SkillFocus).HasMaxLength(128).IsRequired();
        builder.Property(focus => focus.SortOrder).IsRequired();
        builder.Property(focus => focus.CreatedAtUtc).IsRequired();
        builder.HasIndex(focus => new { focus.DialogueLessonId, focus.SkillFocus }).IsUnique();
        builder.HasIndex(focus => focus.SkillFocus).HasDatabaseName("IX_DialogueSkillFocus_SkillFocus");
    }
}

internal sealed class DialogueSpeakingFunctionConfiguration : IEntityTypeConfiguration<DialogueSpeakingFunction>
{
    public void Configure(EntityTypeBuilder<DialogueSpeakingFunction> builder)
    {
        builder.ToTable("DialogueSpeakingFunctions");
        builder.HasKey(function => function.Id);
        builder.Property(function => function.Id).ValueGeneratedNever();
        builder.Property(function => function.DialogueLessonId).IsRequired();
        builder.Property(function => function.SpeakingFunction).HasMaxLength(128).IsRequired();
        builder.Property(function => function.SortOrder).IsRequired();
        builder.Property(function => function.CreatedAtUtc).IsRequired();
        builder.HasIndex(function => new { function.DialogueLessonId, function.SpeakingFunction }).IsUnique();
    }
}

internal sealed class DialogueUsefulWordConfiguration : IEntityTypeConfiguration<DialogueUsefulWord>
{
    public void Configure(EntityTypeBuilder<DialogueUsefulWord> builder)
    {
        builder.ToTable("DialogueUsefulWords");
        builder.HasKey(word => word.Id);
        builder.Property(word => word.Id).ValueGeneratedNever();
        builder.Property(word => word.DialogueLessonId).IsRequired();
        builder.Property(word => word.Lemma).HasMaxLength(256).IsRequired();
        builder.Property(word => word.WordSlug).HasMaxLength(128);
        builder.Property(word => word.CefrLevel).HasConversion<string>().HasMaxLength(8);
        builder.Property(word => word.SortOrder).IsRequired();
        builder.Property(word => word.CreatedAtUtc).IsRequired();
        builder.HasIndex(word => new { word.DialogueLessonId, word.SortOrder }).IsUnique();
        builder.HasIndex(word => word.WordSlug).HasDatabaseName("IX_DialogueUsefulWords_WordSlug");
    }
}

internal sealed class DialogueSpeakingPromptConfiguration : IEntityTypeConfiguration<DialogueSpeakingPrompt>
{
    public void Configure(EntityTypeBuilder<DialogueSpeakingPrompt> builder)
    {
        builder.ToTable("DialogueSpeakingPrompts");
        builder.HasKey(prompt => prompt.Id);
        builder.Property(prompt => prompt.Id).ValueGeneratedNever();
        builder.Property(prompt => prompt.DialogueLessonId).IsRequired();
        builder.Property(prompt => prompt.SortOrder).IsRequired();
        builder.Property(prompt => prompt.PromptType).HasMaxLength(128).IsRequired();
        builder.Property(prompt => prompt.Prompt).HasMaxLength(2000).IsRequired();
        builder.Property(prompt => prompt.CreatedAtUtc).IsRequired();
        builder.Property(prompt => prompt.UpdatedAtUtc).IsRequired();
        builder.HasIndex(prompt => new { prompt.DialogueLessonId, prompt.SortOrder }).IsUnique();
        builder.HasMany(prompt => prompt.Translations).WithOne().HasForeignKey(translation => translation.OwnerId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class DialogueLessonTopicConfiguration : IEntityTypeConfiguration<DialogueLessonTopic>
{
    public void Configure(EntityTypeBuilder<DialogueLessonTopic> builder)
    {
        builder.ToTable("DialogueLessonTopics");
        builder.HasKey(topic => topic.Id);
        builder.Property(topic => topic.Id).ValueGeneratedNever();
        builder.Property(topic => topic.DialogueLessonId).IsRequired();
        builder.Property(topic => topic.TopicId).IsRequired();
        builder.Property(topic => topic.IsPrimary).IsRequired();
        builder.Property(topic => topic.CreatedAtUtc).IsRequired();
        builder.HasIndex(topic => new { topic.DialogueLessonId, topic.TopicId }).IsUnique();
        builder.HasIndex(topic => topic.TopicId).HasDatabaseName("IX_DialogueLessonTopics_TopicId");
        builder.HasIndex(topic => topic.DialogueLessonId).HasDatabaseName("IX_DialogueLessonTopics_PrimaryPerLesson").IsUnique().HasFilter($"\"{nameof(DialogueLessonTopic.IsPrimary)}\"");
        builder.HasOne<Topic>().WithMany().HasForeignKey(topic => topic.TopicId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class DialogueTurnConfiguration : IEntityTypeConfiguration<DialogueTurn>
{
    public void Configure(EntityTypeBuilder<DialogueTurn> builder)
    {
        builder.ToTable("DialogueTurns");
        builder.HasKey(turn => turn.Id);
        builder.Property(turn => turn.Id).ValueGeneratedNever();
        builder.Property(turn => turn.DialogueLessonId).IsRequired();
        builder.Property(turn => turn.SortOrder).IsRequired();
        builder.Property(turn => turn.SpeakerRole).HasMaxLength(64).IsRequired();
        builder.Property(turn => turn.BaseText).HasMaxLength(2000).IsRequired();
        builder.Property(turn => turn.CreatedAtUtc).IsRequired();
        builder.Property(turn => turn.UpdatedAtUtc).IsRequired();
        builder.HasIndex(turn => new { turn.DialogueLessonId, turn.SortOrder }).IsUnique();
        builder.HasMany(turn => turn.Translations).WithOne().HasForeignKey(translation => translation.OwnerId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class DialoguePhraseConfiguration : IEntityTypeConfiguration<DialoguePhrase>
{
    public void Configure(EntityTypeBuilder<DialoguePhrase> builder)
    {
        builder.ToTable("DialoguePhrases");
        builder.HasKey(phrase => phrase.Id);
        builder.Property(phrase => phrase.Id).ValueGeneratedNever();
        builder.Property(phrase => phrase.DialogueLessonId).IsRequired();
        builder.Property(phrase => phrase.SortOrder).IsRequired();
        builder.Property(phrase => phrase.BaseText).HasMaxLength(1024).IsRequired();
        builder.Property(phrase => phrase.UsageNote).HasMaxLength(1024);
        builder.Property(phrase => phrase.CreatedAtUtc).IsRequired();
        builder.Property(phrase => phrase.UpdatedAtUtc).IsRequired();
        builder.HasIndex(phrase => new { phrase.DialogueLessonId, phrase.SortOrder }).IsUnique();
        builder.HasMany(phrase => phrase.Translations).WithOne().HasForeignKey(translation => translation.OwnerId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class DialogueQuestionConfiguration : IEntityTypeConfiguration<DialogueQuestion>
{
    public void Configure(EntityTypeBuilder<DialogueQuestion> builder)
    {
        builder.ToTable("DialogueQuestions");
        builder.HasKey(question => question.Id);
        builder.Property(question => question.Id).ValueGeneratedNever();
        builder.Property(question => question.DialogueLessonId).IsRequired();
        builder.Property(question => question.SortOrder).IsRequired();
        builder.Property(question => question.Prompt).HasMaxLength(1024).IsRequired();
        builder.Property(question => question.CreatedAtUtc).IsRequired();
        builder.Property(question => question.UpdatedAtUtc).IsRequired();
        builder.HasIndex(question => new { question.DialogueLessonId, question.SortOrder }).IsUnique();
        builder.HasMany(question => question.Translations).WithOne().HasForeignKey(translation => translation.OwnerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(question => question.Answers).WithOne().HasForeignKey(answer => answer.DialogueQuestionId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class DialogueAnswerConfiguration : IEntityTypeConfiguration<DialogueAnswer>
{
    public void Configure(EntityTypeBuilder<DialogueAnswer> builder)
    {
        builder.ToTable("DialogueAnswers");
        builder.HasKey(answer => answer.Id);
        builder.Property(answer => answer.Id).ValueGeneratedNever();
        builder.Property(answer => answer.DialogueQuestionId).IsRequired();
        builder.Property(answer => answer.SortOrder).IsRequired();
        builder.Property(answer => answer.Text).HasMaxLength(1024).IsRequired();
        builder.Property(answer => answer.IsCorrect).IsRequired();
        builder.Property(answer => answer.Feedback).HasMaxLength(1024);
        builder.Property(answer => answer.CreatedAtUtc).IsRequired();
        builder.Property(answer => answer.UpdatedAtUtc).IsRequired();
        builder.HasIndex(answer => new { answer.DialogueQuestionId, answer.SortOrder }).IsUnique();
        builder.HasMany(answer => answer.Translations).WithOne().HasForeignKey(translation => translation.OwnerId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal abstract class DialogueTranslationConfigurationBase<TTranslation>
    where TTranslation : DialogueTranslationBase
{
    protected static void ConfigureBase(EntityTypeBuilder<TTranslation> builder)
    {
        builder.HasKey(translation => translation.Id);
        builder.Property(translation => translation.Id).ValueGeneratedNever();
        builder.Property(translation => translation.OwnerId).IsRequired();
        builder.Property(translation => translation.LanguageCode)
            .HasConversion(languageCode => languageCode.Value, value => LanguageCode.From(value))
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(translation => translation.Text).HasMaxLength(2000).IsRequired();
        builder.Property(translation => translation.CreatedAtUtc).IsRequired();
        builder.Property(translation => translation.UpdatedAtUtc).IsRequired();
        builder.HasIndex(translation => new { translation.OwnerId, translation.LanguageCode }).IsUnique();
    }
}

internal sealed class DialogueTurnTranslationConfiguration :
    DialogueTranslationConfigurationBase<DialogueTurnTranslation>,
    IEntityTypeConfiguration<DialogueTurnTranslation>
{
    public void Configure(EntityTypeBuilder<DialogueTurnTranslation> builder)
    {
        ConfigureBase(builder);
        builder.ToTable("DialogueTurnTranslations");
    }
}

internal sealed class DialoguePhraseTranslationConfiguration :
    DialogueTranslationConfigurationBase<DialoguePhraseTranslation>,
    IEntityTypeConfiguration<DialoguePhraseTranslation>
{
    public void Configure(EntityTypeBuilder<DialoguePhraseTranslation> builder)
    {
        ConfigureBase(builder);
        builder.ToTable("DialoguePhraseTranslations");
    }
}

internal sealed class DialogueQuestionTranslationConfiguration :
    DialogueTranslationConfigurationBase<DialogueQuestionTranslation>,
    IEntityTypeConfiguration<DialogueQuestionTranslation>
{
    public void Configure(EntityTypeBuilder<DialogueQuestionTranslation> builder)
    {
        ConfigureBase(builder);
        builder.ToTable("DialogueQuestionTranslations");
    }
}

internal sealed class DialogueAnswerTranslationConfiguration :
    DialogueTranslationConfigurationBase<DialogueAnswerTranslation>,
    IEntityTypeConfiguration<DialogueAnswerTranslation>
{
    public void Configure(EntityTypeBuilder<DialogueAnswerTranslation> builder)
    {
        ConfigureBase(builder);
        builder.ToTable("DialogueAnswerTranslations");
    }
}

internal sealed class DialogueSpeakingPromptTranslationConfiguration :
    DialogueTranslationConfigurationBase<DialogueSpeakingPromptTranslation>,
    IEntityTypeConfiguration<DialogueSpeakingPromptTranslation>
{
    public void Configure(EntityTypeBuilder<DialogueSpeakingPromptTranslation> builder)
    {
        ConfigureBase(builder);
        builder.ToTable("DialogueSpeakingPromptTranslations");
    }
}

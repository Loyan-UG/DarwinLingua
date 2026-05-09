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
        builder.Property(lesson => lesson.Slug).HasMaxLength(128).IsRequired();
        builder.Property(lesson => lesson.Title).HasMaxLength(256).IsRequired();
        builder.Property(lesson => lesson.Description).HasMaxLength(4000).IsRequired();
        builder.Property(lesson => lesson.LearnerGoal).HasMaxLength(1024).IsRequired();
        builder.Property(lesson => lesson.CefrLevel).HasConversion<string>().HasMaxLength(8).IsRequired();
        builder.Property(lesson => lesson.Category).HasMaxLength(128).IsRequired();
        builder.Property(lesson => lesson.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(lesson => lesson.SortOrder).IsRequired();
        builder.Property(lesson => lesson.CreatedAtUtc).IsRequired();
        builder.Property(lesson => lesson.UpdatedAtUtc).IsRequired();
        builder.HasIndex(lesson => lesson.Slug).IsUnique();

        builder.HasMany(lesson => lesson.Topics).WithOne().HasForeignKey(topic => topic.DialogueLessonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(lesson => lesson.DialogueTurns).WithOne().HasForeignKey(turn => turn.DialogueLessonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(lesson => lesson.UsefulPhrases).WithOne().HasForeignKey(phrase => phrase.DialogueLessonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(lesson => lesson.Questions).WithOne().HasForeignKey(question => question.DialogueLessonId).OnDelete(DeleteBehavior.Cascade);
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

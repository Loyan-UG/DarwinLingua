using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class ScenarioLessonConfiguration : IEntityTypeConfiguration<ScenarioLesson>
{
    public void Configure(EntityTypeBuilder<ScenarioLesson> builder)
    {
        builder.ToTable("ScenarioLessons");
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

        builder.HasMany(lesson => lesson.Topics).WithOne().HasForeignKey(topic => topic.ScenarioLessonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(lesson => lesson.DialogueTurns).WithOne().HasForeignKey(turn => turn.ScenarioLessonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(lesson => lesson.UsefulPhrases).WithOne().HasForeignKey(phrase => phrase.ScenarioLessonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(lesson => lesson.Questions).WithOne().HasForeignKey(question => question.ScenarioLessonId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ScenarioLessonTopicConfiguration : IEntityTypeConfiguration<ScenarioLessonTopic>
{
    public void Configure(EntityTypeBuilder<ScenarioLessonTopic> builder)
    {
        builder.ToTable("ScenarioLessonTopics");
        builder.HasKey(topic => topic.Id);
        builder.Property(topic => topic.Id).ValueGeneratedNever();
        builder.Property(topic => topic.ScenarioLessonId).IsRequired();
        builder.Property(topic => topic.TopicId).IsRequired();
        builder.Property(topic => topic.IsPrimary).IsRequired();
        builder.Property(topic => topic.CreatedAtUtc).IsRequired();
        builder.HasIndex(topic => new { topic.ScenarioLessonId, topic.TopicId }).IsUnique();
        builder.HasIndex(topic => topic.ScenarioLessonId).HasDatabaseName("IX_ScenarioLessonTopics_PrimaryPerLesson").IsUnique().HasFilter($"\"{nameof(ScenarioLessonTopic.IsPrimary)}\"");
        builder.HasOne<Topic>().WithMany().HasForeignKey(topic => topic.TopicId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ScenarioDialogueTurnConfiguration : IEntityTypeConfiguration<ScenarioDialogueTurn>
{
    public void Configure(EntityTypeBuilder<ScenarioDialogueTurn> builder)
    {
        builder.ToTable("ScenarioDialogueTurns");
        builder.HasKey(turn => turn.Id);
        builder.Property(turn => turn.Id).ValueGeneratedNever();
        builder.Property(turn => turn.ScenarioLessonId).IsRequired();
        builder.Property(turn => turn.SortOrder).IsRequired();
        builder.Property(turn => turn.SpeakerRole).HasMaxLength(64).IsRequired();
        builder.Property(turn => turn.BaseText).HasMaxLength(2000).IsRequired();
        builder.Property(turn => turn.CreatedAtUtc).IsRequired();
        builder.Property(turn => turn.UpdatedAtUtc).IsRequired();
        builder.HasIndex(turn => new { turn.ScenarioLessonId, turn.SortOrder }).IsUnique();
        builder.HasMany(turn => turn.Translations).WithOne().HasForeignKey(translation => translation.OwnerId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ScenarioPhraseConfiguration : IEntityTypeConfiguration<ScenarioPhrase>
{
    public void Configure(EntityTypeBuilder<ScenarioPhrase> builder)
    {
        builder.ToTable("ScenarioPhrases");
        builder.HasKey(phrase => phrase.Id);
        builder.Property(phrase => phrase.Id).ValueGeneratedNever();
        builder.Property(phrase => phrase.ScenarioLessonId).IsRequired();
        builder.Property(phrase => phrase.SortOrder).IsRequired();
        builder.Property(phrase => phrase.BaseText).HasMaxLength(1024).IsRequired();
        builder.Property(phrase => phrase.UsageNote).HasMaxLength(1024);
        builder.Property(phrase => phrase.CreatedAtUtc).IsRequired();
        builder.Property(phrase => phrase.UpdatedAtUtc).IsRequired();
        builder.HasIndex(phrase => new { phrase.ScenarioLessonId, phrase.SortOrder }).IsUnique();
        builder.HasMany(phrase => phrase.Translations).WithOne().HasForeignKey(translation => translation.OwnerId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ScenarioQuestionConfiguration : IEntityTypeConfiguration<ScenarioQuestion>
{
    public void Configure(EntityTypeBuilder<ScenarioQuestion> builder)
    {
        builder.ToTable("ScenarioQuestions");
        builder.HasKey(question => question.Id);
        builder.Property(question => question.Id).ValueGeneratedNever();
        builder.Property(question => question.ScenarioLessonId).IsRequired();
        builder.Property(question => question.SortOrder).IsRequired();
        builder.Property(question => question.Prompt).HasMaxLength(1024).IsRequired();
        builder.Property(question => question.CreatedAtUtc).IsRequired();
        builder.Property(question => question.UpdatedAtUtc).IsRequired();
        builder.HasIndex(question => new { question.ScenarioLessonId, question.SortOrder }).IsUnique();
        builder.HasMany(question => question.Translations).WithOne().HasForeignKey(translation => translation.OwnerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(question => question.Answers).WithOne().HasForeignKey(answer => answer.ScenarioQuestionId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ScenarioAnswerConfiguration : IEntityTypeConfiguration<ScenarioAnswer>
{
    public void Configure(EntityTypeBuilder<ScenarioAnswer> builder)
    {
        builder.ToTable("ScenarioAnswers");
        builder.HasKey(answer => answer.Id);
        builder.Property(answer => answer.Id).ValueGeneratedNever();
        builder.Property(answer => answer.ScenarioQuestionId).IsRequired();
        builder.Property(answer => answer.SortOrder).IsRequired();
        builder.Property(answer => answer.Text).HasMaxLength(1024).IsRequired();
        builder.Property(answer => answer.IsCorrect).IsRequired();
        builder.Property(answer => answer.Feedback).HasMaxLength(1024);
        builder.Property(answer => answer.CreatedAtUtc).IsRequired();
        builder.Property(answer => answer.UpdatedAtUtc).IsRequired();
        builder.HasIndex(answer => new { answer.ScenarioQuestionId, answer.SortOrder }).IsUnique();
        builder.HasMany(answer => answer.Translations).WithOne().HasForeignKey(translation => translation.OwnerId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal abstract class ScenarioTranslationConfigurationBase<TTranslation>
    where TTranslation : ScenarioTranslationBase
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

internal sealed class ScenarioDialogueTurnTranslationConfiguration :
    ScenarioTranslationConfigurationBase<ScenarioDialogueTurnTranslation>,
    IEntityTypeConfiguration<ScenarioDialogueTurnTranslation>
{
    public void Configure(EntityTypeBuilder<ScenarioDialogueTurnTranslation> builder)
    {
        ConfigureBase(builder);
        builder.ToTable("ScenarioDialogueTurnTranslations");
    }
}

internal sealed class ScenarioPhraseTranslationConfiguration :
    ScenarioTranslationConfigurationBase<ScenarioPhraseTranslation>,
    IEntityTypeConfiguration<ScenarioPhraseTranslation>
{
    public void Configure(EntityTypeBuilder<ScenarioPhraseTranslation> builder)
    {
        ConfigureBase(builder);
        builder.ToTable("ScenarioPhraseTranslations");
    }
}

internal sealed class ScenarioQuestionTranslationConfiguration :
    ScenarioTranslationConfigurationBase<ScenarioQuestionTranslation>,
    IEntityTypeConfiguration<ScenarioQuestionTranslation>
{
    public void Configure(EntityTypeBuilder<ScenarioQuestionTranslation> builder)
    {
        ConfigureBase(builder);
        builder.ToTable("ScenarioQuestionTranslations");
    }
}

internal sealed class ScenarioAnswerTranslationConfiguration :
    ScenarioTranslationConfigurationBase<ScenarioAnswerTranslation>,
    IEntityTypeConfiguration<ScenarioAnswerTranslation>
{
    public void Configure(EntityTypeBuilder<ScenarioAnswerTranslation> builder)
    {
        ConfigureBase(builder);
        builder.ToTable("ScenarioAnswerTranslations");
    }
}

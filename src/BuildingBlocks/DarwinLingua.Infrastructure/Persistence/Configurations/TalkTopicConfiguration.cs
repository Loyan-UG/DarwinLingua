using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class TalkTopicConfiguration : IEntityTypeConfiguration<TalkTopic>
{
    public void Configure(EntityTypeBuilder<TalkTopic> builder)
    {
        builder.ToTable("TalkTopics");
        builder.HasKey(topic => topic.Id);
        builder.Property(topic => topic.Id).ValueGeneratedNever();
        builder.Property(topic => topic.Slug).HasMaxLength(128).IsRequired();
        builder.Property(topic => topic.TopicGroupKey).HasMaxLength(128).IsRequired();
        builder.Property(topic => topic.Title).HasMaxLength(256).IsRequired();
        builder.Property(topic => topic.Description).HasMaxLength(4000).IsRequired();
        builder.Property(topic => topic.CefrLevel).HasConversion<string>().HasMaxLength(8).IsRequired();
        builder.Property(topic => topic.Category).HasMaxLength(128).IsRequired();
        builder.Property(topic => topic.ContentType).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(topic => topic.ArticleBaseText).HasMaxLength(12000).IsRequired();
        builder.Property(topic => topic.EstimatedReadingMinutes).IsRequired();
        builder.Property(topic => topic.EstimatedDiscussionMinutes).IsRequired();
        builder.Property(topic => topic.IsSensitive).IsRequired();
        builder.Property(topic => topic.SensitivityNote).HasMaxLength(1024);
        builder.Property(topic => topic.RecommendedForModeratedGroupsOnly).IsRequired();
        builder.Property(topic => topic.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(topic => topic.SortOrder).IsRequired();
        builder.Property(topic => topic.CreatedAtUtc).IsRequired();
        builder.Property(topic => topic.UpdatedAtUtc).IsRequired();
        builder.Ignore(topic => topic.WarmupQuestions);
        builder.Ignore(topic => topic.DiscussionQuestions);
        builder.HasIndex(topic => topic.Slug).IsUnique();
        builder.HasIndex(topic => new { topic.CefrLevel, topic.ContentType, topic.Category });

        builder.HasMany(topic => topic.Topics).WithOne().HasForeignKey(link => link.TalkTopicId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(topic => topic.ArticleTranslations).WithOne().HasForeignKey(translation => translation.OwnerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(topic => topic.Questions).WithOne().HasForeignKey(question => question.TalkTopicId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(topic => topic.VocabularyItems).WithOne().HasForeignKey(item => item.TalkTopicId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(topic => topic.SpeakingGoals).WithOne().HasForeignKey(goal => goal.TalkTopicId).OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(topic => topic.Questions).HasField("_questions").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class TalkTopicTopicConfiguration : IEntityTypeConfiguration<TalkTopicTopic>
{
    public void Configure(EntityTypeBuilder<TalkTopicTopic> builder)
    {
        builder.ToTable("TalkTopicTopics");
        builder.HasKey(topic => topic.Id);
        builder.Property(topic => topic.Id).ValueGeneratedNever();
        builder.Property(topic => topic.TalkTopicId).IsRequired();
        builder.Property(topic => topic.TopicId).IsRequired();
        builder.Property(topic => topic.IsPrimary).IsRequired();
        builder.Property(topic => topic.CreatedAtUtc).IsRequired();
        builder.HasIndex(topic => new { topic.TalkTopicId, topic.TopicId }).IsUnique();
        builder.HasIndex(topic => topic.TopicId);
        builder.HasIndex(topic => topic.TalkTopicId).IsUnique().HasFilter($"\"{nameof(TalkTopicTopic.IsPrimary)}\"");
        builder.HasOne<Topic>().WithMany().HasForeignKey(topic => topic.TopicId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class TalkTopicArticleTranslationConfiguration :
    TalkTopicTranslationConfigurationBase<TalkTopicArticleTranslation>,
    IEntityTypeConfiguration<TalkTopicArticleTranslation>
{
    public void Configure(EntityTypeBuilder<TalkTopicArticleTranslation> builder)
    {
        ConfigureBase(builder, 12000);
        builder.ToTable("TalkTopicArticleTranslations");
    }
}

internal sealed class TalkTopicQuestionConfiguration : IEntityTypeConfiguration<TalkTopicQuestion>
{
    public void Configure(EntityTypeBuilder<TalkTopicQuestion> builder)
    {
        builder.ToTable("TalkTopicQuestions");
        builder.HasKey(question => question.Id);
        builder.Property(question => question.Id).ValueGeneratedNever();
        builder.Property(question => question.TalkTopicId).IsRequired();
        builder.Property(question => question.Kind).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(question => question.QuestionType).HasConversion<string>().HasMaxLength(64);
        builder.Property(question => question.SortOrder).IsRequired();
        builder.Property(question => question.Prompt).HasMaxLength(1024).IsRequired();
        builder.Property(question => question.CreatedAtUtc).IsRequired();
        builder.Property(question => question.UpdatedAtUtc).IsRequired();
        builder.HasIndex(question => new { question.TalkTopicId, question.Kind, question.SortOrder }).IsUnique();
        builder.HasMany(question => question.Translations).WithOne().HasForeignKey(translation => translation.OwnerId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class TalkTopicQuestionTranslationConfiguration :
    TalkTopicTranslationConfigurationBase<TalkTopicQuestionTranslation>,
    IEntityTypeConfiguration<TalkTopicQuestionTranslation>
{
    public void Configure(EntityTypeBuilder<TalkTopicQuestionTranslation> builder)
    {
        ConfigureBase(builder, 2000);
        builder.ToTable("TalkTopicQuestionTranslations");
    }
}

internal sealed class TalkTopicVocabularyItemConfiguration : IEntityTypeConfiguration<TalkTopicVocabularyItem>
{
    public void Configure(EntityTypeBuilder<TalkTopicVocabularyItem> builder)
    {
        builder.ToTable("TalkTopicVocabularyItems");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.TalkTopicId).IsRequired();
        builder.Property(item => item.Lemma).HasMaxLength(128).IsRequired();
        builder.Property(item => item.WordSlug).HasMaxLength(128);
        builder.Property(item => item.CefrLevel).HasConversion<string>().HasMaxLength(8);
        builder.Property(item => item.SortOrder).IsRequired();
        builder.Property(item => item.CreatedAtUtc).IsRequired();
        builder.HasIndex(item => new { item.TalkTopicId, item.SortOrder }).IsUnique();
        builder.HasIndex(item => item.WordSlug);
    }
}

internal sealed class TalkTopicSpeakingGoalLinkConfiguration : IEntityTypeConfiguration<TalkTopicSpeakingGoalLink>
{
    public void Configure(EntityTypeBuilder<TalkTopicSpeakingGoalLink> builder)
    {
        builder.ToTable("TalkTopicSpeakingGoals");
        builder.HasKey(goal => goal.Id);
        builder.Property(goal => goal.Id).ValueGeneratedNever();
        builder.Property(goal => goal.TalkTopicId).IsRequired();
        builder.Property(goal => goal.SpeakingGoal).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(goal => goal.SortOrder).IsRequired();
        builder.Property(goal => goal.CreatedAtUtc).IsRequired();
        builder.HasIndex(goal => new { goal.TalkTopicId, goal.SpeakingGoal }).IsUnique();
        builder.HasIndex(goal => new { goal.TalkTopicId, goal.SortOrder }).IsUnique();
    }
}

internal abstract class TalkTopicTranslationConfigurationBase<TTranslation>
    where TTranslation : TalkTopicTranslationBase
{
    protected static void ConfigureBase(EntityTypeBuilder<TTranslation> builder, int maxLength)
    {
        builder.HasKey(translation => translation.Id);
        builder.Property(translation => translation.Id).ValueGeneratedNever();
        builder.Property(translation => translation.OwnerId).IsRequired();
        builder.Property(translation => translation.LanguageCode)
            .HasConversion(languageCode => languageCode.Value, value => LanguageCode.From(value))
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(translation => translation.Text).HasMaxLength(maxLength).IsRequired();
        builder.Property(translation => translation.CreatedAtUtc).IsRequired();
        builder.Property(translation => translation.UpdatedAtUtc).IsRequired();
        builder.HasIndex(translation => new { translation.OwnerId, translation.LanguageCode }).IsUnique();
    }
}

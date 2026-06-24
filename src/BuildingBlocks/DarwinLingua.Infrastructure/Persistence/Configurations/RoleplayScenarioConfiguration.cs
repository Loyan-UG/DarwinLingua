using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class RoleplayScenarioConfiguration : IEntityTypeConfiguration<RoleplayScenario>
{
    public void Configure(EntityTypeBuilder<RoleplayScenario> builder)
    {
        builder.ToTable("RoleplayScenarios");
        builder.HasKey(scenario => scenario.Id);
        builder.Property(scenario => scenario.Id).ValueGeneratedNever();
        builder.Property(scenario => scenario.TargetLearningLanguageCode)
            .HasMaxLength(16)
            .HasDefaultValue(ContentLanguageRequirements.DefaultTargetLearningLanguageCode)
            .IsRequired();
        builder.Property(scenario => scenario.Slug).HasMaxLength(128).IsRequired();
        builder.Property(scenario => scenario.LinkedDialogueSlug).HasMaxLength(128);
        builder.Property(scenario => scenario.Title).HasMaxLength(256).IsRequired();
        builder.Property(scenario => scenario.TitleTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(scenario => scenario.Description).HasMaxLength(4000).IsRequired();
        builder.Property(scenario => scenario.DescriptionTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(scenario => scenario.LearnerGoal).HasMaxLength(2000).IsRequired();
        builder.Property(scenario => scenario.LearnerGoalTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(scenario => scenario.CefrLevel).HasConversion<string>().HasMaxLength(8).IsRequired();
        builder.Property(scenario => scenario.Category).HasMaxLength(128).IsRequired();
        builder.Property(scenario => scenario.TaskType).HasMaxLength(128).IsRequired();
        builder.Property(scenario => scenario.InteractionMode).HasMaxLength(128).IsRequired();
        builder.Property(scenario => scenario.Register).HasMaxLength(64).IsRequired();
        builder.Property(scenario => scenario.EstimatedPracticeMinutes).IsRequired();
        builder.Property(scenario => scenario.ExamProfilesJson).HasColumnType("TEXT").IsRequired();
        builder.Property(scenario => scenario.SkillFocusJson).HasColumnType("TEXT").IsRequired();
        builder.Property(scenario => scenario.RolesJson).HasColumnType("TEXT").IsRequired();
        builder.Property(scenario => scenario.TurnsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(scenario => scenario.AnswerChoicesJson).HasColumnType("TEXT").IsRequired();
        builder.Property(scenario => scenario.StaticFeedbackJson).HasColumnType("TEXT").IsRequired();
        builder.Property(scenario => scenario.ImageSlotsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(scenario => scenario.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(scenario => scenario.SortOrder).IsRequired();
        builder.Property(scenario => scenario.CreatedAtUtc).IsRequired();
        builder.Property(scenario => scenario.UpdatedAtUtc).IsRequired();
        builder.HasIndex(scenario => new { scenario.TargetLearningLanguageCode, scenario.Slug }).IsUnique();
        builder.HasIndex(scenario => new { scenario.TargetLearningLanguageCode, scenario.CefrLevel, scenario.Category, scenario.TaskType, scenario.InteractionMode, scenario.Register });

        builder.HasMany(scenario => scenario.Topics).WithOne().HasForeignKey(topic => topic.RoleplayScenarioId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class RoleplayScenarioTopicConfiguration : IEntityTypeConfiguration<RoleplayScenarioTopic>
{
    public void Configure(EntityTypeBuilder<RoleplayScenarioTopic> builder)
    {
        builder.ToTable("RoleplayScenarioTopics");
        builder.HasKey(topic => topic.Id);
        builder.Property(topic => topic.Id).ValueGeneratedNever();
        builder.Property(topic => topic.RoleplayScenarioId).IsRequired();
        builder.Property(topic => topic.TopicId).IsRequired();
        builder.Property(topic => topic.IsPrimary).IsRequired();
        builder.Property(topic => topic.CreatedAtUtc).IsRequired();
        builder.HasIndex(topic => new { topic.RoleplayScenarioId, topic.TopicId }).IsUnique();
        builder.HasIndex(topic => topic.TopicId);
        builder.HasOne<Topic>().WithMany().HasForeignKey(topic => topic.TopicId).OnDelete(DeleteBehavior.Restrict);
    }
}

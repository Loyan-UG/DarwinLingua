using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class ExamProfileConfiguration : IEntityTypeConfiguration<ExamProfile>
{
    public void Configure(EntityTypeBuilder<ExamProfile> builder)
    {
        builder.ToTable("ExamProfiles");
        builder.HasKey(profile => profile.Id);
        builder.Property(profile => profile.Id).ValueGeneratedNever();
        builder.Property(profile => profile.Key).HasMaxLength(96).IsRequired();
        builder.Property(profile => profile.DisplayName).HasMaxLength(256).IsRequired();
        builder.Property(profile => profile.DisplayNameTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(profile => profile.CefrRange).HasMaxLength(64).IsRequired();
        builder.Property(profile => profile.Description).HasMaxLength(1000).IsRequired();
        builder.Property(profile => profile.DescriptionTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(profile => profile.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(profile => profile.SortOrder).IsRequired();
        builder.Property(profile => profile.CreatedAtUtc).IsRequired();
        builder.Property(profile => profile.UpdatedAtUtc).IsRequired();
        builder.HasIndex(profile => profile.Key).IsUnique();
    }
}

internal sealed class ExamPrepUnitConfiguration : IEntityTypeConfiguration<ExamPrepUnit>
{
    public void Configure(EntityTypeBuilder<ExamPrepUnit> builder)
    {
        builder.ToTable("ExamPrepUnits");
        builder.HasKey(unit => unit.Id);
        builder.Property(unit => unit.Id).ValueGeneratedNever();
        builder.Property(unit => unit.Slug).HasMaxLength(128).IsRequired();
        builder.Property(unit => unit.ExamProfileKey).HasMaxLength(96).IsRequired();
        builder.Property(unit => unit.Title).HasMaxLength(256).IsRequired();
        builder.Property(unit => unit.ShortDescription).HasMaxLength(1000).IsRequired();
        builder.Property(unit => unit.CefrLevel).HasConversion<string>().HasMaxLength(8).IsRequired();
        builder.Property(unit => unit.ExamSection).HasMaxLength(64).IsRequired();
        builder.Property(unit => unit.TaskType).HasMaxLength(96).IsRequired();
        builder.Property(unit => unit.SkillFocus).HasMaxLength(64).IsRequired();
        builder.Property(unit => unit.Explanation).HasColumnType("TEXT").IsRequired();
        builder.Property(unit => unit.TitleTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(unit => unit.ShortDescriptionTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(unit => unit.ExplanationTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(unit => unit.StrategyNotesJson).HasColumnType("TEXT").IsRequired();
        builder.Property(unit => unit.StrategyNotesTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(unit => unit.ChecklistJson).HasColumnType("TEXT").IsRequired();
        builder.Property(unit => unit.ChecklistTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(unit => unit.LinkedDialogueSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(unit => unit.LinkedTalkTopicSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(unit => unit.LinkedGrammarTopicSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(unit => unit.LinkedExpressionSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(unit => unit.LinkedWritingTemplateSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(unit => unit.LinkedExerciseSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(unit => unit.LinkedRoleplaySlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(unit => unit.LinkedCourseLessonSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(unit => unit.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(unit => unit.SortOrder).IsRequired();
        builder.Property(unit => unit.CreatedAtUtc).IsRequired();
        builder.Property(unit => unit.UpdatedAtUtc).IsRequired();
        builder.HasIndex(unit => unit.Slug).IsUnique();
        builder.HasIndex(unit => new { unit.ExamProfileKey, unit.CefrLevel, unit.ExamSection });
    }
}

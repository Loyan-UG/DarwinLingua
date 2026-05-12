using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class CoursePathConfiguration : IEntityTypeConfiguration<CoursePath>
{
    public void Configure(EntityTypeBuilder<CoursePath> builder)
    {
        builder.ToTable("CoursePaths");
        builder.HasKey(course => course.Id);
        builder.Property(course => course.Id).ValueGeneratedNever();
        builder.Property(course => course.Slug).HasMaxLength(128).IsRequired();
        builder.Property(course => course.Title).HasMaxLength(256).IsRequired();
        builder.Property(course => course.Description).HasMaxLength(2000).IsRequired();
        builder.Property(course => course.CefrLevel).HasConversion<string>().HasMaxLength(8);
        builder.Property(course => course.CefrRange).HasMaxLength(32).IsRequired();
        builder.Property(course => course.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(course => course.SortOrder).IsRequired();
        builder.Property(course => course.CreatedAtUtc).IsRequired();
        builder.Property(course => course.UpdatedAtUtc).IsRequired();
        builder.HasIndex(course => course.Slug).IsUnique();
        builder.HasIndex(course => course.CefrLevel);
        builder.HasMany(course => course.Modules).WithOne().HasForeignKey(module => module.CoursePathId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class CourseModuleConfiguration : IEntityTypeConfiguration<CourseModule>
{
    public void Configure(EntityTypeBuilder<CourseModule> builder)
    {
        builder.ToTable("CourseModules");
        builder.HasKey(module => module.Id);
        builder.Property(module => module.Id).ValueGeneratedNever();
        builder.Property(module => module.CoursePathId).IsRequired();
        builder.Property(module => module.CoursePathSlug).HasMaxLength(128).IsRequired();
        builder.Property(module => module.Slug).HasMaxLength(128).IsRequired();
        builder.Property(module => module.Title).HasMaxLength(256).IsRequired();
        builder.Property(module => module.Description).HasMaxLength(2000).IsRequired();
        builder.Property(module => module.ModuleNumber).IsRequired();
        builder.Property(module => module.CefrLevel).HasConversion<string>().HasMaxLength(8).IsRequired();
        builder.Property(module => module.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(module => module.SortOrder).IsRequired();
        builder.Property(module => module.CreatedAtUtc).IsRequired();
        builder.Property(module => module.UpdatedAtUtc).IsRequired();
        builder.HasIndex(module => module.Slug).IsUnique();
        builder.HasIndex(module => new { module.CoursePathId, module.ModuleNumber }).IsUnique();
        builder.HasMany(module => module.Lessons).WithOne().HasForeignKey(lesson => lesson.CourseModuleId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class CourseLessonConfiguration : IEntityTypeConfiguration<CourseLesson>
{
    public void Configure(EntityTypeBuilder<CourseLesson> builder)
    {
        builder.ToTable("CourseLessons");
        builder.HasKey(lesson => lesson.Id);
        builder.Property(lesson => lesson.Id).ValueGeneratedNever();
        builder.Property(lesson => lesson.CourseModuleId).IsRequired();
        builder.Property(lesson => lesson.CoursePathSlug).HasMaxLength(128).IsRequired();
        builder.Property(lesson => lesson.ModuleSlug).HasMaxLength(128).IsRequired();
        builder.Property(lesson => lesson.Slug).HasMaxLength(128).IsRequired();
        builder.Property(lesson => lesson.LessonNumber).IsRequired();
        builder.Property(lesson => lesson.Title).HasMaxLength(256).IsRequired();
        builder.Property(lesson => lesson.ShortDescription).HasMaxLength(1000).IsRequired();
        builder.Property(lesson => lesson.Narrative).HasMaxLength(4000).IsRequired();
        builder.Property(lesson => lesson.CefrLevel).HasConversion<string>().HasMaxLength(8).IsRequired();
        builder.Property(lesson => lesson.EstimatedMinutes).IsRequired();
        builder.Property(lesson => lesson.LearningGoalsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(lesson => lesson.PrerequisiteLessonSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(lesson => lesson.NextLessonSlug).HasMaxLength(128);
        builder.Property(lesson => lesson.LinkedGrammarTopicSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(lesson => lesson.LinkedWordSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(lesson => lesson.LinkedExpressionSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(lesson => lesson.LinkedDialogueSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(lesson => lesson.LinkedTalkTopicSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(lesson => lesson.LinkedExerciseSetSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(lesson => lesson.LinkedExamPrepSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(lesson => lesson.ReviewSummary).HasMaxLength(2000);
        builder.Property(lesson => lesson.HomeworkTask).HasMaxLength(2000);
        builder.Property(lesson => lesson.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(lesson => lesson.SortOrder).IsRequired();
        builder.Property(lesson => lesson.CreatedAtUtc).IsRequired();
        builder.Property(lesson => lesson.UpdatedAtUtc).IsRequired();
        builder.HasIndex(lesson => lesson.Slug).IsUnique();
        builder.HasIndex(lesson => new { lesson.CourseModuleId, lesson.LessonNumber }).IsUnique();
        builder.HasIndex(lesson => new { lesson.CoursePathSlug, lesson.ModuleSlug });
    }
}

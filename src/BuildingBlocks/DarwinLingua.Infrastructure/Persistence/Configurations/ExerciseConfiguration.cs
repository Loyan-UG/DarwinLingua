using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("Exercises");
        builder.HasKey(exercise => exercise.Id);
        builder.Property(exercise => exercise.Id).ValueGeneratedNever();
        builder.Property(exercise => exercise.Slug).HasMaxLength(128).IsRequired();
        builder.Property(exercise => exercise.Title).HasMaxLength(256).IsRequired();
        builder.Property(exercise => exercise.Instruction).HasMaxLength(2000).IsRequired();
        builder.Property(exercise => exercise.CefrLevel).HasConversion<string>().HasMaxLength(8).IsRequired();
        builder.Property(exercise => exercise.ExerciseType).HasMaxLength(64).IsRequired();
        builder.Property(exercise => exercise.TargetSkill).HasMaxLength(64).IsRequired();
        builder.Property(exercise => exercise.OwnerType).HasMaxLength(64).IsRequired();
        builder.Property(exercise => exercise.OwnerSlug).HasMaxLength(128);
        builder.Property(exercise => exercise.PromptJson).HasColumnType("TEXT").IsRequired();
        builder.Property(exercise => exercise.AnswerKeyJson).HasColumnType("TEXT").IsRequired();
        builder.Property(exercise => exercise.TitleTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(exercise => exercise.InstructionTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(exercise => exercise.CorrectExplanation).HasMaxLength(2000).IsRequired();
        builder.Property(exercise => exercise.CorrectExplanationTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(exercise => exercise.IncorrectExplanation).HasMaxLength(2000).IsRequired();
        builder.Property(exercise => exercise.IncorrectExplanationTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(exercise => exercise.Hint).HasMaxLength(1000);
        builder.Property(exercise => exercise.HintTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(exercise => exercise.CommonMistakeNote).HasMaxLength(1000);
        builder.Property(exercise => exercise.CommonMistakeNoteTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(exercise => exercise.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(exercise => exercise.SortOrder).IsRequired();
        builder.Property(exercise => exercise.CreatedAtUtc).IsRequired();
        builder.Property(exercise => exercise.UpdatedAtUtc).IsRequired();
        builder.HasIndex(exercise => exercise.Slug).IsUnique();
        builder.HasIndex(exercise => new { exercise.CefrLevel, exercise.ExerciseType, exercise.TargetSkill });
        builder.HasIndex(exercise => new { exercise.OwnerType, exercise.OwnerSlug });
    }
}

internal sealed class ExerciseSetConfiguration : IEntityTypeConfiguration<ExerciseSet>
{
    public void Configure(EntityTypeBuilder<ExerciseSet> builder)
    {
        builder.ToTable("ExerciseSets");
        builder.HasKey(set => set.Id);
        builder.Property(set => set.Id).ValueGeneratedNever();
        builder.Property(set => set.Slug).HasMaxLength(128).IsRequired();
        builder.Property(set => set.Title).HasMaxLength(256).IsRequired();
        builder.Property(set => set.TitleTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(set => set.Description).HasMaxLength(2000).IsRequired();
        builder.Property(set => set.DescriptionTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(set => set.CefrLevel).HasConversion<string>().HasMaxLength(8).IsRequired();
        builder.Property(set => set.OwnerType).HasMaxLength(64).IsRequired();
        builder.Property(set => set.OwnerSlug).HasMaxLength(128);
        builder.Property(set => set.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(set => set.SortOrder).IsRequired();
        builder.Property(set => set.CreatedAtUtc).IsRequired();
        builder.Property(set => set.UpdatedAtUtc).IsRequired();
        builder.HasIndex(set => set.Slug).IsUnique();
        builder.HasIndex(set => new { set.OwnerType, set.OwnerSlug });
        builder.HasMany(set => set.Items).WithOne().HasForeignKey(item => item.ExerciseSetId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ExerciseSetItemConfiguration : IEntityTypeConfiguration<ExerciseSetItem>
{
    public void Configure(EntityTypeBuilder<ExerciseSetItem> builder)
    {
        builder.ToTable("ExerciseSetItems");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.ExerciseSetId).IsRequired();
        builder.Property(item => item.ExerciseSlug).HasMaxLength(128).IsRequired();
        builder.Property(item => item.SortOrder).IsRequired();
        builder.Property(item => item.CreatedAtUtc).IsRequired();
        builder.HasIndex(item => new { item.ExerciseSetId, item.ExerciseSlug }).IsUnique();
        builder.HasIndex(item => new { item.ExerciseSetId, item.SortOrder }).IsUnique();
    }
}

internal sealed class UserExerciseAttemptConfiguration : IEntityTypeConfiguration<UserExerciseAttempt>
{
    public void Configure(EntityTypeBuilder<UserExerciseAttempt> builder)
    {
        builder.ToTable("UserExerciseAttempts");
        builder.HasKey(attempt => attempt.Id);
        builder.Property(attempt => attempt.Id).ValueGeneratedNever();
        builder.Property(attempt => attempt.UserId).HasMaxLength(256).IsRequired();
        builder.Property(attempt => attempt.ExerciseSlug).HasMaxLength(128).IsRequired();
        builder.Property(attempt => attempt.SubmittedAnswerJson).HasColumnType("TEXT").IsRequired();
        builder.Property(attempt => attempt.IsCorrect).IsRequired();
        builder.Property(attempt => attempt.FeedbackExplanation).HasMaxLength(2000).IsRequired();
        builder.Property(attempt => attempt.AttemptedAtUtc).IsRequired();
        builder.Property(attempt => attempt.CreatedAtUtc).IsRequired();
        builder.HasIndex(attempt => new { attempt.UserId, attempt.ExerciseSlug, attempt.AttemptedAtUtc });
    }
}

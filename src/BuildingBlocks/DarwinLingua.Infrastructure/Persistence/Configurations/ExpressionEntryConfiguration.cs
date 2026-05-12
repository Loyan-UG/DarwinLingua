using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class ExpressionEntryConfiguration : IEntityTypeConfiguration<ExpressionEntry>
{
    public void Configure(EntityTypeBuilder<ExpressionEntry> builder)
    {
        builder.ToTable("ExpressionEntries");
        builder.HasKey(expression => expression.Id);
        builder.Property(expression => expression.Id).ValueGeneratedNever();
        builder.Property(expression => expression.Slug).HasMaxLength(128).IsRequired();
        builder.Property(expression => expression.ExpressionText).HasMaxLength(512).IsRequired();
        builder.Property(expression => expression.LiteralMeaningText).HasMaxLength(1024);
        builder.Property(expression => expression.ActualMeaningText).HasMaxLength(4000).IsRequired();
        builder.Property(expression => expression.UsageExplanation).HasMaxLength(4000);
        builder.Property(expression => expression.CefrLevel).HasConversion<string>().HasMaxLength(8).IsRequired();
        builder.Property(expression => expression.ExpressionType).HasMaxLength(64).IsRequired();
        builder.Property(expression => expression.Register).HasMaxLength(64).IsRequired();
        builder.Property(expression => expression.Category).HasMaxLength(128).IsRequired();
        builder.Property(expression => expression.Region).HasMaxLength(128);
        builder.Property(expression => expression.IsRisky).IsRequired();
        builder.Property(expression => expression.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(expression => expression.SortOrder).IsRequired();
        builder.Property(expression => expression.CreatedAtUtc).IsRequired();
        builder.Property(expression => expression.UpdatedAtUtc).IsRequired();
        builder.HasIndex(expression => expression.Slug).IsUnique();
        builder.HasIndex(expression => new { expression.CefrLevel, expression.ExpressionType, expression.Register, expression.Category });

        builder.HasMany(expression => expression.Topics).WithOne().HasForeignKey(link => link.ExpressionEntryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(expression => expression.Meanings).WithOne().HasForeignKey(meaning => meaning.ExpressionEntryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(expression => expression.Examples).WithOne().HasForeignKey(example => example.ExpressionEntryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(expression => expression.Warnings).WithOne().HasForeignKey(warning => warning.ExpressionEntryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(expression => expression.LinkedWords).WithOne().HasForeignKey(link => link.ExpressionEntryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(expression => expression.RelatedExpressions).WithOne().HasForeignKey(link => link.ExpressionEntryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(expression => expression.LinkedExercises).WithOne().HasForeignKey(link => link.ExpressionEntryId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ExpressionTopicConfiguration : IEntityTypeConfiguration<ExpressionTopic>
{
    public void Configure(EntityTypeBuilder<ExpressionTopic> builder)
    {
        builder.ToTable("ExpressionTopics");
        builder.HasKey(link => link.Id);
        builder.Property(link => link.Id).ValueGeneratedNever();
        builder.Property(link => link.ExpressionEntryId).IsRequired();
        builder.Property(link => link.TopicId).IsRequired();
        builder.Property(link => link.IsPrimary).IsRequired();
        builder.Property(link => link.CreatedAtUtc).IsRequired();
        builder.HasIndex(link => new { link.ExpressionEntryId, link.TopicId }).IsUnique();
        builder.HasIndex(link => link.TopicId);
        builder.HasOne<Topic>().WithMany().HasForeignKey(link => link.TopicId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExpressionMeaningConfiguration : IEntityTypeConfiguration<ExpressionMeaning>
{
    public void Configure(EntityTypeBuilder<ExpressionMeaning> builder)
    {
        builder.ToTable("ExpressionMeanings");
        builder.HasKey(meaning => meaning.Id);
        builder.Property(meaning => meaning.Id).ValueGeneratedNever();
        builder.Property(meaning => meaning.ExpressionEntryId).IsRequired();
        builder.Property(meaning => meaning.LanguageCode)
            .HasConversion(languageCode => languageCode.Value, value => LanguageCode.From(value))
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(meaning => meaning.ActualMeaningText).HasMaxLength(4000).IsRequired();
        builder.Property(meaning => meaning.LiteralMeaningText).HasMaxLength(1024);
        builder.Property(meaning => meaning.UsageExplanation).HasMaxLength(4000);
        builder.Property(meaning => meaning.CreatedAtUtc).IsRequired();
        builder.Property(meaning => meaning.UpdatedAtUtc).IsRequired();
        builder.HasIndex(meaning => new { meaning.ExpressionEntryId, meaning.LanguageCode }).IsUnique();
    }
}

internal sealed class ExpressionExampleConfiguration : IEntityTypeConfiguration<ExpressionExample>
{
    public void Configure(EntityTypeBuilder<ExpressionExample> builder)
    {
        builder.ToTable("ExpressionExamples");
        builder.HasKey(example => example.Id);
        builder.Property(example => example.Id).ValueGeneratedNever();
        builder.Property(example => example.ExpressionEntryId).IsRequired();
        builder.Property(example => example.SortOrder).IsRequired();
        builder.Property(example => example.GermanText).HasMaxLength(1024).IsRequired();
        builder.Property(example => example.Note).HasMaxLength(512);
        builder.Property(example => example.CreatedAtUtc).IsRequired();
        builder.Property(example => example.UpdatedAtUtc).IsRequired();
        builder.HasIndex(example => new { example.ExpressionEntryId, example.SortOrder }).IsUnique();
        builder.HasMany(example => example.Translations).WithOne().HasForeignKey(translation => translation.OwnerId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ExpressionWarningConfiguration : IEntityTypeConfiguration<ExpressionWarning>
{
    public void Configure(EntityTypeBuilder<ExpressionWarning> builder)
    {
        builder.ToTable("ExpressionWarnings");
        builder.HasKey(warning => warning.Id);
        builder.Property(warning => warning.Id).ValueGeneratedNever();
        builder.Property(warning => warning.ExpressionEntryId).IsRequired();
        builder.Property(warning => warning.WarningType).HasMaxLength(64).IsRequired();
        builder.Property(warning => warning.Text).HasMaxLength(2000).IsRequired();
        builder.Property(warning => warning.CreatedAtUtc).IsRequired();
        builder.Property(warning => warning.UpdatedAtUtc).IsRequired();
        builder.HasIndex(warning => new { warning.ExpressionEntryId, warning.WarningType }).IsUnique();
        builder.HasMany(warning => warning.Translations).WithOne().HasForeignKey(translation => translation.OwnerId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ExpressionExampleTranslationConfiguration : ExpressionTranslationConfiguration<ExpressionExampleTranslation>
{
    protected override string TableName => "ExpressionExampleTranslations";
}

internal sealed class ExpressionWarningTranslationConfiguration : ExpressionTranslationConfiguration<ExpressionWarningTranslation>
{
    protected override string TableName => "ExpressionWarningTranslations";
}

internal sealed class RelatedExpressionLinkConfiguration : ExpressionSlugLinkConfiguration<RelatedExpressionLink>
{
    protected override string TableName => "RelatedExpressionLinks";
}

internal sealed class ExpressionLinkedExerciseConfiguration : ExpressionSlugLinkConfiguration<ExpressionLinkedExercise>
{
    protected override string TableName => "ExpressionLinkedExercises";
}

internal sealed class ExpressionLinkedWordConfiguration : IEntityTypeConfiguration<ExpressionLinkedWord>
{
    public void Configure(EntityTypeBuilder<ExpressionLinkedWord> builder)
    {
        builder.ToTable("ExpressionLinkedWords");
        builder.HasKey(link => link.Id);
        builder.Property(link => link.Id).ValueGeneratedNever();
        builder.Property(link => link.ExpressionEntryId).IsRequired();
        builder.Property(link => link.Lemma).HasMaxLength(128).IsRequired();
        builder.Property(link => link.WordSlug).HasMaxLength(128);
        builder.Property(link => link.SortOrder).IsRequired();
        builder.Property(link => link.CreatedAtUtc).IsRequired();
        builder.HasIndex(link => new { link.ExpressionEntryId, link.SortOrder }).IsUnique();
        builder.HasIndex(link => link.WordSlug);
    }
}

internal abstract class ExpressionTranslationConfiguration<TTranslation> : IEntityTypeConfiguration<TTranslation>
    where TTranslation : ExpressionTranslationBase
{
    protected abstract string TableName { get; }

    public void Configure(EntityTypeBuilder<TTranslation> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(translation => translation.Id);
        builder.Property(translation => translation.Id).ValueGeneratedNever();
        builder.Property(translation => translation.OwnerId).IsRequired();
        builder.Property(translation => translation.LanguageCode)
            .HasConversion(languageCode => languageCode.Value, value => LanguageCode.From(value))
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(translation => translation.Text).HasMaxLength(4000).IsRequired();
        builder.Property(translation => translation.CreatedAtUtc).IsRequired();
        builder.Property(translation => translation.UpdatedAtUtc).IsRequired();
        builder.HasIndex(translation => new { translation.OwnerId, translation.LanguageCode }).IsUnique();
    }
}

internal abstract class ExpressionSlugLinkConfiguration<TLink> : IEntityTypeConfiguration<TLink>
    where TLink : ExpressionSlugLink
{
    protected abstract string TableName { get; }

    public void Configure(EntityTypeBuilder<TLink> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(link => link.Id);
        builder.Property(link => link.Id).ValueGeneratedNever();
        builder.Property(link => link.ExpressionEntryId).IsRequired();
        builder.Property(link => link.TargetSlug).HasMaxLength(128).IsRequired();
        builder.Property(link => link.SortOrder).IsRequired();
        builder.Property(link => link.CreatedAtUtc).IsRequired();
        builder.HasIndex(link => new { link.ExpressionEntryId, link.TargetSlug }).IsUnique();
        builder.HasIndex(link => new { link.ExpressionEntryId, link.SortOrder }).IsUnique();
    }
}

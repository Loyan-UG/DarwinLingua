using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class GrammarTopicConfiguration : IEntityTypeConfiguration<GrammarTopic>
{
    public void Configure(EntityTypeBuilder<GrammarTopic> builder)
    {
        builder.ToTable("GrammarTopics");
        builder.HasKey(topic => topic.Id);
        builder.Property(topic => topic.Id).ValueGeneratedNever();
        builder.Property(topic => topic.Slug).HasMaxLength(128).IsRequired();
        builder.Property(topic => topic.Title).HasMaxLength(256).IsRequired();
        builder.Property(topic => topic.ShortDescription).HasMaxLength(1024).IsRequired();
        builder.Property(topic => topic.ContentRevision);
        builder.Property(topic => topic.TitleLocalizedJson).HasColumnType("text");
        builder.Property(topic => topic.ShortDescriptionLocalizedJson).HasColumnType("text");
        builder.Property(topic => topic.ImageSlotsJson).HasColumnType("text");
        builder.Property(topic => topic.CefrLevel).HasConversion<string>().HasMaxLength(8).IsRequired();
        builder.Property(topic => topic.GrammarCategory).HasMaxLength(128).IsRequired();
        builder.Property(topic => topic.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(topic => topic.SortOrder).IsRequired();
        builder.Property(topic => topic.CreatedAtUtc).IsRequired();
        builder.Property(topic => topic.UpdatedAtUtc).IsRequired();
        builder.HasIndex(topic => topic.Slug).IsUnique();
        builder.HasIndex(topic => new { topic.CefrLevel, topic.GrammarCategory });

        builder.HasMany(topic => topic.Topics).WithOne().HasForeignKey(link => link.GrammarTopicId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(topic => topic.Sections).WithOne().HasForeignKey(section => section.GrammarTopicId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(topic => topic.Examples).WithOne().HasForeignKey(example => example.GrammarTopicId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(topic => topic.CommonMistakes).WithOne().HasForeignKey(mistake => mistake.GrammarTopicId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(topic => topic.RuleSummaries).WithOne().HasForeignKey(rule => rule.GrammarTopicId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(topic => topic.ExceptionNotes).WithOne().HasForeignKey(note => note.GrammarTopicId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(topic => topic.Prerequisites).WithOne().HasForeignKey(link => link.GrammarTopicId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(topic => topic.RelatedTopics).WithOne().HasForeignKey(link => link.GrammarTopicId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(topic => topic.LinkedWords).WithOne().HasForeignKey(link => link.GrammarTopicId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(topic => topic.LinkedDialogues).WithOne().HasForeignKey(link => link.GrammarTopicId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(topic => topic.LinkedTalkTopics).WithOne().HasForeignKey(link => link.GrammarTopicId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(topic => topic.LinkedExercises).WithOne().HasForeignKey(link => link.GrammarTopicId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class GrammarTopicTopicConfiguration : IEntityTypeConfiguration<GrammarTopicTopic>
{
    public void Configure(EntityTypeBuilder<GrammarTopicTopic> builder)
    {
        builder.ToTable("GrammarTopicTopics");
        builder.HasKey(link => link.Id);
        builder.Property(link => link.Id).ValueGeneratedNever();
        builder.Property(link => link.GrammarTopicId).IsRequired();
        builder.Property(link => link.TopicId).IsRequired();
        builder.Property(link => link.IsPrimary).IsRequired();
        builder.Property(link => link.CreatedAtUtc).IsRequired();
        builder.HasIndex(link => new { link.GrammarTopicId, link.TopicId }).IsUnique();
        builder.HasIndex(link => link.TopicId);
        builder.HasOne<Topic>().WithMany().HasForeignKey(link => link.TopicId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class GrammarSectionConfiguration : IEntityTypeConfiguration<GrammarSection>
{
    public void Configure(EntityTypeBuilder<GrammarSection> builder)
    {
        builder.ToTable("GrammarSections");
        builder.HasKey(section => section.Id);
        builder.Property(section => section.Id).ValueGeneratedNever();
        builder.Property(section => section.GrammarTopicId).IsRequired();
        builder.Property(section => section.SortOrder).IsRequired();
        builder.Property(section => section.SectionKey).HasMaxLength(128);
        builder.Property(section => section.Heading).HasMaxLength(256).IsRequired();
        builder.Property(section => section.Explanation).HasMaxLength(12000).IsRequired();
        builder.Property(section => section.LocalizedBlocksJson).HasColumnType("text");
        builder.Property(section => section.CreatedAtUtc).IsRequired();
        builder.Property(section => section.UpdatedAtUtc).IsRequired();
        builder.HasIndex(section => new { section.GrammarTopicId, section.SortOrder }).IsUnique();
        builder.HasMany(section => section.Translations).WithOne().HasForeignKey(translation => translation.OwnerId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class GrammarExampleConfiguration : IEntityTypeConfiguration<GrammarExample>
{
    public void Configure(EntityTypeBuilder<GrammarExample> builder)
    {
        builder.ToTable("GrammarExamples");
        builder.HasKey(example => example.Id);
        builder.Property(example => example.Id).ValueGeneratedNever();
        builder.Property(example => example.GrammarTopicId).IsRequired();
        builder.Property(example => example.SortOrder).IsRequired();
        builder.Property(example => example.GermanText).HasMaxLength(1024).IsRequired();
        builder.Property(example => example.Note).HasMaxLength(512);
        builder.Property(example => example.CreatedAtUtc).IsRequired();
        builder.Property(example => example.UpdatedAtUtc).IsRequired();
        builder.HasIndex(example => new { example.GrammarTopicId, example.SortOrder }).IsUnique();
        builder.HasMany(example => example.Translations).WithOne().HasForeignKey(translation => translation.OwnerId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class GrammarCommonMistakeConfiguration : IEntityTypeConfiguration<GrammarCommonMistake>
{
    public void Configure(EntityTypeBuilder<GrammarCommonMistake> builder)
    {
        builder.ToTable("GrammarCommonMistakes");
        builder.HasKey(mistake => mistake.Id);
        builder.Property(mistake => mistake.Id).ValueGeneratedNever();
        builder.Property(mistake => mistake.GrammarTopicId).IsRequired();
        builder.Property(mistake => mistake.SortOrder).IsRequired();
        builder.Property(mistake => mistake.WrongText).HasMaxLength(1024).IsRequired();
        builder.Property(mistake => mistake.CorrectedText).HasMaxLength(1024).IsRequired();
        builder.Property(mistake => mistake.Explanation).HasMaxLength(4000).IsRequired();
        builder.Property(mistake => mistake.CreatedAtUtc).IsRequired();
        builder.Property(mistake => mistake.UpdatedAtUtc).IsRequired();
        builder.HasIndex(mistake => new { mistake.GrammarTopicId, mistake.SortOrder }).IsUnique();
        builder.HasMany(mistake => mistake.Translations).WithOne().HasForeignKey(translation => translation.OwnerId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class GrammarRuleSummaryConfiguration : GrammarLocalizedTextOwnerConfiguration<GrammarRuleSummary, GrammarRuleSummaryTranslation>
{
    protected override string TableName => "GrammarRuleSummaries";
}

internal sealed class GrammarExceptionNoteConfiguration : GrammarLocalizedTextOwnerConfiguration<GrammarExceptionNote, GrammarExceptionNoteTranslation>
{
    protected override string TableName => "GrammarExceptionNotes";
}

internal sealed class GrammarSectionTranslationConfiguration : GrammarTranslationConfiguration<GrammarSectionTranslation>
{
    protected override string TableName => "GrammarSectionTranslations";

    protected override void ConfigureExtra(EntityTypeBuilder<GrammarSectionTranslation> builder) =>
        builder.Property(translation => translation.Heading).HasMaxLength(256).IsRequired();
}

internal sealed class GrammarExampleTranslationConfiguration : GrammarTranslationConfiguration<GrammarExampleTranslation>
{
    protected override string TableName => "GrammarExampleTranslations";
}

internal sealed class GrammarCommonMistakeTranslationConfiguration : GrammarTranslationConfiguration<GrammarCommonMistakeTranslation>
{
    protected override string TableName => "GrammarCommonMistakeTranslations";
}

internal sealed class GrammarRuleSummaryTranslationConfiguration : GrammarTranslationConfiguration<GrammarRuleSummaryTranslation>
{
    protected override string TableName => "GrammarRuleSummaryTranslations";
}

internal sealed class GrammarExceptionNoteTranslationConfiguration : GrammarTranslationConfiguration<GrammarExceptionNoteTranslation>
{
    protected override string TableName => "GrammarExceptionNoteTranslations";
}

internal sealed class GrammarPrerequisiteLinkConfiguration : GrammarSlugLinkConfiguration<GrammarPrerequisiteLink>
{
    protected override string TableName => "GrammarPrerequisiteLinks";
}

internal sealed class GrammarRelatedTopicLinkConfiguration : GrammarSlugLinkConfiguration<GrammarRelatedTopicLink>
{
    protected override string TableName => "GrammarRelatedTopicLinks";
}

internal sealed class GrammarLinkedDialogueConfiguration : GrammarSlugLinkConfiguration<GrammarLinkedDialogue>
{
    protected override string TableName => "GrammarLinkedDialogues";
}

internal sealed class GrammarLinkedTalkTopicConfiguration : GrammarSlugLinkConfiguration<GrammarLinkedTalkTopic>
{
    protected override string TableName => "GrammarLinkedTalkTopics";
}

internal sealed class GrammarLinkedExerciseConfiguration : GrammarSlugLinkConfiguration<GrammarLinkedExercise>
{
    protected override string TableName => "GrammarLinkedExercises";
}

internal sealed class GrammarLinkedWordConfiguration : IEntityTypeConfiguration<GrammarLinkedWord>
{
    public void Configure(EntityTypeBuilder<GrammarLinkedWord> builder)
    {
        builder.ToTable("GrammarLinkedWords");
        builder.HasKey(link => link.Id);
        builder.Property(link => link.Id).ValueGeneratedNever();
        builder.Property(link => link.GrammarTopicId).IsRequired();
        builder.Property(link => link.Lemma).HasMaxLength(128).IsRequired();
        builder.Property(link => link.WordSlug).HasMaxLength(128);
        builder.Property(link => link.SortOrder).IsRequired();
        builder.Property(link => link.CreatedAtUtc).IsRequired();
        builder.HasIndex(link => new { link.GrammarTopicId, link.SortOrder }).IsUnique();
        builder.HasIndex(link => link.WordSlug);
    }
}

internal abstract class GrammarLocalizedTextOwnerConfiguration<TOwner, TTranslation> : IEntityTypeConfiguration<TOwner>
    where TOwner : GrammarLocalizedTextOwner<TTranslation>
    where TTranslation : GrammarTranslationBase
{
    protected abstract string TableName { get; }

    public void Configure(EntityTypeBuilder<TOwner> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(owner => owner.Id);
        builder.Property(owner => owner.Id).ValueGeneratedNever();
        builder.Property(owner => owner.GrammarTopicId).IsRequired();
        builder.Property(owner => owner.SortOrder).IsRequired();
        builder.Property(owner => owner.Text).HasMaxLength(2000).IsRequired();
        builder.Property(owner => owner.CreatedAtUtc).IsRequired();
        builder.Property(owner => owner.UpdatedAtUtc).IsRequired();
        builder.HasIndex(owner => new { owner.GrammarTopicId, owner.SortOrder }).IsUnique();
        builder.HasMany(owner => owner.Translations).WithOne().HasForeignKey(translation => translation.OwnerId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal abstract class GrammarTranslationConfiguration<TTranslation> : IEntityTypeConfiguration<TTranslation>
    where TTranslation : GrammarTranslationBase
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
        builder.Property(translation => translation.Text).HasMaxLength(12000).IsRequired();
        builder.Property(translation => translation.CreatedAtUtc).IsRequired();
        builder.Property(translation => translation.UpdatedAtUtc).IsRequired();
        builder.HasIndex(translation => new { translation.OwnerId, translation.LanguageCode }).IsUnique();
        ConfigureExtra(builder);
    }

    protected virtual void ConfigureExtra(EntityTypeBuilder<TTranslation> builder)
    {
    }
}

internal abstract class GrammarSlugLinkConfiguration<TLink> : IEntityTypeConfiguration<TLink>
    where TLink : GrammarSlugLink
{
    protected abstract string TableName { get; }

    public void Configure(EntityTypeBuilder<TLink> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(link => link.Id);
        builder.Property(link => link.Id).ValueGeneratedNever();
        builder.Property(link => link.GrammarTopicId).IsRequired();
        builder.Property(link => link.TargetSlug).HasMaxLength(128).IsRequired();
        builder.Property(link => link.SortOrder).IsRequired();
        builder.Property(link => link.CreatedAtUtc).IsRequired();
        builder.HasIndex(link => new { link.GrammarTopicId, link.TargetSlug }).IsUnique();
        builder.HasIndex(link => new { link.GrammarTopicId, link.SortOrder }).IsUnique();
    }
}

using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class ConversationStarterPackConfiguration : IEntityTypeConfiguration<ConversationStarterPack>
{
    public void Configure(EntityTypeBuilder<ConversationStarterPack> builder)
    {
        builder.ToTable("ConversationStarterPacks");
        builder.HasKey(pack => pack.Id);
        builder.Property(pack => pack.Id).ValueGeneratedNever();
        builder.Property(pack => pack.Slug).HasMaxLength(128).IsRequired();
        builder.Property(pack => pack.Title).HasMaxLength(256).IsRequired();
        builder.Property(pack => pack.Description).HasMaxLength(4000).IsRequired();
        builder.Property(pack => pack.CefrLevel).HasConversion<string>().HasMaxLength(8).IsRequired();
        builder.Property(pack => pack.Category).HasMaxLength(128).IsRequired();
        builder.Property(pack => pack.Situation).HasMaxLength(128).IsRequired();
        builder.Property(pack => pack.Tone).HasMaxLength(128).IsRequired();
        builder.Property(pack => pack.ConversationGoal).HasMaxLength(128).IsRequired();
        builder.Property(pack => pack.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(pack => pack.SortOrder).IsRequired();
        builder.Property(pack => pack.CreatedAtUtc).IsRequired();
        builder.Property(pack => pack.UpdatedAtUtc).IsRequired();
        builder.HasIndex(pack => pack.Slug).IsUnique();
        builder.HasIndex(pack => new { pack.CefrLevel, pack.Situation, pack.Tone, pack.ConversationGoal });

        builder.HasMany(pack => pack.Topics).WithOne().HasForeignKey(topic => topic.ConversationStarterPackId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(pack => pack.LinkedScenarios).WithOne().HasForeignKey(link => link.ConversationStarterPackId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(pack => pack.LinkedEventPreparationPacks).WithOne().HasForeignKey(link => link.ConversationStarterPackId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(pack => pack.Phrases).WithOne().HasForeignKey(phrase => phrase.ConversationStarterPackId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ConversationStarterPackTopicConfiguration : IEntityTypeConfiguration<ConversationStarterPackTopic>
{
    public void Configure(EntityTypeBuilder<ConversationStarterPackTopic> builder)
    {
        builder.ToTable("ConversationStarterPackTopics");
        builder.HasKey(topic => topic.Id);
        builder.Property(topic => topic.Id).ValueGeneratedNever();
        builder.Property(topic => topic.ConversationStarterPackId).IsRequired();
        builder.Property(topic => topic.TopicId).IsRequired();
        builder.Property(topic => topic.IsPrimary).IsRequired();
        builder.Property(topic => topic.CreatedAtUtc).IsRequired();
        builder.HasIndex(topic => new { topic.ConversationStarterPackId, topic.TopicId }).IsUnique();
        builder.HasIndex(topic => topic.ConversationStarterPackId).HasDatabaseName("IX_ConversationStarterPackTopics_PrimaryPerPack").IsUnique().HasFilter($"\"{nameof(ConversationStarterPackTopic.IsPrimary)}\"");
        builder.HasOne<Topic>().WithMany().HasForeignKey(topic => topic.TopicId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ConversationStarterLinkedScenarioConfiguration : IEntityTypeConfiguration<ConversationStarterLinkedScenario>
{
    public void Configure(EntityTypeBuilder<ConversationStarterLinkedScenario> builder)
    {
        builder.ToTable("ConversationStarterLinkedScenarios");
        builder.HasKey(link => link.Id);
        builder.Property(link => link.Id).ValueGeneratedNever();
        builder.Property(link => link.ConversationStarterPackId).IsRequired();
        builder.Property(link => link.ScenarioSlug).HasMaxLength(128).IsRequired();
        builder.Property(link => link.SortOrder).IsRequired();
        builder.Property(link => link.CreatedAtUtc).IsRequired();
        builder.HasIndex(link => new { link.ConversationStarterPackId, link.ScenarioSlug }).IsUnique();
    }
}

internal sealed class ConversationStarterLinkedEventPreparationPackConfiguration : IEntityTypeConfiguration<ConversationStarterLinkedEventPreparationPack>
{
    public void Configure(EntityTypeBuilder<ConversationStarterLinkedEventPreparationPack> builder)
    {
        builder.ToTable("ConversationStarterLinkedEventPreparationPacks");
        builder.HasKey(link => link.Id);
        builder.Property(link => link.Id).ValueGeneratedNever();
        builder.Property(link => link.ConversationStarterPackId).IsRequired();
        builder.Property(link => link.EventPreparationPackSlug).HasMaxLength(128).IsRequired();
        builder.Property(link => link.SortOrder).IsRequired();
        builder.Property(link => link.CreatedAtUtc).IsRequired();
        builder.HasIndex(link => new { link.ConversationStarterPackId, link.EventPreparationPackSlug }).IsUnique();
    }
}

internal sealed class ConversationStarterPhraseConfiguration : IEntityTypeConfiguration<ConversationStarterPhrase>
{
    public void Configure(EntityTypeBuilder<ConversationStarterPhrase> builder)
    {
        builder.ToTable("ConversationStarterPhrases");
        builder.HasKey(phrase => phrase.Id);
        builder.Property(phrase => phrase.Id).ValueGeneratedNever();
        builder.Property(phrase => phrase.ConversationStarterPackId).IsRequired();
        builder.Property(phrase => phrase.SortOrder).IsRequired();
        builder.Property(phrase => phrase.BaseText).HasMaxLength(1024).IsRequired();
        builder.Property(phrase => phrase.Function).HasMaxLength(128).IsRequired();
        builder.Property(phrase => phrase.UsageNote).HasMaxLength(1024);
        builder.Property(phrase => phrase.Register).HasMaxLength(64);
        builder.Property(phrase => phrase.CommonMistake).HasMaxLength(1024);
        builder.Property(phrase => phrase.CreatedAtUtc).IsRequired();
        builder.Property(phrase => phrase.UpdatedAtUtc).IsRequired();
        builder.HasIndex(phrase => new { phrase.ConversationStarterPackId, phrase.SortOrder }).IsUnique();
        builder.HasMany(phrase => phrase.Translations).WithOne().HasForeignKey(translation => translation.ConversationStarterPhraseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(phrase => phrase.AlternativeBaseTexts).WithOne().HasForeignKey(alternative => alternative.ConversationStarterPhraseId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ConversationStarterPhraseTranslationConfiguration : IEntityTypeConfiguration<ConversationStarterPhraseTranslation>
{
    public void Configure(EntityTypeBuilder<ConversationStarterPhraseTranslation> builder)
    {
        builder.ToTable("ConversationStarterPhraseTranslations");
        builder.HasKey(translation => translation.Id);
        builder.Property(translation => translation.Id).ValueGeneratedNever();
        builder.Property(translation => translation.ConversationStarterPhraseId).IsRequired();
        builder.Property(translation => translation.LanguageCode)
            .HasConversion(languageCode => languageCode.Value, value => LanguageCode.From(value))
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(translation => translation.Text).HasMaxLength(2000).IsRequired();
        builder.Property(translation => translation.CreatedAtUtc).IsRequired();
        builder.Property(translation => translation.UpdatedAtUtc).IsRequired();
        builder.HasIndex(translation => new { translation.ConversationStarterPhraseId, translation.LanguageCode }).IsUnique();
    }
}

internal sealed class ConversationStarterPhraseAlternativeConfiguration : IEntityTypeConfiguration<ConversationStarterPhraseAlternative>
{
    public void Configure(EntityTypeBuilder<ConversationStarterPhraseAlternative> builder)
    {
        builder.ToTable("ConversationStarterPhraseAlternatives");
        builder.HasKey(alternative => alternative.Id);
        builder.Property(alternative => alternative.Id).ValueGeneratedNever();
        builder.Property(alternative => alternative.ConversationStarterPhraseId).IsRequired();
        builder.Property(alternative => alternative.SortOrder).IsRequired();
        builder.Property(alternative => alternative.BaseText).HasMaxLength(1024).IsRequired();
        builder.Property(alternative => alternative.CreatedAtUtc).IsRequired();
        builder.HasIndex(alternative => new { alternative.ConversationStarterPhraseId, alternative.SortOrder }).IsUnique();
    }
}

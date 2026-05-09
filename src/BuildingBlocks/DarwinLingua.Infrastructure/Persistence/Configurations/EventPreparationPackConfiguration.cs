using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class EventPreparationPackConfiguration : IEntityTypeConfiguration<EventPreparationPack>
{
    public void Configure(EntityTypeBuilder<EventPreparationPack> builder)
    {
        builder.ToTable("EventPreparationPacks");
        builder.HasKey(pack => pack.Id);
        builder.Property(pack => pack.Id).ValueGeneratedNever();
        builder.Property(pack => pack.Slug).HasMaxLength(128).IsRequired();
        builder.Property(pack => pack.Title).HasMaxLength(256).IsRequired();
        builder.Property(pack => pack.Description).HasMaxLength(4000).IsRequired();
        builder.Property(pack => pack.CefrLevel).HasConversion<string>().HasMaxLength(8).IsRequired();
        builder.Property(pack => pack.Category).HasMaxLength(128).IsRequired();
        builder.Property(pack => pack.EventType).HasMaxLength(128).IsRequired();
        builder.Property(pack => pack.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(pack => pack.SortOrder).IsRequired();
        builder.Property(pack => pack.CreatedAtUtc).IsRequired();
        builder.Property(pack => pack.UpdatedAtUtc).IsRequired();
        builder.HasIndex(pack => pack.Slug).IsUnique();
        builder.HasIndex(pack => new { pack.CefrLevel, pack.Category, pack.EventType });

        builder.HasMany(pack => pack.Topics).WithOne().HasForeignKey(topic => topic.EventPreparationPackId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(pack => pack.LinkedDialogues).WithOne().HasForeignKey(link => link.EventPreparationPackId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(pack => pack.LinkedConversationStarterPacks).WithOne().HasForeignKey(link => link.EventPreparationPackId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(pack => pack.LinkedVocabulary).WithOne().HasForeignKey(reference => reference.EventPreparationPackId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(pack => pack.Prompts).WithOne().HasForeignKey(prompt => prompt.EventPreparationPackId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class EventPreparationPackTopicConfiguration : IEntityTypeConfiguration<EventPreparationPackTopic>
{
    public void Configure(EntityTypeBuilder<EventPreparationPackTopic> builder)
    {
        builder.ToTable("EventPreparationPackTopics");
        builder.HasKey(topic => topic.Id);
        builder.Property(topic => topic.Id).ValueGeneratedNever();
        builder.Property(topic => topic.EventPreparationPackId).IsRequired();
        builder.Property(topic => topic.TopicId).IsRequired();
        builder.Property(topic => topic.IsPrimary).IsRequired();
        builder.Property(topic => topic.CreatedAtUtc).IsRequired();
        builder.HasIndex(topic => new { topic.EventPreparationPackId, topic.TopicId }).IsUnique();
        builder.HasIndex(topic => topic.TopicId).HasDatabaseName("IX_EventPreparationPackTopics_TopicId");
        builder.HasIndex(topic => topic.EventPreparationPackId).HasDatabaseName("IX_EventPreparationPackTopics_PrimaryPerPack").IsUnique().HasFilter($"\"{nameof(EventPreparationPackTopic.IsPrimary)}\"");
        builder.HasOne<Topic>().WithMany().HasForeignKey(topic => topic.TopicId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class EventPreparationLinkedDialogueConfiguration : IEntityTypeConfiguration<EventPreparationLinkedDialogue>
{
    public void Configure(EntityTypeBuilder<EventPreparationLinkedDialogue> builder)
    {
        builder.ToTable("EventPreparationLinkedDialogues");
        builder.HasKey(link => link.Id);
        builder.Property(link => link.Id).ValueGeneratedNever();
        builder.Property(link => link.EventPreparationPackId).IsRequired();
        builder.Property(link => link.DialogueSlug).HasMaxLength(128).IsRequired();
        builder.Property(link => link.SortOrder).IsRequired();
        builder.Property(link => link.CreatedAtUtc).IsRequired();
        builder.HasIndex(link => new { link.EventPreparationPackId, link.DialogueSlug }).IsUnique();
    }
}

internal sealed class EventPreparationLinkedConversationStarterPackConfiguration : IEntityTypeConfiguration<EventPreparationLinkedConversationStarterPack>
{
    public void Configure(EntityTypeBuilder<EventPreparationLinkedConversationStarterPack> builder)
    {
        builder.ToTable("EventPreparationLinkedConversationStarterPacks");
        builder.HasKey(link => link.Id);
        builder.Property(link => link.Id).ValueGeneratedNever();
        builder.Property(link => link.EventPreparationPackId).IsRequired();
        builder.Property(link => link.ConversationStarterPackSlug).HasMaxLength(128).IsRequired();
        builder.Property(link => link.SortOrder).IsRequired();
        builder.Property(link => link.CreatedAtUtc).IsRequired();
        builder.HasIndex(link => new { link.EventPreparationPackId, link.ConversationStarterPackSlug }).IsUnique();
    }
}

internal sealed class EventPreparationVocabularyReferenceConfiguration : IEntityTypeConfiguration<EventPreparationVocabularyReference>
{
    public void Configure(EntityTypeBuilder<EventPreparationVocabularyReference> builder)
    {
        builder.ToTable("EventPreparationVocabularyReferences");
        builder.HasKey(reference => reference.Id);
        builder.Property(reference => reference.Id).ValueGeneratedNever();
        builder.Property(reference => reference.EventPreparationPackId).IsRequired();
        builder.Property(reference => reference.Word).HasMaxLength(256).IsRequired();
        builder.Property(reference => reference.PartOfSpeech).HasConversion<string>().HasMaxLength(32);
        builder.Property(reference => reference.CefrLevel).HasConversion<string>().HasMaxLength(8);
        builder.Property(reference => reference.SortOrder).IsRequired();
        builder.Property(reference => reference.CreatedAtUtc).IsRequired();
        builder.HasIndex(reference => new { reference.EventPreparationPackId, reference.SortOrder }).IsUnique();
    }
}

internal sealed class EventPreparationPromptConfiguration : IEntityTypeConfiguration<EventPreparationPrompt>
{
    public void Configure(EntityTypeBuilder<EventPreparationPrompt> builder)
    {
        builder.ToTable("EventPreparationPrompts");
        builder.HasKey(prompt => prompt.Id);
        builder.Property(prompt => prompt.Id).ValueGeneratedNever();
        builder.Property(prompt => prompt.EventPreparationPackId).IsRequired();
        builder.Property(prompt => prompt.PromptType).HasMaxLength(64).IsRequired();
        builder.Property(prompt => prompt.SortOrder).IsRequired();
        builder.Property(prompt => prompt.Text).HasMaxLength(2000).IsRequired();
        builder.Property(prompt => prompt.CreatedAtUtc).IsRequired();
        builder.HasIndex(prompt => new { prompt.EventPreparationPackId, prompt.PromptType, prompt.SortOrder }).IsUnique();
    }
}

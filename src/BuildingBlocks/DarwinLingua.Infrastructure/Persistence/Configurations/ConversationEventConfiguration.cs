using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class ConversationEventConfiguration : IEntityTypeConfiguration<ConversationEvent>
{
    public void Configure(EntityTypeBuilder<ConversationEvent> builder)
    {
        builder.ToTable("ConversationEvents");
        builder.HasKey(conversationEvent => conversationEvent.Id);
        builder.Property(conversationEvent => conversationEvent.Id).ValueGeneratedNever();
        builder.Property(conversationEvent => conversationEvent.Slug).HasMaxLength(128).IsRequired();
        builder.Property(conversationEvent => conversationEvent.Name).HasMaxLength(256).IsRequired();
        builder.Property(conversationEvent => conversationEvent.Description).HasMaxLength(4000).IsRequired();
        builder.Property(conversationEvent => conversationEvent.City).HasMaxLength(128);
        builder.Property(conversationEvent => conversationEvent.CountryRegion).HasMaxLength(128).IsRequired();
        builder.Property(conversationEvent => conversationEvent.ApproximateLocation).HasMaxLength(512);
        builder.Property(conversationEvent => conversationEvent.IsOnline).IsRequired();
        builder.Property(conversationEvent => conversationEvent.Category).HasMaxLength(128).IsRequired();
        builder.Property(conversationEvent => conversationEvent.OrganizerName).HasMaxLength(256).IsRequired();
        builder.Property(conversationEvent => conversationEvent.OrganizerProfileSlug).HasMaxLength(128);
        builder.Property(conversationEvent => conversationEvent.ExternalLink).HasMaxLength(1024);
        builder.Property(conversationEvent => conversationEvent.ContactMethod).HasMaxLength(512);
        builder.Property(conversationEvent => conversationEvent.ScheduleText).HasMaxLength(1000).IsRequired();
        builder.Property(conversationEvent => conversationEvent.RecurrenceRule).HasMaxLength(256);
        builder.Property(conversationEvent => conversationEvent.Capacity);
        builder.Property(conversationEvent => conversationEvent.PriceType).HasMaxLength(64).IsRequired();
        builder.Property(conversationEvent => conversationEvent.VerificationStatus).HasMaxLength(64).IsRequired();
        builder.Property(conversationEvent => conversationEvent.SourceName).HasMaxLength(256);
        builder.Property(conversationEvent => conversationEvent.SourceUrl).HasMaxLength(1024);
        builder.Property(conversationEvent => conversationEvent.LastVerifiedAtUtc);
        builder.Property(conversationEvent => conversationEvent.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(conversationEvent => conversationEvent.SortOrder).IsRequired();
        builder.Property(conversationEvent => conversationEvent.CreatedAtUtc).IsRequired();
        builder.Property(conversationEvent => conversationEvent.UpdatedAtUtc).IsRequired();
        builder.HasIndex(conversationEvent => conversationEvent.Slug).IsUnique();
        builder.HasIndex(conversationEvent => conversationEvent.OrganizerProfileSlug);
        builder.HasIndex(conversationEvent => new { conversationEvent.City, conversationEvent.IsOnline, conversationEvent.PriceType });
        builder.HasIndex(conversationEvent => new { conversationEvent.Category, conversationEvent.PublicationStatus });

        builder.HasMany(conversationEvent => conversationEvent.SupportedLevels)
            .WithOne()
            .HasForeignKey(level => level.ConversationEventId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(conversationEvent => conversationEvent.HelperLanguages)
            .WithOne()
            .HasForeignKey(language => language.ConversationEventId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(conversationEvent => conversationEvent.PreparationPackLinks)
            .WithOne()
            .HasForeignKey(link => link.ConversationEventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ConversationEventLevelConfiguration : IEntityTypeConfiguration<ConversationEventLevel>
{
    public void Configure(EntityTypeBuilder<ConversationEventLevel> builder)
    {
        builder.ToTable("ConversationEventLevels");
        builder.HasKey(level => level.Id);
        builder.Property(level => level.Id).ValueGeneratedNever();
        builder.Property(level => level.ConversationEventId).IsRequired();
        builder.Property(level => level.CefrLevel).HasConversion<string>().HasMaxLength(8).IsRequired();
        builder.Property(level => level.SortOrder).IsRequired();
        builder.Property(level => level.CreatedAtUtc).IsRequired();
        builder.HasIndex(level => new { level.ConversationEventId, level.CefrLevel }).IsUnique();
    }
}

internal sealed class ConversationEventHelperLanguageConfiguration : IEntityTypeConfiguration<ConversationEventHelperLanguage>
{
    public void Configure(EntityTypeBuilder<ConversationEventHelperLanguage> builder)
    {
        builder.ToTable("ConversationEventHelperLanguages");
        builder.HasKey(language => language.Id);
        builder.Property(language => language.Id).ValueGeneratedNever();
        builder.Property(language => language.ConversationEventId).IsRequired();
        builder.Property(language => language.LanguageCode).HasMaxLength(16).IsRequired();
        builder.Property(language => language.SortOrder).IsRequired();
        builder.Property(language => language.CreatedAtUtc).IsRequired();
        builder.HasIndex(language => new { language.ConversationEventId, language.LanguageCode }).IsUnique();
    }
}

internal sealed class ConversationEventPreparationPackLinkConfiguration : IEntityTypeConfiguration<ConversationEventPreparationPackLink>
{
    public void Configure(EntityTypeBuilder<ConversationEventPreparationPackLink> builder)
    {
        builder.ToTable("ConversationEventPreparationPackLinks");
        builder.HasKey(link => link.Id);
        builder.Property(link => link.Id).ValueGeneratedNever();
        builder.Property(link => link.ConversationEventId).IsRequired();
        builder.Property(link => link.PreparationPackSlug).HasMaxLength(128).IsRequired();
        builder.Property(link => link.SortOrder).IsRequired();
        builder.Property(link => link.CreatedAtUtc).IsRequired();
        builder.HasIndex(link => new { link.ConversationEventId, link.PreparationPackSlug }).IsUnique();
    }
}

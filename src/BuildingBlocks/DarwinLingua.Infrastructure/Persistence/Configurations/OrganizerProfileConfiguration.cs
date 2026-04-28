using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class OrganizerProfileConfiguration : IEntityTypeConfiguration<OrganizerProfile>
{
    public void Configure(EntityTypeBuilder<OrganizerProfile> builder)
    {
        builder.ToTable("OrganizerProfiles");
        builder.HasKey(profile => profile.Id);
        builder.Property(profile => profile.Id).ValueGeneratedNever();
        builder.Property(profile => profile.Slug).HasMaxLength(128).IsRequired();
        builder.Property(profile => profile.DisplayName).HasMaxLength(256).IsRequired();
        builder.Property(profile => profile.OrganizerType).HasMaxLength(64).IsRequired();
        builder.Property(profile => profile.Description).HasMaxLength(4000).IsRequired();
        builder.Property(profile => profile.CityRegion).HasMaxLength(128);
        builder.Property(profile => profile.IsOnlineAvailable).IsRequired();
        builder.Property(profile => profile.WebsiteUrl).HasMaxLength(1024);
        builder.Property(profile => profile.PublicContactMethod).HasMaxLength(512);
        builder.Property(profile => profile.VerificationStatus).HasMaxLength(64).IsRequired();
        builder.Property(profile => profile.PlanKey).HasMaxLength(64).IsRequired();
        builder.Property(profile => profile.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(profile => profile.HistoricalEventCount).IsRequired();
        builder.Property(profile => profile.CreatedAtUtc).IsRequired();
        builder.Property(profile => profile.UpdatedAtUtc).IsRequired();
        builder.HasIndex(profile => profile.Slug).IsUnique();
        builder.HasIndex(profile => new { profile.OrganizerType, profile.PublicationStatus });
        builder.HasIndex(profile => new { profile.CityRegion, profile.PublicationStatus });

        builder.HasMany(profile => profile.SupportedLevels)
            .WithOne()
            .HasForeignKey(level => level.OrganizerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(profile => profile.HelperLanguages)
            .WithOne()
            .HasForeignKey(language => language.OrganizerProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class OrganizerProfileSupportedLevelConfiguration : IEntityTypeConfiguration<OrganizerProfileSupportedLevel>
{
    public void Configure(EntityTypeBuilder<OrganizerProfileSupportedLevel> builder)
    {
        builder.ToTable("OrganizerProfileSupportedLevels");
        builder.HasKey(level => level.Id);
        builder.Property(level => level.Id).ValueGeneratedNever();
        builder.Property(level => level.OrganizerProfileId).IsRequired();
        builder.Property(level => level.CefrLevel).HasConversion<string>().HasMaxLength(8).IsRequired();
        builder.Property(level => level.SortOrder).IsRequired();
        builder.Property(level => level.CreatedAtUtc).IsRequired();
        builder.HasIndex(level => new { level.OrganizerProfileId, level.CefrLevel }).IsUnique();
    }
}

internal sealed class OrganizerProfileHelperLanguageConfiguration : IEntityTypeConfiguration<OrganizerProfileHelperLanguage>
{
    public void Configure(EntityTypeBuilder<OrganizerProfileHelperLanguage> builder)
    {
        builder.ToTable("OrganizerProfileHelperLanguages");
        builder.HasKey(language => language.Id);
        builder.Property(language => language.Id).ValueGeneratedNever();
        builder.Property(language => language.OrganizerProfileId).IsRequired();
        builder.Property(language => language.LanguageCode).HasMaxLength(16).IsRequired();
        builder.Property(language => language.SortOrder).IsRequired();
        builder.Property(language => language.CreatedAtUtc).IsRequired();
        builder.HasIndex(language => new { language.OrganizerProfileId, language.LanguageCode }).IsUnique();
    }
}

using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class LearnerConversationProfileConfiguration : IEntityTypeConfiguration<LearnerConversationProfile>
{
    public void Configure(EntityTypeBuilder<LearnerConversationProfile> builder)
    {
        builder.ToTable("LearnerConversationProfiles");
        builder.HasKey(profile => profile.Id);
        builder.Property(profile => profile.Id).ValueGeneratedNever();
        builder.Property(profile => profile.OwnerEmail).HasMaxLength(320).IsRequired();
        builder.Property(profile => profile.DisplayName).HasMaxLength(128).IsRequired();
        builder.Property(profile => profile.CityRegion).HasMaxLength(128);
        builder.Property(profile => profile.InteractionPreference).HasMaxLength(32).IsRequired();
        builder.Property(profile => profile.GermanLevel).HasMaxLength(8).IsRequired();
        builder.Property(profile => profile.HelperLanguageCodes).HasMaxLength(256).IsRequired();
        builder.Property(profile => profile.ConversationGoals).HasMaxLength(1000).IsRequired();
        builder.Property(profile => profile.AvailabilityNotes).HasMaxLength(1000);
        builder.Property(profile => profile.Visibility).HasMaxLength(32).IsRequired();
        builder.Property(profile => profile.HasConfirmedAdult).IsRequired();
        builder.Property(profile => profile.CreatedAtUtc).IsRequired();
        builder.Property(profile => profile.UpdatedAtUtc).IsRequired();
        builder.HasIndex(profile => profile.OwnerEmail).IsUnique();
        builder.HasIndex(profile => new { profile.Visibility, profile.CityRegion, profile.GermanLevel });
    }
}

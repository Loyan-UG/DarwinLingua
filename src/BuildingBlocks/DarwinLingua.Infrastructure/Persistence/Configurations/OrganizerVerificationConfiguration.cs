using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class OrganizerVerificationConfiguration : IEntityTypeConfiguration<OrganizerVerification>
{
    public void Configure(EntityTypeBuilder<OrganizerVerification> builder)
    {
        builder.ToTable("OrganizerVerifications");
        builder.HasKey(verification => verification.Id);
        builder.Property(verification => verification.Id).ValueGeneratedNever();
        builder.Property(verification => verification.OrganizerProfileSlug).HasMaxLength(128).IsRequired();
        builder.Property(verification => verification.Status).HasMaxLength(64).IsRequired();
        builder.Property(verification => verification.RequestedByEmail).HasMaxLength(320).IsRequired();
        builder.Property(verification => verification.CreatedAtUtc).IsRequired();
        builder.Property(verification => verification.UpdatedAtUtc).IsRequired();
        builder.HasIndex(verification => new { verification.OrganizerProfileSlug, verification.Status });
    }
}

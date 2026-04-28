using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class OrganizerProfileOwnerConfiguration : IEntityTypeConfiguration<OrganizerProfileOwner>
{
    public void Configure(EntityTypeBuilder<OrganizerProfileOwner> builder)
    {
        builder.ToTable("OrganizerProfileOwners");
        builder.HasKey(owner => owner.Id);
        builder.Property(owner => owner.Id).ValueGeneratedNever();
        builder.Property(owner => owner.OrganizerProfileSlug).HasMaxLength(128).IsRequired();
        builder.Property(owner => owner.OwnerEmail).HasMaxLength(320).IsRequired();
        builder.Property(owner => owner.AssignedBy).HasMaxLength(320).IsRequired();
        builder.Property(owner => owner.CreatedAtUtc).IsRequired();
        builder.HasIndex(owner => new { owner.OrganizerProfileSlug, owner.OwnerEmail }).IsUnique();
        builder.HasIndex(owner => owner.OwnerEmail);
    }
}

using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class OrganizerClaimRequestConfiguration : IEntityTypeConfiguration<OrganizerClaimRequest>
{
    public void Configure(EntityTypeBuilder<OrganizerClaimRequest> builder)
    {
        builder.ToTable("OrganizerClaimRequests");
        builder.HasKey(claimRequest => claimRequest.Id);
        builder.Property(claimRequest => claimRequest.Id).ValueGeneratedNever();
        builder.Property(claimRequest => claimRequest.OrganizerProfileSlug).HasMaxLength(128).IsRequired();
        builder.Property(claimRequest => claimRequest.RequesterName).HasMaxLength(256).IsRequired();
        builder.Property(claimRequest => claimRequest.RequesterEmail).HasMaxLength(320).IsRequired();
        builder.Property(claimRequest => claimRequest.RelationshipToOrganizer).HasMaxLength(256).IsRequired();
        builder.Property(claimRequest => claimRequest.EvidenceText).HasMaxLength(4000).IsRequired();
        builder.Property(claimRequest => claimRequest.Status).HasMaxLength(64).IsRequired();
        builder.Property(claimRequest => claimRequest.CreatedAtUtc).IsRequired();
        builder.Property(claimRequest => claimRequest.UpdatedAtUtc).IsRequired();
        builder.HasIndex(claimRequest => new { claimRequest.OrganizerProfileSlug, claimRequest.Status });
        builder.HasIndex(claimRequest => new { claimRequest.RequesterEmail, claimRequest.OrganizerProfileSlug, claimRequest.Status });
    }
}

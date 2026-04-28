using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class PartnerRequestConfiguration : IEntityTypeConfiguration<PartnerRequest>
{
    public void Configure(EntityTypeBuilder<PartnerRequest> builder)
    {
        builder.ToTable("PartnerRequests");
        builder.HasKey(request => request.Id);
        builder.Property(request => request.Id).ValueGeneratedNever();
        builder.Property(request => request.RequesterEmail).HasMaxLength(320).IsRequired();
        builder.Property(request => request.TargetLearnerProfileId).IsRequired();
        builder.Property(request => request.OpenerTemplateKey).HasMaxLength(64).IsRequired();
        builder.Property(request => request.Note).HasMaxLength(500);
        builder.Property(request => request.Status).HasMaxLength(64).IsRequired();
        builder.Property(request => request.CreatedAtUtc).IsRequired();
        builder.Property(request => request.UpdatedAtUtc).IsRequired();
        builder.Property(request => request.ExpiresAtUtc).IsRequired();
        builder.HasIndex(request => new { request.RequesterEmail, request.TargetLearnerProfileId, request.Status });
        builder.HasIndex(request => new { request.TargetLearnerProfileId, request.Status });
        builder.HasIndex(request => new { request.RequesterEmail, request.CreatedAtUtc });
    }
}

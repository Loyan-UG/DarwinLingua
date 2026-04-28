using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class ModerationDecisionAuditConfiguration : IEntityTypeConfiguration<ModerationDecisionAudit>
{
    public void Configure(EntityTypeBuilder<ModerationDecisionAudit> builder)
    {
        builder.ToTable("ModerationDecisionAudits");
        builder.HasKey(audit => audit.Id);
        builder.Property(audit => audit.Id).ValueGeneratedNever();
        builder.Property(audit => audit.UserReportId).IsRequired();
        builder.Property(audit => audit.DecisionStatus).HasMaxLength(64).IsRequired();
        builder.Property(audit => audit.DecidedBy).HasMaxLength(320).IsRequired();
        builder.Property(audit => audit.DecisionNote).HasMaxLength(1000);
        builder.Property(audit => audit.CreatedAtUtc).IsRequired();
        builder.HasIndex(audit => new { audit.UserReportId, audit.CreatedAtUtc });
    }
}

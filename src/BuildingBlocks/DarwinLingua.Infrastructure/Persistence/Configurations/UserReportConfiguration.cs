using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class UserReportConfiguration : IEntityTypeConfiguration<UserReport>
{
    public void Configure(EntityTypeBuilder<UserReport> builder)
    {
        builder.ToTable("UserReports");
        builder.HasKey(report => report.Id);
        builder.Property(report => report.Id).ValueGeneratedNever();
        builder.Property(report => report.ReporterEmail).HasMaxLength(320).IsRequired();
        builder.Property(report => report.TargetType).HasMaxLength(64).IsRequired();
        builder.Property(report => report.TargetKey).HasMaxLength(256).IsRequired();
        builder.Property(report => report.ReportedUserEmail).HasMaxLength(320);
        builder.Property(report => report.Reason).HasMaxLength(64).IsRequired();
        builder.Property(report => report.Details).HasMaxLength(2000).IsRequired();
        builder.Property(report => report.Status).HasMaxLength(64).IsRequired();
        builder.Property(report => report.DecisionNote).HasMaxLength(1000);
        builder.Property(report => report.DecidedBy).HasMaxLength(320);
        builder.Property(report => report.CreatedAtUtc).IsRequired();
        builder.Property(report => report.UpdatedAtUtc).IsRequired();
        builder.HasIndex(report => new { report.Status, report.CreatedAtUtc });
        builder.HasIndex(report => new { report.TargetType, report.TargetKey });
    }
}

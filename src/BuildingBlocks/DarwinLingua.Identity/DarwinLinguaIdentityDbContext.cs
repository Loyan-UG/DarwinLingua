using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Identity;

public class DarwinLinguaIdentityDbContext(DbContextOptions options)
    : IdentityDbContext<DarwinLinguaIdentityUser, IdentityRole, string>(options)
{
    public DbSet<UserEntitlementState> UserEntitlementStates => Set<UserEntitlementState>();

    public DbSet<UserEntitlementAuditEvent> UserEntitlementAuditEvents => Set<UserEntitlementAuditEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        base.OnModelCreating(builder);

        builder.Entity<UserEntitlementState>(entity =>
        {
            entity.ToTable("UserEntitlementStates");
            entity.HasKey(entitlement => entitlement.UserId);
            entity.Property(entitlement => entitlement.UserId).HasMaxLength(450);
            entity.Property(entitlement => entitlement.Tier).HasMaxLength(32).IsRequired();
            entity.Property(entitlement => entitlement.LastUpdatedBy).HasMaxLength(256);
        });

        builder.Entity<UserEntitlementAuditEvent>(entity =>
        {
            entity.ToTable("UserEntitlementAuditEvents");
            entity.HasKey(auditEvent => auditEvent.Id);
            entity.Property(auditEvent => auditEvent.UserId).HasMaxLength(450).IsRequired();
            entity.Property(auditEvent => auditEvent.EventType).HasMaxLength(64).IsRequired();
            entity.Property(auditEvent => auditEvent.PreviousTier).HasMaxLength(32);
            entity.Property(auditEvent => auditEvent.NewTier).HasMaxLength(32).IsRequired();
            entity.Property(auditEvent => auditEvent.UpdatedBy).HasMaxLength(256).IsRequired();
            entity.HasIndex(auditEvent => new { auditEvent.UserId, auditEvent.CreatedAtUtc });
        });
    }
}

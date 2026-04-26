using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Identity;

public class DarwinLinguaIdentityDbContext(DbContextOptions options)
    : IdentityDbContext<DarwinLinguaIdentityUser, IdentityRole, string>(options)
{
    public DbSet<UserEntitlementState> UserEntitlementStates => Set<UserEntitlementState>();

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
    }
}

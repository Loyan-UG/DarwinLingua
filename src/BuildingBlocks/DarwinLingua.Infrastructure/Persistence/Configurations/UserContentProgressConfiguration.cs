using DarwinLingua.Learning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class UserContentProgressConfiguration : IEntityTypeConfiguration<UserContentProgress>
{
    public void Configure(EntityTypeBuilder<UserContentProgress> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("UserContentProgress");
        builder.HasKey(progress => progress.Id);
        builder.Property(progress => progress.Id).ValueGeneratedNever();
        builder.Property(progress => progress.UserId).HasMaxLength(256).IsRequired();
        builder.Property(progress => progress.ContentOwnerType).HasMaxLength(64).IsRequired();
        builder.Property(progress => progress.ContentOwnerSlug).HasMaxLength(256).IsRequired();
        builder.Property(progress => progress.State).HasMaxLength(32).IsRequired();
        builder.Property(progress => progress.ViewCount).IsRequired();
        builder.Property(progress => progress.CreatedAtUtc).IsRequired();
        builder.Property(progress => progress.UpdatedAtUtc).IsRequired();

        builder.HasIndex(progress => new
            {
                progress.UserId,
                progress.ContentOwnerType,
                progress.ContentOwnerSlug,
            })
            .IsUnique();
        builder.HasIndex(progress => new { progress.UserId, progress.State });
        builder.HasIndex(progress => new { progress.UserId, progress.UpdatedAtUtc });
    }
}

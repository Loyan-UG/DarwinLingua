using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class UserBlockConfiguration : IEntityTypeConfiguration<UserBlock>
{
    public void Configure(EntityTypeBuilder<UserBlock> builder)
    {
        builder.ToTable("UserBlocks");
        builder.HasKey(block => block.Id);
        builder.Property(block => block.Id).ValueGeneratedNever();
        builder.Property(block => block.BlockerEmail).HasMaxLength(320).IsRequired();
        builder.Property(block => block.BlockedEmail).HasMaxLength(320).IsRequired();
        builder.Property(block => block.Reason).HasMaxLength(500);
        builder.Property(block => block.CreatedAtUtc).IsRequired();
        builder.HasIndex(block => new { block.BlockerEmail, block.BlockedEmail }).IsUnique();
        builder.HasIndex(block => block.BlockedEmail);
    }
}

using DarwinLingua.ContentOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence details for the <see cref="ContentPackageEntry"/> entity.
/// </summary>
internal sealed class ContentPackageEntryConfiguration : IEntityTypeConfiguration<ContentPackageEntry>
{
    public void Configure(EntityTypeBuilder<ContentPackageEntry> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("ContentPackageEntries");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id)
            .ValueGeneratedNever();

        builder.Property(entry => entry.RawLemma)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(entry => entry.NormalizedLemma)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(entry => entry.CefrLevel)
            .HasMaxLength(16);

        builder.Property(entry => entry.PartOfSpeech)
            .HasMaxLength(32);

        builder.Property(entry => entry.ProcessingStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(entry => entry.ErrorMessage)
            .HasMaxLength(2048);

        builder.Property(entry => entry.WarningMessage)
            .HasMaxLength(2048);

        builder.Property(entry => entry.CreatedAtUtc)
            .IsRequired();

        builder.Property(entry => entry.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(entry => entry.ContentPackageId);
    }
}

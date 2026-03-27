using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence details for lexical word labels.
/// </summary>
internal sealed class WordLabelConfiguration : IEntityTypeConfiguration<WordLabel>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<WordLabel> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("WordLabels");

        builder.HasKey(label => label.Id);

        builder.Property(label => label.Id)
            .ValueGeneratedNever();

        builder.Property(label => label.WordEntryId)
            .IsRequired();

        builder.Property(label => label.Kind)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(label => label.Key)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(label => label.SortOrder)
            .IsRequired();

        builder.Property(label => label.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(label => new { label.WordEntryId, label.Kind, label.Key })
            .IsUnique();

        builder.HasIndex(label => new { label.WordEntryId, label.Kind, label.SortOrder })
            .IsUnique();
    }
}

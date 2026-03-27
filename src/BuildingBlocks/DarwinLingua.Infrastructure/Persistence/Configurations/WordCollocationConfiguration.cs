using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence details for learner-facing collocations.
/// </summary>
internal sealed class WordCollocationConfiguration : IEntityTypeConfiguration<WordCollocation>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<WordCollocation> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("WordCollocations");

        builder.HasKey(collocation => collocation.Id);

        builder.Property(collocation => collocation.Id)
            .ValueGeneratedNever();

        builder.Property(collocation => collocation.WordEntryId)
            .IsRequired();

        builder.Property(collocation => collocation.Text)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(collocation => collocation.Meaning)
            .HasMaxLength(256);

        builder.Property(collocation => collocation.SortOrder)
            .IsRequired();

        builder.Property(collocation => collocation.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(collocation => new { collocation.WordEntryId, collocation.Text })
            .IsUnique();

        builder.HasIndex(collocation => new { collocation.WordEntryId, collocation.SortOrder })
            .IsUnique();
    }
}

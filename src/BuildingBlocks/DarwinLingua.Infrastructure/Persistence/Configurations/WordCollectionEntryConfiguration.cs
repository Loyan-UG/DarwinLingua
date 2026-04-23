using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class WordCollectionEntryConfiguration : IEntityTypeConfiguration<WordCollectionEntry>
{
    public void Configure(EntityTypeBuilder<WordCollectionEntry> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("WordCollectionEntries");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id)
            .ValueGeneratedNever();

        builder.Property(entry => entry.WordCollectionId)
            .IsRequired();

        builder.Property(entry => entry.WordEntryId)
            .IsRequired();

        builder.Property(entry => entry.SortOrder)
            .IsRequired();

        builder.Property(entry => entry.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(entry => new { entry.WordCollectionId, entry.SortOrder })
            .IsUnique();

        builder.HasIndex(entry => new { entry.WordCollectionId, entry.WordEntryId })
            .IsUnique();

        builder.HasOne(entry => entry.WordEntry)
            .WithMany()
            .HasForeignKey(entry => entry.WordEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

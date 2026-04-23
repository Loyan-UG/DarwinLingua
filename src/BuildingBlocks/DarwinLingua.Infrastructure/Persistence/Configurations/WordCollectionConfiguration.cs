using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class WordCollectionConfiguration : IEntityTypeConfiguration<WordCollection>
{
    public void Configure(EntityTypeBuilder<WordCollection> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("WordCollections");

        builder.HasKey(collection => collection.Id);

        builder.Property(collection => collection.Id)
            .ValueGeneratedNever();

        builder.Property(collection => collection.Slug)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(collection => collection.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(collection => collection.Description)
            .HasMaxLength(4000);

        builder.Property(collection => collection.ImageUrl)
            .HasMaxLength(1024);

        builder.Property(collection => collection.PublicationStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(collection => collection.SortOrder)
            .IsRequired();

        builder.Property(collection => collection.CreatedAtUtc)
            .IsRequired();

        builder.Property(collection => collection.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(collection => collection.Slug)
            .IsUnique();

        builder.HasMany(collection => collection.Entries)
            .WithOne()
            .HasForeignKey(entry => entry.WordCollectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

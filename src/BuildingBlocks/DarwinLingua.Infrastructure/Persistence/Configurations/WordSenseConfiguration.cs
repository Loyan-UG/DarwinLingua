using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence details for the <see cref="WordSense"/> entity.
/// </summary>
internal sealed class WordSenseConfiguration : IEntityTypeConfiguration<WordSense>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<WordSense> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("WordSenses");

        builder.HasKey(sense => sense.Id);

        builder.Property(sense => sense.Id)
            .ValueGeneratedNever();

        builder.Property(sense => sense.WordEntryId)
            .IsRequired();

        builder.Property(sense => sense.SenseOrder)
            .IsRequired();

        builder.Property(sense => sense.IsPrimarySense)
            .IsRequired();

        builder.Property(sense => sense.ShortDefinitionDe)
            .HasMaxLength(512);

        builder.Property(sense => sense.ShortGloss)
            .HasMaxLength(512);

        builder.Property(sense => sense.PublicationStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(sense => sense.CreatedAtUtc)
            .IsRequired();

        builder.Property(sense => sense.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(sense => new { sense.WordEntryId, sense.SenseOrder })
            .IsUnique();

        builder.HasIndex(sense => sense.WordEntryId)
            .HasDatabaseName("IX_WordSenses_PrimaryPerWordEntry")
            .IsUnique()
            .HasFilter($"\"{nameof(WordSense.IsPrimarySense)}\"");

        builder.HasMany(sense => sense.Translations)
            .WithOne()
            .HasForeignKey(translation => translation.WordSenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(sense => sense.Examples)
            .WithOne()
            .HasForeignKey(example => example.WordSenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(sense => sense.Translations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(sense => sense.Examples)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

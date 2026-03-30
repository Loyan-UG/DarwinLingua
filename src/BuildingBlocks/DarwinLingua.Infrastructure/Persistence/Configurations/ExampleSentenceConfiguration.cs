using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence details for the <see cref="ExampleSentence"/> entity.
/// </summary>
internal sealed class ExampleSentenceConfiguration : IEntityTypeConfiguration<ExampleSentence>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ExampleSentence> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("ExampleSentences");

        builder.HasKey(example => example.Id);

        builder.Property(example => example.Id)
            .ValueGeneratedNever();

        builder.Property(example => example.WordSenseId)
            .IsRequired();

        builder.Property(example => example.SentenceOrder)
            .IsRequired();

        builder.Property(example => example.GermanText)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(example => example.IsPrimaryExample)
            .IsRequired();

        builder.Property(example => example.CreatedAtUtc)
            .IsRequired();

        builder.Property(example => example.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(example => new { example.WordSenseId, example.SentenceOrder })
            .IsUnique();

        builder.HasIndex(example => example.WordSenseId)
            .HasDatabaseName("IX_ExampleSentences_PrimaryPerSense")
            .IsUnique()
            .HasFilter($"\"{nameof(ExampleSentence.IsPrimaryExample)}\"");

        builder.HasMany(example => example.Translations)
            .WithOne()
            .HasForeignKey(translation => translation.ExampleSentenceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(example => example.Translations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

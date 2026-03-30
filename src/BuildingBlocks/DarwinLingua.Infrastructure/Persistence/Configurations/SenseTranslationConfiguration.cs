using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence details for the <see cref="SenseTranslation"/> entity.
/// </summary>
internal sealed class SenseTranslationConfiguration : IEntityTypeConfiguration<SenseTranslation>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SenseTranslation> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("SenseTranslations");

        builder.HasKey(translation => translation.Id);

        builder.Property(translation => translation.Id)
            .ValueGeneratedNever();

        builder.Property(translation => translation.WordSenseId)
            .IsRequired();

        builder.Property(translation => translation.LanguageCode)
            .HasConversion(
                languageCode => languageCode.Value,
                value => LanguageCode.From(value))
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(translation => translation.TranslationText)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(translation => translation.IsPrimary)
            .IsRequired();

        builder.Property(translation => translation.CreatedAtUtc)
            .IsRequired();

        builder.Property(translation => translation.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(translation => new { translation.WordSenseId, translation.LanguageCode })
            .IsUnique();

        builder.HasIndex(translation => translation.WordSenseId)
            .HasDatabaseName("IX_SenseTranslations_PrimaryPerSense")
            .IsUnique()
            .HasFilter($"\"{nameof(SenseTranslation.IsPrimary)}\"");
    }
}

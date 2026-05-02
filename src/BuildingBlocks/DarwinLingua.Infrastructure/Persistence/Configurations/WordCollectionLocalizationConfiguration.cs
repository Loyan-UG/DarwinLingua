using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class WordCollectionLocalizationConfiguration : IEntityTypeConfiguration<WordCollectionLocalization>
{
    public void Configure(EntityTypeBuilder<WordCollectionLocalization> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("WordCollectionLocalizations");

        builder.HasKey(localization => localization.Id);

        builder.Property(localization => localization.Id)
            .ValueGeneratedNever();

        builder.Property(localization => localization.WordCollectionId)
            .IsRequired();

        builder.Property(localization => localization.LanguageCode)
            .HasConversion(
                languageCode => languageCode.Value,
                value => LanguageCode.From(value))
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(localization => localization.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(localization => localization.Description)
            .HasMaxLength(4000);

        builder.Property(localization => localization.CreatedAtUtc)
            .IsRequired();

        builder.Property(localization => localization.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(localization => new { localization.WordCollectionId, localization.LanguageCode })
            .IsUnique();
    }
}

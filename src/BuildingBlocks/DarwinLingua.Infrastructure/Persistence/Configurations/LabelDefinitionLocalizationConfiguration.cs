using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class LabelDefinitionLocalizationConfiguration : IEntityTypeConfiguration<LabelDefinitionLocalization>
{
    public void Configure(EntityTypeBuilder<LabelDefinitionLocalization> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("LabelDefinitionLocalizations");

        builder.HasKey(localization => localization.Id);

        builder.Property(localization => localization.Id)
            .ValueGeneratedNever();

        builder.Property(localization => localization.LabelDefinitionId)
            .IsRequired();

        builder.Property(localization => localization.LanguageCode)
            .HasConversion(
                languageCode => languageCode.Value,
                value => LanguageCode.From(value))
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(localization => localization.DisplayName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(localization => localization.CreatedAtUtc)
            .IsRequired();

        builder.Property(localization => localization.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(localization => new { localization.LabelDefinitionId, localization.LanguageCode })
            .IsUnique();
    }
}

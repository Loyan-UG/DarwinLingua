using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class LabelDefinitionConfiguration : IEntityTypeConfiguration<LabelDefinition>
{
    public void Configure(EntityTypeBuilder<LabelDefinition> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("LabelDefinitions");

        builder.HasKey(label => label.Id);

        builder.Property(label => label.Id)
            .ValueGeneratedNever();

        builder.Property(label => label.Kind)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(label => label.Key)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(label => label.DisplayName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(label => label.SortOrder)
            .IsRequired();

        builder.Property(label => label.IsSystem)
            .IsRequired();

        builder.Property(label => label.CreatedAtUtc)
            .IsRequired();

        builder.Property(label => label.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(label => new { label.Kind, label.Key })
            .IsUnique();

        builder.HasMany(label => label.Localizations)
            .WithOne()
            .HasForeignKey(localization => localization.LabelDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(label => label.Localizations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

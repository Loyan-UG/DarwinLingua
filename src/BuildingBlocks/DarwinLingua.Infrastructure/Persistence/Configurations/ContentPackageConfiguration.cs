using DarwinLingua.ContentOps.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence details for the <see cref="ContentPackage"/> entity.
/// </summary>
internal sealed class ContentPackageConfiguration : IEntityTypeConfiguration<ContentPackage>
{
    public void Configure(EntityTypeBuilder<ContentPackage> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("ContentPackages");

        builder.HasKey(contentPackage => contentPackage.Id);

        builder.Property(contentPackage => contentPackage.Id)
            .ValueGeneratedNever();

        builder.Property(contentPackage => contentPackage.PackageId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(contentPackage => contentPackage.PackageVersion)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(contentPackage => contentPackage.PackageName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(contentPackage => contentPackage.TargetLearningLanguageCode)
            .HasMaxLength(16)
            .HasDefaultValue(ContentLanguageRequirements.DefaultTargetLearningLanguageCode)
            .IsRequired();

        builder.Property(contentPackage => contentPackage.LevelSystemCode)
            .HasMaxLength(32);

        builder.Property(contentPackage => contentPackage.CountryContextCode)
            .HasMaxLength(8);

        builder.Property(contentPackage => contentPackage.SourceType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(contentPackage => contentPackage.InputFileName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(contentPackage => contentPackage.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(contentPackage => contentPackage.CreatedAtUtc)
            .IsRequired();

        builder.Property(contentPackage => contentPackage.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(contentPackage => new { contentPackage.TargetLearningLanguageCode, contentPackage.PackageId })
            .IsUnique();

        builder.HasMany(contentPackage => contentPackage.Entries)
            .WithOne()
            .HasForeignKey(entry => entry.ContentPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(contentPackage => contentPackage.Entries)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

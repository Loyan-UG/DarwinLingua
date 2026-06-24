using DarwinLingua.Learning.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence details for the <see cref="UserLearningProfile"/> entity.
/// </summary>
internal sealed class UserLearningProfileConfiguration : IEntityTypeConfiguration<UserLearningProfile>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserLearningProfile> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("UserLearningProfiles");

        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.Id)
            .ValueGeneratedNever();

        builder.Property(profile => profile.UserId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(profile => profile.PreferredMeaningLanguage1)
            .HasConversion(
                languageCode => languageCode.Value,
                value => LanguageCode.From(value))
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(profile => profile.PreferredMeaningLanguage2)
            .HasConversion(
                languageCode => languageCode.HasValue ? languageCode.Value.Value : null,
                value => string.IsNullOrWhiteSpace(value) ? (LanguageCode?)null : LanguageCode.From(value))
            .HasMaxLength(16);

        builder.Property(profile => profile.UiLanguageCode)
            .HasConversion(
                languageCode => languageCode.Value,
                value => LanguageCode.From(value))
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(profile => profile.TargetLearningLanguageCode)
            .HasConversion(
                languageCode => languageCode.Value,
                value => LanguageCode.From(value))
            .HasMaxLength(16)
            .HasDefaultValue(LanguageCode.From(ContentLanguageRequirements.DefaultTargetLearningLanguageCode))
            .IsRequired();

        builder.Property(profile => profile.CreatedAtUtc)
            .IsRequired();

        builder.Property(profile => profile.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(profile => profile.UserId)
            .IsUnique();
    }
}

using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class CountryGuidanceNoteConfiguration : IEntityTypeConfiguration<CountryGuidanceNote>
{
    public void Configure(EntityTypeBuilder<CountryGuidanceNote> builder)
    {
        builder.ToTable("CountryGuidanceNotes");
        builder.HasKey(note => note.Id);
        builder.Property(note => note.Id).ValueGeneratedNever();
        builder.Property(note => note.TargetLearningLanguageCode)
            .HasMaxLength(16)
            .HasDefaultValue(ContentLanguageRequirements.DefaultTargetLearningLanguageCode)
            .IsRequired();
        builder.Property(note => note.CountryContextCode)
            .HasMaxLength(8)
            .HasDefaultValue(CountryContextCatalog.Germany.Code)
            .IsRequired();
        builder.Property(note => note.Slug).HasMaxLength(128).IsRequired();
        builder.Property(note => note.Title).HasMaxLength(256).IsRequired();
        builder.Property(note => note.ShortDescription).HasMaxLength(1000).IsRequired();
        builder.Property(note => note.CefrLevel).HasConversion<string>().HasMaxLength(8).IsRequired();
        builder.Property(note => note.Category).HasMaxLength(96).IsRequired();
        builder.Property(note => note.Context).HasMaxLength(512).IsRequired();
        builder.Property(note => note.SectionsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.ExamplesJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.DoNotesJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.DontNotesJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.SensitivityWarning).HasMaxLength(1000);
        builder.Property(note => note.TitleTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.ShortDescriptionTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.ContextTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.SectionsTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.ExamplesTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.DoNotesTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.DontNotesTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.SensitivityWarningTranslationsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.LinkedDialogueSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.LinkedExpressionSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.LinkedWritingTemplateSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.LinkedTalkTopicSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.LinkedCourseLessonSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(note => note.SortOrder).IsRequired();
        builder.Property(note => note.CreatedAtUtc).IsRequired();
        builder.Property(note => note.UpdatedAtUtc).IsRequired();
        builder
            .HasIndex(note => new { note.TargetLearningLanguageCode, note.CountryContextCode, note.Slug })
            .HasDatabaseName("IX_CountryGuidanceNotes_TargetCountrySlug")
            .IsUnique();
        builder
            .HasIndex(note => new { note.TargetLearningLanguageCode, note.CountryContextCode, note.CefrLevel, note.Category })
            .HasDatabaseName("IX_CountryGuidanceNotes_TargetCountryFilters");
    }
}

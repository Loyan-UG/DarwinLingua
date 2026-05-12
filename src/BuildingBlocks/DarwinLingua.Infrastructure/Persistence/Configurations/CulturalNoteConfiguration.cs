using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class CulturalNoteConfiguration : IEntityTypeConfiguration<CulturalNote>
{
    public void Configure(EntityTypeBuilder<CulturalNote> builder)
    {
        builder.ToTable("CulturalNotes");
        builder.HasKey(note => note.Id);
        builder.Property(note => note.Id).ValueGeneratedNever();
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
        builder.Property(note => note.LinkedDialogueSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.LinkedExpressionSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.LinkedWritingTemplateSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.LinkedTalkTopicSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.LinkedCourseLessonSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(note => note.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(note => note.SortOrder).IsRequired();
        builder.Property(note => note.CreatedAtUtc).IsRequired();
        builder.Property(note => note.UpdatedAtUtc).IsRequired();
        builder.HasIndex(note => note.Slug).IsUnique();
        builder.HasIndex(note => new { note.CefrLevel, note.Category });
    }
}

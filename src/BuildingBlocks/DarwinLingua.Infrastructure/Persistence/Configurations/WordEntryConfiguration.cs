using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence details for the <see cref="WordEntry"/> aggregate.
/// </summary>
internal sealed class WordEntryConfiguration : IEntityTypeConfiguration<WordEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<WordEntry> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("WordEntries");

        builder.HasKey(word => word.Id);

        builder.Property(word => word.Id)
            .ValueGeneratedNever();

        builder.Property(word => word.PublicId)
            .ValueGeneratedNever();

        builder.Property(word => word.Lemma)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(word => word.NormalizedLemma)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(word => word.LanguageCode)
            .HasConversion(
                languageCode => languageCode.Value,
                value => LanguageCode.From(value))
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(word => word.PrimaryCefrLevel)
            .HasConversion<string>()
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(word => word.PartOfSpeech)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(word => word.Article)
            .HasMaxLength(32);

        builder.Property(word => word.PluralForm)
            .HasMaxLength(256);

        builder.Property(word => word.InfinitiveForm)
            .HasMaxLength(256);

        builder.Property(word => word.PronunciationIpa)
            .HasMaxLength(256);

        builder.Property(word => word.SyllableBreak)
            .HasMaxLength(256);

        builder.Property(word => word.PublicationStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(word => word.ContentSourceType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(word => word.SourceReference)
            .HasMaxLength(512);

        builder.Property(word => word.CreatedAtUtc)
            .IsRequired();

        builder.Property(word => word.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(word => word.PublicId)
            .IsUnique();

        builder.HasIndex(word => word.NormalizedLemma)
            .HasDatabaseName("IX_WordEntries_Search_NormalizedLemma");

        builder.HasIndex(word => new
            {
                word.PublicationStatus,
                word.NormalizedLemma,
            })
            .HasDatabaseName("IX_WordEntries_Search_ActiveNormalizedLemma");

        builder.HasIndex(word => new
            {
                word.PrimaryCefrLevel,
                word.NormalizedLemma,
            })
            .HasDatabaseName("IX_WordEntries_Browse_Cefr_NormalizedLemma");

        builder.HasIndex(word => new { word.NormalizedLemma, word.PartOfSpeech, word.PrimaryCefrLevel })
            .IsUnique();

        builder.HasMany(word => word.Senses)
            .WithOne()
            .HasForeignKey(sense => sense.WordEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(word => word.Topics)
            .WithOne()
            .HasForeignKey(topic => topic.WordEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(word => word.Senses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(word => word.Topics)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

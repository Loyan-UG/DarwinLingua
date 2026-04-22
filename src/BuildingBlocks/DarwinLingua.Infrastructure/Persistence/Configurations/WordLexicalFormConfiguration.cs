using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence details for <see cref="WordLexicalForm"/> rows.
/// </summary>
internal sealed class WordLexicalFormConfiguration : IEntityTypeConfiguration<WordLexicalForm>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<WordLexicalForm> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("WordLexicalForms");

        builder.HasKey(form => form.Id);

        builder.Property(form => form.Id)
            .ValueGeneratedNever();

        builder.Property(form => form.WordEntryId)
            .IsRequired();

        builder.Property(form => form.PartOfSpeech)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(form => form.Article)
            .HasMaxLength(32);

        builder.Property(form => form.PluralForm)
            .HasMaxLength(256);

        builder.Property(form => form.InfinitiveForm)
            .HasMaxLength(256);

        builder.Property(form => form.SortOrder)
            .IsRequired();

        builder.Property(form => form.IsPrimary)
            .IsRequired();

        builder.Property(form => form.CreatedAtUtc)
            .IsRequired();

        builder.Property(form => form.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(form => new { form.WordEntryId, form.PartOfSpeech })
            .IsUnique();

        builder.HasIndex(form => new { form.WordEntryId, form.SortOrder })
            .IsUnique();

        builder.HasIndex(form => form.WordEntryId)
            .HasDatabaseName("IX_WordLexicalForms_PrimaryPerWordEntry")
            .IsUnique()
            .HasFilter("\"IsPrimary\"");
    }
}

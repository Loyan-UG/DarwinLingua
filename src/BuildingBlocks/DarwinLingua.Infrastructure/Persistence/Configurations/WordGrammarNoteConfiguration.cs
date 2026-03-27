using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence details for learner-facing grammar notes.
/// </summary>
internal sealed class WordGrammarNoteConfiguration : IEntityTypeConfiguration<WordGrammarNote>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<WordGrammarNote> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("WordGrammarNotes");

        builder.HasKey(note => note.Id);

        builder.Property(note => note.Id)
            .ValueGeneratedNever();

        builder.Property(note => note.WordEntryId)
            .IsRequired();

        builder.Property(note => note.Text)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(note => note.SortOrder)
            .IsRequired();

        builder.Property(note => note.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(note => new { note.WordEntryId, note.Text })
            .IsUnique();

        builder.HasIndex(note => new { note.WordEntryId, note.SortOrder })
            .IsUnique();
    }
}

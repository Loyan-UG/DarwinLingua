using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence details for learner-facing word-family members.
/// </summary>
internal sealed class WordFamilyMemberConfiguration : IEntityTypeConfiguration<WordFamilyMember>
{
    public void Configure(EntityTypeBuilder<WordFamilyMember> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("WordFamilyMembers");

        builder.HasKey(member => member.Id);

        builder.Property(member => member.Id)
            .ValueGeneratedNever();

        builder.Property(member => member.WordEntryId)
            .IsRequired();

        builder.Property(member => member.Lemma)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(member => member.RelationLabel)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(member => member.Note)
            .HasMaxLength(256);

        builder.Property(member => member.SortOrder)
            .IsRequired();

        builder.Property(member => member.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(member => new { member.WordEntryId, member.Lemma, member.RelationLabel })
            .IsUnique();

        builder.HasIndex(member => new { member.WordEntryId, member.SortOrder })
            .IsUnique();
    }
}

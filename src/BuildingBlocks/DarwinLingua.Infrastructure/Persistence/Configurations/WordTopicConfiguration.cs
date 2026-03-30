using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence details for the <see cref="WordTopic"/> entity.
/// </summary>
internal sealed class WordTopicConfiguration : IEntityTypeConfiguration<WordTopic>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<WordTopic> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("WordTopics");

        builder.HasKey(topic => topic.Id);

        builder.Property(topic => topic.Id)
            .ValueGeneratedNever();

        builder.Property(topic => topic.WordEntryId)
            .IsRequired();

        builder.Property(topic => topic.TopicId)
            .IsRequired();

        builder.Property(topic => topic.IsPrimaryTopic)
            .IsRequired();

        builder.Property(topic => topic.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(topic => new { topic.WordEntryId, topic.TopicId })
            .IsUnique();

        builder.HasIndex(topic => topic.WordEntryId)
            .HasDatabaseName("IX_WordTopics_PrimaryPerWordEntry")
            .IsUnique()
            .HasFilter($"\"{nameof(WordTopic.IsPrimaryTopic)}\"");

        builder.HasOne<Topic>()
            .WithMany()
            .HasForeignKey(topic => topic.TopicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

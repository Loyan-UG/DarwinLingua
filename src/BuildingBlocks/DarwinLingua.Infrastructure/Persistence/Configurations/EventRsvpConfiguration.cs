using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class EventRsvpConfiguration : IEntityTypeConfiguration<EventRsvp>
{
    public void Configure(EntityTypeBuilder<EventRsvp> builder)
    {
        builder.ToTable("EventRsvps");
        builder.HasKey(rsvp => rsvp.Id);
        builder.Property(rsvp => rsvp.Id).ValueGeneratedNever();
        builder.Property(rsvp => rsvp.ConversationEventSlug).HasMaxLength(128).IsRequired();
        builder.Property(rsvp => rsvp.ParticipantName).HasMaxLength(256).IsRequired();
        builder.Property(rsvp => rsvp.ParticipantEmail).HasMaxLength(320).IsRequired();
        builder.Property(rsvp => rsvp.Status).HasMaxLength(64).IsRequired();
        builder.Property(rsvp => rsvp.CreatedAtUtc).IsRequired();
        builder.Property(rsvp => rsvp.UpdatedAtUtc).IsRequired();
        builder.HasIndex(rsvp => new { rsvp.ConversationEventSlug, rsvp.ParticipantEmail }).IsUnique();
        builder.HasIndex(rsvp => new { rsvp.ConversationEventSlug, rsvp.Status });
    }
}

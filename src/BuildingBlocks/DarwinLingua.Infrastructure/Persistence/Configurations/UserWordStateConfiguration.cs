using DarwinLingua.Learning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence details for the <see cref="UserWordState"/> entity.
/// </summary>
internal sealed class UserWordStateConfiguration : IEntityTypeConfiguration<UserWordState>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserWordState> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("UserWordStates");

        builder.HasKey(userWordState => userWordState.Id);

        builder.Property(userWordState => userWordState.Id)
            .ValueGeneratedNever();

        builder.Property(userWordState => userWordState.UserId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(userWordState => userWordState.WordEntryPublicId)
            .IsRequired();

        builder.Property(userWordState => userWordState.IsKnown)
            .IsRequired();

        builder.Property(userWordState => userWordState.IsDifficult)
            .IsRequired();

        builder.Property(userWordState => userWordState.ViewCount)
            .IsRequired();

        builder.Property(userWordState => userWordState.CreatedAtUtc)
            .IsRequired();

        builder.Property(userWordState => userWordState.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(userWordState => new
            {
                userWordState.UserId,
                userWordState.WordEntryPublicId,
            })
            .IsUnique();
    }
}

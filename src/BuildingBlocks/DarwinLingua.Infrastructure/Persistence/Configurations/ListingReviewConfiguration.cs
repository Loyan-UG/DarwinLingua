using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class ListingReviewConfiguration : IEntityTypeConfiguration<ListingReview>
{
    public void Configure(EntityTypeBuilder<ListingReview> builder)
    {
        builder.ToTable("ListingReviews");
        builder.HasKey(review => review.Id);
        builder.Property(review => review.Id).ValueGeneratedNever();
        builder.Property(review => review.ListingType).HasMaxLength(64).IsRequired();
        builder.Property(review => review.ListingKey).HasMaxLength(256).IsRequired();
        builder.Property(review => review.Status).HasMaxLength(64).IsRequired();
        builder.Property(review => review.CreatedAtUtc).IsRequired();
        builder.Property(review => review.UpdatedAtUtc).IsRequired();
        builder.HasIndex(review => new { review.ListingType, review.ListingKey, review.Status });
    }
}

using DarwinLingua.WebApi.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Persistence;

/// <summary>
/// Represents the PostgreSQL-backed shared-content database used by the Web API.
/// </summary>
public sealed class ServerContentDbContext(DbContextOptions<ServerContentDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the client-product set.
    /// </summary>
    public DbSet<ClientProductEntity> ClientProducts => Set<ClientProductEntity>();

    /// <summary>
    /// Gets the content-stream set.
    /// </summary>
    public DbSet<ContentStreamEntity> ContentStreams => Set<ContentStreamEntity>();

    /// <summary>
    /// Gets the published-package set.
    /// </summary>
    public DbSet<PublishedPackageEntity> PublishedPackages => Set<PublishedPackageEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<ClientProductEntity>(entity =>
        {
            entity.ToTable("ClientProducts");
            entity.HasKey(product => product.Id);
            entity.Property(product => product.Key).HasMaxLength(128).IsRequired();
            entity.Property(product => product.DisplayName).HasMaxLength(256).IsRequired();
            entity.Property(product => product.LearningLanguageCode).HasMaxLength(16).IsRequired();
            entity.Property(product => product.DefaultUiLanguageCode).HasMaxLength(16).IsRequired();
            entity.HasIndex(product => product.Key).IsUnique();
        });

        modelBuilder.Entity<ContentStreamEntity>(entity =>
        {
            entity.ToTable("ContentStreams");
            entity.HasKey(stream => stream.Id);
            entity.Property(stream => stream.ContentAreaKey).HasMaxLength(128).IsRequired();
            entity.Property(stream => stream.SliceKey).HasMaxLength(128).IsRequired();
            entity.Property(stream => stream.LearningLanguageCode).HasMaxLength(16).IsRequired();
            entity.HasOne(stream => stream.ClientProduct)
                .WithMany(product => product.ContentStreams)
                .HasForeignKey(stream => stream.ClientProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(stream => new { stream.ClientProductId, stream.ContentAreaKey, stream.SliceKey }).IsUnique();
        });

        modelBuilder.Entity<PublishedPackageEntity>(entity =>
        {
            entity.ToTable("PublishedPackages");
            entity.HasKey(package => package.Id);
            entity.Property(package => package.PackageId).HasMaxLength(256).IsRequired();
            entity.Property(package => package.PackageType).HasMaxLength(128).IsRequired();
            entity.Property(package => package.Version).HasMaxLength(64).IsRequired();
            entity.Property(package => package.Checksum).HasMaxLength(256).IsRequired();
            entity.Property(package => package.RelativeDownloadPath).HasMaxLength(512).IsRequired();
            entity.HasOne(package => package.ContentStream)
                .WithMany(stream => stream.PublishedPackages)
                .HasForeignKey(package => package.ContentStreamId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(package => package.PackageId).IsUnique();
        });
    }
}

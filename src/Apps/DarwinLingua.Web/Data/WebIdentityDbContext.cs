using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Data;

public sealed class WebIdentityDbContext(DbContextOptions<WebIdentityDbContext> options)
    : IdentityDbContext<WebApplicationUser, IdentityRole, string>(options)
{
    public DbSet<WebUserPreference> UserPreferences => Set<WebUserPreference>();

    public DbSet<WebUserFavoriteWord> UserFavoriteWords => Set<WebUserFavoriteWord>();

    public DbSet<WebUserWordState> UserWordStates => Set<WebUserWordState>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        base.OnModelCreating(builder);

        builder.Entity<WebUserPreference>(entity =>
        {
            entity.ToTable("WebUserPreferences");
            entity.HasKey(preference => preference.Id);
            entity.Property(preference => preference.ActorId).HasMaxLength(128).IsRequired();
            entity.Property(preference => preference.UiLanguageCode).HasMaxLength(16).IsRequired();
            entity.Property(preference => preference.PrimaryMeaningLanguageCode).HasMaxLength(16).IsRequired();
            entity.Property(preference => preference.SecondaryMeaningLanguageCode).HasMaxLength(16);
            entity.HasIndex(preference => preference.ActorId).IsUnique();
        });

        builder.Entity<WebUserFavoriteWord>(entity =>
        {
            entity.ToTable("WebUserFavoriteWords");
            entity.HasKey(favoriteWord => favoriteWord.Id);
            entity.Property(favoriteWord => favoriteWord.ActorId).HasMaxLength(128).IsRequired();
            entity.HasIndex(favoriteWord => new { favoriteWord.ActorId, favoriteWord.WordPublicId }).IsUnique();
        });

        builder.Entity<WebUserWordState>(entity =>
        {
            entity.ToTable("WebUserWordStates");
            entity.HasKey(wordState => wordState.Id);
            entity.Property(wordState => wordState.ActorId).HasMaxLength(128).IsRequired();
            entity.HasIndex(wordState => new { wordState.ActorId, wordState.WordPublicId }).IsUnique();
            entity.HasIndex(wordState => new { wordState.ActorId, wordState.LastViewedAtUtc });
        });
    }
}

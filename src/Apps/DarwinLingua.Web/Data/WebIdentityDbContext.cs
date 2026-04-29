using DarwinLingua.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Data;

public sealed class WebIdentityDbContext(DbContextOptions<WebIdentityDbContext> options)
    : DarwinLinguaIdentityDbContext(options)
{
    public DbSet<WebUserPreference> UserPreferences => Set<WebUserPreference>();

    public DbSet<WebUserFavoriteWord> UserFavoriteWords => Set<WebUserFavoriteWord>();

    public DbSet<WebUserWordState> UserWordStates => Set<WebUserWordState>();

    public DbSet<WebEmailDeliveryLog> EmailDeliveryLogs => Set<WebEmailDeliveryLog>();

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

        builder.Entity<WebEmailDeliveryLog>(entity =>
        {
            entity.ToTable("WebEmailDeliveryLogs");
            entity.HasKey(log => log.Id);
            entity.Property(log => log.ScenarioKey).HasMaxLength(128).IsRequired();
            entity.Property(log => log.RecipientEmailHash).HasMaxLength(128).IsRequired();
            entity.Property(log => log.RecipientUserId).HasMaxLength(450);
            entity.Property(log => log.TemplateKey).HasMaxLength(128).IsRequired();
            entity.Property(log => log.Culture).HasMaxLength(16).IsRequired();
            entity.Property(log => log.Subject).HasMaxLength(256).IsRequired();
            entity.Property(log => log.ProviderName).HasMaxLength(64).IsRequired();
            entity.Property(log => log.ProviderMessageId).HasMaxLength(256);
            entity.Property(log => log.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(log => log.FailureCode).HasMaxLength(128);
            entity.Property(log => log.FailureMessageSummary).HasMaxLength(512);
            entity.Property(log => log.CorrelationId).HasMaxLength(128);
            entity.HasIndex(log => new { log.CreatedAtUtc, log.Status });
            entity.HasIndex(log => new { log.ScenarioKey, log.CreatedAtUtc });
        });
    }
}

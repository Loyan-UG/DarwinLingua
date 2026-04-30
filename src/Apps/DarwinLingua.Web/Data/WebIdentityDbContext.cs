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

    public DbSet<WebEmailSuppression> EmailSuppressions => Set<WebEmailSuppression>();

    public DbSet<WebBillingProfile> WebBillingProfiles => Set<WebBillingProfile>();

    public DbSet<WebBillingEvent> WebBillingEvents => Set<WebBillingEvent>();

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
            entity.Property(log => log.ProviderLastEvent).HasMaxLength(64);
            entity.Property(log => log.ProviderLastEventReason).HasMaxLength(512);
            entity.Property(log => log.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(log => log.FailureCode).HasMaxLength(128);
            entity.Property(log => log.FailureMessageSummary).HasMaxLength(512);
            entity.Property(log => log.CorrelationId).HasMaxLength(128);
            entity.HasIndex(log => new { log.CreatedAtUtc, log.Status });
            entity.HasIndex(log => new { log.Status, log.CreatedAtUtc });
            entity.HasIndex(log => new { log.ScenarioKey, log.CreatedAtUtc });
            entity.HasIndex(log => log.ProviderMessageId);
            entity.HasIndex(log => new { log.ProviderLastEvent, log.ProviderLastEventAtUtc })
                .HasDatabaseName("IX_WebEmailLogs_Event_EventAtUtc");
            entity.HasIndex(log => log.ProviderLastEventAtUtc)
                .HasDatabaseName("IX_WebEmailLogs_EventAtUtc");
        });

        builder.Entity<WebEmailSuppression>(entity =>
        {
            entity.ToTable("WebEmailSuppressions");
            entity.HasKey(suppression => suppression.Id);
            entity.Property(suppression => suppression.RecipientEmailHash).HasMaxLength(128).IsRequired();
            entity.Property(suppression => suppression.Reason).HasMaxLength(128).IsRequired();
            entity.Property(suppression => suppression.ProviderName).HasMaxLength(64).IsRequired();
            entity.Property(suppression => suppression.ProviderMessageId).HasMaxLength(256);
            entity.HasIndex(suppression => suppression.RecipientEmailHash).IsUnique();
            entity.HasIndex(suppression => suppression.CreatedAtUtc);
        });

        builder.Entity<WebBillingProfile>(entity =>
        {
            entity.ToTable("WebBillingProfiles");
            entity.HasKey(profile => profile.UserId);
            entity.Property(profile => profile.UserId).HasMaxLength(450).IsRequired();
            entity.Property(profile => profile.ProviderName).HasMaxLength(64).IsRequired();
            entity.Property(profile => profile.ProviderCustomerId).HasMaxLength(128);
            entity.Property(profile => profile.ProviderSubscriptionId).HasMaxLength(128);
            entity.Property(profile => profile.PlanKey).HasMaxLength(128).IsRequired();
            entity.Property(profile => profile.Status).HasMaxLength(64).IsRequired();
            entity.HasIndex(profile => new { profile.ProviderName, profile.ProviderCustomerId })
                .HasDatabaseName("IX_WebBillingProfiles_Provider_Customer");
            entity.HasIndex(profile => new { profile.ProviderName, profile.ProviderSubscriptionId })
                .HasDatabaseName("IX_WebBillingProfiles_Provider_Subscription");
        });

        builder.Entity<WebBillingEvent>(entity =>
        {
            entity.ToTable("WebBillingEvents");
            entity.HasKey(billingEvent => billingEvent.Id);
            entity.Property(billingEvent => billingEvent.ProviderName).HasMaxLength(64).IsRequired();
            entity.Property(billingEvent => billingEvent.ProviderEventId).HasMaxLength(128).IsRequired();
            entity.Property(billingEvent => billingEvent.EventType).HasMaxLength(128).IsRequired();
            entity.Property(billingEvent => billingEvent.Status).HasMaxLength(64).IsRequired();
            entity.Property(billingEvent => billingEvent.UserId).HasMaxLength(450);
            entity.Property(billingEvent => billingEvent.ProviderCustomerId).HasMaxLength(128);
            entity.Property(billingEvent => billingEvent.ProviderSubscriptionId).HasMaxLength(128);
            entity.Property(billingEvent => billingEvent.ErrorSummary).HasMaxLength(512);
            entity.HasIndex(billingEvent => new { billingEvent.ProviderName, billingEvent.ProviderEventId })
                .IsUnique()
                .HasDatabaseName("IX_WebBillingEvents_Provider_EventId");
            entity.HasIndex(billingEvent => new { billingEvent.Status, billingEvent.CreatedAtUtc })
                .HasDatabaseName("IX_WebBillingEvents_Status_CreatedAtUtc");
        });
    }
}

using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.ContentOps.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence.Configurations;
using DarwinLingua.Learning.Domain.Entities;
using DarwinLingua.Localization.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Infrastructure.Persistence;

/// <summary>
/// Represents the shared Phase 1 EF Core database context used by the current local-first hosts.
/// </summary>
public sealed class DarwinLinguaDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DarwinLinguaDbContext"/> class.
    /// </summary>
    /// <param name="options">The configured EF Core options.</param>
    public DarwinLinguaDbContext(DbContextOptions<DarwinLinguaDbContext> options)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(options);
    }

    /// <summary>
    /// Gets the language reference data set.
    /// </summary>
    public DbSet<Language> Languages => Set<Language>();

    /// <summary>
    /// Gets the topic reference data set.
    /// </summary>
    public DbSet<Topic> Topics => Set<Topic>();

    /// <summary>
    /// Gets the lexical entry data set.
    /// </summary>
    public DbSet<WordEntry> WordEntries => Set<WordEntry>();

    /// <summary>
    /// Gets the word-sense data set.
    /// </summary>
    public DbSet<WordSense> WordSenses => Set<WordSense>();

    /// <summary>
    /// Gets the sense-translation data set.
    /// </summary>
    public DbSet<SenseTranslation> SenseTranslations => Set<SenseTranslation>();

    /// <summary>
    /// Gets the example-sentence data set.
    /// </summary>
    public DbSet<ExampleSentence> ExampleSentences => Set<ExampleSentence>();

    /// <summary>
    /// Gets the example-translation data set.
    /// </summary>
    public DbSet<ExampleTranslation> ExampleTranslations => Set<ExampleTranslation>();

    /// <summary>
    /// Gets the topic localization reference data set.
    /// </summary>
    public DbSet<TopicLocalization> TopicLocalizations => Set<TopicLocalization>();

    /// <summary>
    /// Gets the word-topic link data set.
    /// </summary>
    public DbSet<WordTopic> WordTopics => Set<WordTopic>();

    /// <summary>
    /// Gets the local user learning profiles.
    /// </summary>
    public DbSet<UserLearningProfile> UserLearningProfiles => Set<UserLearningProfile>();

    /// <summary>
    /// Gets the imported content-package audit rows.
    /// </summary>
    public DbSet<ContentPackage> ContentPackages => Set<ContentPackage>();

    /// <summary>
    /// Gets the content-package entry audit rows.
    /// </summary>
    public DbSet<ContentPackageEntry> ContentPackageEntries => Set<ContentPackageEntry>();

    /// <summary>
    /// Gets the local user favorite words.
    /// </summary>
    public DbSet<UserFavoriteWord> UserFavoriteWords => Set<UserFavoriteWord>();

    /// <summary>
    /// Gets the local user lightweight word states.
    /// </summary>
    public DbSet<UserWordState> UserWordStates => Set<UserWordState>();

    /// <summary>
    /// Applies all explicit entity configurations.
    /// </summary>
    /// <param name="modelBuilder">The EF Core model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfiguration(new LanguageConfiguration());
        modelBuilder.ApplyConfiguration(new TopicConfiguration());
        modelBuilder.ApplyConfiguration(new TopicLocalizationConfiguration());
        modelBuilder.ApplyConfiguration(new WordEntryConfiguration());
        modelBuilder.ApplyConfiguration(new WordSenseConfiguration());
        modelBuilder.ApplyConfiguration(new SenseTranslationConfiguration());
        modelBuilder.ApplyConfiguration(new ExampleSentenceConfiguration());
        modelBuilder.ApplyConfiguration(new ExampleTranslationConfiguration());
        modelBuilder.ApplyConfiguration(new WordTopicConfiguration());
        modelBuilder.ApplyConfiguration(new ContentPackageConfiguration());
        modelBuilder.ApplyConfiguration(new ContentPackageEntryConfiguration());
        modelBuilder.ApplyConfiguration(new UserLearningProfileConfiguration());
        modelBuilder.ApplyConfiguration(new UserFavoriteWordConfiguration());
        modelBuilder.ApplyConfiguration(new UserWordStateConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}

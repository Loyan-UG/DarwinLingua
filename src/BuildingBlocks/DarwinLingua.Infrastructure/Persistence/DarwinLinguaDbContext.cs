using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.ContentOps.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence.Configurations;
using DarwinLingua.Learning.Domain.Entities;
using DarwinLingua.Localization.Domain.Entities;
using DarwinLingua.Practice.Domain.Entities;
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
    /// Gets the lexical-form data set.
    /// </summary>
    public DbSet<WordLexicalForm> WordLexicalForms => Set<WordLexicalForm>();

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
    /// Gets the lexical word-label data set.
    /// </summary>
    public DbSet<WordLabel> WordLabels => Set<WordLabel>();

    public DbSet<LabelDefinition> LabelDefinitions => Set<LabelDefinition>();

    public DbSet<LabelDefinitionLocalization> LabelDefinitionLocalizations => Set<LabelDefinitionLocalization>();

    /// <summary>
    /// Gets the lexical grammar-note data set.
    /// </summary>
    public DbSet<WordGrammarNote> WordGrammarNotes => Set<WordGrammarNote>();

    /// <summary>
    /// Gets the lexical collocation data set.
    /// </summary>
    public DbSet<WordCollocation> WordCollocations => Set<WordCollocation>();

    /// <summary>
    /// Gets the lexical word-family data set.
    /// </summary>
    public DbSet<WordFamilyMember> WordFamilyMembers => Set<WordFamilyMember>();

    /// <summary>
    /// Gets the lexical relation data set.
    /// </summary>
    public DbSet<WordRelation> WordRelations => Set<WordRelation>();

    /// <summary>
    /// Gets curated word collections such as study lists and book playlists.
    /// </summary>
    public DbSet<WordCollection> WordCollections => Set<WordCollection>();

    public DbSet<WordCollectionLocalization> WordCollectionLocalizations => Set<WordCollectionLocalization>();

    /// <summary>
    /// Gets the words linked into curated collections.
    /// </summary>
    public DbSet<WordCollectionEntry> WordCollectionEntries => Set<WordCollectionEntry>();

    public DbSet<ScenarioLesson> ScenarioLessons => Set<ScenarioLesson>();

    public DbSet<ScenarioLessonTopic> ScenarioLessonTopics => Set<ScenarioLessonTopic>();

    public DbSet<ScenarioDialogueTurn> ScenarioDialogueTurns => Set<ScenarioDialogueTurn>();

    public DbSet<ScenarioPhrase> ScenarioPhrases => Set<ScenarioPhrase>();

    public DbSet<ScenarioQuestion> ScenarioQuestions => Set<ScenarioQuestion>();

    public DbSet<ScenarioAnswer> ScenarioAnswers => Set<ScenarioAnswer>();

    public DbSet<ConversationStarterPack> ConversationStarterPacks => Set<ConversationStarterPack>();

    public DbSet<ConversationStarterPackTopic> ConversationStarterPackTopics => Set<ConversationStarterPackTopic>();

    public DbSet<ConversationStarterPhrase> ConversationStarterPhrases => Set<ConversationStarterPhrase>();

    public DbSet<EventPreparationPack> EventPreparationPacks => Set<EventPreparationPack>();

    public DbSet<EventPreparationPackTopic> EventPreparationPackTopics => Set<EventPreparationPackTopic>();

    public DbSet<EventPreparationPrompt> EventPreparationPrompts => Set<EventPreparationPrompt>();

    public DbSet<ConversationEvent> ConversationEvents => Set<ConversationEvent>();

    public DbSet<ConversationEventLevel> ConversationEventLevels => Set<ConversationEventLevel>();

    public DbSet<ConversationEventHelperLanguage> ConversationEventHelperLanguages => Set<ConversationEventHelperLanguage>();

    public DbSet<ConversationEventPreparationPackLink> ConversationEventPreparationPackLinks => Set<ConversationEventPreparationPackLink>();

    public DbSet<OrganizerProfile> OrganizerProfiles => Set<OrganizerProfile>();

    public DbSet<OrganizerProfileSupportedLevel> OrganizerProfileSupportedLevels => Set<OrganizerProfileSupportedLevel>();

    public DbSet<OrganizerProfileHelperLanguage> OrganizerProfileHelperLanguages => Set<OrganizerProfileHelperLanguage>();

    public DbSet<OrganizerClaimRequest> OrganizerClaimRequests => Set<OrganizerClaimRequest>();

    public DbSet<OrganizerProfileOwner> OrganizerProfileOwners => Set<OrganizerProfileOwner>();

    public DbSet<EventRsvp> EventRsvps => Set<EventRsvp>();

    public DbSet<LearnerConversationProfile> LearnerConversationProfiles => Set<LearnerConversationProfile>();

    public DbSet<PartnerRequest> PartnerRequests => Set<PartnerRequest>();

    public DbSet<UserReport> UserReports => Set<UserReport>();

    public DbSet<UserBlock> UserBlocks => Set<UserBlock>();

    public DbSet<ModerationDecisionAudit> ModerationDecisionAudits => Set<ModerationDecisionAudit>();

    public DbSet<OrganizerVerification> OrganizerVerifications => Set<OrganizerVerification>();

    public DbSet<ListingReview> ListingReviews => Set<ListingReview>();

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
    /// Gets the persisted learner review-state rows.
    /// </summary>
    public DbSet<PracticeReviewState> PracticeReviewStates => Set<PracticeReviewState>();

    /// <summary>
    /// Gets the persisted learner attempt-history rows.
    /// </summary>
    public DbSet<PracticeAttempt> PracticeAttempts => Set<PracticeAttempt>();

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
        modelBuilder.ApplyConfiguration(new WordLexicalFormConfiguration());
        modelBuilder.ApplyConfiguration(new WordCollocationConfiguration());
        modelBuilder.ApplyConfiguration(new WordFamilyMemberConfiguration());
        modelBuilder.ApplyConfiguration(new WordRelationConfiguration());
        modelBuilder.ApplyConfiguration(new WordCollectionConfiguration());
        modelBuilder.ApplyConfiguration(new WordCollectionLocalizationConfiguration());
        modelBuilder.ApplyConfiguration(new WordCollectionEntryConfiguration());
        modelBuilder.ApplyConfiguration(new ScenarioLessonConfiguration());
        modelBuilder.ApplyConfiguration(new ScenarioLessonTopicConfiguration());
        modelBuilder.ApplyConfiguration(new ScenarioDialogueTurnConfiguration());
        modelBuilder.ApplyConfiguration(new ScenarioDialogueTurnTranslationConfiguration());
        modelBuilder.ApplyConfiguration(new ScenarioPhraseConfiguration());
        modelBuilder.ApplyConfiguration(new ScenarioPhraseTranslationConfiguration());
        modelBuilder.ApplyConfiguration(new ScenarioQuestionConfiguration());
        modelBuilder.ApplyConfiguration(new ScenarioQuestionTranslationConfiguration());
        modelBuilder.ApplyConfiguration(new ScenarioAnswerConfiguration());
        modelBuilder.ApplyConfiguration(new ScenarioAnswerTranslationConfiguration());
        modelBuilder.ApplyConfiguration(new ConversationStarterPackConfiguration());
        modelBuilder.ApplyConfiguration(new ConversationStarterPackTopicConfiguration());
        modelBuilder.ApplyConfiguration(new ConversationStarterLinkedScenarioConfiguration());
        modelBuilder.ApplyConfiguration(new ConversationStarterLinkedEventPreparationPackConfiguration());
        modelBuilder.ApplyConfiguration(new ConversationStarterPhraseConfiguration());
        modelBuilder.ApplyConfiguration(new ConversationStarterPhraseTranslationConfiguration());
        modelBuilder.ApplyConfiguration(new ConversationStarterPhraseAlternativeConfiguration());
        modelBuilder.ApplyConfiguration(new EventPreparationPackConfiguration());
        modelBuilder.ApplyConfiguration(new EventPreparationPackTopicConfiguration());
        modelBuilder.ApplyConfiguration(new EventPreparationLinkedScenarioConfiguration());
        modelBuilder.ApplyConfiguration(new EventPreparationLinkedConversationStarterPackConfiguration());
        modelBuilder.ApplyConfiguration(new EventPreparationVocabularyReferenceConfiguration());
        modelBuilder.ApplyConfiguration(new EventPreparationPromptConfiguration());
        modelBuilder.ApplyConfiguration(new ConversationEventConfiguration());
        modelBuilder.ApplyConfiguration(new ConversationEventLevelConfiguration());
        modelBuilder.ApplyConfiguration(new ConversationEventHelperLanguageConfiguration());
        modelBuilder.ApplyConfiguration(new ConversationEventPreparationPackLinkConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizerProfileConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizerProfileSupportedLevelConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizerProfileHelperLanguageConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizerClaimRequestConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizerProfileOwnerConfiguration());
        modelBuilder.ApplyConfiguration(new EventRsvpConfiguration());
        modelBuilder.ApplyConfiguration(new LearnerConversationProfileConfiguration());
        modelBuilder.ApplyConfiguration(new PartnerRequestConfiguration());
        modelBuilder.ApplyConfiguration(new UserReportConfiguration());
        modelBuilder.ApplyConfiguration(new UserBlockConfiguration());
        modelBuilder.ApplyConfiguration(new ModerationDecisionAuditConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizerVerificationConfiguration());
        modelBuilder.ApplyConfiguration(new ListingReviewConfiguration());
        modelBuilder.ApplyConfiguration(new WordGrammarNoteConfiguration());
        modelBuilder.ApplyConfiguration(new WordLabelConfiguration());
        modelBuilder.ApplyConfiguration(new LabelDefinitionConfiguration());
        modelBuilder.ApplyConfiguration(new LabelDefinitionLocalizationConfiguration());
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
        modelBuilder.ApplyConfiguration(new PracticeReviewStateConfiguration());
        modelBuilder.ApplyConfiguration(new PracticeAttemptConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}

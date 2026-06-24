# Domain Model

## Purpose

This document defines the domain model for the Darwin Deutsch platform.

The goal is to design the domain early and thoroughly enough so that:

- the vocabulary learning core is stable
- future modules can be added without architectural damage
- data import remains predictable
- web, mobile, admin, and API layers can share the same core model
- future practice, progress, and migrant-support modules can be added cleanly

This document intentionally focuses on the domain and data model, not UI design.

---

# 1. Domain Design Principles

## 1.1 Main Principles

The domain should follow these principles:

- clear separation of bounded contexts
- stable aggregate roots
- controlled vocabulary for metadata
- localization-aware design
- support for future extensibility
- support for AI-assisted content generation and import
- support for optional offline-first usage
- avoid overloading a single entity with unrelated concerns
- preserve a clean path toward one shared backend that can serve multiple learner apps later

## 1.2 Domain Shape

The product domain is not a single flat vocabulary table.

It is composed of several conceptual areas:

- Content Catalog
- Learning Content Structure
- Localization
- User Learning Data
- Practice and Review
- Resource Discovery
- Dialogue Learning
- Talk Topics
- Events and Organizers
- Profiles, Matching, and Moderation
- Import and Content Operations

The first production phase will mostly use the Content Catalog and part of the User Learning Data.

---

# 2. High-Level Bounded Contexts

The domain should be split into the following bounded contexts.

## 2.1 Content Context

Responsible for:

- words
- meanings
- examples
- levels
- topics
- tags
- lexical metadata
- audio metadata
- content packages

This is the primary context for phase 1.

## 2.2 Learning Context

Responsible for:

- user favorites
- user study history
- known/unknown markers
- lightweight progress
- learning preferences

Used partly in phase 1 and more in later phases.

## 2.3 Practice Context

Responsible for:

- flashcards
- quizzes
- review queues
- spaced repetition
- mistake tracking
- practice sessions

Mainly phase 2.

## 2.4 Dialogue Learning

Dialogue lessons are role-based practical conversations used for real-life speaking tasks and exam preparation. A Dialogue is not a Talk Topic: Talk Topics are article-led discussion materials, while Dialogues model turn-by-turn interaction.

DialogueLesson is a catalog aggregate with first-class `cefrLevel`, `category`, topic links, exam profiles, skill focus tags, task type, interaction mode, register, speaking functions, useful word references, estimated practice minutes, optional difficulty notes, optional exam relevance, dialogue turns, useful phrases, quick-check questions, and speaking prompts.

Useful words store only references such as lemma, optional word slug/key, optional CEFR level, and sort order. Meanings and translations are resolved from the Word Catalog where possible. Dialogue quality is validated by minimum meaningful sentence counts per side: A1 5, A2 6, B1 7, B2 8, C1 9, and C2 10.

## 2.4 Resource Discovery Context

Responsible for:

- useful places
- service providers
- support centers
- resource categories
- city/location links
- migrant-support information

Mainly phase 4.

## 2.5 Dialogue Learning Context

Responsible for:

- practical dialogue practice lessons
- conversation starter packs
- scripted roleplay preparation
- event preparation packs that reference learning content
- CEFR- and situation-aware practical conversation material

Mainly phase 6.

## 2.5.1 Talk Topics

Talk Topics are reading-based conversation materials for learner groups. A Talk Topic contains a slug, topic group key, title, description, CEFR level, category, topic links, controlled content type, German article body, optional article translations, warm-up questions, discussion questions, vocabulary references, speaking goals, sensitivity metadata, estimated reading/discussion minutes, sort order, and publication state.

Vocabulary in Talk Topics stores references only. Word meanings and examples remain owned by the Word Catalog.

## 2.6 Events and Organizers Context

Responsible for:

- local and online conversation events
- organizer profiles
- organizer verification state
- RSVP and attendance basics
- organizer-facing management workflows

Mainly phase 6.

## 2.7 Profiles and Matching Context

Responsible for:

- minimal learner profiles
- profile visibility state
- request-based conversation partner matching
- consent-based contact reveal

Mainly phase 6. This context must not include unrestricted open chat in the first social MVP.

## 2.8 Moderation Context

Responsible for:

- user reports
- user blocks
- organizer and listing review
- moderation queues
- moderation decisions and audit trail

Mainly phase 6, and required before public profiles or matching can ship.

## 2.9 Content Operations Context

Responsible for:

- import packages
- import jobs
- duplicate detection results
- validation reports
- content source tracking

This may exist as a domain-supporting context or application-supporting context depending on implementation strategy.

---

# 3. Domain Language

## 3.1 Core Terms

### Word
A German lexical entry shown to the learner.

### Sense
A distinct meaning or semantic usage of a word.

### Meaning Translation
A translation of a sense into another language.

### Example Sentence
A German sentence showing usage of a word or sense.

### Example Translation
A translation of the example sentence into another language.

### Topic
A controlled thematic grouping such as travel or doctor.

### CEFR Level
A level such as A1, A2, B1, B2, C1, or C2.

### Usage Label
A label such as formal, informal, spoken, written, business, or daily-life.

### Context Label
A real-life usage context such as doctor, office, transport, or housing.

### Content Package
A batch of content prepared for import.

### Resource
A helpful place, organization, or support entry for users.

---

# 4. Enumerations and Controlled Values

The domain should avoid uncontrolled text wherever stability matters.

---

## 4.1 CEFR Level

Allowed values:

- A1
- A2
- B1
- B2
- C1
- C2

---

## 4.2 Part of Speech

Recommended initial values:

- Noun
- Verb
- Adjective
- Adverb
- Pronoun
- Preposition
- Conjunction
- Interjection
- Numeral
- Phrase
- Expression
- Other

---

## 4.3 Usage Register

Recommended values:

- Formal
- Informal
- Spoken
- Written
- Everyday
- Business
- Academic
- Official
- Colloquial
- Regional
- ChildFriendly
- Polite
- Impolite
- Sensitive

Not all are needed in phase 1, but the model should support them.

---

## 4.4 Audio Kind

Recommended values:

- WordPronunciation
- ExamplePronunciation
- SlowPronunciation
- HumanRecorded
- Synthetic

---

## 4.5 Import Source Type

Recommended values:

- Manual
- AIAssisted
- Hybrid
- ExternalCurated

---

## 4.6 Publication Status

Recommended values:

- Draft
- Active
- Archived
- Deprecated

Useful for future content workflow even if phase 1 keeps everything active.

---

# 5. Core Content Domain

This is the most important section.

The content model must be rich enough for future growth but not chaotic.

---

## 5.1 Aggregate Root: WordEntry

Represents a core German lexical entry.

### Responsibilities

- identify the base German word
- hold stable lexical metadata
- hold one or more senses
- hold topic associations
- hold CEFR metadata
- hold usage metadata
- hold audio metadata
- support content lifecycle and import traceability

### Recommended Fields

- Id
- PublicId
- Lemma
- NormalizedLemma
- SearchLemma
- LanguageCode
- PrimaryCefrLevel
- PartOfSpeech
- Article
- PluralForm
- InfinitiveForm
- ComparativeForm
- SuperlativeForm
- SeparablePrefix
- IsSeparableVerb
- IsReflexiveVerb
- IsIrregularVerb
- Gender
- PronunciationIpa
- SyllableBreak
- DifficultyScore
- FrequencyRank
- IsCommon
- PublicationStatus
- ContentSourceType
- SourceReference
- NotesInternal
- CreatedAtUtc
- UpdatedAtUtc
- PublishedAtUtc
- CreatedBy
- LastModifiedBy
- RowVersion

### Notes

Not every field is required for every part of speech.

Examples:

- Article is mostly relevant for nouns
- Comparative/Superlative are relevant for adjectives/adverbs
- InfinitiveForm is relevant for verbs
- PluralForm is mostly relevant for nouns
- Separable metadata is relevant for verbs

### Important Design Note

Do not try to force all lexical logic into one flat table forever.
The aggregate root may store common fields directly, while specialized child metadata may later be introduced for nouns, verbs, and adjectives if needed.

---

## 5.2 Entity: WordSense

A word may have one or more senses.

This is critical and should not be skipped.

Example:
A single German word may have multiple meanings depending on context.

### Responsibilities

- represent a distinct meaning of a word
- attach translations
- attach examples
- attach usage labels
- optionally attach grammar hints
- optionally attach context labels

### Recommended Fields

- Id
- WordEntryId
- SenseOrder
- ShortDefinitionDe
- FullDefinitionDe
- ShortGloss
- UsageNote
- GrammarNote
- IsPrimarySense
- PrimaryCefrLevel
- PublicationStatus
- CreatedAtUtc
- UpdatedAtUtc
- RowVersion

### Notes

`ShortDefinitionDe` and `FullDefinitionDe` are optional in early versions but valuable later.

---

## 5.3 Entity: SenseTranslation

Represents a translation of a specific sense into a target language.

### Responsibilities

- provide multilingual meaning text for a sense
- support one or multiple meaning languages
- allow a user to view one or two selected languages simultaneously

### Recommended Fields

- Id
- WordSenseId
- LanguageCode
- TranslationText
- Transliteration
- TranslationNote
- IsPrimary
- PublicationStatus
- CreatedAtUtc
- UpdatedAtUtc
- RowVersion

### Constraints

- unique by WordSenseId + LanguageCode
- multiple records allowed for multiple target languages

---

## 5.4 Entity: ExampleSentence

Represents a German example sentence associated with a word or sense.

### Responsibilities

- show real usage
- support learning context
- support audio playback
- support translations

### Recommended Fields

- Id
- WordSenseId
- SentenceOrder
- GermanText
- NormalizedGermanText
- CefrLevel
- IsPrimaryExample
- UsageRegister
- ContextHint
- GrammarHint
- SourceReference
- PublicationStatus
- CreatedAtUtc
- UpdatedAtUtc
- RowVersion

### Notes

Examples should be attached to a sense, not only to the word entry.
This matters when a word has multiple meanings.

---

## 5.5 Entity: ExampleTranslation

Represents a translation of a German example sentence into another language.

### Recommended Fields

- Id
- ExampleSentenceId
- LanguageCode
- TranslationText
- Transliteration
- TranslationNote
- CreatedAtUtc
- UpdatedAtUtc
- RowVersion

### Constraints

- unique by ExampleSentenceId + LanguageCode

---

## 5.6 Entity: Topic

Represents a stable topic key used across the system.

### Responsibilities

- classify vocabulary by practical themes
- support browsing and filtering
- support future localization of topic display values

### Recommended Fields

- Id
- Key
- ParentTopicId
- SortOrder
- IconKey
- ColorKey
- IsSystem
- PublicationStatus
- CreatedAtUtc
- UpdatedAtUtc
- RowVersion

### Notes

Examples of `Key`:

- travel
- work
- doctor
- school
- shopping
- housing
- government-office

### Design Rule

Topic keys should be stable and language-neutral.
Localized display names should not be stored directly as free text in UI code.

---

## 5.7 Entity: TopicLocalization

Stores localized display text for topics.

### Recommended Fields

- Id
- TopicId
- LanguageCode
- DisplayName
- Description
- CreatedAtUtc
- UpdatedAtUtc
- RowVersion

### Constraint

- unique by TopicId + LanguageCode

---

## 5.8 Entity: WordTopic

Many-to-many link between words and topics.

### Recommended Fields

- Id
- WordEntryId
- TopicId
- RelevanceScore
- IsPrimaryTopic
- CreatedAtUtc

### Notes

A word may belong to multiple topics.
Example:
"Bahnhof" may belong to both travel and transport.

---

## 5.9 Entity: UsageLabel

Controlled label such as formal, informal, spoken, business.

### Recommended Fields

- Id
- Key
- SortOrder
- PublicationStatus
- CreatedAtUtc
- UpdatedAtUtc

---

## 5.10 Entity: UsageLabelLocalization

### Recommended Fields

- Id
- UsageLabelId
- LanguageCode
- DisplayName
- Description

---

## 5.11 Entity: WordSenseUsageLabel

Many-to-many link between a sense and usage labels.

### Recommended Fields

- Id
- WordSenseId
- UsageLabelId
- CreatedAtUtc

---

## 5.12 Entity: ContextLabel

Represents practical usage context.

Examples:

- doctor
- office
- school
- interview
- rental
- supermarket

### Recommended Fields

- Id
- Key
- SortOrder
- PublicationStatus
- CreatedAtUtc
- UpdatedAtUtc

---

## 5.13 Entity: ContextLabelLocalization

### Recommended Fields

- Id
- ContextLabelId
- LanguageCode
- DisplayName
- Description

---

## 5.14 Entity: WordSenseContextLabel

Many-to-many link between a sense and one or more context labels.

### Recommended Fields

- Id
- WordSenseId
- ContextLabelId
- CreatedAtUtc

---

## 5.15 Entity: AudioAsset

Represents audio metadata for word pronunciation or example pronunciation.

### Responsibilities

- support TTS-based or stored audio
- keep audio independent from UI layer
- allow future audio replacement

### Recommended Fields

- Id
- OwnerType
- OwnerId
- AudioKind
- LanguageCode
- AccentCode
- Provider
- StorageType
- RelativePath
- ExternalUrl
- DurationMs
- MimeType
- Checksum
- IsGenerated
- IsHumanRecorded
- PublicationStatus
- CreatedAtUtc
- UpdatedAtUtc
- RowVersion

### Notes

`OwnerType` may initially be:
- WordEntry
- ExampleSentence

`StorageType` may initially be:
- PlatformTts
- LocalFile
- RemoteFile
- Blob

If phase 1 uses only native platform TTS, this entity may be deferred or simplified.
But the domain should anticipate it.

---

## 5.16 Entity: Language

Represents supported meaning or UI languages.

### Recommended Fields

- Id
- Code
- NameEnglish
- NativeName
- IsUiSupported
- IsMeaningSupported
- IsActive
- SortOrder
- CreatedAtUtc
- UpdatedAtUtc

### Examples

- de
- en
- fa
- ar
- tr
- ru
- ku

---

# 6. Lexical Metadata Extensions

These entities are not strictly required for phase 1, but they should be anticipated.

---

## 6.1 Entity: WordRelation

Represents lexical relations between entries.

### Use Cases

- synonym
- antonym
- related word
- word family
- derived form

### Recommended Fields

- Id
- SourceWordEntryId
- TargetWordEntryId
- RelationType
- Note
- SortOrder
- CreatedAtUtc
- UpdatedAtUtc

### Recommended Relation Types

- Synonym
- Antonym
- Related
- Derived
- WordFamily
- Opposite
- SimilarUsage

---

## 6.2 Entity: Collocation

Represents a useful phrase or word combination.

### Recommended Fields

- Id
- WordEntryId
- GermanText
- MeaningHint
- CefrLevel
- PublicationStatus
- CreatedAtUtc
- UpdatedAtUtc

---

## 6.3 Entity: CollocationTranslation

### Recommended Fields

- Id
- CollocationId
- LanguageCode
- TranslationText
- CreatedAtUtc
- UpdatedAtUtc

---

## 6.4 Entity: GrammarHint

Short structured grammar support.

### Recommended Fields

- Id
- WordSenseId
- Title
- HintText
- LanguageCode
- SortOrder
- CreatedAtUtc
- UpdatedAtUtc

### Note

This is useful when you later want bilingual grammar hints without turning the product into a full grammar engine.

---

# 7. Content Package and Import Domain

Because your content will be generated gradually and often with AI assistance, import is part of the real product ecosystem.

---

## 7.1 Aggregate Root: ContentPackage

Represents a logical batch of imported content.

### Recommended Fields

- Id
- PackageId
- PackageVersion
- PackageName
- SourceType
- SourceReference
- InputFileName
- Checksum
- ImportedAtUtc
- ImportedBy
- TotalEntries
- InsertedEntries
- SkippedEntries
- InvalidEntries
- WarningCount
- Status
- Notes
- CreatedAtUtc
- UpdatedAtUtc
- RowVersion

---

## 7.2 Entity: ContentPackageEntry

Tracks an imported or attempted word record from a package.

### Recommended Fields

- Id
- ContentPackageId
- ExternalEntryKey
- RawLemma
- NormalizedLemma
- CefrLevel
- PartOfSpeech
- ProcessingStatus
- ErrorCode
- ErrorMessage
- WarningMessage
- ImportedWordEntryId
- CreatedAtUtc
- UpdatedAtUtc

---

## 7.3 Entity: ImportJob

Represents a technical import execution.

### Recommended Fields

- Id
- ContentPackageId
- StartedAtUtc
- FinishedAtUtc
- Status
- ProcessedCount
- SuccessCount
- WarningCount
- ErrorCount
- ExecutedBy
- SummaryJson
- RowVersion

### Note

Depending on implementation, ContentPackage and ImportJob may be collapsed in v1.
But conceptually they are different:
- ContentPackage = logical content batch
- ImportJob = technical execution

---

# 8. Learning Domain

This context supports user-facing learning behavior.

---

## 8.1 Aggregate Root: UserLearningProfile

Represents user-specific learning settings and lightweight profile data.

### Recommended Fields

- Id
- UserId
- NativeLanguageCode
- PreferredMeaningLanguage1
- PreferredMeaningLanguage2
- UiLanguageCode
- PreferredAudioSpeed
- PreferredVoiceType
- DailyGoal
- CurrentCefrFocus
- CreatedAtUtc
- UpdatedAtUtc
- RowVersion

---

## 8.2 Entity: UserFavoriteWord

### Recommended Fields

- Id
- UserId
- WordEntryId
- CreatedAtUtc

### Constraint

- unique by UserId + WordEntryId

---

## 8.3 Entity: UserWordState

Tracks user-specific familiarity or state for a word.

### Recommended Fields

- Id
- UserId
- WordEntryId
- IsKnown
- IsDifficult
- IsHidden
- LastViewedAtUtc
- FirstViewedAtUtc
- ViewCount
- LastPracticedAtUtc
- ConfidenceScore
- ManualNote
- CreatedAtUtc
- UpdatedAtUtc
- RowVersion

### Notes

This gives flexibility without forcing a full spaced repetition engine in phase 1.

---

## 8.4 Entity: UserRecentWord

### Recommended Fields

- Id
- UserId
- WordEntryId
- ViewedAtUtc

### Note

This entity may later be replaced with an event stream or trimmed history strategy.

---

## 8.5 Entity: LearningList

Represents a personal saved list.

### Examples

- Interview words
- Doctor visit words
- My difficult words

### Recommended Fields

- Id
- UserId
- Name
- Description
- IsSystemGenerated
- CreatedAtUtc
- UpdatedAtUtc
- RowVersion

---

## 8.6 Entity: LearningListItem

### Recommended Fields

- Id
- LearningListId
- WordEntryId
- SortOrder
- AddedAtUtc

---

# 9. Practice Domain

This context becomes important in phase 2.

---

## 9.1 Aggregate Root: ReviewQueueItem

Represents a user-specific scheduled review item.

### Recommended Fields

- Id
- UserId
- WordEntryId
- SourceType
- NextReviewAtUtc
- LastReviewedAtUtc
- ReviewCount
- SuccessCount
- FailureCount
- EaseFactor
- IntervalDays
- Status
- CreatedAtUtc
- UpdatedAtUtc
- RowVersion

### Note

This supports future spaced repetition.

---

## 9.2 Entity: PracticeSession

### Recommended Fields

- Id
- UserId
- SessionType
- StartedAtUtc
- FinishedAtUtc
- TotalItems
- CorrectItems
- WrongItems
- DurationSeconds
- CreatedAtUtc
- UpdatedAtUtc

---

## 9.3 Entity: PracticeSessionItem

### Recommended Fields

- Id
- PracticeSessionId
- WordEntryId
- PromptType
- UserAnswer
- CorrectAnswer
- IsCorrect
- ResponseTimeMs
- CreatedAtUtc

---

## 9.4 Entity: UserMistakePattern

Tracks repeated difficulties.

### Recommended Fields

- Id
- UserId
- WordEntryId
- MistakeType
- Count
- LastOccurredAtUtc
- CreatedAtUtc
- UpdatedAtUtc

### Examples of MistakeType

- WrongMeaning
- WrongArticle
- WrongPlural
- WrongSpelling
- WrongContext
- WrongUsage

---

# 10. Resource Discovery Domain

This is the long-term migrant-support module.

---

## 10.1 Aggregate Root: SupportResource

Represents a helpful real-world resource.

Examples:

- language school
- speaking café
- counseling center
- support office
- volunteer organization

### Recommended Fields

- Id
- PublicId
- Name
- Slug
- ShortDescription
- FullDescription
- ResourceType
- WebsiteUrl
- Email
- Phone
- IsFree
- IsAppointmentRequired
- LanguageSupportNotes
- OpeningHoursText
- PublicationStatus
- SourceReference
- VerifiedAtUtc
- CreatedAtUtc
- UpdatedAtUtc
- RowVersion

---

## 10.2 Entity: SupportResourceCategory

### Recommended Fields

- Id
- Key
- SortOrder
- PublicationStatus
- CreatedAtUtc
- UpdatedAtUtc

### Examples

- language-school
- conversation-cafe
- counseling-center
- legal-help
- family-support
- job-support
- health-support

---

## 10.3 Entity: SupportResourceCategoryLocalization

### Recommended Fields

- Id
- SupportResourceCategoryId
- LanguageCode
- DisplayName
- Description

---

## 10.4 Entity: SupportResourceCategoryLink

### Recommended Fields

- Id
- SupportResourceId
- SupportResourceCategoryId
- IsPrimary
- CreatedAtUtc

---

## 10.5 Entity: Address

General reusable address entity.

### Recommended Fields

- Id
- CountryCode
- Region
- City
- PostalCode
- Street
- HouseNumber
- AdditionalLine
- Latitude
- Longitude
- CreatedAtUtc
- UpdatedAtUtc

### Note

Could be shared by multiple future modules.

---

## 10.6 Entity: SupportResourceAddress

### Recommended Fields

- Id
- SupportResourceId
- AddressId
- CreatedAtUtc

---

## 10.7 Entity: SupportResourceTag

### Recommended Fields

- Id
- SupportResourceId
- TagKey
- CreatedAtUtc

Examples:

- wheelchair-access
- family-friendly
- multilingual-staff
- no-registration-needed

---

# 11. Localization Domain Support

Localization should not be hacked into the UI only.

---

## 11.1 UI Localization

UI resource files may live outside the domain, but the domain should still recognize supported languages.

For domain-controlled localizable data, translation tables should exist where needed.

## 11.2 Localizable Domain Data

The following should support localization through domain entities or translation tables:

- Topic
- UsageLabel
- ContextLabel
- SupportResourceCategory
- future descriptive content

---

# 11A. Phase 6 Conversation, Event, and Social Domain

These concepts are planned for Phase 6. They are documented here to preserve domain clarity before implementation.

## 11A.1 Aggregate Root: DialogueLesson

Represents a practical lesson for a real German situation.

### Recommended Fields

- Id
- PublicId
- Slug
- Title
- ShortDescription
- SituationKey
- PrimaryCefrLevel
- MinCefrLevel
- MaxCefrLevel
- PrimaryTopicId
- PublicationStatus
- CreatedAtUtc
- UpdatedAtUtc
- RowVersion

### Child Concepts

- ScenarioStep
- DialoguePhrase
- ScenarioVocabularyLink
- ScenarioLocalization

Dialogue practice lessons may reference content catalog words, but they must not own word meanings.

## 11A.2 Aggregate Root: EventListing

Represents a conversation event or related language-practice event.

### Recommended Fields

- Id
- PublicId
- OrganizerId
- Title
- Slug
- Description
- EventMode
- StartsAtUtc
- EndsAtUtc
- TimeZoneId
- City
- CountryCode
- AddressId
- MeetingUrl
- MinCefrLevel
- MaxCefrLevel
- PriceType
- Capacity
- PublicationStatus
- VerificationStatus
- CreatedAtUtc
- UpdatedAtUtc
- RowVersion

## 11A.3 Aggregate Root: OrganizerProfile

Represents a public organizer identity for conversation cafes, clubs, teachers, libraries, associations, integration groups, or company language programs.

### Recommended Fields

- Id
- PublicId
- DisplayName
- Slug
- OrganizerType
- Description
- WebsiteUrl
- ContactEmail
- VerificationStatus
- PlanKey
- CreatedAtUtc
- UpdatedAtUtc
- RowVersion

## 11A.4 Aggregate Root: LearnerProfile

Represents the minimal public or semi-public learner profile used for safe conversation practice discovery.

### Recommended Fields

- Id
- UserId
- DisplayName
- City
- CurrentCefrLevel
- TargetPracticeMode
- Visibility
- ContactRevealPolicy
- CreatedAtUtc
- UpdatedAtUtc
- RowVersion

Profiles must default to private or limited visibility until explicit user consent is captured.

## 11A.5 Aggregate Root: PartnerRequest

Represents a request-based conversation partner flow.

### Recommended Fields

- Id
- PublicId
- RequesterUserId
- RecipientUserId
- RequestState
- MessageTemplateKey
- CreatedAtUtc
- RespondedAtUtc
- ExpiresAtUtc
- RowVersion

Request states should include pending, accepted, declined, cancelled, expired, and blocked.

## 11A.6 Moderation Concepts

### UserReport

Captures reports against profiles, events, organizers, partner requests, or other public surfaces.

### UserBlock

Prevents further matching or profile interaction between two users.

### OrganizerReview

Tracks verification and review decisions for organizer profiles.

### ListingReview

Tracks review decisions for public event listings.

### ModerationDecision

Stores admin action, reason, actor, target, and timestamp for auditability.

Moderation concepts should be available before public profiles, matching, or organizer self-service listings are released.

---

# 11B. Phase 7 Learning Portal Domain Concepts

Phase 7 expands Darwin Lingua into a complete Web-first learning portal. The detailed roadmap lives in `76-Learning-Portal-Roadmap-And-Backlog.md`; this section records only the domain concepts and boundaries.

## 11B.1 Content Concepts

- `GrammarTopic` represents implemented structured grammar reference content with sections, examples, common mistakes, rule summaries, exception notes, prerequisites, related grammar topics, linked words, linked dialogues, linked Talk Topics, and linked exercise slugs.
- `ExpressionEntry` represents implemented idioms, proverbs, colloquial phrases, fixed expressions, cultural phrases, warning phrases, examples, warnings, linked words, related expressions, and linked exercise slugs. Expressions are separate from Words because their actual meaning can differ from the literal meaning of individual words.
- `Exercise` represents an implemented reusable deterministic practice item with an exercise type, answer key, feedback model, CEFR level, target skill, and optional linked content owner.
- `ExerciseSet` groups implemented exercises for a grammar topic, expression, dialogue, Talk Topic, course lesson, exam unit, or other learning surface.
- `CoursePath`, `CourseModule`, and `CourseLesson` represent implemented structured learning paths. Course lessons orchestrate references to grammar, words, expressions, dialogues, Talk Topics, exercise sets, and future exam-prep units instead of duplicating that content.
- `ExamProfile` and `ExamPrepUnit` represent implemented exam taxonomies and original preparation units linked to dialogues, exercises, grammar topics, expressions, writing templates, Talk Topics, and course lessons.
- `WritingTemplate` represents implemented practical German message/email and exam-writing templates with variables, filled examples, register, linked grammar, linked words, linked expressions, and linked exercises.
- `CountryGuidanceNote` represents implemented German communication and culture guidance with body sections, practical examples, optional do/don't notes, sensitivity warnings, and links to dialogues, expressions, writing templates, Talk Topics, and course lessons.
- `UnifiedLearningSearchResult` is an implemented projection model for deterministic search results across words, grammar, expressions, dialogues, Talk Topics, exercises, lessons, exam prep, writing templates, country guidance notes, events, and organizers.

## 11B.2 User State Concept

- `UserContentProgress` is implemented as the cross-content Learning Portal progress record. It stores only user id, content owner type, owner slug, progress state, view timestamps, completion timestamp, and counters.
- `UserExerciseAttempt` tracks submitted deterministic exercise attempts separately from shared exercise definitions.

User progress must remain separate from content entities. Content records are shared and importable; progress records are per user.
Supported `UserContentProgress` owner types are `word`, `grammar-topic`, `expression`, `dialogue`, `talk-topic`, `exercise`, `exercise-set`, `course`, `course-module`, `course-lesson`, `exam-prep-unit`, `writing-template`, and `country-guidance`.
Supported progress states are `not-started`, `viewed`, `in-progress`, `completed`, `needs-review`, and `skipped`.
Basic personalization is deterministic: it can recommend next incomplete course lessons and grammar topics, but it must not introduce AI ranking until explicit product rules exist.

## 11B.3 Phase 7 Domain Rules

- Educational content must be content-driven and importable.
- Course lessons should reference grammar, words, expressions, dialogues, Talk Topics, and exercises instead of duplicating them.
- Exercises are reusable and may attach to different content owner types.
- Words remain the source of lexical meanings; expressions may link to words but do not own word meanings.
- Web is the first implementation surface; mobile parity comes later after Web/API contracts, validation, and rendering are stable.
- Unified learning search is a projection over owned content records. It does not own content and does not replace WordEntry search semantics.
- Grammar Guide is the first implemented Phase 7 content module and uses the contract in `77-Grammar-Content-Package-Contract.md`.
- Everyday Expressions is the second implemented Phase 7 content module and uses the contract in `78-Expression-Content-Package-Contract.md`.

---

# 12. Identity and User Boundary

The domain should not tightly couple itself to a specific identity provider.

Use a stable `UserId` reference in learning and practice entities.

### Recommended Rule

The learning domain references users by external identity ID only, without embedding authentication logic into domain entities.

---

# 13. Auditing and Concurrency

Because content will evolve and may be edited/imported multiple times, the domain should consistently support:

- CreatedAtUtc
- UpdatedAtUtc
- CreatedBy where useful
- LastModifiedBy where useful
- RowVersion where useful

Not every table needs every field, but aggregate roots and important mutable entities should support concurrency.

---

# 14. Soft Delete vs Archive

For this product, soft delete should be used carefully.

### Recommended Rule

For learning content, prefer:

- PublicationStatus
- Active/Archived/Deprecated

instead of physical or logical delete in most cases.

Why:
- imported content may be referenced later
- user history may point to content
- auditability matters

For purely technical import logs, normal deletion policies may be acceptable.

---

# 15. Search Model Considerations

Search will be important from phase 1.

The content domain should support fields such as:

- NormalizedLemma
- SearchLemma
- NormalizedGermanText for examples
- topic keys

### Suggested Search Targets

- German word
- meanings in selected languages
- topics
- possibly example texts in later phases

This does not require a separate search engine in phase 1, but the model should support later expansion.

---

# 16. Recommended Aggregate Summary

## Content Context Aggregates

- WordEntry
- Topic
- UsageLabel
- ContextLabel
- Language
- ContentPackage

## Learning Context Aggregates

- UserLearningProfile
- LearningList

## Practice Context Aggregates

- ReviewQueueItem
- PracticeSession

## Resource Context Aggregates

- SupportResource
- SupportResourceCategory

---

# 17. Minimum Viable Domain for Phase 1

Even though the full model is broader, the actual first implementation can start with this smaller set:

- WordEntry
- WordSense
- SenseTranslation
- ExampleSentence
- ExampleTranslation
- Topic
- TopicLocalization
- WordTopic
- Language
- UserLearningProfile
- UserFavoriteWord
- UserWordState
- ContentPackage
- ContentPackageEntry

This is the minimum serious baseline.

---

# 18. Expanded Domain Candidate Set

If you want to design broadly from the start, these are the most likely long-term entities:

- WordEntry
- WordSense
- SenseTranslation
- ExampleSentence
- ExampleTranslation
- Topic
- TopicLocalization
- WordTopic
- UsageLabel
- UsageLabelLocalization
- WordSenseUsageLabel
- ContextLabel
- ContextLabelLocalization
- WordSenseContextLabel
- AudioAsset
- Language
- WordRelation
- Collocation
- CollocationTranslation
- GrammarHint
- ContentPackage
- ContentPackageEntry
- ImportJob
- UserLearningProfile
- UserFavoriteWord
- UserWordState
- UserRecentWord
- LearningList
- LearningListItem
- ReviewQueueItem
- PracticeSession
- PracticeSessionItem
- UserMistakePattern
- SupportResource
- SupportResourceCategory
- SupportResourceCategoryLocalization
- SupportResourceCategoryLink
- Address
- SupportResourceAddress
- SupportResourceTag
- DialogueLesson
- ScenarioStep
- DialoguePhrase
- ScenarioVocabularyLink
- GrammarTopic
- ExpressionEntry
- Exercise
- ExerciseSet
- CoursePath
- CourseModule
- CourseLesson
- ExamProfile
- ExamPrepUnit
- WritingTemplate
- CountryGuidanceNote
- UnifiedLearningSearchResult
- UserContentProgress
- EventListing
- EventPreparationPack
- OrganizerProfile
- LearnerProfile
- PartnerRequest
- UserReport
- UserBlock
- OrganizerReview
- ListingReview
- ModerationDecision

---

# 19. Design Warnings

## Warning 1

Do not collapse all meanings into a single text field on WordEntry.
That will break future extensibility.

## Warning 2

Do not store topic display names as random free text everywhere.
Use stable topic keys and localizations.

## Warning 3

Do not bind content structure directly to MAUI view models.
The domain must remain UI-independent.

## Warning 4

Do not design import around only the first 100 words.
Assume long-term growth from the beginning.

## Warning 5

Do not mix user progress data into the core content entities.
Keep user-specific state separate.

---

# 20. Recommended Next Steps

After this document, the next architectural documents should define:

- bounded contexts and boundaries
- aggregate rules
- entity relationships
- project structure
- database strategy
- import pipeline design
- API contract direction
- offline sync strategy

For the future shared backend, also see:

- `36-Server-Content-Distribution.md`
- `37-Shared-Content-Server-Domain.md`


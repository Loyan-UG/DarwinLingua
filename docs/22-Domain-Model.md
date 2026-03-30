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

## 2.4 Resource Discovery Context

Responsible for:

- useful places
- service providers
- support centers
- resource categories
- city/location links
- migrant-support information

Mainly phase 4.

## 2.5 Content Operations Context

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

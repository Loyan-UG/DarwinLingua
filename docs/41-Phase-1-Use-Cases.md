# Phase 1 Use Cases

## Purpose

This document defines the essential Phase 1 use cases of Darwin Deutsch.

The goal is to make implementation scope explicit and actionable.

This document should help with:

- application feature planning
- command/query design
- UI screen planning
- API thinking for later
- implementation sequencing
- avoiding accidental scope creep

This document builds on:

- `25-Phase-1-Domain-Cut.md`
- `26-Bounded-Contexts.md`
- `31-Solution-Architecture.md`
- `34-Import-Workflow.md`

---

# 1. Phase 1 Product Goal Reminder

Phase 1 is a vocabulary-first German learning application with:

- CEFR browsing
- topic browsing
- multilingual meanings
- example sentences
- one or two selected meaning languages
- favorites
- lightweight user word state
- local-first behavior
- structured import support

Phase 1 is not a full learning ecosystem.

---

# 2. Use Case Groups

The essential Phase 1 use cases are grouped into these areas:

- Content Discovery
- Word Details
- User Preferences
- Favorites
- User Word State
- Content Import
- App Initialization / Reference Data

---

# 3. Content Discovery Use Cases

# 3.1 Browse Words by CEFR Level

## Goal

Allow the user to browse words filtered by CEFR level.

## Actor

- Learner

## Preconditions

- database exists
- content is present
- CEFR values are valid

## Main Flow

1. user opens CEFR browsing screen
2. user selects a CEFR level
3. system queries matching words
4. system returns word list
5. user sees results

## Expected Output

Each item should show at least:

- word lemma
- part of speech
- optional article/plural where relevant
- topic hints if useful

## Notes

This is a read-heavy query use case.

---

# 3.2 Browse Words by Topic

## Goal

Allow the user to browse words linked to a selected topic.

## Actor

- Learner

## Preconditions

- topics exist
- topic localizations exist for current UI language where possible

## Main Flow

1. user opens topic browsing screen
2. system loads topic list
3. user selects a topic
4. system queries words linked to that topic
5. system returns word list
6. user sees results

## Notes

Topic browsing is a core navigation method and must work offline.

---

# 3.3 Search Words by German Lemma

## Goal

Allow the user to search words using German text.

## Actor

- Learner

## Preconditions

- content exists
- normalized search support exists

## Main Flow

1. user enters search text
2. system normalizes query
3. system searches matching words
4. system returns results
5. user selects a result if desired

## Notes

Phase 1 search should remain simple:

- German lemma search
- prefix-first ranking
- contains fallback
- no advanced semantic search
- deterministic ordering on normalized lemma

---

# 3.4 Get Topic List

## Goal

Load the available topic list for browsing/filtering.

## Actor

- Learner
- App UI

## Main Flow

1. system determines current UI language
2. system loads topics
3. system selects localized display names when available
4. system returns ordered topic list

## Notes

If a localization is missing, a safe fallback strategy should be defined later, but topic retrieval itself is required in Phase 1.

---

# 4. Word Detail Use Cases

# 4.1 Get Word Details

## Goal

Display the full details of a selected word.

## Actor

- Learner

## Preconditions

- selected word exists
- at least one meaning language is configured or a fallback exists

## Main Flow

1. user selects a word
2. system loads the word aggregate
3. system loads senses
4. system loads translations for selected meaning languages
5. system loads example sentences
6. system loads example translations for selected meaning languages
7. system loads topic information
8. system returns detail model
9. UI renders the word detail screen

## Output Should Include

- word
- part of speech
- CEFR level
- article/plural if relevant
- meanings in selected language 1
- meanings in selected language 2 if configured
- example sentences
- example translations in selected languages
- topics
- favorite state if available
- lightweight user state if available

---

# 4.2 Play Word Pronunciation

## Goal

Allow the user to hear pronunciation of the selected German word.

## Actor

- Learner

## Preconditions

- device/platform TTS capability is available

## Main Flow

1. user taps pronunciation action on a word
2. UI calls platform audio/TTS service
3. service attempts pronunciation playback
4. UI reflects success or graceful failure

## Notes

This is a UI/platform use case.
The content domain does not own audio playback.
If TTS support or a compatible German voice is unavailable, the UI should show a localized non-blocking message and continue normally.

---

# 4.3 Play Example Sentence Pronunciation

## Goal

Allow the user to hear pronunciation of a German example sentence.

## Actor

- Learner

## Preconditions

- example exists
- platform TTS capability is available

## Main Flow

1. user taps pronunciation action on example sentence
2. UI calls platform audio/TTS service
3. service attempts playback
4. UI reflects success or graceful failure

## Notes

If TTS support or a compatible German voice is unavailable, the UI should show a localized non-blocking message and continue normally.

---

# 5. User Preference Use Cases

# 5.1 Get User Learning Preferences

## Goal

Load current user learning and language preferences.

## Actor

- Learner
- App UI

## Main Flow

1. system resolves current user profile
2. system loads user learning profile
3. system returns current preferences

## Returned Data

Should include at minimum:

- preferred meaning language 1
- preferred meaning language 2 if any
- UI language code
- optional native language code
- optional CEFR focus

---

# 5.2 Update Meaning Language Preferences

## Goal

Allow the user to select one or two target meaning languages.

## Actor

- Learner

## Preconditions

- chosen languages exist
- chosen languages are active for meaning support

## Main Flow

1. user opens preferences/settings
2. user selects meaning language 1
3. user optionally selects meaning language 2
4. system validates language support
5. system saves the updated profile
6. future word detail queries use the new preference

## Rules

- language 1 is required
- language 2 is optional
- the same language should not be stored twice

---

# 5.3 Update UI Language Preference

## Goal

Allow the user to set the preferred app UI language when supported.

## Actor

- Learner

## Main Flow

1. user selects a UI language
2. system validates support
3. profile is updated
4. app uses or schedules the updated UI language behavior

## Notes

Some UI frameworks may require restart or refresh behavior.
That is an implementation concern, not a domain concern.

---

# 6. Favorites Use Cases

# 6.1 Get Favorite Words

## Goal

Show the user’s saved favorite words.

## Actor

- Learner

## Main Flow

1. user opens favorites screen
2. system loads favorite word links for the user
3. system joins word summary data
4. system returns ordered favorite list

---

# 6.2 Add Word to Favorites

## Goal

Allow a user to save a word as favorite.

## Actor

- Learner

## Preconditions

- word exists
- user exists in local profile scope

## Main Flow

1. user taps favorite action
2. system checks whether favorite already exists
3. if not, system creates favorite record
4. system returns updated state

## Rules

- duplicate favorites are not allowed

---

# 6.3 Remove Word from Favorites

## Goal

Allow a user to remove a saved favorite.

## Actor

- Learner

## Main Flow

1. user taps favorite action again or remove action
2. system finds favorite record
3. system removes it
4. system returns updated state

---

# 6.4 Toggle Favorite

## Goal

Provide a single simple command path for favorite state changes.

## Actor

- Learner
- UI

## Notes

This may be implemented instead of separate add/remove commands if preferred.

That is acceptable for Phase 1 and often simpler for UI integration.

---

# 7. User Word State Use Cases

# 7.1 Track Word Viewed

## Goal

Record that the user has viewed a word.

## Actor

- App UI
- Learner indirectly

## Main Flow

1. user opens a word detail screen
2. system checks for existing user-word state
3. if none exists, system creates one
4. system updates:
   - FirstViewedAtUtc if first time
   - LastViewedAtUtc
   - ViewCount
5. changes are saved

## Reason

This supports lightweight personalization and future analytics without introducing full practice logic.

---

# 7.2 Mark Word as Known

## Goal

Allow the user to mark a word as known.

## Actor

- Learner

## Main Flow

1. user selects mark-as-known action
2. system creates or updates user word state
3. `IsKnown` is set
4. updated state is saved

## Notes

This is a lightweight Phase 1 marker, not a full mastery model.

---

# 7.3 Mark Word as Difficult

## Goal

Allow the user to mark a word as difficult.

## Actor

- Learner

## Main Flow

1. user selects mark-as-difficult action
2. system creates or updates user word state
3. `IsDifficult` is set
4. updated state is saved

---

# 7.4 Unmark Known / Difficult

## Goal

Allow the user to clear a lightweight word marker.

## Actor

- Learner

## Notes

This may be implemented through dedicated commands or one general update-state command.

Either is acceptable as long as the UI flow remains simple.

---

# 8. Content Import Use Cases

# 8.1 Import Content Package

## Goal

Import a JSON package into the local content database.

## Actor

- Operator

## Preconditions

- import file exists
- database exists
- reference data exists
- file format is supported

## Main Flow

1. operator selects or provides a file
2. system reads file
3. system parses JSON
4. system validates package
5. system validates entries
6. system checks reference values
7. system normalizes data
8. system detects duplicates
9. system writes valid content
10. system writes package and entry tracking
11. system returns summary

## Outputs

- package id
- completion status
- inserted count
- duplicate count
- invalid count
- warnings/errors summary

---

# 8.2 Validate Content Package

## Goal

Run validation logic on a content package before or during import.

## Actor

- Operator
- Import workflow

## Notes

In Phase 1 this may be part of the main import command rather than a separate standalone public use case.

That is acceptable.

---

# 8.3 Review Import Summary

## Goal

Allow the operator to inspect the outcome of an import.

## Actor

- Operator

## Output Should Include

- totals
- duplicates skipped
- invalid entries
- warnings
- fatal errors if any

---

# 9. App Initialization / Reference Use Cases

# 9.1 Initialize Database

## Goal

Prepare the local database for first use.

## Actor

- App startup
- Import tool startup

## Main Flow

1. system resolves database location
2. system applies migrations
3. system ensures schema exists
4. system returns ready state

## Notes

This is an infrastructure/host startup use case, but it is essential.

---

# 9.2 Seed Reference Data

## Goal

Ensure required reference data exists.

## Actor

- App initialization
- setup routine
- operator during setup if needed

## Phase 1 Seed Scope

- Languages
- Topics
- Topic localizations

## Notes

Vocabulary content itself should not be seeded this way in bulk.

---

# 9.3 Ensure User Learning Profile Exists

## Goal

Make sure a usable local user profile exists for the app installation.

## Actor

- App startup
- settings flow
- content access flow

## Main Flow

1. system tries to load user profile
2. if missing, system creates a default local profile
3. profile is saved
4. app continues normally

## Notes

This keeps the Phase 1 experience simple and local-first.

---

# 10. Essential Query Use Cases for Application Layer

The following read use cases are essential in Phase 1:

- GetTopics
- GetWordsByCefr
- GetWordsByTopic
- SearchWords
- GetWordDetails
- GetFavoriteWords
- GetUserLearningProfile
- GetSupportedMeaningLanguages
- GetSupportedUiLanguages

---

# 11. Essential Command Use Cases for Application Layer

The following write use cases are essential in Phase 1:

- UpdateMeaningLanguagePreferences
- UpdateUiLanguagePreference
- ToggleFavorite
- TrackWordViewed
- MarkWordKnown
- MarkWordDifficult
- ClearWordKnownState
- ClearWordDifficultState
- ImportContentPackage
- EnsureUserLearningProfileExists

---

# 12. Recommended Use Case Priorities

## 12.1 Must-Have for First Working Build

These are the minimum essential use cases:

- InitializeDatabase
- SeedReferenceData
- EnsureUserLearningProfileExists
- GetTopics
- GetWordsByCefr
- GetWordsByTopic
- SearchWords
- GetWordDetails
- UpdateMeaningLanguagePreferences
- ToggleFavorite
- TrackWordViewed
- ImportContentPackage

## 12.2 Strongly Recommended for First Real Usable Build

Add these immediately after the minimum:

- GetFavoriteWords
- MarkWordKnown
- MarkWordDifficult
- GetSupportedMeaningLanguages
- GetSupportedUiLanguages

## 12.3 Can Be Slightly Later Within Phase 1

- UpdateUiLanguagePreference
- ClearWordKnownState
- ClearWordDifficultState
- richer settings queries

---

# 13. Suggested Screen-to-Use-Case Mapping

## 13.1 Home / Browse Screen

Likely uses:

- GetWordsByCefr
- GetWordsByTopic
- SearchWords

## 13.2 Topic Screen

Likely uses:

- GetTopics
- GetWordsByTopic

## 13.3 Word Detail Screen

Likely uses:

- GetWordDetails
- TrackWordViewed
- ToggleFavorite
- MarkWordKnown
- MarkWordDifficult
- PlayWordPronunciation
- PlayExampleSentencePronunciation

## 13.4 Favorites Screen

Likely uses:

- GetFavoriteWords
- ToggleFavorite

## 13.5 Settings Screen

Likely uses:

- GetUserLearningProfile
- GetSupportedMeaningLanguages
- GetSupportedUiLanguages
- UpdateMeaningLanguagePreferences
- UpdateUiLanguagePreference

## 13.6 Import Tool Screen / Console Flow

Likely uses:

- ImportContentPackage
- ReviewImportSummary

---

# 14. Use Case Design Rules

## 14.1 Keep Use Cases Narrow

Each use case should do one clear thing.

Avoid giant multi-purpose handlers.

## 14.2 Prefer Application DTOs

Use cases should return DTOs or query models, not domain entities directly to UI.

## 14.3 Keep UI Logic Out

Navigation, page state, and control state belong in the MAUI layer, not in use case handlers.

## 14.4 Keep Import Separate from User UI

Import use cases belong to the operator workflow, not learner-facing screens.

---

# 15. Out-of-Scope Use Cases for Phase 1

The following use cases are explicitly out of scope:

- StartReviewSession
- SubmitPracticeAnswer
- ScheduleReviewQueue
- BuildLearningList
- DownloadContentPackFromServer
- SyncUserFavoritesToCloud
- SyncUserProgressToCloud
- ManageSupportResources
- SearchSupportResourcesByCity
- EditContentInAdminPanel
- MergeExistingWordDuringImport

These belong to later phases.

---

# 16. Phase 1 Use Case Summary

Phase 1 use cases should stay focused on:

- discovering words
- understanding words
- selecting preferred meaning languages
- saving favorites
- recording lightweight user state
- importing structured content safely
- initializing local app data

This is enough to deliver a serious and clean first version.

The correct Phase 1 mindset is:

**useful, stable, local-first, and expandable.**

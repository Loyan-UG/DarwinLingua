# Course Content Package Contract

## Purpose

This document defines the initial JSON import contract for Web-first Course Lessons and CEFR Learning Paths.

Courses are dynamic content. They link to existing grammar topics, words, expressions, dialogues, Talk Topics, exercise sets, roleplays, writing templates, Life in Germany notes, and exam prep units instead of duplicating full teaching content. Course lesson activity blocks define the ordered learning route through those resources.

## Root Arrays

Course packages may include:

- `coursePaths`
- `courseModules`
- `courseLessons`

## Required Fields

Course path:

- `slug`
- `title`
- `titleTranslations`
- `description`
- `descriptionTranslations`
- `cefrLevel` or `cefrRange`
- `sortOrder`

Course module:

- `slug`
- `coursePathSlug`
- `title`
- `titleTranslations`
- `description`
- `descriptionTranslations`
- `moduleNumber`
- `cefrLevel`
- `sortOrder`

Course lesson:

- `slug`
- `coursePathSlug`
- `moduleSlug`
- `lessonNumber`
- `title`
- `titleTranslations`
- `shortDescription`
- `shortDescriptionTranslations`
- `narrative`
- `narrativeTranslations`
- `cefrLevel`
- `estimatedMinutes`
- `learningGoals`
- `learningGoalsTranslations`
- `sortOrder`
- `activityBlocks` for new or touched lessons; legacy lessons without activity blocks remain valid during migration.

Optional lesson links:

- `prerequisiteLessonSlugs`
- `nextLessonSlug`
- `linkedGrammarTopicSlugs`
- `linkedWordSlugs`
- `linkedExpressionSlugs`
- `linkedDialogueSlugs`
- `linkedTalkTopicSlugs`
- `linkedExerciseSetSlugs`
- `linkedExamPrepSlugs`
- `reviewSummary`
- `reviewSummaryTranslations`
- `homeworkTask`
- `homeworkTaskTranslations`
- `isPublished`

## Lesson Activities (`activityBlocks`)

`activityBlocks` is an ordered array embedded in each `CourseLesson`. It is optional for already imported legacy lessons, but required for new Course content and for any existing lesson that is edited after this contract version.

Activity blocks are the primary future source for the lesson reading flow. They tell the learner what to read, listen to, practice, write, roleplay, or review in a deliberate sequence. They do not contain the full grammar, dialogue, exercise, writing template, or linked content body.

Each activity block contains:

- `kind`: one of `read`, `listen`, `grammar`, `expression`, `practice`, `roleplay`, `write`, `life-in-germany`, `exam-prep`, `review`.
- `title`: German canonical activity title.
- `titleTranslations`: helper translations for active learner languages.
- `instruction`: German canonical instruction that explains the learner action.
- `instructionTranslations`: helper translations for active learner languages.
- `targetType`: one of `course-lesson`, `grammar-topic`, `expression`, `dialogue`, `talk-topic`, `exercise-set`, `exercise`, `roleplay`, `writing-template`, `life-in-germany`, `exam-prep-unit`, or `none`.
- `targetSlug`: slug of the target content when `targetType` is not `none`.
- `estimatedMinutes`: estimated time for this activity.
- `sortOrder`: unique ordering value inside the lesson.
- `isRequired`: whether this activity is part of the required lesson path.

Example:

```json
"activityBlocks": [
  {
    "kind": "read",
    "title": "Den Einstieg lesen",
    "titleTranslations": [
      { "language": "en", "text": "Read the introduction" }
    ],
    "instruction": "Lies den kurzen Einstieg und achte auf die Situation.",
    "instructionTranslations": [
      { "language": "en", "text": "Read the short introduction and notice the situation." }
    ],
    "targetType": "none",
    "targetSlug": null,
    "estimatedMinutes": 4,
    "sortOrder": 10,
    "isRequired": true
  },
  {
    "kind": "practice",
    "title": "Kurze Antworten ueben",
    "titleTranslations": [
      { "language": "en", "text": "Practise short answers" }
    ],
    "instruction": "Bearbeite die Uebung und pruefe danach die Erklaerung.",
    "instructionTranslations": [
      { "language": "en", "text": "Complete the exercise and then check the explanation." }
    ],
    "targetType": "exercise-set",
    "targetSlug": "a1-erste-antworten-uebung",
    "estimatedMinutes": 8,
    "sortOrder": 20,
    "isRequired": true
  }
]
```

Educational rules:

- Activity blocks build the learning route; they must not copy full target content.
- German source fields remain canonical. Translations are learner helper text, not replacements.
- Activity titles should describe the action clearly, without repeating metadata that already exists elsewhere, such as CEFR level or module number.
- A lesson activity sequence should feel like a guided book page: read a short source, inspect language, listen or compare where relevant, practise, write or roleplay, and review.
- The first activity should orient the learner. The last required activity should normally be `review`, unless the lesson itself is a short review lesson.

Compatibility rules:

- Existing link lists such as `linkedGrammarTopicSlugs`, `linkedExerciseSetSlugs`, and related fields are not removed.
- `activityBlocks` is now the primary UI source for the lesson flow whenever a lesson provides activity blocks.
- Legacy slug lists remain available for fallback display, admin diagnostics, search indexing, and migration checks until the migration is complete.
- Legacy lessons without activity blocks remain learner-usable through the old linked-content fallback until controlled backfill is complete.

## German-First Localization Rule

Course source fields are German-first and canonical. `title`, `description`, `shortDescription`, `narrative`, `learningGoals`, `reviewSummary`, and `homeworkTask` must be German source text.

Learner-language fields are helper translations only. They must not replace the German source in Web/API responses.

Translation arrays use:

```json
{ "language": "fa", "text": "..." }
```

`learningGoalsTranslations` uses list translations because each German goal remains a separate learner-visible item:

```json
{ "language": "fa", "texts": ["...", "..."] }
```

## Validation Rules

- slugs must be lowercase kebab-case
- CEFR levels must be valid
- module and lesson numbers must be positive
- lessons must contain at least one learning goal
- learner-helper translations must use active learner meaning languages only
- duplicate translation languages are rejected
- `learningGoalsTranslations[*].texts` must match the number of German `learningGoals`
- module references must point to course paths in the same package
- lesson references must point to modules in the same package
- prerequisite and next lesson links must point to lessons in the same package
- a lesson cannot reference itself as prerequisite or next lesson
- new or touched lessons must include a valid `activityBlocks` array
- `activityBlocks.kind` must be one of the supported activity kinds
- `activityBlocks.targetType` must be one of the supported target types
- `activityBlocks.sortOrder` must be unique inside the same lesson
- `activityBlocks.targetSlug` is required when `targetType` is not `none`
- `activityBlocks.targetSlug` must be empty or null when `targetType` is `none`
- `activityBlocks.targetSlug` must be lowercase kebab-case when present
- each new or touched lesson must include at least one `read` or `review` activity
- activity helper translations must use active learner meaning languages only
- duplicate translation languages are rejected for each activity translation field

## No Duplication Rule

Course lessons may provide a short narrative, learning goals, review summary, and practice task. They must not copy full grammar articles, word meanings, expression explanations, full dialogue content, or full Talk Topic text.

`activityBlocks` follow the same rule: they may point to target content and explain the learner action, but they must not embed the full target content.

Bulk course content generation must not start until this contract, validation, Web API, Web rendering, and admin inspection are stable.

## Import Semantics

Course imports are cumulative per `CoursePath` in the current importer. When a package contains an existing course path slug, the importer replaces that path and its child modules/lessons with the package version.

Therefore, small Course batches for an existing path must keep the full reviewed path tree produced so far, not only the newly added lessons. This prevents accidentally deleting earlier lessons during import.

`activityBlocks` are stored with each lesson as JSON. Existing legacy lessons without activity blocks remain valid until backfill is complete.

## Web/API Behavior

Course API endpoints accept optional `primaryMeaningLanguageCode`.

Responses include German source fields plus localized helper fields when available. Web rendering shows German first, then the learner helper text according to the user's selected content language.

Course APIs expose ordered activity cards with German source text, learner helper translations, and target metadata.

The Web lesson page renders those cards as the primary lesson flow when `activityBlocks` is non-empty. Activity links are resolved to learner-facing routes and raw target slugs are not shown in the main flow. Lessons without activity blocks keep the legacy linked-content fallback.

Admin diagnostics report published lessons without activity blocks, malformed activity JSON, unsupported activity target types, and unresolved activity target slugs.

As of the C2 Module 12 checkpoint, the full A1 path has been backfilled and imported with reviewed activity blocks (`60/60` lessons, 297 activities), the full A2 path has been backfilled (`80/80` lessons, 400 activities), the full B1 path has been backfilled (`100/100` lessons, 500 activities), the full B2 path has been backfilled (`80/80` lessons, 400 activities), the full C1 path has been backfilled (`120/120` lessons, 600 activities), and the full C2 path has been backfilled (`120/120` lessons, 600 activities). Current live PostgreSQL verification reports `CourseLessons=560`, `C2ActivityEnabled=120`, `TotalActivityEnabled=560`, `ActiveLessonsWithoutActivityBlocks=0`, and zero unresolved C2 activity targets. Public Web/API smoke passed for the final C2 lesson and Persian helper projection. A restore-ready staging backup exists at `D:\_Projects\DarwinLingua.Backup.Staging\20260618-073641-course-c2-activity-flow-complete-pre-user-testing` with dump, restore list, dry-run restore, manifest, checksums, repo overlay, Docker metadata, and separate secret bundle. Final sync to `X:\Projects\DarwinLingua.Backup` remains required before the backup gate is fully closed. Legacy slug-list fields remain compatibility fallback and admin/search support, but every imported Course lesson now has `activityBlocks`.

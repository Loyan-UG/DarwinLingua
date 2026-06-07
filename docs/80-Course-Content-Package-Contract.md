# Course Content Package Contract

## Purpose

This document defines the initial JSON import contract for Web-first Course Lessons and CEFR Learning Paths.

Courses are dynamic content. They link to existing grammar topics, words, expressions, dialogues, Talk Topics, and exercise sets instead of duplicating full teaching content.

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

## No Duplication Rule

Course lessons may provide a short narrative, learning goals, review summary, and practice task. They must not copy full grammar articles, word meanings, expression explanations, full dialogue content, or full Talk Topic text.

Bulk course content generation must not start until this contract, validation, Web API, Web rendering, and admin inspection are stable.

## Import Semantics

Course imports are cumulative per `CoursePath` in the current importer. When a package contains an existing course path slug, the importer replaces that path and its child modules/lessons with the package version.

Therefore, small Course batches for an existing path must keep the full reviewed path tree produced so far, not only the newly added lessons. This prevents accidentally deleting earlier lessons during import.

## Web/API Behavior

Course API endpoints accept optional `primaryMeaningLanguageCode`.

Responses include German source fields plus localized helper fields when available. Web rendering shows German first, then the learner helper text according to the user's selected content language.

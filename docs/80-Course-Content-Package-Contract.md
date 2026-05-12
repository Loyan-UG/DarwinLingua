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
- `description`
- `cefrLevel` or `cefrRange`
- `sortOrder`

Course module:

- `slug`
- `coursePathSlug`
- `title`
- `description`
- `moduleNumber`
- `cefrLevel`
- `sortOrder`

Course lesson:

- `slug`
- `coursePathSlug`
- `moduleSlug`
- `lessonNumber`
- `title`
- `shortDescription`
- `narrative`
- `cefrLevel`
- `estimatedMinutes`
- `learningGoals`
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
- `homeworkTask`
- `isPublished`

## Validation Rules

- slugs must be lowercase kebab-case
- CEFR levels must be valid
- module and lesson numbers must be positive
- lessons must contain at least one learning goal
- module references must point to course paths in the same package
- lesson references must point to modules in the same package
- prerequisite and next lesson links must point to lessons in the same package
- a lesson cannot reference itself as prerequisite or next lesson

## No Duplication Rule

Course lessons may provide a short narrative, learning goals, review summary, and practice task. They must not copy full grammar articles, word meanings, expression explanations, full dialogue content, or full Talk Topic text.

Bulk course content generation must not start until this contract, validation, Web API, Web rendering, and admin inspection are stable.

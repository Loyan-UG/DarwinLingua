# Exercise Content Package Contract

## Purpose

This document defines the JSON/import contract for the Phase 7 reusable Exercise Engine.

Exercises are dynamic content records. They must link to learning content instead of duplicating grammar lessons, word meanings, expression explanations, dialogue content, course lessons, or exam-prep units.

## Package Shape

Exercise definitions are imported through the existing content package format under `exercises` and `exerciseSets`.

```json
{
  "packageVersion": "1.0",
  "packageId": "exercise-sample",
  "packageName": "Exercise Sample",
  "defaultMeaningLanguages": ["en"],
  "entries": [],
  "exercises": [],
  "exerciseSets": []
}
```

## Exercise Fields

Required: `slug`, `title`, `instruction`, `cefrLevel`, `exerciseType`, `targetSkill`, `ownerType`, `prompt`, `answerKey`, `correctExplanation`, `incorrectExplanation`.

Optional: `ownerSlug`, `hint`, `commonMistakeNote`, `isPublished`, `sortOrder`.

## Supported Types

Initial deterministic types: `multiple-choice`, `fill-in-the-blank`, `matching`, `sentence-ordering`, `error-correction`, `article-selection`, `case-selection`, `conjugation`, `translation-controlled`, `dialogue-completion`, `vocabulary-choice`, `grammar-choice`.

Future-only types are not accepted by the importer yet: dictation, listening comprehension, pronunciation shadowing, writing prompts, speaking prompts, and AI-assisted feedback.

## Target Skills

Supported target skills: `grammar`, `vocabulary`, `reading`, `listening`, `speaking`, `writing`, `pronunciation`, `exam-preparation`.

## Owner Types

Supported owner types: `word`, `grammar-topic`, `expression`, `dialogue`, `talk-topic`, `course-lesson`, `exam-prep-unit`.

Owner slugs may point to modules that are not implemented yet. Those references must fail safely in UI and reports.

## Answer Key Rules

- choice-style exercises require `correctOptionIds`
- fill-in/conjugation/controlled translation require `acceptedAnswers`
- matching requires `pairs`
- sentence ordering requires `orderedSegments`
- error correction requires `correctedText`

The Web runner must never expose `answerKey` before submission. It receives only `prompt`.

## Exercise Sets

Exercise sets require `slug`, `title`, `description`, `cefrLevel`, `ownerType`, and `exerciseSlugs`.

## Minimal Example

```json
{
  "slug": "a1-article-der",
  "title": "Choose the article",
  "instruction": "Choose the correct article.",
  "cefrLevel": "A1",
  "exerciseType": "article-selection",
  "targetSkill": "grammar",
  "ownerType": "grammar-topic",
  "ownerSlug": "a1-definite-articles",
  "prompt": {
    "stem": "___ Kaffee",
    "options": [
      { "id": "der", "text": "der" },
      { "id": "die", "text": "die" },
      { "id": "das", "text": "das" }
    ]
  },
  "answerKey": {
    "correctOptionIds": ["der"]
  },
  "correctExplanation": "Der Kaffee is masculine.",
  "incorrectExplanation": "Review article gender for common nouns.",
  "hint": "Kaffee is masculine."
}
```

## Content Generation Rule

Bulk exercise content generation must not start until implementation, validation, Web API, Web rendering, admin visibility, and release tests are stable.

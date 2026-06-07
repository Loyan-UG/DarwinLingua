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
  "defaultMeaningLanguages": ["en", "fa", "ar", "tr", "ru", "ckb", "kmr", "pl", "ro", "sq"],
  "entries": [],
  "exercises": [],
  "exerciseSets": []
}
```

## Exercise Fields

Required: `slug`, `title`, `instruction`, `cefrLevel`, `exerciseType`, `targetSkill`, `ownerType`, `prompt`, `answerKey`, `correctExplanation`, `incorrectExplanation`.

Optional: `ownerSlug`, `hint`, `commonMistakeNote`, `isPublished`, `sortOrder`.

Learner helper translations are optional at the schema level but required for reviewed multilingual packages:

- `titleTranslations`
- `instructionTranslations`
- `correctExplanationTranslations`
- `incorrectExplanationTranslations`
- `hintTranslations`
- `commonMistakeNoteTranslations`

Each translation item uses `{ "language": "...", "text": "..." }`. Languages must be active learner meaning languages, duplicate language entries are rejected, and non-English fields must not be English fallback. The German source fields remain canonical and must not be replaced by localized helper text.

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

Reviewed multilingual exercise sets should also include:

- `titleTranslations`
- `descriptionTranslations`

The importer rejects empty exercise sets and unknown exercise slugs inside the same package. Admin reports flag unresolved persisted set items, unpublished drafts, missing translations, malformed prompt/answer-key JSON, and missing explanations.

## Minimal Example

```json
{
  "slug": "a1-article-der",
  "title": "Den Artikel wählen",
  "titleTranslations": [{ "language": "en", "text": "Choose the article" }],
  "instruction": "Wähle den richtigen Artikel.",
  "instructionTranslations": [{ "language": "en", "text": "Choose the correct article." }],
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
  "correctExplanation": "Es heißt: der Kaffee.",
  "correctExplanationTranslations": [{ "language": "en", "text": "The correct form is: der Kaffee." }],
  "incorrectExplanation": "Kaffee ist maskulin: der Kaffee.",
  "incorrectExplanationTranslations": [{ "language": "en", "text": "Kaffee is masculine: der Kaffee." }],
  "hint": "Kaffee ist maskulin.",
  "hintTranslations": [{ "language": "en", "text": "Kaffee is masculine." }]
}
```

## Content Generation Rule

Bulk exercise content generation must not start until implementation, validation, Web API, Web rendering, admin visibility, and release tests are stable. The first reviewed Exercise package now validates and imports with multilingual helper translations; future Exercise content should remain small-batch and German-first.

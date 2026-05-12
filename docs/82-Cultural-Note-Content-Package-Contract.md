# Cultural Note Content Package Contract

## Purpose

This document defines the initial JSON import contract for Web-first Cultural Notes.

Cultural Notes teach German communication norms, social expectations, and context-specific language use. Notes are dynamic content and must not be hardcoded in Razor views.

## Root Array

- `culturalNotes`

## Required Fields

- `slug`
- `title`
- `shortDescription`
- `cefrLevel`
- `category`
- `context`
- `sections`
- `sortOrder`

Optional fields:

- `examples`
- `doNotes`
- `dontNotes`
- `sensitivityWarning`
- `linkedDialogueSlugs`
- `linkedExpressionSlugs`
- `linkedWritingTemplateSlugs`
- `linkedTalkTopicSlugs`
- `linkedCourseLessonSlugs`
- `isPublished`

## Controlled Categories

- `du-vs-sie`
- `politeness`
- `directness`
- `small-talk`
- `workplace-culture`
- `office-communication`
- `school-kindergarten`
- `doctor-visit`
- `appointments`
- `punctuality`
- `complaints`
- `bureaucracy`
- `conversation-cafe-etiquette`

## Example Shape

```json
{
  "slug": "a2-du-vs-sie-at-work",
  "title": "Du vs. Sie at work",
  "shortDescription": "A practical note about choosing address forms.",
  "cefrLevel": "A2",
  "category": "du-vs-sie",
  "context": "Workplace introductions",
  "sections": ["Use Sie until a colleague offers du."],
  "examples": [
    {
      "germanText": "Sollen wir uns duzen?",
      "explanation": "A polite way to ask about switching to du."
    }
  ],
  "doNotes": ["Start with Sie in formal settings."],
  "dontNotes": ["Do not switch to du automatically."],
  "sensitivityWarning": "Address forms can feel personal in hierarchical contexts.",
  "linkedDialogueSlugs": ["a2-workplace-introduction"],
  "linkedExpressionSlugs": ["sollen-wir-uns-duzen"],
  "linkedWritingTemplateSlugs": ["a2-formal-work-email"],
  "linkedTalkTopicSlugs": ["a2-workplace-small-talk"],
  "linkedCourseLessonSlugs": ["a2-workplace-communication"],
  "sortOrder": 10
}
```

## Validation Rules

- slugs must be lowercase kebab-case
- CEFR levels must be valid
- category must use a controlled value
- `sections` must contain at least one non-empty item
- examples require `germanText`
- linked slugs must use lowercase kebab-case
- sensitive topics should include `sensitivityWarning`

## Safety Rule

Cultural Notes may discuss sensitive communication expectations, but the tone must stay neutral, practical, and learner-safe.

## No Duplication Rule

Cultural Notes may link to dialogues, expressions, writing templates, Talk Topics, and course lessons. They should not duplicate full linked content.

Bulk cultural-note content generation must not start until this contract, validation, Web API, Web rendering, and admin inspection are stable.

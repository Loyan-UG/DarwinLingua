# Exam Prep Content Package Contract

## Purpose

This document defines the initial JSON import contract for Web-first Exam Preparation content.

Exam Prep content must be original learning material. Do not copy raw provider exam pages, exam-book tasks, answer keys, prompts, audio transcripts, or copyrighted sample tests unless the project has a documented license and manual approval.

## Root Arrays

- `examProfiles`
- `examPrepUnits`

## ExamProfile Fields

Required:

- `key`
- `displayName`
- `cefrRange`
- `description`
- `sortOrder`

Optional:

- `isPublished`

Initial profile keys:

- `goethe-a1`
- `goethe-a2`
- `goethe-b1`
- `goethe-b2`
- `telc-a1`
- `telc-a2`
- `telc-b1`
- `telc-b2`
- `dtz-a2-b1`
- `berufssprache-b1`
- `berufssprache-b2`
- `c1-hochschule`
- `testdaf`

## ExamPrepUnit Fields

Required:

- `slug`
- `examProfileKey`
- `title`
- `shortDescription`
- `cefrLevel`
- `examSection`
- `taskType`
- `skillFocus`
- `explanation`
- `sortOrder`

Optional linked-content fields:

- `strategyNotes`
- `checklist`
- `linkedDialogueSlugs`
- `linkedTalkTopicSlugs`
- `linkedGrammarTopicSlugs`
- `linkedExpressionSlugs`
- `linkedWritingTemplateSlugs`
- `linkedExerciseSlugs`
- `linkedCourseLessonSlugs`
- `isPublished`

## Controlled Exam Sections

- `speaking`
- `writing`
- `reading`
- `listening`
- `grammar-vocabulary`
- `overview`
- `strategy`
- `mock-task`

## Initial Task Types

- `overview`
- `strategy`
- `roleplay`
- `discussion`
- `presentation`
- `email`
- `opinion-text`
- `form-filling`
- `reading-task`
- `listening-task`
- `grammar-vocabulary`
- `mock-task`
- `scoring-checklist`

## Example Shape

```json
{
  "examProfiles": [
    {
      "key": "goethe-a2",
      "displayName": "Goethe A2",
      "cefrRange": "A2",
      "description": "Goethe A2 preparation.",
      "sortOrder": 10
    }
  ],
  "examPrepUnits": [
    {
      "slug": "a2-goethe-speaking-roleplay",
      "examProfileKey": "goethe-a2",
      "title": "Speaking roleplay strategy",
      "shortDescription": "Prepare a short A2 roleplay.",
      "cefrLevel": "A2",
      "examSection": "speaking",
      "taskType": "roleplay",
      "skillFocus": "exam-preparation",
      "explanation": "Use short, clear questions and answers.",
      "strategyNotes": ["Ask for clarification when needed."],
      "checklist": ["Answer the prompt directly."],
      "linkedDialogueSlugs": ["a2-appointment-roleplay"],
      "linkedTalkTopicSlugs": ["a2-appointments"],
      "linkedGrammarTopicSlugs": ["a2-question-word-order"],
      "linkedExpressionSlugs": ["koennten-sie-bitte"],
      "linkedWritingTemplateSlugs": ["a2-appointment-reschedule"],
      "linkedExerciseSlugs": ["a2-speaking-roleplay-practice"],
      "linkedCourseLessonSlugs": ["a2-appointments-lesson"],
      "sortOrder": 20
    }
  ]
}
```

## Validation Rules

- slugs and keys must be lowercase kebab-case
- `examProfileKey` must use a supported profile key
- CEFR levels must be valid
- `examSection` must use a controlled value
- `taskType` must use a controlled value
- `skillFocus` must use a supported skill value
- linked slugs must use lowercase kebab-case
- exam content must be original, manually authored learning material

Bulk exam-prep content generation must not start until this contract, validation, Web API, Web rendering, and admin inspection are stable.

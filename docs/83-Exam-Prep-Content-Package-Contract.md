# Exam Prep Content Package Contract

## Purpose

This document defines the initial JSON import contract for Web-first Exam Preparation content.

Exam Prep content must be original learning material. Do not copy raw provider exam pages, exam-book tasks, answer keys, prompts, audio transcripts, or copyrighted sample tests unless the project has a documented license and manual approval.

Exam Prep is German-first. Source fields are canonical German learning text. Helper translations support the learner's selected meaning language and must not replace or dilute the German source.

## Root Arrays

- `examProfiles`
- `examPrepUnits`

## ExamProfile Fields

Required:

- `key`
- `displayName`
- `displayNameTranslations`
- `cefrRange`
- `description`
- `descriptionTranslations`
- `sortOrder`

Optional:

- `isPublished`

Initial profile keys:

- `goethe-a1`
- `goethe-a2`
- `goethe-b1`
- `goethe-b2`
- `goethe-c1`
- `goethe-c2`
- `telc-a1`
- `telc-a2`
- `telc-b1`
- `telc-b2`
- `telc-c1`
- `telc-c1-hochschule`
- `dtz-a2-b1`
- `berufssprache-b1`
- `berufssprache-b2`
- `berufssprache-c1`
- `c1-hochschule`
- `testdaf`
- `testdaf-b2-c1`
- `dsh`
- `dsh-c1`
- `oesd-c1`

## ExamPrepUnit Fields

Required:

- `slug`
- `examProfileKey`
- `title`
- `titleTranslations`
- `shortDescription`
- `shortDescriptionTranslations`
- `cefrLevel`
- `examSection`
- `taskType`
- `skillFocus`
- `explanation`
- `explanationTranslations`
- `strategyNotes`
- `strategyNotesTranslations`
- `checklist`
- `checklistTranslations`
- `sortOrder`

Optional linked-content fields:

- `linkedDialogueSlugs`
- `linkedTalkTopicSlugs`
- `linkedGrammarTopicSlugs`
- `linkedExpressionSlugs`
- `linkedWritingTemplateSlugs`
- `linkedExerciseSlugs`
- `linkedRoleplaySlugs`
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

## Translation Fields

Text helper translations use the standard shape:

```json
{ "language": "fa", "text": "..." }
```

List helper translations use the text-list shape and must align one-to-one with the German source array:

```json
{ "language": "fa", "texts": ["...", "..."] }
```

Required active learner languages:

- `en`
- `fa`
- `ar`
- `tr`
- `ru`
- `ckb`
- `kmr`
- `pl`
- `ro`
- `sq`

Rules:

- `displayName`, `description`, `title`, `shortDescription`, `explanation`, `strategyNotes`, and `checklist` are German source content.
- `displayNameTranslations`, `descriptionTranslations`, `titleTranslations`, `shortDescriptionTranslations`, and `explanationTranslations` must include all active learner languages.
- `strategyNotesTranslations` and `checklistTranslations` must include all active learner languages and the same number of items as their German source arrays.
- Non-English helper translations must be real translations, not English fallback text.
- UI chrome labels still come from Web resources; content helper translations are package data.

## Title And Helper Translation Quality

Exam Prep titles are learner-facing content titles, not metadata summaries.

- Do not put CEFR level, exam profile, or exam section into `title` when the same information is already stored in `cefrLevel`, `examProfileKey`, `examSection`, or `taskType`.
- Prefer natural task titles such as `Einen Termin oder ein Problem klaeren`, not `Im A2-Sprechen einen Termin oder ein Problem klaeren`.
- Proper exam/provider names may appear in `displayName`, profile metadata, filters, and badges. They should appear in unit titles only when the unit is specifically about comparing or understanding that provider's format.
- Helper translations must explain the German source naturally in the learner language. Literal translations that preserve awkward German or English word order are not acceptable.
- Non-English helper translations must not use English fallback or English education labels unless that borrowed term is genuinely the standard term in that target language.

## Linked Practice Rules

Linked practice fields should connect Exam Prep to already imported, reviewed learning content.

- Link Course, Dialogue, Roleplay, Exercise, Grammar, Expression, Talk Topic, or Writing Template slugs only when that content exists.
- Do not invent linked slugs for future material.
- Leave `linkedWritingTemplateSlugs` empty until Writing Templates are imported.
- If a generated package leaves all linked-practice fields empty, planning must document why no suitable links exist yet.

## Example Shape

```json
{
  "examProfiles": [
    {
      "key": "goethe-a2",
      "displayName": "Goethe A2",
      "displayNameTranslations": [
        { "language": "en", "text": "Goethe A2 exam profile" },
        { "language": "fa", "text": "پروفایل آزمون گوته A2" }
      ],
      "cefrRange": "A2",
      "description": "Vorbereitung auf die Goethe-A2-Pruefung.",
      "descriptionTranslations": [
        { "language": "en", "text": "Goethe A2 exam preparation" },
        { "language": "fa", "text": "آمادگی آزمون گوته A2" }
      ],
      "sortOrder": 10
    }
  ],
  "examPrepUnits": [
    {
      "slug": "a2-goethe-speaking-roleplay",
      "examProfileKey": "goethe-a2",
      "title": "Strategie fuer das Sprechrollenspiel",
      "titleTranslations": [
        { "language": "en", "text": "Speaking roleplay strategy" },
        { "language": "fa", "text": "راهبرد نقش‌آفرینی شفاهی" }
      ],
      "shortDescription": "Bereite ein kurzes Rollenspiel vor.",
      "shortDescriptionTranslations": [
        { "language": "en", "text": "Prepare a short roleplay" },
        { "language": "fa", "text": "یک نقش‌آفرینی کوتاه را آماده کن" }
      ],
      "cefrLevel": "A2",
      "examSection": "speaking",
      "taskType": "roleplay",
      "skillFocus": "exam-preparation",
      "explanation": "Nutze kurze, klare Fragen und Antworten.",
      "explanationTranslations": [
        { "language": "en", "text": "Use short, clear questions and answers" },
        { "language": "fa", "text": "از پرسش‌ها و پاسخ‌های کوتاه و روشن استفاده کن" }
      ],
      "strategyNotes": ["Ask for clarification when needed."],
      "strategyNotesTranslations": [
        { "language": "en", "texts": ["Ask for clarification when needed"] },
        { "language": "fa", "texts": ["وقتی لازم است، درخواست توضیح بیشتر کن"] }
      ],
      "checklist": ["Answer the prompt directly."],
      "checklistTranslations": [
        { "language": "en", "texts": ["Answer the prompt directly"] },
        { "language": "fa", "texts": ["به موضوع خواسته‌شده مستقیم پاسخ بده"] }
      ],
      "linkedDialogueSlugs": ["a2-appointment-roleplay"],
      "linkedTalkTopicSlugs": ["a2-appointments"],
      "linkedGrammarTopicSlugs": ["a2-question-word-order"],
      "linkedExpressionSlugs": ["koennten-sie-bitte"],
      "linkedWritingTemplateSlugs": ["a2-appointment-reschedule"],
      "linkedExerciseSlugs": ["a2-speaking-roleplay-practice"],
      "linkedRoleplaySlugs": ["a2-termin-verschieben-am-telefon"],
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
- required helper translation fields must cover all active learner languages
- list helper translations must have the same item count as the German source list
- non-English helper translations must not fall back to English
- generated titles must not unnecessarily repeat CEFR/profile/section metadata
- linked slugs must use lowercase kebab-case
- linked-practice fields must be populated from reviewed existing content where suitable content exists, or the planning note must document why they remain empty
- exam content must be original, manually authored learning material

Bulk exam-prep content generation must not start until this contract, validation, Web API, Web rendering, and admin inspection are stable.

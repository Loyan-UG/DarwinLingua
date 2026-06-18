# Exam Prep A1-A2 Pilot Candidates

## Purpose

This planning note defines the first small reviewed Exam Prep package after the Exam Prep v1 infrastructure gate. It is intentionally narrow: original strategy and checklist content only, no copied official tasks, answer keys, transcripts, prompts, or provider sample material.

## Current Status

The first generated pilot was rejected on 2026-06-08 and removed from the official package path and shared PostgreSQL database.

Reasons:

- Several titles mixed metadata into the learner-facing title, for example CEFR level or exam section text that already belongs in `cefrLevel`, `examProfileKey`, `examSection`, and `taskType`.
- Some helper translations were literal but not natural enough for learner support.
- Linked practice fields were left empty even though the platform already has Course, Dialogue, Roleplay, and Exercise content that can be referenced after review.

Do not regenerate this pilot until the candidate list below has been reviewed against available linked content.

## Generation Rules

- German source is canonical for profile and unit content.
- Helper translations must cover `en`, `fa`, `ar`, `tr`, `ru`, `ckb`, `kmr`, `pl`, `ro`, and `sq`.
- Non-English helper translations must not fall back to English.
- Unit titles must be clean pedagogical titles. Do not prefix titles with CEFR levels, exam names, or exam sections when that data already exists in metadata fields.
- Helper translations must be natural learner support in each target language, not word-for-word glosses. Example: translate `wenn es natuerlich passt` as meaning-based language such as "if it fits the meaning/context", not as an awkward literal phrase.
- Units should teach exam behavior, task framing, and self-check routines, not duplicate full Course, Dialogue, Exercise, Writing Template, or Roleplay content.
- A1/A2 language stays practical and concise, while explanations are complete enough for a learner to act on.
- Linked practice should be populated only with reviewed existing content slugs. Leave a linked field empty only when no suitable imported content exists yet.

## Candidate Backlog

| Order | Suggested Slug | German Title | Exam Profile | CEFR | Section | Task Type | Skill Focus | Core Learner Task |
| ---: | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | `a1-goethe-pruefung-ueberblick` | Pruefungsteile verstehen | `goethe-a1` | A1 | overview | overview | exam-preparation | Understand the parts of the A1 exam and how to prepare without memorizing scripts. |
| 2 | `a1-sprechen-sich-vorstellen` | Sich sicher vorstellen | `goethe-a1` | A1 | speaking | strategy | speaking | Build a short, natural self-introduction and answer simple follow-up questions. |
| 3 | `a1-formulare-und-kurze-notizen` | Formulare und kurze Notizen ausfuellen | `goethe-a1` | A1 | writing | form-filling | writing | Handle basic form fields and short notes with correct, simple information. |
| 4 | `a1-lese-und-hoeraufgaben-erkennen` | Lese- und Hoeraufgaben erkennen | `goethe-a1` | A1 | strategy | strategy | reading | Recognize what an A1 reading/listening task asks before choosing an answer. |
| 5 | `a2-goethe-pruefung-ueberblick` | Pruefungserwartungen einordnen | `goethe-a2` | A2 | overview | overview | exam-preparation | Understand A2 exam expectations and how they differ from A1. |
| 6 | `a2-sprechen-termin-und-problem` | Einen Termin oder ein Problem klaeren | `goethe-a2` | A2 | speaking | roleplay | speaking | Ask, explain, clarify, and close a short practical roleplay. |
| 7 | `a2-schreiben-kurze-email-planen` | Eine kurze E-Mail planen | `goethe-a2` | A2 | writing | email | writing | Plan a short email with greeting, reason, request, and closing. |
| 8 | `a2-checkliste-vor-der-pruefung` | Checkliste vor der Pruefung nutzen | `goethe-a2` | A2 | strategy | scoring-checklist | exam-preparation | Use a simple checklist for timing, clarity, politeness, and direct answers. |

## Linked Practice Availability

As of the rejected pilot cleanup, imported related content exists for:

- Course lessons
- Dialogue lessons
- Roleplay scenarios
- Exercises

No Writing Templates are imported yet, so `linkedWritingTemplateSlugs` must stay empty until that content exists.

## Pilot Package

The next generated package should be rebuilt from scratch after linked-practice review. It may include a smaller subset than all 8 candidates if that improves translation and pedagogical quality.

Required active profiles for the first regenerated pilot should be only the profiles used by the package. Do not import placeholder profiles unless they have at least one linked or planned unit in the same package.

## Production Readiness Gate

Before import:

- JSON parse passes.
- ContentOps parser/import validation has zero errors.
- Translation coverage is complete for every required helper field.
- No non-English helper text is copied from English.
- At least one reviewer sample checks Persian helper text for natural wording.
- Titles do not repeat CEFR level, exam profile, or section metadata unnecessarily.
- Linked practice fields are intentionally populated or intentionally empty with a documented reason.
- Package remains original and does not include official exam material.

After import:

- PostgreSQL counts show the expected active profiles and 8 active units.
- `/api/catalog/exam-profiles`, `/api/catalog/exam-prep`, `/api/catalog/exam-prep/{slug}`, search result type `exam-prep`, and the admin report are smoke-checked.

# Roleplay Content Package Contract

## Purpose

This document defines the Phase 6 contract for scripted roleplay practice.

Roleplay is an MVP practice layer on top of dialogue practice lessons. It must work without AI, without chat, and without learner-generated content being sent to a model.

## Current Implementation Status

Last updated: 2026-05-25.

Standalone `RoleplayScenario` infrastructure has an initial Web-first implementation slice: parser support, application validation, shared-content persistence, repository/query services, Web API list/detail routes, Web list/detail rendering, Unified Learning Search result support, and admin count visibility. The first parser test and full solution build pass.

Current supported behavior:

- Dialogue content can be used as source material for future roleplay scenarios.
- Event Preparation Packs can include simple `roleplayPrompts`.
- The Web MVP may derive basic practice from existing Dialogue turns.

Still blocked before a reviewed pilot package can be generated:

- dedicated application import tests for valid/invalid `roleplayScenarios`
- repository/API/Web structural tests for list/detail filters and image-slot rendering
- admin quality tests for missing translations, missing answer choices/static feedback, and invalid playable sequence
- local import and Web/API/search/admin smoke against the shared development database

Do not generate standalone `RoleplayScenario` packages until those validation gaps are closed.

## `RoleplayScenario`

A `RoleplayScenario` is a structured deterministic practice flow. It may link to one `DialogueLesson`, but it must not duplicate full Dialogue lessons unnecessarily.

Top-level package array:

- `roleplayScenarios`

Required fields:

- `slug`: stable lowercase identifier
- `linkedDialogueSlug`: optional existing `DialogueLesson.slug`
  - Compatibility alias: packages may use `scenarioSlug`; the importer maps it to `linkedDialogueSlug`.
- `title`: learner-facing title
- `description`: learner-facing summary
- `learnerGoal`: concrete goal for the learner
- `cefrLevel`: expected `A1`, `A2`, `B1`, `B2`, `C1`, or `C2`
- `category`: lowercase kebab-case category
- `topics`: active topic keys
- `examProfiles`: optional lowercase kebab-case exam profile keys
- `skillFocus`: lowercase kebab-case skill focus values
- `taskType`: controlled value shared with Dialogue task types
- `interactionMode`: controlled value shared with Dialogue interaction modes
- `register`: `formal`, `informal`, `neutral`, or `mixed`
- `estimatedPracticeMinutes`: positive integer
- `roles`: ordered list of role labels used by the flow
- `turns`: ordered scripted turn sequence
- `answerChoices`: required for guided learner choice moments
- `staticFeedback`: deterministic feedback blocks shown without AI
- `imageSlots`: optional image placeholders
- `isPublished`: boolean
- `sortOrder`: non-negative integer

The Web MVP may derive a default `RoleplayScenario` from `DialogueLesson.dialogueTurns` when no dedicated roleplay package exists.

## Role Labels

Role labels are lowercase, stable identifiers. The supported starter set is:

- `learner`
- `doctor`
- `teacher`
- `colleague`
- `organizer`
- `partner`
- `neighbor`
- `landlord`
- `office-staff`
- `interviewer`

Content may add a role only when the label is stable enough for filtering, analytics, and translation.

## Scripted Turn Sequence

Each turn represents either the other person's prompt or the learner's model response.

Required fields:

- `sortOrder`: positive integer
- `speakerRole`: one of the scenario roles
- `baseText`: German text
- `translations`: learner-language translations for active meaning languages

Optional fields:

- `function`
- `toneNote`
- `expectedLearnerAction`

Validation rules:

- `sortOrder` values must be unique within the roleplay scenario.
- A playable sequence must contain at least one non-learner prompt followed by a learner response.
- Learner turns must have either a scripted model answer or answer choices.

## Answer Choices

`answerChoices` are optional for the early MVP. They support guided practice before free typing.

Required fields:

- `turnSortOrder`: learner turn that owns the choices
- `choices`: list of answer choices

Each choice requires:

- `id`: lowercase kebab-case answer identifier
- `text`: German answer text
- `translations`: learner-language translations
- `isCorrect`: boolean
- `feedback`: static feedback shown after selection
- `feedbackTranslations`: learner-language feedback translations
- `explanationKey`: optional lowercase kebab-case explanation key

At least one choice must be correct when choices are provided for a turn.

## Static Feedback

Static feedback is deterministic content authored with the package.

Supported fields:

- `turnSortOrder`: optional turn reference
- `feedbackType`: lowercase kebab-case, for example `model-answer`, `politeness-note`, `common-mistake`, or `pronunciation-prompt`
- `text`: learner-facing feedback
- `translations`: learner-language feedback translations

MVP feedback must be short and must not pretend to evaluate an unsent free-text answer.

## Image Slots

`imageSlots` are optional placeholders for future generated or curated images. Do not generate binary images inside roleplay content tasks.

Each image slot requires:

- `slotKey`: lowercase kebab-case
- `placement`: lowercase kebab-case placement such as `header`, `context`, or `sidebar`
- `purpose`: author-facing reason for the image
- `altText`: German or English accessible alt text
- `altTextTranslations`: learner-language alt text translations
- `imagePrompt`: safe image-generation prompt for a neutral educational illustration
- `assetPath`: nullable path to a future asset
- `isRequired`: boolean

Missing `assetPath` must not fail import or Web rendering. Normal learners must not see `imagePrompt`; it is authoring metadata.

## AI Boundary

AI-assisted roleplay feedback is outside the MVP.

Before AI feedback is added, the project must define:

- cost controls
- abuse and safety rules
- data retention rules
- entitlement boundaries
- fallback behavior when AI is unavailable

Until those boundaries exist, Web roleplay remains scripted and static.

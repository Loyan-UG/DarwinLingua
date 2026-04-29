# Roleplay Content Package Contract

## Purpose

This document defines the Phase 6 contract for scripted roleplay practice.

Roleplay is an MVP practice layer on top of scenario lessons. It must work without AI, without chat, and without learner-generated content being sent to a model.

## `RoleplayScenario`

A `RoleplayScenario` is a structured practice flow connected to one `ScenarioLesson`.

Required fields:

- `slug`: stable lowercase identifier
- `scenarioSlug`: existing `ScenarioLesson.slug`
- `title`: learner-facing title
- `cefrLevel`: expected `A1`, `A2`, `B1`, `B2`, `C1`, or `C2`
- `roles`: ordered list of role labels used by the flow
- `turns`: ordered scripted turn sequence
- `answerChoices`: optional early-MVP answer choices for learner turns
- `staticFeedback`: optional feedback blocks shown without AI

The Web MVP may derive a default `RoleplayScenario` from `ScenarioLesson.dialogueTurns` when no dedicated roleplay package exists.

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
- `office_staff`
- `interviewer`

Content may add a role only when the label is stable enough for filtering, analytics, and translation.

## Scripted Turn Sequence

Each turn represents either the other person's prompt or the learner's model response.

Required fields:

- `sortOrder`: positive integer
- `speakerRole`: one of the scenario roles
- `baseText`: German text
- `primaryMeaning`: meaning in the learner's primary meaning language
- `secondaryMeaning`: optional meaning in the learner's secondary meaning language

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

- `text`: German answer text
- `isCorrect`: boolean
- `feedback`: static feedback shown after selection

At least one choice must be correct when choices are provided for a turn.

## Static Feedback

Static feedback is deterministic content authored with the package.

Supported fields:

- `turnSortOrder`: optional turn reference
- `feedbackType`: `model_answer`, `politeness_note`, `common_mistake`, or `pronunciation_prompt`
- `text`: learner-facing feedback

MVP feedback must be short and must not pretend to evaluate an unsent free-text answer.

## AI Boundary

AI-assisted roleplay feedback is outside the MVP.

Before AI feedback is added, the project must define:

- cost controls
- abuse and safety rules
- data retention rules
- entitlement boundaries
- fallback behavior when AI is unavailable

Until those boundaries exist, Web roleplay remains scripted and static.

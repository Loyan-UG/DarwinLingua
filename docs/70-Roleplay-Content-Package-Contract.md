# Roleplay Content Package Contract

## Purpose

This document defines the Phase 6 contract for scripted roleplay practice.

Roleplay is an MVP practice layer on top of dialogue practice lessons. It must work without AI, without chat, and without learner-generated content being sent to a model.

## Terminology

The project currently uses three related but separate roleplay concepts:

| Concept | Content owner | Storage/import shape | Web route | Purpose |
| --- | --- | --- | --- | --- |
| Dialogue-derived Roleplay | `DialogueLesson` | No standalone package. It is derived at render time from `dialogueTurns`. | `/dialogues/{slug}/roleplay` | Quick scripted practice for an existing Dialogue. |
| Event Preparation `roleplayPrompts` | `EventPreparationPack` | String prompts inside `eventPreparationPacks[].roleplayPrompts`. | Event Preparation surfaces only. | Lightweight event-preparation instructions. |
| Standalone `RoleplayScenario` | `RoleplayScenario` | Top-level `roleplayScenarios` package array. | `/roleplays` and `/roleplays/{slug}` | Reusable deterministic roleplay content with roles, turns, answer choices, static feedback, and optional image slots. |

`RoleplayScenario` is not a rename of the older `ScenarioLesson` model. `ScenarioLesson` was renamed to `DialogueLesson` in the content model. Standalone `RoleplayScenario` is a separate scripted practice model that may link to a Dialogue but is stored, imported, searched, and rendered independently.

## Current Implementation Status

Last updated: 2026-05-25.

Standalone `RoleplayScenario` infrastructure has a Web-first pilot-ready implementation slice: parser support, application validation, shared-content persistence, repository/query services, Web API list/detail routes, Web list/detail rendering, Unified Learning Search result support, and admin count visibility. Dedicated import, validation, repository, Web/API structural, Unified Search, admin-report, build, shared-database import, and local smoke checks pass for the first pilot package.

Current supported behavior:

- Dialogue content can be used as source material for future standalone roleplay scenarios.
- Event Preparation Packs can include simple `roleplayPrompts`.
- The Web MVP derives Dialogue-roleplay practice directly from existing Dialogue turns at `/dialogues/{slug}/roleplay`; this does not create or import a `RoleplayScenario`.
- Standalone `RoleplayScenario` list/detail routes exist and the first reviewed pilot package has been generated, imported into the shared development database, and smoke-tested locally.

Current content inventory:

- Standalone `roleplayScenarios`: 1 pilot package file at `content/learning-portal/roleplays/packages/roleplays-a1-b2-pilot-v1.json`, with 10 A1-B2 scenarios.
- Event Preparation `roleplayPrompts`: 16 prompts in `content/generated/conversation-support/conversation-support-baseline-v1.json`.
- Dialogue `promptType: "roleplay-task"` and `skillFocus: "roleplay"`: present across generated Dialogue packages; these are Dialogue metadata and prompts, not standalone roleplay content.

Closed before the reviewed pilot package was generated:

- dedicated application import tests for valid/invalid `roleplayScenarios`
- repository/API/Web structural tests for list/detail filters and image-slot rendering
- Unified Search test coverage for the `roleplay` result type
- admin report count visibility tests for RoleplayScenario content
- local import and Web/API/search smoke against the shared development database
- admin report endpoint authentication behavior verified; service-level admin report tests cover count visibility

Do not generate bulk standalone `RoleplayScenario` packages. Further roleplay content must remain small-batch, reviewed, imported, and smoke-tested with the same gates.

## `RoleplayScenario`

A `RoleplayScenario` is a structured deterministic practice flow. It may link to one `DialogueLesson`, but it must not duplicate full Dialogue lessons unnecessarily.

Top-level package array:

- `roleplayScenarios`

Required fields:

- `slug`: stable lowercase identifier
- `linkedDialogueSlug`: optional existing `DialogueLesson.slug`
  - Compatibility alias: packages may use `scenarioSlug`; the importer maps it to `linkedDialogueSlug`.
- `title`: learner-facing title
- `titleTranslations`: learner-language title translations for active meaning languages
- `description`: learner-facing summary
- `descriptionTranslations`: learner-language summary translations for active meaning languages
- `learnerGoal`: concrete goal for the learner
- `learnerGoalTranslations`: learner-language goal translations for active meaning languages
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
- `answerChoices`: guided learner choice moments. The current parser accepts an empty array, but reviewed pilot packages must include answer-choice moments.
- `staticFeedback`: deterministic feedback blocks shown without AI
- `imageSlots`: optional image placeholders
- `isPublished`: boolean
- `sortOrder`: non-negative integer

The source `title`, `description`, and `learnerGoal` fields are German-first learner content. Official packages must also include `titleTranslations`, `descriptionTranslations`, and `learnerGoalTranslations` for active learner meaning languages. Web rendering shows the German source text and the learner's selected meaning-language translation; it must not fall back to English when the learner selected another language.

Do not duplicate a full Dialogue lesson into a `RoleplayScenario`. Link to the Dialogue with `linkedDialogueSlug` when the Dialogue is source material, and author only the extra deterministic practice flow needed for the roleplay.

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

`answerChoices` support guided practice without free-text evaluation. They are optional in the current parser, but official standalone pilot packages should include answer-choice moments so the runner can be validated end to end.

In the Web learner view, the scripted learner turn already shows the recommended answer. Answer choices should therefore include a correct deterministic choice for validation, but learner-facing UI should emphasize weaker or not-recommended alternatives and their static feedback instead of repeating the recommended answer below the model response.

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

At least one choice must be correct when choices are provided for a turn. Incorrect choices must be plausible but insufficient for the scenario: too short, missing a reason, too direct for the register, unclear, or incomplete. Do not use irrelevant filler such as `Ich weiß nicht.` as the default incorrect option.

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

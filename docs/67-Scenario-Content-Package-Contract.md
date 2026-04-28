# Scenario Content Package Contract

This document defines the Phase 6 JSON contract for scenario lessons before persistence and UI work.

## Package Shape

Scenario lessons live beside vocabulary entries and collections:

```json
{
  "packageVersion": "1.0",
  "packageId": "a1-practical-scenarios",
  "packageName": "A1 Practical Scenarios",
  "source": "Hybrid",
  "defaultMeaningLanguages": ["en", "fa"],
  "entries": [],
  "scenarios": []
}
```

`entries` remains required for the current importer. Scenario persistence will be added separately.

## ScenarioLesson

Required fields:

- `slug`: stable lowercase kebab-case identifier.
- `title`: learner-facing title.
- `description`: short explanation of the situation.
- `learnerGoal`: concrete outcome the learner should be able to perform.
- `cefrLevel`: one of `A1`, `A2`, `B1`, `B2`, `C1`, `C2`.
- `category`: stable lowercase kebab-case scenario category.
- `topics`: one or more existing catalog topic keys.
- `dialogueTurns`: ordered dialogue turns.
- `usefulPhrases`: reusable phrases for the scenario.
- `questions`: comprehension or preparation questions.

Optional fields:

- `sortOrder`: non-negative display order.

## ScenarioDialogueTurn

Required fields:

- `speakerRole`: stable lowercase kebab-case role such as `learner`, `doctor`, `teacher`, `colleague`, `organizer`, or `partner`.
- `baseText`: German text shown in the scenario.
- `translations`: one or more meaning-language translations.

## ScenarioPhrase

Required fields:

- `baseText`: German phrase.
- `translations`: one or more meaning-language translations.

Optional fields:

- `usageNote`: short note describing when to use the phrase.

## ScenarioQuestion

Required fields:

- `prompt`: German question prompt.
- `translations`: one or more meaning-language prompt translations.
- `answers`: at least two selectable answers.

## ScenarioAnswer

Required fields:

- `text`: German answer text.
- `translations`: one or more meaning-language answer translations.
- `isCorrect`: `true` for the correct answer.

Optional fields:

- `feedback`: short static feedback. AI-generated feedback is not part of the MVP contract.

## CEFR And Topic Rules

- A scenario has exactly one primary CEFR level.
- Scenario language is German by convention; embedded German text is stored as `baseText`, `prompt`, or `text`.
- Scenario `topics` must reference active catalog topic keys.
- A1/A2 scenarios should use short dialogue turns and one clear learner goal.
- B1+ scenarios may include longer turns, more nuanced roles, and more distractor answers.

## Import Validation Rules

The importer should reject scenario content when:

- `slug`, `title`, `learnerGoal`, `cefrLevel`, `category`, or `topics` is missing.
- `slug`, `category`, or `speakerRole` is not lowercase kebab-case.
- `cefrLevel` is not a supported CEFR value.
- a referenced topic key is unknown.
- a dialogue turn, phrase, question, or answer is missing German base text.
- translations are empty, unsupported, duplicated by language, or missing text.
- a question has fewer than two answers.
- a question has zero correct answers or more than one correct answer.

Incomplete dual-language coverage should be reported as a warning first, matching vocabulary import behavior.

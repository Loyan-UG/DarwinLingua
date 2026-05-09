# Dialogue Content Package Contract

This document defines the Phase 6 JSON contract for dialogue lessons before persistence and UI work.

## Package Shape

Dialogue lessons live beside vocabulary entries and collections:

```json
{
  "packageVersion": "1.0",
  "packageId": "a1-practical-dialogues",
  "packageName": "A1 Practical Dialogues",
  "source": "Hybrid",
  "defaultMeaningLanguages": ["en", "fa"],
  "entries": [],
  "dialogues": []
}
```

`entries` remains required for the current importer. Dialogue persistence will be added separately.

## DialogueLesson

Required fields:

- `slug`: stable lowercase kebab-case identifier.
- `title`: learner-facing title.
- `description`: short explanation of the situation.
- `learnerGoal`: concrete outcome the learner should be able to perform.
- `cefrLevel`: one of `A1`, `A2`, `B1`, `B2`, `C1`, `C2`.
- `category`: stable lowercase kebab-case dialogue category.
- `topics`: one or more existing catalog topic keys.
- `dialogueTurns`: ordered dialogue turns.
- `usefulPhrases`: reusable phrases for the dialogue.
- `questions`: comprehension or preparation questions.

Optional fields:

- `sortOrder`: non-negative display order.

## DialogueTurn

Required fields:

- `speakerRole`: stable lowercase kebab-case role such as `learner`, `doctor`, `teacher`, `colleague`, `organizer`, or `partner`.
- `baseText`: German text shown in the dialogue.
- `translations`: one or more meaning-language translations.

## DialoguePhrase

Required fields:

- `baseText`: German phrase.
- `translations`: one or more meaning-language translations.

Optional fields:

- `usageNote`: short note describing when to use the phrase.

## DialogueQuestion

Required fields:

- `prompt`: German question prompt.
- `translations`: one or more meaning-language prompt translations.
- `answers`: at least two selectable answers.

## DialogueAnswer

Required fields:

- `text`: German answer text.
- `translations`: one or more meaning-language answer translations.
- `isCorrect`: `true` for the correct answer.

Optional fields:

- `feedback`: short static feedback. AI-generated feedback is not part of the MVP contract.

## CEFR And Topic Rules

- A dialogue has exactly one primary CEFR level.
- Dialogue language is German by convention; embedded German text is stored as `baseText`, `prompt`, or `text`.
- Dialogue `topics` must reference active catalog topic keys.
- A1/A2 dialogues should use short dialogue turns and one clear learner goal.
- B1+ dialogues may include longer turns, more nuanced roles, and more distractor answers.

## Import Validation Rules

The importer should reject dialogue content when:

- `slug`, `title`, `learnerGoal`, `cefrLevel`, `category`, or `topics` is missing.
- `slug`, `category`, or `speakerRole` is not lowercase kebab-case.
- `cefrLevel` is not a supported CEFR value.
- a referenced topic key is unknown.
- a dialogue turn, phrase, question, or answer is missing German base text.
- translations are empty, unsupported, duplicated by language, or missing text.
- a question has fewer than two answers.
- a question has zero correct answers or more than one correct answer.

Incomplete dual-language coverage should be reported as a warning first, matching vocabulary import behavior.

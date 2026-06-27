# Dialogue Content Package Contract

This document defines the JSON contract for Dialogue lessons. Dialogues are role-based practical conversations for real-life tasks and exam preparation. They are different from Talk Topics, which are reading-based discussion items.

Dialogue packages may include roleplay-related metadata, but a Dialogue is not a standalone RoleplayScenario. `skillFocus: "roleplay"` and speaking prompts with `promptType: "roleplay-task"` mark a Dialogue as useful for roleplay-style practice. The Web can derive a simple practice page from `dialogueTurns` at `/dialogues/{slug}/roleplay`. Standalone scripted roleplays live in top-level `roleplayScenarios` packages and are governed by `docs/70-Roleplay-Content-Package-Contract.md`.

## Package Target Language

Every import package must declare package-level `targetLearningLanguageCode`. Current official German-learning packages use `"de"`.

`targetLearningLanguageCode` is the language being taught. It is separate from `defaultMeaningLanguages` and from all `...Translations` fields, which remain helper/meaning languages for learner support.

Import validation accepts only content-importable target learning languages: public-active languages plus explicitly approved pilot/staging languages. Current reviewed imports may use German (`de`) and pilot English (`en`); planned languages such as Spanish (`es`) and French (`fr`) must be rejected until their readiness gates are complete.

Levelled packages must declare `levelSystemCode`; current German packages use CEFR (`"cefr"`). Import validation rejects missing `levelSystemCode`, unsupported level systems, and non-content-importable target-learning languages before content is persisted.

All source fields must be authored natively in the package target language. Future English, Spanish, or French dialogue packages must be new source content for those languages, not translated copies of German dialogues.

## Package Shape

Dialogue lessons live beside vocabulary entries and collections:

```json
{
  "packageVersion": "1.0",
  "packageId": "a1-practical-dialogues",
  "packageName": "A1 Practical Dialogues",
  "targetLearningLanguageCode": "de",
  "levelSystemCode": "cefr",
  "source": "Hybrid",
  "defaultMeaningLanguages": ["en", "fa"],
  "entries": [],
  "dialogues": []
}
```

`entries` remains required for the current importer and may be empty for dialogue-only packages.

## DialogueLesson

Required fields:

- `slug`: stable lowercase kebab-case identifier.
- `title`: learner-facing title.
- `description`: short explanation of the situation.
- `learnerGoal`: concrete outcome the learner should be able to perform.
- `cefrLevel`: one of `A1`, `A2`, `B1`, `B2`, `C1`, `C2`.
- `category`: stable lowercase kebab-case dialogue category.
- `topics`: one or more existing catalog topic keys.
- `examProfiles`: one or more exam/profile tags such as `goethe-b1`, `telc-b2`, `dtz-a2-b1`, or `berufssprache-b2`. Use ASCII key `oeso-*` for ÖSD display labels.
- `skillFocus`: one or more skill tags such as `speaking`, `roleplay`, `exam-speaking`, `phone-call`, `workplace-communication`, `discussion`, or `negotiation`.
- `taskType`: one main task type, for example `reschedule-appointment`, `explain-problem`, `complain-politely`, `workplace-meeting`, `job-interview`, or `exam-discussion`.
- `interactionMode`: one interaction setting, for example `face-to-face`, `phone`, `workplace`, `doctor-office`, `government-office`, `exam-room`, or `group-work`.
- `register`: `formal`, `informal`, `neutral`, or `mixed`.
- `speakingFunctions`: communicative functions such as `greet`, `request`, `explain`, `clarify`, `agree`, `disagree`, `negotiate`, `summarize`, and `close-conversation`.
- `usefulWords`: ordered Word Catalog references only; meanings are resolved from the Word Catalog and are not duplicated in Dialogue payloads.
- `speakingPrompts`: ordered speaking or exam prompts.
- `estimatedPracticeMinutes`: expected practice time.
- `dialogueTurns`: ordered dialogue turns.
- `usefulPhrases`: reusable phrases for the dialogue.
- `questions`: comprehension or preparation questions.

Optional fields:

- `sortOrder`: non-negative display order.
- `difficultyNote`: concise learner-facing or internal difficulty note.
- `examRelevance`: concise note explaining exam usefulness.

## Useful Words

Required fields:

- `lemma`: German lemma or phrase.
- `sortOrder`: display order.

Optional fields:

- `wordSlug`: preferred when the word exists in the Word Catalog.
- `cefrLevel`: useful disambiguation.

Useful words must not contain meanings or translations.

## Speaking Prompts

Required fields:

- `promptType`: one of `comprehension`, `speaking-prompt`, `roleplay-task`, `exam-prompt`, `follow-up-question`, `self-correction`, or `vocabulary-check`.
- `prompt`: German prompt.
- `translations`: meaning-language translations when the current dialogue contract requires learner-facing prompt translations.
- `sortOrder`: display order.

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
- `cefrLevel` is first-class and directly filterable in API and Web.
- Dialogue language is German by convention; embedded German text is stored as `baseText`, `prompt`, or `text`.
- Dialogue `topics` must reference active catalog topic keys.
- A1/A2 dialogues should use short dialogue turns and one clear learner goal.
- B1/B2 are priority levels for exam-oriented generation.
- B1+ dialogues may include longer turns, more nuanced roles, speaking prompts, and more distractor answers.
- Minimum learner-side and partner-side sentence counts are A1: 5, A2: 6, B1: 7, B2: 8, C1: 9, C2: 10.

## Import Validation Rules

The importer should reject dialogue content when:

- `slug`, `title`, `learnerGoal`, `cefrLevel`, `category`, or `topics` is missing.
- `slug`, `category`, or `speakerRole` is not lowercase kebab-case.
- `cefrLevel` is not a supported CEFR value.
- exam profiles, skill focus tags, task type, interaction mode, register, prompt type, or speaking functions are unsupported.
- a referenced topic key is unknown.
- useful words are missing or store anything beyond references.
- speaking prompts are missing or invalid.
- learner-side or partner-side sentence counts are below the CEFR minimum.
- a dialogue turn, phrase, question, or answer is missing German base text.
- translations are empty, unsupported, duplicated by language, or missing text.
- a question has fewer than two answers.
- a question has zero correct answers or more than one correct answer.

Incomplete dual-language coverage should be reported as a warning first, matching vocabulary import behavior.

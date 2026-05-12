# Grammar Content Package Contract

This document defines the Phase 7 Grammar Guide package contract. Grammar content is dynamic and importable; Razor views must render imported `GrammarTopic` data instead of hardcoded lessons.

Reference roadmap: `76-Learning-Portal-Roadmap-And-Backlog.md`.

## Package Location

Grammar topics are included in the shared content package root under `grammarTopics`.

```json
{
  "packageVersion": "1.0",
  "packageId": "grammar-a1-starter-v1",
  "packageName": "Grammar A1 Starter",
  "entries": [],
  "grammarTopics": []
}
```

## GrammarTopic Fields

- `slug`: required lowercase kebab-case, unique inside the package.
- `title`: required display title.
- `shortDescription`: required learner-facing summary.
- `cefrLevel`: required `A1`, `A2`, `B1`, `B2`, `C1`, or `C2`.
- `grammarCategory`: required controlled category.
- `topics`: optional Catalog topic keys; each key must already exist.
- `isPublished`: optional, defaults to `true`.
- `sortOrder`: optional numeric ordering.
- `sections`: required, at least one explanation section.
- `examples`: optional German examples with translations.
- `ruleSummaries`: optional concise rules.
- `commonMistakes`: optional wrong/correct pairs and explanation.
- `exceptionNotes`: optional exception notes.
- `linkedWords`: optional word references by lemma and optional word slug.
- `linkedDialogueSlugs`: optional dialogue slugs.
- `linkedTalkTopicSlugs`: optional Talk Topic slugs.
- `linkedExerciseSlugs`: optional future Exercise Engine slugs.
- `prerequisiteSlugs`: optional prerequisite GrammarTopic slugs.
- `relatedTopicSlugs`: optional related GrammarTopic slugs.

## Controlled Categories

`articles`, `nouns`, `gender`, `plural`, `pronouns`, `verbs`, `modal-verbs`, `tenses`, `separable-verbs`, `reflexive-verbs`, `cases`, `nominative`, `accusative`, `dative`, `genitive`, `adjective-declension`, `prepositions`, `word-order`, `subordinate-clauses`, `connectors`, `negation`, `questions`, `imperative`, `passive`, `konjunktiv`, `reported-speech`, `punctuation`.

## Localized Text

Sections store a base explanation plus optional learner-language translations:

```json
{
  "heading": "When to use definite articles",
  "explanation": "Use definite articles before known nouns.",
  "translations": [
    { "language": "en", "heading": "When to use definite articles", "text": "Use definite articles before known nouns." }
  ],
  "sortOrder": 10
}
```

Examples require German text and may include learner-language translations:

```json
{
  "germanText": "Der Kaffee ist heiss.",
  "translations": [
    { "language": "en", "text": "The coffee is hot." }
  ],
  "sortOrder": 10
}
```

If the requested learner language is missing, Web rendering falls back safely to English or the base text when available.

## Validation Rules

- Slugs and linked slugs must be lowercase kebab-case.
- `title`, `shortDescription`, `cefrLevel`, and `grammarCategory` are required.
- `grammarCategory` must be one of the controlled categories.
- `sections` must contain at least one item.
- Examples require `germanText`.
- Translation language codes must be active meaning languages.
- Duplicate translation language codes are rejected per translated owner.
- Unknown Catalog topic keys fail import.
- Future exercise slugs are stored as safe references until the Exercise Engine exists.

## Generation Rule

Do not generate bulk grammar content until the implementation, import validation, Web API, and Web rendering have been validated with a small reviewed package.

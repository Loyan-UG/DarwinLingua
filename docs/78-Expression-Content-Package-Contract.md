# Expression Content Package Contract

## Purpose

This document defines the JSON/import contract for the Phase 7 Everyday Expressions module.

Everyday Expressions are dynamic, importable learning content. They must not be stored as WordEntry-only records, and they must not duplicate Word meanings. They may link to Words by lemma or slug when useful.

The roadmap source of truth remains `76-Learning-Portal-Roadmap-And-Backlog.md`.

## Package Shape

Expression entries are imported through the existing content package format under `expressionEntries`.

```json
{
  "packageVersion": "1.0",
  "packageId": "expressions-a2-sample",
  "packageName": "Expressions A2 Sample",
  "defaultMeaningLanguages": ["en"],
  "entries": [],
  "expressionEntries": []
}
```

## ExpressionEntry Fields

Required:

- `slug`: lowercase kebab-case unique key
- `expressionText`: German expression text
- `actualMeaningText`: learner-facing actual meaning summary
- `cefrLevel`: valid CEFR level
- `expressionType`: controlled expression type
- `register`: controlled register
- `category`: lowercase kebab-case category/context key

Optional:

- `literalMeaningText`
- `usageExplanation`
- `region`
- `isRisky`
- `topics`
- `isPublished`
- `sortOrder`
- `meanings`
- `examples`
- `warnings`
- `linkedWords`
- `relatedExpressionSlugs`
- `linkedExerciseSlugs`

## Controlled Values

Expression types:

- `idiom`
- `colloquial-phrase`
- `proverb`
- `fixed-expression`
- `slang`
- `cultural-phrase`
- `false-friend`
- `regional-expression`
- `polite-formula`
- `warning-phrase`

Registers:

- `formal`
- `informal`
- `neutral`
- `colloquial`
- `slang`
- `rude`
- `polite`
- `workplace-safe`
- `friends-only`
- `regional`

## Localization

`meanings` provide learner-language actual meaning, literal meaning, and usage explanation variants.

Example and warning translations use the standard `{ "language": "...", "text": "..." }` shape.

Supported language codes must already be active platform meaning languages. Duplicate translations for the same language are rejected.

## Safety And Tone

Expressions that are risky, rude, slang-heavy, friends-only, warning phrases, discriminatory, sexual, highly political, or easy to misuse must include warning metadata.

The importer rejects risky expressions that do not include at least one warning with text.

## Linking Rules

`linkedWords` may store:

- `lemma`
- `wordSlug`
- `sortOrder`

They must not include word definitions or copied word meanings.

`relatedExpressionSlugs` and `linkedExerciseSlugs` are stored as slugs. Unresolved exercise links are allowed as warnings until the Exercise Engine exists.

## Minimal Example

```json
{
  "slug": "a2-alles-klar",
  "expressionText": "Alles klar.",
  "literalMeaningText": "Everything clear.",
  "actualMeaningText": "All good or understood.",
  "usageExplanation": "Used to confirm understanding or agreement.",
  "cefrLevel": "A2",
  "expressionType": "fixed-expression",
  "register": "neutral",
  "category": "daily-life",
  "topics": ["daily-life"],
  "isPublished": true,
  "sortOrder": 10,
  "meanings": [
    {
      "language": "en",
      "actualMeaningText": "All good or understood.",
      "literalMeaningText": "Everything clear.",
      "usageExplanation": "A neutral confirmation phrase."
    }
  ],
  "examples": [
    {
      "germanText": "Alles klar, wir treffen uns um acht.",
      "translations": [
        { "language": "en", "text": "All good, we meet at eight." }
      ],
      "sortOrder": 10
    }
  ],
  "linkedWords": [
    { "lemma": "klar", "wordSlug": "klar", "sortOrder": 10 }
  ],
  "relatedExpressionSlugs": ["verstanden"],
  "linkedExerciseSlugs": ["a2-confirmation-phrases"]
}
```

## Validation Summary

- slug required and kebab-case
- expression text required
- actual meaning required
- CEFR level valid
- expression type valid
- register valid
- category required and kebab-case
- examples require German text
- linked words store only lemma/slug references, not meanings
- duplicate translation language codes rejected
- unsupported translation language codes rejected
- risky expressions require warning text

## Content Generation Rule

Bulk expression content generation must not start until implementation, validation, Web API, Web rendering, admin visibility, and release tests are stable.

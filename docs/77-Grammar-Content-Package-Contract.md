# Grammar Content Package Contract

This document defines the Phase 7 Grammar Guide package contract. Grammar content is dynamic and importable; Razor views must render imported `GrammarTopic` data instead of hardcoded lessons.

Reference roadmap: `76-Learning-Portal-Roadmap-And-Backlog.md`.

Content-generation lessons and recurring prompt-quality rules are tracked in `84-Content-Generation-Lessons-Learned.md`.

## Package Location

Grammar topics are included in the shared content package root under `grammarTopics`.

The first official A1 package is `content/learning-portal/grammar/packages/grammar-a1-core-v1.json`. Temporary pilot and validation packages should stay outside the official package path, for example under `content/learning-portal/grammar/packages/archive/validation/`.

```json
{
  "schema": "darwin-lingua-grammar-content-v2-proposed",
  "packageVersion": "1.0",
  "packageId": "grammar-a1-starter-v1",
  "packageName": "Grammar A1 Starter",
  "source": "manually authored",
  "targetLanguages": ["en", "fa", "ar", "tr", "ru", "ckb", "kmr", "pl", "ro", "sq"],
  "upsertMode": "by-slug",
  "entries": [],
  "grammarTopics": []
}
```

The legacy package shape with `defaultMeaningLanguages`, base `sections[].heading`, base `sections[].explanation`, and array-based `translations` remains supported for backward compatibility. Official newly generated Grammar Guide content must include section `translations` for every target language so the section navigation and section headings render in the selected content language instead of falling back to the base English `heading`.

## GrammarTopic Fields

- `slug`: required lowercase kebab-case, unique inside the package.
- `contentRevision`: optional integer used for diagnostics; `slug` remains the import identity.
- `title`: required display title.
- `titleLocalized`: optional object keyed by learner language code.
- `shortDescription`: required learner-facing summary.
- `shortDescriptionLocalized`: optional object keyed by learner language code.
- `cefrLevel`: required `A1`, `A2`, `B1`, `B2`, `C1`, or `C2`.
- `grammarCategory`: required controlled category.
- `topics`: optional Catalog topic keys; each key must already exist.
- `isPublished`: optional, defaults to `true`.
- `sortOrder`: optional numeric ordering.
- `sections`: required, at least one explanation section.
- `examples`: optional German examples with translations.
- `ruleSummaries`: optional concise rules. Each item may use base `text`, legacy `translations`, or `localizedText` keyed by learner language code.
- `commonMistakes`: optional wrong/correct pairs and explanation.
- `exceptionNotes`: optional exception notes.
- `linkedWords`: optional word references by lemma and optional word slug.
- `linkedDialogueSlugs`: optional dialogue slugs.
- `linkedTalkTopicSlugs`: optional Talk Topic slugs.
- `linkedExerciseSlugs`: optional future Exercise Engine slugs.
- `prerequisiteSlugs`: optional prerequisite GrammarTopic slugs.
- `relatedTopicSlugs`: optional related GrammarTopic slugs.
- `imageSlots`: optional image asset references for future/available visual assets.

Import uses upsert-by-slug behavior for grammar topics. Re-importing a package that contains an existing `slug` replaces the previous stored topic graph for that slug and does not create duplicate topics. Grammar-only packages may be re-imported with the same source `packageId`; the import audit record receives a generated `-reimport-` package id while the grammar topic identity remains the stable `slug`.

## Controlled Categories

`articles`, `nouns`, `gender`, `plural`, `pronouns`, `verbs`, `modal-verbs`, `tenses`, `separable-verbs`, `reflexive-verbs`, `cases`, `nominative`, `accusative`, `dative`, `genitive`, `adjective-declension`, `prepositions`, `word-order`, `subordinate-clauses`, `connectors`, `negation`, `questions`, `imperative`, `passive`, `konjunktiv`, `reported-speech`, `punctuation`.

## Rich Localized Sections

Sections may use the legacy base explanation shape, or the rich localized block shape. Rich sections require `sectionKey`, `sortOrder`, `localizedBlocks`, and localized section `translations` for official content.

```json
{
  "sectionKey": "core-table",
  "sortOrder": 20,
  "heading": "Core table",
  "explanation": "Base English fallback.",
  "translations": [
    {
      "language": "fa",
      "heading": "جدول اصلی",
      "text": "متن توضیحی فارسی برای همین بخش."
    }
  ],
  "localizedBlocks": {
    "en": [
      {
        "type": "table",
        "caption": "Basic subject pronouns",
        "columns": ["German", "Use"],
        "rows": [["ich", "I"]]
      }
    ]
  }
}
```

Supported block types:

- `paragraph`: requires `text`.
- `table`: requires `caption`, `columns`, and `rows`.
- `callout`: requires `style` and `text`.
- `rule-list`: requires `items`.
- `example-list`: requires `items`.
- `mistake-pair`: requires `wrong` and `correct`.
- `image-slot`: requires `assetKey` or `imageSlotKey`.

Unknown block types fail validation. Rendering must never emit raw untrusted HTML; Web and future MAUI render only known structured block fields.

Examples require German text and may include learner-language translations:

```json
{
  "germanText": "Der Kaffee ist heiss.",
  "translations": {
    "en": "The coffee is hot."
  },
  "sortOrder": 10
}
```

The legacy array translation shape remains supported.

If the requested learner language is missing, Web rendering falls back safely to English or the base text when available.

## Image Slots

Image slots may reference expected assets under:

`content/learning-portal/grammar/assets/{topicSlug}/{imageFileName}`

Missing image assets must not fail import or break rendering. The imported content stores image references only; binary/image generation is handled separately.

## Validation Rules

- Slugs and linked slugs must be lowercase kebab-case.
- `title`, `shortDescription`, `cefrLevel`, and `grammarCategory` are required.
- `grammarCategory` must be one of the controlled categories.
- `sections` must contain at least one item.
- Rich sections require `sectionKey`.
- A topic cannot contain duplicate rich `sectionKey` values; localized section identity is `topic slug + sectionKey + languageCode`.
- `localizedBlocks` language codes must be active meaning languages.
- Rich block JSON must match the supported block contract.
- Examples require `germanText`.
- Translation language codes must be active meaning languages.
- Duplicate translation language codes are rejected per translated owner.
- Unknown Catalog topic keys are reported as warnings and skipped as taxonomy links.
- Linked words store only lemma/slug references and must not duplicate word meanings.
- Future exercise slugs are stored as safe references until the Exercise Engine exists.

## Generation Rule

Do not generate bulk grammar content until the implementation, import validation, Web API, and Web rendering have been validated with a small reviewed package.

Before any grammar regeneration pass, run the grammar prompt audit/sync tool and review `84-Content-Generation-Lessons-Learned.md`.

# Darwin Lingua Grammar Content Plan

Target repository root:

```text
content/learning-portal/grammar/
```

Recommended structure:

```text
content/learning-portal/grammar/
├─ syllabus/
│  ├─ grammar-syllabus-a1-c2-v1.json
│  └─ grammar-syllabus-a1-c2-v1.md
├─ packages/
│  ├─ grammar-a1-core-v1.json
│  ├─ grammar-a2-core-v1.json
│  ├─ grammar-b1-core-v1.json
│  ├─ grammar-b2-core-v1.json
│  ├─ grammar-c1-core-v1.json
│  └─ grammar-c2-core-v1.json
├─ image-prompts/
│  └─ <grammar-topic-slug>.images.md
└─ assets/
   └─ <grammar-topic-slug>/
      ├─ image-01.png
      └─ image-02.png
```

## Upsert rule

Use the stable `slug` / `contentKey` as the primary content identity.

If a grammar topic with the same slug already exists, Codex/importer should update it rather than create a duplicate.

For localized content, use:

```text
topic slug + section key + language code
```

as the stable identity for localized sections.

## Localization rule

Grammar explanations must be localized and adapted per language, not merely translated.

Target languages:

- English: `en`
- Persian: `fa`
- Arabic: `ar`
- Turkish: `tr`
- Russian: `ru`
- Kurdish Sorani: `ckb`
- Kurdish Kurmanji: `kmr`
- Polish: `pl`
- Romanian: `ro`
- Albanian: `sq`

## Rich content rule

Prefer structured rich-content blocks over unsafe free HTML.

Supported future block types should include:

- `paragraph`
- `table`
- `example-list`
- `callout`
- `mistake-pair`
- `image-slot`
- `rule-list`

The renderer can convert these blocks to safe HTML for Web and to native/HTML-like rendering for MAUI.

# Content Package Format

## Purpose

This document defines the official Phase 1 content package format used for importing vocabulary data into Darwin Deutsch.

The format is intended for:

- manually authored content
- AI-assisted content
- hybrid editorial workflows
- future automated validation tooling

---

## 1. Official Format Choice

The official import format for Phase 1 is `JSON`.

Reasons:

- strict and structured
- easy to validate
- easy to parse in .NET
- well suited to machine-assisted generation
- easy to version over time

YAML may be supported later as an authoring convenience, but JSON remains the canonical import format.

---

## 2. Format Design Goals

The package format should be:

- explicit
- predictable
- easy to validate
- suitable for batch import
- traceable to a package identity
- stable enough for future tooling

---

## 3. Package-Level Structure

Each package must contain:

- package metadata
- default meaning-language metadata
- an array of vocabulary entries
- an optional array of curated collections

### 3.1 Required Package Fields

- `packageVersion`
- `packageId`
- `packageName`
- `entries`

### 3.2 Recommended Package Fields

- `source`
- `defaultMeaningLanguages`
- `collections`
- `notes`

---

## 4. Entry-Level Structure

Each vocabulary entry should contain:

- German word data
- normalization-friendly fields
- CEFR information
- part-of-speech information
- optional lexical-form blocks for multi-role words
- topic keys
- meaning translations
- example sentences

### 4.1 Required Entry Fields

- `word`
- `language`
- `cefrLevel`
- either `partOfSpeech` or `lexicalForms`
- `topics`
- `meanings`
- `examples`

### 4.2 Recommended Optional Fields

- `normalizedWord`
- `article`
- `plural`
- `infinitive`
- `pronunciationIpa`
- `syllableBreak`
- `lexicalForms`
- `usageLabels`
- `contextLabels`
- `grammarNotes`
- `collocations`
- `wordFamilies`
- `relations`
- `notes`

---

## 5. Meaning Structure

Each meaning item should contain:

- `language`
- `text`

Duplicate meaning languages inside the same entry are not allowed.

---

## 5.1 Lexical Form Structure

Use `lexicalForms` when one lemma can appear in more than one lexical role.

Each item may contain:

- `partOfSpeech` required
- `article` optional
- `plural` optional
- `infinitive` optional
- `isPrimary` optional

Rules:

- if `lexicalForms` is omitted, the importer uses the top-level lexical fields
- if `lexicalForms` is present, it must contain at least one valid item
- each `partOfSpeech` may appear only once inside one entry
- at most one item may declare `isPrimary = true`
- if no item declares `isPrimary = true`, the first item becomes primary
- if top-level lexical fields are also present, they must match the primary lexical form

For AI-generated files, prefer always writing `lexicalForms`, even for single-role words.

---

## 6. Example Structure

Each example item should contain:

- `baseText`
- optional `translations`

Each example translation should contain:

- `language`
- `text`

Duplicate translation languages inside the same example are not allowed.

---

## 7. Suggested File Naming

Examples:

- `a1-basic-home-pack-01.json`
- `a1-travel-fa-en-001.json`
- `b1-workplace-words-2026-01.json`
- `mixed-doctor-topic-pack.json`

File naming should be readable and stable, but the real identity is the `packageId`.

---

## 7.1 Collection Structure

Packages may include an optional top-level `collections` array.

Each collection may contain:

- `slug` required
- `name` required
- `description` optional
- `imageUrl` optional
- `sortOrder` optional
- `words` required

Each `words` item may contain:

- `word` required
- `partOfSpeech` optional but strongly recommended
- `cefrLevel` optional but recommended for ambiguous lemmas

Rules:

- `slug` must use lowercase kebab-case
- collection slugs must be unique inside one package
- each collection must contain at least one word reference
- each word reference should resolve to exactly one active word after the current package import is considered
- if a lemma can map to more than one word, add `partOfSpeech` or `cefrLevel`
- collection import is curation-oriented, so an existing collection may be updated predictably by `slug`

Example:

```json
{
  "collections": [
    {
      "slug": "book-a-unit-03",
      "name": "Book A Unit 03",
      "description": "Vocabulary for the third unit of Book A.",
      "imageUrl": "/images/collections/book-a-unit-03.svg",
      "sortOrder": 30,
      "words": [
        { "word": "Aufgabe", "partOfSpeech": "Noun", "cefrLevel": "B1" },
        { "word": "Anforderung", "partOfSpeech": "Noun", "cefrLevel": "B2" }
      ]
    }
  ]
}
```

---

## 8. Root JSON Shape

```json
{
  "packageVersion": "1.0",
  "packageId": "a1-travel-fa-en-001",
  "packageName": "A1 Travel Starter Pack",
  "source": "Hybrid",
  "defaultMeaningLanguages": ["fa", "en"],
  "entries": [],
  "collections": []
}
```

---

## 9. Entry Example

See:

- `13-Entry-Structure.json`

That file should remain a realistic example of one vocabulary entry.

For new JSON packages, the recommended canonical shape is:

- top-level lexical fields for compatibility
- `lexicalForms` as the explicit lexical-role structure
- `pronunciationIpa` and `syllableBreak` when known

---

## 10. Format Rules

- Phase 1 packages must contain German source entries only.
- Controlled values such as topics and language codes must reference known reference data.
- The package format must not silently coerce invalid values.
- The package format is import-oriented, not edit-oriented.
- Merge/update semantics are not part of the Phase 1 package contract.
- Curated collection membership is the exception:
  collections may be updated predictably by `slug` because they are ordered study artifacts, not lexical-source records.

---

## 11. Versioning Direction

The package format should be versioned from the start.

Initial recommendation:

- `packageVersion = "1.0"`

If the structure changes later, validation should branch by package version rather than by ad hoc file guessing.

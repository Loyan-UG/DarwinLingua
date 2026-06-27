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
- an `entries` array, which may be empty when the package contains other content
- optional arrays of curated collections or other supported content types

A package must contain at least one content item overall. Vocabulary packages usually carry entries; collection-only packages may carry `entries: []` plus a populated `collections` array.

### 3.1 Required Package Fields

- `packageVersion`
- `packageId`
- `packageName`
- `targetLearningLanguageCode`
- `entries`

### 3.1.1 Target Learning Language

`targetLearningLanguageCode` is required for all import packages. It identifies the language being taught, not the language used to explain meanings.

Current official German-learning packages use:

```json
"targetLearningLanguageCode": "de"
```

Import validation accepts only content-importable target learning languages. A content-importable language is either public-active or explicitly opened for reviewed pilot/staging imports. As of the current multi-target checkpoint, German (`de`) is public-active, English (`en`) is pilot/content-importable, and Spanish (`es`) plus French (`fr`) remain planned and must be rejected by import until their readiness gates are complete.

`defaultMeaningLanguages` and all `...Translations` fields remain helper/meaning-language metadata. For example, a German package may teach German (`de`) while explaining the content in English, Persian, Arabic, Turkish, Russian, Central Kurdish, Northern Kurdish, Polish, Romanian, and Albanian.

Helper-language coverage can expand independently from target-language support. A future German package may still teach German while adding new helper languages beyond the current active set; that must not change the package target-language identity.

Packages that use levels may also declare `levelSystemCode`; current German content uses CEFR (`cefr`). Country Guidance packages must additionally declare `countryContextCode`, because a country guidance stream is identified by both `targetLearningLanguageCode` and `countryContextCode`, such as `de|DE`, `de|AT`, `de|CH`, `en|US`, `en|GB`, or `fr|CH`.

Source fields must be authored natively in the package target language and for that language's learning needs. Future English, Spanish, or French packages must not be bulk translations of German packages. Helper translations explain the source content for learners; they do not change the package target language.

### 3.2 Recommended Package Fields

- `source`
- `defaultMeaningLanguages`
- `levelSystemCode`
- `countryContextCode`
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

Collection-only packages are valid. Use them when all referenced words already exist in the catalog or were imported by earlier files in the same folder import. Keep `entries: []` in these packages; do not add a duplicate vocabulary entry just to satisfy the root shape.

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

Collection-only package example:

```json
{
  "packageVersion": "1.0",
  "packageId": "book-a-unit-03-collections",
  "packageName": "Book A Unit 03 Collections",
  "source": "Hybrid",
  "defaultMeaningLanguages": ["en", "fa"],
  "labels": [],
  "entries": [],
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

- Vocabulary packages must contain source entries in the package `targetLearningLanguageCode`.
- Controlled values such as topics and language codes must reference known reference data.
- The package format must not silently coerce invalid values.
- The package format is import-oriented, not edit-oriented.
- A package with no vocabulary entries is valid only when it contains another supported content item, such as a collection.
- Merge/update semantics are not part of the Phase 1 package contract.
- Curated collection membership is the exception:
  collections may be updated predictably by `slug` because they are ordered study artifacts, not lexical-source records.

---

## 11. Versioning Direction

The package format should be versioned from the start.

Initial recommendation:

- `packageVersion = "1.0"`

If the structure changes later, validation should branch by package version rather than by ad hoc file guessing.

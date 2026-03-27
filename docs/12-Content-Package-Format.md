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

### 3.1 Required Package Fields

- `packageVersion`
- `packageId`
- `packageName`
- `entries`

### 3.2 Recommended Package Fields

- `source`
- `defaultMeaningLanguages`
- `notes`

---

## 4. Entry-Level Structure

Each vocabulary entry should contain:

- German word data
- normalization-friendly fields
- CEFR information
- part-of-speech information
- topic keys
- meaning translations
- example sentences

### 4.1 Required Entry Fields

- `word`
- `language`
- `cefrLevel`
- `partOfSpeech`
- `topics`
- `meanings`
- `examples`

### 4.2 Recommended Optional Fields

- `normalizedWord`
- `article`
- `plural`
- `usageLabels`
- `contextLabels`
- `grammarNotes`
- `collocations`
- `wordFamilies`
- `notes`

---

## 5. Meaning Structure

Each meaning item should contain:

- `language`
- `text`

Duplicate meaning languages inside the same entry are not allowed.

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

## 8. Root JSON Shape

```json
{
  "packageVersion": "1.0",
  "packageId": "a1-travel-fa-en-001",
  "packageName": "A1 Travel Starter Pack",
  "source": "Hybrid",
  "defaultMeaningLanguages": ["fa", "en"],
  "entries": []
}
```

---

## 9. Entry Example

See:

- `13-Entry-Structure.json`

That file should remain a realistic example of one vocabulary entry.

---

## 10. Format Rules

- Phase 1 packages must contain German source entries only.
- Controlled values such as topics and language codes must reference known reference data.
- The package format must not silently coerce invalid values.
- The package format is import-oriented, not edit-oriented.
- Merge/update semantics are not part of the Phase 1 package contract.

---

## 11. Versioning Direction

The package format should be versioned from the start.

Initial recommendation:

- `packageVersion = "1.0"`

If the structure changes later, validation should branch by package version rather than by ad hoc file guessing.

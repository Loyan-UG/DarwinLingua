# Import Rules

## Purpose

This document defines the stable Phase 1 rules for content import.

It does not describe the full execution flow in detail. That is covered in:

- `34-Import-Workflow.md`

This document focuses on:

- what the import pipeline is allowed to do
- what it must validate
- what it must reject
- what it must defer to later phases

---

## 1. Import Purpose

The Phase 1 import tool exists to insert structured vocabulary content safely into the local database.

It is not intended to be a full content management system in Phase 1.

---

## 2. First-Version Responsibilities

The first import implementation must:

- accept a JSON content package
- parse package metadata and entries
- validate required fields
- validate controlled values
- validate language codes
- validate topic references
- validate meaning/example structure
- normalize values before duplicate detection
- detect duplicates conservatively
- skip duplicates
- insert valid content
- resolve optional curated collections
- produce an import summary

---

## 3. Duplicate Detection Rule

The first version must use a conservative duplicate rule.

Recommended duplicate identity:

- normalized German word
- primary lexical-form part of speech
- primary CEFR level

This is acceptable for Phase 1 even though it is not a perfect long-term linguistic identity model.

### Duplicate Outcome Rule

If a duplicate is found:

- skip that entry
- log a warning
- continue processing the package

A duplicate is a handled outcome, not a fatal import error.

---

## 4. Validation Rules

### 4.1 File-Level Validation

- `packageVersion` is required
- `packageId` is required
- `packageName` is required
- `entries` is required
- `entries` must not be empty
- `packageId` must be unique in the local database
- collection slugs must be unique inside one package when `collections` is present

### 4.2 Entry-Level Validation

- `word` is required
- `language` must be `de` in Phase 1
- `cefrLevel` must be valid
- `partOfSpeech` must be valid when `lexicalForms` is omitted
- `lexicalForms` must contain at least one valid item when present
- duplicate `partOfSpeech` values inside `lexicalForms` are not allowed
- top-level lexical fields must match the primary lexical form when both shapes are present
- `topics` must not be empty
- `meanings` must not be empty
- `examples` must not be empty

### 4.3 Meaning Validation

- meaning `language` is required
- meaning `text` is required
- duplicate meaning languages inside one entry are not allowed

### 4.4 Example Validation

- `baseText` is required
- example `translations` may be empty only if explicitly allowed by the current import policy
- duplicate translation languages inside one example are not allowed

### 4.5 Reference Validation

- topic keys must already exist
- language codes must already exist
- unsupported or inactive language codes must be rejected
- collection word references must resolve to exactly one active word after the current import batch is considered

---

## 5. Normalization Rules

Normalization must happen before duplicate detection and persistence.

Recommended normalization actions:

- trim word text
- generate or verify normalized lemma
- normalize lexical-form ordering and primary selection
- normalize topic keys
- normalize language-code casing
- trim meanings
- trim example text

Normalization logic must be centralized. It must not be duplicated in unrelated parts of the codebase.

---

## 6. Import Safety Rules

- import must not bypass domain integrity
- invalid entries must not leave orphan child data
- duplicate entries must not create partial records
- import must be traceable to a package identity
- import should be transactional at a sensible level
- collection metadata and entry replacement should be atomic

---

## 7. Forbidden Phase 1 Behaviors

The Phase 1 import pipeline must not:

- merge into existing entries
- patch existing translations
- patch existing examples
- auto-create unknown topics
- auto-create unknown languages
- silently coerce invalid values into fallback values
- re-import an existing `packageId`

If the data is bad, the system should show it as bad data and reject that entry.

Word-entry merge/update remains forbidden.
Curated collections may be upserted by `slug` because they are curation artifacts, not source lexical records.

---

## 8. Import Summary Requirements

The import result must report at least:

- total entries found
- inserted entries count
- skipped duplicate count
- invalid entry count
- warning count
- final status

It should also surface:

- warning list
- validation error list
- package identity

---

## 9. Future Import Modes

These are useful later but not required for the first implementation:

- validate-only mode
- dry-run mode
- update-existing mode
- package diff/review mode

The first version should support only:

- import with duplicate skip behavior

### Canonical Authoring Guidance

For new human-authored or AI-authored files:

- prefer `lexicalForms` even for single-role words
- include `pronunciationIpa` when reliable
- include `syllableBreak` when it materially helps learner readability or TTS quality
- include `collections` when the package belongs to a book, course path, revision list, or curated study playlist
- prefer specifying both `partOfSpeech` and `cefrLevel` in collection word references

---

## 10. Final Rule

The most important import rule for Phase 1 is:

**import must be predictable.**

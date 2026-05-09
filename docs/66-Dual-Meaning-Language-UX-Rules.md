# Dual Meaning-Language UX Rules

## Purpose

This document defines how Darwin Lingua should present German learning content when the learner selects one or two meaning languages.

It applies to:

- word lists
- word detail pages
- examples
- favorites and collections
- practice prompts
- future dialogue practice lessons
- future conversation starters
- future event preparation packs

Use it together with:

- `02-Product-Scope.md`
- `22-Domain-Model.md`
- `26-Bounded-Contexts.md`
- `04-Implementation-Backlog.md`

---

## 1. Language Roles

The learner may have:

- one required primary meaning language
- one optional secondary meaning language

The primary meaning language is the main explanation language.

The secondary meaning language is additional support. It must not replace the primary language or become the only visible meaning unless the learner changes the primary preference.

---

## 2. Display Order

When two meaning languages are active, display order is:

1. German source content
2. primary meaning language
3. secondary meaning language

This order should stay consistent across mobile, web, dialogue practice lessons, conversation starters, and practice preparation.

---

## 3. Compact Mode

Compact mode is for dense surfaces such as:

- word list rows
- search results
- favorite lists
- collection items
- practice answer summaries

Rules:

- show the primary meaning language first
- show the secondary meaning language only if space allows without crowding
- use a short language label when two translations are shown
- avoid multiline overflow that makes list scanning slow
- do not hide the German lemma to make room for translations

If the secondary translation is unavailable, compact mode should omit it quietly.

---

## 4. Expanded Mode

Expanded mode is for detail surfaces such as:

- word detail pages
- example sentence details
- scenario lesson steps
- conversation starter detail views
- event preparation packs

Rules:

- show both active meaning languages when content exists
- group translations by the content item they explain
- label languages clearly when two meaning languages are visible
- preserve the primary/secondary order
- allow missing secondary content to fall back without blocking the page

Expanded mode should make incomplete translation coverage visible enough for quality review but not disruptive for learners.

---

## 5. Missing Translation Fallback

If the primary meaning language is missing:

- show the best available fallback only if the content item would otherwise be unusable
- prefer English as the operational fallback when available
- mark the issue for content-quality reporting

If the secondary meaning language is missing:

- show the primary language normally
- omit the secondary translation in compact mode
- show a subtle unavailable state in expanded mode only when it helps explain asymmetry
- mark the issue for content-quality reporting

Learners should not see raw language codes as fallback text.

---

## 6. Practice Behavior

Practice should use the active meaning languages deliberately.

Recommended initial rules:

- recognition prompts may show German and ask for the primary meaning language
- review summaries may show both active languages
- secondary language should not make answer validation ambiguous
- answer checking should use one expected target language at a time

Dual-language support is for comprehension first. It should not accidentally turn every practice item into a multi-answer quiz.

---

## 7. Scenario And Conversation Content

Dialogue practice lessons and conversation starters should follow the same rules as word details:

- German phrase first
- primary translation second
- secondary translation third
- tone notes and cultural notes use the primary meaning language first
- secondary translation is supportive and optional

When space is tight, scenario cards may show only German plus primary language and let the detail view show both.

---

## 8. Accessibility And Localization

Required rules:

- language labels must be localizable UI strings
- screen reader labels should identify which language each translation belongs to
- RTL meaning languages must be displayed with correct text direction
- primary and secondary translations must not rely only on color to be distinguishable

---

## 9. Implementation Contract

Presentation code should consume an ordered active-language list rather than rebuilding meaning-language rules on each screen.

The current active-language contract is:

- `PreferredMeaningLanguage1` is required
- `PreferredMeaningLanguage2` is optional
- `ActiveMeaningLanguages` returns one or two language codes in display order

UI code should treat more than two active meaning languages as invalid for the current product scope.

# Helper-Language Expansion Readiness

Status: readiness plan.

Created: 2026-06-24.

Purpose: define how Darwin Lingua can add more helper or meaning languages without redesigning target-learning-language identity.

## Stable Rule

Helper languages explain authored source content. They do not become target-learning languages automatically and they do not change the source identity of a lesson, exercise, template, exam-prep unit, or country-guidance note.

Example:

- German source content can later be explained in 15 helper languages.
- That does not create 15 German source streams.
- It does not make Persian, Arabic, or English a target-learning-language unless those languages also have separate native learning content.

## Current Active Helper Set

The current required helper-language set is:

- `ar`
- `ckb`
- `en`
- `fa`
- `kmr`
- `pl`
- `ro`
- `ru`
- `sq`
- `tr`

This set is used by content validation and admin diagnostics. It can grow later as a content-coverage expansion.

## Expansion Gate

Before adding a new helper language:

1. Add the language to the supported helper/meaning-language reference data.
2. Add UI display metadata, native name, English name, text direction, and font/rendering expectations.
3. Confirm whether the helper language is left-to-right or right-to-left.
4. Add localization/resource keys only if it is also a UI language. Helper-language support alone does not require UI chrome translation.
5. Update import validation so missing helper translations are reported for the new language.
6. Update admin reports so missing helper coverage is counted by target language, module, and helper language.
7. Backfill helper translations for all active public content before making the helper language visible as a learner option.
8. Smoke helper projection on list/detail pages for long text, lists, examples, activity blocks, and linked practice.
9. Verify no helper-language expansion changes `targetLearningLanguageCode`, slugs, progress keys, search scope, or country-context source identity.

## RTL Requirements

For right-to-left helper languages:

- Helper text containers must render with `dir="rtl"` or the shared direction helper.
- Mixed German/English source text inside helper explanations must remain readable.
- Long template text, examples, lists, and activity instructions must wrap without horizontal overflow.
- Search filters, settings controls, and helper-language badges must not reverse target-language source text.

## Admin Diagnostics

Admin reports should show:

- missing helper translations by helper language
- missing helper translations by module
- counts scoped by target language
- issue drilldown with content owner slug and missing language code

These diagnostics are coverage signals. They are not target-language activation signals.

## Non-Goals

- Do not create a new target-learning-language because a helper language is added.
- Do not duplicate source content per helper language.
- Do not treat helper-language translations as source content for another learning language.
- Do not make a helper language public until active content has reviewed helper coverage.

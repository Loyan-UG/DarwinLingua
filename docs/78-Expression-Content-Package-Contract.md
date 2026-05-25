# Expression Content Package Contract

## Purpose

This document defines the JSON/import contract for the Phase 7 Everyday Expressions module.

Everyday Expressions are dynamic, importable learning content. They must not be stored as WordEntry-only records, and they must not duplicate Word meanings. They may link to Words by lemma or slug when useful.

Everyday Expressions is not a general sentence bank. Ordinary literal sentences such as `Die Heizung funktioniert nicht.`, `Ich brauche einen Termin.`, or `Ich gehe zum Arzt.` belong in Dialogues, Courses, Exercises, Writing Templates, or Grammar examples unless they have a conventional pragmatic, idiomatic, cultural, or non-literal use that must be taught as an expression.

The roadmap source of truth remains `76-Learning-Portal-Roadmap-And-Backlog.md`.

## Expression Eligibility Rule

Allowed in official Expression packages:

- non-literal idioms
- semi-idiomatic colloquial phrases
- fixed social formulas with conventional pragmatic usage
- polite formulas
- proverbs
- cultural phrases
- false friends
- regional expressions
- slang, rude, warning, or adult expressions only with safety metadata and access controls

Not allowed in published official Expression packages:

- ordinary literal sentences whose meaning is directly compositional
- normal situational sentences better taught in Dialogues, Courses, Exercises, or Writing Templates
- utility sentences such as "The heating does not work" unless the phrase has a conventional non-literal or pragmatic meaning
- generic translations or simple classroom sentences

Every official Expression entry must explain why it belongs in Expressions rather than normal lessons or Dialogues.

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
- `meaningTransparency`: controlled transparency classification
- `usageExplanation`: context/tone explanation
- `teachingReason`: why this belongs in Expressions
- `safetyRating`: controlled safety classification
- `sensitiveContentKind`: controlled sensitive-education classification
- `minimumAge`
- `requiresAdultAccess`
- `requiresSensitiveOptIn`
- `requiresVerifiedAdult`
- `usagePolicy`

Optional:

- `literalMeaningText`
- `region`
- `isRisky`
- `adultContentCategory`
- `contentWarnings`
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

Meaning transparency:

- `non-literal`
- `semi-idiomatic`
- `pragmatic-formula`
- `literal-fixed-formula`
- `ordinary-literal`

Published official content must reject `ordinary-literal`. Legacy records without this metadata may remain renderable only as quality-report findings until repaired.

Safety ratings:

- `general`
- `mild-rude`
- `strong-rude`
- `sexual-educational`
- `romantic-social`
- `explicit-adult`
- `discriminatory-slur`
- `politically-sensitive`
- `blocked-illegal`

Sensitive content kinds:

- `none`
- `swear-word`
- `insult`
- `rude-colloquial`
- `mild-emotional`
- `romantic-social`
- `sexual-educational-neutral`
- `slur-educational`
- `blocked`

Minimum ages:

- `0`
- `12`
- `16`
- `18`

Usage policies:

- `safe-to-use`
- `use-with-care`
- `understand-only`
- `do-not-use`
- `blocked`

Adult content categories are optional controlled keys for restricted educational content, such as `rude-slang`, `sexual-language`, `explicit-sexual-language`, `discriminatory-language`, or `politically-sensitive-language`.

## Localization

`meanings` provide learner-language actual meaning, literal meaning, and usage explanation variants.

Example and warning translations use the standard `{ "language": "...", "text": "..." }` shape.

Supported language codes must already be active platform meaning languages. Duplicate translations for the same language are rejected.

## Safety And Tone

Expressions that are risky, rude, slang-heavy, friends-only, warning phrases, discriminatory, sexual, highly political, or easy to misuse must include warning metadata.

The importer rejects risky expressions that do not include at least one warning with text.

Sensitive Educational Language is the product term for warning-labeled educational content such as common rude words, insults, slang, mild emotional or romantic/social phrases, and neutral non-graphic sexual-language comprehension. It is for comprehension first and must not encourage learners to casually use insults or risky phrases.

Sensitive Educational Language is not pornographic content. Official content must not generate pornographic, arousing, graphic sexual, exploitative, coercive, minor-related, illegal, hate-inciting, Nazi-propaganda, or harm-facilitating content.

`explicit-adult`, pornographic, or strongly sexual content must not be visible to anonymous users, minors, self-declared-only users, or unverified users. In Germany, a simple checkbox is not sufficient for pornographic or legally restricted adult content. Production launch of explicit adult content requires legal review and an approved age-verification/closed-user-group concept.

Implementation guidance for Germany: public documentation from NLM, KJM, and FSM describes closed adult user groups as requiring adult identification plus per-use authentication for content that may be offered only to adults. Treat `self-declared-adult` as a stored preference and not as sufficient production access for explicit adult/pornographic content. Approved future states should use the minimal needed claim, such as a verified-over-18 status, rather than storing full birthdates unless a reviewed provider flow requires it.

Educational rude or slang content may be marked and filtered separately from explicit adult content. Discriminatory slurs may only be included for comprehension and safety education, with strong warnings, neutral explanation, no endorsement, and restricted visibility if needed. Sexual content involving minors, coercion, exploitation, or illegal content is never allowed.

Current implementation rules:

- `blocked-illegal` is rejected.
- `explicit-adult` is rejected until legal review and verified-adult access exist.
- `requiresVerifiedAdult: true` is rejected in official generated content until a verified-adult system exists.
- `discriminatory-slur` and `slur-educational` are blocked by default unless a future manually reviewed safety package explicitly enables them.
- non-general sensitive entries must set `requiresSensitiveOptIn: true`.
- non-general sensitive entries must include warnings and a non-default `usagePolicy`.
- `sexual-educational` must be neutral, non-graphic, non-arousing, and use `sensitiveContentKind: sexual-educational-neutral`.
- `romantic-social` may be included only when mild, non-graphic, and socially useful.
- mobile export excludes Sensitive Educational Language by default until mobile has equivalent opt-in filtering and warning rendering.

See `85-Sensitive-Educational-Language-Policy.md` for the cross-module policy, privacy, filtering, and release requirements. See `86-Web-Legal-Compliance-Baseline.md` for the Web registration, legal page, cookie/storage, GDPR/TDDDG/DDG, and legal-review release gates that must stay in place before sensitive educational content is expanded.

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
  "meaningTransparency": "pragmatic-formula",
  "teachingReason": "It is a conventional confirmation formula whose social function is broader than the literal words.",
  "safetyRating": "general",
  "sensitiveContentKind": "none",
  "minimumAge": 0,
  "requiresAdultAccess": false,
  "requiresSensitiveOptIn": false,
  "requiresVerifiedAdult": false,
  "usagePolicy": "safe-to-use",
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
    },
    {
      "germanText": "Alles klar, ich bringe die Unterlagen morgen mit.",
      "translations": [
        { "language": "en", "text": "Understood, I will bring the documents tomorrow." }
      ],
      "sortOrder": 20
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
- `meaningTransparency` must use the controlled set
- published `ordinary-literal` entries are rejected
- `literalMeaningText` is required for `non-literal` and `semi-idiomatic`
- `usageExplanation` and `teachingReason` are required for classified official content
- published classified official content needs at least two German examples unless a documented exception exists
- German examples must show realistic contexts and must not repeat the same generic frame
- `safetyRating`, `minimumAge`, and `requiresAdultAccess` must be valid and consistent
- `sensitiveContentKind`, `requiresSensitiveOptIn`, `requiresVerifiedAdult`, and `usagePolicy` must be valid and consistent
- `blocked-illegal`, `explicit-adult`, blocked sensitive kinds, blocked usage policies, and verified-adult-required official generated entries are rejected until a dedicated legal/access system exists
- sensitive entries require warnings, `requiresSensitiveOptIn: true`, and a non-default usage policy
- examples require German text
- linked words store only lemma/slug references, not meanings
- duplicate translation language codes rejected
- unsupported translation language codes rejected
- risky expressions require warning text
- adult-only content is hidden from public learner surfaces by default until profile eligibility and legal review gates are complete

## Content Generation Rule

Bulk expression content generation must not start until implementation, validation, Web API, Web rendering, admin visibility, and release tests are stable.

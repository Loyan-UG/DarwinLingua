# Country Guidance Content Package Contract

## Package Target Language

Every import package must declare package-level `targetLearningLanguageCode`. Current official German-learning packages use `"de"`.

`targetLearningLanguageCode` is the language being taught. It is separate from `defaultMeaningLanguages` and from all `...Translations` fields, which remain helper/meaning languages for learner support.

This feature is the stable platform concept `Country Guidance`. German/Germany content must use `targetLearningLanguageCode = "de"` and `countryContextCode = "DE"`. Future country guidance for Austria, Switzerland, the United States, the United Kingdom, and other countries must be authored separately for each target-language/country-context pair instead of being reused as translated copies.

Import validation accepts only content-importable target learning languages: public-active languages plus explicitly approved pilot/staging languages. Current reviewed imports may use German (`de`) and pilot English (`en`); planned languages such as Spanish (`es`) and French (`fr`) must be rejected until their readiness gates are complete.

Levelled packages must declare `levelSystemCode`; current German packages use CEFR (`"cefr"`). Import validation rejects missing `levelSystemCode`, unsupported level systems, and non-content-importable target-learning languages before content is persisted.

Every Country Guidance package must declare package-level `countryContextCode`. Import validation rejects a Country Guidance package that omits the country context or uses a country context that is not active for the package target learning language.

All source fields must be authored natively for the package target language and country context. A Switzerland stream under German, French, and Italian is three separate original source streams, not one country note translated into three source languages.

## Purpose

This document defines the JSON import contract for the Web-first `Country Guidance` feature.

For German learners in the Germany country context, the learner-facing display label can be `Life in Germany`. That content teaches German communication norms, social expectations, everyday systems, basic legal/civic knowledge, and context-specific language use. It also covers important knowledge areas behind the official Orientierungskurs, Test Leben in Deutschland, and Einbuergerungstest, without copying the official question bank as product-owned test content.

Implementation note: the canonical platform feature name is `Country Guidance`. The implementation now uses `CountryGuidanceNote` and the `CountryGuidanceNotes` table; old `CulturalNote`/`CulturalNotes` names remain only in historical migrations and explicit table-rename SQL.

## Root Array

- `countryGuidanceNotes`

## Required Fields

- `slug`
- `title`
- `titleTranslations`
- `shortDescription`
- `shortDescriptionTranslations`
- `cefrLevel`
- `category`
- `context`
- `contextTranslations`
- `sections`
- `sectionsTranslations`
- `sortOrder`

Optional fields:

- `examples`
- `doNotes`
- `doNotesTranslations`
- `dontNotes`
- `dontNotesTranslations`
- `sensitivityWarning`
- `sensitivityWarningTranslations`
- `linkedDialogueSlugs`
- `linkedExpressionSlugs`
- `linkedWritingTemplateSlugs`
- `linkedTalkTopicSlugs`
- `linkedCourseLessonSlugs`
- `isPublished`

## Translation Fields

Target-language source fields are canonical. Helper translations explain the country-context note for learners and must not replace the target-language source behavior. Current Germany guidance under `de|DE` therefore uses German source text.

Required helper translation languages:

- `en`
- `fa`
- `ar`
- `tr`
- `ru`
- `ckb`
- `kmr`
- `pl`
- `ro`
- `sq`

Text translation fields use:

```json
{ "language": "fa", "text": "..." }
```

List translation fields use:

```json
{ "language": "fa", "items": ["...", "..."] }
```

For `examples`, each example may include `explanationTranslations`.

Helper translations should be semantic and culturally aware. They may use a target-language-friendly comparison or clarification when it helps learners understand the German norm, legal/civic concept, or public system, but they must avoid stereotypes and must not claim that all speakers of a language or country behave the same way.

## Level Depth Rules

- `A1-A2` notes should stay practical, short, and immediately usable in everyday life.
- `B1` notes should explain the core Orientierungskurs/Leben-in-Deutschland concept clearly enough that a learner understands the idea behind common test topics instead of only memorizing answers.
- `B2+` notes may add more precise civic, legal, historical, and social-system nuance, but must remain general education and not individual legal advice.

The German `context` field is intentionally compact and must stay within the storage limit used by validation. For B1+ notes, keep `context` substantial but concise, and put the deeper explanation in `sections`. Do not make B1+ notes look complete by title alone while leaving the actual explanation too thin.

## Controlled Categories

- `du-vs-sie`
- `politeness`
- `directness`
- `small-talk`
- `workplace-culture`
- `office-communication`
- `school-kindergarten`
- `doctor-visit`
- `appointments`
- `punctuality`
- `complaints`
- `bureaucracy`
- `conversation-cafe-etiquette`
- `law-and-rights`
- `democracy-and-state`
- `history-and-responsibility`
- `society-and-family`
- `education-and-work`
- `religion-and-tolerance`
- `equality-and-non-discrimination`
- `federal-states-and-geography`
- `political-participation`
- `social-system`
- `exam-orientation`

## Example Shape

```json
{
  "slug": "a2-du-vs-sie-at-work",
  "title": "Du vs. Sie at work",
  "titleTranslations": [
    { "language": "fa", "text": "تو یا شما در محیط کار" }
  ],
  "shortDescription": "A practical note about choosing address forms.",
  "shortDescriptionTranslations": [
    { "language": "fa", "text": "یادداشتی کاربردی درباره انتخاب شکل خطاب کردن." }
  ],
  "cefrLevel": "A2",
  "category": "du-vs-sie",
  "context": "Workplace introductions",
  "contextTranslations": [
    { "language": "fa", "text": "معرفی در محیط کار" }
  ],
  "sections": ["Use Sie until a colleague offers du."],
  "sectionsTranslations": [
    { "language": "fa", "items": ["تا وقتی همکار خودش du را پیشنهاد نکرده، از Sie استفاده کن."] }
  ],
  "examples": [
    {
      "germanText": "Sollen wir uns duzen?",
      "explanation": "A polite way to ask about switching to du.",
      "explanationTranslations": [
        { "language": "fa", "text": "راهی محترمانه برای پرسیدن درباره تغییر خطاب به du." }
      ]
    }
  ],
  "doNotes": ["Start with Sie in formal settings."],
  "doNotesTranslations": [
    { "language": "fa", "items": ["در موقعیت رسمی با Sie شروع کن."] }
  ],
  "dontNotes": ["Do not switch to du automatically."],
  "dontNotesTranslations": [
    { "language": "fa", "items": ["خودکار به du تغییر نده."] }
  ],
  "sensitivityWarning": "Address forms can feel personal in hierarchical contexts.",
  "sensitivityWarningTranslations": [
    { "language": "fa", "text": "در موقعیت‌های سلسله‌مراتبی، شکل خطاب کردن می‌تواند جنبه شخصی پیدا کند." }
  ],
  "linkedDialogueSlugs": ["a2-workplace-introduction"],
  "linkedExpressionSlugs": ["sollen-wir-uns-duzen"],
  "linkedWritingTemplateSlugs": ["a2-formal-work-email"],
  "linkedTalkTopicSlugs": ["a2-workplace-small-talk"],
  "linkedCourseLessonSlugs": ["a2-workplace-communication"],
  "sortOrder": 10
}
```

## Validation Rules

- slugs must be lowercase kebab-case
- package-level `targetLearningLanguageCode` is required and must be content-importable
- package-level `countryContextCode` is required for Country Guidance packages and must be active for the target learning language
- CEFR levels must be valid
- category must use a controlled value
- `sections` must contain at least one non-empty item
- examples require `germanText`
- all required helper translation fields must include every active learner language
- duplicate helper translation language entries are rejected
- helper translation languages outside active learner languages are rejected
- non-English helper translations must not reuse the English helper text
- list translation item counts must match the source German list count
- linked slugs must use lowercase kebab-case
- sensitive topics should include `sensitivityWarning`

## Safety And Legal-Education Rule

Country Guidance notes may discuss sensitive communication expectations, democratic values, basic rights and duties, public institutions, and everyday legal-administrative expectations. The tone must stay neutral, practical, and learner-safe.

Content must be general education, not individual legal advice.

Official `Leben in Deutschland` or `Einbuergerungstest` questions should not be bulk-copied as the core app content. The safer product approach is to teach the concepts in original language and link to official resources where needed.

## No Duplication Rule

Country Guidance notes may link to dialogues, expressions, writing templates, Talk Topics, Exam Prep units, and course lessons. They should not duplicate full linked content.

Bulk Country Guidance content generation for additional countries or target languages must not start until this contract, validation, Web API, Web rendering, and admin inspection are stable.

# Conversation Starter Content Package Contract

## Package Target Language

Every import package must declare package-level `targetLearningLanguageCode`. Current official German-learning packages use `"de"`.

`targetLearningLanguageCode` is the language being taught. It is separate from `defaultMeaningLanguages` and from all `...Translations` fields, which remain helper/meaning languages for learner support.

Import validation accepts only content-importable target learning languages: public-active languages plus explicitly approved pilot/staging languages. Current reviewed imports may use German (`de`) and pilot English (`en`); planned languages such as Spanish (`es`) and French (`fr`) must be rejected until their readiness gates are complete.

Levelled packages must declare `levelSystemCode`; current German packages use CEFR (`"cefr"`). Import validation rejects missing `levelSystemCode`, unsupported level systems, and non-content-importable target-learning languages before content is persisted.

All source fields must be authored natively in the package target language. Future English, Spanish, or French conversation-starter packages must be new source content for those languages, not translated copies of German packs.

This document defines the Phase 6 JSON contract for conversation starter packs before persistence and UI work.

## Package Shape

Conversation starter packs live beside vocabulary entries, collections, and dialogue lessons:

```json
{
  "packageVersion": "1.0",
  "packageId": "a1-a2-conversation-starters",
  "packageName": "A1/A2 Conversation Starters",
  "source": "Hybrid",
  "defaultMeaningLanguages": ["en", "fa"],
  "entries": [],
  "conversationStarterPacks": []
}
```

`entries` remains required by the current importer until non-vocabulary-only content packages are supported.

## ConversationStarterPack

Required fields:

- `slug`: stable lowercase kebab-case identifier.
- `title`: learner-facing title.
- `description`: short explanation of the conversation situation.
- `cefrLevel`: one of `A1`, `A2`, `B1`, `B2`, `C1`, `C2`.
- `category`: stable lowercase kebab-case category.
- `situation`: stable lowercase kebab-case situation such as `work`, `cafe`, `class`, `neighbor`, `school`, `event`, `online-meeting`, or `first-meeting`.
- `tone`: stable lowercase kebab-case tone such as `formal`, `friendly`, `very-simple`, or `casual`.
- `conversationGoal`: stable lowercase kebab-case goal such as `introduction`, `asking-questions`, `continuing-conversation`, `ending-politely`, or `arranging-next-contact`.
- `topics`: one or more existing catalog topic keys.
- `phrases`: one or more conversation starter phrases.

Optional fields:

- `sortOrder`: non-negative display order.
- `linkedDialogueSlugs`: zero or more dialogue lessons that can surface this pack.
- `linkedEventPreparationPackSlugs`: zero or more event preparation packs that can reuse this pack later.

## ConversationStarterPhrase

Required fields:

- `baseText`: German phrase.
- `translations`: one or more meaning-language translations.
- `function`: stable lowercase kebab-case phrase function such as `opening`, `follow-up-question`, `clarification`, `reaction`, `transition`, `closing`, or `next-step`.

Optional fields:

- `usageNote`: short note describing when to use the phrase.
- `register`: stable lowercase kebab-case register such as `formal`, `neutral`, or `informal`.
- `sortOrder`: non-negative order inside the pack.
- `alternativeBaseTexts`: German alternatives that preserve the same function.
- `commonMistake`: short static warning about a common learner error.

## Starter Categories And Filters

The first mobile and web browsing filters should use these dimensions:

- CEFR level
- situation
- tone
- conversation goal
- topic key

Category values should stay stable and lowercase kebab-case. Learner-facing localized labels belong in UI localization, not in the content key.

## Dual Meaning-Language Rules

- Every phrase should include at least the package's default meaning languages.
- Missing primary meaning-language translations are errors when the language is declared as default.
- Missing secondary meaning-language translations should be warnings first, matching dialogue lesson behavior.
- UI must show German first, primary meaning second, and secondary meaning third when available.
- Compact cards may show only German plus primary meaning; detail screens should show both meaning languages when available.

## Dialogue Integration Rules

Dialogue lessons may reference starter packs by slug after starter persistence exists.

The first implementation should support:

- starter packs shown on dialogue detail
- dialogue lessons shown as related practice from starter detail
- starter packs exported in full catalog packages
- CEFR-slice packages including only packs matching that CEFR level

## Validation Rules

The importer should reject conversation starter content when:

- `slug`, `title`, `description`, `cefrLevel`, `category`, `situation`, `tone`, `conversationGoal`, or `topics` is missing.
- `slug`, `category`, `situation`, `tone`, `conversationGoal`, or phrase `function` is not lowercase kebab-case.
- `cefrLevel` is not a supported CEFR value.
- a referenced topic key is unknown.
- a linked dialogue slug does not exist when dialogue references are validated.
- a phrase is missing German `baseText`.
- phrase translations are empty, unsupported, duplicated by language, or missing text.
- a pack has no valid phrases.

## Sample

```json
{
  "slug": "a1-cafe-first-meeting",
  "title": "Cafe First Meeting",
  "description": "Simple phrases for starting a friendly first conversation in a cafe.",
  "cefrLevel": "A1",
  "category": "first-meetings",
  "situation": "cafe",
  "tone": "friendly",
  "conversationGoal": "introduction",
  "topics": ["everyday-life"],
  "sortOrder": 10,
  "linkedDialogueSlugs": ["a1-buy-bread-at-bakery"],
  "phrases": [
    {
      "baseText": "Hallo, ich heiße Sara.",
      "function": "opening",
      "register": "neutral",
      "usageNote": "Use this as a simple first introduction.",
      "translations": [
        { "language": "en", "text": "Hello, my name is Sara." },
        { "language": "fa", "text": "سلام، اسم من سارا است." }
      ]
    },
    {
      "baseText": "Und wie heißen Sie?",
      "function": "follow-up-question",
      "register": "formal",
      "translations": [
        { "language": "en", "text": "And what is your name?" },
        { "language": "fa", "text": "و اسم شما چیست؟" }
      ]
    }
  ]
}
```

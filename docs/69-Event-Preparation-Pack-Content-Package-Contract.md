# Event Preparation Pack Content Package Contract

This document defines the Phase 6 JSON contract for event preparation packs before event-directory UI work.

## Package Shape

Event preparation packs live beside vocabulary entries, collections, dialogue lessons, and conversation starter packs:

```json
{
  "packageVersion": "1.0",
  "packageId": "a1-a2-event-preparation-packs",
  "packageName": "A1/A2 Event Preparation Packs",
  "source": "Hybrid",
  "defaultMeaningLanguages": ["en", "fa"],
  "entries": [],
  "eventPreparationPacks": []
}
```

`entries` remains required by the current importer until non-vocabulary-only content packages are supported.

## EventPreparationPack

Required fields:

- `slug`: stable lowercase kebab-case identifier.
- `title`: learner-facing title.
- `description`: short description of the event context.
- `cefrLevel`: one of `A1`, `A2`, `B1`, `B2`, `C1`, `C2`.
- `category`: stable lowercase kebab-case category.
- `eventType`: stable lowercase kebab-case event type such as `conversation-cafe`, `online-practice`, `workshop`, `club-meetup`, or `parent-meeting`.
- `topics`: one or more existing catalog topic keys.

Optional fields:

- `sortOrder`: non-negative display order.
- `linkedDialogueSlugs`: dialogue lessons useful for this event.
- `linkedConversationStarterPackSlugs`: conversation starter packs useful for this event.
- `linkedVocabulary`: vocabulary references using `word`, optional `partOfSpeech`, and optional `cefrLevel`.
- `openingPrompts`: short German prompts learners can try at the event.
- `roleplayPrompts`: short roleplay instructions.
- `reviewPrompts`: short post-event review prompts.

## Validation Rules

The importer should reject event preparation content when:

- `slug`, `title`, `description`, `cefrLevel`, `category`, `eventType`, or `topics` is missing.
- `slug`, `category`, `eventType`, linked dialogue slugs, or linked starter slugs are not lowercase kebab-case.
- `cefrLevel` is not a supported CEFR value.
- a referenced topic key is unknown.
- `linkedVocabulary` contains empty `word` values.
- optional `partOfSpeech` or `cefrLevel` vocabulary selectors are invalid.
- prompt arrays contain empty items.

## Sample

```json
{
  "slug": "a1-conversation-cafe-first-visit",
  "title": "First Conversation Cafe Visit",
  "description": "Prepare for a friendly beginner conversation cafe.",
  "cefrLevel": "A1",
  "category": "conversation-cafe",
  "eventType": "conversation-cafe",
  "topics": ["everyday-life"],
  "sortOrder": 10,
  "linkedDialogueSlugs": ["a1-buy-bread-at-bakery"],
  "linkedConversationStarterPackSlugs": ["a1-cafe-first-meeting-starters"],
  "linkedVocabulary": [
    { "word": "Hilfe", "partOfSpeech": "Noun", "cefrLevel": "A1" }
  ],
  "openingPrompts": ["Hallo, ich heiße ..."],
  "roleplayPrompts": ["Introduce yourself and ask one simple question."],
  "reviewPrompts": ["Which phrase did you use successfully?"]
}
```

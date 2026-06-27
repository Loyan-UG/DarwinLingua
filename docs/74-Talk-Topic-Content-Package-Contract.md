# Talk Topic Content Package Contract

Talk Topics are reading-based conversation materials for friendly learner groups, language cafes, online practice groups, and informal speaking sessions.

## Package Target Language

Every import package must declare package-level `targetLearningLanguageCode`. Current official German-learning packages use `"de"`.

`targetLearningLanguageCode` is the language being taught. It is separate from `defaultMeaningLanguages` and from all `...Translations` fields, which remain helper/meaning languages for learner support.

Import validation accepts only content-importable target learning languages: public-active languages plus explicitly approved pilot/staging languages. Current reviewed imports may use German (`de`) and pilot English (`en`); planned languages such as Spanish (`es`) and French (`fr`) must be rejected until their readiness gates are complete.

Levelled packages must declare `levelSystemCode`; current German packages use CEFR (`"cefr"`). Import validation rejects missing `levelSystemCode`, unsupported level systems, and non-content-importable target-learning languages before content is persisted.

All source fields must be authored natively in the package target language and conversation culture. Future English, Spanish, or French Talk Topic packages must be new source content for those languages, not translated copies of German topics.

## JSON Shape

Packages may include a top-level `talkTopics` array. Each item stores metadata, one target-language article body, target-language warm-up questions, target-language discussion questions, vocabulary references, speaking goals, sensitivity metadata, sort order, and publication state.

Required fields:

- `slug`, lowercase kebab-case
- `topicGroupKey`, lowercase kebab-case
- `title`
- `description`
- `cefrLevel`: `A1`, `A2`, `B1`, `B2`, `C1`, or `C2`
- `category`, lowercase kebab-case
- `topics`, with at least one known active topic key
- `contentType`
- `article.baseText`
- `discussionQuestions`
- `vocabularyItems`
- `speakingGoals`
- `estimatedReadingMinutes`
- `estimatedDiscussionMinutes`
- `sortOrder`
- `isPublished`

Allowed `contentType` values:

- `article`
- `book-summary`
- `movie-summary`
- `story`
- `fact-sheet`
- `opinion-text`
- `interview`
- `debate-text`

Book summaries and movie summaries are especially intended for higher CEFR levels because they usually require more background knowledge and longer discussion.

## Text Length Validation

The normalized German `article.baseText` length is measured in characters, not words. Leading and trailing whitespace is ignored. The length is a target range, not only a minimum.

- `A1`: target 1000 characters, acceptable range 900-1100
- `A2`: target 1500 characters, acceptable range 1400-1600
- `B1`: target 2000 characters, acceptable range 1900-2100
- `B2`: target 2500 characters, acceptable range 2400-2600
- `C1`: target 3000 characters, acceptable range 2900-3100
- `C2`: target 3500 characters, acceptable range 3400-3600

Article translations are not stored for Talk Topics. The main article is target-language source content; current German packages therefore store the article in German.

## Questions

Warm-up questions are first-class target-language source content. They are for speaking before reading, not comprehension.

- `A1`, `A2`, and `B1`: at least 3 warm-up questions.
- `B2`, `C1`, and `C2`: at least 4 warm-up questions.

Discussion questions are required target-language source content. Allowed question types are:

- `opinion`
- `imagination`
- `prediction`
- `comparison`

Each Talk Topic must include enough questions per type:

- `A1`, `A2`, and `B1`: at least 2 questions per question type.
- `B2`, `C1`, and `C2`: at least 3 questions per question type.

Question translations are not stored for Talk Topics.

## Vocabulary

Talk Topics must not duplicate word meanings, explanations, or examples. Vocabulary items store only references:

- `lemma`
- optional `wordSlug`
- optional `cefrLevel`
- optional `sortOrder`

The Web detail page resolves meanings from the existing Word Catalog. Unresolved references render safely as plain text.

Recommended vocabulary-reference counts:

- `A1`: 12-18 items
- `A2`: 15-22 items
- `B1`: 18-26 items
- `B2`: 22-32 items
- `C1`: 26-38 items
- `C2`: 30-45 items

## Speaking Goals

Allowed speaking goals are `express-opinion`, `give-reasons`, `agree-disagree`, `ask-follow-up-questions`, `compare-options`, `make-predictions`, `describe-experiences`, `imagine-possibilities`, `debate-politely`, and `summarize-position`.

Each Talk Topic must include 2-5 speaking goals. Higher-level topics should often include `debate-politely`, `summarize-position`, `compare-options`, and `give-reasons`.

## CEFR Filtering And Variants

The TalkTopic `cefrLevel` field is separate from vocabulary item levels and is used by the list API and Web page for filtering.

A real-world topic may have multiple level variants. Use the same `topicGroupKey` for related variants and a level-specific `slug`, for example:

- `topicGroupKey`: `alien-life`
- `slug`: `a2-alien-life`
- `slug`: `b2-alien-life`
- `slug`: `c1-alien-life`

## Sensitivity

`isSensitive` defaults to false. Sensitive topics are not hidden automatically, but the UI should show a mild warning. `recommendedForModeratedGroupsOnly` marks topics that should normally be used with a facilitator.

## Future Reusable Support Content

Useful sentence patterns and moderator guidance should be reusable/static learning-help content, not repeated inside every Talk Topic. Future sections may include:

- how to express your opinion in German
- how to agree and disagree politely
- how to ask follow-up questions
- how to moderate a language cafe discussion
- how to use Talk Topics in pairs or small groups
- useful sentence patterns for A1/A2/B1/B2 discussions

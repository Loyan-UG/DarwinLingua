# Talk Topic Content Package Contract

Talk Topics are reading-based conversation materials for friendly learner groups, language cafes, online practice groups, and informal speaking sessions.

## JSON Shape

Packages may include a top-level `talkTopics` array. Each item stores metadata, one German article body, optional article translations, warm-up questions, discussion questions, vocabulary references, speaking goals, sensitivity metadata, sort order, and publication state.

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

The normalized German `article.baseText` must meet the CEFR-specific minimum length. Leading and trailing whitespace is ignored. Translations are not used for this check.

- `A1`: 1000 characters
- `A2`: 1500 characters
- `B1`: 2000 characters
- `B2`: 2500 characters
- `C1`: 3000 characters
- `C2`: 3500 characters

## Questions

Warm-up questions are first-class content and should usually contain at least two accessible, personal, or experience-based prompts. They are for speaking before reading, not comprehension.

Discussion questions are required. Allowed question types are `opinion`, `personal-experience`, `prediction`, `comparison`, `imagination`, `debate`, `ethics`, and `comprehension`. Opinion and speaking-oriented questions should dominate; comprehension questions may exist but should not be the main focus.

## Vocabulary

Talk Topics must not duplicate word meanings, explanations, or examples. Vocabulary items store only references:

- `lemma`
- optional `wordSlug`
- optional `cefrLevel`
- optional `sortOrder`

The Web detail page resolves meanings from the existing Word Catalog. Unresolved references render safely as plain text.

## Speaking Goals

Allowed speaking goals are `express-opinion`, `give-reasons`, `agree-disagree`, `ask-follow-up-questions`, `compare-options`, `make-predictions`, `describe-experiences`, `imagine-possibilities`, `debate-politely`, and `summarize-position`.

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

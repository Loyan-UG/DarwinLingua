# English Target-Language Capability Profile And Pilot Plan

Status: pilot, not public active.

Created: 2026-06-24.

Purpose: define and track the first non-German target-language pilot.

Current implementation status:

- The English A1 pilot package has been generated, expanded once, and imported as controlled pilot/staging data.
- Package: `content/learning-portal/english/pilot/packages/english-a1-platform-pilot-01-v1.json`
- Evidence: `artifacts/validation/english-a1-pilot-import-20260624.md`; `artifacts/validation/english-a1-expansion-02-import-20260624.md`
- English remains non-public and must not appear in normal learner navigation unless a future activation goal explicitly approves it.

## Decision

English is the recommended first non-German pilot after the German baseline is accepted for tester use.

Reasons:

- It uses the same CEFR level system already supported by the platform.
- It is a high-demand target language for many learner groups.
- It exercises country-context branching early because English has several major country contexts.
- It validates that Darwin Lingua is a multilingual language-learning platform, not a German-only portal.

English remains unavailable in normal public learner routes until the pilot is smoke-tested, reviewed, backed up, and explicitly activated. It may be used for reviewed pilot imports and internal diagnostics before public activation.

## Target-Language Scope

- Target learning language code: `en`
- Source language for authored learning content: English
- Source content must be original English learning content, not translated German content.
- Helper translations remain separate from English source content.
- Initial helper/meaning languages should reuse the current active helper-language set unless the helper-language expansion plan changes.
- No German learning content may appear under `/learn/en/...`.

## Country Contexts

Initial country contexts for English:

- `US` - United States
- `GB` - United Kingdom
- `AU` - Australia

Country Guidance content must be written separately per country context. A United States note, a United Kingdom note, and an Australia note are not translations of the same source note. They may cover similar topics, but examples, institutions, spelling conventions, and everyday systems must match the selected country context.

First pilot country-context decision:

- `en|US` is the first English Country Guidance stream for the pilot scope.
- Generate `en|US` guidance only after the first English language-learning pilot proves routing, import, search, and helper-language projection.
- Keep `en|GB` and `en|AU` planned but inactive until a country-context pilot list is reviewed.

## Level System

Use CEFR for the first English pilot:

| Code | Friendly label | Learner meaning |
| --- | --- | --- |
| A1 | First steps | Understand and use very simple everyday English. |
| A2 | Everyday basics | Handle common routines with simple sentences. |
| B1 | Independent use | Explain needs, plans, opinions, and everyday problems. |
| B2 | Confident use | Discuss familiar and professional topics with control. |
| C1 | Advanced control | Use English flexibly in study, work, and public contexts. |
| C2 | Expert style | Handle nuance, rhetoric, and complex discourse. |

Implementation note: the current level metadata model already supports friendly labels. Before activating English, the level catalog should allow target-language-specific labels instead of assuming the German baseline labels everywhere.

## Language-Specific Requirements

English content needs capabilities that are different from German content:

- pronunciation support for stress, weak forms, reductions, linking, rhythm, and common sound contrasts
- spelling variants by country context, especially US versus GB
- register differences across casual, neutral, professional, academic, and service interactions
- phrasal verbs and fixed collocations as first-class learning material
- article and countability practice, which is difficult for many learner groups
- tense and aspect sequencing, especially present perfect, past simple, future forms, and conditionals
- politeness formulas for requests, refusals, disagreement, and follow-up messages
- listening content with accents and transcript support once audio exists

The pilot must not simply mirror the German A1 syllabus. It should share the same product structure, but the language work must be English-native.

## Module Coverage For The First Pilot

The first English pilot should be small enough to inspect manually but broad enough to prove the platform:

- Course path: one A1 path
- Course lessons: 10 planned candidates, current pilot includes all 10
- Grammar topics: 5 planned candidates, current pilot includes all 5
- Expressions: 20 planned candidates, current pilot includes all 20
- Exercises: 8 planned candidates, current pilot includes all 8
- Writing templates: 5 planned candidates, current pilot includes all 5
- Dialogue/talk-topic/roleplay: optional in the first package unless audio/dialogue UX is explicitly part of the pilot
- Country Guidance: planned only; do not generate until English learning pilot is green

## A1 Course Candidate List

Course path slug: `en-a1-everyday-start`

1. `en-a1-say-hello-and-give-your-name` - Say hello and give your name
2. `en-a1-use-i-am-and-i-have` - Use "I am" and "I have"
3. `en-a1-ask-simple-questions` - Ask simple questions
4. `en-a1-numbers-times-and-short-details` - Numbers, times, and short details
5. `en-a1-name-everyday-objects` - Name everyday objects
6. `en-a1-people-and-pronouns` - People and pronouns
7. `en-a1-polite-you-and-basic-requests` - Polite "you" and basic requests
8. `en-a1-regular-verbs-in-daily-life` - Regular verbs in daily life
9. `en-a1-build-a-short-introduction` - Build a short introduction
10. `en-a1-first-contact-review` - First contact review

Current pilot coverage: lessons 1-10 are imported as a non-public staging slice.

## Grammar Candidate List

1. `en-a1-subject-pronouns-and-be` - subject pronouns with "be"
2. `en-a1-have-and-simple-possession` - "have" and simple possession
3. `en-a1-a-an-and-plural-nouns` - "a/an" and plural nouns
4. `en-a1-simple-questions-with-be` - simple questions with "be"
5. `en-a1-basic-word-order` - basic word order

## Expression Candidate List

1. `hello`
2. `good-morning`
3. `nice-to-meet-you`
4. `my-name-is`
5. `i-am-from`
6. `i-live-in`
7. `how-are-you`
8. `i-am-fine`
9. `please`
10. `thank-you`
11. `sorry`
12. `excuse-me`
13. `can-you-repeat-that`
14. `i-do-not-understand`
15. `can-you-help-me`
16. `where-is`
17. `how-much-is-it`
18. `see-you-later`
19. `have-a-nice-day`
20. `one-moment-please`

Slug namespace note: English slugs must be target-language scoped in storage and search. Same or similar slugs in German must not collide with English content.

## Exercise Candidate List

1. choose the correct greeting
2. complete "I am" sentences
3. match names and countries
4. reorder a simple question
5. choose "a" or "an"
6. identify singular and plural nouns
7. complete a short introduction
8. review first-contact phrases

## Writing Template Candidate List

1. short self-introduction message
2. simple appointment request
3. simple class question
4. short apology message
5. simple thank-you message

## Validation Gates Before Activation

- English target language remains non-public until routes, API, search, progress, admin reports, import, and backup all pass with English pilot data.
- Inactive `/learn/en/...` must keep returning 404 until activation.
- Admin diagnostics must show English content counts only after import and must not mix them with German counts.
- Unified Search must return English URLs under `/learn/en/...` for English content and German URLs under `/learn/de/...` for German content.
- User progress must be partitioned by `targetLearningLanguageCode`.
- Same slug in two target languages must be allowed only when target language disambiguates the namespace.
- Any Country Guidance pilot must include country context and must not reuse another country stream as source.

Current post-import evidence:

- English pilot import completed with zero warnings.
- PostgreSQL counts for `en`: `CoursePaths=1`, `CourseModules=1`, `CourseLessons=10`, `GrammarTopics=5`, `ExpressionEntries=20`, `Exercises=8`, `ExerciseSets=1`, `WritingTemplates=5`.
- Public smoke keeps English inactive: `/learn/en/courses` returns 404 while `/learn/de/courses` returns 200.
- Admin report, issue drilldown, preview, and search smoke can inspect English pilot content while public learner routes keep English inactive.
- Restore-ready checkpoint backup for the expanded pilot exists:
  `X:\Projects\DarwinLingua.Backup\20260624-211627-english-a1-expansion-02-branch-checkpoint-pre-activation`.
- Phase 9 decision: English is explicitly kept as pilot/non-public after the expanded checkpoint. Public activation and deeper English generation are future product-expansion decisions, not implicit follow-ups from this import.

## Explicit Non-Goals

- Do not generate broad English packages in this phase.
- Do not make English public active in `TargetLearningLanguageCatalog` yet.
- Do not translate existing German source content into English as English learning content.
- Do not change MAUI/mobile UX.

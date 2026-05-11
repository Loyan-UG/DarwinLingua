# Learning Portal Roadmap And Backlog

## Purpose

This document defines the complete Web-first roadmap for turning Darwin Lingua into a full German-learning portal.

It covers:

- Grammar Guide
- Everyday Expressions, idioms, colloquial phrases, and proverbs
- reusable Exercise Engine
- Course Lessons and structured CEFR learning paths
- Exam Preparation
- Writing Templates and real-life communication support
- Cultural Notes and German-in-Germany guidance
- unified learning search
- learning progress and personalization
- admin and content operations
- Web-first implementation followed by mobile parity
- documentation consolidation rules

This is the single planning source for the learning-portal expansion. Do not create separate roadmap documents for each feature unless implementation proves that a dedicated contract document is required.

---

## Product Direction

Darwin Lingua should become a complete practical German-learning portal, not a collection of isolated features.

The platform should help learners:

- learn vocabulary
- understand grammar
- recognize everyday expressions and idioms
- read and discuss real topics
- practice dialogues and roleplay
- complete structured course lessons
- solve exercises
- prepare for German exams
- write practical German messages and emails
- understand German cultural and communication norms
- find conversation events and practice partners
- track progress across all learning surfaces

The Web platform is the primary implementation surface for this expansion. Mobile parity comes after the Web model, Web API contracts, content model, tests, and content operations are stable.

Existing vocabulary, dialogue, Talk Topic, event, organizer, and conversation content must not be deleted. Existing content may be linked, enriched, corrected, or extended only when needed.

---

## Guiding Principles

### No Throwaway MVP

The first implementation of each module should be production-grade.

That means:

- clean domain model
- import-ready content contract
- Web API support
- Web UI support
- admin visibility or management where appropriate
- tests
- localization
- release validation
- mobile parity backlog
- no hardcoded educational content in Razor views or MAUI screens

### Dynamic Content, Static-Like User Experience

Educational content should be dynamic and content-driven, not hardcoded static pages.

The learner-facing experience may feel like static pages because the layouts are stable and cacheable, but the content must come from structured content records.

Allowed hardcoded static pages:

- legal pages
- privacy/contact/about pages
- small operational help pages

Not allowed as hardcoded static pages:

- grammar lessons
- expression entries
- course lessons
- exercises
- exam-preparation content
- writing templates
- cultural notes

### Content References Instead Of Duplication

New modules should reference existing content where possible.

Examples:

- Grammar topics link to exercises, lessons, dialogues, words, and common mistakes.
- Course lessons link to grammar topics, words, expressions, dialogues, Talk Topics, and exercises.
- Expressions link to words but do not duplicate word meanings.
- Exercises link to grammar/topics/lessons instead of owning all teaching content.

### Web First, Mobile Later

Implementation order:

1. Web domain model and persistence
2. Web API contracts
3. Web UI
4. admin/content operations
5. tests and validation
6. content generation
7. mobile package export
8. MAUI refactor and parity

Mobile should not drive the first model design for this expansion. The Web implementation should establish the complete feature model first.

---

## Documentation Consolidation Policy

The documentation set is already large. For this expansion:

- keep this document as the single roadmap/backlog source
- do not create separate planning docs for Grammar, Expressions, Exercises, Courses, and Exam Prep
- add dedicated content-contract docs only when a module is implemented and needs a stable JSON/import contract
- keep `04-Implementation-Backlog.md` as the historic project-wide execution backlog
- keep `71-Web-Test-Backlog.md` for cross-feature test debt
- prefer updating this document over creating new one-off planning files
- consolidate obsolete compatibility pointer docs when safe

Recommended future cleanup:

- keep core docs: vision, scope, phases, backlog, architecture, domain, bounded contexts, engineering standards, release checklist
- keep content contracts that are actively used by importers
- remove or merge obsolete phase-specific worksheets after their release validation is complete
- avoid duplicate documents that repeat the same roadmap or backlog

---

## Proposed Documentation Changes

Add this file:

- `docs/75-Learning-Portal-Roadmap-And-Backlog.md`

Update:

- `docs/00-Documentation-Index.md`
- `docs/03-Product-Phases.md`
- `docs/04-Implementation-Backlog.md`
- `docs/22-Domain-Model.md`
- `docs/26-Bounded-Contexts.md`
- `docs/71-Web-Test-Backlog.md`

Do not create separate documents for each new module yet. Dedicated contract docs should be created only when the corresponding implementation starts.

---

## Module 1: Grammar Guide

### Product Purpose

Grammar Guide is a structured German grammar reference and teaching section. It explains grammar in the learner's selected language while keeping German examples central.

### Recommended UI Name

- Grammar
- Grammar Guide

### Dynamic Content Model

Suggested aggregate:

- `GrammarTopic`

Suggested child concepts:

- `GrammarSection`
- `GrammarExample`
- `GrammarCommonMistake`
- `GrammarRuleSummary`
- `GrammarExceptionNote`
- `GrammarPrerequisiteLink`
- `GrammarRelatedTopicLink`
- `GrammarLinkedWord`
- `GrammarLinkedDialogue`
- `GrammarLinkedTalkTopic`
- `GrammarLinkedExercise`

### Required Metadata

- slug
- title
- short description
- CEFR level
- grammar category
- tags/topic keys
- supported explanation languages
- publication status
- sort order
- prerequisite grammar topics
- next recommended grammar topics

### Grammar Categories

Initial controlled categories:

- articles
- nouns
- gender
- plural
- pronouns
- verbs
- modal-verbs
- tenses
- separable-verbs
- reflexive-verbs
- cases
- nominative
- accusative
- dative
- genitive
- adjective-declension
- prepositions
- word-order
- subordinate-clauses
- connectors
- negation
- questions
- imperative
- passive
- konjunktiv
- reported-speech
- punctuation

### Page Structure

Each grammar detail page should show:

1. title and CEFR metadata
2. short explanation
3. learner-language explanation
4. German examples
5. example translations where useful
6. mini rules
7. common mistakes
8. exceptions
9. linked words
10. linked dialogues
11. linked Talk Topics
12. linked exercises
13. prerequisites
14. next grammar topics

### Localization Rule

Grammar explanations should support learner-selected language where available.

German examples remain German. Explanations may be shown in:

- UI language
- primary meaning language
- secondary meaning language

The exact display mode should follow the existing dual-language UX rules where appropriate.

---

## Module 2: Everyday Expressions

### Product Purpose

Everyday Expressions teaches idioms, colloquial phrases, informal expressions, fixed phrases, cultural expressions, and proverbs.

This module is separate from Words because expression meaning is often not equal to the literal meaning of the individual words.

### Recommended UI Names

- Expressions
- Everyday Expressions
- German Expressions

### Suggested Aggregate

- `ExpressionEntry`

### Expression Types

Initial controlled types:

- idiom
- colloquial-phrase
- proverb
- fixed-expression
- slang
- cultural-phrase
- false-friend
- regional-expression
- polite-formula
- warning-phrase

### Required Metadata

- slug
- expression text
- literal meaning text where useful
- actual meaning text
- CEFR level
- expression type
- register
- context/category
- topic keys
- region where applicable
- publication status
- sort order

### Register Values

Initial controlled values:

- formal
- informal
- neutral
- colloquial
- slang
- rude
- polite
- workplace-safe
- friends-only
- regional

### Page Structure

Each expression detail page should show:

1. expression
2. type and register
3. CEFR level
4. actual meaning
5. literal meaning where useful
6. usage explanation
7. German example sentences
8. learner-language explanation/translation
9. warnings about tone or context
10. linked words
11. related expressions
12. linked exercises

### Safety Rule

Expressions that are rude, sexual, discriminatory, politically sensitive, or easy to misuse must have warnings and should be filtered or marked clearly.

---

## Module 3: Exercise Engine

### Product Purpose

The Exercise Engine is a reusable practice system. Exercises must be usable independently and also attachable to grammar topics, words, expressions, dialogues, Talk Topics, course lessons, and exam-prep units.

### Suggested Aggregates

- `Exercise`
- `ExerciseSet`
- `ExerciseAttempt`

### Exercise Types

Initial supported types:

- multiple-choice
- fill-in-the-blank
- matching
- sentence-ordering
- error-correction
- article-selection
- case-selection
- conjugation
- translation-controlled
- dialogue-completion
- vocabulary-choice
- grammar-choice

Future types:

- dictation
- listening-comprehension
- pronunciation-shadowing
- writing-prompt
- speaking-prompt
- AI-assisted-feedback

### Required Metadata

- slug
- title
- instruction
- CEFR level
- exercise type
- target skill
- linked content owner type
- linked content owner slug/id
- answer key
- feedback model
- publication status
- sort order

### Target Skills

Initial values:

- grammar
- vocabulary
- reading
- listening
- speaking
- writing
- pronunciation
- exam-preparation

### Feedback Requirements

Every exercise must support deterministic feedback before any AI-based feedback is considered.

Required:

- correct/incorrect state
- short explanation
- optional hint
- optional common mistake note

### Relationship Rules

Exercises should link to content, not duplicate teaching material.

Examples:

- a grammar exercise links to `GrammarTopic`
- a dialogue-completion exercise links to `DialogueLesson`
- an idiom exercise links to `ExpressionEntry`
- a course lesson contains references to exercise sets

---

## Module 4: Course Lessons And Learning Paths

### Product Purpose

Course Lessons provide book-like structured learning paths by CEFR level. Lessons combine grammar, vocabulary, expressions, dialogues, Talk Topics, and exercises.

### Recommended UI Names

- Courses
- Learning Path
- Lessons

### Suggested Aggregates

- `CoursePath`
- `CourseModule`
- `CourseLesson`

### Course Structure

Example:

- A1 German
  - A1.1
  - A1.2
- A2 German
- B1 German
- B2 German
- C1 German
- C2 German

### CourseLesson Required Sections

Each lesson should support:

1. lesson title
2. CEFR level
3. module/unit number
4. learning goals
5. short explanation
6. linked grammar topics
7. new word references
8. linked expressions
9. linked dialogue lessons
10. linked Talk Topics
11. linked exercise sets
12. review summary
13. homework/practice task
14. next lesson link

### No Duplication Rule

Course lessons should not copy full grammar articles, word meanings, or expression explanations. They should reference them and provide a short lesson narrative.

### Lesson Metadata

- slug
- course key
- module key
- CEFR level
- lesson number
- estimated minutes
- learning goals
- prerequisite lesson slugs
- publication status
- sort order

---

## Module 5: Exam Preparation

### Product Purpose

Exam Prep helps learners prepare for recognized German exams and exam-like speaking/writing tasks.

### Recommended UI Name

- Exam Prep

### Initial Exam Profiles

- Goethe A1
- Goethe A2
- Goethe B1
- Goethe B2
- telc A1
- telc A2
- telc B1
- telc B2
- DTZ A2-B1
- Berufssprache B1
- Berufssprache B2
- C1 Hochschule
- TestDaF

### Suggested Aggregates

- `ExamProfile`
- `ExamSection`
- `ExamTaskType`
- `ExamPrepUnit`
- `MockExam`

### ExamPrepUnit Should Link To

- dialogues
- Talk Topics
- grammar topics
- expressions
- writing templates
- exercises
- course lessons

### Exam Prep Features

- exam overview
- task explanations
- sample speaking tasks
- sample writing tasks
- useful phrases
- scoring checklist
- mock exam placeholders
- linked practice sets

---

## Module 6: Writing Templates

### Product Purpose

Writing Templates help learners produce practical German texts for real-life situations.

### Initial Template Categories

- email-to-school
- email-to-kindergarten
- message-to-landlord
- doctor-appointment-request
- appointment-reschedule
- sick-note-to-employer
- complaint
- application-email
- cancellation
- insurance-message
- government-office-message

### Suggested Aggregate

- `WritingTemplate`

### Required Fields

- slug
- title
- CEFR level
- category
- situation
- register
- template text
- explanation
- replaceable variables
- sample filled version
- linked grammar topics
- linked words
- linked exercises

---

## Module 7: Cultural Notes

### Product Purpose

Cultural Notes teach German communication behavior, social expectations, and context-specific language use.

### Initial Categories

- du-vs-sie
- politeness
- directness
- small-talk
- workplace-culture
- office-communication
- school-kindergarten
- doctor-visit
- appointments
- punctuality
- complaints
- bureaucracy
- conversation-cafe-etiquette

### Suggested Aggregate

- `CulturalNote`

### Relationship Rules

Cultural Notes may link to:

- dialogues
- grammar topics
- expressions
- Talk Topics
- writing templates
- course lessons

---

## Module 8: Unified Learning Search

### Product Purpose

The learner should be able to search across the full learning portal.

Search targets:

- words
- grammar topics
- expressions
- dialogues
- Talk Topics
- exercises
- course lessons
- exam prep units
- writing templates
- cultural notes
- events and organizers where appropriate

### Search Result Model

Suggested fields:

- result type
- title
- short snippet
- CEFR level
- category/topic
- URL
- relevance score
- matched term information where useful

### Search Requirements

- search by German term
- search by learner-language meaning where applicable
- filter by CEFR level
- filter by content type
- filter by category/topic
- preserve clear ranking rules

---

## Module 9: Progress And Personalization

### Product Purpose

The portal should track learner progress across content types without mixing user state into content entities.

### Suggested Concepts

- `UserContentProgress`
- `UserLessonProgress`
- `UserExerciseAttempt`
- `UserGrammarTopicState`
- `UserExpressionState`
- `UserCourseProgress`

### Progress States

- not-started
- viewed
- in-progress
- completed
- needs-review
- skipped

### Personalization Features

- current CEFR focus
- recommended next lesson
- weak grammar topics
- difficult words
- saved expressions
- recent learning activity
- exam preparation target

---

## Complete Roadmap

### Phase 7.1: Learning Portal Foundation

- [ ] define shared learning portal navigation structure
- [ ] add content-type taxonomy for Grammar, Expressions, Exercises, Courses, Exam Prep, Writing Templates, and Cultural Notes
- [ ] define cross-content linking model
- [ ] define reusable linked-word reference model
- [ ] define unified CEFR/category/topic filter conventions
- [ ] define Web-first/mobile-later parity rule in docs
- [ ] add admin/system report placeholders for new content families
- [ ] add test backlog entries for all new modules

### Phase 7.2: Grammar Guide

- [ ] implement `GrammarTopic` domain model
- [ ] implement grammar child sections/examples/common mistakes/rule summaries
- [ ] add EF Core mappings and migration
- [ ] add import parser and validation support
- [ ] add Web API list/detail endpoints
- [ ] add Web list/detail pages
- [ ] add filters by CEFR/category/topic
- [ ] add linked words/dialogues/Talk Topics/exercises support
- [ ] add admin visibility or management surface
- [ ] add tests for import, validation, query, Web API, and Web rendering
- [ ] document grammar content contract only when implementation starts

### Phase 7.3: Everyday Expressions

- [ ] implement `ExpressionEntry` domain model
- [ ] implement expression examples, warnings, related expressions, linked words
- [ ] add EF Core mappings and migration
- [ ] add import parser and validation support
- [ ] add Web API list/detail endpoints
- [ ] add Web list/detail pages
- [ ] add filters by CEFR/type/register/context/topic
- [ ] add safety warnings for risky expressions
- [ ] add admin visibility or management surface
- [ ] add tests for import, validation, query, Web API, and Web rendering
- [ ] document expression content contract only when implementation starts

### Phase 7.4: Exercise Engine

- [ ] implement `Exercise` and `ExerciseSet` models
- [ ] support initial exercise types
- [ ] support deterministic answer keys and feedback
- [ ] support links to grammar, words, expressions, dialogues, Talk Topics, and lessons
- [ ] add learner exercise attempts
- [ ] add Web API endpoints
- [ ] add Web exercise runner UI
- [ ] add admin/import support
- [ ] add tests for each initial exercise type
- [ ] document exercise content contract only when implementation starts

### Phase 7.5: Course Lessons And Learning Paths

- [ ] implement `CoursePath`, `CourseModule`, and `CourseLesson`
- [ ] support CEFR-level course tracks
- [ ] link lessons to grammar, words, expressions, dialogues, Talk Topics, and exercises
- [ ] add Web course/lesson pages
- [ ] add lesson progress tracking
- [ ] add admin/import support
- [ ] add tests for lesson queries, linked content, and progress
- [ ] document course content contract only when implementation starts

### Phase 7.6: Exam Preparation

- [ ] implement exam profile taxonomy
- [ ] implement exam prep unit model
- [ ] link exam prep to dialogues, exercises, grammar, writing templates, and Talk Topics
- [ ] add Web exam prep pages
- [ ] add exam-specific filters and landing pages
- [ ] add tests for exam profile linking and filtering

### Phase 7.7: Writing Templates

- [ ] implement writing template model
- [ ] support template variables and sample filled versions
- [ ] link to grammar, words, expressions, and exercises
- [ ] add Web list/detail pages
- [ ] add tests for template rendering and linked content

### Phase 7.8: Cultural Notes

- [ ] implement cultural note model
- [ ] add Web list/detail pages
- [ ] link to dialogues, expressions, writing templates, and course lessons
- [ ] add tests for filtering and rendering

### Phase 7.9: Unified Learning Search

- [ ] define unified search result model
- [ ] implement cross-content search endpoint
- [ ] add Web search page/tab for all learning content
- [ ] support filters by CEFR/content type/category/topic
- [ ] add tests for ranking and result projection

### Phase 7.10: Progress And Personalization

- [ ] implement cross-content progress model
- [ ] track viewed/completed states for grammar, expressions, lessons, and exercises
- [ ] extend learner dashboard/recent activity
- [ ] add recommendations based on CEFR focus and weak areas
- [ ] add tests for progress state transitions

### Phase 7.11: Admin And Operations

- [ ] add admin content overview for new modules
- [ ] add admin import and validation summaries
- [ ] add content quality reports
- [ ] add reports for missing links, unresolved words, missing exercises, missing translations, and unpublished drafts
- [ ] add seed coverage reports per CEFR/module

### Phase 7.12: Mobile Parity

- [ ] define mobile navigation update after Web sign-off
- [ ] update mobile content package export for grammar, expressions, exercises, courses, exam prep, writing templates, and cultural notes
- [ ] refactor MAUI list/detail pages to match Web content model
- [ ] add mobile exercise runner where appropriate
- [ ] add mobile progress sync where account support is ready
- [ ] add mobile validation worksheets after Web implementation is stable

---

## First Implementation Order

Recommended build order:

1. Learning Portal Foundation
2. Grammar Guide
3. Everyday Expressions
4. Exercise Engine
5. Course Lessons
6. Exam Preparation
7. Writing Templates
8. Cultural Notes
9. Unified Search
10. Progress and personalization
11. Mobile parity
12. Content generation for new modules

Content generation for new modules should start only after the module contracts, validation rules, and Web rendering are stable.

---

## Release Readiness Rules

A new learning-portal module is not release-ready until:

- domain model is implemented
- EF migration exists
- import validation exists
- Web API list/detail endpoints exist
- Web list/detail pages exist
- admin visibility exists where operationally needed
- user-facing strings are localized
- linked content behavior is tested
- unresolved references fail safely
- tests cover import, query, rendering, and validation
- docs are updated
- mobile parity is explicitly completed or backlog-tracked

---

## Codex Prompt Plan

After this roadmap is accepted, implementation prompts should be produced in this order:

1. Learning Portal Foundation prompt
2. Grammar Guide implementation prompt
3. Everyday Expressions implementation prompt
4. Exercise Engine implementation prompt
5. Course Lessons implementation prompt
6. Exam Prep implementation prompt
7. Writing Templates implementation prompt
8. Cultural Notes implementation prompt
9. Unified Search implementation prompt
10. Progress/personalization implementation prompt
11. Mobile parity prompt
12. Grammar content generation prompt
13. Expressions content generation prompt
14. Exercise content generation prompt
15. Course lesson content generation prompt
16. Exam prep content generation prompt
17. Writing template content generation prompt
18. Cultural notes content generation prompt

Do not generate bulk content before the corresponding implementation and validation rules exist.

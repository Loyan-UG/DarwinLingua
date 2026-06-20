# Content Generation Lessons Learned

## Purpose

This document records content-generation mistakes, prompt-design rules, and validation lessons that must be checked before producing or importing new learning content.

Use it together with the active content contract for the module being generated. Add new lessons here when a generation, import, rendering, localization, or QA problem is found so future batches do not repeat the same failure.

## Global Content Generation Rules

- Treat each module syllabus or contract as the source of truth for slugs, titles, categories, package paths, and ordering.
- Generate from reviewed prompts only. Do not use ad hoc prompts for official bulk content.
- Keep generated content in the target educational language. Do not allow English fallback inside non-English localized fields except for German examples, German grammar terms, or controlled technical labels.
- Prefer specific, local explanations over repeated generic safety or learner-background text.
- Do not ask the model to produce very high counts without validation. Large example, rule, or mistake counts need duplicate and language-quality checks.
- Validate structure, imports, API projection, and Web rendering before accepting content as official.
- Before any bulk prompt rewrite or content generation pass, create and verify a restorable backup.

## Grammar Guide Lessons

### Syllabus And Prompt Alignment

- The Grammar Guide syllabus file is the source of truth: `content/learning-portal/grammar/syllabus/grammar-syllabus-a1-c2-v1.json`.
- Prompt slugs must match syllabus slugs exactly. Near matches such as `b1-connectors-for-cause-effect` versus `b1-connectors-for-cause-and-effect` must be normalized before generation.
- Prompt titles, CEFR levels, and `grammarCategory` values must match the syllabus exactly.
- Prompt file numbering must follow syllabus order. The current global order is A1 1-35, A2 36-75, B1 76-120, B2 121-165, C1 166-200, and C2 201-225.
- Missing prompts must be added before content regeneration starts. Do not compensate for missing prompts by generating topics from memory.

### Localization Mistakes To Avoid

- Do not address the reader as a native-language group. Avoid phrases such as "Persian speakers", "Arabic learners", "Turkish learners", and "for Persian speakers" in generated explanations.
- Comparisons may mention a language system directly when useful, for example "In Persian ..." or "In Turkish ...", but this should be rare and specific.
- Do not repeat the same learner-background sentence across sections, rule summaries, and common mistakes.
- Common mistake explanations must explain the exact wrong/correct pair. Empty repetition is worse than a short specific explanation.
- Kurdish Sorani, Kurdish Kurmanji, Persian, Arabic, Turkish, Russian, Polish, Romanian, and Albanian content must be written in the requested language rather than filled with English.
- Section headings are learner-facing text. Do not leave section headings only in English when the body blocks are localized; add section `translations` for every target language so navigation and `<h3>` headings localize correctly.

### Grammar JSON Contract Lessons

- Use only supported rich block types from `77-Grammar-Content-Package-Contract.md`: `paragraph`, `table`, `callout`, `rule-list`, `example-list`, `mistake-pair`, and `image-slot`.
- `ruleSummaries` must be localized. Prefer `localizedText` keyed by learner language code for each rule summary item.
- Each section must include localized `translations` with `language`, `heading`, and `text` for every target language. `localizedBlocks` alone localizes the body but not the section heading used by the API and Web UI.
- `examples` must include `germanText` and all target-language translations.
- `commonMistakes` must include localized explanations for all target languages.
- `linkedWords` are references only; do not include meanings or translations there.
- `prerequisiteSlugs` and `relatedTopicSlugs` must use exact existing syllabus slugs.

### Import And Rendering Lessons

- Importer support must be verified for every JSON shape used by generated content. A mismatch between generated `localizedText` fields and importer support caused localized rule summaries to fall back incorrectly.
- Grammar detail pages must render rich section blocks vertically and semantically. Paragraphs and callouts inside a section must stack unless the block type intentionally defines a table/grid.
- Web rendering should be checked with real generated content before a batch is accepted.
- Prompt and content tooling must verify backup file counts before removing or rewriting prompt files.
- `example-list` rich blocks must use a non-empty `items` array. A property named `examples` may look reasonable in generated JSON, but the importer rejects it.
- Folder imports must exclude archived validation packages and `*.proposed.json` drafts. Archive/proposal files can be useful for review, but they are not official content packages and must not be imported with production core packages.

## Validation Checklist Before Regeneration

- Prompt audit returns zero unmatched prompts, missing syllabus topics, duplicate slugs, category mismatches, and guardrail omissions.
- Every prompt contains the anti-boilerplate localization rules.
- Every prompt points to the correct package for its CEFR level.
- A small sample generation is imported and rendered before regenerating the whole level.
- Duplicate-sentence checks run on localized sections, rule summaries, and common mistake explanations.
- Language-script and fallback checks run on all non-English localized fields.
- Web detail pages are smoke-tested for layout, tables, examples, rule summaries, common mistakes, prerequisites, and related topics.

## Conversation Content Lessons

- Run the conversation audit before generating new Dialogues, Talk Topics, Conversation Starter Packs, Event Preparation Packs, or Roleplays. The current audit tool is `tools/Content/Audit-ConversationContent.js`.
- Do not delete existing Dialogues, Talk Topics, vocabulary, event, organizer, or conversation content during cleanup. Repair only malformed JSON, malformed references, obvious metadata errors, and duplicate references that do not carry unique content.
- `linkedWords`, `usefulWords`, and Talk Topic vocabulary references must stay references only. Do not duplicate word meanings inside conversation content.
- Normalize malformed word references before content generation. The repair tool `tools/Content/Repair-ConversationContentLinks.js` currently fixes known nominalized-verb reference mismatches such as `das-essen` to `essen`, then deduplicates repeated useful-word references inside each Dialogue.
- Conversation Starter and Event Preparation content can be generated now because parser, import, persistence, Web/API surfaces, and tests exist. Use small baseline/gap-fill packages first, not blind bulk generation.
- Standalone RoleplayScenario content may only be generated as reviewed small batches after parser/import/persistence/Web/search/admin/test support is green. The first A1-B2 pilot package passed those gates on 2026-05-25; bulk roleplay generation remains blocked.
- After any repair, rerun the conversation audit and check that P0 blockers, unresolved Dialogue word references, unresolved Talk Topic references, and contract issue counts remain zero.

## Maintenance Rule

When a content-quality problem is found, add a short note here with:

- what failed
- where it appeared
- why it happened
- the prompt, importer, validation, or rendering rule that prevents it next time

## Phase Completion Rule

- After a content phase is accepted, update the roadmap, test backlog, release checklist, and operations notes before starting the next phase.
- Create and verify an external phase backup under `X:\Projects\DarwinLingua.Backup` after docs are updated and before the next content phase begins.
- The backup must include the shared PostgreSQL logical dump, non-Git source artifacts, validation/planning artifacts, and a separate local config/secret bundle so GitHub plus the backup can restore the exact checkpoint.

## Logged Content Quality Issues

### 2026-05-19: Repeated section explanations in generated Grammar Guide topics

- What failed: Some generated grammar lessons reused the same localized section explanation across many different section keys. Tables differed, but learner-facing paragraphs repeated too often.
- Where it appeared: The issue was confirmed in `b2-advanced-relative-clauses` and detected by audit in several recently generated B1 lessons.
- Why it happened: The generation helper used a small set of generic body templates and rotated them across sections to satisfy structure/count requirements, but did not enforce duplicate-sentence quality checks before import.
- Prevention rule: Run `tools/Content/Test-GrammarContentQuality.ps1` on every newly generated grammar package/topic before import. A generated topic is not accepted when localized section/paragraph text or common-mistake explanations repeat beyond the configured threshold.

### 2026-05-20: `example-list` block used unsupported `examples` property

- What failed: A generated Grammar Guide topic used `example-list` blocks with an `examples` property instead of the contract/importer-required `items` array.
- Where it appeared: Importing `b2-expressing-criticism-politely` failed before the block shape was corrected.
- Why it happened: The pre-import quality gate checked duplicate text and localization boilerplate, but did not validate rich-block field names against the importer contract.
- Prevention rule: `tools/Content/Test-GrammarContentQuality.ps1` now checks `example-list.items`, table columns/rows, and callout text before import.

### 2026-05-20: Generated German examples need grammar sanity checks beyond JSON quality

- What failed: A generated comparison topic initially produced structurally valid examples with wrong German case or agreement, such as `für der Zug` and singular agreement after paired subjects.
- Where it appeared: The issue was caught during pre-import review of `b2-comparing-options-grammatically`.
- Why it happened: The generator reused nominative noun chunks inside accusative prepositional frames and did not separate sentence-shape validation from JSON contract validation.
- Prevention rule: Before importing generated Grammar Guide topics, inspect representative German examples for case, article, agreement, pronoun reference, and verb placement. JSON quality gates are necessary but not sufficient.

### 2026-05-20: Internal QA checkpoint text leaked into learner-facing content

- What failed: Internal generation instructions such as `Checkpoint for ... do not continue until the German example works with the rule named in this section` were written into localized `callout` blocks and `rule-list` items.
- Where it appeared: The issue was confirmed in several B1 review topics, including `b1-b1-formal-versus-informal-grammar`.
- Why it happened: A generation helper used learner-facing rich blocks to store a QA reminder that should have stayed inside validation logic or developer-only tooling.
- Prevention rule: Never put process instructions, self-check commands, importer notes, or QA reminders into `sections`, `localizedBlocks`, `examples`, `ruleSummaries`, or `commonMistakes`. `tools/Content/Test-GrammarContentQuality.ps1` now rejects known checkpoint phrases across all target languages.

### 2026-05-20: Duplicate common-mistake items inflated quality counts

- What failed: Some topics appeared to meet high common-mistake count requirements only because the same wrong/correct pair was repeated several times, often once per target language loop.
- Where it appeared: The issue was confirmed in B1 topics such as `b1-damit-versus-um-zu`, `b1-um-zu`, `b1-infinitive-with-zu`, `b1-passive-voice-introduction`, and `b1-werden-as-auxiliary`.
- Why it happened: The generation helper appended common mistakes inside a localization loop instead of creating one mistake item with localized explanations for all languages.
- Prevention rule: Common mistakes are language-independent items with localized explanations. A topic must not repeat the same `wrongText`/`correctedText` pair to satisfy count requirements. Run `tools/Content/Test-GrammarContentQuality.ps1` and inspect low post-deduplication counts before accepting a package.

### 2026-05-20: Automated repair must not hide content loss

- What failed: Removing duplicated learner-facing text can lower example or mistake counts when duplicates were previously masking missing content.
- Where it appeared: After duplicate cleanup, several B1 topics needed new distinct mistake pairs to keep B1-level coverage.
- Why it happened: A cleanup pass can make the JSON technically cleaner while exposing that a lesson was underdeveloped.
- Prevention rule: After every cleanup, check per-topic counts and coverage again. A clean duplicate gate is necessary but not sufficient; repaired lessons must still meet the CEFR-level minimums and must retain realistic examples and mistake coverage.

### 2026-05-21: Folder import included archived/proposed grammar files

- What failed: Importing the grammar package folder also picked up archived validation JSON and a `.proposed.json` pilot file, producing warnings unrelated to the official core grammar packages.
- Where it appeared: Importing `content/learning-portal/grammar/packages` included `archive/validation/grammar-a1-pilot-personal-pronouns-v1.proposed.json`.
- Why it happened: The import tool enumerated `*.json` recursively without excluding archive folders or draft/proposed packages.
- Prevention rule: Folder import selection now excludes any path under an `archive` directory and any filename ending in `.proposed.json`. Official bulk imports should report the expected core package count before content is accepted.

### 2026-05-21: Conversation audit blockers must be repaired before gap-fill generation

- What failed: The post-Grammar conversation audit found three generated JSON files with raw control characters and 733 unresolved Talk Topic vocabulary references.
- Where it appeared: `content/generated/de-b2-generated-batch-071.json`, `content/generated/de-b2-generated-batch-073.json`, `content/generated/de-b2-generated-batch-106.json`, and Talk Topic linked-word references.
- Why it happened: Earlier generated content allowed malformed string content and near-match word references through without a full cross-package audit.
- Prevention rule: Run `node tools/Content/Audit-ConversationContent.js --report artifacts/validation/conversation-content-audit-report.md` after any Conversation repair or generation. Do not start new Conversation bulk generation unless P0 blockers and unresolved word references are zero.

### 2026-05-21: Duplicate Dialogue usefulWords can hide noisy content quality

- What failed: Many Dialogue packages repeated the same useful-word reference inside a single Dialogue, inflating link counts without adding learner value.
- Where it appeared: Dialogue expansion packages under `content/generated/dialogues`.
- Why it happened: Generation appended useful-word references repeatedly across turn/template loops.
- Prevention rule: Treat `usefulWords` as a deduplicated reference list per Dialogue. Repair duplicate references mechanically, but do not delete Dialogue turns or learner-facing content during reference cleanup.

### 2026-05-21: Do not generate standalone RoleplayScenario content before infrastructure exists

- What failed: The roadmap called for Roleplay gap-fill after Conversation audit, but the repository currently only supports `roleplayPrompts` inside Event Preparation Packs. There is no dedicated RoleplayScenario parser/import/persistence path yet.
- Where it appeared: `docs/70-Roleplay-Content-Package-Contract.md` defines the desired contract, while code search found no standalone RoleplayScenario importer.
- Why it happened: The contract was documented ahead of full implementation.
- Prevention rule: Implement parser, application validation, persistence, Web API, Web rendering, search/admin visibility, and tests before generating standalone roleplay packages. After those gates pass, create only reviewed small pilot packages, import them into the shared development database, run Web/API/search/admin smoke, and keep Dialogue-derived Roleplay and Event Preparation `roleplayPrompts` separate from standalone `RoleplayScenario` content.

### 2026-05-23: Expression list pages must use learner-language meanings

- What failed: The Everyday Expressions detail endpoint localized meanings by requested learner language, but the list endpoint projected the base `actualMeaningText`, which is normally English.
- Where it appeared: The first real Expressions pilot review showed that `/expressions` would show English meanings even when the learner profile preferred another meaning language.
- Why it happened: The list query did not accept or forward `primaryMeaningLanguageCode`, while detail already did.
- Prevention rule: Expression list requests now carry `primaryMeaningLanguageCode`; repository tests verify localized list projection. New learning modules must check list and detail localization, not only detail pages.

### 2026-05-23: Provider-specific search functions need production-provider coverage

- What failed: Early Expression repository/search tests mixed PostgreSQL-style search behavior with SQLite-backed local fixtures.
- Where it appeared: `GetPublishedExpressionsAsync` and `UnifiedLearningSearchRepository.SearchExpressionsAsync` failed in SQLite-backed tests.
- Why it happened: The Web/API production path is PostgreSQL, but some legacy/local tests used SQLite and created pressure to make production queries provider-neutral.
- Prevention rule: Do not weaken Web/API production search semantics to satisfy SQLite-backed tests. For Web/API module search, keep PostgreSQL-native behavior and add PostgreSQL integration tests. SQLite-backed fixtures may remain only for mobile/local surfaces where SQLite is the actual runtime store and they do not define Web/API production behavior.

### 2026-05-31: Exercise helper text is content, not UI chrome

- What failed: The first Exercise package imported and rendered, but Exercise title, instruction, hint, and feedback explanation had no learner-language helper translations.
- Where it appeared: Exercise list/detail/runner pages could only show German source and UI-localized chrome, not the learner's selected content language.
- Why it happened: Exercise was treated as deterministic practice infrastructure first, while multilingual content projection was deferred.
- Prevention rule: Exercise packages must stay German-first, but reviewed packages need translations for `titleTranslations`, `instructionTranslations`, `correctExplanationTranslations`, `incorrectExplanationTranslations`, `hintTranslations`, `commonMistakeNoteTranslations`, and ExerciseSet title/description translations before import. API/Web must project helper text by selected meaning language while never replacing German source.

### 2026-05-31: Course lessons need translated helper text before content grows

- What failed: The Course contract existed before the first real pilot, but its learner-facing title, description, narrative, learning-goal, review, and homework fields initially had no learner-helper translation fields.
- Where it appeared: Course planning after Exercise Engine completion.
- Why it happened: Course Lessons were modeled as orchestration links first, but those orchestration fields are still learner-facing content and must follow the same German-first plus learner-helper rule as Roleplay and Exercise.
- Prevention rule: Course packages must keep source `title`, `description`, `shortDescription`, `narrative`, `learningGoals`, `reviewSummary`, and `homeworkTask` in German, and must include active learner-language helper translations before import. `learningGoalsTranslations` must contain one translated item per German learning goal.

### 2026-05-23: Expressions need a dedicated pre-import content quality gate

- What failed: The Expression contract had structural validation, but no focused pilot content gate for English fallback leakage, repeated generic explanations, internal QA text, warning localization, or linked-word meaning leaks.
- Where it appeared: The first real Expressions pilot added `tools/Content/Validate-ExpressionPilot.js` before accepting the package.
- Why it happened: Parser/import validation intentionally validates contract shape and controlled values; it does not prove learner-facing localization quality.
- Prevention rule: Run `node tools/Content/Validate-ExpressionPilot.js content/learning-portal/expressions/packages/expressions-a1-a2-core-pilot-v1.json` before importing or expanding Expressions content. Bulk Expressions generation remains blocked until the pilot passes this gate plus import, Web/API, search, and admin validation.

### 2026-05-23: Production-like PostgreSQL schema drift must be validated before enabling pages

- What failed: The `/expressions` page called the Web API successfully, but the API returned 500 in production because the existing PostgreSQL catalog database did not contain the newer `ExpressionEntries` table.
- Where it appeared: `https://lingua.vafadar.pro/expressions` failed when `/api/catalog/expressions?isRisky=False&primaryMeaningLanguageCode=fa` queried the missing table.
- Why it happened: Local tests covered parser/import/query behavior and SQLite initialization, but did not prove that an already-existing PostgreSQL database would be retrofitted with the new module schema at startup.
- Prevention rule: For every new Learning Portal module, validate an existing PostgreSQL database path, not only clean SQLite tests. If the server uses startup retrofit schema, add an idempotent PostgreSQL retrofit plus tests/structural checks before exposing public Web routes.

### 2026-05-24: Admin reports must tolerate not-yet-created module tables

- What failed: The Everyday Expressions learner/API smoke passed, but `/api/admin/catalog/system-report` returned 500 on the shared PostgreSQL database because other Phase 7 tables such as `Exercises`, `WritingTemplates`, and `CulturalNotes` were not present yet.
- Where it appeared: The local target/dev smoke for the Expressions pilot failed at the admin report endpoint after the pilot import.
- Why it happened: The admin report assumed every planned Learning Portal module table existed and also started multiple queries on the same `DbContext` concurrently.
- Prevention rule: Admin quality reports must treat missing optional module tables as empty coverage, not as fatal errors. Query a single `DbContext` sequentially or use separate contexts for parallel work. Add tests that drop a future-module table and verify the report still returns counts for already-imported modules.

### 2026-05-24: Local Web smoke must override late-loaded local settings deliberately

- What failed: A local Web smoke run still called the public API even after setting `WebApi__BaseUrl=http://localhost:5099`.
- Where it appeared: `DarwinLingua.Web` loaded `appsettings.Development.Local.json` after the default environment variables, so the public API URL in that file won during Development smoke.
- Why it happened: The smoke process used the normal Development environment without accounting for the repository's late-loaded local settings file.
- Prevention rule: For local smoke against a temporary WebApi instance, either use a dedicated non-Development environment with explicit env overrides or update the local-only config deliberately. Do not trust route smoke until logs show the Web client is calling the intended API origin.

### 2026-05-24: Expression topic keys must match the active Catalog topics

- What failed: The first import attempt for `expressions-a1-a2-core-01-v1.json` failed because several `topics` values used natural planning labels such as `phone-calls`, `appointments`, `work`, and `public-office` that are not active Catalog topic keys.
- Where it appeared: `DarwinLingua.ImportTool` rejected the new Expressions batch before writing it to `darwinlingua_shared`.
- Why it happened: The content contract validates expression shape and localization, but topic references are resolved against the current `Topics` table. Content-generation labels must not be invented from prompt wording.
- Prevention rule: Before importing a new Expressions batch, compare all `topics` values with the active Catalog topic keys, or omit optional topics when the `category` already carries the context. Import validation must keep rejecting unknown topic keys.

### 2026-05-24: Expressions must not become an ordinary sentence bank

- What failed: The first Everyday Expressions content path accepted ordinary literal sentences that were technically valid JSON but pedagogically belonged in Dialogues, Courses, Exercises, Writing Templates, or Grammar examples.
- Where it appeared: Everyday Expressions pilot and small-batch content included practical literal sentences such as problem reports and appointment statements.
- Why it happened: The Expression contract did not distinguish ordinary literal sentences from idioms, pragmatic formulas, non-literal expressions, cultural phrases, and false friends.
- Prevention rule: Every new Expression entry needs an eligibility classification. Reject published `ordinary-literal`, require literal versus actual meaning for `non-literal` and `semi-idiomatic`, require a `teachingReason`, require at least two contextual German examples for classified official content, and run `node tools/Content/Audit-ExpressionContentQuality.js` before accepting another batch. The gate must report zero issues before any next small Expressions batch is generated.

### 2026-05-24: Expression examples must not use generic meaning-only translations

- What failed: A generated B2 Expressions batch initially used structurally valid but too-generic example translations and repeated usage text. The package passed structural validation, but the learner-facing examples did not explain the German sentence context well enough.
- Where it appeared: `expressions-b2-core-03-v1.json` during pre-final smoke review of the Persian detail API projection.
- Why it happened: The generator treated localized example translations as reusable meaning labels instead of contextual learner-facing translations. The quality gate caught missing fields and English fallback, but not this softer pedagogical issue.
- Prevention rule: For future Expressions batches, review at least two detail API samples in a non-English learner language before final import sign-off. Example translations must either translate the German sentence context naturally or explicitly state that they explain the idiomatic use in that example. Repeated usage text should be varied by context, category, and tone.

### 2026-05-24: Adult-language learning content needs access state, not a checkbox-only shortcut

- What failed: Expressions planning initially treated `+18` as a simple profile flag, which is not enough for explicit adult or pornographic content in Germany.
- Where it appeared: Everyday Expressions adult/sensitive content planning.
- Why it happened: The content contract had safety ratings, but profile/access-state requirements were not explicit enough for future adult-language generation.
- Prevention rule: Store a separate adult-content access state (`not-requested`, `self-declared-adult`, `age-verified-adult`, `blocked`) and keep rude/slang preference separate from explicit adult access. `self-declared-adult` must not unlock explicit adult/pornographic content in production. Explicit adult content generation remains blocked until legal review, approved age verification, list/detail/search filtering, admin reporting, and release gates are all verified.

### 2026-05-25: Sensitive educational language needs metadata and opt-in before generation

- What failed: Future Expressions planning risked mixing educational rude/slang/romantic language with explicit adult content under a broad `+18` label.
- Where it appeared: Everyday Expressions sensitive-language planning.
- Why it happened: The product vocabulary did not clearly separate Sensitive Educational Language for comprehension from pornographic or legally restricted adult content.
- Prevention rule: Use the product term Sensitive Educational Language for warning-labeled educational rude/slang/romantic/social content. Do not generate sensitive batches until metadata, warnings, opt-in filtering, admin reports, registration/legal notice coverage, mobile export exclusion, and content quality gates exist. A simple profile checkbox is a reversible learning preference, not age verification. Do not generate pornographic, arousing, graphic, exploitative, coercive, minor-related, hate-inciting, Nazi-propaganda, or illegal content.

### 2026-05-26: RoleplayScenario metadata must be German-first and learner-language aware

- What failed: The first standalone RoleplayScenario pilot used English `title`, `description`, and `learnerGoal` values even though these are primary learner-facing content fields. The Web/API detail path could localize turns and choices, but the top-level metadata still appeared in English for users whose selected learner language was Persian.
- Where it appeared: `/roleplays` and `/roleplays/{slug}` for the first standalone RoleplayScenario pilot.
- Why it happened: The RoleplayScenario contract and persistence model initially treated top-level metadata as plain strings and did not require `titleTranslations`, `descriptionTranslations`, or `learnerGoalTranslations`.
- Prevention rule: Official RoleplayScenario packages must keep source `title`, `description`, and `learnerGoal` in German and include learner-language translations for active meaning languages. Web/API projection must show the German source plus the user's selected learner-language translation and must not fall back to English when another learner language is selected.

### 2026-05-26: Roleplay answer choices must teach weaker alternatives, not repeat or distract

- What failed: The Roleplay detail page repeated the correct scripted learner answer under `Choose a response`, then often showed an obviously irrelevant wrong option such as `Ich weiss nicht.`. This did not help learners understand why a plausible answer would lose points or sound incomplete.
- Where it appeared: Answer-choice moments in the first standalone RoleplayScenario pilot.
- Why it happened: The content generation prompt focused on satisfying the deterministic answer-choice contract instead of defining the learner-facing purpose of wrong choices. The UI then rendered every choice, including the already visible model answer.
- Prevention rule: Keep a correct choice in JSON for deterministic validation, but learner-facing UI should emphasize weaker/not-recommended alternatives and static feedback. Incorrect choices must be plausible but insufficient for the scenario: too short, missing a reason, too direct for the register, unclear, or incomplete. Do not use irrelevant filler such as `Ich weiss nicht.` as the default wrong answer.

### 2026-05-26: Roleplay title planning must be persisted before generation

- What failed: Roleplay generation moved too quickly from infrastructure validation to full pilot content, which made it easier to miss title suitability, duplicate coverage, and CEFR progression issues.
- Where it appeared: The first RoleplayScenario pilot replacement and the A1 planning pass.
- Why it happened: There was no persistent reviewed title backlog for standalone RoleplayScenario content.
- Prevention rule: Before generating full RoleplayScenario content, create or update a level-specific title backlog under `artifacts/planning/`. For A1, use `artifacts/planning/roleplay-a1-title-candidates.md` as the source for small-batch selection, and mark already imported titles to avoid duplicate generation.

### 2026-05-26: Roleplay packages need a pre-import required-field check

- What failed: The first A1 RoleplayScenario core batch initially omitted `cefrLevel` on all generated scenarios. Import validation rejected the file before writing rows, but the issue should have been caught by a local package preflight.
- Where it appeared: `roleplays-a1-core-01-v1.json` during the first import attempt.
- Why it happened: The generator applied common package fields and translations, but did not run a required-field checklist before invoking the shared database import.
- Prevention rule: Before importing any RoleplayScenario package, run a package-specific preflight that checks `slug`, `title`, `description`, `learnerGoal`, `cefrLevel`, `category`, `topics`, `taskType`, `interactionMode`, `register`, `roles`, `turns`, `answerChoices`, `staticFeedback`, image-slot safety, and all active learner-language translations. Import validation remains the final gate, but it should not be the first place missing required fields are discovered.

### 2026-05-26: Roleplay controlled values must be checked before import

- What failed: An A1 RoleplayScenario batch used natural planning labels such as `clarify-information`, `small-talk`, `order-food-drink`, `social-interaction`, `shopping`, and `customer-service` for `taskType` or `skillFocus`.
- Where it appeared: `roleplays-a1-core-06-v1.json` during the first import attempt.
- Why it happened: The content plan used pedagogical labels that sounded reasonable, but the RoleplayScenario importer accepts only the controlled values already defined for catalog filtering and reporting.
- Prevention rule: Before importing any RoleplayScenario package, compare `taskType`, `skillFocus`, `interactionMode`, `register`, topics, and exam profiles against the active controlled values from previously accepted packages and the contract. Natural planning labels may be used in notes, but official JSON must use only importer-supported controlled values.

### 2026-06-08: Course helper translations must avoid casual English carryover

- What failed: The final C2 Course batch initially used borrowed English labels such as `feedback`, `register`, and `practice` inside some non-English learner-helper translations. The content was structurally valid, but it weakened the promise that helper translations respect each target language rather than falling back to English terminology.
- Where it appeared: `course-c2-stil-souveraenitaet-und-komplexer-diskurs-v1.json`, lessons 119-120 during pre-import preflight.
- Why it happened: These words are common in some multilingual education contexts, so they can slip through as plausible terms unless the preflight checks likely English carryover in recently generated translations.
- Prevention rule: Course content preflight must scan new lessons for common English carryover terms in non-English helper fields. Borrowed terms are allowed only when they are genuinely the best natural term for that learner language; otherwise use native equivalents such as `بازخورد`, `بۆچوونی گەڕاوە`, `bersiva nirxandinê`, `vlerësim kthyes`, or language-appropriate phrasing.

### 2026-06-08: PostgreSQL retrofit must cover every Web/API learning-portal table

- What failed: WebApi could start with the correct launch profile, but Unified Search returned 500 because the existing shared PostgreSQL database did not yet contain `ExamPrepUnits`. Follow-up schema inspection showed `ExamProfiles`, `WritingTemplates`, and `CulturalNotes` were also missing.
- Where it appeared: `/api/catalog/search` after the Course A1-C2 content phase, before the post-completion smoke pass.
- Why it happened: The domain model and content contracts existed, but the startup retrofit for existing PostgreSQL databases covered only some Phase 7 tables. `EnsureCreated` does not retrofit missing tables into an already populated PostgreSQL database.
- Prevention rule: Before closing any Web/API feature phase, verify both model support and existing-database retrofit support for every table queried by routes, Unified Search, import, admin reports, and publication/export paths. Add structural tests that assert the startup initializer contains the required PostgreSQL `CREATE TABLE IF NOT EXISTS` statements, then run local WebApi smoke against the shared PostgreSQL database.

### 2026-06-08: Exam profile display names still need real helper text

- What failed: The first Exam Prep A1/A2 pilot validation rejected profile `displayNameTranslations` because brand-like names such as `Goethe A1`, `telc A2`, and `DTZ A2-B1` were copied exactly into multiple learner languages.
- Where it appeared: `exam-prep-a1-a2-pilot-v1.json` during temporary PostgreSQL validation import before writing to `darwinlingua_shared`.
- Why it happened: Exam profile names are partly proper nouns, so it looked reasonable to repeat them unchanged. The stricter fallback gate correctly treated exact English reuse as a missing localized helper.
- Prevention rule: For proper-noun content fields, keep the German/source display name unchanged, but make helper translations descriptive in each learner language, such as "Goethe A1 exam profile" translated naturally. Do not bypass fallback checks by assuming brand labels are enough learner help.

### 2026-06-08: Exam Prep titles and helper translations need human-quality review before import

- What failed: The first generated Exam Prep A1/A2 pilot used learner-facing titles that mixed metadata into the title, such as CEFR level and exam section wording that already belonged in `cefrLevel`, `examProfileKey`, `examSection`, and `taskType`. Some helper translations were structurally valid but sounded literal or unnatural, and linked-practice fields were empty despite existing Course, Dialogue, Roleplay, and Exercise content.
- Where it appeared: `exam-prep-a1-a2-pilot-v1.json` after import review; the package was removed from the official content path and `darwinlingua_shared`.
- Why it happened: The generation prompt optimized for filling the new contract rather than first building a reviewed level/profile candidate plan and checking learner-facing title style, natural translations, and available linked material.
- Prevention rule: Before regenerating Exam Prep content, create level/profile planning first. Unit titles must not repeat CEFR/profile/section metadata unless the title is explicitly about comparing a provider format. Helper translations must be meaning-based and natural in each learner language; for example, German `wenn es natuerlich passt` should be rendered in Persian as "if it fits the meaning/context", not as an awkward literal phrase. Linked-practice fields should point to reviewed existing slugs where available, and empty linked fields need a documented reason.

### 2026-06-09: Exam Prep foundation batches must stay small and semantically translated

- What worked: The regenerated Exam Prep foundation batches were accepted when each batch stayed small, titles avoided CEFR/provider/section metadata, linked-practice slugs were verified against PostgreSQL before import, and Persian helper text was reviewed for meaning rather than word-for-word transfer.
- Where it appeared: `exam-prep-a1-a2-foundation-01-v1.json`, C1/B1 foundation packages, and B2 packages through `exam-prep-b2-foundation-05-v1.json`.
- Prevention rule: Keep future Exam Prep generation to small reviewed batches. For each unit, run a semantic sample read in Persian and at least spot-check other helper languages before import. Phrases such as `distanzierend`, `Einwand`, `Rueckfrage`, `Stellungnahme`, and `Sprachbausteine` must be translated by function and context, not by a single literal dictionary word.

### 2026-06-09: C2 Exam Prep foundation completed with a final phase backup

- What worked: The Goethe C2 Exam Prep foundation was completed in five small packages, each with three units, verified linked Course/Roleplay slugs, targeted ExamPrep tests, zero-warning import, and local/production smoke before closing the phase.
- Prevention rule: When a reviewed Exam Prep level reaches its planned count, update docs and create the external phase backup immediately before starting another content line. The backup must record live and restored `ExamProfiles`, `ExamPrepUnits`, and level-specific counts.

### 2026-06-12: Exam Prep depth content exposed doc drift and semantic-translation risk

- What failed: After the Exam Prep depth cycle continued beyond the original foundation count, roadmap and checklist docs still recorded the older `ExamPrepUnits=95` state while the live database contained 246 units. Some earlier helper translations also showed the recurring risk of literal wording, such as translating `distanzierend` as an opaque word instead of explaining the function as distance, non-alignment, or lack of empathy with the source position.
- Where it appeared: Exam Prep C2 depth batches and pre-Writing-Templates phase planning.
- Why it happened: The content loop optimized for small successful imports and smoke checks, but did not make docs/count synchronization and semantic translation review a required phase gate after the accepted depth cycle.
- Prevention rule: Before starting a new content family, sync docs to live PostgreSQL counts, run a phase backup, and add the current mistakes to this lessons-learned file. For Writing Templates, titles must not repeat CEFR/provider/section metadata, helper translations must be meaning-based in every learner language, and linked-practice fields may be empty only when related content truly does not exist.

### 2026-06-13: Writing Template text blocks and RTL helper text need rendering gates

- What failed: Long `templateText` and `sampleFilledVersion` values initially rendered like unwrapped single-line text blocks, creating horizontal page overflow. RTL helper translations also needed explicit direction handling so Persian, Arabic, and Sorani content read naturally.
- Where it appeared: Writing Template detail pages during A1-C2 content generation and live UI review.
- Why it happened: The content was structurally valid, but the UI container treated learner-facing letter text too much like code/preformatted text without enough wrapping constraints and direction metadata.
- Prevention rule: Before closing any content family with long prose fields, smoke-test at least one short beginner item and one long advanced item in Web detail pages. Long text containers must use bounded width and wrapping behavior, and RTL helper languages must render with `dir=rtl` or equivalent direction-aware markup.

### 2026-06-13: Writing Template difficulty must grow with CEFR level

- What failed: Early Writing Template batches risked making higher-level email/text samples too close in length and complexity to A1/A2 templates.
- Where it appeared: Writing Templates B1-C2 review before finalizing the full A1-C2 baseline.
- Why it happened: Small-batch generation kept content manageable, but without a level-length rule it could underuse the discourse complexity expected at B1, B2, C1, and C2.
- Prevention rule: For practical writing content, increase text length, sentence complexity, register control, argument structure, and revision guidance by CEFR level. A1/A2 templates should stay short and scaffolded; B1 should add clearer reasons and connected sentences; B2 should add paragraph-level control; C1/C2 should support nuance, synthesis, and style decisions without becoming generic essays.

### 2026-06-13: Life in Germany requires culturally aware helper translations

- What can fail: Cultural explanations can become misleading if helper translations are word-for-word or if examples assume one cultural background for all speakers of a language.
- Where it applies: Life in Germany notes from the first A1 pilot onward, using the current `CulturalNote` backing store.
- Why it matters: Life in Germany notes teach German communication norms, civic/legal concepts, and everyday systems, but learners understand them through their own language and social expectations. A literal helper translation may hide the practical contrast the learner needs.
- Prevention rule: Keep the German source note canonical, but write helper translations semantically. When a comparison helps, adapt the explanation to the target language audience in a careful, non-stereotyping way, such as "in some familiar informal contexts..." rather than broad claims about a whole country or language community. RTL helper languages must render with explicit direction metadata in Web views.

### 2026-06-13: Life in Germany is broader than culture notes

- What changed: The former public Cultural Notes feature was too narrow for the next content phase. Learners also need civic, legal, social-system, geography, and Orientierungskurs/Leben-in-Deutschland knowledge in their own languages.
- Where it applies: The renamed `Life in Germany` feature and all future packages using the `culturalNotes` backing store.
- Why it matters: Many learners memorize the fixed German test questions but do not understand the underlying concepts. The product should teach the concepts clearly and safely, not just reproduce a question bank.
- Prevention rule: Treat official LiD/Einbuergerungstest material as source orientation, not as bulk copied app content. Write original explanatory notes, keep legal topics as general education rather than individual legal advice, and use target-language helper translations to make the German civic/social concept understandable without stereotyping.

### 2026-06-20: Legal-adjacent Life in Germany content needs source refresh before generation

- What could fail: Laws, fines, residence/citizenship rules, cannabis rules, and civil-status rules can change after content has been generated, while learners may treat Life in Germany explanations as practical guidance.
- Where it applies: Life in Germany B2+ content and any future note that mentions rights, duties, crimes, fines, traffic, cannabis, residence, citizenship, school obligations, discrimination, violence protection, or public-office procedure.
- Why it matters: The feature is educational, not legal advice, but inaccurate legal-adjacent explanations can still mislead learners in everyday decisions. A source refresh also prevents old Orientierungskurs assumptions from missing newer topics such as the 2024 nationality-law reform, Chancenkarte, Cannabisgesetz, and Selbstbestimmungsgesetz.
- Prevention rule: Before generating legal/civic content batches, refresh the official-source checklist in `artifacts/planning/life-in-germany-content-plan.md`. Use exact fine amounts only with a cited review date; otherwise use cautious wording such as "can lead to a fine or criminal investigation." Teach the safe everyday action and where to get help, not tactics for avoiding enforcement.

### 2026-06-13: Life in Germany B1+ notes need real explanatory depth

- What failed: The first B1 Life in Germany drafts were structurally correct, but the main explanations were too short for learners who need to understand civic and legal concepts behind Orientierungskurs/Leben-in-Deutschland topics.
- Where it appeared: B1 foundation notes before expanding the German `context` and adding fuller explanatory `sections`.
- Why it happened: The generation pass treated B1 as only slightly longer than A1/A2, while the learner need at B1 is conceptual clarity, not just everyday action guidance.
- Prevention rule: For Life in Germany B1 and higher, keep the German `context` substantial while respecting the validation/storage limit, and use `sections` for the fuller explanation. Helper translations must explain the concept in the learner language, not compress it into vague abstract wording. B2+ should increase precision and nuance without turning content into legal advice or copied official question-bank material.

### 2026-06-16: Course activity-flow backfill needs slug verification before writing

- What could fail: Course `activityBlocks` can look correct in JSON while pointing learners to old or invented target slugs. That would turn the book-like flow into broken navigation even when import validation passes.
- Where it appeared: A1 activity-flow backfill planning, where candidate grammar/dialogue/exercise links needed to be checked against the current PostgreSQL content before being written into the cumulative A1 package.
- Why it matters: `activityBlocks` are now learner-facing navigation, not internal metadata. A broken target is more visible than a missing legacy linked slug because it appears as an explicit next step in the lesson flow.
- Prevention rule: Before writing activity targets, query the live content inventory and use only confirmed slugs. If a target is uncertain, convert the activity to `targetType: none` or choose another verified resource. After import, run admin unresolved-target diagnostics and a representative Web/API smoke before closing the backfill batch.

### 2026-06-16: Content import must not depend on a broken EF pending-model state

- What failed: The standard `DarwinLingua.ImportTool` currently runs the database initializer before importing and stops on EF pending-model changes. The A2 Module 1 content import had to use the existing `ImportNoInit` helper after schema verification.
- Where it appeared: Course A2 Module 1 activity-flow import.
- Why it matters: `ImportNoInit` is acceptable for a content-only repair when the target schema is already present, but it should not become the normal path. A broken primary import tool creates operational uncertainty and hides migration/snapshot drift.
- Prevention rule: Before each Course backfill batch, verify that `dotnet ef migrations has-pending-model-changes` is clean and that the standard shared ImportTool can process the cumulative package. If a diagnostic migration suggests adding a column that already exists in both an older hand-written migration and PostgreSQL, inspect the snapshot/designer files and use an idempotent sync migration rather than accepting a duplicate unsafe migration.

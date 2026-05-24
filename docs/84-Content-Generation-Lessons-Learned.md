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
- Standalone RoleplayScenario content must not be generated yet. The roleplay contract exists, but dedicated parser/import/persistence/Web/search/admin/test support is still missing. Until then, only `roleplayPrompts` inside Event Preparation Packs are importable.
- After any repair, rerun the conversation audit and check that P0 blockers, unresolved Dialogue word references, unresolved Talk Topic references, and contract issue counts remain zero.

## Maintenance Rule

When a content-quality problem is found, add a short note here with:

- what failed
- where it appeared
- why it happened
- the prompt, importer, validation, or rendering rule that prevents it next time

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
- Prevention rule: Implement parser, application validation, persistence, Web API, Web rendering, search/admin visibility, and tests before generating standalone roleplay packages. Until then, keep roleplay-like practice inside Event Preparation `roleplayPrompts`.

### 2026-05-23: Expression list pages must use learner-language meanings

- What failed: The Everyday Expressions detail endpoint localized meanings by requested learner language, but the list endpoint projected the base `actualMeaningText`, which is normally English.
- Where it appeared: The first real Expressions pilot review showed that `/expressions` would show English meanings even when the learner profile preferred another meaning language.
- Why it happened: The list query did not accept or forward `primaryMeaningLanguageCode`, while detail already did.
- Prevention rule: Expression list requests now carry `primaryMeaningLanguageCode`; repository tests verify localized list projection. New learning modules must check list and detail localization, not only detail pages.

### 2026-05-23: Provider-specific search functions need local-test coverage

- What failed: New Expression repository/search tests on SQLite exposed `EF.Functions.ILike` translation failures in Expression list/search paths.
- Where it appeared: `GetPublishedExpressionsAsync` and `UnifiedLearningSearchRepository.SearchExpressionsAsync` failed in SQLite-backed tests.
- Why it happened: The implementation used PostgreSQL-specific matching without a provider-neutral fallback for local test coverage.
- Prevention rule: For new content modules, add repository/search tests before bulk content generation. If the project supports SQLite-backed tests, keep module search filters provider-neutral unless a provider-specific path has a tested fallback.

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

### 2026-05-24: Adult-language learning content needs access state, not a checkbox-only shortcut

- What failed: Expressions planning initially treated `+18` as a simple profile flag, which is not enough for explicit adult or pornographic content in Germany.
- Where it appeared: Everyday Expressions adult/sensitive content planning.
- Why it happened: The content contract had safety ratings, but profile/access-state requirements were not explicit enough for future adult-language generation.
- Prevention rule: Store a separate adult-content access state (`not-requested`, `self-declared-adult`, `age-verified-adult`, `blocked`) and keep rude/slang preference separate from explicit adult access. `self-declared-adult` must not unlock explicit adult/pornographic content in production. Explicit adult content generation remains blocked until legal review, approved age verification, list/detail/search filtering, admin reporting, and release gates are all verified.

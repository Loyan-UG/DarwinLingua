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

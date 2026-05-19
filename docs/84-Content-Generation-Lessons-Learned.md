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

## Validation Checklist Before Regeneration

- Prompt audit returns zero unmatched prompts, missing syllabus topics, duplicate slugs, category mismatches, and guardrail omissions.
- Every prompt contains the anti-boilerplate localization rules.
- Every prompt points to the correct package for its CEFR level.
- A small sample generation is imported and rendered before regenerating the whole level.
- Duplicate-sentence checks run on localized sections, rule summaries, and common mistake explanations.
- Language-script and fallback checks run on all non-English localized fields.
- Web detail pages are smoke-tested for layout, tables, examples, rule summaries, common mistakes, prerequisites, and related topics.

## Maintenance Rule

When a content-quality problem is found, add a short note here with:

- what failed
- where it appeared
- why it happened
- the prompt, importer, validation, or rendering rule that prevents it next time

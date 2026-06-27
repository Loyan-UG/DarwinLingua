# Multi-Target Business Branch Plan

Status: branch checkpoint closed 2026-06-24; updated with the complete
multi-target expansion roadmap after the latest product decisions.

## Purpose

This document records the product decisions that shape the next multi-target
language goal after the platform readiness checkpoint. It is intentionally a
planning artifact only. It does not authorize broad content generation by
itself.

Darwin Lingua is still pre-customer, so the next implementation goal should use
clean target-language, country-context, package, route, import, search, progress,
and admin design instead of preserving ambiguous legacy behavior.

Detailed roadmap: `artifacts/planning/multi-target-language-expansion-roadmap.md`.

## Locked Product Decisions

1. German remains the only public active target-learning language until another
   target language passes an explicit activation decision.
2. The language being learned, UI language, helper language, level system, and
   country context are separate product concepts.
3. `Life in Germany` belongs inside the German target-language experience because
   Germany is a German-speaking country, but the stable platform feature is
   `Country Guidance`.
4. Country Guidance is scoped by `(targetLearningLanguageCode,
   countryContextCode)`.
5. German can later show Germany, Austria, and Switzerland as separate
   German-authored Country Guidance streams.
6. English can later show United States, United Kingdom, Australia, and other
   English-speaking country contexts as separate English-authored streams.
7. Switzerland can later appear under German, French, and Italian, but each
   stream must be authored separately in the relevant target language.
8. Helper translations remain learner support. They do not turn one source
   stream into another target-language stream.
9. Future helper-language expansion is independent from target-language
   expansion. German can grow from the current helper-language set to a larger
   set without changing German source-content identity.
10. Level selection must always show a compact level code plus a plain
    learner-friendly label and explanation. CEFR codes alone are not enough.
11. Future target languages must be planned for their own grammar, search,
    writing, pronunciation, country contexts, exams, script/direction, and input
    needs before content generation starts.
12. Because there are no active customers yet, canonical routes and model names
    should replace ambiguous legacy routes when compatibility would create
    technical debt.
13. Level labels need a learner-facing explanation in addition to compact codes,
    because many learners do not know CEFR terminology.
14. Every future target language must be planned for its own script, grammar,
    search, pronunciation, writing, exam, and country-context needs before
    content generation starts.

## Current Capability Profile Review

### German

German is the active baseline. Its current CEFR level metadata, public content,
Country Guidance `de|DE`, search, progress, recommendations, admin diagnostics,
and backup discipline are the production reference. German-specific design risks
remain cases, gender, separable verbs, verb-final clauses, formal/informal
address, compound words, umlaut search, and DACH country contexts.

### English

English is the first non-German pilot candidate and already has controlled
staging content imported. It is not public active.

Current status:

- Capability profile exists.
- Native A1 pilot content exists as English source content, not translated
  German content.
- English remains hidden from normal public learner routes.
- Public activation still requires an explicit product decision.

Before any English depth expansion, the next goal must decide whether English is
the business branch to continue. If yes, English should expand in small reviewed
batches and keep country guidance deferred until the language-learning pilot is
accepted.

English-specific risks to keep active in planning:

- spelling variants and country variants, especially US versus GB
- contractions, phrasal verbs, articles, countability, and tense/aspect
- stress, weak forms, rhythm, pronunciation, and accent handling
- writing conventions by country and register
- future `en|US`, `en|GB`, and `en|AU` Country Guidance as separate streams

### Spanish

Spanish remains planned and not content-importable. The current profile is
sufficient for planning status, but not enough to start content generation
without a dedicated Spanish implementation goal.

Before Spanish pilot content starts, the goal must decide:

- Spain-first or Latin America-first.
- Whether later variants use country-specific or region-specific streams.
- How `vosotros`, `ustedes`, and `voseo` are represented.
- How accent-insensitive search preserves correct source spelling.
- Which exam ecosystem and Country Guidance context are in the first pilot.

### French

French remains planned and not content-importable. The current profile is
sufficient for planning status, but not enough to start content generation
without a dedicated French implementation goal.

Before French pilot content starts, the goal must decide:

- France-first or another country context.
- When Switzerland, Belgium, or Canada become separate streams.
- How liaison, elision, silent letters, and pronunciation metadata are handled.
- How accent/apostrophe search preserves correct source spelling.
- Which writing conventions and administrative vocabulary belong in the first
  pilot.

## Recommended Next Business Branch

The recommended branch for the current checkpoint was:

1. Keep English non-public.
2. Review the imported English A1 pilot from a product/content point of view.
3. If accepted, expand English depth in small reviewed batches.
4. Add `en|US` Country Guidance only after the English learning-content flow is
   accepted.

Reasoning:

- English is already the only imported non-German staging pilot.
- It validates the platform assumptions with a high-demand target language.
- It avoids starting Spanish/French before their country and variant decisions
  are ready.
- It keeps German DACH Country Guidance as a clean alternate branch if tester
  feedback says German integration content has higher priority.

Checkpoint result:

- Branch A was selected.
- English was expanded once in a controlled cumulative A1 staging package.
- English remains pilot/non-public.
- Public activation and deeper English generation remain future product
  decisions.
- Closure evidence: `artifacts/validation/multi-target-phase9-closure-20260624.md`.

## Alternative Branches

### Branch A - English Depth First

Use when the next goal should validate Darwin Lingua as a multi-target language
learning product.

Initial scope:

- Expand English A1 from the current small pilot to a reviewed A1 foundation
  slice.
- Keep package imports cumulative and target-scoped.
- Keep English inactive unless public activation is explicitly approved.
- Do not add broad English country guidance yet.

### Branch B - German DACH Country Guidance

Use when the next goal should improve the current German user experience before
expanding non-German languages.

Initial scope:

- Create German-authored `de|AT` and `de|CH` planning profiles.
- Generate only a small reviewed Country Guidance pilot for Austria or
  Switzerland.
- Refresh legal-adjacent and public-process facts from current sources before
  publishing.
- Keep Germany, Austria, and Switzerland as separate source streams.

### Branch C - Spanish Pilot

Use only after a Spanish-specific implementation goal approves the country and
regional variant strategy.

Initial scope:

- Spain-first A1 pilot unless the product decision says otherwise.
- Native Spanish source content, not translated German or English.
- Spanish search, accents, regional forms, and writing conventions included in
  the acceptance gate.

### Branch D - French Pilot

Use only after a French-specific implementation goal approves the country
context and pronunciation/search handling.

Initial scope:

- France-first A1 pilot unless the product decision says otherwise.
- Native French source content, not translated German or English.
- French liaison, elision, accents, apostrophes, and register conventions in the
  acceptance gate.

## Goal-Ready Work Order

The next goal should execute one branch, not all branches at once.

Recommended goal if no new product priority overrides it:

1. Mark English as the selected branch for the next non-German pilot expansion,
   but keep it non-public.
2. Review the current English pilot content and diagnostics.
3. Repair any English pilot quality, link, search, helper-language, or admin
   diagnostics issues.
4. Create an English A1 expansion plan before generating more content.
5. Generate the next small English batch only after the plan is accepted.
6. Import, verify PostgreSQL counts, smoke Web/API/admin/search/progress, and
   back up after the branch checkpoint.

If the selected branch is German DACH instead:

1. Create `de|AT` and `de|CH` Country Guidance planning profiles.
2. Refresh current-source requirements for official-process and legal-adjacent
   topics.
3. Generate only a small reviewed pilot for one country context first.
4. Import, verify, smoke, and back up.

## Acceptance For Starting Any New Target-Language Content

No new broad content generation is allowed until all are true:

- The selected branch is explicit.
- The target language status and country context are explicit.
- Capability profile gaps are closed for that branch.
- Level labels are present for that target language.
- Package root and JSON root are target-language/country-context aware.
- Public route behavior for inactive or pilot targets is intentional.
- Helper-language coverage and RTL display requirements are defined.
- Search, progress, recommendations, admin reports, and backup inventories are
  target-scoped.
- There is a small reviewed batch plan with strict content and translation
  quality gates.

## Non-Goals

- Do not activate English publicly without an explicit activation decision.
- Do not start Spanish or French content generation from the current planned
  profiles alone.
- Do not generate Country Guidance by copying another country stream.
- Do not preserve old ambiguous learner routes as final behavior.
- Do not work on direct MAUI/mobile UI in this branch.

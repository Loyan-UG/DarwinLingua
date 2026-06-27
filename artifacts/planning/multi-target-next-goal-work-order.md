# Multi-Target Platform Next Goal Work Order

Status: completed 2026-06-24 for the multi-target readiness checkpoint.

## Purpose

This work order turns the multi-target architecture into the next executable goal. It is not a content-generation plan. It is the no-debt platform readiness plan that must be completed before broad English, Spanish, French, or later target-language content is generated.

German remains the only public active target-learning language. English may remain as a controlled pilot/staging target only after the existing pilot gate is closed. Spanish and French remain planned.

Master plan: `docs/95-Multi-Target-Platform-Refactor-Plan.md`.

## Locked Product Decisions

1. `targetLearningLanguage`, `uiLanguage`, `meaningLanguage`, `levelSystem`, and `countryContext` are separate concepts.
2. German stays public active and default until another target language passes its own reviewed activation gate.
3. `Life in Germany` is the German/Germany display label only. The stable platform module is `Country Guidance`.
4. Country Guidance is scoped by `(targetLearningLanguageCode, countryContextCode)`.
5. The same country can appear under multiple target languages only as separate authored source streams. For example, `de|CH`, `fr|CH`, and `it|CH` are three different source-content streams.
6. Helper translations remain learner support. They do not turn one country stream into another target-language stream.
7. Because the product is pre-customer, clean canonical routes, model names, package roots, and JSON roots are preferred over permanent compatibility routes.
8. Level selectors must show compact code plus learner-friendly label and explanation. CEFR codes alone are not enough.
9. Future target languages must be planned for their own grammar, search, writing, pronunciation, exams, country contexts, and script/direction needs before content generation starts.
10. German/Germany Country Guidance remains visible inside German learning because Germany is a German-speaking country, but the global feature identity remains Country Guidance.
11. Helper-language expansion, such as expanding German from ten helper languages to a larger set, is independent from target-language activation and must not duplicate source content.

## Next Goal Scope

The completed goal closed the platform readiness work in this order:

1. Normalize source/helper projections.
2. Finish English pilot diagnostics while keeping English non-public.
3. Audit target-language scope across all Web learning roots.
4. Close canonical route and naming cleanup.
5. Verify Country Guidance stream behavior.
6. Verify learner-friendly level metadata on every relevant surface.
7. Verify helper-language expansion readiness.
8. Produce a restore-ready backup.
9. Decide the next business branch after the pilot review.

Completion evidence:

- `artifacts/validation/multi-target-phase9-closure-20260624.md`
- `artifacts/validation/english-a1-expansion-02-import-20260624.md`
- `X:\Projects\DarwinLingua.Backup\20260624-211627-english-a1-expansion-02-branch-checkpoint-pre-activation`

Checkpoint decision:

- German remains the only public active target-learning language.
- English is explicitly kept as pilot/non-public after the staging expansion.
- Branch A was selected and executed for this checkpoint.
- Public English activation and deeper English content generation are future
  product-expansion goals.

Direct MAUI/mobile UI work is out of scope. APIs and package contracts must remain future-mobile compatible.

## Step 1 - Normalize Source And Helper Projections

Goal: every content detail response keeps target-language source fields separate from learner-helper fields.

Known current issue:

- Grammar detail projection localizes `title` and `shortDescription` in place for helper language preview. It must expose canonical source fields plus separate helper fields before English activation or broader multi-target activation.

Required work:

- Add separate learner-helper projection fields where missing.
- Keep source fields in the target language.
- Update API/admin preview tests so pilot English grammar returns English source text and Persian helper text separately.
- Check the same pattern for Course, Expressions, Exercises, Writing Templates, Exam Prep, Country Guidance, Conversation Packs, and Event Preparation Packs.

Acceptance:

- No detail API or admin preview replaces source text with helper translation.
- English pilot preview can inspect source and helper fields without making English public.

## Step 2 - Close English Pilot Diagnostics Without Public Activation

Goal: English pilot data proves target isolation but remains hidden from normal learner routes.

Required work:

- Verify imported English pilot counts by module.
- Verify public `/learn/en/...` routes and public APIs reject English while status is pilot.
- Verify admin-only preview and admin-only search can inspect English pilot data.
- Verify Unified Search is target-isolated.
- Verify progress, recommendations, and recent activity are target-isolated.
- Verify admin reports show English as pilot/content-importable and not active.

Acceptance:

- German public routes still work.
- English pilot diagnostics work only through admin/internal paths.
- No German fallback appears under English target context.

## Step 3 - Target-Language Scope Audit

Goal: classify every data root as target-scoped or intentionally global.

Target-scoped roots:

- vocabulary entries
- grammar topics
- expressions
- dialogues
- talk topics
- exercises and exercise sets
- roleplays
- courses, modules, lessons, and activity blocks
- exam prep
- writing templates
- Country Guidance
- conversation starter packs
- event preparation packs
- conversation events and RSVPs
- progress, recommendations, recent activity
- unified search results
- learning-level support

Intentionally global roots:

- user account/security identity
- organizer identity/directory profile
- operational configuration that is not learning-content scoped

Acceptance:

- Every repository query, Web route, WebApi endpoint, import replacement path, linked-content resolver, admin report, search query, progress query, and recommendation query has explicit target-language context where required.
- Global exemptions are documented and tested.

## Step 4 - Canonical Route And Naming Cleanup

Goal: no permanent route/name compatibility layer hides missing scope.

Canonical learner route pattern:

- `/learn/{targetLearningLanguageCode}/courses`
- `/learn/{targetLearningLanguageCode}/grammar/...`
- `/learn/{targetLearningLanguageCode}/expressions/...`
- `/learn/{targetLearningLanguageCode}/exercises/...`
- `/learn/{targetLearningLanguageCode}/writing-templates/...`
- `/learn/{targetLearningLanguageCode}/exam-prep/...`
- `/learn/{targetLearningLanguageCode}/country-guidance/{countryContextCode}`

Country Guidance naming:

- Public German/Germany display label: `Life in Germany`
- Stable feature/module name: `Country Guidance`
- Canonical package root: `content/learning-portal/country-guidance/packages`
- Canonical JSON root: `countryGuidanceNotes`

Acceptance:

- Old top-level learner routes are removed or fail once canonical routes are green.
- Tests and docs do not require old `cultural-notes` or ambiguous non-target routes as public behavior.

## Step 5 - Country Guidance Stream Verification

Goal: prove the platform can add multiple country contexts per target language and the same country under multiple target languages without redesign.

Current active stream:

- `de|DE`: German-authored Germany content, displayed as `Life in Germany`.

Planned examples:

- `de|AT`: German-authored Austria content.
- `de|CH`: German-authored Switzerland content.
- `en|US`: English-authored United States content.
- `en|GB`: English-authored United Kingdom content.
- `en|AU`: English-authored Australia content.
- `fr|CH`: French-authored Switzerland content.
- `it|CH`: Italian-authored Switzerland content.

Acceptance:

- Country Guidance packages require `targetLearningLanguageCode` and `countryContextCode`.
- Search links, activity links, admin diagnostics, backup manifests, and package receipts include both dimensions.
- No target language without an active country context silently falls back to Germany.

## Step 6 - Learner-Friendly Level System Verification

Goal: learners can choose levels even if they do not understand CEFR codes.

German CEFR baseline:

| Code | Friendly label | Purpose |
| --- | --- | --- |
| `A1` | Einstieg | First contact with greetings, identity, numbers, and simple everyday actions. |
| `A2` | Grundlagen | Everyday independence with appointments, shopping, messages, and common public-service situations. |
| `B1` | Selbststaendig | Independent daily life, connected speaking/writing, bureaucracy, work, school, health, and basic argumentation. |
| `B2` | Kompetent | Confident communication, detailed explanations, opinions, workplace/study tasks, nuanced reading, and longer writing. |
| `C1` | Souveraen | Advanced academic/professional discourse, implicit meaning, structured argumentation, and flexible register. |
| `C2` | Meisterschaft | Near-native precision, style, rhetorical control, complex synthesis, subtle tone, and expert/public discourse. |

Required surfaces:

- Course level cards
- search/filter controls
- settings/onboarding
- progress summaries
- event/profile matching
- admin preview/report pages
- package manifests and validation reports

Acceptance:

- Level surfaces read reference metadata instead of hard-coded German-only CEFR copy.
- Future target languages can use CEFR or another level system without UI redesign.

## Step 7 - Helper-Language Expansion Readiness

Goal: adding more helper languages later is a coverage/content task, not a schema refactor.

Required work:

- Verify centralized helper-language catalog drives import validation.
- Verify admin reports show missing helper coverage by module, target language, and helper language.
- Verify RTL helper text uses correct direction and wraps correctly.
- Verify backup inventory records helper-language coverage.

Acceptance:

- German can expand from the current helper-language set to a larger set without changing source-content identity.
- A helper language is not accidentally treated as a target-learning language.

## Step 8 - Restore-Ready Backup

Goal: close the platform-readiness checkpoint safely.

Backup must include:

- PostgreSQL logical dump and globals
- restore list and dry-run restore evidence
- repo overlay for local changes not necessarily in GitHub
- secret/local config bundle outside Git
- manifest with git state, target-language counts, country-context counts, helper-language coverage, checksums, and restore steps

Backup target:

- `X:\Projects\DarwinLingua.Backup\YYYYMMDD-HHMMSS-multi-target-readiness-pre-next-branch`

Acceptance:

- Counts in source DB and dry-run restored DB match.
- Manifest can be used with GitHub code plus backup contents to restore the exact checkpoint.

## Step 9 - Next Business Branch Decision

After the above gate is complete, choose one branch:

| Option | Meaning | Recommendation |
| --- | --- | --- |
| A | Expand English depth first | Recommended if the English pilot proves the architecture and tester/business demand supports English next. |
| B | Add German Austria/Switzerland Country Guidance | Recommended if German learners need DACH integration content before broad English. |
| C | Start Spanish pilot | Use only after a Spanish capability profile and Spain/regional decision are approved. |
| D | Start French pilot | Use only after a French capability profile and France/Switzerland/Belgium/Canada direction is approved. |

Current recommendation:

Finish and review the English pilot gate first. It validates the whole multi-target architecture with a high-demand non-German target while keeping German DACH Country Guidance as a clean separate branch.

## Non-Goals

- No broad English, Spanish, or French content generation before the readiness gate closes.
- No direct MAUI/mobile UI work.
- No permanent compatibility routes for old public names if they create ambiguity.
- No translated copies of German content as source content for another target language.
- No Country Guidance question-bank clone.

# Documentation Index

## Purpose

This document is the canonical index for the project documentation.

It exists to:

- define the official documentation set
- provide a recommended reading order
- reduce duplicate documents
- keep file naming and numbering consistent
- make future maintenance easier

All project documentation should remain in English.

---

## Official Documentation Set

### Core Product and Planning

- `01-Product-Vision.md`
- `02-Product-Scope.md`
- `03-Product-Phases.md`
- `04-Implementation-Backlog.md`
- `21-Early-Product-Decisions.md`
- `63-Market-Product-And-Organizer-Strategy.md`

### Content and Import

- `11-Content-Strategy.md`
- `12-Content-Package-Format.md`
- `13-Entry-Structure.json`
- `14-Import-Rules.md`
- `15-Topic-Seed-Ideas.md`
- `34-Import-Workflow.md`

### Domain Reference

- `22-Domain-Model.md`
- `23-Domain-Rules.md`
- `24-Entity-Relationships.md`
- `25-Phase-1-Domain-Cut.md`
- `26-Bounded-Contexts.md`

### Architecture and Storage

- `31-Solution-Architecture.md`
- `32-Storage-Strategy.md`
- `33-Offline-Strategy.md`
- `36-Server-Content-Distribution.md`
- `37-Shared-Content-Server-Domain.md`
- `38-Web-Platform-Architecture.md`
- `35-Engineering-Standards.md`

### Runbooks and Handoffs

- `41-Phase-1-Use-Cases.md`
- `42-Continuation-Handoff.md`
- `43-Phase-1-Release-Checklist.md`
- `44-Phase-1-Manual-Validation-Worksheet.md`
- `45-Phase-1-Release-Notes-Template.md`
- `46-Phase-2-Practice-Validation-Worksheet.md`
- `47-Phase-3-Mobile-UX-Validation-Worksheet.md`
- `48-Mobile-Validation-Bundle-Runbook.md`
- `49-Local-Postgres-Setup.md`
- `50-Phase-5-Remote-Update-Validation-Worksheet.md`
- `51-Local-Server-Bootstrap.md`
- `52-Future-Platform-Expansion.md`
- `53-Manual-System-Test-Runbook.md`
- `54-Web-Development-Backlog.md`
- `55-Web-Browse-Search-Detail-Validation-Worksheet.md`
- `56-Web-Pwa-Install-Validation-Worksheet.md`
- `57-Web-Auth-Validation-Worksheet.md`
- `58-Web-Deployment-Runbook.md`
- `59-Web-Asset-Build-Pipeline.md`
- `60-Web-Operations-Notes.md`
- `61-Web-Release-Checklist.md`
- `62-Web-Accessibility-Checklist.md`
- `65-Safety-And-Moderation-Requirements.md`
- `66-Dual-Meaning-Language-UX-Rules.md`
- `67-Dialogue-Content-Package-Contract.md`
- `68-Conversation-Starter-Content-Package-Contract.md`
- `69-Event-Preparation-Pack-Content-Package-Contract.md`
- `70-Roleplay-Content-Package-Contract.md`
- `71-Web-Test-Backlog.md`
- `72-Web-Seed-Coverage-And-Admin-Reports.md`
- `73-Transactional-Email-And-Account-Communication-Backlog.md`
- `75-Stripe-Billing-Validation-Playbook.md`
- `74-Talk-Topic-Content-Package-Contract.md`
- `76-Learning-Portal-Roadmap-And-Backlog.md`
- `77-Grammar-Content-Package-Contract.md`
- `78-Expression-Content-Package-Contract.md`
- `79-Exercise-Content-Package-Contract.md`
- `80-Course-Content-Package-Contract.md`
- `81-Writing-Template-Content-Package-Contract.md`
- `82-Country-Guidance-Content-Package-Contract.md`
- `83-Exam-Prep-Content-Package-Contract.md`
- `84-Content-Generation-Lessons-Learned.md`
- `85-Sensitive-Educational-Language-Policy.md`
- `86-Web-Legal-Compliance-Baseline.md`
- `87-Web-Tester-Onboarding-Runbook.md`
- `88-Web-Tester-Quick-Start.md`
- `89-Brevo-Operator-Handoff.fa.md`
- `90-Web-Operational-Incident-Runbook.md`
- `91-Web-Manual-External-Review-Checklist.md`
- `92-Web-Legal-Research-And-Risk-Audit.md`
- `93-Web-Human-Gate-Handoff.md`
- `94-Multi-Target-Language-Architecture.md`

Localized operator-oriented Persian variants also exist for:

- `51-Local-Server-Bootstrap.fa.md`
- `53-Manual-System-Test-Runbook.fa.md`

Related release-execution helpers live under `tools/Phase1/`.

---

## Recommended Reading Order

If you are new to the project, read the documents in this order:

1. `01-Product-Vision.md`
2. `02-Product-Scope.md`
3. `03-Product-Phases.md`
4. `21-Early-Product-Decisions.md`
5. `31-Solution-Architecture.md`
6. `32-Storage-Strategy.md`
7. `33-Offline-Strategy.md`
8. `36-Server-Content-Distribution.md`
9. `37-Shared-Content-Server-Domain.md`
10. `38-Web-Platform-Architecture.md`
11. `11-Content-Strategy.md`
12. `12-Content-Package-Format.md`
13. `14-Import-Rules.md`
14. `22-Domain-Model.md`
15. `25-Phase-1-Domain-Cut.md`
16. `26-Bounded-Contexts.md`
17. `35-Engineering-Standards.md`
18. `34-Import-Workflow.md`
19. `49-Local-Postgres-Setup.md`
20. `51-Local-Server-Bootstrap.md`
21. `04-Implementation-Backlog.md`
22. `42-Continuation-Handoff.md`
23. `50-Phase-5-Remote-Update-Validation-Worksheet.md`
24. `53-Manual-System-Test-Runbook.md`
25. `54-Web-Development-Backlog.md`
26. `55-Web-Browse-Search-Detail-Validation-Worksheet.md`
27. `56-Web-Pwa-Install-Validation-Worksheet.md`
28. `57-Web-Auth-Validation-Worksheet.md`
29. `58-Web-Deployment-Runbook.md`
30. `59-Web-Asset-Build-Pipeline.md`
31. `60-Web-Operations-Notes.md`
32. `61-Web-Release-Checklist.md`
33. `62-Web-Accessibility-Checklist.md`
34. `63-Market-Product-And-Organizer-Strategy.md`
35. `52-Future-Platform-Expansion.md`
36. `65-Safety-And-Moderation-Requirements.md`
37. `66-Dual-Meaning-Language-UX-Rules.md`
38. `67-Dialogue-Content-Package-Contract.md`
39. `68-Conversation-Starter-Content-Package-Contract.md`
40. `69-Event-Preparation-Pack-Content-Package-Contract.md`
41. `70-Roleplay-Content-Package-Contract.md`
42. `71-Web-Test-Backlog.md`
43. `72-Web-Seed-Coverage-And-Admin-Reports.md`
44. `73-Transactional-Email-And-Account-Communication-Backlog.md`
45. `75-Stripe-Billing-Validation-Playbook.md`
46. `74-Talk-Topic-Content-Package-Contract.md`
47. `76-Learning-Portal-Roadmap-And-Backlog.md`
48. `77-Grammar-Content-Package-Contract.md`
49. `78-Expression-Content-Package-Contract.md`
50. `79-Exercise-Content-Package-Contract.md`
51. `80-Course-Content-Package-Contract.md`
52. `81-Writing-Template-Content-Package-Contract.md`
53. `82-Country-Guidance-Content-Package-Contract.md`
54. `83-Exam-Prep-Content-Package-Contract.md`
55. `84-Content-Generation-Lessons-Learned.md`
56. `85-Sensitive-Educational-Language-Policy.md`
57. `86-Web-Legal-Compliance-Baseline.md`
58. `87-Web-Tester-Onboarding-Runbook.md`
59. `88-Web-Tester-Quick-Start.md`
60. `89-Brevo-Operator-Handoff.fa.md`
61. `90-Web-Operational-Incident-Runbook.md`
62. `91-Web-Manual-External-Review-Checklist.md`
63. `92-Web-Legal-Research-And-Risk-Audit.md`
64. `93-Web-Human-Gate-Handoff.md`
65. `94-Multi-Target-Language-Architecture.md`

---

## Document Roles

### Vision vs Scope

- `01-Product-Vision.md` defines why the product exists and what it should become.
- `02-Product-Scope.md` defines what is in scope and out of scope, especially for Phase 1.

### Phases vs Backlog

- `03-Product-Phases.md` defines the high-level product phases.
- `04-Implementation-Backlog.md` defines executable work planning, with detailed Phase 1 tasks, later-phase workstreams, and the merged Phase 6 backlog for dialogue learning, conversation events, organizer tooling, safe profiles, and partner matching.
- `64-Conversation-And-Organizer-Implementation-Backlog.md` is now only a compatibility pointer to the merged Phase 6 section in `04-Implementation-Backlog.md`.

### Product Strategy vs Execution

- `63-Market-Product-And-Organizer-Strategy.md` records the product positioning, market assumptions, monetization direction, and B2B organizer strategy.
- `65-Safety-And-Moderation-Requirements.md` defines the release gate for public profiles, matching, organizer listings, report/block, and moderation workflows.
- `66-Dual-Meaning-Language-UX-Rules.md` defines presentation rules for primary and secondary meaning languages across compact and expanded learning surfaces.
- `67-Dialogue-Content-Package-Contract.md` defines the Phase 6 JSON contract for dialogue lessons, dialogue turns, useful phrases, questions, answers, CEFR/topic rules, and validation rules.
- `68-Conversation-Starter-Content-Package-Contract.md` defines the Phase 6 JSON contract for conversation starter packs, starter phrases, filters, dialogue integration, dual-language behavior, and validation rules.
- `69-Event-Preparation-Pack-Content-Package-Contract.md` defines the Phase 6 JSON contract for event preparation packs, links to dialogues, vocabulary, starter packs, and preparation prompts.
- `70-Roleplay-Content-Package-Contract.md` defines the Phase 6 scripted roleplay contract and distinguishes Dialogue-derived Roleplay, Event Preparation `roleplayPrompts`, and standalone `RoleplayScenario` packages. It records the current Web-first infrastructure slice and the remaining validation gates before standalone RoleplayScenario content generation.
- `71-Web-Test-Backlog.md` defines the remaining Web automated and manual validation backlog owned by the separate test-development workflow.
- `72-Web-Seed-Coverage-And-Admin-Reports.md` records Web seed coverage, admin report scope, and the operational-seed follow-up.
- `73-Transactional-Email-And-Account-Communication-Backlog.md` defines the release-critical transactional email, account recovery, email confirmation, provider, diagnostics, localization, and security backlog.
- `75-Stripe-Billing-Validation-Playbook.md` defines the local and staging validation flow for Stripe Checkout, webhooks, Premium entitlement, Customer Portal, billing diagnostics, and billing notification emails.
- `74-Talk-Topic-Content-Package-Contract.md` defines the Talk Topics JSON contract, controlled content types, CEFR article-length validation, vocabulary-reference rules, speaking goals, sensitivity metadata, and reusable future support-content ideas.
- `76-Learning-Portal-Roadmap-And-Backlog.md` defines the Web-first roadmap and backlog for Grammar Guide, Everyday Expressions, Exercise Engine, Course Lessons, Exam Prep, Writing Templates, Life in Germany, Unified Search, Progress/Personalization, Admin Operations, and deferred Mobile parity. It includes the current implementation snapshot for Grammar A1-C2 validation, post-Grammar Conversation audit repair, Conversation Starter/Event Preparation baseline content, Roleplay blockers, and Brevo transactional email status.
- `77-Grammar-Content-Package-Contract.md` defines the Phase 7 Grammar Guide JSON contract now that the dynamic GrammarTopic implementation has started.
- `78-Expression-Content-Package-Contract.md` defines the Phase 7 Everyday Expressions JSON contract now that the dynamic ExpressionEntry implementation has started.
- `79-Exercise-Content-Package-Contract.md` defines the Phase 7 reusable Exercise Engine JSON contract now that deterministic exercises and exercise sets have started.
- `80-Course-Content-Package-Contract.md` defines the Phase 7 Course Lessons and CEFR Learning Paths JSON contract now that dynamic course implementation has started.
- `81-Writing-Template-Content-Package-Contract.md` defines the Phase 7 Writing Templates JSON contract now that dynamic template implementation has started.
- `82-Country-Guidance-Content-Package-Contract.md` defines the Phase 7 Life in Germany JSON contract using the current `CountryGuidanceNote` backing store.
- `83-Exam-Prep-Content-Package-Contract.md` defines the Phase 7 Exam Preparation JSON contract now that dynamic exam-prep implementation has started.
- `84-Content-Generation-Lessons-Learned.md` records prompt, localization, import, rendering, and validation lessons that must be checked before future bulk content generation.
- `85-Sensitive-Educational-Language-Policy.md` defines the product, content, filtering, privacy, and release policy for warning-labeled rude/slang/romantic/social educational language without allowing pornographic or explicit adult content.
- `86-Web-Legal-Compliance-Baseline.md` defines the Web-first legal/compliance baseline for public legal pages, registration acknowledgements, policy records, cookie/storage inventory, GDPR/TDDDG/DDG review gates, Sensitive Educational Language legal boundaries, and deferred mobile compliance.
- `87-Web-Tester-Onboarding-Runbook.md` defines the controlled real-user Web testing pass, tester tasks, feedback format, triage rules, and completion criteria before mobile work resumes.
- `88-Web-Tester-Quick-Start.md` defines the short tester-facing task brief that can be shared without the operator runbook.
- `89-Brevo-Operator-Handoff.fa.md` gives the Persian operator handoff for Brevo sender/domain setup, webhook configuration, secrets, DPA review, readiness checks, and Darwin Lingua production-domain rules.
- `90-Web-Operational-Incident-Runbook.md` defines the Web-first operational ownership and incident process for Brevo, DNS/Cloudflare, privacy requests, security/breach triage, community escalation, and backup/restore.
- `91-Web-Manual-External-Review-Checklist.md` gives the operator one evidence checklist for the remaining human gates before and during the controlled Web tester pass: real mailbox rendering, PWA install acceptance, and tester-pass start/close criteria.
- `92-Web-Legal-Research-And-Risk-Audit.md` records the current-source Web legal and compliance risk audit for DDG/TDDDG/GDPR, self-service account controls, Brevo email, DSA, BFSG/accessibility, crime/content-risk guardrails, Life in Germany content, and deferred billing gates.
- `93-Web-Human-Gate-Handoff.md` defines the generated operator handoff that maps current automated readiness evidence to exact remaining human-gate actions before tester invitations.
- `94-Multi-Target-Language-Architecture.md` defines target learning language, UI language, meaning/helper language, country context, level-system, route, package, testing, and first non-German pilot rules for the Phase 8 platform refactor.
- `95-Multi-Target-Platform-Refactor-Plan.md` turns the multi-target architecture into an executable no-debt refactor plan for target-language scope, Country Guidance streams, learner-friendly level metadata, helper-language expansion, canonical routes, diagnostics, backup gates, and future target-language activation.
- `artifacts/planning/multi-target-language-expansion-roadmap.md` is the goal-ready roadmap for future target-language expansion, including Country Guidance streams, no-legacy route policy, learner-friendly level labels, helper-language expansion, and branch choices after the English pilot.
- `04-Implementation-Backlog.md` turns that strategy into implementation-ready backlog items, including the active Phase 8 multi-target-language execution checklist.

### Learning Portal Planning Rule

- `76-Learning-Portal-Roadmap-And-Backlog.md` is the single roadmap/backlog source for Phase 7.
- Do not create extra planning documents for individual Phase 7 modules such as Grammar, Expressions, Exercises, Courses, Exam Prep, Writing Templates, or Life in Germany.
- Dedicated content-contract documents should be created only when implementation starts for that module; `77-Grammar-Content-Package-Contract.md`, `78-Expression-Content-Package-Contract.md`, `79-Exercise-Content-Package-Contract.md`, `80-Course-Content-Package-Contract.md`, `81-Writing-Template-Content-Package-Contract.md`, `82-Country-Guidance-Content-Package-Contract.md`, and `83-Exam-Prep-Content-Package-Contract.md` are implementation contract examples.

### Numbering Cleanup Follow-Up

- Two `74-*` documents currently exist: `74-Production-Operations-Setup-Checklist.md` and `74-Talk-Topic-Content-Package-Contract.md`.
- Do not delete either file during feature work. Resolve the duplicate numbering in a dedicated documentation cleanup task.

### Rules vs Workflow

- `14-Import-Rules.md` defines stable import rules and constraints.
- `34-Import-Workflow.md` defines the end-to-end processing flow and responsibilities.

### Domain vs Architecture

- `22-Domain-Model.md` explains the domain concepts.
- `23-Domain-Rules.md` defines invariant and lifecycle rules.
- `31-Solution-Architecture.md` defines project/layer structure and dependency direction.
- `32-Storage-Strategy.md` defines the SQLite, migration, and indexing direction for Phase 1.
- `33-Offline-Strategy.md` defines the local-first runtime behavior.
- `36-Server-Content-Distribution.md` defines the future server-authored content-update architecture.
- `37-Shared-Content-Server-Domain.md` defines the multi-product server-side domain and publishing model.
- `38-Web-Platform-Architecture.md` defines the future ASP.NET Core MVC web host, PWA baseline, Identity boundary, and `Admin` area direction.

### Reference vs Runbooks

- domain and architecture documents should stay stable and explanatory
- release worksheets and runbooks should stay short and operational
- the backlog remains the main execution checklist

---

## File Naming Rule

Documentation file names must follow this pattern:

- numeric prefix for reading order
- short stable English title
- words separated with hyphens
- no spaces
- no draft-only names unless the file is truly temporary

Examples:

- `01-Product-Vision.md`
- `32-Storage-Strategy.md`
- `35-Engineering-Standards.md`
- `63-Market-Product-And-Organizer-Strategy.md`

---

## Maintenance Rules

- Do not keep duplicate documents that describe the same concern.
- If a document becomes obsolete, remove it instead of leaving conflicting copies.
- Keep README as the repository entry point, not as the place for every detail.
- Keep detailed implementation rules in the dedicated docs, not buried in issue text or chat history.
- Update internal references when files are renamed.

---

## Current Cleanup Result

The documentation set is intentionally split into:

- a smaller core reading path
- stable reference documents
- execution runbooks
- market/product strategy documents
- implementation backlogs

If future cleanup is needed, prefer consolidating overlapping runbooks before touching the core architecture/domain docs.


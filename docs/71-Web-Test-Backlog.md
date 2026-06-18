# Web Test Backlog

## Purpose

This document is the handoff backlog for tests that must be implemented outside the current Web feature-development thread.

Web feature development is considered implementation-complete for the current backlog when:

- Web learner and admin features are implemented
- Web routes build and run against WebApi
- existing test suites pass
- remaining work is limited to explicit test automation, manual device/browser validation, or mobile parity

The items below are intentionally not implemented in this thread. They are owned by the separate test-development workflow.

## Current Web Development Status

Status: feature-complete for the current Web-focused backlog.

Remaining release gates:

- automated test coverage listed in this document
- manual browser/device validation listed in the Web validation worksheets
- mobile parity work, after Web sign-off

Registration/legal acknowledgement coverage must remain part of the Web-side release gate. Registration must require Terms of Use acceptance, show a Privacy Policy notice acknowledgement, store versioned policy acceptance records, and keep Sensitive Educational Language disabled by default.

Web legal/compliance baseline coverage must remain Web-only until Web sign-off. Public legal pages must render, footer links must expose Privacy/Terms/Legal/Cookies/Contact, cookie/storage inventory must match the checked-in scripts, non-essential storage must stay blocked until opt-in if introduced, and mobile legal/store-compliance work remains deferred.

Latest local Web verification:

- 2026-05-23: First real Everyday Expressions pilot package added at `content/learning-portal/expressions/packages/expressions-a1-a2-core-pilot-v1.json`; `node tools/Content/Validate-ExpressionPilot.js content/learning-portal/expressions/packages/expressions-a1-a2-core-pilot-v1.json` passed.
- 2026-05-23: Expression parser/import/repository/search/admin/report structural coverage updated. `DarwinLingua.ContentOps.Infrastructure.Tests` passed 37/37, `DarwinLingua.ContentOps.Application.Tests` passed 71/71, `DarwinLingua.Catalog.Infrastructure.Tests` passed 28/28, `DarwinLingua.WebApi.Tests` passed 57/57, and filtered `WebLearnerShellStructureTests` passed 5/5.
- 2026-05-23: Full `DarwinLingua.Localization.Application.Tests` still has two unrelated MAUI smoke failures (`AppStrings.HomeTabTitle`, `EnsureSeedDatabaseAsync`). Mobile/MAUI remains outside the current Expressions pilot scope.
- 2026-05-24: Everyday Expressions pilot was re-imported into the shared PostgreSQL catalog with `Package content items total: 12`, then smoke-tested through local Web/API services. `/expressions`, `/expressions/alles-klar`, `/api/catalog/expressions`, `/api/catalog/expressions/alles-klar`, `/api/catalog/search`, and `/api/admin/catalog/system-report` returned HTTP 200 with Expression results/counts. Admin report hardening now treats not-yet-created Phase 7 tables as empty instead of failing the whole report.
- 2026-05-24: Small Everyday Expressions batch `expressions-a1-a2-core-01-v1.json` was created with 25 expressions, validated with `tools/Content/Validate-ExpressionPilot.js`, imported into `darwinlingua_shared`, and smoke-tested through public-routed local services. `/expressions`, `/expressions/vielen-dank-im-voraus`, `/api/catalog/expressions`, `/api/catalog/expressions/vielen-dank-im-voraus`, `/api/catalog/search`, `/search`, and `/api/admin/catalog/system-report` returned HTTP 200. Admin report showed `expression` count 37, missing translations 0, and unresolved linked words 0.
- 2026-05-24: The stricter Expressions eligibility pass repaired the two existing Expressions packages. `tools/Content/Audit-ExpressionContentQuality.js` now reports 0 issues; 5 ordinary-literal candidates are unpublished; public `/api/catalog/expressions` returns 32 published expressions; `/api/catalog/search?q=Alles&resultType=expression` returns an Expression result; admin report quality counters show 0 ordinary-literal leakage, 0 missing eligibility metadata, 0 low example counts, and 0 missing risky-content warnings.
- 2026-05-24: A2 Everyday Expressions package `expressions-a2-core-v1.json` was created with 30 published non-literal, semi-idiomatic, and pragmatic-formula expressions. Package validation and `tools/Content/Audit-ExpressionContentQuality.js` passed with 0 issues across 4 packages. Import into `darwinlingua_shared` succeeded with 30 content items and 0 warnings. Local smoke returned HTTP 200 for `/expressions`, `/expressions/ich-verstehe-nur-bahnhof`, `/expressions/das-ist-nicht-mein-bier`, `/api/catalog/expressions`, `/api/catalog/expressions/ich-verstehe-nur-bahnhof?primaryMeaningLanguageCode=fa`, `/api/catalog/search?q=Bahnhof&resultType=expression`, and `/api/admin/catalog/system-report`. Admin report showed `expression` count 97, missing translations 0, unresolved linked words 0, and zero Expressions quality counters.
- 2026-05-24: B1 Everyday Expressions package `expressions-b1-core-v1.json` was created with 40 published idiom/pragmatic entries, including 7 mild-rude/frustration entries with warning metadata and no explicit-adult content. Package validation and `tools/Content/Audit-ExpressionContentQuality.js` passed with 0 issues across 5 packages. Import into `darwinlingua_shared` succeeded with 40 content items and 0 warnings. Local and public-routed smoke returned HTTP 200 for `/expressions`, `/expressions/ich-stehe-auf-dem-schlauch`, `/expressions/jetzt-mal-butter-bei-die-fische`, `/api/catalog/expressions`, `/api/catalog/expressions/ich-stehe-auf-dem-schlauch?primaryMeaningLanguageCode=fa`, `/api/catalog/search?q=Schlauch&resultType=expression`, and `/api/admin/catalog/system-report`. Admin report showed 137 Expression records, 132 active records, missing translations 0, unresolved linked words 0, and zero Expressions quality counters.
- 2026-05-24: B2 Everyday Expressions package `expressions-b2-core-v1.json` was created with 40 published idiom/semi-idiomatic/pragmatic entries, including 2 mild-rude/frustration entries with warning metadata and no explicit-adult content. Package validation and `tools/Content/Audit-ExpressionContentQuality.js` passed with 0 issues across 6 packages. Import into `darwinlingua_shared` succeeded with 40 content items and 0 warnings. Public-routed smoke returned HTTP 200 for `/expressions`, `/expressions/den-ball-flach-halten`, `/expressions/durch-die-blume-sagen`, `/api/catalog/expressions`, `/api/catalog/expressions/den-ball-flach-halten?primaryMeaningLanguageCode=fa`, `/api/catalog/search?q=Schirm&resultType=expression`, and `/api/admin/catalog/system-report`. Admin report showed 177 Expression records, 172 active records, missing translations 0, unresolved linked words 0, and zero Expressions quality counters.
- 2026-05-24: Second B2 Everyday Expressions package `expressions-b2-core-02-v1.json` was created with 40 published idiom/semi-idiomatic/pragmatic entries, including 3 mild-rude/social-risk entries with warning metadata and no explicit-adult content. Package validation and `tools/Content/Audit-ExpressionContentQuality.js` passed with 0 issues across 7 packages. Import into `darwinlingua_shared` succeeded with 40 content items and 0 warnings. Public-routed smoke returned HTTP 200 for `/expressions`, `/expressions/etwas-auf-die-lange-bank-schieben`, `/expressions/auf-taube-ohren-stossen`, `/api/catalog/expressions`, `/api/catalog/expressions/etwas-auf-die-lange-bank-schieben?primaryMeaningLanguageCode=fa`, `/api/catalog/search?q=Bank&resultType=expression`, and `/api/admin/catalog/system-report`. Admin report showed 217 Expression records, 212 active records, missing translations 0, unresolved linked words 0, and zero Expressions quality counters.
- 2026-05-24: Third B2 Everyday Expressions package `expressions-b2-core-03-v1.json` was created with 40 published idiom/semi-idiomatic entries, including 5 mild-rude/social-risk entries with warning metadata and no explicit-adult content. Package validation and `tools/Content/Audit-ExpressionContentQuality.js` passed with 0 issues across 8 packages. Import into `darwinlingua_shared` succeeded with 40 content items and 0 warnings. Public-routed smoke returned HTTP 200 for `/expressions`, `/expressions/den-nagel-auf-den-kopf-treffen`, `/expressions/mit-der-tuer-ins-haus-fallen`, `/api/catalog/expressions`, `/api/catalog/expressions/den-nagel-auf-den-kopf-treffen?primaryMeaningLanguageCode=fa`, `/api/catalog/search?q=Nagel&resultType=expression`, and `/api/admin/catalog/system-report`. Admin report showed 257 Expression records, 252 active records, 120 active B2 records, missing translations 0, unresolved linked words 0, and zero Expressions quality counters.
- 2026-05-24: C1 Everyday Expressions package `expressions-c1-core-v1.json` was created with 40 published idiom/cultural/nuance entries, including 6 mild-rude/social-risk entries with warning metadata and no explicit-adult content. Package validation and `tools/Content/Audit-ExpressionContentQuality.js` passed with 0 issues across 9 packages. Import into `darwinlingua_shared` succeeded with 40 content items and 0 warnings. Public-routed smoke returned HTTP 200 for `/expressions`, `/expressions/ins-fettnaepfchen-treten`, `/expressions/den-teufel-an-die-wand-malen`, `/api/catalog/expressions`, `/api/catalog/expressions/ins-fettnaepfchen-treten?primaryMeaningLanguageCode=fa`, `/api/catalog/search?q=Fettn%C3%A4pfchen&resultType=expression`, and `/api/admin/catalog/system-report`. Admin report showed 297 Expression records, 292 active records, 40 active C1 records, missing translations 0, unresolved linked words 0, and zero Expressions quality counters.
- 2026-05-24: Second C1 Everyday Expressions package `expressions-c1-core-02-v1.json` was created with 40 published advanced idiom/pragmatic entries, including 2 mild-rude/social-risk entries with warning metadata and no explicit-adult content. Package validation and `tools/Content/Audit-ExpressionContentQuality.js` passed with 0 issues across 10 packages. Import into `darwinlingua_shared` succeeded with 40 content items and 0 warnings. Public-routed smoke returned HTTP 200 for `/expressions`, `/expressions/die-zweite-geige-spielen`, `/expressions/den-pudels-kern-erkennen`, `/api/catalog/expressions`, `/api/catalog/expressions/die-zweite-geige-spielen?primaryMeaningLanguageCode=fa`, `/api/catalog/search?q=Geige&resultType=expression`, and `/api/admin/catalog/system-report`. Admin report showed 337 Expression records, 332 active records, 80 active C1 records, missing translations 0, unresolved linked words 0, and zero Expressions quality counters.
- 2026-05-25: C2 Everyday Expressions package `expressions-c2-core-v1.json` was created with 40 published high-context idiom/cultural/irony entries, including 13 mild-rude/social-risk entries with warning metadata and no explicit-adult content. Package validation and `tools/Content/Audit-ExpressionContentQuality.js` passed with 0 issues across 11 packages. Import into `darwinlingua_shared` succeeded with 40 content items and 0 warnings after correcting one unknown topic key before the successful import. Public-routed smoke returned HTTP 200 for `/expressions`, `/expressions/das-schlaegt-dem-fass-den-boden-aus`, `/expressions/perlen-vor-die-saeue-werfen`, `/api/catalog/expressions`, `/api/catalog/expressions/das-schlaegt-dem-fass-den-boden-aus?primaryMeaningLanguageCode=fa`, `/api/catalog/search?q=Fass&resultType=expression`, and `/api/admin/catalog/system-report`. Admin report showed 377 Expression records, 372 active records, 40 active C2 records, missing translations 0, unresolved linked words/content references 0, and zero Expressions quality counters.
- 2026-05-25: Second C2 Everyday Expressions package `expressions-c2-core-02-v1.json` was created with 40 published high-context idiom/rhetorical/cultural entries, including 11 mild-rude/social-risk entries with warning metadata and no explicit-adult content. Package validation and `tools/Content/Audit-ExpressionContentQuality.js` passed with 0 issues across 12 packages. Import into `darwinlingua_shared` succeeded with 40 content items and 0 warnings. Public-routed smoke returned HTTP 200 for `/expressions`, `/expressions/den-gordischen-knoten-durchschlagen`, `/expressions/auf-toenernen-fuessen-stehen`, `/api/catalog/expressions`, `/api/catalog/expressions/den-gordischen-knoten-durchschlagen?primaryMeaningLanguageCode=fa`, `/api/catalog/search?q=Gordischen&resultType=expression`, and `/api/admin/catalog/system-report`. Admin report showed 417 Expression records, 412 active records, 80 active C2 records, missing translations 0, unresolved linked words/content references 0, and zero Expressions quality counters.
- 2026-05-25: Mixed Everyday Expressions supplement `expressions-mixed-supplement-01-v1.json` was created with 14 additional A2-C1 idiom/cultural/social expressions that were absent from the existing packages, including 1 social-risk entry with warning metadata and no explicit-adult content. Package validation and `tools/Content/Audit-ExpressionContentQuality.js` passed with 0 issues across 13 packages. Import into `darwinlingua_shared` succeeded with 14 content items and 0 warnings. Public-routed learner/API smoke returned HTTP 200 for `/expressions`, `/expressions/hals-und-beinbruch`, `/expressions/die-faeden-ziehen`, `/api/catalog/expressions/hals-und-beinbruch?primaryMeaningLanguageCode=fa`, and `/api/catalog/search?q=Groschen&resultType=expression`. The admin endpoint returned 401 without credentials as expected; `WebsiteAdminQueryServiceLearningPortalReportTests` passed 2/2 and the strict audit reported zero Expressions quality issues.
- 2026-05-25: Web legal/compliance baseline added public `/legal`/`/impressum`, `/cookies`/`/cookie-policy`, and `/contact` pages, footer links, `docs/86-Web-Legal-Compliance-Baseline.md`, and `artifacts/validation/web-cookie-storage-inventory.md`. Structural tests cover routes, footer links, registration acknowledgements, policy acceptance records, Sensitive Educational Language settings copy, and the no-banner cookie/storage decision for the current strictly necessary/preference-storage baseline.
- 2026-05-31: Course Lessons v1 foundation added learner-helper translations, PostgreSQL-backed Course persistence/projection support, admin translation quality counters, and the first A1 pilot package. The original A1 pilot evidence covered 1 path, 1 module, and 5 cumulative lessons with targeted parser/import, PostgreSQL repository, admin report tests, Web project build, full solution build, and local Course Web/API smoke. This is now superseded by the 2026-06-08 A1-C2 imported baseline below.
- 2026-06-08: Course Lessons A1-C2 are the current imported Course baseline in shared PostgreSQL: 6 paths, 56 modules, 560 lessons total (`A1=60`, `A2=80`, `B1=100`, `B2=80`, `C1=120`, `C2=120`). The C2 source package `content/learning-portal/courses/packages/course-c2-stil-souveraenitaet-und-komplexer-diskurs-v1.json` imported with zero warnings after C2 batch 24. Local Web smoke returned HTTP 200 for `/`, `/courses`, `/courses/c2-stil-souveraenitaet-und-komplexer-diskurs`, and `/courses/c2-stil-souveraenitaet-und-komplexer-diskurs/c2-abschluss-und-meisterschaftspflege`. Local WebApi smoke returned HTTP 200 for course list/detail, Persian lesson detail, and course-lesson search. `WebsiteAdminQueryServiceLearningPortalReportTests` passed 2/2 because the local admin endpoint correctly returns 401 without credentials. PostgreSQL counts are `A1=60`, `A2=80`, `B1=100`, `B2=80`, `C1=120`, `C2=120`.
- 2026-06-16: Course lesson reading-flow UI uses `activityBlocks` when present and keeps legacy linked-content fallback for lessons not yet backfilled. Structural tests cover the Razor activity flow, wrapping CSS classes, RTL helper text, required/optional badges, and target URL mapping. Admin diagnostics cover published lessons without activity blocks, malformed activity JSON, unsupported activity target types, and unresolved activity target slugs.
- 2026-06-09/12: Reviewed Exam Prep foundation and depth content is imported for A1/A2/DTZ, C1, B1, B2, and Goethe C2. Current shared PostgreSQL counts are `ExamProfiles=17`, `ExamPrepUnits=246`, and `GoetheC2Units=86`, with C2 distributed as reading 17, listening 17, speaking 17, writing 17, strategy 17, and overview 1. Targeted ExamPrep ContentOps tests passed during content batches, imports completed with zero warnings, public API/search/Web smoke returned HTTP 200 where public routes are exposed, and the phase backup was created at `X:\Projects\DarwinLingua.Backup\20260612-142146-exam-prep-complete-pre-writing-templates`.
- 2026-06-13: Writing Templates A1-C2 are generated and imported into `darwinlingua_shared` with `WritingTemplates=120`, distributed as 20 templates per CEFR level. Targeted WritingTemplate ContentOps tests passed. Local Web/API smoke returned HTTP 200 for `/writing-templates`, A1 detail, C2 detail, Persian API detail, and `/api/catalog/search?q=Abschlussstatement&resultType=writing-template`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 talking about plans and conditions topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-talking-about-plans-and-conditions?primaryMeaningLanguageCode=fa`, `/grammar/b1-talking-about-plans-and-conditions`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 talking about plans and conditions topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 describing experiences in the past topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-describing-experiences-in-the-past?primaryMeaningLanguageCode=fa`, `/grammar/b1-describing-experiences-in-the-past`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 describing experiences in the past topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 agreeing and disagreeing grammatically topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-agreeing-and-disagreeing-grammatically?primaryMeaningLanguageCode=fa`, `/grammar/b1-agreeing-and-disagreeing-grammatically`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 agreeing and disagreeing grammatically topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 giving reasons clearly topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-giving-reasons-clearly?primaryMeaningLanguageCode=fa`, `/grammar/b1-giving-reasons-clearly`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 giving reasons clearly topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 complaint sentence patterns topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-complaint-sentence-patterns?primaryMeaningLanguageCode=fa`, `/grammar/b1-complaint-sentence-patterns`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 complaint sentence patterns topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 formal email sentence structure topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-formal-email-sentence-structure?primaryMeaningLanguageCode=fa`, `/grammar/b1-formal-email-sentence-structure`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 formal email sentence structure topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 sentence order with multiple clauses topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-sentence-order-with-multiple-clauses?primaryMeaningLanguageCode=fa`, `/grammar/b1-sentence-order-with-multiple-clauses`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 sentence order with multiple clauses topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 connectors for cause/effect topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-connectors-for-cause-and-effect?primaryMeaningLanguageCode=fa`, `/grammar/b1-connectors-for-cause-and-effect`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 connectors for cause/effect topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 connectors for contrast topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-connectors-for-contrast?primaryMeaningLanguageCode=fa`, `/grammar/b1-connectors-for-contrast`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 connectors for contrast topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 connectors for opinion topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-connectors-for-opinion?primaryMeaningLanguageCode=fa`, `/grammar/b1-connectors-for-opinion`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 connectors for opinion topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 noun-verb phrases topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-noun-verb-phrases?primaryMeaningLanguageCode=fa`, `/grammar/b1-noun-verb-phrases`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 noun-verb phrases topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 verb + preposition combinations topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-verb-plus-preposition-combinations?primaryMeaningLanguageCode=fa`, `/grammar/b1-verb-plus-preposition-combinations`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 verb + preposition combinations topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 prepositional verbs introduction topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-prepositional-verbs-introduction?primaryMeaningLanguageCode=fa`, `/grammar/b1-prepositional-verbs-introduction`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 prepositional verbs introduction topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 genitive introduction topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-genitive-introduction?primaryMeaningLanguageCode=fa`, `/grammar/b1-genitive-introduction`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 genitive introduction topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 adjective declension without article topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-adjective-declension-without-article?primaryMeaningLanguageCode=fa`, `/grammar/b1-adjective-declension-without-article`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 adjective declension without article topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 adjective declension after indefinite article topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-adjective-declension-after-indefinite-article?primaryMeaningLanguageCode=fa`, `/grammar/b1-adjective-declension-after-indefinite-article`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 adjective declension after indefinite article topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 adjective declension after definite article topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-adjective-declension-after-definite-article?primaryMeaningLanguageCode=fa`, `/grammar/b1-adjective-declension-after-definite-article`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 adjective declension after definite article topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 reported requests and polite questions topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-reported-requests-and-polite-questions?primaryMeaningLanguageCode=fa`, `/grammar/b1-reported-requests-and-polite-questions`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 reported requests and polite questions topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 indirect questions topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-indirect-questions?primaryMeaningLanguageCode=fa`, `/grammar/b1-indirect-questions`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 indirect questions topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 nachdem/bevor/während topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-nachdem-bevor-waehrend?primaryMeaningLanguageCode=fa`, `/grammar/b1-nachdem-bevor-waehrend`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 nachdem/bevor/während topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 als vs wenn topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-als-versus-wenn?primaryMeaningLanguageCode=fa`, `/grammar/b1-als-versus-wenn`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 als vs wenn topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 weil/obwohl/trotzdem topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-weil-obwohl-trotzdem?primaryMeaningLanguageCode=fa`, `/grammar/b1-weil-obwohl-trotzdem`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 weil/obwohl/trotzdem topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 damit vs um ... zu topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-damit-versus-um-zu?primaryMeaningLanguageCode=fa`, `/grammar/b1-damit-versus-um-zu`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 damit vs um ... zu topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 um ... zu topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-um-zu?primaryMeaningLanguageCode=fa`, `/grammar/b1-um-zu`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 um ... zu topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 infinitive with zu topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-infinitive-with-zu?primaryMeaningLanguageCode=fa`, `/grammar/b1-infinitive-with-zu`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 infinitive with zu topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 werden as auxiliary topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-werden-as-auxiliary?primaryMeaningLanguageCode=fa`, `/grammar/b1-werden-as-auxiliary`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 werden as auxiliary topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 passive voice introduction topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-passive-voice-introduction?primaryMeaningLanguageCode=fa`, `/grammar/b1-passive-voice-introduction`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 passive voice introduction topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 Konjunktiv II with wäre/hätte/würde topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-konjunktiv-ii-with-waere-haette-wuerde?primaryMeaningLanguageCode=fa`, `/grammar/b1-konjunktiv-ii-with-waere-haette-wuerde`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 Konjunktiv II with wäre/hätte/würde topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 Konjunktiv II for polite requests topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-konjunktiv-ii-for-polite-requests?primaryMeaningLanguageCode=fa`, `/grammar/b1-konjunktiv-ii-for-polite-requests`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 Konjunktiv II for polite requests topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 relative pronouns in dative topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-relative-pronouns-in-dative?primaryMeaningLanguageCode=fa`, `/grammar/b1-relative-pronouns-in-dative`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 relative pronouns in dative topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 relative pronouns in nominative and accusative topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-relative-pronouns-in-nominative-and-accusative?primaryMeaningLanguageCode=fa`, `/grammar/b1-relative-pronouns-in-nominative-and-accusative`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 relative pronouns in nominative and accusative topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 30/30 after adding the official B1 relative clauses basics topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/b1-relative-clauses-basics?primaryMeaningLanguageCode=fa`, `/grammar/b1-relative-clauses-basics`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official B1 relative clauses basics topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the final official A2 grammar review map topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-a2-grammar-review-map?primaryMeaningLanguageCode=fa`, `/grammar/a2-a2-grammar-review-map`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the final official A2 grammar review map topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 verb review topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-a2-verb-review?primaryMeaningLanguageCode=fa`, `/grammar/a2-a2-verb-review`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 verb review topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 case review topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-a2-case-review?primaryMeaningLanguageCode=fa`, `/grammar/a2-a2-case-review`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 case review topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 connectors overview topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-a2-connectors-overview?primaryMeaningLanguageCode=fa`, `/grammar/a2-a2-connectors-overview`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 connectors overview topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 common mistakes topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-common-a2-mistakes?primaryMeaningLanguageCode=fa`, `/grammar/a2-common-a2-mistakes`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 common mistakes topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 grammar for school/kindergarten communication topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-grammar-for-school-and-kindergarten-communication?primaryMeaningLanguageCode=fa`, `/grammar/a2-grammar-for-school-and-kindergarten-communication`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 grammar for school/kindergarten communication topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 grammar for doctor visits topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-grammar-for-doctor-visits?primaryMeaningLanguageCode=fa`, `/grammar/a2-grammar-for-doctor-visits`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 grammar for doctor visits topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 grammar for appointments topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-grammar-for-appointments?primaryMeaningLanguageCode=fa`, `/grammar/a2-grammar-for-appointments`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 grammar for appointments topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 grammar for phone calls topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-grammar-for-phone-calls?primaryMeaningLanguageCode=fa`, `/grammar/a2-grammar-for-phone-calls`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 grammar for phone calls topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 simple email grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-simple-email-grammar?primaryMeaningLanguageCode=fa`, `/grammar/a2-simple-email-grammar`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 simple email grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 polite forms with würde Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-polite-forms-with-wuerde?primaryMeaningLanguageCode=fa`, `/grammar/a2-polite-forms-with-wuerde`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 polite forms with würde Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 es gibt Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-es-gibt?primaryMeaningLanguageCode=fa`, `/grammar/a2-es-gibt`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 es gibt Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 man as general subject Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-man-as-general-subject?primaryMeaningLanguageCode=fa`, `/grammar/a2-man-as-general-subject`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 man as general subject Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 zu + infinitive introduction Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-zu-plus-infinitive-introduction?primaryMeaningLanguageCode=fa`, `/grammar/a2-zu-plus-infinitive-introduction`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 zu + infinitive introduction Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 time clauses with bevor/nachdem Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-time-clauses-bevor-and-nachdem-introduction?primaryMeaningLanguageCode=fa`, `/grammar/a2-time-clauses-bevor-and-nachdem-introduction`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 time clauses with bevor/nachdem Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 imperative formal and informal Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-imperative-formal-and-informal?primaryMeaningLanguageCode=fa`, `/grammar/a2-imperative-formal-and-informal`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 imperative formal and informal Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 indirect questions introduction Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-indirect-questions-introduction?primaryMeaningLanguageCode=fa`, `/grammar/a2-indirect-questions-introduction`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 indirect questions introduction Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 adjective endings introduction Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-adjective-endings-introduction?primaryMeaningLanguageCode=fa`, `/grammar/a2-adjective-endings-introduction`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 adjective endings introduction Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 superlative basics Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-superlative-basics?primaryMeaningLanguageCode=fa`, `/grammar/a2-superlative-basics`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 superlative basics Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 comparative forms Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-comparative-forms?primaryMeaningLanguageCode=fa`, `/grammar/a2-comparative-forms`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 comparative forms Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 sentence order in subordinate clauses Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-sentence-order-in-subordinate-clauses?primaryMeaningLanguageCode=fa`, `/grammar/a2-sentence-order-in-subordinate-clauses`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 sentence order in subordinate clauses Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 denn vs weil Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-denn-versus-weil?primaryMeaningLanguageCode=fa`, `/grammar/a2-denn-versus-weil`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 denn vs weil Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 wenn for conditions Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-wenn-for-conditions?primaryMeaningLanguageCode=fa`, `/grammar/a2-wenn-for-conditions`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 wenn for conditions Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 weil clauses Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-weil-clauses?primaryMeaningLanguageCode=fa`, `/grammar/a2-weil-clauses`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 weil clauses Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 dass clauses Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-dass-clauses?primaryMeaningLanguageCode=fa`, `/grammar/a2-dass-clauses`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 dass clauses Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 reflexive verbs introduction Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-reflexive-verbs-introduction?primaryMeaningLanguageCode=fa`, `/grammar/a2-reflexive-verbs-introduction`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 reflexive verbs introduction Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 separable verbs in Perfekt Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-separable-verbs-in-perfekt?primaryMeaningLanguageCode=fa`, `/grammar/a2-separable-verbs-in-perfekt`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 separable verbs in Perfekt Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 prepositions with accusative Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-prepositions-with-accusative?primaryMeaningLanguageCode=fa`, `/grammar/a2-prepositions-with-accusative`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 prepositions with accusative Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 prepositions with dative Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-prepositions-with-dative?primaryMeaningLanguageCode=fa`, `/grammar/a2-prepositions-with-dative`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 prepositions with dative Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 Wechselpräpositionen introduction Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-wechselpraepositionen-introduction?primaryMeaningLanguageCode=fa`, `/grammar/a2-wechselpraepositionen-introduction`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 Wechselpräpositionen introduction Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 possessive pronouns in cases Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-possessive-pronouns-in-cases?primaryMeaningLanguageCode=fa`, `/grammar/a2-possessive-pronouns-in-cases`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 possessive pronouns in cases Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 accusative pronouns Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-accusative-pronouns?primaryMeaningLanguageCode=fa`, `/grammar/a2-accusative-pronouns`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 accusative pronouns Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 dative pronouns Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-dative-pronouns?primaryMeaningLanguageCode=fa`, `/grammar/a2-dative-pronouns`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 dative pronouns Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 accusative vs dative basics Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-accusative-versus-dative-basics?primaryMeaningLanguageCode=fa`, `/grammar/a2-accusative-versus-dative-basics`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 accusative vs dative basics Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 dative case basics Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-dative-case-basics?primaryMeaningLanguageCode=fa`, `/grammar/a2-dative-case-basics`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 dative case basics Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 modal verbs in more detail Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-modal-verbs-in-more-detail?primaryMeaningLanguageCode=fa`, `/grammar/a2-modal-verbs-in-more-detail`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 modal verbs in more detail Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 Präteritum of sein and haben Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 common irregular participles Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` added parser and import/query coverage for the official A2 Perfekt with sein Grammar topic.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 28/28 after adding the official A2 Perfekt with haben Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a2-perfekt-with-haben?primaryMeaningLanguageCode=fa`, `/grammar/a2-perfekt-with-haben`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official final A1 grammar review map topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-a1-grammar-review-map?primaryMeaningLanguageCode=fa`, `/grammar/a1-a1-grammar-review-map`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 common A1 grammar mistakes Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-common-a1-grammar-mistakes?primaryMeaningLanguageCode=fa`, `/grammar/a1-common-a1-grammar-mistakes`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 basic appointment phrases Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-basic-appointment-phrases?primaryMeaningLanguageCode=fa`, `/grammar/a1-basic-appointment-phrases`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 basic location phrases Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-basic-location-phrases?primaryMeaningLanguageCode=fa`, `/grammar/a1-basic-location-phrases`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 articles with food, drinks, and shopping nouns Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-articles-with-food-drinks-and-shopping-nouns?primaryMeaningLanguageCode=fa`, `/grammar/a1-articles-with-food-drinks-and-shopping-nouns`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 question-answer sentence patterns Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-question-answer-sentence-patterns?primaryMeaningLanguageCode=fa`, `/grammar/a1-question-answer-sentence-patterns`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 basic sentence negation Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-basic-sentence-negation?primaryMeaningLanguageCode=fa`, `/grammar/a1-basic-sentence-negation`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 du versus Sie grammar basics Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-du-versus-sie-grammar-basics?primaryMeaningLanguageCode=fa`, `/grammar/a1-du-versus-sie-grammar-basics`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 formal Sie Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-formal-sie?primaryMeaningLanguageCode=fa`, `/grammar/a1-formal-sie`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 pronoun and verb agreement Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-pronoun-and-verb-agreement?primaryMeaningLanguageCode=fa`, `/grammar/a1-pronoun-and-verb-agreement`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 simple conjunctions und/aber Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-simple-conjunctions-und-aber?primaryMeaningLanguageCode=fa`, `/grammar/a1-simple-conjunctions-und-aber`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 word order with time and place Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-word-order-with-time-and-place?primaryMeaningLanguageCode=fa`, `/grammar/a1-word-order-with-time-and-place`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 time expressions Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-time-expressions-heute-morgen-gestern?primaryMeaningLanguageCode=fa`, `/grammar/a1-time-expressions-heute-morgen-gestern`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 numbers and grammar use Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-numbers-and-grammar-use?primaryMeaningLanguageCode=fa`, `/grammar/a1-numbers-and-grammar-use`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 basic prepositions Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-basic-prepositions-in-aus-nach-bei?primaryMeaningLanguageCode=fa`, `/grammar/a1-basic-prepositions-in-aus-nach-bei`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 basic adjective position Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-basic-adjective-position?primaryMeaningLanguageCode=fa`, `/grammar/a1-basic-adjective-position`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 possessive pronouns Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-possessive-pronouns-mein-dein?primaryMeaningLanguageCode=fa`, `/grammar/a1-possessive-pronouns-mein-dein`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 kein vs nicht basics Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-kein-versus-nicht-basics?primaryMeaningLanguageCode=fa`, `/grammar/a1-kein-versus-nicht-basics`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 simple accusative introduction Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-simple-accusative-introduction?primaryMeaningLanguageCode=fa`, `/grammar/a1-simple-accusative-introduction`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 nominative case Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-nominative-case?primaryMeaningLanguageCode=fa`, `/grammar/a1-nominative-case`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 plural basics Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-plural-basics?primaryMeaningLanguageCode=fa`, `/grammar/a1-plural-basics`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 noun gender basics Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-noun-gender-basics?primaryMeaningLanguageCode=fa`, `/grammar/a1-noun-gender-basics`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 indefinite articles Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-indefinite-articles-ein-eine?primaryMeaningLanguageCode=fa`, `/grammar/a1-indefinite-articles-ein-eine`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 definite articles Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-definite-articles-der-die-das?primaryMeaningLanguageCode=fa`, `/grammar/a1-definite-articles-der-die-das`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 W-questions Grammar topic.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-w-questions-wer-was-wo-wann-wie?primaryMeaningLanguageCode=fa`, `/grammar/a1-w-questions-wer-was-wo-wann-wie`, and `/grammar`.
- 2026-05-17: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 26/26 after adding the official A1 yes/no questions Grammar topic.
- 2026-05-17: `DarwinLingua.Catalog.Application.Tests` passed 31/31.
- 2026-05-17: `DarwinLingua.WebApi.Tests` passed 51/51 with `--no-build`; rebuild was blocked by running local `DarwinLingua.Web.exe` and `DarwinLingua.WebApi.exe` processes locking debug outputs.
- 2026-05-17: development smoke returned HTTP 200 for `/api/catalog/grammar/a1-yes-no-questions?primaryMeaningLanguageCode=fa`, `/grammar/a1-yes-no-questions`, and `/grammar`.
- 2026-05-12: `DarwinLingua.WebApi` build passed with 0 warnings and 0 errors.
- 2026-05-12: `DarwinLingua.Web` build passed with 0 warnings and 0 errors after duplicate localization key cleanup.
- 2026-05-12: `DarwinLingua.WebApi.Tests` passed 36/36.
- 2026-05-12: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 23/23, including Phase 7 parser contract tests.
- 2026-05-12: `DarwinLingua.Catalog.Application.Tests` passed 21/21.
- 2026-05-12: `DarwinLingua.Learning.Application.Tests` passed 29/29.
- 2026-05-12: `DarwinLingua.Learning.Domain.Tests` passed 47/47.
- 2026-05-12: `DarwinLingua.Localization.Application.Tests` passed 24/24, including Phase 7 Web route, WebApi route, and English/German localization-key hardening checks.
- 2026-05-12: full `DarwinLingua.slnx` build passed with 0 warnings and 0 errors when run sequentially with `-m:1`.
- 2026-05-01: `DarwinLingua.Web` build passed with 0 warnings and 0 errors.
- 2026-05-01: `DarwinLingua.WebApi` build passed with 0 warnings and 0 errors after stopping the local smoke host that was locking build outputs.
- 2026-05-01: local GET smoke passed for the main learner routes: `/`, `/browse`, `/browse/cefr/A1`, `/search`, `/search?q=auto`, unknown-search suggestion state, `/collections`, `/dialogues`, `/conversation-starters`, `/conversation-events`, `/organizers`, `/install`, `/privacy`, and the Identity account pages.
- 2026-05-01: authenticated admin GET smoke passed for dashboard, reports, analytics, diagnostics, content import/history/draft/publishing/rollback pages, catalog management pages, taxonomy pages, users, moderation, word suggestions, organizer profiles, conversation events, billing diagnostics, and email diagnostics.
- 2026-05-01: smoke response bodies and server logs were checked for unhandled exception signatures; none were found.
- 2026-05-01: in-app browser opened `/admin/billing-diagnostics` successfully. Local Stripe readiness warnings were expected because local Stripe billing settings were not enabled/configured.
- 2026-05-01: local security smoke verified external `returnUrl` values are not rendered back into the Identity login hidden field; the value is normalized to `/`.
- 2026-05-01: local webhook smoke verified unsigned Stripe billing webhooks return `401`, and client telemetry accepts a bounded valid payload.
- 2026-05-01: provider-error logging was reviewed for Brevo and Stripe billing paths; external provider response bodies are not logged for email send, checkout, portal, fulfillment, or reconciliation failures.

## Automated Test Backlog

### Talk Topics

- [x] Import parser accepts valid `talkTopics` package contracts.
- [x] Import validation rejects German articles outside CEFR character target ranges.
- [x] Import validation rejects Talk Topic article and question translations.
- [x] Import validation enforces warm-up counts, discussion question counts by type, vocabulary count ranges, and speaking-goal count ranges.
- [x] Import validation rejects invalid content type, question type, and speaking goal values.
- [x] Persistence stores German-only article, German-only questions, vocabulary references, speaking goals, content type, and sensitivity fields.
- [x] Generated Talk Topic package validation checks CEFR character ranges, no article/question translations, required question counts, vocabulary counts, content types, speaking goals, duplicate slugs, and distinct topic groups.
- [x] List query filters by CEFR level, category, topic, content type, speaking goal, and sensitivity.
- [ ] Detail query resolves vocabulary references against the Word Catalog where possible.
- [x] WebApi list/detail endpoints return successful Talk Topic payloads in smoke coverage.
- [ ] Web detail rendering does not fail when a vocabulary `wordSlug` cannot be resolved.
- [ ] German and English localization keys cover Talk Topics, content types, speaking goals, and sensitivity warnings.
- [x] Catalog package export includes Talk Topics where expected.
- [ ] Mobile UI parity for Talk Topics after the Web flow is signed off.

### Dialogue Learning

- [x] Dialogue import rejects malformed dialogue packages with clear issue messages for missing metadata, unsupported values, missing useful words, missing speaking prompts, and insufficient sentence counts.
- [x] Dialogue import persists lesson metadata, topics, dialogue turns, useful phrases, questions, answers, translations, useful words, and speaking prompts.
- [x] Dialogue list query returns only published dialogues in stable sort order and supports CEFR, category, topic, exam profile, skill focus, task type, interaction mode, register, and search filters.
- [ ] Dialogue detail query applies primary and secondary meaning-language selection.
- [x] Web dialogue detail rendering shows dialogue metadata, useful phrases, useful words, speaking prompts, quick checks, related starter packs, and related preparation packs.
- [x] Web dialogue list includes prominent CEFR filtering and metadata badges.
- [ ] Web roleplay sequence builder skips learner prompts and pairs each non-learner prompt with the next learner model answer.
- [ ] Web roleplay page renders model answers, static feedback, and no-AI behavior.
- [ ] Empty or unknown dialogue detail payloads return safe 404 behavior instead of Web 500 errors.
- [ ] Human-review generated Dialogue translations before public launch; current starter generation preserves required language slots for import integrity.
- [ ] Add full mobile Dialogue metadata/detail parity after Web sign-off.

### Standalone RoleplayScenario

- [x] Parser accepts the top-level `roleplayScenarios` array and maps `linkedDialogueSlug` / compatibility `scenarioSlug`.
- [x] Application import test persists a valid standalone RoleplayScenario package.
- [x] Application validation rejects invalid CEFR, malformed slugs, duplicate turn sort orders, missing playable sequence, missing translations, answer-choice groups without a correct choice, invalid skill focus, and invalid exam profile.
- [x] Repository list/detail tests cover CEFR, category, topic, exam profile, skill focus, task type, interaction mode, register, and search filters.
- [x] WebApi route structural tests cover `/api/catalog/roleplays` and `/api/catalog/roleplays/{slug}`.
- [x] Web structural tests cover `/roleplays`, `/roleplays/{slug}`, deterministic answer choices, static feedback, and missing image assets without broken images or learner-visible image prompts.
- [x] Unified Learning Search tests include the `roleplay` result type.
- [x] Admin report tests include RoleplayScenario count visibility by type and CEFR.
- [x] PostgreSQL integration tests cover RoleplayScenario repository filtering, detail projection, and Unified Learning Search using PostgreSQL-native search semantics.
- [x] Local shared-database import and Web/API/search smoke passed before and after the first roleplay pilot package import.
- [x] Add deeper admin quality counters for RoleplayScenario missing translations, unpublished drafts, missing required image assets, missing answer choices/static feedback, and invalid playable sequence after more reviewed packages exist.
- [x] Migrate Web/API-critical SQLite-backed tests to PostgreSQL when the behavior under test depends on PostgreSQL provider semantics.
  - Evidence: WebApi service tests, Identity bootstrapper tests, RoleplayScenario repository/search tests, admin-report tests, server-content manifest/delivery tests, and catalog publication lifecycle tests now use temporary PostgreSQL databases. The remaining SQLite mentions in WebApi tests are structural assertions that Web startup does not register SQLite.

### Conversation Starters

- [ ] Starter import rejects invalid filters, linked dialogue slugs, and invalid dual-language payloads.
- [ ] Starter list query filters by CEFR level, situation, tone, and conversation goal.
- [ ] Starter detail query renders primary and optional secondary meaning languages.
- [ ] Dialogue detail query returns related starter packs for linked dialogues.
- [ ] Web starter detail page renders phrase alternatives, usage notes, register, common mistakes, and dual-language text.

### Event Preparation Packs

- [ ] Preparation-pack import rejects invalid linked dialogues, vocabulary references, starter links, and prompt types.
- [ ] Preparation-pack persistence stores prompts, vocabulary references, topic links, dialogue links, and starter-pack links.
- [ ] Event-to-preparation-pack mapping returns only published packs linked to the active event.
- [ ] Web event detail renders entitled preparation-pack links.
- [ ] Web preparation-pack detail renders prompts, vocabulary, related dialogues, and related starter packs.
- [ ] `Mark prepared` records aggregate completion analytics and redirects back to the preparation-pack detail page.
- [ ] Direct preparation-pack access is forbidden when the learner lacks the entitlement.

### Event Directory And Organizer Tools

- [ ] Event filtering covers city, online/offline, CEFR level, helper language, price type, date, and publication status.
- [ ] Event detail respects public/private projection rules.
- [ ] Organizer ownership rules restrict dashboard event management to assigned owners.
- [ ] Organizer dashboard enforces active-event limits by organizer plan.
- [ ] RSVP flow enforces capacity, duplicate registration rules, cancellation, waitlist behavior if enabled, and attendance updates.
- [ ] Organizer visibility rules hide archived/unpublished/private records from public pages.

### Learner Profiles And Partner Matching

- [ ] Public learner profile projection excludes private contact details.
- [ ] Learner profile anonymization clears public/private profile fields and disables matching.
- [ ] Partner request transitions cover pending, accepted, declined, cancelled, and blocked states.
- [ ] Partner request rate limits suppress excessive requests.
- [ ] Accepted partner requests reveal contact details only after mutual consent.
- [ ] Blocked users are hidden from matching results and cannot create new requests.

### Moderation And Safety

- [ ] User report creation validates target, reason, and reporter context.
- [ ] User block creation suppresses matching and contact surfaces.
- [ ] Admin moderation queue filters by status, reason, target type, and assigned state.
- [ ] Moderation decision audit captures decision type, actor, target, notes, and timestamps.
- [ ] Report/block visibility rules are enforced in public pages and matching APIs.

### Admin Reporting And Seed Coverage

- [x] Admin system report endpoint returns catalog, social-learning, moderation, operations, and Learning Portal counts from the server database.
- [x] Web Admin Reports page combines system report counts, Identity user count, Web analytics counters, and Learning Portal quality/coverage tables.
- [x] Admin Reports authorization requires the Admin policy.
- [x] Learning Portal report test covers content counts, CEFR coverage, unresolved content links, missing translations, and missing exercise coverage.
- [ ] Admin dashboard links to Reports and the implemented management pages.
- [ ] Web seed fixtures include multiple records for dialogues, starters, preparation packs, organizers, events, RSVPs, claims, learner profiles, partner requests, reports, blocks, and moderation audits.
- [ ] Operational seed loading path is validated once a dedicated event-directory/safety seed applier exists.
- [ ] Web rendering coverage exists for the Learning Portal coverage and issue-sample tables.

### Entitlements And Feature Gates

- [ ] Free, Trial, and Premium entitlement states resolve expected feature flags.
- [ ] Expired trial and expired premium states fall back to free access.
- [ ] Web feature gates hide or block premium preparation packs and advanced features.
- [ ] WebApi feature gates reject direct unauthorized calls.
- [ ] Admin entitlement changes write audit records and update effective access immediately.

### Billing And Payments

- [ ] Billing page renders the current entitlement and disables checkout when Stripe is off.
- [ ] Billing page starts Stripe Checkout only for authenticated users and only when Stripe is enabled.
- [ ] Billing page shows Stripe Customer Portal management only after a customer id is linked to the account.
- [ ] Stripe Customer Portal session creation redirects only for the authenticated user's linked Stripe customer.
- [ ] `/billing/success` fetches the returned Stripe checkout session and immediately syncs entitlement only when the session belongs to the authenticated user.
- [ ] `/billing/success` only fulfills completed subscription checkout sessions with paid or no-payment-required status and customer/subscription ids.
- [ ] `/billing/success` falls back to webhook-based activation when Stripe lookup fails or session is not complete.
- [ ] `checkout.session.completed` webhook rejects non-subscription, unpaid, or incomplete checkout sessions before granting Premium.
- [ ] Stripe Checkout, Stripe Customer Portal, and Admin reconciliation enforce bounded rate limits with safe user-facing messages.
- [ ] Stripe Checkout session creation sends the Darwin Lingua user id in session and subscription metadata.
- [ ] Stripe Checkout failures show a safe user-facing message and do not leak provider response bodies.
- [ ] Stripe webhook rejects missing, malformed, expired, or invalid signatures.
- [ ] Duplicate Stripe webhook event ids are idempotent.
- [ ] `checkout.session.completed` maps the Stripe session to the correct user and grants Premium.
- [ ] `customer.subscription.created` and `customer.subscription.updated` persist customer/subscription ids and current period end.
- [ ] Active or trialing Stripe subscription states keep Premium access.
- [ ] Cancelled, unpaid, or incomplete-expired Stripe subscription states downgrade the user to Free.
- [ ] Unmapped Stripe subscription events fail closed and are visible in billing-event diagnostics/logs.
- [ ] Admin Billing Diagnostics renders Stripe readiness without exposing secret values.
- [ ] Admin Billing Diagnostics filters billing events by status, event type, user id, customer id, and subscription id.
- [ ] Admin Billing Diagnostics filters billing profiles by user id, customer id, and subscription id.
- [ ] Admin-only Stripe subscription reconciliation rejects malformed subscription ids.
- [ ] Admin-only Stripe subscription reconciliation fetches current Stripe subscription state without storing raw provider payloads.
- [ ] Admin-only Stripe subscription reconciliation updates billing profile and entitlement for active, trialing, past-due, cancelled, unpaid, and expired states.
- [ ] Billing notification emails render for Premium activation, payment action needed, Premium ended, and Admin reconciliation completed.
- [ ] Billing notification emails are logged through the transactional email delivery log without storing Stripe raw payloads.
- [ ] Billing notification emails are idempotent per scenario, user, subscription, and billing status.
- [ ] Stripe webhook processing sends user billing emails for checkout completion, payment-action-needed states, and Premium-ended states.
- [ ] Admin reconciliation sends both user billing status email and admin reconciliation-completed email when recipients are configured.

### Web Runtime And Bootstrap

- [ ] PostgreSQL startup bootstrap retrofits Phase 6 catalog tables on an existing shared database.
- [ ] Empty optional Identity and catalog connection strings fall back to the shared server-content database.
- [ ] WebCatalogApiClient treats empty successful detail responses as `null`.
- [ ] Local server bootstrap handles both folder imports and single-file imports under `Set-StrictMode`.

## Learning Portal Expansion

### Foundation

- [x] Web learner navigation groups implemented routes under Learn, Practice, Speak, Prepare, and Resources.
- [x] Web learner navigation avoids dead routes for future Phase 7 modules.
- [x] Phase 7 Web learner routes are covered by structural route tests for Grammar, Expressions, Exercises, Courses, Exam Prep, Writing Templates, Cultural Notes, and Unified Search.
- [x] Phase 7 WebApi route registrations are covered by structural route tests for module list/detail endpoints, exercise attempts, unified search, progress summary/update, and recommendations.
- [x] Phase 7 English/German localization resource keys are covered by structural tests for the release route surface.
- [x] Shared CEFR filter conventions expose stable A1-C2 values for reusable Web filters.
- [ ] Cross-content linking helper coverage once a real Phase 7 module consumes the model.
- [x] Admin/system report coverage exists for persisted Phase 7 module counts and quality metrics.

### Grammar Guide

- [x] Parser coverage exists for the GrammarTopic content contract shape.
- [x] Parser/import coverage exists for the rich localized Grammar pilot package, including localized title/description and table blocks.
- [x] Parser/import coverage exists for the first official A1 Grammar package, including all 10 languages, 6 sections, 20 examples, and 10 common mistakes.
- [x] Navigation/localization smoke coverage includes the live Grammar Guide route.
- [x] Release route hardening covers `/grammar` and `/api/catalog/grammar`.
- [x] Import validation covers required rich section keys, supported block types, localized block language codes, and table/callout block shape.
- [x] List/detail query coverage includes the first rich pilot topic and localized fallback behavior.
- [ ] CEFR/category/topic/search filters return expected grammar topics.
- [x] Web/API rendering handles paragraph, table, and callout rich blocks from the official A1 personal-pronouns package.
- [ ] Add a future reviewed cross-level validation batch after the official A1 core package establishes the production content pattern.
- [ ] Linked words/dialogues/Talk Topics/exercises render where available.
- [ ] Localized explanation rendering follows learner language preferences.
- [ ] Unresolved links fail safely without Web 500 errors.

### Everyday Expressions

- [x] Parser coverage exists for the ExpressionEntry content contract shape.
- [x] Navigation/localization smoke coverage includes the live Everyday Expressions route.
- [x] Release route hardening covers `/expressions` and `/api/catalog/expressions`.
- [x] Expression type/register validation rejects unsupported values.
- [x] Published `ordinary-literal` Expressions are rejected by import validation when classified.
- [x] Non-literal and semi-idiomatic Expressions require literal meaning metadata.
- [x] Pragmatic fixed formulas such as `Alles klar` are accepted when classified and explained.
- [x] Classified published Expressions require at least two German examples.
- [x] Risky expression validation rejects entries without required warning text.
- [x] Explicit-adult metadata requires warning/access consistency in import validation.
- [x] Public list/detail/search hides adult-only Expressions by default.
- [x] Sensitive Educational Language metadata validation rejects blocked-illegal, explicit-adult, verified-adult-required, missing-warning, and missing-usage-policy official entries.
- [x] Sensitive Educational Language entries are hidden from default list/detail/search queries.
- [x] Opted-in query paths can include allowed sensitive educational entries while still hiding verified-adult-required and blocked entries.
- [x] Admin report coverage includes Expression counts by safety rating, sensitive content kind, usage policy, opt-in requirement, verified-adult requirement, blocked/explicit entries, and missing sensitive warnings.
- [x] Parser coverage imports the first official pilot Expressions package from disk.
- [x] Import validation coverage accepts pilot-style all-language expression data.
- [x] List/detail queries return published expressions in stable order.
- [x] CEFR/type/register/context filters return expected expressions.
- [x] Risky expression warnings render structurally for unsafe tone or context.
- [x] Linked words and related expressions render structurally where available.
- [x] Unresolved related expression and linked exercise slugs project safely without repository failures.
- [x] Unified Search repository coverage returns Expression results from seeded content.
- [x] Admin report coverage includes Expression counts by type and register.
- [x] Admin report coverage includes Expression counts by meaning transparency and safety rating plus quality counters for missing metadata, ordinary-literal leakage, missing teaching reasons, low example counts, missing warnings, and adult-access count.
- [ ] Add authenticated WebApi/profile-bound eligibility tests for Sensitive Educational Language preference instead of relying on explicit query flags.
- [ ] Add mobile export tests proving Sensitive Educational Language and adult-only Expressions are excluded until mobile eligibility enforcement exists.
- [ ] Add settings UI smoke coverage for the English/German Sensitive Educational Language checkbox copy.
- [ ] Add Web detail rendering coverage for `usagePolicy: understand-only` and `usagePolicy: do-not-use`.
- [x] Live Web/API smoke for the imported pilot package remains required before bulk Expressions generation.
  - Evidence: local target/dev services backed by `darwinlingua_shared` returned Expression list/detail/search/admin report data after the pilot import. Public `linguaapi.vafadar.pro`/`lingua.vafadar.pro` returned 502 during this pass and should be checked separately as an environment/reverse-proxy issue.

### Exercise Engine

- [x] Parser coverage exists for the Exercise and ExerciseSet content contract shape.
- [x] Release route hardening covers `/exercises`, exercise-set/detail endpoints, and exercise attempt submission route registration.
- [x] Exercise type validation rejects unsupported types.
- [x] Answer key validation rejects missing or malformed deterministic answers.
- [x] Deterministic feedback returns stable correct/incorrect outcomes.
- [ ] Exercise set linking resolves valid owner references.
- [x] Exercise runner behavior covers structured choice prompts and malformed prompt fallback.
- [x] Authenticated attempt persistence stores the authenticated user id and never falls back to an `anonymous` user id.
- [x] Public exercise evaluation is stateless and does not persist anonymous attempts.
- [x] Malformed and oversized submitted-answer JSON is rejected before persistence.
- [x] Exercise attempt and evaluation endpoints are covered by rate-limiting structural checks.
- [x] Attempt results do not expose answer keys.
- [x] Type-specific runner controls cover initial choice, single-answer, error-correction, sentence-ordering, and matching submission shapes.
- [x] Seeded deterministic evaluator coverage verifies each initial exercise type against representative answer-key shapes.
  - Status: first real package `content/learning-portal/exercises/packages/exercises-a1-a2-core-01-v1.json` imported into shared PostgreSQL on 2026-05-31 with 12 translated exercises and one translated exercise set. Targeted tests now cover parser translations, import validation, all 12 evaluator shapes, Web runner projection, exercise endpoints, and admin report counters.
- [x] Exercise package localization is validated for active learner meaning languages and projected through API/Web as helper text while German source remains canonical.

### Course Lessons

- [x] Parser coverage exists for the CoursePath/CourseModule/CourseLesson content contract shape.
- [x] Release route hardening covers `/courses`, course list/detail endpoints, and course lesson detail route registration.
- [x] Parser/import validation covers Course learner-helper translations and `learningGoalsTranslations`.
- [x] PostgreSQL repository coverage verifies Course list/detail search and localized helper projection.
- [x] Admin report coverage includes Course translation quality counters.
- [x] Admin report coverage includes Course activity-flow quality counters and drilldown issue rows.
- [x] Lesson/module/course ordering is stable for the imported A1-C2 Course baseline.
- [ ] Backfill `activityBlocks` for A1 Module 1 in a small cumulative package and smoke one activity-enabled lesson page.
- [ ] Linked content rendering covers grammar, words, expressions, dialogues, Talk Topics, and exercises.
- [x] Prerequisite and next-lesson navigation resolves for the imported A1-C2 Course baseline.
- [ ] WebApi list/detail endpoint coverage exists.
- [ ] Progress tracking works where implemented.
- [x] Browser smoke coverage exists for `/courses`, `/courses/{slug}`, and `/courses/{courseSlug}/{lessonSlug}` after reviewed Course imports.
- [x] Run a broader Course Web/API/admin smoke pass after C2 is fully generated/imported.
  - 2026-06-08 evidence: Web and WebApi were restarted locally after repairing PostgreSQL startup retrofit for `ExamProfiles`, `ExamPrepUnits`, `WritingTemplates`, and `CulturalNotes`. Course Web routes, WebApi course list/detail/lesson/search routes, and service-level admin report tests passed. The anonymous admin endpoint returns 401 as expected.

### Exam Preparation

- [x] Parser coverage exists for the Exam Prep content contract shape.
- [x] Import validation covers supported profile taxonomy, controlled unit fields, active learner-language helper translations, and exact English fallback rejection.
- [x] Navigation/localization shell includes Exam Prep.
- [x] Release route hardening covers `/exam-prep`, exam profile, and exam prep list/detail route registrations.
- [x] Exam profile taxonomy validates supported profiles and task types in targeted ContentOps coverage.
- [x] Exam unit linking resolves reviewed Course, Dialogue, Roleplay, Exercise, Grammar, Expression, Writing Template, and Talk Topic slugs without invented future links for the imported foundation packages.
- [x] Exam filter behavior covers profile, CEFR, skill focus, task type, section, and query in PostgreSQL repository coverage.
- [x] Regenerated Exam Prep pilot/foundation rendering works after the rejected A1/A2 pilot was rebuilt from reviewed planning.
- [x] Exam Prep content preflight flags titles that repeat CEFR/profile/section metadata and helper translations that are literal but unnatural in Persian or other learner languages.
- [x] Unified Search returns `exam-prep` result type from seeded PostgreSQL repository coverage.
- [x] Admin report service coverage includes Exam Prep missing translations, unpublished drafts, malformed strategy/checklist JSON, and units without an active profile.
  - 2026-06-09 evidence: reviewed packages through `exam-prep-c2-foundation-05-v1.json` imported into `darwinlingua_shared` with zero warnings. Local and production-routed smoke passed, and the C2 completion backup records matching live and restored counts.

### Writing Templates

- [x] Parser coverage exists for the WritingTemplate content contract shape.
- [x] Variable validation requires declared placeholders to exist in template text.
- [x] Variable validation rejects placeholders used in template text but missing from the declared variable list.
- [x] Helper translation validation rejects missing active learner languages, unsupported/duplicate languages, and exact English fallback in non-English helper fields.
- [x] Release route hardening covers `/writing-templates` and writing-template list/detail route registrations.
- [x] Sample filled version rendering works for published templates.
- [x] Linked grammar/words/exercises/course lessons render where available.
- [x] WebApi list/detail structural coverage exists.
- [x] PostgreSQL repository coverage verifies filters, search, and learner helper projection.
- [x] Imported A1-C2 baseline has live Web/API smoke coverage for list/detail, Persian helper projection, and Unified Search.
- [ ] Variable rendering substitutes supported placeholders safely in an interactive editor flow.

### Life in Germany

- [x] Parser coverage exists for the CulturalNote content contract shape.
- [x] Navigation/localization shell includes Life in Germany.
- [x] Release route hardening covers public `/life-in-germany` Web routes and internal cultural-note API registrations.
- [x] List/detail queries return published cultural notes in stable order.
- [x] Filtering covers CEFR/category/context where supported.
- [x] WebApi list/detail endpoint coverage exists for the internal `/api/catalog/cultural-notes` API.
- [x] Web list/detail rendering coverage exists for `/life-in-germany`.
- [x] Linked content rendering covers dialogues, expressions, writing templates, Talk Topics, and course lessons.
  - Evidence: `CulturalNoteRouteStructuralTests` covers public route naming, helper-language rendering, RTL direction hooks, and linked-content surface; `CulturalNotePostgresRepositoryTests` covers PostgreSQL filtering, stable ordering, localized helper projection, detail links, and Unified Search URL projection.

### Unified Search

- [x] Application-level empty query handling avoids repository calls.
- [x] Application-level short, long, and unsupported result-type query handling is covered.
- [x] Application-level result projection returns repository results unchanged.
- [x] Release route hardening covers `/search` and `/api/catalog/search`.
- [x] `/api/catalog/search` is covered by rate-limiting structural checks.
- [x] PostgreSQL trigram and filter-index migration coverage exists for the bulk-content search path.
- [x] Shared database startup applies PostgreSQL trigram/filter indexes for existing search tables and skips not-yet-created Phase 7 tables safely.
- [x] Result type projection distinguishes current Web learning types with seeded data.
- [x] Ranking behavior is deterministic for the same indexed content in repository/WebApi coverage.
- [x] CEFR/content type/category filters return expected mixed results.
- [x] WebApi endpoint coverage exists for `/api/catalog/search`.
- [x] Web rendering coverage exists for learning result cards and filters.
- [x] Missing content references fail safely.
- [x] Seeded performance coverage verifies bounded result counts and acceptable query plans before bulk content generation.
  - Evidence: Unified Search structural tests cover endpoint/query/filter/card behavior; PostgreSQL repository tests cover cross-type ranking/filter/URL projection and seeded bulk-corpus bounds across Course, Grammar, Writing Templates, and Life in Germany.

### Progress And Personalization

- [x] Domain tests cover supported owner types and progress state transitions.
- [x] Application tests cover viewed/completed updates, summary counts, and deterministic recommendation exclusion for completed content.
- [x] Release route hardening covers progress summary/update and recommendations route registrations.
- [x] WebApi endpoint coverage exists for authenticated `/api/learning/progress/summary`.
- [x] WebApi endpoint coverage exists for authenticated `/api/learning/progress/content`.
- [x] WebApi endpoint coverage exists for `/api/learning/recommendations`.
- [x] Anonymous Web users fall back to existing guest actor behavior without breaking recent activity.
- [x] Course lesson pages render viewed/completed state where progress exists.
- [x] Recent activity dashboard renders cross-content progress summary.
- [x] Recommendations remain deterministic and do not use AI ranking.
  - Evidence: Learning progress structural tests cover authenticated endpoint registrations, `GetRequiredUserId`, course progress chips, recent progress summary, weak-exercise recommendations, difficult-word recommendations, and deterministic recommendation reader signals.

### 2026-06-14 Web Readiness Manual Smoke

- [x] Desktop in-app Chromium smoke covers `/courses`, `/courses/a1-einstieg-in-den-alltag`, `/exercises`, `/exam-prep`, `/exam-prep/profile/goethe-c1`, `/exam-prep/c1-pruefungsanforderungen-einordnen`, `/writing-templates`, `/writing-templates/a1-kurze-vorstellung-nachricht`, `/life-in-germany`, `/life-in-germany/a1-sie-und-du-im-alltag`, `/search?q=Demokratie&resultType=cultural-note`, and `/recent`.
- [x] Account/admin anonymous smoke covers `/Identity/Account/Login`, `/Identity/Account/Register`, `/account`, `/admin`, `/admin/reports`, and `/admin/reports/learning-portal-issues`; protected routes redirect to Login with a local `ReturnUrl`.
- [x] Narrow 390px viewport smoke covers the long-text pages `/writing-templates/a1-kurze-vorstellung-nachricht`, `/life-in-germany/a1-sie-und-du-im-alltag`, `/exam-prep/c1-pruefungsanforderungen-einordnen`, and `/search?q=Demokratie&resultType=cultural-note` with no horizontal overflow.
- [x] Local API smoke covers Persian helper projection for Life in Germany, Writing Templates, and Exam Prep detail endpoints plus Unified Search result types `cultural-note` and `writing-template`.
- [x] Shared PostgreSQL content counts match the Web-readiness baseline: `CourseLessons=560`, `WritingTemplates=120`, `ExamPrepUnits=246`, and `CulturalNotes=30` (`A1=10`, `A2=10`, `B1=10`).

### Deferred Mobile Parity Tracking

Mobile is outside the active Web-readiness path. These items remain post-Web work and should not block Web tester onboarding.

- [x] Web sign-off is recorded before MAUI parity starts.
- [x] Mobile package export structural coverage confirms Phase 7 arrays are present in full/catalog-full packages.
- [x] Web startup structural coverage confirms the Web app does not register local SQLite learning/content initialization.
- [x] WebApi manifest/package tests cover module-scoped `catalog-module` packages.
- [x] MAUI route/localization structural coverage confirms Learning Portal list/detail/search routes and Learn/Practice/Speak/Prepare/Resources navigation labels.
- [x] Full mobile replacement script coverage confirms Phase 7 content tables are copied from remote package imports.
- [x] Module replacement script coverage confirms MAUI can request and apply selective module packages.
- [ ] Add seeded module-slice package tests that import one selected module without removing unrelated local modules.
- [ ] Add first-run onboarding UI automation for choose/skip flows.
- [ ] Add seeded mobile package export tests that import a package with all Phase 7 module types into a local SQLite database.
- [ ] Add MAUI smoke coverage for opening Learning Portal list/detail/search pages on target devices.
- [x] Add manual mobile validation worksheet entries for Phase 7 offline behavior and local package update behavior.
  - Evidence: `artifacts/validation/phase7-mobile-validation-worksheet.md` covers device matrix, first-run module selection, module-scoped package updates, offline behavior, Phase 7 content surfaces, and progress/account boundaries.

## Manual Validation Backlog

### Browser And Device Matrix

- [ ] Validate English UI in desktop Chromium.
- [ ] Validate German UI in desktop Chromium.
- [ ] Validate responsive layout on narrow mobile viewport.
- [ ] Validate PWA install flow on Android Chrome.
- [ ] Validate PWA install flow on desktop Chromium.
- [ ] Validate offline behavior for installed Web/PWA shell.

### Learner Workflows

- [ ] Browse by CEFR.
- [ ] Browse by topic.
- [ ] Search and open word detail.
- [ ] Favorite/unfavorite word.
- [ ] Recent activity.
- [ ] Meaning-language preferences.
- [ ] Dialogue list/detail.
- [ ] Dialogue roleplay.
- [ ] Conversation starter list/detail.
- [ ] Event directory list/detail.
- [ ] Event preparation pack detail and `Mark prepared`.

### Account And Admin Workflows

- [ ] Register learner.
- [ ] Sign in/out.
- [ ] Seeded admin login.
- [ ] Seeded learner login.
- [ ] Admin import/drafts/history/publish/rollback.
- [ ] Admin user entitlement management.
- [ ] Admin organizer/event management.
- [ ] Admin moderation queue and decision logging.
- [ ] Admin reports summary.

## Out Of Scope For This Web Test Backlog

- Mobile screen implementation.
- MAUI UI automation.
- Mobile offline catalog validation.
- Payment-provider integration.
- AI roleplay or AI feedback.

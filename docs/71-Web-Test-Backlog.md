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
- 2026-06-18: Talk Topic vocabulary references now fail safely. PostgreSQL repository tests verify known vocabulary resolves through the Word Catalog while unresolved vocabulary returns without a public word slug, and Web structural tests verify detail pages only link resolved words with the `wordSlug` route parameter.
- 2026-06-18: Talk Topic Web localization keys now cover list/detail labels, content types, discussion question types, speaking goals, and sensitivity warnings in English and German. Structural tests verify the complete key set, Web build passes without resource warnings, and public HTML smoke confirms the article content-type label is rendered as localized text instead of raw `article`.
- 2026-06-18: Web PWA shell readiness now has a real desktop Chromium smoke pass. `/manifest.webmanifest`, `/sw.js`, `/offline.html`, and `/images/logo.png` returned HTTP 200 locally; `WebPwaInstallStructuralTests` passed 2/2; desktop Chromium registered the service worker at `https://localhost:7501/`, created cache `darwin-lingua-shell-v3`, cached `/offline.html`, and rendered `Offline - Darwin Lingua` from the cached shell after the Web host was intentionally stopped.
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
- [x] Detail query resolves vocabulary references against the Word Catalog where possible.
- [x] WebApi list/detail endpoints return successful Talk Topic payloads in smoke coverage.
- [x] Web detail rendering does not fail when a vocabulary `wordSlug` cannot be resolved.
- [x] German and English localization keys cover Talk Topics, content types, speaking goals, and sensitivity warnings.
- [x] Catalog package export includes Talk Topics where expected.
- [ ] Mobile UI parity for Talk Topics after the Web flow is signed off.

### Dialogue Learning

- [x] Dialogue import rejects malformed dialogue packages with clear issue messages for missing metadata, unsupported values, missing useful words, missing speaking prompts, and insufficient sentence counts.
- [x] Dialogue import persists lesson metadata, topics, dialogue turns, useful phrases, questions, answers, translations, useful words, and speaking prompts.
- [x] Dialogue list query returns only published dialogues in stable sort order and supports CEFR, category, topic, exam profile, skill focus, task type, interaction mode, register, and search filters.
- [x] Dialogue detail query applies primary and secondary meaning-language selection.
  - Evidence 2026-06-18: `DialogueLessonPostgresRepositoryTests` verifies PostgreSQL detail projection for Persian primary and English secondary meanings across speaking prompts, turns, useful phrases, questions, and answers, including the no-secondary case. Public smoke for `a1-cafe-order` returned Persian primary and English secondary meanings from `/api/catalog/dialogues/{slug}`.
- [x] Web dialogue detail rendering shows dialogue metadata, useful phrases, useful words, speaking prompts, quick checks, related starter packs, and related preparation packs.
- [x] Web dialogue list includes prominent CEFR filtering and metadata badges.
- [x] Web roleplay sequence builder skips learner prompts and pairs each non-learner prompt with the next learner model answer.
  - Evidence 2026-06-18: `DialoguesControllerDualMeaningLanguageTests.Roleplay_ShouldSkipLearnerPromptsAndPairNonLearnerPromptsWithNextLearnerAnswer` verifies the Web roleplay view model omits learner prompts, pairs staff/pharmacist prompts with the following learner model answers, skips trailing prompts without learner answers, and preserves resolved primary/secondary language codes.
- [x] Web roleplay page renders model answers, static feedback, and no-AI behavior.
  - Evidence 2026-06-18: `DialogueRouteStructuralTests` verifies the roleplay view renders `ModelAnswerText`, model answer helper meanings, `StaticFeedback`, German TTS controls, no form/textarea/submitted-answer surface, and no OpenAI/ChatGPT/AI feedback hooks. Public smoke for `/dialogues/a1-cafe-order/roleplay` returned 200 with scripted practice and model-answer feedback text.
- [x] Empty or unknown dialogue detail payloads return safe 404 behavior instead of Web 500 errors.
  - Evidence 2026-06-18: `DialoguesControllerDualMeaningLanguageTests` verifies `Detail` and `Roleplay` return `NotFoundResult` when the catalog API returns no dialogue. Public smoke for `/dialogues/unknown-dialogue-for-smoke` returned 404.
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

- [x] Starter import rejects invalid filters, linked dialogue slugs, and invalid dual-language payloads.
  - Evidence 2026-06-18: `ImportAsync_ShouldFail_WhenConversationStarterContractIsInvalid` verifies bad starter slug/filter fields, unknown topic references, malformed `linkedDialogueSlugs`, duplicate translation languages, and unsupported translation languages are rejected before persistence.
- [x] Starter list query filters by CEFR level, situation, tone, and conversation goal.
  - Evidence 2026-06-18: `ImportAsync_ShouldPersistConversationStarterPacks` now runs against temporary PostgreSQL and verifies a valid starter pack is queryable through `GetPublishedStarterPacksAsync` with CEFR, situation, tone, conversation goal, and topic filters.
- [x] Starter detail query renders primary and optional secondary meaning languages.
  - Evidence 2026-06-18: the same PostgreSQL import/query test verifies detail projection resolves Persian primary meanings and English secondary meanings for starter phrases, including alternatives.
- [x] Dialogue detail query returns related starter packs for linked dialogues.
  - Evidence 2026-06-18: the PostgreSQL import/query test verifies `GetPublishedStarterPacksForDialogueAsync("doctor-appointment-a1")` returns the linked starter pack.
- [x] Web starter detail page renders phrase alternatives, usage notes, register, common mistakes, and dual-language text.
  - Evidence 2026-06-18: `ConversationStarterRouteStructuralTests` verifies Web/API starter routes, list filters, detail phrase alternatives, usage notes, register, common mistakes, primary/secondary meaning rendering, RTL-aware `dir` binding, and linked-dialogue rendering. Imported `conversation-starters-a1-foundation-01-v1.json` to shared PostgreSQL with `ConversationStarterPacks=3`, `ConversationStarterPhrases=12`, and `ConversationStarterPhraseTranslations=120`; public smoke passed for `/conversation-starters`, `/conversation-starters/a1-im-cafe-ein-gespraech-beginnen`, Persian detail, API list, and API detail with `primaryMeaningLanguageCode=fa&secondaryMeaningLanguageCode=en`.

### Event Preparation Packs

- [x] Preparation-pack import rejects invalid linked dialogues, vocabulary references, starter links, and prompt payloads.
  - Evidence 2026-06-18: `ImportAsync_ShouldFail_WhenEventPreparationContractIsInvalid` verifies bad pack slug/metadata, unknown topics, malformed linked starter slugs, invalid vocabulary word/part-of-speech/CEFR, and empty opening/roleplay/review prompt entries are rejected before persistence.
- [x] Preparation-pack persistence stores prompts, vocabulary references, topic links, dialogue links, and starter-pack links.
  - Evidence 2026-06-18: `ImportAsync_ShouldPersistEventPreparationPacks` now runs against temporary PostgreSQL and verifies persisted topics, linked dialogues, linked starter packs, linked vocabulary, and opening/roleplay/review prompts. Imported `event-preparation-a1-foundation-01-v1.json` to shared PostgreSQL with `EventPreparationPacks=3`, `EventPreparationPrompts=27`, `EventPreparationVocabularyReferences=9`, `EventPreparationLinkedDialogues=4`, and `EventPreparationLinkedConversationStarterPacks=3`.
- [x] Event-to-preparation-pack mapping returns only published packs linked to the active event.
  - Evidence 2026-06-18: `EventPreparationRepository` filters `PublicationStatus.Active`, `ConversationEventRepositoryTests` verifies event detail projection includes linked preparation pack slugs, and `EventPreparationRouteStructuralTests` verifies event detail renders `Model.PreparationPacks` as preparation links.
- [x] Web event detail renders entitled preparation-pack links.
  - Evidence 2026-06-18: `EventPreparationRouteStructuralTests` verifies `ConversationEvents/Detail.cshtml` renders `Model.PreparationPacks` with links to `EventPreparationPacks.Detail`. Public smoke for `/conversation-events` returned 200.
- [x] Web preparation-pack detail renders prompts, vocabulary, related dialogues, and related starter packs.
  - Evidence 2026-06-18: `EventPreparationRouteStructuralTests` verifies the detail page renders `PreparationPack.Prompts`, `PreparationPack.LinkedVocabulary`, related dialogue links, related starter-pack links, completion banner state, and the Mark prepared form. Local premium API smoke with the dev admin API key and `actorEmail=shahramvafadar@gmail.com` passed for `/api/catalog/event-preparation-packs`, `/api/catalog/event-preparation-packs/a1-sprachcafe-erster-besuch`, and `/api/catalog/dialogues/a1-cafe-order/event-preparation-packs`.
- [x] `Mark prepared` records aggregate completion analytics and redirects back to the preparation-pack detail page.
  - Evidence 2026-06-18: `EventPreparationPacksControllerFeatureGateTests.Complete_ShouldRecordCompletionAndRedirectToDetail` verifies completion analytics, `TempData["PreparationPackCompleted"]`, and redirect back to `Detail`.
- [x] Direct preparation-pack access is forbidden when the learner lacks the entitlement.
  - Evidence 2026-06-18: `EventPreparationPacksControllerFeatureGateTests` verifies `Detail` and `Complete` return `ForbidResult` without the entitlement and record premium-feature-denied analytics. Anonymous public API smoke for event preparation pack list/detail returns `401`, while anonymous Web detail redirects/renders the sign-in flow; entitled API smoke with the premium actor returns `200`.

### Event Directory And Organizer Tools

- [x] Event filtering covers city, online/offline, CEFR level, helper language, price type, date, and publication status.
  - Evidence 2026-06-18: `ConversationEventRepositoryTests` runs against PostgreSQL and verifies city, online/offline, CEFR level, helper language, price type, category, date-range filtering through structured `StartsAtUtc`, and `PublicationStatus.Active` filtering. Web/WebApi now expose `dateFromUtc`/`dateToUtc`, while legacy schedule-text-only events remain visible when no date filter is applied.
- [x] Event detail respects public/private projection rules.
  - Evidence 2026-06-18: PostgreSQL `ConversationEventRepositoryTests.GetPublishedEventBySlugAsync_ShouldReturnDetailWithPreparationLinks` verifies detail projection only returns active events and returns null for missing/unpublished slugs.
- [x] Organizer ownership rules restrict dashboard event management to assigned owners.
  - Evidence 2026-06-18: `OrganizerDashboardControllerTests` verifies profile edit is forbidden for non-owned organizer profiles and event edit returns `NotFound` when the event is not under the owned profile.
- [x] Organizer dashboard enforces active-event limits by organizer plan.
  - Evidence 2026-06-18: `OrganizerDashboardControllerTests` verifies a free organizer at its one-active-event limit is redirected with an explanatory error, while a lite organizer below the limit reaches the new-event edit view.
- [x] RSVP flow enforces capacity, duplicate registration rules, cancellation, and attendance updates.
  - Evidence 2026-06-18: PostgreSQL `EventRsvpServiceTests` verifies one RSVP per normalized participant email, status updates, capacity rejection for new/promoted `going` RSVP records, cancellation allowance, admin attendance updates, and draft/unknown event rejection. Waitlist behavior is not marked separately because no waitlist feature is currently enabled in the product model.
- [x] Organizer visibility rules hide archived/unpublished/private records from public pages.
  - Evidence 2026-06-18: PostgreSQL `OrganizerProfileRepositoryTests` verifies only active organizer profiles are listed publicly and active linked events are exposed on organizer detail. PostgreSQL `ConversationEventRepositoryTests` verifies draft events are hidden from public event list/detail.

### Learner Profiles And Partner Matching

- [x] Public learner profile projection excludes private contact details.
  - Evidence 2026-06-18: PostgreSQL `LearnerPartnerMatchingServiceTests` verifies public learner profile responses expose display name, city, practice preferences, language helpers, and goals, while omitting owner email and availability notes.
- [x] Learner profile anonymization clears public/private profile fields and disables matching.
  - Evidence 2026-06-18: PostgreSQL `LearnerPartnerMatchingServiceTests` verifies anonymization replaces the public name with a deleted marker, clears city and availability fields, disables visibility, removes adult confirmation, and hides the profile from public listings and partner search.
- [x] Partner request transitions cover pending, accepted, declined, cancelled, and blocked states.
  - Evidence 2026-06-18: PostgreSQL `LearnerPartnerMatchingServiceTests` covers pending request creation plus accepted, declined, cancelled, and blocked transitions, including creation of the backing `UserBlock` for blocked requests.
- [x] Partner request rate limits suppress excessive requests.
  - Evidence 2026-06-18: PostgreSQL `LearnerPartnerMatchingServiceTests` verifies the service-level daily request limit rejects the sixth new request inside the one-day window. The Web controller also keeps its existing short-window email rate limiter before forwarding requests.
- [x] Accepted partner requests reveal contact details only after mutual consent.
  - Evidence 2026-06-18: PostgreSQL `LearnerPartnerMatchingServiceTests` verifies pending, declined, cancelled, and blocked requests do not expose contact email, while an accepted request exposes the other participant's email to both sides.
- [x] Blocked users are hidden from matching results and cannot create new requests.
  - Evidence 2026-06-18: PostgreSQL `LearnerPartnerMatchingServiceTests` verifies existing `UserBlock` rows suppress both matching directions and reject new partner requests between the blocked pair.

### Moderation And Safety

- [x] User report creation validates target, reason, and reporter context.
  - Evidence 2026-06-18: PostgreSQL `ModerationServiceTests` verifies valid reports normalize reporter/reported emails and invalid target type, invalid reason, and invalid reporter context are rejected.
- [x] User block creation suppresses matching and contact surfaces.
  - Evidence 2026-06-18: PostgreSQL `ModerationServiceTests` verifies block creation by learner profile and source partner request, duplicate block idempotency, and suppression from partner matching. PostgreSQL `LearnerPartnerMatchingServiceTests` verifies blocked pairs cannot create new partner requests and accepted contact details are not exposed for blocked/declined/cancelled requests.
- [x] Admin moderation queue filters by status, reason, target type, and assigned state.
  - Evidence 2026-06-18: `ModerationServiceTests` verifies filtering by status, reason, target type, `assigned`, and `unassigned`. The Web admin moderation queue now exposes these filters in `/admin/moderation`; `assigned` maps to reports with a recorded decision owner because the product does not have a separate assignment workflow yet.
- [x] Moderation decision audit captures decision type, actor, target, notes, and timestamps.
  - Evidence 2026-06-18: PostgreSQL `ModerationServiceTests` verifies decisions update the report status, decision note, decision actor, decision timestamp, and create a `ModerationDecisionAudit` tied to the report id.
- [x] Report/block visibility rules are enforced in public pages and matching APIs.
  - Evidence 2026-06-18: Web `ModerationController` validates allowed report/block target shapes before calling the API, and PostgreSQL `ModerationServiceTests`/`LearnerPartnerMatchingServiceTests` verify block effects are enforced by the matching API.

### Admin Reporting And Seed Coverage

- [x] Admin system report endpoint returns catalog, social-learning, moderation, operations, and Learning Portal counts from the server database.
- [x] Web Admin Reports page combines system report counts, Identity user count, Web analytics counters, and Learning Portal quality/coverage tables.
- [x] Admin Reports authorization requires the Admin policy.
- [x] Learning Portal report test covers content counts, CEFR coverage, unresolved content links, missing translations, and missing exercise coverage.
- [x] Admin dashboard links to Reports and the implemented management pages.
  - Evidence 2026-06-18: `AdminDashboardRouteStructuralTests` verifies `/admin` links to Reports, Users/entitlements, Moderation, Billing Diagnostics, catalog management, Learning Portal management pages, organizer/events management, and operations/diagnostics pages. The dashboard remains protected by the `Operator` policy.
- [x] Web seed fixtures include multiple records for dialogues, starters, preparation packs, organizers, events, RSVPs, claims, learner profiles, partner requests, reports, blocks, and moderation audits.
  - Evidence 2026-06-18: `tools/Web/WebReadinessSeedFixtureManifest.json` defines a multi-record Web readiness fixture with content references for dialogues, conversation starter packs, and event preparation packs plus organizer profiles, organizer owners, conversation events, RSVPs, organizer claims, learner profiles, partner requests, user reports, user blocks, and moderation audits. `WebReadinessSeedFixtureManifestTests` passed and validates minimum record counts, required endpoint payload fields, and cross-references for organizer/event/preparation links, RSVPs, owners, claims, partner requests, blocks, and moderation audit report keys.
- [x] Operational seed loading path is validated once a dedicated event-directory/safety seed applier exists.
  - Evidence 2026-06-18: `tools/Server/Initialize-LocalWebOperationalSeeds.ps1` now defaults to `tools/Web/WebReadinessSeedFixtureManifest.json`, reads the manifest under strict mode, preserves arrays for one-item JSON payloads, maps learner/profile/report/partner keys to runtime ids, applies organizer profiles/events/owners/claims/RSVPs/learner profiles/partner requests/reports/blocks/audits through the real WebApi endpoints, and skips already-decided moderation audits on repeat runs. Local execution against `http://localhost:5099` completed successfully; a repeat run produced `PartnerRequests: 0` and `UserReports: 0`, confirming duplicate-sensitive records are not recreated. `WebReadinessSeedFixtureManifestTests` passed 2/2.
- [x] Web rendering coverage exists for the Learning Portal coverage and issue-sample tables.
  - Evidence: `AdminReportsLearningPortalIssueDrilldownStructuralTests` verifies the Reports index renders Learning Portal coverage tables, empty states, row key/count cells, issue-sample filters, CSV export controls, and Area/Owner/Issue/Target issue columns. Filtered test run passed 3/3.

### Entitlements And Feature Gates

- [x] Free, Trial, and Premium entitlement states resolve expected feature flags.
  - Evidence 2026-06-18: `DarwinLinguaIdentityBootstrapperTests.EntitlementStatesResolveExpectedFeatureFlags` verifies Free users keep catalog/search/learner-profile access without paid features, Trial users receive paid feature flags, and Premium users retain paid feature flags including advanced dialogue packs and partner matching.
- [x] Expired trial and expired premium states fall back to free access.
  - Evidence 2026-06-18: `DarwinLinguaIdentityBootstrapperTests.ExpiredTrialAndPremiumStatesFallBackToFreeAccess` verifies expired Trial and Premium states normalize to Free, retain only free catalog/search access, remove paid features, and record `trial-expired`/`premium-expired` audit events.
- [x] Web feature gates hide or block premium preparation packs and advanced features.
  - Evidence 2026-06-18: `EntitlementFeatureGateStructuralTests` verifies Web routes use feature-gate checks for event preparation packs, partner matching, dialogue preparation-pack access, and dual meaning-language resolution. Existing `EventPreparationPacksControllerFeatureGateTests`, `ScenariosControllerDualMeaningLanguageTests`, and `WordsControllerDualMeaningLanguageTests` cover forbidden/limited behavior at controller level.
- [x] WebApi feature gates reject direct unauthorized calls.
  - Evidence 2026-06-18: Event-preparation-pack API endpoints are now protected by the admin API middleware and additionally resolve `X-DarwinLingua-Actor-Email` through `IUserEntitlementService.HasFeatureAsync` before returning premium preparation-pack payloads. `EntitlementFeatureGateStructuralTests` verifies the API hardening, actor-email forwarding, and Web call sites; targeted WebApi tests for entitlement/event-preparation scenarios passed 7/7. Targeted WebApi and Web builds passed after stopping local hosts.
- [x] Admin entitlement changes write audit records and update effective access immediately.
  - Evidence 2026-06-18: `EntitlementFeatureGateStructuralTests` verifies Admin Users uses `userEntitlementService.SetTierAsync`, preserves the admin actor as `updatedBy`, loads recent entitlement audit events, and renders entitlement history. `DarwinLinguaIdentityBootstrapperTests.EntitlementStatesResolveExpectedFeatureFlags` verifies effective feature flags change immediately after tier updates.

### Billing And Payments

- [x] Billing page renders the current entitlement and disables checkout when Stripe is off.
  - Evidence 2026-06-18: `BillingSafetyStructuralTests` verifies the billing page renders `Model.Entitlement.Tier`, Stripe-disabled/config-incomplete states, `CanStartCheckout`, and `CanManageStripeSubscription`. `BillingController` now uses the same `IsStripeConfigured` guard for the view model and direct POST actions.
- [x] Billing page starts Stripe Checkout only for authenticated users and only when Stripe is enabled.
  - Evidence 2026-06-18: `BillingController` is `[Authorize]`, rejects disabled or incomplete Stripe configuration before provider calls, rate-limits checkout, and calls `CreatePremiumCheckoutSessionAsync(userId, email, ...)` only after those checks. `BillingSafetyStructuralTests` covers these guardrails.
- [x] Billing page shows Stripe Customer Portal management only after a customer id is linked to the account.
  - Evidence 2026-06-18: `BillingController` sets `CanManageStripeSubscription` only when Stripe is enabled and `billingProfile.ProviderCustomerId` exists; `Billing/Index.cshtml` renders `Manage in Stripe` only behind that flag. Covered by `BillingSafetyStructuralTests`.
- [x] Stripe Customer Portal session creation redirects only for the authenticated user's linked Stripe customer.
  - Evidence 2026-06-18: `OpenStripePortal` loads `WebBillingProfiles` by authenticated `userId`, requires that profile's `ProviderCustomerId`, applies the portal rate limit, and passes only that customer id to `CreateCustomerPortalSessionAsync`. Covered by `BillingSafetyStructuralTests`.
- [x] `/billing/success` fetches the returned Stripe checkout session and immediately syncs entitlement only when the session belongs to the authenticated user.
  - Evidence 2026-06-18: `BillingController.Success` calls `FulfillCheckoutSessionAsync(sessionId, GetUserId(), ...)`; `StripeCheckoutFulfillmentService` compares `metadata.darwin_user_id`/`client_reference_id` with the authenticated user before setting Premium. Covered by `BillingSafetyStructuralTests`.
- [x] `/billing/success` only fulfills completed subscription checkout sessions with paid or no-payment-required status and customer/subscription ids.
  - Evidence 2026-06-18: `StripeCheckoutFulfillmentService` requires `mode=subscription`, `status=complete`, `payment_status` paid or no-payment-required, and non-empty customer/subscription ids before updating billing profile and Premium entitlement. Covered by `BillingSafetyStructuralTests`.
- [x] `/billing/success` falls back to webhook-based activation when Stripe lookup fails or session is not complete.
  - Evidence 2026-06-18: `BillingController.Success` catches Stripe lookup/fulfillment failures and also handles non-fulfilled sessions with a user-facing "will update as soon as Stripe confirms" message instead of granting access. Covered structurally by `BillingSafetyStructuralTests` and controller source review.
- [x] `checkout.session.completed` webhook rejects non-subscription, unpaid, or incomplete checkout sessions before granting Premium.
  - Evidence 2026-06-18: `StripeWebhookSafetyTests` verifies the webhook handler requires completed paid/no-payment-required subscription checkout sessions with customer and subscription ids before Premium is granted.
- [x] Stripe Checkout, Stripe Customer Portal, and Admin reconciliation enforce bounded rate limits with safe user-facing messages.
  - Evidence 2026-06-18: `BillingSafetyStructuralTests` verifies bounded checkout, customer-portal, and admin reconciliation rate limits plus safe user-facing redirect messages.
- [x] Stripe Checkout session creation sends the Darwin Lingua user id in session and subscription metadata.
  - Evidence 2026-06-18: `StripeBillingCheckoutService` sends `client_reference_id`, `metadata[darwin_user_id]`, and `subscription_data[metadata][darwin_user_id]` with the authenticated user id. Covered by `BillingSafetyStructuralTests`.
- [x] Stripe Checkout failures show a safe user-facing message and do not leak provider response bodies.
  - Evidence 2026-06-18: `StripeBillingCheckoutService` logs only Stripe status/reason phrase on provider failure and throws a generic error; `BillingController.StartStripeCheckout` catches failures and shows "Checkout could not be started. Please try again later." Covered by `BillingSafetyStructuralTests` and targeted Web build.
- [x] Stripe webhook rejects missing, malformed, expired, or invalid signatures.
  - Evidence 2026-06-18: `StripeWebhookSafetyTests.StripeWebhookVerifier_ShouldAcceptOnlyCurrentValidV1Signatures` verifies valid HMAC-SHA256 v1 Stripe signatures and rejects missing, malformed, tampered, and expired signatures. Controller structural coverage verifies invalid signatures return `Unauthorized` before handler execution.
- [x] Duplicate Stripe webhook event ids are idempotent.
  - Evidence 2026-06-18: `StripeWebhookSafetyTests` verifies `StripeBillingWebhookHandler` returns immediately when a stored `WebBillingEvent` with the same provider event id is already `Processed`; the database has a unique provider-event index.
- [x] `checkout.session.completed` maps the Stripe session to the correct user and grants Premium.
  - Evidence 2026-06-18: `StripeWebhookSafetyTests` verifies checkout-completed handling uses `darwin_user_id`/client reference, persists customer/subscription ids, and sets `DarwinLinguaEntitlementTiers.Premium`.
- [x] `customer.subscription.created` and `customer.subscription.updated` persist customer/subscription ids and current period end.
  - Evidence 2026-06-18: `StripeWebhookSafetyTests` verifies subscription created/updated/deleted events are routed through subscription handling and persist billing profile status/customer/subscription/current-period data.
- [x] Active or trialing Stripe subscription states keep Premium access.
  - Evidence 2026-06-18: `StripeWebhookSafetyTests` verifies entitled subscription status handling maps active/trialing states to Premium.
- [x] Cancelled, unpaid, or incomplete-expired Stripe subscription states downgrade the user to Free.
  - Evidence 2026-06-18: `StripeWebhookSafetyTests` verifies non-entitled subscription status handling maps cancelled, unpaid, and incomplete-expired states to Free.
- [x] Unmapped Stripe subscription events fail closed and are visible in billing-event diagnostics/logs.
  - Evidence 2026-06-18: `StripeWebhookSafetyTests` verifies unmapped subscription events throw, mark the billing event as `Failed`, summarize the error, and log a warning; admin diagnostics query persisted billing events for operator review.
- [x] Admin Billing Diagnostics renders Stripe readiness without exposing secret values.
  - Evidence 2026-06-18: `BillingSafetyStructuralTests.AdminBillingDiagnostics_ShouldExposeReadinessFiltersAndSafeReconciliation` verifies readiness exposes configured/missing booleans for Stripe secret and webhook secret, renders configuration warnings, and does not bind raw secret-value properties into the view.
- [x] Admin Billing Diagnostics filters billing events by status, event type, user id, customer id, and subscription id.
  - Evidence 2026-06-18: `BillingSafetyStructuralTests` verifies Admin Billing Diagnostics accepts and normalizes those filter inputs, applies them to `WebBillingEvents`, bounds `take`, and renders the event filter controls.
- [x] Admin Billing Diagnostics filters billing profiles by user id, customer id, and subscription id.
  - Evidence 2026-06-18: `BillingSafetyStructuralTests` verifies the same normalized user/customer/subscription filters are applied to `WebBillingProfiles` and rendered in the admin diagnostics view.
- [x] Admin-only Stripe subscription reconciliation rejects malformed subscription ids.
  - Evidence 2026-06-18: `BillingSafetyStructuralTests` verifies reconciliation is Admin-only and rejects values that fail `IsAllowedStripeSubscriptionId` before calling Stripe.
- [x] Admin-only Stripe subscription reconciliation fetches current Stripe subscription state without storing raw provider payloads.
  - Evidence 2026-06-18: `BillingSafetyStructuralTests` verifies reconciliation fetches `/v1/subscriptions/{sub_...}`, parses the provider response into bounded local fields, and updates local billing state without a raw payload storage model.
- [x] Admin-only Stripe subscription reconciliation updates billing profile and entitlement for active, trialing, past-due, cancelled, unpaid, and expired states.
  - Evidence 2026-06-18: `BillingSafetyStructuralTests` verifies reconciliation maps active, trialing, and past-due statuses to Premium, all other non-entitled statuses to Free, upserts the billing profile, updates entitlement, and sends user/admin reconciliation notifications.
- [x] Billing notification emails render for Premium activation, payment action needed, Premium ended, and Admin reconciliation completed.
  - Evidence 2026-06-18: `BillingSafetyStructuralTests.BillingNotificationEmails_ShouldRenderLogAndDeduplicateAllBillingScenarios` verifies all four billing email scenarios exist, have transactional email templates, and are rendered through `IEmailTemplateRenderer`.
- [x] Billing notification emails are logged through the transactional email delivery log without storing Stripe raw payloads.
  - Evidence 2026-06-18: `BillingSafetyStructuralTests` verifies billing notification emails use suppression checks, queued/sent/failed delivery-log paths, and bounded billing-status/subscription metadata instead of storing provider raw payloads.
- [x] Billing notification emails are idempotent per scenario, user, subscription, and billing status.
  - Evidence 2026-06-18: `BillingSafetyStructuralTests` verifies billing notification registration uses `WebBillingNotifications`, unique `NotificationKey`, and keys built from scenario, user, subscription, and billing status.
- [x] Stripe webhook processing sends user billing emails for checkout completion, payment-action-needed states, and Premium-ended states.
  - Evidence 2026-06-18: `BillingSafetyStructuralTests` verifies `StripeBillingWebhookHandler` triggers Premium activated, payment-action-needed, and Premium ended notification paths.
- [x] Admin reconciliation sends both user billing status email and admin reconciliation-completed email when recipients are configured.
  - Evidence 2026-06-18: `BillingSafetyStructuralTests` verifies reconciliation triggers user Premium activated/ended messages and `SendAdminReconciliationCompletedAsync` for configured admin recipients.

### Web Runtime And Bootstrap

- [x] PostgreSQL startup bootstrap retrofits Phase 6 catalog tables on an existing shared database.
  - Evidence 2026-06-18: `WebRuntimeBootstrapStructuralTests` verifies `ServerContentDatabaseBootstrapper` runs `EnsureCreatedAsync`, creates missing server-content base tables, applies published-package compatibility columns/tables with `ColumnExistsAsync`/`TableExistsAsync`, and requires the PostgreSQL Npgsql provider.
- [x] Empty optional Identity and catalog connection strings fall back to the shared server-content database.
  - Evidence 2026-06-18: `WebRuntimeBootstrapStructuralTests` verifies WebApi falls back SharedCatalog/Identity to ServerContent and Web falls SharedCatalog to ServerContent/WebIdentity, all through Npgsql configuration.
- [x] WebCatalogApiClient treats empty successful detail responses as `null`.
  - Evidence 2026-06-18: `WebRuntimeBootstrapStructuralTests` verifies optional `GetAsync<T>` returns default for 404 and empty successful response bodies while required calls still throw on empty payloads.
- [x] Local server bootstrap handles both folder imports and single-file imports under `Set-StrictMode`.
  - Evidence 2026-06-18: `WebRuntimeBootstrapStructuralTests` verifies `tools/Server/Initialize-LocalServerContent.ps1` runs under `Set-StrictMode`, supports leaf `.json` files and recursive directory imports, and loops over the normalized content file list. It also verifies operational seed bootstrap runs under `Set-StrictMode` and reads seed JSON safely.

## Learning Portal Expansion

### Foundation

- [x] Web learner navigation groups implemented routes under Learn, Practice, Speak, Prepare, and Resources.
- [x] Web learner navigation avoids dead routes for future Phase 7 modules.
- [x] Phase 7 Web learner routes are covered by structural route tests for Grammar, Expressions, Exercises, Courses, Exam Prep, Writing Templates, Cultural Notes, and Unified Search.
- [x] Phase 7 WebApi route registrations are covered by structural route tests for module list/detail endpoints, exercise attempts, unified search, progress summary/update, and recommendations.
- [x] Phase 7 English/German localization resource keys are covered by structural tests for the release route surface.
- [x] Shared CEFR filter conventions expose stable A1-C2 values for reusable Web filters.
- [x] Cross-content linking helper coverage once a real Phase 7 module consumes the model.
  - Evidence 2026-06-18: Web linked-content rendering now uses shared `LearningContentLinkResolver` instead of raw slug output for Course, Writing Templates, Exam Prep, and Life in Germany linked-content sections. `CourseActivityTargetLinkResolver` delegates to the shared resolver, and `/courses/lessons/{lessonSlug}` redirects course-lesson links from non-Course pages to the canonical course path. Structural tests cover supported content-type mappings, fail-closed unsupported targets, clickable linked-content partials, and Course/Writing/Exam/Life in Germany view wiring. Targeted WebApi structural tests passed (`33/33`), and targeted Web/WebApi builds succeeded.
- [x] Admin/system report coverage exists for persisted Phase 7 module counts and quality metrics.

### Grammar Guide

- [x] Parser coverage exists for the GrammarTopic content contract shape.
- [x] Parser/import coverage exists for the rich localized Grammar pilot package, including localized title/description and table blocks.
- [x] Parser/import coverage exists for the first official A1 Grammar package, including all 10 languages, 6 sections, 20 examples, and 10 common mistakes.
- [x] Navigation/localization smoke coverage includes the live Grammar Guide route.
- [x] Release route hardening covers `/grammar` and `/api/catalog/grammar`.
- [x] Import validation covers required rich section keys, supported block types, localized block language codes, and table/callout block shape.
- [x] List/detail query coverage includes the first rich pilot topic and localized fallback behavior.
- [x] CEFR/category/topic/search filters return expected grammar topics.
  - Evidence 2026-06-18: PostgreSQL `GrammarTopicRepositoryTests.GetPublishedGrammarTopicsAsync_ShouldFilterByCefrCategoryTopicAndSearch` verifies CEFR, grammar category, topic-key, case-insensitive search through `ILike`, and draft-topic exclusion using real Npgsql queries.
- [x] Web/API rendering handles paragraph, table, and callout rich blocks from the official A1 personal-pronouns package.
- [x] Add a future reviewed cross-level validation batch after the official A1 core package establishes the production content pattern.
  - Evidence 2026-06-18: `ContentImportParserGrammarTopicTests.ParseAsync_ShouldValidateAllOfficialA1C2GrammarPackagesAgainstSyllabus` now validates all six official Grammar packages (`A1-C2`, 225 topics) against the syllabus, checks localized section coverage, rejects unknown rich block types, verifies table row/column shape, and verifies real official usage of `paragraph`, `table`, `callout`, `rule-list`, and `example-list`. `ContentImportServiceApplicationTests.ImportAsync_ShouldAcceptEverySupportedGrammarRichBlockType` and `ImportAsync_ShouldRejectGrammarRichBlockTypes_WhenRequiredShapeIsMissing` cover application validation for all seven contract block types, including `mistake-pair` and `image-slot`. Targeted ContentOps Application tests passed `8/8`; targeted ContentOps Infrastructure parser validation passed `1/1`.
- [x] Linked words/dialogues/Talk Topics/exercises render where available.
  - Evidence 2026-06-18: `GrammarRouteStructuralTests` verifies Grammar detail renders linked words, dialogues, Talk Topics, and exercises through their Web routes. PostgreSQL `GrammarTopicRepositoryTests.GetPublishedGrammarTopicBySlugAsync_ShouldProjectLocalizedTextAndLinksSafely` verifies detail projection returns linked word slugs, dialogue slugs, Talk Topic slugs, and exercise slugs.
- [x] Localized explanation rendering follows learner language preferences.
  - Evidence 2026-06-18: PostgreSQL `GrammarTopicRepositoryTests.GetPublishedGrammarTopicBySlugAsync_ShouldProjectLocalizedTextAndLinksSafely` verifies Persian learner-language projection for section heading/explanation, examples, mini rules, common mistakes, and exception notes. `GrammarRouteStructuralTests` verifies the Web controller requests `profile.PreferredMeaningLanguage1` and the detail view applies direction-aware rendering.
- [x] Unresolved links fail safely without Web 500 errors.
  - Evidence 2026-06-18: PostgreSQL `GrammarTopicRepositoryTests.GetPublishedGrammarTopicBySlugAsync_ShouldProjectLocalizedTextAndLinksSafely` verifies unresolved linked words can project with `WordSlug=null`. `GrammarRouteStructuralTests` verifies the Web detail page renders unresolved linked words as plain text instead of a broken word-detail route.

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
- [x] Add authenticated WebApi/profile-bound eligibility tests for Sensitive Educational Language preference instead of relying on explicit query flags.
  - Evidence: `SensitiveEducationalLanguageStructuralTests` verifies Web expressions read `AllowsRudeSlangContent` from the learner profile, public WebApi requests cannot unlock sensitive educational expressions with a bare query flag, and the Web internal API client supplies the configured admin API header for server-side catalog calls.
- [ ] Add mobile export tests proving Sensitive Educational Language and adult-only Expressions are excluded until mobile eligibility enforcement exists.
- [x] Add settings UI smoke coverage for the English/German Sensitive Educational Language checkbox copy.
  - Evidence: `SensitiveEducationalLanguageStructuralTests` verifies the Settings checkbox binding plus English/German resource copy for Sensitive Educational Language, explicit adult-content exclusion, warning guidance, and the Expressions filter clarification.
- [x] Add Web detail rendering coverage for `usagePolicy: understand-only` and `usagePolicy: do-not-use`.
  - Evidence: `SensitiveEducationalLanguageStructuralTests` verifies the expression detail view branches and localized English/German copy for both usage policies.
- [x] Live Web/API smoke for the imported pilot package remains required before bulk Expressions generation.
  - Evidence: local target/dev services backed by `darwinlingua_shared` returned Expression list/detail/search/admin report data after the pilot import. Public `linguaapi.vafadar.pro`/`lingua.vafadar.pro` returned 502 during this pass and should be checked separately as an environment/reverse-proxy issue.

### Exercise Engine

- [x] Parser coverage exists for the Exercise and ExerciseSet content contract shape.
- [x] Release route hardening covers `/exercises`, exercise-set/detail endpoints, and exercise attempt submission route registration.
- [x] Exercise type validation rejects unsupported types.
- [x] Answer key validation rejects missing or malformed deterministic answers.
- [x] Deterministic feedback returns stable correct/incorrect outcomes.
- [x] Exercise set linking resolves valid owner references.
  - Evidence: `ContentImportServiceApplicationTests` rejects malformed ExerciseSet `ownerSlug` values before import, and `WebsiteAdminQueryServiceLearningPortalReportTests` reports unresolved ExerciseSet owner targets with drill-down rows such as `course-lesson:missing-owner-lesson`. WebApi/Web builds passed after adding the admin response counter and Web report metric.
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
- [x] Backfill `activityBlocks` for A1 Module 1 in a small cumulative package and smoke one activity-enabled lesson page.
  - Evidence: the current Course packages contain activity blocks for every imported lesson, including A1 Module 1 (`10/10` lessons with 5 activities each). Package verification reports A1 `60/60`, A2 `80/80`, B1 `100/100`, B2 `80/80`, C1 `120/120`, and C2 `120/120` activity-enabled lessons; docs `76`/`80` record the imported Course activity-flow checkpoint and external backup.
- [x] Linked content rendering covers grammar, words, expressions, dialogues, Talk Topics, and exercises.
  - Evidence: `_SlugList` now resolves Course fallback linked-content slugs to learner-facing links for Grammar, Words, Expressions, Dialogues, Talk Topics, Exercise sets, and Exercises; `CourseRouteStructuralTests` verifies the mappings and that raw fallback slugs are not rendered as inert code-only rows.
- [x] Prerequisite and next-lesson navigation resolves for the imported A1-C2 Course baseline.
- [x] WebApi list/detail endpoint coverage exists.
  - Evidence: `CourseRouteStructuralTests` verifies WebApi registrations for `/api/catalog/courses`, `/api/catalog/courses/{slug}`, and `/api/catalog/course-lessons/{slug}` plus Web client calls with `primaryMeaningLanguageCode`; PostgreSQL repository tests cover localized Course list/detail/lesson projection.
- [x] Progress tracking works where implemented.
  - Evidence: `LearningProgressRouteStructuralTests` verifies authenticated progress/recommendation endpoint authorization, course lesson viewed/progress updates, antiforgery-protected manual controls, and recent/recommendation rendering.
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
- [x] Variable rendering substitutes supported placeholders safely in an interactive editor flow.
  - Evidence: `WritingTemplateRouteStructuralTests` verifies the learner detail view renders a client-side variable editor, preview output, reset control, English/German copy, and JavaScript replacement logic that keeps empty placeholders visible and writes preview text via `textContent` rather than `innerHTML`.

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

- [x] Application-level empty query handling rejects invalid API requests before repository calls.
- [x] Application-level short, long, and unsupported result-type query handling is covered.
  - Evidence: 2026-06-18 `UnifiedLearningSearchServiceTests` covers empty, one-character, over-100-character, unsupported result type, and normalized valid search behavior; Web structural coverage verifies one-character learning-content queries are not sent from `/search`.
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

- [x] Domain tests cover supported owner types and progress state transitions, including clearing stale completion timestamps when a completed item moves back to review.
- [x] Application tests cover viewed/completed updates, summary counts, and deterministic recommendation exclusion for completed content.
- [x] Release route hardening covers progress summary/update and recommendations route registrations.
- [x] WebApi endpoint coverage exists for authenticated `/api/learning/progress/summary`.
- [x] WebApi endpoint coverage exists for authenticated `/api/learning/progress/content`.
- [x] WebApi endpoint coverage exists for `/api/learning/recommendations`.
- [x] Anonymous Web users fall back to existing guest actor behavior without breaking recent activity.
- [x] Course lesson pages render viewed/completed state where progress exists and expose antiforgery-protected manual controls for `in-progress`, `completed`, and `needs-review`.
- [x] Recent activity dashboard renders cross-content progress summary.
- [x] Recommendations remain deterministic and do not use AI ranking.
  - Evidence: Learning progress structural tests cover authenticated endpoint registrations, `GetRequiredUserId`, course progress chips, course lesson manual progress update controls, recent progress summary, weak-exercise recommendations, difficult-word recommendations, and deterministic recommendation reader signals.

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
- [x] Add seeded module-slice package tests that import one selected module without removing unrelated local modules.
  - Evidence 2026-06-18: `ContentImportRepository` now upserts existing `CoursePath` metadata and replaces only imported `CourseModule` rows plus their child lessons, instead of deleting the whole path. PostgreSQL integration test `ContentImportServiceTests.ImportAsync_ShouldReplaceSelectedCourseModuleWithoutRemovingUnrelatedModules` imports a two-module Course package, then imports a one-module slice for the same path and verifies the touched module/lesson are updated while the unrelated module/lesson remain. Targeted Course test runs passed for ContentOps Infrastructure (`2/2`) and ContentOps Application (`10/10`). `docs/80-Course-Content-Package-Contract.md` now documents module-slice-safe import semantics and the absence of implicit full-delete without a future explicit full-replace mode.
- [ ] Add first-run onboarding UI automation for choose/skip flows.
- [ ] Add seeded mobile package export tests that import a package with all Phase 7 module types into a local SQLite database.
- [ ] Add MAUI smoke coverage for opening Learning Portal list/detail/search pages on target devices.
- [x] Add manual mobile validation worksheet entries for Phase 7 offline behavior and local package update behavior.
  - Evidence: `artifacts/validation/phase7-mobile-validation-worksheet.md` covers device matrix, first-run module selection, module-scoped package updates, offline behavior, Phase 7 content surfaces, and progress/account boundaries.

## Manual Validation Backlog

### Browser And Device Matrix

- [x] Validate English UI in desktop Chromium.
  - Evidence 2026-06-18: in-app desktop Chromium smoke covered `/`, `/courses`, `/exercises`, `/exam-prep`, `/writing-templates`, `/life-in-germany`, and `/search` with `?culture=en&ui-culture=en`. Each page returned `html lang="en"`, expected English title/H1/navigation labels, no server error text, no raw localizer markup, and zero captured console errors.
- [x] Validate German UI in desktop Chromium.
  - Evidence 2026-06-18: in-app desktop Chromium smoke covered the same routes with `?culture=de&ui-culture=de`. This found and fixed an OutputCache culture-vary bug on `/courses`; after the fix every page returned `html lang="de"`, expected German title/H1/navigation labels, no server error text, no raw localizer markup, and zero captured console errors.
- [x] Validate responsive layout on narrow mobile viewport.
  - Evidence 2026-06-18: in-app browser viewport override at 390x844 covered `/`, `/courses`, `/exercises`, `/exam-prep`, `/writing-templates`, `/life-in-germany`, and `/search`. Each page had `scrollWidth == clientWidth`, no horizontal overflow, no server error text, no raw localizer markup, and zero captured console errors. The viewport override was reset after validation.
- [ ] Validate PWA install flow on Android Chrome.
- [ ] Validate PWA install flow on desktop Chromium.
  - Partial evidence 2026-06-18: `tools/Web/New-WebPwaInstallabilityReport.ps1` generated `artifacts/installability-report.json` with 17 passed automated desktop Chromium checks for HTTPS reachability, manifest completeness, standalone display metadata, PNG and maskable icons, service-worker readiness/control, shell cache creation, cached `offline.html`, and direct offline-shell route health. Real browser install prompt acceptance and installed-window behavior remain manual and keep this item open.
- [x] Validate offline behavior for Web/PWA shell.
  - Evidence 2026-06-18: `WebPwaInstallStructuralTests` passed 2/2 for manifest/install wiring, service-worker registration script, and offline fallback shape. A desktop Chromium Puppeteer smoke loaded `https://localhost:7501/`, waited for `navigator.serviceWorker.ready`, verified cache `darwin-lingua-shell-v3`, stopped the Web host, then navigated to `/offline-smoke-check`; the service worker returned cached `offline.html` with title `Offline - Darwin Lingua`. Full install prompt acceptance remains covered by the separate Android/Desktop install-flow items.

### Learner Workflows

- [x] Browse by CEFR.
  - Evidence 2026-06-18: in-app desktop Chromium opened `/browse`, verified the six CEFR chips, then opened `/browse/cefr/a1?culture=en&ui-culture=en`. The page rendered `CEFR A1`, returned word detail links such as `/words/ab`, had no empty state, no server error text, and zero captured console errors.
- [x] Browse by topic.
  - Evidence 2026-06-18: in-app desktop Chromium read topic chips from `/browse`, opened `/browse/topic/everyday-life?culture=en&ui-culture=en`, rendered `Topic: Everyday Life`, returned word detail links such as `/words/ab`, had no empty state, no server error text, and zero captured console errors.
- [x] Search and open word detail.
  - Evidence 2026-06-18: in-app desktop Chromium opened `/search?q=Termin&culture=en&ui-culture=en`, rendered word results including `/words/termingerecht`, `/words/der-arzttermin`, and `/words/der-termin`, then opened `/words/termingerecht` successfully with no server error text and zero captured console errors.
- [x] Favorite/unfavorite word.
  - Evidence 2026-06-18: `WordsControllerDualMeaningLanguageTests` verifies the word detail favorite toggle posts to the safe local return URL when favorites are available and redirects to the Favorites page when the premium-gated feature is locked; the same tests keep dual meaning-language routing intact.
- [x] Recent activity.
  - Evidence 2026-06-18: in-app desktop Chromium opened `/recent?culture=en&ui-culture=en`, verified the Recent Activity page, learning progress summary, deterministic recommended next course lessons, a recent word entry after opening search/detail content, no server error text, and zero captured console errors.
- [x] Meaning-language preferences.
  - Evidence 2026-06-18: `SettingsControllerLanguagePreferenceTests` verifies Settings update persists the secondary meaning language only when the dual-meaning feature is entitled, omits it when the feature is unavailable, and the Settings UI explains where the secondary helper language appears. PostgreSQL verification for `shahramvafadar@gmail.com` reports `Premium` entitlement with `PrimaryMeaningLanguageCode=fa` and `SecondaryMeaningLanguageCode=en`.
- [x] Dialogue list/detail.
  - Evidence 2026-06-18: in-app desktop Chromium opened `/dialogues?culture=en&ui-culture=en`, found dialogue detail links including `/dialogues/a1-dialogue-appointment-001`, opened the detail page, verified title `Einen Termin verschieben (A1)`, dialogue content, useful phrase/word sections, a roleplay link, no server error text, and zero captured console errors.
- [x] Dialogue roleplay.
  - Evidence 2026-06-18: in-app desktop Chromium opened `/dialogues/a1-dialogue-appointment-001/roleplay?culture=en&ui-culture=en`, verified the scripted roleplay page with 4 partner/learner steps, German lines, English helper meanings, replay/hear-sentence controls, no prepared-empty state, no server error text, and zero captured console errors.
- [x] Conversation starter list/detail.
  - Evidence 2026-06-18: imported `conversation-starters-a1-foundation-01-v1.json` to shared PostgreSQL with 3 packs, 12 phrases, and 120 translations; public Web/API smoke passed for list, detail, Persian detail, API list, and API detail with Persian primary plus English secondary meanings.
- [x] Event directory list/detail.
  - Evidence 2026-06-18: in-app desktop Chromium opened `/conversation-events?culture=en&ui-culture=en`, found active event links including `/conversation-events/bamf-integration-course-information`, opened that detail page, verified the event title and RSVP form presence without submitting it, saw no empty state, no server error text, and zero captured console errors.
- [x] Event preparation pack detail and `Mark prepared`.
  - Evidence 2026-06-18: imported `event-preparation-a1-foundation-01-v1.json` to shared PostgreSQL with 3 packs, 27 prompts, 9 vocabulary references, 4 dialogue links, and 3 starter links. Targeted parser/persistence tests passed, anonymous public access remains gated, and local premium API smoke passed for list, detail, and dialogue-related preparation packs; controller tests cover Mark prepared completion analytics and redirect.

### Account And Admin Workflows

- [x] Register learner.
  - Evidence 2026-06-18: `WebRegistrationLegalAcknowledgementTests` passed 5/5 and now verifies the registration workflow creates a learner user, assigns the `Learner` role, initializes entitlement state, records versioned Terms/Privacy acceptances, sends confirmation email, and returns the same CheckEmail flow for existing users to avoid account enumeration. Public smoke for `/Identity/Account/Register` returned 200 with the register form, email autocomplete, Terms link, and Privacy link.
- [x] Sign in/out.
  - Evidence 2026-06-18: `WebAccountAuthenticationWorkflowTests` passed 2/2 and verifies login clears the external auth cookie, normalizes return URLs, checks confirmed-email state before password sign-in, enables lockout-on-failure, sends lockout notifications through the safe rate-limited path, routes not-allowed users to CheckEmail, and wires sign-out as a POST to the Identity default UI logout page. Public smoke for `/Identity/Account/Login` returned 200 with email/password autocomplete, remember-me, Register, and Forgot password controls.
- [x] Seeded admin login.
  - Evidence 2026-06-18: `DarwinLinguaIdentityBootstrapperTests` passed 7/7, covering required seed-account configuration, admin/learner creation, roles, entitlement defaults, and expiry behavior. Public seeded-login smoke used the configured local admin seed account with antiforgery token and session cookie; login redirected successfully and `/admin` returned 200 without a forbidden page.
- [x] Seeded learner login.
  - Evidence 2026-06-18: `DarwinLinguaIdentityBootstrapperTests` passed 7/7 for learner seed creation and role assignment. Public seeded-login smoke used the configured local learner seed account with antiforgery token and session cookie; login redirected successfully and `/account` returned 200 with authenticated account chrome and Sign out available.
- [x] Admin import/drafts/history/publish/rollback.
  - Evidence 2026-06-18: `AdminOperationsWorkflowStructuralTests` passed 3/3 and verifies the Admin imports, drafts, history, publishing, and rollback controllers/routes, Operator/Admin policies, safe status/query normalization, HTMX table/panel refresh surfaces, publishing summary, protected rollback warning modal, dashboard links, WebApi admin endpoints, and Web client/service wiring. Public seeded-admin smoke returned 200 for `/admin/imports`, `/admin/drafts`, `/admin/history`, `/admin/publishing`, and `/admin/rollback` without falling back to sign-in.
- [x] Admin user entitlement management.
  - Evidence 2026-06-18: `EntitlementFeatureGateStructuralTests` passed 3/3 and now verifies Admin-only user list/detail routes, entitlement update validation for supported tiers and UTC expirations, free-tier expiry rejection, future expiry enforcement for trial/premium, audited `updatedBy` resolution, recent entitlement history loading, enabled feature rendering, confirmation-protected update form, and WebApi `/api/admin/identity/users/{userId}/entitlement` management endpoint behavior for unsupported tiers and missing users. Public seeded-admin smoke signed in successfully, `/admin/users` returned 200 with the user table, and a user detail page returned 200 with entitlement controls and entitlement history.
- [x] Admin organizer/event management.
  - Evidence 2026-06-18: `AdminOrganizerEventManagementStructuralTests`, `OrganizerProfileAdminServiceTests`, and `ConversationEventAdminServiceTests` passed 7/7. Coverage verifies Operator-protected Admin organizer profile creation/replacement, owner assignment, claim decision flow, organizer plan/status/CEFR/language validation, notification call sites, Admin conversation event creation/replacement, UTC start/end parsing, recurrence/capacity fields, preparation-pack slug validation, Web client methods, and WebApi admin endpoints for organizer profiles, claim requests, owners, conversation events, publication status, and RSVPs. Public seeded-admin smoke returned 200 for `/admin/organizer-profiles` with create, claim, and owner sections and `/admin/conversation-events` with create and published-list sections.
- [x] Admin moderation queue and decision logging.
  - Evidence 2026-06-18: `AdminModerationWorkflowStructuralTests` and PostgreSQL `ModerationServiceTests` passed 5/5. Coverage verifies Operator-protected `/admin/moderation`, normalized status/reason/target/assigned filters, admin report queue loading, decision audit loading, allowed decision status validation, decision-note length cap, audited `DecidedBy` request creation, reporter outcome notification call site, Web client query parameters, and WebApi admin endpoints for reports, decisions, and audits. Public seeded-admin smoke returned 200 for `/admin/moderation` with filter, reports, and audit sections.
- [x] Admin reports summary.
  - Evidence 2026-06-18: `AdminReportsSummaryStructuralTests`, `AdminReportsLearningPortalIssueDrilldownStructuralTests`, `WebsiteAdminQueryServiceLearningPortalReportTests`, and `AdminReportsAuthorizationTests` passed 7/7. Coverage verifies Admin-only `/admin/reports`, system report API wiring, identity-user count, catalog/social/moderation/operations/email/Learning Portal metric sections, Web analytics counters, Learning Portal quality counters including course activity, exercise set, exam prep, and sensitive-policy gaps, plus issue filtering and CSV export. Public seeded-admin smoke returned 200 for `/admin/reports` with system report, Learning Portal coverage, issue samples, and analytics sections, and `/admin/reports/learning-portal-issues` returned 200 with the issue drilldown.

## Out Of Scope For This Web Test Backlog

- Mobile screen implementation.
- MAUI UI automation.
- Mobile offline catalog validation.
- Payment-provider integration.
- AI roleplay or AI feedback.

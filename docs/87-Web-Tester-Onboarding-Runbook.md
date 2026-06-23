# Web Tester Onboarding Runbook

## Purpose

This runbook prepares the current Web baseline for controlled real-user testing.

The goal is not to generate more content during testing. The goal is to find practical blockers, unclear learning flows, broken links, missing translations, confusing UI states, and content-quality issues before mobile work resumes.

## Current Test Baseline

- Public Web: `https://darwinlingua.com`
- Public API health: `https://api.darwinlingua.com/health`
- Current content counts: `CourseLessons=560`, `WritingTemplates=120`, `ExamPrepUnits=246`, `LifeInGermany/CulturalNotes=30`
- Course lesson activity flow is complete for A1-C2: `560/560` lessons have `activityBlocks`.
- Latest phase backup: `X:\Projects\DarwinLingua.Backup\20260623-164407-web-transactional-email-preview-ready-pre-user-testing`
- Latest readiness commits: recorded by the phase backup manifest and recoverable through `repo-overlay/darwinlingua-current-head.bundle`; recent changes cover operational incident ownership, Brevo/domain hardening, email-change public-link verification, branded transactional email HTML, and safe template preview generation.
- Latest tester bundle: `artifacts/validation/web-tester-runs/20260623-171610-web-tester-pass-brevo-ready`
- Latest public preflight: `artifacts/validation/web-tester-runs/20260623-171610-web-tester-pass-brevo-ready/preflight/web-tester-preflight-20260623-171614.json` with 25/25 checks passed.
- Latest feedback triage dry-run: `artifacts/validation/web-tester-feedback/web-tester-feedback-triage-20260623-171624.md` with 0 validation errors on the empty bundle feedback template.
- Mobile/MAUI is explicitly out of scope for this test pass.

## Tester Profile

Use a small controlled group first:

- 2-3 learners around A1/A2
- 2-3 learners around B1/B2
- 1-2 advanced learners around C1/C2
- at least one right-to-left helper-language user, preferably Persian or Arabic
- at least one user who mainly uses English helper text

Do not use this pass as a marketing launch. The tester group should know that the Web product is still being validated.

## Pre-Test Operator Checklist

Before inviting testers:

- Use Brevo-ready self-registration for this tester pass. Brevo setup, DNS/domain verification, webhook secret, DPA acceptance, public delivery smoke, app-level email link smoke, webhook/suppression smoke, and Admin Email Diagnostics smoke are complete for the controlled public Web stack. Testers may validate registration, email confirmation, password reset, and email-change behavior as part of the pass.
- Use pre-created accounts only for a specific tester who cannot self-register, and record that exception in the generated bundle notes.
- Grant Premium in batch only after the tester accounts already exist:

```powershell
Copy-Item .\tools\Web\WebTesterAccounts.example.csv .\artifacts\validation\web-tester-runs\<run-id>\WebTesterAccounts.csv
# Edit WebTesterAccounts.csv so the Email column contains real existing tester account emails.
.\tools\Web\Set-WebTesterPremiumAccess.ps1 -TesterCsvPath .\artifacts\validation\web-tester-runs\<run-id>\WebTesterAccounts.csv -UpdatedBy "first-web-tester-wave"
```

- The batch tool confirms the existing account email and grants Premium with an entitlement audit record. It does not create passwords or send email; keep account-password delivery out of public channels.
- Confirm the public test host is `https://darwinlingua.com`; do not use `www.darwinlingua.com` unless that host is intentionally configured.
- Confirm the Legal Notice/Impressum and Privacy Policy still carry the correct development-stage wording and that production operator data is not claimed before legal review.
- Confirm the two PWA manual install checks from `docs/56-Web-Pwa-Install-Validation-Worksheet.md` are either completed or explicitly left out of this tester pass.

- Create a timestamped tester bundle:

```powershell
.\tools\Web\New-WebTesterValidationBundle.ps1
```

- Use the generated folder under `artifacts/validation/web-tester-runs/` for that tester pass.
- Share `TesterQuickStart.md` from the generated bundle with testers; keep this runbook for operators.
- Run the repeatable preflight script from the repository root:

```powershell
.\tools\Web\Invoke-WebTesterPreflight.ps1
```

- Confirm the script exits successfully and writes a JSON report under `artifacts/validation/web-tester-preflight/`.
- Confirm the runtime/database bootstrap check is green:

```powershell
.\tools\Web\Invoke-WebOperationsBootstrapCheck.ps1
```

- Confirm it exits successfully and writes a report under `artifacts/validation/web-operations-bootstrap/`.
- For local operator smoke, also run authenticated admin checks:

```powershell
.\tools\Web\Invoke-WebAdminAuthenticatedSmoke.ps1 -UseLocalDevelopmentSeed
```

- For non-local environments, set `DARWINLINGUA_WEB_ADMIN_EMAIL` and `DARWINLINGUA_WEB_ADMIN_PASSWORD` instead of using the local seed switch.
- Confirm the admin smoke exits successfully and writes a JSON report under `artifacts/validation/web-admin-smoke/`.
- If Brevo account-email checks are in scope for this tester pass, run the more specific Email Diagnostics smoke too:

```powershell
.\tools\Web\Invoke-WebEmailDiagnosticsAdminSmoke.ps1 -UseLocalDevelopmentSeed
```

- Confirm it exits successfully and writes a report under `artifacts/validation/web-email-diagnostics-admin-smoke/`.
- If you used `New-WebTesterValidationBundle.ps1` without `-SkipPreflight`, the bundle already contains a pass-specific preflight report under its `preflight/` folder.
- The script verifies `https://darwinlingua.com` and `https://api.darwinlingua.com/health`.
- It also smokes these pages:
  - `/browse`
  - `/browse/cefr/A1`
  - `/browse/topic/everyday-life`
  - `/search?q=Termin`
  - `/words/das-haus`
  - `/favorites`
  - `/settings`
  - `/courses`
  - one A1 course lesson
  - one C2 course lesson
  - `/exercises`
  - `/exam-prep`
  - `/writing-templates`
  - `/life-in-germany`
  - `/search`
  - `/recent`
- It verifies anonymous admin requests redirect to login for:
  - `/admin`
  - `/admin/reports`
  - `/admin/email-diagnostics`
- It verifies representative API search and a Persian Course lesson helper projection.
- Confirm helper-language settings work with at least:
  - primary Persian
  - primary English
  - Persian plus English secondary
- Confirm no page creates horizontal scrolling at mobile width around 390px.
- Confirm admin reports do not show new malformed JSON, unsupported activity targets, or unresolved high-priority links.

## Tester Tasks

Ask each tester to complete only a small set. Do not overload them.

### Task 1: First Impression

1. Open `https://darwinlingua.com`.
2. Find the learning area that feels most relevant.
3. Say what you expected to do next and whether the page made that clear.

Record:

- Were they able to start without explanation?
- Which label or navigation item confused them?
- Did they understand that German is the source language and helper text is only support?

### Task 2: Course Flow

1. Open `/courses`.
2. Choose a CEFR level close to the tester's level.
3. Open one lesson.
4. Follow the visible lesson flow cards in order.
5. Open one linked activity if available.

Record:

- Did the lesson feel like a guided path or a loose collection of links?
- Were the activity titles and instructions clear?
- Did linked activities open where the tester expected?
- Did long text wrap correctly on desktop and mobile?

### Task 3: Helper Languages

1. Set primary helper language to the tester's strongest non-German language.
2. If relevant, set a secondary helper language.
3. Reopen Course, Writing Templates, Exam Prep, and Life in Germany pages.

Record:

- Is the helper text visible where expected?
- Is the translation natural, not word-for-word?
- For Persian, Arabic, Kurdish, or other RTL text: is direction and alignment readable?
- If two helper languages are selected, is it clear why one or both are shown?

### Task 4: Writing Templates

1. Open `/writing-templates`.
2. Choose a template close to a real-life need.
3. Read the template text and sample filled version.
4. Try to explain how the tester would adapt it.

Record:

- Did template text and sample text wrap correctly?
- Were variables understandable?
- Was the example realistic for the CEFR level?
- Was the helper translation useful without replacing the German text?

### Task 5: Exam Prep

1. Open `/exam-prep`.
2. Choose a relevant profile or exam level.
3. Open one unit.
4. Read strategy notes and linked practice.

Record:

- Did the title avoid duplicated metadata?
- Were strategy notes concrete enough?
- Were linked practice materials relevant?
- Did the content avoid pretending to be official exam material?

### Task 6: Life In Germany

1. Open `/life-in-germany`.
2. Choose one everyday or civic/legal topic.
3. Read the note with helper text.

Record:

- Did the explanation help with real life in Germany, not only exam memorization?
- Was legal/civic content careful and non-misleading?
- Was any example culturally unclear for the tester's language background?

### Task 7: Search And Recent

1. Search for `Termin`, `Antrag`, `Pruefung`, and `Demokratie`.
2. Open one result.
3. Visit `/recent`.

Record:

- Did search results feel relevant?
- Did labels and result types make sense?
- Did Recent show useful next steps?

## Feedback Format

Use one row per issue or observation:

Template: `docs/87-Web-Tester-Feedback-Template.csv`

After feedback rows are collected, generate a triage report:

```powershell
.\tools\Web\Convert-WebTesterFeedbackToReport.ps1 -FeedbackCsvPath docs\87-Web-Tester-Feedback-Template.csv
```

For a generated bundle, use that bundle's `WebTesterFeedback.csv` path instead of the template path.
The report is written under `artifacts/validation/web-tester-feedback/` as JSON and Markdown. It sorts blocker and major issues first, flags invalid rows, counts RTL-language observations, and identifies fix-now candidates.
`-FeedbackCsvPath` accepts either a repository-relative path or an absolute path to a collected tester CSV.

| Field | Required | Notes |
| --- | --- | --- |
| Tester code | yes | Do not store unnecessary personal data. |
| Date | yes | Use `YYYY-MM-DD`. |
| Device/browser | yes | Example: Windows Chrome, Android Chrome, iPhone Safari. |
| Page URL | yes | Full URL or route. |
| Task | yes | Use task number/name from this runbook. |
| Severity | yes | `blocker`, `major`, `minor`, `content-quality`, `suggestion`. |
| Language settings | yes | Example: `fa primary, en secondary`. |
| What happened | yes | Concrete observation, not interpretation only. |
| Expected behavior | yes | What the tester expected. |
| Screenshot/video | optional | Useful for layout issues. |
| Follow-up decision | later | `fix-now`, `backlog`, `won't-fix`, `needs-review`. |

## Triage Rules

Fix immediately before adding new content when:

- a public route is unavailable
- login/settings breaks normal testing
- helper-language display is missing or unreadable
- RTL text renders in the wrong direction
- a lesson flow link opens the wrong destination
- a page has horizontal overflow on common mobile widths
- content gives misleading legal, exam, or safety guidance

Backlog, but do not block testing, when:

- wording can be clearer but the task is still usable
- a nice-to-have filter or shortcut is missing
- a linked practice item is relevant but not ideal
- an advanced-level example could be richer

## Completion Criteria For This Test Pass

The first controlled Web tester pass is complete when:

- at least 5 testers complete the core tasks
- at least one RTL helper-language session is reviewed
- all blocker and major issues are triaged
- every fix-now item has either a patch or a documented owner
- docs and admin/readiness notes are updated
- a new phase backup is taken after accepted fixes

## Out Of Scope

- Mobile/MAUI testing
- bulk new content generation
- AI scoring or AI feedback
- public marketing launch
- official exam question-bank reproduction

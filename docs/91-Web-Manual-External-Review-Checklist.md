# Web Manual External Review Checklist

## Purpose

This checklist closes the remaining human validation gates before the controlled Web tester pass is treated as ready to start.

It does not replace legal counsel, real tester feedback, or target-browser checks. It gives the operator one evidence format for the manual work that cannot be fully proven by automated scripts.

## Scope

- Public Web: `https://darwinlingua.com`
- Public API: `https://api.darwinlingua.com`
- Do not use `www.darwinlingua.com` unless that host is intentionally configured later.
- Mobile/MAUI is out of scope.
- Public paid billing remains disabled.

## A. Transactional Email Mailbox Review

Use this after the Brevo real-delivery and app-level email-link smokes have passed.

Evidence inputs:

- Latest safe template preview: `artifacts/validation/transactional-email-template-preview/`
- Latest action-link smoke: `artifacts/validation/web-account-email-link-smoke/`
- Latest Brevo transactional log check: `artifacts/validation/brevo-transactional-log-check/`
- Real mailbox: `info@darwinlingua.com`

Manual checks:

- [ ] Open the latest real registration confirmation email in the mailbox.
- [ ] Confirm the sender name is `Darwin Lingua` and the sender address is the verified `no-reply@darwinlingua.com` sender.
- [ ] Confirm the reply/support path is clear and uses `support@darwinlingua.com` where shown.
- [ ] Confirm the HTML layout looks branded and readable on desktop webmail.
- [ ] Confirm the HTML layout looks readable on a phone-width mail view.
- [ ] Confirm the plain-text alternative is readable if the mail client exposes it.
- [ ] Confirm action buttons/links clearly explain what they do.
- [ ] Confirm action links resolve to `https://darwinlingua.com`, not `www` and not the old temporary domain.
- [ ] Confirm no email displays raw reset tokens, webhook secrets, API keys, internal provider message ids, or diagnostic hashes.
- [ ] Repeat the same review for password reset, password reset completed, email change confirmation, and old-email notification if present in the mailbox.

Record:

- Reviewer:
- Date:
- Mail client/browser:
- Device:
- Result: `passed`, `passed-with-notes`, or `failed`
- Notes:

## B. PWA Install Review

Use this after the automated PWA installability report has zero failed checks.

Evidence inputs:

- `docs/56-Web-Pwa-Install-Validation-Worksheet.md`
- `artifacts/validation/pwa-installability/`
- Public URL: `https://darwinlingua.com`

Desktop Chrome or Edge:

- [ ] Open `https://darwinlingua.com`.
- [ ] Confirm the browser offers install/app mode when eligible.
- [ ] Install the app.
- [ ] Launch it from the operating-system app launcher.
- [ ] Confirm the installed window opens without normal browser chrome.
- [ ] Confirm `Home`, `Browse`, `Search`, `Favorites`, `Recent`, and `Settings` open correctly.
- [ ] Confirm admin routes still require sign-in and do not look like normal learner content.
- [ ] Uninstall the app if this was only a test.

Android Chrome, if available:

- [ ] Open `https://darwinlingua.com`.
- [ ] Use Add to Home screen or the install prompt.
- [ ] Launch from the home screen.
- [ ] Confirm the app shell opens and basic navigation works.
- [ ] Confirm offline relaunch shows the cached shell or offline page instead of a broken browser error.

Record:

- Reviewer:
- Date:
- Browser/device:
- Result: `passed`, `passed-with-notes`, `failed`, or `not-in-scope-for-this-pass`
- Notes:

## C. Controlled Tester Pass Start Gate

Evidence inputs:

- Current tester bundle under `artifacts/validation/web-tester-runs/`
- Current preflight JSON under that bundle's `preflight/` folder
- Feedback CSV in that bundle

Before inviting testers:

- [ ] Confirm the tester bundle README says the pass is Brevo-ready and uses self-registration as the normal path.
- [ ] Confirm the preflight report shows all checks passed.
- [ ] Confirm `TesterQuickStart.md` is the only tester-facing instruction file shared directly with testers.
- [ ] Confirm `WebTesterAccounts.csv` is kept operator-only and contains only real tester emails when Premium access is granted.
- [ ] Confirm testers understand this is a controlled test, not a public marketing launch.
- [ ] Confirm at least one tester will use a right-to-left helper language such as Persian or Arabic.
- [ ] Confirm at least one tester will use English helper text.

After feedback arrives:

- [ ] Run `tools/Web/Convert-WebTesterFeedbackToReport.ps1` on the collected `WebTesterFeedback.csv`.
- [ ] Run the same command with `-FailOnMajor` before closing the pass.
- [ ] Fix or assign every blocker and major issue.
- [ ] Update docs with accepted findings and follow-up decisions.
- [ ] Take a phase backup after accepted fixes.

Record:

- Tester bundle:
- Invited tester count:
- Completed tester count:
- Triage report:
- Result: `ready-to-invite`, `needs-fix-before-invite`, `in-progress`, or `closed`
- Notes:

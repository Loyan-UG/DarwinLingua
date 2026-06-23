# Web Human Gate Handoff

## Purpose

This document defines the final human-gate handoff before the controlled Web tester pass can start.

The automated Web stack can be ready while human gates remain open. This handoff keeps those decisions separate so an operator does not confuse automated readiness with permission to invite testers or launch publicly.

## Generate The Current Handoff

From the repository root:

```powershell
.\tools\Web\New-WebHumanGateHandoff.ps1 -GenerateFreshAudit
```

The tool writes timestamped Markdown and JSON reports under:

```text
artifacts/validation/web-human-gate-handoff/
```

For a single operator-facing packet that includes the current Brevo Authorized IP action, mailbox review, PWA install checks, and tester-start gate, run:

```powershell
.\tools\Web\New-WebExternalActionPacket.ps1 -GenerateFreshAudit -RunBrevoReadinessCheck
```

The packet is written under:

```text
artifacts/validation/web-external-action-packet/
```

Generated packets may include the current IP address that Brevo asks to authorize, but must never include Brevo API keys, webhook tokens, raw action URLs, reset tokens, provider message ids, diagnostic hashes, or full real email bodies.

When the real mailbox review CSV is filled in, validate it before using it to close the mailbox-rendering gate:

```powershell
.\tools\Web\Test-WebManualExternalEvidence.ps1 -FailOnIssue -FailOnOpenMailboxRows
```

The generated handoff reads the latest `New-WebControlledTesterReadinessAudit.ps1` evidence and lists the current manual statuses for:

- real mailbox rendering review
- desktop PWA install acceptance
- Android PWA install acceptance or explicit out-of-scope decision
- controlled tester pass start status

Before closing the legal/operator review row for tester invitations, also run:

```powershell
.\tools\Web\New-WebLegalSurfaceAudit.ps1 -FailOnIssue
```

This confirms that the public legal/support pages render from `https://darwinlingua.com` with configured operator/contact text and without placeholders, old temporary domains, `www`, or obvious secret leaks.

## Required Public Hosts

- Web: `https://darwinlingua.com`
- API: `https://api.darwinlingua.com/health`
- `www.darwinlingua.com` is not a required host unless it is intentionally configured later.

## Gate Closure Rule

Do not mark a gate as passed unless the evidence was actually reviewed.
The manual report also keeps a gate open when a passing status is supplied without the required evidence path, for example mailbox rendering without a CSV, PWA review without worksheet evidence, or tester-pass status without a tester bundle. This is intentional: readiness must be backed by reviewable evidence, not only by a status label.

Allowed non-passing statuses must remain visible:

- `not-reviewed`
- `not-started`
- `failed`
- `needs-fix-before-invite`

`not-in-scope-for-this-pass` is acceptable only for a browser/device install check that is intentionally excluded from the controlled tester pass. It is not a broad launch sign-off.

## Brevo Authorized IP Note

If Brevo API verification fails with an `unrecognised IP address` error, the operator must add the current machine/server IP in Brevo under `Security` -> `Authorised IPs`, then rerun the production readiness check:

```powershell
.\tools\Web\Invoke-BrevoProductionReadinessCheck.ps1 `
  -VerifyBrevoApi `
  -RequireRealDelivery `
  -SenderVerified `
  -DnsAuthenticated `
  -WebhookConfigured `
  -DpaAccepted
```

Never record Brevo API keys, webhook tokens, raw action URLs, reset tokens, provider message ids, diagnostic hashes, or full real email bodies in Git-tracked files or handoff evidence.

## Relationship To Other Documents

- `91-Web-Manual-External-Review-Checklist.md` is the full manual checklist.
- `87-Web-Tester-Onboarding-Runbook.md` is the operator runbook for the tester pass.
- `88-Web-Tester-Quick-Start.md` is the tester-facing brief.
- `90-Web-Operational-Incident-Runbook.md` covers incidents after the pass starts.
- `92-Web-Legal-Research-And-Risk-Audit.md` covers legal risk evidence; broad public launch still needs final operator/legal review.

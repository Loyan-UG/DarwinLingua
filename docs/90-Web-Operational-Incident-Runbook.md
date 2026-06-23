# Web Operational Incident Runbook

Last updated: 2026-06-23.

This runbook defines the current Web-first operational ownership and response process for Darwin Lingua during controlled Web testing. Mobile remains deferred.

## Scope

This runbook covers:

- Brevo API key compromise, rotation, sender-domain failure, account suspension, webhook delivery failure, and transactional email delivery incidents.
- DNS or Cloudflare routing incidents for `darwinlingua.com` and `api.darwinlingua.com`.
- Account/privacy incidents, including data-subject requests that cannot be completed through self-service export/delete.
- Security or abuse incidents that may require temporary account suspension, content moderation, or operator escalation.
- Backup/restore incidents where the system must be restored from Git plus `X:\Projects\DarwinLingua.Backup`.

Billing/Stripe incidents are deferred while public paid billing is disabled. Re-open `docs/75-Stripe-Billing-Validation-Playbook.md` before enabling self-service paid subscriptions.

## Owners

| Area | Primary owner | Contact |
| --- | --- | --- |
| Product/operator responsibility | Shahram Vafadar | `info@darwinlingua.com` |
| Privacy/data-subject requests | Shahram Vafadar | `info@darwinlingua.com` |
| Brevo account, sender domain, API key rotation, and plan upgrades | Shahram Vafadar | `info@darwinlingua.com` |
| DNS and Cloudflare routing | Shahram Vafadar | `info@darwinlingua.com` |
| Community moderation and illegal-content escalation before broad community launch | Shahram Vafadar | `info@darwinlingua.com` |
| Backup/restore decision | Shahram Vafadar | `info@darwinlingua.com` |

## Severity

| Severity | Examples | Initial response target |
| --- | --- | --- |
| SEV-1 | Data breach, exposed secret, account takeover pattern, public site unavailable, wrong DNS routing to another service | Same day |
| SEV-2 | Brevo delivery blocked, webhook failing, admin cannot access diagnostics, repeated transactional failures, privacy request cannot be completed by self-service | 1 business day |
| SEV-3 | Single failed email, isolated tester issue, non-critical content/reporting issue | 3 business days |

## Brevo API Key Rotation

Use this flow when a Brevo API key is exposed, suspected to be exposed, or scheduled for rotation.

1. In Brevo, create a new API key for Darwin Lingua transactional email.
2. Store the new key only in local/production secret storage. Do not commit it and do not paste it into docs.
3. Update the Web app secret `TransactionalEmail:BrevoApiKey`.
4. Restart `DarwinLingua.Web`.
5. Run:

```powershell
.\tools\Web\Invoke-BrevoProductionReadinessCheck.ps1 `
  -ConfigPath .\src\Apps\DarwinLingua.Web\appsettings.Development.Local.json `
  -SendingDomain "darwinlingua.com" `
  -SenderVerified `
  -DnsAuthenticated `
  -WebhookConfigured `
  -DpaAccepted `
  -RequireRealDelivery `
  -VerifyBrevoApi
```

6. Run the app-level smoke:

```powershell
.\tools\Web\Invoke-WebAccountEmailFlowSmoke.ps1
```

7. Revoke the old API key in Brevo after the new key passes readiness and smoke.
8. Take a phase backup in `X:\Projects\DarwinLingua.Backup`.

## Brevo Sender-Domain Or Account Failure

Use this flow if Brevo reports sender-domain failure, blocked sending, account suspension, or unexplained delivery rejection.

1. Pause broad tester self-registration if confirmation or password-reset delivery is unreliable.
2. Check Brevo Transactional logs for message id, delivery state, bounce/error details, and webhook status.
3. Check `/admin/email-diagnostics` for provider message ids, provider events, suppressions, and readiness warnings.
4. Re-run:

```powershell
.\tools\Web\Invoke-BrevoProductionReadinessCheck.ps1 `
  -ConfigPath .\src\Apps\DarwinLingua.Web\appsettings.Development.Local.json `
  -SendingDomain "darwinlingua.com" `
  -SenderVerified `
  -DnsAuthenticated `
  -WebhookConfigured `
  -DpaAccepted `
  -RequireRealDelivery `
  -VerifyBrevoApi
```

5. Verify DNS records in Cloudflare against the exact SPF/DKIM/DMARC values shown by Brevo.
6. If Brevo rejects API calls with `unrecognised IP address`, add the current operator/server IP in Brevo Authorized IPs.
7. If Brevo account suspension is shown, open a Brevo support ticket and keep tester registration paused until sending is restored.

## Webhook Failure

Use this flow if Brevo events do not appear in `/admin/email-diagnostics`.

1. Confirm the Brevo outbound webhook URL is:

```text
https://darwinlingua.com/webhooks/brevo/transactional-email
```

2. Confirm webhook authentication method is `Token` and the token matches `TransactionalEmail:BrevoWebhookSecret`.
3. Run:

```powershell
.\tools\Web\Invoke-BrevoWebhookSuppressionSmoke.ps1
```

4. Run:

```powershell
.\tools\Web\Invoke-WebEmailDiagnosticsAdminSmoke.ps1 -UseLocalDevelopmentSeed
```

5. If provider events are visible in Brevo but webhook delivery failed, use the Admin-only manual provider-event reconciliation form only for support recovery.

## Privacy And Account Requests

Self-service export/delete is the primary path for normal users. Manual escalation is required when:

- the user cannot sign in,
- identity/account ownership is unclear,
- backup retention or security retention must be explained,
- billing/provider data would be involved later,
- abuse/security retention is required.

Process:

1. Route requests to `info@darwinlingua.com`.
2. Verify the request relates to the correct account without collecting unnecessary identity documents.
3. Prefer self-service export/delete when the user can authenticate.
4. For manual handling, record the request date, requested action, verification method, and final response.
5. Use the GDPR one-month response baseline unless an extension is legally justified.
6. If a breach is suspected, follow the breach process below.

## Security Or Breach Triage

1. Preserve logs and stop any process that would worsen exposure.
2. Rotate exposed secrets immediately.
3. Determine affected data categories: account, learning progress, preferences, delivery logs, billing if enabled later.
4. Check whether notification to users or a supervisory authority is required under GDPR Articles 33/34.
5. Document facts, timing, mitigation, and owner decisions.
6. Take a post-mitigation backup only after secrets have been rotated and the system is stable.

## DNS Or Cloudflare Routing Incident

1. Check that `darwinlingua.com` routes to the Web origin and `api.darwinlingua.com` routes to the WebApi origin.
2. Do not rely on `www.darwinlingua.com`; it is not part of the current required route unless explicitly configured later.
3. Run:

```powershell
$urls=@('https://darwinlingua.com','https://darwinlingua.com/legal','https://darwinlingua.com/privacy','https://api.darwinlingua.com/health')
foreach($url in $urls){
  $tmp=New-TemporaryFile
  $status=& curl.exe -k -L -s -o $tmp.FullName -w '%{http_code}' $url
  $len=(Get-Item -LiteralPath $tmp.FullName).Length
  Remove-Item -LiteralPath $tmp.FullName -Force
  "$url $status bytes=$len"
}
```

4. If Cloudflare tunnel or DNS records changed, restore the last known-good values from the latest phase backup and Cloudflare dashboard.

## Backup/Restore Incident

1. Identify the target checkpoint in `X:\Projects\DarwinLingua.Backup`.
2. Check `manifest.md` for the Git commit and phase label.
3. Start from that Git commit.
4. Restore secrets from `secrets/`.
5. Restore PostgreSQL using the `.dump` file and verify `*.restore-list.txt`.
6. Run Web/WebApi smoke before exposing the environment.

## Current Deferred Items

- Real inbox rendering must still be confirmed manually in `info@darwinlingua.com`.
- Brevo dashboard monitoring over time remains an operational habit, not an implementation blocker. The automated provider-log check, public action-link smoke, webhook suppression smoke, suppressed-send smoke, and Admin Email Diagnostics smoke have passed for the controlled Web stack.
- Stripe remains disabled and must be separately validated before public paid subscriptions.
- PWA install prompt acceptance still requires real target-browser testing.
- Legal/operator copy is an engineering baseline and should receive owner/counsel review before broad public launch.

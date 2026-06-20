# Transactional Email and Account Communication Backlog

## Purpose

This document defines the required transactional email and account-communication work for Darwin Lingua before public release.

This is not optional polish. Account email flows are a release-quality requirement because they affect:

- registration UX
- account recovery
- account security
- trust and deliverability
- support load
- GDPR/privacy expectations
- abuse prevention
- premium and organizer workflows

The project is no longer planned as a throwaway MVP. Therefore, the system must ship with production-grade transactional email foundations and no avoidable technical debt.

---

## Current Gap

The existing documentation and backlog already cover web identity, learner registration, roles, entitlements, organizer profiles, moderation, and release validation.

However, the product planning did not explicitly include transactional email foundations such as:

- email confirmation after registration
- password reset email
- email change confirmation
- security notification emails
- organizer claim/request notifications
- admin/operator notifications
- localized email templates
- SMTP/provider abstraction
- email delivery logging and diagnostics
- resend and failure UX

This is a major UX and release-readiness gap.

---

## Release Policy

The public product must not be released without at least:

- working email confirmation flow
- working password reset flow
- production email provider configuration
- localized user-facing email content
- resend confirmation UX
- delivery logging and diagnostics
- rate limiting for email-triggering endpoints
- tests for token generation, token validation, expired tokens, invalid tokens, and successful account recovery

## Current Implementation Snapshot

The web app now has a production-provider path for transactional email:

- `TransactionalEmail:Mode=BrevoApi` sends through Brevo's transactional API.
- Transactional email templates now render a designed HTML document with a branded card layout, inline email-safe styling, a plain-text alternative, and dark-mode readability hints.
- Brevo sandbox mode sends the `X-Sib-Sandbox: drop` request header and payload header so staging tests can avoid real delivery.
- Delivery logs store provider message ids and webhook events for diagnostics.
- Permanent Brevo failure events, including hard bounces, blocked addresses, invalid email, spam, and complaints, add the recipient hash to the internal suppression list.
- Admin email diagnostics can inspect delivery logs, manual provider events, and suppressions.

Operational work still required before production launch: configure a real Brevo API key and webhook secret outside source control, verify the sender domain, set SPF/DKIM/DMARC, configure the Brevo transactional webhook URL, and review the provider DPA.

## Brevo Production Setup Runbook

The application is ready to use Brevo, but the operator must complete the provider-side setup before real production sending.

### Brevo dashboard tasks

1. Create or select the production Brevo account owned by the Darwin Lingua operator.
2. Add and verify the production sender identity, for example `no-reply@<production-domain>` with a human-readable sender name such as `Darwin Lingua`.
3. Add the sending domain in Brevo domain authentication.
4. Copy the exact SPF, DKIM, and DMARC-related DNS records Brevo gives you and publish them in the DNS zone for the sending domain. Do not guess these values; they are account/domain-specific.
5. Wait until Brevo shows the domain and sender as verified.
6. Create a transactional API key dedicated to Darwin Lingua. Store it only in the production secret store or the local secret bundle used for the target environment; never commit it.
7. Create a strong random webhook secret for Darwin Lingua.
8. Configure the Brevo transactional webhook URL:
   - URL: `https://<public-domain>/webhooks/brevo/transactional-email`
   - Method: `POST`
   - Secret header: `X-DarwinLingua-Brevo-Webhook-Secret`
   - Header value: the same value configured as `TransactionalEmail:BrevoWebhookSecret`
9. Select transactional events needed for diagnostics and suppression handling: delivered/sent, soft bounce, hard bounce, blocked, invalid email, spam, complaint, and other provider failure events Brevo exposes for transactional email.
10. Review and accept the required Brevo data-processing terms/DPA for EU/GDPR operation.

Official Brevo references to use during setup:

- Domain authentication: `https://help.brevo.com/hc/en-us/articles/12163873383186-Authenticate-your-domain-with-Brevo-Brevo-code-DKIM-DMARC`
- Transactional email API: `https://developers.brevo.com/docs/send-a-transactional-email`
- Transactional webhook events: `https://developers.brevo.com/docs/transactional-webhooks`
- Secured webhooks and custom headers/Bearer authorization: `https://developers.brevo.com/docs/secured-webhooks`

### Operator handoff

The developer cannot finish real delivery without these operator-owned values and external actions:

1. Brevo account owner email and organization name.
2. Sending domain to use during the current development/test phase.
3. Verified sender email, recommended shape: `no-reply@<verified-domain>`.
4. Support/reply-to email, recommended shape: `support@<verified-domain>`.
5. DNS access for the sending domain, or confirmation that Brevo automatic domain authentication may manage the required DNS records.
6. Brevo transactional API key, stored only in local/production secret configuration.
7. A strong random `TransactionalEmail:BrevoWebhookSecret`, stored in Darwin Lingua config and also configured in Brevo webhook authentication.
8. Confirmation that the Brevo DPA/data-processing terms have been reviewed and accepted for the operating entity.

For the current development environment, `https://lingua.vafadar.pro` is the temporary public base URL. Do not configure `www.lingua.vafadar.pro` unless that host is intentionally enabled. When Web moves to the final main domain, update the following together in the same release step:

- `TransactionalEmail:PublicBaseUrl`
- Brevo webhook URL
- sender/domain authentication if the sending domain changes
- legal pages and support contact references
- DNS records shown by Brevo for the final sending domain

### Webhook authentication choice

Darwin Lingua accepts these secure webhook authentication forms:

- Preferred: Bearer token, with `Authorization: Bearer <TransactionalEmail:BrevoWebhookSecret>`.
- Also supported: custom header `X-DarwinLingua-Brevo-Webhook-Secret: <TransactionalEmail:BrevoWebhookSecret>`.

The query-string fallback is only for local diagnostics and must stay disabled outside local development:

```json
"BrevoAllowQuerySecretFallback": false
```

### Darwin Lingua configuration

Set these values outside source control for the target environment:

```json
{
  "TransactionalEmail": {
    "Mode": "BrevoApi",
    "PublicBaseUrl": "https://lingua.vafadar.pro",
    "ProductName": "Darwin Lingua",
    "FromEmail": "no-reply@<verified-domain>",
    "FromName": "Darwin Lingua",
    "ReplyToEmail": "support@<verified-domain>",
    "SupportEmail": "support@<verified-domain>",
    "BrevoApiBaseUrl": "https://api.brevo.com",
    "BrevoApiKey": "<secret-from-brevo>",
    "BrevoWebhookSecret": "<strong-random-secret>",
    "BrevoSandboxMode": false,
    "BrevoAllowQuerySecretFallback": false
  }
}
```

During the current development phase, `lingua.vafadar.pro` is the temporary public base URL. When Web is mature and moved to the main domain, update `PublicBaseUrl`, sender/domain DNS, legal pages, and the Brevo webhook URL together.

### Verification checklist

- Run the repeatable local readiness check after configuring secrets and after every domain/sender change:

```powershell
.\tools\Web\Invoke-BrevoProductionReadinessCheck.ps1 `
  -ConfigPath .\src\Apps\DarwinLingua.Web\appsettings.Development.Local.json `
  -SendingDomain "<verified-domain>" `
  -SenderVerified `
  -DnsAuthenticated `
  -WebhookConfigured `
  -DpaAccepted `
  -RequireRealDelivery
```

- The readiness tool writes JSON and Markdown reports under `artifacts/validation/brevo-readiness/`. It does not print API keys or webhook secrets; it only reports whether they appear configured.
- Treat every `blocker` in that report as a hard stop before asking testers to self-register or before enabling real transactional delivery.
- Send one email-confirmation message to a real inbox.
- Send one password-reset message to a real inbox.
- Confirm that both HTML and plain-text alternatives render correctly.
- Confirm that action links use the configured `PublicBaseUrl`.
- Trigger or manually reconcile one Brevo event and verify Admin Email Diagnostics receives or can reconcile provider status.
- Confirm hard bounce/spam/complaint events create internal recipient suppression.
- Confirm `BrevoSandboxMode=false` only after sender/domain verification is complete.
- Confirm Admin -> Email diagnostics reports Brevo mode, API key configured, webhook secret configured, sandbox disabled, and no production-readiness warnings.

---

## Status Tracking Rule

Use these markers consistently:

- `[ ]` not started
- `[-]` in progress
- `[x]` completed
- `[!]` blocked / needs decision

Do not delete completed items. Mark them as completed so progress remains visible.

---

## 1. Product and UX Requirements

### Goals

Email flows must feel reliable, clear, and safe. A learner should never be stuck after registering or forgetting a password.

### Backlog

- [x] define the full list of transactional email scenarios for the first public release
- [x] define account-state UX for unconfirmed email users
- [x] define clear resend-confirmation behavior
- [x] define expired-token UX and recovery path
- [x] define invalid-token UX without leaking account existence
- [x] define success pages for confirmation, password reset, and email change
- [x] define user-facing copy for all email-related screens
- [x] define support fallback text for users who cannot receive email
- [x] define anti-enumeration wording for password reset and confirmation resend
- [x] define what actions require confirmed email
- [x] define whether browsing is allowed before confirmation
- [x] define whether premium purchase, organizer claim, RSVP, matching, and profile publishing require confirmed email

### Recommended Product Rules

| Area | Recommendation |
|---|---|
| Registration | Allow account creation, send confirmation immediately |
| Login before confirmation | Allow login only if product policy permits limited browsing; otherwise show confirmation-required page |
| Premium features | Require confirmed email |
| Organizer claim | Require confirmed email |
| Public profile | Require confirmed email |
| Partner matching | Require confirmed email |
| Admin/operator account | Require confirmed email and stronger policy |
| Password reset | Always show neutral success message whether the email exists or not |

---

## 2. Email Scenarios Required For Release

### Account Lifecycle Emails

- [x] registration email confirmation
- [x] resend email confirmation
- [x] password reset request
- [x] password reset success notification
- [x] email address change confirmation for the new address
- [x] email address changed notification to the old address
- [x] account locked or suspicious login notification if lockout is enabled
- [ ] account deleted/deactivated confirmation if account deletion exists

### Security and Trust Emails

- [x] password changed notification
- [ ] new login from unknown device or location placeholder if device tracking is later implemented
- [ ] two-factor authentication setup placeholder if 2FA is later implemented
- [ ] recovery code regeneration placeholder if 2FA is later implemented

### Organizer and Community Emails

- [x] organizer claim submitted notification to the claimant
- [x] organizer claim approved notification
- [x] organizer claim rejected notification
- [x] organizer profile ownership changed notification
- [x] event RSVP confirmation notification if RSVP is enabled
- [ ] event cancellation/update notification if event management is enabled
- [ ] partner request received notification if matching is enabled
- [x] partner request accepted notification if matching is enabled
- [x] moderation/report outcome notification where appropriate

### Admin/Operator Emails

- [x] new organizer claim admin notification
- [x] high-severity user report admin notification
- [x] repeated email delivery failure admin notification
- [ ] publish/import failure admin notification placeholder if useful
- [ ] payment/subscription failure admin notification placeholder if payment is added later

---

## 3. Architecture Requirements

### Goals

Transactional email must be provider-independent and testable. Do not couple business workflows directly to SMTP or a specific vendor SDK.

### Required Abstractions

- [x] define `IEmailSender` or equivalent application abstraction
- [x] define `IEmailTemplateRenderer` or equivalent template abstraction
- [x] define `IEmailMessageFactory` or equivalent scenario-specific message factory
- [x] define `IEmailDeliveryLogRepository` or equivalent persistence boundary
- [x] define email provider options and validation
- [x] define environment-specific behavior for development, staging, and production
- [-] define background sending boundary if synchronous sending becomes unreliable
- [x] define retry policy and non-retryable failure handling
- [x] define cancellation-token support for all email operations
- [x] define correlation/request id storage for diagnostics

Current boundary: transactional emails are still sent synchronously with retry handling, while a hosted failure monitor watches delivery logs and notifies admins when repeated provider failures exceed the configured threshold. A durable background queue should be added only if request-path sending becomes unreliable under production load.

### Provider Strategy

The first implementation should support one production provider and one local development provider.

| Environment | Recommendation |
|---|---|
| Development | log-to-file/in-memory sink or local SMTP catcher such as Mailpit/MailHog |
| Staging | Brevo API mode with test domain/subdomain |
| Production | Brevo API mode with verified domain, SPF, DKIM, DMARC, bounce handling, and webhook events |

The implementation must not hard-code provider-specific logic into controllers, Razor pages, handlers, or domain entities.

Current implementation notes:

- [x] Brevo API mode posts transactional messages to `/v3/smtp/email`.
- [x] Brevo API authentication uses the `api-key` header.
- [x] Brevo sandbox mode sends `X-Sib-Sandbox: drop`.
- [x] Brevo webhook events update delivery logs.
- [x] Permanent Brevo events create internal recipient suppressions.
- [x] Brevo API failure responses are summarized from provider `code` and `message` fields when available.

---

## 4. Domain and Persistence Requirements

### Email Delivery Log

Add a durable delivery log for observability and support.

Suggested fields:

- id
- scenario key
- recipient email hash or normalized email depending on privacy decision
- recipient user id where available
- template key
- culture
- subject
- provider name
- provider message id
- provider last event
- provider last event at UTC
- provider last event reason
- status: queued, sent, failed, skipped, suppressed
- failure code
- failure message summary
- retry count
- created at UTC
- sent at UTC
- last attempt at UTC
- correlation id

### Backlog

- [x] define email delivery status enum
- [x] define email scenario keys
- [x] define email delivery log entity
- [-] define EF Core mapping and migration
- [x] define retention policy for delivery logs
- [x] avoid storing sensitive token values in logs
- [x] avoid logging full reset/confirmation URLs
- [x] define Admin-only cleanup job or manual cleanup endpoint for old logs
- [x] add admin diagnostics page for recent email delivery status
- [x] define internal suppression behavior for permanent provider failures
- [x] handle Brevo complaint events as permanent suppressions

---

## 5. Token and Security Requirements

### Goals

Email tokens are security-sensitive. They must be short-lived, single-purpose, and validated through ASP.NET Core Identity where possible.

### Backlog

- [x] use ASP.NET Core Identity token providers for confirmation and password reset unless a stronger reason exists
- [x] configure token lifetimes explicitly
- [x] define token lifetime for email confirmation
- [x] define token lifetime for password reset
- [x] define token lifetime for email change
- [x] ensure tokens are URL-safe and encoded correctly
- [x] ensure reset/confirmation URLs are generated from configured public base URL, not from unsafe request headers alone
- [x] protect against user enumeration in reset and resend flows
- [x] protect against user enumeration in registration for existing email addresses
- [x] rate-limit password reset requests
- [x] rate-limit confirmation resend requests
- [x] rate-limit registration confirmation sending
- [x] invalidate or make old flows harmless after email/password changes where applicable
- [ ] add tests for expired, invalid, reused, malformed, and wrong-purpose tokens

### Suggested Initial Policy

| Token | Suggested Lifetime |
|---|---:|
| Email confirmation | 24 hours |
| Password reset | 30-60 minutes |
| Email change confirmation | 60 minutes |

These values can be adjusted, but they must be explicit.

---

## 6. Localization and Template Requirements

### Goals

Emails are user-facing product surfaces. They must follow the same localization quality rules as the app and website.

### Required Template Languages

At minimum:

- English
- German

Later, email body content may support the user's selected UI language and possibly selected helper languages.

### Template Requirements

- [-] define template keys for every transactional scenario
- [x] implement localized subject lines
- [x] implement localized plain-text bodies
- [x] implement localized HTML bodies if HTML email is used
- [x] keep templates outside controllers and handlers
- [x] include product name and support contact
- [x] include clear expiration text for token emails
- [x] include fallback copyable URL when using button-style HTML emails
- [x] avoid unnecessary personal data in email bodies
- [ ] test missing-localization fallback behavior
- [ ] add template rendering tests for English and German

### Minimum Templates For Release

- [x] `Account.EmailConfirmation`
- [x] `Account.PasswordReset`
- [x] `Account.PasswordResetCompleted`
- [x] `Account.EmailChangeConfirmation`
- [x] `Account.EmailChangedNotification`
- [x] `Organizer.ClaimSubmitted`
- [x] `Organizer.ClaimApproved`
- [x] `Organizer.ClaimRejected`
- [x] `Admin.NewOrganizerClaim`
- [x] `Moderation.HighSeverityReport`

---

## 7. Web Account Flow Backlog

### Registration and Confirmation

- [x] send confirmation email immediately after successful registration
- [x] show a `Check your email` page after registration
- [x] add `Resend confirmation email` page/action
- [x] add confirmation callback endpoint/page
- [x] show success page after valid confirmation
- [x] show expired/invalid token page with resend option
- [x] block or limit protected features for unconfirmed users according to product policy
- [ ] add tests for confirmed and unconfirmed login behavior

### Password Reset

- [x] add `Forgot password` page
- [x] add password reset request handler
- [x] send password reset email with neutral success response
- [x] add reset password page
- [x] validate reset token and update password
- [x] send password reset success notification
- [x] show success page after reset
- [ ] add tests for non-existing email, invalid token, expired token, and successful reset

### Email Change

- [x] add change-email screen if account profile editing exists
- [x] send confirmation to new email address
- [x] notify old email address after successful change
- [x] require re-authentication for email change if necessary
- [ ] add tests for change-email token validation and old-email notification

---

## 8. API and Mobile Account Flow Backlog

### Goals

Mobile account flows must use the same identity system and email infrastructure as Web.

### Backlog

- [ ] define mobile registration flow and confirmation UX
- [ ] define mobile forgot-password flow
- [ ] decide whether password reset completes inside the app, the web app, or both
- [ ] define deep-link strategy for confirmation and password reset if mobile handles links
- [ ] define fallback web URLs for mobile email links
- [ ] expose safe API endpoints for registration/resend/reset when mobile account flows are added
- [ ] ensure all account endpoints share rate limiting and anti-enumeration behavior
- [ ] add integration tests for API account email flows

### Recommendation

For the first release, use web-hosted confirmation and password-reset pages from email links. Mobile can later add deep links, but web fallback must always work.

---

## 9. Admin and Operations Backlog

### Admin Email Diagnostics

- [x] add admin page for recent transactional email deliveries
- [x] add admin readiness warnings for transactional email configuration
- [x] add transactional email summary metrics to the admin system report
- [x] filter by status, scenario, date range, and recipient/user where privacy policy allows
- [x] show provider message id when available
- [x] show safe failure summary without leaking tokens or sensitive URLs
- [x] allow manual resend for safe scenarios such as confirmation email
- [x] do not allow manual resend of expired reset tokens; generate a new token instead

### Operational Configuration

- [x] add production email provider settings
- [x] add staging email provider settings
- [x] add local development email sink settings
- [x] validate required email settings at startup in production
- [x] document DNS requirements: SPF, DKIM, DMARC, bounce handling
- [x] document sender identity: from address, reply-to address, support address
- [x] document provider limits and cost assumptions

---

## 10. UX and Copy Backlog

### Required Pages

- [x] registration success / check email
- [x] confirmation success
- [x] confirmation failed or expired
- [x] resend confirmation
- [x] forgot password
- [x] reset password
- [x] reset password success
- [x] change email confirmation sent
- [x] email changed success
- [x] unconfirmed account gate page

### UX Rules

- [x] do not reveal whether an email exists during password reset
- [x] do not show raw token errors to users
- [x] provide clear next step after every account email action
- [x] include support fallback text only where useful
- [x] keep wording short and localized
- [x] keep error states consistent with existing UI components

---

## 11. Testing Backlog

### Unit Tests

- [ ] email message factory creates expected scenario keys
- [ ] template renderer returns localized English and German output
- [ ] missing template or missing localization fails safely
- [x] provider options validation catches missing production settings
- [x] Brevo API sender posts the expected payload, API key header, and sandbox header
- [x] Brevo API sender returns useful provider error summaries
- [x] Brevo complaint events mark delivery failed and suppress the recipient

### Integration Tests

- [ ] registration sends confirmation email
- [ ] resend confirmation sends a new confirmation email
- [ ] confirmed user can access confirmed-only flows
- [ ] unconfirmed user is blocked from confirmed-only flows
- [ ] password reset request returns neutral response for existing and non-existing emails
- [ ] password reset token resets password successfully
- [ ] expired password reset token fails safely
- [ ] invalid password reset token fails safely
- [ ] malformed token fails safely
- [ ] email change sends confirmation to new email
- [ ] successful email change notifies old email
- [ ] rate limit prevents abusive repeated reset/resend attempts

### Manual Validation

- [ ] registration email received in development sink
- [ ] registration email received in staging inbox
- [ ] confirmation link works from desktop browser
- [ ] confirmation link works from mobile browser
- [ ] password reset link works from desktop browser
- [ ] password reset link works from mobile browser
- [ ] German email template renders correctly
- [ ] English email template renders correctly
- [ ] dark-mode email clients remain readable if HTML email is used
- [ ] links use the correct public base URL

---

## 12. Security, Privacy, and Compliance Checklist

- [x] no reset or confirmation token is logged
- [x] no full magic link is logged
- [x] reset and resend endpoints are rate-limited
- [x] password reset response is account-enumeration safe
- [x] email confirmation resend response is account-enumeration safe where applicable
- [x] email delivery logs have a retention policy
- [x] unsubscribe links are not required for transactional security emails, but marketing emails must not be mixed into this channel
- [x] privacy policy mentions transactional emails and provider processing where required
- [-] data processing agreement with production email provider is reviewed before launch
- [ ] sender domain is verified with SPF, DKIM, and DMARC before production release

---

## 13. Suggested Implementation Order

1. Add email abstractions and provider options.
2. Add development email sink.
3. Add delivery log entity, migration, and repository.
4. Add localized template system.
5. Implement registration confirmation email.
6. Implement confirmation callback and resend UX.
7. Implement forgot-password and reset-password flow.
8. Implement reset-success notification.
9. Implement email-change flow if account profile editing exists.
10. Add admin diagnostics page.
11. Add rate limiting and anti-enumeration tests.
12. Configure staging/production provider.
13. Validate SPF, DKIM, DMARC, public base URL, and support sender identity.
14. Add release checklist gate.

---

## 14. Explicit Non-Goals For First Release

Do not add these until the basic account email flows are stable:

- marketing newsletters
- promotional campaigns
- drip courses by email
- AI-generated emails
- complex email automation builder
- multi-provider failover
- SMS fallback
- push-notification replacement for account recovery

Transactional email must stay cleanly separated from marketing communication.

---

## 15. Release Acceptance Criteria

The product is not release-ready until all of these are true:

- registration sends a localized confirmation email
- confirmation link works and produces a clear success/failure UX
- users can request another confirmation email
- forgot-password works without account enumeration
- password reset link works and enforces token expiry
- password reset success notification is sent
- production email provider configuration is documented and validated
- sender domain DNS requirements are documented and completed
- delivery failures are visible to admins/operators
- email-triggering endpoints are rate-limited
- no token or full recovery URL is logged
- automated tests cover the main account email flows
- manual validation has been completed in staging

---

## 16. Related Documents

- `04-Implementation-Backlog.md`
- `38-Web-Platform-Architecture.md`
- `57-Web-Auth-Validation-Worksheet.md`
- `58-Web-Deployment-Runbook.md`
- `61-Web-Release-Checklist.md`
- `62-Web-Accessibility-Checklist.md`
- `65-Safety-And-Moderation-Requirements.md`
- `71-Web-Test-Backlog.md`

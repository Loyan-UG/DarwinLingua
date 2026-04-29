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

- [ ] define the full list of transactional email scenarios for the first public release
- [ ] define account-state UX for unconfirmed email users
- [ ] define clear resend-confirmation behavior
- [ ] define expired-token UX and recovery path
- [ ] define invalid-token UX without leaking account existence
- [ ] define success pages for confirmation, password reset, and email change
- [ ] define user-facing copy for all email-related screens
- [ ] define support fallback text for users who cannot receive email
- [ ] define anti-enumeration wording for password reset and confirmation resend
- [ ] define what actions require confirmed email
- [ ] define whether browsing is allowed before confirmation
- [ ] define whether premium purchase, organizer claim, RSVP, matching, and profile publishing require confirmed email

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

- [ ] registration email confirmation
- [ ] resend email confirmation
- [ ] password reset request
- [ ] password reset success notification
- [ ] email address change confirmation for the new address
- [ ] email address changed notification to the old address
- [ ] account locked or suspicious login notification if lockout is enabled
- [ ] account deleted/deactivated confirmation if account deletion exists

### Security and Trust Emails

- [ ] password changed notification
- [ ] new login from unknown device or location placeholder if device tracking is later implemented
- [ ] two-factor authentication setup placeholder if 2FA is later implemented
- [ ] recovery code regeneration placeholder if 2FA is later implemented

### Organizer and Community Emails

- [ ] organizer claim submitted notification to the claimant
- [ ] organizer claim approved notification
- [ ] organizer claim rejected notification
- [ ] organizer profile ownership changed notification
- [ ] event RSVP confirmation notification if RSVP is enabled
- [ ] event cancellation/update notification if event management is enabled
- [ ] partner request received notification if matching is enabled
- [ ] partner request accepted notification if matching is enabled
- [ ] moderation/report outcome notification where appropriate

### Admin/Operator Emails

- [ ] new organizer claim admin notification
- [ ] high-severity user report admin notification
- [ ] repeated email delivery failure admin notification
- [ ] publish/import failure admin notification placeholder if useful
- [ ] payment/subscription failure admin notification placeholder if payment is added later

---

## 3. Architecture Requirements

### Goals

Transactional email must be provider-independent and testable. Do not couple business workflows directly to SMTP or a specific vendor SDK.

### Required Abstractions

- [ ] define `IEmailSender` or equivalent application abstraction
- [ ] define `IEmailTemplateRenderer` or equivalent template abstraction
- [ ] define `IEmailMessageFactory` or equivalent scenario-specific message factory
- [ ] define `IEmailDeliveryLogRepository` or equivalent persistence boundary
- [ ] define email provider options and validation
- [ ] define environment-specific behavior for development, staging, and production
- [ ] define background sending boundary if synchronous sending becomes unreliable
- [ ] define retry policy and non-retryable failure handling
- [ ] define cancellation-token support for all email operations
- [ ] define correlation/request id storage for diagnostics

### Provider Strategy

The first implementation should support one production provider and one local development provider.

| Environment | Recommendation |
|---|---|
| Development | log-to-file/in-memory sink or local SMTP catcher such as Mailpit/MailHog |
| Staging | real provider with test domain/subdomain |
| Production | provider with verified domain, SPF, DKIM, DMARC, bounce handling |

The implementation must not hard-code provider-specific logic into controllers, Razor pages, handlers, or domain entities.

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
- status: queued, sent, failed, skipped, suppressed
- failure code
- failure message summary
- retry count
- created at UTC
- sent at UTC
- last attempt at UTC
- correlation id

### Backlog

- [ ] define email delivery status enum
- [ ] define email scenario keys
- [ ] define email delivery log entity
- [ ] define EF Core mapping and migration
- [ ] define retention policy for delivery logs
- [ ] avoid storing sensitive token values in logs
- [ ] avoid logging full reset/confirmation URLs
- [ ] define cleanup job or manual cleanup endpoint for old logs
- [ ] add admin diagnostics page for recent email delivery status

---

## 5. Token and Security Requirements

### Goals

Email tokens are security-sensitive. They must be short-lived, single-purpose, and validated through ASP.NET Core Identity where possible.

### Backlog

- [ ] use ASP.NET Core Identity token providers for confirmation and password reset unless a stronger reason exists
- [ ] configure token lifetimes explicitly
- [ ] define token lifetime for email confirmation
- [ ] define token lifetime for password reset
- [ ] define token lifetime for email change
- [ ] ensure tokens are URL-safe and encoded correctly
- [ ] ensure reset/confirmation URLs are generated from configured public base URL, not from unsafe request headers alone
- [ ] protect against user enumeration in reset and resend flows
- [ ] rate-limit password reset requests
- [ ] rate-limit confirmation resend requests
- [ ] invalidate or make old flows harmless after email/password changes where applicable
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

- [ ] define template keys for every transactional scenario
- [ ] implement localized subject lines
- [ ] implement localized plain-text bodies
- [ ] implement localized HTML bodies if HTML email is used
- [ ] keep templates outside controllers and handlers
- [ ] include product name and support contact
- [ ] include clear expiration text for token emails
- [ ] include fallback copyable URL when using button-style HTML emails
- [ ] avoid unnecessary personal data in email bodies
- [ ] test missing-localization fallback behavior
- [ ] add template rendering tests for English and German

### Minimum Templates For Release

- [ ] `Account.EmailConfirmation`
- [ ] `Account.PasswordReset`
- [ ] `Account.PasswordResetCompleted`
- [ ] `Account.EmailChangeConfirmation`
- [ ] `Account.EmailChangedNotification`
- [ ] `Organizer.ClaimSubmitted`
- [ ] `Organizer.ClaimApproved`
- [ ] `Organizer.ClaimRejected`
- [ ] `Admin.NewOrganizerClaim`
- [ ] `Moderation.HighSeverityReport`

---

## 7. Web Account Flow Backlog

### Registration and Confirmation

- [ ] send confirmation email immediately after successful registration
- [ ] show a `Check your email` page after registration
- [ ] add `Resend confirmation email` page/action
- [ ] add confirmation callback endpoint/page
- [ ] show success page after valid confirmation
- [ ] show expired/invalid token page with resend option
- [ ] block or limit protected features for unconfirmed users according to product policy
- [ ] add tests for confirmed and unconfirmed login behavior

### Password Reset

- [ ] add `Forgot password` page
- [ ] add password reset request handler
- [ ] send password reset email with neutral success response
- [ ] add reset password page
- [ ] validate reset token and update password
- [ ] send password reset success notification
- [ ] show success page after reset
- [ ] add tests for non-existing email, invalid token, expired token, and successful reset

### Email Change

- [ ] add change-email screen if account profile editing exists
- [ ] send confirmation to new email address
- [ ] notify old email address after successful change
- [ ] require re-authentication for email change if necessary
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

- [ ] add admin page for recent transactional email deliveries
- [ ] filter by status, scenario, date range, and recipient/user where privacy policy allows
- [ ] show provider message id when available
- [ ] show safe failure summary without leaking tokens or sensitive URLs
- [ ] allow manual resend for safe scenarios such as confirmation email
- [ ] do not allow manual resend of expired reset tokens; generate a new token instead

### Operational Configuration

- [ ] add production email provider settings
- [ ] add staging email provider settings
- [ ] add local development email sink settings
- [ ] validate required email settings at startup in production
- [ ] document DNS requirements: SPF, DKIM, DMARC, bounce handling
- [ ] document sender identity: from address, reply-to address, support address
- [ ] document provider limits and cost assumptions

---

## 10. UX and Copy Backlog

### Required Pages

- [ ] registration success / check email
- [ ] confirmation success
- [ ] confirmation failed or expired
- [ ] resend confirmation
- [ ] forgot password
- [ ] reset password
- [ ] reset password success
- [ ] change email confirmation sent
- [ ] email changed success
- [ ] unconfirmed account gate page

### UX Rules

- [ ] do not reveal whether an email exists during password reset
- [ ] do not show raw token errors to users
- [ ] provide clear next step after every account email action
- [ ] include support fallback text only where useful
- [ ] keep wording short and localized
- [ ] keep error states consistent with existing UI components

---

## 11. Testing Backlog

### Unit Tests

- [ ] email message factory creates expected scenario keys
- [ ] template renderer returns localized English and German output
- [ ] missing template or missing localization fails safely
- [ ] provider options validation catches missing production settings

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

- [ ] no reset or confirmation token is logged
- [ ] no full magic link is logged
- [ ] reset and resend endpoints are rate-limited
- [ ] password reset response is account-enumeration safe
- [ ] email confirmation resend response is account-enumeration safe where applicable
- [ ] email delivery logs have a retention policy
- [ ] unsubscribe links are not required for transactional security emails, but marketing emails must not be mixed into this channel
- [ ] privacy policy mentions transactional emails and provider processing where required
- [ ] data processing agreement with production email provider is reviewed before launch
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

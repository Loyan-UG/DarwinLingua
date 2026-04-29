# Web Auth Validation Worksheet

## Purpose

This worksheet validates the `ASP.NET Core Identity` rollout in `DarwinLingua.Web`.

Use it once registration, sign-in, sign-out, email confirmation, password reset, email change, and role-based admin authorization are implemented.

Do not mark the related backlog items complete until this worksheet has been executed on the deployed host.

Transactional email validation is release-critical. See `73-Transactional-Email-And-Account-Communication-Backlog.md`.

---

## Build Under Test

- Build commit:
- Validation date:
- Validator:
- Environment:
- Browser:
- Identity store:
- Email provider/sink:
- Public base URL used in emails:

---

## Preconditions

- [ ] web host starts successfully
- [ ] database migrations have been applied
- [ ] identity tables exist
- [ ] transactional email settings are configured for the active environment
- [ ] public base URL for email links is configured
- [ ] sender address and reply-to address are configured
- [ ] development/staging email sink or provider inbox is accessible
- [ ] transactional email delivery log is accessible to admins/operators
- [ ] one of these seed-secret sources is configured for the active non-production environment:
- [ ] `IdentityBootstrap:*` values in local configuration
- [ ] `DARWINLINGUA_IDENTITY_SEED_ADMIN_EMAIL`
- [ ] `DARWINLINGUA_IDENTITY_SEED_ADMIN_PASSWORD`
- [ ] `DARWINLINGUA_IDENTITY_SEED_LEARNER_EMAIL`
- [ ] `DARWINLINGUA_IDENTITY_SEED_LEARNER_PASSWORD`
- [ ] test learner account is available
- [ ] test operator/admin account is available
- [ ] seeded learner test account is available
- [ ] seeded system admin account is available

---

## A. Registration And Email Confirmation

- [ ] create a new learner account
- [ ] verify validation messages render correctly
- [ ] verify duplicate-email behavior
- [ ] verify the new account receives the expected default role
- [ ] verify the new learner receives the expected initial entitlement state
- [ ] verify confirmation email is sent immediately after registration
- [ ] verify confirmation email uses the correct culture and template
- [ ] verify confirmation link uses the configured public base URL
- [ ] confirm the account using the email link
- [ ] verify confirmation success page renders correctly
- [ ] verify confirmed-only flows behave according to product policy
- Result:
- Notes:

---

## B. Resend Confirmation

- [ ] create or use an unconfirmed learner account
- [ ] request another confirmation email
- [ ] verify response text does not expose sensitive account state unnecessarily
- [ ] verify a new confirmation email is sent
- [ ] verify old/expired/invalid confirmation links fail safely
- [ ] verify resend is rate-limited
- Result:
- Notes:

---

## C. Sign-In And Sign-Out

- [ ] sign in with the confirmed learner account
- [ ] sign out
- [ ] sign in with the operator/admin account
- [ ] verify unauthenticated access redirects correctly where required
- [ ] verify unconfirmed-account behavior matches product policy
- Result:
- Notes:

---

## D. Password Reset

- [ ] request password reset for an existing learner email
- [ ] request password reset for a non-existing email and verify the same neutral response style
- [ ] verify reset email is sent only where appropriate
- [ ] verify reset email uses the correct culture and template
- [ ] verify reset link uses the configured public base URL
- [ ] complete reset using the email token flow
- [ ] sign in with the updated password
- [ ] verify password reset success notification is sent
- [ ] verify expired token fails safely
- [ ] verify invalid token fails safely
- [ ] verify malformed token fails safely
- [ ] verify reset request is rate-limited
- Result:
- Notes:

---

## E. Email Change

- [ ] open account settings
- [ ] request email address change where this feature is enabled
- [ ] verify confirmation email is sent to the new email address
- [ ] verify old email address is notified after successful change
- [ ] verify invalid or expired email-change token fails safely
- [ ] verify session behavior after email change
- Result:
- Notes:

---

## F. Authorization Boundaries

- [ ] verify learner account cannot access admin routes
- [ ] verify operator/admin account can access admin routes
- [ ] verify role-protected actions return the expected response when forbidden
- [ ] verify seeded learner test account cannot reach admin routes
- [ ] verify seeded system admin account can reach `Admin` routes immediately after bootstrap
- Result:
- Notes:

---

## G. Account Management

- [ ] open account settings
- [ ] verify profile and language preferences persist correctly
- [ ] verify session remains stable after profile changes
- [ ] verify account bootstrap does not overwrite guest learner state unexpectedly
- [ ] verify premium-gated features render the expected locked or unlocked state
- Result:
- Notes:

---

## H. Email Diagnostics And Operations

- [ ] verify delivery log captures confirmation email delivery attempt
- [ ] verify delivery log captures password reset delivery attempt
- [ ] verify failed delivery is visible without exposing tokens or full recovery links
- [ ] verify admin/operator can filter recent email deliveries where implemented
- [ ] verify no confirmation/reset token is logged
- [ ] verify no full confirmation/reset URL is logged
- Result:
- Notes:

---

## I. Cross-Platform Readiness

- [ ] verify the chosen identity boundary is usable by both web and mobile clients
- [ ] verify the non-production learner test account can authenticate from both delivery surfaces once mobile auth is wired
- [ ] verify confirmation and reset links have a web fallback even if mobile deep links are added later
- [ ] verify entitlement checks are enforced server-side rather than only in the web UI
- Result:
- Notes:

---

## J. Sign-Off

- Known issues accepted:
- Follow-up tasks filed:
- Final auth-flow readiness recommendation:

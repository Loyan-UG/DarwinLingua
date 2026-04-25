# Web Auth Validation Worksheet

## Purpose

This worksheet is reserved for the future `ASP.NET Core Identity` rollout in `DarwinLingua.Web`.

Use it once registration, sign-in, sign-out, password reset, and role-based admin authorization are implemented.

Do not mark the related backlog items complete until this worksheet has been executed on the deployed host.

---

## Build Under Test

- Build commit:
- Validation date:
- Validator:
- Environment:
- Browser:
- Identity store:

---

## Preconditions

- [ ] web host starts successfully
- [ ] database migrations have been applied
- [ ] identity tables exist
- [ ] test learner account is available
- [ ] test operator/admin account is available
- [ ] seeded learner test account is available
- [ ] seeded system admin account is available
- [ ] email/reset-token flow is configured or replaced with the intended non-production process

---

## A. Registration

- [ ] create a new learner account
- [ ] verify validation messages render correctly
- [ ] verify duplicate-email behavior
- [ ] verify the new account receives the expected default role
- [ ] verify the new learner receives the expected initial entitlement state
- Result:
- Notes:

---

## B. Sign-In And Sign-Out

- [ ] sign in with the learner account
- [ ] sign out
- [ ] sign in with the operator/admin account
- [ ] verify unauthenticated access redirects correctly where required
- Result:
- Notes:

---

## C. Password Reset

- [ ] request password reset
- [ ] complete reset using the intended token flow
- [ ] sign in with the updated password
- Result:
- Notes:

---

## D. Authorization Boundaries

- [ ] verify learner account cannot access admin routes
- [ ] verify operator/admin account can access admin routes
- [ ] verify role-protected actions return the expected response when forbidden
- [ ] verify seeded learner test account cannot reach admin routes
- [ ] verify seeded system admin account can reach `Admin` routes immediately after bootstrap
- Result:
- Notes:

---

## E. Account Management

- [ ] open account settings
- [ ] verify profile and language preferences persist correctly
- [ ] verify session remains stable after profile changes
- [ ] verify account bootstrap does not overwrite guest learner state unexpectedly
- [ ] verify premium-gated features render the expected locked or unlocked state
- Result:
- Notes:

---

## F. Cross-Platform Readiness

- [ ] verify the chosen identity boundary is usable by both web and mobile clients
- [ ] verify the non-production learner test account can authenticate from both delivery surfaces once mobile auth is wired
- [ ] verify entitlement checks are enforced server-side rather than only in the web UI
- Result:
- Notes:

---

## G. Sign-Off

- Known issues accepted:
- Follow-up tasks filed:
- Final auth-flow readiness recommendation:

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
- [ ] email/reset-token flow is configured or replaced with the intended non-production process

---

## A. Registration

- [ ] create a new learner account
- [ ] verify validation messages render correctly
- [ ] verify duplicate-email behavior
- [ ] verify the new account receives the expected default role
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
- Result:
- Notes:

---

## E. Account Management

- [ ] open account settings
- [ ] verify profile and language preferences persist correctly
- [ ] verify session remains stable after profile changes
- Result:
- Notes:

---

## F. Sign-Off

- Known issues accepted:
- Follow-up tasks filed:
- Final auth-flow readiness recommendation:

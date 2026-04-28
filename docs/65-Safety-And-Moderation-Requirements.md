# Safety And Moderation Requirements

## Purpose

This document defines the minimum safety, privacy, and moderation requirements for Phase 6 social, event, and organizer features.

It applies before implementing or releasing:

- public learner profiles
- partner requests
- organizer self-service profiles
- public event listings
- RSVP and attendee surfaces
- user reports and blocks

Use it together with:

- `02-Product-Scope.md`
- `03-Product-Phases.md`
- `04-Implementation-Backlog.md`
- `22-Domain-Model.md`
- `26-Bounded-Contexts.md`
- `63-Market-Product-And-Organizer-Strategy.md`

---

## 1. Release Gate

No public matching, public profile discovery, organizer self-service publishing, or user-generated public listing should ship until these controls exist:

- report action
- block action
- moderation queue
- admin review workflow
- audit trail for moderation decisions
- profile and listing visibility controls
- privacy review for exposed contact, city, and attendee data

Unrestricted user-to-user chat is outside the first social MVP.

---

## 2. Learner Profile Safety

Learner profiles must default to private or limited visibility.

Required controls:

- user-controlled display name
- optional city or region display
- no exact address display
- no public email or phone display
- explicit profile visibility setting
- explicit consent before contact reveal
- report profile action
- block user action

Profile data should stay minimal. The product should not collect sensitive personal details unless a later requirement justifies them and a privacy review approves them.

---

## 3. Partner Request Safety

Partner matching should use request-based interaction only.

Required rules:

- requests have states: pending, accepted, declined, cancelled, expired, blocked
- users can decline without explanation
- users can block requesters
- blocked users cannot send new requests
- requests expire automatically
- rate limits apply per sender
- contact details are hidden until acceptance and explicit reveal rules allow them

The first MVP should use predefined or constrained request text rather than free-form messaging.

---

## 4. Event And Organizer Safety

Organizer profiles and event listings require controlled publication.

Required controls:

- organizer verification state
- listing publication state
- listing review before public visibility where risk is non-trivial
- report organizer action
- report event action
- admin ability to hide or archive unsafe listings
- clear distinction between platform-owned listings and organizer-submitted listings

Event listings must not expose private attendee data publicly.

---

## 5. Moderation Workflow

The moderation workflow must support:

- report intake
- target type classification
- severity classification
- queue review
- decision recording
- action execution
- audit history

Initial moderation actions:

- no action
- hide listing
- archive listing
- suspend organizer publishing
- hide learner profile
- restrict partner requests
- dismiss duplicate report

Every moderation decision should record actor, target, reason, action, and timestamp.

---

## 6. Blocking Rules

Blocking must affect all matching and profile interaction surfaces.

Minimum behavior:

- blocked users cannot send partner requests to the blocker
- existing pending requests between the users are cancelled or hidden
- blocked profiles should not appear in matching results for either side where practical
- reports and blocks remain separate records

Blocking does not automatically prove abuse. It is a user safety control.

---

## 7. Privacy Rules

Privacy-sensitive data includes:

- real name
- exact address
- email
- phone number
- attendee list
- contact reveal decisions
- moderation reports
- block relationships

Required rules:

- collect the minimum data needed for the feature
- avoid public exact location for learners
- keep report and block data private
- limit admin access to operational need
- avoid exposing attendee lists publicly
- document any future analytics that use social or event data

---

## 8. Analytics Boundary

Social and organizer analytics must be aggregate-first.

Allowed early metrics:

- event views
- RSVP counts
- organizer profile views
- report counts
- block counts
- request counts by state

Avoid collecting or exposing raw message-like content, private contact data, or individual-level behavioral profiles unless a later privacy review approves it.

---

## 9. Implementation Order

Recommended order:

1. moderation domain contracts
2. report and block persistence
3. admin moderation queue
4. learner profile visibility controls
5. partner request flow
6. organizer verification and listing review
7. public event and profile surfaces

This order keeps the safety foundation ahead of public social features.

---

## 10. Moderation Runbook

This runbook is for the first Web moderation surface. It assumes reports, blocks, moderation decisions, and decision audits are available from Web Admin.

### Queue Triage

1. Open Web Admin > Moderation.
2. Review new reports before previously reviewed reports.
3. Check the target type and target key before reading free-text details.
4. Do not copy report details into public notes, support replies, analytics, or issue trackers unless a separate privacy review approves the destination.
5. Treat duplicate reports as supporting context, not as automatic proof of abuse.

### Immediate Safety Actions

Use a fast action when the report suggests harassment, unsafe contact reveal, impersonation, spam, or a listing that could mislead learners:

- hide learner profile
- restrict partner requests
- hide listing
- archive listing
- suspend organizer publishing

When evidence is unclear, prefer a reversible visibility restriction while an operator reviews source pages, profile history, partner-request state, and prior reports.

### Blocks

User blocks are user safety controls, not moderation decisions by themselves.

Operational rules:

- keep blocks private
- do not notify the blocked learner from the MVP workflow
- do not reveal blocker identity through admin-facing notes copied into user-facing messages
- check whether existing pending partner requests should be cancelled or suppressed
- escalate repeated blocks against the same account to manual moderation review

### Event And Organizer Reports

For event and organizer reports:

1. Open the public source URL, if available.
2. Compare event name, organizer, schedule, price, location, and verification timestamp.
3. If the listing is stale, misleading, or unverifiable, set the listing to hidden or archive it.
4. If the source is valid but schedule details changed, update the listing and `LastVerifiedAtUtc`.
5. If organizer ownership is disputed, pause organizer self-service edits until the claim is reviewed.

### Decision Logging

Every decision should record:

- decision action
- target type and target key
- operator actor
- reason category
- short internal rationale
- timestamp

Do not store unnecessary personal data in the rationale. Use target keys and report identifiers instead of copying email addresses or free-text report content.

### Escalation

Escalate to a project owner before:

- permanently deleting moderation records
- publishing public explanations about a user or organizer
- changing access to an organizer profile with disputed ownership
- exporting report, block, RSVP, or contact data
- adding any direct messaging or public-comment feature

---

## 11. Privacy Review Checklist

Before releasing public learner profiles, partner matching, RSVP, organizer listings, or analytics beyond aggregate counters, complete this review.

### Data Inventory

Confirm the feature inventory for:

- learner display name
- city or region
- learner goals and interests
- email or contact reveal state
- partner request note
- report details
- block relationships
- RSVP participant name and email
- organizer public contact method
- event source URLs and verification dates
- analytics counters

### Public Exposure Review

The public Web surface must not expose:

- learner email before accepted partner-request consent
- exact learner address
- private profile text for disabled, private, or request-only profiles
- attendee names or attendee emails
- report details
- block relationships
- moderation rationale

### Admin Exposure Review

Admin views should expose only operationally necessary data.

Required checks:

- report details are visible only to Admin/Operator roles
- RSVP attendee data is visible only where organizer RSVP management is entitled and authorized
- analytics remains aggregate-only
- moderation audit history does not include avoidable copied personal data
- source verification metadata is visible for event/listing review

### Retention And Removal

Required checks:

- learner profile anonymization clears public profile fields
- disabling a learner profile removes it from public discovery
- blocks and moderation decisions remain operational records unless a project owner approves deletion
- event listings can be hidden or archived without deleting source metadata
- exported files are not part of the MVP unless a separate operational need is approved

### Release Decision

The release reviewer should mark one of:

- approved for private/internal pilot
- approved for public MVP
- blocked pending fixes

Record the decision date, reviewer, scope, and any required follow-up in the release checklist.

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

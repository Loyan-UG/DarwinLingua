# Conversation and Organizer Implementation Backlog

## Purpose

This document is the executable backlog for the practical conversation, learner-profile, event-directory, and B2B organizer direction.

It expands the product beyond vocabulary and review into:

- scenario-based practical German learning
- conversation starters and roleplay preparation
- multilingual and dual-meaning-language learning flows
- local conversation clubs, cafes, and events
- organizer profiles and B2B organizer tooling
- safe learner profiles and request-based conversation matching
- premium and organizer entitlement boundaries
- moderation and safety workflows

Use this document together with:

- `04-Implementation-Backlog.md`
- `63-Market-Product-And-Organizer-Strategy.md`
- `52-Future-Platform-Expansion.md`
- `38-Web-Platform-Architecture.md`

---

## Status Tracking Rule

Use these markers consistently:

- `[ ]` not started
- `[-]` in progress
- `[x]` completed
- `[!]` blocked / needs decision

Do not delete completed items. Mark them as completed so the implementation history remains visible.

---

## Target Outcome

This backlog is complete when Darwin Lingua supports a safe and useful practical conversation layer:

- learners can prepare for real German situations using scenario lessons
- learners can see German content with one or two meaning languages at the same time
- learners can use conversation starters for specific contexts
- learners can find local and online conversation events
- organizers can publish and manage language-practice events
- learners can prepare for events using event-specific preparation packs
- safe profile and partner-request flows exist without unrestricted open chat in the MVP
- moderation, report, block, and organizer verification workflows exist
- premium learner features and organizer plan flags are technically possible

---

## 1. Product and Documentation Foundation

- [x] document market, product, and organizer strategy
- [x] document the conversation and organizer implementation backlog
- [ ] update product phases to explicitly include scenario learning, event discovery, organizer tooling, and safe partner matching
- [ ] update product scope with MVP/non-MVP boundaries for the social and organizer layer
- [ ] update domain model documentation with planned `Scenario`, `Event`, `Organizer`, `Profile`, `PartnerRequest`, and `Moderation` concepts
- [ ] update bounded-context documentation with `Scenarios`, `Events`, `Organizers`, `Profiles`, `Matching`, and `Moderation`
- [ ] update monetization documentation with learner and organizer entitlement boundaries
- [ ] add a safety and moderation requirements document before implementing public profiles or matching

---

## 2. Dual Meaning-Language Learning

### Goal

Make dual meaning-language learning a first-class product feature.

Examples:

- German + English + Persian
- German + English + Arabic
- German + English + Turkish
- German + English + Ukrainian
- German + English + Russian

### Backlog

- [ ] review the existing meaning-language preference model and confirm it supports exactly one or two active meaning languages cleanly
- [ ] define UI rules for primary and secondary meaning languages
- [ ] add compact and expanded translation display modes where needed
- [ ] ensure word details, examples, scenario lessons, and conversation starters can display two meaning languages consistently
- [ ] add validation rules for missing translations in one of the selected meaning languages
- [ ] add fallback rules when the secondary meaning language is unavailable
- [ ] add tests for dual-language selection and rendering decisions
- [ ] add content-quality checks that identify incomplete dual-language coverage

---

## 3. Scenario-Based Learning

### Goal

Add practical real-life scenario lessons that help learners use German in concrete situations.

### Initial Scenario Categories

- doctor and healthcare
- school and kindergarten
- workplace and colleagues
- job interviews
- government offices and appointments
- housing and landlords
- shopping and services
- transport and travel
- social small talk
- conversation cafes and first meetings

### Scenario Content Model

Each scenario should support:

- title
- description
- learner goal
- CEFR range
- topic/category
- context type: formal, informal, phone, office, workplace, social
- supported meaning languages
- dialogue turns
- key vocabulary
- likely questions
- useful answers
- politeness notes
- common mistakes
- roleplay prompts
- practice links

### Backlog

- [ ] define `ScenarioLesson` content contract
- [ ] define `ScenarioDialogueTurn` content contract
- [ ] define `ScenarioPhrase` or `UsefulPhrase` content contract
- [ ] define `ScenarioQuestion` and `ScenarioAnswer` content contract
- [ ] define scenario CEFR-level and topic mapping rules
- [ ] define import validation rules for scenario content
- [ ] implement scenario import pipeline support
- [ ] persist scenario lessons in the shared content model
- [ ] expose scenario lessons through application queries
- [ ] add scenario list and detail screens in MAUI
- [ ] add scenario list and detail pages in the web app when appropriate
- [ ] add tests for scenario import, persistence, query, and rendering
- [ ] add initial sample scenarios for A1/A2 learners

---

## 4. Conversation Starters and Topic Packs

### Goal

Help learners start and continue real conversations.

### Conversation Starter Dimensions

- situation: work, cafe, class, neighbor, school, event, online meeting
- CEFR level: A1, A2, B1, B2
- tone: formal, friendly, very simple, casual
- goal: introduction, asking questions, continuing conversation, ending politely, arranging next contact
- meaning languages: selected one or two languages

### Backlog

- [ ] define `ConversationStarterPack` content contract
- [ ] define `ConversationStarterPhrase` content contract
- [ ] define starter categories and filters
- [ ] support starter packs inside scenario lessons
- [ ] support standalone starter packs
- [ ] add mobile browsing and detail UI for starter packs
- [ ] connect starter packs to event preparation packs
- [ ] add tests for starter filtering and dual-language rendering
- [ ] add sample starter packs for conversation cafes, workplace small talk, and first meetings

---

## 5. Roleplay Preparation

### Goal

Add structured roleplay practice without requiring AI in the first implementation.

### MVP Direction

Start with scripted roleplay flows:

- system displays the other person's line
- learner selects or types a response later
- system gives static feedback or shows model answers
- learner can replay the roleplay

AI-assisted feedback can be added later behind a premium or pro tier.

### Backlog

- [ ] define `RoleplayScenario` content contract
- [ ] define role labels such as learner, doctor, teacher, colleague, organizer, partner
- [ ] define scripted turn sequence model
- [ ] define answer-choice model for early MVP
- [ ] define static feedback model
- [ ] add roleplay launch from scenario detail
- [ ] persist basic roleplay attempts if useful
- [ ] add tests for roleplay sequence behavior
- [ ] keep AI roleplay feedback out of MVP unless cost and safety boundaries are decided

---

## 6. Event and Club Directory

### Goal

Let learners find conversation cafes, clubs, online meetings, language events, and support resources.

### MVP Directory Fields

- name
- description
- city or online flag
- country/region
- approximate location
- event or resource category
- supported learner levels
- supported helper languages
- organizer name
- external link
- email or contact method if public
- schedule text
- price type: free, donation, paid, unknown
- verification status
- source/update metadata

### Backlog

- [ ] define `ResourceDirectory` and `ConversationEvent` boundaries
- [ ] define event category list
- [ ] define city/location model without exposing precise private user locations
- [ ] implement read-only event directory model
- [ ] implement event list query with filters by city, level, language, online/offline, and price type
- [ ] implement event detail query
- [ ] add mobile event directory screens
- [ ] add web event directory pages
- [ ] support external registration links
- [ ] support manual admin-managed event creation first
- [ ] add validation for stale event data
- [ ] add tests for filtering and visibility rules

---

## 7. Event Preparation Packs

### Goal

Differentiate Darwin Lingua from generic event platforms by helping learners prepare before attending.

### Preparation Pack Content

Each event can have:

- useful words
- opening sentences
- fallback phrases
- short dialogues
- likely questions
- topic suggestions
- roleplay exercises
- post-event review prompts

### Backlog

- [ ] define `EventPreparationPack` content model
- [ ] allow preparation packs to link to existing scenario lessons
- [ ] allow preparation packs to link to vocabulary entries
- [ ] allow preparation packs to link to conversation starter packs
- [ ] show preparation packs on event detail screens
- [ ] add `Prepare for this event` action
- [ ] track preparation-pack completion if useful
- [ ] add tests for event-to-pack mapping

---

## 8. Organizer Profiles

### Goal

Create public pages for clubs, teachers, cafes, organizations, and other language-practice organizers.

### Organizer Profile Fields

- organizer name
- organization type
- description
- city/region
- online availability
- supported levels
- supported helper languages
- website
- public contact method
- verified status
- active events
- historical event count
- optional logo/image later

### Backlog

- [ ] define `OrganizerProfile` domain model
- [ ] define organizer type list: teacher, cafe, club, association, school, company, library, other
- [ ] implement read-only organizer profile pages
- [ ] link events to organizer profiles
- [ ] add admin-managed organizer creation first
- [ ] add organizer claim workflow placeholder
- [ ] add verified organizer badge rules
- [ ] add tests for organizer visibility and event linking

---

## 9. Organizer Dashboard

### Goal

Allow approved organizers to manage their own profile and events.

### MVP Dashboard Features

- edit organizer profile
- create event
- edit event
- disable event
- define recurring schedule text or simple recurrence rules
- define capacity
- view RSVP list when RSVP is implemented
- see basic analytics

### Backlog

- [ ] define organizer account ownership rules
- [ ] define organizer admin role or entitlement boundary
- [ ] implement organizer dashboard authorization
- [ ] implement profile edit workflow
- [ ] implement event create/edit workflow
- [ ] implement event activation/deactivation workflow
- [ ] implement recurring-event support after one-off events work
- [ ] implement basic organizer analytics
- [ ] add tests for authorization and ownership rules

---

## 10. RSVP and Attendance

### Goal

Let learners express interest or reserve a spot without building a full event-ticketing system.

### Backlog

- [ ] define `EventRsvp` model
- [ ] support RSVP states: interested, going, cancelled, attended, no-show
- [ ] define capacity and waitlist rules
- [ ] add `I am interested` or `Join` action on event detail
- [ ] show remaining capacity where appropriate
- [ ] expose attendee count without exposing private attendee details publicly
- [ ] show attendee list only to authorized organizers/admins
- [ ] add post-event attendance confirmation
- [ ] add tests for capacity, cancellation, and organizer visibility

---

## 11. Learner Profiles

### Goal

Create a minimal learner profile suitable for conversation practice and event participation.

### MVP Profile Fields

- display name
- city or region
- online/in-person preference
- German level
- selected helper languages
- conversation goals
- availability notes
- profile visibility
- age confirmation for social features

### Safety Boundaries

- no exact home address
- no public email or phone by default
- no full profile visibility for minors unless a separate safe model is designed
- allow user to hide or delete profile

### Backlog

- [ ] define learner profile model
- [ ] define profile visibility states
- [ ] define public profile projection separate from private profile data
- [ ] implement profile create/edit screen
- [ ] implement profile enable/disable toggle
- [ ] implement profile deletion or anonymization flow
- [ ] add tests for public/private projection
- [ ] add privacy review before release

---

## 12. Safe Partner Matching

### Goal

Let learners find conversation partners without creating an unsafe open social network.

### MVP Direction

Use request-based matching, not unrestricted open chat.

### Matching Filters

- city/region
- online/in-person
- German level
- helper languages
- native language
- learning goal
- availability

### Backlog

- [ ] define `PartnerRequest` model
- [ ] define match search query model
- [ ] define request states: pending, accepted, declined, cancelled, blocked, expired
- [ ] add rate limits for new users
- [ ] add predefined opener templates
- [ ] add accept/decline flow
- [ ] avoid unrestricted direct messaging in MVP
- [ ] reveal contact details only after mutual consent if this feature is added
- [ ] add tests for request-state transitions and rate limits

---

## 13. Moderation, Abuse Handling, and Trust

### Goal

Make social and organizer features safe enough to release publicly.

### Required Before Public Profiles or Matching

- report user
- block user
- hide profile
- admin report queue
- organizer verification workflow
- listing approval workflow
- rate limits
- audit logs for moderation decisions

### Backlog

- [ ] define `UserReport` model
- [ ] define `UserBlock` model
- [ ] define `OrganizerVerification` model
- [ ] define `ListingReview` workflow
- [ ] implement report action from profile, event, and organizer pages
- [ ] implement block action from profile and partner-request flows
- [ ] implement admin moderation queue
- [ ] implement moderation decision audit log
- [ ] add tests for block visibility and request suppression
- [ ] add operational runbook for moderation

---

## 14. Entitlements and Monetization

### Learner Plans

| Plan | Direction |
|---|---|
| Free | Useful core learning, basic directory, limited saves/requests |
| Plus | More saved phrases, more scenario packs, advanced dual-language layout, event preparation packs |
| Pro | Future AI-assisted practice, writing help, advanced roleplay, interview preparation |

### Organizer Plans

| Plan | Direction |
|---|---|
| Free Organizer | Limited public page and limited active events |
| Organizer Lite | More events, RSVP, basic analytics |
| Organizer Standard | Recurring events, attendee export, featured city placement, verified badge |
| Organizer Pro | Multiple admins, multiple locations, richer analytics, branded profile |

### Backlog

- [ ] define entitlement keys for scenario and conversation features
- [ ] define entitlement keys for organizer features
- [ ] keep core catalog browse and basic scenario access outside strict premium enforcement
- [ ] add feature gates for advanced scenario packs if needed
- [ ] add feature gates for preparation packs if needed
- [ ] add organizer plan flags without payment integration first
- [ ] add admin entitlement management for organizer plans
- [ ] add tests for feature-gate behavior

---

## 15. Analytics and KPIs

### MVP Metrics

- scenario views
- scenario completion
- conversation-starter usage
- saved phrases
- event views
- event RSVP rate
- preparation-pack usage
- organizer profile views
- partner request sent/accepted rate
- report/block rate
- premium feature usage

### Backlog

- [ ] define analytics event names and payloads
- [ ] avoid collecting unnecessary personal data
- [ ] implement privacy-safe analytics boundaries
- [ ] add admin summary views for organizer/event analytics
- [ ] add export only where operationally necessary

---

## 16. Content Seed Priorities

### First Scenario Packs

- [ ] First language-practice meeting
- [ ] Introducing yourself in simple German
- [ ] Asking someone to speak slowly
- [ ] Asking for correction politely
- [ ] Talking to a doctor
- [ ] Writing to kindergarten or school
- [ ] Calling an office for an appointment
- [ ] Talking to a landlord
- [ ] Workplace small talk
- [ ] Job interview basics

### First Conversation Starter Packs

- [ ] Conversation cafe starters
- [ ] Workplace lunch starters
- [ ] School/kindergarten parent starters
- [ ] Neighbor small talk starters
- [ ] Online practice meeting starters

### First Event Directory Seeds

- [ ] manually add a small set of public conversation cafes in selected cities
- [ ] manually add online German practice groups where allowed
- [ ] manually add integration-oriented support resources where appropriate
- [ ] define update/verification date per listing

---

## 17. Explicit Non-MVP Items

Do not implement these in the first release unless there is a specific decision to expand scope:

- unrestricted user-to-user chat
- ticket payments inside the app
- live audio or video calls
- dating-style matching
- public comments under profiles or events
- map-heavy real-time location tracking
- AI feedback without cost, safety, and privacy boundaries
- organizer self-service billing
- complex recommendation engine

---

## Recommended Execution Order

1. Product/documentation updates
2. Scenario content contract
3. Scenario import and query support
4. Scenario mobile UI
5. Conversation starter packs
6. Event and club directory
7. Event preparation packs
8. Organizer profiles
9. Basic organizer dashboard
10. RSVP
11. Learner profiles
12. Partner requests
13. Moderation and report/block flows
14. Entitlement flags
15. Analytics and KPI tracking

---

## Acceptance Criteria

The first public version of this expansion should be considered acceptable only when:

- scenario lessons work in at least German + one meaning language
- dual-language display works consistently where content exists
- at least 10 high-value practical scenarios exist
- event directory can be browsed and filtered
- event detail can show a preparation pack
- organizer profiles can be displayed
- no public matching feature is released without report/block/moderation controls
- no unrestricted chat exists in the MVP
- premium features do not block the core learning value
- all user-facing strings are localized through the existing localization system
- tests cover the main query, state-transition, and visibility rules

---

## Maintenance Rule

Before starting a new implementation slice:

- move selected items to `[-]`
- keep unrelated items unchanged
- mark completed work with `[x]`
- add new tasks in the correct section
- update `04-Implementation-Backlog.md` if the work changes the main project phase plan
- update `63-Market-Product-And-Organizer-Strategy.md` only when the strategy changes, not for every implementation detail

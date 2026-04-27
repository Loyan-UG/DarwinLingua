# Product Phases

## Phase 0 - Foundation and Validation

### Goals

- define product vision
- define content model
- define import format
- define architecture
- set up solution structure
- create initial sample dataset
- build basic import utility

### Deliverables

- product documentation
- domain model draft
- project structure draft
- import file specification
- sample content package
- initial admin/import utility concept

---

## Phase 1 - MVP Vocabulary Learning

### Goals

Build a useful vocabulary application with multilingual support and audio.

### Features

- browse by CEFR level
- browse by topic
- search words
- word detail screen
- multilingual meanings
- support for one or two user-selected meaning languages at a time from the seeded starter language set
- example sentences
- word audio
- sentence audio
- favorites
- UI localization
- local data storage
- import initial content into database

### Success Criteria

- user can discover words quickly
- user can understand meanings in selected languages from the seeded starter language catalog
- user can hear word and sentence pronunciation
- system can import content reliably
- basic app UX is stable on Android, Windows, and iOS

---

## Phase 2 - Practice and Review

### Goals

Turn content into a repeatable learning experience.

### Features

- flashcards
- quiz modes
- review sessions
- wrong-answer tracking
- spaced repetition
- recent activity
- lightweight progress tracking

### Success Criteria

- user can actively practice words
- app can prioritize difficult words
- user sees learning progress

---

## Phase 3 - Enhanced Content Intelligence

### Goals

Improve quality and usefulness of content.

### Features

- word families
- synonyms and antonyms
- collocations
- grammar notes
- separable verbs
- usage labels such as formal/informal
- context labels such as doctor/work/school

### Success Criteria

- word entries become richer
- learners get more practical usage support
- content becomes more context-aware

---

## Phase 4 - Migrant Support Module

### Goals

Extend beyond language learning into real-life support.

### Features

- language schools
- speaking cafés / dialogue cafés
- counseling centers
- useful local resources
- search by category
- search by city or area
- save useful places

### Success Criteria

- users can find local support information
- information is discoverable by topic and location
- this module remains separate from the core learning model

---

## Phase 5 - Web Platform and Management Tools

### Goals

Expand to a broader platform and turn shared content into a server-managed distribution model.

### Features

- central server database for shared content
- Web API for mobile content manifests and package downloads
- ASP.NET Core MVC website
- learner-facing root web experience
- `Admin` area inside the MVC website for operational workflows
- installable PWA-capable web shell
- Bootstrap-based responsive UI
- ASP.NET Core Identity-based registration and sign-in
- persisted user state for favorites, recent activity, and user-specific web preferences
- mobile content update controls for:
  - full database update
  - content-area update
  - CEFR-level word updates
- shared backend
- content admin panel
- user account and sync
- moderation/import review tools
- analytics
- optional monetization
- explicit planning boundaries for future admin and web products

### Success Criteria

- the team updates shared content in one central place
- mobile apps can refresh local SQLite content from the Web API without losing local user state
- full, area, and CEFR-slice updates are supported cleanly
- the website can be installed like an app where browser/platform support allows it
- learners can sign up and keep basic personal state on the web
- admin workflows remain clearly separated from learner-facing pages
- content can be managed efficiently
- code reuse remains strong
- the system scales without architectural redesign
- future admin/web/account/analytics/monetization work can start from explicit shared-boundary documents

---

## Phase 6 - Practical Conversation, Community, and Organizer Platform

### Goals

Turn Darwin Lingua into a practical German-in-Germany platform, not only a vocabulary and review app.

This phase adds scenario-based learning, conversation preparation, event discovery, safe learner profiles, and B2B organizer tooling.

### Features

- real-life scenario lessons for situations such as doctor visits, school, kindergarten, offices, housing, workplace, job interviews, and conversation cafes
- conversation starter packs by situation, CEFR level, tone, and goal
- dual meaning-language learning as a first-class product mode, such as:
  - German + English + Persian
  - German + English + Arabic
  - German + English + Turkish
  - German + English + Ukrainian
  - German + English + Russian
- scripted roleplay preparation without requiring AI in the first version
- local and online conversation event directory
- event preparation packs linked to vocabulary, scenarios, starter phrases, and roleplay prompts
- organizer profiles for conversation cafes, clubs, teachers, libraries, associations, and integration groups
- organizer dashboard for event/profile management
- RSVP and attendance basics without becoming a full event-ticketing platform
- minimal learner profiles for conversation practice
- request-based partner matching without unrestricted open chat in the MVP
- report, block, verification, listing review, and moderation workflows
- learner premium feature flags
- B2B organizer plan flags

### Success Criteria

- learners can prepare for real-life German conversations through structured scenario lessons
- learners can view German learning content with one or two selected meaning languages consistently
- learners can discover conversation events by city, level, language, online/offline mode, and price type
- event pages can show preparation packs that help users practice before attending
- organizers can have public profiles and publish/manage events through controlled workflows
- no public matching feature is released without report, block, moderation, and privacy controls
- unrestricted user-to-user chat is not part of the first social MVP
- monetization does not block the core learning value
- B2B organizer tooling can become a sustainable revenue path without compromising learner trust

### Reference Documents

- `63-Market-Product-And-Organizer-Strategy.md`
- `64-Conversation-And-Organizer-Implementation-Backlog.md`

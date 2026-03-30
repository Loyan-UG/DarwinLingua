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
- mobile content update controls for:
  - full database update
  - content-area update
  - CEFR-level word updates
- website
- shared backend
- content admin panel
- user account and sync
- moderation/import review tools
- analytics
- optional monetization

### Success Criteria

- the team updates shared content in one central place
- mobile apps can refresh local SQLite content from the Web API without losing local user state
- full, area, and CEFR-slice updates are supported cleanly
- content can be managed efficiently
- code reuse remains strong
- the system scales without architectural redesign

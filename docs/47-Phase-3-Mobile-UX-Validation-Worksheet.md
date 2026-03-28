# Phase 3 Mobile UX Validation Worksheet

## Purpose

This worksheet is the execution companion for the remaining manual Phase 3 mobile validation.

Use it on the target device matrix after the automated Phase 3 checks have passed.

Do not mark the related backlog item complete until this worksheet has been executed and the results are recorded.

---

## Build Under Test

- Build commit:
- Validation date:
- Validator:
- Device:
- OS version:
- App package/build identifier:
- Imported content package version:

---

## Device Matrix

Record one row per tested device and UI-language combination.

| Device | OS | UI Language | Meaning Language | Flow | Result | Notes |
| --- | --- | --- | --- | --- | --- | --- |
|  |  |  |  |  |  |  |

---

## Preconditions

- [ ] build is produced from the intended release commit
- [ ] automated checks already passed on the same commit
- [ ] sample content package has been imported successfully
- [ ] imported entries include usage/context labels, grammar notes, collocations, word families, and lexical relations
- [ ] at least one word with rich metadata can be opened from search or browse
- [ ] validation will be executed once in English UI and once in German UI

---

## A. Home And Browse Visual Consistency

### Setup

- [ ] open the Home screen
- [ ] open the Browse/Topics screen
- [ ] navigate between Home, Browse, Search, Favorites, and Practice

### Checks

- [ ] hero sections render without overlap or clipped text
- [ ] section headings create a clear visual hierarchy
- [ ] action cards use a consistent visual style
- [ ] topic list items match the visual tone of word list items
- [ ] spacing, card corners, and contrast feel consistent between Home and Browse
- [ ] no layout breakage appears on the tested screen size or orientation

### Result

- Home/Browse result:
- Home/Browse notes:

---

## B. Search Flow UX

### Setup

- [ ] open the Search screen
- [ ] run at least three searches: exact lemma, prefix match, and no-result query

### Checks

- [ ] search hero and search panel render correctly
- [ ] search hint text is visible and localized
- [ ] results panel remains readable with multiple items
- [ ] empty state renders cleanly for a no-result query
- [ ] selecting a result still opens the correct word-detail page
- [ ] repeated searches do not produce stale or duplicated results

### Result

- Search result:
- Search notes:

---

## C. Enhanced Word Detail UX

### Setup

- [ ] open a word that includes rich metadata
- [ ] open at least one word with minimal metadata for comparison

### Checks

- [ ] hero area renders article, lemma, CEFR, and actions cleanly
- [ ] usage/context chips wrap correctly on the tested device width
- [ ] grammar notes section is readable and visually distinct
- [ ] collocations render in a scannable format
- [ ] word-family section renders related forms clearly
- [ ] synonym and antonym sections render without clipping or empty-card artifacts
- [ ] example sentences and meanings remain easy to scan after the richer metadata sections
- [ ] learner-state and favorite actions still work after visiting richer entries

### Result

- Word detail result:
- Word detail notes:

---

## D. Practice Regression After UI Polish

### Setup

- [ ] open the Practice screen
- [ ] start at least one flashcard session
- [ ] start at least one quiz session

### Checks

- [ ] polished card styling does not reduce readability of metrics or activity items
- [ ] action sections still feel distinct and easy to use
- [ ] word list items in practice remain readable and tappable
- [ ] navigation from practice preview/activity items to word detail still works
- [ ] returning from sessions still refreshes overview data correctly

### Result

- Practice regression result:
- Practice regression notes:

---

## E. Localization And Offline Regression

### Setup

- [ ] validate once in English UI
- [ ] validate once in German UI
- [ ] disable network access after content import

### Checks

- [ ] all new section labels and status badges render in English
- [ ] all new section labels and status badges render in German
- [ ] no hard-coded English text remains in the polished mobile screens
- [ ] Home, Browse, Search, and Word Detail still open while offline
- [ ] rich metadata already imported remains visible while offline
- [ ] no network-dependent placeholder or failure state appears in these local-first flows

### Result

- Localization/offline result:
- Localization/offline notes:

---

## F. Sign-Off

- Remaining known issues accepted for release:
- Follow-up bugs filed:
- Final Phase 3 mobile-UX readiness recommendation:

Copy the accepted results into the release notes or sign-off record used for the build under test.

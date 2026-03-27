# Phase 2 Practice Validation Worksheet

## Purpose

This worksheet is the execution companion for the remaining manual Phase 2 practice-flow validation.

Use it on the actual target device matrix after the automated Phase 2 checks have passed.

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

| Device | OS | UI Language | Meaning Language | Practice Mode | Result | Notes |
| --- | --- | --- | --- | --- | --- | --- |
|  |  |  |  |  |  |  |

---

## Preconditions

- [ ] build is produced from the intended release commit
- [ ] automated Phase 2 checks already passed on the same commit
- [ ] sample content package is available on the validation machine
- [ ] sample content package has been imported successfully
- [ ] tracked words exist so the practice overview is not empty
- [ ] at least one due item exists before review-session validation begins

---

## A. Practice Entry And Overview

### Setup

- [ ] launch the app with imported sample content present
- [ ] open the Practice tab from shell navigation
- [ ] open the Practice entry point from the home screen

### Checks

- [ ] Practice tab title is visible and localized
- [ ] home-screen Practice entry opens the same overview screen
- [ ] overview metrics load without crash or empty-state regression
- [ ] review preview renders tracked words and meanings correctly
- [ ] recent activity section renders persisted attempts correctly
- [ ] overview refresh after returning from a session reflects new state

### Result

- Practice overview result:
- Practice overview notes:

---

## B. Flashcard Session Flow

### Setup

- [ ] start a flashcard session from the Practice overview

### Checks

- [ ] session opens with a bounded queue and visible progress position
- [ ] answer reveal works without navigation or state loss
- [ ] selecting `Incorrect` records feedback and advances correctly
- [ ] selecting `Hard` records feedback and advances correctly
- [ ] selecting `Correct` records feedback and advances correctly
- [ ] selecting `Easy` records feedback and advances correctly
- [ ] feedback text is localized and consistent with the selected answer
- [ ] finishing the queue shows a session summary state
- [ ] returning to Practice overview updates queue/activity/progress correctly

### Result

- Flashcard result:
- Flashcard notes / issues:

---

## C. Quiz Session Flow

### Setup

- [ ] start a quiz session from the Practice overview

### Checks

- [ ] quiz session opens without reusing flashcard-only UI text or controls
- [ ] question/answer interaction is understandable and localized
- [ ] selecting each answer outcome updates feedback correctly
- [ ] session advances through multiple items without duplicate or skipped cards
- [ ] session summary appears after the final item
- [ ] returning to Practice overview updates queue/activity/progress correctly

### Result

- Quiz result:
- Quiz notes / issues:

---

## D. Localization And Restart Regression

### Setup

- [ ] validate once in English UI
- [ ] validate once in German UI
- [ ] switch app language and revisit the Practice flows

### Checks

- [ ] Practice overview text renders in English
- [ ] flashcard session text renders in English
- [ ] quiz session text renders in English
- [ ] Practice overview text renders in German
- [ ] flashcard session text renders in German
- [ ] quiz session text renders in German
- [ ] app restart preserves Practice access and previously recorded attempts
- [ ] previously scheduled due items still appear correctly after restart

### Result

- Localization/restart result:
- Localization/restart notes:

---

## E. Offline Practice Behavior

### Setup

- [ ] disable network access on the device after content import
- [ ] reopen the app if needed

### Checks

- [ ] Practice overview opens successfully while offline
- [ ] flashcard session starts successfully while offline
- [ ] quiz session starts successfully while offline
- [ ] answer submission works while offline
- [ ] session summary works while offline
- [ ] overview refresh still reflects updated practice state while offline

### Result

- Offline Practice result:
- Offline Practice notes:

---

## F. Sign-Off

- Remaining known issues accepted for release:
- Follow-up bugs filed:
- Final Phase 2 practice-readiness recommendation:

Copy the accepted results into the release notes or release sign-off record used for the build under test.

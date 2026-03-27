# Phase 1 Manual Validation Worksheet

## Purpose

This worksheet is the execution companion for the remaining manual Phase 1 release-readiness checks.

Use it on the actual release device matrix.

Do not mark the related backlog items complete until this worksheet has been executed and the results are recorded.

---

## Build Under Test

- Build commit:
- Validation date:
- Validator:
- Device:
- OS version:
- App package/build identifier:
- Content package version:

---

## Device Matrix

Record one row per tested device.

| Device | OS | UI Language | German TTS Voice Present | Result | Notes |
| --- | --- | --- | --- | --- | --- |
|  |  |  |  |  |  |

---

## Preconditions

- [ ] build is produced from the intended release commit
- [ ] sample content package is available on the validation machine
- [ ] app data directory can be cleared before first-run validation
- [ ] validator has confirmed which device language and app language combination is under test

---

## A. Offline Behavior

### Setup

- [ ] clear local app data for a clean first run where required
- [ ] launch the app once with normal connectivity
- [ ] import the sample Phase 1 content package
- [ ] disable network access on the device

### Checks

- [ ] home page opens successfully while offline
- [ ] browse hub opens successfully while offline
- [ ] CEFR browse returns locally stored words while offline
- [ ] topic browse returns locally stored words while offline
- [ ] search returns locally stored words while offline
- [ ] word detail opens successfully while offline
- [ ] favorites still load while offline
- [ ] settings still open and save language preferences while offline

### Result

- Offline result:
- Offline notes / known issues:

---

## B. English UI

### Setup

- [ ] set UI language to English
- [ ] return to home page

### Checks

- [ ] shell tab titles render in English
- [ ] home dashboard text renders in English
- [ ] browse hub text renders in English
- [ ] search page labels and empty/error states render in English
- [ ] topic and CEFR browse labels render in English
- [ ] word detail actions and learning-state labels render in English
- [ ] favorites page labels render in English
- [ ] settings labels render in English
- [ ] lexical metadata labels render in English

### Result

- English UI result:
- English UI notes / copy issues:

---

## C. German UI

### Setup

- [ ] set UI language to German
- [ ] return to home page

### Checks

- [ ] shell tab titles render in German
- [ ] home dashboard text renders in German
- [ ] browse hub text renders in German
- [ ] search page labels and empty/error states render in German
- [ ] topic and CEFR browse labels render in German
- [ ] word detail actions and learning-state labels render in German
- [ ] favorites page labels render in German
- [ ] settings labels render in German
- [ ] lexical metadata labels render in German

### Result

- German UI result:
- German UI notes / copy issues:

---

## D. TTS Device Validation

### Supported-Voice Device

- [ ] word playback succeeds on a device with a compatible German voice
- [ ] example-sentence playback succeeds on a device with a compatible German voice

### Unsupported-Voice / Missing-Voice Device

- [ ] unsupported TTS state shows graceful localized message
- [ ] missing-locale state shows graceful localized message

### Result

- TTS result:
- TTS notes / device-specific issues:

---

## E. Sign-Off

- Remaining known issues accepted for release:
- Follow-up bugs filed:
- Final release-readiness recommendation:

# Web Tester Quick Start

Thank you for testing Darwin Lingua Web.

This is a controlled test, not a public launch. The goal is to find confusing flows, broken links, missing helper translations, layout problems, and content that does not feel useful or natural.

## Test Address

Open:

`https://lingua.vafadar.pro`

Use your normal browser and device. If possible, test once on a phone-sized screen too.

## Before You Start

Set your helper language in the app settings if you can.

Useful combinations for this test:

- Persian as primary helper language
- English as primary helper language
- Persian as primary and English as secondary

German is the main learning language. Helper translations are there to support understanding, not to replace the German content.

## What To Test

You do not need to test everything. Spend 30-45 minutes and write down concrete observations.

### 1. First Impression

Open the home page and try to find a learning area that feels useful to you.

Notice:

- Was it clear where to start?
- Which label or menu item was confusing?
- Did the page feel too busy, too empty, or clear enough?

### 2. Courses

Open `/courses`, choose a level near your current German level, then open one lesson.

Notice:

- Does the lesson feel like a guided learning path?
- Are the lesson steps clear?
- Do linked activities open where you expected?
- Does long text wrap correctly, especially on mobile?

### 3. Helper Languages

Open a Course lesson, Writing Template, Exam Prep unit, and Life in Germany note with your helper language enabled.

Notice:

- Is the helper text visible?
- Is the translation natural and useful?
- For Persian, Arabic, Kurdish, or other right-to-left text: is it readable and correctly aligned?
- If two helper languages are selected, is it clear what is being shown?

### 4. Writing Templates

Open `/writing-templates` and choose one template that could be useful in real life.

Notice:

- Are the template and sample easy to read?
- Are the placeholders understandable?
- Does the example fit the language level?
- Does the helper translation explain the meaning without replacing the German?

### 5. Exam Prep

Open `/exam-prep`, choose a relevant exam/profile, and open one unit.

Notice:

- Are the strategy notes concrete?
- Are linked practice materials relevant?
- Does the content avoid sounding like copied official exam questions?

### 6. Life In Germany

Open `/life-in-germany` and read one note.

Notice:

- Does it help you understand daily life in Germany?
- Is the explanation clear in your helper language?
- Is anything culturally unclear or misleading?

### 7. Search And Recent

Try searching:

- `Termin`
- `Antrag`
- `Pruefung`
- `Demokratie`

Then open `/recent`.

Notice:

- Are the search results relevant?
- Are result labels understandable?
- Does Recent show useful next steps?

## How To Report Feedback

For each issue, write:

- page URL
- device and browser
- helper language settings
- what happened
- what you expected
- severity: `blocker`, `major`, `minor`, `content-quality`, or `suggestion`

Use `blocker` only when you cannot continue testing.
Use `major` when the issue strongly hurts learning or trust.
Use `content-quality` when the wording, translation, example, or explanation is the main problem.

Screenshots are helpful for layout issues.

## Out Of Scope

Please do not test the mobile app. This pass is only for the Web product.

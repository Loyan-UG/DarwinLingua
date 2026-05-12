# Writing Template Content Package Contract

## Purpose

This document defines the initial JSON import contract for Web-first Writing Templates.

Writing Templates help learners write practical German messages and exam texts. Templates are dynamic content and must not be hardcoded in Razor views.

## Root Array

- `writingTemplates`

## Required Fields

- `slug`
- `title`
- `shortDescription`
- `cefrLevel`
- `category`
- `situation`
- `register`
- `templateText`
- `explanation`
- `replaceableVariables`
- `sampleFilledVersion`
- `sortOrder`

Optional linked-content fields:

- `linkedGrammarTopicSlugs`
- `linkedWordSlugs`
- `linkedExpressionSlugs`
- `linkedExerciseSlugs`
- `isPublished`

## Controlled Categories

- `email-to-school`
- `email-to-kindergarten`
- `message-to-landlord`
- `doctor-appointment-request`
- `appointment-reschedule`
- `sick-note-to-employer`
- `complaint`
- `application-email`
- `cancellation`
- `insurance-message`
- `government-office-message`
- `exam-email`
- `exam-opinion-text`

## Controlled Registers

- `formal`
- `informal`
- `neutral`
- `official`
- `workplace`
- `exam`

## Validation Rules

- slugs must be lowercase kebab-case
- CEFR levels must be valid
- category and register must use controlled values
- `templateText` is required
- every declared variable must appear in `templateText` as `{{variable-name}}`
- `sampleFilledVersion` is required for release-quality content
- linked slugs must use lowercase kebab-case

## No Duplication Rule

Writing Templates may link to grammar topics, words, expressions, and exercises. They must not duplicate word meanings, full grammar explanations, expression explanations, or exercise answer keys.

Bulk writing-template content generation must not start until this contract, validation, Web API, Web rendering, and admin inspection are stable.

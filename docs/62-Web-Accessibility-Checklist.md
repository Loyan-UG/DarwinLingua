# Web Accessibility Checklist

This checklist is the minimum accessibility review for `DarwinLingua.Web` before broader release validation.

## Landmarks and Structure

- verify each learner page has a single primary content region
- verify the root learner shell exposes a clear header, main region, and footer
- verify the admin shell exposes a distinct navigation landmark
- verify each page has one clear `h1` or page-level title

## Keyboard Navigation

- verify primary navigation links are reachable in a logical order
- verify forms can be completed without a pointer device
- verify htmx-updated panels preserve usable focus flow
- verify dialog open and close actions remain keyboard accessible

## Contrast and Visual Clarity

- verify text on panel backgrounds remains readable at WCAG AA contrast targets
- verify interactive chips, links, and buttons still look interactive without hover
- verify focus indication is visible on links, buttons, inputs, and dialog actions

## Async and State Changes

- verify empty, loading, and failure states always render text, not only icons or color
- verify incremental search and admin filter refreshes do not hide the current context
- verify confirmation dialogs explain the action and non-destructive exit path

## Mobile Review

- verify mobile navigation remains reachable and readable below `640px`
- verify tap targets remain large enough for primary learner and admin actions
- verify the install prompt banner does not block access to the main content

## Release Rule

Before a release candidate, run this checklist together with:

- `docs/55-Web-Browse-Search-Detail-Validation-Worksheet.md`
- `docs/56-Web-Pwa-Install-Validation-Worksheet.md`
- `docs/57-Web-Auth-Validation-Worksheet.md`

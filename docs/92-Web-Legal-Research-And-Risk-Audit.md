# Web Legal Research And Risk Audit

## Purpose

This document records the current engineering legal-risk audit for the Web product. It is not legal advice and it is not a substitute for qualified legal review before broad public launch.

The purpose is to keep Darwin Lingua aligned with current Germany/EU Web requirements while the product is still in controlled development testing on `https://darwinlingua.com`.

## Review Date And Scope

- Review date: 2026-06-23.
- Product scope: public Web, Web API, learner accounts, transactional email, learning content, Life in Germany content, admin diagnostics, controlled tester pass.
- Out of scope for this pass: MAUI/mobile implementation and public self-service paid subscriptions.
- Current public domains: `https://darwinlingua.com` and `https://api.darwinlingua.com`.
- `https://www.darwinlingua.com` is configured only as a canonical redirect to `https://darwinlingua.com`; tester action links and public legal/support copy must use the apex domain.

## Official Sources Checked

Primary/current sources reviewed for this pass:

- DDG full law and current single norms:
  - `https://www.gesetze-im-internet.de/ddg/BJNR0950B0024.html`
  - `https://www.gesetze-im-internet.de/ddg/__5.html`
  - `https://www.gesetze-im-internet.de/ddg/__33.html`
- TDDDG terminal-device storage/access:
  - `https://www.gesetze-im-internet.de/ttdsg/__25.html`
  - `https://www.bfdi.bund.de/DE/Buerger/Inhalte/Telemedien/Cookies.html`
  - `https://www.bfdi.bund.de/SharedDocs/Downloads/DE/DSK/Orientierungshilfen/OH_Digitale-Dienste.pdf`
- GDPR rights, transparency, deletion, portability, security, breach, and fines:
  - `https://eur-lex.europa.eu/eli/reg/2016/679/oj/eng`
  - `https://gdpr-info.eu/art-12-gdpr/`
  - `https://gdpr-info.eu/art-13-gdpr/`
  - `https://gdpr-info.eu/art-15-gdpr/`
  - `https://gdpr-info.eu/art-17-gdpr/`
  - `https://gdpr-info.eu/art-20-gdpr/`
  - `https://gdpr-info.eu/art-32-gdpr/`
  - `https://gdpr-info.eu/art-33-gdpr/`
  - `https://gdpr-info.eu/art-34-gdpr/`
  - `https://gdpr-info.eu/art-83-gdpr/`
- Digital Services Act enforcement in Germany:
  - `https://eur-lex.europa.eu/eli/reg/2022/2065/oj/eng`
  - `https://digital-strategy.ec.europa.eu/en/policies/dsa-dscs`
  - `https://www.dsc.bund.de/`
  - `https://www.bundesnetzagentur.de/SharedDocs/Pressemitteilungen/EN/2024/20240514_DSC.html`
  - `https://www.bundesnetzagentur.de/SharedDocs/Pressemitteilungen/EN/2026/20260430_TB_DSC.html`
- Accessibility and consumer-service duties:
  - `https://www.gesetze-im-internet.de/bfsg/`
  - `https://www.gesetze-im-internet.de/bfsg/__1.html`
  - `https://www.gesetze-im-internet.de/bfsg/__2.html`
  - `https://www.gesetze-im-internet.de/bfsgv/__19.html`
  - `https://www.bundesfachstelle-barrierefreiheit.de/DE/Barrierefreiheitsstaerkungsgesetz`
  - `https://www.bundesfachstelle-barrierefreiheit.de/DE/Barrierefreiheitsstaerkungsgesetz/FAQ/faq_node`
  - `https://www.bundesfachstelle-barrierefreiheit.de/DE/Barrierefreiheitsstaerkungsgesetz/E-Commerce/online-shops_node`
  - `https://www.gesetze-im-internet.de/vsbg/__36.html`
  - `https://www.gesetze-im-internet.de/bgb/__312k.html`
- Electronic advertising and transactional email boundary:
  - `https://www.gesetze-im-internet.de/uwg_2004/__7.html`
  - `https://www.bfdi.bund.de/DE/Fachthemen/Inhalte/Telemedien/Newsletter.html`
- Crime/fine/content-risk guardrails:
  - `https://www.gesetze-im-internet.de/stgb/__86a.html`
  - `https://www.gesetze-im-internet.de/stgb/__130.html`
  - `https://www.gesetze-im-internet.de/stgb/__184.html`
  - `https://www.gesetze-im-internet.de/stgb/__184b.html`
  - `https://www.gesetze-im-internet.de/stgb/__184c.html`
  - `https://www.gesetze-im-internet.de/stgb/__201a.html`
  - `https://www.gesetze-im-internet.de/stgb/`
  - `https://www.kjm-online.de/themen/technischer-jugendmedienschutz/unzulaessige-inhalte/`
  - `https://www.kjm-online.de/themen/aufsicht-internet/pornografie/`
- Life in Germany legal/civic content:
  - `https://www.bamf.de/DE/Themen/Integration/ZugewanderteTeilnehmende/Integrationskurse/Abschlusspruefung/abschlusspruefung-node.html`
  - `https://www.bamf.de/SharedDocs/Anlagen/DE/Integration/Einbuergerung/gesamtfragenkatalog-lebenindeutschland.html`
  - `https://www.gesetze-im-internet.de/kcang/BJNR06D0B0024.html`
  - `https://www.gesetze-im-internet.de/sbgg/BJNR0CE0B0024.html`

## Current Law Changes Reflected

- DDG replaced the old TMG reference for public provider information. The project must use DDG section 5, not old TMG section 5 wording.
- The official DDG text is current on Gesetze im Internet and records later amendments, including the 2026 amendment state shown on the full law page. The project therefore treats `DDG section 5` and `DDG section 33` as the current provider-information and fine-risk baseline.
- TTDSG is now TDDDG for the terminal-device storage/access discussion. The cookie/storage baseline must reference `TDDDG section 25`.
- BFSG has applied since 2025-06-28 for covered products/services. Official accessibility guidance treats electronic-commerce services and e-books as possible BFSG scope, while interactive learning offers that are not electronic books are not automatically treated as e-books. Because the current controlled Web test has no public paid self-service consumer contract flow and is an interactive learning product, this pass does not block tester readiness on BFSG. Reopen BFSG/BFSGV review before self-service paid subscriptions, consumer checkout, downloadable/e-book learning products, app-store consumer contract flows, or later mobile publication.
- The 2026 Bundesnetzagentur DSC activity report keeps DSA notice/action, statement-of-reasons, and internal complaint-handling implementation as active enforcement themes. Darwin Lingua's current contact/report route is enough for controlled learning tests, but broad public community/user-submitted surfaces need a specific moderation/reporting review before exposure.

## Current Project Mapping

The current implementation aligns with the audit as follows:

- `/legal` and `/impressum` exist and read provider data from `LegalNotice:*` configuration, including `Shahram Vafadar`, the configured Northeim address, `info@darwinlingua.com`, and data-protection contact.
- `/privacy` describes account, learning, organizer, moderation, operational email diagnostics, export, deletion, backup, and support-processing boundaries.
- `/terms` states that Darwin Lingua is educational, not legal/medical/immigration/financial advice, and prohibits illegal, hateful, extremist, pornographic, exploitative, harassing, fraudulent, security-abuse, and rights-infringing content.
- `/cookies` states the current no-banner position and limits it to necessary cookies, preference storage, session storage, and first-party PWA cache.
- `/contact` provides a route for support, privacy, security, abuse, and illegal-content reports.
- `tools/Web/New-WebLegalSurfaceAudit.ps1` verifies the public legal/support page surface from the configured Web domain before tester invitations: `/legal`, `/impressum`, `/privacy`, `/terms`, `/cookies`, and `/contact` must return HTTP 2xx, contain the expected operator/contact text, and avoid placeholders, old temporary domains, `www.darwinlingua.com` action links, and obvious secret leaks.
- Registration requires Terms acceptance and Privacy notice acknowledgement, and policy acceptance records are stored.
- Signed-in users have self-service account export and account deletion from `/account`.
- Transactional email is Brevo-backed, uses `no-reply@darwinlingua.com`, keeps `support@darwinlingua.com` as reply/support contact, and is documented as service-related rather than marketing.
- Manual public billing is disabled; Premium can be granted manually for testing.

## Risk Audit

| Risk area | Current status | Release decision |
| --- | --- | --- |
| Provider information / Impressum | Config-backed public legal notice exists. | Keep as blocker if operator data is missing, stale, or not reviewed. |
| Privacy notice / GDPR transparency | Privacy page and self-service controls exist. | Needs final operator/counsel review before broad launch. |
| Data-subject access/export | Self-service JSON export exists. | Verify in tester pass; keep manual escalation path. |
| Account deletion | Self-service delete exists with confirmation and retention notes. | Verify in tester pass; document backup/retention limits. |
| Cookie/storage consent | No marketing/analytics storage active. | No banner for current baseline; any non-essential storage requires opt-in design first. |
| Transactional email | Brevo real delivery, webhook, suppression, diagnostics, and DPA evidence exist. | Manual mailbox rendering review remains open. |
| Marketing email/newsletter | Not enabled. | Must not be added without separate consent/unsubscribe model. |
| DSA/illegal content reports | Contact/report route exists; community surfaces controlled. | Broader user-submitted content needs DSA/moderation review covering notice/action, reasons, complaint handling, and staff-supervised escalation. |
| BFSG/accessibility | Web has accessibility work; current interactive no-billing test is not treated as a BFSG blocker. | Reopen before self-service paid subscriptions, consumer checkout, downloadable/e-book learning products, app-store consumer contract flows, or later mobile publication. |
| VSBG consumer dispute information | Monitored. | Reopen before formal consumer launch/AGB changes. |
| BGB 312k cancellation button | Deferred because public paid subscriptions are disabled. | Required before online consumer subscription contracts are enabled. |
| Crime/youth-media content | Terms and Sensitive Educational Language policy block illegal and explicit content. | Keep content generation/admin gates active. |
| Life in Germany legal-adjacent content | Educational framing exists. | Do not copy official question banks; review exact legal/fine claims before each new legal/civic batch. |

## Remaining Human Gates

These are intentionally not marked complete by engineering automation:

- Legal Notice, Privacy Policy, Terms, Cookie/Storage Notice, and Contact page review by the operator and, before broad public launch, qualified legal counsel.
- Manual mailbox rendering review for real Brevo emails in `info@darwinlingua.com`.
- PWA install review on target desktop and Android browsers.
- Controlled tester pass and triage of blocker/major feedback.
- Accessibility/BFSG applicability review before paid self-service subscriptions, consumer e-commerce flows, downloadable/e-book learning products, app-store consumer contract flows, or mobile publication.
- Stripe and BGB 312k cancellation-button review before public paid billing is enabled.

## Engineering Rule

Any future feature that adds marketing email, analytics, third-party scripts, non-essential browser storage, user-submitted public content, paid self-service billing, consumer checkout, downloadable/e-book learning products, age-restricted content, official-looking civic/legal claims, app-store consumer contract flows, mobile publication, or broader community workflows must update this audit and the related release checklist before it is exposed to testers or production users.

# Web Legal Compliance Baseline

## Purpose

This document defines the practical Web-first legal and compliance baseline for `DarwinLingua.Web`.

It is an engineering and operations checklist, not final legal advice. Production launch still needs review by the operator and qualified legal counsel. Do not invent provider data, addresses, tax ids, VAT ids, phone numbers, legal representatives, data-protection officers, or registration numbers. Configure real operator data through environment-specific configuration before public launch.

Mobile compliance is deferred until the Web product, Web API, content model, registration, filtering, and operational reports are signed off. Do not use this document as authorization to change MAUI/mobile code.

## Assumptions

- Target environment: Germany/EU.
- Public Web product with learner accounts, transactional email, optional billing, Learning Portal content, and admin operations.
- Stripe billing is implemented but disabled by default in checked-in configuration.
- Sensitive Educational Language is educational, warning-labeled, hidden by default, and not pornographic or explicit adult content.

## References

Review these primary sources during production legal review:

- GDPR Regulation (EU) 2016/679: https://eur-lex.europa.eu/eli/reg/2016/679/oj/eng
- GDPR Article 12, transparent information and response timing: https://gdpr-info.eu/art-12-gdpr/
- GDPR Article 15, access: https://gdpr-info.eu/art-15-gdpr/
- GDPR Article 17, erasure: https://gdpr-info.eu/art-17-gdpr/
- GDPR Article 20, portability: https://gdpr-info.eu/art-20-gdpr/
- GDPR Article 83, administrative fines: https://gdpr-info.eu/art-83-gdpr/
- TDDDG section 25, terminal-device storage/access: https://www.gesetze-im-internet.de/ttdsg/__25.html
- DDG section 5, provider information / Impressum duties: https://www.gesetze-im-internet.de/ddg/__5.html
- DDG section 33, fines: https://www.gesetze-im-internet.de/ddg/__33.html
- BGB section 312k, cancellation button for consumer subscriptions: https://www.gesetze-im-internet.de/bgb/__312k.html
- StGB section 86a, unconstitutional and terrorist-organization symbols: https://www.gesetze-im-internet.de/stgb/__86a.html
- StGB section 184, pornographic content: https://www.gesetze-im-internet.de/stgb/__184.html
- StGB section 184b, child-pornographic content: https://www.gesetze-im-internet.de/stgb/__184b.html
- StGB section 184c, youth-pornographic content: https://www.gesetze-im-internet.de/stgb/__184c.html
- KJM development-impairing content: https://www.kjm-online.de/themen/technischer-jugendmedienschutz/entwicklungsbeeintraechtigung/
- KJM impermissible content and age-verification systems: https://www.kjm-online.de/themen/technischer-jugendmedienschutz/unzulaessige-inhalte/
- KJM pornography supervision notes: https://www.kjm-online.de/themen/aufsicht-internet/pornografie/
- Stripe legal terms and privacy/provider documentation: https://stripe.com/legal

## Legal Research Snapshot 2026-06-19

This snapshot is an engineering review of current official sources. It is not final legal advice and must be reviewed by the operator or qualified counsel before production launch.

- DDG section 5 is the current German provider-information baseline for the public legal notice / Anbieterkennzeichnung. `/legal` and `/impressum` must not go production with placeholder operator data.
- DDG section 33 defines fines for violations of DDG duties. The release checklist must treat missing or incorrect provider information as a production blocker.
- TDDDG section 25 remains the terminal-device storage/access reference. The current cookie/storage position is: strictly necessary authentication and anti-forgery cookies, culture preference storage, session-scoped learner navigation storage, and first-party PWA cache; no marketing or analytics storage is active.
- GDPR Articles 12, 15, 17, and 20 require transparent handling of access, deletion, and portability. Article 12 response timing is normally without undue delay and within one month, with possible extension for complex requests. The new self-service export/delete controls reduce support-only dependency but do not remove the need for an operator escalation process.
- GDPR Article 83 defines administrative fines, including a higher tier for data-subject-rights failures. Broken export/delete/rectification handling is therefore not just a UX issue.
- BGB section 312k becomes relevant when public paid consumer subscriptions are enabled online. Because public billing is disabled during Web testing and premium is manual, this is deferred but must be re-opened before self-service paid subscriptions are exposed.
- StGB sections 86a, 184, 184b, and 184c and KJM youth-media guidance are the current content-risk reference points for illegal symbols/propaganda and pornographic or minor-related sexual content. Darwin Lingua must keep explicit adult/pornographic, exploitative, minor-related, extremist propaganda, hate-inciting, and harm-facilitating content blocked. The current "Sensitive Educational Language" feature is not age verification and must not be used as a bypass for verified-adult content.

## Required Public Pages

The Web release must expose these public pages from the footer and registration flow:

- `/privacy`: Privacy Policy / privacy notice.
- `/terms`: Terms of Use.
- `/legal` and `/impressum`: Legal Notice / Anbieterkennzeichnung placeholder backed by configuration.
- `/cookies` and `/cookie-policy`: Cookie and browser-storage notice.
- `/contact`: support and privacy request contact placeholder.

Public pages must state clearly when text is draft operational copy pending legal review.

## Registration Requirements

Registration must:

- require Terms of Use acceptance
- show a clear Privacy Policy notice link and acknowledgement
- avoid calling the privacy notice a marketing consent
- avoid bundling marketing consent into account creation
- keep Sensitive Educational Language disabled by default
- store versioned policy acceptance records for required acknowledgements

The preferred acceptance records are:

- user id
- policy key
- policy version
- accepted-at UTC timestamp
- source
- culture/locale

Do not store raw IP address, full user agent, identity documents, or full birthdate for this baseline unless a later legal review explicitly requires and designs that processing.

## Cookie And Storage Rules

TDDDG section 25 is the release reference for storing or accessing information on a user's terminal equipment. GDPR transparency and legal-basis rules still apply to personal-data processing.

Current Web inventory:

- authentication cookies: strictly necessary
- anti-forgery cookies/tokens: strictly necessary
- culture preference cookie: preference/necessary for the requested UI language
- session storage for word-navigation context: first-party, session-scoped learner navigation support
- service-worker cache: first-party PWA shell assets only
- htmx and local JavaScript: first-party assets
- client event telemetry: no browser storage, but privacy notice must describe operational telemetry
- Stripe: server-side API and redirect flows only when billing is enabled; no Stripe client script is active in the current baseline
- analytics/marketing cookies: not present

Consent decision:

- No cookie banner is required for the current checked-in baseline because no non-essential analytics or marketing cookies/storage are activated.
- If future analytics, marketing, external-provider scripts, or non-essential storage are added, they must be blocked until opt-in.
- Withdrawal must be as easy as giving consent.
- Reject All and Accept All controls must be equally accessible if a future consent banner offers both.

## GDPR Baseline

Production review must map processing purposes to legal bases under GDPR Article 6. The baseline purposes are:

- account creation, sign-in, and account security
- email confirmation, password reset, email change, and transactional service messages
- learning preferences, favorites, recent activity, progress, and learner settings
- Sensitive Educational Language preference
- policy acceptance records
- support, moderation, organizer, partner, and RSVP workflows where enabled
- billing and entitlement state where Stripe is enabled
- admin diagnostics and operational security logs

Release review must cover:

- Article 5 principles: lawfulness, fairness, transparency, purpose limitation, data minimisation, accuracy, storage limitation, integrity/confidentiality, accountability
- Article 6 legal bases
- Article 7 only where consent is the chosen legal basis
- Articles 12 and 13 transparent notice
- Articles 15-22 data-subject rights
- Article 25 privacy by design and default
- Article 32 security
- Articles 33 and 34 breach notification triage

## Data-Subject Support

The Privacy and Contact pages must give a practical request path for:

- access
- rectification
- deletion
- restriction
- objection
- portability
- complaint to a supervisory authority

Current implementation:

- Signed-in users can export their account data from `/account/export-data`.
- Signed-in users can delete their account from `/account` after password verification where applicable and exact `DELETE` confirmation.
- `/account` links users to editable account and learner settings and to support/contact for rectification that cannot be completed self-service.
- Export includes identity/account state, roles, web preferences, favorites, policy acceptances, entitlement/billing state, safe email-delivery summaries, learning profiles, progress, attempts, partner/community records, and retention notes.
- Deletion removes account-linked learning state where possible, anonymizes conversation profiles and word suggestions tied to email, and detaches operational audit records that may need retention for legal, accounting, security, fraud-prevention, or abuse-prevention reasons.
- Backups are not edited record by record; restoration and expiry follow the operational backup policy.

Before production, the operator must still define:

- request intake owner
- identity confirmation process
- response timelines
- retention and backup-deletion boundaries
- escalation path for difficult requests

## Sensitive Educational Language

Sensitive Educational Language is covered by `85-Sensitive-Educational-Language-Policy.md`.

Release rules:

- hidden from anonymous users by default
- hidden from authenticated users until they enable the dedicated setting
- setting label: "Show sensitive educational language"
- setting is a reversible learner preference
- setting is not age verification
- explicit adult or pornographic content remains blocked
- `requiresVerifiedAdult` content remains hidden because no verified-adult system exists
- admin reports may show aggregate counts but not private learner preferences

The current product must not generate pornographic, arousing, graphic sexual, exploitative, coercive, minor-related, illegal, hate-inciting, Nazi-propaganda, or harm-facilitating content.

Crime/fine review notes:

- Do not add content that instructs users to commit fraud, evade legal obligations, bypass security systems, or misuse government/identity processes.
- Do not add extremist propaganda, unconstitutional-symbol promotion, hate-inciting content, or content that normalizes violence against protected or vulnerable groups.
- Do not add pornographic, arousing, explicit adult, exploitative, coercive, or minor-related sexual content. If adult-only educational content is ever reconsidered, it requires a separate verified-adult access system and legal review first.
- Keep Life in Germany content educational and explanatory; it may help learners understand everyday civic, social, and administrative topics but must not present itself as official legal, immigration, or government advice.

## Admin And Operator Access

Admin reports should show content and readiness counts without exposing private user-specific preferences.

Required operator checks:

- legal pages present and linked
- required legal notice configuration present before production
- Terms and Privacy policy versions current
- cookie/storage inventory reviewed
- transactional email provider and DPA reviewed
- Stripe provider/legal text reviewed if billing is enabled
- Sensitive Educational Language filtering and warnings verified
- policy acceptance table available and populated by registration

## Billing And Stripe

Stripe is disabled by default in checked-in configuration. If billing is enabled:

- review `75-Stripe-Billing-Validation-Playbook.md`
- review Stripe provider terms, DPA/data-processing settings, customer communications, invoices/receipts, tax/VAT assumptions, refund/cancellation handling, and customer portal behavior
- keep Stripe secrets out of source control
- do not expose raw Stripe payloads or secrets in admin diagnostics

## Release Gates

Before public release:

- Legal Notice configured and reviewed
- Privacy Policy reviewed
- Terms of Use reviewed
- Cookie/Storage Notice reviewed
- Contact/support route configured
- registration requires Terms acceptance and Privacy notice acknowledgement
- policy acceptance records are stored
- no marketing consent is bundled into account creation
- Sensitive Educational Language opt-in is separate, off by default, and reversible
- Web/API/search filtering for sensitive content verified
- admin reports show sensitive content counts and gaps
- cookie/storage inventory reviewed
- no non-essential storage runs before consent
- transactional email provider/DPA reviewed
- billing provider/legal text reviewed if billing is enabled
- self-service account export/delete verified
- data-subject request escalation process documented
- breach triage process documented
- mobile compliance remains deferred until the mobile phase

## Current Evidence

As of 2026-06-19:

- `/terms`, `/privacy`, `/legal`, `/impressum`, `/cookies`, `/cookie-policy`, and `/contact` are implemented in Web.
- Registration requires Terms acceptance and Privacy notice acknowledgement.
- Versioned `WebPolicyAcceptances` records are created during registration.
- `/account` exposes self-service account data export, rectification links, and account deletion.
- Sensitive Educational Language is disabled by default and controlled from Settings.
- Current checked-in Web assets do not use third-party analytics or marketing cookies.
- Public billing remains disabled during Web testing; premium access can be granted manually by an operator.
- The temporary development domain is `lingua.vafadar.pro`; final production domain migration is deferred until Web maturity and user-testing feedback.
- Mobile/MAUI implementation remains unchanged and deferred.

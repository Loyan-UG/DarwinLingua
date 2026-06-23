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
- GDPR Article 32, processing security: https://eur-lex.europa.eu/eli/reg/2016/679/oj/eng
- GDPR Article 33 and 34, personal-data-breach notification and communication: https://eur-lex.europa.eu/eli/reg/2016/679/oj/eng
- GDPR Article 83, administrative fines: https://gdpr-info.eu/art-83-gdpr/
- TDDDG section 25, terminal-device storage/access: https://www.gesetze-im-internet.de/ttdsg/__25.html
- DDG section 5, provider information / Impressum duties: https://www.gesetze-im-internet.de/ddg/__5.html
- DDG section 33, fines: https://www.gesetze-im-internet.de/ddg/__33.html
- Digital Services Act, Regulation (EU) 2022/2065: https://eur-lex.europa.eu/eli/reg/2022/2065/oj/eng
- Digital Services Coordinator Germany / Bundesnetzagentur: https://www.dsc.bund.de/
- Digital Services Coordinators, European Commission overview: https://digital-strategy.ec.europa.eu/en/policies/dsa-dscs
- Bundesnetzagentur DSC 2025 activity report press release: https://www.bundesnetzagentur.de/SharedDocs/Pressemitteilungen/EN/2026/20260430_TB_DSC.html
- UWG section 7, unreasonable nuisance / electronic advertising: https://www.gesetze-im-internet.de/uwg_2004/__7.html
- BGB section 312k, cancellation button for consumer subscriptions: https://www.gesetze-im-internet.de/bgb/__312k.html
- BFSG / Barrierefreiheitsstaerkungsgesetz: https://www.gesetze-im-internet.de/bfsg/
- BFSG section 1, scope: https://www.gesetze-im-internet.de/bfsg/__1.html
- BFSG section 2, definitions: https://www.gesetze-im-internet.de/bfsg/__2.html
- BFSGV / Barrierefreiheitsanforderungen: https://www.gesetze-im-internet.de/bfsgv/
- BFSGV section 19, e-commerce service requirements: https://www.gesetze-im-internet.de/bfsgv/__19.html
- Bundesfachstelle Barrierefreiheit BFSG overview and FAQ: https://www.bundesfachstelle-barrierefreiheit.de/DE/Barrierefreiheitsstaerkungsgesetz
- Bundesfachstelle Barrierefreiheit BFSG FAQ: https://www.bundesfachstelle-barrierefreiheit.de/DE/Barrierefreiheitsstaerkungsgesetz/FAQ/faq_node
- VSBG section 36, consumer dispute-resolution information duty: https://www.gesetze-im-internet.de/vsbg/__36.html
- BAMF Abschlusspruefung / Test Leben in Deutschland framing: https://www.bamf.de/DE/Themen/Integration/ZugewanderteTeilnehmende/Integrationskurse/Abschlusspruefung/abschlusspruefung-node.html
- BAMF Gesamtfragenkatalog download page: https://www.bamf.de/SharedDocs/Anlagen/DE/Integration/Einbuergerung/gesamtfragenkatalog-lebenindeutschland.html
- BMG Cannabisgesetz FAQ: https://www.bundesgesundheitsministerium.de/themen/cannabis/faq-cannabisgesetz
- KCanG / Konsumcannabisgesetz: https://www.gesetze-im-internet.de/kcang/BJNR06D0B0024.html
- SBGG / Self-Determination Act: https://www.gesetze-im-internet.de/sbgg/BJNR0CE0B0024.html
- StGB section 86a, unconstitutional and terrorist-organization symbols: https://www.gesetze-im-internet.de/stgb/__86a.html
- StGB section 130, incitement to hatred / Volksverhetzung: https://www.gesetze-im-internet.de/stgb/__130.html
- StGB section 184, pornographic content: https://www.gesetze-im-internet.de/stgb/__184.html
- StGB section 184b, child-pornographic content: https://www.gesetze-im-internet.de/stgb/__184b.html
- StGB section 184c, youth-pornographic content: https://www.gesetze-im-internet.de/stgb/__184c.html
- StGB section 201a, intimate image/privacy violations: https://www.gesetze-im-internet.de/stgb/__201a.html
- StGB sections 202a-202d, data espionage, interception, preparation, and stolen data: https://www.gesetze-im-internet.de/stgb/
- StGB section 263a, computer fraud: https://www.gesetze-im-internet.de/stgb/__263a.html
- StGB section 303a, data alteration: https://www.gesetze-im-internet.de/stgb/__303a.html
- KJM development-impairing content: https://www.kjm-online.de/themen/technischer-jugendmedienschutz/entwicklungsbeeintraechtigung/
- KJM impermissible content and age-verification systems: https://www.kjm-online.de/themen/technischer-jugendmedienschutz/unzulaessige-inhalte/
- KJM pornography supervision notes: https://www.kjm-online.de/themen/aufsicht-internet/pornografie/
- Stripe legal terms and privacy/provider documentation: https://stripe.com/legal

## Legal Research Snapshot 2026-06-20

This snapshot is an engineering review of current official sources. It is not final legal advice and must be reviewed by the operator or qualified counsel before production launch.

- DDG section 5 is the current German provider-information baseline for the public legal notice / Anbieterkennzeichnung. `/legal` and `/impressum` must not go production with placeholder operator data.
- DDG section 33 defines fines for violations of DDG duties. The release checklist must treat missing or incorrect provider information as a production blocker.
- TDDDG section 25 remains the terminal-device storage/access reference. The current cookie/storage position is: strictly necessary authentication and anti-forgery cookies, culture preference storage, session-scoped learner navigation storage, and first-party PWA cache; no marketing or analytics storage is active. The 2024 DSK orientation aid confirms that section 25 applies to terminal-device storage/access independently of whether personal data is involved, and that cookie/storage necessity must be minimized by purpose, duration, and who can read the data.
- GDPR Articles 12, 15, 17, and 20 require transparent handling of access, deletion, and portability. Article 12 response timing is normally without undue delay and within one month, with possible extension for complex requests. The new self-service export/delete controls reduce support-only dependency but do not remove the need for an operator escalation process.
- GDPR Articles 32, 33, and 34 require security measures and breach triage. Security incidents involving account data, email tokens, learning data, billing state, or private community records must have an operator-owned triage path before real-user testing expands beyond a controlled group.
- GDPR Article 83 defines administrative fines, including a higher tier for data-subject-rights failures. Broken export/delete/rectification handling is therefore not just a UX issue.
- UWG section 7 makes unsolicited electronic advertising a legal risk. Current transactional email must stay strictly service-related; newsletters, promotional campaigns, or win-back campaigns require a separate consent model and unsubscribe process before they are enabled.
- The DSA applies on a graduated basis to intermediary services. Darwin Lingua is primarily a language-learning service, but organizer profiles, partner/community workflows, claims, reports, and user-submitted content can create notice/action and moderation-process obligations if exposed publicly. Germany's Digital Services Coordinator is the Bundesnetzagentur. Terms, Contact, and Admin operations must preserve a clear route for illegal-content, abuse, and rights reports before broader community release.
- BGB section 312k becomes relevant when public paid consumer subscriptions are enabled online. Because public billing is disabled during Web testing and premium is manual, this is deferred but must be re-opened before self-service paid subscriptions are exposed.
- StGB sections 86a, 130, 184, 184b, 184c, 201a, 202a-202d, 263a, and 303a and KJM youth-media guidance are current crime-risk reference points for illegal symbols/propaganda, hate-inciting content, pornographic or minor-related sexual content, intimate-image abuse, data-security crimes, computer fraud, and data alteration. Darwin Lingua must keep explicit adult/pornographic, exploitative, minor-related, extremist propaganda, hate-inciting, illegal hacking/security-abuse, fraud-facilitating, and harm-facilitating content blocked. The current "Sensitive Educational Language" feature is not age verification and must not be used as a bypass for verified-adult content.
- The 2026-06-20 official-source refresh found one Life in Germany content-risk correction: do not state a single fixed official question-bank total without context. The BAMF 2026 Abschlusspruefung page states the real LiD test sheet has 33 questions, 60 minutes, 15 correct answers to pass, and 17 correct answers for citizenship-relevant proof, and says the interactive online catalog shows answers for 310 questions. The separate BAMF downloadable Gesamtfragenkatalog page dated 2025-05-26 describes the full catalog as 300 federal questions plus 160 federal-state-specific questions. App content should teach the underlying concepts and cite source/review date when exact test-catalog numbers are shown.
- The 2026-06-20 cannabis/crime/fine refresh confirms that cannabis content must not imply broad legality: BMG/KCanG currently describe limited adult possession/cultivation, continued prohibition for minors, consumption-place restrictions, no transfer to minors, and administrative-offence/criminal-law thresholds for excess quantities. Use cautious wording for consequences unless the exact rule and review date are carried in the content.

### Official-Source Refresh 2026-06-22

This follow-up checked the current operator-facing legal baseline against official/current sources again before the Brevo/domain readiness checkpoint.

- DDG section 5 remains the active German provider-information basis. The configured public operator baseline is now `Shahram Vafadar`, `Achterkirchenstrasse 10, 37154 Northeim, Germany`, contact `info@darwinlingua.com`, and data-protection contact `info@darwinlingua.com`. `/legal` and `/impressum` must keep reading those values from configuration rather than hard-coded page copy.
- DDG section 33 remains the fine-risk reference for DDG obligations. Missing, misleading, or stale provider information stays a launch blocker even during controlled Web testing.
- TDDDG section 25 and the DSK 2024 "OH Digitale Dienste" remain the cookie/storage gate. The current app still has no marketing cookies or third-party analytics, so the no-banner position remains valid only while no non-essential storage is introduced.
- GDPR Articles 12, 15, 17, 20, 32, 33, 34, and 83 remain the data-subject-rights, security, breach, portability, deletion, and fine-risk baseline. Self-service export/delete is implemented, but manual privacy requests and breach triage still need an operator-owned process before broad public launch.
- Because the configured operator location is in Lower Saxony, the likely competent data-protection supervisory authority for the public privacy notice is the Landesbeauftragte fuer den Datenschutz Niedersachsen. The Privacy page should route users there without removing their right to contact another competent authority under GDPR Article 77.
- VSBG section 36 can become relevant when Darwin Lingua is operated as a consumer-facing business with more than the statutory employee threshold or with AGB consumer-dispute obligations. Public paid subscriptions remain disabled, so this is monitored but not a blocker for the current no-billing controlled Web testing phase.
- BGB section 312k remains deferred until self-service paid subscriptions are exposed. Manual Premium grants during testing do not trigger the same online cancellation-button flow, but this must be reopened before Stripe self-service billing is enabled.

### Official-Source Refresh 2026-06-23

This refresh checked current official sources again after the `darwinlingua.com` and Brevo readiness work. It also reviewed the service from the perspective of administrative fines, crimes, illegal-content reports, and accessibility duties that can become relevant once the Web product moves from controlled development testing to broader public use.

- DDG section 5 and section 33 remain the active German public-provider-information and fine-risk baseline. The checked-in public Legal Notice now reads operator data from configuration and currently has the configured development-stage operator details. Do not publish with stale address, email, or responsible-person data.
- TDDDG section 25 remains the terminal-device storage/access gate. The current no-banner position depends on keeping analytics, advertising, third-party tracking, and non-essential storage disabled until a real consent model exists.
- GDPR Articles 12, 15, 17, 20, 32, 33, 34, and 83 remain the data-subject-rights, deletion/export/portability, security, breach, and fine-risk baseline. Self-service export/delete is implemented and now transaction-protected for the local account/user-state deletion path, but the operator still needs a manual escalation process for complex identity, backup, billing, security, and abuse-retention cases.
- The Bundesnetzagentur is the German Digital Services Coordinator for DSA enforcement and acts as a central complaint point. Darwin Lingua is currently a controlled language-learning product, but organizer, partner, RSVP, report, claim, profile, or other user-submitted surfaces must keep a clear abuse/illegal-content reporting route before broader community release.
- BFSG has been applicable since 28 June 2025 for covered consumer products and services. The Bundesfachstelle Barrierefreiheit states that services in electronic commerce and e-books can be in scope, while interactive learning offers that are not the electronic version of a book are not automatically covered as e-books. Because the current controlled Web test is an interactive learning product with no public paid self-service consumer contract flow, BFSG is not treated as a blocker for this test. Reopen BFSG/BFSGV review before Stripe/self-service paid subscriptions, consumer e-commerce checkout, downloadable/e-book learning products, app-store consumer contract flows, or later mobile publication are exposed.
- VSBG section 36 can require consumer-dispute information on a website/AGB for qualifying businesses. This remains monitored while Darwin Lingua is a development-stage personal project with no public paid subscriptions; reopen before broad consumer launch or if formal AGB/business operations change.
- BGB section 312k remains deferred until online consumer subscription contracts can be concluded through the product. Manual Premium grants during testing are not the same as self-service paid subscriptions.
- StGB crime-risk and KJM youth-media references remain product-content guardrails. The Terms page already prohibits illegal, hateful, extremist, pornographic, exploitative, harassing, fraudulent, security-abuse, or rights-infringing content. Keep these prohibitions aligned with moderation tooling before user-submitted content is widened.

### Post-Audit Official-Source Confirmation 2026-06-23

After the controlled tester readiness audit, the current public-page and content-safety baseline was spot-checked again against official sources on `gesetze-im-internet.de` and EUR-Lex:

- DDG sections 5 and 33 still support the existing release gate: public provider information must stay complete and stale/misleading provider data remains a fine-risk blocker.
- TDDDG section 25 still supports the current cookie/storage position: no marketing, advertising, analytics, or third-party tracking storage is introduced without a real consent model.
- GDPR Articles 12, 15, 17, 20, 32, 33, 34, and 83 still support the current self-service export/delete plus operator escalation model; the remaining gap is operational/legal review, not missing Web UI.
- BGB section 312k remains deferred because no self-service paid consumer subscription contract can currently be concluded through the product.
- VSBG section 36 remains monitored for later consumer-business/AGB launch conditions; it is not treated as a blocker for the current no-billing controlled Web test.
- StGB crime-risk references, especially sections 86a, 130, 184, 184b, 184c, 201a, 202a-202d, 263a, and 303a, remain aligned with the current Terms and content gates. No new allowance is created for extremist propaganda, hate-incitement, pornographic/minor-related sexual content, intimate-image abuse, credential theft, hacking misuse, fraud facilitation, or destructive data misuse.

### Late Official-Source Refresh 2026-06-23

This late refresh checked official/current sources after the public-domain, TLS, Brevo, and tester-readiness work:

- TLS/domain note: `https://darwinlingua.com` is the primary public Web origin, `https://api.darwinlingua.com` is the API origin, and `https://www.darwinlingua.com` is only a canonical redirect to the apex domain. Public legal/support pages and tester instructions must not use `www` as an action URL.
- DSA/Bundesnetzagentur note: the German DSC's 2025 activity reporting highlights complaints about inadequate reasons for restrictions/removal/non-removal and the usability of illegal-content reporting systems, and proceedings focused on DSA Articles 16, 17, and 20. Darwin Lingua's current controlled learning product is not being treated as a broad public platform, but any wider user-submitted/community surface must preserve a visible report route, reasoned moderation records, staff-supervised decisions, and a non-automated escalation path before release.
- BFSG note: the Bundesfachstelle FAQ says BFSG covers certain products/services, including electronic-commerce services and e-books, and says websites fall in scope when corresponding consumer services are offered through them. It also distinguishes digital learning media in e-book form from interactive learning offers that are not electronic books. Therefore the current no-billing interactive Web test remains a monitored accessibility risk, not a launch blocker; paid checkout, consumer contract flows, e-book/download products, or app-store publication must reopen BFSG/BFSGV section 19 review.
- GDPR fine-risk note: Article 83 keeps data-subject rights, deletion/export/portability, security, and breach handling in the administrative-fine risk surface. Self-service export/delete is implemented, but manual escalation, backup-retention explanation, breach triage ownership, and complex identity/support handling stay mandatory before broad public launch.

## Life In Germany Legal-Adjacent Content Gate

`Life in Germany` is educational civic and everyday-orientation content. It may explain legal systems, rights, duties, crimes, fines, public offices, residence/citizenship basics, and everyday administrative expectations, but it must not present itself as official government information or individual legal, immigration, medical, financial, or tax advice.

The current source-refresh gate is recorded in `artifacts/planning/life-in-germany-content-plan.md` under `2026 Legal and Civic Research Refresh`. Before any new B2+ legal/civic batch is generated or imported, verify that the gate still reflects current official sources for:

- BAMF integration course, Orientierungskurs, DTZ, Test Leben in Deutschland, and Einbuergerungstest framing.
- Grundgesetz fundamentals: dignity, equality, non-discrimination, religious freedom, expression and its limits, family, school, assembly, and federal democracy.
- Citizenship and residence orientation after the 2024 nationality-law reform and the 2024 skilled-worker/Chancenkarte changes.
- Cannabis rules after the 2024 Cannabisgesetz, including the difference between limited legality and unrestricted use.
- Self-Determination Act / SBGG civil-status changes that took effect in 2024.
- Crime and fine boundaries for violence, threats, stalking, forced marriage, insult, false accusation, discrimination, hate/extremist symbols, traffic, cannabis, fare evasion, shoplifting, weapons, waste/local rules, and public-order issues.
- Help and escalation routes such as emergency services, police, anti-discrimination advice, migration advice, tenant/consumer advice, victim support, school/youth-office context, and when to ask a lawyer.

Release rules for this content:

- Teach the civic reason, safe everyday action, and help path.
- Use exact fine amounts only when the source and review date are recorded; otherwise use cautious wording such as "can lead to a fine" or "can lead to a criminal investigation."
- Do not describe tactics for avoiding enforcement, deceiving authorities, bypassing identity checks, or hiding illegal conduct.
- Do not copy the official BAMF question bank as product-owned content. It is acceptable to cite official resources in docs/planning and to teach the underlying concepts in original explanatory language.
- Keep helper translations semantic and culturally aware, but non-stereotyping.
- Keep public page disclaimers aligned with `/terms`: Life in Germany is learning content and does not replace official guidance or legal advice.

## Required Public Pages

The Web release must expose these public pages from the footer and registration flow:

- `/privacy`: Privacy Policy / privacy notice.
- `/terms`: Terms of Use.
- `/legal` and `/impressum`: Legal Notice / Anbieterkennzeichnung backed by operator configuration.
- `/cookies` and `/cookie-policy`: Cookie and browser-storage notice.
- `/contact`: support and privacy request contact backed by operator configuration.

Public pages must state clearly that the current copy is an engineering/legal baseline pending operator or counsel review before broad public launch.

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
- security, abuse, illegal-content, or rights reports where user/community content is involved

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
- breach triage owner and decision record for GDPR Articles 33 and 34
- moderation/illegal-content report intake owner for public user-generated content

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
- Do not add simulated phishing, credential theft, malware, bypass, exploit, identity-document misuse, payment fraud, or data-destruction instructions. Security-related language learning must stay defensive and general.
- Do not accept or publish private photos, intimate images, ID documents, medical records, or immigration-case files unless a later feature has a reviewed necessity, minimisation, retention, and deletion design.
- Keep Life in Germany content educational and explanatory; it may help learners understand everyday civic, social, and administrative topics but must not present itself as official legal, immigration, or government advice.

## Community, Reports, And DSA-Aware Boundaries

The Web product has organizer, partner, RSVP, claim, profile, and report concepts. They are not the core learning content, but they can create moderation and illegal-content handling duties when exposed to real users.

Release rules:

- Keep community/user-submitted content controlled during the first tester wave.
- Provide a clear support route for illegal-content, abuse, privacy, rights, and security reports.
- Keep admin issue handling role-protected and avoid exposing private reporter data in public pages.
- Do not represent claim approval, organizer pages, or Life in Germany content as official government verification, legal advice, immigration advice, or professional certification.
- Before any broad public community release, review DSA classification, notice/action handling, moderation transparency, and appeal/escalation requirements with counsel.

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
- BFSG/accessibility applicability reviewed before paid consumer e-commerce, broader public subscription flows, downloadable/e-book learning products, or app-store consumer contract flows
- data-subject request escalation process documented
- breach triage process documented
- mobile compliance remains deferred until the mobile phase

## Current Evidence

As of 2026-06-23:

- `/terms`, `/privacy`, `/legal`, `/impressum`, `/cookies`, `/cookie-policy`, and `/contact` are implemented in Web.
- Registration requires Terms acceptance and Privacy notice acknowledgement.
- Versioned `WebPolicyAcceptances` records are created during registration.
- `/account` exposes self-service account data export, rectification links, and account deletion.
- Sensitive Educational Language is disabled by default and controlled from Settings.
- Current checked-in Web assets do not use third-party analytics or marketing cookies.
- Public billing remains disabled during Web testing; premium access can be granted manually by an operator.
- The primary product domain is `darwinlingua.com`; `api.darwinlingua.com` is the API host for Web/API separation during Web testing and production hardening.
- Current configured operator baseline: Shahram Vafadar, Achterkirchenstrasse 10, 37154 Northeim, Germany, contact `info@darwinlingua.com`, data-protection contact `info@darwinlingua.com`.
- Brevo transactional email is configured outside Git, DPA is accepted, direct real-delivery smoke passed, and app-level registration/password-reset sends are logged through provider `brevo-api`.
- Account deletion now uses explicit transactions for Web identity/user-state and shared learning cleanup before the user deletion is committed.
- Mobile/MAUI implementation remains unchanged and deferred.

# Sensitive Educational Language Policy

## Purpose

This document defines the product, content, filtering, privacy, and release policy for Sensitive Educational Language in Darwin Lingua.

Sensitive Educational Language is language-learning content that may include rude words, insults, slang, mild emotional or romantic/social phrases, warning phrases, and neutral sexual-language comprehension where pedagogically justified. It is not pornographic content, adult entertainment, or arousing/graphic sexual content.

This policy applies first to Everyday Expressions and should be reused by later modules before they generate sensitive language-learning content.

Current implementation scope is Web-only. Mobile parity and mobile package enforcement remain deferred until the Web/API/content-operation path is signed off.

## Scope

Allowed Sensitive Educational Language:

- common swear words and insults that learners may hear in real life
- rude or slang expressions needed for comprehension
- mild frustration, rejection, annoyance, and boundary-setting phrases
- mild emotional, embarrassment, affection, and flirting-light phrases
- romantic/social expressions that are non-graphic and socially useful
- neutral sexual-language comprehension only when non-arousing, non-graphic, and practical for safe language understanding

Blocked content:

- pornographic, arousing, or graphic sexual content
- explicit adult entertainment content
- sexual content involving minors
- coercion, exploitation, sexual violence, or illegal sexual content
- animal sexual content
- hate incitement, Nazi propaganda, illegal instructions, or content that facilitates harm
- discriminatory slurs unless a future manually reviewed comprehension-only package is explicitly approved

## Why This Is Not Pornographic Content

The feature teaches language that learners may encounter and need to understand. It does not produce pornographic scenes, sexual arousal content, explicit adult entertainment, or graphic sexual descriptions.

German youth-media protection distinguishes content that may impair the development of minors from content that is pornographic or otherwise strongly youth-endangering. KJM guidance explains that development-impairing content can be handled through technical or other access restrictions, while pornographic and certain youth-endangering online content requires closed adult user groups with age-verification systems.

Operational references:

- KJM Entwicklungsbeeintraechtigung: https://www.kjm-online.de/themen/technischer-jugendmedienschutz/entwicklungsbeeintraechtigung/
- KJM Unzulaessige Inhalte / AVS: https://www.kjm-online.de/themen/technischer-jugendmedienschutz/unzulaessige-inhalte/
- KJM Pornografie: https://www.kjm-online.de/themen/aufsicht-internet/pornografie/

This policy is a product and engineering control, not legal advice. Production use of any legally restricted explicit adult content remains blocked until legal review and an approved age-verification or closed-user-group concept exist.

## User Setting

The learner setting is named:

- Show sensitive educational language

The setting may reveal warning-labeled educational content such as rude words, insults, slang, and mild relationship or emotional expressions for learning and comprehension.

The setting must also explain what it does not do:

- it does not show pornographic or explicit adult content
- it does not verify the learner's age
- it does not unlock content that requires verified adult access
- it does not remove usage warnings

Anonymous users and users without this setting see only general content.

The setting is separate from account registration. Registration may require Terms of Use acceptance and Privacy Policy notice acknowledgement, but it must not enable Sensitive Educational Language by default.

## Registration And Legal Notice

Account registration must provide clear links to the Terms of Use and Privacy Policy before account creation.

The registration flow may require:

- Terms of Use acceptance
- Privacy Policy notice acknowledgement

Privacy notice acknowledgement is not a marketing or optional-content consent. Do not describe it as consent unless the product has explicitly chosen consent as the legal basis for that processing purpose.

Policy acceptance records should be versioned and minimal:

- user id
- policy key
- policy version
- accepted-at UTC timestamp
- source
- locale/culture when available

Do not store full birth dates, identity documents, raw IP addresses, or user-agent fingerprints for this Sensitive Educational Language feature.

## Metadata

Expression entries use these fields:

- `safetyRating`
- `sensitiveContentKind`
- `minimumAge`
- `requiresSensitiveOptIn`
- `requiresVerifiedAdult`
- `contentWarnings`
- `usagePolicy`

Allowed `safetyRating` values:

- `general`
- `mild-rude`
- `strong-rude`
- `sexual-educational`
- `romantic-social`
- `discriminatory-slur`
- `politically-sensitive`
- `explicit-adult`
- `blocked-illegal`

Allowed `sensitiveContentKind` values:

- `none`
- `swear-word`
- `insult`
- `rude-colloquial`
- `mild-emotional`
- `romantic-social`
- `sexual-educational-neutral`
- `slur-educational`
- `blocked`

Allowed `minimumAge` values:

- `0`
- `12`
- `16`
- `18`

Allowed `usagePolicy` values:

- `safe-to-use`
- `use-with-care`
- `understand-only`
- `do-not-use`
- `blocked`

## Validation Rules

Official content import must reject:

- `blocked-illegal`
- `explicit-adult`
- `requiresVerifiedAdult: true`
- `sensitiveContentKind: blocked`
- `sensitiveContentKind: slur-educational` unless a future manual-review workflow explicitly allows it
- `usagePolicy: blocked`
- graphic, arousing, pornographic, exploitative, coercive, minor-related, hate-inciting, Nazi-propaganda, or illegal-instruction content

Official sensitive educational entries must:

- set `requiresSensitiveOptIn: true`
- use non-`general` `safetyRating`
- use a non-`none` `sensitiveContentKind`
- include warning metadata
- use a non-default `usagePolicy` such as `use-with-care`, `understand-only`, or `do-not-use`
- remain educational, neutral, and non-endorsing

`sexual-educational` content may only be neutral and non-graphic. It must use `sensitiveContentKind: sexual-educational-neutral` and must not be arousing or explicit.

`romantic-social` content may be included when mild, non-graphic, and socially useful.

## Filtering Rules

Learner list, detail, API, and search surfaces must hide sensitive entries by default.

Visible by default:

- `general`
- `sensitiveContentKind: none`
- `minimumAge: 0`
- `requiresSensitiveOptIn: false`
- `requiresVerifiedAdult: false`
- `usagePolicy: safe-to-use`

Visible only after Sensitive Educational Language opt-in:

- `mild-rude`
- `strong-rude`
- `romantic-social`
- `sexual-educational` when neutral and accepted by import validation

Always hidden in the current system:

- `explicit-adult`
- `blocked-illegal`
- `discriminatory-slur`
- `requiresVerifiedAdult: true`
- blocked/slur sensitive kinds

Filtering must happen in repository, query, and API layers. Razor views may add presentation cues but must not be the only protection.

## Admin And Reports

Admin and quality reports must show:

- expression counts by `safetyRating`
- expression counts by `sensitiveContentKind`
- expression counts by `usagePolicy`
- count requiring sensitive opt-in
- count requiring verified adult access
- count blocked or explicit-adult
- count missing warning metadata
- count sensitive entries missing usage policy
- count old risky entries missing sensitive metadata
- missing translations by language

Admin reports must not expose private learner settings or profile data.

## Mobile Export

Until mobile has equivalent opt-in filtering and warning rendering, mobile catalog export must exclude entries that require Sensitive Educational Language opt-in.

Do not implement mobile parity in the current Web-side slice. Track mobile filtering as a deferred release gate after Web sign-off.

Mobile export must always exclude:

- `explicit-adult`
- `blocked-illegal`
- `requiresVerifiedAdult: true`
- `discriminatory-slur`
- blocked/slur sensitive kinds

## Privacy And Data Minimisation

For this feature:

- do not store full birth dates
- do not collect identity documents
- do not claim age verification
- store only the learner preference needed to show or hide Sensitive Educational Language
- keep any future verified-over-18 state as a minimal status claim from a reviewed provider

If explicit adult content is ever considered, it needs a separate legal, privacy, provider, and age-verification design before any content generation starts.

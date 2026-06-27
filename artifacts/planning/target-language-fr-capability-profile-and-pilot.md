# French Target-Language Capability Profile And Pilot Plan

Status: planned, not content-importable.

Created: 2026-06-24.

Purpose: define French as a future target-learning-language before any French learning-content package is generated.

## Decision

French is a planned later target language. It should not be activated or made content-importable until the English pilot proves the multi-target flow and a French-specific implementation goal is approved.

French content must be authored as French-learning content. It must not be translated from German, English, or Spanish source packages.

## Target-Language Scope

- Target learning language code: `fr`
- Source language for authored learning content: French
- Writing direction: left-to-right
- Script: Latin alphabet with French accents, apostrophes, elision, and spacing conventions
- Initial level system: CEFR A1-C2
- Helper translations remain separate learner support.
- No German or English learning content may appear under future `/learn/fr/...` routes.

## Country Contexts

Initial country context:

- `FR` - France

Later French country contexts may include Switzerland (`CH`), Belgium (`BE`), Canada (`CA`), or other contexts if product demand justifies them. Switzerland under French is a separate French-authored stream, not a translation of German/Switzerland content.

## Level System

Use CEFR for the first French pilot:

| Code | Friendly label | Learner meaning |
| --- | --- | --- |
| A1 | Premiers pas | Comprendre et utiliser des phrases tres simples du quotidien. |
| A2 | Bases du quotidien | Gerer des routines courantes avec des phrases simples. |
| B1 | Usage autonome | Expliquer des besoins, des projets, des opinions et des problemes courants. |
| B2 | Usage assure | Communiquer avec controle sur des sujets familiers et professionnels. |
| C1 | Maitrise avancee | Utiliser le francais avec souplesse dans les etudes, le travail et le debat public. |
| C2 | Style expert | Maitriser les nuances, la rhetorique et les discours complexes. |

## Language-Specific Design Risks

- French has gender, agreement, partitive articles, pronoun order, and liaison/elision rules that need target-specific grammar and exercise validation.
- Pronunciation cannot be inferred reliably from spelling. Listening/pronunciation metadata should support liaison, silent letters, nasal vowels, and rhythm.
- Formality and register matter early: `tu`/`vous`, polite requests, professional email conventions, and administrative language differ from German and English.
- Accents and apostrophes affect spelling and search. Search should tolerate missing accents for discovery but preserve correct French spelling.
- Country variants matter for administrative vocabulary, everyday institutions, school/work systems, and address conventions.
- French punctuation spacing and quotation conventions should be treated as language-specific formatting rules.

## First Pilot Direction

The first French pilot should be small and France-focused unless a later decision chooses Switzerland, Belgium, or Canada first.

Recommended first pilot shape:

- Course path: one A1 path
- Course lessons: 5 reviewed A1 lessons
- Grammar topics: 2-3 foundational topics
- Expressions: 8-10 practical expressions
- Exercises: 4 targeted exercises
- Writing templates: 2 short everyday templates
- Country Guidance: planned only after the French learning pilot proves routing/import/search/helper projection

## Validation Gates Before Pilot

- French remains planned and not content-importable until a reviewed French implementation goal starts.
- `/learn/fr/...` must not show German, English, or fallback content.
- Admin diagnostics must show French as planned with level metadata and planned country context only.
- French content packages must declare `targetLearningLanguageCode = fr`.
- Country Guidance packages for France must declare `countryContextCode = FR` and use French-authored source content.

## Explicit Non-Goals

- Do not generate French packages in this phase.
- Do not make French content-importable yet.
- Do not translate German or English source packages into French learning content.
- Do not add Switzerland, Belgium, or Canada guidance before separate country-context plans exist.
- Do not change MAUI/mobile UX.

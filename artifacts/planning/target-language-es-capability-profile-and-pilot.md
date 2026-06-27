# Spanish Target-Language Capability Profile And Pilot Plan

Status: planned, not content-importable.

Created: 2026-06-24.

Purpose: define Spanish as a future target-learning-language before any Spanish learning-content package is generated.

## Decision

Spanish is a planned later target language. It should not be activated or made content-importable until the English pilot proves the multi-target flow and a Spanish-specific implementation goal is approved.

Spanish content must be authored as Spanish-learning content. It must not be translated from German or English source packages.

## Target-Language Scope

- Target learning language code: `es`
- Source language for authored learning content: Spanish
- Writing direction: left-to-right
- Script: Latin alphabet with Spanish diacritics and punctuation conventions
- Initial level system: CEFR A1-C2
- Helper translations remain separate learner support.
- No German or English learning content may appear under future `/learn/es/...` routes.

## Country Contexts

Initial country context:

- `ES` - Spain

Later Spanish country contexts may include Latin American variants if product demand justifies them. Regional vocabulary, address forms, exam ecosystem, and official-process content must be planned per country or region instead of assuming one global Spanish stream.

## Level System

Use CEFR for the first Spanish pilot:

| Code | Friendly label | Learner meaning |
| --- | --- | --- |
| A1 | Primeros pasos | Comprender y usar frases muy simples en situaciones cotidianas. |
| A2 | Bases cotidianas | Resolver rutinas comunes con frases sencillas. |
| B1 | Uso independiente | Explicar necesidades, planes, opiniones y problemas cotidianos. |
| B2 | Uso seguro | Hablar y escribir con control sobre temas conocidos y profesionales. |
| C1 | Dominio avanzado | Usar el espanol con flexibilidad en estudio, trabajo y debate publico. |
| C2 | Estilo experto | Manejar matices, retorica y discurso complejo con precision. |

## Language-Specific Design Risks

- Spanish has gender and number agreement across nouns, articles, adjectives, and participles.
- `ser` versus `estar` must be taught as a core conceptual contrast, not only as two translations of "to be".
- Preterite/imperfect, subjunctive sequencing, and object pronouns require target-specific exercise validation.
- Regional variation matters. `vosotros`, `ustedes`, `voseo`, address forms, and everyday vocabulary cannot be handled as one undifferentiated global Spanish.
- Accent marks affect meaning and search. Search normalization should support accent-insensitive discovery without losing correct spelling in source content.
- Inverted question and exclamation punctuation should render and validate correctly.

## First Pilot Direction

The first Spanish pilot should be small and Spain-focused unless a later decision chooses a Latin American variant first.

Recommended first pilot shape:

- Course path: one A1 path
- Course lessons: 5 reviewed A1 lessons
- Grammar topics: 2-3 foundational topics
- Expressions: 8-10 practical expressions
- Exercises: 4 targeted exercises
- Writing templates: 2 short everyday templates
- Country Guidance: planned only after the Spanish learning pilot proves routing/import/search/helper projection

## Validation Gates Before Pilot

- Spanish remains planned and not content-importable until a reviewed Spanish implementation goal starts.
- `/learn/es/...` must not show German, English, or fallback content.
- Admin diagnostics must show Spanish as planned with level metadata and planned country context only.
- Spanish content packages must declare `targetLearningLanguageCode = es`.
- Country Guidance packages for Spain must declare `countryContextCode = ES` and use Spanish-authored source content.

## Explicit Non-Goals

- Do not generate Spanish packages in this phase.
- Do not make Spanish content-importable yet.
- Do not translate German or English source packages into Spanish learning content.
- Do not add Latin American variants before a separate country/region plan exists.
- Do not change MAUI/mobile UX.

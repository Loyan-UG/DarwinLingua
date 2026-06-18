# Life in Germany Content Plan

Status: started 2026-06-13 after renaming the former public Cultural Notes feature.

## Purpose

`Life in Germany` teaches the social, legal, civic, and everyday-system knowledge that learners need for living in Germany. It also covers the important knowledge areas behind the official `Orientierungskurs`, `Test Leben in Deutschland`, and `Einbürgerungstest`, but it is not a copied question bank.

The main learner problem is comprehension: many learners can memorize fixed German test questions before the exam, but still do not understand the underlying rights, duties, institutions, history, social expectations, and everyday rules. This feature explains those topics in the learner's own language while keeping the German source canonical.

The official tests are only one support goal. The broader product goal is to help foreign residents understand Germany, German social expectations, public systems, and everyday civic life so they can participate and integrate more quickly. Future content should not repeatedly name the exam unless the note is specifically about test orientation.

## Scope

Include:

- German social communication norms where they matter in everyday life.
- Basic rights and duties.
- Democracy, state institutions, elections, and political participation.
- Federal states, geography, and local administration.
- History and responsibility, including democratic values and remembrance culture.
- Equality, family, education, work, social system, religion, and tolerance.
- Everyday legal-administrative knowledge for appointments, forms, housing, school, work, healthcare, and public offices.
- Orientation for the fixed official tests without copying protected official question text as the core content.

Do not include:

- Full official BAMF question-bank replication as product-owned test content.
- Legal advice for individual cases.
- Stereotyping claims about language communities or countries.
- Sensitive political framing beyond neutral civic education.

## Current Technical Decision

The public feature name is `Life in Germany`.

The existing internal backing store and API remain `CulturalNotes` / `/api/catalog/cultural-notes` for now. This avoids a large table/entity migration while still allowing the feature scope to expand. The public learner route must not keep the old cultural-notes URL during active development, so route tests can catch missed rename work quickly.

Public route:

- Canonical Web route: `/life-in-germany`
- No legacy public Web route for `/cultural-notes`

## Categories

Existing cultural categories remain available:

- `du-vs-sie`
- `politeness`
- `directness`
- `small-talk`
- `workplace-culture`
- `office-communication`
- `school-kindergarten`
- `doctor-visit`
- `appointments`
- `punctuality`
- `complaints`
- `bureaucracy`
- `conversation-cafe-etiquette`

Expanded Life in Germany categories:

- `law-and-rights`
- `democracy-and-state`
- `history-and-responsibility`
- `society-and-family`
- `education-and-work`
- `religion-and-tolerance`
- `equality-and-non-discrimination`
- `federal-states-and-geography`
- `political-participation`
- `social-system`
- `exam-orientation`

## Content Progression

Although the official exam usually appears after B1-level integration-course work, explanations should be accessible. Use simple German source at lower levels and richer German source later.

- `A1-A2`: very practical everyday orientation, simple explanations, short examples.
- `B1`: core Orientierungskurs and Life in Germany concepts in clear language.
- `B2`: more precise civic, legal, and social-system explanations.
- `C1-C2`: nuanced public discourse, historical responsibility, democratic institutions, and argumentation about social topics.

## First Planning Batch: A1/A2 Foundation Candidates

These are not official exam questions. They are conceptual notes that help learners understand Germany before and during later test preparation.

1. `a1-sie-und-du-im-alltag` - Sie und du im Alltag - `du-vs-sie`
2. `a1-puenktlichkeit-bei-terminen` - Puenktlichkeit bei Terminen - `punctuality`
3. `a1-einen-termin-absagen` - Einen Termin absagen - `appointments`
4. `a1-im-amt-ruhig-und-klar-bleiben` - Im Amt ruhig und klar bleiben - `bureaucracy`
5. `a1-beim-arzt-klar-sagen-was-weh-tut` - Beim Arzt klar sagen, was weh tut - `doctor-visit`
6. `a1-schule-und-kindergarten-informieren` - Schule und Kindergarten informieren - `school-kindergarten`
7. `a1-hausordnung-und-ruhezeiten` - Hausordnung und Ruhezeiten - `law-and-rights`
8. `a1-muelltrennung-im-haus` - Muelltrennung im Haus - `social-system`
9. `a1-notruf-und-hilfe-holen` - Notruf und Hilfe holen - `law-and-rights`
10. `a1-polizei-feuerwehr-krankenwagen` - Polizei, Feuerwehr und Krankenwagen - `law-and-rights`
11. `a2-grundrechte-einfach-verstehen` - Grundrechte einfach verstehen - `law-and-rights`
12. `a2-gleichberechtigung-im-alltag` - Gleichberechtigung im Alltag - `equality-and-non-discrimination`
13. `a2-religion-und-respekt` - Religion und Respekt - `religion-and-tolerance`
14. `a2-demokratie-im-alltag` - Demokratie im Alltag - `democracy-and-state`
15. `a2-waehlen-und-mitbestimmen` - Waehlen und mitbestimmen - `political-participation`
16. `a2-bundesland-stadt-gemeinde` - Bundesland, Stadt und Gemeinde - `federal-states-and-geography`
17. `a2-geschichte-nicht-vergessen` - Geschichte nicht vergessen - `history-and-responsibility`
18. `a2-arbeit-vertrag-und-pflichten` - Arbeit, Vertrag und Pflichten - `education-and-work`
19. `a2-sozialversicherung-einfach` - Sozialversicherung einfach - `social-system`
20. `a2-leben-in-deutschland-test-verstehen` - Den Test Leben in Deutschland verstehen - `exam-orientation`

## Quality Rules

- German source is canonical.
- Helper translations must be semantic and culturally aware.
- For Persian, Arabic, Turkish, Russian, Sorani, Kurmanji, Polish, Romanian, and Albanian, examples may be adapted to what helps that language community understand the German norm, but must avoid stereotypes.
- Explain legal/civic topics as general education, not individual legal advice.
- Link to existing Course, Writing Template, Expression, Exercise, or Exam Prep content only after slug verification.
- Do not copy official question wording as the main content. If official resources are referenced, link or cite them in docs/planning, not as bulk app content.

## Completed Content

- `Life in Germany A1/A2 Foundation 01`: 20/20 notes produced and imported to `darwinlingua_shared`.
- `Life in Germany B1 Foundation 01`: 10/10 notes produced and imported to `darwinlingua_shared`.

Current imported baseline:

- `A1`: 10 notes
- `A2`: 10 notes
- `B1`: 10 notes
- Total: 30 notes

## B1 Foundation Candidates

These notes go one level deeper than A1/A2. They still avoid official question-bank copying, but they explain the core ideas behind the Orientierungskurs/Leben-in-Deutschland topics in clearer civic language.

1. `b1-grundgesetz-und-menschenwuerde` - Grundgesetz und Menschenwuerde - `law-and-rights`
2. `b1-rechtsstaat-und-gewaltenteilung` - Rechtsstaat und Gewaltenteilung - `democracy-and-state`
3. `b1-demokratische-wahlen-und-parteien` - Demokratische Wahlen und Parteien - `political-participation`
4. `b1-foederalismus-und-bundeslaender` - Foederalismus und Bundeslaender - `federal-states-and-geography`
5. `b1-soziale-marktwirtschaft-und-arbeit` - Soziale Marktwirtschaft und Arbeit - `education-and-work`
6. `b1-bildung-und-chancengleichheit` - Bildung und Chancengleichheit - `education-and-work`
7. `b1-familie-und-gleichberechtigung` - Familie und Gleichberechtigung - `equality-and-non-discrimination`
8. `b1-religionsfreiheit-und-weltanschauung` - Religionsfreiheit und Weltanschauung - `religion-and-tolerance`
9. `b1-erinnerungskultur-und-verantwortung` - Erinnerungskultur und Verantwortung - `history-and-responsibility`
10. `b1-einbuergerung-und-mitwirkung` - Einbuergerung und Mitwirkung - `exam-orientation`

## Next Content Step

Close the current A1-B1 foundation checkpoint with docs/count synchronization and a phase backup before starting the next expansion.

The next content expansion, if we continue this module, should be `Life in Germany B2 Foundation 01`. B2 should explain civic, legal, social-system, history, and participation topics more precisely than B1 while staying neutral and practical. Keep the German `context` field close to the current 512-character storage limit when needed, and put the richer explanation in `sections` so B1+ notes are not too short.

Do not copy official `Leben in Deutschland` or `Einbuergerungstest` question wording as app-owned content. Teach the underlying concepts in original explanatory notes and keep helper translations semantic, culturally aware, and non-stereotyping.

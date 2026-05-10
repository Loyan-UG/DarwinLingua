# Darwin Deutsch Talk Topics A1-A2 Expansion 2

Generated on 2026-05-10 for the main WebApi content database.

## Scope

- Generated 100 additional TalkTopic items.
- Generated 50 distinct topic groups.
- Generated only A1 and A2 content.
- Used simple everyday topics and beginner-level vocabulary references.
- Did not generate or modify B1, B2, C1, or C2 source packages.

## CEFR Distribution

| CEFR level | New TalkTopics |
| --- | ---: |
| A1 | 50 |
| A2 | 50 |

## Article Lengths

The article length rule is measured in characters, not words.

| CEFR level | Required range | Actual min | Actual max |
| --- | ---: | ---: | ---: |
| A1 | 900-1100 | 968 | 1099 |
| A2 | 1400-1600 | 1431 | 1596 |

## Validation

Package validation command:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\ContentUtilities\TestTalkTopicPackages.ps1 -ContentPath .\content\generated\talk-topics-20260510-a1-a2-expansion-2
```

Result:

- `validationOk`: `true`
- JSON files: 4
- Topic groups: 50
- TalkTopics: 100
- Distinct vocabulary lemmas: 24
- Higher-level items: 0

Vocabulary resolution command:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\ContentUtilities\TestTalkTopicVocabularyResolution.ps1 -ApiBaseUrl http://localhost:53945 -PrimaryMeaningLanguageCode en -SecondaryMeaningLanguageCode fa -MinimumResolvedPercent 100 -UnresolvedSampleLimit 10
```

Result after import:

- TalkTopics in API: 3401
- Vocabulary references: 88712
- Resolved vocabulary references: 88712
- Resolution: 100%

## Import

Import command:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\Server\Initialize-LocalServerContent.ps1 -ApiBaseUrl http://localhost:53945 -ContentPath .\content\generated\talk-topics-20260510-a1-a2-expansion-2 -AdminApiKey local-dev-admin-api-key-change-me
```

Published batch:

- `darwin-deutsch-20260510153822640`

Final API distribution after import:

| CEFR level | Total TalkTopics |
| --- | ---: |
| A1 | 201 |
| A2 | 300 |
| B1 | 750 |
| B2 | 1300 |
| C1 | 750 |
| C2 | 100 |


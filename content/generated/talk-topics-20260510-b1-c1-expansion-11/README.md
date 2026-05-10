# Darwin Deutsch Talk Topics B1-C1 Expansion 11

Generated on 2026-05-10 for the main WebApi content database.

## Scope

- Generated 100 additional TalkTopic items.
- Generated 50 distinct topic groups.
- Generated only B1 and C1 content.
- Did not generate or modify A1, A2, B2, or C2 source packages.

## CEFR Distribution

| CEFR level | New TalkTopics |
| --- | ---: |
| B1 | 50 |
| C1 | 50 |

## Validation

Package validation command:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\ContentUtilities\TestTalkTopicPackages.ps1 -ContentPath .\content\generated\talk-topics-20260510-b1-c1-expansion-11
```

Result:

- `validationOk`: `true`
- JSON files: 4
- Topic groups: 50
- TalkTopics: 100
- Distinct vocabulary lemmas: 162
- B2 items: 0

Vocabulary resolution command:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\ContentUtilities\TestTalkTopicVocabularyResolution.ps1 -ApiBaseUrl http://localhost:53945 -PrimaryMeaningLanguageCode en -SecondaryMeaningLanguageCode fa -MinimumResolvedPercent 100 -UnresolvedSampleLimit 10
```

Result after import:

- TalkTopics in API: 3201
- Vocabulary references: 85512
- Resolved vocabulary references: 85512
- Resolution: 100%

## Import

Import command:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\Server\Initialize-LocalServerContent.ps1 -ApiBaseUrl http://localhost:53945 -ContentPath .\content\generated\talk-topics-20260510-b1-c1-expansion-11 -AdminApiKey local-dev-admin-api-key-change-me
```

Published batch:

- `darwin-deutsch-20260510152110219`

Final API distribution after import:

| CEFR level | Total TalkTopics |
| --- | ---: |
| A1 | 101 |
| A2 | 200 |
| B1 | 750 |
| B2 | 1300 |
| C1 | 750 |
| C2 | 100 |


# Darwin Deutsch Talk Topics B1-C1 Expansion 2

Generated on 2026-05-10 for the main WebApi content database.

## Scope

- Generated 250 additional TalkTopic items.
- Generated 150 distinct topic groups.
- Generated only B1, B2, and C1 content.
- Did not generate or modify A1, A2, or C2 source packages.

## CEFR Distribution

| CEFR level | New TalkTopics |
| --- | ---: |
| B1 | 50 |
| B2 | 150 |
| C1 | 50 |

## Package Files

- `de-talk-topics-20260510-b1-c1-expansion-001.json`
- `de-talk-topics-20260510-b1-c1-expansion-002.json`
- `de-talk-topics-20260510-b1-c1-expansion-003.json`
- `de-talk-topics-20260510-b1-c1-expansion-004.json`
- `de-talk-topics-20260510-b1-c1-expansion-005.json`
- `de-talk-topics-20260510-b1-c1-expansion-006.json`
- `de-talk-topics-20260510-b1-c1-expansion-007.json`
- `de-talk-topics-20260510-b1-c1-expansion-008.json`
- `de-talk-topics-20260510-b1-c1-expansion-009.json`

## Validation

Package validation command:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\ContentUtilities\TestTalkTopicPackages.ps1 -ContentPath .\content\generated\talk-topics-20260510-b1-c1-expansion-2
```

Result:

- `validationOk`: `true`
- JSON files: 9
- Topic groups: 150
- TalkTopics: 250
- Distinct vocabulary lemmas: 162

Vocabulary resolution command:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\ContentUtilities\TestTalkTopicVocabularyResolution.ps1 -ApiBaseUrl http://localhost:53945 -PrimaryMeaningLanguageCode en -SecondaryMeaningLanguageCode fa -MinimumResolvedPercent 100 -UnresolvedSampleLimit 10
```

Result after import:

- TalkTopics in API: 1401
- Vocabulary references: 36012
- Resolved vocabulary references: 36012
- Resolution: 100%

## Import

Import command:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\Server\Initialize-LocalServerContent.ps1 -ApiBaseUrl http://localhost:53945 -ContentPath .\content\generated\talk-topics-20260510-b1-c1-expansion-2 -AdminApiKey local-dev-admin-api-key-change-me
```

Published batch:

- `darwin-deutsch-20260510142034404`

Final API distribution after import:

| CEFR level | Total TalkTopics |
| --- | ---: |
| A1 | 101 |
| A2 | 200 |
| B1 | 300 |
| B2 | 400 |
| C1 | 300 |
| C2 | 100 |


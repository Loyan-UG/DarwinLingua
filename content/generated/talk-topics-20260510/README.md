# Talk Topics 2026-05-10

Generated Talk Topics package set for the WebApi shared catalog.

This package set was refreshed after the first bulk import to improve deterministic
topic/category pairing and avoid mechanically repeated title endings such as
`im Alltag im Alltag`.

The current v4 package files also use article templates with German category
names in the article text instead of internal category keys.

The current v5 package files also use canonical German spelling in generated
descriptions and questions, for example `für`, `über`, `könnte`, and `würde`.

The current v6 package files add category-specific vocabulary seeds before the
shared discussion vocabulary, so important words are less repetitive and more
closely related to each TalkTopic.

## Contents

- `de-talk-topics-20260510-001.json` through `de-talk-topics-20260510-010.json`
- 300 distinct `topicGroupKey` values
- 900 TalkTopic items
- 3 CEFR variants per topic group
- 161 distinct vocabulary lemmas across the generated TalkTopic items

## Distribution

- A1: 100
- A2: 200
- B1: 200
- B2: 100
- C1: 200
- C2: 100

Content types:

- article: 210
- fact-sheet: 160
- story: 100
- opinion-text: 110
- interview: 110
- movie-summary: 110
- book-summary: 50
- debate-text: 50

## Validation

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File D:\_Projects\DarwinLingua\tools\ContentUtilities\TestTalkTopicPackages.ps1 -ContentPath D:\_Projects\DarwinLingua\content\generated\talk-topics-20260510
```

The validator checks CEFR article character ranges, German-only articles and questions, discussion question counts by type, warm-up question counts, vocabulary count ranges, content types, speaking goals, duplicate slugs, distinct topic groups, repeated mechanical title endings, leaked internal/template fragments in article text, common ASCII spellings where canonical German umlauts are expected, and minimum generated vocabulary diversity.

## Import

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File D:\_Projects\DarwinLingua\tools\Server\Initialize-LocalServerContent.ps1 -ApiBaseUrl http://localhost:53945 -ContentPath D:\_Projects\DarwinLingua\content\generated\talk-topics-20260510 -AdminApiKey local-dev-admin-api-key-change-me
```

Talk Topic articles and questions intentionally do not contain translations. Important words are references only; meanings are resolved from the Word Catalog where possible.

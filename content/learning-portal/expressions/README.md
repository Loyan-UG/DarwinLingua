# Everyday Expressions Content

Official Everyday Expressions packages live in `content/learning-portal/expressions/packages`.

Current rule: create and validate a small pilot package first. Bulk Expressions generation stays blocked until import, Web API, Web rendering, Unified Search, admin quality reporting, and localization quality gates pass with real package data.

Current official small batches:

- `expressions-a1-a2-core-pilot-v1.json`
- `expressions-a1-a2-core-01-v1.json`
- `expressions-a1-core-v1.json`
- `expressions-a2-core-v1.json`
- `expressions-b1-core-v1.json`
- `expressions-b2-core-v1.json`
- `expressions-b2-core-02-v1.json`
- `expressions-b2-core-03-v1.json`
- `expressions-c1-core-v1.json`
- `expressions-c1-core-02-v1.json`
- `expressions-c2-core-v1.json`
- `expressions-c2-core-02-v1.json`

Before creating or regenerating Expressions content, review:

- `docs/78-Expression-Content-Package-Contract.md`
- `docs/84-Content-Generation-Lessons-Learned.md`

Quality rules:

- Do not use Expressions as a general sentence bank. Published entries must be idioms, semi-idiomatic phrases, pragmatic formulas, polite/social formulas, proverbs, cultural phrases, false friends, regional phrases, or properly gated sensitive language.
- Published `ordinary-literal` entries are not allowed.
- Include `meaningTransparency`, `teachingReason`, `safetyRating`, `minimumAge`, and `requiresAdultAccess` for new official batches.
- Use only controlled `expressionType` and `register` values from the contract.
- Include all active Learning Portal learner languages: `en`, `fa`, `ar`, `tr`, `ru`, `ckb`, `kmr`, `pl`, `ro`, `sq`.
- Do not use English fallback text in non-English localized fields.
- Do not repeat generic learner-facing explanations across entries.
- Keep `linkedWords` as references only: `lemma`, optional `wordSlug`, and optional `sortOrder`.
- Risky, rude, slang-heavy, or easy-to-misuse expressions require warning text and localized warning translations.

Validation:

```powershell
node tools/Content/Validate-ExpressionPilot.js content/learning-portal/expressions/packages/expressions-a1-a2-core-pilot-v1.json
node tools/Content/Validate-ExpressionPilot.js content/learning-portal/expressions/packages/expressions-a1-a2-core-01-v1.json
node tools/Content/Validate-ExpressionPilot.js content/learning-portal/expressions/packages/expressions-a1-core-v1.json
node tools/Content/Validate-ExpressionPilot.js content/learning-portal/expressions/packages/expressions-a2-core-v1.json
node tools/Content/Validate-ExpressionPilot.js content/learning-portal/expressions/packages/expressions-b1-core-v1.json
node tools/Content/Validate-ExpressionPilot.js content/learning-portal/expressions/packages/expressions-b2-core-v1.json
node tools/Content/Validate-ExpressionPilot.js content/learning-portal/expressions/packages/expressions-b2-core-02-v1.json
node tools/Content/Validate-ExpressionPilot.js content/learning-portal/expressions/packages/expressions-b2-core-03-v1.json
node tools/Content/Validate-ExpressionPilot.js content/learning-portal/expressions/packages/expressions-c1-core-v1.json
node tools/Content/Validate-ExpressionPilot.js content/learning-portal/expressions/packages/expressions-c1-core-02-v1.json
node tools/Content/Validate-ExpressionPilot.js content/learning-portal/expressions/packages/expressions-c2-core-v1.json
node tools/Content/Validate-ExpressionPilot.js content/learning-portal/expressions/packages/expressions-c2-core-02-v1.json
node tools/Content/Audit-ExpressionContentQuality.js
dotnet test tests/Modules/ContentOps/DarwinLingua.ContentOps.Infrastructure.Tests/DarwinLingua.ContentOps.Infrastructure.Tests.csproj
dotnet test tests/Modules/ContentOps/DarwinLingua.ContentOps.Application.Tests/DarwinLingua.ContentOps.Application.Tests.csproj
```

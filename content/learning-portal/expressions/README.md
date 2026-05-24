# Everyday Expressions Content

Official Everyday Expressions packages live in `content/learning-portal/expressions/packages`.

Current rule: create and validate a small pilot package first. Bulk Expressions generation stays blocked until import, Web API, Web rendering, Unified Search, admin quality reporting, and localization quality gates pass with real package data.

Before creating or regenerating Expressions content, review:

- `docs/78-Expression-Content-Package-Contract.md`
- `docs/84-Content-Generation-Lessons-Learned.md`

Quality rules:

- Use only controlled `expressionType` and `register` values from the contract.
- Include all active Learning Portal learner languages: `en`, `fa`, `ar`, `tr`, `ru`, `ckb`, `kmr`, `pl`, `ro`, `sq`.
- Do not use English fallback text in non-English localized fields.
- Do not repeat generic learner-facing explanations across entries.
- Keep `linkedWords` as references only: `lemma`, optional `wordSlug`, and optional `sortOrder`.
- Risky, rude, slang-heavy, or easy-to-misuse expressions require warning text and localized warning translations.

Validation:

```powershell
node tools/Content/Validate-ExpressionPilot.js content/learning-portal/expressions/packages/expressions-a1-a2-core-pilot-v1.json
dotnet test tests/Modules/ContentOps/DarwinLingua.ContentOps.Infrastructure.Tests/DarwinLingua.ContentOps.Infrastructure.Tests.csproj
dotnet test tests/Modules/ContentOps/DarwinLingua.ContentOps.Application.Tests/DarwinLingua.ContentOps.Application.Tests.csproj
```

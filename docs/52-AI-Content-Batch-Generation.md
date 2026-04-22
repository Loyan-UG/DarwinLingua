# AI Content Batch Generation

This document defines the practical workflow for generating large German vocabulary batches for DarwinLingua without breaking the current import pipeline.

## Goal

Use small, validated JSON batches to grow the seed database incrementally.

Recommended batch size:

- `30` entries per file for AI-generated content
- Import and validate each batch before generating the next one

Acceptable practical range:

- `25` to `35` entries per file when a domain slice is cleaner that way

This keeps failures localized and makes duplicate or schema issues easier to fix.

## Current Constraints

The current Phase 1 import pipeline only accepts:

- entry language: `de`
- meaning languages that already exist in the reference data
- topic keys that already exist in the database

Supported meaning languages currently include:

- `en`
- `fa`
- `ru`
- `ar`
- `pl`
- `tr`
- `ro`
- `sq`
- `ckb`
- `kmr`

Current active topic keys in the packaged seed database:

- `everyday-life`
- `housing`
- `shopping`
- `work-and-jobs`
- `appointments-and-health`

For ERP, workplace, software, CRM, warehouse, purchasing, reporting, and technical-discussion content, use:

- primary topic: `work-and-jobs`
- optional secondary topic: `shopping` when the word is directly tied to ordering, suppliers, invoicing, delivery, or inventory movement

Use `contextLabels` and `usageLabels` for finer classification such as:

- `erp`
- `crm`
- `sales`
- `procurement`
- `warehouse`
- `inventory`
- `finance`
- `software`
- `architecture`
- `reporting`
- `data-quality`
- `workflow`
- `support`

These labels must be lowercase kebab-case.

## Required Entry Shape

Each entry must contain:

- `word`
- `language`
- `cefrLevel`
- `partOfSpeech`
- `lexicalForms`
- `topics`
- `meanings`
- `examples`

Recommended minimum conventions for AI batches:

- always include `lexicalForms`, even for a single part of speech
- for nouns, keep top-level `article` and `plural` equal to the primary lexical form
- for verbs, keep top-level `infinitive` equal to the primary lexical form
- include at least `en` and `fa` meaning translations
- include at least two domain-relevant examples with both `en` and `fa` translations
- keep examples domain-relevant when possible

## Recommended Package Template

```json
{
  "packageVersion": "1.0",
  "packageId": "de-work-erp-b1-c1-YYYYMMDD-XX",
  "packageName": "German Work ERP B1-C1 Batch 00X",
  "source": "AiAssisted",
  "defaultMeaningLanguages": ["en", "fa"],
  "entries": []
}
```

Package ID convention:

- prefix: `de-work-erp-b1-c1`
- date: `YYYYMMDD`
- batch suffix: `01`, `02`, `03`, ...

Example file location:

- [de-work-erp-b1-c1-batch-001.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-001.json)
- [de-work-erp-b1-c1-batch-002.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-002.json)
- [de-work-erp-b1-c1-batch-003.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-003.json)
- [de-work-erp-b1-c1-batch-004.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-004.json)
- [de-work-erp-b1-c1-batch-005.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-005.json)
- [de-work-erp-b1-c1-batch-006.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-006.json)
- [de-work-erp-b1-c1-batch-007.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-007.json)
- [de-work-erp-b1-c1-batch-008.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-008.json)
- [de-work-erp-b1-c1-batch-009.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-009.json)
- [de-work-erp-b1-c1-batch-010.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-010.json)
- [de-work-erp-b1-c1-batch-011.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-011.json)
- [de-work-erp-b1-c1-batch-012.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-012.json)
- [de-work-erp-b1-c1-batch-013.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-013.json)
- [de-work-erp-b1-c1-batch-014.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-014.json)
- [de-work-erp-b1-c1-batch-015.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-015.json)
- [de-work-erp-b1-c1-batch-016.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-016.json)
- [de-work-erp-b1-c1-batch-017.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-017.json)
- [de-work-erp-b1-c1-batch-018.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-018.json)
- [de-work-erp-b1-c1-batch-019.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-019.json)
- [de-work-erp-b1-c1-batch-020.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-020.json)
- [de-work-erp-b1-c1-batch-021.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-021.json)
- [de-work-erp-b1-c1-batch-022.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-022.json)
- [de-work-erp-b1-c1-batch-023.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-023.json)
- [de-work-erp-b1-c1-batch-024.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-024.json)

## CEFR Content Strategy

For workplace German aimed at a developer working on ERP systems:

- `B1`: common operational words used in meetings, tasks, orders, invoices, warehouse actions, and customer communication
- `B2`: process, integration, coordination, reporting, data, and approval vocabulary
- `C1`: architecture, traceability, configuration, permissions, resilience, optimization, and enterprise-process vocabulary

Priority domains:

1. CRM and customer communication
2. sales and quotations
3. warehouse and inventory
4. purchasing and suppliers
5. finance and accounting
6. ERP configuration and permissions
7. software discussions with German colleagues

## Import Command

Import a specific batch into the packaged MAUI seed database:

```powershell
dotnet run --project D:\_Projects\DarwinLingua\src\Apps\DarwinLingua.ImportTool\DarwinLingua.ImportTool.csproj -- D:\_Projects\DarwinLingua\content\generated\de-work-erp-b1-c1-batch-001.json
```

Import a specific batch into the shared PostgreSQL catalog used by `WebApi`:

```powershell
dotnet run --project D:\_Projects\DarwinLingua\src\Apps\DarwinLingua.ImportTool\DarwinLingua.ImportTool.csproj -- --target shared --yes D:\_Projects\DarwinLingua\content\generated\de-work-erp-b1-c1-batch-001.json
```

Important:

- the default target is the packaged seed database
- use `--target shared` to import into the shared server-side PostgreSQL catalog
- new app installs copy the seed database on first launch
- already-installed apps keep their existing local database until app data is cleared or the app is reinstalled
- the website does not use the seed database for catalog content; it uses `WebApi`, which reads from the shared PostgreSQL catalog

## Validation Rules To Respect

Before importing a batch, ensure:

- `packageVersion` is exactly `1.0`
- `partOfSpeech` matches the primary item in `lexicalForms`
- topics are valid
- meanings do not repeat the same language
- each entry has at least `2` examples
- each example contains at least one valid translation
- labels are kebab-case
- no duplicate lemma + primary part-of-speech + CEFR combination is generated within the same batch

## Recommended AI Prompting Rules

When asking an AI to generate the next batch, specify:

- generate exactly `30` entries
- focus on German vocabulary for ERP, CRM, sales, procurement, warehouse, finance, and software teamwork
- use CEFR levels only from `B1`, `B2`, and `C1`
- use topic `work-and-jobs`, and optionally `shopping` where appropriate
- include meanings in `en` and `fa`
- include two domain-relevant examples with `en` and `fa` translations
- use canonical import JSON only
- avoid duplicates against already-generated batches

## Repair Tool

When existing generated batches need to be normalized to the minimum-example rule, use:

```powershell
dotnet run --project D:\_Projects\DarwinLingua\tools\ContentUtilities\DarwinLingua.ContentTools\DarwinLingua.ContentTools.csproj -- --content-path D:\_Projects\DarwinLingua\content\generated --minimum-examples 2
```

If the secondary examples need to be regenerated from scratch while preserving the first example in every entry, use:

```powershell
dotnet run --project D:\_Projects\DarwinLingua\tools\ContentUtilities\DarwinLingua.ContentTools\DarwinLingua.ContentTools.csproj -- --content-path D:\_Projects\DarwinLingua\content\generated --minimum-examples 2 --rewrite-secondary-examples
```

Current normalization rule for the repair tool:

- preserve the first example that was originally curated/generated
- regenerate only the secondary examples
- keep the secondary examples aligned with `partOfSpeech`, `contextLabels`, and the entry `meanings`

## Current Progress

The following AI-generated ERP/workplace batches already exist and were imported successfully:

- [de-work-erp-b1-c1-batch-001.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-001.json)
- [de-work-erp-b1-c1-batch-002.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-002.json)
- [de-work-erp-b1-c1-batch-003.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-003.json)
- [de-work-erp-b1-c1-batch-004.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-004.json)
- [de-work-erp-b1-c1-batch-005.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-005.json)
- [de-work-erp-b1-c1-batch-006.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-006.json)
- [de-work-erp-b1-c1-batch-007.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-007.json)
- [de-work-erp-b1-c1-batch-008.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-008.json)
- [de-work-erp-b1-c1-batch-009.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-009.json)
- [de-work-erp-b1-c1-batch-010.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-010.json)
- [de-work-erp-b1-c1-batch-011.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-011.json)
- [de-work-erp-b1-c1-batch-012.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-012.json)
- [de-work-erp-b1-c1-batch-013.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-013.json)
- [de-work-erp-b1-c1-batch-014.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-014.json)
- [de-work-erp-b1-c1-batch-015.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-015.json)
- [de-work-erp-b1-c1-batch-016.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-016.json)
- [de-work-erp-b1-c1-batch-017.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-017.json)
- [de-work-erp-b1-c1-batch-018.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-018.json)
- [de-work-erp-b1-c1-batch-019.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-019.json)
- [de-work-erp-b1-c1-batch-020.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-020.json)
- [de-work-erp-b1-c1-batch-021.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-021.json)
- [de-work-erp-b1-c1-batch-022.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-022.json)
- [de-work-erp-b1-c1-batch-023.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-023.json)
- [de-work-erp-b1-c1-batch-024.json](/D:/_Projects/DarwinLingua/content/generated/de-work-erp-b1-c1-batch-024.json)

Current verified shared-catalog state after importing the existing generated batches:

- `25` content packages
- `480` words
- `439` lexical-form records

These batches are the current baseline pattern for continuing toward `1000+` words.

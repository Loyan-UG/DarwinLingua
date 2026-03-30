# Mobile Validation Bundle Runbook

## Purpose

This runbook explains how to prepare one bundle for the remaining manual mobile validation work across Phase 1, Phase 2, Phase 3, and the Phase 5 remote-update flow.

Use it when you want a single evidence folder per build under test instead of opening the worksheets directly from `docs/`.

---

## Script

Use:

```powershell
pwsh ./tools/Mobile/Start-MobileValidationBundle.ps1
```

Optional parameters:

```powershell
pwsh ./tools/Mobile/Start-MobileValidationBundle.ps1 -BuildCommit <commit> -BundleLabel <label>
```

---

## Output

The script creates a timestamped bundle under:

```text
artifacts/mobile-validation/<bundle-label>/
```

It includes:

- `Phase1ManualValidationWorksheet.md`
- `Phase2PracticeValidationWorksheet.md`
- `Phase3MobileUxValidationWorksheet.md`
- `Phase5RemoteUpdateValidationWorksheet.md`
- `MobileValidationSummary.md`

---

## Recommended Execution Order

1. Run the Phase 1 worksheet.
2. Run the Phase 2 Practice worksheet.
3. Run the Phase 3 Mobile UX worksheet.
4. Run the Phase 5 remote-update worksheet.
5. Record the final recommendation and accepted issues in `MobileValidationSummary.md`.

---

## When To Use

Use this runbook when:

- the build is ready for target-device validation
- the same build needs to be validated across multiple mobile phases
- you want one folder to hand to a tester or release owner

Do not treat the bundle as completed validation evidence until the copied worksheets have been filled in.

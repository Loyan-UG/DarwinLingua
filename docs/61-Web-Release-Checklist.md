# Web Release Checklist

## Purpose

This checklist is the release gate for `DarwinLingua.Web`.

Use it for both the learner-facing root site and the admin area.

---

## A. Build And Test

- [ ] release commit selected
- [ ] solution build succeeded
- [ ] automated tests succeeded
- [ ] web-specific structural tests succeeded

---

## B. Learner Web

- [ ] learner shell renders correctly
- [ ] browse by CEFR works
- [ ] browse by topic works
- [ ] search works
- [ ] word detail works
- [ ] favorites work
- [ ] recent activity works
- [ ] settings work

---

## C. Admin Web

- [ ] admin layout renders correctly
- [ ] admin overview loads
- [ ] publishing page loads
- [ ] diagnostics page loads
- [ ] admin/learner separation is preserved

---

## D. PWA

- [ ] manifest is delivered
- [ ] service worker registers
- [ ] install flow validated on target browsers
- Validation evidence may be attached from `artifacts/installability-report.json` and `artifacts/installability-report-android.json`

---

## E. Operational Readiness

- [ ] production configuration applied
- [ ] database connectivity verified
- [ ] security headers verified
- [ ] logging baseline verified
- [ ] rollback owner identified

---

## F. Sign-Off

- Release owner:
- Validation owner:
- Known accepted issues:
- Final release recommendation:

# Web Browse/Search/Detail Validation Worksheet

## Purpose

This worksheet is the manual validation companion for the learner-facing web flows in `DarwinLingua.Web`.

Use it after automated checks pass on the same commit and the shared catalog is populated.

---

## Build Under Test

- Build commit:
- Validation date:
- Validator:
- Environment:
- Browser:
- Shared catalog connection in use:

---

## Preconditions

- [ ] `DarwinLingua.Web` starts successfully
- [ ] learner navigation shell renders
- [ ] shared catalog contains active words and topics
- [ ] at least one word has rich metadata
- [ ] the current profile has a preferred meaning language set

---

## A. Browse By CEFR

- [ ] open `Browse`
- [ ] open at least three CEFR levels
- [ ] verify paging works with `Previous` and `Next page`
- [ ] verify empty states render cleanly when a slice is empty
- Result:
- Notes:

---

## B. Browse By Topic

- [ ] open at least three topics
- [ ] verify topic chips navigate to the expected list
- [ ] verify topic pages preserve consistent card layout
- [ ] verify topic links from word detail return to valid pages
- Result:
- Notes:

---

## C. Search

- [ ] search exact lemma
- [ ] search partial/prefix lemma
- [ ] search a no-result value
- [ ] verify htmx updates replace only the results panel
- [ ] verify no stale results remain after repeated queries
- Result:
- Notes:

---

## D. Word Detail

- [ ] open at least one noun, one verb, and one richer multi-metadata word
- [ ] verify word forms, examples, labels, grammar notes, collocations, and relations render when present
- [ ] verify recent view count increments after reopening a word
- [ ] verify `Add to favorites`, `Mark known`, and `Mark difficult` update without a full reload
- [ ] verify the Topics section links back into browse pages
- Result:
- Notes:

---

## E. Favorites And Recent

- [ ] add at least two favorites
- [ ] verify favorites appear in `Favorites`
- [ ] open `Recent`
- [ ] verify recently viewed words appear in descending last-viewed order
- [ ] verify known/difficult chips reflect the current state
- Result:
- Notes:

---

## F. Sign-Off

- Known issues accepted:
- Follow-up tasks filed:
- Final learner-web readiness recommendation:

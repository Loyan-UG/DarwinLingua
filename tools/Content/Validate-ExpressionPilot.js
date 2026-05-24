#!/usr/bin/env node

const fs = require("fs");
const path = require("path");

const packagePath = process.argv[2] || "content/learning-portal/expressions/packages/expressions-a1-a2-core-pilot-v1.json";
const targetLanguages = ["en", "fa", "ar", "tr", "ru", "ckb", "kmr", "pl", "ro", "sq"];
const expressionTypes = new Set([
  "idiom",
  "colloquial-phrase",
  "proverb",
  "fixed-expression",
  "slang",
  "cultural-phrase",
  "false-friend",
  "regional-expression",
  "polite-formula",
  "warning-phrase"
]);
const registers = new Set([
  "formal",
  "informal",
  "neutral",
  "colloquial",
  "slang",
  "rude",
  "polite",
  "workplace-safe",
  "friends-only",
  "regional"
]);
const riskyRegisters = new Set(["slang", "rude", "friends-only"]);
const riskyTypes = new Set(["slang", "warning-phrase"]);
const kebabCase = /^[a-z0-9]+(?:-[a-z0-9]+)*$/;
const processLeakPattern = /\b(checkpoint|do not continue|validate this|qa note|prompt instruction|internal note|todo|tba|lorem ipsum)\b|نقطه بررسی|ادامه نده|یادداشت داخلی|دستور پرامپت/i;
const englishFallbackPhrases = [
  "this is a useful phrase",
  "use this expression",
  "for persian speakers",
  "arabic learners",
  "turkish learners",
  "russian learners",
  "kurdish learners"
];

function fail(message) {
  issues.push(message);
}

function text(value) {
  return typeof value === "string" ? value.trim() : "";
}

function normalize(value) {
  return text(value).replace(/\s+/g, " ").toLowerCase();
}

function hasArabicScript(value) {
  return /[\u0600-\u06FF]/.test(value);
}

function hasCyrillic(value) {
  return /[\u0400-\u04FF]/.test(value);
}

function isLikelyEnglishFallback(value) {
  const normalized = normalize(value);
  if (!normalized) return true;
  if (englishFallbackPhrases.some(phrase => normalized.includes(phrase))) return true;
  const words = normalized.match(/[a-z]+/g) || [];
  if (words.length < 5) return false;
  const englishSignals = new Set(["the", "and", "or", "to", "with", "when", "use", "this", "that", "you", "your", "please", "can", "not", "formal"]);
  const hits = words.filter(word => englishSignals.has(word)).length;
  return hits >= 3;
}

function validateLanguageText(language, value, location, englishReference) {
  const current = text(value);
  if (!current) {
    fail(`${location} is empty.`);
    return;
  }

  if (processLeakPattern.test(current)) {
    fail(`${location} contains internal process/QA text.`);
  }

  if (language !== "en" && englishReference && normalize(current) === normalize(englishReference)) {
    fail(`${location} duplicates the English text.`);
  }

  if (["fa", "ar", "ckb"].includes(language) && !hasArabicScript(current)) {
    fail(`${location} does not appear to use Arabic-script text.`);
  }

  if (language === "ru" && !hasCyrillic(current)) {
    fail(`${location} does not appear to use Cyrillic text.`);
  }

  if (language !== "en" && isLikelyEnglishFallback(current)) {
    fail(`${location} looks like English fallback text.`);
  }
}

function requireTranslations(translations, location, englishReferenceByLanguage = new Map()) {
  if (!Array.isArray(translations)) {
    fail(`${location} must be an array.`);
    return;
  }

  const seen = new Set();
  for (const translation of translations) {
    const language = text(translation?.language).toLowerCase();
    if (!targetLanguages.includes(language)) {
      fail(`${location} contains unsupported language '${language || "(missing)"}'.`);
      continue;
    }
    if (seen.has(language)) {
      fail(`${location} contains duplicate language '${language}'.`);
    }
    seen.add(language);
    validateLanguageText(language, translation?.text, `${location}.${language}`, englishReferenceByLanguage.get(language) || englishReferenceByLanguage.get("en"));
  }

  for (const language of targetLanguages) {
    if (!seen.has(language)) {
      fail(`${location} is missing '${language}'.`);
    }
  }
}

const absolutePath = path.resolve(process.cwd(), packagePath);
const issues = [];
let document;

try {
  document = JSON.parse(fs.readFileSync(absolutePath, "utf8").replace(/^\uFEFF/, ""));
} catch (error) {
  console.error(`Invalid JSON: ${error.message}`);
  process.exit(1);
}

if (document.packageVersion !== "1.0") fail("packageVersion must be 1.0.");
if (!kebabCase.test(text(document.packageId))) fail("packageId must use lowercase kebab-case.");
if (!Array.isArray(document.entries)) fail("entries array is required, even when empty.");
if (!Array.isArray(document.expressionEntries)) fail("expressionEntries array is required.");

const languages = Array.isArray(document.defaultMeaningLanguages) ? document.defaultMeaningLanguages.map(item => text(item).toLowerCase()) : [];
if (targetLanguages.join("|") !== languages.join("|")) {
  fail(`defaultMeaningLanguages must be exactly ${targetLanguages.join(", ")}.`);
}

const expressions = document.expressionEntries || [];
if (expressions.length < 12 || expressions.length > 40) {
  fail(`Expression package must contain 12-40 expressions; found ${expressions.length}.`);
}

const slugs = new Set();
const localizedSentenceCounts = new Map();
const usageCounts = new Map();

for (const [index, expression] of expressions.entries()) {
  const prefix = `expressionEntries[${index + 1}]`;
  const slug = text(expression.slug);
  if (!kebabCase.test(slug)) fail(`${prefix}.slug must use lowercase kebab-case.`);
  if (slugs.has(slug)) fail(`${prefix}.slug '${slug}' is duplicated.`);
  slugs.add(slug);

  for (const field of ["expressionText", "actualMeaningText", "cefrLevel", "expressionType", "register", "category"]) {
    if (!text(expression[field])) fail(`${prefix}.${field} is required.`);
  }

  if (!expressionTypes.has(text(expression.expressionType))) fail(`${prefix}.expressionType '${expression.expressionType}' is unsupported.`);
  if (!registers.has(text(expression.register))) fail(`${prefix}.register '${expression.register}' is unsupported.`);
  if (!kebabCase.test(text(expression.category))) fail(`${prefix}.category must use lowercase kebab-case.`);

  for (const field of ["expressionText", "literalMeaningText", "actualMeaningText", "usageExplanation"]) {
    if (text(expression[field]) && processLeakPattern.test(expression[field])) {
      fail(`${prefix}.${field} contains internal process/QA text.`);
    }
  }

  const risky = expression.isRisky === true || riskyRegisters.has(text(expression.register)) || riskyTypes.has(text(expression.expressionType));
  if (risky && (!Array.isArray(expression.warnings) || expression.warnings.length === 0)) {
    fail(`${prefix} is risky but has no warnings.`);
  }

  if (!Array.isArray(expression.meanings)) {
    fail(`${prefix}.meanings must be an array.`);
  } else {
    const seenMeanings = new Set();
    const englishMeaning = expression.meanings.find(item => text(item?.language).toLowerCase() === "en") || {};
    for (const meaning of expression.meanings) {
      const language = text(meaning?.language).toLowerCase();
      if (!targetLanguages.includes(language)) {
        fail(`${prefix}.meanings contains unsupported language '${language || "(missing)"}'.`);
        continue;
      }
      if (seenMeanings.has(language)) fail(`${prefix}.meanings contains duplicate language '${language}'.`);
      seenMeanings.add(language);
      validateLanguageText(language, meaning.actualMeaningText, `${prefix}.meanings.${language}.actualMeaningText`, englishMeaning.actualMeaningText);
      if (meaning.usageExplanation !== undefined) {
        validateLanguageText(language, meaning.usageExplanation, `${prefix}.meanings.${language}.usageExplanation`, englishMeaning.usageExplanation);
        const usageKey = `${language}:${normalize(meaning.usageExplanation)}`;
        usageCounts.set(usageKey, (usageCounts.get(usageKey) || 0) + 1);
      }

      const sentenceKey = `${language}:${normalize(meaning.actualMeaningText)}`;
      localizedSentenceCounts.set(sentenceKey, (localizedSentenceCounts.get(sentenceKey) || 0) + 1);
    }
    for (const language of targetLanguages) {
      if (!seenMeanings.has(language)) fail(`${prefix}.meanings is missing '${language}'.`);
    }
  }

  if (!Array.isArray(expression.examples) || expression.examples.length === 0) {
    fail(`${prefix}.examples must contain at least one item.`);
  } else {
    for (const [exampleIndex, example] of expression.examples.entries()) {
      if (!text(example.germanText)) fail(`${prefix}.examples[${exampleIndex + 1}].germanText is required.`);
      requireTranslations(example.translations, `${prefix}.examples[${exampleIndex + 1}].translations`);
    }
  }

  if (Array.isArray(expression.warnings)) {
    for (const [warningIndex, warning] of expression.warnings.entries()) {
      if (!kebabCase.test(text(warning.warningType))) fail(`${prefix}.warnings[${warningIndex + 1}].warningType must use lowercase kebab-case.`);
      validateLanguageText("en", warning.text, `${prefix}.warnings[${warningIndex + 1}].text`, warning.text);
      const englishWarning = new Map([["en", warning.text]]);
      requireTranslations(warning.translations, `${prefix}.warnings[${warningIndex + 1}].translations`, englishWarning);
    }
  }

  if (Array.isArray(expression.linkedWords)) {
    for (const [wordIndex, word] of expression.linkedWords.entries()) {
      const allowedKeys = new Set(["lemma", "wordSlug", "sortOrder"]);
      for (const key of Object.keys(word || {})) {
        if (!allowedKeys.has(key)) {
          fail(`${prefix}.linkedWords[${wordIndex + 1}] contains unsupported key '${key}'.`);
        }
      }
      if (!text(word?.lemma)) fail(`${prefix}.linkedWords[${wordIndex + 1}].lemma is required.`);
      if (word?.wordSlug !== undefined && !kebabCase.test(text(word.wordSlug))) {
        fail(`${prefix}.linkedWords[${wordIndex + 1}].wordSlug must use lowercase kebab-case.`);
      }
    }
  }

  for (const field of ["relatedExpressionSlugs", "linkedExerciseSlugs"]) {
    if (Array.isArray(expression[field])) {
      for (const slugValue of expression[field]) {
        if (!kebabCase.test(text(slugValue))) fail(`${prefix}.${field} contains malformed slug '${slugValue}'.`);
      }
    }
  }
}

for (const [key, count] of localizedSentenceCounts) {
  if (count > 1) fail(`Localized actual meaning is repeated ${count} times: ${key}`);
}

for (const [key, count] of usageCounts) {
  if (count > 1) fail(`Usage explanation is repeated ${count} times: ${key}`);
}

if (issues.length > 0) {
  console.error(`Expression package validation failed with ${issues.length} issue(s):`);
  for (const issue of issues) console.error(`- ${issue}`);
  process.exit(1);
}

console.log(`Expression package validation passed: ${expressions.length} expressions, ${targetLanguages.length} languages.`);

#!/usr/bin/env node

const fs = require("fs");
const path = require("path");

const repoRoot = path.resolve(__dirname, "..", "..");
const packagesDir = path.join(repoRoot, "content", "learning-portal", "expressions", "packages");
const reportPath = path.join(repoRoot, "artifacts", "validation", "expression-content-quality-report.md");

const allowedTransparency = new Set([
  "non-literal",
  "semi-idiomatic",
  "pragmatic-formula",
  "literal-fixed-formula",
  "ordinary-literal",
]);
const allowedSafetyRatings = new Set([
  "general",
  "mild-rude",
  "strong-rude",
  "sexual-context",
  "explicit-adult",
  "discriminatory-slur",
  "politically-sensitive",
]);
const allowedMinimumAges = new Set([0, 16, 18]);
const allowedAdultContentCategories = new Set([
  "rude-slang",
  "sexual-language",
  "explicit-sexual-language",
  "discriminatory-language",
  "politically-sensitive-language",
]);
const requiredLanguages = ["en", "fa", "ar", "tr", "ru", "ckb", "kmr", "pl", "ro", "sq"];
const processLeakPattern = /\b(checkpoint|do not continue|validate this|qa note|prompt instruction|internal note)\b/i;
const knownOrdinaryLiteralSlugs = new Set([
  "die-heizung-funktioniert-nicht",
  "ich-brauche-einen-termin",
  "ich-gehe-zum-arzt",
]);

function readJson(filePath) {
  return JSON.parse(fs.readFileSync(filePath, "utf8"));
}

function isEnglishLike(text) {
  return typeof text === "string" && /[a-z]{4,}\s+[a-z]{3,}/i.test(text) && !/[اآبپتثجچحخدذرزژسشصضطظعغفقکگلمنوهیەێڕڵۆŞşĞğİıÇçÖöÜüА-Яа-яĄąĆćĘęŁłŃńÓóŚśŹźŻżĂăÂâÎîȘșȚțËëÇç]/.test(text);
}

function collectTexts(entry) {
  const texts = [
    entry.expressionText,
    entry.literalMeaningText,
    entry.actualMeaningText,
    entry.usageExplanation,
    entry.teachingReason,
  ];
  for (const meaning of entry.meanings || []) {
    texts.push(meaning.actualMeaningText, meaning.literalMeaningText, meaning.usageExplanation);
  }
  for (const example of entry.examples || []) {
    texts.push(example.germanText, example.note);
    for (const translation of example.translations || []) texts.push(translation.text);
  }
  for (const warning of entry.warnings || []) {
    texts.push(warning.text);
    for (const translation of warning.translations || []) texts.push(translation.text);
  }
  return texts.filter(Boolean);
}

function auditEntry(packageId, entry, issues) {
  const owner = `${packageId}:${entry.slug || "(missing-slug)"}`;
  if (!entry.slug) issues.push([owner, "P0", "Missing slug"]);
  if (!entry.meaningTransparency) {
    issues.push([owner, "P1", "Missing meaningTransparency metadata"]);
  } else if (!allowedTransparency.has(entry.meaningTransparency)) {
    issues.push([owner, "P0", `Unsupported meaningTransparency '${entry.meaningTransparency}'`]);
  }
  if ((entry.meaningTransparency === "ordinary-literal" || knownOrdinaryLiteralSlugs.has(entry.slug)) && entry.isPublished !== false) {
    issues.push([owner, "P0", "Published ordinary-literal expression leakage"]);
  }
  if ((entry.meaningTransparency === "non-literal" || entry.meaningTransparency === "semi-idiomatic") && !entry.literalMeaningText) {
    issues.push([owner, "P0", "Missing literalMeaningText for non-literal/semi-idiomatic expression"]);
  }
  if (entry.meaningTransparency && !entry.teachingReason) {
    issues.push([owner, "P1", "Missing teachingReason"]);
  }
  if ((entry.examples || []).length < 2 && entry.isPublished !== false) {
    issues.push([owner, "P1", "Published expression has fewer than two examples"]);
  }
  const safetyRating = entry.safetyRating || "general";
  if (!allowedSafetyRatings.has(safetyRating)) {
    issues.push([owner, "P0", `Unsupported safetyRating '${safetyRating}'`]);
  }
  if (!allowedMinimumAges.has(entry.minimumAge ?? 0)) {
    issues.push([owner, "P0", "Unsupported minimumAge; expected 0, 16, or 18"]);
  }
  if (entry.requiresAdultAccess === true && (entry.minimumAge ?? 0) < 18) {
    issues.push([owner, "P0", "Adult-access expression must use minimumAge 18"]);
  }
  if (entry.adultContentCategory !== undefined && !allowedAdultContentCategories.has(entry.adultContentCategory)) {
    issues.push([owner, "P0", `Unsupported adultContentCategory '${entry.adultContentCategory}'`]);
  }
  const warnings = [...(entry.warnings || []), ...(entry.contentWarnings || [])];
  const requiresWarning = entry.isRisky === true || ["mild-rude", "strong-rude", "sexual-context", "explicit-adult", "discriminatory-slur", "politically-sensitive"].includes(safetyRating);
  if (requiresWarning && !warnings.some(warning => typeof warning.text === "string" && warning.text.trim())) {
    issues.push([owner, "P0", "Risky/sensitive expression is missing warning metadata"]);
  }
  if (safetyRating === "explicit-adult" && (entry.requiresAdultAccess !== true || entry.minimumAge !== 18)) {
    issues.push([owner, "P0", "Explicit-adult expression must require adult access and minimumAge 18"]);
  }
  const meaningLanguages = new Set((entry.meanings || []).map(meaning => meaning.language));
  for (const language of requiredLanguages) {
    if (!meaningLanguages.has(language)) issues.push([owner, "P1", `Missing localized meaning for ${language}`]);
  }
  for (const meaning of entry.meanings || []) {
    if (meaning.language !== "en" && isEnglishLike(`${meaning.actualMeaningText || ""} ${meaning.usageExplanation || ""}`)) {
      issues.push([owner, "P1", `Possible English fallback in ${meaning.language} meaning`]);
    }
  }
  for (const word of entry.linkedWords || []) {
    if ("meaning" in word || "translation" in word || "definition" in word || "explanation" in word) {
      issues.push([owner, "P0", "linkedWords contains meaning/translation/explanation data"]);
    }
  }
  for (const text of collectTexts(entry)) {
    if (processLeakPattern.test(text)) {
      issues.push([owner, "P0", "Learner-facing text contains internal process/checkpoint wording"]);
      break;
    }
  }
}

function main() {
  const packageFiles = fs.existsSync(packagesDir)
    ? fs.readdirSync(packagesDir).filter(name => name.endsWith(".json")).sort()
    : [];
  const issues = [];
  const counts = [];

  for (const fileName of packageFiles) {
    const filePath = path.join(packagesDir, fileName);
    try {
      const json = readJson(filePath);
      const entries = Array.isArray(json.expressionEntries) ? json.expressionEntries : [];
      counts.push([fileName, json.packageId || "(missing-package-id)", entries.length]);
      if (entries.length === 0) issues.push([json.packageId || fileName, "P0", "No expressionEntries"]);
      for (const entry of entries) auditEntry(json.packageId || fileName, entry, issues);
    } catch (error) {
      issues.push([fileName, "P0", `Invalid JSON: ${error.message}`]);
    }
  }

  fs.mkdirSync(path.dirname(reportPath), { recursive: true });
  const lines = [
    "# Expression Content Quality Report",
    "",
    `Generated at: ${new Date().toISOString()}`,
    "",
    "## Packages",
    "",
    "| File | Package | Expressions |",
    "| --- | --- | ---: |",
    ...counts.map(([file, id, count]) => `| ${file} | ${id} | ${count} |`),
    "",
    "## Issues",
    "",
    `Total issues: ${issues.length}`,
    "",
    "| Owner | Priority | Issue |",
    "| --- | --- | --- |",
    ...(issues.length === 0 ? ["| - | - | No issues found |"] : issues.map(([owner, priority, issue]) => `| ${owner} | ${priority} | ${issue.replaceAll("|", "\\|")} |`)),
    "",
    "## Decision",
    "",
    issues.some(([, priority]) => priority === "P0")
      ? "Expressions bulk generation remains blocked until P0 issues are repaired."
      : "No P0 issue was found by this audit; review P1 issues before the next small batch.",
    "",
  ];
  fs.writeFileSync(reportPath, lines.join("\n"), "utf8");
  console.log(`Expression content quality report written to ${reportPath}`);
  console.log(`Packages: ${counts.length}; issues: ${issues.length}`);
  if (issues.some(([, priority]) => priority === "P0")) process.exitCode = 1;
}

main();

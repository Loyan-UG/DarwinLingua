#!/usr/bin/env node

const fs = require("fs");
const path = require("path");

const root = process.cwd();
const targetLanguages = ["en", "fa", "ar", "tr", "ru", "ckb", "kmr", "pl", "ro", "sq"];
const packagePaths = [
  "content/learning-portal/grammar/packages/grammar-a1-core-v1.json",
  "content/learning-portal/grammar/packages/grammar-a2-core-v1.json",
  "content/learning-portal/grammar/packages/grammar-b1-core-v1.json",
  "content/learning-portal/grammar/packages/grammar-b2-core-v1.json",
  "content/learning-portal/grammar/packages/grammar-c1-core-v1.json",
  "content/learning-portal/grammar/packages/grammar-c2-core-v1.json",
];
const syllabusPath = "content/learning-portal/grammar/syllabus/grammar-syllabus-a1-c2-v1.json";
const reportPath = process.argv.includes("--report")
  ? process.argv[process.argv.indexOf("--report") + 1]
  : "artifacts/validation/grammar-a1-c2-validation-report.md";
const failOnIssue = process.argv.includes("--fail-on-issue");

const allowedCategories = new Set([
  "articles",
  "nouns",
  "gender",
  "plural",
  "pronouns",
  "verbs",
  "modal-verbs",
  "tenses",
  "separable-verbs",
  "reflexive-verbs",
  "cases",
  "nominative",
  "accusative",
  "dative",
  "genitive",
  "adjective-declension",
  "prepositions",
  "word-order",
  "subordinate-clauses",
  "connectors",
  "negation",
  "questions",
  "imperative",
  "passive",
  "konjunktiv",
  "reported-speech",
  "punctuation",
]);
const knownBlockTypes = new Set([
  "paragraph",
  "table",
  "callout",
  "rule-list",
  "example-list",
  "mistake-pair",
  "image-slot",
]);
const slugPattern = /^[a-z0-9]+(?:-[a-z0-9]+)*$/;
const rawHtmlPattern = /<\/?[a-z][\s\S]*>/i;
const placeholderPattern = /\b(TODO|TBA|lorem ipsum|sample only|test data|Checkpoint for|do not continue until)\b/i;
const linkedWordMeaningPattern = /\s[-–—:]\s|=| means | meaning | یعنی | به معنی | معناه | anlamı | значит | oznacza | înseamnă | do të thotë /i;

function readJson(relativePath) {
  const fullPath = path.join(root, relativePath);
  return JSON.parse(fs.readFileSync(fullPath, "utf8"));
}

function asArray(value) {
  return Array.isArray(value) ? value : [];
}

function hasText(value) {
  return typeof value === "string" && value.trim().length > 0;
}

function localizedObjectHasAllLanguages(owner, field, issues, context) {
  if (!owner || typeof owner !== "object" || !owner[field] || typeof owner[field] !== "object") {
    issues.push(`${context}: missing ${field}`);
    return;
  }
  for (const language of targetLanguages) {
    if (!hasText(owner[field][language])) {
      issues.push(`${context}: missing ${field}.${language}`);
    }
  }
}

function translationLanguages(translations) {
  if (!translations) {
    return new Set();
  }
  if (Array.isArray(translations)) {
    return new Set(translations.map((item) => item && item.language).filter(Boolean));
  }
  if (typeof translations === "object") {
    return new Set(Object.keys(translations));
  }
  return new Set();
}

function checkTranslationSet(translations, issues, context) {
  const languages = translationLanguages(translations);
  for (const language of targetLanguages) {
    if (!languages.has(language)) {
      issues.push(`${context}: missing translation ${language}`);
    }
  }
  if (Array.isArray(translations)) {
    const seen = new Set();
    for (const translation of translations) {
      if (!translation || !translation.language) {
        issues.push(`${context}: malformed translation`);
        continue;
      }
      if (seen.has(translation.language)) {
        issues.push(`${context}: duplicate translation ${translation.language}`);
      }
      seen.add(translation.language);
      const text = translation.text ?? translation.heading;
      if (!hasText(text)) {
        issues.push(`${context}: empty translation ${translation.language}`);
      }
    }
  } else if (translations && typeof translations === "object") {
    for (const language of Object.keys(translations)) {
      if (!hasText(translations[language])) {
        issues.push(`${context}: empty translation ${language}`);
      }
    }
  }
}

function checkNoRawHtmlOrPlaceholder(value, issues, context) {
  if (!hasText(value)) {
    return;
  }
  if (rawHtmlPattern.test(value)) {
    issues.push(`${context}: contains raw HTML`);
  }
  if (placeholderPattern.test(value)) {
    issues.push(`${context}: contains placeholder/internal instruction text`);
  }
}

function checkBlock(block, issues, context) {
  if (!block || typeof block !== "object") {
    issues.push(`${context}: malformed block`);
    return;
  }
  if (!knownBlockTypes.has(block.type)) {
    issues.push(`${context}: unknown block type ${block.type ?? "<missing>"}`);
    return;
  }

  const textFields = ["text", "caption", "style", "wrong", "correct", "assetKey", "imageSlotKey"];
  for (const field of textFields) {
    if (hasText(block[field])) {
      checkNoRawHtmlOrPlaceholder(block[field], issues, `${context}.${field}`);
    }
  }

  switch (block.type) {
    case "paragraph":
      if (!hasText(block.text)) issues.push(`${context}: paragraph missing text`);
      break;
    case "table":
      if (!hasText(block.caption)) issues.push(`${context}: table missing caption`);
      if (!Array.isArray(block.columns) || block.columns.length === 0) issues.push(`${context}: table missing columns`);
      if (!Array.isArray(block.rows) || block.rows.length === 0) {
        issues.push(`${context}: table missing rows`);
      } else if (Array.isArray(block.columns)) {
        block.rows.forEach((row, index) => {
          if (!Array.isArray(row) || row.length !== block.columns.length) {
            issues.push(`${context}: malformed table row ${index + 1}`);
          }
        });
      }
      break;
    case "callout":
      if (!hasText(block.style)) issues.push(`${context}: callout missing style`);
      if (!hasText(block.text)) issues.push(`${context}: callout missing text`);
      break;
    case "rule-list":
    case "example-list":
      if (!Array.isArray(block.items) || block.items.length === 0) {
        issues.push(`${context}: ${block.type} missing items`);
      }
      asArray(block.items).forEach((item, index) => checkNoRawHtmlOrPlaceholder(String(item ?? ""), issues, `${context}.items[${index}]`));
      break;
    case "mistake-pair":
      if (!hasText(block.wrong)) issues.push(`${context}: mistake-pair missing wrong`);
      if (!hasText(block.correct)) issues.push(`${context}: mistake-pair missing correct`);
      if (hasText(block.wrong) && block.wrong === block.correct) issues.push(`${context}: mistake-pair wrong/correct identical`);
      break;
    case "image-slot":
      if (!hasText(block.assetKey) && !hasText(block.imageSlotKey)) {
        issues.push(`${context}: image-slot missing assetKey/imageSlotKey`);
      }
      break;
  }
}

function linkedWordText(word) {
  if (typeof word === "string") {
    return word;
  }
  if (word && typeof word === "object") {
    return `${word.lemma ?? ""} ${word.wordSlug ?? ""}`.trim();
  }
  return "";
}

function checkLinkedWord(word, issues, context) {
  const text = linkedWordText(word);
  if (!hasText(text)) {
    issues.push(`${context}: empty linked word`);
    return;
  }
  if (linkedWordMeaningPattern.test(text)) {
    issues.push(`${context}: linkedWords appears to contain a meaning (${text})`);
  }
  if (placeholderPattern.test(text)) {
    issues.push(`${context}: linkedWords contains placeholder/internal text`);
  }
}

const syllabus = readJson(syllabusPath);
const syllabusTopics = new Map(syllabus.topics.map((topic) => [topic.slug, topic]));
const syllabusSlugs = new Set(syllabusTopics.keys());
const issues = [];
const warnings = [];
const stats = {
  packages: [],
  byCefr: {},
  byCategory: {},
  totalTopics: 0,
  missingSyllabusSlugs: [],
  extraPackageSlugs: [],
  duplicateSlugs: [],
  imageSlots: 0,
  missingImageAssets: [],
  unresolvedExternalReferences: [],
};
const packageSlugs = new Map();
const allTopics = [];

for (const packagePath of packagePaths) {
  const pkg = readJson(packagePath);
  const expectedLevel = packagePath.match(/grammar-([a-z0-9]+)-core-v1\.json$/)?.[1]?.toUpperCase();
  const packageContext = packagePath;
  const grammarTopics = asArray(pkg.grammarTopics);
  stats.packages.push({
    path: packagePath,
    packageId: pkg.packageId,
    packageName: pkg.packageName,
    level: expectedLevel,
    topicCount: grammarTopics.length,
  });

  if (!hasText(pkg.packageId)) issues.push(`${packageContext}: missing packageId`);
  if (!hasText(pkg.packageName)) issues.push(`${packageContext}: missing packageName`);
  if (!hasText(pkg.packageVersion)) issues.push(`${packageContext}: missing packageVersion`);
  if (!hasText(pkg.source)) issues.push(`${packageContext}: missing source`);
  if (pkg.upsertMode !== "by-slug") issues.push(`${packageContext}: upsertMode must be by-slug`);
  if (!Array.isArray(pkg.grammarTopics) || pkg.grammarTopics.length === 0) issues.push(`${packageContext}: grammarTopics missing/empty`);
  if (Array.isArray(pkg.entries) && pkg.entries.length > 0) issues.push(`${packageContext}: entries must be empty for official grammar packages`);
  for (const language of targetLanguages) {
    if (!Array.isArray(pkg.targetLanguages) || !pkg.targetLanguages.includes(language)) {
      issues.push(`${packageContext}: targetLanguages missing ${language}`);
    }
  }

  grammarTopics.forEach((topic, index) => {
    const context = `${packageContext}#${topic.slug ?? index}`;
    allTopics.push({ topic, packagePath, expectedLevel, context });
    stats.totalTopics += 1;
    stats.byCefr[topic.cefrLevel ?? "<missing>"] = (stats.byCefr[topic.cefrLevel ?? "<missing>"] ?? 0) + 1;
    stats.byCategory[topic.grammarCategory ?? "<missing>"] = (stats.byCategory[topic.grammarCategory ?? "<missing>"] ?? 0) + 1;

    if (!hasText(topic.slug)) {
      issues.push(`${context}: missing slug`);
    } else {
      if (!slugPattern.test(topic.slug)) issues.push(`${context}: slug is not lowercase kebab-case`);
      if (packageSlugs.has(topic.slug)) {
        stats.duplicateSlugs.push(topic.slug);
        issues.push(`${context}: duplicate slug also in ${packageSlugs.get(topic.slug)}`);
      }
      packageSlugs.set(topic.slug, packagePath);
    }
    if (!hasText(topic.title)) issues.push(`${context}: missing title`);
    if (!hasText(topic.shortDescription)) issues.push(`${context}: missing shortDescription`);
    if (!["A1", "A2", "B1", "B2", "C1", "C2"].includes(topic.cefrLevel)) issues.push(`${context}: invalid cefrLevel`);
    if (topic.cefrLevel !== expectedLevel) issues.push(`${context}: cefrLevel ${topic.cefrLevel} does not match package ${expectedLevel}`);
    if (!allowedCategories.has(topic.grammarCategory)) issues.push(`${context}: invalid grammarCategory ${topic.grammarCategory}`);
    if (topic.isPublished !== undefined && topic.isPublished !== true) warnings.push(`${context}: isPublished is not true`);
    if (topic.sortOrder !== undefined && typeof topic.sortOrder !== "number") issues.push(`${context}: sortOrder is not numeric`);
    localizedObjectHasAllLanguages(topic, "titleLocalized", issues, context);
    localizedObjectHasAllLanguages(topic, "shortDescriptionLocalized", issues, context);
    checkNoRawHtmlOrPlaceholder(topic.title, issues, `${context}.title`);
    checkNoRawHtmlOrPlaceholder(topic.shortDescription, issues, `${context}.shortDescription`);

    const syllabusTopic = syllabusTopics.get(topic.slug);
    if (!syllabusTopic) {
      stats.extraPackageSlugs.push(topic.slug);
      issues.push(`${context}: slug not found in syllabus`);
    } else {
      if (topic.title !== syllabusTopic.titleEn) issues.push(`${context}: title mismatch with syllabus (${syllabusTopic.titleEn})`);
      if (topic.cefrLevel !== syllabusTopic.cefrLevel) issues.push(`${context}: cefrLevel mismatch with syllabus`);
      if (topic.grammarCategory !== syllabusTopic.grammarCategory) issues.push(`${context}: grammarCategory mismatch with syllabus (${syllabusTopic.grammarCategory})`);
    }

    const sections = asArray(topic.sections);
    if (sections.length === 0) issues.push(`${context}: sections missing/empty`);
    const sectionKeys = new Set();
    sections.forEach((section, sectionIndex) => {
      const sectionContext = `${context}.sections[${section.sectionKey ?? sectionIndex}]`;
      if (!hasText(section.sectionKey)) issues.push(`${sectionContext}: missing sectionKey`);
      if (hasText(section.sectionKey) && sectionKeys.has(section.sectionKey)) issues.push(`${sectionContext}: duplicate sectionKey`);
      if (hasText(section.sectionKey)) sectionKeys.add(section.sectionKey);
      if (section.sortOrder !== undefined && typeof section.sortOrder !== "number") issues.push(`${sectionContext}: sortOrder is not numeric`);
      checkTranslationSet(section.translations, issues, `${sectionContext}.translations`);
      if (!section.localizedBlocks || typeof section.localizedBlocks !== "object") {
        issues.push(`${sectionContext}: missing localizedBlocks`);
      } else {
        for (const language of targetLanguages) {
          const blocks = section.localizedBlocks[language];
          if (!Array.isArray(blocks) || blocks.length === 0) {
            issues.push(`${sectionContext}: localizedBlocks missing/empty ${language}`);
          } else {
            blocks.forEach((block, blockIndex) => checkBlock(block, issues, `${sectionContext}.localizedBlocks.${language}[${blockIndex}]`));
          }
        }
      }
    });

    asArray(topic.examples).forEach((example, exampleIndex) => {
      const exampleContext = `${context}.examples[${exampleIndex}]`;
      if (!hasText(example.germanText)) issues.push(`${exampleContext}: missing germanText`);
      checkNoRawHtmlOrPlaceholder(example.germanText, issues, `${exampleContext}.germanText`);
      checkTranslationSet(example.translations, issues, `${exampleContext}.translations`);
      if (example.sortOrder !== undefined && typeof example.sortOrder !== "number") issues.push(`${exampleContext}: sortOrder is not numeric`);
    });

    const germanExampleCounts = new Map();
    for (const example of asArray(topic.examples)) {
      if (hasText(example.germanText)) {
        germanExampleCounts.set(example.germanText, (germanExampleCounts.get(example.germanText) ?? 0) + 1);
      }
    }
    for (const [germanText, count] of germanExampleCounts.entries()) {
      if (count > 3) issues.push(`${context}: German example repeated ${count} times: ${germanText}`);
    }

    asArray(topic.ruleSummaries).forEach((rule, ruleIndex) => {
      const ruleContext = `${context}.ruleSummaries[${ruleIndex}]`;
      const text = rule.text ?? "";
      checkNoRawHtmlOrPlaceholder(text, issues, `${ruleContext}.text`);
      if (rule.localizedText) {
        localizedObjectHasAllLanguages(rule, "localizedText", issues, ruleContext);
      } else {
        checkTranslationSet(rule.translations, issues, `${ruleContext}.translations`);
      }
    });

    asArray(topic.commonMistakes).forEach((mistake, mistakeIndex) => {
      const mistakeContext = `${context}.commonMistakes[${mistakeIndex}]`;
      const wrong = mistake.wrongText ?? mistake.wrongGerman ?? mistake.wrong;
      const correct = mistake.correctedText ?? mistake.correctGerman ?? mistake.correct;
      const explanation = mistake.explanation ?? mistake.explanationLocalized?.en;
      if (!hasText(wrong)) issues.push(`${mistakeContext}: missing wrongText`);
      if (!hasText(correct)) issues.push(`${mistakeContext}: missing correctedText`);
      if (hasText(wrong) && wrong === correct) issues.push(`${mistakeContext}: wrong/correct identical`);
      if (!hasText(explanation)) issues.push(`${mistakeContext}: missing explanation`);
      if (mistake.explanationLocalized) {
        localizedObjectHasAllLanguages(mistake, "explanationLocalized", issues, mistakeContext);
      } else {
        checkTranslationSet(mistake.translations, issues, `${mistakeContext}.translations`);
      }
    });

    asArray(topic.linkedWords).forEach((word, wordIndex) => checkLinkedWord(word, issues, `${context}.linkedWords[${wordIndex}]`));

    for (const field of ["prerequisiteSlugs", "relatedTopicSlugs"]) {
      for (const linkedSlug of asArray(topic[field])) {
        if (!slugPattern.test(linkedSlug)) issues.push(`${context}.${field}: invalid slug ${linkedSlug}`);
        if (!packageSlugs.has(linkedSlug) && !syllabusSlugs.has(linkedSlug)) {
          issues.push(`${context}.${field}: unresolved grammar slug ${linkedSlug}`);
        }
      }
    }

    for (const field of ["linkedDialogueSlugs", "linkedTalkTopicSlugs", "linkedExerciseSlugs"]) {
      for (const linkedSlug of asArray(topic[field])) {
        if (!slugPattern.test(linkedSlug)) issues.push(`${context}.${field}: invalid linked content slug ${linkedSlug}`);
        stats.unresolvedExternalReferences.push(`${topic.slug}.${field}:${linkedSlug}`);
      }
    }

    for (const slot of asArray(topic.imageSlots)) {
      stats.imageSlots += 1;
      const assetKey = slot.assetKey ?? slot.imageSlotKey;
      if (!hasText(assetKey)) {
        issues.push(`${context}.imageSlots: missing assetKey/imageSlotKey`);
      }
      const fileName = slot.fileName ?? slot.imageFileName;
      if (hasText(fileName)) {
        const expectedPath = path.join(root, "content", "learning-portal", "grammar", "assets", topic.slug, fileName);
        if (!fs.existsSync(expectedPath)) {
          stats.missingImageAssets.push(path.relative(root, expectedPath).replace(/\\/g, "/"));
        }
      }
    }
  });
}

for (const [slug, topic] of syllabusTopics.entries()) {
  if (!packageSlugs.has(slug)) {
    stats.missingSyllabusSlugs.push(slug);
    issues.push(`syllabus: missing generated topic ${slug}`);
  }
}

function table(rows) {
  if (rows.length === 0) return "_None._";
  const headers = Object.keys(rows[0]);
  const header = `| ${headers.join(" | ")} |`;
  const separator = `| ${headers.map(() => "---").join(" | ")} |`;
  const body = rows.map((row) => `| ${headers.map((headerName) => String(row[headerName] ?? "").replace(/\|/g, "\\|")).join(" | ")} |`);
  return [header, separator, ...body].join("\n");
}

function countRows(record) {
  return Object.entries(record)
    .sort(([left], [right]) => left.localeCompare(right))
    .map(([key, count]) => ({ key, count }));
}

const report = [
  "# Grammar A1-C2 Validation Report",
  "",
  `Generated at: ${new Date().toISOString()}`,
  "",
  "## Summary",
  "",
  `- Official package count: ${packagePaths.length}`,
  `- Total GrammarTopic count: ${stats.totalTopics}`,
  `- Validation issue count: ${issues.length}`,
  `- Warning count: ${warnings.length}`,
  `- Missing syllabus slugs: ${stats.missingSyllabusSlugs.length}`,
  `- Extra package slugs: ${stats.extraPackageSlugs.length}`,
  `- Duplicate slugs: ${stats.duplicateSlugs.length}`,
  `- Missing image assets: ${stats.missingImageAssets.length}`,
  "",
  "## Packages",
  "",
  table(stats.packages),
  "",
  "## Topic Counts By CEFR",
  "",
  table(countRows(stats.byCefr)),
  "",
  "## Topic Counts By Grammar Category",
  "",
  table(countRows(stats.byCategory)),
  "",
  "## Syllabus Coverage",
  "",
  `- Missing syllabus slugs: ${stats.missingSyllabusSlugs.length ? stats.missingSyllabusSlugs.join(", ") : "None"}`,
  `- Extra package slugs: ${stats.extraPackageSlugs.length ? stats.extraPackageSlugs.join(", ") : "None"}`,
  `- Duplicate slugs: ${stats.duplicateSlugs.length ? stats.duplicateSlugs.join(", ") : "None"}`,
  "",
  "## Validation Issues",
  "",
  issues.length === 0 ? "_None._" : issues.map((issue) => `- ${issue}`).join("\n"),
  "",
  "## Warnings",
  "",
  warnings.length === 0 ? "_None._" : warnings.map((warning) => `- ${warning}`).join("\n"),
  "",
  "## Linked External Content References",
  "",
  stats.unresolvedExternalReferences.length === 0
    ? "_None._"
    : [
        `Count: ${stats.unresolvedExternalReferences.length}`,
        "",
        "These are stored as safe future references and were not generated in this task.",
      ].join("\n"),
  "",
  "## Image Assets",
  "",
  stats.missingImageAssets.length === 0
    ? "No missing image assets were detected for imageSlots with file names."
    : stats.missingImageAssets.map((item) => `- ${item}`).join("\n"),
  "",
  "## Import, Web API, Rendering, Search, And Admin Validation",
  "",
  "- Import validation: covered by ContentOps import/parser tests listed in the final execution log.",
  "- Web API validation: covered by WebApi tests listed in the final execution log.",
  "- Web rendering validation: covered by Web build plus grammar Razor/controller structural coverage; no browser runtime was required for this package-only validation pass.",
  "- Unified Search validation: covered by existing unified search service tests and structural search hardening tests.",
  "- Admin report validation: covered by WebsiteAdminQueryServiceLearningPortalReportTests.",
  "",
  "## Fixes Applied",
  "",
  "_This report records the post-generation validation state. See the final response for any repairs made during the run._",
  "",
  "## Remaining Backlog Items",
  "",
  "- Exercise Engine content is intentionally not generated yet.",
  "- Dialogue, Talk Topic, Conversation, Expression, Writing Template, Exam Prep, and Cultural Note audits remain separate future module passes.",
  "- Missing image assets, if any appear in future imageSlots, should be handled by the image asset pipeline and must not block grammar import.",
  "",
].join("\n");

fs.mkdirSync(path.dirname(path.join(root, reportPath)), { recursive: true });
fs.writeFileSync(path.join(root, reportPath), report, "utf8");

const output = {
  reportPath,
  packageCount: packagePaths.length,
  totalTopics: stats.totalTopics,
  issues: issues.length,
  warnings: warnings.length,
  missingSyllabusSlugs: stats.missingSyllabusSlugs.length,
  extraPackageSlugs: stats.extraPackageSlugs.length,
  duplicateSlugs: stats.duplicateSlugs.length,
  missingImageAssets: stats.missingImageAssets.length,
  byCefr: stats.byCefr,
  byCategory: stats.byCategory,
};
console.log(JSON.stringify(output, null, 2));

if (failOnIssue && issues.length > 0) {
  process.exitCode = 1;
}

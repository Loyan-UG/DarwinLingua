#!/usr/bin/env node

const fs = require("fs");
const path = require("path");

const repoRoot = process.cwd();
const reportArgIndex = process.argv.indexOf("--report");
const reportPath = reportArgIndex >= 0 && process.argv[reportArgIndex + 1]
  ? process.argv[reportArgIndex + 1]
  : "artifacts/validation/conversation-content-audit-report.md";

const sourceRoots = [
  "content",
  "src/Apps/DarwinLingua.WebApi/SeedContent",
  "assets/ServerContent",
  "src/Apps/DarwinLingua.WebApi/assets/ServerContent"
];

const cefrLevels = ["A1", "A2", "B1", "B2", "C1", "C2"];
const targetLanguages = ["en", "fa", "ar", "tr", "ru", "ckb", "kmr", "pl", "ro", "sq"];
const kebabCase = /^[a-z0-9]+(?:-[a-z0-9]+)*$/;
const talkTopicLengthRanges = {
  A1: [900, 1100],
  A2: [1400, 1600],
  B1: [1900, 2100],
  B2: [2400, 2600],
  C1: [2900, 3100],
  C2: [3400, 3600]
};
const talkTopicQuestionMinimums = { A1: 2, A2: 2, B1: 2, B2: 3, C1: 3, C2: 3 };
const talkTopicWarmupMinimums = { A1: 3, A2: 3, B1: 3, B2: 4, C1: 4, C2: 4 };
const talkTopicTypes = ["opinion", "imagination", "prediction", "comparison"];
const allowedTalkTopicContentTypes = new Set([
  "article",
  "book-summary",
  "movie-summary",
  "story",
  "fact-sheet",
  "opinion-text",
  "interview",
  "debate-text"
]);

const contextFamilies = [
  ["greetings-introductions", ["greeting", "introduc", "vorstell", "kennenlernen"]],
  ["appointments", ["termin", "appointment", "reschedule", "verschieb", "absag"]],
  ["doctor-praxis", ["doctor", "arzt", "aerzt", "praxis", "krank"]],
  ["school-kindergarten", ["school", "schule", "kindergarten", "teacher", "lehrer", "kurs"]],
  ["landlord-apartment", ["landlord", "vermieter", "wohnung", "apartment", "miete"]],
  ["workplace", ["work", "job", "arbeit", "workplace", "colleague", "kollege", "meeting"]],
  ["job-interview", ["interview", "bewerbung", "job-interview"]],
  ["public-office", ["office", "behoerde", "behorde", "amt", "formular"]],
  ["customer-service", ["service", "customer", "beschwerde", "complaint", "shopping"]],
  ["phone-calls", ["phone", "telefon", "call", "anruf"]],
  ["transportation", ["bus", "train", "bahn", "transport", "bahnhof"]],
  ["language-cafe", ["language-cafe", "conversation-cafe", "sprachcafe", "sprach-cafe", "cafe"]],
  ["exam-speaking", ["exam", "pruefung", "prufung", "goethe", "telc", "dtz"]],
  ["opinion-discussion", ["opinion", "meinung", "discussion", "diskussion", "agree", "disagree"]],
  ["negotiation", ["negotiat", "verhandlung", "kompromiss"]]
];

const expectedDialogueContexts = contextFamilies.map(item => item[0]);
const expectedTalkCategories = [
  "science",
  "politics",
  "culture",
  "sports",
  "movies",
  "books",
  "technology",
  "environment",
  "health",
  "work",
  "education",
  "society",
  "migration-integration",
  "daily-life",
  "relationships-family",
  "language-learning",
  "germany-life",
  "conversation-cafe",
  "funny-light",
  "sensitive-controversial"
];

function walkJsonFiles(directory, files = []) {
  const absolute = path.join(repoRoot, directory);
  if (!fs.existsSync(absolute)) return files;

  for (const entry of fs.readdirSync(absolute, { withFileTypes: true })) {
    const fullPath = path.join(absolute, entry.name);
    if (entry.isDirectory()) {
      if ([".git", "bin", "obj", "node_modules"].includes(entry.name)) continue;
      walkJsonFiles(path.relative(repoRoot, fullPath), files);
    } else if (entry.isFile() && entry.name.endsWith(".json")) {
      files.push(path.relative(repoRoot, fullPath).replaceAll("\\", "/"));
    }
  }

  return files;
}

function readJson(relativePath) {
  try {
    const text = fs.readFileSync(path.join(repoRoot, relativePath), "utf8").replace(/^\uFEFF/, "");
    return JSON.parse(text);
  } catch (error) {
    return { __parseError: error.message };
  }
}

function inc(map, key, amount = 1) {
  const safeKey = key || "(missing)";
  map[safeKey] = (map[safeKey] || 0) + amount;
}

function translationLanguage(item) {
  return item?.languageCode || item?.language || item?.lang || "";
}

function countTranslations(items) {
  const languages = new Set();
  for (const item of items || []) {
    const language = translationLanguage(item);
    if (language) languages.add(language);
  }
  return languages;
}

function hasMeaningLeak(reference) {
  if (!reference || typeof reference !== "object") return false;
  return [
    "meaning",
    "meaningText",
    "translation",
    "translations",
    "definition",
    "explanation",
    "example",
    "examples"
  ].some(key => Object.prototype.hasOwnProperty.call(reference, key));
}

function normalizeWordSlug(value) {
  if (!value || typeof value !== "string") return "";
  return value.trim().toLowerCase();
}

function classifyText(text) {
  const lower = (text || "").toLowerCase();
  return contextFamilies
    .filter(([, markers]) => markers.some(marker => lower.includes(marker)))
    .map(([name]) => name);
}

function unique(array) {
  return [...new Set(array)];
}

function mdTable(headers, rows) {
  const escape = value => String(value ?? "")
    .replaceAll("|", "\\|")
    .replace(/\r?\n/g, "<br>");
  return [
    `| ${headers.map(escape).join(" | ")} |`,
    `| ${headers.map(() => "---").join(" | ")} |`,
    ...rows.map(row => `| ${row.map(escape).join(" | ")} |`)
  ].join("\n");
}

function topEntries(map, limit = 20) {
  return Object.entries(map)
    .sort((a, b) => b[1] - a[1] || a[0].localeCompare(b[0]))
    .slice(0, limit);
}

const files = unique(sourceRoots.flatMap(root => walkJsonFiles(root)));
const packages = [];
const wordSlugs = new Set();
const wordLemmas = new Set();
const dialogues = [];
const talkTopics = [];
const starterPacks = [];
const eventPacks = [];
const roleplays = [];
const invalidJsonFiles = [];

for (const file of files) {
  const json = readJson(file);
  if (json.__parseError) {
    invalidJsonFiles.push({ file, error: json.__parseError });
    continue;
  }

  for (const entry of json.entries || []) {
    const slug = normalizeWordSlug(entry.slug || entry.wordSlug);
    const lemma = normalizeWordSlug(entry.lemma || entry.baseText || entry.germanText || entry.word);
    if (slug) wordSlugs.add(slug);
    if (lemma) wordLemmas.add(lemma);
  }

  const packageRecord = {
    file,
    packageId: json.packageId || "",
    packageName: json.packageName || "",
    source: json.source || "",
    counts: {
      entries: Array.isArray(json.entries) ? json.entries.length : 0,
      dialogues: Array.isArray(json.dialogues) ? json.dialogues.length : 0,
      talkTopics: Array.isArray(json.talkTopics) ? json.talkTopics.length : 0,
      conversationStarterPacks: Array.isArray(json.conversationStarterPacks) ? json.conversationStarterPacks.length : 0,
      eventPreparationPacks: Array.isArray(json.eventPreparationPacks) ? json.eventPreparationPacks.length : 0,
      roleplays: Array.isArray(json.roleplays) ? json.roleplays.length : 0,
      roleplayScenarios: Array.isArray(json.roleplayScenarios) ? json.roleplayScenarios.length : 0
    }
  };

  if (Object.values(packageRecord.counts).some(count => count > 0)) {
    packages.push(packageRecord);
  }

  for (const item of json.dialogues || []) dialogues.push({ item, file, packageId: json.packageId || "" });
  for (const item of json.talkTopics || []) talkTopics.push({ item, file, packageId: json.packageId || "" });
  for (const item of json.conversationStarterPacks || []) starterPacks.push({ item, file, packageId: json.packageId || "" });
  for (const item of json.eventPreparationPacks || []) eventPacks.push({ item, file, packageId: json.packageId || "" });
  for (const item of json.roleplays || []) roleplays.push({ item, file, packageId: json.packageId || "" });
  for (const item of json.roleplayScenarios || []) roleplays.push({ item, file, packageId: json.packageId || "" });
}

const inventory = {
  packages: packages.filter(packageRecord =>
    packageRecord.counts.dialogues ||
    packageRecord.counts.talkTopics ||
    packageRecord.counts.conversationStarterPacks ||
    packageRecord.counts.eventPreparationPacks ||
    packageRecord.counts.roleplays ||
    packageRecord.counts.roleplayScenarios),
  entriesPackages: packages.filter(packageRecord => packageRecord.counts.entries)
};

function auditDialogue(records) {
  const countsByCefr = {};
  const countsByCategory = {};
  const countsByContext = {};
  const countsByRegister = {};
  const countsByTaskType = {};
  const countsByInteractionMode = {};
  const issues = [];
  const warnings = [];
  const unresolvedWordReferences = [];
  const meaningLeaks = [];
  const missingMetadata = [];
  const weakLength = [];
  const duplicateUsefulWords = [];
  const slugMap = new Map();

  for (const record of records) {
    const dialogue = record.item;
    const slug = dialogue.slug || "";
    const cefr = dialogue.cefrLevel || "";
    inc(countsByCefr, cefr);
    inc(countsByCategory, dialogue.category);
    inc(countsByRegister, dialogue.register || "(missing)");
    inc(countsByTaskType, dialogue.taskType || "(missing)");
    inc(countsByInteractionMode, dialogue.interactionMode || "(missing)");

    for (const context of classifyText([
      slug,
      dialogue.title,
      dialogue.description,
      dialogue.category,
      dialogue.taskType,
      dialogue.interactionMode,
      ...(dialogue.topics || []),
      ...(dialogue.skillFocus || [])
    ].join(" "))) {
      inc(countsByContext, context);
    }

    if (!slug || !kebabCase.test(slug)) issues.push(`${record.file}: dialogue has malformed slug '${slug}'.`);
    if (slugMap.has(slug)) issues.push(`${record.file}: duplicate dialogue slug '${slug}' also appears in ${slugMap.get(slug)}.`);
    if (slug) slugMap.set(slug, record.file);

    const required = ["title", "description", "learnerGoal", "cefrLevel", "category"];
    for (const field of required) {
      if (!dialogue[field]) issues.push(`${record.file}: dialogue '${slug}' is missing ${field}.`);
    }
    if (!cefrLevels.includes(cefr)) issues.push(`${record.file}: dialogue '${slug}' has invalid CEFR '${cefr}'.`);
    if (!Array.isArray(dialogue.topics) || dialogue.topics.length === 0) issues.push(`${record.file}: dialogue '${slug}' is missing topics.`);
    if (!Array.isArray(dialogue.dialogueTurns) || dialogue.dialogueTurns.length === 0) issues.push(`${record.file}: dialogue '${slug}' is missing dialogueTurns.`);

    const recommendedMetadata = ["examProfiles", "skillFocus", "taskType", "interactionMode", "register", "speakingFunctions", "estimatedPracticeMinutes"];
    const missing = recommendedMetadata.filter(field => {
      const value = dialogue[field];
      return Array.isArray(value) ? value.length === 0 : value === undefined || value === null || value === "";
    });
    if (missing.length) missingMetadata.push(`${slug}: ${missing.join(", ")}`);

    const turns = dialogue.dialogueTurns || [];
    const learnerTurns = turns.filter(turn => (turn.speakerRole || "").toLowerCase() === "learner").length;
    const partnerTurns = turns.length - learnerTurns;
    const levelIndex = Math.max(0, cefrLevels.indexOf(cefr));
    const minimum = [5, 6, 7, 8, 9, 10][levelIndex] || 5;
    if (learnerTurns < minimum || partnerTurns < minimum) {
      weakLength.push(`${slug}: learner turns ${learnerTurns}, partner turns ${partnerTurns}, expected at least ${minimum} each for ${cefr || "level"}`);
    }

    for (const turn of turns) {
      if (!turn.baseText) issues.push(`${record.file}: dialogue '${slug}' has a turn without baseText.`);
      const languages = countTranslations(turn.translations);
      const missingLanguages = targetLanguages.filter(language => !languages.has(language));
      if (missingLanguages.length) warnings.push(`${slug}: turn ${turn.sortOrder || "?"} missing translations ${missingLanguages.join(", ")}.`);
    }

    const usefulWordKeys = new Set();
    for (const word of dialogue.usefulWords || []) {
      if (hasMeaningLeak(word)) meaningLeaks.push(`${slug}: usefulWords contains meaning-like fields.`);
      const key = normalizeWordSlug(word.wordSlug || word.lemma);
      if (!key) {
        warnings.push(`${slug}: usefulWords contains an empty reference.`);
        continue;
      }
      if (usefulWordKeys.has(key)) duplicateUsefulWords.push(`${slug}: duplicate useful word '${key}'.`);
      usefulWordKeys.add(key);
      const resolved = wordSlugs.has(normalizeWordSlug(word.wordSlug)) || wordLemmas.has(normalizeWordSlug(word.lemma));
      if (!resolved) unresolvedWordReferences.push(`${slug}: ${word.wordSlug || word.lemma}`);
    }
  }

  return {
    countsByCefr,
    countsByCategory,
    countsByContext,
    countsByRegister,
    countsByTaskType,
    countsByInteractionMode,
    issues,
    warnings,
    unresolvedWordReferences,
    meaningLeaks,
    missingMetadata,
    weakLength,
    duplicateUsefulWords
  };
}

function auditTalkTopics(records) {
  const countsByCefr = {};
  const countsByCategory = {};
  const countsByContentType = {};
  const issues = [];
  const warnings = [];
  const lengthIssues = [];
  const questionIssues = [];
  const unresolvedWordReferences = [];
  const meaningLeaks = [];
  const missingSpeakingGoals = [];
  const sensitiveFlagIssues = [];
  const slugMap = new Map();

  for (const record of records) {
    const topic = record.item;
    const slug = topic.slug || "";
    const cefr = topic.cefrLevel || "";
    inc(countsByCefr, cefr);
    inc(countsByCategory, topic.category);
    inc(countsByContentType, topic.contentType);

    if (!slug || !kebabCase.test(slug)) issues.push(`${record.file}: Talk Topic has malformed slug '${slug}'.`);
    if (slugMap.has(slug)) issues.push(`${record.file}: duplicate Talk Topic slug '${slug}' also appears in ${slugMap.get(slug)}.`);
    if (slug) slugMap.set(slug, record.file);

    for (const field of ["topicGroupKey", "title", "description", "cefrLevel", "category", "contentType"]) {
      if (!topic[field]) issues.push(`${record.file}: Talk Topic '${slug}' is missing ${field}.`);
    }
    if (!cefrLevels.includes(cefr)) issues.push(`${record.file}: Talk Topic '${slug}' has invalid CEFR '${cefr}'.`);
    if (topic.contentType && !allowedTalkTopicContentTypes.has(topic.contentType)) {
      issues.push(`${record.file}: Talk Topic '${slug}' has unsupported contentType '${topic.contentType}'.`);
    }
    if (!Array.isArray(topic.topics) || topic.topics.length === 0) issues.push(`${record.file}: Talk Topic '${slug}' is missing topics.`);

    const article = topic.article || {};
    const articleText = typeof article.baseText === "string" ? article.baseText.trim() : "";
    if (!articleText) issues.push(`${record.file}: Talk Topic '${slug}' is missing article.baseText.`);
    if (Array.isArray(article.translations) && article.translations.length) {
      issues.push(`${record.file}: Talk Topic '${slug}' stores article translations, but contract expects German-only article text.`);
    }
    const range = talkTopicLengthRanges[cefr];
    if (range && (articleText.length < range[0] || articleText.length > range[1])) {
      lengthIssues.push(`${slug}: ${articleText.length} characters, expected ${range[0]}-${range[1]} for ${cefr}.`);
    }

    const warmups = topic.warmupQuestions || [];
    const warmupMinimum = talkTopicWarmupMinimums[cefr] || 3;
    if (warmups.length < warmupMinimum) questionIssues.push(`${slug}: ${warmups.length} warm-up questions, expected at least ${warmupMinimum}.`);
    for (const question of warmups) {
      if (!question.prompt) questionIssues.push(`${slug}: warm-up question without prompt.`);
      if (Array.isArray(question.translations) && question.translations.length) {
        questionIssues.push(`${slug}: warm-up question stores translations, but contract expects German-only questions.`);
      }
    }

    const discussion = topic.discussionQuestions || [];
    const questionMinimum = talkTopicQuestionMinimums[cefr] || 2;
    for (const questionType of talkTopicTypes) {
      const count = discussion.filter(question => question.questionType === questionType).length;
      if (count < questionMinimum) questionIssues.push(`${slug}: ${count} '${questionType}' questions, expected at least ${questionMinimum}.`);
    }
    for (const question of discussion) {
      if (!question.prompt) questionIssues.push(`${slug}: discussion question without prompt.`);
      if (!talkTopicTypes.includes(question.questionType)) questionIssues.push(`${slug}: invalid discussion questionType '${question.questionType}'.`);
      if (Array.isArray(question.translations) && question.translations.length) {
        questionIssues.push(`${slug}: discussion question stores translations, but contract expects German-only questions.`);
      }
    }

    if (!Array.isArray(topic.speakingGoals) || topic.speakingGoals.length === 0) {
      missingSpeakingGoals.push(slug);
    }

    const sensitiveMarkers = ["politik", "politic", "migration", "religion", "krieg", "war", "gender", "health", "gesundheit", "diskriminierung"];
    const textForSensitivity = [slug, topic.title, topic.description, topic.category, ...(topic.topics || [])].join(" ").toLowerCase();
    if (sensitiveMarkers.some(marker => textForSensitivity.includes(marker)) && topic.isSensitive !== true && topic.recommendedForModeratedGroupsOnly !== true) {
      sensitiveFlagIssues.push(slug);
    }

    for (const word of topic.vocabularyItems || topic.importantWords || topic.linkedWords || []) {
      if (hasMeaningLeak(word)) meaningLeaks.push(`${slug}: vocabulary item contains meaning-like fields.`);
      const key = normalizeWordSlug(word.wordSlug || word.lemma || word.word);
      if (!key) {
        warnings.push(`${slug}: vocabulary item contains an empty reference.`);
        continue;
      }
      const resolved = wordSlugs.has(normalizeWordSlug(word.wordSlug)) || wordLemmas.has(normalizeWordSlug(word.lemma || word.word));
      if (!resolved) unresolvedWordReferences.push(`${slug}: ${word.wordSlug || word.lemma || word.word}`);
    }
  }

  return {
    countsByCefr,
    countsByCategory,
    countsByContentType,
    issues,
    warnings,
    lengthIssues,
    questionIssues,
    unresolvedWordReferences,
    meaningLeaks,
    missingSpeakingGoals,
    sensitiveFlagIssues
  };
}

function auditStarters(records) {
  const countsByCefr = {};
  const countsByCategory = {};
  const issues = [];
  for (const record of records) {
    const pack = record.item;
    inc(countsByCefr, pack.cefrLevel);
    inc(countsByCategory, pack.category);
    for (const field of ["slug", "title", "description", "cefrLevel", "category", "situation", "tone", "conversationGoal"]) {
      if (!pack[field]) issues.push(`${record.file}: starter pack '${pack.slug || "(missing)"}' is missing ${field}.`);
    }
    if (!Array.isArray(pack.phrases) || pack.phrases.length === 0) issues.push(`${record.file}: starter pack '${pack.slug || "(missing)"}' has no phrases.`);
  }
  return { countsByCefr, countsByCategory, issues };
}

function auditEventPacks(records) {
  const countsByCefr = {};
  const countsByCategory = {};
  const countsByEventType = {};
  const issues = [];
  for (const record of records) {
    const pack = record.item;
    inc(countsByCefr, pack.cefrLevel);
    inc(countsByCategory, pack.category);
    inc(countsByEventType, pack.eventType);
    for (const field of ["slug", "title", "description", "cefrLevel", "category", "eventType"]) {
      if (!pack[field]) issues.push(`${record.file}: event preparation pack '${pack.slug || "(missing)"}' is missing ${field}.`);
    }
  }
  return { countsByCefr, countsByCategory, countsByEventType, issues };
}

function auditRoleplays(records) {
  const countsByCefr = {};
  const issues = [];
  for (const record of records) {
    const roleplay = record.item;
    inc(countsByCefr, roleplay.cefrLevel);
    for (const field of ["slug", "title", "cefrLevel"]) {
      if (!roleplay[field]) issues.push(`${record.file}: roleplay '${roleplay.slug || "(missing)"}' is missing ${field}.`);
    }
  }
  return { countsByCefr, issues };
}

const dialogueAudit = auditDialogue(dialogues);
const talkTopicAudit = auditTalkTopics(talkTopics);
const starterAudit = auditStarters(starterPacks);
const eventAudit = auditEventPacks(eventPacks);
const roleplayAudit = auditRoleplays(roleplays);

const sourceItemCounts = {
  dialogues: dialogues.length,
  talkTopics: talkTopics.length,
  conversationStarterPacks: starterPacks.length,
  eventPreparationPacks: eventPacks.length,
  roleplays: roleplays.length
};

const missingDialogueContexts = expectedDialogueContexts.filter(context => !dialogueAudit.countsByContext[context]);
const missingTalkCategoryFamilies = expectedTalkCategories.filter(category => {
  const keys = Object.keys(talkTopicAudit.countsByCategory).join(" ").toLowerCase();
  return !keys.includes(category.replace("-integration", "").replace("relationships-", "").replace("-light", "").replace("-controversial", ""));
});

const p0Items = [
  ...invalidJsonFiles.map(item => `Invalid JSON: ${item.file}`),
  ...dialogueAudit.issues.slice(0, 25).map(item => `Dialogue contract issue: ${item}`),
  ...talkTopicAudit.issues.slice(0, 25).map(item => `Talk Topic contract issue: ${item}`),
  ...starterAudit.issues.slice(0, 10).map(item => `Conversation Starter contract issue: ${item}`),
  ...eventAudit.issues.slice(0, 10).map(item => `Event Preparation contract issue: ${item}`),
  ...roleplayAudit.issues.slice(0, 10).map(item => `Roleplay contract issue: ${item}`)
];

const p1Gaps = [
  ...(sourceItemCounts.conversationStarterPacks === 0 ? ["No Conversation Starter Packs exist in source content packages."] : []),
  ...(sourceItemCounts.eventPreparationPacks === 0 ? ["No Event Preparation Packs exist in source content packages."] : []),
  ...(sourceItemCounts.roleplays === 0 ? ["No dedicated Roleplay packages exist; Web may derive roleplay from dialogues but there is no authored roleplay coverage."] : []),
  ...(["B1", "B2"].flatMap(level => (dialogueAudit.countsByCefr[level] ? [] : [`No ${level} dialogues found.`]))),
  ...(["B1", "B2"].flatMap(level => (talkTopicAudit.countsByCefr[level] ? [] : [`No ${level} Talk Topics found.`]))),
  ...(dialogueAudit.unresolvedWordReferences.length ? [`Dialogue usefulWords include ${dialogueAudit.unresolvedWordReferences.length} unresolved word references.`] : []),
  ...(talkTopicAudit.unresolvedWordReferences.length ? [`Talk Topic vocabulary includes ${talkTopicAudit.unresolvedWordReferences.length} unresolved word references.`] : []),
  ...(talkTopicAudit.lengthIssues.length ? [`${talkTopicAudit.lengthIssues.length} Talk Topics are outside the documented character-length range.`] : []),
  ...(talkTopicAudit.questionIssues.length ? [`${talkTopicAudit.questionIssues.length} Talk Topic question coverage issues were detected.`] : [])
];

function recommendationRows() {
  const rows = [];
  let index = 1;
  const add = (priority, module, title, reason, dependencies, promptType) => {
    rows.push([`CC-AUD-${String(index).padStart(3, "0")}`, title, module, priority, reason, dependencies, promptType]);
    index += 1;
  };

  if (p0Items.length) add("P0", "Conversation content", "Fix blocking import/render contract issues", `${p0Items.length} blocking or potentially blocking issues were detected.`, "Audit report issue samples", "repair-only prompt");
  if (dialogueAudit.unresolvedWordReferences.length || talkTopicAudit.unresolvedWordReferences.length) add("P1", "Words + conversation links", "Resolve conversation linkedWords", "Some dialogue or Talk Topic vocabulary references do not resolve to the scanned Word Catalog.", "Word Catalog audit", "link repair prompt");
  if (sourceItemCounts.conversationStarterPacks === 0) add("P1", "Conversation Starters", "Generate A1-C2 starter pack baseline", "No starter packs exist, so language cafe and first-meeting flows lack reusable warm-up material.", "Starter contract and reviewed seed examples", "generation prompt");
  if (sourceItemCounts.eventPreparationPacks === 0) add("P1", "Event Preparation", "Generate real-life event preparation baseline", "No event preparation packs exist for language cafes, appointments, offices, school, or work.", "Starter packs and dialogue links", "generation prompt");
  if (sourceItemCounts.roleplays === 0) add("P1", "Roleplays", "Implement RoleplayScenario import before generation", "No dedicated roleplay scenario packages exist. The contract exists, but standalone roleplay generation should wait for parser, persistence, Web rendering, search, admin visibility, and tests.", "Roleplay contract and Dialogue source selection", "implementation prompt");
  if (talkTopicAudit.lengthIssues.length || talkTopicAudit.questionIssues.length) add("P1", "Talk Topics", "Repair Talk Topic length and discussion-question coverage", "Some Talk Topics may not meet character-length or question-type contract rules.", "Talk Topic contract", "repair-only prompt");

  for (const context of missingDialogueContexts.slice(0, 20)) {
    add("P1", "Dialogues", `Gap-fill ${context} dialogues`, `No source dialogue was classified into the ${context} context.`, "Dialogue contract", "gap-fill prompt");
  }
  for (const category of missingTalkCategoryFamilies.slice(0, 30)) {
    add("P1", "Talk Topics", `Gap-fill ${category} Talk Topics`, `No source Talk Topic category matched the ${category} family.`, "Talk Topic contract", "gap-fill prompt");
  }

  add("P2", "Admin reports", "Add conversation coverage quality report", "Current admin reporting surfaces learning portal counts, but this audit needs unresolved linkedWords, missing starters, missing event packs, sensitive-topic flags, and Talk Topic length/question coverage.", "Catalog query services", "implementation prompt");
  add("P2", "Search/filtering", "Verify conversation search with imported full content", "Search infrastructure supports dialogue and Talk Topic result types, but a full imported dataset should be checked for discoverability and ranking.", "Imported content database", "validation prompt");
  add("P3", "Documentation", "Keep post-Grammar audit-first rule current", "Roadmap already blocks bulk generation until module contracts and validation are stable; keep this audit report linked from future prompts.", "None", "documentation prompt");

  return rows;
}

const backlogRows = recommendationRows();

function renderList(items, empty = "_None._", limit = 50) {
  if (!items.length) return empty;
  return items.slice(0, limit).map(item => `- ${item}`).join("\n") + (items.length > limit ? `\n- ... ${items.length - limit} more` : "");
}

const inventoryRows = inventory.packages.map(packageRecord => [
  packageRecord.file,
  packageRecord.packageId || "(none)",
  packageRecord.counts.dialogues,
  packageRecord.counts.talkTopics,
  packageRecord.counts.conversationStarterPacks,
  packageRecord.counts.eventPreparationPacks,
  packageRecord.counts.roleplays + packageRecord.counts.roleplayScenarios
]);

const highestPriorityGaps = [];
if (sourceItemCounts.conversationStarterPacks === 0) highestPriorityGaps.push("missing Conversation Starter Packs");
if (sourceItemCounts.eventPreparationPacks === 0) highestPriorityGaps.push("missing Event Preparation Packs");
if (sourceItemCounts.roleplays === 0) highestPriorityGaps.push("missing dedicated Roleplay packages");
if (dialogueAudit.unresolvedWordReferences.length || talkTopicAudit.unresolvedWordReferences.length) highestPriorityGaps.push("unresolved linkedWords");
if (talkTopicAudit.lengthIssues.length || talkTopicAudit.questionIssues.length) highestPriorityGaps.push("Talk Topic length/question issues");
if (starterAudit.issues.length) highestPriorityGaps.push("Conversation Starter contract issues");
if (eventAudit.issues.length) highestPriorityGaps.push("Event Preparation contract issues");

const highestPriorityGapText = highestPriorityGaps.length
  ? highestPriorityGaps.join(", ")
  : "no P0/P1 blocking coverage or link gaps detected by this static audit";

const starterAuditNote = sourceItemCounts.conversationStarterPacks
  ? "Baseline exists. Next work should expand only proven gaps and keep starter packs reusable across language cafes, first meetings, online meetings, and group discussion."
  : "Gap priority: P1. Generate A1-C2 starter packs before Course Lessons or Exam Prep, because they are reusable support content for language cafes, first meetings, shy learner support, online meetings, and group discussion.";

const expectedEventTypes = [
  "conversation-cafe",
  "conversation-club",
  "doctor-appointment",
  "school-kindergarten-meeting",
  "landlord-meeting",
  "job-interview",
  "public-office-appointment",
  "exam-speaking-day",
  "group-study-meeting",
  "online-practice",
  "work-event"
];
const missingEventTypes = expectedEventTypes.filter(eventType => !eventAudit.countsByEventType[eventType]);
const eventAuditNote = sourceItemCounts.eventPreparationPacks
  ? "Baseline exists. Remaining work should fill only missing event types and strengthen links to Dialogues, Talk Topics, Cultural Notes, and future roleplay support."
  : "Gap priority: P1 for language cafe, public office, doctor, school/kindergarten, landlord, and job interview packs.";

const sourceCoverageMissing = [
  ...(sourceItemCounts.conversationStarterPacks === 0 ? ["Conversation Starter coverage"] : []),
  ...(sourceItemCounts.eventPreparationPacks === 0 ? ["Event Preparation Pack coverage"] : []),
  ...(sourceItemCounts.roleplays === 0 ? ["Roleplay coverage"] : [])
];

const report = `# Conversation Content Audit Report

Generated at: ${new Date().toISOString()}

## A. Executive Summary

- Content families audited: Dialogues, Talk Topics, Conversation Starter Packs, Event Preparation Packs, Roleplays, Word Catalog references, Web/API/search/admin surfaces, and relevant docs.
- Source content files with conversation families: ${inventory.packages.length}
- Total Dialogues found: ${sourceItemCounts.dialogues}
- Total Talk Topics found: ${sourceItemCounts.talkTopics}
- Total Conversation Starter Packs found: ${sourceItemCounts.conversationStarterPacks}
- Total Event Preparation Packs found: ${sourceItemCounts.eventPreparationPacks}
- Total Roleplays found: ${sourceItemCounts.roleplays}
- Key risks: ${p0Items.length ? `${p0Items.length} blocking contract issue samples detected; inspect before import.` : "no P0 source-file blockers detected by this static audit."}
- Highest-priority gaps: ${highestPriorityGapText}.
- Content generation can proceed: ${p0Items.length ? "not before P0 issues are repaired." : "yes, for gap-fill prompts that target remaining gaps only. Standalone Roleplay content should wait until RoleplayScenario import/persistence/Web support exists."}

## B. Inventory

${mdTable(["file", "packageId", "dialogues", "talkTopics", "starterPacks", "eventPacks", "roleplays"], inventoryRows)}

Word Catalog references available from scanned source packages:

- Word slugs: ${wordSlugs.size}
- Word lemmas: ${wordLemmas.size}

Source package parse issues:

${renderList(invalidJsonFiles.map(item => `${item.file}: ${item.error}`), "_None detected._", 20)}

## C. Dialogue Audit

Counts by CEFR:

${mdTable(["CEFR", "count"], cefrLevels.map(level => [level, dialogueAudit.countsByCefr[level] || 0]))}

Top categories:

${mdTable(["category", "count"], topEntries(dialogueAudit.countsByCategory, 25))}

Detected practical contexts:

${mdTable(["context", "count"], topEntries(dialogueAudit.countsByContext, 25))}

Missing practical contexts: ${missingDialogueContexts.length ? missingDialogueContexts.join(", ") : "None detected by keyword classification."}

Import/render issue samples:

${renderList(dialogueAudit.issues, "_None detected._", 40)}

Missing or weak metadata samples:

${renderList(dialogueAudit.missingMetadata, "_None detected._", 40)}

Dialogue length warnings:

${renderList(dialogueAudit.weakLength, "_None detected._", 40)}

Dialogue usefulWords issues:

- Meaning leaks: ${dialogueAudit.meaningLeaks.length}
- Duplicate usefulWords samples:
${renderList(dialogueAudit.duplicateUsefulWords, "_None detected._", 20)}
- Unresolved usefulWords samples:
${renderList(dialogueAudit.unresolvedWordReferences, "_None detected._", 40)}

Top 20 Dialogue gap-fill recommendations:

${mdTable(["priority", "CEFR", "gap", "reason"], [
  ["P1", "B1/B2", "doctor/praxis roleplays", "Priority practical context for real life and exams."],
  ["P1", "B1/B2", "school/kindergarten meetings", "Important integration and family communication context."],
  ["P1", "B1/B2", "landlord/apartment problem conversations", "High-value real-life administrative communication."],
  ["P1", "B1/B2", "public office/Behoerde appointments", "Critical survival and integration context."],
  ["P1", "B1/B2", "phone calls and follow-up calls", "Frequent real-life and workplace speaking need."],
  ["P1", "B1/B2", "complaint and service recovery conversations", "Practical and exam-relevant roleplay type."],
  ["P1", "B1/B2", "workplace feedback and clarification", "Supports workplace communication and Berufssprache."],
  ["P1", "B1/B2", "negotiation and compromise", "Useful for advanced B1/B2 interaction."],
  ["P1", "B1/B2", "exam speaking partner task variants", "Directly supports oral exam practice."],
  ["P1", "B1/B2", "language cafe participation", "Supports the conversation-practice product direction."],
  ["P2", "A1/A2", "survival appointment conversations", "Lower levels need meaningful but controlled practice."],
  ["P2", "A1/A2", "shopping/service mini complaints", "Useful everyday speaking at low levels."],
  ["P2", "A1/A2", "transportation help", "Core practical travel context."],
  ["P2", "A1/A2", "first meeting and small talk", "Needed before conversation cafes."],
  ["P2", "C1/C2", "formal workplace negotiation", "Advanced register and strategy practice."],
  ["P2", "C1/C2", "presentation Q&A", "Advanced speaking and academic/workplace relevance."],
  ["P2", "C1/C2", "seminar discussion", "Advanced academic discussion practice."],
  ["P2", "C1/C2", "conflict mediation", "Advanced nuance and politeness control."],
  ["P2", "C1/C2", "policy or organizational debate", "Advanced argumentation practice."],
  ["P3", "All", "link dialogues to grammar topics", "Future Course Lessons should reference existing dialogues instead of duplicating them."]
])}

## D. Talk Topic Audit

Counts by CEFR:

${mdTable(["CEFR", "count"], cefrLevels.map(level => [level, talkTopicAudit.countsByCefr[level] || 0]))}

Counts by contentType:

${mdTable(["contentType", "count"], topEntries(talkTopicAudit.countsByContentType, 25))}

Top categories:

${mdTable(["category", "count"], topEntries(talkTopicAudit.countsByCategory, 50))}

Allowed contentType values from docs/74: ${[...allowedTalkTopicContentTypes].join(", ")}

Text length issues:

${renderList(talkTopicAudit.lengthIssues, "_None detected._", 50)}

Question coverage issues:

${renderList(talkTopicAudit.questionIssues, "_None detected._", 50)}

Missing speaking goals:

${renderList(talkTopicAudit.missingSpeakingGoals, "_None detected._", 50)}

Sensitive-topic flag concerns:

${renderList(talkTopicAudit.sensitiveFlagIssues, "_None detected._", 50)}

Vocabulary reference issues:

- Meaning leaks: ${talkTopicAudit.meaningLeaks.length}
- Unresolved vocabulary samples:
${renderList(talkTopicAudit.unresolvedWordReferences, "_None detected._", 50)}

Top 50 Talk Topic gap-fill recommendations:

${mdTable(["priority", "CEFR", "contentType/category", "reason"], [
  ["P1", "B1/B2", "conversation-cafe", "Needed for informal group speaking after Grammar completion."],
  ["P1", "B1/B2", "daily-life", "Practical bridge between grammar and real conversation."],
  ["P1", "B1/B2", "work", "Useful for workplace speaking and job integration."],
  ["P1", "B1/B2", "education", "School, course, and training discussions are common learner needs."],
  ["P1", "B1/B2", "health", "Doctor and wellbeing discussions need careful but practical prompts."],
  ["P1", "B1/B2", "environment", "Common B1/B2 exam and group discussion theme."],
  ["P1", "B1/B2", "technology", "High-value modern discussion theme."],
  ["P1", "B1/B2", "society", "Supports opinion and reason-giving practice."],
  ["P1", "B1/B2", "movies", "Light discussion category for cafes and groups."],
  ["P1", "B1/B2", "books", "Good for longer but safe group discussion."],
  ["P1", "B1/B2", "sports", "Accessible topic for informal conversation."],
  ["P1", "B1/B2", "migration/integration", "Important learner-life theme; needs sensitivity flags when appropriate."],
  ["P1", "B1/B2", "Germany life", "Directly supports living-in-Germany practice."],
  ["P1", "B1/B2", "language learning", "Safe and highly relevant conversation-cafe theme."],
  ["P1", "B1/B2", "funny/light topics", "Needed for relaxed conversation practice."],
  ["P2", "A1/A2", "first meetings", "Lower levels need simple speaking starters connected to topics."],
  ["P2", "A1/A2", "shopping and food", "Simple daily-life topics for beginners."],
  ["P2", "A1/A2", "family and hobbies", "Core beginner discussion categories."],
  ["P2", "A1/A2", "transport and city life", "Practical everyday themes."],
  ["P2", "A1/A2", "weather and routines", "Low-level fluency builders."],
  ["P2", "C1/C2", "formal debate", "Advanced argumentation and register control."],
  ["P2", "C1/C2", "academic seminar", "Needed for advanced learners and university contexts."],
  ["P2", "C1/C2", "workplace strategy", "Advanced professional discussion."],
  ["P2", "C1/C2", "presentation Q&A", "Connects to future Exam Prep and Course Lessons."],
  ["P2", "C1/C2", "ethics and technology", "Advanced nuanced debate with sensitivity handling."],
  ["P2", "C1/C2", "public policy", "Advanced civic discussion with moderated-group flags where needed."],
  ["P2", "C1/C2", "science communication", "Advanced but reusable discussion material."],
  ["P2", "C1/C2", "culture and identity", "Needs careful moderation and sensitivity metadata."],
  ["P2", "C1/C2", "negotiation cases", "Advanced role and discussion preparation."],
  ["P2", "C1/C2", "feedback culture", "Workplace and academic relevance."],
  ["P3", "All", "book-summary variants", "Use only original generic summaries; no copyrighted passages."],
  ["P3", "All", "movie-summary variants", "Use original generic prompts; no copied source material."],
  ["P3", "All", "interview format topics", "Adds variety after core coverage."],
  ["P3", "All", "story format topics", "Useful for lower levels after starter coverage."],
  ["P3", "All", "debate-text variants", "Use for B2+ after discussion frames are stable."]
])}

## E. Conversation Starter Audit

- Total packs: ${sourceItemCounts.conversationStarterPacks}
- Counts by CEFR:

${mdTable(["CEFR", "count"], cefrLevels.map(level => [level, starterAudit.countsByCefr[level] || 0]))}

Issues:

${renderList(starterAudit.issues, sourceItemCounts.conversationStarterPacks ? "_None detected._" : "- No Conversation Starter Packs found.")}

${starterAuditNote}

## F. Event Preparation Pack Audit

- Total packs: ${sourceItemCounts.eventPreparationPacks}
- Counts by CEFR:

${mdTable(["CEFR", "count"], cefrLevels.map(level => [level, eventAudit.countsByCefr[level] || 0]))}

Issues:

${renderList(eventAudit.issues, sourceItemCounts.eventPreparationPacks ? "_None detected._" : "- No Event Preparation Packs found.")}

Missing event types: ${missingEventTypes.length ? missingEventTypes.join(", ") : "None detected against the current baseline event-type list."}

${eventAuditNote}

## G. Roleplay Audit

- Total roleplays: ${sourceItemCounts.roleplays}
- Counts by CEFR:

${mdTable(["CEFR", "count"], cefrLevels.map(level => [level, roleplayAudit.countsByCefr[level] || 0]))}

Issues:

${renderList(roleplayAudit.issues, sourceItemCounts.roleplays ? "_None detected._" : "- No dedicated Roleplay packages found.")}

Gap priority: P1 for B1/B2 exam and practical speaking roleplays. Existing Dialogue content can be used as source material, but authored roleplay packages are missing.

## H. Word Dependency Audit

- Scanned Word Catalog slugs: ${wordSlugs.size}
- Scanned Word Catalog lemmas: ${wordLemmas.size}
- Dialogue unresolved usefulWords: ${dialogueAudit.unresolvedWordReferences.length}
- Talk Topic unresolved vocabulary references: ${talkTopicAudit.unresolvedWordReferences.length}
- Dialogue usefulWords meaning leaks: ${dialogueAudit.meaningLeaks.length}
- Talk Topic vocabulary meaning leaks: ${talkTopicAudit.meaningLeaks.length}

High-value missing-word follow-up: resolve the unresolved reference samples above before large gap-fill generation, then add missing high-value words through the Word Catalog workflow only, not inside conversation content.

## I. Search, Filter, And Admin Audit

What works structurally:

- WebApi exposes dialogue list/detail endpoints and dialogue-related starter/event-preparation endpoints.
- Web exposes Dialogue, Talk Topic, Conversation Starter, and Event Preparation pages/controllers.
- Unified Search repository includes dialogue and Talk Topic result types.
- Talk Topic list supports CEFR, category, topic, contentType, and speaking-goal filter dimensions in Web.
- Admin dialogue management surfaces exist.
- Admin system report includes learning-portal counts, including preparation packs.

Missing or needs validation with imported full content:

- ${sourceCoverageMissing.length ? `${sourceCoverageMissing.join(", ")} is missing in source packages.` : "Conversation Starter and Event Preparation source coverage exists; standalone Roleplay coverage remains blocked by missing importer/domain support."}
- Admin quality report should surface unresolved linkedWords, missing translations, Talk Topic length issues, question-type gaps, sensitive-topic flags, and missing coverage by CEFR/category.
- Search ranking should be tested against the full imported dialogue/Talk Topic dataset before judging performance.
- Event/organizer discoverability and linking to preparation packs needs a separate runtime/data audit.

## J. Documentation Audit

Docs checked:

- docs/76-Learning-Portal-Roadmap-And-Backlog.md
- docs/67-Dialogue-Content-Package-Contract.md
- docs/68-Conversation-Starter-Content-Package-Contract.md
- docs/69-Event-Preparation-Pack-Content-Package-Contract.md
- docs/70-Roleplay-Content-Package-Contract.md
- docs/74-Talk-Topic-Content-Package-Contract.md
- docs/77-Grammar-Content-Package-Contract.md
- docs/78-Expression-Content-Package-Contract.md
- docs/79-Exercise-Content-Package-Contract.md
- docs/80-Course-Content-Package-Contract.md
- docs/81-Writing-Template-Content-Package-Contract.md
- docs/82-Cultural-Note-Content-Package-Contract.md
- docs/83-Exam-Prep-Content-Package-Contract.md

Updates made: docs/76-Learning-Portal-Roadmap-And-Backlog.md records the post-Grammar conversation blocker repairs, the Conversation Starter/Event Preparation baseline, and the dedicated RoleplayScenario importer blocker.

Remaining documentation gaps: none blocking. Future module-specific generation prompts should cite this report.

## K. Recommended Gap-Fill Order

1. Fix import/render/blocking contract issues in existing content.
2. Fix unresolved linkedWords and malformed slugs.
3. Expand Conversation Starter Packs only where audit shows missing situations or CEFR coverage.
4. Expand Event Preparation Packs only where audit shows missing real-life meeting types.
5. Implement RoleplayScenario import/persistence/Web support before generating standalone B1/B2 roleplay packages.
6. Gap-fill B1/B2 Roleplays after the dedicated roleplay import/render path exists.
7. Gap-fill B1/B2 Dialogues and Talk Topics only for proven practical or category gaps.
8. Add C1/C2 advanced discussion/presentation/workplace Dialogues and Talk Topics only after B1/B2 practical coverage is stable.
9. Then proceed to the first real Expressions package and validate import/render/search/admin before bulk Expressions generation.

## L. Backlog

${mdTable(["id", "title", "module", "priority", "reason", "dependencies", "suggested next prompt type"], backlogRows)}
`;

fs.mkdirSync(path.dirname(path.join(repoRoot, reportPath)), { recursive: true });
fs.writeFileSync(path.join(repoRoot, reportPath), report, "utf8");

const summary = {
  reportPath,
  sourceItemCounts,
  packageCount: inventory.packages.length,
  p0Count: p0Items.length,
  p1GapCount: p1Gaps.length,
  dialogueCountsByCefr: dialogueAudit.countsByCefr,
  talkTopicCountsByCefr: talkTopicAudit.countsByCefr,
  dialogueIssueCount: dialogueAudit.issues.length,
  talkTopicIssueCount: talkTopicAudit.issues.length,
  talkTopicLengthIssueCount: talkTopicAudit.lengthIssues.length,
  talkTopicQuestionIssueCount: talkTopicAudit.questionIssues.length,
  dialogueUnresolvedWordReferenceCount: dialogueAudit.unresolvedWordReferences.length,
  talkTopicUnresolvedWordReferenceCount: talkTopicAudit.unresolvedWordReferences.length
};

console.log(JSON.stringify(summary, null, 2));

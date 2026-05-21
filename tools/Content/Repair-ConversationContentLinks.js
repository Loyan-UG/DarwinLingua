#!/usr/bin/env node

const fs = require("fs");
const path = require("path");

const repoRoot = process.cwd();
const roots = [
  "content/generated",
  "src/Apps/DarwinLingua.WebApi/SeedContent"
];

const referenceRewrites = new Map([
  ["das-essen", { lemma: "essen", wordSlug: "essen" }],
  ["das-lernen", { lemma: "lernen", wordSlug: "lernen" }],
  ["das-lesen", { lemma: "lesen", wordSlug: "lesen" }],
  ["das-kochen", { lemma: "kochen", wordSlug: "kochen" }],
  ["das-leben", { lemma: "leben", wordSlug: "leben" }]
]);

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

function normalize(value) {
  return (value || "").toString().trim().toLowerCase();
}

function readJson(relativePath) {
  const absolutePath = path.join(repoRoot, relativePath);
  const text = fs.readFileSync(absolutePath, "utf8");
  return {
    hadBom: text.charCodeAt(0) === 0xFEFF,
    json: JSON.parse(text.replace(/^\uFEFF/, ""))
  };
}

function writeJson(relativePath, json, hadBom) {
  const absolutePath = path.join(repoRoot, relativePath);
  const text = `${hadBom ? "\uFEFF" : ""}${JSON.stringify(json, null, 2)}\n`;
  fs.writeFileSync(absolutePath, text, "utf8");
}

function repairWordReference(word) {
  const key = normalize(word.wordSlug || word.lemma || word.word);
  const replacement = referenceRewrites.get(key);
  if (!replacement) return false;

  if (Object.prototype.hasOwnProperty.call(word, "wordSlug")) word.wordSlug = replacement.wordSlug;
  if (Object.prototype.hasOwnProperty.call(word, "lemma")) word.lemma = replacement.lemma;
  if (Object.prototype.hasOwnProperty.call(word, "word")) word.word = replacement.lemma;
  return true;
}

function dedupeWordReferences(words) {
  if (!Array.isArray(words)) return { words, removed: 0 };

  const seen = new Set();
  const deduped = [];
  let removed = 0;

  for (const word of words) {
    const key = normalize(word.wordSlug || word.lemma || word.word);
    if (key && seen.has(key)) {
      removed += 1;
      continue;
    }

    if (key) seen.add(key);
    deduped.push(word);
  }

  return { words: deduped, removed };
}

function repairPackage(json) {
  let rewrittenReferences = 0;
  let removedDuplicateUsefulWords = 0;

  for (const topic of json.talkTopics || []) {
    for (const field of ["vocabularyItems", "importantWords", "linkedWords"]) {
      if (!Array.isArray(topic[field])) continue;
      for (const word of topic[field]) {
        if (repairWordReference(word)) rewrittenReferences += 1;
      }
    }
  }

  for (const dialogue of json.dialogues || []) {
    for (const field of ["usefulWords", "linkedWords"]) {
      if (!Array.isArray(dialogue[field])) continue;
      const result = dedupeWordReferences(dialogue[field]);
      if (result.removed) {
        dialogue[field] = result.words;
        removedDuplicateUsefulWords += result.removed;
      }
    }
  }

  return { rewrittenReferences, removedDuplicateUsefulWords };
}

const files = [...new Set(roots.flatMap(root => walkJsonFiles(root)))];
const totals = {
  filesChanged: 0,
  rewrittenReferences: 0,
  removedDuplicateUsefulWords: 0,
  parseErrors: []
};

for (const file of files) {
  let packageJson;
  try {
    packageJson = readJson(file);
  } catch (error) {
    totals.parseErrors.push({ file, error: error.message });
    continue;
  }

  const result = repairPackage(packageJson.json);
  if (!result.rewrittenReferences && !result.removedDuplicateUsefulWords) continue;

  writeJson(file, packageJson.json, packageJson.hadBom);
  totals.filesChanged += 1;
  totals.rewrittenReferences += result.rewrittenReferences;
  totals.removedDuplicateUsefulWords += result.removedDuplicateUsefulWords;
}

console.log(JSON.stringify(totals, null, 2));
if (totals.parseErrors.length) process.exitCode = 1;

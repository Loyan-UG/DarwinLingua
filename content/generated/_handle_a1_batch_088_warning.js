const fs = require('fs');
const path = require('path');
const root = 'D:/_Projects/DarwinLingua';
const sourcePath = path.join(root, 'content', 'A1.txt');
const failPath = path.join(root, 'content', 'generated', 'de-a1-import-failures.txt');
const processed = ['sicher','sie','Sie','sieben','siebzehn','siebzig'];
const failed = ['Sie'];
const jsonPath = path.join(root, 'content', 'generated', 'de-a1-generated-batch-088.json');
const log = [
  `Batch de-a1-generated-batch-088`,
  `JSON: ${jsonPath}`,
  `Import result: Entries imported: 5; Entries invalid: 0; Warnings: 1`,
  `Unresolved/possibly skipped word: ${failed.join(', ')}`,
  `Processed batch tokens removed from source to avoid duplicate generation: ${processed.join(', ')}`,
  ''
].join('\n');
fs.appendFileSync(failPath, log, 'utf8');
const raw = fs.readFileSync(sourcePath, 'utf8');
const tokens = raw.split(',').map(s => s.trim()).filter(Boolean);
const remove = new Map();
processed.forEach(w => remove.set(w, (remove.get(w) || 0) + 1));
const remaining = [];
for (const t of tokens) {
  const c = remove.get(t) || 0;
  if (c > 0) remove.set(t, c - 1); else remaining.push(t);
}
const leftovers = [...remove.entries()].filter(([,c]) => c !== 0);
if (leftovers.length) throw new Error(`Exact delete failed: ${JSON.stringify(leftovers)}`);
fs.writeFileSync(sourcePath, remaining.join(', '), 'utf8');
console.log(JSON.stringify({sourcePath, processed, failed, failPath, deleted:true, remainingCount: remaining.length, first10Remaining: remaining.slice(0,10)}, null, 2));

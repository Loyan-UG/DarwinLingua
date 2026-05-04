const fs = require('fs');
const path = require('path');
const root = 'D:/_Projects/DarwinLingua';
const sourcePath = path.join(root, 'content', 'C1.txt');
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const tokens = fs.readFileSync(sourcePath, 'utf8').split(',').map(s=>s.trim()).filter(Boolean);
console.log(JSON.stringify({count: tokens.length, first6: tokens.slice(0,6), first10: tokens.slice(0,10)}, null, 2));

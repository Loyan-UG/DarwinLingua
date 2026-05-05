const fs = require('fs');
const src = 'D:/_Projects/DarwinLingua/content/B2.txt';
const remove = ['Schwund','Sonderangebot','Speditionsauftrag','Speditionsladung','Sperre','Standardtour'];
let parts = fs.readFileSync(src, 'utf8').split(',').map(s => s.trim()).filter(Boolean);
for (const r of remove) {
  const idx = parts.indexOf(r);
  if (idx < 0) throw new Error('Token not found for deletion: ' + r);
  parts.splice(idx, 1);
}
fs.writeFileSync(src, parts.join(', '), 'utf8');
console.log('DELETED=true');
console.log('REMAINING_COUNT=' + parts.length);
console.log('FIRST10=' + parts.slice(0, 10).join(' | '));

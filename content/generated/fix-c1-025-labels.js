const fs = require('fs');
const pkgPath = 'D:/_Projects/DarwinLingua/content/generated/de-c1-generated-erp-025-words.json';
const taxPath = 'D:/_Projects/DarwinLingua/content/taxonomy/darwinlingua-taxonomy-v1.json';
const wanted = new Set(['business','workplace','process','administrative','analysis']);
const pkg = JSON.parse(fs.readFileSync(pkgPath, 'utf8'));
const tax = JSON.parse(fs.readFileSync(taxPath, 'utf8'));
pkg.packageId = 'de-c1-generated-erp-025-words';
pkg.labels = (tax.labels || []).filter(l => wanted.has(l.key)).map(l => ({
  kind: l.kind,
  key: l.key,
  displayName: l.displayName,
  sortOrder: l.sortOrder,
  localizations: l.localizations
}));
if (pkg.labels.length !== wanted.size) throw new Error('Could not copy all labels from taxonomy');
fs.writeFileSync(pkgPath, JSON.stringify(pkg, null, 2), 'utf8');

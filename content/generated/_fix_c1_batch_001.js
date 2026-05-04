const fs = require('fs');
const path = require('path');
const cp = require('child_process');
const root='D:/_Projects/DarwinLingua';
const sourcePath=path.join(root,'content','C1.txt');
const taxonomyPath=path.join(root,'content','taxonomy','darwinlingua-taxonomy-v1.json');
const inPath=path.join(root,'content','generated','de-c1-generated-batch-001.json');
const outPath=path.join(root,'content','generated','de-c1-generated-batch-001-final.json');
const taxonomy=JSON.parse(fs.readFileSync(taxonomyPath,'utf8'));
const labelKeys=['formal','written','business','workplace','administrative','management','technical','process','analysis','advanced','polite','spoken'];
const labels=labelKeys.map(key=>{const found=(taxonomy.labels||[]).find(l=>l.key===key); if(!found) throw new Error('Missing label '+key); return found;});
const pkg=JSON.parse(fs.readFileSync(inPath,'utf8'));
pkg.packageId='de-c1-generated-batch-001-final';
pkg.packageName='German C1 Generated Batch 001 Final';
pkg.labels=labels;
for (const e of pkg.entries) {
  e.contextLabels=[];
  e.usageLabels=(e.usageLabels||[]).filter(x=>labelKeys.includes(x));
  if (!e.usageLabels.includes('advanced')) e.usageLabels.push('advanced');
}
fs.writeFileSync(outPath,JSON.stringify(pkg,null,2),'utf8');
const project=path.join(root,'src','Apps','DarwinLingua.ImportTool','DarwinLingua.ImportTool.csproj');
let importOutput=''; let importOk=false;
try { importOutput=cp.execSync(`dotnet run --project "${project}" -- --target shared --yes "${outPath}"`,{cwd:root,encoding:'utf8',stdio:['ignore','pipe','pipe']}); importOk=/Entries imported:\s*6/.test(importOutput)&&/Entries invalid:\s*0/.test(importOutput)&&/Warnings:\s*0/.test(importOutput); }
catch(e){ importOutput=((e.stdout||'')+(e.stderr||'')).toString(); }
const words=['abberufen','abbitten','abblasen','abfließen','abgleiten','abgraben'];
const raw=fs.readFileSync(sourcePath,'utf8');
const tokens=raw.split(',').map(s=>s.trim()).filter(Boolean);
let deleted=false; let remaining=tokens;
if(importOk){ const remove=new Map(); words.forEach(w=>remove.set(w,(remove.get(w)||0)+1)); remaining=[]; for(const t of tokens){const c=remove.get(t)||0; if(c>0) remove.set(t,c-1); else remaining.push(t);} const leftovers=[...remove.entries()].filter(([,c])=>c!==0); if(leftovers.length) throw new Error('Exact delete failed: '+JSON.stringify(leftovers)); fs.writeFileSync(sourcePath,remaining.join(', '),'utf8'); deleted=true; }
console.log(JSON.stringify({sourcePath,words,outPath,importOk,deleted,importResult:{entriesImported:(importOutput.match(/Entries imported:\s*(\d+)/)||[])[1],entriesInvalid:(importOutput.match(/Entries invalid:\s*(\d+)/)||[])[1],warnings:(importOutput.match(/Warnings:\s*(\d+)/)||[])[1]},remainingCount:remaining.length,first10Remaining:remaining.slice(0,10)},null,2));

const fs=require('fs');const path=require('path');const cp=require('child_process');
const root='D:/_Projects/DarwinLingua',level='B2',levelLower='b2',batch='erp-001';
const srcPath=path.join(root,'content',`${level}.txt`),outDir=path.join(root,'content','generated');
const basePath=path.join(outDir,`de-${levelLower}-generated-${batch}.json`),collectionPath=path.join(outDir,`de-${levelLower}-generated-${batch}-collection.json`),wordPath=path.join(outDir,`de-${levelLower}-generated-${batch}-words.json`);
const expected=['Abfahrtstag','Abholauftrag','abweichender Auftraggeber','Aktionstyp','Aktivität','Angebotsposition'];
const splitTokens=s=>s.split(',').map(x=>x.trim()).filter(Boolean);
const tokens=splitTokens(fs.readFileSync(srcPath,'utf8')); const first=tokens.slice(0,expected.length); if(JSON.stringify(first)!==JSON.stringify(expected)) throw new Error(`Source head mismatch. Expected ${JSON.stringify(expected)}, got ${JSON.stringify(first)}`);
const base=JSON.parse(fs.readFileSync(basePath,'utf8'));
const collectionPkg={...base,packageId:`de-${levelLower}-generated-${batch}-collection-v2`,packageName:`German ${level} ERP Generated Batch 001 Collection`,collections:base.collections};
fs.writeFileSync(collectionPath,JSON.stringify(collectionPkg,null,2),'utf8');
const res=cp.spawnSync('dotnet',['run','--project',path.join(root,'content','generated','ImportNoInit','ImportNoInit.csproj'),'--',collectionPath],{encoding:'utf8',cwd:root,timeout:120000});
const output=`${res.stdout||''}${res.stderr||''}`; process.stdout.write(output);
const ok=res.status===0&&output.includes('Entries skipped as duplicates: 6')&&output.includes('Entries invalid: 0')&&output.includes('Warnings: 0');
if(!ok) throw new Error('Collection import failed strict criteria. Source not modified.');
const remaining=tokens.slice(expected.length); fs.writeFileSync(srcPath,remaining.join(', '),'utf8');
const failedPath=path.join(outDir,`${levelLower}-failed-words.txt`);
if(fs.existsSync(failedPath)){ const lines=fs.readFileSync(failedPath,'utf8').split(/\r?\n/).filter(line=>line && !line.includes(`\t${batch}\t`)); fs.writeFileSync(failedPath,lines.join('\n')+(lines.length?'\n':''),'utf8'); }
console.log(`\nJSON_PATH=${wordPath}`);console.log(`COLLECTION_JSON_PATH=${collectionPath}`);console.log(`SOURCE_PATH=${srcPath}`);console.log(`PROCESSED=${expected.join(' | ')}`);console.log(`COLLECTION=erp`);console.log(`SOURCE_UPDATED=yes`);console.log(`REMAINING_COUNT=${remaining.length}`);console.log(`FIRST_10=${remaining.slice(0,10).join(' | ')}`);

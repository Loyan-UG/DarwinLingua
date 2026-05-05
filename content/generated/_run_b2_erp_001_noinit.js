const fs=require('fs');const path=require('path');const cp=require('child_process');
const root='D:/_Projects/DarwinLingua',level='B2',levelLower='b2',batch='erp-001';
const srcPath=path.join(root,'content',`${level}.txt`),outDir=path.join(root,'content','generated');
const wordPath=path.join(outDir,`de-${levelLower}-generated-${batch}-words.json`);
const collectionPath=path.join(outDir,`de-${levelLower}-generated-${batch}-collection.json`);
const expected=['Abfahrtstag','Abholauftrag','abweichender Auftraggeber','Aktionstyp','Aktivität','Angebotsposition'];
const splitTokens=s=>s.split(',').map(x=>x.trim()).filter(Boolean);
const tokens=splitTokens(fs.readFileSync(srcPath,'utf8')); const first=tokens.slice(0,expected.length); if(JSON.stringify(first)!==JSON.stringify(expected)) throw new Error(`Source head mismatch. Expected ${JSON.stringify(expected)}, got ${JSON.stringify(first)}`);
function run(file){ const res=cp.spawnSync('dotnet',['run','--project',path.join(root,'content','generated','ImportNoInit','ImportNoInit.csproj'),'--',file],{encoding:'utf8',cwd:root,timeout:120000}); const output=`${res.stdout||''}${res.stderr||''}`; process.stdout.write(output); return {status:res.status,output}; }
const wordImport=run(wordPath);
const wordOk=wordImport.status===0&&wordImport.output.includes('Entries imported: 6')&&wordImport.output.includes('Entries invalid: 0')&&wordImport.output.includes('Warnings: 0');
if(!wordOk) throw new Error('Word import failed strict criteria. Source not modified.');
const collectionImport=run(collectionPath);
const collectionOk=collectionImport.status===0&&collectionImport.output.includes('Entries invalid: 0')&&collectionImport.output.includes('Warnings: 0');
if(!collectionOk) throw new Error('Collection import failed strict criteria. Source not modified.');
const remaining=tokens.slice(expected.length); fs.writeFileSync(srcPath,remaining.join(', '),'utf8');
const failedPath=path.join(outDir,`${levelLower}-failed-words.txt`);
if(fs.existsSync(failedPath)){ const lines=fs.readFileSync(failedPath,'utf8').split(/\r?\n/).filter(line=>line && !line.includes(`\t${batch}\t`)); fs.writeFileSync(failedPath,lines.join('\n')+(lines.length?'\n':''),'utf8'); }
console.log(`\nJSON_PATH=${wordPath}`);console.log(`COLLECTION_JSON_PATH=${collectionPath}`);console.log(`SOURCE_PATH=${srcPath}`);console.log(`PROCESSED=${expected.join(' | ')}`);console.log(`COLLECTION=erp`);console.log(`SOURCE_UPDATED=yes`);console.log(`REMAINING_COUNT=${remaining.length}`);console.log(`FIRST_10=${remaining.slice(0,10).join(' | ')}`);

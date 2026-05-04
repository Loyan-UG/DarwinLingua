const fs=require('fs'), path=require('path'), cp=require('child_process');
const root='D:/_Projects/DarwinLingua';
const sourcePath=path.join(root,'content/C1.txt');
const inPath=path.join(root,'content/generated/de-c1-generated-batch-010.json');
const outPath=path.join(root,'content/generated/de-c1-generated-batch-010-retry.json');
const words=['die Anpassungsfähigkeit','anpreisen','anreißen','anrennen','anscheißen','anschlagen'];
const tokens=fs.readFileSync(sourcePath,'utf8').split(',').map(s=>s.trim()).filter(Boolean);
const current=tokens.slice(0,6);
if(current.length!==6||words.some((w,i)=>current[i]!==w)) throw new Error('Unexpected first tokens before retry: '+JSON.stringify(current));
const pkg=JSON.parse(fs.readFileSync(inPath,'utf8'));
pkg.packageId='de-c1-generated-batch-010-retry';
pkg.packageName='German C1 Generated Batch 010 Retry';
const anpreisen=pkg.entries.find(e=>e.word==='anpreisen');
if(!anpreisen) throw new Error('anpreisen entry not found');
anpreisen.topics=['sales-and-customers','business-communication','shopping-and-services'];
fs.writeFileSync(outPath,JSON.stringify(pkg,null,2),'utf8');
const args=['run','--project',path.join(root,'src/Apps/DarwinLingua.ImportTool/DarwinLingua.ImportTool.csproj'),'--','--target','shared','--yes',outPath];
let output='';
try{output=cp.execFileSync('dotnet',args,{cwd:root,encoding:'utf8',stdio:['ignore','pipe','pipe']});}catch(e){output=(e.stdout||'')+(e.stderr||'');fs.writeFileSync(path.join(root,'content/generated/de-c1-import-failures.txt'),words.join(', ')+'\tbatch-010-retry\timport command failed\n',{flag:'a',encoding:'utf8'});console.log(JSON.stringify({sourcePath,words,outPath,importOutput:output,deleted:false,remainingCount:tokens.length,first10Remaining:tokens.slice(0,10)},null,2));process.exit(1);}
const ok=/Entries imported:\s*1\b/.test(output)&&/Entries skipped as duplicates:\s*5\b/.test(output)&&/Entries invalid:\s*0\b/.test(output)&&/Warnings:\s*0\b/.test(output);
let deleted=false, remaining=tokens;
if(ok){const counts=Object.fromEntries(words.map(w=>[w,1])); remaining=[]; for(const t of tokens){if(Object.prototype.hasOwnProperty.call(counts,t)&&counts[t]>0) counts[t]--; else remaining.push(t);} fs.writeFileSync(sourcePath,remaining.join(', '),'utf8'); deleted=true;} else {fs.writeFileSync(path.join(root,'content/generated/de-c1-import-failures.txt'),words.join(', ')+'\tbatch-010-retry\t'+output.replace(/\s+/g,' ').trim()+'\n',{flag:'a',encoding:'utf8'});}
console.log(JSON.stringify({sourcePath,words,outPath,importOutput:output,deleted,remainingCount:remaining.length,first10Remaining:remaining.slice(0,10)},null,2));

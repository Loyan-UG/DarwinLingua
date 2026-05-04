const fs=require('fs'), path=require('path');
const sourcePath='D:/_Projects/DarwinLingua/content/C1.txt';
const words=['die Anpassungsfähigkeit','anpreisen','anreißen','anrennen','anscheißen','anschlagen'];
const tokens=fs.readFileSync(sourcePath,'utf8').split(',').map(s=>s.trim()).filter(Boolean);
const current=tokens.slice(0,6);
if(current.length!==6||words.some((w,i)=>current[i]!==w)) throw new Error('Unexpected first tokens before cleanup: '+JSON.stringify(current));
const counts=Object.fromEntries(words.map(w=>[w,1]));
const remaining=[];
for(const t of tokens){ if(Object.prototype.hasOwnProperty.call(counts,t)&&counts[t]>0) counts[t]--; else remaining.push(t); }
fs.writeFileSync(sourcePath,remaining.join(', '),'utf8');
console.log(JSON.stringify({deleted:true,remainingCount:remaining.length,first10Remaining:remaining.slice(0,10)},null,2));

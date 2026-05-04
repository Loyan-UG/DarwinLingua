const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:\\_Projects\\DarwinLingua';
const level = 'A2';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const generatedDir = path.join(root, 'content', 'generated');
const packageId = 'de-a2-generated-batch-046-retry-001';
const outputPath = path.join(generatedDir, `${packageId}.json`);
const expected = ['die Großmutter','der Großvater','die Gruppenarbeit','die Gültigkeit','günstig','die Gurke'];
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const labelLangs = ['de', ...langs];
const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const sourceRaw = fs.readFileSync(sourcePath, 'utf8');
const tokens = sourceRaw.split(',').map(s => s.trim()).filter(Boolean);
const words = tokens.slice(0, 6);
if (JSON.stringify(words) !== JSON.stringify(expected)) throw new Error(`Unexpected first tokens before retry deletion: ${JSON.stringify(words)}`);
function complete(label){ const loc = label.localizations || []; return labelLangs.every(l => loc.some(x => x.language === l && x.name)); }
function getLabel(key){ const label = (taxonomy.labels || []).find(l => (l.key === key || l.id === key) && complete(l)); if (!label) throw new Error('Missing complete label: ' + key); return label; }
function topic(key){ if (!(taxonomy.topics || []).some(t => t.key === key || t.id === key)) throw new Error('Missing topic: ' + key); return key; }
function tr(ar,ckb,en,fa,kmr,pl,ro,ru,sq,trText){ return [{language:'ar',text:ar},{language:'ckb',text:ckb},{language:'en',text:en},{language:'fa',text:fa},{language:'kmr',text:kmr},{language:'pl',text:pl},{language:'ro',text:ro},{language:'ru',text:ru},{language:'sq',text:sq},{language:'tr',text:trText}]; }
function ex(baseText, translations){ return {baseText, translations}; }
const usedLabels = ['everyday','spoken','written','high-frequency'];
const entries = [
  {
    word:'die Gruppenarbeit', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'die', plural:'Gruppenarbeiten', infinitive:null, pronunciationIpa:null, syllableBreak:'Grup-pen-ar-beit',
    topics:[topic('education-and-training'), topic('work-and-jobs'), topic('meetings-and-presentations')], usageLabels:['everyday','spoken','written','high-frequency'], contextLabels:[],
    grammarNotes:['feminine noun; plural: die Gruppenarbeiten'], collocations:[{text:'in Gruppenarbeit arbeiten', meaning:'to work in group work'}], wordFamilies:[{lemma:'die Gruppe', relationLabel:'noun', note:'group'},{lemma:'arbeiten', relationLabel:'verb', note:'to work'}], relations:[],
    meanings:tr('عمل جماعي','کاری گرووپی','group work','کار گروهی','xebata komê','praca grupowa','lucru în grup','групповая работа','punë në grup','grup çalışması'),
    examples:[
      ex('Im Kurs machen wir heute Gruppenarbeit.', tr('في الدورة نقوم اليوم بعمل جماعي.','لە کۆرسەکەدا ئەمڕۆ کاری گرووپی دەکەین.','In the course we are doing group work today.','امروز در کلاس کار گروهی انجام می‌دهیم.','Di kursê de em îro xebata komê dikin.','Na kursie robimy dziś pracę grupową.','La curs facem astăzi lucru în grup.','На курсе мы сегодня делаем групповую работу.','Në kurs sot bëjmë punë në grup.','Kursta bugün grup çalışması yapıyoruz.')),
      ex('Die Gruppenarbeit dauert zwanzig Minuten.', tr('العمل الجماعي يستغرق عشرين دقيقة.','کاری گرووپی بیست خولەک دەخایەنێت.','The group work lasts twenty minutes.','کار گروهی بیست دقیقه طول می‌کشد.','Xebata komê bîst deqe dirêj dibe.','Praca grupowa trwa dwadzieścia minut.','Lucrul în grup durează douăzeci de minute.','Групповая работа длится двадцать минут.','Puna në grup zgjat njëzet minuta.','Grup çalışması yirmi dakika sürer.'))
    ]
  }
];
const pkg = {packageVersion:'1.0', packageId, packageName:'German A2 Generated Batch 046 Retry 001', source:'Hybrid', defaultMeaningLanguages:langs, labels:usedLabels.map(getLabel), entries, collections:[], scenarios:[], conversationStarterPacks:[], eventPreparationPacks:[]};
fs.writeFileSync(outputPath, JSON.stringify(pkg, null, 2), 'utf8');
const importCmd = `dotnet run --project "${path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj')}" -- --target shared --yes "${outputPath}"`;
let importOutput = '';
let retrySucceeded = false;
try { importOutput = cp.execSync(importCmd, {cwd: root, encoding: 'utf8', stdio:['ignore','pipe','pipe']}); retrySucceeded = /Entries imported:\s*1/.test(importOutput) && /Entries invalid:\s*0/.test(importOutput) && /Warnings:\s*0/.test(importOutput); }
catch (e) { importOutput = (e.stdout || '') + (e.stderr || '') + e.message; }
let removed = false;
let remainingTokens = tokens;
if (retrySucceeded) { remainingTokens = tokens.slice(6); fs.writeFileSync(sourcePath, remainingTokens.join(', '), 'utf8'); removed = true; }
console.log(JSON.stringify({sourcePath, processedWords:words, retryOutputPath:outputPath, retrySucceeded, retrySummary:(importOutput.match(/Entries imported:\s*\d+[\s\S]*?Warnings:\s*\d+/)||[''])[0].split(/\r?\n/).map(s=>s.trim()).filter(Boolean), removed, remainingCount:remainingTokens.length, first10Remaining:remainingTokens.slice(0,10)}, null, 2));

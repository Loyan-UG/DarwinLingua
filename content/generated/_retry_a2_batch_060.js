const fs = require('fs');
const path = require('path');
const cp = require('child_process');
const root = 'D:\\_Projects\\DarwinLingua';
const level = 'A2';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const generatedDir = path.join(root, 'content', 'generated');
const packageId = 'de-a2-generated-batch-060-retry-001';
const outputPath = path.join(generatedDir, `${packageId}.json`);
const expected = ['die Landkarte','laufen','der Lebenslauf','der Lebensmittelladen','ledig','die Lieblingsfarbe'];
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
  { word:'der Lebenslauf', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'der', plural:'Lebensläufe', infinitive:null, pronunciationIpa:null, syllableBreak:'Le-bens-lauf',
    topics:[topic('work-and-jobs'), topic('documents-and-administration'), topic('human-resources')], usageLabels:['everyday','written','high-frequency'], contextLabels:[], grammarNotes:['masculine noun; plural: die Lebensläufe'], collocations:[{text:'einen Lebenslauf schreiben', meaning:'to write a CV'}], wordFamilies:[{lemma:'das Leben', relationLabel:'noun', note:'life'},{lemma:'laufen', relationLabel:'verb', note:'to run; to go'}], relations:[],
    meanings:tr('سيرة ذاتية','ژیاننامە؛ CV','CV; résumé','رزومه؛ زندگی‌نامه کاری','CV; jiyanname','CV; życiorys','CV; curriculum vitae','резюме; автобиография','CV; jetëshkrim','özgeçmiş; CV'),
    examples:[
      ex('Bitte schicken Sie uns Ihren Lebenslauf.', tr('يرجى إرسال سيرتك الذاتية إلينا.','تکایە ژیاننامەکەتان بۆ ئێمە بنێرن.','Please send us your CV.','لطفاً رزومه خود را برای ما بفرستید.','Ji kerema xwe CV-ya xwe ji me re bişînin.','Proszę przesłać nam swoje CV.','Vă rugăm să ne trimiteți CV-ul dumneavoastră.','Пожалуйста, пришлите нам ваше резюме.','Ju lutemi na dërgoni CV-në tuaj.','Lütfen özgeçmişinizi bize gönderin.')),
      ex('Ich aktualisiere meinen Lebenslauf heute Abend.', tr('أحدث سيرتي الذاتية هذا المساء.','ئەم ئێوارە ژیاننامەکەم نوێ دەکەمەوە.','I am updating my CV this evening.','امشب رزومه‌ام را به‌روزرسانی می‌کنم.','Ez îvarê CV-ya xwe nû dikim.','Dziś wieczorem aktualizuję swoje CV.','În seara aceasta îmi actualizez CV-ul.','Сегодня вечером я обновляю своё резюме.','Sonte e përditësoj CV-në time.','Bu akşam özgeçmişimi güncelliyorum.'))
    ] },
  { word:'ledig', language:'de', cefrLevel:level, partOfSpeech:'Adjective', article:null, plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'le-dig',
    topics:[topic('documents-and-administration'), topic('social-and-relationships'), topic('everyday-life')], usageLabels:['everyday','written','high-frequency'], contextLabels:[], grammarNotes:['adjective; marital status, means not married'], collocations:[{text:'ledig sein', meaning:'to be single/unmarried'}], wordFamilies:[{lemma:'der Familienstand', relationLabel:'noun', note:'marital status'}], relations:[],
    meanings:tr('أعزب؛ غير متزوج','سەڵت؛ هاوسەرگیری نەکردوو','single; unmarried','مجرد؛ ازدواج‌نکرده','nezewicî; tenê','stanu wolnego; niezamężny/nieżonaty','necăsătorit','холостой; незамужняя','beqar; i/e pamartuar','bekâr'),
    examples:[
      ex('Im Formular steht: ledig.', tr('في الاستمارة مكتوب: أعزب.','لە فۆڕمەکەدا نووسراوە: سەڵت.','The form says: single.','در فرم نوشته شده: مجرد.','Di formê de nivîsî ye: nezewicî.','W formularzu jest napisane: stan wolny.','În formular scrie: necăsătorit.','В форме написано: холост/не замужем.','Në formular shkruan: beqar.','Formda şöyle yazıyor: bekâr.')),
      ex('Sie ist ledig und wohnt allein.', tr('هي عزباء وتعيش وحدها.','ئەو سەڵتە و بە تەنیا دەژیت.','She is single and lives alone.','او مجرد است و تنها زندگی می‌کند.','Ew nezewicî ye û bi tenê dijî.','Ona jest stanu wolnego i mieszka sama.','Ea este necăsătorită și locuiește singură.','Она не замужем и живёт одна.','Ajo është beqare dhe jeton vetëm.','O bekâr ve yalnız yaşıyor.'))
    ] }
];
const pkg = {packageVersion:'1.0', packageId, packageName:'German A2 Generated Batch 060 Retry 001', source:'Hybrid', defaultMeaningLanguages:langs, labels:usedLabels.map(getLabel), entries, collections:[], scenarios:[], conversationStarterPacks:[], eventPreparationPacks:[]};
fs.writeFileSync(outputPath, JSON.stringify(pkg, null, 2), 'utf8');
const importCmd = `dotnet run --project "${path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj')}" -- --target shared --yes "${outputPath}"`;
let importOutput = '';
let retrySucceeded = false;
try { importOutput = cp.execSync(importCmd, {cwd: root, encoding: 'utf8', stdio:['ignore','pipe','pipe']}); retrySucceeded = /Entries imported:\s*2/.test(importOutput) && /Entries invalid:\s*0/.test(importOutput) && /Warnings:\s*0/.test(importOutput); }
catch (e) { importOutput = (e.stdout || '') + (e.stderr || '') + e.message; }
let removed = false;
let remainingTokens = tokens;
if (retrySucceeded) { remainingTokens = tokens.slice(6); fs.writeFileSync(sourcePath, remainingTokens.join(', '), 'utf8'); removed = true; }
console.log(JSON.stringify({sourcePath, processedWords:words, retryOutputPath:outputPath, retrySucceeded, retrySummary:(importOutput.match(/Entries imported:\s*\d+[\s\S]*?Warnings:\s*\d+/)||[''])[0].split(/\r?\n/).map(s=>s.trim()).filter(Boolean), removed, remainingCount:remainingTokens.length, first10Remaining:remainingTokens.slice(0,10)}, null, 2));

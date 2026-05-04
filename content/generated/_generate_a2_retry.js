const fs = require('fs');
const path = require('path');
const root = 'D:/_Projects/DarwinLingua';
const taxonomy = JSON.parse(fs.readFileSync(path.join(root, 'content/taxonomy/darwinlingua-taxonomy-v1.json'), 'utf8'));
const outPath = path.join(root, 'content/generated/de-a2-generated-batch-023-retry.json');
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const labelLangs = ['de', ...langs];
function complete(label){ const loc=label.localizations||[]; return labelLangs.every(l=>loc.some(x=>x.language===l && x.name)); }
const usedLabels = ['everyday','spoken','written','customer-facing','polite','high-frequency'];
function getLabel(key){ const label=(taxonomy.labels||[]).find(l=>(l.key===key||l.id===key)&&complete(l)); if(!label) throw new Error('Missing complete label: '+key); return label; }
function tr(ar,ckb,en,fa,kmr,pl,ro,ru,sq,tr){ return [{language:'ar',text:ar},{language:'ckb',text:ckb},{language:'en',text:en},{language:'fa',text:fa},{language:'kmr',text:kmr},{language:'pl',text:pl},{language:'ro',text:ro},{language:'ru',text:ru},{language:'sq',text:sq},{language:'tr',text:tr}]; }
function ex(base, translations){ return { baseText: base, translations }; }
const entries = [{ word:'China', language:'de', cefrLevel:'A2', partOfSpeech:'Noun', article:null, plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'Chi-na', topics:['culture-and-media','transport-and-travel','everyday-life'], usageLabels:['written','spoken','high-frequency'], contextLabels:[], grammarNotes:['country name; usually used without article'], collocations:[{text:'nach China reisen',meaning:'to travel to China'},{text:'aus China kommen',meaning:'to come from China'}], wordFamilies:[{lemma:'chinesisch',relationLabel:'adjective',note:null}], relations:[], meanings:tr('الصين','چین','China','چین','Çîn','Chiny','China','Китай','Kina','Çin'), examples:[ex('Meine Kollegin reist nächste Woche nach China.',tr('زميلتي تسافر إلى الصين الأسبوع القادم.','هاوکارەکەم هەفتەی داهاتوو دەچێت بۆ چین.','My colleague is traveling to China next week.','همکارم هفته آینده به چین سفر می‌کند.','Hevkariya min hefteya bê diçe Çînê.','Moja koleżanka jedzie w przyszłym tygodniu do Chin.','Colega mea călătorește săptămâna viitoare în China.','Моя коллега на следующей неделе едет в Китай.','Kolegia ime udhëton javën tjetër në Kinë.','İş arkadaşım gelecek hafta Çin’e gidiyor.')),ex('Viele Produkte kommen aus China.',tr('تأتي كثير من المنتجات من الصين.','زۆر بەرهەم لە چینەوە دێن.','Many products come from China.','بسیاری از محصولات از چین می‌آیند.','Gelek berhem ji Çînê tên.','Wiele produktów pochodzi z Chin.','Multe produse vin din China.','Многие товары приходят из Китая.','Shumë produkte vijnë nga Kina.','Birçok ürün Çin’den geliyor.'))] }];
const pkg = { packageVersion:'1.0', packageId:'de-a2-generated-batch-023-retry', packageName:'German A2 Generated Batch 023 Retry', source:'Hybrid', defaultMeaningLanguages:langs, labels: usedLabels.map(getLabel), entries, collections:[], scenarios:[], conversationStarterPacks:[], eventPreparationPacks:[] };
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');
console.log(outPath);

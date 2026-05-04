const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');
const root = 'D:/_Projects/DarwinLingua';
const level = 'A2';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const generatedDir = path.join(root, 'content', 'generated');
const batchNo = 112;
const packageId = `de-a2-generated-batch-${String(batchNo).padStart(3,'0')}`;
const outPath = path.join(generatedDir, `${packageId}.json`);
const importProject = path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj');
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const labelLangs = ['de', ...langs];
const expected = ['die Zusatzinformation','zuverlässig','zweisprachig','die Zwiebel','zwischendurch'];
const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const sourceText = fs.readFileSync(sourcePath, 'utf8');
const tokens = sourceText.split(',').map(s => s.trim()).filter(Boolean);
const words = tokens.slice(0, 5);
if (JSON.stringify(words) !== JSON.stringify(expected)) throw new Error(`Unexpected first tokens: ${JSON.stringify(words)}`);
function complete(label){ const loc = label.localizations || []; return labelLangs.every(l => loc.some(x => x.language === l && x.name)); }
function getLabel(key){ const label = (taxonomy.labels || []).find(l => (l.key === key || l.id === key) && complete(l)); if(!label) throw new Error('Missing complete label: '+key); return label; }
function topic(key){ if (!(taxonomy.topics || []).some(t => t.key === key || t.id === key)) throw new Error('Missing topic: '+key); return key; }
function tr(ar,ckb,en,fa,kmr,pl,ro,ru,sq,trText){ return [{language:'ar',text:ar},{language:'ckb',text:ckb},{language:'en',text:en},{language:'fa',text:fa},{language:'kmr',text:kmr},{language:'pl',text:pl},{language:'ro',text:ro},{language:'ru',text:ru},{language:'sq',text:sq},{language:'tr',text:trText}]; }
function meaning(arr){ return tr(...arr); }
function ex(baseText, arr){ return { baseText, translations: tr(...arr) }; }
const usedLabels = ['everyday','spoken','written','customer-facing','polite','high-frequency'];
const entries = [
  {
    word:'die Zusatzinformation', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'die', plural:'Zusatzinformationen', infinitive:null, pronunciationIpa:null, syllableBreak:'Zu-satz-in-for-ma-ti-on',
    topics:[topic('documents-and-administration'), topic('customer-service'), topic('business-communication')], usageLabels:['everyday','written','spoken','customer-facing','polite'], contextLabels:[], grammarNotes:['feminine compound noun'],
    collocations:[{text:'eine Zusatzinformation geben', meaning:'to give additional information'}], wordFamilies:[{lemma:'zusätzlich', relationLabel:'adjective', note:null},{lemma:'die Information', relationLabel:'noun', note:null}], relations:[],
    meanings: meaning(['معلومة إضافية','زانیاریی زیادە','additional information','اطلاعات اضافی','agahiya zêde','dodatkowa informacja','informație suplimentară','дополнительная информация','informacion shtesë','ek bilgi']),
    examples:[
      ex('Auf dem Formular fehlt eine Zusatzinformation.', ['في الاستمارة تنقص معلومة إضافية.','لە فۆرمەکەدا زانیارییەکی زیادە کەمە.','An additional piece of information is missing on the form.','در فرم یک اطلاعات اضافی کم است.','Li ser formê agahiyeke zêde kêm e.','W formularzu brakuje dodatkowej informacji.','Pe formular lipsește o informație suplimentară.','В форме отсутствует дополнительная информация.','Në formular mungon një informacion shtesë.','Formda bir ek bilgi eksik.']),
      ex('Bitte lesen Sie die Zusatzinformation zur Buchung.', ['يرجى قراءة المعلومة الإضافية الخاصة بالحجز.','تکایە زانیاریی زیادەی حجزەکە بخوێننەوە.','Please read the additional information about the booking.','لطفاً اطلاعات اضافی مربوط به رزرو را بخوانید.','Ji kerema xwe agahiya zêde ya veqetandinê bixwînin.','Proszę przeczytać dodatkową informację do rezerwacji.','Vă rugăm să citiți informația suplimentară despre rezervare.','Пожалуйста, прочитайте дополнительную информацию о бронировании.','Ju lutem lexoni informacionin shtesë për rezervimin.','Lütfen rezervasyonla ilgili ek bilgiyi okuyun.'])
    ]
  },
  {
    word:'zuverlässig', language:'de', cefrLevel:level, partOfSpeech:'Adjective', article:null, plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'zu-ver-läs-sig',
    topics:[topic('work-and-jobs'), topic('customer-service'), topic('social-and-relationships')], usageLabels:['everyday','spoken','written','customer-facing','high-frequency'], contextLabels:[], grammarNotes:['adjective'],
    collocations:[{text:'zuverlässig arbeiten', meaning:'to work reliably'}], wordFamilies:[{lemma:'die Zuverlässigkeit', relationLabel:'noun', note:null}], relations:[],
    meanings: meaning(['موثوق؛ يعتمد عليه','جێی متمانە؛ پشتپێبەستراو','reliable; dependable','قابل اعتماد؛ مطمئن','bawerbar; pêbawer','niezawodny; godny zaufania','de încredere; fiabil','надежный','i besueshëm','güvenilir']),
    examples:[
      ex('Unser neuer Kollege ist sehr zuverlässig.', ['زميلنا الجديد موثوق جداً.','هاوکارە نوێیەکەمان زۆر جێی متمانەیە.','Our new colleague is very reliable.','همکار جدید ما خیلی قابل اعتماد است.','Hevkarê me yê nû pir bawerbar e.','Nasz nowy kolega jest bardzo niezawodny.','Noul nostru coleg este foarte de încredere.','Наш новый коллега очень надежный.','Kolegu ynë i ri është shumë i besueshëm.','Yeni iş arkadaşımız çok güvenilir.']),
      ex('Der Bus ist hier leider nicht sehr zuverlässig.', ['الحافلة هنا للأسف ليست موثوقة جداً.','پاسەکە لێرە بەداخەوە زۆر پشتپێبەستراو نییە.','Unfortunately, the bus here is not very reliable.','متأسفانه اتوبوس اینجا خیلی قابل اعتماد نیست.','Mixabin otobûs li vir ne pir bawerbar e.','Autobus tutaj niestety nie jest bardzo niezawodny.','Din păcate, autobuzul aici nu este foarte fiabil.','К сожалению, автобус здесь не очень надежный.','Fatkeqësisht autobusi këtu nuk është shumë i besueshëm.','Buradaki otobüs maalesef çok güvenilir değil.'])
    ]
  },
  {
    word:'zweisprachig', language:'de', cefrLevel:level, partOfSpeech:'Adjective', article:null, plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'zwei-spra-chig',
    topics:[topic('education-and-training'), topic('business-communication'), topic('customer-service')], usageLabels:['everyday','spoken','written','customer-facing'], contextLabels:[], grammarNotes:['adjective; means in two languages or bilingual'],
    collocations:[{text:'zweisprachige Informationen', meaning:'bilingual information'}], wordFamilies:[{lemma:'die Sprache', relationLabel:'noun', note:null},{lemma:'zweisprachig arbeiten', relationLabel:'phrase', note:null}], relations:[],
    meanings: meaning(['ثنائي اللغة','دووزمانە','bilingual; in two languages','دوزبانه','duzimanî','dwujęzyczny','bilingv','двуязычный','dygjuhësh','iki dilli']),
    examples:[
      ex('Die Broschüre ist zweisprachig: Deutsch und Englisch.', ['الكتيب ثنائي اللغة: الألمانية والإنجليزية.','بروشورەکە دووزمانەیە: ئەڵمانی و ئینگلیزی.','The brochure is bilingual: German and English.','بروشور دوزبانه است: آلمانی و انگلیسی.','Broşûr duzimanî ye: almanî û îngilîzî.','Broszura jest dwujęzyczna: niemiecki i angielski.','Broșura este bilingvă: germană și engleză.','Брошюра двуязычная: немецкий и английский.','Broshura është dygjuhëshe: gjermanisht dhe anglisht.','Broşür iki dilli: Almanca ve İngilizce.']),
      ex('Am Empfang arbeitet heute eine zweisprachige Mitarbeiterin.', ['تعمل اليوم موظفة ثنائية اللغة في الاستقبال.','ئەمڕۆ لە پێشوازی کارمەندێکی ژنی دووزمانە کار دەکات.','A bilingual employee is working at reception today.','امروز یک کارمند دوزبانه در پذیرش کار می‌کند.','Îro li pêşwaziyê karmendeke duzimanî dixebite.','Dziś w recepcji pracuje dwujęzyczna pracownica.','Astăzi lucrează la recepție o angajată bilingvă.','Сегодня на ресепшене работает двуязычная сотрудница.','Sot në recepsion punon një punonjëse dygjuhëshe.','Bugün resepsiyonda iki dilli bir çalışan görev yapıyor.'])
    ]
  },
  {
    word:'die Zwiebel', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'die', plural:'Zwiebeln', infinitive:null, pronunciationIpa:null, syllableBreak:'Zwie-bel',
    topics:[topic('everyday-life'), topic('shopping-and-services')], usageLabels:['everyday','spoken','customer-facing','high-frequency'], contextLabels:[], grammarNotes:['feminine noun'],
    collocations:[{text:'Zwiebeln schneiden', meaning:'to cut onions'}], wordFamilies:[], relations:[],
    meanings: meaning(['بصل؛ بصلة','پیاز','onion','پیاز','pîvaz','cebula','ceapă','лук; луковица','qepë','soğan']),
    examples:[
      ex('Für die Suppe brauchen wir eine Zwiebel.', ['نحتاج إلى بصلة للشوربة.','بۆ شۆرباکە پێویستمان بە پیازێک هەیە.','We need an onion for the soup.','برای سوپ به یک پیاز نیاز داریم.','Ji bo şorbê em hewceyê pîvazekê ne.','Do zupy potrzebujemy jednej cebuli.','Pentru supă avem nevoie de o ceapă.','Для супа нам нужна одна луковица.','Për supën na duhet një qepë.','Çorba için bir soğana ihtiyacımız var.']),
      ex('Möchten Sie die Pizza mit oder ohne Zwiebeln?', ['هل تريدون البيتزا مع البصل أم من دونه؟','پیتزاکەتان بە پیاز دەوێت یان بێ پیاز؟','Would you like the pizza with or without onions?','پیتزا را با پیاز می‌خواهید یا بدون پیاز؟','Hûn pizzayê bi pîvaz dixwazin an bê pîvaz?','Czy chce pan/pani pizzę z cebulą czy bez?','Doriți pizza cu sau fără ceapă?','Хотите пиццу с луком или без?','E dëshironi picën me apo pa qepë?','Pizzayı soğanlı mı soğansız mı istersiniz?'])
    ]
  },
  {
    word:'zwischendurch', language:'de', cefrLevel:level, partOfSpeech:'Adverb', article:null, plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'zwi-schen-durch',
    topics:[topic('everyday-life'), topic('work-and-jobs'), topic('education-and-training')], usageLabels:['everyday','spoken','written','high-frequency'], contextLabels:[], grammarNotes:['adverb; means in between or from time to time'],
    collocations:[{text:'zwischendurch eine Pause machen', meaning:'to take a break in between'}], wordFamilies:[{lemma:'zwischen', relationLabel:'preposition', note:null}], relations:[],
    meanings: meaning(['بين الحين والآخر؛ في الأثناء','لە نێواندا؛ جار جار','in between; from time to time','در میان؛ گاهی در بین کار','di navberê de; car caran','w międzyczasie; od czasu do czasu','între timp; din când în când','между делом; время от времени','ndërmjet; herë pas here','arada; zaman zaman']),
    examples:[
      ex('Wir machen zwischendurch eine kurze Pause.', ['سنأخذ استراحة قصيرة في الأثناء.','لە نێواندا پشوویەکی کورت دەدەین.','We take a short break in between.','در بین کار یک استراحت کوتاه می‌کنیم.','Em di navberê de bêhnvedaneke kurt dikin.','W międzyczasie robimy krótką przerwę.','Între timp facem o pauză scurtă.','Между делом мы делаем короткий перерыв.','Ndërmjet bëjmë një pushim të shkurtër.','Arada kısa bir mola veriyoruz.']),
      ex('Ich trinke zwischendurch gern einen Kaffee.', ['أحب أن أشرب قهوة بين الحين والآخر.','من لە نێواندا حەز دەکەم قاوەیەک بخۆمەوە.','I like to drink a coffee from time to time.','گاهی در میان کار قهوه می‌نوشم.','Ez car caran hez dikim qehweyekê vexwim.','Od czasu do czasu chętnie piję kawę.','Din când în când îmi place să beau o cafea.','Время от времени я люблю выпить кофе.','Herë pas here më pëlqen të pi një kafe.','Ara sıra kahve içmeyi severim.'])
    ]
  }
];
const pkg = { packageVersion:'1.0', packageId, packageName:`German ${level} Generated Batch ${String(batchNo).padStart(3,'0')}`, source:'Hybrid', defaultMeaningLanguages:langs, labels:usedLabels.map(getLabel), entries, collections:[], scenarios:[], conversationStarterPacks:[], eventPreparationPacks:[] };
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');
let output = '';
try { output = execFileSync('dotnet', ['run','--project',importProject,'--','--target','shared','--yes',outPath], { cwd: root, encoding:'utf8', stdio:['ignore','pipe','pipe'] }); }
catch (e) { output = `${e.stdout || ''}${e.stderr || ''}`; console.log(output); throw new Error('Import command failed'); }
const ok = output.includes('Entries imported: 5') && output.includes('Entries invalid: 0') && output.includes('Warnings: 0');
let deleted = false;
let remainingNow = tokens;
if (ok) { const removeSet = new Set(words); remainingNow = tokens.filter(t => !removeSet.has(t)); fs.writeFileSync(sourcePath, remainingNow.join(', '), 'utf8'); deleted = true; }
console.log(JSON.stringify({ sourcePath, words, outPath, importSummary: output.split(/\r?\n/).filter(l => /Entries imported|Entries invalid|Warnings/.test(l)), deleted, remainingCount: remainingNow.length, first10Remaining: remainingNow.slice(0,10) }, null, 2));

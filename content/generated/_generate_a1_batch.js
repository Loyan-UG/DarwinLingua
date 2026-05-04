const fs = require('fs');
const path = require('path');
const cp = require('child_process');
const root = 'D:/_Projects/DarwinLingua';
const level = 'A1';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const generatedDir = path.join(root, 'content', 'generated');
const batchNo = '111';
const packageId = `de-a1-generated-batch-${batchNo}`;
const outPath = path.join(generatedDir, `${packageId}.json`);
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const usedLabelKeys = ['everyday','spoken','written','customer-facing','polite','high-frequency'];
const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const labels = usedLabelKeys.map(key => {
  const found = (taxonomy.labels || []).find(l => l.key === key);
  if (!found) throw new Error(`Missing label: ${key}`);
  return found;
});
const raw = fs.readFileSync(sourcePath, 'utf8');
const tokens = raw.split(',').map(s => s.trim()).filter(Boolean);
const words = tokens.slice(0, 6);
const expected = ['zweimal','zweite','zwischen','zwölf'];
if (words.length !== 4) throw new Error(`Expected 4 words, found ${words.length}`);
if (JSON.stringify(words) !== JSON.stringify(expected)) throw new Error(`Unexpected words: ${JSON.stringify(words)}`);
function meanings(arr){ return langs.map((language,i)=>({language,text:arr[i]})); }
function translations(arr){ return langs.map((language,i)=>({language,text:arr[i]})); }
function ex(baseText, arr){ return {baseText, translations: translations(arr)}; }
const entries = [
  {
    word:'zweimal', language:'de', cefrLevel:level, partOfSpeech:'Adverb', article:null, plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'zwei-mal',
    topics:['everyday-life','work-and-jobs','education-and-training'], usageLabels:['everyday','spoken','written','high-frequency'], contextLabels:[], grammarNotes:['adverb of frequency'],
    collocations:[{text:'zweimal pro Woche', meaning:'twice a week'}], wordFamilies:[{lemma:'zwei', relationLabel:'number', note:null}], relations:[],
    meanings:meanings(['مرتين','دوو جار','twice','دو بار','du caran','dwa razy','de două ori','два раза','dy herë','iki kez']),
    examples:[
      ex('Ich gehe zweimal pro Woche schwimmen.', ['أذهب للسباحة مرتين في الأسبوع.','دوو جار لە هەفتەیەکدا دەچم مەلە.','I go swimming twice a week.','من هفته‌ای دو بار شنا می‌روم.','Ez heftê du caran diçim avjeniyê.','Chodzę pływać dwa razy w tygodniu.','Merg la înot de două ori pe săptămână.','Я хожу плавать два раза в неделю.','Shkoj të notoj dy herë në javë.','Haftada iki kez yüzmeye giderim.']),
      ex('Bitte wiederholen Sie das Wort zweimal.', ['من فضلك كرر الكلمة مرتين.','تکایە وشەکە دوو جار دووبارە بکەرەوە.','Please repeat the word twice.','لطفاً کلمه را دو بار تکرار کنید.','Ji kerema xwe peyvê du caran dubare bikin.','Proszę powtórzyć słowo dwa razy.','Vă rog să repetați cuvântul de două ori.','Пожалуйста, повторите слово два раза.','Ju lutem përsëriteni fjalën dy herë.','Lütfen kelimeyi iki kez tekrarlayın.'])
    ]
  },
  {
    word:'zweite', language:'de', cefrLevel:level, partOfSpeech:'Adjective', article:null, plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'zwei-te',
    topics:['everyday-life','documents-and-administration','education-and-training'], usageLabels:['everyday','spoken','written','high-frequency'], contextLabels:[], grammarNotes:['ordinal number form of zwei'],
    collocations:[{text:'die zweite Seite', meaning:'the second page'}], wordFamilies:[{lemma:'zwei', relationLabel:'cardinal number', note:null}], relations:[],
    meanings:meanings(['الثاني','دووەم','second','دوم','duyem','drugi','al doilea','второй','i dytë','ikinci']),
    examples:[
      ex('Lesen Sie bitte die zweite Seite.', ['من فضلك اقرأ الصفحة الثانية.','تکایە لاپەڕەی دووەم بخوێنەوە.','Please read the second page.','لطفاً صفحه دوم را بخوانید.','Ji kerema xwe rûpela duyem bixwînin.','Proszę przeczytać drugą stronę.','Vă rog să citiți a doua pagină.','Пожалуйста, прочитайте вторую страницу.','Ju lutem lexoni faqen e dytë.','Lütfen ikinci sayfayı okuyun.']),
      ex('Der zweite Termin ist morgen.', ['الموعد الثاني غدًا.','کاتی دووەم سبەیە.','The second appointment is tomorrow.','قرار دوم فرداست.','Hevdîtina duyem sibê ye.','Drugi termin jest jutro.','A doua programare este mâine.','Вторая встреча завтра.','Takimi i dytë është nesër.','İkinci randevu yarın.'])
    ]
  },
  {
    word:'zwischen', language:'de', cefrLevel:level, partOfSpeech:'Adverb', article:null, plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'zwi-schen',
    topics:['everyday-life','housing-and-real-estate','transport-and-travel'], usageLabels:['everyday','spoken','written','high-frequency'], contextLabels:[], grammarNotes:['common word for position or time; between'],
    collocations:[{text:'zwischen dem Tisch und der Tür', meaning:'between the table and the door'}], wordFamilies:[], relations:[],
    meanings:meanings(['بين','لە نێوان','between','بین؛ میان','di navbera','między','între','между','midis; ndërmjet','arasında']),
    examples:[
      ex('Der Stuhl steht zwischen Tisch und Tür.', ['الكرسي بين الطاولة والباب.','کورسییەکە لە نێوان مێز و دەرگادایە.','The chair is between the table and the door.','صندلی بین میز و در است.','Kursî di navbera masê û derî de ye.','Krzesło stoi między stołem a drzwiami.','Scaunul este între masă și ușă.','Стул стоит между столом и дверью.','Karrigia është midis tavolinës dhe derës.','Sandalye masa ile kapı arasında.']),
      ex('Der Termin ist zwischen neun und zehn Uhr.', ['الموعد بين الساعة التاسعة والعاشرة.','کاتەکە لە نێوان کاتژمێر نۆ و دەدایە.','The appointment is between nine and ten o’clock.','قرار بین ساعت نه و ده است.','Hevdîtin di navbera saet neh û deh de ye.','Termin jest między dziewiątą a dziesiątą.','Programarea este între ora nouă și zece.','Встреча между девятью и десятью часами.','Takimi është midis orës nëntë dhe dhjetë.','Randevu saat dokuz ile on arasında.'])
    ]
  },
  {
    word:'zwölf', language:'de', cefrLevel:level, partOfSpeech:'Adjective', article:null, plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'zwölf',
    topics:['everyday-life','shopping-and-services','documents-and-administration'], usageLabels:['everyday','spoken','written','high-frequency'], contextLabels:[], grammarNotes:['cardinal number'],
    collocations:[{text:'zwölf Uhr', meaning:'twelve o’clock'}], wordFamilies:[], relations:[],
    meanings:meanings(['اثنا عشر','دوازدە','twelve','دوازده','dwanzdeh','dwanaście','doisprezece','двенадцать','dymbëdhjetë','on iki']),
    examples:[
      ex('Die Pause beginnt um zwölf Uhr.', ['تبدأ الاستراحة في الساعة الثانية عشرة.','پشووکە کاتژمێر دوازدە دەست پێ دەکات.','The break starts at twelve o’clock.','استراحت ساعت دوازده شروع می‌شود.','Bêhnvedan saet dwanzdehê dest pê dike.','Przerwa zaczyna się o dwunastej.','Pauza începe la ora doisprezece.','Перерыв начинается в двенадцать часов.','Pushimi fillon në orën dymbëdhjetë.','Mola saat on ikide başlıyor.']),
      ex('Das kostet zwölf Euro.', ['هذا يكلف اثني عشر يورو.','ئەمە دوازدە یۆرۆیە.','That costs twelve euros.','این دوازده یورو قیمت دارد.','Ev dwanzdeh euro ye.','To kosztuje dwanaście euro.','Asta costă doisprezece euro.','Это стоит двенадцать евро.','Kjo kushton dymbëdhjetë euro.','Bu on iki avro tutuyor.'])
    ]
  }
];
const pkg = {packageVersion:'1.0',packageId,packageName:'German A1 Generated Batch 111',source:'Hybrid',defaultMeaningLanguages:langs,labels,entries,collections:[],scenarios:[],conversationStarterPacks:[],eventPreparationPacks:[]};
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');
const project = path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj');
let importOutput = ''; let importOk = false;
try {
  importOutput = cp.execSync(`dotnet run --project "${project}" -- --target shared --yes "${outPath}"`, {cwd: root, encoding:'utf8', stdio:['ignore','pipe','pipe']});
  importOk = /Entries imported:\s*4/.test(importOutput) && /Entries invalid:\s*0/.test(importOutput) && /Warnings:\s*0/.test(importOutput);
} catch(e) { importOutput = ((e.stdout||'') + (e.stderr||'')).toString(); }
let deleted = false; let remaining = tokens;
if (importOk) {
  const remove = new Map(); words.forEach(w => remove.set(w, (remove.get(w)||0)+1));
  remaining = [];
  for (const t of tokens) { const c = remove.get(t)||0; if (c>0) remove.set(t,c-1); else remaining.push(t); }
  const leftovers = [...remove.entries()].filter(([,c]) => c !== 0);
  if (leftovers.length) throw new Error(`Exact delete failed: ${JSON.stringify(leftovers)}`);
  fs.writeFileSync(sourcePath, remaining.join(', '), 'utf8');
  deleted = true;
}
console.log(JSON.stringify({sourcePath,words,outPath,importOk,deleted,importResult:{entriesImported:(importOutput.match(/Entries imported:\s*(\d+)/)||[])[1],entriesInvalid:(importOutput.match(/Entries invalid:\s*(\d+)/)||[])[1],warnings:(importOutput.match(/Warnings:\s*(\d+)/)||[])[1]},remainingCount:remaining.length,first10Remaining:remaining.slice(0,10)}, null, 2));

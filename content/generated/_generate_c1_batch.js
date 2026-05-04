const fs = require('fs');
const path = require('path');
const cp = require('child_process');
const root = 'D:/_Projects/DarwinLingua';
const level = 'C1';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const generatedDir = path.join(root, 'content', 'generated');
const batchNo = '002';
const packageId = `de-c1-generated-batch-${batchNo}`;
const outPath = path.join(generatedDir, `${packageId}.json`);
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const usedLabelKeys = ['formal','written','business','workplace','technical','administrative','analysis','process','advanced','polite'];
const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const labels = usedLabelKeys.map(key => {
  const found = (taxonomy.labels || []).find(l => l.key === key);
  if (!found) throw new Error(`Missing label: ${key}`);
  return found;
});
const raw = fs.readFileSync(sourcePath, 'utf8');
const tokens = raw.split(',').map(s => s.trim()).filter(Boolean);
const words = tokens.slice(0, 6);
const expected = ['abhelfen','ablassen','die Ableitung','abreiben','abringen','abschleifen'];
if (words.length !== 6) throw new Error(`Expected 6 words, found ${words.length}`);
if (JSON.stringify(words) !== JSON.stringify(expected)) throw new Error(`Unexpected words: ${JSON.stringify(words)}`);
function meanings(arr){ return langs.map((language,i)=>({language,text:arr[i]})); }
function translations(arr){ return langs.map((language,i)=>({language,text:arr[i]})); }
function ex(baseText, arr){ return {baseText, translations: translations(arr)}; }
const entries = [
  {
    word:'abhelfen', language:'de', cefrLevel:level, partOfSpeech:'Verb', article:null, plural:null, infinitive:'abhelfen', pronunciationIpa:null, syllableBreak:'ab-hel-fen',
    topics:['law-and-compliance','quality-and-risk','customer-service'], usageLabels:['formal','written','administrative','advanced'], contextLabels:[], grammarNotes:['separable verb; used with dative: einem Mangel abhelfen'],
    collocations:[{text:'einem Mangel abhelfen', meaning:'to remedy a defect or shortcoming'}], wordFamilies:[{lemma:'die Abhilfe', relationLabel:'noun', note:null}], relations:[],
    meanings:meanings(['يعالج؛ يضع حدًا لمشكلة','چارەسەرکردن؛ کێشە لابردن','to remedy; to rectify','رفع کردن؛ چاره کردن','çareser kirin','zaradzić; usunąć problem','a remedia; a corecta','устранить; исправить','të korrigjojë; të zgjidhë','gidermek; çare bulmak']),
    examples:[
      ex('Der Anbieter muss dem Sicherheitsmangel innerhalb von zehn Werktagen abhelfen.', ['يجب على المزود معالجة الخلل الأمني خلال عشرة أيام عمل.','دابینکەر دەبێت لە ماوەی دە ڕۆژی کاری چارەسەری کەموکوڕیی ئاسایش بکات.','The provider must remedy the security defect within ten working days.','ارائه‌دهنده باید ظرف ده روز کاری نقص امنیتی را رفع کند.','Dabînker divê di nav deh rojên kar de kêmasiya ewlehiyê çareser bike.','Dostawca musi usunąć lukę bezpieczeństwa w ciągu dziesięciu dni roboczych.','Furnizorul trebuie să remedieze deficiența de securitate în termen de zece zile lucrătoare.','Поставщик должен устранить недостаток безопасности в течение десяти рабочих дней.','Ofruesi duhet ta korrigjojë mangësinë e sigurisë brenda dhjetë ditëve pune.','Sağlayıcı güvenlik eksikliğini on iş günü içinde gidermelidir.']),
      ex('Mit der zusätzlichen Schulung wollte die Leitung den wiederkehrenden Fehlern im Supportprozess abhelfen.', ['أرادت الإدارة من خلال التدريب الإضافي معالجة الأخطاء المتكررة في عملية الدعم.','بەڕێوەبەرایەتی بە ڕاهێنانی زیادە ویستی هەڵە دووبارەبووەکانی پرۆسەی پشتگیری چارەسەر بکات.','With the additional training, management wanted to remedy recurring errors in the support process.','مدیریت می‌خواست با آموزش اضافی خطاهای تکرارشونده در فرایند پشتیبانی را رفع کند.','Rêveberî bi perwerdeya zêde dixwest xeletiyên dubare yên pêvajoya piştgiriyê çareser bike.','Dodatkowym szkoleniem kierownictwo chciało zaradzić powtarzającym się błędom w procesie wsparcia.','Prin instruirea suplimentară, conducerea a vrut să remedieze erorile recurente din procesul de suport.','Дополнительным обучением руководство хотело устранить повторяющиеся ошибки в процессе поддержки.','Me trajnimin shtesë, drejtimi donte të korrigjonte gabimet e përsëritura në procesin e mbështetjes.','Yönetim ek eğitimle destek sürecindeki tekrarlayan hataları gidermek istedi.'])
    ]
  },
  {
    word:'ablassen', language:'de', cefrLevel:level, partOfSpeech:'Verb', article:null, plural:null, infinitive:'ablassen', pronunciationIpa:null, syllableBreak:'ab-las-sen',
    topics:['production-and-maintenance','management-and-leadership','quality-and-risk'], usageLabels:['technical','written','business','advanced'], contextLabels:[], grammarNotes:['separable verb; literal draining or figurative refraining from something'],
    collocations:[{text:'Druck ablassen', meaning:'to release pressure'}], wordFamilies:[{lemma:'der Ablass', relationLabel:'noun', note:null}], relations:[],
    meanings:meanings(['يفرغ؛ يطلق؛ يمتنع عن','دەرکردن؛ بەردان؛ وازلێهێنان','to drain; to release; to refrain from','تخلیه کردن؛ رها کردن؛ دست برداشتن از','vala kirin; berdan; dev jê berdan','spuścić; wypuścić; odstąpić od','a evacua; a elibera; a renunța la','спустить; выпустить; отказаться от','të shkarkojë; të lëshojë; të heqë dorë','boşaltmak; salmak; vazgeçmek']),
    examples:[
      ex('Vor der Wartung muss der Techniker den Druck aus der Leitung kontrolliert ablassen.', ['قبل الصيانة يجب على الفني تفريغ الضغط من الخط بشكل مضبوط.','پێش چاکسازی، تەکنیککار دەبێت فشارەکە لە هێڵەکە بە شێوەی کۆنترۆڵکراو دەر بکات.','Before maintenance, the technician must release the pressure from the line in a controlled manner.','پیش از تعمیرات، تکنسین باید فشار را به‌صورت کنترل‌شده از خط تخلیه کند.','Berî lênêrînê, teknîsyen divê zextê ji xetê bi awayekî kontrolkirî derxe.','Przed konserwacją technik musi kontrolowanie spuścić ciśnienie z przewodu.','Înainte de mentenanță, tehnicianul trebuie să elibereze controlat presiunea din conductă.','Перед обслуживанием техник должен контролируемо стравить давление из линии.','Para mirëmbajtjes, tekniku duhet ta lirojë presionin nga linja në mënyrë të kontrolluar.','Bakım öncesinde teknisyen hattaki basıncı kontrollü şekilde boşaltmalıdır.']),
      ex('Nach der kritischen Rückmeldung ließ das Team von der geplanten Automatisierung zunächst ab.', ['بعد الملاحظة النقدية تراجع الفريق مؤقتًا عن الأتمتة المخطط لها.','دوای فیدباکی ڕەخنەگرانە، تیمەکە سەرەتا لە ئۆتۆماتیککردنی پلانکراو وازی هێنا.','After the critical feedback, the team initially refrained from the planned automation.','پس از بازخورد انتقادی، تیم ابتدا از اتوماسیون برنامه‌ریزی‌شده صرف‌نظر کرد.','Piştî bersiva rexnegirî, tîm destpêkê ji otomatîzekirina plansazkirî dev jê berda.','Po krytycznej opinii zespół początkowo odstąpił od planowanej automatyzacji.','După feedbackul critic, echipa a renunțat inițial la automatizarea planificată.','После критической обратной связи команда сначала отказалась от запланированной автоматизации.','Pas reagimit kritik, ekipi fillimisht hoqi dorë nga automatizimi i planifikuar.','Eleştirel geri bildirimden sonra ekip planlanan otomasyondan başlangıçta vazgeçti.'])
    ]
  },
  {
    word:'die Ableitung', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'die', plural:'Ableitungen', infinitive:null, pronunciationIpa:null, syllableBreak:'Ab-lei-tung',
    topics:['advanced-analysis','technology-and-it','data-and-reporting'], usageLabels:['technical','written','analysis','advanced'], contextLabels:[], grammarNotes:['feminine noun; used in technical, linguistic, mathematical, and analytical contexts'],
    collocations:[{text:'eine Ableitung herleiten', meaning:'to derive a derivative or conclusion'}], wordFamilies:[{lemma:'ableiten', relationLabel:'verb', note:null}], relations:[],
    meanings:meanings(['اشتقاق؛ استنتاج؛ تصريف','لێکدانەوە؛ دەرھێنان؛ ئاڕاستەکردنی دەرەوە','derivation; deduction; drainage line','اشتقاق؛ استنتاج؛ انشعاب/هدایت خروجی','derxistin; encamgirtin','wyprowadzenie; pochodna; odprowadzenie','derivare; deducție; evacuare','вывод; производная; отвод','derivim; nxjerrje; shkarkim','türetme; çıkarım; tahliye']),
    examples:[
      ex('Die Ableitung der Kennzahl muss im Bericht transparent dokumentiert werden.', ['يجب توثيق اشتقاق المؤشر في التقرير بشفافية.','دەبێت دەرھێنانی پێوەرەکە لە ڕاپۆرتدا بە ڕوونی تۆمار بکرێت.','The derivation of the metric must be documented transparently in the report.','نحوه استخراج شاخص باید در گزارش به‌صورت شفاف مستند شود.','Derxistina pîvanê divê di raporê de bi zelalî were belgekirin.','Wyprowadzenie wskaźnika musi być przejrzyście udokumentowane w raporcie.','Derivarea indicatorului trebuie documentată transparent în raport.','Вывод показателя должен быть прозрачно задокументирован в отчёте.','Derivimi i treguesit duhet të dokumentohet qartë në raport.','Metrik türetimi raporda şeffaf biçimde belgelenmelidir.']),
      ex('Die fehlerhafte Ableitung des Regenwassers führte zu Schäden an der Fassade.', ['أدى التصريف الخاطئ لمياه الأمطار إلى أضرار في الواجهة.','ئاڕاستەکردنی هەڵەی ئاوی باران بووە هۆی زیان بە ڕووی بیناکە.','The faulty drainage of rainwater caused damage to the facade.','هدایت نادرست آب باران باعث آسیب به نمای ساختمان شد.','Şandina şaş a ava baranê bû sedema ziyanê li rûyê avahiyê.','Wadliwe odprowadzenie wody deszczowej spowodowało uszkodzenia elewacji.','Evacuarea defectuoasă a apei pluviale a provocat daune fațadei.','Неправильный отвод дождевой воды привёл к повреждению фасада.','Shkarkimi i gabuar i ujit të shiut shkaktoi dëmtime në fasadë.','Yağmur suyunun hatalı tahliyesi cephede hasara yol açtı.'])
    ]
  },
  {
    word:'abreiben', language:'de', cefrLevel:level, partOfSpeech:'Verb', article:null, plural:null, infinitive:'abreiben', pronunciationIpa:null, syllableBreak:'ab-rei-ben',
    topics:['production-and-maintenance','healthcare-and-appointments','quality-and-risk'], usageLabels:['technical','written','process','advanced'], contextLabels:[], grammarNotes:['separable verb; to rub off or wipe down'],
    collocations:[{text:'eine Oberfläche abreiben', meaning:'to rub or wipe down a surface'}], wordFamilies:[{lemma:'der Abrieb', relationLabel:'noun', note:null}], relations:[],
    meanings:meanings(['يفرك؛ يمسح؛ يزيل بالاحتكاك','سڕینەوە؛ پاککردنەوە بە سوڕاندن','to rub off; to wipe down','مالیدن و پاک کردن؛ ساییدن','paqij kirin bi firandin','zetrzeć; wytrzeć','a freca; a șterge','стереть; протереть','të fërkojë; të fshijë','ovmak; silmek']),
    examples:[
      ex('Vor der Lackierung muss die Werkstatt die Oberfläche gründlich abreiben.', ['قبل الطلاء يجب على الورشة فرك السطح جيدًا.','پێش ڕەنگکردن، کارگەکە دەبێت ڕووبەرەکە بە وردی بسڕێتەوە.','Before painting, the workshop must rub down the surface thoroughly.','پیش از رنگ‌کاری، کارگاه باید سطح را کاملاً سایش و پاک کند.','Berî boyaxkirinê, karxane divê rûberê bi hûrgilî paqij bike.','Przed lakierowaniem warsztat musi dokładnie przetrzeć powierzchnię.','Înainte de vopsire, atelierul trebuie să frece temeinic suprafața.','Перед покраской мастерская должна тщательно зачистить поверхность.','Para lyerjes, punishtja duhet ta fërkojë mirë sipërfaqen.','Boyamadan önce atölye yüzeyi iyice ovup temizlemelidir.']),
      ex('Die Ärztin rieb die Einstichstelle mit Desinfektionsmittel ab.', ['مسحت الطبيبة موضع الحقن بمطهر.','پزیشکەکە شوێنی دەرزیکەوتنەکەی بە مادەی دژەباکتری پاککردەوە.','The doctor wiped the injection site with disinfectant.','پزشک محل تزریق را با ماده ضدعفونی‌کننده پاک کرد.','Bijîjkê jin cihê derzîlêdanê bi madeya dezenfektan paqij kir.','Lekarka przetarła miejsce wkłucia środkiem dezynfekującym.','Medicul a șters locul injecției cu dezinfectant.','Врач протёр место укола дезинфицирующим средством.','Mjekja e fshiu vendin e shpimit me dezinfektues.','Doktor enjeksiyon yerini dezenfektanla sildi.'])
    ]
  },
  {
    word:'abringen', language:'de', cefrLevel:level, partOfSpeech:'Verb', article:null, plural:null, infinitive:'abringen', pronunciationIpa:null, syllableBreak:'ab-rin-gen',
    topics:['contracts-and-negotiation','management-and-leadership','business-communication'], usageLabels:['formal','business','written','advanced'], contextLabels:[], grammarNotes:['separable verb; often used as jemandem etwas abringen'],
    collocations:[{text:'jemandem ein Zugeständnis abringen', meaning:'to wrest a concession from someone'}], wordFamilies:[], relations:[],
    meanings:meanings(['ينتزع؛ يحصل بصعوبة','بە زەحمەت وەرگرتن؛ دەرھێنان','to wrest; to extract with difficulty','به‌سختی گرفتن؛ بیرون کشیدن','bi zehmet standin','wydrzeć; wymóc','a smulge; a obține cu greu','вырвать; добиться с трудом','të nxjerrë me vështirësi','koparmak; güçlükle almak']),
    examples:[
      ex('In den Verhandlungen konnte die Gewerkschaft dem Arbeitgeber nur geringe Zugeständnisse abringen.', ['في المفاوضات استطاعت النقابة انتزاع تنازلات قليلة فقط من صاحب العمل.','لە دانوستانەکاندا سەندیکا تەنها چەند سازشێکی کەم لە خاوەنکار بە زەحمەت وەرگرت.','In the negotiations, the union was able to extract only minor concessions from the employer.','در مذاکرات، اتحادیه فقط توانست امتیازهای اندکی از کارفرما بگیرد.','Di danûstandinan de sendîka tenê çend destdanên biçûk ji kardêr bi zehmet stand.','W negocjacjach związek zdołał wymóc na pracodawcy jedynie niewielkie ustępstwa.','În negocieri, sindicatul a putut obține de la angajator doar concesii minore.','На переговорах профсоюзу удалось добиться от работодателя лишь небольших уступок.','Në negociata, sindikata arriti t’i nxirrte punëdhënësit vetëm lëshime të vogla.','Müzakerelerde sendika işverenden yalnızca küçük tavizler koparabildi.']),
      ex('Der Analyst rang den Rohdaten eine plausible Erklärung für den Umsatzrückgang ab.', ['استخرج المحلل من البيانات الخام تفسيرًا معقولًا لانخفاض المبيعات.','شیکەرەوەکە لە داتای خامدا ڕوونکردنەوەیەکی باوەڕپێکراو بۆ دابەزینی فرۆشتن دەرھێنا.','The analyst wrested a plausible explanation for the revenue decline from the raw data.','تحلیل‌گر از داده‌های خام توضیحی قابل قبول برای افت فروش بیرون کشید.','Analîst ji daneyên xav ravekirineke maqûl ji bo kêmkirina firotanê derxist.','Analityk wydobył z surowych danych wiarygodne wyjaśnienie spadku przychodów.','Analistul a extras din datele brute o explicație plauzibilă pentru scăderea veniturilor.','Аналитик извлёк из сырых данных правдоподобное объяснение падения выручки.','Analisti nxori nga të dhënat e papërpunuara një shpjegim të besueshëm për rënien e të ardhurave.','Analist ham verilerden gelir düşüşü için makul bir açıklama çıkardı.'])
    ]
  },
  {
    word:'abschleifen', language:'de', cefrLevel:level, partOfSpeech:'Verb', article:null, plural:null, infinitive:'abschleifen', pronunciationIpa:null, syllableBreak:'ab-schlei-fen',
    topics:['production-and-maintenance','quality-and-risk','housing-and-real-estate'], usageLabels:['technical','written','process','advanced'], contextLabels:[], grammarNotes:['separable verb; to sand or grind down'],
    collocations:[{text:'Kanten abschleifen', meaning:'to sand down edges'}], wordFamilies:[{lemma:'der Schliff', relationLabel:'related noun', note:null}], relations:[],
    meanings:meanings(['يصقل؛ يسنفر؛ يزيل بالصنفرة','سایین؛ سافکردن بە ساندپەیپەر','to sand down; to grind off','سنباده زدن؛ ساییدن','sivandin; aş kirin','zeszlifować; oszlifować','a șlefui; a poliza','отшлифовать; сточить','të lëmojë; të grijë','zımparalamak; taşlamak']),
    examples:[
      ex('Vor der Montage müssen die scharfen Kanten am Gehäuse abgeschliffen werden.', ['قبل التركيب يجب صقل الحواف الحادة في الهيكل.','پێش دامەزراندن، دەبێت لێوارە تیژەکانی قاپەکە بسایرێن.','Before assembly, the sharp edges on the housing must be sanded down.','پیش از مونتاژ باید لبه‌های تیز بدنه سنباده زده شوند.','Berî montajê divê keviyên tûj ên qalikê werin sivandin.','Przed montażem ostre krawędzie obudowy muszą zostać oszlifowane.','Înainte de montaj, marginile ascuțite ale carcasei trebuie șlefuite.','Перед сборкой острые края корпуса необходимо отшлифовать.','Para montimit, skajet e mprehta të kasës duhet të lëmohen.','Montajdan önce gövdedeki keskin kenarlar zımparalanmalıdır.']),
      ex('Der Restaurator schleifte die alte Lackschicht vorsichtig ab, ohne das Holz zu beschädigen.', ['قام المرمم بصنفرة طبقة الطلاء القديمة بحذر من دون إتلاف الخشب.','نۆژەنکەرەوەکە بە وریایی چینە ڕەنگی کۆنی سایەوە بێ ئەوەی دارەکە زیان پێبگات.','The restorer carefully sanded off the old varnish layer without damaging the wood.','مرمت‌گر لایه قدیمی لاک را با احتیاط سنباده زد، بدون آنکه به چوب آسیب بزند.','Nûjenker bi baldarî qata lakê kevn sivand bê ku darê xera bike.','Konserwator ostrożnie zeszlifował starą warstwę lakieru, nie uszkadzając drewna.','Restauratorul a șlefuit cu grijă stratul vechi de lac fără să deterioreze lemnul.','Реставратор осторожно снял старый слой лака, не повредив дерево.','Restauruesi e lëmoi me kujdes shtresën e vjetër të llakut pa dëmtuar drurin.','Restoratör eski vernik katmanını ahşaba zarar vermeden dikkatlice zımparaladı.'])
    ]
  }
];
const pkg = {packageVersion:'1.0',packageId,packageName:'German C1 Generated Batch 002',source:'Hybrid',defaultMeaningLanguages:langs,labels,entries,collections:[],scenarios:[],conversationStarterPacks:[],eventPreparationPacks:[]};
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');
const project = path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj');
let importOutput = ''; let importOk = false;
try {
  importOutput = cp.execSync(`dotnet run --project "${project}" -- --target shared --yes "${outPath}"`, {cwd: root, encoding:'utf8', stdio:['ignore','pipe','pipe']});
  importOk = /Entries imported:\s*6/.test(importOutput) && /Entries invalid:\s*0/.test(importOutput) && /Warnings:\s*0/.test(importOutput);
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

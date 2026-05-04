const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const taxonomyPath = path.join(root, 'content/taxonomy/darwinlingua-taxonomy-v1.json');
const sourcePath = path.join(root, 'content/C1.txt');
const outPath = path.join(root, 'content/generated/de-c1-generated-batch-003.json');
const packageId = 'de-c1-generated-batch-003';
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const sourceText = fs.readFileSync(sourcePath, 'utf8');
const tokens = sourceText.split(',').map(s => s.trim()).filter(Boolean);
const words = tokens.slice(0, 6);
const expected = ['abschwören','absitzen','abstechen','abstehen','abstoßen','abstrahieren'];
if (words.length !== 6 || expected.some((w,i) => words[i] !== w)) {
  throw new Error('Unexpected first tokens: ' + JSON.stringify(words));
}

function labelObj(key) {
  const found = (taxonomy.labels || []).find(l => l.key === key);
  if (!found) throw new Error('Missing taxonomy label: ' + key);
  return found;
}
function entry(e) {
  return Object.assign({
    language: 'de',
    cefrLevel: 'C1',
    pronunciationIpa: null,
    contextLabels: [],
    relations: []
  }, e);
}
function meaning(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) {
  return [ar,ckb,en,fa,kmr,pl,ro,ru,sq,tr].map((text, i) => ({ language: langs[i], text }));
}
function ex(baseText, ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) {
  return { baseText, translations: meaning(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) };
}

const entries = [
  entry({
    word: 'abschwören', partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'abschwören', syllableBreak: 'ab-schwö-ren',
    topics: ['law-and-compliance','social-and-relationships','management-and-leadership'],
    usageLabels: ['formal','written','advanced'],
    grammarNotes: ['strong verb; often used with dative object'],
    collocations: [{ text: 'einer Praxis abschwören', meaning: 'to renounce a practice' }, { text: 'einer Überzeugung abschwören', meaning: 'to renounce a conviction' }],
    wordFamilies: [{ lemma: 'der Schwur', relationLabel: 'noun', note: null }],
    meanings: meaning('يتخلى رسميًا عن؛ ينبذ', 'دەستبەرداری دەبێت لە؛ بە فەرمی وازی لێ دێنێت', 'to renounce; to swear off', 'رسماً کنار گذاشتن؛ دست کشیدن از', 'dev jê berdan; sond xwarin ku nema bike', 'wyrzec się; odstąpić od', 'a renunța la; a se dezice de', 'отрекаться от; зарекаться', 'të heqësh dorë; të betohesh se nuk do ta bësh më', 'vazgeçmek; tövbe etmek'),
    examples: [
      ex('Nach dem Compliance-Verstoß schwor der Manager allen informellen Absprachen mit Lieferanten ab.', 'بعد انتهاك قواعد الامتثال، تخلى المدير عن كل التفاهمات غير الرسمية مع الموردين.', 'دوای پێشێلکردنی پابەندبوون، بەڕێوەبەرەکە دەستبەرداری هەموو ڕێککەوتنە نافەرمییەکان لەگەڵ دابینکەران بوو.', 'After the compliance violation, the manager renounced all informal arrangements with suppliers.', 'پس از تخلف در انطباق، مدیر از همه توافق‌های غیررسمی با تأمین‌کنندگان دست کشید.', 'Piştî binpêkirina lihevhatinê, rêveber ji hemû lihevkirinên nefermî bi dabînkeran re dev berda.', 'Po naruszeniu zasad zgodności menedżer wyrzekł się wszystkich nieformalnych ustaleń z dostawcami.', 'După încălcarea regulilor de conformitate, managerul a renunțat la toate înțelegerile informale cu furnizorii.', 'После нарушения требований комплаенса менеджер отказался от всех неформальных договоренностей с поставщиками.', 'Pas shkeljes së rregullave të përputhshmërisë, menaxheri hoqi dorë nga të gjitha marrëveshjet joformale me furnitorët.', 'Uyumluluk ihlalinden sonra yönetici tedarikçilerle yapılan tüm gayriresmî anlaşmalardan vazgeçti.'),
      ex('Die Zeugin schwor ihrer früheren Aussage nicht ab, relativierte aber mehrere Details.', 'لم تتراجع الشاهدة عن إفادتها السابقة، لكنها خففت من أهمية عدة تفاصيل.', 'شایەتەکە دەستبەرداری لێدوانی پێشووی نەبوو، بەڵام چەند وردەکارییەکی ڕێژەیی کرد.', 'The witness did not renounce her earlier statement, but she qualified several details.', 'شاهد از گفته قبلی خود برنگشت، اما چند جزئیات را نسبی‌تر بیان کرد.', 'Şahid ji daxuyaniya xwe ya berê venegeriya, lê çend hûrgulî bi şert û mercan got.', 'Świadek nie wycofała wcześniejszego zeznania, ale doprecyzowała kilka szczegółów.', 'Martora nu s-a dezis de declarația anterioară, dar a nuanțat mai multe detalii.', 'Свидетельница не отказалась от прежних показаний, но уточнила несколько деталей.', 'Dëshmitarja nuk hoqi dorë nga deklarata e mëparshme, por zbuti disa hollësi.', 'Tanık önceki ifadesinden dönmedi, ancak birkaç ayrıntıyı yumuşatarak açıkladı.')
    ]
  }),
  entry({
    word: 'absitzen', partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'absitzen', syllableBreak: 'ab-sit-zen',
    topics: ['law-and-compliance','work-and-jobs','public-services'], usageLabels: ['formal','written','advanced'],
    grammarNotes: ['separable verb'], collocations: [{ text: 'eine Haftstrafe absitzen', meaning: 'to serve a prison sentence' }, { text: 'eine Sitzung absitzen', meaning: 'to sit through a meeting reluctantly' }],
    wordFamilies: [{ lemma: 'sitzen', relationLabel: 'verb', note: null }],
    meanings: meaning('يقضي عقوبة أو فترة؛ يتحمل الجلوس خلالها', 'ماوەیەک یان سزایەک تێدەپەڕێنێت', 'to serve or sit out a sentence or period', 'دوران محکومیت یا مدتی را گذراندن', 'maweyek an cezayek derbas kirin', 'odsiedzieć; przeczekać', 'a executa o pedeapsă; a sta până la capăt', 'отбывать срок; высиживать', 'të vuash një dënim; të rrish deri në fund', 'cezasını çekmek; oturup bitmesini beklemek'),
    examples: [
      ex('Der Verurteilte musste die restliche Haftstrafe ohne Bewährung absitzen.', 'كان على المدان أن يقضي بقية عقوبة السجن دون إفراج مشروط.', 'حوکم‌دراوەکە دەبوو ماوەی ماوەیی زیندانەکە بەبێ لێخۆشبوون تێپەڕێنێت.', 'The convicted man had to serve the remaining prison sentence without probation.', 'فرد محکوم باید باقی‌مانده حکم زندان را بدون تعلیق می‌گذراند.', 'Kesê mehkûm diviya mayîna cezayê girtîgehê bê betalkirin derbas bikira.', 'Skazany musiał odsiedzieć resztę kary pozbawienia wolności bez zawieszenia.', 'Condamnatul a trebuit să execute restul pedepsei cu închisoarea fără suspendare.', 'Осужденный должен был отбыть оставшийся срок без условного освобождения.', 'I dënuari duhej të vuante pjesën e mbetur të dënimit pa lirim me kusht.', 'Hükümlü kalan hapis cezasını erteleme olmadan çekmek zorundaydı.'),
      ex('Viele Mitarbeitende saßen die endlose Pflichtschulung nur widerwillig ab.', 'جلس كثير من الموظفين في التدريب الإلزامي الطويل جدًا على مضض فقط.', 'زۆرێک لە کارمەندان بە ناڕەزاییەوە دانیشتنەکەی ڕاهێنانی زۆر درێژی ئیجباریان تێپەڕاند.', 'Many employees only reluctantly sat through the endless mandatory training.', 'بسیاری از کارکنان آموزش اجباری بی‌پایان را فقط با بی‌میلی تحمل کردند.', 'Gelek karmend perwerdehiya mecbûrî ya bêdawî bi dilnexwazî tenê rûniştin û qedandin.', 'Wielu pracowników z niechęcią przesiedziało niekończące się szkolenie obowiązkowe.', 'Mulți angajați au stat cu greu până la capăt la instruirea obligatorie interminabilă.', 'Многие сотрудники неохотно высидели бесконечное обязательное обучение.', 'Shumë punonjës e qëndruan me zor trajnimin e pafund të detyrueshëm.', 'Birçok çalışan bitmek bilmeyen zorunlu eğitimi isteksizce oturup tamamladı.')
    ]
  }),
  entry({
    word: 'abstechen', partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'abstechen', syllableBreak: 'ab-ste-chen',
    topics: ['advanced-analysis','quality-and-risk','production-and-maintenance'], usageLabels: ['written','technical','advanced','analysis'],
    grammarNotes: ['strong separable verb; in comparisons often means to stand out or contrast'], collocations: [{ text: 'von etwas abstechen', meaning: 'to contrast with something' }],
    wordFamilies: [{ lemma: 'der Stich', relationLabel: 'noun', note: null }],
    meanings: meaning('يبرز بالمقارنة؛ يتباين', 'جیا دەردەکەوێت؛ جیاوازیی ڕوون هەیە', 'to contrast; to stand out', 'در تضاد بودن؛ برجسته دیده شدن', 'cuda xuya bûn; ji hev derketin', 'odcinać się; wyróżniać się', 'a contrasta; a ieși în evidență', 'выделяться; контрастировать', 'të bëjë kontrast; të dallohet', 'ayırt edilmek; belirgin şekilde farklı durmak'),
    examples: [
      ex('Die rote Warnmeldung stach deutlich vom grauen Hintergrund der Anwendung ab.', 'كانت رسالة التحذير الحمراء بارزة بوضوح عن الخلفية الرمادية للتطبيق.', 'ئاگادارکردنەوە سوورەکە بە ڕوونی لە پاشبنەما خۆڵەمێشییەکەی بەرنامەکە جیا دەردەکەوت.', 'The red warning message clearly stood out against the application’s gray background.', 'پیام هشدار قرمز به‌وضوح از پس‌زمینه خاکستری برنامه متمایز بود.', 'Peyama hişyariyê ya sor ji paşxaneya gewr a sepanê bi eşkere cuda xuya bû.', 'Czerwony komunikat ostrzegawczy wyraźnie odcinał się od szarego tła aplikacji.', 'Mesajul roșu de avertizare contrasta clar cu fundalul gri al aplicației.', 'Красное предупреждение отчетливо выделялось на сером фоне приложения.', 'Mesazhi i kuq paralajmërues dallohej qartë nga sfondi gri i aplikacionit.', 'Kırmızı uyarı mesajı uygulamanın gri arka planından açıkça ayrılıyordu.'),
      ex('Der handwerklich gefertigte Prototyp stach qualitativ von den Serienmustern ab.', 'كان النموذج الأولي المصنوع يدويًا متميزًا من حيث الجودة عن عينات الإنتاج المتسلسل.', 'پڕۆتۆتایپی دەستکرد بە کوالیتی لە نموونەکانی بەرهەمهێنانی زنجیرەیی جیا دەردەکەوت.', 'The handcrafted prototype stood out in quality from the production samples.', 'نمونه اولیه دست‌ساز از نظر کیفیت از نمونه‌های تولیدی متمایز بود.', 'Prototîpa bi destan çêkirî bi kalîteyê ji nimûneyên hilberîna rêzeyî cuda xuya bû.', 'Ręcznie wykonany prototyp jakościowo wyróżniał się na tle próbek seryjnych.', 'Prototipul realizat manual se distingea calitativ de mostrele de serie.', 'Прототип ручной работы заметно отличался по качеству от серийных образцов.', 'Prototipi i punuar me dorë dallohej për cilësi nga mostrat e prodhimit në seri.', 'El yapımı prototip kalite açısından seri üretim örneklerinden ayrılıyordu.')
    ]
  }),
  entry({
    word: 'abstehen', partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'abstehen', syllableBreak: 'ab-ste-hen',
    topics: ['quality-and-risk','production-and-maintenance','housing-and-real-estate'], usageLabels: ['technical','written','advanced'],
    grammarNotes: ['strong separable verb'], collocations: [{ text: 'vom Rand abstehen', meaning: 'to protrude from the edge' }, { text: 'Kabel stehen ab', meaning: 'cables stick out' }],
    wordFamilies: [{ lemma: 'stehen', relationLabel: 'verb', note: null }],
    meanings: meaning('يبرز إلى الخارج؛ يكون ناتئًا', 'دەرەوە دەردەکەوێت؛ هەڵدەستێت', 'to protrude; to stick out', 'بیرون زدن؛ برجسته بودن', 'derve derketin; derketî man', 'odstawać; wystawać', 'a ieși în afară; a sta ridicat', 'торчать; выступать', 'të dalë jashtë; të qëndrojë i ngritur', 'dışarı çıkıntı yapmak; kalkık durmak'),
    examples: [
      ex('Nach der Montage standen mehrere Kabelenden gefährlich aus dem Gehäuse ab.', 'بعد التركيب، كانت عدة أطراف كابلات بارزة بشكل خطير من الهيكل.', 'دوای دامەزراندن، چەند کۆتایی کەیبڵ بە شێوەی مەترسیدار لە قاڵبەکە دەرکەوتبوون.', 'After installation, several cable ends were dangerously sticking out of the housing.', 'بعد از نصب، چند سر کابل به شکل خطرناکی از بدنه بیرون زده بود.', 'Piştî sazkirinê, çend seriyên kabloyê bi xeter ji qalikê derketibûn.', 'Po montażu kilka końcówek kabli niebezpiecznie wystawało z obudowy.', 'După montaj, mai multe capete de cablu ieșeau periculos din carcasă.', 'После монтажа несколько концов кабеля опасно торчали из корпуса.', 'Pas montimit, disa skaje kabllosh dilnin rrezikshëm nga kasa.', 'Montajdan sonra birkaç kablo ucu kasadan tehlikeli biçimde dışarı çıkıyordu.'),
      ex('Im Altbau standen die Fensterrahmen leicht von der Wand ab.', 'في المبنى القديم، كانت إطارات النوافذ بارزة قليلًا عن الجدار.', 'لە بینای کۆنەکەدا چوارچێوەی پەنجەرەکان کەمێک لە دیوارەکە دەرچووبوون.', 'In the old building, the window frames protruded slightly from the wall.', 'در ساختمان قدیمی، قاب‌های پنجره کمی از دیوار بیرون زده بودند.', 'Di avahiya kevn de, çarçoveyên pencereyan hinekî ji dîwarê derketibûn.', 'W starej kamienicy ramy okienne lekko odstawały od ściany.', 'În clădirea veche, ramele ferestrelor ieșeau ușor din perete.', 'В старом здании оконные рамы слегка выступали из стены.', 'Në ndërtesën e vjetër, kornizat e dritareve dilnin pak nga muri.', 'Eski binada pencere çerçeveleri duvardan hafifçe dışarı taşıyordu.')
    ]
  }),
  entry({
    word: 'abstoßen', partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'abstoßen', syllableBreak: 'ab-sto-ßen',
    topics: ['finance-and-accounting','quality-and-risk','management-and-leadership'], usageLabels: ['business','written','advanced'],
    grammarNotes: ['strong separable verb'], collocations: [{ text: 'eine Sparte abstoßen', meaning: 'to sell off a division' }, { text: 'Kunden abstoßen', meaning: 'to put customers off' }],
    wordFamilies: [{ lemma: 'der Abstoß', relationLabel: 'noun', note: null }],
    meanings: meaning('يبيع أو يتخلص من؛ ينفر', 'دەفرۆشێت یان خۆی لێ دەبوێرێت؛ بێزار دەکات', 'to sell off; to repel; to put off', 'واگذار کردن؛ دفع کردن؛ دل‌زده کردن', 'firotin û ji dest dan; dûr xistin', 'zbyć; odpychać; zniechęcać', 'a vinde; a respinge; a îndepărta', 'сбывать; отталкивать; отпугивать', 'të shesësh; të largosh; të zmbrapsësh', 'elden çıkarmak; itmek; uzaklaştırmak'),
    examples: [
      ex('Der Konzern will die verlustreiche Sparte bis Jahresende abstoßen.', 'تريد المجموعة بيع القسم الخاسر قبل نهاية السنة.', 'کۆمپانیا گەورەکە دەیەوێت بەشی زیانبار تا کۆتایی ساڵ بفرۆشێت.', 'The corporation wants to sell off the loss-making division by the end of the year.', 'این گروه می‌خواهد بخش زیان‌ده را تا پایان سال واگذار کند.', 'Koncern dixwaze beşa zirardar heta dawiya salê bifiroşe.', 'Koncern chce do końca roku zbyć nierentowny dział.', 'Concernul vrea să vândă divizia neprofitabilă până la sfârșitul anului.', 'Концерн хочет продать убыточное подразделение до конца года.', 'Koncerni dëshiron ta shesë sektorin me humbje deri në fund të vitit.', 'Şirketler grubu zarar eden bölümü yıl sonuna kadar elden çıkarmak istiyor.'),
      ex('Die stark riechende Verpackung stieß mehrere Testkunden ab.', 'نفّرت العبوة ذات الرائحة القوية عدة عملاء في الاختبار.', 'پاکەتەکەی بۆنی توندی هەبوو چەند کڕیاری تاقیکاری بێزار کرد.', 'The strongly smelling packaging put off several test customers.', 'بسته‌بندی با بوی شدید چند مشتری آزمایشی را دل‌زده کرد.', 'Pakêta bi bîhna xurt çend xerîdarên testê dûr xist.', 'Mocno pachnące opakowanie zniechęciło kilku klientów testowych.', 'Ambalajul cu miros puternic a respins mai mulți clienți de test.', 'Упаковка с резким запахом отпугнула нескольких тестовых клиентов.', 'Ambalazhi me erë të fortë largoi disa klientë testues.', 'Keskin kokulu ambalaj birkaç test müşterisini üründen soğuttu.')
    ]
  }),
  entry({
    word: 'abstrahieren', partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'abstrahieren', syllableBreak: 'abs-tra-hie-ren',
    topics: ['advanced-analysis','technology-and-it','data-and-reporting'], usageLabels: ['academic','technical','analysis','advanced'],
    grammarNotes: ['regular verb; often used with von plus dative'], collocations: [{ text: 'von Einzelfällen abstrahieren', meaning: 'to abstract from individual cases' }, { text: 'Details abstrahieren', meaning: 'to abstract away details' }],
    wordFamilies: [{ lemma: 'die Abstraktion', relationLabel: 'noun', note: null }, { lemma: 'abstrakt', relationLabel: 'adjective', note: null }],
    meanings: meaning('يجرّد؛ يعمم', 'ئەبستراکت دەکات؛ گشتی دەکات', 'to abstract; to generalize', 'انتزاع کردن؛ کلی‌سازی کردن', 'abstrakte kirin; giştî kirin', 'abstrahować; uogólniać', 'a abstractiza; a generaliza', 'абстрагировать; обобщать', 'të abstragosh; të përgjithësosh', 'soyutlamak; genelleştirmek'),
    examples: [
      ex('Für das Architekturdiagramm abstrahierten wir die internen Implementierungsdetails bewusst.', 'بالنسبة إلى مخطط البنية، جرّدنا تفاصيل التنفيذ الداخلية عن قصد.', 'بۆ دیاگرامی ئەندازیاری، بە ئەنقەست وردەکارییە ناوخۆییەکانی جێبەجێکردنمان ئەبستراکت کرد.', 'For the architecture diagram, we deliberately abstracted away the internal implementation details.', 'برای نمودار معماری، جزئیات داخلی پیاده‌سازی را عمداً انتزاعی کردیم.', 'Ji bo diyagrama avahîsaziyê, me bi zanebûn hûrguliyên bicihanîna hundirîn abstrakte kirin.', 'Na potrzeby diagramu architektury celowo pominęliśmy szczegóły wewnętrznej implementacji.', 'Pentru diagrama de arhitectură, am abstractizat în mod deliberat detaliile interne de implementare.', 'Для архитектурной диаграммы мы сознательно абстрагировались от внутренних деталей реализации.', 'Për diagramin e arkitekturës, ne abstraguam qëllimisht detajet e brendshme të implementimit.', 'Mimari diyagram için iç uygulama ayrıntılarını bilinçli olarak soyutladık.'),
      ex('Die Studie abstrahiert von Einzelfällen und betrachtet nur wiederkehrende Muster.', 'تتجرد الدراسة من الحالات الفردية وتنظر فقط إلى الأنماط المتكررة.', 'توێژینەوەکە لە دۆخە تاکەکان ئەبستراکت دەکات و تەنها شێوە دووبارەبووەکان دەبینێت.', 'The study abstracts from individual cases and considers only recurring patterns.', 'این مطالعه از موارد منفرد فاصله می‌گیرد و فقط الگوهای تکرارشونده را بررسی می‌کند.', 'Lêkolîn ji bûyerên takekesî abstrakte dike û tenê şêwazên dubare dibîne.', 'Badanie abstrahuje od pojedynczych przypadków i analizuje tylko powtarzające się wzorce.', 'Studiul face abstracție de cazurile individuale și analizează doar tiparele recurente.', 'Исследование абстрагируется от отдельных случаев и рассматривает только повторяющиеся закономерности.', 'Studimi abstrahon nga rastet individuale dhe shqyrton vetëm modelet që përsëriten.', 'Çalışma tekil vakalardan soyutlanır ve yalnızca tekrarlayan örüntüleri inceler.')
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...(e.usageLabels||[]), ...(e.contextLabels||[])]))];
const pkg = {
  packageVersion: '1.0', packageId, packageName: 'German C1 Generated Batch 003', source: 'Hybrid',
  defaultMeaningLanguages: langs,
  labels: usedLabels.map(labelObj),
  entries,
  collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: []
};
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');

const cmd = 'dotnet';
const args = ['run','--project', path.join(root,'src/Apps/DarwinLingua.ImportTool/DarwinLingua.ImportTool.csproj'),'--','--target','shared','--yes', outPath];
let output = '';
try {
  output = cp.execFileSync(cmd, args, { cwd: root, encoding: 'utf8', stdio: ['ignore','pipe','pipe'] });
} catch (e) {
  output = (e.stdout || '') + (e.stderr || '');
  fs.writeFileSync(path.join(root,'content/generated/de-c1-import-failures.txt'), words.join(', ') + '\tbatch-003\timport command failed\n', { flag: 'a', encoding: 'utf8' });
  console.log(JSON.stringify({ sourcePath, words, outPath, importOutput: output, deleted: false, remainingCount: tokens.length, first10Remaining: tokens.slice(0,10) }, null, 2));
  process.exit(1);
}
const imported = /Entries imported:\s*6\b/.test(output);
const invalid0 = /Entries invalid:\s*0\b/.test(output);
const warnings0 = /Warnings:\s*0\b/.test(output);
let deleted = false;
let remaining = tokens;
if (imported && invalid0 && warnings0) {
  const remove = new Set(words);
  let counts = Object.fromEntries(words.map(w => [w, 1]));
  remaining = [];
  for (const t of tokens) {
    if (remove.has(t) && counts[t] > 0) counts[t]--;
    else remaining.push(t);
  }
  fs.writeFileSync(sourcePath, remaining.join(', '), 'utf8');
  deleted = true;
} else {
  fs.writeFileSync(path.join(root,'content/generated/de-c1-import-failures.txt'), words.join(', ') + '\tbatch-003\t' + output.replace(/\s+/g,' ').trim() + '\n', { flag: 'a', encoding: 'utf8' });
}
console.log(JSON.stringify({ sourcePath, words, outPath, importOutput: output, deleted, remainingCount: remaining.length, first10Remaining: remaining.slice(0,10) }, null, 2));

const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '072';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['der Seitenhieb','die Semiotik','sentenzenhaft','das Signum','das Simulakrum','das Sinnbild'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const topicKeys = new Set((taxonomy.topics || []).map(t => t.key));
const labelsByKey = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const source = fs.readFileSync(sourcePath, 'utf8');
const tokens = source.split(',').map(s => s.trim()).filter(Boolean);
const first = tokens.slice(0, 6);
if (JSON.stringify(first) !== JSON.stringify(expected)) throw new Error(`Unexpected first tokens: ${JSON.stringify(first)}`);

function meaning(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) { return [ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr].map((text, i) => ({ language: langs[i], text })); }
function trans(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) { return meaning(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr); }
function ex(baseText, translations) { return { baseText, translations }; }
function entry(e) {
  for (const t of e.topics) if (!topicKeys.has(t)) throw new Error(`Unknown topic ${t}`);
  for (const l of [...(e.usageLabels || []), ...(e.contextLabels || [])]) if (!labelsByKey.has(l)) throw new Error(`Unknown label ${l}`);
  return Object.assign({ language: 'de', cefrLevel: level, article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: null, contextLabels: [], grammarNotes: [], collocations: [], wordFamilies: [], relations: [] }, e);
}

const entries = [
  entry({
    word: 'der Seitenhieb', partOfSpeech: 'Noun', article: 'der', plural: 'Seitenhiebe', syllableBreak: 'Sei-ten-hieb',
    topics: ['business-communication','social-and-relationships','culture-and-media'], usageLabels: ['formal','written','sensitive','advanced'],
    collocations: [{ text: 'ein versteckter Seitenhieb', meaning: 'a veiled dig or side swipe' }],
    meanings: meaning('تعريض؛ لمزة جانبية','تێهەڵدانی لاوەکی؛ ڕەخنەی شاراوە','side swipe; veiled dig','طعنه غیرمستقیم؛ کنایه تند','tîrêjeka veşartî; rexneya kêlekî','przytyk; złośliwa aluzja','aluzie răutăcioasă; atac indirect','колкость; скрытый выпад','thumbim anësor; aluzion therës','iğneleme; dolaylı taşlama'),
    examples: [
      ex('Der Seitenhieb auf die alte Projektleitung war unnötig und lenkte von der eigentlichen Risikoanalyse ab.', trans('كان التعريض بإدارة المشروع السابقة غير ضروري وصرف الانتباه عن تحليل المخاطر الحقيقي.','تێهەڵدانی لاوەکی بە بەڕێوەبردنی پڕۆژەی کۆن پێویست نەبوو و سەرنجی لە شیکاری ڕاستەقینەی مەترسییەکان لادا.','The side swipe at the former project leadership was unnecessary and distracted from the actual risk analysis.','طعنه به مدیریت قبلی پروژه غیرضروری بود و توجه را از تحلیل واقعی ریسک منحرف کرد.','Tîrêja kêlekî li ser rêveberiya berê ya projeyê nepêwîst bû û bal ji analîza rastîn a xetereyan kişand.','Przytyk pod adresem dawnego kierownictwa projektu był niepotrzebny i odwrócił uwagę od właściwej analizy ryzyka.','Aluzia răutăcioasă la vechea conducere a proiectului a fost inutilă și a distras atenția de la analiza reală a riscurilor.','Колкость в адрес прежнего руководства проекта была лишней и отвлекла от собственно анализа рисков.','Thumbimi ndaj drejtimit të vjetër të projektit ishte i panevojshëm dhe largoi vëmendjen nga analiza reale e rrezikut.','Eski proje yönetimine yönelik iğneleme gereksizdi ve asıl risk analizinden dikkati uzaklaştırdı.')),
      ex('Im Dialog wirkt der kurze Seitenhieb komisch, weil er die angespannte Höflichkeit der Szene durchbricht.', trans('في الحوار تبدو اللمزة القصيرة مضحكة لأنها تكسر المجاملة المتوترة في المشهد.','لە دیالۆگەکەدا تێهەڵدانی کورت پێکەنیناوییە، چونکە ڕێزداریی گرژەکەی دیمەنەکە دەشکێنێت.','In the dialogue, the brief dig is funny because it breaks the scene’s tense politeness.','در گفتگو، طعنه کوتاه خنده‌دار است، چون ادب پرتنش صحنه را می‌شکند.','Di diyalogê de tîrêja kurt kenbar e, ji ber ku nezaketiya teng a dîmenê dişkîne.','W dialogu krótki przytyk działa komicznie, bo przełamuje napiętą uprzejmość sceny.','În dialog, scurta aluzie este comică deoarece rupe politețea tensionată a scenei.','В диалоге короткая колкость смешна, потому что прерывает напряженную вежливость сцены.','Në dialog, thumbimi i shkurtër është komik, sepse thyen mirësjelljen e tensionuar të skenës.','Diyalogdaki kısa iğneleme komiktir, çünkü sahnenin gergin nezaketini kırar.'))
    ]
  }),
  entry({
    word: 'die Semiotik', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Se-mi-o-tik',
    topics: ['education-and-training','culture-and-media','advanced-analysis'], usageLabels: ['formal','written','academic','analysis'],
    grammarNotes: ['feminine field name; normally used in singular'],
    collocations: [{ text: 'die Semiotik visueller Zeichen', meaning: 'the semiotics of visual signs' }],
    meanings: meaning('السيميائيات؛ علم العلامات','سیمیۆتیک؛ زانستی نیشانەکان','semiotics; study of signs','نشانه‌شناسی؛ سمیوتیک','semiyotîk; zanista nîşanan','semiotyka','semiotică','семиотика','semiotikë','göstergebilim; semiyotik'),
    examples: [
      ex('Für das neue Interface analysierte das UX-Team die Semiotik der Icons, damit Warnungen nicht wie bloße Hinweise wirken.', trans('حلل فريق تجربة المستخدم سيميائية الأيقونات في الواجهة الجديدة كي لا تبدو التحذيرات مجرد ملاحظات.','بۆ ڕووکارە نوێیەکە، تیمی UX سیمیۆتیکی ئایکۆنەکانی شیکردەوە بۆ ئەوەی ئاگادارکردنەوەکان وەک تێبینیی سادە دەرنەکەون.','For the new interface, the UX team analyzed the semiotics of the icons so warnings would not look like mere hints.','برای رابط کاربری جدید، تیم UX نشانه‌شناسی آیکون‌ها را تحلیل کرد تا هشدارها مثل راهنمایی ساده به نظر نرسند.','Ji bo navrûya nû, tîma UX semiyotîka îkonan analîz kir da ku hişyarî wek tenê amaje xuya nekin.','Na potrzeby nowego interfejsu zespół UX przeanalizował semiotykę ikon, aby ostrzeżenia nie wyglądały jak zwykłe wskazówki.','Pentru noua interfață, echipa UX a analizat semiotica iconițelor, astfel încât avertismentele să nu pară simple sugestii.','Для нового интерфейса UX-команда проанализировала семиотику иконок, чтобы предупреждения не выглядели как простые подсказки.','Për ndërfaqen e re, ekipi UX analizoi semiotikën e ikonave që paralajmërimet të mos dukeshin si këshilla të thjeshta.','Yeni arayüz için UX ekibi ikonların semiyotiğini analiz etti; böylece uyarılar sıradan ipuçları gibi görünmeyecekti.')),
      ex('Die Semiotik des Films zeigt sich in wiederkehrenden Farben, Gesten und räumlichen Grenzen.', trans('تظهر سيميائية الفيلم في الألوان والإيماءات والحدود المكانية المتكررة.','سیمیۆتیکی فیلمەکە لە ڕەنگ، ئاماژە و سنوورە شوێنییە دووبارەبووەکاندا دەردەکەوێت.','The film’s semiotics appears in recurring colors, gestures, and spatial boundaries.','نشانه‌شناسی فیلم در رنگ‌ها، ژست‌ها و مرزهای مکانی تکرارشونده آشکار می‌شود.','Semiyotîka fîlmê di reng, tevger û sînorên cihî yên dubare de xuya dibe.','Semiotyka filmu ujawnia się w powracających kolorach, gestach i granicach przestrzennych.','Semiotica filmului se vede în culori, gesturi și granițe spațiale recurente.','Семиотика фильма проявляется в повторяющихся цветах, жестах и пространственных границах.','Semiotika e filmit shfaqet në ngjyra, gjeste dhe kufij hapësinorë të përsëritur.','Filmin semiyotiği tekrarlanan renklerde, jestlerde ve mekansal sınırlarda ortaya çıkar.'))
    ]
  }),
  entry({
    word: 'sentenzenhaft', partOfSpeech: 'Adjective', syllableBreak: 'sen-ten-zen-haft',
    topics: ['culture-and-media','business-communication','advanced-analysis'], usageLabels: ['formal','written','academic','advanced'],
    collocations: [{ text: 'sentenzenhaft formuliert', meaning: 'formulated like an aphoristic maxim' }],
    meanings: meaning('على هيئة حكمة موجزة','وەک پەندی کورت؛ حکمتئامێز','aphoristic; maxim-like','حکمت‌آمیز و گزیده؛ شبیه جمله قصار','wek gotina pêndî; aforîstîk','sentencjonalny; aforystyczny','sentențios; aforistic','сентенциозный; афористичный','sentencëformë; aforistik','özdeyiş gibi; aforizmik'),
    examples: [
      ex('Die Leitlinien waren sentenzenhaft formuliert, aber im Alltag fehlten konkrete Kriterien für schwierige Entscheidungen.', trans('صيغت المبادئ التوجيهية كحكم موجزة، لكن في الواقع اليومي كانت تنقص معايير ملموسة للقرارات الصعبة.','ڕێنماییەکان وەک پەندی کورت داڕێژرابوون، بەڵام لە ڕۆژانەدا پێوەری دیاریکراو بۆ بڕیارە قورسەکان کەم بوو.','The guidelines were formulated aphoristically, but in everyday practice concrete criteria for difficult decisions were missing.','دستورالعمل‌ها حکمت‌آمیز و گزیده نوشته شده بودند، اما در عمل معیارهای مشخص برای تصمیم‌های دشوار کم بود.','Rêbername wek gotinên pêndî hatibûn nivîsandin, lê di jiyana rojane de pîvanên zehmet ji bo biryarên dijwar kêm bûn.','Wytyczne sformułowano sentencjonalnie, lecz w codziennej praktyce brakowało konkretnych kryteriów trudnych decyzji.','Liniile directoare erau formulate sentențios, dar în practica zilnică lipseau criterii concrete pentru decizii dificile.','Руководящие принципы были сформулированы афористично, но в повседневной практике не хватало конкретных критериев для сложных решений.','Udhëzimet ishin formuluar në mënyrë aforistike, por në praktikën e përditshme mungonin kritere konkrete për vendime të vështira.','İlkeler özdeyiş gibi formüle edilmişti, ancak günlük pratikte zor kararlar için somut ölçütler eksikti.')),
      ex('Der sentenzenhafte Ton der Erzählung verleiht einfachen Beobachtungen eine fast philosophische Schwere.', trans('يضفي الطابع الحكمي للسرد على الملاحظات البسيطة ثقلاً شبه فلسفي.','تۆنی پەندئامێزی گێڕانەوەکە بە چاودێرییە سادەکان قورساییەکی نزیک بە فەلسەفی دەدات.','The aphoristic tone of the narrative gives simple observations an almost philosophical weight.','لحن حکمت‌آمیز روایت به مشاهده‌های ساده وزنی تقریباً فلسفی می‌دهد.','Tona aforîstîk a vegotinê giraniyeke hema felsefî dide çavdêriyên sade.','Sentencjonalny ton narracji nadaje prostym obserwacjom niemal filozoficzny ciężar.','Tonul sentențios al narațiunii dă observațiilor simple o greutate aproape filosofică.','Сентенциозный тон повествования придает простым наблюдениям почти философскую тяжесть.','Toni aforistik i rrëfimit u jep vëzhgimeve të thjeshta një peshë gati filozofike.','Anlatının özdeyişe benzeyen tonu, basit gözlemlere neredeyse felsefi bir ağırlık verir.'))
    ]
  }),
  entry({
    word: 'das Signum', partOfSpeech: 'Noun', article: 'das', plural: 'Signa', syllableBreak: 'Sig-num',
    topics: ['advanced-analysis','culture-and-media','business-communication'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'das Signum einer Epoche', meaning: 'the defining mark of an era' }],
    meanings: meaning('علامة مميزة؛ سمة دالة','نیشانەی جیاکەرەوە؛ تایبەتمەندیی دیار','sign; defining mark','نشان متمایز؛ علامت شاخص','nîşan; taybetmendiya diyar','znamię; znak rozpoznawczy','semn distinctiv; marcă','знак; отличительная черта','shenjë dalluese','ayırt edici işaret; alamet'),
    examples: [
      ex('Die permanente Beschleunigung gilt vielen als Signum der digitalen Arbeitswelt.', trans('يرى كثيرون أن التسارع الدائم علامة مميزة لعالم العمل الرقمي.','زۆر کەس خێرابوونی هەمیشەیی وەک نیشانەی جیاکەرەوەی جیهانی کاری دیجیتاڵی دەبینن.','Many regard permanent acceleration as the defining mark of the digital workplace.','بسیاری شتاب دائمی را نشان شاخص دنیای کار دیجیتال می‌دانند.','Gelek kes lezkirina herdemî wek nîşana cîhana kar a dîjîtal dibînin.','Wielu uważa trwałe przyspieszenie za znamię cyfrowego świata pracy.','Mulți consideră accelerarea permanentă drept semnul distinctiv al lumii muncii digitale.','Многие считают постоянное ускорение отличительным признаком цифрового мира труда.','Shumë e shohin përshpejtimin e përhershëm si shenjën dalluese të botës digjitale të punës.','Birçok kişi sürekli hızlanmayı dijital çalışma dünyasının ayırt edici işareti olarak görür.')),
      ex('Das zerbrochene Fenster wird im Roman zum Signum einer Ordnung, die nur noch äußerlich stabil wirkt.', trans('تصبح النافذة المكسورة في الرواية علامة على نظام يبدو مستقراً من الخارج فقط.','پەنجەرەی شکاو لە ڕۆمانەکەدا دەبێتە نیشانەی سیستەمێک کە تەنها لە دەرەوە جێگیر دەردەکەوێت.','In the novel, the broken window becomes the sign of an order that only appears stable from the outside.','پنجره شکسته در رمان به نشانی از نظمی تبدیل می‌شود که فقط از بیرون پایدار به نظر می‌رسد.','Di romanê de paceya şikestî dibe nîşana rêzikek ku tenê ji derve aram xuya dike.','Rozbite okno staje się w powieści znakiem porządku, który stabilny wydaje się już tylko z zewnątrz.','Fereastra spartă devine în roman semnul unei ordini care pare stabilă doar la exterior.','Разбитое окно в романе становится знаком порядка, который лишь внешне кажется стабильным.','Dritarja e thyer në roman bëhet shenjë e një rendi që duket i qëndrueshëm vetëm nga jashtë.','Kırık pencere romanda yalnızca dışarıdan istikrarlı görünen bir düzenin işaretine dönüşür.'))
    ]
  }),
  entry({
    word: 'das Simulakrum', partOfSpeech: 'Noun', article: 'das', plural: 'Simulakren', syllableBreak: 'Si-mu-la-krum',
    topics: ['culture-and-media','technology-and-it','advanced-analysis'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'ein digitales Simulakrum', meaning: 'a digital simulacrum' }],
    meanings: meaning('محاكاة بلا أصل؛ صورة بديلة','وێنەی شبیه‌سازیبێ بنەما؛ سیمولاکروم','simulacrum; copy without a clear original','بدل‌نما؛ شبیه‌واره بی‌اصل روشن','simulakrum; kopiyek bê reseniyeke zelal','symulakrum','simulacru','симулякр','simulakër','simülakr; aslı belirsiz kopya'),
    examples: [
      ex('Der digitale Zwilling wurde im Vertrieb wie ein Simulakrum behandelt: überzeugend anzusehen, aber nur lose mit der realen Maschine verbunden.', trans('تُعامل النسخة الرقمية في المبيعات كأنها شبيه بلا أصل واضح: مقنعة بصرياً، لكنها مرتبطة بالآلة الحقيقية ارتباطاً فضفاضاً فقط.','دووانەی دیجیتاڵی لە فرۆشتندا وەک سیمولاکروم مامەڵەی لەگەڵ دەکرا: لە بینین قایلکەر، بەڵام تەنها بە شێوەیەکی لاواز بە ئامێری ڕاستەقینەوە گرێدراو.','In sales, the digital twin was treated like a simulacrum: convincing to look at, but only loosely connected to the real machine.','در فروش، دوقلوی دیجیتال مثل بدل‌نمایی برخورد می‌شد: از نظر بصری قانع‌کننده، اما فقط به‌طور سست به ماشین واقعی متصل.','Di firotanê de hevduya dîjîtal wek simulakrum hate dîtin: ji bo dîtinê qanihker, lê tenê bi awayekî sist bi makîneya rastîn ve girêdayî.','W sprzedaży cyfrowego bliźniaka traktowano jak symulakrum: przekonujący wizualnie, lecz luźno powiązany z realną maszyną.','În vânzări, geamănul digital era tratat ca un simulacru: convingător vizual, dar doar slab legat de mașina reală.','В продажах цифровой двойник воспринимали как симулякр: визуально убедительный, но лишь слабо связанный с реальной машиной.','Në shitje, binjaku digjital trajtohej si simulakër: bindës për t’u parë, por i lidhur lirshëm me makinën reale.','Satışta dijital ikiz bir simülakr gibi ele alınıyordu: bakınca ikna edici, ama gerçek makineyle yalnızca gevşek biçimde bağlantılı.')),
      ex('Die Installation fragt, ob die perfekte Rekonstruktion eines Ortes noch Erinnerung ist oder bereits ein Simulakrum.', trans('يسأل العمل التركيبي عما إذا كانت إعادة بناء مكان بإتقان ما تزال ذاكرة أم أصبحت بالفعل شبيهاً بلا أصل.','دامەزراندنە هونەرییەکە دەپرسێت ئایا دووبارە دروستکردنەوەی تەواوی شوێنێک هێشتا بیرەوەرییە یان ئیتر سیمولاکرومە.','The installation asks whether the perfect reconstruction of a place is still memory or already a simulacrum.','این چیدمان می‌پرسد آیا بازسازی کامل یک مکان هنوز خاطره است یا دیگر بدل‌نما شده است.','Sazkirin dipirse ka avakirina bêkêmasî ya cihê hîn bîranîn e an êdî simulakrum e.','Instalacja pyta, czy perfekcyjna rekonstrukcja miejsca jest jeszcze pamięcią, czy już symulakrum.','Instalația întreabă dacă reconstrucția perfectă a unui loc mai este memorie sau deja simulacru.','Инсталляция спрашивает, является ли идеальная реконструкция места еще памятью или уже симулякром.','Instalacioni pyet nëse rindërtimi i përsosur i një vendi është ende kujtesë apo tashmë simulakër.','Enstalasyon, bir yerin kusursuz yeniden inşasının hâlâ hafıza mı yoksa artık simülakr mı olduğunu sorar.'))
    ]
  }),
  entry({
    word: 'das Sinnbild', partOfSpeech: 'Noun', article: 'das', plural: 'Sinnbilder', syllableBreak: 'Sinn-bild',
    topics: ['culture-and-media','business-communication','advanced-analysis'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'als Sinnbild für etwas stehen', meaning: 'to stand as a symbol of something' }],
    meanings: meaning('رمز؛ صورة دالة','هێما؛ وێنەی مانادار','symbol; emblematic image','نماد؛ تصویر معنادار','sembol; wêneya watedar','symbol; alegoria','simbol; imagine emblematică','символ; образ','simbol; figurë kuptimore','simge; anlamlı imge'),
    examples: [
      ex('Die leere Warteschlange wurde zum Sinnbild einer Verwaltung, die digitale Prozesse verspricht, aber analoge Engpässe reproduziert.', trans('أصبح الطابور الفارغ رمزاً لإدارة تعد بعمليات رقمية لكنها تعيد إنتاج اختناقات تناظرية.','ڕیزی چاوەڕوانی بەتاڵ بوو بە هێمای کارگێڕییەک کە بەڵێنی پڕۆسەی دیجیتاڵی دەدات، بەڵام قەیرانی ئەناڵۆگ دووبارە دەکاتەوە.','The empty queue became a symbol of an administration that promises digital processes but reproduces analog bottlenecks.','صف انتظار خالی به نماد اداری تبدیل شد که فرایند دیجیتال وعده می‌دهد اما گلوگاه‌های آنالوگ را بازتولید می‌کند.','Rêza bendê ya vala bû sembola rêveberiyekê ku pêvajoyên dîjîtal soz dide lê tengaviyên analog dubare dike.','Pusta kolejka stała się symbolem administracji, która obiecuje procesy cyfrowe, lecz reprodukuje analogowe wąskie gardła.','Coada goală a devenit simbolul unei administrații care promite procese digitale, dar reproduce blocaje analogice.','Пустая очередь стала символом администрации, которая обещает цифровые процессы, но воспроизводит аналоговые узкие места.','Radha bosh u bë simbol i një administrate që premton procese digjitale, por riprodhon ngërçe analoge.','Boş kuyruk, dijital süreçler vaat eden ama analog darboğazları yeniden üreten bir idarenin simgesi oldu.')),
      ex('Der verblühte Garten ist im Gedicht kein realistisches Detail, sondern ein Sinnbild verlorener Zeit.', trans('الحديقة الذابلة في القصيدة ليست تفصيلاً واقعياً، بل رمزاً للزمن الضائع.','باخچەی پژاو لە شیعرەکەدا وردەکارییەکی ڕاستەقینە نییە، بەڵکو هێمای کاتی لەدەستچووە.','The faded garden in the poem is not a realistic detail, but a symbol of lost time.','باغ پژمرده در شعر جزئیات واقع‌گرایانه نیست، بلکه نماد زمان ازدست‌رفته است.','Baxçeya şilbûyî di helbestê de ne hûrguliyeke realistîk e, lê sembola dema winda ye.','Przekwitły ogród w wierszu nie jest realistycznym szczegółem, lecz symbolem utraconego czasu.','Grădina ofilită din poem nu este un detaliu realist, ci un simbol al timpului pierdut.','Отцветший сад в стихотворении не реалистическая деталь, а символ утраченного времени.','Kopshti i vyshkur në poezi nuk është detaj realist, por simbol i kohës së humbur.','Şiirdeki solmuş bahçe gerçekçi bir ayrıntı değil, yitirilmiş zamanın simgesidir.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 072', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');

const cmd = `dotnet run --project "${path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj')}" -- --target shared --yes "${outPath}"`;
const result = cp.spawnSync(cmd, { shell: true, encoding: 'utf8', cwd: root });
const output = `${result.stdout || ''}${result.stderr || ''}`;
process.stdout.write(output);
const ok = result.status === 0 && output.includes('Entries imported: 6') && output.includes('Entries invalid: 0') && output.includes('Warnings: 0');
if (!ok) {
  const failedPath = path.join(root, 'content', 'generated', `${levelLower}-failed-words.txt`);
  fs.appendFileSync(failedPath, `${new Date().toISOString()}\tbatch-${batch}\t${expected.join(', ')}\n`, 'utf8');
  throw new Error('Import did not meet strict success criteria; source not modified.');
}
const remaining = tokens.slice(expected.length);
fs.writeFileSync(sourcePath, remaining.join(', ') + (remaining.length ? '\n' : ''), 'utf8');
console.log(`SOURCE_UPDATED: yes`);
console.log(`SOURCE_FILE: ${sourcePath}`);
console.log(`JSON_FILE: ${outPath}`);
console.log(`PROCESSED: ${expected.join(' | ')}`);
console.log(`REMAINING_COUNT: ${remaining.length}`);
console.log(`FIRST_10_REMAINING: ${remaining.slice(0, 10).join(' | ')}`);

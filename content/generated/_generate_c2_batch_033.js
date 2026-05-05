const fs = require('fs');
const cp = require('child_process');
const path = require('path');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '033';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const expected = ['der Erzählfluss', 'die Erzählstimme', 'esoterisch', 'die Exegese', 'das Exerzitium', 'das Fanal'];
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const labelMap = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const topicSet = new Set((taxonomy.topics || []).map(t => t.key));
const splitTokens = text => text.split(',').map(t => t.trim()).filter(Boolean);
const tokens = splitTokens(fs.readFileSync(sourcePath, 'utf8'));
const first = tokens.slice(0, expected.length);
if (JSON.stringify(first) !== JSON.stringify(expected)) throw new Error(`Source token mismatch. Expected ${JSON.stringify(expected)} but found ${JSON.stringify(first)}`);

function m(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) { return [
  {language:'ar', text:ar}, {language:'ckb', text:ckb}, {language:'en', text:en}, {language:'fa', text:fa}, {language:'kmr', text:kmr},
  {language:'pl', text:pl}, {language:'ro', text:ro}, {language:'ru', text:ru}, {language:'sq', text:sq}, {language:'tr', text:tr}
]; }
function ex(baseText, translations) { return { baseText, translations }; }

const entries = [
  {
    word:'der Erzählfluss', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'der', plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'Er-zähl-fluss',
    topics:['culture-and-media','advanced-analysis','education-and-training'], usageLabels:['formal','written','academic','analysis'], contextLabels:[],
    grammarNotes:['masculine compound noun; plural is uncommon'], collocations:[{text:'den Erzählfluss unterbrechen', meaning:'to interrupt the narrative flow'}],
    wordFamilies:[{lemma:'erzählen', relationLabel:'verb', note:null},{lemma:'die Erzählung', relationLabel:'noun', note:null}], relations:[],
    meanings:m('تدفّق السرد؛ انسياب الحكي','ڕەوانی گێڕانەوە؛ ڕەوتی چیرۆکگێڕان','narrative flow','جریان روایت؛ روانی داستان‌گویی','herikîna vegotinê','tok narracji; płynność opowieści','flux narativ','повествовательный поток','rrjedhë narrative','anlatı akışı'),
    examples:[
      ex('Die vielen Fußnoten liefern wertvolle Hinweise, unterbrechen aber immer wieder den Erzählfluss.', m('تقدم الحواشي الكثيرة Hinweise قيّمة، لكنها تقطع تدفق السرد مراراً.', 'پەراوێزە زۆرەکان ئاماژەی بەهادار دەدەن، بەڵام بەردەوام ڕەوانی گێڕانەوەکە دەبڕن.', 'The many footnotes provide valuable clues, but repeatedly interrupt the narrative flow.', 'پاورقی‌های فراوان نکات ارزشمندی می‌دهند، اما مدام جریان روایت را قطع می‌کنند.', 'Gelek têbînîyên jêrîn nîşaneyên bi nirx didin, lê her car herikîna vegotinê dibirin.', 'Liczne przypisy dostarczają cennych wskazówek, ale raz po raz przerywają tok narracji.', 'Numeroasele note de subsol oferă indicii valoroase, dar întrerup mereu fluxul narativ.', 'Многочисленные сноски дают ценные указания, но постоянно прерывают повествовательный поток.', 'Shënimet e shumta japin të dhëna të vlefshme, por e ndërpresin vazhdimisht rrjedhën narrative.', 'Çok sayıdaki dipnot değerli ipuçları sunuyor, ancak anlatı akışını sürekli kesiyor.')),
      ex('Im Workshop erklärte die Lektorin, wie eine klare Szenenfolge den Erzählfluss deutlich verbessert.', m('شرحت المحررة في ورشة العمل كيف يحسّن ترتيب واضح للمشاهد تدفق السرد بشكل ملحوظ.', 'لە وۆرکشۆپەکەدا دەستکاریکەرەکە ڕوونی کردەوە کە چۆن ڕیزبەندییەکی ڕوونی دیمەنەکان ڕەوانی گێڕانەوە باشتر دەکات.', 'In the workshop, the editor explained how a clear sequence of scenes significantly improves narrative flow.', 'در کارگاه، ویراستار توضیح داد که ترتیب روشن صحنه‌ها چگونه جریان روایت را به‌طور محسوسی بهتر می‌کند.', 'Di atolyeyê de edîtorê rave kir ku rêza zelal a dîmenan çawa herikîna vegotinê baştir dike.', 'Na warsztatach redaktorka wyjaśniła, jak jasna sekwencja scen wyraźnie poprawia tok narracji.', 'La atelier, redactoarea a explicat cum o succesiune clară a scenelor îmbunătățește semnificativ fluxul narativ.', 'На семинаре редактор объяснила, как четкая последовательность сцен заметно улучшает повествовательный поток.', 'Në punëtori, redaktorja shpjegoi si një rend i qartë skenash e përmirëson ndjeshëm rrjedhën narrative.', 'Atölyede editör, net bir sahne sıralamasının anlatı akışını nasıl belirgin biçimde iyileştirdiğini açıkladı.'))
    ]
  },
  {
    word:'die Erzählstimme', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'die', plural:'Erzählstimmen', infinitive:null, pronunciationIpa:null, syllableBreak:'Er-zähl-stim-me',
    topics:['culture-and-media','advanced-analysis','education-and-training'], usageLabels:['formal','written','academic','analysis'], contextLabels:[],
    grammarNotes:['feminine compound noun'], collocations:[{text:'eine unzuverlässige Erzählstimme', meaning:'an unreliable narrative voice'}],
    wordFamilies:[{lemma:'erzählen', relationLabel:'verb', note:null},{lemma:'die Stimme', relationLabel:'noun', note:null}], relations:[],
    meanings:m('الصوت السردي؛ الراوي','دەنگی گێڕانەوە؛ دەنگی چیرۆکگێڕ','narrative voice','صدای روایی؛ لحن راوی','dengê vegotinê','głos narracyjny','voce narativă','повествовательный голос','zë narrativ','anlatıcı sesi'),
    examples:[
      ex('Die Erzählstimme wirkt zunächst sachlich, lässt aber allmählich ihre Vorurteile erkennen.', m('يبدو الصوت السردي في البداية موضوعياً، لكنه يكشف تدريجياً عن تحيزاته.', 'دەنگی گێڕانەوە لە سەرەتا وەک بێلایەن دەردەکەوێت، بەڵام وردەوردە پێشداوەرییەکانی نیشان دەدات.', 'The narrative voice initially seems objective, but gradually reveals its prejudices.', 'صدای روایی ابتدا عینی به نظر می‌رسد، اما کم‌کم پیش‌داوری‌های خود را آشکار می‌کند.', 'Dengê vegotinê di destpêkê de bêalî xuya dike, lê hêdî hêdî pêşdaraziyên xwe eşkere dike.', 'Głos narracyjny początkowo wydaje się rzeczowy, ale stopniowo ujawnia swoje uprzedzenia.', 'Vocea narativă pare la început obiectivă, dar își dezvăluie treptat prejudecățile.', 'Повествовательный голос сначала кажется объективным, но постепенно раскрывает свои предубеждения.', 'Zëri narrativ fillimisht duket objektiv, por gradualisht zbulon paragjykimet e veta.', 'Anlatıcı sesi başlangıçta nesnel görünür, ancak yavaş yavaş önyargılarını açığa çıkarır.')),
      ex('Für die neue Markenstory suchte das Kommunikationsteam eine Erzählstimme, die kompetent, aber nicht belehrend klingt.', m('بحث فريق التواصل لقصة العلامة التجارية الجديدة عن صوت سردي يبدو كفؤاً من دون أن يكون واعظاً.', 'بۆ چیرۆکی نوێی براندەکە، تیمی پەیوەندیکردن دەنگێکی گێڕانەوەی دەویست کە لێهاتوو بێت بەڵام فێرکاری نەبێت.', 'For the new brand story, the communications team looked for a narrative voice that sounded competent but not patronizing.', 'برای داستان جدید برند، تیم ارتباطات به دنبال صدای روایی‌ای بود که حرفه‌ای اما نصیحت‌گرانه نباشد.', 'Ji bo çîroka nû ya markayê, tîma ragihandinê li dengê vegotinê digeriya ku jêhatî lê ne fêrkar xuya bike.', 'Do nowej historii marki zespół komunikacji szukał głosu narracyjnego, który brzmi kompetentnie, ale nie pouczająco.', 'Pentru noua poveste de brand, echipa de comunicare căuta o voce narativă competentă, dar nu moralizatoare.', 'Для новой истории бренда команда коммуникаций искала повествовательный голос, который звучал бы компетентно, но не назидательно.', 'Për historinë e re të markës, ekipi i komunikimit kërkonte një zë narrativ që tingëllonte kompetent, por jo moralizues.', 'Yeni marka hikayesi için iletişim ekibi yetkin ama öğretici olmayan bir anlatıcı sesi arıyordu.'))
    ]
  },
  {
    word:'esoterisch', language:'de', cefrLevel:level, partOfSpeech:'Adjective', article:null, plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'e-so-te-risch',
    topics:['culture-and-media','advanced-analysis','social-and-relationships'], usageLabels:['formal','written','advanced'], contextLabels:[],
    grammarNotes:['adjective; can mean hidden, specialist, or associated with esotericism depending on context'], collocations:[{text:'esoterische Lehren', meaning:'esoteric teachings'}],
    wordFamilies:[{lemma:'die Esoterik', relationLabel:'noun', note:null}], relations:[],
    meanings:m('باطني؛ غامض ومخصّص لدائرة ضيقة','نهێنی؛ تایبەت بە بازنەیەکی داخراو','esoteric; obscure to outsiders','باطنی؛ رازآمیز؛ فقط قابل فهم برای گروهی خاص','veşartî; tenê ji bo komeke taybet tê famkirin','ezoteryczny; hermetyczny','ezoteric; greu accesibil celor din afară','эзотерический; понятный узкому кругу','ezoterik; i kuptueshëm për një rreth të ngushtë','ezoterik; dışarıdakilere kapalı'),
    examples:[
      ex('Der Vortrag blieb vielen Zuhörern zu esoterisch, weil zentrale Begriffe nicht erklärt wurden.', m('بدا المحاضرة لكثير من المستمعين غامضة جداً لأن المفاهيم الأساسية لم تُشرح.', 'وتارەکە بۆ زۆرێک لە گوێگران زۆر نهێنی و داخراو بوو، چونکە چەمکە سەرەکییەکان ڕوون نەکرانەوە.', 'The lecture remained too esoteric for many listeners because key terms were not explained.', 'سخنرانی برای بسیاری از شنوندگان بیش از حد مبهم ماند، چون مفاهیم اصلی توضیح داده نشدند.', 'Gotar ji bo gelek guhdaran pir veşartî ma, ji ber ku têgehên bingehîn nehatin ravekirin.', 'Wykład był dla wielu słuchaczy zbyt hermetyczny, ponieważ nie wyjaśniono kluczowych pojęć.', 'Prelegerea a rămas prea ezoterică pentru mulți ascultători, deoarece termenii centrali nu au fost explicați.', 'Лекция показалась многим слушателям слишком эзотерической, потому что ключевые понятия не были объяснены.', 'Ligjërata mbeti tepër ezoterike për shumë dëgjues, sepse termat kryesorë nuk u shpjeguan.', 'Konferans, temel kavramlar açıklanmadığı için birçok dinleyiciye fazla ezoterik geldi.')),
      ex('Im Kundengespräch sollte die Beraterin esoterische Fachsprache vermeiden und die Risiken klar benennen.', m('في الحديث مع العميل ينبغي للمستشارة أن تتجنب لغة تخصصية غامضة وأن تسمي المخاطر بوضوح.', 'لە گفتوگۆی کڕیاردا دەبوو ڕاوێژکارەکە زمانی پسپۆڕی داخراو بەکار نەهێنێت و مەترسییەکان بە ڕوونی ناو ببات.', 'In the customer meeting, the consultant should avoid esoteric jargon and name the risks clearly.', 'در گفت‌وگو با مشتری، مشاور باید از زبان تخصصی مبهم پرهیز کند و ریسک‌ها را روشن نام ببرد.', 'Di axaftina bi xerîdar re, şêwirmend divê ji zimanê pisporî yê veşartî dûr bikeve û metirsiyan zelal bibêje.', 'W rozmowie z klientem konsultantka powinna unikać hermetycznego żargonu i jasno nazwać ryzyka.', 'În discuția cu clientul, consultanta ar trebui să evite jargonul ezoteric și să numească riscurile clar.', 'В разговоре с клиентом консультанту следует избегать непонятного жаргона и четко называть риски.', 'Në bisedën me klientin, këshilltarja duhet të shmangë zhargonin ezoterik dhe t’i emërtojë qartë rreziqet.', 'Müşteri görüşmesinde danışman ezoterik uzman jargonundan kaçınmalı ve riskleri açıkça belirtmelidir.'))
    ]
  },
  {
    word:'die Exegese', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'die', plural:'Exegesen', infinitive:null, pronunciationIpa:null, syllableBreak:'Ex-e-ge-se',
    topics:['culture-and-media','education-and-training','advanced-analysis'], usageLabels:['formal','written','academic','analysis'], contextLabels:[],
    grammarNotes:['feminine noun; common in theology, literary studies, and textual interpretation'], collocations:[{text:'historisch-kritische Exegese', meaning:'historical-critical exegesis'}],
    wordFamilies:[{lemma:'exegetisch', relationLabel:'adjective', note:null}], relations:[],
    meanings:m('تفسير نصي دقيق؛ شرح نقدي','لێکدانەوەی وردی دەق؛ شیکردنەوەی ڕەخنەیی','exegesis; critical textual interpretation','تفسیر دقیق متن؛ شرح انتقادی','şîroveya nivîsê ya rexneyî','egzegeza; krytyczna interpretacja tekstu','exegeză; interpretare textuală critică','экзегеза; критическое толкование текста','ekzegjezë; interpretim kritik i tekstit','tefsir; eleştirel metin yorumu'),
    examples:[
      ex('Die Exegese des Vertrags zeigte, dass die scheinbar klare Klausel mehrere Lesarten zulässt.', m('أظهر التفسير الدقيق للعقد أن البند الذي بدا واضحاً يسمح بعدة قراءات.', 'لێکدانەوەی وردی گرێبەستەکە نیشانی دا کە ماددەیەکەی بە ظاهر ڕوون چەند خوێندنەوەیەک ڕێگە پێدەدات.', 'The exegesis of the contract showed that the seemingly clear clause allows several readings.', 'تفسیر دقیق قرارداد نشان داد که بند ظاهراً روشن، چند برداشت مختلف را ممکن می‌کند.', 'Şîroveya peymanê nîşan da ku bendê ku xuya zelal bû çend xwendinên cuda destûr dide.', 'Egzegeza umowy pokazała, że pozornie jasna klauzula dopuszcza kilka interpretacji.', 'Exegeza contractului a arătat că clauza aparent clară permite mai multe lecturi.', 'Экзегеза договора показала, что казавшийся ясным пункт допускает несколько толкований.', 'Ekzegjeza e kontratës tregoi se klauzola në dukje e qartë lejon disa lexime.', 'Sözleşmenin ayrıntılı yorumu, görünüşte açık olan maddenin birkaç farklı okumaya izin verdiğini gösterdi.')),
      ex('In der theologischen Exegese wurde der Abschnitt nicht wörtlich, sondern im historischen Kontext gelesen.', m('في التفسير اللاهوتي لم يُقرأ المقطع حرفياً، بل ضمن سياقه التاريخي.', 'لە لێکدانەوەی ئایینناسیدا بەشەکە وەک وشە بە وشە نەخوێندرایەوە، بەڵکو لە چوارچێوەی مێژووییەکەیدا.', 'In theological exegesis, the passage was read not literally, but in its historical context.', 'در تفسیر الهیاتی، بخش مورد نظر نه به‌صورت لفظی بلکه در زمینه تاریخی آن خوانده شد.', 'Di şîroveya teolojîk de beş ne bi peyv bi peyv, lê di çarçoveya dîrokî de hate xwendin.', 'W egzegezie teologicznej fragment odczytano nie dosłownie, lecz w kontekście historycznym.', 'În exegeza teologică, pasajul a fost citit nu literal, ci în contextul său istoric.', 'В богословской экзегезе отрывок читался не буквально, а в историческом контексте.', 'Në ekzegjezën teologjike, fragmenti nuk u lexua fjalë për fjalë, por në kontekstin historik.', 'Teolojik tefsirde bölüm kelimesi kelimesine değil, tarihsel bağlamı içinde okundu.'))
    ]
  },
  {
    word:'das Exerzitium', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'das', plural:'Exerzitien', infinitive:null, pronunciationIpa:null, syllableBreak:'Ex-er-zi-ti-um',
    topics:['education-and-training','culture-and-media','social-and-relationships'], usageLabels:['formal','written','academic'], contextLabels:[],
    grammarNotes:['neuter noun; plural Exerzitien, often used for spiritual exercises or disciplined practice'], collocations:[{text:'geistliche Exerzitien', meaning:'spiritual exercises or retreats'}],
    wordFamilies:[{lemma:'exerzieren', relationLabel:'verb', note:null}], relations:[],
    meanings:m('تمرين روحي أو منهجي؛ تدريب منضبط','ڕاهێنانی ڕۆحی یان ڕێکخراو','spiritual or disciplined exercise','تمرین معنوی یا منظم؛ ریاضت','rahênana ruhî an rêkûpêk','ćwiczenie duchowe lub zdyscyplinowane','exercițiu spiritual sau disciplinat','духовное или дисциплинированное упражнение','ushtrim shpirtëror ose i disiplinuar','ruhsal ya da disiplinli alıştırma'),
    examples:[
      ex('Das tägliche Schreiben wurde für ihn zu einem Exerzitium der Genauigkeit und Selbstprüfung.', m('أصبح الكتابة اليومية بالنسبة له تمريناً في الدقة ومحاسبة الذات.', 'نووسینی ڕۆژانە بۆ ئەو بوو بە ڕاهێنانێکی وردبینی و پشکنینی خود.', 'Daily writing became for him an exercise in precision and self-examination.', 'نوشتن روزانه برای او به تمرینی در دقت و خودسنجی تبدیل شد.', 'Nivîsandina rojane ji bo wî bû rahênanek ji bo rastbînî û xwepirsînê.', 'Codzienne pisanie stało się dla niego ćwiczeniem dokładności i samokontroli.', 'Scrisul zilnic a devenit pentru el un exercițiu de precizie și autoexaminare.', 'Ежедневное письмо стало для него упражнением в точности и самопроверке.', 'Shkrimi i përditshëm u bë për të një ushtrim saktësie dhe vetëkontrolli.', 'Günlük yazı onun için bir kesinlik ve özdenetim alıştırmasına dönüştü.')),
      ex('Die Führungskraft verstand das Zuhören nicht als Technik, sondern als Exerzitium beruflicher Demut.', m('فهم المدير الإصغاء لا كتقنية، بل كتمرين في التواضع المهني.', 'بەڕێوەبەرەکە گوێگرتنی وەک تەکنیک نەدەبینی، بەڵکو وەک ڕاهێنانێکی خاکەڕایی پیشەیی.', 'The manager understood listening not as a technique, but as an exercise in professional humility.', 'مدیر گوش دادن را نه یک تکنیک، بلکه تمرینی در فروتنی حرفه‌ای می‌دانست.', 'Rêveber guhdarîkirin ne wek teknîk, lê wek rahênana nefsbiçûkiya pîşeyî fêm dikir.', 'Menedżer rozumiał słuchanie nie jako technikę, lecz jako ćwiczenie zawodowej pokory.', 'Managerul înțelegea ascultarea nu ca tehnică, ci ca exercițiu de modestie profesională.', 'Руководитель понимал слушание не как технику, а как упражнение в профессиональном смирении.', 'Drejtuesi e kuptonte dëgjimin jo si teknikë, por si ushtrim përulësie profesionale.', 'Yönetici dinlemeyi bir teknik değil, mesleki tevazu alıştırması olarak görüyordu.'))
    ]
  },
  {
    word:'das Fanal', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'das', plural:'Fanale', infinitive:null, pronunciationIpa:null, syllableBreak:'Fa-nal',
    topics:['culture-and-media','advanced-analysis','quality-and-risk'], usageLabels:['formal','written','advanced'], contextLabels:[],
    grammarNotes:['neuter noun; often figurative for a warning signal or dramatic sign'], collocations:[{text:'ein politisches Fanal', meaning:'a political warning signal or dramatic sign'}],
    wordFamilies:[], relations:[],
    meanings:m('إشارة تحذير قوية؛ علامة منذرة','نیشانەیەکی ئاگادارکەرەوەی بەهێز','warning signal; dramatic sign','هشدار جدی؛ نشانه تکان‌دهنده','nîşaneya hişyarkirinê ya bihêz','sygnał ostrzegawczy; dramatyczny znak','semnal de avertizare; semn dramatic','предупреждающий сигнал; яркий знак','sinjal paralajmërues; shenjë dramatike','uyarı işareti; çarpıcı alamet'),
    examples:[
      ex('Der Rücktritt der gesamten Kommission galt als Fanal für eine tiefere institutionelle Krise.', m('اعتُبرت استقالة اللجنة بأكملها إشارة تحذير إلى أزمة مؤسسية أعمق.', 'دەستلەکارکێشانەوەی هەموو کۆمیسیۆنەکە وەک نیشانەی ئاگادارکەرەوە بۆ قەیرانێکی قووڵتری دامەزراوەیی هەژمار کرا.', 'The resignation of the entire commission was seen as a warning signal of a deeper institutional crisis.', 'استعفای کل کمیسیون به‌عنوان هشدار نسبت به بحرانی عمیق‌تر در نهاد تلقی شد.', 'Dest ji kar kişandina tevahiya komîsyonê wek nîşaneya hişyarkirinê ya qeyraneke kûrtir a sazûmanî hate dîtin.', 'Dymisję całej komisji uznano za sygnał ostrzegawczy głębszego kryzysu instytucjonalnego.', 'Demisia întregii comisii a fost considerată un semnal de avertizare pentru o criză instituțională mai profundă.', 'Отставку всей комиссии сочли предупреждающим сигналом более глубокого институционального кризиса.', 'Dorëheqja e gjithë komisionit u pa si sinjal paralajmërues për një krizë më të thellë institucionale.', 'Tüm komisyonun istifası, daha derin bir kurumsal krizin uyarı işareti olarak görüldü.')),
      ex('Für die Literaturkritik wurde der Roman zu einem Fanal gegen die Verharmlosung autoritärer Sprache.', m('أصبح roman بالنسبة للنقد الأدبي إشارة قوية ضد التقليل من خطورة اللغة السلطوية.', 'ڕۆمانەکە بۆ ڕەخنەی ئەدەبی بوو بە نیشانەیەکی بەهێز دژی سووککردنەوەی زمانی دەسەڵاتخواز.', 'For literary criticism, the novel became a warning sign against downplaying authoritarian language.', 'برای نقد ادبی، رمان به نشانه‌ای هشداردهنده علیه کم‌اهمیت جلوه دادن زبان اقتدارگرا تبدیل شد.', 'Ji bo rexneya wêjeyî, roman bû nîşaneyek hişyarkirinê li dijî sivikkirina zimanê otorîter.', 'Dla krytyki literackiej powieść stała się sygnałem ostrzegawczym przeciw bagatelizowaniu języka autorytarnego.', 'Pentru critica literară, romanul a devenit un semnal de avertizare împotriva minimalizării limbajului autoritar.', 'Для литературной критики роман стал предупреждающим знаком против преуменьшения авторитарного языка.', 'Për kritikën letrare, romani u bë sinjal paralajmërues kundër banalizimit të gjuhës autoritare.', 'Edebiyat eleştirisi için roman, otoriter dili hafife almaya karşı bir uyarı işareti oldu.'))
    ]
  }
];

for (const entry of entries) {
  for (const topic of entry.topics) if (!topicSet.has(topic)) throw new Error(`Unknown topic ${topic}`);
  for (const label of [...entry.usageLabels, ...entry.contextLabels]) if (!labelMap.has(label)) throw new Error(`Unknown label ${label}`);
  if (entry.meanings.length !== 10) throw new Error(`Meaning count failed for ${entry.word}`);
  if (entry.examples.length < 2) throw new Error(`Example count failed for ${entry.word}`);
  for (const e of entry.examples) if (e.translations.length !== 10) throw new Error(`Translation count failed for ${entry.word}`);
}
const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const labels = usedLabels.map(k => labelMap.get(k));
const pkg = { packageVersion:'1.0', packageId:`de-${levelLower}-generated-batch-${batch}`, packageName:`German ${level} Generated Batch ${batch}`, source:'Hybrid', defaultMeaningLanguages:langs, labels, entries, collections:[], scenarios:[], conversationStarterPacks:[], eventPreparationPacks:[] };
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');

const importCmd = `dotnet run --project "${path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj')}" -- --target shared --yes "${outPath}"`;
const result = cp.spawnSync(importCmd, { shell:true, encoding:'utf8', cwd:root });
const output = `${result.stdout || ''}\n${result.stderr || ''}`;
console.log(output.replace(/(ConnectionString|Password|Pwd|Secret|Key)=[^\s;]+/gi, '$1=***'));
if (result.status !== 0) throw new Error(`Import command failed with status ${result.status}`);
const ok = output.includes('Entries imported: 6') && output.includes('Entries invalid: 0') && output.includes('Warnings: 0');
if (!ok) throw new Error('Import did not meet strict success criteria; source not modified.');
const remaining = tokens.slice(expected.length);
fs.writeFileSync(sourcePath, remaining.join(', '), 'utf8');
console.log(`SOURCE_UPDATED: ${sourcePath}`);
console.log(`PROCESSED: ${expected.join(' | ')}`);
console.log(`JSON_PATH: ${outPath}`);
console.log(`REMAINING_COUNT: ${remaining.length}`);
console.log(`FIRST_10: ${remaining.slice(0, 10).join(' | ')}`);

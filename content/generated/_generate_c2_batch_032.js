const fs = require('fs');
const cp = require('child_process');
const path = require('path');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '032';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const expected = ['erschleichen', 'erschüttern', 'erschwingen', 'ersinnen', 'eruieren', 'eruptiv'];
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const labelsSource = Array.isArray(taxonomy.labels) ? taxonomy.labels : [];
const topicsSource = Array.isArray(taxonomy.topics) ? taxonomy.topics : [];
const labelMap = new Map(labelsSource.map(l => [l.key, l]));
const topicSet = new Set(topicsSource.map(t => t.key));

const splitTokens = text => text.split(',').map(t => t.trim()).filter(Boolean);
const sourceText = fs.readFileSync(sourcePath, 'utf8');
const tokens = splitTokens(sourceText);
const first = tokens.slice(0, expected.length);
if (JSON.stringify(first) !== JSON.stringify(expected)) {
  throw new Error(`Source token mismatch. Expected ${JSON.stringify(expected)} but found ${JSON.stringify(first)}`);
}

function m(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) {
  return [
    {language:'ar', text:ar}, {language:'ckb', text:ckb}, {language:'en', text:en}, {language:'fa', text:fa}, {language:'kmr', text:kmr},
    {language:'pl', text:pl}, {language:'ro', text:ro}, {language:'ru', text:ru}, {language:'sq', text:sq}, {language:'tr', text:tr}
  ];
}
function ex(baseText, translations) { return { baseText, translations }; }

const entries = [
  {
    word: 'erschleichen', language: 'de', cefrLevel: level, partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'erschleichen', pronunciationIpa: null, syllableBreak: 'er-schlei-chen',
    topics: ['law-and-compliance','business-communication','quality-and-risk'], usageLabels: ['formal','written','administrative','sensitive'], contextLabels: [],
    grammarNotes: ['transitive verb; means to obtain something by deception or improper means'], collocations: [{text:'sich Zugang erschleichen', meaning:'to gain access by deception'}],
    wordFamilies: [{lemma:'schleichen', relationLabel:'verb', note:null}], relations: [],
    meanings: m('ينال شيئاً بالخداع؛ ينتزع بوسيلة غير مشروعة', 'بە فێڵ شتێک بەدەست بهێنێت', 'to obtain by deception; to procure fraudulently', 'با فریب به دست آوردن؛ به‌طور ناروا کسب کردن', 'bi xapandinê bi dest xistin', 'wyłudzić; uzyskać podstępem', 'a obține prin înșelăciune', 'добиваться обманом; получать мошенническим путем', 'të marrësh me mashtrim', 'hileyle elde etmek'),
    examples: [
      ex('Der Mitarbeiter soll sich vertrauliche Zugriffsrechte erschlichen haben, indem er falsche Angaben machte.', m('يُقال إن الموظف حصل بالخداع على صلاحيات وصول سرية من خلال تقديم معلومات كاذبة.', 'دەوترێت کارمەندەکە بە دانانی زانیاری هەڵە مافی دەستگەیشتنی نهێنی بە فێڵ بەدەست هێناوە.', 'The employee is said to have obtained confidential access rights by providing false information.', 'گفته می‌شود کارمند با ارائه اطلاعات نادرست، دسترسی‌های محرمانه را به‌طور فریبکارانه به دست آورده است.', 'Tê gotin ku karmend bi dayîna agahiyên şaş destûrên veşartî bi xapandinê bi dest xistine.', 'Pracownik miał podstępnie uzyskać poufne uprawnienia dostępu, podając fałszywe informacje.', 'Se spune că angajatul ar fi obținut prin înșelăciune drepturi de acces confidențiale, furnizând informații false.', 'Сотрудник, как утверждается, получил конфиденциальные права доступа обманом, предоставив ложные сведения.', 'Punonjësi thuhet se ka marrë me mashtrim të drejta konfidenciale aksesi duke dhënë të dhëna të rreme.', 'Çalışanın yanlış bilgiler vererek gizli erişim haklarını hileyle elde ettiği söyleniyor.')),
      ex('Die Genehmigung wurde widerrufen, weil der Antragsteller sie sich durch gefälschte Nachweise erschlichen hatte.', m('أُلغيت الموافقة لأن مقدم الطلب حصل عليها بالخداع عبر مستندات مزورة.', 'ڕەزامەندییەکە هەڵوەشێندرایەوە چونکە داواکەرەکە بە بەڵگەنامەی ساختە بە فێڵ بەدەستی هێنابوو.', 'The approval was revoked because the applicant had obtained it through forged evidence.', 'مجوز لغو شد، چون متقاضی آن را با مدارک جعلی و فریبکارانه گرفته بود.', 'Destûr hate betal kirin, ji ber ku daxwazkarê wê bi belgeyên sexte bi xapandinê bi dest xistibû.', 'Zezwolenie cofnięto, ponieważ wnioskodawca uzyskał je podstępem za pomocą sfałszowanych dowodów.', 'Aprobarea a fost retrasă deoarece solicitantul o obținuse prin dovezi falsificate.', 'Разрешение было отозвано, потому что заявитель получил его обманом с помощью поддельных документов.', 'Leja u anulua sepse aplikanti e kishte marrë atë me prova të falsifikuara.', 'Başvuru sahibinin sahte belgelerle hile yoluyla aldığı için onay geri çekildi.'))
    ]
  },
  {
    word: 'erschüttern', language: 'de', cefrLevel: level, partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'erschüttern', pronunciationIpa: null, syllableBreak: 'er-schüt-tern',
    topics: ['social-and-relationships','quality-and-risk','advanced-analysis'], usageLabels: ['formal','written','sensitive'], contextLabels: [],
    grammarNotes: ['transitive verb; can refer to emotional shock or destabilization'], collocations: [{text:'das Vertrauen erschüttern', meaning:'to shake trust deeply'}],
    wordFamilies: [{lemma:'die Erschütterung', relationLabel:'noun', note:null}], relations: [],
    meanings: m('يهزّ بعمق؛ يصدم', 'بە قووڵی بلەرزێنێت؛ شۆک بکات', 'to shake deeply; to shock', 'به‌شدت تکان دادن؛ عمیقاً متاثر کردن', 'bi kûrahî hejandin; şok kirin', 'wstrząsnąć; głęboko poruszyć', 'a zgudui; a afecta profund', 'потрясать; глубоко шокировать', 'të tronditësh; të prekësh thellë', 'derinden sarsmak; şoke etmek'),
    examples: [
      ex('Der Datenverlust erschütterte das Vertrauen der Kunden in die Sicherheitsprozesse des Unternehmens.', m('هزّ فقدان البيانات ثقة العملاء في إجراءات الأمان لدى الشركة.', 'لەدەستدانی داتا متمانەی کڕیاران بە پرۆسەکانی ئاسایشی کۆمپانیاکە بە قووڵی لەرزاند.', 'The data loss shook customers’ trust in the company’s security processes.', 'از دست رفتن داده‌ها اعتماد مشتریان به فرایندهای امنیتی شرکت را به‌شدت متزلزل کرد.', 'Windabûna daneyan baweriya xerîdaran bi pêvajoyên ewlehiyê yên şirketê hejand.', 'Utrata danych zachwiała zaufaniem klientów do procesów bezpieczeństwa firmy.', 'Pierderea datelor a zguduit încrederea clienților în procesele de securitate ale companiei.', 'Потеря данных подорвала доверие клиентов к процессам безопасности компании.', 'Humbja e të dhënave tronditi besimin e klientëve te proceset e sigurisë së kompanisë.', 'Veri kaybı, müşterilerin şirketin güvenlik süreçlerine duyduğu güveni derinden sarstı.')),
      ex('Die Nachricht vom plötzlichen Tod der Kollegin erschütterte das gesamte Team.', m('صدمت أخبار الوفاة المفاجئة للزميلة الفريق بأكمله.', 'هەواڵی مردنی لەناکاوی هاوکارە ژنەکە هەموو تیمەکەی بە قووڵی شۆک کرد.', 'The news of the colleague’s sudden death deeply shook the entire team.', 'خبر مرگ ناگهانی همکار زن، تمام تیم را عمیقاً تکان داد.', 'Nûçeya mirina ji nişkê ve ya hevkarê jin tevahiya tîmê bi kûrahî hejand.', 'Wiadomość o nagłej śmierci koleżanki głęboko wstrząsnęła całym zespołem.', 'Vestea morții subite a colegei a zguduit profund întreaga echipă.', 'Новость о внезапной смерти коллеги потрясла всю команду.', 'Lajmi për vdekjen e papritur të koleges tronditi thellë të gjithë ekipin.', 'Kadın çalışma arkadaşının ani ölüm haberi tüm ekibi derinden sarstı.'))
    ]
  },
  {
    word: 'erschwingen', language: 'de', cefrLevel: level, partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'erschwingen', pronunciationIpa: null, syllableBreak: 'er-schwin-gen',
    topics: ['culture-and-media','advanced-analysis','social-and-relationships'], usageLabels: ['formal','written','advanced'], contextLabels: [],
    grammarNotes: ['literary or elevated verb; often means to gain or reach something with effort'], collocations: [{text:'sich zu einer Haltung erschwingen', meaning:'to rise to or attain an attitude with effort'}],
    wordFamilies: [{lemma:'schwingen', relationLabel:'verb', note:null}], relations: [],
    meanings: m('يرتقي إلى شيء بجهد؛ يبلغ مستوى عالياً', 'بە هەوڵ خۆی بگەیەنێتە ئاستێکی بەرز', 'to rise to; to attain with effort', 'با تلاش به سطحی رسیدن؛ خود را به چیزی رساندن', 'bi hewlê xwe bigihînin asta bilind', 'wznieść się do czegoś; osiągnąć coś wysiłkiem', 'a se ridica la; a atinge cu efort', 'подняться до; достичь усилием', 'të ngrihesh deri te; të arrish me përpjekje', 'çabayla yükselmek; erişmek'),
    examples: [
      ex('Nach langem Zögern erschwang sich der Vorstand zu einer öffentlichen Entschuldigung.', m('بعد تردد طويل ارتقى مجلس الإدارة أخيراً إلى تقديم اعتذار علني.', 'دوای دوودڵییەکی درێژ، بەڕێوەبەرایەتییەکە خۆی گەیاندە داوای لێبوردنی ئاشکرا.', 'After long hesitation, the board finally brought itself to issue a public apology.', 'پس از تردید طولانی، هیئت‌مدیره سرانجام خود را به عذرخواهی عمومی رساند.', 'Piştî dudiliyek dirêj, rêveberî xwe gihand lêborînek giştî.', 'Po długim wahaniu zarząd zdobył się na publiczne przeprosiny.', 'După o lungă ezitare, conducerea s-a ridicat la nivelul unei scuze publice.', 'После долгих колебаний правление все же решилось на публичные извинения.', 'Pas një hezitimi të gjatë, bordi më në fund u ngrit deri te një ndjesë publike.', 'Uzun bir tereddütten sonra yönetim kurulu kamuya açık bir özür dilemeye razı oldu.')),
      ex('Der Essay erschwingt sich stellenweise zu einer Sprache, die eher poetisch als analytisch wirkt.', m('يرتقي المقال في بعض المواضع إلى لغة تبدو شعرية أكثر منها تحليلية.', 'وتارەکە لە هەندێ شوێندا خۆی دەگەیەنێتە زمانێک کە زیاتر شاعیرانە دەردەکەوێت تا شیکاری.', 'In places, the essay rises to a language that feels more poetic than analytical.', 'مقاله در بخش‌هایی به زبانی می‌رسد که بیش از آنکه تحلیلی باشد، شاعرانه به نظر می‌رسد.', 'Gotar li hin deveran xwe digihîne zimanekî ku ji analîtîkbûnê zêdetir helbestî xuya dike.', 'Esej miejscami wznosi się do języka, który wydaje się bardziej poetycki niż analityczny.', 'Eseul se ridică pe alocuri la un limbaj care pare mai degrabă poetic decât analitic.', 'Эссе местами поднимается до языка, который кажется скорее поэтичным, чем аналитическим.', 'Eseja në disa vende ngrihet në një gjuhë që duket më shumë poetike sesa analitike.', 'Deneme yer yer analitik olmaktan çok şiirsel görünen bir dile yükseliyor.'))
    ]
  },
  {
    word: 'ersinnen', language: 'de', cefrLevel: level, partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'ersinnen', pronunciationIpa: null, syllableBreak: 'er-sin-nen',
    topics: ['planning-and-projects','advanced-analysis','culture-and-media'], usageLabels: ['formal','written','advanced'], contextLabels: [],
    grammarNotes: ['transitive verb; often elevated, meaning to devise or imagine something'], collocations: [{text:'eine Strategie ersinnen', meaning:'to devise a strategy'}],
    wordFamilies: [{lemma:'der Sinn', relationLabel:'noun', note:null}], relations: [],
    meanings: m('يبتكر؛ يتصور خطة أو فكرة', 'دابنێت؛ بیرۆکە یان پلانێک دابهێنێت', 'to devise; to conceive', 'ابداع کردن؛ طرح‌ریزی کردن', 'afirandin; planek an ramanek çêkirin', 'wymyślić; obmyślić', 'a concepe; a născoci', 'придумывать; замышлять', 'të sajesh; të konceptosh', 'tasarlamak; kurgulamak'),
    examples: [
      ex('Die Projektgruppe musste eine Lösung ersinnen, die rechtlich sauber und zugleich technisch umsetzbar war.', m('كان على فريق المشروع ابتكار حل يكون سليماً قانونياً وقابلاً للتنفيذ تقنياً في الوقت نفسه.', 'گرووپی پرۆژەکە دەبوو چارەسەرێک دابنێت کە یاسایی پاک بێت و لە هەمان کاتدا تەکنیکی جێبەجێ بکرێت.', 'The project group had to devise a solution that was legally sound and technically feasible.', 'گروه پروژه باید راه‌حلی طراحی می‌کرد که هم از نظر حقوقی بی‌نقص و هم از نظر فنی قابل اجرا باشد.', 'Koma projeyê diviya çareseriyek biafirîne ku hem qanûnî saxlem be hem jî teknîkî pêkan be.', 'Grupa projektowa musiała obmyślić rozwiązanie zgodne z prawem i jednocześnie wykonalne technicznie.', 'Grupul de proiect trebuia să conceapă o soluție corectă juridic și totodată realizabilă tehnic.', 'Проектная группа должна была придумать решение, которое было бы юридически корректным и технически осуществимым.', 'Grupi i projektit duhej të sajonte një zgjidhje ligjërisht të pastër dhe njëkohësisht teknikisht të zbatueshme.', 'Proje grubu, hem hukuken sağlam hem de teknik olarak uygulanabilir bir çözüm tasarlamak zorundaydı.')),
      ex('Die Autorin ersann eine Nebenfigur, die die moralische Unsicherheit der Hauptfigur sichtbar macht.', m('ابتكرت الكاتبة شخصية ثانوية تكشف التردد الأخلاقي لدى الشخصية الرئيسية.', 'نووسەرەکە کارەکتەرێکی لاوەکی داهێنا کە نادڵنیایی ئەخلاقی کارەکتەری سەرەکی دەردەخات.', 'The author devised a minor character who makes the main character’s moral uncertainty visible.', 'نویسنده شخصیتی فرعی خلق کرد که تردید اخلاقی شخصیت اصلی را آشکار می‌کند.', 'Nivîskar kesayetiyeke alî afirand ku nediyariya exlaqî ya kesayetiya sereke eşkere dike.', 'Autorka wymyśliła postać drugoplanową, która uwidacznia moralną niepewność głównej bohaterki.', 'Autoarea a conceput un personaj secundar care face vizibilă nesiguranța morală a personajului principal.', 'Писательница придумала второстепенного персонажа, который показывает нравственную неуверенность главного героя.', 'Autorja krijoi një personazh dytësor që e bën të dukshme pasigurinë morale të personazhit kryesor.', 'Yazar, ana karakterin ahlaki belirsizliğini görünür kılan bir yan karakter tasarladı.'))
    ]
  },
  {
    word: 'eruieren', language: 'de', cefrLevel: level, partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'eruieren', pronunciationIpa: null, syllableBreak: 'eru-ie-ren',
    topics: ['data-and-reporting','advanced-analysis','business-communication'], usageLabels: ['formal','written','business','analysis'], contextLabels: [],
    grammarNotes: ['transitive verb; formal, meaning to determine or ascertain through investigation'], collocations: [{text:'den Bedarf eruieren', meaning:'to determine the need'}],
    wordFamilies: [{lemma:'die Eruierung', relationLabel:'noun', note:null}], relations: [],
    meanings: m('يتحقق من؛ يستقصي بدقة', 'بە وردی دیاری بکات؛ لێکۆڵینەوە بکات', 'to determine; to ascertain', 'بررسی و مشخص کردن؛ احراز کردن', 'tespît kirin; bi lêkolînê zelal kirin', 'ustalić; dokładnie zbadać', 'a stabili; a clarifica prin cercetare', 'выяснять; устанавливать', 'të përcaktosh; të verifikosh', 'tespit etmek; araştırarak belirlemek'),
    examples: [
      ex('Vor der Ausschreibung müssen wir eruieren, welche Schnittstellen das neue System tatsächlich benötigt.', m('قبل طرح المناقصة علينا أن نحدد بدقة ما هي الواجهات التي يحتاجها النظام الجديد فعلاً.', 'پێش بانگەوازەکە دەبێت بە وردی دیاری بکەین سیستەمی نوێ بە ڕاستی پێویستی بە کام ڕووکارەکان هەیە.', 'Before the tender, we must determine which interfaces the new system actually needs.', 'پیش از مناقصه باید مشخص کنیم سیستم جدید واقعاً به کدام رابط‌ها نیاز دارد.', 'Berî îhaleyê divê em tespît bikin pergala nû bi rastî hewceyê kîjan navrûyan e.', 'Przed przetargiem musimy ustalić, jakich interfejsów nowe system rzeczywiście potrzebuje.', 'Înainte de licitație trebuie să stabilim de ce interfețe are nevoie cu adevărat noul sistem.', 'Перед тендером нам нужно выяснить, какие интерфейсы действительно нужны новой системе.', 'Para tenderit duhet të përcaktojmë cilat ndërfaqe i duhen realisht sistemit të ri.', 'İhale öncesinde yeni sistemin gerçekte hangi arayüzlere ihtiyaç duyduğunu belirlemeliyiz.')),
      ex('Die Beratungsstelle versucht zunächst zu eruieren, welche Unterstützung die Familie kurzfristig braucht.', m('يحاول مركز الاستشارة أولاً تحديد نوع الدعم الذي تحتاجه الأسرة على المدى القصير.', 'ناوەندی ڕاوێژکاری سەرەتا هەوڵ دەدات دیاری بکات خێزانەکە لە ماوەیەکی کورتدا پێویستی بە چ جۆرە پشتگیرییەک هەیە.', 'The counseling center first tries to determine what support the family needs in the short term.', 'مرکز مشاوره ابتدا تلاش می‌کند مشخص کند خانواده در کوتاه‌مدت به چه حمایتی نیاز دارد.', 'Navenda şêwirmendiyê pêşî hewl dide tespît bike malbat di demeke nêzîk de hewceyê çi piştgiriyê ye.', 'Poradnia najpierw próbuje ustalić, jakiego wsparcia rodzina potrzebuje w najbliższym czasie.', 'Centrul de consiliere încearcă mai întâi să stabilească de ce sprijin are nevoie familia pe termen scurt.', 'Консультационный центр сначала пытается выяснить, какая поддержка нужна семье в ближайшее время.', 'Qendra e këshillimit përpiqet fillimisht të përcaktojë çfarë mbështetjeje i duhet familjes në afat të shkurtër.', 'Danışma merkezi önce ailenin kısa vadede hangi desteğe ihtiyaç duyduğunu belirlemeye çalışır.'))
    ]
  },
  {
    word: 'eruptiv', language: 'de', cefrLevel: level, partOfSpeech: 'Adjective', article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'e-rup-tiv',
    topics: ['advanced-analysis','culture-and-media','quality-and-risk'], usageLabels: ['formal','written','advanced','analysis'], contextLabels: [],
    grammarNotes: ['adjective; used literally for volcanic processes and figuratively for sudden, intense outbursts'], collocations: [{text:'eruptive Gewalt', meaning:'sudden and explosive violence'}],
    wordFamilies: [{lemma:'die Eruption', relationLabel:'noun', note:null}], relations: [],
    meanings: m('اندفاعي؛ انفجاري؛ بركاني', 'تەقینەوەیی؛ لەناکاو و بەهێز', 'eruptive; explosive; sudden and forceful', 'فورانی؛ انفجاری؛ ناگهانی و شدید', 'teqîner; ji nişkê ve û bi hêz', 'erupcyjny; gwałtowny', 'eruptiv; exploziv; brusc și puternic', 'изверженный; взрывной; внезапный и сильный', 'eruptiv; shpërthyes; i papritur dhe i fuqishëm', 'püskürmeli; patlayıcı; ani ve güçlü'),
    examples: [
      ex('Die Krise entwickelte eine eruptive Dynamik, die mit den üblichen Eskalationsmodellen kaum zu erklären war.', m('طورت الأزمة ديناميكية انفجارية كان من الصعب تفسيرها بنماذج التصعيد المعتادة.', 'قەیرانەکە دینامیکییەکی تەقینەوەیی وەریگرت کە بە مۆدێلە ئاساییەکانی هەڵکشانی ناکرێت بە ئاسانی ڕوون بکرێتەوە.', 'The crisis developed an eruptive dynamic that could hardly be explained by the usual escalation models.', 'بحران پویایی انفجاری پیدا کرد که با مدل‌های معمول تشدید به‌سختی قابل توضیح بود.', 'Qeyranê dinamîkeke teqîner pêş xist ku bi modelên asayî yên bilindbûnê bi zorê tê ravekirin.', 'Kryzys rozwinął gwałtowną dynamikę, którą trudno było wyjaśnić zwykłymi modelami eskalacji.', 'Criza a dezvoltat o dinamică eruptivă, greu de explicat prin modelele obișnuite de escaladare.', 'Кризис приобрел взрывную динамику, которую трудно было объяснить обычными моделями эскалации.', 'Kriza zhvilloi një dinamikë shpërthyese që vështirë shpjegohej me modelet e zakonshme të përshkallëzimit.', 'Kriz, olağan tırmanma modelleriyle zor açıklanabilecek patlayıcı bir dinamik kazandı.')),
      ex('Seine eruptive Malweise lässt die Landschaft weniger realistisch als innerlich aufgewühlt erscheinen.', m('أسلوبه الانفجاري في الرسم يجعل المشهد يبدو مضطرباً داخلياً أكثر منه واقعياً.', 'شێوازی وێنەکێشانی تەقینەوەییەکەی دیمەنەکە وا پیشان دەدات کە کەمتر ڕیالیستی و زیاتر ناوخۆیی شڵەژاوە.', 'His eruptive painting style makes the landscape appear less realistic than inwardly agitated.', 'شیوه نقاشی فورانی او منظره را نه چندان واقع‌گرایانه، بلکه درونی و آشفته نشان می‌دهد.', 'Şêweya wênekişandina wî ya teqîner dîmenê kêm realîstîk û zêdetir di hundir de aloz nîşan dide.', 'Jego gwałtowny sposób malowania sprawia, że krajobraz wydaje się mniej realistyczny, a bardziej wewnętrznie wzburzony.', 'Stilul său eruptiv de pictură face ca peisajul să pară mai puțin realist și mai degrabă frământat interior.', 'Его взрывная манера письма делает пейзаж не столько реалистичным, сколько внутренне взволнованным.', 'Mënyra e tij shpërthyese e pikturimit e bën peizazhin të duket më pak realist dhe më shumë i trazuar nga brenda.', 'Onun patlayıcı resim tarzı, manzarayı gerçekçi olmaktan çok içsel olarak çalkantılı gösteriyor.'))
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

const pkg = {
  packageVersion: '1.0',
  packageId: `de-${levelLower}-generated-batch-${batch}`,
  packageName: `German ${level} Generated Batch ${batch}`,
  source: 'Hybrid',
  defaultMeaningLanguages: langs,
  labels,
  entries,
  collections: [],
  scenarios: [],
  conversationStarterPacks: [],
  eventPreparationPacks: []
};
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');

const importCmd = `dotnet run --project "${path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj')}" -- --target shared --yes "${outPath}"`;
const result = cp.spawnSync(importCmd, { shell: true, encoding: 'utf8', cwd: root });
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

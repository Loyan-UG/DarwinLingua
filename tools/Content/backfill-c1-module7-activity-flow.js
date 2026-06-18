const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c1-souveraen-in-studium-beruf-und-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'Konfliktlage klaeren',
    en: 'Clarify the conflict situation',
    fa: 'وضعیت تعارض را روشن کن',
    ar: 'وضّح حالة الخلاف',
    tr: 'Çatışma durumunu netleştir',
    ru: 'Проясни конфликтную ситуацию',
    ckb: 'دۆخی ناکۆکی ڕوون بکەوە',
    kmr: 'Rewşa nakokiyê zelal bike',
    pl: 'Wyjaśnij sytuację konfliktową',
    ro: 'Clarifică situația de conflict',
    sq: 'Qartëso situatën e konfliktit'
  },
  language: {
    de: 'Deeskalierende Sprache sammeln',
    en: 'Collect de-escalating language',
    fa: 'عبارت‌های آرام‌کننده جمع کن',
    ar: 'اجمع عبارات تخفف التوتر',
    tr: 'Gerilimi azaltan dil topla',
    ru: 'Собери язык для снижения напряжения',
    ckb: 'دەربڕینی کەمکردنەوەی گرژی کۆبکەوە',
    kmr: 'Zimana kêmkirina tengaviyê kom bike',
    pl: 'Zbierz język deeskalacyjny',
    ro: 'Adună formulări de detensionare',
    sq: 'Mblidh gjuhë që ul tensionin'
  },
  material: {
    de: 'Konflikt praktisch trainieren',
    en: 'Practise the conflict practically',
    fa: 'تعارض را عملی تمرین کن',
    ar: 'تدرّب عمليًا على الخلاف',
    tr: 'Çatışmayı pratikte çalış',
    ru: 'Практически отработай конфликт',
    ckb: 'ناکۆکییەکە بە کرداری ڕاهێنان بکە',
    kmr: 'Nakokiyê bi pratîkî perwerde bike',
    pl: 'Przećwicz konflikt praktycznie',
    ro: 'Exersează conflictul practic',
    sq: 'Ushtro konfliktin praktikisht'
  },
  apply: {
    de: 'Gespraechsschritt formulieren',
    en: 'Formulate a conversation step',
    fa: 'یک قدم گفت‌وگو را دقیق بیان کن',
    ar: 'صغ خطوة في الحوار',
    tr: 'Bir konuşma adımı formüle et',
    ru: 'Сформулируй шаг разговора',
    ckb: 'هەنگاوێکی گفتوگۆ داڕێژە',
    kmr: 'Gaveke axaftinê formule bike',
    pl: 'Sformułuj krok rozmowy',
    ro: 'Formulează un pas al conversației',
    sq: 'Formulo një hap bisede'
  },
  review: {
    de: 'Ton und Grenze pruefen',
    en: 'Check tone and boundary',
    fa: 'لحن و مرز را بررسی کن',
    ar: 'راجع النبرة والحدود',
    tr: 'Tonu ve sınırı kontrol et',
    ru: 'Проверь тон и границу',
    ckb: 'تۆن و سنوور بپشکنە',
    kmr: 'Ton û sînorê kontrol bike',
    pl: 'Sprawdź ton i granicę',
    ro: 'Verifică tonul și limita',
    sq: 'Kontrollo tonin dhe kufirin'
  }
};

const items = [
  {
    slug: 'c1-schwieriges-feedbackgespraech-fuehren',
    topic: {
      de: 'ein schwieriges Feedbackgespraech fuehren',
      en: 'conducting a difficult feedback conversation',
      fa: 'پیش بردن یک گفت‌وگوی بازخورد دشوار',
      ar: 'إدارة حوار تغذية راجعة صعب',
      tr: 'zor bir geri bildirim görüşmesini yürütme',
      ru: 'ведение сложного разговора с обратной связью',
      ckb: 'بەڕێوەبردنی گفتوگۆیەکی سەختی فیدباک',
      kmr: 'birêvebirina axaftineke zehmet a feedbackê',
      pl: 'prowadzenie trudnej rozmowy feedbackowej',
      ro: 'conducerea unei conversații dificile de feedback',
      sq: 'drejtimi i një bisede të vështirë feedback-u'
    },
    focus: {
      de: 'Beobachtung, Wirkung und Erwartung trennen, ohne die Person anzugreifen',
      en: 'separating observation, effect, and expectation without attacking the person',
      fa: 'مشاهده، اثر و انتظار را از هم جدا کنی، بدون اینکه به خود فرد حمله شود',
      ar: 'الفصل بين الملاحظة والأثر والتوقع من دون مهاجمة الشخص',
      tr: 'kişiye saldırmadan gözlem, etki ve beklentiyi ayırmayı',
      ru: 'разделять наблюдение, эффект и ожидание, не нападая на человека',
      ckb: 'بینین، کاریگەری و چاوەڕوانی جیا بکەیتەوە، بەبێ هێرش بۆ سەر کەسەکە',
      kmr: 'dîtin, bandor û hêviyê cuda kirin bêyî êrîş li kesê',
      pl: 'oddzielenie obserwacji, skutku i oczekiwania bez atakowania osoby',
      ro: 'separarea observației, efectului și așteptării fără a ataca persoana',
      sq: 'ndarjen e vëzhgimit, efektit dhe pritjes pa sulmuar personin'
    },
    grammar: ['c1-hedging-and-cautious-language', 'vorsichtige Sprache'],
    target: ['roleplay', 'c1-ein-schwieriges-feedbackgespraech-fuehren']
  },
  {
    slug: 'c1-fuehrungsentscheidung-taktvoll-kritisieren',
    topic: {
      de: 'eine Fuehrungsentscheidung taktvoll kritisieren',
      en: 'criticizing a leadership decision tactfully',
      fa: 'نقد محترمانه یک تصمیم مدیریتی',
      ar: 'نقد قرار إداري بلباقة',
      tr: 'bir yönetim kararını incelikle eleştirme',
      ru: 'тактичная критика управленческого решения',
      ckb: 'ڕەخنەکردنی بڕیارێکی بەڕێوەبردن بە ڕێزەوە',
      kmr: 'rexnekirina biryareke rêveberiyê bi nazikî',
      pl: 'taktowna krytyka decyzji kierownictwa',
      ro: 'criticarea tactfulă a unei decizii de conducere',
      sq: 'kritikimi me takt i një vendimi drejtues'
    },
    focus: {
      de: 'Respekt fuer die Rolle zeigen und trotzdem die sachlichen Folgen klar benennen',
      en: 'showing respect for the role while clearly naming factual consequences',
      fa: 'به جایگاه طرف مقابل احترام بگذاری و در عین حال پیامدهای واقعی تصمیم را روشن بگویی',
      ar: 'إظهار احترام للدور مع تسمية النتائج الواقعية بوضوح',
      tr: 'role saygı gösterirken somut sonuçları açıkça adlandırmayı',
      ru: 'проявлять уважение к роли и при этом ясно называть фактические последствия',
      ckb: 'ڕێز لە ڕۆڵەکە بگریت و لە هەمان کاتدا دەرئەنجامە بابەتییەکان ڕوون بڵێیت',
      kmr: 'rêz li rola wî/wê girtin û heman demê encamên rastî bi zelalî gotin',
      pl: 'okazanie szacunku dla roli przy jasnym nazwaniu rzeczowych skutków',
      ro: 'respectarea rolului și numirea clară a consecințelor concrete',
      sq: 'të tregosh respekt për rolin dhe njëkohësisht të emërtosh qartë pasojat faktike'
    },
    grammar: ['c1-register-shifting', 'Registerwechsel'],
    target: ['roleplay', 'c1-eine-fuehrungsentscheidung-taktvoll-kritisieren']
  },
  {
    slug: 'c1-eskalation-vorbereiten-ohne-zu-drohen',
    topic: {
      de: 'eine Eskalation vorbereiten, ohne zu drohen',
      en: 'preparing escalation without threatening',
      fa: 'آماده‌کردن پیگیری جدی بدون تهدید کردن',
      ar: 'التحضير للتصعيد من دون تهديد',
      tr: 'tehdit etmeden üst aşamaya taşımaya hazırlanma',
      ru: 'подготовка к эскалации без угроз',
      ckb: 'ئامادەکردنی قۆناغی توندتر بەبێ هەڕەشە',
      kmr: 'amadekirina eskalasyonê bê gefxwarin',
      pl: 'przygotowanie eskalacji bez grożenia',
      ro: 'pregătirea escaladării fără amenințare',
      sq: 'përgatitja e përshkallëzimit pa kërcënuar'
    },
    focus: {
      de: 'naechste Schritte klar ankuendigen, ohne Drucksprache oder Drohung zu verwenden',
      en: 'announcing next steps clearly without pressure language or threats',
      fa: 'قدم‌های بعدی را روشن اعلام کنی، بدون زبان فشار یا تهدید',
      ar: 'إعلان الخطوات التالية بوضوح من دون لغة ضغط أو تهديد',
      tr: 'sonraki adımları baskı dili veya tehdit kullanmadan açıkça bildirmeyi',
      ru: 'ясно объявлять следующие шаги без языка давления или угроз',
      ckb: 'هەنگاوەکانی داهاتوو بە ڕوونی ڕابگەیەنیت، بەبێ زمانی فشار یان هەڕەشە',
      kmr: 'gavên din bi zelalî ragihandin bê zimanê zextê an gefê',
      pl: 'jasne zapowiedzenie kolejnych kroków bez języka presji lub groźby',
      ro: 'anunțarea clară a pașilor următori fără limbaj de presiune sau amenințare',
      sq: 'njoftimin qartë të hapave të ardhshëm pa gjuhë presioni ose kërcënimi'
    },
    grammar: ['c1-concession-structures', 'Konzession und Grenze'],
    target: ['roleplay', 'c1-eine-eskalation-vorbereiten-ohne-zu-drohen']
  },
  {
    slug: 'c1-verhandlung-mit-mehreren-interessen',
    topic: {
      de: 'eine Verhandlung mit mehreren Interessen fuehren',
      en: 'conducting a negotiation with several interests',
      fa: 'پیش بردن مذاکره با چند منفعت متفاوت',
      ar: 'إدارة تفاوض فيه عدة مصالح',
      tr: 'birden çok çıkarın olduğu bir müzakere yürütme',
      ru: 'ведение переговоров с несколькими интересами',
      ckb: 'بەڕێوەبردنی دانوسانێک بە چەند بەرژەوەندی',
      kmr: 'birêvebirina danûstandinekê bi gelek berjewendiyan',
      pl: 'prowadzenie negocjacji z wieloma interesami',
      ro: 'conducerea unei negocieri cu mai multe interese',
      sq: 'drejtimi i një negociate me disa interesa'
    },
    focus: {
      de: 'Positionen, Interessen und Spielraeume getrennt benennen',
      en: 'naming positions, interests, and room for movement separately',
      fa: 'موضع‌ها، منفعت‌ها و فضای مانور را جداگانه نام ببری',
      ar: 'تسمية المواقف والمصالح وهامش الحركة كلٌ على حدة',
      tr: 'pozisyonları, çıkarları ve hareket alanını ayrı ayrı adlandırmayı',
      ru: 'отдельно называть позиции, интересы и пространство для маневра',
      ckb: 'هەڵوێست، بەرژەوەندی و مەودای جووڵان جیا جیا ناو ببەیت',
      kmr: 'helwest, berjewendî û qada livê cuda cuda nav kirin',
      pl: 'osobne nazwanie stanowisk, interesów i pola manewru',
      ro: 'numirea separată a pozițiilor, intereselor și spațiului de manevră',
      sq: 'emërtimin veçmas të pozicioneve, interesave dhe hapësirës së lëvizjes'
    },
    grammar: ['c1-expressing-nuance-and-limitation', 'Nuancierung und Begrenzung'],
    target: ['roleplay', 'c1-eine-verhandlung-mit-mehreren-interessen-fuehren']
  },
  {
    slug: 'c1-kompromiss-unter-vorbehalt-akzeptieren',
    topic: {
      de: 'einen Kompromiss unter Vorbehalt akzeptieren',
      en: 'accepting a compromise with reservations',
      fa: 'پذیرفتن راه‌حل میانی با شرط و احتیاط',
      ar: 'قبول حل وسط مع تحفظات',
      tr: 'bir uzlaşmayı çekinceyle kabul etme',
      ru: 'принятие компромисса с оговорками',
      ckb: 'وەرگرتنی ڕێککەوتنێک بە مەرجەوە',
      kmr: 'qebûlkirina lihevhatinekê bi şert',
      pl: 'przyjęcie kompromisu z zastrzeżeniem',
      ro: 'acceptarea unui compromis cu rezerve',
      sq: 'pranimi i një kompromisi me rezerva'
    },
    focus: {
      de: 'Zustimmung geben und zugleich Bedingung, Risiko und Pruefpunkt festhalten',
      en: 'giving agreement while recording condition, risk, and point to review',
      fa: 'موافقت کنی و همزمان شرط، ریسک و نکته قابل بررسی را ثبت کنی',
      ar: 'إبداء الموافقة مع تثبيت الشرط والخطر ونقطة المراجعة',
      tr: 'onay verirken koşul, risk ve kontrol noktasını kayda geçirmeyi',
      ru: 'соглашаться и одновременно фиксировать условие, риск и пункт проверки',
      ckb: 'ڕەزامەندی بدەیت و لە هەمان کاتدا مەرج، مەترسی و خاڵی پشکنین تۆمار بکەیت',
      kmr: 'erêkirin û heman demê şert, rîsk û xala kontrolê tomar kirin',
      pl: 'wyrażenie zgody przy jednoczesnym zapisaniu warunku, ryzyka i punktu kontroli',
      ro: 'acordul exprimat împreună cu condiția, riscul și punctul de verificare',
      sq: 'dhënien e pëlqimit duke shënuar kushtin, rrezikun dhe pikën e kontrollit'
    },
    grammar: ['c1-concession-structures', 'Konzession und Bedingung'],
    target: ['roleplay', 'c1-einen-kompromiss-unter-vorbehalt-akzeptieren']
  },
  {
    slug: 'c1-organisationskonflikt-moderieren',
    topic: {
      de: 'einen Organisationskonflikt moderieren',
      en: 'moderating an organizational conflict',
      fa: 'مدیریت گفت‌وگو در یک تعارض سازمانی',
      ar: 'إدارة حوار في خلاف تنظيمي',
      tr: 'örgütsel bir çatışmayı yönetme',
      ru: 'модерация организационного конфликта',
      ckb: 'ناوبژیوانی ناکۆکییەکی ڕێکخراوەیی',
      kmr: 'moderasyona nakokiyeke rêxistinî',
      pl: 'moderowanie konfliktu organizacyjnego',
      ro: 'moderarea unui conflict organizațional',
      sq: 'moderimi i një konflikti organizativ'
    },
    focus: {
      de: 'Beteiligte, Sachebene und Entscheidungsweg sichtbar ordnen',
      en: 'visibly organizing participants, factual level, and decision path',
      fa: 'افراد درگیر، سطح موضوعی و مسیر تصمیم‌گیری را شفاف مرتب کنی',
      ar: 'تنظيم الأطراف والمستوى الموضوعي ومسار القرار بوضوح',
      tr: 'katılımcıları, konu düzeyini ve karar yolunu görünür biçimde düzenlemeyi',
      ru: 'ясно упорядочивать участников, предметный уровень и путь решения',
      ckb: 'بەشداربووان، ئاستی بابەتی و ڕێگای بڕیار بە ڕوونی ڕێکبخەیت',
      kmr: 'beşdar, asta babetî û rêya biryarê bi zelalî rêz kirin',
      pl: 'czytelne uporządkowanie uczestników, poziomu rzeczowego i drogi decyzji',
      ro: 'ordonarea clară a participanților, nivelului factual și căii de decizie',
      sq: 'renditjen e qartë të pjesëmarrësve, nivelit faktik dhe rrugës së vendimit'
    },
    grammar: ['c1-c1-discussion-grammar', 'Diskussionsgrammatik'],
    target: ['roleplay', 'c1-einen-organisationskonflikt-moderieren']
  },
  {
    slug: 'c1-persoenliche-und-sachliche-ebene-trennen',
    topic: {
      de: 'persoenliche und sachliche Ebene trennen',
      en: 'separating personal and factual levels',
      fa: 'جدا کردن سطح شخصی و سطح موضوعی',
      ar: 'الفصل بين المستوى الشخصي والموضوعي',
      tr: 'kişisel ve nesnel düzeyi ayırma',
      ru: 'разделение личного и предметного уровня',
      ckb: 'جیاکردنەوەی ئاستی کەسی و بابەتی',
      kmr: 'cuda kirina asta kesî û babetî',
      pl: 'oddzielanie poziomu osobistego i rzeczowego',
      ro: 'separarea nivelului personal de cel factual',
      sq: 'ndarja e nivelit personal dhe faktik'
    },
    focus: {
      de: 'Gefuehle anerkennen und trotzdem zur konkreten Sache zurueckfuehren',
      en: 'acknowledging feelings while still returning to the concrete issue',
      fa: 'احساسات را به رسمیت بشناسی و با این حال بحث را به موضوع مشخص برگردانی',
      ar: 'الاعتراف بالمشاعر مع العودة إلى الموضوع المحدد',
      tr: 'duyguları kabul edip yine de somut konuya dönmeyi',
      ru: 'признавать чувства и всё же возвращаться к конкретному вопросу',
      ckb: 'هەستەکان بناسیت و لەگەڵ ئەوەشدا گفتوگۆکە بگەڕێنیتەوە بۆ بابەتی دیاریکراو',
      kmr: 'hestan nas kirin û dîsa vegerandina gotûbêjê bo mijara konkret',
      pl: 'uznanie emocji i jednoczesny powrót do konkretnej sprawy',
      ro: 'recunoașterea sentimentelor și revenirea la problema concretă',
      sq: 'pranimin e ndjenjave dhe kthimin te çështja konkrete'
    },
    grammar: ['c1-register-shifting', 'Registerwechsel'],
    target: ['roleplay', 'c1-eine-indirekte-kritik-richtig-ansprechen']
  },
  {
    slug: 'c1-verbraucherbeschwerde-sachlich-eskalieren',
    topic: {
      de: 'eine Verbraucherbeschwerde sachlich eskalieren',
      en: 'escalating a consumer complaint factually',
      fa: 'پیگیری جدی و عینی یک شکایت مصرف‌کننده',
      ar: 'تصعيد شكوى مستهلك بشكل موضوعي',
      tr: 'tüketici şikâyetini nesnel biçimde üst aşamaya taşıma',
      ru: 'предметная эскалация потребительской жалобы',
      ckb: 'توندکردنەوەی سکاڵای بەکاربەر بە شێوەی بابەتی',
      kmr: 'eskalasyona gilîya xerîdar bi awayekî babetî',
      pl: 'rzeczowa eskalacja reklamacji konsumenckiej',
      ro: 'escaladarea factuală a unei reclamații de consumator',
      sq: 'përshkallëzimi faktik i një ankese konsumatori'
    },
    focus: {
      de: 'Nachweise, Frist und erwartete Loesung klar, aber nicht aggressiv formulieren',
      en: 'formulating evidence, deadline, and expected solution clearly but not aggressively',
      fa: 'مدرک‌ها، مهلت و راه‌حل مورد انتظار را روشن ولی غیرتهاجمی بیان کنی',
      ar: 'صياغة الأدلة والمهلة والحل المتوقع بوضوح ومن دون عدوانية',
      tr: 'kanıtları, süreyi ve beklenen çözümü açık ama saldırgan olmayan biçimde ifade etmeyi',
      ru: 'ясно, но не агрессивно формулировать доказательства, срок и ожидаемое решение',
      ckb: 'بەڵگە، ماوە و چارەسەری چاوەڕوانکراو بە ڕوونی و بەبێ هێرشی بڵێیت',
      kmr: 'delîl, dem û çareseriya hêvîkirî bi zelalî lê ne bi êrîşkarî gotin',
      pl: 'jasne, ale nieagresywne sformułowanie dowodów, terminu i oczekiwanego rozwiązania',
      ro: 'formularea clară, dar neagresivă a dovezilor, termenului și soluției așteptate',
      sq: 'formulimin qartë, por jo agresiv, të provave, afatit dhe zgjidhjes së pritur'
    },
    grammar: ['c1-formal-style-in-essays', 'formeller Stil'],
    target: ['roleplay', 'c1-eine-verbraucherbeschwerde-sachlich-eskalieren']
  },
  {
    slug: 'c1-ablehnung-hinterfragen-ohne-aggression',
    topic: {
      de: 'eine Ablehnung hinterfragen, ohne aggressiv zu wirken',
      en: 'questioning a rejection without sounding aggressive',
      fa: 'پرسیدن دلیل رد شدن بدون حالت تهاجمی',
      ar: 'مراجعة سبب الرفض من دون أن تبدو عدوانيًا',
      tr: 'reddi saldırgan görünmeden sorgulama',
      ru: 'уточнение отказа без агрессивного звучания',
      ckb: 'پرسیارکردن لە هۆکاری ڕەتکردنەوە بەبێ تۆنی هێرشی',
      kmr: 'pirsîna redkirinê bê dengê êrîşkar',
      pl: 'dopytanie o odmowę bez agresywnego tonu',
      ro: 'chestionarea unui refuz fără ton agresiv',
      sq: 'pyetja për një refuzim pa tingëlluar agresiv'
    },
    focus: {
      de: 'Begruendung einfordern und zugleich Kooperationsbereitschaft signalisieren',
      en: 'asking for a reason while signaling willingness to cooperate',
      fa: 'دلیل بخواهی و همزمان نشان بدهی که آماده همکاری هستی',
      ar: 'طلب التبرير مع إظهار الاستعداد للتعاون',
      tr: 'gerekçe isterken işbirliğine açık olduğunu göstermeyi',
      ru: 'запрашивать обоснование и одновременно показывать готовность к сотрудничеству',
      ckb: 'داوای هۆکار بکەیت و لە هەمان کاتدا ئامادەیی بۆ هاوکاری نیشان بدەیت',
      kmr: 'sedem xwestin û heman demê amadebûna hevkariyê nîşan dan',
      pl: 'poproszenie o uzasadnienie przy jednoczesnym sygnalizowaniu gotowości do współpracy',
      ro: 'cererea unei justificări și semnalarea disponibilității de cooperare',
      sq: 'kërkimin e arsyes dhe njëkohësisht sinjalizimin e gatishmërisë për bashkëpunim'
    },
    grammar: ['c1-hedging-and-cautious-language', 'vorsichtige Sprache'],
    target: ['roleplay', 'c1-eine-ablehnung-des-serviceanbieters-hinterfragen']
  },
  {
    slug: 'c1-konflikt-feedback-und-verhandlung-wiederholen',
    topic: {
      de: 'Konflikt, Feedback und Verhandlung wiederholen',
      en: 'reviewing conflict, feedback, and negotiation',
      fa: 'مرور تعارض، بازخورد و مذاکره',
      ar: 'مراجعة الخلاف والتغذية الراجعة والتفاوض',
      tr: 'çatışma, geri bildirim ve müzakere tekrarı',
      ru: 'повторение конфликта, обратной связи и переговоров',
      ckb: 'دووبارەکردنەوەی ناکۆکی، فیدباک و دانوسان',
      kmr: 'dubarekirina nakokî, feedback û danûstandinê',
      pl: 'powtórka konfliktu, feedbacku i negocjacji',
      ro: 'recapitularea conflictului, feedbackului și negocierii',
      sq: 'përsëritja e konfliktit, feedback-ut dhe negociatës'
    },
    focus: {
      de: 'Klarheit, Beziehungsschutz und naechsten Schritt in einem Gespraech verbinden',
      en: 'combining clarity, relationship protection, and next step in one conversation',
      fa: 'شفافیت، حفظ رابطه و قدم بعدی را در یک گفت‌وگو به هم وصل کنی',
      ar: 'ربط الوضوح وحماية العلاقة والخطوة التالية في حوار واحد',
      tr: 'netliği, ilişkiyi korumayı ve sonraki adımı tek konuşmada birleştirmeyi',
      ru: 'соединять ясность, сохранение отношений и следующий шаг в одном разговоре',
      ckb: 'ڕوونی، پاراستنی پەیوەندی و هەنگاوی داهاتوو لە گفتوگۆیەکدا پێکەوە ببەستیت',
      kmr: 'zelalî, parastina peywendiyê û gava din di axaftinekê de girêdan',
      pl: 'łączenie jasności, ochrony relacji i następnego kroku w jednej rozmowie',
      ro: 'îmbinarea clarității, protejării relației și pasului următor într-o conversație',
      sq: 'lidhjen e qartësisë, mbrojtjes së marrëdhënies dhe hapit tjetër në një bisedë'
    },
    grammar: ['c1-c1-register-review', 'Register-Review'],
    target: ['exam-prep-unit', 'c1-einwaende-produktiv-aufgreifen']
  }
];

function translations(map) {
  return langs.map(language => ({ language, text: map[language] }));
}

function title(key) {
  return {
    title: titles[key].de,
    titleTranslations: translations(titles[key])
  };
}

function orientInstruction(item) {
  return {
    de: `Lies den Lektionstext zu ${item.topic.de} und notiere, worin der eigentliche Konflikt besteht.`,
    en: `Read the lesson text on ${item.topic.en} and note what the actual conflict is.`,
    fa: `متن درس درباره ${item.topic.fa} را بخوان و یادداشت کن تعارض اصلی دقیقاً چیست.`,
    ar: `اقرأ نص الدرس عن ${item.topic.ar} وسجّل ما هو الخلاف الحقيقي بدقة.`,
    tr: `${item.topic.tr} konulu ders metnini oku ve asıl çatışmanın tam olarak ne olduğunu not et.`,
    ru: `Прочитай текст урока о теме «${item.topic.ru}» и отметь, в чем именно состоит реальный конфликт.`,
    ckb: `دەقی وانەکە لەسەر ${item.topic.ckb} بخوێنەوە و بنووسە ناکۆکییە سەرەکییەکە بە وردی چییە.`,
    kmr: `Nivîsa dersê li ser ${item.topic.kmr} bixwîne û binivîse nakokiya rastîn bi rastî çi ye.`,
    pl: `Przeczytaj tekst lekcji o temacie: ${item.topic.pl} i zanotuj, na czym dokładnie polega właściwy konflikt.`,
    ro: `Citește textul lecției despre ${item.topic.ro} și notează în ce constă exact conflictul real.`,
    sq: `Lexo tekstin e mësimit për ${item.topic.sq} dhe shëno cili është saktësisht konflikti kryesor.`
  };
}

function languageInstruction(item) {
  return {
    de: `Oeffne ${item.grammar[1]} und sammle drei Formulierungen, mit denen du ${item.focus.de}.`,
    en: `Open the linked language section and collect three formulations that help you with this skill: ${item.focus.en}.`,
    fa: `بخش زبانی لینک‌شده را باز کن و سه عبارت پیدا کن که به این مهارت کمک کند: ${item.focus.fa}.`,
    ar: `افتح القسم اللغوي المرتبط واجمع ثلاث صيغ تساعد على هذه المهارة: ${item.focus.ar}.`,
    tr: `Bağlantılı dil bölümünü aç ve şu beceriye yardım eden üç ifade topla: ${item.focus.tr}.`,
    ru: `Открой связанный языковой раздел и собери три формулировки для этого умения: ${item.focus.ru}.`,
    ckb: `بەشی زمانی بەستەرکراو بکەرەوە و سێ دەربڕین کۆبکەوە کە یارمەتی ئەم توانایە بدات: ${item.focus.ckb}.`,
    kmr: `Beşa zimanê ya girêdayî veke û sê derbirînan kom bike ku alîkarîya vê jêhatinê bikin: ${item.focus.kmr}.`,
    pl: `Otwórz połączoną sekcję językową i zbierz trzy sformułowania wspierające tę umiejętność: ${item.focus.pl}.`,
    ro: `Deschide secțiunea lingvistică legată și adună trei formulări care susțin această abilitate: ${item.focus.ro}.`,
    sq: `Hap seksionin e lidhur gjuhësor dhe mblidh tri formulime që ndihmojnë këtë aftësi: ${item.focus.sq}.`
  };
}

function materialInstruction() {
  return {
    de: 'Bearbeite das verlinkte Rollenspiel oder Pruefungsmaterial und achte darauf, wie Spannung reduziert wird, ohne das Problem zu beschweigen.',
    en: 'Work through the linked roleplay or exam material and notice how tension is reduced without keeping the problem silent.',
    fa: 'تمرین لینک‌شده را انجام بده و دقت کن چطور تنش کم می‌شود، بدون اینکه مشکل اصلی ناگفته بماند.',
    ar: 'اعمل على التدريب المرتبط وانتبه إلى كيفية خفض التوتر من دون إسكات المشكلة الأساسية.',
    tr: 'Bağlantılı alıştırmayı çalış ve ana sorun suskun bırakılmadan gerilimin nasıl azaltıldığına dikkat et.',
    ru: 'Проработай связанное задание и обрати внимание, как напряжение снижается, не замалчивая основную проблему.',
    ckb: 'ڕاهێنانی بەستەرکراو کاربکە و سەرنج بدە چۆن گرژی کەم دەبێتەوە، بەبێ ئەوەی کێشەی سەرەکی بێدەنگ بکرێت.',
    kmr: 'Rahênana girêdayî bixebite û bala xwe bide ka tengavî çawa kêm dibe bêyî ku pirsgirêka sereke bê deng were hiştin.',
    pl: 'Przerób połączone ćwiczenie i zwróć uwagę, jak zmniejsza się napięcie bez przemilczania głównego problemu.',
    ro: 'Lucrează cu exercițiul legat și observă cum scade tensiunea fără ca problema centrală să fie trecută sub tăcere.',
    sq: 'Puno me ushtrimin e lidhur dhe vëzhgo si ulet tensioni pa e heshtur problemin kryesor.'
  };
}

function applyInstruction() {
  return {
    de: 'Formuliere einen Gespraechsschritt in vier Teilen: Anerkennung, Sachpunkt, Grenze oder Bitte, naechster Schritt.',
    en: 'Formulate one conversation step in four parts: acknowledgment, factual point, boundary or request, next step.',
    fa: 'یک قدم گفت‌وگو را در چهار بخش بنویس: پذیرش یا درک طرف مقابل، نکته موضوعی، مرز یا درخواست، قدم بعدی.',
    ar: 'صغ خطوة في الحوار في أربعة أجزاء: اعتراف أو تفهم، نقطة موضوعية، حد أو طلب، خطوة تالية.',
    tr: 'Bir konuşma adımını dört parçada formüle et: kabul veya anlayış, somut nokta, sınır ya da rica, sonraki adım.',
    ru: 'Сформулируй шаг разговора из четырех частей: признание, предметный пункт, граница или просьба, следующий шаг.',
    ckb: 'هەنگاوێکی گفتوگۆ بە چوار بەش بنووسە: دانپێدانان یان تێگەیشتن، خاڵی بابەتی، سنوور یان داواکاری، هەنگاوی داهاتوو.',
    kmr: 'Gaveke axaftinê bi çar beşan formule bike: pejirandin an têgihiştin, xalê babetî, sînor an daxwaz, gava din.',
    pl: 'Sformułuj krok rozmowy w czterech częściach: uznanie, punkt rzeczowy, granica lub prośba, następny krok.',
    ro: 'Formulează un pas al conversației în patru părți: recunoaștere, punct factual, limită sau cerere, pas următor.',
    sq: 'Formulo një hap bisede në katër pjesë: njohje, pikë faktike, kufi ose kërkesë, hapi tjetër.'
  };
}

function reviewInstruction() {
  return {
    de: 'Pruefe, ob dein Schritt klar und respektvoll bleibt und trotzdem eine echte Grenze oder Bitte enthaelt.',
    en: 'Check whether your step stays clear and respectful while still containing a real boundary or request.',
    fa: 'بررسی کن که قدم گفت‌وگوی تو روشن و محترمانه می‌ماند و با این حال یک مرز یا درخواست واقعی دارد.',
    ar: 'راجع ما إذا كانت خطوتك واضحة ومحترمة ومع ذلك تتضمن حدًا أو طلبًا حقيقيًا.',
    tr: 'Adımının açık ve saygılı kaldığını ve yine de gerçek bir sınır ya da rica içerdiğini kontrol et.',
    ru: 'Проверь, остается ли твой шаг ясным и уважительным и содержит ли он реальную границу или просьбу.',
    ckb: 'بپشکنە هەنگاوەکەت ڕوون و ڕێزدارانە دەمێنێتەوە و لەگەڵ ئەوەشدا سنوور یان داواکارییەکی ڕاستەقینەی تێدایە.',
    kmr: 'Kontrol bike ka gava te zelal û bi rêz dimîne û dîsa sînorek an daxwazeke rastîn tê de heye.',
    pl: 'Sprawdź, czy twój krok pozostaje jasny i pełen szacunku, a jednocześnie zawiera realną granicę lub prośbę.',
    ro: 'Verifică dacă pasul tău rămâne clar și respectuos și totuși conține o limită sau o cerere reală.',
    sq: 'Kontrollo nëse hapi yt mbetet i qartë dhe respektues dhe prapë përmban një kufi ose kërkesë reale.'
  };
}

function block(kind, titleKey, instructionMap, targetType, targetSlug, estimatedMinutes, sortOrder, required) {
  return {
    kind,
    ...title(titleKey),
    instruction: instructionMap.de,
    instructionTranslations: translations(instructionMap),
    targetType,
    targetSlug: targetSlug || null,
    estimatedMinutes,
    sortOrder,
    isRequired: required
  };
}

for (const item of items) {
  const lesson = lessons.find(l => l.slug === item.slug);
  if (!lesson) {
    throw new Error(`Missing lesson ${item.slug}`);
  }

  lesson.activityBlocks = [
    block('read', 'orient', orientInstruction(item), 'none', null, 6, 10, true),
    block('grammar', 'language', languageInstruction(item), 'grammar-topic', item.grammar[0], 8, 20, true),
    block(item.target[0] === 'roleplay' ? 'roleplay' : 'exam-prep', 'material', materialInstruction(item), item.target[0], item.target[1], 10, 30, true),
    block('practice', 'apply', applyInstruction(item), 'none', null, 8, 40, true),
    block('review', 'review', reviewInstruction(item), 'none', null, 6, 50, true)
  ];
}

fs.writeFileSync(file, JSON.stringify(data, null, 2) + '\n', 'utf8');
console.log(`Updated ${items.length} C1 Module 7 lessons with ${items.length * 5} activity blocks.`);

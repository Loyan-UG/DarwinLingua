const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c2-stil-souveraenitaet-und-komplexer-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'Aufgabe als Ganzes planen',
    en: 'Plan the task as a whole',
    fa: 'کل کار را یکپارچه برنامه‌ریزی کن',
    ar: 'خطط للمهمة بوصفها كلًا واحدًا',
    tr: 'Görevi bir bütün olarak planla',
    ru: 'Спланируй задачу как единое целое',
    ckb: 'ئەرکەکە وەک یەک گشتی پلاندانێ',
    kmr: 'Karê wekî yekpareyekê plansaz bike',
    pl: 'Zaplanuj zadanie jako całość',
    ro: 'Planifică sarcina ca întreg',
    sq: 'Planifiko detyrën si një tërësi'
  },
  language: {
    de: 'Sprachwerkzeug gezielt waehlen',
    en: 'Choose the language tool deliberately',
    fa: 'ابزار زبانی را آگاهانه انتخاب کن',
    ar: 'اختر الأداة اللغوية بوعي',
    tr: 'Dil aracını bilinçli seç',
    ru: 'Выбери языковой инструмент осознанно',
    ckb: 'ئامرازی زمانی بە ئاگایی هەڵبژێرە',
    kmr: 'Amûra zimanî bi hişyarî hilbijêre',
    pl: 'Wybierz narzędzie językowe świadomie',
    ro: 'Alege instrumentul lingvistic deliberat',
    sq: 'Zgjidh mjetin gjuhësor me vetëdije'
  },
  target: {
    de: 'Transfer sauber ausfuehren',
    en: 'Carry out the transfer cleanly',
    fa: 'انتقال به موقعیت تازه را دقیق انجام بده',
    ar: 'نفّذ النقل إلى السياق الجديد بدقة',
    tr: 'Aktarımı temiz biçimde uygula',
    ru: 'Выполни перенос в новую ситуацию точно',
    ckb: 'گواستنەوە بۆ دۆخی نوێ بە وردی ئەنجام بدە',
    kmr: 'Veguhastina bo rewşa nû bi paqijî bike',
    pl: 'Wykonaj transfer do nowej sytuacji precyzyjnie',
    ro: 'Realizează transferul în noul context cu precizie',
    sq: 'Bëje kalimin në situatën e re me saktësi'
  },
  portfolio: {
    de: 'Eigenes Ergebnis sichern',
    en: 'Secure your own result',
    fa: 'نتیجه خودت را قابل استفاده نگه دار',
    ar: 'احفظ نتيجتك بحيث يمكن استخدامها لاحقًا',
    tr: 'Kendi sonucunu kullanılabilir hâlde sakla',
    ru: 'Сохрани свой результат так, чтобы им можно было пользоваться',
    ckb: 'ئەنجامی خۆت بە شێوەیەک بپارێزە کە دواتر بەکاربهێنرێت',
    kmr: 'Encama xwe wisa biparêze ku paşê were bikaranîn',
    pl: 'Zabezpiecz swój wynik tak, by dało się z niego korzystać',
    ro: 'Păstrează rezultatul propriu într-o formă utilizabilă',
    sq: 'Ruaje rezultatin tënd në një formë të përdorshme'
  },
  review: {
    de: 'Qualitaet ehrlich pruefen',
    en: 'Check quality honestly',
    fa: 'کیفیت را صادقانه بررسی کن',
    ar: 'افحص الجودة بصدق',
    tr: 'Kaliteyi dürüstçe kontrol et',
    ru: 'Проверь качество честно',
    ckb: 'کوالێتی بە ڕاستگۆیی بپشکنە',
    kmr: 'Qalîteyê bi rastî kontrol bike',
    pl: 'Uczciwie sprawdź jakość',
    ro: 'Verifică onest calitatea',
    sq: 'Kontrollo cilësinë me ndershmëri'
  }
};

const items = [
  {
    slug: 'c2-integriertes-projekt-expertentext-und-debatte',
    topic: {
      de: 'Expertentext und Debatte',
      en: 'expert text and debate',
      fa: 'متن تخصصی و بحث استدلالی',
      ar: 'نص متخصص ونقاش حجاجي',
      tr: 'uzman metni ve tartışma',
      ru: 'экспертный текст и дискуссия',
      ckb: 'دەقی پسپۆڕی و گفتوگۆی بەڵگەدار',
      kmr: 'nivîsa pisporî û nîqaşa bi arguman',
      pl: 'tekst ekspercki i debata',
      ro: 'text de specialitate și dezbatere',
      sq: 'tekst ekspertësh dhe debat'
    },
    grammar: 'c2-nominal-style-in-expert-texts',
    targetKind: 'write',
    targetType: 'writing-template',
    targetSlug: 'c2-expertentext-fuer-laien-verstaendlich-machen'
  },
  {
    slug: 'c2-integriertes-projekt-krisenfall-und-statement',
    topic: {
      de: 'Krisenfall und Statement',
      en: 'crisis case and statement',
      fa: 'وضعیت بحرانی و بیانیه',
      ar: 'حالة أزمة وبيان',
      tr: 'kriz durumu ve açıklama',
      ru: 'кризисная ситуация и заявление',
      ckb: 'دۆخی قەیران و ڕاگەیاندن',
      kmr: 'rewşa krîzê û daxuyanî',
      pl: 'sytuacja kryzysowa i oświadczenie',
      ro: 'situație de criză și comunicat',
      sq: 'rast krize dhe deklaratë'
    },
    grammar: 'c2-journalistic-compression',
    targetKind: 'write',
    targetType: 'writing-template',
    targetSlug: 'c2-krisenupdate-intern-abstimmen'
  },
  {
    slug: 'c2-integriertes-projekt-kultur-und-literatur',
    topic: {
      de: 'Kultur und Literatur',
      en: 'culture and literature',
      fa: 'فرهنگ و ادبیات',
      ar: 'الثقافة والأدب',
      tr: 'kültür ve edebiyat',
      ru: 'культура и литература',
      ckb: 'کەلتوور و ئەدەبیات',
      kmr: 'çand û wêje',
      pl: 'kultura i literatura',
      ro: 'cultură și literatură',
      sq: 'kulturë dhe letërsi'
    },
    grammar: 'c2-literary-sentence-structures',
    targetKind: 'roleplay',
    targetType: 'roleplay',
    targetSlug: 'c2-goethe-c2-eine-literarische-position-diskutieren'
  },
  {
    slug: 'c2-stilportfolio-aufbauen',
    topic: {
      de: 'Stilportfolio',
      en: 'style portfolio',
      fa: 'پرونده شخصی سبک نوشتار و بیان',
      ar: 'ملف شخصي للأسلوب',
      tr: 'üslup portföyü',
      ru: 'портфолио стиля',
      ckb: 'پۆرتفۆلیۆی شێوازی دەربڕین',
      kmr: 'portfolyoya şêwazê',
      pl: 'portfolio stylu',
      ro: 'portofoliu de stil',
      sq: 'portofol stili'
    },
    grammar: 'c2-c2-register-review',
    targetKind: 'write',
    targetType: 'writing-template',
    targetSlug: 'c2-selbstlektorat-kommentar-fuer-eigenen-text'
  },
  {
    slug: 'c2-eigene-staerken-und-risiken-analysieren',
    topic: {
      de: 'eigene Staerken und Risiken',
      en: 'your own strengths and risks',
      fa: 'نقاط قوت و ریسک‌های شخصی خودت',
      ar: 'نقاط قوتك ومخاطرك الشخصية',
      tr: 'kendi güçlü yönlerin ve risklerin',
      ru: 'свои сильные стороны и риски',
      ckb: 'هێز و مەترسییەکانی خۆت',
      kmr: 'hêz û rîskên xwe',
      pl: 'własne mocne strony i ryzyka',
      ro: 'propriile puncte forte și riscuri',
      sq: 'pikat e tua të forta dhe rreziqet'
    },
    grammar: 'c2-c2-common-pitfalls',
    targetKind: 'exam-prep',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-qualitaetscheck-priorisieren'
  },
  {
    slug: 'c2-grammatik-und-stil-vernetzt-wiederholen',
    topic: {
      de: 'Grammatik und Stil',
      en: 'grammar and style',
      fa: 'گرامر و سبک',
      ar: 'القواعد والأسلوب',
      tr: 'dil bilgisi ve üslup',
      ru: 'грамматика и стиль',
      ckb: 'ڕێزمان و شێواز',
      kmr: 'rêziman û şêwaz',
      pl: 'gramatyka i styl',
      ro: 'gramatică și stil',
      sq: 'gramatikë dhe stil'
    },
    grammar: 'c2-c2-grammar-review-map',
    targetKind: 'exam-prep',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-bewertungskriterien-in-handlungen-uebersetzen'
  },
  {
    slug: 'c2-repertoire-pflegen-und-aktualisieren',
    topic: {
      de: 'sprachliches Repertoire',
      en: 'language repertoire',
      fa: 'گنجینه زبانی شخصی',
      ar: 'ذخيرتك اللغوية',
      tr: 'dil repertuvarı',
      ru: 'языковой репертуар',
      ckb: 'کۆگای زمانی خۆت',
      kmr: 'repertuara zimanî',
      pl: 'repertuar językowy',
      ro: 'repertoriu lingvistic',
      sq: 'repertor gjuhësor'
    },
    grammar: 'c2-stylistic-variation-in-german-grammar',
    targetKind: 'roleplay',
    targetType: 'roleplay',
    targetSlug: 'c2-einen-komplexen-standpunkt-in-zwei-minuten-verdichten'
  },
  {
    slug: 'c2-c2-abschluss-selbstcheck',
    topic: {
      de: 'Abschluss-Selbstcheck',
      en: 'final self-check',
      fa: 'خودارزیابی پایانی',
      ar: 'تقييم ذاتي ختامي',
      tr: 'son öz değerlendirme',
      ru: 'итоговая самопроверка',
      ckb: 'خۆهەڵسەنگاندنی کۆتایی',
      kmr: 'xwe-nirxandina dawî',
      pl: 'końcowa samoocena',
      ro: 'autoverificare finală',
      sq: 'vetëkontroll përfundimtar'
    },
    grammar: 'c2-c2-grammar-review-map',
    targetKind: 'exam-prep',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-antwort-vor-abgabe-kalibrieren'
  },
  {
    slug: 'c2-nach-c2-weiterlernen',
    topic: {
      de: 'Weiterlernen nach dem Kurs',
      en: 'continuing to learn after the course',
      fa: 'ادامه یادگیری بعد از دوره',
      ar: 'مواصلة التعلم بعد الدورة',
      tr: 'kurstan sonra öğrenmeye devam etme',
      ru: 'дальнейшее обучение после курса',
      ckb: 'بەردەوامبوون لە فێربوون دوای کۆرس',
      kmr: 'piştî kursê berdewam fêrbûn',
      pl: 'dalsza nauka po kursie',
      ro: 'continuarea învățării după curs',
      sq: 'vazhdimi i të mësuarit pas kursit'
    },
    grammar: 'c2-stylistic-variation-in-german-grammar',
    targetKind: 'write',
    targetType: 'writing-template',
    targetSlug: 'c2-abschlussstatement-mit-stilistischer-souveraenitaet'
  },
  {
    slug: 'c2-abschluss-und-meisterschaftspflege',
    topic: {
      de: 'Abschluss und Meisterschaftspflege',
      en: 'completion and maintaining mastery',
      fa: 'پایان دوره و نگهداری مهارت پیشرفته',
      ar: 'الخاتمة والحفاظ على مستوى الإتقان',
      tr: 'tamamlama ve ustalığı sürdürme',
      ru: 'завершение и поддержание мастерства',
      ckb: 'کۆتایی و پاراستنی ئاستی باڵا',
      kmr: 'dawî û parastina hostetiyê',
      pl: 'zakończenie i utrzymanie mistrzostwa',
      ro: 'încheiere și menținerea măiestriei',
      sq: 'përfundim dhe mirëmbajtje e zotërimit'
    },
    grammar: 'c2-c2-register-review',
    targetKind: 'write',
    targetType: 'writing-template',
    targetSlug: 'c2-abschlussstatement-mit-stilistischer-souveraenitaet'
  }
];

function tr(map) {
  return langs.map(language => ({ language, text: map[language] }));
}

function createActivity(kind, titleKey, instructionMap, targetType, targetSlug, minutes, sortOrder, isRequired = true) {
  return {
    kind,
    title: titles[titleKey].de,
    titleTranslations: tr(titles[titleKey]),
    instruction: instructionMap.de,
    instructionTranslations: tr(instructionMap),
    targetType,
    targetSlug: targetType === 'none' ? null : targetSlug,
    estimatedMinutes: minutes,
    sortOrder,
    isRequired
  };
}

function orientInstruction(item) {
  return {
    de: `Lies die Lesson zu ${item.topic.de}. Entscheide zuerst: Welche Teilaufgaben gehoeren zusammen, welche Reihenfolge ist sinnvoll, und wo darfst du bewusst vereinfachen?`,
    en: `Read the lesson on ${item.topic.en}. First decide: which subtasks belong together, which order makes sense, and where may you simplify deliberately?`,
    fa: `درس مربوط به ${item.topic.fa} را بخوان. اول تصمیم بگیر: کدام بخش‌های کار به هم مربوط‌اند، چه ترتیبی منطقی است، و کجا می‌توانی آگاهانه ساده‌سازی کنی؟`,
    ar: `اقرأ الدرس عن ${item.topic.ar}. قرر أولًا: أي الأجزاء تنتمي إلى بعضها، وما الترتيب المنطقي، وأين يمكنك التبسيط بوعي؟`,
    tr: `${item.topic.tr} konusundaki dersi oku. Önce karar ver: hangi alt görevler birlikte düşünülmeli, hangi sıra mantıklı, nerede bilinçli sadeleştirme yapabilirsin?`,
    ru: `Прочитай урок о ${item.topic.ru}. Сначала реши: какие части задачи связаны между собой, какой порядок логичен и где можно осознанно упростить?`,
    ckb: `وانەکە دەربارەی ${item.topic.ckb} بخوێنەوە. سەرەتا بڕیار بدە: کام بەشەکان پێکەوە پەیوەستن، کام ڕیزبەندی واتادارە، و لە کوێ دەتوانیت بە ئاگایی سادە بکەیت؟`,
    kmr: `Dersa li ser ${item.topic.kmr} bixwîne. Pêşî biryar bide: kîjan beş bi hev ve girêdayî ne, kîjan rêz watedar e, û li ku derê dikarî bi hişyarî hêsan bikî?`,
    pl: `Przeczytaj lekcję o ${item.topic.pl}. Najpierw zdecyduj: które części zadania należą do siebie, jaka kolejność ma sens i gdzie wolno świadomie uprościć?`,
    ro: `Citește lecția despre ${item.topic.ro}. Decide mai întâi: ce părți ale sarcinii se leagă între ele, ce ordine are sens și unde poți simplifica deliberat?`,
    sq: `Lexoje mësimin për ${item.topic.sq}. Së pari vendos: cilat pjesë të detyrës lidhen mes tyre, cila renditje ka kuptim dhe ku mund të thjeshtosh me vetëdije?`
  };
}

function languageInstruction(item) {
  return {
    de: `Oeffne den verlinkten Grammatikpunkt. Waehle daraus ein Werkzeug, das bei ${item.topic.de} wirklich hilft: Verdichtung, Distanzierung, Registerwechsel oder klare Leserfuehrung.`,
    en: `Open the linked grammar point. Choose one tool that genuinely helps with ${item.topic.en}: compression, distance, register shift, or clear reader guidance.`,
    fa: `نکته گرامری لینک‌شده را باز کن. از آن یک ابزار انتخاب کن که واقعاً برای ${item.topic.fa} کمک کند: فشرده‌سازی، حفظ فاصله تحلیلی، تغییر سطح زبان، یا هدایت روشن خواننده.`,
    ar: `افتح نقطة القواعد المرتبطة. اختر أداة تساعد فعلًا في ${item.topic.ar}: التكثيف، المسافة التحليلية، تغيير السجل، أو توجيه القارئ بوضوح.`,
    tr: `Bağlantılı dil bilgisi noktasını aç. ${item.topic.tr} için gerçekten işe yarayan bir araç seç: yoğunlaştırma, analitik mesafe, register değişimi ya da okuru açık yönlendirme.`,
    ru: `Открой связанный грамматический пункт. Выбери инструмент, который действительно помогает в теме ${item.topic.ru}: сжатие, аналитическая дистанция, смена регистра или ясное ведение читателя.`,
    ckb: `خاڵی ڕێزمانی بەستەرکراو بکەرەوە. ئامرازێک هەڵبژێرە کە بەڕاستی بۆ ${item.topic.ckb} یارمەتیدەرە: چڕکردنەوە، دووریی شیکاری، گۆڕینی تۆنی زمان، یان ڕێنمایی ڕوونی خوێنەر.`,
    kmr: `Xala rêzimanê ya girêdayî veke. Amûrek hilbijêre ku bi rastî ji bo ${item.topic.kmr} alîkar e: komkirin, dûrbûna analîtîk, guherîna registerê an rêberiya zelal a xwendevan.`,
    pl: `Otwórz podlinkowany punkt gramatyczny. Wybierz narzędzie naprawdę pomocne przy ${item.topic.pl}: kondensację, dystans analityczny, zmianę rejestru albo jasne prowadzenie czytelnika.`,
    ro: `Deschide punctul de gramatică legat. Alege un instrument care ajută real la ${item.topic.ro}: condensare, distanță analitică, schimbare de registru sau ghidarea clară a cititorului.`,
    sq: `Hap pikën gramatikore të lidhur. Zgjidh një mjet që ndihmon vërtet për ${item.topic.sq}: ngjeshje, distancë analitike, ndryshim regjistri ose udhëzim të qartë për lexuesin.`
  };
}

function targetInstruction(item) {
  return {
    de: `Bearbeite die verlinkte Ressource zu ${item.topic.de}. Uebernimm nicht den Text, sondern die Strategie: Wie wird Komplexitaet geordnet und adressatengerecht formuliert?`,
    en: `Work through the linked resource on ${item.topic.en}. Do not copy the text; take the strategy: how is complexity ordered and formulated for the audience?`,
    fa: `منبع لینک‌شده درباره ${item.topic.fa} را انجام بده. متن آن را کپی نکن؛ راهبردش را بگیر: پیچیدگی چگونه مرتب می‌شود و متناسب با مخاطب بیان می‌شود؟`,
    ar: `اعمل على المورد المرتبط عن ${item.topic.ar}. لا تنسخ النص، بل خذ الاستراتيجية: كيف يُنظّم التعقيد ويُصاغ بما يناسب المخاطب؟`,
    tr: `${item.topic.tr} konusundaki bağlantılı kaynağı çalış. Metni kopyalama; stratejiyi al: karmaşıklık nasıl düzenleniyor ve hedef kitleye uygun ifade ediliyor?`,
    ru: `Проработай связанный ресурс о ${item.topic.ru}. Не копируй текст, а возьми стратегию: как сложность упорядочивается и формулируется для адресата?`,
    ckb: `سەرچاوەی بەستەرکراو دەربارەی ${item.topic.ckb} کاربکە. دەقەکە کۆپی مەکە؛ ستراتیژەکەی وەربگرە: ئاڵۆزی چۆن ڕێکدەخرێت و بۆ وەرگر بەگونجاوی دەردەبڕدرێت؟`,
    kmr: `Çavkaniya girêdayî li ser ${item.topic.kmr} bixebitîne. Nivîsê kopî neke; stratejiyê bigire: alozi çawa tê rêzkirin û ji bo muxatebê çawa tê gotin?`,
    pl: `Przerób podlinkowany materiał o ${item.topic.pl}. Nie kopiuj tekstu, tylko przejmij strategię: jak porządkuje się złożoność i formułuje ją dla odbiorcy?`,
    ro: `Lucrează resursa legată despre ${item.topic.ro}. Nu copia textul; preia strategia: cum este ordonată complexitatea și formulată pentru destinatar?`,
    sq: `Puno me burimin e lidhur për ${item.topic.sq}. Mos e kopjo tekstin; merre strategjinë: si renditet kompleksiteti dhe si formulohet për marrësin?`
  };
}

function portfolioInstruction(item) {
  return {
    de: `Lege fuer ${item.topic.de} ein eigenes Mini-Muster an: eine starke Einleitung, eine praezise Ueberleitung und einen Satz, der Grenzen oder Unsicherheit sauber markiert.`,
    en: `Create your own mini-pattern for ${item.topic.en}: a strong opening, a precise transition, and one sentence that marks limits or uncertainty cleanly.`,
    fa: `برای ${item.topic.fa} یک الگوی کوچک شخصی بساز: یک شروع قوی، یک گذار دقیق، و یک جمله که مرزها یا ابهام را روشن و تمیز نشان بدهد.`,
    ar: `أنشئ لنفسك نموذجًا صغيرًا لـ ${item.topic.ar}: بداية قوية، انتقال دقيق، وجملة توضّح الحدود أو عدم اليقين بوضوح.`,
    tr: `${item.topic.tr} için kendine küçük bir kalıp oluştur: güçlü bir giriş, net bir geçiş ve sınırları ya da belirsizliği temiz işaretleyen bir cümle.`,
    ru: `Создай для ${item.topic.ru} свой мини-шаблон: сильное вступление, точный переход и одно предложение, которое ясно обозначает границы или неопределенность.`,
    ckb: `بۆ ${item.topic.ckb} نموونەیەکی بچووکی خۆت دروست بکە: دەستپێکێکی بەهێز، گواستنەوەیەکی ورد، و ڕستەیەک کە سنوور یان نادڵنیایی بە ڕوونی نیشان بدات.`,
    kmr: `Ji bo ${item.topic.kmr} nimûneyeke biçûk a xwe çêbike: destpêkeke bihêz, derbasbûneke daqîq, û hevokek ku sînor an nediyarbûnê bi zelalî nîşan dide.`,
    pl: `Stwórz własny miniwzorzec do ${item.topic.pl}: mocny wstęp, precyzyjne przejście i jedno zdanie, które jasno zaznacza granice lub niepewność.`,
    ro: `Creează un mini-model propriu pentru ${item.topic.ro}: o introducere puternică, o tranziție precisă și o propoziție care marchează clar limitele sau incertitudinea.`,
    sq: `Krijo një mini-model tëndin për ${item.topic.sq}: një hyrje të fortë, një kalim të saktë dhe një fjali që shënon qartë kufijtë ose pasigurinë.`
  };
}

function reviewInstruction(item) {
  return {
    de: `Pruefe dein Ergebnis zu ${item.topic.de}: Ist die Struktur sichtbar, der Ton passend und die Aussage nicht ueberdehnt? Markiere eine Stelle, die du beim naechsten Mal frueher planen musst.`,
    en: `Check your result on ${item.topic.en}: is the structure visible, the tone appropriate, and the claim not overstretched? Mark one point you need to plan earlier next time.`,
    fa: `نتیجه خودت درباره ${item.topic.fa} را بررسی کن: آیا ساختار دیده می‌شود، لحن مناسب است، و ادعا بیش از حد کشیده نشده؟ یک نقطه را مشخص کن که دفعه بعد باید زودتر برایش برنامه‌ریزی کنی.`,
    ar: `افحص نتيجتك في ${item.topic.ar}: هل البنية واضحة، والنبرة مناسبة، والادعاء غير مبالغ فيه؟ علّم نقطة يجب أن تخطط لها مبكرًا في المرة القادمة.`,
    tr: `${item.topic.tr} konusundaki sonucunu kontrol et: yapı görünür mü, ton uygun mu, iddia fazla genişletilmemiş mi? Bir dahaki sefere daha erken planlaman gereken bir noktayı işaretle.`,
    ru: `Проверь результат по теме ${item.topic.ru}: видна ли структура, уместен ли тон и не слишком ли растянуто утверждение? Отметь место, которое в следующий раз нужно планировать раньше.`,
    ckb: `ئەنجامت دەربارەی ${item.topic.ckb} بپشکنە: ئایا پێکهاتەکە دیارە، تۆنەکە گونجاوە، و بانگەشەکە زیادەڕۆیی نییە؟ شوێنێک دیاری بکە کە جارێکی داهاتوو دەبێت زووتر پلانی بۆ دابنێیت.`,
    kmr: `Encama xwe li ser ${item.topic.kmr} kontrol bike: avahî xuya ye, ton guncav e, û îdia zêde nehatiye firehkirin? Xalek nîşan bike ku cara din divê zûtir were plansazkirin.`,
    pl: `Sprawdź wynik dotyczący ${item.topic.pl}: czy struktura jest widoczna, ton właściwy, a teza nie jest przeciągnięta? Zaznacz miejsce, które następnym razem trzeba zaplanować wcześniej.`,
    ro: `Verifică rezultatul despre ${item.topic.ro}: este vizibilă structura, tonul este potrivit, iar afirmația nu este exagerată? Marchează un punct pe care data viitoare trebuie să-l planifici mai devreme.`,
    sq: `Kontrollo rezultatin për ${item.topic.sq}: a është e dukshme struktura, a është toni i përshtatshëm dhe a nuk është tepruar pretendimi? Shëno një pikë që herën tjetër duhet ta planifikosh më herët.`
  };
}

for (const item of items) {
  const lesson = lessons.find(l => l.slug === item.slug);
  if (!lesson) {
    throw new Error(`Lesson not found: ${item.slug}`);
  }

  lesson.activityBlocks = [
    createActivity('read', 'orient', orientInstruction(item), 'none', null, 7, 10),
    createActivity('grammar', 'language', languageInstruction(item), 'grammar-topic', item.grammar, 8, 20),
    createActivity(item.targetKind, 'target', targetInstruction(item), item.targetType, item.targetSlug, 10, 30),
    createActivity('practice', 'portfolio', portfolioInstruction(item), 'none', null, 8, 40),
    createActivity('review', 'review', reviewInstruction(item), 'none', null, 7, 50)
  ];
}

fs.writeFileSync(file, JSON.stringify(data, null, 2) + '\n', 'utf8');
console.log(`Updated ${items.length} C2 Module 12 lessons with ${items.length * 5} activity blocks.`);

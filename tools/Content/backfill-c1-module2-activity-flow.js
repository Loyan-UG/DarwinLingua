const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c1-souveraen-in-studium-beruf-und-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titleTexts = {
  orient: {
    de: 'Argumentationsziel klaeren',
    en: 'Clarify the argument goal',
    fa: 'هدف استدلال را روشن کن',
    ar: 'وضّح هدف الحجة',
    tr: 'Argüman hedefini netleştir',
    ru: 'Проясни цель аргумента',
    ckb: 'ئامانجی ئارگیومێنتەکە ڕوون بکەوە',
    kmr: 'Armanca argumanê zelal bike',
    pl: 'Wyjaśnij cel argumentacji',
    ro: 'Clarifică scopul argumentării',
    sq: 'Qartëso qëllimin e argumentimit'
  },
  grammar: {
    de: 'Sprachmittel pruefen',
    en: 'Check the language tools',
    fa: 'ابزارهای زبانی را بررسی کن',
    ar: 'افحص الأدوات اللغوية',
    tr: 'Dil araçlarını kontrol et',
    ru: 'Проверь языковые средства',
    ckb: 'ئامرازە زمانییەکان بپشکنە',
    kmr: 'Amûrên zimanî kontrol bike',
    pl: 'Sprawdź środki językowe',
    ro: 'Verifică mijloacele lingvistice',
    sq: 'Kontrollo mjetet gjuhësore'
  },
  material: {
    de: 'Material gezielt nutzen',
    en: 'Use the material deliberately',
    fa: 'از محتوای لینک‌شده هدفمند استفاده کن',
    ar: 'استخدم المادة بهدف واضح',
    tr: 'Malzemeyi amaçlı kullan',
    ru: 'Используй материал целенаправленно',
    ckb: 'ماتریاڵەکە بە ئامانج بەکاربهێنە',
    kmr: 'Materyalê bi armanc bi kar bîne',
    pl: 'Wykorzystaj materiał celowo',
    ro: 'Folosește materialul cu un scop clar',
    sq: 'Përdore materialin me qëllim të qartë'
  },
  write: {
    de: 'Eigenen Baustein schreiben',
    en: 'Write your own building block',
    fa: 'یک بخش کوتاه از متن خودت بنویس',
    ar: 'اكتب جزءًا قصيرًا من نصك',
    tr: 'Kendi kısa metin parçasını yaz',
    ru: 'Напиши собственный небольшой фрагмент',
    ckb: 'بەشێکی کورتی خۆت بنووسە',
    kmr: 'Parçeyeke kurt a xwe binivîse',
    pl: 'Napisz własny krótki fragment',
    ro: 'Scrie propriul fragment scurt',
    sq: 'Shkruaj një pjesë të shkurtër vetë'
  },
  review: {
    de: 'Qualitaet pruefen',
    en: 'Check the quality',
    fa: 'کیفیت پاسخ را بررسی کن',
    ar: 'تحقّق من جودة الإجابة',
    tr: 'Cevabın kalitesini kontrol et',
    ru: 'Проверь качество ответа',
    ckb: 'کوالیتی وەڵامەکە بپشکنە',
    kmr: 'Qalîteya bersivê kontrol bike',
    pl: 'Sprawdź jakość odpowiedzi',
    ro: 'Verifică calitatea răspunsului',
    sq: 'Kontrollo cilësinë e përgjigjes'
  }
};

function tr(map) {
  return langs.map(language => ({ language, text: map[language] }));
}

function title(key) {
  return {
    title: titleTexts[key].de,
    titleTranslations: tr(titleTexts[key])
  };
}

const items = [
  {
    slug: 'c1-these-position-und-einschraenkung',
    topic: {
      de: 'These, Position und Einschraenkung',
      en: 'thesis, position, and limitation',
      fa: 'تز، موضع و محدودکردن ادعا',
      ar: 'الأطروحة والموقف وتقييد الادعاء',
      tr: 'tez, tutum ve iddiayı sınırlama',
      ru: 'тезис, позиция и ограничение утверждения',
      ckb: 'تێز، هەڵوێست و سنووردارکردنی بانگەشە',
      kmr: 'tez, helwest û sînordarkirina îddîayê',
      pl: 'teza, stanowisko i ograniczenie twierdzenia',
      ro: 'teză, poziție și limitarea afirmației',
      sq: 'teza, qëndrimi dhe kufizimi i pretendimit'
    },
    focus: {
      de: 'eine klare Position mit einer sinnvollen Einschraenkung',
      en: 'a clear position with a meaningful limitation',
      fa: 'یک موضع روشن همراه با محدودیتی منطقی',
      ar: 'موقفًا واضحًا مع تقييد منطقي',
      tr: 'anlamlı bir sınırlamayla birlikte açık bir tutumu',
      ru: 'ясную позицию с разумным ограничением',
      ckb: 'هەڵوێستێکی ڕوون لەگەڵ سنوورێکی مانادار',
      kmr: 'helwesteke zelal bi sînorekî watedar',
      pl: 'jasne stanowisko z sensownym ograniczeniem',
      ro: 'o poziție clară cu o limitare logică',
      sq: 'një qëndrim të qartë me një kufizim kuptimplotë'
    },
    grammar: ['c1-academic-argument-grammar', 'akademische Argumentationsgrammatik'],
    target: ['exam-prep-unit', 'c1-akademische-argumentation-strukturieren', 'akademische Argumentation'],
    practice: {
      de: 'Formuliere zu einem Thema eine These, eine vorsichtige Einschraenkung und einen Satz, der zeigt, wofuer deine Position gilt.',
      en: 'For one topic, write a thesis, a careful limitation, and one sentence that shows where your position applies.',
      fa: 'برای یک موضوع، یک تز، یک محدودیت محتاطانه و یک جمله بنویس که نشان دهد موضع تو در چه محدوده‌ای معتبر است.',
      ar: 'اكتب عن موضوع واحد أطروحة وتقييدًا حذرًا وجملة توضّح في أي نطاق يكون موقفك صالحًا.',
      tr: 'Bir konu için bir tez, dikkatli bir sınırlama ve tutumunun hangi çerçevede geçerli olduğunu gösteren bir cümle yaz.',
      ru: 'Для одной темы сформулируй тезис, осторожное ограничение и предложение, показывающее, в каких рамках действует твоя позиция.',
      ckb: 'بۆ یەک بابەت تێزێک، سنوورێکی بەهۆش و ڕستەیەک بنووسە کە پیشان بدات هەڵوێستەکەت لە چوارچێوەی کامدا دروستە.',
      kmr: 'Ji bo mijarekê tezek, sînorekî baldar û hevokekê binivîse ku nîşan bide helwesta te di kîjan çarçoveyê de derbasdar e.',
      pl: 'Do jednego tematu napisz tezę, ostrożne ograniczenie i zdanie pokazujące, w jakim zakresie twoje stanowisko jest trafne.',
      ro: 'Pentru o temă, formulează o teză, o limitare prudentă și o propoziție care arată în ce cadru este valabilă poziția ta.',
      sq: 'Për një temë, shkruaj një tezë, një kufizim të kujdesshëm dhe një fjali që tregon ku vlen qëndrimi yt.'
    },
    review: {
      de: 'Pruefe, ob deine These nicht nur stark klingt, sondern auch begrenzt, begruendet und fair formuliert ist.',
      en: 'Check whether your thesis not only sounds strong, but is also limited, justified, and fair.',
      fa: 'بررسی کن که تز تو فقط قوی به نظر نرسد، بلکه محدود، مستدل و منصفانه هم بیان شده باشد.',
      ar: 'تحقّق مما إذا كانت أطروحتك لا تبدو قوية فقط، بل هي أيضًا محددة ومعللة ومنصفة.',
      tr: 'Tezinin yalnızca güçlü görünmediğini, aynı zamanda sınırlı, gerekçeli ve adil biçimde ifade edildiğini kontrol et.',
      ru: 'Проверь, не только ли твой тезис звучит убедительно, но и ограничен ли он, обоснован и справедливо сформулирован.',
      ckb: 'بپشکنە تێزەکەت تەنها بەهێز نەدەنێت، بەڵکو سنووردار، بە بەڵگە و دادپەروەرانەش فۆرمولە کرابێت.',
      kmr: 'Kontrol bike ka teza te tenê xurt nayê bihîstin, lê di heman demê de sînordar, bi sedem û adilane hatiye gotin.',
      pl: 'Sprawdź, czy twoja teza nie tylko brzmi mocno, ale jest też ograniczona, uzasadniona i sformułowana fair.',
      ro: 'Verifică dacă teza ta nu doar sună puternic, ci este și limitată, justificată și formulată echilibrat.',
      sq: 'Kontrollo nëse teza jote jo vetëm tingëllon e fortë, por është edhe e kufizuar, e arsyetuar dhe e drejtë.'
    }
  },
  {
    slug: 'c1-hedging-und-vorsichtige-sprache',
    topic: {
      de: 'Hedging und vorsichtige Sprache',
      en: 'hedging and cautious language',
      fa: 'بیان محتاطانه و نرم‌کردن ادعا',
      ar: 'التحوّط اللغوي وصياغة الادعاء بحذر',
      tr: 'temkinli ifade ve iddiayı yumuşatma',
      ru: 'хеджирование и осторожная формулировка',
      ckb: 'قسەکردنی بەهۆش و نەرمکردنی بانگەشە',
      kmr: 'axiftina baldar û nermkirina îddîayê',
      pl: 'ostrożne formułowanie i łagodzenie twierdzeń',
      ro: 'formularea prudentă și nuanțarea afirmațiilor',
      sq: 'formulimi i kujdesshëm dhe zbutja e pretendimit'
    },
    focus: {
      de: 'vorsichtige, aber nicht schwache Aussagen',
      en: 'careful but not weak statements',
      fa: 'جمله‌های محتاطانه‌ای که ضعیف یا بی‌اثر به نظر نرسند',
      ar: 'عبارات حذرة من دون أن تبدو ضعيفة',
      tr: 'zayıf görünmeyen temkinli ifadeleri',
      ru: 'осторожные, но не слабые высказывания',
      ckb: 'دەربڕینی بەهۆش کە لاواز نەدەنێت',
      kmr: 'gotinên baldar ku qels xuya nekin',
      pl: 'ostrożne, ale nie słabe wypowiedzi',
      ro: 'afirmații prudente, dar nu slabe',
      sq: 'deklarata të kujdesshme, por jo të dobëta'
    },
    grammar: ['c1-hedging-and-cautious-language', 'vorsichtige Sprache'],
    target: ['exam-prep-unit', 'c1-schriftliche-stellungnahme-fokussieren', 'schriftliche Stellungnahme'],
    practice: {
      de: 'Schreibe drei Varianten derselben Aussage: direkt, vorsichtig und zu vorsichtig. Entscheide, welche fuer C1 am ueberzeugendsten ist.',
      en: 'Write three versions of the same statement: direct, careful, and too careful. Decide which one is most convincing at C1.',
      fa: 'از یک ادعا سه نسخه بنویس: مستقیم، محتاطانه و بیش از حد محتاطانه. مشخص کن کدام نسخه در سطح C1 قانع‌کننده‌تر است.',
      ar: 'اكتب ثلاث صيغ للعبارة نفسها: مباشرة، حذرة، وحذرة أكثر من اللازم. حدّد أيها أكثر إقناعًا في C1.',
      tr: 'Aynı ifadenin üç biçimini yaz: doğrudan, temkinli ve fazla temkinli. C1 için hangisinin daha ikna edici olduğuna karar ver.',
      ru: 'Напиши три варианта одного утверждения: прямой, осторожный и чрезмерно осторожный. Реши, какой убедительнее на уровне C1.',
      ckb: 'هەمان بانگەشە بە سێ شێوە بنووسە: ڕاستەوخۆ، بەهۆش و زۆر بەهۆش. بڕیار بدە کامەیان لە C1 باوەڕپێهێنەرترە.',
      kmr: 'Heman gotinê bi sê awayan binivîse: rasterast, baldar û zêde baldar. Biryara xwe bide ka kîjan ji bo C1 qanehker e.',
      pl: 'Napisz trzy wersje tego samego stwierdzenia: bezpośrednią, ostrożną i zbyt ostrożną. Wybierz najbardziej przekonującą na C1.',
      ro: 'Scrie trei variante ale aceleiași afirmații: directă, prudentă și prea prudentă. Decide care este cea mai convingătoare la C1.',
      sq: 'Shkruaj tri versione të së njëjtës deklaratë: të drejtpërdrejtë, të kujdesshme dhe tepër të kujdesshme. Vendos cila bind më shumë në C1.'
    },
    review: {
      de: 'Streiche Formulierungen, die Unsicherheit statt Praezision erzeugen, und ersetze sie durch genaue Begrenzungen.',
      en: 'Remove formulations that create uncertainty instead of precision, and replace them with exact limitations.',
      fa: 'عبارت‌هایی را حذف کن که به جای دقت، تردید و ابهام ایجاد می‌کنند و آن‌ها را با محدودیت‌های دقیق جایگزین کن.',
      ar: 'احذف الصيغ التي تخلق ترددًا بدل الدقة، واستبدلها بقيود واضحة.',
      tr: 'Kesinlik yerine belirsizlik yaratan ifadeleri çıkar ve yerlerine net sınırlamalar koy.',
      ru: 'Убери формулировки, которые создают неопределенность вместо точности, и замени их точными ограничениями.',
      ckb: 'ئەو دەربڕینانە بسڕەوە کە لەبری وردی دوودڵی دروست دەکەن، و بە سنووری ورد جێیان بگرەوە.',
      kmr: 'Ew gotinên ku li şûna hûrguliyê dudilî çêdikin jê bibe û bi sînorên zelal biguherîne.',
      pl: 'Usuń sformułowania, które zamiast precyzji tworzą niepewność, i zastąp je dokładnymi ograniczeniami.',
      ro: 'Elimină formulările care creează nesiguranță în loc de precizie și înlocuiește-le cu limitări exacte.',
      sq: 'Hiq formulimet që krijojnë pasiguri në vend të saktësisë dhe zëvendësoji me kufizime të qarta.'
    }
  },
  {
    slug: 'c1-einwaende-aufgreifen-und-entkraeften',
    topic: {
      de: 'Einwaende aufgreifen und entkraeften',
      en: 'taking up and weakening objections',
      fa: 'پرداختن به ایرادها و کم‌اثر کردن آن‌ها',
      ar: 'تناول الاعتراضات وإضعاف أثرها',
      tr: 'itirazları ele alma ve etkisini azaltma',
      ru: 'учет и ослабление возражений',
      ckb: 'وەرگرتنی ڕەخنەکان و کەمکردنەوەی کاریگەرییان',
      kmr: 'girtina îtirazan û kêmkirina bandora wan',
      pl: 'podejmowanie i osłabianie zastrzeżeń',
      ro: 'preluarea și diminuarea obiecțiilor',
      sq: 'marrja parasysh dhe dobësimi i kundërshtimeve'
    },
    focus: {
      de: 'einen Einwand fair aufnehmen und sachlich entkraeften',
      en: 'taking an objection seriously and weakening it factually',
      fa: 'یک ایراد را منصفانه جدی گرفتن و با دلیل اثرش را کم کردن',
      ar: 'أخذ الاعتراض بجدية ثم إضعافه بحجج موضوعية',
      tr: 'bir itirazı ciddiye alıp nesnel biçimde zayıflatmayı',
      ru: 'серьезно принять возражение и предметно ослабить его',
      ckb: 'ڕەخنەیەک بە دادپەروەری وەرگرتن و بە بەڵگە کاریگەرییەکەی کەمکردنەوە',
      kmr: 'îtirazek bi awayekî adil girtin û bi sedeman qelskirin',
      pl: 'uczciwe przyjęcie zastrzeżenia i rzeczowe osłabienie go',
      ro: 'preluarea corectă a unei obiecții și diminuarea ei prin argumente',
      sq: 'pranimin e drejtë të një kundërshtimi dhe dobësimin e tij me arsye'
    },
    grammar: ['c1-concession-structures', 'Konzessionsstrukturen'],
    target: ['roleplay', 'c1-einen-einwand-im-kolloquium-aufgreifen', 'Einwand im Kolloquium'],
    practice: {
      de: 'Schreibe eine Mini-Replik: zuerst den Einwand anerkennen, dann seine Grenze zeigen, danach zur eigenen Position zurueckfuehren.',
      en: 'Write a mini response: first acknowledge the objection, then show its limit, and finally return to your own position.',
      fa: 'یک پاسخ کوتاه بنویس: اول ایراد را بپذیر، بعد محدوده اعتبارش را نشان بده، سپس به موضع خودت برگرد.',
      ar: 'اكتب ردًا قصيرًا: اعترف أولًا بالاعتراض، ثم بيّن حدوده، وبعد ذلك عُد إلى موقفك.',
      tr: 'Kısa bir karşılık yaz: önce itirazı kabul et, sonra sınırını göster, ardından kendi tutumuna dön.',
      ru: 'Напиши короткий ответ: сначала признай возражение, затем покажи его пределы, потом вернись к своей позиции.',
      ckb: 'وەڵامێکی کورت بنووسە: سەرەتا ڕەخنەکە بناسە، پاشان سنوورەکەی پیشان بدە، دواتر بگەڕێوە بۆ هەڵوێستی خۆت.',
      kmr: 'Bersiveke kurt binivîse: pêşî îtirazê nas bike, paşê sînorê wê nîşan bide, dûv re vegere helwesta xwe.',
      pl: 'Napisz krótką replikę: najpierw uznaj zastrzeżenie, potem pokaż jego granicę, a następnie wróć do własnego stanowiska.',
      ro: 'Scrie o replică scurtă: mai întâi recunoaște obiecția, apoi arată-i limita, după aceea revino la poziția ta.',
      sq: 'Shkruaj një përgjigje të shkurtër: prano fillimisht kundërshtimin, pastaj trego kufirin e tij dhe kthehu te qëndrimi yt.'
    },
    review: {
      de: 'Pruefe, ob du den Einwand ernst genommen hast, ohne deine eigene Argumentation unnoetig zu schwaechen.',
      en: 'Check whether you took the objection seriously without unnecessarily weakening your own argument.',
      fa: 'بررسی کن که ایراد را جدی گرفته‌ای، بدون اینکه استدلال خودت را بی‌دلیل ضعیف کنی.',
      ar: 'تحقّق مما إذا كنت أخذت الاعتراض بجدية من دون أن تضعف حجتك بلا داعٍ.',
      tr: 'İtirazı ciddiye alırken kendi argümanını gereksiz yere zayıflatmadığını kontrol et.',
      ru: 'Проверь, принял ли ты возражение серьезно, не ослабив при этом свою аргументацию без необходимости.',
      ckb: 'بپشکنە ئایا ڕەخنەکەت بە جدی وەرگرتووە، بەبێ ئەوەی ئارگیومێنتی خۆت بێهۆ لاواز بکەیت.',
      kmr: 'Kontrol bike ka te îtirazê bi ciddî girt, bêyî ku argumana xwe bêhewce qels bikî.',
      pl: 'Sprawdź, czy potraktowałeś zastrzeżenie poważnie, nie osłabiając niepotrzebnie własnej argumentacji.',
      ro: 'Verifică dacă ai tratat obiecția serios fără să îți slăbești inutil argumentația.',
      sq: 'Kontrollo nëse e more seriozisht kundërshtimin pa e dobësuar pa nevojë argumentin tënd.'
    }
  },
  {
    slug: 'c1-konzessionen-und-trotzdem-logik',
    topic: {
      de: 'Konzessionen und Trotzdem-Logik',
      en: 'concessions and nevertheless logic',
      fa: 'امتیاز دادن به نظر مقابل و منطق «با این حال»',
      ar: 'التنازل الجزئي ومنطق «مع ذلك»',
      tr: 'kısmi kabul ve “buna rağmen” mantığı',
      ru: 'уступки и логика «тем не менее»',
      ckb: 'دانپێدان بە بەشێک و لۆجیکی «لەگەڵ ئەوەشدا»',
      kmr: 'qebûlkirina beşek û mantiqa “tevî vê yekê”',
      pl: 'ustępstwa i logika „mimo to”',
      ro: 'concesii și logica „totuși”',
      sq: 'lëshime të pjesshme dhe logjika “megjithatë”'
    },
    focus: {
      de: 'eine Gegenposition anerkennen und trotzdem klar weiterargumentieren',
      en: 'acknowledging an opposing view while still arguing clearly',
      fa: 'پذیرفتن بخشی از نظر مقابل و با این حال ادامه دادن استدلال خودت به شکل روشن',
      ar: 'الاعتراف بجزء من الرأي المقابل مع مواصلة الحجة بوضوح',
      tr: 'karşı görüşü kısmen kabul edip yine de açık biçimde tartışmayı sürdürmeyi',
      ru: 'признание противоположной позиции при сохранении ясной линии аргумента',
      ckb: 'دانپێدان بە بەشێک لە هەڵوێستی بەرامبەر و لەگەڵ ئەوەشدا بە ڕوونی بەردەوامبوون',
      kmr: 'qebûlkirina beşek ji helwesta dijber û tevî wê argumankirina zelal',
      pl: 'uznanie stanowiska przeciwnego i mimo to jasne prowadzenie argumentu',
      ro: 'recunoașterea poziției opuse și continuarea clară a argumentării',
      sq: 'pranimin e një qëndrimi të kundërt dhe vazhdimin e qartë të argumentit'
    },
    grammar: ['c1-concession-structures', 'Konzessionsstrukturen'],
    target: ['exam-prep-unit', 'c1-pruefungsdiskussion-mit-konzessionen-fuehren', 'Konzessionen in der Pruefungsdiskussion'],
    practice: {
      de: 'Baue zwei Saetze mit zwar, dennoch oder gleichwohl. Achte darauf, dass die zweite Aussage logisch staerker ist.',
      en: 'Build two sentences with zwar, dennoch, or gleichwohl. Make sure the second statement is logically stronger.',
      fa: 'دو جمله با zwar، dennoch یا gleichwohl بساز. دقت کن جمله دوم از نظر منطقی وزن بیشتری داشته باشد.',
      ar: 'كوّن جملتين باستخدام zwar أو dennoch أو gleichwohl. انتبه إلى أن تكون العبارة الثانية أقوى منطقيًا.',
      tr: 'Zwar, dennoch ya da gleichwohl ile iki cümle kur. İkinci ifadenin mantıksal olarak daha güçlü olmasına dikkat et.',
      ru: 'Составь два предложения с zwar, dennoch или gleichwohl. Следи, чтобы второе утверждение было логически сильнее.',
      ckb: 'دوو ڕستە بە zwar، dennoch یان gleichwohl دروست بکە. ئاگاداربە کە وتەی دووەم لە ڕووی لۆجیکەوە بەهێزتر بێت.',
      kmr: 'Bi zwar, dennoch an gleichwohl du hevokan çêbike. Bala xwe bide ku gotina duyem ji aliyê mantiqê ve xurtir be.',
      pl: 'Zbuduj dwa zdania z zwar, dennoch albo gleichwohl. Dopilnuj, aby drugie stwierdzenie było logicznie silniejsze.',
      ro: 'Construiește două propoziții cu zwar, dennoch sau gleichwohl. Ai grijă ca a doua afirmație să fie mai puternică logic.',
      sq: 'Ndërto dy fjali me zwar, dennoch ose gleichwohl. Kujdesu që pohimi i dytë të jetë më i fortë logjikisht.'
    },
    review: {
      de: 'Pruefe, ob deine Konzession wirklich zur Argumentation gehoert und nicht nur wie eine dekorative Floskel wirkt.',
      en: 'Check whether your concession truly belongs to the argument and is not just decorative wording.',
      fa: 'بررسی کن که امتیاز دادن به نظر مقابل واقعاً بخشی از استدلال است، نه فقط یک عبارت تزئینی.',
      ar: 'تحقّق مما إذا كان التنازل الجزئي جزءًا حقيقيًا من الحجة وليس مجرد عبارة شكلية.',
      tr: 'Kısmi kabulün gerçekten argümanın parçası olduğunu, yalnızca süslü bir ifade olmadığını kontrol et.',
      ru: 'Проверь, действительно ли уступка входит в аргументацию, а не является лишь декоративной фразой.',
      ckb: 'بپشکنە ئەو دانپێدانە بەڕاستی بەشێکە لە ئارگیومێنتەکە، نەک تەنها دەربڕینێکی جوانکاری.',
      kmr: 'Kontrol bike ka ew qebûlkirin rastî beşek ji argumanê ye, ne tenê peyveke xemilandî.',
      pl: 'Sprawdź, czy ustępstwo naprawdę należy do argumentacji, a nie jest tylko ozdobnym zwrotem.',
      ro: 'Verifică dacă această concesie aparține cu adevărat argumentării, nu este doar o formulare decorativă.',
      sq: 'Kontrollo nëse lëshimi është vërtet pjesë e argumentimit, jo thjesht një shprehje zbukuruese.'
    }
  },
  {
    slug: 'c1-kausalketten-in-formellen-texten',
    topic: {
      de: 'Kausalketten in formellen Texten',
      en: 'causal chains in formal texts',
      fa: 'زنجیره‌های علت و نتیجه در متن رسمی',
      ar: 'سلاسل السبب والنتيجة في النصوص الرسمية',
      tr: 'resmî metinlerde neden-sonuç zincirleri',
      ru: 'причинно-следственные цепочки в официальных текстах',
      ckb: 'زنجیرەی هۆکار و ئەنجام لە دەقی فەرمی',
      kmr: 'zincîreyên sedem û encam di nivîsên fermî de',
      pl: 'łańcuchy przyczynowo-skutkowe w tekstach formalnych',
      ro: 'lanțuri cauzale în texte formale',
      sq: 'zinxhirë shkak-pasojë në tekste formale'
    },
    focus: {
      de: 'mehrere Gruende und Folgen sauber verbinden',
      en: 'connecting several reasons and consequences cleanly',
      fa: 'وصل کردن چند دلیل و پیامد به شکلی منظم و قابل پیگیری',
      ar: 'ربط عدة أسباب ونتائج بطريقة منظمة وواضحة',
      tr: 'birkaç nedeni ve sonucu düzenli biçimde bağlamayı',
      ru: 'четко связывать несколько причин и последствий',
      ckb: 'بەستنی چەند هۆکار و ئەنجام بە شێوەیەکی ڕێک و ڕوون',
      kmr: 'girêdana çend sedem û encaman bi awayekî rêkûpêk',
      pl: 'czyste łączenie kilku powodów i skutków',
      ro: 'legarea ordonată a mai multor cauze și consecințe',
      sq: 'lidhjen e disa arsyeve dhe pasojave në mënyrë të rregullt'
    },
    grammar: ['c1-causal-chains-in-formal-writing', 'Kausalketten'],
    target: ['writing-template', 'c1-gutachtennahe-empfehlung-formulieren', 'gutachtennahe Empfehlung'],
    practice: {
      de: 'Schreibe eine Kette aus drei Schritten: Ursache, Zwischenfolge und Konsequenz. Vermeide, alles mit weil zu verbinden.',
      en: 'Write a three-step chain: cause, intermediate result, and consequence. Avoid connecting everything with weil.',
      fa: 'یک زنجیره سه‌مرحله‌ای بنویس: علت، پیامد میانی و نتیجه نهایی. همه چیز را فقط با weil به هم وصل نکن.',
      ar: 'اكتب سلسلة من ثلاث مراحل: سبب، نتيجة وسيطة، ونتيجة نهائية. تجنّب ربط كل شيء بـ weil فقط.',
      tr: 'Üç aşamalı bir zincir yaz: neden, ara sonuç ve sonuç. Her şeyi yalnızca weil ile bağlamaktan kaçın.',
      ru: 'Напиши цепочку из трех шагов: причина, промежуточный результат и последствие. Не связывай всё только через weil.',
      ckb: 'زنجیرەیەکی سێ قۆناغی بنووسە: هۆکار، ئەنجامی ناوەندی و دەرئەنجام. هەموو شت بە weil مەبەستەوە.',
      kmr: 'Zincîreyeke sê gavî binivîse: sedem, encama navîn û encam. Hemû tiştan tenê bi weil negirêde.',
      pl: 'Napisz łańcuch z trzech kroków: przyczyna, skutek pośredni i konsekwencja. Nie łącz wszystkiego samym weil.',
      ro: 'Scrie un lanț în trei pași: cauză, efect intermediar și consecință. Evită să legi totul doar cu weil.',
      sq: 'Shkruaj një zinxhir me tri hapa: shkak, pasojë e ndërmjetme dhe përfundim. Mos e lidh gjithçka vetëm me weil.'
    },
    review: {
      de: 'Pruefe, ob jede Folge wirklich aus dem vorherigen Satz entsteht oder ob ein Zwischenschritt fehlt.',
      en: 'Check whether each consequence really follows from the previous sentence, or whether an intermediate step is missing.',
      fa: 'بررسی کن که هر پیامد واقعاً از جمله قبلی نتیجه می‌شود یا یک مرحله میانی جا افتاده است.',
      ar: 'تحقّق مما إذا كانت كل نتيجة تنشأ فعلًا من الجملة السابقة أم أن هناك خطوة وسيطة ناقصة.',
      tr: 'Her sonucun gerçekten önceki cümleden çıktığını ya da arada eksik bir adım olup olmadığını kontrol et.',
      ru: 'Проверь, действительно ли каждое последствие вытекает из предыдущего предложения или пропущен промежуточный шаг.',
      ckb: 'بپشکنە هەر ئەنجامێک بەڕاستی لە ڕستەی پێشووەوە دێت یان هەنگاوێکی ناوەندی ونە.',
      kmr: 'Kontrol bike ka her encam rastî ji hevoka berê derdikeve an gaveke navîn kêm e.',
      pl: 'Sprawdź, czy każdy skutek naprawdę wynika z poprzedniego zdania, czy brakuje kroku pośredniego.',
      ro: 'Verifică dacă fiecare consecință rezultă cu adevărat din propoziția anterioară sau lipsește un pas intermediar.',
      sq: 'Kontrollo nëse çdo pasojë del vërtet nga fjalia e mëparshme apo mungon një hap ndërmjetës.'
    }
  },
  {
    slug: 'c1-nuance-und-begrenzung-ausdruecken',
    topic: {
      de: 'Nuance und Begrenzung ausdruecken',
      en: 'expressing nuance and limitation',
      fa: 'بیان ظرافت معنا و محدودیت ادعا',
      ar: 'التعبير عن الدقة المعنوية وحدود الادعاء',
      tr: 'anlam inceliği ve iddianın sınırını ifade etme',
      ru: 'выражение нюанса и ограничения',
      ckb: 'دەربڕینی وردیی مانا و سنووری بانگەشە',
      kmr: 'derbirîna hûrguliya wateyê û sînorê îddîayê',
      pl: 'wyrażanie niuansu i ograniczenia',
      ro: 'exprimarea nuanței și a limitării',
      sq: 'shprehja e nuancës dhe e kufizimit'
    },
    focus: {
      de: 'Bedeutung praezise eingrenzen, ohne unklar zu werden',
      en: 'limiting meaning precisely without becoming unclear',
      fa: 'دقیق محدود کردن معنا بدون اینکه جمله مبهم شود',
      ar: 'تحديد المعنى بدقة من دون أن يصبح غامضًا',
      tr: 'anlamı belirsizleştirmeden kesin biçimde sınırlamayı',
      ru: 'точно ограничивать смысл, не делая его неясным',
      ckb: 'سنووردارکردنی مانا بە وردی بەبێ ئەوەی ناڕوون بێت',
      kmr: 'sînordarkirina wateyê bi hûrgulî bêyî ku nezelal bibe',
      pl: 'precyzyjne zawężenie znaczenia bez utraty jasności',
      ro: 'limitarea precisă a sensului fără a deveni neclar',
      sq: 'kufizimin e saktë të kuptimit pa u bërë i paqartë'
    },
    grammar: ['c1-expressing-nuance-and-limitation', 'Nuance und Begrenzung'],
    target: ['exam-prep-unit', 'c1-zusammenfassung-und-bewertung-trennen', 'Zusammenfassung und Bewertung'],
    practice: {
      de: 'Nimm eine zu absolute Aussage und begrenze sie mit meist, tendenziell, unter bestimmten Bedingungen oder im vorliegenden Kontext.',
      en: 'Take an overly absolute statement and limit it with meist, tendenziell, unter bestimmten Bedingungen, or im vorliegenden Kontext.',
      fa: 'یک ادعای بیش از حد مطلق را بردار و با عبارت‌هایی مثل meist، tendenziell، unter bestimmten Bedingungen یا im vorliegenden Kontext آن را دقیق‌تر محدود کن.',
      ar: 'خذ عبارة مطلقة أكثر من اللازم وقيّدها بتعابير مثل meist أو tendenziell أو unter bestimmten Bedingungen أو im vorliegenden Kontext.',
      tr: 'Fazla mutlak bir ifadeyi al ve meist, tendenziell, unter bestimmten Bedingungen ya da im vorliegenden Kontext gibi ifadelerle sınırla.',
      ru: 'Возьми слишком абсолютное утверждение и ограничь его выражениями meist, tendenziell, unter bestimmten Bedingungen или im vorliegenden Kontext.',
      ckb: 'بانگەشەیەکی زۆر ڕەها وەربگرە و بە meist، tendenziell، unter bestimmten Bedingungen یان im vorliegenden Kontext وردتر سنوورداری بکە.',
      kmr: 'Gotineke zêde mutleq hilde û bi meist, tendenziell, unter bestimmten Bedingungen an im vorliegenden Kontext wê hûrgulîtir sînordar bike.',
      pl: 'Weź zbyt absolutne stwierdzenie i ogranicz je za pomocą meist, tendenziell, unter bestimmten Bedingungen albo im vorliegenden Kontext.',
      ro: 'Ia o afirmație prea absolută și limiteaz-o cu meist, tendenziell, unter bestimmten Bedingungen sau im vorliegenden Kontext.',
      sq: 'Merr një pohim tepër absolut dhe kufizoje me meist, tendenziell, unter bestimmten Bedingungen ose im vorliegenden Kontext.'
    },
    review: {
      de: 'Pruefe, ob deine Begrenzung die Aussage praeziser macht und nicht einfach nur vorsichtiger klingen laesst.',
      en: 'Check whether your limitation makes the statement more precise, not merely more cautious.',
      fa: 'بررسی کن که محدودسازی تو جمله را دقیق‌تر کرده، نه اینکه فقط محتاطانه‌تر به نظر برسد.',
      ar: 'تحقّق مما إذا كان التقييد يجعل العبارة أدق، لا مجرد أكثر حذرًا.',
      tr: 'Sınırlamanın ifadeyi gerçekten daha kesin yaptığını, yalnızca daha temkinli göstermediğini kontrol et.',
      ru: 'Проверь, делает ли ограничение высказывание точнее, а не просто осторожнее.',
      ckb: 'بپشکنە سنووردارکردنەکەت وتەکە وردتر دەکات، نەک تەنها بەهۆشتر دەنێوێنێت.',
      kmr: 'Kontrol bike ka sînordarkirina te gotinê hûrgulîtir dike, ne tenê baldartir xuya dike.',
      pl: 'Sprawdź, czy ograniczenie czyni wypowiedź bardziej precyzyjną, a nie tylko ostrożniejszą.',
      ro: 'Verifică dacă limitarea face afirmația mai precisă, nu doar mai prudentă.',
      sq: 'Kontrollo nëse kufizimi e bën pohimin më të saktë, jo vetëm më të kujdesshëm.'
    }
  },
  {
    slug: 'c1-komplexe-vergleiche-formulieren',
    topic: {
      de: 'Komplexe Vergleiche formulieren',
      en: 'formulating complex comparisons',
      fa: 'ساختن مقایسه‌های پیچیده',
      ar: 'صياغة مقارنات معقّدة',
      tr: 'karmaşık karşılaştırmalar kurma',
      ru: 'формулирование сложных сравнений',
      ckb: 'فۆرمولەکردنی بەراوردی ئاڵۆز',
      kmr: 'formulekirina berawirdên aloz',
      pl: 'formułowanie złożonych porównań',
      ro: 'formularea comparațiilor complexe',
      sq: 'formulimi i krahasimeve komplekse'
    },
    focus: {
      de: 'Aehnlichkeiten, Unterschiede und Bewertungsmassstaebe trennen',
      en: 'separating similarities, differences, and criteria of evaluation',
      fa: 'جدا کردن شباهت‌ها، تفاوت‌ها و معیارهای ارزیابی',
      ar: 'فصل أوجه الشبه والاختلاف ومعايير التقييم',
      tr: 'benzerlikleri, farkları ve değerlendirme ölçütlerini ayırmayı',
      ru: 'разделять сходства, различия и критерии оценки',
      ckb: 'جیاکردنەوەی هاوشێوەیی، جیاوازی و پێوەری هەڵسەنگاندن',
      kmr: 'cuda kirina wekhevî, cudahî û pîvanên nirxandinê',
      pl: 'oddzielanie podobieństw, różnic i kryteriów oceny',
      ro: 'separarea asemănărilor, diferențelor și criteriilor de evaluare',
      sq: 'ndarjen e ngjashmërive, dallimeve dhe kritereve të vlerësimit'
    },
    grammar: ['c1-complex-comparison-structures', 'komplexe Vergleichsstrukturen'],
    target: ['writing-template', 'c1-daten-oder-grafik-schriftlich-auswerten', 'Daten oder Grafik schriftlich auswerten'],
    practice: {
      de: 'Vergleiche zwei Positionen nicht nur nach pro und contra, sondern nach Reichweite, Beleglage und praktischer Folge.',
      en: 'Compare two positions not only by pros and cons, but by scope, evidence, and practical consequence.',
      fa: 'دو موضع را فقط با موافق و مخالف مقایسه نکن؛ دامنه اعتبار، شواهد و پیامد عملی را هم جداگانه بررسی کن.',
      ar: 'قارن بين موقفين لا بحسب الإيجابيات والسلبيات فقط، بل بحسب النطاق والأدلة والنتيجة العملية.',
      tr: 'İki tutumu yalnızca artı-eksiyle değil; kapsam, kanıt durumu ve pratik sonuç açısından karşılaştır.',
      ru: 'Сравни две позиции не только по плюсам и минусам, но и по охвату, доказательности и практическим последствиям.',
      ckb: 'دوو هەڵوێست تەنها بە سود و زیان بەراورد مەکە؛ مەودا، بەڵگە و ئەنجامی کرداریش جیاواز بپشکنە.',
      kmr: 'Du helwestan ne tenê bi erênî û neyînî berawird bike; qada derbasbûnê, belge û encama pratîk jî binirxîne.',
      pl: 'Porównaj dwa stanowiska nie tylko według plusów i minusów, ale też według zakresu, dowodów i skutków praktycznych.',
      ro: 'Compară două poziții nu doar prin avantaje și dezavantaje, ci prin amploare, dovezi și consecințe practice.',
      sq: 'Krahaso dy qëndrime jo vetëm sipas pro dhe kundër, por edhe sipas shtrirjes, provave dhe pasojës praktike.'
    },
    review: {
      de: 'Pruefe, ob dein Vergleich einen gemeinsamen Massstab hat; ohne Massstab wirkt er wie eine blosse Aufzaehlung.',
      en: 'Check whether your comparison has a shared criterion; without one, it reads like a mere list.',
      fa: 'بررسی کن که مقایسه‌ات معیار مشترک دارد؛ بدون معیار، متن بیشتر شبیه فهرست‌کردن می‌شود.',
      ar: 'تحقّق مما إذا كانت المقارنة تعتمد معيارًا مشتركًا؛ من دونه تبدو كأنها مجرد تعداد.',
      tr: 'Karşılaştırmanın ortak bir ölçütü olup olmadığını kontrol et; ölçüt yoksa metin yalnızca liste gibi görünür.',
      ru: 'Проверь, есть ли у сравнения общий критерий; без него оно выглядит как простое перечисление.',
      ckb: 'بپشکنە بەراوردەکەت پێوەرێکی هاوبەشی هەیە؛ بەبێ پێوەر وەک ژماردنێکی سادە دەردەکەوێت.',
      kmr: 'Kontrol bike ka berawirda te pîvaneke hevpar heye; bê pîvan ew wek rêzkirineke sade xuya dike.',
      pl: 'Sprawdź, czy porównanie ma wspólne kryterium; bez niego wygląda jak zwykłe wyliczenie.',
      ro: 'Verifică dacă comparația are un criteriu comun; fără el pare doar o enumerare.',
      sq: 'Kontrollo nëse krahasimi ka një kriter të përbashkët; pa të duket si thjesht një listë.'
    }
  },
  {
    slug: 'c1-kontroverse-thesen-differenziert-verteidigen',
    topic: {
      de: 'Kontroverse Thesen differenziert verteidigen',
      en: 'defending controversial theses in a differentiated way',
      fa: 'دفاع دقیق و چندلایه از یک تز بحث‌برانگیز',
      ar: 'الدفاع عن أطروحة خلافية بطريقة متمايزة',
      tr: 'tartışmalı bir tezi ayrıntılı ve dengeli savunma',
      ru: 'дифференцированная защита спорного тезиса',
      ckb: 'بەرگریکردنی ورد و چەندلایەنە لە تێزێکی مشتومڕهەڵگر',
      kmr: 'parastina hûrgulî û piralî ya tezeke nakokî',
      pl: 'zróżnicowana obrona kontrowersyjnej tezy',
      ro: 'apărarea nuanțată a unei teze controversate',
      sq: 'mbrojtja e nuancuar e një teze të diskutueshme'
    },
    focus: {
      de: 'eine starke These vertreten, ohne Gegenargumente zu ignorieren',
      en: 'defending a strong thesis without ignoring counterarguments',
      fa: 'دفاع از یک تز قوی بدون نادیده گرفتن استدلال‌های مخالف',
      ar: 'الدفاع عن أطروحة قوية من دون تجاهل الحجج المضادة',
      tr: 'karşı argümanları yok saymadan güçlü bir tezi savunmayı',
      ru: 'защищать сильный тезис, не игнорируя контраргументы',
      ckb: 'بەرگری لە تێزێکی بەهێز بەبێ پشتگوێخستنی ئارگیومێنتی بەرامبەر',
      kmr: 'parastina tezeke xurt bêyî paşguhkirina argumanên dijber',
      pl: 'obronę mocnej tezy bez ignorowania kontrargumentów',
      ro: 'susținerea unei teze puternice fără ignorarea contraargumentelor',
      sq: 'mbrojtjen e një teze të fortë pa injoruar kundërargumentet'
    },
    grammar: ['c1-academic-argument-grammar', 'akademische Argumentationsgrammatik'],
    target: ['roleplay', 'c1-eine-kontroverse-these-differenziert-verteidigen', 'kontroverse These verteidigen'],
    practice: {
      de: 'Schreibe eine Verteidigung in vier Saetzen: These, Grund, Gegenargument, begrenzte Schlussfolgerung.',
      en: 'Write a defense in four sentences: thesis, reason, counterargument, limited conclusion.',
      fa: 'یک دفاع چهار جمله‌ای بنویس: تز، دلیل، استدلال مخالف، نتیجه‌گیری محدود و دقیق.',
      ar: 'اكتب دفاعًا في أربع جمل: أطروحة، سبب، حجة مضادة، واستنتاج محدود.',
      tr: 'Dört cümlelik bir savunma yaz: tez, gerekçe, karşı argüman, sınırlı sonuç.',
      ru: 'Напиши защиту в четырех предложениях: тезис, причина, контраргумент, ограниченный вывод.',
      ckb: 'بەرگرییەک بە چوار ڕستە بنووسە: تێز، هۆکار، ئارگیومێنتی بەرامبەر، دەرئەنجامی سنووردار.',
      kmr: 'Parastinekê bi çar hevokan binivîse: tez, sedem, argumana dijber, encama sînordar.',
      pl: 'Napisz obronę w czterech zdaniach: teza, powód, kontrargument, ograniczony wniosek.',
      ro: 'Scrie o apărare în patru propoziții: teză, motiv, contraargument, concluzie limitată.',
      sq: 'Shkruaj një mbrojtje në katër fjali: tezë, arsye, kundërargument, përfundim i kufizuar.'
    },
    review: {
      de: 'Pruefe, ob deine Verteidigung differenziert bleibt oder ob sie durch zu absolute Woerter angreifbar wird.',
      en: 'Check whether your defense remains differentiated or becomes vulnerable through overly absolute words.',
      fa: 'بررسی کن که دفاع تو همچنان دقیق و چندلایه مانده یا با واژه‌های بیش از حد مطلق، آسیب‌پذیر شده است.',
      ar: 'تحقّق مما إذا كان دفاعك لا يزال متمايزًا أم أنه أصبح سهل النقد بسبب ألفاظ مطلقة أكثر من اللازم.',
      tr: 'Savunmanın ayrıntılı kaldığını mı, yoksa fazla mutlak kelimeler yüzünden zayıfladığını mı kontrol et.',
      ru: 'Проверь, остается ли защита дифференцированной или становится уязвимой из-за слишком абсолютных слов.',
      ckb: 'بپشکنە بەرگرییەکەت هێشتا ورد و چەندلایەنەیە یان بە وشەی زۆر ڕەها هێرشبەر بووە.',
      kmr: 'Kontrol bike ka parastina te hîn jî hûrgulî û piralî ye, an bi peyvên zêde mutleq lawaz dibe.',
      pl: 'Sprawdź, czy obrona pozostaje zróżnicowana, czy przez zbyt absolutne słowa staje się łatwa do podważenia.',
      ro: 'Verifică dacă apărarea ta rămâne nuanțată sau devine vulnerabilă prin cuvinte prea absolute.',
      sq: 'Kontrollo nëse mbrojtja jote mbetet e nuancuar apo bëhet e cenueshme nga fjalë tepër absolute.'
    }
  },
  {
    slug: 'c1-schlussfolgerungen-mit-tragweite',
    topic: {
      de: 'Schlussfolgerungen mit Tragweite',
      en: 'conclusions with broader implications',
      fa: 'نتیجه‌گیری‌هایی که پیامد و دامنه دارند',
      ar: 'استنتاجات ذات أثر ونطاق أوسع',
      tr: 'etki alanı olan sonuçlar',
      ru: 'выводы с более широкими последствиями',
      ckb: 'دەرئەنجامەکان کە مەودا و کاریگەرییان هەیە',
      kmr: 'encamên ku bandor û qada wan heye',
      pl: 'wnioski o szerszych konsekwencjach',
      ro: 'concluzii cu implicații mai largi',
      sq: 'përfundime me pasoja më të gjera'
    },
    focus: {
      de: 'aus Argumenten eine angemessene, nicht ueberzogene Folgerung ziehen',
      en: 'drawing an appropriate, not exaggerated conclusion from arguments',
      fa: 'گرفتن نتیجه‌ای متناسب و نه اغراق‌آمیز از استدلال‌ها',
      ar: 'استخلاص نتيجة مناسبة وغير مبالغ فيها من الحجج',
      tr: 'argümanlardan uygun, abartısız bir sonuç çıkarmayı',
      ru: 'делать уместный, не преувеличенный вывод из аргументов',
      ckb: 'وەرگرتنی دەرئەنجامێکی گونجاو و نە زیادەڕۆیانە لە ئارگیومێنتەکان',
      kmr: 'derxistina encameke guncaw û ne zêde ji argumanan',
      pl: 'wyciąganie właściwego, nieprzesadzonego wniosku z argumentów',
      ro: 'formularea unei concluzii potrivite, nu exagerate, din argumente',
      sq: 'nxjerrjen e një përfundimi të përshtatshëm, jo të tepruar, nga argumentet'
    },
    grammar: ['c1-advanced-academic-connectors', 'akademische Konnektoren'],
    target: ['exam-prep-unit', 'c1-schlussfolgerungen-mit-tragweite-formulieren', 'Schlussfolgerungen mit Tragweite'],
    practice: {
      de: 'Formuliere eine Schlussfolgerung mit zwei Grenzen: Was folgt aus deinen Argumenten, und was folgt gerade nicht daraus?',
      en: 'Formulate a conclusion with two boundaries: what follows from your arguments, and what does not follow from them?',
      fa: 'یک نتیجه‌گیری با دو مرز بنویس: از استدلال تو چه چیزی نتیجه می‌شود و چه چیزی دقیقاً نتیجه نمی‌شود؟',
      ar: 'صغ استنتاجًا بحدّين: ما الذي ينتج عن حججك، وما الذي لا ينتج عنها تحديدًا؟',
      tr: 'İki sınırı olan bir sonuç yaz: Argümanlarından ne çıkar, özellikle ne çıkmaz?',
      ru: 'Сформулируй вывод с двумя границами: что следует из твоих аргументов и что из них как раз не следует?',
      ckb: 'دەرئەنجامێک بە دوو سنوور بنووسە: چی لە ئارگیومێنتەکانت دەردەکەوێت، و چی بە تایبەتی لێیان دەردەرناکەوێت؟',
      kmr: 'Encamekê bi du sînoran binivîse: ji argumanên te çi derdikeve, û çi bi taybetî jê dernakeve?',
      pl: 'Sformułuj wniosek z dwiema granicami: co wynika z twoich argumentów, a co właśnie z nich nie wynika?',
      ro: 'Formulează o concluzie cu două limite: ce rezultă din argumentele tale și ce nu rezultă din ele?',
      sq: 'Formulo një përfundim me dy kufij: çfarë del nga argumentet e tua dhe çfarë nuk del prej tyre?'
    },
    review: {
      de: 'Pruefe, ob deine Schlussfolgerung genug Gewicht hat, ohne mehr zu behaupten, als deine Belege tragen.',
      en: 'Check whether your conclusion has enough weight without claiming more than your evidence supports.',
      fa: 'بررسی کن که نتیجه‌گیری تو وزن کافی دارد، اما بیش از چیزی که شواهد پشتیبانی می‌کنند ادعا نمی‌کند.',
      ar: 'تحقّق مما إذا كان استنتاجك له وزن كافٍ من دون أن يدّعي أكثر مما تدعمه الأدلة.',
      tr: 'Sonucunun yeterli ağırlığı olduğunu, ama kanıtlarının taşıyabileceğinden fazlasını iddia etmediğini kontrol et.',
      ru: 'Проверь, достаточно ли весом вывод, но не утверждает ли он больше, чем подтверждают доказательства.',
      ckb: 'بپشکنە دەرئەنجامەکەت قورسایی پێویستی هەیە، بەڵام زیاتر لەوەی بەڵگەکانت پشتگیری دەکەن بانگەشە ناکات.',
      kmr: 'Kontrol bike ka encama te giraniya pêwîst heye, lê zêdetir ji tiştê ku belgeyên te piştgirî dikin nadibêje.',
      pl: 'Sprawdź, czy wniosek ma odpowiednią wagę, ale nie twierdzi więcej, niż pozwalają dowody.',
      ro: 'Verifică dacă concluzia are suficientă greutate fără să afirme mai mult decât susțin dovezile.',
      sq: 'Kontrollo nëse përfundimi ka peshë të mjaftueshme pa pretenduar më shumë sesa mbështesin provat.'
    }
  },
  {
    slug: 'c1-komplexe-argumentation-und-nuance-wiederholen',
    topic: {
      de: 'Komplexe Argumentation und Nuance wiederholen',
      en: 'reviewing complex argumentation and nuance',
      fa: 'مرور استدلال پیچیده و ظرافت معنا',
      ar: 'مراجعة الحجاج المعقّد والدقة المعنوية',
      tr: 'karmaşık argümantasyonu ve anlam inceliğini tekrar etme',
      ru: 'повторение сложной аргументации и нюанса',
      ckb: 'دووبارەکردنەوەی ئارگیومێنتی ئاڵۆز و وردیی مانا',
      kmr: 'dubarekirina argumana aloz û hûrguliya wateyê',
      pl: 'powtórka złożonej argumentacji i niuansu',
      ro: 'recapitularea argumentării complexe și a nuanței',
      sq: 'përsëritja e argumentimit kompleks dhe nuancës'
    },
    focus: {
      de: 'These, Einwand, Begrenzung und Schlussfolgerung zu einem klaren Ablauf verbinden',
      en: 'linking thesis, objection, limitation, and conclusion into a clear sequence',
      fa: 'وصل کردن تز، ایراد، محدودسازی و نتیجه‌گیری به یک روند روشن',
      ar: 'ربط الأطروحة والاعتراض والتقييد والاستنتاج في تسلسل واضح',
      tr: 'tez, itiraz, sınırlama ve sonucu açık bir akışta birleştirmeyi',
      ru: 'связать тезис, возражение, ограничение и вывод в ясную последовательность',
      ckb: 'بەستنی تێز، ڕەخنە، سنووردارکردن و دەرئەنجام بە ڕێڕەوێکی ڕوون',
      kmr: 'girêdana tez, îtiraz, sînordarkirin û encamê di rêzeke zelal de',
      pl: 'połączenie tezy, zastrzeżenia, ograniczenia i wniosku w jasny przebieg',
      ro: 'legarea tezei, obiecției, limitării și concluziei într-un parcurs clar',
      sq: 'lidhjen e tezës, kundërshtimit, kufizimit dhe përfundimit në një rrjedhë të qartë'
    },
    grammar: ['c1-c1-academic-grammar-review', 'akademische Grammatik-Review'],
    target: ['writing-template', 'c1-abschlussstellungnahme-mit-ausblick', 'Abschlussstellungnahme mit Ausblick'],
    practice: {
      de: 'Schreibe einen Mini-Absatz mit vier Funktionen: These, Konzession, Praezisierung und Schlussfolgerung.',
      en: 'Write a mini paragraph with four functions: thesis, concession, precision, and conclusion.',
      fa: 'یک پاراگراف کوتاه با چهار نقش بنویس: تز، پذیرش بخشی از نظر مقابل، دقیق‌سازی و نتیجه‌گیری.',
      ar: 'اكتب فقرة قصيرة بأربع وظائف: أطروحة، تنازل جزئي، توضيح دقيق، واستنتاج.',
      tr: 'Dört işlevli kısa bir paragraf yaz: tez, kısmi kabul, kesinleştirme ve sonuç.',
      ru: 'Напиши короткий абзац с четырьмя функциями: тезис, уступка, уточнение и вывод.',
      ckb: 'پاراگرافێکی کورت بە چوار ئەرک بنووسە: تێز، دانپێدان بە بەشێک، وردکردنەوە و دەرئەنجام.',
      kmr: 'Paragrafeke kurt bi çar erkên cuda binivîse: tez, qebûlkirina beşek, hûrgulîkirin û encam.',
      pl: 'Napisz krótki akapit z czterema funkcjami: teza, ustępstwo, doprecyzowanie i wniosek.',
      ro: 'Scrie un mini-paragraf cu patru funcții: teză, concesie, precizare și concluzie.',
      sq: 'Shkruaj një paragraf të shkurtër me katër funksione: tezë, lëshim, saktësim dhe përfundim.'
    },
    review: {
      de: 'Lies den Absatz laut und pruefe, ob jede Funktion erkennbar ist, ohne dass der Text wie eine Liste klingt.',
      en: 'Read the paragraph aloud and check whether each function is recognizable without the text sounding like a list.',
      fa: 'پاراگراف را بلند بخوان و بررسی کن که هر نقش روشن است، بدون اینکه متن شبیه فهرست خشک به نظر برسد.',
      ar: 'اقرأ الفقرة بصوت عالٍ وتحقق من أن كل وظيفة واضحة من دون أن يبدو النص كقائمة جامدة.',
      tr: 'Paragrafı sesli oku ve metnin liste gibi görünmeden her işlevin anlaşılır olup olmadığını kontrol et.',
      ru: 'Прочитай абзац вслух и проверь, распознается ли каждая функция, но текст не звучит как список.',
      ckb: 'پاراگرافەکە بە دەنگی بەرز بخوێنەوە و بپشکنە هەر ئەرکێک دیارە، بەبێ ئەوەی دەقەکە وەک لیستێکی وشک بدەنێت.',
      kmr: 'Paragrafê bi dengê bilind bixwîne û kontrol bike ka her erk xuya ye, bêyî ku nivîs wek lîsteyek hişk xuya bike.',
      pl: 'Przeczytaj akapit na głos i sprawdź, czy każda funkcja jest rozpoznawalna, a tekst nie brzmi jak lista.',
      ro: 'Citește paragraful cu voce tare și verifică dacă fiecare funcție se recunoaște fără ca textul să sune ca o listă.',
      sq: 'Lexoje paragrafin me zë dhe kontrollo nëse çdo funksion dallohet pa tingëlluar teksti si listë.'
    }
  }
];

function readInstruction(item) {
  return {
    de: `Lies den Lektionstext zu ${item.topic.de} und markiere, wo eine Aussage genauer, vorsichtiger oder tragfaehiger werden muss.`,
    en: `Read the lesson text on ${item.topic.en} and mark where a statement needs to become more precise, more careful, or more robust.`,
    fa: `متن درس درباره ${item.topic.fa} را بخوان و مشخص کن کجا یک جمله باید دقیق‌تر، محتاطانه‌تر یا از نظر استدلالی محکم‌تر شود.`,
    ar: `اقرأ نص الدرس عن ${item.topic.ar} وحدّد أين يجب أن تصبح العبارة أدق أو أحذر أو أقوى حجاجيًا.`,
    tr: `${item.topic.tr} konulu ders metnini oku ve bir ifadenin nerede daha kesin, daha temkinli ya da daha sağlam olması gerektiğini işaretle.`,
    ru: `Прочитай текст урока о теме «${item.topic.ru}» и отметь, где высказывание должно стать точнее, осторожнее или убедительнее.`,
    ckb: `دەقی وانەکە لەسەر ${item.topic.ckb} بخوێنەوە و دیاری بکە لە کوێ وتەیەک پێویستی بە وردی، بەهۆشی یان بەهێزیی زیاتری ئارگیومێنتی هەیە.`,
    kmr: `Nivîsa dersê li ser ${item.topic.kmr} bixwîne û nîşan bike ku li ku gotinek divê hûrgulîtir, baldartir an ji aliyê argumanê ve xurtir bibe.`,
    pl: `Przeczytaj tekst lekcji o temacie: ${item.topic.pl} i zaznacz, gdzie wypowiedź musi stać się precyzyjniejsza, ostrożniejsza albo mocniejsza argumentacyjnie.`,
    ro: `Citește textul lecției despre ${item.topic.ro} și marchează unde o afirmație trebuie să devină mai precisă, mai prudentă sau mai solidă argumentativ.`,
    sq: `Lexo tekstin e mësimit për ${item.topic.sq} dhe shëno ku një pohim duhet të bëhet më i saktë, më i kujdesshëm ose më i fortë si argument.`
  };
}

function grammarInstruction(item) {
  return {
    de: `Oeffne ${item.grammar[1]} und sammle drei Formulierungen, mit denen du ${item.focus.de} praeziser steuerst.`,
    en: `Open ${item.grammar[1]} and collect three formulations that help you manage ${item.focus.en} more precisely.`,
    fa: `بخش ${item.grammar[1]} را باز کن و سه عبارت پیدا کن که کمک کند ${item.focus.fa} را دقیق‌تر کنترل کنی.`,
    ar: `افتح قسم ${item.grammar[1]} واجمع ثلاث صيغ تساعدك على ضبط ${item.focus.ar} بدقة أكبر.`,
    tr: `${item.grammar[1]} bölümünü aç ve ${item.focus.tr} daha kesin yönetmene yardım eden üç ifade topla.`,
    ru: `Открой раздел ${item.grammar[1]} и собери три формулировки, которые помогут точнее управлять темой: ${item.focus.ru}.`,
    ckb: `بەشی ${item.grammar[1]} بکەرەوە و سێ دەربڕین کۆبکەوە کە یارمەتیت بدات ${item.focus.ckb} وردتر کۆنترۆڵ بکەیت.`,
    kmr: `Beşa ${item.grammar[1]} veke û sê derbirînan kom bike ku alîkar bin ${item.focus.kmr} hûrgulîtir rêve bibî.`,
    pl: `Otwórz sekcję ${item.grammar[1]} i zbierz trzy sformułowania, które pomogą ci precyzyjniej prowadzić: ${item.focus.pl}.`,
    ro: `Deschide secțiunea ${item.grammar[1]} și adună trei formulări care te ajută să controlezi mai precis: ${item.focus.ro}.`,
    sq: `Hap seksionin ${item.grammar[1]} dhe mblidh tri formulime që të ndihmojnë të drejtosh më saktë: ${item.focus.sq}.`
  };
}

function materialInstruction(item) {
  return {
    de: `Bearbeite das verlinkte Material zu ${item.target[2]} und achte darauf, wie du ${item.focus.de} in einer realistischen C1-Aufgabe einsetzt.`,
    en: `Work through the linked material on ${item.target[2]} and notice how to use ${item.focus.en} in a realistic C1 task.`,
    fa: `محتوای لینک‌شده درباره ${item.target[2]} را انجام بده و دقت کن چطور می‌توان ${item.focus.fa} را در یک موقعیت واقعی C1 به کار برد.`,
    ar: `اعمل على المادة المرتبطة حول ${item.target[2]} وانتبه إلى كيفية استخدام ${item.focus.ar} في مهمة واقعية على مستوى C1.`,
    tr: `${item.target[2]} hakkındaki bağlantılı malzemeyi çalış ve ${item.focus.tr} gerçekçi bir C1 görevinde nasıl kullanabileceğine dikkat et.`,
    ru: `Проработай связанный материал по теме ${item.target[2]} и обрати внимание, как использовать ${item.focus.ru} в реалистичном задании C1.`,
    ckb: `ماتریاڵی بەستەرکراو لەسەر ${item.target[2]} کاربکە و سەرنج بدە چۆن ${item.focus.ckb} لە ئەرکێکی ڕاستەقینەی C1دا بەکاربهێنیت.`,
    kmr: `Li ser materyala girêdayî ya ${item.target[2]} bixebite û bala xwe bide ka çawa ${item.focus.kmr} di erkeke rastîn a C1 de bi kar tînî.`,
    pl: `Przerób połączony materiał o ${item.target[2]} i zwróć uwagę, jak zastosować ${item.focus.pl} w realistycznym zadaniu C1.`,
    ro: `Lucrează cu materialul legat despre ${item.target[2]} și observă cum folosești ${item.focus.ro} într-o sarcină realistă de C1.`,
    sq: `Puno me materialin e lidhur për ${item.target[2]} dhe vëzhgo si përdoret ${item.focus.sq} në një detyrë reale të C1.`
  };
}

function block(kind, titleKey, instructionMap, targetType, targetSlug, estimatedMinutes, sortOrder, required) {
  return {
    kind,
    ...title(titleKey),
    instruction: instructionMap.de,
    instructionTranslations: tr(instructionMap),
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
    block('read', 'orient', readInstruction(item), 'none', null, 7, 10, true),
    block('grammar', 'grammar', grammarInstruction(item), 'grammar-topic', item.grammar[0], 8, 20, true),
    block(item.target[0] === 'writing-template' ? 'write' : item.target[0] === 'roleplay' ? 'roleplay' : 'exam-prep', 'material', materialInstruction(item), item.target[0], item.target[1], 9, 30, true),
    block('practice', 'write', item.practice, 'none', null, 8, 40, true),
    block('review', 'review', item.review, 'none', null, 6, 50, true)
  ];
}

fs.writeFileSync(file, JSON.stringify(data, null, 2) + '\n', 'utf8');
console.log(`Updated ${items.length} C1 Module 2 lessons with ${items.length * 5} activity blocks.`);

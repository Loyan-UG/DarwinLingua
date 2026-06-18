const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c1-souveraen-in-studium-beruf-und-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'Sprechziel klaeren',
    en: 'Clarify the speaking goal',
    fa: 'هدف گفتار را روشن کن',
    ar: 'وضّح هدف الكلام',
    tr: 'Konuşma hedefini netleştir',
    ru: 'Проясни цель высказывания',
    ckb: 'ئامانجی قسەکردن ڕوون بکەوە',
    kmr: 'Armanca axaftinê zelal bike',
    pl: 'Wyjaśnij cel wypowiedzi ustnej',
    ro: 'Clarifică scopul vorbirii',
    sq: 'Qartëso qëllimin e të folurit'
  },
  language: {
    de: 'Interaktionssprache pruefen',
    en: 'Check interaction language',
    fa: 'زبان تعامل را بررسی کن',
    ar: 'افحص لغة التفاعل',
    tr: 'Etkileşim dilini kontrol et',
    ru: 'Проверь язык взаимодействия',
    ckb: 'زمانی کارلێک بپشکنە',
    kmr: 'Zimana têkiliyê kontrol bike',
    pl: 'Sprawdź język interakcji',
    ro: 'Verifică limbajul interacțiunii',
    sq: 'Kontrollo gjuhën e ndërveprimit'
  },
  material: {
    de: 'Situation gezielt trainieren',
    en: 'Train the situation deliberately',
    fa: 'موقعیت را هدفمند تمرین کن',
    ar: 'تدرّب على الموقف بهدف واضح',
    tr: 'Durumu amaçlı çalış',
    ru: 'Целенаправленно отработай ситуацию',
    ckb: 'دۆخەکە بە ئامانج ڕاهێنان بکە',
    kmr: 'Rewşê bi armanc perwerde bike',
    pl: 'Przećwicz sytuację celowo',
    ro: 'Exersează situația cu scop clar',
    sq: 'Ushtroje situatën me qëllim të qartë'
  },
  speak: {
    de: 'Eigenen Beitrag sprechen',
    en: 'Deliver your own contribution',
    fa: 'بخش گفتاری خودت را ارائه کن',
    ar: 'قدّم مداخلتك الشفوية',
    tr: 'Kendi sözlü katkını sun',
    ru: 'Произнеси собственный вклад',
    ckb: 'بەشداریی زارەکی خۆت پێشکەش بکە',
    kmr: 'Beşdariya xwe ya devkî pêşkêş bike',
    pl: 'Wygłoś własną wypowiedź',
    ro: 'Prezintă propria intervenție orală',
    sq: 'Paraqit kontributin tënd gojor'
  },
  review: {
    de: 'Wirkung pruefen',
    en: 'Check the effect',
    fa: 'اثر گفتار را بررسی کن',
    ar: 'راجع أثر الكلام',
    tr: 'Etkisini kontrol et',
    ru: 'Проверь эффект',
    ckb: 'کاریگەری قسەکە بپشکنە',
    kmr: 'Bandorê kontrol bike',
    pl: 'Sprawdź efekt wypowiedzi',
    ro: 'Verifică efectul',
    sq: 'Kontrollo efektin'
  }
};

function translations(map) {
  return langs.map(language => ({ language, text: map[language] }));
}

function title(key) {
  return {
    title: titles[key].de,
    titleTranslations: translations(titles[key])
  };
}

const items = [
  {
    slug: 'c1-seminardiskussion-eroeffnen-und-einordnen',
    topic: {
      de: 'Seminardiskussion eroeffnen und einordnen',
      en: 'opening and framing a seminar discussion',
      fa: 'شروع و چارچوب‌بندی یک بحث سمیناری',
      ar: 'افتتاح نقاش في حلقة دراسية وتأطيره',
      tr: 'seminer tartışmasını açma ve çerçeveleme',
      ru: 'открытие и рамочное введение семинарской дискуссии',
      ckb: 'دەستپێکردن و چوارچێوەدان بە گفتوگۆی سمینار',
      kmr: 'destpêkirin û çarçovekirina gotûbêja semînerê',
      pl: 'otwarcie i ujęcie dyskusji seminaryjnej',
      ro: 'deschiderea și încadrarea unei discuții de seminar',
      sq: 'hapja dhe kornizimi i një diskutimi seminari'
    },
    focus: {
      de: 'ein Thema kurz einleiten, ohne schon die ganze Diskussion vorwegzunehmen',
      en: 'briefly introducing a topic without pre-empting the whole discussion',
      fa: 'معرفی کوتاه موضوع بدون اینکه کل بحث را از قبل مصرف کنی',
      ar: 'تقديم الموضوع بإيجاز من دون استباق النقاش كله',
      tr: 'konuyu kısa tanıtıp tüm tartışmayı baştan tüketmemeyi',
      ru: 'кратко вводить тему, не предвосхищая всю дискуссию',
      ckb: 'ناساندنی کورتی بابەت بەبێ ئەوەی هەموو گفتوگۆکە پێشوەخت بخۆیت',
      kmr: 'mijarê bi kurtî destnîşan kirin bêyî ku hemû gotûbêjê ji berê ve bigirî',
      pl: 'krótkie wprowadzenie tematu bez uprzedzania całej dyskusji',
      ro: 'introducerea scurtă a temei fără a anticipa întreaga discuție',
      sq: 'prezantimin e shkurtër të temës pa e konsumuar gjithë diskutimin që në fillim'
    },
    grammar: ['c1-c1-discussion-grammar', 'Diskussionsgrammatik'],
    target: ['exam-prep-unit', 'c1-seminardiskussion-eroeffnen', 'Seminardiskussion eroeffnen'],
    practice: {
      de: 'Bereite eine Eroeffnung mit drei Saetzen vor: Kontext, offene Leitfrage und Einladung zu einer ersten Position.',
      en: 'Prepare an opening in three sentences: context, open guiding question, and invitation to a first position.',
      fa: 'یک شروع سه‌جمله‌ای آماده کن: زمینه بحث، پرسش راهنمای باز و دعوت به بیان اولین موضع.',
      ar: 'حضّر افتتاحية في ثلاث جمل: السياق، سؤال موجّه مفتوح، ودعوة إلى موقف أول.',
      tr: 'Üç cümlelik bir açılış hazırla: bağlam, açık yönlendirici soru ve ilk tutuma davet.',
      ru: 'Подготовь вступление из трех предложений: контекст, открытый направляющий вопрос и приглашение к первой позиции.',
      ckb: 'دەستپێکێک بە سێ ڕستە ئامادە بکە: پاشبنەما، پرسی ڕێنوێنی کراوە و بانگهێشت بۆ یەکەم هەڵوێست.',
      kmr: 'Destpêkek bi sê hevokan amade bike: çarçove, pirsa rêber a vekirî û vexwendin bo helwesta yekem.',
      pl: 'Przygotuj otwarcie w trzech zdaniach: kontekst, otwarte pytanie przewodnie i zaproszenie do pierwszego stanowiska.',
      ro: 'Pregătește o deschidere în trei propoziții: context, întrebare-ghid deschisă și invitație la o primă poziție.',
      sq: 'Përgatit një hapje me tri fjali: konteksti, pyetje udhëzuese e hapur dhe ftesë për qëndrimin e parë.'
    },
    review: {
      de: 'Pruefe, ob deine Eroeffnung Raum fuer andere laesst und nicht wie ein fertiges Urteil klingt.',
      en: 'Check whether your opening leaves room for others and does not sound like a finished judgment.',
      fa: 'بررسی کن که شروع تو برای نظر دیگران جا می‌گذارد و شبیه حکم نهایی به نظر نمی‌رسد.',
      ar: 'تحقّق من أن الافتتاحية تترك مجالًا للآخرين ولا تبدو كحكم نهائي.',
      tr: 'Açılışının başkalarına alan bıraktığını ve kesin hüküm gibi görünmediğini kontrol et.',
      ru: 'Проверь, оставляет ли вступление место другим и не звучит ли как готовый вердикт.',
      ckb: 'بپشکنە دەستپێکەکەت شوێن بۆ کەسانی تر دەهێڵێتەوە و وەک بڕیاری کۆتایی نادنێت.',
      kmr: 'Kontrol bike ka destpêka te cih ji bo kesên din dihêle û wek biryareke dawî xuya nake.',
      pl: 'Sprawdź, czy otwarcie zostawia miejsce innym i nie brzmi jak gotowy osąd.',
      ro: 'Verifică dacă deschiderea lasă loc celorlalți și nu sună ca o judecată finală.',
      sq: 'Kontrollo nëse hapja jote u lë hapësirë të tjerëve dhe nuk tingëllon si gjykim përfundimtar.'
    }
  },
  {
    slug: 'c1-these-im-seminar-einordnen',
    topic: {
      de: 'These im Seminar einordnen',
      en: 'placing a thesis in a seminar context',
      fa: 'جای‌دادن یک تز در بحث سمیناری',
      ar: 'وضع أطروحة في سياق النقاش الدراسي',
      tr: 'bir tezi seminer bağlamına yerleştirme',
      ru: 'встраивание тезиса в контекст семинара',
      ckb: 'دانانی تێزێک لە چوارچێوەی سمیناردا',
      kmr: 'danîna tezekê di çarçoveya semînerê de',
      pl: 'osadzenie tezy w kontekście seminarium',
      ro: 'încadrarea unei teze în contextul seminarului',
      sq: 'vendosja e një teze në kontekst seminari'
    },
    focus: {
      de: 'eine These mit Quelle, Reichweite und eigener Position verbinden',
      en: 'linking a thesis with source, scope, and your own position',
      fa: 'وصل کردن یک تز به منبع، دامنه اعتبار و موضع خودت',
      ar: 'ربط الأطروحة بالمصدر والنطاق وموقفك أنت',
      tr: 'bir tezi kaynak, kapsam ve kendi tutumunla ilişkilendirmeyi',
      ru: 'связывать тезис с источником, сферой действия и собственной позицией',
      ckb: 'بەستنی تێز بە سەرچاوە، مەودا و هەڵوێستی خۆت',
      kmr: 'girêdana tezê bi çavkanî, qada derbasbûnê û helwesta xwe',
      pl: 'łączenie tezy ze źródłem, zakresem i własnym stanowiskiem',
      ro: 'legarea unei teze de sursă, sferă de valabilitate și poziția ta',
      sq: 'lidhjen e tezës me burimin, shtrirjen dhe qëndrimin tënd'
    },
    grammar: ['c1-academic-argument-grammar', 'akademische Argumentationsgrammatik'],
    target: ['roleplay', 'c1-in-einem-seminar-eine-these-einordnen', 'These im Seminar'],
    practice: {
      de: 'Ordne eine These in vier Schritten ein: Wer sagt es, worauf bezieht es sich, was traegt es bei, wo ist die Grenze?',
      en: 'Place one thesis in four steps: who says it, what it refers to, what it contributes, and where its limit lies.',
      fa: 'یک تز را در چهار قدم جا بده: چه کسی می‌گوید، به چه چیزی مربوط است، چه کمکی می‌کند و محدودیتش کجاست.',
      ar: 'ضع أطروحة في أربع خطوات: من يقولها، إلى ماذا تشير، ماذا تضيف، وأين حدودها؟',
      tr: 'Bir tezi dört adımda konumlandır: kim söylüyor, neye dayanıyor, ne katkı sağlıyor, sınırı nerede?',
      ru: 'Размести тезис в четыре шага: кто его высказывает, к чему он относится, что добавляет и где его предел.',
      ckb: 'تێزێک بە چوار هەنگاو دابنێ: کێ دەڵێت، پەیوەندی بە چییە، چی زیاد دەکات، سنوورەکەی لە کوێیە؟',
      kmr: 'Tezekê di çar gavan de bi cih bike: kî dibêje, bi çi ve girêdayî ye, çi zêde dike, sînorê wê li ku ye?',
      pl: 'Osadź tezę w czterech krokach: kto ją wypowiada, do czego się odnosi, co wnosi i gdzie jest jej granica.',
      ro: 'Încadrează o teză în patru pași: cine o spune, la ce se referă, ce aduce și unde îi este limita.',
      sq: 'Vendose një tezë në katër hapa: kush e thotë, me çfarë lidhet, çfarë shton dhe ku është kufiri i saj.'
    },
    review: {
      de: 'Pruefe, ob deine Einordnung die These klaert, ohne sie sofort zu vereinnahmen oder abzulehnen.',
      en: 'Check whether your framing clarifies the thesis without immediately claiming or rejecting it.',
      fa: 'بررسی کن که چارچوب‌بندی تو تز را روشن می‌کند، بدون اینکه فوراً آن را تصاحب یا رد کند.',
      ar: 'تحقّق من أن تأطيرك يوضح الأطروحة من دون أن يتبناها أو يرفضها فورًا.',
      tr: 'Çerçevelemenin tezi hemen sahiplenmeden ya da reddetmeden netleştirdiğini kontrol et.',
      ru: 'Проверь, проясняет ли твое введение тезис, не присваивая и не отвергая его сразу.',
      ckb: 'بپشکنە چوارچێوەدانەکەت تێزەکە ڕوون دەکات، بەبێ ئەوەی دەستبەجێ بیخاتە ژێر دەستی خۆت یان ڕەتی بکاتەوە.',
      kmr: 'Kontrol bike ka çarçovekirina te tezê zelal dike, bêyî ku wê tavilê xwedî bike an red bike.',
      pl: 'Sprawdź, czy twoje osadzenie wyjaśnia tezę, nie przejmując jej od razu ani jej nie odrzucając.',
      ro: 'Verifică dacă încadrarea clarifică teza fără să o preia sau respingă imediat.',
      sq: 'Kontrollo nëse kornizimi yt e qartëson tezën pa e përvetësuar ose refuzuar menjëherë.'
    }
  }
];

const extraItems = [
  {
    slug: 'c1-einwand-im-kolloquium-aufgreifen',
    topic: { de: 'Einwand im Kolloquium aufgreifen', en: 'taking up an objection in a colloquium', fa: 'پرداختن به ایراد در kolloquium', ar: 'تناول اعتراض في مناقشة أكاديمية', tr: 'kolokyumda bir itirazı ele alma', ru: 'реакция на возражение на коллоквиуме', ckb: 'وەرگرتنی ڕەخنە لە کۆلۆکیۆم', kmr: 'girtina îtirazekê di kolokyumê de', pl: 'podjęcie zastrzeżenia na kolokwium', ro: 'preluarea unei obiecții într-un colocviu', sq: 'marrja parasysh e një kundërshtimi në kolokium' },
    focus: { de: 'einen Einwand ernst nehmen und die eigene Linie trotzdem halten', en: 'taking an objection seriously while still keeping your own line', fa: 'یک ایراد را جدی گرفتن و با این حال مسیر استدلال خودت را حفظ کردن', ar: 'أخذ الاعتراض بجدية مع الحفاظ على خطك في الحجة', tr: 'itirazı ciddiye alıp yine de kendi çizgini korumayı', ru: 'серьезно принять возражение и всё же сохранить собственную линию', ckb: 'ڕەخنەیەک بە جدی وەرگرتن و لەگەڵ ئەوەشدا ڕێڕەوی خۆت پاراستن', kmr: 'îtirazek bi ciddî girtin û tevî wê rêça xwe parastin', pl: 'poważne potraktowanie zastrzeżenia przy zachowaniu własnej linii', ro: 'tratarea serioasă a unei obiecții păstrând totuși propria linie', sq: 'marrjen seriozisht të një kundërshtimi duke ruajtur vijën tënde' },
    grammar: ['c1-concession-structures', 'Konzessionsstrukturen'],
    target: ['roleplay', 'c1-einen-einwand-im-kolloquium-aufgreifen', 'Einwand im Kolloquium']
  },
  {
    slug: 'c1-praesentation-mit-kritischen-nachfragen',
    topic: { de: 'Praesentation mit kritischen Nachfragen', en: 'presentation with critical follow-up questions', fa: 'ارائه همراه با پرسش‌های انتقادی', ar: 'عرض مع أسئلة نقدية متابعة', tr: 'eleştirel sorularla sunum', ru: 'презентация с критическими вопросами', ckb: 'پێشکەشکردن لەگەڵ پرسیاری ڕەخنەیی', kmr: 'pêşkêşî bi pirsên rexneyî', pl: 'prezentacja z krytycznymi pytaniami', ro: 'prezentare cu întrebări critice', sq: 'prezantim me pyetje kritike' },
    focus: { de: 'kritische Nachfragen als Praezisierung nutzen, nicht als Angriff behandeln', en: 'using critical questions for clarification rather than treating them as an attack', fa: 'پرسش‌های انتقادی را فرصتی برای دقیق‌سازی بدانی، نه حمله شخصی', ar: 'استخدام الأسئلة النقدية للتوضيح لا التعامل معها كهجوم', tr: 'eleştirel soruları saldırı değil netleştirme fırsatı olarak kullanmayı', ru: 'использовать критические вопросы для уточнения, а не воспринимать их как нападение', ckb: 'پرسیاری ڕەخنەیی وەک دەرفەتێک بۆ وردکردنەوە بەکاربهێنیت، نەک وەک هێرش', kmr: 'pirsên rexneyî wek derfet bo zelalkirinê bikar anîn, ne wek êrîş', pl: 'traktowanie krytycznych pytań jako doprecyzowania, nie ataku', ro: 'folosirea întrebărilor critice pentru clarificare, nu ca atac', sq: 'përdorimin e pyetjeve kritike për saktësim, jo si sulm' },
    grammar: ['c1-c1-presentation-grammar', 'Praesentationsgrammatik'],
    target: ['exam-prep-unit', 'c1-kritische-nachfragen-in-praesentationen-beantworten', 'kritische Nachfragen']
  },
  {
    slug: 'c1-diskussion-trotz-einwaenden-strukturieren',
    topic: { de: 'Diskussion trotz Einwaenden strukturieren', en: 'structuring a discussion despite objections', fa: 'ساختار دادن به بحث با وجود ایرادها', ar: 'تنظيم النقاش رغم الاعتراضات', tr: 'itirazlara rağmen tartışmayı yapılandırma', ru: 'структурирование дискуссии несмотря на возражения', ckb: 'ڕێکخستنی گفتوگۆ لەگەڵ بوونی ڕەخنەکان', kmr: 'avakirin û rêxistina gotûbêjê tevî îtirazan', pl: 'strukturyzowanie dyskusji mimo zastrzeżeń', ro: 'structurarea discuției în ciuda obiecțiilor', sq: 'strukturimi i diskutimit pavarësisht kundërshtimeve' },
    focus: { de: 'Einwaende ordnen, priorisieren und zur Leitfrage zurueckfuehren', en: 'organizing objections, prioritizing them, and returning to the guiding question', fa: 'ایرادها را مرتب و اولویت‌بندی کنی و دوباره به پرسش اصلی برگردانی', ar: 'ترتيب الاعتراضات وتحديد أولويتها والعودة إلى السؤال الرئيس', tr: 'itirazları düzenlemeyi, önceliklendirmeyi ve ana soruya döndürmeyi', ru: 'упорядочивать возражения, расставлять приоритеты и возвращать к ведущему вопросу', ckb: 'ڕەخنەکان ڕێکبخەیت، پێشینەیان بدەیت و بیانگەڕێنیتەوە بۆ پرسی سەرەکی', kmr: 'îtirazan rêz bike, pêşîtiyê bidî û vegerînî ser pirsa sereke', pl: 'porządkowanie zastrzeżeń, nadawanie im priorytetu i powrót do pytania przewodniego', ro: 'ordonarea obiecțiilor, prioritizarea lor și revenirea la întrebarea principală', sq: 'renditjen e kundërshtimeve, prioritizimin e tyre dhe kthimin te pyetja kryesore' },
    grammar: ['c1-c1-discussion-grammar', 'Diskussionsgrammatik'],
    target: ['roleplay', 'c1-eine-diskussion-trotz-einwaenden-strukturieren', 'Diskussion trotz Einwaenden']
  },
  {
    slug: 'c1-minderheitenposition-vertreten',
    topic: { de: 'Minderheitenposition vertreten', en: 'representing a minority position', fa: 'دفاع از موضع اقلیت', ar: 'تمثيل موقف أقلية', tr: 'azınlık görüşünü savunma', ru: 'представление позиции меньшинства', ckb: 'بەرگری لە هەڵوێستی کەمینە', kmr: 'parastina helwesta kêmarî', pl: 'reprezentowanie stanowiska mniejszościowego', ro: 'susținerea unei poziții minoritare', sq: 'mbrojtja e një qëndrimi pakice' },
    focus: { de: 'eine Minderheitenposition sachlich vertreten, ohne defensiv oder provokativ zu wirken', en: 'presenting a minority position factually without sounding defensive or provocative', fa: 'از موضع اقلیت عینی دفاع کنی، بدون اینکه حالت تدافعی یا تحریک‌آمیز بگیرد', ar: 'عرض موقف أقلية بموضوعية من دون أن يبدو دفاعيًا أو استفزازيًا', tr: 'azınlık görüşünü savunmacı ya da kışkırtıcı görünmeden nesnel sunmayı', ru: 'представлять позицию меньшинства предметно, не звуча оборонительно или провокационно', ckb: 'هەڵوێستی کەمینە بە بابەتی پێشکەش بکەیت، بەبێ ئەوەی بەرگریانە یان ورووژێنەر دەنێت', kmr: 'helwesta kêmarî bi awayekî babetî pêşkêş kirin bêyî ku berevanî an provokatîf xuya bike', pl: 'rzeczowe przedstawienie stanowiska mniejszości bez tonu defensywnego lub prowokacyjnego', ro: 'susținerea factuală a unei poziții minoritare fără ton defensiv sau provocator', sq: 'paraqitjen faktike të një qëndrimi pakice pa tingëlluar mbrojtës ose provokues' },
    grammar: ['c1-hedging-and-cautious-language', 'vorsichtige Sprache'],
    target: ['roleplay', 'c1-in-einer-gruppe-eine-minderheitenposition-vertreten', 'Minderheitenposition in der Gruppe']
  },
  {
    slug: 'c1-kompromiss-in-formaler-pruefung-aushandeln',
    topic: { de: 'Kompromiss in formaler Pruefung aushandeln', en: 'negotiating a compromise in a formal exam', fa: 'مذاکره برای راه‌حل میانی در امتحان رسمی', ar: 'التفاوض على حل وسط في امتحان رسمي', tr: 'resmî sınavda uzlaşma müzakere etme', ru: 'переговоры о компромиссе на официальном экзамене', ckb: 'دانوسان بۆ ڕێککەوتنی ناوەندی لە تاقیکردنەوەی فەرمی', kmr: 'li azmûneke fermî li ser lihevhatinekê danûstandin', pl: 'negocjowanie kompromisu w formalnym egzaminie', ro: 'negocierea unui compromis într-un examen formal', sq: 'negocimi i një kompromisi në provim formal' },
    focus: { de: 'Zugestaendnis, Bedingung und eigene Mindestposition klar trennen', en: 'clearly separating concession, condition, and your minimum position', fa: 'امتیاز دادن، شرط و حداقل موضع خودت را روشن از هم جدا کنی', ar: 'فصل التنازل والشرط والحد الأدنى لموقفك بوضوح', tr: 'taviz, koşul ve asgari tutumunu açıkça ayırmayı', ru: 'четко разделять уступку, условие и свою минимальную позицию', ckb: 'دانپێدان، مەرج و کەمترین هەڵوێستی خۆت بە ڕوونی جیا بکەیتەوە', kmr: 'qebûlkirin, şert û helwesta herî kêm a xwe bi zelalî cuda kirin', pl: 'jasne oddzielenie ustępstwa, warunku i własnego minimum', ro: 'separarea clară a concesiei, condiției și poziției minime proprii', sq: 'ndarjen e qartë të lëshimit, kushtit dhe pozicionit minimal tënd' },
    grammar: ['c1-concession-structures', 'Konzessionsstrukturen'],
    target: ['roleplay', 'c1-einen-kompromiss-in-einer-formalen-pruefung-aushandeln', 'Kompromiss in formaler Pruefung']
  },
  {
    slug: 'c1-spontan-reagieren-ohne-strukturverlust',
    topic: { de: 'Spontan reagieren ohne Strukturverlust', en: 'responding spontaneously without losing structure', fa: 'واکنش سریع بدون از دست دادن ساختار', ar: 'الرد العفوي من دون فقدان البنية', tr: 'yapıyı kaybetmeden spontane tepki verme', ru: 'спонтанная реакция без потери структуры', ckb: 'وەڵامی خێرا بەبێ ونکردنی پێکهاتە', kmr: 'bersiva spontan bê windakirina avahiyê', pl: 'spontaniczna reakcja bez utraty struktury', ro: 'reacție spontană fără pierderea structurii', sq: 'reagim spontan pa humbur strukturën' },
    focus: { de: 'spontan antworten und trotzdem These, Grund und Rueckbezug behalten', en: 'answering spontaneously while still keeping thesis, reason, and link back', fa: 'سریع پاسخ بدهی و با این حال تز، دلیل و اتصال به بحث را حفظ کنی', ar: 'الرد بسرعة مع الحفاظ على الأطروحة والسبب والعودة إلى النقاش', tr: 'spontane cevap verirken tez, gerekçe ve geri bağlantıyı korumayı', ru: 'отвечать спонтанно, сохраняя тезис, причину и связь с обсуждением', ckb: 'وەڵامی خێرا بدەیت و هێشتا تێز، هۆکار و بەستنەوە بە گفتوگۆکە بپارێزیت', kmr: 'spontan bersiv dan û herwiha tez, sedem û vegerandina bo gotûbêjê parastin', pl: 'odpowiadanie spontaniczne przy zachowaniu tezy, powodu i powiązania z dyskusją', ro: 'răspuns spontan păstrând teza, motivul și legătura cu discuția', sq: 'të përgjigjesh spontanisht duke ruajtur tezën, arsyen dhe lidhjen me diskutimin' },
    grammar: ['c1-c1-discussion-grammar', 'Diskussionsgrammatik'],
    target: ['exam-prep-unit', 'c1-spontane-antworten-strukturieren', 'spontane Antworten']
  },
  {
    slug: 'c1-anschlussfragen-souveraen-beantworten',
    topic: { de: 'Anschlussfragen souveraen beantworten', en: 'answering follow-up questions confidently', fa: 'پاسخ مطمئن به پرسش‌های پیگیری', ar: 'الإجابة بثبات عن أسئلة المتابعة', tr: 'takip sorularını güvenle yanıtlama', ru: 'уверенные ответы на уточняющие вопросы', ckb: 'وەڵامی دڵنیایانە بە پرسیاری بەدواداچوون', kmr: 'bersivdana bi ewle li pirsên şopandinê', pl: 'pewne odpowiadanie na pytania uzupełniające', ro: 'răspuns sigur la întrebări de continuare', sq: 'përgjigje e sigurt ndaj pyetjeve pasuese' },
    focus: { de: 'Rueckfragen klaeren, beantworten und bei Bedarf begrenzt korrigieren', en: 'clarifying follow-up questions, answering them, and correcting with limits when needed', fa: 'پرسش‌های پیگیری را روشن کنی، پاسخ بدهی و اگر لازم بود محدود و دقیق اصلاح کنی', ar: 'توضيح أسئلة المتابعة والإجابة عنها والتصحيح المحدود عند الحاجة', tr: 'takip sorularını netleştirmeyi, yanıtlamayı ve gerekirse sınırlı düzeltmeyi', ru: 'прояснять уточняющие вопросы, отвечать на них и при необходимости ограниченно корректировать', ckb: 'پرسیاری بەدواداچوون ڕوون بکەیت، وەڵام بدەیت و ئەگەر پێویست بوو بە سنوورەوە ڕاستی بکەیتەوە', kmr: 'pirsên şopandinê zelal kirin, bersiv dan û heke pêwîst be bi sînor sererast kirin', pl: 'wyjaśnianie pytań uzupełniających, odpowiadanie i ograniczone korygowanie w razie potrzeby', ro: 'clarificarea întrebărilor de continuare, răspunsul și corectarea limitată când e nevoie', sq: 'qartësimin e pyetjeve pasuese, përgjigjen dhe korrigjimin e kufizuar kur duhet' },
    grammar: ['c1-c1-presentation-grammar', 'Praesentationsgrammatik'],
    target: ['roleplay', 'c1-eine-frage-aus-dem-publikum-souveraen-beantworten', 'Frage aus dem Publikum']
  },
  {
    slug: 'c1-gesprochene-interaktion-seminar-und-pruefung-wiederholen',
    topic: { de: 'Gesprochene Interaktion wiederholen', en: 'reviewing spoken interaction', fa: 'مرور تعامل گفتاری در سمینار و امتحان', ar: 'مراجعة التفاعل الشفهي في الندوة والامتحان', tr: 'sözlü etkileşimi tekrar etme', ru: 'повторение устного взаимодействия', ckb: 'دووبارەکردنەوەی کارلێکی زارەکی', kmr: 'dubarekirina têkiliya devkî', pl: 'powtórka interakcji ustnej', ro: 'recapitularea interacțiunii orale', sq: 'përsëritja e ndërveprimit gojor' },
    focus: { de: 'Eroeffnen, Einordnen, Reagieren und Abschliessen zu einem klaren Gespraechsverlauf verbinden', en: 'linking opening, framing, reacting, and closing into a clear conversation flow', fa: 'شروع، چارچوب‌بندی، واکنش و جمع‌بندی را به یک روند گفت‌وگوی روشن وصل کنی', ar: 'ربط الافتتاح والتأطير والرد والخاتمة في مسار حوار واضح', tr: 'açma, çerçeveleme, tepki verme ve kapatmayı açık bir konuşma akışında birleştirmeyi', ru: 'связать открытие, рамку, реакцию и завершение в ясный ход разговора', ckb: 'دەستپێک، چوارچێوەدان، وەڵامدانەوە و کۆتایی بە ڕێڕەوی گفتوگۆیەکی ڕوون ببەستیت', kmr: 'destpêk, çarçove, bersiv û dawî di rêça axaftineke zelal de girêdan', pl: 'połączenie otwarcia, ramy, reakcji i zamknięcia w jasny przebieg rozmowy', ro: 'legarea deschiderii, încadrării, reacției și încheierii într-un flux clar de conversație', sq: 'lidhjen e hapjes, kornizimit, reagimit dhe mbylljes në një rrjedhë të qartë bisede' },
    grammar: ['c1-c1-register-review', 'Register-Review'],
    target: ['exam-prep-unit', 'c1-muendliche-pruefungsleistung-steuern', 'muendliche Pruefungsleistung']
  }
];

for (const item of extraItems) {
  items.push({
    ...item,
    practice: {
      de: `Sprich einen Beitrag in vier Teilen: Bezug auf die Frage, klare Position, ein Grund und eine kurze Rueckbindung an die Diskussion.`,
      en: `Deliver a contribution in four parts: reference to the question, clear position, one reason, and a short link back to the discussion.`,
      fa: `یک مشارکت گفتاری در چهار بخش ارائه کن: اشاره به پرسش، موضع روشن، یک دلیل و اتصال کوتاه به ادامه بحث.`,
      ar: `قدّم مداخلة في أربعة أجزاء: إشارة إلى السؤال، موقف واضح، سبب واحد، وربط قصير بالنقاش.`,
      tr: `Dört parçalı bir sözlü katkı sun: soruya bağlantı, açık tutum, bir gerekçe ve tartışmaya kısa dönüş.`,
      ru: `Произнеси вклад из четырех частей: связь с вопросом, ясная позиция, один аргумент и короткая привязка к дискуссии.`,
      ckb: `بەشدارییەکی زارەکی بە چوار بەش پێشکەش بکە: ئاماژە بە پرس، هەڵوێستی ڕوون، یەک هۆکار و بەستنەوەی کورت بە گفتوگۆکە.`,
      kmr: `Beşdariyeke devkî bi çar beşan pêşkêş bike: girêdan bi pirsê, helwesta zelal, sedemek û vegerandina kurt bo gotûbêjê.`,
      pl: `Wygłoś wypowiedź w czterech częściach: odniesienie do pytania, jasne stanowisko, jeden powód i krótkie nawiązanie do dyskusji.`,
      ro: `Prezintă o intervenție în patru părți: raportare la întrebare, poziție clară, un motiv și o scurtă revenire la discuție.`,
      sq: `Paraqit një ndërhyrje në katër pjesë: lidhje me pyetjen, qëndrim i qartë, një arsye dhe rikthim i shkurtër te diskutimi.`
    },
    review: {
      de: `Pruefe, ob dein Beitrag kooperativ wirkt und zugleich eine erkennbare eigene Linie behaelt.`,
      en: `Check whether your contribution sounds cooperative while still keeping a recognizable line of your own.`,
      fa: `بررسی کن که مشارکت تو هم همکاری‌جویانه به نظر می‌رسد و هم خط فکری مستقل خودت را حفظ می‌کند.`,
      ar: `تحقّق من أن مداخلتك تبدو تعاونية ومع ذلك تحافظ على خطك الفكري الخاص.`,
      tr: `Katkının işbirliğine açık göründüğünü ve yine de kendi çizgisini koruduğunu kontrol et.`,
      ru: `Проверь, звучит ли твой вклад кооперативно и сохраняет ли при этом собственную линию.`,
      ckb: `بپشکنە بەشدارییەکەت هاوکاریخوازانە دەنێت و هێشتا ڕێڕەوی بیرکردنەوەی خۆت پاراستووە.`,
      kmr: `Kontrol bike ka beşdariya te hevkarane xuya dike û herwiha rêça xwe ya taybet diparêze.`,
      pl: `Sprawdź, czy wypowiedź brzmi współpracująco, a jednocześnie zachowuje własną linię myślenia.`,
      ro: `Verifică dacă intervenția ta pare cooperantă și totuși păstrează o linie proprie clară.`,
      sq: `Kontrollo nëse ndërhyrja jote tingëllon bashkëpunuese dhe ruan një vijë të qartë tënden.`
    }
  });
}

function orientInstruction(item) {
  return {
    de: `Lies den Lektionstext zu ${item.topic.de} und notiere, welche kommunikative Aufgabe im Gespraech zuerst geloest werden muss.`,
    en: `Read the lesson text on ${item.topic.en} and note which communicative task must be solved first in the conversation.`,
    fa: `متن درس درباره ${item.topic.fa} را بخوان و یادداشت کن در گفت‌وگو اول باید کدام وظیفه ارتباطی حل شود.`,
    ar: `اقرأ نص الدرس عن ${item.topic.ar} وسجّل أي مهمة تواصلية يجب حلها أولًا في الحوار.`,
    tr: `${item.topic.tr} konulu ders metnini oku ve konuşmada önce hangi iletişim görevini çözmen gerektiğini not et.`,
    ru: `Прочитай текст урока о теме «${item.topic.ru}» и отметь, какую коммуникативную задачу в разговоре нужно решить первой.`,
    ckb: `دەقی وانەکە لەسەر ${item.topic.ckb} بخوێنەوە و بنووسە لە گفتوگۆدا سەرەتا کام ئەرکی پەیوەندی دەبێت چارەسەر بکرێت.`,
    kmr: `Nivîsa dersê li ser ${item.topic.kmr} bixwîne û binivîse di axaftinê de kîjan erka ragihandinê divê pêşî were çareserkirin.`,
    pl: `Przeczytaj tekst lekcji o temacie: ${item.topic.pl} i zanotuj, które zadanie komunikacyjne trzeba rozwiązać w rozmowie najpierw.`,
    ro: `Citește textul lecției despre ${item.topic.ro} și notează ce sarcină comunicativă trebuie rezolvată prima în conversație.`,
    sq: `Lexo tekstin e mësimit për ${item.topic.sq} dhe shëno cila detyrë komunikimi duhet zgjidhur e para në bisedë.`
  };
}

function languageInstruction(item) {
  return {
    de: `Oeffne ${item.grammar[1]} und sammle drei Formulierungen, mit denen du ${item.focus.de} muendlich steuerst.`,
    en: `Open ${item.grammar[1]} and collect three formulations that help you manage ${item.focus.en} orally.`,
    fa: `بخش ${item.grammar[1]} را باز کن و سه عبارت پیدا کن که در گفتار به این مهارت کمک کند: ${item.focus.fa}.`,
    ar: `افتح قسم ${item.grammar[1]} واجمع ثلاث صيغ تساعدك على ضبط ${item.focus.ar} شفهيًا.`,
    tr: `${item.grammar[1]} bölümünü aç ve ${item.focus.tr} sözlü olarak yönetmene yardım eden üç ifade topla.`,
    ru: `Открой раздел ${item.grammar[1]} и собери три формулировки, которые помогут устно управлять темой: ${item.focus.ru}.`,
    ckb: `بەشی ${item.grammar[1]} بکەرەوە و سێ دەربڕین کۆبکەوە کە یارمەتیت بدات ${item.focus.ckb} لە قسەکردندا کۆنترۆڵ بکەیت.`,
    kmr: `Beşa ${item.grammar[1]} veke û sê derbirînan kom bike ku alîkar bin ${item.focus.kmr} bi devkî rêve bibî.`,
    pl: `Otwórz sekcję ${item.grammar[1]} i zbierz trzy sformułowania, które pomogą ci ustnie prowadzić: ${item.focus.pl}.`,
    ro: `Deschide secțiunea ${item.grammar[1]} și adună trei formulări care te ajută să gestionezi oral: ${item.focus.ro}.`,
    sq: `Hap seksionin ${item.grammar[1]} dhe mblidh tri formulime që të ndihmojnë të drejtosh me gojë: ${item.focus.sq}.`
  };
}

function materialInstruction(item) {
  return {
    de: `Bearbeite das verlinkte Material zu ${item.target[2]} und achte darauf, wie du ${item.focus.de} in einer realistischen C1-Interaktion einsetzt.`,
    en: `Work through the linked material on ${item.target[2]} and notice how to use ${item.focus.en} in a realistic C1 interaction.`,
    fa: `محتوای لینک‌شده درباره ${item.target[2]} را انجام بده و دقت کن این مهارت در یک تعامل واقعی C1 چطور اجرا می‌شود: ${item.focus.fa}.`,
    ar: `اعمل على المادة المرتبطة حول ${item.target[2]} وانتبه إلى كيفية استخدام ${item.focus.ar} في تفاعل واقعي على مستوى C1.`,
    tr: `${item.target[2]} hakkındaki bağlantılı malzemeyi çalış ve ${item.focus.tr} gerçekçi bir C1 etkileşiminde nasıl kullanabileceğine dikkat et.`,
    ru: `Проработай связанный материал по теме ${item.target[2]} и обрати внимание, как использовать ${item.focus.ru} в реалистичном взаимодействии C1.`,
    ckb: `ماتریاڵی بەستەرکراو لەسەر ${item.target[2]} کاربکە و سەرنج بدە چۆن ${item.focus.ckb} لە کارلێکی ڕاستەقینەی C1دا بەکاربهێنیت.`,
    kmr: `Li ser materyala girêdayî ya ${item.target[2]} bixebite û bala xwe bide ka çawa ${item.focus.kmr} di têkiliyeke rastîn a C1 de bi kar tînî.`,
    pl: `Przerób połączony materiał o ${item.target[2]} i zwróć uwagę, jak zastosować ${item.focus.pl} w realistycznej interakcji C1.`,
    ro: `Lucrează cu materialul legat despre ${item.target[2]} și observă cum folosești ${item.focus.ro} într-o interacțiune realistă de C1.`,
    sq: `Puno me materialin e lidhur për ${item.target[2]} dhe vëzhgo si përdoret ${item.focus.sq} në një ndërveprim real C1.`
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
    block('practice', 'speak', item.practice, 'none', null, 8, 40, true),
    block('review', 'review', item.review, 'none', null, 6, 50, true)
  ];
}

fs.writeFileSync(file, JSON.stringify(data, null, 2) + '\n', 'utf8');
console.log(`Updated ${items.length} C1 Module 5 lessons with ${items.length * 5} activity blocks.`);

const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c2-stil-souveraenitaet-und-komplexer-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'Pruefungsleistung fokussieren',
    en: 'Focus the exam performance',
    fa: 'عملکرد امتحانی را متمرکز کن',
    ar: 'ركّز أداء الامتحان',
    tr: 'Sınav performansını odakla',
    ru: 'Сфокусируй экзаменационное выступление',
    ckb: 'ئەدای تاقیکردنەوەکە بخە سەر خاڵ',
    kmr: 'Performansa azmûnê bixe navendê',
    pl: 'Skup wykonanie egzaminacyjne',
    ro: 'Focalizează performanța de examen',
    sq: 'Përqendro performancën në provim'
  },
  language: {
    de: 'Deutung und Beleg verbinden',
    en: 'Connect interpretation and evidence',
    fa: 'تفسیر و شاهد را به هم وصل کن',
    ar: 'اربط التأويل بالدليل',
    tr: 'Yorumu kanıtla bağla',
    ru: 'Свяжи интерпретацию и доказательство',
    ckb: 'لێکدانەوە و بەڵگە پێکەوە ببەستە',
    kmr: 'Şîrove û delîlê girê bide',
    pl: 'Połącz interpretację z dowodem',
    ro: 'Leagă interpretarea de dovadă',
    sq: 'Lidhe interpretimin me provën'
  },
  target: {
    de: 'Unter Druck anwenden',
    en: 'Apply it under pressure',
    fa: 'زیر فشار به کار ببر',
    ar: 'طبّق ذلك تحت الضغط',
    tr: 'Baskı altında uygula',
    ru: 'Примени под давлением',
    ckb: 'لە ژێر فشاردا بەکاری بهێنە',
    kmr: 'Di bin zextê de bikar bîne',
    pl: 'Zastosuj pod presją',
    ro: 'Aplică sub presiune',
    sq: 'Zbatoje nën presion'
  },
  transfer: {
    de: 'Antwortlinie verdichten',
    en: 'Condense the response line',
    fa: 'خط پاسخ را فشرده و روشن کن',
    ar: 'كثّف خط الإجابة',
    tr: 'Yanıt çizgisini yoğunlaştır',
    ru: 'Сожми линию ответа',
    ckb: 'هێڵی وەڵامەکە چڕ و ڕوون بکە',
    kmr: 'Xeta bersivê kurt û zelal bike',
    pl: 'Zagęść linię odpowiedzi',
    ro: 'Condensează linia răspunsului',
    sq: 'Përmblidh vijën e përgjigjes'
  },
  review: {
    de: 'Kriterium und Wirkung pruefen',
    en: 'Check criterion and effect',
    fa: 'معیار و اثر پاسخ را بررسی کن',
    ar: 'افحص المعيار والأثر',
    tr: 'Ölçütü ve etkiyi kontrol et',
    ru: 'Проверь критерий и эффект',
    ckb: 'پێوەر و کاریگەری بپشکنە',
    kmr: 'Pîvan û bandorê kontrol bike',
    pl: 'Sprawdź kryterium i efekt',
    ro: 'Verifică criteriul și efectul',
    sq: 'Kontrollo kriterin dhe efektin'
  }
};

const items = [
  {
    slug: 'c2-goethe-c2-lesen-literatur-und-sachtext',
    topic: { de: 'Literatur und Sachtext im Goethe-C2-Lesen', en: 'literature and factual text in Goethe C2 reading', fa: 'متن ادبی و متن غیرداستانی در بخش خواندن Goethe C2', ar: 'النص الأدبي والنص المعلوماتي في قراءة Goethe C2', tr: 'Goethe C2 okuma bölümünde edebiyat ve bilgi metni', ru: 'литература и информационный текст в чтении Goethe C2', ckb: 'دەقی ئەدەبی و دەقی زانیاری لە خوێندنەوەی Goethe C2', kmr: 'nivîsa edebî û agahdarî di xwendina Goethe C2 de', pl: 'literatura i tekst rzeczowy w czytaniu Goethe C2', ro: 'literatură și text informativ la citirea Goethe C2', sq: 'letërsi dhe tekst informues në leximin Goethe C2' },
    focus: { de: 'Textfunktion, Stimme und Beleg zu unterscheiden', en: 'distinguishing text function, voice, and evidence', fa: 'کارکرد متن، صدای نویسنده و شاهد را از هم جدا کنی', ar: 'تمييز وظيفة النص والصوت والدليل', tr: 'metin işlevini, sesi ve kanıtı ayırmak', ru: 'различать функцию текста, голос и доказательство', ckb: 'کاری دەق، دەنگ و بەڵگە لێک جیا بکەیت', kmr: 'karê nivîsê, deng û delîlê ji hev cuda bikî', pl: 'odróżnić funkcję tekstu, głos i dowód', ro: 'să distingi funcția textului, vocea și dovada', sq: 'të dallosh funksionin e tekstit, zërin dhe provën' },
    grammar: 'c2-literary-sentence-structures',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-literatur-und-sachtext-strategisch-lesen'
  },
  {
    slug: 'c2-goethe-c2-hoeren-komplexe-beitraege',
    topic: { de: 'komplexe Hoerbeitraege', en: 'complex listening passages', fa: 'فایل‌ها یا گفتارهای شنیداری پیچیده', ar: 'مقاطع استماع معقدة', tr: 'karmaşık dinleme parçaları', ru: 'сложные аудиофрагменты', ckb: 'بەشی گوێگرتنی ئاڵۆز', kmr: 'parçeyên guhdarîkirinê yên aloz', pl: 'złożone nagrania do słuchania', ro: 'fragmente audio complexe', sq: 'pjesë dëgjimore komplekse' },
    focus: { de: 'Hauptlinie, Nebenbemerkung und Sprecherhaltung getrennt zu sichern', en: 'securing main line, aside, and speaker stance separately', fa: 'خط اصلی، نکته فرعی و موضع گوینده را جداگانه ثبت کنی', ar: 'تثبيت الخط الرئيسي والملاحظة الجانبية وموقف المتكلم كلٌّ على حدة', tr: 'ana çizgiyi, yan notu ve konuşmacı tutumunu ayrı ayrı yakalamak', ru: 'отдельно удерживать основную линию, побочное замечание и позицию говорящего', ckb: 'هێڵی سەرەکی، تێبینی لاوەکی و هەڵوێستی قسەکەر جیاجیا بگریت', kmr: 'xeta sereke, nîşeya alî û helwesta axaftvan cuda cuda bigirî', pl: 'oddzielnie uchwycić główną linię, uwagę poboczną i postawę mówiącego', ro: 'să reții separat linia principală, observația secundară și atitudinea vorbitorului', sq: 'të kapësh veçmas vijën kryesore, shënimin anësor dhe qëndrimin e folësit' },
    grammar: 'c2-implicit-references-and-cohesion',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-komplexe-hoerbeitraege-verdichten'
  },
  {
    slug: 'c2-goethe-c2-schreiben-argument-und-stil',
    topic: { de: 'Argument und Stil im Schreiben', en: 'argument and style in writing', fa: 'استدلال و سبک در نوشتن', ar: 'الحجة والأسلوب في الكتابة', tr: 'yazıda argüman ve üslup', ru: 'аргумент и стиль в письме', ckb: 'بەڵگەهێنان و ستایل لە نووسیندا', kmr: 'arguman û şêwaz di nivîsê de', pl: 'argument i styl w pisaniu', ro: 'argument și stil în scriere', sq: 'argument dhe stil në shkrim' },
    focus: { de: 'klare These, kontrollierten Stil und Gegenposition zusammenzufuehren', en: 'bringing clear thesis, controlled style, and opposing position together', fa: 'تز روشن، سبک کنترل‌شده و موضع مخالف را کنار هم بیاوری', ar: 'جمع الأطروحة الواضحة والأسلوب المضبوط والموقف المقابل', tr: 'açık tezi, kontrollü üslubu ve karşı pozisyonu birleştirmek', ru: 'соединить ясный тезис, контролируемый стиль и противоположную позицию', ckb: 'تێزی ڕوون، ستایلی کۆنترۆڵکراو و هەڵوێستی دژ پێکەوە بهێنیت', kmr: 'tezê zelal, şêwaza kontrolkirî û helwesta dijber bigihînî hev', pl: 'połączyć jasną tezę, kontrolowany styl i stanowisko przeciwne', ro: 'să aduci împreună teza clară, stilul controlat și poziția opusă', sq: 'të bashkosh tezën e qartë, stilin e kontrolluar dhe pozicionin kundërshtar' },
    grammar: 'c2-c2-formal-writing-grammar',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-argument-und-stil-im-schreiben-steuern'
  },
  {
    slug: 'c2-goethe-c2-sprechen-spontan-und-praezise',
    topic: { de: 'spontanes und praezises Sprechen', en: 'spontaneous and precise speaking', fa: 'صحبت خودجوش و دقیق', ar: 'التحدث العفوي والدقيق', tr: 'spontane ve kesin konuşma', ru: 'спонтанная и точная речь', ckb: 'قسەکردنی خۆڕسک و ورد', kmr: 'axaftina spontan û daqîq', pl: 'spontaniczne i precyzyjne mówienie', ro: 'vorbire spontană și precisă', sq: 'të folur spontan dhe i saktë' },
    focus: { de: 'schnell zu reagieren, ohne Praezision oder Hoeflichkeit zu verlieren', en: 'reacting quickly without losing precision or politeness', fa: 'سریع واکنش نشان بدهی، بدون از دست دادن دقت یا ادب', ar: 'الرد بسرعة من دون فقدان الدقة أو اللباقة', tr: 'hızlı tepki vermek ama kesinliği ya da nezaketi kaybetmemek', ru: 'быстро реагировать, не теряя точности или вежливости', ckb: 'بە خێرایی وەڵام بدەیتەوە بەبێ لەدەستدانی وردی یان ڕێز', kmr: 'zû bersiv bidî bêyî ku daqîqî an rêzdarî winda bikî', pl: 'szybko reagować bez utraty precyzji lub uprzejmości', ro: 'să reacționezi rapid fără să pierzi precizia sau politețea', sq: 'të reagosh shpejt pa humbur saktësinë ose mirësjelljen' },
    grammar: 'c2-rhetorical-sentence-structures',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-spontane-nachfragen-praezise-beantworten'
  },
  {
    slug: 'c2-literarische-position-diskutieren',
    topic: { de: 'eine literarische Position', en: 'a literary position', fa: 'یک موضع یا برداشت ادبی', ar: 'موقف أو قراءة أدبية', tr: 'edebi bir pozisyon', ru: 'литературная позиция', ckb: 'هەڵوێستێکی ئەدەبی', kmr: 'helwesteke edebî', pl: 'stanowisko literackie', ro: 'o poziție literară', sq: 'një pozicion letrar' },
    focus: { de: 'Deutung zu vertreten, ohne den Text zu ueberdehnen', en: 'defending an interpretation without overstretching the text', fa: 'برداشتت را دفاع کنی، بدون اینکه بیش از ظرفیت متن ادعا کنی', ar: 'الدفاع عن التأويل من دون تحميل النص أكثر مما يحتمل', tr: 'yorumu savunmak ama metni aşırı zorlamamak', ru: 'защищать интерпретацию, не перегружая текст сверх меры', ckb: 'لێکدانەوەکەت بپارێزیت بەبێ ئەوەی زیاتر لە توانای دەقەکە بانگەشە بکەیت', kmr: 'şîroveya xwe biparêzî bêyî ku nivîsê zêde bikêşî', pl: 'bronić interpretacji bez przeciążania tekstu', ro: 'să aperi interpretarea fără să forțezi textul', sq: 'të mbrosh interpretimin pa e tepruar me tekstin' },
    grammar: 'c2-literary-sentence-structures',
    targetType: 'roleplay',
    targetSlug: 'c2-goethe-c2-eine-literarische-position-diskutieren'
  },
  {
    slug: 'c2-kulturelle-these-verteidigen',
    topic: { de: 'eine kulturelle These', en: 'a cultural thesis', fa: 'یک تز فرهنگی', ar: 'أطروحة ثقافية', tr: 'kültürel bir tez', ru: 'культурный тезис', ckb: 'تێزێکی کەلتووری', kmr: 'tezeke çandî', pl: 'teza kulturowa', ro: 'o teză culturală', sq: 'një tezë kulturore' },
    focus: { de: 'Kulturbeobachtung zu begruenden, ohne Klischees zu reproduzieren', en: 'justifying a cultural observation without reproducing clichés', fa: 'مشاهده فرهنگی را توضیح بدهی، بدون بازتولید کلیشه‌ها', ar: 'تعليل ملاحظة ثقافية من دون إعادة إنتاج الصور النمطية', tr: 'kültürel gözlemi gerekçelendirmek ama klişeleri yeniden üretmemek', ru: 'обосновать культурное наблюдение, не воспроизводя клише', ckb: 'تێبینی کەلتووری ڕوون بکەیت بەبێ دووبارەکردنەوەی کلیشەکان', kmr: 'çavdêriya çandî bi sedem bikî bêyî ku klîşeyan dubare bikî', pl: 'uzasadnić obserwację kulturową bez reprodukowania stereotypów', ro: 'să justifici o observație culturală fără să reproduci clișee', sq: 'të arsyetosh një vëzhgim kulturor pa riprodhuar klishe' },
    grammar: 'c2-register-and-syntactic-choice',
    targetType: 'roleplay',
    targetSlug: 'c2-goethe-c2-eine-kulturelle-these-verteidigen'
  },
  {
    slug: 'c2-bildbeschreibung-mit-gesellschaftlicher-deutung',
    topic: { de: 'Bildbeschreibung mit gesellschaftlicher Deutung', en: 'image description with social interpretation', fa: 'توصیف تصویر همراه با تفسیر اجتماعی', ar: 'وصف صورة مع تأويل اجتماعي', tr: 'toplumsal yorumla görsel betimleme', ru: 'описание изображения с социальной интерпретацией', ckb: 'وەسفی وێنە لەگەڵ لێکدانەوەی کۆمەڵایەتی', kmr: 'şiroveya wêne bi şîroveya civakî', pl: 'opis obrazu z interpretacją społeczną', ro: 'descriere de imagine cu interpretare socială', sq: 'përshkrim figure me interpretim shoqëror' },
    focus: { de: 'sichtbare Details und gesellschaftliche Deutung sauber zu verbinden', en: 'connecting visible details and social interpretation cleanly', fa: 'جزئیات قابل دیدن و تفسیر اجتماعی را دقیق به هم وصل کنی', ar: 'ربط التفاصيل المرئية بالتأويل الاجتماعي بدقة', tr: 'görünen ayrıntılarla toplumsal yorumu temiz bağlamak', ru: 'точно связать видимые детали и социальную интерпретацию', ckb: 'وردەکارییە بینراوەکان و لێکدانەوەی کۆمەڵایەتی بە وردی پێکەوە ببەستیت', kmr: 'hûrgiliyên xuya û şîroveya civakî bi paqijî girê bidî', pl: 'czysto połączyć widoczne szczegóły z interpretacją społeczną', ro: 'să legi curat detaliile vizibile de interpretarea socială', sq: 'të lidhësh qartë detajet e dukshme me interpretimin shoqëror' },
    grammar: 'c2-literary-sentence-structures',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-bildbeschreibung-mit-gesellschaftlicher-deutung-verbinden'
  },
  {
    slug: 'c2-eigenen-fehler-elegant-korrigieren',
    topic: { de: 'einen eigenen Fehler in der Pruefung', en: 'your own mistake in the exam', fa: 'اشتباه خودت در امتحان', ar: 'خطؤك في الامتحان', tr: 'sınavdaki kendi hatan', ru: 'собственная ошибка на экзамене', ckb: 'هەڵەی خۆت لە تاقیکردنەوەدا', kmr: 'şaşiya xwe di azmûnê de', pl: 'własny błąd na egzaminie', ro: 'propria greșeală la examen', sq: 'gabimi yt në provim' },
    focus: { de: 'Fehler zu korrigieren, ohne deine ganze Antwort zu destabilisieren', en: 'correcting the mistake without destabilizing your whole answer', fa: 'اشتباه را اصلاح کنی، بدون اینکه کل پاسخ را به هم بریزی', ar: 'تصحيح الخطأ من دون زعزعة إجابتك كلها', tr: 'hatayı düzeltmek ama tüm yanıtı sarsmamak', ru: 'исправить ошибку, не разрушая весь ответ', ckb: 'هەڵەکە ڕاست بکەیتەوە بەبێ تێکدانی هەموو وەڵامەکەت', kmr: 'şaşiyê rast bikî bêyî ku hemû bersiva xwe bêîstiqrar bikî', pl: 'poprawić błąd bez destabilizacji całej odpowiedzi', ro: 'să corectezi greșeala fără să destabilizezi tot răspunsul', sq: 'ta korrigjosh gabimin pa e tronditur gjithë përgjigjen' },
    grammar: 'c2-c2-common-pitfalls',
    targetType: 'roleplay',
    targetSlug: 'c2-in-einer-pruefung-einen-eigenen-fehler-elegant-korrigieren'
  },
  {
    slug: 'c2-standpunkt-in-zwei-minuten-verdichten',
    topic: { de: 'einen Standpunkt in zwei Minuten', en: 'a position in two minutes', fa: 'بیان یک موضع در دو دقیقه', ar: 'عرض موقف في دقيقتين', tr: 'iki dakikada bir pozisyon', ru: 'позиция за две минуты', ckb: 'هەڵوێستێک لە دوو خولەکدا', kmr: 'helwestek di du deqîqeyan de', pl: 'stanowisko w dwie minuty', ro: 'o poziție în două minute', sq: 'një qëndrim në dy minuta' },
    focus: { de: 'These, Beispiel und Grenze knapp, aber nicht flach zu setzen', en: 'placing thesis, example, and limit briefly but not shallowly', fa: 'تز، مثال و مرز را کوتاه اما سطحی‌نشده بیان کنی', ar: 'وضع الأطروحة والمثال والحد بإيجاز من دون سطحية', tr: 'tez, örnek ve sınırı kısa ama yüzeysel olmayan biçimde kurmak', ru: 'кратко, но не поверхностно задать тезис, пример и границу', ckb: 'تێز، نموونە و سنوور بە کورتی بەڵام نە بە ڕووکەشی دابنێیت', kmr: 'tez, nimûne û sînorê kurt lê ne sivik saz bikî', pl: 'krótko, ale nie płytko ustawić tezę, przykład i granicę', ro: 'să fixezi teza, exemplul și limita concis, dar nu superficial', sq: 'të vendosësh tezën, shembullin dhe kufirin shkurt, por jo cekët' },
    grammar: 'c2-rhetorical-sentence-structures',
    targetType: 'roleplay',
    targetSlug: 'c2-einen-komplexen-standpunkt-in-zwei-minuten-verdichten'
  },
  {
    slug: 'c2-pruefung-literatur-kultur-und-hochleistung-wiederholen',
    topic: { de: 'Pruefung, Literatur, Kultur und Hochleistung', en: 'exam, literature, culture, and high performance', fa: 'امتحان، ادبیات، فرهنگ و عملکرد سطح بالا', ar: 'الامتحان والأدب والثقافة والأداء العالي', tr: 'sınav, edebiyat, kültür ve yüksek performans', ru: 'экзамен, литература, культура и высокая результативность', ckb: 'تاقیکردنەوە، ئەدەب، کەلتوور و ئەدای بەرز', kmr: 'azmûn, edebiyat, çand û performansa bilind', pl: 'egzamin, literatura, kultura i wysoka sprawność', ro: 'examen, literatură, cultură și performanță înaltă', sq: 'provim, letërsi, kulturë dhe performancë e lartë' },
    focus: { de: 'unter Zeitdruck eine belegte, stilistisch kontrollierte Antwortlinie zu halten', en: 'holding an evidenced, stylistically controlled response line under time pressure', fa: 'زیر فشار زمان خط پاسخ مستند و از نظر سبک کنترل‌شده را نگه داری', ar: 'الحفاظ تحت ضغط الوقت على خط إجابة مدعوم ومضبوط أسلوبيًا', tr: 'zaman baskısı altında kanıtlı ve üslupça kontrollü yanıt çizgisini korumak', ru: 'под давлением времени удерживать доказательную и стилистически контролируемую линию ответа', ckb: 'لە ژێر فشاری کاتدا هێڵی وەڵامی بەڵگەدار و ستایلی کۆنترۆڵکراو بپارێزیت', kmr: 'di bin zexta demê de xeta bersiva bi delîl û şêwazî kontrolkirî biparêzî', pl: 'utrzymać pod presją czasu udokumentowaną i stylistycznie kontrolowaną linię odpowiedzi', ro: 'să menții sub presiunea timpului o linie de răspuns dovedită și controlată stilistic', sq: 'të mbash nën presion kohe një vijë përgjigjeje me prova dhe stil të kontrolluar' },
    grammar: 'c2-c2-grammar-review-map',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-abschlussstrategie-fuer-hochrisikoteile-planen'
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
    de: `Lies die Lesson und notiere, welches Kriterium bei ${item.topic.de} entscheidet: Textbeleg, Struktur, Spontaneitaet, Stil oder Zeitkontrolle.`,
    en: `Read the lesson and note which criterion decides in ${item.topic.en}: textual evidence, structure, spontaneity, style, or time control.`,
    fa: `درس را بخوان و یادداشت کن در موضوع ${item.topic.fa} کدام معیار تعیین‌کننده است: شاهد متنی، ساختار، واکنش خودجوش، سبک یا کنترل زمان.`,
    ar: `اقرأ الدرس وسجّل أي معيار يحسم في موضوع ${item.topic.ar}: دليل من النص، البنية، العفوية، الأسلوب أو ضبط الوقت.`,
    tr: `Dersi oku ve ${item.topic.tr} konusunda hangi ölçütün belirleyici olduğunu not et: metin kanıtı, yapı, spontanlık, üslup ya da zaman kontrolü.`,
    ru: `Прочитай урок и отметь, какой критерий решает в теме ${item.topic.ru}: текстовое доказательство, структура, спонтанность, стиль или контроль времени.`,
    ckb: `وانەکە بخوێنەوە و بنووسە لە بابەتی ${item.topic.ckb} کام پێوەر بڕیار دەدات: بەڵگەی دەق، پێکهاتە، خۆڕسکی، ستایل یان کۆنترۆڵی کات.`,
    kmr: `Dersê bixwîne û binivîse di mijara ${item.topic.kmr} de kîjan pîvan diyarker e: delîla nivîsê, avahî, spontanî, şêwaz an kontrola demê.`,
    pl: `Przeczytaj lekcję i zanotuj, które kryterium decyduje w temacie ${item.topic.pl}: dowód z tekstu, struktura, spontaniczność, styl czy kontrola czasu.`,
    ro: `Citește lecția și notează ce criteriu decide în tema ${item.topic.ro}: dovada textuală, structura, spontaneitatea, stilul sau controlul timpului.`,
    sq: `Lexoje mësimin dhe shëno cili kriter vendos në temën ${item.topic.sq}: prova nga teksti, struktura, spontaniteti, stili apo kontrolli i kohës.`
  };
}

function languageInstruction(item) {
  return {
    de: `Oeffne den verlinkten Grammatikpunkt. Waehle eine Struktur, mit der du ${item.focus.de}. Vermeide eine Formulierung, die nur gebildet klingt, aber nichts belegt.`,
    en: `Open the linked grammar point. Choose one structure that helps you ${item.focus.en}. Avoid a formulation that merely sounds educated but proves nothing.`,
    fa: `نکته گرامری لینک‌شده را باز کن. یک ساختار انتخاب کن که کمک کند ${item.focus.fa}. از عبارتی پرهیز کن که فقط فرهیخته به نظر می‌رسد اما چیزی را ثابت نمی‌کند.`,
    ar: `افتح نقطة القواعد المرتبطة. اختر بنية تساعدك على أن ${item.focus.ar}. تجنب صياغة تبدو مثقفة فقط لكنها لا تثبت شيئًا.`,
    tr: `Bağlantılı dil bilgisi noktasını aç. ${item.focus.tr} için yardımcı olan bir yapı seç. Sadece bilgili görünen ama hiçbir şeyi kanıtlamayan ifadeden kaçın.`,
    ru: `Открой связанный грамматический пункт. Выбери структуру, которая помогает ${item.focus.ru}. Избегай формулировки, которая лишь звучит образованно, но ничего не доказывает.`,
    ckb: `خاڵی ڕێزمانی بەستەرکراو بکەرەوە. پێکهاتەیەک هەڵبژێرە کە یارمەتیت بدات ${item.focus.ckb}. لە دەربڕینێک دوور بکەوە کە تەنها زاناوانە دەردەکەوێت بەڵام هیچ شتێک ناسەلمێنێت.`,
    kmr: `Xala rêzimanê ya girêdayî veke. Avahiyek hilbijêre ku alîkar e ${item.focus.kmr}. Ji gotinekê dûr bikeve ku tenê zana xuya dike lê tiştekî nîşan nade.`,
    pl: `Otwórz podlinkowany punkt gramatyczny. Wybierz strukturę, która pomoże ci ${item.focus.pl}. Unikaj sformułowania, które tylko brzmi uczono, ale niczego nie dowodzi.`,
    ro: `Deschide punctul de gramatică legat. Alege o structură care te ajută să ${item.focus.ro}. Evită o formulare care doar sună cultivat, dar nu dovedește nimic.`,
    sq: `Hap pikën gramatikore të lidhur. Zgjidh një strukturë që të ndihmon ${item.focus.sq}. Shmang një formulim që vetëm tingëllon i ditur, por nuk provon asgjë.`
  };
}

function targetInstruction(item) {
  return {
    de: `Bearbeite die verlinkte Ressource unter Zeitdruck. Pruefe danach, ob dein Ergebnis das Ziel trifft: ${item.focus.de}. Markiere eine Stelle, die zu breit, zu vage oder zu ornamental wirkt.`,
    en: `Work through the linked resource under time pressure. Then check whether your result meets the goal: ${item.focus.en}. Mark one place that feels too broad, too vague, or too ornamental.`,
    fa: `منبع لینک‌شده را با محدودیت زمانی انجام بده. بعد بررسی کن آیا نتیجه به هدف می‌رسد: ${item.focus.fa}. یک بخش را مشخص کن که بیش از حد گسترده، مبهم یا تزئینی است.`,
    ar: `اعمل على المورد المرتبط تحت ضغط الوقت. ثم تحقق هل يحقق الناتج الهدف: ${item.focus.ar}. حدّد موضعًا يبدو واسعًا جدًا أو غامضًا جدًا أو زخرفيًا جدًا.`,
    tr: `Bağlantılı kaynağı zaman baskısı altında çalış. Sonra sonucun hedefe ulaşıp ulaşmadığını kontrol et: ${item.focus.tr}. Fazla geniş, belirsiz ya da süslü görünen bir yeri işaretle.`,
    ru: `Проработай связанный ресурс под давлением времени. Затем проверь, достигает ли результат цели: ${item.focus.ru}. Отметь место, которое кажется слишком широким, расплывчатым или декоративным.`,
    ckb: `سەرچاوەی بەستەرکراو لە ژێر فشاری کاتدا کاربکە. پاشان بپشکنە ئایا ئەنجامەکەت دەگاتە ئامانجەکە: ${item.focus.ckb}. شوێنێک دیاری بکە کە زۆر فراوان، ناڕوون یان ڕازاوە دەردەکەوێت.`,
    kmr: `Çavkaniya girêdayî di bin zexta demê de bixebitîne. Paşê kontrol bike gelo encam digihîje armancê: ${item.focus.kmr}. Cihê ku pir fireh, nezelal an xemilandî xuya dike nîşan bike.`,
    pl: `Przerób podlinkowany materiał pod presją czasu. Potem sprawdź, czy wynik trafia w cel: ${item.focus.pl}. Zaznacz miejsce zbyt szerokie, zbyt niejasne albo zbyt ozdobne.`,
    ro: `Lucrează resursa legată sub presiunea timpului. Apoi verifică dacă rezultatul atinge scopul: ${item.focus.ro}. Marchează un loc care pare prea larg, prea vag sau prea ornamental.`,
    sq: `Puno me burimin e lidhur nën presion kohe. Pastaj kontrollo nëse rezultati arrin synimin: ${item.focus.sq}. Shëno një vend që duket tepër i gjerë, tepër i paqartë ose tepër zbukurues.`
  };
}

function transferInstruction() {
  return {
    de: 'Formuliere eine Antwortlinie in drei Schritten: Ausgangspunkt, Beleg oder Beispiel, kontrollierte Zuspitzung. Schreibe sie so, dass sie muendlich und schriftlich tragfaehig bleibt.',
    en: 'Formulate a response line in three steps: starting point, evidence or example, controlled sharpening. Write it so it remains viable both orally and in writing.',
    fa: 'خط پاسخ را در سه قدم بنویس: نقطه شروع، شاهد یا مثال، جمع‌بندی کنترل‌شده. طوری بنویس که هم برای گفتار و هم برای نوشتار قابل استفاده باشد.',
    ar: 'صغ خط إجابة في ثلاث خطوات: نقطة الانطلاق، دليل أو مثال، وتكثيف مضبوط. اكتبه بحيث يبقى صالحًا شفهيًا وكتابيًا.',
    tr: 'Yanıt çizgisini üç adımda kur: çıkış noktası, kanıt ya da örnek, kontrollü keskinleştirme. Hem sözlü hem yazılı taşınabilir olsun.',
    ru: 'Сформулируй линию ответа в три шага: исходная точка, доказательство или пример, контролируемое заострение. Напиши так, чтобы она работала устно и письменно.',
    ckb: 'هێڵی وەڵام لە سێ هەنگاودا بنووسە: خاڵی دەستپێک، بەڵگە یان نموونە، و چڕکردنەوەی کۆنترۆڵکراو. وەها بینووسە کە هەم بە زاری و هەم بە نووسین بەهێز بمێنێت.',
    kmr: 'Xeta bersivê di sê gavan de ava bike: xala destpêkê, delîl an nimûne, û tûjkirina kontrolkirî. Wisa binivîse ku hem devkî hem nivîskî xurt bimîne.',
    pl: 'Sformułuj linię odpowiedzi w trzech krokach: punkt wyjścia, dowód lub przykład, kontrolowane zaostrzenie. Napisz ją tak, aby działała ustnie i pisemnie.',
    ro: 'Formulează linia răspunsului în trei pași: punct de plecare, dovadă sau exemplu, accentuare controlată. Scrie-o astfel încât să funcționeze oral și în scris.',
    sq: 'Formulo vijën e përgjigjes në tri hapa: pikënisje, provë ose shembull, mprehje e kontrolluar. Shkruaje që të funksionojë si me gojë, ashtu edhe me shkrim.'
  };
}

function reviewInstruction(item) {
  return {
    de: `Pruefe deine Antwortlinie fuer ${item.topic.de}: Ist sie belegbar? Ist sie knapp genug? Hat sie C2-Niveau ohne ueberladene Sprache? Streiche eine Formulierung, die nur Eindruck machen soll.`,
    en: `Check your response line for ${item.topic.en}: is it evidence-based? Is it concise enough? Does it have C2 level without overloaded language? Remove one formulation that only tries to impress.`,
    fa: `خط پاسخ خودت را برای موضوع ${item.topic.fa} بررسی کن: آیا مستند است؟ آیا به اندازه کافی فشرده است؟ آیا سطح C2 دارد بدون اینکه زبانش سنگین و نمایشی شود؟ یک عبارت را که فقط برای تأثیرگذاری ظاهری است حذف کن.`,
    ar: `افحص خط إجابتك لموضوع ${item.topic.ar}: هل هو مدعوم بدليل؟ هل هو موجز بما يكفي؟ هل له مستوى C2 من دون لغة مثقلة؟ احذف عبارة هدفها فقط إحداث انطباع.`,
    tr: `${item.topic.tr} için yanıt çizgini kontrol et: kanıtlanabilir mi? Yeterince kısa mı? Aşırı yüklü dil olmadan C2 düzeyinde mi? Sadece etki yaratmaya çalışan bir ifadeyi sil.`,
    ru: `Проверь линию ответа для темы ${item.topic.ru}: есть ли доказательство? Достаточно ли она кратка? Есть ли уровень C2 без перегруженного языка? Убери формулировку, которая только должна производить впечатление.`,
    ckb: `هێڵی وەڵامەکەت بۆ بابەتی ${item.topic.ckb} بپشکنە: ئایا بەڵگەی هەیە؟ ئایا بە پێویستی کورتە؟ ئایا ئاستی C2 هەیە بەبێ زمانێکی قورس و پیشاندان؟ دەربڕینێک بسڕەوە کە تەنها بۆ کاریگەریی ڕووکەشە.`,
    kmr: `Xeta bersiva xwe ji bo mijara ${item.topic.kmr} kontrol bike: delîl heye? Têra xwe kurt e? Asta C2 heye bê zimanê zêde barkirî? Gotinekê jê bibe ku tenê dixwaze bandor bike.`,
    pl: `Sprawdź linię odpowiedzi dla tematu ${item.topic.pl}: czy jest oparta na dowodzie? Czy jest dość zwięzła? Czy ma poziom C2 bez przeciążonego języka? Usuń sformułowanie, które ma tylko robić wrażenie.`,
    ro: `Verifică linia răspunsului pentru tema ${item.topic.ro}: este dovedibilă? Este suficient de concisă? Are nivel C2 fără limbaj încărcat? Elimină o formulare menită doar să impresioneze.`,
    sq: `Kontrollo vijën e përgjigjes për temën ${item.topic.sq}: a ka prova? A është mjaft e përmbledhur? A ka nivel C2 pa gjuhë të mbingarkuar? Hiq një formulim që synon vetëm të bëjë përshtypje.`
  };
}

for (const item of items) {
  const lesson = lessons.find(l => l.slug === item.slug);
  if (!lesson) {
    throw new Error(`Lesson not found: ${item.slug}`);
  }

  const kind = item.targetType === 'roleplay' ? 'roleplay' : item.targetType === 'writing-template' ? 'write' : 'exam-prep';
  lesson.activityBlocks = [
    createActivity('read', 'orient', orientInstruction(item), 'none', null, 6, 10),
    createActivity('grammar', 'language', languageInstruction(item), 'grammar-topic', item.grammar, 8, 20),
    createActivity(kind, 'target', targetInstruction(item), item.targetType, item.targetSlug, 10, 30),
    createActivity('practice', 'transfer', transferInstruction(), 'none', null, 8, 40),
    createActivity('review', 'review', reviewInstruction(item), 'none', null, 6, 50)
  ];
}

fs.writeFileSync(file, JSON.stringify(data, null, 2) + '\n', 'utf8');
console.log(`Updated ${items.length} C2 Module 10 lessons with ${items.length * 5} activity blocks.`);

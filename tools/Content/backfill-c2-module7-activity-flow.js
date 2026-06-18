const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c2-stil-souveraenitaet-und-komplexer-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'Streitpunkt praezisieren',
    en: 'Make the point of controversy precise',
    fa: 'نقطه اختلاف را دقیق کن',
    ar: 'حدّد نقطة الخلاف بدقة',
    tr: 'Tartışma noktasını kesinleştir',
    ru: 'Уточни предмет спора',
    ckb: 'خاڵی ناکۆکی بە وردی دیاری بکە',
    kmr: 'Xala nakokiyê bi daqîqî zelal bike',
    pl: 'Doprecyzuj punkt sporny',
    ro: 'Precizează punctul controversat',
    sq: 'Saktëso pikën e debatit'
  },
  language: {
    de: 'Wertung und Distanz trennen',
    en: 'Separate evaluation and distance',
    fa: 'ارزیابی و فاصله‌گیری را از هم جدا کن',
    ar: 'افصل بين التقييم واتخاذ المسافة',
    tr: 'Değerlendirme ile mesafeyi ayır',
    ru: 'Раздели оценку и дистанцию',
    ckb: 'هەڵسەنگاندن و دوورگرتنەوە لێک جیا بکە',
    kmr: 'Nirxandin û dûrbûnê ji hev cuda bike',
    pl: 'Oddziel ocenę od dystansu',
    ro: 'Separă evaluarea de distanțare',
    sq: 'Ndaje vlerësimin nga distancimi'
  },
  target: {
    de: 'Oeffentlich argumentieren',
    en: 'Argue in public discourse',
    fa: 'در فضای عمومی استدلال کن',
    ar: 'حاجج في الخطاب العام',
    tr: 'Kamusal söylemde argüman kur',
    ru: 'Аргументируй в публичном дискурсе',
    ckb: 'لە گوتاری گشتیدا بەڵگە بهێنەوە',
    kmr: 'Di gotara giştî de arguman ava bike',
    pl: 'Argumentuj w dyskursie publicznym',
    ro: 'Argumentează în discursul public',
    sq: 'Argumento në diskursin publik'
  },
  transfer: {
    de: 'Gegenposition fair einbauen',
    en: 'Integrate the opposing position fairly',
    fa: 'موضع مخالف را منصفانه وارد کن',
    ar: 'أدخل الموقف المقابل بإنصاف',
    tr: 'Karşı pozisyonu adil biçimde dahil et',
    ru: 'Справедливо включи противоположную позицию',
    ckb: 'هەڵوێستی دژ بە دادپەروەری تێبکە',
    kmr: 'Helwesta dijber bi adilî têxe navê',
    pl: 'Uczciwie włącz stanowisko przeciwne',
    ro: 'Integrează corect poziția opusă',
    sq: 'Përfshi me drejtësi pozicionin kundërshtar'
  },
  review: {
    de: 'Sachlichkeit pruefen',
    en: 'Check objectivity',
    fa: 'بی‌طرفی و دقت را بررسی کن',
    ar: 'افحص الموضوعية والدقة',
    tr: 'Nesnelliği ve kesinliği kontrol et',
    ru: 'Проверь объективность и точность',
    ckb: 'بابەتیبوون و وردی بپشکنە',
    kmr: 'Babetîbûn û daqîqiyê kontrol bike',
    pl: 'Sprawdź rzeczowość i precyzję',
    ro: 'Verifică obiectivitatea și precizia',
    sq: 'Kontrollo paanshmërinë dhe saktësinë'
  }
};

const items = [
  {
    slug: 'c2-podiumsdiskussion-mit-kontroverse-leiten',
    topic: { de: 'eine kontroverse Podiumsdiskussion', en: 'a controversial panel discussion', fa: 'یک میزگرد بحث‌برانگیز', ar: 'نقاش منصة مثير للجدل', tr: 'tartışmalı bir panel', ru: 'спорная панельная дискуссия', ckb: 'پانێڵێکی گفتوگۆی ناکۆکیدار', kmr: 'panelake nîqaşê ya nakokîdar', pl: 'kontrowersyjna debata panelowa', ro: 'o dezbatere de panel controversată', sq: 'një diskutim paneli me debat' },
    focus: { de: 'Stimmen zu ordnen, ohne die Kontroverse kuenstlich zu glaetten', en: 'ordering voices without artificially smoothing the controversy', fa: 'صداها و دیدگاه‌ها را مرتب کنی، بدون اینکه اختلاف را مصنوعی صاف و بی‌خطر نشان بدهی', ar: 'ترتيب الأصوات من دون تلطيف الخلاف بشكل مصطنع', tr: 'sesleri düzenlemek ama tartışmayı yapay biçimde yumuşatmamak', ru: 'упорядочить голоса, не сглаживая спор искусственно', ckb: 'دەنگەکان ڕێکبخەیت بەبێ ئەوەی ناکۆکییەکە بە ساختەیی نەرم بکەیت', kmr: 'dengan rêz bikî bêyî ku nakokiyê bi awayekî çêkirî nerm bikî', pl: 'uporządkować głosy bez sztucznego wygładzania kontrowersji', ro: 'să ordonezi vocile fără să netezești artificial controversa', sq: 'të rendisësh zërat pa e zbutur artificialisht kundërshtinë' },
    grammar: 'c2-c2-debate-grammar',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-podiumsdiskussion-mit-kontroverse-leiten'
  },
  {
    slug: 'c2-politische-position-differenziert-verteidigen',
    topic: { de: 'eine politische Position', en: 'a political position', fa: 'یک موضع سیاسی', ar: 'موقف سياسي', tr: 'siyasi bir pozisyon', ru: 'политическая позиция', ckb: 'هەڵوێستێکی سیاسی', kmr: 'helwesteke siyasî', pl: 'stanowisko polityczne', ro: 'o poziție politică', sq: 'një qëndrim politik' },
    focus: { de: 'ueberzeugt zu bleiben, ohne Gegenargumente als Dummheit abzuwerten', en: 'remaining convinced without dismissing counterarguments as stupidity', fa: 'باورمند بمانی، بدون اینکه استدلال مخالف را احمقانه یا بی‌ارزش جلوه بدهی', ar: 'البقاء مقتنعًا من دون تحقير الحجج المقابلة بوصفها غباءً', tr: 'ikna olmuş kalmak ama karşı argümanları aptallık diye küçümsememek', ru: 'оставаться убежденным, не списывая контраргументы на глупость', ckb: 'باوەڕدار بمێنیت بەبێ ئەوەی بەڵگەی دژ بە گێلی یان بێ‌بەها دابنێیت', kmr: 'bawermend bimînî bêyî ku argumanên dijber wek bêaqilî biçûk bikî', pl: 'pozostać przekonanym bez zbywania kontrargumentów jako głupoty', ro: 'să rămâi convins fără să respingi contraargumentele ca prostie', sq: 'të mbetesh i bindur pa i përçmuar kundërargumentet si marrëzi' },
    grammar: 'c2-register-and-syntactic-choice',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-politische-position-differenziert-verteidigen'
  },
  {
    slug: 'c2-medienbeitrag-kritisch-einordnen',
    topic: { de: 'einen Medienbeitrag', en: 'a media piece', fa: 'یک مطلب رسانه‌ای', ar: 'مادة إعلامية', tr: 'bir medya içeriği', ru: 'медиаматериал', ckb: 'بابەتێکی میدیایی', kmr: 'nivîsek an bernameyek medyayî', pl: 'materiał medialny', ro: 'un material media', sq: 'një material mediatik' },
    focus: { de: 'These, Auswahl und Ton zu unterscheiden, ohne pauschal Misstrauen zu erzeugen', en: 'distinguishing thesis, selection, and tone without creating blanket mistrust', fa: 'تز، انتخاب داده‌ها و لحن را جدا کنی، بدون اینکه بی‌اعتمادی کلی و بی‌دلیل بسازی', ar: 'التمييز بين الأطروحة والاختيار والنبرة من دون خلق عدم ثقة عام', tr: 'tez, seçim ve tonu ayırmak ama genel bir güvensizlik yaratmamak', ru: 'различать тезис, отбор и тон, не создавая общего недоверия', ckb: 'تێز، هەڵبژاردن و تۆن لێک جیا بکەیتەوە بەبێ دروستکردنی بێ‌باوەڕی گشتی', kmr: 'tez, hilbijartin û tonê ji hev cuda bikî bêyî ku bêbaweriyeke giştî çê bikî', pl: 'odróżnić tezę, dobór i ton bez tworzenia ogólnej nieufności', ro: 'să distingi teza, selecția și tonul fără să creezi neîncredere generală', sq: 'të dallosh tezën, përzgjedhjen dhe tonin pa krijuar mosbesim të përgjithshëm' },
    grammar: 'c2-journalistic-compression',
    targetType: 'roleplay',
    targetSlug: 'c2-einen-medienbeitrag-kritisch-einordnen'
  },
  {
    slug: 'c2-gesellschaftliche-zumutung-abwaegen',
    topic: { de: 'eine gesellschaftliche Zumutung', en: 'a social imposition', fa: 'یک فشار یا مطالبه سنگین اجتماعی', ar: 'عبء أو مطلب اجتماعي ثقيل', tr: 'toplumsal bir yük veya dayatma', ru: 'общественное требование, воспринимаемое как нагрузка', ckb: 'داواکارییەکی قورس یان فشاری کۆمەڵایەتی', kmr: 'bar an daxwazeke civakî ya giran', pl: 'społeczne obciążenie lub wymaganie', ro: 'o povară sau cerință socială dificilă', sq: 'një barrë ose kërkesë e rëndë shoqërore' },
    focus: { de: 'Belastung anzuerkennen und trotzdem die Gegenfrage nach Gemeinwohl zu stellen', en: 'acknowledging burden while still asking the counter-question about the common good', fa: 'سنگینی فشار را بپذیری و هم‌زمان پرسش مربوط به خیر عمومی را هم مطرح کنی', ar: 'الاعتراف بالعبء مع طرح السؤال المقابل عن الصالح العام', tr: 'yükü kabul etmek ve yine de ortak yarar sorusunu sormak', ru: 'признать нагрузку и при этом поставить встречный вопрос об общем благе', ckb: 'قورساییەکە بپذێریت و هەمان کات پرسیاری بەرژەوەندی گشتی بکەیت', kmr: 'barê qebûl bikî û di heman demê de pirsa berjewendiya giştî bikî', pl: 'uznać obciążenie, a jednocześnie postawić pytanie o dobro wspólne', ro: 'să recunoști povara și totuși să pui întrebarea despre binele comun', sq: 'ta pranosh barrën dhe njëkohësisht të shtrosh pyetjen për të mirën e përbashkët' },
    grammar: 'c2-fine-differences-in-connectors',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-gesellschaftliche-zumutung-abwaegen'
  },
  {
    slug: 'c2-wissenschaftsskepsis-sachlich-besprechen',
    topic: { de: 'Wissenschaftsskepsis', en: 'skepticism toward science', fa: 'بدبینی یا تردید نسبت به علم', ar: 'الشك تجاه العلم', tr: 'bilime yönelik kuşku', ru: 'скепсис по отношению к науке', ckb: 'گومان نسبت بە زانست', kmr: 'guman li hember zanistê', pl: 'sceptycyzm wobec nauki', ro: 'scepticismul față de știință', sq: 'skepticizmi ndaj shkencës' },
    focus: { de: 'Unsicherheit ernst zu nehmen, ohne Belege und Methode preiszugeben', en: 'taking uncertainty seriously without giving up evidence and method', fa: 'ابهام و تردید را جدی بگیری، بدون اینکه شواهد و روش علمی را کنار بگذاری', ar: 'أخذ عدم اليقين بجدية من دون التخلي عن الأدلة والمنهج', tr: 'belirsizliği ciddiye almak ama kanıt ve yöntemi bırakmamak', ru: 'серьезно относиться к неопределенности, не отказываясь от доказательств и метода', ckb: 'نادڵنیایی بە جدی وەربگریت بەبێ وازهێنان لە بەڵگە و میتۆد', kmr: 'nediyarbûnê ciddî bigirî bêyî ku ji delîl û rêbazê vaz bî', pl: 'traktować niepewność poważnie bez rezygnacji z dowodów i metody', ro: 'să iei incertitudinea în serios fără să renunți la dovezi și metodă', sq: 'ta marrësh seriozisht pasigurinë pa hequr dorë nga provat dhe metoda' },
    grammar: 'c2-complex-reported-speech',
    targetType: 'roleplay',
    targetSlug: 'c2-ueber-wissenschaftsskepsis-sachlich-sprechen'
  },
  {
    slug: 'c2-kulturelle-aneignung-nuanciert-diskutieren',
    topic: { de: 'kulturelle Aneignung', en: 'cultural appropriation', fa: 'استفاده بحث‌برانگیز از عناصر فرهنگ دیگران', ar: 'الاستحواذ الثقافي', tr: 'kültürel sahiplenme', ru: 'культурная апроприация', ckb: 'وەرگرتنی کەلتووری بە شێوەی ناکۆکیدار', kmr: 'desteserkirina çandî', pl: 'zawłaszczenie kulturowe', ro: 'aproprierea culturală', sq: 'përvetësimi kulturor' },
    focus: { de: 'Verletzung, Austausch und Machtverhaeltnis getrennt zu betrachten', en: 'considering harm, exchange, and power relations separately', fa: 'آسیب، تبادل فرهنگی و رابطه قدرت را جداگانه بررسی کنی', ar: 'النظر إلى الأذى والتبادل وعلاقات القوة كلٌّ على حدة', tr: 'zarar, alışveriş ve güç ilişkisini ayrı ayrı ele almak', ru: 'рассматривать вред, обмен и отношения власти раздельно', ckb: 'ئازار، ئاڵوگۆڕ و پەیوەندی هێز جیاجیا لێک بدەیتەوە', kmr: 'zirar, danûstandin û têkiliya hêzê cuda cuda binirxînî', pl: 'oddzielnie rozważyć krzywdę, wymianę i relację władzy', ro: 'să analizezi separat vătămarea, schimbul și raportul de putere', sq: 'të shqyrtosh veçmas dëmin, shkëmbimin dhe raportin e pushtetit' },
    grammar: 'c2-register-and-syntactic-choice',
    targetType: 'roleplay',
    targetSlug: 'c2-ueber-kulturelle-aneignung-nuanciert-diskutieren'
  },
  {
    slug: 'c2-kontroverse-statistik-interpretieren',
    topic: { de: 'eine kontroverse Statistik', en: 'a controversial statistic', fa: 'یک آمار بحث‌برانگیز', ar: 'إحصائية مثيرة للجدل', tr: 'tartışmalı bir istatistik', ru: 'спорная статистика', ckb: 'ئامارێکی ناکۆکیدار', kmr: 'statîstîkeke nakokîdar', pl: 'kontrowersyjna statystyka', ro: 'o statistică controversată', sq: 'një statistikë e diskutueshme' },
    focus: { de: 'Zahl, Bezugsrahmen und Deutung auseinanderzuhalten', en: 'keeping number, frame of reference, and interpretation apart', fa: 'عدد، چارچوب مقایسه و تفسیر را از هم جدا نگه داری', ar: 'الفصل بين الرقم وإطار المرجع والتفسير', tr: 'sayıyı, referans çerçevesini ve yorumu ayırmak', ru: 'развести число, рамку сравнения и интерпретацию', ckb: 'ژمارە، چوارچێوەی بەراورد و لێکدانەوە لێک جیا بهێڵیتەوە', kmr: 'hejmar, çarçoveya berawirdê û şîrove ji hev cuda bihêlî', pl: 'oddzielić liczbę, ramę odniesienia i interpretację', ro: 'să ții separat numărul, cadrul de referință și interpretarea', sq: 'të ndash numrin, kornizën e krahasimit dhe interpretimin' },
    grammar: 'c2-ambiguity-and-disambiguation',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-kontroverse-statistik-interpretieren'
  },
  {
    slug: 'c2-migrationsdebatte-versachlichen',
    topic: { de: 'eine Migrationsdebatte', en: 'a migration debate', fa: 'بحث مهاجرت', ar: 'نقاش حول الهجرة', tr: 'göç tartışması', ru: 'дебаты о миграции', ckb: 'گفتوگۆی کۆچکردن', kmr: 'nîqaşa koçberiyê', pl: 'debata o migracji', ro: 'o dezbatere despre migrație', sq: 'një debat për migracionin' },
    focus: { de: 'Erfahrung, Statistik und politische Bewertung nicht zu vermischen', en: 'not mixing experience, statistics, and political evaluation', fa: 'تجربه شخصی، آمار و داوری سیاسی را با هم قاطی نکنی', ar: 'عدم خلط التجربة والإحصاء والتقييم السياسي', tr: 'deneyimi, istatistiği ve siyasi değerlendirmeyi karıştırmamak', ru: 'не смешивать опыт, статистику и политическую оценку', ckb: 'ئەزموون، ئامار و هەڵسەنگاندنی سیاسی تێکەڵ نەکەیت', kmr: 'ezmûn, statîstîk û nirxandina siyasî tevlihev nekî', pl: 'nie mieszać doświadczenia, statystyki i oceny politycznej', ro: 'să nu amesteci experiența, statistica și evaluarea politică', sq: 'të mos përziesh përvojën, statistikat dhe vlerësimin politik' },
    grammar: 'c2-c2-debate-grammar',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-migrationsdebatte-versachlichen'
  },
  {
    slug: 'c2-komplexe-umweltentscheidung-vermitteln',
    topic: { de: 'eine komplexe Umweltentscheidung', en: 'a complex environmental decision', fa: 'یک تصمیم پیچیده محیط‌زیستی', ar: 'قرار بيئي معقد', tr: 'karmaşık bir çevre kararı', ru: 'сложное экологическое решение', ckb: 'بڕیارێکی ئاڵۆزی ژینگەیی', kmr: 'biryarake hawîrdorî ya aloz', pl: 'złożona decyzja środowiskowa', ro: 'o decizie complexă de mediu', sq: 'një vendim kompleks mjedisor' },
    focus: { de: 'Kosten, Folgen und Gerechtigkeit ohne einfache Schuldformel zu verbinden', en: 'connecting costs, consequences, and justice without a simple blame formula', fa: 'هزینه‌ها، پیامدها و عدالت را به هم وصل کنی، بدون اینکه همه چیز را به فرمول ساده مقصریابی تبدیل کنی', ar: 'ربط التكاليف والنتائج والعدالة من دون صيغة لوم مبسطة', tr: 'maliyetleri, sonuçları ve adaleti basit bir suçlama formülüne çevirmeden bağlamak', ru: 'связать издержки, последствия и справедливость без простой формулы обвинения', ckb: 'تێچوو، ئەنجام و دادپەروەری پێکەوە ببەستیت بەبێ فۆرمولەی سادەی تاوانبارکردن', kmr: 'lêçûn, encam û dadmendiyê girê bidî bêyî formuleke sade ya tawanbarî', pl: 'połączyć koszty, skutki i sprawiedliwość bez prostej formuły winy', ro: 'să legi costurile, consecințele și dreptatea fără o formulă simplă a vinei', sq: 'të lidhësh kostot, pasojat dhe drejtësinë pa një formulë të thjeshtë fajësimi' },
    grammar: 'c2-fine-differences-in-connectors',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-komplexe-umweltentscheidung-vermitteln'
  },
  {
    slug: 'c2-gesellschaft-politik-und-medienkritik-wiederholen',
    topic: { de: 'Gesellschaft, Politik und Medienkritik', en: 'society, politics, and media criticism', fa: 'جامعه، سیاست و نقد رسانه', ar: 'المجتمع والسياسة ونقد الإعلام', tr: 'toplum, siyaset ve medya eleştirisi', ru: 'общество, политика и критика медиа', ckb: 'کۆمەڵگا، سیاسەت و ڕەخنەی میدیا', kmr: 'civak, siyaset û rexneya medyayê', pl: 'społeczeństwo, polityka i krytyka mediów', ro: 'societate, politică și critică media', sq: 'shoqëri, politikë dhe kritikë mediatike' },
    focus: { de: 'eine kontroverse Frage so zusammenzufassen, dass Problem, Gegenposition und begruendetes Urteil sichtbar bleiben', en: 'summarizing a controversial question so that problem, opposing position, and justified judgment remain visible', fa: 'یک پرسش بحث‌برانگیز را طوری جمع‌بندی کنی که مسئله، موضع مخالف و داوری مستدل همچنان دیده شوند', ar: 'تلخيص سؤال خلافي بحيث تبقى المشكلة والموقف المقابل والحكم المعلل واضحة', tr: 'tartışmalı bir soruyu sorun, karşı pozisyon ve gerekçeli yargı görünür kalacak şekilde özetlemek', ru: 'резюмировать спорный вопрос так, чтобы проблема, противоположная позиция и обоснованное суждение оставались видимыми', ckb: 'پرسیارێکی ناکۆکیدار وەها کورت بکەیتەوە کە کێشە، هەڵوێستی دژ و حوکمی بە بەڵگە دیار بمێنن', kmr: 'pirsekî nakokîdar wisa kurt bikî ku pirsgirêk, helwesta dijber û dîtina bi sedem xuya bimînin', pl: 'podsumować kontrowersyjne pytanie tak, aby problem, stanowisko przeciwne i uzasadniony osąd pozostały widoczne', ro: 'să rezumi o întrebare controversată astfel încât problema, poziția opusă și judecata argumentată să rămână vizibile', sq: 'ta përmbledhësh një pyetje të diskutueshme në mënyrë që problemi, pozicioni kundërshtar dhe gjykimi i arsyetuar të mbeten të dukshme' },
    grammar: 'c2-c2-grammar-review-map',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-perspektivenkonflikt-synthetisieren'
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
    de: `Lies die Lesson und formuliere den eigentlichen Streitpunkt bei ${item.topic.de} in einem Satz. Trenne dabei Anlass, Behauptung und Wertfrage.`,
    en: `Read the lesson and formulate the real point of controversy in ${item.topic.en} in one sentence. Separate occasion, claim, and value question.`,
    fa: `درس را بخوان و نقطه اختلاف اصلی در موضوع ${item.topic.fa} را در یک جمله بنویس. علت شروع بحث، ادعا و پرسش ارزشی را از هم جدا کن.`,
    ar: `اقرأ الدرس وصغ نقطة الخلاف الحقيقية في موضوع ${item.topic.ar} في جملة واحدة. افصل بين سبب النقاش والادعاء والسؤال القيمي.`,
    tr: `Dersi oku ve ${item.topic.tr} konusundaki gerçek tartışma noktasını bir cümlede yaz. Çıkış nedeni, iddia ve değer sorusunu ayır.`,
    ru: `Прочитай урок и сформулируй настоящий предмет спора в теме ${item.topic.ru} одним предложением. Раздели повод, утверждение и ценностный вопрос.`,
    ckb: `وانەکە بخوێنەوە و خاڵی سەرەکی ناکۆکی لە بابەتی ${item.topic.ckb} لە یەک ڕستەدا بنووسە. هۆکاری دەستپێک، بانگەشە و پرسیاری بەهایی لێک جیا بکە.`,
    kmr: `Dersê bixwîne û xala rastîn a nakokiyê di mijara ${item.topic.kmr} de bi yek hevokê binivîse. Sedema destpêkê, îdia û pirsa nirxî ji hev cuda bike.`,
    pl: `Przeczytaj lekcję i sformułuj właściwy punkt sporny w temacie ${item.topic.pl} jednym zdaniem. Oddziel powód, twierdzenie i pytanie o wartość.`,
    ro: `Citește lecția și formulează într-o propoziție punctul real de dispută în tema ${item.topic.ro}. Separă motivul, afirmația și întrebarea de valoare.`,
    sq: `Lexoje mësimin dhe formulo në një fjali pikën e vërtetë të debatit në temën ${item.topic.sq}. Ndaje shkakun, pretendimin dhe pyetjen e vlerës.`
  };
}

function languageInstruction(item) {
  return {
    de: `Oeffne den verlinkten Grammatikpunkt. Markiere zwei Formen, mit denen du Bewertung ausdrueckst, ohne den Abstand zur Sache zu verlieren. Nutze sie fuer dieses Ziel: ${item.focus.de}.`,
    en: `Open the linked grammar point. Mark two forms that express evaluation without losing distance from the issue. Use them for this goal: ${item.focus.en}.`,
    fa: `نکته گرامری لینک‌شده را باز کن. دو ساختار پیدا کن که با آن‌ها ارزیابی می‌کنی، اما فاصله تحلیلی خودت را از موضوع از دست نمی‌دهی. آن‌ها را برای این هدف به کار ببر: ${item.focus.fa}.`,
    ar: `افتح نقطة القواعد المرتبطة. حدّد شكلين تعبّر بهما عن التقييم من دون فقدان المسافة التحليلية عن الموضوع. استخدمهما لهذا الهدف: ${item.focus.ar}.`,
    tr: `Bağlantılı dil bilgisi noktasını aç. Konuya analitik mesafeyi kaybetmeden değerlendirme ifade eden iki yapı işaretle. Bunları şu hedef için kullan: ${item.focus.tr}.`,
    ru: `Открой связанный грамматический пункт. Отметь две формы, с помощью которых можно выражать оценку, не теряя аналитической дистанции к теме. Используй их для этой цели: ${item.focus.ru}.`,
    ckb: `خاڵی ڕێزمانی بەستەرکراو بکەرەوە. دوو فۆرم دیاری بکە کە پێیان هەڵسەنگاندن دەردەبڕیت بەبێ لەدەستدانی دووری شیکاری لە بابەتەکە. بۆ ئەم ئامانجە بەکاریان بهێنە: ${item.focus.ckb}.`,
    kmr: `Xala rêzimanê ya girêdayî veke. Du formên ku pê nirxandinê dibêjî bêyî ku dûrbûna analîtîk ji mijarê winda bikî nîşan bike. Wan ji bo vê armancê bikar bîne: ${item.focus.kmr}.`,
    pl: `Otwórz podlinkowany punkt gramatyczny. Zaznacz dwie formy, którymi wyrażasz ocenę bez utraty analitycznego dystansu do sprawy. Użyj ich do tego celu: ${item.focus.pl}.`,
    ro: `Deschide punctul de gramatică legat. Marchează două forme prin care exprimi evaluarea fără să pierzi distanța analitică față de temă. Folosește-le pentru acest scop: ${item.focus.ro}.`,
    sq: `Hap pikën gramatikore të lidhur. Shëno dy forma me të cilat shpreh vlerësim pa humbur distancën analitike ndaj temës. Përdori për këtë synim: ${item.focus.sq}.`
  };
}

function targetInstruction(item) {
  return {
    de: `Bearbeite die verlinkte Ressource. Achte darauf, ob du dieses Ziel erreichst: ${item.focus.de}. Notiere danach eine Stelle, an der mehr Sachlichkeit oder mehr Empathie noetig waere.`,
    en: `Work through the linked resource. Watch whether you reach this goal: ${item.focus.en}. Then note one place where more objectivity or more empathy would be needed.`,
    fa: `منبع لینک‌شده را انجام بده. دقت کن آیا به این هدف می‌رسی: ${item.focus.fa}. بعد یک بخش را یادداشت کن که در آن بی‌طرفی بیشتر یا همدلی بیشتری لازم است.`,
    ar: `اعمل على المورد المرتبط. انتبه هل تصل إلى هذا الهدف: ${item.focus.ar}. ثم سجّل موضعًا يحتاج إلى موضوعية أكثر أو تعاطف أكثر.`,
    tr: `Bağlantılı kaynağı çalış. Şu hedefe ulaşıp ulaşmadığına dikkat et: ${item.focus.tr}. Sonra daha fazla nesnellik ya da empati gerektiren bir yer not et.`,
    ru: `Проработай связанный ресурс. Следи, достигаешь ли этой цели: ${item.focus.ru}. Затем отметь одно место, где нужна большая объективность или больше эмпатии.`,
    ckb: `سەرچاوەی بەستەرکراو کاربکە. سەرنج بدە ئایا دەگەیتە ئەم ئامانجە: ${item.focus.ckb}. پاشان شوێنێک بنووسە کە پێویستی بە بابەتیبوونی زیاتر یان هاوسۆزی زیاتر هەیە.`,
    kmr: `Çavkaniya girêdayî bixebitîne. Bala xwe bide gelo tu digihîjî vê armancê: ${item.focus.kmr}. Paşê cihê ku zêdetir babetîbûn an zêdetir hevdilî pêwîst e binivîse.`,
    pl: `Przerób podlinkowany materiał. Zwróć uwagę, czy osiągasz ten cel: ${item.focus.pl}. Potem zanotuj miejsce, w którym potrzeba więcej rzeczowości albo empatii.`,
    ro: `Lucrează resursa legată. Urmărește dacă atingi acest scop: ${item.focus.ro}. Apoi notează un loc unde ar fi nevoie de mai multă obiectivitate sau empatie.`,
    sq: `Puno me burimin e lidhur. Vëzhgo nëse e arrin këtë synim: ${item.focus.sq}. Pastaj shëno një vend ku do të duhej më shumë paanshmëri ose më shumë empati.`
  };
}

function transferInstruction() {
  return {
    de: 'Schreibe eine Mini-Analyse in vier Saetzen: Streitpunkt, Gegenposition, eigene Bewertung, offene Grenze. Vermeide Schlagworte, die nur Zustimmung der eigenen Seite erzeugen.',
    en: 'Write a mini-analysis in four sentences: controversy, opposing position, your evaluation, open limit. Avoid slogans that only create agreement from your own side.',
    fa: 'یک تحلیل کوتاه در چهار جمله بنویس: نقطه اختلاف، موضع مخالف، ارزیابی خودت، و مرز یا پرسش باز. از شعارهایی که فقط موافقت گروه خودت را برمی‌انگیزند پرهیز کن.',
    ar: 'اكتب تحليلًا قصيرًا في أربع جمل: نقطة الخلاف، الموقف المقابل، تقييمك، والحد أو السؤال المفتوح. تجنب الشعارات التي لا تفعل إلا كسب موافقة جماعتك.',
    tr: 'Dört cümlede kısa bir analiz yaz: tartışma noktası, karşı pozisyon, kendi değerlendirmen, açık sınır. Yalnızca kendi tarafının onayını üreten sloganlardan kaçın.',
    ru: 'Напиши мини-анализ в четырех предложениях: предмет спора, противоположная позиция, твоя оценка, открытая граница. Избегай лозунгов, которые только вызывают согласие своей стороны.',
    ckb: 'شیکارییەکی کورتی چوار ڕستەیی بنووسە: خاڵی ناکۆکی، هەڵوێستی دژ، هەڵسەنگاندنی خۆت و سنوور یان پرسیاری کراوە. لە دروشمەکان دوور بکەوە کە تەنها ڕەزامەندی لایەنی خۆت دروست دەکەن.',
    kmr: 'Analîzeke kurt bi çar hevokan binivîse: xala nakokiyê, helwesta dijber, nirxandina xwe, sînora vekirî. Ji sloganan dûr bikeve ku tenê pejirandina aliyê xwe çê dikin.',
    pl: 'Napisz mini-analizę w czterech zdaniach: punkt sporny, stanowisko przeciwne, własna ocena, otwarta granica. Unikaj haseł, które tylko budują zgodę po twojej stronie.',
    ro: 'Scrie o mini-analiză în patru propoziții: punctul disputat, poziția opusă, evaluarea ta, limita deschisă. Evită sloganurile care produc doar acordul propriei tabere.',
    sq: 'Shkruaj një mini-analizë në katër fjali: pika e debatit, pozicioni kundërshtar, vlerësimi yt, kufiri i hapur. Shmang parullat që krijojnë vetëm miratim nga ana jote.'
  };
}

function reviewInstruction(item) {
  return {
    de: `Pruefe deine Mini-Analyse: Wird die Gegenposition wirklich erkennbar? Ist dein Urteil begruendet? Bleibt bei ${item.topic.de} eine legitime Unsicherheit stehen? Schwaeche eine ueberzogene Formulierung ab.`,
    en: `Check your mini-analysis: is the opposing position really visible? Is your judgment justified? Does legitimate uncertainty remain in ${item.topic.en}? Soften one exaggerated formulation.`,
    fa: `تحلیل کوتاهت را بررسی کن: آیا موضع مخالف واقعاً دیده می‌شود؟ آیا داوری تو دلیل دارد؟ آیا در موضوع ${item.topic.fa} جایی برای ابهام مشروع باقی مانده است؟ یک عبارت اغراق‌آمیز را ملایم‌تر کن.`,
    ar: `افحص تحليلك القصير: هل يظهر الموقف المقابل فعلًا؟ هل حكمك معلل؟ هل يبقى قدر مشروع من عدم اليقين في موضوع ${item.topic.ar}؟ خفف عبارة مبالغًا فيها.`,
    tr: `Mini analizini kontrol et: karşı pozisyon gerçekten görünür mü? Yargın gerekçeli mi? ${item.topic.tr} konusunda meşru bir belirsizlik kalıyor mu? Abartılı bir ifadeyi yumuşat.`,
    ru: `Проверь мини-анализ: действительно ли видна противоположная позиция? Обосновано ли твое суждение? Остается ли в теме ${item.topic.ru} легитимная неопределенность? Смягчи одну преувеличенную формулировку.`,
    ckb: `شیکارییە کورتەکەت بپشکنە: ئایا هەڵوێستی دژ بە ڕاستی دیارە؟ ئایا حوکمەکەت بەڵگەی هەیە؟ ئایا لە بابەتی ${item.topic.ckb} نادڵنیاییەکی ڕەوا دەمێنێتەوە؟ دەربڕینێکی زیادەڕۆیی نەرمتر بکە.`,
    kmr: `Analîza xwe ya kurt kontrol bike: helwesta dijber bi rastî xuya ye? Dîtina te sedem heye? Di mijara ${item.topic.kmr} de nediyarbûneke rewa dimîne? Gotineke zêde nermtir bike.`,
    pl: `Sprawdź mini-analizę: czy stanowisko przeciwne naprawdę jest widoczne? Czy twój osąd jest uzasadniony? Czy w temacie ${item.topic.pl} zostaje uzasadniona niepewność? Osłab jedno przesadne sformułowanie.`,
    ro: `Verifică mini-analiza: poziția opusă este cu adevărat vizibilă? Judecata ta este argumentată? Rămâne o incertitudine legitimă în tema ${item.topic.ro}? Atenuează o formulare exagerată.`,
    sq: `Kontrollo mini-analizën: a është vërtet i dukshëm pozicioni kundërshtar? A është i arsyetuar gjykimi yt? A mbetet pasiguri e ligjshme në temën ${item.topic.sq}? Zbute një formulim të tepruar.`
  };
}

for (const item of items) {
  const lesson = lessons.find(l => l.slug === item.slug);
  if (!lesson) {
    throw new Error(`Lesson not found: ${item.slug}`);
  }

  lesson.activityBlocks = [
    createActivity('read', 'orient', orientInstruction(item), 'none', null, 6, 10),
    createActivity('grammar', 'language', languageInstruction(item), 'grammar-topic', item.grammar, 8, 20),
    createActivity(item.targetType === 'exam-prep-unit' ? 'exam-prep' : 'roleplay', 'target', targetInstruction(item), item.targetType, item.targetSlug, 10, 30),
    createActivity('practice', 'transfer', transferInstruction(), 'none', null, 8, 40),
    createActivity('review', 'review', reviewInstruction(item), 'none', null, 6, 50)
  ];
}

fs.writeFileSync(file, JSON.stringify(data, null, 2) + '\n', 'utf8');
console.log(`Updated ${items.length} C2 Module 7 lessons with ${items.length * 5} activity blocks.`);

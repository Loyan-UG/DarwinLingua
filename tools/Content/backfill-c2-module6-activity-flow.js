const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c2-stil-souveraenitaet-und-komplexer-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'Lage und Rolle klaeren',
    en: 'Clarify the situation and your role',
    fa: 'وضعیت و نقش خودت را روشن کن',
    ar: 'وضّح الوضع ودورك',
    tr: 'Durumu ve rolünü netleştir',
    ru: 'Проясни ситуацию и свою роль',
    ckb: 'دۆخەکە و ڕۆڵی خۆت ڕوون بکە',
    kmr: 'Rewşê û rola xwe zelal bike',
    pl: 'Wyjaśnij sytuację i swoją rolę',
    ro: 'Clarifică situația și rolul tău',
    sq: 'Qartëso situatën dhe rolin tënd'
  },
  language: {
    de: 'Ton und Verantwortung steuern',
    en: 'Control tone and responsibility',
    fa: 'لحن و مسئولیت‌پذیری را کنترل کن',
    ar: 'اضبط النبرة وتحمل المسؤولية',
    tr: 'Tonu ve sorumluluğu yönet',
    ru: 'Управляй тоном и ответственностью',
    ckb: 'تۆن و بەرپرسیارێتی بەڕێوەببە',
    kmr: 'Ton û berpirsiyariyê birêve bibe',
    pl: 'Kontroluj ton i odpowiedzialność',
    ro: 'Controlează tonul și responsabilitatea',
    sq: 'Kontrollo tonin dhe përgjegjësinë'
  },
  target: {
    de: 'Konflikt praktisch fuehren',
    en: 'Handle the conflict in practice',
    fa: 'تعارض را در عمل مدیریت کن',
    ar: 'أدر الخلاف عمليًا',
    tr: 'Çatışmayı pratikte yönet',
    ru: 'Практически веди конфликт',
    ckb: 'ناکۆکییەکە بە کرداری بەڕێوەببە',
    kmr: 'Nakokiyê di pratîkê de birêve bibe',
    pl: 'Poprowadź konflikt w praktyce',
    ro: 'Gestionează conflictul practic',
    sq: 'Menaxho konfliktin në praktikë'
  },
  transfer: {
    de: 'Kernaussage belastbar formulieren',
    en: 'Formulate a robust core message',
    fa: 'پیام اصلی را محکم و قابل دفاع بیان کن',
    ar: 'صغ الرسالة الأساسية بشكل متين',
    tr: 'Ana mesajı sağlam biçimde ifade et',
    ru: 'Сформулируй устойчивое ключевое сообщение',
    ckb: 'پەیامی سەرەکی بە شێوەیەکی بەهێز دەرببڕە',
    kmr: 'Peyama sereke bi awayekî xurt bibêje',
    pl: 'Sformułuj odporny komunikat główny',
    ro: 'Formulează solid mesajul central',
    sq: 'Formulo mesazhin kryesor në mënyrë të qëndrueshme'
  },
  review: {
    de: 'Risiko und Wirkung pruefen',
    en: 'Check risk and effect',
    fa: 'ریسک و اثر پیام را بررسی کن',
    ar: 'افحص المخاطر والأثر',
    tr: 'Riski ve etkiyi kontrol et',
    ru: 'Проверь риск и эффект',
    ckb: 'مەترسی و کاریگەری بپشکنە',
    kmr: 'Rîsk û bandorê kontrol bike',
    pl: 'Sprawdź ryzyko i efekt',
    ro: 'Verifică riscul și efectul',
    sq: 'Kontrollo rrezikun dhe efektin'
  }
};

const items = [
  {
    slug: 'c2-heikle-strategieentscheidung-moderieren',
    topic: { de: 'eine heikle Strategieentscheidung', en: 'a sensitive strategic decision', fa: 'یک تصمیم راهبردی حساس', ar: 'قرار استراتيجي حساس', tr: 'hassas bir strateji kararı', ru: 'деликатное стратегическое решение', ckb: 'بڕیارێکی ستراتیژی هەستیار', kmr: 'biryarake stratejîk a hesas', pl: 'delikatna decyzja strategiczna', ro: 'o decizie strategică sensibilă', sq: 'një vendim strategjik i ndjeshëm' },
    focus: { de: 'Entscheidungsdruck sichtbar zu machen, ohne Alternativen vorschnell abzuwerten', en: 'making decision pressure visible without prematurely devaluing alternatives', fa: 'فشار تصمیم‌گیری را نشان بدهی، بدون اینکه گزینه‌های دیگر را زود و بی‌دلیل کم‌ارزش کنی', ar: 'إظهار ضغط القرار من دون التقليل المتسرع من قيمة البدائل', tr: 'karar baskısını görünür kılmak ama alternatifleri aceleyle değersizleştirmemek', ru: 'показать давление решения, не обесценивая альтернативы преждевременно', ckb: 'فشاری بڕیاردان پیشان بدەیت بەبێ ئەوەی بژاردەکان بە پەلە کەم‌بەها بکەیت', kmr: 'zexta biryardanê xuya bikî bêyî ku alternatîfan zû bêqîmet bikî', pl: 'pokazać presję decyzyjną bez przedwczesnego obniżania wartości alternatyw', ro: 'să faci vizibilă presiunea deciziei fără să devalorizezi prematur alternativele', sq: 'ta bësh të dukshëm presionin e vendimit pa i zhvlerësuar para kohe alternativat' },
    grammar: 'c2-c2-debate-grammar',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-heikle-strategieentscheidung-moderieren'
  },
  {
    slug: 'c2-vorstandsvorschlag-diplomatisch-kritisieren',
    topic: { de: 'einen Vorstandsvorschlag', en: 'a board proposal', fa: 'یک پیشنهاد هیئت‌مدیره', ar: 'اقتراح مجلس الإدارة', tr: 'bir yönetim kurulu önerisi', ru: 'предложение правления', ckb: 'پێشنیارێکی دەستەی بەڕێوەبەری', kmr: 'pêşniyareke lijneya rêveberiyê', pl: 'propozycja zarządu', ro: 'o propunere a conducerii', sq: 'një propozim i bordit drejtues' },
    focus: { de: 'klar zu kritisieren, ohne Rang, Gesicht oder Beziehung unnoetig zu beschaedigen', en: 'criticizing clearly without unnecessarily damaging rank, face, or relationship', fa: 'روشن انتقاد کنی، بدون اینکه جایگاه، آبرو یا رابطه را بی‌دلیل آسیب بزنی', ar: 'النقد بوضوح من دون الإضرار غير الضروري بالمكانة أو حفظ الوجه أو العلاقة', tr: 'açık eleştirmek ama makamı, itibarı ya da ilişkiyi gereksiz yere zedelememek', ru: 'критиковать ясно, не повреждая без необходимости статус, лицо или отношения', ckb: 'بە ڕوونی ڕەخنە بگریت بەبێ زیان گەیاندنی ناپێویست بە پلە، ڕوو یان پەیوەندی', kmr: 'bi zelalî rexne bikî bêyî ku paye, rû an têkiliyê bêhewce zirar bidî', pl: 'krytykować jasno bez niepotrzebnego naruszania pozycji, twarzy lub relacji', ro: 'să critici clar fără să afectezi inutil statutul, imaginea sau relația', sq: 'të kritikosh qartë pa dëmtuar panevojshëm pozitën, fytyrën ose marrëdhënien' },
    grammar: 'c2-register-and-syntactic-choice',
    targetType: 'roleplay',
    targetSlug: 'c2-einen-vorstandsvorschlag-diplomatisch-kritisieren'
  },
  {
    slug: 'c2-krisenkommunikation-intern-abstimmen',
    topic: { de: 'interne Krisenkommunikation', en: 'internal crisis communication', fa: 'ارتباطات داخلی در بحران', ar: 'التواصل الداخلي في الأزمة', tr: 'iç kriz iletişimi', ru: 'внутренняя кризисная коммуникация', ckb: 'پەیوەندی ناوخۆیی لە قەیراندا', kmr: 'ragihandina hundirîn di qeyranê de', pl: 'wewnętrzna komunikacja kryzysowa', ro: 'comunicarea internă de criză', sq: 'komunikimi i brendshëm në krizë' },
    focus: { de: 'Information, Unsicherheit und Zuständigkeit sauber zu trennen', en: 'separating information, uncertainty, and responsibility cleanly', fa: 'اطلاعات، ابهام و مسئولیت را تمیز از هم جدا کنی', ar: 'الفصل بوضوح بين المعلومة وعدم اليقين والمسؤولية', tr: 'bilgi, belirsizlik ve sorumluluğu temiz biçimde ayırmak', ru: 'четко разделять информацию, неопределенность и ответственность', ckb: 'زانیاری، نادڵنیایی و بەرپرسیارێتی بە ڕوونی لێک جیا بکەیتەوە', kmr: 'agahî, nediyarbûn û berpirsiyariyê bi zelalî ji hev cuda bikî', pl: 'czysto oddzielić informację, niepewność i odpowiedzialność', ro: 'să separi clar informația, incertitudinea și responsabilitatea', sq: 'të ndash qartë informacionin, pasigurinë dhe përgjegjësinë' },
    grammar: 'c2-journalistic-compression',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-krisenkommunikation-intern-abstimmen'
  },
  {
    slug: 'c2-interessenkonflikt-offenlegen',
    topic: { de: 'einen Interessenkonflikt', en: 'a conflict of interest', fa: 'تعارض منافع', ar: 'تضارب مصالح', tr: 'çıkar çatışması', ru: 'конфликт интересов', ckb: 'ناکۆکی بەرژەوەندی', kmr: 'nakokiya berjewendiyan', pl: 'konflikt interesów', ro: 'un conflict de interese', sq: 'një konflikt interesi' },
    focus: { de: 'Transparenz herzustellen, ohne dich zu rechtfertigen oder andere zu belasten', en: 'creating transparency without justifying yourself or burdening others', fa: 'شفافیت ایجاد کنی، بدون اینکه خودت را توجیه کنی یا بار تقصیر را روی دیگران بگذاری', ar: 'خلق الشفافية من دون تبرير نفسك أو تحميل الآخرين عبئًا', tr: 'kendini savunmaya geçmeden ya da başkalarını yük altında bırakmadan şeffaflık kurmak', ru: 'создать прозрачность, не оправдываясь и не перекладывая нагрузку на других', ckb: 'ڕووناکی دروست بکەیت بەبێ پاساو هێنانەوە بۆ خۆت یان بارکردنی کێشە لەسەر ئەوانی تر', kmr: 'zelalbûnê çê bikî bêyî ku xwe paqij bikî an bar bavêjî ser kesên din', pl: 'stworzyć przejrzystość bez usprawiedliwiania siebie lub obciążania innych', ro: 'să creezi transparență fără să te justifici sau să îi încarci pe alții', sq: 'të krijosh transparencë pa u justifikuar ose pa rënduar të tjerët' },
    grammar: 'c2-advanced-passive-and-agent-omission',
    targetType: 'roleplay',
    targetSlug: 'c2-einen-interessenkonflikt-offenlegen'
  },
  {
    slug: 'c2-verhandlung-ohne-gesichtsverlust',
    topic: { de: 'eine Verhandlung ohne Gesichtsverlust', en: 'a negotiation without loss of face', fa: 'مذاکره‌ای بدون از بین رفتن آبرو یا جایگاه طرف مقابل', ar: 'تفاوض من دون فقدان ماء الوجه', tr: 'itibar kaybı olmadan müzakere', ru: 'переговоры без потери лица', ckb: 'دانوسانی بەبێ لەدەستدانی ڕوو', kmr: 'danûstandin bê windakirina rû', pl: 'negocjacje bez utraty twarzy', ro: 'o negociere fără pierderea prestigiului', sq: 'një negociatë pa humbje fytyre' },
    focus: { de: 'eine harte Grenze zu setzen und trotzdem einen Rueckweg offen zu lassen', en: 'setting a firm boundary while still leaving a path back open', fa: 'مرز محکم بگذاری و هم‌زمان راه برگشت محترمانه را باز بگذاری', ar: 'وضع حد واضح مع ترك طريق محترم للعودة', tr: 'sağlam bir sınır koyup yine de dönüş yolunu açık bırakmak', ru: 'поставить твердую границу и при этом оставить достойный путь назад', ckb: 'سنوورێکی بەهێز دابنێیت و هەمان کات ڕێگای گەڕانەوە بەڕێزەوە بە کراوەیی بهێڵیتەوە', kmr: 'sînorekî xurt danî û di heman demê de rêya vegerê bi rêz vekirî bihêlî', pl: 'postawić twardą granicę i jednocześnie zostawić godną drogę powrotu', ro: 'să stabilești o limită fermă și totuși să lași deschisă o cale de revenire', sq: 'të vendosësh një kufi të fortë dhe njëkohësisht të lësh të hapur një rrugë kthimi me dinjitet' },
    grammar: 'c2-fine-differences-in-connectors',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-verhandlung-mit-gesichtsverlust-vermeiden'
  },
  {
    slug: 'c2-scheinkonsens-im-meeting-aufbrechen',
    topic: { de: 'Scheinkonsens im Meeting', en: 'false consensus in a meeting', fa: 'توافق ظاهری در جلسه', ar: 'توافق ظاهري في الاجتماع', tr: 'toplantıda sahte uzlaşma', ru: 'мнимый консенсус на встрече', ckb: 'ڕێککەوتنی ڕووکەشی لە کۆبوونەوەدا', kmr: 'li hevkirina derewîn di civînê de', pl: 'pozorny konsensus na spotkaniu', ro: 'un consens aparent într-o ședință', sq: 'konsensus i rremë në takim' },
    focus: { de: 'scheinbare Zustimmung zu oeffnen, ohne einzelne Personen blosszustellen', en: 'opening up apparent agreement without exposing individual people', fa: 'توافق ظاهری را باز کنی، بدون اینکه افراد مشخصی را در معرض فشار یا شرمندگی بگذاری', ar: 'فتح التوافق الظاهري من دون إحراج أشخاص بعينهم', tr: 'görünür onayı açmak ama kişileri teşhir etmemek', ru: 'раскрыть видимое согласие, не выставляя отдельных людей', ckb: 'ڕەزامەندی ڕووکەش بکەیتەوە بەبێ ئەوەی کەسانی دیاریکراو ڕیسوا بکەیت', kmr: 'pejirandina xuya vekî bêyî ku kesên taybetî şerm bikevin', pl: 'otworzyć pozorną zgodę bez wystawiania pojedynczych osób', ro: 'să deschizi acordul aparent fără să expui persoane individuale', sq: 'ta hapësh pajtimin e dukshëm pa ekspozuar persona të veçantë' },
    grammar: 'c2-ambiguity-and-disambiguation',
    targetType: 'roleplay',
    targetSlug: 'c2-einen-scheinkonsens-im-meeting-aufbrechen'
  },
  {
    slug: 'c2-unternehmensentscheidung-ethisch-einordnen',
    topic: { de: 'eine Unternehmensentscheidung ethisch', en: 'a company decision ethically', fa: 'یک تصمیم شرکتی از نظر اخلاقی', ar: 'قرار شركة من منظور أخلاقي', tr: 'bir şirket kararını etik açıdan', ru: 'решение компании с этической точки зрения', ckb: 'بڕیارێکی کۆمپانیا لە ڕووی ئەخلاقییەوە', kmr: 'biryarake pargîdanî ji aliyê etîkî ve', pl: 'decyzja firmy etycznie', ro: 'o decizie de companie din punct de vedere etic', sq: 'një vendim kompanie nga ana etike' },
    focus: { de: 'wirtschaftliche Gruende, Folgen und Werte getrennt, aber verbunden darzustellen', en: 'presenting business reasons, consequences, and values separately but connected', fa: 'دلیل‌های اقتصادی، پیامدها و ارزش‌ها را جدا اما مرتبط توضیح بدهی', ar: 'عرض الأسباب الاقتصادية والنتائج والقيم منفصلة لكنها مترابطة', tr: 'ekonomik gerekçeleri, sonuçları ve değerleri ayrı ama bağlantılı sunmak', ru: 'представить экономические причины, последствия и ценности раздельно, но связанно', ckb: 'هۆکارە ئابوورییەکان، ئەنجامەکان و بەهاکان جیا بەڵام پەیوەندیدار پیشان بدەیت', kmr: 'sedemên aborî, encam û nirxan cuda lê girêdayî nîşan bidî', pl: 'przedstawić powody biznesowe, skutki i wartości oddzielnie, ale w powiązaniu', ro: 'să prezinți motivele economice, consecințele și valorile separat, dar conectat', sq: 'të paraqesësh arsyet ekonomike, pasojat dhe vlerat veçmas, por të lidhura' },
    grammar: 'c2-c2-formal-writing-grammar',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-unternehmensentscheidung-ethisch-einordnen'
  },
  {
    slug: 'c2-verfahrene-projektsituation-neu-rahmen',
    topic: { de: 'eine verfahrene Projektsituation', en: 'a deadlocked project situation', fa: 'یک وضعیت پروژه که گیر کرده و پیش نمی‌رود', ar: 'وضع مشروع عالق', tr: 'kilitlenmiş bir proje durumu', ru: 'застрявшая проектная ситуация', ckb: 'دۆخێکی پڕۆژە کە گیرەی خواردووە', kmr: 'rewşeke projeyê ya asê mayî', pl: 'zablokowana sytuacja projektowa', ro: 'o situație de proiect blocată', sq: 'një situatë projekti e bllokuar' },
    focus: { de: 'aus Schuldzuweisung eine naechste bearbeitbare Frage zu machen', en: 'turning blame into the next workable question', fa: 'سرزنش را به یک پرسش عملی برای قدم بعدی تبدیل کنی', ar: 'تحويل اللوم إلى سؤال عملي للخطوة التالية', tr: 'suçlamayı bir sonraki çalışılabilir soruya çevirmek', ru: 'превратить обвинение в следующий рабочий вопрос', ckb: 'تاوانبارکردن بکەیتە پرسیارێکی کرداری بۆ هەنگاوی داهاتوو', kmr: 'tawanbarî veguherînî pirseke karbar ji bo gava din', pl: 'zamienić obwinianie w kolejne wykonalne pytanie', ro: 'să transformi vina într-o întrebare lucrabilă pentru pasul următor', sq: 'ta kthesh fajësimin në një pyetje praktike për hapin tjetër' },
    grammar: 'c2-fine-differences-in-connectors',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-verfahrene-projektsituation-neu-rahmen'
  },
  {
    slug: 'c2-reputationsschaden-sachlich-besprechen',
    topic: { de: 'einen Reputationsschaden', en: 'reputational damage', fa: 'آسیب به اعتبار', ar: 'ضرر السمعة', tr: 'itibar zararı', ru: 'репутационный ущерб', ckb: 'زیان بە ناوبانگ', kmr: 'zirara navûdengê', pl: 'szkoda reputacyjna', ro: 'un prejudiciu de reputație', sq: 'dëm reputacional' },
    focus: { de: 'Schaden klar zu benennen, ohne Panik, Schuldtheater oder Beschwichtigung', en: 'naming the damage clearly without panic, blame theater, or soothing language', fa: 'آسیب را روشن نام ببری، بدون ترساندن، نمایش مقصریابی یا کوچک‌نمایی', ar: 'تسمية الضرر بوضوح من دون هلع أو مسرحية لوم أو تهوين', tr: 'zararı açıkça adlandırmak ama panik, suçlama gösterisi ya da yatıştırıcı boş söz kullanmamak', ru: 'четко назвать ущерб без паники, показного поиска виноватых или успокоительных фраз', ckb: 'زیانەکە بە ڕوونی ناوببەیت بەبێ ترساندن، شانۆی تاوانبارکردن یان بچووک نیشاندانی کێشە', kmr: 'zirarê bi zelalî nav bikî bê panîk, şanoya tawanbarî an biçûk nîşandana pirsgirêkê', pl: 'jasno nazwać szkodę bez paniki, teatru winy lub uspokajania na siłę', ro: 'să numești clar prejudiciul fără panică, spectacol al vinei sau minimalizare', sq: 'ta emërtosh qartë dëmin pa panik, teatër fajësimi ose qetësim të rremë' },
    grammar: 'c2-journalistic-compression',
    targetType: 'roleplay',
    targetSlug: 'c2-einen-reputationsschaden-sachlich-besprechen'
  },
  {
    slug: 'c2-professionelle-krisen-und-strategiekommunikation-wiederholen',
    topic: { de: 'Krisen- und Strategiekommunikation', en: 'crisis and strategy communication', fa: 'ارتباطات بحران و راهبرد', ar: 'تواصل الأزمات والاستراتيجية', tr: 'kriz ve strateji iletişimi', ru: 'кризисная и стратегическая коммуникация', ckb: 'پەیوەندی قەیران و ستراتیژی', kmr: 'ragihandina qeyran û stratejiyê', pl: 'komunikacja kryzysowa i strategiczna', ro: 'comunicarea de criză și strategie', sq: 'komunikimi i krizës dhe strategjisë' },
    focus: { de: 'Lage, Verantwortung, Grenze und naechsten Schritt in einer kontrollierten Linie zu verbinden', en: 'connecting situation, responsibility, boundary, and next step in one controlled line', fa: 'وضعیت، مسئولیت، مرز و قدم بعدی را در یک خط کنترل‌شده به هم وصل کنی', ar: 'ربط الوضع والمسؤولية والحد والخطوة التالية في خط مضبوط واحد', tr: 'durum, sorumluluk, sınır ve sonraki adımı kontrollü tek bir çizgide birleştirmek', ru: 'связать ситуацию, ответственность, границу и следующий шаг в одну контролируемую линию', ckb: 'دۆخ، بەرپرسیارێتی، سنوور و هەنگاوی داهاتوو لە هێڵێکی کۆنترۆڵکراودا پێکەوە ببەستیت', kmr: 'rewş, berpirsiyarî, sînor û gava din di xeteke kontrolkirî de girê bidî', pl: 'połączyć sytuację, odpowiedzialność, granicę i następny krok w jednej kontrolowanej linii', ro: 'să legi situația, responsabilitatea, limita și pasul următor într-o linie controlată', sq: 'të lidhësh situatën, përgjegjësinë, kufirin dhe hapin tjetër në një vijë të kontrolluar' },
    grammar: 'c2-c2-grammar-review-map',
    targetType: 'writing-template',
    targetSlug: 'c2-krisenupdate-intern-abstimmen'
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
    de: `Lies die Lesson langsam und markiere, worum es bei ${item.topic.de} wirklich geht: Entscheidung, Beziehung, Risiko oder Verantwortung. Notiere eine Formulierung, die Druck benennt, ohne Alarm zu erzeugen.`,
    en: `Read the lesson slowly and mark what ${item.topic.en} is really about: decision, relationship, risk, or responsibility. Note one phrase that names pressure without creating alarm.`,
    fa: `درس را آرام بخوان و مشخص کن موضوع ${item.topic.fa} واقعاً درباره چیست: تصمیم، رابطه، ریسک یا مسئولیت. یک عبارت یادداشت کن که فشار را بیان کند، بدون اینکه فضای ترس ایجاد کند.`,
    ar: `اقرأ الدرس ببطء وحدد ما يدور حوله ${item.topic.ar} فعلًا: قرار أم علاقة أم خطر أم مسؤولية. سجّل عبارة واحدة تذكر الضغط من دون خلق إنذار.`,
    tr: `Dersi yavaş oku ve ${item.topic.tr} konusunun aslında neyle ilgili olduğunu işaretle: karar, ilişki, risk ya da sorumluluk. Baskıyı anlatan ama alarm yaratmayan bir ifade not et.`,
    ru: `Медленно прочитай урок и отметь, о чем на самом деле идет речь в теме ${item.topic.ru}: решение, отношения, риск или ответственность. Запиши одну формулировку, которая называет давление без создания тревоги.`,
    ckb: `وانەکە بە هێواشی بخوێنەوە و دیاری بکە ${item.topic.ckb} بە ڕاستی پەیوەندی بە چییەوە هەیە: بڕیار، پەیوەندی، مەترسی یان بەرپرسیارێتی. یەک دەربڕین بنووسە کە فشار ناوببات بەبێ دروستکردنی ترس.`,
    kmr: `Dersê hêdî bixwîne û nîşan bike ku mijara ${item.topic.kmr} bi rastî bi çi ve girêdayî ye: biryar, têkilî, rîsk an berpirsiyarî. Gotinek binivîse ku zextê nav dike bêyî ku tirs çê bike.`,
    pl: `Przeczytaj lekcję powoli i zaznacz, czego naprawdę dotyczy temat ${item.topic.pl}: decyzji, relacji, ryzyka czy odpowiedzialności. Zapisz jedno sformułowanie, które nazywa presję bez tworzenia alarmu.`,
    ro: `Citește lecția încet și marchează despre ce este de fapt ${item.topic.ro}: decizie, relație, risc sau responsabilitate. Notează o formulare care numește presiunea fără să creeze alarmă.`,
    sq: `Lexoje mësimin ngadalë dhe shëno për çfarë flet vërtet ${item.topic.sq}: vendim, marrëdhënie, rrezik apo përgjegjësi. Shkruaj një formulim që e përmend presionin pa krijuar alarm.`
  };
}

function languageInstruction(item) {
  return {
    de: `Oeffne den verlinkten Grammatikpunkt und achte auf Distanz, Aktiv/Passiv, Konnektoren oder Register. Waehle danach zwei Saetze, die ${item.focus.de}.`,
    en: `Open the linked grammar point and pay attention to distance, active/passive voice, connectors, or register. Then choose two sentences that support this goal: ${item.focus.en}.`,
    fa: `نکته گرامری لینک‌شده را باز کن و به فاصله‌گذاری، معلوم/مجهول، رابط‌ها یا سطح رسمی زبان دقت کن. بعد دو جمله انتخاب کن که به این هدف کمک کند: ${item.focus.fa}.`,
    ar: `افتح نقطة القواعد المرتبطة وانتبه إلى المسافة، المبني للمعلوم أو المجهول، أدوات الربط أو مستوى اللغة. ثم اختر جملتين تخدمان هذا الهدف: ${item.focus.ar}.`,
    tr: `Bağlantılı dil bilgisi noktasını aç ve mesafe, etken/edilgen yapı, bağlaçlar ya da register konularına dikkat et. Sonra şu hedefe hizmet eden iki cümle seç: ${item.focus.tr}.`,
    ru: `Открой связанный грамматический пункт и обрати внимание на дистанцию, актив/пассив, связки или регистр. Затем выбери два предложения, которые помогают этой цели: ${item.focus.ru}.`,
    ckb: `خاڵی ڕێزمانی بەستەرکراو بکەرەوە و سەرنج بدە بە دووری، کاری ڕاستەوخۆ/ناراستەوخۆ، پەیوەندیکەرەکان یان ئاستی فەرمی زمان. پاشان دوو ڕستە هەڵبژێرە کە یارمەتی ئەم ئامانجە بدەن: ${item.focus.ckb}.`,
    kmr: `Xala rêzimanê ya girêdayî veke û bala xwe bide dûrbûn, çalak/neçalak, girêder an asta fermî ya ziman. Paşê du hevokên ku ji bo vê armancê alîkar in hilbijêre: ${item.focus.kmr}.`,
    pl: `Otwórz podlinkowany punkt gramatyczny i zwróć uwagę na dystans, stronę czynną/bierną, łączniki lub rejestr. Potem wybierz dwa zdania, które wspierają ten cel: ${item.focus.pl}.`,
    ro: `Deschide punctul de gramatică legat și urmărește distanța, activul/pasivul, conectorii sau registrul. Apoi alege două propoziții care susțin acest scop: ${item.focus.ro}.`,
    sq: `Hap pikën gramatikore të lidhur dhe kushto vëmendje distancës, veprores/pësore, lidhëzave ose regjistrit. Pastaj zgjidh dy fjali që ndihmojnë këtë synim: ${item.focus.sq}.`
  };
}

function targetInstruction(item) {
  return {
    de: `Bearbeite die verlinkte Ressource. Pruefe danach, ob du das Ziel erreichst: ${item.focus.de}. Korrigiere eine Stelle, an der der Ton zu hart, zu weich oder zu ausweichend wirkt.`,
    en: `Work through the linked resource. Then check whether you reach the goal: ${item.focus.en}. Correct one place where the tone sounds too hard, too soft, or too evasive.`,
    fa: `منبع لینک‌شده را انجام بده. بعد بررسی کن آیا به هدف می‌رسی: ${item.focus.fa}. یک بخش را که لحنش بیش از حد تند، بیش از حد نرم یا بیش از حد طفره‌آمیز است اصلاح کن.`,
    ar: `اعمل على المورد المرتبط. ثم تحقق هل تصل إلى الهدف: ${item.focus.ar}. صحح موضعًا تبدو فيه النبرة حادة جدًا أو لينة جدًا أو مراوغة جدًا.`,
    tr: `Bağlantılı kaynağı çalış. Sonra hedefe ulaşıp ulaşmadığını kontrol et: ${item.focus.tr}. Tonun fazla sert, fazla yumuşak ya da fazla kaçamak olduğu bir yeri düzelt.`,
    ru: `Проработай связанный ресурс. Затем проверь, достигаешь ли цели: ${item.focus.ru}. Исправь одно место, где тон звучит слишком жестко, слишком мягко или слишком уклончиво.`,
    ckb: `سەرچاوەی بەستەرکراو کاربکە. پاشان بپشکنە ئایا دەگەیتە ئامانجەکە: ${item.focus.ckb}. شوێنێک ڕاست بکەرەوە کە تۆنەکە زۆر توند، زۆر نەرم یان زۆر لادەر دەردەکەوێت.`,
    kmr: `Çavkaniya girêdayî bixebitîne. Paşê kontrol bike gelo tu digihîjî armancê: ${item.focus.kmr}. Cihê ku ton pir hişk, pir nerm an pir revok xuya dike rast bike.`,
    pl: `Przerób podlinkowany materiał. Potem sprawdź, czy osiągasz cel: ${item.focus.pl}. Popraw jedno miejsce, w którym ton brzmi zbyt twardo, zbyt miękko albo zbyt wymijająco.`,
    ro: `Lucrează resursa legată. Apoi verifică dacă atingi scopul: ${item.focus.ro}. Corectează un loc în care tonul pare prea dur, prea moale sau prea evaziv.`,
    sq: `Puno me burimin e lidhur. Pastaj kontrollo nëse e arrin synimin: ${item.focus.sq}. Korrigjo një vend ku toni duket tepër i ashpër, tepër i butë ose tepër shmangës.`
  };
}

function transferInstruction() {
  return {
    de: 'Formuliere eine Kernbotschaft in vier Saetzen: Lage, Verantwortung, Grenze und naechster Schritt. Jeder Satz muss eine eigene Funktion haben und darf keine leere Managementfloskel sein.',
    en: 'Formulate a core message in four sentences: situation, responsibility, boundary, and next step. Each sentence must have its own function and must not be an empty management phrase.',
    fa: 'پیام اصلی را در چهار جمله بنویس: وضعیت، مسئولیت، مرز، و قدم بعدی. هر جمله باید کارکرد جداگانه داشته باشد و نباید فقط یک عبارت مدیریتی توخالی باشد.',
    ar: 'صغ رسالة أساسية في أربع جمل: الوضع، المسؤولية، الحد، والخطوة التالية. يجب أن تكون لكل جملة وظيفة خاصة، وألا تكون عبارة إدارية فارغة.',
    tr: 'Ana mesajı dört cümlede kur: durum, sorumluluk, sınır ve sonraki adım. Her cümlenin ayrı bir işlevi olmalı ve boş bir yönetim klişesi olmamalı.',
    ru: 'Сформулируй ключевое сообщение в четырех предложениях: ситуация, ответственность, граница и следующий шаг. У каждого предложения должна быть своя функция, и оно не должно быть пустым управленческим клише.',
    ckb: 'پەیامی سەرەکی لە چوار ڕستەدا بنووسە: دۆخ، بەرپرسیارێتی، سنوور و هەنگاوی داهاتوو. هەر ڕستەیەک دەبێت کاری تایبەتی هەبێت و نابێت دەربڕینێکی بەتاڵی بەڕێوەبەری بێت.',
    kmr: 'Peyama sereke di çar hevokan de ava bike: rewş, berpirsiyarî, sînor û gava din. Her hevok divê karê xwe hebe û ne klîşeyeke rêveberî ya vala be.',
    pl: 'Sformułuj komunikat główny w czterech zdaniach: sytuacja, odpowiedzialność, granica i następny krok. Każde zdanie musi mieć własną funkcję i nie może być pustym frazesem zarządczym.',
    ro: 'Formulează mesajul central în patru propoziții: situație, responsabilitate, limită și pasul următor. Fiecare propoziție trebuie să aibă o funcție proprie și să nu fie o formulă managerială goală.',
    sq: 'Formulo mesazhin kryesor në katër fjali: situata, përgjegjësia, kufiri dhe hapi tjetër. Çdo fjali duhet të ketë funksion të veçantë dhe të mos jetë frazë boshe menaxheriale.'
  };
}

function reviewInstruction(item) {
  return {
    de: `Pruefe deine vier Saetze mit drei Fragen: Wird das Risiko klar? Bleibt die Beziehung arbeitsfaehig? Ist der naechste Schritt konkret genug? Streiche alles, was bei ${item.topic.de} nur wichtig klingt, aber nichts klaert.`,
    en: `Check your four sentences with three questions: is the risk clear? Does the working relationship remain usable? Is the next step concrete enough? Remove anything that only sounds important in ${item.topic.en} but clarifies nothing.`,
    fa: `چهار جمله‌ات را با سه پرسش بررسی کن: آیا ریسک روشن است؟ آیا رابطه کاری هنوز قابل ادامه است؟ آیا قدم بعدی به اندازه کافی مشخص است؟ هر چیزی را که در موضوع ${item.topic.fa} فقط مهم به نظر می‌رسد اما چیزی را روشن نمی‌کند حذف کن.`,
    ar: `افحص الجمل الأربع بثلاثة أسئلة: هل الخطر واضح؟ هل تبقى علاقة العمل قابلة للاستمرار؟ هل الخطوة التالية محددة بما يكفي؟ احذف كل ما يبدو مهمًا في موضوع ${item.topic.ar} لكنه لا يوضح شيئًا.`,
    tr: `Dört cümleni üç soruyla kontrol et: risk açık mı? Çalışma ilişkisi hâlâ sürdürülebilir mi? Sonraki adım yeterince somut mu? ${item.topic.tr} konusunda yalnızca önemli görünen ama hiçbir şeyi açıklığa kavuşturmayan her şeyi sil.`,
    ru: `Проверь четыре предложения тремя вопросами: ясен ли риск? Сохраняются ли рабочие отношения? Достаточно ли конкретен следующий шаг? Убери все, что в теме ${item.topic.ru} только звучит важно, но ничего не проясняет.`,
    ckb: `چوار ڕستەکەت بە سێ پرسیار بپشکنە: ئایا مەترسییەکە ڕوونە؟ ئایا پەیوەندی کاری هێشتا بەردەوام دەبێت؟ ئایا هەنگاوی داهاتوو بە پێویستی دیاریکراوە؟ هەر شتێک بسڕەوە کە لە بابەتی ${item.topic.ckb} تەنها گرنگ دەردەکەوێت بەڵام هیچ شتێک ڕوون ناکات.`,
    kmr: `Çar hevokên xwe bi sê pirsan kontrol bike: rîsk zelal e? Têkiliya kar hîn dikare bidome? Gava din têra xwe konkret e? Her tiştê ku di mijara ${item.topic.kmr} de tenê girîng xuya dike lê tiştekî zelal nake jê bibe.`,
    pl: `Sprawdź cztery zdania trzema pytaniami: czy ryzyko jest jasne? Czy relacja robocza pozostaje możliwa? Czy następny krok jest dość konkretny? Usuń wszystko, co przy temacie ${item.topic.pl} tylko brzmi ważnie, ale niczego nie wyjaśnia.`,
    ro: `Verifică cele patru propoziții cu trei întrebări: riscul este clar? Relația de lucru rămâne funcțională? Pasul următor este suficient de concret? Elimină tot ce în tema ${item.topic.ro} doar sună important, dar nu clarifică nimic.`,
    sq: `Kontrollo katër fjalitë me tri pyetje: a është i qartë rreziku? A mbetet marrëdhënia e punës funksionale? A është hapi tjetër mjaft konkret? Hiq çdo gjë që në temën ${item.topic.sq} vetëm tingëllon e rëndësishme, por nuk sqaron asgjë.`
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
    createActivity('roleplay', 'target', targetInstruction(item), item.targetType, item.targetSlug, 10, 30),
    createActivity('practice', 'transfer', transferInstruction(), 'none', null, 8, 40),
    createActivity('review', 'review', reviewInstruction(item), 'none', null, 6, 50)
  ];
}

fs.writeFileSync(file, JSON.stringify(data, null, 2) + '\n', 'utf8');
console.log(`Updated ${items.length} C2 Module 6 lessons with ${items.length * 5} activity blocks.`);

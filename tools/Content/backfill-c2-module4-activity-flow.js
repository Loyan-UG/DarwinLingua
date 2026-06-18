const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c2-stil-souveraenitaet-und-komplexer-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'Fachtextfunktion klaeren',
    en: 'Clarify the expert-text function',
    fa: 'کارکرد متن تخصصی را روشن کن',
    ar: 'وضّح وظيفة النص المتخصص',
    tr: 'Uzman metnin işlevini netleştir',
    ru: 'Проясни функцию экспертного текста',
    ckb: 'ئەرکی دەقی پسپۆڕی ڕوون بکە',
    kmr: 'Erka nivîsa pisporî zelal bike',
    pl: 'Wyjaśnij funkcję tekstu eksperckiego',
    ro: 'Clarifică funcția textului de specialitate',
    sq: 'Qartëso funksionin e tekstit specialist'
  },
  language: {
    de: 'Registermittel pruefen',
    en: 'Check register tools',
    fa: 'ابزارهای register را بررسی کن',
    ar: 'افحص أدوات السجل اللغوي',
    tr: 'Register araçlarını kontrol et',
    ru: 'Проверь средства регистра',
    ckb: 'ئامرازەکانی ڕەجستەر بپشکنە',
    kmr: 'Amûrên registerê kontrol bike',
    pl: 'Sprawdź środki rejestru',
    ro: 'Verifică instrumentele de registru',
    sq: 'Kontrollo mjetet e regjistrit'
  },
  target: {
    de: 'Verstaendlichkeit testen',
    en: 'Test comprehensibility',
    fa: 'قابل‌فهم بودن را آزمایش کن',
    ar: 'اختبر قابلية الفهم',
    tr: 'Anlaşılırlığı sına',
    ru: 'Проверь понятность',
    ckb: 'تێگەیشتن ئاسانە یان نا بپشکنە',
    kmr: 'Têgihiştinê biceribîne',
    pl: 'Sprawdź zrozumiałość',
    ro: 'Testează inteligibilitatea',
    sq: 'Testo kuptueshmërinë'
  },
  transfer: {
    de: 'Fachliches umformulieren',
    en: 'Reformulate expert content',
    fa: 'محتوای تخصصی را بازنویسی کن',
    ar: 'أعد صياغة المحتوى المتخصص',
    tr: 'Uzman içeriği yeniden formüle et',
    ru: 'Переформулируй специальное содержание',
    ckb: 'ناوەڕۆکی پسپۆڕی دووبارە بنووسە',
    kmr: 'Naveroka pisporî ji nû ve formule bike',
    pl: 'Przeformułuj treść specjalistyczną',
    ro: 'Reformulează conținutul specializat',
    sq: 'Riformulo përmbajtjen specialistike'
  },
  review: {
    de: 'Praezision gegen Zugang abwaegen',
    en: 'Balance precision and access',
    fa: 'دقت و دسترس‌پذیری را با هم بسنج',
    ar: 'وازن بين الدقة وسهولة الوصول',
    tr: 'Kesinlik ile erişilebilirliği tart',
    ru: 'Сопоставь точность и доступность',
    ckb: 'وردی و ئاسان‌گەیشتن پێکەوە هەڵبسەنگێنە',
    kmr: 'Daqîqî û gihîştinê li hev binirxîne',
    pl: 'Wyważ precyzję i dostępność',
    ro: 'Cântărește precizia și accesibilitatea',
    sq: 'Balanco saktësinë dhe qasjen'
  }
};

const items = [
  {
    slug: 'c2-rechtsnahe-satzstrukturen-verstehen',
    topic: { de: 'rechtsnahe Satzstrukturen', en: 'legal-adjacent sentence structures', fa: 'ساختارهای جمله نزدیک به زبان حقوقی', ar: 'بنى الجمل القريبة من اللغة القانونية', tr: 'hukuk diline yakın cümle yapıları', ru: 'структуры предложений, близкие к юридическому стилю', ckb: 'ڕستەسازی نزیک بە زمانی یاسایی', kmr: 'avahiyên hevokî yên nêzî zimanê hiqûqî', pl: 'struktury zdań bliskie językowi prawnemu', ro: 'structuri de frază apropiate de limbajul juridic', sq: 'struktura fjalish pranë gjuhës juridike' },
    focus: { de: 'Pflicht, Ausnahme, Bedingung und Geltungsbereich sauber voneinander zu trennen', en: 'separating obligation, exception, condition, and scope cleanly', fa: 'اجبار، استثنا، شرط و محدوده اعتبار را تمیز از هم جدا کنی', ar: 'فصل الالتزام والاستثناء والشرط ونطاق السريان بوضوح', tr: 'yükümlülük, istisna, koşul ve geçerlilik alanını temiz biçimde ayırmak', ru: 'четко отделять обязанность, исключение, условие и область действия', ckb: 'ناچاری، جیاوازی، مەرج و سنوری کاریگەری بە ڕوونی جیا بکەیتەوە', kmr: 'erk, îstîsna, şert û qada derbasbûnê bi zelalî cuda bikî', pl: 'wyraźnie oddzielać obowiązek, wyjątek, warunek i zakres obowiązywania', ro: 'să separi clar obligația, excepția, condiția și domeniul de aplicare', sq: 'të ndash qartë detyrimin, përjashtimin, kushtin dhe fushën e vlefshmërisë' },
    grammar: 'c2-legal-style-sentence-structures',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-textabschnitt-nach-funktion-lesen'
  },
  {
    slug: 'c2-akademischer-hochregister-stil',
    topic: { de: 'akademischer Hochregister-Stil', en: 'high academic register', fa: 'register بالای دانشگاهی', ar: 'السجل الأكاديمي العالي', tr: 'yüksek akademik register', ru: 'высокий академический регистр', ckb: 'ڕەجستەری بەرزی ئەکادیمی', kmr: 'registera bilind a akademîk', pl: 'wysoki rejestr akademicki', ro: 'registru academic înalt', sq: 'regjistër i lartë akademik' },
    focus: { de: 'abstrakt und praezise zu schreiben, ohne Verantwortung hinter leeren Formeln zu verstecken', en: 'writing abstractly and precisely without hiding responsibility behind empty formulas', fa: 'انتزاعی و دقیق بنویسی، بدون اینکه مسئولیت را پشت فرمول‌های توخالی پنهان کنی', ar: 'الكتابة بتجريد ودقة من دون إخفاء المسؤولية وراء صيغ فارغة', tr: 'soyut ve kesin yazmak ama sorumluluğu boş formüllerin arkasına saklamamak', ru: 'писать абстрактно и точно, не пряча ответственность за пустыми формулами', ckb: 'بە شێوەیەکی ئەبستراکت و ورد بنووسیت، بەبێ شاردنەوەی بەرپرسیارێتی لە پشت فۆرمولی بەتاڵ', kmr: 'abstrakt û daqîq binivîsî bêyî berpirsiyariyê li pişt formûlên vala veşêrî', pl: 'pisać abstrakcyjnie i precyzyjnie, nie chowając odpowiedzialności za pustymi formułami', ro: 'să scrii abstract și precis fără să ascunzi responsabilitatea în formule goale', sq: 'të shkruash në mënyrë abstrakte dhe të saktë pa fshehur përgjegjësinë pas formulave boshe' },
    grammar: 'c2-academic-high-register-syntax',
    targetType: 'writing-template',
    targetSlug: 'c2-forschungsposition-mit-methodengrenze-darstellen'
  },
  {
    slug: 'c2-expertentexte-mit-nominalstil',
    topic: { de: 'Nominalstil in Expertentexten', en: 'nominal style in expert texts', fa: 'Nominalstil در متن‌های تخصصی', ar: 'الأسلوب الاسمي في النصوص المتخصصة', tr: 'uzman metinlerde adlaştırmalı üslup', ru: 'номинальный стиль в экспертных текстах', ckb: 'ستایلی ناوی لە دەقی پسپۆڕیدا', kmr: 'şêwaza navdêrî di nivîsên pisporî de', pl: 'styl nominalny w tekstach eksperckich', ro: 'stil nominal în texte de specialitate', sq: 'stil nominal në tekste specialistike' },
    focus: { de: 'Nominalisierungen zu entpacken und nur dort zu nutzen, wo sie Information wirklich buendeln', en: 'unpacking nominalizations and using them only where they truly bundle information', fa: 'Nominalisierungها را باز کنی و فقط جایی به کار ببری که واقعاً اطلاعات را فشرده و منظم می‌کنند', ar: 'تفكيك التحويلات الاسمية واستخدامها فقط عندما تجمع المعلومات فعلًا', tr: 'adlaştırmaları açmak ve yalnızca bilgiyi gerçekten topladığında kullanmak', ru: 'раскрывать номинализации и использовать их только там, где они действительно собирают информацию', ckb: 'ناوکردنەکان بکەیتەوە و تەنها لەو شوێنە بەکاری بهێنیت کە بەڕاستی زانیاری کۆدەکاتەوە', kmr: 'nomînalîzasyonan vekî û tenê li cihê ku rastî agahiyê kom dike bikar bînî', pl: 'rozpakowywać nominalizacje i używać ich tylko tam, gdzie naprawdę porządkują informacje', ro: 'să desfaci nominalizările și să le folosești doar când chiar grupează informația', sq: 't’i zbërthesh nominalizimet dhe t’i përdorësh vetëm kur vërtet përmbledhin informacion' },
    grammar: 'c2-nominal-style-in-expert-texts',
    targetType: 'writing-template',
    targetSlug: 'c2-expertentext-fuer-laien-verstaendlich-machen'
  },
  {
    slug: 'c2-passiv-und-akteursausblendung',
    topic: { de: 'Passiv und Akteursausblendung', en: 'passive voice and hiding the actor', fa: 'مجهول و پنهان شدن عامل انجام‌دهنده', ar: 'المبني للمجهول وإخفاء الفاعل', tr: 'edilgen yapı ve aktörün silinmesi', ru: 'пассив и скрытие действующего лица', ckb: 'پاسیڤ و شاردنەوەی ئەنجامدەر', kmr: 'passîv û veşartina kiryarvan', pl: 'strona bierna i ukrywanie sprawcy', ro: 'pasivul și ascunderea actorului', sq: 'pësorja dhe fshehja e vepruesit' },
    focus: { de: 'zu entscheiden, ob das Passiv neutralisiert, verdichtet oder Verantwortung verschleiert', en: 'deciding whether the passive neutralizes, condenses, or obscures responsibility', fa: 'تشخیص بدهی مجهول جمله را خنثی‌تر می‌کند، فشرده‌تر می‌کند یا مسئولیت را مبهم می‌کند', ar: 'تحديد هل يحيّد المبني للمجهول المعنى أو يكثفه أو يخفي المسؤولية', tr: 'edilgen yapının nötrleştirip yoğunlaştırdığını mı yoksa sorumluluğu mu örttüğünü ayırt etmek', ru: 'решать, делает ли пассив высказывание нейтральнее, плотнее или скрывает ответственность', ckb: 'بڕیار بدەیت پاسیڤ مانا بێلایەن دەکات، چڕی دەکاتەوە یان بەرپرسیارێتی دەشارێتەوە', kmr: 'biryar bidî gelo passîv bêalî dike, tije dike an berpirsiyariyê veşêre', pl: 'rozstrzygać, czy strona bierna neutralizuje, zagęszcza czy zaciera odpowiedzialność', ro: 'să decizi dacă pasivul neutralizează, concentrează sau ascunde responsabilitatea', sq: 'të vendosësh nëse pësorja e neutralizon, e ngjesh apo e errëson përgjegjësinë' },
    grammar: 'c2-advanced-passive-and-agent-omission',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-textstimme-von-autorposition-trennen'
  },
  {
    slug: 'c2-feine-unterschiede-bei-konnektoren',
    topic: { de: 'feine Unterschiede bei Konnektoren', en: 'subtle differences between connectors', fa: 'تفاوت‌های ظریف میان Konnektorها', ar: 'الفروق الدقيقة بين أدوات الربط', tr: 'bağlaçlar arasındaki ince farklar', ru: 'тонкие различия между связками', ckb: 'جیاوازی ورد لە نێوان بەستەرەکاندا', kmr: 'cudatiyên hûr di navbera girêderan de', pl: 'subtelne różnice między łącznikami', ro: 'diferențe fine între conectori', sq: 'dallime të holla mes lidhëzave' },
    focus: { de: 'Begruendung, Einschraenkung, Gegensatz und Folgerung nicht durch fast passende Konnektoren zu verwischen', en: 'not blurring reason, limitation, contrast, and conclusion with almost-fitting connectors', fa: 'دلیل، محدودیت، تضاد و نتیجه را با Konnektorهای تقریباً مناسب با هم قاطی نکنی', ar: 'ألا تخلط السبب والقيد والتضاد والنتيجة بأدوات ربط شبه مناسبة', tr: 'gerekçe, sınırlama, karşıtlık ve sonucu neredeyse uygun bağlaçlarla bulanıklaştırmamak', ru: 'не размывать причину, ограничение, противопоставление и вывод почти подходящими связками', ckb: 'هۆکار، سنووردانان، دژایەتی و دەرەنجام بە بەستەری نزیک بە گونجاو تێکەڵ نەکەیت', kmr: 'sedem, sînordarkirin, dijayetî û encamê bi girêderên hema-hema guncan tevlihev nekî', pl: 'nie zacierać uzasadnienia, ograniczenia, kontrastu i wniosku prawie pasującymi łącznikami', ro: 'să nu estompezi motivul, limitarea, contrastul și concluzia cu conectori aproape potriviți', sq: 'të mos i mjegullosh arsyen, kufizimin, kontrastin dhe përfundimin me lidhëza pothuajse të përshtatshme' },
    grammar: 'c2-fine-differences-in-connectors',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-uebergaenge-im-argument-pruefen'
  },
  {
    slug: 'c2-komplexe-indirekte-rede',
    topic: { de: 'komplexe indirekte Rede', en: 'complex reported speech', fa: 'نقل قول غیرمستقیم پیچیده', ar: 'النقل غير المباشر المعقّد', tr: 'karmaşık dolaylı anlatım', ru: 'сложная косвенная речь', ckb: 'گێڕانەوەی ناڕاستەوخۆی ئاڵۆز', kmr: 'axaftina neyekser a tevlihev', pl: 'złożona mowa zależna', ro: 'vorbire indirectă complexă', sq: 'ligjëratë e zhdrejtë komplekse' },
    focus: { de: 'fremde Positionen praezise wiederzugeben, ohne Zustimmung, Distanz oder Zweifel versehentlich mitzuschreiben', en: 'reporting other positions precisely without accidentally adding agreement, distance, or doubt', fa: 'موضع دیگران را دقیق بازگویی کنی، بدون اینکه ناخواسته موافقت، فاصله‌گیری یا تردید اضافه کنی', ar: 'نقل مواقف الآخرين بدقة من دون إضافة موافقة أو مسافة أو شك من غير قصد', tr: 'başkalarının pozisyonlarını yanlışlıkla onay, mesafe veya kuşku eklemeden aktarmak', ru: 'точно передавать чужие позиции, случайно не добавляя согласия, дистанции или сомнения', ckb: 'هەڵوێستی کەسانی تر بە وردی بگێڕیتەوە بەبێ ئەوەی بە هەڵە ڕەزامەندی، دووری یان گومان زیاد بکەیت', kmr: 'helwestên kesên din bi daqîqî veguhêzî bêyî ku bê qest erêkirin, dûrahî an guman lê zêde bikî', pl: 'precyzyjnie relacjonować cudze stanowiska bez przypadkowego dodania zgody, dystansu lub wątpliwości', ro: 'să redai precis pozițiile altora fără să adaugi involuntar acord, distanță sau îndoială', sq: 'të riprodhosh saktë pozicionet e të tjerëve pa shtuar pa dashje pajtim, distancë ose dyshim' },
    grammar: 'c2-complex-reported-speech',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-quellenstatus-im-hoeren-erkennen'
  },
  {
    slug: 'c2-technische-sachverhalte-verstaendlich-erklaeren',
    topic: { de: 'technische Sachverhalte', en: 'technical issues', fa: 'موضوعات فنی', ar: 'الموضوعات التقنية', tr: 'teknik konular', ru: 'технические вопросы', ckb: 'بابەتی تەکنیکی', kmr: 'mijarên teknîkî', pl: 'zagadnienia techniczne', ro: 'aspecte tehnice', sq: 'çështje teknike' },
    focus: { de: 'fachliche Genauigkeit zu behalten und trotzdem Laien Orientierung, Reihenfolge und Konsequenz zu geben', en: 'keeping technical accuracy while still giving non-specialists orientation, sequence, and consequence', fa: 'دقت تخصصی را حفظ کنی و همزمان به مخاطب غیرمتخصص جهت، ترتیب و پیامد را نشان بدهی', ar: 'الحفاظ على الدقة المتخصصة مع إعطاء غير المختصين اتجاهًا وتسلسلًا ونتيجة', tr: 'uzmanlık doğruluğunu koruyup uzman olmayanlara yön, sıra ve sonuç göstermek', ru: 'сохранять специальную точность и при этом давать неспециалистам ориентацию, порядок и последствия', ckb: 'وردی پسپۆڕی بپارێزیت و هەمان کات ئاراستە، ڕیزبەندی و دەرەنجام بۆ کەسی ناپسپۆڕ ڕوون بکەیت', kmr: 'daqîqiya pisporî biparêzî û di heman demê de ji nepisporan re araste, rêz û encam bidî', pl: 'zachować fachową dokładność, a jednocześnie dać laikom orientację, kolejność i konsekwencje', ro: 'să păstrezi precizia tehnică și totuși să oferi nespecialiștilor orientare, ordine și consecință', sq: 'të ruash saktësinë teknike dhe njëkohësisht t’u japësh jo-specialistëve orientim, radhë dhe pasojë' },
    grammar: 'c2-nominal-style-in-expert-texts',
    targetType: 'writing-template',
    targetSlug: 'c2-expertentext-fuer-laien-verstaendlich-machen'
  },
  {
    slug: 'c2-redaktionelle-kompression-und-neutralitaet',
    topic: { de: 'redaktionelle Kompression und Neutralitaet', en: 'editorial compression and neutrality', fa: 'فشرده‌سازی تحریریه‌ای و بی‌طرفی', ar: 'التكثيف التحريري والحياد', tr: 'editoryal yoğunlaştırma ve tarafsızlık', ru: 'редакционное сжатие и нейтральность', ckb: 'چڕکردنەوەی دەستکاریی دەق و بێلایەنی', kmr: 'kompresyona edîtorî û bêalîtî', pl: 'redakcyjna kompresja i neutralność', ro: 'compresie editorială și neutralitate', sq: 'ngjeshje redaktoriale dhe neutralitet' },
    focus: { de: 'Information zu verdichten, ohne Bewertung, Quelle oder Unsicherheit unsichtbar zu machen', en: 'condensing information without making evaluation, source, or uncertainty invisible', fa: 'اطلاعات را فشرده کنی، بدون اینکه ارزیابی، منبع یا عدم قطعیت نامرئی شود', ar: 'تكثيف المعلومات من دون إخفاء التقييم أو المصدر أو عدم اليقين', tr: 'değerlendirme, kaynak veya belirsizliği görünmez kılmadan bilgiyi yoğunlaştırmak', ru: 'сжимать информацию, не скрывая оценку, источник или неопределенность', ckb: 'زانیاری چڕ بکەیتەوە بەبێ ئەوەی هەڵسەنگاندن، سەرچاوە یان نادڵنیایی نەبینراو بێت', kmr: 'agahiyê tije bikî bêyî ku nirxandin, çavkanî an nediyarbûn ne xuya bimîne', pl: 'zagęszczać informację bez ukrywania oceny, źródła lub niepewności', ro: 'să comprimi informația fără să faci invizibile evaluarea, sursa sau incertitudinea', sq: 'ta ngjeshësh informacionin pa e bërë të padukshëm vlerësimin, burimin ose pasigurinë' },
    grammar: 'c2-journalistic-compression',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-textstimme-von-autorposition-trennen'
  },
  {
    slug: 'c2-fachregister-sicher-wechseln',
    topic: { de: 'sicherer Fachregisterwechsel', en: 'safe switching of expert register', fa: 'تغییر امن register تخصصی', ar: 'الانتقال الآمن بين السجلات المتخصصة', tr: 'uzman registerını güvenli değiştirme', ru: 'уверенное переключение специального регистра', ckb: 'گۆڕینی دڵنیای ڕەجستەری پسپۆڕی', kmr: 'guhertina ewle ya registera pisporî', pl: 'bezpieczna zmiana rejestru specjalistycznego', ro: 'schimbare sigură a registrului de specialitate', sq: 'ndërrim i sigurt i regjistrit specialist' },
    focus: { de: 'zwischen Fachpublikum, Verwaltung, Team und Laien zu wechseln, ohne Inhalt oder Haltung zu verlieren', en: 'switching between expert audience, administration, team, and laypeople without losing content or stance', fa: 'میان مخاطب تخصصی، اداره، تیم و افراد غیرمتخصص جابه‌جا شوی، بدون اینکه محتوا یا موضعت از دست برود', ar: 'الانتقال بين جمهور متخصص وإدارة وفريق وأشخاص غير مختصين من دون فقدان المحتوى أو الموقف', tr: 'uzman kitle, idare, ekip ve uzman olmayanlar arasında içerik ya da tutumu kaybetmeden geçiş yapmak', ru: 'переключаться между экспертной аудиторией, администрацией, командой и неспециалистами, не теряя содержания или позиции', ckb: 'لە نێوان گوێگری پسپۆڕ، ئیدارە، تیم و کەسانی ناپسپۆڕدا بگۆڕیت بەبێ لەدەستدانی ناوەڕۆک یان هەڵوێست', kmr: 'di navbera temaşevanên pispor, rêveberî, tîm û nepisporan de biguherî bêyî naverok an helwest winda bibe', pl: 'przełączać się między odbiorcą fachowym, administracją, zespołem i laikami bez utraty treści ani stanowiska', ro: 'să treci între public de specialitate, administrație, echipă și nespecialiști fără să pierzi conținutul sau poziția', sq: 'të kalosh mes publikut specialist, administratës, ekipit dhe jo-specialistëve pa humbur përmbajtjen ose qëndrimin' },
    grammar: 'c2-register-and-syntactic-choice',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-interdisziplinaere-perspektive-vermitteln'
  },
  {
    slug: 'c2-expertentexte-und-fachregister-wiederholen',
    topic: { de: 'Expertentexte und Fachregister', en: 'expert texts and specialist register', fa: 'متن‌های تخصصی و register تخصصی', ar: 'النصوص المتخصصة والسجل المتخصص', tr: 'uzman metinler ve uzman registerı', ru: 'экспертные тексты и специальный регистр', ckb: 'دەقە پسپۆڕییەکان و ڕەجستەری پسپۆڕی', kmr: 'nivîsên pisporî û registera pisporî', pl: 'teksty eksperckie i rejestr specjalistyczny', ro: 'texte de specialitate și registru specializat', sq: 'tekste specialistike dhe regjistër specialist' },
    focus: { de: 'eine persoenliche Routine fuer Praezision, Transparenz, Zielgruppe und Verantwortung zu nutzen', en: 'using a personal routine for precision, transparency, audience, and responsibility', fa: 'یک روال شخصی برای دقت، شفافیت، مخاطب و مسئولیت به کار ببری', ar: 'استخدام روتين شخصي للدقة والشفافية والجمهور والمسؤولية', tr: 'kesinlik, şeffaflık, hedef kitle ve sorumluluk için kişisel bir rutin kullanmak', ru: 'использовать личную процедуру для точности, прозрачности, аудитории и ответственности', ckb: 'ڕووتینێکی تایبەتی بۆ وردی، ڕوونی، گوێگر و بەرپرسیارێتی بەکاربهێنیت', kmr: 'rêbazeke xwe ji bo daqîqî, zelalî, temaşevan û berpirsiyarî bikar bînî', pl: 'stosować własną rutynę dla precyzji, przejrzystości, odbiorcy i odpowiedzialności', ro: 'să folosești o rutină proprie pentru precizie, transparență, public și responsabilitate', sq: 'të përdorësh një rutinë personale për saktësi, transparencë, audiencë dhe përgjegjësi' },
    grammar: 'c2-c2-grammar-review-map',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-bewertungskriterien-in-handlungen-uebersetzen'
  }
];

function translationArray(map) {
  return langs.map(language => ({ language, text: map[language] }));
}

function orientInstruction(item) {
  return {
    de: `Lies den Lektionstext zu ${item.topic.de}. Bestimme zuerst die Funktion: definiert, begruendet, grenzt ab, fasst zusammen oder steuert Verantwortung?`,
    en: `Read the lesson text on ${item.topic.en}. First identify the function: does it define, justify, delimit, summarize, or manage responsibility?`,
    fa: `متن درس درباره ${item.topic.fa} را بخوان. اول کارکرد آن را مشخص کن: تعریف می‌کند، دلیل می‌آورد، مرزبندی می‌کند، خلاصه می‌کند یا مسئولیت را مدیریت می‌کند؟`,
    ar: `اقرأ نص الدرس عن ${item.topic.ar}. حدّد الوظيفة أولًا: هل يعرّف، يبرّر، يضع حدودًا، يلخّص أم يدير المسؤولية؟`,
    tr: `${item.topic.tr} hakkındaki ders metnini oku. Önce işlevi belirle: tanımlıyor mu, gerekçelendiriyor mu, sınır çiziyor mu, özetliyor mu yoksa sorumluluğu mu yönetiyor?`,
    ru: `Прочитай урок о теме: ${item.topic.ru}. Сначала определи функцию: текст определяет, обосновывает, ограничивает, суммирует или управляет ответственностью?`,
    ckb: `دەقی وانەکە دەربارەی ${item.topic.ckb} بخوێنەوە. سەرەتا ئەرکەکەی دیاری بکە: پێناسە دەکات، پاساو دەهێنێتەوە، سنوور دادەنێت، کورت دەکاتەوە یان بەرپرسیارێتی بەڕێوە دەبات؟`,
    kmr: `Nivîsa dersê derbarê ${item.topic.kmr} bixwîne. Pêşî erkê diyar bike: pênase dike, sedem tîne, sînor dide, kurt dike an berpirsiyariyê birêve dibe?`,
    pl: `Przeczytaj tekst lekcji o: ${item.topic.pl}. Najpierw określ funkcję: definiuje, uzasadnia, wyznacza granice, streszcza czy zarządza odpowiedzialnością?`,
    ro: `Citește textul lecției despre ${item.topic.ro}. Stabilește mai întâi funcția: definește, justifică, delimitează, rezumă sau gestionează responsabilitatea?`,
    sq: `Lexo tekstin e mësimit për ${item.topic.sq}. Së pari përcakto funksionin: përkufizon, arsyeton, kufizon, përmbledh apo menaxhon përgjegjësinë?`
  };
}

function languageInstruction(item) {
  return {
    de: `Oeffne den verlinkten Grammatikpunkt und sammle drei Mittel, mit denen du ${item.focus.de}.`,
    en: `Open the linked grammar point and collect three tools that help you ${item.focus.en}.`,
    fa: `بخش گرامر لینک‌شده را باز کن و سه ابزار زبانی پیدا کن که کمک می‌کنند ${item.focus.fa}.`,
    ar: `افتح نقطة القواعد المرتبطة واجمع ثلاث أدوات لغوية تساعدك على ${item.focus.ar}.`,
    tr: `Bağlantılı dilbilgisi bölümünü aç ve sana şunda yardımcı olan üç araç bul: ${item.focus.tr}.`,
    ru: `Открой связанный раздел грамматики и собери три средства, которые помогают ${item.focus.ru}.`,
    ckb: `خاڵی ڕێزمانی بەستەرکراو بکەرەوە و سێ ئامرازی زمانی بدۆزەوە کە یارمەتیت دەدەن ${item.focus.ckb}.`,
    kmr: `Xala rêzimanê ya girêdayî veke û sê amûrên zimanî bibîne ku alîkarin ${item.focus.kmr}.`,
    pl: `Otwórz podlinkowany punkt gramatyczny i zbierz trzy środki, które pomagają ${item.focus.pl}.`,
    ro: `Deschide punctul de gramatică legat și adună trei instrumente care te ajută să ${item.focus.ro}.`,
    sq: `Hap pikën e lidhur të gramatikës dhe mblidh tri mjete që të ndihmojnë ${item.focus.sq}.`
  };
}

function targetInstruction(item) {
  return {
    de: `Bearbeite die verlinkte Ressource. Pruefe danach, ob dein Text oder Beitrag fuer Fachleute praezise und fuer die Zielgruppe nachvollziehbar bleibt.`,
    en: `Work through the linked resource. Then check whether your text or contribution remains precise for experts and understandable for the target audience.`,
    fa: `منبع لینک‌شده را انجام بده. بعد بررسی کن آیا متن یا مشارکت تو برای متخصصان دقیق و برای مخاطب هدف قابل‌پیگیری می‌ماند.`,
    ar: `اعمل على المورد المرتبط. ثم تحقق هل يبقى نصك أو مداخلتك دقيقًا للمتخصصين ومفهومًا للجمهور المستهدف.`,
    tr: `Bağlantılı kaynağı çalış. Sonra metnin ya da katkının uzmanlar için kesin, hedef kitle için izlenebilir kalıp kalmadığını kontrol et.`,
    ru: `Проработай связанный ресурс. Затем проверь, остается ли твой текст или вклад точным для специалистов и понятным для целевой аудитории.`,
    ckb: `سەرچاوەی بەستەرکراو کاربکە. پاشان بپشکنە ئایا دەق یان بەشدارییەکەت بۆ پسپۆڕان ورد و بۆ گوێگری ئامانج ئاسان بۆ شوێنکەوتن دەمێنێت.`,
    kmr: `Çavkaniya girêdayî bixebitîne. Paşê binirxîne gelo nivîs an beşdariya te ji bo pisporan daqîq û ji bo temaşevanên armanc têgihîştinbar dimîne.`,
    pl: `Przerób podlinkowany materiał. Potem sprawdź, czy twój tekst lub wypowiedź pozostaje precyzyjna dla specjalistów i zrozumiała dla odbiorcy docelowego.`,
    ro: `Lucrează resursa legată. Apoi verifică dacă textul sau intervenția ta rămâne precisă pentru specialiști și ușor de urmărit pentru publicul țintă.`,
    sq: `Puno me burimin e lidhur. Pastaj kontrollo nëse teksti ose ndërhyrja jote mbetet e saktë për specialistët dhe e ndjekshme për audiencën e synuar.`
  };
}

function transferInstruction() {
  return {
    de: 'Schreibe einen fachlichen Satz in zwei Fassungen: eine fuer Expertinnen und Experten, eine fuer informierte Laien. Markiere, welche Information du vereinfacht, aber nicht veraendert hast.',
    en: 'Write one expert sentence in two versions: one for specialists and one for informed non-specialists. Mark which information you simplified but did not change.',
    fa: 'یک جمله تخصصی را در دو نسخه بنویس: یکی برای متخصصان و یکی برای افراد غیرمتخصص اما آگاه. مشخص کن کدام اطلاعات را ساده‌تر کردی، اما تغییر ندادی.',
    ar: 'اكتب جملة متخصصة بصيغتين: صيغة للمتخصصين وصيغة لغير المتخصصين المطلعين. حدّد أي معلومة بسّطتها من دون تغييرها.',
    tr: 'Bir uzmanlık cümlesini iki sürümde yaz: biri uzmanlar için, biri bilgili ama uzman olmayanlar için. Hangi bilgiyi basitleştirdiğini ama değiştirmediğini işaretle.',
    ru: 'Напиши одно специальное предложение в двух версиях: для специалистов и для информированных неспециалистов. Отметь, какую информацию ты упростил, но не изменил.',
    ckb: 'ڕستەیەکی پسپۆڕی بە دوو وەشان بنووسە: یەکێک بۆ پسپۆڕان، یەکێک بۆ کەسانی ناپسپۆڕی ئاگادار. دیاری بکە کام زانیاریت سادەتر کردووە، بەڵام نەگۆڕیوە.',
    kmr: 'Hevokeke pisporî bi du guhertoyan binivîse: yek ji bo pisporan, yek ji bo nepisporên agahdar. Nîşan bike kîjan agahî te hêsantir kiriye lê neguherandiye.',
    pl: 'Napisz jedno zdanie fachowe w dwóch wersjach: dla specjalistów i dla świadomych laików. Zaznacz, którą informację uprościłeś, ale jej nie zmieniłeś.',
    ro: 'Scrie o frază de specialitate în două versiuni: una pentru specialiști și una pentru nespecialiști informați. Marchează ce informație ai simplificat, dar nu ai schimbat.',
    sq: 'Shkruaj një fjali specialistike në dy versione: një për specialistë dhe një për jo-specialistë të informuar. Shëno cilin informacion e thjeshtove, por nuk e ndryshove.'
  };
}

function reviewInstruction() {
  return {
    de: 'Notiere deine Kontrollfrage: Welche Praezision darf ich nicht verlieren, und welche Fachformulierung muss ich fuer diese Zielgruppe erklaeren?',
    en: 'Write your control question: which precision must I not lose, and which specialist formulation must I explain for this audience?',
    fa: 'پرسش کنترل خودت را بنویس: کدام دقت را نباید از دست بدهم، و کدام فرمول‌بندی تخصصی را باید برای این مخاطب توضیح بدهم؟',
    ar: 'دوّن سؤال التحكم الخاص بك: أي دقة لا يجوز أن أفقدها، وأي صياغة متخصصة يجب أن أشرحها لهذا الجمهور؟',
    tr: 'Kontrol sorunu yaz: Hangi kesinliği kaybetmemeliyim ve bu hedef kitle için hangi uzmanlık ifadesini açıklamalıyım?',
    ru: 'Запиши контрольный вопрос: какую точность нельзя потерять и какую специальную формулировку нужно объяснить этой аудитории?',
    ckb: 'پرسیاری کۆنترۆڵی خۆت بنووسە: کام وردی نابێت لەدەست بدەم، و کام داڕشتنی پسپۆڕی پێویستە بۆ ئەم گوێگرە ڕوون بکەمەوە؟',
    kmr: 'Pirsa kontrola xwe binivîse: kîjan daqîqî divê winda nekim, û kîjan formulekirina pisporî divê ji bo vê temaşevanê rave bikim?',
    pl: 'Zapisz pytanie kontrolne: jakiej precyzji nie wolno mi stracić i którą fachową formułę muszę wyjaśnić tej grupie odbiorców?',
    ro: 'Notează întrebarea de control: ce precizie nu trebuie să pierd și ce formulare de specialitate trebuie să explic acestui public?',
    sq: 'Shëno pyetjen kontrolluese: cilën saktësi nuk duhet ta humbas dhe cilën formulim specialist duhet ta shpjegoj për këtë audiencë?'
  };
}

function makeActivity(kind, titleKey, instructionMap, targetType, targetSlug, sortOrder, estimatedMinutes) {
  return {
    kind,
    title: titles[titleKey].de,
    titleTranslations: translationArray(titles[titleKey]),
    instruction: instructionMap.de,
    instructionTranslations: translationArray(instructionMap),
    targetType,
    targetSlug: targetType === 'none' ? null : targetSlug,
    estimatedMinutes,
    sortOrder,
    isRequired: true
  };
}

const bySlug = new Map(lessons.map(lesson => [lesson.slug, lesson]));
for (const item of items) {
  const lesson = bySlug.get(item.slug);
  if (!lesson) {
    throw new Error(`Lesson not found: ${item.slug}`);
  }

  lesson.activityBlocks = [
    makeActivity('read', 'orient', orientInstruction(item), 'none', null, 10, 7),
    makeActivity('grammar', 'language', languageInstruction(item), 'grammar-topic', item.grammar, 20, 9),
    makeActivity(item.targetType === 'writing-template' ? 'write' : item.targetType === 'roleplay' ? 'roleplay' : 'exam-prep', 'target', targetInstruction(item), item.targetType, item.targetSlug, 30, 10),
    makeActivity('practice', 'transfer', transferInstruction(), 'none', null, 40, 9),
    makeActivity('review', 'review', reviewInstruction(), 'none', null, 50, 5)
  ];
}

fs.writeFileSync(file, `${JSON.stringify(data, null, 2)}\n`, 'utf8');
console.log(`Updated ${items.length} C2 Module 4 lessons with ${items.length * 5} activity blocks.`);

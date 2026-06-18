const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c2-stil-souveraenitaet-und-komplexer-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'Wirkung bewusst lesen',
    en: 'Read for intended effect',
    fa: 'اثر متن را آگاهانه بخوان',
    ar: 'اقرأ الأثر المقصود بوعي',
    tr: 'Etkisini bilinçli oku',
    ru: 'Осознанно читай эффект текста',
    ckb: 'کاریگەری دەق بە ئاگایی بخوێنەوە',
    kmr: 'Bandora nivîsê bi hişyarî bixwîne',
    pl: 'Czytaj z uwagą na efekt',
    ro: 'Citește conștient efectul',
    sq: 'Lexo me vetëdije efektin'
  },
  language: {
    de: 'Form und Register pruefen',
    en: 'Check form and register',
    fa: 'فرم و register را بررسی کن',
    ar: 'افحص الشكل والسجل اللغوي',
    tr: 'Biçimi ve registerı kontrol et',
    ru: 'Проверь форму и регистр',
    ckb: 'فۆرم و ڕەجستەر بپشکنە',
    kmr: 'Form û registerê kontrol bike',
    pl: 'Sprawdź formę i rejestr',
    ro: 'Verifică forma și registrul',
    sq: 'Kontrollo formën dhe regjistrin'
  },
  target: {
    de: 'Praxiswirkung testen',
    en: 'Test the effect in practice',
    fa: 'اثر را در عمل آزمایش کن',
    ar: 'اختبر الأثر عمليًا',
    tr: 'Etkisini pratikte sına',
    ru: 'Проверь эффект на практике',
    ckb: 'کاریگەری بە کرداری بپشکنە',
    kmr: 'Bandorê di pratîkê de biceribîne',
    pl: 'Sprawdź efekt w praktyce',
    ro: 'Testează efectul în practică',
    sq: 'Testo efektin në praktikë'
  },
  transfer: {
    de: 'Absatz stilistisch ueberarbeiten',
    en: 'Revise a paragraph stylistically',
    fa: 'یک بند را از نظر سبک بازنویسی کن',
    ar: 'نقّح فقرة من ناحية الأسلوب',
    tr: 'Bir paragrafı üslup açısından düzelt',
    ru: 'Стилистически отредактируй абзац',
    ckb: 'پەرەگرافێک لە ڕووی ستایلەوە چاک بکە',
    kmr: 'Paragrafekê ji aliyê şêwazê ve sererast bike',
    pl: 'Przeredaguj akapit stylistycznie',
    ro: 'Revizuiește stilistic un paragraf',
    sq: 'Rishkruaj stilistikisht një paragraf'
  },
  review: {
    de: 'Stilentscheidung begruenden',
    en: 'Justify the stylistic choice',
    fa: 'انتخاب سبکی را توضیح بده',
    ar: 'برّر الاختيار الأسلوبي',
    tr: 'Üslup seçimini gerekçelendir',
    ru: 'Обоснуй стилистический выбор',
    ckb: 'بڕیاری ستایلی ڕوون بکەوە',
    kmr: 'Hilbijartina şêwazî rave bike',
    pl: 'Uzasadnij wybór stylistyczny',
    ro: 'Justifică alegerea stilistică',
    sq: 'Arsyeto zgjedhjen stilistike'
  }
};

const items = [
  {
    slug: 'c2-stilistische-variation-gezielt-nutzen',
    topic: { de: 'stilistische Variation', en: 'stylistic variation', fa: 'تنوع سبکی', ar: 'التنويع الأسلوبي', tr: 'üslup çeşitliliği', ru: 'стилистическое варьирование', ckb: 'جیاوازی ستایلی', kmr: 'guhertina şêwazî', pl: 'wariant stylistyczny', ro: 'variația stilistică', sq: 'ndryshimi stilistik' },
    focus: { de: 'eine Form nicht nur abwechslungsreich, sondern passend zur beabsichtigten Wirkung einzusetzen', en: 'using a form not merely for variety, but because it fits the intended effect', fa: 'یک فرم را نه فقط برای تنوع، بلکه چون با اثر مورد نظر متناسب است به کار ببری', ar: 'استخدام صيغة لا لمجرد التنويع، بل لأنها تناسب الأثر المقصود', tr: 'bir biçimi yalnızca çeşitlilik için değil, hedeflenen etkiye uygun olduğu için kullanmak', ru: 'использовать форму не просто ради разнообразия, а потому что она подходит к нужному эффекту', ckb: 'فۆرمێک تەنها بۆ جیاوازی بەکارنەهێنیت، بەڵکو چونکە لەگەڵ کاریگەری مەبەستدار گونجاوە', kmr: 'formekê ne tenê ji bo cihêrengiyê, lê ji ber ku bi bandora armanc re tê bikaranîn', pl: 'użyć formy nie tylko dla urozmaicenia, lecz dlatego, że pasuje do zamierzonego efektu', ro: 'să folosești o formă nu doar pentru varietate, ci pentru că se potrivește efectului dorit', sq: 'ta përdorësh një formë jo vetëm për larmi, por sepse i përshtatet efektit të synuar' },
    grammar: 'c2-stylistic-variation-in-german-grammar',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-stilistische-dichte-dosieren'
  },
  {
    slug: 'c2-register-und-syntaxwahl',
    topic: { de: 'Register und Syntaxwahl', en: 'register and syntax choice', fa: 'register و انتخاب ساختار جمله', ar: 'السجل اللغوي واختيار البنية النحوية', tr: 'register ve söz dizimi seçimi', ru: 'регистр и выбор синтаксиса', ckb: 'ڕەجستەر و هەڵبژاردنی ڕستەسازی', kmr: 'register û hilbijartina hevoksaziyê', pl: 'rejestr i wybór składni', ro: 'registrul și alegerea sintaxei', sq: 'regjistri dhe zgjedhja sintaksore' },
    focus: { de: 'Syntax so zu waehlen, dass Naehe, Distanz, Autoritaet oder Kooperation klar gesteuert werden', en: 'choosing syntax so that closeness, distance, authority, or cooperation are clearly controlled', fa: 'ساختار جمله را طوری انتخاب کنی که نزدیکی، فاصله، اقتدار یا همکاری روشن و کنترل‌شده منتقل شود', ar: 'اختيار البنية النحوية بحيث تُدار القرب أو المسافة أو السلطة أو التعاون بوضوح', tr: 'söz dizimini yakınlık, mesafe, otorite ya da iş birliği açıkça yönetilecek şekilde seçmek', ru: 'выбирать синтаксис так, чтобы ясно управлять близостью, дистанцией, авторитетом или сотрудничеством', ckb: 'ڕستەسازی بە شێوەیەک هەڵبژێریت کە نزیکی، دووری، دەسەڵات یان هاوکاری بە ڕوونی کۆنترۆڵ بکرێت', kmr: 'hevoksaziyê wisa hilbijêrî ku nêzîkî, dûrahî, otorîte an hevkariyê bi zelalî birêve bibe', pl: 'dobrać składnię tak, aby jasno sterować bliskością, dystansem, autorytetem lub współpracą', ro: 'să alegi sintaxa astfel încât apropierea, distanța, autoritatea sau cooperarea să fie controlate clar', sq: 'ta zgjedhësh sintaksën që afërsia, distanca, autoriteti ose bashkëpunimi të drejtohen qartë' },
    grammar: 'c2-register-and-syntactic-choice',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-registerwechsel-kontrolliert-einsetzen'
  },
  {
    slug: 'c2-rhetorische-satzstrukturen',
    topic: { de: 'rhetorische Satzstrukturen', en: 'rhetorical sentence structures', fa: 'ساختارهای بلاغی جمله', ar: 'البنى الجملية البلاغية', tr: 'retorik cümle yapıları', ru: 'риторические структуры предложения', ckb: 'ڕستەسازی ڕەوانبێژی', kmr: 'avahiyên hevokî yên retorîkî', pl: 'retoryczne struktury zdań', ro: 'structuri sintactice retorice', sq: 'struktura fjalish retorike' },
    focus: { de: 'Fragen, Zuspitzungen oder Gegensaetze zu nutzen, ohne manipulativ oder kuenstlich zu wirken', en: 'using questions, sharpening, or contrasts without sounding manipulative or artificial', fa: 'پرسش، برجسته‌سازی یا تقابل را طوری به کار ببری که دستکاری‌گرانه یا تصنعی به نظر نرسد', ar: 'استخدام الأسئلة أو التشديد أو التضاد من دون أن يبدو الكلام تلاعبًا أو تصنعًا', tr: 'soruları, keskinleştirmeyi veya karşıtlıkları yapay ya da manipülatif görünmeden kullanmak', ru: 'использовать вопросы, заострение или противопоставления без манипулятивного или искусственного звучания', ckb: 'پرسیار، توندکردنەوە یان دژایەتی بەکاربهێنیت بەبێ ئەوەی دەستکاری‌کارانە یان ساختەیی دەربکەوێت', kmr: 'pirs, tundkirin an dijayetiyan bikar bînî bêyî ku wek manipulasyon an çêkirî xuya bike', pl: 'użyć pytań, zaostrzenia lub kontrastów bez wrażenia manipulacji albo sztuczności', ro: 'să folosești întrebări, accentuări sau contraste fără să pari manipulator sau artificial', sq: 'të përdorësh pyetje, theksim ose kontrast pa u dukur manipulues apo artificial' },
    grammar: 'c2-rhetorical-sentence-structures',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-rhetorische-frage-in-einer-debatte-aufgreifen'
  },
  {
    slug: 'c2-ellipse-und-auslassung-verstehen',
    topic: { de: 'Ellipse und Auslassung', en: 'ellipsis and omission', fa: 'حذف و ناگفته‌گذاشتن بخشی از جمله', ar: 'الحذف والإضمار', tr: 'eksiltme ve atlama', ru: 'эллипсис и пропуск', ckb: 'لابردن و بەجێهێشتنی بەشێک لە ڕستە', kmr: 'kêmkirin û berdana beşek ji hevokê', pl: 'elipsa i opuszczenie', ro: 'elipsa și omisiunea', sq: 'elipsa dhe lënia jashtë' },
    focus: { de: 'Auslassungen als Stilmittel zu erkennen und nur dort zu nutzen, wo der Bezug eindeutig bleibt', en: 'recognizing omissions as a stylistic device and using them only where the reference remains clear', fa: 'حذف را به‌عنوان ابزار سبکی بشناسی و فقط جایی استفاده کنی که مرجع جمله روشن می‌ماند', ar: 'تمييز الحذف كأداة أسلوبية واستخدامه فقط عندما تبقى الإحالة واضحة', tr: 'eksiltmeyi bir üslup aracı olarak tanımak ve yalnızca gönderme açık kaldığında kullanmak', ru: 'распознавать пропуск как стилистический прием и использовать его только там, где ссылка остается ясной', ckb: 'لابردن وەک ئامرازی ستایلی بناسیت و تەنها لەو شوێنە بەکاری بهێنیت کە ئاماژەکە ڕوون دەمێنێت', kmr: 'kêmkirinê wek amûrek şêwazî nas bikî û tenê li cihê ku girêdan zelal dimîne bikar bînî', pl: 'rozpoznać opuszczenie jako środek stylistyczny i używać go tylko tam, gdzie odniesienie pozostaje jasne', ro: 'să recunoști omisiunea ca instrument stilistic și să o folosești doar când referința rămâne clară', sq: 'ta njohësh lënien jashtë si mjet stilistik dhe ta përdorësh vetëm kur lidhja mbetet e qartë' },
    grammar: 'c2-advanced-ellipsis',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-mehrdeutigkeit-produktiv-nutzen'
  },
  {
    slug: 'c2-parallele-strukturen-und-klang',
    topic: { de: 'parallele Strukturen und Klang', en: 'parallel structures and sound', fa: 'ساختارهای موازی و آهنگ جمله', ar: 'البنى المتوازية وإيقاع الجملة', tr: 'paralel yapılar ve cümle ritmi', ru: 'параллельные структуры и звучание', ckb: 'پێکهاتە هاوشێوەکان و ئاوازی ڕستە', kmr: 'avahiyên paralel û dengê hevokê', pl: 'struktury równoległe i brzmienie', ro: 'structuri paralele și sonoritate', sq: 'struktura paralele dhe tingulli' },
    focus: { de: 'Parallelitaet so einzusetzen, dass sie Orientierung und Nachdruck schafft, nicht bloss Schmuck', en: 'using parallelism so that it creates orientation and emphasis, not just decoration', fa: 'موازی‌سازی را طوری به کار ببری که جهت و تاکید بسازد، نه فقط تزئین زبانی', ar: 'استخدام التوازي بحيث يخلق توجيهًا وتأكيدًا، لا مجرد زخرفة لغوية', tr: 'paralelliği yalnızca süs değil, yönlendirme ve vurgu oluşturacak şekilde kullanmak', ru: 'использовать параллелизм так, чтобы он давал ориентацию и акцент, а не просто украшал текст', ckb: 'هاوشێوەیی بەکاربهێنیت بۆ ڕێنمایی و جەختکردنەوە، نە تەنها وەک ڕازاندنەوەی زمان', kmr: 'paraleliyê wisa bikar bînî ku rêberî û giranî çêbike, ne tenê xemilandina zimanî', pl: 'użyć równoległości tak, by dawała orientację i nacisk, a nie tylko ozdobę', ro: 'să folosești paralelismul ca să creeze orientare și accent, nu doar ornament', sq: 'ta përdorësh paralelizmin që të krijojë orientim dhe theks, jo vetëm zbukurim' },
    grammar: 'c2-complex-parallel-structures',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-argumentative-rhythmik-im-text-pruefen'
  },
  {
    slug: 'c2-interpunktion-und-leserfuehrung',
    topic: { de: 'Interpunktion und Leserfuehrung', en: 'punctuation and reader guidance', fa: 'نشانه‌گذاری و هدایت خواننده', ar: 'علامات الترقيم وتوجيه القارئ', tr: 'noktalama ve okur yönlendirmesi', ru: 'пунктуация и ведение читателя', ckb: 'نیشانەدان و ڕێنمایی خوێنەر', kmr: 'nîşankirin û rêberiya xwîner', pl: 'interpunkcja i prowadzenie czytelnika', ro: 'punctuația și ghidarea cititorului', sq: 'pikësimi dhe udhëzimi i lexuesit' },
    focus: { de: 'Zeichen so zu setzen, dass ein dichter Satz lesbar, rhythmisch und eindeutig bleibt', en: 'placing punctuation so that a dense sentence remains readable, rhythmic, and clear', fa: 'نشانه‌ها را طوری بگذاری که جمله فشرده همچنان خوانا، آهنگ‌دار و روشن بماند', ar: 'وضع العلامات بحيث تبقى الجملة الكثيفة مقروءة وإيقاعية وواضحة', tr: 'yoğun bir cümlenin okunur, ritmik ve açık kalması için noktalama yapmak', ru: 'ставить знаки так, чтобы плотное предложение оставалось читаемым, ритмичным и ясным', ckb: 'نیشانەکان بە شێوەیەک دابنێیت کە ڕستەی چڕ هێشتا خوێندراو، ئاوازدار و ڕوون بێت', kmr: 'nîşanan wisa danîn ku hevoka tije hîn jî xwendinbar, bi ritm û zelal bimîne', pl: 'stawiać znaki tak, aby gęste zdanie pozostało czytelne, rytmiczne i jasne', ro: 'să pui semnele astfel încât o frază densă să rămână lizibilă, ritmică și clară', sq: 't’i vendosësh shenjat që një fjali e dendur të mbetet e lexueshme, ritmike dhe e qartë' },
    grammar: 'c2-advanced-punctuation-and-rhythm',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-leserfuehrung-im-schreiben-sichern'
  },
  {
    slug: 'c2-lange-saetze-ohne-verlust-kontrollieren',
    topic: { de: 'lange Saetze ohne Verlust', en: 'long sentences without loss', fa: 'جمله‌های بلند بدون از دست دادن معنا', ar: 'الجمل الطويلة من دون فقدان المعنى', tr: 'anlam kaybetmeden uzun cümleler', ru: 'длинные предложения без потери смысла', ckb: 'ڕستەی درێژ بەبێ لەدەستدانی مانا', kmr: 'hevokên dirêj bê windakirina wateyê', pl: 'długie zdania bez utraty sensu', ro: 'fraze lungi fără pierderea sensului', sq: 'fjali të gjata pa humbur kuptimin' },
    focus: { de: 'lange Saetze so zu bauen, dass Hauptlinie, Einschub und Gewichtung jederzeit erkennbar bleiben', en: 'building long sentences so that the main line, insertion, and weighting remain visible throughout', fa: 'جمله بلند را طوری بسازی که خط اصلی، جمله معترضه و وزن هر بخش همیشه قابل تشخیص بماند', ar: 'بناء جمل طويلة تبقى فيها الفكرة الرئيسة والاعتراض والوزن واضحًا دائمًا', tr: 'uzun cümleleri ana çizgi, ara ek ve ağırlık her zaman görülecek şekilde kurmak', ru: 'строить длинные предложения так, чтобы главная линия, вставка и вес частей всегда были различимы', ckb: 'ڕستەی درێژ بە شێوەیەک دروست بکەیت کە هێڵی سەرەکی، ناوەڕاست و گرنگی هەر بەشێک هەمیشە دیار بێت', kmr: 'hevokên dirêj wisa ava bikî ku rêza sereke, navber û giraniya beşan her dem xuya bimîne', pl: 'budować długie zdania tak, by główna linia, wtrącenie i waga części były stale widoczne', ro: 'să construiești fraze lungi astfel încât linia principală, inserția și ponderea să rămână clare', sq: 'të ndërtosh fjali të gjata ku linja kryesore, ndërhyrja dhe pesha të mbeten të dallueshme' },
    grammar: 'c2-long-sentence-control',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-satzlast-im-schreiben-steuern'
  },
  {
    slug: 'c2-kompression-in-journalistischen-texten',
    topic: { de: 'Kompression in journalistischen Texten', en: 'compression in journalistic texts', fa: 'فشرده‌سازی در متن‌های روزنامه‌نگارانه', ar: 'التكثيف في النصوص الصحفية', tr: 'gazetecilik metinlerinde yoğunlaştırma', ru: 'сжатие в журналистских текстах', ckb: 'چڕکردنەوە لە دەقی ڕۆژنامەوانیدا', kmr: 'kompresyon di nivîsên rojnamevanî de', pl: 'kompresja w tekstach dziennikarskich', ro: 'compresia în texte jurnalistice', sq: 'ngjeshja në tekste gazetareske' },
    focus: { de: 'verdichtete Information zu entpacken und selbst knapp zu formulieren, ohne Differenzierung zu verlieren', en: 'unpacking compressed information and writing concisely yourself without losing nuance', fa: 'اطلاعات فشرده را باز کنی و خودت هم کوتاه بنویسی، بدون اینکه ظرافت معنا از بین برود', ar: 'تفكيك المعلومات المكثفة والكتابة بإيجاز من دون فقدان الفروق الدقيقة', tr: 'yoğun bilgiyi açmak ve ayrımı kaybetmeden kısa yazmak', ru: 'раскрывать сжатую информацию и самому писать кратко, не теряя нюансов', ckb: 'زانیاری چڕکراوە بکەیتەوە و خۆشت بە کورتی بنووسیت، بەبێ لەدەستدانی وردی مانا', kmr: 'agahiya tije vekî û bi kurtî binivîsî bêyî ku hûrgiliya wateyê winda bibe', pl: 'rozpakować zagęszczoną informację i samemu pisać zwięźle bez utraty niuansu', ro: 'să desfaci informația comprimată și să scrii concis fără să pierzi nuanța', sq: 'ta zbërthesh informacionin e ngjeshur dhe të shkruash shkurt pa humbur nuancën' },
    grammar: 'c2-journalistic-compression',
    targetType: 'writing-template',
    targetSlug: 'c2-expertentext-fuer-laien-verstaendlich-machen'
  },
  {
    slug: 'c2-literarische-satzstrukturen-einordnen',
    topic: { de: 'literarische Satzstrukturen', en: 'literary sentence structures', fa: 'ساختارهای جمله در متن ادبی', ar: 'البنى الجملية في النص الأدبي', tr: 'edebi metinlerde cümle yapıları', ru: 'синтаксис литературного текста', ckb: 'ڕستەسازی لە دەقی ئەدەبیدا', kmr: 'hevoksazî di nivîsa edebî de', pl: 'struktury zdań w tekście literackim', ro: 'structuri sintactice literare', sq: 'struktura fjalish në tekst letrar' },
    focus: { de: 'literarische Form nicht zu paraphrasieren, sondern als Bedeutungstraeger zu deuten', en: 'not merely paraphrasing literary form, but interpreting it as a carrier of meaning', fa: 'فرم ادبی را فقط بازگویی نکنی، بلکه آن را حامل معنا تفسیر کنی', ar: 'ألا تكتفي بإعادة صياغة الشكل الأدبي، بل تفسّره كحامل للمعنى', tr: 'edebi biçimi sadece başka sözlerle anlatmak değil, anlam taşıyıcısı olarak yorumlamak', ru: 'не просто пересказывать литературную форму, а толковать ее как носитель смысла', ckb: 'فۆرمی ئەدەبی تەنها دووبارە نەگێڕیتەوە، بەڵکو وەک هەڵگری مانا شرۆڤەی بکەیت', kmr: 'forma edebî ne tenê vegotin, lê wek hilgirê wateyê şîrove bikî', pl: 'nie tylko parafrazować formę literacką, lecz interpretować ją jako nośnik sensu', ro: 'să nu parafrazezi forma literară, ci să o interpretezi ca purtătoare de sens', sq: 'të mos e perifrazosh vetëm formën letrare, por ta interpretosh si bartëse kuptimi' },
    grammar: 'c2-literary-sentence-structures',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-literarische-position-diskutieren'
  },
  {
    slug: 'c2-stil-register-und-rhythmus-wiederholen',
    topic: { de: 'Stil, Register und Rhythmus', en: 'style, register, and rhythm', fa: 'سبک، register و آهنگ جمله', ar: 'الأسلوب والسجل والإيقاع', tr: 'üslup, register ve ritim', ru: 'стиль, регистр и ритм', ckb: 'ستایل، ڕەجستەر و ڕیتم', kmr: 'şêwaz, register û ritm', pl: 'styl, rejestr i rytm', ro: 'stil, registru și ritm', sq: 'stil, regjistër dhe ritëm' },
    focus: { de: 'eine eigene Kontrollroutine fuer Stil, Register und Rhythmus vor Abgabe oder Vortrag anzuwenden', en: 'using your own control routine for style, register, and rhythm before submitting or speaking', fa: 'قبل از تحویل متن یا ارائه، یک روال شخصی برای کنترل سبک، register و آهنگ جمله به کار ببری', ar: 'تطبيق روتين شخصي لمراجعة الأسلوب والسجل والإيقاع قبل التسليم أو العرض', tr: 'teslim ya da konuşma öncesi üslup, register ve ritim için kendi kontrol rutinini kullanmak', ru: 'применять собственную процедуру контроля стиля, регистра и ритма перед сдачей текста или выступлением', ckb: 'پێش پێشکەشکردن یان قسەکردن، ڕووتینێکی تایبەتی بۆ کۆنترۆڵی ستایل، ڕەجستەر و ڕیتم بەکاربهێنیت', kmr: 'berî radestkirin an axaftinê rêbazeke xwe ji bo kontrola şêwaz, register û ritmê bikar bînî', pl: 'stosować własną rutynę kontroli stylu, rejestru i rytmu przed oddaniem tekstu lub wystąpieniem', ro: 'să folosești o rutină proprie de control pentru stil, registru și ritm înainte de predare sau prezentare', sq: 'të përdorësh një rutinë personale kontrolli për stilin, regjistrin dhe ritmin para dorëzimit ose prezantimit' },
    grammar: 'c2-c2-register-review',
    targetType: 'writing-template',
    targetSlug: 'c2-selbstlektorat-kommentar-fuer-eigenen-text'
  }
];

function translationArray(map) {
  return langs.map(language => ({ language, text: map[language] }));
}

function orientInstruction(item) {
  return {
    de: `Lies den Lektionstext zu ${item.topic.de}. Markiere zwei Stellen: eine, an der die Form die Wirkung staerkt, und eine, an der sie die Aussage ueberladen koennte.`,
    en: `Read the lesson text on ${item.topic.en}. Mark two places: one where the form strengthens the effect, and one where it could overload the message.`,
    fa: `متن درس درباره ${item.topic.fa} را بخوان. دو جا را مشخص کن: جایی که فرم اثر متن را قوی‌تر می‌کند، و جایی که ممکن است پیام را بیش از حد سنگین کند.`,
    ar: `اقرأ نص الدرس عن ${item.topic.ar}. حدّد موضعين: موضعًا تقوّي فيه الصيغة الأثر، وموضعًا قد تُثقل فيه الرسالة.`,
    tr: `${item.topic.tr} hakkındaki ders metnini oku. İki yeri işaretle: biçimin etkiyi güçlendirdiği bir yer ve mesajı fazla ağırlaştırabileceği bir yer.`,
    ru: `Прочитай урок о теме: ${item.topic.ru}. Отметь два места: где форма усиливает эффект и где она может перегрузить сообщение.`,
    ckb: `دەقی وانەکە دەربارەی ${item.topic.ckb} بخوێنەوە. دوو شوێن دیاری بکە: شوێنێک کە فۆرم کاریگەری بەهێزتر دەکات، و شوێنێک کە ڕەنگە پەیامەکە زۆر قورس بکات.`,
    kmr: `Nivîsa dersê derbarê ${item.topic.kmr} bixwîne. Du cih nîşan bike: cihê ku form bandorê bihêztir dike, û cihê ku dikare peyamê pir giran bike.`,
    pl: `Przeczytaj tekst lekcji o: ${item.topic.pl}. Zaznacz dwa miejsca: jedno, w którym forma wzmacnia efekt, i jedno, w którym może przeciążyć przekaz.`,
    ro: `Citește textul lecției despre ${item.topic.ro}. Marchează două locuri: unul în care forma întărește efectul și unul în care poate încărca prea mult mesajul.`,
    sq: `Lexo tekstin e mësimit për ${item.topic.sq}. Shëno dy vende: një ku forma e forcon efektin dhe një ku mund ta rëndojë shumë mesazhin.`
  };
}

function languageInstruction(item) {
  return {
    de: `Oeffne den verlinkten Grammatikpunkt und sammle drei Entscheidungen, die dir helfen, ${item.focus.de}.`,
    en: `Open the linked grammar point and collect three decisions that help you ${item.focus.en}.`,
    fa: `بخش گرامر لینک‌شده را باز کن و سه تصمیم زبانی پیدا کن که کمک می‌کنند ${item.focus.fa}.`,
    ar: `افتح نقطة القواعد المرتبطة واجمع ثلاثة قرارات لغوية تساعدك على ${item.focus.ar}.`,
    tr: `Bağlantılı dilbilgisi bölümünü aç ve sana şunda yardımcı olan üç dilsel karar bul: ${item.focus.tr}.`,
    ru: `Открой связанный раздел грамматики и найди три языковых решения, которые помогают ${item.focus.ru}.`,
    ckb: `خاڵی ڕێزمانی بەستەرکراو بکەرەوە و سێ بڕیاری زمانی بدۆزەوە کە یارمەتیت دەدەن ${item.focus.ckb}.`,
    kmr: `Xala rêzimanê ya girêdayî veke û sê biryarên zimanî bibîne ku alîkarin ${item.focus.kmr}.`,
    pl: `Otwórz podlinkowany punkt gramatyczny i zbierz trzy decyzje językowe, które pomagają ${item.focus.pl}.`,
    ro: `Deschide punctul de gramatică legat și adună trei decizii lingvistice care te ajută să ${item.focus.ro}.`,
    sq: `Hap pikën e lidhur të gramatikës dhe mblidh tri vendime gjuhësore që të ndihmojnë ${item.focus.sq}.`
  };
}

function targetInstruction(item) {
  return {
    de: `Bearbeite die verlinkte Ressource. Pruefe danach die Wirkung: Stuetzt deine sprachliche Entscheidung wirklich dieses Ziel - ${item.focus.de}?`,
    en: `Work through the linked resource. Then check the effect: does your language choice really support this goal - ${item.focus.en}?`,
    fa: `منبع لینک‌شده را انجام بده. بعد اثر را بررسی کن: آیا انتخاب زبانی تو واقعاً به این هدف کمک می‌کند که ${item.focus.fa}؟`,
    ar: `اعمل على المورد المرتبط. ثم قيّم الأثر: هل يدعم اختيارك اللغوي هذا الهدف فعلًا، أي ${item.focus.ar}؟`,
    tr: `Bağlantılı kaynağı çalış. Sonra etkiyi kontrol et: Dil seçimin gerçekten şu hedefi destekliyor mu - ${item.focus.tr}?`,
    ru: `Проработай связанный ресурс. Затем проверь эффект: действительно ли твой языковой выбор поддерживает эту цель — ${item.focus.ru}?`,
    ckb: `سەرچاوەی بەستەرکراو کاربکە. پاشان کاریگەرییەکە بپشکنە: ئایا هەڵبژاردنی زمانیت بەڕاستی یارمەتی ئەم ئامانجە دەدات کە ${item.focus.ckb}؟`,
    kmr: `Çavkaniya girêdayî bixebitîne. Paşê bandorê binirxîne: gelo hilbijartina zimanî ya te rastî vê armancê piştgirî dike - ${item.focus.kmr}?`,
    pl: `Przerób podlinkowany materiał. Potem sprawdź efekt: czy twój wybór językowy naprawdę wspiera ten cel - ${item.focus.pl}?`,
    ro: `Lucrează resursa legată. Apoi verifică efectul: alegerea ta lingvistică susține cu adevărat acest scop - ${item.focus.ro}?`,
    sq: `Puno me burimin e lidhur. Pastaj kontrollo efektin: a e mbështet vërtet zgjedhja jote gjuhësore këtë synim - ${item.focus.sq}?`
  };
}

function transferInstruction() {
  return {
    de: 'Nimm einen kurzen eigenen Absatz und schreibe ihn in zwei Versionen um: einmal dichter und formeller, einmal klarer und zugänglicher. Notiere, welche Version fuer welchen Kontext passt.',
    en: 'Take a short paragraph of your own and rewrite it in two versions: once denser and more formal, once clearer and more accessible. Note which version fits which context.',
    fa: 'یک بند کوتاه از متن خودت بردار و آن را در دو نسخه بازنویسی کن: یک بار فشرده‌تر و رسمی‌تر، یک بار روشن‌تر و قابل‌فهم‌تر. بنویس هر نسخه برای کدام موقعیت مناسب است.',
    ar: 'خذ فقرة قصيرة من نصك وأعد كتابتها بنسختين: نسخة أكثر كثافة ورسمية، ونسخة أوضح وأسهل وصولًا. دوّن أي نسخة تناسب أي سياق.',
    tr: 'Kendi metninden kısa bir paragraf al ve iki biçimde yeniden yaz: bir kez daha yoğun ve resmi, bir kez daha açık ve erişilebilir. Hangi sürümün hangi bağlama uygun olduğunu not et.',
    ru: 'Возьми короткий собственный абзац и перепиши его в двух версиях: более плотной и официальной, затем более ясной и доступной. Запиши, какая версия подходит для какого контекста.',
    ckb: 'پەرەگرافێکی کورتی خۆت هەڵبژێرە و بە دوو شێوە دووبارەی بنووسە: جارێک چڕتر و فەرمیتر، جارێک ڕوونتر و ئاسانتر. تۆمار بکە هەر وەشانە بۆ کام بارودۆخ گونجاوە.',
    kmr: 'Paragrafek kurt a xwe hilbijêre û bi du awayan ji nû ve binivîse: carekê tijetir û fermîtir, carekê zelaltir û gihîştintir. Binivîse her guhertoyek ji bo kîjan kontekstê guncan e.',
    pl: 'Weź krótki własny akapit i przeredaguj go w dwóch wersjach: raz bardziej gęsto i formalnie, raz jaśniej i przystępniej. Zapisz, która wersja pasuje do którego kontekstu.',
    ro: 'Ia un paragraf scurt propriu și rescrie-l în două versiuni: una mai densă și mai formală, una mai clară și mai accesibilă. Notează ce versiune se potrivește fiecărui context.',
    sq: 'Merr një paragraf të shkurtër tëndin dhe rishkruaje në dy versione: një më të ngjeshur dhe më formal, një më të qartë dhe më të kuptueshëm. Shëno cili version i përshtatet cilit kontekst.'
  };
}

function reviewInstruction() {
  return {
    de: 'Formuliere eine Regel fuer dich: Wann darf ein Satz dichter, rhythmischer oder kuenstlicher werden, und wann muss er einfacher werden?',
    en: 'Formulate a rule for yourself: when may a sentence become denser, more rhythmic, or more artful, and when must it become simpler?',
    fa: 'برای خودت یک قاعده بنویس: چه زمانی جمله می‌تواند فشرده‌تر، آهنگ‌دارتر یا هنرمندانه‌تر شود، و چه زمانی باید ساده‌تر شود؟',
    ar: 'صغ قاعدة لنفسك: متى يمكن للجملة أن تصبح أكثر كثافة أو إيقاعًا أو فنية، ومتى يجب أن تصبح أبسط؟',
    tr: 'Kendin için bir kural yaz: Bir cümle ne zaman daha yoğun, daha ritmik ya da daha sanatsal olabilir; ne zaman daha basit olmalıdır?',
    ru: 'Сформулируй для себя правило: когда предложение может стать плотнее, ритмичнее или художественнее, а когда должно стать проще?',
    ckb: 'بۆ خۆت یاسایەک بنووسە: کەی ڕستە دەتوانێت چڕتر، ڕیتمدارتر یان هونەریانەتر بێت، و کەی پێویستە سادەتر بێت؟',
    kmr: 'Ji bo xwe qaîdeyek binivîse: kengî hevok dikare tijetir, bi ritmtir an hunerîrtir bibe, û kengî divê hêsantir bibe?',
    pl: 'Sformułuj dla siebie zasadę: kiedy zdanie może stać się gęstsze, bardziej rytmiczne lub artystyczne, a kiedy musi być prostsze?',
    ro: 'Formulează o regulă pentru tine: când poate o frază să devină mai densă, mai ritmică sau mai artistică și când trebuie să devină mai simplă?',
    sq: 'Formulo një rregull për veten: kur mund të bëhet fjalia më e ngjeshur, më ritmike ose më artistike, dhe kur duhet të bëhet më e thjeshtë?'
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
    makeActivity(item.targetType === 'roleplay' ? 'roleplay' : item.targetType === 'writing-template' ? 'write' : 'exam-prep', 'target', targetInstruction(item), item.targetType, item.targetSlug, 30, 10),
    makeActivity('practice', 'transfer', transferInstruction(), 'none', null, 40, 9),
    makeActivity('review', 'review', reviewInstruction(), 'none', null, 50, 5)
  ];
}

fs.writeFileSync(file, `${JSON.stringify(data, null, 2)}\n`, 'utf8');
console.log(`Updated ${items.length} C2 Module 2 lessons with ${items.length * 5} activity blocks.`);

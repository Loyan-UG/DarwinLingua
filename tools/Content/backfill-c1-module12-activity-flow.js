const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c1-souveraen-in-studium-beruf-und-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'Kompetenzlage ordnen',
    en: 'Organize your competence profile',
    fa: 'وضعیت مهارت‌هایت را مرتب کن',
    ar: 'رتّب وضع كفاءاتك',
    tr: 'Yetkinlik durumunu düzenle',
    ru: 'Упорядочь профиль навыков',
    ckb: 'دۆخی تواناکانت ڕێکبخە',
    kmr: 'Rewşa jêhatinên xwe rêz bike',
    pl: 'Uporządkuj profil kompetencji',
    ro: 'Ordonează profilul competențelor',
    sq: 'Rendit profilin e aftësive'
  },
  connect: {
    de: 'Ressourcen verknuepfen',
    en: 'Connect resources',
    fa: 'منابع را به هم وصل کن',
    ar: 'اربط الموارد ببعضها',
    tr: 'Kaynakları birbirine bağla',
    ru: 'Свяжи ресурсы между собой',
    ckb: 'سەرچاوەکان پێکەوە ببەستە',
    kmr: 'Çavkaniyan bi hev ve girêde',
    pl: 'Połącz zasoby',
    ro: 'Leagă resursele',
    sq: 'Lidh burimet'
  },
  produce: {
    de: 'Portfolio-Leistung erstellen',
    en: 'Create a portfolio performance',
    fa: 'یک نمونه کار برای پرونده یادگیری بساز',
    ar: 'أنشئ أداءً لملف التعلّم',
    tr: 'Portfolyo performansı oluştur',
    ru: 'Создай работу для портфолио',
    ckb: 'ئەنجامدانێک بۆ پۆرتفۆلیۆ دروست بکە',
    kmr: 'Performanseke portfolyo çêbike',
    pl: 'Utwórz pracę do portfolio',
    ro: 'Creează o prestație de portofoliu',
    sq: 'Krijo një performancë për portofol'
  },
  refine: {
    de: 'Stil und Genauigkeit schaerfen',
    en: 'Sharpen style and precision',
    fa: 'سبک و دقت را بهتر کن',
    ar: 'حسّن الأسلوب والدقة',
    tr: 'Üslubu ve kesinliği keskinleştir',
    ru: 'Отточи стиль и точность',
    ckb: 'ستایل و وردی باشتر بکە',
    kmr: 'Şêwaz û rastiyê tûj bike',
    pl: 'Dopracuj styl i precyzję',
    ro: 'Rafinează stilul și precizia',
    sq: 'Mpreh stilin dhe saktësinë'
  },
  review: {
    de: 'Naechsten Schritt festlegen',
    en: 'Set the next step',
    fa: 'قدم بعدی را مشخص کن',
    ar: 'حدّد الخطوة التالية',
    tr: 'Sonraki adımı belirle',
    ru: 'Определи следующий шаг',
    ckb: 'هەنگاوی داهاتوو دیاری بکە',
    kmr: 'Gava din diyar bike',
    pl: 'Ustal następny krok',
    ro: 'Stabilește pasul următor',
    sq: 'Përcakto hapin tjetër'
  }
};

const items = [
  {
    slug: 'c1-integriertes-projekt-akademischer-diskurs',
    topic: { de: 'akademischer Diskurs', en: 'academic discourse', fa: 'گفتمان دانشگاهی', ar: 'الخطاب الأكاديمي', tr: 'akademik söylem', ru: 'академический дискурс', ckb: 'گوتاری زانکۆیی', kmr: 'gotara akademîk', pl: 'dyskurs akademicki', ro: 'discurs academic', sq: 'diskurs akademik' },
    focus: { de: 'These, Quelle, Einwand und eigene Bewertung in einem kontrollierten Beitrag verbinden', en: 'linking thesis, source, objection, and your own evaluation in a controlled contribution', fa: 'تز، منبع، ایراد و ارزیابی خودت را در یک متن یا گفتار کنترل‌شده به هم وصل کنی', ar: 'ربط الأطروحة والمصدر والاعتراض وتقييمك الشخصي في مساهمة مضبوطة', tr: 'tez, kaynak, itiraz ve kendi değerlendirmeni kontrollü bir katkıda birleştirmeyi', ru: 'связать тезис, источник, возражение и собственную оценку в контролируемом выступлении', ckb: 'تێز، سەرچاوە، ناڕەزایی و هەڵسەنگاندنی خۆت لە بەشدارییەکی کۆنترۆڵکراودا پێکەوە ببەستیت', kmr: 'tez, çavkanî, nerazîbûn û nirxandina xwe di beşdariyeke kontrolkirî de girêdan', pl: 'połączenie tezy, źródła, zastrzeżenia i własnej oceny w kontrolowanej wypowiedzi', ro: 'legarea tezei, sursei, obiecției și evaluării proprii într-o contribuție controlată', sq: 'lidhjen e tezës, burimit, kundërshtimit dhe vlerësimit tënd në një kontribut të kontrolluar' },
    grammar: ['c1-academic-argument-grammar', 'akademische Argumentation'],
    target: ['roleplay', 'c1-eine-akademische-praesentation-diskutieren']
  },
  {
    slug: 'c1-integriertes-projekt-berufliche-strategie',
    topic: { de: 'berufliche Strategie', en: 'professional strategy', fa: 'راهبرد حرفه‌ای', ar: 'استراتيجية مهنية', tr: 'mesleki strateji', ru: 'профессиональная стратегия', ckb: 'ستراتیژی پیشەیی', kmr: 'stratejiya pîşeyî', pl: 'strategia zawodowa', ro: 'strategie profesională', sq: 'strategji profesionale' },
    focus: { de: 'Ziel, Risiko, Prioritaet und Gegenposition sachlich verhandeln', en: 'negotiating goal, risk, priority, and opposing view factually', fa: 'هدف، ریسک، اولویت و نگاه مخالف را عینی و حرفه‌ای مذاکره کنی', ar: 'التفاوض بموضوعية حول الهدف والخطر والأولوية والرأي المقابل', tr: 'hedef, risk, öncelik ve karşı görüşü nesnel müzakere etmeyi', ru: 'предметно обсуждать цель, риск, приоритет и противоположную позицию', ckb: 'ئامانج، مەترسی، پێشەکی و دیدی بەرامبەر بە بابەتی وتووێژ بکەیت', kmr: 'armanc, rîsk, pêşî û nêrîna dijber bi awayekî babetî gotûbêj kirin', pl: 'rzeczowe negocjowanie celu, ryzyka, priorytetu i stanowiska przeciwnego', ro: 'negocierea factuală a scopului, riscului, priorității și poziției opuse', sq: 'negocimin faktik të qëllimit, rrezikut, përparësisë dhe këndvështrimit kundër' },
    grammar: ['c1-register-shifting', 'beruflicher Registerwechsel'],
    target: ['roleplay', 'c1-berufssprache-c1-strategiegespraech-fuehren']
  },
  {
    slug: 'c1-integriertes-projekt-gesellschaftliche-debatte',
    topic: { de: 'gesellschaftliche Debatte', en: 'social debate', fa: 'بحث اجتماعی', ar: 'نقاش اجتماعي', tr: 'toplumsal tartışma', ru: 'общественная дискуссия', ckb: 'مشتومڕی کۆمەڵایەتی', kmr: 'nîqaşa civakî', pl: 'debata społeczna', ro: 'dezbatere socială', sq: 'debat shoqëror' },
    focus: { de: 'mehrere Perspektiven fair darstellen und trotzdem eine erkennbare eigene Linie halten', en: 'presenting several perspectives fairly while still keeping a recognizable line of your own', fa: 'چند دیدگاه را منصفانه نشان بدهی و همزمان خط فکری خودت را هم قابل تشخیص نگه داری', ar: 'عرض عدة وجهات نظر بإنصاف مع الحفاظ على خطك الفكري بوضوح', tr: 'birkaç bakış açısını adil sunup yine de kendi çizgini görünür tutmayı', ru: 'справедливо представить несколько перспектив и при этом сохранить собственную линию', ckb: 'چەند دیدگا بە دادپەروەری پیشان بدەیت و هێڵی بیرکردنەوەی خۆتیش دیار بمێنێتەوە', kmr: 'çend perspektîfan bi dadperwerî nîşan dan û her weha xeta xwe xuya hiştin', pl: 'uczciwe przedstawienie kilku perspektyw przy zachowaniu własnej linii', ro: 'prezentarea corectă a mai multor perspective păstrând totuși o linie proprie recognoscibilă', sq: 'paraqitjen e drejtë të disa këndvështrimeve duke ruajtur një vijë tënden të dallueshme' },
    grammar: ['c1-c1-discussion-grammar', 'Diskussionsgrammatik'],
    target: ['roleplay', 'c1-eine-diskussion-trotz-einwaenden-strukturieren']
  },
  {
    slug: 'c1-stilistische-feinabstimmung',
    topic: { de: 'stilistische Feinabstimmung', en: 'stylistic fine-tuning', fa: 'تنظیم دقیق سبک', ar: 'الضبط الدقيق للأسلوب', tr: 'üslup ince ayarı', ru: 'тонкая настройка стиля', ckb: 'ڕێکخستنی وردی ستایل', kmr: 'lihevkirina hûr a şêwazê', pl: 'precyzyjne dostrojenie stylu', ro: 'ajustare stilistică fină', sq: 'rregullim i hollë stilistik' },
    focus: { de: 'praeziser, natuerlicher und adressatengerechter formulieren, ohne kuenstlich zu wirken', en: 'formulating more precisely, naturally, and appropriately for the addressee without sounding artificial', fa: 'دقیق‌تر، طبیعی‌تر و متناسب با مخاطب بنویسی یا بگویی، بدون اینکه مصنوعی به نظر برسد', ar: 'الصياغة بدقة وطبيعية وبما يناسب المخاطب من دون أن تبدو مصطنعة', tr: 'yapay görünmeden daha kesin, doğal ve muhataba uygun formüle etmeyi', ru: 'формулировать точнее, естественнее и адресатно, не звуча искусственно', ckb: 'وردتر، سروشتیتر و گونجاوتر بۆ وەرگر دەرببڕیت، بەبێ دەستکرد دەرکەوتن', kmr: 'rasttir, xwezayîtir û li gorî muxateb gotin bêyî ku çêkirî xuya bike', pl: 'formułowanie precyzyjniej, naturalniej i bardziej do adresata bez sztuczności', ro: 'formularea mai precisă, naturală și potrivită destinatarului fără artificialitate', sq: 'formulim më i saktë, më natyrshëm dhe sipas adresatit pa tingëlluar artificial' },
    grammar: ['c1-c1-register-review', 'Register-Review'],
    target: ['writing-template', 'c1-registerwechsel-in-email-kette-steuern']
  },
  {
    slug: 'c1-grammatiksysteme-vernetzt-wiederholen',
    topic: { de: 'Grammatiksysteme vernetzt wiederholen', en: 'reviewing grammar systems in connection', fa: 'مرور شبکه‌ای سیستم‌های گرامری', ar: 'مراجعة الأنظمة النحوية بشكل مترابط', tr: 'gramer sistemlerini bağlantılı tekrar etme', ru: 'связное повторение грамматических систем', ckb: 'دووبارەکردنەوەی پەیوەستکراوی سیستەمە ڕێزمانییەکان', kmr: 'dubarekirina girêdayî ya pergalên rêzimanê', pl: 'powtórka systemów gramatycznych w powiązaniu', ro: 'recapitularea conectată a sistemelor gramaticale', sq: 'përsëritje e lidhur e sistemeve gramatikore' },
    focus: { de: 'nicht einzelne Regeln sammeln, sondern Funktionen vergleichen: Distanz, Verdichtung, Bezug und Bewertung', en: 'comparing functions rather than collecting isolated rules: distance, compression, reference, and evaluation', fa: 'به جای جمع‌کردن قاعده‌های جدا، کارکردها را مقایسه کنی: فاصله، فشرده‌سازی، ارجاع و ارزیابی', ar: 'مقارنة الوظائف بدل جمع قواعد منفصلة: المسافة، التكثيف، الإحالة والتقييم', tr: 'tek tek kurallar toplamak yerine işlevleri karşılaştırmayı: mesafe, yoğunlaştırma, gönderim ve değerlendirme', ru: 'сравнивать функции, а не собирать отдельные правила: дистанция, сжатие, ссылка и оценка', ckb: 'لە جیاتی کۆکردنەوەی یاسای جیاواز، کارەکان بەراورد بکەیت: دووری، چڕکردنەوە، ئاماژە و هەڵسەنگاندن', kmr: 'li şûna komkirina qaîdeyên cuda, fonksiyonan berawird kirin: dûrî, tîrbûn, referans û nirxandin', pl: 'porównywanie funkcji zamiast zbierania oddzielnych reguł: dystans, kondensacja, odniesienie i ocena', ro: 'compararea funcțiilor, nu adunarea regulilor izolate: distanță, densitate, referință și evaluare', sq: 'krahasimin e funksioneve, jo mbledhjen e rregullave të ndara: distancë, ngjeshje, referim dhe vlerësim' },
    grammar: ['c1-c1-grammar-review-map', 'C1-Review-Map'],
    target: ['grammar-topic', 'c1-c1-academic-grammar-review']
  },
  {
    slug: 'c1-fossilisierte-fehler-gezielt-abbauen',
    topic: { de: 'fossilisierte Fehler gezielt abbauen', en: 'reducing fossilized errors deliberately', fa: 'کاهش هدفمند خطاهای جاافتاده', ar: 'تقليل الأخطاء المتحجرة بوعي', tr: 'yerleşmiş hataları bilinçli azaltma', ru: 'целенаправленное уменьшение закрепившихся ошибок', ckb: 'کەمکردنەوەی ئامانجدارانی هەڵە جێگیرەکان', kmr: 'kêmkirina bi armanc a şaşitiyên cihgirtî', pl: 'celowe ograniczanie utrwalonych błędów', ro: 'reducerea intenționată a greșelilor fosilizate', sq: 'ulja e qëllimshme e gabimeve të ngulitura' },
    focus: { de: 'ein kleines Fehlermuster erkennen, isoliert trainieren und dann in echter Sprache kontrollieren', en: 'recognizing one small error pattern, training it in isolation, and then controlling it in real language', fa: 'یک الگوی خطای کوچک را تشخیص بدهی، جداگانه تمرین کنی و بعد در زبان واقعی کنترلش کنی', ar: 'التعرّف على نمط خطأ صغير وتدريبه منفصلًا ثم ضبطه في لغة حقيقية', tr: 'küçük bir hata kalıbını tanıyıp ayrı çalışmayı ve sonra gerçek dilde kontrol etmeyi', ru: 'распознать один небольшой шаблон ошибки, отдельно потренировать его и затем контролировать в реальной речи', ckb: 'یەک شێوازی هەڵەی بچووک بناسیتەوە، جیا ڕاهێنان بکەیت و پاشان لە زمانی ڕاستەقینەدا کۆنترۆڵی بکەیت', kmr: 'şêwazeke biçûk a şaşitiyê nas kirin, cuda perwerde kirin û paşê di zimanê rastîn de kontrol kirin', pl: 'rozpoznanie małego wzorca błędu, osobne przećwiczenie go i kontrola w prawdziwym języku', ro: 'recunoașterea unui mic tipar de eroare, exersarea izolată și apoi controlul în limbaj real', sq: 'dallimin e një modeli të vogël gabimi, ushtrimin veçmas dhe kontrollin në gjuhë reale' },
    grammar: ['c1-c1-common-mistakes', 'typische C1-Fehler'],
    target: ['exam-prep-unit', 'c1-pruefungsanforderungen-einordnen']
  },
  {
    slug: 'c1-repertoire-erweitern-ohne-kuenstlich-zu-klingen',
    topic: { de: 'Repertoire erweitern ohne kuenstlich zu klingen', en: 'expanding your repertoire without sounding artificial', fa: 'گسترش دامنه بیان بدون مصنوعی شدن', ar: 'توسيع الرصيد التعبيري من دون تصنّع', tr: 'yapay duyulmadan ifade repertuarını genişletme', ru: 'расширение репертуара без искусственного звучания', ckb: 'فراوانکردنی دەربڕین بەبێ دەستکرد دەرکەوتن', kmr: 'firehkirina repertuwara xwe bêyî ku çêkirî xuya bike', pl: 'poszerzanie repertuaru bez sztuczności', ro: 'extinderea repertoriului fără a suna artificial', sq: 'zgjerimi i repertorit pa tingëlluar artificial' },
    focus: { de: 'neue Wendungen nur dann einsetzen, wenn Bedeutung, Register und eigener Ton zusammenpassen', en: 'using new expressions only when meaning, register, and your own voice fit together', fa: 'عبارت‌های جدید را فقط وقتی به کار ببری که معنا، سطح رسمی‌بودن و لحن خودت با هم جور باشند', ar: 'استخدام تعابير جديدة فقط عندما يتوافق المعنى والسجل ونبرتك الشخصية', tr: 'yeni ifadeleri yalnızca anlam, register ve kendi tonun birlikte uyduğunda kullanmayı', ru: 'использовать новые обороты только тогда, когда смысл, регистр и собственный тон совпадают', ckb: 'دەربڕینی نوێ تەنها کاتێک بەکاربهێنیت کە واتا، تۆن و دەنگی خۆت پێکەوە بگونجێن', kmr: 'derbirînên nû tenê dema ku wate, register û dengê xwe bi hev re biguncin bikaranîn', pl: 'używanie nowych zwrotów tylko wtedy, gdy znaczenie, rejestr i własny ton pasują do siebie', ro: 'folosirea expresiilor noi doar când sensul, registrul și vocea ta se potrivesc', sq: 'përdorimin e shprehjeve të reja vetëm kur kuptimi, regjistri dhe zëri yt përputhen' },
    grammar: ['c1-c1-register-review', 'Register-Review'],
    target: ['course-lesson', 'c2-repertoire-ohne-kuenstlichkeit-erweitern']
  },
  {
    slug: 'c1-c1-abschluss-selbstcheck',
    topic: { de: 'C1 Abschluss-Selbstcheck', en: 'C1 final self-check', fa: 'خودارزیابی پایانی C1', ar: 'التقييم الذاتي النهائي لمستوى C1', tr: 'C1 final öz kontrolü', ru: 'итоговая самооценка C1', ckb: 'خۆهەڵسەنگاندنی کۆتایی C1', kmr: 'xwenirxandina dawî ya C1', pl: 'końcowa samoocena C1', ro: 'autoevaluare finală C1', sq: 'vetëkontroll përfundimtar C1' },
    focus: { de: 'Staerken, wiederkehrende Risiken und naechste Trainingsprioritaet ehrlich festhalten', en: 'recording strengths, recurring risks, and the next training priority honestly', fa: 'نقطه‌قوت‌ها، ریسک‌های تکرارشونده و اولویت تمرین بعدی را صادقانه ثبت کنی', ar: 'تسجيل نقاط القوة والمخاطر المتكررة وأولوية التدريب التالية بصدق', tr: 'güçlü yönleri, tekrar eden riskleri ve sonraki çalışma önceliğini dürüstçe kaydetmeyi', ru: 'честно зафиксировать сильные стороны, повторяющиеся риски и следующий приоритет тренировки', ckb: 'خاڵە بەهێزەکان، مەترسییە دووبارەبووەکان و پێشەکی ڕاهێنانی داهاتوو بە ڕاستی تۆمار بکەیت', kmr: 'hêz, rîskên dubare û pêşîya perwerdeya din bi rastî tomar kirin', pl: 'uczciwe zapisanie mocnych stron, powracających ryzyk i kolejnego priorytetu treningu', ro: 'notarea sinceră a punctelor forte, riscurilor recurente și următoarei priorități de antrenament', sq: 'regjistrimin sinqerisht të pikave të forta, rreziqeve të përsëritura dhe përparësisë së radhës' },
    grammar: ['c1-c1-grammar-review-map', 'C1-Review-Map'],
    target: ['exam-prep-unit', 'c1-pruefungsanforderungen-einordnen']
  },
  {
    slug: 'c1-c2-bruecke-dichte-ironie-und-stil',
    topic: { de: 'Dichte, Ironie und Stil als C2-Bruecke', en: 'density, irony, and style as a bridge to C2', fa: 'فشردگی، کنایه و سبک به عنوان پل به C2', ar: 'الكثافة والسخرية والأسلوب كجسر إلى C2', tr: 'C2’ye köprü olarak yoğunluk, ironi ve üslup', ru: 'плотность, ирония и стиль как мост к C2', ckb: 'چڕی، گاڵتەئامێزی و ستایل وەک پرد بۆ C2', kmr: 'tîrbûn, ironî û şêwaz wek pirek bo C2', pl: 'gęstość, ironia i styl jako most do C2', ro: 'densitate, ironie și stil ca punte spre C2', sq: 'dendësia, ironia dhe stili si urë drejt C2' },
    focus: { de: 'dichte oder indirekte Texte nicht nur verstehen, sondern ihre Wirkung beschreiben', en: 'not only understanding dense or indirect texts, but describing their effect', fa: 'متن‌های فشرده یا غیرمستقیم را فقط نفهمی، بلکه اثرشان را هم توصیف کنی', ar: 'لا تكتفي بفهم النصوص الكثيفة أو غير المباشرة، بل صف أثرها أيضًا', tr: 'yoğun veya dolaylı metinleri sadece anlamayı değil, etkilerini de betimlemeyi', ru: 'не только понимать плотные или косвенные тексты, но и описывать их эффект', ckb: 'دەقە چڕ یان ناڕاستەوخۆکان تەنها تێنەگەیت، بەڵکو کاریگەرییانیش باس بکەیت', kmr: 'nivîsên tîr an ne-rasterast ne tenê fêm kirin, lê bandora wan jî vegotin', pl: 'nie tylko rozumienie gęstych lub pośrednich tekstów, lecz także opisywanie ich efektu', ro: 'nu doar înțelegerea textelor dense sau indirecte, ci și descrierea efectului lor', sq: 'jo vetëm të kuptosh tekste të dendura ose të tërthorta, por edhe të përshkruash efektin e tyre' },
    grammar: ['c1-c1-register-review', 'Register-Review'],
    target: ['course-lesson', 'c2-feine-bedeutungsunterschiede-erkennen']
  },
  {
    slug: 'c1-abschluss-und-c2-vorbereitung',
    topic: { de: 'C1 Abschluss und C2-Vorbereitung', en: 'C1 completion and C2 preparation', fa: 'پایان C1 و آمادگی برای C2', ar: 'إنهاء C1 والاستعداد لـ C2', tr: 'C1 kapanışı ve C2 hazırlığı', ru: 'завершение C1 и подготовка к C2', ckb: 'کۆتایی C1 و ئامادەکاری بۆ C2', kmr: 'dawiya C1 û amadekariya C2', pl: 'zakończenie C1 i przygotowanie do C2', ro: 'finalizarea C1 și pregătirea pentru C2', sq: 'përfundimi i C1 dhe përgatitja për C2' },
    focus: { de: 'aus C1-Routine einen bewussten C2-Lernplan machen', en: 'turning C1 routine into a deliberate C2 learning plan', fa: 'روال C1 را به یک برنامه آگاهانه برای C2 تبدیل کنی', ar: 'تحويل روتين C1 إلى خطة تعلم واعية لمستوى C2', tr: 'C1 rutinini bilinçli bir C2 öğrenme planına dönüştürmeyi', ru: 'превратить рутину C1 в осознанный план обучения C2', ckb: 'ڕۆتینی C1 بکەیت بە پلانێکی ئاگاهانەی فێربوونی C2', kmr: 'rutina C1 veguherandina planeke fêrbûna C2 ya hişyar', pl: 'przekształcenie rutyny C1 w świadomy plan nauki C2', ro: 'transformarea rutinei C1 într-un plan conștient de învățare C2', sq: 'shndërrimin e rutinës C1 në një plan të vetëdijshëm për C2' },
    grammar: ['c1-c1-grammar-review-map', 'C1-Review-Map'],
    target: ['course-lesson', 'c2-von-c1-zu-c2-ankommen']
  }
];

function translations(map) {
  return langs.map(language => ({ language, text: map[language] }));
}

function title(key) {
  return { title: titles[key].de, titleTranslations: translations(titles[key]) };
}

function orientInstruction(item) {
  return {
    de: `Lies den Lektionstext zu ${item.topic.de} und notiere: Welche C1-Kompetenzen werden kombiniert, welche Stelle wirkt noch unsicher, und was waere der naechste Beleg fuer Fortschritt?`,
    en: `Read the lesson text on ${item.topic.en} and note which C1 skills are combined, which point still feels uncertain, and what the next proof of progress would be.`,
    fa: `متن درس درباره ${item.topic.fa} را بخوان و یادداشت کن: کدام مهارت‌های C1 با هم ترکیب می‌شوند، کدام بخش هنوز نامطمئن است و نشانه بعدی پیشرفت چه می‌تواند باشد.`,
    ar: `اقرأ نص الدرس عن ${item.topic.ar} وسجّل: أي كفاءات C1 تُدمج، أي موضع ما زال غير مستقر، وما الدليل التالي على التقدم.`,
    tr: `${item.topic.tr} konulu ders metnini oku ve not et: hangi C1 becerileri birleşiyor, hangi nokta hâlâ güvensiz, ilerlemenin sonraki kanıtı ne olabilir.`,
    ru: `Прочитай текст урока о теме «${item.topic.ru}» и отметь, какие навыки C1 соединяются, какое место еще неуверенно и что было бы следующим доказательством прогресса.`,
    ckb: `دەقی وانەکە لەسەر ${item.topic.ckb} بخوێنەوە و بنووسە: کام تواناکانی C1 پێکەوە تێکەڵ دەبن، کام شوێن هێشتا نادڵنیایە و بەڵگەی داهاتووی پێشکەوتن چی دەبێت.`,
    kmr: `Nivîsa dersê li ser ${item.topic.kmr} bixwîne û binivîse: kîjan jêhatinên C1 bi hev re tên girêdan, kîjan xal hîn neewle ye û delîla din a pêşketinê çi dikare be.`,
    pl: `Przeczytaj tekst lekcji o temacie: ${item.topic.pl} i zanotuj, które kompetencje C1 się łączą, które miejsce nadal jest niepewne i jaki byłby kolejny dowód postępu.`,
    ro: `Citește textul lecției despre ${item.topic.ro} și notează ce competențe C1 sunt combinate, ce punct încă pare nesigur și care ar fi următoarea dovadă a progresului.`,
    sq: `Lexo tekstin e mësimit për ${item.topic.sq} dhe shëno cilat aftësi C1 kombinohen, cila pjesë është ende e pasigurt dhe cila do të ishte prova tjetër e përparimit.`
  };
}

function connectInstruction(item) {
  return {
    de: `Oeffne die verlinkte Ressource und pruefe sie mit einer Frage: Hilft sie dir dabei, ${item.focus.de}?`,
    en: `Open the linked resource and check it with one question: does it help you with this skill: ${item.focus.en}?`,
    fa: `منبع لینک‌شده را باز کن و با یک پرسش بررسی کن: آیا به تو کمک می‌کند این مهارت را انجام بدهی: ${item.focus.fa}؟`,
    ar: `افتح المورد المرتبط وافحصه بسؤال واحد: هل يساعدك على هذه المهارة: ${item.focus.ar}؟`,
    tr: `Bağlantılı kaynağı aç ve tek soruyla kontrol et: şu beceriye yardımcı oluyor mu: ${item.focus.tr}?`,
    ru: `Открой связанный ресурс и проверь одним вопросом: помогает ли он с этим навыком: ${item.focus.ru}?`,
    ckb: `سەرچاوەی بەستەرکراو بکەرەوە و بە یەک پرسیار بپشکنە: ئایا یارمەتیت دەدات بۆ ئەم توانایە: ${item.focus.ckb}؟`,
    kmr: `Çavkaniya girêdayî veke û bi yek pirsê kontrol bike: gelo alîkarîya vê jêhatinê dike: ${item.focus.kmr}?`,
    pl: `Otwórz połączony zasób i sprawdź go jednym pytaniem: czy pomaga w tej umiejętności: ${item.focus.pl}?`,
    ro: `Deschide resursa legată și verific-o cu o întrebare: te ajută la această abilitate: ${item.focus.ro}?`,
    sq: `Hap burimin e lidhur dhe kontrolloje me një pyetje: a të ndihmon për këtë aftësi: ${item.focus.sq}?`
  };
}

function produceInstruction() {
  return {
    de: 'Erstelle eine kurze Portfolio-Leistung: 180 bis 220 Woerter oder drei Minuten Sprechen. Nutze keine neue Aufgabe, sondern verbessere ein eigenes C1-Thema.',
    en: 'Create a short portfolio performance: 180 to 220 words or three minutes of speaking. Do not use a new task; improve one of your own C1 topics.',
    fa: 'یک نمونه کوتاه برای پرونده یادگیری بساز: ۱۸۰ تا ۲۲۰ واژه یا سه دقیقه صحبت. کار جدید نساز؛ یکی از موضوع‌های C1 خودت را بهتر کن.',
    ar: 'أنشئ أداءً قصيرًا لملف التعلّم: من 180 إلى 220 كلمة أو ثلاث دقائق حديث. لا تستخدم مهمة جديدة؛ حسّن موضوعًا من موضوعاتك في C1.',
    tr: 'Kısa bir portfolyo ürünü oluştur: 180-220 kelime veya üç dakika konuşma. Yeni görev kullanma; kendi C1 konularından birini geliştir.',
    ru: 'Создай короткую работу для портфолио: 180-220 слов или три минуты речи. Не бери новое задание; улучши одну из своих тем C1.',
    ckb: 'ئەنجامدانێکی کورت بۆ پۆرتفۆلیۆ دروست بکە: ١٨٠ تا ٢٢٠ وشە یان سێ خولەک قسەکردن. ئەرکی نوێ بەکارمەهێنە؛ یەکێک لە بابەتەکانی C1 ـی خۆت باشتر بکە.',
    kmr: 'Performanseke kurt ji bo portfolyo çêbike: 180 heta 220 peyv an sê xulek axaftin. Erkeke nû bikar neyne; yek ji mijarên xwe yên C1 baştir bike.',
    pl: 'Utwórz krótką pracę do portfolio: 180-220 słów albo trzy minuty mówienia. Nie bierz nowego zadania; popraw jeden z własnych tematów C1.',
    ro: 'Creează o prestație scurtă de portofoliu: 180-220 de cuvinte sau trei minute de vorbire. Nu folosi o sarcină nouă; îmbunătățește o temă C1 proprie.',
    sq: 'Krijo një performancë të shkurtër për portofol: 180 deri në 220 fjalë ose tre minuta të folur. Mos përdor detyrë të re; përmirëso një temë tënden C1.'
  };
}

function refineInstruction() {
  return {
    de: 'Ueberarbeite nur drei Stellen: eine Stelle fuer Praezision, eine fuer Register, eine fuer Natuerlichkeit. Mehr Korrektur ist in diesem Schritt nicht das Ziel.',
    en: 'Revise only three places: one for precision, one for register, and one for naturalness. More correction is not the goal in this step.',
    fa: 'فقط سه بخش را بازنویسی کن: یکی برای دقت، یکی برای سطح رسمی‌بودن، یکی برای طبیعی‌تر شدن. در این مرحله هدف اصلاح زیاد نیست.',
    ar: 'راجع ثلاثة مواضع فقط: موضعًا للدقة، وموضعًا للسجل اللغوي، وموضعًا للطبيعية. التصحيح الكثير ليس هدف هذه الخطوة.',
    tr: 'Yalnızca üç yeri gözden geçir: biri kesinlik, biri register, biri doğallık için. Bu adımda daha fazla düzeltme hedef değil.',
    ru: 'Отредактируй только три места: одно для точности, одно для регистра, одно для естественности. Больше правок на этом шаге не цель.',
    ckb: 'تەنها سێ شوێن چاک بکەوە: یەکێک بۆ وردی، یەکێک بۆ تۆن، یەکێک بۆ سروشتیبوون. لەم هەنگاوەدا چاککردنەوەی زیاتر ئامانج نییە.',
    kmr: 'Tenê sê cihan sererast bike: yek ji bo rastî, yek ji bo register, yek ji bo xwezayîbûn. Di vê gavê de zêdetir rastkirin armanc nîne.',
    pl: 'Popraw tylko trzy miejsca: jedno pod precyzję, jedno pod rejestr, jedno pod naturalność. Więcej korekty nie jest celem tego kroku.',
    ro: 'Revizuiește doar trei locuri: unul pentru precizie, unul pentru registru, unul pentru naturalețe. Mai multă corectură nu este scopul acestui pas.',
    sq: 'Rishiko vetëm tri vende: një për saktësi, një për regjistër, një për natyrshmëri. Më shumë korrigjim nuk është qëllimi i këtij hapi.'
  };
}

function reviewInstruction() {
  return {
    de: 'Lege am Ende einen naechsten Schritt fest: wiederholen, vertiefen oder zu C2 wechseln. Begruende die Entscheidung mit einem konkreten Beleg.',
    en: 'At the end, set one next step: repeat, deepen, or move to C2. Justify the decision with concrete evidence.',
    fa: 'در پایان یک قدم بعدی مشخص کن: تکرار، عمیق‌تر کردن یا رفتن به C2. تصمیم را با یک شاهد مشخص توجیه کن.',
    ar: 'في النهاية حدّد خطوة تالية واحدة: المراجعة، التعمّق أو الانتقال إلى C2. برّر القرار بدليل ملموس.',
    tr: 'Sonunda bir sonraki adımı belirle: tekrar, derinleştirme veya C2’ye geçiş. Kararı somut bir kanıtla gerekçelendir.',
    ru: 'В конце определи следующий шаг: повторить, углубить или перейти к C2. Обоснуй решение конкретным доказательством.',
    ckb: 'لە کۆتاییدا یەک هەنگاوی داهاتوو دیاری بکە: دووبارەکردنەوە، قووڵکردنەوە یان چوون بۆ C2. بڕیارەکە بە بەڵگەیەکی دیاریکراو ڕوون بکەوە.',
    kmr: 'Di dawiyê de gaveke din diyar bike: dubarekirin, kûrkirin an derbasbûn bo C2. Biryara xwe bi delîlekî konkret şîrove bike.',
    pl: 'Na końcu ustal jeden następny krok: powtórzyć, pogłębić albo przejść do C2. Uzasadnij decyzję konkretnym dowodem.',
    ro: 'La final stabilește un pas următor: repetare, aprofundare sau trecere la C2. Justifică decizia cu o dovadă concretă.',
    sq: 'Në fund përcakto një hap të radhës: përsëritje, thellim ose kalim në C2. Arsyeto vendimin me një provë konkrete.'
  };
}

function activity(kind, titleKey, instructionMap, targetType, targetSlug, sortOrder, estimatedMinutes, isRequired = true) {
  return {
    kind,
    ...title(titleKey),
    instruction: instructionMap.de,
    instructionTranslations: translations(instructionMap),
    targetType,
    targetSlug,
    estimatedMinutes,
    sortOrder,
    isRequired
  };
}

for (const item of items) {
  const lesson = lessons.find(candidate => candidate.slug === item.slug);
  if (!lesson) {
    throw new Error(`Lesson not found: ${item.slug}`);
  }

  const targetKind = item.target[0] === 'roleplay'
    ? 'roleplay'
    : item.target[0] === 'writing-template'
      ? 'write'
      : item.target[0] === 'course-lesson'
        ? 'read'
        : item.target[0] === 'grammar-topic'
          ? 'grammar'
          : 'exam-prep';

  lesson.activityBlocks = [
    activity('read', 'orient', orientInstruction(item), 'none', null, 10, 6),
    activity(targetKind, 'connect', connectInstruction(item), item.target[0], item.target[1], 20, 8),
    activity('practice', 'produce', produceInstruction(), 'none', null, 30, 12),
    activity('review', 'refine', refineInstruction(), 'grammar-topic', item.grammar[0], 40, 8),
    activity('review', 'review', reviewInstruction(), 'none', null, 50, 5)
  ];
}

fs.writeFileSync(file, `${JSON.stringify(data, null, 2)}\n`);
console.log('Updated 10 C1 Module 12 lessons with 50 activity blocks.');

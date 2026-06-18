const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c2-stil-souveraenitaet-und-komplexer-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'C2-Perspektive setzen',
    en: 'Set the C2 perspective',
    fa: 'زاویه نگاه C2 را تنظیم کن',
    ar: 'اضبط منظور C2',
    tr: 'C2 bakışını kur',
    ru: 'Задай перспективу C2',
    ckb: 'دیدی C2 دابنێ',
    kmr: 'Perspektîfa C2 saz bike',
    pl: 'Ustaw perspektywę C2',
    ro: 'Setează perspectiva C2',
    sq: 'Vendos këndvështrimin C2'
  },
  language: {
    de: 'Feinheit sprachlich sichern',
    en: 'Secure the linguistic subtlety',
    fa: 'ظرافت زبانی را مطمئن کن',
    ar: 'ثبّت الدقة اللغوية الدقيقة',
    tr: 'Dilsel inceliği güvenceye al',
    ru: 'Закрепи языковую тонкость',
    ckb: 'وردی زمانی دڵنیا بکە',
    kmr: 'Hûrgiliya zimanî misoger bike',
    pl: 'Zabezpiecz subtelność językową',
    ro: 'Asigură finețea lingvistică',
    sq: 'Siguro hollësinë gjuhësore'
  },
  target: {
    de: 'Anspruch praktisch pruefen',
    en: 'Test the demand in practice',
    fa: 'سطح انتظار را عملی آزمایش کن',
    ar: 'اختبر مستوى المتطلب عمليًا',
    tr: 'Beklentiyi pratikte sına',
    ru: 'Проверь требование на практике',
    ckb: 'ئاستی داواکاری بە کرداری بپشکنە',
    kmr: 'Asta daxwazê bi pratîkî biceribîne',
    pl: 'Sprawdź wymaganie w praktyce',
    ro: 'Testează cerința practic',
    sq: 'Testo kërkesën praktikisht'
  },
  transfer: {
    de: 'Eigene Stimme kalibrieren',
    en: 'Calibrate your own voice',
    fa: 'صدای شخصی خودت را کالیبره کن',
    ar: 'عاير صوتك الشخصي',
    tr: 'Kendi sesini kalibre et',
    ru: 'Откалибруй собственный голос',
    ckb: 'دەنگی تایبەتی خۆت ڕێکبخە',
    kmr: 'Dengê xwe yê taybet kalîbre bike',
    pl: 'Skalibruj własny głos',
    ro: 'Calibrează-ți vocea proprie',
    sq: 'Kalibro zërin tënd'
  },
  review: {
    de: 'C2-Kriterium festhalten',
    en: 'Record the C2 criterion',
    fa: 'معیار C2 را ثبت کن',
    ar: 'سجّل معيار C2',
    tr: 'C2 ölçütünü kaydet',
    ru: 'Зафиксируй критерий C2',
    ckb: 'پێوەری C2 تۆمار بکە',
    kmr: 'Pîvana C2 tomar bike',
    pl: 'Zapisz kryterium C2',
    ro: 'Notează criteriul C2',
    sq: 'Regjistro kriterin C2'
  }
};

const items = [
  {
    slug: 'c2-von-c1-zu-c2-ankommen',
    topic: { de: 'den Schritt von C1 zu C2', en: 'the step from C1 to C2', fa: 'گام از C1 به C2', ar: 'الانتقال من C1 إلى C2', tr: 'C1’den C2’ye geçiş', ru: 'переход от C1 к C2', ckb: 'هەنگاوەکە لە C1 بۆ C2', kmr: 'gava ji C1 ber bi C2 ve', pl: 'przejście z C1 do C2', ro: 'trecerea de la C1 la C2', sq: 'kalimi nga C1 në C2' },
    focus: { de: 'nicht mehr nur korrekt zu sein, sondern Absicht, Wirkung und Stil bewusst zu steuern', en: 'not merely being correct anymore, but deliberately controlling intention, effect, and style', fa: 'دیگر فقط درست حرف نزنی، بلکه نیت، اثر و سبک را آگاهانه کنترل کنی', ar: 'ألا تكتفي بالصحة اللغوية، بل تتحكم بوعي في النية والأثر والأسلوب', tr: 'artık yalnızca doğru olmakla yetinmeyip niyet, etki ve üslubu bilinçli yönetmeyi', ru: 'не просто говорить правильно, а осознанно управлять намерением, эффектом и стилем', ckb: 'تەنها ڕاست قسە نەکەیت، بەڵکو نیاز، کاریگەری و ستایل بە ئاگایی کۆنترۆڵ بکەیت', kmr: 'ne tenê rast axaftin, lê armanc, bandor û şêwazê bi hişyarî birêve birin', pl: 'nie tylko mówić poprawnie, lecz świadomie sterować intencją, efektem i stylem', ro: 'să nu fii doar corect, ci să controlezi conștient intenția, efectul și stilul', sq: 'të mos jesh vetëm i saktë, por të drejtosh me vetëdije qëllimin, efektin dhe stilin' },
    grammar: ['c2-c2-register-review', 'C2-Register-Review'],
    target: ['exam-prep-unit', 'c2-pruefungsanforderungen-souveraen-einordnen']
  },
  {
    slug: 'c2-feine-bedeutungsunterschiede-erkennen',
    topic: { de: 'feine Bedeutungsunterschiede', en: 'subtle differences in meaning', fa: 'تفاوت‌های ظریف معنایی', ar: 'الفروق الدقيقة في المعنى', tr: 'ince anlam farkları', ru: 'тонкие различия значения', ckb: 'جیاوازییە وردەکانی واتا', kmr: 'cudahiyên hûr ên wateyê', pl: 'subtelne różnice znaczeniowe', ro: 'diferențe fine de sens', sq: 'dallime të holla kuptimore' },
    focus: { de: 'nahe Bedeutungen nach Haltung, Sicherheit, Distanz und Bewertung zu unterscheiden', en: 'distinguishing close meanings by attitude, certainty, distance, and evaluation', fa: 'معناهای نزدیک را بر اساس نگرش، میزان اطمینان، فاصله‌گیری و ارزیابی از هم جدا کنی', ar: 'تمييز المعاني المتقاربة حسب الموقف ودرجة اليقين والمسافة والتقييم', tr: 'yakın anlamları tutum, kesinlik, mesafe ve değerlendirmeye göre ayırmayı', ru: 'различать близкие значения по позиции, уверенности, дистанции и оценке', ckb: 'واتا نزیکەکان بە پێی هەڵوێست، دڵنیایی، دووری و هەڵسەنگاندن جیا بکەیتەوە', kmr: 'wateyên nêzîk li gorî helwest, ewlehî, dûrî û nirxandinê cuda kirin', pl: 'odróżnianie bliskich znaczeń według postawy, pewności, dystansu i oceny', ro: 'diferențierea sensurilor apropiate după atitudine, certitudine, distanță și evaluare', sq: 'ndarjen e kuptimeve të afërta sipas qëndrimit, sigurisë, distancës dhe vlerësimit' },
    grammar: ['c2-subtle-modal-verb-nuance', 'subtile Modalitaet'],
    target: ['exam-prep-unit', 'c2-implizite-wertung-erkennen']
  },
  {
    slug: 'c2-spontanitaet-und-kontrolle-verbinden',
    topic: { de: 'Spontanitaet und Kontrolle', en: 'spontaneity and control', fa: 'خودانگیختگی و کنترل', ar: 'العفوية والتحكم', tr: 'spontanlık ve kontrol', ru: 'спонтанность и контроль', ckb: 'خۆڕسکی و کۆنترۆڵ', kmr: 'spontanî û kontrol', pl: 'spontaniczność i kontrola', ro: 'spontaneitate și control', sq: 'spontanitet dhe kontroll' },
    focus: { de: 'spontan zu reagieren, ohne Linie, Register oder Praezision zu verlieren', en: 'responding spontaneously without losing line, register, or precision', fa: 'خودجوش پاسخ بدهی، بدون اینکه خط فکری، لحن یا دقت را از دست بدهی', ar: 'الرد بعفوية من دون فقدان الخط الفكري أو السجل أو الدقة', tr: 'çizgiyi, registeri veya kesinliği kaybetmeden spontan yanıt vermeyi', ru: 'отвечать спонтанно, не теряя линии, регистра и точности', ckb: 'بە خۆڕسکی وەڵام بدەیت، بەبێ لەدەستدانی هێڵی بیر، تۆن یان وردی', kmr: 'spontan bersiv dan bê windakirina xet, register an rastiyê', pl: 'reagowanie spontanicznie bez utraty linii, rejestru lub precyzji', ro: 'a răspunde spontan fără a pierde linia, registrul sau precizia', sq: 'të përgjigjesh spontanisht pa humbur vijën, regjistrin ose saktësinë' },
    grammar: ['c2-rhetorical-sentence-structures', 'rhetorische Satzsteuerung'],
    target: ['roleplay', 'c2-goethe-c2-eine-spontane-nachfrage-praezise-beantworten']
  },
  {
    slug: 'c2-komplexe-quellen-synthetisieren',
    topic: { de: 'komplexe Quellen synthetisieren', en: 'synthesizing complex sources', fa: 'ترکیب کردن منابع پیچیده', ar: 'تركيب مصادر معقدة', tr: 'karmaşık kaynakları sentezleme', ru: 'синтез сложных источников', ckb: 'تێکەڵکردنی سەرچاوە ئاڵۆزەکان', kmr: 'sentezkirina çavkaniyên tevlihev', pl: 'synteza złożonych źródeł', ro: 'sintetizarea surselor complexe', sq: 'sintezimi i burimeve të ndërlikuara' },
    focus: { de: 'Quellen nicht nebeneinanderzustellen, sondern Perspektiven, Status und Spannung zusammenzufuehren', en: 'not placing sources side by side, but bringing together perspectives, status, and tension', fa: 'منابع را فقط کنار هم نگذاری، بلکه دیدگاه‌ها، اعتبار و تنش میان آن‌ها را با هم ترکیب کنی', ar: 'ألا تضع المصادر جنبًا إلى جنب فقط، بل تجمع المنظورات والمكانة والتوتر بينها', tr: 'kaynakları yan yana koymakla yetinmeyip bakışları, statüyü ve gerilimi birleştirmeyi', ru: 'не просто ставить источники рядом, а соединять перспективы, статус и напряжение между ними', ckb: 'سەرچاوەکان تەنها لە پاڵ یەک دابنەنێیت، بەڵکو دیدگا، پێگە و گرژییان پێکەوە بهێنیت', kmr: 'çavkaniyan tenê li kêleka hev danîn na, lê perspektîf, statû û tengaviyê bi hev re anîn', pl: 'nie zestawiać źródeł obok siebie, lecz łączyć perspektywy, status i napięcie', ro: 'să nu așezi sursele doar una lângă alta, ci să unești perspectivele, statutul și tensiunea', sq: 'të mos i vendosësh burimet thjesht pranë njëra-tjetrës, por të bashkosh perspektivat, statusin dhe tensionin' },
    grammar: ['c2-implicit-references-and-cohesion', 'implizite Bezuege und Kohaesion'],
    target: ['exam-prep-unit', 'c2-quellenperspektiven-synthetisieren']
  },
  {
    slug: 'c2-stilbewusstsein-entwickeln',
    topic: { de: 'Stilbewusstsein', en: 'style awareness', fa: 'آگاهی سبکی', ar: 'الوعي الأسلوبي', tr: 'üslup bilinci', ru: 'стилистическая осознанность', ckb: 'ئاگایی لە ستایل', kmr: 'hişyariya şêwazê', pl: 'świadomość stylu', ro: 'conștiință stilistică', sq: 'vetëdije stilistike' },
    focus: { de: 'nicht nur schoen zu formulieren, sondern die Wirkung einer Stilentscheidung zu begruenden', en: 'not merely formulating elegantly, but justifying the effect of a stylistic choice', fa: 'فقط زیبا ننویسی، بلکه اثر یک انتخاب سبکی را هم توضیح بدهی', ar: 'ألا تصوغ بشكل جميل فقط، بل تبرر أثر اختيار أسلوبي', tr: 'yalnızca güzel ifade etmek değil, bir üslup seçiminin etkisini gerekçelendirmek', ru: 'не просто красиво формулировать, а обосновывать эффект стилистического выбора', ckb: 'تەنها جوان دەربڕین نەکەیت، بەڵکو کاریگەری هەڵبژاردنی ستایلیش ڕوون بکەیتەوە', kmr: 'ne tenê xweşik gotin, lê bandora hilbijartineke şêwazî jî şîrove kirin', pl: 'nie tylko ładnie formułować, lecz uzasadniać efekt decyzji stylistycznej', ro: 'nu doar să formulezi frumos, ci să justifici efectul unei alegeri stilistice', sq: 'jo vetëm të formulosh bukur, por të arsyetosh efektin e një zgjedhjeje stilistike' },
    grammar: ['c2-stylistic-variation-in-german-grammar', 'stilistische Variation'],
    target: ['exam-prep-unit', 'c2-stilistische-dichte-dosieren']
  },
  {
    slug: 'c2-selbstlektorat-auf-c2-niveau',
    topic: { de: 'Selbstlektorat auf C2-Niveau', en: 'self-editing at C2 level', fa: 'خودویرایش در سطح C2', ar: 'المراجعة الذاتية على مستوى C2', tr: 'C2 düzeyinde öz editörlük', ru: 'саморедактура на уровне C2', ckb: 'خۆدەستکاریکردن لە ئاستی C2', kmr: 'xwe-sererastkirin li asta C2', pl: 'samoredakcja na poziomie C2', ro: 'autoeditare la nivel C2', sq: 'vetëredaktim në nivel C2' },
    focus: { de: 'eigene Texte nach Wirkung, Dichte und Risiko zu ueberarbeiten, nicht nur nach Fehlern', en: 'revising your own texts for effect, density, and risk, not only for errors', fa: 'متن خودت را بر اساس اثر، فشردگی و ریسک بازبینی کنی، نه فقط بر اساس خطاها', ar: 'مراجعة نصوصك بحسب الأثر والكثافة والمخاطر، لا بحسب الأخطاء فقط', tr: 'kendi metinlerini yalnızca hatalara göre değil, etki, yoğunluk ve risk açısından gözden geçirmeyi', ru: 'редактировать собственные тексты по эффекту, плотности и риску, а не только по ошибкам', ckb: 'دەقەکانی خۆت بە پێی کاریگەری، چڕی و مەترسی چاک بکەیتەوە، نەک تەنها بە پێی هەڵە', kmr: 'nivîsên xwe li gorî bandor, tîrbûn û rîskê sererast kirin, ne tenê li gorî şaşitiyan', pl: 'redagowanie własnych tekstów pod kątem efektu, gęstości i ryzyka, nie tylko błędów', ro: 'revizuirea propriilor texte după efect, densitate și risc, nu doar după greșeli', sq: 'rishikimin e teksteve të tua sipas efektit, dendësisë dhe rrezikut, jo vetëm gabimeve' },
    grammar: ['c2-c2-common-pitfalls', 'C2-Risikostellen'],
    target: ['writing-template', 'c2-abschlussstatement-mit-stilistischer-souveraenitaet']
  },
  {
    slug: 'c2-hochdichte-inputs-verarbeiten',
    topic: { de: 'hochdichte Inputs verarbeiten', en: 'processing dense input', fa: 'پردازش ورودی‌های فشرده', ar: 'معالجة مدخلات كثيفة', tr: 'yoğun girdileri işleme', ru: 'обработка плотного входного материала', ckb: 'پرۆسەکردنی زانیاریی چڕ', kmr: 'pêvajokirina inputên tîr', pl: 'przetwarzanie gęstych informacji', ro: 'procesarea inputurilor dense', sq: 'përpunimi i inputeve të dendura' },
    focus: { de: 'Hauptlinie, Nebenbemerkung und implizite Bewertung getrennt zu sichern', en: 'securing main line, side comment, and implicit evaluation separately', fa: 'خط اصلی، نکته حاشیه‌ای و ارزیابی پنهان را جداگانه ثبت کنی', ar: 'تثبيت الخط الرئيسي والملاحظة الجانبية والتقييم الضمني كلٌ على حدة', tr: 'ana çizgiyi, yan notu ve örtük değerlendirmeyi ayrı güvenceye almayı', ru: 'отдельно фиксировать основную линию, побочное замечание и скрытую оценку', ckb: 'هێڵی سەرەکی، تێبینی لاوەکی و هەڵسەنگاندنی شاراوە جیا جیا بپارێزیت', kmr: 'xeta sereke, têbîniya kêlekî û nirxandina veşartî cuda parastin', pl: 'oddzielne uchwycenie głównej linii, uwagi pobocznej i ukrytej oceny', ro: 'fixarea separată a liniei principale, observației laterale și evaluării implicite', sq: 'sigurimin veçmas të vijës kryesore, vërejtjes anësore dhe vlerësimit të nënkuptuar' },
    grammar: ['c2-journalistic-compression', 'journalistische Verdichtung'],
    target: ['exam-prep-unit', 'c2-implizite-bezuege-im-hoeren-sichern']
  },
  {
    slug: 'c2-repertoire-ohne-kuenstlichkeit-erweitern',
    topic: { de: 'Repertoire ohne Kuenstlichkeit erweitern', en: 'expanding repertoire without artificiality', fa: 'گسترش دامنه بیان بدون مصنوعی شدن', ar: 'توسيع الرصيد التعبيري من دون تصنع', tr: 'yapaylık olmadan repertuar genişletme', ru: 'расширение репертуара без искусственности', ckb: 'فراوانکردنی دەربڕین بەبێ دەستکردی', kmr: 'firehkirina repertuwarê bê çêkirîbûn', pl: 'poszerzanie repertuaru bez sztuczności', ro: 'extinderea repertoriului fără artificialitate', sq: 'zgjerimi i repertorit pa artificialitet' },
    focus: { de: 'neue Wendungen erst zu testen und dann nur dort einzusetzen, wo sie wirklich tragen', en: 'testing new expressions first and then using them only where they genuinely carry the meaning', fa: 'عبارت‌های جدید را اول امتحان کنی و بعد فقط جایی به کار ببری که واقعاً معنا را بهتر حمل می‌کنند', ar: 'اختبار التعابير الجديدة أولًا ثم استخدامها فقط حيث تحمل المعنى فعلًا', tr: 'yeni ifadeleri önce deneyip sonra yalnızca anlamı gerçekten taşıdıkları yerde kullanmayı', ru: 'сначала проверять новые обороты, а затем использовать их только там, где они действительно несут смысл', ckb: 'دەربڕینی نوێ سەرەتا تاقی بکەیتەوە و پاشان تەنها لەو شوێنانە بەکاری بهێنیت کە بەڕاستی واتاکە هەڵدەگرن', kmr: 'derbirînên nû pêşî ceribandin û paşê tenê li cihên ku bi rastî wateyê hildigirin bikaranîn', pl: 'najpierw testować nowe zwroty, a potem używać ich tylko tam, gdzie naprawdę niosą znaczenie', ro: 'testarea expresiilor noi mai întâi și folosirea lor doar acolo unde chiar susțin sensul', sq: 'të provosh së pari shprehjet e reja dhe pastaj t’i përdorësh vetëm aty ku vërtet mbajnë kuptimin' },
    grammar: ['c2-register-and-syntactic-choice', 'Register und syntaktische Wahl'],
    target: ['writing-template', 'c2-institutionelle-antwort-diplomatisch-korrigieren']
  },
  {
    slug: 'c2-eigene-stimme-auf-deutsch-finden',
    topic: { de: 'die eigene Stimme auf Deutsch', en: 'your own voice in German', fa: 'صدای شخصی خودت در آلمانی', ar: 'صوتك الشخصي بالألمانية', tr: 'Almancada kendi sesin', ru: 'собственный голос на немецком', ckb: 'دەنگی تایبەتی خۆت بە ئەڵمانی', kmr: 'dengê xwe yê taybet bi Almanî', pl: 'własny głos po niemiecku', ro: 'vocea proprie în germană', sq: 'zëri yt në gjermanisht' },
    focus: { de: 'persoenlich erkennbar zu bleiben, ohne Register, Genauigkeit oder Hoeflichkeit zu opfern', en: 'remaining personally recognizable without sacrificing register, precision, or politeness', fa: 'شخصیت زبانی خودت را حفظ کنی، بدون اینکه لحن مناسب، دقت یا ادب را قربانی کنی', ar: 'الحفاظ على بصمتك الشخصية من دون التضحية بالسجل أو الدقة أو اللباقة', tr: 'register, kesinlik veya nezaketten vazgeçmeden kişisel olarak tanınabilir kalmayı', ru: 'оставаться узнаваемым лично, не жертвуя регистром, точностью или вежливостью', ckb: 'ناسنامەی زمانی خۆت بپارێزیت، بەبێ قوربانیکردنی تۆن، وردی یان ڕێز', kmr: 'nasnameya zimanî ya xwe parastin bê qurbankirina register, rastî an nezaketê', pl: 'pozostać rozpoznawalnym osobiście bez poświęcania rejestru, precyzji lub uprzejmości', ro: 'a rămâne recognoscibil personal fără a sacrifica registrul, precizia sau politețea', sq: 'të mbetesh i dallueshëm personalisht pa sakrifikuar regjistrin, saktësinë ose mirësjelljen' },
    grammar: ['c2-c2-register-review', 'C2-Register-Review'],
    target: ['roleplay', 'c2-eine-fachliche-kritik-ohne-abwertung-formulieren']
  },
  {
    slug: 'c2-von-c1-zu-c2-wiederholen',
    topic: { de: 'den Uebergang von C1 zu C2 wiederholen', en: 'reviewing the transition from C1 to C2', fa: 'مرور گذار از C1 به C2', ar: 'مراجعة الانتقال من C1 إلى C2', tr: 'C1’den C2’ye geçişi tekrar etme', ru: 'повторение перехода от C1 к C2', ckb: 'دووبارەکردنەوەی گواستنەوە لە C1 بۆ C2', kmr: 'dubarekirina derbasbûna ji C1 bo C2', pl: 'powtórka przejścia z C1 do C2', ro: 'recapitularea trecerii de la C1 la C2', sq: 'përsëritja e kalimit nga C1 në C2' },
    focus: { de: 'einen persoenlichen C2-Kompass aus Bedeutung, Stil, Quellenarbeit und Spontanitaet zu bauen', en: 'building a personal C2 compass from meaning, style, source work, and spontaneity', fa: 'از معنا، سبک، کار با منبع و پاسخ خودجوش یک قطب‌نمای شخصی برای C2 بسازی', ar: 'بناء بوصلة شخصية لـ C2 من المعنى والأسلوب والعمل مع المصادر والعفوية', tr: 'anlam, üslup, kaynak çalışması ve spontanlıktan kişisel bir C2 pusulası oluşturmayı', ru: 'создать личный компас C2 из смысла, стиля, работы с источниками и спонтанности', ckb: 'لە واتا، ستایل، کارکردن بە سەرچاوە و خۆڕسکی کۆمپاسێکی تایبەتی C2 دروست بکەیت', kmr: 'ji wate, şêwaz, xebata bi çavkaniyan û spontanî pusûleyeke xweser a C2 çêkirin', pl: 'zbudowanie osobistego kompasu C2 ze znaczenia, stylu, pracy ze źródłami i spontaniczności', ro: 'construirea unei busole personale C2 din sens, stil, lucru cu surse și spontaneitate', sq: 'ndërtimin e një busulle personale C2 nga kuptimi, stili, puna me burime dhe spontaniteti' },
    grammar: ['c2-c2-grammar-review-map', 'C2-Review-Map'],
    target: ['exam-prep-unit', 'c2-pruefungsanforderungen-souveraen-einordnen']
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
    de: `Lies den Lektionstext zu ${item.topic.de} und markiere: Wo reicht C1 noch aus, und wo verlangt C2 mehr Kontrolle ueber Wirkung, Dichte oder Stil?`,
    en: `Read the lesson text on ${item.topic.en} and mark where C1 is still enough and where C2 demands more control over effect, density, or style.`,
    fa: `متن درس درباره ${item.topic.fa} را بخوان و مشخص کن: کجا C1 هنوز کافی است و کجا C2 کنترل بیشتری روی اثر، فشردگی یا سبک می‌خواهد.`,
    ar: `اقرأ نص الدرس عن ${item.topic.ar} وحدد: أين ما زال C1 كافيًا، وأين يتطلب C2 تحكمًا أكبر في الأثر أو الكثافة أو الأسلوب.`,
    tr: `${item.topic.tr} konulu ders metnini oku ve işaretle: nerede C1 hâlâ yeterli, nerede C2 etki, yoğunluk veya üslup üzerinde daha fazla kontrol istiyor.`,
    ru: `Прочитай текст урока о теме «${item.topic.ru}» и отметь, где C1 еще достаточно, а где C2 требует большего контроля над эффектом, плотностью или стилем.`,
    ckb: `دەقی وانەکە لەسەر ${item.topic.ckb} بخوێنەوە و دیاری بکە: لە کوێ C1 هێشتا بەسە، و لە کوێ C2 کۆنترۆڵی زیاتر لەسەر کاریگەری، چڕی یان ستایل دەخوازێت.`,
    kmr: `Nivîsa dersê li ser ${item.topic.kmr} bixwîne û nîşan bike: li ku C1 hîn têr e, û li ku C2 kontrola zêdetir li ser bandor, tîrbûn an şêwaz dixwaze.`,
    pl: `Przeczytaj tekst lekcji o temacie: ${item.topic.pl} i zaznacz, gdzie C1 jeszcze wystarcza, a gdzie C2 wymaga większej kontroli nad efektem, gęstością lub stylem.`,
    ro: `Citește textul lecției despre ${item.topic.ro} și marchează unde C1 încă ajunge și unde C2 cere mai mult control asupra efectului, densității sau stilului.`,
    sq: `Lexo tekstin e mësimit për ${item.topic.sq} dhe shëno ku C1 ende mjafton dhe ku C2 kërkon më shumë kontroll mbi efektin, dendësinë ose stilin.`
  };
}

function languageInstruction(item) {
  return {
    de: `Oeffne ${item.grammar[1]} und sammle drei Mittel, mit denen du ${item.focus.de}.`,
    en: `Open the linked language section and collect three devices that help you with this skill: ${item.focus.en}.`,
    fa: `بخش زبانی لینک‌شده را باز کن و سه ابزار پیدا کن که به این مهارت کمک کند: ${item.focus.fa}.`,
    ar: `افتح القسم اللغوي المرتبط واجمع ثلاث وسائل تساعد على هذه المهارة: ${item.focus.ar}.`,
    tr: `Bağlantılı dil bölümünü aç ve şu beceriye yardım eden üç araç topla: ${item.focus.tr}.`,
    ru: `Открой связанный языковой раздел и собери три средства для этого навыка: ${item.focus.ru}.`,
    ckb: `بەشی زمانی بەستەرکراو بکەرەوە و سێ ئامراز کۆبکەوە کە یارمەتی ئەم توانایە بدات: ${item.focus.ckb}.`,
    kmr: `Beşa zimanê ya girêdayî veke û sê amûran kom bike ku alîkarîya vê jêhatinê bikin: ${item.focus.kmr}.`,
    pl: `Otwórz połączoną sekcję językową i zbierz trzy środki wspierające tę umiejętność: ${item.focus.pl}.`,
    ro: `Deschide secțiunea lingvistică legată și adună trei mijloace care susțin această abilitate: ${item.focus.ro}.`,
    sq: `Hap seksionin e lidhur gjuhësor dhe mblidh tri mjete që ndihmojnë këtë aftësi: ${item.focus.sq}.`
  };
}

function targetInstruction(item) {
  return {
    de: `Bearbeite die verlinkte Ressource und pruefe nicht nur die Loesung, sondern die Wirkung: Gelingt dir damit, ${item.focus.de}?`,
    en: `Work through the linked resource and check not only the solution but the effect: does it help you ${item.focus.en}?`,
    fa: `منبع لینک‌شده را انجام بده و فقط جواب را بررسی نکن؛ اثر آن را هم بسنج: آیا تو را به این توانایی نزدیک‌تر می‌کند که ${item.focus.fa}؟`,
    ar: `اعمل على المورد المرتبط ولا تفحص الحل فقط؛ قيّم الأثر أيضًا: هل يقرّبك من القدرة على ${item.focus.ar}؟`,
    tr: `Bağlantılı kaynağı çalış ve yalnızca çözümü değil etkiyi de kontrol et: sana şu beceride gerçekten yardımcı oluyor mu: ${item.focus.tr}?`,
    ru: `Проработай связанный ресурс и проверь не только решение, но и эффект: помогает ли он тебе ${item.focus.ru}?`,
    ckb: `سەرچاوەی بەستەرکراو کاربکە و تەنها چارەسەرەکە مەپشکنە؛ کاریگەریش هەڵبسەنگێنە: ئایا نزیکت دەکاتەوە لەو توانایەی کە ${item.focus.ckb}؟`,
    kmr: `Çavkaniya girêdayî bixebitîne û ne tenê çareseriyê, bandorê jî binirxîne: gelo te nêzî vê jêhatîbûnê dike ku ${item.focus.kmr}?`,
    pl: `Przerób podlinkowany materiał i sprawdź nie tylko rozwiązanie, lecz także efekt: czy przybliża cię do tego, by ${item.focus.pl}?`,
    ro: `Lucrează resursa legată și verifică nu doar soluția, ci și efectul: te apropie de capacitatea de a ${item.focus.ro}?`,
    sq: `Puno me burimin e lidhur dhe kontrollo jo vetëm zgjidhjen, por edhe efektin: a të afron me aftësinë për të ${item.focus.sq}?`
  };
}

function transferInstruction() {
  return {
    de: 'Formuliere eine eigene Version in 120 bis 160 Woertern oder 90 Sekunden Sprechen. Halte danach fest, welche Wirkung du bewusst erzeugen wolltest.',
    en: 'Formulate your own version in 120 to 160 words or 90 seconds of speaking. Then record which effect you deliberately wanted to create.',
    fa: 'یک نسخه شخصی در ۱۲۰ تا ۱۶۰ واژه یا ۹۰ ثانیه صحبت بساز. بعد ثبت کن که آگاهانه می‌خواستی چه اثری ایجاد کنی.',
    ar: 'صغ نسختك الخاصة في 120 إلى 160 كلمة أو في 90 ثانية حديث. ثم سجّل الأثر الذي أردت إحداثه بوعي.',
    tr: '120-160 kelimelik ya da 90 saniyelik kendi versiyonunu oluştur. Sonra bilinçli olarak hangi etkiyi yaratmak istediğini yaz.',
    ru: 'Сформулируй собственную версию на 120-160 слов или 90 секунд речи. Затем зафиксируй, какой эффект ты хотел создать осознанно.',
    ckb: 'وەشانی خۆت بە ١٢٠ تا ١٦٠ وشە یان ٩٠ چرکە قسەکردن داڕێژە. پاشان تۆمار بکە کە بە ئاگایی دەتویست کام کاریگەری دروست بکەیت.',
    kmr: 'Guhertoya xwe bi 120 heta 160 peyvan an 90 çirkeyên axaftinê formule bike. Paşê tomar bike ka te bi hişyarî dixwest kîjan bandorê çêbikî.',
    pl: 'Sformułuj własną wersję w 120-160 słowach albo w 90 sekundach mówienia. Potem zapisz, jaki efekt świadomie chciałeś osiągnąć.',
    ro: 'Formulează propria versiune în 120-160 de cuvinte sau 90 de secunde de vorbire. Apoi notează ce efect ai vrut să creezi conștient.',
    sq: 'Formulo versionin tënd në 120 deri në 160 fjalë ose 90 sekonda të folur. Pastaj shëno çfarë efekti deshe të krijoje me vetëdije.'
  };
}

function reviewInstruction() {
  return {
    de: 'Notiere ein C2-Kriterium fuer diese Lektion: Was muss in Zukunft nicht nur korrekt, sondern wirksam, passend und eigenstaendig sein?',
    en: 'Write down one C2 criterion for this lesson: what must in future be not only correct, but effective, appropriate, and independent?',
    fa: 'برای این درس یک معیار C2 بنویس: از این به بعد چه چیزی باید فقط درست نباشد، بلکه اثرگذار، متناسب و مستقل هم باشد؟',
    ar: 'اكتب معيارًا واحدًا لمستوى C2 في هذا الدرس: ما الذي يجب أن يكون لاحقًا ليس صحيحًا فقط، بل مؤثرًا ومناسبًا ومستقلًا؟',
    tr: 'Bu ders için bir C2 ölçütü yaz: Bundan sonra ne yalnızca doğru değil, etkili, uygun ve bağımsız da olmalı?',
    ru: 'Запиши один критерий C2 для этого урока: что в будущем должно быть не только правильным, но и действенным, уместным и самостоятельным?',
    ckb: 'بۆ ئەم وانەیە یەک پێوەری C2 بنووسە: لە داهاتوودا چی نابێت تەنها ڕاست بێت، بەڵکو کاریگەر، گونجاو و سەربەخۆش بێت؟',
    kmr: 'Ji bo vê dersê pîvaneke C2 binivîse: ji niha pê ve çi divê ne tenê rast, lê bandorî, guncan û serbixwe jî be?',
    pl: 'Zapisz jedno kryterium C2 dla tej lekcji: co w przyszłości ma być nie tylko poprawne, lecz także skuteczne, odpowiednie i samodzielne?',
    ro: 'Notează un criteriu C2 pentru această lecție: ce trebuie de acum să fie nu doar corect, ci eficient, potrivit și autonom?',
    sq: 'Shkruaj një kriter C2 për këtë mësim: çfarë duhet në të ardhmen të jetë jo vetëm e saktë, por edhe efektive, e përshtatshme dhe e pavarur?'
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
      : 'exam-prep';

  lesson.activityBlocks = [
    activity('read', 'orient', orientInstruction(item), 'none', null, 10, 7),
    activity('grammar', 'language', languageInstruction(item), 'grammar-topic', item.grammar[0], 20, 9),
    activity(targetKind, 'target', targetInstruction(item), item.target[0], item.target[1], 30, 10),
    activity('practice', 'transfer', transferInstruction(), 'none', null, 40, 9),
    activity('review', 'review', reviewInstruction(), 'none', null, 50, 5)
  ];
}

fs.writeFileSync(file, `${JSON.stringify(data, null, 2)}\n`);
console.log('Updated 10 C2 Module 1 lessons with 50 activity blocks.');

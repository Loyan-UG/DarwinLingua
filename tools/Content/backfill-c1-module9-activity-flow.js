const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c1-souveraen-in-studium-beruf-und-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'Standpunktfeld klaeren',
    en: 'Clarify the field of positions',
    fa: 'فضای موضع‌ها را روشن کن',
    ar: 'وضّح مجال المواقف',
    tr: 'Tutum alanını netleştir',
    ru: 'Проясни поле позиций',
    ckb: 'بواری هەڵوێستەکان ڕوون بکەوە',
    kmr: 'Qada helwestan zelal bike',
    pl: 'Wyjaśnij pole stanowisk',
    ro: 'Clarifică spațiul pozițiilor',
    sq: 'Qartëso fushën e qëndrimeve'
  },
  language: {
    de: 'Nuance und Bewertung steuern',
    en: 'Control nuance and evaluation',
    fa: 'ظرافت و ارزیابی را کنترل کن',
    ar: 'تحكّم في الدقة والتقييم',
    tr: 'Nüansı ve değerlendirmeyi yönet',
    ru: 'Управляй нюансом и оценкой',
    ckb: 'وردی و هەڵسەنگاندن کۆنترۆڵ بکە',
    kmr: 'Nûans û nirxandinê birêve bibe',
    pl: 'Steruj niuansem i oceną',
    ro: 'Controlează nuanța și evaluarea',
    sq: 'Drejto nuancën dhe vlerësimin'
  },
  material: {
    de: 'Debatte praktisch trainieren',
    en: 'Practise the debate',
    fa: 'بحث را عملی تمرین کن',
    ar: 'تدرّب عمليًا على النقاش',
    tr: 'Tartışmayı pratikte çalış',
    ru: 'Практически отработай дебаты',
    ckb: 'مشتومڕەکە بە کرداری ڕاهێنان بکە',
    kmr: 'Nîqaşê bi pratîkî perwerde bike',
    pl: 'Przećwicz debatę praktycznie',
    ro: 'Exersează dezbaterea practic',
    sq: 'Ushtro debatin praktikisht'
  },
  apply: {
    de: 'Ausgewogenen Beitrag formulieren',
    en: 'Formulate a balanced contribution',
    fa: 'یک مشارکت متعادل بنویس',
    ar: 'صغ مساهمة متوازنة',
    tr: 'Dengeli bir katkı formüle et',
    ru: 'Сформулируй сбалансированный вклад',
    ckb: 'بەشدارییەکی هاوسەنگ داڕێژە',
    kmr: 'Beşdariyeke hevseng formule bike',
    pl: 'Sformułuj wyważoną wypowiedź',
    ro: 'Formulează o contribuție echilibrată',
    sq: 'Formulo një kontribut të balancuar'
  },
  review: {
    de: 'Fairness und Tiefe pruefen',
    en: 'Check fairness and depth',
    fa: 'انصاف و عمق را بررسی کن',
    ar: 'راجع الإنصاف والعمق',
    tr: 'Adaleti ve derinliği kontrol et',
    ru: 'Проверь справедливость и глубину',
    ckb: 'دادپەروەری و قووڵی بپشکنە',
    kmr: 'Dadperwerî û kûrahiyê kontrol bike',
    pl: 'Sprawdź sprawiedliwość i głębię',
    ro: 'Verifică echilibrul și profunzimea',
    sq: 'Kontrollo drejtësinë dhe thellësinë'
  }
};

const items = [
  {
    slug: 'c1-medienvertrauen-und-manipulation',
    topic: { de: 'Medienvertrauen und Manipulation', en: 'media trust and manipulation', fa: 'اعتماد به رسانه و دست‌کاری اطلاعات', ar: 'الثقة بالإعلام والتلاعب بالمعلومات', tr: 'medya güveni ve manipülasyon', ru: 'доверие к СМИ и манипуляция', ckb: 'متمانە بە میدیا و دەستکاری زانیاری', kmr: 'baweriya medyayê û manîpulasyon', pl: 'zaufanie do mediów i manipulacja', ro: 'încrederea în media și manipularea', sq: 'besimi te media dhe manipulimi' },
    focus: { de: 'zwischen berechtigter Skepsis, Beleglage und pauschalem Misstrauen unterscheiden', en: 'distinguishing justified skepticism, evidence, and blanket mistrust', fa: 'بین تردید موجه، وضعیت شواهد و بی‌اعتمادی کلی فرق بگذاری', ar: 'التمييز بين الشك المبرر وحالة الأدلة وانعدام الثقة العام', tr: 'haklı şüphe, kanıt durumu ve genelleyici güvensizliği ayırmayı', ru: 'различать обоснованный скепсис, доказательную базу и общее недоверие', ckb: 'جیاوازی بکەیت لە نێوان گومانێکی هۆکاربەدار، دۆخی بەڵگە و بێمتمانەیی گشتی', kmr: 'cudahî kirin di navbera gumanê bi sedem, rewşa delîlan û nebaweriya giştî de', pl: 'odróżnianie uzasadnionego sceptycyzmu, stanu dowodów i ogólnej nieufności', ro: 'diferențierea între scepticism justificat, dovezi și neîncredere generală', sq: 'dallimin mes skepticizmit të arsyetuar, provave dhe mosbesimit të përgjithshëm' },
    grammar: ['c1-reported-opinions', 'berichtete Positionen'],
    target: ['roleplay', 'c1-ueber-medienvertrauen-und-manipulation-sprechen']
  },
  {
    slug: 'c1-ki-im-alltag-differenziert-diskutieren',
    topic: { de: 'KI im Alltag differenziert diskutieren', en: 'discussing AI in everyday life in a nuanced way', fa: 'بحث دقیق و چندجانبه درباره هوش مصنوعی در زندگی روزمره', ar: 'مناقشة الذكاء الاصطناعي في الحياة اليومية بدقة', tr: 'günlük yaşamda yapay zekâyı nüanslı tartışma', ru: 'дифференцированное обсуждение ИИ в повседневности', ckb: 'مشتومڕی ورد لەسەر زیرەکی دەستکرد لە ژیانی ڕۆژانە', kmr: 'nîqaşa bi nûans li ser zîrekiya çêkirî di jiyana rojane de', pl: 'niuansowana dyskusja o AI w codzienności', ro: 'discutarea nuanțată a inteligenței artificiale în viața de zi cu zi', sq: 'diskutim i nuancuar për inteligjencën artificiale në përditshmëri' },
    focus: { de: 'Nutzen, Risiko und Verantwortung getrennt benennen, ohne Technik pauschal zu idealisieren oder abzulehnen', en: 'naming benefit, risk, and responsibility separately without idealizing or rejecting technology wholesale', fa: 'فایده، ریسک و مسئولیت را جداگانه بگویی، بدون اینکه فناوری را کلی ستایش یا رد کنی', ar: 'تسمية الفائدة والخطر والمسؤولية كلٌ على حدة من دون تمجيد التقنية أو رفضها كليًا', tr: 'fayda, risk ve sorumluluğu ayrı adlandırıp teknolojiyi toptan yüceltmemeyi veya reddetmemeyi', ru: 'отдельно называть пользу, риск и ответственность, не идеализируя и не отвергая технологию целиком', ckb: 'سوود، مەترسی و بەرپرسیاریەتی جیا جیا ناو ببەیت، بەبێ ئەوەی تەکنەلۆژیا بە گشتی ستایش یان ڕەت بکەیتەوە', kmr: 'sûd, rîsk û berpirsiyarî cuda cuda nav kirin bêyî ku teknolojiyê bi giştî mezinkirî an red bikî', pl: 'osobne nazwanie korzyści, ryzyka i odpowiedzialności bez idealizowania lub odrzucania technologii w całości', ro: 'numirea separată a beneficiului, riscului și responsabilității fără idealizarea sau respingerea totală a tehnologiei', sq: 'emërtimin veçmas të përfitimit, rrezikut dhe përgjegjësisë pa e idealizuar ose refuzuar teknologjinë në bllok' },
    grammar: ['c1-expressing-nuance-and-limitation', 'Nuancierung und Begrenzung'],
    target: ['roleplay', 'c1-kuenstliche-intelligenz-im-alltag-differenziert-diskutieren']
  },
  {
    slug: 'c1-datenschutz-und-komfort-abwaegen',
    topic: { de: 'Datenschutz und Komfort abwaegen', en: 'weighing data protection and convenience', fa: 'سنجیدن حریم خصوصی و راحتی', ar: 'الموازنة بين حماية البيانات والراحة', tr: 'veri koruması ile konforu tartma', ru: 'взвешивание защиты данных и удобства', ckb: 'هاوسەنگکردنی پاراستنی داتا و ئاسانی', kmr: 'hevsengkirina parastina daneyan û rehetî', pl: 'ważenie ochrony danych i wygody', ro: 'cântărirea protecției datelor și confortului', sq: 'peshimi i mbrojtjes së të dhënave dhe komoditetit' },
    focus: { de: 'persoenlichen Komfort, gesellschaftliche Folgen und Schutzbedarf fair gegeneinander abwaegen', en: 'fairly weighing personal convenience, social consequences, and need for protection', fa: 'راحتی شخصی، پیامد اجتماعی و نیاز به حفاظت را منصفانه کنار هم بسنجی', ar: 'الموازنة بعدل بين الراحة الشخصية والآثار الاجتماعية والحاجة إلى الحماية', tr: 'kişisel konforu, toplumsal sonuçları ve koruma ihtiyacını adil tartmayı', ru: 'справедливо сопоставлять личное удобство, общественные последствия и потребность в защите', ckb: 'ئاسانی کەسی، دەرئەنجامی کۆمەڵایەتی و پێویستی پاراستن بە دادپەروەری هەڵبسەنگێنیت', kmr: 'rehetiya kesî, encamên civakî û pêdiviya parastinê bi dadperwerî hevseng kirin', pl: 'uczciwe ważenie osobistej wygody, skutków społecznych i potrzeby ochrony', ro: 'cântărirea corectă a confortului personal, consecințelor sociale și nevoii de protecție', sq: 'peshimin e drejtë të komoditetit personal, pasojave shoqërore dhe nevojës për mbrojtje' },
    grammar: ['c1-concession-structures', 'Konzession und Abwaegung'],
    target: ['roleplay', 'c1-datenschutz-und-komfort-gegeneinander-abwaegen']
  },
  {
    slug: 'c1-klimaschutz-und-soziale-folgen',
    topic: { de: 'Klimaschutz und soziale Folgen', en: 'climate protection and social consequences', fa: 'حفاظت از اقلیم و پیامدهای اجتماعی', ar: 'حماية المناخ والآثار الاجتماعية', tr: 'iklim koruması ve sosyal sonuçlar', ru: 'защита климата и социальные последствия', ckb: 'پاراستنی کەشوهەوا و دەرئەنجامە کۆمەڵایەتییەکان', kmr: 'parastina avhewayê û encamên civakî', pl: 'ochrona klimatu i skutki społeczne', ro: 'protecția climei și consecințele sociale', sq: 'mbrojtja e klimës dhe pasojat sociale' },
    focus: { de: 'oekologische Notwendigkeit und soziale Belastung zusammen denken, statt sie gegeneinander auszuspielen', en: 'thinking ecological necessity and social burden together instead of playing them against each other', fa: 'ضرورت محیط‌زیستی و فشار اجتماعی را با هم ببینی، نه اینکه آن‌ها را مقابل هم قرار بدهی', ar: 'التفكير في الضرورة البيئية والعبء الاجتماعي معًا بدل وضعهما ضد بعضهما', tr: 'ekolojik gereklilik ile sosyal yükü birbirine karşı koymadan birlikte düşünmeyi', ru: 'мыслить экологическую необходимость и социальную нагрузку вместе, а не противопоставлять их', ckb: 'پێویستی ژینگەیی و باری کۆمەڵایەتی پێکەوە ببینیت، نەک یەکیان دژ بە ئەوی تر دابنێیت', kmr: 'pêdiviya ekolojîk û barê civakî bi hev re dîtin, ne ku wan li dijî hev bidî', pl: 'myślenie razem o konieczności ekologicznej i obciążeniu społecznym zamiast przeciwstawiania ich sobie', ro: 'gândirea împreună a necesității ecologice și poverii sociale, nu opunerea lor', sq: 'ta mendosh së bashku domosdoshmërinë ekologjike dhe barrën sociale, jo t’i vësh kundër njëra-tjetrës' },
    grammar: ['c1-academic-argument-grammar', 'Argumentationsgrammatik'],
    target: ['roleplay', 'c1-ueber-klimaschutz-und-soziale-folgen-argumentieren']
  },
  {
    slug: 'c1-integration-und-teilhabe-nuanciert-diskutieren',
    topic: { de: 'Integration und Teilhabe nuanciert diskutieren', en: 'discussing integration and participation in a nuanced way', fa: 'بحث دقیق درباره ادغام و مشارکت اجتماعی', ar: 'مناقشة الاندماج والمشاركة بدقة', tr: 'entegrasyon ve katılımı nüanslı tartışma', ru: 'нюансированное обсуждение интеграции и участия', ckb: 'مشتومڕی ورد لەسەر تێکەڵبوون و بەشداری', kmr: 'nîqaşa bi nûans li ser entegrasyon û beşdarî', pl: 'niuansowana dyskusja o integracji i uczestnictwie', ro: 'discutarea nuanțată a integrării și participării', sq: 'diskutim i nuancuar për integrimin dhe pjesëmarrjen' },
    focus: { de: 'Anpassung, Rechte, Verantwortung und Ausschlussrisiken differenziert verbinden', en: 'linking adaptation, rights, responsibility, and risks of exclusion in a differentiated way', fa: 'سازگاری، حقوق، مسئولیت و خطر طردشدن را دقیق به هم وصل کنی', ar: 'ربط التكيّف والحقوق والمسؤولية ومخاطر الإقصاء بدقة', tr: 'uyum, haklar, sorumluluk ve dışlanma risklerini ayrıntılı bağlamayı', ru: 'дифференцированно связывать адаптацию, права, ответственность и риски исключения', ckb: 'خۆگونجاندن، ماف، بەرپرسیاریەتی و مەترسی دوورخستنەوە بە وردی پێکەوە ببەستیت', kmr: 'lihevhatin, maf, berpirsiyarî û rîska derxistinê bi nûans girêdan', pl: 'różnicowane łączenie adaptacji, praw, odpowiedzialności i ryzyka wykluczenia', ro: 'legarea nuanțată a adaptării, drepturilor, responsabilității și riscurilor de excludere', sq: 'lidhjen e nuancuar të përshtatjes, të drejtave, përgjegjësisë dhe rrezikut të përjashtimit' },
    grammar: ['c1-expressing-nuance-and-limitation', 'Nuancierung und Begrenzung'],
    target: ['roleplay', 'c1-integration-und-teilhabe-nuanciert-diskutieren']
  },
  {
    slug: 'c1-generationenkonflikte-einordnen',
    topic: { de: 'Generationenkonflikte einordnen', en: 'contextualizing generational conflicts', fa: 'جای‌دادن اختلاف نسل‌ها در یک زمینه روشن', ar: 'وضع صراعات الأجيال في سياق واضح', tr: 'kuşak çatışmalarını bağlama yerleştirme', ru: 'контекстуализация конфликтов поколений', ckb: 'دانانی ناکۆکیی نەوەکان لە چوارچێوەیەکی ڕووندا', kmr: 'danîna nakokiyên nifşan di çarçoveyekê de', pl: 'osadzanie konfliktów pokoleniowych w kontekście', ro: 'încadrarea conflictelor între generații', sq: 'vendosja në kontekst e konflikteve mes brezave' },
    focus: { de: 'Erfahrung, Wertewandel und konkrete Situation trennen, ohne eine Generation pauschal zu bewerten', en: 'separating experience, value change, and concrete situation without judging a whole generation', fa: 'تجربه، تغییر ارزش‌ها و موقعیت مشخص را جدا کنی، بدون قضاوت کلی درباره یک نسل', ar: 'فصل الخبرة وتغير القيم والموقف المحدد من دون الحكم العام على جيل كامل', tr: 'deneyimi, değer değişimini ve somut durumu bir kuşağı genellemeden ayırmayı', ru: 'разделять опыт, изменение ценностей и конкретную ситуацию, не оценивая целое поколение обобщенно', ckb: 'ئەزموون، گۆڕانی بەهاکان و دۆخی دیاریکراو جیا بکەیتەوە، بەبێ داوەری گشتی لەسەر نەوەیەک', kmr: 'ezmûn, guhertina nirxan û rewşa konkret cuda kirin bê dadbariya giştî li ser nifşekê', pl: 'oddzielenie doświadczenia, zmiany wartości i konkretnej sytuacji bez oceniania całego pokolenia', ro: 'separarea experienței, schimbării valorilor și situației concrete fără judecarea unei generații întregi', sq: 'ndarjen e përvojës, ndryshimit të vlerave dhe situatës konkrete pa gjykuar një brez të tërë' },
    grammar: ['c1-register-shifting', 'Registerwechsel'],
    target: ['roleplay', 'c1-generationenkonflikte-im-alltag-einordnen']
  },
  {
    slug: 'c1-arbeitsmarkt-und-qualifizierung',
    topic: { de: 'Arbeitsmarkt und Qualifizierung', en: 'labor market and qualification', fa: 'بازار کار و مهارت‌آموزی', ar: 'سوق العمل والتأهيل', tr: 'iş piyasası ve nitelik kazanma', ru: 'рынок труда и квалификация', ckb: 'بازاڕی کار و بەهێزکردنی شارەزایی', kmr: 'bazara kar û pisporî', pl: 'rynek pracy i kwalifikacje', ro: 'piața muncii și calificarea', sq: 'tregu i punës dhe kualifikimi' },
    focus: { de: 'individuelle Verantwortung, strukturelle Huerden und realistische Weiterbildung zusammendenken', en: 'thinking individual responsibility, structural barriers, and realistic training together', fa: 'مسئولیت فردی، مانع‌های ساختاری و آموزش واقع‌بینانه را با هم ببینی', ar: 'التفكير في المسؤولية الفردية والعوائق البنيوية والتدريب الواقعي معًا', tr: 'bireysel sorumluluk, yapısal engeller ve gerçekçi eğitimi birlikte düşünmeyi', ru: 'совместно рассматривать личную ответственность, структурные барьеры и реалистичное обучение', ckb: 'بەرپرسیاریەتی تاکەکەسی، ئاستەنگە پێکهاتەییەکان و فێربوونی ڕاستبینانە پێکەوە ببینیت', kmr: 'berpirsiyariya kesane, astengên avahî û perwerdeya rastîn bi hev re dîtin', pl: 'wspólne myślenie o odpowiedzialności jednostki, barierach strukturalnych i realistycznym dokształcaniu', ro: 'gândirea împreună a responsabilității individuale, barierelor structurale și formării realiste', sq: 'ta mendosh së bashku përgjegjësinë individuale, pengesat strukturore dhe trajnimin realist' },
    grammar: ['c1-concession-structures', 'Konzession und Abwaegung'],
    target: ['roleplay', 'c1-arbeitsmarkt-und-qualifizierung-diskutieren']
  },
  {
    slug: 'c1-ethische-grenzfrage-diskutieren',
    topic: { de: 'eine ethische Grenzfrage diskutieren', en: 'discussing an ethical boundary question', fa: 'بحث درباره یک مرز اخلاقی دشوار', ar: 'مناقشة مسألة حدّية أخلاقية', tr: 'etik bir sınır sorusunu tartışma', ru: 'обсуждение этического пограничного вопроса', ckb: 'مشتومڕ لەسەر پرسی سنووری ئەخلاقی', kmr: 'nîqaş li ser pirseke sînorî ya exlaqî', pl: 'dyskusja o etycznej kwestii granicznej', ro: 'discutarea unei chestiuni etice de limită', sq: 'diskutimi i një çështjeje kufitare etike' },
    focus: { de: 'moralische Intuition, Folgen und Prinzipien trennen, ohne andere Positionen abzuwerten', en: 'separating moral intuition, consequences, and principles without devaluing other positions', fa: 'حس اخلاقی، پیامدها و اصل‌ها را جدا کنی، بدون بی‌ارزش کردن موضع دیگران', ar: 'فصل الحدس الأخلاقي والنتائج والمبادئ من دون التقليل من المواقف الأخرى', tr: 'ahlaki sezgi, sonuçlar ve ilkeleri başka tutumları küçümsemeden ayırmayı', ru: 'разделять моральную интуицию, последствия и принципы, не обесценивая другие позиции', ckb: 'هەستی ئەخلاقی، دەرئەنجام و بنەماکان جیا بکەیتەوە، بەبێ بێنرخکردنی هەڵوێستی کەسانی تر', kmr: 'hesta exlaqî, encam û prensîban cuda kirin bê kêmnîrxkirina helwestên din', pl: 'oddzielenie intuicji moralnej, skutków i zasad bez umniejszania innym stanowiskom', ro: 'separarea intuiției morale, consecințelor și principiilor fără a devaloriza alte poziții', sq: 'ndarjen e intuitës morale, pasojave dhe parimeve pa zhvlerësuar qëndrimet e tjera' },
    grammar: ['c1-expressing-nuance-and-limitation', 'Nuancierung und Begrenzung'],
    target: ['roleplay', 'c1-eine-ethische-grenzfrage-diskutieren']
  },
  {
    slug: 'c1-politisches-thema-ausgewogen-diskutieren',
    topic: { de: 'ein politisches Thema ausgewogen diskutieren', en: 'discussing a political topic in a balanced way', fa: 'بحث متعادل درباره یک موضوع سیاسی', ar: 'مناقشة موضوع سياسي بتوازن', tr: 'siyasi bir konuyu dengeli tartışma', ru: 'сбалансированное обсуждение политической темы', ckb: 'مشتومڕی هاوسەنگ لەسەر بابەتێکی سیاسی', kmr: 'nîqaşa hevseng li ser mijareke siyasî', pl: 'wyważona dyskusja o temacie politycznym', ro: 'discutarea echilibrată a unei teme politice', sq: 'diskutim i balancuar për një temë politike' },
    focus: { de: 'Position, Gegenposition und gemeinsame Grundlage sichtbar halten', en: 'keeping position, counterposition, and common ground visible', fa: 'موضع، موضع مخالف و زمینه مشترک را همزمان قابل دید نگه داری', ar: 'إبقاء الموقف والموقف المقابل والأساس المشترك واضحين', tr: 'tutumu, karşı tutumu ve ortak zemini görünür tutmayı', ru: 'удерживать видимыми позицию, противопозицию и общую основу', ckb: 'هەڵوێست، هەڵوێستی بەرامبەر و بنەمای هاوبەش بە دیاری بهێڵیتەوە', kmr: 'helwest, dijhelwest û bingeha hevpar xuya hiştin', pl: 'utrzymanie widoczności stanowiska, kontrstanowiska i wspólnej podstawy', ro: 'menținerea vizibilă a poziției, contra-poziției și bazei comune', sq: 'mbajtjen të dukshme të qëndrimit, kundërqëndrimit dhe bazës së përbashkët' },
    grammar: ['c1-academic-argument-grammar', 'Argumentationsgrammatik'],
    target: ['roleplay', 'c1-ein-politisches-thema-ausgewogen-diskutieren']
  },
  {
    slug: 'c1-gesellschaft-medien-und-ethik-wiederholen',
    topic: { de: 'Gesellschaft, Medien und Ethik wiederholen', en: 'reviewing society, media, and ethics', fa: 'مرور جامعه، رسانه و اخلاق', ar: 'مراجعة المجتمع والإعلام والأخلاق', tr: 'toplum, medya ve etiği tekrar etme', ru: 'повторение общества, СМИ и этики', ckb: 'دووبارەکردنەوەی کۆمەڵگا، میدیا و ئەخلاق', kmr: 'dubarekirina civak, medya û exlaqê', pl: 'powtórka społeczeństwa, mediów i etyki', ro: 'recapitularea societății, media și eticii', sq: 'përsëritja e shoqërisë, medias dhe etikës' },
    focus: { de: 'komplexe gesellschaftliche Themen fair, belegt und ohne vorschnelle Urteile diskutieren', en: 'discussing complex social topics fairly, with evidence, and without rushed judgments', fa: 'موضوع‌های پیچیده اجتماعی را منصفانه، با تکیه بر شواهد و بدون قضاوت عجولانه بحث کنی', ar: 'مناقشة الموضوعات الاجتماعية المعقدة بإنصاف وبالأدلة ومن دون أحكام متسرعة', tr: 'karmaşık toplumsal konuları adil, kanıta dayalı ve acele yargısız tartışmayı', ru: 'обсуждать сложные общественные темы справедливо, с опорой на доказательства и без поспешных суждений', ckb: 'بابەتە ئاڵۆزە کۆمەڵایەتییەکان بە دادپەروەری، بە پشتبەستن بە بەڵگە و بەبێ داوەری پەلەیی باس بکەیت', kmr: 'mijarên civakî yên tevlihev bi dadperwerî, bi delîlan û bê dadbariya lezgîn nîqaş kirin', pl: 'omawianie złożonych tematów społecznych uczciwie, z dowodami i bez pochopnych ocen', ro: 'discutarea temelor sociale complexe corect, cu dovezi și fără judecăți pripite', sq: 'diskutimin e temave të ndërlikuara shoqërore drejt, me prova dhe pa gjykime të nxituara' },
    grammar: ['c1-c1-register-review', 'Register-Review'],
    target: ['exam-prep-unit', 'c1-kontroverse-position-differenziert-vertreten']
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
    de: `Lies den Lektionstext zu ${item.topic.de} und notiere: Welche Positionen gibt es, welche Belege fehlen, und wo liegt die eigentliche Spannung?`,
    en: `Read the lesson text on ${item.topic.en} and note which positions exist, which evidence is missing, and where the real tension lies.`,
    fa: `متن درس درباره ${item.topic.fa} را بخوان و یادداشت کن: چه موضع‌هایی وجود دارد، چه شواهدی کم است و تنش اصلی کجاست.`,
    ar: `اقرأ نص الدرس عن ${item.topic.ar} وسجّل: ما المواقف الموجودة، ما الأدلة الناقصة، وأين يقع التوتر الأساسي.`,
    tr: `${item.topic.tr} konulu ders metnini oku ve not et: hangi tutumlar var, hangi kanıtlar eksik, asıl gerilim nerede.`,
    ru: `Прочитай текст урока о теме «${item.topic.ru}» и отметь, какие позиции есть, каких доказательств не хватает и где настоящее напряжение.`,
    ckb: `دەقی وانەکە لەسەر ${item.topic.ckb} بخوێنەوە و بنووسە: کام هەڵوێستان هەن، کام بەڵگە کەمە و گرژیی سەرەکی لە کوێیە.`,
    kmr: `Nivîsa dersê li ser ${item.topic.kmr} bixwîne û binivîse: kîjan helwest hene, kîjan delîl kêm in û tengaviya sereke li ku ye.`,
    pl: `Przeczytaj tekst lekcji o temacie: ${item.topic.pl} i zanotuj, jakie są stanowiska, jakich dowodów brakuje i gdzie leży właściwe napięcie.`,
    ro: `Citește textul lecției despre ${item.topic.ro} și notează ce poziții există, ce dovezi lipsesc și unde este tensiunea reală.`,
    sq: `Lexo tekstin e mësimit për ${item.topic.sq} dhe shëno cilat qëndrime ekzistojnë, cilat prova mungojnë dhe ku është tensioni kryesor.`
  };
}

function languageInstruction(item) {
  return {
    de: `Oeffne ${item.grammar[1]} und sammle drei Formulierungen, mit denen du ${item.focus.de}.`,
    en: `Open the linked language section and collect three formulations that help you with this skill: ${item.focus.en}.`,
    fa: `بخش زبانی لینک‌شده را باز کن و سه عبارت پیدا کن که به این مهارت کمک کند: ${item.focus.fa}.`,
    ar: `افتح القسم اللغوي المرتبط واجمع ثلاث صيغ تساعد على هذه المهارة: ${item.focus.ar}.`,
    tr: `Bağlantılı dil bölümünü aç ve şu beceriye yardım eden üç ifade topla: ${item.focus.tr}.`,
    ru: `Открой связанный языковой раздел и собери три формулировки для этого умения: ${item.focus.ru}.`,
    ckb: `بەشی زمانی بەستەرکراو بکەرەوە و سێ دەربڕین کۆبکەوە کە یارمەتی ئەم توانایە بدات: ${item.focus.ckb}.`,
    kmr: `Beşa zimanê ya girêdayî veke û sê derbirînan kom bike ku alîkarîya vê jêhatinê bikin: ${item.focus.kmr}.`,
    pl: `Otwórz połączoną sekcję językową i zbierz trzy sformułowania wspierające tę umiejętność: ${item.focus.pl}.`,
    ro: `Deschide secțiunea lingvistică legată și adună trei formulări care susțin această abilitate: ${item.focus.ro}.`,
    sq: `Hap seksionin e lidhur gjuhësor dhe mblidh tri formulime që ndihmojnë këtë aftësi: ${item.focus.sq}.`
  };
}

function materialInstruction() {
  return {
    de: 'Bearbeite das verlinkte Debattentraining und achte darauf, wie Positionen begruendet werden, ohne andere Stimmen abzuwerten.',
    en: 'Work through the linked debate practice and notice how positions are justified without devaluing other voices.',
    fa: 'تمرین بحث لینک‌شده را انجام بده و دقت کن چطور موضع‌ها دلیل‌دار می‌شوند، بدون اینکه صدای دیگران بی‌ارزش شود.',
    ar: 'اعمل على تدريب النقاش المرتبط وانتبه إلى كيفية تبرير المواقف من دون التقليل من الأصوات الأخرى.',
    tr: 'Bağlantılı tartışma alıştırmasını çalış ve başka sesleri değersizleştirmeden tutumların nasıl gerekçelendirildiğine dikkat et.',
    ru: 'Проработай связанное упражнение по дебатам и обрати внимание, как позиции обосновываются без обесценивания других голосов.',
    ckb: 'ڕاهێنانی مشتومڕی بەستەرکراو کاربکە و سەرنج بدە چۆن هەڵوێستەکان هۆکاربەدار دەبن، بەبێ بێنرخکردنی دەنگی کەسانی تر.',
    kmr: 'Rahênana nîqaşê ya girêdayî bixebite û bala xwe bide ka helwest çawa bi sedem dibin bê kêmnîrxkirina dengên din.',
    pl: 'Przerób połączone ćwiczenie debaty i zwróć uwagę, jak uzasadniać stanowiska bez umniejszania innym głosom.',
    ro: 'Lucrează cu exercițiul de dezbatere legat și observă cum sunt justificate pozițiile fără a devaloriza alte voci.',
    sq: 'Puno me ushtrimin e lidhur të debatit dhe vëzhgo si arsyetohen qëndrimet pa zhvlerësuar zërat e tjerë.'
  };
}

function applyInstruction() {
  return {
    de: 'Formuliere einen ausgewogenen Beitrag in vier Teilen: Position, Gegenperspektive, Beleg oder Beispiel, vorsichtige Schlussfolgerung.',
    en: 'Formulate a balanced contribution in four parts: position, counter-perspective, evidence or example, cautious conclusion.',
    fa: 'یک مشارکت متعادل در چهار بخش بنویس: موضع، نگاه مخالف، شاهد یا مثال، نتیجه‌گیری محتاطانه.',
    ar: 'صغ مساهمة متوازنة في أربعة أجزاء: موقف، منظور مقابل، دليل أو مثال، استنتاج حذر.',
    tr: 'Dört parçalı dengeli bir katkı formüle et: tutum, karşı bakış, kanıt veya örnek, temkinli sonuç.',
    ru: 'Сформулируй сбалансированный вклад из четырех частей: позиция, противоположный взгляд, доказательство или пример, осторожный вывод.',
    ckb: 'بەشدارییەکی هاوسەنگ بە چوار بەش بنووسە: هەڵوێست، دیدی بەرامبەر، بەڵگە یان نموونە، دەرئەنجامی وریا.',
    kmr: 'Beşdariyeke hevseng bi çar beşan formule bike: helwest, nêrîna dijber, delîl an mînak, encameke hişyar.',
    pl: 'Sformułuj wyważoną wypowiedź w czterech częściach: stanowisko, perspektywa przeciwna, dowód lub przykład, ostrożny wniosek.',
    ro: 'Formulează o contribuție echilibrată în patru părți: poziție, perspectivă opusă, dovadă sau exemplu, concluzie prudentă.',
    sq: 'Formulo një kontribut të balancuar në katër pjesë: qëndrim, këndvështrim kundër, provë ose shembull, përfundim i kujdesshëm.'
  };
}

function reviewInstruction() {
  return {
    de: 'Pruefe, ob dein Beitrag fair bleibt, eine Gegenposition ernst nimmt und trotzdem eine erkennbare eigene Linie hat.',
    en: 'Check whether your contribution stays fair, takes a counterposition seriously, and still has a recognizable line of its own.',
    fa: 'بررسی کن که مشارکت تو منصفانه می‌ماند، موضع مخالف را جدی می‌گیرد و با این حال خط فکری خودت را دارد.',
    ar: 'راجع ما إذا كانت مساهمتك منصفة، تأخذ الموقف المقابل بجدية، ومع ذلك لها خط فكري واضح.',
    tr: 'Katkının adil kaldığını, karşı tutumu ciddiye aldığını ve yine de kendi çizgisinin belli olduğunu kontrol et.',
    ru: 'Проверь, остается ли твой вклад справедливым, серьезно ли учитывает противопозицию и сохраняет ли собственную линию.',
    ckb: 'بپشکنە بەشدارییەکەت دادپەروەرانە دەمێنێتەوە، هەڵوێستی بەرامبەر بە جدی وەردەگرێت و هێشتا ڕێڕەوی خۆتی تێدایە.',
    kmr: 'Kontrol bike ka beşdariya te dadperwer dimîne, dijhelwestê bi ciddî digire û dîsa rêça xwe ya taybet heye.',
    pl: 'Sprawdź, czy wypowiedź pozostaje uczciwa, poważnie traktuje kontrstanowisko i nadal ma własną wyraźną linię.',
    ro: 'Verifică dacă intervenția rămâne corectă, ia în serios poziția opusă și totuși are o linie proprie clară.',
    sq: 'Kontrollo nëse kontributi yt mbetet i drejtë, merr seriozisht kundërqëndrimin dhe prapë ka vijën tënde të qartë.'
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
    block('practice', 'apply', applyInstruction(item), 'none', null, 8, 40, true),
    block('review', 'review', reviewInstruction(item), 'none', null, 6, 50, true)
  ];
}

fs.writeFileSync(file, JSON.stringify(data, null, 2) + '\n', 'utf8');
console.log(`Updated ${items.length} C1 Module 9 lessons with ${items.length * 5} activity blocks.`);

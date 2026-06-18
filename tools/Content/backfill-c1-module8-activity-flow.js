const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c1-souveraen-in-studium-beruf-und-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'Falllage ordnen',
    en: 'Organize the case situation',
    fa: 'وضعیت پرونده را مرتب کن',
    ar: 'رتّب وضع الحالة',
    tr: 'Dosya durumunu düzenle',
    ru: 'Упорядочь ситуацию по делу',
    ckb: 'دۆخی پەروەندەکە ڕێکبخە',
    kmr: 'Rewşa dosyayê rêz bike',
    pl: 'Uporządkuj sytuację sprawy',
    ro: 'Ordonează situația cazului',
    sq: 'Rendit situatën e rastit'
  },
  language: {
    de: 'Formelle Sprache sichern',
    en: 'Secure formal language',
    fa: 'زبان رسمی را کنترل کن',
    ar: 'اضبط اللغة الرسمية',
    tr: 'Resmî dili güvenceye al',
    ru: 'Обеспечь официальный стиль',
    ckb: 'زمانی فەرمی بپارێزە',
    kmr: 'Zimana fermî biparêze',
    pl: 'Zabezpiecz język formalny',
    ro: 'Asigură limbajul formal',
    sq: 'Siguro gjuhën formale'
  },
  material: {
    de: 'Fall praktisch trainieren',
    en: 'Practise the case practically',
    fa: 'پرونده را عملی تمرین کن',
    ar: 'تدرّب عمليًا على الحالة',
    tr: 'Vakayı pratikte çalış',
    ru: 'Практически отработай случай',
    ckb: 'پەروەندەکە بە کرداری ڕاهێنان بکە',
    kmr: 'Dosyayê bi pratîkî perwerde bike',
    pl: 'Przećwicz sprawę praktycznie',
    ro: 'Exersează cazul practic',
    sq: 'Ushtro rastin praktikisht'
  },
  apply: {
    de: 'Naechsten Schritt formulieren',
    en: 'Formulate the next step',
    fa: 'قدم بعدی را دقیق بیان کن',
    ar: 'صغ الخطوة التالية',
    tr: 'Sonraki adımı formüle et',
    ru: 'Сформулируй следующий шаг',
    ckb: 'هەنگاوی داهاتوو داڕێژە',
    kmr: 'Gava din formule bike',
    pl: 'Sformułuj następny krok',
    ro: 'Formulează pasul următor',
    sq: 'Formulo hapin tjetër'
  },
  review: {
    de: 'Sachlichkeit pruefen',
    en: 'Check factual tone',
    fa: 'عینی بودن لحن را بررسی کن',
    ar: 'راجع موضوعية النبرة',
    tr: 'Nesnel tonu kontrol et',
    ru: 'Проверь предметный тон',
    ckb: 'بابەتیبوونی تۆنەکە بپشکنە',
    kmr: 'Tona babetî kontrol bike',
    pl: 'Sprawdź rzeczowy ton',
    ro: 'Verifică tonul factual',
    sq: 'Kontrollo tonin faktik'
  }
};

const items = [
  {
    slug: 'c1-komplexen-behoerdenfall-strukturieren',
    topic: { de: 'einen komplexen Behoerdenfall strukturieren', en: 'structuring a complex administrative case', fa: 'ساختار دادن به یک پرونده اداری پیچیده', ar: 'تنظيم حالة إدارية معقدة', tr: 'karmaşık bir resmî daire dosyasını yapılandırma', ru: 'структурирование сложного административного случая', ckb: 'ڕێکخستنی پەروەندەیەکی ئاڵۆزی فەرمی', kmr: 'rêxistina dosyayeke fermî ya tevlihev', pl: 'strukturyzowanie złożonej sprawy urzędowej', ro: 'structurarea unui caz administrativ complex', sq: 'strukturimi i një rasti administrativ kompleks' },
    focus: { de: 'Sachverhalt, Nachweise, offene Fragen und Fristen getrennt darstellen', en: 'presenting facts, evidence, open questions, and deadlines separately', fa: 'واقعیت‌ها، مدارک، پرسش‌های باز و مهلت‌ها را جداگانه نشان بدهی', ar: 'عرض الوقائع والأدلة والأسئلة المفتوحة والمواعيد كلٌ على حدة', tr: 'olayları, belgeleri, açık soruları ve süreleri ayrı ayrı sunmayı', ru: 'отдельно представлять факты, доказательства, открытые вопросы и сроки', ckb: 'ڕاستییەکان، بەڵگەکان، پرسیارە کراوەکان و ماوەکان جیا جیا پێشکەش بکەیت', kmr: 'rastî, delîl, pirsên vekirî û deman cuda cuda nîşan dan', pl: 'oddzielne przedstawienie faktów, dowodów, pytań otwartych i terminów', ro: 'prezentarea separată a faptelor, dovezilor, întrebărilor deschise și termenelor', sq: 'paraqitjen veçmas të fakteve, provave, pyetjeve të hapura dhe afateve' },
    grammar: ['c1-formal-summaries', 'formelle Zusammenfassung'],
    target: ['roleplay', 'c1-einen-komplexen-behoerdenfall-strukturieren']
  },
  {
    slug: 'c1-amtliche-entscheidung-sachlich-anzweifeln',
    topic: { de: 'eine amtliche Entscheidung sachlich anzweifeln', en: 'questioning an official decision factually', fa: 'زیر سؤال بردن محترمانه و عینی یک تصمیم اداری', ar: 'التشكيك الموضوعي في قرار رسمي', tr: 'resmî bir kararı nesnel biçimde sorgulama', ru: 'предметное оспаривание официального решения', ckb: 'گومانکردنی بابەتی لە بڕیارێکی فەرمی', kmr: 'bi awayekî babetî pirsîna biryareke fermî', pl: 'rzeczowe zakwestionowanie decyzji urzędowej', ro: 'contestarea factuală a unei decizii oficiale', sq: 'vënia në dyshim faktike e një vendimi zyrtar' },
    focus: { de: 'Begruendung erfragen und eigene Zweifel mit Belegen statt Vorwurf formulieren', en: 'asking for reasons and formulating doubts with evidence rather than accusation', fa: 'دلیل بخواهی و تردید خودت را با مدرک بیان کنی، نه با اتهام', ar: 'طلب التبرير وصياغة الشك بالأدلة لا بالاتهام', tr: 'gerekçe istemeyi ve şüpheyi suçlama yerine kanıtla ifade etmeyi', ru: 'запрашивать обоснование и формулировать сомнение доказательствами, а не обвинением', ckb: 'داوای هۆکار بکەیت و گومانەکەت بە بەڵگە بڵێیت، نەک بە تۆمەت', kmr: 'sedem xwestin û gumanê xwe bi delîlan gotin, ne bi sûcdarkirin', pl: 'poproszenie o uzasadnienie i wyrażenie wątpliwości dowodami, nie zarzutem', ro: 'cererea justificării și formularea îndoielii cu dovezi, nu ca acuzație', sq: 'kërkimin e arsyetimit dhe shprehjen e dyshimit me prova, jo me akuzë' },
    grammar: ['c1-hedging-and-cautious-language', 'vorsichtige Sprache'],
    target: ['roleplay', 'c1-eine-amtliche-entscheidung-sachlich-anzweifeln']
  },
  {
    slug: 'c1-ausnahmeregelung-beantragen',
    topic: { de: 'eine Ausnahmeregelung beantragen', en: 'requesting an exception', fa: 'درخواست یک استثنا یا مقررات ویژه', ar: 'طلب استثناء أو تنظيم خاص', tr: 'istisna düzenlemesi talep etme', ru: 'запрос исключения из правила', ckb: 'داواکردنی ڕێکخستنی تایبەت', kmr: 'daxwaza rêkûpêkeke taybet', pl: 'wnioskowanie o wyjątek', ro: 'solicitarea unei excepții', sq: 'kërkimi i një përjashtimi' },
    focus: { de: 'besondere Umstaende konkret erklaeren, ohne einen Anspruch zu behaupten', en: 'explaining special circumstances concretely without claiming entitlement', fa: 'شرایط خاص را مشخص توضیح بدهی، بدون اینکه حق قطعی برای خودت فرض کنی', ar: 'شرح الظروف الخاصة بوضوح من دون ادعاء حق مؤكد', tr: 'özel koşulları somut açıklayıp kesin hak iddia etmemeyi', ru: 'конкретно объяснять особые обстоятельства, не утверждая автоматического права', ckb: 'بارودۆخی تایبەت بە دیاریکراوی ڕوون بکەیتەوە، بەبێ ئەوەی مافێکی دڵنیای خۆت دابنێیت', kmr: 'rewşên taybet bi konkretî vegotin bêyî ku mafekî teqez îdia bikî', pl: 'konkretne wyjaśnienie szczególnych okoliczności bez twierdzenia o pewnym prawie', ro: 'explicarea concretă a circumstanțelor speciale fără a pretinde un drept sigur', sq: 'shpjegimin konkret të rrethanave të veçanta pa pretenduar një të drejtë të sigurt' },
    grammar: ['c1-konjunktiv-i-versus-konjunktiv-ii', 'hoefliche Distanz'],
    target: ['roleplay', 'c1-eine-ausnahmeregelung-beantragen']
  },
  {
    slug: 'c1-fristversaeumnis-differenziert-begruenden',
    topic: { de: 'eine Fristversaeumnis differenziert begruenden', en: 'explaining a missed deadline in a differentiated way', fa: 'توضیح دقیق و متعادل درباره از دست رفتن یک مهلت', ar: 'تفسير فوات موعد نهائي بشكل متوازن', tr: 'kaçırılan süreyi ayrıntılı ve dengeli gerekçelendirme', ru: 'дифференцированное объяснение пропущенного срока', ckb: 'ڕوونکردنەوەی وردی لەدەستدانی ماوەیەک', kmr: 'vegotina cuda û zelal a derengmayîna demekê', pl: 'zróżnicowane uzasadnienie przekroczenia terminu', ro: 'justificarea nuanțată a ratării unui termen', sq: 'arsyetimi i nuancuar i humbjes së një afati' },
    focus: { de: 'Verantwortung uebernehmen und entlastende Gruende nachvollziehbar, aber nicht entschuldigend ordnen', en: 'taking responsibility while organizing mitigating reasons clearly but not evasively', fa: 'مسئولیت را بپذیری و دلیل‌های قابل توضیح را روشن مرتب کنی، بدون فرار از مسئولیت', ar: 'تحمّل المسؤولية مع ترتيب الأسباب المخففة بوضوح من دون تهرّب', tr: 'sorumluluğu alıp hafifletici gerekçeleri kaçmadan anlaşılır düzenlemeyi', ru: 'принять ответственность и ясно упорядочить смягчающие причины без ухода от ответственности', ckb: 'بەرپرسیاریەتی وەربگریت و هۆکارە ڕوونکەرەوەکان ڕێکبخەیت، بەبێ هەڵهاتن لە بەرپرسیاریەتی', kmr: 'berpirsiyarî wergirtin û sedemên hêsanker bi zelalî rêz kirin bê revîn ji berpirsiyariyê', pl: 'wzięcie odpowiedzialności i jasne uporządkowanie okoliczności łagodzących bez unikania odpowiedzialności', ro: 'asumarea responsabilității și ordonarea clară a motivelor atenuante fără evitare', sq: 'marrjen e përgjegjësisë dhe renditjen qartë të arsyeve lehtësuese pa iu shmangur përgjegjësisë' },
    grammar: ['c1-causal-chains-in-formal-writing', 'Kausalketten'],
    target: ['roleplay', 'c1-eine-fristversaeumnis-differenziert-begruenden']
  },
  {
    slug: 'c1-rechtslage-und-einzelfall-abwaegen',
    topic: { de: 'Rechtslage und Einzelfall abwaegen', en: 'weighing legal situation and individual case', fa: 'سنجیدن قاعده کلی و وضعیت خاص پرونده', ar: 'الموازنة بين الوضع القانوني والحالة الفردية', tr: 'hukuki durum ile tekil vakayı tartma', ru: 'взвешивание правовой ситуации и индивидуального случая', ckb: 'هاوسەنگکردنی دۆخی یاسایی و کەیسی تاکەکەسی', kmr: 'hevsengkirina rewşa hiqûqî û rewşa taybet', pl: 'ważenie sytuacji prawnej i indywidualnego przypadku', ro: 'cântărirea situației juridice și a cazului individual', sq: 'peshimi i situatës ligjore dhe rastit individual' },
    focus: { de: 'allgemeine Regel, Besonderheit und Bitte um Pruefung klar voneinander trennen', en: 'separating general rule, special circumstance, and request for review clearly', fa: 'قاعده کلی، ویژگی خاص پرونده و درخواست بررسی را روشن از هم جدا کنی', ar: 'الفصل بوضوح بين القاعدة العامة والخصوصية وطلب المراجعة', tr: 'genel kuralı, özel durumu ve inceleme talebini net ayırmayı', ru: 'четко разделять общее правило, особенность случая и просьбу о проверке', ckb: 'یاسای گشتی، تایبەتمەندییەکە و داوای پشکنین بە ڕوونی جیا بکەیتەوە', kmr: 'qaîdeya giştî, taybetmendî û daxwaza kontrolê bi zelalî cuda kirin', pl: 'jasne oddzielenie zasady ogólnej, szczególnej okoliczności i prośby o sprawdzenie', ro: 'separarea clară a regulii generale, particularității și cererii de verificare', sq: 'ndarjen e qartë të rregullit të përgjithshëm, veçantisë dhe kërkesës për shqyrtim' },
    grammar: ['c1-expressing-nuance-and-limitation', 'Nuancierung und Begrenzung'],
    target: ['roleplay', 'c1-zwischen-rechtslage-und-einzelfall-abwaegen']
  },
  {
    slug: 'c1-integrationsfall-mit-mehreren-stellen-koordinieren',
    topic: { de: 'einen Integrationsfall mit mehreren Stellen koordinieren', en: 'coordinating an integration case with several offices', fa: 'هماهنگ کردن یک پرونده ادغام با چند نهاد', ar: 'تنسيق حالة اندماج مع عدة جهات', tr: 'bir entegrasyon vakasını birkaç kurumla koordine etme', ru: 'координация интеграционного случая с несколькими ведомствами', ckb: 'هەماهەنگکردنی کەیسی تێکەڵبوون لەگەڵ چەند دامەزراوە', kmr: 'koordînasyona rewşa entegrasyonê bi çend saziyan', pl: 'koordynacja sprawy integracyjnej z kilkoma instytucjami', ro: 'coordonarea unui caz de integrare cu mai multe instituții', sq: 'koordinimi i një rasti integrimi me disa institucione' },
    focus: { de: 'Zustaendigkeiten, Informationsstand und naechste Rueckmeldung transparent halten', en: 'keeping responsibilities, information status, and next update transparent', fa: 'مسئولیت هر نهاد، وضعیت اطلاعات و پیگیری بعدی را شفاف نگه داری', ar: 'إبقاء المسؤوليات وحالة المعلومات والتحديث التالي واضحة', tr: 'sorumlulukları, bilgi durumunu ve sonraki geri bildirimi şeffaf tutmayı', ru: 'сохранять прозрачность обязанностей, состояния информации и следующего ответа', ckb: 'بەرپرسیاریەتی هەر دامەزراوە، دۆخی زانیاری و پەیوەندی داهاتوو ڕوون بهێڵیتەوە', kmr: 'berpirsiyariya her saziyê, rewşa agahiyan û bersiva din zelal hiştin', pl: 'utrzymanie przejrzystości kompetencji, stanu informacji i kolejnej odpowiedzi', ro: 'menținerea clară a responsabilităților, stadiului informațiilor și următorului răspuns', sq: 'mbajtjen të qartë të përgjegjësive, gjendjes së informacionit dhe përgjigjes së radhës' },
    grammar: ['c1-formal-summaries', 'formelle Zusammenfassung'],
    target: ['roleplay', 'c1-einen-integrationsfall-mit-mehreren-stellen-koordinieren']
  },
  {
    slug: 'c1-schriftliche-stellungnahme-muendlich-erlaeutern',
    topic: { de: 'eine schriftliche Stellungnahme muendlich erlaeutern', en: 'explaining a written statement orally', fa: 'توضیح شفاهی یک موضع‌گیری کتبی', ar: 'شرح موقف مكتوب شفهيًا', tr: 'yazılı bir görüşü sözlü açıklama', ru: 'устное объяснение письменного заявления', ckb: 'ڕوونکردنەوەی زارەکیی ڕاگەیاندنێکی نووسراو', kmr: 'vegotina devkî ya daxuyaniyeke nivîskî', pl: 'ustne wyjaśnienie pisemnego stanowiska', ro: 'explicarea orală a unei poziții scrise', sq: 'shpjegimi me gojë i një qëndrimi të shkruar' },
    focus: { de: 'Kernaussage, Begruendung und Bitte muendlich kuerzen, ohne Genauigkeit zu verlieren', en: 'shortening main point, reason, and request orally without losing precision', fa: 'نکته اصلی، دلیل و درخواست را شفاهی کوتاه کنی، بدون اینکه دقت از بین برود', ar: 'اختصار الفكرة الأساسية والسبب والطلب شفهيًا من دون فقدان الدقة', tr: 'ana ifade, gerekçe ve ricayı sözlü olarak kısaltırken kesinliği kaybetmemeyi', ru: 'устно сокращать основной тезис, обоснование и просьбу без потери точности', ckb: 'خاڵی سەرەکی، هۆکار و داواکاری بە زارەکی کورت بکەیتەوە، بەبێ لەدەستدانی وردی', kmr: 'xala sereke, sedem û daxwazê bi devkî kurt kirin bê windakirina rastiyê', pl: 'ustne skrócenie głównej tezy, uzasadnienia i prośby bez utraty precyzji', ro: 'scurtarea orală a ideii principale, motivului și cererii fără pierderea preciziei', sq: 'shkurtimin me gojë të pikës kryesore, arsyes dhe kërkesës pa humbur saktësinë' },
    grammar: ['c1-nominal-style-versus-verbal-style', 'Nominalstil und Verbalstil'],
    target: ['roleplay', 'c1-eine-schriftliche-stellungnahme-muendlich-erlaeutern']
  },
  {
    slug: 'c1-komplexe-krankheitsgeschichte-schildern',
    topic: { de: 'eine komplexe Krankheitsgeschichte schildern', en: 'describing a complex medical history', fa: 'شرح دادن یک سابقه بیماری پیچیده', ar: 'وصف تاريخ مرضي معقد', tr: 'karmaşık bir hastalık geçmişini anlatma', ru: 'описание сложной истории болезни', ckb: 'باسکردنی مێژووی نەخۆشییەکی ئاڵۆز', kmr: 'vegotina dîroka nexweşiyeke tevlihev', pl: 'opisanie złożonej historii choroby', ro: 'descrierea unui istoric medical complex', sq: 'përshkrimi i një historie të ndërlikuar sëmundjeje' },
    focus: { de: 'Symptome, Verlauf, Unsicherheit und konkrete Bitte geordnet darstellen', en: 'presenting symptoms, development, uncertainty, and concrete request in order', fa: 'علائم، روند، ابهام‌ها و درخواست مشخص را منظم توضیح بدهی', ar: 'عرض الأعراض والتطور والشكوك والطلب المحدد بترتيب واضح', tr: 'belirtileri, süreci, belirsizlikleri ve somut ricayı düzenli anlatmayı', ru: 'упорядоченно описывать симптомы, ход, неопределенности и конкретную просьбу', ckb: 'نیشانەکان، ڕەوت، نادڵنیاییەکان و داواکاریی دیاریکراو بە ڕێکخراوی باس بکەیت', kmr: 'nîşan, pêvajoyê, nezelalî û daxwaza konkret bi rêz vegotin', pl: 'uporządkowane przedstawienie objawów, przebiegu, niepewności i konkretnej prośby', ro: 'prezentarea ordonată a simptomelor, evoluției, incertitudinii și cererii concrete', sq: 'paraqitjen me rregull të simptomave, ecurisë, paqartësive dhe kërkesës konkrete' },
    grammar: ['c1-formal-summaries', 'strukturierte Zusammenfassung'],
    target: ['roleplay', 'c1-eine-komplexe-krankheitsgeschichte-schildern']
  },
  {
    slug: 'c1-mietrechtliche-streitfrage-sachlich-besprechen',
    topic: { de: 'eine mietrechtliche Streitfrage sachlich besprechen', en: 'discussing a rental-law dispute factually', fa: 'گفت‌وگوی عینی درباره اختلاف مربوط به اجاره', ar: 'مناقشة خلاف متعلق بالإيجار بشكل موضوعي', tr: 'kira hukukuyla ilgili anlaşmazlığı nesnel konuşma', ru: 'предметное обсуждение спора по аренде', ckb: 'گفتوگۆی بابەتی لەسەر ناکۆکیی کرێ', kmr: 'axaftina babetî li ser nakokiya kirê', pl: 'rzeczowe omówienie sporu najmu', ro: 'discutarea factuală a unei dispute de închiriere', sq: 'diskutimi faktik i një mosmarrëveshjeje qiraje' },
    focus: { de: 'Forderung, Gegenargument und Loesungsoption sachlich voneinander trennen', en: 'separating claim, counterargument, and possible solution factually', fa: 'درخواست، دلیل طرف مقابل و گزینه حل را عینی از هم جدا کنی', ar: 'الفصل موضوعيًا بين المطلب والحجة المقابلة وخيار الحل', tr: 'talebi, karşı argümanı ve çözüm seçeneğini nesnel ayırmayı', ru: 'предметно разделять требование, контраргумент и вариант решения', ckb: 'داواکاری، بەڵگەی بەرامبەر و هەڵبژاردەی چارەسەر بە بابەتی جیا بکەیتەوە', kmr: 'daxwaz, argumana dijber û vebijarka çareseriyê bi awayekî babetî cuda kirin', pl: 'rzeczowe oddzielenie żądania, kontrargumentu i opcji rozwiązania', ro: 'separarea factuală a cererii, contraargumentului și opțiunii de soluție', sq: 'ndarjen faktike të kërkesës, kundërargumentit dhe opsionit të zgjidhjes' },
    grammar: ['c1-expressing-nuance-and-limitation', 'Nuancierung und Begrenzung'],
    target: ['roleplay', 'c1-eine-mietrechtliche-streitfrage-sachlich-besprechen']
  },
  {
    slug: 'c1-amt-recht-und-komplexe-alltagsfaelle-wiederholen',
    topic: { de: 'Amt, Recht und komplexe Alltagsfaelle wiederholen', en: 'reviewing administration, law, and complex everyday cases', fa: 'مرور اداره، قانون و پرونده‌های پیچیده روزمره', ar: 'مراجعة الإدارة والقانون والحالات اليومية المعقدة', tr: 'resmî daire, hukuk ve karmaşık günlük vakaları tekrar etme', ru: 'повторение администрации, права и сложных бытовых случаев', ckb: 'دووبارەکردنەوەی دامەزراوە، یاسا و کەیسە ئاڵۆزەکانی ژیانی ڕۆژانە', kmr: 'dubarekirina dezgeh, hiqûq û rewşên rojane yên tevlihev', pl: 'powtórka urzędu, prawa i złożonych spraw codziennych', ro: 'recapitularea administrației, dreptului și cazurilor cotidiene complexe', sq: 'përsëritja e administratës, ligjit dhe rasteve të ndërlikuara të përditshme' },
    focus: { de: 'Sachverhalt, Beleg, Bitte und naechsten Schritt in einem klaren Ablauf verbinden', en: 'linking facts, evidence, request, and next step in a clear sequence', fa: 'واقعیت، مدرک، درخواست و قدم بعدی را در یک روند روشن به هم وصل کنی', ar: 'ربط الوقائع والأدلة والطلب والخطوة التالية في تسلسل واضح', tr: 'olay, belge, rica ve sonraki adımı açık bir akışta birleştirmeyi', ru: 'связывать факты, доказательство, просьбу и следующий шаг в ясную последовательность', ckb: 'ڕاستی، بەڵگە، داواکاری و هەنگاوی داهاتوو لە ڕەوتێکی ڕووندا پێکەوە ببەستیت', kmr: 'rastî, delîl, daxwaz û gava din di rêzeke zelal de girêdan', pl: 'połączenie faktów, dowodu, prośby i następnego kroku w jasny przebieg', ro: 'legarea faptelor, dovezii, cererii și pasului următor într-o secvență clară', sq: 'lidhjen e faktit, provës, kërkesës dhe hapit tjetër në një rrjedhë të qartë' },
    grammar: ['c1-c1-register-review', 'Register-Review'],
    target: ['exam-prep-unit', 'c1-adressatenbezug-konsequent-halten']
  }
];

function translations(map) {
  return langs.map(language => ({ language, text: map[language] }));
}

function title(key) {
  return {
    title: titles[key].de,
    titleTranslations: translations(titles[key])
  };
}

function orientInstruction(item) {
  return {
    de: `Lies den Lektionstext zu ${item.topic.de} und notiere: Was ist gesichert, was ist unklar, und was muss als Naechstes passieren?`,
    en: `Read the lesson text on ${item.topic.en} and note what is certain, what is unclear, and what needs to happen next.`,
    fa: `متن درس درباره ${item.topic.fa} را بخوان و یادداشت کن: چه چیز قطعی است، چه چیز مبهم است و قدم بعدی چیست.`,
    ar: `اقرأ نص الدرس عن ${item.topic.ar} وسجّل: ما المؤكد، ما غير الواضح، وما الخطوة التالية؟`,
    tr: `${item.topic.tr} konulu ders metnini oku ve not et: ne kesin, ne belirsiz, sonraki adım ne?`,
    ru: `Прочитай текст урока о теме «${item.topic.ru}» и отметь: что точно, что неясно и что должно произойти дальше.`,
    ckb: `دەقی وانەکە لەسەر ${item.topic.ckb} بخوێنەوە و بنووسە: چی دڵنیایە، چی ناڕوونە و هەنگاوی داهاتوو چییە.`,
    kmr: `Nivîsa dersê li ser ${item.topic.kmr} bixwîne û binivîse: çi teqez e, çi nezelal e û gava din çi ye.`,
    pl: `Przeczytaj tekst lekcji o temacie: ${item.topic.pl} i zanotuj: co jest pewne, co niejasne i co powinno wydarzyć się dalej.`,
    ro: `Citește textul lecției despre ${item.topic.ro} și notează: ce este sigur, ce este neclar și care este pasul următor.`,
    sq: `Lexo tekstin e mësimit për ${item.topic.sq} dhe shëno: çfarë është e sigurt, çfarë është e paqartë dhe cili është hapi tjetër.`
  };
}

function languageInstruction(item) {
  return {
    de: `Oeffne ${item.grammar[1]} und sammle drei Formulierungen, mit denen du ${item.focus.de}.`,
    en: `Open the linked formal-language section and collect three formulations that help you with this skill: ${item.focus.en}.`,
    fa: `بخش زبان رسمی لینک‌شده را باز کن و سه عبارت پیدا کن که به این مهارت کمک کند: ${item.focus.fa}.`,
    ar: `افتح قسم اللغة الرسمية المرتبط واجمع ثلاث صيغ تساعد على هذه المهارة: ${item.focus.ar}.`,
    tr: `Bağlantılı resmî dil bölümünü aç ve şu beceriye yardım eden üç ifade topla: ${item.focus.tr}.`,
    ru: `Открой связанный раздел официального языка и собери три формулировки для этого умения: ${item.focus.ru}.`,
    ckb: `بەشی زمانی فەرمیی بەستەرکراو بکەرەوە و سێ دەربڕین کۆبکەوە کە یارمەتی ئەم توانایە بدات: ${item.focus.ckb}.`,
    kmr: `Beşa zimana fermî ya girêdayî veke û sê derbirînan kom bike ku alîkarîya vê jêhatinê bikin: ${item.focus.kmr}.`,
    pl: `Otwórz połączoną sekcję języka formalnego i zbierz trzy sformułowania wspierające tę umiejętność: ${item.focus.pl}.`,
    ro: `Deschide secțiunea legată de limbaj formal și adună trei formulări care susțin această abilitate: ${item.focus.ro}.`,
    sq: `Hap seksionin e lidhur të gjuhës formale dhe mblidh tri formulime që ndihmojnë këtë aftësi: ${item.focus.sq}.`
  };
}

function materialInstruction() {
  return {
    de: 'Bearbeite das verlinkte Falltraining und achte darauf, wie Fakten, Bitte und Ton voneinander getrennt werden.',
    en: 'Work through the linked case practice and notice how facts, request, and tone are kept separate.',
    fa: 'تمرین پرونده لینک‌شده را انجام بده و دقت کن چطور واقعیت‌ها، درخواست و لحن از هم جدا نگه داشته می‌شوند.',
    ar: 'اعمل على تدريب الحالة المرتبط وانتبه إلى كيفية فصل الوقائع والطلب والنبرة.',
    tr: 'Bağlantılı vaka alıştırmasını çalış ve olgular, rica ve tonun nasıl ayrı tutulduğuna dikkat et.',
    ru: 'Проработай связанное упражнение по случаю и обрати внимание, как отделяются факты, просьба и тон.',
    ckb: 'ڕاهێنانی پەروەندەی بەستەرکراو کاربکە و سەرنج بدە چۆن ڕاستییەکان، داواکاری و تۆن جیا دەهێڵدرێنەوە.',
    kmr: 'Rahênana dosyayê ya girêdayî bixebite û bala xwe bide ka rastî, daxwaz û ton çawa cuda tên hiştin.',
    pl: 'Przerób połączone ćwiczenie sprawy i zwróć uwagę, jak oddzielane są fakty, prośba i ton.',
    ro: 'Lucrează cu exercițiul de caz legat și observă cum sunt separate faptele, cererea și tonul.',
    sq: 'Puno me ushtrimin e lidhur të rastit dhe vëzhgo si ndahen faktet, kërkesa dhe toni.'
  };
}

function applyInstruction() {
  return {
    de: 'Formuliere den naechsten Schritt in vier Teilen: Bezug auf Aktenlage, klares Anliegen, fehlende Information, Bitte um Rueckmeldung.',
    en: 'Formulate the next step in four parts: reference to the file situation, clear concern, missing information, request for response.',
    fa: 'قدم بعدی را در چهار بخش بنویس: اشاره به وضعیت پرونده، خواسته روشن، اطلاعات ناقص، درخواست پاسخ.',
    ar: 'صغ الخطوة التالية في أربعة أجزاء: إشارة إلى وضع الملف، طلب واضح، معلومة ناقصة، طلب رد.',
    tr: 'Sonraki adımı dört parçada formüle et: dosya durumuna atıf, açık talep, eksik bilgi, yanıt talebi.',
    ru: 'Сформулируй следующий шаг из четырех частей: ссылка на состояние дела, ясная просьба, недостающая информация, просьба об ответе.',
    ckb: 'هەنگاوی داهاتوو بە چوار بەش بنووسە: ئاماژە بە دۆخی پەروەندە، داواکاریی ڕوون، زانیاریی کەم، داوای وەڵام.',
    kmr: 'Gava din bi çar beşan formule bike: amaje bo rewşa dosyayê, daxwaza zelal, agahiya kêm, daxwaza bersivê.',
    pl: 'Sformułuj następny krok w czterech częściach: odniesienie do stanu akt, jasna sprawa, brakująca informacja, prośba o odpowiedź.',
    ro: 'Formulează pasul următor în patru părți: referire la situația dosarului, solicitare clară, informație lipsă, cerere de răspuns.',
    sq: 'Formulo hapin tjetër në katër pjesë: lidhje me gjendjen e dosjes, kërkesë e qartë, informacion i munguar, kërkesë për përgjigje.'
  };
}

function reviewInstruction() {
  return {
    de: 'Pruefe, ob deine Formulierung sachlich bleibt und keine Rechtsberatung behauptet, sondern eine Pruefung oder Klaerung ermoeglicht.',
    en: 'Check whether your wording stays factual and does not claim legal advice, but enables review or clarification.',
    fa: 'بررسی کن که جمله‌بندی تو عینی می‌ماند و ادعای مشاوره حقوقی نمی‌کند، بلکه امکان بررسی یا روشن‌سازی می‌دهد.',
    ar: 'راجع ما إذا كانت صياغتك موضوعية ولا تدّعي تقديم استشارة قانونية، بل تتيح المراجعة أو التوضيح.',
    tr: 'İfadenin nesnel kaldığını ve hukuki danışmanlık iddiası taşımadığını, sadece inceleme veya açıklama sağladığını kontrol et.',
    ru: 'Проверь, остается ли формулировка предметной и не заявляет юридическую консультацию, а позволяет проверку или уточнение.',
    ckb: 'بپشکنە داڕشتنەکەت بابەتی دەمێنێتەوە و بانگەشەی ڕاوێژی یاسایی ناکات، بەڵکو ڕێگە بە پشکنین یان ڕوونکردنەوە دەدات.',
    kmr: 'Kontrol bike ka formulekirina te babetî dimîne û îdiaya şêwirmendiya hiqûqî nake, lê kontrol an zelalkirinê gengaz dike.',
    pl: 'Sprawdź, czy sformułowanie pozostaje rzeczowe i nie udaje porady prawnej, lecz umożliwia sprawdzenie lub wyjaśnienie.',
    ro: 'Verifică dacă formularea rămâne factuală și nu pretinde consiliere juridică, ci permite verificare sau clarificare.',
    sq: 'Kontrollo nëse formulimi yt mbetet faktik dhe nuk pretendon këshillë juridike, por mundëson shqyrtim ose sqarim.'
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
console.log(`Updated ${items.length} C1 Module 8 lessons with ${items.length * 5} activity blocks.`);

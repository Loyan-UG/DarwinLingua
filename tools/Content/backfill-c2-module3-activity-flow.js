const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c2-stil-souveraenitaet-und-komplexer-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'Unausgesprochenes markieren',
    en: 'Mark what is left unsaid',
    fa: 'بخش ناگفته را مشخص کن',
    ar: 'حدّد ما لم يُقل صراحة',
    tr: 'Söylenmeyeni işaretle',
    ru: 'Отметь невысказанное',
    ckb: 'ئەوەی نەوتراوە دیاری بکە',
    kmr: 'Ya nehatiye gotin nîşan bike',
    pl: 'Zaznacz to, co niewypowiedziane',
    ro: 'Marchează ce rămâne nespus',
    sq: 'Shëno atë që nuk thuhet hapur'
  },
  language: {
    de: 'Signal und Bezug sichern',
    en: 'Secure signal and reference',
    fa: 'نشانه و مرجع را روشن کن',
    ar: 'ثبّت الإشارة والمرجع',
    tr: 'İşareti ve göndermeyi güvenceye al',
    ru: 'Закрепи сигнал и отсылку',
    ckb: 'نیشانە و ئاماژە ڕوون بکە',
    kmr: 'Nîşan û referansê misoger bike',
    pl: 'Zabezpiecz sygnał i odniesienie',
    ro: 'Asigură semnalul și referința',
    sq: 'Siguro shenjën dhe referimin'
  },
  target: {
    de: 'Implizite Wirkung testen',
    en: 'Test the implicit effect',
    fa: 'اثر ضمنی را آزمایش کن',
    ar: 'اختبر الأثر الضمني',
    tr: 'Örtük etkiyi sına',
    ru: 'Проверь скрытый эффект',
    ckb: 'کاریگەری شاراوە بپشکنە',
    kmr: 'Bandora veşartî biceribîne',
    pl: 'Sprawdź efekt ukryty',
    ro: 'Testează efectul implicit',
    sq: 'Testo efektin e nënkuptuar'
  },
  transfer: {
    de: 'Deutung kontrolliert formulieren',
    en: 'Formulate the interpretation carefully',
    fa: 'برداشت خود را کنترل‌شده بیان کن',
    ar: 'صغ تفسيرك بحذر',
    tr: 'Yorumunu kontrollü biçimde ifade et',
    ru: 'Сформулируй толкование осторожно',
    ckb: 'لێکدانەوەکەت بە کۆنترۆڵەوە دەرببڕە',
    kmr: 'Şîroveya xwe bi kontrolî bibêje',
    pl: 'Sformułuj interpretację ostrożnie',
    ro: 'Formulează interpretarea controlat',
    sq: 'Formulo interpretimin me kujdes'
  },
  review: {
    de: 'Grenze der Deutung notieren',
    en: 'Note the limit of interpretation',
    fa: 'مرز برداشت را یادداشت کن',
    ar: 'دوّن حدود التفسير',
    tr: 'Yorumun sınırını not et',
    ru: 'Запиши границу толкования',
    ckb: 'سنوری لێکدانەوە تۆمار بکە',
    kmr: 'Sînorê şîroveyê tomar bike',
    pl: 'Zapisz granicę interpretacji',
    ro: 'Notează limita interpretării',
    sq: 'Shëno kufirin e interpretimit'
  }
};

const items = [
  {
    slug: 'c2-implizite-bezuege-und-kohaesion',
    topic: { de: 'implizite Bezuege und Kohaesion', en: 'implicit references and cohesion', fa: 'ارجاع‌های ضمنی و پیوستگی متن', ar: 'الإحالات الضمنية والتماسك النصي', tr: 'örtük göndermeler ve metin bütünlüğü', ru: 'скрытые отсылки и связность текста', ckb: 'ئاماژە شاراوەکان و پەیوەندی دەق', kmr: 'referansên veşartî û girêdana nivîsê', pl: 'ukryte odniesienia i spójność tekstu', ro: 'referințe implicite și coeziune', sq: 'referime të nënkuptuara dhe kohezion' },
    focus: { de: 'unausgesprochene Bezuge so zu erkennen, dass die Argumentlinie nicht verloren geht', en: 'recognizing unstated references so the argumentative line is not lost', fa: 'ارجاع‌های ناگفته را طوری تشخیص بدهی که خط استدلال گم نشود', ar: 'تمييز الإحالات غير المصرّح بها بحيث لا يضيع خط الحجة', tr: 'açık söylenmeyen göndermeleri fark edip argüman çizgisini kaybetmemek', ru: 'распознавать невысказанные отсылки так, чтобы не потерять линию аргумента', ckb: 'ئاماژە نەوتراوەکان بناسیت بە شێوەیەک کە هێڵی بەڵگەهێنان ون نەبێت', kmr: 'referansên nehatiye gotin nas bikî bêyî ku xeta argumanê winda bibe', pl: 'rozpoznać niewypowiedziane odniesienia, aby nie zgubić linii argumentu', ro: 'să recunoști referințele nespuse fără să pierzi linia argumentului', sq: 'të njohësh referimet e pathëna pa humbur vijën e argumentit' },
    grammar: 'c2-implicit-references-and-cohesion',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-verweisbeziehungen-im-lesetext-klaeren'
  },
  {
    slug: 'c2-mehrdeutigkeit-erkennen-und-aufloesen',
    topic: { de: 'Mehrdeutigkeit', en: 'ambiguity', fa: 'ابهام معنایی', ar: 'تعدد المعنى', tr: 'çok anlamlılık', ru: 'многозначность', ckb: 'چەندمانایی', kmr: 'pirwateyî', pl: 'wieloznaczność', ro: 'ambiguitate', sq: 'dykuptimësi' },
    focus: { de: 'mehrere Lesarten fair zu pruefen und erst dann die wahrscheinlichste Deutung zu begruenden', en: 'checking several readings fairly before justifying the most likely interpretation', fa: 'چند برداشت ممکن را منصفانه بررسی کنی و بعد محتمل‌ترین برداشت را توضیح بدهی', ar: 'فحص قراءات متعددة بإنصاف ثم تبرير التفسير الأرجح', tr: 'birden fazla okuma biçimini adilce kontrol edip sonra en olası yorumu gerekçelendirmek', ru: 'честно проверить несколько прочтений и затем обосновать наиболее вероятное толкование', ckb: 'چەند خوێندنەوەیەک بە دادپەروەری بپشکنیت و دواتر لێکدانەوەی گونجاوتر ڕوون بکەیتەوە', kmr: 'çend xwendinên mimkun bi adilî binirxînî û paşê şîroveya herî gengaz rave bikî', pl: 'uczciwie sprawdzić kilka odczytań, a dopiero potem uzasadnić najbardziej prawdopodobną interpretację', ro: 'să verifici corect mai multe lecturi și abia apoi să justifici interpretarea cea mai probabilă', sq: 'të shqyrtosh disa lexime në mënyrë të drejtë dhe pastaj të arsyetosh interpretimin më të mundshëm' },
    grammar: 'c2-ambiguity-and-disambiguation',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-mehrdeutigkeit-produktiv-nutzen'
  },
  {
    slug: 'c2-ironie-und-indirektheit-in-der-syntax',
    topic: { de: 'Ironie und indirekte Syntax', en: 'irony and indirect syntax', fa: 'کنایه و ساختار غیرمستقیم جمله', ar: 'السخرية والبنية غير المباشرة', tr: 'ironi ve dolaylı söz dizimi', ru: 'ирония и непрямой синтаксис', ckb: 'ئیرۆنی و ڕستەسازی ناڕاستەوخۆ', kmr: 'îronî û hevoksaziya nerasterast', pl: 'ironia i składnia pośrednia', ro: 'ironie și sintaxă indirectă', sq: 'ironi dhe sintaksë e tërthortë' },
    focus: { de: 'Ironie nicht vorschnell zu unterstellen, sondern an Satzbau, Kontext und Ton abzusichern', en: 'not assuming irony too quickly, but grounding it in syntax, context, and tone', fa: 'کنایه را عجولانه فرض نکنی، بلکه آن را با ساختار جمله، زمینه و لحن تأیید کنی', ar: 'ألا تفترض السخرية بسرعة، بل تثبتها بالبنية والسياق والنبرة', tr: 'ironiyi aceleyle varsaymamak, onu söz dizimi, bağlam ve tonla sağlamlaştırmak', ru: 'не приписывать иронию слишком быстро, а подтверждать ее синтаксисом, контекстом и тоном', ckb: 'ئیرۆنی بە پەلە دانەنێیت، بەڵکو بە ڕستەسازی، دەوروبەر و دەنگ پشتڕاستی بکەیتەوە', kmr: 'îroniyê zû texmîn nekî, lê bi hevoksazî, kontekst û tonê piştrast bikî', pl: 'nie zakładać ironii zbyt szybko, lecz potwierdzać ją składnią, kontekstem i tonem', ro: 'să nu presupui ironia prea repede, ci să o susții prin sintaxă, context și ton', sq: 'të mos supozosh ironinë shumë shpejt, por ta mbështesësh te sintaksa, konteksti dhe toni' },
    grammar: 'c2-irony-and-indirectness-in-syntax',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-ironie-und-indirekte-wertung-lesen'
  },
  {
    slug: 'c2-modalverben-mit-feiner-bedeutung',
    topic: { de: 'Modalverben mit feiner Bedeutung', en: 'modal verbs with subtle meaning', fa: 'فعل‌های modal با معنای ظریف', ar: 'الأفعال المساعدة ذات المعاني الدقيقة', tr: 'ince anlamlı modal fiiller', ru: 'модальные глаголы с тонким значением', ckb: 'کرداری مۆدال بە مانای ورد', kmr: 'lêkerên modal bi wateya hûr', pl: 'czasowniki modalne z subtelnym znaczeniem', ro: 'verbe modale cu sens fin', sq: 'folje modale me kuptim të hollë' },
    focus: { de: 'Wahrscheinlichkeit, Pflicht, Zweifel oder Distanzierung durch Modalverben genau zu unterscheiden', en: 'distinguishing probability, obligation, doubt, or lack of alignment through modal verbs precisely', fa: 'با فعل‌های modal احتمال، اجبار، تردید یا عدم همسویی با گفته را دقیق از هم جدا کنی', ar: 'التمييز بدقة بين الاحتمال والالتزام والشك أو عدم التبنّي عبر الأفعال المساعدة', tr: 'modal fiillerle olasılık, zorunluluk, kuşku veya mesafe almayı kesin ayırmak', ru: 'точно различать вероятность, обязанность, сомнение или несогласованность с высказыванием через модальные глаголы', ckb: 'بە کردارە مۆدالەکان ئەگەر، ناچاری، گومان یان نەهاوسۆزی لەگەڵ وتەکە بە وردی جیا بکەیتەوە', kmr: 'bi lêkerên modal îhtîmal, mecbûrî, guman an nehevahengiya bi gotinê re bi hûrgilî cuda bikî', pl: 'precyzyjnie odróżniać prawdopodobieństwo, obowiązek, wątpliwość albo brak utożsamienia z wypowiedzią przez czasowniki modalne', ro: 'să distingi precis probabilitatea, obligația, îndoiala sau lipsa de aliniere prin verbe modale', sq: 'të dallosh saktë probabilitetin, detyrimin, dyshimin ose mosidentifikimin me pohimin përmes foljeve modale' },
    grammar: 'c2-subtle-modal-verb-nuance',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-sicherheitsgrad-im-hoeren-erkennen'
  },
  {
    slug: 'c2-modalpartikeln-fortgeschritten',
    topic: { de: 'fortgeschrittene Modalpartikeln', en: 'advanced modal particles', fa: 'ذره‌های modal پیشرفته', ar: 'الجسيمات التعبيرية المتقدمة', tr: 'ileri düzey modal partikeller', ru: 'продвинутые модальные частицы', ckb: 'پارچەوشە مۆدالە پێشکەوتووەکان', kmr: 'partîkelên modal ên pêşketî', pl: 'zaawansowane partykuły modalne', ro: 'particule modale avansate', sq: 'pjesëza modale të avancuara' },
    focus: { de: 'kleine Partikeln als Hinweis auf Erwartung, Geduld, Zweifel oder gemeinsame Annahmen zu lesen', en: 'reading small particles as signals of expectation, patience, doubt, or shared assumptions', fa: 'ذره‌های کوچک را نشانه انتظار، صبر، تردید یا فرض مشترک بدانی', ar: 'قراءة الجسيمات الصغيرة كإشارات إلى التوقع أو الصبر أو الشك أو الافتراض المشترك', tr: 'küçük partikelleri beklenti, sabır, kuşku veya ortak varsayım işareti olarak okumak', ru: 'читать маленькие частицы как сигналы ожидания, терпения, сомнения или общих предпосылок', ckb: 'پارچەوشە بچووکەکان وەک نیشانەی چاوەڕوانی، ئارامی، گومان یان پێشبینی هاوبەش بخوێنیتەوە', kmr: 'partîkelên biçûk wek nîşana hêvî, sebir, guman an texmînên hevpar bixwînî', pl: 'czytać małe partykuły jako sygnały oczekiwania, cierpliwości, wątpliwości lub wspólnych założeń', ro: 'să citești particulele mici ca semnale de așteptare, răbdare, îndoială sau presupuneri comune', sq: 't’i lexosh pjesëzat e vogla si sinjale pritjeje, durimi, dyshimi ose supozimesh të përbashkëta' },
    grammar: 'c2-advanced-modal-particles',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-frageabsicht-vor-antwort-klaeren'
  },
  {
    slug: 'c2-zwischen-den-zeilen-liegende-kritik',
    topic: { de: 'zwischen den Zeilen liegende Kritik', en: 'criticism between the lines', fa: 'انتقاد بین خطوط', ar: 'النقد بين السطور', tr: 'satır aralarındaki eleştiri', ru: 'критика между строк', ckb: 'ڕەخنەی نێوان دێڕەکان', kmr: 'rexneya di navbera rêzan de', pl: 'krytyka między wierszami', ro: 'critică printre rânduri', sq: 'kritikë mes rreshtave' },
    focus: { de: 'verdeckte Kritik anzusprechen, ohne sie zu ueberdehnen oder das Gegenueber blosszustellen', en: 'addressing hidden criticism without overstating it or exposing the other person', fa: 'انتقاد پنهان را مطرح کنی، بدون اینکه آن را بیش از حد بزرگ کنی یا طرف مقابل را در تنگنا بگذاری', ar: 'التطرق إلى النقد غير المباشر من دون تضخيمه أو إحراج الطرف الآخر', tr: 'örtük eleştiriyi abartmadan ve karşı tarafı zor durumda bırakmadan ele almak', ru: 'поднимать скрытую критику, не преувеличивая ее и не ставя собеседника в неловкое положение', ckb: 'ڕەخنەی شاراوە باس بکەیت بەبێ ئەوەی زۆر بکەیتەوە یان بەرامبەرەکە ڕیسوا بکەیت', kmr: 'rexneya veşartî bînin ser ziman bêyî wê zêde bikî an kesê din şerm bike', pl: 'poruszyć ukrytą krytykę bez wyolbrzymiania jej i bez zawstydzania rozmówcy', ro: 'să abordezi critica ascunsă fără să o exagerezi sau să expui interlocutorul', sq: 'ta trajtosh kritikën e fshehur pa e tepruar dhe pa e vënë tjetrin në siklet' },
    grammar: 'c2-irony-and-indirectness-in-syntax',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-zwischen-den-zeilen-liegende-kritik-ansprechen'
  },
  {
    slug: 'c2-rhetorische-fragen-in-debatten',
    topic: { de: 'rhetorische Fragen in Debatten', en: 'rhetorical questions in debates', fa: 'پرسش‌های بلاغی در بحث‌ها', ar: 'الأسئلة البلاغية في النقاشات', tr: 'tartışmalarda retorik sorular', ru: 'риторические вопросы в дебатах', ckb: 'پرسیاری ڕەوانبێژی لە مشتومڕدا', kmr: 'pirsên retorîkî di nîqaşan de', pl: 'pytania retoryczne w debatach', ro: 'întrebări retorice în dezbateri', sq: 'pyetje retorike në debate' },
    focus: { de: 'rhetorische Fragen als Denkangebot zu nutzen, nicht als versteckten Angriff', en: 'using rhetorical questions as an invitation to think, not as a hidden attack', fa: 'پرسش بلاغی را به‌عنوان دعوت به فکر کردن به کار ببری، نه حمله پنهان', ar: 'استخدام السؤال البلاغي كدعوة إلى التفكير، لا كهجوم مبطن', tr: 'retorik soruları gizli saldırı değil, düşünmeye davet olarak kullanmak', ru: 'использовать риторические вопросы как приглашение подумать, а не как скрытую атаку', ckb: 'پرسیاری ڕەوانبێژی وەک بانگهێشتی بیرکردنەوە بەکاربهێنیت، نە وەک هێرشی شاراوە', kmr: 'pirsên retorîkî wek vexwendina fikirkirinê bikar bînî, ne wek êrişeke veşartî', pl: 'użyć pytania retorycznego jako zaproszenia do myślenia, nie jako ukrytego ataku', ro: 'să folosești întrebarea retorică drept invitație la reflecție, nu ca atac ascuns', sq: 'ta përdorësh pyetjen retorike si ftesë për të menduar, jo si sulm të fshehur' },
    grammar: 'c2-rhetorical-sentence-structures',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-rhetorische-frage-in-einer-debatte-aufgreifen'
  },
  {
    slug: 'c2-provokante-these-entschaerfen',
    topic: { de: 'provokante Thesen', en: 'provocative theses', fa: 'تزهای تحریک‌آمیز', ar: 'الأطروحات الاستفزازية', tr: 'kışkırtıcı tezler', ru: 'провокационные тезисы', ckb: 'تیزی ورووژێنەر', kmr: 'tezên provokatîf', pl: 'prowokacyjne tezy', ro: 'teze provocatoare', sq: 'teza provokuese' },
    focus: { de: 'eine provokante These zu entschaerfen, ohne ihren moeglichen Erkenntniswert zu verlieren', en: 'defusing a provocative thesis without losing its possible insight', fa: 'یک تز تحریک‌آمیز را آرام‌تر کنی، بدون اینکه ارزش فکری احتمالی آن از بین برود', ar: 'تخفيف حدة أطروحة استفزازية من دون فقدان قيمتها الفكرية المحتملة', tr: 'kışkırtıcı bir tezi olası düşünsel değerini kaybetmeden yumuşatmak', ru: 'смягчать провокационный тезис, не теряя его возможной познавательной ценности', ckb: 'تیزێکی ورووژێنەر ئارام بکەیتەوە بەبێ لەدەستدانی نرخە بیرکردنەوەیەکەی', kmr: 'tezê provokatîf aramtir bikî bêyî nirxa zanistî ya gengaz winda bibe', pl: 'rozbroić prowokacyjną tezę bez utraty jej możliwej wartości poznawczej', ro: 'să dezamorsezi o teză provocatoare fără să pierzi posibila ei valoare de înțelegere', sq: 'ta zbutësh një tezë provokuese pa humbur vlerën e saj të mundshme për të kuptuar' },
    grammar: 'c2-c2-debate-grammar',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-provokante-these-entschaerfen-und-nutzen'
  },
  {
    slug: 'c2-nicht-gesagtes-problem-ansprechen',
    topic: { de: 'nicht gesagte Probleme', en: 'unspoken problems', fa: 'مشکل‌های ناگفته', ar: 'المشكلات غير المصرّح بها', tr: 'dile getirilmeyen sorunlar', ru: 'невысказанные проблемы', ckb: 'کێشە نەوتراوەکان', kmr: 'pirsgirêkên nehatiye gotin', pl: 'niewypowiedziane problemy', ro: 'probleme nespuse', sq: 'probleme të pathëna' },
    focus: { de: 'ein unausgesprochenes Problem vorsichtig zu benennen und dem Gegenueber Raum fuer Korrektur zu lassen', en: 'naming an unspoken problem carefully while leaving the other person room to correct you', fa: 'مشکل ناگفته را با احتیاط نام ببری و همزمان برای اصلاح برداشتت به طرف مقابل فضا بدهی', ar: 'تسمية مشكلة غير مصرح بها بحذر مع ترك مجال للطرف الآخر كي يصحح فهمك', tr: 'dile getirilmeyen bir sorunu dikkatle adlandırmak ve karşı tarafa yorumunu düzeltme alanı bırakmak', ru: 'осторожно назвать невысказанную проблему и оставить собеседнику пространство для корректировки твоего понимания', ckb: 'کێشەیەکی نەوتراو بە وریایی ناوببەیت و هەمان کات شوێن بدەیت بە بەرامبەرەکە بۆ ڕاستکردنەوەی تێگەیشتنت', kmr: 'pirsgirêkeke nehatiye gotin bi baldarî nav bikî û ji kesê din re cih bihêlî ku têgihiştina te rast bike', pl: 'ostrożnie nazwać niewypowiedziany problem i zostawić rozmówcy miejsce na skorygowanie twojego odczytania', ro: 'să numești cu grijă o problemă nespusă și să lași interlocutorului spațiu să îți corecteze interpretarea', sq: 'ta emërtosh me kujdes një problem të pathënë dhe t’i lësh tjetrit hapësirë të korrigjojë leximin tënd' },
    grammar: 'c2-implicit-references-and-cohesion',
    targetType: 'roleplay',
    targetSlug: 'c2-ein-nicht-gesagtes-problem-ansprechen'
  },
  {
    slug: 'c2-implizites-ironie-und-mehrdeutigkeit-wiederholen',
    topic: { de: 'Implizites, Ironie und Mehrdeutigkeit', en: 'implicit meaning, irony, and ambiguity', fa: 'معنای ضمنی، کنایه و ابهام', ar: 'المعنى الضمني والسخرية وتعدد المعنى', tr: 'örtük anlam, ironi ve çok anlamlılık', ru: 'скрытый смысл, ирония и многозначность', ckb: 'مانای شاراوە، ئیرۆنی و چەندمانایی', kmr: 'wateya veşartî, îronî û pirwateyî', pl: 'sens ukryty, ironia i wieloznaczność', ro: 'sens implicit, ironie și ambiguitate', sq: 'kuptim i nënkuptuar, ironi dhe dykuptimësi' },
    focus: { de: 'eine persoenliche Pruefroutine zu nutzen: Signal, Kontext, alternative Deutung und angemessene Reaktion', en: 'using a personal checking routine: signal, context, alternative interpretation, and appropriate response', fa: 'یک روال شخصی برای بررسی به کار ببری: نشانه، زمینه، برداشت جایگزین و واکنش مناسب', ar: 'استخدام روتين شخصي للفحص: الإشارة والسياق والتفسير البديل والرد المناسب', tr: 'kişisel bir kontrol rutini kullanmak: işaret, bağlam, alternatif yorum ve uygun tepki', ru: 'использовать личную процедуру проверки: сигнал, контекст, альтернативное толкование и подходящая реакция', ckb: 'ڕووتینێکی تایبەتی بۆ پشکنین بەکاربهێنیت: نیشانە، دەوروبەر، لێکدانەوەی جێگرەوە و وەڵامی گونجاو', kmr: 'rêbazeke xwe ji bo kontrolê bikar bînî: nîşan, kontekst, şîroveya alternatîf û bersiva guncan', pl: 'stosować własną rutynę sprawdzania: sygnał, kontekst, alternatywna interpretacja i odpowiednia reakcja', ro: 'să folosești o rutină proprie de verificare: semnal, context, interpretare alternativă și reacție potrivită', sq: 'të përdorësh një rutinë personale kontrolli: shenja, konteksti, interpretimi alternativ dhe reagimi i përshtatshëm' },
    grammar: 'c2-c2-grammar-review-map',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-frageabsicht-vor-antwort-klaeren'
  }
];

function translationArray(map) {
  return langs.map(language => ({ language, text: map[language] }));
}

function orientInstruction(item) {
  return {
    de: `Lies den Lektionstext zu ${item.topic.de}. Markiere eine Stelle, an der die eigentliche Aussage nicht direkt ausgesprochen wird, sondern aus Kontext, Ton oder Bezug entsteht.`,
    en: `Read the lesson text on ${item.topic.en}. Mark one place where the real message is not stated directly, but arises from context, tone, or reference.`,
    fa: `متن درس درباره ${item.topic.fa} را بخوان. جایی را مشخص کن که پیام اصلی مستقیم گفته نشده، بلکه از زمینه، لحن یا ارجاع فهمیده می‌شود.`,
    ar: `اقرأ نص الدرس عن ${item.topic.ar}. حدّد موضعًا لا تُقال فيه الرسالة الأساسية مباشرة، بل تُفهم من السياق أو النبرة أو الإحالة.`,
    tr: `${item.topic.tr} hakkındaki ders metnini oku. Asıl mesajın doğrudan söylenmediği, bağlamdan, tondan veya göndermeden çıktığı bir yeri işaretle.`,
    ru: `Прочитай урок о теме: ${item.topic.ru}. Отметь место, где главная мысль не сказана прямо, а возникает из контекста, тона или отсылки.`,
    ckb: `دەقی وانەکە دەربارەی ${item.topic.ckb} بخوێنەوە. شوێنێک دیاری بکە کە پەیامی سەرەکی ڕاستەوخۆ نەوتراوە، بەڵکو لە دەوروبەر، دەنگ یان ئاماژەوە تێدەگەیت.`,
    kmr: `Nivîsa dersê derbarê ${item.topic.kmr} bixwîne. Cihê ku peyama sereke rasterast nehatiye gotin, lê ji kontekst, ton an referansê derdikeve nîşan bike.`,
    pl: `Przeczytaj tekst lekcji o: ${item.topic.pl}. Zaznacz miejsce, w którym właściwy przekaz nie jest powiedziany wprost, lecz wynika z kontekstu, tonu lub odniesienia.`,
    ro: `Citește textul lecției despre ${item.topic.ro}. Marchează un loc în care mesajul real nu este spus direct, ci reiese din context, ton sau referință.`,
    sq: `Lexo tekstin e mësimit për ${item.topic.sq}. Shëno një vend ku mesazhi kryesor nuk thuhet drejtpërdrejt, por del nga konteksti, toni ose referimi.`
  };
}

function languageInstruction(item) {
  return {
    de: `Oeffne den verlinkten Grammatikpunkt und sammle drei sprachliche Signale, die dir helfen, ${item.focus.de}.`,
    en: `Open the linked grammar point and collect three language signals that help you ${item.focus.en}.`,
    fa: `بخش گرامر لینک‌شده را باز کن و سه نشانه زبانی پیدا کن که کمک می‌کنند ${item.focus.fa}.`,
    ar: `افتح نقطة القواعد المرتبطة واجمع ثلاث إشارات لغوية تساعدك على ${item.focus.ar}.`,
    tr: `Bağlantılı dilbilgisi bölümünü aç ve sana şunda yardımcı olan üç dilsel işaret bul: ${item.focus.tr}.`,
    ru: `Открой связанный раздел грамматики и собери три языковых сигнала, которые помогают ${item.focus.ru}.`,
    ckb: `خاڵی ڕێزمانی بەستەرکراو بکەرەوە و سێ نیشانەی زمانی بدۆزەوە کە یارمەتیت دەدەن ${item.focus.ckb}.`,
    kmr: `Xala rêzimanê ya girêdayî veke û sê nîşanên zimanî bibîne ku alîkarin ${item.focus.kmr}.`,
    pl: `Otwórz podlinkowany punkt gramatyczny i zbierz trzy sygnały językowe, które pomagają ${item.focus.pl}.`,
    ro: `Deschide punctul de gramatică legat și adună trei semnale lingvistice care te ajută să ${item.focus.ro}.`,
    sq: `Hap pikën e lidhur të gramatikës dhe mblidh tri sinjale gjuhësore që të ndihmojnë ${item.focus.sq}.`
  };
}

function targetInstruction(item) {
  return {
    de: `Bearbeite die verlinkte Ressource. Pruefe danach, ob deine Reaktion vorsichtig genug bleibt, um ${item.focus.de}.`,
    en: `Work through the linked resource. Then check whether your response remains careful enough to ${item.focus.en}.`,
    fa: `منبع لینک‌شده را انجام بده. بعد بررسی کن آیا واکنش تو به‌اندازه کافی محتاطانه هست تا بتوانی ${item.focus.fa}.`,
    ar: `اعمل على المورد المرتبط. ثم تحقق هل يبقى ردك حذرًا بما يكفي كي ${item.focus.ar}.`,
    tr: `Bağlantılı kaynağı çalış. Sonra tepkinin şunu yapacak kadar dikkatli kalıp kalmadığını kontrol et: ${item.focus.tr}.`,
    ru: `Проработай связанный ресурс. Затем проверь, остается ли твоя реакция достаточно осторожной, чтобы ${item.focus.ru}.`,
    ckb: `سەرچاوەی بەستەرکراو کاربکە. پاشان بپشکنە ئایا وەڵامەکەت بەقەدەر پێویست وریایە بۆ ئەوەی ${item.focus.ckb}.`,
    kmr: `Çavkaniya girêdayî bixebitîne. Paşê binirxîne gelo bersiva te ewqas baldar dimîne ku ${item.focus.kmr}.`,
    pl: `Przerób podlinkowany materiał. Potem sprawdź, czy twoja reakcja pozostaje dość ostrożna, by ${item.focus.pl}.`,
    ro: `Lucrează resursa legată. Apoi verifică dacă reacția ta rămâne suficient de atentă ca să ${item.focus.ro}.`,
    sq: `Puno me burimin e lidhur. Pastaj kontrollo nëse reagimi yt mbetet mjaft i kujdesshëm për të ${item.focus.sq}.`
  };
}

function transferInstruction() {
  return {
    de: 'Formuliere zwei Saetze: zuerst eine direkte Deutung, dann eine vorsichtigere Version mit Signalwoertern wie offenbar, moeglicherweise, es wirkt so, als ob. Entscheide, welche Version sozial klueger ist.',
    en: 'Write two sentences: first a direct interpretation, then a more cautious version with signal words such as apparently, possibly, it seems as if. Decide which version is socially wiser.',
    fa: 'دو جمله بنویس: اول یک برداشت مستقیم، بعد نسخه‌ای محتاطانه‌تر با نشانه‌هایی مثل «ظاهراً»، «ممکن است»، «به نظر می‌رسد که». تصمیم بگیر کدام نسخه از نظر اجتماعی هوشمندانه‌تر است.',
    ar: 'اكتب جملتين: أولًا تفسيرًا مباشرًا، ثم صيغة أكثر حذرًا بإشارات مثل: يبدو أن، ربما، من المحتمل. قرّر أي صيغة أذكى اجتماعيًا.',
    tr: 'İki cümle yaz: önce doğrudan bir yorum, sonra offenbar, moeglicherweise, es wirkt so, als ob gibi işaretlerle daha temkinli bir versiyon. Hangi versiyonun sosyal açıdan daha akıllıca olduğuna karar ver.',
    ru: 'Напиши два предложения: сначала прямое толкование, затем более осторожную версию с сигналами вроде offenbar, moeglicherweise, es wirkt so, als ob. Реши, какая версия социально разумнее.',
    ckb: 'دوو ڕستە بنووسە: سەرەتا لێکدانەوەیەکی ڕاستەوخۆ، پاشان وەشانێکی وریاتر بە نیشانەووشەی وەک offenbar، moeglicherweise، es wirkt so, als ob. بڕیار بدە کام وەشان لە ڕووی کۆمەڵایەتییەوە ژیرانەترە.',
    kmr: 'Du hevokan binivîse: pêşî şîroveya rasterast, paşê guhertoyeke baldartir bi nîşanên wek offenbar, moeglicherweise, es wirkt so, als ob. Biryar bide kîjan guherto ji aliyê civakî ve jîrtir e.',
    pl: 'Napisz dwa zdania: najpierw bezpośrednią interpretację, potem ostrożniejszą wersję z sygnałami typu offenbar, moeglicherweise, es wirkt so, als ob. Zdecyduj, która wersja jest społecznie rozsądniejsza.',
    ro: 'Scrie două propoziții: mai întâi o interpretare directă, apoi o versiune mai prudentă cu semnale precum offenbar, moeglicherweise, es wirkt so, als ob. Decide ce versiune este social mai inteligentă.',
    sq: 'Shkruaj dy fjali: së pari një interpretim të drejtpërdrejtë, pastaj një version më të kujdesshëm me sinjale si offenbar, moeglicherweise, es wirkt so, als ob. Vendos cili version është më i mençur shoqërisht.'
  };
}

function reviewInstruction() {
  return {
    de: 'Notiere deine C2-Prueffrage fuer implizite Bedeutung: Welche Belege habe ich, welche alternative Deutung ist moeglich, und wie vorsichtig muss ich reagieren?',
    en: 'Write your C2 check question for implicit meaning: what evidence do I have, what alternative interpretation is possible, and how carefully must I respond?',
    fa: 'پرسش کنترل C2 خودت را برای معنای ضمنی بنویس: چه شاهدی دارم، چه برداشت جایگزینی ممکن است، و واکنشم باید چقدر محتاطانه باشد؟',
    ar: 'دوّن سؤال الفحص الخاص بك في C2 للمعنى الضمني: ما الدليل الذي أملكه، وما التفسير البديل الممكن، وما درجة الحذر المطلوبة في الرد؟',
    tr: 'Örtük anlam için C2 kontrol sorunu yaz: Hangi kanıtım var, hangi alternatif yorum mümkün, ve ne kadar dikkatli tepki vermeliyim?',
    ru: 'Запиши свой C2-контрольный вопрос для скрытого смысла: какие у меня доказательства, какое альтернативное толкование возможно и насколько осторожно нужно реагировать?',
    ckb: 'پرسیاری پشکنینی C2 ـی خۆت بۆ مانای شاراوە بنووسە: چ بەڵگەیەکم هەیە، چ لێکدانەوەی جێگرەوەیەک دەکرێت، و دەبێت چەندە بە وریایی وەڵام بدەم؟',
    kmr: 'Pirsa kontrola C2 ya xwe ji bo wateya veşartî binivîse: çi delîlên min hene, kîjan şîroveya alternatîf gengaz e, û divê çiqas baldar bersiv bidim?',
    pl: 'Zapisz swoje pytanie kontrolne C2 dla sensu ukrytego: jakie mam dowody, jaka alternatywna interpretacja jest możliwa i jak ostrożnie muszę zareagować?',
    ro: 'Notează întrebarea ta de control C2 pentru sens implicit: ce dovezi am, ce interpretare alternativă este posibilă și cât de atent trebuie să reacționez?',
    sq: 'Shëno pyetjen tënde kontrolluese C2 për kuptimin e nënkuptuar: çfarë provash kam, çfarë interpretimi alternativ është i mundur dhe sa me kujdes duhet të reagoj?'
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
    makeActivity(item.targetType === 'roleplay' ? 'roleplay' : 'exam-prep', 'target', targetInstruction(item), item.targetType, item.targetSlug, 30, 10),
    makeActivity('practice', 'transfer', transferInstruction(), 'none', null, 40, 9),
    makeActivity('review', 'review', reviewInstruction(), 'none', null, 50, 5)
  ];
}

fs.writeFileSync(file, `${JSON.stringify(data, null, 2)}\n`, 'utf8');
console.log(`Updated ${items.length} C2 Module 3 lessons with ${items.length * 5} activity blocks.`);

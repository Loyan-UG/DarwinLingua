const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c2-stil-souveraenitaet-und-komplexer-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: { de: 'Technischen Kern klaeren', en: 'Clarify the technical core', fa: 'هسته فنی موضوع را روشن کن', ar: 'وضّح جوهر المسألة التقنية', tr: 'Teknik özü netleştir', ru: 'Проясни техническое ядро', ckb: 'ناوەڕۆکی تەکنیکی ڕوون بکە', kmr: 'Navenda teknîkî zelal bike', pl: 'Wyjaśnij techniczny rdzeń', ro: 'Clarifică nucleul tehnic', sq: 'Qartëso thelbin teknik' },
  language: { de: 'Komplexitaet uebersetzen', en: 'Translate complexity', fa: 'پیچیدگی را قابل فهم کن', ar: 'حوّل التعقيد إلى صياغة مفهومة', tr: 'Karmaşıklığı anlaşılır kıl', ru: 'Переведи сложность в понятный вид', ckb: 'ئاڵۆزییەکە تێگەیشتنپەذیر بکە', kmr: 'Aloziyê têgihiştinbar bike', pl: 'Przełóż złożoność na zrozumiały język', ro: 'Transformă complexitatea în ceva inteligibil', sq: 'Kthe kompleksitetin në të kuptueshëm' },
  target: { de: 'Oeffentlich sauber kommunizieren', en: 'Communicate cleanly in public', fa: 'در فضای عمومی دقیق و مسئولانه ارتباط برقرار کن', ar: 'تواصل في العلن بدقة ومسؤولية', tr: 'Kamusal alanda temiz ve sorumlu iletişim kur', ru: 'Коммуницируй публично точно и ответственно', ckb: 'لە گشتییدا بە وردی و بەرپرسیارییەوە پەیوەندی بکە', kmr: 'Li qada giştî bi daqîqî û berpirsiyarî ragihîne', pl: 'Komunikuj publicznie precyzyjnie i odpowiedzialnie', ro: 'Comunică public precis și responsabil', sq: 'Komuniko publikisht me saktësi dhe përgjegjësi' },
  transfer: { de: 'Risiko ohne Alarm formulieren', en: 'Formulate risk without alarm', fa: 'ریسک را بدون ایجاد وحشت بیان کن', ar: 'صغ الخطر من دون إثارة هلع', tr: 'Riski panik yaratmadan ifade et', ru: 'Сформулируй риск без паники', ckb: 'مەترسی بەبێ ترساندن دەرببڕە', kmr: 'Rîskê bê panîk bibêje', pl: 'Sformułuj ryzyko bez alarmu', ro: 'Formulează riscul fără alarmă', sq: 'Formulo rrezikun pa alarm' },
  review: { de: 'Verstaendlichkeit und Verantwortung pruefen', en: 'Check clarity and responsibility', fa: 'فهم‌پذیری و مسئولیت‌پذیری را بررسی کن', ar: 'افحص الوضوح والمسؤولية', tr: 'Anlaşılırlığı ve sorumluluğu kontrol et', ru: 'Проверь понятность и ответственность', ckb: 'تێگەیشتنپەذیری و بەرپرسیارێتی بپشکنە', kmr: 'Têgihiştinbarî û berpirsiyarî kontrol bike', pl: 'Sprawdź zrozumiałość i odpowiedzialność', ro: 'Verifică inteligibilitatea și responsabilitatea', sq: 'Kontrollo kuptueshmërinë dhe përgjegjësinë' }
};

const items = [
  ['c2-digitale-innovation-kritisch-einordnen','digitale Innovation','digital innovation','نوآوری دیجیتال','ابتكار رقمي','dijital yenilik','цифровая инновация','داهێنانی دیجیتاڵی','nûbûneke dîjîtal','innowacja cyfrowa','inovație digitală','inovacion digjital','c2-register-and-syntactic-choice','roleplay','c2-eine-digitale-innovation-kritisch-einordnen'],
  ['c2-algorithmischen-fehler-verstaendlich-erklaeren','algorithmischen Fehler','algorithmic error','خطای الگوریتمی','خطأ خوارزمي','algoritmik hata','алгоритмическая ошибка','هەڵەی ئەلگۆریتمی','şaşiya algorîtmî','błąd algorytmiczny','eroare algoritmică','gabim algoritmik','c2-nominal-style-in-expert-texts','roleplay','c2-einen-algorithmischen-fehler-verstaendlich-erklaeren'],
  ['c2-datenethik-im-produktteam-diskutieren','Datenethik im Produktteam','data ethics in a product team','اخلاق داده در تیم محصول','أخلاقيات البيانات في فريق منتج','ürün ekibinde veri etiği','этика данных в продуктовой команде','ئەخلاقی داتا لە تیمی بەرهەمدا','etîka daneyan di tîma berhemê de','etyka danych w zespole produktu','etica datelor într-o echipă de produs','etika e të dhënave në ekip produkti','c2-c2-debate-grammar','roleplay','c2-ueber-datenethik-in-einem-produktteam-diskutieren'],
  ['c2-sicherheitsluecke-ohne-panik-kommunizieren','Sicherheitsluecke','security vulnerability','آسیب‌پذیری امنیتی','ثغرة أمنية','güvenlik açığı','уязвимость безопасности','کەلێنی ئاسایش','valahiya ewlehiyê','luka bezpieczeństwa','vulnerabilitate de securitate','dobësi sigurie','c2-journalistic-compression','roleplay','c2-eine-sicherheitsluecke-ohne-panik-kommunizieren'],
  ['c2-redaktionelle-entscheidung-begruenden','redaktionelle Entscheidung','editorial decision','تصمیم تحریریه','قرار تحريري','editoryal karar','редакционное решение','بڕیاری دەستکاریکردنی میدیا','biryarake edîtorî','decyzja redakcyjna','decizie editorială','vendim redaksional','c2-c2-formal-writing-grammar','roleplay','c2-eine-redaktionelle-entscheidung-begruenden'],
  ['c2-kommunikationsfehler-oeffentlich-korrigieren','Kommunikationsfehler','communication mistake','خطای ارتباطی','خطأ تواصلي','iletişim hatası','коммуникационная ошибка','هەڵەی پەیوەندی','şaşiya ragihandinê','błąd komunikacyjny','eroare de comunicare','gabim komunikimi','c2-c2-common-pitfalls','roleplay','c2-einen-kommunikationsfehler-oeffentlich-korrigieren'],
  ['c2-oeffentlichen-vorwurf-sachlich-einordnen','oeffentlichen Vorwurf','public accusation','اتهام یا انتقاد عمومی','اتهام علني','kamusal suçlama','публичное обвинение','تۆمەتێکی گشتی','tawanbariya giştî','publiczny zarzut','acuzație publică','akuzë publike','c2-advanced-passive-and-agent-omission','roleplay','c2-einen-oeffentlichen-vorwurf-sachlich-einordnen'],
  ['c2-entscheidung-unter-unsicherheit-kommunizieren','Entscheidung unter Unsicherheit','decision under uncertainty','تصمیم در شرایط ابهام','قرار في ظل عدم اليقين','belirsizlik altında karar','решение в условиях неопределенности','بڕیار لە ژێر نادڵنیایی','biryar di bin nediyarbûnê de','decyzja w niepewności','decizie în incertitudine','vendim në pasiguri','c2-subtle-modal-verb-nuance','roleplay','c2-eine-entscheidung-unter-unsicherheit-kommunizieren'],
  ['c2-verfahrene-diskussion-mit-fazit-schliessen','verfahrene Diskussion','stuck discussion','بحث گیرکرده و بی‌نتیجه','نقاش عالق','kilitlenmiş tartışma','застрявшая дискуссия','گفتوگۆی گیرخواردوو','nîqaşa asê mayî','zablokowana dyskusja','discuție blocată','diskutim i bllokuar','c2-fine-differences-in-connectors','roleplay','c2-eine-verfahrene-diskussion-mit-einem-fazit-schliessen'],
  ['c2-digitalisierung-technik-und-oeffentliche-kommunikation-wiederholen','Digitalisierung, Technik und oeffentliche Kommunikation','digitalization, technology, and public communication','دیجیتال‌سازی، فناوری و ارتباطات عمومی','الرقمنة والتقنية والتواصل العام','dijitalleşme, teknoloji ve kamusal iletişim','цифровизация, техника и публичная коммуникация','دیجیتاڵکردن، تەکنیک و پەیوەندی گشتی','dîjîtalbûn, teknolojî û ragihandina giştî','cyfryzacja, technologia i komunikacja publiczna','digitalizare, tehnologie și comunicare publică','digjitalizim, teknologji dhe komunikim publik','c2-c2-grammar-review-map','writing-template','c2-technischen-fehler-ohne-panik-kommunizieren']
].map(([slug,de,en,fa,ar,tr,ru,ckb,kmr,pl,ro,sq,grammar,targetType,targetSlug]) => ({
  slug,
  topic: { de, en, fa, ar, tr, ru, ckb, kmr, pl, ro, sq },
  grammar,
  targetType,
  targetSlug
}));

function tr(map) { return langs.map(language => ({ language, text: map[language] })); }

function createActivity(kind, titleKey, instructionMap, targetType, targetSlug, minutes, sortOrder, isRequired = true) {
  return { kind, title: titles[titleKey].de, titleTranslations: tr(titles[titleKey]), instruction: instructionMap.de, instructionTranslations: tr(instructionMap), targetType, targetSlug: targetType === 'none' ? null : targetSlug, estimatedMinutes: minutes, sortOrder, isRequired };
}

function orientInstruction(item) {
  return {
    de: `Lies die Lesson und notiere bei ${item.topic.de}: Was ist technisch gesichert, was ist Interpretation, und was muss oeffentlich verantwortet werden?`,
    en: `Read the lesson and note for ${item.topic.en}: what is technically confirmed, what is interpretation, and what must be publicly accountable?`,
    fa: `درس را بخوان و برای موضوع ${item.topic.fa} یادداشت کن: چه چیزی از نظر فنی قطعی است، چه چیزی تفسیر است، و چه چیزی باید در برابر عموم مسئولانه توضیح داده شود؟`,
    ar: `اقرأ الدرس وسجّل في موضوع ${item.topic.ar}: ما المؤكد تقنيًا، وما هو تفسير، وما الذي يجب تحمله علنًا؟`,
    tr: `Dersi oku ve ${item.topic.tr} için not et: teknik olarak ne kesin, ne yorum, ne de kamusal sorumluluk gerektiriyor?`,
    ru: `Прочитай урок и отметь по теме ${item.topic.ru}: что технически подтверждено, что является интерпретацией и за что нужно отвечать публично?`,
    ckb: `وانەکە بخوێنەوە و بۆ ${item.topic.ckb} بنووسە: چی تەکنیکییە و پشتڕاستکراوە، چی لێکدانەوەیە، و چی دەبێت بە بەرپرسیارێتی بۆ گشتی ڕوون بکرێتەوە؟`,
    kmr: `Dersê bixwîne û ji bo ${item.topic.kmr} binivîse: çi teknîkî piştrast e, çi şîrove ye, û çi divê bi berpirsiyarî ji bo giştî were ravekirin?`,
    pl: `Przeczytaj lekcję i zanotuj przy temacie ${item.topic.pl}: co jest technicznie potwierdzone, co jest interpretacją i za co trzeba publicznie odpowiadać?`,
    ro: `Citește lecția și notează pentru ${item.topic.ro}: ce este confirmat tehnic, ce este interpretare și ce trebuie asumat public?`,
    sq: `Lexoje mësimin dhe shëno për ${item.topic.sq}: çfarë është teknikisht e vërtetuar, çfarë është interpretim dhe çfarë duhet shpjeguar publikisht me përgjegjësi?`
  };
}

function languageInstruction(item) {
  return {
    de: `Oeffne den verlinkten Grammatikpunkt. Waehle eine Formulierung, die technische Genauigkeit in verstaendliche Sprache uebersetzt, ohne Sicherheit vorzutäuschen.`,
    en: `Open the linked grammar point. Choose one formulation that translates technical precision into understandable language without pretending certainty.`,
    fa: `نکته گرامری لینک‌شده را باز کن. یک فرمول‌بندی انتخاب کن که دقت فنی را به زبان قابل فهم تبدیل کند، بدون اینکه قطعیت غیرواقعی نشان بدهد.`,
    ar: `افتح نقطة القواعد المرتبطة. اختر صياغة تنقل الدقة التقنية إلى لغة مفهومة من دون ادعاء يقين غير موجود.`,
    tr: `Bağlantılı dil bilgisi noktasını aç. Teknik kesinliği anlaşılır dile çeviren ama sahte kesinlik göstermeyen bir ifade seç.`,
    ru: `Открой связанный грамматический пункт. Выбери формулировку, которая переводит техническую точность в понятный язык, не изображая ложную уверенность.`,
    ckb: `خاڵی ڕێزمانی بەستەرکراو بکەرەوە. داڕشتنێک هەڵبژێرە کە وردی تەکنیکی دەکاتە زمانێکی تێگەیشتنپەذیر بەبێ پیشاندانی دڵنیایی ساختە.`,
    kmr: `Xala rêzimanê ya girêdayî veke. Formulasyonek hilbijêre ku daqîqiya teknîkî dike zimanekî têgihiştinbar bêyî ku ewlehiya derewîn nîşan bide.`,
    pl: `Otwórz podlinkowany punkt gramatyczny. Wybierz sformułowanie, które przekłada techniczną precyzję na zrozumiały język bez udawania pewności.`,
    ro: `Deschide punctul de gramatică legat. Alege o formulare care traduce precizia tehnică în limbaj inteligibil fără să simuleze certitudine.`,
    sq: `Hap pikën gramatikore të lidhur. Zgjidh një formulim që e kthen saktësinë teknike në gjuhë të kuptueshme pa shtirur siguri.`
  };
}

function targetInstruction(item) {
  return {
    de: `Bearbeite die verlinkte Ressource. Pruefe danach, ob du bei ${item.topic.de} Risiko, Ursache und naechsten Schritt klar trennst.`,
    en: `Work through the linked resource. Then check whether you clearly separate risk, cause, and next step in ${item.topic.en}.`,
    fa: `منبع لینک‌شده را انجام بده. بعد بررسی کن آیا در موضوع ${item.topic.fa} ریسک، علت و قدم بعدی را روشن از هم جدا کرده‌ای.`,
    ar: `اعمل على المورد المرتبط. ثم تحقق هل فصلت بوضوح بين الخطر والسبب والخطوة التالية في موضوع ${item.topic.ar}.`,
    tr: `Bağlantılı kaynağı çalış. Sonra ${item.topic.tr} konusunda risk, neden ve sonraki adımı açıkça ayırıp ayırmadığını kontrol et.`,
    ru: `Проработай связанный ресурс. Затем проверь, ясно ли ты разделяешь риск, причину и следующий шаг в теме ${item.topic.ru}.`,
    ckb: `سەرچاوەی بەستەرکراو کاربکە. پاشان بپشکنە ئایا لە بابەتی ${item.topic.ckb} مەترسی، هۆکار و هەنگاوی داهاتووت بە ڕوونی لێک جیا کردووە.`,
    kmr: `Çavkaniya girêdayî bixebitîne. Paşê kontrol bike gelo di mijara ${item.topic.kmr} de rîsk, sedem û gava din bi zelalî ji hev cuda kirî.`,
    pl: `Przerób podlinkowany materiał. Potem sprawdź, czy w temacie ${item.topic.pl} jasno oddzielasz ryzyko, przyczynę i następny krok.`,
    ro: `Lucrează resursa legată. Apoi verifică dacă în tema ${item.topic.ro} separi clar riscul, cauza și pasul următor.`,
    sq: `Puno me burimin e lidhur. Pastaj kontrollo nëse në temën ${item.topic.sq} ndan qartë rrezikun, shkakun dhe hapin tjetër.`
  };
}

function transferInstruction() {
  return {
    de: 'Schreibe ein kurzes oeffentliches Update: Was ist passiert? Was wissen wir sicher? Was pruefen wir noch? Was ist der naechste konkrete Schritt?',
    en: 'Write a short public update: what happened? What do we know for certain? What are we still checking? What is the next concrete step?',
    fa: 'یک اطلاع‌رسانی کوتاه عمومی بنویس: چه اتفاقی افتاده؟ چه چیزی را با قطعیت می‌دانیم؟ چه چیزی هنوز در حال بررسی است؟ قدم مشخص بعدی چیست؟',
    ar: 'اكتب تحديثًا عامًا قصيرًا: ماذا حدث؟ ما الذي نعرفه يقينًا؟ ما الذي ما زلنا نتحقق منه؟ ما الخطوة التالية المحددة؟',
    tr: 'Kısa bir kamusal güncelleme yaz: Ne oldu? Neyi kesin biliyoruz? Neyi hâlâ kontrol ediyoruz? Sonraki somut adım nedir?',
    ru: 'Напиши короткое публичное обновление: что произошло? Что мы знаем точно? Что еще проверяем? Какой следующий конкретный шаг?',
    ckb: 'نوێکردنەوەیەکی کورتی گشتی بنووسە: چی ڕوویدا؟ چی بە دڵنیایی دەزانین؟ چی هێشتا دەپشکنین؟ هەنگاوی دیاریکراوی داهاتوو چییە؟',
    kmr: 'Nûvekirineke kurt a giştî binivîse: Çi qewimî? Em çi bi ewlehî dizanin? Em hîn çi kontrol dikin? Gava konkret a din çi ye?',
    pl: 'Napisz krótką publiczną aktualizację: co się stało? Co wiemy na pewno? Co jeszcze sprawdzamy? Jaki jest następny konkretny krok?',
    ro: 'Scrie o actualizare publică scurtă: ce s-a întâmplat? Ce știm sigur? Ce verificăm încă? Care este următorul pas concret?',
    sq: 'Shkruaj një përditësim të shkurtër publik: çfarë ndodhi? Çfarë dimë me siguri? Çfarë po verifikojmë ende? Cili është hapi konkret tjetër?'
  };
}

function reviewInstruction(item) {
  return {
    de: `Pruefe dein Update zu ${item.topic.de}: Ist es verstaendlich fuer Laien? Ist es fair gegenueber Betroffenen? Entferne eine Formulierung, die Panik oder falsche Sicherheit erzeugt.`,
    en: `Check your update on ${item.topic.en}: is it understandable for non-specialists? Is it fair to those affected? Remove one formulation that creates panic or false certainty.`,
    fa: `اطلاع‌رسانی خودت درباره ${item.topic.fa} را بررسی کن: آیا برای افراد غیرمتخصص قابل فهم است؟ آیا نسبت به افراد درگیر منصفانه است؟ یک عبارت را که وحشت یا قطعیت کاذب ایجاد می‌کند حذف کن.`,
    ar: `افحص تحديثك عن ${item.topic.ar}: هل هو مفهوم لغير المتخصصين؟ هل هو منصف تجاه المتضررين؟ احذف صياغة تخلق هلعًا أو يقينًا زائفًا.`,
    tr: `${item.topic.tr} hakkındaki güncellemeni kontrol et: uzman olmayanlar için anlaşılır mı? Etkilenenlere karşı adil mi? Panik ya da sahte kesinlik yaratan bir ifadeyi çıkar.`,
    ru: `Проверь обновление по теме ${item.topic.ru}: понятно ли оно неспециалистам? Справедливо ли к затронутым людям? Убери формулировку, создающую панику или ложную уверенность.`,
    ckb: `نوێکردنەوەکەت دەربارەی ${item.topic.ckb} بپشکنە: ئایا بۆ کەسانی ناپسۆر تێگەیشتنپەذیرە؟ ئایا بۆ کەسانی کاریگەرکراو دادپەروەرە؟ دەربڕینێک بسڕەوە کە ترس یان دڵنیایی ساختە دروست دەکات.`,
    kmr: `Nûvekirina xwe li ser ${item.topic.kmr} kontrol bike: ji bo kesên nepispor têgihiştinbar e? Li hember kesên bandorkirî adil e? Gotinekê jê bibe ku panîk an ewlehiya derewîn çê dike.`,
    pl: `Sprawdź aktualizację o ${item.topic.pl}: czy jest zrozumiała dla niespecjalistów? Czy jest uczciwa wobec osób dotkniętych? Usuń sformułowanie tworzące panikę lub fałszywą pewność.`,
    ro: `Verifică actualizarea despre ${item.topic.ro}: este inteligibilă pentru nespecialiști? Este corectă față de cei afectați? Elimină o formulare care creează panică sau falsă certitudine.`,
    sq: `Kontrollo përditësimin për ${item.topic.sq}: a kuptohet nga jo-specialistët? A është i drejtë ndaj të prekurve? Hiq një formulim që krijon panik ose siguri të rreme.`
  };
}

for (const item of items) {
  const lesson = lessons.find(l => l.slug === item.slug);
  if (!lesson) throw new Error(`Lesson not found: ${item.slug}`);
  const kind = item.targetType === 'writing-template' ? 'write' : 'roleplay';
  lesson.activityBlocks = [
    createActivity('read', 'orient', orientInstruction(item), 'none', null, 6, 10),
    createActivity('grammar', 'language', languageInstruction(item), 'grammar-topic', item.grammar, 8, 20),
    createActivity(kind, 'target', targetInstruction(item), item.targetType, item.targetSlug, 10, 30),
    createActivity('practice', 'transfer', transferInstruction(), 'none', null, 8, 40),
    createActivity('review', 'review', reviewInstruction(item), 'none', null, 6, 50)
  ];
}

fs.writeFileSync(file, JSON.stringify(data, null, 2) + '\n', 'utf8');
console.log(`Updated ${items.length} C2 Module 11 lessons with ${items.length * 5} activity blocks.`);

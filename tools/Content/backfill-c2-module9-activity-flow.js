const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c2-stil-souveraenitaet-und-komplexer-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'Soziale Bedeutung erkennen',
    en: 'Recognize the social meaning',
    fa: 'معنای اجتماعی را تشخیص بده',
    ar: 'تعرّف على المعنى الاجتماعي',
    tr: 'Sosyal anlamı fark et',
    ru: 'Распознай социальный смысл',
    ckb: 'مانای کۆمەڵایەتی بناسە',
    kmr: 'Wateya civakî nas bike',
    pl: 'Rozpoznaj znaczenie społeczne',
    ro: 'Recunoaște sensul social',
    sq: 'Dallo kuptimin shoqëror'
  },
  language: {
    de: 'Andeutung und Grenze steuern',
    en: 'Control implication and boundary',
    fa: 'اشاره غیرمستقیم و مرز را کنترل کن',
    ar: 'اضبط التلميح والحد',
    tr: 'İmayı ve sınırı yönet',
    ru: 'Управляй намеком и границей',
    ckb: 'ئاماژەی ناڕاستەوخۆ و سنوور بەڕێوەببە',
    kmr: 'Îma û sînorê birêve bibe',
    pl: 'Kontroluj aluzję i granicę',
    ro: 'Controlează sugestia și limita',
    sq: 'Kontrollo nënkuptimin dhe kufirin'
  },
  target: {
    de: 'Beziehung praktisch klaeren',
    en: 'Clarify the relationship in practice',
    fa: 'رابطه را در عمل روشن کن',
    ar: 'وضّح العلاقة عمليًا',
    tr: 'İlişkiyi pratikte netleştir',
    ru: 'Практически проясни отношения',
    ckb: 'پەیوەندییەکە بە کرداری ڕوون بکە',
    kmr: 'Têkiliyê di pratîkê de zelal bike',
    pl: 'Praktycznie wyjaśnij relację',
    ro: 'Clarifică relația practic',
    sq: 'Qartëso marrëdhënien në praktikë'
  },
  transfer: {
    de: 'Satz mit doppelter Wirkung schreiben',
    en: 'Write a sentence with a double effect',
    fa: 'جمله‌ای بنویس که هم روشن باشد هم رابطه را حفظ کند',
    ar: 'اكتب جملة واضحة وتحافظ على العلاقة',
    tr: 'Hem açık hem ilişkiyi koruyan bir cümle yaz',
    ru: 'Напиши фразу, которая ясна и сохраняет отношения',
    ckb: 'ڕستەیەک بنووسە کە هەم ڕوون بێت و هەم پەیوەندی بپارێزێت',
    kmr: 'Hevokek binivîse ku hem zelal be hem têkilî biparêze',
    pl: 'Napisz zdanie jasne i chroniące relację',
    ro: 'Scrie o propoziție clară și care păstrează relația',
    sq: 'Shkruaj një fjali të qartë që ruan marrëdhënien'
  },
  review: {
    de: 'Wirkung aus zwei Perspektiven pruefen',
    en: 'Check the effect from two perspectives',
    fa: 'اثر جمله را از دو نگاه بررسی کن',
    ar: 'افحص أثر الجملة من منظورين',
    tr: 'Cümlenin etkisini iki bakıştan kontrol et',
    ru: 'Проверь эффект фразы с двух точек зрения',
    ckb: 'کاریگەری ڕستەکە لە دوو دیدەوە بپشکنە',
    kmr: 'Bandora hevokê ji du perspektîfan kontrol bike',
    pl: 'Sprawdź efekt zdania z dwóch perspektyw',
    ro: 'Verifică efectul propoziției din două perspective',
    sq: 'Kontrollo efektin e fjalisë nga dy këndvështrime'
  }
};

const items = [
  {
    slug: 'c2-kulturelle-irritation-in-freundschaft-klaeren',
    topic: { de: 'eine kulturelle Irritation in einer Freundschaft', en: 'a cultural irritation in a friendship', fa: 'یک سوءتفاهم یا ناراحتی فرهنگی در دوستی', ar: 'انزعاج أو سوء فهم ثقافي في صداقة', tr: 'arkadaşlıkta kültürel bir rahatsızlık', ru: 'культурное недоразумение в дружбе', ckb: 'ناڕەحەتییەکی کەلتووری لە هاوڕێیەتیدا', kmr: 'nerehetiyeke çandî di hevaltiyê de', pl: 'kulturowe nieporozumienie w przyjaźni', ro: 'o iritare culturală într-o prietenie', sq: 'një keqkuptim kulturor në miqësi' },
    focus: { de: 'Irritation zu benennen, ohne die Person oder ihre Herkunft festzulegen', en: 'naming irritation without defining the person or their background', fa: 'ناراحتی را بیان کنی، بدون اینکه فرد یا فرهنگ او را برچسب بزنی', ar: 'تسمية الانزعاج من دون وصم الشخص أو ثقافته', tr: 'rahatsızlığı adlandırmak ama kişiyi ya da kökenini etiketlememek', ru: 'назвать раздражение, не навешивая ярлык на человека или его происхождение', ckb: 'ناڕەحەتییەکە ناوببەیت بەبێ نیشانەکردنی کەسەکە یان ڕەچەڵەکی', kmr: 'nerehetiyê nav bikî bêyî ku kes an paşxaneya wî etîket bikî', pl: 'nazwać irytację bez etykietowania osoby lub jej pochodzenia', ro: 'să numești iritarea fără să etichetezi persoana sau originea ei', sq: 'ta emërtosh shqetësimin pa etiketuar personin ose prejardhjen e tij' },
    grammar: 'c2-irony-and-indirectness-in-syntax',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-kulturelle-irritation-in-einer-freundschaft-klaeren'
  },
  {
    slug: 'c2-grenze-in-intensiver-beziehung-setzen',
    topic: { de: 'eine Grenze in einer intensiven Beziehung', en: 'a boundary in an intense relationship', fa: 'یک مرز در رابطه نزدیک و پرتنش', ar: 'حد في علاقة مكثفة', tr: 'yoğun bir ilişkide sınır', ru: 'граница в интенсивных отношениях', ckb: 'سنوورێک لە پەیوەندییەکی چڕدا', kmr: 'sînorek di têkiliyeke xurt de', pl: 'granica w intensywnej relacji', ro: 'o limită într-o relație intensă', sq: 'një kufi në një marrëdhënie intensive' },
    focus: { de: 'Nahe zuzulassen und trotzdem eine klare Grenze zu setzen', en: 'allowing closeness while still setting a clear boundary', fa: 'نزدیکی را انکار نکنی اما مرز روشن بگذاری', ar: 'السماح بالقرب مع وضع حد واضح', tr: 'yakınlığı kabul edip yine de açık sınır koymak', ru: 'допустить близость и при этом поставить ясную границу', ckb: 'نزیکی قبوڵ بکەیت و هەمان کات سنوورێکی ڕوون دابنێیت', kmr: 'nêzîkbûnê qebûl bikî û di heman demê de sînorekî zelal danî', pl: 'dopuścić bliskość, a jednocześnie postawić jasną granicę', ro: 'să permiți apropierea și totuși să stabilești o limită clară', sq: 'të lejosh afërsinë dhe njëkohësisht të vendosësh kufi të qartë' },
    grammar: 'c2-subtle-modal-verb-nuance',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-grenze-in-einer-intensiven-beziehung-setzen'
  },
  {
    slug: 'c2-ironische-bemerkung-richtig-einordnen',
    topic: { de: 'eine ironische Bemerkung', en: 'an ironic remark', fa: 'یک جمله کنایه‌آمیز یا طنزآلود', ar: 'ملاحظة ساخرة أو ضمنية', tr: 'ironik bir söz', ru: 'ироническое замечание', ckb: 'تێبینییەکی گاڵتەئامێز', kmr: 'gotineke îronîk', pl: 'ironiczna uwaga', ro: 'o remarcă ironică', sq: 'një koment ironik' },
    focus: { de: 'nicht vorschnell verletzt zu reagieren und trotzdem den Sinn zu klaeren', en: 'not reacting as hurt too quickly while still clarifying the meaning', fa: 'زود واکنش رنجیده نشان ندهی و در عین حال معنی جمله را روشن کنی', ar: 'ألا تتصرف بسرعة كأنك مجروح، مع توضيح المعنى', tr: 'hemen kırılmış gibi tepki vermemek ama anlamı açıklığa kavuşturmak', ru: 'не реагировать сразу как обиженный, но прояснить смысл', ckb: 'زۆر بە پەلە وەک ڕەنجاو وەڵام نەدەیتەوە و هەمان کات ماناکە ڕوون بکەیت', kmr: 'zû wek birîndar bersiv nedî û di heman demê de wateyê zelal bikî', pl: 'nie reagować od razu jak zraniony, ale wyjaśnić sens', ro: 'să nu reacționezi prea repede ca rănit, dar să clarifici sensul', sq: 'të mos reagosh menjëherë si i lënduar dhe prapë të qartësosh kuptimin' },
    grammar: 'c2-irony-and-indirectness-in-syntax',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-ironische-bemerkung-richtig-einordnen'
  },
  {
    slug: 'c2-entschuldigung-mit-verantwortung-formulieren',
    topic: { de: 'eine Entschuldigung mit Verantwortung', en: 'an apology with responsibility', fa: 'عذرخواهی همراه با مسئولیت‌پذیری', ar: 'اعتذار مع تحمل المسؤولية', tr: 'sorumluluk içeren özür', ru: 'извинение с ответственностью', ckb: 'داوای لێبووردن لەگەڵ بەرپرسیارێتی', kmr: 'lêborîn bi berpirsiyarî', pl: 'przeprosiny z odpowiedzialnością', ro: 'o scuză cu asumarea responsabilității', sq: 'një kërkimfalje me përgjegjësi' },
    focus: { de: 'Verantwortung zu uebernehmen, ohne dich selbst theatralisch in den Mittelpunkt zu stellen', en: 'taking responsibility without theatrically putting yourself at the center', fa: 'مسئولیت را بپذیری، بدون اینکه خودت را نمایشی در مرکز ماجرا قرار بدهی', ar: 'تحمل المسؤولية من دون جعل نفسك مركز الموقف بشكل تمثيلي', tr: 'sorumluluk almak ama kendini teatral biçimde merkeze koymamak', ru: 'взять ответственность, не ставя себя театрально в центр', ckb: 'بەرپرسیارێتی وەربگریت بەبێ ئەوەی خۆت بە شانۆیی بکەیتە ناوەندی بابەتەکە', kmr: 'berpirsiyarî bigirî bêyî ku xwe bi şanoyî bixe navendê', pl: 'wziąć odpowiedzialność bez teatralnego stawiania siebie w centrum', ro: 'să îți asumi responsabilitatea fără să te pui teatral în centru', sq: 'të marrësh përgjegjësi pa e vendosur veten në qendër në mënyrë teatrale' },
    grammar: 'c2-register-and-syntactic-choice',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-entschuldigung-mit-verantwortung-formulieren'
  },
  {
    slug: 'c2-gruppendynamik-im-verein-moderieren',
    topic: { de: 'Gruppendynamik im Verein', en: 'group dynamics in an association', fa: 'دینامیک گروهی در یک انجمن یا جمع داوطلبانه', ar: 'ديناميكية المجموعة في جمعية', tr: 'dernekte grup dinamiği', ru: 'групповая динамика в объединении', ckb: 'دینامیکی گرووپ لە کۆمەڵەدا', kmr: 'dînamîka komê di komeleyê de', pl: 'dynamika grupy w stowarzyszeniu', ro: 'dinamica de grup într-o asociație', sq: 'dinamika e grupit në një shoqatë' },
    focus: { de: 'Spannung sichtbar zu machen, ohne Lagerbildung zu verstaerken', en: 'making tension visible without strengthening camps', fa: 'تنش را آشکار کنی، بدون اینکه گروه‌بندی و جناح‌سازی را شدیدتر کنی', ar: 'إظهار التوتر من دون تعزيز الانقسام إلى معسكرات', tr: 'gerilimi görünür kılmak ama kamplaşmayı güçlendirmemek', ru: 'сделать напряжение видимым, не усиливая лагеря', ckb: 'گرژی دیار بکەیت بەبێ بەهێزکردنی جبهەبەندی', kmr: 'tengezarî xuya bikî bêyî ku alîgirî xurt bikî', pl: 'pokazać napięcie bez wzmacniania podziałów na obozy', ro: 'să faci tensiunea vizibilă fără să întărești taberele', sq: 'ta bësh të dukshëm tensionin pa forcuar ndarjen në kampe' },
    grammar: 'c2-c2-debate-grammar',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-gruppendynamik-im-verein-moderieren'
  },
  {
    slug: 'c2-tabuthema-umsichtig-ansprechen',
    topic: { de: 'ein Tabuthema im privaten Kontext', en: 'a taboo topic in a private context', fa: 'یک موضوع حساس یا تابو در فضای خصوصی', ar: 'موضوع حساس أو محظور في سياق خاص', tr: 'özel bağlamda tabu bir konu', ru: 'табуированная тема в личном контексте', ckb: 'بابەتێکی قەدەغە یان هەستیار لە چوارچێوەی تایبەتدا', kmr: 'mijareke tabu an hesas di çarçoveya taybet de', pl: 'temat tabu w prywatnym kontekście', ro: 'un subiect tabu într-un context privat', sq: 'një temë tabu në kontekst privat' },
    focus: { de: 'Vorsicht zu zeigen, ohne das Thema aus Angst unklar zu lassen', en: 'showing caution without leaving the topic unclear out of fear', fa: 'احتیاط نشان بدهی، بدون اینکه از ترس موضوع را مبهم رها کنی', ar: 'إظهار الحذر من دون ترك الموضوع غامضًا بسبب الخوف', tr: 'dikkat göstermek ama korkudan konuyu belirsiz bırakmamak', ru: 'проявить осторожность, не оставляя тему неясной из страха', ckb: 'وریایی پیشان بدەیت بەبێ ئەوەی لە ترسدا بابەتەکە ناڕوون بهێڵیتەوە', kmr: 'hişyariyê nîşan bidî bêyî ku ji tirsê mijarê nezelal bihêlî', pl: 'okazać ostrożność bez pozostawiania tematu niejasnym ze strachu', ro: 'să arăți prudență fără să lași tema neclară din teamă', sq: 'të tregosh kujdes pa e lënë temën të paqartë nga frika' },
    grammar: 'c2-advanced-modal-particles',
    targetType: 'roleplay',
    targetSlug: 'c2-ein-tabuthema-im-privaten-kontext-umsichtig-ansprechen'
  },
  {
    slug: 'c2-mehrdeutige-einladung-diplomatisch-klaeren',
    topic: { de: 'eine mehrdeutige Einladung', en: 'an ambiguous invitation', fa: 'یک دعوت مبهم', ar: 'دعوة ملتبسة', tr: 'belirsiz bir davet', ru: 'неоднозначное приглашение', ckb: 'بانگهێشتێکی ناڕوون', kmr: 'vexwendineke nezelal', pl: 'niejednoznaczne zaproszenie', ro: 'o invitație ambiguă', sq: 'një ftesë e paqartë' },
    focus: { de: 'Absicht zu klaeren, ohne der anderen Person Peinlichkeit zu bereiten', en: 'clarifying intention without embarrassing the other person', fa: 'قصد طرف مقابل را روشن کنی، بدون اینکه او را معذب یا شرمنده کنی', ar: 'توضيح نية الطرف الآخر من دون إحراجه', tr: 'karşı tarafın niyetini netleştirmek ama onu utandırmamak', ru: 'прояснить намерение, не ставя другого человека в неловкое положение', ckb: 'مەبەستی کەسی بەرامبەر ڕوون بکەیت بەبێ شەرمەزارکردنی', kmr: 'mebesta kesê din zelal bikî bêyî ku wî/wê şerm bikevî', pl: 'wyjaśnić intencję bez zawstydzania drugiej osoby', ro: 'să clarifici intenția fără să stânjenești cealaltă persoană', sq: 'të qartësosh qëllimin pa e vënë personin tjetër në siklet' },
    grammar: 'c2-ambiguity-and-disambiguation',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-mehrdeutige-einladung-diplomatisch-klaeren'
  },
  {
    slug: 'c2-gesichtsverlust-in-privaten-gespraechen-vermeiden',
    topic: { de: 'Gesichtsverlust in privaten Gespraechen', en: 'loss of face in private conversations', fa: 'حفظ آبرو در گفت‌وگوهای خصوصی', ar: 'حفظ ماء الوجه في محادثات خاصة', tr: 'özel konuşmalarda itibar kaybı', ru: 'потеря лица в личных разговорах', ckb: 'لەدەستدانی ڕوو لە گفتوگۆی تایبەتدا', kmr: 'windakirina rû di axaftinên taybet de', pl: 'utrata twarzy w rozmowach prywatnych', ro: 'pierderea prestigiului în conversații private', sq: 'humbja e fytyrës në biseda private' },
    focus: { de: 'eine klare Sache anzusprechen und der anderen Person trotzdem einen Rueckweg zu lassen', en: 'addressing a clear issue while still leaving the other person a way back', fa: 'موضوع را روشن مطرح کنی و هم‌زمان راه برگشت محترمانه برای طرف مقابل بگذاری', ar: 'طرح المسألة بوضوح مع ترك طريق محترم للطرف الآخر', tr: 'konuyu açıkça konuşmak ve yine de karşı tarafa dönüş yolu bırakmak', ru: 'ясно поднять вопрос и при этом оставить другому человеку достойный путь назад', ckb: 'بابەتەکە بە ڕوونی بخەیتەڕوو و هەمان کات ڕێگای گەڕانەوەی بەڕێز بۆ کەسی بەرامبەر بهێڵیتەوە', kmr: 'mijarê bi zelalî bêjî û di heman demê de rêya vegerê ya bi rêz ji bo kesê din bihêlî', pl: 'poruszyć sprawę jasno i jednocześnie zostawić drugiej osobie godną drogę powrotu', ro: 'să abordezi clar problema și totuși să lași celuilalt o cale demnă de revenire', sq: 'ta hapësh qartë çështjen dhe njëkohësisht t’i lësh personit tjetër një rrugë kthimi me dinjitet' },
    grammar: 'c2-fine-differences-in-connectors',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-verhandlung-mit-gesichtsverlust-vermeiden'
  },
  {
    slug: 'c2-kulturell-geladene-redemittel-verstehen',
    topic: { de: 'kulturell geladene Redemittel', en: 'culturally loaded expressions', fa: 'عبارت‌هایی که بار فرهنگی یا اجتماعی دارند', ar: 'تعابير ذات حمولة ثقافية أو اجتماعية', tr: 'kültürel yük taşıyan ifadeler', ru: 'культурно нагруженные выражения', ckb: 'دەربڕینە کەلتووری یان کۆمەڵایەتی بارکراوەکان', kmr: 'gotinên barkirî yên çandî an civakî', pl: 'wyrażenia kulturowo obciążone', ro: 'formulări încărcate cultural', sq: 'shprehje me ngarkesë kulturore' },
    focus: { de: 'die soziale Funktion zu verstehen, statt nur woertlich zu uebersetzen', en: 'understanding the social function instead of only translating literally', fa: 'کارکرد اجتماعی عبارت را بفهمی، نه اینکه فقط کلمه‌به‌کلمه ترجمه کنی', ar: 'فهم الوظيفة الاجتماعية للعبارة بدل ترجمتها حرفيًا فقط', tr: 'ifadeyi yalnızca kelimesi kelimesine çevirmek yerine sosyal işlevini anlamak', ru: 'понять социальную функцию выражения, а не только переводить дословно', ckb: 'کاری کۆمەڵایەتی دەربڕینەکە تێبگەیت نەک تەنها وشە بە وشە وەریبگێڕیت', kmr: 'karê civakî yê gotinê fam bikî ne ku tenê peyv bi peyv wergerînî', pl: 'zrozumieć funkcję społeczną wyrażenia zamiast tłumaczyć je tylko dosłownie', ro: 'să înțelegi funcția socială a expresiei, nu doar traducerea literală', sq: 'të kuptosh funksionin shoqëror të shprehjes, jo vetëm ta përkthesh fjalë për fjalë' },
    grammar: 'c2-advanced-modal-particles',
    targetType: 'writing-template',
    targetSlug: 'c2-kulturell-geladene-formulierung-entschaerfen'
  },
  {
    slug: 'c2-soziale-feinheiten-und-kulturelle-dichte-wiederholen',
    topic: { de: 'soziale Feinheiten und kulturelle Dichte', en: 'social nuance and cultural density', fa: 'ظرافت‌های اجتماعی و لایه‌های فرهنگی', ar: 'الدقة الاجتماعية والكثافة الثقافية', tr: 'sosyal incelikler ve kültürel yoğunluk', ru: 'социальные нюансы и культурная плотность', ckb: 'وردەکاری کۆمەڵایەتی و قووڵی کەلتووری', kmr: 'hûrgiliyên civakî û qelewbûna çandî', pl: 'niuanse społeczne i gęstość kulturowa', ro: 'nuanțe sociale și densitate culturală', sq: 'hollësi shoqërore dhe dendësi kulturore' },
    focus: { de: 'Bedeutung, Beziehung und Grenze in einem kontrollierten Satz zusammenzufuehren', en: 'bringing meaning, relationship, and boundary together in one controlled sentence', fa: 'معنا، رابطه و مرز را در یک جمله کنترل‌شده کنار هم بیاوری', ar: 'جمع المعنى والعلاقة والحد في جملة مضبوطة واحدة', tr: 'anlamı, ilişkiyi ve sınırı kontrollü tek bir cümlede birleştirmek', ru: 'соединить смысл, отношения и границу в одной контролируемой фразе', ckb: 'مانا، پەیوەندی و سنوور لە یەک ڕستەی کۆنترۆڵکراودا پێکەوە بهێنیت', kmr: 'wate, têkilî û sînorê di hevokeke kontrolkirî de bigihînî hev', pl: 'połączyć znaczenie, relację i granicę w jednym kontrolowanym zdaniu', ro: 'să aduci sensul, relația și limita într-o singură propoziție controlată', sq: 'të bashkosh kuptimin, marrëdhënien dhe kufirin në një fjali të kontrolluar' },
    grammar: 'c2-c2-register-review',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-ironie-und-indirekte-wertung-lesen'
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
    de: `Lies die Lesson und markiere, welche soziale Bedeutung bei ${item.topic.de} mittransportiert wird: Naehe, Distanz, Kritik, Humor, Scham oder Respekt.`,
    en: `Read the lesson and mark which social meaning is carried in ${item.topic.en}: closeness, distance, criticism, humor, shame, or respect.`,
    fa: `درس را بخوان و مشخص کن در موضوع ${item.topic.fa} چه معنای اجتماعی همراه جمله منتقل می‌شود: نزدیکی، فاصله، انتقاد، شوخی، شرم یا احترام.`,
    ar: `اقرأ الدرس وحدد أي معنى اجتماعي ينتقل في موضوع ${item.topic.ar}: القرب، المسافة، النقد، الفكاهة، الحرج أو الاحترام.`,
    tr: `Dersi oku ve ${item.topic.tr} konusunda hangi sosyal anlamın taşındığını işaretle: yakınlık, mesafe, eleştiri, mizah, utanç ya da saygı.`,
    ru: `Прочитай урок и отметь, какой социальный смысл несет тема ${item.topic.ru}: близость, дистанцию, критику, юмор, стыд или уважение.`,
    ckb: `وانەکە بخوێنەوە و دیاری بکە لە بابەتی ${item.topic.ckb} چ مانایەکی کۆمەڵایەتی دەگوازرێتەوە: نزیکی، دووری، ڕەخنە، گاڵتە، شەرم یان ڕێز.`,
    kmr: `Dersê bixwîne û nîşan bike di mijara ${item.topic.kmr} de kîjan wateya civakî tê veguhestin: nêzîkbûn, dûrbûn, rexne, henek, şerm an rêz.`,
    pl: `Przeczytaj lekcję i zaznacz, jakie znaczenie społeczne niesie temat ${item.topic.pl}: bliskość, dystans, krytykę, humor, wstyd czy szacunek.`,
    ro: `Citește lecția și marchează ce sens social poartă tema ${item.topic.ro}: apropiere, distanță, critică, umor, rușine sau respect.`,
    sq: `Lexoje mësimin dhe shëno çfarë kuptimi shoqëror bart tema ${item.topic.sq}: afërsi, distancë, kritikë, humor, turp apo respekt.`
  };
}

function languageInstruction(item) {
  return {
    de: `Oeffne den verlinkten Grammatikpunkt. Suche eine Formulierung, die ${item.focus.de}. Achte darauf, ob die Wirkung direkt, indirekt, ironisch oder versoehnlich ist.`,
    en: `Open the linked grammar point. Find a formulation that helps you ${item.focus.en}. Watch whether the effect is direct, indirect, ironic, or conciliatory.`,
    fa: `نکته گرامری لینک‌شده را باز کن. یک فرمول‌بندی پیدا کن که کمک کند ${item.focus.fa}. دقت کن اثر آن مستقیم، غیرمستقیم، کنایه‌آمیز یا آشتی‌دهنده است.`,
    ar: `افتح نقطة القواعد المرتبطة. ابحث عن صياغة تساعدك على أن ${item.focus.ar}. انتبه هل أثرها مباشر أو غير مباشر أو ساخر أو تصالحي.`,
    tr: `Bağlantılı dil bilgisi noktasını aç. ${item.focus.tr} için yardımcı olan bir ifade bul. Etkisinin doğrudan, dolaylı, ironik ya da uzlaştırıcı olup olmadığına dikkat et.`,
    ru: `Открой связанный грамматический пункт. Найди формулировку, которая помогает ${item.focus.ru}. Обрати внимание, действует ли она прямо, косвенно, иронично или примиряюще.`,
    ckb: `خاڵی ڕێزمانی بەستەرکراو بکەرەوە. داڕشتنێک بدۆزەرەوە کە یارمەتیت بدات ${item.focus.ckb}. سەرنج بدە کاریگەرییەکە ڕاستەوخۆ، ناڕاستەوخۆ، گاڵتەئامێز یان ئاشتیکەرەوەیە.`,
    kmr: `Xala rêzimanê ya girêdayî veke. Formulasyonek bibîne ku alîkar e ${item.focus.kmr}. Bala xwe bide ka bandor rast, nerast, îronîk an aştkirinê ye.`,
    pl: `Otwórz podlinkowany punkt gramatyczny. Znajdź sformułowanie, które pomaga ${item.focus.pl}. Zwróć uwagę, czy efekt jest bezpośredni, pośredni, ironiczny czy pojednawczy.`,
    ro: `Deschide punctul de gramatică legat. Caută o formulare care te ajută să ${item.focus.ro}. Urmărește dacă efectul este direct, indirect, ironic sau conciliant.`,
    sq: `Hap pikën gramatikore të lidhur. Gjej një formulim që të ndihmon ${item.focus.sq}. Vëzhgo nëse efekti është i drejtpërdrejtë, i tërthortë, ironik apo pajtues.`
  };
}

function targetInstruction(item) {
  return {
    de: `Bearbeite die verlinkte Ressource. Pruefe danach, ob du Bedeutung und Beziehung gleichzeitig klaerst. Markiere eine Stelle, an der eine woertliche Reaktion die Situation verschlechtern wuerde.`,
    en: `Work through the linked resource. Then check whether you clarify meaning and relationship at the same time. Mark one place where a literal reaction would make the situation worse.`,
    fa: `منبع لینک‌شده را انجام بده. بعد بررسی کن آیا هم معنا را روشن می‌کنی و هم رابطه را حفظ می‌کنی. یک بخش را مشخص کن که واکنش کلمه‌به‌کلمه در آن وضعیت را بدتر می‌کند.`,
    ar: `اعمل على المورد المرتبط. ثم تحقق هل توضح المعنى والعلاقة في الوقت نفسه. حدّد موضعًا تجعل فيه ردة الفعل الحرفية الوضع أسوأ.`,
    tr: `Bağlantılı kaynağı çalış. Sonra hem anlamı hem ilişkiyi aynı anda netleştirip netleştirmediğini kontrol et. Kelimesi kelimesine tepkinin durumu kötüleştireceği bir yeri işaretle.`,
    ru: `Проработай связанный ресурс. Затем проверь, проясняешь ли ты одновременно смысл и отношения. Отметь место, где буквальная реакция ухудшила бы ситуацию.`,
    ckb: `سەرچاوەی بەستەرکراو کاربکە. پاشان بپشکنە ئایا هەم مانا و هەم پەیوەندی بە یەک کات ڕوون دەکەیت. شوێنێک دیاری بکە کە وەڵامی وشە بە وشە دۆخەکە خراپتر دەکات.`,
    kmr: `Çavkaniya girêdayî bixebitîne. Paşê kontrol bike gelo tu di heman demê de hem wate hem têkilî zelal dikî. Cihê ku bersiveke peyv bi peyv rewşê xirabtir dike nîşan bike.`,
    pl: `Przerób podlinkowany materiał. Potem sprawdź, czy jednocześnie wyjaśniasz sens i relację. Zaznacz miejsce, w którym dosłowna reakcja pogorszyłaby sytuację.`,
    ro: `Lucrează resursa legată. Apoi verifică dacă clarifici simultan sensul și relația. Marchează un loc în care o reacție literală ar înrăutăți situația.`,
    sq: `Puno me burimin e lidhur. Pastaj kontrollo nëse qartëson njëkohësisht kuptimin dhe marrëdhënien. Shëno një vend ku reagimi fjalë për fjalë do ta përkeqësonte situatën.`
  };
}

function transferInstruction() {
  return {
    de: 'Schreibe einen Satz, der klar genug ist, aber der anderen Person einen wuerdigen Rueckweg laesst. Danach schreibe eine direktere und eine weichere Variante.',
    en: 'Write one sentence that is clear enough but leaves the other person a dignified way back. Then write one more direct and one softer version.',
    fa: 'یک جمله بنویس که به اندازه کافی روشن باشد اما برای طرف مقابل راه برگشت محترمانه بگذارد. بعد یک نسخه مستقیم‌تر و یک نسخه ملایم‌تر هم بنویس.',
    ar: 'اكتب جملة واضحة بما يكفي لكنها تترك للطرف الآخر طريقًا كريمًا للعودة. ثم اكتب نسخة أكثر مباشرة ونسخة ألطف.',
    tr: 'Yeterince açık ama karşı tarafa onurlu bir dönüş yolu bırakan bir cümle yaz. Sonra daha doğrudan ve daha yumuşak bir versiyon yaz.',
    ru: 'Напиши фразу, достаточно ясную, но оставляющую другому человеку достойный путь назад. Затем напиши более прямой и более мягкий вариант.',
    ckb: 'ڕستەیەک بنووسە کە بە پێویستی ڕوون بێت بەڵام ڕێگای گەڕانەوەی بەڕێز بۆ کەسی بەرامبەر بهێڵێتەوە. پاشان وەشانی ڕاستەوخۆتر و نەرمتر بنووسە.',
    kmr: 'Hevokek binivîse ku têra xwe zelal be lê rêya vegerê ya bi rûmet ji bo kesê din bihêle. Paşê guhertoyeke rasterasttir û yek nermtir binivîse.',
    pl: 'Napisz zdanie wystarczająco jasne, ale zostawiające drugiej osobie godną drogę powrotu. Potem napisz wersję bardziej bezpośrednią i łagodniejszą.',
    ro: 'Scrie o propoziție suficient de clară, dar care îi lasă celuilalt o cale demnă de revenire. Apoi scrie o variantă mai directă și una mai blândă.',
    sq: 'Shkruaj një fjali mjaft të qartë, por që i lë personit tjetër një rrugë kthimi me dinjitet. Pastaj shkruaj një version më të drejtpërdrejtë dhe një më të butë.'
  };
}

function reviewInstruction(item) {
  return {
    de: `Pruefe deine Varianten fuer ${item.topic.de}: Welche ist sachlich am klarsten? Welche schuetzt die Beziehung am besten? Welche waere in Deutschland zu indirekt oder zu direkt?`,
    en: `Check your variants for ${item.topic.en}: which is clearest factually? Which best protects the relationship? Which would be too indirect or too direct in Germany?`,
    fa: `نسخه‌هایت را برای موضوع ${item.topic.fa} بررسی کن: کدام از نظر محتوا روشن‌تر است؟ کدام رابطه را بهتر حفظ می‌کند؟ کدام در آلمان بیش از حد غیرمستقیم یا بیش از حد مستقیم است؟`,
    ar: `افحص صيغك لموضوع ${item.topic.ar}: أيها أوضح من حيث المحتوى؟ أيها يحمي العلاقة أكثر؟ أيها سيكون في ألمانيا غير مباشر جدًا أو مباشرًا جدًا؟`,
    tr: `${item.topic.tr} için versiyonlarını kontrol et: hangisi içerik açısından en açık? Hangisi ilişkiyi en iyi koruyor? Hangisi Almanya’da fazla dolaylı ya da fazla doğrudan olurdu?`,
    ru: `Проверь варианты для темы ${item.topic.ru}: какой фактически самый ясный? Какой лучше всего защищает отношения? Какой в Германии был бы слишком косвенным или слишком прямым?`,
    ckb: `وەشانەکانت بۆ بابەتی ${item.topic.ckb} بپشکنە: کامەیان لە ڕووی ناوەڕۆکەوە ڕوونترە؟ کامەیان پەیوەندی باشتر دەپارێزێت؟ کامەیان لە ئەڵمانیا زۆر ناڕاستەوخۆ یان زۆر ڕاستەوخۆ دەبێت؟`,
    kmr: `Guhertoyên xwe ji bo mijara ${item.topic.kmr} kontrol bike: kîjan ji aliyê naverokê ve herî zelal e? Kîjan têkilî baştir diparêze? Kîjan li Almanyayê pir nerast an pir rasterast bûya?`,
    pl: `Sprawdź swoje wersje dla tematu ${item.topic.pl}: która jest najjaśniejsza rzeczowo? Która najlepiej chroni relację? Która w Niemczech byłaby zbyt pośrednia albo zbyt bezpośrednia?`,
    ro: `Verifică variantele tale pentru tema ${item.topic.ro}: care este cea mai clară factual? Care protejează cel mai bine relația? Care ar fi în Germania prea indirectă sau prea directă?`,
    sq: `Kontrollo variantet e tua për temën ${item.topic.sq}: cila është më e qartë në përmbajtje? Cila e mbron më mirë marrëdhënien? Cila do të ishte në Gjermani tepër e tërthortë ose tepër e drejtpërdrejtë?`
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
    createActivity(item.targetType === 'writing-template' ? 'write' : item.targetType === 'exam-prep-unit' ? 'exam-prep' : 'roleplay', 'target', targetInstruction(item), item.targetType, item.targetSlug, 10, 30),
    createActivity('practice', 'transfer', transferInstruction(), 'none', null, 8, 40),
    createActivity('review', 'review', reviewInstruction(item), 'none', null, 6, 50)
  ];
}

fs.writeFileSync(file, JSON.stringify(data, null, 2) + '\n', 'utf8');
console.log(`Updated ${items.length} C2 Module 9 lessons with ${items.length * 5} activity blocks.`);

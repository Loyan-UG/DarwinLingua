const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c1-souveraen-in-studium-beruf-und-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  read: {
    de: 'Lesestrategie festlegen',
    en: 'Set the reading strategy',
    fa: 'راهبرد خواندن را مشخص کن',
    ar: 'حدّد استراتيجية القراءة',
    tr: 'Okuma stratejisini belirle',
    ru: 'Определи стратегию чтения',
    ckb: 'ستراتیژی خوێندنەوە دیاری بکە',
    kmr: 'Stratejiya xwendinê diyar bike',
    pl: 'Ustal strategię czytania',
    ro: 'Stabilește strategia de lectură',
    sq: 'Përcakto strategjinë e leximit'
  },
  structure: {
    de: 'Textlogik sichern',
    en: 'Secure the text logic',
    fa: 'منطق متن را روشن کن',
    ar: 'ثبّت منطق النص',
    tr: 'Metnin mantığını güvenceye al',
    ru: 'Зафиксируй логику текста',
    ckb: 'لۆجیکی دەقەکە بسەلمێنە',
    kmr: 'Mantiqa nivîsê ewle bike',
    pl: 'Uchwyć logikę tekstu',
    ro: 'Fixează logica textului',
    sq: 'Siguro logjikën e tekstit'
  },
  material: {
    de: 'Material gezielt nutzen',
    en: 'Use the material deliberately',
    fa: 'از محتوای لینک‌شده هدفمند استفاده کن',
    ar: 'استخدم المادة بهدف واضح',
    tr: 'Malzemeyi amaçlı kullan',
    ru: 'Используй материал целенаправленно',
    ckb: 'ماتریاڵەکە بە ئامانج بەکاربهێنە',
    kmr: 'Materyalê bi armanc bi kar bîne',
    pl: 'Wykorzystaj materiał celowo',
    ro: 'Folosește materialul cu un scop clar',
    sq: 'Përdore materialin me qëllim të qartë'
  },
  practice: {
    de: 'Eigenen Analysebaustein schreiben',
    en: 'Write your own analysis block',
    fa: 'یک بخش تحلیلی کوتاه بنویس',
    ar: 'اكتب جزءًا تحليليًا قصيرًا',
    tr: 'Kısa bir analiz bölümü yaz',
    ru: 'Напиши короткий аналитический блок',
    ckb: 'بەشێکی شیکاری کورتی خۆت بنووسە',
    kmr: 'Parçeyeke analîtîk a kurt binivîse',
    pl: 'Napisz krótki fragment analizy',
    ro: 'Scrie un scurt fragment de analiză',
    sq: 'Shkruaj një pjesë të shkurtër analize'
  },
  review: {
    de: 'Analyse pruefen',
    en: 'Check the analysis',
    fa: 'تحلیل را بررسی کن',
    ar: 'راجع التحليل',
    tr: 'Analizi kontrol et',
    ru: 'Проверь анализ',
    ckb: 'شیکارییەکە بپشکنە',
    kmr: 'Analîzê kontrol bike',
    pl: 'Sprawdź analizę',
    ro: 'Verifică analiza',
    sq: 'Kontrollo analizën'
  }
};

function translations(map) {
  return langs.map(language => ({ language, text: map[language] }));
}

function title(key) {
  return {
    title: titles[key].de,
    titleTranslations: translations(titles[key])
  };
}

const items = [
  {
    slug: 'c1-akademische-textstruktur-erkennen',
    topic: {
      de: 'akademische Textstruktur',
      en: 'academic text structure',
      fa: 'ساختار متن دانشگاهی',
      ar: 'بنية النص الأكاديمي',
      tr: 'akademik metin yapısı',
      ru: 'структура академического текста',
      ckb: 'پێکهاتەی دەقی ئەکادیمی',
      kmr: 'avahiya nivîsa akademîk',
      pl: 'struktura tekstu akademickiego',
      ro: 'structura textului academic',
      sq: 'struktura e tekstit akademik'
    },
    focus: {
      de: 'Einleitung, Fragestellung, Argumentationsschritte und Ergebnis voneinander trennen',
      en: 'separating introduction, research question, argument steps, and result',
      fa: 'جدا کردن مقدمه، پرسش اصلی، مراحل استدلال و نتیجه',
      ar: 'فصل المقدمة والسؤال الرئيس وخطوات الحجة والنتيجة',
      tr: 'giriş, temel soru, argüman adımları ve sonucu ayırmayı',
      ru: 'разделять введение, исследовательский вопрос, шаги аргументации и результат',
      ckb: 'جیاکردنەوەی پێشەکی، پرسی سەرەکی، هەنگاوەکانی ئارگیومێنت و ئەنجام',
      kmr: 'cuda kirina destpêk, pirsa sereke, gavên argumanê û encamê',
      pl: 'oddzielanie wstępu, pytania głównego, kroków argumentacji i wyniku',
      ro: 'separarea introducerii, întrebării principale, pașilor argumentării și rezultatului',
      sq: 'ndarjen e hyrjes, pyetjes kryesore, hapave të argumentimit dhe rezultatit'
    },
    grammar: ['c1-formal-summaries', 'formale Zusammenfassungen'],
    target: ['exam-prep-unit', 'c1-lesetexte-selektiv-erschliessen', 'selektives Lesen'],
    practice: {
      de: 'Skizziere einen unbekannten Text in vier Zeilen: Thema, Leitfrage, zwei Argumentationsschritte und vorlaeufiges Ergebnis.',
      en: 'Outline an unfamiliar text in four lines: topic, guiding question, two argument steps, and preliminary result.',
      fa: 'یک متن ناآشنا را در چهار خط طرح‌ریزی کن: موضوع، پرسش راهنما، دو مرحله استدلال و نتیجه موقت.',
      ar: 'لخّص بنية نص غير مألوف في أربعة أسطر: الموضوع، السؤال الموجّه، خطوتان في الحجة، ونتيجة أولية.',
      tr: 'Tanımadığın bir metni dört satırda taslakla: konu, yönlendirici soru, iki argüman adımı ve geçici sonuç.',
      ru: 'Набросай структуру незнакомого текста в четырех строках: тема, ведущий вопрос, два шага аргументации и предварительный результат.',
      ckb: 'پێکهاتەی دەقێکی نەناسراو لە چوار دێڕدا بنووسە: بابەت، پرسی ڕێنوێن، دوو هەنگاوی ئارگیومێنت و ئەنجامی کاتی.',
      kmr: 'Nexşeya nivîseke nenas di çar rêzan de çêbike: mijar, pirsa rêber, du gavên argumanê û encama demkî.',
      pl: 'Naszkicuj nieznany tekst w czterech liniach: temat, pytanie przewodnie, dwa kroki argumentacji i wynik roboczy.',
      ro: 'Schițează un text necunoscut în patru rânduri: temă, întrebare-ghid, doi pași argumentativi și rezultat provizoriu.',
      sq: 'Skico një tekst të panjohur në katër rreshta: tema, pyetja udhëzuese, dy hapa argumentimi dhe rezultati paraprak.'
    },
    review: {
      de: 'Pruefe, ob deine Skizze die Funktion der Textteile zeigt und nicht nur Inhaltswoerter aus dem Text wiederholt.',
      en: 'Check whether your outline shows the function of the text parts and does not merely repeat content words from the text.',
      fa: 'بررسی کن که طرح تو نقش بخش‌های متن را نشان می‌دهد، نه اینکه فقط چند واژه از متن را تکرار کرده باشد.',
      ar: 'تحقّق مما إذا كان مخططك يوضح وظيفة أجزاء النص ولا يكرر فقط كلمات من النص.',
      tr: 'Taslağının metin bölümlerinin işlevini gösterdiğini, yalnızca metinden sözcükleri tekrarlamadığını kontrol et.',
      ru: 'Проверь, показывает ли схема функцию частей текста, а не просто повторяет слова из текста.',
      ckb: 'بپشکنە نەخشەکەت ئەرکی بەشەکانی دەق پیشان دەدات، نەک تەنها وشەکانی ناو دەق دووبارە بکاتەوە.',
      kmr: 'Kontrol bike ka nexşeya te erka beşên nivîsê nîşan dide, ne tenê peyvên nivîsê dubare dike.',
      pl: 'Sprawdź, czy szkic pokazuje funkcję części tekstu, a nie tylko powtarza słowa z tekstu.',
      ro: 'Verifică dacă schița arată funcția părților textului, nu doar repetă cuvinte din text.',
      sq: 'Kontrollo nëse skica tregon funksionin e pjesëve të tekstit, jo vetëm përsërit fjalë nga teksti.'
    }
  },
  {
    slug: 'c1-argumentationsgang-rekonstruieren',
    topic: {
      de: 'Argumentationsgang',
      en: 'line of argument',
      fa: 'مسیر استدلال',
      ar: 'مسار الحجة',
      tr: 'argümanın akışı',
      ru: 'ход аргументации',
      ckb: 'ڕێڕەوی ئارگیومێنت',
      kmr: 'rêça argumanê',
      pl: 'tok argumentacji',
      ro: 'firul argumentării',
      sq: 'rrjedha e argumentimit'
    },
    focus: {
      de: 'These, Belege, Gegenposition und Folgerung in der richtigen Reihenfolge rekonstruieren',
      en: 'reconstructing thesis, evidence, opposing view, and conclusion in the right order',
      fa: 'بازسازی تز، شواهد، موضع مخالف و نتیجه به ترتیب درست',
      ar: 'إعادة بناء الأطروحة والأدلة والرأي المقابل والاستنتاج بالترتيب الصحيح',
      tr: 'tez, kanıt, karşı görüş ve sonucu doğru sırayla yeniden kurmayı',
      ru: 'восстанавливать тезис, доказательства, противоположную позицию и вывод в правильном порядке',
      ckb: 'دووبارە دروستکردنەوەی تێز، بەڵگە، هەڵوێستی بەرامبەر و دەرئەنجام بە ڕیزبەندی دروست',
      kmr: 'ji nû ve avakirina tez, belge, helwesta dijber û encamê bi rêza rast',
      pl: 'odtwarzanie tezy, dowodów, stanowiska przeciwnego i wniosku we właściwej kolejności',
      ro: 'reconstruirea tezei, dovezilor, poziției opuse și concluziei în ordinea corectă',
      sq: 'rindërtimin e tezës, provave, qëndrimit të kundërt dhe përfundimit në rendin e duhur'
    },
    grammar: ['c1-advanced-academic-connectors', 'akademische Konnektoren'],
    target: ['exam-prep-unit', 'c1-hauptaussage-und-belege-trennen', 'Hauptaussage und Belege'],
    practice: {
      de: 'Schreibe den Argumentationsgang als Kette aus fuenf Pfeilen. Jeder Pfeil muss zeigen, warum der naechste Schritt folgt.',
      en: 'Write the line of argument as a chain of five arrows. Each arrow must show why the next step follows.',
      fa: 'مسیر استدلال را به صورت زنجیره‌ای با پنج پیکان بنویس. هر پیکان باید نشان دهد چرا مرحله بعدی از قبلی نتیجه می‌شود.',
      ar: 'اكتب مسار الحجة كسلسلة من خمسة أسهم. يجب أن يوضح كل سهم لماذا تأتي الخطوة التالية.',
      tr: 'Argümanın akışını beş oklu bir zincir olarak yaz. Her ok, sonraki adımın neden geldiğini göstermeli.',
      ru: 'Запиши ход аргументации как цепочку из пяти стрелок. Каждая стрелка должна показывать, почему следует следующий шаг.',
      ckb: 'ڕێڕەوی ئارگیومێنتەکە وەک زنجیرەیەک بە پێنج تیر بنووسە. هەر تیرێک دەبێت پیشان بدات بۆچی هەنگاوی دواتر دێت.',
      kmr: 'Rêça argumanê wek zincîreyek bi pênc tîran binivîse. Her tîr divê nîşan bide çima gava din tê.',
      pl: 'Zapisz tok argumentacji jako łańcuch pięciu strzałek. Każda strzałka ma pokazać, dlaczego następuje kolejny krok.',
      ro: 'Scrie firul argumentării ca un lanț de cinci săgeți. Fiecare săgeată trebuie să arate de ce urmează pasul următor.',
      sq: 'Shkruaj rrjedhën e argumentimit si zinxhir me pesë shigjeta. Çdo shigjetë duhet të tregojë pse vjen hapi tjetër.'
    },
    review: {
      de: 'Pruefe, ob deine Kette wirklich begruendet ist oder ob du nur Textabschnitte nebeneinander gestellt hast.',
      en: 'Check whether your chain is truly reasoned, or whether you only placed text sections next to each other.',
      fa: 'بررسی کن که زنجیره تو واقعاً رابطه استدلالی دارد یا فقط بخش‌های متن را کنار هم چیده‌ای.',
      ar: 'تحقّق مما إذا كانت السلسلة مبنية على تعليل فعلي أم أنك وضعت مقاطع النص بجانب بعضها فقط.',
      tr: 'Zincirinin gerçekten gerekçeli olup olmadığını, yoksa yalnızca metin bölümlerini yan yana koyup koymadığını kontrol et.',
      ru: 'Проверь, действительно ли цепочка обоснована, или ты просто поставил фрагменты текста рядом.',
      ckb: 'بپشکنە زنجیرەکەت بەڕاستی بە هۆکارەوە بەستراوە یان تەنها بەشەکانی دەقت لە تەنیشت یەک داناوە.',
      kmr: 'Kontrol bike ka zincîra te rastî bi sedeman girêdayî ye, an tenê beşên nivîsê li kêleka hev daniye.',
      pl: 'Sprawdź, czy łańcuch jest naprawdę uzasadniony, czy tylko ustawiłeś fragmenty tekstu obok siebie.',
      ro: 'Verifică dacă lanțul este cu adevărat justificat sau doar ai pus fragmentele de text unul lângă altul.',
      sq: 'Kontrollo nëse zinxhiri yt është vërtet i arsyetuar apo thjesht ke vendosur pjesë të tekstit pranë njëra-tjetrës.'
    }
  },
  {
    slug: 'c1-quellenkritik-im-seminar',
    topic: {
      de: 'Quellenkritik im Seminar',
      en: 'source criticism in a seminar',
      fa: 'نقد منبع در سمینار',
      ar: 'نقد المصدر في حلقة دراسية',
      tr: 'seminerde kaynak eleştirisi',
      ru: 'критика источника на семинаре',
      ckb: 'ڕەخنەی سەرچاوە لە سمینار',
      kmr: 'rexneya çavkaniyê di semînerê de',
      pl: 'krytyka źródła na seminarium',
      ro: 'critica sursei într-un seminar',
      sq: 'kritika e burimit në seminar'
    },
    focus: {
      de: 'eine Quelle kritisch, aber fair nach Autor, Kontext, Interesse und Beleglage einordnen',
      en: 'evaluating a source critically but fairly by author, context, interest, and evidence',
      fa: 'ارزیابی منصفانه و انتقادی منبع بر اساس نویسنده، زمینه، منفعت و شواهد',
      ar: 'تقييم مصدر نقديًا وبإنصاف بحسب الكاتب والسياق والمصلحة والأدلة',
      tr: 'bir kaynağı yazar, bağlam, çıkar ve kanıt durumu açısından eleştirel ama adil değerlendirmeyi',
      ru: 'критически, но справедливо оценивать источник по автору, контексту, интересу и доказательствам',
      ckb: 'هەڵسەنگاندنی سەرچاوە بە ڕەخنەیی و دادپەروەرانە بەپێی نووسەر، پاشبنەما، بەرژەوەندی و بەڵگە',
      kmr: 'nirxandina çavkaniyê bi rexneyî lê adil li gorî nivîskar, çarçove, berjewendî û belgeyan',
      pl: 'krytyczną, ale uczciwą ocenę źródła według autora, kontekstu, interesu i dowodów',
      ro: 'evaluarea critică, dar echilibrată a unei surse după autor, context, interes și dovezi',
      sq: 'vlerësimin kritik, por të drejtë të një burimi sipas autorit, kontekstit, interesit dhe provave'
    },
    grammar: ['c1-indirect-speech-in-journalism-and-formal-contexts', 'indirekte Rede in formellen Kontexten'],
    target: ['roleplay', 'c1-eine-quellenkritik-im-seminar-formulieren', 'Quellenkritik im Seminar'],
    practice: {
      de: 'Formuliere eine Quellenkritik in drei Saetzen: Was leistet die Quelle, wo ist sie begrenzt, und wie nutzt du sie trotzdem?',
      en: 'Formulate a source critique in three sentences: what the source provides, where it is limited, and how you still use it.',
      fa: 'نقد منبع را در سه جمله بنویس: منبع چه کمکی می‌کند، محدودیتش کجاست و با وجود آن چطور از آن استفاده می‌کنی.',
      ar: 'صغ نقدًا للمصدر في ثلاث جمل: ماذا يقدّم المصدر، أين حدوده، وكيف تستخدمه رغم ذلك.',
      tr: 'Üç cümlede kaynak eleştirisi yaz: kaynak ne sağlar, nerede sınırlıdır ve buna rağmen onu nasıl kullanırsın?',
      ru: 'Сформулируй критику источника в трех предложениях: что дает источник, где его пределы и как ты всё же его используешь.',
      ckb: 'ڕەخنەی سەرچاوە لە سێ ڕستەدا بنووسە: سەرچاوەکە چی دەدات، سنوورەکەی لە کوێیە، و لەگەڵ ئەوەشدا چۆن بەکاری دەهێنیت.',
      kmr: 'Rexneya çavkaniyê di sê hevokan de binivîse: çavkanî çi dide, sînorê wê li ku ye, û tevî wê çawa wê bi kar tînî.',
      pl: 'Sformułuj krytykę źródła w trzech zdaniach: co źródło wnosi, gdzie jest ograniczone i jak mimo to z niego korzystasz.',
      ro: 'Formulează critica unei surse în trei propoziții: ce oferă sursa, unde este limitată și cum o folosești totuși.',
      sq: 'Formulo kritikën e një burimi në tri fjali: çfarë ofron burimi, ku kufizohet dhe si e përdor megjithatë.'
    },
    review: {
      de: 'Pruefe, ob deine Kritik sachlich bleibt und die Quelle nicht pauschal abwertet.',
      en: 'Check whether your critique remains factual and does not dismiss the source wholesale.',
      fa: 'بررسی کن که نقد تو عینی و منصفانه مانده و منبع را یک‌سره بی‌ارزش نکرده است.',
      ar: 'تحقّق مما إذا كان نقدك موضوعيًا ولا يقلّل من قيمة المصدر بالكامل.',
      tr: 'Eleştirinin nesnel kaldığını ve kaynağı bütünüyle değersizleştirmediğini kontrol et.',
      ru: 'Проверь, остается ли критика предметной и не обесценивает ли источник полностью.',
      ckb: 'بپشکنە ڕەخنەکەت بابەتی ماوە و سەرچاوەکە بە گشتی بێبها ناکات.',
      kmr: 'Kontrol bike ka rexneya te rastî û babetî maye û çavkaniyê bi giştî bêqîmet nake.',
      pl: 'Sprawdź, czy krytyka pozostaje rzeczowa i nie deprecjonuje źródła całościowo.',
      ro: 'Verifică dacă critica rămâne factuală și nu devalorizează sursa în bloc.',
      sq: 'Kontrollo nëse kritika jote mbetet faktike dhe nuk e zhvlerëson burimin në tërësi.'
    }
  },
  {
    slug: 'c1-zitate-paraphrasen-und-verweise-sprachlich-einordnen',
    topic: {
      de: 'Zitate, Paraphrasen und Verweise',
      en: 'quotes, paraphrases, and references',
      fa: 'نقل‌قول، بازنویسی و ارجاع',
      ar: 'الاقتباس وإعادة الصياغة والإحالة',
      tr: 'alıntı, yeniden ifade ve atıf',
      ru: 'цитаты, пересказ и ссылки',
      ckb: 'وەرگرتنی ڕاستەوخۆ، دووبارەنووسین و ئاماژەدان',
      kmr: 'gotinên rasterast, vegotin û referans',
      pl: 'cytaty, parafrazy i odwołania',
      ro: 'citate, parafraze și trimiteri',
      sq: 'citime, parafrazime dhe referime'
    },
    focus: {
      de: 'fremde Positionen sprachlich sauber vom eigenen Kommentar trennen',
      en: 'separating other people’s positions cleanly from your own comment',
      fa: 'جدا کردن روشن دیدگاه دیگران از نظر و تحلیل خودت',
      ar: 'فصل مواقف الآخرين بوضوح عن تعليقك وتحليلك',
      tr: 'başkalarının görüşlerini kendi yorumundan açık biçimde ayırmayı',
      ru: 'четко отделять чужие позиции от собственного комментария',
      ckb: 'جیاکردنەوەی ڕوونی هەڵوێستی کەسانی تر لە کۆمێنت و شیکاری خۆت',
      kmr: 'cuda kirina zelal a helwestên kesên din ji şîroveya xwe',
      pl: 'jasne oddzielanie cudzych stanowisk od własnego komentarza',
      ro: 'separarea clară a pozițiilor altora de propriul comentariu',
      sq: 'ndarjen e qartë të qëndrimeve të të tjerëve nga komenti yt'
    },
    grammar: ['c1-reported-opinions', 'wiedergegebene Meinungen'],
    target: ['exam-prep-unit', 'c1-testdaf-texte-gezielt-paraphrasieren', 'gezielte Paraphrase'],
    practice: {
      de: 'Paraphrasiere eine fremde Position mit laut, zufolge oder demnach und fuege erst danach deine eigene Einordnung hinzu.',
      en: 'Paraphrase another position with laut, zufolge, or demnach, and only then add your own evaluation.',
      fa: 'دیدگاه شخص یا منبع دیگر را با laut، zufolge یا demnach بازنویسی کن و فقط بعد از آن ارزیابی خودت را اضافه کن.',
      ar: 'أعد صياغة موقف مصدر آخر باستخدام laut أو zufolge أو demnach، ثم أضف تقييمك أنت بعد ذلك فقط.',
      tr: 'Başka bir görüşü laut, zufolge ya da demnach ile yeniden ifade et; ancak ondan sonra kendi değerlendirmeni ekle.',
      ru: 'Перефразируй чужую позицию с помощью laut, zufolge или demnach и только затем добавь собственную оценку.',
      ckb: 'هەڵوێستی سەرچاوەیەکی تر بە laut، zufolge یان demnach دووبارە بنووسە و تەنها پاشان هەڵسەنگاندنی خۆت زیاد بکە.',
      kmr: 'Helwesta çavkaniyeke din bi laut, zufolge an demnach veguherîne û tenê paşê nirxandina xwe zêde bike.',
      pl: 'Sparafrazuj cudze stanowisko za pomocą laut, zufolge albo demnach, a dopiero potem dodaj własną ocenę.',
      ro: 'Parafrazează poziția altcuiva cu laut, zufolge sau demnach și abia apoi adaugă propria evaluare.',
      sq: 'Parafrazo qëndrimin e dikujt tjetër me laut, zufolge ose demnach dhe vetëm pastaj shto vlerësimin tënd.'
    },
    review: {
      de: 'Pruefe, ob klar ist, welche Aussage aus der Quelle stammt und welche Bewertung von dir kommt.',
      en: 'Check whether it is clear which statement comes from the source and which evaluation comes from you.',
      fa: 'بررسی کن که روشن است کدام جمله از منبع آمده و کدام ارزیابی از طرف توست.',
      ar: 'تحقّق مما إذا كان واضحًا أي عبارة من المصدر وأي تقييم منك.',
      tr: 'Hangi ifadenin kaynaktan, hangi değerlendirmenin senden geldiğinin açık olup olmadığını kontrol et.',
      ru: 'Проверь, ясно ли, какое утверждение взято из источника, а какая оценка принадлежит тебе.',
      ckb: 'بپشکنە ڕوونە کام وتە لە سەرچاوەکەوەیە و کام هەڵسەنگاندن لە تۆوەیە.',
      kmr: 'Kontrol bike ka zelal e kîjan gotin ji çavkaniyê tê û kîjan nirxandin ya te ye.',
      pl: 'Sprawdź, czy wiadomo, które stwierdzenie pochodzi ze źródła, a która ocena jest twoja.',
      ro: 'Verifică dacă este clar ce afirmație vine din sursă și ce evaluare îți aparține.',
      sq: 'Kontrollo nëse është e qartë cili pohim vjen nga burimi dhe cili vlerësim është i yti.'
    }
  },
  {
    slug: 'c1-implizite-bewertungen-in-texten-erkennen',
    topic: {
      de: 'implizite Bewertungen in Texten',
      en: 'implicit evaluations in texts',
      fa: 'ارزیابی‌های پنهان در متن',
      ar: 'التقييمات الضمنية في النصوص',
      tr: 'metinlerde örtük değerlendirmeler',
      ru: 'скрытые оценки в текстах',
      ckb: 'هەڵسەنگاندنی شاراوە لە دەقەکان',
      kmr: 'nirxandinên veşartî di nivîsan de',
      pl: 'ukryte oceny w tekstach',
      ro: 'evaluări implicite în texte',
      sq: 'vlerësime të nënkuptuara në tekste'
    },
    focus: {
      de: 'Wertung, Distanz, Zustimmung und Skepsis aus Wortwahl und Struktur erkennen',
      en: 'recognizing evaluation, distance, agreement, and skepticism through wording and structure',
      fa: 'تشخیص ارزیابی، فاصله‌گیری، هم‌سویی یا تردید از واژه‌ها و ساختار متن',
      ar: 'تمييز التقييم والمسافة والتأييد أو الشك من اختيار الكلمات وبنية النص',
      tr: 'sözcük seçimi ve yapıdan değerlendirme, mesafe, onay ya da şüpheyi tanımayı',
      ru: 'распознавать оценку, дистанцию, согласие и скепсис по выбору слов и структуре',
      ckb: 'ناسینەوەی هەڵسەنگاندن، دووریگرتن، هاوڕایی یان گومان لە ڕێگەی وشە و پێکهاتەوە',
      kmr: 'nasîna nirxandin, dûrmayîn, hevpeymandin an guman ji hilbijartina peyvan û avahiyê',
      pl: 'rozpoznawanie oceny, dystansu, zgody i sceptycyzmu przez dobór słów i strukturę',
      ro: 'recunoașterea evaluării, distanțării, acordului sau scepticismului din cuvinte și structură',
      sq: 'njohjen e vlerësimit, distancimit, pajtimit ose skepticizmit nga fjalët dhe struktura'
    },
    grammar: ['c1-register-shifting', 'Register und Ton'],
    target: ['exam-prep-unit', 'c1-lesetexte-mit-impliziter-wertung-analysieren', 'implizite Wertung in Lesetexten'],
    practice: {
      de: 'Markiere drei wertende Woerter und erklaere, ob sie Zustimmung, Kritik, Vorsicht oder Distanz zum Thema signalisieren.',
      en: 'Mark three evaluative words and explain whether they signal agreement, criticism, caution, or distance from the topic.',
      fa: 'سه واژه ارزیابانه را مشخص کن و توضیح بده آیا نشانه هم‌سویی، انتقاد، احتیاط یا فاصله‌گیری از موضوع هستند.',
      ar: 'حدّد ثلاث كلمات تقييمية واشرح هل تشير إلى تأييد أو نقد أو حذر أو مسافة من الموضوع.',
      tr: 'Üç değerlendirici kelimeyi işaretle ve bunların onay, eleştiri, temkin ya da konuya mesafe gösterip göstermediğini açıkla.',
      ru: 'Отметь три оценочных слова и объясни, выражают ли они согласие, критику, осторожность или дистанцию к теме.',
      ckb: 'سێ وشەی هەڵسەنگاندن دیاری بکە و ڕوونی بکە ئایا هاوڕایی، ڕەخنە، بەهۆشی یان دووری لە بابەت پیشان دەدەن.',
      kmr: 'Sê peyvên nirxandinê nîşan bike û rave bike ka ew hevpeymandin, rexne, baldariyê an dûrmayîna ji mijarê nîşan didin.',
      pl: 'Zaznacz trzy słowa oceniające i wyjaśnij, czy sygnalizują zgodę, krytykę, ostrożność czy dystans wobec tematu.',
      ro: 'Marchează trei cuvinte evaluative și explică dacă indică acord, critică, prudență sau distanțare față de subiect.',
      sq: 'Shëno tri fjalë vlerësuese dhe shpjego nëse tregojnë pajtim, kritikë, kujdes apo distancim nga tema.'
    },
    review: {
      de: 'Pruefe, ob du die Wertung aus dem Text belegst und nicht nur aus deinem Vorwissen ableitest.',
      en: 'Check whether you support the evaluation with the text and do not derive it only from prior knowledge.',
      fa: 'بررسی کن که ارزیابی را با خود متن ثابت می‌کنی، نه فقط با دانسته‌های قبلی خودت.',
      ar: 'تحقّق مما إذا كنت تثبت التقييم من النص نفسه، لا من معرفتك السابقة فقط.',
      tr: 'Değerlendirmeyi metinden desteklediğini, yalnızca ön bilginden çıkarmadığını kontrol et.',
      ru: 'Проверь, подтверждаешь ли ты оценку текстом, а не выводишь её только из своих прежних знаний.',
      ckb: 'بپشکنە هەڵسەنگاندنەکە بە دەقەکە پشتگیری دەکەیت، نەک تەنها لە زانیاری پێشووی خۆتەوە.',
      kmr: 'Kontrol bike ka tu nirxandinê bi nivîsê piştgirî dikî, ne tenê ji zanîna berê ya xwe derdixî.',
      pl: 'Sprawdź, czy opierasz ocenę na tekście, a nie tylko na swojej wcześniejszej wiedzy.',
      ro: 'Verifică dacă susții evaluarea prin text, nu doar prin cunoștințele tale anterioare.',
      sq: 'Kontrollo nëse e mbështet vlerësimin me tekstin, jo vetëm me njohuritë e tua të mëparshme.'
    }
  },
  {
    slug: 'c1-statistik-grafik-und-text-verknuepfen',
    topic: {
      de: 'Statistik, Grafik und Text',
      en: 'statistics, charts, and text',
      fa: 'آمار، نمودار و متن',
      ar: 'الإحصاء والرسم البياني والنص',
      tr: 'istatistik, grafik ve metin',
      ru: 'статистика, график и текст',
      ckb: 'ئامار، گرافیک و دەق',
      kmr: 'statîstîk, grafîk û nivîs',
      pl: 'statystyka, wykres i tekst',
      ro: 'statistică, grafic și text',
      sq: 'statistika, grafiku dhe teksti'
    },
    focus: {
      de: 'Daten nicht nur beschreiben, sondern mit einer Textthese verbinden',
      en: 'not only describing data, but connecting it to a textual thesis',
      fa: 'فقط توصیف نکردن داده‌ها، بلکه وصل کردن آن‌ها به تز متن',
      ar: 'عدم وصف البيانات فقط، بل ربطها بأطروحة النص',
      tr: 'verileri yalnızca betimlemeyip metindeki tezle ilişkilendirmeyi',
      ru: 'не только описывать данные, но связывать их с тезисом текста',
      ckb: 'نەک تەنها وەسفکردنی داتا، بەڵکو بەستنی بە تێزی دەقەکە',
      kmr: 'ne tenê danasîna daneyan, lê girêdana wan bi teza nivîsê',
      pl: 'nie tylko opisywanie danych, lecz łączenie ich z tezą tekstu',
      ro: 'nu doar descrierea datelor, ci legarea lor de teza textului',
      sq: 'jo vetëm përshkrimin e të dhënave, por lidhjen e tyre me tezën e tekstit'
    },
    grammar: ['c1-complex-comparison-structures', 'komplexe Vergleichsstrukturen'],
    target: ['exam-prep-unit', 'c1-grafik-und-text-verknuepfen', 'Grafik und Text'],
    practice: {
      de: 'Formuliere zwei Saetze: einen Datenbefund und einen Satz, der zeigt, wie dieser Befund die Textthese stuetzt oder begrenzt.',
      en: 'Write two sentences: one data finding and one sentence showing how that finding supports or limits the text thesis.',
      fa: 'دو جمله بنویس: یکی درباره یافته آماری و دیگری درباره اینکه این یافته چطور تز متن را تقویت یا محدود می‌کند.',
      ar: 'اكتب جملتين: نتيجة من البيانات، وجملة تبيّن كيف تدعم هذه النتيجة أطروحة النص أو تحدّ منها.',
      tr: 'İki cümle yaz: biri veri bulgusu, diğeri bu bulgunun metindeki tezi nasıl desteklediğini ya da sınırladığını göstersin.',
      ru: 'Напиши два предложения: одно о данных, второе о том, как этот результат поддерживает или ограничивает тезис текста.',
      ckb: 'دوو ڕستە بنووسە: یەکێک لەسەر دۆزینەوەی ئاماری و یەکێک لەسەر ئەوەی ئەم دۆزینەوەیە چۆن تێزی دەق پشتگیری یان سنووردار دەکات.',
      kmr: 'Du hevokan binivîse: yek li ser encama daneyan û yek jî nîşan bide ev encam çawa teza nivîsê piştgirî an sînordar dike.',
      pl: 'Napisz dwa zdania: jedno o wyniku danych, drugie o tym, jak ten wynik wspiera albo ogranicza tezę tekstu.',
      ro: 'Scrie două propoziții: una despre constatarea din date și una care arată cum aceasta susține sau limitează teza textului.',
      sq: 'Shkruaj dy fjali: një gjetje nga të dhënat dhe një fjali që tregon si kjo gjetje mbështet ose kufizon tezën e tekstit.'
    },
    review: {
      de: 'Pruefe, ob du die Grafik nicht ueberinterpretierst und ob die Textthese wirklich zum Datenbefund passt.',
      en: 'Check whether you are not overinterpreting the chart and whether the text thesis really fits the data finding.',
      fa: 'بررسی کن که نمودار را بیش از حد تفسیر نکرده‌ای و تز متن واقعاً با یافته آماری سازگار است.',
      ar: 'تحقّق من أنك لا تفسّر الرسم البياني أكثر من اللازم وأن أطروحة النص تناسب فعلًا نتيجة البيانات.',
      tr: 'Grafiği aşırı yorumlamadığını ve metindeki tezin veri bulgusuna gerçekten uyduğunu kontrol et.',
      ru: 'Проверь, не переинтерпретируешь ли график и действительно ли тезис текста соответствует данным.',
      ckb: 'بپشکنە گرافیکەکە زیادەڕۆیانە شیکاری نەکردووە و تێزی دەقەکە بەڕاستی لەگەڵ دۆزینەوەی داتاکە دەگونجێت.',
      kmr: 'Kontrol bike ka grafîkê zêde şîrove nekirî û teza nivîsê rastî bi encama daneyan re li hev tê.',
      pl: 'Sprawdź, czy nie nadinterpretujesz wykresu i czy teza tekstu naprawdę pasuje do wyniku danych.',
      ro: 'Verifică dacă nu supra-interpretezi graficul și dacă teza textului se potrivește cu datele.',
      sq: 'Kontrollo nëse nuk po e mbinterpreton grafikun dhe nëse teza e tekstit përputhet vërtet me të dhënat.'
    }
  },
  {
    slug: 'c1-notizen-aus-vortraegen-verdichten',
    topic: {
      de: 'Notizen aus Vortraegen',
      en: 'notes from lectures',
      fa: 'یادداشت‌برداری از سخنرانی‌ها',
      ar: 'ملاحظات من المحاضرات',
      tr: 'sunumlardan notlar',
      ru: 'заметки из лекций',
      ckb: 'تێبینی لە وانە و پێشکەشکردنەکان',
      kmr: 'notên ji axaftin û pêşkêşiyan',
      pl: 'notatki z wykładów',
      ro: 'notițe din prelegeri',
      sq: 'shënime nga ligjëratat'
    },
    focus: {
      de: 'aus vielen Einzelnotizen eine knappe, geordnete Aussage bilden',
      en: 'turning many individual notes into a concise, ordered statement',
      fa: 'تبدیل یادداشت‌های پراکنده به یک بیان کوتاه و منظم',
      ar: 'تحويل ملاحظات كثيرة منفصلة إلى عبارة موجزة ومنظمة',
      tr: 'çok sayıda ayrı notu kısa ve düzenli bir ifadeye dönüştürmeyi',
      ru: 'превращать множество отдельных заметок в краткое, упорядоченное высказывание',
      ckb: 'گۆڕینی زۆر تێبینی جیاواز بۆ وتەیەکی کورت و ڕێکخراو',
      kmr: 'guhertina gelek notên cuda bo gotineke kurt û rêkûpêk',
      pl: 'przekształcanie wielu pojedynczych notatek w zwięzłą, uporządkowaną wypowiedź',
      ro: 'transformarea multor notițe separate într-o afirmație scurtă și ordonată',
      sq: 'kthimin e shumë shënimeve të veçanta në një pohim të shkurtër dhe të rregullt'
    },
    grammar: ['c1-formal-summaries', 'formale Zusammenfassungen'],
    target: ['exam-prep-unit', 'c1-hoertexte-strukturiert-notieren', 'strukturierte Hoernotizen'],
    practice: {
      de: 'Reduziere sechs Stichpunkte auf drei Kernaussagen: Thema, Entwicklung und offene Frage.',
      en: 'Reduce six bullet points to three core statements: topic, development, and open question.',
      fa: 'شش نکته یادداشت‌شده را به سه پیام اصلی کاهش بده: موضوع، روند/تحول و پرسش باز.',
      ar: 'اختصر ست نقاط إلى ثلاث أفكار أساسية: الموضوع، التطور، والسؤال المفتوح.',
      tr: 'Altı maddeyi üç temel ifadeye indir: konu, gelişme ve açık soru.',
      ru: 'Сократи шесть пунктов до трех ключевых высказываний: тема, развитие и открытый вопрос.',
      ckb: 'شەش خاڵ کەم بکەوە بۆ سێ وتەی سەرەکی: بابەت، پەرەسەندن و پرسی کراوە.',
      kmr: 'Şeş xalan kêm bike bo sê gotinên bingehîn: mijar, pêşketin û pirsa vekirî.',
      pl: 'Zredukuj sześć punktów do trzech głównych stwierdzeń: temat, rozwój i pytanie otwarte.',
      ro: 'Redu șase puncte la trei idei centrale: temă, evoluție și întrebare deschisă.',
      sq: 'Redukto gjashtë pika në tri pohime kryesore: tema, zhvillimi dhe pyetja e hapur.'
    },
    review: {
      de: 'Pruefe, ob deine Verdichtung Information spart, ohne die argumentative Richtung zu verlieren.',
      en: 'Check whether your condensation saves information without losing the argumentative direction.',
      fa: 'بررسی کن که فشرده‌سازی تو اطلاعات را کم کرده، اما جهت استدلال را از بین نبرده است.',
      ar: 'تحقّق مما إذا كان الاختصار يقلّل المعلومات من دون أن يفقد اتجاه الحجة.',
      tr: 'Yoğunlaştırmanın bilgiyi azalttığını ama argümanın yönünü kaybettirmediğini kontrol et.',
      ru: 'Проверь, сокращает ли сжатие информацию, не теряя направления аргументации.',
      ckb: 'بپشکنە کورتکردنەوەکەت زانیاری کەم دەکات، بەبێ ئەوەی ئاراستەی ئارگیومێنتەکە ون بکات.',
      kmr: 'Kontrol bike ka kurtkirina te agahiyê kêm dike, bêyî ku rêça argumanê winda bike.',
      pl: 'Sprawdź, czy kondensacja oszczędza informacje, nie gubiąc kierunku argumentacji.',
      ro: 'Verifică dacă sintetizarea reduce informația fără să piardă direcția argumentării.',
      sq: 'Kontrollo nëse përmbledhja kursen informacion pa humbur drejtimin e argumentimit.'
    }
  },
  {
    slug: 'c1-fachnahe-texte-ohne-fachstudium-verstehen',
    topic: {
      de: 'fachnahe Texte ohne Fachstudium',
      en: 'specialized-adjacent texts without studying the field',
      fa: 'متن‌های نزدیک به حوزه تخصصی بدون داشتن تحصیل تخصصی در آن حوزه',
      ar: 'نصوص قريبة من التخصص من دون دراسة ذلك التخصص',
      tr: 'alan bilgisi olmadan uzmanlık alanına yakın metinler',
      ru: 'около-специальные тексты без профильного образования',
      ckb: 'دەقە نزیکە-تایبەتییەکان بەبێ خوێندنی تایبەتی ئەو بوارە',
      kmr: 'nivîsên nêzîkî pisporiyê bê xwendina wê pisporiyê',
      pl: 'teksty bliskie specjalistycznym bez studiowania danej dziedziny',
      ro: 'texte apropiate de un domeniu specializat fără studii în domeniu',
      sq: 'tekste pranë fushës së specializuar pa studiuar atë fushë'
    },
    focus: {
      de: 'Fachbegriffe, Beispiele und Hauptaussage trennen, auch wenn das Thema neu ist',
      en: 'separating technical terms, examples, and main claim even when the topic is new',
      fa: 'جدا کردن اصطلاحات تخصصی، مثال‌ها و پیام اصلی حتی وقتی موضوع جدید است',
      ar: 'فصل المصطلحات المتخصصة والأمثلة والفكرة الرئيسة حتى عندما يكون الموضوع جديدًا',
      tr: 'konu yeni olsa bile terimleri, örnekleri ve ana iddiayı ayırmayı',
      ru: 'отделять термины, примеры и главный тезис, даже если тема новая',
      ckb: 'جیاکردنەوەی زاراوە، نموونە و بانگەشەی سەرەکی تەنانەت کاتێک بابەتەکە نوێیە',
      kmr: 'cuda kirina têgehên pisporî, mînak û gotina sereke, her çend mijar nû be',
      pl: 'oddzielanie terminów, przykładów i głównej tezy, nawet gdy temat jest nowy',
      ro: 'separarea termenilor tehnici, exemplelor și ideii principale chiar când tema este nouă',
      sq: 'ndarjen e termave teknikë, shembujve dhe idesë kryesore edhe kur tema është e re'
    },
    grammar: ['c1-advanced-nominalization', 'Nominalisierungen'],
    target: ['exam-prep-unit', 'c1-fachnahe-texte-ohne-vorwissen-erschliessen', 'fachnahe Texte ohne Vorwissen'],
    practice: {
      de: 'Waehle drei Fachbegriffe und schreibe daneben: Definition im Text, Beispiel im Text, eigene vorsichtige Umschreibung.',
      en: 'Choose three technical terms and write next to each: definition in the text, example in the text, and your own careful paraphrase.',
      fa: 'سه اصطلاح تخصصی انتخاب کن و کنار هرکدام بنویس: تعریف در متن، مثال در متن و بازنویسی محتاطانه خودت.',
      ar: 'اختر ثلاثة مصطلحات متخصصة واكتب بجانب كل واحد: تعريفه في النص، مثاله في النص، وصياغتك الحذرة له.',
      tr: 'Üç teknik terim seç ve her birinin yanına şunları yaz: metindeki tanım, metindeki örnek, kendi temkinli açıklaman.',
      ru: 'Выбери три термина и рядом с каждым запиши: определение в тексте, пример в тексте и свою осторожную переформулировку.',
      ckb: 'سێ زاراوەی تایبەتی هەڵبژێرە و لە تەنیشتی هەر یەک بنووسە: پێناسە لە دەق، نموونە لە دەق، و دووبارەنووسینی بەهۆشی خۆت.',
      kmr: 'Sê têgehên pisporî hilbijêre û li kêleka her yekê binivîse: danasîn di nivîsê de, mînak di nivîsê de, vegotina baldar a xwe.',
      pl: 'Wybierz trzy terminy i dopisz przy każdym: definicję w tekście, przykład w tekście i własną ostrożną parafrazę.',
      ro: 'Alege trei termeni tehnici și scrie lângă fiecare: definiția din text, exemplul din text și propria parafrază prudentă.',
      sq: 'Zgjidh tri terma teknikë dhe shkruaj pranë secilit: përkufizimin në tekst, shembullin në tekst dhe parafrazimin tënd të kujdesshëm.'
    },
    review: {
      de: 'Pruefe, ob du wirklich aus dem Text erschlossen hast und keine Fachbedeutung frei geraten hast.',
      en: 'Check whether you truly inferred from the text and did not freely guess a technical meaning.',
      fa: 'بررسی کن که معنا را واقعاً از متن استخراج کرده‌ای، نه اینکه معنای تخصصی را حدس زده باشی.',
      ar: 'تحقّق من أنك استنتجت المعنى فعلًا من النص ولم تخمّن معنى تخصصيًا بحرية.',
      tr: 'Anlamı gerçekten metinden çıkardığını, teknik anlamı serbestçe tahmin etmediğini kontrol et.',
      ru: 'Проверь, действительно ли ты вывел значение из текста, а не просто угадал специальный смысл.',
      ckb: 'بپشکنە ماناکەت بەڕاستی لە دەقەکەوە دەرکێشاوە، نەک مانای تایبەتی بە خەیاڵت دانابێت.',
      kmr: 'Kontrol bike ka te wate rastî ji nivîsê derxistiye, ne ku wateya pisporî texmîn kiriye.',
      pl: 'Sprawdź, czy naprawdę wywnioskowałeś znaczenie z tekstu, a nie swobodnie zgadłeś sens terminu.',
      ro: 'Verifică dacă ai dedus sensul din text, nu ai ghicit liber o semnificație specializată.',
      sq: 'Kontrollo nëse e nxore vërtet kuptimin nga teksti, jo nëse e hamendësove lirshëm kuptimin teknik.'
    }
  },
  {
    slug: 'c1-lesen-zwischen-detail-und-gesamtthese',
    topic: {
      de: 'Detail und Gesamtthese',
      en: 'detail and overall thesis',
      fa: 'جزئیات و تز کلی',
      ar: 'التفصيل والأطروحة العامة',
      tr: 'ayrıntı ve genel tez',
      ru: 'деталь и общий тезис',
      ckb: 'وردەکاری و تێزی گشتی',
      kmr: 'hûrgulî û teza giştî',
      pl: 'szczegół i ogólna teza',
      ro: 'detaliu și teză generală',
      sq: 'detaji dhe teza e përgjithshme'
    },
    focus: {
      de: 'ein Detail als Beleg, Beispiel oder Einschraenkung der Gesamtthese einordnen',
      en: 'classifying a detail as evidence, example, or limitation of the overall thesis',
      fa: 'تشخیص اینکه یک جزئیات، شاهد، مثال یا محدودکننده تز کلی است',
      ar: 'تصنيف التفصيل كدليل أو مثال أو تقييد للأطروحة العامة',
      tr: 'bir ayrıntıyı genel tezin kanıtı, örneği ya da sınırlaması olarak sınıflandırmayı',
      ru: 'классифицировать деталь как доказательство, пример или ограничение общего тезиса',
      ckb: 'پۆلێنکردنی وردەکاری وەک بەڵگە، نموونە یان سنووردارکەری تێزی گشتی',
      kmr: 'dabeşkirina hûrguliyê wek belge, mînak an sînorkera teza giştî',
      pl: 'klasyfikowanie szczegółu jako dowodu, przykładu albo ograniczenia tezy ogólnej',
      ro: 'încadrarea unui detaliu ca dovadă, exemplu sau limitare a tezei generale',
      sq: 'klasifikimin e një detaji si provë, shembull ose kufizim i tezës së përgjithshme'
    },
    grammar: ['c1-embedded-clauses', 'eingebettete Nebensaetze'],
    target: ['exam-prep-unit', 'c1-detail-und-gesamtthese-verbinden', 'Detail und Gesamtthese'],
    practice: {
      de: 'Waehle ein Detail und schreibe drei moegliche Funktionen: Beleg, Beispiel oder Einschraenkung. Entscheide dich dann fuer eine.',
      en: 'Choose one detail and write three possible functions: evidence, example, or limitation. Then decide on one.',
      fa: 'یک جزئیات انتخاب کن و سه نقش ممکن برایش بنویس: شاهد، مثال یا محدودکننده. سپس یکی را انتخاب کن.',
      ar: 'اختر تفصيلًا واحدًا واكتب ثلاث وظائف ممكنة له: دليل، مثال، أو تقييد. ثم اختر واحدة.',
      tr: 'Bir ayrıntı seç ve üç olası işlev yaz: kanıt, örnek ya da sınırlama. Sonra birine karar ver.',
      ru: 'Выбери одну деталь и запиши три возможные функции: доказательство, пример или ограничение. Затем выбери одну.',
      ckb: 'وردەکارییەک هەڵبژێرە و سێ ئەرکی گونجاوی بۆ بنووسە: بەڵگە، نموونە یان سنووردارکەر. پاشان یەکێکیان هەڵبژێرە.',
      kmr: 'Hûrguliyek hilbijêre û sê erkên gengaz binivîse: belge, mînak an sînorker. Paşê yekê hilbijêre.',
      pl: 'Wybierz jeden szczegół i zapisz trzy możliwe funkcje: dowód, przykład albo ograniczenie. Potem wybierz jedną.',
      ro: 'Alege un detaliu și scrie trei funcții posibile: dovadă, exemplu sau limitare. Apoi alege una.',
      sq: 'Zgjidh një detaj dhe shkruaj tri funksione të mundshme: provë, shembull ose kufizim. Pastaj zgjidh njërin.'
    },
    review: {
      de: 'Pruefe, ob das Detail wirklich zur Gesamtthese passt oder nur auffaellig wirkt.',
      en: 'Check whether the detail really fits the overall thesis or merely seems striking.',
      fa: 'بررسی کن که آن جزئیات واقعاً به تز کلی مربوط است یا فقط به چشم می‌آید.',
      ar: 'تحقّق مما إذا كان التفصيل يناسب فعلًا الأطروحة العامة أم أنه لافت فقط.',
      tr: 'Ayrıntının gerçekten genel teze uyduğunu mu, yoksa yalnızca dikkat çekici mi olduğunu kontrol et.',
      ru: 'Проверь, действительно ли деталь относится к общему тезису или просто бросается в глаза.',
      ckb: 'بپشکنە وردەکارییەکە بەڕاستی لەگەڵ تێزی گشتی دەگونجێت یان تەنها سەرنجڕاکێشە.',
      kmr: 'Kontrol bike ka hûrgulî rastî bi teza giştî re li hev tê, an tenê balkêş xuya dike.',
      pl: 'Sprawdź, czy szczegół naprawdę pasuje do ogólnej tezy, czy tylko rzuca się w oczy.',
      ro: 'Verifică dacă detaliul se potrivește cu adevărat tezei generale sau doar atrage atenția.',
      sq: 'Kontrollo nëse detaji lidhet vërtet me tezën e përgjithshme apo thjesht bie në sy.'
    }
  },
  {
    slug: 'c1-akademisches-lesen-und-quellenkritik-wiederholen',
    topic: {
      de: 'akademisches Lesen und Quellenkritik',
      en: 'academic reading and source criticism',
      fa: 'خواندن دانشگاهی و نقد منبع',
      ar: 'القراءة الأكاديمية ونقد المصدر',
      tr: 'akademik okuma ve kaynak eleştirisi',
      ru: 'академическое чтение и критика источников',
      ckb: 'خوێندنەوەی ئەکادیمی و ڕەخنەی سەرچاوە',
      kmr: 'xwendina akademîk û rexneya çavkaniyê',
      pl: 'czytanie akademickie i krytyka źródeł',
      ro: 'lectură academică și critică a sursei',
      sq: 'leximi akademik dhe kritika e burimit'
    },
    focus: {
      de: 'Struktur, These, Beleg, Bewertung und Quellenkritik zu einem kurzen Analyseablauf verbinden',
      en: 'linking structure, thesis, evidence, evaluation, and source criticism into a short analysis sequence',
      fa: 'وصل کردن ساختار، تز، شاهد، ارزیابی و نقد منبع به یک روند تحلیلی کوتاه',
      ar: 'ربط البنية والأطروحة والدليل والتقييم ونقد المصدر في تسلسل تحليلي قصير',
      tr: 'yapı, tez, kanıt, değerlendirme ve kaynak eleştirisini kısa bir analiz akışında birleştirmeyi',
      ru: 'связать структуру, тезис, доказательство, оценку и критику источника в короткую аналитическую последовательность',
      ckb: 'بەستنی پێکهاتە، تێز، بەڵگە، هەڵسەنگاندن و ڕەخنەی سەرچاوە بە ڕێڕەوێکی شیکاری کورت',
      kmr: 'girêdana avahî, tez, belge, nirxandin û rexneya çavkaniyê di rêzeke analîtîk a kurt de',
      pl: 'połączenie struktury, tezy, dowodu, oceny i krytyki źródła w krótki tok analizy',
      ro: 'legarea structurii, tezei, dovezii, evaluării și criticii sursei într-un scurt parcurs analitic',
      sq: 'lidhjen e strukturës, tezës, provës, vlerësimit dhe kritikës së burimit në një rrjedhë të shkurtër analize'
    },
    grammar: ['c1-c1-academic-grammar-review', 'akademische Grammatik-Review'],
    target: ['writing-template', 'c1-quellenpositionen-synthetisieren', 'Quellenpositionen synthetisieren'],
    practice: {
      de: 'Schreibe einen Analyseabsatz mit fuenf Funktionen: Strukturhinweis, These, Beleg, Bewertung und Quellenbegrenzung.',
      en: 'Write an analysis paragraph with five functions: structure cue, thesis, evidence, evaluation, and source limitation.',
      fa: 'یک پاراگراف تحلیلی با پنج نقش بنویس: اشاره به ساختار، تز، شاهد، ارزیابی و محدودیت منبع.',
      ar: 'اكتب فقرة تحليلية بخمس وظائف: إشارة إلى البنية، أطروحة، دليل، تقييم، وحدود المصدر.',
      tr: 'Beş işlevli bir analiz paragrafı yaz: yapı işareti, tez, kanıt, değerlendirme ve kaynak sınırlaması.',
      ru: 'Напиши аналитический абзац с пятью функциями: указание структуры, тезис, доказательство, оценка и ограничение источника.',
      ckb: 'پاراگرافێکی شیکاری بە پێنج ئەرک بنووسە: ئاماژەی پێکهاتە، تێز، بەڵگە، هەڵسەنگاندن و سنووری سەرچاوە.',
      kmr: 'Paragrafeke analîtîk bi pênc erkan binivîse: nîşana avahiyê, tez, belge, nirxandin û sînorê çavkaniyê.',
      pl: 'Napisz akapit analityczny z pięcioma funkcjami: wskazanie struktury, teza, dowód, ocena i ograniczenie źródła.',
      ro: 'Scrie un paragraf analitic cu cinci funcții: indiciu de structură, teză, dovadă, evaluare și limitarea sursei.',
      sq: 'Shkruaj një paragraf analitik me pesë funksione: shenjë strukture, tezë, provë, vlerësim dhe kufizim burimi.'
    },
    review: {
      de: 'Lies den Absatz laut und pruefe, ob Analyse und Quellenkritik zusammenhaengen, statt als zwei getrennte Listen zu wirken.',
      en: 'Read the paragraph aloud and check whether analysis and source criticism are connected, rather than sounding like two separate lists.',
      fa: 'پاراگراف را بلند بخوان و بررسی کن که تحلیل و نقد منبع به هم وصل‌اند، نه اینکه مثل دو فهرست جدا به نظر برسند.',
      ar: 'اقرأ الفقرة بصوت عالٍ وتحقق من أن التحليل ونقد المصدر مترابطان ولا يبدوان كقائمتين منفصلتين.',
      tr: 'Paragrafı sesli oku ve analiz ile kaynak eleştirisinin bağlantılı olduğunu, iki ayrı liste gibi durmadığını kontrol et.',
      ru: 'Прочитай абзац вслух и проверь, связаны ли анализ и критика источника, а не звучат как два отдельных списка.',
      ckb: 'پاراگرافەکە بە دەنگی بەرز بخوێنەوە و بپشکنە شیکاری و ڕەخنەی سەرچاوە پێکەوە بەستراون، نەک وەک دوو لیستی جیاواز بدەنێن.',
      kmr: 'Paragrafê bi dengê bilind bixwîne û kontrol bike ka analîz û rexneya çavkaniyê girêdayî ne, ne wek du lîsteyên cuda xuya dikin.',
      pl: 'Przeczytaj akapit na głos i sprawdź, czy analiza i krytyka źródła są połączone, a nie brzmią jak dwie osobne listy.',
      ro: 'Citește paragraful cu voce tare și verifică dacă analiza și critica sursei sunt legate, nu par două liste separate.',
      sq: 'Lexoje paragrafin me zë dhe kontrollo nëse analiza dhe kritika e burimit janë të lidhura, jo si dy lista të ndara.'
    }
  }
];

function readInstruction(item) {
  return {
    de: `Lies den Lektionstext zu ${item.topic.de} und markiere, welche Stellen du zuerst global und welche du erst im zweiten Durchgang genau lesen musst.`,
    en: `Read the lesson text on ${item.topic.en} and mark which parts you should read globally first and which parts require close reading on the second pass.`,
    fa: `متن درس درباره ${item.topic.fa} را بخوان و مشخص کن کدام بخش‌ها را اول باید کلی بخوانی و کدام بخش‌ها در دور دوم نیاز به خواندن دقیق دارند.`,
    ar: `اقرأ نص الدرس عن ${item.topic.ar} وحدّد أي الأجزاء تقرؤها أولًا قراءة عامة وأيها يحتاج إلى قراءة دقيقة في الجولة الثانية.`,
    tr: `${item.topic.tr} konulu ders metnini oku ve hangi bölümleri önce genel, hangilerini ikinci turda ayrıntılı okuman gerektiğini işaretle.`,
    ru: `Прочитай текст урока о теме «${item.topic.ru}» и отметь, какие места сначала нужно прочитать глобально, а какие во втором проходе подробно.`,
    ckb: `دەقی وانەکە لەسەر ${item.topic.ckb} بخوێنەوە و دیاری بکە کام بەشەکان سەرەتا بە گشتی و کام بەشەکان لە جارێکی دووەمدا بە وردی بخوێنرێنەوە.`,
    kmr: `Nivîsa dersê li ser ${item.topic.kmr} bixwîne û nîşan bike kîjan beş pêşî bi giştî û kîjan beş di cara duyem de bi hûrgulî bêne xwendin.`,
    pl: `Przeczytaj tekst lekcji o temacie: ${item.topic.pl} i zaznacz, które fragmenty najpierw czytać globalnie, a które dokładnie w drugim czytaniu.`,
    ro: `Citește textul lecției despre ${item.topic.ro} și marchează ce părți trebuie citite mai întâi global și care cer lectură atentă la a doua trecere.`,
    sq: `Lexo tekstin e mësimit për ${item.topic.sq} dhe shëno cilat pjesë duhen lexuar fillimisht globalisht dhe cilat kërkojnë lexim të saktë në raundin e dytë.`
  };
}

function structureInstruction(item) {
  return {
    de: `Oeffne ${item.grammar[1]} und sammle drei Formulierungen, mit denen du ${item.focus.de} sprachlich sauber ausdrueckst.`,
    en: `Open ${item.grammar[1]} and collect three formulations that help you express ${item.focus.en} cleanly.`,
    fa: `بخش ${item.grammar[1]} را باز کن و سه عبارت پیدا کن که کمک کند ${item.focus.fa} را روشن و دقیق بیان کنی.`,
    ar: `افتح قسم ${item.grammar[1]} واجمع ثلاث صيغ تساعدك على التعبير بوضوح عن ${item.focus.ar}.`,
    tr: `${item.grammar[1]} bölümünü aç ve ${item.focus.tr} açık biçimde ifade etmene yardım eden üç kalıp topla.`,
    ru: `Открой раздел ${item.grammar[1]} и собери три формулировки, которые помогут ясно выразить: ${item.focus.ru}.`,
    ckb: `بەشی ${item.grammar[1]} بکەرەوە و سێ دەربڕین کۆبکەوە کە یارمەتیت بدات ${item.focus.ckb} بە ڕوونی دەرببڕیت.`,
    kmr: `Beşa ${item.grammar[1]} veke û sê derbirînan kom bike ku alîkar bin ${item.focus.kmr} zelal bibêjî.`,
    pl: `Otwórz sekcję ${item.grammar[1]} i zbierz trzy sformułowania, które pomogą jasno wyrazić: ${item.focus.pl}.`,
    ro: `Deschide secțiunea ${item.grammar[1]} și adună trei formulări care te ajută să exprimi clar: ${item.focus.ro}.`,
    sq: `Hap seksionin ${item.grammar[1]} dhe mblidh tri formulime që të ndihmojnë të shprehësh qartë: ${item.focus.sq}.`
  };
}

function materialInstruction(item) {
  return {
    de: `Bearbeite das verlinkte Material zu ${item.target[2]} und achte darauf, wie du ${item.focus.de} in einer realistischen C1-Leseaufgabe einsetzt.`,
    en: `Work through the linked material on ${item.target[2]} and notice how to use ${item.focus.en} in a realistic C1 reading task.`,
    fa: `محتوای لینک‌شده درباره ${item.target[2]} را انجام بده و دقت کن چطور می‌توان ${item.focus.fa} را در یک تمرین واقعی خواندن C1 به کار برد.`,
    ar: `اعمل على المادة المرتبطة حول ${item.target[2]} وانتبه إلى كيفية استخدام ${item.focus.ar} في مهمة قراءة واقعية على مستوى C1.`,
    tr: `${item.target[2]} hakkındaki bağlantılı malzemeyi çalış ve ${item.focus.tr} gerçekçi bir C1 okuma görevinde nasıl kullanabileceğine dikkat et.`,
    ru: `Проработай связанный материал по теме ${item.target[2]} и обрати внимание, как использовать ${item.focus.ru} в реалистичном задании на чтение C1.`,
    ckb: `ماتریاڵی بەستەرکراو لەسەر ${item.target[2]} کاربکە و سەرنج بدە چۆن ${item.focus.ckb} لە ئەرکێکی ڕاستەقینەی خوێندنەوەی C1دا بەکاربهێنیت.`,
    kmr: `Li ser materyala girêdayî ya ${item.target[2]} bixebite û bala xwe bide ka çawa ${item.focus.kmr} di erkeke xwendinê ya rastîn a C1 de bi kar tînî.`,
    pl: `Przerób połączony materiał o ${item.target[2]} i zwróć uwagę, jak zastosować ${item.focus.pl} w realistycznym zadaniu czytania C1.`,
    ro: `Lucrează cu materialul legat despre ${item.target[2]} și observă cum folosești ${item.focus.ro} într-o sarcină realistă de lectură C1.`,
    sq: `Puno me materialin e lidhur për ${item.target[2]} dhe vëzhgo si përdoret ${item.focus.sq} në një detyrë reale leximi C1.`
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
    block('read', 'read', readInstruction(item), 'none', null, 7, 10, true),
    block('grammar', 'structure', structureInstruction(item), 'grammar-topic', item.grammar[0], 8, 20, true),
    block(item.target[0] === 'writing-template' ? 'write' : item.target[0] === 'roleplay' ? 'roleplay' : 'exam-prep', 'material', materialInstruction(item), item.target[0], item.target[1], 9, 30, true),
    block('practice', 'practice', item.practice, 'none', null, 8, 40, true),
    block('review', 'review', item.review, 'none', null, 6, 50, true)
  ];
}

fs.writeFileSync(file, JSON.stringify(data, null, 2) + '\n', 'utf8');
console.log(`Updated ${items.length} C1 Module 3 lessons with ${items.length * 5} activity blocks.`);

const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c1-souveraen-in-studium-beruf-und-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'Schreibziel klaeren',
    en: 'Clarify the writing goal',
    fa: 'هدف نوشتن را روشن کن',
    ar: 'وضّح هدف الكتابة',
    tr: 'Yazma hedefini netleştir',
    ru: 'Проясни цель письма',
    ckb: 'ئامانجی نووسین ڕوون بکەوە',
    kmr: 'Armanca nivîsînê zelal bike',
    pl: 'Wyjaśnij cel pisania',
    ro: 'Clarifică scopul scrierii',
    sq: 'Qartëso qëllimin e shkrimit'
  },
  model: {
    de: 'Form und Stil pruefen',
    en: 'Check form and style',
    fa: 'فرم و سبک را بررسی کن',
    ar: 'افحص الشكل والأسلوب',
    tr: 'Biçim ve üslubu kontrol et',
    ru: 'Проверь форму и стиль',
    ckb: 'فۆرم و ستایل بپشکنە',
    kmr: 'Form û şêwazê kontrol bike',
    pl: 'Sprawdź formę i styl',
    ro: 'Verifică forma și stilul',
    sq: 'Kontrollo formën dhe stilin'
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
  write: {
    de: 'Textbaustein schreiben',
    en: 'Write a text block',
    fa: 'یک بخش از متن بنویس',
    ar: 'اكتب جزءًا من النص',
    tr: 'Bir metin bölümü yaz',
    ru: 'Напиши фрагмент текста',
    ckb: 'بەشێک لە دەق بنووسە',
    kmr: 'Parçeyek ji nivîsê binivîse',
    pl: 'Napisz fragment tekstu',
    ro: 'Scrie un fragment de text',
    sq: 'Shkruaj një pjesë teksti'
  },
  review: {
    de: 'Text ueberarbeiten',
    en: 'Revise the text',
    fa: 'متن را بازبینی کن',
    ar: 'راجع النص',
    tr: 'Metni gözden geçir',
    ru: 'Отредактируй текст',
    ckb: 'دەقەکە پێداچوونەوە بکە',
    kmr: 'Nivîsê ji nû ve binêre',
    pl: 'Popraw tekst',
    ro: 'Revizuiește textul',
    sq: 'Rishiko tekstin'
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
    slug: 'c1-formelle-zusammenfassung-schreiben',
    topic: {
      de: 'formelle Zusammenfassung',
      en: 'formal summary',
      fa: 'خلاصه رسمی',
      ar: 'ملخص رسمي',
      tr: 'resmî özet',
      ru: 'официальное резюме',
      ckb: 'پوختەی فەرمی',
      kmr: 'kurteya fermî',
      pl: 'formalne streszczenie',
      ro: 'rezumat formal',
      sq: 'përmbledhje formale'
    },
    focus: {
      de: 'Inhalt knapp wiedergeben, ohne Bewertung oder eigene Meinung einzumischen',
      en: 'presenting content concisely without mixing in evaluation or personal opinion',
      fa: 'بازگویی کوتاه محتوا بدون وارد کردن ارزیابی یا نظر شخصی',
      ar: 'عرض المحتوى بإيجاز من دون إدخال تقييم أو رأي شخصي',
      tr: 'içeriği değerlendirme ya da kişisel görüş katmadan kısa aktarmayı',
      ru: 'кратко передавать содержание, не смешивая его с оценкой или личным мнением',
      ckb: 'گەیاندنی ناوەڕۆک بە کورتی بەبێ تێکەڵکردنی هەڵسەنگاندن یان بۆچوونی خۆت',
      kmr: 'naverokê bi kurtî vegotin bêyî tevlihevkirina nirxandin an raya xwe',
      pl: 'zwięzłe oddanie treści bez mieszania oceny lub własnej opinii',
      ro: 'redarea concisă a conținutului fără evaluare sau opinie personală',
      sq: 'paraqitjen e shkurtër të përmbajtjes pa përzier vlerësim ose mendim personal'
    },
    grammar: ['c1-formal-summaries', 'formale Zusammenfassungen'],
    target: ['exam-prep-unit', 'c1-zusammenfassung-und-bewertung-trennen', 'Zusammenfassung und Bewertung trennen'],
    practice: {
      de: 'Schreibe eine Zusammenfassung in drei Saetzen: Thema, Hauptaussage und zentrale Begruendung. Verzichte auf Ich finde.',
      en: 'Write a three-sentence summary: topic, main claim, and central justification. Avoid using “I think”.',
      fa: 'یک خلاصه سه‌جمله‌ای بنویس: موضوع، پیام اصلی و دلیل مرکزی. از عبارت‌هایی مثل «به نظر من» استفاده نکن.',
      ar: 'اكتب ملخصًا في ثلاث جمل: الموضوع، الفكرة الرئيسة، والتعليل المركزي. تجنّب عبارات مثل «أرى أن».',
      tr: 'Üç cümlelik bir özet yaz: konu, ana iddia ve temel gerekçe. “Bence” türü ifadelerden kaçın.',
      ru: 'Напиши резюме в трех предложениях: тема, главный тезис и центральное обоснование. Избегай выражений типа «я считаю».',
      ckb: 'پوختەیەک بە سێ ڕستە بنووسە: بابەت، وتەی سەرەکی و هۆکاری ناوەندی. لە دەربڕینی وەک «بە بۆچوونی من» خۆ بپارێزە.',
      kmr: 'Kurteyek bi sê hevokan binivîse: mijar, gotina sereke û sedema navendî. Ji gotinên wekî “li gorî min” dûr bikeve.',
      pl: 'Napisz streszczenie w trzech zdaniach: temat, główna teza i kluczowe uzasadnienie. Unikaj zwrotów typu „moim zdaniem”.',
      ro: 'Scrie un rezumat în trei propoziții: tema, ideea principală și justificarea centrală. Evită formulări de tipul „eu cred”.',
      sq: 'Shkruaj një përmbledhje me tri fjali: tema, ideja kryesore dhe arsyeja qendrore. Shmang shprehje si “mendoj se”.'
    },
    review: {
      de: 'Pruefe, ob jeder Satz wirklich zusammenfasst und nicht schon bewertet, kommentiert oder ergaenzt.',
      en: 'Check whether each sentence really summarizes and does not already evaluate, comment, or add extra content.',
      fa: 'بررسی کن که هر جمله واقعاً خلاصه می‌کند و هنوز وارد ارزیابی، نظر دادن یا اضافه کردن مطلب نشده است.',
      ar: 'تحقّق من أن كل جملة تلخّص فعلًا ولا تبدأ بالتقييم أو التعليق أو إضافة معلومات.',
      tr: 'Her cümlenin gerçekten özetlediğini, değerlendirme, yorum ya da ekleme yapmadığını kontrol et.',
      ru: 'Проверь, действительно ли каждое предложение резюмирует, а не оценивает, комментирует или добавляет новое.',
      ckb: 'بپشکنە هەر ڕستەیەک بەڕاستی پوختە دەکات، نەک هەڵسەنگاندن، کۆمێنت یان زیادکردن بکات.',
      kmr: 'Kontrol bike ka her hevok rastî kurt dike, ne ku nirxandin, şîrove an zêdekirin dike.',
      pl: 'Sprawdź, czy każde zdanie naprawdę streszcza, a nie ocenia, komentuje ani dopowiada.',
      ro: 'Verifică dacă fiecare propoziție chiar rezumă și nu evaluează, comentează sau adaugă deja ceva.',
      sq: 'Kontrollo nëse çdo fjali vërtet përmbledh dhe nuk vlerëson, komenton ose shton diçka.'
    }
  },
  {
    slug: 'c1-essaystruktur-und-roter-faden',
    topic: {
      de: 'Essaystruktur und roter Faden',
      en: 'essay structure and line of thought',
      fa: 'ساختار essay و خط فکری پیوسته',
      ar: 'بنية المقال والخيط المنطقي',
      tr: 'essay yapısı ve düşünce akışı',
      ru: 'структура эссе и логическая линия',
      ckb: 'پێکهاتەی ئێسەی و ڕێڕەوی بیرکردنەوە',
      kmr: 'avahiya essayê û rêça ramînê',
      pl: 'struktura eseju i spójny tok myśli',
      ro: 'structura eseului și firul logic',
      sq: 'struktura e esesë dhe rrjedha logjike'
    },
    focus: {
      de: 'Abschnitte so verbinden, dass Leser die Argumentation ohne Sucharbeit verfolgen koennen',
      en: 'connecting paragraphs so readers can follow the argument without having to search for the logic',
      fa: 'وصل کردن پاراگراف‌ها طوری که خواننده بدون حدس زدن، مسیر استدلال را دنبال کند',
      ar: 'ربط الفقرات بحيث يستطيع القارئ متابعة الحجة من دون أن يبحث عن منطقها',
      tr: 'paragrafları okuyucunun mantığı aramak zorunda kalmadan argümanı izleyebileceği şekilde bağlamayı',
      ru: 'связывать абзацы так, чтобы читатель следил за аргументом без поиска логики',
      ckb: 'بەستنی پەرەگرافەکان بە شێوەیەک خوێنەر بەبێ گەڕان بەدوای لۆجیکدا ڕێڕەوی ئارگیومێنت ببینێت',
      kmr: 'girêdana paragrafan wisa ku xwîner bê lêgerîna mantiqê argumanê bişopîne',
      pl: 'łączenie akapitów tak, aby czytelnik śledził argument bez szukania logiki',
      ro: 'legarea paragrafelor astfel încât cititorul să urmărească argumentul fără să caute logica',
      sq: 'lidhjen e paragrafëve që lexuesi ta ndjekë argumentin pa kërkuar logjikën'
    },
    grammar: ['c1-formal-style-in-essays', 'formaler Essaystil'],
    target: ['writing-template', 'c1-akademische-stellungnahme-einordnen', 'akademische Stellungnahme'],
    practice: {
      de: 'Plane einen Essay mit vier Stationen: Ausgangsproblem, These, Gegenperspektive und Schlussfolgerung mit Begrenzung.',
      en: 'Plan an essay in four stages: starting problem, thesis, opposing perspective, and conclusion with limitation.',
      fa: 'یک essay را در چهار ایستگاه برنامه‌ریزی کن: مسئله آغازین، تز، دیدگاه مقابل و نتیجه‌گیری همراه با محدودیت.',
      ar: 'خطط لمقال في أربع مراحل: المشكلة الأولية، الأطروحة، المنظور المقابل، والاستنتاج مع تقييد.',
      tr: 'Bir essayi dört aşamada planla: başlangıç problemi, tez, karşı bakış ve sınırlı sonuç.',
      ru: 'Спланируй эссе в четыре этапа: исходная проблема, тезис, противоположная перспектива и вывод с ограничением.',
      ckb: 'ئێسەیەک بە چوار وێستگە پلان بکە: کێشەی دەستپێک، تێز، ڕوانگەی بەرامبەر و دەرئەنجام لەگەڵ سنوور.',
      kmr: 'Essayekê di çar qonaxan de plan bike: pirsgirêka destpêkê, tez, perspektîfa dijber û encam bi sînor.',
      pl: 'Zaplanuj esej w czterech punktach: problem wyjściowy, teza, perspektywa przeciwna i wniosek z ograniczeniem.',
      ro: 'Planifică un eseu în patru etape: problema de pornire, teza, perspectiva opusă și concluzia cu limitare.',
      sq: 'Planifiko një ese në katër pika: problemi fillestar, teza, këndvështrimi i kundërt dhe përfundimi me kufizim.'
    },
    review: {
      de: 'Pruefe, ob jeder Abschnitt eine klare Funktion hat und nicht nur ein weiteres Beispiel hinzufuegt.',
      en: 'Check whether each paragraph has a clear function and does not simply add another example.',
      fa: 'بررسی کن که هر پاراگراف نقش روشنی دارد و فقط یک مثال دیگر به متن اضافه نمی‌کند.',
      ar: 'تحقّق من أن لكل فقرة وظيفة واضحة ولا تضيف مثالًا آخر فقط.',
      tr: 'Her paragrafın açık bir işlevi olduğunu ve yalnızca başka bir örnek eklemediğini kontrol et.',
      ru: 'Проверь, есть ли у каждого абзаца ясная функция, а не просто еще один пример.',
      ckb: 'بپشکنە هەر پەرەگرافێک ئەرکێکی ڕوونی هەیە، نەک تەنها نموونەیەکی تر زیاد بکات.',
      kmr: 'Kontrol bike ka her paragraf erkeke zelal heye, ne tenê mînakeke din zêde dike.',
      pl: 'Sprawdź, czy każdy akapit ma jasną funkcję, a nie tylko dodaje kolejny przykład.',
      ro: 'Verifică dacă fiecare paragraf are o funcție clară și nu adaugă doar un alt exemplu.',
      sq: 'Kontrollo nëse çdo paragraf ka funksion të qartë dhe nuk shton thjesht një shembull tjetër.'
    }
  },
  {
    slug: 'c1-formaler-stil-ohne-ueberladung',
    topic: {
      de: 'formaler Stil ohne Ueberladung',
      en: 'formal style without overload',
      fa: 'سبک رسمی بدون سنگین‌نویسی افراطی',
      ar: 'أسلوب رسمي من دون إثقال زائد',
      tr: 'aşırı yüklenmemiş resmî üslup',
      ru: 'официальный стиль без перегрузки',
      ckb: 'ستایلی فەرمی بەبێ قورسکردنی زیادە',
      kmr: 'şêwaza fermî bê barkirina zêde',
      pl: 'styl formalny bez przeciążenia',
      ro: 'stil formal fără supraîncărcare',
      sq: 'stil formal pa mbingarkesë'
    },
    focus: {
      de: 'praezise und formal schreiben, ohne den Text kuenstlich kompliziert zu machen',
      en: 'writing precisely and formally without making the text artificially complicated',
      fa: 'دقیق و رسمی نوشتن بدون اینکه متن را مصنوعی و بی‌دلیل پیچیده کنی',
      ar: 'الكتابة بدقة وبأسلوب رسمي من دون جعل النص معقدًا بشكل مصطنع',
      tr: 'metni yapay biçimde karmaşıklaştırmadan kesin ve resmî yazmayı',
      ru: 'писать точно и официально, не делая текст искусственно сложным',
      ckb: 'نووسینی ورد و فەرمی بەبێ ئەوەی دەقەکە بە دەستکردی ئاڵۆز بکەیت',
      kmr: 'nivîsîna hûrgulî û fermî bêyî ku nivîsê bi awayekî çêkirî aloz bikî',
      pl: 'pisanie precyzyjne i formalne bez sztucznego komplikowania tekstu',
      ro: 'scriere precisă și formală fără a complica artificial textul',
      sq: 'të shkruash saktë dhe formalisht pa e komplikuar tekstin artificialisht'
    },
    grammar: ['c1-formal-style-in-essays', 'formaler Essaystil'],
    target: ['exam-prep-unit', 'c1-formelle-textproduktion-schaerfen', 'formelle Textproduktion'],
    practice: {
      de: 'Ueberarbeite zwei Saetze: Ersetze vage Woerter durch praezise Begriffe und streiche unnoetig schwere Formulierungen.',
      en: 'Revise two sentences: replace vague words with precise terms and remove unnecessarily heavy wording.',
      fa: 'دو جمله را بازنویسی کن: واژه‌های مبهم را با اصطلاحات دقیق جایگزین کن و عبارت‌های بی‌دلیل سنگین را حذف کن.',
      ar: 'راجع جملتين: استبدل الكلمات الغامضة بمصطلحات دقيقة واحذف الصيغ الثقيلة بلا داعٍ.',
      tr: 'İki cümleyi gözden geçir: belirsiz kelimeleri kesin terimlerle değiştir ve gereksiz ağır ifadeleri çıkar.',
      ru: 'Отредактируй два предложения: замени расплывчатые слова точными понятиями и убери ненужно тяжёлые формулировки.',
      ckb: 'دوو ڕستە پێداچوونەوە بکە: وشە ناڕوونەکان بە زاراوەی ورد بگۆڕە و دەربڕینی قورسی بێهۆ بسڕەوە.',
      kmr: 'Du hevokan ji nû ve binêre: peyvên nezelal bi têgehên hûrgulî biguherîne û gotinên giran ên bêhewce jê bibe.',
      pl: 'Popraw dwa zdania: zastąp niejasne słowa precyzyjnymi pojęciami i usuń niepotrzebnie ciężkie sformułowania.',
      ro: 'Revizuiește două propoziții: înlocuiește cuvintele vagi cu termeni preciși și elimină formulările inutil de grele.',
      sq: 'Rishiko dy fjali: zëvendëso fjalët e paqarta me terma të saktë dhe hiq formulimet e rënda pa nevojë.'
    },
    review: {
      de: 'Pruefe, ob dein Text formell wirkt, weil er klar ist, nicht nur weil er lange Woerter enthaelt.',
      en: 'Check whether your text sounds formal because it is clear, not merely because it contains long words.',
      fa: 'بررسی کن که متن تو به خاطر وضوحش رسمی به نظر می‌رسد، نه فقط به خاطر واژه‌های طولانی و سنگین.',
      ar: 'تحقّق مما إذا كان النص يبدو رسميًا لأنه واضح، لا لأنه يحتوي فقط على كلمات طويلة.',
      tr: 'Metninin yalnızca uzun kelimeler içerdiği için değil, açık olduğu için resmî göründüğünü kontrol et.',
      ru: 'Проверь, звучит ли текст официально благодаря ясности, а не просто из-за длинных слов.',
      ckb: 'بپشکنە دەقەکەت لەبەر ڕوونییەکەی فەرمی دەنێت، نەک تەنها لەبەر وشە درێژەکان.',
      kmr: 'Kontrol bike ka nivîsa te ji ber zelalbûnê fermî xuya dike, ne tenê ji ber peyvên dirêj.',
      pl: 'Sprawdź, czy tekst brzmi formalnie dzięki jasności, a nie tylko dzięki długim słowom.',
      ro: 'Verifică dacă textul pare formal pentru că este clar, nu doar pentru că are cuvinte lungi.',
      sq: 'Kontrollo nëse teksti duket formal sepse është i qartë, jo vetëm sepse ka fjalë të gjata.'
    }
  },
  {
    slug: 'c1-nominalstil-und-verbalstil-abwaegen',
    topic: {
      de: 'Nominalstil und Verbalstil',
      en: 'nominal style and verbal style',
      fa: 'سبک اسمی و سبک فعلی',
      ar: 'الأسلوب الاسمي والأسلوب الفعلي',
      tr: 'isim ağırlıklı ve fiil ağırlıklı üslup',
      ru: 'именной и глагольный стиль',
      ckb: 'ستایلی ناوی و ستایلی کرداری',
      kmr: 'şêwaza navî û şêwaza lêkerî',
      pl: 'styl nominalny i werbalny',
      ro: 'stil nominal și stil verbal',
      sq: 'stili nominal dhe stili verbal'
    },
    focus: {
      de: 'Nominalstil nur dort einsetzen, wo er Praezision schafft und nicht Lesbarkeit nimmt',
      en: 'using nominal style only where it creates precision and does not reduce readability',
      fa: 'استفاده از سبک اسمی فقط جایی که دقت می‌آورد و خوانایی را کم نمی‌کند',
      ar: 'استخدام الأسلوب الاسمي فقط حيث يضيف دقة ولا يضعف قابلية القراءة',
      tr: 'isim ağırlıklı üslubu yalnızca kesinlik sağladığında ve okunabilirliği azaltmadığında kullanmayı',
      ru: 'использовать именной стиль только там, где он повышает точность и не снижает читаемость',
      ckb: 'بەکارهێنانی ستایلی ناوی تەنها لەو شوێنەی وردی زیاد دەکات و خوێندنەوە سەخت ناکات',
      kmr: 'şêwaza navî tenê li cihê ku hûrgulî çêdike û xwendinê dijwar nake bi kar anîn',
      pl: 'używanie stylu nominalnego tylko tam, gdzie zwiększa precyzję i nie obniża czytelności',
      ro: 'folosirea stilului nominal doar unde aduce precizie și nu scade lizibilitatea',
      sq: 'përdorimin e stilit nominal vetëm kur shton saktësi dhe nuk ul lexueshmërinë'
    },
    grammar: ['c1-nominal-style-versus-verbal-style', 'Nominalstil versus Verbalstil'],
    target: ['exam-prep-unit', 'c1-nominalstil-und-verbalstil-pruefen', 'Nominalstil und Verbalstil pruefen'],
    practice: {
      de: 'Schreibe eine Aussage einmal nominal und einmal verbal. Entscheide, welche Version fuer eine formelle Antwort klarer ist.',
      en: 'Write one statement once in nominal style and once in verbal style. Decide which version is clearer for a formal answer.',
      fa: 'یک جمله را یک‌بار با سبک اسمی و یک‌بار با سبک فعلی بنویس. مشخص کن کدام نسخه برای پاسخ رسمی روشن‌تر است.',
      ar: 'اكتب عبارة مرة بالأسلوب الاسمي ومرة بالأسلوب الفعلي. حدّد أي نسخة أوضح لإجابة رسمية.',
      tr: 'Bir ifadeyi bir kez isim ağırlıklı, bir kez fiil ağırlıklı yaz. Resmî cevap için hangisinin daha açık olduğuna karar ver.',
      ru: 'Напиши одно высказывание один раз в именном, а один раз в глагольном стиле. Реши, какая версия яснее для официального ответа.',
      ckb: 'وتەیەک جارێک بە ستایلی ناوی و جارێک بە ستایلی کرداری بنووسە. بڕیار بدە کام وەشان بۆ وەڵامی فەرمی ڕوونترە.',
      kmr: 'Gotinek carekê bi şêwaza navî û carekê bi şêwaza lêkerî binivîse. Biryara xwe bide ka kîjan ji bo bersiveke fermî zelaltir e.',
      pl: 'Napisz jedno stwierdzenie raz w stylu nominalnym i raz w werbalnym. Wybierz, która wersja jest jaśniejsza w formalnej odpowiedzi.',
      ro: 'Scrie o afirmație o dată în stil nominal și o dată în stil verbal. Decide care versiune este mai clară pentru un răspuns formal.',
      sq: 'Shkruaj një pohim një herë në stil nominal dhe një herë në stil verbal. Vendos cili version është më i qartë për një përgjigje formale.'
    },
    review: {
      de: 'Pruefe, ob die nominale Version wirklich dichter ist oder nur schwerer zu lesen.',
      en: 'Check whether the nominal version is truly more compact or only harder to read.',
      fa: 'بررسی کن که نسخه اسمی واقعاً فشرده‌تر و دقیق‌تر است یا فقط خواندنش سخت‌تر شده است.',
      ar: 'تحقّق مما إذا كانت النسخة الاسمية أكثر كثافة فعلًا أم أنها فقط أصعب في القراءة.',
      tr: 'İsim ağırlıklı versiyonun gerçekten daha yoğun mu, yoksa sadece daha zor okunur mu olduğunu kontrol et.',
      ru: 'Проверь, действительно ли именная версия плотнее или просто труднее читается.',
      ckb: 'بپشکنە وەشانی ناوی بەڕاستی چڕترە یان تەنها خوێندنەوەی سەختترە.',
      kmr: 'Kontrol bike ka guhertoya navî rastî qelewtir e, an tenê xwendina wê dijwartir e.',
      pl: 'Sprawdź, czy wersja nominalna jest naprawdę bardziej zwarta, czy tylko trudniejsza do czytania.',
      ro: 'Verifică dacă versiunea nominală este cu adevărat mai densă sau doar mai greu de citit.',
      sq: 'Kontrollo nëse versioni nominal është vërtet më i dendur apo thjesht më i vështirë për t’u lexuar.'
    }
  },
  {
    slug: 'c1-erweiterte-nominalisierung',
    topic: {
      de: 'erweiterte Nominalisierung',
      en: 'advanced nominalization',
      fa: 'اسمی‌سازی پیشرفته',
      ar: 'التحويل الاسمي المتقدم',
      tr: 'ileri düzey adlaştırma',
      ru: 'расширенная номинализация',
      ckb: 'ناوکردنی پێشکەوتوو',
      kmr: 'navkirina pêşketî',
      pl: 'zaawansowana nominalizacja',
      ro: 'nominalizare avansată',
      sq: 'nominalizim i avancuar'
    },
    focus: {
      de: 'Handlungen verdichten, ohne Akteur, Verantwortung oder zeitliche Logik zu verlieren',
      en: 'condensing actions without losing actor, responsibility, or temporal logic',
      fa: 'فشرده کردن کنش‌ها بدون از دست دادن فاعل، مسئولیت یا ترتیب زمانی',
      ar: 'تكثيف الأفعال من دون فقدان الفاعل أو المسؤولية أو التسلسل الزمني',
      tr: 'eylemleri özne, sorumluluk ya da zaman mantığını kaybetmeden yoğunlaştırmayı',
      ru: 'сжимать действия, не теряя действующее лицо, ответственность или временную логику',
      ckb: 'چڕکردنەوەی کردارەکان بەبێ ونکردنی بکەر، بەرپرسیاریەتی یان لۆجیکی کات',
      kmr: 'çrkirina çalakiyan bêyî windakirina kiryar, berpirsiyarî an mantiqa demê',
      pl: 'zagęszczanie działań bez utraty wykonawcy, odpowiedzialności lub logiki czasu',
      ro: 'condensarea acțiunilor fără a pierde actorul, responsabilitatea sau logica temporală',
      sq: 'ngjeshjen e veprimeve pa humbur vepruesin, përgjegjësinë ose logjikën kohore'
    },
    grammar: ['c1-advanced-nominalization', 'erweiterte Nominalisierung'],
    target: ['writing-template', 'c1-gutachtennahe-empfehlung-formulieren', 'gutachtennahe Empfehlung'],
    practice: {
      de: 'Nominalisiere eine Handlungskette und schreibe danach eine klarere verbale Version. Entscheide, welche Version im Kontext besser ist.',
      en: 'Nominalize a chain of actions and then write a clearer verbal version. Decide which version fits the context better.',
      fa: 'یک زنجیره کنش را اسمی‌سازی کن و بعد نسخه فعلی و روشن‌ترش را بنویس. تصمیم بگیر کدام نسخه در بافت متن بهتر است.',
      ar: 'حوّل سلسلة أفعال إلى صيغة اسمية ثم اكتب نسخة فعلية أوضح. حدّد أي النسختين أنسب للسياق.',
      tr: 'Bir eylem zincirini adlaştır, sonra daha açık bir fiilli versiyon yaz. Bağlam için hangisinin daha iyi olduğuna karar ver.',
      ru: 'Номинализируй цепочку действий, затем напиши более ясную глагольную версию. Реши, какая версия лучше подходит контексту.',
      ckb: 'زنجیرەی کردارێک ناوی بکە، پاشان وەشانێکی کرداریی ڕوونتر بنووسە. بڕیار بدە کام وەشان لە بافتەکەدا باشترە.',
      kmr: 'Zincîreya çalakiyekê navî bike, paşê guhertoyeke lêkerî ya zelaltir binivîse. Biryara xwe bide ka kîjan di çarçoveyê de çêtir e.',
      pl: 'Znominalizuj łańcuch działań, a potem napisz jaśniejszą wersję werbalną. Wybierz, która lepiej pasuje do kontekstu.',
      ro: 'Nominalizează un lanț de acțiuni, apoi scrie o versiune verbală mai clară. Decide care se potrivește mai bine contextului.',
      sq: 'Nominalizo një zinxhir veprimesh dhe pastaj shkruaj një version verbal më të qartë. Vendos cili përshtatet më mirë me kontekstin.'
    },
    review: {
      de: 'Pruefe, ob nach der Nominalisierung noch klar ist, wer handelt und worauf sich die Aussage bezieht.',
      en: 'Check whether it remains clear after nominalization who acts and what the statement refers to.',
      fa: 'بررسی کن که بعد از اسمی‌سازی هنوز روشن است چه کسی عمل می‌کند و جمله به چه چیزی اشاره دارد.',
      ar: 'تحقّق مما إذا كان واضحًا بعد التحويل الاسمي من يقوم بالفعل وإلى ماذا تشير العبارة.',
      tr: 'Adlaştırmadan sonra kimin eylem yaptığı ve ifadenin neye gönderme yaptığı hâlâ açık mı kontrol et.',
      ru: 'Проверь, остается ли после номинализации ясно, кто действует и к чему относится высказывание.',
      ckb: 'بپشکنە پاش ناوکردنەکە هێشتا ڕوونە کێ کردار دەکات و وتەکە ئاماژە بە چی دەکات.',
      kmr: 'Kontrol bike ka piştî navkirinê hîn jî zelal e kî dixebite û gotin bi çi ve girêdayî ye.',
      pl: 'Sprawdź, czy po nominalizacji nadal wiadomo, kto działa i do czego odnosi się wypowiedź.',
      ro: 'Verifică dacă după nominalizare rămâne clar cine acționează și la ce se referă afirmația.',
      sq: 'Kontrollo nëse pas nominalizimit mbetet e qartë kush vepron dhe kujt i referohet pohimi.'
    }
  },
  {
    slug: 'c1-konjunktiv-i-in-formellen-texten',
    topic: {
      de: 'Konjunktiv I in formellen Texten',
      en: 'Konjunktiv I in formal texts',
      fa: 'Konjunktiv I در متن‌های رسمی',
      ar: 'صيغة Konjunktiv I في النصوص الرسمية',
      tr: 'resmî metinlerde Konjunktiv I',
      ru: 'Konjunktiv I в официальных текстах',
      ckb: 'Konjunktiv I لە دەقی فەرمی',
      kmr: 'Konjunktiv I di nivîsên fermî de',
      pl: 'Konjunktiv I w tekstach formalnych',
      ro: 'Konjunktiv I în texte formale',
      sq: 'Konjunktiv I në tekste formale'
    },
    focus: {
      de: 'fremde Aussagen distanziert wiedergeben, ohne Zweifel zu erfinden',
      en: 'reporting other people’s statements with distance without inventing doubt',
      fa: 'بازگویی گفته دیگران با فاصله‌گذاری زبانی، بدون اینکه شک و تردید ساختگی ایجاد شود',
      ar: 'نقل أقوال الآخرين بمسافة لغوية من دون اختلاق شك غير موجود',
      tr: 'başkalarının sözlerini yapay şüphe yaratmadan dilsel mesafeyle aktarmayı',
      ru: 'передавать чужие высказывания с дистанцией, не создавая искусственного сомнения',
      ckb: 'گواستنەوەی وتەی کەسانی تر بە دووریی زمانی، بەبێ دروستکردنی گومانی دەستکرد',
      kmr: 'vegotina gotinên kesên din bi dûrmayîna zimanî bêyî afirandina gumaneke çêkirî',
      pl: 'relacjonowanie cudzych wypowiedzi z dystansem językowym bez tworzenia sztucznej wątpliwości',
      ro: 'redarea afirmațiilor altora cu distanță lingvistică fără a inventa îndoială',
      sq: 'raportimin e deklaratave të të tjerëve me distancë gjuhësore pa krijuar dyshim artificial'
    },
    grammar: ['c1-konjunktiv-i-for-reported-speech', 'Konjunktiv I fuer indirekte Rede'],
    target: ['exam-prep-unit', 'c1-formelle-textproduktion-schaerfen', 'formelle Textproduktion'],
    practice: {
      de: 'Formuliere zwei fremde Aussagen mit Konjunktiv I. Schreibe danach einen Satz, der deine eigene Bewertung klar davon trennt.',
      en: 'Formulate two reported statements with Konjunktiv I. Then write one sentence that clearly separates your own evaluation from them.',
      fa: 'دو گفته نقل‌شده را با Konjunktiv I بنویس. سپس یک جمله اضافه کن که ارزیابی خودت را روشن از آن‌ها جدا کند.',
      ar: 'صغ عبارتين منقولتين باستخدام Konjunktiv I. ثم اكتب جملة تفصل تقييمك أنت عنهما بوضوح.',
      tr: 'Aktarılan iki ifadeyi Konjunktiv I ile yaz. Sonra kendi değerlendirmeni onlardan açıkça ayıran bir cümle ekle.',
      ru: 'Сформулируй два чужих высказывания с Konjunktiv I. Затем напиши предложение, которое ясно отделяет твою оценку от них.',
      ckb: 'دوو وتەی گوازراوە بە Konjunktiv I بنووسە. پاشان ڕستەیەک بنووسە کە هەڵسەنگاندنی خۆت بە ڕوونی لێیان جیا بکاتەوە.',
      kmr: 'Du gotinên veguhestî bi Konjunktiv I binivîse. Paşê hevokekê binivîse ku nirxandina te bi zelalî ji wan cuda bike.',
      pl: 'Sformułuj dwie cudze wypowiedzi z Konjunktiv I. Potem napisz zdanie, które jasno oddziela twoją ocenę.',
      ro: 'Formulează două afirmații raportate cu Konjunktiv I. Apoi scrie o propoziție care separă clar propria evaluare.',
      sq: 'Formulo dy deklarata të raportuara me Konjunktiv I. Pastaj shkruaj një fjali që ndan qartë vlerësimin tënd.'
    },
    review: {
      de: 'Pruefe, ob Konjunktiv I hier Distanz zur Quelle zeigt und nicht versehentlich Ironie oder Misstrauen erzeugt.',
      en: 'Check whether Konjunktiv I shows distance from the source here and does not accidentally create irony or mistrust.',
      fa: 'بررسی کن که Konjunktiv I اینجا فقط فاصله‌گذاری نسبت به منبع را نشان می‌دهد و ناخواسته حالت طعنه یا بی‌اعتمادی نمی‌سازد.',
      ar: 'تحقّق من أن Konjunktiv I هنا يبيّن المسافة من المصدر ولا يخلق سخرية أو عدم ثقة بلا قصد.',
      tr: 'Konjunktiv I’in burada kaynağa mesafe gösterdiğini, yanlışlıkla ironi ya da güvensizlik yaratmadığını kontrol et.',
      ru: 'Проверь, показывает ли Konjunktiv I здесь дистанцию к источнику и не создает ли случайно иронию или недоверие.',
      ckb: 'بپشکنە Konjunktiv I لێرەدا تەنها دووری لە سەرچاوە پیشان دەدات و بەهەڵە گاڵتە یان بێباوەڕی دروست ناکات.',
      kmr: 'Kontrol bike ka Konjunktiv I li vir tenê dûrmayîna ji çavkaniyê nîşan dide û bi şaşî tinaz an bêbawerî çênake.',
      pl: 'Sprawdź, czy Konjunktiv I pokazuje tu dystans do źródła i nie tworzy przypadkiem ironii albo nieufności.',
      ro: 'Verifică dacă Konjunktiv I arată aici distanță față de sursă și nu creează accidental ironie sau neîncredere.',
      sq: 'Kontrollo nëse Konjunktiv I këtu tregon distancë ndaj burimit dhe nuk krijon pa dashje ironi ose mosbesim.'
    }
  },
  {
    slug: 'c1-konjunktiv-i-und-ii-unterscheiden',
    topic: {
      de: 'Konjunktiv I und II',
      en: 'Konjunktiv I and II',
      fa: 'تفاوت Konjunktiv I و II',
      ar: 'الفرق بين Konjunktiv I و II',
      tr: 'Konjunktiv I ve II ayrımı',
      ru: 'различие Konjunktiv I и II',
      ckb: 'جیاوازی Konjunktiv I و II',
      kmr: 'cudahîya Konjunktiv I û II',
      pl: 'różnica między Konjunktiv I i II',
      ro: 'diferența dintre Konjunktiv I și II',
      sq: 'dallimi midis Konjunktiv I dhe II'
    },
    focus: {
      de: 'Bericht, Distanz, Hypothese und Hoeflichkeit sprachlich sauber unterscheiden',
      en: 'distinguishing report, distance, hypothesis, and politeness clearly in language',
      fa: 'جدا کردن گزارش، فاصله‌گذاری، فرضیه و ادب زبانی به شکل دقیق',
      ar: 'تمييز النقل والمسافة والافتراض والتهذيب لغويًا بوضوح',
      tr: 'aktarım, mesafe, varsayım ve nezaketi dilsel olarak açık ayırmayı',
      ru: 'четко различать сообщение, дистанцию, гипотезу и вежливость языковыми средствами',
      ckb: 'جیاکردنەوەی گواستنەوە، دووری، گریمانە و ڕێزگرتن بە شێوەیەکی زمانیی ورد',
      kmr: 'cuda kirina ragihandin, dûrmayîn, farazî û nezaketê bi awayekî zimanî zelal',
      pl: 'jasne językowe rozróżnianie relacji, dystansu, hipotezy i uprzejmości',
      ro: 'distincția clară între relatare, distanță, ipoteză și politețe',
      sq: 'ndarjen e raportimit, distancës, hipotezës dhe mirësjelljes në mënyrë të qartë gjuhësore'
    },
    grammar: ['c1-konjunktiv-i-versus-konjunktiv-ii', 'Konjunktiv I versus Konjunktiv II'],
    target: ['exam-prep-unit', 'c1-formelle-einwaende-schriftlich-abfedern', 'formelle Einwaende schriftlich abfedern'],
    practice: {
      de: 'Schreibe vier kurze Saetze: indirekte Rede, hypothetische Einschraenkung, hoefliche Bitte und distanzierte Wiedergabe.',
      en: 'Write four short sentences: indirect speech, hypothetical limitation, polite request, and distanced report.',
      fa: 'چهار جمله کوتاه بنویس: نقل‌قول غیرمستقیم، محدودیت فرضی، درخواست مؤدبانه و بازگویی با فاصله‌گذاری.',
      ar: 'اكتب أربع جمل قصيرة: كلام غير مباشر، تقييد افتراضي، طلب مهذّب، ونقل بمسافة لغوية.',
      tr: 'Dört kısa cümle yaz: dolaylı anlatım, varsayımsal sınırlama, nazik rica ve mesafeli aktarım.',
      ru: 'Напиши четыре коротких предложения: косвенная речь, гипотетическое ограничение, вежливая просьба и дистанцированная передача.',
      ckb: 'چوار ڕستەی کورت بنووسە: قسەی ناڕاستەوخۆ، سنووری گریمانەیی، داوای بەڕێز و گواستنەوەی بە دووری.',
      kmr: 'Çar hevokên kurt binivîse: axaftina nerasterast, sînora farazî, daxwaza bi nezaket û vegotina bi dûrmayîn.',
      pl: 'Napisz cztery krótkie zdania: mowę zależną, ograniczenie hipotetyczne, uprzejmą prośbę i zdystansowaną relację.',
      ro: 'Scrie patru propoziții scurte: vorbire indirectă, limitare ipotetică, rugăminte politicoasă și relatare distanțată.',
      sq: 'Shkruaj katër fjali të shkurtra: ligjërim të tërthortë, kufizim hipotetik, kërkesë të sjellshme dhe raportim me distancë.'
    },
    review: {
      de: 'Pruefe, ob jede Form wirklich die richtige Funktion traegt und nicht nur komplizierter klingt.',
      en: 'Check whether each form really carries the right function and does not merely sound more complicated.',
      fa: 'بررسی کن که هر فرم واقعاً نقش درست خود را دارد، نه اینکه فقط پیچیده‌تر به نظر برسد.',
      ar: 'تحقّق مما إذا كانت كل صيغة تؤدي وظيفتها الصحيحة ولا تبدو فقط أكثر تعقيدًا.',
      tr: 'Her biçimin gerçekten doğru işlevi taşıdığını, yalnızca daha karmaşık görünmediğini kontrol et.',
      ru: 'Проверь, действительно ли каждая форма выполняет нужную функцию, а не просто звучит сложнее.',
      ckb: 'بپشکنە هەر فۆرمێک بەڕاستی ئەرکی دروستی هەیە، نەک تەنها ئاڵۆزتر دەنێت.',
      kmr: 'Kontrol bike ka her form rastî erka rast digire, ne tenê aloz xuya dike.',
      pl: 'Sprawdź, czy każda forma naprawdę pełni właściwą funkcję, a nie tylko brzmi trudniej.',
      ro: 'Verifică dacă fiecare formă are funcția corectă și nu doar sună mai complicat.',
      sq: 'Kontrollo nëse çdo formë mban vërtet funksionin e duhur dhe nuk tingëllon thjesht më e ndërlikuar.'
    }
  },
  {
    slug: 'c1-textueberarbeitung-auf-logik-und-stil',
    topic: {
      de: 'Textueberarbeitung auf Logik und Stil',
      en: 'text revision for logic and style',
      fa: 'بازبینی متن از نظر منطق و سبک',
      ar: 'مراجعة النص من حيث المنطق والأسلوب',
      tr: 'mantık ve üslup açısından metin düzeltme',
      ru: 'редактирование текста на логику и стиль',
      ckb: 'پێداچوونەوەی دەق لە ڕووی لۆجیک و ستایل',
      kmr: 'sererastkirina nivîsê ji aliyê mantiq û şêwazê',
      pl: 'korekta tekstu pod kątem logiki i stylu',
      ro: 'revizuirea textului pentru logică și stil',
      sq: 'rishikimi i tekstit për logjikë dhe stil'
    },
    focus: {
      de: 'erst Argumentationslogik, dann Satzstil und erst zuletzt Oberflaechenfehler korrigieren',
      en: 'correcting argument logic first, then sentence style, and only finally surface errors',
      fa: 'اول اصلاح منطق استدلال، بعد سبک جمله‌ها و در آخر خطاهای سطحی',
      ar: 'تصحيح منطق الحجة أولًا، ثم أسلوب الجمل، وفي النهاية الأخطاء السطحية',
      tr: 'önce argüman mantığını, sonra cümle üslubunu, en son yüzeysel hataları düzeltmeyi',
      ru: 'сначала исправлять логику аргументации, затем стиль предложений и только потом поверхностные ошибки',
      ckb: 'سەرەتا چاککردنی لۆجیکی ئارگیومێنت، پاشان ستایلی ڕستە و لە کۆتاییدا هەڵەی سەرپێیی',
      kmr: 'pêşî rastkirina mantiqa argumanê, paşê şêwaza hevokan û dawî şaşiyên rûxarî',
      pl: 'najpierw poprawianie logiki argumentu, potem stylu zdań, a dopiero na końcu błędów powierzchniowych',
      ro: 'corectarea mai întâi a logicii argumentării, apoi a stilului frazei și abia la final a erorilor de suprafață',
      sq: 'së pari korrigjimin e logjikës së argumentit, pastaj stilin e fjalive dhe në fund gabimet sipërfaqësore'
    },
    grammar: ['c1-c1-writing-exam-grammar', 'C1-Schreibgrammatik'],
    target: ['exam-prep-unit', 'c1-ueberarbeitung-auf-logik-und-stil', 'Ueberarbeitung auf Logik und Stil'],
    practice: {
      de: 'Nimm einen Absatz und mache drei Durchgaenge: Logik, Stil, Korrektheit. Notiere pro Durchgang nur zwei Eingriffe.',
      en: 'Take one paragraph and revise it in three passes: logic, style, correctness. Note only two edits per pass.',
      fa: 'یک پاراگراف را بردار و در سه مرحله بازبینی کن: منطق، سبک، درستی زبانی. در هر مرحله فقط دو اصلاح یادداشت کن.',
      ar: 'خذ فقرة وراجعها في ثلاث جولات: المنطق، الأسلوب، الصحة اللغوية. سجّل تعديلين فقط في كل جولة.',
      tr: 'Bir paragraf al ve üç turda düzelt: mantık, üslup, doğruluk. Her turda yalnızca iki müdahale not et.',
      ru: 'Возьми абзац и отредактируй его в три прохода: логика, стиль, правильность. В каждом проходе отметь только две правки.',
      ckb: 'پەرەگرافێک وەربگرە و بە سێ جار پێداچوونەوەی بکە: لۆجیک، ستایل، دروستی. لە هەر جاردا تەنها دوو دەستکاری بنووسە.',
      kmr: 'Paragrafekê hilde û di sê caran de sererast bike: mantiq, şêwaz, rastî. Di her carê de tenê du destwerdan binivîse.',
      pl: 'Weź akapit i popraw go w trzech przejściach: logika, styl, poprawność. W każdym zapisz tylko dwie zmiany.',
      ro: 'Ia un paragraf și revizuiește-l în trei treceri: logică, stil, corectitudine. Notează doar două intervenții pe fiecare trecere.',
      sq: 'Merr një paragraf dhe rishikoje në tri kalime: logjikë, stil, saktësi. Shëno vetëm dy ndërhyrje për çdo kalim.'
    },
    review: {
      de: 'Pruefe, ob deine Ueberarbeitung den Text wirklich verbessert oder nur einzelne Woerter austauscht.',
      en: 'Check whether your revision truly improves the text or only replaces individual words.',
      fa: 'بررسی کن که بازبینی تو واقعاً متن را بهتر کرده، نه اینکه فقط چند واژه را عوض کرده باشد.',
      ar: 'تحقّق مما إذا كانت مراجعتك تحسّن النص فعلًا أم أنها تستبدل كلمات منفردة فقط.',
      tr: 'Düzeltmenin metni gerçekten iyileştirip iyileştirmediğini, yoksa yalnızca tek tek kelimeleri değiştirip değiştirmediğini kontrol et.',
      ru: 'Проверь, действительно ли правка улучшила текст, а не просто заменила отдельные слова.',
      ckb: 'بپشکنە پێداچوونەوەکەت بەڕاستی دەقەکە باشتر کردووە یان تەنها چەند وشەی گۆڕیوە.',
      kmr: 'Kontrol bike ka sererastkirina te nivîsê rastî baştir kiriye, an tenê hin peyvan guherandiye.',
      pl: 'Sprawdź, czy poprawka naprawdę ulepsza tekst, czy tylko zamienia pojedyncze słowa.',
      ro: 'Verifică dacă revizuirea chiar îmbunătățește textul sau doar înlocuiește cuvinte izolate.',
      sq: 'Kontrollo nëse rishikimi e përmirëson vërtet tekstin apo vetëm zëvendëson disa fjalë.'
    }
  },
  {
    slug: 'c1-brief-stellungnahme-und-gutachtennahe-texte',
    topic: {
      de: 'Brief, Stellungnahme und gutachtennahe Texte',
      en: 'letter, position statement, and report-like texts',
      fa: 'نامه، Stellungnahme و متن‌های نزدیک به گزارش کارشناسی',
      ar: 'رسالة وبيان موقف ونصوص قريبة من التقرير المهني',
      tr: 'mektup, görüş yazısı ve rapora yakın metinler',
      ru: 'письмо, позиционное высказывание и тексты, близкие к экспертному заключению',
      ckb: 'نامە، هەڵوێستنامە و دەقی نزیک بە ڕاپۆرتی شارەزایی',
      kmr: 'name, daxuyaniya helwestê û nivîsên nêzîkî rapora pisporî',
      pl: 'list, stanowisko i teksty podobne do opinii eksperckiej',
      ro: 'scrisoare, luare de poziție și texte apropiate de un raport de expertiză',
      sq: 'letër, qëndrim me shkrim dhe tekste pranë raportit ekspert'
    },
    focus: {
      de: 'Textsorte, Adressat und Zweck konsequent zusammenhalten',
      en: 'keeping text type, audience, and purpose consistent',
      fa: 'هماهنگ نگه داشتن نوع متن، مخاطب و هدف از ابتدا تا پایان',
      ar: 'الحفاظ على اتساق نوع النص والمخاطب والهدف من البداية إلى النهاية',
      tr: 'metin türünü, muhatabı ve amacı baştan sona tutarlı tutmayı',
      ru: 'сохранять согласованность жанра, адресата и цели от начала до конца',
      ckb: 'یەکگرتووی جۆری دەق، وەرگر و ئامانج لە سەرەتاوە تا کۆتایی',
      kmr: 'lihevhatina cureya nivîsê, muxatab û armancê ji destpêkê heta dawiyê',
      pl: 'utrzymanie spójności typu tekstu, adresata i celu od początku do końca',
      ro: 'menținerea coerentă a tipului de text, destinatarului și scopului',
      sq: 'mbajtjen të qëndrueshme të llojit të tekstit, adresuesit dhe qëllimit'
    },
    grammar: ['c1-c1-writing-exam-grammar', 'C1-Schreibgrammatik'],
    target: ['writing-template', 'c1-formelle-einwendung-begruenden', 'formelle Einwendung'],
    practice: {
      de: 'Waehle eine Textsorte und schreibe eine passende Eroeffnung, einen Hauptabschnitt und einen Abschluss mit klarer Funktion.',
      en: 'Choose one text type and write a matching opening, main section, and closing, each with a clear function.',
      fa: 'یک نوع متن انتخاب کن و برای آن شروع، بخش اصلی و پایان مناسب بنویس؛ هر بخش باید نقش روشنی داشته باشد.',
      ar: 'اختر نوع نص واكتب افتتاحية وجزءًا رئيسيًا وخاتمة مناسبة، ولكل جزء وظيفة واضحة.',
      tr: 'Bir metin türü seç ve ona uygun giriş, ana bölüm ve kapanış yaz; her bölümün açık işlevi olsun.',
      ru: 'Выбери тип текста и напиши подходящее начало, основную часть и завершение с ясной функцией.',
      ckb: 'جۆرێکی دەق هەڵبژێرە و دەستپێک، بەشی سەرەکی و کۆتایی گونجاو بنووسە؛ هەر بەشێک ئەرکی ڕوونی هەبێت.',
      kmr: 'Cureyekî nivîsê hilbijêre û destpêk, beşa sereke û dawiyeke guncaw binivîse; her beş erkeke zelal hebe.',
      pl: 'Wybierz typ tekstu i napisz pasujący wstęp, część główną oraz zakończenie z jasną funkcją.',
      ro: 'Alege un tip de text și scrie o deschidere, o parte principală și o încheiere potrivită, fiecare cu funcție clară.',
      sq: 'Zgjidh një lloj teksti dhe shkruaj hyrje, pjesë kryesore dhe mbyllje të përshtatshme, secila me funksion të qartë.'
    },
    review: {
      de: 'Pruefe, ob Ton, Aufbau und Forderung zur Textsorte passen und nicht zwischen Brief und Essay wechseln.',
      en: 'Check whether tone, structure, and request fit the text type and do not switch between letter and essay.',
      fa: 'بررسی کن که لحن، ساختار و درخواست با نوع متن هماهنگ است و متن بین نامه و essay جابه‌جا نمی‌شود.',
      ar: 'تحقّق من أن النبرة والبنية والطلب تناسب نوع النص ولا تنتقل بين الرسالة والمقال.',
      tr: 'Ton, yapı ve talebin metin türüne uyduğunu ve mektup ile essay arasında gidip gelmediğini kontrol et.',
      ru: 'Проверь, соответствуют ли тон, структура и просьба типу текста и не смешивают ли письмо с эссе.',
      ckb: 'بپشکنە لحن، پێکهاتە و داواکاری لەگەڵ جۆری دەق دەگونجێن و دەقەکە لە نێوان نامە و ئێسەیدا ناگۆڕێت.',
      kmr: 'Kontrol bike ka ton, avahî û daxwaz bi cureya nivîsê re li hev tên û di navbera name û essayê de naguhere.',
      pl: 'Sprawdź, czy ton, struktura i żądanie pasują do typu tekstu i nie przełączają się między listem a esejem.',
      ro: 'Verifică dacă tonul, structura și cererea se potrivesc tipului de text și nu oscilează între scrisoare și eseu.',
      sq: 'Kontrollo nëse toni, struktura dhe kërkesa i përshtaten llojit të tekstit dhe nuk kalojnë mes letrës dhe esesë.'
    }
  },
  {
    slug: 'c1-akademisches-und-formelles-schreiben-wiederholen',
    topic: {
      de: 'akademisches und formelles Schreiben',
      en: 'academic and formal writing',
      fa: 'نوشتار دانشگاهی و رسمی',
      ar: 'الكتابة الأكاديمية والرسمية',
      tr: 'akademik ve resmî yazma',
      ru: 'академическое и официальное письмо',
      ckb: 'نووسینی ئەکادیمی و فەرمی',
      kmr: 'nivîsîna akademîk û fermî',
      pl: 'pisanie akademickie i formalne',
      ro: 'scriere academică și formală',
      sq: 'shkrimi akademik dhe formal'
    },
    focus: {
      de: 'Zusammenfassung, Stil, Grammatik und Ueberarbeitung zu einem stabilen Schreibprozess verbinden',
      en: 'linking summary, style, grammar, and revision into a stable writing process',
      fa: 'وصل کردن خلاصه‌نویسی، سبک، گرامر و بازبینی به یک روند نوشتن پایدار',
      ar: 'ربط التلخيص والأسلوب والقواعد والمراجعة في عملية كتابة مستقرة',
      tr: 'özet, üslup, dilbilgisi ve düzeltmeyi istikrarlı bir yazma sürecinde birleştirmeyi',
      ru: 'связать резюме, стиль, грамматику и редактирование в устойчивый процесс письма',
      ckb: 'بەستنی پوختەکردنەوە، ستایل، گرامەر و پێداچوونەوە بە پرۆسەیەکی جێگیری نووسین',
      kmr: 'girêdana kurtkirin, şêwaz, rêziman û sererastkirinê di pêvajoyeke nivîsînê ya stabîl de',
      pl: 'połączenie streszczenia, stylu, gramatyki i korekty w stabilny proces pisania',
      ro: 'legarea rezumatului, stilului, gramaticii și revizuirii într-un proces stabil de scriere',
      sq: 'lidhjen e përmbledhjes, stilit, gramatikës dhe rishikimit në një proces të qëndrueshëm shkrimi'
    },
    grammar: ['c1-c1-writing-exam-grammar', 'C1-Schreibgrammatik'],
    target: ['writing-template', 'c1-abschlussstellungnahme-mit-ausblick', 'Abschlussstellungnahme mit Ausblick'],
    practice: {
      de: 'Schreibe einen kurzen Abschlussabschnitt mit Rueckbezug auf These, Begrenzung und naechsten sinnvollen Schritt.',
      en: 'Write a short closing paragraph that refers back to the thesis, limitation, and next reasonable step.',
      fa: 'یک پاراگراف پایانی کوتاه بنویس که دوباره به تز، محدودیت و قدم منطقی بعدی اشاره کند.',
      ar: 'اكتب فقرة ختامية قصيرة تعود إلى الأطروحة والتقييد والخطوة المنطقية التالية.',
      tr: 'Teze, sınırlamaya ve bir sonraki makul adıma geri bağlanan kısa bir kapanış paragrafı yaz.',
      ru: 'Напиши короткий заключительный абзац, который возвращается к тезису, ограничению и следующему разумному шагу.',
      ckb: 'پاراگرافێکی کۆتایی کورت بنووسە کە جارێکی تر ئاماژە بە تێز، سنوور و هەنگاوی گونجاوی دواتر بکات.',
      kmr: 'Paragrafeke dawî ya kurt binivîse ku dîsa bi tez, sînor û gava maqûl a din ve girêdayî be.',
      pl: 'Napisz krótki akapit końcowy, który wraca do tezy, ograniczenia i kolejnego sensownego kroku.',
      ro: 'Scrie un scurt paragraf final care revine la teză, limitare și următorul pas rezonabil.',
      sq: 'Shkruaj një paragraf të shkurtër përmbyllës që lidhet përsëri me tezën, kufizimin dhe hapin tjetër të arsyeshëm.'
    },
    review: {
      de: 'Lies den Text in einem Zug und pruefe, ob er als C1-Text klar, formal und lesbar bleibt.',
      en: 'Read the text in one go and check whether it remains clear, formal, and readable as a C1 text.',
      fa: 'متن را یک‌بار کامل بخوان و بررسی کن که به عنوان متن C1 روشن، رسمی و خوانا مانده است.',
      ar: 'اقرأ النص مرة واحدة كاملة وتحقق مما إذا كان لا يزال واضحًا ورسميًا ومقروءًا كنص C1.',
      tr: 'Metni baştan sona oku ve C1 metni olarak açık, resmî ve okunabilir kalıp kalmadığını kontrol et.',
      ru: 'Прочитай текст целиком и проверь, остается ли он ясным, официальным и читаемым как текст уровня C1.',
      ckb: 'دەقەکە یەکجار بە تەواوی بخوێنەوە و بپشکنە وەک دەقی C1 ڕوون، فەرمی و خوێندراوە ماوە.',
      kmr: 'Nivîsê yekcar bi tevahî bixwîne û kontrol bike ka wek nivîsa C1 zelal, fermî û xwendbar maye.',
      pl: 'Przeczytaj tekst jednym ciągiem i sprawdź, czy jako tekst C1 pozostaje jasny, formalny i czytelny.',
      ro: 'Citește textul dintr-o dată și verifică dacă rămâne clar, formal și lizibil ca text de C1.',
      sq: 'Lexoje tekstin njëherësh dhe kontrollo nëse si tekst C1 mbetet i qartë, formal dhe i lexueshëm.'
    }
  }
];

function orientInstruction(item) {
  return {
    de: `Lies den Lektionstext zu ${item.topic.de} und notiere, welche Schreibentscheidung fuer Form, Adressat und Zweck zuerst geklaert werden muss.`,
    en: `Read the lesson text on ${item.topic.en} and note which writing decision about form, audience, and purpose must be clarified first.`,
    fa: `متن درس درباره ${item.topic.fa} را بخوان و یادداشت کن کدام تصمیم نوشتاری درباره فرم، مخاطب و هدف باید اول روشن شود.`,
    ar: `اقرأ نص الدرس عن ${item.topic.ar} وسجّل أي قرار كتابي بشأن الشكل والمخاطب والهدف يجب توضيحه أولًا.`,
    tr: `${item.topic.tr} konulu ders metnini oku ve biçim, muhatap ve amaçla ilgili hangi yazma kararının önce netleşmesi gerektiğini not et.`,
    ru: `Прочитай текст урока о теме «${item.topic.ru}» и отметь, какое письменное решение о форме, адресате и цели нужно прояснить первым.`,
    ckb: `دەقی وانەکە لەسەر ${item.topic.ckb} بخوێنەوە و بنووسە کام بڕیاری نووسین لەسەر فۆرم، وەرگر و ئامانج دەبێت سەرەتا ڕوون بکرێتەوە.`,
    kmr: `Nivîsa dersê li ser ${item.topic.kmr} bixwîne û binivîse kîjan biryara nivîsînê li ser form, muxatab û armancê divê pêşî zelal bibe.`,
    pl: `Przeczytaj tekst lekcji o temacie: ${item.topic.pl} i zanotuj, którą decyzję dotyczącą formy, odbiorcy i celu trzeba wyjaśnić najpierw.`,
    ro: `Citește textul lecției despre ${item.topic.ro} și notează ce decizie de scriere despre formă, destinatar și scop trebuie clarificată mai întâi.`,
    sq: `Lexo tekstin e mësimit për ${item.topic.sq} dhe shëno cili vendim shkrimi për formën, adresuesin dhe qëllimin duhet qartësuar i pari.`
  };
}

function modelInstruction(item) {
  return {
    de: `Oeffne ${item.grammar[1]} und sammle drei Formulierungen, mit denen du ${item.focus.de} sicherer umsetzt.`,
    en: `Open ${item.grammar[1]} and collect three formulations that help you apply ${item.focus.en} more reliably.`,
    fa: `بخش ${item.grammar[1]} را باز کن و سه عبارت پیدا کن که کمک کند ${item.focus.fa} را مطمئن‌تر اجرا کنی.`,
    ar: `افتح قسم ${item.grammar[1]} واجمع ثلاث صيغ تساعدك على تطبيق ${item.focus.ar} بثبات أكبر.`,
    tr: `${item.grammar[1]} bölümünü aç ve ${item.focus.tr} daha güvenli uygulamana yardım eden üç ifade topla.`,
    ru: `Открой раздел ${item.grammar[1]} и собери три формулировки, которые помогут надежнее применять: ${item.focus.ru}.`,
    ckb: `بەشی ${item.grammar[1]} بکەرەوە و سێ دەربڕین کۆبکەوە کە یارمەتیت بدات ${item.focus.ckb} بە دڵنیایی زیاتر جێبەجێ بکەیت.`,
    kmr: `Beşa ${item.grammar[1]} veke û sê derbirînan kom bike ku alîkar bin ${item.focus.kmr} bi ewlehiyeke zêdetir pêk bînî.`,
    pl: `Otwórz sekcję ${item.grammar[1]} i zbierz trzy sformułowania, które pomogą ci pewniej stosować: ${item.focus.pl}.`,
    ro: `Deschide secțiunea ${item.grammar[1]} și adună trei formulări care te ajută să aplici mai sigur: ${item.focus.ro}.`,
    sq: `Hap seksionin ${item.grammar[1]} dhe mblidh tri formulime që të ndihmojnë të zbatosh më sigurt: ${item.focus.sq}.`
  };
}

function materialInstruction(item) {
  return {
    de: `Bearbeite das verlinkte Material zu ${item.target[2]} und achte darauf, wie du ${item.focus.de} in einer realistischen C1-Schreibaufgabe einsetzt.`,
    en: `Work through the linked material on ${item.target[2]} and notice how to use ${item.focus.en} in a realistic C1 writing task.`,
    fa: `محتوای لینک‌شده درباره ${item.target[2]} را انجام بده و دقت کن چطور می‌توان ${item.focus.fa} را در یک تمرین واقعی نوشتن C1 به کار برد.`,
    ar: `اعمل على المادة المرتبطة حول ${item.target[2]} وانتبه إلى كيفية استخدام ${item.focus.ar} في مهمة كتابة واقعية على مستوى C1.`,
    tr: `${item.target[2]} hakkındaki bağlantılı malzemeyi çalış ve ${item.focus.tr} gerçekçi bir C1 yazma görevinde nasıl kullanabileceğine dikkat et.`,
    ru: `Проработай связанный материал по теме ${item.target[2]} и обрати внимание, как использовать ${item.focus.ru} в реалистичном письменном задании C1.`,
    ckb: `ماتریاڵی بەستەرکراو لەسەر ${item.target[2]} کاربکە و سەرنج بدە چۆن ${item.focus.ckb} لە ئەرکێکی ڕاستەقینەی نووسینی C1دا بەکاربهێنیت.`,
    kmr: `Li ser materyala girêdayî ya ${item.target[2]} bixebite û bala xwe bide ka çawa ${item.focus.kmr} di erkeke nivîsînê ya rastîn a C1 de bi kar tînî.`,
    pl: `Przerób połączony materiał o ${item.target[2]} i zwróć uwagę, jak zastosować ${item.focus.pl} w realistycznym zadaniu pisania C1.`,
    ro: `Lucrează cu materialul legat despre ${item.target[2]} și observă cum folosești ${item.focus.ro} într-o sarcină realistă de scriere C1.`,
    sq: `Puno me materialin e lidhur për ${item.target[2]} dhe vëzhgo si përdoret ${item.focus.sq} në një detyrë reale shkrimi C1.`
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
    block('read', 'orient', orientInstruction(item), 'none', null, 7, 10, true),
    block('grammar', 'model', modelInstruction(item), 'grammar-topic', item.grammar[0], 8, 20, true),
    block(item.target[0] === 'writing-template' ? 'write' : 'exam-prep', 'material', materialInstruction(item), item.target[0], item.target[1], 9, 30, true),
    block('write', 'write', item.practice, 'none', null, 9, 40, true),
    block('review', 'review', item.review, 'none', null, 7, 50, true)
  ];
}

fs.writeFileSync(file, JSON.stringify(data, null, 2) + '\n', 'utf8');
console.log(`Updated ${items.length} C1 Module 4 lessons with ${items.length * 5} activity blocks.`);

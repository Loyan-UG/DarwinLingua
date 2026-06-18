const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c1-souveraen-in-studium-beruf-und-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'Aufgabe und Erwartung klaeren',
    en: 'Clarify task and expectation',
    fa: 'نوع کار و انتظار را روشن کن',
    ar: 'وضّح نوع المهمة والتوقع',
    tr: 'Görevi ve beklentiyi netleştir',
    ru: 'Проясни задание и ожидание',
    ckb: 'ئەرک و چاوەڕوانی ڕوون بکەوە',
    kmr: 'Erk û hêviyê zelal bike',
    pl: 'Wyjaśnij zadanie i oczekiwanie',
    ro: 'Clarifică sarcina și așteptarea',
    sq: 'Qartëso detyrën dhe pritshmërinë'
  },
  strategy: {
    de: 'Pruefungsstrategie sichern',
    en: 'Secure the exam strategy',
    fa: 'راهبرد آزمون را محکم کن',
    ar: 'ثبّت استراتيجية الامتحان',
    tr: 'Sınav stratejisini sağlamlaştır',
    ru: 'Закрепи экзаменационную стратегию',
    ckb: 'ستراتیژی تاقیکردنەوەکە بەهێز بکە',
    kmr: 'Stratejiya ezmûnê xurt bike',
    pl: 'Utrwal strategię egzaminacyjną',
    ro: 'Consolidează strategia de examen',
    sq: 'Forco strategjinë e provimit'
  },
  language: {
    de: 'Sprachmittel gezielt waehlen',
    en: 'Choose language resources deliberately',
    fa: 'ابزارهای زبانی را هدفمند انتخاب کن',
    ar: 'اختر الوسائل اللغوية بوعي',
    tr: 'Dil araçlarını bilinçli seç',
    ru: 'Выбирай языковые средства целенаправленно',
    ckb: 'ئامرازە زمانییەکان بە ئامانج هەڵبژێرە',
    kmr: 'Amûrên zimanî bi armanc hilbijêre',
    pl: 'Dobierz środki językowe celowo',
    ro: 'Alege mijloacele lingvistice intenționat',
    sq: 'Zgjidh mjetet gjuhësore me qëllim'
  },
  apply: {
    de: 'Mini-Leistung trainieren',
    en: 'Practise a mini-performance',
    fa: 'یک اجرای کوتاه تمرین کن',
    ar: 'تدرّب على أداء قصير',
    tr: 'Kısa bir performans çalış',
    ru: 'Отработай короткое выполнение',
    ckb: 'ئەنجامدانێکی کورت ڕاهێنان بکە',
    kmr: 'Performanseke kurt perwerde bike',
    pl: 'Przećwicz krótkie wykonanie',
    ro: 'Exersează o prestație scurtă',
    sq: 'Ushtro një performancë të shkurtër'
  },
  review: {
    de: 'Leistung gezielt verbessern',
    en: 'Improve the performance deliberately',
    fa: 'اجرای خودت را هدفمند بهتر کن',
    ar: 'حسّن أداءك بوعي',
    tr: 'Performansını bilinçli geliştir',
    ru: 'Целенаправленно улучши выполнение',
    ckb: 'ئەنجامدانەکەت بە ئامانج باشتر بکە',
    kmr: 'Performansa xwe bi armanc baştir bike',
    pl: 'Celowo popraw wykonanie',
    ro: 'Îmbunătățește prestația intenționat',
    sq: 'Përmirëso performancën me qëllim'
  }
};

const items = [
  {
    slug: 'c1-goethe-c1-lesen-und-sprachbausteine',
    topic: { de: 'Lesen und Sprachbausteine', en: 'reading and language-building tasks', fa: 'خواندن و بخش‌های ساختار زبان', ar: 'القراءة ومهام البنية اللغوية', tr: 'okuma ve dil yapı taşları', ru: 'чтение и языковые блоки', ckb: 'خوێندنەوە و ئەرکە زمانییەکان', kmr: 'xwendin û erkên avakirina ziman', pl: 'czytanie i zadania językowe', ro: 'citire și sarcini de construcție lingvistică', sq: 'lexim dhe detyra të strukturës gjuhësore' },
    focus: { de: 'Textfunktion, Signalwoerter und Lueckenlogik getrennt pruefen, statt nur nach Bauchgefuehl zu raten', en: 'checking text function, signal words, and gap logic separately instead of guessing by instinct', fa: 'کارکرد متن، واژه‌های راهنما و منطق جای خالی را جدا بررسی کنی، نه اینکه فقط حدسی جواب بدهی', ar: 'فحص وظيفة النص وكلمات الإشارة ومنطق الفراغات كلٌ على حدة بدل التخمين فقط', tr: 'metin işlevini, işaret sözcüklerini ve boşluk mantığını sadece sezgiyle tahmin etmek yerine ayrı ayrı kontrol etmeyi', ru: 'отдельно проверять функцию текста, сигнальные слова и логику пропусков, а не только угадывать интуитивно', ckb: 'کاری دەق، وشەی ئاماژە و لۆژیکی بۆشایی جیا جیا بپشکنیت، نەک تەنها بە هەست هەڵبژێریت', kmr: 'karê nivîsê, peyvên nîşan û mantiqa valahiyan cuda kontrol kirin, ne tenê bi hest texmîn kirin', pl: 'osobne sprawdzanie funkcji tekstu, słów sygnałowych i logiki luk zamiast zgadywania na wyczucie', ro: 'verificarea separată a funcției textului, cuvintelor-semnale și logicii spațiilor, nu ghicitul după instinct', sq: 'kontrollimin veçmas të funksionit të tekstit, fjalëve sinjalizuese dhe logjikës së boshllëqeve, jo përgjigje me hamendje' },
    grammar: ['c1-advanced-academic-connectors', 'Konnektoren und Textsignale'],
    strategy: 'c1-pruefungsanforderungen-einordnen',
    target: ['exam-prep-unit', 'c1-pruefungsanforderungen-einordnen']
  },
  {
    slug: 'c1-goethe-c1-hoeren-und-notizen',
    topic: { de: 'Hoeren und Notizen', en: 'listening and notes', fa: 'شنیدن و یادداشت‌برداری', ar: 'الاستماع وتدوين الملاحظات', tr: 'dinleme ve not alma', ru: 'аудирование и заметки', ckb: 'گوێگرتن و تێبینی نووسین', kmr: 'guhdarî û tomarên kurt', pl: 'słuchanie i notatki', ro: 'ascultare și notițe', sq: 'dëgjim dhe shënime' },
    focus: { de: 'Kernaussage, Einschraenkung und Beispiel in knappen Notizen sichern', en: 'capturing main point, limitation, and example in concise notes', fa: 'پیام اصلی، محدودیت و مثال را در یادداشت‌های کوتاه ثبت کنی', ar: 'تثبيت الفكرة الأساسية والقيد والمثال في ملاحظات مختصرة', tr: 'ana mesajı, sınırlamayı ve örneği kısa notlarla güvenceye almayı', ru: 'фиксировать основную мысль, ограничение и пример в кратких заметках', ckb: 'پەیامی سەرەکی، سنووردارکردن و نموونە لە تێبینیی کورتدا بپارێزیت', kmr: 'peyama sereke, sînorkirin û mînakê di tomarên kurt de parastin', pl: 'uchwycenie głównej myśli, ograniczenia i przykładu w krótkich notatkach', ro: 'fixarea ideii principale, limitării și exemplului în notițe scurte', sq: 'kapjen e mesazhit kryesor, kufizimit dhe shembullit në shënime të shkurtra' },
    grammar: ['c1-formal-summaries', 'knappe Zusammenfassung'],
    strategy: 'c1-pruefungsanforderungen-einordnen',
    target: ['exam-prep-unit', 'c1-pruefungsanforderungen-einordnen']
  },
  {
    slug: 'c1-goethe-c1-schreiben-und-stellungnahme',
    topic: { de: 'Schreiben und Stellungnahme', en: 'writing and position statement', fa: 'نوشتن و موضع‌گیری', ar: 'الكتابة وإبداء الموقف', tr: 'yazma ve görüş bildirme', ru: 'письмо и выражение позиции', ckb: 'نووسین و دەربڕینی هەڵوێست', kmr: 'nivîsandin û gotina helwestê', pl: 'pisanie i stanowisko', ro: 'scriere și exprimarea poziției', sq: 'shkrim dhe shprehje qëndrimi' },
    focus: { de: 'These, Gegenperspektive und Schlussfolgerung klar fuehren, ohne in Musterfloskeln zu bleiben', en: 'guiding thesis, counter-perspective, and conclusion clearly without relying on formulaic phrases', fa: 'تز، نگاه مخالف و نتیجه را روشن پیش ببری، بدون اینکه فقط به عبارت‌های قالبی تکیه کنی', ar: 'قيادة الأطروحة والمنظور المقابل والاستنتاج بوضوح من دون الاعتماد على عبارات جاهزة فقط', tr: 'tez, karşı bakış ve sonucu kalıp ifadelere yaslanmadan açık yürütmeyi', ru: 'ясно вести тезис, противоположный взгляд и вывод, не полагаясь только на шаблонные фразы', ckb: 'تێز، دیدی بەرامبەر و دەرئەنجام بە ڕوونی ببەیت، بەبێ پشتبەستن تەنها بە دەربڕینی ئامادە', kmr: 'tez, nêrîna dijber û encamê bi zelalî rêve birin bê piştgirîkirina tenê bi qalibên amade', pl: 'jasne prowadzenie tezy, perspektywy przeciwnej i wniosku bez opierania się tylko na formułkach', ro: 'conducerea clară a tezei, perspectivei opuse și concluziei fără a rămâne în formule fixe', sq: 'drejtimin e qartë të tezës, këndvështrimit kundër dhe përfundimit pa mbetur te frazat shabllon' },
    grammar: ['c1-c1-writing-exam-grammar', 'Schreibpruefungsgrammatik'],
    strategy: 'c1-schriftliche-stellungnahme-fokussieren',
    target: ['writing-template', 'c1-akademische-stellungnahme-einordnen']
  },
  {
    slug: 'c1-goethe-c1-sprechen-und-diskussion',
    topic: { de: 'Sprechen und Diskussion', en: 'speaking and discussion', fa: 'صحبت کردن و بحث', ar: 'التحدث والنقاش', tr: 'konuşma ve tartışma', ru: 'говорение и дискуссия', ckb: 'قسەکردن و مشتومڕ', kmr: 'axaftin û nîqaş', pl: 'mówienie i dyskusja', ro: 'vorbire și discuție', sq: 'të folur dhe diskutim' },
    focus: { de: 'Standpunkt, Einwand und Rueckfrage ruhig verbinden', en: 'linking position, objection, and follow-up question calmly', fa: 'موضع، ایراد طرف مقابل و پرسش بعدی را آرام و منظم به هم وصل کنی', ar: 'ربط الموقف والاعتراض والسؤال اللاحق بهدوء', tr: 'tutumu, itirazı ve takip sorusunu sakin biçimde bağlamayı', ru: 'спокойно связывать позицию, возражение и уточняющий вопрос', ckb: 'هەڵوێست، ناڕەزایی بەرامبەر و پرسیاری دواتر بە ئارامی پێکەوە ببەستیت', kmr: 'helwest, nerazîbûn û pirsa şopandinê bi aramî girêdan', pl: 'spokojne łączenie stanowiska, zastrzeżenia i pytania uzupełniającego', ro: 'legarea calmă a poziției, obiecției și întrebării de urmărire', sq: 'lidhjen qetë të qëndrimit, kundërshtimit dhe pyetjes pasuese' },
    grammar: ['c1-c1-presentation-grammar', 'Praesentations- und Diskussionssprache'],
    strategy: 'c1-muendliche-pruefungsleistung-steuern',
    target: ['roleplay', 'c1-eine-diskussion-trotz-einwaenden-strukturieren']
  },
  {
    slug: 'c1-telc-c1-hochschule-lesen-und-hoeren',
    topic: { de: 'hochschulbezogenes Lesen und Hoeren', en: 'university-oriented reading and listening', fa: 'خواندن و شنیدن در فضای دانشگاهی', ar: 'القراءة والاستماع في سياق جامعي', tr: 'üniversite odaklı okuma ve dinleme', ru: 'чтение и аудирование в университетском контексте', ckb: 'خوێندنەوە و گوێگرتنی پەیوەست بە زانکۆ', kmr: 'xwendin û guhdarîya girêdayî zanîngehê', pl: 'czytanie i słuchanie w kontekście akademickim', ro: 'citire și ascultare în context universitar', sq: 'lexim dhe dëgjim në kontekst universitar' },
    focus: { de: 'Definition, Forschungsbezug und Einschraenkung in akademischen Texten erkennen', en: 'recognizing definition, research reference, and limitation in academic texts', fa: 'تعریف، اشاره پژوهشی و محدودیت را در متن‌های دانشگاهی تشخیص بدهی', ar: 'تمييز التعريف والإحالة البحثية والقيد في النصوص الجامعية', tr: 'akademik metinlerde tanımı, araştırma bağlantısını ve sınırlamayı tanımayı', ru: 'распознавать определение, исследовательскую ссылку и ограничение в академических текстах', ckb: 'پێناسە، پەیوەندیی توێژینەوە و سنووردارکردن لە دەقە زانکۆییەکاندا بناسیتەوە', kmr: 'pênase, têkiliya lêkolînê û sînorkirinê di nivîsên akademîk de nas kirin', pl: 'rozpoznawanie definicji, odniesienia badawczego i ograniczenia w tekstach akademickich', ro: 'recunoașterea definiției, referinței de cercetare și limitării în texte academice', sq: 'dallimin e përkufizimit, lidhjes kërkimore dhe kufizimit në tekste akademike' },
    grammar: ['c1-advanced-academic-connectors', 'akademische Textsignale'],
    strategy: 'c1-pruefungsanforderungen-einordnen',
    target: ['exam-prep-unit', 'c1-pruefungsanforderungen-einordnen']
  },
  {
    slug: 'c1-telc-c1-hochschule-mediation-und-schreiben',
    topic: { de: 'Mediation und Schreiben im Hochschulkontext', en: 'mediation and writing in a university context', fa: 'میانجی‌گری زبانی و نوشتن در فضای دانشگاهی', ar: 'الوساطة اللغوية والكتابة في سياق جامعي', tr: 'üniversite bağlamında aracılık ve yazma', ru: 'медиация и письмо в университетском контексте', ckb: 'ناوبژیکاری زمانی و نووسین لە چوارچێوەی زانکۆدا', kmr: 'navbeynkariya zimanî û nivîsandin di çarçoveya zanîngehê de', pl: 'mediacja językowa i pisanie w kontekście akademickim', ro: 'mediere lingvistică și scriere în context universitar', sq: 'ndërmjetësim gjuhësor dhe shkrim në kontekst universitar' },
    focus: { de: 'Fremdinformation adressatengerecht zusammenfassen, ohne sie mechanisch zu uebersetzen', en: 'summarizing external information for the addressee without translating it mechanically', fa: 'اطلاعات متن دیگر را متناسب با مخاطب خلاصه کنی، نه اینکه آن را مکانیکی ترجمه کنی', ar: 'تلخيص معلومات من مصدر آخر بما يناسب المخاطب من دون ترجمتها آليًا', tr: 'başka kaynaktaki bilgiyi muhataba uygun özetlemeyi, mekanik çevirmemeyi', ru: 'резюмировать внешнюю информацию под адресата, не переводя ее механически', ckb: 'زانیاریی سەرچاوەی تر بەپێی وەرگر کورت بکەیتەوە، نەک بە شێوەی میکانیکی وەربگێڕیت', kmr: 'agahiya çavkaniyek din li gorî muxateb kurt kirin, ne wergera mekanîkî', pl: 'streszczenie informacji z innego źródła pod adresata, bez mechanicznego tłumaczenia', ro: 'rezumarea informației externe pentru destinatar fără traducere mecanică', sq: 'përmbledhjen e informacionit nga një burim tjetër sipas adresatit, jo përkthim mekanik' },
    grammar: ['c1-formal-summaries', 'adressatengerechte Zusammenfassung'],
    strategy: 'c1-mediation-aufgabenadressaten-gerecht-schreiben',
    target: ['exam-prep-unit', 'c1-mediation-aufgabenadressaten-gerecht-schreiben']
  },
  {
    slug: 'c1-testdaf-b2-c1-stellungnahme-und-diskussion',
    topic: { de: 'Stellungnahme und Diskussion', en: 'position statement and discussion', fa: 'موضع‌گیری و بحث', ar: 'إبداء الموقف والنقاش', tr: 'görüş bildirme ve tartışma', ru: 'выражение позиции и дискуссия', ckb: 'دەربڕینی هەڵوێست و مشتومڕ', kmr: 'gotina helwestê û nîqaş', pl: 'stanowisko i dyskusja', ro: 'exprimarea poziției și discuție', sq: 'shprehje qëndrimi dhe diskutim' },
    focus: { de: 'Datenbezug, eigene Bewertung und akademische Vorsicht zusammenbringen', en: 'combining data reference, personal evaluation, and academic caution', fa: 'ارجاع به داده، ارزیابی خودت و احتیاط دانشگاهی را کنار هم بیاوری', ar: 'الجمع بين الإشارة إلى البيانات والتقييم الشخصي والحذر الأكاديمي', tr: 'veri bağlantısını, kendi değerlendirmeyi ve akademik temkini birleştirmeyi', ru: 'соединять ссылку на данные, собственную оценку и академическую осторожность', ckb: 'ئاماژە بە داتا، هەڵسەنگاندنی خۆت و وریایی زانکۆیی پێکەوە بهێنیت', kmr: 'girêdana daneyan, nirxandina xwe û hişyariya akademîk bi hev re anîn', pl: 'łączenie odniesienia do danych, własnej oceny i akademickiej ostrożności', ro: 'combinarea referinței la date, evaluării proprii și prudenței academice', sq: 'bashkimin e referimit te të dhënat, vlerësimit personal dhe kujdesit akademik' },
    grammar: ['c1-academic-argument-grammar', 'akademische Argumentation'],
    strategy: 'c1-datenbezug-in-stellungnahme-einbauen',
    target: ['exam-prep-unit', 'c1-datenbezug-in-stellungnahme-einbauen']
  },
  {
    slug: 'c1-berufssprache-c1-strategiegespraech',
    topic: { de: 'ein Strategiegespraech im Beruf', en: 'a strategic conversation at work', fa: 'گفت‌وگوی راهبردی در محیط کار', ar: 'حوار استراتيجي في العمل', tr: 'işte strateji görüşmesi', ru: 'стратегический разговор на работе', ckb: 'گفتوگۆی ستراتیژی لە کاردا', kmr: 'axaftineke stratejîk di kar de', pl: 'rozmowa strategiczna w pracy', ro: 'o discuție strategică la serviciu', sq: 'bisedë strategjike në punë' },
    focus: { de: 'Ziel, Risiko, Prioritaet und naechsten Schritt klar verhandeln', en: 'negotiating goal, risk, priority, and next step clearly', fa: 'هدف، ریسک، اولویت و قدم بعدی را روشن مذاکره کنی', ar: 'التفاوض بوضوح حول الهدف والخطر والأولوية والخطوة التالية', tr: 'hedef, risk, öncelik ve sonraki adımı net müzakere etmeyi', ru: 'ясно обсуждать цель, риск, приоритет и следующий шаг', ckb: 'ئامانج، مەترسی، پێشەکی و هەنگاوی داهاتوو بە ڕوونی وتووێژ بکەیت', kmr: 'armanc, rîsk, pêşî û gava din bi zelalî gotûbêj kirin', pl: 'jasne negocjowanie celu, ryzyka, priorytetu i następnego kroku', ro: 'negocierea clară a scopului, riscului, priorității și pasului următor', sq: 'negocimin qartë të qëllimit, rrezikut, përparësisë dhe hapit tjetër' },
    grammar: ['c1-register-shifting', 'beruflicher Registerwechsel'],
    strategy: 'c1-standpunkt-muendlich-sachlich-vertreten',
    target: ['roleplay', 'c1-berufssprache-c1-strategiegespraech-fuehren']
  },
  {
    slug: 'c1-pruefungsleistung-analysieren-und-verbessern',
    topic: { de: 'eine Pruefungsleistung analysieren und verbessern', en: 'analyzing and improving exam performance', fa: 'تحلیل و بهتر کردن عملکرد آزمونی', ar: 'تحليل أداء الامتحان وتحسينه', tr: 'sınav performansını analiz edip geliştirme', ru: 'анализ и улучшение экзаменационного выполнения', ckb: 'شیکردنەوە و باشترکردنی ئەنجامی تاقیکردنەوە', kmr: 'analîz û baştirkirina performansa ezmûnê', pl: 'analiza i poprawa wykonania egzaminacyjnego', ro: 'analizarea și îmbunătățirea performanței la examen', sq: 'analizimi dhe përmirësimi i performancës në provim' },
    focus: { de: 'Fehlerursache, Strategieproblem und sprachliches Problem getrennt erkennen', en: 'distinguishing error cause, strategy problem, and language problem separately', fa: 'علت خطا، مشکل راهبردی و مشکل زبانی را جدا تشخیص بدهی', ar: 'تمييز سبب الخطأ ومشكلة الاستراتيجية والمشكلة اللغوية كلٌ على حدة', tr: 'hata nedenini, strateji sorununu ve dil sorununu ayrı tanımayı', ru: 'отдельно распознавать причину ошибки, стратегическую проблему и языковую проблему', ckb: 'هۆکاری هەڵە، کێشەی ستراتیژی و کێشەی زمانی جیا جیا بناسیتەوە', kmr: 'sedema şaşitiyê, pirsgirêka stratejiyê û pirsgirêka zimanî cuda nas kirin', pl: 'oddzielne rozpoznanie przyczyny błędu, problemu strategii i problemu językowego', ro: 'recunoașterea separată a cauzei greșelii, problemei de strategie și problemei lingvistice', sq: 'dallimin veçmas të shkakut të gabimit, problemit strategjik dhe problemit gjuhësor' },
    grammar: ['c1-c1-common-mistakes', 'typische C1-Fehler'],
    strategy: 'c1-pruefungsanforderungen-einordnen',
    target: ['exam-prep-unit', 'c1-pruefungsanforderungen-einordnen']
  },
  {
    slug: 'c1-pruefung-goethe-telc-testdaf-und-hochschule-wiederholen',
    topic: { de: 'Pruefungsformate und Hochschulaufgaben wiederholen', en: 'reviewing exam formats and university tasks', fa: 'مرور قالب‌های آزمون و کارهای دانشگاهی', ar: 'مراجعة صيغ الامتحان والمهام الجامعية', tr: 'sınav formatları ve üniversite görevlerini tekrar etme', ru: 'повторение форматов экзамена и университетских заданий', ckb: 'دووبارەکردنەوەی فۆرماتی تاقیکردنەوە و ئەرکە زانکۆییەکان', kmr: 'dubarekirina formatên ezmûnê û erkên zanîngehê', pl: 'powtórka formatów egzaminu i zadań akademickich', ro: 'recapitularea formatelor de examen și sarcinilor universitare', sq: 'përsëritja e formateve të provimit dhe detyrave universitare' },
    focus: { de: 'Format, Zeitdruck, Bewertungsziel und eigene Routine zu einem stabilen Plan verbinden', en: 'linking format, time pressure, assessment goal, and personal routine into a stable plan', fa: 'قالب، فشار زمان، هدف ارزیابی و روال شخصی خودت را به یک برنامه پایدار وصل کنی', ar: 'ربط الصيغة وضغط الوقت وهدف التقييم وروتينك الشخصي في خطة مستقرة', tr: 'formatı, zaman baskısını, değerlendirme hedefini ve kişisel rutini sağlam bir plana bağlamayı', ru: 'связать формат, нехватку времени, цель оценки и личную рутину в устойчивый план', ckb: 'فۆرمات، گوشاری کات، ئامانجی هەڵسەنگاندن و ڕۆتینی خۆت لە پلانێکی جێگیردا پێکەوە ببەستیت', kmr: 'format, zexta demê, armanca nirxandinê û rutina xwe di planeke aram de girêdan', pl: 'połączenie formatu, presji czasu, celu oceny i własnej rutyny w stabilny plan', ro: 'legarea formatului, presiunii timpului, scopului evaluării și rutinei personale într-un plan stabil', sq: 'lidhjen e formatit, presionit të kohës, qëllimit të vlerësimit dhe rutinës personale në një plan të qëndrueshëm' },
    grammar: ['c1-c1-grammar-review-map', 'C1-Review-Map'],
    strategy: 'c1-pruefungsanforderungen-einordnen',
    target: ['exam-prep-unit', 'c1-pruefungsanforderungen-einordnen']
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
    de: `Lies den Lektionstext zu ${item.topic.de} und notiere: Was wird bewertet, welche Handlung ist verlangt, und wo kann Zeit verloren gehen?`,
    en: `Read the lesson text on ${item.topic.en} and note what is assessed, which action is required, and where time can be lost.`,
    fa: `متن درس درباره ${item.topic.fa} را بخوان و یادداشت کن: چه چیزی ارزیابی می‌شود، چه کاری لازم است و کجا ممکن است زمان از دست برود.`,
    ar: `اقرأ نص الدرس عن ${item.topic.ar} وسجّل: ما الذي يُقيّم، ما الفعل المطلوب، وأين يمكن أن يضيع الوقت.`,
    tr: `${item.topic.tr} konulu ders metnini oku ve not et: ne değerlendiriliyor, hangi eylem isteniyor, nerede zaman kaybedilebilir.`,
    ru: `Прочитай текст урока о теме «${item.topic.ru}» и отметь, что оценивается, какое действие требуется и где можно потерять время.`,
    ckb: `دەقی وانەکە لەسەر ${item.topic.ckb} بخوێنەوە و بنووسە: چی هەڵسەنگاندن دەکرێت، کام کردار داواکراوە و لە کوێ کات لەدەست دەچێت.`,
    kmr: `Nivîsa dersê li ser ${item.topic.kmr} bixwîne û binivîse: çi tê nirxandin, kîjan kiryar tê xwestin û li ku dem dikare winda bibe.`,
    pl: `Przeczytaj tekst lekcji o temacie: ${item.topic.pl} i zanotuj, co jest oceniane, jakie działanie jest wymagane i gdzie można stracić czas.`,
    ro: `Citește textul lecției despre ${item.topic.ro} și notează ce se evaluează, ce acțiune este cerută și unde se poate pierde timp.`,
    sq: `Lexo tekstin e mësimit për ${item.topic.sq} dhe shëno çfarë vlerësohet, çfarë veprimi kërkohet dhe ku mund të humbasë koha.`
  };
}

function strategyInstruction(item) {
  return {
    de: `Bearbeite die verlinkte Pruefungsstrategie und konzentriere dich darauf, ${item.focus.de}.`,
    en: `Work through the linked exam strategy and focus on this skill: ${item.focus.en}.`,
    fa: `راهبرد آزمونی لینک‌شده را انجام بده و روی این مهارت تمرکز کن: ${item.focus.fa}.`,
    ar: `اعمل على استراتيجية الامتحان المرتبطة وركّز على هذه المهارة: ${item.focus.ar}.`,
    tr: `Bağlantılı sınav stratejisini çalış ve şu beceriye odaklan: ${item.focus.tr}.`,
    ru: `Проработай связанную экзаменационную стратегию и сосредоточься на этом навыке: ${item.focus.ru}.`,
    ckb: `ستراتیژی تاقیکردنەوەی بەستەرکراو کاربکە و سەرنج بخە سەر ئەم توانایە: ${item.focus.ckb}.`,
    kmr: `Stratejiya ezmûnê ya girêdayî bixebite û bala xwe bide vê jêhatinê: ${item.focus.kmr}.`,
    pl: `Przerób połączoną strategię egzaminacyjną i skup się na tej umiejętności: ${item.focus.pl}.`,
    ro: `Lucrează cu strategia de examen legată și concentrează-te pe această abilitate: ${item.focus.ro}.`,
    sq: `Puno me strategjinë e lidhur të provimit dhe përqendrohu te kjo aftësi: ${item.focus.sq}.`
  };
}

function languageInstruction(item) {
  return {
    de: `Oeffne ${item.grammar[1]} und sammle drei Formulierungen, die deine Antwort genauer, knapper oder pruefungstauglicher machen.`,
    en: `Open the linked language section and collect three formulations that make your answer more precise, concise, or exam-ready.`,
    fa: `بخش زبانی لینک‌شده را باز کن و سه عبارت پیدا کن که پاسخ تو را دقیق‌تر، کوتاه‌تر یا مناسب‌تر برای آزمون کند.`,
    ar: `افتح القسم اللغوي المرتبط واجمع ثلاث صيغ تجعل إجابتك أدق أو أوجز أو أنسب للامتحان.`,
    tr: `Bağlantılı dil bölümünü aç ve yanıtını daha kesin, daha kısa veya sınava daha uygun yapan üç ifade topla.`,
    ru: `Открой связанный языковой раздел и собери три формулировки, которые делают ответ точнее, короче или пригоднее для экзамена.`,
    ckb: `بەشی زمانی بەستەرکراو بکەرەوە و سێ دەربڕین کۆبکەوە کە وەڵامەکەت وردتر، کورتتر یان گونجاوتر بۆ تاقیکردنەوە بکات.`,
    kmr: `Beşa zimanê ya girêdayî veke û sê derbirînan kom bike ku bersiva te rasttir, kurttir an guncantir ji bo ezmûnê bikin.`,
    pl: `Otwórz połączoną sekcję językową i zbierz trzy sformułowania, które czynią odpowiedź dokładniejszą, krótszą lub lepszą na egzamin.`,
    ro: `Deschide secțiunea lingvistică legată și adună trei formulări care îți fac răspunsul mai precis, mai scurt sau mai potrivit pentru examen.`,
    sq: `Hap seksionin e lidhur gjuhësor dhe mblidh tri formulime që e bëjnë përgjigjen më të saktë, më të shkurtër ose më të përshtatshme për provim.`
  };
}

function applyInstruction() {
  return {
    de: 'Trainiere eine kurze Teilantwort unter Zeitdruck: erst planen, dann formulieren, dann eine Schwachstelle markieren.',
    en: 'Practise a short partial answer under time pressure: plan first, then formulate, then mark one weak point.',
    fa: 'یک پاسخ کوتاه را با محدودیت زمان تمرین کن: اول برنامه‌ریزی کن، بعد بنویس یا بگو، بعد یک نقطه ضعف را مشخص کن.',
    ar: 'تدرّب على إجابة جزئية قصيرة تحت ضغط الوقت: خطّط أولًا، ثم صغ الإجابة، ثم حدد نقطة ضعف واحدة.',
    tr: 'Zaman baskısı altında kısa bir kısmi yanıt çalış: önce planla, sonra formüle et, sonra bir zayıf noktayı işaretle.',
    ru: 'Отработай короткий частичный ответ под давлением времени: сначала спланируй, затем сформулируй, затем отметь одно слабое место.',
    ckb: 'وەڵامێکی بەشی کورت لە ژێر گوشاری کاتدا ڕاهێنان بکە: سەرەتا پلان دابنێ، پاشان داڕێژە، پاشان خاڵێکی لاواز دیاری بکە.',
    kmr: 'Bersiveke beşî ya kurt di bin zexta demê de perwerde bike: pêşî plan bike, paşê formule bike, paşê xalek lawaz nîşan bike.',
    pl: 'Przećwicz krótką częściową odpowiedź pod presją czasu: najpierw zaplanuj, potem sformułuj, potem zaznacz jeden słaby punkt.',
    ro: 'Exersează un răspuns parțial scurt sub presiunea timpului: mai întâi planifică, apoi formulează, apoi marchează un punct slab.',
    sq: 'Ushtro një përgjigje të shkurtër të pjesshme nën presion kohe: së pari planifiko, pastaj formulo, pastaj shëno një pikë të dobët.'
  };
}

function reviewInstruction() {
  return {
    de: 'Verbessere deine Mini-Leistung mit drei Fragen: Ist die Aufgabe getroffen? Ist die Sprache kontrolliert? Ist die Zeit realistisch eingeteilt?',
    en: 'Improve your mini-performance with three questions: Is the task addressed? Is the language controlled? Is the time allocation realistic?',
    fa: 'اجرای کوتاهت را با سه پرسش بهتر کن: کار خواسته‌شده انجام شده؟ زبان کنترل‌شده است؟ تقسیم زمان واقع‌بینانه است؟',
    ar: 'حسّن أداءك القصير بثلاثة أسئلة: هل عالجت المهمة؟ هل اللغة مضبوطة؟ هل توزيع الوقت واقعي؟',
    tr: 'Kısa performansını üç soruyla geliştir: Görev karşılandı mı? Dil kontrollü mü? Zaman dağılımı gerçekçi mi?',
    ru: 'Улучши короткое выполнение тремя вопросами: задание выполнено? Язык контролируемый? Время распределено реалистично?',
    ckb: 'ئەنجامدانی کورتەکەت بە سێ پرسیار باشتر بکە: ئەرکەکە کراوە؟ زمانەکە کۆنترۆڵکراوە؟ دابەشکردنی کات ڕاستبینانەیە؟',
    kmr: 'Performansa xwe ya kurt bi sê pirsan baştir bike: erk hatî girtin? Ziman kontrolkirî ye? Dabeşkirina demê rastîn e?',
    pl: 'Popraw krótkie wykonanie trzema pytaniami: Czy zadanie zostało trafione? Czy język jest kontrolowany? Czy podział czasu jest realistyczny?',
    ro: 'Îmbunătățește prestația scurtă cu trei întrebări: Este sarcina atinsă? Este limbajul controlat? Este timpul împărțit realist?',
    sq: 'Përmirëso performancën e shkurtër me tri pyetje: A është trajtuar detyra? A është gjuha e kontrolluar? A është ndarja e kohës realiste?'
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

  const materialKind = item.target[0] === 'writing-template'
    ? 'write'
    : item.target[0] === 'roleplay'
      ? 'roleplay'
      : 'exam-prep';

  lesson.activityBlocks = [
    activity('read', 'orient', orientInstruction(item), 'none', null, 10, 6),
    activity('exam-prep', 'strategy', strategyInstruction(item), 'exam-prep-unit', item.strategy, 20, 10),
    activity('grammar', 'language', languageInstruction(item), 'grammar-topic', item.grammar[0], 30, 8),
    activity(materialKind, 'apply', applyInstruction(), item.target[0], item.target[1], 40, 8),
    activity('review', 'review', reviewInstruction(), 'none', null, 50, 5)
  ];
}

fs.writeFileSync(file, `${JSON.stringify(data, null, 2)}\n`);
console.log('Updated 10 C1 Module 11 lessons with 50 activity blocks.');

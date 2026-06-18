const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c1-souveraen-in-studium-beruf-und-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'Berufliche Aufgabe klaeren',
    en: 'Clarify the workplace task',
    fa: 'وظیفه کاری را روشن کن',
    ar: 'وضّح المهمة المهنية',
    tr: 'İş yerindeki görevi netleştir',
    ru: 'Проясни рабочую задачу',
    ckb: 'ئەرکی کار ڕوون بکەوە',
    kmr: 'Erka kar zelal bike',
    pl: 'Wyjaśnij zadanie zawodowe',
    ro: 'Clarifică sarcina profesională',
    sq: 'Qartëso detyrën profesionale'
  },
  language: {
    de: 'Wirkung sprachlich steuern',
    en: 'Control the effect through language',
    fa: 'اثر حرفت را با زبان کنترل کن',
    ar: 'تحكّم في الأثر عبر اللغة',
    tr: 'Etkisini dil yoluyla yönet',
    ru: 'Управляй эффектом через язык',
    ckb: 'کاریگەرییەکە بە زمان کۆنترۆڵ بکە',
    kmr: 'Bandorê bi zimanê xwe birêve bibe',
    pl: 'Steruj efektem przez język',
    ro: 'Controlează efectul prin limbaj',
    sq: 'Drejto efektin përmes gjuhës'
  },
  material: {
    de: 'Situation realistisch trainieren',
    en: 'Practise the situation realistically',
    fa: 'موقعیت را واقع‌بینانه تمرین کن',
    ar: 'تدرّب على الموقف بشكل واقعي',
    tr: 'Durumu gerçekçi biçimde çalış',
    ru: 'Отработай ситуацию реалистично',
    ckb: 'دۆخەکە بە شێوەی ڕاستەقینە ڕاهێنان بکە',
    kmr: 'Rewşê bi awayekî rastîn perwerde bike',
    pl: 'Przećwicz sytuację realistycznie',
    ro: 'Exersează situația realist',
    sq: 'Ushtroje situatën në mënyrë realiste'
  },
  apply: {
    de: 'Eigenen Vorschlag formulieren',
    en: 'Formulate your own proposal',
    fa: 'پیشنهاد خودت را دقیق بیان کن',
    ar: 'صغ اقتراحك أنت',
    tr: 'Kendi önerini formüle et',
    ru: 'Сформулируй собственное предложение',
    ckb: 'پێشنیازی خۆت داڕێژە',
    kmr: 'Pêşniyara xwe formule bike',
    pl: 'Sformułuj własną propozycję',
    ro: 'Formulează propria propunere',
    sq: 'Formulo propozimin tënd'
  },
  review: {
    de: 'Risiko und Ton pruefen',
    en: 'Check risk and tone',
    fa: 'ریسک و لحن را بررسی کن',
    ar: 'راجع الخطر والنبرة',
    tr: 'Riski ve tonu kontrol et',
    ru: 'Проверь риск и тон',
    ckb: 'مەترسی و تۆنەکە بپشکنە',
    kmr: 'Rîsk û tonê kontrol bike',
    pl: 'Sprawdź ryzyko i ton',
    ro: 'Verifică riscul și tonul',
    sq: 'Kontrollo rrezikun dhe tonin'
  }
};

const items = [
  {
    slug: 'c1-strategie-im-team-kritisch-pruefen',
    topic: {
      de: 'eine Strategie im Team kritisch pruefen',
      en: 'critically reviewing a team strategy',
      fa: 'بررسی انتقادی یک استراتژی در تیم',
      ar: 'مراجعة استراتيجية داخل الفريق بنظرة نقدية',
      tr: 'ekip içinde bir stratejiyi eleştirel değerlendirme',
      ru: 'критическая проверка стратегии в команде',
      ckb: 'پشکنینی ڕەخنەیی ستراتیژییەک لە تیمدا',
      kmr: 'nirxandina rexneyî ya stratejiyekê di tîmê de',
      pl: 'krytyczna ocena strategii w zespole',
      ro: 'evaluarea critică a unei strategii în echipă',
      sq: 'shqyrtimi kritik i një strategjie në ekip'
    },
    focus: {
      de: 'Kritik als Beitrag zur Verbesserung formulieren, nicht als persoenliche Ablehnung',
      en: 'framing criticism as a contribution to improvement, not as personal rejection',
      fa: 'انتقاد را به شکل کمک به بهتر شدن مطرح کنی، نه رد شخصی افراد',
      ar: 'صياغة النقد كإسهام في التحسين لا كرفض شخصي',
      tr: 'eleştiriyi kişisel ret değil, iyileştirmeye katkı olarak ifade etmeyi',
      ru: 'формулировать критику как вклад в улучшение, а не как личное неприятие',
      ckb: 'ڕەخنە وەک بەشدارییەک بۆ باشترکردن بڵێیت، نەک وەک ڕەتکردنەوەی کەسی',
      kmr: 'rexneyê wek beşdariyek ji bo baştirkirinê pêşkêş kirin, ne wek redkirina kesî',
      pl: 'formułowanie krytyki jako wkładu w poprawę, nie jako osobistego odrzucenia',
      ro: 'formularea criticii ca sprijin pentru îmbunătățire, nu ca respingere personală',
      sq: 'ta paraqes kritikën si ndihmë për përmirësim, jo si refuzim personal'
    },
    grammar: ['c1-expressing-nuance-and-limitation', 'Nuancierung und Begrenzung'],
    target: ['roleplay', 'c1-eine-strategie-im-team-kritisch-pruefen', 'Strategie im Team']
  },
  {
    slug: 'c1-change-prozess-erklaeren',
    topic: {
      de: 'einen Change-Prozess erklaeren',
      en: 'explaining a change process',
      fa: 'توضیح دادن یک روند تغییر در سازمان',
      ar: 'شرح عملية تغيير في المؤسسة',
      tr: 'bir değişim sürecini açıklama',
      ru: 'объяснение процесса изменений',
      ckb: 'ڕوونکردنەوەی پرۆسەی گۆڕانکاری',
      kmr: 'vegotina pêvajoya guhertinê',
      pl: 'wyjaśnianie procesu zmiany',
      ro: 'explicarea unui proces de schimbare',
      sq: 'shpjegimi i një procesi ndryshimi'
    },
    focus: {
      de: 'Ursache, Ziel, Belastung und naechsten Schritt nachvollziehbar verbinden',
      en: 'linking cause, goal, pressure, and next step in a traceable way',
      fa: 'علت، هدف، فشار کاری و قدم بعدی را قابل فهم به هم وصل کنی',
      ar: 'ربط السبب والهدف والضغط والخطوة التالية بطريقة مفهومة',
      tr: 'neden, hedef, yük ve sonraki adımı anlaşılır biçimde bağlamayı',
      ru: 'понятно связывать причину, цель, нагрузку и следующий шаг',
      ckb: 'هۆکار، ئامانج، فشار و هەنگاوی داهاتوو بە شێوەیەکی تێگەیشتوو ببەستیتەوە',
      kmr: 'sedem, armanc, bar û gava din bi awayekî têgihiştî girêdan',
      pl: 'zrozumiałe połączenie przyczyny, celu, obciążenia i następnego kroku',
      ro: 'legarea clară a cauzei, scopului, presiunii și pasului următor',
      sq: 'lidhjen qartë të shkakut, qëllimit, ngarkesës dhe hapit tjetër'
    },
    grammar: ['c1-causal-chains-in-formal-writing', 'Kausalketten'],
    target: ['roleplay', 'c1-einen-change-prozess-erklaeren-und-einordnen', 'Change-Prozess']
  },
  {
    slug: 'c1-stakeholder-von-vorschlag-ueberzeugen',
    topic: {
      de: 'Stakeholder von einem Vorschlag ueberzeugen',
      en: 'convincing stakeholders of a proposal',
      fa: 'قانع کردن ذی‌نفعان نسبت به یک پیشنهاد',
      ar: 'إقناع أصحاب المصلحة باقتراح',
      tr: 'paydaşları bir öneriye ikna etme',
      ru: 'убеждение заинтересованных сторон в предложении',
      ckb: 'قایلکردنی خاوەن بەرژەوەندییەکان بە پێشنیازێک',
      kmr: 'qanîkirina alîgirên peywendîdar bi pêşniyarekê',
      pl: 'przekonywanie interesariuszy do propozycji',
      ro: 'convingerea părților interesate de o propunere',
      sq: 'bindja e palëve të interesuara për një propozim'
    },
    focus: {
      de: 'Nutzen, Einwand und Entscheidungsspielraum in einer ueberzeugenden Reihenfolge darstellen',
      en: 'presenting benefit, objection, and room for decision in a convincing order',
      fa: 'فایده، ایراد احتمالی و فضای تصمیم‌گیری را با ترتیب قانع‌کننده نشان بدهی',
      ar: 'عرض الفائدة والاعتراض المحتمل وهامش القرار بترتيب مقنع',
      tr: 'fayda, itiraz ve karar alanını ikna edici sırayla sunmayı',
      ru: 'убедительно выстраивать пользу, возражение и пространство для решения',
      ckb: 'سوود، ڕەخنەی پێشبینیکراو و مەودای بڕیار بە ڕیزێکی قایلکەر پێشکەش بکەیت',
      kmr: 'sûd, îtiraza gengaz û qada biryarê bi rêzeke qanîker nîşan dan',
      pl: 'przedstawienie korzyści, zastrzeżenia i pola decyzji w przekonującej kolejności',
      ro: 'prezentarea beneficiului, obiecției și spațiului de decizie într-o ordine convingătoare',
      sq: 'paraqitjen e përfitimit, kundërshtimit dhe hapësirës së vendimit në rend bindës'
    },
    grammar: ['c1-hedging-and-cautious-language', 'vorsichtige Ueberzeugungssprache'],
    target: ['roleplay', 'c1-einen-stakeholder-von-einem-unpopulaeren-vorschlag-ueberzeugen', 'Stakeholder ueberzeugen']
  },
  {
    slug: 'c1-riskante-entscheidung-verteidigen',
    topic: {
      de: 'eine riskante Entscheidung verteidigen',
      en: 'defending a risky decision',
      fa: 'دفاع از یک تصمیم پرریسک',
      ar: 'الدفاع عن قرار محفوف بالمخاطر',
      tr: 'riskli bir kararı savunma',
      ru: 'защита рискованного решения',
      ckb: 'بەرگری لە بڕیارێکی پڕمەترسی',
      kmr: 'parastina biryareke bi rîsk',
      pl: 'obrona ryzykownej decyzji',
      ro: 'apărarea unei decizii riscante',
      sq: 'mbrojtja e një vendimi me rrezik'
    },
    focus: {
      de: 'Risiko offen benennen und trotzdem eine begruendete Entscheidung vertreten',
      en: 'naming risk openly while still defending a reasoned decision',
      fa: 'ریسک را شفاف بگویی و با این حال از تصمیمی دلیل‌دار دفاع کنی',
      ar: 'تسمية الخطر بوضوح مع الدفاع عن قرار مبرر',
      tr: 'riski açıkça adlandırıp yine de gerekçeli kararı savunmayı',
      ru: 'открыто назвать риск и всё же защитить обоснованное решение',
      ckb: 'مەترسی بە ڕوونی ناو ببەیت و هێشتا بەرگری لە بڕیارێکی هۆکاربەدار بکەیت',
      kmr: 'rîskê bi zelalî nav kirin û dîsa biryareke bi sedem parastin',
      pl: 'otwarte nazwanie ryzyka i jednoczesna obrona uzasadnionej decyzji',
      ro: 'numirea deschisă a riscului și susținerea unei decizii motivate',
      sq: 'ta thuash hapur rrezikun dhe prapë të mbrosh një vendim të arsyetuar'
    },
    grammar: ['c1-concession-structures', 'Konzession und Begruendung'],
    target: ['roleplay', 'c1-eine-riskante-entscheidung-verteidigen', 'riskante Entscheidung']
  },
  {
    slug: 'c1-strategisches-meeting-zusammenfassen',
    topic: {
      de: 'ein strategisches Meeting zusammenfassen',
      en: 'summarizing a strategic meeting',
      fa: 'جمع‌بندی یک جلسه استراتژیک',
      ar: 'تلخيص اجتماع استراتيجي',
      tr: 'stratejik bir toplantıyı özetleme',
      ru: 'подведение итогов стратегического совещания',
      ckb: 'پوختەکردنەوەی کۆبوونەوەیەکی ستراتیژی',
      kmr: 'kurteya civîneke stratejîk',
      pl: 'podsumowanie spotkania strategicznego',
      ro: 'rezumarea unei ședințe strategice',
      sq: 'përmbledhja e një takimi strategjik'
    },
    focus: {
      de: 'Entscheidungen, offene Punkte und Verantwortlichkeiten klar trennen',
      en: 'clearly separating decisions, open points, and responsibilities',
      fa: 'تصمیم‌ها، موارد باز و مسئولیت‌ها را روشن از هم جدا کنی',
      ar: 'الفصل بوضوح بين القرارات والنقاط المفتوحة والمسؤوليات',
      tr: 'kararları, açık noktaları ve sorumlulukları net ayırmayı',
      ru: 'четко разделять решения, открытые вопросы и обязанности',
      ckb: 'بڕیار، خاڵە کراوەکان و بەرپرسیاریەتییەکان بە ڕوونی جیا بکەیتەوە',
      kmr: 'biryar, xalên vekirî û berpirsiyariyan bi zelalî cuda kirin',
      pl: 'jasne oddzielenie decyzji, kwestii otwartych i odpowiedzialności',
      ro: 'separarea clară a deciziilor, punctelor deschise și responsabilităților',
      sq: 'ndarjen e qartë të vendimeve, pikave të hapura dhe përgjegjësive'
    },
    grammar: ['c1-formal-summaries', 'formelle Zusammenfassungen'],
    target: ['roleplay', 'c1-ein-strategisches-meeting-zusammenfassen', 'strategisches Meeting']
  },
  {
    slug: 'c1-transparenz-und-diplomatie-abwaegen',
    topic: {
      de: 'Transparenz und Diplomatie abwaegen',
      en: 'balancing transparency and diplomacy',
      fa: 'سنجیدن شفافیت و دیپلماسی در محیط کار',
      ar: 'الموازنة بين الشفافية والدبلوماسية',
      tr: 'şeffaflık ile diplomasiyi dengeleme',
      ru: 'баланс между прозрачностью и дипломатичностью',
      ckb: 'هاوسەنگکردنی ڕوونی و دیپلۆماسی',
      kmr: 'hevsengkirina zelalî û dîplomasiyê',
      pl: 'ważenie przejrzystości i dyplomacji',
      ro: 'echilibrarea transparenței și diplomației',
      sq: 'balancimi i transparencës dhe diplomacisë'
    },
    focus: {
      de: 'notwendige Klarheit geben, ohne Vertrauen oder Beziehung unnoetig zu beschaedigen',
      en: 'giving necessary clarity without unnecessarily damaging trust or relationships',
      fa: 'شفافیت لازم را بدهی، بدون اینکه بی‌دلیل به اعتماد یا رابطه کاری آسیب بزنی',
      ar: 'تقديم الوضوح اللازم من دون إلحاق ضرر غير ضروري بالثقة أو العلاقة',
      tr: 'gerekli açıklığı verip güveni veya ilişkiyi gereksiz yere zedelememeyi',
      ru: 'давать необходимую ясность, не разрушая без нужды доверие или отношения',
      ckb: 'ڕوونی پێویست بدەیت، بەبێ ئەوەی بە بێهۆکار زیان بە متمانە یان پەیوەندی بدەیت',
      kmr: 'zelaliya pêwîst bidî bêyî ku bêhewce zirar bide bawerî an peywendiyê',
      pl: 'dać potrzebną jasność bez niepotrzebnego niszczenia zaufania lub relacji',
      ro: 'oferirea clarității necesare fără a afecta inutil încrederea sau relația',
      sq: 'të japësh qartësinë e nevojshme pa dëmtuar panevojshëm besimin ose marrëdhënien'
    },
    grammar: ['c1-register-shifting', 'Registerwechsel'],
    target: ['roleplay', 'c1-eine-fuehrungsentscheidung-taktvoll-kritisieren', 'taktvolle Kritik']
  },
  {
    slug: 'c1-ressourcenknappheit-verhandeln',
    topic: {
      de: 'Ressourcenknappheit verhandeln',
      en: 'negotiating scarce resources',
      fa: 'مذاکره درباره کمبود منابع',
      ar: 'التفاوض حول ندرة الموارد',
      tr: 'kaynak kıtlığını müzakere etme',
      ru: 'переговоры при нехватке ресурсов',
      ckb: 'دانوسان لەسەر کەمی سەرچاوەکان',
      kmr: 'danûstandin li ser kêmbûna çavkaniyan',
      pl: 'negocjowanie niedoboru zasobów',
      ro: 'negocierea resurselor limitate',
      sq: 'negocimi i mungesës së burimeve'
    },
    focus: {
      de: 'Prioritaeten, Zumutbarkeit und Alternativen sachlich aushandeln',
      en: 'negotiating priorities, feasibility, and alternatives factually',
      fa: 'اولویت‌ها، میزان قابل‌قبول بودن فشار و گزینه‌های جایگزین را عینی مذاکره کنی',
      ar: 'التفاوض موضوعيًا حول الأولويات والقدرة على التحمل والبدائل',
      tr: 'öncelikleri, uygulanabilirliği ve alternatifleri nesnel biçimde müzakere etmeyi',
      ru: 'предметно обсуждать приоритеты, допустимую нагрузку и альтернативы',
      ckb: 'پێشینەکان، توانای وەرگرتنی فشار و جێگرەوەکان بە بابەتی دانوسان بکەیت',
      kmr: 'pêşîti, tolerans û alternatîfan bi awayekî babetî danûstandin kirin',
      pl: 'rzeczowe negocjowanie priorytetów, wykonalności i alternatyw',
      ro: 'negocierea factuală a priorităților, fezabilității și alternativelor',
      sq: 'negocimin objektiv të prioriteteve, mundësisë reale dhe alternativave'
    },
    grammar: ['c1-concession-structures', 'Konzession und Bedingung'],
    target: ['roleplay', 'c1-ressourcenknappheit-in-einem-projekt-verhandeln', 'Ressourcenknappheit im Projekt']
  },
  {
    slug: 'c1-interne-fehlentscheidung-aufarbeiten',
    topic: {
      de: 'eine interne Fehlentscheidung aufarbeiten',
      en: 'working through an internal wrong decision',
      fa: 'بررسی و جمع‌بندی یک تصمیم اشتباه داخلی',
      ar: 'معالجة قرار داخلي خاطئ',
      tr: 'içeride alınmış yanlış bir kararı değerlendirme',
      ru: 'разбор внутреннего ошибочного решения',
      ckb: 'چارەسەرکردنی بڕیارێکی هەڵەی ناوخۆیی',
      kmr: 'nirxandin û çareserkirina biryareke şaş a hundurîn',
      pl: 'przepracowanie błędnej decyzji wewnętrznej',
      ro: 'analizarea unei decizii interne greșite',
      sq: 'analizimi i një vendimi të brendshëm të gabuar'
    },
    focus: {
      de: 'Verantwortung klaeren, ohne Schuldzuweisung und ohne das Problem zu verharmlosen',
      en: 'clarifying responsibility without blame and without minimizing the problem',
      fa: 'مسئولیت را روشن کنی، بدون مقصرسازی و بدون کوچک‌کردن مشکل',
      ar: 'توضيح المسؤولية من دون لوم ومن دون التقليل من المشكلة',
      tr: 'sorumluluğu suçlama yapmadan ve sorunu küçültmeden netleştirmeyi',
      ru: 'прояснять ответственность без обвинений и без преуменьшения проблемы',
      ckb: 'بەرپرسیاریەتی ڕوون بکەیت، بەبێ تاوانبارکردن و بەبێ بچووککردنەوەی کێشەکە',
      kmr: 'berpirsiyarî zelal kirin bê sûcdarkirin û bê biçûkkirina pirsgirêkê',
      pl: 'wyjaśnienie odpowiedzialności bez obwiniania i bez pomniejszania problemu',
      ro: 'clarificarea responsabilității fără învinovățire și fără minimalizarea problemei',
      sq: 'qartësimin e përgjegjësisë pa fajësim dhe pa e zvogëluar problemin'
    },
    grammar: ['c1-indirect-speech-in-journalism-and-formal-contexts', 'indirekte Wiedergabe'],
    target: ['roleplay', 'c1-eine-interne-fehlentscheidung-aufarbeiten', 'interne Fehlentscheidung']
  },
  {
    slug: 'c1-ethischen-konflikt-im-unternehmen-ansprechen',
    topic: {
      de: 'einen ethischen Konflikt im Unternehmen ansprechen',
      en: 'addressing an ethical conflict in a company',
      fa: 'مطرح کردن یک تعارض اخلاقی در شرکت',
      ar: 'طرح تعارض أخلاقي داخل الشركة',
      tr: 'şirkette etik bir çatışmayı gündeme getirme',
      ru: 'обсуждение этического конфликта в компании',
      ckb: 'باسکردنی ناکۆکییەکی ئەخلاقی لە کۆمپانیا',
      kmr: 'gotina nakokiyeke exlaqî di pargîdaniyê de',
      pl: 'poruszenie konfliktu etycznego w firmie',
      ro: 'abordarea unui conflict etic într-o companie',
      sq: 'ngritja e një konflikti etik në kompani'
    },
    focus: {
      de: 'ethische Bedenken konkret benennen, ohne moralisch pauschal zu urteilen',
      en: 'naming ethical concerns concretely without making sweeping moral judgments',
      fa: 'نگرانی اخلاقی را مشخص بگویی، بدون اینکه کلی و اخلاقی‌زده قضاوت کنی',
      ar: 'تسمية المخاوف الأخلاقية بشكل محدد من دون أحكام أخلاقية عامة',
      tr: 'etik kaygıları somut adlandırıp genelleyici ahlaki yargıdan kaçınmayı',
      ru: 'конкретно называть этические опасения без обобщающих моральных оценок',
      ckb: 'نیگەرانی ئەخلاقی بە دیاریکراوی ناو ببەیت، بەبێ داوەریی گشتی ئەخلاقی',
      kmr: 'xemên exlaqî bi awayekî konkret nav kirin bê dadbariya exlaqî ya giştî',
      pl: 'konkretne nazwanie wątpliwości etycznych bez ogólnych ocen moralnych',
      ro: 'numirea concretă a preocupărilor etice fără judecăți morale generale',
      sq: 'emërtimin konkret të shqetësimeve etike pa gjykime morale të përgjithshme'
    },
    grammar: ['c1-hedging-and-cautious-language', 'vorsichtige Sprache'],
    target: ['roleplay', 'c1-einen-ethischen-konflikt-im-unternehmen-ansprechen', 'ethischer Konflikt im Unternehmen']
  },
  {
    slug: 'c1-beruf-strategie-und-organisation-wiederholen',
    topic: {
      de: 'Beruf, Strategie und Organisation wiederholen',
      en: 'reviewing work, strategy, and organization',
      fa: 'مرور کار، استراتژی و سازمان',
      ar: 'مراجعة العمل والاستراتيجية والتنظيم',
      tr: 'iş, strateji ve organizasyonu tekrar etme',
      ru: 'повторение тем работы, стратегии и организации',
      ckb: 'دووبارەکردنەوەی کار، ستراتیژی و ڕێکخستن',
      kmr: 'dubarekirina kar, stratejî û rêxistinê',
      pl: 'powtórka pracy, strategii i organizacji',
      ro: 'recapitularea muncii, strategiei și organizării',
      sq: 'përsëritja e punës, strategjisë dhe organizimit'
    },
    focus: {
      de: 'Analyse, Diplomatie und Entscheidungssprache in einem beruflichen Gespraech verbinden',
      en: 'combining analysis, diplomacy, and decision language in a workplace conversation',
      fa: 'تحلیل، دیپلماسی و زبان تصمیم‌گیری را در یک گفت‌وگوی کاری به هم وصل کنی',
      ar: 'ربط التحليل والدبلوماسية ولغة القرار في حوار مهني واحد',
      tr: 'analizi, diplomasiyi ve karar dilini iş konuşmasında birleştirmeyi',
      ru: 'соединять анализ, дипломатичность и язык решений в рабочем разговоре',
      ckb: 'شیکردنەوە، دیپلۆماسی و زمانی بڕیار لە گفتوگۆیەکی کاری دا پێکەوە ببەستیت',
      kmr: 'analîz, dîplomasî û zimanê biryarê di axaftineke karî de girêdan',
      pl: 'łączenie analizy, dyplomacji i języka decyzji w rozmowie zawodowej',
      ro: 'îmbinarea analizei, diplomației și limbajului deciziei într-o conversație profesională',
      sq: 'lidhjen e analizës, diplomacisë dhe gjuhës së vendimit në një bisedë pune'
    },
    grammar: ['c1-c1-register-review', 'Register-Review'],
    target: ['exam-prep-unit', 'c1-standpunkt-muendlich-sachlich-vertreten', 'sachlicher Standpunkt']
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
    de: `Lies den Lektionstext zu ${item.topic.de} und notiere, welche berufliche Aufgabe zuerst geklaert werden muss.`,
    en: `Read the lesson text on ${item.topic.en} and note which workplace task must be clarified first.`,
    fa: `متن درس درباره ${item.topic.fa} را بخوان و یادداشت کن اول باید کدام وظیفه کاری روشن شود.`,
    ar: `اقرأ نص الدرس عن ${item.topic.ar} وسجّل أي مهمة مهنية يجب توضيحها أولًا.`,
    tr: `${item.topic.tr} konulu ders metnini oku ve önce hangi iş görevinin netleşmesi gerektiğini not et.`,
    ru: `Прочитай текст урока о теме «${item.topic.ru}» и отметь, какую рабочую задачу нужно прояснить первой.`,
    ckb: `دەقی وانەکە لەسەر ${item.topic.ckb} بخوێنەوە و بنووسە سەرەتا کام ئەرکی کاری دەبێت ڕوون بکرێتەوە.`,
    kmr: `Nivîsa dersê li ser ${item.topic.kmr} bixwîne û binivîse pêşî kîjan erka kar divê zelal bibe.`,
    pl: `Przeczytaj tekst lekcji o temacie: ${item.topic.pl} i zanotuj, które zadanie zawodowe trzeba najpierw wyjaśnić.`,
    ro: `Citește textul lecției despre ${item.topic.ro} și notează ce sarcină profesională trebuie clarificată prima.`,
    sq: `Lexo tekstin e mësimit për ${item.topic.sq} dhe shëno cila detyrë profesionale duhet qartësuar e para.`
  };
}

function languageInstruction(item) {
  return {
    de: `Oeffne ${item.grammar[1]} und sammle drei Formulierungen, mit denen du ${item.focus.de}.`,
    en: `Open the linked grammar section and collect three formulations that help you with this skill: ${item.focus.en}.`,
    fa: `بخش گرامر لینک‌شده را باز کن و سه عبارت پیدا کن که به این مهارت کمک کند: ${item.focus.fa}.`,
    ar: `افتح قسم القواعد المرتبط واجمع ثلاث صيغ تساعد على هذه المهارة: ${item.focus.ar}.`,
    tr: `Bağlantılı dil bilgisi bölümünü aç ve şu beceriye yardım eden üç ifade topla: ${item.focus.tr}.`,
    ru: `Открой связанный раздел грамматики и собери три формулировки для этого умения: ${item.focus.ru}.`,
    ckb: `بەشی ڕێزمانی بەستەرکراو بکەرەوە و سێ دەربڕین کۆبکەوە کە یارمەتی ئەم توانایە بدات: ${item.focus.ckb}.`,
    kmr: `Beşa rêzimanê ya girêdayî veke û sê derbirînan kom bike ku alîkarîya vê jêhatinê bikin: ${item.focus.kmr}.`,
    pl: `Otwórz połączoną sekcję gramatyki i zbierz trzy sformułowania wspierające tę umiejętność: ${item.focus.pl}.`,
    ro: `Deschide secțiunea de gramatică legată și adună trei formulări care susțin această abilitate: ${item.focus.ro}.`,
    sq: `Hap seksionin e lidhur të gramatikës dhe mblidh tri formulime që ndihmojnë këtë aftësi: ${item.focus.sq}.`
  };
}

function materialInstruction(item) {
  return {
    de: `Bearbeite das verlinkte Material zu ${item.target[2]} und achte darauf, wie diese Situation ohne Eskalation gesteuert wird.`,
    en: `Work through the linked practice material and notice how the situation is managed without escalation.`,
    fa: `تمرین لینک‌شده را انجام بده و دقت کن این موقعیت چطور بدون تشدید تنش مدیریت می‌شود.`,
    ar: `اعمل على التدريب المرتبط وانتبه إلى كيفية إدارة الموقف من دون تصعيد.`,
    tr: `Bağlantılı alıştırmayı çalış ve durumun gerilimi artırmadan nasıl yönetildiğine dikkat et.`,
    ru: `Проработай связанное практическое задание и обрати внимание, как ситуация управляется без эскалации.`,
    ckb: `ڕاهێنانی بەستەرکراو کاربکە و سەرنج بدە دۆخەکە چۆن بەبێ توندکردنەوە بەڕێوەدەبرێت.`,
    kmr: `Rahênana girêdayî bixebite û bala xwe bide ka rewş bê eskalasyon çawa tê rêvebirin.`,
    pl: `Przerób połączone ćwiczenie i zwróć uwagę, jak prowadzić sytuację bez eskalacji.`,
    ro: `Lucrează cu exercițiul legat și observă cum este gestionată situația fără escaladare.`,
    sq: `Puno me ushtrimin e lidhur dhe vëzhgo si menaxhohet situata pa përshkallëzim.`
  };
}

function applyInstruction(item) {
  return {
    de: `Formuliere einen kurzen beruflichen Beitrag in vier Teilen: Ausgangslage, Kernpunkt, begruendeter Vorschlag und naechster Schritt.`,
    en: `Formulate a short workplace contribution in four parts: situation, core point, reasoned proposal, and next step.`,
    fa: `یک مشارکت کوتاه کاری در چهار بخش بنویس: وضعیت اولیه، نکته اصلی، پیشنهاد دلیل‌دار و قدم بعدی.`,
    ar: `صغ مداخلة مهنية قصيرة في أربعة أجزاء: الوضع، النقطة الأساسية، اقتراح مبرر، والخطوة التالية.`,
    tr: `Dört parçalı kısa bir iş katkısı formüle et: durum, ana nokta, gerekçeli öneri ve sonraki adım.`,
    ru: `Сформулируй короткий рабочий вклад из четырех частей: исходная ситуация, главный пункт, обоснованное предложение и следующий шаг.`,
    ckb: `بەشدارییەکی کاریی کورت بە چوار بەش داڕێژە: دۆخی سەرەتا، خاڵی سەرەکی، پێشنیازی هۆکاربەدار و هەنگاوی داهاتوو.`,
    kmr: `Beşdariyeke karî ya kurt bi çar beşan formule bike: rewşa destpêkê, xalê sereke, pêşniyara bi sedem û gava din.`,
    pl: `Sformułuj krótką wypowiedź zawodową w czterech częściach: sytuacja, główny punkt, uzasadniona propozycja i następny krok.`,
    ro: `Formulează o scurtă intervenție profesională în patru părți: situația, punctul principal, propunerea motivată și pasul următor.`,
    sq: `Formulo një ndërhyrje të shkurtër profesionale në katër pjesë: situata, pika kryesore, propozimi i arsyetuar dhe hapi tjetër.`
  };
}

function reviewInstruction(item) {
  return {
    de: `Pruefe, ob deine Formulierung praezise, diplomatisch und handlungsorientiert bleibt, ohne das Kernproblem zu verstecken.`,
    en: `Check whether your wording stays precise, diplomatic, and action-oriented without hiding the central problem.`,
    fa: `بررسی کن که جمله‌بندی تو دقیق، دیپلماتیک و عملی می‌ماند، بدون اینکه مشکل اصلی را پنهان کند.`,
    ar: `راجع ما إذا كانت صياغتك دقيقة ودبلوماسية وموجهة نحو العمل من دون إخفاء المشكلة الأساسية.`,
    tr: `İfadenin ana sorunu saklamadan kesin, diplomatik ve eyleme dönük kaldığını kontrol et.`,
    ru: `Проверь, остается ли формулировка точной, дипломатичной и ориентированной на действие, не скрывая главную проблему.`,
    ckb: `بپشکنە داڕشتنەکەت ورد، دیپلۆماسی و کرداری دەمێنێتەوە، بەبێ ئەوەی کێشەی سەرەکی بشارێتەوە.`,
    kmr: `Kontrol bike ka formulekirina te rast, dîplomatîk û ber bi çalakiyê dimîne, bêyî ku pirsgirêka sereke veşêre.`,
    pl: `Sprawdź, czy sformułowanie pozostaje precyzyjne, dyplomatyczne i nastawione na działanie, nie ukrywając głównego problemu.`,
    ro: `Verifică dacă formularea rămâne precisă, diplomatică și orientată spre acțiune fără să ascundă problema centrală.`,
    sq: `Kontrollo nëse formulimi yt mbetet i saktë, diplomatik dhe i orientuar drejt veprimit pa fshehur problemin kryesor.`
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
console.log(`Updated ${items.length} C1 Module 6 lessons with ${items.length * 5} activity blocks.`);

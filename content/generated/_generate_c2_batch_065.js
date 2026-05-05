const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '065';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['das Postulat','postulieren','die Präfiguration','die Prägnanz','prätentiös','die Pragmalinguistik'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const topicKeys = new Set((taxonomy.topics || []).map(t => t.key));
const labelsByKey = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const source = fs.readFileSync(sourcePath, 'utf8');
const tokens = source.split(',').map(s => s.trim()).filter(Boolean);
const first = tokens.slice(0, 6);
if (JSON.stringify(first) !== JSON.stringify(expected)) throw new Error(`Unexpected first tokens: ${JSON.stringify(first)}`);

function meaning(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) { return [ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr].map((text, i) => ({ language: langs[i], text })); }
function trans(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) { return meaning(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr); }
function ex(baseText, translations) { return { baseText, translations }; }
function entry(e) {
  for (const t of e.topics) if (!topicKeys.has(t)) throw new Error(`Unknown topic ${t}`);
  for (const l of [...(e.usageLabels || []), ...(e.contextLabels || [])]) if (!labelsByKey.has(l)) throw new Error(`Unknown label ${l}`);
  return Object.assign({ language: 'de', cefrLevel: level, article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: null, contextLabels: [], grammarNotes: [], collocations: [], wordFamilies: [], relations: [] }, e);
}

const entries = [
  entry({
    word: 'das Postulat', partOfSpeech: 'Noun', article: 'das', plural: 'Postulate', syllableBreak: 'Pos-tu-lat',
    topics: ['advanced-analysis','education-and-training','business-communication'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'ein Postulat aufstellen', meaning: 'to formulate a postulate' }],
    meanings: meaning('مسلّمة؛ مطلب أساسي','بنەمای پێشدانراو؛ داواکاریی بنەڕەتی','postulate; fundamental assumption or demand','اصل موضوع؛ فرض یا مطالبه بنیادین','postulat; texmîna bingehîn','postulat; założenie podstawowe','postulat; presupunere fundamentală','постулат; основополагающее требование','postulat; supozim themelor','postüla; temel varsayım'),
    examples: [
      ex('Das Postulat vollständiger Transparenz klingt überzeugend, stößt aber in sicherheitskritischen Systemen schnell an praktische Grenzen.', trans('تبدو مسلّمة الشفافية الكاملة مقنعة، لكنها تصطدم سريعاً بحدود عملية في الأنظمة الحساسة أمنياً.','بنەمای شەفافیەتی تەواو قایلکەر دەنگ دەدات، بەڵام لە سیستەمە هەستیارەکانی ئاسایشدا زوو تووشی سنووری کرداری دەبێت.','The postulate of complete transparency sounds persuasive, but in security-critical systems it quickly meets practical limits.','اصل شفافیت کامل قانع‌کننده به نظر می‌رسد، اما در سیستم‌های حساس امنیتی سریعاً به محدودیت‌های عملی می‌رسد.','Postulata şefafiyeta tevahî qanihker xuya dike, lê di pergalên ewlehiyê-hestyar de zû digihêje sînorên pratîk.','Postulat pełnej przejrzystości brzmi przekonująco, lecz w systemach krytycznych dla bezpieczeństwa szybko napotyka praktyczne granice.','Postulatul transparenței complete sună convingător, dar în sistemele critice de securitate ajunge rapid la limite practice.','Постулат полной прозрачности звучит убедительно, но в критически важных для безопасности системах быстро сталкивается с практическими пределами.','Postulati i transparencës së plotë tingëllon bindës, por në sistemet kritike për sigurinë ndesh shpejt kufij praktikë.','Tam şeffaflık postülası ikna edici görünür, ancak güvenlik açısından kritik sistemlerde hızla pratik sınırlara çarpar.')),
      ex('Im Seminar wurde das Postulat diskutiert, dass literarische Bedeutung nie unabhängig von ihrer historischen Lesart entsteht.', trans('نوقشت في الحلقة الدراسية مسلّمة أن المعنى الأدبي لا ينشأ أبداً بمعزل عن قراءته التاريخية.','لە سیمینارەکەدا ئەو بنەمایە گفتوگۆی لەسەر کرا کە مانای ئەدەبی هەرگیز بەبێ خوێندنەوەی مێژوویی دروست نابێت.','In the seminar, they discussed the postulate that literary meaning never arises independently of its historical reading.','در سمینار این اصل بررسی شد که معنای ادبی هرگز مستقل از خوانش تاریخی آن شکل نمی‌گیرد.','Di seminarê de ew postulat hate nîqaşkirin ku wateya edebî qet ji xwendina xwe ya dîrokî serbixwe çênabe.','Na seminarium dyskutowano postulat, że znaczenie literackie nigdy nie powstaje niezależnie od historycznego odczytania.','La seminar s-a discutat postulatul că sensul literar nu apare niciodată independent de lectura sa istorică.','На семинаре обсуждался постулат о том, что литературный смысл никогда не возникает независимо от его исторического прочтения.','Në seminar u diskutua postulati se kuptimi letrar nuk lind kurrë i pavarur nga leximi i tij historik.','Seminerde, edebi anlamın tarihsel okumasından bağımsız olarak asla oluşmadığı postülası tartışıldı.'))
    ]
  }),
  entry({
    word: 'postulieren', partOfSpeech: 'Verb', infinitive: 'postulieren', syllableBreak: 'pos-tu-lie-ren',
    topics: ['advanced-analysis','education-and-training','law-and-compliance'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'eine These postulieren', meaning: 'to postulate a thesis' }],
    meanings: meaning('يفترض كمبدأ؛ يطالب نظرياً','وەک بنەما دانان؛ داواکاریی تیۆری کردن','to postulate; to assert as a premise','اصل گرفتن؛ فرض بنیادین مطرح کردن','wek bingeh danîn; postulat kirin','postulować; zakładać','a postula; a presupune ca premisă','постулировать','postuloj; vendos si premisë','postüle etmek; varsayım olarak ileri sürmek'),
    examples: [
      ex('Die Studie postuliert keinen direkten Kausalzusammenhang, sondern beschreibt zunächst eine auffällige Korrelation.', trans('لا تفترض الدراسة علاقة سببية مباشرة، بل تصف أولاً ارتباطاً لافتاً.','توێژینەوەکە پەیوەندیی هۆکاریی ڕاستەوخۆ وەک بنەما دانانێت، بەڵکو سەرەتا هاوپەیوەندییەکی سەرنجڕاکێش وەسف دەکات.','The study does not postulate a direct causal link, but first describes a striking correlation.','مطالعه رابطه علّی مستقیم را اصل نمی‌گیرد، بلکه ابتدا یک همبستگی چشمگیر را توصیف می‌کند.','Lêkolîn têkiliyeke sedemî ya rasterast napostulîne, lê pêşî korelasyoneke balkêş vedibêje.','Badanie nie postuluje bezpośredniego związku przyczynowego, lecz najpierw opisuje uderzającą korelację.','Studiul nu postulează o legătură cauzală directă, ci descrie mai întâi o corelație evidentă.','Исследование не постулирует прямую причинную связь, а сначала описывает заметную корреляцию.','Studimi nuk postulon një lidhje të drejtpërdrejtë shkakësore, por fillimisht përshkruan një korrelacion të dukshëm.','Çalışma doğrudan bir nedensellik ilişkisi postüle etmiyor, önce dikkat çekici bir korelasyonu tanımlıyor.')),
      ex('Wer absolute Neutralität postuliert, muss erklären, welche Interessen durch diese Neutralität unsichtbar bleiben.', trans('من يفترض حياداً مطلقاً يجب أن يوضح أي مصالح تبقى غير مرئية بسبب هذا الحياد.','ئەوەی بێلایەنیی تەواو وەک بنەما دادەنێت، دەبێت ڕوون بکاتەوە کام بەرژەوەندییەکان بەهۆی ئەو بێلایەنییە نادیار دەمێننەوە.','Anyone who postulates absolute neutrality must explain which interests remain invisible through that neutrality.','کسی که بی‌طرفی مطلق را اصل می‌گیرد، باید توضیح دهد کدام منافع به واسطه همین بی‌طرفی نامرئی می‌مانند.','Kesê ku bêalîbûna tevahî dipostulîne, divê rave bike ka kîjan berjewendî bi vê bêalîbûnê ve nayên dîtin.','Kto postuluje absolutną neutralność, musi wyjaśnić, jakie interesy pozostają przez tę neutralność niewidoczne.','Cine postulează neutralitatea absolută trebuie să explice ce interese rămân invizibile prin această neutralitate.','Тот, кто постулирует абсолютную нейтральность, должен объяснить, какие интересы из-за этой нейтральности остаются невидимыми.','Kush postulon neutralitet absolut duhet të shpjegojë cilat interesa mbeten të padukshme përmes këtij neutraliteti.','Mutlak tarafsızlığı postüle eden kişi, bu tarafsızlık nedeniyle hangi çıkarların görünmez kaldığını açıklamalıdır.'))
    ]
  }),
  entry({
    word: 'die Präfiguration', partOfSpeech: 'Noun', article: 'die', plural: 'Präfigurationen', syllableBreak: 'Prä-fi-gu-ra-tion',
    topics: ['culture-and-media','advanced-analysis','education-and-training'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'eine spätere Entwicklung präfigurieren', meaning: 'to prefigure a later development' }],
    meanings: meaning('تمهيد رمزي؛ صورة مسبقة','پێشوەختە وێنەکردن؛ پێشنیساندان','prefiguration; anticipatory representation','پیش‌نمونگی؛ پیش‌تصویرسازی','pêşwênekirin; pêşnîşandan','prefiguracja; zapowiedź','prefigurare; anticipare simbolică','префигурация; предвосхищение','parafytyrim; paralajmërim simbolik','önbiçimleme; önceden simgesel gösterim'),
    examples: [
      ex('Die frühe Szene mit dem beschädigten Archiv wirkt wie eine Präfiguration des späteren institutionellen Zerfalls.', trans('يبدو المشهد المبكر للأرشيف المتضرر كتمهيد رمزي للانهيار المؤسسي اللاحق.','دیمەنی سەرەتایی ئەرشیفی زیانلێکەوتوو وەک پێشوەختە وێنەکردنی داڕمانی دامەزراوەیی دواتر دەردەکەوێت.','The early scene with the damaged archive seems like a prefiguration of the later institutional collapse.','صحنه ابتدایی با آرشیو آسیب‌دیده مثل پیش‌نمونه‌ای از فروپاشی نهادی بعدی عمل می‌کند.','Dîmena destpêkê ya arşîva zirardar wek pêşwênekirina hilweşîna sazûmanî ya paşê xuya dike.','Wczesna scena z uszkodzonym archiwum działa jak prefiguracja późniejszego rozpadu instytucjonalnego.','Scena timpurie cu arhiva deteriorată pare o prefigurare a prăbușirii instituționale ulterioare.','Ранняя сцена с поврежденным архивом выглядит как префигурация последующего институционального распада.','Skena e hershme me arkivin e dëmtuar duket si parafytyrim i shpërbërjes së mëvonshme institucionale.','Hasarlı arşivle ilgili erken sahne, sonraki kurumsal çöküşün önbiçimlemesi gibi görünür.')),
      ex('In der Strategie galt der kleine Pilotprozess als Präfiguration einer vollständig automatisierten Lieferkette.', trans('في الاستراتيجية عُدّت العملية التجريبية الصغيرة صورة مسبقة لسلسلة توريد مؤتمتة بالكامل.','لە ستراتیژیاکەدا پڕۆسەی تاقیکاریی بچووک وەک پێشوەختە وێنەکردنی زنجیرەی دابینکردنی تەواو ئۆتۆماتیکی هەژمارکرا.','In the strategy, the small pilot process was treated as a prefiguration of a fully automated supply chain.','در راهبرد، فرایند آزمایشی کوچک به‌عنوان پیش‌نمونه‌ای از زنجیره تأمین کاملاً خودکار تلقی شد.','Di stratejiyê de pêvajoya pîlot a biçûk wek pêşwêneya zincîra dabînkirinê ya bi tevahî otomatîk hate dîtin.','W strategii mały proces pilotażowy uznano za prefigurację w pełni zautomatyzowanego łańcucha dostaw.','În strategie, micul proces pilot a fost considerat o prefigurare a unui lanț de aprovizionare complet automatizat.','В стратегии небольшой пилотный процесс рассматривался как префигурация полностью автоматизированной цепочки поставок.','Në strategji, procesi i vogël pilot u konsiderua si parafytyrim i një zinxhiri furnizimi plotësisht të automatizuar.','Stratejide küçük pilot süreç, tamamen otomatik bir tedarik zincirinin önbiçimlemesi olarak görüldü.'))
    ]
  }),
  entry({
    word: 'die Prägnanz', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Prä-gnanz',
    topics: ['business-communication','culture-and-media','advanced-analysis'], usageLabels: ['formal','written','analysis','business'],
    grammarNotes: ['feminine abstract noun; normally used in singular'],
    collocations: [{ text: 'an Prägnanz gewinnen', meaning: 'to gain concision and force' }],
    meanings: meaning('إيجاز ودقة لافتة','کورت و ڕوونیی کاریگەر','conciseness; striking precision','ایجاز و دقت اثرگذار','kurtebûn û zelaliya bandorker','zwięzłość; wyrazistość','concizie; pregnanță','лаконичность; выразительность','përpikëri dhe shkurtësi shprehëse','özlülük; çarpıcı açıklık'),
    examples: [
      ex('Die Prägnanz der Fehlermeldung entscheidet oft darüber, ob der Supportfall in Minuten oder erst nach Stunden gelöst wird.', trans('غالباً ما يحدد إيجاز رسالة الخطأ ووضوحها ما إذا كانت حالة الدعم ستُحل خلال دقائق أم بعد ساعات.','کورت و ڕوونی پەیامی هەڵە زۆرجار بڕیار دەدات کە کەیسی پشتگیری لە چەند خولەکێکدا چارەسەر دەبێت یان پاش چەند کاتژمێر.','The conciseness of the error message often determines whether a support case is resolved in minutes or only after hours.','ایجاز و دقت پیام خطا اغلب تعیین می‌کند که پرونده پشتیبانی در چند دقیقه حل شود یا تازه بعد از چند ساعت.','Kurtebûn û zelaliya peyama çewtiyê gelek caran diyar dike ka doza piştgiriyê di çend xulekên de çareser dibe an piştî saetan.','Zwięzłość komunikatu błędu często decyduje o tym, czy sprawa wsparcia zostanie rozwiązana w kilka minut, czy dopiero po godzinach.','Concizia mesajului de eroare decide adesea dacă un caz de suport se rezolvă în câteva minute sau abia după ore.','Лаконичность сообщения об ошибке часто определяет, будет ли обращение в поддержку решено за минуты или только через несколько часов.','Përpikëria dhe shkurtësia e mesazhit të gabimit shpesh vendos nëse rasti i suportit zgjidhet brenda minutash apo pas orësh.','Hata mesajının özlülüğü, destek vakasının dakikalar içinde mi yoksa saatler sonra mı çözüleceğini çoğu zaman belirler.')),
      ex('Der letzte Absatz gewinnt an Prägnanz, weil er die vorher verstreuten Motive in einem einzigen Bild bündelt.', trans('تزداد الفقرة الأخيرة قوة وإيجازاً لأنها تجمع الدوافع المتفرقة سابقاً في صورة واحدة.','پاراگرافی کۆتایی کورت و کاریگەرتر دەبێت، چونکە مۆتیفە پێشتر پەرتەوازەکان لە یەک وێنەدا کۆدەکاتەوە.','The final paragraph gains conciseness and force because it gathers the previously scattered motifs into a single image.','بند آخر از ایجاز و اثرگذاری بیشتری برخوردار می‌شود، چون موتیف‌های پراکنده قبلی را در یک تصویر واحد جمع می‌کند.','Paragrafa dawî bi kurtebûn û hêz zêde dibe, ji ber ku motîfên berê belavbûyî di wêneyekê de dicivîne.','Ostatni akapit zyskuje na wyrazistości, ponieważ skupia wcześniej rozproszone motywy w jednym obrazie.','Ultimul paragraf câștigă în pregnanță deoarece adună motivele anterior dispersate într-o singură imagine.','Последний абзац приобретает выразительность, потому что собирает ранее рассеянные мотивы в один образ.','Paragrafi i fundit fiton përpikëri shprehëse, sepse mbledh motivet e shpërndara më parë në një imazh të vetëm.','Son paragraf, daha önce dağınık duran motifleri tek bir imgede topladığı için çarpıcılık kazanır.'))
    ]
  }),
  entry({
    word: 'prätentiös', partOfSpeech: 'Adjective', syllableBreak: 'prä-ten-ti-ös',
    topics: ['business-communication','culture-and-media','social-and-relationships'], usageLabels: ['formal','written','advanced','sensitive'],
    collocations: [{ text: 'prätentiös wirken', meaning: 'to come across as pretentious' }],
    meanings: meaning('متكلف؛ متظاهر بالعمق','خۆنمایانە؛ بە زۆر خۆ گەورە نیشاندان','pretentious; affected','متظاهرانه؛ پرمدعا','xwe-mezin-nîşandan; pretensîyoz','pretensjonalny','pretențios; afectat','претенциозный','pretencioz; i shtirur','iddialı ve yapmacık; pretansiyöz'),
    examples: [
      ex('Das Konzeptpapier wirkte prätentiös, weil es einfache Prozessprobleme hinter unnötig abstrakten Begriffen versteckte.', trans('بدا ورق المفهوم متكلفاً لأنه أخفى مشكلات عملية بسيطة خلف مصطلحات مجردة لا داعي لها.','بەڵگەنامەی کۆنسێپتەکە خۆنمایانە دەردەکەوت، چونکە کێشە سادەکانی پڕۆسەی لە پشت چەمکی ئەبستراکتی بێپێویست شارەوە.','The concept paper came across as pretentious because it hid simple process problems behind unnecessarily abstract terms.','سند مفهومی متظاهرانه به نظر می‌رسید، چون مشکلات ساده فرایندی را پشت اصطلاحات انتزاعی غیرضروری پنهان کرده بود.','Belgeya konseptê pretensîyoz xuya kir, ji ber ku pirsgirêkên sade yên pêvajoyê li pişt têgehên razber ên nepêwîst veşartibûn.','Dokument koncepcyjny sprawiał wrażenie pretensjonalnego, bo ukrywał proste problemy procesowe za niepotrzebnie abstrakcyjnymi pojęciami.','Documentul conceptual părea pretențios deoarece ascundea probleme simple de proces în spatele unor termeni inutil de abstracți.','Концептуальный документ выглядел претенциозно, потому что скрывал простые процессные проблемы за излишне абстрактными терминами.','Dokumenti konceptual dukej pretencioz, sepse fshihte probleme të thjeshta procesi pas termave panevojshëm abstraktë.','Konsept dokümanı, basit süreç sorunlarını gereksiz soyut kavramların arkasına sakladığı için pretansiyöz görünüyordu.')),
      ex('Der Film ist nicht deshalb schwierig, weil er komplex wäre, sondern weil seine Symbolik oft prätentiös eingesetzt wird.', trans('الفيلم ليس صعباً لأنه معقد، بل لأن رموزه تُستخدم غالباً بتكلف.','فیلمەکە قورس نییە چونکە ئاڵۆزە، بەڵکو چونکە هێماکانی زۆرجار بە خۆنمایی بەکاردێن.','The film is difficult not because it is complex, but because its symbolism is often used pretentiously.','فیلم نه به این دلیل دشوار است که پیچیده باشد، بلکه چون نمادهایش اغلب متظاهرانه به کار می‌روند.','Fîlm ne ji ber ku tevlihev e dijwar e, lê ji ber ku sembolîzma wê gelek caran pretensîyoz tê bikaranîn.','Film jest trudny nie dlatego, że jest złożony, lecz dlatego, że jego symbolika bywa używana pretensjonalnie.','Filmul nu este dificil pentru că ar fi complex, ci pentru că simbolistica lui este adesea folosită pretențios.','Фильм труден не потому, что он сложен, а потому что его символика часто используется претенциозно.','Filmi nuk është i vështirë sepse është kompleks, por sepse simbolika e tij përdoret shpesh në mënyrë pretencioze.','Film karmaşık olduğu için değil, sembolizmi çoğu zaman pretansiyöz biçimde kullanıldığı için zordur.'))
    ]
  }),
  entry({
    word: 'die Pragmalinguistik', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Prag-ma-lin-gu-is-tik',
    topics: ['education-and-training','advanced-analysis','business-communication'], usageLabels: ['formal','written','academic','analysis'],
    grammarNotes: ['feminine academic field name; normally used in singular'],
    collocations: [{ text: 'aus pragmalinguistischer Sicht', meaning: 'from a pragmalinguistic perspective' }],
    meanings: meaning('البراغمالسانيات؛ دراسة اللغة في الاستعمال','پراگمالینگوستیک؛ لێکۆڵینەوەی زمان لە بەکارهێناندا','pragmalinguistics; study of language use','کاربردشناسی زبان؛ پراگمالینگوستیک','pragmalinguîstîk; lêkolîna bikaranîna ziman','pragmalingwistyka','pragmalingvistică','прагмалингвистика','pragmalinguistikë','pragmadilbilim'),
    examples: [
      ex('Die Pragmalinguistik erklärt, warum dieselbe höfliche Formulierung im Support-Chat beruhigend, in einer Mahnung aber ausweichend wirken kann.', trans('توضح البراغمالسانيات لماذا يمكن للعبارة المهذبة نفسها أن تبدو مطمئنة في دردشة الدعم ومراوغة في إنذار دفع.','پراگمالینگوستیک ڕوون دەکاتەوە بۆچی هەمان دەربڕینی ڕێزدار دەتوانێت لە چاتی پشتگیریدا دڵنیایی بدات، بەڵام لە ئاگادارکردنەوەی قەرزدا وەک خۆدزینەوە دەربکەوێت.','Pragmalinguistics explains why the same polite formulation can sound reassuring in a support chat but evasive in a payment reminder.','کاربردشناسی زبان توضیح می‌دهد چرا یک عبارت مؤدبانه یکسان در چت پشتیبانی آرام‌بخش، اما در اخطار پرداخت طفره‌آمیز به نظر می‌رسد.','Pragmalinguîstîk rave dike ka çima heman gotina nezaketî di chata piştgiriyê de aramker, lê di hişyariya pere-danê de wek dûrketin xuya dike.','Pragmalingwistyka wyjaśnia, dlaczego ta sama uprzejma formuła w czacie wsparcia może uspokajać, a w wezwaniu do zapłaty brzmieć wymijająco.','Pragmalingvistica explică de ce aceeași formulare politicoasă poate suna liniștitor într-un chat de suport, dar evaziv într-o somație de plată.','Прагмалингвистика объясняет, почему одна и та же вежливая формулировка в чате поддержки может успокаивать, а в напоминании об оплате звучать уклончиво.','Pragmalinguistika shpjegon pse e njëjta formulë e sjellshme mund të tingëllojë qetësuese në një chat suporti, por shmangëse në një kujtesë pagese.','Pragmadilbilim, aynı nazik ifadenin destek sohbetinde yatıştırıcı, ödeme hatırlatmasında ise kaçamak duyulabileceğini açıklar.')),
      ex('Im Forschungsprojekt verbindet die Pragmalinguistik Gesprächsanalyse mit der Frage, wie institutionelle Rollen sprachlich hergestellt werden.', trans('في المشروع البحثي تربط البراغمالسانيات تحليل المحادثة بسؤال كيفية إنتاج الأدوار المؤسسية لغوياً.','لە پڕۆژەی توێژینەوەکەدا پراگمالینگوستیک شیکاری گفتوگۆ بە پرسی ئەوەوە دەبەستێتەوە کە ڕۆڵە دامەزراوەییەکان چۆن بە زمانی دروست دەکرێن.','In the research project, pragmalinguistics links conversation analysis with the question of how institutional roles are produced through language.','در پروژه پژوهشی، پراگمالینگوستیک تحلیل گفتگو را با این پرسش پیوند می‌دهد که نقش‌های نهادی چگونه زبانی ساخته می‌شوند.','Di projeya lêkolînê de pragmalinguîstîk analîza axaftinê bi pirsa çawa rolên sazûmanî bi ziman têne çêkirin ve girê dide.','W projekcie badawczym pragmalingwistyka łączy analizę rozmowy z pytaniem, jak role instytucjonalne są wytwarzane językowo.','În proiectul de cercetare, pragmalingvistica leagă analiza conversației de întrebarea cum sunt produse lingvistic rolurile instituționale.','В исследовательском проекте прагмалингвистика связывает анализ разговора с вопросом о том, как институциональные роли создаются языковыми средствами.','Në projektin kërkimor, pragmalinguistika lidh analizën e bisedës me pyetjen se si prodhohen gjuhësisht rolet institucionale.','Araştırma projesinde pragmadilbilim, konuşma analizini kurumsal rollerin dilsel olarak nasıl üretildiği sorusuyla ilişkilendirir.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 065', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');

const cmd = `dotnet run --project "${path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj')}" -- --target shared --yes "${outPath}"`;
const result = cp.spawnSync(cmd, { shell: true, encoding: 'utf8', cwd: root });
const output = `${result.stdout || ''}${result.stderr || ''}`;
process.stdout.write(output);
const ok = result.status === 0 && output.includes('Entries imported: 6') && output.includes('Entries invalid: 0') && output.includes('Warnings: 0');
if (!ok) {
  const failedPath = path.join(root, 'content', 'generated', `${levelLower}-failed-words.txt`);
  fs.appendFileSync(failedPath, `${new Date().toISOString()}\tbatch-${batch}\t${expected.join(', ')}\n`, 'utf8');
  throw new Error('Import did not meet strict success criteria; source not modified.');
}
const remaining = tokens.slice(expected.length);
fs.writeFileSync(sourcePath, remaining.join(', ') + (remaining.length ? '\n' : ''), 'utf8');
console.log(`SOURCE_UPDATED: yes`);
console.log(`SOURCE_FILE: ${sourcePath}`);
console.log(`JSON_FILE: ${outPath}`);
console.log(`PROCESSED: ${expected.join(' | ')}`);
console.log(`REMAINING_COUNT: ${remaining.length}`);
console.log(`FIRST_10_REMAINING: ${remaining.slice(0, 10).join(' | ')}`);

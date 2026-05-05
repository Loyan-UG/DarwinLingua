const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '060';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['der Nexus','notorisch','nuanciert','obliegen','offenbaren','die Offenbarung'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const topicKeys = new Set((taxonomy.topics || []).map(t => t.key));
const labelsByKey = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const source = fs.readFileSync(sourcePath, 'utf8');
const tokens = source.split(',').map(s => s.trim()).filter(Boolean);
const first = tokens.slice(0, 6);
if (JSON.stringify(first) !== JSON.stringify(expected)) {
  throw new Error(`Unexpected first tokens: ${JSON.stringify(first)}`);
}

function meaning(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) {
  return [ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr].map((text, i) => ({ language: langs[i], text }));
}
function trans(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) { return meaning(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr); }
function ex(baseText, arr) { return { baseText, translations: arr }; }
function entry(e) {
  for (const t of e.topics) if (!topicKeys.has(t)) throw new Error(`Unknown topic ${t}`);
  for (const l of [...(e.usageLabels || []), ...(e.contextLabels || [])]) if (!labelsByKey.has(l)) throw new Error(`Unknown label ${l}`);
  return Object.assign({
    language: 'de', cefrLevel: level, article: null, plural: null, infinitive: null,
    pronunciationIpa: null, syllableBreak: null, contextLabels: [], grammarNotes: [],
    collocations: [], wordFamilies: [], relations: []
  }, e);
}

const entries = [
  entry({
    word: 'der Nexus', partOfSpeech: 'Noun', article: 'der', plural: 'Nexus', syllableBreak: 'Ne-xus',
    topics: ['advanced-analysis','business-communication','culture-and-media'],
    usageLabels: ['formal','written','academic','analysis'],
    grammarNotes: ['masculine noun; plural is often identical in academic usage'],
    collocations: [{ text: 'den Nexus zwischen etwas beschreiben', meaning: 'to describe the connection between things' }],
    wordFamilies: [{ lemma: 'vernetzen', relationLabel: 'related verb', note: null }],
    meanings: meaning('رابطة؛ صلة معقدة','پەیوەندیی ئاڵۆز؛ گرێدەر','nexus; complex connection','پیوند؛ ارتباط پیچیده','girêdaneke tevlihev; nexus','związek; powiązanie','nexus; legătură complexă','связь; сложное соотношение','lidhje komplekse; nyje lidhëse','bağlantı; karmaşık ilişki'),
    examples: [
      ex('Der Bericht beschreibt den Nexus zwischen technischen Schulden, Budgetdruck und sinkender Servicequalität.', trans('يصف التقرير الصلة بين الديون التقنية وضغط الميزانية وتراجع جودة الخدمة.','ڕاپۆرتەکە پەیوەندیی نێوان قەرزی تەکنیکی، گوشاری بودجە و دابەزینی کوالیتی خزمەتگوزاری دەخاتە ڕوو.','The report describes the nexus between technical debt, budget pressure, and declining service quality.','گزارش پیوند میان بدهی فنی، فشار بودجه و کاهش کیفیت خدمات را توضیح می‌دهد.','Rapor girêdana di navbera deynê teknîkî, zexta budceyê û daketina kalîteya xizmetê de vedibêje.','Raport opisuje związek między długiem technicznym, presją budżetową i spadkiem jakości usług.','Raportul descrie legătura dintre datoria tehnică, presiunea bugetară și scăderea calității serviciilor.','В отчете описана связь между техническим долгом, бюджетным давлением и снижением качества сервиса.','Raporti përshkruan lidhjen mes borxhit teknik, presionit buxhetor dhe rënies së cilësisë së shërbimit.','Rapor, teknik borç, bütçe baskısı ve düşen hizmet kalitesi arasındaki bağlantıyı açıklıyor.')),
      ex('Im Roman bildet der Nexus von Erinnerung und Schuld den eigentlichen Kern der Handlung.', trans('في الرواية تشكل الصلة بين الذاكرة والذنب الجوهر الحقيقي للحبكة.','لە ڕۆمانەکەدا پەیوەندیی نێوان بیرەوەری و تاوان ناوەڕۆکی ڕاستەقینەی ڕووداوەکان پێکدەهێنێت.','In the novel, the nexus of memory and guilt forms the real core of the plot.','در رمان، پیوند میان خاطره و گناه هسته واقعی داستان را شکل می‌دهد.','Di romanê de, girêdana bîranînê û sûcê navenda rastîn a çîrokê pêk tîne.','W powieści związek pamięci i winy stanowi właściwy rdzeń fabuły.','În roman, legătura dintre memorie și vină formează adevăratul nucleu al acțiunii.','В романе связь памяти и вины образует подлинное ядро сюжета.','Në roman, lidhja mes kujtesës dhe fajit përbën bërthamën e vërtetë të ngjarjes.','Romanda hafıza ile suçluluk arasındaki bağlantı olay örgüsünün asıl çekirdeğini oluşturur.'))
    ]
  }),
  entry({
    word: 'notorisch', partOfSpeech: 'Adjective', syllableBreak: 'no-to-risch',
    topics: ['business-communication','quality-and-risk','social-and-relationships'],
    usageLabels: ['formal','written','advanced','sensitive'],
    collocations: [{ text: 'notorisch unzuverlässig', meaning: 'notoriously unreliable' }],
    meanings: meaning('سيئ السمعة؛ معروف عادةً بسلوك سلبي','بە ناوی خراپ ناسراو؛ بەردەوام بە شتێکی نەرێنی ناسراو','notorious; habitually known for something negative','بدنام؛ مشهور به رفتاری منفی و تکراری','bi navê xerab nasîn; bi tiştekî neyînî navdar','notoryczny; znany z czegoś negatywnego','notoriu; cunoscut în mod negativ','печально известный; отъявленный','famëkeq; i njohur për diçka negative','adı çıkmış; kötü şöhretli'),
    examples: [
      ex('Der Lieferant war notorisch unzuverlässig, trotzdem wurde der Vertrag aus Zeitdruck verlängert.', trans('كان المورد معروفاً بعدم موثوقيته، ومع ذلك مُدّد العقد بسبب ضغط الوقت.','دابینکەرەکە بە نەبوونی متمانەپێکراوی بەردەوام ناسراو بوو، بەڵام لەبەر گوشاری کات گرێبەستەکە درێژکرایەوە.','The supplier was notoriously unreliable, yet the contract was extended because of time pressure.','تأمین‌کننده به بدقولی و غیرقابل‌اعتماد بودن معروف بود، با این حال قرارداد به دلیل کمبود وقت تمدید شد.','Dabînker bi bêbawerîya xwe navdar bû, lêbelê ji ber tengiya demê peyman hat dirêjkirin.','Dostawca był notorycznie nierzetelny, mimo to przedłużono umowę z powodu presji czasu.','Furnizorul era notoriu de nesigur, totuși contractul a fost prelungit din cauza presiunii timpului.','Поставщик был печально известен своей ненадежностью, но договор продлили из-за нехватки времени.','Furnizuesi ishte famëkeq për mungesë besueshmërie, megjithatë kontrata u zgjat për shkak të presionit të kohës.','Tedarikçi güvenilmezliğiyle nam salmıştı, buna rağmen zaman baskısı nedeniyle sözleşme uzatıldı.')),
      ex('Die Figur ist notorisch misstrauisch, was jede Nähe sofort in Verdacht verwandelt.', trans('الشخصية معروفة بارتيابها الدائم، وهذا يحوّل كل قرب فوراً إلى موضع شك.','کارەکتەرەکە بە گومانی بەردەوام ناسراوە، ئەمەش هەر نزیکبوونەوەیەک دەکاتە جێی گومان.','The character is notoriously mistrustful, turning any closeness immediately into suspicion.','این شخصیت به بدگمانی همیشگی معروف است و هر نزدیکی را فوراً به سوءظن تبدیل می‌کند.','Kesayet bi bêbaweriya xwe ya domdar navdar e û her nêzîkbûnê tavilê dike guman.','Postać jest notorycznie nieufna, przez co każda bliskość natychmiast budzi podejrzenie.','Personajul este notoriu de suspicios, transformând orice apropiere imediat în bănuială.','Персонаж печально известен своей подозрительностью, из-за чего любая близость сразу превращается в повод для недоверия.','Personazhi është famëkeq për mosbesimin e tij, duke e kthyer çdo afërsi menjëherë në dyshim.','Karakter sürekli güvensizliğiyle bilinir; bu da her yakınlığı hemen şüpheye dönüştürür.'))
    ]
  }),
  entry({
    word: 'nuanciert', partOfSpeech: 'Adjective', syllableBreak: 'nu-an-ciert',
    topics: ['advanced-analysis','business-communication','culture-and-media'],
    usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'eine nuancierte Einschätzung', meaning: 'a nuanced assessment' }],
    wordFamilies: [{ lemma: 'die Nuance', relationLabel: 'noun', note: null }],
    meanings: meaning('دقيق الفروق؛ متوازن التفاصيل','وردەکارییەکان لەبەرچاوگرتوو؛ جیاوازیکراو','nuanced; differentiated','ظریف و دقیق؛ دارای تفکیک‌های معنایی','bi hûrgulî; cuda kirî','zniuansowany; wyważony','nuanțat; diferențiat','нюансированный; дифференцированный','i nuancuar; i diferencuar','nüanslı; ayrıntılı ve dengeli'),
    examples: [
      ex('Die Analyse war nuanciert genug, um technische Ursachen von organisatorischen Fehlanreizen zu trennen.', trans('كان التحليل دقيقاً بما يكفي ليفصل الأسباب التقنية عن الحوافز التنظيمية الخاطئة.','شیکردنەوەکە ئەوەندە ورد بوو کە هۆکارە تەکنیکییەکان لە پاڵنەرە ڕێکخراوەییە هەڵەکان جیا بکاتەوە.','The analysis was nuanced enough to separate technical causes from organizational misincentives.','تحلیل آن‌قدر دقیق و ظریف بود که علت‌های فنی را از مشوق‌های سازمانی نادرست جدا کند.','Analîz ewqas hûrgulî bû ku sedemên teknîkî ji teşwîqên şaş ên rêxistinî cuda bike.','Analiza była wystarczająco zniuansowana, by oddzielić przyczyny techniczne od błędnych bodźców organizacyjnych.','Analiza a fost suficient de nuanțată pentru a separa cauzele tehnice de stimulentele organizaționale greșite.','Анализ был достаточно нюансированным, чтобы отделить технические причины от ошибочных организационных стимулов.','Analiza ishte mjaftueshëm e nuancuar për të ndarë shkaqet teknike nga nxitjet e gabuara organizative.','Analiz, teknik nedenleri hatalı organizasyonel teşviklerden ayıracak kadar nüanslıydı.')),
      ex('Der Film zeichnet die Gegnerin nuanciert, ohne ihre Verantwortung zu relativieren.', trans('يرسم الفيلم صورة دقيقة للخصمة من دون التقليل من مسؤوليتها.','فیلمەکە نەیارەکە بە وردی پیشان دەدات بەبێ ئەوەی بەرپرسیارێتییەکەی کەم بکاتەوە.','The film portrays the antagonist in a nuanced way without relativizing her responsibility.','فیلم شخصیت مخالف را با ظرافت نشان می‌دهد، بدون آن‌که مسئولیت او را کم‌اهمیت جلوه دهد.','Fîlm dijberê bi awayekî hûrgulî nîşan dide bê ku berpirsiyariya wê kêm bike.','Film przedstawia przeciwniczkę w sposób zniuansowany, nie umniejszając jej odpowiedzialności.','Filmul o prezintă pe adversară într-un mod nuanțat, fără a-i relativiza responsabilitatea.','Фильм нюансированно изображает противницу, не преуменьшая ее ответственности.','Filmi e paraqet kundërshtaren në mënyrë të nuancuar, pa relativizuar përgjegjësinë e saj.','Film, karşıt karakteri sorumluluğunu hafifletmeden nüanslı biçimde betimliyor.'))
    ]
  }),
  entry({
    word: 'obliegen', partOfSpeech: 'Verb', infinitive: 'obliegen', syllableBreak: 'ob-lie-gen',
    topics: ['law-and-compliance','documents-and-administration','business-communication'],
    usageLabels: ['formal','written','administrative','business'],
    collocations: [{ text: 'jemandem obliegt etwas', meaning: 'something is someone’s responsibility' }],
    grammarNotes: ['formal verb, often used with dative'],
    meanings: meaning('يقع ضمن مسؤولية؛ يكون من واجب','لە ئەستۆی کەسێکدا بوون؛ بەرپرسیاری بوون','to be incumbent on; to be the responsibility of','بر عهده بودن؛ وظیفه کسی بودن','li ser milê kesekî bûn; berpirsiyarîya kesekî bûn','spoczywać na kimś; należeć do obowiązków','a reveni cuiva; a fi responsabilitatea cuiva','лежать на ком-либо как обязанность; входить в обязанности','i takon dikujt si detyrë; është përgjegjësi e dikujt','birinin sorumluluğunda olmak; görev olarak düşmek'),
    examples: [
      ex('Die abschließende Prüfung obliegt der Datenschutzbeauftragten, nicht dem Projektteam.', trans('تقع المراجعة النهائية على عاتق مسؤولة حماية البيانات، وليس فريق المشروع.','پشکنینی کۆتایی لە ئەستۆی بەرپرسی پاراستنی داتایە، نە تیمی پڕۆژەکە.','The final review is the responsibility of the data protection officer, not the project team.','بررسی نهایی بر عهده مسئول حفاظت از داده‌هاست، نه تیم پروژه.','Kontrola dawî li ser milê berpirsê parastina daneyan e, ne tîma projeyê.','Końcowa kontrola należy do inspektorki ochrony danych, a nie do zespołu projektowego.','Verificarea finală îi revine responsabilului cu protecția datelor, nu echipei de proiect.','Окончательная проверка возлагается на специалиста по защите данных, а не на проектную команду.','Kontrolli përfundimtar i takon përgjegjëses për mbrojtjen e të dhënave, jo ekipit të projektit.','Son inceleme proje ekibinin değil, veri koruma sorumlusunun görevidir.')),
      ex('In diesem Verfahren obliegt es dem Antragsteller, die fehlenden Nachweise fristgerecht einzureichen.', trans('في هذا الإجراء يقع على مقدم الطلب تقديم المستندات الناقصة في الموعد المحدد.','لەو ڕێکارەدا لە ئەستۆی داواکەرە کە بەڵگەنامە کەمەکان لە کاتی دیاریکراودا پێشکەش بکات.','In this procedure, it is the applicant’s responsibility to submit the missing documents on time.','در این روند، ارائه مدارک ناقص در مهلت مقرر بر عهده متقاضی است.','Di vê pêvajoyê de erkê daxwazkar e ku belgeyên kêm di dema xwe de radest bike.','W tym postępowaniu to na wnioskodawcy spoczywa obowiązek terminowego złożenia brakujących dokumentów.','În această procedură, solicitantului îi revine obligația de a depune la timp dovezile lipsă.','В этой процедуре заявитель обязан своевременно представить недостающие документы.','Në këtë procedurë, aplikuesit i takon të dorëzojë provat që mungojnë brenda afatit.','Bu süreçte eksik belgeleri süresinde sunmak başvuru sahibinin sorumluluğundadır.'))
    ]
  }),
  entry({
    word: 'offenbaren', partOfSpeech: 'Verb', infinitive: 'offenbaren', syllableBreak: 'of-fen-ba-ren',
    topics: ['advanced-analysis','business-communication','culture-and-media'],
    usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'Schwächen offenbaren', meaning: 'to reveal weaknesses' }],
    wordFamilies: [{ lemma: 'die Offenbarung', relationLabel: 'noun', note: null }],
    meanings: meaning('يكشف؛ يظهر بوضوح','ئاشکراکردن؛ دەرخستن','to reveal; to disclose','آشکار کردن؛ نمایان ساختن','eşkere kirin; derxistin holê','ujawniać; odsłaniać','a dezvălui; a scoate la iveală','раскрывать; выявлять','zbuloj; nxjerr në pah','ortaya çıkarmak; açığa vurmak'),
    examples: [
      ex('Die Auswertung offenbarte, dass die meisten Verzögerungen nicht im Code, sondern in den Freigaben entstanden.', trans('كشف التحليل أن معظم التأخيرات لم تنشأ في الكود بل في عمليات الموافقة.','شیکردنەوەکە ئاشکرای کرد کە زۆربەی دواخستنەکان نە لە کۆدەکە، بەڵکو لە ڕێپێدانەکاندا دروست بوون.','The analysis revealed that most delays arose not in the code but in the approval steps.','تحلیل نشان داد که بیشتر تأخیرها نه در کد، بلکه در فرایندهای تأیید ایجاد شده‌اند.','Analîz eşkere kir ku piraniya derengiyan ne di kodê de, lê di pejirandinan de derketine.','Analiza ujawniła, że większość opóźnień powstała nie w kodzie, lecz na etapach zatwierdzania.','Analiza a dezvăluit că majoritatea întârzierilor au apărut nu în cod, ci în etapele de aprobare.','Анализ показал, что большинство задержек возникло не в коде, а на этапах согласования.','Analiza zbuloi se shumica e vonesave nuk lindën në kod, por në hapat e miratimit.','Analiz, gecikmelerin çoğunun kodda değil onay süreçlerinde oluştuğunu ortaya çıkardı.')),
      ex('Der Brief offenbart eine Verletzlichkeit, die der öffentliche Auftritt der Autorin kaum ahnen lässt.', trans('تكشف الرسالة هشاشة لا يكاد الظهور العام للكاتبة يوحي بها.','نامەکە لاوازییەک ئاشکرا دەکات کە دەرکەوتنی گشتی نووسەرەکە بە زەحمەت نیشانی دەدات.','The letter reveals a vulnerability that the author’s public appearance barely suggests.','نامه آسیب‌پذیری‌ای را آشکار می‌کند که حضور عمومی نویسنده کمتر نشانی از آن دارد.','Name lawaziyekê eşkere dike ku xuyabûna giştî ya nivîskarê wê kêm nîşan dide.','List ujawnia wrażliwość, której publiczny wizerunek autorki prawie nie pozwala się domyślić.','Scrisoarea dezvăluie o vulnerabilitate pe care aparițiile publice ale autoarei abia o lasă să se întrevadă.','Письмо раскрывает уязвимость, о которой публичный образ авторки почти не позволяет догадаться.','Letra zbulon një brishtësi që paraqitja publike e autores mezi e lë të kuptohet.','Mektup, yazarın kamusal görünümünün pek sezdirmediği bir kırılganlığı açığa çıkarıyor.'))
    ]
  }),
  entry({
    word: 'die Offenbarung', partOfSpeech: 'Noun', article: 'die', plural: 'Offenbarungen', syllableBreak: 'Of-fen-ba-rung',
    topics: ['culture-and-media','advanced-analysis','social-and-relationships'],
    usageLabels: ['formal','written','advanced','analysis'],
    collocations: [{ text: 'eine späte Offenbarung', meaning: 'a late revelation' }],
    wordFamilies: [{ lemma: 'offenbaren', relationLabel: 'verb', note: null }],
    meanings: meaning('كشف؛ إعلان مفاجئ','ئاشکراکردن؛ دەرکەوتنی ناوەڕۆک','revelation; disclosure','آشکارسازی؛ مکاشفه','eşkerekirin; eşkerebûn','objawienie; ujawnienie','revelație; dezvăluire','откровение; раскрытие','zbulim; revelim','açığa çıkış; vahiy; ifşa'),
    examples: [
      ex('Die späte Offenbarung des Motivs verändert rückwirkend die Lesart der ersten Kapitel.', trans('إن الكشف المتأخر عن الدافع يغيّر بأثر رجعي قراءة الفصول الأولى.','ئاشکراکردنی درەنگی پاڵنەرەکە بە شێوەیەکی گەڕاوە خوێندنەوەی بەشە سەرەتاییەکان دەگۆڕێت.','The late revelation of the motive retrospectively changes how the first chapters are read.','آشکار شدن دیرهنگام انگیزه، خوانش فصل‌های نخست را به‌صورت پس‌نگر تغییر می‌دهد.','Eşkerekirina dereng a armancê bi paşve xwendina beşên yekem diguhezîne.','Późne ujawnienie motywu retrospektywnie zmienia odczytanie pierwszych rozdziałów.','Revelația târzie a motivului schimbă retrospectiv lectura primelor capitole.','Позднее раскрытие мотива задним числом меняет прочтение первых глав.','Zbulimi i vonë i motivit ndryshon në mënyrë retrospektive leximin e kapitujve të parë.','Motifin geç ortaya çıkması, ilk bölümlerin geriye dönük okunuşunu değiştirir.')),
      ex('Für das Team war die ehrliche Fehleranalyse keine Blamage, sondern eine notwendige Offenbarung.', trans('بالنسبة إلى الفريق لم يكن تحليل الأخطاء الصادق فضيحة، بل كشفاً ضرورياً.','بۆ تیمەکە شیکردنەوەی ڕاستگۆیانەی هەڵەکان شەرمەزاری نەبوو، بەڵکو ئاشکراکردنێکی پێویست بوو.','For the team, the honest error analysis was not an embarrassment but a necessary revelation.','برای تیم، تحلیل صادقانه خطاها مایه شرمندگی نبود، بلکه آشکارسازی ضروری بود.','Ji bo tîmê, analîza şaşiyan a rastgoyane ne şerm bû, lê eşkerekirineke pêwîst bû.','Dla zespołu szczera analiza błędów nie była kompromitacją, lecz koniecznym ujawnieniem problemu.','Pentru echipă, analiza onestă a erorilor nu a fost o rușine, ci o dezvăluire necesară.','Для команды честный анализ ошибок был не позором, а необходимым откровением.','Për ekipin, analiza e sinqertë e gabimeve nuk ishte turp, por një zbulim i nevojshëm.','Ekip için dürüst hata analizi bir rezalet değil, gerekli bir açığa çıkarma süreciydi.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = {
  packageVersion: '1.0', packageId,
  packageName: 'German C2 Generated Batch 060', source: 'Hybrid',
  defaultMeaningLanguages: langs,
  labels: usedLabels.map(k => labelsByKey.get(k)),
  entries,
  collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: []
};
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


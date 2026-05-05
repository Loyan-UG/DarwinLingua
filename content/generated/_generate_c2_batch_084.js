const fs = require('fs');
const path = require('path');
const cp = require('child_process');
const root = 'D:/_Projects/DarwinLingua';
const level = 'C2', levelLower = 'c2', batch = '084';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['der Unterton','die Unterwanderung','die Unübersichtlichkeit','die Unverbrüchlichkeit','die Unverfügbarkeit','unverkennbar'];
const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const topicKeys = new Set((taxonomy.topics || []).map(t => t.key));
const labelsByKey = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const source = fs.readFileSync(sourcePath, 'utf8');
const tokens = source.split(',').map(s => s.trim()).filter(Boolean);
if (JSON.stringify(tokens.slice(0, 6)) !== JSON.stringify(expected)) throw new Error(`Unexpected first tokens: ${JSON.stringify(tokens.slice(0, 6))}`);
function m(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) { return [ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr].map((text, i) => ({ language: langs[i], text })); }
function ex(baseText, translations) { return { baseText, translations }; }
function entry(e) {
  for (const t of e.topics) if (!topicKeys.has(t)) throw new Error(`Unknown topic ${t}`);
  for (const l of [...(e.usageLabels || []), ...(e.contextLabels || [])]) if (!labelsByKey.has(l)) throw new Error(`Unknown label ${l}`);
  return Object.assign({ language: 'de', cefrLevel: level, article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: null, contextLabels: [], grammarNotes: [], collocations: [], wordFamilies: [], relations: [] }, e);
}
const entries = [
  entry({
    word: 'der Unterton', partOfSpeech: 'Noun', article: 'der', plural: 'Untertöne', syllableBreak: 'Un-ter-ton',
    topics: ['business-communication','social-and-relationships','culture-and-media'], usageLabels: ['formal','written','analysis','sensitive'],
    collocations: [{ text: 'ein kritischer Unterton', meaning: 'a critical undertone' }],
    meanings: m('نبرة ضمنية؛ إيحاء خفي','تۆنی شاراوە؛ مانای لاوەکی','undertone; implicit nuance','لحن پنهان؛ معنای ضمنی','tona veşartî; nîansa veşartî','podtekst; ton poboczny','subton; nuanță implicită','подтекст; оттенок','nënton; nuancë e fshehtë','alt ton; ima'),
    examples: [
      ex('In der Antwort des Kunden lag ein deutlicher Unterton von Misstrauen, obwohl er formal höflich blieb.', m('كان في رد العميل إيحاء واضح بعدم الثقة، رغم أنه بقي مهذباً رسمياً.','لە وەڵامی کڕیارەکەدا تۆنێکی ڕوونی بێمتمانەیی هەبوو، هەرچەندە بە فەرمی ڕێزدار مایەوە.','There was a clear undertone of mistrust in the customer’s reply, although it remained formally polite.','در پاسخ مشتری لحن پنهان بی‌اعتمادی کاملاً محسوس بود، هرچند از نظر رسمی مؤدبانه ماند.','Di bersiva xerîdar de tona bêbaweriyê ya zelal hebû, herçend ew bi fermî nezaketî ma.','W odpowiedzi klienta był wyraźny podtekst nieufności, choć formalnie pozostała uprzejma.','În răspunsul clientului exista un subton clar de neîncredere, deși formal a rămas politicos.','В ответе клиента был явный оттенок недоверия, хотя формально он оставался вежливым.','Në përgjigjen e klientit kishte një nënton të qartë mosbesimi, megjithëse formalisht mbeti i sjellshëm.','Müşterinin yanıtında belirgin bir güvensizlik alt tonu vardı, ancak biçimsel olarak nazik kaldı.')),
      ex('Der ironische Unterton der Erzählung verhindert, dass die Trauer sentimental wirkt.', m('تمنع النبرة الساخرة الخفية في السرد الحزن من أن يبدو عاطفياً مبتذلاً.','تۆنی ئیرۆنیای شاراوەی گێڕانەوەکە ڕێگری دەکات لەوەی خەمگینی هەستیارانەی سادە دەربکەوێت.','The ironic undertone of the narrative keeps the grief from seeming sentimental.','لحن پنهان طنزآمیز روایت مانع می‌شود اندوه احساساتی و سطحی به نظر برسد.','Tona ironîk a veşartî ya vegotinê nahêle ku xem sentimental xuya bike.','Ironiczny podtekst opowieści sprawia, że żałoba nie wydaje się sentymentalna.','Subtonul ironic al narațiunii împiedică durerea să pară sentimentală.','Иронический подтекст повествования не дает горю выглядеть сентиментальным.','Nëntoni ironik i rrëfimit e pengon pikëllimin të duket sentimental.','Anlatının ironik alt tonu, kederin duygusal ve basit görünmesini engeller.'))
    ]
  }),
  entry({
    word: 'die Unterwanderung', partOfSpeech: 'Noun', article: 'die', plural: 'Unterwanderungen', syllableBreak: 'Un-ter-wan-de-rung',
    topics: ['law-and-compliance','quality-and-risk','advanced-analysis'], usageLabels: ['formal','written','sensitive','analysis'],
    collocations: [{ text: 'die Unterwanderung von Kontrollmechanismen', meaning: 'the infiltration or undermining of control mechanisms' }],
    meanings: m('تسلل تقويضي؛ اختراق من الداخل','ژێرەوە تێکدان؛ چوونەناوەوە بۆ لاوازکردن','infiltration; undermining from within','نفوذ و تضعیف از درون','ji hundir ve têkdan; ketina nav ji bo lawazkirin','infiltracja; podkopywanie','infiltrare; subminare din interior','подрыв изнутри; инфильтрация','infiltrim; minimin nga brenda','içten sızma; altını oyma'),
    examples: [
      ex('Die interne Revision warnte vor einer schleichenden Unterwanderung der Freigabeprozesse durch informelle Absprachen.', m('حذرت المراجعة الداخلية من تقويض تدريجي لعمليات الموافقة عبر تفاهمات غير رسمية.','پشکنینی ناوخۆ ئاگاداری دا لە ژێرەوە تێکدانی هێواشی پڕۆسەکانی پەسەندکردن بە ڕێککەوتنی نافەرمی.','Internal audit warned of a gradual undermining of approval processes through informal arrangements.','حسابرسی داخلی درباره تضعیف تدریجی فرایندهای تأیید از طریق توافق‌های غیررسمی هشدار داد.','Kontrola hundirîn hişyarî da li ser binpêkirina hêdî ya pêvajoyên pejirandinê bi rêkeftinên nefermî.','Audyt wewnętrzny ostrzegł przed stopniowym podkopywaniem procesów zatwierdzania przez nieformalne ustalenia.','Auditul intern a avertizat asupra subminării treptate a proceselor de aprobare prin înțelegeri informale.','Внутренний аудит предупредил о постепенном подрыве процессов согласования через неформальные договоренности.','Auditimi i brendshëm paralajmëroi për minimin gradual të proceseve të miratimit përmes marrëveshjeve joformale.','İç denetim, gayriresmi anlaşmalar yoluyla onay süreçlerinin kademeli olarak altının oyulmasına karşı uyardı.')),
      ex('Der Roman beschreibt die Unterwanderung einer Familie durch Schweigen, nicht durch offene Feindschaft.', m('تصف الرواية تقويض عائلة بالصمت لا بالعداء المفتوح.','ڕۆمانەکە ژێرەوە تێکدانی خێزانێک بە بێدەنگی وەسف دەکات، نەک بە دوژمنایەتی ئاشکرا.','The novel describes the undermining of a family by silence, not by open hostility.','رمان تضعیف یک خانواده را از راه سکوت توصیف می‌کند، نه دشمنی آشکار.','Roman binpêkirina malbatekê bi bêdengiyê vedibêje, ne bi dijminatiya eşkere.','Powieść opisuje podkopywanie rodziny przez milczenie, a nie przez otwartą wrogość.','Romanul descrie subminarea unei familii prin tăcere, nu prin ostilitate deschisă.','Роман описывает подрыв семьи молчанием, а не открытой враждой.','Romani përshkruan minimin e një familjeje përmes heshtjes, jo armiqësisë së hapur.','Roman bir ailenin açık düşmanlıkla değil, sessizlikle içten içe aşındırılmasını anlatır.'))
    ]
  }),
  entry({
    word: 'die Unübersichtlichkeit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Un-ü-ber-sicht-lich-keit',
    topics: ['planning-and-projects','technology-and-it','advanced-analysis'], usageLabels: ['formal','written','analysis','business'], grammarNotes: ['feminine abstract noun; normally singular'],
    collocations: [{ text: 'organisatorische Unübersichtlichkeit', meaning: 'organizational lack of clarity' }],
    meanings: m('عدم الوضوح؛ صعوبة الإحاطة','ناڕوونی؛ سەختی تێگەیشتنی گشتی','lack of clarity; complexity hard to survey','پیچیدگی نامشخص؛ دشواری درک کلی','nezelalî; zehmetiya dîtina giştî','nieprzejrzystość','lipsă de claritate; necuprindere','необозримость; запутанность','paqartësi; vështirësi për t’u përmbledhur','karmaşıklık; anlaşılmazlık'),
    examples: [
      ex('Die Unübersichtlichkeit der Ticketlandschaft machte es fast unmöglich, Abhängigkeiten zwischen Bugs und Features zu erkennen.', m('جعل عدم وضوح مشهد التذاكر من شبه المستحيل معرفة التبعيات بين الأخطاء والميزات.','ناڕوونی دۆخی تیکەتەکان وای کرد نزیکەی نامومکین بێت پەیوەندی نێوان هەڵە و فیچەرەکان بناسرێت.','The lack of clarity in the ticket landscape made it almost impossible to recognize dependencies between bugs and features.','نامشخص‌بودن فضای تیکت‌ها تشخیص وابستگی میان باگ‌ها و قابلیت‌ها را تقریباً ناممکن کرد.','Nezelaliya qada ticketan kir ku nasîna girêdanên navbera bug û featurean hema ne gengaz be.','Nieprzejrzystość systemu ticketów niemal uniemożliwiała rozpoznanie zależności między błędami a funkcjami.','Lipsa de claritate a peisajului de tichete făcea aproape imposibilă recunoașterea dependențelor dintre buguri și funcționalități.','Запутанность системы тикетов делала почти невозможным распознавание зависимостей между багами и функциями.','Paqartësia e peizazhit të tiketave e bëri pothuajse të pamundur njohjen e varësive mes bug-eve dhe veçorive.','Ticket ortamının karmaşıklığı, hatalar ile özellikler arasındaki bağımlılıkları görmeyi neredeyse imkansız hale getirdi.')),
      ex('Die Unübersichtlichkeit der Stadt wird im Film als ästhetisches Prinzip genutzt.', m('تُستخدم صعوبة الإحاطة بالمدينة في الفيلم كمبدأ جمالي.','ناڕوونی شارەکە لە فیلمەکەدا وەک بنەمایەکی جوانکاری بەکاردێت.','The city’s lack of easy overview is used in the film as an aesthetic principle.','غیرقابل‌مرور بودن شهر در فیلم به‌عنوان اصل زیبایی‌شناختی به کار می‌رود.','Nezelaliya bajêr di fîlmê de wek prensîba estetîk tê bikaranîn.','Nieprzejrzystość miasta zostaje w filmie użyta jako zasada estetyczna.','Neclaritatea orașului este folosită în film ca principiu estetic.','Необозримость города используется в фильме как эстетический принцип.','Paqartësia e qytetit përdoret në film si parim estetik.','Şehrin karmaşıklığı filmde estetik bir ilke olarak kullanılır.'))
    ]
  }),
  entry({
    word: 'die Unverbrüchlichkeit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Un-ver-brüch-lich-keit',
    topics: ['law-and-compliance','social-and-relationships','advanced-analysis'], usageLabels: ['formal','written','advanced','analysis'], grammarNotes: ['feminine abstract noun; normally singular'],
    collocations: [{ text: 'die Unverbrüchlichkeit eines Versprechens', meaning: 'the inviolability of a promise' }],
    meanings: m('ثبات لا يُنقض؛ عدم قابلية الكسر','شکاندنەوەناپذیری؛ پابەندیی نەگۆڕ','inviolability; unbreakable firmness','نقض‌ناپذیری؛ استواری شکست‌ناپذیر','neşikestinbarî; pabendiya neguher','nienaruszalność','inviolabilitate','нерушимость','pathyeshmëri; paprekshmëri','bozulmazlık; dokunulmazlık'),
    examples: [
      ex('Die Unverbrüchlichkeit der Vertraulichkeit war für den Kunden wichtiger als jeder Preisnachlass.', m('كان عدم قابلية السرية للنقض أهم للعميل من أي تخفيض في السعر.','شکاندنەوەناپذیری نهێنیپارێزی بۆ کڕیار گرنگتر بوو لە هەر داشکاندنێکی نرخ.','The inviolability of confidentiality was more important to the customer than any discount.','نقض‌ناپذیری محرمانگی برای مشتری از هر تخفیفی مهم‌تر بود.','Neşikestinbarîya nihênîparastinê ji bo xerîdar ji her daxistina bihayê girîngtir bû.','Nienaruszalność poufności była dla klienta ważniejsza niż jakikolwiek rabat.','Inviolabilitatea confidențialității era mai importantă pentru client decât orice reducere.','Нерушимость конфиденциальности была для клиента важнее любой скидки.','Pathyeshmëria e konfidencialitetit ishte më e rëndësishme për klientin se çdo zbritje.','Gizliliğin bozulmazlığı müşteri için her indirimden daha önemliydi.')),
      ex('Der Text stellt die Unverbrüchlichkeit der Freundschaft infrage, ohne sie vollständig zu verneinen.', m('يشكك النص في ثبات الصداقة الذي لا يُنقض من دون أن ينفيها تماماً.','دەقەکە پرسیار لە شکاندنەوەناپذیری هاوڕێیەتی دەکات، بەبێ ئەوەی تەواو نکۆڵی لێ بکات.','The text questions the unbreakable firmness of friendship without denying it completely.','متن نقض‌ناپذیری دوستی را زیر سؤال می‌برد، بدون آن‌که کاملاً انکارش کند.','Nivîs neşikestinbarîya hevaltîyê dipirse bê ku wê bi tevahî înkar bike.','Tekst kwestionuje nienaruszalność przyjaźni, nie negując jej całkowicie.','Textul pune sub semnul întrebării inviolabilitatea prieteniei fără să o nege complet.','Текст ставит под вопрос нерушимость дружбы, не отрицая ее полностью.','Teksti vë në dyshim pathyeshmërinë e miqësisë pa e mohuar plotësisht.','Metin dostluğun bozulmazlığını tamamen reddetmeden sorgular.'))
    ]
  }),
  entry({
    word: 'die Unverfügbarkeit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Un-ver-füg-bar-keit',
    topics: ['technology-and-it','advanced-analysis','culture-and-media'], usageLabels: ['formal','written','analysis','academic'], grammarNotes: ['feminine abstract noun; normally singular'],
    collocations: [{ text: 'die Unverfügbarkeit eines Dienstes', meaning: 'the unavailability of a service' }],
    meanings: m('عدم التوافر؛ عدم قابلية التصرف','بەردەستنەبوون؛ دەستپێنەگەیشتن','unavailability; non-disposability','دردسترس‌نبودن؛ دراختیارنبودن','neberdestbûn; nekarîna destgirtinê','niedostępność','indisponibilitate','недоступность','padisponueshmëri','erişilemezlik; kullanılamazlık'),
    examples: [
      ex('Die Unverfügbarkeit des Authentifizierungsdienstes legte alle Kundenportale gleichzeitig lahm.', m('أدى عدم توافر خدمة المصادقة إلى تعطيل جميع بوابات العملاء في الوقت نفسه.','بەردەستنەبوونی خزمەتی پشتڕاستکردنەوە هەموو پۆرتاڵەکانی کڕیاری هاوکات وەستاند.','The unavailability of the authentication service brought all customer portals down at the same time.','دردسترس نبودن سرویس احراز هویت همه پورتال‌های مشتری را هم‌زمان از کار انداخت.','Neberdestbûna xizmeta piştrastkirinê hemû portalên xerîdar bi hev re rawestand.','Niedostępność usługi uwierzytelniania unieruchomiła jednocześnie wszystkie portale klientów.','Indisponibilitatea serviciului de autentificare a blocat simultan toate portalurile clienților.','Недоступность службы аутентификации одновременно вывела из строя все клиентские порталы.','Padisponueshmëria e shërbimit të autentifikimit i nxori jashtë funksionit të gjitha portalet e klientëve njëkohësisht.','Kimlik doğrulama hizmetinin kullanılamaması tüm müşteri portallarını aynı anda devre dışı bıraktı.')),
      ex('Die Unverfügbarkeit des Vergangenen ist im Roman keine Schwäche, sondern die Bedingung jeder Erinnerung.', m('إن عدم توافر الماضي في الرواية ليس ضعفاً، بل شرط كل ذاكرة.','بەردەستنەبوونی ڕابردوو لە ڕۆمانەکەدا لاوازی نییە، بەڵکو مەرجی هەر بیرەوەرییەکە.','The unavailability of the past is not a weakness in the novel, but the condition of every memory.','دردسترس‌نبودن گذشته در رمان ضعف نیست، بلکه شرط هر خاطره است.','Neberdestbûna rabirdûyê di romanê de ne lawazî ye, lê merca her bîranînekê ye.','Niedostępność przeszłości nie jest w powieści słabością, lecz warunkiem każdej pamięci.','Indisponibilitatea trecutului nu este în roman o slăbiciune, ci condiția oricărei amintiri.','Недоступность прошлого в романе не слабость, а условие всякой памяти.','Padisponueshmëria e së kaluarës në roman nuk është dobësi, por kusht i çdo kujtese.','Geçmişin erişilemezliği romanda bir zayıflık değil, her hafızanın koşuludur.'))
    ]
  }),
  entry({
    word: 'unverkennbar', partOfSpeech: 'Adjective', syllableBreak: 'un-ver-kenn-bar',
    topics: ['advanced-analysis','business-communication','culture-and-media'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'unverkennbar sein', meaning: 'to be unmistakable' }],
    meanings: m('واضح لا يمكن إنكاره؛ لا يُخطئ','نەناسرێنەوەناپذیر؛ زۆر دیار','unmistakable; clearly recognizable','انکارناپذیر؛ به‌روشنی قابل تشخیص','bêguman tê nasîn; zelal','nie do pomylenia; wyraźny','inconfundabil; evident','безошибочно узнаваемый; очевидный','i pagabueshëm; i qartë','apaçık; kolayca tanınır'),
    examples: [
      ex('Der Einfluss des alten Monolithen ist in der neuen Servicearchitektur noch unverkennbar.', m('ما يزال تأثير النظام الأحادي القديم واضحاً في بنية الخدمات الجديدة.','کاریگەری مۆنۆلیتی کۆن لە ئەندازیاری خزمەتگوزاری نوێدا هێشتا زۆر دیارە.','The influence of the old monolith is still unmistakable in the new service architecture.','تأثیر مونولیت قدیمی هنوز در معماری سرویس جدید کاملاً قابل تشخیص است.','Bandora monolîta kevn di avahiya nû ya xizmetan de hîn jî bêguman tê nasîn.','Wpływ starego monolitu jest w nowej architekturze usług nadal wyraźny.','Influența vechiului monolit este încă inconfundabilă în noua arhitectură de servicii.','Влияние старого монолита в новой сервисной архитектуре все еще безошибочно узнаваемо.','Ndikimi i monolitit të vjetër është ende i pagabueshëm në arkitekturën e re të shërbimeve.','Eski monolitin etkisi yeni servis mimarisinde hâlâ açıkça görülüyor.')),
      ex('Die letzten Kapitel tragen unverkennbar den Ton einer Autorin, die ihrer eigenen Lösung misstraut.', m('تحمل الفصول الأخيرة بوضوح نبرة كاتبة لا تثق بحلها الخاص.','بەشە کۆتاییەکان بە ڕوونی تۆنی نووسەرێک هەڵدەگرن کە متمانەی بە چارەسەری خۆی نییە.','The final chapters unmistakably carry the tone of an author who distrusts her own solution.','فصل‌های پایانی آشکارا لحن نویسنده‌ای را دارند که به راه‌حل خود بی‌اعتماد است.','Beşên dawî bêguman tona nivîskarekê hildigirin ku bawerî bi çareseriya xwe nake.','Ostatnie rozdziały noszą wyraźny ton autorki, która nie ufa własnemu rozwiązaniu.','Ultimele capitole poartă inconfundabil tonul unei autoare care nu are încredere în propria soluție.','Последние главы безошибочно несут тон авторки, которая не доверяет собственному решению.','Kapitujt e fundit mbartin qartë tonin e një autoreje që nuk i beson zgjidhjes së vet.','Son bölümler, kendi çözümüne güvenmeyen bir yazarın tonunu apaçık taşır.'))
    ]
  })
];
const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId: `de-${levelLower}-generated-batch-${batch}`, packageName: 'German C2 Generated Batch 084', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');
const cmd = `dotnet run --project "${path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj')}" -- --target shared --yes "${outPath}"`;
const result = cp.spawnSync(cmd, { shell: true, encoding: 'utf8', cwd: root });
const output = `${result.stdout || ''}${result.stderr || ''}`;
process.stdout.write(output);
const ok = result.status === 0 && output.includes('Entries imported: 6') && output.includes('Entries invalid: 0') && output.includes('Warnings: 0');
if (!ok) {
  fs.appendFileSync(path.join(root, 'content', 'generated', `${levelLower}-failed-words.txt`), `${new Date().toISOString()}\tbatch-${batch}\t${expected.join(', ')}\n`, 'utf8');
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

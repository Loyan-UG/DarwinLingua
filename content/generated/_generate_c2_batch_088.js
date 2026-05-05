const fs = require('fs');
const path = require('path');
const cp = require('child_process');
const root = 'D:/_Projects/DarwinLingua';
const level = 'C2', levelLower = 'c2', batch = '088';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['verquast','verrätseln','versanden','verschachtelt','verschroben','verselbstständigen'];
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
    word: 'verquast', partOfSpeech: 'Adjective', syllableBreak: 'ver-quast',
    topics: ['business-communication','advanced-analysis','culture-and-media'], usageLabels: ['informal','written','analysis','advanced'],
    collocations: [{ text: 'verquaste Formulierungen', meaning: 'convoluted and pretentious formulations' }],
    meanings: m('ملتوي ومتكلف؛ غامض بلا داع','ئاڵۆز و خۆنمایانە؛ بێپێویست ناڕوون','convoluted and pretentious; needlessly obscure','پیچیده و پرمدعا؛ بی‌دلیل مبهم','tevlihev û xwe-mezin; bêhewce nezelal','mętny i pretensjonalny','încâlcit și pretențios','заумный; вычурно-туманный','i ngatërruar dhe pretencioz','ağdalı; gereksiz karmaşık'),
    examples: [
      ex('Das Konzeptpapier war so verquast formuliert, dass selbst die Fachabteilung die eigentliche Entscheidung nicht erkannte.', m('صيغت ورقة المفهوم بشكل ملتوي إلى درجة أن القسم المختص نفسه لم يدرك القرار الحقيقي.','بەڵگەنامەی کۆنسێپتەکە ئەوەندە ئاڵۆز داڕێژرابوو کە تەنانەت بەشی پسپۆڕیش بڕیاری ڕاستەقینەی نەناسی.','The concept paper was phrased so convolutedly that even the specialist department did not recognize the actual decision.','سند مفهومی آن‌قدر پیچیده و پرمدعا نوشته شده بود که حتی بخش تخصصی تصمیم اصلی را تشخیص نداد.','Belgeya konseptê ewqas tevlihev hate nivîsandin ku tewra beşa pisporî biryara rastîn nas nekir.','Dokument koncepcyjny sformułowano tak mętnie, że nawet dział merytoryczny nie rozpoznał właściwej decyzji.','Documentul conceptual era formulat atât de încâlcit încât nici departamentul de specialitate nu a recunoscut decizia reală.','Концептуальный документ был сформулирован так заумно, что даже профильный отдел не распознал собственно решение.','Dokumenti konceptual ishte formuluar aq ngatërruar sa edhe departamenti specialist nuk e dalloi vendimin real.','Konsept dokümanı o kadar ağdalı yazılmıştı ki uzman departman bile asıl kararı fark etmedi.')),
      ex('Der verquaste Stil des Essays verdeckt eher Unsicherheit, als dass er gedankliche Tiefe erzeugt.', m('يخفي الأسلوب المتكلف للمقال عدم اليقين أكثر مما يخلق عمقاً فكرياً.','شێوازی ئاڵۆز و خۆنمایانەی وتارەکە زیاتر نادڵنیایی دەشارێتەوە، نەک قووڵیی فکری دروست بکات.','The convoluted style of the essay conceals uncertainty rather than creating intellectual depth.','سبک پیچیده و پرمدعای مقاله بیشتر نااطمینانی را می‌پوشاند تا اینکه عمق فکری بسازد.','Şêwaza tevlihev a gotarê zêdetir nediyarî veşêre, ne ku kûrahiya fikirî biafirîne.','Mętny styl eseju raczej ukrywa niepewność, niż tworzy głębię myśli.','Stilul încâlcit al eseului ascunde mai degrabă nesiguranța decât să creeze profunzime intelectuală.','Заумный стиль эссе скорее скрывает неуверенность, чем создает интеллектуальную глубину.','Stili i ngatërruar i esesë më shumë fsheh pasiguri sesa krijon thellësi mendimi.','Denemenin ağdalı üslubu düşünsel derinlik yaratmaktan çok güvensizliği örter.'))
    ]
  }),
  entry({
    word: 'verrätseln', partOfSpeech: 'Verb', infinitive: 'verrätseln', syllableBreak: 'ver-rät-seln',
    topics: ['culture-and-media','advanced-analysis','business-communication'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'eine Botschaft verrätseln', meaning: 'to make a message enigmatic' }],
    meanings: m('يجعل غامضاً كالأحجية','بە نهێنی و پرسیارئامێز کردن','to make enigmatic; to obscure as a riddle','معماگونه کردن؛ رازآلود ساختن','wek razek kirin; nepenî kirin','uczynić zagadkowym','a face enigmatic','загадочно зашифровать','e bëj enigmatik','bilmeceye çevirmek; gizemli kılmak'),
    examples: [
      ex('Die Statusmeldung verrätselte das Problem unnötig, statt klar zu sagen, welcher Dienst ausgefallen war.', m('جعلت رسالة الحالة المشكلة غامضة بلا داع بدلاً من أن تقول بوضوح أي خدمة تعطلت.','پەیامی دۆخەکە کێشەکەی بێپێویست نهێنی کرد، لەبری ئەوەی بە ڕوونی بڵێت کام خزمەتگوزاری وەستاوە.','The status message made the problem unnecessarily enigmatic instead of clearly saying which service had failed.','پیام وضعیت مشکل را بی‌دلیل معماگونه کرد، به‌جای اینکه روشن بگوید کدام سرویس از کار افتاده است.','Peyama rewşê pirsgirêk bêhewce nepenî kir li şûna ku zelal bêje kîjan xizmet qut bûye.','Komunikat statusowy niepotrzebnie uczynił problem zagadkowym zamiast jasno wskazać, która usługa przestała działać.','Mesajul de stare a făcut problema inutil de enigmatică în loc să spună clar ce serviciu a căzut.','Статусное сообщение излишне загадочно описало проблему вместо того, чтобы ясно сказать, какой сервис отказал.','Mesazhi i statusit e bëri problemin panevojshëm enigmatik në vend që të thoshte qartë cili shërbim kishte rënë.','Durum mesajı, hangi hizmetin çöktüğünü açıkça söylemek yerine sorunu gereksiz yere gizemli hale getirdi.')),
      ex('Der Film verrätselt die Herkunft der Fremden so konsequent, dass jede Erklärung verdächtig wirkt.', m('يجعل الفيلم أصل الغريبة غامضاً بإصرار إلى درجة أن كل تفسير يبدو مشبوهاً.','فیلمەکە سەرچاوەی نامۆکە بەردەوام نهێنی دەکات، بەجۆرێک کە هەر ڕوونکردنەوەیەک گومانلێکراو دەردەکەوێت.','The film makes the stranger’s origin so consistently enigmatic that every explanation seems suspicious.','فیلم منشأ زن غریبه را چنان پیوسته معماگونه می‌کند که هر توضیحی مشکوک به نظر می‌رسد.','Fîlm eslê xerîbê ewqas bi domdarî nepenî dike ku her şirove gumanbar xuya dike.','Film tak konsekwentnie czyni pochodzenie obcej zagadkowym, że każde wyjaśnienie wydaje się podejrzane.','Filmul face originea străinei atât de enigmatică încât orice explicație pare suspectă.','Фильм так последовательно загадочно подает происхождение незнакомки, что любое объяснение кажется подозрительным.','Filmi e bën origjinën e të huajës aq enigmatike sa çdo shpjegim duket i dyshimtë.','Film yabancının kökenini öyle tutarlı biçimde gizemli kılar ki her açıklama şüpheli görünür.'))
    ]
  }),
  entry({
    word: 'versanden', partOfSpeech: 'Verb', infinitive: 'versanden', syllableBreak: 'ver-san-den',
    topics: ['planning-and-projects','business-communication','advanced-analysis'], usageLabels: ['formal','written','business','analysis'],
    collocations: [{ text: 'ein Projekt versandet', meaning: 'a project fizzles out' }],
    meanings: m('يتعثر ويتلاشى؛ يضيع تدريجياً','هێواش هێواش ونبوون؛ بێئەنجام مانەوە','to fizzle out; to peter out','به جایی نرسیدن؛ تدریجاً فروکش کردن','hêdî bêencam man; lawaz bûn','utknąć i wygasnąć','a se împotmoli și a se stinge','заглохнуть; сойти на нет','shuhet gradualisht; mbetet pa rezultat','sonuçsuz sönmek; kuma saplanmak'),
    examples: [
      ex('Ohne klare Eigentümer versandete die Initiative nach drei Workshops und mehreren unverbindlichen Protokollen.', m('من دون مالكين واضحين تلاشت المبادرة بعد ثلاث ورش عمل وعدة محاضر غير ملزمة.','بەبێ خاوەنی ڕوون، هەوڵەکە دوای سێ وۆرکشۆپ و چەند پڕۆتۆکۆڵی ناپابەند هێواش هێواش ون بوو.','Without clear owners, the initiative fizzled out after three workshops and several non-binding minutes.','بدون مالک مشخص، ابتکار پس از سه کارگاه و چند صورتجلسه غیرالزام‌آور به جایی نرسید.','Bê xwediyên zelal, destpêşxerî piştî sê atolyeyan û çend protokolên negirêdayî versand.','Bez jasnych właścicieli inicjatywa wygasła po trzech warsztatach i kilku niewiążących protokołach.','Fără proprietari clari, inițiativa s-a stins după trei workshopuri și câteva procese-verbale neobligatorii.','Без четких владельцев инициатива заглохла после трех воркшопов и нескольких необязательных протоколов.','Pa pronarë të qartë, nisma u shua pas tre punëtorive dhe disa procesverbaleve jo detyruese.','Net sahipler olmadan girişim üç atölye ve birkaç bağlayıcı olmayan tutanaktan sonra sönüp gitti.')),
      ex('Die Suche nach dem Bruder versandet nicht plötzlich, sondern verliert mit jedem Kapitel ein wenig mehr Richtung.', m('لا تتلاشى عملية البحث عن الأخ فجأة، بل تفقد مع كل فصل شيئاً من اتجاهها.','گەڕان بەدوای براکەدا لەناکاو ون نابێت، بەڵکو لەگەڵ هەر بەشێک کەمێک زیاتر ئاراستەی لەدەست دەدات.','The search for the brother does not fizzle out suddenly, but loses a little more direction with each chapter.','جست‌وجوی برادر ناگهان به جایی نمی‌رسد، بلکه با هر فصل کمی بیشتر جهت خود را از دست می‌دهد.','Lêgerîna birayê ji nişkê ve versand nabe, lê bi her beşê hinekî zêdetir rêça xwe winda dike.','Poszukiwanie brata nie wygasa nagle, lecz z każdym rozdziałem traci trochę więcej kierunku.','Căutarea fratelui nu se stinge brusc, ci își pierde treptat direcția cu fiecare capitol.','Поиск брата не заглохает внезапно, а с каждой главой все больше теряет направление.','Kërkimi për vëllanë nuk shuhet papritur, por me çdo kapitull humb pak më shumë drejtim.','Kardeşi arayış birden sönmez, her bölümde biraz daha yönünü kaybeder.'))
    ]
  }),
  entry({
    word: 'verschachtelt', partOfSpeech: 'Adjective', syllableBreak: 'ver-schach-telt',
    topics: ['technology-and-it','documents-and-administration','culture-and-media'], usageLabels: ['formal','written','analysis','business'],
    collocations: [{ text: 'verschachtelte Strukturen', meaning: 'nested structures' }],
    meanings: m('متداخل؛ متشعب الطبقات','تودەرچوو؛ چینەچین','nested; intricately layered','تودرتو؛ لایه‌لایه و پیچیده','tevlihev û qatqat','zagnieżdżony; wielowarstwowy','încapsulat; încâlcit','вложенный; сложносоставной','i ndërthurur; me shtresa','iç içe geçmiş; katmanlı'),
    examples: [
      ex('Die verschachtelte JSON-Struktur war gültig, aber für die fachliche Prüfung kaum noch lesbar.', m('كانت بنية JSON المتداخلة صالحة، لكنها كانت بالكاد قابلة للقراءة في المراجعة المتخصصة.','پێکهاتەی JSON ـی تودەرچوو دروست بوو، بەڵام بۆ پشکنینی پسپۆڕی بە زەحمەت دەخوێندرایەوە.','The nested JSON structure was valid, but hardly readable for the domain review.','ساختار JSON تودرتو معتبر بود، اما برای بررسی تخصصی تقریباً خواندنی نبود.','Avahiya JSON ya tevlihev derbasdar bû, lê ji bo kontrola pisporî hema nexwendbar bû.','Zagnieżdżona struktura JSON była poprawna, ale dla przeglądu merytorycznego prawie nieczytelna.','Structura JSON încapsulată era validă, dar aproape ilizibilă pentru verificarea de specialitate.','Вложенная структура JSON была валидной, но почти нечитаемой для предметной проверки.','Struktura e ndërthurur JSON ishte valide, por pothuajse e palexueshme për kontrollin profesional.','İç içe geçmiş JSON yapısı geçerliydi, ancak alan incelemesi için neredeyse okunamazdı.')),
      ex('Die verschachtelte Erzählweise zwingt dazu, jede Erinnerung auf ihre Quelle hin zu prüfen.', m('تجبر طريقة السرد المتداخلة على فحص كل ذكرى من حيث مصدرها.','شێوازی گێڕانەوەی تودەرچوو ناچارت دەکات هەر بیرەوەرییەک لە ڕووی سەرچاوەکەیەوە بپشکنیت.','The nested narrative style forces the reader to check every memory against its source.','شیوه روایت تودرتو مجبور می‌کند هر خاطره از نظر منبعش بررسی شود.','Şêwaza vegotina tevlihev neçar dike ku her bîranîn li gorî çavkaniya xwe were kontrolkirin.','Zagnieżdżony sposób narracji zmusza do sprawdzania każdego wspomnienia pod kątem jego źródła.','Stilul narativ încâlcit obligă la verificarea fiecărei amintiri în raport cu sursa ei.','Вложенный способ повествования заставляет проверять каждое воспоминание по его источнику.','Mënyra e ndërthurur e rrëfimit detyron të kontrollohet çdo kujtim sipas burimit të tij.','İç içe anlatım tarzı, her anının kaynağına göre kontrol edilmesini zorunlu kılar.'))
    ]
  }),
  entry({
    word: 'verschroben', partOfSpeech: 'Adjective', syllableBreak: 'ver-schro-ben',
    topics: ['social-and-relationships','culture-and-media','business-communication'], usageLabels: ['formal','written','advanced','sensitive'],
    collocations: [{ text: 'eine verschrobene Idee', meaning: 'an eccentric or odd idea' }],
    meanings: m('غريب الأطوار؛ ملتوي التفكير','سەیر و نامۆ؛ بیرکردنەوەی ناڕێک','eccentric; odd; quirky','عجیب‌وغریب؛ نامتعارف','xerîb; neasayî','dziwaczny; ekscentryczny','excentric; ciudat','чудаковатый; странный','i çuditshëm; ekscentrik','tuhaf; eksantrik'),
    examples: [
      ex('Die Idee klang zunächst verschroben, erwies sich aber als brauchbarer Ansatz für ein schwieriges Datenproblem.', m('بدت الفكرة في البداية غريبة، لكنها أثبتت أنها نهج مفيد لمشكلة بيانات صعبة.','بیرۆکەکە سەرەتا سەیر دەنگی دەدا، بەڵام دواتر وەک ڕێبازێکی بەسوود بۆ کێشەیەکی قورسی داتا دەرکەوت.','The idea sounded eccentric at first, but proved to be a useful approach to a difficult data problem.','ایده ابتدا عجیب‌وغریب به نظر می‌رسید، اما برای یک مسئله دشوار داده رویکردی مفید از آب درآمد.','Fikr pêşî xerîb xuya kir, lê ji bo pirsgirêkeke dijwar a daneyan wek rêbazeke bikêr derket.','Pomysł początkowo brzmiał dziwacznie, ale okazał się użytecznym podejściem do trudnego problemu danych.','Ideea a sunat inițial excentric, dar s-a dovedit o abordare utilă pentru o problemă dificilă de date.','Идея сначала звучала чудаковато, но оказалась полезным подходом к сложной проблеме данных.','Ideja fillimisht tingëlloi e çuditshme, por doli qasje e dobishme për një problem të vështirë të të dhënave.','Fikir ilk başta tuhaf geliyordu, ancak zor bir veri problemi için kullanışlı bir yaklaşım olduğu ortaya çıktı.')),
      ex('Der verschrobene Onkel ist im Roman nicht komische Staffage, sondern der Einzige, der die Familienlüge durchschaut.', m('العم غريب الأطوار في الرواية ليس مجرد زينة كوميدية، بل الوحيد الذي يرى كذبة العائلة.','مامە سەیرەکە لە ڕۆمانەکەدا تەنها ڕازاندنەوەی کۆمیدی نییە، بەڵکو تەنها کەسە کە درۆی خێزانەکە دەبینێت.','The eccentric uncle in the novel is not comic decoration, but the only one who sees through the family lie.','عموی عجیب در رمان تزئین کمدی نیست، بلکه تنها کسی است که دروغ خانواده را می‌فهمد.','Apê xerîb di romanê de ne tenê xemilandina komîk e, lê yekane kes e ku derewa malbatê dibîne.','Dziwaczny wujek nie jest w powieści komicznym dodatkiem, lecz jedynym, który przejrzał rodzinne kłamstwo.','Unchiul excentric din roman nu este decor comic, ci singurul care vede minciuna familiei.','Чудаковатый дядя в романе не комическая декорация, а единственный, кто видит семейную ложь.','Xhaxhai ekscentrik në roman nuk është zbukurim komik, por i vetmi që sheh gënjeshtrën e familjes.','Romandaki tuhaf amca komik bir süs değil, aile yalanını gören tek kişidir.'))
    ]
  }),
  entry({
    word: 'verselbstständigen', partOfSpeech: 'Verb', infinitive: 'verselbstständigen', syllableBreak: 'ver-selbst-stän-di-gen',
    topics: ['management-and-leadership','advanced-analysis','technology-and-it'], usageLabels: ['formal','written','analysis','business'],
    grammarNotes: ['often reflexive: sich verselbstständigen'],
    collocations: [{ text: 'sich verselbstständigen', meaning: 'to take on a life of its own' }],
    meanings: m('يستقل بذاته؛ يخرج عن السيطرة','سەربەخۆبوون؛ لە دەست دەرچوون','to become independent; to take on a life of its own','مستقل شدن؛ از کنترل خارج شدن','serbixwe bûn; ji kontrolê derketin','usamodzielnić się; wymknąć się spod kontroli','a se autonomiza; a scăpa de sub control','обособиться; выйти из-под контроля','pavarësohet; del jashtë kontrollit','bağımsızlaşmak; kendi başına işler hale gelmek'),
    examples: [
      ex('Der Workaround verselbstständigte sich und wurde nach wenigen Monaten zum inoffiziellen Standardprozess.', m('استقل الحل المؤقت بذاته وأصبح بعد أشهر قليلة العملية القياسية غير الرسمية.','چارەسەری کاتییەکە سەربەخۆ بوو و دوای چەند مانگێک بوو بە پڕۆسەی ستانداردی نافەرمی.','The workaround took on a life of its own and became the unofficial standard process after a few months.','راه‌حل موقت مستقل شد و پس از چند ماه به فرایند استاندارد غیررسمی تبدیل شد.','Workaround serbixwe bû û piştî çend mehan bû pêvajoya standard a nefermî.','Workaround usamodzielnił się i po kilku miesiącach stał się nieoficjalnym procesem standardowym.','Soluția temporară s-a autonomizat și a devenit după câteva luni procesul standard neoficial.','Временный обходной путь зажил собственной жизнью и через несколько месяцев стал неофициальным стандартным процессом.','Zgjidhja e përkohshme u pavarësua dhe pas disa muajsh u bë proces standard jozyrtar.','Geçici çözüm kendi başına işler hale geldi ve birkaç ay sonra gayriresmi standart süreç oldu.')),
      ex('Im Verlauf der Erzählung verselbstständigt sich ein Gerücht, bis niemand mehr seine Quelle kennt.', m('مع تقدم السرد يستقل شائعة بذاتها حتى لا يعود أحد يعرف مصدرها.','لە ڕەوتی گێڕانەوەکەدا دەنگۆیەک سەربەخۆ دەبێت تا کەس ئیتر سەرچاوەکەی نازانێت.','As the narrative progresses, a rumor takes on a life of its own until no one knows its source anymore.','در جریان روایت، شایعه‌ای مستقل می‌شود تا جایی که دیگر هیچ‌کس منبعش را نمی‌داند.','Di pêvajoya vegotinê de gotegotek serbixwe dibe heta ku kes êdî çavkaniya wê nizanibe.','W toku opowieści plotka usamodzielnia się, aż nikt nie zna już jej źródła.','Pe parcursul narațiunii, un zvon capătă viață proprie până când nimeni nu îi mai cunoaște sursa.','По ходу повествования слух обособляется, пока никто уже не знает его источника.','Gjatë rrëfimit, një thashethem pavarësohet derisa askush nuk e di më burimin e tij.','Anlatı ilerledikçe bir söylenti kendi başına yaşamaya başlar, ta ki kimse kaynağını bilmeyene kadar.'))
    ]
  })
];
const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId: `de-${levelLower}-generated-batch-${batch}`, packageName: 'German C2 Generated Batch 088', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

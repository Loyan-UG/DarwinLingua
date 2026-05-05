const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '080';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['der Trugschluss','überbordend','die Überhöhung','überschreien','umschleichen','umwinden'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const topicKeys = new Set((taxonomy.topics || []).map(t => t.key));
const labelsByKey = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const source = fs.readFileSync(sourcePath, 'utf8');
const tokens = source.split(',').map(s => s.trim()).filter(Boolean);
const first = tokens.slice(0, 6);
if (JSON.stringify(first) !== JSON.stringify(expected)) throw new Error(`Unexpected first tokens: ${JSON.stringify(first)}`);

function meaning(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) { return [ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr].map((text, i) => ({ language: langs[i], text })); }
function ex(baseText, translations) { return { baseText, translations }; }
function entry(e) {
  for (const t of e.topics) if (!topicKeys.has(t)) throw new Error(`Unknown topic ${t}`);
  for (const l of [...(e.usageLabels || []), ...(e.contextLabels || [])]) if (!labelsByKey.has(l)) throw new Error(`Unknown label ${l}`);
  return Object.assign({ language: 'de', cefrLevel: level, article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: null, contextLabels: [], grammarNotes: [], collocations: [], wordFamilies: [], relations: [] }, e);
}

const entries = [
  entry({
    word: 'der Trugschluss', partOfSpeech: 'Noun', article: 'der', plural: 'Trugschlüsse', syllableBreak: 'Trug-schluss',
    topics: ['advanced-analysis','business-communication','education-and-training'], usageLabels: ['formal','written','analysis','academic'],
    collocations: [{ text: 'einem Trugschluss erliegen', meaning: 'to fall prey to a fallacy' }],
    meanings: meaning('استنتاج خاطئ؛ مغالطة','ئەنجامی هەڵە؛ خەڵەتاندنی لۆژیکی','fallacy; false conclusion','نتیجه‌گیری غلط؛ مغالطه','encameke şaş; xapandina lojîkî','błędny wniosek; złudne rozumowanie','raționament fals; concluzie greșită','ложный вывод; заблуждение','përfundim i gabuar; arsyetim mashtrues','yanlış çıkarım; safsata'),
    examples: [
      ex('Es ist ein Trugschluss, aus wenigen positiven Kundenstimmen sofort auf Marktreife zu schließen.', meaning('من المغالطة الاستنتاج فوراً من بضعة آراء إيجابية للعملاء أن المنتج جاهز للسوق.','ئەوە ئەنجامێکی هەڵەیە کە لە چەند دەنگێکی ئەرێنیی کڕیارەوە دەستبەجێ بگەیتە ئەوەی بەرهەمەکە ئامادەی بازاڕە.','It is a fallacy to infer market readiness immediately from a few positive customer comments.','این مغالطه است که از چند بازخورد مثبت مشتری فوراً به آمادگی بازار نتیجه بگیریم.','Ew encameke şaş e ku ji çend dengên erênî yên xerîdaran tavilê amadebûna bazarê derxin.','To błędny wniosek, by z kilku pozytywnych opinii klientów od razu wyprowadzać gotowość rynkową.','Este un raționament fals să deduci imediat maturitatea de piață din câteva opinii pozitive ale clienților.','Это ложный вывод — по нескольким положительным отзывам клиентов сразу заключать о готовности к рынку.','Është përfundim i gabuar të nxirret menjëherë gatishmëria për treg nga disa komente pozitive të klientëve.','Birkaç olumlu müşteri yorumundan hemen pazar olgunluğu sonucu çıkarmak bir safsatadır.')),
      ex('Der Essay entlarvt den Trugschluss, dass moralische Reinheit automatisch politische Klugheit hervorbringe.', meaning('يكشف المقال مغالطة أن النقاء الأخلاقي ينتج تلقائياً حكمة سياسية.','وتارەکە ئەو خەڵەتاندنە لۆژیکییە ئاشکرا دەکات کە پاکیی ئەخلاقی بە شێوەی ئۆتۆماتیکی ژیریی سیاسی دروست دەکات.','The essay exposes the fallacy that moral purity automatically produces political wisdom.','مقاله مغالطه‌ای را افشا می‌کند که پاکی اخلاقی خودبه‌خود خرد سیاسی می‌آورد.','Gotar ew xapandina lojîkî eşkere dike ku paqijiya exlaqî otomatîk jîrîya siyasî diafirîne.','Esej demaskuje błędny wniosek, że moralna czystość automatycznie rodzi polityczną mądrość.','Eseul demască raționamentul fals că puritatea morală produce automat înțelepciune politică.','Эссе разоблачает заблуждение, будто нравственная чистота автоматически порождает политическую мудрость.','Eseja zbulon arsyetimin mashtrues se pastërtia morale prodhon automatikisht mençuri politike.','Deneme, ahlaki saflığın otomatik olarak siyasi bilgelik ürettiği safsatasını açığa çıkarır.'))
    ]
  }),
  entry({
    word: 'überbordend', partOfSpeech: 'Adjective', syllableBreak: 'ü-ber-bor-dend',
    topics: ['business-communication','culture-and-media','advanced-analysis'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'überbordende Bürokratie', meaning: 'excessive bureaucracy' }],
    meanings: meaning('مفرط؛ فائض عن الحد','لەسنووردەرچوو؛ زۆر زیاد','excessive; overflowing','افراطی؛ سرریز و بیش از حد','ji sînor derbasbûyî; pir zêde','nadmierny; przelewający się','excesiv; debordant','чрезмерный; переполняющий','i tepruar; i tejmbushur','aşırı; taşkın'),
    examples: [
      ex('Die überbordende Dokumentationspflicht verlangsamte das Projekt stärker als die technische Komplexität.', meaning('أبطأ واجب التوثيق المفرط المشروع أكثر من التعقيد التقني.','ئەرکی بەڵگەنامەکردنی لەسنووردەرچوو پڕۆژەکەی زیاتر لە ئاڵۆزی تەکنیکی خاو کردەوە.','The excessive documentation requirement slowed the project more than the technical complexity did.','الزام مستندسازی افراطی پروژه را بیش از پیچیدگی فنی کند کرد.','Erka belgekirinê ya ji sînor derbasbûyî projeyê ji tevliheviya teknîkî zêdetir hêdî kir.','Nadmierny obowiązek dokumentacyjny spowolnił projekt bardziej niż złożoność techniczna.','Obligația excesivă de documentare a încetinit proiectul mai mult decât complexitatea tehnică.','Чрезмерное требование к документации замедлило проект сильнее, чем техническая сложность.','Detyrimi i tepruar për dokumentim e ngadalësoi projektin më shumë se kompleksiteti teknik.','Aşırı dokümantasyon yükümlülüğü projeyi teknik karmaşıklıktan daha fazla yavaşlattı.')),
      ex('Die überbordende Bildsprache des Romans erzeugt Fülle, droht aber gelegentlich die Handlung zu verdrängen.', meaning('تخلق لغة الصور المفرطة في الرواية وفرة، لكنها تهدد أحياناً بإزاحة الحبكة.','زمانی وێنەیی لەسنووردەرچووی ڕۆمانەکە پڕیی دروست دەکات، بەڵام جاروبار هەڕەشە دەکات ڕووداوەکان لاببات.','The novel’s overflowing imagery creates richness, but at times threatens to displace the plot.','تصویرپردازی افراطی رمان غنا ایجاد می‌کند، اما گاهی تهدید می‌کند داستان را کنار بزند.','Zimanê wêneyî yê pir zêde yê romanê dewlemendî diafirîne, lê carinan tehdît dike ku çîrokê bixe alî.','Nadmierna obrazowość powieści tworzy bogactwo, lecz chwilami grozi wyparciem fabuły.','Limbajul imagistic debordant al romanului creează bogăție, dar uneori amenință să înlocuiască acțiunea.','Чрезмерная образность романа создает богатство, но временами грозит вытеснить сюжет.','Gjuha e tejmbushur me imazhe e romanit krijon pasuri, por herë pas here rrezikon të zhvendosë ngjarjen.','Romanın taşkın imgeleri zenginlik yaratır, ancak zaman zaman olay örgüsünü geri plana itmekle tehdit eder.'))
    ]
  }),
  entry({
    word: 'die Überhöhung', partOfSpeech: 'Noun', article: 'die', plural: 'Überhöhungen', syllableBreak: 'Ü-ber-hö-hung',
    topics: ['culture-and-media','business-communication','advanced-analysis'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'eine ideologische Überhöhung', meaning: 'an ideological elevation or exaggeration' }],
    meanings: meaning('مبالغة تمجيدية؛ رفع رمزي زائد','بەرزکردنەوەی زیادە؛ مەزنکردنی زیادە','exaggerated elevation; idealization','بزرگ‌نمایی آرمانی؛ والاسازی افراطی','bilindkirina zêde; îdealîzekirin','wywyższenie; idealizujące przerysowanie','supraînălțare; idealizare excesivă','превознесение; чрезмерная идеализация','lartësim i tepruar; idealizim','aşırı yüceltme; idealizasyon'),
    examples: [
      ex('Die Überhöhung der Gründerfigur erschwerte es, sachlich über strategische Fehler zu sprechen.', meaning('جعل تمجيد شخصية المؤسس الحديث الموضوعي عن الأخطاء الاستراتيجية أكثر صعوبة.','بەرزکردنەوەی زیادەی کەسایەتی دامەزرێنەر قسەکردنی بابەتی لەسەر هەڵە ستراتیژییەکان قورستر کرد.','The idealized elevation of the founder figure made it harder to speak objectively about strategic mistakes.','والاسازی افراطی چهره بنیان‌گذار صحبت عینی درباره خطاهای راهبردی را دشوارتر کرد.','Bilindkirina zêde ya kesayeta damezrîner axaftina rast li ser çewtiyên stratejîk dijwartir kir.','Wywyższenie postaci założyciela utrudniało rzeczową rozmowę o błędach strategicznych.','Supraînălțarea figurii fondatorului a îngreunat discuția obiectivă despre greșelile strategice.','Чрезмерное превознесение фигуры основателя затрудняло предметный разговор о стратегических ошибках.','Lartësimi i tepruar i figurës së themeluesit e vështirësoi bisedën objektive për gabimet strategjike.','Kurucu figürün aşırı yüceltilmesi stratejik hatalar hakkında nesnel konuşmayı zorlaştırdı.')),
      ex('Die Überhöhung des Alltäglichen verleiht den einfachen Gegenständen im Gedicht eine beinahe religiöse Aura.', meaning('يضفي رفع اليومي إلى مرتبة أعلى على الأشياء البسيطة في القصيدة هالة شبه دينية.','بەرزکردنەوەی ڕۆژانەکان بە شتێکی مەزن بە ئامرازە سادەکانی شیعرەکە هەستی نزیک بە ئاینی دەدات.','The elevation of the everyday gives the simple objects in the poem an almost religious aura.','والاسازی امر روزمره به اشیای ساده شعر هاله‌ای تقریباً دینی می‌دهد.','Bilindkirina tiştên rojane aura hema olî dide tiştên sade yên helbestê.','Wywyższenie codzienności nadaje prostym przedmiotom w wierszu niemal religijną aurę.','Supraînălțarea cotidianului conferă obiectelor simple din poem o aură aproape religioasă.','Превознесение повседневного придает простым предметам в стихотворении почти религиозную ауру.','Lartësimi i së përditshmes u jep objekteve të thjeshta në poezi një aurë pothuajse fetare.','Gündelik olanın yüceltilmesi, şiirdeki basit nesnelere neredeyse dinsel bir aura verir.'))
    ]
  }),
  entry({
    word: 'überschreien', partOfSpeech: 'Verb', infinitive: 'überschreien', syllableBreak: 'ü-ber-schrei-en',
    topics: ['business-communication','social-and-relationships','culture-and-media'], usageLabels: ['formal','written','sensitive','advanced'],
    collocations: [{ text: 'Einwände überschreien', meaning: 'to shout down objections' }],
    meanings: meaning('يعلو بالصراخ على؛ يسكت بالصوت العالي','بە هاوار دەنگی کەسێک داپۆشین','to shout down; to drown out by shouting','با فریاد پوشاندن؛ صدای کسی را خاموش کردن','bi qîrîn dengê kesekî dorpêç kirin','przekrzykiwać; zagłuszać krzykiem','a acoperi prin strigăte','перекрикивать; заглушать криком','mbys me britma; tejkaloj me zë','bağırarak bastırmak'),
    examples: [
      ex('Im Krisenmeeting wurden die sachlichen Einwände der Entwickler von politischen Parolen überschrieen.', meaning('في اجتماع الأزمة غُطّيت اعتراضات المطورين الموضوعية بشعارات سياسية صاخبة.','لە کۆبوونەوەی قەیراندا ناڕەزاییە بابەتییەکانی پەرەپێدەران بە دروشمی سیاسی هاوارکراو داپۆشران.','In the crisis meeting, the developers’ factual objections were shouted down by political slogans.','در جلسه بحران، ایرادهای منطقی توسعه‌دهندگان با شعارهای سیاسی پوشانده شد.','Di civîna krîzê de îtirazên rast ên pêşvebirên bi dirûşmeyên siyasî hatin dorpêçkirin.','Na spotkaniu kryzysowym rzeczowe zastrzeżenia deweloperów zagłuszono politycznymi hasłami.','În ședința de criză, obiecțiile obiective ale dezvoltatorilor au fost acoperite de lozinci politice.','На кризисном совещании предметные возражения разработчиков были перекричаны политическими лозунгами.','Në mbledhjen e krizës, kundërshtimet faktike të zhvilluesve u mbytën nga slogane politike.','Kriz toplantısında geliştiricilerin nesnel itirazları siyasi sloganlarla bastırıldı.')),
      ex('Die Figur versucht ihre Angst zu überschreien, doch gerade die Lautstärke verrät ihre Unsicherheit.', meaning('تحاول الشخصية أن تغطي خوفها بالصراخ، لكن علو الصوت نفسه يكشف عدم يقينها.','کارەکتەرەکە هەوڵ دەدات ترسی خۆی بە هاوار داپۆشێت، بەڵام هەر بەرزی دەنگەکە نادڵنیاییەکەی ئاشکرا دەکات.','The character tries to shout down her fear, but the very volume reveals her insecurity.','شخصیت می‌کوشد ترسش را با فریاد بپوشاند، اما همین بلندی صدا ناامنی او را لو می‌دهد.','Kesayet hewl dide tirsa xwe bi qîrînê dorpêç bike, lê rast bilindbûna dengê bêewlehiya wê eşkere dike.','Postać próbuje przekrzyczeć swój strach, lecz właśnie głośność zdradza jej niepewność.','Personajul încearcă să își acopere frica prin strigăte, dar tocmai volumul îi trădează nesiguranța.','Персонаж пытается перекричать свой страх, но именно громкость выдает ее неуверенность.','Personazhi përpiqet ta mbysë frikën me britma, por pikërisht zëri i lartë zbulon pasigurinë e saj.','Karakter korkusunu bağırarak bastırmaya çalışır, ama tam da sesinin yüksekliği güvensizliğini ele verir.'))
    ]
  }),
  entry({
    word: 'umschleichen', partOfSpeech: 'Verb', infinitive: 'umschleichen', syllableBreak: 'um-schlei-chen',
    topics: ['culture-and-media','social-and-relationships','advanced-analysis'], usageLabels: ['formal','written','advanced','analysis'],
    collocations: [{ text: 'ein Thema umschleichen', meaning: 'to circle around a topic evasively' }],
    meanings: meaning('يتسلل حول؛ يطوف بحذر حول','بە نهێنی دەورەدان؛ بە وریایی لە دەوری شتێک گەڕان','to prowl around; to circle evasively','دزدانه دور چیزی گشتن؛ طفره‌رفتن از موضوع','bi dizî li dora tiştekê gerîn','skradać się wokół; omijać temat','a da târcoale; a ocoli prudent','красться вокруг; обходить стороной','vërtitem fshehurazi rreth; shmang','etrafında sinsi dolaşmak; konuyu dolanmak'),
    examples: [
      ex('Das Team umschlich die eigentliche Budgetfrage lange, weil niemand den Konflikt mit dem Vertrieb eröffnen wollte.', meaning('دار الفريق طويلاً حول سؤال الميزانية الحقيقي لأن أحداً لم يرد فتح النزاع مع المبيعات.','تیمەکە ماوەیەکی درێژ لە دەوری پرسیاری ڕاستەقینەی بودجە دەگەڕا، چونکە کەس نەیدەویست ململاێ لەگەڵ فرۆشتن دەست پێبکات.','The team circled around the real budget question for a long time because no one wanted to open the conflict with sales.','تیم مدت‌ها دور سؤال اصلی بودجه می‌چرخید، چون هیچ‌کس نمی‌خواست تعارض با فروش را آغاز کند.','Tîm demek dirêj li dora pirsa rastîn a budceyê geriya, ji ber ku kesek nexwest nakokiya bi firotanê re veke.','Zespół długo krążył wokół właściwej kwestii budżetu, bo nikt nie chciał otworzyć konfliktu ze sprzedażą.','Echipa a tot ocolit adevărata întrebare bugetară, deoarece nimeni nu voia să deschidă conflictul cu vânzările.','Команда долго обходила настоящий бюджетный вопрос, потому что никто не хотел открывать конфликт с продажами.','Ekipi e rrotulloi gjatë çështjen e vërtetë të buxhetit, sepse askush nuk donte të hapte konfliktin me shitjet.','Ekip asıl bütçe sorusunun etrafında uzun süre dolandı, çünkü kimse satışla çatışmayı açmak istemedi.')),
      ex('Im Roman umschleicht eine unausgesprochene Schuld jede Begegnung der Geschwister.', meaning('في الرواية تطوف ذنب غير مصرح به حول كل لقاء بين الإخوة.','لە ڕۆمانەکەدا تاوانێکی نەگوتراو هەر دیدارێکی خوشک و براکان دەورەدەدات.','In the novel, an unspoken guilt prowls around every meeting between the siblings.','در رمان، گناهی ناگفته دور هر دیدار خواهر و برادرها می‌گردد.','Di romanê de sûceke negotî li dora her hevdîtina xwişk û birayan digere.','W powieści niewypowiedziana wina krąży wokół każdego spotkania rodzeństwa.','În roman, o vină nerostită dă târcoale fiecărei întâlniri dintre frați.','В романе невысказанная вина кружит вокруг каждой встречи брата и сестры.','Në roman, një faj i pathënë sillet rreth çdo takimi të vëllezërve e motrave.','Romanda söylenmemiş bir suçluluk kardeşlerin her buluşmasının etrafında dolaşır.'))
    ]
  }),
  entry({
    word: 'umwinden', partOfSpeech: 'Verb', infinitive: 'umwinden', syllableBreak: 'um-win-den',
    topics: ['culture-and-media','environment-and-sustainability','advanced-analysis'], usageLabels: ['formal','written','advanced','analysis'],
    grammarNotes: ['literary verb; means to wind or twine around something'],
    collocations: [{ text: 'einen Pfosten umwinden', meaning: 'to wind around a post' }],
    meanings: meaning('يلتف حول؛ يطوّق','بە دەوری شتێک ئاڵاندن؛ پێچانەوە','to wind around; to entwine','پیچیدن به دور؛ دربرگرفتن','li dora tiştekê peçandin','oplatać; owijać','a se încolăci; a înfășura','обвивать; оплетать','mbështjell rreth; ndërthur','sarmak; dolanmak'),
    examples: [
      ex('Die Kabel umwanden den Serverrahmen so unübersichtlich, dass jede Wartung unnötig riskant wurde.', meaning('التفت الكابلات حول إطار الخادم بشكل غير واضح لدرجة أن كل صيانة أصبحت خطرة بلا داع.','کێبڵەکان بە شێوەیەکی ناڕێک لە دەوری چوارچێوەی سێرڤەرەکە ئاڵابوون کە هەر چاکسازییەک بە بێپێویستی مەترسیدار بوو.','The cables wound around the server frame so confusingly that every maintenance task became unnecessarily risky.','کابل‌ها آن‌قدر نامرتب دور قاب سرور پیچیده بودند که هر تعمیر و نگهداری بی‌دلیل پرریسک شده بود.','Kablo li dora çarçoveya serverê ewqas tevlihev peçabûn ku her parastin bê hewce xeternak bû.','Kable oplatały ramę serwera tak nieczytelnie, że każda konserwacja stawała się niepotrzebnie ryzykowna.','Cablurile se înfășurau în jurul cadrului serverului atât de neclar încât orice mentenanță devenea inutil de riscantă.','Кабели так запутанно обвивали раму сервера, что любое обслуживание становилось неоправданно рискованным.','Kabllot mbështilleshin rreth kornizës së serverit aq ngatërrueshëm sa çdo mirëmbajtje u bë panevojshëm e rrezikshme.','Kablolar sunucu çerçevesini o kadar karmaşık biçimde sarmıştı ki her bakım gereksiz yere riskli hale gelmişti.')),
      ex('Efeu umwindet die verlassene Villa und macht aus dem Verfall ein fast friedliches Bild.', meaning('يلتف اللبلاب حول الفيلا المهجورة ويحوّل الخراب إلى صورة شبه هادئة.','دارەهەڵگەر بە دەوری ڤێلای چۆڵدا ئاڵاوە و لە داڕمانەکە وێنەیەکی نزیک بە ئارام دروست دەکات.','Ivy winds around the abandoned villa and turns decay into an almost peaceful image.','پیچک دور ویلای متروک پیچیده و از زوال تصویری تقریباً آرام ساخته است.','Pelê darê li dora vîlaya vala peçaye û ji hilweşînê wêneyeke hema aram çêdike.','Bluszcz oplata opuszczoną willę i zamienia rozpad w niemal spokojny obraz.','Iedera se încolăcește în jurul vilei părăsite și transformă degradarea într-o imagine aproape pașnică.','Плющ обвивает заброшенную виллу и превращает распад в почти мирную картину.','Dredhka mbështillet rreth vilës së braktisur dhe e kthen rrënimin në një pamje pothuajse paqësore.','Sarmaşık terk edilmiş villanın etrafını sarar ve çürümeyi neredeyse huzurlu bir görüntüye dönüştürür.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 080', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

const fs = require('fs');
const cp = require('child_process');
const path = require('path');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '031';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const expected = ['ergründen', 'die Erhabenheit', 'erhellen', 'die Erkenntniskritik', 'die Erkenntnistheorie', 'der Erkenntniswert'];
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const labelsSource = Array.isArray(taxonomy.labels) ? taxonomy.labels : [];
const topicsSource = Array.isArray(taxonomy.topics) ? taxonomy.topics : [];
const labelMap = new Map(labelsSource.map(l => [l.key, l]));
const topicSet = new Set(topicsSource.map(t => t.key));

const splitTokens = text => text.split(',').map(t => t.trim()).filter(Boolean);
const sourceText = fs.readFileSync(sourcePath, 'utf8');
const tokens = splitTokens(sourceText);
const first = tokens.slice(0, expected.length);
if (JSON.stringify(first) !== JSON.stringify(expected)) {
  throw new Error(`Source token mismatch. Expected ${JSON.stringify(expected)} but found ${JSON.stringify(first)}`);
}

function m(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) {
  return [
    {language:'ar', text:ar}, {language:'ckb', text:ckb}, {language:'en', text:en}, {language:'fa', text:fa}, {language:'kmr', text:kmr},
    {language:'pl', text:pl}, {language:'ro', text:ro}, {language:'ru', text:ru}, {language:'sq', text:sq}, {language:'tr', text:tr}
  ];
}
function ex(baseText, translations) { return { baseText, translations }; }

const entries = [
  {
    word: 'ergründen', language: 'de', cefrLevel: level, partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'ergründen', pronunciationIpa: null, syllableBreak: 'er-grün-den',
    topics: ['advanced-analysis','quality-and-risk','culture-and-media'], usageLabels: ['formal','written','advanced','analysis'], contextLabels: [],
    grammarNotes: ['transitive verb; often used for deep investigation or interpretation'],
    collocations: [{text:'die Ursachen ergründen', meaning:'to investigate the underlying causes'}],
    wordFamilies: [{lemma:'der Grund', relationLabel:'root noun', note:null}, {lemma:'gründlich', relationLabel:'adjective', note:null}], relations: [],
    meanings: m('يتعمّق في فهم شيء؛ يستقصي أسبابه', 'بە قووڵی لێی بکۆڵێتەوە؛ هۆکارەکانی بدۆزێتەوە', 'to fathom; to investigate deeply', 'به‌طور عمیق بررسی کردن؛ به کنه چیزی پی بردن', 'bi kûrahî lêkolîn kirin; sedemên tiştekî fêm kirin', 'zgłębiać; dogłębnie badać', 'a pătrunde; a cerceta în profunzime', 'постигать; глубоко исследовать', 'të kuptosh thellë; të hetosh në thellësi', 'derinlemesine anlamak; kökenini araştırmak'),
    examples: [
      ex('Das Team versuchte zu ergründen, warum die Beschwerden trotz neuer Prozesse nicht zurückgingen.', m('حاول الفريق أن يفهم بعمق لماذا لم تنخفض الشكاوى رغم العمليات الجديدة.', 'تیمەکە هەوڵی دا بزانێت بۆچی سکاڵاکان سەرەڕای پرۆسە نوێیەکان کەم نەبوونەوە.', 'The team tried to understand why complaints did not decrease despite the new processes.', 'تیم تلاش کرد بفهمد چرا با وجود فرایندهای جدید، شکایت‌ها کمتر نشده‌اند.', 'Tîmê hewl da ku fêm bike çima gilî tevî pêvajoyên nû kêm nebûn.', 'Zespół próbował ustalić, dlaczego mimo nowych procesów liczba skarg nie spadła.', 'Echipa a încercat să înțeleagă de ce reclamațiile nu au scăzut, în ciuda noilor procese.', 'Команда пыталась понять, почему жалобы не уменьшились, несмотря на новые процессы.', 'Ekipi u përpoq të kuptonte pse ankesat nuk u ulën pavarësisht proceseve të reja.', 'Ekip, yeni süreçlere rağmen şikayetlerin neden azalmadığını anlamaya çalıştı.')),
      ex('Der Roman ergründet die Motive einer Figur, die sich jeder einfachen Erklärung entzieht.', m('تستكشف الرواية دوافع شخصية لا تقبل أي تفسير بسيط.', 'ڕۆمانەکە پاڵنەرەکانی کارەکتەرێک دەکۆڵێتەوە کە خۆی لە هەر ڕوونکردنەوەیەکی سادە دەدزێتەوە.', 'The novel explores the motives of a character who resists any simple explanation.', 'رمان انگیزه‌های شخصیتی را می‌کاود که از هر توضیح ساده‌ای می‌گریزد.', 'Roman motîvên kesayetiyekê vedikole ku ji her ravekirina hêsan dûr dikeve.', 'Powieść zgłębia motywy postaci, która wymyka się prostemu wyjaśnieniu.', 'Romanul explorează motivele unui personaj care scapă oricărei explicații simple.', 'Роман исследует мотивы персонажа, который не поддается простому объяснению.', 'Romani hulumton motivet e një personazhi që i shmanget çdo shpjegimi të thjeshtë.', 'Roman, basit açıklamalara sığmayan bir karakterin güdülerini derinlemesine inceler.'))
    ]
  },
  {
    word: 'die Erhabenheit', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'Er-ha-ben-heit',
    topics: ['culture-and-media','advanced-analysis','social-and-relationships'], usageLabels: ['formal','written','academic','analysis'], contextLabels: [],
    grammarNotes: ['feminine abstract noun; plural is uncommon'], collocations: [{text:'ästhetische Erhabenheit', meaning:'aesthetic sublimity or grandeur'}],
    wordFamilies: [{lemma:'erhaben', relationLabel:'adjective', note:null}], relations: [],
    meanings: m('سموّ؛ عظمة جليلة', 'بەرزی و شکۆمەندی', 'sublimity; grandeur', 'والایی؛ شکوه', 'bilindî; heybet', 'wzniosłość; majestatyczność', 'sublimitate; măreție', 'возвышенность; величие', 'madhështi; sublimitet', 'yücelik; ihtişam'),
    examples: [
      ex('Die Erhabenheit der Berglandschaft steht im Kontrast zur nüchternen Sprache des Berichts.', m('تقف عظمة المشهد الجبلي في تناقض مع لغة التقرير الجافة.', 'شکۆمەندی دیمەنی شاخەکان پێچەوانەی زمانی سارد و ڕاستەوخۆی ڕاپۆرتەکەیە.', 'The grandeur of the mountain landscape contrasts with the sober language of the report.', 'والایی منظره کوهستان با زبان خشک گزارش در تضاد است.', 'Heybeta dîmena çiyayî li hember zimanê hişk ê raporê radiweste.', 'Wzniosłość górskiego krajobrazu kontrastuje z rzeczowym językiem raportu.', 'Măreția peisajului montan contrastează cu limbajul sobru al raportului.', 'Величие горного пейзажа контрастирует со сдержанным языком отчета.', 'Madhështia e peizazhit malor bie në kontrast me gjuhën e përmbajtur të raportit.', 'Dağ manzarasının yüceliği, raporun ölçülü diliyle karşıtlık oluşturuyor.')),
      ex('Der Film sucht Erhabenheit nicht in großen Gesten, sondern in stillen Momenten der Aufmerksamkeit.', m('لا يبحث الفيلم عن السمو في الإيماءات الكبيرة، بل في لحظات هادئة من الانتباه.', 'فیلمەکە شکۆمەندی لە جووڵە گەورەکاندا ناگەڕێت، بەڵکو لە ساتە بێدەنگەکانی سەرنجدا.', 'The film seeks sublimity not in grand gestures, but in quiet moments of attention.', 'فیلم والایی را نه در حرکات بزرگ، بلکه در لحظه‌های آرام توجه جست‌وجو می‌کند.', 'Fîlm bilindiyê ne di tevgerên mezin de, lê di kêliyên bêdeng ên baldarî de digere.', 'Film szuka wzniosłości nie w wielkich gestach, lecz w cichych chwilach uwagi.', 'Filmul caută sublimul nu în gesturi grandioase, ci în momente tăcute de atenție.', 'Фильм ищет возвышенность не в громких жестах, а в тихих моментах внимания.', 'Filmi e kërkon madhështinë jo në gjeste të mëdha, por në çaste të qeta vëmendjeje.', 'Film yüceliği büyük jestlerde değil, sessiz dikkat anlarında arıyor.'))
    ]
  },
  {
    word: 'erhellen', language: 'de', cefrLevel: level, partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'erhellen', pronunciationIpa: null, syllableBreak: 'er-hel-len',
    topics: ['advanced-analysis','data-and-reporting','education-and-training'], usageLabels: ['formal','written','analysis'], contextLabels: [],
    grammarNotes: ['transitive verb; can mean to make something clearer or brighter'], collocations: [{text:'einen Zusammenhang erhellen', meaning:'to clarify a connection'}],
    wordFamilies: [{lemma:'hell', relationLabel:'adjective', note:null}], relations: [],
    meanings: m('يوضح؛ يسلّط الضوء على', 'ڕوون بکاتەوە؛ ڕوونایی بخاتە سەر', 'to clarify; to shed light on', 'روشن کردن؛ توضیح دادن', 'ronî kirin; zelal kirin', 'wyjaśniać; rzucać światło na coś', 'a clarifica; a lumina', 'прояснять; проливать свет', 'të sqarosh; të hedhësh dritë mbi diçka', 'açıklığa kavuşturmak; ışık tutmak'),
    examples: [
      ex('Die zusätzliche Auswertung erhellte, weshalb die Abbruchquote im Onboarding gestiegen war.', m('أوضح التحليل الإضافي سبب ارتفاع معدل الانسحاب أثناء مرحلة التعريف بالعمل.', 'شیکردنەوەی زیادە ڕوونی کردەوە بۆچی ڕێژەی وازهێنان لە ئۆنبۆردینگ زیاد بوو.', 'The additional analysis clarified why the dropout rate during onboarding had increased.', 'تحلیل تکمیلی روشن کرد که چرا نرخ ریزش در فرایند ورود به کار افزایش یافته بود.', 'Analîza zêde zelal kir ku çima rêjeya devjêberdanê di onboarding de zêde bû.', 'Dodatkowa analiza wyjaśniła, dlaczego wzrósł odsetek rezygnacji podczas onboardingu.', 'Analiza suplimentară a clarificat de ce crescuse rata abandonului în procesul de onboarding.', 'Дополнительный анализ прояснил, почему выросла доля отказов во время онбординга.', 'Analiza shtesë sqaroi pse ishte rritur shkalla e ndërprerjeve gjatë onboarding-ut.', 'Ek analiz, işe alıştırma sürecindeki ayrılma oranının neden arttığını açıklığa kavuşturdu.')),
      ex('Ein Blick in die Briefe erhellt die schwierige Beziehung zwischen den beiden Autoren.', m('تساعد نظرة إلى الرسائل على توضيح العلاقة الصعبة بين الكاتبين.', 'سەیرکردنی نامەکان پەیوەندییە سەختەکەی نێوان هەردوو نووسەر ڕوون دەکاتەوە.', 'A look at the letters sheds light on the difficult relationship between the two authors.', 'نگاهی به نامه‌ها رابطه دشوار میان دو نویسنده را روشن می‌کند.', 'Nêrînek li nameyan têkiliya dijwar a di navbera her du nivîskaran de zelal dike.', 'Wgląd w listy rzuca światło na trudną relację między dwoma autorami.', 'O privire asupra scrisorilor clarifică relația dificilă dintre cei doi autori.', 'Обращение к письмам проливает свет на сложные отношения между двумя авторами.', 'Një vështrim në letrat hedh dritë mbi marrëdhënien e vështirë mes dy autorëve.', 'Mektuplara bakmak, iki yazar arasındaki zor ilişkiye ışık tutar.'))
    ]
  },
  {
    word: 'die Erkenntniskritik', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'Er-kennt-nis-kri-tik',
    topics: ['advanced-analysis','education-and-training','culture-and-media'], usageLabels: ['formal','written','academic','analysis'], contextLabels: [],
    grammarNotes: ['feminine noun; plural is rare'], collocations: [{text:'kantische Erkenntniskritik', meaning:'Kantian critique of knowledge'}],
    wordFamilies: [{lemma:'die Erkenntnis', relationLabel:'noun', note:null}, {lemma:'kritisch', relationLabel:'adjective', note:null}], relations: [],
    meanings: m('نقد المعرفة؛ فحص شروط المعرفة', 'ڕەخنەی زانین؛ پشکنینی مەرجەکانی زانین', 'critique of knowledge; epistemological critique', 'نقد شناخت؛ بررسی شرایط امکان شناخت', 'rexneya zanînê; rexneya epîstemolojîk', 'krytyka poznania; krytyka epistemologiczna', 'critica cunoașterii; critică epistemologică', 'критика познания; эпистемологическая критика', 'kritikë e njohjes; kritikë epistemologjike', 'bilgi eleştirisi; epistemolojik eleştiri'),
    examples: [
      ex('Die Erkenntniskritik fragt, welche Voraussetzungen unsere Dateninterpretation überhaupt möglich machen.', m('يسأل نقد المعرفة عن الشروط التي تجعل تفسيرنا للبيانات ممكناً أصلاً.', 'ڕەخنەی زانین دەپرسی کە چ مەرجانێک لێکدانەوەی داتاکانمان بە بنەڕەت دەکەنە شتێکی مومکین.', 'The critique of knowledge asks which preconditions make our interpretation of data possible at all.', 'نقد شناخت می‌پرسد چه پیش‌فرض‌هایی اساساً تفسیر داده‌های ما را ممکن می‌کنند.', 'Rexneya zanînê dipirse ka kîjan şert şîrovekirina daneyên me bi rastî gengaz dikin.', 'Krytyka poznania pyta, jakie warunki w ogóle umożliwiają naszą interpretację danych.', 'Critica cunoașterii întreabă ce premise fac posibilă interpretarea datelor noastre.', 'Критика познания спрашивает, какие предпосылки вообще делают возможной нашу интерпретацию данных.', 'Kritika e njohjes pyet cilat parakushte e bëjnë të mundur interpretimin tonë të të dhënave.', 'Bilgi eleştirisi, veri yorumumuzu mümkün kılan önkoşulların neler olduğunu sorar.')),
      ex('Im Seminar wurde Kants Erkenntniskritik nicht historisch isoliert, sondern auf aktuelle KI-Debatten bezogen.', m('في الحلقة الدراسية لم تُعزل نظرية كانط النقدية للمعرفة تاريخياً، بل رُبطت بنقاشات الذكاء الاصطناعي الحالية.', 'لە سیمینارەکەدا ڕەخنەی زانینی کانت بە شێوەی مێژوویی جیا نەکرایەوە، بەڵکو پەیوەست کرا بە گفتوگۆکانی ئێستای زیرەکی دەستکرد.', 'In the seminar, Kant’s critique of knowledge was not treated in historical isolation, but connected to current AI debates.', 'در سمینار، نقد شناخت کانت به‌صورت تاریخی و جداگانه بررسی نشد، بلکه به بحث‌های کنونی هوش مصنوعی پیوند داده شد.', 'Di seminarê de rexneya zanînê ya Kant ne bi awayekî dîrokî veqetandî hate nîqaşkirin, lê bi nîqaşên niha yên AI ve hate girêdan.', 'Na seminarium Kantowskiej krytyki poznania nie omawiano w historycznej izolacji, lecz odniesiono ją do obecnych debat o AI.', 'La seminar, critica cunoașterii la Kant nu a fost tratată izolat istoric, ci legată de dezbaterile actuale despre IA.', 'На семинаре кантовскую критику познания рассматривали не в исторической изоляции, а в связи с актуальными дебатами об ИИ.', 'Në seminar, kritika e njohjes te Kanti nuk u trajtua e izoluar historikisht, por u lidh me debatet aktuale për AI.', 'Seminerde Kant’ın bilgi eleştirisi tarihsel olarak yalıtılmış biçimde değil, güncel yapay zeka tartışmalarıyla bağlantılı ele alındı.'))
    ]
  },
  {
    word: 'die Erkenntnistheorie', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Erkenntnistheorien', infinitive: null, pronunciationIpa: null, syllableBreak: 'Er-kennt-nis-the-o-rie',
    topics: ['advanced-analysis','education-and-training','culture-and-media'], usageLabels: ['formal','written','academic','analysis'], contextLabels: [],
    grammarNotes: ['feminine noun'], collocations: [{text:'klassische Erkenntnistheorie', meaning:'classical epistemology'}],
    wordFamilies: [{lemma:'die Erkenntnis', relationLabel:'noun', note:null}, {lemma:'theoretisch', relationLabel:'adjective', note:null}], relations: [],
    meanings: m('نظرية المعرفة؛ الإبستمولوجيا', 'تیۆری زانین؛ ئیپستیمۆلۆژیا', 'epistemology; theory of knowledge', 'معرفت‌شناسی؛ نظریه شناخت', 'epîstemolojî; teoriya zanînê', 'epistemologia; teoria poznania', 'epistemologie; teoria cunoașterii', 'эпистемология; теория познания', 'epistemologji; teori e njohjes', 'epistemoloji; bilgi teorisi'),
    examples: [
      ex('Die Erkenntnistheorie untersucht, wann eine begründete Überzeugung als Wissen gelten kann.', m('تبحث نظرية المعرفة متى يمكن اعتبار قناعة مبررة معرفةً.', 'تیۆری زانین لێکۆڵینەوە دەکات لەوەی کە کەی باوەڕێکی پشتڕاستکراو دەتوانێت وەک زانین هەژمار بکرێت.', 'Epistemology examines when a justified belief can count as knowledge.', 'معرفت‌شناسی بررسی می‌کند که چه زمانی یک باور موجه می‌تواند دانش محسوب شود.', 'Epîstemolojî lêkolîn dike kengî baweriyek bi sedem dikare wek zanîn bê hesibandin.', 'Epistemologia bada, kiedy uzasadnione przekonanie można uznać za wiedzę.', 'Epistemologia examinează când o convingere justificată poate fi considerată cunoaștere.', 'Эпистемология исследует, когда обоснованное убеждение может считаться знанием.', 'Epistemologjia shqyrton kur një bindje e arsyetuar mund të quhet dije.', 'Epistemoloji, gerekçelendirilmiş bir inancın ne zaman bilgi sayılabileceğini inceler.')),
      ex('Für die Bewertung automatisierter Entscheidungen wird Erkenntnistheorie plötzlich praktisch relevant.', m('عند تقييم القرارات المؤتمتة تصبح نظرية المعرفة فجأة ذات أهمية عملية.', 'بۆ هەڵسەنگاندنی بڕیارە ئۆتۆماتیکەکان، تیۆری زانین لەناکاو گرنگییەکی کرداری پەیدا دەکات.', 'For assessing automated decisions, epistemology suddenly becomes practically relevant.', 'برای ارزیابی تصمیم‌های خودکار، معرفت‌شناسی ناگهان اهمیت عملی پیدا می‌کند.', 'Ji bo nirxandina biryarên xweser, epîstemolojî ji nişkê ve girîngiyeke pratîk distîne.', 'Przy ocenie decyzji zautomatyzowanych epistemologia nagle staje się praktycznie istotna.', 'Pentru evaluarea deciziilor automatizate, epistemologia devine brusc relevantă practic.', 'При оценке автоматизированных решений эпистемология внезапно приобретает практическое значение.', 'Për vlerësimin e vendimeve të automatizuara, epistemologjia bëhet papritur praktikisht e rëndësishme.', 'Otomatik kararların değerlendirilmesinde epistemoloji birden pratik olarak önemli hale gelir.'))
    ]
  },
  {
    word: 'der Erkenntniswert', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'der', plural: 'Erkenntniswerte', infinitive: null, pronunciationIpa: null, syllableBreak: 'Er-kennt-nis-wert',
    topics: ['advanced-analysis','data-and-reporting','education-and-training'], usageLabels: ['formal','written','academic','analysis'], contextLabels: [],
    grammarNotes: ['masculine noun'], collocations: [{text:'ein hoher Erkenntniswert', meaning:'a high informational or analytical value'}],
    wordFamilies: [{lemma:'die Erkenntnis', relationLabel:'noun', note:null}, {lemma:'der Wert', relationLabel:'noun', note:null}], relations: [],
    meanings: m('القيمة المعرفية؛ الفائدة التحليلية', 'بەهای زانستی؛ سوودی شیکاری', 'informational value; epistemic value', 'ارزش شناختی؛ ارزش تحلیلی', 'nirxa zanînê; nirxa analîtîk', 'wartość poznawcza; wartość analityczna', 'valoare de cunoaștere; valoare analitică', 'познавательная ценность; аналитическая ценность', 'vlerë njohëse; vlerë analitike', 'bilgisel değer; analitik değer'),
    examples: [
      ex('Der Erkenntniswert der Umfrage bleibt begrenzt, weil nur sehr wenige Kunden geantwortet haben.', m('تبقى القيمة المعرفية للاستبيان محدودة لأن عدداً قليلاً جداً من العملاء أجابوا.', 'بەهای زانستی ڕاپرسییەکە سنووردار دەمێنێتەوە، چونکە ژمارەیەکی زۆر کەم لە کڕیاران وەڵامیان داوەتەوە.', 'The informational value of the survey remains limited because very few customers responded.', 'ارزش شناختی نظرسنجی محدود می‌ماند، چون فقط تعداد بسیار کمی از مشتریان پاسخ داده‌اند.', 'Nirxa zanînê ya rapirsiyê sînordar dimîne, ji ber ku pir kêm xerîdar bersiv dane.', 'Wartość poznawcza ankiety pozostaje ograniczona, ponieważ odpowiedziało bardzo niewielu klientów.', 'Valoarea de cunoaștere a sondajului rămâne limitată, deoarece au răspuns foarte puțini clienți.', 'Познавательная ценность опроса остается ограниченной, потому что ответило очень мало клиентов.', 'Vlera njohëse e anketës mbetet e kufizuar, sepse janë përgjigjur shumë pak klientë.', 'Anketin bilgisel değeri sınırlı kalıyor, çünkü çok az müşteri yanıt verdi.')),
      ex('Der Text hat einen hohen Erkenntniswert, obwohl er keine einfachen Lösungen anbietet.', m('للنص قيمة معرفية عالية رغم أنه لا يقدم حلولاً سهلة.', 'دەقەکە بەهای زانستی بەرزی هەیە، هەرچەندە چارەسەری سادە پێشکەش ناکات.', 'The text has high analytical value, even though it offers no simple solutions.', 'متن ارزش شناختی بالایی دارد، هرچند راه‌حل‌های ساده ارائه نمی‌دهد.', 'Nivîs nirxeke zanînê ya bilind heye, herçend çareseriyên hêsan pêşkêş nake.', 'Tekst ma dużą wartość poznawczą, choć nie proponuje prostych rozwiązań.', 'Textul are o valoare de cunoaștere ridicată, deși nu oferă soluții simple.', 'Текст обладает высокой познавательной ценностью, хотя не предлагает простых решений.', 'Teksti ka vlerë të lartë njohëse, megjithëse nuk ofron zgjidhje të thjeshta.', 'Metin, basit çözümler sunmasa da yüksek bir bilgisel değere sahip.'))
    ]
  }
];

for (const entry of entries) {
  for (const topic of entry.topics) if (!topicSet.has(topic)) throw new Error(`Unknown topic ${topic}`);
  for (const label of [...entry.usageLabels, ...entry.contextLabels]) if (!labelMap.has(label)) throw new Error(`Unknown label ${label}`);
  if (entry.meanings.length !== 10) throw new Error(`Meaning count failed for ${entry.word}`);
  for (const e of entry.examples) if (e.translations.length !== 10) throw new Error(`Translation count failed for ${entry.word}`);
}
const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const labels = usedLabels.map(k => labelMap.get(k));

const pkg = {
  packageVersion: '1.0',
  packageId: `de-${levelLower}-generated-batch-${batch}`,
  packageName: `German ${level} Generated Batch ${batch}`,
  source: 'Hybrid',
  defaultMeaningLanguages: langs,
  labels,
  entries,
  collections: [],
  scenarios: [],
  conversationStarterPacks: [],
  eventPreparationPacks: []
};
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');

const importCmd = `dotnet run --project "${path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj')}" -- --target shared --yes "${outPath}"`;
const result = cp.spawnSync(importCmd, { shell: true, encoding: 'utf8', cwd: root });
const output = `${result.stdout || ''}\n${result.stderr || ''}`;
console.log(output.replace(/(ConnectionString|Password|Pwd|Secret|Key)=[^\s;]+/gi, '$1=***'));
if (result.status !== 0) throw new Error(`Import command failed with status ${result.status}`);
const ok = output.includes('Entries imported: 6') && output.includes('Entries invalid: 0') && output.includes('Warnings: 0');
if (!ok) throw new Error('Import did not meet strict success criteria; source not modified.');

const remaining = tokens.slice(expected.length);
fs.writeFileSync(sourcePath, remaining.join(', '), 'utf8');
console.log(`SOURCE_UPDATED: ${sourcePath}`);
console.log(`PROCESSED: ${expected.join(' | ')}`);
console.log(`JSON_PATH: ${outPath}`);
console.log(`REMAINING_COUNT: ${remaining.length}`);
console.log(`FIRST_10: ${remaining.slice(0, 10).join(' | ')}`);

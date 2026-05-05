const fs = require('fs');
const path = require('path');
const cp = require('child_process');
const root = 'D:/_Projects/DarwinLingua';
const level = 'C2', levelLower = 'c2', batch = '085';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['die Unversöhnlichkeit','die Unwägbarkeit','die Unzulänglichkeit','verabsolutieren','verbeißen','das Verdikt'];
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
    word: 'die Unversöhnlichkeit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Un-ver-söhn-lich-keit',
    topics: ['social-and-relationships','advanced-analysis','culture-and-media'], usageLabels: ['formal','written','sensitive','analysis'], grammarNotes: ['feminine abstract noun; normally singular'],
    collocations: [{ text: 'politische Unversöhnlichkeit', meaning: 'political irreconcilability' }],
    meanings: m('عدم المصالحة؛ قطيعة لا تلين','ئاشتنەبوون؛ پێکناگەیشتنی توند','irreconcilability; unforgiving hostility','آشتی‌ناپذیری؛ خصومت حل‌ناشدنی','lihevnehatin; dijminatiya neçareser','nieprzejednanie; niepojednanie','intransigență; ireconciliabilitate','непримиримость','papajtueshmëri','uzlaşmazlık; barışmazlık'),
    examples: [
      ex('Die Unversöhnlichkeit zwischen Vertrieb und Produktteam blockierte Entscheidungen, die technisch längst vorbereitet waren.', m('أدى عدم المصالحة بين المبيعات وفريق المنتج إلى تعطيل قرارات كانت معدة تقنياً منذ زمن.','ئاشتنەبوونی نێوان فرۆشتن و تیمی بەرهەم بڕیارەکانی ڕاگرت کە لە ڕووی تەکنیکییەوە زۆر پێشتر ئامادە بوون.','The irreconcilability between sales and the product team blocked decisions that had long been technically prepared.','آشتی‌ناپذیری میان فروش و تیم محصول تصمیم‌هایی را متوقف کرد که از نظر فنی مدت‌ها آماده بودند.','Lihevnehatina navbera firotan û tîma berhemê biryarên ku ji aliyê teknîkî ve demek dirêj amade bûn asteng kir.','Nieprzejednanie między sprzedażą a zespołem produktu blokowało decyzje technicznie od dawna przygotowane.','Intransigența dintre vânzări și echipa de produs bloca decizii pregătite tehnic de mult timp.','Непримиримость между продажами и продуктовой командой блокировала решения, которые технически давно были подготовлены.','Papajtueshmëria mes shitjeve dhe ekipit të produktit bllokoi vendime që teknikisht ishin përgatitur prej kohësh.','Satış ile ürün ekibi arasındaki uzlaşmazlık, teknik olarak çoktan hazırlanmış kararları engelledi.')),
      ex('Im Drama wird Unversöhnlichkeit nicht als Stärke, sondern als Erbe einer alten Verletzung gezeigt.', m('في المسرحية لا تُعرض عدم المصالحة كقوة، بل كإرث لجرح قديم.','لە شانۆنامەکەدا ئاشتنەبوون وەک هێز پیشان نادرێت، بەڵکو وەک میراتی بریندارییەکی کۆن.','In the drama, irreconcilability is not shown as strength, but as the legacy of an old wound.','در نمایش، آشتی‌ناپذیری نه به‌عنوان قدرت، بلکه میراث زخمی قدیمی نشان داده می‌شود.','Di dramayê de lihevnehatin ne wek hêz tê nîşandan, lê wek mîrata birînek kevn.','W dramacie niepojednanie nie jest pokazane jako siła, lecz jako dziedzictwo dawnej rany.','În dramă, intransigența nu este prezentată ca forță, ci ca moștenire a unei răni vechi.','В драме непримиримость показана не как сила, а как наследие старой раны.','Në dramë, papajtueshmëria nuk paraqitet si forcë, por si trashëgimi e një plage të vjetër.','Dramda uzlaşmazlık güç olarak değil, eski bir yaranın mirası olarak gösterilir.'))
    ]
  }),
  entry({
    word: 'die Unwägbarkeit', partOfSpeech: 'Noun', article: 'die', plural: 'Unwägbarkeiten', syllableBreak: 'Un-wäg-bar-keit',
    topics: ['quality-and-risk','planning-and-projects','advanced-analysis'], usageLabels: ['formal','written','business','analysis'],
    collocations: [{ text: 'rechtliche Unwägbarkeiten', meaning: 'legal imponderables' }],
    meanings: m('عامل غير قابل للتقدير؛ مخاطرة غير محسوبة','هۆکاری پێوانەناکرێت؛ مەترسیی نادیار','imponderable; unpredictable factor','عامل سنجش‌ناپذیر؛ ریسک نامعلوم','faktora nepîvanbar; xetera nediyar','niewiadoma; czynnik nieprzewidywalny','imponderabil; factor imprevizibil','непредсказуемый фактор','faktor i paparashikueshëm','öngörülemez etken; bilinmezlik'),
    examples: [
      ex('Die größte Unwägbarkeit im Projekt ist nicht die Technik, sondern die noch offene Entscheidung der Aufsichtsbehörde.', m('أكبر عامل غير قابل للتقدير في المشروع ليس التقنية، بل قرار الجهة الرقابية الذي ما زال مفتوحاً.','گەورەترین هۆکاری پێوانەناکرێت لە پڕۆژەکەدا تەکنیک نییە، بەڵکو بڕیاری هێشتا کراوەی دەزگای چاودێرییە.','The biggest imponderable in the project is not the technology, but the still open decision of the supervisory authority.','بزرگ‌ترین عامل نامعلوم در پروژه فناوری نیست، بلکه تصمیم هنوز باز نهاد ناظر است.','Faktora herî mezin a nepîvanbar di projeyê de ne teknîk e, lê biryara hêj vekirî ya saziya çavdêrî ye.','Największą niewiadomą w projekcie nie jest technologia, lecz wciąż otwarta decyzja organu nadzoru.','Cel mai mare imponderabil din proiect nu este tehnologia, ci decizia încă deschisă a autorității de supraveghere.','Самый большой непредсказуемый фактор проекта — не технология, а еще открытое решение надзорного органа.','Faktori më i madh i paparashikueshëm në projekt nuk është teknologjia, por vendimi ende i hapur i autoritetit mbikëqyrës.','Projede en büyük bilinmezlik teknoloji değil, denetleyici kurumun hâlâ açık olan kararıdır.')),
      ex('Die Erzählung lebt von Unwägbarkeiten, die jede scheinbar sichere Deutung wieder ins Rutschen bringen.', m('تعيش القصة من عوامل غير قابلة للتقدير تجعل كل تفسير آمن ظاهرياً ينزلق من جديد.','گێڕانەوەکە بە هۆکارە پێوانەناکرێتەکان دەژی کە هەر لێکدانەوەیەکی بە ڕواڵەت دڵنیا دووبارە دەخەنە جوڵان.','The narrative thrives on imponderables that make every seemingly secure interpretation slip again.','روایت از عوامل سنجش‌ناپذیری جان می‌گیرد که هر تفسیر ظاهراً مطمئن را دوباره لغزان می‌کنند.','Vegotin ji faktorên nepîvanbar dijî ku her şîroveya xuya ewle dîsa dixin tevgerê.','Opowieść żyje niewiadomymi, które każdą pozornie pewną interpretację ponownie wprawiają w ruch.','Narațiunea trăiește din imponderabile care fac ca orice interpretare aparent sigură să alunece din nou.','Повествование живет непредсказуемыми факторами, которые снова сдвигают каждое, казалось бы, надежное толкование.','Rrëfimi jeton nga të paparashikueshmet që e bëjnë çdo interpretim në dukje të sigurt të rrëshqasë përsëri.','Anlatı, görünüşte güvenli her yorumu yeniden kaydıran bilinmezliklerle yaşar.'))
    ]
  }),
  entry({
    word: 'die Unzulänglichkeit', partOfSpeech: 'Noun', article: 'die', plural: 'Unzulänglichkeiten', syllableBreak: 'Un-zu-läng-lich-keit',
    topics: ['quality-and-risk','business-communication','advanced-analysis'], usageLabels: ['formal','written','analysis','business'],
    collocations: [{ text: 'strukturelle Unzulänglichkeiten', meaning: 'structural inadequacies' }],
    meanings: m('قصور؛ عدم كفاية','کەموکوڕی؛ نەگونجاوی','inadequacy; insufficiency','نارسایی؛ ناکافی‌بودن','kêmasî; têrnebûn','niedostatek; niewystarczalność','insuficiență; inadecvare','недостаточность; несостоятельность','pamjaftueshmëri','yetersizlik'),
    examples: [
      ex('Die Unzulänglichkeit des Testkonzepts zeigte sich erst, als mehrere Randfälle gleichzeitig auftraten.', m('لم يظهر قصور مفهوم الاختبار إلا عندما ظهرت عدة حالات حدية في الوقت نفسه.','کەموکوڕی کۆنسێپتی تاقیکردنەوە تەنها کاتێک دیار بوو کە چەند کەیسی سنووری هاوکات ڕوویاندا.','The inadequacy of the test concept became apparent only when several edge cases occurred at the same time.','نارسایی طرح آزمون فقط وقتی آشکار شد که چند حالت مرزی هم‌زمان رخ دادند.','Kêmasiya konsepta testê tenê wê demê xuya bû ku çend rewşên sînorî bi hev re derketin.','Niewystarczalność koncepcji testów ujawniła się dopiero, gdy jednocześnie wystąpiło kilka przypadków brzegowych.','Insuficiența conceptului de testare a devenit evidentă abia când au apărut simultan mai multe cazuri limită.','Недостаточность тестовой концепции стала видна только тогда, когда одновременно возникло несколько граничных случаев.','Pamjaftueshmëria e konceptit të testimit u shfaq vetëm kur disa raste kufitare ndodhën njëkohësisht.','Test konseptinin yetersizliği ancak birkaç uç durum aynı anda ortaya çıktığında görüldü.')),
      ex('Der Text thematisiert die Unzulänglichkeit jeder Sprache, die traumatische Erfahrung vollständig zu erfassen.', m('يتناول النص قصور كل لغة عن الإحاطة الكاملة بالتجربة الصادمة.','دەقەکە بابەتی کەموکوڕی هەر زمانێک دەکات کە ناتوانێت ئەزموونی ترۆماتیک بە تەواوی بگرێتەوە.','The text addresses the inadequacy of any language to fully capture traumatic experience.','متن نارسایی هر زبانی را در ثبت کامل تجربه آسیب‌زا موضوع قرار می‌دهد.','Nivîs li ser kêmasiya her zimanekê diaxive ku nikare ezmûna traumatîk bi tevahî bigire.','Tekst podejmuje temat niewystarczalności każdego języka wobec pełnego uchwycenia doświadczenia traumatycznego.','Textul tematizează insuficiența oricărui limbaj de a surprinde complet experiența traumatică.','Текст тематизирует недостаточность любого языка для полного охвата травматического опыта.','Teksti trajton pamjaftueshmërinë e çdo gjuhe për të kapur plotësisht përvojën traumatike.','Metin, travmatik deneyimi bütünüyle kavramada her dilin yetersizliğini konu eder.'))
    ]
  }),
  entry({
    word: 'verabsolutieren', partOfSpeech: 'Verb', infinitive: 'verabsolutieren', syllableBreak: 'ver-ab-so-lu-tie-ren',
    topics: ['advanced-analysis','management-and-leadership','culture-and-media'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'ein Prinzip verabsolutieren', meaning: 'to absolutize a principle' }],
    meanings: m('يجعل مطلقاً؛ يرفع إلى قيمة مطلقة','ڕەهاکردن؛ وەک بنەمای تەواو دانان','to absolutize; to treat as absolute','مطلق کردن؛ اصل را بی‌قید دانستن','mutleq kirin; wek tiştekî bê şert dîtin','absolutyzować','a absolutiza','абсолютизировать','absolutizoj','mutlaklaştırmak'),
    examples: [
      ex('Wer Geschwindigkeit verabsolutiert, übersieht leicht die Folgekosten technischer Schulden.', m('من يجعل السرعة قيمة مطلقة يغفل بسهولة تكاليف الديون التقنية اللاحقة.','ئەوەی خێرایی ڕەها دەکات، بە ئاسانی تێچووی دواتری قەرزی تەکنیکی لەبەرچاو ناگرێت.','Anyone who absolutizes speed easily overlooks the follow-up costs of technical debt.','کسی که سرعت را مطلق می‌کند، به‌راحتی هزینه‌های بعدی بدهی فنی را نادیده می‌گیرد.','Kesê ku lezê mutleq dike, bi hêsanî mesrefên paşerojê yên deynê teknîkî nedibîne.','Kto absolutyzuje szybkość, łatwo przeocza późniejsze koszty długu technicznego.','Cine absolutizează viteza trece ușor cu vederea costurile ulterioare ale datoriei tehnice.','Тот, кто абсолютизирует скорость, легко упускает последующие издержки технического долга.','Kush absolutizon shpejtësinë, lehtë shpërfill kostot pasuese të borxhit teknik.','Hızı mutlaklaştıran kişi teknik borcun sonraki maliyetlerini kolayca gözden kaçırır.')),
      ex('Der Essay warnt davor, Freiheit so zu verabsolutieren, dass jede Form von Verantwortung verdächtig erscheint.', m('يحذر المقال من جعل الحرية مطلقة إلى حد تبدو معه كل مسؤولية مشبوهة.','وتارەکە ئاگاداری دەدات لەوەی ئازادی بە شێوەیەک ڕەها بکرێت کە هەر جۆرێک لە بەرپرسیاری وەک گومانلێکراو دەربکەوێت.','The essay warns against absolutizing freedom in such a way that every form of responsibility appears suspicious.','مقاله هشدار می‌دهد که آزادی را چنان مطلق نکنیم که هر شکل مسئولیت مشکوک به نظر برسد.','Gotar hişyar dike ku azadî wisa mutleq neyê kirin ku her şêweya berpirsiyarî gumanbar xuya bike.','Esej ostrzega przed absolutyzowaniem wolności tak, że każda forma odpowiedzialności wydaje się podejrzana.','Eseul avertizează împotriva absolutizării libertății în așa fel încât orice formă de responsabilitate să pară suspectă.','Эссе предостерегает от абсолютизации свободы так, что любая форма ответственности кажется подозрительной.','Eseja paralajmëron kundër absolutizimit të lirisë në mënyrë që çdo formë përgjegjësie të duket e dyshimtë.','Deneme, özgürlüğü her sorumluluk biçimi şüpheli görünecek şekilde mutlaklaştırmaya karşı uyarır.'))
    ]
  }),
  entry({
    word: 'verbeißen', partOfSpeech: 'Verb', infinitive: 'verbeißen', syllableBreak: 'ver-bei-ßen',
    topics: ['work-and-jobs','social-and-relationships','advanced-analysis'], usageLabels: ['formal','written','sensitive','advanced'],
    grammarNotes: ['often reflexive: sich in etwas verbeißen'],
    collocations: [{ text: 'sich in ein Problem verbeißen', meaning: 'to get doggedly stuck on a problem' }],
    meanings: m('يتمسك بعناد؛ يعضّ بقوة','بە سەرسەختی چەسبان؛ ددان تێگرتن','to bite into; to become doggedly fixated','با سماجت گیر کردن؛ دندان گرفتن','bi serhişkî çespîn; diranan lê girtin','zaciąć się na czymś; wgryźć się','a se fixa cu încăpățânare; a mușca','вцепиться; зациклиться','ngulitem me kokëfortësi','inatla takılıp kalmak; ısırmak'),
    examples: [
      ex('Der Entwickler verbiss sich in einen seltenen Bug und übersah dabei, dass der Release insgesamt gefährdet war.', m('انشغل المطور بعناد بخطأ نادر وغفل عن أن الإصدار بأكمله كان مهدداً.','پەرەپێدەرەکە بە سەرسەختی لە باگێکی دەگمەن گیر بوو و لەوە غافڵ بوو کە تەواوی release ـەکە لە مەترسیدا بوو.','The developer became doggedly fixated on a rare bug and overlooked that the release as a whole was at risk.','توسعه‌دهنده با سماجت روی یک باگ نادر گیر کرد و ندید که کل انتشار در خطر است.','Pêşvebir bi serhişkî li bugê kêm verbeiss bû û nedît ku tevahiya release di xeterê de ye.','Deweloper zaciął się na rzadkim błędzie i przeoczył, że całe wydanie było zagrożone.','Dezvoltatorul s-a fixat cu încăpățânare pe un bug rar și a trecut cu vederea că întregul release era în pericol.','Разработчик зациклился на редком баге и упустил, что весь релиз оказался под угрозой.','Zhvilluesi u ngulit me kokëfortësi te një bug i rrallë dhe shpërfilli se release-i në tërësi ishte në rrezik.','Geliştirici nadir bir hataya inatla takılıp kaldı ve sürümün bütünüyle risk altında olduğunu gözden kaçırdı.')),
      ex('Die Figur verbeißt sich in eine alte Kränkung, bis jede neue Begegnung von ihr vergiftet wird.', m('تتشبث الشخصية بعناد بإهانة قديمة حتى تسمم كل لقاء جديد.','کارەکتەرەکە بە سەرسەختی لە سووکایەتییەکی کۆن دەچەسپێت تا هەر دیدارێکی نوێی پێوە ژەهراوی دەبێت.','The character fixates on an old injury until every new encounter is poisoned by it.','شخصیت با سماجت به رنجشی قدیمی می‌چسبد تا هر دیدار تازه از آن مسموم می‌شود.','Kesayet bi serhişkî li êşeke kevn diçe heta her hevdîtineke nû pê jehrî dibe.','Postać zacina się na dawnej urazie, aż każde nowe spotkanie zostaje przez nią zatrute.','Personajul se fixează cu încăpățânare pe o ofensă veche până când fiecare întâlnire nouă este otrăvită de ea.','Персонаж вцепляется в старую обиду, пока каждая новая встреча не отравляется ею.','Personazhi ngulitet në një lëndim të vjetër derisa çdo takim i ri helmohet prej tij.','Karakter eski bir kırgınlığa inatla saplanır, ta ki her yeni karşılaşma onunla zehirlenir.'))
    ]
  }),
  entry({
    word: 'das Verdikt', partOfSpeech: 'Noun', article: 'das', plural: 'Verdikte', syllableBreak: 'Ver-dikt',
    topics: ['law-and-compliance','culture-and-media','advanced-analysis'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'ein hartes Verdikt', meaning: 'a harsh verdict' }],
    meanings: m('حكم قاطع؛ إدانة','حوکمی توند؛ بڕیاری قاطع','verdict; authoritative judgment','حکم قاطع؛ داوری سخت','verdîkt; biryara tund','werdykt; osąd','verdict; judecată categorică','вердикт; решительный приговор','verdikt; gjykim i prerë','kesin hüküm; verdict'),
    examples: [
      ex('Das Verdikt der Prüfer fiel hart aus: Die Dokumentation sei für einen regulierten Betrieb nicht ausreichend.', m('جاء حكم المراجعين قاسياً: فالوثائق غير كافية للتشغيل المنظم.','حوکمی پشکنەرەکان توند بوو: بەڵگەنامەکان بۆ کارکردنی ڕێکخراو بەس نین.','The auditors’ verdict was harsh: the documentation was not sufficient for regulated operations.','حکم ممیزان سخت بود: مستندات برای بهره‌برداری در محیط تنظیم‌شده کافی نیست.','Verdîkta kontrolkeran tund bû: belgekirin ji bo xebata rêkûpêk têr nake.','Werdykt audytorów był surowy: dokumentacja nie wystarcza do pracy w środowisku regulowanym.','Verdictul auditorilor a fost dur: documentația nu era suficientă pentru operare reglementată.','Вердикт аудиторов был жестким: документации недостаточно для работы в регулируемой среде.','Verdikti i auditorëve ishte i ashpër: dokumentacioni nuk mjaftonte për operim të rregulluar.','Denetçilerin hükmü sertti: Dokümantasyon düzenlenmiş bir işletim için yeterli değildi.')),
      ex('Das literarische Verdikt über die Epoche bleibt ambivalent und verweigert jede einfache Verurteilung.', m('يبقى الحكم الأدبي على العصر ملتبساً ويرفض أي إدانة بسيطة.','حوکمی ئەدەبی لەسەر سەردەمەکە دوولایەنە دەمێنێتەوە و ڕەتیدەکاتەوە هەر مەحکومکردنێکی سادە.','The literary verdict on the era remains ambivalent and refuses any simple condemnation.','داوری ادبی درباره دوره مبهم و دوگانه می‌ماند و هر محکومیت ساده را رد می‌کند.','Verdîkta edebî li ser serdemê ambîvalent dimîne û her mehkûmkirineke sade red dike.','Literacki werdykt o epoce pozostaje ambiwalentny i odmawia prostego potępienia.','Verdictul literar asupra epocii rămâne ambivalent și refuză orice condamnare simplă.','Литературный вердикт эпохе остается амбивалентным и отказывается от простого осуждения.','Verdikti letrar mbi epokën mbetet ambivalent dhe refuzon çdo dënim të thjeshtë.','Döneme dair edebi hüküm ikircikli kalır ve basit bir mahkumiyeti reddeder.'))
    ]
  })
];
const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId: `de-${levelLower}-generated-batch-${batch}`, packageName: 'German C2 Generated Batch 085', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

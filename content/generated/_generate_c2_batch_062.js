const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '062';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['oszillieren','das Oxymoron','das Palimpsest','parabolisch','das Paradoxon','paraphrasieren'];

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
    word: 'oszillieren', partOfSpeech: 'Verb', infinitive: 'oszillieren', syllableBreak: 'os-zil-lie-ren',
    topics: ['advanced-analysis','data-and-reporting','technology-and-it'], usageLabels: ['formal','written','analysis','academic'],
    collocations: [{ text: 'zwischen zwei Positionen oszillieren', meaning: 'to oscillate between two positions' }],
    meanings: meaning('يتذبذب؛ يتأرجح','لە نێوان دوو شتدا لەرزین؛ هەڵوەشانەوە','to oscillate; to fluctuate','نوسان کردن؛ در رفت‌وبرگشت بودن','livîn di navbera du tiştan de; hejandin','oscylować; wahać się','a oscila; a fluctua','колебаться; осциллировать','lëkundet; oscilon','salınmak; dalgalanmak'),
    examples: [
      ex('Die Kennzahlen oszillieren seit Wochen zwischen leichter Erholung und erneuter Verschlechterung.', trans('تتذبذب المؤشرات منذ أسابيع بين تحسن طفيف وتدهور جديد.','پێوەرەکان چەند هەفتەیە لە نێوان باشبوونەوەیەکی کەم و خراپبوونەوەی دووبارەدا دەخولێنەوە.','For weeks, the indicators have oscillated between slight recovery and renewed deterioration.','شاخص‌ها هفته‌هاست میان بهبود جزئی و وخامت دوباره نوسان می‌کنند.','Nîşaneyan ev çend hefte di navbera başbûneke biçûk û xirabûneke nû de dihejên.','Od tygodni wskaźniki oscylują między lekką poprawą a ponownym pogorszeniem.','Indicatorii oscilează de săptămâni între o ușoară revenire și o nouă deteriorare.','Показатели уже несколько недель колеблются между небольшим восстановлением и новым ухудшением.','Treguesit prej javësh lëkunden mes një rimëkëmbjeje të lehtë dhe një përkeqësimi të ri.','Göstergeler haftalardır hafif toparlanma ile yeniden kötüleşme arasında dalgalanıyor.')),
      ex('Der Essay oszilliert bewusst zwischen persönlicher Erinnerung und politischer Analyse.', trans('يتأرجح المقال عمداً بين الذاكرة الشخصية والتحليل السياسي.','وتارەکە بە ئەنقەست لە نێوان بیرەوەریی کەسی و شیکاریی سیاسی دەجوڵێتەوە.','The essay deliberately oscillates between personal memory and political analysis.','مقاله عمداً میان خاطره شخصی و تحلیل سیاسی در رفت‌وبرگشت است.','Gotar bi mebest di navbera bîranîna kesane û analîza siyasî de dihejê.','Esej świadomie oscyluje między osobistym wspomnieniem a analizą polityczną.','Eseul oscilează deliberat între amintirea personală și analiza politică.','Эссе сознательно колеблется между личным воспоминанием и политическим анализом.','Eseja lëkundet qëllimisht mes kujtesës personale dhe analizës politike.','Deneme bilinçli olarak kişisel anı ile siyasi analiz arasında gidip geliyor.'))
    ]
  }),
  entry({
    word: 'das Oxymoron', partOfSpeech: 'Noun', article: 'das', plural: 'Oxymora', syllableBreak: 'O-xy-mo-ron',
    topics: ['culture-and-media','education-and-training','advanced-analysis'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'ein scheinbares Oxymoron', meaning: 'an apparent oxymoron' }],
    meanings: meaning('تعبير متناقض ظاهرياً؛ أوكسيمورون','دەربڕینێکی دژبەیەکی دیار؛ ئۆکسیمۆرۆن','oxymoron; apparent contradiction','ترکیب متناقض‌نما؛ اکسیمورون','gotineke dijber xuya; oksîmoron','oksymoron','oximoron','оксюморон','oksimoron','oksimoron'),
    examples: [
      ex('Die Formulierung „kontrollierte Offenheit“ klingt wie ein Oxymoron, beschreibt aber präzise die neue API-Strategie.', trans('تبدو عبارة «انفتاح مضبوط» كأنها تعبير متناقض، لكنها تصف بدقة استراتيجية واجهة البرمجة الجديدة.','دەربڕینی «کراوەیی کۆنتڕۆڵکراو» وەک ئۆکسیمۆرۆنێک دەبیسترێت، بەڵام ستراتیژیای نوێی API بە وردی وەسف دەکات.','The phrase “controlled openness” sounds like an oxymoron, but it precisely describes the new API strategy.','عبارت «بازبودن کنترل‌شده» مثل یک ترکیب متناقض‌نما به نظر می‌رسد، اما راهبرد جدید API را دقیق توصیف می‌کند.','Gotina “vekirina kontrolkirî” wek oksîmoronek xuya dike, lê stratejiya nû ya API bi rastî vedibêje.','Sformułowanie „kontrolowana otwartość” brzmi jak oksymoron, ale precyzyjnie opisuje nową strategię API.','Formularea „deschidere controlată” sună ca un oximoron, dar descrie precis noua strategie API.','Формулировка «контролируемая открытость» звучит как оксюморон, но точно описывает новую стратегию API.','Shprehja “hapje e kontrolluar” tingëllon si oksimoron, por përshkruan saktë strategjinë e re të API-së.','“Kontrollü açıklık” ifadesi bir oksimoron gibi geliyor, ama yeni API stratejisini tam olarak anlatıyor.')),
      ex('In der Lyrik kann ein Oxymoron eine Spannung erzeugen, die eine nüchterne Beschreibung nicht leisten würde.', trans('في الشعر يمكن للتعبير المتناقض أن يخلق توتراً لا تحققه الوصفية الجافة.','لە شیعردا ئۆکسیمۆرۆن دەتوانێت گرژییەک دروست بکات کە وەسفێکی سارد ناتوانێت.','In poetry, an oxymoron can create a tension that a sober description could not achieve.','در شعر، اکسیمورون می‌تواند تنشی بسازد که توصیف خشک از عهده آن برنمی‌آید.','Di helbestê de oksîmoron dikare tevlîheviyek biafirîne ku şiroveyeke hişk nikare bike.','W poezji oksymoron może wytworzyć napięcie, którego nie dałby trzeźwy opis.','În poezie, un oximoron poate crea o tensiune pe care o descriere sobră nu ar putea-o produce.','В поэзии оксюморон может создать напряжение, которого не даст сухое описание.','Në poezi, një oksimoron mund të krijojë një tension që një përshkrim i ftohtë nuk do ta arrinte.','Şiirde bir oksimoron, sade bir betimlemenin sağlayamayacağı bir gerilim yaratabilir.'))
    ]
  }),
  entry({
    word: 'das Palimpsest', partOfSpeech: 'Noun', article: 'das', plural: 'Palimpseste', syllableBreak: 'Pa-limp-sest',
    topics: ['culture-and-media','advanced-analysis','education-and-training'], usageLabels: ['formal','written','academic','advanced'],
    collocations: [{ text: 'wie ein Palimpsest gelesen werden', meaning: 'to be read like a palimpsest' }],
    meanings: meaning('رقّ ممحو ومكتوب فوقه؛ نص بطبقات','دەستنووسێکی سڕاوە و دووبارە نووسراو؛ دەقێکی چینەدار','palimpsest; layered text or surface','پالیمپسست؛ متن یا سطح لایه‌لایه','palîmpsest; nivîsek bi qatên cuda','palimpsest','palimpsest','палимпсест; многослойный текст','palimpsest; tekst me shtresa','palimpsest; katmanlı metin'),
    examples: [
      ex('Die Stadt liest sich wie ein Palimpsest, in dem jede Generation Spuren hinterlassen und zugleich ältere Zeichen überdeckt hat.', trans('تُقرأ المدينة كأنها نص بطبقات، ترك فيه كل جيل آثاره وغطّى في الوقت نفسه علامات أقدم.','شارەکە وەک پالیمپسستێک دەخوێندرێتەوە کە هەر نەوەیەک شوێنی خۆی جێهێشتووە و هەمان کات نیشانە کۆنەکانیش داپۆشیوە.','The city reads like a palimpsest in which each generation has left traces while covering older signs.','شهر مانند پالیمپسستی خوانده می‌شود که هر نسل در آن رد خود را گذاشته و هم‌زمان نشانه‌های قدیمی‌تر را پوشانده است.','Bajar wek palîmpsestek tê xwendin ku her nifş şopên xwe lê hiştine û di heman demê de nîşanên kevintir veşartine.','Miasto czyta się jak palimpsest, w którym każde pokolenie zostawiło ślady i zarazem przykryło starsze znaki.','Orașul se citește ca un palimpsest în care fiecare generație a lăsat urme și a acoperit totodată semne mai vechi.','Город читается как палимпсест, где каждое поколение оставило следы и одновременно перекрыло более старые знаки.','Qyteti lexohet si një palimpsest ku çdo brez ka lënë gjurmë dhe njëkohësisht ka mbuluar shenja më të vjetra.','Şehir, her kuşağın iz bıraktığı ve aynı zamanda daha eski işaretleri örttüğü bir palimpsest gibi okunur.')),
      ex('Auch die Dokumentation des Altsystems war ein Palimpsest aus überarbeiteten Annahmen, gestrichenen Funktionen und nie gelöschten Randnotizen.', trans('كانت وثائق النظام القديم أيضاً نصاً بطبقات من افتراضات معدلة ووظائف مشطوبة وملاحظات هامشية لم تُحذف قط.','بەڵگەنامەکانی سیستەمی کۆنیش پالیمپسستێک بوون لە گریمانەی دەستکاریکراو، فەنکشنی سڕاوە و تێبینی لاوەکیی هەرگیز نەسڕاوە.','The legacy system documentation was also a palimpsest of revised assumptions, crossed-out functions, and marginal notes that were never deleted.','مستندات سیستم قدیمی نیز پالیمپسستی از فرض‌های بازنگری‌شده، قابلیت‌های خط‌خورده و یادداشت‌های حاشیه‌ای حذف‌نشده بود.','Belgekirina pergala kevn jî palîmpsestek bû ji texmînên guherandî, fonksiyonên jêbirî û têbîniyên kêlekan ên qet nehatiye jêbirin.','Dokumentacja starego systemu była także palimpsestem poprawionych założeń, skreślonych funkcji i nigdy nieusuniętych notatek na marginesie.','Documentația sistemului vechi era și ea un palimpsest de ipoteze revizuite, funcții tăiate și note marginale niciodată șterse.','Документация старой системы тоже была палимпсестом из пересмотренных допущений, зачеркнутых функций и так и не удаленных примечаний на полях.','Dokumentacioni i sistemit të vjetër ishte gjithashtu një palimpsest supozimesh të rishikuara, funksionesh të fshira dhe shënimesh anësore që nuk u hoqën kurrë.','Eski sistemin dokümantasyonu da gözden geçirilmiş varsayımlar, üstü çizilmiş işlevler ve hiç silinmemiş kenar notlarından oluşan bir palimpsestti.'))
    ]
  }),
  entry({
    word: 'parabolisch', partOfSpeech: 'Adjective', syllableBreak: 'pa-ra-bo-lisch',
    topics: ['education-and-training','advanced-analysis','culture-and-media'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'eine parabolische Erzählung', meaning: 'a parabolic or allegorical narrative' }],
    meanings: meaning('قطع مكافئ؛ رمزي على شكل مثل','پەرابۆڵی؛ هاوشێوەی چیرۆکی وانەدار','parabolic; allegorical','سهمی؛ تمثیلی','parabolîk; alegorîk','paraboliczny; przypowieściowy','parabolic; alegoric','параболический; притчевый','parabolik; alegorik','parabolik; mesel niteliğinde'),
    examples: [
      ex('Die Kurve verläuft parabolisch, weshalb kleine Änderungen am Anfang später stark verstärkt werden.', trans('يسير المنحنى بشكل قطع مكافئ، ولذلك تتضخم التغييرات الصغيرة في البداية لاحقاً بقوة.','هێڵەکە بە شێوەی پەرابۆڵی دەڕوات، بۆیە گۆڕانکارییە بچووکەکانی سەرەتا دواتر بەهێزتر دەبن.','The curve is parabolic, which is why small changes at the beginning are amplified strongly later.','منحنی سهمی است؛ بنابراین تغییرات کوچک در ابتدا بعداً به‌شدت تقویت می‌شوند.','Xet bi awayekî parabolîk diçe, ji ber vê yekê guherînên biçûk di destpêkê de paşê pir xurt dibin.','Krzywa ma przebieg paraboliczny, dlatego niewielkie zmiany na początku później silnie się wzmacniają.','Curba are o evoluție parabolică, de aceea mici modificări la început sunt amplificate puternic mai târziu.','Кривая имеет параболический вид, поэтому небольшие изменения в начале позже сильно усиливаются.','Kurbja zhvillohet në mënyrë parabolike, prandaj ndryshimet e vogla në fillim më vonë përforcohen shumë.','Eğri parabolik ilerler; bu yüzden baştaki küçük değişiklikler daha sonra güçlü biçimde büyür.')),
      ex('Die Novelle ist parabolisch angelegt und verhandelt Machtmissbrauch nicht direkt, sondern über ein scheinbar einfaches Dorfereignis.', trans('صُممت الرواية القصيرة بصورة رمزية وتناقش إساءة استخدام السلطة لا مباشرة، بل عبر حدث قروي بسيط ظاهرياً.','نوڤێلەکە بە شێوەیەکی تمثیلی دانراوە و خراپ بەکارهێنانی دەسەڵات ڕاستەوخۆ باس ناکات، بەڵکو لە ڕێگەی ڕووداوێکی گوندیی بە ڕواڵەت سادەوە.','The novella is structured parabolically and addresses abuse of power not directly, but through an apparently simple village event.','داستان کوتاه ساختاری تمثیلی دارد و سوءاستفاده از قدرت را نه مستقیم، بلکه از طریق رویدادی ظاهراً ساده در یک روستا بررسی می‌کند.','Novella bi awayekî alegorîk hatiye avakirin û xerab bikaranîna desthilatê ne rasterast, lê bi bûyereke gundî ya xuya sade vedibêje.','Nowela ma konstrukcję paraboliczną i omawia nadużycie władzy nie bezpośrednio, lecz przez pozornie proste wydarzenie wiejskie.','Nuvela este construită parabolic și tratează abuzul de putere nu direct, ci printr-un eveniment rural aparent simplu.','Новелла построена как притча и говорит о злоупотреблении властью не напрямую, а через, на первый взгляд, простое деревенское событие.','Novela është ndërtuar në mënyrë alegorike dhe trajton abuzimin me pushtetin jo drejtpërdrejt, por përmes një ngjarjeje fshati në dukje të thjeshtë.','Novella parabolik biçimde kurulmuş ve güç istismarını doğrudan değil, görünüşte basit bir köy olayı üzerinden ele alıyor.'))
    ]
  }),
  entry({
    word: 'das Paradoxon', partOfSpeech: 'Noun', article: 'das', plural: 'Paradoxien', syllableBreak: 'Pa-ra-do-xon',
    topics: ['advanced-analysis','business-communication','education-and-training'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'ein scheinbares Paradoxon', meaning: 'an apparent paradox' }],
    meanings: meaning('مفارقة؛ تناقض ظاهري','پارادۆکس؛ دژبەیەکی دیار','paradox; apparent contradiction','پارادوکس؛ تناقض‌نما','paradoks; dijberiya xuya','paradoks','paradox','парадокс','paradoks','paradoks'),
    examples: [
      ex('Das Paradoxon besteht darin, dass strengere Kontrollen die Qualität erhöhten, aber zugleich die Innovationsgeschwindigkeit senkten.', trans('تكمن المفارقة في أن الضوابط الأشد رفعت الجودة، لكنها في الوقت نفسه خفّضت سرعة الابتكار.','پارادۆکسەکە لەوەدایە کە کۆنتڕۆڵی توندتر کوالیتی زیاد کرد، بەڵام هەمان کات خێرایی داهێنانی کەمکردەوە.','The paradox is that stricter controls improved quality but at the same time reduced the speed of innovation.','پارادوکس این است که کنترل‌های سخت‌گیرانه‌تر کیفیت را بالا بردند، اما هم‌زمان سرعت نوآوری را کاهش دادند.','Paradoks ev e ku kontrolên tundtir kalîte bilind kirin, lê di heman demê de leza nûjeniyê kêm kirin.','Paradoks polega na tym, że surowsze kontrole podniosły jakość, ale zarazem obniżyły tempo innowacji.','Paradoxul constă în faptul că verificările mai stricte au crescut calitatea, dar au redus în același timp viteza inovației.','Парадокс в том, что более строгий контроль повысил качество, но одновременно снизил скорость инноваций.','Paradoksi qëndron në faktin se kontrollet më të rrepta rritën cilësinë, por njëkohësisht ulën shpejtësinë e inovacionit.','Paradoks şu ki, daha sıkı kontroller kaliteyi artırdı ama aynı zamanda inovasyon hızını düşürdü.')),
      ex('In der Debatte wurde das Paradoxon sichtbar, dass alle Transparenz forderten, aber niemand eigene Machtressourcen offenlegen wollte.', trans('في النقاش ظهرت المفارقة أن الجميع طالبوا بالشفافية، لكن لا أحد أراد كشف مصادر قوته الخاصة.','لە گفتوگۆکەدا پارادۆکسەکە دیار بوو کە هەمووان داوای شەفافییان دەکرد، بەڵام کەس نەیدەویست سەرچاوەکانی دەسەڵاتی خۆی ئاشکرا بکات.','In the debate, the paradox became visible that everyone demanded transparency, but no one wanted to disclose their own power resources.','در بحث، این پارادوکس آشکار شد که همه خواهان شفافیت بودند، اما هیچ‌کس نمی‌خواست منابع قدرت خود را افشا کند.','Di nîqaşê de paradoks xuya bû ku hemû kes şefafiyet dixwestin, lê kesek nexwest çavkaniyên hêza xwe eşkere bike.','W debacie uwidocznił się paradoks, że wszyscy domagali się przejrzystości, ale nikt nie chciał ujawnić własnych zasobów władzy.','În dezbatere a devenit vizibil paradoxul că toți cereau transparență, dar nimeni nu voia să își dezvăluie propriile resurse de putere.','В дебатах проявился парадокс: все требовали прозрачности, но никто не хотел раскрывать собственные ресурсы влияния.','Në debat u bë i dukshëm paradoksi se të gjithë kërkonin transparencë, por askush nuk donte të zbulonte burimet e veta të pushtetit.','Tartışmada herkesin şeffaflık istediği, ama kimsenin kendi güç kaynaklarını açıklamak istemediği paradoks ortaya çıktı.'))
    ]
  }),
  entry({
    word: 'paraphrasieren', partOfSpeech: 'Verb', infinitive: 'paraphrasieren', syllableBreak: 'pa-ra-phra-sie-ren',
    topics: ['education-and-training','business-communication','documents-and-administration'], usageLabels: ['formal','written','academic','business'],
    collocations: [{ text: 'eine Aussage paraphrasieren', meaning: 'to paraphrase a statement' }],
    meanings: meaning('يعيد الصياغة؛ يشرح بعبارة أخرى','بە وشەی دیکە گوتنەوە؛ دووبارە داڕشتن','to paraphrase; to rephrase','بازنویسی کردن؛ به بیان دیگر گفتن','ji nû ve bi gotinên din vegotin','parafrazować','a parafraza; a reformula','перефразировать','parafrazoj; riformuloj','başka sözlerle ifade etmek; parafraz etmek'),
    examples: [
      ex('Bitte paraphrasieren Sie die Kundenbeschwerde, bevor Sie eine Lösung vorschlagen, damit klar ist, dass Sie das Anliegen verstanden haben.', trans('يرجى إعادة صياغة شكوى العميل قبل اقتراح حل، حتى يتضح أنك فهمت الطلب.','تکایە سکاڵای کڕیارەکە بە وشەی دیکە بڵێنەوە پێش ئەوەی چارەسەر پێشنیار بکەن، بۆ ئەوەی ڕوون بێت کە داواکارییەکەتان تێگەیشتووە.','Please paraphrase the customer complaint before suggesting a solution so it is clear that you have understood the concern.','لطفاً پیش از پیشنهاد راه‌حل، شکایت مشتری را با بیان دیگر بازگو کنید تا روشن شود موضوع را فهمیده‌اید.','Ji kerema xwe berî ku çareseriyek pêşniyar bikin, gilîya xerîdarê bi gotinên din vegêrin da ku xuya be hûn daxwazê fam kirine.','Proszę sparafrazować skargę klienta przed zaproponowaniem rozwiązania, aby było jasne, że zrozumieli Państwo problem.','Vă rog să parafrazați reclamația clientului înainte de a propune o soluție, ca să fie clar că ați înțeles problema.','Пожалуйста, перефразируйте жалобу клиента перед тем, как предложить решение, чтобы было ясно, что вы поняли суть обращения.','Ju lutem parafrazoni ankesën e klientit para se të propozoni një zgjidhje, që të jetë e qartë se e keni kuptuar shqetësimin.','Lütfen çözüm önermeden önce müşteri şikayetini başka sözlerle ifade edin ki konuyu anladığınız açık olsun.')),
      ex('Die Studierenden sollten den theoretischen Absatz nicht kopieren, sondern präzise paraphrasieren und anschließend kritisch einordnen.', trans('كان على الطلاب ألا ينسخوا الفقرة النظرية، بل يعيدوا صياغتها بدقة ثم يضعوها في سياق نقدي.','خوێندکاران دەبوو پاراگرافە تیۆرییەکە کۆپی نەکەن، بەڵکو بە وردی دووبارەی دابڕێژن و پاشان بە ڕەخنەوە لە چوارچێوەدا دابنێن.','The students were supposed not to copy the theoretical paragraph, but to paraphrase it precisely and then place it in a critical context.','دانشجویان نباید بند نظری را کپی می‌کردند، بلکه باید آن را دقیق بازنویسی کرده و سپس به‌صورت انتقادی جایگاهش را مشخص می‌کردند.','Xwendekar diviya paragrafê teorîk kopî nekin, lê bi rastî paraphrase bikin û paşê bi rexneyî cih bidin.','Studenci nie mieli kopiować akapitu teoretycznego, lecz precyzyjnie go sparafrazować, a następnie krytycznie umiejscowić.','Studenții nu trebuiau să copieze paragraful teoretic, ci să îl parafrazeze precis și apoi să îl contextualizeze critic.','Студенты должны были не копировать теоретический абзац, а точно перефразировать его и затем критически contextualizeировать.','Studentët nuk duhej ta kopjonin paragrafin teorik, por ta parafrazonin saktë dhe pastaj ta vendosnin në një kontekst kritik.','Öğrencilerin teorik paragrafı kopyalamaları değil, onu doğru biçimde başka sözlerle ifade edip ardından eleştirel olarak konumlandırmaları gerekiyordu.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 062', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

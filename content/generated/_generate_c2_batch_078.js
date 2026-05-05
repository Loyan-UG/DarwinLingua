const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '078';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['das Sujet','die Syntax','tänzelnd','tangieren','die Textgestalt','die Textlinguistik'];

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
    word: 'das Sujet', partOfSpeech: 'Noun', article: 'das', plural: 'Sujets', syllableBreak: 'Su-jet',
    topics: ['culture-and-media','advanced-analysis','education-and-training'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'ein literarisches Sujet', meaning: 'a literary subject or theme' }],
    meanings: meaning('موضوع العمل الفني؛ ثيمة','بابەتی هونەری؛ مۆتیف','subject; theme of an artwork','موضوع اثر؛ مضمون هنری','mijara hunerî; tema','temat; motyw dzieła','subiect artistic; temă','сюжет; тема произведения','temë artistike; subjekt','konu; sanatsal tema'),
    examples: [
      ex('Das Sujet des Romans ist nicht die Flucht selbst, sondern die langsame Erosion von Zugehörigkeit.', meaning('موضوع الرواية ليس الهروب نفسه، بل التآكل البطيء للانتماء.','بابەتی ڕۆمانەکە خۆی هەڵاتن نییە، بەڵکو خاوخاو لەناوچوونی هەست بە سەر بە شوێنێک بوونە.','The subject of the novel is not the flight itself, but the slow erosion of belonging.','موضوع رمان خودِ فرار نیست، بلکه فرسایش آرام حس تعلق است.','Mijara romanê ne xwe rev e, lê hilweşîna hêdî ya girêdayîbûnê ye.','Tematem powieści nie jest sama ucieczka, lecz powolna erozja przynależności.','Subiectul romanului nu este fuga în sine, ci erodarea lentă a apartenenței.','Тема романа не сам побег, а медленная эрозия принадлежности.','Tema e romanit nuk është vetë ikja, por erozioni i ngadalshëm i përkatësisë.','Romanın konusu kaçışın kendisi değil, aidiyetin yavaş aşınmasıdır.')),
      ex('Für die Kampagne wurde ein heikles Sujet gewählt, das ohne präzise Tonalität schnell zynisch gewirkt hätte.', meaning('اختير للحملة موضوع حساس كان سيبدو ساخراً بسرعة لولا نبرة دقيقة.','بۆ کەمپەینەکە بابەتێکی هەستیار هەڵبژێردرا کە بەبێ تۆنی ورد خێرا وەک زینیك دەردەکەوت.','A delicate subject was chosen for the campaign; without precise tonality, it would quickly have seemed cynical.','برای کمپین موضوعی حساس انتخاب شد که بدون لحن دقیق سریعاً بدبینانه و زننده به نظر می‌رسید.','Ji bo kampanyayê mijareke hestyar hate hilbijartin ku bê tonalîteya rast zû wek zînîk xuya bikira.','Do kampanii wybrano delikatny temat, który bez precyzyjnej tonacji szybko zabrzmiałby cynicznie.','Pentru campanie a fost ales un subiect sensibil care, fără o tonalitate precisă, ar fi părut rapid cinic.','Для кампании выбрали деликатную тему, которая без точной тональности быстро выглядела бы циничной.','Për fushatën u zgjodh një temë delikate që pa tonalitet të saktë do të dukej shpejt cinike.','Kampanya için hassas bir konu seçildi; doğru tonlama olmadan hızla sinik görünebilirdi.'))
    ]
  }),
  entry({
    word: 'die Syntax', partOfSpeech: 'Noun', article: 'die', plural: 'Syntaxen', syllableBreak: 'Syn-tax',
    topics: ['education-and-training','technology-and-it','advanced-analysis'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'die Syntax einer Programmiersprache', meaning: 'the syntax of a programming language' }],
    meanings: meaning('النحو التركيبي؛ قواعد ترتيب الجملة أو الكود','سینتاکس؛ ڕێکخستنی ڕستە یان کۆد','syntax; rules of sentence or code structure','نحو؛ قواعد ساخت جمله یا کد','sîntaks; rêzikên avahiya hevok an kodê','składnia; syntaksa','sintaxă','синтаксис','sintaksë','sözdizimi; sentaks'),
    examples: [
      ex('Der Compiler akzeptierte die Syntax, aber die fachliche Logik der Berechnung war trotzdem falsch.', meaning('قبل المترجم الصياغة النحوية للكود، لكن المنطق المهني للحساب كان مع ذلك خاطئاً.','کۆمپایلەرەکە سینتاکسەکەی پەسەند کرد، بەڵام لۆژیکی پسپۆڕیی ژمێرینەکە هێشتا هەڵە بوو.','The compiler accepted the syntax, but the business logic of the calculation was still wrong.','کامپایلر نحو کد را پذیرفت، اما منطق تخصصی محاسبه همچنان غلط بود.','Compilerê sîntaks pejirand, lê lojîka pisporî ya hesabê hîn jî şaş bû.','Kompilator zaakceptował składnię, ale logika biznesowa obliczenia nadal była błędna.','Compilatorul a acceptat sintaxa, dar logica de business a calculului era totuși greșită.','Компилятор принял синтаксис, но бизнес-логика расчета все равно была ошибочной.','Kompiluesi e pranoi sintaksën, por logjika e biznesit e llogaritjes ishte ende e gabuar.','Derleyici sözdizimini kabul etti, ancak hesaplamanın iş mantığı yine de yanlıştı.')),
      ex('Die verschachtelte Syntax des Absatzes zwingt den Leser, die zeitlichen Ebenen genau zu unterscheiden.', meaning('يجبر التركيب المتداخل للفقرة القارئ على التمييز بدقة بين المستويات الزمنية.','سینتاکسی ئاڵۆزی پاراگرافەکە خوێنەر ناچار دەکات ئاستە کاتییەکان بە وردی جیا بکاتەوە.','The nested syntax of the paragraph forces the reader to distinguish the temporal levels precisely.','نحو تودرتوی بند خواننده را مجبور می‌کند لایه‌های زمانی را دقیق از هم جدا کند.','Sîntaksa tevlihev a paragrafê xwendevan neçar dike astên demê bi hûrgulî cuda bike.','Zagnieżdżona składnia akapitu zmusza czytelnika do dokładnego rozróżnienia poziomów czasowych.','Sintaxa încâlcită a paragrafului îl obligă pe cititor să distingă precis nivelurile temporale.','Вложенный синтаксис абзаца заставляет читателя точно различать временные уровни.','Sintaksa e ndërthurur e paragrafit e detyron lexuesin të dallojë saktë nivelet kohore.','Paragrafın iç içe sözdizimi okuru zamansal düzeyleri dikkatle ayırt etmeye zorlar.'))
    ]
  }),
  entry({
    word: 'tänzelnd', partOfSpeech: 'Adjective', syllableBreak: 'tän-zelnd',
    topics: ['culture-and-media','everyday-life','advanced-analysis'], usageLabels: ['formal','written','advanced','analysis'],
    collocations: [{ text: 'tänzelnde Schritte', meaning: 'light, dancing steps' }],
    meanings: meaning('راقص بخفة؛ متمايل بخطوات صغيرة','بە هەنگاوی سووک و شاد؛ وەک سەما','lightly dancing; prancing','رقصان و سبک‌پا','bi gavên sivik wek dans','taneczny; podrygujący','dansant; săltăreț','пританцовывающий','duke vallëzuar lehtë','dans eder gibi; sekerek'),
    examples: [
      ex('Der Moderator betrat tänzelnd die Bühne, doch die lockere Geste passte kaum zur angespannten Lage des Unternehmens.', meaning('دخل المقدم المسرح بخطوات راقصة خفيفة، لكن هذه الإيماءة المرحة بالكاد ناسبت وضع الشركة المتوتر.','پێشکەشکارەکە بە هەنگاوی سەما ئاسا چووە سەر شانۆ، بەڵام ئەو ئاماژەیەی ئازادانە زۆر نەگونجا لەگەڵ دۆخی گرژی کۆمپانیا.','The host entered the stage with light, dancing steps, but the relaxed gesture hardly fit the company’s tense situation.','مجری سبک‌پا و رقصان وارد صحنه شد، اما این ژست آزادانه چندان با وضعیت پرتنش شرکت جور نبود.','Pêşkêşkar bi gavên dansê ket ser sehneyê, lê ew gesta rehet zêde bi rewşa teng a şirketê re neguncî.','Moderator wszedł na scenę tanecznym krokiem, lecz luźny gest słabo pasował do napiętej sytuacji firmy.','Moderatorul a intrat pe scenă cu pași dansanți, dar gestul relaxat se potrivea prea puțin cu situația tensionată a companiei.','Ведущий вышел на сцену пританцовывая, но непринужденный жест едва соответствовал напряженной ситуации компании.','Moderatori hyri në skenë me hapa vallëzues, por gjesti i lirshëm vështirë se përputhej me situatën e tensionuar të kompanisë.','Sunucu sahneye dans eder gibi adımlarla girdi, ancak rahat jest şirketin gergin durumuna pek uymadı.')),
      ex('Die Kamera folgt dem tänzelnden Kind, bis die leichte Bewegung plötzlich in Stillstand umschlägt.', meaning('تتبع الكاميرا الطفل الراقص بخفة إلى أن تتحول الحركة الخفيفة فجأة إلى سكون.','کامێرا بەدوای منداڵە بە هەنگاوی سووکەکە دەکەوێت، تا جوڵەی سووکەکە لەناکاو دەبێتە وەستان.','The camera follows the lightly dancing child until the gentle movement suddenly turns into stillness.','دوربین کودک سبک‌پا را دنبال می‌کند تا حرکت نرم ناگهان به سکون تبدیل می‌شود.','Kamera li dû zarokê bi gavên dansê diçe heta tevgera sivik ji nişkê ve dibe rawestîn.','Kamera podąża za podrygującym dzieckiem, aż lekki ruch nagle przechodzi w bezruch.','Camera urmărește copilul care pășește dansant până când mișcarea ușoară se transformă brusc în nemișcare.','Камера следует за пританцовывающим ребенком, пока легкое движение внезапно не переходит в неподвижность.','Kamera ndjek fëmijën që lëviz lehtë si në vallëzim derisa lëvizja e lehtë papritur kthehet në palëvizshmëri.','Kamera dans eder gibi ilerleyen çocuğu izler; hafif hareket aniden durgunluğa dönüşür.'))
    ]
  }),
  entry({
    word: 'tangieren', partOfSpeech: 'Verb', infinitive: 'tangieren', syllableBreak: 'tan-gie-ren',
    topics: ['law-and-compliance','business-communication','advanced-analysis'], usageLabels: ['formal','written','business','analysis'],
    collocations: [{ text: 'Interessen tangieren', meaning: 'to affect interests' }],
    meanings: meaning('يمس؛ يؤثر في','دەست لێدان؛ کاریگەری هەبوون لەسەر','to affect; to touch upon','مرتبط و اثرگذار بودن بر؛ تماس داشتن با','bandor kirin; dest lê dan','dotyczyć; naruszać','a afecta; a atinge','затрагивать','prek; ndikoj mbi','etkilemek; ilgilendirmek'),
    examples: [
      ex('Die geplante Schnittstelle tangiert personenbezogene Daten und muss deshalb vor der Umsetzung geprüft werden.', meaning('تمس الواجهة المخطط لها بيانات شخصية ولذلك يجب فحصها قبل التنفيذ.','ئینتەرفەیسی پلاندانراو دەست لە داتای کەسی دەدات، بۆیە دەبێت پێش جێبەجێکردن پشکنین بکرێت.','The planned interface affects personal data and therefore must be reviewed before implementation.','رابط برنامه‌ریزی‌شده با داده‌های شخصی سروکار دارد و بنابراین باید پیش از اجرا بررسی شود.','Navrûya plansazkirî bandor li daneyên kesane dike û ji ber vê divê berî cîbicîkirinê were kontrolkirin.','Planowany interfejs dotyczy danych osobowych i dlatego musi zostać sprawdzony przed wdrożeniem.','Interfața planificată afectează date cu caracter personal și trebuie verificată înainte de implementare.','Планируемый интерфейс затрагивает персональные данные и поэтому должен быть проверен до реализации.','Ndërfaqja e planifikuar prek të dhëna personale dhe prandaj duhet kontrolluar para zbatimit.','Planlanan arayüz kişisel verileri etkiliyor ve bu nedenle uygulamadan önce incelenmelidir.')),
      ex('Die Entscheidung tangiert auch die Frage, wer im Text überhaupt als glaubwürdiger Zeuge erscheinen darf.', meaning('يمس القرار أيضاً سؤال من يحق له أصلاً أن يظهر في النص كشاهد موثوق.','بڕیارەکە ئەو پرسیارەش دەست لێدەدات کە کێ لە دەقەکەدا دەتوانێت وەک شایەتی باوەڕپێکراو دەربکەوێت.','The decision also touches on the question of who may appear in the text as a credible witness at all.','این تصمیم همچنین به این پرسش مربوط می‌شود که چه کسی اصلاً می‌تواند در متن شاهدی معتبر به نظر برسد.','Biryara jî dest dide pirsa ka kî dikare di nivîsê de wek şahidê bawerbar xuya bike.','Decyzja dotyczy także pytania, kto w tekście w ogóle może wystąpić jako wiarygodny świadek.','Decizia atinge și întrebarea cine poate apărea în text ca martor credibil.','Решение затрагивает и вопрос о том, кто вообще может предстать в тексте как достоверный свидетель.','Vendimi prek edhe pyetjen se kush mund të shfaqet në tekst si dëshmitar i besueshëm.','Karar, metinde kimin güvenilir bir tanık olarak görünebileceği sorusuna da temas eder.'))
    ]
  }),
  entry({
    word: 'die Textgestalt', partOfSpeech: 'Noun', article: 'die', plural: 'Textgestalten', syllableBreak: 'Text-ge-stalt',
    topics: ['culture-and-media','education-and-training','documents-and-administration'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'die endgültige Textgestalt', meaning: 'the final form of the text' }],
    meanings: meaning('شكل النص؛ الصيغة النهائية للنص','شێوەی دەق؛ فۆڕمی کۆتایی دەق','textual form; final shape of a text','صورت متن؛ شکل نهایی متن','şêweya nivîsê; forma dawî','postać tekstu; kształt tekstu','formă textuală','текстовая форма','formë teksti','metin biçimi'),
    examples: [
      ex('Nach der juristischen Prüfung erhielt die Datenschutzerklärung eine neue Textgestalt, ohne dass die Pflichten verändert wurden.', meaning('بعد المراجعة القانونية حصل بيان الخصوصية على صيغة نصية جديدة من دون تغيير الالتزامات.','دوای پشکنینی یاسایی، ڕاگەیاندنی پاراستنی داتا شێوەی دەقی نوێی وەرگرت، بەبێ ئەوەی ئەرکەکان بگۆڕدرێن.','After the legal review, the privacy statement received a new textual form without the obligations being changed.','پس از بررسی حقوقی، بیانیه حریم خصوصی صورت متنی تازه‌ای گرفت، بدون آن‌که تعهدات تغییر کند.','Piştî kontrola yasayî, daxuyaniya parastina daneyan şêweya nivîsê ya nû wergirt bê ku erk bên guherandin.','Po kontroli prawnej oświadczenie o ochronie danych otrzymało nową postać tekstową, bez zmiany obowiązków.','După verificarea juridică, declarația de confidențialitate a primit o nouă formă textuală fără ca obligațiile să fie modificate.','После юридической проверки заявление о конфиденциальности получило новую текстовую форму без изменения обязанностей.','Pas kontrollit ligjor, deklarata e privatësisë mori një formë të re teksti pa ndryshuar detyrimet.','Hukuki incelemeden sonra gizlilik bildirimi, yükümlülükler değişmeden yeni bir metin biçimi aldı.')),
      ex('Die überlieferte Textgestalt ist instabil, weil mehrere Abschriften voneinander abweichen.', meaning('الصيغة النصية المنقولة غير مستقرة لأن عدة نسخ تختلف بعضها عن بعض.','شێوەی دەقی گوازراوە ناجێگیرە، چونکە چەند کۆپییەک لە یەکتر جیاوازن.','The transmitted textual form is unstable because several copies differ from one another.','صورت متنِ منتقل‌شده ناپایدار است، چون چند نسخه از یکدیگر متفاوت‌اند.','Şêweya nivîsê ya veguhestî ne aram e, ji ber ku çend nusxe ji hev cuda ne.','Przekazana postać tekstu jest niestabilna, ponieważ kilka odpisów różni się od siebie.','Forma textuală transmisă este instabilă deoarece mai multe copii diferă între ele.','Переданная текстовая форма нестабильна, потому что несколько списков отличаются друг от друга.','Forma e përcjellë e tekstit është e paqëndrueshme, sepse disa kopje ndryshojnë nga njëra-tjetra.','Aktarılan metin biçimi kararsızdır, çünkü birkaç nüsha birbirinden farklıdır.'))
    ]
  }),
  entry({
    word: 'die Textlinguistik', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Text-lin-gu-is-tik',
    topics: ['education-and-training','advanced-analysis','business-communication'], usageLabels: ['formal','written','academic','analysis'], grammarNotes: ['feminine academic field name; normally used in singular'],
    collocations: [{ text: 'aus textlinguistischer Sicht', meaning: 'from a text-linguistic perspective' }],
    meanings: meaning('لسانيات النص؛ علم اللغة النصي','دەق‌زمانناسی؛ زمانناسی دەق','text linguistics','زبان‌شناسی متن','zimanvaniya nivîsê','lingwistyka tekstu','lingvistică textuală','лингвистика текста','gjuhësi e tekstit','metindilbilim'),
    examples: [
      ex('Die Textlinguistik hilft dabei, Supportartikel nicht nur korrekt, sondern auch kohärent und navigierbar zu schreiben.', meaning('تساعد لسانيات النص على كتابة مقالات الدعم ليس فقط بشكل صحيح، بل أيضاً متماسكة وسهلة التصفح.','دەق‌زمانناسی یارمەتیدەرە بۆ نووسینی وتاری پشتگیری نەک تەنها بە دروستی، بەڵکو بە هاوئاهەنگی و ئاسانگەرێکی گەڕان.','Text linguistics helps write support articles that are not only correct, but also coherent and navigable.','زبان‌شناسی متن کمک می‌کند مقاله‌های پشتیبانی نه فقط درست، بلکه منسجم و قابل پیمایش نوشته شوند.','Zimanvaniya nivîsê alîkar e ku gotarên piştgiriyê ne tenê rast, lê jî hevaheng û hêsan ji bo gerînê bên nivîsandin.','Lingwistyka tekstu pomaga pisać artykuły wsparcia nie tylko poprawnie, lecz także spójnie i łatwo do nawigacji.','Lingvistica textuală ajută la redactarea articolelor de suport nu doar corect, ci și coerent și ușor de navigat.','Лингвистика текста помогает писать статьи поддержки не только правильно, но и связно, с удобной навигацией.','Gjuhësia e tekstit ndihmon që artikujt e suportit të shkruhen jo vetëm saktë, por edhe koherentë dhe të lehtë për t’u naviguar.','Metindilbilim, destek makalelerinin yalnızca doğru değil, aynı zamanda tutarlı ve gezilebilir yazılmasına yardımcı olur.')),
      ex('Im Seminar zeigte die Textlinguistik, warum ein Absatz trotz korrekter Grammatik unverständlich bleiben kann.', meaning('أظهرت لسانيات النص في الحلقة الدراسية لماذا يمكن أن تبقى فقرة غير مفهومة رغم صحة قواعدها.','لە سیمینارەکەدا دەق‌زمانناسی پیشانی دا بۆچی پاراگرافێک سەرەڕای گرامەری دروست دەتوانێت نەفهمراو بمێنێتەوە.','In the seminar, text linguistics showed why a paragraph can remain incomprehensible despite correct grammar.','در سمینار، زبان‌شناسی متن نشان داد چرا یک بند با وجود دستور زبان درست می‌تواند نامفهوم بماند.','Di seminarê de zimanvaniya nivîsê nîşan da ka çima paragrafek tevî gramera rast dikare nezelal bimîne.','Na seminarium lingwistyka tekstu pokazała, dlaczego akapit mimo poprawnej gramatyki może pozostać niezrozumiały.','La seminar, lingvistica textuală a arătat de ce un paragraf poate rămâne neinteligibil în ciuda gramaticii corecte.','На семинаре лингвистика текста показала, почему абзац при правильной грамматике может оставаться непонятным.','Në seminar, gjuhësia e tekstit tregoi pse një paragraf mund të mbetet i pakuptueshëm pavarësisht gramatikës së saktë.','Seminerde metindilbilim, doğru dilbilgisine rağmen bir paragrafın neden anlaşılmaz kalabileceğini gösterdi.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 078', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

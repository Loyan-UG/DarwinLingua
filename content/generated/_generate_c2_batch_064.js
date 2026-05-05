const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '064';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['pervertieren','die Phänomenologie','pointiert','die Pointierung','polemisch','die Polyphonie'];

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
    word: 'pervertieren', partOfSpeech: 'Verb', infinitive: 'pervertieren', syllableBreak: 'per-ver-tie-ren',
    topics: ['law-and-compliance','management-and-leadership','advanced-analysis'], usageLabels: ['formal','written','advanced','sensitive'],
    collocations: [{ text: 'ein Prinzip pervertieren', meaning: 'to distort or corrupt a principle' }],
    meanings: meaning('يفسد؛ يحرف عن غايته','خراپکردن؛ لە مانای ڕاستی لادان','to pervert; to distort or corrupt','منحرف کردن؛ تباه کردن','xerab kirin; ji armanca xwe derxistin','wypaczać; degenerować','a perverti; a denatura','извращать; искажать','shtrembëroj; degjeneroj','çarpıtmak; yozlaştırmak'),
    examples: [
      ex('Wenn Kennzahlen nur noch zur Absicherung von Macht dienen, pervertieren sie den ursprünglichen Zweck des Controllings.', trans('عندما لا تعود المؤشرات تخدم إلا حماية السلطة، فإنها تحرف الغاية الأصلية للرقابة الإدارية.','کاتێک پێوەرەکان تەنها بۆ پاراستنی دەسەڵات بەکاردێن، ئەوا ئامانجی سەرەتایی کۆنتڕۆڵینگ دەشێوێنن.','When metrics serve only to secure power, they pervert the original purpose of controlling.','وقتی شاخص‌ها فقط برای تثبیت قدرت به کار می‌روند، هدف اصلی کنترل مدیریتی را منحرف می‌کنند.','Dema nîşane tenê ji bo parastina hêzê tên bikaranîn, armanca bingehîn a kontrolê pervert dikin.','Gdy wskaźniki służą już tylko zabezpieczeniu władzy, wypaczają pierwotny cel controllingu.','Când indicatorii servesc doar la consolidarea puterii, ei pervertesc scopul inițial al controllingului.','Когда показатели служат лишь укреплению власти, они извращают первоначальную цель контроллинга.','Kur treguesit shërbejnë vetëm për sigurimin e pushtetit, ata shtrembërojnë qëllimin fillestar të kontrollit menaxherial.','Göstergeler yalnızca iktidarı güvenceye almaya hizmet ettiğinde, kontrolün asıl amacını çarpıtır.')),
      ex('Der Roman zeigt, wie eine Befreiungsidee durch Fanatismus pervertiert werden kann.', trans('تُظهر الرواية كيف يمكن أن تُفسد فكرة التحرر بفعل التعصب.','ڕۆمانەکە پیشان دەدات چۆن بیرۆکەی ڕزگاربوون دەتوانێت بە فاناتیزم بشێوێندرێت.','The novel shows how an idea of liberation can be perverted by fanaticism.','رمان نشان می‌دهد چگونه یک ایده رهایی‌بخش می‌تواند به‌واسطه تعصب منحرف شود.','Roman nîşan dide ka çawa fikra azadkirinê dikare bi fanatîzmê were xerabkirin.','Powieść pokazuje, jak idea wyzwolenia może zostać wypaczona przez fanatyzm.','Romanul arată cum o idee de eliberare poate fi pervertită de fanatism.','Роман показывает, как идея освобождения может быть извращена фанатизмом.','Romani tregon se si një ide çlirimi mund të shtrembërohet nga fanatizmi.','Roman, bir özgürleşme fikrinin fanatizm tarafından nasıl çarpıtılabileceğini gösterir.'))
    ]
  }),
  entry({
    word: 'die Phänomenologie', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Phä-no-me-no-lo-gie',
    topics: ['education-and-training','advanced-analysis','culture-and-media'], usageLabels: ['formal','written','academic','analysis'],
    grammarNotes: ['feminine abstract noun; normally used in singular'],
    collocations: [{ text: 'die Phänomenologie der Wahrnehmung', meaning: 'the phenomenology of perception' }],
    meanings: meaning('الفينومينولوجيا؛ دراسة الخبرة كما تظهر','فینۆمێنۆلۆژیا؛ لێکۆڵینەوەی ئەزموون وەک دەردەکەوێت','phenomenology','پدیدارشناسی','fenomenolojî','fenomenologia','fenomenologie','феноменология','fenomenologji','fenomenoloji'),
    examples: [
      ex('In der Nutzerforschung hilft eine vorsichtige Phänomenologie dabei, nicht nur Klicks, sondern erlebte Unsicherheit zu beschreiben.', trans('في أبحاث المستخدمين تساعد فينومينولوجيا حذرة على وصف ليس النقرات فقط، بل أيضاً انعدام الأمان كما يُعاش.','لە توێژینەوەی بەکارهێنەردا فینۆمێنۆلۆژیایەکی وریا یارمەتیدەرە بۆ وەسفکردنی نەک تەنها کلیکەکان، بەڵکو نادڵنیایی ئەزموونکراو.','In user research, a careful phenomenology helps describe not only clicks but also experienced uncertainty.','در پژوهش کاربر، پدیدارشناسی محتاطانه کمک می‌کند نه فقط کلیک‌ها، بلکه نااطمینانی زیسته نیز توصیف شود.','Di lêkolîna bikarhêneran de fenomenolojiyeke hişyar alîkar e ku ne tenê klik, lê jî bêewlehiya jiyandî were ravekirin.','W badaniach użytkowników ostrożna fenomenologia pomaga opisać nie tylko kliknięcia, lecz także doświadczaną niepewność.','În cercetarea utilizatorilor, o fenomenologie prudentă ajută la descrierea nu doar a clicurilor, ci și a nesiguranței trăite.','В пользовательских исследованиях осторожная феноменология помогает описывать не только клики, но и переживаемую неуверенность.','Në kërkimin mbi përdoruesit, një fenomenologji e kujdesshme ndihmon të përshkruhen jo vetëm klikimet, por edhe pasiguria e përjetuar.','Kullanıcı araştırmasında dikkatli bir fenomenoloji, yalnızca tıklamaları değil, yaşanan belirsizliği de betimlemeye yardımcı olur.')),
      ex('Die Vorlesung führte in die Phänomenologie ein, ohne sie auf eine bloße Methode der Selbstbeobachtung zu reduzieren.', trans('قدّمت المحاضرة مدخلاً إلى الفينومينولوجيا من دون اختزالها في مجرد طريقة للملاحظة الذاتية.','وانەکە پێشەکییەک بۆ فینۆمێنۆلۆژیا بوو، بەبێ ئەوەی بیکاتە تەنها شێوازێکی خۆچاودێری.','The lecture introduced phenomenology without reducing it to a mere method of introspection.','درس پدیدارشناسی را معرفی کرد، بدون آن‌که آن را به روشی صرف برای درون‌نگری تقلیل دهد.','Dersê destpêkek ji fenomenolojiyê da bê ku wê tenê wek rêbazek xwe-nêrînê kêm bike.','Wykład wprowadzał w fenomenologię, nie redukując jej do zwykłej metody samoobserwacji.','Cursul a introdus fenomenologia fără a o reduce la o simplă metodă de autoobservare.','Лекция вводила в феноменологию, не сводя ее к простой методике самонаблюдения.','Ligjërata e prezantoi fenomenologjinë pa e reduktuar në një metodë të thjeshtë vetëvëzhgimi.','Ders, fenomenolojiyi salt bir iç gözlem yöntemine indirgemeden tanıttı.'))
    ]
  }),
  entry({
    word: 'pointiert', partOfSpeech: 'Adjective', syllableBreak: 'poin-tiert',
    topics: ['business-communication','culture-and-media','advanced-analysis'], usageLabels: ['formal','written','analysis','business'],
    collocations: [{ text: 'pointiert formulieren', meaning: 'to formulate pointedly and concisely' }],
    meanings: meaning('موجز وحاد التعبير؛ دقيق العبارة','کورت و کاریگەر؛ بە خاڵی گرنگ','pointed; concise and striking','دقیق و گزیده؛ برجسته و اثرگذار','kurte û bandorker; bi xala sereke','celny; dobitny','concis și pregnant','заостренный; меткий','i mprehtë; i përmbledhur','çarpıcı ve özlü'),
    examples: [
      ex('Die Beraterin fasste den Konflikt pointiert zusammen: Das Team hatte kein Ressourcenproblem, sondern ein Entscheidungsproblem.', trans('لخّصت المستشارة النزاع بدقة لافتة: لم تكن لدى الفريق مشكلة موارد، بل مشكلة قرار.','ڕاوێژکارەکە ململاێکەی بە کورت و کاریگەر کورتکردەوە: تیمەکە کێشەی سەرچاوەی نەبوو، بەڵکو کێشەی بڕیاربوو.','The consultant summarized the conflict pointedly: the team did not have a resource problem, but a decision problem.','مشاور تعارض را دقیق و گزیده جمع‌بندی کرد: تیم مشکل منابع نداشت، مشکل تصمیم‌گیری داشت.','Şêwirmend nakokî bi awayekî kurte û bandorker kurt kir: tîm pirsgirêka çavkaniyan tune bû, lê pirsgirêka biryarê hebû.','Konsultantka celnie podsumowała konflikt: zespół nie miał problemu z zasobami, lecz z decyzjami.','Consultanta a sintetizat conflictul pregnant: echipa nu avea o problemă de resurse, ci una de decizie.','Консультант метко резюмировала конфликт: у команды была не проблема ресурсов, а проблема принятия решений.','Konsulentja e përmblodhi konfliktin në mënyrë të mprehtë: ekipi nuk kishte problem burimesh, por problem vendimmarrjeje.','Danışman çatışmayı çarpıcı biçimde özetledi: Ekibin kaynak sorunu değil, karar alma sorunu vardı.')),
      ex('Seine Kritik ist pointiert, aber nicht unfair; sie trifft die Schwäche des Arguments ohne persönliche Angriffe.', trans('نقده حاد وموجز، لكنه غير ظالم؛ فهو يصيب ضعف الحجة من دون هجمات شخصية.','ڕەخنەکەی کورت و کاریگەرە، بەڵام نادادپەروەر نییە؛ لاوازیی ئارگومێنتەکە دەگرێت بەبێ هێرشی کەسی.','His criticism is pointed but not unfair; it targets the weakness of the argument without personal attacks.','نقد او تیز و دقیق است، اما ناعادلانه نیست؛ ضعف استدلال را بدون حمله شخصی هدف می‌گیرد.','Rexneya wî kurte û xurt e, lê ne nedadil e; lawaziya argumanê digire bê êrîşên kesane.','Jego krytyka jest celna, ale nie niesprawiedliwa; trafia w słabość argumentu bez ataków osobistych.','Critica lui este tăioasă, dar nu nedreaptă; lovește slăbiciunea argumentului fără atacuri personale.','Его критика остра, но не несправедлива; она бьет по слабости аргумента без личных нападок.','Kritika e tij është e mprehtë, por jo e padrejtë; ajo godet dobësinë e argumentit pa sulme personale.','Eleştirisi keskin ama haksız değil; kişisel saldırı olmadan argümanın zayıflığını hedef alıyor.'))
    ]
  }),
  entry({
    word: 'die Pointierung', partOfSpeech: 'Noun', article: 'die', plural: 'Pointierungen', syllableBreak: 'Poin-tie-rung',
    topics: ['business-communication','culture-and-media','advanced-analysis'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'eine rhetorische Pointierung', meaning: 'a rhetorical sharpening or pointed formulation' }],
    wordFamilies: [{ lemma: 'pointiert', relationLabel: 'adjective', note: null }],
    meanings: meaning('صياغة حادة ومركزة؛ إبراز النقطة','خاڵبەندیی کاریگەر؛ دەربڕینی کورت و توند','pointed formulation; rhetorical sharpening','گزیده‌سازی برجسته؛ تأکید دقیق','formulasyoneke tûj; xurtkirina xalê','pointa; wyostrzenie','formulare pregnantă; accentuare retorică','заострение; точная формулировка','formulim i mprehtë; theksim retorik','vurgulu ifade; retorik keskinleştirme'),
    examples: [
      ex('Die Pointierung im Management Summary half dem Vorstand, die eigentliche Entscheidung von den Nebenthemen zu trennen.', trans('ساعدت الصياغة المركزة في الملخص التنفيذي مجلس الإدارة على فصل القرار الحقيقي عن القضايا الجانبية.','خاڵبەندیی کورت لە کورتەی بەڕێوەبەریدا یارمەتی ئەنجومەنی بەڕێوەبەری دا بڕیاری ڕاستەقینە لە بابەتە لاوەکییەکان جیا بکاتەوە.','The pointed formulation in the management summary helped the board distinguish the actual decision from side issues.','تأکید دقیق در خلاصه مدیریتی به هیئت‌مدیره کمک کرد تصمیم اصلی را از موضوعات فرعی جدا کند.','Formulasyona xurt di kurteya rêveberiyê de alîkarî da desteya rêveberiyê ku biryara rastîn ji mijarên kêlekî cuda bike.','Pointa w podsumowaniu zarządczym pomogła zarządowi oddzielić właściwą decyzję od tematów pobocznych.','Formularea pregnantă din rezumatul managerial a ajutat consiliul să separe decizia reală de temele secundare.','Заостренная формулировка в управленческом резюме помогла правлению отделить собственно решение от второстепенных тем.','Formulimi i mprehtë në përmbledhjen menaxheriale e ndihmoi bordin të ndajë vendimin real nga temat anësore.','Yönetici özetindeki vurgulu ifade, yönetim kurulunun asıl kararı yan konulardan ayırmasına yardımcı oldu.')),
      ex('Die literarische Pointierung entsteht nicht durch Lautstärke, sondern durch die präzise Verschiebung eines einzigen Bildes.', trans('لا تنشأ الصياغة الأدبية اللافتة من الصخب، بل من إزاحة دقيقة لصورة واحدة.','خاڵبەندیی ئەدەبی نە بە دەنگی بەرز، بەڵکو بە گواستنەوەی وردی تەنها وێنەیەک دروست دەبێت.','The literary pointedness arises not from loudness, but from the precise shift of a single image.','تأکید ادبی نه از بلندگویی، بلکه از جابه‌جایی دقیق یک تصویر واحد پدید می‌آید.','Pointkirina edebî ne ji dengbilindiyê, lê ji guheztina rast a wêneyekê çêdibe.','Literacka pointa nie powstaje przez donośność, lecz przez precyzyjne przesunięcie jednego obrazu.','Pointarea literară nu apare prin intensitate, ci prin deplasarea precisă a unei singure imagini.','Литературная заостренность возникает не из громкости, а из точного смещения одного-единственного образа.','Theksimi letrar nuk lind nga zhurma, por nga zhvendosja e saktë e një imazhi të vetëm.','Edebi vurgu yüksek sesten değil, tek bir imgenin hassas biçimde kaydırılmasından doğar.'))
    ]
  }),
  entry({
    word: 'polemisch', partOfSpeech: 'Adjective', syllableBreak: 'po-le-misch',
    topics: ['business-communication','culture-and-media','social-and-relationships'], usageLabels: ['formal','written','sensitive','advanced'],
    collocations: [{ text: 'polemisch zuspitzen', meaning: 'to sharpen something polemically' }],
    meanings: meaning('جدلي هجومي؛ سجالي','مشتومڕئامێز؛ هێرشکارانە','polemical; aggressively argumentative','جدلی و حمله‌آمیز','polemîk; bi nîqaşeke tund','polemiczny','polemic; combativ','полемический; спорный','polemik; sulmues në debat','polemik; tartışmacı ve sert'),
    examples: [
      ex('Der Kommentar war polemisch zugespitzt, enthielt aber einen berechtigten Hinweis auf die unklare Verantwortungsverteilung.', trans('كان التعليق مصاغاً بحدة سجالية، لكنه تضمن إشارة مشروعة إلى غموض توزيع المسؤوليات.','کۆمێنتەکە بە شێوەی مشتومڕئامێز توندکرابوو، بەڵام ئاماژەیەکی ڕەوای لەسەر نادیاری دابەشکردنی بەرپرسیارییەکان تێدا بوو.','The comment was polemically sharpened, but it contained a justified point about the unclear distribution of responsibilities.','نظر با لحنی جدلی تند شده بود، اما به نکته‌ای موجه درباره ابهام در تقسیم مسئولیت‌ها اشاره داشت.','Şîrove bi awayekî polemîk hatibû tûjkirin, lê tê de amajeyeke rewa li ser nezelaliya dabeşkirina berpirsiyariyan hebû.','Komentarz był polemicznie zaostrzony, ale zawierał uzasadnioną uwagę o niejasnym podziale odpowiedzialności.','Comentariul era formulat polemic, dar conținea o observație justificată despre distribuirea neclară a responsabilităților.','Комментарий был полемически заострен, но содержал обоснованное указание на неясное распределение ответственности.','Komenti ishte i mprehur në mënyrë polemike, por përmbante një vërejtje të drejtë për ndarjen e paqartë të përgjegjësive.','Yorum polemik biçimde keskinleştirilmişti, ancak belirsiz sorumluluk dağılımına dair haklı bir nokta içeriyordu.')),
      ex('Die Autorin schreibt polemisch, weil sie die scheinbare Neutralität der Debatte selbst für ein Machtinstrument hält.', trans('تكتب الكاتبة بأسلوب سجالي لأنها ترى أن الحياد الظاهر في النقاش نفسه أداة سلطة.','نووسەرەکە بە شێوەی مشتومڕئامێز دەنووسێت، چونکە بێلایەنیی دیاری گفتوگۆکە خۆی بە ئامرازی دەسەڵات دەزانێت.','The author writes polemically because she regards the debate’s apparent neutrality itself as an instrument of power.','نویسنده جدلی می‌نویسد، چون بی‌طرفی ظاهری بحث را خودْ ابزاری برای قدرت می‌داند.','Nivîskar bi awayekî polemîk dinivîse, ji ber ku bêalîbûna xuya ya nîqaşê bixwe wek amûra hêzê dibîne.','Autorka pisze polemicznie, ponieważ samą pozorną neutralność debaty uważa za narzędzie władzy.','Autoarea scrie polemic deoarece consideră însăși neutralitatea aparentă a dezbaterii un instrument de putere.','Авторка пишет полемически, потому что считает саму кажущуюся нейтральность дебатов инструментом власти.','Autorja shkruan në mënyrë polemike, sepse e sheh vetë neutralitetin e dukshëm të debatit si instrument pushteti.','Yazar polemik bir üslupla yazar, çünkü tartışmanın görünürdeki tarafsızlığını bizzat bir iktidar aracı olarak görür.'))
    ]
  }),
  entry({
    word: 'die Polyphonie', partOfSpeech: 'Noun', article: 'die', plural: 'Polyphonien', syllableBreak: 'Po-ly-pho-nie',
    topics: ['culture-and-media','advanced-analysis','education-and-training'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'literarische Polyphonie', meaning: 'literary polyphony' }],
    meanings: meaning('تعدد الأصوات؛ بوليفونية','فرەدەنگی؛ پۆلیفۆنی','polyphony; multiplicity of voices','چندصدایی؛ پلی‌فونی','pir-dengî; polîfonî','polifonia; wielogłosowość','polifonie; pluralitate de voci','полифония; многоголосие','polifoni; shumëzërësi','polifoni; çok seslilik'),
    examples: [
      ex('Die Polyphonie des Romans verhindert, dass ein einzelner Erzähler die moralische Deutung vollständig kontrolliert.', trans('تمنع تعددية الأصوات في الرواية راوياً واحداً من التحكم الكامل في التأويل الأخلاقي.','فرەدەنگیی ڕۆمانەکە ڕێگری دەکات لەوەی گێڕەرێکی تاک تەواوی لێکدانەوەی ئەخلاقی کۆنتڕۆڵ بکات.','The polyphony of the novel prevents a single narrator from fully controlling the moral interpretation.','چندصدایی رمان مانع می‌شود یک راوی واحد تفسیر اخلاقی را کاملاً کنترل کند.','Pir-dengiya romanê nahêle ku vegêrekî yekane tevahiya şîroveya exlaqî kontrol bike.','Polifonia powieści sprawia, że pojedynczy narrator nie kontroluje w pełni moralnej interpretacji.','Polifonia romanului împiedică un singur narator să controleze complet interpretarea morală.','Полифония романа не позволяет одному рассказчику полностью контролировать моральную интерпретацию.','Polifonia e romanit pengon një rrëfimtar të vetëm të kontrollojë plotësisht interpretimin moral.','Romanın polifonisi, tek bir anlatıcının ahlaki yorumu tamamen kontrol etmesini engeller.')),
      ex('In einem internationalen Projekt kann Polyphonie produktiv sein, wenn unterschiedliche Fachsprachen nicht vorschnell vereinheitlicht werden.', trans('في مشروع دولي يمكن أن تكون تعددية الأصوات منتجة إذا لم تُوحّد لغات التخصص المختلفة بتسرع.','لە پڕۆژەیەکی نێودەوڵەتیدا فرەدەنگی دەتوانێت بەرهەمدار بێت ئەگەر زمانە پسپۆڕییە جیاوازەکان بە پەلە یەکسان نەکرێن.','In an international project, polyphony can be productive if different specialist languages are not unified too hastily.','در یک پروژه بین‌المللی، چندصدایی می‌تواند سازنده باشد، اگر زبان‌های تخصصی متفاوت شتاب‌زده یکدست نشوند.','Di projeyeke navneteweyî de pir-dengî dikare berhemdar be, heger zimanên pisporî yên cuda bi lez neyên yekgirtin.','W projekcie międzynarodowym polifonia może być produktywna, jeśli różnych języków specjalistycznych nie ujednolica się zbyt pochopnie.','Într-un proiect internațional, polifonia poate fi productivă dacă limbajele de specialitate diferite nu sunt unificate prea grăbit.','В международном проекте полифония может быть продуктивной, если разные профессиональные языки не унифицируются слишком поспешно.','Në një projekt ndërkombëtar, polifonia mund të jetë produktive nëse gjuhët e ndryshme profesionale nuk unifikohen me nxitim.','Uluslararası bir projede farklı uzmanlık dilleri aceleyle tekleştirilmezse çok seslilik üretken olabilir.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 064', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

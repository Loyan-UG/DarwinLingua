const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '089';
const srcPath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outDir = path.join(root, 'content', 'generated');
const outPath = path.join(outDir, `de-${levelLower}-generated-batch-${batch}.json`);
const expected = ['versitzen', 'versteigen', 'die Verstiegenheit', 'die Verve', 'verwachsen', 'verwahren'];
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];

function splitTokens(text) { return text.split(',').map(s => s.trim()).filter(Boolean); }
function meaning(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) {
  return [ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr].map((text, i) => ({ language: langs[i], text }));
}
function translations(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) { return meaning(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr); }
function example(baseText, t) { return { baseText, translations: t }; }
function entry(e) {
  return {
    word: e.word,
    language: 'de',
    cefrLevel: level,
    partOfSpeech: e.partOfSpeech,
    article: e.article ?? null,
    plural: e.plural ?? null,
    infinitive: e.infinitive ?? null,
    pronunciationIpa: null,
    syllableBreak: e.syllableBreak,
    topics: e.topics,
    usageLabels: e.usageLabels,
    contextLabels: [],
    grammarNotes: e.grammarNotes ?? [],
    collocations: e.collocations ?? [],
    wordFamilies: e.wordFamilies ?? [],
    relations: [],
    meanings: e.meanings,
    examples: e.examples
  };
}

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const labelsByKey = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const topicKeys = new Set((taxonomy.topics || []).map(t => t.key));
const sourceText = fs.readFileSync(srcPath, 'utf8');
const tokens = splitTokens(sourceText);
const first = tokens.slice(0, expected.length);
if (JSON.stringify(first) !== JSON.stringify(expected)) {
  throw new Error(`Source head mismatch. Expected ${JSON.stringify(expected)}, got ${JSON.stringify(first)}`);
}

const entries = [
  entry({
    word: 'versitzen', partOfSpeech: 'Verb', infinitive: 'versitzen', syllableBreak: 'ver-sit-zen',
    topics: ['work-and-jobs','business-communication','culture-and-media'],
    usageLabels: ['formal','written','advanced','analysis'],
    grammarNotes: ['transitive verb; often used critically for spending time passively in a meeting or place'],
    collocations: [{ text: 'Zeit in einer Sitzung versitzen', meaning: 'to spend or waste time sitting through a meeting' }],
    wordFamilies: [{ lemma: 'sitzen', relationLabel: 'base verb', note: null }],
    meanings: meaning('يقضي وقتًا جالسًا غالبًا بلا فائدة','دانیشتووانە کات بەسەر دەبات، زۆرجار بێسوود','to sit through; to waste or spend time sitting somewhere','زمانی را نشسته و اغلب بی‌ثمر سپری کردن','demê rûniştî derbas kirin, pir caran bêfeyde','przesiedzieć; spędzić czas siedząc, często bezproduktywnie','a sta aşezat pe durata unui timp, adesea inutil','просидеть; провести время сидя, часто без пользы','të kalosh kohë ulur, shpesh pa dobi','oturarak geçirmek; çoğu zaman boşa zaman harcamak'),
    examples: [
      example('Das Team versitzte zwei Stunden im Lenkungskreis, ohne eine einzige verbindliche Entscheidung zu treffen.', translations('قضى الفريق ساعتين جالسًا في لجنة التوجيه من دون اتخاذ أي قرار ملزم.','تیمەکە دوو کاتژمێر لە کۆبوونەوەی بەڕێوەبردندا دانیشت، بێ ئەوەی بڕیارێکی پابەندکەر بدات.','The team sat through two hours in the steering committee without making a single binding decision.','تیم دو ساعت را در کمیته راهبری نشست، بدون اینکه حتی یک تصمیم الزام‌آور بگیرد.','Tîmê du saetan di komîteya rêberiyê de rûnişt, bêyî ku biryareke girêdayî bide.','Zespół przesiedział dwie godziny w komitecie sterującym, nie podejmując ani jednej wiążącej decyzji.','Echipa a stat două ore în comitetul director fără să ia nicio decizie obligatorie.','Команда просидела два часа в руководящем комитете, не приняв ни одного обязательного решения.','Ekipi ndenji dy orë në komitetin drejtues pa marrë asnjë vendim detyrues.','Ekip, yönlendirme kurulunda iki saat oturdu ve tek bir bağlayıcı karar bile almadı.')),
      example('Im Roman versitzt der alte Richter den Nachmittag im leeren Saal, als könne bloße Anwesenheit Schuld auflösen.', translations('في الرواية يقضي القاضي العجوز بعد الظهر جالسًا في القاعة الخالية، كأن الحضور وحده قادر على محو الذنب.','لە ڕۆمانەکەدا دادوەری پیر نیوەڕۆ لە هۆڵە بەتاڵەکەدا دادەنیشێت، وەک ئەوەی تەنها ئامادەبوون بتوانێت تاوان بسڕێتەوە.','In the novel, the old judge sits through the afternoon in the empty hall, as if mere presence could dissolve guilt.','در رمان، قاضی پیر بعدازظهر را در سالن خالی می‌نشیند، گویی صرف حضور می‌تواند گناه را محو کند.','Di romanê de dadwerê kal êvarê li salona vala rûdine, wekî tenê amadebûn dikare sûcê hilweşîne.','W powieści stary sędzia przesiedzi popołudnie w pustej sali, jakby sama obecność mogła rozpuścić winę.','În roman, bătrânul judecător își petrece după-amiaza așezat în sala goală, ca și cum simpla prezență ar putea dizolva vina.','В романе старый судья просиживает день в пустом зале, словно одно лишь присутствие способно растворить вину.','Në roman, gjykatësi i vjetër e kalon pasditen ulur në sallën bosh, sikur prania e thjeshtë të mund ta shpërbënte fajin.','Romanda yaşlı yargıç öğleden sonrayı boş salonda oturarak geçirir; sanki salt varlık suçu çözebilirmiş gibi.'))
    ]
  }),
  entry({
    word: 'versteigen', partOfSpeech: 'Verb', infinitive: 'versteigen', syllableBreak: 'ver-stei-gen',
    topics: ['business-communication','advanced-analysis','culture-and-media'],
    usageLabels: ['formal','written','advanced','sensitive'],
    grammarNotes: ['usually reflexive: sich zu etwas versteigen; means to go too far in a claim or judgment'],
    collocations: [{ text: 'sich zu einer Behauptung versteigen', meaning: 'to go so far as to make an exaggerated claim' }],
    wordFamilies: [{ lemma: 'steigen', relationLabel: 'base verb', note: null }],
    meanings: meaning('يبالغ في ادعاء أو حكم؛ يذهب بعيدًا جدًا','لە بانگەشە یان حوکمێکدا زێدەڕەوی دەکات','to go too far; to get carried away into an exaggerated claim','در ادعا یا قضاوتی زیاده‌روی کردن؛ از حد گذشتن','di gotin an dadbarîyekê de zêde çûn','posunąć się za daleko; zapędzić się w twierdzeniu','a merge prea departe; a se aventura într-o afirmație exagerată','зайти слишком далеко; увлечься чрезмерным утверждением','të shkosh tepër larg në një pretendim','bir iddiada fazla ileri gitmek; abartıya kaçmak'),
    examples: [
      example('Der Projektleiter verstieg sich zu der Behauptung, die Migration sei praktisch risikofrei.', translations('ذهب مدير المشروع بعيدًا إلى حد الادعاء بأن عملية الترحيل تكاد تكون بلا مخاطر.','بەڕێوەبەری پرۆژەکە زێدەڕەوی کرد و بانگەشەی کرد کە کۆچکردنەکە بە کردار بێ مەترسییە.','The project manager went so far as to claim that the migration was practically risk-free.','مدیر پروژه تا آنجا پیش رفت که ادعا کرد مهاجرت سیستم عملاً بدون ریسک است.','Rêveberê projeyê ewqas pêş çû ku got veguhastin bi rastî bêxetere ye.','Kierownik projektu posunął się do twierdzenia, że migracja jest praktycznie pozbawiona ryzyka.','Managerul de proiect a mers până la afirmația că migrarea este practic lipsită de riscuri.','Руководитель проекта зашел так далеко, что заявил, будто миграция практически не несет рисков.','Menaxheri i projektit shkoi aq larg sa pretendoi se migrimi ishte praktikisht pa rrezik.','Proje yöneticisi, geçişin neredeyse risksiz olduğunu iddia edecek kadar ileri gitti.')),
      example('Die Figur versteigt sich in eine moralische Gewissheit, die der Text später systematisch unterläuft.', translations('تنجرف الشخصية إلى يقين أخلاقي يقوضه النص لاحقًا بشكل منهجي.','کارەکتەرەکە خۆی دەباتە ناو دڵنیاییەکی ئەخلاقی کە دەقەکە دواتر بە شێوەی سیستەماتیک لاوازی دەکات.','The character gets carried away into a moral certainty that the text later systematically undermines.','شخصیت به نوعی یقین اخلاقی افراطی می‌رسد که متن بعداً آن را به‌طور نظام‌مند تضعیف می‌کند.','Kesayetî xwe dibe nav baweriyeke exlaqî ya teqez ku nivîs paşê bi rêkûpêkî lawaz dike.','Postać zapędza się w moralną pewność, którą tekst później systematycznie podważa.','Personajul se lasă dus într-o certitudine morală pe care textul o subminează ulterior sistematic.','Персонаж впадает в моральную уверенность, которую текст затем систематически подрывает.','Personazhi shkon drejt një sigurie morale që teksti më vonë e minon në mënyrë sistematike.','Karakter, metnin daha sonra sistemli biçimde zayıflattığı ahlaki bir kesinliğe kapılır.'))
    ]
  }),
  entry({
    word: 'die Verstiegenheit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Ver-stie-gen-heit',
    topics: ['advanced-analysis','culture-and-media','business-communication'],
    usageLabels: ['formal','written','analysis','advanced'],
    grammarNotes: ['feminine abstract noun; normally used in the singular'],
    collocations: [{ text: 'die Verstiegenheit einer Idee', meaning: 'the far-fetched or overblown nature of an idea' }],
    wordFamilies: [{ lemma: 'versteigen', relationLabel: 'verb', note: null }],
    meanings: meaning('المبالغة المتكلفة؛ الطابع البعيد عن الواقع','زێدەڕەویی بیرکردنەوە؛ دووربوون لە واقیع','far-fetchedness; overblown extravagance','افراط فکری؛ دور از واقع بودن','zêdeçûna dûr ji rastiyê; hînbûna pir bilind','przesadność; wydumany charakter','caracter exagerat sau fantezist','надуманность; чрезмерная претенциозность','largësi nga realiteti; teprim i mendimit','aşırı iddialılık; gerçeklikten kopukluk'),
    examples: [
      example('Die Verstiegenheit der Roadmap zeigte sich darin, dass sie drei Großprojekte mit demselben Team parallel versprach.', translations('ظهرت مبالغة خارطة الطريق في أنها وعدت بتنفيذ ثلاثة مشاريع كبرى بالتوازي بالفريق نفسه.','زێدەڕەویی ڕۆدمەپەکە لەوەدا دەرکەوت کە بەڵێنی سێ پرۆژەی گەورەی هاوکات بە هەمان تیم دەدا.','The far-fetchedness of the roadmap was evident in its promise to run three major projects in parallel with the same team.','دور از واقع بودن نقشه راه در این بود که وعده اجرای هم‌زمان سه پروژه بزرگ با همان تیم را می‌داد.','Dûrbûna nexşerêyê ji rastiyê di vê de xuya bû ku soz dida sê projeyên mezin bi heman tîmê hevdem bimeşîne.','Przesadność roadmapy ujawniła się w obietnicy równoległego prowadzenia trzech dużych projektów tym samym zespołem.','Caracterul nerealist al foii de parcurs se vedea în promisiunea de a derula trei proiecte mari în paralel cu aceeași echipă.','Надуманность дорожной карты проявлялась в обещании вести три крупных проекта параллельно одной и той же командой.','Largësia nga realiteti e planit u duk te premtimi për të zhvilluar paralelisht tri projekte të mëdha me të njëjtin ekip.','Yol haritasının gerçeklikten kopukluğu, aynı ekiple üç büyük projeyi paralel yürütme vaadinde görülüyordu.')),
      example('Gerade die Verstiegenheit seiner Vision macht die Figur zugleich lächerlich und tragisch.', translations('إن مبالغة رؤيته بالذات تجعل الشخصية مضحكة ومأساوية في آن واحد.','هەر زێدەڕەویی بینینەکەی وای دەکات کارەکتەرەکە هەمان کات گاڵتەجاڕ و تراژیدی بێت.','It is precisely the overblown nature of his vision that makes the character both ridiculous and tragic.','دقیقاً افراط در چشم‌انداز اوست که شخصیت را هم مضحک و هم تراژیک می‌کند.','Bi rastî zêdeçûna dîtina wî ye ku kesayetiyê hem pêkenok hem jî trajîk dike.','Właśnie przesadność jego wizji czyni tę postać jednocześnie śmieszną i tragiczną.','Tocmai caracterul exagerat al viziunii sale face personajul deopotrivă ridicol și tragic.','Именно чрезмерная претенциозность его видения делает персонажа одновременно смешным и трагичным.','Pikërisht teprimi i vizionit të tij e bën personazhin njëkohësisht qesharak dhe tragjik.','Onun vizyonundaki aşırı iddialılık, karakteri aynı anda hem gülünç hem trajik kılar.'))
    ]
  }),
  entry({
    word: 'die Verve', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Ver-ve',
    topics: ['business-communication','culture-and-media','meetings-and-presentations'],
    usageLabels: ['formal','written','advanced','analysis'],
    grammarNotes: ['feminine noun; often used without plural to describe spirited energy or expressive force'],
    collocations: [{ text: 'mit Verve auftreten', meaning: 'to appear or perform with spirited energy' }],
    wordFamilies: [],
    meanings: meaning('حيوية وحماس تعبيري','وزە و گەرمیی دەربڕین','verve; spirited energy','شور و نیروی بیانگر','coş û enerjiya vegotinê','werwa; żywiołowa energia','vervă; energie expresivă','жар; выразительная энергия','vrull; energji shprehëse','coşku; canlı anlatım gücü'),
    examples: [
      example('Sie präsentierte den Sanierungsplan mit solcher Verve, dass selbst skeptische Stakeholder nachfragten statt abzuwinken.', translations('قدمت خطة إعادة الهيكلة بحيوية كبيرة لدرجة أن حتى أصحاب المصلحة المتشككين طرحوا أسئلة بدلًا من الرفض.','پلانی چاکسازییەکەی بە هێندە گەرمییەوە پێشکەش کرد کە تەنانەت لایەنە گومانبارەکان پرسیاریان کرد نەک ڕەتی بکەنەوە.','She presented the restructuring plan with such verve that even skeptical stakeholders asked questions instead of dismissing it.','او برنامه بازسازی را با چنان شور و انرژی ارائه کرد که حتی ذی‌نفعان بدبین به جای رد کردن، سؤال پرسیدند.','Wê plana sererastkirinê bi qasî enerji pêşkêş kir ku hetta beşdarên gumanbar jî li şûna redkirinê pirs pirsîn.','Przedstawiła plan restrukturyzacji z taką werwą, że nawet sceptyczni interesariusze zadawali pytania zamiast go zbyć.','A prezentat planul de restructurare cu atâta vervă încât chiar și stakeholderii sceptici au pus întrebări în loc să îl respingă.','Она представила план оздоровления с таким жаром, что даже скептически настроенные участники задавали вопросы, а не отмахивались.','Ajo e prezantoi planin e ristrukturimit me aq vrull, sa edhe palët skeptike bënë pyetje në vend që ta hidhnin poshtë.','Yeniden yapılandırma planını öyle bir coşkuyla sundu ki şüpheci paydaşlar bile geçiştirmek yerine soru sordu.')),
      example('Die Verve des ersten Kapitels lässt später nach, wenn die Erzählung in dokumentarische Passagen übergeht.', translations('تتراجع حيوية الفصل الأول لاحقًا عندما ينتقل السرد إلى مقاطع وثائقية.','گەرمیی بەشی یەکەم دواتر کەم دەبێتەوە کاتێک گێڕانەوەکە دەچێتە ناو بەشە دۆکیومێنتارییەکان.','The verve of the first chapter later fades when the narrative shifts into documentary passages.','شور فصل اول بعداً کم‌رنگ می‌شود، وقتی روایت به بخش‌های مستندگونه می‌رسد.','Enerjiya beşa yekem paşê kêm dibe dema ku vegotin dikeve beşên belgeyî.','Werwa pierwszego rozdziału później słabnie, gdy narracja przechodzi w partie dokumentalne.','Verva primului capitol scade ulterior, când narațiunea trece în pasaje documentare.','Живость первой главы позже ослабевает, когда повествование переходит в документальные фрагменты.','Vrulli i kapitullit të parë zbehet më vonë, kur rrëfimi kalon në pasazhe dokumentare.','İlk bölümün coşkusu, anlatı belgesel pasajlara geçtiğinde daha sonra azalır.'))
    ]
  }),
  entry({
    word: 'verwachsen', partOfSpeech: 'Verb', infinitive: 'verwachsen', syllableBreak: 'ver-wach-sen',
    topics: ['technology-and-it','healthcare-and-appointments','advanced-analysis'],
    usageLabels: ['formal','written','advanced','analysis'],
    grammarNotes: ['intransitive verb; describes things growing together, healing over, or becoming inseparably linked'],
    collocations: [{ text: 'eng miteinander verwachsen sein', meaning: 'to be closely grown together or intertwined' }],
    wordFamilies: [{ lemma: 'wachsen', relationLabel: 'base verb', note: null }],
    meanings: meaning('ينمو متداخلًا أو يلتئم؛ يصبح مترابطًا بشدة','پێکەوە گەشە دەکات یان ساڕێژ دەبێت؛ زۆر بەیەکەوە گرێ دەدرێت','to grow together; to heal over; to become closely intertwined','به هم رشد کردن؛ جوش خوردن؛ عمیقاً درهم‌تنیده شدن','bi hev re mezin bûn; sax bûn; pir bi hev ve girêdan','zrosnąć się; stać się ściśle splecionym','a se suda; a se vindeca; a deveni strâns împletit','срастись; зарасти; тесно переплестись','të ngjitet; të shërohet; të ndërthuret ngushtë','kaynaşmak; iyileşip kapanmak; iç içe geçmek'),
    examples: [
      example('Über Jahre waren die Module so eng miteinander verwachsen, dass eine saubere Trennung kaum noch möglich war.', translations('على مدى سنوات أصبحت الوحدات متداخلة إلى حد أن فصلها بشكل نظيف لم يعد ممكنًا تقريبًا.','بە درێژایی ساڵان مۆدیولەکان ئەوەندە بەیەکەوە گرێدرابوون کە جیاکردنەوەی پاک نزیکەی نەما بوو.','Over the years, the modules had become so tightly intertwined that a clean separation was hardly possible anymore.','در طول سال‌ها، ماژول‌ها چنان درهم‌تنیده شده بودند که جداسازی تمیز آن‌ها تقریباً دیگر ممکن نبود.','Di salan de modul ewqas bi hev ve girêdabûn ku veqetandineke paqij êdî hema hema ne gengaz bû.','Przez lata moduły tak mocno się ze sobą zrosły, że czyste rozdzielenie było już prawie niemożliwe.','De-a lungul anilor, modulele deveniseră atât de strâns împletite încât o separare curată era aproape imposibilă.','За годы модули настолько тесно переплелись, что аккуратное разделение стало почти невозможным.','Me kalimin e viteve, modulet ishin ndërthurur aq ngushtë sa një ndarje e pastër ishte pothuajse e pamundur.','Yıllar içinde modüller öyle sıkı iç içe geçmişti ki temiz bir ayrım neredeyse artık mümkün değildi.')),
      example('Die Narbe war verwachsen, doch die Bewegung blieb schmerzhaft eingeschränkt.', translations('كانت الندبة قد التأمت، لكن الحركة بقيت محدودة ومؤلمة.','برینەکە ساڕێژ ببوو، بەڵام جووڵەکە بە ئازارەوە سنووردار مابوو.','The scar had healed over, but movement remained painfully restricted.','جای زخم جوش خورده بود، اما حرکت هنوز به‌طور دردناکی محدود بود.','Şopa birînê sax bibû, lê tevger hê jî bi êş ve sînordar mabû.','Blizna się zrosła, ale ruch nadal pozostawał boleśnie ograniczony.','Cicatricea se vindecase, dar mișcarea rămăsese dureros de limitată.','Шрам зарос, но движение оставалось болезненно ограниченным.','Shenja ishte mbyllur, por lëvizja mbeti e kufizuar me dhimbje.','Yara izi kapanmıştı, ancak hareket acı verecek şekilde kısıtlı kaldı.'))
    ]
  }),
  entry({
    word: 'verwahren', partOfSpeech: 'Verb', infinitive: 'verwahren', syllableBreak: 'ver-wah-ren',
    topics: ['documents-and-administration','law-and-compliance','business-communication'],
    usageLabels: ['formal','written','administrative','business'],
    grammarNotes: ['transitive verb meaning to keep safely; reflexive form sich gegen etwas verwahren means to object firmly'],
    collocations: [{ text: 'Dokumente sicher verwahren', meaning: 'to keep documents safely' }, { text: 'sich gegen einen Vorwurf verwahren', meaning: 'to firmly reject an accusation' }],
    wordFamilies: [{ lemma: 'die Verwahrung', relationLabel: 'noun', note: null }],
    meanings: meaning('يحفظ بأمان؛ يعترض بشدة على شيء','بە پارێزراوی هەڵدەگرێت؛ بە توندی دژایەتی تۆمەتێک دەکات','to keep safely; to object firmly to something','ایمن نگهداری کردن؛ قاطعانه به چیزی اعتراض کردن','bi ewlehî parastin; bi tundî li dijî tiştekî rawestin','przechowywać bezpiecznie; stanowczo się sprzeciwiać','a păstra în siguranță; a respinge ferm ceva','хранить надежно; решительно возражать против чего-либо','të ruash në mënyrë të sigurt; të kundërshtosh prerazi','güvenli şekilde saklamak; bir şeye kesin biçimde karşı çıkmak'),
    examples: [
      example('Die Originalverträge werden im Archiv gesondert verwahrt und nur gegen Protokoll herausgegeben.', translations('تُحفظ العقود الأصلية بشكل منفصل في الأرشيف ولا تُسلّم إلا بموجب محضر.','گرێبەستە ڕەسەنەکان لە ئەرشیفدا بە جیا هەڵدەگیرێن و تەنها بە تۆمارکردن دەدرێنە دەرەوە.','The original contracts are stored separately in the archive and released only against a record.','قراردادهای اصلی جداگانه در آرشیو نگهداری می‌شوند و فقط با ثبت صورت‌جلسه تحویل داده می‌شوند.','Peymanên resen li arşîvê cuda tên parastin û tenê bi tomarê tên derxistin.','Oryginały umów są przechowywane oddzielnie w archiwum i wydawane wyłącznie za protokołem.','Contractele originale sunt păstrate separat în arhivă și eliberate doar pe bază de proces-verbal.','Оригиналы договоров хранятся отдельно в архиве и выдаются только под протокол.','Kontratat origjinale ruhen veçmas në arkiv dhe jepen vetëm me procesverbal.','Orijinal sözleşmeler arşivde ayrı saklanır ve yalnızca tutanak karşılığında verilir.')),
      example('Die Abteilungsleiterin verwahrte sich gegen den Vorwurf, sie habe Risiken bewusst verschwiegen.', translations('اعترضت رئيسة القسم بشدة على الاتهام بأنها أخفت المخاطر عمدًا.','سەرۆکی بەشەکە بە توندی دژایەتی ئەو تۆمەتەی کرد کە گوایە مەترسییەکانی بە ئەنقەست شاردووەتەوە.','The department head firmly rejected the accusation that she had deliberately concealed risks.','رئیس بخش قاطعانه اتهام پنهان کردن آگاهانه ریسک‌ها را رد کرد.','Seroka beşê bi tundî ew tomet red kir ku wê metirsî bi zanebûn veşartibin.','Kierowniczka działu stanowczo odrzuciła zarzut, że świadomie ukryła ryzyka.','Șefa departamentului a respins ferm acuzația că ar fi ascuns în mod deliberat riscurile.','Руководительница отдела решительно отвергла обвинение в том, что она сознательно скрыла риски.','Drejtuesja e departamentit e kundërshtoi prerazi akuzën se kishte fshehur me vetëdije rreziqet.','Bölüm yöneticisi, riskleri bilerek gizlediği suçlamasına kesin biçimde karşı çıktı.'))
    ]
  })
];

for (const e of entries) {
  for (const t of e.topics) if (!topicKeys.has(t)) throw new Error(`Unknown topic ${t}`);
  for (const l of [...e.usageLabels, ...e.contextLabels]) if (!labelsByKey.has(l)) throw new Error(`Unknown label ${l}`);
}
const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = {
  packageVersion: '1.0',
  packageId: `de-${levelLower}-generated-batch-${batch}`,
  packageName: `German ${level} Generated Batch ${batch}`,
  source: 'Hybrid',
  defaultMeaningLanguages: langs,
  labels: usedLabels.map(k => labelsByKey.get(k)),
  entries,
  collections: [],
  scenarios: [],
  conversationStarterPacks: [],
  eventPreparationPacks: []
};

fs.mkdirSync(outDir, { recursive: true });
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');

const project = path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj');
const res = cp.spawnSync('dotnet', ['run', '--project', project, '--', '--target', 'shared', '--yes', outPath], { encoding: 'utf8', cwd: root });
const output = `${res.stdout || ''}${res.stderr || ''}`;
process.stdout.write(output);
const ok = res.status === 0 && output.includes('Entries imported: 6') && output.includes('Entries invalid: 0') && output.includes('Warnings: 0');
if (!ok) {
  const failedPath = path.join(outDir, `${levelLower}-failed-words.txt`);
  fs.appendFileSync(failedPath, `${new Date().toISOString()}\tbatch-${batch}\t${expected.join(', ')}\n`, 'utf8');
  throw new Error(`Import did not meet strict success criteria. Source not modified. Failed words logged to ${failedPath}`);
}
const remaining = tokens.slice(expected.length);
fs.writeFileSync(srcPath, remaining.join(', '), 'utf8');
console.log(`\nJSON_PATH=${outPath}`);
console.log(`SOURCE_PATH=${srcPath}`);
console.log(`PROCESSED=${expected.join(' | ')}`);
console.log(`SOURCE_UPDATED=yes`);
console.log(`REMAINING_COUNT=${remaining.length}`);
console.log(`FIRST_10=${remaining.slice(0, 10).join(' | ')}`);

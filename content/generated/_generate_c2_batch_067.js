const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '067';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['die Quellenkritik','quertreiben','räsonieren','die Rätselhaftigkeit','raunend','die Redefigur'];

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
    word: 'die Quellenkritik', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Quel-len-kri-tik',
    topics: ['education-and-training','advanced-analysis','culture-and-media'], usageLabels: ['formal','written','academic','analysis'],
    grammarNotes: ['feminine academic term; normally used in singular'],
    collocations: [{ text: 'strenge Quellenkritik üben', meaning: 'to apply rigorous source criticism' }],
    meanings: meaning('نقد المصادر؛ تقييم موثوقية الأدلة','ڕەخنەی سەرچاوەکان؛ هەڵسەنگاندنی متمانەپێکراوی بەڵگەکان','source criticism; critical evaluation of sources','نقد منابع؛ ارزیابی اعتبار شواهد','rexneya çavkaniyan; nirxandina baweriyê','krytyka źródeł','critica surselor','критика источников','kritikë burimesh','kaynak eleştirisi'),
    examples: [
      ex('Ohne sorgfältige Quellenkritik wirkt der Bericht zwar umfangreich, bleibt aber gegenüber manipulierten Daten anfällig.', trans('من دون نقد دقيق للمصادر يبدو التقرير واسعاً، لكنه يظل عرضة للبيانات المتلاعب بها.','بەبێ ڕەخنەی وردی سەرچاوەکان، ڕاپۆرتەکە هەرچەندە فراوان دەردەکەوێت، بەڵام بەرامبەر داتای دەستکاریکراو لاواز دەمێنێتەوە.','Without careful source criticism, the report may look extensive but remains vulnerable to manipulated data.','بدون نقد دقیق منابع، گزارش هرچند مفصل به نظر می‌رسد، اما در برابر داده‌های دستکاری‌شده آسیب‌پذیر می‌ماند.','Bê rexneya hûrgulî ya çavkaniyan, rapor herçend fireh xuya bike jî li hember daneyên destkarîkirî lawaz dimîne.','Bez starannej krytyki źródeł raport wydaje się obszerny, lecz pozostaje podatny na zmanipulowane dane.','Fără o critică atentă a surselor, raportul pare amplu, dar rămâne vulnerabil la date manipulate.','Без тщательной критики источников отчет выглядит обширным, но остается уязвимым к манипулированным данным.','Pa kritikë të kujdesshme të burimeve, raporti duket i gjerë, por mbetet i cenueshëm ndaj të dhënave të manipuluara.','Dikkatli kaynak eleştirisi olmadan rapor kapsamlı görünür, ancak manipüle edilmiş verilere karşı savunmasız kalır.')),
      ex('Die Historikerin betrieb Quellenkritik, bevor sie den privaten Brief als Beleg für eine politische Haltung deutete.', trans('مارست المؤرخة نقد المصادر قبل أن تفسر الرسالة الخاصة دليلاً على موقف سياسي.','مێژوونووسەکە پێش ئەوەی نامە تایبەتییەکە وەک بەڵگەی هەڵوێستێکی سیاسی لێکبداتەوە، ڕەخنەی سەرچاوەی کرد.','The historian applied source criticism before interpreting the private letter as evidence of a political position.','مورخ پیش از آن‌که نامه خصوصی را نشانه‌ای از موضع سیاسی بداند، نقد منبع انجام داد.','Dîroknasê berî ku nameya taybet wek belgeya helwesteke siyasî şîrove bike, rexneya çavkaniyê kir.','Historyczka zastosowała krytykę źródeł, zanim zinterpretowała prywatny list jako dowód postawy politycznej.','Istorica a aplicat critica surselor înainte de a interpreta scrisoarea privată ca dovadă a unei poziții politice.','Историк провела критику источника, прежде чем истолковать частное письмо как свидетельство политической позиции.','Historiania bëri kritikë burimi përpara se ta interpretonte letrën private si dëshmi të një qëndrimi politik.','Tarihçi, özel mektubu siyasi bir tutumun kanıtı olarak yorumlamadan önce kaynak eleştirisi yaptı.'))
    ]
  }),
  entry({
    word: 'quertreiben', partOfSpeech: 'Verb', infinitive: 'quertreiben', syllableBreak: 'quer-trei-ben',
    topics: ['work-and-jobs','management-and-leadership','social-and-relationships'], usageLabels: ['informal','workplace','sensitive','advanced'],
    collocations: [{ text: 'im Projekt quertreiben', meaning: 'to obstruct or stir up trouble in a project' }],
    meanings: meaning('يعرقل؛ يثير المتاعب من الداخل','ڕێگری کردن؛ لە ناوخۆدا ئاژاوەنانەوە','to obstruct; to stir up trouble','سنگ‌اندازی کردن؛ کارشکنی کردن','asteng kirin; aloziyan derxistin','mieszać; przeszkadzać','a sabota; a face opoziție din interior','вставлять палки в колеса; саботировать','pengoj; bëj sabotim të brendshëm','ayak diremek; işi baltalamak'),
    examples: [
      ex('Ein einzelner Bereich kann ein Transformationsprojekt monatelang quertreiben, wenn Zuständigkeiten politisch ausgelegt werden.', trans('يمكن لقسم واحد أن يعرقل مشروع تحول لأشهر إذا فُسرت المسؤوليات سياسياً.','بەشێکی تاک دەتوانێت پڕۆژەی گۆڕانکاری بۆ چەند مانگێک ڕێگری بکات، ئەگەر بەرپرسیارییەکان بە سیاسی لێکبدرێنەوە.','A single department can obstruct a transformation project for months if responsibilities are interpreted politically.','یک واحد به‌تنهایی می‌تواند ماه‌ها پروژه تحول را کارشکنی کند، اگر مسئولیت‌ها سیاسی تفسیر شوند.','Beşekî tenê dikare projeya veguherînê bi mehan asteng bike, heger berpirsiyarî bi awayekî siyasî bên şîrovekirin.','Jeden dział może miesiącami torpedować projekt transformacji, jeśli odpowiedzialności interpretuje się politycznie.','Un singur departament poate sabota luni întregi un proiect de transformare dacă responsabilitățile sunt interpretate politic.','Один отдел может месяцами саботировать проект трансформации, если зоны ответственности трактуются политически.','Një departament i vetëm mund ta pengojë për muaj të tërë një projekt transformimi nëse përgjegjësitë interpretohen politikisht.','Tek bir departman, sorumluluklar politik biçimde yorumlanırsa bir dönüşüm projesini aylarca baltalayabilir.')),
      ex('In der Nachbarschaft galt er als jemand, der aus Prinzip quertreibt, sobald andere eine gemeinsame Lösung gefunden haben.', trans('في الحي كان يُعرف بأنه شخص يعرقل الأمور بدافع المبدأ حالما يجد الآخرون حلاً مشتركاً.','لە هاوسێیەتیدا وەک کەسێک ناسرابوو کە بە بنەما ڕێگری دەکات کاتێک ئەوانی دیکە چارەسەرێکی هاوبەش دەدۆزنەوە.','In the neighborhood, he was seen as someone who obstructs on principle as soon as others have found a common solution.','در محله او را کسی می‌دانستند که به محض پیدا شدن راه‌حل مشترک، از سر لجاجت سنگ‌اندازی می‌کند.','Di cîranîyê de ew wek kesek dihate dîtin ku ji prensîbê asteng dike gava yên din çareseriyeke hevpar dibînin.','W sąsiedztwie uchodził za kogoś, kto z zasady przeszkadza, gdy tylko inni znajdą wspólne rozwiązanie.','În cartier era considerat cineva care se opune din principiu imediat ce ceilalți găsesc o soluție comună.','Соседи считали его человеком, который из принципа мешает, как только другие находят общее решение.','Në lagje ai shihej si dikush që pengon nga parimi sapo të tjerët gjejnë një zgjidhje të përbashkët.','Mahallede, başkaları ortak bir çözüm bulur bulmaz ilkesel olarak işi bozan biri sayılırdı.'))
    ]
  }),
  entry({
    word: 'räsonieren', partOfSpeech: 'Verb', infinitive: 'räsonieren', syllableBreak: 'rä-so-nie-ren',
    topics: ['business-communication','culture-and-media','social-and-relationships'], usageLabels: ['formal','written','advanced','sensitive'],
    collocations: [{ text: 'über etwas räsonieren', meaning: 'to reason or hold forth about something, often at length' }],
    meanings: meaning('يتفلسف أو يجادل بإسهاب','بە درێژی هۆکار دەهێنانەوە؛ قسەی درێژکردن','to reason at length; to hold forth','داد سخن دادن؛ استدلال‌پردازی طولانی کردن','bi dirêjî aqilane axaftin; gotar kirin','rezonować; rozprawiać','a raționa pe larg; a perora','рассуждать пространно','arsyetoj gjatë; ligjëroj','uzun uzun akıl yürütmek; nutuk çekmek'),
    examples: [
      ex('Statt über agile Werte zu räsonieren, hätte das Leitungsteam konkrete Abhängigkeiten zwischen den Projekten klären müssen.', trans('بدلاً من الإطالة في الحديث عن القيم الرشيقة، كان على فريق القيادة توضيح التبعيات الملموسة بين المشاريع.','لەبری ئەوەی دەربارەی بەهاکانی agile قسەی درێژ بکەن، دەبوو تیمی بەڕێوەبردن پەیوەندییە دیاریکراوەکانی نێوان پڕۆژەکان ڕوون بکاتەوە.','Instead of holding forth about agile values, the leadership team should have clarified concrete dependencies between the projects.','به‌جای داد سخن دادن درباره ارزش‌های اجایل، تیم مدیریت باید وابستگی‌های مشخص میان پروژه‌ها را روشن می‌کرد.','Li şûna ku li ser nirxên agile dirêj bipeyivin, diviya tîma rêveberiyê girêdanên zehmet ên navbera projeyan zelal bike.','Zamiast rozprawiać o wartościach agile, zespół kierowniczy powinien był wyjaśnić konkretne zależności między projektami.','În loc să peroreze despre valorile agile, echipa de conducere ar fi trebuit să clarifice dependențele concrete dintre proiecte.','Вместо того чтобы пространно рассуждать об agile-ценностях, руководство должно было прояснить конкретные зависимости между проектами.','Në vend që të ligjëronte gjatë për vlerat agile, ekipi drejtues duhej të sqaronte varësitë konkrete mes projekteve.','Yönetim ekibi agile değerler üzerine uzun uzun konuşmak yerine projeler arasındaki somut bağımlılıkları netleştirmeliydi.')),
      ex('Der Erzähler räsoniert über Schuld und Zufall, ohne dem Leser eine eindeutige moralische Lösung anzubieten.', trans('يتأمل الراوي مطولاً في الذنب والمصادفة من دون أن يقدم للقارئ حلاً أخلاقياً واضحاً.','گێڕەرەوەکە بە درێژی دەربارەی تاوان و ڕێکەوت قسە دەکات، بەبێ ئەوەی چارەسەرێکی ئەخلاقیی ڕوون پێشکەشی خوێنەر بکات.','The narrator reasons at length about guilt and chance without offering the reader a clear moral solution.','راوی درباره گناه و تصادف طولانی تأمل می‌کند، بی‌آن‌که راه‌حل اخلاقی روشنی به خواننده بدهد.','Vegêr bi dirêjî li ser sûc û tesadufê difikire bê ku çareseriyeke exlaqî ya zelal pêşkêşî xwendevan bike.','Narrator rozprawia o winie i przypadku, nie oferując czytelnikowi jednoznacznego rozwiązania moralnego.','Naratorul raționează pe larg despre vină și întâmplare fără să ofere cititorului o soluție morală clară.','Рассказчик пространно размышляет о вине и случайности, не предлагая читателю однозначного морального решения.','Rrëfimtari arsyeton gjatë mbi fajin dhe rastësinë pa i ofruar lexuesit një zgjidhje të qartë morale.','Anlatıcı, okura açık bir ahlaki çözüm sunmadan suçluluk ve rastlantı üzerine uzun uzun düşünür.'))
    ]
  }),
  entry({
    word: 'die Rätselhaftigkeit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Rät-sel-haf-tig-keit',
    topics: ['culture-and-media','advanced-analysis','social-and-relationships'], usageLabels: ['formal','written','analysis','advanced'],
    grammarNotes: ['feminine abstract noun; normally used in singular'],
    collocations: [{ text: 'eine gewisse Rätselhaftigkeit bewahren', meaning: 'to preserve a certain mysteriousness' }],
    meanings: meaning('غموض؛ طابع محيّر','نهێنیی و سەرسوڕهێنەر بوون','mysteriousness; enigmatic quality','معماگونگی؛ رازآلودگی','nepenîbûn; tiştê pirsgirêkî','zagadkowość','caracter enigmatic; mister','загадочность','enigmatikë; mister','gizemlilik; muammalık'),
    examples: [
      ex('Die Rätselhaftigkeit der Fehlermuster erschwerte die Analyse, weil kein einzelner Service eindeutig verantwortlich war.', trans('زاد غموض أنماط الأخطاء صعوبة التحليل لأنه لم تكن هناك خدمة واحدة مسؤولة بوضوح.','نهێنیی شێوازەکانی هەڵە شیکردنەوەکەی قورس کرد، چونکە هیچ خزمەتگوزارییەکی تاک بە ڕوونی بەرپرسیار نەبوو.','The mysteriousness of the error patterns made analysis harder because no single service was clearly responsible.','معماگونگی الگوهای خطا تحلیل را دشوارتر کرد، چون هیچ سرویس واحدی به‌روشنی مسئول نبود.','Nepenîbûna şêweyên çewtiyê analîz dijwartir kir, ji ber ku ti xizmetekî yekane bi zelalî berpirsiyar nebû.','Zagadkowość wzorców błędów utrudniła analizę, ponieważ żadna pojedyncza usługa nie była jednoznacznie odpowiedzialna.','Caracterul enigmatic al tiparelor de eroare a îngreunat analiza, deoarece niciun serviciu nu era clar responsabil.','Загадочность шаблонов ошибок усложнила анализ, потому что ни один сервис не был явно ответственным.','Enigmatika e modeleve të gabimeve e vështirësoi analizën, sepse asnjë shërbim i vetëm nuk ishte qartë përgjegjës.','Hata örüntülerinin gizemliliği analizi zorlaştırdı, çünkü tek bir servis açıkça sorumlu değildi.')),
      ex('Gerade die Rätselhaftigkeit der Figur verhindert, dass ihre Entscheidung psychologisch vollständig aufgelöst wird.', trans('إن غموض الشخصية تحديداً يمنع تفسير قرارها نفسياً بصورة كاملة.','هەر نهێنیی کارەکتەرەکە ڕێگری دەکات لەوەی بڕیارەکەی بە تەواوی بە دەروونناسی شیکرابێتەوە.','It is precisely the character’s mysteriousness that prevents her decision from being fully resolved psychologically.','دقیقاً رازآلودگی شخصیت مانع می‌شود تصمیم او از نظر روان‌شناختی کاملاً توضیح داده شود.','Bi taybetî nepenîbûna kesayetê nahêle ku biryara wê bi tevahî ji aliyê derûnî ve were çareserkirin.','Właśnie zagadkowość postaci sprawia, że jej decyzji nie da się całkowicie wyjaśnić psychologicznie.','Tocmai caracterul enigmatic al personajului împiedică explicarea completă a deciziei sale în termeni psihologici.','Именно загадочность персонажа не позволяет полностью психологически объяснить ее решение.','Pikërisht enigmatika e personazhit pengon që vendimi i saj të shpjegohet plotësisht psikologjikisht.','Karakterin gizemliliği, kararının psikolojik olarak bütünüyle açıklanmasını engeller.'))
    ]
  }),
  entry({
    word: 'raunend', partOfSpeech: 'Adjective', syllableBreak: 'rau-nend',
    topics: ['culture-and-media','social-and-relationships','advanced-analysis'], usageLabels: ['formal','written','advanced','analysis'],
    collocations: [{ text: 'in raunendem Ton', meaning: 'in a murmuring, suggestive tone' }],
    meanings: meaning('هامس بإيحاء؛ متمتم','چرپەچرپەیی؛ بە تۆنی نهێنی','murmuring; suggestive and whispering','زمزمه‌وار؛ رازآلود و القایی','bi şepqîn; bi tonê nepenî','szepczący; tajemniczo sugerujący','șoptit; insinuant','шепчущий; многозначительно бормочущий','pëshpëritës; sugjerues','fısıltılı; ima yüklü'),
    examples: [
      ex('Der Vortrag begann in einem raunenden Ton, als ginge es weniger um Fakten als um exklusive Insidergewissheiten.', trans('بدأ العرض بنبرة هامسة موحية، كأن الأمر لا يتعلق بالحقائق بقدر ما يتعلق بيقين داخلي حصري.','پێشکەشکردنەکە بە تۆنێکی چرپەچرپەیی دەستی پێکرد، وەک ئەوەی کەمتر دەربارەی ڕاستییەکان بێت و زیاتر دەربارەی دڵنیایی ناوخۆی تایبەت.','The presentation began in a murmuring tone, as if it were less about facts than about exclusive insider certainties.','ارائه با لحنی زمزمه‌وار شروع شد، انگار کمتر درباره واقعیت‌ها و بیشتر درباره قطعیت‌های محرمانه داخلی بود.','Pêşkêşkirin bi tonekî şepqîn dest pê kir, wek ku kêmtir li ser rastiyan û zêdetir li ser baweriyên taybet ên navxweyî be.','Prezentacja zaczęła się raunącym tonem, jakby chodziło mniej o fakty, a bardziej o ekskluzywne pewniki insiderów.','Prezentarea a început într-un ton șoptit, ca și cum ar fi fost mai puțin despre fapte și mai mult despre certitudini exclusive de insider.','Презентация началась шепчущим тоном, будто речь шла не столько о фактах, сколько об эксклюзивной внутренней осведомленности.','Prezantimi filloi me një ton pëshpëritës, sikur të bëhej fjalë më pak për fakte dhe më shumë për siguri ekskluzive të brendshme.','Sunum fısıltılı bir tonla başladı; sanki konu gerçeklerden çok özel içeriden bilgilerin kesinliğiymiş gibi.')),
      ex('Die raunenden Stimmen im Flur erzeugen im Roman eine Atmosphäre permanenter, aber nie greifbarer Bedrohung.', trans('تخلق الأصوات الهامسة في الممر في الرواية جواً من تهديد دائم لكنه لا يُمسك به أبداً.','دەنگە چرپەچرپەکان لە کۆڕیدۆرەکەدا لە ڕۆمانەکەدا کەشێکی هەڕەشەی هەمیشەیی بەڵام هەرگیز نەگیران دروست دەکەن.','The murmuring voices in the hallway create an atmosphere of constant but never tangible threat in the novel.','صداهای زمزمه‌وار در راهرو در رمان فضایی از تهدید دائمی اما هرگز قابل‌لمس ایجاد می‌کنند.','Dengên şepqîn di korîdorê de di romanê de atmosferake tehdîda herdemî lê qet negirtbar diafirînin.','Raunące głosy na korytarzu tworzą w powieści atmosferę stałego, lecz nigdy uchwytnego zagrożenia.','Vocile șoptite de pe coridor creează în roman o atmosferă de amenințare permanentă, dar niciodată palpabilă.','Шепчущие голоса в коридоре создают в романе атмосферу постоянной, но никогда неуловимой угрозы.','Zërat pëshpëritës në korridor krijojnë në roman një atmosferë kërcënimi të përhershëm, por kurrë të prekshëm.','Koridordaki fısıltılı sesler romanda sürekli ama asla somutlaşmayan bir tehdit atmosferi yaratır.'))
    ]
  }),
  entry({
    word: 'die Redefigur', partOfSpeech: 'Noun', article: 'die', plural: 'Redefiguren', syllableBreak: 'Re-de-fi-gur',
    topics: ['education-and-training','culture-and-media','business-communication'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'eine Redefigur verwenden', meaning: 'to use a figure of speech' }],
    meanings: meaning('صورة بلاغية؛ أسلوب بياني','شێوازی ڕەوانبێژی؛ فێگوری قسە','figure of speech; rhetorical figure','آرایه کلامی؛ صنعت بلاغی','figura axaftinê; şêwaza retorîkî','figura retoryczna','figură de stil; figură retorică','риторическая фигура; фигура речи','figurë ligjërimi; figurë retorike','söz sanatı; retorik figür'),
    examples: [
      ex('Die Metapher ist hier keine dekorative Redefigur, sondern strukturiert das gesamte Argument der Rede.', trans('الاستعارة هنا ليست صورة بلاغية تزيينية، بل تنظّم الحجة الكاملة للخطاب.','میتافۆرەکە لێرەدا شێوازێکی ڕەوانبێژیی ڕازاندنەوە نییە، بەڵکو تەواوی ئارگومێنتی وتارەکە ڕێکدەخات.','Here, the metaphor is not a decorative figure of speech; it structures the entire argument of the speech.','استعاره در اینجا آرایه‌ای تزئینی نیست، بلکه کل استدلال سخنرانی را ساختار می‌دهد.','Metafor li vir ne figura axaftinê ya xemilandî ye, lê tevahiya argumana gotarê rêxistin dike.','Metafora nie jest tu dekoracyjną figurą retoryczną, lecz strukturyzuje cały argument przemówienia.','Metafora nu este aici o figură de stil decorativă, ci structurează întregul argument al discursului.','Метафора здесь не декоративная фигура речи, а структурирует весь аргумент выступления.','Metafora këtu nuk është një figurë ligjërimi dekorative, por strukturon të gjithë argumentin e fjalimit.','Metafor burada dekoratif bir söz sanatı değil, konuşmanın tüm argümanını yapılandıran bir unsurdur.')),
      ex('Im Kundengespräch kann eine unpassende Redefigur Vertrauen zerstören, weil sie Distanz statt Verständnis signalisiert.', trans('في حديث مع العميل يمكن لصورة بلاغية غير مناسبة أن تدمّر الثقة لأنها توحي بالمسافة بدلاً من الفهم.','لە گفتوگۆی کڕیاردا شێوازێکی ڕەوانبێژیی نەگونجاو دەتوانێت متمانە تێکبدات، چونکە دووری نیشان دەدات نەک تێگەیشتن.','In a customer conversation, an inappropriate figure of speech can destroy trust because it signals distance rather than understanding.','در گفت‌وگو با مشتری، یک آرایه کلامی نامناسب می‌تواند اعتماد را از بین ببرد، چون به‌جای فهم، فاصله را القا می‌کند.','Di axaftina bi xerîdar re, figura axaftinê ya neguncav dikare bawerî hilweşîne ji ber ku dûrî nîşan dide, ne têgihiştin.','W rozmowie z klientem nietrafiona figura retoryczna może zniszczyć zaufanie, ponieważ sygnalizuje dystans zamiast zrozumienia.','Într-o conversație cu clientul, o figură de stil nepotrivită poate distruge încrederea deoarece semnalează distanță, nu înțelegere.','В разговоре с клиентом неуместная фигура речи может разрушить доверие, потому что она сигнализирует дистанцию, а не понимание.','Në një bisedë me klientin, një figurë ligjërimi e papërshtatshme mund të shkatërrojë besimin, sepse sinjalizon distancë në vend të mirëkuptimit.','Müşteri görüşmesinde uygunsuz bir söz sanatı güveni zedeleyebilir, çünkü anlayış yerine mesafe sinyali verir.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 067', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

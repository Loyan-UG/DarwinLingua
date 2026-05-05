const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '076';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['die Sprachphilosophie','spröde','die Spurensuche','stieben','stigmatisieren','stilisieren'];

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
    word: 'die Sprachphilosophie', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Sprach-phi-lo-so-phie',
    topics: ['education-and-training','advanced-analysis','culture-and-media'], usageLabels: ['formal','written','academic','analysis'],
    grammarNotes: ['feminine academic field name; normally used in singular'],
    collocations: [{ text: 'Fragen der Sprachphilosophie', meaning: 'questions of philosophy of language' }],
    meanings: meaning('فلسفة اللغة','فەلسەفەی زمان','philosophy of language','فلسفه زبان','felsefeya ziman','filozofia języka','filosofia limbajului','философия языка','filozofi e gjuhës','dil felsefesi'),
    examples: [
      ex('Die Sprachphilosophie fragt, ob Bedeutung im Kopf, im Gebrauch oder in sozialen Praktiken entsteht.', meaning('تسأل فلسفة اللغة عما إذا كان المعنى ينشأ في الذهن أم في الاستعمال أم في الممارسات الاجتماعية.','فەلسەفەی زمان دەپرسێت ئایا مانا لە مێشکدا، لە بەکارهێناندا یان لە پراکتیکی کۆمەڵایەتیدا دروست دەبێت.','Philosophy of language asks whether meaning arises in the mind, in use, or in social practices.','فلسفه زبان می‌پرسد معنا در ذهن، در کاربرد یا در کنش‌های اجتماعی پدید می‌آید.','Felsefeya ziman dipirse ka wate di hiş de, di bikaranînê de an di pratîkên civakî de çêdibe.','Filozofia języka pyta, czy znaczenie powstaje w umyśle, w użyciu czy w praktykach społecznych.','Filosofia limbajului întreabă dacă sensul apare în minte, în utilizare sau în practicile sociale.','Философия языка спрашивает, возникает ли значение в сознании, в употреблении или в социальных практиках.','Filozofia e gjuhës pyet nëse kuptimi lind në mendje, në përdorim apo në praktikat shoqërore.','Dil felsefesi, anlamın zihinde mi, kullanımda mı yoksa toplumsal pratiklerde mi oluştuğunu sorgular.')),
      ex('Für die Analyse von Chatbots ist Sprachphilosophie relevant, weil sie klärt, was Verstehen überhaupt heißen kann.', meaning('تعد فلسفة اللغة مهمة لتحليل روبوتات المحادثة لأنها توضح ما الذي يمكن أن يعنيه الفهم أصلاً.','بۆ شیکردنەوەی چاتبۆتەکان فەلسەفەی زمان گرنگە، چونکە ڕوون دەکاتەوە تێگەیشتن بە بنەڕەت چی دەتوانێت مانا بدات.','Philosophy of language is relevant for analyzing chatbots because it clarifies what understanding can mean at all.','فلسفه زبان برای تحلیل چت‌بات‌ها مهم است، چون روشن می‌کند فهم اساساً چه معنایی می‌تواند داشته باشد.','Ji bo analîza chatbotan felsefeya ziman girîng e, ji ber ku zelal dike ka têgihiştin bi bingehîn çi dikare wate bike.','Filozofia języka jest istotna dla analizy chatbotów, ponieważ wyjaśnia, co w ogóle może znaczyć rozumienie.','Filosofia limbajului este relevantă pentru analiza chatboturilor, deoarece clarifică ce poate însemna înțelegerea.','Философия языка важна для анализа чат-ботов, потому что проясняет, что вообще может означать понимание.','Filozofia e gjuhës është e rëndësishme për analizën e chatbotëve, sepse sqaron çfarë mund të nënkuptojë fare të kuptuarit.','Dil felsefesi chatbot analizinde önemlidir, çünkü anlamanın ne demek olabileceğini açıklığa kavuşturur.'))
    ]
  }),
  entry({
    word: 'spröde', partOfSpeech: 'Adjective', syllableBreak: 'sprö-de',
    topics: ['culture-and-media','business-communication','advanced-analysis'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'spröde wirken', meaning: 'to seem austere, dry, or hard to access' }],
    meanings: meaning('جاف؛ صعب الوصول؛ هش','وشک؛ قورس بۆ تێگەیشتن؛ شکننده','brittle; austere; hard to access','خشک؛ دیرفهم؛ شکننده','hişk; dijwar ji bo nêzîkbûnê','szorstki; oschły; kruchy','aspru; arid; casant','сухой; труднодоступный; хрупкий','i thatë; i vështirë për t’u afruar','kuru; zor erişilir; kırılgan'),
    examples: [
      ex('Die Dokumentation wirkt spröde, enthält aber genau die Informationen, die für die Fehlersuche entscheidend sind.', meaning('تبدو الوثائق جافة، لكنها تحتوي بالضبط على المعلومات الحاسمة لتتبع الأخطاء.','بەڵگەنامەکان وشک دەردەکەون، بەڵام هەمان ئەو زانیارییانەیان تێدایە کە بۆ دۆزینەوەی هەڵە چارەنووسسازن.','The documentation seems dry, but it contains exactly the information that is crucial for troubleshooting.','مستندات خشک به نظر می‌رسد، اما دقیقاً همان اطلاعاتی را دارد که برای خطایابی حیاتی است.','Belgekirin hişk xuya dike, lê rast ew agahiyan tê de ne ku ji bo lêgerîna çewtiyan girîng in.','Dokumentacja wydaje się oschła, ale zawiera dokładnie te informacje, które są kluczowe dla szukania błędów.','Documentația pare aridă, dar conține exact informațiile decisive pentru depanare.','Документация кажется сухой, но содержит именно ту информацию, которая критична для поиска ошибок.','Dokumentacioni duket i thatë, por përmban pikërisht informacionet vendimtare për gjetjen e gabimeve.','Dokümantasyon kuru görünüyor, ancak hata ayıklama için kritik olan bilgileri tam olarak içeriyor.')),
      ex('Der spröde Stil des Films verlangt Geduld, belohnt sie aber mit ungewöhnlicher Genauigkeit.', meaning('يتطلب الأسلوب الجاف للفيلم صبراً، لكنه يكافئه بدقة غير عادية.','شێوازی وشکی فیلمەکە ئارامی دەوێت، بەڵام بە وردییەکی نائاسایی پاداشتی دەداتەوە.','The austere style of the film requires patience, but rewards it with unusual precision.','سبک خشک فیلم صبر می‌طلبد، اما با دقتی غیرمعمول پاداش می‌دهد.','Şêwaza hişk a fîlmê sebr dixwaze, lê bi rastiyeke neasayî xelat dide.','Sprödy styl filmu wymaga cierpliwości, ale nagradza ją niezwykłą precyzją.','Stilul arid al filmului cere răbdare, dar o răsplătește cu o precizie neobișnuită.','Сухой стиль фильма требует терпения, но вознаграждает его необычной точностью.','Stili i thatë i filmit kërkon durim, por e shpërblen atë me saktësi të pazakontë.','Filmin kuru üslubu sabır ister, ama bunu alışılmadık bir kesinlikle ödüllendirir.'))
    ]
  }),
  entry({
    word: 'die Spurensuche', partOfSpeech: 'Noun', article: 'die', plural: 'Spurensuchen', syllableBreak: 'Spu-ren-su-che',
    topics: ['advanced-analysis','culture-and-media','quality-and-risk'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'eine gründliche Spurensuche', meaning: 'a thorough search for traces or clues' }],
    meanings: meaning('البحث عن آثار؛ تتبع الأدلة','گەڕان بەدوای شوێنەوار؛ بەدواداچوونی نیشانەکان','search for traces; investigation of clues','ردیابی؛ جست‌وجوی نشانه‌ها','lêgerîna şopan; şopandin','poszukiwanie śladów','căutare de urme; investigație','поиск следов','kërkim gjurmësh','iz sürme; ipucu arama'),
    examples: [
      ex('Die Spurensuche im Logsystem zeigte, dass der Fehler schon Tage vor dem Ausfall begonnen hatte.', meaning('أظهر تتبع الآثار في نظام السجلات أن الخطأ بدأ قبل أيام من الانقطاع.','گەڕان بەدوای شوێنەوار لە سیستەمی لۆگدا پیشانی دا کە هەڵەکە چەند ڕۆژ پێش وەستانەکە دەستی پێکردبوو.','The search through the log system showed that the error had started days before the outage.','ردیابی در سیستم لاگ نشان داد خطا چند روز پیش از اختلال آغاز شده بود.','Lêgerîna di pergala logê de nîşan da ku çewtî çend roj berî qutbûnê dest pê kiribû.','Poszukiwanie śladów w systemie logów pokazało, że błąd zaczął się kilka dni przed awarią.','Căutarea urmelor în sistemul de loguri a arătat că eroarea începuse cu zile înainte de cădere.','Поиск следов в системе логов показал, что ошибка началась за несколько дней до сбоя.','Kërkimi i gjurmëve në sistemin e logjeve tregoi se gabimi kishte nisur ditë para ndërprerjes.','Log sistemindeki iz sürme, hatanın kesintiden günler önce başladığını gösterdi.')),
      ex('Der Roman ist eine Spurensuche nach einer Mutter, die in offiziellen Archiven fast vollständig verschwunden ist.', meaning('الرواية بحث عن آثار أم اختفت تقريباً تماماً من الأرشيفات الرسمية.','ڕۆمانەکە گەڕانێکە بەدوای شوێنەواری دایکێک کە لە ئەرشیفی فەرمی نزیکەی تەواو ون بووە.','The novel is a search for traces of a mother who has almost completely disappeared from official archives.','رمان جست‌وجوی رد مادری است که تقریباً به‌طور کامل از آرشیوهای رسمی ناپدید شده است.','Roman lêgerîna şopên dayikekê ye ku hema bi tevahî ji arşîvên fermî winda bûye.','Powieść jest poszukiwaniem śladów matki, która niemal całkowicie zniknęła z oficjalnych archiwów.','Romanul este o căutare de urme ale unei mame care a dispărut aproape complet din arhivele oficiale.','Роман представляет собой поиск следов матери, почти полностью исчезнувшей из официальных архивов.','Romani është kërkim gjurmësh për një nënë që është zhdukur pothuajse plotësisht nga arkivat zyrtare.','Roman, resmi arşivlerden neredeyse tamamen kaybolmuş bir annenin izlerini arayıştır.'))
    ]
  }),
  entry({
    word: 'stieben', partOfSpeech: 'Verb', infinitive: 'stieben', syllableBreak: 'stie-ben',
    topics: ['culture-and-media','everyday-life','advanced-analysis'], usageLabels: ['formal','written','advanced','analysis'],
    grammarNotes: ['literary verb; means to fly or scatter apart, often in particles or groups'],
    collocations: [{ text: 'auseinander stieben', meaning: 'to scatter apart' }],
    meanings: meaning('يتطاير؛ يتفرق بسرعة','پەرتەوازەبوون؛ بە خێرایی بڵاوبوون','to scatter; to fly apart','پراکنده شدن؛ به اطراف پاشیدن','belav bûn; bi lez parçe bûn','rozpryskiwać się; rozbiegać się','a se împrăștia; a zbura în toate părțile','разлетаться; рассыпаться','shpërndahem; fluturoj në drejtime','dağılmak; savrulmak'),
    examples: [
      ex('Als die neue Priorisierung bekannt wurde, stoben die Teams in verschiedene Richtungen, statt ihre Abhängigkeiten zu klären.', meaning('عندما أُعلنت الأولويات الجديدة تفرقت الفرق في اتجاهات مختلفة بدلاً من توضيح تبعياتها.','کاتێک پێشینەیی نوێیەکان ئاشکرا بوون، تیمەکان بە ئاراستەی جیاواز پەرتەوازە بوون لەبری ئەوەی پەیوەندییەکانیان ڕوون بکەنەوە.','When the new prioritization was announced, the teams scattered in different directions instead of clarifying their dependencies.','وقتی اولویت‌بندی جدید اعلام شد، تیم‌ها به جای روشن‌کردن وابستگی‌ها در جهت‌های مختلف پراکنده شدند.','Dema pêşîtiya nû hate ragihandin, tîm li alîyên cuda belav bûn li şûna ku girêdanên xwe zelal bikin.','Gdy ogłoszono nowe priorytety, zespoły rozbiegły się w różne strony zamiast wyjaśnić zależności.','Când noua prioritizare a fost anunțată, echipele s-au împrăștiat în direcții diferite în loc să își clarifice dependențele.','Когда объявили новую приоритизацию, команды разбежались в разные стороны вместо того, чтобы прояснить зависимости.','Kur u shpall prioritizimi i ri, ekipet u shpërndanë në drejtime të ndryshme në vend që të sqaronin varësitë.','Yeni önceliklendirme duyurulunca ekipler bağımlılıklarını netleştirmek yerine farklı yönlere dağıldı.')),
      ex('Im Morgenlicht stieben die Vögel aus dem Baum, als hätte ein unsichtbarer Impuls sie zugleich erfasst.', meaning('في ضوء الصباح تطايرت الطيور من الشجرة كأن دافعاً غير مرئي أصابها في اللحظة نفسها.','لە ڕووناکی بەیانییدا باڵندەکان لە دارەکە پەرتەوازە بوون، وەک ئەوەی پاڵنەرێکی نادیار هەموویانی هاوکات گرتبێت.','In the morning light, the birds scattered from the tree as if an invisible impulse had seized them all at once.','در نور صبح، پرندگان از درخت پراکنده شدند، انگار نیرویی نامرئی هم‌زمان همه را گرفته باشد.','Di ronahiya sibehê de çûk ji darê belav bûn wek ku hêzeke nedîtbar hemûyan bi hev re girtibe.','W porannym świetle ptaki rozprysły się z drzewa, jakby niewidzialny impuls pochwycił je jednocześnie.','În lumina dimineții, păsările s-au împrăștiat din copac, ca și cum un impuls invizibil le-ar fi cuprins simultan.','В утреннем свете птицы разлетелись с дерева, будто невидимый импульс охватил их одновременно.','Në dritën e mëngjesit, zogjtë u shpërndanë nga pema sikur një impuls i padukshëm t’i kishte kapur njëherësh.','Sabah ışığında kuşlar ağaçtan savruldu; sanki görünmez bir dürtü hepsini aynı anda yakalamıştı.'))
    ]
  }),
  entry({
    word: 'stigmatisieren', partOfSpeech: 'Verb', infinitive: 'stigmatisieren', syllableBreak: 'stig-ma-ti-sie-ren',
    topics: ['social-and-relationships','human-resources','law-and-compliance'], usageLabels: ['formal','written','sensitive','analysis'],
    collocations: [{ text: 'eine Gruppe stigmatisieren', meaning: 'to stigmatize a group' }],
    meanings: meaning('يوصم؛ يضع وصمة اجتماعية','لەکەدارکردن؛ بە نیشانەی خراپ ناساندن','to stigmatize','انگ زدن؛ بدنام اجتماعی کردن','stîgmatîze kirin; lekeyê danîn','stygmatyzować','a stigmatiza','стигматизировать','stigmatizoj','damgalamak; stigmatize etmek'),
    examples: [
      ex('Eine Fehlerkultur darf Mitarbeitende nicht stigmatisieren, wenn sie Risiken früh melden.', meaning('لا يجوز لثقافة التعامل مع الأخطاء أن توصم الموظفين عندما يبلغون عن المخاطر مبكراً.','کەلتووری مامەڵە لەگەڵ هەڵە نابێت کارمەندان لەکەدار بکات کاتێک مەترسییەکان زوو ڕادەگەیەنن.','A culture of dealing with mistakes must not stigmatize employees when they report risks early.','فرهنگ خطاپذیری نباید کارکنانی را که ریسک‌ها را زود گزارش می‌کنند انگ بزند.','Çanda çewtiyan divê karmendan stîgmatîze neke dema xeteran zû radigihînin.','Kultura błędu nie może stygmatyzować pracowników, gdy wcześnie zgłaszają ryzyka.','O cultură a erorilor nu trebuie să stigmatizeze angajații când raportează riscurile devreme.','Культура работы с ошибками не должна стигматизировать сотрудников, когда они рано сообщают о рисках.','Një kulturë gabimesh nuk duhet të stigmatizojë punonjësit kur raportojnë rreziqet herët.','Hata kültürü, riskleri erken bildiren çalışanları damgalamamalıdır.')),
      ex('Der Film zeigt, wie schnell eine Gemeinschaft Außenseiter stigmatisiert, sobald sie eine einfache Erklärung für ihre Angst sucht.', meaning('يُظهر الفيلم مدى سرعة وصم جماعة للغرباء عندما تبحث عن تفسير بسيط لخوفها.','فیلمەکە پیشان دەدات کۆمەڵگا چەند خێرا دەرەکییەکان لەکەدار دەکات کاتێک بۆ ترسی خۆی ڕوونکردنەوەیەکی سادە دەگەڕێت.','The film shows how quickly a community stigmatizes outsiders once it seeks a simple explanation for its fear.','فیلم نشان می‌دهد یک جامعه وقتی برای ترس خود توضیحی ساده می‌خواهد، چقدر سریع به بیگانگان انگ می‌زند.','Fîlm nîşan dide civak çiqas zû kesên derve stîgmatîze dike gava ji bo tirsa xwe şiroveyeke sade digere.','Film pokazuje, jak szybko wspólnota stygmatyzuje outsiderów, gdy szuka prostego wyjaśnienia swojego lęku.','Filmul arată cât de repede o comunitate stigmatizează outsiderii când caută o explicație simplă pentru frica ei.','Фильм показывает, как быстро сообщество стигматизирует чужаков, когда ищет простое объяснение своему страху.','Filmi tregon sa shpejt një komunitet stigmatizon të jashtmit kur kërkon një shpjegim të thjeshtë për frikën e vet.','Film, bir topluluğun korkusuna basit bir açıklama aradığında dışarıdakileri ne kadar hızlı damgaladığını gösterir.'))
    ]
  }),
  entry({
    word: 'stilisieren', partOfSpeech: 'Verb', infinitive: 'stilisieren', syllableBreak: 'sti-li-sie-ren',
    topics: ['culture-and-media','business-communication','advanced-analysis'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'sich als Opfer stilisieren', meaning: 'to portray oneself in a stylized way as a victim' }],
    meanings: meaning('يصوغ بأسلوب فني؛ يقدم بصورة مبالغ فيها','شێوازدان؛ بە وێنەیەکی دەستکاریکراو پیشاندان','to stylize; to portray in a deliberately shaped way','سبک‌پردازی کردن؛ به شکلی خاص جلوه دادن','stîlîze kirin; bi awayekî taybet nîşan dan','stylizować','a stiliza','стилизовать','stilizoj','stilize etmek'),
    examples: [
      ex('Das Unternehmen stilisierte den Strategiewechsel als mutigen Neuanfang, obwohl er vor allem eine Reaktion auf Marktverluste war.', meaning('قدمت الشركة تغيير الاستراتيجية كبداية جديدة شجاعة، رغم أنه كان قبل كل شيء رداً على خسائر السوق.','کۆمپانیاکە گۆڕینی ستراتیژیی وەک دەستپێکی نوێی ئازایانە پیشان دا، هەرچەندە بە تایبەتی وەڵامێک بوو بۆ لەدەستدانی بازاڕ.','The company stylized the strategy shift as a bold new beginning, although it was mainly a reaction to market losses.','شرکت تغییر راهبرد را به‌عنوان آغازی جسورانه جلوه داد، با اینکه عمدتاً واکنشی به زیان‌های بازار بود.','Şirket guherîna stratejiyê wek destpêkeke wêrek stîlîze kir, herçend ew bi taybetî bersivek bû li windahiyên bazarê.','Firma wystylizowała zmianę strategii na odważny nowy początek, choć była ona przede wszystkim reakcją na straty rynkowe.','Compania a stilizat schimbarea de strategie ca pe un nou început curajos, deși era mai ales o reacție la pierderile de piață.','Компания стилизовала смену стратегии как смелое новое начало, хотя прежде всего это была реакция на рыночные потери.','Kompania e stilizoi ndryshimin e strategjisë si një fillim të ri të guximshëm, megjithëse ishte kryesisht reagim ndaj humbjeve në treg.','Şirket strateji değişikliğini cesur bir yeni başlangıç olarak stilize etti, oysa bu esasen pazar kayıplarına verilen bir tepkiydi.')),
      ex('Der Roman stilisiert die Großstadt nicht zur Verheißung, sondern zu einem Raum permanenter Prüfung.', meaning('لا يجعل الرواية المدينة الكبرى وعداً خلاصياً، بل يصوغها كفضاء اختبار دائم.','ڕۆمانەکە شارە گەورەکە وەک بەڵێنێک شێواز نادات، بەڵکو وەک شوێنی تاقیکردنەوەی هەمیشەیی پیشان دەدات.','The novel does not stylize the metropolis as a promise, but as a space of constant testing.','رمان کلان‌شهر را به شکل وعده‌ای رهایی‌بخش جلوه نمی‌دهد، بلکه آن را فضایی برای آزمون دائمی می‌سازد.','Roman bajarê mezin wek soz stîlîze nake, lê wek qada testkirina domdar nîşan dide.','Powieść nie stylizuje metropolii na obietnicę, lecz na przestrzeń ciągłej próby.','Romanul nu stilizează metropola ca promisiune, ci ca spațiu al unei probe permanente.','Роман стилизует мегаполис не как обещание, а как пространство постоянного испытания.','Romani nuk e stilizon metropolin si premtim, por si hapësirë prove të përhershme.','Roman metropolü bir vaat olarak değil, sürekli sınanma alanı olarak stilize eder.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 076', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

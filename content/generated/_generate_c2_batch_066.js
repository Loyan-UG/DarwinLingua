const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '066';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['die Pragmatik','profund','das Proprium','prosaisch','die Prosodie','provokant'];

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
    word: 'die Pragmatik', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Prag-ma-tik',
    topics: ['education-and-training','advanced-analysis','business-communication'], usageLabels: ['formal','written','academic','analysis'],
    grammarNotes: ['feminine abstract noun; usually singular; can mean pragmatics in linguistics or practical orientation'],
    collocations: [{ text: 'die Pragmatik einer Entscheidung', meaning: 'the practical implications of a decision' }],
    meanings: meaning('البراغماتية؛ علم الاستعمال اللغوي','پراگماتیک؛ بەکارهێنانی کرداری یان زمانی','pragmatics; practical orientation','کاربردشناسی؛ عمل‌گرایی','pragmatîk; bikaranîna pratîkî','pragmatyka','pragmatică; orientare practică','прагматика; практическая направленность','pragmatikë; orientim praktik','pragmatik; pratik yönelim'),
    examples: [
      ex('Die Pragmatik der Entscheidung wurde erst sichtbar, als klar war, welche Teams zusätzliche Arbeit übernehmen müssten.', trans('لم تظهر الجوانب العملية للقرار إلا عندما اتضح أي الفرق ستضطر إلى تولي عمل إضافي.','پراگماتیکی بڕیارەکە تەنها کاتێک دیار بوو کە ڕوون بوو کام تیم دەبێت کاری زیادە وەربگرێت.','The pragmatics of the decision became visible only when it was clear which teams would have to take on additional work.','عملکرد عملی تصمیم فقط وقتی روشن شد که مشخص شد کدام تیم‌ها باید کار اضافی بر عهده بگیرند.','Pragmatîka biryarê tenê wê demê xuya bû ku diyar bû kîjan tîm dê karekî zêde bigirin.','Pragmatyka decyzji stała się widoczna dopiero wtedy, gdy było jasne, które zespoły muszą przejąć dodatkową pracę.','Pragmatica deciziei a devenit vizibilă abia când a fost clar ce echipe ar trebui să preia muncă suplimentară.','Практическая сторона решения стала видна лишь тогда, когда стало ясно, каким командам придется взять на себя дополнительную работу.','Pragmatika e vendimit u bë e dukshme vetëm kur u qartësua se cilat ekipe do të duhej të merrnin punë shtesë.','Kararın pragmatiği, hangi ekiplerin ek iş üstleneceği netleşince görünür oldu.')),
      ex('In der Linguistik untersucht die Pragmatik, wie Bedeutung durch Kontext, Absicht und soziale Beziehung entsteht.', trans('في اللسانيات تدرس البراغماتية كيف ينشأ المعنى من السياق والقصد والعلاقة الاجتماعية.','لە زمانناسی‌دا پراگماتیک لێکۆڵینەوە دەکات کە مانا چۆن لە چوارچێوە، مەبەست و پەیوەندیی کۆمەڵایەتی دروست دەبێت.','In linguistics, pragmatics studies how meaning arises through context, intention, and social relationship.','در زبان‌شناسی، کاربردشناسی بررسی می‌کند که معنا چگونه از بافت، قصد و رابطه اجتماعی پدید می‌آید.','Di zimanvaniyê de pragmatîk lêkolîn dike ka wate çawa ji konteks, armanc û têkiliya civakî çêdibe.','W językoznawstwie pragmatyka bada, jak znaczenie powstaje przez kontekst, intencję i relację społeczną.','În lingvistică, pragmatica studiază cum apare sensul prin context, intenție și relație socială.','В лингвистике прагматика изучает, как значение возникает через контекст, намерение и социальные отношения.','Në gjuhësi, pragmatika studion se si kuptimi lind përmes kontekstit, qëllimit dhe marrëdhënies shoqërore.','Dilbilimde pragmatik, anlamın bağlam, niyet ve toplumsal ilişki yoluyla nasıl oluştuğunu inceler.'))
    ]
  }),
  entry({
    word: 'profund', partOfSpeech: 'Adjective', syllableBreak: 'pro-fund',
    topics: ['advanced-analysis','business-communication','education-and-training'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'profunde Kenntnisse', meaning: 'profound knowledge' }],
    meanings: meaning('عميق؛ راسخ المعرفة','قووڵ؛ زانیاریی بەهێز','profound; deep and well-founded','عمیق؛ ریشه‌دار و مستند','kûr; bi bingeh','głęboki; gruntowny','profund; temeinic','глубокий; основательный','i thellë; i mirëbazuar','derin; sağlam temelli'),
    examples: [
      ex('Ihre profunde Kenntnis der Altsysteme verhinderte, dass die Migration an scheinbar nebensächlichen Abhängigkeiten scheiterte.', trans('منعت معرفتها العميقة بالأنظمة القديمة فشل الترحيل بسبب تبعيات بدت هامشية.','زانیاریی قووڵی ئەو لەسەر سیستەمە کۆنەکان ڕێگری کرد لەوەی کۆچکردنەکە بە پەیوەندییەکانی بە ڕواڵەت لاوەکی شکست بهێنێت.','Her profound knowledge of the legacy systems prevented the migration from failing because of seemingly minor dependencies.','شناخت عمیق او از سیستم‌های قدیمی مانع شد مهاجرت به دلیل وابستگی‌های ظاهراً فرعی شکست بخورد.','Zanîna wê ya kûr li ser pergalên kevn rê li ber wê girt ku koçberî ji ber girêdanên xuya biçûk têk biçe.','Jej gruntowna znajomość starych systemów zapobiegła temu, by migracja nie powiodła się z powodu pozornie drugorzędnych zależności.','Cunoașterea ei profundă a sistemelor vechi a împiedicat migrarea să eșueze din cauza unor dependențe aparent secundare.','Ее глубокое знание старых систем не позволило миграции провалиться из-за, казалось бы, второстепенных зависимостей.','Njohja e saj e thellë e sistemeve të vjetra pengoi që migrimi të dështonte për shkak të varësive në dukje anësore.','Eski sistemlere dair derin bilgisi, geçişin görünüşte ikincil bağımlılıklar yüzünden başarısız olmasını engelledi.')),
      ex('Die Studie ist profund, weil sie historische Quellen, statistische Daten und aktuelle Interviews systematisch zusammenführt.', trans('الدراسة عميقة لأنها تجمع بشكل منهجي بين المصادر التاريخية والبيانات الإحصائية والمقابلات الحالية.','توێژینەوەکە قووڵە، چونکە سەرچاوە مێژووییەکان، داتای ئاماری و چاوپێکەوتنی ئێستا بە سیستەماتیک کۆدەکاتەوە.','The study is profound because it systematically brings together historical sources, statistical data, and current interviews.','مطالعه عمیق است، چون منابع تاریخی، داده‌های آماری و مصاحبه‌های جدید را نظام‌مند کنار هم می‌گذارد.','Lêkolîn kûr e ji ber ku çavkaniyên dîrokî, daneyên statîstîkî û hevpeyvînên nû bi rêkûpêk tîne cem hev.','Badanie jest gruntowne, ponieważ systematycznie łączy źródła historyczne, dane statystyczne i aktualne wywiady.','Studiul este profund deoarece reunește sistematic surse istorice, date statistice și interviuri actuale.','Исследование глубоко, потому что системно объединяет исторические источники, статистические данные и актуальные интервью.','Studimi është i thellë, sepse bashkon në mënyrë sistematike burime historike, të dhëna statistikore dhe intervista aktuale.','Çalışma derindir, çünkü tarihsel kaynakları, istatistiksel verileri ve güncel görüşmeleri sistematik biçimde bir araya getirir.'))
    ]
  }),
  entry({
    word: 'das Proprium', partOfSpeech: 'Noun', article: 'das', plural: 'Propria', syllableBreak: 'Pro-pri-um',
    topics: ['advanced-analysis','education-and-training','culture-and-media'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'das Proprium einer Gattung', meaning: 'the distinctive feature of a genre' }],
    meanings: meaning('الخاصية المميزة؛ الجوهر الخاص','تایبەتمەندیی جیاکەرەوە؛ تایبەتیی بنەڕەتی','distinctive property; defining characteristic','ویژگی خاص و متمایز؛ خصیصه ذاتی','taybetmendiya cuda; xasiyetê bingehîn','cecha swoista; proprium','trăsătură proprie; specific distinctiv','собственное отличие; характерная особенность','tipar dallues; veçori thelbësore','ayırt edici özellik; özgül nitelik'),
    examples: [
      ex('Das Proprium der Plattform liegt nicht in einzelnen Funktionen, sondern in der Art, wie sie Datenflüsse zwischen Abteilungen koordiniert.', trans('لا تكمن الخاصية المميزة للمنصة في وظائف منفردة، بل في الطريقة التي تنسق بها تدفقات البيانات بين الأقسام.','تایبەتمەندیی جیاکەرەوەی پلاتفۆرمەکە لە فەنکشنە تاکەکاندا نییە، بەڵکو لە شێوازی هاوبەشکردنی ڕەوتی داتا لە نێوان بەشەکاندا.','The platform’s distinctive feature lies not in individual functions, but in how it coordinates data flows between departments.','ویژگی خاص پلتفرم در قابلیت‌های تک‌تک نیست، بلکه در شیوه هماهنگ‌کردن جریان داده میان بخش‌هاست.','Propriuma platformê ne di fonksiyonên yekane de ye, lê di awayê hevahengkirina herikîna daneyan di navbera beşan de ye.','Swoista cecha platformy nie tkwi w pojedynczych funkcjach, lecz w sposobie koordynowania przepływów danych między działami.','Specificul platformei nu stă în funcții individuale, ci în modul în care coordonează fluxurile de date între departamente.','Отличительная особенность платформы не в отдельных функциях, а в том, как она координирует потоки данных между отделами.','Veçoria dalluese e platformës nuk qëndron te funksionet e veçanta, por te mënyra si koordinon rrjedhat e të dhënave mes departamenteve.','Platformun ayırt edici niteliği tekil işlevlerde değil, departmanlar arasındaki veri akışlarını koordine etme biçimindedir.')),
      ex('Die Vorlesung fragte nach dem Proprium der Tragödie und grenzte es von bloßem Unglück ab.', trans('طرحت المحاضرة سؤال الخاصية المميزة للمأساة وميزتها عن مجرد سوء الحظ.','وانەکە پرسیاری تایبەتمەندیی جیاکەرەوەی تراژیدیا کرد و لە بەختی خراپی ئاسایی جیاکردەوە.','The lecture asked about the distinctive property of tragedy and distinguished it from mere misfortune.','درس درباره ویژگی متمایز تراژدی پرسید و آن را از بداقبالی صرف جدا کرد.','Dersê li propriuma trajediyê pirsî û wê ji tenê nebaşbextiyê cuda kir.','Wykład pytał o swoistość tragedii i odróżniał ją od zwykłego nieszczęścia.','Cursul a întrebat care este specificul tragediei și l-a delimitat de simpla nenorocire.','Лекция ставила вопрос об отличительной особенности трагедии и отделяла ее от простого несчастья.','Ligjërata pyeti për tiparin dallues të tragjedisë dhe e ndau atë nga fatkeqësia e thjeshtë.','Ders, trajedinin ayırt edici niteliğini sorguladı ve onu basit talihsizlikten ayırdı.'))
    ]
  }),
  entry({
    word: 'prosaisch', partOfSpeech: 'Adjective', syllableBreak: 'pro-sa-isch',
    topics: ['culture-and-media','business-communication','everyday-life'], usageLabels: ['formal','written','advanced','analysis'],
    collocations: [{ text: 'eine prosaische Erklärung', meaning: 'a prosaic explanation' }],
    meanings: meaning('نثري؛ عادي غير شاعرية','پڕۆزایی؛ ئاسایی و بێ شیعرانە','prosaic; plain and unpoetic','نثری؛ معمولی و بی‌زرق‌وبرق','prozaîk; sade û nehelbestî','prozaiczny','prozaic; lipsit de poezie','прозаический; будничный','prozaik; i zakonshëm','düzyazısal; sıradan'),
    examples: [
      ex('Die Ursache des Ausfalls war prosaisch: Ein abgelaufenes Zertifikat hatte den automatischen Datenaustausch blockiert.', trans('كان سبب العطل عادياً جداً: شهادة منتهية الصلاحية منعت تبادل البيانات الآلي.','هۆکاری کەوتنەکە زۆر ئاسایی بوو: بڕوانامەیەکی بەسەرچوو ئاڵوگۆڕی ئۆتۆماتیکی داتای ڕاگرتبوو.','The cause of the outage was prosaic: an expired certificate had blocked the automatic data exchange.','علت اختلال کاملاً ساده بود: یک گواهی منقضی‌شده تبادل خودکار داده را مسدود کرده بود.','Sedema qutbûnê prozaîk bû: sertîfîkayeke demborî guheztina daneyan a otomatîk asteng kiribû.','Przyczyna awarii była prozaiczna: wygasły certyfikat zablokował automatyczną wymianę danych.','Cauza întreruperii a fost prozaică: un certificat expirat blocase schimbul automat de date.','Причина сбоя была прозаической: просроченный сертификат заблокировал автоматический обмен данными.','Shkaku i ndërprerjes ishte prozaik: një certifikatë e skaduar kishte bllokuar shkëmbimin automatik të të dhënave.','Kesintinin nedeni sıradandı: Süresi dolmuş bir sertifika otomatik veri alışverişini engellemişti.')),
      ex('Nach den großen politischen Gesten folgt im Roman ein prosaischer Alltag aus Rechnungen, Müdigkeit und kleinen Kompromissen.', trans('بعد الإيماءات السياسية الكبرى يأتي في الرواية يوم عادي من فواتير وتعب وتسويات صغيرة.','دوای ئاماژە سیاسییە گەورەکان، لە ڕۆمانەکەدا ژیانێکی ڕۆژانەی ئاسایی دێت لە فاکتورەکان، ماندووبوون و ڕێککەوتنی بچووک.','After the grand political gestures, the novel moves into a prosaic everyday life of bills, fatigue, and small compromises.','پس از ژست‌های بزرگ سیاسی، رمان به روزمرگی نثری و ساده‌ای از قبض‌ها، خستگی و مصالحه‌های کوچک می‌رسد.','Piştî nîşanên siyasî yên mezin, roman tê jiyana rojane ya prozaîk ji fatûre, westîn û lihevhatinên biçûk.','Po wielkich gestach politycznych w powieści następuje prozaiczna codzienność rachunków, zmęczenia i drobnych kompromisów.','După marile gesturi politice, în roman urmează un cotidian prozaic de facturi, oboseală și mici compromisuri.','После громких политических жестов в романе следует прозаическая повседневность счетов, усталости и мелких компромиссов.','Pas gjesteve të mëdha politike, në roman vjen një përditshmëri prozaike me fatura, lodhje dhe kompromise të vogla.','Büyük siyasi jestlerden sonra romanda faturalar, yorgunluk ve küçük uzlaşmalardan oluşan sıradan bir gündelik hayat gelir.'))
    ]
  }),
  entry({
    word: 'die Prosodie', partOfSpeech: 'Noun', article: 'die', plural: 'Prosodien', syllableBreak: 'Pro-so-die',
    topics: ['education-and-training','business-communication','culture-and-media'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'Prosodie analysieren', meaning: 'to analyze prosody' }],
    meanings: meaning('النبر والتنغيم؛ علم الإيقاع الصوتي','پرۆسۆدی؛ ئاهەنگ و جەختی دەنگ','prosody; rhythm and intonation of speech','آهنگ و تکیه گفتار؛ عروض آوایی','prosodî; awaz û lêdanên axaftinê','prozodia','prozodie; ritm și intonație','просодия; ритм и интонация речи','prozodi; ritëm dhe intonacion','prozodi; konuşmanın ritim ve tonlaması'),
    examples: [
      ex('Im Callcenter-Training wurde nicht nur der Text geübt, sondern auch die Prosodie, damit Entschuldigungen glaubwürdig klingen.', trans('في تدريب مركز الاتصال لم يُتدرّب على النص فقط، بل أيضاً على النبر والتنغيم حتى تبدو الاعتذارات صادقة.','لە ڕاهێنانی کالسەنتەردا نەک تەنها دەقەکە ڕاهێنرا، بەڵکو پرۆسۆدیش، بۆ ئەوەی داوای لێبوردنەکان باوەڕپێکراو دەنگ بدەن.','In call center training, they practiced not only the text but also the prosody so that apologies would sound credible.','در آموزش مرکز تماس، فقط متن تمرین نشد، بلکه آهنگ گفتار هم تمرین شد تا عذرخواهی‌ها قابل‌باور به نظر برسند.','Di perwerdeya navenda telefonê de ne tenê nivîs, lê jî prosodî hat rahênan da ku lêborîn bawerker xuya bikin.','W szkoleniu call center ćwiczono nie tylko tekst, lecz także prozodię, aby przeprosiny brzmiały wiarygodnie.','În trainingul pentru call center s-a exersat nu doar textul, ci și prozodia, pentru ca scuzele să sune credibil.','На тренинге колл-центра отрабатывали не только текст, но и просодию, чтобы извинения звучали убедительно.','Në trajnimin e qendrës së thirrjeve u ushtrua jo vetëm teksti, por edhe prozodia, që kërkimfaljet të tingëllonin të besueshme.','Çağrı merkezi eğitiminde yalnızca metin değil, özürlerin inandırıcı duyulması için prozodi de çalışıldı.')),
      ex('Die Prosodie des Gedichts trägt mehr zur Bedeutung bei als sein vordergründiger Inhalt.', trans('تسهم إيقاعية القصيدة وتنغيمها في المعنى أكثر من مضمونها الظاهر.','پرۆسۆدی شیعرەکە زیاتر لە ناوەڕۆکی ڕووکەشی بەشداری لە مانادا دەکات.','The prosody of the poem contributes more to its meaning than its surface content.','آهنگ و ریتم شعر بیش از محتوای ظاهری آن در معنا نقش دارد.','Prosodiya helbestê ji naveroka wê ya xuya zêdetir beşdarî wateyê dike.','Prozodia wiersza wnosi do znaczenia więcej niż jego powierzchowna treść.','Prozodia poemului contribuie la sens mai mult decât conținutul său aparent.','Просодия стихотворения вносит в смысл больше, чем его поверхностное содержание.','Prozodia e poezisë kontribuon më shumë në kuptim sesa përmbajtja e saj e dukshme.','Şiirin prozodisi, anlamına yüzeydeki içeriğinden daha fazla katkı sağlar.'))
    ]
  }),
  entry({
    word: 'provokant', partOfSpeech: 'Adjective', syllableBreak: 'pro-vo-kant',
    topics: ['business-communication','culture-and-media','social-and-relationships'], usageLabels: ['formal','written','sensitive','advanced'],
    collocations: [{ text: 'eine provokante These', meaning: 'a provocative thesis' }],
    meanings: meaning('مستفز؛ مثير للجدل عمداً','هەستورووژێن؛ بە ئەنقەست مشتومڕدروستکەر','provocative; deliberately challenging','تحریک‌آمیز؛ چالش‌برانگیز عمدی','provokatîf; bi mebest dijwar','prowokacyjny','provocator; provocator','провокационный','provokues','kışkırtıcı; provokatif'),
    examples: [
      ex('Die provokante These im Workshop zwang die Führungskräfte, ihre unausgesprochenen Annahmen über Leistung offenzulegen.', trans('أجبرت الأطروحة المستفزة في ورشة العمل القادة على كشف افتراضاتهم غير المعلنة حول الأداء.','تێزی هەستورووژێن لە وۆرکشۆپەکەدا بەڕێوەبەرەکانی ناچار کرد گریمانە نەگوتراوەکانیان لەبارەی کارایی ئاشکرا بکەن.','The provocative thesis in the workshop forced the managers to disclose their unspoken assumptions about performance.','تز تحریک‌آمیز در کارگاه مدیران را وادار کرد فرض‌های ناگفته خود درباره عملکرد را آشکار کنند.','Teza provokatîf di atolyeyê de rêveber neçar kir ku texmînên xwe yên negotî li ser performansê eşkere bikin.','Prowokacyjna teza w warsztacie zmusiła kadrę kierowniczą do ujawnienia niewypowiedzianych założeń dotyczących wyników.','Teza provocatoare din workshop i-a obligat pe manageri să își dezvăluie presupunerile nerostite despre performanță.','Провокационный тезис на семинаре заставил руководителей раскрыть свои невысказанные предположения о результативности.','Teza provokuese në punëtori i detyroi drejtuesit të zbulonin supozimet e tyre të pathëna për performancën.','Atölyedeki provokatif tez, yöneticileri performansa dair dile getirilmemiş varsayımlarını açıklamaya zorladı.')),
      ex('Der Künstler wählte ein provokantes Motiv, nicht um zu schockieren, sondern um die Routine des Sehens zu unterbrechen.', trans('اختار الفنان موضوعاً مستفزاً لا بغرض الصدمة، بل لقطع روتين النظر.','هونەرمەندەکە مۆتیفێکی هەستورووژێنی هەڵبژارد، نە بۆ شۆککردن، بەڵکو بۆ پچڕاندنی ڕۆتینی بینین.','The artist chose a provocative motif not to shock, but to interrupt the routine of seeing.','هنرمند موتیفی تحریک‌آمیز انتخاب کرد، نه برای شوکه‌کردن، بلکه برای قطع‌کردن عادت دیدن.','Hunermend motîfekî provokatîf hilbijart ne ji bo şok kirinê, lê ji bo qutkirina rutîna dîtinê.','Artysta wybrał prowokacyjny motyw nie po to, by szokować, lecz by przerwać rutynę patrzenia.','Artistul a ales un motiv provocator nu pentru a șoca, ci pentru a întrerupe rutina privirii.','Художник выбрал провокационный мотив не для того, чтобы шокировать, а чтобы прервать привычку смотреть.','Artisti zgjodhi një motiv provokues jo për të tronditur, por për të ndërprerë rutinën e të parit.','Sanatçı, şok etmek için değil, görme rutinini kesintiye uğratmak için provokatif bir motif seçti.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 066', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

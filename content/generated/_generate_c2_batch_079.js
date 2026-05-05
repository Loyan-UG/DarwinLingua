const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '079';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['die Textur','tiefgründig','tilgen','die Topologie','der Topos','transzendieren'];

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
    word: 'die Textur', partOfSpeech: 'Noun', article: 'die', plural: 'Texturen', syllableBreak: 'Tex-tur',
    topics: ['culture-and-media','technology-and-it','advanced-analysis'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'eine raue Textur', meaning: 'a rough texture' }, { text: 'die Textur eines Textes', meaning: 'the texture of a text' }],
    meanings: meaning('ملمس؛ نسيج؛ بنية دقيقة','دەستپێهەست؛ پێکهاتەی ورد','texture; fine structure','بافت؛ ساختار ظریف','tekstûr; avahiya hûrgulî','tekstura; faktura','textură; structură fină','текстура; фактура','teksturë; strukturë e imët','doku; ince yapı'),
    examples: [
      ex('Die neue Oberfläche hat eine matte Textur, damit Fingerabdrücke im Kundenterminal weniger sichtbar sind.', meaning('للسطح الجديد ملمس مطفأ كي تكون بصمات الأصابع أقل وضوحاً على جهاز العميل.','ڕووکاری نوێەکە دەستپێهەستی ماتەی هەیە بۆ ئەوەی پەنجەمۆرەکان لە تێرمیناڵی کڕیار کەمتر دیار بن.','The new surface has a matte texture so fingerprints are less visible on the customer terminal.','سطح جدید بافتی مات دارد تا اثر انگشت روی ترمینال مشتری کمتر دیده شود.','Rûbera nû tekstûreke mat heye da ku şopa tilîyan li termînala xerîdar kêmtir xuya be.','Nowa powierzchnia ma matową teksturę, aby odciski palców na terminalu klienta były mniej widoczne.','Noua suprafață are o textură mată, astfel încât amprentele să fie mai puțin vizibile pe terminalul clientului.','Новая поверхность имеет матовую текстуру, чтобы отпечатки пальцев на клиентском терминале были менее заметны.','Sipërfaqja e re ka teksturë mat që gjurmët e gishtave në terminalin e klientit të jenë më pak të dukshme.','Yeni yüzey mat bir dokuya sahip, böylece müşteri terminalindeki parmak izleri daha az görünür.')),
      ex('Die Textur des Essays entsteht aus kurzen Beobachtungen, Zitaten und abrupten Perspektivwechseln.', meaning('تتشكل بنية المقال من ملاحظات قصيرة واقتباسات وتحولات مفاجئة في المنظور.','پێکهاتەی وردی وتارەکە لە چاودێریی کورت، وەرگرتن و گۆڕانی لەناکاوی دیدگاوە دروست دەبێت.','The texture of the essay emerges from short observations, quotations, and abrupt shifts in perspective.','بافت مقاله از مشاهده‌های کوتاه، نقل‌قول‌ها و تغییرهای ناگهانی زاویه دید شکل می‌گیرد.','Tekstûra gotarê ji çavdêriyên kurt, gotinên wergirtî û guherînên ji nişkê ve yên perspektîfê çêdibe.','Tekstura eseju powstaje z krótkich obserwacji, cytatów i nagłych zmian perspektywy.','Textura eseului se formează din observații scurte, citate și schimbări bruște de perspectivă.','Текстура эссе складывается из коротких наблюдений, цитат и резких смен перспективы.','Tekstura e esesë krijohet nga vëzhgime të shkurtra, citime dhe ndryshime të papritura perspektive.','Denemenin dokusu kısa gözlemlerden, alıntılardan ve ani bakış açısı değişimlerinden oluşur.'))
    ]
  }),
  entry({
    word: 'tiefgründig', partOfSpeech: 'Adjective', syllableBreak: 'tief-grün-dig',
    topics: ['advanced-analysis','culture-and-media','business-communication'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'eine tiefgründige Analyse', meaning: 'a profound analysis' }],
    meanings: meaning('عميق التفكير؛ ذو معنى عميق','قووڵ؛ مانای قووڵ هەبوو','profound; deep in meaning','عمیق؛ ژرف‌معنا','kûr; bi wateya kûr','głęboki; wnikliwy','profund; cu sens adânc','глубокий; содержательный','i thellë; kuptimplotë','derin; anlamlı'),
    examples: [
      ex('Die Retrospektive war tiefgründig, weil sie nicht bei Symptomen stehenblieb, sondern Entscheidungsroutinen hinterfragte.', meaning('كانت المراجعة عميقة لأنها لم تتوقف عند الأعراض، بل سألت عن روتينات اتخاذ القرار.','ڕیترۆسپێکتیڤەکە قووڵ بوو، چونکە لە نیشانەکاندا نەوەستا، بەڵکو پرسیاری لە ڕۆتینی بڕیاردان کرد.','The retrospective was profound because it did not stop at symptoms but questioned decision routines.','بازنگری عمیق بود، چون در نشانه‌ها متوقف نشد و عادت‌های تصمیم‌گیری را زیر سؤال برد.','Retrospektîv kûr bû, ji ber ku li nîşaneyan rawestan nekir, lê rutînên biryardanê pirsî.','Retrospektywa była wnikliwa, ponieważ nie zatrzymała się na objawach, lecz zakwestionowała rutyny decyzyjne.','Retrospectiva a fost profundă deoarece nu s-a oprit la simptome, ci a pus sub semnul întrebării rutinele decizionale.','Ретроспектива была глубокой, потому что не остановилась на симптомах, а поставила под вопрос привычки принятия решений.','Retrospektiva ishte e thellë, sepse nuk u ndal te simptomat, por vuri në dyshim rutinat e vendimmarrjes.','Retrospektif derindi, çünkü belirtilerde kalmadı, karar alma rutinlerini sorguladı.')),
      ex('Der Roman wirkt tiefgründig, ohne seine Figuren zu bloßen Trägern philosophischer Ideen zu machen.', meaning('تبدو الرواية عميقة من دون أن تجعل شخصياتها مجرد حوامل لأفكار فلسفية.','ڕۆمانەکە قووڵ دەردەکەوێت، بەبێ ئەوەی کارەکتەرەکانی بکاتە تەنها هەڵگری بیرۆکەی فەلسەفی.','The novel feels profound without turning its characters into mere carriers of philosophical ideas.','رمان عمیق به نظر می‌رسد، بدون آن‌که شخصیت‌هایش را به حاملان صرف ایده‌های فلسفی تبدیل کند.','Roman kûr xuya dike bê ku kesayetên xwe bike tenê hilgirên fikrên felsefî.','Powieść wydaje się głęboka, nie czyniąc postaci jedynie nośnikami idei filozoficznych.','Romanul pare profund fără să își transforme personajele în simpli purtători ai ideilor filosofice.','Роман кажется глубоким, не превращая персонажей в простых носителей философских идей.','Romani duket i thellë pa i kthyer personazhet në bartës të thjeshtë idesh filozofike.','Roman, karakterlerini yalnızca felsefi fikirlerin taşıyıcısına dönüştürmeden derin görünür.'))
    ]
  }),
  entry({
    word: 'tilgen', partOfSpeech: 'Verb', infinitive: 'tilgen', syllableBreak: 'til-gen',
    topics: ['finance-and-accounting','law-and-compliance','documents-and-administration'], usageLabels: ['formal','written','business','administrative'],
    collocations: [{ text: 'Schulden tilgen', meaning: 'to repay debts' }, { text: 'einen Eintrag tilgen', meaning: 'to erase an entry' }],
    meanings: meaning('يسدد؛ يمحو؛ يزيل','دانەوەی قەرز؛ سڕینەوە','to repay; to erase; to extinguish','تسویه کردن؛ پاک کردن؛ زدودن','dayîna deynê; jêbirin','spłacać; usuwać','a rambursa; a șterge','погашать; удалять','shlyej; fshij','borç ödemek; silmek'),
    examples: [
      ex('Das Unternehmen will die Altschulden innerhalb von drei Jahren tilgen, ohne die Investitionen in Sicherheit zu kürzen.', meaning('تريد الشركة سداد الديون القديمة خلال ثلاث سنوات من دون خفض الاستثمارات في الأمن.','کۆمپانیاکە دەیەوێت قەرزە کۆنەکان لە ماوەی سێ ساڵدا بداتەوە، بەبێ کەمکردنەوەی وەبەرهێنان لە ئاسایش.','The company wants to repay the old debts within three years without cutting investments in security.','شرکت می‌خواهد بدهی‌های قدیمی را ظرف سه سال تسویه کند، بدون آن‌که سرمایه‌گذاری در امنیت را کاهش دهد.','Şirket dixwaze deynên kevn di nav sê salan de bide bê ku veberhênanên ewlehiyê kêm bike.','Firma chce spłacić stare długi w ciągu trzech lat, nie ograniczając inwestycji w bezpieczeństwo.','Compania vrea să ramburseze datoriile vechi în trei ani fără a reduce investițiile în securitate.','Компания хочет погасить старые долги в течение трех лет, не сокращая инвестиции в безопасность.','Kompania dëshiron të shlyejë borxhet e vjetra brenda tre vitesh pa shkurtuar investimet në siguri.','Şirket eski borçları üç yıl içinde, güvenlik yatırımlarını kısmadan ödemek istiyor.')),
      ex('Der Versuch, jede Spur der Vergangenheit zu tilgen, macht die Erinnerung im Roman nur noch mächtiger.', meaning('إن محاولة محو كل أثر للماضي تجعل الذاكرة في الرواية أقوى فقط.','هەوڵی سڕینەوەی هەر شوێنەوارێکی ڕابردوو بیرەوەری لە ڕۆمانەکەدا تەنها بەهێزتر دەکات.','The attempt to erase every trace of the past only makes memory more powerful in the novel.','تلاش برای پاک کردن هر رد گذشته، خاطره را در رمان فقط قدرتمندتر می‌کند.','Hewla jêbirina her şopa rabirdûyê di romanê de bîranînê tenê hêzdartir dike.','Próba usunięcia każdego śladu przeszłości czyni pamięć w powieści tylko potężniejszą.','Încercarea de a șterge orice urmă a trecutului face memoria doar mai puternică în roman.','Попытка стереть каждый след прошлого в романе лишь делает память сильнее.','Përpjekja për të fshirë çdo gjurmë të së kaluarës e bën kujtesën në roman vetëm më të fuqishme.','Geçmişin her izini silme girişimi romanda hafızayı yalnızca daha güçlü kılar.'))
    ]
  }),
  entry({
    word: 'die Topologie', partOfSpeech: 'Noun', article: 'die', plural: 'Topologien', syllableBreak: 'To-po-lo-gie',
    topics: ['technology-and-it','advanced-analysis','education-and-training'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'die Topologie eines Netzwerks', meaning: 'the topology of a network' }],
    meanings: meaning('طوبولوجيا؛ بنية العلاقات المكانية أو الشبكية','تۆپۆلۆژیا؛ پێکهاتەی پەیوەندیی شوێنی یان تۆڕی','topology; structure of spatial or network relations','توپولوژی؛ ساختار روابط فضایی یا شبکه‌ای','topolojî; avahiya têkiliyên cihî an torî','topologia','topologie','топология','topologji','topoloji'),
    examples: [
      ex('Die Topologie des Netzwerks erklärte, warum ein kleiner Routerausfall mehrere Standorte gleichzeitig traf.', meaning('فسرت طوبولوجيا الشبكة لماذا أصاب تعطل موجه صغير عدة مواقع في الوقت نفسه.','تۆپۆلۆژیای تۆڕەکە ڕوونی کردەوە بۆچی وەستانی ڕاوتەرێکی بچووک چەند شوێنێکی هاوکات گرت.','The topology of the network explained why a small router failure affected several locations at the same time.','توپولوژی شبکه توضیح داد چرا خرابی یک روتر کوچک هم‌زمان چند شعبه را تحت تأثیر قرار داد.','Topolojiya torê rave kir ka çima qutbûna routerekî biçûk çend cihan bi hev re bandor kir.','Topologia sieci wyjaśniła, dlaczego awaria małego routera jednocześnie dotknęła kilka lokalizacji.','Topologia rețelei a explicat de ce defectarea unui router mic a afectat simultan mai multe locații.','Топология сети объяснила, почему сбой небольшого маршрутизатора одновременно затронул несколько площадок.','Topologjia e rrjetit shpjegoi pse dështimi i një routeri të vogël preku disa vendndodhje njëkohësisht.','Ağın topolojisi, küçük bir yönlendirici arızasının neden aynı anda birkaç lokasyonu etkilediğini açıkladı.')),
      ex('In der Ausstellung entsteht eine Topologie der Erinnerung, in der private Räume und politische Orte ineinander übergehen.', meaning('في المعرض تنشأ طوبولوجيا للذاكرة تتداخل فيها المساحات الخاصة والأماكن السياسية.','لە پێشانگاکەدا تۆپۆلۆژیایەکی بیرەوەری دروست دەبێت کە تێیدا شوێنە تایبەتییەکان و شوێنە سیاسییەکان تێکەڵ دەبن.','In the exhibition, a topology of memory emerges in which private rooms and political places merge into one another.','در نمایشگاه، توپولوژی‌ای از حافظه شکل می‌گیرد که در آن فضاهای خصوصی و مکان‌های سیاسی در هم می‌روند.','Di pêşangehê de topolojiyeke bîranînê çêdibe ku tê de odeyên taybet û cihên siyasî dikevin nav hev.','W wystawie powstaje topologia pamięci, w której przestrzenie prywatne i miejsca polityczne przenikają się.','În expoziție apare o topologie a memoriei în care spațiile private și locurile politice se întrepătrund.','В выставке возникает топология памяти, где частные пространства и политические места переходят друг в друга.','Në ekspozitë krijohet një topologji e kujtesës ku hapësirat private dhe vendet politike ndërthuren.','Sergide özel mekanlar ile siyasi yerlerin iç içe geçtiği bir hafıza topolojisi oluşur.'))
    ]
  }),
  entry({
    word: 'der Topos', partOfSpeech: 'Noun', article: 'der', plural: 'Topoi', syllableBreak: 'To-pos',
    topics: ['culture-and-media','education-and-training','advanced-analysis'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'ein literarischer Topos', meaning: 'a literary topos or recurring motif' }],
    meanings: meaning('موتيف متكرر؛ موضع بلاغي','مۆتیفی دووبارەبووە؛ تۆپۆس','topos; recurring motif or rhetorical commonplace','توپوس؛ مضمون یا موتیف تکرارشونده','topos; motîfa dubare','topos; motyw powracający','topos; motiv recurent','топос; устойчивый мотив','topos; motiv i përsëritur','topos; yinelenen motif'),
    examples: [
      ex('Der Topos der transparenten Organisation wird oft bemüht, ohne konkrete Informationsrechte zu definieren.', meaning('كثيراً ما يُستدعى موتيف المنظمة الشفافة من دون تعريف حقوق معلومات ملموسة.','تۆپۆسی ڕێکخراوی شەفاف زۆرجار بەکاردێت، بەبێ دیاریکردنی مافی زانیاریی دیاریکراو.','The topos of the transparent organization is often invoked without defining concrete information rights.','توپوس سازمان شفاف اغلب مطرح می‌شود، بدون آن‌که حقوق اطلاعاتی مشخص تعریف شود.','Toposa rêxistina şefaf gelek caran tê gotin bê ku mafên agahiyê yên zehmet bên pênasekirin.','Topos przejrzystej organizacji często się przywołuje, nie definiując konkretnych praw do informacji.','Toposul organizației transparente este invocat adesea fără a defini drepturi concrete la informație.','Топос прозрачной организации часто используется без определения конкретных прав на информацию.','Toposi i organizatës transparente shpesh përmendet pa përcaktuar të drejta konkrete informacioni.','Şeffaf organizasyon toposu somut bilgi hakları tanımlanmadan sık sık kullanılır.')),
      ex('Der Garten als Topos der verlorenen Unschuld wird im Gedicht mehrfach variiert.', meaning('يتنوع Motif الحديقة بوصفه رمز البراءة المفقودة عدة مرات في القصيدة.','باخچە وەک تۆپۆسی بێگوناهایی لەدەستچوو لە شیعرەکەدا چەند جار جیاواز دەکرێتەوە.','The garden as a topos of lost innocence is varied several times in the poem.','باغ به‌عنوان توپوس معصومیت ازدست‌رفته در شعر چندین بار دگرگون می‌شود.','Baxçe wek toposa bêgunehiya winda di helbestê de çend caran tê guherandin.','Ogród jako topos utraconej niewinności jest w wierszu kilkakrotnie wariowany.','Grădina ca topos al inocenței pierdute este variată de mai multe ori în poem.','Сад как топос утраченной невинности несколько раз варьируется в стихотворении.','Kopshti si topos i pafajësisë së humbur variohet disa herë në poezi.','Kayıp masumiyet toposu olarak bahçe şiirde birkaç kez çeşitlendirilir.'))
    ]
  }),
  entry({
    word: 'transzendieren', partOfSpeech: 'Verb', infinitive: 'transzendieren', syllableBreak: 'tran-szen-die-ren',
    topics: ['advanced-analysis','culture-and-media','education-and-training'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'Grenzen transzendieren', meaning: 'to transcend boundaries' }],
    meanings: meaning('يتجاوز؛ يسمو فوق حدود','سنوور تێپەڕاندن؛ بەرزبوونەوە لەسەر','to transcend; to go beyond','فراتر رفتن؛ از مرزها گذشتن','derbasî sînoran bûn; bilindtir çûn','transcendować; wykraczać poza','a transcende; a depăși','трансцендировать; выходить за пределы','transcendoj; tejkaloj','aşmak; ötesine geçmek'),
    examples: [
      ex('Das neue Architekturmodell soll Abteilungsgrenzen transzendieren, statt sie in digitaler Form zu reproduzieren.', meaning('ينبغي لنموذج البنية الجديد أن يتجاوز حدود الأقسام بدلاً من إعادة إنتاجها رقمياً.','مۆدێلی نوێی ئەندازیاری دەبێت سنووری بەشەکان تێپەڕێنێت، نەک بە شێوەی دیجیتاڵی دووبارەیان بکاتەوە.','The new architecture model is meant to transcend departmental boundaries instead of reproducing them in digital form.','مدل معماری جدید باید از مرزهای واحدها فراتر برود، نه اینکه آن‌ها را به شکل دیجیتال بازتولید کند.','Modela nû ya avahiyê divê sînorên beşan derbas bike, ne ku wan bi şêweya dîjîtal dubare bike.','Nowy model architektury ma przekraczać granice działów, zamiast reprodukować je w formie cyfrowej.','Noul model de arhitectură trebuie să transcendă granițele departamentale, nu să le reproducă în formă digitală.','Новая архитектурная модель должна преодолевать границы отделов, а не воспроизводить их в цифровой форме.','Modeli i ri i arkitekturës duhet të tejkalojë kufijtë e departamenteve, jo t’i riprodhojë ato në formë digjitale.','Yeni mimari modeli departman sınırlarını dijital biçimde yeniden üretmek yerine aşmalıdır.')),
      ex('Die Musik transzendiert im Schluss die Handlung und öffnet einen Raum, in dem Trauer nicht mehr erklärbar sein muss.', meaning('في النهاية تتجاوز الموسيقى الحبكة وتفتح فضاء لا يعود فيه الحزن بحاجة إلى تفسير.','مۆسیقا لە کۆتاییدا ڕووداوەکان تێپەڕێنێت و شوێنێک دەکاتەوە کە خەمگینی ئیتر پێویستی بە ڕوونکردنەوە نییە.','At the end, the music transcends the plot and opens a space in which grief no longer has to be explained.','در پایان، موسیقی از داستان فراتر می‌رود و فضایی می‌گشاید که اندوه دیگر لازم نیست توضیح داده شود.','Di dawiyê de muzîk çîrokê derbas dike û cihê vedike ku êdî pêwîst nake xem were ravekirin.','Muzyka w finale transcenduje fabułę i otwiera przestrzeń, w której żal nie musi już być wyjaśniany.','Muzica transcende acțiunea în final și deschide un spațiu în care doliul nu mai trebuie explicat.','В финале музыка превосходит сюжет и открывает пространство, где горе больше не нуждается в объяснении.','Në fund, muzika e tejkalon ngjarjen dhe hap një hapësirë ku pikëllimi nuk ka më nevojë të shpjegohet.','Müzik finalde olay örgüsünü aşar ve kederin artık açıklanmak zorunda olmadığı bir alan açar.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 079', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

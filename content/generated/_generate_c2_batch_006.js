const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '006';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const projectPath = path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj');
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['die Anmaßung','der Anschein','anschmeißen','die Anspielung','anstiften','anzüglich'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const labelMap = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const tokens = fs.readFileSync(sourcePath, 'utf8').split(',').map(t => t.trim()).filter(Boolean);
const words = tokens.slice(0, 6);
if (JSON.stringify(words) !== JSON.stringify(expected)) throw new Error(`Unexpected first tokens: ${JSON.stringify(words)}`);
const usedLabels = ['formal','informal','written','advanced','business','workplace','analysis','sensitive'];
for (const key of usedLabels) if (!labelMap.has(key)) throw new Error(`Missing taxonomy label: ${key}`);
const labels = usedLabels.map(key => labelMap.get(key));
function meanings(obj) { return langs.map(language => ({ language, text: obj[language] })); }
function ex(baseText, obj) { return { baseText, translations: meanings(obj) }; }

const entries = [
  {
    word: 'die Anmaßung', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Anmaßungen', infinitive: null, pronunciationIpa: null, syllableBreak: 'An-ma-ßung',
    topics: ['business-communication','management-and-leadership','social-and-relationships'], usageLabels: ['formal','written','advanced','sensitive'], contextLabels: [],
    grammarNotes: ['feminine noun; often describes presumptuous or arrogant overstepping'],
    collocations: [{ text: 'eine unerträgliche Anmaßung', meaning: 'an intolerable presumption' }],
    wordFamilies: [{ lemma: 'anmaßend', relationLabel: 'adjective', note: null }, { lemma: 'sich anmaßen', relationLabel: 'verb', note: null }], relations: [],
    meanings: meanings({ ar:'غطرسة؛ ادعاء متجاوز؛ وقاحة', ckb:'خۆبەزلزانی؛ داوای بێجێ؛ سنووربەزاندن', en:'presumption; arrogance; overstepping', fa:'گستاخی؛ خودبزرگ‌بینی؛ ادعای بی‌جا', kmr:'xwe-mezinbînî; daxwaza bêcih; derbasbûna sînor', pl:'arogancja; uzurpacja; zarozumiałość', ro:'aroganță; pretenție nejustificată; depășire a limitelor', ru:'самонадеянность; высокомерие; неправомерное притязание', sq:'arrogancë; pretendim i pajustifikuar; tejkalim', tr:'kibir; haddini aşma; yersiz iddia' }),
    examples: [
      ex('Die Anmaßung, ohne Mandat im Namen des gesamten Teams zu sprechen, führte zu erheblichem Unmut.', { ar:'أدى الادعاء بالتحدث باسم الفريق كله من دون تفويض إلى استياء كبير.', ckb:'ئەو سنووربەزاندنەی بەبێ ماندات بە ناوی هەموو تیمەکەوە قسە بکات، ناڕەزاییەکی زۆری دروست کرد.', en:'The presumption of speaking on behalf of the entire team without a mandate caused considerable resentment.', fa:'این گستاخی که بدون اختیار از طرف کل تیم صحبت کند، نارضایتی زیادی ایجاد کرد.', kmr:'Xwe-mezinbîniya ku bê destûr bi navê hemû tîmê biaxive, nerazîbûneke mezin çêkir.', pl:'Uzurpacja prawa do wypowiadania się w imieniu całego zespołu bez mandatu wywołała znaczne niezadowolenie.', ro:'Pretenția de a vorbi în numele întregii echipe fără mandat a provocat nemulțumiri considerabile.', ru:'Самонадеянная попытка говорить от имени всей команды без полномочий вызвала значительное недовольство.', sq:'Pretendimi për të folur në emër të gjithë ekipit pa mandat shkaktoi pakënaqësi të madhe.', tr:'Yetki olmadan tüm ekip adına konuşma haddini aşması ciddi hoşnutsuzluğa yol açtı.' }),
      ex('Viele empfanden es als Anmaßung, dass die Behörde die Betroffenen erst nach der Entscheidung anhörte.', { ar:'اعتبر كثيرون أنه تجاوز أن تسمع الجهة الحكومية المتأثرين فقط بعد اتخاذ القرار.', ckb:'زۆر کەس ئەوەیان بە خۆبەزلزانی زانی کە دامەزراوەکە تەنها دوای بڕیارەکە گوێی لە کەسانی کاریگەری لێکەوتوو گرت.', en:'Many saw it as presumptuous that the authority heard those affected only after the decision had been made.', fa:'بسیاری این را گستاخی دانستند که اداره فقط پس از تصمیم‌گیری نظر افراد درگیر را شنید.', kmr:'Gelekan ew wek xwe-mezinbînî dît ku dezgeh tenê piştî biryarê guhdarîya kesên bandorlêketî kir.', pl:'Wiele osób uznało za arogancję, że urząd wysłuchał zainteresowanych dopiero po podjęciu decyzji.', ro:'Mulți au considerat o aroganță faptul că autoritatea i-a ascultat pe cei afectați abia după decizie.', ru:'Многие сочли самонадеянностью то, что ведомство выслушало затронутых людей только после принятия решения.', sq:'Shumë e panë si arrogancë faktin që autoriteti i dëgjoi të prekurit vetëm pas vendimit.', tr:'Birçok kişi, kurumun etkilenenleri ancak karardan sonra dinlemesini haddini aşmak olarak gördü.' })
    ]
  },
  {
    word: 'der Anschein', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'der', plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'An-schein',
    topics: ['advanced-analysis','business-communication','law-and-compliance'], usageLabels: ['formal','written','analysis','business'], contextLabels: [],
    grammarNotes: ['masculine noun; usually singular; often in phrases like dem Anschein nach and den Anschein erwecken'],
    collocations: [{ text: 'den Anschein erwecken', meaning: 'to give the impression' }],
    wordFamilies: [{ lemma: 'scheinen', relationLabel: 'verb', note: null }], relations: [],
    meanings: meanings({ ar:'مظهر؛ انطباع؛ ظاهر الأمر', ckb:'دەرکەوتن؛ وێنەی دەرەکی؛ وا دیاربوون', en:'appearance; impression; semblance', fa:'ظاهر؛ نمود؛ برداشت اولیه', kmr:'xuyabûn; bandora derveyî; şibandin', pl:'pozór; wrażenie; wygląd', ro:'aparență; impresie; aparență exterioară', ru:'видимость; впечатление; кажимость', sq:'pamje e jashtme; përshtypje; dukje', tr:'görünüş; izlenim; intiba' }),
    examples: [
      ex('Der Bericht erweckte den Anschein vollständiger Transparenz, verschwieg aber die wichtigsten Annahmen.', { ar:'أعطى التقرير انطباعاً بالشفافية الكاملة، لكنه أخفى أهم الافتراضات.', ckb:'ڕاپۆرتەکە وێنەی ڕوونی تەواوی دروست کرد، بەڵام گرنگترین گریمانەکانی شاردەوە.', en:'The report gave the impression of full transparency but concealed the most important assumptions.', fa:'گزارش ظاهری از شفافیت کامل ایجاد می‌کرد، اما مهم‌ترین فرض‌ها را پنهان می‌کرد.', kmr:'Rapor bandora şefafiyeta temam da, lê girîngtirîn texmînan veşart.', pl:'Raport sprawiał wrażenie pełnej przejrzystości, ale przemilczał najważniejsze założenia.', ro:'Raportul dădea impresia unei transparențe complete, dar ascundea cele mai importante ipoteze.', ru:'Отчёт создавал впечатление полной прозрачности, но умалчивал о важнейших допущениях.', sq:'Raporti krijonte përshtypjen e transparencës së plotë, por fshihte supozimet më të rëndësishme.', tr:'Rapor tam şeffaflık izlenimi veriyordu, ancak en önemli varsayımları gizliyordu.' }),
      ex('Dem Anschein nach war die Beschwerde erledigt, doch der Kunde wartete weiterhin auf eine verbindliche Antwort.', { ar:'بحسب الظاهر كانت الشكوى قد أُغلقت، لكن العميل كان لا يزال ينتظر جواباً ملزماً.', ckb:'بە دەرکەوتنەوە، سکاڵاکە چارەسەر کرابوو، بەڵام کڕیارەکە هێشتا چاوەڕێی وەڵامێکی پابەند بوو.', en:'Apparently the complaint had been resolved, but the customer was still waiting for a binding response.', fa:'ظاهراً شکایت حل شده بود، اما مشتری همچنان منتظر پاسخی الزام‌آور بود.', kmr:'Li gor xuyabûnê gilî hatibû çareserkirin, lê xerîdar hîn li benda bersiveke girêdayî bû.', pl:'Z pozoru reklamacja była załatwiona, lecz klient nadal czekał na wiążącą odpowiedź.', ro:'După aparențe, reclamația fusese rezolvată, însă clientul încă aștepta un răspuns ferm.', ru:'С виду жалоба была закрыта, но клиент всё ещё ждал обязательного ответа.', sq:'Në dukje ankesa ishte zgjidhur, por klienti ende priste një përgjigje detyruese.', tr:'Görünüşe göre şikâyet çözülmüştü, ancak müşteri hâlâ bağlayıcı bir yanıt bekliyordu.' })
    ]
  },
  {
    word: 'anschmeißen', language: 'de', cefrLevel: level, partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'anschmeißen', pronunciationIpa: null, syllableBreak: 'an-schmei-ßen',
    topics: ['technology-and-it','everyday-life','work-and-jobs'], usageLabels: ['informal','workplace'], contextLabels: [],
    grammarNotes: ['separable informal verb; schmeißt an, schmiss an, hat angeschmissen; means to start or switch on a machine/system'],
    collocations: [{ text: 'den Rechner anschmeißen', meaning: 'to boot up or start the computer informally' }],
    wordFamilies: [{ lemma: 'schmeißen', relationLabel: 'verb', note: 'informal base verb' }], relations: [],
    meanings: meanings({ ar:'يشغّل؛ يبدأ تشغيله بلهجة عامية', ckb:'بە شێوەی نافەرمی کارپێبکات؛ داگیرساندن/کارپێکردن', en:'to fire up; to switch on; to start up', fa:'روشن کردن؛ راه انداختن؛ به کار انداختن', kmr:'dest pê kirin; vekirin; xebitandin', pl:'odpalić; uruchomić; włączyć', ro:'a porni; a da drumul la', ru:'запустить; включить', sq:'ta ndezësh; ta vësh në punë', tr:'çalıştırmak; açmak; devreye sokmak' }),
    examples: [
      ex('Ich schmeiße kurz die Testumgebung an, dann sehen wir, ob der Fehler reproduzierbar ist.', { ar:'سأشغّل بيئة الاختبار سريعاً، ثم نرى ما إذا كان الخطأ قابلاً للتكرار.', ckb:'بە خێرایی ژینگەی تاقیکردنەوەکە کارپێدەکەم، پاشان دەبینین هەڵەکە دووبارە دەکرێتەوە یان نا.', en:'I’ll quickly fire up the test environment, then we’ll see whether the bug is reproducible.', fa:'سریع محیط تست را بالا می‌آورم، بعد می‌بینیم آیا خطا قابل بازتولید است یا نه.', kmr:'Ez zû hawîrdora testê dixebitînim, paşê em dibînin ka xeletî dubare dibe an na.', pl:'Szybko odpalę środowisko testowe, potem zobaczymy, czy błąd da się odtworzyć.', ro:'Pornesc rapid mediul de testare, apoi vedem dacă eroarea poate fi reprodusă.', ru:'Я быстро запущу тестовую среду, затем посмотрим, воспроизводится ли ошибка.', sq:'Po e ndez shpejt mjedisin e testimit, pastaj shohim nëse gabimi riprodhohet.', tr:'Test ortamını hızlıca ayağa kaldırayım, sonra hatanın yeniden üretilebilir olup olmadığını görürüz.' }),
      ex('Wenn es kalt wird, schmeißen wir den alten Ofen im Vereinsraum wieder an.', { ar:'عندما يصبح الجو بارداً، نشغّل الموقد القديم في غرفة الجمعية مرة أخرى.', ckb:'کاتێک سارد دەبێت، سۆبای کۆنەکەی ژووری یانەکە جارێکی تر داگیرسێنین.', en:'When it gets cold, we fire up the old stove in the club room again.', fa:'وقتی هوا سرد می‌شود، بخاری قدیمی اتاق انجمن را دوباره روشن می‌کنیم.', kmr:'Dema sar dibe, em sobeya kevn a odeya komeleyê dîsa vedikin.', pl:'Gdy robi się zimno, znów odpalamy stary piec w sali klubowej.', ro:'Când se face frig, pornim din nou soba veche din sala clubului.', ru:'Когда становится холодно, мы снова разжигаем старую печь в клубной комнате.', sq:'Kur bëhet ftohtë, e ndezim përsëri sobën e vjetër në dhomën e klubit.', tr:'Hava soğuyunca dernek odasındaki eski sobayı yeniden yakıyoruz.' })
    ]
  },
  {
    word: 'die Anspielung', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Anspielungen', infinitive: null, pronunciationIpa: null, syllableBreak: 'An-spie-lung',
    topics: ['business-communication','culture-and-media','social-and-relationships'], usageLabels: ['formal','written','advanced','sensitive'], contextLabels: [],
    grammarNotes: ['feminine noun; means indirect reference or allusion'],
    collocations: [{ text: 'eine subtile Anspielung', meaning: 'a subtle allusion' }],
    wordFamilies: [{ lemma: 'anspielen', relationLabel: 'verb', note: null }], relations: [],
    meanings: meanings({ ar:'تلميح؛ إشارة غير مباشرة', ckb:'ئاماژەی ناڕاستەوخۆ؛ تێکەڵکردنی مانای شاراوە', en:'allusion; indirect reference', fa:'اشاره غیرمستقیم؛ کنایه؛ تلمیح', kmr:'îşareta nerasterast; telmîh', pl:'aluzja; nawiązanie pośrednie', ro:'aluzie; referire indirectă', ru:'намёк; аллюзия; косвенная отсылка', sq:'aludim; referencë e tërthortë', tr:'ima; dolaylı gönderme' }),
    examples: [
      ex('Die Anspielung auf frühere Budgetfehler blieb im Meeting nicht unbemerkt.', { ar:'لم يمر التلميح إلى أخطاء الميزانية السابقة دون ملاحظة في الاجتماع.', ckb:'ئاماژە ناڕاستەوخۆکە بۆ هەڵەکانی بودجەی پێشوو لە کۆبوونەوەکەدا بێسەرنج نەما.', en:'The allusion to earlier budget mistakes did not go unnoticed in the meeting.', fa:'اشاره غیرمستقیم به خطاهای بودجه‌ای گذشته در جلسه بی‌توجه نماند.', kmr:'Telmîha li ser xeletiyên berê yên budceyê di civînê de bêdîtin nema.', pl:'Aluzja do wcześniejszych błędów budżetowych nie pozostała na spotkaniu niezauważona.', ro:'Aluzia la greșelile bugetare anterioare nu a trecut neobservată în ședință.', ru:'Намёк на прежние бюджетные ошибки не остался незамеченным на совещании.', sq:'Aludimi për gabimet e mëparshme buxhetore nuk kaloi pa u vënë re në mbledhje.', tr:'Önceki bütçe hatalarına yapılan ima toplantıda fark edilmeden geçmedi.' }),
      ex('Der Titel enthält eine Anspielung auf einen bekannten Mythos, ohne ihn ausdrücklich zu nennen.', { ar:'يتضمن العنوان تلميحاً إلى أسطورة معروفة من دون ذكرها صراحة.', ckb:'ناونیشانەکە ئاماژەیەکی ناڕاستەوخۆ بە ئەفسانەیەکی ناسراو دەکات، بەبێ ئەوەی بە ڕوونی ناوی بهێنێت.', en:'The title contains an allusion to a well-known myth without naming it explicitly.', fa:'عنوان حاوی اشاره‌ای غیرمستقیم به یک اسطوره شناخته‌شده است، بدون آنکه صریحاً نام آن را بیاورد.', kmr:'Sernav telmîhek li ser mîtoseke naskirî dihewîne, bê ku wê bi eşkere nav bike.', pl:'Tytuł zawiera aluzję do znanego mitu, nie wymieniając go wprost.', ro:'Titlul conține o aluzie la un mit cunoscut, fără să îl numească explicit.', ru:'Название содержит отсылку к известному мифу, не называя его прямо.', sq:'Titulli përmban një aludim për një mit të njohur pa e përmendur shprehimisht.', tr:'Başlık, açıkça adını anmadan bilinen bir mite gönderme içerir.' })
    ]
  },
  {
    word: 'anstiften', language: 'de', cefrLevel: level, partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'anstiften', pronunciationIpa: null, syllableBreak: 'an-stif-ten',
    topics: ['law-and-compliance','social-and-relationships','quality-and-risk'], usageLabels: ['formal','written','sensitive'], contextLabels: [],
    grammarNotes: ['separable verb; stiftet an, stiftete an, hat angestiftet; often means to incite someone to wrongdoing'],
    collocations: [{ text: 'zu einer Straftat anstiften', meaning: 'to incite someone to commit a crime' }],
    wordFamilies: [{ lemma: 'die Anstiftung', relationLabel: 'noun', note: null }], relations: [],
    meanings: meanings({ ar:'يحرّض؛ يدفع إلى فعل خاطئ', ckb:'هان بدات؛ بەرەو کارێکی خراپ پاڵ بنێت', en:'to incite; to instigate', fa:'تحریک کردن؛ واداشتن به کار نادرست', kmr:'han dan; teşwîq kirin bo karê xerab', pl:'podżegać; nakłaniać do złego', ro:'a instiga; a incita', ru:'подстрекать; побуждать к проступку', sq:'të nxisësh; të shtysh në veprim të gabuar', tr:'kışkırtmak; azmettirmek' }),
    examples: [
      ex('Die Ermittlungen sollten klären, ob der Berater den Mitarbeiter zur Manipulation der Zahlen angestiftet hatte.', { ar:'كان على التحقيقات أن توضح ما إذا كان المستشار قد حرّض الموظف على التلاعب بالأرقام.', ckb:'لێکۆڵینەوەکان دەبوو ڕوون بکەنەوە ئایا ڕاوێژکارەکە کارمەندەکەی هانداوە بۆ دەستکاریکردنی ژمارەکان.', en:'The investigation was meant to determine whether the consultant had incited the employee to manipulate the figures.', fa:'تحقیقات باید روشن می‌کرد که آیا مشاور کارمند را به دستکاری ارقام تحریک کرده بود یا نه.', kmr:'Lêpirsîn diviya zelal bikira ka şêwirmend karmend han dabû ku hejmaran biguherîne an na.', pl:'Śledztwo miało wyjaśnić, czy doradca nakłonił pracownika do manipulowania danymi.', ro:'Ancheta trebuia să clarifice dacă consultantul îl instigase pe angajat să manipuleze cifrele.', ru:'Расследование должно было выяснить, подстрекал ли консультант сотрудника к манипуляции цифрами.', sq:'Hetimi duhej të sqaronte nëse këshilltari e kishte nxitur punonjësin të manipulonte shifrat.', tr:'Soruşturma, danışmanın çalışanı rakamları manipüle etmeye azmettirip azmettirmediğini açıklığa kavuşturmalıydı.' }),
      ex('Der ältere Bruder stiftete die Kinder nicht zu Mutproben an, sondern versuchte sie davon abzuhalten.', { ar:'لم يحرّض الأخ الأكبر الأطفال على اختبارات الجرأة، بل حاول منعهم منها.', ckb:'برا گەورەکە منداڵەکانی هان نەدا بۆ تاقیکردنەوەی بوێری، بەڵکو هەوڵی دا ڕێگرییان لێبکات.', en:'The older brother did not incite the children to dares; he tried to stop them.', fa:'برادر بزرگ‌تر کودکان را به کارهای خطرناک تحریک نکرد، بلکه تلاش کرد جلوی آن‌ها را بگیرد.', kmr:'Birayê mezin zarok han neda bo ceribandinên wêrekî; hewl da ku wan rawestîne.', pl:'Starszy brat nie podżegał dzieci do prób odwagi, lecz próbował je powstrzymać.', ro:'Fratele mai mare nu i-a instigat pe copii la probe de curaj, ci a încercat să-i oprească.', ru:'Старший брат не подстрекал детей к испытаниям на смелость, а пытался их остановить.', sq:'Vëllai i madh nuk i nxiti fëmijët për prova guximi, por u përpoq t’i ndalonte.', tr:'Ağabey çocukları cesaret denemelerine kışkırtmadı, aksine onları durdurmaya çalıştı.' })
    ]
  },
  {
    word: 'anzüglich', language: 'de', cefrLevel: level, partOfSpeech: 'Adjective', article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'an-züg-lich',
    topics: ['business-communication','social-and-relationships','work-and-jobs'], usageLabels: ['formal','written','sensitive'], contextLabels: [],
    grammarNotes: ['adjective; describes suggestive, indecent, or sexually insinuating remarks or behavior'],
    collocations: [{ text: 'eine anzügliche Bemerkung', meaning: 'a suggestive or indecent remark' }],
    wordFamilies: [{ lemma: 'die Anzüglichkeit', relationLabel: 'noun', note: null }], relations: [],
    meanings: meanings({ ar:'موحٍ بشكل غير لائق؛ ذو تلميح جنسي', ckb:'ئاماژەدار بە شێوەی نەگونجاو؛ سێکسی بە تێکەڵکردنی شاراوە', en:'suggestive; indecent; sexually insinuating', fa:'کنایه‌آمیز و نامناسب؛ دارای اشاره جنسی', kmr:'bi îşareta neguncav; cinsî bi awayekî veşartî', pl:'dwuznaczny; nieprzyzwoity; z podtekstem seksualnym', ro:'aluziv indecent; cu insinuare sexuală', ru:'непристойно-двусмысленный; с сексуальным намёком', sq:'sugjestiv në mënyrë të papërshtatshme; me nënkuptim seksual', tr:'müstehcen imalı; uygunsuz çağrışımlı' }),
    examples: [
      ex('Anzügliche Bemerkungen haben in einem Kundengespräch keinen Platz, selbst wenn sie als Scherz gemeint sind.', { ar:'لا مكان للتعليقات ذات الإيحاء غير اللائق في حديث مع عميل، حتى لو كانت مقصودة كمزحة.', ckb:'تێبینییە ئاماژەدارە نەگونجاوەکان لە گفتوگۆی کڕیاردا شوێنیان نییە، تەنانەت ئەگەر وەک گاڵتە مەبەست کرابن.', en:'Suggestive remarks have no place in a customer conversation, even if they are meant as a joke.', fa:'اظهارنظرهای نامناسب و کنایه‌آمیز در گفت‌وگو با مشتری جایی ندارند، حتی اگر به‌عنوان شوخی گفته شوند.', kmr:'Gotinên bi îşareta neguncav di axaftina bi xerîdar re cihê wan tune ye, heta wek henek bên gotin jî.', pl:'Dwuznaczne uwagi nie mają miejsca w rozmowie z klientem, nawet jeśli są pomyślane jako żart.', ro:'Remarcile cu aluzii indecente nu au loc într-o discuție cu un client, chiar dacă sunt intenționate ca glumă.', ru:'Непристойным намёкам не место в разговоре с клиентом, даже если они задуманы как шутка.', sq:'Vërejtjet sugjestive nuk kanë vend në një bisedë me klientin, edhe nëse synohen si shaka.', tr:'İmalı ve uygunsuz sözlerin müşteri görüşmesinde yeri yoktur, şaka amaçlı olsa bile.' }),
      ex('Der Roman arbeitet mit anzüglichen Andeutungen, ohne jemals ausdrücklich vulgär zu werden.', { ar:'تستخدم الرواية تلميحات موحية من دون أن تصبح فجة صراحة في أي وقت.', ckb:'ڕۆمانەکە بە ئاماژە ئەنزۆگلیشەکان کار دەکات، بەبێ ئەوەی هیچ کات بە ڕوونی بێئەدەب ببێت.', en:'The novel uses suggestive hints without ever becoming explicitly vulgar.', fa:'رمان با اشاره‌های کنایه‌آمیز کار می‌کند، بدون آنکه هرگز آشکارا مبتذل شود.', kmr:'Roman bi îşaretên suggestîv dixebite bê ku tu carî bi eşkere xerab-ziman bibe.', pl:'Powieść operuje dwuznacznymi aluzjami, nigdy nie stając się wprost wulgarna.', ro:'Romanul folosește aluzii sugestive fără să devină vreodată explicit vulgar.', ru:'Роман использует двусмысленные намёки, никогда не становясь откровенно вульгарным.', sq:'Romani përdor aludime sugjestive pa u bërë kurrë shprehimisht vulgar.', tr:'Roman, hiçbir zaman açıkça kaba olmadan imalı göndermeler kullanır.' })
    ]
  }
];

const pkg = { packageVersion: '1.0', packageId: `de-${levelLower}-generated-batch-${batch}`, packageName: `German ${level} Generated Batch ${batch}`, source: 'Hybrid', defaultMeaningLanguages: langs, labels, entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');
let output = '';
try { output = execFileSync('dotnet', ['run', '--project', projectPath, '--', '--target', 'shared', '--yes', outPath], { cwd: root, encoding: 'utf8', stdio: ['ignore','pipe','pipe'] }); }
catch (e) { output = `${e.stdout || ''}\n${e.stderr || ''}`; console.log(output); throw new Error('Import failed'); }
console.log(output);
const ok = output.includes('Entries imported: 6') && output.includes('Entries invalid: 0') && output.includes('Warnings: 0');
if (!ok) throw new Error('Import did not meet strict success criteria');
const currentTokens = fs.readFileSync(sourcePath, 'utf8').split(',').map(t => t.trim()).filter(Boolean);
const remaining = currentTokens.slice();
for (const w of expected) { const idx = remaining.indexOf(w); if (idx === -1) throw new Error(`Processed token not found for deletion: ${w}`); remaining.splice(idx, 1); }
fs.writeFileSync(sourcePath, remaining.join(', '), 'utf8');
console.log('SOURCE_UPDATED: yes');
console.log('REMAINING_COUNT:', remaining.length);
console.log('FIRST_10:', remaining.slice(0, 10).join(' | '));
console.log('JSON_PATH:', outPath);

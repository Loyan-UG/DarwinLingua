const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '070';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['schelten','der Schematismus','schemenhaft','scheren','schillernd','schlechterdings'];

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
    word: 'schelten', partOfSpeech: 'Verb', infinitive: 'schelten', syllableBreak: 'schel-ten',
    topics: ['social-and-relationships','work-and-jobs','business-communication'], usageLabels: ['formal','written','sensitive','advanced'],
    grammarNotes: ['strong verb; often elevated or literary'],
    collocations: [{ text: 'jemanden einen Lügner schelten', meaning: 'to call someone a liar reproachfully' }],
    meanings: meaning('يوبّخ؛ يصف بسوء','سەرزەنشتکردن؛ بە ناوی خراپ ناوبردن','to scold; to denounce as','سرزنش کردن؛ به چیزی متهم و خطاب کردن','şermezar kirin; bi navê xerab bang kirin','łajać; ganić','a mustra; a certa','бранить; поносить','qortoj; fajësoj rëndë','azarlamak; kötü bir sıfatla suçlamak'),
    examples: [
      ex('Der Vorstand schelte die Kritiker nicht als Blockierer, sondern musste ihre Einwände sachlich beantworten.', trans('لم يكن ينبغي لمجلس الإدارة أن يصف المنتقدين بأنهم معرقلون، بل كان عليه أن يجيب عن اعتراضاتهم بموضوعية.','ئەنجومەنی بەڕێوەبەری نەدەبوو ڕەخنەگران وەک ڕێگر ناوببات، بەڵکو دەبوو بە بابەتی وەڵامی ناڕەزاییەکانیان بداتەوە.','The board should not denounce the critics as blockers, but had to answer their objections factually.','هیئت‌مدیره نباید منتقدان را مانع‌تراش خطاب می‌کرد، بلکه باید به ایرادهایشان به‌صورت عینی پاسخ می‌داد.','Desteya rêveberiyê nedivî rexnegiran wek astengker bi nav bike, lê diviya bersiva îtirazên wan bi rastî bide.','Zarząd nie powinien był łajać krytyków jako blokujących, lecz rzeczowo odpowiedzieć na ich zastrzeżenia.','Consiliul nu trebuia să îi denunțe pe critici ca blocanți, ci să răspundă obiectiv obiecțiilor lor.','Правление не должно было клеймить критиков как блокировщиков, а должно было предметно ответить на их возражения.','Bordi nuk duhej t’i quante kritikët bllokues, por t’u përgjigjej kundërshtimeve të tyre në mënyrë faktike.','Yönetim kurulu eleştirmenleri engelleyici diye azarlamamalı, itirazlarına nesnel biçimde yanıt vermeliydi.')),
      ex('Im Brief scheltet sie den Freund einen Feigling, obwohl der Ton eher verletzte Nähe als wirkliche Verachtung verrät.', trans('في الرسالة تصف صديقها بالجبان، مع أن النبرة تكشف قرباً مجروحاً أكثر من احتقار حقيقي.','لە نامەکەدا هاوڕێکەی بە ترسنۆک ناودەبات، هەرچەندە تۆنەکە زیاتر نزیکیی بریندارکراو دەردەخات نەک سووکایەتی ڕاستەقینە.','In the letter, she calls her friend a coward, although the tone reveals wounded closeness rather than real contempt.','در نامه، دوستش را ترسو می‌خواند، هرچند لحن بیشتر نزدیکی زخمی‌شده را نشان می‌دهد تا تحقیر واقعی.','Di nameyê de ew hevalê xwe wek tirsonek bi nav dike, herçend ton zêdetir nêzîkbûneke birîndar nîşan dide ne nefretê rastîn.','W liście nazywa przyjaciela tchórzem, choć ton zdradza raczej zranioną bliskość niż prawdziwą pogardę.','În scrisoare îl numește pe prieten laș, deși tonul trădează mai degrabă apropiere rănită decât dispreț real.','В письме она называет друга трусом, хотя тон выдает скорее раненую близость, чем настоящее презрение.','Në letër ajo e quan mikun frikacak, megjithëse toni zbulon më shumë afërsi të lënduar sesa përbuzje të vërtetë.','Mektupta arkadaşını korkak diye azarlar, ancak ton gerçek küçümsemeden çok yaralanmış yakınlığı ele verir.'))
    ]
  }),
  entry({
    word: 'der Schematismus', partOfSpeech: 'Noun', article: 'der', plural: 'Schematismen', syllableBreak: 'Sche-ma-tis-mus',
    topics: ['advanced-analysis','education-and-training','management-and-leadership'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'starrer Schematismus', meaning: 'rigid schematic thinking' }],
    meanings: meaning('نزعة تخطيطية جامدة؛ قوالبية','قالبیبوونی توند؛ بیرکردنەوەی شێوازی','schematism; rigid schematic thinking','قالب‌گرایی؛ الگوی خشک فکری','şematiyzm; fikra qalibî ya hişk','schematyzm','schematism; gândire rigidă','схематизм','skematikë e ngurtë','şematizm; kalıpçı düşünme'),
    examples: [
      ex('Der Schematismus der Bewertungsmatrix übersah, dass einige Risiken nur im Zusammenspiel mehrerer Systeme entstehen.', trans('أغفلت القالبية في مصفوفة التقييم أن بعض المخاطر لا تنشأ إلا بتفاعل عدة أنظمة.','قالبیبوونی ماتریکسی هەڵسەنگاندن ئەوەی لەبەرچاو نەگرت کە هەندێک مەترسی تەنها لە هاوکاری چەند سیستەمدا دروست دەبن.','The schematism of the evaluation matrix overlooked that some risks arise only through the interaction of several systems.','قالب‌گرایی ماتریس ارزیابی نادیده گرفت که برخی ریسک‌ها فقط در تعامل چند سیستم پدید می‌آیند.','Şematiyzma matrîsa nirxandinê nedît ku hin xeter tenê bi hevkarîya çend pergalan çêdibin.','Schematyzm macierzy oceny pominął fakt, że niektóre ryzyka powstają dopiero we współdziałaniu kilku systemów.','Schematismul matricei de evaluare a ignorat faptul că unele riscuri apar doar prin interacțiunea mai multor sisteme.','Схематизм оценочной матрицы упустил, что некоторые риски возникают только во взаимодействии нескольких систем.','Skematika e ngurtë e matricës së vlerësimit shpërfilli faktin se disa rreziqe lindin vetëm nga ndërveprimi i disa sistemeve.','Değerlendirme matrisinin şematizmi, bazı risklerin yalnızca birkaç sistemin etkileşimiyle doğduğunu gözden kaçırdı.')),
      ex('Der Kritiker wirft dem Roman keinen Mangel an Ideen vor, sondern den Schematismus seiner Figurenzeichnung.', trans('لا يتهم الناقد الرواية بنقص الأفكار، بل بقالبية رسم شخصياتها.','ڕەخنەگرەکە ڕۆمانەکە بە کەمی بیرۆکە تۆمەتبار ناکات، بەڵکو بە قالبیبوونی وێنەکردنی کارەکتەرەکانی.','The critic does not accuse the novel of lacking ideas, but of schematism in its characterization.','منتقد رمان را به کمبود ایده متهم نمی‌کند، بلکه به قالبی‌بودن شخصیت‌پردازی‌اش ایراد می‌گیرد.','Rexnegir romanê bi kêmbûna fikran tawanbar nake, lê bi şematiyzma çêkirina kesayetan rexne dike.','Krytyk nie zarzuca powieści braku pomysłów, lecz schematyzm w konstrukcji postaci.','Criticul nu îi reproșează romanului lipsa ideilor, ci schematismul construirii personajelor.','Критик упрекает роман не в недостатке идей, а в схематизме изображения персонажей.','Kritiku nuk e akuzon romanin për mungesë idesh, por për skematizmin e ndërtimit të personazheve.','Eleştirmen romana fikir eksikliği değil, karakter çizimindeki şematizmi yöneltir.'))
    ]
  }),
  entry({
    word: 'schemenhaft', partOfSpeech: 'Adjective', syllableBreak: 'sche-men-haft',
    topics: ['culture-and-media','advanced-analysis','everyday-life'], usageLabels: ['formal','written','advanced','analysis'],
    collocations: [{ text: 'schemenhaft erkennbar sein', meaning: 'to be vaguely discernible' }],
    meanings: meaning('شبحياً؛ غير واضح المعالم','وەک سێبەر؛ نادیار و ناڕوون','shadowy; indistinct','شبح‌وار؛ مبهم و نامشخص','wek siyê; nezelal','widmowy; niewyraźny','fantomatic; indistinct','призрачный; смутный','si hije; i paqartë','gölge gibi; belirsiz'),
    examples: [
      ex('Auf dem Überwachungsvideo war die Person nur schemenhaft zu erkennen, weshalb keine belastbare Identifizierung möglich war.', trans('في فيديو المراقبة لم يكن الشخص ظاهراً إلا بشكل غير واضح، لذلك لم تكن هوية موثوقة ممكنة.','لە ڤیدیۆی چاودێریدا کەسەکە تەنها بە شێوەی سێبەر دیار بوو، بۆیە ناساندنێکی پشتپێبەستوو نەگونجا.','On the surveillance video, the person was only vaguely discernible, so a reliable identification was not possible.','در ویدئوی نظارتی، فرد فقط به‌صورت مبهم دیده می‌شد؛ بنابراین شناسایی قابل اتکا ممکن نبود.','Di vîdyoya çavdêriyê de kes tenê bi awayekî nezelal xuya bû, ji ber vê nasandineke bawerbar ne gengaz bû.','Na nagraniu z monitoringu osoba była widoczna tylko niewyraźnie, dlatego wiarygodna identyfikacja nie była możliwa.','Pe înregistrarea de supraveghere, persoana se vedea doar indistinct, astfel că identificarea sigură nu era posibilă.','На записи видеонаблюдения человека было видно лишь смутно, поэтому надежная идентификация была невозможна.','Në videon e mbikëqyrjes personi dallohej vetëm në mënyrë të paqartë, ndaj identifikimi i besueshëm nuk ishte i mundur.','Güvenlik videosunda kişi yalnızca belirsiz biçimde seçiliyordu, bu yüzden güvenilir kimlik tespiti mümkün değildi.')),
      ex('Die Erinnerung an den Sommer bleibt schemenhaft, gerade weil der Erzähler jedes konkrete Datum ausspart.', trans('تبقى ذكرى الصيف شبحية تحديداً لأن الراوي يحذف كل تاريخ محدد.','بیرەوەری هاوینەکە وەک سێبەر دەمێنێتەوە، چونکە گێڕەرەوەکە هەر بەروارێکی دیاریکراو لا دەبات.','The memory of the summer remains shadowy precisely because the narrator omits every concrete date.','خاطره آن تابستان شبح‌وار می‌ماند، دقیقاً چون راوی هر تاریخ مشخصی را حذف می‌کند.','Bîranîna havînê wek siyê dimîne, ji ber ku vegêr her tarîxeke zehmet derdixe.','Wspomnienie lata pozostaje widmowe właśnie dlatego, że narrator pomija każdą konkretną datę.','Amintirea verii rămâne fantomatică tocmai pentru că naratorul omite orice dată concretă.','Воспоминание о лете остается призрачным именно потому, что рассказчик опускает каждую конкретную дату.','Kujtimi i verës mbetet si hije, pikërisht sepse rrëfimtari lë jashtë çdo datë konkrete.','Yaz anısı, anlatıcının her somut tarihi atlaması nedeniyle gölge gibi kalır.'))
    ]
  }),
  entry({
    word: 'scheren', partOfSpeech: 'Verb', infinitive: 'scheren', syllableBreak: 'sche-ren',
    topics: ['everyday-life','social-and-relationships','business-communication'], usageLabels: ['formal','written','advanced','sensitive'],
    grammarNotes: ['can mean to shear/cut or, reflexively, to care about something in negative constructions'],
    collocations: [{ text: 'sich nicht um etwas scheren', meaning: 'not to care about something' }],
    meanings: meaning('يقصّ الصوف أو الشعر؛ لا يكترث بشيء','قرتاندن؛ گرنگی نەدان بە شتێک','to shear; not to care about something','چیدن؛ اعتنا نکردن به چیزی','birîn; girîngî nedan','strzyc; nie przejmować się czymś','a tunde; a nu-i păsa de ceva','стричь; не заботиться о чем-либо','qeth; mos çaj kokën për diçka','kırpmak; umursamamak'),
    examples: [
      ex('Die Führung scherte sich lange nicht um die Warnungen des Supports, bis die Beschwerden der Großkunden öffentlich wurden.', trans('لم تكترث الإدارة طويلاً بتحذيرات الدعم إلى أن أصبحت شكاوى العملاء الكبار علنية.','سەرکردایەتی ماوەیەکی درێژ گرنگی بە ئاگادارکردنەوەکانی پشتگیری نەدا، تا سکاڵای کڕیارە گەورەکان ئاشکرا بوو.','Management did not care about the support team’s warnings for a long time until the complaints from major customers became public.','مدیریت مدت‌ها به هشدارهای پشتیبانی اعتنا نکرد، تا اینکه شکایت مشتریان بزرگ علنی شد.','Rêveberî demek dirêj girîngî neda hişyariyên piştgiriyê heta gilîyên xerîdarên mezin eşkere bûn.','Kierownictwo długo nie przejmowało się ostrzeżeniami wsparcia, aż skargi dużych klientów stały się publiczne.','Conducerea nu s-a sinchisit mult timp de avertismentele suportului, până când reclamațiile clienților mari au devenit publice.','Руководство долго не обращало внимания на предупреждения поддержки, пока жалобы крупных клиентов не стали публичными.','Drejtimi për një kohë të gjatë nuk u kujdes për paralajmërimet e suportit, derisa ankesat e klientëve të mëdhenj u bënë publike.','Yönetim, büyük müşterilerin şikayetleri kamuya açılana kadar destek ekibinin uyarılarını uzun süre umursamadı.')),
      ex('Im Frühjahr werden die Schafe geschoren, damit sie die Sommerhitze besser ertragen.', trans('في الربيع تُجزّ الأغنام لكي تتحمل حرارة الصيف بشكل أفضل.','لە بەهاردا مەڕەکان دەقرتێنرێن بۆ ئەوەی گەرمای هاوین باشتر بگرن.','In spring, the sheep are shorn so that they can better tolerate the summer heat.','در بهار گوسفندها پشم‌چینی می‌شوند تا گرمای تابستان را بهتر تحمل کنند.','Di biharê de pez tên birîn da ku germiya havînê baştir tehemul bikin.','Wiosną strzyże się owce, aby lepiej znosiły letnie upały.','Primăvara, oile sunt tunse ca să suporte mai bine căldura verii.','Весной овец стригут, чтобы они легче переносили летнюю жару.','Në pranverë delet qethen që ta përballojnë më mirë vapën e verës.','İlkbaharda koyunlar yaz sıcağına daha iyi dayansın diye kırkılır.'))
    ]
  }),
  entry({
    word: 'schillernd', partOfSpeech: 'Adjective', syllableBreak: 'schil-lernd',
    topics: ['culture-and-media','business-communication','advanced-analysis'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'eine schillernde Persönlichkeit', meaning: 'a dazzling, multifaceted personality' }],
    meanings: meaning('متلألئ؛ متعدد الأوجه','درەوشاوە؛ فرەڕەهەند','iridescent; dazzling; multifaceted','رنگارنگ و درخشان؛ چندوجهی','rengîn û biriqok; piralî','mieniący się; wieloznaczny','sclipitor; cu mai multe fațete','переливчатый; многогранный','i shkëlqyeshëm; shumëfaqësh','parıltılı; çok yönlü'),
    examples: [
      ex('Der neue CEO gilt als schillernde Figur, weil er technische Visionen mit aggressiver Finanzpolitik verbindet.', trans('يُعد الرئيس التنفيذي الجديد شخصية متعددة الأوجه لأنه يجمع الرؤى التقنية بسياسة مالية هجومية.','CEOی نوێ وەک کەسایەتییەکی درەوشاوە دادەنرێت، چونکە بینینی تەکنیکی بە سیاسەتی دارایی هێرشکارانە دەبەستێتەوە.','The new CEO is considered a dazzling figure because he combines technical visions with aggressive financial policy.','مدیرعامل جدید شخصیتی چندوجهی به شمار می‌رود، چون چشم‌اندازهای فنی را با سیاست مالی تهاجمی پیوند می‌دهد.','CEOyê nû wek kesayetiyeke piralî tê dîtin, ji ber ku dîtinên teknîkî bi siyaseta darayî ya êrîşkar ve girê dide.','Nowy CEO uchodzi za postać barwną, ponieważ łączy wizje techniczne z agresywną polityką finansową.','Noul CEO este considerat o figură sclipitoare, deoarece combină viziuni tehnice cu o politică financiară agresivă.','Нового CEO считают яркой фигурой, потому что он сочетает технические видения с агрессивной финансовой политикой.','CEO i ri konsiderohet figurë shumëfaqëshe, sepse lidh vizionet teknike me politikë financiare agresive.','Yeni CEO, teknik vizyonları agresif finans politikasıyla birleştirdiği için çok yönlü ve parlak bir figür sayılıyor.')),
      ex('Die schillernden Farben der Fassade stehen im Kontrast zur nüchternen Strenge des Innenraums.', trans('تتباين الألوان المتلألئة للواجهة مع الصرامة الهادئة للداخل.','ڕەنگە درەوشاوەکانی ڕووی بیناکە لەگەڵ توندیی ساردی ناوەوەدا دژایەتی دەکەن.','The iridescent colors of the facade contrast with the sober austerity of the interior.','رنگ‌های درخشان نما با سخت‌گیری خونسرد فضای داخلی در تضادند.','Rengên biriqok ên rûyê avahiyê bi hişkatiya sade ya hundir re dijber in.','Mieniące się kolory fasady kontrastują z trzeźwą surowością wnętrza.','Culorile sclipitoare ale fațadei contrastează cu austeritatea sobră a interiorului.','Переливчатые цвета фасада контрастируют со сдержанной строгостью интерьера.','Ngjyrat e shkëlqyeshme të fasadës bien në kontrast me ashpërsinë e kthjellët të brendësisë.','Cephenin parıltılı renkleri iç mekanın sade sertliğiyle karşıtlık oluşturur.'))
    ]
  }),
  entry({
    word: 'schlechterdings', partOfSpeech: 'Adverb', syllableBreak: 'schlech-ter-dings',
    topics: ['advanced-analysis','business-communication','law-and-compliance'], usageLabels: ['formal','written','academic','advanced'],
    collocations: [{ text: 'schlechterdings unmöglich', meaning: 'simply or absolutely impossible' }],
    meanings: meaning('ببساطة؛ قطعاً؛ على الإطلاق','بە تەواوی؛ هەرگیز؛ بە سادەیی','simply; absolutely; utterly','اصلاً؛ به‌هیچ‌وجه؛ مطلقاً','bi tevahî; qet; sade','po prostu; absolutnie','pur și simplu; absolut','попросту; совершенно','thjesht; absolutisht','basbayağı; kesinlikle; düpedüz'),
    examples: [
      ex('Unter diesen Datenschutzauflagen ist eine Speicherung der Rohdaten außerhalb der EU schlechterdings unmöglich.', trans('في ظل شروط حماية البيانات هذه، فإن تخزين البيانات الخام خارج الاتحاد الأوروبي غير ممكن إطلاقاً.','لەژێر ئەم مەرجانەی پاراستنی داتادا، هەڵگرتنی داتای خاوی لە دەرەوەی یەکێتی ئەوروپا بە تەواوی نامومکینە.','Under these data protection requirements, storing the raw data outside the EU is simply impossible.','با این الزامات حفاظت از داده، ذخیره‌سازی داده خام خارج از اتحادیه اروپا مطلقاً ناممکن است.','Di bin van mercên parastina daneyan de, parastina daneyên xav li derveyî YE bi tevahî ne gengaz e.','Przy tych wymogach ochrony danych przechowywanie surowych danych poza UE jest po prostu niemożliwe.','În aceste condiții de protecție a datelor, stocarea datelor brute în afara UE este pur și simplu imposibilă.','При таких требованиях защиты данных хранение сырых данных за пределами ЕС попросту невозможно.','Nën këto kërkesa për mbrojtjen e të dhënave, ruajtja e të dhënave të papërpunuara jashtë BE-së është thjesht e pamundur.','Bu veri koruma şartları altında ham verilerin AB dışında saklanması kesinlikle imkansızdır.')),
      ex('Die Behauptung, der Text habe keine politische Dimension, ist schlechterdings nicht haltbar.', trans('إن الادعاء بأن النص لا يملك بعداً سياسياً لا يمكن الدفاع عنه إطلاقاً.','ئەو بانگەشەیەی کە دەقەکە هیچ ڕەهەندێکی سیاسی نییە، بە هیچ شێوەیەک پشتپێبەستوو نییە.','The claim that the text has no political dimension is simply untenable.','این ادعا که متن هیچ بعد سیاسی ندارد، مطلقاً قابل دفاع نیست.','Doza ku nivîs ti aliyekî siyasî tune ye, bi tevahî nayê ragirtin.','Twierdzenie, że tekst nie ma wymiaru politycznego, jest po prostu nie do utrzymania.','Afirmația că textul nu are nicio dimensiune politică este pur și simplu nesustenabilă.','Утверждение, что текст не имеет политического измерения, попросту несостоятельно.','Pretendimi se teksti nuk ka dimension politik është thjesht i pambrojtshëm.','Metnin hiçbir siyasi boyutu olmadığı iddiası kesinlikle savunulamaz.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 070', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '071';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['schlechthin','schmähen','schmälern','die Schroffheit','schwadronieren','der Seiltanz'];

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
    word: 'schlechthin', partOfSpeech: 'Adverb', syllableBreak: 'schlecht-hin',
    topics: ['advanced-analysis','business-communication','education-and-training'], usageLabels: ['formal','written','academic','advanced'],
    collocations: [{ text: 'schlechthin unmöglich', meaning: 'simply impossible; absolutely impossible' }],
    meanings: meaning('ببساطة؛ على الإطلاق؛ بالمطلق','بە تەواوی؛ بە شێوەیەکی ڕەها','simply; absolutely; as such','به‌طور مطلق؛ اساساً','bi tevahî; wek xwe','po prostu; absolutnie','pur și simplu; absolut','просто; абсолютно','thjesht; absolutisht','basitçe; mutlak anlamda'),
    examples: [
      ex('Ohne belastbare Testdaten ist die versprochene Prognosegenauigkeit schlechthin nicht nachweisbar.', trans('من دون بيانات اختبار موثوقة لا يمكن إطلاقاً إثبات دقة التنبؤ الموعودة.','بەبێ داتای تاقیکردنەوەی پشتپێبەستوو، وردیی پێشبینی بەڵێندراو بە تەواوی ناتوانرێت بسەلمێنرێت.','Without reliable test data, the promised forecast accuracy is simply not verifiable.','بدون داده آزمون قابل اتکا، دقت پیش‌بینی وعده‌داده‌شده اساساً قابل اثبات نیست.','Bê daneyên testê yên bawerbar, rastiya pêşbîniya sozdayî bi tevahî nayê îsbatkirin.','Bez wiarygodnych danych testowych obiecanej dokładności prognozy po prostu nie da się udowodnić.','Fără date de testare solide, acuratețea promisă a prognozei pur și simplu nu poate fi demonstrată.','Без надежных тестовых данных обещанную точность прогноза просто невозможно доказать.','Pa të dhëna testimi të besueshme, saktësia e premtuar e parashikimit thjesht nuk mund të provohet.','Güvenilir test verileri olmadan vaat edilen tahmin doğruluğu kesinlikle kanıtlanamaz.')),
      ex('Für die Erzählerin ist die Rückkehr in das Dorf nicht nur schwierig, sondern schlechthin undenkbar.', trans('بالنسبة إلى الراوية ليست العودة إلى القرية صعبة فقط، بل غير قابلة للتصور إطلاقاً.','بۆ گێڕەرەوەکە گەڕانەوە بۆ گوندەکە نەک تەنها قورسە، بەڵکو بە تەواوی بیرلێنەکراوە.','For the narrator, returning to the village is not merely difficult but absolutely unthinkable.','برای راوی، بازگشت به روستا نه فقط دشوار، بلکه اساساً تصورناپذیر است.','Ji bo vegêrê vegera gund ne tenê dijwar e, lê bi tevahî nayê fikirîn.','Dla narratorki powrót do wsi jest nie tylko trudny, lecz absolutnie nie do pomyślenia.','Pentru naratoare, întoarcerea în sat nu este doar dificilă, ci absolut de neconceput.','Для рассказчицы возвращение в деревню не просто трудно, а абсолютно немыслимо.','Për rrëfimtaren, kthimi në fshat nuk është vetëm i vështirë, por absolutisht i paimagjinueshëm.','Anlatıcı için köye dönüş yalnızca zor değil, mutlak anlamda düşünülemezdir.'))
    ]
  }),
  entry({
    word: 'schmähen', partOfSpeech: 'Verb', infinitive: 'schmähen', syllableBreak: 'schmä-hen',
    topics: ['social-and-relationships','business-communication','culture-and-media'], usageLabels: ['formal','written','sensitive','advanced'],
    collocations: [{ text: 'jemanden öffentlich schmähen', meaning: 'to publicly vilify someone' }],
    meanings: meaning('يشتم؛ يذم بقسوة','سووکایەتی پێکردن؛ بە توندی خراپ ناوبردن','to vilify; to revile','دشنام دادن؛ بدنام و تحقیر کردن','şermezar kirin; bi tundî xerab gotin','znieważać; lżyć','a defăima; a insulta','поносить; оскорблять','shaj; përçmoj publikisht','kötülemek; hakaret etmek'),
    examples: [
      ex('Wer interne Kritiker öffentlich schmäht, zerstört oft schneller Vertrauen als jede sachliche Gegenrede.', trans('من يذم المنتقدين الداخليين علناً يدمّر الثقة غالباً أسرع من أي رد موضوعي.','ئەوەی ڕەخنەگرانی ناوخۆ بە ئاشکرا سووکایەتی پێدەکات، زۆرجار خێراتر لە هەر وەڵامێکی بابەتی متمانە تێکدەدات.','Anyone who publicly vilifies internal critics often destroys trust faster than any factual rebuttal would.','کسی که منتقدان داخلی را علناً تحقیر می‌کند، اغلب اعتماد را سریع‌تر از هر پاسخ منطقی از بین می‌برد.','Kesê ku rexnegirên hundirîn bi eşkere şermezar dike, gelek caran baweriyê zûtir ji her bersiveke rastîn têk dide.','Kto publicznie znieważa wewnętrznych krytyków, często niszczy zaufanie szybciej niż jakakolwiek rzeczowa polemika.','Cine îi defăimează public pe criticii interni distruge adesea încrederea mai repede decât orice replică obiectivă.','Тот, кто публично поносит внутренних критиков, часто разрушает доверие быстрее, чем любая предметная контраргументация.','Kush i shan publikisht kritikët e brendshëm, shpesh e shkatërron besimin më shpejt se çdo kundërargument faktik.','İç eleştirmenleri açıkça kötüleyen biri, güveni çoğu zaman her türlü nesnel karşı yanıttan daha hızlı yok eder.')),
      ex('Die Pamphlete schmähten den Dichter als Verräter, ohne sich mit seinen Argumenten auseinanderzusetzen.', trans('هاجمت المنشورات الشاعر بوصفه خائناً من دون أن تتعامل مع حججه.','پامفلێتەکان شاعیرەکەیان وەک خیانەتکار خراپ ناوبرد، بەبێ ئەوەی لەگەڵ ئارگومێنتەکانی ڕووبەڕوو ببنەوە.','The pamphlets vilified the poet as a traitor without engaging with his arguments.','جزوه‌ها شاعر را خائن می‌خواندند، بدون آن‌که با استدلال‌هایش درگیر شوند.','Pamfletan helbestvan wek xayîn şermezar kirin bê ku bi argumanên wî re bikevin nîqaşê.','Pamflety lżyły poetę jako zdrajcę, nie odnosząc się do jego argumentów.','Pamfletele îl defăimau pe poet ca trădător fără să îi abordeze argumentele.','Памфлеты поносили поэта как предателя, не разбирая его аргументы.','Pamfletet e shanin poetin si tradhtar pa u marrë me argumentet e tij.','Pamfletler şairi argümanlarıyla yüzleşmeden hain diye kötüledi.'))
    ]
  }),
  entry({
    word: 'schmälern', partOfSpeech: 'Verb', infinitive: 'schmälern', syllableBreak: 'schmä-lern',
    topics: ['finance-and-accounting','business-communication','advanced-analysis'], usageLabels: ['formal','written','business','analysis'],
    collocations: [{ text: 'den Gewinn schmälern', meaning: 'to reduce profits' }],
    meanings: meaning('يقلل؛ ينقص من القيمة أو الأثر','کەمکردنەوە؛ بەهای شتێک کەمکردن','to diminish; to reduce','کاستن؛ کم‌اثر یا کم‌ارزش کردن','kêm kirin; nirxê tiştekî daxistin','pomniejszać; obniżać','a diminua; a reduce','уменьшать; снижать','zvogëloj; pakësoj','azaltmak; küçültmek'),
    examples: [
      ex('Die unerwarteten Lizenzkosten schmälerten den Projekterfolg, obwohl die technische Umsetzung überzeugend war.', trans('قللت تكاليف الترخيص غير المتوقعة من نجاح المشروع، رغم أن التنفيذ التقني كان مقنعاً.','تێچووی مۆڵەتی چاوەڕواننەکراو سەرکەوتنی پڕۆژەکەی کەمکردەوە، هەرچەندە جێبەجێکردنی تەکنیکی قایلکەر بوو.','The unexpected licensing costs diminished the project’s success, even though the technical implementation was convincing.','هزینه‌های غیرمنتظره مجوز از موفقیت پروژه کاست، هرچند اجرای فنی قانع‌کننده بود.','Mesrefên lîsansê yên neçaverêkirî serkeftina projeyê kêm kirin, herçend cîbicîkirina teknîkî qanihker bû.','Nieoczekiwane koszty licencji pomniejszyły sukces projektu, choć techniczna realizacja była przekonująca.','Costurile neașteptate de licențiere au diminuat succesul proiectului, deși implementarea tehnică era convingătoare.','Неожиданные лицензионные расходы снизили успех проекта, хотя техническая реализация была убедительной.','Kostot e papritura të licencave e pakësuan suksesin e projektit, megjithëse zbatimi teknik ishte bindës.','Beklenmedik lisans maliyetleri, teknik uygulama ikna edici olsa da projenin başarısını azalttı.')),
      ex('Die späte Einsicht schmälert nicht den Mut der Figur, macht ihn aber moralisch ambivalenter.', trans('لا تقلل البصيرة المتأخرة من شجاعة الشخصية، لكنها تجعلها أكثر التباساً أخلاقياً.','تێگەیشتنی درەنگ ئازایەتی کارەکتەرەکە کەمناکاتەوە، بەڵام لە ڕووی ئەخلاقییەوە ئاڵۆزتری دەکات.','The late insight does not diminish the character’s courage, but it makes it morally more ambiguous.','درک دیرهنگام از شجاعت شخصیت نمی‌کاهد، اما آن را از نظر اخلاقی مبهم‌تر می‌کند.','Têgihiştina dereng wêrekiya kesayetê kêm nake, lê wê ji aliyê exlaqî ve ambîvalanttir dike.','Późne zrozumienie nie pomniejsza odwagi postaci, lecz czyni ją moralnie bardziej ambiwalentną.','Înțelegerea târzie nu diminuează curajul personajului, dar îl face moral mai ambivalent.','Позднее прозрение не умаляет мужества персонажа, но делает его морально более неоднозначным.','Kuptimi i vonë nuk e zvogëlon guximin e personazhit, por e bën moralisht më ambivalent.','Geç gelen kavrayış karakterin cesaretini azaltmaz, ama onu ahlaki olarak daha ikircikli kılar.'))
    ]
  }),
  entry({
    word: 'die Schroffheit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Schroff-heit',
    topics: ['business-communication','social-and-relationships','culture-and-media'], usageLabels: ['formal','written','sensitive','advanced'],
    grammarNotes: ['feminine abstract noun; normally used in singular'],
    collocations: [{ text: 'die Schroffheit eines Tons', meaning: 'the harshness of a tone' }],
    meanings: meaning('حدة؛ خشونة في الأسلوب','توندی؛ زبریی ڕەفتار یان تۆن','harshness; abruptness','تندی؛ خشونت لحن یا رفتار','tundî; hişkbûn','szorstkość; opryskliwość','asprime; bruschețe','резкость; грубоватость','ashpërsi; vrazhdësi','sertlik; kabalık'),
    examples: [
      ex('Die Schroffheit der Antwort ließ die Kundin zweifeln, ob ihre Beschwerde überhaupt ernst genommen wurde.', trans('جعلت حدة الرد العميلة تشك فيما إذا كانت شكواها تؤخذ أصلاً على محمل الجد.','توندیی وەڵامەکە وای کرد کڕیارەکە گومان بکات کە ئایا سکاڵاکەی بە جدی وەرگیراوە یان نا.','The harshness of the response made the customer doubt whether her complaint was being taken seriously at all.','تندی پاسخ باعث شد مشتری شک کند که آیا اصلاً شکایتش جدی گرفته شده است یا نه.','Tundiya bersivê kir ku xerîdar guman bike gelo gilîya wê bi rastî cidî tê girtin an na.','Szorstkość odpowiedzi sprawiła, że klientka zaczęła wątpić, czy jej skargę w ogóle potraktowano poważnie.','Asprimea răspunsului a făcut-o pe clientă să se îndoiască dacă reclamația ei era luată în serios.','Резкость ответа заставила клиентку усомниться, воспринимают ли ее жалобу вообще всерьез.','Ashpërsia e përgjigjes e bëri klienten të dyshojë nëse ankesa e saj po merrej fare seriozisht.','Yanıtın sertliği, müşterinin şikayetinin gerçekten ciddiye alınıp alınmadığından kuşku duymasına yol açtı.')),
      ex('Die Schroffheit der Landschaft spiegelt im Gedicht die innere Verhärtung des Sprechers.', trans('تعكس خشونة المشهد الطبيعي في القصيدة تصلب المتكلم الداخلي.','زبریی سروشتەکە لە شیعرەکەدا ڕەقبوونی ناوخۆی قسەکەر ڕەنگدەداتەوە.','The harshness of the landscape mirrors the speaker’s inner hardening in the poem.','خشونت منظره در شعر، سخت‌شدن درونی گوینده را بازتاب می‌دهد.','Hişkbûna erdê di helbestê de hişkbûna hundirîn a axaftvan vedide.','Surowość krajobrazu odzwierciedla w wierszu wewnętrzne stwardnienie mówiącego.','Asprimea peisajului reflectă în poezie împietrirea interioară a vorbitorului.','Суровость пейзажа в стихотворении отражает внутреннее ожесточение говорящего.','Ashpërsia e peizazhit në poezi pasqyron ngurtësimin e brendshëm të folësit.','Manzaranın sertliği şiirde konuşanın içsel katılaşmasını yansıtır.'))
    ]
  }),
  entry({
    word: 'schwadronieren', partOfSpeech: 'Verb', infinitive: 'schwadronieren', syllableBreak: 'schwa-dro-nie-ren',
    topics: ['business-communication','social-and-relationships','culture-and-media'], usageLabels: ['informal','written','sensitive','advanced'],
    collocations: [{ text: 'über Visionen schwadronieren', meaning: 'to ramble grandly about visions' }],
    meanings: meaning('يثرثر بإسهاب؛ يتكلم بطنطنة','قسەی درێژ و بێناوەڕۆک کردن','to ramble; to spout pompously','پرگویی کردن؛ با آب‌وتاب بی‌محتوا حرف زدن','dirêj û vala axaftin','perorować; ględzić','a perora; a bate câmpii pompos','разглагольствовать; трепаться высокопарно','llomotit; flas me pompozitet bosh','uzun uzun boş konuşmak; atıp tutmak'),
    examples: [
      ex('Während der Kunde konkrete Termine verlangte, schwadronierte der Anbieter weiter über seine digitale Vision.', trans('بينما كان العميل يطلب مواعيد محددة، واصل المورّد الثرثرة عن رؤيته الرقمية.','کاتێک کڕیار داوای بەرواری دیاریکراوی دەکرد، دابینکەرەکە بەردەوام بوو لە قسەی درێژی دەربارەی بینینی دیجیتاڵی خۆی.','While the customer demanded concrete dates, the provider kept rambling about its digital vision.','در حالی که مشتری تاریخ‌های مشخص می‌خواست، ارائه‌دهنده همچنان درباره چشم‌انداز دیجیتال خود پرگویی می‌کرد.','Dema xerîdar tarîxên zehmet dixwest, dabînker hê jî li ser dîtina xwe ya dîjîtal dirêj axivî.','Podczas gdy klient żądał konkretnych terminów, dostawca nadal perorował o swojej cyfrowej wizji.','În timp ce clientul cerea termene concrete, furnizorul continua să peroreze despre viziunea sa digitală.','Пока клиент требовал конкретных сроков, поставщик продолжал разглагольствовать о своем цифровом видении.','Ndërsa klienti kërkonte afate konkrete, ofruesi vazhdonte të llomotiste për vizionin e tij digjital.','Müşteri somut tarihler isterken sağlayıcı dijital vizyonu üzerine boş boş konuşmaya devam etti.')),
      ex('Der Erzähler schwadroniert über Größe und Schicksal, doch gerade darin zeigt sich seine Angst vor Bedeutungslosigkeit.', trans('يثرثر الراوي عن العظمة والمصير، لكن خوفه من انعدام الأهمية يظهر تحديداً في ذلك.','گێڕەرەوەکە دەربارەی گەورەیی و چارەنووس قسەی درێژ دەکات، بەڵام هەر لەوەدا ترسی لە بێمانایی دەردەکەوێت.','The narrator rambles about greatness and destiny, yet this is precisely where his fear of insignificance becomes visible.','راوی درباره بزرگی و سرنوشت پرگویی می‌کند، اما دقیقاً در همین جا ترس او از بی‌اهمیت‌بودن آشکار می‌شود.','Vegêr li ser mezinahî û qedera xwe dirêj diaxive, lê rast li vir tirsa wî ji bêwatebûnê xuya dibe.','Narrator peroruje o wielkości i przeznaczeniu, lecz właśnie w tym ujawnia się jego lęk przed nieistotnością.','Naratorul perorează despre măreție și destin, dar tocmai aici se vede teama lui de insignifianță.','Рассказчик разглагольствует о величии и судьбе, но именно в этом проявляется его страх перед незначительностью.','Rrëfimtari llomotit për madhështinë dhe fatin, por pikërisht aty shfaqet frika e tij nga parëndësia.','Anlatıcı büyüklük ve kader üzerine atıp tutar, ama tam da bunda önemsizlik korkusu görünür olur.'))
    ]
  }),
  entry({
    word: 'der Seiltanz', partOfSpeech: 'Noun', article: 'der', plural: 'Seiltänze', syllableBreak: 'Seil-tanz',
    topics: ['management-and-leadership','business-communication','culture-and-media'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'ein politischer Seiltanz', meaning: 'a political tightrope act' }],
    meanings: meaning('مشي على الحبل؛ توازن دقيق','سەیرکردن لەسەر حەبل؛ هاوسەنگیی زۆر ورد','tightrope act; delicate balancing act','بندبازی؛ موازنه بسیار حساس','li ser benê meşîn; hevsengiyeke nazik','taniec na linie; balansowanie','mers pe sârmă; echilibru delicat','ходьба по канату; тонкий баланс','ecje mbi litar; ekuilibër i brishtë','ip cambazlığı; hassas denge'),
    examples: [
      ex('Die Migration im laufenden Betrieb war ein Seiltanz zwischen Stabilität, Kundenerwartungen und begrenzten Entwicklerkapazitäten.', trans('كانت عملية الترحيل أثناء التشغيل مشياً على الحبل بين الاستقرار وتوقعات العملاء وقدرات المطورين المحدودة.','کۆچکردن لە کاتی کارکردندا سەیرکردنێک بوو لەسەر حەبل لە نێوان جێگیری، چاوەڕوانیی کڕیاران و توانای سنوورداری پەرەپێدەراندا.','The migration during live operations was a tightrope act between stability, customer expectations, and limited developer capacity.','مهاجرت در حین بهره‌برداری بندبازی‌ای میان پایداری، انتظارهای مشتری و ظرفیت محدود توسعه‌دهندگان بود.','Koçberî di dema xebata zindî de seiltanzek bû di navbera aramî, hêviyên xerîdaran û kapasîteya sînorkirî ya pêşvebirên de.','Migracja podczas bieżącej pracy była balansowaniem między stabilnością, oczekiwaniami klientów i ograniczoną dostępnością deweloperów.','Migrarea în timpul funcționării a fost un mers pe sârmă între stabilitate, așteptările clienților și capacitatea limitată a dezvoltatorilor.','Миграция во время работы была хождением по канату между стабильностью, ожиданиями клиентов и ограниченными ресурсами разработчиков.','Migrimi gjatë operimit aktiv ishte ecje mbi litar mes stabilitetit, pritshmërive të klientëve dhe kapacitetit të kufizuar të zhvilluesve.','Canlı işletim sırasında geçiş, istikrar, müşteri beklentileri ve sınırlı geliştirici kapasitesi arasında bir ip cambazlığıydı.')),
      ex('Die Regisseurin inszeniert den Schluss als Seiltanz zwischen Trost und bitterer Ironie.', trans('تُخرج المخرجة النهاية كموازنة دقيقة بين العزاء والسخرية المريرة.','دەرهێنەرەکە کۆتایی وەک سەیرکردنێک لەسەر حەبل لە نێوان دڵنەوایی و ئیرۆنیای تاڵ نمایش دەکات.','The director stages the ending as a tightrope act between consolation and bitter irony.','کارگردان پایان را به‌صورت بندبازی میان تسلی و طنز تلخ اجرا می‌کند.','Derhêner dawiyê wek seiltanzek di navbera dilxweşî û ironîya tal de dide nîşan.','Reżyserka inscenizuje zakończenie jako balansowanie między pocieszeniem a gorzką ironią.','Regizoarea pune finalul în scenă ca pe un echilibru delicat între consolare și ironie amară.','Режиссер ставит финал как тонкий баланс между утешением и горькой иронией.','Regjisorja e vë në skenë fundin si ecje mbi litar mes ngushëllimit dhe ironisë së hidhur.','Yönetmen finali teselli ile acı ironi arasında bir ip cambazlığı olarak sahneler.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 071', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

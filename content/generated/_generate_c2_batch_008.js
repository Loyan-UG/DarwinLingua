const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '008';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const projectPath = path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj');
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['der Argwohn','artifiziell','asketisch','auf Anhieb','aufbegehren','aufkündigen'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const labelMap = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const tokens = fs.readFileSync(sourcePath, 'utf8').split(',').map(t => t.trim()).filter(Boolean);
const words = tokens.slice(0, 6);
if (JSON.stringify(words) !== JSON.stringify(expected)) throw new Error(`Unexpected first tokens: ${JSON.stringify(words)}`);
const usedLabels = ['formal','written','advanced','business','workplace','academic','analysis','sensitive'];
for (const key of usedLabels) if (!labelMap.has(key)) throw new Error(`Missing taxonomy label: ${key}`);
const labels = usedLabels.map(key => labelMap.get(key));
function meanings(obj) { return langs.map(language => ({ language, text: obj[language] })); }
function ex(baseText, obj) { return { baseText, translations: meanings(obj) }; }

const entries = [
  {
    word: 'der Argwohn', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'der', plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'Arg-wohn',
    topics: ['social-and-relationships','business-communication','advanced-analysis'], usageLabels: ['formal','written','advanced','sensitive'], contextLabels: [],
    grammarNotes: ['masculine noun; usually singular; means suspicion or distrust'],
    collocations: [{ text: 'Argwohn wecken', meaning: 'to arouse suspicion' }],
    wordFamilies: [{ lemma: 'argwöhnisch', relationLabel: 'adjective', note: null }], relations: [],
    meanings: meanings({ ar:'ارتياب؛ سوء ظن', ckb:'گومان؛ بێمتمانەیی', en:'suspicion; distrust', fa:'بدگمانی؛ سوءظن', kmr:'guman; bêbawerî', pl:'podejrzliwość; nieufność', ro:'suspiciune; neîncredere', ru:'подозрение; недоверие', sq:'dyshim; mosbesim', tr:'kuşku; güvensizlik' }),
    examples: [
      ex('Die ungewöhnlich späte Änderung im Vertrag weckte bei der Einkaufsabteilung sofort Argwohn.', { ar:'أثار التغيير المتأخر وغير المعتاد في العقد ارتياب قسم المشتريات فوراً.', ckb:'گۆڕانکارییە نائاسایی و درەنگەکە لە گرێبەستەکەدا دەستبەجێ گومانی لای بەشی کڕین دروست کرد.', en:'The unusually late change in the contract immediately aroused suspicion in the purchasing department.', fa:'تغییر غیرمعمول و دیرهنگام در قرارداد فوراً بدگمانی بخش خرید را برانگیخت.', kmr:'Guhertina neasayî ya dereng di peymanê de yekser guman li beşa kirînê çêkir.', pl:'Nietypowo późna zmiana w umowie natychmiast wzbudziła podejrzenia działu zakupów.', ro:'Modificarea neobișnuit de târzie din contract a trezit imediat suspiciuni în departamentul de achiziții.', ru:'Необычно позднее изменение в договоре сразу вызвало подозрение у отдела закупок.', sq:'Ndryshimi jashtëzakonisht i vonë në kontratë ngjalli menjëherë dyshim te departamenti i blerjeve.', tr:'Sözleşmedeki olağandışı geç değişiklik satın alma departmanında hemen kuşku uyandırdı.' }),
      ex('Ihr Argwohn war nicht unbegründet, doch er erschwerte jedes offene Gespräch.', { ar:'لم يكن ارتيابها بلا أساس، لكنه جعل كل حوار صريح أكثر صعوبة.', ckb:'گومانەکەی بێبنەما نەبوو، بەڵام هەر گفتوگۆیەکی کراوەی قورس کرد.', en:'Her suspicion was not unfounded, but it made every open conversation more difficult.', fa:'بدگمانی او بی‌اساس نبود، اما هر گفت‌وگوی صریحی را دشوارتر می‌کرد.', kmr:'Gumana wê bêbingeh nebû, lê her axaftineke vekirî dijwartir dikir.', pl:'Jej podejrzliwość nie była bezpodstawna, lecz utrudniała każdą otwartą rozmowę.', ro:'Suspiciunea ei nu era nefondată, dar îngreuna orice conversație deschisă.', ru:'Её подозрение не было беспочвенным, но оно затрудняло любой открытый разговор.', sq:'Dyshimi i saj nuk ishte i pabazë, por e vështirësonte çdo bisedë të hapur.', tr:'Kuşkusu temelsiz değildi, ama her açık konuşmayı zorlaştırıyordu.' })
    ]
  },
  {
    word: 'artifiziell', language: 'de', cefrLevel: level, partOfSpeech: 'Adjective', article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'ar-ti-fi-zi-ell',
    topics: ['culture-and-media','advanced-analysis','technology-and-it'], usageLabels: ['formal','written','academic','analysis'], contextLabels: [],
    grammarNotes: ['adjective; means artificial, deliberately constructed, or not naturally developed'],
    collocations: [{ text: 'artifiziell wirken', meaning: 'to appear artificial' }],
    wordFamilies: [{ lemma: 'das Artefakt', relationLabel: 'noun', note: 'related in meaning of artificial product' }], relations: [],
    meanings: meanings({ ar:'مصطنع؛ غير طبيعي؛ مصنوع بعناية', ckb:'دەستکرد؛ نەسروشتی؛ بە ئاگاداری دروستکراو', en:'artificial; contrived; deliberately constructed', fa:'مصنوعی؛ ساختگی؛ آگاهانه ساخته‌شده', kmr:'çêkirî; ne xwezayî; bi zanebûn avakirî', pl:'sztuczny; wykoncypowany', ro:'artificial; construit; nenatural', ru:'искусственный; нарочитый; сконструированный', sq:'artificial; i sajuar; i ndërtuar qëllimisht', tr:'yapay; kurgulanmış; doğal olmayan' }),
    examples: [
      ex('Die Dialoge der Demo wirkten artifiziell, weil echte Kundeneinwände nicht berücksichtigt wurden.', { ar:'بدت حوارات العرض التوضيحي مصطنعة لأن اعتراضات العملاء الحقيقية لم تؤخذ في الحسبان.', ckb:'دیالۆگەکانی دیمۆکە دەستکرد دیار بوون، چونکە ناڕەزاییە ڕاستەقینەکانی کڕیاران لەبەرچاو نەگیرابوون.', en:'The demo dialogues felt artificial because real customer objections had not been considered.', fa:'گفت‌وگوهای دمو مصنوعی به نظر می‌رسیدند، چون اعتراض‌های واقعی مشتریان در نظر گرفته نشده بود.', kmr:'Diyalogên demoyê çêkirî xuya dikirin, ji ber ku nerazîbûnên rastîn ên xerîdaran nehatibûn hesibandin.', pl:'Dialogi w wersji demonstracyjnej brzmiały sztucznie, ponieważ nie uwzględniono rzeczywistych zastrzeżeń klientów.', ro:'Dialogurile din demo păreau artificiale, deoarece obiecțiile reale ale clienților nu fuseseră luate în considerare.', ru:'Диалоги в демо выглядели искусственными, потому что реальные возражения клиентов не были учтены.', sq:'Dialogët e demos dukeshin artificialë sepse kundërshtimet reale të klientëve nuk ishin marrë parasysh.', tr:'Demodaki diyaloglar yapay görünüyordu çünkü gerçek müşteri itirazları dikkate alınmamıştı.' }),
      ex('Der Film setzt bewusst auf eine artifizielle Farbgebung, um jede Illusion von Natürlichkeit zu vermeiden.', { ar:'يعتمد الفيلم عمداً على ألوان مصطنعة لتجنب أي وهم بالطبيعية.', ckb:'فیلمەکە بە ئاگاداری ڕەنگدانەوەیەکی دەستکرد بەکاردەهێنێت بۆ ئەوەی هەر خەیاڵێکی سروشتیبوون نەهێڵێت.', en:'The film deliberately uses an artificial color scheme to avoid any illusion of naturalness.', fa:'فیلم آگاهانه از رنگ‌پردازی مصنوعی استفاده می‌کند تا هر توهم طبیعی بودن را از بین ببرد.', kmr:'Fîlm bi zanebûn rengdaneke çêkirî bi kar tîne da ku her xeyala xwezayîbûnê dûr bike.', pl:'Film świadomie korzysta ze sztucznej kolorystyki, aby uniknąć iluzji naturalności.', ro:'Filmul folosește deliberat o cromatică artificială pentru a evita orice iluzie de naturalețe.', ru:'Фильм сознательно использует искусственную цветовую гамму, чтобы избежать всякой иллюзии естественности.', sq:'Filmi përdor qëllimisht një skemë ngjyrash artificiale për të shmangur çdo iluzion natyrshmërie.', tr:'Film, doğallık yanılsamasından kaçınmak için bilinçli olarak yapay bir renk düzeni kullanır.' })
    ]
  },
  {
    word: 'asketisch', language: 'de', cefrLevel: level, partOfSpeech: 'Adjective', article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'as-ke-tisch',
    topics: ['culture-and-media','social-and-relationships','advanced-analysis'], usageLabels: ['formal','written','advanced'], contextLabels: [],
    grammarNotes: ['adjective; describes severe simplicity, self-denial, or austere discipline'],
    collocations: [{ text: 'ein asketischer Lebensstil', meaning: 'an austere or self-denying lifestyle' }],
    wordFamilies: [{ lemma: 'die Askese', relationLabel: 'noun', note: null }], relations: [],
    meanings: meanings({ ar:'زاهد؛ متقشف؛ صارم في البساطة', ckb:'زاهیدانە؛ خۆبەخشینەوە؛ زۆر سادە و توند', en:'ascetic; austere; self-denying', fa:'زاهدانه؛ ریاضت‌کشانه؛ سخت‌گیرانه ساده', kmr:'asketîk; sade û xweqedexekar', pl:'ascetyczny; surowy; wyrzekający się wygód', ro:'ascetic; auster; plin de renunțare', ru:'аскетичный; суровый; отказывающийся от удобств', sq:'asketik; i përkorë; vetëmohues', tr:'asketik; sade ve mahrumiyetçi' }),
    examples: [
      ex('Sein asketischer Führungsstil ließ wenig Raum für Statussymbole oder repräsentative Gesten.', { ar:'لم يترك أسلوبه القيادي المتقشف مجالاً كبيراً لرموز المكانة أو الإيماءات الاستعراضية.', ckb:'شێوازی سەرکردایەتی زاهیدانەکەی شوێنی کەم بۆ هێماکانی پێگە یان جووڵەی نوێنەرایەتی دەهێشتەوە.', en:'His ascetic leadership style left little room for status symbols or representative gestures.', fa:'سبک رهبری زاهدانه او جای کمی برای نمادهای جایگاه یا حرکت‌های نمایشی باقی می‌گذاشت.', kmr:'Şêwaza rêveberiya wî ya asketîk cihê kêm ji bo sembolên statuyê an tevgerên nûnerî dihêşt.', pl:'Jego ascetyczny styl przywództwa pozostawiał niewiele miejsca na symbole statusu czy reprezentacyjne gesty.', ro:'Stilul său de conducere ascetic lăsa puțin loc simbolurilor de statut sau gesturilor reprezentative.', ru:'Его аскетичный стиль руководства оставлял мало места символам статуса или представительским жестам.', sq:'Stili i tij asketik i drejtimit linte pak hapësirë për simbole statusi ose gjeste përfaqësuese.', tr:'Onun asketik liderlik tarzı statü sembollerine veya temsili jestlere pek yer bırakmıyordu.' }),
      ex('Die asketische Ausstattung des Zimmers wirkte nicht ärmlich, sondern bewusst konzentriert.', { ar:'بدا تجهيز الغرفة المتقشف ليس فقيراً، بل مركزاً عن قصد.', ckb:'کەرەستەی زاهیدانەی ژوورەکە هەژارانە دیار نەبوو، بەڵکو بە ئاگاداری چڕبووەوە دیار بوو.', en:'The room’s austere furnishings did not seem poor, but deliberately focused.', fa:'چیدمان زاهدانه اتاق فقیرانه به نظر نمی‌رسید، بلکه آگاهانه متمرکز و مینیمال بود.', kmr:'Amadekirina asketîk a odeyê ne wek hejarî, lê wek hûrguliyeke bi zanebûn xuya dikir.', pl:'Ascetyczne wyposażenie pokoju nie sprawiało wrażenia biednego, lecz świadomie skoncentrowanego.', ro:'Amenajarea austeră a camerei nu părea sărăcăcioasă, ci deliberat concentrată.', ru:'Аскетичная обстановка комнаты выглядела не бедной, а сознательно сосредоточенной.', sq:'Pajisja asketike e dhomës nuk dukej e varfër, por e përqendruar qëllimisht.', tr:'Odanın asketik döşemesi yoksul değil, bilinçli olarak yoğunlaştırılmış görünüyordu.' })
    ]
  },
  {
    word: 'auf Anhieb', language: 'de', cefrLevel: level, partOfSpeech: 'Adverb', article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'auf An-hieb',
    topics: ['everyday-life','work-and-jobs','technology-and-it'], usageLabels: ['formal','written','workplace'], contextLabels: [],
    grammarNotes: ['fixed adverbial phrase; means immediately or on the first attempt'],
    collocations: [{ text: 'auf Anhieb funktionieren', meaning: 'to work on the first attempt' }],
    wordFamilies: [], relations: [],
    meanings: meanings({ ar:'من أول محاولة؛ فوراً', ckb:'لە یەکەم هەوڵدا؛ دەستبەجێ', en:'straight away; on the first attempt', fa:'در همان تلاش اول؛ فوراً', kmr:'di hewla yekem de; yekser', pl:'od razu; za pierwszym razem', ro:'din prima; imediat', ru:'сразу; с первой попытки', sq:'menjëherë; që në përpjekjen e parë', tr:'ilk denemede; hemen' }),
    examples: [
      ex('Die neue Schnittstelle funktionierte auf Anhieb, obwohl wir mit mehreren Sonderfällen gerechnet hatten.', { ar:'عملت الواجهة الجديدة من أول محاولة، رغم أننا كنا نتوقع عدة حالات خاصة.', ckb:'ڕووکاری پەیوەندی نوێ لە یەکەم هەوڵدا کاریکرد، هەرچەندە چاوەڕێی چەند حاڵەتی تایبەت بووین.', en:'The new interface worked on the first attempt, although we had expected several edge cases.', fa:'رابط جدید در همان تلاش اول کار کرد، هرچند انتظار چند حالت خاص را داشتیم.', kmr:'Navbera nû di hewla yekem de xebitî, herçend me çend rewşên taybetî hêvî dikirin.', pl:'Nowy interfejs zadziałał od razu, choć spodziewaliśmy się kilku przypadków szczególnych.', ro:'Noua interfață a funcționat din prima, deși ne așteptam la mai multe cazuri speciale.', ru:'Новый интерфейс заработал с первой попытки, хотя мы ожидали несколько особых случаев.', sq:'Ndërfaqja e re funksionoi që në përpjekjen e parë, megjithëse prisnim disa raste të veçanta.', tr:'Yeni arayüz ilk denemede çalıştı, oysa birkaç özel durum bekliyorduk.' }),
      ex('Sie fand sich in der fremden Stadt auf Anhieb zurecht.', { ar:'استطاعت أن تهتدي في المدينة الغريبة فوراً.', ckb:'لە شارە نامۆکەدا دەستبەجێ ڕێگای خۆی دۆزییەوە.', en:'She found her way around the unfamiliar city straight away.', fa:'او در شهر ناآشنا فوراً راه خود را پیدا کرد.', kmr:'Ew di bajarê xerîb de yekser rêya xwe dît.', pl:'Od razu odnalazła się w obcym mieście.', ro:'S-a orientat din prima în orașul necunoscut.', ru:'Она сразу сориентировалась в незнакомом городе.', sq:'Ajo u orientua menjëherë në qytetin e panjohur.', tr:'Yabancı şehirde hemen yolunu buldu.' })
    ]
  },
  {
    word: 'aufbegehren', language: 'de', cefrLevel: level, partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'aufbegehren', pronunciationIpa: null, syllableBreak: 'auf-be-geh-ren',
    topics: ['social-and-relationships','management-and-leadership','public-services'], usageLabels: ['formal','written','advanced','sensitive'], contextLabels: [],
    grammarNotes: ['verb; begehrt auf, begehrte auf, hat aufbegehrt; means to rebel or protest strongly'],
    collocations: [{ text: 'gegen eine Entscheidung aufbegehren', meaning: 'to rebel or protest against a decision' }],
    wordFamilies: [{ lemma: 'das Aufbegehren', relationLabel: 'noun', note: null }], relations: [],
    meanings: meanings({ ar:'يتمرد؛ يحتج بقوة', ckb:'سەرهەڵدان؛ بە توندی ناڕەزایی دەربڕین', en:'to rebel; to protest strongly', fa:'سرکشی کردن؛ شدیداً اعتراض کردن', kmr:'serî hildan; bi tundî nerazîbûn nîşan dan', pl:'buntować się; stanowczo protestować', ro:'a se revolta; a protesta vehement', ru:'восставать; резко протестовать', sq:'të ngrihesh kundër; të protestosh fuqishëm', tr:'başkaldırmak; sert biçimde itiraz etmek' }),
    examples: [
      ex('Als die Kürzungen ohne Anhörung beschlossen wurden, begehrten mehrere Abteilungen offen auf.', { ar:'عندما تقررت التخفيضات من دون استماع، تمردت عدة أقسام علناً.', ckb:'کاتێک کەمکردنەوەکان بەبێ گوێگرتن بڕیاردران، چەند بەشێک بە ئاشکرا ناڕەزایی توندیان دەربڕی.', en:'When the cuts were decided without consultation, several departments openly rebelled.', fa:'وقتی کاهش‌ها بدون شنیدن نظرها تصویب شد، چند بخش آشکارا اعتراض شدید کردند.', kmr:'Dema qutkirin bê guhdarîkirin hatin biryardan, çend beş bi eşkere serî hildan.', pl:'Gdy cięcia zatwierdzono bez konsultacji, kilka działów otwarcie się zbuntowało.', ro:'Când reducerile au fost decise fără consultare, mai multe departamente s-au revoltat deschis.', ru:'Когда сокращения утвердили без обсуждения, несколько отделов открыто восстали.', sq:'Kur shkurtimet u vendosën pa dëgjim, disa departamente u ngritën hapur kundër.', tr:'Kesintiler danışma yapılmadan kararlaştırılınca birkaç departman açıkça başkaldırdı.' }),
      ex('Der Roman erzählt von Jugendlichen, die gegen eine erstarrte Dorfordnung aufbegehren.', { ar:'تروي الرواية قصة شباب يتمردون على نظام قروي جامد.', ckb:'ڕۆمانەکە باس لە گەنجانێک دەکات کە دژی ڕێکخستنی وشکی گوندەکە سەرهەڵدەدەن.', en:'The novel tells of young people rebelling against a rigid village order.', fa:'رمان از جوانانی می‌گوید که علیه نظم خشک روستا سرکشی می‌کنند.', kmr:'Roman behsa ciwanan dike ku li dijî pergala hişk a gund serî hildidin.', pl:'Powieść opowiada o młodych ludziach buntujących się przeciw skostniałemu porządkowi wsi.', ro:'Romanul povestește despre tineri care se revoltă împotriva unei ordini rurale încremenite.', ru:'Роман рассказывает о молодых людях, восстающих против застывшего деревенского порядка.', sq:'Romani tregon për të rinj që ngrihen kundër një rendi të ngurtë fshati.', tr:'Roman, katılaşmış bir köy düzenine başkaldıran gençleri anlatır.' })
    ]
  },
  {
    word: 'aufkündigen', language: 'de', cefrLevel: level, partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'aufkündigen', pronunciationIpa: null, syllableBreak: 'auf-kün-di-gen',
    topics: ['contracts-and-negotiation','business-communication','social-and-relationships'], usageLabels: ['formal','written','business','advanced'], contextLabels: [],
    grammarNotes: ['separable verb; kündigt auf, kündigte auf, hat aufgekündigt; means to terminate or renounce an agreement, alliance, or relationship'],
    collocations: [{ text: 'eine Vereinbarung aufkündigen', meaning: 'to terminate or renounce an agreement' }],
    wordFamilies: [{ lemma: 'die Aufkündigung', relationLabel: 'noun', note: null }], relations: [],
    meanings: meanings({ ar:'ينهي اتفاقاً؛ يفسخ؛ يتخلى رسمياً عن', ckb:'گرێبەست یان ڕێککەوتن هەڵبوەشێنێتەوە؛ بە فەرمی واز بهێنێت', en:'to terminate; to renounce; to give notice ending an agreement', fa:'فسخ کردن؛ پایان دادن رسمی؛ از توافق خارج شدن', kmr:'peymanê bidawî kirin; bi fermî dev jê berdan', pl:'wypowiedzieć; zerwać formalnie', ro:'a denunța; a rezilia; a pune capăt formal', ru:'расторгать; официально отказываться от соглашения', sq:'ta ndërpresësh zyrtarisht; ta denoncosh marrëveshjen', tr:'feshetmek; resmen sona erdirmek' }),
    examples: [
      ex('Der Lieferant kündigte die Rahmenvereinbarung auf, nachdem die Zahlungsziele mehrfach überschritten worden waren.', { ar:'أنهى المورد الاتفاقية الإطارية بعد تجاوز مواعيد الدفع عدة مرات.', ckb:'دابینکەرەکە ڕێککەوتنی چوارچێوەیی هەڵوەشاندەوە دوای ئەوەی ماوەکانی پارەدان چەند جار تێپەڕێنران.', en:'The supplier terminated the framework agreement after payment terms had been exceeded several times.', fa:'تأمین‌کننده پس از چند بار عبور از مهلت‌های پرداخت، قرارداد چارچوب را فسخ کرد.', kmr:'Dabînker peymana çarçoveyê bidawî kir piştî ku demên dayînê çend caran derbas bûn.', pl:'Dostawca wypowiedział umowę ramową po wielokrotnym przekroczeniu terminów płatności.', ro:'Furnizorul a reziliat acordul-cadru după ce termenele de plată fuseseră depășite de mai multe ori.', ru:'Поставщик расторг рамочное соглашение после неоднократного нарушения сроков оплаты.', sq:'Furnitori e ndërpreu marrëveshjen kuadër pasi afatet e pagesës ishin tejkaluar disa herë.', tr:'Tedarikçi, ödeme vadeleri birkaç kez aşıldıktan sonra çerçeve anlaşmayı feshetti.' }),
      ex('Mit seinem öffentlichen Vorwurf kündigte er das fragile Vertrauensverhältnis endgültig auf.', { ar:'باتهامه العلني أنهى نهائياً علاقة الثقة الهشة.', ckb:'بە تۆمەتبارکردنی ئاشکرای، پەیوەندی متمانەی ناسکەکەی بە تەواوی هەڵوەشاندەوە.', en:'With his public accusation, he finally destroyed the fragile relationship of trust.', fa:'او با اتهام علنی خود رابطه شکننده اعتماد را برای همیشه از هم گسست.', kmr:'Bi tawanbariya xwe ya eşkere, têkiliya baweriyê ya nazik bi dawî kir.', pl:'Publicznym oskarżeniem ostatecznie zerwał kruche relacje zaufania.', ro:'Prin acuzația sa publică, a rupt definitiv relația fragilă de încredere.', ru:'Своим публичным обвинением он окончательно разрушил хрупкие доверительные отношения.', sq:'Me akuzën e tij publike, ai e prishi përfundimisht marrëdhënien e brishtë të besimit.', tr:'Kamusal suçlamasıyla kırılgan güven ilişkisini kesin olarak sona erdirdi.' })
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

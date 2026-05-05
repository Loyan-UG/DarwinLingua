const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '004';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const projectPath = path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj');
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['ahnungsvoll','allegorisch','allemal','die Alliteration','der Anachronismus','anachronistisch'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const labelMap = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const tokens = fs.readFileSync(sourcePath, 'utf8').split(',').map(t => t.trim()).filter(Boolean);
const words = tokens.slice(0, 6);
if (JSON.stringify(words) !== JSON.stringify(expected)) throw new Error(`Unexpected first tokens: ${JSON.stringify(words)}`);
const usedLabels = ['formal','written','advanced','academic','analysis','business','sensitive'];
for (const key of usedLabels) if (!labelMap.has(key)) throw new Error(`Missing taxonomy label: ${key}`);
const labels = usedLabels.map(key => labelMap.get(key));
function meanings(obj) { return langs.map(language => ({ language, text: obj[language] })); }
function ex(baseText, obj) { return { baseText, translations: meanings(obj) }; }

const entries = [
  {
    word: 'ahnungsvoll', language: 'de', cefrLevel: level, partOfSpeech: 'Adjective', article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'ah-nungs-voll',
    topics: ['culture-and-media','advanced-analysis','social-and-relationships'], usageLabels: ['formal','written','advanced'], contextLabels: [],
    grammarNotes: ['adjective; literary or elevated register; means full of foreboding or suggestive awareness'],
    collocations: [{ text: 'ein ahnungsvoller Blick', meaning: 'a look full of foreboding or intuition' }],
    wordFamilies: [{ lemma: 'die Ahnung', relationLabel: 'noun', note: null }, { lemma: 'ahnen', relationLabel: 'verb', note: null }], relations: [],
    meanings: meanings({ ar:'مليء بالحدس أو التوجس', ckb:'پڕ لە هەستکردن بە شتێکی داهاتوو؛ پڕ لە دڵەڕاوکێ', en:'full of foreboding or intuition', fa:'آکنده از پیش‌آگاهی یا دلشوره', kmr:'tijî pêşhest an xemgîniya nezelal', pl:'pełen przeczucia; złowróżbny', ro:'plin de presimțire; prevestitor', ru:'исполненный предчувствия; зловещий', sq:'plot parandjenjë; ogurzi', tr:'sezgi veya önsezi dolu' }),
    examples: [
      ex('Ihr ahnungsvoller Blick verriet, dass sie die Tragweite der Entscheidung früher erfasste als die anderen.', { ar:'كشف نظرها المليء بالتوجس أنها أدركت مدى القرار قبل الآخرين.', ckb:'نیگای پڕ لە پێشبینینەکەی ئاشکرا کرد کە پێش ئەوانی تر قورسایی بڕیارەکەی تێگەیشتبوو.', en:'Her foreboding look revealed that she grasped the significance of the decision earlier than the others.', fa:'نگاه آکنده از پیش‌آگاهی او نشان می‌داد که پیامد تصمیم را زودتر از دیگران فهمیده است.', kmr:'Nêrîna wê ya tijî pêşhest nîşan da ku ew girîngiya biryarê ji yên din zûtir fêm kir.', pl:'Jej pełne przeczucia spojrzenie zdradzało, że wcześniej niż inni pojęła wagę tej decyzji.', ro:'Privirea ei plină de presimțire trăda faptul că înțelesese amploarea deciziei mai devreme decât ceilalți.', ru:'Её полный предчувствия взгляд выдавал, что она раньше других поняла значение этого решения.', sq:'Vështrimi i saj plot parandjenjë tregonte se ajo e kuptoi peshën e vendimit më herët se të tjerët.', tr:'Önsezi dolu bakışı, kararın ağırlığını diğerlerinden önce kavradığını belli ediyordu.' }),
      ex('Der Roman beginnt mit einer ahnungsvollen Ruhe, die den späteren Konflikt kaum merklich vorbereitet.', { ar:'تبدأ الرواية بهدوء مفعم بالتوجس يمهد للصراع اللاحق بشكل شبه غير ملحوظ.', ckb:'ڕۆمانەکە بە ئارامییەکی پڕ لە پێشبینین دەست پێدەکات کە بە کەمترین شێوە ناکۆکی دواتر ئامادە دەکات.', en:'The novel begins with a foreboding calm that almost imperceptibly prepares the later conflict.', fa:'رمان با آرامشی آکنده از دلشوره آغاز می‌شود که تقریباً نامحسوس درگیری بعدی را آماده می‌کند.', kmr:'Roman bi aramiyek tijî pêşhest dest pê dike ku şerê paşê bi zor xuya were amadekirin.', pl:'Powieść zaczyna się pełnym przeczucia spokojem, który niemal niezauważalnie przygotowuje późniejszy konflikt.', ro:'Romanul începe cu o liniște plină de presimțire, care pregătește aproape imperceptibil conflictul ulterior.', ru:'Роман начинается с предчувственной тишины, почти незаметно подготавливающей последующий конфликт.', sq:'Romani nis me një qetësi plot parandjenjë që përgatit pothuajse pa u vënë re konfliktin e mëvonshëm.', tr:'Roman, sonraki çatışmayı neredeyse fark edilmeden hazırlayan önsezili bir sükûnetle başlar.' })
    ]
  },
  {
    word: 'allegorisch', language: 'de', cefrLevel: level, partOfSpeech: 'Adjective', article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'al-le-go-risch',
    topics: ['culture-and-media','education-and-training','advanced-analysis'], usageLabels: ['formal','written','academic','advanced'], contextLabels: [],
    grammarNotes: ['adjective; related to allegory and symbolic representation'],
    collocations: [{ text: 'eine allegorische Darstellung', meaning: 'an allegorical representation' }],
    wordFamilies: [{ lemma: 'die Allegorie', relationLabel: 'noun', note: null }], relations: [],
    meanings: meanings({ ar:'رمزي؛ استعاري تمثيلي', ckb:'هێمایی؛ ئەلێگۆریایی', en:'allegorical; symbolically representative', fa:'تمثیلی؛ نمادین', kmr:'alegorîk; bi sembolî nûnerî dike', pl:'alegoryczny; symboliczny', ro:'alegoric; simbolic', ru:'аллегорический; символический', sq:'alegorik; simbolik', tr:'alegorik; simgesel' }),
    examples: [
      ex('Die Inszenierung deutet den zerfallenden Markt allegorisch als Bild einer erschöpften Gesellschaft.', { ar:'يفسر الإخراج السوق المنهار رمزياً كصورة لمجتمع منهك.', ckb:'شانۆسازییەکە بازاڕی داڕماو بە شێوەیەکی هێمایی وەک وێنەی کۆمەڵگایەکی ماندوو ڕاڤە دەکات.', en:'The production interprets the collapsing market allegorically as an image of an exhausted society.', fa:'این اجرا بازار رو به فروپاشی را به‌صورت تمثیلی تصویری از جامعه‌ای فرسوده تعبیر می‌کند.', kmr:'Derhênerî bazara hilweşiyayî bi awayekî alegorîk wek wêneyê civakeke westiyayî şîrove dike.', pl:'Inscenizacja interpretuje rozpadający się rynek alegorycznie jako obraz wyczerpanego społeczeństwa.', ro:'Montarea interpretează piața în destrămare alegoric ca imagine a unei societăți epuizate.', ru:'Постановка аллегорически трактует распадающийся рынок как образ истощённого общества.', sq:'Vënia në skenë e interpreton tregun që po shpërbëhet në mënyrë alegorike si imazh të një shoqërie të rraskapitur.', tr:'Sahneleme, çöken pazarı yorgun bir toplumun görüntüsü olarak alegorik biçimde yorumlar.' }),
      ex('Im Seminar wurde diskutiert, ob die Figur allegorisch für politische Verantwortung steht.', { ar:'نوقش في الندوة ما إذا كانت الشخصية تمثل رمزياً المسؤولية السياسية.', ckb:'لە سیمینارەکەدا باس کرا کە ئایا کارەکتەرەکە بە شێوەیەکی هێمایی نوێنەرایەتی بەرپرسیارێتی سیاسی دەکات یان نا.', en:'The seminar discussed whether the character stands allegorically for political responsibility.', fa:'در سمینار بحث شد که آیا این شخصیت به‌طور تمثیلی نماینده مسئولیت سیاسی است یا نه.', kmr:'Di semînerê de hate nîqaşkirin ka karakter bi awayekî alegorîk berpirsiyariya siyasî temsîl dike an na.', pl:'Na seminarium dyskutowano, czy postać alegorycznie reprezentuje odpowiedzialność polityczną.', ro:'În seminar s-a discutat dacă personajul reprezintă alegoric responsabilitatea politică.', ru:'На семинаре обсуждали, является ли персонаж аллегорическим воплощением политической ответственности.', sq:'Në seminar u diskutua nëse personazhi përfaqëson në mënyrë alegorike përgjegjësinë politike.', tr:'Seminerde karakterin siyasi sorumluluğu alegorik olarak temsil edip etmediği tartışıldı.' })
    ]
  },
  {
    word: 'allemal', language: 'de', cefrLevel: level, partOfSpeech: 'Adverb', article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'al-le-mal',
    topics: ['business-communication','everyday-life','advanced-analysis'], usageLabels: ['formal','written','advanced'], contextLabels: [],
    grammarNotes: ['adverb; means certainly, without question, or in any case; elevated but common in written argumentation'],
    collocations: [{ text: 'allemal besser', meaning: 'certainly better; better in any case' }],
    wordFamilies: [], relations: [],
    meanings: meanings({ ar:'بالتأكيد؛ على أي حال', ckb:'بێگومان؛ بەهەر حاڵ', en:'certainly; in any case; by all means', fa:'قطعاً؛ به هر حال', kmr:'bêguman; her halê de', pl:'z pewnością; w każdym razie', ro:'cu siguranță; în orice caz', ru:'несомненно; во всяком случае', sq:'me siguri; sidoqoftë', tr:'kesinlikle; her hâlükârda' }),
    examples: [
      ex('Eine transparente Begründung wäre allemal besser gewesen als die knappe Mitteilung per E-Mail.', { ar:'كان تقديم تبرير شفاف سيكون بالتأكيد أفضل من الإخطار المقتضب عبر البريد الإلكتروني.', ckb:'پاساودانێکی ڕوون بێگومان باشتر دەبوو لە ئاگادارکردنەوەیەکی کورتی بە ئیمەیڵ.', en:'A transparent explanation would certainly have been better than the brief notice by email.', fa:'یک توضیح شفاف قطعاً بهتر از اطلاع‌رسانی کوتاه از طریق ایمیل بود.', kmr:'Sedemek zelal bêguman ji agahdariya kurt bi e-nameyê çêtir bû.', pl:'Przejrzyste uzasadnienie byłoby z pewnością lepsze niż krótka informacja e-mailem.', ro:'O justificare transparentă ar fi fost cu siguranță mai bună decât notificarea scurtă prin e-mail.', ru:'Прозрачное обоснование было бы несомненно лучше краткого уведомления по электронной почте.', sq:'Një arsyetim transparent do të ishte me siguri më i mirë se njoftimi i shkurtër me email.', tr:'Şeffaf bir gerekçe, e-postayla gönderilen kısa bildirimden kesinlikle daha iyi olurdu.' }),
      ex('Für eine Nacht ist das kleine Hotel allemal ausreichend, auch wenn es keinen besonderen Komfort bietet.', { ar:'لليلة واحدة يكون الفندق الصغير كافياً بالتأكيد، حتى لو لم يوفر راحة خاصة.', ckb:'بۆ یەک شەو، هوتێلە بچووکەکە بەهەر حاڵ بەسە، هەرچەندە ئاسوودەیی تایبەت پێشکەش ناکات.', en:'For one night, the small hotel is certainly sufficient, even if it offers no special comfort.', fa:'برای یک شب، این هتل کوچک قطعاً کافی است، هرچند راحتی خاصی ارائه نمی‌دهد.', kmr:'Ji bo şevekê, otêla biçûk bêguman têrê dike, herçend rehetiyeke taybet nade.', pl:'Na jedną noc mały hotel z pewnością wystarczy, nawet jeśli nie oferuje szczególnego komfortu.', ro:'Pentru o noapte, hotelul mic este cu siguranță suficient, chiar dacă nu oferă un confort deosebit.', ru:'На одну ночь маленькой гостиницы вполне достаточно, даже если она не предлагает особого комфорта.', sq:'Për një natë, hoteli i vogël është me siguri i mjaftueshëm, edhe pse nuk ofron ndonjë komoditet të veçantë.', tr:'Bir gece için küçük otel kesinlikle yeterlidir, özel bir konfor sunmasa bile.' })
    ]
  },
  {
    word: 'die Alliteration', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Alliterationen', infinitive: null, pronunciationIpa: null, syllableBreak: 'Al-li-te-ra-ti-on',
    topics: ['culture-and-media','education-and-training','business-communication'], usageLabels: ['formal','written','academic','analysis'], contextLabels: [],
    grammarNotes: ['feminine noun; rhetorical and literary term'],
    collocations: [{ text: 'eine wirkungsvolle Alliteration', meaning: 'an effective alliteration' }],
    wordFamilies: [{ lemma: 'alliterieren', relationLabel: 'verb', note: null }], relations: [],
    meanings: meanings({ ar:'جناس استهلالي؛ تكرار الحرف الأول', ckb:'دووبارەبوونەوەی دەنگی سەرەتا؛ ئالیتێراسیۆن', en:'alliteration; repetition of initial sounds', fa:'واج‌آرایی؛ تکرار صدای آغازین', kmr:'dubarekirina dengên destpêkê; alîterasyon', pl:'aliteracja; powtórzenie początkowych głosek', ro:'aliterație; repetarea sunetelor inițiale', ru:'аллитерация; повтор начальных звуков', sq:'aliteracion; përsëritje e tingujve fillestarë', tr:'aliterasyon; baştaki seslerin tekrarı' }),
    examples: [
      ex('Die Alliteration im Slogan machte die Marke einprägsamer, ohne aufdringlich zu wirken.', { ar:'جعلت الجناس الاستهلالي في الشعار العلامة التجارية أسهل تذكراً دون أن يبدو مزعجاً.', ckb:'ئالیتێراسیۆنی دروشمەکە مارکەکەی زیاتر لەبیرماندنی کرد، بەبێ ئەوەی توند یان بێزارکەر دیار بێت.', en:'The alliteration in the slogan made the brand more memorable without seeming intrusive.', fa:'واج‌آرایی در شعار باعث شد برند به‌یادماندنی‌تر شود، بدون آنکه آزاردهنده به نظر برسد.', kmr:'Alîterasyona di sloganê de marke zêdetir di bîrê de ma, bê ku zêde têketî xuya bike.', pl:'Aliteracja w sloganie sprawiła, że marka była łatwiejsza do zapamiętania, nie sprawiając wrażenia nachalnej.', ro:'Aliterația din slogan a făcut marca mai ușor de reținut, fără să pară insistentă.', ru:'Аллитерация в слогане сделала бренд более запоминающимся, не выглядя навязчиво.', sq:'Aliteracioni në slogan e bëri markën më të paharrueshme pa u dukur imponuese.', tr:'Slogandaki aliterasyon markayı daha akılda kalıcı yaptı, rahatsız edici görünmeden.' }),
      ex('Im Gedicht verstärkt die Alliteration den Rhythmus, ohne den Sinn der Zeile zu überdecken.', { ar:'في القصيدة تعزز الجناس الاستهلالي الإيقاع دون أن تغطي معنى السطر.', ckb:'لە شیعرەکەدا ئالیتێراسیۆن ڕیتمەکە بەهێزتر دەکات، بەبێ ئەوەی مانای دێڕەکە دابپۆشێت.', en:'In the poem, the alliteration strengthens the rhythm without obscuring the meaning of the line.', fa:'در شعر، واج‌آرایی ریتم را تقویت می‌کند بدون آنکه معنای مصرع را بپوشاند.', kmr:'Di helbestê de alîterasyon rîtimê xurt dike bê ku wateya rêzê veşêre.', pl:'W wierszu aliteracja wzmacnia rytm, nie przesłaniając sensu wersu.', ro:'În poem, aliterația întărește ritmul fără să acopere sensul versului.', ru:'В стихотворении аллитерация усиливает ритм, не заслоняя смысл строки.', sq:'Në poezi, aliteracioni forcon ritmin pa mbuluar kuptimin e vargut.', tr:'Şiirde aliterasyon, dizenin anlamını örtmeden ritmi güçlendirir.' })
    ]
  },
  {
    word: 'der Anachronismus', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'der', plural: 'Anachronismen', infinitive: null, pronunciationIpa: null, syllableBreak: 'A-na-chro-nis-mus',
    topics: ['culture-and-media','advanced-analysis','management-and-leadership'], usageLabels: ['formal','written','academic','analysis'], contextLabels: [],
    grammarNotes: ['masculine noun; plural: Anachronismen'],
    collocations: [{ text: 'ein historischer Anachronismus', meaning: 'a historical anachronism' }],
    wordFamilies: [{ lemma: 'anachronistisch', relationLabel: 'adjective', note: null }], relations: [],
    meanings: meanings({ ar:'مفارقة زمنية؛ شيء خارج زمنه', ckb:'شتێک لە دەرەوەی کاتی خۆی؛ ناهەماهنگیی کاتی', en:'anachronism; something out of its time', fa:'زمان‌پریشی؛ چیزی نامتناسب با زمان خود', kmr:'anakronîzm; tiştek li derveyî dema xwe', pl:'anachronizm; coś niezgodnego z epoką', ro:'anacronism; ceva nepotrivit epocii sale', ru:'анахронизм; нечто не соответствующее своему времени', sq:'anakronizëm; diçka jashtë kohës së vet', tr:'anakronizm; zamanına uymayan şey' }),
    examples: [
      ex('Die starre Präsenzpflicht wirkt in einem international verteilten Team zunehmend wie ein Anachronismus.', { ar:'يبدو شرط الحضور الصارم في فريق موزع دولياً بشكل متزايد كأنه مفارقة زمنية.', ckb:'پابەندی توندی ئامادەبوون لە تیمێکی نێودەوڵەتی دابەشکراودا زیاتر وەک شتێکی دەرەوەی کاتی خۆی دەردەکەوێت.', en:'The rigid attendance requirement increasingly seems like an anachronism in an internationally distributed team.', fa:'الزام سخت‌گیرانه حضور در تیمی پراکنده در سطح بین‌المللی بیش از پیش شبیه یک زمان‌پریشی به نظر می‌رسد.', kmr:'Merca amadebûna hişk di tîmeke navneteweyî de zêdetir wek anakronîzmek xuya dike.', pl:'Sztywny obowiązek obecności w międzynarodowo rozproszonym zespole coraz bardziej wygląda jak anachronizm.', ro:'Obligația rigidă de prezență pare tot mai mult un anacronism într-o echipă distribuită internațional.', ru:'Жёсткое требование присутствия во всё более распределённой международной команде выглядит анахронизмом.', sq:'Detyrimi i ngurtë për prani duket gjithnjë e më shumë si anakronizëm në një ekip të shpërndarë ndërkombëtarisht.', tr:'Katı ofiste bulunma zorunluluğu, uluslararası dağılmış bir ekipte giderek anakronizm gibi görünüyor.' }),
      ex('Der Film nutzt den Anachronismus bewusst, um die Gegenwart im Gewand der Vergangenheit zu kritisieren.', { ar:'يستخدم الفيلم المفارقة الزمنية عمداً لينتقد الحاضر في هيئة الماضي.', ckb:'فیلمەکە بە ئاگاداری ناهەماهنگیی کاتی بەکاردەهێنێت بۆ ڕەخنەگرتن لە ئێستا لە جل و بەرگی ڕابردوودا.', en:'The film deliberately uses anachronism to criticize the present in the guise of the past.', fa:'فیلم آگاهانه از زمان‌پریشی استفاده می‌کند تا حال را در پوشش گذشته نقد کند.', kmr:'Fîlm bi zanebûn anakronîzmê bi kar tîne da ku roja îro di cilê rabirdûyê de rexne bike.', pl:'Film celowo wykorzystuje anachronizm, aby krytykować teraźniejszość w kostiumie przeszłości.', ro:'Filmul folosește deliberat anacronismul pentru a critica prezentul sub înfățișarea trecutului.', ru:'Фильм сознательно использует анахронизм, чтобы критиковать настоящее в облике прошлого.', sq:'Filmi e përdor qëllimisht anakronizmin për të kritikuar të tashmen me petkun e së kaluarës.', tr:'Film, bugünü geçmişin kılığı altında eleştirmek için anakronizmi bilinçli olarak kullanır.' })
    ]
  },
  {
    word: 'anachronistisch', language: 'de', cefrLevel: level, partOfSpeech: 'Adjective', article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'a-na-chro-nis-tisch',
    topics: ['advanced-analysis','culture-and-media','business-communication'], usageLabels: ['formal','written','academic','analysis'], contextLabels: [],
    grammarNotes: ['adjective; describes something historically misplaced or outdated'],
    collocations: [{ text: 'anachronistisch wirken', meaning: 'to seem outdated or out of its historical time' }],
    wordFamilies: [{ lemma: 'der Anachronismus', relationLabel: 'noun', note: null }], relations: [],
    meanings: meanings({ ar:'مخالف لزمنه؛ عتيق على نحو غير مناسب', ckb:'لەگەڵ کاتی خۆی نەگونجاو؛ کۆن و ناهەماهنگ', en:'anachronistic; out of date or out of its historical time', fa:'زمان‌پریشانه؛ نامتناسب با زمان؛ کهنه', kmr:'anakronîstîk; li derveyî dema xwe', pl:'anachroniczny; niepasujący do epoki', ro:'anacronic; nepotrivit epocii', ru:'анахроничный; не соответствующий времени', sq:'anakronik; jashtë kohe', tr:'anakronik; zamanına uymayan' }),
    examples: [
      ex('Die Vorstellung, Innovation entstehe nur hinter verschlossenen Labortüren, ist anachronistisch.', { ar:'فكرة أن الابتكار ينشأ فقط خلف أبواب المختبرات المغلقة فكرة عتيقة وغير مناسبة لعصرها.', ckb:'ئەو بۆچوونەی نوێکاری تەنها لە پشت دەرگای داخراوی تاقیگەکان دروست دەبێت، کۆن و ناهەماهنگە.', en:'The idea that innovation emerges only behind closed laboratory doors is anachronistic.', fa:'این تصور که نوآوری فقط پشت درهای بسته آزمایشگاه شکل می‌گیرد، زمان‌پریشانه است.', kmr:'Ew fikir ku nûjenî tenê li pişt deriyên girtî yên laboratûvarê çêdibe anakronîstîk e.', pl:'Wyobrażenie, że innowacja powstaje tylko za zamkniętymi drzwiami laboratoriów, jest anachroniczne.', ro:'Ideea că inovația apare doar în spatele ușilor închise ale laboratoarelor este anacronică.', ru:'Представление о том, что инновации возникают только за закрытыми дверями лабораторий, анахронично.', sq:'Ideja se inovacioni lind vetëm pas dyerve të mbyllura të laboratorëve është anakronike.', tr:'İnovasyonun yalnızca kapalı laboratuvar kapılarının ardında doğduğu düşüncesi anakroniktir.' }),
      ex('Die anachronistische Sprache der Figur macht sichtbar, wie weit sie von ihrer Umgebung entfremdet ist.', { ar:'تكشف لغة الشخصية المفارقة لزمنها مدى ابتعادها عن محيطها.', ckb:'زمانی ناهەماهنگی کارەکتەرەکە دەری دەخات کە چەندە لە ژینگەی دەوروبەری نامۆ بووە.', en:'The character’s anachronistic language reveals how alienated they are from their surroundings.', fa:'زبان زمان‌پریشانه شخصیت نشان می‌دهد که او تا چه اندازه از محیط اطراف خود بیگانه شده است.', kmr:'Zimanê anakronîstîk ê karakterê nîşan dide ku ew çiqas ji derdora xwe xerîb bûye.', pl:'Anachroniczny język postaci pokazuje, jak bardzo jest ona wyobcowana ze swojego otoczenia.', ro:'Limbajul anacronic al personajului arată cât de înstrăinat este de mediul său.', ru:'Анахроничная речь персонажа показывает, насколько он отчуждён от своего окружения.', sq:'Gjuha anakronike e personazhit tregon sa shumë është tjetërsuar nga mjedisi i tij.', tr:'Karakterin anakronik dili, çevresine ne kadar yabancılaştığını görünür kılar.' })
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

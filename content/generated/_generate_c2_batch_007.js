const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '007';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const projectPath = path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj');
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['das Aperçu','apodiktisch','aporetisch','die Aporie','archaisch','der Archetyp'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const labelMap = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const tokens = fs.readFileSync(sourcePath, 'utf8').split(',').map(t => t.trim()).filter(Boolean);
const words = tokens.slice(0, 6);
if (JSON.stringify(words) !== JSON.stringify(expected)) throw new Error(`Unexpected first tokens: ${JSON.stringify(words)}`);
const usedLabels = ['formal','written','advanced','academic','analysis','business'];
for (const key of usedLabels) if (!labelMap.has(key)) throw new Error(`Missing taxonomy label: ${key}`);
const labels = usedLabels.map(key => labelMap.get(key));
function meanings(obj) { return langs.map(language => ({ language, text: obj[language] })); }
function ex(baseText, obj) { return { baseText, translations: meanings(obj) }; }

const entries = [
  {
    word: 'das Aperçu', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'das', plural: 'Aperçus', infinitive: null, pronunciationIpa: null, syllableBreak: 'A-per-çu',
    topics: ['culture-and-media','business-communication','advanced-analysis'], usageLabels: ['formal','written','advanced'], contextLabels: [],
    grammarNotes: ['neuter noun; French loanword; concise witty or insightful remark'],
    collocations: [{ text: 'ein treffendes Aperçu', meaning: 'a concise and apt observation' }],
    wordFamilies: [], relations: [],
    meanings: meanings({ ar:'ملاحظة ذكية وموجزة', ckb:'تێبینییەکی زیرەک و کورت', en:'witty concise observation; aperçu', fa:'نکته‌سنجی کوتاه و هوشمندانه', kmr:'têbîniyeke jîr û kurt', pl:'błyskotliwa, zwięzła uwaga', ro:'observație concisă și spirituală', ru:'остроумное краткое замечание', sq:'vërejtje e shkurtër dhe e mprehtë', tr:'kısa ve zekice gözlem' }),
    examples: [
      ex('Sein Aperçu über die Bürokratie war so präzise, dass es später im Bericht zitiert wurde.', { ar:'كانت ملاحظته الذكية عن البيروقراطية دقيقة جداً حتى اقتُبست لاحقاً في التقرير.', ckb:'تێبینییە زیرەکەکەی دەربارەی بیرۆکراسی هێندە ورد بوو کە دواتر لە ڕاپۆرتەکەدا وەرگیرا.', en:'His aperçu about bureaucracy was so precise that it was later quoted in the report.', fa:'نکته‌سنجی او درباره بوروکراسی آن‌قدر دقیق بود که بعداً در گزارش نقل شد.', kmr:'Aperçuya wî li ser bîrokratiyê ewqas rast bû ku paşê di raporê de hate gotin.', pl:'Jego błyskotliwa uwaga o biurokracji była tak trafna, że później zacytowano ją w raporcie.', ro:'Observația lui spirituală despre birocrație a fost atât de precisă încât a fost citată ulterior în raport.', ru:'Его остроумное замечание о бюрократии было настолько точным, что позже его процитировали в отчёте.', sq:'Vërejtja e tij e mprehtë për burokracinë ishte aq e saktë sa më vonë u citua në raport.', tr:'Bürokrasi hakkındaki kısa ve zekice gözlemi o kadar yerindeydi ki daha sonra raporda alıntılandı.' }),
      ex('Der Essay besteht nicht aus Argumenten, sondern aus glänzenden Aperçus, die lose miteinander verbunden sind.', { ar:'لا يتكون المقال من حجج، بل من ملاحظات لامعة مترابطة بشكل فضفاض.', ckb:'وتارەکە لە بەڵگە پێک نەهاتووە، بەڵکو لە تێبینییە درەوشاوەکان پێک هاتووە کە بە شێوەیەکی نەرم پێکەوە بەستراون.', en:'The essay consists not of arguments but of brilliant aperçus loosely connected to one another.', fa:'جستار نه از استدلال‌ها، بلکه از نکته‌سنجی‌های درخشانی تشکیل شده که آزادانه به هم وصل شده‌اند.', kmr:'Gotar ne ji argumanan, lê ji aperçuyên geş pêk tê ku bi şêweyekî sist bi hev ve girêdayî ne.', pl:'Esej nie składa się z argumentów, lecz z błyskotliwych uwag luźno ze sobą powiązanych.', ro:'Eseul nu este alcătuit din argumente, ci din observații strălucite legate lejer între ele.', ru:'Эссе состоит не из аргументов, а из блестящих афоризмов, свободно связанных между собой.', sq:'Eseja nuk përbëhet nga argumente, por nga vërejtje të shkëlqyera të lidhura lirshëm me njëra-tjetrën.', tr:'Deneme argümanlardan değil, birbirine gevşekçe bağlanmış parlak kısa gözlemlerden oluşur.' })
    ]
  },
  {
    word: 'apodiktisch', language: 'de', cefrLevel: level, partOfSpeech: 'Adjective', article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'a-po-dik-tisch',
    topics: ['advanced-analysis','business-communication','education-and-training'], usageLabels: ['formal','written','academic','analysis'], contextLabels: [],
    grammarNotes: ['adjective; describes categorical, dogmatic, or asserted as unquestionably true'],
    collocations: [{ text: 'apodiktisch behaupten', meaning: 'to assert categorically' }],
    wordFamilies: [{ lemma: 'die Apodiktik', relationLabel: 'noun', note: null }], relations: [],
    meanings: meanings({ ar:'قطعي؛ جازم بشكل لا يقبل النقاش', ckb:'یەکلاکارەوە؛ بێ ئەوەی گفتوگۆ قبوڵ بکات', en:'apodictic; categorical; dogmatic', fa:'قطعی و جزم‌گرایانه؛ بی‌چون‌وچرا', kmr:'qetî; bê nîqaş; dogmatîk', pl:'apodyktyczny; kategoryczny', ro:'apodictic; categoric; dogmatic', ru:'аподиктический; категоричный', sq:'apodiktik; kategorik', tr:'kesin hükümlü; kategorik' }),
    examples: [
      ex('Die Geschäftsführung formulierte ihre Vorgaben so apodiktisch, dass kaum Raum für fachliche Einwände blieb.', { ar:'صاغت الإدارة تعليماتها بشكل قطعي إلى حد لم يترك مجالاً كبيراً للاعتراضات المتخصصة.', ckb:'بەڕێوەبەرایەتی ڕێنماییەکانی بە شێوەیەکی یەکلاکارەوە داڕشت کە شوێنی کەم بۆ ناڕەزایی پسپۆڕانە ما.', en:'Management phrased its requirements so categorically that there was hardly room for professional objections.', fa:'مدیریت دستورالعمل‌های خود را چنان قطعی بیان کرد که تقریباً جایی برای ایرادهای تخصصی باقی نماند.', kmr:'Rêveberî daxwazên xwe ewqas qetî got ku cihê nerazîbûnên pisporî kêm ma.', pl:'Zarząd sformułował wytyczne tak apodyktycznie, że prawie nie było miejsca na merytoryczne zastrzeżenia.', ro:'Conducerea și-a formulat cerințele atât de categoric încât aproape nu a mai rămas loc pentru obiecții profesionale.', ru:'Руководство сформулировало требования настолько категорично, что почти не осталось места для профессиональных возражений.', sq:'Drejtoria i formuloi kërkesat aq kategorikisht sa mbeti pak hapësirë për kundërshtime profesionale.', tr:'Yönetim taleplerini öyle kesin bir dille ifade etti ki mesleki itirazlara neredeyse yer kalmadı.' }),
      ex('Der Autor urteilt apodiktisch, ohne die historischen Gegenbeispiele ernsthaft zu prüfen.', { ar:'يحكم الكاتب بشكل جازم من دون فحص الأمثلة التاريخية المضادة بجدية.', ckb:'نووسەر بە شێوەیەکی یەکلاکارەوە حوکم دەدات، بەبێ ئەوەی نموونە مێژووییە دژەکان بە جدی بپشکنێت.', en:'The author judges categorically without seriously examining the historical counterexamples.', fa:'نویسنده به‌طور جزم‌گرایانه داوری می‌کند، بی‌آنکه نمونه‌های تاریخی مخالف را جدی بررسی کند.', kmr:'Nivîskar bi awayekî qetî dad dide bê ku nimûneyên dîrokî yên dijber bi ciddî vekole.', pl:'Autor ocenia apodyktycznie, nie analizując poważnie historycznych kontrprzykładów.', ro:'Autorul judecă apodictic, fără să examineze serios contraexemplele istorice.', ru:'Автор судит категорично, не рассматривая всерьёз исторические контрпримеры.', sq:'Autori gjykon në mënyrë kategorike pa shqyrtuar seriozisht kundërshembujt historikë.', tr:'Yazar, tarihsel karşı örnekleri ciddi biçimde incelemeden kesin hükümler veriyor.' })
    ]
  },
  {
    word: 'aporetisch', language: 'de', cefrLevel: level, partOfSpeech: 'Adjective', article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'a-po-re-tisch',
    topics: ['advanced-analysis','education-and-training','culture-and-media'], usageLabels: ['formal','written','academic','analysis'], contextLabels: [],
    grammarNotes: ['adjective; philosophical term for a problem that ends in an unresolved difficulty'],
    collocations: [{ text: 'eine aporetische Situation', meaning: 'a situation marked by an unresolved contradiction' }],
    wordFamilies: [{ lemma: 'die Aporie', relationLabel: 'noun', note: null }], relations: [],
    meanings: meanings({ ar:'مأزقي؛ قائم على إشكال بلا حل واضح', ckb:'گرفتارییەکی فەلسەفی؛ بێ چارەسەری ڕوون', en:'aporetic; marked by an unresolved difficulty', fa:'آپورتیک؛ گرفتار تناقض حل‌نشده', kmr:'aporetîk; bi dijwariyeke bêçareserî', pl:'aporetyczny; oparty na nierozwiązywalnej trudności', ro:'aporetic; marcat de o dificultate nerezolvată', ru:'апоретический; связанный с неразрешимым затруднением', sq:'aporetik; me vështirësi të pazgjidhur', tr:'aporetik; çözümsüz güçlük içeren' }),
    examples: [
      ex('Die Diskussion wurde aporetisch, sobald Freiheit und Sicherheit als absolut gleichrangig behandelt wurden.', { ar:'أصبح النقاش مأزقياً عندما عوملت الحرية والأمن كقيمتين مطلقتين ومتساويتين تماماً.', ckb:'گفتوگۆکە گرفتاریی فەلسەفی وەرگرت کاتێک ئازادی و ئاسایش وەک دوو بەهای یەکسان و ڕەها مامەڵەیان لەگەڵ کرا.', en:'The discussion became aporetic once freedom and security were treated as absolutely equal values.', fa:'بحث زمانی آپورتیک شد که آزادی و امنیت به‌عنوان ارزش‌هایی کاملاً هم‌رتبه و مطلق در نظر گرفته شدند.', kmr:'Nîqaş aporetîk bû dema azadî û ewlehî wek nirxên bi tevahî hevqed û mutleq hatin dîtin.', pl:'Dyskusja stała się aporetyczna, gdy wolność i bezpieczeństwo potraktowano jako wartości absolutnie równorzędne.', ro:'Discuția a devenit aporetică de îndată ce libertatea și securitatea au fost tratate ca valori absolut egale.', ru:'Дискуссия стала апоретической, как только свободу и безопасность стали рассматривать как абсолютно равнозначные ценности.', sq:'Diskutimi u bë aporetik sapo liria dhe siguria u trajtuan si vlera absolutisht të barabarta.', tr:'Özgürlük ve güvenlik mutlak olarak eşdeğer değerler gibi ele alınınca tartışma aporetik hale geldi.' }),
      ex('Der Film endet aporetisch: Jede mögliche Lösung würde den moralischen Konflikt nur verschieben.', { ar:'ينتهي الفيلم بمأزق: فكل حل ممكن لن يفعل سوى تأجيل الصراع الأخلاقي.', ckb:'فیلمەکە بە گرفتارییەکی بێچارەسەر کۆتایی دێت: هەر چارەسەرێک تەنها ناکۆکی ئەخلاقی دوا دەخات.', en:'The film ends aporetically: every possible solution would merely postpone the moral conflict.', fa:'فیلم به‌صورت آپورتیک پایان می‌یابد: هر راه‌حل ممکن فقط تعارض اخلاقی را به تعویق می‌اندازد.', kmr:'Fîlm bi awayekî aporetîk diqede: her çareseriyek gengaz tenê nakokiya exlaqî paş dixîne.', pl:'Film kończy się aporetycznie: każde możliwe rozwiązanie jedynie odsunęłoby konflikt moralny.', ro:'Filmul se încheie aporetic: orice soluție posibilă ar amâna doar conflictul moral.', ru:'Фильм заканчивается апоретически: любое возможное решение лишь отложило бы моральный конфликт.', sq:'Filmi përfundon në mënyrë aporetike: çdo zgjidhje e mundshme vetëm do ta shtynte konfliktin moral.', tr:'Film aporetik biçimde biter: Olası her çözüm ahlaki çatışmayı yalnızca erteleyecektir.' })
    ]
  },
  {
    word: 'die Aporie', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Aporien', infinitive: null, pronunciationIpa: null, syllableBreak: 'A-po-rie',
    topics: ['advanced-analysis','education-and-training','culture-and-media'], usageLabels: ['formal','written','academic','analysis'], contextLabels: [],
    grammarNotes: ['feminine noun; philosophical term for an impasse or unresolved contradiction'],
    collocations: [{ text: 'in eine Aporie geraten', meaning: 'to end up in an unresolved contradiction' }],
    wordFamilies: [{ lemma: 'aporetisch', relationLabel: 'adjective', note: null }], relations: [],
    meanings: meanings({ ar:'مأزق فكري؛ تناقض بلا حل', ckb:'گرفتاریی بیرکردنەوە؛ ناکۆکی بێ چارەسەر', en:'aporia; unresolved contradiction or impasse', fa:'آپوری؛ بن‌بست فکری یا تناقض حل‌نشده', kmr:'aporî; nakokiya bêçareserî', pl:'aporia; nierozwiązywalna sprzeczność', ro:'aporie; contradicție nerezolvată', ru:'апория; неразрешимое противоречие', sq:'apori; kundërthënie e pazgjidhur', tr:'apori; çözümsüz çelişki' }),
    examples: [
      ex('Die Reform geriet in die Aporie, zugleich mehr Kontrolle und mehr Autonomie versprechen zu müssen.', { ar:'وقعت الإصلاحات في مأزق لأنها اضطرت إلى الوعد بمزيد من الرقابة ومزيد من الاستقلالية في الوقت نفسه.', ckb:'چاکسازییەکە کەوتە گرفتارییەوە چونکە پێویست بوو هاوکات بە کۆنترۆڵی زیاتر و ئۆتۆنۆمیی زیاتر بەڵێن بدات.', en:'The reform fell into the aporia of having to promise both more control and more autonomy.', fa:'اصلاحات در این آپوری گرفتار شد که باید هم‌زمان کنترل بیشتر و خودمختاری بیشتر را وعده می‌داد.', kmr:'Reform kete nav aporiyê ku diviya hem kontrola zêdetir û hem jî xweseriya zêdetir soz bida.', pl:'Reforma popadła w aporię, ponieważ musiała obiecywać jednocześnie więcej kontroli i więcej autonomii.', ro:'Reforma a intrat în aporia de a trebui să promită simultan mai mult control și mai multă autonomie.', ru:'Реформа попала в апорию необходимости обещать одновременно больше контроля и больше автономии.', sq:'Reforma ra në aporinë e detyrimit për të premtuar njëkohësisht më shumë kontroll dhe më shumë autonomi.', tr:'Reform, aynı anda hem daha fazla kontrol hem de daha fazla özerklik vaat etmek zorunda kalma aporisine düştü.' }),
      ex('Die Aporie des Textes besteht darin, dass er Wahrheit fordert und jede verbindliche Wahrheit bezweifelt.', { ar:'تكمن مأزقية النص في أنه يطالب بالحقيقة ويشك في كل حقيقة ملزمة.', ckb:'گرفتاریی دەقەکە لەوەدایە کە داوای ڕاستی دەکات و هەموو ڕاستییەکی پابەند گومان لێدەکات.', en:'The aporia of the text lies in the fact that it demands truth while doubting every binding truth.', fa:'آپوری متن در این است که حقیقت را مطالبه می‌کند و هم‌زمان در هر حقیقت الزام‌آور تردید می‌کند.', kmr:'Aporiya nivîsê ew e ku rastiyê dixwaze û her rastiyeke girêdayî guman dike.', pl:'Aporia tekstu polega na tym, że domaga się prawdy, a zarazem podważa każdą wiążącą prawdę.', ro:'Aporia textului constă în faptul că cere adevărul și totodată pune la îndoială orice adevăr obligatoriu.', ru:'Апория текста состоит в том, что он требует истины и одновременно сомневается во всякой обязательной истине.', sq:'Aporia e tekstit qëndron në faktin se kërkon të vërtetën dhe dyshon çdo të vërtetë detyruese.', tr:'Metnin aporisi, hakikati talep edip aynı zamanda her bağlayıcı hakikatten şüphe etmesidir.' })
    ]
  },
  {
    word: 'archaisch', language: 'de', cefrLevel: level, partOfSpeech: 'Adjective', article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'ar-cha-isch',
    topics: ['culture-and-media','advanced-analysis','education-and-training'], usageLabels: ['formal','written','academic','analysis'], contextLabels: [],
    grammarNotes: ['adjective; means archaic, ancient, or deliberately old-fashioned'],
    collocations: [{ text: 'archaische Sprache', meaning: 'archaic language' }],
    wordFamilies: [{ lemma: 'die Archaik', relationLabel: 'noun', note: null }], relations: [],
    meanings: meanings({ ar:'قديم الطابع؛ عتيق؛ بدائي', ckb:'کۆنباو؛ دێرین؛ سەرەتایی', en:'archaic; ancient; old-fashioned', fa:'کهن؛ باستانی؛ کهنه‌نما', kmr:'kevnar; arxaîk; kevnşêwaz', pl:'archaiczny; dawny; staroświecki', ro:'arhaic; vechi; învechit', ru:'архаичный; древний; старомодный', sq:'arkaik; i lashtë; i vjetruar', tr:'arkaik; eski; kadim' }),
    examples: [
      ex('Die archaische Formulierung im Vertrag erschwerte selbst erfahrenen Juristen die Auslegung.', { ar:'صعّبت الصياغة القديمة في العقد تفسيره حتى على محامين ذوي خبرة.', ckb:'داڕشتنی کۆنباوی ناو گرێبەستەکە تەنانەت بۆ یاساناسانی شارەزا ڕاڤەکردنی قورس کرد.', en:'The archaic wording in the contract made interpretation difficult even for experienced lawyers.', fa:'عبارت‌پردازی کهن در قرارداد حتی برای حقوق‌دانان باتجربه تفسیر را دشوار کرد.', kmr:'Gotina arxaîk a di peymanê de heta ji bo hiqûqnasên ezmûndar jî şîrovekirin zehmet kir.', pl:'Archaiczne sformułowanie w umowie utrudniało interpretację nawet doświadczonym prawnikom.', ro:'Formularea arhaică din contract a îngreunat interpretarea chiar și pentru juriști experimentați.', ru:'Архаичная формулировка в договоре затруднила толкование даже опытным юристам.', sq:'Formulimi arkaik në kontratë e vështirësoi interpretimin edhe për juristë me përvojë.', tr:'Sözleşmedeki arkaik ifade, deneyimli hukukçular için bile yorumu zorlaştırdı.' }),
      ex('Der Dichter verwendet archaische Bilder, um eine Welt vor jeder modernen Ordnung aufzurufen.', { ar:'يستخدم الشاعر صوراً قديمة الطابع لاستحضار عالم سابق لكل نظام حديث.', ckb:'شاعیر وێنەی کۆنباو بەکاردەهێنێت بۆ بانگکردنی جیهانێک پێش هەر ڕێکخستنێکی نوێ.', en:'The poet uses archaic images to evoke a world before any modern order.', fa:'شاعر از تصاویر کهن استفاده می‌کند تا جهانی پیش از هر نظم مدرن را احضار کند.', kmr:'Helbestvan wêneyên arxaîk bi kar tîne da ku cîhanek berî her pergala nûjen bîne bîrê.', pl:'Poeta używa archaicznych obrazów, aby przywołać świat sprzed każdego nowoczesnego porządku.', ro:'Poetul folosește imagini arhaice pentru a evoca o lume anterioară oricărei ordini moderne.', ru:'Поэт использует архаические образы, чтобы вызвать мир, предшествующий всякому современному порядку.', sq:'Poeti përdor imazhe arkaike për të thirrur një botë para çdo rendi modern.', tr:'Şair, her modern düzenden önceki bir dünyayı çağırmak için arkaik imgeler kullanır.' })
    ]
  },
  {
    word: 'der Archetyp', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'der', plural: 'Archetypen', infinitive: null, pronunciationIpa: null, syllableBreak: 'Ar-che-typ',
    topics: ['culture-and-media','advanced-analysis','education-and-training'], usageLabels: ['formal','written','academic','analysis'], contextLabels: [],
    grammarNotes: ['masculine noun; means original pattern, model, or recurring symbolic figure'],
    collocations: [{ text: 'der Archetyp des Helden', meaning: 'the archetype of the hero' }],
    wordFamilies: [{ lemma: 'archetypisch', relationLabel: 'adjective', note: null }], relations: [],
    meanings: meanings({ ar:'نموذج أصلي؛ صورة نمطية عميقة', ckb:'نموونەی بنەڕەتی؛ شێوەی سەرەکی دووبارەبووەوە', en:'archetype; original model or recurring symbolic pattern', fa:'کهن‌الگو؛ الگوی بنیادین', kmr:'arxetîp; modela bingehîn an şêwaza sembolî', pl:'archetyp; pierwotny wzorzec', ro:'arhetip; model originar', ru:'архетип; первообраз', sq:'arketip; model fillestar', tr:'arketip; temel örnek' }),
    examples: [
      ex('Die Figur ist kein individueller Charakter, sondern der Archetyp eines machtbewussten Herrschers.', { ar:'الشخصية ليست فرداً مميزاً، بل نموذجاً أصلياً لحاكم واعٍ بسلطته.', ckb:'کارەکتەرەکە کەسایەتییەکی تاکەکەسی نییە، بەڵکو ئارکێتایپی فەرمانڕەوایەکی هۆشیار بە دەسەڵاتە.', en:'The figure is not an individual character but the archetype of a power-conscious ruler.', fa:'این شخصیت فردیتی مستقل نیست، بلکه کهن‌الگوی فرمانروایی آگاه به قدرت خویش است.', kmr:'Karakter ne kesayetek takekesî ye, lê arxetîpa rêveberek bi hêz-agah e.', pl:'Postać nie jest indywidualnym charakterem, lecz archetypem władcy świadomego swojej siły.', ro:'Personajul nu este un caracter individual, ci arhetipul unui conducător conștient de putere.', ru:'Фигура является не индивидуальным характером, а архетипом властолюбивого правителя.', sq:'Figura nuk është karakter individual, por arketipi i një sundimtari të vetëdijshëm për pushtetin.', tr:'Figür bireysel bir karakter değil, iktidar bilincine sahip hükümdarın arketipidir.' }),
      ex('In vielen Markenstrategien dient der Archetyp des Mentors dazu, Vertrauen und Orientierung zu vermitteln.', { ar:'في كثير من استراتيجيات العلامات التجارية يُستخدم نموذج المرشد الأصلي لنقل الثقة والتوجيه.', ckb:'لە زۆر ستراتیژیی مارکەدا ئارکێتایپی ڕێنیشاندەر بەکاردەهێنرێت بۆ گواستنەوەی متمانە و ئاراستە.', en:'In many brand strategies, the mentor archetype is used to convey trust and guidance.', fa:'در بسیاری از استراتژی‌های برند، کهن‌الگوی راهنما برای انتقال اعتماد و جهت‌دهی به کار می‌رود.', kmr:'Di gelek stratejiyên markeyê de arxetîpa mamoste ji bo bawerî û rêberî dayîn tê bikaranîn.', pl:'W wielu strategiach marek archetyp mentora służy do przekazywania zaufania i orientacji.', ro:'În multe strategii de brand, arhetipul mentorului este folosit pentru a transmite încredere și orientare.', ru:'Во многих бренд-стратегиях архетип наставника используется для передачи доверия и ориентации.', sq:'Në shumë strategji markash, arketipi i mentorit përdoret për të përcjellë besim dhe orientim.', tr:'Birçok marka stratejisinde mentor arketipi güven ve yönlendirme iletmek için kullanılır.' })
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

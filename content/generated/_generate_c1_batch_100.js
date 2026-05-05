const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C1';
const levelLower = 'c1';
const batch = '100';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const importProject = path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj');
const expected = ['die Stilistik','stringent','die Strukturähnlichkeit','strukturprägend','die Subjektivität','substanziell'];
const languages = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const labelMap = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const sourceText = fs.readFileSync(sourcePath, 'utf8');
const tokens = sourceText.split(',').map(s => s.trim()).filter(Boolean);
const words = tokens.slice(0, 6);
if (JSON.stringify(words) !== JSON.stringify(expected)) throw new Error(`Unexpected first words: ${JSON.stringify(words)}`);

const usedLabels = ['academic','analysis','written','advanced','business','formal'];
const labels = usedLabels.map(k => { const l = labelMap.get(k); if (!l) throw new Error(`Missing label ${k}`); return l; });
function arr(obj) { return languages.map(language => ({ language, text: obj[language] })); }
function ex(baseText, tr) { return { baseText, translations: arr(tr) }; }

const entries = [
  {
    word:'die Stilistik', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'die', plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'Sti-lis-tik',
    topics:['culture-and-media','education-and-training','advanced-analysis'], usageLabels:['academic','analysis','written','advanced'], contextLabels:[],
    grammarNotes:['feminine noun; usually used in the singular for the study or quality of style'],
    collocations:[{text:'die Stilistik eines Textes analysieren', meaning:'to analyze the stylistic features of a text'}],
    wordFamilies:[{lemma:'stilistisch', relationLabel:'adjective', note:null},{lemma:'der Stil', relationLabel:'noun', note:null}], relations:[],
    meanings: arr({ar:'علم الأسلوب؛ السمات الأسلوبية',ckb:'ستایلناسی؛ تایبەتمەندییەکانی شێواز',en:'stylistics; stylistic features',fa:'سبک‌شناسی؛ ویژگی‌های سبکی',kmr:'stilnasî; taybetmendiyên şêwazî',pl:'stylistyka; cechy stylistyczne',ro:'stilistică; trăsături stilistice',ru:'стилистика; стилистические особенности',sq:'stilistikë; veçori stilistike',tr:'üslupbilim; biçemsel özellikler'}),
    examples:[
      ex('Die Stilistik des Berichts wirkt sachlich, vermeidet aber jede persönliche Verantwortung.',{ar:'تبدو أسلوبية التقرير موضوعية، لكنها تتجنب أي مسؤولية شخصية.',ckb:'ستایلی ڕاپۆرتەکە بابەتی دەردەکەوێت، بەڵام هەر بەرپرسیارێتییەکی کەسی دەرباز دەکات.',en:'The stylistics of the report appear objective, but avoid any personal responsibility.',fa:'سبک گزارش عینی به نظر می‌رسد، اما از هرگونه مسئولیت شخصی پرهیز می‌کند.',kmr:'Stilistîka raporê objektîf dixuye, lê ji her berpirsiyariya kesane dûr dikeve.',pl:'Stylistyka raportu wydaje się rzeczowa, ale unika jakiejkolwiek osobistej odpowiedzialności.',ro:'Stilistica raportului pare obiectivă, dar evită orice responsabilitate personală.',ru:'Стилистика отчета выглядит объективной, но избегает любой личной ответственности.',sq:'Stilistika e raportit duket objektive, por shmang çdo përgjegjësi personale.',tr:'Raporun üslubu nesnel görünüyor, ancak her türlü kişisel sorumluluktan kaçınıyor.'}),
      ex('In der Redaktion wurde lange über die Stilistik der Überschrift diskutiert.',{ar:'في هيئة التحرير نوقش أسلوب العنوان طويلاً.',ckb:'لە دەستەی نووسەراندا ماوەیەکی درێژ لەسەر شێوازی ناونیشانەکە گفتوگۆ کرا.',en:'The editorial team discussed the stylistics of the headline for a long time.',fa:'در تحریریه مدت زیادی درباره سبک تیتر بحث شد.',kmr:'Di redaksiyonê de demeke dirêj li ser stilistîka sernavê hate gotûbêjkirin.',pl:'W redakcji długo dyskutowano o stylistyce nagłówka.',ro:'În redacție s-a discutat mult despre stilistica titlului.',ru:'В редакции долго обсуждали стилистику заголовка.',sq:'Në redaksi u diskutua gjatë për stilistikën e titullit.',tr:'Yazı kurulunda başlığın üslubu uzun süre tartışıldı.'})
    ]
  },
  {
    word:'stringent', language:'de', cefrLevel:level, partOfSpeech:'Adjective', article:null, plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'strin-gent',
    topics:['advanced-analysis','business-communication','meetings-and-presentations'], usageLabels:['formal','analysis','written','advanced'], contextLabels:[],
    grammarNotes:['adjective; describes reasoning, argumentation, or structure as logically consistent and tight'],
    collocations:[{text:'stringent argumentieren', meaning:'to argue in a logically consistent way'}],
    wordFamilies:[{lemma:'die Stringenz', relationLabel:'noun', note:null}], relations:[],
    meanings: arr({ar:'محكم ومنطقي ومتسق',ckb:'توندوتۆڵ، لۆژیکی و یەکگرتوو',en:'stringent; logically consistent and rigorous',fa:'منسجم، منطقی و دقیق',kmr:'zexm, mantiqî û yekgirtî',pl:'spójny, logiczny i rygorystyczny',ro:'riguros și logic coerent',ru:'строгий, логичный и последовательный',sq:'i rreptë, logjik dhe koherent',tr:'tutarlı, mantıklı ve sıkı'}),
    examples:[
      ex('Die Präsentation war fachlich stark, aber die Argumentation hätte stringenter aufgebaut sein müssen.',{ar:'كان العرض قوياً من الناحية المتخصصة، لكن كان ينبغي بناء الحجة بشكل أكثر إحكاماً.',ckb:'پێشکەشکردنەکە لە ڕووی پسپۆڕییەوە بەهێز بوو، بەڵام پێویست بوو بەڵگەهێنانەکە یەکگرتووتر دابنرێت.',en:'The presentation was strong in terms of expertise, but the argumentation should have been structured more stringently.',fa:'ارائه از نظر تخصصی قوی بود، اما استدلال باید منسجم‌تر ساخته می‌شد.',kmr:'Pêşkêşî ji aliyê pisporiyê ve xurt bû, lê diviya argûmentasyon zêdetir bi rêkûpêkî were avakirin.',pl:'Prezentacja była mocna merytorycznie, ale argumentacja powinna być zbudowana bardziej spójnie.',ro:'Prezentarea a fost solidă profesional, dar argumentația ar fi trebuit structurată mai riguros.',ru:'Презентация была сильной с профессиональной точки зрения, но аргументацию следовало выстроить более строго.',sq:'Prezantimi ishte i fortë nga ana profesionale, por argumentimi duhej ndërtuar më koherentisht.',tr:'Sunum uzmanlık açısından güçlüydü, ancak argümantasyon daha tutarlı kurulmalıydı.'}),
      ex('Für ein komplexes Migrationsprojekt brauchen wir einen stringenten Ablaufplan mit klaren Abhängigkeiten.',{ar:'لمشروع ترحيل معقد نحتاج إلى خطة سير محكمة ذات تبعيات واضحة.',ckb:'بۆ پرۆژەیەکی ئاڵۆزی گواستنەوە پێویستمان بە پلانی کاری یەکگرتوو هەیە کە پەیوەندییەکان ڕوون بن.',en:'For a complex migration project, we need a stringent process plan with clear dependencies.',fa:'برای یک پروژه مهاجرت پیچیده، به برنامه اجرایی منسجم با وابستگی‌های روشن نیاز داریم.',kmr:'Ji bo projeyeke tevlihev a veguheztinê em hewceyê plansaziyeke rêkûpêk in bi girêdanên zelal.',pl:'W złożonym projekcie migracji potrzebujemy spójnego planu przebiegu z jasnymi zależnościami.',ro:'Pentru un proiect complex de migrare avem nevoie de un plan riguros de desfășurare, cu dependențe clare.',ru:'Для сложного проекта миграции нам нужен строгий план процесса с четкими зависимостями.',sq:'Për një projekt kompleks migrimi na duhet një plan i rreptë procesi me varësi të qarta.',tr:'Karmaşık bir geçiş projesi için net bağımlılıkları olan tutarlı bir süreç planına ihtiyacımız var.'})
    ]
  },
  {
    word:'die Strukturähnlichkeit', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'die', plural:'Strukturähnlichkeiten', infinitive:null, pronunciationIpa:null, syllableBreak:'Struk-tur-ähn-lich-keit',
    topics:['advanced-analysis','technology-and-it','education-and-training'], usageLabels:['academic','analysis','written','advanced'], contextLabels:[],
    grammarNotes:['feminine noun; used when comparing systems, texts, data, or institutions'],
    collocations:[{text:'Strukturähnlichkeiten erkennen', meaning:'to identify structural similarities'}],
    wordFamilies:[{lemma:'strukturähnlich', relationLabel:'adjective', note:null},{lemma:'die Struktur', relationLabel:'noun', note:null}], relations:[],
    meanings: arr({ar:'تشابه بنيوي',ckb:'لێکچوونی پێکهاتەیی',en:'structural similarity',fa:'شباهت ساختاری',kmr:'wekheviya avahiyî',pl:'podobieństwo strukturalne',ro:'asemănare structurală',ru:'структурное сходство',sq:'ngjashmëri strukturore',tr:'yapısal benzerlik'}),
    examples:[
      ex('Die Strukturähnlichkeit der beiden Verträge erleichterte die juristische Prüfung erheblich.',{ar:'سهّل التشابه البنيوي بين العقدين الفحص القانوني بشكل كبير.',ckb:'لێکچوونی پێکهاتەییی هەردوو گرێبەستەکە پشکنینی یاسایی زۆر ئاسانتر کرد.',en:'The structural similarity of the two contracts made the legal review much easier.',fa:'شباهت ساختاری دو قرارداد بررسی حقوقی را بسیار آسان‌تر کرد.',kmr:'Wekheviya avahiyî ya her du peymanan vekolîna hiqûqî gelek hêsantir kir.',pl:'Podobieństwo strukturalne obu umów znacznie ułatwiło analizę prawną.',ro:'Asemănarea structurală a celor două contracte a facilitat considerabil verificarea juridică.',ru:'Структурное сходство двух договоров значительно упростило юридическую проверку.',sq:'Ngjashmëria strukturore e dy kontratave e lehtësoi ndjeshëm kontrollin juridik.',tr:'İki sözleşmenin yapısal benzerliği hukuki incelemeyi önemli ölçüde kolaylaştırdı.'}),
      ex('Beim Refactoring fiel auf, dass mehrere Module trotz unterschiedlicher Namen eine hohe Strukturähnlichkeit haben.',{ar:'أثناء إعادة هيكلة الكود لوحظ أن عدة وحدات، رغم اختلاف أسمائها، لديها تشابه بنيوي كبير.',ckb:'لە کاتی ڕیفاکتۆرکردندا دەرکەوت کە چەند مۆدیولێک سەرەڕای ناوی جیاواز، لێکچوونی پێکهاتەیی زۆریان هەیە.',en:'During refactoring, it became clear that several modules have a high structural similarity despite different names.',fa:'هنگام بازآرایی کد مشخص شد که چند ماژول با وجود نام‌های متفاوت، شباهت ساختاری زیادی دارند.',kmr:'Di dema refactoringê de xuya bû ku çend modul tevî navên cuda wekheviya avahiyî ya bilind hene.',pl:'Podczas refaktoryzacji zauważono, że kilka modułów mimo różnych nazw ma duże podobieństwo strukturalne.',ro:'În timpul refactorizării s-a observat că mai multe module au o mare asemănare structurală, deși poartă nume diferite.',ru:'Во время рефакторинга выяснилось, что несколько модулей, несмотря на разные названия, имеют сильное структурное сходство.',sq:'Gjatë refaktorimit u vu re se disa module, megjithëse kanë emra të ndryshëm, kanë ngjashmëri të lartë strukturore.',tr:'Refactoring sırasında, farklı adlara rağmen birkaç modülün yüksek yapısal benzerliğe sahip olduğu fark edildi.'})
    ]
  },
  {
    word:'strukturprägend', language:'de', cefrLevel:level, partOfSpeech:'Adjective', article:null, plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'struk-tur-prä-gend',
    topics:['advanced-analysis','management-and-leadership','public-services'], usageLabels:['formal','analysis','written','advanced'], contextLabels:[],
    grammarNotes:['adjective; describes something that shapes or determines a structure'],
    collocations:[{text:'eine strukturprägende Entscheidung', meaning:'a decision that shapes the structure of a system or organization'}],
    wordFamilies:[{lemma:'prägen', relationLabel:'verb', note:null},{lemma:'die Struktur', relationLabel:'noun', note:null}], relations:[],
    meanings: arr({ar:'مؤثر في تشكيل البنية',ckb:'ئەوەی پێکهاتە دادەڕێژێت',en:'structure-shaping; structurally defining',fa:'شکل‌دهنده ساختار؛ تعیین‌کننده ساختاری',kmr:'avahiyê çêdike an diyarker e',pl:'kształtujący strukturę',ro:'care modelează structura',ru:'формирующий структуру; структурообразующий',sq:'që formëson strukturën',tr:'yapıyı şekillendiren'}),
    examples:[
      ex('Die Zusammenlegung der Abteilungen war eine strukturprägende Entscheidung für das gesamte Unternehmen.',{ar:'كان دمج الأقسام قراراً شكّل بنية الشركة بأكملها.',ckb:'یەکخستنی بەشەکان بڕیارێکی پێکهاتەساز بوو بۆ تەواوی کۆمپانیاکە.',en:'The merger of the departments was a structure-shaping decision for the entire company.',fa:'ادغام بخش‌ها تصمیمی ساختارساز برای کل شرکت بود.',kmr:'Yekgirtina beşan biryareke avahî-şêwazker bû ji bo tevahiya şirketê.',pl:'Połączenie działów było decyzją kształtującą strukturę całej firmy.',ro:'Comasarea departamentelor a fost o decizie care a modelat structura întregii companii.',ru:'Объединение отделов стало структурообразующим решением для всей компании.',sq:'Bashkimi i departamenteve ishte një vendim që formësoi strukturën e gjithë kompanisë.',tr:'Departmanların birleştirilmesi tüm şirket için yapıyı şekillendiren bir karardı.'}),
      ex('Historisch war der Zugang zu Bildung für viele Regionen strukturprägend.',{ar:'تاريخياً كان الوصول إلى التعليم عاملاً مشكلاً للبنية في كثير من المناطق.',ckb:'لە ڕووی مێژووییەوە دەستگەیشتن بە پەروەردە بۆ زۆر ناوچە پێکهاتەساز بوو.',en:'Historically, access to education shaped the structure of many regions.',fa:'از نظر تاریخی، دسترسی به آموزش برای بسیاری از مناطق عاملی ساختارساز بود.',kmr:'Ji aliyê dîrokî ve gihiştina perwerdeyê ji bo gelek herêman avahî-şêwazker bû.',pl:'Historycznie dostęp do edukacji kształtował strukturę wielu regionów.',ro:'Istoric, accesul la educație a modelat structura multor regiuni.',ru:'Исторически доступ к образованию формировал структуру многих регионов.',sq:'Historikisht, qasja në arsim ka formësuar strukturën e shumë rajoneve.',tr:'Tarihsel olarak eğitime erişim birçok bölgenin yapısını şekillendirdi.'})
    ]
  },
  {
    word:'die Subjektivität', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'die', plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'Sub-jek-ti-vi-tät',
    topics:['advanced-analysis','culture-and-media','social-and-relationships'], usageLabels:['academic','analysis','written','advanced'], contextLabels:[],
    grammarNotes:['feminine noun; usually singular; opposite of objectivity'],
    collocations:[{text:'die eigene Subjektivität reflektieren', meaning:'to reflect on one’s own subjectivity'}],
    wordFamilies:[{lemma:'subjektiv', relationLabel:'adjective', note:null},{lemma:'das Subjekt', relationLabel:'noun', note:null}], relations:[],
    meanings: arr({ar:'ذاتية؛ منظور شخصي',ckb:'خۆییبوون؛ دیدی کەسی',en:'subjectivity; personal perspective',fa:'ذهنیت؛ نگاه شخصی',kmr:'subjektîvîtî; perspektîfa kesane',pl:'subiektywność; osobista perspektywa',ro:'subiectivitate; perspectivă personală',ru:'субъективность; личная перспектива',sq:'subjektivitet; këndvështrim personal',tr:'öznellik; kişisel bakış açısı'}),
    examples:[
      ex('Gute qualitative Forschung macht die eigene Subjektivität sichtbar, statt sie zu verschweigen.',{ar:'البحث النوعي الجيد يجعل الذاتية الخاصة بالباحث مرئية بدلاً من إخفائها.',ckb:'توێژینەوەی چۆنیەتیی باش خۆییبوونی توێژەر دەردەخات، نەک بیشارێتەوە.',en:'Good qualitative research makes one’s own subjectivity visible instead of concealing it.',fa:'پژوهش کیفی خوب ذهنیت پژوهشگر را آشکار می‌کند، نه اینکه آن را پنهان کند.',kmr:'Lêkolîna kalîteyî ya baş subjektîvîtiya xwe xuya dike, ne ku wê veşêre.',pl:'Dobre badania jakościowe ujawniają własną subiektywność, zamiast ją ukrywać.',ro:'O cercetare calitativă bună face vizibilă propria subiectivitate, în loc să o ascundă.',ru:'Хорошее качественное исследование показывает собственную субъективность, а не скрывает ее.',sq:'Kërkimi cilësor i mirë e bën të dukshëm subjektivitetin e vet, në vend që ta fshehë.',tr:'İyi nitel araştırma, kendi öznelliğini gizlemek yerine görünür kılar.'}),
      ex('Bei Leistungsbeurteilungen lässt sich Subjektivität nie vollständig ausschließen.',{ar:'في تقييمات الأداء لا يمكن استبعاد الذاتية بالكامل أبداً.',ckb:'لە هەڵسەنگاندنی کارایی دا هەرگیز ناتوانرێت خۆییبوون بە تەواوی لاببرێت.',en:'In performance evaluations, subjectivity can never be completely excluded.',fa:'در ارزیابی عملکرد، ذهنیت را هرگز نمی‌توان کاملاً حذف کرد.',kmr:'Di nirxandinên performansê de subjektîvîtî qet bi tevahî nayê rakirin.',pl:'W ocenach wyników pracy nigdy nie da się całkowicie wykluczyć subiektywności.',ro:'În evaluările de performanță, subiectivitatea nu poate fi niciodată exclusă complet.',ru:'В оценке эффективности невозможно полностью исключить субъективность.',sq:'Në vlerësimet e performancës, subjektiviteti nuk mund të përjashtohet kurrë plotësisht.',tr:'Performans değerlendirmelerinde öznellik hiçbir zaman tamamen dışlanamaz.'})
    ]
  },
  {
    word:'substanziell', language:'de', cefrLevel:level, partOfSpeech:'Adjective', article:null, plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'sub-stan-zi-ell',
    topics:['business-communication','finance-and-accounting','advanced-analysis'], usageLabels:['formal','business','analysis','written'], contextLabels:[],
    grammarNotes:['adjective; means considerable, essential, or materially significant depending on context'],
    collocations:[{text:'eine substanzielle Verbesserung', meaning:'a significant and meaningful improvement'}],
    wordFamilies:[{lemma:'die Substanz', relationLabel:'noun', note:null}], relations:[],
    meanings: arr({ar:'جوهري؛ كبير وذو أهمية',ckb:'بنەڕەتی؛ گرنگ و بەرچاو',en:'substantial; significant and meaningful',fa:'اساسی؛ قابل توجه و مهم',kmr:'bingehîn; girîng û berbiçav',pl:'istotny; znaczący',ro:'substanțial; semnificativ',ru:'существенный; значительный',sq:'thelbësor; i konsiderueshëm',tr:'esaslı; önemli ve kayda değer'}),
    examples:[
      ex('Ohne substanzielle Investitionen wird die Infrastruktur in wenigen Jahren an ihre Grenzen stoßen.',{ar:'من دون استثمارات جوهرية ستصل البنية التحتية إلى حدودها خلال سنوات قليلة.',ckb:'بێ وەبەرهێنانی بەرچاو ژێرخانەکە لە چەند ساڵی داهاتوودا دەگاتە سنووری خۆی.',en:'Without substantial investment, the infrastructure will reach its limits within a few years.',fa:'بدون سرمایه‌گذاری قابل توجه، زیرساخت در چند سال آینده به مرز ظرفیت خود می‌رسد.',kmr:'Bê veberhênanên girîng, binesazî di çend salên pêş de dê bigihêje sînorên xwe.',pl:'Bez istotnych inwestycji infrastruktura w ciągu kilku lat osiągnie swoje granice.',ro:'Fără investiții substanțiale, infrastructura își va atinge limitele în câțiva ani.',ru:'Без существенных инвестиций инфраструктура через несколько лет достигнет своих пределов.',sq:'Pa investime thelbësore, infrastruktura do të arrijë kufijtë e saj brenda pak vitesh.',tr:'Önemli yatırımlar olmadan altyapı birkaç yıl içinde sınırlarına ulaşacak.'}),
      ex('Der Kunde erwartet keine kosmetische Änderung, sondern eine substanzielle Verbesserung des Prozesses.',{ar:'لا يتوقع العميل تغييراً شكلياً، بل تحسيناً جوهرياً في العملية.',ckb:'کڕیار چاوەڕێی گۆڕانکارییەکی ڕووکەشی ناکات، بەڵکو چاکسازییەکی بنەڕەتی لە پرۆسەکەدا دەوێت.',en:'The customer does not expect a cosmetic change, but a substantial improvement in the process.',fa:'مشتری انتظار تغییر ظاهری ندارد، بلکه بهبود اساسی فرایند را می‌خواهد.',kmr:'Mişterî ne guhertineke rûxarî, lê baştirkirineke bingehîn a pêvajoyê hêvî dike.',pl:'Klient nie oczekuje kosmetycznej zmiany, lecz istotnego usprawnienia procesu.',ro:'Clientul nu așteaptă o schimbare cosmetică, ci o îmbunătățire substanțială a procesului.',ru:'Клиент ожидает не косметического изменения, а существенного улучшения процесса.',sq:'Klienti nuk pret një ndryshim kozmetik, por një përmirësim thelbësor të procesit.',tr:'Müşteri yüzeysel bir değişiklik değil, süreçte esaslı bir iyileştirme bekliyor.'})
    ]
  }
];

const pkg = { packageVersion:'1.0', packageId:`de-${levelLower}-generated-batch-${batch}`, packageName:`German ${level} Generated Batch ${batch}`, source:'Hybrid', defaultMeaningLanguages:languages, labels, entries, collections:[], scenarios:[], conversationStarterPacks:[], eventPreparationPacks:[] };
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');
const result = cp.spawnSync(`dotnet run --project "${importProject}" -- --target shared --yes "${outPath}"`, { shell:true, cwd:root, encoding:'utf8', maxBuffer:1024*1024*10 });
const output = `${result.stdout || ''}${result.stderr || ''}`;
const imported = (output.match(/Entries imported:\s*(\d+)/) || [])[1];
const invalid = (output.match(/Entries invalid:\s*(\d+)/) || [])[1];
const warnings = (output.match(/Warnings:\s*(\d+)/) || [])[1];
const ok = result.status === 0 && imported === '6' && invalid === '0' && warnings === '0';
let removed = false;
if (ok) {
  const remaining = tokens.filter(t => !expected.includes(t));
  if (tokens.length - remaining.length !== expected.length) throw new Error('Exact delete count mismatch');
  fs.writeFileSync(sourcePath, remaining.join(', '), 'utf8');
  removed = true;
}
const finalTokens = fs.readFileSync(sourcePath, 'utf8').split(',').map(s => s.trim()).filter(Boolean);
console.log(JSON.stringify({ sourcePath, words, outPath, status:result.status, imported, invalid, warnings, removed, remainingCount:finalTokens.length, first10:finalTokens.slice(0,10), importOutputTail:output.split(/\r?\n/).filter(l => /Entries imported|Entries invalid|Warnings|Import completed|Import failed/i.test(l)).slice(-10) }, null, 2));
process.exit(ok ? 0 : 2);

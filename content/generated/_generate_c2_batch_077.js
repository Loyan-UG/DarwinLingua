const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '077';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['die Stilisierung','die Stimmigkeit','störrisch','subtil','die Subversion','subvertieren'];

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
    word: 'die Stilisierung', partOfSpeech: 'Noun', article: 'die', plural: 'Stilisierungen', syllableBreak: 'Sti-li-sie-rung',
    topics: ['culture-and-media','business-communication','advanced-analysis'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'eine bewusste Stilisierung', meaning: 'a deliberate stylization' }],
    meanings: meaning('أسلبة؛ تقديم بأسلوب معين','شێوازدان؛ ستایلکردن','stylization; deliberate shaping of style','سبک‌پردازی؛ جلوه‌دادن آگاهانه','stîlîzekirin; şêwazdayîn','stylizacja','stilizare','стилизация','stilizim','stilizasyon'),
    examples: [
      ex('Die Stilisierung des Produkts als „revolutionär“ wirkte überzogen, weil die Kernfunktionen kaum verändert worden waren.', meaning('بدت أسلبة المنتج بوصفه «ثورياً» مبالغاً فيها لأن الوظائف الأساسية بالكاد تغيرت.','شێوازدانی بەرهەمەکە وەک «شۆڕشگێڕ» زیادەڕەوی بوو، چونکە فەنکشنە سەرەکییەکان بە زەحمەت گۆڕابوون.','The stylization of the product as “revolutionary” seemed exaggerated because the core functions had barely changed.','سبک‌پردازی محصول به‌عنوان «انقلابی» اغراق‌آمیز به نظر می‌رسید، چون قابلیت‌های اصلی تقریباً تغییر نکرده بودند.','Stîlîzekirina berhemê wek “şoreşger” zêde xuya kir, ji ber ku fonksiyonên bingehîn hema neguherîbûn.','Stylizacja produktu na „rewolucyjny” wydawała się przesadzona, ponieważ funkcje podstawowe prawie się nie zmieniły.','Stilizarea produsului ca „revoluționar” părea exagerată, deoarece funcțiile de bază abia se schimbaseră.','Стилизация продукта как «революционного» выглядела преувеличенной, потому что основные функции почти не изменились.','Stilizimi i produktit si “revolucionar” dukej i tepruar, sepse funksionet kryesore mezi kishin ndryshuar.','Ürünün “devrimsel” olarak stilize edilmesi abartılı görünüyordu, çünkü temel işlevler neredeyse hiç değişmemişti.')),
      ex('Die Stilisierung der Hauptfigur zur Märtyrerin wird im letzten Kapitel bewusst gebrochen.', meaning('تُكسر في الفصل الأخير عمداً أسلبة الشخصية الرئيسية كشهيدة.','لە بەشی کۆتاییدا شێوازدانی کارەکتەری سەرەکی وەک شەهید بە ئەنقەست دەشکێندرێت.','The stylization of the main character as a martyr is deliberately broken in the final chapter.','سبک‌پردازی شخصیت اصلی به‌عنوان شهید در فصل آخر عمداً شکسته می‌شود.','Stîlîzekirina kesayeta sereke wek şehîd di beşa dawî de bi mebest tê şikandin.','Stylizacja głównej postaci na męczennicę zostaje w ostatnim rozdziale celowo przełamana.','Stilizarea personajului principal ca martiră este ruptă deliberat în ultimul capitol.','Стилизация главной героини как мученицы в последней главе намеренно ломается.','Stilizimi i personazhit kryesor si martire thyhet qëllimisht në kapitullin e fundit.','Ana karakterin şehit olarak stilize edilmesi son bölümde bilinçli olarak kırılır.'))
    ]
  }),
  entry({
    word: 'die Stimmigkeit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Stim-mig-keit',
    topics: ['quality-and-risk','advanced-analysis','business-communication'], usageLabels: ['formal','written','analysis','business'], grammarNotes: ['feminine abstract noun; normally singular'],
    collocations: [{ text: 'die innere Stimmigkeit prüfen', meaning: 'to check internal coherence' }],
    meanings: meaning('اتساق؛ انسجام داخلي','گونجاوی؛ هاوئاهەنگیی ناوخۆیی','coherence; internal consistency','انسجام؛ هماهنگی درونی','hevahengî; guncawiya hundirîn','spójność; trafność','coerență; potrivire internă','согласованность; внутренняя цельность','koherencë; përputhje e brendshme','tutarlılık; iç uyum'),
    examples: [
      ex('Vor der Freigabe prüfte das Team die Stimmigkeit zwischen Roadmap, Budget und verfügbaren Kapazitäten.', meaning('قبل الموافقة فحص الفريق الاتساق بين خارطة الطريق والميزانية والقدرات المتاحة.','پێش ڕێپێدان، تیمەکە گونجاوی نێوان ڕۆدمەپ، بودجە و توانای بەردەستی پشکنین کرد.','Before approval, the team checked the coherence between roadmap, budget, and available capacity.','پیش از تأیید، تیم انسجام میان نقشه راه، بودجه و ظرفیت موجود را بررسی کرد.','Berî pejirandinê, tîmê hevahengiya navbera roadmap, budce û kapasîteya berdest kontrol kir.','Przed zatwierdzeniem zespół sprawdził spójność między roadmapą, budżetem i dostępnymi zasobami.','Înainte de aprobare, echipa a verificat coerența dintre roadmap, buget și capacitățile disponibile.','Перед утверждением команда проверила согласованность roadmap, бюджета и доступных мощностей.','Para miratimit, ekipi kontrolloi koherencën mes roadmap-it, buxhetit dhe kapaciteteve të disponueshme.','Onaydan önce ekip roadmap, bütçe ve mevcut kapasite arasındaki tutarlılığı kontrol etti.')),
      ex('Die Stimmigkeit des Romans entsteht weniger aus der Handlung als aus der konsequenten Perspektive der Erzählerin.', meaning('ينشأ انسجام الرواية من منظور الراوية المتسق أكثر مما ينشأ من الحبكة.','گونجاوی ڕۆمانەکە کەمتر لە ڕووداوەکانەوە، زیاتر لە دیدگای بەردەوامی گێڕەرەوەکەوە دروست دەبێت.','The coherence of the novel comes less from the plot than from the narrator’s consistent perspective.','انسجام رمان کمتر از داستان و بیشتر از دیدگاه پیوسته راوی ناشی می‌شود.','Hevahengiya romanê kêmtir ji çîrokê, zêdetir ji perspektîfa domdar a vegêrê çêdibe.','Spójność powieści wynika mniej z fabuły niż z konsekwentnej perspektywy narratorki.','Coerența romanului provine mai puțin din acțiune și mai mult din perspectiva consecventă a naratoarei.','Цельность романа возникает не столько из сюжета, сколько из последовательной перспективы рассказчицы.','Koherenca e romanit vjen më pak nga ngjarja dhe më shumë nga perspektiva e qëndrueshme e rrëfimtares.','Romanın tutarlılığı olay örgüsünden çok anlatıcının tutarlı bakış açısından doğar.'))
    ]
  }),
  entry({
    word: 'störrisch', partOfSpeech: 'Adjective', syllableBreak: 'stör-risch',
    topics: ['social-and-relationships','work-and-jobs','business-communication'], usageLabels: ['formal','written','sensitive','advanced'],
    collocations: [{ text: 'störrisch reagieren', meaning: 'to react stubbornly' }],
    meanings: meaning('عنيد؛ رافض للانقياد','سەرەڕۆ؛ گوێنەگر','stubborn; obstinate','لجباز؛ سرکش','serhişk; guhnedar','uparty; krnąbrny','încăpățânat; recalcitrant','упрямый; строптивый','kokëfortë; i pabindur','inatçı; dikbaşlı'),
    examples: [
      ex('Der Dienstleister reagierte störrisch auf jede Änderungsbitte, obwohl die Anforderungen vertraglich klar gedeckt waren.', meaning('كان مزود الخدمة يرد بعناد على كل طلب تغيير رغم أن المتطلبات كانت مغطاة بوضوح في العقد.','دابینکەرەکە بە سەرەڕۆیی وەڵامی هەر داواکارییەکی گۆڕانی دەدایەوە، هەرچەندە پێداویستییەکان بە ڕوونی لە گرێبەستدا داپۆشرابوون.','The service provider reacted stubbornly to every change request, although the requirements were clearly covered by the contract.','ارائه‌دهنده خدمات به هر درخواست تغییر لجبازانه واکنش نشان می‌داد، با اینکه الزامات به‌روشنی در قرارداد پوشش داده شده بود.','Dabînker li hember her daxwaza guherînê serhişk bersiv dida, herçend daxwazî bi zelalî di peymanê de hatibûn girtin.','Dostawca usług reagował uparcie na każdą prośbę o zmianę, choć wymagania były wyraźnie objęte umową.','Furnizorul de servicii reacționa încăpățânat la fiecare cerere de schimbare, deși cerințele erau clar acoperite de contract.','Поставщик услуг упрямо реагировал на каждый запрос изменений, хотя требования явно покрывались договором.','Ofruesi i shërbimit reagonte kokëfortë ndaj çdo kërkese ndryshimi, megjithëse kërkesat mbuloheshin qartë nga kontrata.','Hizmet sağlayıcı, gereksinimler sözleşmede açıkça kapsanmış olmasına rağmen her değişiklik isteğine inatçı tepki veriyordu.')),
      ex('Die Figur wirkt störrisch, doch ihr Schweigen schützt eine Verletzung, die niemand sehen will.', meaning('تبدو الشخصية عنيدة، لكن صمتها يحمي جرحاً لا يريد أحد رؤيته.','کارەکتەرەکە سەرەڕۆ دەردەکەوێت، بەڵام بێدەنگییەکەی بریندارییەک دەپارێزێت کە کەس نایەوێت بیبینێت.','The character seems stubborn, but her silence protects a wound no one wants to see.','شخصیت لجباز به نظر می‌رسد، اما سکوتش زخمی را حفظ می‌کند که هیچ‌کس نمی‌خواهد ببیند.','Kesayet serhişk xuya dike, lê bêdengiya wê birînek diparêze ku kes naxwaze bibîne.','Postać wydaje się uparta, lecz jej milczenie chroni ranę, której nikt nie chce zobaczyć.','Personajul pare încăpățânat, dar tăcerea lui protejează o rană pe care nimeni nu vrea să o vadă.','Персонаж кажется упрямым, но ее молчание защищает рану, которую никто не хочет видеть.','Personazhi duket kokëfortë, por heshtja e saj mbron një plagë që askush nuk do ta shohë.','Karakter inatçı görünür, ama sessizliği kimsenin görmek istemediği bir yarayı korur.'))
    ]
  }),
  entry({
    word: 'subtil', partOfSpeech: 'Adjective', syllableBreak: 'sub-til',
    topics: ['advanced-analysis','business-communication','culture-and-media'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'ein subtiler Unterschied', meaning: 'a subtle difference' }],
    meanings: meaning('دقيق خفي؛ لطيف المعنى','ورد و شاراوە؛ نازک','subtle; nuanced and delicate','ظریف؛ نامحسوس و دقیق','hûrgulî û nazik','subtelny','subtil; fin','тонкий; едва уловимый','i hollë; subtil','ince; nüanslı'),
    examples: [
      ex('Der Unterschied zwischen Zustimmung und bloßer Kenntnisnahme ist im Genehmigungsprozess subtil, aber rechtlich entscheidend.', meaning('الفرق بين الموافقة ومجرد الإحاطة في عملية الاعتماد دقيق لكنه حاسم قانونياً.','جیاوازی نێوان ڕەزامەندی و تەنها ئاگاداربوون لە پڕۆسەی پەسەندکردندا وردە، بەڵام لە ڕووی یاساییەوە چارەنووسسازە.','The difference between approval and mere acknowledgement is subtle in the approval process, but legally decisive.','تفاوت میان تأیید و صرفاً اطلاع در فرایند مجوزدهی ظریف است، اما از نظر حقوقی تعیین‌کننده.','Cudahî di navbera pejirandin û tenê agahdarbûnê de di pêvajoya pejirandinê de hûrgulî ye, lê ji aliyê yasayî ve biryardar e.','Różnica między zgodą a samym przyjęciem do wiadomości jest w procesie akceptacji subtelna, ale prawnie decydująca.','Diferența dintre aprobare și simpla luare la cunoștință este subtilă în procesul de autorizare, dar decisivă juridic.','Разница между одобрением и простым принятием к сведению в процессе согласования тонкая, но юридически решающая.','Dallimi mes miratimit dhe thjesht marrjes në dijeni është subtil në procesin e aprovimit, por juridikisht vendimtar.','Onay ile yalnızca bilgi edinme arasındaki fark onay sürecinde incedir, ancak hukuken belirleyicidir.')),
      ex('Der Film arbeitet mit subtilen Verschiebungen im Licht, um das Misstrauen zwischen den Figuren wachsen zu lassen.', meaning('يعمل الفيلم بتحولات ضوئية دقيقة كي يجعل انعدام الثقة بين الشخصيات ينمو.','فیلمەکە بە گواستنەوەی وردی ڕووناکی کاردەکات بۆ ئەوەی بێمتمانەیی نێوان کارەکتەرەکان گەشە بکات.','The film uses subtle shifts in light to let the mistrust between the characters grow.','فیلم با جابه‌جایی‌های ظریف نور کار می‌کند تا بی‌اعتمادی میان شخصیت‌ها رشد کند.','Fîlm bi guherînên hûrgulî yên ronahiyê kar dike da ku bêbawerîya navbera kesayetan mezin bibe.','Film operuje subtelnymi zmianami światła, aby narastała nieufność między postaciami.','Filmul lucrează cu schimbări subtile de lumină pentru a face să crească neîncrederea dintre personaje.','Фильм работает с тонкими смещениями света, чтобы недоверие между персонажами нарастало.','Filmi punon me zhvendosje subtile të dritës për të rritur mosbesimin mes personazheve.','Film, karakterler arasındaki güvensizliği büyütmek için ışıktaki ince değişimlerle çalışır.'))
    ]
  }),
  entry({
    word: 'die Subversion', partOfSpeech: 'Noun', article: 'die', plural: 'Subversionen', syllableBreak: 'Sub-ver-si-on',
    topics: ['culture-and-media','law-and-compliance','advanced-analysis'], usageLabels: ['formal','written','academic','sensitive'],
    collocations: [{ text: 'ästhetische Subversion', meaning: 'aesthetic subversion' }],
    meanings: meaning('تقويض؛ عمل تخريبي ضد نظام قائم','بنەڕەت لەژێرکردن؛ ژێرخستنی سیستەم','subversion; undermining of an established order','براندازی پنهان؛ زیرورو کردن نظم موجود','binpêkirin; hilweşandina rêzika heyî','subwersja','subversiune','субверсия; подрыв','subversion; minimin i rendit','yıkıcılık; düzeni altüst etme'),
    examples: [
      ex('Die Compliance-Abteilung unterschied klar zwischen legitimer Kritik und gezielter Subversion interner Kontrollmechanismen.', meaning('فرّق قسم الامتثال بوضوح بين النقد المشروع والتقويض المتعمد لآليات الرقابة الداخلية.','بەشی پابەندبوون بە ڕوونی جیاوازی کرد لە نێوان ڕەخنەی ڕەوا و ژێرخستنی ئامانجدارەوەی میکانیزمەکانی کۆنتڕۆڵی ناوخۆ.','The compliance department clearly distinguished between legitimate criticism and targeted subversion of internal control mechanisms.','بخش انطباق به‌روشنی میان نقد مشروع و براندازی هدفمند سازوکارهای کنترل داخلی فرق گذاشت.','Beşa compliance bi zelalî cudahî kir di navbera rexneya rewa û subversiyona armancdar a mekanîzmayên kontrola hundirîn de.','Dział compliance wyraźnie odróżnił uzasadnioną krytykę od celowej subwersji wewnętrznych mechanizmów kontroli.','Departamentul de conformitate a diferențiat clar între critica legitimă și subversiunea țintită a mecanismelor interne de control.','Отдел комплаенса четко различал легитимную критику и целенаправленный подрыв внутренних механизмов контроля.','Departamenti i përputhshmërisë dalloi qartë mes kritikës legjitime dhe subversionit të synuar të mekanizmave të brendshëm të kontrollit.','Uyum departmanı meşru eleştiri ile iç kontrol mekanizmalarının hedefli biçimde altını oyma arasında net ayrım yaptı.')),
      ex('Die Subversion des Märchens besteht darin, dass die vermeintliche Nebenfigur am Ende die Deutungshoheit gewinnt.', meaning('يكمن تقويض الحكاية في أن الشخصية الثانوية ظاهرياً تكتسب في النهاية سلطة التأويل.','ژێرخستنی چیرۆکەکە لەوەدایە کە کارەکتەری بە ڕواڵەت لاوەکی لە کۆتاییدا دەسەڵاتی لێکدانەوە بەدەست دەهێنێت.','The subversion of the fairy tale lies in the fact that the supposedly minor character gains interpretive authority at the end.','براندازی درونی افسانه در این است که شخصیت ظاهراً فرعی در پایان اختیار تفسیر را به دست می‌آورد.','Subversiyona çîrokê di wê de ye ku kesayeta xuya kêlekî di dawiyê de desthilata şîroveyê distîne.','Subwersja baśni polega na tym, że rzekomo drugoplanowa postać zyskuje na końcu władzę interpretacji.','Subversiunea basmului constă în faptul că personajul aparent secundar câștigă la final autoritatea interpretării.','Субверсия сказки состоит в том, что якобы второстепенный персонаж в конце получает право на интерпретацию.','Subversioni i përrallës qëndron në faktin se personazhi gjoja dytësor fiton në fund autoritetin interpretues.','Masalın altüst edilişi, sözde yan karakterin sonunda yorumlama yetkisini kazanmasında yatar.'))
    ]
  }),
  entry({
    word: 'subvertieren', partOfSpeech: 'Verb', infinitive: 'subvertieren', syllableBreak: 'sub-ver-tie-ren',
    topics: ['culture-and-media','law-and-compliance','advanced-analysis'], usageLabels: ['formal','written','academic','sensitive'],
    collocations: [{ text: 'eine Ordnung subvertieren', meaning: 'to subvert an order' }],
    meanings: meaning('يقوّض؛ يزعزع من الداخل','لەژێرەوە تێکدان؛ بنەما ژێرخستن','to subvert; to undermine from within','زیرورو کردن؛ از درون تضعیف کردن','ji hundir ve hilweşandin; subvert kirin','subwertować; podważać','a submina; a subverti','подрывать; субвертировать','minoj; subvertoj','altını oymak; subvert etmek'),
    examples: [
      ex('Ein inoffizieller Freigabeweg subvertiert jede Auditierbarkeit, selbst wenn er kurzfristig Geschwindigkeit verspricht.', meaning('يقوّض مسار موافقة غير رسمي كل قابلية للتدقيق حتى لو وعد بسرعة قصيرة الأجل.','ڕێڕەوی پەسەندکردنی نافەرمی هەر auditabilityیەک لەژێرەوە تێکدەدات، تەنانەت ئەگەر بە خێرایی کورتخایەنیش بەڵێن بدات.','An unofficial approval path subverts any auditability, even if it promises short-term speed.','یک مسیر تأیید غیررسمی هرگونه قابلیت حسابرسی را زیرورو می‌کند، حتی اگر در کوتاه‌مدت وعده سرعت بدهد.','Rêya pejirandinê ya nefermî her auditability binpê dike, herçend leza demkurt soz bide.','Nieformalna ścieżka akceptacji podważa każdą audytowalność, nawet jeśli krótkoterminowo obiecuje szybkość.','O cale neoficială de aprobare subminează orice auditabilitate, chiar dacă promite viteză pe termen scurt.','Неофициальный путь согласования подрывает любую аудируемость, даже если обещает краткосрочную скорость.','Një rrugë jozyrtare miratimi minon çdo auditueshmëri, edhe nëse premton shpejtësi afatshkurtër.','Gayriresmi bir onay yolu, kısa vadede hız vaat etse bile her türlü denetlenebilirliğin altını oyar.')),
      ex('Die Erzählung subvertiert das Heldenmotiv, indem sie den Sieg als moralische Niederlage lesbar macht.', meaning('تقوّض السردية دافع البطل حين تجعل النصر قابلاً للقراءة كهزيمة أخلاقية.','گێڕانەوەکە مۆتیفی پاڵەوان لەژێرەوە تێکدەدات بەوەی سەرکەوتن وەک شکستێکی ئەخلاقی دەخوێندرێتەوە.','The narrative subverts the heroic motif by making victory readable as a moral defeat.','روایت موتیف قهرمان را زیرورو می‌کند، چون پیروزی را به‌صورت شکست اخلاقی خواندنی می‌سازد.','Vegotin motîfa lehengî subvert dike bi wê yekê ku serkeftin wek têkçûneke exlaqî tê xwendin.','Narracja subwertuje motyw bohatera, czyniąc zwycięstwo czytelnym jako moralną klęskę.','Narațiunea subminează motivul eroului făcând victoria lizibilă ca înfrângere morală.','Повествование подрывает героический мотив, делая победу читаемой как моральное поражение.','Rrëfimi subverton motivin e heroit duke e bërë fitoren të lexueshme si humbje morale.','Anlatı, zaferi ahlaki bir yenilgi olarak okunabilir kılarak kahramanlık motifini altüst eder.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 077', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

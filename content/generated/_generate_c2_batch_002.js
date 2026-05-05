const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '002';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const projectPath = path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj');

const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['abkehren','abliegen','die Abschweifung','abseitig','die Absonderung','das Abstraktum'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const labelMap = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const sourceText = fs.readFileSync(sourcePath, 'utf8');
const tokens = sourceText.split(',').map(t => t.trim()).filter(Boolean);
const words = tokens.slice(0, 6);
if (JSON.stringify(words) !== JSON.stringify(expected)) {
  throw new Error(`Unexpected first tokens: ${JSON.stringify(words)}`);
}

const usedLabels = ['formal','written','advanced','academic','analysis','business','administrative','sensitive'];
for (const key of usedLabels) if (!labelMap.has(key)) throw new Error(`Missing taxonomy label: ${key}`);

const labels = usedLabels.map(key => labelMap.get(key));
function meanings(obj) { return langs.map(language => ({ language, text: obj[language] })); }
function ex(baseText, obj) { return { baseText, translations: meanings(obj) }; }

const entries = [
  {
    word: 'abkehren', language: 'de', cefrLevel: level, partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'abkehren', pronunciationIpa: null, syllableBreak: 'ab-keh-ren',
    topics: ['advanced-analysis','management-and-leadership','social-and-relationships'], usageLabels: ['formal','written','advanced'], contextLabels: [],
    grammarNotes: ['separable verb; kehrt ab, kehrte ab, ist abgekehrt'],
    collocations: [{ text: 'sich von einer Position abkehren', meaning: 'to turn away from a position or attitude' }],
    wordFamilies: [{ lemma: 'die Abkehr', relationLabel: 'noun', note: null }], relations: [],
    meanings: meanings({ ar:'ينصرف عن؛ يتخلى عن', ckb:'پشت لە شتێک بکات؛ واز لە هەڵوێستێک بهێنێت', en:'to turn away from; to abandon', fa:'روی برگرداندن از؛ دست کشیدن از', kmr:'ji tiştekî vegerin; dev ji helwestekê berdan', pl:'odwrócić się od; porzucić', ro:'a se îndepărta de; a abandona', ru:'отвернуться от; отказаться от', sq:'të largohet nga; të braktisë', tr:'yüz çevirmek; terk etmek' }),
    examples: [
      ex('Die Organisation kehrte von ihrer früheren Wachstumslogik ab und setzte stärker auf Stabilität.', { ar:'ابتعدت المؤسسة عن منطق النمو السابق وركزت بشكل أكبر على الاستقرار.', ckb:'دامەزراوەکە لە لۆژیکی گەشەی پێشووی پشتیکرد و زیاتر گرنگی بە جێگیری دا.', en:'The organization turned away from its previous logic of growth and placed greater emphasis on stability.', fa:'سازمان از منطق رشد پیشین خود فاصله گرفت و بیشتر بر ثبات تمرکز کرد.', kmr:'Rêxistin ji mantiqa xwe ya berê ya mezinbûnê vegeriya û zêdetir girîngî da aramiyê.', pl:'Organizacja odeszła od wcześniejszej logiki wzrostu i położyła większy nacisk na stabilność.', ro:'Organizația s-a îndepărtat de logica sa anterioară de creștere și a pus mai mult accent pe stabilitate.', ru:'Организация отказалась от прежней логики роста и сделала больший упор на стабильность.', sq:'Organizata u largua nga logjika e mëparshme e rritjes dhe vuri më shumë theks te stabiliteti.', tr:'Kurum önceki büyüme mantığından uzaklaştı ve istikrara daha fazla ağırlık verdi.' }),
      ex('In der Debatte kehrte er bewusst von persönlichen Angriffen ab und sprach über strukturelle Ursachen.', { ar:'في النقاش ابتعد عمداً عن الهجمات الشخصية وتحدث عن الأسباب البنيوية.', ckb:'لە مشتومڕەکەدا بە ئاگاداری پشت لە هێرشە کەسییەکان کرد و باس لە هۆکارە پێکهاتەییەکان کرد.', en:'In the debate, he deliberately moved away from personal attacks and spoke about structural causes.', fa:'در مناظره آگاهانه از حملات شخصی فاصله گرفت و درباره علت‌های ساختاری صحبت کرد.', kmr:'Di nîqaşê de bi zanebûn ji êrîşên kesane dûr ket û li ser sedemên avahî axivî.', pl:'W debacie świadomie odszedł od osobistych ataków i mówił o przyczynach strukturalnych.', ro:'În dezbatere, s-a îndepărtat în mod deliberat de atacurile personale și a vorbit despre cauze structurale.', ru:'В ходе дискуссии он сознательно отказался от личных нападок и говорил о структурных причинах.', sq:'Në debat ai u largua me vetëdije nga sulmet personale dhe foli për shkaqe strukturore.', tr:'Tartışmada bilinçli olarak kişisel saldırılardan uzaklaştı ve yapısal nedenlerden söz etti.' })
    ]
  },
  {
    word: 'abliegen', language: 'de', cefrLevel: level, partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'abliegen', pronunciationIpa: null, syllableBreak: 'ab-lie-gen',
    topics: ['documents-and-administration','technology-and-it','work-and-jobs'], usageLabels: ['formal','written','administrative'], contextLabels: [],
    grammarNotes: ['intransitive verb; often used for documents or items stored at a particular place'],
    collocations: [{ text: 'bei einer Stelle abliegen', meaning: 'to be deposited or kept at an office or department' }],
    wordFamilies: [{ lemma: 'die Ablage', relationLabel: 'noun', note: 'related storage concept' }], relations: [],
    meanings: meanings({ ar:'يكون محفوظاً أو مودعاً في مكان ما', ckb:'لە شوێنێک پارێزراو یان دانراو بێت', en:'to be deposited or kept somewhere', fa:'در جایی نگهداری یا بایگانی شدن', kmr:'li cihêkî were parastin an danîn', pl:'znajdować się w depozycie lub być przechowywanym gdzieś', ro:'a se afla depus sau păstrat undeva', ru:'храниться или находиться где-либо', sq:'të jetë i depozituar ose i ruajtur diku', tr:'bir yerde muhafaza edilmek veya kayıtlı bulunmak' }),
    examples: [
      ex('Die Originalverträge liegen bei der Rechtsabteilung ab und dürfen nicht in Projektordner kopiert werden.', { ar:'العقود الأصلية محفوظة لدى القسم القانوني ولا يجوز نسخها إلى مجلدات المشاريع.', ckb:'گرێبەستە ڕەسەنەکان لای بەشی یاسایی پارێزراون و نابێت بۆ فۆڵدەری پڕۆژەکان کۆپی بکرێن.', en:'The original contracts are kept by the legal department and must not be copied into project folders.', fa:'قراردادهای اصلی نزد بخش حقوقی نگهداری می‌شوند و نباید در پوشه‌های پروژه کپی شوند.', kmr:'Peymanên orîjînal li beşa hiqûqê tên parastin û nabe ku li peldankên projeyê bên kopîkirin.', pl:'Oryginały umów są przechowywane w dziale prawnym i nie wolno ich kopiować do folderów projektowych.', ro:'Contractele originale se află la departamentul juridic și nu trebuie copiate în dosarele de proiect.', ru:'Оригиналы договоров хранятся в юридическом отделе, и их нельзя копировать в проектные папки.', sq:'Kontratat origjinale ruhen te departamenti juridik dhe nuk duhet të kopjohen në dosjet e projekteve.', tr:'Orijinal sözleşmeler hukuk departmanında tutulur ve proje klasörlerine kopyalanmamalıdır.' }),
      ex('Mehrere alte Protokolle lagen noch auf einem nicht indexierten Laufwerk ab, sodass die Suche sie nicht fand.', { ar:'كانت عدة محاضر قديمة لا تزال محفوظة على قرص غير مفهرس، لذلك لم يعثر عليها البحث.', ckb:'چەند تۆماری کۆنی کۆبوونەوە هێشتا لە درایڤێکی ئیندێکس نەکراو پارێزرا بوون، بۆیە گەڕانەکە نەیدۆزینەوە.', en:'Several old minutes were still stored on a non-indexed drive, so the search did not find them.', fa:'چند صورت‌جلسه قدیمی هنوز روی یک درایو ایندکس‌نشده نگهداری می‌شدند، به همین دلیل جست‌وجو آن‌ها را پیدا نکرد.', kmr:'Çend protokolên kevn hîn li ser ajokerek ne-indekskirî mabûn, ji ber vê lêgerînê ew nedîtin.', pl:'Kilka starych protokołów nadal znajdowało się na nieindeksowanym dysku, więc wyszukiwarka ich nie znalazła.', ro:'Mai multe procese-verbale vechi se aflau încă pe o unitate neindexată, astfel că funcția de căutare nu le-a găsit.', ru:'Несколько старых протоколов всё ещё хранились на неиндексированном диске, поэтому поиск их не нашёл.', sq:'Disa procesverbale të vjetra ishin ende në një disk të paindeksuar, prandaj kërkimi nuk i gjeti.', tr:'Birkaç eski tutanak hâlâ indekslenmemiş bir sürücüde duruyordu, bu yüzden arama onları bulamadı.' })
    ]
  },
  {
    word: 'die Abschweifung', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Abschweifungen', infinitive: null, pronunciationIpa: null, syllableBreak: 'Ab-schwei-fung',
    topics: ['business-communication','education-and-training','culture-and-media'], usageLabels: ['formal','written','academic'], contextLabels: [],
    grammarNotes: ['feminine noun'],
    collocations: [{ text: 'eine längere Abschweifung', meaning: 'a longer digression' }],
    wordFamilies: [{ lemma: 'abschweifen', relationLabel: 'verb', note: null }], relations: [],
    meanings: meanings({ ar:'استطراد؛ خروج عن الموضوع', ckb:'لابردن لە بابەت؛ قسەی لاوەکی', en:'digression; departure from the subject', fa:'حاشیه‌روی؛ خروج از موضوع', kmr:'ji mijarê derketin; dûrketina ji babetê', pl:'dygresja; odejście od tematu', ro:'digresiune; abatere de la subiect', ru:'отступление от темы; дигрессия', sq:'digresion; shmangie nga tema', tr:'konudan sapma; ara söz' }),
    examples: [
      ex('Die Abschweifung zur Unternehmensgeschichte war interessant, trug aber wenig zur Risikoanalyse bei.', { ar:'كان الاستطراد حول تاريخ الشركة مثيراً للاهتمام، لكنه لم يضف الكثير إلى تحليل المخاطر.', ckb:'لابردنەکە بۆ مێژووی کۆمپانیا سەرنجڕاکێش بوو، بەڵام زۆر یارمەتی شیکردنەوەی مەترسییەکان نەدا.', en:'The digression into the company’s history was interesting but contributed little to the risk analysis.', fa:'حاشیه‌روی درباره تاریخ شرکت جالب بود، اما کمک زیادی به تحلیل ریسک نکرد.', kmr:'Dûrketina li ser dîroka şirketê balkêş bû, lê kêm alîkarî da analîza xetereyê.', pl:'Dygresja o historii firmy była interesująca, ale niewiele wniosła do analizy ryzyka.', ro:'Digresiunea despre istoria companiei a fost interesantă, dar a contribuit puțin la analiza riscurilor.', ru:'Отступление об истории компании было интересным, но мало что добавило к анализу рисков.', sq:'Digresioni mbi historinë e kompanisë ishte interesant, por kontribuoi pak në analizën e rrezikut.', tr:'Şirket tarihine ilişkin konu dışı bölüm ilginçti, ancak risk analizine pek katkı sağlamadı.' }),
      ex('Der Essay lebt von kontrollierten Abschweifungen, die den Hauptgedanken später wieder aufnehmen.', { ar:'يعتمد المقال على استطرادات مضبوطة تعود لاحقاً إلى الفكرة الرئيسية.', ckb:'وتارەکە بە لابردنە کۆنترۆڵکراوەکان زیندووە کە دواتر بیرۆکەی سەرەکی دەگرنەوە.', en:'The essay thrives on controlled digressions that later return to the main idea.', fa:'این جستار بر حاشیه‌روی‌های کنترل‌شده‌ای استوار است که بعداً دوباره به ایده اصلی بازمی‌گردند.', kmr:'Gotar bi dûrketinên kontrolkirî dijî ku paşê dîsa vedigerin ser fikra sereke.', pl:'Esej opiera się na kontrolowanych dygresjach, które później wracają do głównej myśli.', ro:'Eseul trăiește prin digresiuni controlate, care reiau mai târziu ideea principală.', ru:'Эссе держится на контролируемых отступлениях, которые позже возвращаются к основной мысли.', sq:'Eseja mbështetet te digresione të kontrolluara që më vonë rikthehen te ideja kryesore.', tr:'Deneme, daha sonra ana düşünceye dönen kontrollü sapmalarla canlılık kazanır.' })
    ]
  },
  {
    word: 'abseitig', language: 'de', cefrLevel: level, partOfSpeech: 'Adjective', article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'ab-sei-tig',
    topics: ['advanced-analysis','culture-and-media','shopping-and-services'], usageLabels: ['formal','written','advanced'], contextLabels: [],
    grammarNotes: ['adjective; can describe something marginal, remote, or unusual'],
    collocations: [{ text: 'eine abseitige These', meaning: 'a marginal or unusual thesis' }],
    wordFamilies: [{ lemma: 'die Abseite', relationLabel: 'noun', note: 'rare related form' }], relations: [],
    meanings: meanings({ ar:'هامشي؛ بعيد؛ غير مألوف', ckb:'پەراوێزی؛ دوور؛ نامۆ', en:'marginal; remote; unusual', fa:'حاشیه‌ای؛ دورافتاده؛ نامتعارف', kmr:'peravî; dûr; neasayî', pl:'marginalny; oddalony; nietypowy', ro:'marginal; izolat; neobișnuit', ru:'периферийный; отдалённый; необычный', sq:'anësor; i largët; i pazakontë', tr:'marjinal; uzak; alışılmadık' }),
    examples: [
      ex('Die These wirkt zunächst abseitig, lässt sich aber mit den Daten überraschend gut begründen.', { ar:'تبدو الأطروحة في البداية هامشية، لكنها يمكن تبريرها بالبيانات بشكل مفاجئ الجيد.', ckb:'بۆچوونەکە سەرەتا پەراوێزی دەردەکەوێت، بەڵام بە داتاکان بە شێوەیەکی سەرسوڕهێنەر باش پاساو دەدرێت.', en:'At first the thesis seems marginal, but the data support it surprisingly well.', fa:'این تز در ابتدا حاشیه‌ای به نظر می‌رسد، اما با داده‌ها به شکل غافلگیرکننده‌ای خوب قابل توجیه است.', kmr:'Tez di destpêkê de peravî xuya dike, lê bi daneyan bi awayekî şaşwaz baş tê piştgirîkirin.', pl:'Teza wydaje się początkowo marginalna, ale dane zaskakująco dobrze ją uzasadniają.', ro:'Teza pare la început marginală, dar poate fi susținută surprinzător de bine prin date.', ru:'Сначала тезис кажется маргинальным, но данные удивительно хорошо его обосновывают.', sq:'Teza fillimisht duket anësore, por mund të mbështetet çuditërisht mirë me të dhënat.', tr:'Tez ilk bakışta marjinal görünür, ancak verilerle şaşırtıcı derecede iyi gerekçelendirilebilir.' }),
      ex('Der Standort ist für Kundenverkehr zu abseitig, auch wenn die Miete ungewöhnlich niedrig ist.', { ar:'الموقع بعيد جداً عن حركة العملاء، حتى لو كان الإيجار منخفضاً على نحو غير معتاد.', ckb:'شوێنەکە بۆ هاتوچۆی کڕیاران زۆر دوورە، هەرچەندە کرێکەی بە شێوەیەکی نامۆ نزمە.', en:'The location is too remote for customer traffic, even though the rent is unusually low.', fa:'این مکان برای رفت‌وآمد مشتریان بیش از حد دورافتاده است، هرچند اجاره آن به‌طور غیرمعمولی پایین است.', kmr:'Cih ji bo hatûçûna xerîdaran pir dûr e, herçendê kirêya wê neasayî kêm e.', pl:'Lokalizacja jest zbyt oddalona dla ruchu klientów, nawet jeśli czynsz jest wyjątkowo niski.', ro:'Locația este prea izolată pentru circulația clienților, chiar dacă chiria este neobișnuit de mică.', ru:'Место слишком удалённое для клиентского потока, даже если аренда необычно низкая.', sq:'Vendndodhja është shumë e largët për qarkullimin e klientëve, edhe pse qiraja është jashtëzakonisht e ulët.', tr:'Konum müşteri trafiği için fazla sapa, kira olağan dışı derecede düşük olsa bile.' })
    ]
  },
  {
    word: 'die Absonderung', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Absonderungen', infinitive: null, pronunciationIpa: null, syllableBreak: 'Ab-son-de-rung',
    topics: ['healthcare-and-appointments','social-and-relationships','advanced-analysis'], usageLabels: ['formal','written','advanced','sensitive'], contextLabels: [],
    grammarNotes: ['feminine noun; can mean isolation/separation or bodily secretion depending on context'],
    collocations: [{ text: 'die Absonderung infektiöser Personen', meaning: 'the isolation of infectious persons' }],
    wordFamilies: [{ lemma: 'absondern', relationLabel: 'verb', note: null }], relations: [],
    meanings: meanings({ ar:'عزل؛ فصل؛ إفراز', ckb:'جیاکردنەوە؛ گۆشەگیرکردن؛ ترشح', en:'isolation; separation; secretion', fa:'جداسازی؛ انزوا؛ ترشح', kmr:'veqetandin; îzolekirin; derxistin', pl:'izolacja; oddzielenie; wydzielina', ro:'izolare; separare; secreție', ru:'изоляция; отделение; выделение', sq:'izolim; ndarje; sekrecion', tr:'ayırma; izolasyon; salgı' }),
    examples: [
      ex('Die Absonderung infektiöser Patienten wurde nur angeordnet, wenn mildere Maßnahmen nicht ausreichten.', { ar:'لم يُؤمر بعزل المرضى المعديين إلا عندما لم تكن الإجراءات الأخف كافية.', ckb:'گۆشەگیرکردنی نەخۆشە تووشکەرەکان تەنها کاتێک فەرماندرا کە ڕێکارە نەرمتەرەکان بەس نەبوون.', en:'The isolation of infectious patients was ordered only when milder measures were insufficient.', fa:'جداسازی بیماران عفونی فقط زمانی دستور داده شد که اقدامات ملایم‌تر کافی نبودند.', kmr:'Îzolekirina nexweşên vegirtî tenê hingê hate biryardan ku tedbîrên siviktir têrê nekirin.', pl:'Izolację zakaźnych pacjentów zarządzano tylko wtedy, gdy łagodniejsze środki nie wystarczały.', ro:'Izolarea pacienților infecțioși a fost dispusă numai când măsurile mai blânde nu erau suficiente.', ru:'Изоляцию инфекционных пациентов назначали только тогда, когда более мягких мер было недостаточно.', sq:'Izolimi i pacientëve infektivë u urdhërua vetëm kur masat më të lehta nuk mjaftonin.', tr:'Bulaşıcı hastaların izolasyonu yalnızca daha hafif önlemler yeterli olmadığında emredildi.' }),
      ex('Soziale Absonderung kann entstehen, wenn ganze Gruppen dauerhaft als Risiko markiert werden.', { ar:'يمكن أن ينشأ العزل الاجتماعي عندما تُوسم جماعات كاملة باستمرار بأنها خطر.', ckb:'جیاکردنەوەی کۆمەڵایەتی دەتوانێت دروست ببێت کاتێک گرووپە تەواوەکان بەردەوام وەک مەترسی نیشان دەدرێن.', en:'Social isolation can arise when entire groups are persistently marked as a risk.', fa:'انزوای اجتماعی می‌تواند زمانی پدید آید که گروه‌های کامل به‌طور دائمی به‌عنوان خطر برچسب بخورند.', kmr:'Îzolekirina civakî dikare çêbibe dema ku komên tevahî bi domdarî wek xetere werin nîşankirin.', pl:'Izolacja społeczna może powstać, gdy całe grupy są trwale oznaczane jako ryzyko.', ro:'Izolarea socială poate apărea atunci când grupuri întregi sunt marcate permanent drept risc.', ru:'Социальная изоляция может возникнуть, когда целые группы постоянно обозначают как риск.', sq:'Izolimi social mund të lindë kur grupe të tëra shënohen vazhdimisht si rrezik.', tr:'Sosyal dışlanma, bütün gruplar sürekli olarak risk diye etiketlendiğinde ortaya çıkabilir.' })
    ]
  },
  {
    word: 'das Abstraktum', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'das', plural: 'Abstrakta', infinitive: null, pronunciationIpa: null, syllableBreak: 'Abs-trak-tum',
    topics: ['education-and-training','advanced-analysis','culture-and-media'], usageLabels: ['formal','written','academic','analysis'], contextLabels: [],
    grammarNotes: ['neuter noun; plural: Abstrakta'],
    collocations: [{ text: 'ein leeres Abstraktum', meaning: 'an empty abstraction' }],
    wordFamilies: [{ lemma: 'abstrakt', relationLabel: 'adjective', note: null }], relations: [],
    meanings: meanings({ ar:'مفهوم مجرد؛ اسم مجرد', ckb:'چەمکی ئەبستراکت؛ ناوی ئەبستراکت', en:'abstract concept; abstract noun', fa:'مفهوم انتزاعی؛ اسم انتزاعی', kmr:'têgeha abstrakt; navê abstrakt', pl:'pojęcie abstrakcyjne; rzeczownik abstrakcyjny', ro:'concept abstract; substantiv abstract', ru:'абстрактное понятие; абстрактное существительное', sq:'koncept abstrakt; emër abstrakt', tr:'soyut kavram; soyut ad' }),
    examples: [
      ex('Im Text wird Freiheit nicht als leeres Abstraktum behandelt, sondern an konkrete Rechte gebunden.', { ar:'في النص لا تُعامل الحرية كمفهوم مجرد فارغ، بل تُربط بحقوق ملموسة.', ckb:'لە دەقەکەدا ئازادی وەک چەمکێکی ئەبستراکتی بەتاڵ مامەڵەی لەگەڵ ناکرێت، بەڵکو بە مافە دیاریکراوەکانەوە دەبەسترێت.', en:'In the text, freedom is not treated as an empty abstraction but tied to concrete rights.', fa:'در متن، آزادی به‌عنوان مفهومی انتزاعی و توخالی扱 نمی‌شود، بلکه به حقوق مشخص پیوند می‌خورد.', kmr:'Di nivîsê de azadî wek têgehek vala ya abstrakt nayê nîqaşkirin, lê bi mafên konkret ve tê girêdan.', pl:'W tekście wolność nie jest traktowana jako pusta abstrakcja, lecz powiązana z konkretnymi prawami.', ro:'În text, libertatea nu este tratată ca o abstracțiune goală, ci legată de drepturi concrete.', ru:'В тексте свобода рассматривается не как пустая абстракция, а связывается с конкретными правами.', sq:'Në tekst, liria nuk trajtohet si abstraksion bosh, por lidhet me të drejta konkrete.', tr:'Metinde özgürlük boş bir soyutlama olarak ele alınmaz, somut haklarla ilişkilendirilir.' }),
      ex('Für viele Lernende ist „Gerechtigkeit“ zunächst ein schwer greifbares Abstraktum.', { ar:'بالنسبة إلى كثير من المتعلمين، تكون «العدالة» في البداية مفهوماً مجرداً يصعب الإمساك به.', ckb:'بۆ زۆر فێرخواز، «دادپەروەری» سەرەتا چەمکێکی ئەبستراکتی قورسە بۆ تێگەیشتن.', en:'For many learners, “justice” is initially an abstract concept that is difficult to grasp.', fa:'برای بسیاری از زبان‌آموزان، «عدالت» در ابتدا مفهومی انتزاعی و دشوار برای درک است.', kmr:'Ji bo gelek fêrkaran, “dadwerî” di destpêkê de têgehek abstrakt e ku girtina wê zehmet e.', pl:'Dla wielu uczących się „sprawiedliwość” jest początkowo trudnym do uchwycenia abstraktem.', ro:'Pentru mulți cursanți, „dreptatea” este la început un concept abstract greu de înțeles.', ru:'Для многих учащихся «справедливость» сначала является трудной для понимания абстракцией.', sq:'Për shumë nxënës, “drejtësia” fillimisht është një koncept abstrakt i vështirë për t’u kapur.', tr:'Birçok öğrenen için “adalet” başlangıçta kavraması zor bir soyut kavramdır.' })
    ]
  }
];

const pkg = {
  packageVersion: '1.0',
  packageId: `de-${levelLower}-generated-batch-${batch}`,
  packageName: `German ${level} Generated Batch ${batch}`,
  source: 'Hybrid',
  defaultMeaningLanguages: langs,
  labels,
  entries,
  collections: [],
  scenarios: [],
  conversationStarterPacks: [],
  eventPreparationPacks: []
};

fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');
let output = '';
try {
  output = execFileSync('dotnet', ['run', '--project', projectPath, '--', '--target', 'shared', '--yes', outPath], { cwd: root, encoding: 'utf8', stdio: ['ignore', 'pipe', 'pipe'] });
} catch (e) {
  output = `${e.stdout || ''}\n${e.stderr || ''}`;
  console.log(output);
  throw new Error('Import failed');
}
console.log(output);
const ok = output.includes('Entries imported: 6') && output.includes('Entries invalid: 0') && output.includes('Warnings: 0');
if (!ok) throw new Error('Import did not meet strict success criteria');

const currentTokens = fs.readFileSync(sourcePath, 'utf8').split(',').map(t => t.trim()).filter(Boolean);
const remaining = currentTokens.slice();
for (const w of expected) {
  const idx = remaining.indexOf(w);
  if (idx === -1) throw new Error(`Processed token not found for deletion: ${w}`);
  remaining.splice(idx, 1);
}
fs.writeFileSync(sourcePath, remaining.join(', '), 'utf8');
console.log('SOURCE_UPDATED: yes');
console.log('REMAINING_COUNT:', remaining.length);
console.log('FIRST_10:', remaining.slice(0, 10).join(' | '));
console.log('JSON_PATH:', outPath);

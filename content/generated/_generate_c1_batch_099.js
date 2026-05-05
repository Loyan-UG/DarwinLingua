const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C1';
const levelLower = 'c1';
const batch = '099';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const importProject = path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj');
const expected = ['die Steuerungsfähigkeit','die Steuerungslogik','steuerungsrelevant','die Stichprobengröße','die Stichprobenziehung','die Stigmatisierung'];
const languages = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const taxonomyLabels = taxonomy.labels || [];
const labelMap = new Map(taxonomyLabels.map(l => [l.key, l]));

const sourceText = fs.readFileSync(sourcePath, 'utf8');
const tokens = sourceText.split(',').map(s => s.trim()).filter(Boolean);
const words = tokens.slice(0, 6);
if (JSON.stringify(words) !== JSON.stringify(expected)) {
  throw new Error(`Unexpected first words: ${JSON.stringify(words)}`);
}

const usedLabels = ['business','analysis','written','advanced','academic','sensitive'];
const labels = usedLabels.map(k => {
  const label = labelMap.get(k);
  if (!label) throw new Error(`Missing label ${k}`);
  return label;
});

function meanings(obj) { return languages.map(language => ({ language, text: obj[language] })); }
function translations(obj) { return languages.map(language => ({ language, text: obj[language] })); }
function ex(baseText, tr) { return { baseText, translations: translations(tr) }; }

const entries = [
  {
    word: 'die Steuerungsfähigkeit', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Steuerungsfähigkeiten', infinitive: null, pronunciationIpa: null, syllableBreak: 'Steu-e-rungs-fä-hig-keit',
    topics: ['management-and-leadership','planning-and-projects','advanced-analysis'], usageLabels: ['business','analysis','written','advanced'], contextLabels: [],
    grammarNotes: ['feminine noun; often used in administration, governance, and management analysis'],
    collocations: [{ text: 'die Steuerungsfähigkeit stärken', meaning: 'to strengthen the ability to steer or govern a system' }],
    wordFamilies: [{ lemma: 'steuerungsfähig', relationLabel: 'adjective', note: null }, { lemma: 'steuern', relationLabel: 'verb', note: null }], relations: [],
    meanings: meanings({ ar:'القدرة على التوجيه أو التحكم', ckb:'توانای ڕێکخستن و ئاراستەکردن', en:'capacity to steer, control, or govern', fa:'توانایی هدایت یا کنترل', kmr:'hêza rêvebirin an kontrolkirinê', pl:'zdolność sterowania lub zarządzania', ro:'capacitate de coordonare sau control', ru:'способность управлять или координировать', sq:'aftësi për drejtim ose kontroll', tr:'yönlendirme veya kontrol etme kapasitesi' }),
    examples: [
      ex('Die Steuerungsfähigkeit der Organisation leidet, wenn Entscheidungen auf zu viele Gremien verteilt sind.', { ar:'تتضرر قدرة المنظمة على التوجيه عندما تتوزع القرارات على عدد كبير جداً من اللجان.', ckb:'توانای ئاراستەکردنی ڕێکخراوەکە لاواز دەبێت کاتێک بڕیارەکان لە نێوان زۆر کۆمیتە دابەش دەبن.', en:'The organization’s ability to steer suffers when decisions are spread across too many committees.', fa:'وقتی تصمیم‌ها میان کمیته‌های بسیار زیادی پخش می‌شود، توانایی هدایت سازمان آسیب می‌بیند.', kmr:'Dema biryar di nav gelek komîteyan de belav dibin, hêza rêvebirina rêxistinê lawaz dibe.', pl:'Zdolność organizacji do sterowania słabnie, gdy decyzje są rozproszone między zbyt wiele gremiów.', ro:'Capacitatea organizației de coordonare scade când deciziile sunt împărțite între prea multe comisii.', ru:'Способность организации управлять процессами снижается, когда решения распределены между слишком многими комитетами.', sq:'Aftësia drejtuese e organizatës dëmtohet kur vendimet shpërndahen në shumë komisione.', tr:'Kararlar çok fazla kurula dağıtıldığında kuruluşun yönlendirme kapasitesi zarar görür.' }),
      ex('In der Krise zeigte sich, wie begrenzt die Steuerungsfähigkeit der Verwaltung tatsächlich war.', { ar:'في الأزمة اتضح مدى محدودية قدرة الإدارة على التوجيه فعلاً.', ckb:'لە قەیرانەکەدا دەرکەوت کە توانای ڕێکخستنی کارگێڕی بەڕاستی چەند سنووردار بوو.', en:'During the crisis, it became clear how limited the administration’s actual capacity to steer was.', fa:'در بحران مشخص شد که توانایی واقعی اداره برای هدایت امور چقدر محدود بوده است.', kmr:'Di krîzê de xuya bû ku hêza rêvebirina rêveberiyê bi rastî çiqas sînorkirî bû.', pl:'W czasie kryzysu okazało się, jak ograniczona była rzeczywista zdolność administracji do sterowania.', ro:'În criză s-a văzut cât de limitată era de fapt capacitatea administrației de coordonare.', ru:'Во время кризиса стало ясно, насколько ограниченной на самом деле была управленческая способность администрации.', sq:'Gjatë krizës u pa sa e kufizuar ishte në të vërtetë aftësia drejtuese e administratës.', tr:'Kriz sırasında idarenin gerçek yönlendirme kapasitesinin ne kadar sınırlı olduğu ortaya çıktı.' })
    ]
  },
  {
    word: 'die Steuerungslogik', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Steuerungslogiken', infinitive: null, pronunciationIpa: null, syllableBreak: 'Steu-e-rungs-lo-gik',
    topics: ['management-and-leadership','advanced-analysis','documents-and-administration'], usageLabels: ['business','analysis','written','advanced'], contextLabels: [],
    grammarNotes: ['feminine noun; common in policy, administration, and management theory'],
    collocations: [{ text: 'einer Steuerungslogik folgen', meaning: 'to follow a specific logic of governance or control' }],
    wordFamilies: [{ lemma: 'Steuerung', relationLabel: 'noun', note: null }, { lemma: 'logisch', relationLabel: 'adjective', note: null }], relations: [],
    meanings: meanings({ ar:'منطق التوجيه أو التحكم', ckb:'لۆژیکی ڕێکخستن و ئاراستەکردن', en:'logic of steering, control, or governance', fa:'منطق هدایت یا کنترل', kmr:'mantiqa rêvebirin an kontrolê', pl:'logika sterowania lub zarządzania', ro:'logică de coordonare sau control', ru:'логика управления или координации', sq:'logjikë drejtimi ose kontrolli', tr:'yönlendirme veya kontrol mantığı' }),
    examples: [
      ex('Die neue Steuerungslogik setzt weniger auf Kontrolle und stärker auf messbare Ergebnisse.', { ar:'يعتمد منطق التوجيه الجديد بدرجة أقل على الرقابة وبدرجة أكبر على النتائج القابلة للقياس.', ckb:'لۆژیکی نوێی ڕێکخستن کەمتر پشت بە کۆنترۆڵ دەبەستێت و زیاتر گرنگی بە ئەنجامی پێوانەکراو دەدات.', en:'The new governance logic relies less on control and more on measurable results.', fa:'منطق جدید هدایت کمتر بر کنترل و بیشتر بر نتایج قابل اندازه‌گیری تکیه دارد.', kmr:'Mantiqa nû ya rêvebirinê kêmtir xwe dispêre kontrolê û zêdetir li ser encamên pîvandinbar disekine.', pl:'Nowa logika zarządzania opiera się mniej na kontroli, a bardziej na mierzalnych wynikach.', ro:'Noua logică de coordonare se bazează mai puțin pe control și mai mult pe rezultate măsurabile.', ru:'Новая логика управления меньше опирается на контроль и больше на измеримые результаты.', sq:'Logjika e re e drejtimit mbështetet më pak te kontrolli dhe më shumë te rezultatet e matshme.', tr:'Yeni yönetim mantığı kontrole daha az, ölçülebilir sonuçlara daha çok dayanıyor.' }),
      ex('Im Bildungsbereich verändert eine rein ökonomische Steuerungslogik auch die Sprache der Institutionen.', { ar:'في قطاع التعليم يغيّر منطق توجيه اقتصادي بحت حتى لغة المؤسسات.', ckb:'لە بواری پەروەردەدا لۆژیکێکی ڕێکخستنی تەواو ئابووری تەنانەت زمانی دامەزراوەکانیش دەگۆڕێت.', en:'In education, a purely economic governance logic also changes the language used by institutions.', fa:'در حوزه آموزش، منطق هدایت کاملاً اقتصادی حتی زبان نهادها را نیز تغییر می‌دهد.', kmr:'Di warê perwerdeyê de mantiqeke rêvebirina tenê aborî zimanê saziyan jî diguherîne.', pl:'W edukacji czysto ekonomiczna logika zarządzania zmienia także język instytucji.', ro:'În domeniul educației, o logică de coordonare pur economică schimbă și limbajul instituțiilor.', ru:'В сфере образования чисто экономическая логика управления меняет также язык учреждений.', sq:'Në arsim, një logjikë drejtimi thjesht ekonomike ndryshon edhe gjuhën e institucioneve.', tr:'Eğitim alanında tamamen ekonomik bir yönetim mantığı kurumların dilini de değiştirir.' })
    ]
  },
  {
    word: 'steuerungsrelevant', language: 'de', cefrLevel: level, partOfSpeech: 'Adjective', article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'steu-e-rungs-re-le-vant',
    topics: ['management-and-leadership','data-and-reporting','planning-and-projects'], usageLabels: ['business','analysis','written','advanced'], contextLabels: [],
    grammarNotes: ['adjective; used for information or factors relevant to steering decisions'],
    collocations: [{ text: 'steuerungsrelevante Kennzahlen', meaning: 'key figures relevant for management decisions' }],
    wordFamilies: [{ lemma: 'Steuerung', relationLabel: 'noun', note: null }, { lemma: 'relevant', relationLabel: 'adjective', note: null }], relations: [],
    meanings: meanings({ ar:'مهم لاتخاذ قرارات التوجيه أو الإدارة', ckb:'گرنگ بۆ بڕیاری ڕێکخستن و ئاراستەکردن', en:'relevant for steering, management, or control decisions', fa:'مرتبط و مهم برای تصمیم‌های هدایت یا مدیریت', kmr:'girîng ji bo biryarên rêvebirin an kontrolê', pl:'istotny dla decyzji zarządczych lub sterujących', ro:'relevant pentru decizii de coordonare sau control', ru:'важный для управленческих или координационных решений', sq:'i rëndësishëm për vendime drejtimi ose kontrolli', tr:'yönetim veya yönlendirme kararları için önemli' }),
    examples: [
      ex('Nur steuerungsrelevante Kennzahlen sollten im Dashboard prominent angezeigt werden.', { ar:'ينبغي عرض المؤشرات المهمة للإدارة فقط بشكل بارز في لوحة المعلومات.', ckb:'تەنها پێوەرە گرنگەکانی ڕێکخستن پێویستە بە شێوەیەکی دیار لە داشبۆردەکەدا پیشان بدرێن.', en:'Only management-relevant key figures should be displayed prominently on the dashboard.', fa:'فقط شاخص‌های مهم برای مدیریت باید در داشبورد به‌صورت برجسته نمایش داده شوند.', kmr:'Tenê hejmarên sereke yên ji bo rêvebirinê girîng divê di dashboardê de bi eşkereyî bêne nîşandan.', pl:'Na pulpicie powinny być wyraźnie pokazywane tylko wskaźniki istotne dla zarządzania.', ro:'În dashboard ar trebui afișați vizibil doar indicatorii relevanți pentru management.', ru:'На панели должны заметно отображаться только показатели, важные для управления.', sq:'Në panel duhet të shfaqen dukshëm vetëm treguesit e rëndësishëm për drejtimin.', tr:'Gösterge panelinde yalnızca yönetim açısından önemli göstergeler belirgin biçimde gösterilmelidir.' }),
      ex('Für die Stadtplanung sind nicht alle Beschwerden steuerungsrelevant, aber sie können Muster sichtbar machen.', { ar:'ليست كل الشكاوى مهمة مباشرة لتوجيه التخطيط الحضري، لكنها قد تكشف أنماطاً.', ckb:'بۆ پلاندانانی شار هەموو سکاڵاکان گرنگی ڕێکخستنیان نییە، بەڵام دەتوانن شێوەکان دیار بکەن.', en:'For urban planning, not every complaint is directly relevant for steering decisions, but they can reveal patterns.', fa:'برای برنامه‌ریزی شهری همه شکایت‌ها از نظر تصمیم‌گیری مدیریتی مهم نیستند، اما می‌توانند الگوهایی را نشان دهند.', kmr:'Ji bo plansaziya bajêr hemû gilî ne ji bo biryarên rêvebirinê girîng in, lê dikarin şêwazan xuya bikin.', pl:'Dla planowania miejskiego nie każda skarga jest istotna decyzyjnie, ale może ujawniać pewne wzorce.', ro:'Pentru planificarea urbană, nu toate reclamațiile sunt relevante pentru coordonare, dar pot face vizibile anumite tipare.', ru:'Для городского планирования не каждая жалоба важна для управленческих решений, но они могут выявлять закономерности.', sq:'Për planifikimin urban, jo çdo ankesë është e rëndësishme për drejtimin, por ato mund të tregojnë modele.', tr:'Şehir planlaması için her şikayet yönetim açısından belirleyici değildir, ancak bazı örüntüleri görünür kılabilir.' })
    ]
  },
  {
    word: 'die Stichprobengröße', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Stichprobengrößen', infinitive: null, pronunciationIpa: null, syllableBreak: 'Stich-pro-ben-grö-ße',
    topics: ['data-and-reporting','advanced-analysis','education-and-training'], usageLabels: ['academic','analysis','written','advanced'], contextLabels: [],
    grammarNotes: ['feminine noun; statistical term'],
    collocations: [{ text: 'eine ausreichende Stichprobengröße', meaning: 'a sufficient sample size' }],
    wordFamilies: [{ lemma: 'die Stichprobe', relationLabel: 'noun', note: null }, { lemma: 'groß', relationLabel: 'adjective', note: null }], relations: [],
    meanings: meanings({ ar:'حجم العينة', ckb:'قەبارەی نموونە', en:'sample size', fa:'اندازه نمونه', kmr:'mezinahiya nimûneyê', pl:'wielkość próby', ro:'dimensiunea eșantionului', ru:'размер выборки', sq:'madhësia e kampionit', tr:'örneklem büyüklüğü' }),
    examples: [
      ex('Die Stichprobengröße war zu klein, um belastbare Aussagen für einzelne Regionen zu treffen.', { ar:'كان حجم العينة صغيراً جداً بحيث لا يسمح باستخلاص نتائج موثوقة لكل منطقة على حدة.', ckb:'قەبارەی نموونەکە زۆر بچووک بوو بۆ ئەوەی دەرئەنجامی پشتپێبەستراو بۆ ناوچە جیاوازەکان بدرێت.', en:'The sample size was too small to make reliable statements for individual regions.', fa:'اندازه نمونه آن‌قدر کوچک بود که نمی‌شد برای مناطق جداگانه نتیجه‌های قابل اتکا گرفت.', kmr:'Mezinahiya nimûneyê pir biçûk bû ku meriv ji bo herêmên cuda daxuyaniyên bawerbar bide.', pl:'Wielkość próby była zbyt mała, aby formułować wiarygodne wnioski dla poszczególnych regionów.', ro:'Dimensiunea eșantionului a fost prea mică pentru a formula concluzii solide pentru regiuni separate.', ru:'Размер выборки был слишком мал, чтобы делать надежные выводы по отдельным регионам.', sq:'Madhësia e kampionit ishte shumë e vogël për të dhënë përfundime të besueshme për rajone të veçanta.', tr:'Örneklem büyüklüğü, tek tek bölgeler için güvenilir sonuçlar çıkarmak için çok küçüktü.' }),
      ex('Vor der Befragung wurde berechnet, welche Stichprobengröße für die gewünschte Genauigkeit nötig ist.', { ar:'قبل الاستطلاع حُسب حجم العينة اللازم للدقة المطلوبة.', ckb:'پێش ڕاپرسییەکە ژمێردرا کە چ قەبارەیەکی نموونە بۆ وردیی خوازراو پێویستە.', en:'Before the survey, the required sample size for the desired accuracy was calculated.', fa:'پیش از نظرسنجی محاسبه شد که برای دقت موردنظر چه اندازه نمونه‌ای لازم است.', kmr:'Berî lêpirsînê hate hesabkirin ka ji bo rastbûna xwestî çi mezinahiya nimûneyê pêwîst e.', pl:'Przed badaniem obliczono, jaka wielkość próby jest potrzebna dla oczekiwanej dokładności.', ro:'Înainte de sondaj s-a calculat ce dimensiune a eșantionului este necesară pentru precizia dorită.', ru:'Перед опросом рассчитали, какой размер выборки нужен для желаемой точности.', sq:'Para anketimit u llogarit se çfarë madhësie kampioni nevojitet për saktësinë e dëshiruar.', tr:'Anketten önce, istenen doğruluk için hangi örneklem büyüklüğünün gerekli olduğu hesaplandı.' })
    ]
  },
  {
    word: 'die Stichprobenziehung', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Stichprobenziehungen', infinitive: null, pronunciationIpa: null, syllableBreak: 'Stich-pro-ben-zie-hung',
    topics: ['data-and-reporting','advanced-analysis','education-and-training'], usageLabels: ['academic','analysis','written','advanced'], contextLabels: [],
    grammarNotes: ['feminine noun; statistical term for selecting a sample'],
    collocations: [{ text: 'die Stichprobenziehung dokumentieren', meaning: 'to document how the sample was selected' }],
    wordFamilies: [{ lemma: 'die Stichprobe', relationLabel: 'noun', note: null }, { lemma: 'ziehen', relationLabel: 'verb', note: null }], relations: [],
    meanings: meanings({ ar:'اختيار العينة', ckb:'هەڵبژاردنی نموونە', en:'sampling; selection of a sample', fa:'نمونه‌گیری؛ انتخاب نمونه', kmr:'hilbijartina nimûneyê', pl:'dobór próby', ro:'selectarea eșantionului', ru:'отбор выборки', sq:'përzgjedhja e kampionit', tr:'örneklem seçimi' }),
    examples: [
      ex('Die Stichprobenziehung wurde dokumentiert, damit mögliche Verzerrungen nachvollziehbar bleiben.', { ar:'وُثّق اختيار العينة كي تبقى الانحيازات المحتملة قابلة للتتبع.', ckb:'هەڵبژاردنی نموونەکە تۆمارکرا بۆ ئەوەی لایەنگرییە ئەگەرییەکان ڕوون و بەدواداچوون‌پێکراو بمێننەوە.', en:'The sampling process was documented so that possible biases remain traceable.', fa:'فرایند نمونه‌گیری مستند شد تا سوگیری‌های احتمالی قابل پیگیری بمانند.', kmr:'Hilbijartina nimûneyê hate belgekirin da ku alîgiriyên mumkin bên şopandin.', pl:'Dobór próby został udokumentowany, aby możliwe zniekształcenia pozostały możliwe do prześledzenia.', ro:'Selectarea eșantionului a fost documentată pentru ca posibilele distorsiuni să rămână trasabile.', ru:'Процесс отбора выборки был задокументирован, чтобы возможные искажения оставались отслеживаемыми.', sq:'Përzgjedhja e kampionit u dokumentua që shtrembërimet e mundshme të mbeten të gjurmueshme.', tr:'Olası sapmalar izlenebilir kalsın diye örneklem seçimi belgelendi.' }),
      ex('Bei der Mitarbeiterbefragung darf die Stichprobenziehung nicht nur die lautesten Teams erfassen.', { ar:'في استطلاع الموظفين لا يجوز أن يقتصر اختيار العينة على الفرق الأعلى صوتاً فقط.', ckb:'لە ڕاپرسی کارمەنداندا نابێت هەڵبژاردنی نموونە تەنها تیمە زۆر دەنگدارەکان بگرێتەوە.', en:'In the employee survey, the sampling must not include only the loudest teams.', fa:'در نظرسنجی کارکنان، نمونه‌گیری نباید فقط تیم‌هایی را دربر بگیرد که صدایشان بیشتر شنیده می‌شود.', kmr:'Di anketa karmendan de divê hilbijartina nimûneyê tenê tîmên herî dengdar negire.', pl:'W ankiecie pracowniczej dobór próby nie może obejmować tylko najgłośniejszych zespołów.', ro:'În sondajul angajaților, selectarea eșantionului nu trebuie să includă doar echipele cele mai vocale.', ru:'В опросе сотрудников отбор выборки не должен охватывать только самые громкие команды.', sq:'Në anketën e punonjësve, përzgjedhja e kampionit nuk duhet të përfshijë vetëm ekipet më të zëshme.', tr:'Çalışan anketinde örneklem seçimi yalnızca sesi en çok çıkan ekipleri kapsamamalıdır.' })
    ]
  },
  {
    word: 'die Stigmatisierung', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Stigmatisierungen', infinitive: null, pronunciationIpa: null, syllableBreak: 'Stig-ma-ti-sie-rung',
    topics: ['social-and-relationships','healthcare-and-appointments','advanced-analysis'], usageLabels: ['academic','analysis','written','sensitive'], contextLabels: [],
    grammarNotes: ['feminine noun; sensitive social and public-health term'],
    collocations: [{ text: 'Stigmatisierung vermeiden', meaning: 'to avoid stigmatization' }],
    wordFamilies: [{ lemma: 'stigmatisieren', relationLabel: 'verb', note: null }, { lemma: 'das Stigma', relationLabel: 'noun', note: null }], relations: [],
    meanings: meanings({ ar:'وصم؛ إلحاق عار اجتماعي', ckb:'نیشانەکردن بە شەرمەزاری؛ ستگماتیزەکردن', en:'stigmatization; social labeling as inferior or shameful', fa:'انگ‌زنی؛ بدنام‌سازی اجتماعی', kmr:'stigmatîzekirin; bi nîşana şermê nîşankirin', pl:'stygmatyzacja; społeczne piętnowanie', ro:'stigmatizare; etichetare socială negativă', ru:'стигматизация; социальное клеймение', sq:'stigmatizim; etiketim shoqëror negativ', tr:'damgalama; sosyal olarak olumsuz etiketleme' }),
    examples: [
      ex('Die Stigmatisierung psychischer Erkrankungen verhindert oft, dass Betroffene früh Hilfe suchen.', { ar:'غالباً ما يمنع وصم الأمراض النفسية المتضررين من طلب المساعدة مبكراً.', ckb:'ستگماتیزەکردنی نەخۆشییە دەروونییەکان زۆرجار ڕێگری دەکات لەوەی کە کەسانی تووشبوو زوو یارمەتی بخوازن.', en:'The stigmatization of mental illness often prevents affected people from seeking help early.', fa:'انگ‌زنی به بیماری‌های روانی اغلب باعث می‌شود افراد درگیر دیرتر کمک بگیرند.', kmr:'Stigmatîzekirina nexweşiyên derûnî gelek caran rê nade ku kesên bandorketî zû alîkariyê bixwazin.', pl:'Stygmatyzacja chorób psychicznych często powstrzymuje osoby dotknięte problemem przed wczesnym szukaniem pomocy.', ro:'Stigmatizarea bolilor psihice îi împiedică adesea pe cei afectați să caute ajutor din timp.', ru:'Стигматизация психических заболеваний часто мешает пострадавшим вовремя обращаться за помощью.', sq:'Stigmatizimi i sëmundjeve mendore shpesh i pengon personat e prekur të kërkojnë ndihmë herët.', tr:'Ruhsal hastalıkların damgalanması, etkilenen kişilerin erken yardım aramasını çoğu zaman engeller.' }),
      ex('In der öffentlichen Debatte kann Stigmatisierung entstehen, wenn ganze Gruppen pauschal als Risiko dargestellt werden.', { ar:'في النقاش العام قد ينشأ الوصم عندما تُعرض جماعات كاملة بشكل عام على أنها خطر.', ckb:'لە گفتوگۆی گشتیدا دەتوانێت ستگماتیزەکردن دروست ببێت کاتێک گرووپە تەواوەکان بە گشتی وەک مەترسی پیشان دەدرێن.', en:'In public debate, stigmatization can arise when entire groups are broadly portrayed as a risk.', fa:'در بحث عمومی، وقتی گروه‌های کامل به‌طور کلی به عنوان خطر معرفی می‌شوند، انگ‌زنی می‌تواند شکل بگیرد.', kmr:'Di gotûbêja giştî de stigmatîzekirin dikare çêbibe dema komên tevahî bi giştî wekî metirsî bên nîşandan.', pl:'W debacie publicznej stygmatyzacja może powstać, gdy całe grupy przedstawia się ogólnie jako ryzyko.', ro:'În dezbaterea publică, stigmatizarea poate apărea când grupuri întregi sunt prezentate în mod general drept un risc.', ru:'В публичной дискуссии стигматизация может возникать, когда целые группы обобщенно представляют как риск.', sq:'Në debatin publik, stigmatizimi mund të lindë kur grupe të tëra paraqiten përgjithësisht si rrezik.', tr:'Kamusal tartışmada, bütün gruplar genelleyici biçimde risk olarak gösterildiğinde damgalama ortaya çıkabilir.' })
    ]
  }
];

const pkg = {
  packageVersion: '1.0',
  packageId: `de-${levelLower}-generated-batch-${batch}`,
  packageName: `German ${level} Generated Batch ${batch}`,
  source: 'Hybrid',
  defaultMeaningLanguages: languages,
  labels,
  entries,
  collections: [],
  scenarios: [],
  conversationStarterPacks: [],
  eventPreparationPacks: []
};

fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');
const cmd = `dotnet run --project "${importProject}" -- --target shared --yes "${outPath}"`;
const result = cp.spawnSync(cmd, { shell: true, cwd: root, encoding: 'utf8', maxBuffer: 1024 * 1024 * 10 });
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
console.log(JSON.stringify({ sourcePath, words, outPath, status: result.status, imported, invalid, warnings, removed, remainingCount: finalTokens.length, first10: finalTokens.slice(0,10), importOutputTail: output.split(/\r?\n/).filter(l => /Entries imported|Entries invalid|Warnings|Import completed|Import failed/i.test(l)).slice(-10) }, null, 2));
process.exit(ok ? 0 : 2);

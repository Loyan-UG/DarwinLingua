const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C1';
const levelLower = 'c1';
const batch = '066';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outDir = path.join(root, 'content', 'generated');
const outPath = path.join(outDir, `de-${levelLower}-generated-batch-${batch}.json`);
const importProject = path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj');
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['klimmen','kneifen','die Kodierung','die Kognition','die Kohärenz','die Komplexität'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const source = fs.readFileSync(sourcePath, 'utf8');
const tokens = source.split(',').map(s => s.trim()).filter(Boolean);
const words = tokens.slice(0, 6);
if (JSON.stringify(words) !== JSON.stringify(expected)) throw new Error(`Unexpected first words: ${JSON.stringify(words)}`);

const labelKeys = ['formal','written','advanced','analysis','technical','academic','business','process'];
const labelMap = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const labels = labelKeys.map(k => { const l = labelMap.get(k); if (!l) throw new Error(`Missing label ${k}`); return l; });
function meanings(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) { return [ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr].map((text, i) => ({ language: langs[i], text })); }
function translations(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) { return meanings(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr); }
function ex(baseText, tr) { return { baseText, translations: tr }; }

const entries = [
  {
    word: 'klimmen', language: 'de', cefrLevel: level, partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'klimmen', pronunciationIpa: null, syllableBreak: 'klim-men',
    topics: ['everyday-life','transport-and-travel','culture-and-media'], usageLabels: ['formal','written','advanced'], contextLabels: [], grammarNotes: ['strong verb; elevated or literary synonym of steigen/klettern'],
    collocations: [{ text: 'auf einen Berg klimmen', meaning: 'to climb up a mountain' }], wordFamilies: [{ lemma: 'der Klimmzug', relationLabel: 'related noun', note: null }], relations: [],
    meanings: meanings('يتسلق؛ يصعد بصعوبة','سەر دەکەوێت؛ بە زەحمەت هەڵدەکشێت','to climb; to ascend with effort','بالا رفتن؛ با زحمت صعود کردن','hilkişîn; bi zehmetî derketin','wspinać się; piąć się','a se cățăra; a urca cu efort','взбираться; подниматься с усилием','të ngjitesh; të hipësh me mundim','tırmanmak; güçlükle yükselmek'),
    examples: [
      ex('Die Rettungskräfte mussten bei Dunkelheit über einen schmalen Steig zum Unfallort klimmen.', translations('اضطرت فرق الإنقاذ في الظلام إلى التسلق عبر ممر ضيق إلى مكان الحادث.','تیمەکانی فریاکەوتن لە تاریکیدا ناچار بوون بە ڕێڕەوێکی تەنگدا بۆ شوێنی ڕووداوەکە سەر بکەون.','In the dark, the rescue teams had to climb up a narrow path to the accident site.','نیروهای امداد در تاریکی مجبور شدند از یک مسیر باریک به محل حادثه بالا بروند.','Tîmên rizgarkirinê di tarîtiyê de neçar bûn bi rêyek teng ve heta cihê qezayê hilkişin.','Ratownicy musieli po ciemku wspiąć się wąską ścieżką na miejsce wypadku.','Echipele de salvare au trebuit să urce pe întuneric pe o potecă îngustă până la locul accidentului.','Спасателям пришлось в темноте взбираться по узкой тропе к месту аварии.','Ekipet e shpëtimit duhej të ngjiteshin në errësirë nëpër një shteg të ngushtë deri te vendi i aksidentit.','Kurtarma ekipleri karanlıkta dar bir patikadan kaza yerine tırmanmak zorunda kaldı.')),
      ex('Im alten Bericht heißt es, die Preise seien damals langsam, aber stetig nach oben geklommen.', translations('يذكر التقرير القديم أن الأسعار في ذلك الوقت صعدت ببطء ولكن بثبات.','لە ڕاپۆرتە کۆنەکەدا هاتووە کە نرخەکان ئەوکات بە هێواشی بەڵام بەردەوام بەرەو سەرەوە چوون.','The old report says that prices climbed slowly but steadily at that time.','در گزارش قدیمی آمده است که قیمت‌ها آن زمان آهسته اما پیوسته بالا رفتند.','Di rapora kevn de hatiye nivîsîn ku wê demê biha hêdî lê bi domdarî hilkişiyan.','W starym raporcie napisano, że ceny rosły wtedy powoli, ale stale.','În vechiul raport se spune că prețurile au urcat atunci încet, dar constant.','В старом отчете говорится, что цены тогда медленно, но неуклонно росли.','Në raportin e vjetër thuhet se çmimet atëherë u ngjitën ngadalë, por vazhdimisht.','Eski raporda fiyatların o dönemde yavaş ama istikrarlı biçimde yükseldiği yazıyor.'))
    ]
  },
  {
    word: 'kneifen', language: 'de', cefrLevel: level, partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'kneifen', pronunciationIpa: null, syllableBreak: 'knei-fen',
    topics: ['everyday-life','social-and-relationships','work-and-jobs'], usageLabels: ['formal','written','advanced'], contextLabels: [], grammarNotes: ['weak verb; can mean pinch or avoid something out of fear'],
    collocations: [{ text: 'vor einer Entscheidung kneifen', meaning: 'to back out of making a decision' }], wordFamilies: [{ lemma: 'der Kneifer', relationLabel: 'related noun', note: null }], relations: [],
    meanings: meanings('يقرص؛ يتهرب جبناً','پینچ دەکات؛ لە ترسدا خۆی لێ دەدزێتەوە','to pinch; to back out','نیشگون گرفتن؛ جا زدن','pinç kirin; paşve kişandin','szczypać; wycofywać się ze strachu','a ciupi; a da înapoi','щипать; струсить и отказаться','të pickosh; të tërhiqesh nga frika','çimdiklemek; korkup vazgeçmek'),
    examples: [
      ex('Kurz vor der Vertragsunterzeichnung durfte das Management nicht kneifen.', translations('قبل توقيع العقد بقليل لم يكن مسموحاً للإدارة أن تتراجع خوفاً.','کەمێک پێش واژۆکردنی گرێبەستەکە بەڕێوەبەرایەتی نەدەبوو خۆی لێ بدزێتەوە.','Shortly before signing the contract, management could not afford to back out.','کمی پیش از امضای قرارداد، مدیریت نباید جا می‌زد.','Demek kurt berî îmzekirina peymanê rêveberî nikaribû paşve bikeve.','Krótko przed podpisaniem umowy zarząd nie mógł się wycofać.','Cu puțin înainte de semnarea contractului, conducerea nu își putea permite să dea înapoi.','Незадолго до подписания договора руководство не могло струсить и отказаться.','Pak para nënshkrimit të kontratës, menaxhmenti nuk mund të tërhiqej.','Sözleşmenin imzalanmasından kısa süre önce yönetim geri adım atamazdı.')),
      ex('Das neue Hemd kneift am Kragen, deshalb trage ich es nicht den ganzen Tag.', translations('القميص الجديد يضغط عند الياقة، لذلك لا أرتديه طوال اليوم.','کراسە نوێیەکە لە ملی پینچ دەکات، بۆیە تەواوی ڕۆژ نایپۆشم.','The new shirt pinches at the collar, so I do not wear it all day.','پیراهن جدید دور یقه اذیت می‌کند، برای همین تمام روز آن را نمی‌پوشم.','Kirasê nû li stûyê min teng dike, ji ber vê yekê ez tevahiya rojê wî napoşim.','Nowa koszula uwiera przy kołnierzu, dlatego nie noszę jej przez cały dzień.','Cămașa nouă mă strânge la guler, de aceea nu o port toată ziua.','Новая рубашка жмет у воротника, поэтому я не ношу ее весь день.','Këmisha e re më shtrëngon te jaka, prandaj nuk e vesh gjithë ditën.','Yeni gömlek yakadan sıkıyor, bu yüzden onu bütün gün giymiyorum.'))
    ]
  },
  {
    word: 'die Kodierung', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Kodierungen', infinitive: null, pronunciationIpa: null, syllableBreak: 'Ko-die-rung',
    topics: ['technology-and-it','data-and-reporting','advanced-analysis'], usageLabels: ['technical','analysis','written','advanced'], contextLabels: [], grammarNotes: ['feminine noun'],
    collocations: [{ text: 'eine Kodierung prüfen', meaning: 'to check an encoding or coding scheme' }], wordFamilies: [{ lemma: 'kodieren', relationLabel: 'verb', note: null }, { lemma: 'der Code', relationLabel: 'related noun', note: null }], relations: [],
    meanings: meanings('ترميز؛ تشفير بنظام رموز','کۆدکردن؛ کۆدگەڕاندن','encoding; coding','کدگذاری؛ رمزگذاری','kodkirin; şîfrekirin','kodowanie','codificare; codare','кодирование','kodim; kodifikim','kodlama; kodlama biçimi'),
    examples: [
      ex('Die falsche Kodierung der Datei führte dazu, dass Umlaute im System nicht korrekt angezeigt wurden.', translations('أدى الترميز الخاطئ للملف إلى عدم عرض علامات الأوملاوت بشكل صحيح في النظام.','کۆدکردنی هەڵەی پەڕگەکە وای کرد نیشانەکانی ئۆملات لە سیستەمەکەدا بە دروستی پیشان نەدرێن.','The wrong file encoding caused umlauts to be displayed incorrectly in the system.','کدگذاری اشتباه فایل باعث شد اوملاوت‌ها در سیستم درست نمایش داده نشوند.','Kodkirina şaş a pelê bû sedem ku tîpên umlaut di pergalê de rast neyên nîşandan.','Błędne kodowanie pliku spowodowało, że umlauty nie były poprawnie wyświetlane w systemie.','Codarea greșită a fișierului a făcut ca umlauturile să nu fie afișate corect în sistem.','Неверная кодировка файла привела к тому, что умлауты отображались в системе неправильно.','Kodimi i gabuar i skedarit bëri që umlautet të mos shfaqeshin saktë në sistem.','Dosyanın yanlış kodlaması, umlaut karakterlerinin sistemde doğru görüntülenmemesine yol açtı.')),
      ex('Bei der qualitativen Studie erfolgte die Kodierung der Interviews durch zwei unabhängige Personen.', translations('في الدراسة النوعية تم ترميز المقابلات من قبل شخصين مستقلين.','لە توێژینەوەی چۆنیەتیدا کۆدکردنی چاوپێکەوتنەکان لەلایەن دوو کەسی سەربەخۆوە ئەنجامدرا.','In the qualitative study, the interviews were coded by two independent people.','در مطالعه کیفی، کدگذاری مصاحبه‌ها توسط دو فرد مستقل انجام شد.','Di lêkolîna çawanî de kodkirina hevpeyvînan ji aliyê du kesên serbixwe ve hat kirin.','W badaniu jakościowym kodowanie wywiadów przeprowadziły dwie niezależne osoby.','În studiul calitativ, codificarea interviurilor a fost realizată de două persoane independente.','В качественном исследовании кодирование интервью выполняли два независимых человека.','Në studimin cilësor, kodimi i intervistave u bë nga dy persona të pavarur.','Nitel çalışmada görüşmelerin kodlanması iki bağımsız kişi tarafından yapıldı.'))
    ]
  },
  {
    word: 'die Kognition', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Kognitionen', infinitive: null, pronunciationIpa: null, syllableBreak: 'Ko-gni-ti-on',
    topics: ['advanced-analysis','education-and-training','healthcare-and-appointments'], usageLabels: ['academic','analysis','written','advanced'], contextLabels: [], grammarNotes: ['feminine noun; often used in psychology and cognitive science'],
    collocations: [{ text: 'menschliche Kognition', meaning: 'human cognition' }], wordFamilies: [{ lemma: 'kognitiv', relationLabel: 'adjective', note: null }], relations: [],
    meanings: meanings('الإدراك؛ العمليات المعرفية','زانینی مێشک؛ پرۆسەکانی تێگەیشتن','cognition; cognitive processes','شناخت؛ فرایندهای شناختی','zanîna hişî; pêvajoyên têgihiştinê','poznanie; procesy poznawcze','cogniție; procese cognitive','когниция; познавательные процессы','kognicion; procese njohëse','biliş; bilişsel süreçler'),
    examples: [
      ex('Die Forschung zur Kognition zeigt, dass Entscheidungen häufig von unbewussten Erwartungen beeinflusst werden.', translations('تُظهر الأبحاث حول الإدراك أن القرارات كثيراً ما تتأثر بتوقعات غير واعية.','توێژینەوە لەسەر زانینی مێشک پیشان دەدات کە بڕیارەکان زۆرجار بە چاوەڕوانیی نائاگایانە کاریگەردەبن.','Research on cognition shows that decisions are often influenced by unconscious expectations.','پژوهش درباره شناخت نشان می‌دهد که تصمیم‌ها اغلب تحت تأثیر انتظارهای ناخودآگاه قرار می‌گیرند.','Lêkolîn li ser zanîna hişî nîşan dide ku biryar gelek caran ji hêviyên nehişyar bandor dibînin.','Badania nad poznaniem pokazują, że decyzje często są wpływane przez nieświadome oczekiwania.','Cercetarea asupra cogniției arată că deciziile sunt adesea influențate de așteptări inconștiente.','Исследования когниции показывают, что решения часто зависят от бессознательных ожиданий.','Kërkimet mbi kognicionin tregojnë se vendimet shpesh ndikohen nga pritshmëri të pavetëdijshme.','Biliş üzerine araştırmalar, kararların çoğu zaman bilinçdışı beklentilerden etkilendiğini gösterir.')),
      ex('In der Reha wird auch geprüft, wie stark Aufmerksamkeit und Kognition nach dem Unfall eingeschränkt sind.', translations('في إعادة التأهيل يتم أيضاً فحص مدى تقييد الانتباه والإدراك بعد الحادث.','لە چاکسازی تەندروستیدا هەروەها دەپشکنرێت سەرنج و زانینی مێشک دوای ڕووداوەکە تا چەند سنووردار بوون.','In rehabilitation, they also assess how much attention and cognition are impaired after the accident.','در توان‌بخشی همچنین بررسی می‌شود که توجه و شناخت پس از حادثه تا چه حد محدود شده‌اند.','Di rehabîlîtasyonê de her wiha tê kontrol kirin ka piştî qezayê baldarî û zanîna hişî çiqas kêm bûne.','W rehabilitacji sprawdza się także, jak bardzo po wypadku ograniczone są uwaga i funkcje poznawcze.','În reabilitare se verifică și cât de mult sunt afectate atenția și cogniția după accident.','Во время реабилитации также проверяют, насколько после аварии ограничены внимание и когнитивные функции.','Në rehabilitim kontrollohet edhe sa janë kufizuar vëmendja dhe kognicioni pas aksidentit.','Rehabilitasyonda kazadan sonra dikkat ve bilişin ne kadar kısıtlandığı da değerlendirilir.'))
    ]
  },
  {
    word: 'die Kohärenz', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Kohärenzen', infinitive: null, pronunciationIpa: null, syllableBreak: 'Ko-hä-renz',
    topics: ['advanced-analysis','documents-and-administration','business-communication'], usageLabels: ['formal','analysis','written','advanced'], contextLabels: [], grammarNotes: ['feminine noun'],
    collocations: [{ text: 'inhaltliche Kohärenz', meaning: 'conceptual or content-related coherence' }], wordFamilies: [{ lemma: 'kohärent', relationLabel: 'adjective', note: null }], relations: [],
    meanings: meanings('تماسك؛ ترابط منطقي','یەکگرتوویی؛ هاوئاهەنگی ناوەڕۆک','coherence; logical consistency','انسجام؛ پیوستگی منطقی','hevahengî; yekgirtina mentiqî','spójność; koherencja','coerență; consistență logică','связность; логическая согласованность','koherencë; qëndrueshmëri logjike','tutarlılık; bağdaşıklık'),
    examples: [
      ex('Dem Konzept fehlt noch die Kohärenz zwischen Zielbild, Budget und Zeitplan.', translations('لا يزال المفهوم يفتقر إلى التماسك بين الصورة المستهدفة والميزانية والجدول الزمني.','چەمکەکە هێشتا یەکگرتوویی نێوان ئامانجی داهاتوو، بودجە و خشتەی کاتیی نییە.','The concept still lacks coherence between the target vision, budget, and timeline.','این طرح هنوز میان تصویر هدف، بودجه و زمان‌بندی انسجام کافی ندارد.','Konseptê hîn hevahengiya di navbera armanca pêşerojê, budce û plansaziya demê de tune ye.','Koncepcji nadal brakuje spójności między wizją docelową, budżetem i harmonogramem.','Conceptului îi lipsește încă coerența dintre imaginea țintă, buget și calendar.','Концепции пока не хватает согласованности между целевым видением, бюджетом и графиком.','Konceptit i mungon ende koherenca midis vizionit të synuar, buxhetit dhe afatit kohor.','Konseptte hedef vizyon, bütçe ve zaman planı arasında hâlâ tutarlılık eksik.')),
      ex('Die Redaktion prüft die sprachliche Kohärenz des Textes, bevor er veröffentlicht wird.', translations('تراجع هيئة التحرير التماسك اللغوي للنص قبل نشره.','دەستەی نووسین هاوئاهەنگی زمانەوانی دەقەکە دەپشکنێت پێش ئەوەی بڵاو بکرێتەوە.','The editorial team checks the linguistic coherence of the text before it is published.','هیئت تحریریه پیش از انتشار، انسجام زبانی متن را بررسی می‌کند.','Desteya edîtoryal hevahengiya zimanî ya nivîsê kontrol dike berî ku were weşandin.','Redakcja sprawdza spójność językową tekstu przed jego publikacją.','Redacția verifică coerența lingvistică a textului înainte de publicare.','Редакция проверяет языковую связность текста перед публикацией.','Redaksia kontrollon koherencën gjuhësore të tekstit para publikimit.','Editör ekibi metin yayımlanmadan önce dilsel tutarlılığını kontrol eder.'))
    ]
  },
  {
    word: 'die Komplexität', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Komplexitäten', infinitive: null, pronunciationIpa: null, syllableBreak: 'Kom-ple-xi-tät',
    topics: ['advanced-analysis','planning-and-projects','technology-and-it'], usageLabels: ['formal','analysis','business','advanced'], contextLabels: [], grammarNotes: ['feminine noun'],
    collocations: [{ text: 'Komplexität reduzieren', meaning: 'to reduce complexity' }], wordFamilies: [{ lemma: 'komplex', relationLabel: 'adjective', note: null }], relations: [],
    meanings: meanings('تعقيد؛ درجة التعقيد','ئاڵۆزی؛ ئاستی ئاڵۆزی','complexity; degree of complexity','پیچیدگی؛ میزان پیچیدگی','tevlihevî; asta tevliheviyê','złożoność; stopień skomplikowania','complexitate; grad de complexitate','сложность; степень сложности','kompleksitet; shkallë ndërlikimi','karmaşıklık; karmaşıklık derecesi'),
    examples: [
      ex('Die Komplexität der Migration wurde unterschätzt, weil mehrere Altsysteme parallel angebunden werden mussten.', translations('تم التقليل من تعقيد عملية الترحيل لأن عدة أنظمة قديمة كان يجب ربطها بالتوازي.','ئاڵۆزی کۆچکردنەوەکە کەم هەژمار کرا، چونکە دەبوو چەند سیستەمی کۆن بە هاوکات ببەسترێنەوە.','The complexity of the migration was underestimated because several legacy systems had to be connected in parallel.','پیچیدگی مهاجرت سیستمی دست‌کم گرفته شد، چون چند سامانه قدیمی باید هم‌زمان متصل می‌شدند.','Tevliheviya koçberiya pergalê kêm hat nirxandin, ji ber ku diviyabû gelek pergalên kevn bi hevdemî werin girêdan.','Złożoność migracji została niedoszacowana, ponieważ kilka starych systemów trzeba było podłączyć równolegle.','Complexitatea migrării a fost subestimată, deoarece mai multe sisteme vechi trebuiau conectate în paralel.','Сложность миграции недооценили, потому что несколько устаревших систем нужно было подключать параллельно.','Kompleksiteti i migrimit u nënvlerësua, sepse disa sisteme të vjetra duhej të lidheshin paralelisht.','Geçişin karmaşıklığı hafife alındı, çünkü birkaç eski sistemin paralel olarak bağlanması gerekiyordu.')),
      ex('In der öffentlichen Debatte geht oft die soziale Komplexität des Problems verloren.', translations('في النقاش العام كثيراً ما تضيع التعقيدات الاجتماعية للمشكلة.','لە گفتوگۆی گشتیدا زۆرجار ئاڵۆزی کۆمەڵایەتیی کێشەکە ون دەبێت.','In the public debate, the social complexity of the problem is often lost.','در بحث عمومی، پیچیدگی اجتماعی مسئله اغلب نادیده گرفته می‌شود.','Di nîqaşa giştî de tevliheviya civakî ya pirsgirêkê gelek caran winda dibe.','W debacie publicznej często gubi się społeczna złożoność problemu.','În dezbaterea publică, complexitatea socială a problemei se pierde adesea.','В общественной дискуссии часто теряется социальная сложность проблемы.','Në debatin publik shpesh humbet kompleksiteti social i problemit.','Kamusal tartışmada sorunun toplumsal karmaşıklığı çoğu zaman kaybolur.'))
    ]
  }
];

const pkg = { packageVersion: '1.0', packageId: `de-${levelLower}-generated-batch-${batch}`, packageName: `German ${level} Generated Batch ${batch}`, source: 'Hybrid', defaultMeaningLanguages: langs, labels, entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');

const importCmd = `dotnet run --project "${importProject}" -- --target shared --yes "${outPath}"`;
let output = '', exitCode = 0;
try { output = cp.execSync(importCmd, { cwd: root, encoding: 'utf8', stdio: ['ignore','pipe','pipe'] }); }
catch (e) { exitCode = e.status || 1; output = `${e.stdout || ''}${e.stderr || ''}`; }
const success = exitCode === 0 && /Entries imported:\s*6\b/.test(output) && /Entries invalid:\s*0\b/.test(output) && /Warnings:\s*0\b/.test(output);
let deleted = false, remainingTokens = tokens;
if (success) {
  const remove = new Set(words); let removed = 0;
  remainingTokens = tokens.filter(t => { if (remove.has(t) && removed < words.length) { removed++; return false; } return true; });
  if (removed !== words.length) throw new Error(`Removed ${removed}, expected ${words.length}`);
  fs.writeFileSync(sourcePath, remainingTokens.join(', '), 'utf8');
  deleted = true;
}
console.log(JSON.stringify({ sourcePath, processedWords: words, jsonPath: outPath, importExitCode: exitCode, importSummary: (output.match(/Entries imported:\s*\d+|Entries invalid:\s*\d+|Warnings:\s*\d+/g) || []).join(' | '), deleted, remainingCount: remainingTokens.length, first10Remaining: remainingTokens.slice(0, 10) }, null, 2));
if (!success) process.exit(2);

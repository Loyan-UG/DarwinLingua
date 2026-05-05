const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '003';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const projectPath = path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj');
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['abverlangen','abwegig','die Ätiologie','der Affront','das Agens','die Ahnungslosigkeit'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const labelMap = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const sourceText = fs.readFileSync(sourcePath, 'utf8');
const tokens = sourceText.split(',').map(t => t.trim()).filter(Boolean);
const words = tokens.slice(0, 6);
if (JSON.stringify(words) !== JSON.stringify(expected)) throw new Error(`Unexpected first tokens: ${JSON.stringify(words)}`);

const usedLabels = ['formal','written','advanced','business','workplace','academic','analysis','sensitive','administrative'];
for (const key of usedLabels) if (!labelMap.has(key)) throw new Error(`Missing taxonomy label: ${key}`);
const labels = usedLabels.map(key => labelMap.get(key));
function meanings(obj) { return langs.map(language => ({ language, text: obj[language] })); }
function ex(baseText, obj) { return { baseText, translations: meanings(obj) }; }

const entries = [
  {
    word: 'abverlangen', language: 'de', cefrLevel: level, partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'abverlangen', pronunciationIpa: null, syllableBreak: 'ab-ver-lan-gen',
    topics: ['work-and-jobs','management-and-leadership','quality-and-risk'], usageLabels: ['formal','written','business','advanced'], contextLabels: [],
    grammarNotes: ['separable verb; verlangt ab, verlangte ab, hat abverlangt; typically used with dative person and accusative demand'],
    collocations: [{ text: 'jemandem viel abverlangen', meaning: 'to demand a great deal from someone' }],
    wordFamilies: [{ lemma: 'verlangen', relationLabel: 'verb', note: null }], relations: [],
    meanings: meanings({ ar:'يتطلب من شخص شيئاً كثيراً؛ يفرض عبئاً على', ckb:'شتێکی زۆر لە کەسێک داوا بکات؛ بارێکی قورس بخاتە سەر', en:'to demand or require something difficult from someone', fa:'چیزی دشوار از کسی طلبیدن؛ فشار زیادی وارد کردن', kmr:'tiştekî giran ji kesekî xwestin; barê zêde dan ser kesekî', pl:'wymagać od kogoś czegoś trudnego', ro:'a cere sau a solicita ceva dificil de la cineva', ru:'требовать от кого-либо чего-то трудного', sq:'t’i kërkosh dikujt diçka të vështirë', tr:'birinden zorlayıcı bir şey talep etmek' }),
    examples: [
      ex('Die Umstellung auf das neue ERP-System verlangte den Teams viel Geduld und Disziplin ab.', { ar:'تطلب الانتقال إلى نظام ERP الجديد كثيراً من الصبر والانضباط من الفرق.', ckb:'گۆڕان بۆ سیستەمی نوێی ERP زۆر ئارامی و ڕێکخراوی لە تیمەکان داوا کرد.', en:'The transition to the new ERP system demanded a great deal of patience and discipline from the teams.', fa:'تغییر به سیستم ERP جدید صبر و انضباط زیادی از تیم‌ها طلب کرد.', kmr:'Veguhastina bo pergala ERP ya nû gelek sebr û dîsîplîn ji tîman xwest.', pl:'Przejście na nowy system ERP wymagało od zespołów dużo cierpliwości i dyscypliny.', ro:'Trecerea la noul sistem ERP le-a cerut echipelor multă răbdare și disciplină.', ru:'Переход на новую ERP-систему потребовал от команд большого терпения и дисциплины.', sq:'Kalimi në sistemin e ri ERP u kërkoi ekipeve shumë durim dhe disiplinë.', tr:'Yeni ERP sistemine geçiş ekiplerden büyük sabır ve disiplin talep etti.' }),
      ex('Die Pflege eines schwerkranken Angehörigen kann einer Familie über Monate enorme Kraft abverlangen.', { ar:'يمكن أن تتطلب رعاية قريب شديد المرض قوة هائلة من الأسرة على مدى أشهر.', ckb:'چاودێریکردنی خزمێکی زۆر نەخۆش دەتوانێت بۆ چەند مانگ هێزێکی زۆر لە خێزانێک داوا بکات.', en:'Caring for a seriously ill relative can demand enormous strength from a family for months.', fa:'مراقبت از یک عضو خانواده که به‌شدت بیمار است می‌تواند ماه‌ها نیروی زیادی از خانواده طلب کند.', kmr:'Xwedîkirina xizmê nexweşekî giran dikare bi mehan hêzeke mezin ji malbatekê bixwaze.', pl:'Opieka nad ciężko chorym krewnym może przez miesiące wymagać od rodziny ogromnej siły.', ro:'Îngrijirea unei rude grav bolnave poate cere unei familii o forță enormă timp de luni întregi.', ru:'Уход за тяжело больным родственником может месяцами требовать от семьи огромных сил.', sq:'Kujdesi për një të afërm rëndë të sëmurë mund t’i kërkojë një familjeje forcë të madhe për muaj të tërë.', tr:'Ağır hasta bir yakına bakmak, aylar boyunca bir aileden büyük güç isteyebilir.' })
    ]
  },
  {
    word: 'abwegig', language: 'de', cefrLevel: level, partOfSpeech: 'Adjective', article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'ab-we-gig',
    topics: ['advanced-analysis','business-communication','quality-and-risk'], usageLabels: ['formal','written','analysis','advanced'], contextLabels: [],
    grammarNotes: ['adjective; often used to reject an idea as implausible or beside the point'],
    collocations: [{ text: 'eine abwegige Annahme', meaning: 'an implausible or far-fetched assumption' }],
    wordFamilies: [{ lemma: 'der Weg', relationLabel: 'noun', note: 'etymologically related' }], relations: [],
    meanings: meanings({ ar:'بعيد عن الصواب؛ غير معقول', ckb:'بێبنەما؛ زۆر دوور لە ڕاستی', en:'far-fetched; absurd; misguided', fa:'بی‌راه؛ نامعقول؛ دور از ذهن', kmr:'ji rastiyê dûr; bêbingeh; neaqilane', pl:'niedorzeczny; chybiony; mało prawdopodobny', ro:'absurd; nefondat; improbabil', ru:'абсурдный; надуманный; ошибочный', sq:'i pabazë; absurd; i paarsyeshëm', tr:'akla uzak; yersiz; saçma' }),
    examples: [
      ex('Die Annahme, dass alle Kunden ohne Schulung mit der neuen Oberfläche zurechtkommen, ist abwegig.', { ar:'الافتراض بأن جميع العملاء سيتعاملون مع الواجهة الجديدة دون تدريب افتراض غير معقول.', ckb:'ئەو گریمانەیەی هەموو کڕیاران بەبێ ڕاهێنان لەگەڵ ڕووکاری نوێ ڕادێن، بێبنەمایە.', en:'The assumption that all customers will manage the new interface without training is far-fetched.', fa:'این فرض که همه مشتریان بدون آموزش با رابط کاربری جدید کنار می‌آیند، نامعقول است.', kmr:'Ew texmîn ku hemû xerîdar bê perwerdekarî bi navrûya nû re baş bikin, ji rastiyê dûr e.', pl:'Założenie, że wszyscy klienci poradzą sobie z nowym interfejsem bez szkolenia, jest niedorzeczne.', ro:'Presupunerea că toți clienții se vor descurca fără instruire cu noua interfață este absurdă.', ru:'Предположение, что все клиенты справятся с новым интерфейсом без обучения, надуманно.', sq:'Supozimi se të gjithë klientët do ta përdorin ndërfaqen e re pa trajnim është i paarsyeshëm.', tr:'Tüm müşterilerin yeni arayüzü eğitim almadan kullanabileceği varsayımı akla uzaktır.' }),
      ex('Es wäre abwegig, die Verantwortung allein bei einzelnen Mitarbeitenden zu suchen.', { ar:'سيكون من غير المعقول البحث عن المسؤولية لدى موظفين أفراد فقط.', ckb:'بێبنەما دەبێت ئەگەر بەرپرسیارێتی تەنها لای کارمەندانی تاکەکەسی بگەڕێین.', en:'It would be misguided to look for responsibility solely among individual employees.', fa:'این بی‌راهه است که مسئولیت را فقط نزد کارکنان منفرد جست‌وجو کنیم.', kmr:'Ne rast e ku berpirsiyariyê tenê li cem karmendên takekesî bigerin.', pl:'Byłoby chybione szukać odpowiedzialności wyłącznie u pojedynczych pracowników.', ro:'Ar fi greșit să căutăm responsabilitatea doar la angajați individuali.', ru:'Было бы ошибочно искать ответственность только у отдельных сотрудников.', sq:'Do të ishte e gabuar të kërkohej përgjegjësia vetëm te punonjës të veçantë.', tr:'Sorumluluğu yalnızca tek tek çalışanlarda aramak yersiz olurdu.' })
    ]
  },
  {
    word: 'die Ätiologie', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Ätiologien', infinitive: null, pronunciationIpa: null, syllableBreak: 'Ä-ti-o-lo-gie',
    topics: ['healthcare-and-appointments','advanced-analysis','education-and-training'], usageLabels: ['formal','written','academic','advanced'], contextLabels: [],
    grammarNotes: ['feminine noun; medical and scientific term'],
    collocations: [{ text: 'die Ätiologie einer Erkrankung', meaning: 'the causes or origin of a disease' }],
    wordFamilies: [{ lemma: 'ätiologisch', relationLabel: 'adjective', note: null }], relations: [],
    meanings: meanings({ ar:'علم أسباب المرض؛ سبب المرض', ckb:'زانستی هۆکاری نەخۆشی؛ هۆکاری سەرچاوەی نەخۆشی', en:'etiology; cause or origin of a disease', fa:'علت‌شناسی؛ علت یا منشأ بیماری', kmr:'etiyolojî; sedem an çavkaniya nexweşiyê', pl:'etiologia; przyczyna lub pochodzenie choroby', ro:'etiologie; cauza sau originea unei boli', ru:'этиология; причина или происхождение болезни', sq:'etiologji; shkaku ose origjina e një sëmundjeje', tr:'etiyoloji; bir hastalığın nedeni veya kökeni' }),
    examples: [
      ex('Die Ätiologie der Beschwerden blieb unklar, obwohl mehrere Fachabteilungen beteiligt waren.', { ar:'ظل سبب الأعراض غير واضح رغم مشاركة عدة أقسام متخصصة.', ckb:'هۆکاری نیشانەکان ڕوون نەبوو، هەرچەندە چەند بەشی پسپۆڕی بەشدارییان کردبوو.', en:'The etiology of the symptoms remained unclear, although several specialist departments were involved.', fa:'علت‌شناسی شکایت‌ها نامشخص ماند، هرچند چند بخش تخصصی درگیر بودند.', kmr:'Etiyolojiya nîşanan nezelal ma, herçend çend beşên pispor tevlî bûn.', pl:'Etiologia dolegliwości pozostała niejasna, choć zaangażowanych było kilka oddziałów specjalistycznych.', ro:'Etiologia simptomelor a rămas neclară, deși au fost implicate mai multe departamente de specialitate.', ru:'Этиология жалоб осталась неясной, хотя были привлечены несколько специализированных отделений.', sq:'Etiologjia e shqetësimeve mbeti e paqartë, megjithëse u përfshinë disa departamente të specializuara.', tr:'Şikâyetlerin etiyolojisi, birkaç uzman bölüm dahil olmasına rağmen belirsiz kaldı.' }),
      ex('In der Studie wird die soziale Ätiologie von Bildungsungleichheit nicht als Nebenfrage behandelt.', { ar:'في الدراسة لا تُعامل الأسباب الاجتماعية لعدم المساواة التعليمية كمسألة ثانوية.', ckb:'لە توێژینەوەکەدا هۆکاری کۆمەڵایەتی نایەکسانیی پەروەردە وەک پرسی لاوەکی مامەڵەی لەگەڵ ناکرێت.', en:'In the study, the social etiology of educational inequality is not treated as a secondary issue.', fa:'در این مطالعه، علت‌شناسی اجتماعی نابرابری آموزشی به‌عنوان مسئله‌ای فرعی扱 نمی‌شود.', kmr:'Di lêkolînê de etiyolojiya civakî ya newekheviya perwerdeyê wek pirsek alî nayê dîtin.', pl:'W badaniu społeczna etiologia nierówności edukacyjnych nie jest traktowana jako kwestia poboczna.', ro:'În studiu, etiologia socială a inegalității educaționale nu este tratată ca o întrebare secundară.', ru:'В исследовании социальная этиология образовательного неравенства не рассматривается как второстепенный вопрос.', sq:'Në studim, etiologjia sociale e pabarazisë arsimore nuk trajtohet si çështje dytësore.', tr:'Çalışmada eğitim eşitsizliğinin sosyal etiyolojisi ikincil bir konu olarak ele alınmaz.' })
    ]
  },
  {
    word: 'der Affront', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'der', plural: 'Affronts', infinitive: null, pronunciationIpa: null, syllableBreak: 'Af-front',
    topics: ['business-communication','management-and-leadership','contracts-and-negotiation'], usageLabels: ['formal','written','business','sensitive'], contextLabels: [],
    grammarNotes: ['masculine noun; plural: Affronts'],
    collocations: [{ text: 'etwas als Affront empfinden', meaning: 'to perceive something as an insult or affront' }],
    wordFamilies: [], relations: [],
    meanings: meanings({ ar:'إهانة علنية؛ إساءة مقصودة', ckb:'سووکایەتی ئاشکرا؛ دژایەتی قەستکراو', en:'affront; deliberate insult', fa:'توهین آشکار؛ بی‌احترامی عمدی', kmr:'rezîlkirina eşkere; heqaretê bi mebest', pl:'afront; celowa zniewaga', ro:'afront; insultă deliberată', ru:'оскорбление; демонстративное неуважение', sq:'fyerje e hapur; poshtërim i qëllimshëm', tr:'hakaret; açık saygısızlık' }),
    examples: [
      ex('Die kurzfristige Ausladung des Partners wurde in der Verhandlung als schwerer Affront wahrgenommen.', { ar:'اعتُبر إلغاء دعوة الشريك في اللحظة الأخيرة إهانة كبيرة أثناء المفاوضات.', ckb:'بانگهێشت نەکردنی هاوبەشەکە لە کاتی کۆتاییدا لە دانوساندا وەک سووکایەتییەکی قورس وەرگیرا.', en:'The last-minute disinvitation of the partner was perceived as a serious affront in the negotiation.', fa:'لغو دعوت شریک در آخرین لحظه در مذاکره به‌عنوان توهینی جدی برداشت شد.', kmr:'Bêdawetkirina hevkarê di dema dawî de di danûstandinê de wek heqaretek giran hate têgihiştin.', pl:'Odwołanie zaproszenia partnera w ostatniej chwili zostało podczas negocjacji odebrane jako poważny afront.', ro:'Retragerea invitației partenerului în ultimul moment a fost percepută în negociere ca un afront grav.', ru:'Отмена приглашения партнёра в последний момент была воспринята на переговорах как серьёзное оскорбление.', sq:'Çftesa e partnerit në momentin e fundit u perceptua në negociata si një fyerje e rëndë.', tr:'Ortağın son anda davetten çıkarılması müzakerede ağır bir hakaret olarak algılandı.' }),
      ex('Für die Angehörigen war die nüchterne Formulierung des Bescheids ein zusätzlicher Affront.', { ar:'بالنسبة إلى الأقارب كانت الصياغة الجافة للقرار إهانة إضافية.', ckb:'بۆ خزمەکان، شێوازی وشک و فەرمی بڕیارنامەکە سووکایەتییەکی زیادە بوو.', en:'For the relatives, the cold wording of the notice was an additional affront.', fa:'برای بستگان، wording خشک ابلاغیه یک بی‌احترامی اضافی بود.', kmr:'Ji bo xizmên wan, şêwaza sar a biryarnameyê heqaretek din bû.', pl:'Dla krewnych chłodne sformułowanie decyzji było dodatkowym afrontem.', ro:'Pentru rude, formularea rece a deciziei a fost un afront suplimentar.', ru:'Для родственников сухая формулировка уведомления стала дополнительным оскорблением.', sq:'Për të afërmit, formulimi i ftohtë i vendimit ishte një fyerje shtesë.', tr:'Yakınları için karar yazısının soğuk üslubu ek bir saygısızlıktı.' })
    ]
  },
  {
    word: 'das Agens', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'das', plural: 'Agentien', infinitive: null, pronunciationIpa: null, syllableBreak: 'A-gens',
    topics: ['education-and-training','advanced-analysis','healthcare-and-appointments'], usageLabels: ['formal','written','academic','advanced'], contextLabels: [],
    grammarNotes: ['neuter noun; plural often Agentien; used in linguistics, medicine, and technical analysis'],
    collocations: [{ text: 'das handelnde Agens', meaning: 'the acting agent in a linguistic or analytical sense' }],
    wordFamilies: [{ lemma: 'agentiv', relationLabel: 'adjective', note: 'technical linguistic term' }], relations: [],
    meanings: meanings({ ar:'العامل الفاعل؛ المؤثر', ckb:'بکەر؛ هۆکاری کاریگەر', en:'agent; active cause or actor', fa:'عامل؛ کنشگر یا علت فعال', kmr:'aktor; sedema çalak', pl:'agens; czynnik sprawczy', ro:'agent; factor activ', ru:'агенс; действующее начало', sq:'agens; faktor veprues', tr:'etken; eyleyen unsur' }),
    examples: [
      ex('In der linguistischen Analyse bezeichnet das Agens die handelnde Instanz eines Satzes.', { ar:'في التحليل اللغوي يشير مصطلح agens إلى الجهة الفاعلة في الجملة.', ckb:'لە شیکردنەوەی زمانەوانیدا Agens ئاماژەیە بۆ لایەنی کردارکەر لە ڕستەدا.', en:'In linguistic analysis, the agent denotes the acting entity in a sentence.', fa:'در تحلیل زبان‌شناختی، agens به عامل کنشگر در جمله اشاره می‌کند.', kmr:'Di analîza zimanî de agens aliyê kiryarvan ê hevokê destnîşan dike.', pl:'W analizie lingwistycznej agens oznacza działający podmiot zdania.', ro:'În analiza lingvistică, agensul desemnează instanța care acționează într-o propoziție.', ru:'В лингвистическом анализе агенс обозначает действующую инстанцию предложения.', sq:'Në analizën gjuhësore, agensi tregon instancën vepruese të fjalisë.', tr:'Dilbilimsel analizde agens, cümlede eylemi gerçekleştiren unsuru ifade eder.' }),
      ex('Das auslösende Agens konnte im Labor nicht eindeutig identifiziert werden.', { ar:'لم يمكن تحديد العامل المسبب بوضوح في المختبر.', ckb:'هۆکاری دەستپێکەر لە تاقیگەدا بە ڕوونی نەناسرا.', en:'The triggering agent could not be clearly identified in the laboratory.', fa:'عامل محرک در آزمایشگاه به‌طور قطعی شناسایی نشد.', kmr:'Agensa destpêker di laboratûvarê de bi zelalî nehat nasîn.', pl:'Czynnik wywołujący nie został jednoznacznie zidentyfikowany w laboratorium.', ro:'Agentul declanșator nu a putut fi identificat clar în laborator.', ru:'Пусковой агент не удалось однозначно определить в лаборатории.', sq:'Agjenti shkaktues nuk mund të identifikohej qartë në laborator.', tr:'Tetikleyici etken laboratuvarda kesin olarak tanımlanamadı.' })
    ]
  },
  {
    word: 'die Ahnungslosigkeit', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'Ah-nungs-lo-sig-keit',
    topics: ['management-and-leadership','advanced-analysis','social-and-relationships'], usageLabels: ['formal','written','analysis','sensitive'], contextLabels: [],
    grammarNotes: ['feminine abstract noun; normally used in singular'],
    collocations: [{ text: 'eine erschreckende Ahnungslosigkeit', meaning: 'a shocking lack of awareness or knowledge' }],
    wordFamilies: [{ lemma: 'ahnungslos', relationLabel: 'adjective', note: null }], relations: [],
    meanings: meanings({ ar:'جهل بالأمر؛ عدم إدراك', ckb:'بێئاگایی؛ نەزانینی بارودۆخ', en:'lack of awareness; cluelessness', fa:'بی‌خبری؛ ناآگاهی', kmr:'bêagahî; nezanîna rewşê', pl:'nieświadomość; brak rozeznania', ro:'neștiință; lipsă de conștientizare', ru:'неосведомлённость; полное непонимание', sq:'padijeni; mungesë vetëdijeje', tr:'habersizlik; farkındalık eksikliği' }),
    examples: [
      ex('Die Ahnungslosigkeit des Vorstands über operative Risiken war nach dem Bericht kaum noch zu erklären.', { ar:'بعد التقرير أصبح من الصعب تفسير جهل مجلس الإدارة بالمخاطر التشغيلية.', ckb:'دوای ڕاپۆرتەکە، بێئاگایی ئەنجومەنی بەڕێوەبردن لە مەترسییە کارگێڕییەکان زۆر قورس بوو بۆ پاساودان.', en:'After the report, the board’s lack of awareness of operational risks was hardly explainable.', fa:'پس از گزارش، ناآگاهی هیئت‌مدیره نسبت به ریسک‌های عملیاتی دیگر به‌سختی قابل توضیح بود.', kmr:'Piştî raporê, bêagahiya rêveberiyê li ser xetereyên operasyonî êdî bi zor dikarîbû were şirovekirin.', pl:'Po raporcie nieświadomość zarządu dotycząca ryzyk operacyjnych była już trudna do wyjaśnienia.', ro:'După raport, neștiința consiliului de administrație privind riscurile operaționale era greu de explicat.', ru:'После отчёта неосведомлённость правления об операционных рисках было уже трудно объяснить.', sq:'Pas raportit, padijenia e bordit për rreziqet operative ishte vështirë të shpjegohej.', tr:'Rapordan sonra yönetim kurulunun operasyonel risklerden habersizliği artık zor açıklanırdı.' }),
      ex('Ihre scheinbare Ahnungslosigkeit schützte sie nicht vor der Verantwortung für die Entscheidung.', { ar:'لم تحمها قلة معرفتها الظاهرية من المسؤولية عن القرار.', ckb:'بێئاگایی دیارەکەی نەیتوانی لە بەرپرسیارێتی بڕیارەکە بیپارێزێت.', en:'Her apparent cluelessness did not protect her from responsibility for the decision.', fa:'بی‌خبری ظاهری او، او را از مسئولیت آن تصمیم محافظت نکرد.', kmr:'Bêagahiya wê ya xuya ew ji berpirsiyariya biryarê neparast.', pl:'Jej pozorna nieświadomość nie uchroniła jej przed odpowiedzialnością za decyzję.', ro:'Aparenta ei neștiință nu a protejat-o de responsabilitatea pentru decizie.', ru:'Её кажущаяся неосведомлённость не избавила её от ответственности за решение.', sq:'Padijenia e saj e dukshme nuk e mbrojti nga përgjegjësia për vendimin.', tr:'Görünürdeki habersizliği onu kararın sorumluluğundan korumadı.' })
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

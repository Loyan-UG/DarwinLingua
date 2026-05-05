const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '069';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['revoltieren','rhetorisch','das Rollenmuster','das Sakrileg','scharfzüngig','der Schein'];

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
    word: 'revoltieren', partOfSpeech: 'Verb', infinitive: 'revoltieren', syllableBreak: 're-vol-tie-ren',
    topics: ['social-and-relationships','law-and-compliance','work-and-jobs'], usageLabels: ['formal','written','sensitive','advanced'],
    collocations: [{ text: 'gegen eine Entscheidung revoltieren', meaning: 'to revolt against a decision' }],
    meanings: meaning('يثور؛ يتمرد','ڕاپەڕین؛ یاخیبوون','to revolt; to rebel','شورش کردن؛ سر به طغیان برداشتن','serhildan; serî hildan','buntować się; rewoltować','a se revolta; a se răzvrăti','бунтовать; восставать','revoltohem; rebelohem','isyan etmek; başkaldırmak'),
    examples: [
      ex('Die Belegschaft begann zu revoltieren, als die Kürzungen ohne jede Anhörung beschlossen wurden.', trans('بدأ العاملون بالتمرد عندما تقررت التخفيضات من دون أي استماع لهم.','کارمەندان دەستیان کرد بە یاخیبوون، کاتێک کەمکردنەوەکان بەبێ هیچ گوێگرتنێک بڕیاریان لەسەر درا.','The workforce began to revolt when the cuts were decided without any hearing.','کارکنان وقتی کاهش‌ها بدون هیچ شنیدنی تصویب شد، شروع به شورش کردند.','Karkeran dest bi serhildanê kir dema kêmkirin bê tu guhdarîkirinê hatin biryardan.','Załoga zaczęła się buntować, gdy cięcia uchwalono bez żadnego wysłuchania.','Angajații au început să se revolte când reducerile au fost decise fără nicio audiere.','Коллектив начал бунтовать, когда сокращения приняли без какого-либо обсуждения.','Punonjësit filluan të revoltoheshin kur shkurtimet u vendosën pa asnjë dëgjim.','Çalışanlar, kesintiler hiçbir dinleme yapılmadan kararlaştırılınca isyan etmeye başladı.')),
      ex('Im Drama revoltieren die Kinder nicht nur gegen den Vater, sondern gegen eine ganze Ordnung des Schweigens.', trans('في المسرحية لا يتمرد الأبناء على الأب فقط، بل على نظام كامل من الصمت.','لە شانۆنامەکەدا منداڵەکان نەک تەنها دژی باوکەکە یاخی دەبن، بەڵکو دژی تەواوی سیستەمێکی بێدەنگی.','In the drama, the children revolt not only against the father but against an entire order of silence.','در نمایش، فرزندان نه فقط علیه پدر، بلکه علیه کل نظمی از سکوت شورش می‌کنند.','Di dramayê de zarok ne tenê li dijî bav serî hildidin, lê li dijî tevahiya rêzika bêdengiyê.','W dramacie dzieci buntują się nie tylko przeciw ojcu, lecz przeciw całemu porządkowi milczenia.','În dramă, copiii se revoltă nu doar împotriva tatălui, ci împotriva unei întregi ordini a tăcerii.','В драме дети восстают не только против отца, но против целого порядка молчания.','Në dramë, fëmijët revoltohen jo vetëm kundër babait, por kundër një rendi të tërë heshtjeje.','Dramda çocuklar yalnızca babaya değil, bütün bir sessizlik düzenine başkaldırır.'))
    ]
  }),
  entry({
    word: 'rhetorisch', partOfSpeech: 'Adjective', syllableBreak: 'rhe-to-risch',
    topics: ['business-communication','culture-and-media','education-and-training'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'eine rhetorische Frage', meaning: 'a rhetorical question' }],
    meanings: meaning('بلاغي؛ خطابي','ڕەوانبێژی؛ وتاری','rhetorical','بلاغی؛ خطابی','retorîkî','retoryczny','retoric','риторический','retorik','retorik'),
    examples: [
      ex('Die rhetorische Stärke der Präsentation verdeckte zunächst, dass die Finanzplanung kaum belastbare Annahmen enthielt.', trans('حجبت القوة البلاغية للعرض في البداية أن التخطيط المالي لم يتضمن افتراضات متينة تقريباً.','هێزی ڕەوانبێژی پێشکەشکردنەکە سەرەتا ئەوەی داپۆشی کە پلانی دارایی نزیکەی هیچ گریمانەیەکی پشتپێبەستوی نەبوو.','The rhetorical strength of the presentation initially concealed that the financial plan contained hardly any robust assumptions.','قدرت بلاغی ارائه ابتدا پنهان کرد که برنامه مالی تقریباً هیچ فرض قابل اتکایی نداشت.','Hêza retorîkî ya pêşkêşkirinê di destpêkê de veşart ku plansaziya darayî hema bê texmînên bawerbar bû.','Retoryczna siła prezentacji początkowo zasłoniła fakt, że plan finansowy zawierał niewiele solidnych założeń.','Forța retorică a prezentării a ascuns inițial faptul că planificarea financiară conținea foarte puține ipoteze solide.','Риторическая сила презентации сначала скрыла, что финансовый план почти не содержал надежных допущений.','Forca retorike e prezantimit fillimisht fshehu se planifikimi financiar kishte shumë pak supozime të qëndrueshme.','Sunumun retorik gücü, finans planının neredeyse hiç sağlam varsayım içermediğini başlangıçta örttü.')),
      ex('Der rhetorische Aufbau des Essays führt den Leser von scheinbarer Zustimmung zu grundlegender Irritation.', trans('يقود البناء البلاغي للمقال القارئ من موافقة ظاهرية إلى ارتباك عميق.','پێکهاتەی ڕەوانبێژی وتارەکە خوێنەر لە ڕەزامەندیی ڕووکەشییەوە بەرەو شڵەژانی بنەڕەتی دەبات.','The rhetorical structure of the essay leads the reader from apparent agreement to fundamental unease.','ساختار بلاغی مقاله خواننده را از موافقت ظاهری به آشفتگی بنیادین می‌برد.','Avahiya retorîkî ya gotarê xwendevan ji razîbûna xuya ber bi nerazîbûna bingehîn dibe.','Retoryczna konstrukcja eseju prowadzi czytelnika od pozornej zgody do zasadniczego niepokoju.','Construcția retorică a eseului conduce cititorul de la acord aparent la neliniște fundamentală.','Риторическое построение эссе ведет читателя от кажущегося согласия к глубокому беспокойству.','Ndërtimi retorik i esesë e çon lexuesin nga pajtimi i dukshëm drejt shqetësimit themelor.','Denemenin retorik yapısı okuru görünürdeki onaydan temel bir huzursuzluğa götürür.'))
    ]
  }),
  entry({
    word: 'das Rollenmuster', partOfSpeech: 'Noun', article: 'das', plural: 'Rollenmuster', syllableBreak: 'Rol-len-mus-ter',
    topics: ['social-and-relationships','human-resources','advanced-analysis'], usageLabels: ['formal','written','analysis','sensitive'],
    collocations: [{ text: 'tradierte Rollenmuster aufbrechen', meaning: 'to break up inherited role patterns' }],
    meanings: meaning('نمط أدوار؛ قالب اجتماعي للأدوار','نموونەی ڕۆڵ؛ شێوازی کۆمەڵایەتی ڕۆڵەکان','role pattern; social role model','الگوی نقش؛ قالب رفتاری اجتماعی','şêwaza rolê; modela civakî ya rolê','wzorzec ról','tipar de rol; model social','ролевая модель; шаблон ролей','model rolesh; tipar shoqëror','rol kalıbı; sosyal rol modeli'),
    examples: [
      ex('Im Führungstraining wurden Rollenmuster sichtbar, die Frauen häufiger in vermittelnde und Männer in entscheidende Positionen drängten.', trans('في تدريب القيادة ظهرت أنماط أدوار دفعت النساء غالباً إلى مواقع الوساطة والرجال إلى مواقع القرار.','لە ڕاهێنانی سەرکردایەتی دا نموونەی ڕۆڵەکان دیار بوون کە ژنان زیاتر بەرەو ڕۆڵی نێوبژیوانی و پیاوان بەرەو بڕیاردان پاڵ پێوەدەن.','In leadership training, role patterns became visible that more often pushed women into mediating roles and men into decision-making positions.','در آموزش رهبری الگوهای نقشی آشکار شد که زنان را بیشتر به نقش‌های میانجی‌گر و مردان را به موقعیت‌های تصمیم‌گیرنده می‌راند.','Di perwerdeya rêveberiyê de şêweyên rolê xuya bûn ku jinan zêdetir ber bi rolên navbeynkarî û mêran ber bi pozîsyonên biryardanê ve didan.','W szkoleniu przywódczym ujawniły się wzorce ról, które częściej spychały kobiety do ról mediacyjnych, a mężczyzn do decyzyjnych.','În trainingul de leadership au devenit vizibile tipare de rol care împingeau mai des femeile spre roluri de mediere și bărbații spre poziții decizionale.','На тренинге лидерства стали видны ролевые шаблоны, чаще отводившие женщинам посреднические роли, а мужчинам позиции принятия решений.','Në trajnimin e lidershipit u bënë të dukshme modele rolesh që i shtynin më shpesh gratë në role ndërmjetësuese dhe burrat në pozicione vendimmarrëse.','Liderlik eğitiminde kadınları daha çok arabulucu rollere, erkekleri ise karar pozisyonlarına iten rol kalıpları görünür oldu.')),
      ex('Der Roman reproduziert alte Rollenmuster zunächst, um sie im letzten Drittel gezielt zu unterlaufen.', trans('يعيد الرواية إنتاج أنماط أدوار قديمة في البداية كي يقوّضها عمداً في الثلث الأخير.','ڕۆمانەکە سەرەتا نموونەی ڕۆڵی کۆن دووبارە دەکاتەوە بۆ ئەوەی لە سێیەکی کۆتاییدا بە ئامانجدار بیانشکێنێت.','The novel first reproduces old role patterns in order to deliberately subvert them in the final third.','رمان ابتدا الگوهای نقش قدیمی را بازتولید می‌کند تا در یک‌سوم پایانی عامدانه آن‌ها را واژگون کند.','Roman pêşî şêweyên rolê yên kevn dubare dike da ku di sêyeka dawî de bi mebest wan binpê bike.','Powieść najpierw reprodukuje stare wzorce ról, aby w ostatniej trzeciej części celowo je podważyć.','Romanul reproduce inițial vechi tipare de rol pentru a le submina deliberat în ultima treime.','Роман сначала воспроизводит старые ролевые модели, чтобы в последней трети целенаправленно их подорвать.','Romani fillimisht riprodhon modele të vjetra rolesh për t’i minuar qëllimisht në të tretën e fundit.','Roman, son üçte birlik bölümde bilinçli olarak altüst etmek için önce eski rol kalıplarını yeniden üretir.'))
    ]
  }),
  entry({
    word: 'das Sakrileg', partOfSpeech: 'Noun', article: 'das', plural: 'Sakrilege', syllableBreak: 'Sa-kri-leg',
    topics: ['culture-and-media','social-and-relationships','advanced-analysis'], usageLabels: ['formal','written','sensitive','advanced'],
    collocations: [{ text: 'als Sakrileg gelten', meaning: 'to be considered sacrilege' }],
    meanings: meaning('تدنيس المقدس؛ انتهاك محرّم','پێشێلکردنی پیرۆزی؛ تاوانی دژی شتی پیرۆز','sacrilege; violation of something sacred','حرمت‌شکنی؛ تعرض به امر مقدس','pîrozîşikandin; li dijî tiştê pîroz','świętokradztwo','sacrilegiu','святотатство','sakrilegj; përdhosje e së shenjtës','kutsala saygısızlık; sakrilej'),
    examples: [
      ex('Für manche Abteilungen galt es beinahe als Sakrileg, das seit zwanzig Jahren genutzte ERP-Modul grundsätzlich infrage zu stellen.', trans('بالنسبة إلى بعض الأقسام كان التشكيك الجذري في وحدة ERP المستخدمة منذ عشرين عاماً يكاد يُعد تدنيساً للمقدسات.','بۆ هەندێک بەش، پرسیارکردنی بنەڕەتی لە مۆدیولی ERP کە بیست ساڵ بەکارهاتبوو نزیک بوو بە پێشێلکردنی پیرۆزی دابنرێت.','For some departments, fundamentally questioning the ERP module used for twenty years was almost considered sacrilege.','برای برخی واحدها، زیر سؤال بردن اساسی ماژول ERP که بیست سال استفاده شده بود تقریباً حرمت‌شکنی محسوب می‌شد.','Ji bo hin beşan, pirskirina bingehîn a modula ERP ya bîst salan tê bikaranîn hema wek pîrozîşikandin dihate dîtin.','Dla niektórych działów zasadnicze zakwestionowanie modułu ERP używanego od dwudziestu lat graniczyło ze świętokradztwem.','Pentru unele departamente, punerea fundamentală sub semnul întrebării a modulului ERP folosit de douăzeci de ani era aproape un sacrilegiu.','Для некоторых отделов принципиально поставить под вопрос ERP-модуль, используемый двадцать лет, почти считалось святотатством.','Për disa departamente, vënia në dyshim thelbësore e modulit ERP të përdorur prej njëzet vitesh konsiderohej pothuajse sakrilegj.','Bazı departmanlar için yirmi yıldır kullanılan ERP modülünü temelden sorgulamak neredeyse kutsala saygısızlık sayılıyordu.')),
      ex('Die Inszenierung wurde als Sakrileg empfunden, weil sie einen kanonischen Text radikal in die Gegenwart übertrug.', trans('اعتُبر العرض حرمت‌شکنی لأنه نقل نصاً قانونياً كلاسيكياً بصورة جذرية إلى الحاضر.','نمایشەکە وەک پێشێلکردنی پیرۆزی هەست پێکرا، چونکە دەقێکی کانونیی بە شێوەی ڕادیکاڵ بۆ ئێستا گواستەوە.','The production was felt to be sacrilege because it radically transferred a canonical text into the present.','اجرا حرمت‌شکنی تلقی شد، چون متنی کلاسیک و کانونی را به‌طور رادیکال به زمان حال منتقل کرد.','Şanogerî wek pîrozîşikandin hate hestkirin, ji ber ku nivîseke kanonîk bi awayekî radîkal anî roja îro.','Inscenizację odebrano jako świętokradztwo, ponieważ radykalnie przeniosła kanoniczny tekst do współczesności.','Punerea în scenă a fost percepută ca un sacrilegiu deoarece a transpus radical un text canonic în prezent.','Постановку восприняли как святотатство, потому что она радикально перенесла канонический текст в современность.','Vënia në skenë u përjetua si sakrilegj, sepse e solli radikalisht një tekst kanonik në të tashmen.','Sahneleme, kanonik bir metni radikal biçimde günümüze taşıdığı için kutsala saygısızlık olarak algılandı.'))
    ]
  }),
  entry({
    word: 'scharfzüngig', partOfSpeech: 'Adjective', syllableBreak: 'scharf-zün-gig',
    topics: ['business-communication','culture-and-media','social-and-relationships'], usageLabels: ['formal','written','sensitive','advanced'],
    collocations: [{ text: 'scharfzüngige Kritik', meaning: 'sharp-tongued criticism' }],
    meanings: meaning('لاذع اللسان؛ حاد النقد','زمانتوند؛ ڕەخنەی توند','sharp-tongued; caustic','تیززبان؛ نیش‌دار','ziman-tûj; bi rexneya tûj','cięty; złośliwie przenikliwy','cu limbă ascuțită; caustic','острый на язык; язвительный','gjuhëmprehtë; therës','sivri dilli; iğneleyici'),
    examples: [
      ex('Ihre scharfzüngige Analyse traf den wunden Punkt der Strategie, ohne in persönliche Abwertung abzugleiten.', trans('أصاب تحليلها اللاذع نقطة الضعف في الاستراتيجية من دون الانزلاق إلى الإهانة الشخصية.','شیکاری زمانتوندی ئەو خاڵی لاوازی ستراتیژییەکەی گرت، بەبێ ئەوەی بگاتە سووکایەتی کەسی.','Her sharp-tongued analysis hit the weak point of the strategy without sliding into personal disparagement.','تحلیل تیززبانانه او نقطه دردناک راهبرد را نشانه رفت، بدون آن‌که به تحقیر شخصی بلغزد.','Analîza wê ya ziman-tûj xala êşdar a stratejiyê girt bê ku bikeve binirxandina kesane.','Jej cięta analiza trafiła w słaby punkt strategii, nie osuwając się w osobiste poniżanie.','Analiza ei caustică a atins punctul vulnerabil al strategiei fără a aluneca în depreciere personală.','Ее язвительный анализ попал в больное место стратегии, не скатываясь в личное унижение.','Analiza e saj gjuhëmprehtë goditi pikën e dobët të strategjisë pa rrëshqitur në përçmim personal.','Sivri dilli analizi stratejinin hassas noktasını vurdu, kişisel küçümsemeye kaymadan.')),
      ex('Der Kritiker schreibt scharfzüngig, aber seine Polemik bleibt durch genaue Textbeobachtung gedeckt.', trans('يكتب الناقد بلسان حاد، لكن سجاله يبقى مسنوداً بملاحظة دقيقة للنص.','ڕەخنەگرەکە بە زمانێکی توند دەنووسێت، بەڵام مشتومڕەکەی بە چاودێری وردی دەق پشتگیری دەکرێت.','The critic writes sharp-tonguedly, but his polemic remains supported by precise textual observation.','منتقد تیززبان می‌نویسد، اما جدل او همچنان بر مشاهده دقیق متن تکیه دارد.','Rexnegir bi zimanê tûj dinivîse, lê polemîka wî bi çavdêriya hûrgulî ya nivîsê tê piştgirîkirin.','Krytyk pisze ciętym językiem, ale jego polemika pozostaje poparta dokładną obserwacją tekstu.','Criticul scrie caustic, dar polemica lui rămâne susținută de o observare precisă a textului.','Критик пишет язвительно, но его полемика подкреплена точным наблюдением над текстом.','Kritiku shkruan me gjuhë të mprehtë, por polemika e tij mbetet e mbështetur nga vëzhgimi i saktë i tekstit.','Eleştirmen sivri dilli yazar, ancak polemiği titiz metin gözlemiyle desteklenir.'))
    ]
  }),
  entry({
    word: 'der Schein', partOfSpeech: 'Noun', article: 'der', plural: 'Scheine', syllableBreak: 'Schein',
    topics: ['advanced-analysis','business-communication','everyday-life'], usageLabels: ['formal','written','analysis','advanced'],
    grammarNotes: ['masculine noun; can mean appearance, glow, or certificate depending on context'],
    collocations: [{ text: 'den Schein wahren', meaning: 'to keep up appearances' }],
    meanings: meaning('مظهر؛ ظاهر؛ شهادة أو ورقة','ڕووکەش؛ دەرکەوتن؛ بڕوانامە یان پارچەکاغەز','appearance; semblance; certificate or note','ظاهر؛ جلوه؛ برگه یا گواهی','xuyabûn; rûkêş; belge','pozór; blask; zaświadczenie','aparență; lucire; certificat','видимость; блеск; документ','pamje; dukje; certifikatë','görünüş; izlenim; belge'),
    examples: [
      ex('Das Management wahrte den Schein der Kontrolle, obwohl intern längst bekannt war, dass der Zeitplan nicht zu halten war.', trans('حافظت الإدارة على مظهر السيطرة، رغم أن الجميع داخلياً كان يعلم منذ مدة أن الجدول الزمني غير قابل للالتزام.','بەڕێوەبەرایەتی ڕووکەشی کۆنتڕۆڵی پاراست، هەرچەندە لە ناوخۆدا زۆر پێشتر زانرابوو کە پلانی کات جێبەجێ ناکرێت.','Management kept up the appearance of control, although internally it had long been known that the schedule could not be met.','مدیریت ظاهر کنترل را حفظ کرد، با اینکه در داخل مدت‌ها بود معلوم شده بود زمان‌بندی قابل اجرا نیست.','Rêveberî xuyabûna kontrolê parast, herçend di hundir de demek dirêj diyar bû ku plansaziya demê nayê girtin.','Kierownictwo zachowywało pozory kontroli, choć wewnętrznie od dawna było wiadomo, że harmonogramu nie da się dotrzymać.','Managementul a păstrat aparența controlului, deși intern se știa de mult că termenul nu putea fi respectat.','Руководство сохраняло видимость контроля, хотя внутри давно было известно, что график не выдержать.','Menaxhmenti ruajti pamjen e kontrollit, megjithëse brenda dihej prej kohësh se afati nuk mund të mbahej.','Yönetim kontrol görüntüsünü korudu, oysa içeride takvimin tutturmanın mümkün olmadığı uzun zamandır biliniyordu.')),
      ex('Im fahlen Schein der Straßenlaterne wirkt die leere Bühne weniger realistisch als traumartig.', trans('في الضوء الشاحب لمصباح الشارع تبدو الخشبة الخالية أقل واقعية وأكثر شبيهة بالحلم.','لە تیشکی زەردباوی چرای شەقامدا شانۆی بەتاڵ کەمتر ڕاستەقینە و زیاتر وەک خەون دەردەکەوێت.','In the pale glow of the streetlamp, the empty stage seems less realistic than dreamlike.','در نور رنگ‌پریده چراغ خیابان، صحنه خالی کمتر واقعی و بیشتر رؤیاگونه به نظر می‌رسد.','Di ronahiya şîn a çiraya kolanê de sehneya vala kêmtir rastîn û zêdetir wek xewn xuya dike.','W bladym blasku latarni pusta scena wydaje się mniej realistyczna niż senna.','În lumina palidă a felinarului, scena goală pare mai puțin realistă și mai degrabă onirică.','В бледном свете фонаря пустая сцена кажется не столько реалистичной, сколько сновидческой.','Në dritën e zbehtë të llambës së rrugës, skena bosh duket më pak realiste dhe më shumë ëndërrimtare.','Sokak lambasının solgun ışığında boş sahne gerçekçi olmaktan çok düşsel görünür.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 069', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

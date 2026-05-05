const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '063';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['parodistisch','die Pathetik','pathetisch','das Pathos','das Patiens','die Perspektivierung'];

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
    word: 'parodistisch', partOfSpeech: 'Adjective', syllableBreak: 'pa-ro-dis-tisch',
    topics: ['culture-and-media','business-communication','advanced-analysis'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'eine parodistische Überzeichnung', meaning: 'a parodic exaggeration' }],
    meanings: meaning('ساخر بأسلوب المحاكاة؛ تهكمي','بە شێوەی پارۆدی؛ گاڵتەئامێز','parodic; parody-like','پارودیک؛ تقلیدی طنزآمیز','parodîk; bi şêweya tinazî','parodystyczny','parodic; satiric','пародийный','parodik; satirik','parodik; hicivli'),
    examples: [
      ex('Die Kampagne griff den Ton klassischer Unternehmensleitbilder parodistisch auf und machte gerade dadurch deren Leere sichtbar.', trans('استعادت الحملة نبرة بيانات الرؤية المؤسسية الكلاسيكية بطريقة ساخرة، وبذلك كشفت فراغها.','کەمپەینەکە تۆنی ڕێنماییە کۆمپانیاییە کلاسیکییەکانی بە شێوەی پارۆدی وەرگرت و بەوەش بەتاڵییەکانیان دیار کرد.','The campaign picked up the tone of classic corporate mission statements in a parodic way, thereby exposing their emptiness.','کمپین لحن بیانیه‌های مأموریت شرکتی کلاسیک را به شکلی پارودیک به کار گرفت و همین، تهی‌بودن آن‌ها را آشکار کرد.','Kampanyayê dengê daxuyaniyên kevneşopî yên armanca pargîdaniyê bi awayekî parodîk girt û bi vê yekê vala bûna wan xuya kir.','Kampania parodystycznie przejęła ton klasycznych deklaracji misji firmowej i właśnie przez to ujawniła ich pustkę.','Campania a preluat parodic tonul declarațiilor clasice de misiune corporativă și tocmai astfel le-a scos la iveală goliciunea.','Кампания пародийно воспроизвела тон классических корпоративных миссий и тем самым показала их пустоту.','Fushata mori në mënyrë parodike tonin e deklaratave klasike të misionit të kompanive dhe pikërisht kështu zbuloi zbrazëtinë e tyre.','Kampanya, klasik kurumsal misyon metinlerinin tonunu parodik biçimde kullandı ve tam da böylece onların boşluğunu görünür kıldı.')),
      ex('Der Roman arbeitet parodistisch mit Heldenmotiven, ohne die moralische Ernsthaftigkeit der Handlung preiszugeben.', trans('تتعامل الرواية بسخرية محاكية مع دوافع البطولة من دون التخلي عن الجدية الأخلاقية للحبكة.','ڕۆمانەکە بە شێوەی پارۆدی لەگەڵ مۆتیفە پاڵەوانییەکان کاردەکات، بەبێ ئەوەی جدییەتی ئەخلاقیی ڕووداوەکان لەدەست بدات.','The novel uses heroic motifs parodically without giving up the moral seriousness of the plot.','رمان با موتیف‌های قهرمانانه به‌صورت پارودیک کار می‌کند، بدون آن‌که جدیت اخلاقی داستان را از دست بدهد.','Roman bi motîfên lehengî bi awayekî parodîk kar dike bê ku giraniya exlaqî ya çîrokê winda bike.','Powieść parodystycznie operuje motywami heroicznymi, nie rezygnując z moralnej powagi fabuły.','Romanul folosește parodic motive eroice fără a renunța la seriozitatea morală a acțiunii.','Роман пародийно работает с героическими мотивами, не отказываясь от нравственной серьезности сюжета.','Romani punon në mënyrë parodike me motive heroike, pa hequr dorë nga serioziteti moral i ngjarjes.','Roman, olay örgüsünün ahlaki ciddiyetinden vazgeçmeden kahramanlık motiflerini parodik biçimde kullanır.'))
    ]
  }),
  entry({
    word: 'die Pathetik', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Pa-the-tik',
    topics: ['culture-and-media','business-communication','advanced-analysis'], usageLabels: ['formal','written','analysis','advanced'],
    grammarNotes: ['feminine abstract noun; normally used in singular'],
    collocations: [{ text: 'in übermäßige Pathetik verfallen', meaning: 'to lapse into excessive pathos' }],
    meanings: meaning('نبرة خطابية عاطفية؛ فخامة مبالغ فيها','پاتۆسی زمان؛ گەرمیی زیاده‌ڕەوی','pathos; elevated emotional rhetoric','لحن پرطمطراق و احساسی؛ پاتتیک','patos; şêwaza hestyarî ya bilind','patos; podniosłość','patetism; retorică emoțională înaltă','патетика; возвышенная эмоциональная риторика','patetikë; retorikë emocionale e ngritur','pathos; abartılı duygusal üslup'),
    examples: [
      ex('Die Rede überzeugte dort, wo sie konkrete Verantwortung benannte; sie verlor jedoch an Kraft, sobald sie in abstrakte Pathetik auswich.', trans('أقنع الخطاب حين سمّى المسؤولية الملموسة، لكنه فقد قوته عندما هرب إلى خطابية عاطفية مجردة.','وتارەکە لەو شوێنە قایلکەر بوو کە بەرپرسیاریی دیاریکراوی ناوبرد؛ بەڵام کاتێک بۆ پاتۆسی ئەبستراکت ڕایکرد، هێزی لەدەستدا.','The speech was persuasive where it named concrete responsibility, but it lost force as soon as it retreated into abstract pathos.','سخنرانی آنجا قانع‌کننده بود که مسئولیت مشخص را نام برد؛ اما به محض پناه بردن به پاتتیک انتزاعی، قدرتش را از دست داد.','Gotar li wir qanihker bû ku berpirsiyariya zehmet bi nav kir; lê gava ku xwe spart patoseke razber, hêza xwe winda kir.','Przemówienie przekonywało tam, gdzie nazywało konkretną odpowiedzialność, lecz traciło siłę, gdy uciekało w abstrakcyjny patos.','Discursul convingea acolo unde numea responsabilitatea concretă, dar își pierdea forța când se refugia în patetism abstract.','Речь убеждала там, где называла конкретную ответственность, но теряла силу, как только уходила в абстрактную патетику.','Fjalimi bindte aty ku emërtonte përgjegjësi konkrete, por humbiste forcën sapo strehohej në patetikë abstrakte.','Konuşma somut sorumluluğu adlandırdığı yerde ikna ediciydi; soyut pathosa kaçtığında ise gücünü yitiriyordu.')),
      ex('Die Inszenierung vermeidet billige Pathetik und lässt die Trauer der Figuren gerade durch Zurückhaltung wirken.', trans('يتجنب العرض العاطفية الرخيصة ويجعل حزن الشخصيات مؤثراً تحديداً من خلال ضبط النفس.','نمایشەکە خۆی لە پاتۆسی هەرزان دەپارێزێت و خەمگینی کارەکتەرەکان بە هۆی خۆگرتنەوە کاریگەر دەکات.','The production avoids cheap pathos and lets the characters’ grief work precisely through restraint.','اجرا از پاتتیک سطحی پرهیز می‌کند و اندوه شخصیت‌ها را دقیقاً از راه خویشتن‌داری اثرگذار می‌سازد.','Şanogerî ji patosa erzan dûr dikeve û xemgîniya kesayetan bi xweparastinê bandorker dike.','Inscenizacja unika taniego patosu i pozwala żałobie postaci oddziaływać właśnie przez powściągliwość.','Punerea în scenă evită patetismul ieftin și lasă durerea personajelor să acționeze tocmai prin reținere.','Постановка избегает дешевой патетики и заставляет горе персонажей действовать именно через сдержанность.','Vënia në skenë shmang patetikën e lirë dhe e bën pikëllimin e personazheve të ndikojë pikërisht përmes përmbajtjes.','Sahneleme ucuz pathostan kaçınır ve karakterlerin yasını tam da ölçülülük yoluyla etkili kılar.'))
    ]
  }),
  entry({
    word: 'pathetisch', partOfSpeech: 'Adjective', syllableBreak: 'pa-the-tisch',
    topics: ['culture-and-media','business-communication','social-and-relationships'], usageLabels: ['formal','written','advanced','sensitive'],
    collocations: [{ text: 'pathetisch klingen', meaning: 'to sound overly solemn or emotional' }],
    meanings: meaning('خطابي عاطفي؛ مفرط الجدية','پاتۆسی؛ بە هەستێکی زیادەوە','pathetic in the rhetorical sense; overly solemn or emotional','پرطمطراق و احساسی؛ بیش از حد جدی','patetîk; bi hesteke zêde','patetyczny','patetic; prea solemn','патетический; чрезмерно торжественный','patetik; tepër solemn','patetik; aşırı duygulu veya ciddi'),
    examples: [
      ex('Die Entschuldigung klang pathetisch, weil sie große Worte über Verantwortung enthielt, aber keine einzige konkrete Maßnahme.', trans('بدا الاعتذار خطابياً مبالغاً فيه لأنه احتوى كلمات كبيرة عن المسؤولية من دون إجراء واحد ملموس.','داوای لێبوردنەکە پاتۆسی دەنگی دەدا، چونکە وشەی گەورەی لەبارەی بەرپرسیارییەوە تێدا بوو، بەڵام هیچ هەنگاوێکی دیاریکراوی نەبوو.','The apology sounded overly solemn because it contained grand words about responsibility but not a single concrete measure.','عذرخواهی پرطمطراق به نظر می‌رسید، چون واژه‌های بزرگی درباره مسئولیت داشت اما حتی یک اقدام مشخص نه.','Lêborîn patetîk xuya kir, ji ber ku peyvên mezin li ser berpirsiyarî tê de hebûn lê ti gavên zehmet tune bûn.','Przeprosiny brzmiały patetycznie, ponieważ zawierały wielkie słowa o odpowiedzialności, ale ani jednego konkretnego działania.','Scuzele au sunat patetic, deoarece conțineau cuvinte mari despre responsabilitate, dar nicio măsură concretă.','Извинение звучало патетически, потому что содержало громкие слова об ответственности, но ни одной конкретной меры.','Kërkimfalja tingëlloi patetike, sepse kishte fjalë të mëdha për përgjegjësinë, por asnjë masë konkrete.','Özür, sorumluluk hakkında büyük sözler içerdiği ama tek bir somut önlem sunmadığı için patetik duyuldu.')),
      ex('Der Schluss des Films ist pathetisch, doch die vorherige Zurückhaltung verhindert, dass er sentimental wirkt.', trans('نهاية الفيلم عاطفية خطابية، لكن التحفظ السابق يمنعها من أن تبدو عاطفية مبتذلة.','کۆتایی فیلمەکە پاتۆسییە، بەڵام خۆگرتنەوەی پێشوو ڕێگری دەکات لەوەی هەستیارانەی سادە دەربکەوێت.','The ending of the film is solemnly emotional, yet the earlier restraint keeps it from seeming sentimental.','پایان فیلم پرشور و پاتتیک است، اما خویشتن‌داری قبلی مانع می‌شود که احساساتی و سطحی به نظر برسد.','Dawiya fîlmê patetîk e, lê xweparastina berê nahêle ku ew sentimental xuya bike.','Zakończenie filmu jest patetyczne, ale wcześniejsza powściągliwość sprawia, że nie wydaje się sentymentalne.','Finalul filmului este patetic, dar reținerea anterioară îl împiedică să pară sentimental.','Финал фильма патетичен, но прежняя сдержанность не дает ему выглядеть сентиментальным.','Fundi i filmit është patetik, por përmbajtja e mëparshme e pengon të duket sentimental.','Filmin sonu patetiktir, ancak önceki ölçülülük onun duygusal ve basit görünmesini engeller.'))
    ]
  }),
  entry({
    word: 'das Pathos', partOfSpeech: 'Noun', article: 'das', plural: null, syllableBreak: 'Pa-thos',
    topics: ['culture-and-media','advanced-analysis','business-communication'], usageLabels: ['formal','written','academic','analysis'],
    grammarNotes: ['neuter abstract noun; normally used in singular'],
    collocations: [{ text: 'mit Pathos sprechen', meaning: 'to speak with pathos' }],
    meanings: meaning('حماسة خطابية؛ شحنة عاطفية عالية','پاتۆس؛ هەستی بەرز و وتاری','pathos; elevated emotional force','پاتوس؛ شور و بار عاطفی والا','pathos; hêza hestyarî ya bilind','patos','pathos; încărcătură emoțională înaltă','пафос; патос','pathos; ngarkesë e lartë emocionale','pathos; yüksek duygusal etki'),
    examples: [
      ex('Das Pathos der Präsentation passte nicht zu den nüchternen Zahlen, die einen sehr begrenzten Fortschritt zeigten.', trans('لم يتناسب الحماس الخطابي في العرض مع الأرقام الجافة التي أظهرت تقدماً محدوداً جداً.','پاتۆسی پێشکەشکردنەکە لەگەڵ ژمارە ساردەکان نەگونجا، کە پێشکەوتنێکی زۆر سنوورداریان پیشان دەدا.','The pathos of the presentation did not match the sober figures, which showed very limited progress.','پاتوس ارائه با اعداد خشک که پیشرفت بسیار محدودی را نشان می‌دادند، همخوان نبود.','Pathosa pêşkêşkirinê bi hejmarên hişk re ne li hev bû, ku pêşketineke pir sînorkirî nîşan didan.','Patos prezentacji nie pasował do trzeźwych danych, które pokazywały bardzo ograniczony postęp.','Pathosul prezentării nu se potrivea cu cifrele seci, care arătau un progres foarte limitat.','Пафос презентации не соответствовал сухим цифрам, показывавшим весьма ограниченный прогресс.','Pathosi i prezantimit nuk përputhej me shifrat e thata, që tregonin përparim shumë të kufizuar.','Sunumun pathosu, çok sınırlı ilerleme gösteren kuru rakamlarla örtüşmüyordu.')),
      ex('Bei aller Kritik bleibt im Text ein Pathos der Freiheit spürbar, das nicht bloß rhetorischer Schmuck ist.', trans('رغم كل النقد يبقى في النص شعور خطابي بالحرية لا يكون مجرد زخرفة بلاغية.','لەگەڵ هەموو ڕەخنەکاندا، لە دەقەکەدا پاتۆسێکی ئازادی هەست پێدەکرێت کە تەنها ڕازاندنەوەی ڕەوانبێژی نییە.','For all its criticism, the text retains a pathos of freedom that is not merely rhetorical decoration.','با وجود همه نقدها، در متن پاتوسی از آزادی حس می‌شود که صرفاً آرایه‌ای بلاغی نیست.','Digel hemû rexneyan, di nivîsê de pathoseke azadiyê tê hestkirin ku tenê xemilandina retorîkî nîne.','Mimo całej krytyki w tekście wyczuwalny jest patos wolności, który nie jest jedynie retoryczną ozdobą.','Cu toată critica, în text rămâne perceptibil un pathos al libertății care nu este doar ornament retoric.','При всей критике в тексте ощущается пафос свободы, который не является лишь риторическим украшением.','Me gjithë kritikën, në tekst ndihet një pathos i lirisë që nuk është thjesht zbukurim retorik.','Tüm eleştiriye rağmen metinde, yalnızca retorik bir süs olmayan bir özgürlük pathosu hissedilir.'))
    ]
  }),
  entry({
    word: 'das Patiens', partOfSpeech: 'Noun', article: 'das', plural: 'Patiens', syllableBreak: 'Pa-ti-ens',
    topics: ['education-and-training','advanced-analysis','culture-and-media'], usageLabels: ['formal','written','academic','analysis'],
    grammarNotes: ['neuter linguistic term; denotes the participant affected by an action'],
    collocations: [{ text: 'Agens und Patiens unterscheiden', meaning: 'to distinguish agent and patient' }],
    meanings: meaning('المفعول الدلالي؛ المتأثر بالفعل','پاتیێنس؛ بەشداربووی کاری لێکراو','patient; affected participant in an action','پذیرنده کنش؛ مشارک اثرپذیر','patiens; beşdara bandor lê bûyî','patiens; uczestnik podlegający działaniu','patiens; participant afectat de acțiune','пациенс; участник, подвергающийся действию','patiens; pjesëmarrës i prekur nga veprimi','patiens; eylemden etkilenen katılımcı'),
    examples: [
      ex('In dem Satz „Der Techniker repariert den Server“ ist der Server das Patiens, weil er von der Handlung betroffen ist.', trans('في الجملة «الفني يصلح الخادم» يكون الخادم هو المتأثر بالفعل لأنه يتأثر بالإجراء.','لە ڕستەی «تەکنیککارەکە سێرڤەرەکە چاکدەکاتەوە» سێرڤەرەکە پاتیێنسە، چونکە کاری لێدەکرێت.','In the sentence “The technician repairs the server,” the server is the patient because it is affected by the action.','در جمله «تکنسین سرور را تعمیر می‌کند»، سرور پذیرنده کنش است، چون از عمل اثر می‌پذیرد.','Di hevoka “Teknîsyen serverê tamîr dike” de server patiens e, ji ber ku ji kiryarê bandor dibîne.','W zdaniu „Technik naprawia serwer” serwer jest patiensem, ponieważ podlega działaniu.','În propoziția „Tehnicianul repară serverul”, serverul este patiensul, deoarece este afectat de acțiune.','В предложении «Техник ремонтирует сервер» сервер является пациенсом, потому что на него направлено действие.','Në fjalinë “Tekniku riparon serverin”, serveri është patiens, sepse preket nga veprimi.','“Teknisyen sunucuyu onarıyor” cümlesinde sunucu patienstir, çünkü eylemden etkilenir.')),
      ex('Die Analyse der Rollen zeigt, dass das Patiens im Märchen nicht passiv bleiben muss, sondern den Verlauf indirekt steuern kann.', trans('يبين تحليل الأدوار أن المتأثر بالفعل في الحكاية لا يجب أن يبقى سلبياً، بل يمكنه توجيه المسار بصورة غير مباشرة.','شیکردنەوەی ڕۆڵەکان پیشان دەدات کە پاتیێنس لە چیرۆکی ئەفسانەییدا ناچار نییە پاسیو بمێنێتەوە، بەڵکو دەتوانێت بە ناڕاستەوخۆ ئاراستەی ڕووداوەکان دیاری بکات.','The analysis of roles shows that the patient in the fairy tale need not remain passive, but can indirectly shape the course of events.','تحلیل نقش‌ها نشان می‌دهد که پذیرنده کنش در افسانه لازم نیست منفعل بماند، بلکه می‌تواند مسیر رویدادها را غیرمستقیم هدایت کند.','Analîza rolan nîşan dide ku patiens di çîroka efsaneyî de ne pêwîst e pasîf bimîne, lê dikare bi awayekî nerasterast rêça bûyeran bişîne.','Analiza ról pokazuje, że patiens w baśni nie musi pozostawać bierny, lecz może pośrednio kierować przebiegiem zdarzeń.','Analiza rolurilor arată că patiensul din basm nu trebuie să rămână pasiv, ci poate influența indirect desfășurarea evenimentelor.','Анализ ролей показывает, что пациенс в сказке не обязан оставаться пассивным, а может косвенно управлять ходом событий.','Analiza e roleve tregon se patiens në përrallë nuk duhet të mbetet pasiv, por mund ta drejtojë rrjedhën në mënyrë të tërthortë.','Rol analizi, masaldaki patiensin pasif kalmak zorunda olmadığını, olayların akışını dolaylı olarak yönlendirebileceğini gösterir.'))
    ]
  }),
  entry({
    word: 'die Perspektivierung', partOfSpeech: 'Noun', article: 'die', plural: 'Perspektivierungen', syllableBreak: 'Per-spek-ti-vie-rung',
    topics: ['advanced-analysis','culture-and-media','business-communication'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'eine historische Perspektivierung', meaning: 'a historical framing or perspective-setting' }],
    wordFamilies: [{ lemma: 'perspektivieren', relationLabel: 'verb', note: null }],
    meanings: meaning('تأطير منظور؛ عرض من زاوية معينة','لە چوارچێوەی دیدگایەکدا دانان','perspectivization; framing from a perspective','چشم‌اندازدهی؛ طرح از منظر خاص','ji perspektîfekê ve çarçove kirin','perspektywizacja; ujęcie z perspektywy','perspectivizare; încadrare dintr-o perspectivă','перспективизация; представление с определенной точки зрения','perspektivizim; kornizim nga një këndvështrim','perspektifleştirme; belirli açıdan çerçeveleme'),
    examples: [
      ex('Die Perspektivierung der Risiken als rein technische Probleme verdeckte lange die organisatorischen Ursachen.', trans('إن تأطير المخاطر بوصفها مشكلات تقنية بحتة أخفى طويلاً الأسباب التنظيمية.','دانانی مەترسییەکان وەک کێشەی تەکنیکیی تەواو، بۆ ماوەیەکی درێژ هۆکارە ڕێکخراوەییەکانی داپۆشی.','Framing the risks as purely technical problems long obscured the organizational causes.','چشم‌اندازدهی ریسک‌ها به‌عنوان مشکلات صرفاً فنی، مدت‌ها علت‌های سازمانی را پنهان کرد.','Çarçovekirina xetereyan wek pirsgirêkên tenê teknîkî demek dirêj sedemên rêxistinî veşart.','Ujęcie ryzyk jako problemów czysto technicznych długo przesłaniało przyczyny organizacyjne.','Perspectivizarea riscurilor ca probleme pur tehnice a ascuns mult timp cauzele organizaționale.','Представление рисков как сугубо технических проблем долго скрывало организационные причины.','Kornizimi i rreziqeve si probleme thjesht teknike për një kohë të gjatë fshehu shkaqet organizative.','Risklerin tamamen teknik sorunlar olarak çerçevelenmesi, organizasyonel nedenleri uzun süre gizledi.')),
      ex('Erst die Perspektivierung durch die betroffene Minderheit veränderte die öffentliche Wahrnehmung des Konflikts.', trans('لم تتغير النظرة العامة إلى النزاع إلا حين قُدّم من منظور الأقلية المتضررة.','تەنها دانانی کێشەکە لە دیدگای کەمینەی زیانلێکەوتووەوە تێڕوانینی گشتیی بۆ ململاێکە گۆڕی.','Only the framing by the affected minority changed the public perception of the conflict.','تنها چشم‌اندازدهی از سوی اقلیت آسیب‌دیده برداشت عمومی از مناقشه را تغییر داد.','Tenê çarçovekirina ji aliyê kêmîneya bandor lê bûyî ve têgihiştina giştî ya nakokiyê guherand.','Dopiero ujęcie konfliktu z perspektywy dotkniętej mniejszości zmieniło jego społeczne postrzeganie.','Abia perspectivizarea de către minoritatea afectată a schimbat percepția publică asupra conflictului.','Только представление конфликта с точки зрения пострадавшего меньшинства изменило его общественное восприятие.','Vetëm kornizimi nga pakica e prekur ndryshoi perceptimin publik të konfliktit.','Çatışmanın etkilenen azınlık tarafından çerçevelenmesi, kamuoyundaki algıyı ancak o zaman değiştirdi.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 063', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

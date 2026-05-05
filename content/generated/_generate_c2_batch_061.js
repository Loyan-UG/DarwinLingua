const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '061';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['die Ohnmacht','oktroyieren','die Ontologie','opak','das Ordnungsprinzip','ornamental'];

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
    word: 'die Ohnmacht', partOfSpeech: 'Noun', article: 'die', plural: 'Ohnmachten', syllableBreak: 'Ohn-macht',
    topics: ['healthcare-and-appointments','social-and-relationships','advanced-analysis'], usageLabels: ['formal','written','sensitive','advanced'],
    grammarNotes: ['feminine noun; can mean unconsciousness or powerlessness'],
    collocations: [{ text: 'in Ohnmacht fallen', meaning: 'to faint' }, { text: 'politische Ohnmacht', meaning: 'political powerlessness' }],
    meanings: meaning('إغماء؛ عجز تام','هۆش لەدەستدان؛ بێدەسەڵاتی تەواو','fainting; powerlessness','بیهوشی؛ درماندگی و ناتوانی','hiş ji dest dan; bêdesthilatî','omdlenie; bezsilność','leșin; neputință','обморок; бессилие','të fikët; pafuqishmëri','bayılma; güçsüzlük'),
    examples: [
      ex('Nach der Sitzung beschrieb sie nicht nur Erschöpfung, sondern ein Gefühl völliger Ohnmacht gegenüber den Entscheidungen des Vorstands.', trans('بعد الاجتماع لم تصف الإرهاق فقط، بل شعوراً بالعجز الكامل أمام قرارات مجلس الإدارة.','دوای دانیشتنەکە نەک تەنها ماندووبوون، بەڵکو هەستی بێدەسەڵاتی تەواوی بەرامبەر بڕیارەکانی ئەنجومەنی بەڕێوەبەری وەسف کرد.','After the meeting, she described not only exhaustion but a feeling of complete powerlessness in the face of the board’s decisions.','بعد از جلسه، او نه فقط خستگی، بلکه احساس درماندگی کامل در برابر تصمیم‌های هیئت‌مدیره را توصیف کرد.','Piştî civînê wê ne tenê westîn, lê hesta bêdesthilatiya tevahî li hember biryarên desteya rêveberiyê vegot.','Po spotkaniu opisała nie tylko wyczerpanie, lecz także poczucie całkowitej bezsilności wobec decyzji zarządu.','După ședință, ea a descris nu doar epuizare, ci și un sentiment de neputință totală în fața deciziilor consiliului de administrație.','После заседания она описала не только истощение, но и чувство полного бессилия перед решениями правления.','Pas mbledhjes, ajo përshkroi jo vetëm lodhje, por edhe një ndjenjë pafuqie të plotë përballë vendimeve të bordit.','Toplantıdan sonra yalnızca yorgunluk değil, yönetim kurulunun kararları karşısında tam bir güçsüzlük hissi anlattı.')),
      ex('Als der Patient im Wartezimmer in Ohnmacht fiel, rief die Assistentin sofort den Notarzt.', trans('عندما أُغمي على المريض في غرفة الانتظار، اتصلت المساعدة فوراً بطبيب الطوارئ.','کاتێک نەخۆشەکە لە ژووری چاوەڕوانیدا هۆشی لەدەستدا، یاریدەدەرەکە دەستبەجێ پزیشکی فریاکەوتنی بانگ کرد.','When the patient fainted in the waiting room, the assistant immediately called the emergency doctor.','وقتی بیمار در اتاق انتظار بیهوش شد، دستیار فوراً پزشک اورژانس را خبر کرد.','Dema nexweş di odeya bendê de hişê xwe ji dest da, alîkar tavilê doktorê acîl bang kir.','Gdy pacjent zemdlał w poczekalni, asystentka natychmiast wezwała lekarza pogotowia.','Când pacientul a leșinat în sala de așteptare, asistenta a chemat imediat medicul de urgență.','Когда пациент упал в обморок в приемной, ассистентка сразу вызвала врача скорой помощи.','Kur pacientit i ra të fikët në dhomën e pritjes, asistentja thirri menjëherë mjekun e urgjencës.','Hasta bekleme odasında bayılınca asistan hemen acil doktorunu çağırdı.'))
    ]
  }),
  entry({
    word: 'oktroyieren', partOfSpeech: 'Verb', infinitive: 'oktroyieren', syllableBreak: 'ok-tro-yie-ren',
    topics: ['management-and-leadership','law-and-compliance','business-communication'], usageLabels: ['formal','written','advanced','sensitive'],
    collocations: [{ text: 'jemandem eine Lösung oktroyieren', meaning: 'to impose a solution on someone' }],
    meanings: meaning('يفرض قسراً؛ يملي من فوق','بەسەر کەسێکدا سەپاندن','to impose; to force upon','تحمیل کردن؛ از بالا دیکته کردن','bi zor ferz kirin; li ser kesekî danîn','narzucać; oktrojować','a impune; a dicta de sus','навязывать; октроировать','imponoj; detyroj nga lart','dayatmak; zorla kabul ettirmek'),
    examples: [
      ex('Die neue Governance wurde den Entwicklungsteams oktroyiert, ohne deren operative Erfahrung ernsthaft einzubeziehen.', trans('فُرضت الحوكمة الجديدة على فرق التطوير من دون إشراك خبرتها العملية بجدية.','حوکمڕانییە نوێیەکە بەسەر تیمەکانی پەرەپێداندا سەپێنرا، بەبێ ئەوەی ئەزموونی کارییان بە جدی لەبەرچاو بگیرێت.','The new governance was imposed on the development teams without seriously incorporating their operational experience.','حاکمیت جدید بدون در نظر گرفتن جدی تجربه عملی تیم‌های توسعه، به آن‌ها تحمیل شد.','Rêveberiya nû li ser tîmên pêşxistinê hat ferzkirin bê ku ezmûna wan a karî bi rastî bê tevlîkirin.','Nowy model zarządzania narzucono zespołom deweloperskim bez poważnego uwzględnienia ich doświadczenia operacyjnego.','Noua guvernanță a fost impusă echipelor de dezvoltare fără a integra serios experiența lor operațională.','Новую модель управления навязали командам разработки, не учитывая всерьез их операционный опыт.','Qeverisja e re iu imponua ekipeve të zhvillimit pa përfshirë seriozisht përvojën e tyre operative.','Yeni yönetişim modeli, operasyonel deneyimleri ciddi biçimde dikkate alınmadan geliştirme ekiplerine dayatıldı.')),
      ex('Ein tragfähiger Kompromiss lässt sich nicht oktroyieren; er muss von den Beteiligten als legitim anerkannt werden.', trans('لا يمكن فرض تسوية قابلة للاستمرار؛ يجب أن يعترف بها المشاركون بوصفها مشروعة.','ڕێککەوتنێکی پایەدار ناکرێت بسەپێندرێت؛ دەبێت لەلایەن بەشداربووانەوە وەک شتێکی ڕەوا بناسرێت.','A viable compromise cannot be imposed; it must be recognized as legitimate by those involved.','مصالحه پایدار را نمی‌توان تحمیل کرد؛ باید از سوی طرف‌های درگیر مشروع دانسته شود.','Lihevhatineke domdar nayê ferzkirin; divê ji aliyê beşdaran ve wek rewa bê pejirandin.','Trwałego kompromisu nie da się narzucić; uczestnicy muszą uznać go za prawomocny.','Un compromis viabil nu poate fi impus; el trebuie recunoscut ca legitim de participanți.','Жизнеспособный компромисс нельзя навязать; участники должны признать его легитимным.','Një kompromis i qëndrueshëm nuk mund të imponohet; ai duhet të pranohet si legjitim nga palët.','Sürdürülebilir bir uzlaşma dayatılamaz; taraflarca meşru görülmesi gerekir.'))
    ]
  }),
  entry({
    word: 'die Ontologie', partOfSpeech: 'Noun', article: 'die', plural: 'Ontologien', syllableBreak: 'On-to-lo-gie',
    topics: ['advanced-analysis','education-and-training','technology-and-it'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'eine Ontologie modellieren', meaning: 'to model an ontology' }],
    meanings: meaning('علم الوجود؛ أنطولوجيا','ئۆنتۆلۆژیا؛ زانستی بوون','ontology','هستی‌شناسی؛ آنتولوژی','ontolojî; zanista hebûnê','ontologia','ontologie','онтология','ontologji','ontoloji'),
    examples: [
      ex('Für die Wissensdatenbank modellierte das Team eine Ontologie, damit Begriffe wie Kunde, Vertrag und Servicefall eindeutig verknüpft werden.', trans('من أجل قاعدة المعرفة صمّم الفريق أنطولوجيا كي تُربط مفاهيم مثل العميل والعقد وحالة الخدمة بوضوح.','بۆ بنکەداتای زانیاری، تیمەکە ئۆنتۆلۆژیایەکی مۆدێل کرد بۆ ئەوەی چەمکەکانی وەک کڕیار، گرێبەست و کەیسی خزمەتگوزاری بە ڕوونی پەیوەست بن.','For the knowledge base, the team modeled an ontology so that terms such as customer, contract, and service case are linked unambiguously.','برای پایگاه دانش، تیم یک آنتولوژی مدل‌سازی کرد تا مفاهیمی مانند مشتری، قرارداد و پرونده خدمات به‌طور دقیق به هم متصل شوند.','Ji bo bingeha zanînê, tîmê ontolojiyek model kir da ku têgehên wek xerîdar, peyman û doza xizmetê bi zelalî werin girêdan.','Na potrzeby bazy wiedzy zespół zamodelował ontologię, aby pojęcia takie jak klient, umowa i sprawa serwisowa były jednoznacznie powiązane.','Pentru baza de cunoștințe, echipa a modelat o ontologie astfel încât termeni precum client, contract și caz de service să fie legați fără ambiguitate.','Для базы знаний команда смоделировала онтологию, чтобы такие понятия, как клиент, договор и сервисный случай, были однозначно связаны.','Për bazën e njohurive, ekipi modeloi një ontologji që termat si klient, kontratë dhe rast shërbimi të lidhen pa paqartësi.','Bilgi tabanı için ekip, müşteri, sözleşme ve servis vakası gibi kavramların açıkça ilişkilendirilmesi amacıyla bir ontoloji modelledi.')),
      ex('In der klassischen Philosophie fragt die Ontologie danach, was überhaupt als seiend gelten kann.', trans('في الفلسفة الكلاسيكية تسأل الأنطولوجيا عمّا يمكن اعتباره موجوداً أصلاً.','لە فەلسەفەی کلاسیکدا ئۆنتۆلۆژیا دەپرسیار دەکات کە چی بە بنەڕەت دەتوانێت وەک بوون هەژمار بکرێت.','In classical philosophy, ontology asks what can count as existing at all.','در فلسفه کلاسیک، هستی‌شناسی می‌پرسد اساساً چه چیزی را می‌توان موجود دانست.','Di felsefeya klasîk de ontolojî dipirse ka çi dikare bi rastî wek hebûyî bê hesibandin.','W filozofii klasycznej ontologia pyta, co w ogóle można uznać za istniejące.','În filosofia clasică, ontologia întreabă ce poate fi considerat în genere ca existând.','В классической философии онтология спрашивает, что вообще может считаться существующим.','Në filozofinë klasike, ontologjia pyet se çfarë mund të konsiderohet si ekzistuese.','Klasik felsefede ontoloji, neyin var sayılabileceğini sorgular.'))
    ]
  }),
  entry({
    word: 'opak', partOfSpeech: 'Adjective', syllableBreak: 'o-pak',
    topics: ['advanced-analysis','business-communication','technology-and-it'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'ein opaker Entscheidungsprozess', meaning: 'an opaque decision-making process' }],
    meanings: meaning('معتم؛ غير شفاف','تاریک؛ نەڕوون','opaque; non-transparent','کدر؛ غیرشفاف','tarî; nezelal','nieprzejrzysty; nieprzezroczysty','opac; netransparent','непрозрачный; неясный','opak; jo transparent','opak; şeffaf olmayan'),
    examples: [
      ex('Der Algorithmus blieb für die Fachabteilung opak, obwohl seine Entscheidungen direkte Auswirkungen auf Kundenanfragen hatten.', trans('بقيت الخوارزمية غير شفافة للقسم المختص، رغم أن قراراتها أثرت مباشرة في طلبات العملاء.','ئەگۆریتمەکە بۆ بەشی پسپۆڕی نەڕوون مایەوە، هەرچەندە بڕیارەکانی کاریگەریی ڕاستەوخۆیان لەسەر داواکارییەکانی کڕیاران هەبوو.','The algorithm remained opaque to the specialist department, although its decisions directly affected customer requests.','الگوریتم برای بخش تخصصی غیرشفاف باقی ماند، با اینکه تصمیم‌هایش مستقیماً بر درخواست‌های مشتریان اثر داشت.','Algorîtma ji bo beşa pisporî nezelal ma, herçend biryarên wê rasterast li ser daxwazên xerîdaran bandor dikirin.','Algorytm pozostał nieprzejrzysty dla działu merytorycznego, choć jego decyzje bezpośrednio wpływały na zapytania klientów.','Algoritmul a rămas opac pentru departamentul de specialitate, deși deciziile lui afectau direct solicitările clienților.','Алгоритм оставался непрозрачным для профильного отдела, хотя его решения напрямую влияли на запросы клиентов.','Algoritmi mbeti opak për departamentin specialist, megjithëse vendimet e tij ndikonin drejtpërdrejt në kërkesat e klientëve.','Algoritma, kararları müşteri taleplerini doğrudan etkilemesine rağmen uzman departman için opak kaldı.')),
      ex('Die Erzählung wirkt absichtlich opak, weil sie entscheidende Informationen erst sehr spät freigibt.', trans('تبدو السردية معتمة عمداً لأنها لا تكشف المعلومات الحاسمة إلا في وقت متأخر جداً.','گێڕانەوەکە بە ئەنقەست تاریک دەردەکەوێت، چونکە زانیاریی گرنگ تەنها زۆر درەنگ ئاشکرا دەکات.','The narrative feels deliberately opaque because it releases crucial information only very late.','روایت عمداً مبهم به نظر می‌رسد، چون اطلاعات تعیین‌کننده را بسیار دیر آشکار می‌کند.','Vegotin bi mebest nezelal xuya dike, ji ber ku agahiyên girîng tenê pir dereng eşkere dike.','Narracja wydaje się celowo nieprzejrzysta, ponieważ kluczowe informacje ujawnia dopiero bardzo późno.','Narațiunea pare intenționat opacă, deoarece dezvăluie informațiile decisive abia foarte târziu.','Повествование кажется намеренно непрозрачным, потому что ключевую информацию раскрывает лишь очень поздно.','Rrëfimi duket qëllimisht opak, sepse informacionet vendimtare i zbulon shumë vonë.','Anlatı, belirleyici bilgileri çok geç verdiği için kasıtlı olarak opak görünür.'))
    ]
  }),
  entry({
    word: 'das Ordnungsprinzip', partOfSpeech: 'Noun', article: 'das', plural: 'Ordnungsprinzipien', syllableBreak: 'Ord-nungs-prin-zip',
    topics: ['advanced-analysis','management-and-leadership','data-and-reporting'], usageLabels: ['formal','written','analysis','academic'],
    collocations: [{ text: 'als Ordnungsprinzip dienen', meaning: 'to serve as an organizing principle' }],
    meanings: meaning('مبدأ تنظيمي؛ قاعدة ترتيب','بنەمای ڕێکخستن','organizing principle; ordering principle','اصل سامان‌دهنده؛ اصل نظم‌بخش','prensîba rêxistinê','zasada porządkująca','principiu ordonator; principiu de organizare','организующий принцип','parim organizues','düzenleyici ilke'),
    examples: [
      ex('In der neuen Datenarchitektur dient die Kundenbeziehung als zentrales Ordnungsprinzip, nicht mehr die einzelne Rechnung.', trans('في بنية البيانات الجديدة تُعد علاقة العميل مبدأ التنظيم المركزي، لا الفاتورة الفردية.','لە ئەندازیاریی نوێی داتادا پەیوەندیی کڕیار وەک بنەمای ڕێکخستنی سەرەکی کاردەکات، نە چیتر فاکتورە تاکەکە.','In the new data architecture, the customer relationship serves as the central organizing principle, no longer the individual invoice.','در معماری داده جدید، رابطه با مشتری اصل سامان‌دهنده مرکزی است، نه هر فاکتور جداگانه.','Di avahiya nû ya daneyan de têkiliya xerîdarê wek prensîba sereke ya rêxistinê kar dike, ne êdî fatûreya yekane.','W nowej architekturze danych centralną zasadą porządkującą jest relacja z klientem, a nie pojedyncza faktura.','În noua arhitectură de date, relația cu clientul servește drept principiu central de organizare, nu factura individuală.','В новой архитектуре данных центральным организующим принципом служит связь с клиентом, а не отдельный счет.','Në arkitekturën e re të të dhënave, marrëdhënia me klientin shërben si parimi qendror organizues, jo më fatura e veçantë.','Yeni veri mimarisinde merkezi düzenleyici ilke artık tek tek faturalar değil, müşteri ilişkisidir.')),
      ex('Für die Ausstellung wählte die Kuratorin nicht die Chronologie, sondern das Motiv der Wiederkehr als Ordnungsprinzip.', trans('اختارت القيّمة للمعرض ليس التسلسل الزمني، بل فكرة العَوْدة كمبدأ تنظيمي.','بۆ پێشانگاکە، سەرپەرشتیارەکە نە ڕیزبەندیی کات، بەڵکو مۆتیفی گەڕانەوەی وەک بنەمای ڕێکخستن هەڵبژارد.','For the exhibition, the curator chose not chronology but the motif of recurrence as the organizing principle.','برای نمایشگاه، کیوریتور نه ترتیب زمانی، بلکه مضمون بازگشت را به‌عنوان اصل سامان‌دهنده انتخاب کرد.','Ji bo pêşangehê, kurator ne kronolojî, lê motîfa vegerê wek prensîba rêxistinê hilbijart.','Kuratorka wybrała dla wystawy nie chronologię, lecz motyw powrotu jako zasadę porządkującą.','Pentru expoziție, curatoarea a ales nu cronologia, ci motivul revenirii ca principiu de organizare.','Для выставки куратор выбрала в качестве организующего принципа не хронологию, а мотив возвращения.','Për ekspozitën, kuratorja zgjodhi jo kronologjinë, por motivin e rikthimit si parim organizues.','Küratör sergi için kronolojiyi değil, tekrar motifini düzenleyici ilke olarak seçti.'))
    ]
  }),
  entry({
    word: 'ornamental', partOfSpeech: 'Adjective', syllableBreak: 'or-na-men-tal',
    topics: ['culture-and-media','housing-and-real-estate','advanced-analysis'], usageLabels: ['formal','written','advanced','analysis'],
    collocations: [{ text: 'eine ornamentale Gestaltung', meaning: 'an ornamental design' }],
    meanings: meaning('زخرفي؛ تزييني','ڕازاندنەوەیی؛ زینەتی','ornamental; decorative','تزئینی؛ آرایه‌ای','xemilandî; xemlî','ornamentalny; dekoracyjny','ornamental; decorativ','орнаментальный; декоративный','ornamental; dekorativ','süslemeli; dekoratif'),
    examples: [
      ex('Die Fassade wirkt ornamental, ohne die klare Struktur des Gebäudes zu überdecken.', trans('تبدو الواجهة زخرفية من دون أن تحجب البنية الواضحة للمبنى.','ڕووی بیناکە زینەتی دەردەکەوێت، بەبێ ئەوەی پێکهاتەی ڕوونی بیناکە داپۆشێت.','The facade appears ornamental without obscuring the building’s clear structure.','نما تزئینی به نظر می‌رسد، بدون آن‌که ساختار روشن ساختمان را بپوشاند.','Rûyê avahiyê xemilandî xuya dike bê ku avahiya zelal a avahiyê veşêre.','Fasada ma charakter ornamentalny, nie przesłaniając klarownej struktury budynku.','Fațada pare ornamentală fără să acopere structura clară a clădirii.','Фасад выглядит декоративным, не скрывая четкую структуру здания.','Fasada duket ornamentale pa mbuluar strukturën e qartë të ndërtesës.','Cephe, binanın net yapısını örtmeden dekoratif görünüyor.')),
      ex('In seinem Essay kritisiert er eine Sprache, die nur ornamental glänzt, aber keine analytische Arbeit leistet.', trans('ينتقد في مقاله لغة تتألق زخرفياً فقط، لكنها لا تؤدي أي عمل تحليلي.','لە وتارەکەیدا ڕەخنە لە زمانێک دەگرێت کە تەنها بە زینەت دەدرەوشێتەوە، بەڵام هیچ کاری شیکاری ناکات.','In his essay, he criticizes language that merely shines ornamentally but does no analytical work.','او در مقاله‌اش زبانی را نقد می‌کند که فقط به‌صورت تزئینی می‌درخشد، اما کار تحلیلی انجام نمی‌دهد.','Di gotara xwe de ew rexne li zimanekî digire ku tenê bi xemil dibiriqe, lê karekî analîtîk nake.','W swoim eseju krytykuje język, który jedynie ornamentalnie błyszczy, ale nie wykonuje pracy analitycznej.','În eseul său, el critică un limbaj care doar strălucește ornamental, dar nu face muncă analitică.','В своем эссе он критикует язык, который лишь декоративно блестит, но не выполняет аналитической работы.','Në esenë e tij, ai kritikon një gjuhë që vetëm shkëlqen në mënyrë ornamentale, por nuk kryen punë analitike.','Denemesinde yalnızca süslü biçimde parlayan ama analitik bir iş yapmayan bir dili eleştirir.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 061', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

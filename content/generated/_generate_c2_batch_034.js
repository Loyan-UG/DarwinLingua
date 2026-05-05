const fs = require('fs');
const cp = require('child_process');
const path = require('path');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '034';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const expected = ['fassen', 'das Faszinosum', 'der Fehlschluss', 'die Feingliedrigkeit', 'der Feinsinn', 'feinsinnig'];
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const labelMap = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const topicSet = new Set((taxonomy.topics || []).map(t => t.key));
const splitTokens = text => text.split(',').map(t => t.trim()).filter(Boolean);
const tokens = splitTokens(fs.readFileSync(sourcePath, 'utf8'));
const first = tokens.slice(0, expected.length);
if (JSON.stringify(first) !== JSON.stringify(expected)) throw new Error(`Source token mismatch. Expected ${JSON.stringify(expected)} but found ${JSON.stringify(first)}`);
function m(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) { return [
  {language:'ar', text:ar}, {language:'ckb', text:ckb}, {language:'en', text:en}, {language:'fa', text:fa}, {language:'kmr', text:kmr},
  {language:'pl', text:pl}, {language:'ro', text:ro}, {language:'ru', text:ru}, {language:'sq', text:sq}, {language:'tr', text:tr}
]; }
function ex(baseText, translations) { return { baseText, translations }; }
const entries = [
  {
    word:'fassen', language:'de', cefrLevel:level, partOfSpeech:'Verb', article:null, plural:null, infinitive:'fassen', pronunciationIpa:null, syllableBreak:'fas-sen',
    topics:['advanced-analysis','business-communication','culture-and-media'], usageLabels:['formal','written','analysis'], contextLabels:[],
    grammarNotes:['transitive verb; at advanced level often means to formulate, comprehend, or put into a certain form'],
    collocations:[{text:'einen Gedanken in Worte fassen', meaning:'to put a thought into words'}],
    wordFamilies:[{lemma:'die Fassung', relationLabel:'noun', note:null}], relations:[],
    meanings:m('يصوغ؛ يستوعب؛ يضع في صيغة محددة','داڕشتن؛ تێگەیشتن؛ خستنە ناو شێوەیەک','to formulate; to grasp; to put into form','صورت‌بندی کردن؛ درک کردن؛ در قالبی آوردن','formulê kirin; fêm kirin; di şêweyekê de danîn','ująć; pojąć; sformułować','a formula; a înțelege; a pune într-o formă','формулировать; постигать; облекать в форму','të formulosh; të kapësh kuptimin; të vësh në formë','ifade etmek; kavramak; biçime sokmak'),
    examples:[
      ex('Die Juristin musste den Kompromiss so fassen, dass beide Seiten ihr Gesicht wahren konnten.', m('كان على المحامية أن تصوغ التسوية بطريقة تتيح للطرفين حفظ ماء الوجه.', 'یاساناسەکە دەبوو سازشەکە وا دابڕێژێت کە هەردوو لا بتوانن ڕووی خۆیان بپارێزن.', 'The lawyer had to formulate the compromise so that both sides could save face.', 'حقوقدان باید سازش را طوری صورت‌بندی می‌کرد که هر دو طرف بتوانند آبروی خود را حفظ کنند.', 'Parêzer diviya lihevhatinê wisa formulê bike ku her du alî bikarin rûyê xwe biparêzin.', 'Prawniczka musiała ująć kompromis tak, aby obie strony mogły zachować twarz.', 'Jurista trebuia să formuleze compromisul astfel încât ambele părți să își poată salva imaginea.', 'Юристке нужно было сформулировать компромисс так, чтобы обе стороны могли сохранить лицо.', 'Juristja duhej ta formulonte kompromisin në mënyrë që të dyja palët të ruanin fytyrën.', 'Hukukçu, uzlaşmayı iki tarafın da itibarını koruyabileceği şekilde ifade etmek zorundaydı.')),
      ex('Der Verlust war so abrupt, dass sie ihn zunächst weder emotional noch sprachlich fassen konnte.', m('كان الفقدان مفاجئاً إلى درجة أنها لم تستطع في البداية استيعابه لا عاطفياً ولا لغوياً.', 'لەدەستدانەکە ئەوەندە لەناکاو بوو کە سەرەتا نە بە هەست و نە بە زمان نەیتوانی بیگرێتەوە.', 'The loss was so abrupt that at first she could grasp it neither emotionally nor in words.', 'فقدان آن‌قدر ناگهانی بود که او ابتدا نه از نظر عاطفی و نه در کلمات نمی‌توانست آن را درک کند.', 'Windabûn ewqas ji nişkê ve bû ku wê di destpêkê de ne bi hestî ne jî bi peyvan nikarî wê fêm bike.', 'Strata była tak nagła, że początkowo nie potrafiła jej pojąć ani emocjonalnie, ani językowo.', 'Pierderea a fost atât de bruscă încât la început nu o putea cuprinde nici emoțional, nici în cuvinte.', 'Потеря была настолько внезапной, что сначала она не могла осмыслить ее ни эмоционально, ни словами.', 'Humbja ishte aq e papritur sa fillimisht ajo nuk mund ta kapte as emocionalisht, as me fjalë.', 'Kayıp o kadar ani oldu ki başlangıçta onu ne duygusal olarak ne de sözcüklerle kavrayabildi.'))
    ]
  },
  {
    word:'das Faszinosum', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'das', plural:'Faszinosen', infinitive:null, pronunciationIpa:null, syllableBreak:'Fas-zi-no-sum',
    topics:['culture-and-media','advanced-analysis','social-and-relationships'], usageLabels:['formal','written','academic','advanced'], contextLabels:[],
    grammarNotes:['neuter noun; specialized term for something that exerts fascination'], collocations:[{text:'das Faszinosum des Fremden', meaning:'the fascination of the unfamiliar'}],
    wordFamilies:[{lemma:'faszinieren', relationLabel:'verb', note:null},{lemma:'faszinierend', relationLabel:'adjective', note:null}], relations:[],
    meanings:m('ما يثير الانجذاب الشديد؛ عنصر الفتنة','ئەو شتەی سەرنجڕاکێشییەکی بەهێز دروست دەکات','fascinating phenomenon; source of fascination','امر جذاب و مسحورکننده؛ منشأ شیفتگی','tişta balkêşker; çavkaniya ecêbmayînê','fascynujące zjawisko; źródło fascynacji','fenomen fascinant; sursă de fascinație','завораживающее явление; источник притяжения','fenomen magjepsës; burim magjepsjeje','büyüleyici olgu; hayranlık kaynağı'),
    examples:[
      ex('Das Faszinosum dieser Technologie liegt darin, dass sie Nähe simuliert und zugleich Distanz erzeugt.', m('يكمن عنصر الجاذبية في هذه التقنية في أنها تحاكي القرب وتنتج في الوقت نفسه مسافة.', 'سەرنجڕاکێشی ئەم تەکنەلۆژیایە لەوەدایە کە نزیکی لاسایی دەکاتەوە و لە هەمان کاتدا دووری دروست دەکات.', 'The fascination of this technology lies in the fact that it simulates closeness while also creating distance.', 'جذابیت این فناوری در این است که نزدیکی را شبیه‌سازی می‌کند و هم‌زمان فاصله می‌سازد.', 'Balkêşiya vê teknolojiyê di wê de ye ku nêzîkbûnê şibandin dike û di heman demê de dûrbûnê diafirîne.', 'Fascynacja tą technologią polega na tym, że symuluje bliskość, a jednocześnie wytwarza dystans.', 'Fascinația acestei tehnologii constă în faptul că simulează apropierea și în același timp produce distanță.', 'Притягательность этой технологии в том, что она имитирует близость и одновременно создает дистанцию.', 'Magjepsja e kësaj teknologjie qëndron në faktin se simulon afërsi dhe njëkohësisht krijon distancë.', 'Bu teknolojinin büyüsü, yakınlığı taklit ederken aynı anda mesafe yaratmasında yatar.')),
      ex('Für die Ausstellungsmacher war das Faszinosum der Handschrift wichtiger als ihr rein dokumentarischer Wert.', m('بالنسبة لمنظمي المعرض، كان سحر المخطوطة أهم من قيمتها التوثيقية الخالصة.', 'بۆ ئامادەکارانی پێشانگاکە، سەرنجڕاکێشی دەستنووسەکە گرنگتر بوو لە بەهای تەنها بەڵگەییەکەی.', 'For the exhibition curators, the fascination of the handwriting was more important than its purely documentary value.', 'برای برگزارکنندگان نمایشگاه، جذابیت دست‌خط از ارزش صرفاً مستند آن مهم‌تر بود.', 'Ji bo amadekarên pêşangehê, balkêşiya destnivîsê ji nirxa wê ya tenê belgeyî girîngtir bû.', 'Dla twórców wystawy fascynacja pismem odręcznym była ważniejsza niż jego czysto dokumentalna wartość.', 'Pentru organizatorii expoziției, fascinația scrisului de mână era mai importantă decât valoarea lui pur documentară.', 'Для создателей выставки притягательность почерка была важнее его чисто документальной ценности.', 'Për kuratorët e ekspozitës, magjepsja e dorëshkrimit ishte më e rëndësishme se vlera e tij thjesht dokumentare.', 'Sergiyi hazırlayanlar için el yazısının büyüsü, onun salt belgesel değerinden daha önemliydi.'))
    ]
  },
  {
    word:'der Fehlschluss', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'der', plural:'Fehlschlüsse', infinitive:null, pronunciationIpa:null, syllableBreak:'Fehl-schluss',
    topics:['advanced-analysis','education-and-training','quality-and-risk'], usageLabels:['formal','written','academic','analysis'], contextLabels:[],
    grammarNotes:['masculine noun; used in logic, argumentation, and analysis'], collocations:[{text:'einem Fehlschluss unterliegen', meaning:'to be subject to a fallacy'}],
    wordFamilies:[{lemma:'fehl-', relationLabel:'prefix', note:'indicates error or wrongness'}, {lemma:'schließen', relationLabel:'verb', note:null}], relations:[],
    meanings:m('استنتاج خاطئ؛ مغالطة','ئەنجامگیریی هەڵە؛ فەڵسەفەی هەڵە','fallacy; false conclusion','نتیجه‌گیری نادرست؛ مغالطه','encamgiriya şaş; xeletîya mantiqî','błędny wniosek; fałsz logiczny','concluzie greșită; eroare logică','ложный вывод; логическая ошибка','përfundim i gabuar; gabim logjik','yanlış çıkarım; safsata'),
    examples:[
      ex('Aus wenigen Kundenbeschwerden auf ein generelles Produktversagen zu schließen, wäre ein methodischer Fehlschluss.', m('الاستنتاج من بضع شكاوى عملاء أن المنتج فشل عموماً سيكون مغالطة منهجية.', 'ئەنجامگیری لە چەند سکاڵای کڕیارەوە بۆ ئەوەی بەرهەمەکە بە گشتی شکستیهێناوە، فەڵسەفەیەکی میتۆدی هەڵە دەبێت.', 'Inferring general product failure from a few customer complaints would be a methodological fallacy.', 'نتیجه گرفتن شکست کلی محصول از چند شکایت مشتری، یک مغالطه روش‌شناختی خواهد بود.', 'Ji çend gilîyên xerîdaran encam girtin ku hilber bi giştî têk çûye, dê xeletiyeke metodolojîk be.', 'Wnioskowanie o ogólnej wadliwości produktu na podstawie kilku skarg klientów byłoby błędem metodologicznym.', 'A deduce un eșec general al produsului din câteva reclamații ale clienților ar fi o eroare metodologică.', 'Делать вывод об общем провале продукта на основании нескольких жалоб клиентов было бы методологической ошибкой.', 'Të nxjerrësh përfundimin për dështim të përgjithshëm të produktit nga disa ankesa klientësh do të ishte gabim metodologjik.', 'Birkaç müşteri şikayetinden genel bir ürün başarısızlığı sonucu çıkarmak yöntemsel bir safsata olurdu.')),
      ex('Der Fehlschluss bestand darin, Korrelation stillschweigend mit Kausalität gleichzusetzen.', m('تمثلت المغالطة في مساواة الارتباط بالسببية بشكل ضمني.', 'هەڵەکە لەوەدا بوو کە پەیوەندیی ئاماری بە بێدەنگی وەک هۆکاریی دانرا.', 'The fallacy consisted in silently equating correlation with causality.', 'مغالطه در این بود که همبستگی بی‌سر و صدا با علیت یکی گرفته شد.', 'Xeletî di wê de bû ku têkiliya amari bêdeng wek sedemîtî hate hesibandin.', 'Błąd polegał na milczącym utożsamieniu korelacji z przyczynowością.', 'Eroarea a constat în echivalarea tacită a corelației cu cauzalitatea.', 'Ошибка состояла в молчаливом отождествлении корреляции с причинностью.', 'Gabimi qëndronte në barazimin e heshtur të korrelacionit me shkakësinë.', 'Safsata, korelasyonu sessizce nedensellikle eşitlemekten ibaretti.'))
    ]
  },
  {
    word:'die Feingliedrigkeit', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'die', plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'Fein-glied-rig-keit',
    topics:['advanced-analysis','culture-and-media','data-and-reporting'], usageLabels:['formal','written','analysis','advanced'], contextLabels:[],
    grammarNotes:['feminine abstract noun; plural is uncommon'], collocations:[{text:'die Feingliedrigkeit einer Struktur', meaning:'the fine articulation of a structure'}],
    wordFamilies:[{lemma:'feingliedrig', relationLabel:'adjective', note:null}], relations:[],
    meanings:m('دقة التركيب؛ تفصيلية البنية','وردی و پێکهاتەی هەستیار','fine structure; delicate articulation','ظرافت ساختاری؛ ریزبافتگی','avahiya nazik; hûrgiliyên pêkhatinê','drobna struktura; delikatne zróżnicowanie','finețe structurală; articulare delicată','тонкая структура; детальная члененность','strukturë e imët; artikulim i hollë','ince yapı; hassas ayrımlanma'),
    examples:[
      ex('Die Feingliedrigkeit des Berechtigungsmodells erleichtert Audits, erhöht aber den Pflegeaufwand.', m('تسهّل دقة نموذج الصلاحيات عمليات التدقيق، لكنها تزيد جهد الصيانة.', 'وردی مۆدێلی مافەکان وردبینییەکان ئاسان دەکات، بەڵام تێچووی چاکسازی زیاد دەکات.', 'The fine-grained structure of the permission model makes audits easier, but increases maintenance effort.', 'ریزساختاری مدل دسترسی‌ها ممیزی را آسان‌تر می‌کند، اما هزینه نگهداری را بالا می‌برد.', 'Hûrgiliya modela destûran kontrolan hêsan dike, lê hewldana parastinê zêde dike.', 'Drobiazgowość modelu uprawnień ułatwia audyty, ale zwiększa nakład utrzymania.', 'Finețea modelului de permisiuni facilitează auditurile, dar crește efortul de întreținere.', 'Детальная структура модели прав упрощает аудиты, но увеличивает затраты на сопровождение.', 'Imtësia e modelit të lejeve i lehtëson auditimet, por rrit përpjekjen e mirëmbajtjes.', 'Yetki modelinin ince yapısı denetimleri kolaylaştırır, ancak bakım yükünü artırır.')),
      ex('Die Feingliedrigkeit der Komposition zeigt sich erst, wenn man die leisen Übergänge zwischen den Stimmen hört.', m('لا تظهر دقة بنية التأليف إلا عندما يسمع المرء الانتقالات الهادئة بين الأصوات.', 'وردی پێکهاتەی مۆسیقاکە تەنها کاتێک دەردەکەوێت کە گوێ لە گواستنەوە بێدەنگەکانی نێوان دەنگەکان بگیرێت.', 'The fine structure of the composition becomes apparent only when one hears the quiet transitions between the voices.', 'ظرافت ساختار قطعه فقط وقتی آشکار می‌شود که گذارهای آرام میان صداها شنیده شود.', 'Hûrgiliya kompozîsyonê tenê wê demê diyar dibe ku mirov guherînên hêdî yên di navbera dengan de bibihîze.', 'Drobna struktura kompozycji ujawnia się dopiero wtedy, gdy słyszy się ciche przejścia między głosami.', 'Finețea compoziției devine vizibilă abia când auzi trecerile discrete dintre voci.', 'Тонкая структура композиции проявляется лишь тогда, когда слышишь тихие переходы между голосами.', 'Struktura e imët e kompozimit shfaqet vetëm kur dëgjohen kalimet e qeta mes zërave.', 'Bestenin ince yapısı ancak sesler arasındaki sessiz geçişler duyulduğunda ortaya çıkar.'))
    ]
  },
  {
    word:'der Feinsinn', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'der', plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'Fein-sinn',
    topics:['social-and-relationships','culture-and-media','business-communication'], usageLabels:['formal','written','advanced'], contextLabels:[],
    grammarNotes:['masculine abstract noun; plural is uncommon'], collocations:[{text:'ästhetischer Feinsinn', meaning:'aesthetic sensitivity'}],
    wordFamilies:[{lemma:'feinsinnig', relationLabel:'adjective', note:null}], relations:[],
    meanings:m('حس مرهف؛ ذوق دقيق','هەستی ناسک؛ تێگەیشتنی ورد','sensitivity; refined discernment','حس ظریف؛ ذوق و درک دقیق','hestiyariya nazik; têgihiştina hûr','subtelność; wyczucie','finețe; sensibilitate rafinată','тонкое чутье; изысканная восприимчивость','ndjeshmëri e hollë; shije e rafinuar','ince sezgi; zarif duyarlılık'),
    examples:[
      ex('Im Konfliktgespräch bewies sie Feinsinn, indem sie den Vorwurf nicht zurückwies, sondern präzise umformulierte.', m('في حديث النزاع أظهرت حساً مرهفاً عندما لم ترفض الاتهام، بل أعادت صياغته بدقة.', 'لە گفتوگۆی ناکۆکیدا هەستی وردی نیشان دا، کاتێک تۆمەتەکەی ڕەت نەکردەوە بەڵکو بە وردی دووبارەی داڕشت.', 'In the conflict discussion, she showed sensitivity by not rejecting the accusation, but reformulating it precisely.', 'در گفت‌وگوی تعارض، او ظرافت نشان داد؛ اتهام را رد نکرد، بلکه دقیق بازصورت‌بندی کرد.', 'Di axaftina nakokiyê de wê hestiyariya hûr nîşan da, bi wê yekê ku tawanbarî red nekir lê bi rastî ji nû ve formulê kir.', 'W rozmowie konfliktowej wykazała się subtelnością, nie odrzucając zarzutu, lecz precyzyjnie go przeformułowując.', 'În discuția conflictuală a dat dovadă de finețe, nu respingând acuzația, ci reformulând-o precis.', 'В конфликтном разговоре она проявила тонкость, не отвергнув обвинение, а точно переформулировав его.', 'Në bisedën konfliktuale ajo tregoi ndjeshmëri, duke mos e hedhur poshtë akuzën, por duke e riformuluar saktë.', 'Çatışma görüşmesinde suçlamayı reddetmek yerine onu dikkatle yeniden ifade ederek incelik gösterdi.')),
      ex('Der Feinsinn des Übersetzers zeigt sich in kleinen Verschiebungen, die den Ton des Originals bewahren.', m('تظهر دقة ذوق المترجم في تغييرات صغيرة تحافظ على نبرة النص الأصلي.', 'هەستی وردی وەرگێڕ لە گۆڕانکارییە بچووکەکاندا دەردەکەوێت کە تۆنی دەقی ڕەسەن دەپارێزن.', 'The translator’s refined sensitivity appears in small shifts that preserve the tone of the original.', 'ظرافت مترجم در جابه‌جایی‌های کوچکی آشکار می‌شود که لحن متن اصلی را حفظ می‌کنند.', 'Hestiyariya nazik a wergêr di guherînên biçûk de xuya dike ku tona orîjînalê diparêzin.', 'Subtelność tłumacza ujawnia się w drobnych przesunięciach, które zachowują ton oryginału.', 'Finețea traducătorului se vede în mici deplasări care păstrează tonul originalului.', 'Тонкое чутье переводчика проявляется в небольших сдвигах, сохраняющих тон оригинала.', 'Ndjeshmëria e hollë e përkthyesit shfaqet në zhvendosje të vogla që ruajnë tonin e origjinalit.', 'Çevirmenin ince sezgisi, özgün metnin tonunu koruyan küçük kaymalarda görülür.'))
    ]
  },
  {
    word:'feinsinnig', language:'de', cefrLevel:level, partOfSpeech:'Adjective', article:null, plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'fein-sin-nig',
    topics:['culture-and-media','social-and-relationships','advanced-analysis'], usageLabels:['formal','written','advanced'], contextLabels:[],
    grammarNotes:['adjective; describes refined, subtle perception or expression'], collocations:[{text:'eine feinsinnige Beobachtung', meaning:'a subtle and perceptive observation'}],
    wordFamilies:[{lemma:'der Feinsinn', relationLabel:'noun', note:null}], relations:[],
    meanings:m('مرهف الحس؛ دقيق الملاحظة','هەستیار و وردبین','sensitive; subtle; perceptive','ظریف‌نگر؛ حساس و دقیق','hestiyar; hûrbîn','subtelny; wrażliwy; przenikliwy','fin; subtil; perspicace','тонкий; проницательный; чувствительный','i hollë; i ndjeshëm; depërtues','ince düşünceli; sezgili; duyarlı'),
    examples:[
      ex('Ihre feinsinnige Analyse der Teamkultur machte sichtbar, warum formale Regeln allein nicht ausreichen.', m('أظهر تحليلها الدقيق لثقافة الفريق لماذا لا تكفي القواعد الرسمية وحدها.', 'شیکردنەوەی وردبینانەی کەلتووری تیمەکە نیشانی دا بۆچی یاسا فەرمییەکان بە تەنها بەس نین.', 'Her subtle analysis of the team culture made clear why formal rules alone are not enough.', 'تحلیل ظریف‌نگر او از فرهنگ تیم نشان داد چرا قواعد رسمی به‌تنهایی کافی نیستند.', 'Analîza wê ya hûrbîn a çanda tîmê diyar kir çima rêzikên fermî bi tenê bes nînin.', 'Jej subtelna analiza kultury zespołu pokazała, dlaczego same formalne zasady nie wystarczą.', 'Analiza ei subtilă a culturii echipei a arătat de ce regulile formale singure nu sunt suficiente.', 'Ее тонкий анализ культуры команды показал, почему одних формальных правил недостаточно.', 'Analiza e saj e hollë e kulturës së ekipit tregoi pse rregullat formale vetëm nuk mjaftojnë.', 'Ekip kültürüne dair ince analizi, resmi kuralların tek başına neden yeterli olmadığını gösterdi.')),
      ex('Der Film erzählt die Trennung feinsinnig, ohne die Figuren moralisch gegeneinander auszuspielen.', m('يروي الفيلم الانفصال بحساسية من دون أن يضع الشخصيات أخلاقياً ضد بعضها.', 'فیلمەکە جیابوونەوەکە بە وردبینی دەگێڕێتەوە، بێ ئەوەی کارەکتەرەکان لە ڕووی ئەخلاقییەوە دژی یەکتر بخات.', 'The film portrays the separation subtly, without morally playing the characters against each other.', 'فیلم جدایی را با ظرافت روایت می‌کند، بی‌آنکه شخصیت‌ها را از نظر اخلاقی مقابل هم قرار دهد.', 'Fîlm veqetînê bi hûrbînî vedibêje, bê ku kesayetiyan ji aliyê exlaqî ve li dijî hev bixe.', 'Film subtelnie opowiada o rozstaniu, nie przeciwstawiając postaci sobie moralnie.', 'Filmul povestește despărțirea cu finețe, fără a pune personajele moral unul împotriva altuia.', 'Фильм тонко рассказывает о расставании, не противопоставляя персонажей друг другу морально.', 'Filmi e rrëfen ndarjen me hollësi, pa i vënë personazhet moralisht kundër njëri-tjetrit.', 'Film ayrılığı karakterleri ahlaken birbirine karşı kullanmadan incelikle anlatıyor.'))
    ]
  }
];
for (const entry of entries) {
  for (const topic of entry.topics) if (!topicSet.has(topic)) throw new Error(`Unknown topic ${topic}`);
  for (const label of [...entry.usageLabels, ...entry.contextLabels]) if (!labelMap.has(label)) throw new Error(`Unknown label ${label}`);
  if (entry.meanings.length !== 10) throw new Error(`Meaning count failed for ${entry.word}`);
  if (entry.examples.length < 2) throw new Error(`Example count failed for ${entry.word}`);
  for (const e of entry.examples) if (e.translations.length !== 10) throw new Error(`Translation count failed for ${entry.word}`);
}
const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const labels = usedLabels.map(k => labelMap.get(k));
const pkg = { packageVersion:'1.0', packageId:`de-${levelLower}-generated-batch-${batch}`, packageName:`German ${level} Generated Batch ${batch}`, source:'Hybrid', defaultMeaningLanguages:langs, labels, entries, collections:[], scenarios:[], conversationStarterPacks:[], eventPreparationPacks:[] };
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');
const importCmd = `dotnet run --project "${path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj')}" -- --target shared --yes "${outPath}"`;
const result = cp.spawnSync(importCmd, { shell:true, encoding:'utf8', cwd:root });
const output = `${result.stdout || ''}\n${result.stderr || ''}`;
console.log(output.replace(/(ConnectionString|Password|Pwd|Secret|Key)=[^\s;]+/gi, '$1=***'));
if (result.status !== 0) throw new Error(`Import command failed with status ${result.status}`);
const ok = output.includes('Entries imported: 6') && output.includes('Entries invalid: 0') && output.includes('Warnings: 0');
if (!ok) throw new Error('Import did not meet strict success criteria; source not modified.');
const remaining = tokens.slice(expected.length);
fs.writeFileSync(sourcePath, remaining.join(', '), 'utf8');
console.log(`SOURCE_UPDATED: ${sourcePath}`);
console.log(`PROCESSED: ${expected.join(' | ')}`);
console.log(`JSON_PATH: ${outPath}`);
console.log(`REMAINING_COUNT: ${remaining.length}`);
console.log(`FIRST_10: ${remaining.slice(0, 10).join(' | ')}`);

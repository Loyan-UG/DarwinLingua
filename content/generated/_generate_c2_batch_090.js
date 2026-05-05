const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '090';
const srcPath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outDir = path.join(root, 'content', 'generated');
const outPath = path.join(outDir, `de-${levelLower}-generated-batch-${batch}.json`);
const expected = ['der Verweisungszusammenhang', 'verwinkelt', 'das Vexierbild', 'vieldeutig', 'die Vieldeutigkeit', 'vielschichtig'];
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];

function splitTokens(text) { return text.split(',').map(s => s.trim()).filter(Boolean); }
function meaning(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) { return [ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr].map((text, i) => ({ language: langs[i], text })); }
function translations(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) { return meaning(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr); }
function example(baseText, t) { return { baseText, translations: t }; }
function entry(e) {
  return {
    word: e.word,
    language: 'de',
    cefrLevel: level,
    partOfSpeech: e.partOfSpeech,
    article: e.article ?? null,
    plural: e.plural ?? null,
    infinitive: e.infinitive ?? null,
    pronunciationIpa: null,
    syllableBreak: e.syllableBreak,
    topics: e.topics,
    usageLabels: e.usageLabels,
    contextLabels: [],
    grammarNotes: e.grammarNotes ?? [],
    collocations: e.collocations ?? [],
    wordFamilies: e.wordFamilies ?? [],
    relations: [],
    meanings: e.meanings,
    examples: e.examples
  };
}

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const labelsByKey = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const topicKeys = new Set((taxonomy.topics || []).map(t => t.key));
const sourceText = fs.readFileSync(srcPath, 'utf8');
const tokens = splitTokens(sourceText);
const first = tokens.slice(0, expected.length);
if (JSON.stringify(first) !== JSON.stringify(expected)) throw new Error(`Source head mismatch. Expected ${JSON.stringify(expected)}, got ${JSON.stringify(first)}`);

const entries = [
  entry({
    word: 'der Verweisungszusammenhang', partOfSpeech: 'Noun', article: 'der', plural: 'Verweisungszusammenhänge', syllableBreak: 'Ver-wei-sungs-zu-sam-men-hang',
    topics: ['law-and-compliance','documents-and-administration','advanced-analysis'],
    usageLabels: ['formal','written','academic','analysis'],
    grammarNotes: ['masculine compound noun; mainly used in legal, academic, and text-analytical contexts'],
    collocations: [{ text: 'einen Verweisungszusammenhang herstellen', meaning: 'to establish a cross-reference context' }],
    wordFamilies: [{ lemma: 'verweisen', relationLabel: 'verb', note: null }, { lemma: 'der Zusammenhang', relationLabel: 'compound base', note: null }],
    meanings: meaning('سياق الإحالة أو الترابط المرجعي','پەیوەندیی ئاماژەدان یان چوارچێوەی پەیوەندیدار','cross-reference context; referential connection','زمینه ارجاعی؛ پیوند ارجاعی میان بخش‌ها','têkiliya referansê; çarçoveya vegerandinê','kontekst odsyłający; związek referencyjny','context de trimitere; legătură referențială','система отсылок; референциальная связь','kontekst referimi; lidhje referenciale','atıf bağlamı; göndergesel bağlantı'),
    examples: [
      example('Der Verweisungszusammenhang zwischen Rahmenvertrag, Anlage und Datenschutzvereinbarung muss im Gutachten ausdrücklich erläutert werden.', translations('يجب في التقرير القانوني توضيح سياق الإحالة بين الاتفاقية الإطارية والملحق واتفاقية حماية البيانات بشكل صريح.','لە ڕاپۆرتی یاساییدا دەبێت پەیوەندیی ئاماژەدان لە نێوان گرێبەستی چوارچێوە، پاشکۆ و ڕێککەوتنی پاراستنی داتادا بە ڕوونی ڕوون بکرێتەوە.','The cross-reference context between the framework agreement, annex, and data protection agreement must be explained explicitly in the legal opinion.','زمینه ارجاعی میان قرارداد چارچوب، پیوست و توافق‌نامه حفاظت از داده‌ها باید در نظریه حقوقی صریحاً توضیح داده شود.','Di nirxandina hiqûqî de divê têkiliya referansê di navbera peymana çarçoveyê, pêvekê û peymana parastina daneyan de bi eşkere were rave kirin.','W opinii prawnej trzeba wyraźnie objaśnić kontekst odsyłający między umową ramową, załącznikiem i umową o ochronie danych.','Contextul de trimitere dintre contractul-cadru, anexă și acordul privind protecția datelor trebuie explicat explicit în opinia juridică.','В юридическом заключении необходимо прямо пояснить систему отсылок между рамочным договором, приложением и соглашением о защите данных.','Konteksti i referimit midis kontratës kornizë, aneksit dhe marrëveshjes për mbrojtjen e të dhënave duhet shpjeguar qartë në opinionin ligjor.','Çerçeve sözleşme, ek ve veri koruma anlaşması arasındaki atıf bağlamı hukuki görüşte açıkça açıklanmalıdır.')),
      example('Ohne diesen Verweisungszusammenhang wirkt die Fußnote wie ein isolierter Kommentar, nicht wie ein Teil der Argumentation.', translations('من دون هذا السياق المرجعي تبدو الحاشية تعليقًا معزولًا لا جزءًا من الحجة.','بێ ئەم چوارچێوەی ئاماژەدانە، پەراوێزەکە وەک سەرنجێکی جیاواز دەردەکەوێت نەک وەک بەشێک لە ئارگومێنتەکە.','Without this referential context, the footnote appears like an isolated comment rather than part of the argument.','بدون این زمینه ارجاعی، پاورقی مانند نظری جداافتاده به نظر می‌رسد، نه بخشی از استدلال.','Bê vê çarçoveya referansê, jêrenivîs wek şîroveyeke tenê xuya dike, ne wek beşek ji argûmanê.','Bez tego kontekstu odsyłającego przypis wygląda jak odizolowany komentarz, a nie część argumentacji.','Fără acest context referențial, nota de subsol pare un comentariu izolat, nu o parte a argumentației.','Без этого референциального контекста сноска выглядит как изолированный комментарий, а не как часть аргументации.','Pa këtë kontekst referencial, fusnota duket si koment i izoluar, jo si pjesë e argumentimit.','Bu göndergesel bağlam olmadan dipnot, argümanın parçası değil, yalıtılmış bir yorum gibi görünür.'))
    ]
  }),
  entry({
    word: 'verwinkelt', partOfSpeech: 'Adjective', syllableBreak: 'ver-win-kelt',
    topics: ['housing-and-real-estate','technology-and-it','advanced-analysis'],
    usageLabels: ['formal','written','advanced','analysis'],
    grammarNotes: ['adjective; describes physical layouts or abstract structures with many angles, turns, or complications'],
    collocations: [{ text: 'verwinkelte Strukturen', meaning: 'intricate or convoluted structures' }],
    wordFamilies: [{ lemma: 'der Winkel', relationLabel: 'noun', note: null }],
    meanings: meaning('متعرج ومعقد؛ كثير الزوايا','ئاڵۆز و پڕ لە گۆشە و لادان','intricate; winding; convoluted','پیچ‌درپیچ؛ پرزاویه؛ پیچیده','tevlîhev; tijî goşe û zivirîn','zawiły; kręty; skomplikowany','întortocheat; sinuos; complicat','извилистый; запутанный; сложный','i ndërlikuar; me shumë kthesa','dolambaçlı; karmaşık'),
    examples: [
      example('Die historisch gewachsene Rechteverwaltung war so verwinkelt, dass selbst erfahrene Administratoren kaum noch nachvollziehen konnten, wer worauf Zugriff hatte.', translations('كانت إدارة الصلاحيات التي نشأت تاريخيًا معقدة جدًا لدرجة أن حتى المسؤولين ذوي الخبرة بالكاد استطاعوا فهم من يملك الوصول إلى ماذا.','بەڕێوەبردنی مافەکان کە بە مێژوویی گەشەی کردبوو ئەوەندە ئاڵۆز بوو کە تەنانەت ئەدمینە شارەزاکانیش بە زەحمەت دەیانزانی کێ دەستی بە چی دەگات.','The historically grown permission management was so convoluted that even experienced administrators could barely tell who had access to what.','مدیریت دسترسی‌ها که در طول زمان شکل گرفته بود چنان پیچیده بود که حتی مدیران باتجربه هم به‌سختی می‌فهمیدند چه کسی به چه چیزی دسترسی دارد.','Rêveberiya mafan a bi demê mezin bûyî ewqas tevlîhev bû ku hetta rêveberên bi ezmûn jî bi zehmet dizanîn kî gihîştina çi heye.','Historycznie narosłe zarządzanie uprawnieniami było tak zawiłe, że nawet doświadczeni administratorzy ledwo mogli ustalić, kto ma do czego dostęp.','Administrarea drepturilor, crescută istoric, era atât de complicată încât chiar și administratorii experimentați abia mai puteau urmări cine avea acces la ce.','Исторически сложившееся управление правами было настолько запутанным, что даже опытные администраторы с трудом понимали, у кого к чему есть доступ.','Menaxhimi i të drejtave, i krijuar me kalimin e kohës, ishte aq i ndërlikuar sa edhe administratorët me përvojë mezi kuptonin kush kishte qasje në çfarë.','Zamanla oluşmuş yetki yönetimi o kadar karmaşıktı ki deneyimli yöneticiler bile kimin neye erişimi olduğunu zor takip ediyordu.')),
      example('Das Gästehaus liegt in einer verwinkelten Altstadtgasse, in der Lieferwagen nur morgens durchkommen.', translations('يقع بيت الضيافة في زقاق قديم متعرج لا تمر فيه شاحنات التوصيل إلا صباحًا.','خانەی میوانەکە لە کۆڵانێکی کۆنی پڕ لە لاداندا هەیە کە ئۆتۆمبێلی گەیاندن تەنها بەیانی تێیدا تێدەپەڕێت.','The guesthouse is located in a winding old-town alley where delivery vans can pass only in the morning.','مهمان‌خانه در کوچه‌ای پیچ‌درپیچ در بافت قدیمی شهر قرار دارد که ون‌های تحویل فقط صبح‌ها می‌توانند از آن عبور کنند.','Mala mêvanan li kolaneke kevn a tevlîhev e ku erebeyên radestkirinê tenê sibehan dikarin tê re derbas bibin.','Pensjonat leży w krętej uliczce starego miasta, przez którą samochody dostawcze przejeżdżają tylko rano.','Casa de oaspeți se află pe o străduță întortocheată din centrul vechi, pe unde dubele de livrare trec doar dimineața.','Гостевой дом находится в извилистом переулке старого города, куда фургоны доставки могут проехать только утром.','Bujtina ndodhet në një rrugicë të vjetër me shumë kthesa, ku furgonët e furnizimit kalojnë vetëm në mëngjes.','Konukevi, teslimat araçlarının yalnızca sabah geçebildiği dolambaçlı bir eski şehir sokağında yer alıyor.'))
    ]
  }),
  entry({
    word: 'das Vexierbild', partOfSpeech: 'Noun', article: 'das', plural: 'Vexierbilder', syllableBreak: 'Ve-xier-bild',
    topics: ['culture-and-media','education-and-training','advanced-analysis'],
    usageLabels: ['formal','written','academic','analysis'],
    grammarNotes: ['neuter noun; denotes an ambiguous or hidden-picture image that changes with perspective'],
    collocations: [{ text: 'wie ein Vexierbild wirken', meaning: 'to appear like an ambiguous image that can be read in different ways' }],
    wordFamilies: [{ lemma: 'vexieren', relationLabel: 'verb', note: 'rare; to puzzle or tease' }],
    meanings: meaning('صورة خادعة أو متعددة الإدراك','وێنەی فریودەر یان چەندمانا','ambiguous image; hidden-picture puzzle','تصویر دوپهلو؛ تصویر معمایی','wêneyê dudilî; wêneyê veşartî','obraz dwuznaczny; układanka optyczna','imagine ambiguă; imagine-enigmă','двусмысленное изображение; картинка-загадка','imazh i dykuptimtë; figurë enigmë','çok anlamlı/gizli resim bulmacası'),
    examples: [
      example('Die Grafik im Bericht wirkt wie ein Vexierbild: Je nach Skalierung erzählt sie entweder von Stabilität oder von schleichendem Kontrollverlust.', translations('يبدو الرسم في التقرير كصورة خادعة: فبحسب طريقة القياس يروي إما عن الاستقرار أو عن فقدان تدريجي للسيطرة.','گرافیکەکە لە ڕاپۆرتەکەدا وەک وێنەیەکی فریودەر دەردەکەوێت: بە پێی پێوانەکردن یان لە جێگیری دەدوێت یان لە لەدەستدانی هێواشی کۆنترۆڵ.','The chart in the report looks like an ambiguous image: depending on the scaling, it tells either of stability or of a gradual loss of control.','نمودار گزارش مثل یک تصویر دوپهلو به نظر می‌رسد: بسته به مقیاس، یا از ثبات حرف می‌زند یا از از دست رفتن تدریجی کنترل.','Grafîka di raporê de wek wêneyeke dudilî xuya dike: Li gor pîvanê yan ji aramiyê dibêje yan ji windakirina hêdî ya kontrolê.','Wykres w raporcie działa jak obraz dwuznaczny: w zależności od skali opowiada albo o stabilności, albo o stopniowej utracie kontroli.','Graficul din raport pare o imagine ambiguă: în funcție de scalare, vorbește fie despre stabilitate, fie despre o pierdere treptată a controlului.','График в отчете выглядит как двусмысленное изображение: в зависимости от масштаба он говорит либо о стабильности, либо о постепенной утрате контроля.','Grafiku në raport duket si imazh i dykuptimtë: në varësi të shkallëzimit, flet ose për stabilitet, ose për humbje graduale kontrolli.','Rapordaki grafik bir optik bilmece gibi görünüyor: Ölçeğe göre ya istikrarı ya da kontrolün yavaş yavaş kaybını anlatıyor.')),
      example('Für die Studierenden wurde das Gemälde zum Vexierbild, sobald sie die kolonialen Anspielungen im Hintergrund erkannten.', translations('تحولت اللوحة بالنسبة إلى الطلاب إلى صورة متعددة الإدراك عندما لاحظوا الإشارات الاستعمارية في الخلفية.','بۆ خوێندکاران تابلوکە بووە وێنەیەکی چەندمانا کاتێک ئاماژە کۆلۆنیالییەکانی پاشبنەماکەیان ناساند.','For the students, the painting became an ambiguous image once they recognized the colonial allusions in the background.','برای دانشجویان، نقاشی وقتی به تصویری دوپهلو تبدیل شد که اشاره‌های استعماری در پس‌زمینه را تشخیص دادند.','Ji bo xwendekaran tablo bû wêneyeke dudilî dema ku wan îşaretên kolonîal ên li paşxaneyê nas kirin.','Dla studentów obraz stał się wieloznaczną zagadką, gdy rozpoznali kolonialne aluzje w tle.','Pentru studenți, tabloul a devenit o imagine ambiguă imediat ce au recunoscut aluziile coloniale din fundal.','Для студентов картина стала двусмысленным изображением, как только они распознали колониальные намеки на заднем плане.','Për studentët, piktura u bë imazh i dykuptimtë sapo dalluan aludimet koloniale në sfond.','Öğrenciler arka plandaki sömürgeci göndermeleri fark edince tablo onlar için çok anlamlı bir görsele dönüştü.'))
    ]
  }),
  entry({
    word: 'vieldeutig', partOfSpeech: 'Adjective', syllableBreak: 'viel-deu-tig',
    topics: ['business-communication','culture-and-media','advanced-analysis'],
    usageLabels: ['formal','written','analysis','advanced'],
    grammarNotes: ['adjective; describes statements, signs, or situations that allow several interpretations'],
    collocations: [{ text: 'eine vieldeutige Formulierung', meaning: 'an ambiguous or multi-layered wording' }],
    wordFamilies: [{ lemma: 'die Vieldeutigkeit', relationLabel: 'noun', note: null }],
    meanings: meaning('متعدد المعاني؛ ملتبس','چەندمانا؛ ناڕوون','ambiguous; having multiple meanings','چندمعنا؛ مبهم','pirwate; nezelal','wieloznaczny; dwuznaczny','ambiguu; cu mai multe sensuri','многозначный; неоднозначный','i shumëkuptimtë; i paqartë','çok anlamlı; muğlak'),
    examples: [
      example('Die Formulierung im Vorstandsprotokoll ist vieldeutig genug, um sowohl als Zustimmung als auch als strategisches Offenhalten gelesen zu werden.', translations('صياغة محضر مجلس الإدارة ملتبسة بما يكفي لتُقرأ إما كموافقة أو كإبقاء استراتيجي للخيارات مفتوحة.','داڕشتنەکە لە تۆماری دەستەی بەڕێوەبردندا بە قەدەرێک چەندمانایە کە دەتوانرێت وەک ڕەزامەندی یان وەک هێشتنەوەی ستراتیژیی بژاردەکان بخوێنرێتەوە.','The wording in the board minutes is ambiguous enough to be read both as approval and as strategic keeping of options open.','عبارت در صورت‌جلسه هیئت‌مدیره آن‌قدر چندمعناست که هم می‌توان آن را موافقت دانست و هم باز نگه داشتن راهبردی گزینه‌ها.','Gotina di protokola rêveberiyê de ewqas pirwate ye ku hem wek erêkirin hem jî wek vekirîhiştina stratejîk a vebijarkan dikare were xwendin.','Sformułowanie w protokole zarządu jest na tyle wieloznaczne, że można je odczytać zarówno jako zgodę, jak i strategiczne pozostawienie opcji otwartych.','Formularea din procesul-verbal al consiliului este suficient de ambiguă pentru a fi citită atât ca aprobare, cât și ca menținere strategică a opțiunilor deschise.','Формулировка в протоколе правления достаточно неоднозначна, чтобы ее можно было прочитать и как согласие, и как стратегическое сохранение вариантов открытыми.','Formulimi në procesverbalin e bordit është mjaft i shumëkuptimtë për t’u lexuar si miratim, por edhe si mbajtje strategjike e opsioneve të hapura.','Yönetim kurulu tutanağındaki ifade, hem onay hem de seçenekleri stratejik olarak açık tutma şeklinde okunabilecek kadar muğlak.')),
      example('Der Schluss des Films bleibt vieldeutig, weil weder Schuld noch Vergebung eindeutig verteilt werden.', translations('تبقى نهاية الفيلم متعددة المعاني لأن الذنب والمغفرة لا يُوزعان بوضوح.','کۆتایی فیلمەکە چەندمانا دەمێنێتەوە چونکە نە تاوان و نە لێخۆشبوون بە ڕوونی دابەش ناکرێن.','The ending of the film remains ambiguous because neither guilt nor forgiveness is assigned clearly.','پایان فیلم چندمعنا باقی می‌ماند، زیرا نه گناه و نه بخشش به‌روشنی مشخص نمی‌شود.','Dawiya fîlmê pirwate dimîne, ji ber ku ne sûc û ne lêborîn bi zelalî nayên dabeşkirin.','Zakończenie filmu pozostaje wieloznaczne, ponieważ ani wina, ani przebaczenie nie zostają jednoznacznie rozdzielone.','Finalul filmului rămâne ambiguu, deoarece nici vina, nici iertarea nu sunt atribuite clar.','Финал фильма остается неоднозначным, потому что ни вина, ни прощение не распределены однозначно.','Fundi i filmit mbetet i shumëkuptimtë, sepse as faji, as falja nuk ndahen qartë.','Filmin sonu çok anlamlı kalır, çünkü ne suç ne de bağışlama açıkça paylaştırılır.'))
    ]
  }),
  entry({
    word: 'die Vieldeutigkeit', partOfSpeech: 'Noun', article: 'die', plural: 'Vieldeutigkeiten', syllableBreak: 'Viel-deu-tig-keit',
    topics: ['advanced-analysis','business-communication','culture-and-media'],
    usageLabels: ['formal','written','academic','analysis'],
    grammarNotes: ['feminine abstract noun; plural is possible when referring to individual ambiguities'],
    collocations: [{ text: 'Vieldeutigkeit bewusst einsetzen', meaning: 'to use ambiguity deliberately' }],
    wordFamilies: [{ lemma: 'vieldeutig', relationLabel: 'adjective', note: null }],
    meanings: meaning('تعدد المعاني؛ الالتباس','چەندمانایی؛ ناڕوونی','ambiguity; multiplicity of meanings','چندمعنایی؛ ابهام','pirwatebûn; nezelalî','wieloznaczność; niejednoznaczność','ambiguitate; multiplicitate de sensuri','многозначность; неоднозначность','shumëkuptimësi; paqartësi','çok anlamlılık; belirsizlik'),
    examples: [
      example('Die Vieldeutigkeit der Klausel mag verhandlungstaktisch nützlich sein, erhöht aber das Risiko späterer Streitigkeiten.', translations('قد تكون تعددية معنى البند مفيدة تكتيكيًا في التفاوض، لكنها تزيد خطر النزاعات اللاحقة.','چەندمانایی ماددەکە لە ڕووی تاکتیکی وتووێژەوە سوودمەندە، بەڵام مەترسیی ناکۆکیی دواتر زیاد دەکات.','The ambiguity of the clause may be useful as a negotiation tactic, but it increases the risk of later disputes.','چندمعنایی بند شاید از نظر تاکتیک مذاکره مفید باشد، اما خطر اختلاف‌های بعدی را افزایش می‌دهد.','Pirwatebûna bendê dikare ji aliyê taktîka danûstandinê ve bikêr be, lê metirsiya nakokiyên paşerojê zêde dike.','Wieloznaczność klauzuli może być użyteczna taktycznie w negocjacjach, ale zwiększa ryzyko późniejszych sporów.','Ambiguitatea clauzei poate fi utilă tactic în negociere, dar crește riscul unor litigii ulterioare.','Неоднозначность положения может быть тактически полезна на переговорах, но повышает риск последующих споров.','Shumëkuptimësia e klauzolës mund të jetë e dobishme taktikisht në negociata, por rrit rrezikun e mosmarrëveshjeve të mëvonshme.','Maddenin çok anlamlılığı müzakere taktiği açısından yararlı olabilir, ancak sonraki uyuşmazlık riskini artırır.')),
      example('In der Lyrik wird Vieldeutigkeit nicht als Mangel, sondern als produktive Spannung verstanden.', translations('في الشعر لا تُفهم تعددية المعنى كعيب، بل كتوتر منتج.','لە شیعردا چەندمانایی وەک کەموکوڕی تێناگەندرێت، بەڵکو وەک گرژییەکی بەرهەمهێنەر دەبینرێت.','In poetry, ambiguity is understood not as a flaw but as a productive tension.','در شعر، چندمعنایی نه به‌عنوان نقص، بلکه به‌عنوان تنشی زایا فهمیده می‌شود.','Di helbestê de pirwatebûn ne wek kêmasî, lê wek tewangeke berhemdar tê fêmkirin.','W poezji wieloznaczność rozumie się nie jako brak, lecz jako produktywne napięcie.','În poezie, ambiguitatea nu este înțeleasă ca un defect, ci ca o tensiune productivă.','В поэзии многозначность понимается не как недостаток, а как продуктивное напряжение.','Në poezi, shumëkuptimësia kuptohet jo si mangësi, por si tension produktiv.','Şiirde çok anlamlılık bir eksiklik değil, üretken bir gerilim olarak anlaşılır.'))
    ]
  }),
  entry({
    word: 'vielschichtig', partOfSpeech: 'Adjective', syllableBreak: 'viel-schich-tig',
    topics: ['advanced-analysis','management-and-leadership','culture-and-media'],
    usageLabels: ['formal','written','analysis','advanced'],
    grammarNotes: ['adjective; describes matters, characters, or systems with many layers or dimensions'],
    collocations: [{ text: 'ein vielschichtiges Problem', meaning: 'a multi-layered problem' }],
    wordFamilies: [{ lemma: 'die Vielschichtigkeit', relationLabel: 'noun', note: null }, { lemma: 'die Schicht', relationLabel: 'noun', note: null }],
    meanings: meaning('متعدد الطبقات؛ معقد الجوانب','چەندچین و فرەڕەهەند','multi-layered; complex; nuanced','چندلایه؛ پیچیده و چندوجهی','pir-qat; tevlihev û xwedî gelek aliyên cuda','wielowarstwowy; złożony','multistratificat; complex; nuanțat','многослойный; сложный; многогранный','shumështresor; kompleks','çok katmanlı; karmaşık; nüanslı'),
    examples: [
      example('Die Ursachen der hohen Fluktuation sind vielschichtig und lassen sich nicht allein mit dem Gehaltsniveau erklären.', translations('أسباب ارتفاع معدل دوران الموظفين متعددة الجوانب ولا يمكن تفسيرها بمستوى الرواتب وحده.','هۆکارەکانی گۆڕانی زۆری کارمەندان چەندڕەهەندن و تەنها بە ئاستی مووچە ڕوون ناکرێنەوە.','The causes of the high turnover are multi-layered and cannot be explained by salary levels alone.','دلایل جابه‌جایی بالای کارکنان چندلایه است و فقط با سطح حقوق توضیح داده نمی‌شود.','Sedemên guherîna bilind a karmendan pir-qat in û tenê bi asta mûçeyê nayên ravekirin.','Przyczyny wysokiej rotacji są złożone i nie dają się wyjaśnić wyłącznie poziomem wynagrodzeń.','Cauzele fluctuației ridicate sunt complexe și nu pot fi explicate doar prin nivelul salariilor.','Причины высокой текучести многослойны и не объясняются одним лишь уровнем зарплат.','Shkaqet e qarkullimit të lartë të stafit janë komplekse dhe nuk shpjegohen vetëm me nivelin e pagave.','Yüksek personel devrinin nedenleri çok katmanlıdır ve yalnızca maaş düzeyiyle açıklanamaz.')),
      example('Der Roman zeichnet eine vielschichtige Hauptfigur, deren politische Überzeugungen nie vollständig von privaten Kränkungen zu trennen sind.', translations('ترسم الرواية شخصية رئيسية متعددة الطبقات لا يمكن فصل قناعاتها السياسية تمامًا عن جراحها الخاصة.','ڕۆمانەکە کارەکتەرێکی سەرەکی چەندچین وێنا دەکات کە باوەڕە سیاسییەکانی هەرگیز بە تەواوی لە ئازارە تایبەتییەکانی جیا ناکرێنەوە.','The novel portrays a multi-layered protagonist whose political convictions can never be fully separated from private wounds.','رمان شخصیت اصلی چندلایه‌ای ترسیم می‌کند که باورهای سیاسی او هرگز کاملاً از رنجش‌های شخصی‌اش جدا نمی‌شود.','Roman kesayetiya sereke ya pir-qat nîşan dide ku baweriyên wê yên siyasî carî bi temamî ji birînên taybet nayên veqetandin.','Powieść kreśli wielowarstwową postać główną, której przekonań politycznych nigdy nie da się całkowicie oddzielić od prywatnych uraz.','Romanul conturează un personaj principal complex, ale cărui convingeri politice nu pot fi separate complet de rănile personale.','Роман создает многослойного главного героя, чьи политические убеждения невозможно полностью отделить от личных обид.','Romani portretizon një personazh kryesor shumështresor, bindjet politike të të cilit nuk ndahen kurrë plotësisht nga plagët private.','Roman, siyasi inançları kişisel kırgınlıklarından hiçbir zaman tamamen ayrılamayan çok katmanlı bir başkarakter çizer.'))
    ]
  })
];

for (const e of entries) {
  for (const t of e.topics) if (!topicKeys.has(t)) throw new Error(`Unknown topic ${t}`);
  for (const l of [...e.usageLabels, ...e.contextLabels]) if (!labelsByKey.has(l)) throw new Error(`Unknown label ${l}`);
}
const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId: `de-${levelLower}-generated-batch-${batch}`, packageName: `German ${level} Generated Batch ${batch}`, source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
fs.mkdirSync(outDir, { recursive: true });
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');
const project = path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj');
const res = cp.spawnSync('dotnet', ['run', '--project', project, '--', '--target', 'shared', '--yes', outPath], { encoding: 'utf8', cwd: root });
const output = `${res.stdout || ''}${res.stderr || ''}`;
process.stdout.write(output);
const ok = res.status === 0 && output.includes('Entries imported: 6') && output.includes('Entries invalid: 0') && output.includes('Warnings: 0');
if (!ok) {
  const failedPath = path.join(outDir, `${levelLower}-failed-words.txt`);
  fs.appendFileSync(failedPath, `${new Date().toISOString()}\tbatch-${batch}\t${expected.join(', ')}\n`, 'utf8');
  throw new Error(`Import did not meet strict success criteria. Source not modified. Failed words logged to ${failedPath}`);
}
const remaining = tokens.slice(expected.length);
fs.writeFileSync(srcPath, remaining.join(', '), 'utf8');
console.log(`\nJSON_PATH=${outPath}`);
console.log(`SOURCE_PATH=${srcPath}`);
console.log(`PROCESSED=${expected.join(' | ')}`);
console.log(`SOURCE_UPDATED=yes`);
console.log(`REMAINING_COUNT=${remaining.length}`);
console.log(`FIRST_10=${remaining.slice(0, 10).join(' | ')}`);

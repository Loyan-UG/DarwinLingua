const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '073';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['sinnfällig','der Sinnhorizont','sinnieren','die Sinnschicht','die Sinnstiftung','skizzieren'];

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
    word: 'sinnfällig', partOfSpeech: 'Adjective', syllableBreak: 'sinn-fäl-lig',
    topics: ['advanced-analysis','business-communication','culture-and-media'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'ein sinnfälliges Beispiel', meaning: 'a vivid and telling example' }],
    meanings: meaning('واضح دال؛ ملموس المعنى','ڕوون و مانادار','vivid; telling; clearly meaningful','گویا؛ روشن و معنادار','zelal û watedar','wymowny; obrazowy','grăitor; sugestiv','наглядный; выразительный','domethënës; i qartë','anlamlı ve açık; çarpıcı'),
    examples: [
      ex('Der Ausfall des Dashboards war ein sinnfälliges Beispiel dafür, wie abhängig das Team von scheinbar nebensächlichen Tools geworden war.', meaning('كان تعطل لوحة المعلومات مثالاً واضحاً على مدى اعتماد الفريق على أدوات بدت هامشية.','وەستانی داشبۆردەکە نموونەیەکی ڕوون بوو بۆ ئەوەی تیمەکە چەندە پشت بە ئامرازە بە ڕواڵەت لاوەکییەکان دەبەستێت.','The dashboard outage was a telling example of how dependent the team had become on seemingly minor tools.','از کار افتادن داشبورد نمونه‌ای گویا بود از اینکه تیم چقدر به ابزارهای ظاهراً فرعی وابسته شده بود.','Qutbûna dashboardê mînakeke watedar bû ku tîm çiqas bûye girêdayî amûrên xuya kêlekî.','Awaria dashboardu była wymownym przykładem tego, jak bardzo zespół uzależnił się od pozornie drugorzędnych narzędzi.','Căderea dashboardului a fost un exemplu grăitor al dependenței echipei de instrumente aparent secundare.','Сбой панели мониторинга стал наглядным примером того, насколько команда стала зависима от, казалось бы, второстепенных инструментов.','Rënia e panelit ishte një shembull domethënës se sa i varur ishte bërë ekipi nga mjetet në dukje dytësore.','Panonun çökmesi, ekibin görünüşte ikincil araçlara ne kadar bağımlı hale geldiğinin çarpıcı bir örneğiydi.')),
      ex('Die letzte Szene macht sinnfällig, dass Versöhnung im Roman nicht als Lösung, sondern als fragile Möglichkeit erscheint.', meaning('يجعل المشهد الأخير واضحاً أن المصالحة في الرواية لا تظهر كحل، بل كاحتمال هش.','دیمەنی کۆتایی بە ڕوونی دەردەخات کە ئاشتبوونەوە لە ڕۆمانەکەدا وەک چارەسەر نا، بەڵکو وەک ئەگەرێکی ناسک دەردەکەوێت.','The final scene makes it vividly clear that reconciliation in the novel appears not as a solution, but as a fragile possibility.','صحنه آخر به‌خوبی نشان می‌دهد که آشتی در رمان نه به‌عنوان راه‌حل، بلکه به‌عنوان امکانی شکننده ظاهر می‌شود.','Dîmena dawî bi zelalî nîşan dide ku lihevhatin di romanê de ne wek çareserî, lê wek derfeteke nazik xuya dike.','Ostatnia scena wymownie pokazuje, że pojednanie w powieści nie jawi się jako rozwiązanie, lecz jako krucha możliwość.','Ultima scenă arată sugestiv că împăcarea în roman nu apare ca soluție, ci ca posibilitate fragilă.','Последняя сцена наглядно показывает, что примирение в романе предстает не как решение, а как хрупкая возможность.','Skena e fundit e bën të qartë se pajtimi në roman nuk shfaqet si zgjidhje, por si mundësi e brishtë.','Son sahne, romanda uzlaşmanın bir çözüm değil, kırılgan bir olasılık olarak belirdiğini çarpıcı biçimde gösterir.'))
    ]
  }),
  entry({
    word: 'der Sinnhorizont', partOfSpeech: 'Noun', article: 'der', plural: 'Sinnhorizonte', syllableBreak: 'Sinn-ho-ri-zont',
    topics: ['advanced-analysis','culture-and-media','education-and-training'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'den Sinnhorizont erweitern', meaning: 'to broaden the horizon of meaning' }],
    meanings: meaning('أفق المعنى؛ إطار الفهم','ئاسۆی مانا؛ چوارچێوەی تێگەیشتن','horizon of meaning; interpretive frame','افق معنا؛ چارچوب فهم','asoya wateyê; çarçoveya têgihiştinê','horyzont sensu','orizont de sens','горизонт смысла','horizont kuptimi','anlam ufku'),
    examples: [
      ex('Die neue Datenstrategie verändert den Sinnhorizont der Kennzahlen: Sie dienen nicht mehr nur der Kontrolle, sondern auch dem Lernen.', meaning('تغيّر استراتيجية البيانات الجديدة أفق معنى المؤشرات: فهي لم تعد تخدم الرقابة فقط، بل التعلم أيضاً.','ستراتیژیای نوێی داتا ئاسۆی مانای پێوەرەکان دەگۆڕێت: ئیتر تەنها بۆ کۆنتڕۆڵ نین، بەڵکو بۆ فێربوونیش.','The new data strategy changes the horizon of meaning of the metrics: they no longer serve only control, but also learning.','راهبرد جدید داده افق معنای شاخص‌ها را تغییر می‌دهد: آن‌ها دیگر فقط برای کنترل نیستند، بلکه برای یادگیری هم هستند.','Stratejiya nû ya daneyan asoya wateya nîşaneyan diguherîne: ew êdî ne tenê ji bo kontrolê ne, lê jî ji bo fêrbûnê ne.','Nowa strategia danych zmienia horyzont sensu wskaźników: służą już nie tylko kontroli, lecz także uczeniu się.','Noua strategie de date schimbă orizontul de sens al indicatorilor: ei nu mai servesc doar controlului, ci și învățării.','Новая стратегия данных меняет горизонт смысла показателей: они служат уже не только контролю, но и обучению.','Strategjia e re e të dhënave ndryshon horizontin e kuptimit të treguesve: ata nuk shërbejnë më vetëm për kontroll, por edhe për të mësuar.','Yeni veri stratejisi göstergelerin anlam ufkunu değiştiriyor: artık yalnızca kontrole değil, öğrenmeye de hizmet ediyorlar.')),
      ex('Der Sinnhorizont des Gedichts öffnet sich erst, wenn man die religiösen Anspielungen mitliest.', meaning('لا ينفتح أفق معنى القصيدة إلا عندما تُقرأ الإشارات الدينية معها.','ئاسۆی مانای شیعرەکە تەنها کاتێک دەکرێتەوە کە ئاماژە ئاینییەکانیش پێکەوە بخوێندرێن.','The poem’s horizon of meaning opens only when the religious allusions are read along with it.','افق معنای شعر فقط وقتی گشوده می‌شود که اشاره‌های دینی نیز در خوانش لحاظ شوند.','Asoya wateya helbestê tenê wê demê vedibe ku amajeyên olî jî bi hev re bên xwendin.','Horyzont sensu wiersza otwiera się dopiero wtedy, gdy uwzględni się aluzje religijne.','Orizontul de sens al poemului se deschide abia când sunt citite și aluziile religioase.','Горизонт смысла стихотворения открывается лишь тогда, когда учитываются религиозные аллюзии.','Horizonti i kuptimit të poezisë hapet vetëm kur lexohen edhe aludimet fetare.','Şiirin anlam ufku ancak dinsel göndermeler de birlikte okunduğunda açılır.'))
    ]
  }),
  entry({
    word: 'sinnieren', partOfSpeech: 'Verb', infinitive: 'sinnieren', syllableBreak: 'sin-nie-ren',
    topics: ['culture-and-media','advanced-analysis','everyday-life'], usageLabels: ['formal','written','advanced','analysis'],
    collocations: [{ text: 'über eine Frage sinnieren', meaning: 'to ponder a question' }],
    meanings: meaning('يتأمل؛ يفكر بعمق','بیرکردنەوەی قووڵ؛ وردبوونەوە','to ponder; to reflect deeply','اندیشیدن؛ درنگ‌آمیز فکر کردن','kûr fikirîn; raman kirin','rozmyślać; dumać','a medita; a reflecta','размышлять; задумываться','meditoj; mendohem thellë','derin derin düşünmek'),
    examples: [
      ex('Nach dem gescheiterten Release sinnierte das Team nicht über Schuld, sondern über die blinden Flecken seines Prozesses.', meaning('بعد فشل الإصدار لم يتأمل الفريق في الذنب، بل في النقاط العمياء في عمليته.','دوای بڵاوکردنەوەی شکستەوە، تیمەکە نە دەربارەی تاوان، بەڵکو دەربارەی خاڵە نادیارەکانی پڕۆسەکەی بیری کردەوە.','After the failed release, the team did not ponder blame, but the blind spots in its process.','پس از انتشار ناموفق، تیم نه درباره مقصر، بلکه درباره نقاط کور فرایند خود اندیشید.','Piştî release a têkçûyî, tîm ne li ser sûc, lê li ser xalên kor ên pêvajoya xwe fikirî.','Po nieudanym wydaniu zespół nie rozmyślał o winie, lecz o ślepych punktach swojego procesu.','După release-ul eșuat, echipa nu a meditat la vină, ci la punctele oarbe ale procesului său.','После неудачного релиза команда размышляла не о вине, а о слепых зонах своего процесса.','Pas release-it të dështuar, ekipi nuk meditoi për fajin, por për pikat e verbra të procesit të vet.','Başarısız sürümden sonra ekip suç üzerine değil, sürecinin kör noktaları üzerine düşündü.')),
      ex('Am Fenster sinniert die Figur über die Möglichkeit, ein Leben zu verlassen, das längst nicht mehr zu ihr passt.', meaning('عند النافذة تتأمل الشخصية إمكانية ترك حياة لم تعد تناسبها منذ زمن.','لە پەنجەرەکەدا کارەکتەرەکە دەربارەی ئەگەری جێهێشتنی ژیانێک بیر دەکاتەوە کە ماوەیەکە گونجاوی نییە.','At the window, the character ponders the possibility of leaving a life that has long no longer suited her.','کنار پنجره، شخصیت به امکان ترک زندگی‌ای می‌اندیشد که مدت‌هاست دیگر با او جور نیست.','Li ber paceyê kesayet li ser derfeta hiştina jiyanekê difikire ku demek dirêj e êdî bi wê re nagunce.','Przy oknie postać rozmyśla o możliwości opuszczenia życia, które od dawna już do niej nie pasuje.','La fereastră, personajul meditează la posibilitatea de a părăsi o viață care de mult nu i se mai potrivește.','У окна персонаж размышляет о возможности оставить жизнь, которая давно ей не подходит.','Te dritarja, personazhi mendon për mundësinë e largimit nga një jetë që prej kohësh nuk i përshtatet më.','Pencerenin yanında karakter, uzun zamandır kendisine uymayan bir hayatı terk etme olasılığı üzerine düşünür.'))
    ]
  }),
  entry({
    word: 'die Sinnschicht', partOfSpeech: 'Noun', article: 'die', plural: 'Sinnschichten', syllableBreak: 'Sinn-schicht',
    topics: ['culture-and-media','advanced-analysis','education-and-training'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'mehrere Sinnschichten freilegen', meaning: 'to uncover several layers of meaning' }],
    meanings: meaning('طبقة معنى؛ مستوى دلالي','چینی مانا؛ ئاستی واتایی','layer of meaning; semantic layer','لایه معنا؛ سطح دلالی','qatê wateyê; asta semantîk','warstwa sensu','strat de sens','смысловой слой','shtresë kuptimi','anlam katmanı'),
    examples: [
      ex('Die Nutzerkommentare legten eine Sinnschicht frei, die in den quantitativen Daten nicht sichtbar war.', meaning('كشفت تعليقات المستخدمين طبقة معنى لم تكن مرئية في البيانات الكمية.','کۆمێنتی بەکارهێنەران چینێکی مانای ئاشکرا کرد کە لە داتای چەندییەکاندا دیار نەبوو.','The user comments uncovered a layer of meaning that was not visible in the quantitative data.','نظرهای کاربران لایه‌ای از معنا را آشکار کرد که در داده‌های کمی دیده نمی‌شد.','Şîroveyên bikarhêneran qatek wateyê eşkere kir ku di daneyên hejmarî de xuya nebû.','Komentarze użytkowników odsłoniły warstwę sensu niewidoczną w danych ilościowych.','Comentariile utilizatorilor au scos la iveală un strat de sens care nu era vizibil în datele cantitative.','Комментарии пользователей выявили смысловой слой, не видимый в количественных данных.','Komentet e përdoruesve zbuluan një shtresë kuptimi që nuk ishte e dukshme në të dhënat sasiore.','Kullanıcı yorumları, nicel verilerde görünmeyen bir anlam katmanını ortaya çıkardı.')),
      ex('Jede Wiederholung im Gedicht fügt eine neue Sinnschicht hinzu, statt nur den vorherigen Vers zu verstärken.', meaning('كل تكرار في القصيدة يضيف طبقة معنى جديدة بدلاً من مجرد تقوية البيت السابق.','هەر دووبارەبوونەوەیەک لە شیعرەکەدا چینێکی نوێی مانا زیاد دەکات، نەک تەنها دێڕی پێشووتر بەهێز بکات.','Each repetition in the poem adds a new layer of meaning instead of merely reinforcing the previous line.','هر تکرار در شعر لایه معنایی تازه‌ای می‌افزاید، نه اینکه فقط سطر قبلی را تقویت کند.','Her dubarekirin di helbestê de qatek nû ya wateyê zêde dike, ne tenê rêza berê xurt dike.','Każde powtórzenie w wierszu dodaje nową warstwę sensu, zamiast tylko wzmacniać poprzedni wers.','Fiecare repetiție din poem adaugă un nou strat de sens, în loc să întărească doar versul anterior.','Каждое повторение в стихотворении добавляет новый смысловой слой, а не просто усиливает предыдущую строку.','Çdo përsëritje në poezi shton një shtresë të re kuptimi, në vend që vetëm të forcojë vargun e mëparshëm.','Şiirdeki her tekrar, önceki dizeyi yalnızca güçlendirmek yerine yeni bir anlam katmanı ekler.'))
    ]
  }),
  entry({
    word: 'die Sinnstiftung', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Sinn-stif-tung',
    topics: ['management-and-leadership','advanced-analysis','social-and-relationships'], usageLabels: ['formal','written','academic','analysis'],
    grammarNotes: ['feminine abstract noun; normally used in singular'],
    collocations: [{ text: 'kollektive Sinnstiftung', meaning: 'collective creation of meaning' }],
    meanings: meaning('إضفاء المعنى؛ خلق معنى مشترك','مانادان؛ دروستکردنی مانای هاوبەش','meaning-making; creation of meaning','معناسازی؛ معنا بخشیدن','wate afirandin; wate dayîn','nadawanie sensu','creare de sens; conferire de sens','создание смысла; придание смысла','krijim kuptimi; dhënie kuptimi','anlam kurma; anlam verme'),
    examples: [
      ex('Nach der Umstrukturierung fehlte nicht nur ein Prozessplan, sondern auch eine glaubwürdige Sinnstiftung für die neue Organisation.', meaning('بعد إعادة الهيكلة لم يكن ينقص فقط مخطط العمليات، بل أيضاً خلق معنى مقنع للتنظيم الجديد.','دوای ڕێکخستنەوەکە نەک تەنها پلانی پڕۆسە کەم بوو، بەڵکو مانادانێکی باوەڕپێکراو بۆ ڕێکخراوی نوێش نەبوو.','After the restructuring, what was missing was not only a process plan, but also credible meaning-making for the new organization.','پس از بازساختاردهی، فقط برنامه فرایندی کم نبود، بلکه معناسازی قابل‌باور برای سازمان جدید هم وجود نداشت.','Piştî ji nû ve rêxistinkirinê ne tenê plana pêvajoyê kêm bû, lê jî wate-afirandineke bawerbar ji bo rêxistina nû tune bû.','Po restrukturyzacji brakowało nie tylko planu procesów, lecz także wiarygodnego nadania sensu nowej organizacji.','După restructurare lipsea nu doar un plan de proces, ci și o creare credibilă de sens pentru noua organizație.','После реструктуризации не хватало не только плана процессов, но и убедительного создания смысла для новой организации.','Pas ristrukturimit mungonte jo vetëm një plan procesesh, por edhe krijim i besueshëm kuptimi për organizatën e re.','Yeniden yapılanmadan sonra yalnızca süreç planı değil, yeni organizasyon için inandırıcı bir anlam kurma da eksikti.')),
      ex('Der Roman zeigt Sinnstiftung als mühsamen sozialen Prozess, nicht als private Einsicht eines einzelnen Helden.', meaning('تُظهر الرواية إضفاء المعنى كعملية اجتماعية شاقة، لا كبصيرة خاصة لبطل واحد.','ڕۆمانەکە مانادان وەک پڕۆسەیەکی کۆمەڵایەتیی سەخت پیشان دەدات، نەک وەک تێگەیشتنی تایبەتی پاڵەوانێکی تاک.','The novel presents meaning-making as a laborious social process, not as the private insight of a single hero.','رمان معناسازی را فرایندی اجتماعی و دشوار نشان می‌دهد، نه بینش خصوصی یک قهرمان تنها.','Roman wate-afirandinê wek pêvajoyeke civakî ya zehmet nîşan dide, ne wek têgihiştina taybet a lehengê yekane.','Powieść ukazuje nadawanie sensu jako żmudny proces społeczny, a nie prywatne olśnienie jednego bohatera.','Romanul prezintă crearea de sens ca pe un proces social dificil, nu ca pe intuiția privată a unui singur erou.','Роман показывает создание смысла как трудный социальный процесс, а не как личное прозрение одного героя.','Romani e paraqet krijimin e kuptimit si proces të mundimshëm shoqëror, jo si njohje private të një heroi të vetëm.','Roman, anlam kurmayı tek bir kahramanın özel kavrayışı değil, zahmetli bir toplumsal süreç olarak gösterir.'))
    ]
  }),
  entry({
    word: 'skizzieren', partOfSpeech: 'Verb', infinitive: 'skizzieren', syllableBreak: 'skiz-zie-ren',
    topics: ['meetings-and-presentations','business-communication','planning-and-projects'], usageLabels: ['formal','written','business','academic'],
    collocations: [{ text: 'einen Lösungsansatz skizzieren', meaning: 'to outline a solution approach' }],
    meanings: meaning('يرسم خطوطاً عريضة؛ يوجز','هێڵکاری کردن؛ بە کورتی وێناکردن','to sketch; to outline','طرح کلی دادن؛ ترسیم اجمالی کردن','xêzkirin; bi kurtî ravekirin','naszkicować; zarysować','a schița; a contura','набросать; очертить','skicoj; përvijoj','taslak çizmek; ana hatlarıyla anlatmak'),
    examples: [
      ex('Bitte skizzieren Sie bis morgen einen Migrationspfad, der Risiken, Abhängigkeiten und notwendige Tests sichtbar macht.', meaning('يرجى رسم مسار ترحيل إجمالي حتى الغد يوضح المخاطر والتبعيات والاختبارات اللازمة.','تکایە تا سبەی ڕێڕەوی کۆچکردنێک بە کورتی وێنا بکەن کە مەترسی، پەیوەندی و تاقیکردنەوە پێویستەکان دیار بکات.','Please outline by tomorrow a migration path that makes risks, dependencies, and necessary tests visible.','لطفاً تا فردا مسیر مهاجرتی را در خطوط کلی ترسیم کنید که ریسک‌ها، وابستگی‌ها و آزمون‌های لازم را نشان دهد.','Ji kerema xwe heta sibê rêya koçberiyê xêz bikin ku xeter, girêdan û testên pêwîst xuya bike.','Proszę do jutra naszkicować ścieżkę migracji, która pokaże ryzyka, zależności i konieczne testy.','Vă rog să schițați până mâine o cale de migrare care să facă vizibile riscurile, dependențele și testele necesare.','Пожалуйста, к завтрашнему дню наметьте путь миграции, показывающий риски, зависимости и необходимые тесты.','Ju lutem skiconi deri nesër një rrugë migrimi që bën të dukshme rreziqet, varësitë dhe testet e nevojshme.','Lütfen yarına kadar riskleri, bağımlılıkları ve gerekli testleri görünür kılan bir geçiş yolu taslağı çıkarın.')),
      ex('Die Autorin skizziert die Kindheit der Figur nur knapp, lässt aber genug Leerstellen für spätere Deutungen.', meaning('ترسم الكاتبة طفولة الشخصية بإيجاز شديد، لكنها تترك فراغات كافية لتأويلات لاحقة.','نووسەرەکە منداڵیی کارەکتەرەکە تەنها بە کورتی وێنا دەکات، بەڵام بۆ لێکدانەوەی دواتر بۆشاییی پێویست جێدەهێڵێت.','The author sketches the character’s childhood only briefly, but leaves enough gaps for later interpretations.','نویسنده کودکی شخصیت را فقط کوتاه ترسیم می‌کند، اما خلأهای کافی برای تفسیرهای بعدی می‌گذارد.','Nivîskar zaroktiya kesayetê tenê bi kurtî xêz dike, lê têra vala cih dihêle ji bo şîroveyên paşê.','Autorka szkicuje dzieciństwo postaci tylko krótko, ale zostawia dość luk dla późniejszych interpretacji.','Autoarea schițează copilăria personajului doar pe scurt, dar lasă suficiente goluri pentru interpretări ulterioare.','Авторка лишь кратко набрасывает детство персонажа, но оставляет достаточно пробелов для последующих интерпретаций.','Autorja e skicon fëmijërinë e personazhit vetëm shkurt, por lë mjaft boshllëqe për interpretime të mëvonshme.','Yazar karakterin çocukluğunu yalnızca kısaca çizer, ancak sonraki yorumlar için yeterince boşluk bırakır.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 073', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

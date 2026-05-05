const fs = require('fs');
const path = require('path');
const cp = require('child_process');
const root = 'D:/_Projects/DarwinLingua';
const level = 'C2', levelLower = 'c2', batch = '082';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['die Unbestimmtheit','unbotmäßig','die Unbotmäßigkeit','die Uneindeutigkeit','die Unermesslichkeit','unerschöpflich'];
const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const topicKeys = new Set((taxonomy.topics || []).map(t => t.key));
const labelsByKey = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const source = fs.readFileSync(sourcePath, 'utf8');
const tokens = source.split(',').map(s => s.trim()).filter(Boolean);
if (JSON.stringify(tokens.slice(0, 6)) !== JSON.stringify(expected)) throw new Error(`Unexpected first tokens: ${JSON.stringify(tokens.slice(0, 6))}`);
function m(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) { return [ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr].map((text, i) => ({ language: langs[i], text })); }
function ex(baseText, translations) { return { baseText, translations }; }
function entry(e) {
  for (const t of e.topics) if (!topicKeys.has(t)) throw new Error(`Unknown topic ${t}`);
  for (const l of [...(e.usageLabels || []), ...(e.contextLabels || [])]) if (!labelsByKey.has(l)) throw new Error(`Unknown label ${l}`);
  return Object.assign({ language: 'de', cefrLevel: level, article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: null, contextLabels: [], grammarNotes: [], collocations: [], wordFamilies: [], relations: [] }, e);
}
const entries = [
  entry({
    word: 'die Unbestimmtheit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Un-be-stimmt-heit',
    topics: ['advanced-analysis','law-and-compliance','culture-and-media'], usageLabels: ['formal','written','analysis','academic'], grammarNotes: ['feminine abstract noun; normally singular'],
    collocations: [{ text: 'semantische Unbestimmtheit', meaning: 'semantic indeterminacy' }],
    meanings: m('عدم التحديد؛ غموض الحدود','نادیاری؛ سنوور نادیاربوون','indeterminacy; lack of definiteness','نامعینی؛ مشخص نبودن','nediarbûn; nezelalî','nieokreśloność','nedeterminare; lipsă de precizie','неопределенность','papërcaktueshmëri','belirsizlik; belirlenmemişlik'),
    examples: [
      ex('Die Unbestimmtheit der Vertragsklausel führte dazu, dass beide Parteien dieselbe Passage unterschiedlich auslegten.', m('أدى عدم تحديد بند العقد إلى أن يفسر الطرفان المقطع نفسه بشكل مختلف.','نادیاری ماددەی گرێبەستەکە وای کرد هەردوو لایەن هەمان بەش بە جیاوازی لێکبدەنەوە.','The indeterminacy of the contract clause led both parties to interpret the same passage differently.','نامعینی بند قرارداد باعث شد هر دو طرف همان بخش را متفاوت تفسیر کنند.','Nediarbûna benda peymanê kir ku her du alî heman beşê cuda şîrove bikin.','Nieokreśloność klauzuli umownej sprawiła, że obie strony różnie interpretowały ten sam fragment.','Nedeterminarea clauzei contractuale a făcut ca ambele părți să interpreteze diferit același pasaj.','Неопределенность договорного пункта привела к тому, что обе стороны по-разному истолковали один и тот же фрагмент.','Papërcaktueshmëria e klauzolës së kontratës bëri që të dy palët ta interpretonin ndryshe të njëjtin pasazh.','Sözleşme maddesinin belirsizliği, iki tarafın aynı bölümü farklı yorumlamasına yol açtı.')),
      ex('Im Gedicht bleibt die Unbestimmtheit der Stimme produktiv, weil sie mehrere Lesarten zugleich zulässt.', m('يبقى عدم تحديد الصوت في القصيدة منتجاً لأنه يسمح بعدة قراءات في الوقت نفسه.','لە شیعرەکەدا نادیاری دەنگەکە بەرهەمدار دەمێنێتەوە، چونکە چەند خوێندنەوەیەک هاوکات ڕێپێدەدات.','In the poem, the indeterminacy of the voice remains productive because it allows several readings at once.','در شعر، نامعینی صدا سازنده می‌ماند، چون چند خوانش را هم‌زمان ممکن می‌کند.','Di helbestê de nediarbûna dengê berhemdar dimîne, ji ber ku çend xwendinan bi hev re rê dide.','W wierszu nieokreśloność głosu pozostaje produktywna, ponieważ dopuszcza jednocześnie kilka odczytań.','În poem, nedeterminarea vocii rămâne productivă deoarece permite simultan mai multe lecturi.','В стихотворении неопределенность голоса остается продуктивной, потому что допускает сразу несколько прочтений.','Në poezi, papërcaktueshmëria e zërit mbetet produktive, sepse lejon disa lexime njëkohësisht.','Şiirde sesin belirsizliği üretken kalır, çünkü aynı anda birkaç okumaya izin verir.'))
    ]
  }),
  entry({
    word: 'unbotmäßig', partOfSpeech: 'Adjective', syllableBreak: 'un-bot-mä-ßig',
    topics: ['law-and-compliance','work-and-jobs','social-and-relationships'], usageLabels: ['formal','written','sensitive','advanced'],
    collocations: [{ text: 'unbotmäßiges Verhalten', meaning: 'insubordinate behavior' }],
    meanings: m('عاصٍ؛ غير مطيع للسلطة','گوێنەگر؛ یاخی لە فەرمان','insubordinate; disobedient','نافرمان؛ سرکش در برابر اقتدار','guhnedar; serhildêr li hember desthilatê','nieposłuszny; krnąbrny','nesupus; recalcitrant','непокорный; неподчиняющийся','i pabindur; kryengritës','itaatsiz; başına buyruk'),
    examples: [
      ex('Die Personalabteilung vermied den Begriff unbotmäßig, weil er den berechtigten Widerspruch des Mitarbeiters abgewertet hätte.', m('تجنبت إدارة الموارد البشرية مصطلح عاصٍ لأنه كان سيقلل من قيمة اعتراض الموظف المشروع.','بەشی سەرچاوە مرۆییەکان وشەی گوێنەگر بەکارنەهێنا، چونکە ناڕەزایی ڕەوای کارمەندەکەی کەمبەها دەکرد.','HR avoided the term insubordinate because it would have devalued the employee’s legitimate objection.','واحد منابع انسانی از واژه نافرمان پرهیز کرد، چون اعتراض موجه کارمند را بی‌ارزش جلوه می‌داد.','Beşa HR peyva guhnedar bi kar neanî, ji ber ku îtiraza rewa ya karmendê bêqîmet dikir.','Dział HR uniknął określenia nieposłuszny, ponieważ umniejszyłoby ono uzasadniony sprzeciw pracownika.','Departamentul HR a evitat termenul nesupus, deoarece ar fi depreciat obiecția legitimă a angajatului.','Отдел кадров избегал слова непокорный, потому что оно обесценило бы обоснованное возражение сотрудника.','Departamenti i burimeve njerëzore shmangu termin i pabindur, sepse do ta zhvlerësonte kundërshtimin e ligjshëm të punonjësit.','İK departmanı itaatsiz teriminden kaçındı, çünkü bu çalışanın meşru itirazını değersizleştirecekti.')),
      ex('Die Figur wirkt unbotmäßig, doch ihr Widerstand richtet sich gegen eine Ordnung, die selbst ungerecht ist.', m('تبدو الشخصية عاصية، لكن مقاومتها موجهة ضد نظام ظالم في ذاته.','کارەکتەرەکە گوێنەگر دەردەکەوێت، بەڵام بەربەرەکانییەکەی دژی سیستەمێکە کە خۆی نادادپەروەرە.','The character seems insubordinate, but her resistance is directed against an order that is itself unjust.','شخصیت نافرمان به نظر می‌رسد، اما مقاومتش علیه نظمی است که خود ناعادلانه است.','Kesayet guhnedar xuya dike, lê berxwedana wê li dijî rêzikekê ye ku bixwe nedadil e.','Postać wydaje się nieposłuszna, lecz jej opór jest skierowany przeciw porządkowi, który sam jest niesprawiedliwy.','Personajul pare nesupus, dar rezistența lui se îndreaptă împotriva unei ordini care este ea însăși nedreaptă.','Персонаж кажется непокорным, но его сопротивление направлено против порядка, который сам несправедлив.','Personazhi duket i pabindur, por rezistenca e tij drejtohet kundër një rendi që vetë është i padrejtë.','Karakter itaatsiz görünür, ama direnişi bizzat adaletsiz olan bir düzene yöneliktir.'))
    ]
  }),
  entry({
    word: 'die Unbotmäßigkeit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Un-bot-mä-ßig-keit',
    topics: ['law-and-compliance','management-and-leadership','social-and-relationships'], usageLabels: ['formal','written','sensitive','advanced'], grammarNotes: ['feminine abstract noun; normally singular'],
    collocations: [{ text: 'als Unbotmäßigkeit auslegen', meaning: 'to interpret as insubordination' }],
    meanings: m('عصيان؛ عدم خضوع للسلطة','گوێنەگری؛ یاخیبوون لە فەرمان','insubordination; disobedience','نافرمانی؛ سرپیچی از اقتدار','guhnedarî; serhildan li hember fermanê','nieposłuszeństwo','nesupunere','неповиновение','pabindshmëri','itaatsizlik'),
    examples: [
      ex('Die Führung interpretierte Nachfragen zur Strategie vorschnell als Unbotmäßigkeit und verlor dadurch wichtige Hinweise aus dem Team.', m('فسرت القيادة الأسئلة حول الاستراتيجية بسرعة مفرطة كعصيان، وبذلك فقدت ملاحظات مهمة من الفريق.','سەرکردایەتی پرسیارەکانی دەربارەی ستراتیژی بە پەلە وەک گوێنەگری لێکدایەوە و بەوەش ئاماژە گرنگەکانی تیمی لەدەستدا.','Leadership too quickly interpreted questions about the strategy as insubordination and thereby lost important signals from the team.','مدیریت پرسش‌های مربوط به راهبرد را شتاب‌زده نافرمانی تلقی کرد و به این ترتیب نشانه‌های مهمی از تیم را از دست داد.','Rêveberî pirsên li ser stratejiyê zû wek guhnedarî şîrove kir û bi vê yekê nîşanên girîng ji tîmê winda kir.','Kierownictwo zbyt pochopnie uznało pytania o strategię za nieposłuszeństwo i przez to straciło ważne sygnały z zespołu.','Conducerea a interpretat prea repede întrebările despre strategie ca nesupunere și a pierdut astfel semnale importante din echipă.','Руководство поспешно истолковало вопросы о стратегии как неповиновение и тем самым потеряло важные сигналы от команды.','Drejtimi i interpretoi shumë shpejt pyetjet për strategjinë si pabindshmëri dhe humbi sinjale të rëndësishme nga ekipi.','Yönetim stratejiye ilişkin soruları aceleyle itaatsizlik olarak yorumladı ve böylece ekipten gelen önemli sinyalleri kaybetti.')),
      ex('Im Drama wird Unbotmäßigkeit nicht bestraft, sondern als notwendige Voraussetzung moralischer Selbstständigkeit gezeigt.', m('في المسرحية لا يُعاقب العصيان، بل يُعرض كشرط ضروري للاستقلال الأخلاقي.','لە شانۆنامەکەدا گوێنەگری سزا نادرێت، بەڵکو وەک مەرجێکی پێویست بۆ سەربەخۆیی ئەخلاقی پیشان دەدرێت.','In the drama, insubordination is not punished but shown as a necessary condition of moral independence.','در نمایش، نافرمانی مجازات نمی‌شود، بلکه شرط ضروری استقلال اخلاقی نشان داده می‌شود.','Di dramayê de guhnedarî nayê cezakirin, lê wek merca pêwîst a serxwebûna exlaqî tê nîşandan.','W dramacie nieposłuszeństwo nie zostaje ukarane, lecz pokazane jako konieczny warunek moralnej samodzielności.','În dramă, nesupunerea nu este pedepsită, ci prezentată ca o condiție necesară a autonomiei morale.','В драме неповиновение не наказывается, а показывается как необходимое условие нравственной самостоятельности.','Në dramë, pabindshmëria nuk ndëshkohet, por paraqitet si kusht i domosdoshëm i pavarësisë morale.','Dramda itaatsizlik cezalandırılmaz, ahlaki bağımsızlığın gerekli koşulu olarak gösterilir.'))
    ]
  }),
  entry({
    word: 'die Uneindeutigkeit', partOfSpeech: 'Noun', article: 'die', plural: 'Uneindeutigkeiten', syllableBreak: 'Un-ein-deu-tig-keit',
    topics: ['advanced-analysis','business-communication','culture-and-media'], usageLabels: ['formal','written','analysis','academic'],
    collocations: [{ text: 'mit Uneindeutigkeit umgehen', meaning: 'to deal with ambiguity' }],
    meanings: m('التباس؛ عدم أحادية المعنى','دوومانایی؛ ناڕوونی مانا','ambiguity; non-univocality','ابهام؛ چندمعنایی','duwateyî; nezelaliya wateyê','niejednoznaczność','ambiguitate','неоднозначность','dykuptimësi; paqartësi','belirsizlik; çok anlamlılık'),
    examples: [
      ex('Die Uneindeutigkeit der Fehlermeldung führte dazu, dass Support und Entwicklung unterschiedliche Ursachen verfolgten.', m('أدى التباس رسالة الخطأ إلى أن الدعم والتطوير تابعا أسباباً مختلفة.','دوومانایی پەیامی هەڵە وای کرد پشتگیری و پەرەپێدان بەدوای هۆکاری جیاوازدا بچن.','The ambiguity of the error message led support and development to pursue different causes.','ابهام پیام خطا باعث شد پشتیبانی و توسعه علت‌های متفاوتی را دنبال کنند.','Duwateyîya peyama çewtiyê kir ku piştgirî û pêşvebirin sedemên cuda bişopînin.','Niejednoznaczność komunikatu błędu sprawiła, że wsparcie i development szukały różnych przyczyn.','Ambiguitatea mesajului de eroare a făcut ca suportul și dezvoltarea să urmărească cauze diferite.','Неоднозначность сообщения об ошибке привела к тому, что поддержка и разработка искали разные причины.','Dykuptimësia e mesazhit të gabimit bëri që suporti dhe zhvillimi të ndiqnin shkaqe të ndryshme.','Hata mesajının belirsizliği, destek ve geliştirme ekiplerinin farklı nedenlerin peşine düşmesine yol açtı.')),
      ex('Die Uneindeutigkeit des Endes macht die Erzählung stärker, weil sie keine einfache moralische Bilanz erlaubt.', m('يجعل التباس النهاية السرد أقوى لأنه لا يسمح بحصيلة أخلاقية بسيطة.','دوومانایی کۆتایی گێڕانەوەکە بەهێزتر دەکات، چونکە ڕێ بە کۆکردنەوەی ئەخلاقیی سادە نادات.','The ambiguity of the ending makes the story stronger because it allows no simple moral accounting.','ابهام پایان روایت را قوی‌تر می‌کند، چون اجازه جمع‌بندی اخلاقی ساده نمی‌دهد.','Duwateyîya dawiyê vegotinê xurtir dike, ji ber ku destûr nade nirxandineke exlaqî ya sade.','Niejednoznaczność zakończenia wzmacnia opowieść, ponieważ nie pozwala na prosty bilans moralny.','Ambiguitatea finalului face povestirea mai puternică deoarece nu permite un bilanț moral simplu.','Неоднозначность финала делает рассказ сильнее, потому что не допускает простого морального итога.','Dykuptimësia e fundit e bën rrëfimin më të fortë, sepse nuk lejon një bilanc të thjeshtë moral.','Sonun belirsizliği anlatıyı güçlendirir, çünkü basit bir ahlaki bilanço çıkarılmasına izin vermez.'))
    ]
  }),
  entry({
    word: 'die Unermesslichkeit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Un-er-mess-lich-keit',
    topics: ['advanced-analysis','culture-and-media','environment-and-sustainability'], usageLabels: ['formal','written','advanced','analysis'], grammarNotes: ['feminine abstract noun; normally singular'],
    collocations: [{ text: 'die Unermesslichkeit des Raums', meaning: 'the immeasurability of space' }],
    meanings: m('لا نهائية؛ ضخامة لا تُقاس','بێسنووری؛ پێوانەناکرێتی','immeasurability; vastness','بی‌کرانگی؛ عظمت سنجش‌ناپذیر','bêsînorî; nepîvanbarî','niezmierzoność','nemărginire; imensitate','неизмеримость; необъятность','pamasa; pafundësi','ölçülemezlik; enginlik'),
    examples: [
      ex('Die Unermesslichkeit der Datenbestände beeindruckte zunächst, erschwerte aber jede belastbare Priorisierung.', m('أثارت ضخامة مخزونات البيانات الإعجاب في البداية، لكنها صعّبت كل ترتيب موثوق للأولويات.','بێسنووری کۆگاکانی داتا سەرەتا سەرسوڕهێنەر بوو، بەڵام هەر پێشینەییەکی پشتپێبەستوی قورس کرد.','The vastness of the data repositories was impressive at first, but made every reliable prioritization harder.','بی‌کرانگی ذخیره‌های داده ابتدا چشمگیر بود، اما هر اولویت‌بندی قابل اتکا را دشوار کرد.','Bêsînorîya depoyên daneyan pêşî ecêb bû, lê her pêşîtiyeke bawerbar dijwartir kir.','Niezmierzoność zasobów danych początkowo imponowała, ale utrudniała każdą wiarygodną priorytetyzację.','Imensitatea depozitelor de date a impresionat inițial, dar a îngreunat orice prioritizare solidă.','Необъятность массивов данных сначала впечатляла, но затрудняла любую надежную приоритизацию.','Pafundësia e rezervave të të dhënave fillimisht impresionoi, por e vështirësoi çdo prioritizim të besueshëm.','Veri depolarının enginliği ilk başta etkileyiciydi, ancak güvenilir önceliklendirmeyi zorlaştırdı.')),
      ex('Die Unermesslichkeit des Meeres wird im Gedicht nicht beschrieben, sondern durch wiederholte Leerstellen erfahrbar gemacht.', m('لا تُوصف لا نهائية البحر في القصيدة، بل تُجعل محسوسة عبر فراغات متكررة.','بێسنووری دەریا لە شیعرەکەدا وەسف ناکرێت، بەڵکو لە ڕێگەی بۆشایی دووبارەوە هەستپێدەکرێت.','The vastness of the sea is not described in the poem, but made palpable through repeated gaps.','بی‌کرانگی دریا در شعر توصیف نمی‌شود، بلکه از طریق خلأهای تکرارشونده تجربه‌پذیر می‌شود.','Bêsînorîya deryayê di helbestê de nayê şirovekirin, lê bi valahiyên dubare tê hestkirin.','Niezmierzoność morza nie jest w wierszu opisana, lecz uobecniona przez powtarzające się luki.','Nemărginirea mării nu este descrisă în poem, ci făcută perceptibilă prin goluri repetate.','Необъятность моря в стихотворении не описана, а дана через повторяющиеся пустоты.','Pafundësia e detit nuk përshkruhet në poezi, por bëhet e ndjeshme përmes boshllëqeve të përsëritura.','Denizin enginliği şiirde betimlenmez, tekrarlanan boşluklarla hissedilir kılınır.'))
    ]
  }),
  entry({
    word: 'unerschöpflich', partOfSpeech: 'Adjective', syllableBreak: 'un-er-schöpf-lich',
    topics: ['advanced-analysis','culture-and-media','education-and-training'], usageLabels: ['formal','written','advanced','analysis'],
    collocations: [{ text: 'eine unerschöpfliche Quelle', meaning: 'an inexhaustible source' }],
    meanings: m('لا ينضب؛ لا ينتهي','نەبڕاوە؛ کۆتایی نەهاتن','inexhaustible; never-ending','تمام‌نشدنی؛ بی‌پایان','neqedayî; bêdawî','niewyczerpany','inepuizabil','неисчерпаемый','i pashtershëm','tükenmez'),
    examples: [
      ex('Die Fehlerberichte sind eine unerschöpfliche Quelle für Verbesserungen, wenn sie nicht nur als Störung betrachtet werden.', m('تُعد تقارير الأخطاء مصدراً لا ينضب للتحسينات إذا لم تُنظر إليها فقط كإزعاج.','ڕاپۆرتەکانی هەڵە سەرچاوەیەکی نەبڕاوەن بۆ باشکردنەوە، ئەگەر تەنها وەک شڵەژان سەیر نەکرێن.','Bug reports are an inexhaustible source of improvements if they are not viewed merely as disruption.','گزارش‌های خطا منبعی تمام‌نشدنی برای بهبودند، اگر فقط مزاحمت تلقی نشوند.','Raporên çewtiyan çavkaniyeke neqedayî ne ji bo başkirinan, heger tenê wek astengî neyên dîtin.','Raporty błędów są niewyczerpanym źródłem ulepszeń, jeśli nie traktuje się ich tylko jako zakłócenia.','Rapoartele de erori sunt o sursă inepuizabilă de îmbunătățiri dacă nu sunt privite doar ca perturbări.','Отчеты об ошибках — неисчерпаемый источник улучшений, если их не рассматривать лишь как помеху.','Raportet e gabimeve janë burim i pashtershëm për përmirësime nëse nuk shihen vetëm si pengesë.','Hata raporları yalnızca rahatsızlık olarak görülmezse tükenmez bir iyileştirme kaynağıdır.')),
      ex('Für die Erzählerin ist die Kindheit ein unerschöpflicher Vorrat an Bildern, aber kein Ort der Unschuld.', m('بالنسبة إلى الراوية الطفولة مخزون لا ينضب من الصور، لكنها ليست مكاناً للبراءة.','بۆ گێڕەرەوەکە منداڵی کۆگایەکی نەبڕاوەی وێنەیە، بەڵام شوێنی بێگوناهایی نییە.','For the narrator, childhood is an inexhaustible store of images, but not a place of innocence.','برای راوی، کودکی ذخیره‌ای تمام‌نشدنی از تصویرهاست، اما جای معصومیت نیست.','Ji bo vegêrê zaroktî depoyeke neqedayî ya wêneyan e, lê ne cihê bêgunehiyê ye.','Dla narratorki dzieciństwo jest niewyczerpanym zasobem obrazów, ale nie miejscem niewinności.','Pentru naratoare, copilăria este un depozit inepuizabil de imagini, dar nu un loc al inocenței.','Для рассказчицы детство — неисчерпаемый запас образов, но не место невинности.','Për rrëfimtaren, fëmijëria është rezervë e pashtershme imazhesh, por jo vend pafajësie.','Anlatıcı için çocukluk tükenmez bir imgeler deposudur, ama masumiyet yeri değildir.'))
    ]
  })
];
const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId: `de-${levelLower}-generated-batch-${batch}`, packageName: 'German C2 Generated Batch 082', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');
const cmd = `dotnet run --project "${path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj')}" -- --target shared --yes "${outPath}"`;
const result = cp.spawnSync(cmd, { shell: true, encoding: 'utf8', cwd: root });
const output = `${result.stdout || ''}${result.stderr || ''}`;
process.stdout.write(output);
const ok = result.status === 0 && output.includes('Entries imported: 6') && output.includes('Entries invalid: 0') && output.includes('Warnings: 0');
if (!ok) {
  fs.appendFileSync(path.join(root, 'content', 'generated', `${levelLower}-failed-words.txt`), `${new Date().toISOString()}\tbatch-${batch}\t${expected.join(', ')}\n`, 'utf8');
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

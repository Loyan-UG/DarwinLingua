const fs = require('fs');
const path = require('path');
const cp = require('child_process');
const root = 'D:/_Projects/DarwinLingua';
const level = 'C2', levelLower = 'c2', batch = '087';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['verklären','verklärt','verkneifen','verlautbaren','verleumden','das Vermächtnis'];
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
    word: 'verklären', partOfSpeech: 'Verb', infinitive: 'verklären', syllableBreak: 'ver-klä-ren',
    topics: ['culture-and-media','advanced-analysis','business-communication'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'die Vergangenheit verklären', meaning: 'to idealize the past' }],
    meanings: m('يمجّد بصورة مثالية؛ يضفي طابعاً وردياً','ئیدیاڵکردن؛ بە جوانیی زیادەوە پیشاندان','to idealize; to romanticize','آرمانی جلوه دادن؛ زیبا‌سازی اغراق‌آمیز','îdealîze kirin; bi awayekî xweş zêde nîşan dan','idealizować; upiększać','a idealiza; a romanța','идеализировать; приукрашивать','idealizoj; zbukuroj tepër','idealleştirmek; romantize etmek'),
    examples: [
      ex('Das Management sollte die Anfangsphase des Start-ups nicht verklären, denn viele damalige Entscheidungen waren schlicht improvisiert.', m('لا ينبغي للإدارة أن تمجّد المرحلة الأولى للشركة الناشئة، فكثير من القرارات آنذاك كانت مرتجلة ببساطة.','بەڕێوەبەرایەتی نابێت قۆناغی سەرەتای ستارتاپەکە ئیدیاڵ بکات، چونکە زۆرێک لە بڕیارەکانی ئەوکات تەنها هەڵبەستراو بوون.','Management should not romanticize the start-up’s early phase, because many decisions back then were simply improvised.','مدیریت نباید مرحله آغازین استارتاپ را آرمانی جلوه دهد، چون بسیاری از تصمیم‌های آن زمان صرفاً بداهه بودند.','Rêveberî divê qonaxa destpêkê ya startupê îdealîze neke, ji ber ku gelek biryarên wê demê tenê improviseyî bûn.','Kierownictwo nie powinno idealizować początkowej fazy start-upu, bo wiele ówczesnych decyzji było po prostu improwizowanych.','Managementul nu ar trebui să idealizeze faza de început a start-upului, deoarece multe decizii de atunci au fost pur și simplu improvizate.','Руководству не следует идеализировать ранний этап стартапа, потому что многие тогдашние решения были просто импровизированными.','Menaxhmenti nuk duhet ta idealizojë fazën fillestare të start-up-it, sepse shumë vendime të atëhershme ishin thjesht të improvizuara.','Yönetim start-up’ın ilk dönemini romantize etmemeli, çünkü o zamanki birçok karar basitçe doğaçlamaydı.')),
      ex('Der Roman verklärt die Kindheit nicht, sondern zeigt, wie Erinnerung Trost und Täuschung zugleich erzeugt.', m('لا يجمّل الرواية الطفولة، بل يبين كيف تنتج الذاكرة عزاءً وخداعاً في الوقت نفسه.','ڕۆمانەکە منداڵی ئیدیاڵ ناکات، بەڵکو پیشان دەدات بیرەوەری چۆن هاوکات دڵنەوایی و خەڵەتاندن دروست دەکات.','The novel does not idealize childhood, but shows how memory creates comfort and deception at the same time.','رمان کودکی را آرمانی نمی‌کند، بلکه نشان می‌دهد خاطره چگونه هم‌زمان تسلی و فریب می‌سازد.','Roman zaroktî îdealîze nake, lê nîşan dide ka bîranîn çawa bi hev re dilxweşî û xapandin diafirîne.','Powieść nie idealizuje dzieciństwa, lecz pokazuje, jak pamięć jednocześnie tworzy pocieszenie i złudzenie.','Romanul nu idealizează copilăria, ci arată cum memoria produce simultan consolare și înșelare.','Роман не идеализирует детство, а показывает, как память одновременно создает утешение и обман.','Romani nuk e idealizon fëmijërinë, por tregon si kujtesa krijon njëkohësisht ngushëllim dhe mashtrim.','Roman çocukluğu idealleştirmez; hafızanın aynı anda teselli ve aldanış yarattığını gösterir.'))
    ]
  }),
  entry({
    word: 'verklärt', partOfSpeech: 'Adjective', syllableBreak: 'ver-klärt',
    topics: ['culture-and-media','advanced-analysis','social-and-relationships'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'ein verklärter Blick', meaning: 'an idealized or transfigured view' }],
    meanings: m('مثالي بصورة غير واقعية؛ مُمجَّد','ئیدیاڵکراو؛ بە ڕووناکیی زیادەوە بینراو','idealized; transfigured','آرمانی‌شده؛ زیبا شده در خیال','îdealîzekirî; bi xeyal xweşkirî','wyidealizowany','idealizat; transfigurat','идеализированный; просветленный','i idealizuar','idealleştirilmiş'),
    examples: [
      ex('Der verklärte Blick auf frühere Release-Zyklen verdeckte, wie oft damals heimlich nachgebessert wurde.', m('حجب النظر المثالي إلى دورات الإصدار السابقة عدد المرات التي كانت تُجرى فيها إصلاحات سرية آنذاك.','دیدگای ئیدیاڵکراو بۆ release cycle ـەکانی پێشوو ئەوەی داپۆشی کە ئەوکات چەند جار بە نهێنی چاکسازی دەکرا.','The idealized view of earlier release cycles concealed how often hidden fixes were made back then.','نگاه آرمانی‌شده به چرخه‌های انتشار قبلی پنهان کرد که آن زمان چقدر اصلاحات مخفی انجام می‌شد.','Nêrîna îdealîzekirî li cycleên release ên berê veşart ku wê demê çend caran bi dizî rastkirin dihat kirin.','Wyidealizowane spojrzenie na wcześniejsze cykle wydań ukrywało, jak często wtedy po cichu poprawiano błędy.','Privirea idealizată asupra ciclurilor de release anterioare ascundea cât de des se făceau atunci corecții pe ascuns.','Идеализированный взгляд на прежние циклы релизов скрывал, как часто тогда тайно дорабатывали.','Vështrimi i idealizuar mbi ciklet e mëparshme të release-it fshehu sa shpesh bëheshin korrigjime fshehurazi.','Önceki sürüm döngülerine idealleştirilmiş bakış, o dönemde ne kadar sık gizlice düzeltme yapıldığını gizledi.')),
      ex('Die verklärte Erinnerung an den Vater bricht zusammen, als die Briefe seine Feigheit offenlegen.', m('تنهار الذكرى المثالية عن الأب عندما تكشف الرسائل جبنه.','بیرەوەری ئیدیاڵکراوی باوک دەڕووخێت کاتێک نامەکان ترسنۆکییەکەی ئاشکرا دەکەن.','The idealized memory of the father collapses when the letters reveal his cowardice.','خاطره آرمانی‌شده از پدر وقتی نامه‌ها بزدلی او را آشکار می‌کنند فرو می‌ریزد.','Bîranîna îdealîzekirî ya bavê diherife dema name tirsonekiya wî eşkere dikin.','Wyidealizowane wspomnienie ojca rozpada się, gdy listy ujawniają jego tchórzostwo.','Amintirea idealizată a tatălui se prăbușește când scrisorile îi dezvăluie lașitatea.','Идеализированная память об отце рушится, когда письма раскрывают его трусость.','Kujtimi i idealizuar për babanë shembet kur letrat zbulojnë frikacakërinë e tij.','Babaya dair idealleştirilmiş anı, mektuplar onun korkaklığını ortaya çıkarınca çöker.'))
    ]
  }),
  entry({
    word: 'verkneifen', partOfSpeech: 'Verb', infinitive: 'verkneifen', syllableBreak: 'ver-knei-fen',
    topics: ['business-communication','social-and-relationships','work-and-jobs'], usageLabels: ['informal','workplace','sensitive','advanced'],
    grammarNotes: ['usually reflexive: sich etwas verkneifen'],
    collocations: [{ text: 'sich einen Kommentar verkneifen', meaning: 'to bite back a comment' }],
    meanings: m('يمسك نفسه عن قول أو فعل؛ يكبت','خۆگرتن لە گوتن یان کردن؛ دانەگرتن','to refrain from; to bite back','خودداری کردن؛ جلوی خود را گرفتن','xwe girtin; negotin','powstrzymać się','a se abține','сдержаться; удержаться','përmbahem','kendini tutmak; söylemekten kaçınmak'),
    examples: [
      ex('Im Kundentermin verkniff sich der Entwickler den sarkastischen Kommentar und erklärte stattdessen die technische Ursache ruhig.', m('في موعد العميل أمسك المطور نفسه عن التعليق الساخر وشرح بدلاً من ذلك السبب التقني بهدوء.','لە دانیشتنی کڕیاردا پەرەپێدەرەکە خۆی لە کۆمێنتی گاڵتەئامێز گرت و لەبری ئەوە هۆکاری تەکنیکی بە ئارامی ڕوونکردەوە.','In the customer meeting, the developer bit back the sarcastic comment and instead calmly explained the technical cause.','در جلسه با مشتری، توسعه‌دهنده از گفتن نظر طعنه‌آمیز خودداری کرد و به‌جایش علت فنی را آرام توضیح داد.','Di civîna xerîdar de pêşvebir xwe ji şîroveya sarkastîk girt û li şûna wê sedema teknîkî bi aramî rave kir.','Podczas spotkania z klientem deweloper powstrzymał sarkastyczny komentarz i zamiast tego spokojnie wyjaśnił przyczynę techniczną.','În întâlnirea cu clientul, dezvoltatorul s-a abținut de la comentariul sarcastic și a explicat calm cauza tehnică.','На встрече с клиентом разработчик сдержал саркастический комментарий и вместо этого спокойно объяснил техническую причину.','Në takimin me klientin, zhvilluesi e përmbajti komentin sarkastik dhe në vend të tij shpjegoi qetë shkakun teknik.','Müşteri toplantısında geliştirici alaycı yorumu kendine sakladı ve bunun yerine teknik nedeni sakin biçimde açıkladı.')),
      ex('Sie konnte sich ein Lächeln nicht verkneifen, als der angeblich strenge Onkel heimlich den Hund fütterte.', m('لم تستطع حبس ابتسامة عندما أطعم العم الصارم المزعوم الكلب سراً.','نەیتوانی پێکەنینێک خۆی لێ بگرێت کاتێک مامی بە گوێرەی قسەکان توند بە نهێنی سەگەکەی خواردن دا.','She could not suppress a smile when the supposedly strict uncle secretly fed the dog.','نتوانست لبخندش را پنهان کند وقتی عموی به‌ظاهر سختگیر پنهانی به سگ غذا داد.','Wê nikarî kenekê bigire dema apê xuya tund bi dizî xwarin da kûçikê.','Nie mogła powstrzymać uśmiechu, gdy rzekomo surowy wujek potajemnie nakarmił psa.','Nu și-a putut reține zâmbetul când unchiul pretins sever a hrănit câinele pe ascuns.','Она не смогла сдержать улыбку, когда якобы строгий дядя тайком покормил собаку.','Ajo nuk mundi ta përmbante buzëqeshjen kur xhaxhai gjoja i rreptë ushqeu fshehurazi qenin.','Sözde sert amca köpeği gizlice besleyince gülümsemesini tutamadı.'))
    ]
  }),
  entry({
    word: 'verlautbaren', partOfSpeech: 'Verb', infinitive: 'verlautbaren', syllableBreak: 'ver-laut-ba-ren',
    topics: ['business-communication','documents-and-administration','law-and-compliance'], usageLabels: ['formal','written','administrative','business'],
    collocations: [{ text: 'offiziell verlautbaren', meaning: 'to announce officially' }],
    meanings: m('يعلن رسمياً؛ يصرح','بە فەرمی ڕاگەیاندن','to announce officially; to state publicly','رسماً اعلام کردن؛ اطلاعیه دادن','bi fermî ragihandin','oficjalnie ogłosić','a anunța oficial','официально объявлять','njoftoj zyrtarisht','resmen duyurmak'),
    examples: [
      ex('Das Unternehmen ließ verlautbaren, dass die Störung behoben sei, obwohl einzelne Kunden noch keinen Zugriff hatten.', m('أعلنت الشركة رسمياً أن العطل قد أُصلح، رغم أن بعض العملاء لم يكن لديهم وصول بعد.','کۆمپانیاکە ڕایگەیاند کە کێشەکە چارەسەر بووە، هەرچەندە هەندێک کڕیار هێشتا دەستیان نەگەیشتبوو.','The company announced that the incident had been resolved, although some customers still had no access.','شرکت اعلام کرد اختلال برطرف شده است، هرچند برخی مشتریان هنوز دسترسی نداشتند.','Şirket ragihand ku astengî hatiye çareserkirin, herçend hin xerîdar hîn gihîştin tune bûn.','Firma ogłosiła, że awaria została usunięta, choć niektórzy klienci nadal nie mieli dostępu.','Compania a anunțat că incidentul fusese remediat, deși unii clienți încă nu aveau acces.','Компания объявила, что сбой устранен, хотя у отдельных клиентов все еще не было доступа.','Kompania njoftoi se ndërprerja ishte zgjidhur, megjithëse disa klientë ende nuk kishin qasje.','Şirket, bazı müşterilerin hâlâ erişimi olmamasına rağmen arızanın giderildiğini duyurdu.')),
      ex('Im letzten Kapitel wird nur knapp verlautbart, dass der Prozess eingestellt wurde.', m('في الفصل الأخير يُعلن بإيجاز فقط أن القضية أُغلقت.','لە بەشی کۆتاییدا تەنها بە کورتی ڕادەگەیەنرێت کە پڕۆسەکە وەستێنراوە.','In the final chapter, it is announced only briefly that the proceedings were discontinued.','در فصل پایانی فقط کوتاه اعلام می‌شود که روند رسیدگی متوقف شده است.','Di beşa dawî de tenê bi kurtî tê ragihandin ku pêvajo rawestiya.','W ostatnim rozdziale jedynie krótko ogłasza się, że postępowanie umorzono.','În ultimul capitol se anunță doar pe scurt că procedura a fost închisă.','В последней главе лишь кратко сообщается, что процесс прекращен.','Në kapitullin e fundit njoftohet vetëm shkurt se procesi u ndërpre.','Son bölümde sürecin durdurulduğu yalnızca kısaca duyurulur.'))
    ]
  }),
  entry({
    word: 'verleumden', partOfSpeech: 'Verb', infinitive: 'verleumden', syllableBreak: 'ver-leum-den',
    topics: ['law-and-compliance','social-and-relationships','business-communication'], usageLabels: ['formal','written','sensitive','advanced'],
    collocations: [{ text: 'jemanden öffentlich verleumden', meaning: 'to publicly defame someone' }],
    meanings: m('يفتري؛ يشهّر كذباً','بوختانکردن؛ بە درۆ ناوبدکردن','to defame; to slander','تهمت زدن؛ افترا بستن','iftira kirin; bi derew xerabkirin','zniesławiać; oczerniać','a defăima; a calomnia','клеветать','shpif; përgojoj rrejshëm','iftira atmak; karalamak'),
    examples: [
      ex('Der Anbieter drohte mit rechtlichen Schritten, weil ein Wettbewerber ihn in sozialen Medien verleumdet habe.', m('هدد المورّد بإجراءات قانونية لأن منافساً افتراه في وسائل التواصل الاجتماعي.','دابینکەرەکە هەڕەشەی هەنگاوی یاسایی کرد، چونکە کێبەرکێکارێک لە تۆڕە کۆمەڵایەتییەکان بوختانی کردبوو.','The provider threatened legal action because a competitor had defamed it on social media.','ارائه‌دهنده تهدید به اقدام حقوقی کرد، چون رقیبی در شبکه‌های اجتماعی به او افترا زده بود.','Dabînker tehdîda gavên yasayî kir, ji ber ku hevrikek li medyaya civakî ew iftira kiribû.','Dostawca zagroził krokami prawnymi, ponieważ konkurent zniesławił go w mediach społecznościowych.','Furnizorul a amenințat cu acțiuni legale deoarece un competitor îl defăimase pe rețelele sociale.','Поставщик пригрозил юридическими мерами, потому что конкурент оклеветал его в социальных сетях.','Ofruesi kërcënoi me hapa ligjorë, sepse një konkurrent e kishte shpifur në rrjetet sociale.','Sağlayıcı, bir rakibin sosyal medyada kendisine iftira attığı gerekçesiyle hukuki adım tehdidinde bulundu.')),
      ex('Die Szene zeigt, wie leicht eine Dorfgemeinschaft jemanden verleumdet, wenn Angst wichtiger wird als Beweise.', m('تُظهر المشهد مدى سهولة أن يفتري مجتمع القرية على شخص عندما يصبح الخوف أهم من الأدلة.','دیمەنەکە پیشان دەدات کۆمەڵگای گوند چەند بە ئاسانی بوختان لە کەسێک دەکات کاتێک ترس گرنگتر دەبێت لە بەڵگە.','The scene shows how easily a village community slanders someone when fear becomes more important than evidence.','صحنه نشان می‌دهد یک جامعه روستایی وقتی ترس از شواهد مهم‌تر می‌شود، چقدر راحت به کسی افترا می‌زند.','Dîmen nîşan dide civata gund çiqas hêsan kesek iftira dike dema tirs ji belgeyan girîngtir dibe.','Scena pokazuje, jak łatwo społeczność wiejska oczernia kogoś, gdy strach staje się ważniejszy niż dowody.','Scena arată cât de ușor o comunitate rurală calomniază pe cineva când frica devine mai importantă decât dovezile.','Сцена показывает, как легко деревенское сообщество клевещет на человека, когда страх становится важнее доказательств.','Skena tregon sa lehtë një komunitet fshati shpif për dikë kur frika bëhet më e rëndësishme se provat.','Sahne, korku kanıtlardan daha önemli hale geldiğinde bir köy topluluğunun birine ne kadar kolay iftira attığını gösterir.'))
    ]
  }),
  entry({
    word: 'das Vermächtnis', partOfSpeech: 'Noun', article: 'das', plural: 'Vermächtnisse', syllableBreak: 'Ver-mächt-nis',
    topics: ['law-and-compliance','culture-and-media','management-and-leadership'], usageLabels: ['formal','written','advanced','analysis'],
    collocations: [{ text: 'ein bleibendes Vermächtnis', meaning: 'a lasting legacy' }],
    meanings: m('إرث؛ وصية؛ تركة معنوية','میرات؛ وەسیەت؛ جێماوەی مانادار','legacy; bequest','میراث؛ وصیت؛ یادگار ماندگار','mîrat; wesiyet; mayînekî mayînde','dziedzictwo; zapis','moștenire; legat','наследие; завещание','trashëgimi; legat','miras; vasiyet'),
    examples: [
      ex('Das wichtigste Vermächtnis der Gründerin war nicht der Code, sondern die Kultur offener technischer Debatten.', m('لم يكن أهم إرث للمؤسسة هو الكود، بل ثقافة النقاشات التقنية المفتوحة.','گرنگترین میراتی دامەزرێنەرەکە کۆد نەبوو، بەڵکو کەلتووری گفتوگۆی تەکنیکی کراوە بوو.','The founder’s most important legacy was not the code, but the culture of open technical debate.','مهم‌ترین میراث بنیان‌گذار کد نبود، بلکه فرهنگ بحث فنی باز بود.','Mîrata herî girîng a damezrînerê ne kod bû, lê çanda nîqaşên teknîkî yên vekirî bû.','Najważniejszym dziedzictwem założycielki nie był kod, lecz kultura otwartych debat technicznych.','Cea mai importantă moștenire a fondatoarei nu a fost codul, ci cultura dezbaterilor tehnice deschise.','Самым важным наследием основательницы был не код, а культура открытых технических дискуссий.','Trashëgimia më e rëndësishme e themelueses nuk ishte kodi, por kultura e debateve të hapura teknike.','Kurucunun en önemli mirası kod değil, açık teknik tartışma kültürüydü.')),
      ex('Das Vermächtnis des Vaters erscheint im Roman weniger als Besitz denn als ungelöste Verpflichtung.', m('يظهر إرث الأب في الرواية أقل كملكية وأكثر كالتزام غير محلول.','میراتی باوک لە ڕۆمانەکەدا کەمتر وەک موڵک، زیاتر وەک پابەندییەکی چارەسەرنەکراو دەردەکەوێت.','The father’s legacy appears in the novel less as property than as an unresolved obligation.','میراث پدر در رمان کمتر به شکل دارایی و بیشتر به‌صورت تعهدی حل‌نشده ظاهر می‌شود.','Mîrata bavê di romanê de kêmtir wek milk, zêdetir wek pabendiyeke neçareserkirî xuya dike.','Dziedzictwo ojca jawi się w powieści mniej jako majątek, a bardziej jako nierozwiązane zobowiązanie.','Moștenirea tatălui apare în roman mai puțin ca proprietate și mai mult ca obligație nerezolvată.','Наследие отца в романе предстает не столько как имущество, сколько как нерешенное обязательство.','Trashëgimia e babait në roman shfaqet më pak si pronë dhe më shumë si detyrim i pazgjidhur.','Babanın mirası romanda mülkten çok çözülmemiş bir yükümlülük olarak görünür.'))
    ]
  })
];
const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId: `de-${levelLower}-generated-batch-${batch}`, packageName: 'German C2 Generated Batch 087', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

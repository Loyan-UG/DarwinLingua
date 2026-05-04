const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const taxonomyPath = path.join(root, 'content/taxonomy/darwinlingua-taxonomy-v1.json');
const sourcePath = path.join(root, 'content/C1.txt');
const outPath = path.join(root, 'content/generated/de-c1-generated-batch-004.json');
const packageId = 'de-c1-generated-batch-004';
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const sourceText = fs.readFileSync(sourcePath, 'utf8');
const tokens = sourceText.split(',').map(s => s.trim()).filter(Boolean);
const words = tokens.slice(0, 6);
const expected = ['abstrahieren von','die Abstraktion','die Abstraktionsebene','abstreichen','abtragen','abtrinken'];
if (words.length !== 6 || expected.some((w,i) => words[i] !== w)) throw new Error('Unexpected first tokens: ' + JSON.stringify(words));
function labelObj(key){ const x=(taxonomy.labels||[]).find(l=>l.key===key); if(!x) throw new Error('Missing label '+key); return x; }
function meaning(ar,ckb,en,fa,kmr,pl,ro,ru,sq,tr){ return [ar,ckb,en,fa,kmr,pl,ro,ru,sq,tr].map((text,i)=>({language:langs[i],text})); }
function ex(baseText,ar,ckb,en,fa,kmr,pl,ro,ru,sq,tr){ return {baseText, translations: meaning(ar,ckb,en,fa,kmr,pl,ro,ru,sq,tr)}; }
function entry(e){ return Object.assign({language:'de', cefrLevel:'C1', pronunciationIpa:null, contextLabels:[], relations:[]}, e); }

const entries = [
entry({
  word:'abstrahieren von', partOfSpeech:'Verb', article:null, plural:null, infinitive:'abstrahieren von', syllableBreak:'abs-tra-hie-ren von',
  topics:['advanced-analysis','technology-and-it','data-and-reporting'], usageLabels:['academic','technical','analysis','advanced'],
  grammarNotes:['regular verb phrase; used with von plus dative'],
  collocations:[{text:'von Einzelfällen abstrahieren', meaning:'to abstract from individual cases'},{text:'von technischen Details abstrahieren', meaning:'to abstract away technical details'}],
  wordFamilies:[{lemma:'abstrahieren', relationLabel:'verb', note:null},{lemma:'die Abstraktion', relationLabel:'noun', note:null}],
  meanings:meaning('يتجرد من؛ يتجاهل التفاصيل المحددة','لە شتە تایبەتەکان ئەبستراکت دەکات؛ وردەکارییەکان لا دەبات','to abstract from; to disregard specific details','از چیزی انتزاع کردن؛ جزئیات خاص را کنار گذاشتن','ji tiştên taybet abstrakte kirin; hûrguliyan paşguh kirin','abstrahować od; pomijać szczegóły','a face abstracție de; a lăsa deoparte detaliile','абстрагироваться от; отвлекаться от деталей','të abstragosh nga; të lësh mënjanë hollësitë','bir şeyden soyutlamak; belirli ayrıntıları dışarıda bırakmak'),
  examples:[
    ex('In der Präsentation sollten wir von internen Sonderfällen abstrahieren und den Kunden nur den Standardprozess zeigen.','في العرض التقديمي ينبغي أن نتجرد من الحالات الداخلية الخاصة ونُظهر للعملاء العملية القياسية فقط.','لە پێشکەشکردنەکەدا پێویستە لە دۆخە تایبەتییە ناوخۆییەکان ئەبستراکت بکەین و تەنها پرۆسەی ستاندارد پیشانی کڕیار بدەین.','In the presentation, we should abstract from internal special cases and show customers only the standard process.','در ارائه باید از موارد خاص داخلی صرف‌نظر کنیم و فقط فرایند استاندارد را به مشتریان نشان دهیم.','Di pêşkêşiyê de divê em ji rewşên taybet ên hundirîn abstrakte bikin û tenê pêvajoya standard nîşanî xerîdaran bidin.','W prezentacji powinniśmy abstrahować od wewnętrznych przypadków szczególnych i pokazać klientom tylko standardowy proces.','În prezentare ar trebui să facem abstracție de cazurile speciale interne și să le arătăm clienților doar procesul standard.','В презентации нам следует абстрагироваться от внутренних особых случаев и показать клиентам только стандартный процесс.','Në prezantim duhet të abstrahojmë nga rastet e veçanta të brendshme dhe t’u tregojmë klientëve vetëm procesin standard.','Sunumda şirket içi özel durumlardan soyutlamalı ve müşterilere yalnızca standart süreci göstermeliyiz.'),
    ex('Das Modell abstrahiert von saisonalen Schwankungen, damit langfristige Trends sichtbar werden.','يتجرد النموذج من التقلبات الموسمية لكي تصبح الاتجاهات طويلة المدى واضحة.','مۆدێلەکە لە گۆڕانکارییە وەرزییەکان ئەبستراکت دەکات بۆ ئەوەی ئاراستە درێژخایەنەکان دیار بن.','The model abstracts from seasonal fluctuations so that long-term trends become visible.','مدل از نوسان‌های فصلی صرف‌نظر می‌کند تا روندهای بلندمدت دیده شوند.','Model ji guherînên demsalî abstrakte dike da ku meylên demdirêj xuya bibin.','Model abstrahuje od wahań sezonowych, aby widoczne były trendy długoterminowe.','Modelul face abstracție de fluctuațiile sezoniere, astfel încât tendințele pe termen lung să devină vizibile.','Модель абстрагируется от сезонных колебаний, чтобы стали видны долгосрочные тенденции.','Modeli abstrahon nga luhatjet sezonale që të duken prirjet afatgjata.','Model, uzun vadeli eğilimlerin görünür olması için mevsimsel dalgalanmalardan soyutlanır.')
  ]
}),
entry({
  word:'die Abstraktion', partOfSpeech:'Noun', article:'die', plural:'Abstraktionen', infinitive:null, syllableBreak:'Abs-trak-tion',
  topics:['advanced-analysis','technology-and-it','education-and-training'], usageLabels:['academic','technical','analysis','advanced'],
  grammarNotes:['feminine noun'], collocations:[{text:'eine sinnvolle Abstraktion wählen', meaning:'to choose a useful abstraction'},{text:'auf hoher Abstraktionsebene', meaning:'at a high level of abstraction'}],
  wordFamilies:[{lemma:'abstrahieren', relationLabel:'verb', note:null},{lemma:'abstrakt', relationLabel:'adjective', note:null}],
  meanings:meaning('تجريد؛ مفهوم مجرد','ئەبستراکشن؛ چەمکی گشتی','abstraction; abstract concept','انتزاع؛ مفهوم انتزاعی','abstraksiyon; têgeha razber','abstrakcja; pojęcie abstrakcyjne','abstracție; concept abstract','абстракция; отвлеченное понятие','abstraksion; koncept abstrakt','soyutlama; soyut kavram'),
  examples:[
    ex('Eine gute Abstraktion macht die Software wartbar, ohne wichtige Fachregeln zu verstecken.','تجريد جيد يجعل البرنامج قابلاً للصيانة من دون إخفاء قواعد العمل المهمة.','ئەبستراکشنێکی باش نەرمەکاڵاکە ئاسانتر بۆ چاککردنەوە دەکات، بەبێ ئەوەی یاسا گرنگەکانی بواری کار بشارێتەوە.','A good abstraction makes the software maintainable without hiding important business rules.','یک انتزاع خوب نرم‌افزار را قابل نگهداری می‌کند، بدون اینکه قواعد مهم کسب‌وکار را پنهان کند.','Abstraksiyonek baş nermalavê hêsan ji bo lênêrînê dike bê ku qaîdeyên girîng ên karî veşêre.','Dobra abstrakcja ułatwia utrzymanie oprogramowania, nie ukrywając ważnych reguł biznesowych.','O abstracție bună face software-ul ușor de întreținut fără să ascundă reguli importante de business.','Хорошая абстракция делает ПО сопровождаемым, не скрывая важных бизнес-правил.','Një abstraksion i mirë e bën softuerin të mirëmbajtshëm pa fshehur rregulla të rëndësishme biznesi.','İyi bir soyutlama, önemli iş kurallarını gizlemeden yazılımı bakımı kolay hale getirir.'),
    ex('In der Diskussion blieb die Abstraktion zu hoch, deshalb fehlten konkrete Entscheidungen.','بقي مستوى التجريد في النقاش عاليًا جدًا، لذلك غابت القرارات الملموسة.','لە گفتوگۆکەدا ئەبستراکشنەکە زۆر بەرز مایەوە، بۆیە بڕیاری کۆنکرێت نەبوو.','In the discussion, the abstraction remained too high, so concrete decisions were missing.','در بحث، سطح انتزاع خیلی بالا ماند، بنابراین تصمیم‌های مشخص گرفته نشد.','Di gotûbêjê de asta abstraksiyonê pir bilind ma, ji ber wê biryarên zeq tune bûn.','W dyskusji poziom abstrakcji pozostał zbyt wysoki, dlatego zabrakło konkretnych decyzji.','În discuție, nivelul de abstracție a rămas prea ridicat, de aceea au lipsit deciziile concrete.','В обсуждении уровень абстракции остался слишком высоким, поэтому не хватило конкретных решений.','Në diskutim, abstraksioni mbeti shumë i lartë, ndaj munguan vendimet konkrete.','Tartışmada soyutlama seviyesi çok yüksek kaldı, bu yüzden somut kararlar eksik kaldı.')
  ]
}),
entry({
  word:'die Abstraktionsebene', partOfSpeech:'Noun', article:'die', plural:'Abstraktionsebenen', infinitive:null, syllableBreak:'Abs-trak-ti-ons-e-be-ne',
  topics:['advanced-analysis','technology-and-it','meetings-and-presentations'], usageLabels:['academic','technical','analysis','advanced'],
  grammarNotes:['feminine compound noun'], collocations:[{text:'die passende Abstraktionsebene finden', meaning:'to find the appropriate level of abstraction'},{text:'auf derselben Abstraktionsebene bleiben', meaning:'to stay on the same abstraction level'}],
  wordFamilies:[{lemma:'die Abstraktion', relationLabel:'noun', note:null},{lemma:'die Ebene', relationLabel:'noun', note:null}],
  meanings:meaning('مستوى التجريد','ئاستی ئەبستراکشن','level of abstraction','سطح انتزاع','asta abstraksiyonê','poziom abstrakcji','nivel de abstracție','уровень абстракции','nivel abstraksioni','soyutlama düzeyi'),
  examples:[
    ex('Im Architekturreview wechselten wir ständig die Abstraktionsebene, was die Bewertung erschwerte.','في مراجعة البنية كنا نغيّر مستوى التجريد باستمرار، وهذا صعّب التقييم.','لە پێداچوونەوەی ئەندازیارییەکەدا بەردەوام ئاستی ئەبستراکشنمان دەگۆڕی، ئەمەش هەڵسەنگاندنەکەی قورس کرد.','In the architecture review, we kept changing the level of abstraction, which made the assessment harder.','در بازبینی معماری مدام سطح انتزاع را عوض می‌کردیم و همین ارزیابی را سخت‌تر کرد.','Di nirxandina avahîsaziyê de me her tim asta abstraksiyonê diguherand, ev jî nirxandinê zehmettir kir.','Podczas przeglądu architektury ciągle zmienialiśmy poziom abstrakcji, co utrudniało ocenę.','În analiza arhitecturii schimbam mereu nivelul de abstracție, ceea ce a îngreunat evaluarea.','На архитектурном ревью мы постоянно меняли уровень абстракции, что затрудняло оценку.','Në shqyrtimin e arkitekturës e ndryshonim vazhdimisht nivelin e abstraksionit, gjë që e vështirësoi vlerësimin.','Mimari incelemesinde soyutlama düzeyini sürekli değiştirdik, bu da değerlendirmeyi zorlaştırdı.'),
    ex('Für den Vorstand brauchen wir eine andere Abstraktionsebene als für das Entwicklungsteam.','بالنسبة إلى مجلس الإدارة نحتاج إلى مستوى تجريد مختلف عن مستوى فريق التطوير.','بۆ دەستەی بەڕێوەبردن پێویستمان بە ئاستێکی جیاوازی ئەبستراکشن هەیە وەک بۆ تیمی پەرەپێدان نییە.','For the board, we need a different level of abstraction than for the development team.','برای هیئت‌مدیره به سطح انتزاعی متفاوت از تیم توسعه نیاز داریم.','Ji bo desteya rêveberiyê, ji tîmê pêşvebirinê cuda, asta abstraksiyonê ya din hewce ye.','Dla zarządu potrzebujemy innego poziomu abstrakcji niż dla zespołu deweloperskiego.','Pentru consiliul de administrație avem nevoie de un alt nivel de abstracție decât pentru echipa de dezvoltare.','Для правления нужен другой уровень абстракции, чем для команды разработки.','Për bordin na duhet një nivel tjetër abstraksioni sesa për ekipin e zhvillimit.','Yönetim kurulu için geliştirme ekibine göre farklı bir soyutlama düzeyine ihtiyacımız var.')
  ]
}),
entry({
  word:'abstreichen', partOfSpeech:'Verb', article:null, plural:null, infinitive:'abstreichen', syllableBreak:'ab-strei-chen',
  topics:['production-and-maintenance','quality-and-risk','everyday-life'], usageLabels:['technical','written','advanced'],
  grammarNotes:['strong separable verb'], collocations:[{text:'überschüssiges Material abstreichen', meaning:'to scrape off excess material'},{text:'eine Kante sauber abstreichen', meaning:'to wipe or level an edge cleanly'}],
  wordFamilies:[{lemma:'streichen', relationLabel:'verb', note:null}],
  meanings:meaning('يمسح أو يكشط الزائد','دەسڕێتەوە یان ماددەی زیادە لادەبات','to wipe off; to scrape off; to level off','پاک کردن؛ تراشیدن؛ صاف کردن سطح','paqij kirin; hêşandin; rast kirin','zetrzeć; zgarnąć; wyrównać','a șterge; a răzui; a nivela','счищать; стирать; выравнивать','të fshish; të kruash; të nivelosh','silmek; sıyırmak; düzlemek'),
  examples:[
    ex('Vor dem Lackieren strich der Techniker den Staub sorgfältig von der Metallkante ab.','قبل الطلاء مسح الفني الغبار بعناية عن حافة المعدن.','پێش بۆیاخکردن، تەکنیکەرەکە بە وریایی تۆزی لەسەر قەراخی مەعدەنەکە سڕییەوە.','Before painting, the technician carefully wiped the dust off the metal edge.','قبل از رنگ‌کاری، تکنسین گردوغبار را با دقت از لبه فلزی پاک کرد.','Berî boyaxkirinê, teknîsyen bi baldarî toz ji qiraxa metalê paqij kir.','Przed lakierowaniem technik starannie starł kurz z metalowej krawędzi.','Înainte de vopsire, tehnicianul a șters cu grijă praful de pe marginea metalică.','Перед покраской техник тщательно стер пыль с металлической кромки.','Para lyerjes, tekniku e fshiu me kujdes pluhurin nga buza metalike.','Boyamadan önce teknisyen metal kenardaki tozu dikkatlice sildi.'),
    ex('Beim Backen streicht man überschüssigen Teig am Rand der Schüssel ab.','عند الخَبز يمسح المرء العجين الزائد عن حافة الوعاء.','لە کاتی نانکردندا ماددەی هەویرە زیادەکە لە قەراخی قاپەکە دەسڕدرێتەوە.','When baking, you scrape excess dough off the edge of the bowl.','هنگام پخت، خمیر اضافی را از لبه کاسه پاک می‌کنند.','Dema nanpêjînê, hevîra zêde ji qiraxa tasê tê paqijkirin.','Podczas pieczenia zgarnia się nadmiar ciasta z brzegu miski.','La copt, se îndepărtează aluatul în exces de pe marginea bolului.','При выпечке лишнее тесто счищают с края миски.','Gjatë pjekjes, brumi i tepërt fshihet nga buza e tasit.','Pişirme sırasında fazla hamur kasenin kenarından sıyrılır.')
  ]
}),
entry({
  word:'abtragen', partOfSpeech:'Verb', article:null, plural:null, infinitive:'abtragen', syllableBreak:'ab-tra-gen',
  topics:['finance-and-accounting','production-and-maintenance','housing-and-real-estate'], usageLabels:['business','technical','written','advanced'],
  grammarNotes:['strong separable verb'], collocations:[{text:'Schulden abtragen', meaning:'to pay down debts'},{text:'Material abtragen', meaning:'to remove material layer by layer'}],
  wordFamilies:[{lemma:'tragen', relationLabel:'verb', note:null},{lemma:'die Abtragung', relationLabel:'noun', note:null}],
  meanings:meaning('يسدد تدريجيًا؛ يزيل طبقةً طبقة','بەهێواشی قەرز دەداتەوە؛ چین بە چین لادەبات','to pay off gradually; to remove layer by layer','تدریجی پرداخت کردن؛ لایه‌لایه برداشتن','hêdî hêdî dayîn; qat bi qat rakirin','spłacać; usuwać warstwami','a achita treptat; a îndepărta strat cu strat','постепенно погашать; снимать слой за слоем','të shlyesh gradualisht; të heqësh shtresë pas shtrese','kademeli ödemek; katman katman kaldırmak'),
  examples:[
    ex('Das Unternehmen will die alten Verbindlichkeiten über drei Jahre abtragen.','تريد الشركة تسديد الالتزامات القديمة على مدى ثلاث سنوات.','کۆمپانیاکە دەیەوێت پابەندییە داراییە کۆنەکان بە ماوەی سێ ساڵ بداتەوە.','The company wants to pay down the old liabilities over three years.','شرکت می‌خواهد بدهی‌های قدیمی را طی سه سال تدریجاً پرداخت کند.','Şirket dixwaze deynên kevn di sê salan de hêdî hêdî bide.','Firma chce spłacić stare zobowiązania w ciągu trzech lat.','Compania vrea să achite vechile obligații pe parcursul a trei ani.','Компания хочет погасить старые обязательства в течение трех лет.','Kompania dëshiron t’i shlyejë detyrimet e vjetra gjatë tre viteve.','Şirket eski yükümlülükleri üç yıl içinde kademeli olarak kapatmak istiyor.'),
    ex('Bei der Sanierung wurde der beschädigte Putz Schicht für Schicht abgetragen.','أثناء الترميم أُزيل الجص المتضرر طبقةً بعد طبقة.','لە کاتی نۆژەنکردنەوەدا پڵاستەری تێکچوو چین بە چین لابرا.','During the renovation, the damaged plaster was removed layer by layer.','در بازسازی، گچ آسیب‌دیده لایه به لایه برداشته شد.','Di nûvekirinê de, maltera xerabûyî qat bi qat hate rakirin.','Podczas remontu uszkodzony tynk usuwano warstwa po warstwie.','În timpul renovării, tencuiala deteriorată a fost îndepărtată strat cu strat.','При ремонте поврежденную штукатурку снимали слой за слоем.','Gjatë rinovimit, suvaja e dëmtuar u hoq shtresë pas shtrese.','Tadilat sırasında hasarlı sıva katman katman kaldırıldı.')
  ]
}),
entry({
  word:'abtrinken', partOfSpeech:'Verb', article:null, plural:null, infinitive:'abtrinken', syllableBreak:'ab-trin-ken',
  topics:['everyday-life','culture-and-media','shopping-and-services'], usageLabels:['spoken','advanced'],
  grammarNotes:['strong separable verb; comparatively rare, often literal'], collocations:[{text:'den Schaum abtrinken', meaning:'to drink off the foam'},{text:'einen Schluck abtrinken', meaning:'to drink a small amount from the top'}],
  wordFamilies:[{lemma:'trinken', relationLabel:'verb', note:null}],
  meanings:meaning('يشرب الجزء العلوي أو كمية صغيرة من شيء','سەرەوەی شتێک یان کەمێک لێ دەخواتەوە','to drink off; to drink a small amount from the top','از روی نوشیدنی یا کمی از آن نوشیدن','ji serê vexwarinê vexwarin; hinek vexwarin','upić z wierzchu; odpić trochę','a bea de la suprafață; a sorbi puțin','отпить сверху; немного отпить','të pish pak nga sipër; të gjerbësh pak','üstünden içmek; biraz yudumlamak'),
  examples:[
    ex('Er trank zuerst den Schaum vom Cappuccino ab, bevor er weiterarbeitete.','شرب أولًا رغوة الكابتشينو قبل أن يواصل العمل.','سەرەتا کفەکەی کاپۆچینۆکەی خواردەوە، پاشان درێژەی بە کارەکەی دا.','He first drank the foam off the cappuccino before continuing his work.','او اول کف کاپوچینو را خورد و بعد به کارش ادامه داد.','Wî pêşî kefê cappuccino vexwar, paşê karê xwe domand.','Najpierw wypił piankę z cappuccino, zanim wrócił do pracy.','Mai întâi a băut spuma de pe cappuccino, înainte să continue lucrul.','Сначала он выпил пену с капучино, прежде чем продолжить работу.','Ai fillimisht piu shkumën e kapuçinos, para se të vazhdonte punën.','Çalışmaya devam etmeden önce kapuçinonun köpüğünü içti.'),
    ex('Im Restaurant bat die Kundin darum, das Glas nicht ganz voll zu machen, weil sie unterwegs noch etwas abtrinken wollte.','في المطعم طلبت الزبونة ألا يُملأ الكأس تمامًا، لأنها أرادت أن تشرب منه قليلًا في الطريق.','لە چێشتخانەکەدا کڕیارەکە داوای کرد پەرداخەکە تەواو پڕ نەکرێت، چونکە دەیویست لە ڕێگادا کەمێکی لێ بخواتەوە.','In the restaurant, the customer asked them not to fill the glass completely because she wanted to sip some on the way.','در رستوران مشتری خواست لیوان را کاملاً پر نکنند، چون می‌خواست در راه کمی از آن بنوشد.','Di xwaringehê de xerîdarê jin xwest ku qedeh bi tevahî tijî nekin, ji ber ku wê dixwest di rê de hinek jê vexwe.','W restauracji klientka poprosiła, aby nie napełniać szklanki do końca, bo chciała jeszcze trochę upić po drodze.','La restaurant, clienta a cerut ca paharul să nu fie umplut complet, deoarece voia să mai soarbă puțin pe drum.','В ресторане клиентка попросила не наливать стакан до краев, потому что хотела еще немного отпить по дороге.','Në restorant, klientja kërkoi që gota të mos mbushej plot, sepse donte të pinte pak gjatë rrugës.','Restoranda müşteri, yolda biraz içmek istediği için bardağın tamamen doldurulmamasını istedi.')
  ]
})
];

const usedLabels = [...new Set(entries.flatMap(e => [...(e.usageLabels||[]), ...(e.contextLabels||[])]))];
const pkg = {packageVersion:'1.0', packageId, packageName:'German C1 Generated Batch 004', source:'Hybrid', defaultMeaningLanguages:langs, labels:usedLabels.map(labelObj), entries, collections:[], scenarios:[], conversationStarterPacks:[], eventPreparationPacks:[]};
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');
const args = ['run','--project',path.join(root,'src/Apps/DarwinLingua.ImportTool/DarwinLingua.ImportTool.csproj'),'--','--target','shared','--yes',outPath];
let output='';
try { output = cp.execFileSync('dotnet', args, {cwd:root, encoding:'utf8', stdio:['ignore','pipe','pipe']}); }
catch(e) { output=(e.stdout||'')+(e.stderr||''); fs.writeFileSync(path.join(root,'content/generated/de-c1-import-failures.txt'), words.join(', ')+'\tbatch-004\timport command failed\n', {flag:'a', encoding:'utf8'}); console.log(JSON.stringify({sourcePath,words,outPath,importOutput:output,deleted:false,remainingCount:tokens.length,first10Remaining:tokens.slice(0,10)},null,2)); process.exit(1); }
const ok = /Entries imported:\s*6\b/.test(output) && /Entries invalid:\s*0\b/.test(output) && /Warnings:\s*0\b/.test(output);
let deleted=false, remaining=tokens;
if (ok) {
  const counts = Object.fromEntries(words.map(w=>[w,1]));
  remaining=[];
  for (const t of tokens) { if (Object.prototype.hasOwnProperty.call(counts,t) && counts[t] > 0) counts[t]--; else remaining.push(t); }
  fs.writeFileSync(sourcePath, remaining.join(', '), 'utf8');
  deleted=true;
} else {
  fs.writeFileSync(path.join(root,'content/generated/de-c1-import-failures.txt'), words.join(', ')+'\tbatch-004\t'+output.replace(/\s+/g,' ').trim()+'\n', {flag:'a', encoding:'utf8'});
}
console.log(JSON.stringify({sourcePath,words,outPath,importOutput:output,deleted,remainingCount:remaining.length,first10Remaining:remaining.slice(0,10)},null,2));

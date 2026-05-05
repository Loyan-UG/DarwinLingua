const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '068';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['die Redundanz','die Reflexivität','rekapitulieren','die Reminiszenz','das Ressentiment','reüssieren'];

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
    word: 'die Redundanz', partOfSpeech: 'Noun', article: 'die', plural: 'Redundanzen', syllableBreak: 'Re-dun-danz',
    topics: ['technology-and-it','quality-and-risk','data-and-reporting'], usageLabels: ['formal','written','analysis','business'],
    collocations: [{ text: 'technische Redundanz schaffen', meaning: 'to create technical redundancy' }],
    meanings: meaning('تكرار احتياطي؛ فائض وظيفي','دووبارەیی؛ زیادەی پاراستن','redundancy; functional duplication','افزونگی؛ تکرار پشتیبان','zêdebûn; dubarekirina piştgir','redundancja; nadmiarowość','redundanță; dublare funcțională','избыточность; резервирование','tepricë; redundancë','yedeklilik; fazlalık'),
    examples: [
      ex('Die zusätzliche Redundanz im Zahlungssystem verteuert den Betrieb, senkt aber das Risiko eines Totalausfalls erheblich.', trans('تزيد الازدواجية الاحتياطية الإضافية في نظام الدفع تكلفة التشغيل، لكنها تخفض كثيراً خطر الانقطاع الكامل.','دووبارەیی زیادە لە سیستەمی پارەداندا تێچووی کارکردن زیاد دەکات، بەڵام مەترسیی وەستانی تەواو زۆر کەم دەکاتەوە.','The additional redundancy in the payment system makes operations more expensive, but it significantly lowers the risk of a total outage.','افزونگی اضافی در سیستم پرداخت هزینه بهره‌برداری را بالا می‌برد، اما خطر قطعی کامل را به‌طور چشمگیری کاهش می‌دهد.','Redundansa zêde di pergala pere-danê de xebatê biha dike, lê xetera qutbûna tevahî gelek kêm dike.','Dodatkowa redundancja w systemie płatności podnosi koszty działania, ale znacząco zmniejsza ryzyko całkowitej awarii.','Redundanța suplimentară din sistemul de plăți scumpește operarea, dar reduce considerabil riscul unei căderi totale.','Дополнительная избыточность в платежной системе удорожает эксплуатацию, но существенно снижает риск полного отказа.','Redundanca shtesë në sistemin e pagesave e rrit koston e operimit, por ul ndjeshëm rrezikun e një ndërprerjeje totale.','Ödeme sistemindeki ek yedeklilik işletme maliyetini artırır, ancak tam kesinti riskini ciddi ölçüde azaltır.')),
      ex('Im Text ist die Redundanz nicht bloß ein Fehler, sondern spiegelt die kreisende Erinnerung der Erzählerin.', trans('في النص لا يكون التكرار مجرد خطأ، بل يعكس ذاكرة الراوية الدائرية.','لە دەقەکەدا دووبارەیی تەنها هەڵە نییە، بەڵکو بیرەوەریی سووڕاوەی گێڕەرەوەکە ڕەنگدەداتەوە.','In the text, the redundancy is not merely a flaw; it reflects the narrator’s circling memory.','در متن، تکرار فقط خطا نیست، بلکه حافظه چرخان راوی را بازتاب می‌دهد.','Di nivîsê de redundans ne tenê çewtî ye, lê bîranîna gerok a vegêrê vedide.','W tekście redundancja nie jest jedynie błędem, lecz odzwierciedla krążącą pamięć narratorki.','În text, redundanța nu este doar o greșeală, ci reflectă memoria circulară a naratoarei.','В тексте избыточность не просто ошибка, а отражение кружащей памяти рассказчицы.','Në tekst, redundanca nuk është thjesht gabim, por pasqyron kujtesën rrotulluese të rrëfimtares.','Metindeki tekrar yalnızca bir hata değil, anlatıcının döngüsel hafızasını yansıtır.'))
    ]
  }),
  entry({
    word: 'die Reflexivität', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Re-fle-xi-vi-tät',
    topics: ['advanced-analysis','education-and-training','management-and-leadership'], usageLabels: ['formal','written','academic','analysis'],
    grammarNotes: ['feminine abstract noun; normally used in singular'],
    collocations: [{ text: 'methodische Reflexivität', meaning: 'methodological reflexivity' }],
    meanings: meaning('انعكاسية؛ وعي ذاتي بالمنهج أو الفعل','خۆڕەنگدانەوە؛ ئاگاداریی خۆیی لە شێواز','reflexivity; self-awareness of method or action','بازتابندگی؛ خودآگاهی روش‌مند','refleksîvîte; hişyariya xwe ya rêbazî','refleksyjność','reflexivitate','рефлексивность','refleksivitet','refleksivite; özdüşünümsellik'),
    examples: [
      ex('Gute Organisationsforschung braucht Reflexivität, weil die Beobachtung selbst das Verhalten der Beteiligten verändern kann.', trans('يحتاج البحث التنظيمي الجيد إلى انعكاسية لأن الملاحظة نفسها قد تغيّر سلوك المشاركين.','توێژینەوەی باشی ڕێکخراوە پێویستی بە خۆڕەنگدانەوە هەیە، چونکە چاودێری خۆی دەتوانێت ڕەفتاری بەشداربووان بگۆڕێت.','Good organizational research needs reflexivity because observation itself can change the behavior of those involved.','پژوهش سازمانی خوب به بازتابندگی نیاز دارد، چون خود مشاهده می‌تواند رفتار مشارکت‌کنندگان را تغییر دهد.','Lêkolîna baş a rêxistinî pêwîstî bi refleksîvîteyê heye, ji ber ku çavdêrî bixwe dikare reftara beşdaran biguherîne.','Dobre badania organizacji wymagają refleksyjności, ponieważ sama obserwacja może zmieniać zachowanie uczestników.','O cercetare organizațională bună are nevoie de reflexivitate, deoarece observația însăși poate schimba comportamentul participanților.','Хорошее организационное исследование требует рефлексивности, потому что само наблюдение может менять поведение участников.','Kërkimi i mirë organizativ kërkon refleksivitet, sepse vetë vëzhgimi mund të ndryshojë sjelljen e pjesëmarrësve.','İyi örgüt araştırması refleksivite gerektirir, çünkü gözlemin kendisi katılımcıların davranışını değiştirebilir.')),
      ex('Die Reflexivität des Romans zeigt sich darin, dass er seine eigenen Erzählbedingungen immer wieder sichtbar macht.', trans('تظهر انعكاسية الرواية في أنها تجعل شروط سردها الخاصة مرئية مراراً.','خۆڕەنگدانەوەی ڕۆمانەکە لەوەدا دەردەکەوێت کە مەرجەکانی گێڕانەوەی خۆی جار بە جار دیار دەکات.','The novel’s reflexivity appears in the way it repeatedly makes its own narrative conditions visible.','بازتابندگی رمان در این است که شرایط روایت خود را بارها آشکار می‌کند.','Refleksîvîteya romanê di wê de xuya dibe ku mercên vegotina xwe her car xuya dike.','Refleksyjność powieści ujawnia się w tym, że wielokrotnie uwidacznia ona własne warunki narracji.','Reflexivitatea romanului se vede în faptul că își face mereu vizibile propriile condiții narative.','Рефлексивность романа проявляется в том, что он снова и снова делает видимыми собственные условия повествования.','Refleksiviteti i romanit shfaqet në faktin se ai i bën vazhdimisht të dukshme kushtet e veta të rrëfimit.','Romanın refleksivitesi, kendi anlatı koşullarını tekrar tekrar görünür kılmasında ortaya çıkar.'))
    ]
  }),
  entry({
    word: 'rekapitulieren', partOfSpeech: 'Verb', infinitive: 'rekapitulieren', syllableBreak: 're-ka-pi-tu-lie-ren',
    topics: ['meetings-and-presentations','business-communication','education-and-training'], usageLabels: ['formal','written','business','academic'],
    collocations: [{ text: 'die wichtigsten Punkte rekapitulieren', meaning: 'to recap the main points' }],
    meanings: meaning('يلخص من جديد؛ يستعرض مجدداً','کورتکردنەوەی دووبارە؛ پێداچوونەوە','to recap; to summarize again','مرور و جمع‌بندی کردن','ji nû ve kurt kirin; vegerandin','rekapitulować; podsumować','a recapitula; a rezuma','рекапитулировать; кратко повторить','rikapituloj; përmbledh sërish','özetlemek; yeniden gözden geçirmek'),
    examples: [
      ex('Am Ende des Workshops rekapitulierte die Moderatorin die offenen Entscheidungen, damit niemand mit stillschweigenden Annahmen weiterarbeitete.', trans('في نهاية ورشة العمل لخصت الميسرة القرارات المفتوحة حتى لا يواصل أحد العمل بافتراضات ضمنية.','لە کۆتایی وۆرکشۆپەکەدا بەڕێوەبەرەکە بڕیارە کراوەکان کورتکردەوە بۆ ئەوەی کەس بە گریمانەی بێدەنگی کار درێژ نەکات.','At the end of the workshop, the facilitator recapped the open decisions so that no one would continue working with tacit assumptions.','در پایان کارگاه، تسهیل‌گر تصمیم‌های باز را مرور کرد تا هیچ‌کس با فرض‌های ناگفته کار را ادامه ندهد.','Di dawiya atolyeyê de rêvebera civînê biryarên vekirî rekapîtule kir da ku kes bi texmînên bêdeng karê xwe nedomîne.','Na końcu warsztatu moderatorka zrekapitulowała otwarte decyzje, aby nikt nie kontynuował pracy z milczącymi założeniami.','La finalul workshopului, moderatoarea a recapitulat deciziile deschise, pentru ca nimeni să nu continue cu presupuneri tacite.','В конце семинара модератор кратко повторила открытые решения, чтобы никто не продолжал работу с неявными предположениями.','Në fund të punëtorisë, moderatorja rikapituloi vendimet e hapura që askush të mos vazhdonte me supozime të heshtura.','Atölyenin sonunda moderatör açık kararları özetledi ki kimse örtük varsayımlarla çalışmaya devam etmesin.')),
      ex('Bevor die Prüfung begann, rekapitulierte der Professor die Argumentationslinie der letzten Sitzung.', trans('قبل بدء الامتحان استعرض الأستاذ خط الحجة في الجلسة السابقة.','پێش دەستپێکردنی تاقیکردنەوەکە، پرۆفیسۆرەکە هێڵی ئارگومێنتی دانیشتنی پێشووی کورتکردەوە.','Before the exam began, the professor recapped the line of argument from the previous session.','پیش از شروع امتحان، استاد خط استدلال جلسه قبل را مرور کرد.','Berî ku ezmûn dest pê bike, profesor rêza argumanên danişîna berê rekapîtule kir.','Przed rozpoczęciem egzaminu profesor zrekapitulował linię argumentacji z poprzednich zajęć.','Înainte de începerea examenului, profesorul a recapitulat linia argumentativă a ultimei ședințe.','Перед началом экзамена профессор кратко повторил ход аргументации прошлого занятия.','Para se të fillonte provimi, profesori rikapituloi vijën e argumentimit të seancës së fundit.','Sınav başlamadan önce profesör önceki dersin argüman çizgisini özetledi.'))
    ]
  }),
  entry({
    word: 'die Reminiszenz', partOfSpeech: 'Noun', article: 'die', plural: 'Reminiszenzen', syllableBreak: 'Re-mi-nis-zenz',
    topics: ['culture-and-media','advanced-analysis','social-and-relationships'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'eine Reminiszenz an etwas', meaning: 'a reminiscence or allusion to something' }],
    meanings: meaning('ذكرى؛ إشارة تذكيرية','بیرەوەری؛ ئاماژەی یادکردنەوە','reminiscence; allusion','یادآوری؛ اشاره خاطره‌انگیز','bîranîn; amajeya bîranînê','reminiscencja; nawiązanie','reminiscență; aluzie','реминисценция; воспоминание','reminishencë; aluzion kujtues','anımsama; gönderme'),
    examples: [
      ex('Das alte Logo blieb als feine Reminiszenz in der neuen Markenwelt erhalten, ohne das moderne Design zu dominieren.', trans('بقي الشعار القديم كإشارة تذكيرية دقيقة في عالم العلامة الجديد من دون أن يهيمن على التصميم الحديث.','لۆگۆ کۆنەکە وەک یادکردنەوەیەکی نازک لە جیهانی نوێی براندەکەدا مایەوە، بەبێ ئەوەی دیزاینی مۆدێرن زاڵ بکات.','The old logo remained as a subtle reminiscence in the new brand world without dominating the modern design.','لوگوی قدیمی به‌عنوان یادآوری ظریفی در هویت جدید برند باقی ماند، بدون آن‌که طراحی مدرن را تحت‌الشعاع قرار دهد.','Logoya kevn wek reminîsanseke nazik di cîhana nû ya markayê de ma bê ku sêwirana modern serdest bike.','Stare logo pozostało jako subtelna reminiscencja w nowym świecie marki, nie dominując nad nowoczesnym designem.','Vechiul logo a rămas ca o reminiscență subtilă în noua lume a brandului, fără a domina designul modern.','Старый логотип сохранился как тонкая реминисценция в новом брендовом мире, не доминируя над современным дизайном.','Logoja e vjetër mbeti si një reminishencë e hollë në botën e re të markës, pa dominuar dizajnin modern.','Eski logo, modern tasarıma hâkim olmadan yeni marka dünyasında ince bir anımsama olarak kaldı.')),
      ex('Die Szene am Bahnhof ist eine deutliche Reminiszenz an den frühen Film noir.', trans('مشهد محطة القطار هو إشارة واضحة إلى بدايات الفيلم noir.','دیمەنی وێستگەی شەمەندەفەر ئاماژەیەکی ڕوونە بۆ فیلم نواری سەرەتایی.','The scene at the train station is a clear reminiscence of early film noir.','صحنه ایستگاه قطار اشاره‌ای روشن به فیلم نوآر اولیه است.','Dîmena li stasyona trenê reminîsanseke zelal a film noir a destpêkê ye.','Scena na dworcu jest wyraźną reminiscencją wczesnego filmu noir.','Scena din gară este o reminiscență clară a filmului noir timpuriu.','Сцена на вокзале является явной реминисценцией раннего нуара.','Skena në stacionin e trenit është një reminishencë e qartë e film noir-it të hershëm.','Tren istasyonundaki sahne erken dönem kara filme açık bir göndermedir.'))
    ]
  }),
  entry({
    word: 'das Ressentiment', partOfSpeech: 'Noun', article: 'das', plural: 'Ressentiments', syllableBreak: 'Res-sen-ti-ment',
    topics: ['social-and-relationships','advanced-analysis','business-communication'], usageLabels: ['formal','written','sensitive','analysis'],
    collocations: [{ text: 'alte Ressentiments bedienen', meaning: 'to appeal to old resentments' }],
    meanings: meaning('ضغينة كامنة؛ استياء متراكم','کینەی شاراوە؛ ناڕەزایی کۆبووەوە','resentment; deep-seated grievance','کینه و رنجش عمیق؛ دلخوری انباشته','kîneya kûr; nerazîbûna komkirî','resentyment; uraza','resentiment; resentiment adânc','ресентимент; затаенная обида','resentiment; mllef i thellë','hınç; derin kırgınlık'),
    examples: [
      ex('Nach der gescheiterten Fusion blieb im Team ein Ressentiment gegenüber der Zentrale, das jede neue Initiative belastete.', trans('بعد فشل الاندماج بقي في الفريق استياء عميق تجاه الإدارة المركزية أثقل كل مبادرة جديدة.','دوای یەکگرتنی شکستەوە، لە تیمەکەدا کینەیەک بەرامبەر ناوەند مایەوە کە هەر هەوڵێکی نوێی قورس دەکرد.','After the failed merger, a resentment toward headquarters remained in the team and burdened every new initiative.','پس از ادغام ناموفق، در تیم رنجشی عمیق نسبت به مرکز باقی ماند که هر ابتکار تازه‌ای را سنگین می‌کرد.','Piştî yekbûna têkçûyî, di tîmê de kîneyek li hember navendê ma ku her destpêşxeriya nû giran dikir.','Po nieudanej fuzji w zespole pozostał resentyment wobec centrali, który obciążał każdą nową inicjatywę.','După fuziunea eșuată, în echipă a rămas un resentiment față de sediul central, care împovăra fiecare inițiativă nouă.','После неудачного слияния в команде осталось resentiment к центральному офису, отягощавшее каждую новую инициативу.','Pas bashkimit të dështuar, në ekip mbeti një resentiment ndaj qendrës, që rëndonte çdo nismë të re.','Başarısız birleşmeden sonra ekipte merkeze karşı bir hınç kaldı ve her yeni girişimi ağırlaştırdı.')),
      ex('Der Essay analysiert, wie politisches Ressentiment aus realer Kränkung und gezielter Vereinfachung entsteht.', trans('يحلل المقال كيف ينشأ الحقد السياسي من جرح حقيقي وتبسيط متعمد.','وتارەکە شیدەکاتەوە کە چۆن کینەی سیاسی لە سووکایەتیی ڕاستەقینە و سادەکردنەوەی ئامانجدار دروست دەبێت.','The essay analyzes how political resentment arises from real injury and deliberate simplification.','مقاله تحلیل می‌کند که چگونه رنجش سیاسی از آزردگی واقعی و ساده‌سازی هدفمند پدید می‌آید.','Gotar analîz dike ka çawa resentîmenta siyasî ji êşa rastîn û hêsankirina bi mebest çêdibe.','Esej analizuje, jak polityczny resentyment powstaje z realnego zranienia i celowego uproszczenia.','Eseul analizează cum resentimentul politic apare dintr-o ofensă reală și o simplificare deliberată.','Эссе анализирует, как политический ресентимент возникает из реальной обиды и намеренного упрощения.','Eseja analizon se si resentimenti politik lind nga lëndimi real dhe thjeshtimi i qëllimshëm.','Deneme, siyasi hıncın gerçek incinme ve kasıtlı basitleştirmeden nasıl doğduğunu analiz eder.'))
    ]
  }),
  entry({
    word: 'reüssieren', partOfSpeech: 'Verb', infinitive: 'reüssieren', syllableBreak: 're-üs-sie-ren',
    topics: ['work-and-jobs','business-communication','culture-and-media'], usageLabels: ['formal','written','advanced','business'],
    collocations: [{ text: 'am Markt reüssieren', meaning: 'to succeed in the market' }],
    meanings: meaning('ينجح؛ يحقق قبولاً أو شهرة','سەرکەوتن؛ ناوبانگ دەرکردن','to succeed; to make a successful impact','موفق شدن؛ جا افتادن و درخشیدن','serkeftin; navdar bûn','odnieść sukces; zaistnieć','a reuși; a se impune','преуспеть; добиться успеха','arrij sukses; spikat','başarı kazanmak; tutunmak'),
    examples: [
      ex('Das Produkt konnte trotz starker Technik nicht reüssieren, weil Vertrieb und Support zu spät aufgebaut wurden.', trans('لم يتمكن المنتج من النجاح رغم قوته التقنية لأن المبيعات والدعم بُنيا متأخرين جداً.','بەرهەمەکە سەرەڕای تەکنیکی بەهێز نەیتوانی سەرکەوێت، چونکە فرۆشتن و پشتگیری زۆر درەنگ دامەزرابوون.','Despite strong technology, the product could not succeed because sales and support were built up too late.','محصول با وجود فناوری قوی نتوانست موفق شود، چون فروش و پشتیبانی خیلی دیر شکل گرفتند.','Berhem tevî teknîka xurt nikarî reüssieren bike, ji ber ku firotan û piştgirî pir dereng hatin avakirin.','Produkt mimo mocnej technologii nie zdołał odnieść sukcesu, ponieważ sprzedaż i wsparcie zbudowano zbyt późno.','Produsul nu a putut reuși, în ciuda tehnologiei solide, deoarece vânzările și suportul au fost dezvoltate prea târziu.','Продукт не смог преуспеть, несмотря на сильную технологию, потому что продажи и поддержка были выстроены слишком поздно.','Produkti nuk arriti sukses pavarësisht teknologjisë së fortë, sepse shitjet dhe suporti u ngritën shumë vonë.','Ürün güçlü teknolojiye rağmen tutunamadı, çünkü satış ve destek çok geç kuruldu.')),
      ex('Die Autorin reüssierte erst mit ihrem dritten Roman, der Kritik und Publikum gleichermaßen überzeugte.', trans('لم تحقق الكاتبة نجاحاً إلا بروايتها الثالثة التي أقنعت النقاد والجمهور على السواء.','نووسەرەکە تەنها بە ڕۆمانی سێیەمیدا سەرکەوت، کە ڕەخنەگران و خوێنەران بە یەکسانی قایل کرد.','The author succeeded only with her third novel, which convinced critics and readers alike.','نویسنده تازه با رمان سومش موفق شد؛ رمانی که هم منتقدان و هم مخاطبان را قانع کرد.','Nivîskar tenê bi romana xwe ya sêyemîn serkeft, ku rexnegir û xwendevanan bi hev re qanih kir.','Autorka odniosła sukces dopiero trzecią powieścią, która przekonała zarówno krytykę, jak i publiczność.','Autoarea a reușit abia cu al treilea roman, care a convins deopotrivă critica și publicul.','Авторка добилась успеха лишь с третьим романом, который убедил и критиков, и публику.','Autorja arriti sukses vetëm me romanin e saj të tretë, që bindi njësoj kritikën dhe publikun.','Yazar ancak eleştirmenleri ve okurları aynı ölçüde ikna eden üçüncü romanıyla başarı kazandı.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 068', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

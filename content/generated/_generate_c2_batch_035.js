const fs = require('fs');
const cp = require('child_process');
const path = require('path');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '035';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const expected = ['das Figurenensemble', 'die Figurenrede', 'die Fiktionalität', 'die Finesse', 'flechten', 'der Fluchtpunkt'];
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
    word:'das Figurenensemble', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'das', plural:'Figurenensembles', infinitive:null, pronunciationIpa:null, syllableBreak:'Fi-gu-ren-en-sem-ble',
    topics:['culture-and-media','advanced-analysis','education-and-training'], usageLabels:['formal','written','academic','analysis'], contextLabels:[],
    grammarNotes:['neuter compound noun; used in literary, theatre, and film analysis'], collocations:[{text:'ein vielschichtiges Figurenensemble', meaning:'a multilayered cast of characters'}],
    wordFamilies:[{lemma:'die Figur', relationLabel:'noun', note:null},{lemma:'das Ensemble', relationLabel:'noun', note:null}], relations:[],
    meanings:m('مجموعة الشخصيات في عمل أدبي أو مسرحي','کۆمەڵە کارەکتەرەکانی دەق یان شانۆ','cast or ensemble of characters','مجموعه شخصیت‌ها در یک اثر','koma kesayetiyan','zespół postaci','ansamblu de personaje','ансамбль персонажей','ansambël personazhesh','karakterler topluluğu'),
    examples:[
      ex('Das Figurenensemble des Romans ist so angelegt, dass keine Perspektive den moralischen Konflikt vollständig beherrscht.', m('صُممت مجموعة شخصيات الرواية بحيث لا تهيمن أي زاوية نظر بالكامل على الصراع الأخلاقي.', 'کۆمەڵە کارەکتەرەکانی ڕۆمانەکە وا داڕێژراوە کە هیچ دیدگایەک بە تەواوی زاڵ نابێت بەسەر ناکۆکیی ئەخلاقیدا.', 'The novel’s ensemble of characters is designed so that no single perspective fully dominates the moral conflict.', 'مجموعه شخصیت‌های رمان طوری طراحی شده که هیچ دیدگاهی کاملاً بر تعارض اخلاقی مسلط نمی‌شود.', 'Koma kesayetiyên romanê wisa hatiye danîn ku tu perspektîf bi tevahî li ser nakokiya exlaqî serdest nabe.', 'Zespół postaci w powieści został skonstruowany tak, że żadna perspektywa nie dominuje całkowicie nad konfliktem moralnym.', 'Ansamblul de personaje al romanului este construit astfel încât nicio perspectivă să nu domine complet conflictul moral.', 'Ансамбль персонажей романа выстроен так, что ни одна перспектива полностью не господствует над нравственным конфликтом.', 'Ansambli i personazheve të romanit është ndërtuar në mënyrë që asnjë perspektivë të mos e zotërojë plotësisht konfliktin moral.', 'Romanın karakterler topluluğu, ahlaki çatışmaya tek bir bakış açısının tamamen hakim olamayacağı şekilde kurulmuştur.')),
      ex('Für die Serie wurde das Figurenensemble erweitert, damit auch die Nebenrollen eigene Konfliktlinien tragen.', m('تم توسيع مجموعة الشخصيات في المسلسل لكي تحمل الأدوار الثانوية أيضاً خطوط صراع خاصة بها.', 'بۆ زنجیرەکە کۆمەڵە کارەکتەرەکان فراوان کرا، بۆ ئەوەی ڕۆڵە لاوەکییەکانیش هێڵی ناکۆکیی تایبەت بە خۆیان هەبێت.', 'For the series, the ensemble of characters was expanded so that supporting roles also carry their own lines of conflict.', 'برای سریال، مجموعه شخصیت‌ها گسترش یافت تا نقش‌های فرعی هم خط تعارض مستقل داشته باشند.', 'Ji bo rêzefîlmê koma kesayetiyan hate firehkirin, da ku rola alî jî xetên nakokiyê yên xwe hilgirin.', 'Na potrzeby serialu rozszerzono zespół postaci, aby role drugoplanowe miały własne linie konfliktu.', 'Pentru serial, ansamblul de personaje a fost extins, astfel încât și rolurile secundare să poarte propriile linii de conflict.', 'Для сериала ансамбль персонажей расширили, чтобы второстепенные роли тоже несли собственные конфликтные линии.', 'Për serialin, ansambli i personazheve u zgjerua që edhe rolet dytësore të mbajnë linjat e tyre të konfliktit.', 'Dizi için karakterler topluluğu genişletildi, böylece yan roller de kendi çatışma hatlarını taşıyabildi.'))
    ]
  },
  {
    word:'die Figurenrede', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'die', plural:'Figurenreden', infinitive:null, pronunciationIpa:null, syllableBreak:'Fi-gu-ren-re-de',
    topics:['culture-and-media','advanced-analysis','education-and-training'], usageLabels:['formal','written','academic','analysis'], contextLabels:[],
    grammarNotes:['feminine compound noun; term from narratology and literary analysis'], collocations:[{text:'direkte Figurenrede', meaning:'direct character speech'}],
    wordFamilies:[{lemma:'die Figur', relationLabel:'noun', note:null},{lemma:'die Rede', relationLabel:'noun', note:null}], relations:[],
    meanings:m('كلام الشخصيات داخل النص','قسەی کارەکتەرەکان لە ناو دەقدا','character speech in a text','گفتار شخصیت‌ها در متن','axaftina kesayetiyan di nivîsê de','mowa postaci','discursul personajelor','речь персонажей','ligjërim i personazheve','karakter konuşması'),
    examples:[
      ex('Die gebrochene Figurenrede macht deutlich, wie sehr die Protagonistin zwischen Anpassung und Widerstand schwankt.', m('يكشف كلام الشخصية المتكسر مدى تردد البطلة بين التكيف والمقاومة.', 'قسەی پارچەپارچەی کارەکتەرەکە ڕوون دەکاتەوە کە پاڵەوانەکە چەندە لە نێوان خۆگونجاندن و بەرگریدا دەگۆڕێت.', 'The fragmented character speech shows how strongly the protagonist wavers between adaptation and resistance.', 'گفتار گسسته شخصیت نشان می‌دهد قهرمان تا چه حد میان سازگاری و مقاومت در نوسان است.', 'Axaftina perçe-perçeyî ya kesayetiyê diyar dike ku qehreman çiqas di navbera xweguncandin û berxwedanê de dihejê.', 'Połamana mowa postaci pokazuje, jak bardzo protagonistka waha się między dostosowaniem a oporem.', 'Discursul fragmentat al personajului arată cât de mult oscilează protagonista între adaptare și rezistență.', 'Разорванная речь персонажа показывает, насколько героиня колеблется между приспособлением и сопротивлением.', 'Ligjërimi i thyer i personazhit tregon sa shumë lëkundet protagonistja mes përshtatjes dhe rezistencës.', 'Parçalı karakter konuşması, başkahramanın uyum sağlama ile direnme arasında ne kadar gidip geldiğini gösterir.')),
      ex('In der Drehbuchbesprechung wurde die Figurenrede gekürzt, weil sie zu viel erklärte und zu wenig Spannung erzeugte.', m('في اجتماع مناقشة السيناريو تم اختصار كلام الشخصيات لأنه كان يشرح كثيراً ولا يخلق توتراً كافياً.', 'لە دانیشتنی گفتوگۆی سیناریۆدا قسەی کارەکتەرەکان کورت کرایەوە، چونکە زۆر شت ڕوون دەکردەوە و کەمتر گرژی دروست دەکرد.', 'In the script meeting, the character dialogue was shortened because it explained too much and created too little tension.', 'در جلسه بررسی فیلمنامه، گفتار شخصیت‌ها کوتاه شد چون بیش از حد توضیح می‌داد و تنش کمی ایجاد می‌کرد.', 'Di civîna senaryoyê de axaftina kesayetiyan hate kurtkirin, ji ber ku pir rave dikir û kêm tengezarî diafirand.', 'Podczas omówienia scenariusza skrócono wypowiedzi postaci, bo zbyt wiele wyjaśniały i tworzyły za mało napięcia.', 'În discuția despre scenariu, replicile personajelor au fost scurtate, deoarece explicau prea mult și creau prea puțină tensiune.', 'На обсуждении сценария речь персонажей сократили, потому что она слишком много объясняла и создавала слишком мало напряжения.', 'Në diskutimin e skenarit, ligjërimi i personazheve u shkurtua sepse shpjegonte tepër dhe krijonte pak tension.', 'Senaryo toplantısında karakter konuşmaları kısaltıldı, çünkü çok fazla açıklıyor ve yeterince gerilim yaratmıyordu.'))
    ]
  },
  {
    word:'die Fiktionalität', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'die', plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'Fik-ti-o-na-li-tät',
    topics:['culture-and-media','advanced-analysis','education-and-training'], usageLabels:['formal','written','academic','analysis'], contextLabels:[],
    grammarNotes:['feminine abstract noun; plural is uncommon'], collocations:[{text:'die Fiktionalität eines Textes', meaning:'the fictionality of a text'}],
    wordFamilies:[{lemma:'fiktiv', relationLabel:'adjective', note:null},{lemma:'die Fiktion', relationLabel:'noun', note:null}], relations:[],
    meanings:m('الطابع التخييلي؛ كون الشيء خيالياً','خەیاڵیبوون؛ تایبەتمەندی فیکشنی','fictionality; fictional character','داستانی‌بودن؛ ماهیت تخیلی','xeyalîbûn; taybetiya fîksiyonî','fikcyjność','ficționalitate','фикциональность','fiksionalitet','kurmacalık'),
    examples:[
      ex('Die Fiktionalität des Berichts wird bewusst verschleiert, damit dokumentarische Autorität entsteht.', m('تُخفى الطبيعة التخيلية للتقرير عمداً لكي تنشأ سلطة توثيقية.', 'خەیاڵیبوونی ڕاپۆرتەکە بە ئەنقەست دەشاردرێتەوە بۆ ئەوەی دەسەڵاتی بەڵگەیی دروست ببێت.', 'The report’s fictionality is deliberately obscured in order to create documentary authority.', 'داستانی‌بودن گزارش عمداً پنهان می‌شود تا اقتدار مستندگونه ایجاد شود.', 'Fîksiyonîbûna raporê bi zanebûn tê veşartin da ku otorîteya belgeyî biafirîne.', 'Fikcyjność raportu jest celowo zaciemniana, aby powstał autorytet dokumentalny.', 'Ficționalitatea raportului este ascunsă deliberat pentru a crea autoritate documentară.', 'Фикциональность отчета намеренно скрывается, чтобы возник авторитет документальности.', 'Fiksionaliteti i raportit fshihet qëllimisht për të krijuar autoritet dokumentar.', 'Raporun kurmacalığı, belgesel bir otorite yaratmak için bilinçli olarak gizlenir.')),
      ex('Im Seminar diskutierten wir, wie soziale Medien die Grenze zwischen Fiktionalität und Selbstdarstellung verwischen.', m('ناقشنا في الحلقة الدراسية كيف تطمس وسائل التواصل الاجتماعي الحد بين التخييل وعرض الذات.', 'لە سیمینارەکەدا گفتوگۆمان کرد کە میدیای کۆمەڵایەتی چۆن سنووری نێوان خەیاڵیبوون و خۆپیشاندان دەسڕێتەوە.', 'In the seminar, we discussed how social media blurs the boundary between fictionality and self-presentation.', 'در سمینار بحث کردیم که شبکه‌های اجتماعی چگونه مرز میان داستانی‌بودن و خودنمایی را محو می‌کنند.', 'Di seminarê de me nîqaş kir ku medyaya civakî çawa sînorê di navbera fîksiyonîbûn û xwenîşandanê de tevlihev dike.', 'Na seminarium omawialiśmy, jak media społecznościowe zacierają granicę między fikcyjnością a autoprezentacją.', 'La seminar am discutat cum rețelele sociale estompează granița dintre ficționalitate și auto-prezentare.', 'На семинаре мы обсуждали, как социальные сети размывают границу между фикциональностью и самопрезентацией.', 'Në seminar diskutuam se si mediat sociale e zbehin kufirin midis fiksionalitetit dhe vetëprezantimit.', 'Seminerde sosyal medyanın kurmacalık ile öz sunum arasındaki sınırı nasıl bulanıklaştırdığını tartıştık.'))
    ]
  },
  {
    word:'die Finesse', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'die', plural:'Finessen', infinitive:null, pronunciationIpa:null, syllableBreak:'Fi-nes-se',
    topics:['business-communication','advanced-analysis','culture-and-media'], usageLabels:['formal','written','business','advanced'], contextLabels:[],
    grammarNotes:['feminine noun; often denotes subtle skill, refinement, or tactical nuance'], collocations:[{text:'strategische Finesse', meaning:'strategic subtlety or finesse'}],
    wordFamilies:[{lemma:'finessenreich', relationLabel:'adjective', note:null}], relations:[],
    meanings:m('براعة دقيقة؛ حيلة ذكية؛ ظرافة','وردەکاری زیرەکانە؛ لێهاتوویی ناسک','finesse; subtle skill; clever nuance','ظرافت؛ مهارت زیرکانه؛ نکته‌سنجی','hûneriya nazik; jêhatîbûna hûr','finezja; subtelny chwyt','finesse; subtilitate abilă','тонкость; изящество; хитроумный нюанс','finesë; mjeshtëri e hollë','incelik; ustalık; nüans'),
    examples:[
      ex('Die Finesse des Angebots lag darin, dem Kunden Flexibilität zu geben, ohne die Marge offen zu gefährden.', m('كانت براعة العرض في منح العميل مرونة من دون تعريض الهامش للخطر بشكل واضح.', 'وردەکاری پێشنیارەکە لەوەدا بوو کە نەرمونیانی بە کڕیار بدات، بێ ئەوەی بە ئاشکرا پەراوێزی قازانج بخاتە مەترسییەوە.', 'The finesse of the offer lay in giving the customer flexibility without openly endangering the margin.', 'ظرافت پیشنهاد در این بود که به مشتری انعطاف بدهد، بی‌آنکه حاشیه سود را آشکارا به خطر بیندازد.', 'Hûneriya pêşniyarê di wê de bû ku ji xerîdar re nermbûn bide bê ku marjê bi eşkere bixe metirsiyê.', 'Finezyjność oferty polegała na daniu klientowi elastyczności bez jawnego narażania marży.', 'Finesa ofertei consta în a oferi clientului flexibilitate fără a periclita deschis marja.', 'Тонкость предложения заключалась в том, чтобы дать клиенту гибкость, не подвергая явно риску маржу.', 'Finesa e ofertës qëndronte në dhënien e fleksibilitetit klientit pa rrezikuar hapur marzhin.', 'Teklifin inceliği, marjı açıkça tehlikeye atmadan müşteriye esneklik sağlamasında yatıyordu.')),
      ex('Die Pianistin spielte die leisen Passagen mit einer Finesse, die im großen Saal fast zerbrechlich wirkte.', m('عزفت عازفة البيانو المقاطع الهادئة بظرافة بدت في القاعة الكبيرة شبه هشة.', 'پیانۆژەنەکە بەشە بێدەنگەکانی بە ناسکییەک ژەنی کە لە هۆڵە گەورەکەدا نزیک بوو بە شکاندنەوە دەردەکەوت.', 'The pianist played the quiet passages with a finesse that seemed almost fragile in the large hall.', 'پیانیست بخش‌های آرام را با ظرافتی نواخت که در سالن بزرگ تقریباً شکننده به نظر می‌رسید.', 'Pîyanîstê beşên hêdî bi hûneriyek nazik lîst ku di salona mezin de nêzîkî şikestîbûnê xuya dikir.', 'Pianistka zagrała ciche fragmenty z finezją, która w dużej sali wydawała się niemal krucha.', 'Pianista a cântat pasajele liniștite cu o finețe care părea aproape fragilă în sala mare.', 'Пианистка сыграла тихие пассажи с такой тонкостью, что в большом зале она казалась почти хрупкой.', 'Pianistja i luajti pasazhet e qeta me një finesë që në sallën e madhe dukej pothuajse e brishtë.', 'Piyanist sessiz pasajları büyük salonda neredeyse kırılgan görünen bir incelikle çaldı.'))
    ]
  },
  {
    word:'flechten', language:'de', cefrLevel:level, partOfSpeech:'Verb', article:null, plural:null, infinitive:'flechten', pronunciationIpa:null, syllableBreak:'flech-ten',
    topics:['culture-and-media','advanced-analysis','production-and-maintenance'], usageLabels:['formal','written','analysis'], contextLabels:[],
    grammarNotes:['transitive verb; literally to braid or weave, figuratively to interweave elements'], collocations:[{text:'Motive ineinander flechten', meaning:'to interweave motifs'}],
    wordFamilies:[{lemma:'das Geflecht', relationLabel:'noun', note:null}], relations:[],
    meanings:m('ينسج؛ يضفر؛ يربط عناصر ببعضها','بچنێت؛ ببافێت؛ توخمەکان تێکەڵ بکات','to braid; to weave; to interweave','بافتن؛ درهم تنیدن','çêlik kirin; têk hev dan','pleść; splatać','a împleti; a întrețese','плести; вплетать','të thurësh; të ndërthurësh','örmek; iç içe geçirmek'),
    examples:[
      ex('Der Autor flicht historische Dokumente in die Handlung, ohne den Rhythmus der Erzählung zu zerstören.', m('ينسج الكاتب وثائق تاريخية في الحبكة من دون أن يدمّر إيقاع السرد.', 'نووسەرەکە بەڵگەنامەی مێژوویی دەخاتە ناو ڕووداوەکانەوە، بێ ئەوەی ریتمی گێڕانەوەکە تێک بدات.', 'The author weaves historical documents into the plot without destroying the rhythm of the narrative.', 'نویسنده اسناد تاریخی را در پیرنگ می‌تند، بی‌آنکه ریتم روایت را خراب کند.', 'Nivîskar belgeyên dîrokî di çîrokê de têk dide bê ku rîtma vegotinê xirab bike.', 'Autor wplata dokumenty historyczne w fabułę, nie niszcząc rytmu narracji.', 'Autorul împletește documente istorice în acțiune fără a distruge ritmul narațiunii.', 'Автор вплетает исторические документы в сюжет, не разрушая ритм повествования.', 'Autori ndërthur dokumente historike në ngjarje pa shkatërruar ritmin e rrëfimit.', 'Yazar tarihsel belgeleri anlatının ritmini bozmadan olay örgüsüne işler.')),
      ex('In der Werkstatt lernte sie, feine Drähte so zu flechten, dass stabile und zugleich leichte Bauteile entstehen.', m('تعلمت في الورشة أن تضفر الأسلاك الدقيقة بحيث تنشأ قطع مستقرة وخفيفة في الوقت نفسه.', 'لە کارگەکەدا فێر بوو وایەرە باریکەکان وا ببافێت کە بەشە جێگیر و لە هەمان کاتدا سووکەکان دروست ببن.', 'In the workshop, she learned to braid fine wires so that stable yet lightweight components are created.', 'در کارگاه یاد گرفت سیم‌های نازک را طوری ببافد که قطعاتی پایدار و در عین حال سبک ساخته شوند.', 'Di atolyeyê de wê fêr bû têlên nazik wisa çêlik bike ku parçeyên stabîl û sivik biafirin.', 'W warsztacie nauczyła się splatać cienkie druty tak, aby powstawały stabilne, a zarazem lekkie elementy.', 'În atelier a învățat să împletească fire subțiri astfel încât să rezulte componente stabile și totodată ușoare.', 'В мастерской она научилась плести тонкие проволоки так, чтобы получались прочные и одновременно легкие детали.', 'Në punishte mësoi të thurte tela të hollë në mënyrë që të krijoheshin pjesë të qëndrueshme dhe njëkohësisht të lehta.', 'Atölyede ince telleri hem sağlam hem de hafif parçalar oluşturacak şekilde örmeyi öğrendi.'))
    ]
  },
  {
    word:'der Fluchtpunkt', language:'de', cefrLevel:level, partOfSpeech:'Noun', article:'der', plural:'Fluchtpunkte', infinitive:null, pronunciationIpa:null, syllableBreak:'Flucht-punkt',
    topics:['culture-and-media','advanced-analysis','planning-and-projects'], usageLabels:['formal','written','academic','analysis'], contextLabels:[],
    grammarNotes:['masculine compound noun; used in perspective drawing and figuratively as a point of orientation or escape'], collocations:[{text:'ein gemeinsamer Fluchtpunkt', meaning:'a shared vanishing point or point of orientation'}],
    wordFamilies:[{lemma:'die Flucht', relationLabel:'noun', note:null},{lemma:'der Punkt', relationLabel:'noun', note:null}], relations:[],
    meanings:m('نقطة التلاشي؛ نقطة مرجعية أو مهرب','خاڵی ونبوون؛ خاڵی ئاراستە یان هەڵهاتن','vanishing point; point of orientation or escape','نقطه گریز؛ نقطه تلاقی یا جهت‌گیری','xala windabûnê; xala arasteyê','punkt zbiegu; punkt odniesienia','punct de fugă; punct de orientare','точка схода; ориентир','pikë zhdukjeje; pikë orientimi','kaçış noktası; yönelim noktası'),
    examples:[
      ex('Im Entwurf liegt der Fluchtpunkt bewusst außerhalb des Bildes, wodurch die Architektur instabil wirkt.', m('في التصميم تقع نقطة التلاشي عمداً خارج الصورة، مما يجعل العمارة تبدو غير مستقرة.', 'لە دیزاینەکەدا خاڵی ونبوون بە ئەنقەست لە دەرەوەی وێنەکە دانراوە، بەمەش تەلارسازییەکە ناجێگیر دەردەکەوێت.', 'In the design, the vanishing point is deliberately placed outside the image, making the architecture appear unstable.', 'در طرح، نقطه گریز عمداً بیرون تصویر قرار دارد و همین باعث می‌شود معماری ناپایدار به نظر برسد.', 'Di sêwirandinê de xala windabûnê bi zanebûn li derveyî wêneyê ye, ji ber vê avahî ne stabîl xuya dike.', 'W projekcie punkt zbiegu celowo znajduje się poza obrazem, przez co architektura wydaje się niestabilna.', 'În proiect, punctul de fugă se află deliberat în afara imaginii, ceea ce face ca arhitectura să pară instabilă.', 'В проекте точка схода намеренно находится за пределами изображения, из-за чего архитектура кажется нестабильной.', 'Në projekt, pika e zhdukjes është qëllimisht jashtë figurës, duke e bërë arkitekturën të duket e paqëndrueshme.', 'Taslakta kaçış noktası bilinçli olarak görüntünün dışına yerleştirilmiştir; bu da mimariyi istikrarsız gösterir.')),
      ex('Für die Reformdebatte wurde die soziale Gerechtigkeit zum Fluchtpunkt sehr unterschiedlicher Interessen.', m('في نقاش الإصلاح أصبحت العدالة الاجتماعية نقطة التقاء لمصالح شديدة الاختلاف.', 'لە گفتوگۆی چاکسازییەکەدا دادپەروەری کۆمەڵایەتی بوو بە خاڵی ئاراستە بۆ بەرژەوەندییە زۆر جیاوازەکان.', 'In the reform debate, social justice became the point of orientation for very different interests.', 'در بحث اصلاحات، عدالت اجتماعی به نقطه جهت‌گیری منافع بسیار متفاوت تبدیل شد.', 'Di nîqaşa reformê de edaleta civakî bû xala arasteyê ji bo berjewendiyên pir cuda.', 'W debacie reform społeczna sprawiedliwość stała się punktem odniesienia dla bardzo różnych interesów.', 'În dezbaterea despre reformă, justiția socială a devenit punctul de orientare al unor interese foarte diferite.', 'В дискуссии о реформе социальная справедливость стала ориентиром для очень разных интересов.', 'Në debatin për reformën, drejtësia sociale u bë pikë orientimi për interesa shumë të ndryshme.', 'Reform tartışmasında sosyal adalet, çok farklı çıkarların yöneldiği ortak nokta haline geldi.'))
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

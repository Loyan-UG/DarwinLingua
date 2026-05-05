const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '075';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['das Sprachbild','der Sprachduktus','die Sprachgestalt','der Sprachgestus','die Sprachgewalt','sprachmächtig'];

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
    word: 'das Sprachbild', partOfSpeech: 'Noun', article: 'das', plural: 'Sprachbilder', syllableBreak: 'Sprach-bild',
    topics: ['culture-and-media','business-communication','advanced-analysis'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'ein prägnantes Sprachbild', meaning: 'a concise and vivid verbal image' }],
    meanings: meaning('صورة لغوية؛ استعارة بصرية في اللغة','وێنەی زمانی؛ دەربڕینی وێنەیی','verbal image; figurative expression','تصویر زبانی؛ بیان تصویری','wêneya zimanî; gotina mecazî','obraz językowy','imagine lingvistică; expresie figurată','языковой образ','figurë gjuhësore; imazh verbal','dilsel imge; mecazi ifade'),
    examples: [
      ex('Das Sprachbild vom „Datenstau“ half dem Management, ein abstraktes Integrationsproblem sofort zu verstehen.', meaning('ساعدت صورة «ازدحام البيانات» الإدارة على فهم مشكلة تكامل مجردة فوراً.','وێنەی زمانیی «قەیرانی ڕێی داتا» یارمەتی بەڕێوەبەرایەتی دا کێشەیەکی ئەبستراکتی یەکخستن دەستبەجێ تێبگات.','The verbal image of a “data traffic jam” helped management immediately understand an abstract integration problem.','تصویر زبانی «ترافیک داده» به مدیریت کمک کرد یک مشکل انتزاعی یکپارچه‌سازی را فوراً بفهمد.','Wêneya zimanî ya “qerebalixa daneyan” alîkarî da rêveberiyê ku pirsgirêkeke razber a entegrasyonê tavilê fam bike.','Obraz językowy „korka danych” pomógł kierownictwu natychmiast zrozumieć abstrakcyjny problem integracji.','Imaginea lingvistică a „ambuteiajului de date” a ajutat managementul să înțeleagă imediat o problemă abstractă de integrare.','Языковой образ «пробки данных» помог руководству сразу понять абстрактную проблему интеграции.','Imazhi verbal i “bllokimit të të dhënave” e ndihmoi menaxhmentin të kuptonte menjëherë një problem abstrakt integrimi.','“Veri sıkışıklığı” dilsel imgesi, yönetimin soyut bir entegrasyon sorununu hemen anlamasına yardımcı oldu.')),
      ex('Im Gedicht kehrt das Sprachbild des gefrorenen Flusses immer wieder und verbindet Stillstand mit verborgener Bewegung.', meaning('في القصيدة تعود صورة النهر المتجمد مراراً وتربط السكون بحركة خفية.','لە شیعرەکەدا وێنەی زمانیی ڕووباری بەستووبوو دووبارە دەگەڕێتەوە و وەستان بە جوڵەی شاراوە دەبەستێتەوە.','In the poem, the verbal image of the frozen river returns again and again, linking stillness with hidden movement.','در شعر، تصویر زبانی رود یخ‌زده بارها بازمی‌گردد و سکون را با حرکت پنهان پیوند می‌دهد.','Di helbestê de wêneya zimanî ya çemê qerisî her car vedigere û rawestînê bi tevgera veşartî ve girê dide.','W wierszu obraz językowy zamarzniętej rzeki powraca wielokrotnie i łączy bezruch z ukrytym ruchem.','În poem, imaginea lingvistică a râului înghețat revine mereu și leagă nemișcarea de mișcarea ascunsă.','В стихотворении языковой образ замерзшей реки постоянно возвращается и связывает неподвижность со скрытым движением.','Në poezi, imazhi verbal i lumit të ngrirë rikthehet vazhdimisht dhe lidh palëvizshmërinë me lëvizjen e fshehur.','Şiirde donmuş nehir dilsel imgesi tekrar tekrar döner ve durgunluğu gizli hareketle ilişkilendirir.'))
    ]
  }),
  entry({
    word: 'der Sprachduktus', partOfSpeech: 'Noun', article: 'der', plural: 'Sprachduktus', syllableBreak: 'Sprach-duk-tus',
    topics: ['culture-and-media','business-communication','advanced-analysis'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'ein nüchterner Sprachduktus', meaning: 'a sober style or flow of language' }],
    meanings: meaning('أسلوب لغوي؛ نبرة وانسياب الكلام','شێوازی زمان؛ ڕەوت و تۆنی قسە','linguistic style; tone and flow of language','سبک زبانی؛ آهنگ و شیوه بیان','şêwaza zimanî; ton û herikîna axaftinê','dukt językowy; styl wypowiedzi','duct lingvistic; stil al limbajului','языковой стиль; манера речи','stil gjuhësor; rrjedhë e të folurit','dil üslubu; söyleyiş akışı'),
    examples: [
      ex('Der Sprachduktus der Fehlermeldungen wurde bewusst vereinfacht, damit auch nichttechnische Nutzer handlungsfähig bleiben.', meaning('بُسّط أسلوب رسائل الخطأ عمداً كي يبقى المستخدمون غير التقنيين قادرين على التصرف.','شێوازی زمانی پەیامەکانی هەڵە بە ئەنقەست سادەکرا بۆ ئەوەی بەکارهێنەرانی ناتەکنیکییش بتوانن کار بکەن.','The linguistic style of the error messages was deliberately simplified so that non-technical users could still act.','سبک زبانی پیام‌های خطا عمداً ساده شد تا کاربران غیر فنی هم بتوانند اقدام کنند.','Şêwaza zimanî ya peyamên çewtiyê bi mebest hate hêsankirin da ku bikarhênerên neteknîkî jî karîger bimînin.','Dukt językowy komunikatów o błędach celowo uproszczono, aby także nietechniczni użytkownicy mogli działać.','Stilul lingvistic al mesajelor de eroare a fost simplificat intenționat, astfel încât și utilizatorii non-tehnici să poată acționa.','Языковой стиль сообщений об ошибках намеренно упростили, чтобы нетехнические пользователи тоже могли действовать.','Stili gjuhësor i mesazheve të gabimit u thjeshtua qëllimisht që edhe përdoruesit jo teknikë të mund të vepronin.','Hata mesajlarının dil üslubu, teknik olmayan kullanıcıların da harekete geçebilmesi için bilinçli olarak sadeleştirildi.')),
      ex('Der Sprachduktus des Romans bleibt kühl, selbst wenn die erzählten Ereignisse äußerste Gewalt zeigen.', meaning('يبقى أسلوب الرواية بارداً حتى عندما تُظهر الأحداث المروية عنفاً شديداً.','شێوازی زمانی ڕۆمانەکە سارد دەمێنێتەوە، تەنانەت کاتێک ڕووداوە گێڕدراوەکان توندوتیژیی زۆر پیشان دەدەن.','The novel’s linguistic style remains cool even when the events narrated show extreme violence.','سبک زبانی رمان حتی وقتی رویدادهای روایت‌شده خشونت شدید نشان می‌دهند، سرد می‌ماند.','Şêwaza zimanî ya romanê sar dimîne, tewra gava bûyerên vegotî tundûtûjiya herî zêde nîşan didin.','Dukt językowy powieści pozostaje chłodny nawet wtedy, gdy opowiadane wydarzenia pokazują skrajną przemoc.','Stilul lingvistic al romanului rămâne rece chiar și atunci când evenimentele relatate arată violență extremă.','Языковой стиль романа остается холодным, даже когда описываемые события показывают крайнее насилие.','Stili gjuhësor i romanit mbetet i ftohtë edhe kur ngjarjet e rrëfyera tregojnë dhunë ekstreme.','Romanın dil üslubu, anlatılan olaylar aşırı şiddet gösterdiğinde bile soğuk kalır.'))
    ]
  }),
  entry({
    word: 'die Sprachgestalt', partOfSpeech: 'Noun', article: 'die', plural: 'Sprachgestalten', syllableBreak: 'Sprach-ge-stalt',
    topics: ['culture-and-media','education-and-training','advanced-analysis'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'die endgültige Sprachgestalt eines Textes', meaning: 'the final linguistic form of a text' }],
    meanings: meaning('الصياغة اللغوية؛ الشكل اللغوي','شێوەی زمانی؛ فۆڕمی دەربڕین','linguistic form; verbal shape','صورت زبانی؛ شکل بیان','şêweya zimanî; forma gotinê','kształt językowy','formă lingvistică','языковая форма','formë gjuhësore','dilsel biçim'),
    examples: [
      ex('Die juristische Prüfung veränderte nicht den Inhalt, aber die Sprachgestalt des Vertrags erheblich.', meaning('لم تغيّر المراجعة القانونية المحتوى، لكنها غيّرت الصياغة اللغوية للعقد بشكل كبير.','پشکنینی یاسایی ناوەڕۆکی نەگۆڕی، بەڵام شێوەی زمانی گرێبەستەکەی زۆر گۆڕی.','The legal review did not change the content, but it significantly changed the linguistic form of the contract.','بازبینی حقوقی محتوا را تغییر نداد، اما صورت زبانی قرارداد را به‌طور چشمگیری عوض کرد.','Kontrola yasayî naverok neguherand, lê şêweya zimanî ya peymanê gelek guherand.','Kontrola prawna nie zmieniła treści, ale znacząco zmieniła kształt językowy umowy.','Verificarea juridică nu a schimbat conținutul, dar a modificat considerabil forma lingvistică a contractului.','Юридическая проверка не изменила содержание, но существенно изменила языковую форму договора.','Kontrolli ligjor nuk ndryshoi përmbajtjen, por ndryshoi ndjeshëm formën gjuhësore të kontratës.','Hukuki inceleme içeriği değiştirmedi, ancak sözleşmenin dilsel biçimini önemli ölçüde değiştirdi.')),
      ex('Erst in der späten Sprachgestalt wird sichtbar, wie stark der Text von mündlichen Erzähltraditionen geprägt ist.', meaning('لا يظهر إلا في الصيغة اللغوية المتأخرة مدى تأثر النص بتقاليد السرد الشفوي.','تەنها لە شێوەی زمانی دواتردا دیار دەبێت دەقەکە چەندە بە نەریتی گێڕانەوەی زارەکی کاریگەری وەرگرتووە.','Only in the later linguistic form does it become visible how strongly the text is shaped by oral storytelling traditions.','فقط در صورت زبانی متأخر آشکار می‌شود که متن چقدر از سنت‌های روایت شفاهی تأثیر گرفته است.','Tenê di şêweya zimanî ya paşê de xuya dibe ku nivîs çiqas ji kevneşopiyên vegotina devkî bandor girtiye.','Dopiero w późnym kształcie językowym widać, jak silnie tekst jest ukształtowany przez ustne tradycje opowiadania.','Abia în forma lingvistică târzie devine vizibil cât de puternic este textul modelat de tradițiile povestirii orale.','Лишь в поздней языковой форме видно, насколько текст сформирован устными повествовательными традициями.','Vetëm në formën e vonë gjuhësore bëhet e dukshme sa fort është formuar teksti nga traditat gojore të rrëfimit.','Metnin sözlü anlatı gelenekleriyle ne kadar güçlü biçimde şekillendiği ancak geç dilsel biçiminde görünür olur.'))
    ]
  }),
  entry({
    word: 'der Sprachgestus', partOfSpeech: 'Noun', article: 'der', plural: 'Sprachgesten', syllableBreak: 'Sprach-ges-tus',
    topics: ['culture-and-media','business-communication','advanced-analysis'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'ein autoritärer Sprachgestus', meaning: 'an authoritarian verbal gesture or stance' }],
    meanings: meaning('إيماءة لغوية؛ موقف تعبيري في الكلام','ئاماژەی زمانی؛ هەڵوێستی دەربڕین','verbal gesture; expressive stance in language','ژست زبانی؛ حالت بیانی','gesta zimanî; helwesta gotinê','gest językowy','gest lingvistic','речевая жестика; языковый жест','gjest gjuhësor','dilsel jest; söylemsel tavır'),
    examples: [
      ex('Der Sprachgestus der neuen Richtlinie wirkt partnerschaftlich, obwohl sie faktisch sehr strenge Kontrollen einführt.', meaning('تبدو إيماءة اللغة في التوجيه الجديد تشاركية، رغم أنه يفرض فعلياً رقابات صارمة جداً.','ئاماژەی زمانی ڕێنمایی نوێیەکە هاوبەشیی دەردەکەوێت، هەرچەندە بە کردەوە کۆنتڕۆڵی زۆر توند دەهێنێتە ناوەوە.','The verbal stance of the new guideline seems collaborative, although it in fact introduces very strict controls.','ژست زبانی دستورالعمل جدید مشارکتی به نظر می‌رسد، هرچند عملاً کنترل‌های بسیار سخت‌گیرانه وارد می‌کند.','Gesta zimanî ya rêbernameya nû wek hevkarî xuya dike, herçend bi rastî kontrolên pir tund tîne.','Gest językowy nowej wytycznej wydaje się partnerski, choć faktycznie wprowadza bardzo surowe kontrole.','Gestul lingvistic al noii directive pare partenerial, deși introduce de fapt controale foarte stricte.','Языковой жест новой директивы выглядит партнерским, хотя фактически вводит очень строгий контроль.','Gjesti gjuhësor i udhëzimit të ri duket bashkëpunues, megjithëse faktikisht vendos kontrolle shumë të rrepta.','Yeni yönergenin dilsel tavrı işbirlikçi görünür, oysa fiilen çok sıkı kontroller getirir.')),
      ex('Im Gedicht ersetzt ein fragender Sprachgestus die frühere Gewissheit des lyrischen Ichs.', meaning('في القصيدة تحل إيماءة لغوية متسائلة محل يقين الذات الشعرية السابق.','لە شیعرەکەدا ئاماژەی زمانیی پرسیارئامێز شوێنی دڵنیایی پێشووی منی شیعری دەگرێتەوە.','In the poem, a questioning verbal gesture replaces the earlier certainty of the lyrical self.','در شعر، ژست زبانی پرسشگر جای یقین پیشین منِ شاعرانه را می‌گیرد.','Di helbestê de gesteke zimanî ya pirsiyarî şûna baweriya berê ya ezê lîrîk digire.','W wierszu pytający gest językowy zastępuje wcześniejszą pewność podmiotu lirycznego.','În poem, un gest lingvistic interogativ înlocuiește certitudinea anterioară a eului liric.','В стихотворении вопрошающий речевой жест заменяет прежнюю уверенность лирического я.','Në poezi, një gjest gjuhësor pyetës zëvendëson sigurinë e mëparshme të unit lirik.','Şiirde sorgulayıcı bir dilsel jest, lirik benliğin önceki kesinliğinin yerini alır.'))
    ]
  }),
  entry({
    word: 'die Sprachgewalt', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Sprach-ge-walt',
    topics: ['culture-and-media','business-communication','advanced-analysis'], usageLabels: ['formal','written','academic','analysis'],
    grammarNotes: ['feminine abstract noun; normally used in singular'],
    collocations: [{ text: 'literarische Sprachgewalt', meaning: 'literary verbal power' }],
    meanings: meaning('قوة لغوية؛ سلطان البيان','هێزی زمان؛ توانای دەربڕینی بەهێز','verbal power; force of language','قدرت زبانی؛ نیروی بیان','hêza ziman; hêza gotinê','siła języka','forță a limbajului','сила языка; речевая мощь','fuqi gjuhësore','dil gücü; ifade kudreti'),
    examples: [
      ex('Die Sprachgewalt der Vorstandserklärung konnte nicht verdecken, dass konkrete Zusagen fehlten.', meaning('لم تستطع قوة البيان في تصريح مجلس الإدارة أن تخفي غياب تعهدات ملموسة.','هێزی زمانی ڕاگەیاندنی ئەنجومەنی بەڕێوەبەری نەیتوانی ئەوە داپۆشێت کە بەڵێنی دیاریکراو نییە.','The verbal force of the board statement could not conceal the lack of concrete commitments.','قدرت زبانی بیانیه هیئت‌مدیره نتوانست نبود تعهدهای مشخص را پنهان کند.','Hêza zimanî ya daxuyaniya desteya rêveberiyê nikarî veşêre ku sozên zehmet tune ne.','Siła języka oświadczenia zarządu nie mogła ukryć braku konkretnych zobowiązań.','Forța limbajului declarației consiliului nu a putut ascunde lipsa angajamentelor concrete.','Речевая мощь заявления правления не смогла скрыть отсутствие конкретных обязательств.','Fuqia gjuhësore e deklaratës së bordit nuk mundi të fshihte mungesën e zotimeve konkrete.','Yönetim kurulu açıklamasının dil gücü, somut taahhütlerin eksikliğini gizleyemedi.')),
      ex('Die Sprachgewalt des Romans entsteht aus knappen Sätzen, nicht aus rhetorischem Überschwang.', meaning('تنشأ القوة اللغوية للرواية من جمل مقتضبة لا من فيض بلاغي.','هێزی زمانی ڕۆمانەکە لە ڕستەی کورتەوە دروست دەبێت، نەک لە زیاده‌ڕەوی ڕەوانبێژی.','The novel’s verbal power arises from terse sentences, not from rhetorical excess.','قدرت زبانی رمان از جمله‌های کوتاه و فشرده می‌آید، نه از افراط بلاغی.','Hêza zimanî ya romanê ji hevokên kurt çêdibe, ne ji zêdebûna retorîkî.','Siła języka powieści bierze się ze zwięzłych zdań, a nie z retorycznego nadmiaru.','Forța limbajului romanului provine din propoziții concise, nu din exces retoric.','Сила языка романа возникает из кратких фраз, а не из риторического изобилия.','Fuqia gjuhësore e romanit buron nga fjali të shkurtra, jo nga tepria retorike.','Romanın dil gücü retorik taşkınlıktan değil, kısa ve yoğun cümlelerden doğar.'))
    ]
  }),
  entry({
    word: 'sprachmächtig', partOfSpeech: 'Adjective', syllableBreak: 'sprach-mäch-tig',
    topics: ['culture-and-media','business-communication','education-and-training'], usageLabels: ['formal','written','academic','advanced'],
    collocations: [{ text: 'eine sprachmächtige Rede', meaning: 'a linguistically powerful speech' }],
    meanings: meaning('قوي التعبير؛ متمكن لغوياً','بەهێز لە زماندا؛ زمانتوانا','linguistically powerful; eloquent','زبان‌آور؛ نیرومند در بیان','bi hêza zimanî; axaftvanê xurt','potężny językowo; elokwentny','puternic expresiv; elocvent','владеющий мощным языком; красноречивый','i fuqishëm në gjuhë; elokuent','dilsel olarak güçlü; etkili konuşan'),
    examples: [
      ex('Die sprachmächtige Präsentation überzeugte viele, doch erst die belastbaren Zahlen machten den Business Case tragfähig.', meaning('أقنع العرض القوي لغوياً كثيرين، لكن الأرقام الموثوقة وحدها جعلت دراسة الجدوى قابلة للدفاع.','پێشکەشکردنی زمانتوانا زۆر کەسی قایل کرد، بەڵام تەنها ژمارە پشتپێبەستووەکان business case ـەکەیان پایەدار کرد.','The linguistically powerful presentation convinced many, but only the robust figures made the business case viable.','ارائه زبان‌آورانه بسیاری را قانع کرد، اما فقط اعداد قابل اتکا طرح تجاری را قابل دفاع ساخت.','Pêşkêşkirina bi hêza zimanî gelek kes qanih kir, lê tenê hejmarên bawerbar business case kirin domdar.','Potężna językowo prezentacja przekonała wielu, lecz dopiero solidne liczby uczyniły business case wiarygodnym.','Prezentarea puternic expresivă i-a convins pe mulți, dar doar cifrele solide au făcut cazul de business sustenabil.','Яркая и сильная презентация убедила многих, но только надежные цифры сделали бизнес-кейс состоятельным.','Prezantimi i fuqishëm gjuhësor bindi shumë veta, por vetëm shifrat e besueshme e bënë rastin e biznesit të qëndrueshëm.','Dilsel olarak güçlü sunum birçok kişiyi ikna etti, ancak iş gerekçesini sürdürülebilir kılan sağlam rakamlar oldu.')),
      ex('Die Autorin ist sprachmächtig, ohne ihre Figuren durch eine einheitliche Kunstsprache zu überformen.', meaning('الكاتبة قوية التعبير من دون أن تطمس شخصياتها بلغة فنية موحدة.','نووسەرەکە زمانتوانایە، بەبێ ئەوەی کارەکتەرەکانی بە زمانێکی هونەریی یەکسان بپۆشێت.','The author is linguistically powerful without reshaping her characters through a uniform artificial language.','نویسنده زبان‌آور است، بدون آن‌که شخصیت‌هایش را با زبانی هنری و یکدست دگرگون کند.','Nivîskar bi hêza zimanî ye bê ku kesayetên xwe bi zimanekî hunerî yê yekgirtî ji nû ve biafirîne.','Autorka jest potężna językowo, nie przekształcając postaci przez jednolity język artystyczny.','Autoarea este puternică expresiv fără să își supramodeleze personajele printr-un limbaj artistic uniform.','Авторка обладает мощным языком, не перекраивая персонажей единой искусственной речью.','Autorja është e fuqishme në gjuhë pa i riformësuar personazhet përmes një gjuhe artistike të njëtrajtshme.','Yazar dilsel olarak güçlüdür, ancak karakterlerini tek tip yapay bir edebi dille yeniden şekillendirmez.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 075', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

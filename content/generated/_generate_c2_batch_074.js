const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '074';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['der Sog','das Spannungsfeld','speien','sperrig','das Spezifikum','spitzfindig'];

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
    word: 'der Sog', partOfSpeech: 'Noun', article: 'der', plural: 'Sogwirkungen', syllableBreak: 'Sog',
    topics: ['advanced-analysis','business-communication','culture-and-media'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'in den Sog geraten', meaning: 'to get drawn into the pull of something' }],
    meanings: meaning('جذب قوي؛ تيار يسحب','ڕاکێشانی بەهێز؛ هێزی کێشان','pull; suction; powerful draw','کشش؛ نیروی مکنده یا جذب‌کننده','kişandina xurt; hêza kişandinê','ciąg; siła przyciągania','atracție puternică; efect de aspirație','тяга; затягивающая сила','tërheqje e fuqishme','çekim; içine alan güç'),
    examples: [
      ex('Nach dem ersten Ausfall geriet das gesamte Supportteam in den Sog immer neuer Eskalationen.', meaning('بعد العطل الأول انجرف فريق الدعم كله في دوامة تصعيدات متجددة.','دوای یەکەم وەستان، تەواوی تیمی پشتگیری کەوتە ناو ڕاکێشانی هەموو جارێک escalationی نوێ.','After the first outage, the entire support team was drawn into the pull of ever new escalations.','پس از اولین اختلال، کل تیم پشتیبانی در کشش تشدیدهای پیاپی گرفتار شد.','Piştî qutbûna yekem, tevahiya tîma piştgiriyê ket bin kişandina escalationên her tim nû.','Po pierwszej awarii cały zespół wsparcia wpadł w wir kolejnych eskalacji.','După prima cădere, întreaga echipă de suport a fost atrasă în vârtejul unor escaladări mereu noi.','После первого сбоя вся команда поддержки оказалась втянута в поток новых эскалаций.','Pas ndërprerjes së parë, i gjithë ekipi i suportit u tërhoq në vorbullën e eskalimeve të reja.','İlk kesintiden sonra tüm destek ekibi sürekli yeni eskalasyonların çekimine kapıldı.')),
      ex('Der Sog der Stadt wird im Roman nicht romantisiert, sondern als Mischung aus Chance und Selbstverlust beschrieben.', meaning('لا تُرومنس جاذبية المدينة في الرواية، بل تُوصف كمزيج من الفرصة وفقدان الذات.','ڕاکێشانی شار لە ڕۆمانەکەدا ڕۆمانسی ناکرێت، بەڵکو وەک تێکەڵەی هەل و لەدەستدانی خۆ وەسف دەکرێت.','The pull of the city is not romanticized in the novel, but described as a mixture of opportunity and self-loss.','کشش شهر در رمان رمانتیک نمی‌شود، بلکه به‌صورت آمیزه‌ای از فرصت و ازخودبیگانگی توصیف می‌شود.','Kişandina bajêr di romanê de nayê romantîzekirin, lê wek tevliheviya derfet û xwe-windakirinê tê vegotin.','Ciąg miasta nie jest w powieści romantyzowany, lecz opisany jako mieszanina szansy i utraty siebie.','Atracția orașului nu este romantizată în roman, ci descrisă ca un amestec de șansă și pierdere de sine.','Тяга города в романе не романтизируется, а описывается как смесь возможности и утраты себя.','Tërheqja e qytetit në roman nuk romantizohet, por përshkruhet si përzierje mundësie dhe humbjeje të vetes.','Şehrin çekimi romanda romantikleştirilmez; fırsat ile kendini kaybetmenin karışımı olarak anlatılır.'))
    ]
  }),
  entry({
    word: 'das Spannungsfeld', partOfSpeech: 'Noun', article: 'das', plural: 'Spannungsfelder', syllableBreak: 'Span-nungs-feld',
    topics: ['management-and-leadership','advanced-analysis','business-communication'], usageLabels: ['formal','written','business','analysis'],
    collocations: [{ text: 'im Spannungsfeld zwischen zwei Zielen stehen', meaning: 'to stand in the field of tension between two goals' }],
    meanings: meaning('مجال توتر؛ تعارض بين أهداف','مەیدانی گرژی؛ نێوان دوو ئامانجی دژ','field of tension; area of conflicting demands','حوزه تنش؛ میدان تعارض اهداف','qada tengaviyê; navbera daxwazên dijber','obszar napięcia; pole konfliktu','câmp de tensiune; zonă de conflict','поле напряжения; зона конфликта','fushë tensioni; zonë konflikti','gerilim alanı; çatışan talepler alanı'),
    examples: [
      ex('Das Produktteam arbeitet im Spannungsfeld zwischen schneller Auslieferung, Datenschutz und langfristiger Wartbarkeit.', meaning('يعمل فريق المنتج في مجال توتر بين التسليم السريع وحماية البيانات وقابلية الصيانة الطويلة الأمد.','تیمی بەرهەم لە مەیدانی گرژی نێوان گەیاندنی خێرا، پاراستنی داتا و چاکراوی درێژخایەندا کار دەکات.','The product team works in the field of tension between fast delivery, data protection, and long-term maintainability.','تیم محصول در میدان تنش میان تحویل سریع، حفاظت از داده و نگهداشت‌پذیری بلندمدت کار می‌کند.','Tîma berhemê di qada tengaviyê de di navbera radestkirina bilez, parastina daneyan û parastinbarîya demdirêj de dixebite.','Zespół produktowy pracuje w polu napięcia między szybkim dostarczaniem, ochroną danych i długoterminową utrzymywalnością.','Echipa de produs lucrează în câmpul de tensiune dintre livrarea rapidă, protecția datelor și mentenabilitatea pe termen lung.','Продуктовая команда работает в поле напряжения между быстрой поставкой, защитой данных и долгосрочной сопровождаемостью.','Ekipi i produktit punon në fushën e tensionit mes dorëzimit të shpejtë, mbrojtjes së të dhënave dhe mirëmbajtjes afatgjatë.','Ürün ekibi hızlı teslimat, veri koruma ve uzun vadeli sürdürülebilirlik arasındaki gerilim alanında çalışıyor.')),
      ex('Der Essay verortet die Figur im Spannungsfeld von persönlicher Schuld und historischer Gewalt.', meaning('يضع المقال الشخصية في مجال توتر بين الذنب الشخصي والعنف التاريخي.','وتارەکە کارەکتەرەکە لە مەیدانی گرژی نێوان تاوانی کەسی و توندوتیژیی مێژوویی دادەنێت.','The essay places the character in the field of tension between personal guilt and historical violence.','مقاله شخصیت را در حوزه تنش میان گناه شخصی و خشونت تاریخی قرار می‌دهد.','Gotar kesayetê di qada tengaviyê de di navbera sûcê kesane û tundûtûjiya dîrokî de cih dike.','Esej umieszcza postać w polu napięcia między osobistą winą a historyczną przemocą.','Eseul plasează personajul în câmpul de tensiune dintre vina personală și violența istorică.','Эссе помещает персонажа в поле напряжения между личной виной и историческим насилием.','Eseja e vendos personazhin në fushën e tensionit mes fajit personal dhe dhunës historike.','Deneme karakteri kişisel suçluluk ile tarihsel şiddet arasındaki gerilim alanına yerleştirir.'))
    ]
  }),
  entry({
    word: 'speien', partOfSpeech: 'Verb', infinitive: 'speien', syllableBreak: 'spei-en',
    topics: ['culture-and-media','environment-and-sustainability','advanced-analysis'], usageLabels: ['formal','written','advanced','sensitive'],
    grammarNotes: ['elevated or literary verb; can mean to spit, vomit, or emit forcefully'],
    collocations: [{ text: 'Rauch speien', meaning: 'to spew smoke' }],
    meanings: meaning('يقذف؛ ينفث؛ يتقيأ','دەرهاویشتن؛ تفکردن؛ ڕشانەوە','to spew; to spit; to vomit','بیرون پاشیدن؛ تف کردن؛ قی کردن','derxistin; tif kirin; vereşîn','pluć; wyrzucać z siebie','a scuipa; a vărsa; a arunca afară','извергать; плевать; рвать','nxjerr vrullshëm; pështyj; vjell','püskürtmek; tükürmek; kusmak'),
    examples: [
      ex('Die alte Anlage speite dunklen Rauch, obwohl der Betreiber kurz zuvor neue Filter versprochen hatte.', meaning('كانت المنشأة القديمة تنفث دخاناً أسود رغم أن المشغل وعد قبل قليل بفلاتر جديدة.','دامەزراوە کۆنەکە دووکەڵی ڕەشی دەرهاویشت، هەرچەندە بەڕێوەبەرەکە کەمێک پێشتر بەڵێنی فلتەری نوێی دابوو.','The old plant spewed dark smoke, although the operator had shortly before promised new filters.','تأسیسات قدیمی دود تیره بیرون می‌داد، با اینکه بهره‌بردار کمی قبل وعده فیلترهای جدید داده بود.','Dezgeha kevn dûmana reş derdida, herçend rêveberê wê demek kurt berê fîlterên nû soz dabû.','Stara instalacja wyrzucała ciemny dym, choć operator krótko wcześniej obiecał nowe filtry.','Vechea instalație scuipa fum negru, deși operatorul promisese cu puțin timp înainte filtre noi.','Старая установка извергала темный дым, хотя оператор незадолго до этого обещал новые фильтры.','Impianti i vjetër nxirrte tym të errët, megjithëse operatori pak më parë kishte premtuar filtra të rinj.','Eski tesis koyu duman püskürtüyordu, oysa işletmeci kısa süre önce yeni filtreler vaat etmişti.')),
      ex('In der Szene speit der Drache Feuer, doch die eigentliche Bedrohung liegt in der Ruhe des Königs.', meaning('في المشهد ينفث التنين ناراً، لكن التهديد الحقيقي يكمن في هدوء الملك.','لە دیمەنەکەدا ئەژدیها ئاگر دەرهاویژێت، بەڵام هەڕەشەی ڕاستەقینە لە ئارامی پاشادا هەیە.','In the scene, the dragon spews fire, but the real threat lies in the king’s calm.','در صحنه، اژدها آتش بیرون می‌پاشد، اما تهدید واقعی در آرامش پادشاه است.','Di dîmenê de ejder agir derdide, lê tehdîda rastîn di aramiya padîşah de ye.','W scenie smok zieje ogniem, lecz prawdziwe zagrożenie tkwi w spokoju króla.','În scenă, dragonul scuipă foc, dar adevărata amenințare stă în calmul regelui.','В сцене дракон извергает огонь, но настоящая угроза заключена в спокойствии короля.','Në skenë dragoi nxjerr zjarr, por kërcënimi i vërtetë qëndron te qetësia e mbretit.','Sahnede ejderha ateş püskürtür, ancak asıl tehdit kralın sakinliğindedir.'))
    ]
  }),
  entry({
    word: 'sperrig', partOfSpeech: 'Adjective', syllableBreak: 'sper-rig',
    topics: ['business-communication','technology-and-it','culture-and-media'], usageLabels: ['formal','written','analysis','advanced'],
    collocations: [{ text: 'sperrig zu bedienen', meaning: 'cumbersome to use' }],
    meanings: meaning('صعب التعامل؛ غير سلس؛ ضخم مزعج','قورس بە کارهێنان؛ نەھەمواڕ','cumbersome; unwieldy; difficult to access','دست‌وپاگیر؛ دشوار و ناهنجار','girankirin; neasayî û dijwar','toporny; nieporęczny','greoi; dificil de utilizat','громоздкий; неудобный','i rëndë për përdorim; i vështirë','hantal; kullanımı zor'),
    examples: [
      ex('Das Formular ist so sperrig, dass selbst erfahrene Sachbearbeiter regelmäßig Rückfragen stellen müssen.', meaning('النموذج معقد إلى درجة أن حتى الموظفين ذوي الخبرة يضطرون بانتظام إلى طرح أسئلة توضيحية.','فۆرمەکە ئەوەندە قورسە کە تەنانەت کارمەندانی ئەزموونداریش بەردەوام ناچار دەبن پرسیاری ڕوونکردنەوە بکەن.','The form is so cumbersome that even experienced caseworkers regularly have to ask follow-up questions.','فرم آن‌قدر دست‌وپاگیر است که حتی کارمندان باتجربه هم مرتب باید سؤال تکمیلی بپرسند.','Form ewqas giran e ku tewra karmendên bi ezmûn jî bi rêkûpêk neçar dibin pirsên şirovekirinê bikin.','Formularz jest tak toporny, że nawet doświadczeni urzędnicy regularnie muszą zadawać pytania uzupełniające.','Formularul este atât de greoi încât chiar și funcționarii experimentați trebuie să pună regulat întrebări suplimentare.','Форма настолько громоздкая, что даже опытным специалистам регулярно приходится задавать уточняющие вопросы.','Formulari është aq i vështirë sa edhe nëpunësit me përvojë rregullisht duhet të bëjnë pyetje sqaruese.','Form o kadar hantal ki deneyimli uzmanlar bile düzenli olarak ek sorular sormak zorunda kalıyor.')),
      ex('Der Roman beginnt sperrig, gewinnt aber an Kraft, sobald die verschachtelte Struktur erkennbar wird.', meaning('تبدأ الرواية بصعوبة، لكنها تكتسب قوة عندما تصبح بنيتها المتداخلة واضحة.','ڕۆمانەکە بە شێوەیەکی قورس دەست پێدەکات، بەڵام کاتێک پێکهاتەی ئاڵۆزی دیار دەبێت هێز وەردەگرێت.','The novel begins in a cumbersome way, but gains force once the nested structure becomes recognizable.','رمان شروعی دشوار و ناهنجار دارد، اما وقتی ساختار تودرتوی آن روشن می‌شود قدرت می‌گیرد.','Roman bi awayekî dijwar dest pê dike, lê gava avahiya tevlihev xuya dibe hêz werdigire.','Powieść zaczyna się topornie, ale zyskuje siłę, gdy staje się rozpoznawalna jej zagnieżdżona struktura.','Romanul începe greoi, dar câștigă forță odată ce structura încâlcită devine recognoscibilă.','Роман начинается тяжеловесно, но набирает силу, когда становится видна его вложенная структура.','Romani fillon rëndë, por fiton forcë sapo struktura e ndërthurur bëhet e dallueshme.','Roman hantal başlar, ancak iç içe yapısı fark edilir hale gelince güç kazanır.'))
    ]
  }),
  entry({
    word: 'das Spezifikum', partOfSpeech: 'Noun', article: 'das', plural: 'Spezifika', syllableBreak: 'Spe-zi-fi-kum',
    topics: ['advanced-analysis','business-communication','education-and-training'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'das Spezifikum eines Marktes', meaning: 'the specific characteristic of a market' }],
    meanings: meaning('خاصية محددة؛ سمة خاصة','تایبەتمەندیی دیاریکراو','specific feature; distinctive characteristic','ویژگی خاص؛ مشخصه متمایز','taybetmendiya taybet','cecha swoista; specyfika','specific; trăsătură distinctivă','специфика; особенность','veçori specifike','özgül özellik; spesifik nitelik'),
    examples: [
      ex('Das Spezifikum dieses Marktes liegt darin, dass Vertrauen wichtiger ist als kurzfristige Preisvorteile.', meaning('تكمن خصوصية هذا السوق في أن الثقة أهم من مزايا السعر القصيرة الأجل.','تایبەتمەندیی ئەم بازاڕە لەوەدایە کە متمانە گرنگترە لە سوودی نرخی کورتخایەن.','The specific feature of this market is that trust is more important than short-term price advantages.','ویژگی خاص این بازار این است که اعتماد از مزیت‌های قیمتی کوتاه‌مدت مهم‌تر است.','Taybetmendiya vî bazarî ew e ku bawerî ji avantajên bihayê yên demkurt girîngtir e.','Specyfika tego rynku polega na tym, że zaufanie jest ważniejsze niż krótkoterminowe korzyści cenowe.','Specificul acestei piețe constă în faptul că încrederea este mai importantă decât avantajele de preț pe termen scurt.','Специфика этого рынка в том, что доверие важнее краткосрочных ценовых преимуществ.','Veçoria e këtij tregu qëndron në faktin se besimi është më i rëndësishëm se avantazhet afatshkurtra të çmimit.','Bu pazarın özgül özelliği, güvenin kısa vadeli fiyat avantajlarından daha önemli olmasıdır.')),
      ex('Das Spezifikum der Erzählung ist die ständige Verschiebung zwischen Bericht, Erinnerung und Selbstrechtfertigung.', meaning('تكمن خصوصية السرد في الانتقال المستمر بين التقرير والذكرى وتبرير الذات.','تایبەتمەندیی گێڕانەوەکە گواستنەوەی بەردەوامە لە نێوان ڕاپۆرت، بیرەوەری و خۆڕەواکردن.','The distinctive feature of the narrative is the constant shifting between report, memory, and self-justification.','ویژگی خاص روایت جابه‌جایی دائمی میان گزارش، خاطره و خودتوجیهی است.','Taybetmendiya vegotinê guherîna domdar e di navbera rapor, bîranîn û xwe-rewakirinê de.','Swoistą cechą narracji jest ciągłe przesuwanie się między relacją, wspomnieniem i samousprawiedliwieniem.','Specificul narațiunii este deplasarea continuă între relatare, amintire și autojustificare.','Специфика повествования состоит в постоянном смещении между отчетом, воспоминанием и самооправданием.','Veçoria e rrëfimit është zhvendosja e vazhdueshme mes raportimit, kujtesës dhe vetëjustifikimit.','Anlatının özgül niteliği rapor, anı ve kendini haklı çıkarma arasında sürekli kaymasıdır.'))
    ]
  }),
  entry({
    word: 'spitzfindig', partOfSpeech: 'Adjective', syllableBreak: 'spitz-fin-dig',
    topics: ['law-and-compliance','business-communication','advanced-analysis'], usageLabels: ['formal','written','sensitive','advanced'],
    collocations: [{ text: 'eine spitzfindige Auslegung', meaning: 'a hairsplitting interpretation' }],
    meanings: meaning('متفذلك؛ دقيق بشكل مبالغ فيه','وردەگیر بە شێوەی زیادە؛ دەستەواژەباز','hairsplitting; overly subtle','موشکافانه افراطی؛ مته به خشخاش‌گذار','pir hûrgulî; bi şêweya zêde zîrekane','drobiazgowy; sofistyczny','sofistic; excesiv de subtil','казуистический; придирчиво тонкий','tepër hollësishëm; sofistik','kılı kırk yaran; sofistike biçimde ince'),
    examples: [
      ex('Die spitzfindige Auslegung der Klausel verschaffte kurzfristig Vorteile, beschädigte aber die Beziehung zum Kunden.', meaning('وفّر التفسير المتفذلك للبند مزايا قصيرة الأجل، لكنه أضر بالعلاقة مع العميل.','لێکدانەوەی وردەگیری ماددەکە سوودی کورتخایەنی هێنا، بەڵام پەیوەندی لەگەڵ کڕیارەکەی زیانمند کرد.','The hairsplitting interpretation of the clause created short-term advantages, but damaged the relationship with the customer.','تفسیر مته‌به‌خشخاش‌گذار بند قرارداد مزیت کوتاه‌مدت ایجاد کرد، اما رابطه با مشتری را آسیب زد.','Şîroveya pir hûrgulî ya bendê avantajên demkurt çêkir, lê têkiliya bi xerîdar re zirardar kir.','Drobiazgowa wykładnia klauzuli dała krótkoterminowe korzyści, ale zaszkodziła relacji z klientem.','Interpretarea sofistică a clauzei a adus avantaje pe termen scurt, dar a afectat relația cu clientul.','Казуистическое толкование пункта дало краткосрочные преимущества, но повредило отношениям с клиентом.','Interpretimi tepër hollësishëm i klauzolës solli avantazhe afatshkurtra, por dëmtoi marrëdhënien me klientin.','Maddenin kılı kırk yaran yorumu kısa vadeli avantaj sağladı, ancak müşteriyle ilişkiye zarar verdi.')),
      ex('Der Einwand ist nicht bloß spitzfindig; er zeigt, dass die scheinbar einfache Definition zwei Fälle ausschließt.', meaning('ليس الاعتراض مجرد تدقيق متفذلك؛ فهو يبيّن أن التعريف البسيط ظاهرياً يستبعد حالتين.','ناڕەزاییەکە تەنها وردەگیری زیادە نییە؛ پیشان دەدات کە پێناسەی بە ڕواڵەت سادە دوو کەیس دەردەکات.','The objection is not merely hairsplitting; it shows that the seemingly simple definition excludes two cases.','ایراد فقط موشکافی افراطی نیست؛ نشان می‌دهد تعریف ظاهراً ساده دو مورد را کنار می‌گذارد.','Îtiraz ne tenê hûrgulîya zêde ye; nîşan dide ku pênaseya xuya sade du rewş derdixe.','Zastrzeżenie nie jest tylko drobiazgowe; pokazuje, że pozornie prosta definicja wyklucza dwa przypadki.','Obiecția nu este doar sofistică; ea arată că definiția aparent simplă exclude două cazuri.','Возражение не просто придирчивое; оно показывает, что кажущееся простым определение исключает два случая.','Kundërshtimi nuk është thjesht kılı kırk yarma; ai tregon se përkufizimi në dukje i thjeshtë përjashton dy raste.','İtiraz yalnızca kılı kırk yaran bir ayrıntı değil; görünüşte basit tanımın iki durumu dışladığını gösteriyor.'))
    ]
  })
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 074', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

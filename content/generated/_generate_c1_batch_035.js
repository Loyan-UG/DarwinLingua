const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const levelOrder = ['B2','B1','A2','A1','C1','C2'];
const taxonomyPath = path.join(root, 'content/taxonomy/darwinlingua-taxonomy-v1.json');
const generatedDir = path.join(root, 'content/generated');
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const labelMap = new Map((taxonomy.labels || []).map(l => [l.key, l]));

function splitTokens(text) {
  return text.split(',').map(s => s.trim()).filter(Boolean);
}
function sourceText(tokens) { return tokens.join(', '); }

let sourceLevel = null, sourceFile = null, tokens = null;
for (const lvl of levelOrder) {
  const file = path.join(root, 'content', `${lvl}.txt`);
  const list = splitTokens(fs.readFileSync(file, 'utf8'));
  if (list.length) { sourceLevel = lvl; sourceFile = file; tokens = list; break; }
}
if (!sourceLevel) throw new Error('No source tokens remain.');
const batchWords = tokens.slice(0, 6);
if (sourceLevel !== 'C1') throw new Error(`Expected current source C1, got ${sourceLevel}: ${batchWords.join(', ')}`);
const expected = ['einbehalten','eindringen','einfahren','die Einflussgröße','einfressen','eingehen'];
if (batchWords.join('|') !== expected.join('|')) throw new Error(`Unexpected first tokens: ${batchWords.join(', ')}`);

const existing = fs.readdirSync(generatedDir)
  .map(n => /^de-c1-generated-batch-(\d+)\.json$/i.exec(n))
  .filter(Boolean)
  .map(m => Number(m[1]));
const next = Math.max(0, ...existing) + 1;
const num = String(next).padStart(3, '0');
const packageId = `de-c1-generated-batch-${num}`;
const outPath = path.join(generatedDir, `${packageId}.json`);

function meaning(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) {
  const vals = {ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr};
  return langs.map(language => ({ language, text: vals[language] }));
}
function ex(baseText, ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) {
  return { baseText, translations: meaning(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) };
}
function labelRefs(keys) {
  return keys.map(k => {
    const src = labelMap.get(k);
    if (!src) throw new Error(`Missing taxonomy label ${k}`);
    return JSON.parse(JSON.stringify(src));
  });
}

const entries = [
  {
    word: 'einbehalten', language: 'de', cefrLevel: 'C1', partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'einbehalten', pronunciationIpa: null, syllableBreak: 'ein-be-hal-ten',
    topics: ['finance-and-accounting','documents-and-administration','law-and-compliance'], usageLabels: ['formal','business','written','advanced'], contextLabels: [], grammarNotes: ['separable verb: behält ein, behielt ein, hat einbehalten'],
    collocations: [{ text: 'einen Betrag einbehalten', meaning: 'to withhold an amount' }, { text: 'die Kaution einbehalten', meaning: 'to retain the deposit' }], wordFamilies: [{ lemma: 'der Einbehalt', relationLabel: 'noun', note: null }], relations: [],
    meanings: meaning('يحتجز؛ يستبقي؛ يقتطع', 'ڕادەگرێت؛ دەهێڵێتەوە؛ دەبڕێت', 'to retain; to withhold; to deduct', 'نگه داشتن؛ کسر کردن؛ نزد خود نگه داشتن', 'girtin; ragirtin; jêbirîn', 'zatrzymać; potrącić; wstrzymać', 'a reține; a opri; a deduce', 'удерживать; оставлять у себя; вычитать', 'të mbash; të ndalosh; të zbresësh', 'alıkoymak; kesmek; tutmak'),
    examples: [
      ex('Die Buchhaltung behält einen Teil der Zahlung ein, bis die fehlenden Nachweise vorliegen.', 'تحتجز المحاسبة جزءًا من الدفعة إلى أن تتوفر المستندات الناقصة.', 'بەشی ژمێریاری بەشێک لە پارەدانەکە ڕادەگرێت تا بەڵگەنامە کەمەکان ئامادەبن.', 'Accounting withholds part of the payment until the missing documents are available.', 'بخش حسابداری بخشی از پرداخت را نگه می‌دارد تا مدارک ناقص ارائه شوند.', 'Beşa hesabgiriyê beşek ji dayînê digire heta belgeyên kêm amade bin.', 'Księgowość wstrzymuje część płatności, aż brakujące dokumenty będą dostępne.', 'Contabilitatea reține o parte din plată până când sunt disponibile documentele lipsă.', 'Бухгалтерия удерживает часть платежа до представления недостающих документов.', 'Kontabiliteti mban një pjesë të pagesës derisa të paraqiten dokumentet që mungojnë.', 'Muhasebe, eksik belgeler sunulana kadar ödemenin bir kısmını alıkoyar.'),
      ex('Der Vermieter darf die Kaution nur einbehalten, wenn konkrete Schäden dokumentiert sind.', 'لا يجوز للمالك الاحتفاظ بالتأمين إلا إذا وُثقت أضرار محددة.', 'خاوەن ماڵ تەنها دەتوانێت بارمتەکە ڕابگرێت ئەگەر زیانە دیاریکراوەکان تۆمار کرابن.', 'The landlord may retain the deposit only if specific damages are documented.', 'صاحب‌خانه فقط زمانی می‌تواند ودیعه را نگه دارد که خسارت‌های مشخص ثبت شده باشند.', 'Xwediyê xanî tenê dikare depozîtoyê ragire heke zirarên taybet hatibin belgekirin.', 'Wynajmujący może zatrzymać kaucję tylko wtedy, gdy udokumentowano konkretne szkody.', 'Proprietarul poate reține garanția doar dacă sunt documentate daune concrete.', 'Арендодатель может удержать залог только при документально подтвержденном конкретном ущербе.', 'Qiradhënësi mund ta mbajë depozitën vetëm nëse janë dokumentuar dëme konkrete.', 'Ev sahibi depozitoyu yalnızca somut hasarlar belgelenmişse alıkoyabilir.')
    ]
  },
  {
    word: 'eindringen', language: 'de', cefrLevel: 'C1', partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'eindringen', pronunciationIpa: null, syllableBreak: 'ein-drin-gen',
    topics: ['technology-and-it','law-and-compliance','quality-and-risk'], usageLabels: ['technical','formal','written','advanced'], contextLabels: [], grammarNotes: ['inseparable-looking but separable verb in usage: dringt ein, drang ein, ist eingedrungen'],
    collocations: [{ text: 'in ein Netzwerk eindringen', meaning: 'to intrude into a network' }, { text: 'in ein Gebäude eindringen', meaning: 'to break into a building' }], wordFamilies: [{ lemma: 'das Eindringen', relationLabel: 'noun', note: null }], relations: [],
    meanings: meaning('يخترق؛ يتسلل؛ يقتحم', 'دەچێتە ناو؛ دەتەقێتە ناو؛ دەستدرێژی دەکاتە ناو', 'to penetrate; to intrude; to break in', 'نفوذ کردن؛ وارد شدن به زور؛ رخنه کردن', 'têketin; derbasbûn; bi zorê ketin', 'wtargnąć; przeniknąć; dostać się do środka', 'a pătrunde; a intra prin efracție; a se infiltra', 'проникать; вторгаться; взламывать', 'të depërtosh; të hysh me forcë; të futesh pa leje', 'sızmak; nüfuz etmek; zorla girmek'),
    examples: [
      ex('Unbekannte konnten über eine ungepatchte Schnittstelle in das interne Netzwerk eindringen.', 'تمكن مجهولون من اختراق الشبكة الداخلية عبر واجهة غير محدّثة.', 'کەسانی نەناسراو توانییان لە ڕێگەی ڕووکاری نوێنەکراوەوە بچنە ناو تۆڕی ناوخۆیی.', 'Unknown attackers were able to intrude into the internal network through an unpatched interface.', 'افراد ناشناس توانستند از طریق یک رابط به‌روزرسانی‌نشده به شبکه داخلی نفوذ کنند.', 'Kesên nenas karîn bi navgîniya navrûyeke bê-pêvekirin bikevin tora navxweyî.', 'Nieznane osoby mogły dostać się do sieci wewnętrznej przez niezałataną usługę.', 'Persoane necunoscute au putut pătrunde în rețeaua internă printr-o interfață neactualizată.', 'Неизвестные смогли проникнуть во внутреннюю сеть через необновленный интерфейс.', 'Persona të panjohur arritën të depërtojnë në rrjetin e brendshëm përmes një ndërfaqeje të papërditësuar.', 'Bilinmeyen kişiler, yamalanmamış bir arayüz üzerinden iç ağa sızabildi.'),
      ex('Durch den Riss drang Wasser in die Kellerwand ein.', 'تسرّب الماء عبر الشق إلى جدار القبو.', 'لە ڕێگەی درزەکەوە ئاو چووە ناو دیواری ژێرزەمینەکە.', 'Water penetrated the basement wall through the crack.', 'آب از طریق ترک به دیوار زیرزمین نفوذ کرد.', 'Av bi şikestê ket hundirê dîwarê jêrzemînê.', 'Przez pęknięcie woda wniknęła w ścianę piwnicy.', 'Prin fisură, apa a pătruns în peretele subsolului.', 'Через трещину вода проникла в стену подвала.', 'Nëpër çarje, uji depërtoi në murin e bodrumit.', 'Çatlaktan bodrum duvarına su sızdı.')
    ]
  },
  {
    word: 'einfahren', language: 'de', cefrLevel: 'C1', partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'einfahren', pronunciationIpa: null, syllableBreak: 'ein-fah-ren',
    topics: ['transport-and-travel','production-and-maintenance','finance-and-accounting'], usageLabels: ['technical','business','written','advanced'], contextLabels: [], grammarNotes: ['separable verb: fährt ein, fuhr ein, hat/ist eingefahren depending on meaning'],
    collocations: [{ text: 'Gewinne einfahren', meaning: 'to bring in profits' }, { text: 'eine Anlage einfahren', meaning: 'to run in a facility or machine' }], wordFamilies: [{ lemma: 'die Einfahrt', relationLabel: 'noun', note: null }], relations: [],
    meanings: meaning('يدخل بالسيارة؛ يحقق؛ يشغّل تدريجيًا', 'بە ئۆتۆمبێل دەچێتە ناو؛ بەدەستدەهێنێت؛ بەهێواشی دەخاتە کار', 'to drive in; to bring in; to run in', 'وارد شدن با وسیله نقلیه؛ به دست آوردن؛ راه‌اندازی تدریجی', 'bi erebeyê têketin; bidestxistin; hêdî hêdî xebitandin', 'wjechać; osiągnąć; docierać urządzenie', 'a intra cu vehiculul; a obține; a roda', 'въезжать; получать; обкатывать', 'të hysh me automjet; të arrish; të vësh gradualisht në punë', 'araçla girmek; elde etmek; alıştırarak çalıştırmak'),
    examples: [
      ex('Die neue Linie muss vor dem Serienbetrieb mehrere Tage eingefahren werden.', 'يجب تشغيل الخط الجديد تدريجيًا لعدة أيام قبل الإنتاج المتسلسل.', 'هێڵە نوێیەکە پێش بەرهەمهێنانی زنجیرەیی دەبێت چەند ڕۆژ بەهێواشی بخڕێتە کار.', 'The new line has to be run in for several days before series production starts.', 'خط جدید باید پیش از تولید سری چند روز به‌صورت تدریجی راه‌اندازی شود.', 'Rêza nû divê berî hilberîna rêzikî çend rojan were xebitandin.', 'Nowa linia musi być docierana przez kilka dni przed produkcją seryjną.', 'Noua linie trebuie rodaită câteva zile înainte de producția de serie.', 'Новую линию нужно несколько дней обкатывать перед серийным производством.', 'Linja e re duhet të vihet gradualisht në punë për disa ditë para prodhimit në seri.', 'Yeni hattın seri üretimden önce birkaç gün alıştırılarak çalıştırılması gerekir.'),
      ex('Das Unternehmen fuhr im letzten Quartal unerwartet hohe Gewinne ein.', 'حققت الشركة في الربع الأخير أرباحًا مرتفعة بشكل غير متوقع.', 'کۆمپانیاکە لە چارەکی ڕابردوودا قازانجی بەرزی چاوەڕواننەکراوی بەدەستهێنا.', 'The company brought in unexpectedly high profits last quarter.', 'شرکت در فصل گذشته سودهای غیرمنتظره بالایی به دست آورد.', 'Şîrketê di çaryeka borî de qezencên pir bilind û neçaverêkirî bidest xist.', 'Firma osiągnęła w ostatnim kwartale nieoczekiwanie wysokie zyski.', 'Compania a obținut profituri neașteptat de mari în trimestrul trecut.', 'В прошлом квартале компания получила неожиданно высокую прибыль.', 'Kompania arriti fitime papritur të larta në tremujorin e kaluar.', 'Şirket geçen çeyrekte beklenmedik ölçüde yüksek kâr elde etti.')
    ]
  },
  {
    word: 'die Einflussgröße', language: 'de', cefrLevel: 'C1', partOfSpeech: 'Noun', article: 'die', plural: 'Einflussgrößen', infinitive: null, pronunciationIpa: null, syllableBreak: 'Ein-fluss-grö-ße',
    topics: ['advanced-analysis','data-and-reporting','quality-and-risk'], usageLabels: ['academic','technical','analysis','advanced'], contextLabels: [], grammarNotes: ['feminine noun; plural: die Einflussgrößen'],
    collocations: [{ text: 'eine zentrale Einflussgröße', meaning: 'a central influencing factor' }, { text: 'relevante Einflussgrößen bestimmen', meaning: 'to determine relevant variables' }], wordFamilies: [{ lemma: 'der Einfluss', relationLabel: 'noun', note: null }], relations: [],
    meanings: meaning('عامل مؤثر؛ متغير مؤثر', 'هۆکاری کاریگەر؛ گۆڕاوەی کاریگەر', 'influencing factor; variable', 'عامل اثرگذار؛ متغیر مؤثر', 'faktora bandorker; guherbara bandorker', 'czynnik wpływu; zmienna wpływająca', 'factor de influență; variabilă relevantă', 'влияющий фактор; переменная', 'faktor ndikues; variabël', 'etki faktörü; değişken'),
    examples: [
      ex('In der Auswertung wurde die Lieferzeit als zentrale Einflussgröße identifiziert.', 'في التحليل تم تحديد وقت التسليم كعامل مؤثر أساسي.', 'لە شیکردنەوەکەدا کاتی گەیاندن وەک هۆکاری کاریگەری سەرەکی دیاریکرا.', 'In the analysis, delivery time was identified as a central influencing factor.', 'در تحلیل، زمان تحویل به عنوان عامل اثرگذار اصلی شناسایی شد.', 'Di analîzê de dema radestkirinê wek faktora sereke ya bandorker hate nasîn.', 'W analizie czas dostawy uznano za kluczowy czynnik wpływu.', 'În analiză, timpul de livrare a fost identificat ca factor central de influență.', 'В анализе срок поставки был определен как ключевой влияющий фактор.', 'Në analizë, koha e dorëzimit u identifikua si faktor kryesor ndikues.', 'Analizde teslimat süresi temel bir etki faktörü olarak belirlendi.'),
      ex('Temperatur und Luftfeuchtigkeit sind wichtige Einflussgrößen für die Materialprüfung.', 'درجة الحرارة ورطوبة الهواء عاملان مهمان في اختبار المواد.', 'پلەی گەرمی و شێی هەوا هۆکارە کاریگەرە گرنگەکانن بۆ پشکنینی ماددە.', 'Temperature and humidity are important variables in material testing.', 'دما و رطوبت هوا متغیرهای مهمی در آزمون مواد هستند.', 'Germahî û şilahiya hewayê ji bo ceribandina materyalê guherbarên girîng in.', 'Temperatura i wilgotność powietrza są ważnymi zmiennymi w badaniu materiałów.', 'Temperatura și umiditatea aerului sunt variabile importante pentru testarea materialelor.', 'Температура и влажность воздуха являются важными переменными при испытании материалов.', 'Temperatura dhe lagështia e ajrit janë faktorë të rëndësishëm në testimin e materialeve.', 'Sıcaklık ve hava nemi malzeme testi için önemli değişkenlerdir.')
    ]
  },
  {
    word: 'einfressen', language: 'de', cefrLevel: 'C1', partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'einfressen', pronunciationIpa: null, syllableBreak: 'ein-fres-sen',
    topics: ['production-and-maintenance','quality-and-risk','environment-and-sustainability'], usageLabels: ['technical','written','advanced'], contextLabels: [], grammarNotes: ['separable verb: frisst ein, fraß ein, hat sich eingefressen; often reflexive'],
    collocations: [{ text: 'sich in Metall einfressen', meaning: 'to corrode into metal' }, { text: 'sich tief einfressen', meaning: 'to eat deeply into something' }], wordFamilies: [{ lemma: 'fressen', relationLabel: 'verb', note: null }], relations: [],
    meanings: meaning('يتآكل داخل؛ يترسخ بعمق', 'دەخواتە ناو؛ قوڵ دەبێتەوە؛ جێگیر دەبێت', 'to eat into; to corrode; to become deeply embedded', 'خوردن و نفوذ کردن؛ خوردگی پیدا کردن؛ عمیقاً جا افتادن', 'xwarin û têketin; xurandin; kûr bicihbûn', 'wżerać się; korodować; głęboko się zakorzenić', 'a se roade în; a coroda; a se fixa adânc', 'въедаться; разъедать; глубоко укореняться', 'të gërryesh; të futesh thellë; të rrënjosesh', 'içine işlemek; aşındırmak; derine yerleşmek'),
    examples: [
      ex('Die Säure hatte sich bereits tief in die Metalloberfläche eingefressen.', 'كان الحمض قد تغلغل بالفعل بعمق في سطح المعدن.', 'ئاسیدەکە پێشتر قوڵ خواردبوویە ناو ڕووی کانزاکە.', 'The acid had already eaten deeply into the metal surface.', 'اسید پیش از این عمیقاً در سطح فلز خوردگی ایجاد کرده بود.', 'Asîtê jixwe kûr ketibû rûyê metalê.', 'Kwas wżarł się już głęboko w powierzchnię metalu.', 'Acidul se rosese deja adânc în suprafața metalică.', 'Кислота уже глубоко въелась в поверхность металла.', 'Acidi ishte futur tashmë thellë në sipërfaqen metalike.', 'Asit metal yüzeye çoktan derinlemesine işlemişti.'),
      ex('Die Sorge um den Arbeitsplatz fraß sich langsam in seinen Alltag ein.', 'تسلل القلق على الوظيفة تدريجيًا إلى حياته اليومية.', 'نیگەرانی لەسەر شوێنی کارەکەی بەهێواشی چووە ناو ژیانی ڕۆژانەی.', 'Concern about his job slowly ate its way into his everyday life.', 'نگرانی درباره شغلش به‌آرامی در زندگی روزمره‌اش جا باز کرد.', 'Xemgîniya derbarê karê wî hêdî hêdî ket nav jiyana wî ya rojane.', 'Obawa o miejsce pracy powoli wżerała się w jego codzienność.', 'Grija pentru locul de muncă i-a pătruns treptat în viața de zi cu zi.', 'Тревога за рабочее место постепенно проникала в его повседневную жизнь.', 'Shqetësimi për vendin e punës iu fut ngadalë në përditshmëri.', 'İşini kaybetme endişesi yavaş yavaş günlük hayatına işledi.')
    ]
  },
  {
    word: 'eingehen', language: 'de', cefrLevel: 'C1', partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'eingehen', pronunciationIpa: null, syllableBreak: 'ein-ge-hen',
    topics: ['contracts-and-negotiation','business-communication','documents-and-administration'], usageLabels: ['formal','business','written','advanced'], contextLabels: [], grammarNotes: ['separable verb: geht ein, ging ein, ist eingegangen; multiple meanings depending on context'],
    collocations: [{ text: 'auf Einwände eingehen', meaning: 'to address objections' }, { text: 'bei der Behörde eingehen', meaning: 'to be received by the authority' }], wordFamilies: [{ lemma: 'der Eingang', relationLabel: 'noun', note: 'receipt or entrance depending on context' }], relations: [],
    meanings: meaning('يتناول؛ يرد على؛ يصل؛ يدخل في', 'باس دەکات؛ وەڵام دەداتەوە؛ دەگات؛ دەچێتە ناو', 'to address; to respond to; to be received; to enter into', 'پرداختن به؛ پاسخ دادن به؛ دریافت شدن؛ وارد شدن به', 'bersivdan; behskirin; gihîştin; têketin', 'odnieść się do; wpłynąć; dotrzeć; zawrzeć', 'a aborda; a răspunde; a fi primit; a intra în', 'затрагивать; отвечать; поступать; вступать в', 'të trajtosh; të përgjigjesh; të merret; të hysh në', 'değinmek; yanıt vermek; ulaşmak; girmek'),
    examples: [
      ex('Auf die Einwände des Kunden ging das Angebot nur oberflächlich ein.', 'تناول العرض اعتراضات العميل بشكل سطحي فقط.', 'پێشنیارەکە تەنها بە شێوەیەکی سەرپێیی وەڵامی ناڕەزاییەکانی کڕیارەکەی دایەوە.', 'The proposal addressed the customer’s objections only superficially.', 'پیشنهاد فقط به‌صورت سطحی به ایرادهای مشتری پرداخت.', 'Pêşniyarê tenê bi awayekî seranserî bersiva nerazîbûnên xerîdar da.', 'Oferta odniosła się do zastrzeżeń klienta tylko powierzchownie.', 'Oferta a abordat obiecțiile clientului doar superficial.', 'Предложение лишь поверхностно затронуло возражения клиента.', 'Oferta i trajtoi kundërshtimet e klientit vetëm sipërfaqësisht.', 'Teklif, müşterinin itirazlarına yalnızca yüzeysel olarak değindi.'),
      ex('Die Unterlagen müssen bis Freitag vollständig bei der Behörde eingehen.', 'يجب أن تصل المستندات كاملة إلى الجهة الرسمية بحلول يوم الجمعة.', 'بەڵگەنامەکان دەبێت تا هەینی بە تەواوی بگەنە دەزگای فەرمی.', 'The documents must be received in full by the authority by Friday.', 'مدارک باید تا جمعه به‌طور کامل به اداره مربوطه برسند.', 'Belge divê heta Înê bi temamî bigihîjin dezgeha fermî.', 'Dokumenty muszą w całości wpłynąć do urzędu do piątku.', 'Documentele trebuie să ajungă complet la autoritate până vineri.', 'Документы должны полностью поступить в ведомство до пятницы.', 'Dokumentet duhet të mbërrijnë të plota te autoriteti deri të premten.', 'Belgelerin cuma gününe kadar eksiksiz olarak kuruma ulaşması gerekir.')
    ]
  }
];

const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = {
  packageVersion: '1.0', packageId, packageName: `German C1 Generated Batch ${num}`, source: 'Hybrid', defaultMeaningLanguages: langs,
  labels: labelRefs(usedLabels), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: []
};
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');

const importOutput = execFileSync('dotnet', ['run','--project', path.join(root, 'src/Apps/DarwinLingua.ImportTool/DarwinLingua.ImportTool.csproj'), '--', '--target', 'shared', '--yes', outPath], { cwd: root, encoding: 'utf8', stdio: ['ignore', 'pipe', 'pipe'] });
const imported = /Entries imported:\s*(\d+)/.exec(importOutput)?.[1] || '0';
const invalid = /Entries invalid:\s*(\d+)/.exec(importOutput)?.[1] || '0';
const warnings = /Warnings:\s*(\d+)/.exec(importOutput)?.[1] || '0';
const clean = imported === String(batchWords.length) && invalid === '0' && warnings === '0';
let deleted = false;
if (clean) {
  const current = splitTokens(fs.readFileSync(sourceFile, 'utf8'));
  const remaining = current.slice(batchWords.length);
  fs.writeFileSync(sourceFile, sourceText(remaining), 'utf8');
  deleted = true;
}
const after = splitTokens(fs.readFileSync(sourceFile, 'utf8'));
console.log(JSON.stringify({ sourceLevel, sourceFile, processedWords: batchWords, outPath, imported, invalid, warnings, deleted, remainingCount: after.length, first10Remaining: after.slice(0,10), importSummary: importOutput.split(/\r?\n/).filter(l => /Entries imported|Entries invalid|Warnings|Import completed/.test(l)).join('\n') }, null, 2));

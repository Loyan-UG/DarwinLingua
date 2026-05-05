const fs = require('fs');
const path = require('path');
const project = 'D:/_Projects/DarwinLingua';
const src = path.join(project, 'content/C1.txt');
const taxonomyPath = path.join(project, 'content/taxonomy/darwinlingua-taxonomy-v1.json');
const out = path.join(project, 'content/generated/de-c1-generated-erp-025-words.json');
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const raw = fs.readFileSync(src, 'utf8');
const tokens = raw.split(',').map(x => x.trim()).filter(Boolean);
const words = tokens.slice(0, 6);
if (words.length !== 4 || words.join('|') !== 'Wareneingangsposition|Warenhauptgruppe|Workflow|Workflow-Queue') {
  throw new Error('Unexpected C1 tokens: ' + words.join('|'));
}
function locText(obj, lang) {
  if (!obj) return '';
  if (typeof obj === 'string') return obj;
  if (obj[lang]) return obj[lang];
  if (obj.text) return obj.text;
  return '';
}
const wantedLabels = ['business','workplace','process','administrative','analysis'];
const taxLabels = taxonomy.labels || [];
const labels = wantedLabels.map(key => {
  const found = taxLabels.find(l => l.key === key || l.id === key || l.slug === key);
  if (!found) throw new Error('Missing label in taxonomy: ' + key);
  const localizations = {};
  for (const l of ['de', ...langs]) localizations[l] = locText(found.localizations?.[l] || found.name?.[l] || found.displayName?.[l] || found.translations?.[l], l) || key;
  return { key, localizations };
});
const entries = [
  {
    word: 'Wareneingangsposition', language: 'de', cefrLevel: 'C1', partOfSpeech: 'Noun', article: 'die', plural: 'Wareneingangspositionen', infinitive: null, pronunciationIpa: null, syllableBreak: 'Wa-ren-ein-gangs-po-si-ti-on',
    topics: ['warehouse-and-logistics','procurement-and-suppliers','erp-and-business-systems'], usageLabels: ['business','workplace','process','administrative'], contextLabels: [], grammarNotes: ['feminine compound noun used in goods receipt and procurement processes'],
    collocations: [{ text: 'eine Wareneingangsposition prüfen', meaning: 'to check a goods receipt line item' }, { text: 'eine Wareneingangsposition sperren', meaning: 'to block a goods receipt line item' }],
    wordFamilies: [{ lemma: 'der Wareneingang', relationLabel: 'base noun', note: null }, { lemma: 'die Position', relationLabel: 'component noun', note: null }], relations: [],
    meanings: [
      {language:'ar',text:'بند استلام البضائع'}, {language:'ckb',text:'بڕگەی وەرگرتنی کاڵا'}, {language:'en',text:'goods receipt line item'}, {language:'fa',text:'آیتم رسید کالا؛ ردیف ورود کالا'}, {language:'kmr',text:'hêla wergirtina malan'}, {language:'pl',text:'pozycja przyjęcia towaru'}, {language:'ro',text:'poziție de recepție a mărfii'}, {language:'ru',text:'позиция поступления товара'}, {language:'sq',text:'pozicion i pranimit të mallrave'}, {language:'tr',text:'mal kabul kalemi'}
    ],
    examples: [
      { baseText: 'Die Wareneingangsposition wird im ERP mit der Bestellung abgeglichen, bevor der Bestand freigegeben wird.', translations: [
        {language:'ar',text:'تتم مطابقة بند استلام البضائع في نظام ERP مع طلب الشراء قبل تحرير المخزون.'}, {language:'ckb',text:'بڕگەی وەرگرتنی کاڵا لە ERP دا لەگەڵ داواکاری کڕین بەراورد دەکرێت پێش ئەوەی کۆگا ئازاد بکرێت.'}, {language:'en',text:'The goods receipt line item is matched with the purchase order in the ERP before the stock is released.'}, {language:'fa',text:'آیتم رسید کالا در ERP با سفارش خرید تطبیق داده می‌شود، قبل از اینکه موجودی آزاد شود.'}, {language:'kmr',text:'Hêla wergirtina malan di ERP de bi fermana kirînê re tê berawirdkirin berî ku stok were berdan.'}, {language:'pl',text:'Pozycja przyjęcia towaru jest w ERP porównywana z zamówieniem, zanim zapas zostanie zwolniony.'}, {language:'ro',text:'Poziția de recepție a mărfii este comparată în ERP cu comanda înainte ca stocul să fie eliberat.'}, {language:'ru',text:'Позиция поступления товара в ERP сверяется с заказом на закупку до разблокировки запаса.'}, {language:'sq',text:'Pozicioni i pranimit të mallrave krahasohet në ERP me porosinë përpara se stoku të lirohet.'}, {language:'tr',text:'Mal kabul kalemi, stok serbest bırakılmadan önce ERP’de satın alma siparişiyle karşılaştırılır.'}
      ]},
      { baseText: 'Der Lagerleiter sperrte die Wareneingangsposition, weil die gelieferte Menge von der Bestellung abwich.', translations: [
        {language:'ar',text:'قام مدير المستودع بحظر بند استلام البضائع لأن الكمية المسلّمة اختلفت عن طلب الشراء.'}, {language:'ckb',text:'بەڕێوەبەری کۆگا بڕگەی وەرگرتنی کاڵای ڕاگرت، چونکە بڕی گەیشتوو لە داواکاری کڕین جیاواز بوو.'}, {language:'en',text:'The warehouse manager blocked the goods receipt line item because the delivered quantity differed from the order.'}, {language:'fa',text:'مدیر انبار آیتم رسید کالا را مسدود کرد، چون مقدار تحویل‌شده با سفارش تفاوت داشت.'}, {language:'kmr',text:'Rêvebirê embarê hêla wergirtina malan asteng kir, ji ber ku hejmarê radestkirî ji fermana kirînê cuda bû.'}, {language:'pl',text:'Kierownik magazynu zablokował pozycję przyjęcia towaru, ponieważ dostarczona ilość różniła się od zamówienia.'}, {language:'ro',text:'Șeful depozitului a blocat poziția de recepție, deoarece cantitatea livrată diferea de comandă.'}, {language:'ru',text:'Заведующий складом заблокировал позицию поступления, потому что поставленное количество отличалось от заказа.'}, {language:'sq',text:'Përgjegjësi i magazinës e bllokoi pozicionin e pranimit sepse sasia e dorëzuar ndryshonte nga porosia.'}, {language:'tr',text:'Depo sorumlusu mal kabul kalemini bloke etti, çünkü teslim edilen miktar siparişten farklıydı.'}
      ]}
    ]
  },
  {
    word: 'Warenhauptgruppe', language: 'de', cefrLevel: 'C1', partOfSpeech: 'Noun', article: 'die', plural: 'Warenhauptgruppen', infinitive: null, pronunciationIpa: null, syllableBreak: 'Wa-ren-haupt-grup-pe',
    topics: ['erp-and-business-systems','warehouse-and-logistics','data-and-reporting'], usageLabels: ['business','workplace','administrative','analysis'], contextLabels: [], grammarNotes: ['feminine compound noun used in product classification and reporting'],
    collocations: [{ text: 'eine Warenhauptgruppe pflegen', meaning: 'to maintain a main product group' }, { text: 'nach Warenhauptgruppen auswerten', meaning: 'to analyze by main product groups' }],
    wordFamilies: [{ lemma: 'die Warengruppe', relationLabel: 'related noun', note: null }], relations: [],
    meanings: [
      {language:'ar',text:'مجموعة السلع الرئيسية'}, {language:'ckb',text:'گرووپی سەرەکیی کاڵا'}, {language:'en',text:'main product group; main goods group'}, {language:'fa',text:'گروه اصلی کالا'}, {language:'kmr',text:'koma sereke ya malan'}, {language:'pl',text:'główna grupa towarowa'}, {language:'ro',text:'grupă principală de mărfuri'}, {language:'ru',text:'основная товарная группа'}, {language:'sq',text:'grupi kryesor i mallrave'}, {language:'tr',text:'ana ürün grubu; ana mal grubu'}
    ],
    examples: [
      { baseText: 'Die Warenhauptgruppe steuert im ERP Statistik, Kontenfindung und Einkaufsfreigaben.', translations: [
        {language:'ar',text:'تتحكم مجموعة السلع الرئيسية في ERP في الإحصاءات وتحديد الحسابات وموافقات الشراء.'}, {language:'ckb',text:'گرووپی سەرەکیی کاڵا لە ERP دا ئامار، دۆزینەوەی ئەکاونت و پەسەندکردنی کڕین ڕێکدەخات.'}, {language:'en',text:'The main product group controls statistics, account determination, and purchasing approvals in the ERP.'}, {language:'fa',text:'گروه اصلی کالا در ERP آمار، تعیین حساب‌ها و تأییدهای خرید را کنترل می‌کند.'}, {language:'kmr',text:'Koma sereke ya malan di ERP de statistik, diyarkirina hesabê û erêkirinên kirînê kontrol dike.'}, {language:'pl',text:'Główna grupa towarowa steruje w ERP statystykami, ustalaniem kont i zatwierdzeniami zakupów.'}, {language:'ro',text:'Grupa principală de mărfuri controlează în ERP statisticile, determinarea conturilor și aprobările de achiziții.'}, {language:'ru',text:'Основная товарная группа в ERP управляет статистикой, определением счетов и согласованиями закупок.'}, {language:'sq',text:'Grupi kryesor i mallrave kontrollon në ERP statistikat, përcaktimin e llogarive dhe miratimet e blerjeve.'}, {language:'tr',text:'Ana ürün grubu ERP’de istatistikleri, hesap belirlemeyi ve satın alma onaylarını yönetir.'}
      ]},
      { baseText: 'Controlling bereinigte mehrere Warenhauptgruppen, weil alte Bezeichnungen in den Berichten missverständlich waren.', translations: [
        {language:'ar',text:'قام قسم الرقابة المالية بتنظيف عدة مجموعات سلع رئيسية لأن التسميات القديمة كانت مربكة في التقارير.'}, {language:'ckb',text:'بەشی کۆنترۆڵینگ چەند گرووپی سەرەکیی کاڵای پاککردەوە، چونکە ناونیشانی کۆن لە ڕاپۆرتەکاندا تێکەڵ بوو.'}, {language:'en',text:'Controlling cleaned up several main product groups because old names were misleading in the reports.'}, {language:'fa',text:'واحد کنترل مالی چند گروه اصلی کالا را اصلاح کرد، چون نام‌های قدیمی در گزارش‌ها گمراه‌کننده بودند.'}, {language:'kmr',text:'Beşa kontrolê çend komên sereke yên malan paqij kir, ji ber ku navên kevn di raporan de şaş têgihiştin çêdikirin.'}, {language:'pl',text:'Controlling uporządkował kilka głównych grup towarowych, ponieważ stare nazwy były mylące w raportach.'}, {language:'ro',text:'Departamentul de controlling a curățat mai multe grupe principale de mărfuri, deoarece denumirile vechi erau neclare în rapoarte.'}, {language:'ru',text:'Отдел контроллинга очистил несколько основных товарных групп, потому что старые названия вводили в заблуждение в отчетах.'}, {language:'sq',text:'Kontrollingu pastroi disa grupe kryesore mallrash sepse emërtimet e vjetra ishin keqkuptuese në raporte.'}, {language:'tr',text:'Kontrol departmanı birkaç ana ürün grubunu düzeltti, çünkü eski adlar raporlarda yanıltıcıydı.'}
      ]}
    ]
  },
  {
    word: 'Workflow', language: 'de', cefrLevel: 'C1', partOfSpeech: 'Noun', article: 'der', plural: 'Workflows', infinitive: null, pronunciationIpa: null, syllableBreak: 'Work-flow',
    topics: ['erp-and-business-systems','technology-and-it','planning-and-projects'], usageLabels: ['business','workplace','process','administrative'], contextLabels: [], grammarNotes: ['masculine loanword commonly used in IT and business process management'],
    collocations: [{ text: 'einen Workflow auslösen', meaning: 'to trigger a workflow' }, { text: 'einen Workflow optimieren', meaning: 'to optimize a workflow' }],
    wordFamilies: [{ lemma: 'Workflow-Queue', relationLabel: 'related noun', note: null }], relations: [],
    meanings: [
      {language:'ar',text:'سير عمل'}, {language:'ckb',text:'ڕەوتی کار'}, {language:'en',text:'workflow'}, {language:'fa',text:'گردش کار؛ روند کاری'}, {language:'kmr',text:'herikîna kar'}, {language:'pl',text:'przepływ pracy'}, {language:'ro',text:'flux de lucru'}, {language:'ru',text:'рабочий процесс; workflow'}, {language:'sq',text:'rrjedhë pune'}, {language:'tr',text:'iş akışı'}
    ],
    examples: [
      { baseText: 'Der Workflow sendet im ERP automatisch eine Aufgabe an die Buchhaltung, sobald eine Rechnung gesperrt wird.', translations: [
        {language:'ar',text:'يرسل سير العمل في ERP مهمة تلقائيًا إلى المحاسبة بمجرد حظر فاتورة.'}, {language:'ckb',text:'ڕەوتی کار لە ERP دا خۆکارانە ئەرکێک بۆ ژمێریاری دەنێرێت کاتێک فاکتورەیەک قەدەغە دەکرێت.'}, {language:'en',text:'The workflow automatically sends a task to accounting in the ERP as soon as an invoice is blocked.'}, {language:'fa',text:'گردش کار در ERP به‌محض مسدود شدن یک فاکتور، به‌صورت خودکار کاری را به حسابداری می‌فرستد.'}, {language:'kmr',text:'Herikîna kar di ERP de dema fatûreyek were astengkirin, xweber karekî ji hesabdarî re dişîne.'}, {language:'pl',text:'Workflow automatycznie wysyła w ERP zadanie do księgowości, gdy tylko faktura zostanie zablokowana.'}, {language:'ro',text:'Fluxul de lucru trimite automat în ERP o sarcină către contabilitate imediat ce o factură este blocată.'}, {language:'ru',text:'Workflow в ERP автоматически отправляет задачу в бухгалтерию, как только счет блокируется.'}, {language:'sq',text:'Rrjedha e punës dërgon automatikisht në ERP një detyrë te kontabiliteti sapo një faturë bllokohet.'}, {language:'tr',text:'İş akışı, bir fatura bloke edilir edilmez ERP’de muhasebeye otomatik olarak bir görev gönderir.'}
      ]},
      { baseText: 'Nach der Prozessanalyse wurde der Workflow vereinfacht, weil zu viele manuelle Freigaben nötig waren.', translations: [
        {language:'ar',text:'بعد تحليل العملية، تم تبسيط سير العمل لأن عددًا كبيرًا من الموافقات اليدوية كان ضروريًا.'}, {language:'ckb',text:'دوای شیکردنەوەی پرۆسەکە، ڕەوتی کار ئاسانکرا، چونکە زۆر پەسەندکردنی دەستی پێویست بوو.'}, {language:'en',text:'After the process analysis, the workflow was simplified because too many manual approvals were required.'}, {language:'fa',text:'پس از تحلیل فرایند، گردش کار ساده‌تر شد، چون تأییدهای دستی زیادی لازم بود.'}, {language:'kmr',text:'Piştî analîza pêvajoyê, herikîna kar hêsan bû, ji ber ku gelek erêkirinên destan pêwîst bûn.'}, {language:'pl',text:'Po analizie procesu workflow został uproszczony, ponieważ wymagano zbyt wielu ręcznych zatwierdzeń.'}, {language:'ro',text:'După analiza procesului, fluxul de lucru a fost simplificat, deoarece erau necesare prea multe aprobări manuale.'}, {language:'ru',text:'После анализа процесса workflow упростили, потому что требовалось слишком много ручных согласований.'}, {language:'sq',text:'Pas analizës së procesit, rrjedha e punës u thjeshtua sepse nevojiteshin shumë miratime manuale.'}, {language:'tr',text:'Süreç analizinden sonra iş akışı sadeleştirildi, çünkü çok fazla manuel onay gerekiyordu.'}
      ]}
    ]
  },
  {
    word: 'Workflow-Queue', language: 'de', cefrLevel: 'C1', partOfSpeech: 'Noun', article: 'die', plural: 'Workflow-Queues', infinitive: null, pronunciationIpa: null, syllableBreak: 'Work-flow-Queue',
    topics: ['erp-and-business-systems','technology-and-it','data-and-reporting'], usageLabels: ['business','workplace','process','analysis'], contextLabels: [], grammarNotes: ['feminine compound loanword used for queued workflow tasks in business systems'],
    collocations: [{ text: 'die Workflow-Queue überwachen', meaning: 'to monitor the workflow queue' }, { text: 'eine Workflow-Queue leeren', meaning: 'to clear a workflow queue' }],
    wordFamilies: [{ lemma: 'der Workflow', relationLabel: 'base noun', note: null }], relations: [],
    meanings: [
      {language:'ar',text:'قائمة انتظار سير العمل'}, {language:'ckb',text:'ڕیزی چاوەڕوانی ڕەوتی کار'}, {language:'en',text:'workflow queue'}, {language:'fa',text:'صف گردش کار'}, {language:'kmr',text:'rêza herikîna kar'}, {language:'pl',text:'kolejka workflow'}, {language:'ro',text:'coadă de flux de lucru'}, {language:'ru',text:'очередь workflow; очередь рабочего процесса'}, {language:'sq',text:'radhë e rrjedhës së punës'}, {language:'tr',text:'iş akışı kuyruğu'}
    ],
    examples: [
      { baseText: 'Die Workflow-Queue zeigt im ERP, welche Aufgaben wegen fehlender Freigaben noch warten.', translations: [
        {language:'ar',text:'تُظهر قائمة انتظار سير العمل في ERP أي المهام لا تزال تنتظر بسبب نقص الموافقات.'}, {language:'ckb',text:'ڕیزی چاوەڕوانی ڕەوتی کار لە ERP دا نیشان دەدات کام ئەرکەکان بەهۆی نەبوونی پەسەندکردنەوە هێشتا چاوەڕوانن.'}, {language:'en',text:'The workflow queue in the ERP shows which tasks are still waiting because approvals are missing.'}, {language:'fa',text:'صف گردش کار در ERP نشان می‌دهد کدام کارها به‌دلیل نبود تأیید هنوز منتظر مانده‌اند.'}, {language:'kmr',text:'Rêza herikîna kar di ERP de nîşan dide kîjan kar ji ber kêmbûna erêkirinan hîn li bendê ne.'}, {language:'pl',text:'Kolejka workflow pokazuje w ERP, które zadania nadal czekają z powodu brakujących zatwierdzeń.'}, {language:'ro',text:'Coada de flux de lucru arată în ERP ce sarcini mai așteaptă din cauza aprobărilor lipsă.'}, {language:'ru',text:'Очередь workflow в ERP показывает, какие задачи все еще ожидают из-за отсутствующих согласований.'}, {language:'sq',text:'Radha e rrjedhës së punës tregon në ERP cilat detyra ende presin për shkak të miratimeve që mungojnë.'}, {language:'tr',text:'İş akışı kuyruğu ERP’de eksik onaylar nedeniyle hangi görevlerin hâlâ beklediğini gösterir.'}
      ]},
      { baseText: 'Der Support leerte die blockierte Workflow-Queue erst, nachdem die fehlerhafte Regel deaktiviert worden war.', translations: [
        {language:'ar',text:'قام فريق الدعم بتفريغ قائمة انتظار سير العمل المحظورة فقط بعد تعطيل القاعدة الخاطئة.'}, {language:'ckb',text:'تیمی پشتگیری تەنها دوای ناچالاککردنی یاسای هەڵە ڕیزی چاوەڕوانی ڕەوتی کاری گیرکردوو بەتاڵ کرد.'}, {language:'en',text:'Support cleared the blocked workflow queue only after the faulty rule had been deactivated.'}, {language:'fa',text:'تیم پشتیبانی صف گردش کار مسدودشده را فقط پس از غیرفعال شدن قانون خطادار خالی کرد.'}, {language:'kmr',text:'Piştgirî tenê piştî ku qaîdeya şaş hate neçalakirn, rêza herikîna kar a astengkirî vala kir.'}, {language:'pl',text:'Support wyczyścił zablokowaną kolejkę workflow dopiero po dezaktywacji błędnej reguły.'}, {language:'ro',text:'Suportul a golit coada blocată de flux de lucru abia după ce regula eronată fusese dezactivată.'}, {language:'ru',text:'Служба поддержки очистила заблокированную очередь workflow только после отключения ошибочного правила.'}, {language:'sq',text:'Mbështetja e pastroi radhën e bllokuar të rrjedhës së punës vetëm pasi rregulli i gabuar u çaktivizua.'}, {language:'tr',text:'Destek ekibi bloke olan iş akışı kuyruğunu ancak hatalı kural devre dışı bırakıldıktan sonra temizledi.'}
      ]}
    ]
  }
];
const pkg = {
  packageVersion: '1.0',
  packageId: 'de-c1-generated-erp-025',
  packageName: 'German C1 Generated ERP Batch 025',
  source: 'Hybrid',
  defaultMeaningLanguages: langs,
  labels,
  entries,
  collections: [],
  scenarios: [],
  conversationStarterPacks: [],
  eventPreparationPacks: []
};
fs.writeFileSync(out, JSON.stringify(pkg, null, 2), 'utf8');
console.log(out);

const fs = require('fs');
const path = require('path');
const root = 'D:/_Projects/DarwinLingua';
const sourcePath = path.join(root, 'content/B1.txt');
const taxonomyPath = path.join(root, 'content/taxonomy/darwinlingua-taxonomy-v1.json');
const generatedDir = path.join(root, 'content/generated');
const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const tokens = fs.readFileSync(sourcePath, 'utf8').split(',').map(s => s.trim()).filter(Boolean);
const words = tokens.slice(0, 6);
const expected = ['zuschließen','der Zuschuss','zuständig','zustimmen','zutreffen','zwingen'];
if (JSON.stringify(words) !== JSON.stringify(expected)) throw new Error('Unexpected first words: ' + words.join(' | '));
const files = fs.readdirSync(generatedDir).map(f => /^de-b1-generated-batch-(\d+)\.json$/.exec(f)).filter(Boolean).map(m => Number(m[1]));
const next = Math.max(0, ...files) + 1;
const batch = String(next).padStart(3, '0');
const packageId = `de-b1-generated-batch-${batch}`;
const outPath = path.join(generatedDir, `${packageId}.json`);
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const labelLangs = ['de', ...langs];
function complete(label){ const loc=label.localizations||[]; return labelLangs.every(l=>loc.some(x=>x.language===l && x.name)); }
const usedLabels = ['everyday','spoken','written','business','workplace','customer-facing','high-frequency','administrative','formal'];
function getLabel(key){ const label=(taxonomy.labels||[]).find(l=>(l.key===key||l.id===key)&&complete(l)); if(!label) throw new Error('Missing complete label: '+key); return label; }
function tr(ar,ckb,en,fa,kmr,pl,ro,ru,sq,tr){ return [{language:'ar',text:ar},{language:'ckb',text:ckb},{language:'en',text:en},{language:'fa',text:fa},{language:'kmr',text:kmr},{language:'pl',text:pl},{language:'ro',text:ro},{language:'ru',text:ru},{language:'sq',text:sq},{language:'tr',text:tr}]; }
function ex(base, translations){ return { baseText: base, translations }; }
const entries = [
  {
    word:'zuschließen', language:'de', cefrLevel:'B1', partOfSpeech:'Verb', article:null, plural:null, infinitive:'zuschließen', pronunciationIpa:null, syllableBreak:'zu-schlie-ßen',
    topics:['everyday-life','housing-and-real-estate','work-and-jobs'], usageLabels:['spoken','everyday','high-frequency'], contextLabels:[], grammarNotes:['separable verb: ich schließe zu, ich schloss zu, ich habe zugeschlossen'],
    collocations:[{text:'die Tür zuschließen',meaning:'to lock the door'},{text:'das Büro zuschließen',meaning:'to lock the office'}],
    wordFamilies:[{lemma:'schließen',relationLabel:'verb',note:null},{lemma:'der Schlüssel',relationLabel:'noun',note:null}], relations:[],
    meanings: tr('يغلق بالمفتاح','بە کلیل داخستن','to lock','قفل کردن','bi mifte girtin','zamknąć na klucz','a încuia','запирать на ключ','të kyçësh','kilitlemek'),
    examples:[
      ex('Bitte schließen Sie die Eingangstür abends immer zu.', tr('يرجى دائماً قفل باب المدخل مساءً.','تکایە ئێواران هەمیشە دەرگای چوونەژوورەوە بە کلیل دابخەن.','Please always lock the entrance door in the evening.','لطفاً عصرها همیشه در ورودی را قفل کنید.','Ji kerema xwe êvaran her dem deriyê ketinê bi mifte bigirin.','Proszę zawsze zamykać drzwi wejściowe wieczorem na klucz.','Vă rugăm să încuiați întotdeauna ușa de la intrare seara.','Пожалуйста, всегда запирайте входную дверь вечером.','Ju lutem kyçeni gjithmonë derën e hyrjes në mbrëmje.','Lütfen akşamları giriş kapısını her zaman kilitleyin.')),
      ex('Ich habe das Büro zugeschlossen und den Schlüssel an der Rezeption abgegeben.', tr('أغلقت المكتب بالمفتاح وسلّمت المفتاح في الاستقبال.','ئۆفیسەکەم بە کلیل داخست و کلیلەکەم لە پێشوازی دامەوە.','I locked the office and left the key at reception.','دفتر را قفل کردم و کلید را به پذیرش تحویل دادم.','Min nivîsgeh bi mifte girt û mifte li resepsiyonê da.','Zamknąłem biuro na klucz i zostawiłem klucz w recepcji.','Am încuiat biroul și am lăsat cheia la recepție.','Я запер офис и оставил ключ на ресепшене.','E kyça zyrën dhe e lashë çelësin te recepsioni.','Ofisi kilitledim ve anahtarı resepsiyona bıraktım.'))
    ]
  },
  {
    word:'der Zuschuss', language:'de', cefrLevel:'B1', partOfSpeech:'Noun', article:'der', plural:'Zuschüsse', infinitive:null, pronunciationIpa:null, syllableBreak:'Zu-schuss',
    topics:['finance-and-accounting','public-services','documents-and-administration'], usageLabels:['formal','administrative','written'], contextLabels:[], grammarNotes:['masculine noun: der Zuschuss, die Zuschüsse'],
    collocations:[{text:'einen Zuschuss beantragen',meaning:'to apply for a subsidy'},{text:'einen Zuschuss bekommen',meaning:'to receive a grant'}],
    wordFamilies:[{lemma:'bezuschussen',relationLabel:'verb',note:null}], relations:[],
    meanings: tr('دعم مالي؛ منحة','یارمەتی دارایی؛ پاڵپشتی پارەیی','subsidy; grant; financial support','کمک‌هزینه؛ یارانه؛ حمایت مالی','piştgiriya darayî; alîkariya pereyî','dotacja; dofinansowanie','subvenție; sprijin financiar','субсидия; дотация; финансовая помощь','subvencion; grant; mbështetje financiare','hibe; destek ödemesi; mali destek'),
    examples:[
      ex('Für die Weiterbildung können Sie einen Zuschuss beantragen.', tr('يمكنكم التقدم بطلب للحصول على دعم مالي للتدريب الإضافي.','بۆ فێربوونی زیاتر دەتوانن داوای یارمەتی دارایی بکەن.','You can apply for a subsidy for the further training.','برای دوره آموزشی تکمیلی می‌توانید درخواست کمک‌هزینه بدهید.','Hûn dikarin ji bo perwerdeya zêde daxwaza piştgiriya darayî bikin.','Na szkolenie można złożyć wniosek o dofinansowanie.','Pentru cursul de perfecționare puteți solicita o subvenție.','На повышение квалификации можно подать заявку на субсидию.','Për trajnimin e mëtejshëm mund të aplikoni për mbështetje financiare.','İleri eğitim için destek ödemesine başvurabilirsiniz.')),
      ex('Die Stadt zahlt Familien mit geringem Einkommen einen Zuschuss zur Miete.', tr('تدفع المدينة للأسر ذات الدخل المنخفض دعماً مالياً للإيجار.','شارەوانی یارمەتی کرێی خانوو بە خێزانە کەم داهاتەکان دەدات.','The city pays families with low income a rent subsidy.','شهر به خانواده‌های کم‌درآمد کمک‌هزینه اجاره می‌دهد.','Şaredarî ji malbatên kêm-dahat re piştgiriya kirê dide.','Miasto wypłaca rodzinom o niskich dochodach dodatek do czynszu.','Orașul plătește familiilor cu venituri mici o subvenție pentru chirie.','Город выплачивает семьям с низким доходом субсидию на аренду.','Qyteti u paguan familjeve me të ardhura të ulëta një subvencion për qiranë.','Belediye düşük gelirli ailelere kira desteği ödüyor.'))
    ]
  },
  {
    word:'zuständig', language:'de', cefrLevel:'B1', partOfSpeech:'Adjective', article:null, plural:null, infinitive:null, pronunciationIpa:null, syllableBreak:'zu-stän-dig',
    topics:['work-and-jobs','documents-and-administration','customer-service'], usageLabels:['business','administrative','workplace'], contextLabels:[], grammarNotes:['adjective; often used with für + accusative'],
    collocations:[{text:'für etwas zuständig sein',meaning:'to be responsible for something'},{text:'die zuständige Abteilung',meaning:'the responsible department'}],
    wordFamilies:[{lemma:'die Zuständigkeit',relationLabel:'noun',note:null}], relations:[],
    meanings: tr('مسؤول؛ مختص','بەرپرسیار؛ پەیوەندیدار بە کارێک','responsible; in charge; competent authority','مسئول؛ مربوط؛ ذی‌صلاح','berpirsiyar; berpirsê karêkî','odpowiedzialny; właściwy','responsabil; competent','ответственный; компетентный','përgjegjës; kompetent','sorumlu; yetkili'),
    examples:[
      ex('Für technische Fragen ist unsere IT-Abteilung zuständig.', tr('قسم تكنولوجيا المعلومات لدينا مسؤول عن الأسئلة التقنية.','بۆ پرسیارە تەکنیکییەکان بەشی IT ی ئێمە بەرپرسیارە.','Our IT department is responsible for technical questions.','برای پرسش‌های فنی، بخش IT ما مسئول است.','Ji bo pirsên teknîkî, beşa IT ya me berpirsiyar e.','Za pytania techniczne odpowiada nasz dział IT.','Pentru întrebări tehnice este responsabil departamentul nostru IT.','За технические вопросы отвечает наш IT-отдел.','Për pyetjet teknike është përgjegjës departamenti ynë i IT-së.','Teknik sorulardan IT departmanımız sorumludur.')),
      ex('Ich verbinde Sie mit der zuständigen Mitarbeiterin.', tr('سأحوّلكم إلى الموظفة المختصة.','دەتانگەیەنم بە کارمەندە پەیوەندیدارەکە.','I will connect you with the responsible employee.','شما را به کارمند مسئول وصل می‌کنم.','Ez ê we bi karmenda berpirsiyar ve girê bidim.','Połączę Pana/Panią z odpowiednią pracownicą.','Vă fac legătura cu angajata responsabilă.','Я соединю вас с ответственным сотрудником.','Do t’ju lidh me punonjësen përgjegjëse.','Sizi sorumlu çalışanla bağlıyorum.'))
    ]
  },
  {
    word:'zustimmen', language:'de', cefrLevel:'B1', partOfSpeech:'Verb', article:null, plural:null, infinitive:'zustimmen', pronunciationIpa:null, syllableBreak:'zu-stim-men',
    topics:['meetings-and-presentations','business-communication','social-and-relationships'], usageLabels:['spoken','business','formal'], contextLabels:[], grammarNotes:['verb with dative: jemandem/einem Vorschlag zustimmen'],
    collocations:[{text:'einem Vorschlag zustimmen',meaning:'to agree to a proposal'},{text:'voll zustimmen',meaning:'to fully agree'}],
    wordFamilies:[{lemma:'die Zustimmung',relationLabel:'noun',note:null},{lemma:'die Stimme',relationLabel:'noun',note:null}], relations:[],
    meanings: tr('يوافق','ڕازیبوون؛ پەسەندکردن','to agree; to approve','موافقت کردن؛ تأیید کردن','razî bûn; pejirandin','zgodzić się; poprzeć','a fi de acord; a aproba','соглашаться; одобрять','të pajtohesh; të miratosh','katılmak; onaylamak'),
    examples:[
      ex('Ich stimme Ihrem Vorschlag grundsätzlich zu.', tr('أنا أوافق على اقتراحكم من حيث المبدأ.','من بە بنەڕەت ڕازیم لەگەڵ پێشنیارەکەتان.','I basically agree with your proposal.','من در اصل با پیشنهاد شما موافقم.','Ez bi bingehîn li ser pêşniyara we razî me.','Zasadniczo zgadzam się z Pana/Pani propozycją.','Sunt de acord în principiu cu propunerea dumneavoastră.','Я в принципе согласен с вашим предложением.','Në parim pajtohem me propozimin tuaj.','Önerinize prensip olarak katılıyorum.')),
      ex('Die Kundin muss der Änderung schriftlich zustimmen.', tr('يجب أن توافق العميلة على التغيير كتابةً.','دەبێت کڕیارەکە بە نووسین ڕەزامەندی لەسەر گۆڕانکارییەکە بدات.','The customer has to agree to the change in writing.','مشتری باید به صورت کتبی با تغییر موافقت کند.','Divê xerîdar bi nivîskî li ser guherînê razî bibe.','Klientka musi pisemnie zgodzić się na zmianę.','Clienta trebuie să fie de acord cu modificarea în scris.','Клиентка должна письменно согласиться на изменение.','Klientja duhet të pajtohet me ndryshimin me shkrim.','Müşterinin değişikliği yazılı olarak onaylaması gerekir.'))
    ]
  },
  {
    word:'zutreffen', language:'de', cefrLevel:'B1', partOfSpeech:'Verb', article:null, plural:null, infinitive:'zutreffen', pronunciationIpa:null, syllableBreak:'zu-tref-fen',
    topics:['documents-and-administration','business-communication','data-and-reporting'], usageLabels:['formal','written','administrative'], contextLabels:[], grammarNotes:['separable verb: trifft zu, traf zu, hat zugetroffen; often impersonal: Das trifft zu.'],
    collocations:[{text:'Das trifft zu.',meaning:'That is correct / That applies.'},{text:'auf jemanden zutreffen',meaning:'to apply to someone'}],
    wordFamilies:[{lemma:'treffen',relationLabel:'verb',note:null}], relations:[],
    meanings: tr('ينطبق؛ يكون صحيحاً','ڕاستبوون؛ گونجاو بوون بۆ شتێک','to apply; to be correct; to be true','صدق کردن؛ درست بودن؛ شامل شدن','rast bûn; li ser tiştekî derbas bûn','dotyczyć; być prawdą','a se aplica; a fi adevărat','соответствовать; быть верным; относиться','të vlejë; të jetë e vërtetë','geçerli olmak; doğru olmak'),
    examples:[
      ex('Diese Regel trifft nur auf neue Verträge zu.', tr('تنطبق هذه القاعدة فقط على العقود الجديدة.','ئەم یاسایە تەنها بۆ گرێبەستە نوێیەکان کاردەکات.','This rule only applies to new contracts.','این قانون فقط شامل قراردادهای جدید می‌شود.','Ev rêgez tenê ji bo peymanên nû derbas dibe.','Ta zasada dotyczy tylko nowych umów.','Această regulă se aplică doar contractelor noi.','Это правило относится только к новым договорам.','Ky rregull vlen vetëm për kontratat e reja.','Bu kural yalnızca yeni sözleşmeler için geçerlidir.')),
      ex('Wenn die Angaben zutreffen, können wir den Antrag heute bearbeiten.', tr('إذا كانت البيانات صحيحة، يمكننا معالجة الطلب اليوم.','ئەگەر زانیارییەکان ڕاست بن، دەتوانین ئەمڕۆ داواکارییەکە چارەسەر بکەین.','If the information is correct, we can process the application today.','اگر اطلاعات درست باشد، می‌توانیم امروز درخواست را بررسی کنیم.','Ger agahî rast bin, em dikarin îro daxwaznameyê bişopînin.','Jeśli dane są prawidłowe, możemy dzisiaj rozpatrzyć wniosek.','Dacă informațiile sunt corecte, putem procesa cererea astăzi.','Если данные верны, мы можем обработать заявление сегодня.','Nëse të dhënat janë të sakta, mund ta përpunojmë kërkesën sot.','Bilgiler doğruysa başvuruyu bugün işleme alabiliriz.'))
    ]
  },
  {
    word:'zwingen', language:'de', cefrLevel:'B1', partOfSpeech:'Verb', article:null, plural:null, infinitive:'zwingen', pronunciationIpa:null, syllableBreak:'zwin-gen',
    topics:['work-and-jobs','law-and-compliance','social-and-relationships'], usageLabels:['written','formal','high-frequency'], contextLabels:[], grammarNotes:['irregular verb: zwingt, zwang, hat gezwungen; often with zu + dative noun or infinitive clause'],
    collocations:[{text:'jemanden zu etwas zwingen',meaning:'to force someone to do something'},{text:'sich gezwungen sehen',meaning:'to feel forced'}],
    wordFamilies:[{lemma:'der Zwang',relationLabel:'noun',note:null},{lemma:'gezwungen',relationLabel:'adjective',note:null}], relations:[],
    meanings: tr('يجبر؛ يُرغم','ناچارکردن؛ زۆرلێکردن','to force; to compel','مجبور کردن؛ وادار کردن','neçar kirin; bi zor kirin','zmuszać','a forța; a obliga','заставлять; принуждать','të detyrosh; të shtrëngosh','zorlamak; mecbur bırakmak'),
    examples:[
      ex('Niemand darf Sie zwingen, einen Vertrag sofort zu unterschreiben.', tr('لا يحق لأحد أن يجبركم على توقيع عقد فوراً.','هیچ کەسێک نابێت ناچارتان بکات دەستبەجێ گرێبەستێک واژۆ بکەن.','No one may force you to sign a contract immediately.','هیچ‌کس حق ندارد شما را مجبور کند فوراً قراردادی را امضا کنید.','Kes nikare we neçar bike ku hûn peymanekê tavilê îmze bikin.','Nikt nie może zmuszać Pana/Pani do natychmiastowego podpisania umowy.','Nimeni nu are voie să vă oblige să semnați imediat un contract.','Никто не имеет права заставлять вас сразу подписывать договор.','Askush nuk mund t’ju detyrojë të nënshkruani menjëherë një kontratë.','Hiç kimse sizi hemen bir sözleşme imzalamaya zorlayamaz.')),
      ex('Der starke Regen zwang uns, die Veranstaltung nach drinnen zu verlegen.', tr('أجبرنا المطر الشديد على نقل الفعالية إلى الداخل.','بارانە بەهێزەکە ناچاری کردین بۆنەکە بگوازینەوە بۆ ناوەوە.','The heavy rain forced us to move the event indoors.','باران شدید ما را مجبور کرد برنامه را به داخل منتقل کنیم.','Barana xurt em neçar kir ku çalakiyê bibin hundur.','Silny deszcz zmusił nas do przeniesienia wydarzenia do środka.','Ploaia puternică ne-a obligat să mutăm evenimentul în interior.','Сильный дождь заставил нас перенести мероприятие в помещение.','Shiu i fortë na detyroi ta zhvendosnim aktivitetin brenda.','Şiddetli yağmur etkinliği içeriye almamıza neden oldu.'))
    ]
  }
];
const pkg = { packageVersion:'1.0', packageId, packageName:`German B1 Generated Batch ${batch}`, source:'Hybrid', defaultMeaningLanguages:langs, labels: usedLabels.map(getLabel), entries, collections:[], scenarios:[], conversationStarterPacks:[], eventPreparationPacks:[] };
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');
console.log(outPath);
console.log(words.join('|'));

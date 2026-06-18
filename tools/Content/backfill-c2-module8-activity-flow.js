const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c2-stil-souveraenitaet-und-komplexer-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'Falllage sauber erfassen',
    en: 'Understand the case clearly',
    fa: 'وضعیت پرونده را دقیق بفهم',
    ar: 'افهم وضع الحالة بدقة',
    tr: 'Vaka durumunu net kavra',
    ru: 'Точно пойми ситуацию дела',
    ckb: 'دۆخی کەیسەکە بە وردی تێبگە',
    kmr: 'Rewşa dozê bi zelalî fam bike',
    pl: 'Dokładnie uchwyć sytuację sprawy',
    ro: 'Înțelege clar situația cazului',
    sq: 'Kuptoje qartë situatën e çështjes'
  },
  language: {
    de: 'Regel und Einzelfall trennen',
    en: 'Separate rule and individual case',
    fa: 'قاعده کلی و مورد خاص را جدا کن',
    ar: 'افصل بين القاعدة والحالة الفردية',
    tr: 'Kuralı ve tekil durumu ayır',
    ru: 'Раздели правило и отдельный случай',
    ckb: 'یاسای گشتی و کەیسی تایبەت جیا بکە',
    kmr: 'Rêgez û rewşa taybet ji hev cuda bike',
    pl: 'Oddziel zasadę od przypadku jednostkowego',
    ro: 'Separă regula de cazul individual',
    sq: 'Ndaje rregullin nga rasti i veçantë'
  },
  target: {
    de: 'Anspruch ruhig vertreten',
    en: 'Present the claim calmly',
    fa: 'درخواست یا حق خودت را آرام و دقیق مطرح کن',
    ar: 'اعرض مطلبك بهدوء ودقة',
    tr: 'Talebini sakin ve kesin ifade et',
    ru: 'Спокойно и точно представь свое требование',
    ckb: 'داواکەت بە ئارامی و وردی بخەڕوو',
    kmr: 'Daxwaza xwe bi aramî û daqîqî pêşkêş bike',
    pl: 'Spokojnie przedstaw swoje roszczenie',
    ro: 'Prezintă solicitarea calm și precis',
    sq: 'Paraqite kërkesën tënde qetë dhe saktë'
  },
  transfer: {
    de: 'Naechsten Schritt festhalten',
    en: 'Record the next step',
    fa: 'قدم بعدی را مشخص و قابل پیگیری بنویس',
    ar: 'ثبّت الخطوة التالية بوضوح وقابلية متابعة',
    tr: 'Sonraki adımı açık ve takip edilebilir yaz',
    ru: 'Зафиксируй следующий шаг ясно и отслеживаемо',
    ckb: 'هەنگاوی داهاتوو بە ڕوونی و بە شێوەیەکی شوێنپێهەڵگیر بنووسە',
    kmr: 'Gava din bi zelalî û şopandinbar binivîse',
    pl: 'Zapisz następny krok jasno i możliwie do śledzenia',
    ro: 'Notează pasul următor clar și urmăribil',
    sq: 'Shkruaje hapin tjetër qartë dhe të ndjekshëm'
  },
  review: {
    de: 'Fairness und Beleg pruefen',
    en: 'Check fairness and evidence',
    fa: 'انصاف و سند را بررسی کن',
    ar: 'افحص الإنصاف والدليل',
    tr: 'Adaleti ve kanıtı kontrol et',
    ru: 'Проверь справедливость и доказательства',
    ckb: 'دادپەروەری و بەڵگە بپشکنە',
    kmr: 'Dadperwerî û delîlê kontrol bike',
    pl: 'Sprawdź uczciwość i dowód',
    ro: 'Verifică echitatea și dovada',
    sq: 'Kontrollo drejtësinë dhe provën'
  }
};

const items = [
  {
    slug: 'c2-amtliche-auflage-verhandeln',
    topic: { de: 'eine amtliche Auflage', en: 'an official requirement', fa: 'یک الزام یا شرط رسمی از طرف اداره', ar: 'شرط أو إلزام رسمي من جهة إدارية', tr: 'resmi bir idari yükümlülük', ru: 'официальное предписание органа', ckb: 'مەرجێکی فەرمی لەلایەن دامەزراوە', kmr: 'merc an şertake fermî ji aliyê rêveberiyê', pl: 'urzędowy warunek lub nakaz', ro: 'o condiție oficială impusă de autoritate', sq: 'një kusht zyrtar nga autoriteti' },
    focus: { de: 'die Auflage ernst zu nehmen und trotzdem Spielraum sachlich zu erfragen', en: 'taking the requirement seriously while asking objectively about room for adjustment', fa: 'شرط رسمی را جدی بگیری و در عین حال امکان انعطاف یا راه‌حل جایگزین را محترمانه بپرسی', ar: 'أخذ الشرط الرسمي بجدية مع السؤال بموضوعية عن مساحة للتعديل', tr: 'yükümlülüğü ciddiye almak ve yine de esneklik alanını nesnel biçimde sormak', ru: 'серьезно отнестись к предписанию и при этом делово спросить о возможном пространстве для решения', ckb: 'مەرجە فەرمییەکە بە جدی وەربگریت و هەمان کات بە شێوەی بابەتی دەربارەی بۆشایی چارەسەر بپرسیت', kmr: 'mercê fermî ciddî bigirî û di heman demê de bi awayekî babetî li qada çareseriyê bipirsî', pl: 'potraktować wymóg poważnie, a jednocześnie rzeczowo zapytać o pole manewru', ro: 'să iei cerința în serios și totuși să întrebi obiectiv despre spațiul de ajustare', sq: 'ta marrësh seriozisht kushtin dhe njëkohësisht të pyesësh me respekt për hapësirë zgjidhjeje' },
    grammar: 'c2-legal-style-sentence-structures',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-amtliche-auflage-verhandeln'
  },
  {
    slug: 'c2-widerspruch-muendlich-begruenden',
    topic: { de: 'einen Widerspruch', en: 'an objection or appeal', fa: 'اعتراض رسمی به یک تصمیم', ar: 'اعتراض رسمي على قرار', tr: 'resmi bir itiraz', ru: 'официальное возражение', ckb: 'ناڕەزاییەکی فەرمی', kmr: 'îtiraza fermî', pl: 'formalny sprzeciw lub odwołanie', ro: 'o contestație formală', sq: 'një kundërshtim zyrtar' },
    focus: { de: 'deinen Punkt muendlich klar zu begruenden, ohne aggressiv oder bittend zu wirken', en: 'justifying your point orally without sounding aggressive or pleading', fa: 'دلیل اعتراضت را شفاهی روشن بگویی، بدون اینکه تهاجمی یا التماس‌آمیز به نظر برسد', ar: 'توضيح سبب اعتراضك شفهيًا من دون أن تبدو عدوانيًا أو متوسلًا', tr: 'itiraz gerekçeni sözlü olarak açık anlatmak ama saldırgan ya da yalvarır görünmemek', ru: 'устно ясно обосновать свою позицию, не звуча агрессивно или просительно', ckb: 'هۆکاری ناڕەزاییەکەت بە زاری ڕوون بکەیت بەبێ ئەوەی توند یان پاڕانەوەیی دەربکەویت', kmr: 'sedema îtiraza xwe bi devkî zelal bikî bêyî ku tund an lava xuya bikî', pl: 'ustnie jasno uzasadnić swój punkt bez tonu agresji lub proszenia', ro: 'să îți justifici oral punctul clar, fără să pari agresiv sau rugător', sq: 'ta arsyetosh qartë me gojë kundërshtimin tënd pa tingëlluar agresiv ose lutës' },
    grammar: 'c2-c2-formal-writing-grammar',
    targetType: 'roleplay',
    targetSlug: 'c2-einen-widerspruch-muendlich-begruenden'
  },
  {
    slug: 'c2-mehrdeutigen-bescheid-klaeren',
    topic: { de: 'einen mehrdeutigen Bescheid', en: 'an ambiguous official notice', fa: 'یک نامه یا تصمیم اداری مبهم', ar: 'إشعار أو قرار إداري ملتبس', tr: 'belirsiz bir resmi karar yazısı', ru: 'неоднозначное официальное уведомление', ckb: 'بڕیارنامەیەکی فەرمی ناڕوون', kmr: 'biryarnameyeke fermî ya nezelal', pl: 'niejednoznaczna decyzja urzędowa', ro: 'o decizie oficială ambiguă', sq: 'një vendim zyrtar i paqartë' },
    focus: { de: 'Unklarheit konkret zu benennen, statt den ganzen Bescheid pauschal abzulehnen', en: 'naming the ambiguity concretely instead of rejecting the whole notice wholesale', fa: 'ابهام را دقیق نام ببری، نه اینکه کل نامه اداری را یک‌جا رد کنی', ar: 'تسمية الالتباس تحديدًا بدل رفض الإشعار كله دفعة واحدة', tr: 'belirsizliği somut adlandırmak, tüm yazıyı toptan reddetmemek', ru: 'конкретно назвать неясность, а не отвергать весь документ целиком', ckb: 'ناڕوونییەکە بە وردی ناوببەیت نەک هەموو بڕیارنامەکە بە گشتی ڕەت بکەیتەوە', kmr: 'nezelaliyê bi konkretî nav bikî ne ku hemû belgeyê bi giştî red bikî', pl: 'konkretnie nazwać niejasność, zamiast odrzucać całą decyzję', ro: 'să numești concret ambiguitatea, nu să respingi întreaga decizie', sq: 'ta emërtosh konkretisht paqartësinë, jo ta refuzosh të gjithë vendimin' },
    grammar: 'c2-ambiguity-and-disambiguation',
    targetType: 'roleplay',
    targetSlug: 'c2-einen-mehrdeutigen-bescheid-klaeren'
  },
  {
    slug: 'c2-verfahrensdauer-beschwerde-vortragen',
    topic: { de: 'eine Beschwerde ueber Verfahrensdauer', en: 'a complaint about processing time', fa: 'شکایت درباره طولانی شدن روند رسیدگی', ar: 'شكوى بشأن طول مدة الإجراء', tr: 'işlem süresinin uzamasıyla ilgili şikayet', ru: 'жалоба на длительность процедуры', ckb: 'سکاڵا لە درێژبوونەوەی ماوەی پرۆسە', kmr: 'gilî li ser dirêjbûna demê pêvajoyê', pl: 'skarga na długość postępowania', ro: 'o plângere privind durata procedurii', sq: 'ankesë për zgjatjen e procedurës' },
    focus: { de: 'Druck aufzubauen, ohne Drohung, Unterstellung oder persoenlichen Angriff', en: 'building pressure without threats, insinuations, or personal attack', fa: 'فشار لازم را ایجاد کنی، بدون تهدید، اتهام ضمنی یا حمله شخصی', ar: 'بناء ضغط لازم من دون تهديد أو تلميح اتهامي أو هجوم شخصي', tr: 'tehdit, ima ya da kişisel saldırı olmadan gerekli baskıyı kurmak', ru: 'создать необходимое давление без угроз, намеков на вину или личной атаки', ckb: 'فشاری پێویست دروست بکەیت بەبێ هەڕەشە، تۆمەتاندنی ناڕاستەوخۆ یان هێرشی کەسی', kmr: 'zexta pêwîst ava bikî bê gef, îma an êrîşa kesane', pl: 'zbudować potrzebną presję bez groźby, insynuacji lub ataku osobistego', ro: 'să creezi presiune necesară fără amenințare, insinuare sau atac personal', sq: 'të krijosh presionin e nevojshëm pa kërcënim, nënkuptim akuzues ose sulm personal' },
    grammar: 'c2-advanced-punctuation-and-rhythm',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-beschwerde-ueber-verfahrensdauer-vortragen'
  },
  {
    slug: 'c2-datenauskunft-verstaendlich-einfordern',
    topic: { de: 'eine Datenauskunft', en: 'access to personal data', fa: 'درخواست دسترسی به اطلاعات شخصی', ar: 'طلب الحصول على البيانات الشخصية', tr: 'kişisel verilere erişim talebi', ru: 'запрос доступа к персональным данным', ckb: 'داواکاری دەستگەیشتن بە داتای کەسی', kmr: 'daxwaza gihiştinê bi daneyên kesane', pl: 'wniosek o dostęp do danych osobowych', ro: 'o cerere de acces la date personale', sq: 'kërkesë për qasje në të dhëna personale' },
    focus: { de: 'dein Recht verstaendlich zu formulieren und die Antwort praktisch nutzbar zu machen', en: 'formulating your right clearly and making the answer practically usable', fa: 'حق خودت را روشن بیان کنی و پاسخ را طوری بخواهی که واقعاً قابل استفاده باشد', ar: 'صياغة حقك بوضوح وطلب إجابة قابلة للاستخدام عمليًا', tr: 'hakkını açık ifade etmek ve yanıtı pratikte kullanılabilir istemek', ru: 'понятно сформулировать свое право и сделать ответ практически пригодным', ckb: 'مافی خۆت بە ڕوونی دەرببڕیت و وەڵامەکە وەها داوا بکەیت کە بە کرداری بەکاربهێنرێت', kmr: 'mafê xwe bi zelalî bibêjî û bersivê wisa bixwazî ku di pratîkê de bikêr be', pl: 'jasno sformułować swoje prawo i zażądać odpowiedzi praktycznie użytecznej', ro: 'să formulezi clar dreptul tău și să ceri un răspuns utilizabil practic', sq: 'ta formolosh qartë të drejtën tënde dhe ta kërkosh përgjigjen në mënyrë të përdorshme' },
    grammar: 'c2-legal-style-sentence-structures',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-datenauskunft-verstaendlich-einfordern'
  },
  {
    slug: 'c2-formale-regel-und-einzelfall-vermitteln',
    topic: { de: 'eine formale Regel und einen Einzelfall', en: 'a formal rule and an individual case', fa: 'یک قاعده رسمی و یک مورد خاص انسانی', ar: 'قاعدة رسمية وحالة فردية', tr: 'resmi bir kural ve tekil bir durum', ru: 'формальное правило и отдельный случай', ckb: 'یاسایەکی فەرمی و کەیسێکی تایبەت', kmr: 'rêgezake fermî û rewşeke taybet', pl: 'formalna zasada i przypadek indywidualny', ro: 'o regulă formală și un caz individual', sq: 'një rregull formal dhe një rast i veçantë' },
    focus: { de: 'die Regel nicht zu verdrehen und trotzdem den Einzelfall menschlich sichtbar zu machen', en: 'not distorting the rule while still making the individual case humanly visible', fa: 'قاعده را تحریف نکنی و در عین حال جنبه انسانی مورد خاص را نشان بدهی', ar: 'عدم تحريف القاعدة مع إظهار الجانب الإنساني للحالة الفردية', tr: 'kuralı çarpıtmamak ve yine de tekil durumun insani yönünü göstermek', ru: 'не искажать правило и при этом показать человеческую сторону отдельного случая', ckb: 'یاساکە نەشێوێنیت و هەمان کات لایەنی مرۆیی کەیسە تایبەتەکە دیار بکەیت', kmr: 'rêgezê nexapînî û di heman demê de aliyê mirovî yê rewşa taybet xuya bikî', pl: 'nie zniekształcać zasady, a jednocześnie pokazać ludzki wymiar przypadku', ro: 'să nu deformezi regula și totuși să faci vizibilă dimensiunea umană a cazului', sq: 'të mos shtrembërosh rregullin dhe njëkohësisht të tregosh anën njerëzore të rastit' },
    grammar: 'c2-advanced-passive-and-agent-omission',
    targetType: 'roleplay',
    targetSlug: 'c2-zwischen-formaler-regel-und-einzelfall-vermitteln'
  },
  {
    slug: 'c2-medizinische-abwaegung-komplex-besprechen',
    topic: { de: 'eine komplexe medizinische Abwaegung', en: 'a complex medical consideration', fa: 'یک تصمیم‌گیری پیچیده پزشکی', ar: 'موازنة طبية معقدة', tr: 'karmaşık bir tıbbi değerlendirme', ru: 'сложное медицинское взвешивание', ckb: 'هەڵسەنگاندنێکی پزیشکی ئاڵۆز', kmr: 'nirxandineke bijîjkî ya aloz', pl: 'złożona ocena medyczna', ro: 'o evaluare medicală complexă', sq: 'një vlerësim kompleks mjekësor' },
    focus: { de: 'Nutzen, Risiko und Unsicherheit so zu benennen, dass Entscheidung moeglich bleibt', en: 'naming benefit, risk, and uncertainty so that a decision remains possible', fa: 'فایده، ریسک و ابهام را طوری بیان کنی که تصمیم‌گیری همچنان ممکن بماند', ar: 'تسمية الفائدة والمخاطر وعدم اليقين بحيث يبقى القرار ممكنًا', tr: 'yarar, risk ve belirsizliği karar hâlâ mümkün kalacak biçimde adlandırmak', ru: 'назвать пользу, риск и неопределенность так, чтобы решение оставалось возможным', ckb: 'سوود، مەترسی و نادڵنیایی وەها ناوببەیت کە بڕیاردان هێشتا مومکین بێت', kmr: 'sûd, rîsk û nediyarbûnê wisa nav bikî ku biryardan hîn gengaz bimîne', pl: 'nazwać korzyść, ryzyko i niepewność tak, aby decyzja pozostała możliwa', ro: 'să numești beneficiul, riscul și incertitudinea astfel încât decizia să rămână posibilă', sq: 'të emërtosh përfitimin, rrezikun dhe pasigurinë në mënyrë që vendimi të mbetet i mundshëm' },
    grammar: 'c2-subtle-modal-verb-nuance',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-komplexe-medizinische-abwaegung-besprechen'
  },
  {
    slug: 'c2-diagnoseunsicherheit-ansprechen',
    topic: { de: 'Diagnoseunsicherheit', en: 'diagnostic uncertainty', fa: 'ابهام در تشخیص پزشکی', ar: 'عدم اليقين في التشخيص', tr: 'tanı belirsizliği', ru: 'диагностическая неопределенность', ckb: 'نادڵنیایی لە دەستنیشانکردنی نەخۆشی', kmr: 'nediyarbûna teşxîsê', pl: 'niepewność diagnozy', ro: 'incertitudinea diagnosticului', sq: 'pasiguria e diagnozës' },
    focus: { de: 'Unsicherheit offen zu sagen, ohne Vertrauen oder Handlungsfaehigkeit zu zerstoeren', en: 'stating uncertainty openly without destroying trust or ability to act', fa: 'ابهام را صریح بگویی، بدون اینکه اعتماد یا امکان اقدام را از بین ببری', ar: 'قول عدم اليقين بصراحة من دون تدمير الثقة أو القدرة على التصرف', tr: 'belirsizliği açık söylemek ama güveni ya da eylem becerisini yıkmamak', ru: 'открыто сказать о неопределенности, не разрушая доверие или способность действовать', ckb: 'نادڵنیایی بە ڕاشکاوی بڵێیت بەبێ تێکشکاندنی متمانە یان توانای کردار', kmr: 'nediyarbûnê eşkere bibêjî bêyî ku bawerî an karîna çalakiyê hilweşînî', pl: 'otwarcie nazwać niepewność bez niszczenia zaufania lub możliwości działania', ro: 'să spui deschis incertitudinea fără să distrugi încrederea sau capacitatea de acțiune', sq: 'ta thuash hapur pasigurinë pa shkatërruar besimin ose aftësinë për veprim' },
    grammar: 'c2-subtle-modal-verb-nuance',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-diagnoseunsicherheit-ansprechen'
  },
  {
    slug: 'c2-langwierigen-mietkonflikt-moderieren',
    topic: { de: 'einen langwierigen Mietkonflikt', en: 'a long-running rental conflict', fa: 'یک اختلاف طولانی در اجاره یا خانه', ar: 'نزاع إيجار طويل', tr: 'uzun süren bir kira anlaşmazlığı', ru: 'затяжной жилищный конфликт', ckb: 'ناکۆکییەکی درێژخایەنی کرێ', kmr: 'nakokiyeke dirêj a kirê', pl: 'długotrwały konflikt najmu', ro: 'un conflict locativ de lungă durată', sq: 'një konflikt i gjatë qiraje' },
    focus: { de: 'Interessen, Nachweise und naechste Frist zu ordnen, ohne Drohkulisse', en: 'ordering interests, evidence, and next deadline without a threatening backdrop', fa: 'منافع، مدارک و مهلت بعدی را مرتب کنی، بدون ساختن فضای تهدید', ar: 'ترتيب المصالح والأدلة والمهلة التالية من دون أجواء تهديد', tr: 'çıkarları, kanıtları ve sonraki süreyi tehdit havası olmadan düzenlemek', ru: 'упорядочить интересы, доказательства и следующий срок без атмосферы угрозы', ckb: 'بەرژەوەندی، بەڵگە و دواوادی داهاتوو ڕێکبخەیت بەبێ دروستکردنی کەشی هەڕەشە', kmr: 'berjewendî, delîl û dema dawî ya din rêz bikî bêyî ku atmosfera gefê çê bikî', pl: 'uporządkować interesy, dowody i następny termin bez atmosfery groźby', ro: 'să ordonezi interesele, dovezile și următorul termen fără fundal amenințător', sq: 'të rendisësh interesat, provat dhe afatin tjetër pa krijuar atmosferë kërcënimi' },
    grammar: 'c2-legal-style-sentence-structures',
    targetType: 'roleplay',
    targetSlug: 'c2-einen-langwierigen-mietkonflikt-moderieren'
  },
  {
    slug: 'c2-amt-gesundheit-wohnen-und-komplexe-faelle-wiederholen',
    topic: { de: 'Amt, Gesundheit, Wohnen und komplexe Faelle', en: 'administration, health, housing, and complex cases', fa: 'اداره، سلامت، مسکن و پرونده‌های پیچیده', ar: 'الإدارة والصحة والسكن والحالات المعقدة', tr: 'idare, sağlık, konut ve karmaşık vakalar', ru: 'администрация, здоровье, жилье и сложные случаи', ckb: 'دامەزراوە، تەندروستی، نیشتەجێبوون و کەیسە ئاڵۆزەکان', kmr: 'rêveberî, tenduristî, xanî û dozên aloz', pl: 'urząd, zdrowie, mieszkanie i złożone sprawy', ro: 'administrație, sănătate, locuire și cazuri complexe', sq: 'administratë, shëndet, banim dhe raste komplekse' },
    focus: { de: 'Regel, Beleg, menschliche Lage und naechsten Schritt zusammenzufuehren', en: 'bringing rule, evidence, human situation, and next step together', fa: 'قاعده، سند، وضعیت انسانی و قدم بعدی را کنار هم بیاوری', ar: 'جمع القاعدة والدليل والوضع الإنساني والخطوة التالية', tr: 'kuralı, kanıtı, insani durumu ve sonraki adımı bir araya getirmek', ru: 'соединить правило, доказательство, человеческую ситуацию и следующий шаг', ckb: 'یاسا، بەڵگە، دۆخی مرۆیی و هەنگاوی داهاتوو پێکەوە بهێنیت', kmr: 'rêgez, delîl, rewşa mirovî û gava din bigihînî hev', pl: 'połączyć zasadę, dowód, ludzką sytuację i następny krok', ro: 'să aduci împreună regula, dovada, situația umană și pasul următor', sq: 'të bashkosh rregullin, provën, gjendjen njerëzore dhe hapin tjetër' },
    grammar: 'c2-c2-grammar-review-map',
    targetType: 'writing-template',
    targetSlug: 'c2-formale-regel-und-einzelfall-vermitteln'
  }
];

function tr(map) {
  return langs.map(language => ({ language, text: map[language] }));
}

function createActivity(kind, titleKey, instructionMap, targetType, targetSlug, minutes, sortOrder, isRequired = true) {
  return {
    kind,
    title: titles[titleKey].de,
    titleTranslations: tr(titles[titleKey]),
    instruction: instructionMap.de,
    instructionTranslations: tr(instructionMap),
    targetType,
    targetSlug: targetType === 'none' ? null : targetSlug,
    estimatedMinutes: minutes,
    sortOrder,
    isRequired
  };
}

function orientInstruction(item) {
  return {
    de: `Lies die Lesson und kläre, worum es bei ${item.topic.de} genau geht: Recht, Pflicht, Frist, Gesundheit, Geld oder Beziehung. Notiere, welche Information noch fehlt.`,
    en: `Read the lesson and clarify exactly what ${item.topic.en} is about: right, duty, deadline, health, money, or relationship. Note which information is still missing.`,
    fa: `درس را بخوان و روشن کن موضوع ${item.topic.fa} دقیقاً درباره چیست: حق، وظیفه، مهلت، سلامت، پول یا رابطه. یادداشت کن کدام اطلاعات هنوز کم است.`,
    ar: `اقرأ الدرس ووضح بدقة ما يتعلق به ${item.topic.ar}: حق أم واجب أم مهلة أم صحة أم مال أم علاقة. سجّل ما المعلومات التي لا تزال ناقصة.`,
    tr: `Dersi oku ve ${item.topic.tr} konusunun tam olarak neyle ilgili olduğunu netleştir: hak, yükümlülük, süre, sağlık, para ya da ilişki. Hangi bilginin hâlâ eksik olduğunu not et.`,
    ru: `Прочитай урок и уточни, о чем именно тема ${item.topic.ru}: право, обязанность, срок, здоровье, деньги или отношения. Запиши, какой информации еще не хватает.`,
    ckb: `وانەکە بخوێنەوە و ڕوون بکە ${item.topic.ckb} بە وردی پەیوەندی بە چییەوە هەیە: ماف، ئەرک، ماوە، تەندروستی، پارە یان پەیوەندی. بنووسە کام زانیاری هێشتا کەمە.`,
    kmr: `Dersê bixwîne û zelal bike ku mijara ${item.topic.kmr} rast bi çi ve girêdayî ye: maf, erk, dem, tenduristî, pere an têkilî. Binivîse kîjan agahî hîn kêm e.`,
    pl: `Przeczytaj lekcję i ustal dokładnie, czego dotyczy temat ${item.topic.pl}: prawa, obowiązku, terminu, zdrowia, pieniędzy czy relacji. Zapisz, jakiej informacji jeszcze brakuje.`,
    ro: `Citește lecția și clarifică exact despre ce este ${item.topic.ro}: drept, obligație, termen, sănătate, bani sau relație. Notează ce informație lipsește încă.`,
    sq: `Lexoje mësimin dhe qartëso saktësisht për çfarë është ${item.topic.sq}: e drejtë, detyrim, afat, shëndet, para apo marrëdhënie. Shëno cili informacion mungon ende.`
  };
}

function languageInstruction(item) {
  return {
    de: `Oeffne den verlinkten Grammatikpunkt. Suche zwei Formulierungen, mit denen du ${item.focus.de}, ohne die Sache zu dramatisieren oder zu verharmlosen.`,
    en: `Open the linked grammar point. Find two formulations that help you ${item.focus.en}, without dramatizing or minimizing the issue.`,
    fa: `نکته گرامری لینک‌شده را باز کن. دو فرمول‌بندی پیدا کن که کمک کند ${item.focus.fa}، بدون اینکه موضوع را بیش از حد بزرگ یا کوچک کنی.`,
    ar: `افتح نقطة القواعد المرتبطة. ابحث عن صيغتين تساعدانك على أن ${item.focus.ar}، من دون تهويل الموضوع أو التقليل منه.`,
    tr: `Bağlantılı dil bilgisi noktasını aç. Konuyu büyütmeden ya da küçültmeden ${item.focus.tr} için iki ifade bul.`,
    ru: `Открой связанный грамматический пункт. Найди две формулировки, которые помогают ${item.focus.ru}, не драматизируя и не преуменьшая дело.`,
    ckb: `خاڵی ڕێزمانی بەستەرکراو بکەرەوە. دوو داڕشتن بدۆزەرەوە کە یارمەتیت بدەن ${item.focus.ckb}، بەبێ ئەوەی بابەتەکە زۆر گەورە یان بچووک بکەیت.`,
    kmr: `Xala rêzimanê ya girêdayî veke. Du formulasyonan bibîne ku alîkar in ${item.focus.kmr}, bêyî ku mijarê mezin an biçûk bikî.`,
    pl: `Otwórz podlinkowany punkt gramatyczny. Znajdź dwa sformułowania, które pomogą ci ${item.focus.pl}, bez dramatyzowania lub bagatelizowania sprawy.`,
    ro: `Deschide punctul de gramatică legat. Caută două formulări care te ajută să ${item.focus.ro}, fără să dramatizezi sau să minimalizezi situația.`,
    sq: `Hap pikën gramatikore të lidhur. Gjej dy formulime që të ndihmojnë ${item.focus.sq}, pa e dramatizuar ose minimizuar çështjen.`
  };
}

function targetInstruction(item) {
  return {
    de: `Bearbeite die verlinkte Ressource. Pruefe danach, ob du dein Anliegen klar, belegbar und respektvoll vertreten hast. Markiere eine Stelle, an der ein Beleg oder eine Rueckfrage noetig ist.`,
    en: `Work through the linked resource. Then check whether you presented your concern clearly, with evidence, and respectfully. Mark one place where evidence or a follow-up question is needed.`,
    fa: `منبع لینک‌شده را انجام بده. بعد بررسی کن آیا درخواست یا مسئله‌ات را روشن، مستند و محترمانه مطرح کرده‌ای. یک بخش را مشخص کن که در آن سند یا پرسش تکمیلی لازم است.`,
    ar: `اعمل على المورد المرتبط. ثم تحقق هل عرضت مطلبك أو مشكلتك بوضوح وبالدليل وباحترام. حدّد موضعًا يحتاج إلى دليل أو سؤال متابعة.`,
    tr: `Bağlantılı kaynağı çalış. Sonra konunu açık, kanıtlı ve saygılı biçimde ifade edip etmediğini kontrol et. Kanıt ya da ek soru gereken bir yeri işaretle.`,
    ru: `Проработай связанный ресурс. Затем проверь, ясно, доказательно и уважительно ли ты представил свой вопрос. Отметь одно место, где нужен документ или уточняющий вопрос.`,
    ckb: `سەرچاوەی بەستەرکراو کاربکە. پاشان بپشکنە ئایا داواکەت یان کێشەکەت بە ڕوونی، بە بەڵگە و بە ڕێز خستووەتەڕوو. شوێنێک دیاری بکە کە پێویستی بە بەڵگە یان پرسیاری زیاتر هەیە.`,
    kmr: `Çavkaniya girêdayî bixebitîne. Paşê kontrol bike gelo daxwaz an pirsgirêka xwe bi zelalî, bi delîl û bi rêz pêşkêş kirî. Cihê ku delîl an pirsiyareke din pêwîst e nîşan bike.`,
    pl: `Przerób podlinkowany materiał. Potem sprawdź, czy przedstawiłeś sprawę jasno, z dowodem i z szacunkiem. Zaznacz miejsce, w którym potrzebny jest dowód albo pytanie doprecyzowujące.`,
    ro: `Lucrează resursa legată. Apoi verifică dacă ai prezentat solicitarea clar, cu dovadă și respectuos. Marchează un loc unde este nevoie de o dovadă sau de o întrebare de clarificare.`,
    sq: `Puno me burimin e lidhur. Pastaj kontrollo nëse e ke paraqitur kërkesën qartë, me provë dhe me respekt. Shëno një vend ku duhet provë ose pyetje sqaruese.`
  };
}

function transferInstruction() {
  return {
    de: 'Schreibe eine kurze Fallnotiz in vier Teilen: Sachverhalt, Beleg, offene Frage, naechster Schritt. Halte sie so klar, dass eine andere Person damit weiterarbeiten koennte.',
    en: 'Write a short case note in four parts: facts, evidence, open question, next step. Make it clear enough that another person could continue working with it.',
    fa: 'یک یادداشت کوتاه پرونده در چهار بخش بنویس: واقعیت‌ها، سند، پرسش باز، قدم بعدی. آن‌قدر روشن بنویس که شخص دیگری هم بتواند با آن کار را ادامه دهد.',
    ar: 'اكتب ملاحظة قصيرة عن الحالة في أربعة أجزاء: الوقائع، الدليل، السؤال المفتوح، الخطوة التالية. اجعلها واضحة بما يكفي ليتمكن شخص آخر من متابعة العمل بها.',
    tr: 'Dört parçalı kısa bir vaka notu yaz: olgular, kanıt, açık soru, sonraki adım. Başka biri bununla çalışmaya devam edebilecek kadar açık olsun.',
    ru: 'Напиши короткую заметку по делу из четырех частей: факты, доказательство, открытый вопрос, следующий шаг. Сделай ее настолько ясной, чтобы другой человек мог продолжить работу.',
    ckb: 'تێبینییەکی کورتی کەیس لە چوار بەشدا بنووسە: ڕاستییەکان، بەڵگە، پرسیاری کراوە، هەنگاوی داهاتوو. ئەوەندە ڕوونی بکە کە کەسێکی تر بتوانێت کاری پێ بەردەوام بکات.',
    kmr: 'Nîşeyeke kurt a dozê di çar beşan de binivîse: rastî, delîl, pirsa vekirî, gava din. Wisa zelal bike ku kesekî din bikare pê karê bidomîne.',
    pl: 'Napisz krótką notatkę sprawy w czterech częściach: fakty, dowód, otwarte pytanie, następny krok. Zrób ją tak jasno, aby inna osoba mogła z nią dalej pracować.',
    ro: 'Scrie o notă scurtă de caz în patru părți: fapte, dovadă, întrebare deschisă, pasul următor. Fă-o suficient de clară încât altcineva să poată continua lucrul cu ea.',
    sq: 'Shkruaj një shënim të shkurtër çështjeje në katër pjesë: faktet, prova, pyetja e hapur, hapi tjetër. Bëje aq të qartë sa një person tjetër të mund të vazhdojë punën me të.'
  };
}

function reviewInstruction(item) {
  return {
    de: `Pruefe deine Fallnotiz: Ist bei ${item.topic.de} erkennbar, was gesichert ist und was nur vermutet wird? Entferne jede Formulierung, die mehr verspricht, als du belegen kannst.`,
    en: `Check your case note: in ${item.topic.en}, is it clear what is confirmed and what is only assumed? Remove any formulation that promises more than you can prove.`,
    fa: `یادداشت پرونده‌ات را بررسی کن: آیا در موضوع ${item.topic.fa} روشن است چه چیزی قطعی است و چه چیزی فقط حدس است؟ هر عبارتی را که بیشتر از سندهایت ادعا می‌کند حذف کن.`,
    ar: `افحص ملاحظة الحالة: هل يظهر في موضوع ${item.topic.ar} ما هو مؤكد وما هو مجرد افتراض؟ احذف كل صياغة تعد بأكثر مما تستطيع إثباته.`,
    tr: `Vaka notunu kontrol et: ${item.topic.tr} konusunda neyin kesin, neyin sadece varsayım olduğu anlaşılıyor mu? Kanıtlayabileceğinden fazlasını vaat eden her ifadeyi çıkar.`,
    ru: `Проверь заметку по делу: в теме ${item.topic.ru} ясно ли, что подтверждено, а что только предполагается? Убери любую формулировку, которая обещает больше, чем ты можешь доказать.`,
    ckb: `تێبینیی کەیسەکەت بپشکنە: ئایا لە بابەتی ${item.topic.ckb} ڕوونە چی پشتڕاستکراوە و چی تەنها گومانە؟ هەر دەربڕینێک بسڕەوە کە زیاتر لەوەی بەڵگەی بۆ هەیە بەڵێن دەدات.`,
    kmr: `Nîşeya doza xwe kontrol bike: di mijara ${item.topic.kmr} de xuya ye ka çi piştrast e û çi tenê texmîn e? Her gotinek ku zêdetir ji delîlên te soz dide jê bibe.`,
    pl: `Sprawdź notatkę sprawy: czy przy temacie ${item.topic.pl} widać, co jest potwierdzone, a co tylko przypuszczalne? Usuń każde sformułowanie, które obiecuje więcej, niż możesz udowodnić.`,
    ro: `Verifică nota de caz: în tema ${item.topic.ro}, este clar ce este confirmat și ce este doar presupus? Elimină orice formulare care promite mai mult decât poți dovedi.`,
    sq: `Kontrollo shënimin e çështjes: në temën ${item.topic.sq}, a është e qartë çfarë është e vërtetuar dhe çfarë është vetëm supozim? Hiq çdo formulim që premton më shumë sesa mund të provosh.`
  };
}

for (const item of items) {
  const lesson = lessons.find(l => l.slug === item.slug);
  if (!lesson) {
    throw new Error(`Lesson not found: ${item.slug}`);
  }

  lesson.activityBlocks = [
    createActivity('read', 'orient', orientInstruction(item), 'none', null, 6, 10),
    createActivity('grammar', 'language', languageInstruction(item), 'grammar-topic', item.grammar, 8, 20),
    createActivity(item.targetType === 'writing-template' ? 'write' : 'roleplay', 'target', targetInstruction(item), item.targetType, item.targetSlug, 10, 30),
    createActivity('practice', 'transfer', transferInstruction(), 'none', null, 8, 40),
    createActivity('review', 'review', reviewInstruction(item), 'none', null, 6, 50)
  ];
}

fs.writeFileSync(file, JSON.stringify(data, null, 2) + '\n', 'utf8');
console.log(`Updated ${items.length} C2 Module 8 lessons with ${items.length * 5} activity blocks.`);

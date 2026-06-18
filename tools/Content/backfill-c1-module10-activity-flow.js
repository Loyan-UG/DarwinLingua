const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c1-souveraen-in-studium-beruf-und-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'Kommunikative Lage klaeren',
    en: 'Clarify the communicative situation',
    fa: 'وضعیت ارتباطی را روشن کن',
    ar: 'وضّح الموقف التواصلي',
    tr: 'İletişim durumunu netleştir',
    ru: 'Проясни коммуникативную ситуацию',
    ckb: 'دۆخی پەیوەندیکردن ڕوون بکەوە',
    kmr: 'Rewşa ragihandinê zelal bike',
    pl: 'Wyjaśnij sytuację komunikacyjną',
    ro: 'Clarifică situația comunicativă',
    sq: 'Qartëso situatën komunikuese'
  },
  language: {
    de: 'Indirektheit steuern',
    en: 'Control indirectness',
    fa: 'غیرمستقیم‌گویی را کنترل کن',
    ar: 'تحكّم في التعبير غير المباشر',
    tr: 'Dolaylılığı yönet',
    ru: 'Управляй косвенностью',
    ckb: 'ناڕاستەوخۆ گوتن کۆنترۆڵ بکە',
    kmr: 'Ne-rasterastbûnê birêve bibe',
    pl: 'Steruj pośredniością',
    ro: 'Controlează exprimarea indirectă',
    sq: 'Drejto të shprehurit të tërthortë'
  },
  material: {
    de: 'Situation praktisch trainieren',
    en: 'Practise the situation',
    fa: 'موقعیت را عملی تمرین کن',
    ar: 'تدرّب عمليًا على الموقف',
    tr: 'Durumu pratikte çalış',
    ru: 'Практически отработай ситуацию',
    ckb: 'دۆخەکە بە کرداری ڕاهێنان بکە',
    kmr: 'Rewşê bi pratîkî perwerde bike',
    pl: 'Przećwicz sytuację praktycznie',
    ro: 'Exersează situația practic',
    sq: 'Ushtro situatën praktikisht'
  },
  apply: {
    de: 'Feine Reaktion formulieren',
    en: 'Formulate a careful response',
    fa: 'یک واکنش سنجیده بنویس',
    ar: 'صغ ردًا مدروسًا',
    tr: 'İncelikli bir tepki formüle et',
    ru: 'Сформулируй деликатную реакцию',
    ckb: 'وەڵامێکی پێکهاتوو داڕێژە',
    kmr: 'Bersiveke hûrgilî formule bike',
    pl: 'Sformułuj wyważoną reakcję',
    ro: 'Formulează o reacție atentă',
    sq: 'Formulo një reagim të kujdesshëm'
  },
  review: {
    de: 'Wirkung pruefen',
    en: 'Check the effect',
    fa: 'اثر جمله‌ها را بررسی کن',
    ar: 'راجع أثر العبارات',
    tr: 'Etkisini kontrol et',
    ru: 'Проверь эффект высказываний',
    ckb: 'کاریگەریی ڕستەکان بپشکنە',
    kmr: 'Bandora hevokan kontrol bike',
    pl: 'Sprawdź efekt wypowiedzi',
    ro: 'Verifică efectul formulărilor',
    sq: 'Kontrollo efektin e fjalive'
  }
};

const items = [
  {
    slug: 'c1-indirekte-kritik-richtig-ansprechen',
    topic: { de: 'indirekte Kritik richtig ansprechen', en: 'addressing indirect criticism correctly', fa: 'درست مطرح کردن انتقاد غیرمستقیم', ar: 'التعامل الصحيح مع النقد غير المباشر', tr: 'dolaylı eleştiriyi doğru ele alma', ru: 'правильное обсуждение косвенной критики', ckb: 'باش باسکردنی ڕەخنەی ناڕاستەوخۆ', kmr: 'rast destnîşankirina rexneya ne-rasterast', pl: 'właściwe omówienie pośredniej krytyki', ro: 'abordarea corectă a criticii indirecte', sq: 'trajtimi i drejtë i kritikës së tërthortë' },
    focus: { de: 'eine Andeutung erkennen, ohne sofort Abwehr oder Gegenangriff auszulösen', en: 'recognizing a hint without immediately triggering defensiveness or counterattack', fa: 'اشاره انتقادی را بفهمی، بدون اینکه فوراً حالت دفاعی یا حمله متقابل ایجاد کنی', ar: 'فهم التلميح النقدي من دون إثارة دفاع أو هجوم مضاد فورًا', tr: 'bir imayı fark edip hemen savunma veya karşı saldırı yaratmamayı', ru: 'распознавать намек, не вызывая сразу защиту или контратаку', ckb: 'ئاماژەی ڕەخنەیی تێبگەیت، بەبێ ئەوەی دەستبەجێ بەرگری یان هێرشی بەرامبەر دروست بکەیت', kmr: 'têgihiştina îşareteke rexneyî bêyî ku yekser parastin an dijêrîş çêbibe', pl: 'rozpoznanie aluzji krytycznej bez natychmiastowego wywołania obrony lub kontrataku', ro: 'recunoașterea unei aluzii critice fără a declanșa imediat defensivă sau contraatac', sq: 'të dallosh një nënkuptim kritik pa shkaktuar menjëherë mbrojtje ose kundërsulm' },
    grammar: ['c1-hedging-and-cautious-language', 'vorsichtige Sprache'],
    target: ['roleplay', 'c1-eine-indirekte-kritik-richtig-ansprechen']
  },
  {
    slug: 'c1-zwischen-ehrlichkeit-und-hoeflichkeit-abwaegen',
    topic: { de: 'zwischen Ehrlichkeit und Hoeflichkeit abwaegen', en: 'balancing honesty and politeness', fa: 'سنجیدن صداقت و ادب در کنار هم', ar: 'الموازنة بين الصراحة واللطف', tr: 'dürüstlük ile nezaketi dengeleme', ru: 'взвешивание честности и вежливости', ckb: 'هاوسەنگکردنی ڕاستگۆیی و ڕێزگرتن', kmr: 'hevsengkirina rastbêjî û nezaketê', pl: 'ważenie szczerości i uprzejmości', ro: 'cântărirea sincerității și politeții', sq: 'peshimi i sinqeritetit dhe mirësjelljes' },
    focus: { de: 'klar bleiben, ohne die Beziehung unnoetig zu belasten', en: 'remaining clear without unnecessarily straining the relationship', fa: 'روشن حرف بزنی، بدون اینکه رابطه را بی‌دلیل سنگین یا آسیب‌پذیر کنی', ar: 'البقاء واضحًا من دون تحميل العلاقة توترًا غير ضروري', tr: 'ilişkiyi gereksiz zorlamadan açık kalmayı', ru: 'оставаться ясным, не создавая лишнего напряжения в отношениях', ckb: 'ڕوون بمێنیتەوە، بەبێ ئەوەی پەیوەندییەکە بەبێ پێویست قورس بکەیت', kmr: 'zelal mayîn bê ku têkiliyê bêhewce giran bikî', pl: 'pozostanie jasnym bez niepotrzebnego obciążania relacji', ro: 'a rămâne clar fără a tensiona inutil relația', sq: 'të mbetesh i qartë pa e rënduar marrëdhënien pa nevojë' },
    grammar: ['c1-konjunktiv-i-versus-konjunktiv-ii', 'hoefliche Distanz'],
    target: ['roleplay', 'c1-zwischen-ehrlichkeit-und-hoeflichkeit-abwaegen']
  },
  {
    slug: 'c1-peinliche-situation-taktvoll-reparieren',
    topic: { de: 'eine peinliche Situation taktvoll reparieren', en: 'repairing an awkward situation tactfully', fa: 'ترمیم محترمانه یک موقعیت خجالت‌آور', ar: 'إصلاح موقف محرج بلباقة', tr: 'utandırıcı bir durumu incelikle onarma', ru: 'тактичное исправление неловкой ситуации', ckb: 'چاککردنەوەی دۆخێکی شەرمەزار بە ڕێزەوە', kmr: 'bi taktîk rewşeke şermokî sererast kirin', pl: 'taktowne naprawienie niezręcznej sytuacji', ro: 'repararea cu tact a unei situații jenante', sq: 'rregullimi me takt i një situate të sikletshme' },
    focus: { de: 'Fehler benennen, Verantwortung zeigen und den anderen nicht blossstellen', en: 'naming the mistake, showing responsibility, and not exposing the other person', fa: 'اشتباه را نام ببری، مسئولیت نشان بدهی و طرف مقابل را در برابر دیگران کوچک نکنی', ar: 'تسمية الخطأ وإظهار المسؤولية من دون إحراج الطرف الآخر أمام الآخرين', tr: 'hatayı adlandırıp sorumluluk göstermeyi ve diğer kişiyi mahcup etmemeyi', ru: 'назвать ошибку, показать ответственность и не выставлять другого человека в неловком положении', ckb: 'هەڵەکە ناو ببەیت، بەرپرسیاریەتی نیشان بدەیت و کەسی بەرامبەر شەرمەزار نەکەیت', kmr: 'şaşitiyê nav kirin, berpirsiyarî nîşan dan û kesê din rezîl nekirin', pl: 'nazwanie błędu, pokazanie odpowiedzialności i nieośmieszanie drugiej osoby', ro: 'numirea greșelii, asumarea responsabilității și evitarea expunerii celuilalt', sq: 'të përmendësh gabimin, të tregosh përgjegjësi dhe të mos e vësh tjetrin në siklet' },
    grammar: ['c1-hedging-and-cautious-language', 'vorsichtige Sprache'],
    target: ['roleplay', 'c1-eine-peinliche-situation-taktvoll-reparieren']
  },
  {
    slug: 'c1-kulturellen-konflikt-nicht-eskalieren',
    topic: { de: 'einen kulturellen Konflikt nicht eskalieren lassen', en: 'preventing a cultural conflict from escalating', fa: 'جلوگیری از تشدید یک سوءتفاهم فرهنگی', ar: 'منع تصعيد خلاف ثقافي', tr: 'kültürel bir çatışmanın tırmanmasını önleme', ru: 'предотвращение эскалации культурного конфликта', ckb: 'ڕێگرتن لە توندبوونەوەی ناکۆکییەکی کولتووری', kmr: 'nehiştina bilindbûna nakokiyeke çandî', pl: 'niedopuszczenie do eskalacji konfliktu kulturowego', ro: 'prevenirea escaladării unui conflict cultural', sq: 'parandalimi i përshkallëzimit të një konflikti kulturor' },
    focus: { de: 'Deutung, Gewohnheit und persoenliche Absicht getrennt behandeln', en: 'treating interpretation, habit, and personal intention separately', fa: 'برداشت، عادت فرهنگی و نیت شخصی را جدا از هم بررسی کنی', ar: 'التعامل مع التفسير والعادة والنية الشخصية كلٌ على حدة', tr: 'yorum, alışkanlık ve kişisel niyeti ayrı ele almayı', ru: 'отдельно рассматривать интерпретацию, привычку и личное намерение', ckb: 'لێکدانەوە، خووی کولتووری و نیازی کەسی جیا جیا مامەڵە بکەیت', kmr: 'şîrove, adet û niyeta kesane cuda cuda dest girtin', pl: 'oddzielne traktowanie interpretacji, nawyku i osobistej intencji', ro: 'tratarea separată a interpretării, obiceiului și intenției personale', sq: 'trajtimin veçmas të interpretimit, zakonit dhe qëllimit personal' },
    grammar: ['c1-expressing-nuance-and-limitation', 'Nuancierung und Begrenzung'],
    target: ['roleplay', 'c1-einen-kulturellen-konflikt-nicht-eskalieren-lassen']
  },
  {
    slug: 'c1-umgangssprache-und-formelle-distanz',
    topic: { de: 'Umgangssprache und formelle Distanz', en: 'colloquial language and formal distance', fa: 'زبان محاوره‌ای و فاصله رسمی', ar: 'اللغة اليومية والمسافة الرسمية', tr: 'gündelik dil ve resmî mesafe', ru: 'разговорная речь и официальная дистанция', ckb: 'زمانی ڕۆژانە و دووری فەرمی', kmr: 'zimana rojane û dûrbûna fermî', pl: 'język potoczny i formalny dystans', ro: 'limbaj colocvial și distanță formală', sq: 'gjuha e përditshme dhe distanca formale' },
    focus: { de: 'Nahe, locker und respektvoll unterscheiden, bevor du das Register wechselst', en: 'distinguishing closeness, informality, and respect before changing register', fa: 'قبل از تغییر لحن، صمیمیت، خودمانی بودن و احترام را از هم جدا کنی', ar: 'التمييز بين القرب والعفوية والاحترام قبل تغيير السجل اللغوي', tr: 'register değiştirmeden önce yakınlık, rahatlık ve saygıyı ayırmayı', ru: 'различать близость, непринужденность и уважение перед сменой регистра', ckb: 'پێش گۆڕینی تۆن، نزیکی، ئاسانی و ڕێزگرتن جیا بکەیتەوە', kmr: 'berî guhertina registerê nêzîkî, rehetî û rêzgirtinê cuda kirin', pl: 'odróżnienie bliskości, swobody i szacunku przed zmianą rejestru', ro: 'diferențierea apropierii, lejerității și respectului înainte de schimbarea registrului', sq: 'ndarjen e afërsisë, lirshmërisë dhe respektit para ndryshimit të regjistrit' },
    grammar: ['c1-register-shifting', 'Registerwechsel'],
    target: ['writing-template', 'c1-registerwechsel-in-email-kette-steuern']
  },
  {
    slug: 'c1-ironische-und-implizite-signale',
    topic: { de: 'ironische und implizite Signale', en: 'ironic and implicit signals', fa: 'نشانه‌های کنایه‌آمیز و پنهان', ar: 'الإشارات الساخرة والضمنية', tr: 'ironik ve örtük işaretler', ru: 'иронические и скрытые сигналы', ckb: 'نیشانەی گاڵتەئامێز و ناڕاستەوخۆ', kmr: 'nîşanên ironîk û ne-vekirî', pl: 'sygnały ironiczne i ukryte', ro: 'semnale ironice și implicite', sq: 'sinjale ironike dhe të nënkuptuara' },
    focus: { de: 'Ton, Kontext und Beziehung pruefen, bevor du eine Aussage woertlich nimmst', en: 'checking tone, context, and relationship before taking a statement literally', fa: 'قبل از اینکه جمله‌ای را کاملاً تحت‌اللفظی بفهمی، لحن، زمینه و رابطه را بررسی کنی', ar: 'فحص النبرة والسياق والعلاقة قبل فهم العبارة حرفيًا', tr: 'bir ifadeyi kelimesi kelimesine almadan önce ton, bağlam ve ilişkiyi kontrol etmeyi', ru: 'проверять тон, контекст и отношения, прежде чем понимать высказывание буквально', ckb: 'پێش ئەوەی ڕستەیەک بە وشەیی وەربگریت، تۆن، چوارچێوە و پەیوەندی بپشکنیت', kmr: 'berî ku gotinekê bi wateya rasterast bigirî, ton, çarçove û têkiliyê kontrol kirin', pl: 'sprawdzenie tonu, kontekstu i relacji, zanim weźmiesz wypowiedź dosłownie', ro: 'verificarea tonului, contextului și relației înainte de a lua afirmația literal', sq: 'kontrollimin e tonit, kontekstit dhe marrëdhënies para se ta marrësh një fjali fjalë për fjalë' },
    grammar: ['c1-indirect-speech-in-journalism-and-formal-contexts', 'indirekte und implizite Sprache'],
    target: ['exam-prep-unit', 'c1-register-in-der-pruefung-anpassen']
  },
  {
    slug: 'c1-gesicht-wahren-und-kritik-platzieren',
    topic: { de: 'Gesicht wahren und Kritik platzieren', en: 'saving face and placing criticism', fa: 'حفظ احترام طرف مقابل و بیان نقد در جای درست', ar: 'حفظ ماء الوجه ووضع النقد في مكانه المناسب', tr: 'itibarı koruyup eleştiriyi doğru yere yerleştirme', ru: 'сохранение лица и уместное размещение критики', ckb: 'پاراستنی ڕێزی کەسی بەرامبەر و دانانی ڕەخنە لە شوێنی گونجاو', kmr: 'parastina rûmetê û danîna rexneyê li cihê rast', pl: 'zachowanie twarzy i właściwe umieszczenie krytyki', ro: 'păstrarea demnității celuilalt și plasarea criticii corect', sq: 'ruajtja e fytyrës dhe vendosja e kritikës në vendin e duhur' },
    focus: { de: 'Kritik so rahmen, dass die Sache klar wird und die Person nicht blossgestellt wird', en: 'framing criticism so the issue becomes clear without exposing the person', fa: 'نقد را طوری قاب‌بندی کنی که موضوع روشن شود و شخص تحقیر یا شرمنده نشود', ar: 'تأطير النقد بحيث تتضح القضية من دون إحراج الشخص أو التقليل منه', tr: 'eleştiriyi konuyu netleştirecek ama kişiyi mahcup etmeyecek biçimde çerçevelemeyi', ru: 'оформлять критику так, чтобы вопрос был ясен, но человек не был выставлен в неловком положении', ckb: 'ڕەخنەکە وا چوارچێوە بدەیت کە بابەتەکە ڕوون بێت و کەسەکە شەرمەزار نەبێت', kmr: 'rexneyê wisa çarçove kirin ku mijar zelal bibe û kes rezîl nebe', pl: 'ramowanie krytyki tak, aby sprawa była jasna, a osoba nie została zawstydzona', ro: 'încadrarea criticii astfel încât problema să fie clară fără a expune persoana', sq: 'kornizimin e kritikës që çështja të bëhet e qartë pa e vënë personin në siklet' },
    grammar: ['c1-hedging-and-cautious-language', 'vorsichtige Sprache'],
    target: ['exam-prep-unit', 'c1-formelle-einwaende-schriftlich-abfedern']
  },
  {
    slug: 'c1-registerwechsel-in-einer-email-kette',
    topic: { de: 'Registerwechsel in einer E-Mail-Kette', en: 'register change in an email thread', fa: 'تغییر لحن در یک زنجیره ایمیلی', ar: 'تغيير السجل اللغوي في سلسلة رسائل بريدية', tr: 'bir e-posta zincirinde register değişimi', ru: 'смена регистра в цепочке электронных писем', ckb: 'گۆڕینی تۆن لە زنجیرەی ئیمەیڵدا', kmr: 'guhertina registerê di zincîra e-nameyan de', pl: 'zmiana rejestru w łańcuchu e-maili', ro: 'schimbarea registrului într-un lanț de e-mailuri', sq: 'ndryshimi i regjistrit në një varg emailesh' },
    focus: { de: 'vom lockeren Austausch zu einer formellen Klaerung wechseln, ohne abrupt oder kalt zu wirken', en: 'moving from informal exchange to formal clarification without sounding abrupt or cold', fa: 'از گفت‌وگوی خودمانی به روشن‌سازی رسمی بروی، بدون اینکه ناگهانی یا سرد به نظر برسی', ar: 'الانتقال من تبادل غير رسمي إلى توضيح رسمي من دون أن يبدو مفاجئًا أو باردًا', tr: 'rahat bir yazışmadan resmî açıklamaya ani veya soğuk görünmeden geçmeyi', ru: 'переходить от неформального обмена к официальному уточнению, не звуча резко или холодно', ckb: 'لە ئاڵوگۆڕی ئاساییەوە بۆ ڕوونکردنەوەی فەرمی بچیت، بەبێ ئەوەی پڕ لە ناکاو یان سارد دەرکەویت', kmr: 'ji danûstandina rehet ber bi zelalkirina fermî ve çûn bêyî ku tund an sar xuya bikî', pl: 'przejście od swobodnej wymiany do formalnego wyjaśnienia bez wrażenia nagłości lub chłodu', ro: 'trecerea de la schimb informal la clarificare formală fără a părea brusc sau rece', sq: 'kalimin nga shkëmbimi i lirshëm te sqarimi formal pa tingëlluar papritur ose ftohtë' },
    grammar: ['c1-register-shifting', 'Registerwechsel'],
    target: ['writing-template', 'c1-registerwechsel-in-email-kette-steuern']
  },
  {
    slug: 'c1-implizite-erwartungen-im-beruf',
    topic: { de: 'implizite Erwartungen im Beruf', en: 'implicit expectations at work', fa: 'انتظارهای نانوشته در محیط کار', ar: 'التوقعات الضمنية في العمل', tr: 'işte örtük beklentiler', ru: 'скрытые ожидания на работе', ckb: 'چاوەڕوانییە نانووسراوەکان لە کاردا', kmr: 'hêviyên ne-vekirî di kar de', pl: 'ukryte oczekiwania w pracy', ro: 'așteptări implicite la locul de muncă', sq: 'pritshmëri të nënkuptuara në punë' },
    focus: { de: 'unausgesprochene Erwartungen erfragen, ohne Unsicherheit oder Vorwurf zu signalisieren', en: 'asking about unspoken expectations without signaling insecurity or accusation', fa: 'انتظارهای گفته‌نشده را بپرسی، بدون اینکه ناامنی یا سرزنش منتقل کنی', ar: 'السؤال عن التوقعات غير المعلنة من دون إظهار عدم ثقة أو اتهام', tr: 'söylenmemiş beklentileri güvensizlik veya suçlama hissettirmeden sormayı', ru: 'спрашивать о невысказанных ожиданиях, не показывая неуверенность или обвинение', ckb: 'پرسیار لە چاوەڕوانییە نەگوتراوەکان بکەیت، بەبێ نیشاندانی نادڵنیایی یان تۆمەت', kmr: 'li hêviyên negotî pirsîn bêyî nîşandana bêewlehî an sûcdarî', pl: 'pytanie o niewypowiedziane oczekiwania bez sygnalizowania niepewności lub zarzutu', ro: 'întrebarea despre așteptări nespuse fără a semnala nesiguranță sau acuzație', sq: 'të pyesësh për pritshmëri të pathëna pa sinjalizuar pasiguri ose akuzë' },
    grammar: ['c1-register-shifting', 'Registerwechsel'],
    target: ['roleplay', 'c1-berufssprache-c1-konfliktmoderation-im-team']
  },
  {
    slug: 'c1-pragmatik-implizites-und-registerwechsel-wiederholen',
    topic: { de: 'Pragmatik, Implizites und Registerwechsel wiederholen', en: 'reviewing pragmatics, implicit meaning, and register change', fa: 'مرور کاربرد زبان، معناهای پنهان و تغییر لحن', ar: 'مراجعة التداولية والمعاني الضمنية وتغيير السجل', tr: 'pragmatik, örtük anlam ve register değişimini tekrar etme', ru: 'повторение прагматики, скрытого смысла и смены регистра', ckb: 'دووبارەکردنەوەی پراگماتیک، واتای شاراوە و گۆڕینی تۆن', kmr: 'dubarekirina pragmatîk, wateya veşartî û guhertina registerê', pl: 'powtórka pragmatyki, znaczeń ukrytych i zmiany rejestru', ro: 'recapitularea pragmaticii, sensurilor implicite și schimbării registrului', sq: 'përsëritja e pragmatikës, kuptimeve të nënkuptuara dhe ndryshimit të regjistrit' },
    focus: { de: 'Ton, Beziehung, Kontext und Ziel gemeinsam pruefen, bevor du reagierst', en: 'checking tone, relationship, context, and goal together before responding', fa: 'قبل از پاسخ دادن، لحن، رابطه، زمینه و هدف را با هم بررسی کنی', ar: 'فحص النبرة والعلاقة والسياق والهدف معًا قبل الرد', tr: 'yanıt vermeden önce ton, ilişki, bağlam ve hedefi birlikte kontrol etmeyi', ru: 'проверять тон, отношения, контекст и цель вместе перед реакцией', ckb: 'پێش وەڵامدانەوە، تۆن، پەیوەندی، چوارچێوە و ئامانج پێکەوە بپشکنیت', kmr: 'berî bersivdanê ton, têkilî, çarçove û armancê bi hev re kontrol kirin', pl: 'sprawdzenie tonu, relacji, kontekstu i celu razem przed reakcją', ro: 'verificarea împreună a tonului, relației, contextului și scopului înainte de răspuns', sq: 'kontrollimin bashkë të tonit, marrëdhënies, kontekstit dhe qëllimit para përgjigjes' },
    grammar: ['c1-c1-register-review', 'Register-Review'],
    target: ['exam-prep-unit', 'c1-adressatenbezug-konsequent-halten']
  }
];

function translations(map) {
  return langs.map(language => ({ language, text: map[language] }));
}

function title(key) {
  return {
    title: titles[key].de,
    titleTranslations: translations(titles[key])
  };
}

function orientInstruction(item) {
  return {
    de: `Lies den Lektionstext zu ${item.topic.de} und markiere: Wer spricht mit wem, was wird nur angedeutet, und welche Reaktion waere zu direkt?`,
    en: `Read the lesson text on ${item.topic.en} and mark who is speaking to whom, what is only hinted at, and which response would be too direct.`,
    fa: `متن درس درباره ${item.topic.fa} را بخوان و مشخص کن: چه کسی با چه کسی حرف می‌زند، چه چیزی فقط اشاره شده و کدام واکنش بیش از حد مستقیم است.`,
    ar: `اقرأ نص الدرس عن ${item.topic.ar} وحدد: من يتحدث مع من، ما الذي يُلمّح إليه فقط، وأي رد سيكون مباشرًا أكثر من اللازم.`,
    tr: `${item.topic.tr} konulu ders metnini oku ve işaretle: kim kiminle konuşuyor, ne sadece ima ediliyor, hangi tepki fazla doğrudan olurdu.`,
    ru: `Прочитай текст урока о теме «${item.topic.ru}» и отметь, кто с кем говорит, что только подразумевается и какая реакция была бы слишком прямой.`,
    ckb: `دەقی وانەکە لەسەر ${item.topic.ckb} بخوێنەوە و دیاری بکە: کێ لەگەڵ کێ قسە دەکات، چی تەنها ئاماژە پێکراوە و کام وەڵام زۆر ڕاستەوخۆ دەبێت.`,
    kmr: `Nivîsa dersê li ser ${item.topic.kmr} bixwîne û nîşan bike: kî bi kê re diaxive, çi tenê tê îşaretkirin û kîjan bersiv pir rasterast dibû.`,
    pl: `Przeczytaj tekst lekcji o temacie: ${item.topic.pl} i zaznacz, kto z kim rozmawia, co jest tylko zasugerowane i jaka reakcja byłaby zbyt bezpośrednia.`,
    ro: `Citește textul lecției despre ${item.topic.ro} și marchează cine vorbește cu cine, ce este doar sugerat și ce reacție ar fi prea directă.`,
    sq: `Lexo tekstin e mësimit për ${item.topic.sq} dhe shëno kush flet me kë, çfarë vetëm nënkuptohet dhe cili reagim do të ishte tepër i drejtpërdrejtë.`
  };
}

function languageInstruction(item) {
  return {
    de: `Oeffne ${item.grammar[1]} und sammle drei Formulierungen, mit denen du ${item.focus.de}.`,
    en: `Open the linked language section and collect three formulations that help you with this skill: ${item.focus.en}.`,
    fa: `بخش زبانی لینک‌شده را باز کن و سه عبارت پیدا کن که به این مهارت کمک کند: ${item.focus.fa}.`,
    ar: `افتح القسم اللغوي المرتبط واجمع ثلاث صيغ تساعد على هذه المهارة: ${item.focus.ar}.`,
    tr: `Bağlantılı dil bölümünü aç ve şu beceriye yardım eden üç ifade topla: ${item.focus.tr}.`,
    ru: `Открой связанный языковой раздел и собери три формулировки для этого умения: ${item.focus.ru}.`,
    ckb: `بەشی زمانی بەستەرکراو بکەرەوە و سێ دەربڕین کۆبکەوە کە یارمەتی ئەم توانایە بدات: ${item.focus.ckb}.`,
    kmr: `Beşa zimanê ya girêdayî veke û sê derbirînan kom bike ku alîkarîya vê jêhatinê bikin: ${item.focus.kmr}.`,
    pl: `Otwórz połączoną sekcję językową i zbierz trzy sformułowania wspierające tę umiejętność: ${item.focus.pl}.`,
    ro: `Deschide secțiunea lingvistică legată și adună trei formulări care susțin această abilitate: ${item.focus.ro}.`,
    sq: `Hap seksionin e lidhur gjuhësor dhe mblidh tri formulime që ndihmojnë këtë aftësi: ${item.focus.sq}.`
  };
}

function materialInstruction(item) {
  return {
    de: `Bearbeite das verlinkte Material und achte darauf, wie ${item.focus.de}, ohne auszuweichen oder verletzend zu werden.`,
    en: `Work through the linked material and notice how to practise this skill without evading the issue or sounding hurtful: ${item.focus.en}.`,
    fa: `محتوای لینک‌شده را انجام بده و دقت کن چطور می‌شود این مهارت را تمرین کرد، بدون طفره رفتن از موضوع یا رنجاندن طرف مقابل: ${item.focus.fa}.`,
    ar: `اعمل على المادة المرتبطة وانتبه إلى كيفية تدريب هذه المهارة من دون التهرب من الموضوع أو إيذاء الطرف الآخر: ${item.focus.ar}.`,
    tr: `Bağlantılı materyali çalış ve konudan kaçmadan veya incitici olmadan bu becerinin nasıl uygulanacağına dikkat et: ${item.focus.tr}.`,
    ru: `Проработай связанный материал и обрати внимание, как тренировать этот навык, не уходя от темы и не раня собеседника: ${item.focus.ru}.`,
    ckb: `ماددەی بەستەرکراو کاربکە و سەرنج بدە چۆن ئەم توانایە ڕاهێنان دەکرێت، بەبێ خۆدزینەوە لە بابەتەکە یان ئازاردانی بەرامبەر: ${item.focus.ckb}.`,
    kmr: `Materyala girêdayî bixebite û bala xwe bide ka ev jêhatî çawa tê perwerdekirin bê dûrketin ji mijarê an êşandina kesê din: ${item.focus.kmr}.`,
    pl: `Przerób połączony materiał i zwróć uwagę, jak ćwiczyć tę umiejętność bez unikania sprawy i bez ranienia rozmówcy: ${item.focus.pl}.`,
    ro: `Lucrează cu materialul legat și observă cum poți exersa această abilitate fără a evita subiectul sau a răni interlocutorul: ${item.focus.ro}.`,
    sq: `Puno me materialin e lidhur dhe vëzhgo si mund të ushtrohet kjo aftësi pa iu shmangur çështjes ose pa lënduar tjetrin: ${item.focus.sq}.`
  };
}

function applyInstruction() {
  return {
    de: 'Formuliere zwei Varianten: eine zu direkte Reaktion und eine feinere Version. Erklaere kurz, warum die zweite besser wirkt.',
    en: 'Formulate two versions: one overly direct response and one more careful version. Briefly explain why the second works better.',
    fa: 'دو نسخه بنویس: یک واکنش بیش از حد مستقیم و یک نسخه سنجیده‌تر. کوتاه توضیح بده چرا نسخه دوم بهتر اثر می‌گذارد.',
    ar: 'اكتب نسختين: ردًا مباشرًا أكثر من اللازم ونسخة أكثر دقة. اشرح بإيجاز لماذا تبدو النسخة الثانية أفضل.',
    tr: 'İki sürüm yaz: fazla doğrudan bir tepki ve daha incelikli bir sürüm. İkincisinin neden daha iyi etki ettiğini kısaca açıkla.',
    ru: 'Сформулируй два варианта: слишком прямую реакцию и более деликатную версию. Кратко объясни, почему второй вариант действует лучше.',
    ckb: 'دوو وەشان بنووسە: وەڵامێکی زۆر ڕاستەوخۆ و وەشانێکی پێکهاتووتر. بە کورتی ڕوون بکەوە بۆچی دووەم باشتر کاریگەری دەکات.',
    kmr: 'Du guhertoyan binivîse: bersiveke pir rasterast û guhertoyeke hûrgilîtir. Bi kurtî şîrove bike çima ya duyem baştir bandor dike.',
    pl: 'Napisz dwie wersje: zbyt bezpośrednią reakcję i bardziej wyważoną wersję. Krótko wyjaśnij, dlaczego druga działa lepiej.',
    ro: 'Formulează două variante: o reacție prea directă și o versiune mai atentă. Explică pe scurt de ce a doua funcționează mai bine.',
    sq: 'Shkruaj dy versione: një reagim tepër të drejtpërdrejtë dhe një version më të kujdesshëm. Shpjego shkurt pse i dyti funksionon më mirë.'
  };
}

function reviewInstruction() {
  return {
    de: 'Pruefe deine Formulierungen mit vier Fragen: Ist die Sache klar? Bleibt die Beziehung stabil? Passt das Register? Wird nichts Wichtiges nur halb gesagt?',
    en: 'Check your formulations with four questions: Is the issue clear? Does the relationship remain stable? Does the register fit? Is anything important only half-said?',
    fa: 'عبارت‌هایت را با چهار پرسش بررسی کن: موضوع روشن است؟ رابطه آسیب نمی‌بیند؟ لحن مناسب است؟ چیز مهمی نیمه‌کاره یا مبهم گفته نشده؟',
    ar: 'راجع صيغك بأربعة أسئلة: هل القضية واضحة؟ هل تبقى العلاقة مستقرة؟ هل السجل اللغوي مناسب؟ هل تُرك أمر مهم نصف واضح؟',
    tr: 'İfadelerini dört soruyla kontrol et: Konu açık mı? İlişki dengede kalıyor mu? Register uygun mu? Önemli bir şey yarım söylenmiş kalıyor mu?',
    ru: 'Проверь формулировки четырьмя вопросами: ясна ли суть? Остаются ли отношения стабильными? Подходит ли регистр? Не осталось ли важное сказанным наполовину?',
    ckb: 'دەربڕینەکانت بە چوار پرسیار بپشکنە: بابەتەکە ڕوونە؟ پەیوەندییەکە جێگیر دەمێنێتەوە؟ تۆنەکە گونجاوە؟ هیچ شتێکی گرنگ نیوەچڵ نەماوەتەوە؟',
    kmr: 'Derbirînên xwe bi çar pirsan kontrol bike: mijar zelal e? Têkilî aram dimîne? Register guncan e? Tiştekî girîng nîvco nehatiye gotin?',
    pl: 'Sprawdź swoje sformułowania czterema pytaniami: Czy sprawa jest jasna? Czy relacja pozostaje stabilna? Czy rejestr pasuje? Czy nic ważnego nie zostało powiedziane tylko połowicznie?',
    ro: 'Verifică formulările cu patru întrebări: Este clară problema? Rămâne relația stabilă? Se potrivește registrul? Nu a rămas ceva important spus doar pe jumătate?',
    sq: 'Kontrollo formulimet me katër pyetje: A është e qartë çështja? A mbetet marrëdhënia e qëndrueshme? A përshtatet regjistri? A ka mbetur diçka e rëndësishme e thënë vetëm përgjysmë?'
  };
}

function activity(kind, titleKey, instructionMap, targetType, targetSlug, sortOrder, estimatedMinutes, isRequired = true) {
  return {
    kind,
    ...title(titleKey),
    instruction: instructionMap.de,
    instructionTranslations: translations(instructionMap),
    targetType,
    targetSlug,
    estimatedMinutes,
    sortOrder,
    isRequired
  };
}

for (const item of items) {
  const lesson = lessons.find(candidate => candidate.slug === item.slug);
  if (!lesson) {
    throw new Error(`Lesson not found: ${item.slug}`);
  }

  lesson.activityBlocks = [
    activity('read', 'orient', orientInstruction(item), 'none', null, 10, 6),
    activity('grammar', 'language', languageInstruction(item), 'grammar-topic', item.grammar[0], 20, 8),
    activity(item.target[0] === 'writing-template' ? 'write' : item.target[0] === 'exam-prep-unit' ? 'exam-prep' : 'roleplay', 'material', materialInstruction(item), item.target[0], item.target[1], 30, 10),
    activity('practice', 'apply', applyInstruction(), 'none', null, 40, 7),
    activity('review', 'review', reviewInstruction(), 'none', null, 50, 5)
  ];
}

fs.writeFileSync(file, `${JSON.stringify(data, null, 2)}\n`);
console.log('Updated 10 C1 Module 10 lessons with 50 activity blocks.');

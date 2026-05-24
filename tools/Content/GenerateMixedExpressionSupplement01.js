#!/usr/bin/env node

const fs = require("fs");
const path = require("path");

const outputPath = "content/learning-portal/expressions/packages/expressions-mixed-supplement-01-v1.json";
const languages = ["en", "fa", "ar", "tr", "ru", "ckb", "kmr", "pl", "ro", "sq"];

const warning = {
  warningType: "tone",
  text: "This expression can sound personal or socially sensitive. Use it only when the relationship and situation make that tone safe.",
  translations: [
    { language: "en", text: "This expression can sound personal or socially sensitive. Use it only when the relationship and situation make that tone safe." },
    { language: "fa", text: "این عبارت می‌تواند شخصی یا از نظر اجتماعی حساس به نظر برسد. فقط وقتی رابطه و موقعیت چنین لحنی را امن می‌کند از آن استفاده کن." },
    { language: "ar", text: "قد تبدو هذه العبارة شخصية أو حساسة اجتماعيًا. استخدمها فقط عندما تسمح العلاقة والموقف بهذه النبرة." },
    { language: "tr", text: "Bu ifade kişisel ya da sosyal açıdan hassas duyulabilir. Sadece ilişki ve durum bu tona uygunsa kullan." },
    { language: "ru", text: "Это выражение может звучать лично или социально чувствительно. Используйте его только там, где отношения и ситуация допускают такой тон." },
    { language: "ckb", text: "ئەم دەستەواژەیە دەتوانێت کەسی یان لە ڕووی کۆمەڵایەتییەوە هەستیار دەربکەوێت. تەنها کاتێک بەکاری بهێنە کە پەیوەندی و دۆخەکە ئەو لەحنە قبووڵ دەکات." },
    { language: "kmr", text: "Ev gotin dikare kesane an ji aliyê civakî ve hestyar xuya bike. Tenê dema têkilî û rewş ev awazê ewle dikin bi kar bîne." },
    { language: "pl", text: "To wyrażenie może brzmieć osobiście albo społecznie wrażliwie. Używaj go tylko wtedy, gdy relacja i sytuacja pozwalają na taki ton." },
    { language: "ro", text: "Această expresie poate suna personal sau sensibil social. Folosește-o doar când relația și situația permit acest ton." },
    { language: "sq", text: "Kjo shprehje mund të tingëllojë personale ose shoqërisht e ndjeshme. Përdore vetëm kur marrëdhënia dhe situata e lejojnë këtë ton." }
  ]
};

const entries = [
  {
    slug: "hals-und-beinbruch",
    text: "Hals- und Beinbruch!",
    level: "A2",
    type: "cultural-phrase",
    transparency: "non-literal",
    register: "colloquial",
    category: "social-and-relationships",
    topics: ["social-and-relationships", "everyday-life"],
    literal: {
      en: "Neck and leg break.",
      fa: "گردن و پا بشکند.",
      ar: "كسر في الرقبة والساق.",
      tr: "Boyun ve bacak kırılsın.",
      ru: "Сломай шею и ногу.",
      ckb: "مل و قاچ بشکێت.",
      kmr: "Stû û ling bişikê.",
      pl: "Złam kark i nogę.",
      ro: "Gât și picior rupt.",
      sq: "Qafë dhe këmbë e thyer."
    },
    actual: {
      en: "Good luck.",
      fa: "موفق باشی؛ برایت شانس می‌خواهم.",
      ar: "حظًا موفقًا.",
      tr: "Bol şans.",
      ru: "Удачи.",
      ckb: "بەختت یار بێت.",
      kmr: "Şansê te baş be.",
      pl: "Powodzenia.",
      ro: "Mult noroc.",
      sq: "Paç fat."
    },
    usage: {
      en: "A common informal wish before an exam, performance, or difficult task; it sounds odd if translated literally.",
      fa: "قبل از امتحان، اجرا یا کار سخت به صورت غیررسمی گفته می‌شود؛ ترجمهٔ کلمه‌به‌کلمه‌اش گمراه‌کننده است.",
      ar: "تُقال بشكل غير رسمي قبل امتحان أو عرض أو مهمة صعبة؛ ترجمتها الحرفية مضللة.",
      tr: "Sınav, gösteri ya da zor bir işten önce samimi şekilde söylenir; kelime kelime çevirisi yanıltır.",
      ru: "Так неформально желают удачи перед экзаменом, выступлением или трудной задачей; дословный перевод сбивает с толку.",
      ckb: "پێش تاقیکردنەوە، نمایش یان کارێکی قورس بە شێوەی ئاسایی دەگوترێت؛ وەرگێڕانی وشە بە وشەی هەڵەڕێنەرە.",
      kmr: "Berî ezmûn, pêşandan an karekî dijwar bi awayekî nefermî tê gotin; wergera peyv bi peyvê şaş dike.",
      pl: "Mówi się tak nieformalnie przed egzaminem, występem albo trudnym zadaniem; dosłowne tłumaczenie myli.",
      ro: "Se spune informal înainte de un examen, o prezentare sau o sarcină grea; traducerea literală derutează.",
      sq: "Thuhet joformalisht para një provimi, shfaqjeje ose detyre të vështirë; përkthimi fjalë për fjalë të ngatërron."
    },
    reason: "It is a conventional German good-luck formula whose real meaning is the opposite of the literal words.",
    examples: [
      {
        de: "Morgen ist deine Prüfung? Hals- und Beinbruch!",
        tr: {
          en: "Your exam is tomorrow? Good luck!",
          fa: "فردا امتحان داری؟ موفق باشی!",
          ar: "امتحانك غدًا؟ حظًا موفقًا!",
          tr: "Sınavın yarın mı? Bol şans!",
          ru: "У тебя завтра экзамен? Удачи!",
          ckb: "سبەی تاقیکردنەوەت هەیە؟ بەختت یار بێت!",
          kmr: "Ezmûna te sibê ye? Şansê te baş be!",
          pl: "Jutro masz egzamin? Powodzenia!",
          ro: "Ai examen mâine? Mult noroc!",
          sq: "E ke provimin nesër? Paç fat!"
        }
      },
      {
        de: "Vor dem Auftritt sagte sie nur: Hals- und Beinbruch!",
        tr: {
          en: "Before the performance she just said: good luck!",
          fa: "قبل از اجرا فقط گفت: موفق باشی!",
          ar: "قبل العرض قالت فقط: حظًا موفقًا!",
          tr: "Gösteriden önce sadece şöyle dedi: bol şans!",
          ru: "Перед выступлением она только сказала: удачи!",
          ckb: "پێش نمایشەکە تەنها گوتی: بەختت یار بێت!",
          kmr: "Berî pêşandanê wê tenê got: şansê te baş be!",
          pl: "Przed występem powiedziała tylko: powodzenia!",
          ro: "Înainte de reprezentație a spus doar: mult noroc!",
          sq: "Para shfaqjes ajo tha vetëm: paç fat!"
        }
      }
    ]
  },
  {
    slug: "toi-toi-toi",
    text: "Toi, toi, toi!",
    level: "A2",
    type: "cultural-phrase",
    transparency: "non-literal",
    register: "colloquial",
    category: "social-and-relationships",
    topics: ["social-and-relationships", "culture-and-media"],
    literal: {
      en: "A ritual sound, not a normal word meaning.",
      fa: "یک صدای آیینی است، نه یک معنی معمولی واژه.",
      ar: "صوت طقوسي وليس كلمة ذات معنى عادي.",
      tr: "Sıradan bir kelime anlamı değil, uğur sesi gibi kullanılır.",
      ru: "Это ритуальный звук, а не обычное значение слова.",
      ckb: "دەنگێکی دابونەریتییە، نە واتای ئاسایی وشە.",
      kmr: "Dengekî kevneşopî ye, ne wateya asayî ya peyvê.",
      pl: "To rytualny dźwięk, a nie zwykłe znaczenie słowa.",
      ro: "Este un sunet ritualic, nu un sens obișnuit al unui cuvânt.",
      sq: "Është një tingull ritual, jo kuptim i zakonshëm fjale."
    },
    actual: {
      en: "Good luck; I hope it goes well.",
      fa: "موفق باشی؛ امیدوارم خوب پیش برود.",
      ar: "حظًا موفقًا؛ آمل أن تسير الأمور جيدًا.",
      tr: "Bol şans; umarım iyi gider.",
      ru: "Удачи; надеюсь, всё пройдет хорошо.",
      ckb: "بەختت یار بێت؛ هیوادارم باش بڕوات.",
      kmr: "Şansê te baş be; hêvî dikim baş derbas bibe.",
      pl: "Powodzenia; mam nadzieję, że pójdzie dobrze.",
      ro: "Mult noroc; sper să meargă bine.",
      sq: "Paç fat; shpresoj të shkojë mirë."
    },
    usage: {
      en: "A short cultural good-luck formula before exams, performances, or important appointments.",
      fa: "فرمول کوتاه فرهنگی برای آرزوی موفقیت قبل از امتحان، اجرا یا قرار مهم است.",
      ar: "صيغة ثقافية قصيرة للتمني بالتوفيق قبل الامتحانات أو العروض أو المواعيد المهمة.",
      tr: "Sınav, gösteri ya da önemli randevu öncesi kısa bir kültürel iyi dilek ifadesidir.",
      ru: "Короткая культурная формула удачи перед экзаменами, выступлениями или важными встречами.",
      ckb: "فۆرمولێکی کورتە بۆ هیواخواستنی سەرکەوتن پێش تاقیکردنەوە، نمایش یان چاوپێکەوتنی گرنگ.",
      kmr: "Formûleke çandî ya kurt e ji bo xwestina serkeftinê berî ezmûn, pêşandan an hevdîtineke girîng.",
      pl: "Krótka kulturowa formuła życzenia powodzenia przed egzaminem, występem albo ważnym spotkaniem.",
      ro: "O formulă culturală scurtă pentru urări înainte de examene, prezentări sau întâlniri importante.",
      sq: "Formulë e shkurtër kulturore për urim para provimeve, shfaqjeve ose takimeve të rëndësishme."
    },
    reason: "It is a culturally fixed good-luck formula that cannot be understood through normal vocabulary.",
    examples: [
      {
        de: "Du hast heute dein Vorstellungsgespräch? Toi, toi, toi!",
        tr: {
          en: "You have your job interview today? Good luck!",
          fa: "امروز مصاحبهٔ کاری داری؟ موفق باشی!",
          ar: "لديك مقابلة عمل اليوم؟ حظًا موفقًا!",
          tr: "Bugün iş görüşmen mi var? Bol şans!",
          ru: "У тебя сегодня собеседование? Удачи!",
          ckb: "ئەمڕۆ چاوپێکەوتنی کارت هەیە؟ بەختت یار بێت!",
          kmr: "Îro hevdîtina karê te heye? Şansê te baş be!",
          pl: "Masz dziś rozmowę kwalifikacyjną? Powodzenia!",
          ro: "Ai interviu de angajare azi? Mult noroc!",
          sq: "Ke intervistë pune sot? Paç fat!"
        }
      },
      {
        de: "Vor der Präsentation wünschte das Team ihr: Toi, toi, toi!",
        tr: {
          en: "Before the presentation, the team wished her good luck.",
          fa: "قبل از ارائه، تیم برای او آرزوی موفقیت کرد.",
          ar: "قبل العرض تمنى لها الفريق حظًا موفقًا.",
          tr: "Sunumdan önce ekip ona bol şans diledi.",
          ru: "Перед презентацией команда пожелала ей удачи.",
          ckb: "پێش پێشکەشکردنەکە، تیمەکە هیوازی سەرکەوتنی بۆ خواست.",
          kmr: "Berî pêşkêşkirinê, tîmê jê re serkeftin xwest.",
          pl: "Przed prezentacją zespół życzył jej powodzenia.",
          ro: "Înainte de prezentare, echipa i-a urat mult noroc.",
          sq: "Para prezantimit, ekipi i uroi fat."
        }
      }
    ]
  },
  {
    slug: "ich-sehe-schwarz",
    text: "Ich sehe schwarz.",
    level: "B1",
    type: "idiom",
    transparency: "non-literal",
    register: "neutral",
    category: "planning-and-projects",
    topics: ["planning-and-projects", "work-and-jobs"],
    literal: {
      en: "I see black.",
      fa: "من سیاه می‌بینم.",
      ar: "أرى اللون الأسود.",
      tr: "Siyah görüyorum.",
      ru: "Я вижу черное.",
      ckb: "من ڕەش دەبینم.",
      kmr: "Ez reş dibînim.",
      pl: "Widzę czarno.",
      ro: "Văd negru.",
      sq: "Shoh të zezë."
    },
    actual: {
      en: "I am pessimistic; I think it will probably not work.",
      fa: "بدبینم؛ فکر می‌کنم احتمالاً درست پیش نمی‌رود.",
      ar: "أنا متشائم؛ أظن أن الأمر غالبًا لن ينجح.",
      tr: "Kötümserim; muhtemelen olmayacağını düşünüyorum.",
      ru: "Я настроен пессимистично; думаю, что, скорее всего, не получится.",
      ckb: "بدبینم؛ پێم وایە ئەگەری زۆرە سەرنەکەوێت.",
      kmr: "Ez nebaş dibînim; dibêjim belkî baş neçe.",
      pl: "Jestem pesymistą; myślę, że to raczej się nie uda.",
      ro: "Sunt pesimist; cred că probabil nu va merge.",
      sq: "Jam pesimist; mendoj se ndoshta nuk do të funksionojë."
    },
    usage: {
      en: "Use it when you expect a bad result, especially in planning, work, or everyday problems.",
      fa: "وقتی انتظار نتیجهٔ بد داری، مخصوصاً در برنامه‌ریزی، کار یا مشکلات روزمره، به کار می‌رود.",
      ar: "تُستخدم عندما تتوقع نتيجة سيئة، خاصة في التخطيط أو العمل أو المشكلات اليومية.",
      tr: "Kötü bir sonuç beklediğinde, özellikle planlama, iş ya da günlük sorunlarda kullanılır.",
      ru: "Используется, когда ожидаешь плохого результата, особенно в планировании, работе или бытовых проблемах.",
      ckb: "کاتێک چاوەڕوانی ئەنجامێکی خراپ دەکەیت، بە تایبەتی لە پلان، کار یان کێشەی ڕۆژانەدا بەکار دێت.",
      kmr: "Dema encamekî xirab hêvî dikî, bi taybetî di plan, kar an pirsgirêkên rojane de tê bikaranîn.",
      pl: "Używa się tego, gdy spodziewasz się złego wyniku, zwłaszcza w planowaniu, pracy albo codziennych problemach.",
      ro: "Se folosește când te aștepți la un rezultat prost, mai ales în planificare, muncă sau probleme zilnice.",
      sq: "Përdoret kur pret një rezultat të keq, sidomos në planifikim, punë ose probleme të përditshme."
    },
    reason: "It is a common pessimism idiom; literal color vocabulary does not explain the real meaning.",
    examples: [
      {
        de: "Ohne mehr Personal sehe ich schwarz für den Termin.",
        tr: {
          en: "Without more staff, I am pessimistic about the deadline.",
          fa: "بدون نیروی بیشتر، نسبت به موعد کار بدبینم.",
          ar: "من دون موظفين أكثر، أنا متشائم بشأن الموعد النهائي.",
          tr: "Daha fazla personel olmadan teslim tarihi konusunda kötümserim.",
          ru: "Без дополнительного персонала я пессимистично смотрю на срок.",
          ckb: "بێ کارمەندی زیاتر، سەبارەت بە کاتی تەواوبوون بدبینم.",
          kmr: "Bê karmendên zêdetir, ez li ser dema dawî nebaş dibînim.",
          pl: "Bez większej liczby pracowników jestem pesymistą co do terminu.",
          ro: "Fără mai mult personal, sunt pesimist în privința termenului.",
          sq: "Pa më shumë staf, jam pesimist për afatin."
        }
      },
      {
        de: "Bei diesem Wetter sehe ich schwarz für das Fest.",
        tr: {
          en: "With this weather, I think the party may not go well.",
          fa: "با این هوا، فکر می‌کنم جشن خوب پیش نرود.",
          ar: "مع هذا الطقس أظن أن الحفل قد لا يسير جيدًا.",
          tr: "Bu havada kutlamanın iyi geçmeyeceğini düşünüyorum.",
          ru: "При такой погоде я думаю, что праздник может пройти неудачно.",
          ckb: "لەگەڵ ئەم کەشەدا پێم وایە ئاهەنگەکە باش نەڕوات.",
          kmr: "Bi vê hewayê, ez difikirim şahî baş neçe.",
          pl: "Przy takiej pogodzie myślę, że impreza może się nie udać.",
          ro: "Cu vremea asta, cred că petrecerea s-ar putea să nu iasă bine.",
          sq: "Me këtë mot, mendoj se festa mund të mos shkojë mirë."
        }
      }
    ]
  },
  {
    slug: "fix-und-fertig-sein",
    text: "Fix und fertig sein.",
    level: "B1",
    type: "colloquial-phrase",
    transparency: "semi-idiomatic",
    register: "colloquial",
    category: "health-and-wellbeing",
    topics: ["everyday-life", "work-and-jobs"],
    literal: {
      en: "To be fixed and finished.",
      fa: "ثابت و تمام‌شده بودن.",
      ar: "أن تكون ثابتًا ومنتهيًا.",
      tr: "Sabit ve bitmiş olmak.",
      ru: "Быть закрепленным и готовым.",
      ckb: "جێگیر و تەواوبوو بوون.",
      kmr: "Sabit û qediyayî bûn.",
      pl: "Być ustalonym i skończonym.",
      ro: "A fi fix și terminat.",
      sq: "Të jesh i fiksuar dhe i mbaruar."
    },
    actual: {
      en: "To be completely exhausted.",
      fa: "کاملاً خسته و از پا افتاده بودن.",
      ar: "أن تكون مرهقًا تمامًا.",
      tr: "Tamamen bitkin olmak.",
      ru: "Быть совершенно измотанным.",
      ckb: "تەواو ماندوو و بێ‌هێز بوون.",
      kmr: "Bi tevahî westiyayî bûn.",
      pl: "Być całkowicie wykończonym.",
      ro: "A fi complet epuizat.",
      sq: "Të jesh krejt i lodhur."
    },
    usage: {
      en: "Use it in everyday speech after a long day, stress, travel, or too much work.",
      fa: "بعد از روز طولانی، فشار، سفر یا کار زیاد در گفتار روزمره به کار می‌رود.",
      ar: "تُستخدم في الكلام اليومي بعد يوم طويل أو ضغط أو سفر أو عمل كثير.",
      tr: "Uzun bir gün, stres, yolculuk ya da çok işten sonra günlük konuşmada kullanılır.",
      ru: "Используется в повседневной речи после долгого дня, стресса, поездки или большого объема работы.",
      ckb: "دوای ڕۆژێکی درێژ، فشار، گەشت یان کاری زۆر لە قسەی ڕۆژانەدا بەکار دێت.",
      kmr: "Piştî rojeke dirêj, stres, rêwîtî an karê zêde di axaftina rojane de tê bikaranîn.",
      pl: "Używa się tego w codziennej mowie po długim dniu, stresie, podróży albo dużej ilości pracy.",
      ro: "Se folosește în vorbirea zilnică după o zi lungă, stres, călătorie sau multă muncă.",
      sq: "Përdoret në të folurën e përditshme pas një dite të gjatë, stresi, udhëtimi ose shumë pune."
    },
    reason: "It is a common colloquial state expression whose real meaning is exhaustion, not completion.",
    examples: [
      {
        de: "Nach der Doppelschicht war ich fix und fertig.",
        tr: {
          en: "After the double shift I was completely exhausted.",
          fa: "بعد از شیفت دوبرابر، کاملاً از پا افتاده بودم.",
          ar: "بعد الوردية المزدوجة كنت مرهقًا تمامًا.",
          tr: "Çift vardiyadan sonra tamamen bitkindim.",
          ru: "После двойной смены я был совершенно измотан.",
          ckb: "دوای شیفتی دوو قات، تەواو ماندوو بووم.",
          kmr: "Piştî nobeta ducarî, ez bi tevahî westiyayî bûm.",
          pl: "Po podwójnej zmianie byłem całkowicie wykończony.",
          ro: "După tura dublă eram complet epuizat.",
          sq: "Pas turnit të dyfishtë isha krejt i lodhur."
        }
      },
      {
        de: "Die Reise war schön, aber wir sind jetzt fix und fertig.",
        tr: {
          en: "The trip was nice, but now we are completely exhausted.",
          fa: "سفر خوب بود، اما الان کاملاً خسته‌ایم.",
          ar: "كانت الرحلة جميلة، لكننا الآن مرهقون تمامًا.",
          tr: "Yolculuk güzeldi ama şimdi tamamen bitkiniz.",
          ru: "Поездка была хорошей, но сейчас мы совершенно вымотаны.",
          ckb: "گەشتەکە خۆش بوو، بەڵام ئێستا تەواو ماندووین.",
          kmr: "Rêwîtî xweş bû, lê niha em bi tevahî westiyayî ne.",
          pl: "Podróż była przyjemna, ale teraz jesteśmy całkowicie wykończeni.",
          ro: "Călătoria a fost frumoasă, dar acum suntem complet epuizați.",
          sq: "Udhëtimi ishte i bukur, por tani jemi krejt të lodhur."
        }
      }
    ]
  },
  {
    slug: "es-ist-hoechste-eisenbahn",
    text: "Es ist höchste Eisenbahn.",
    level: "B1",
    type: "idiom",
    transparency: "non-literal",
    register: "colloquial",
    category: "planning-and-projects",
    topics: ["planning-and-projects", "transport-and-travel"],
    literal: {
      en: "It is highest railway.",
      fa: "این بالاترین راه‌آهن است.",
      ar: "إنها أعلى سكة حديد.",
      tr: "En yüksek demiryolu.",
      ru: "Это самая высокая железная дорога.",
      ckb: "ئەمە بەرزترین ئاسنەڕێیە.",
      kmr: "Ev rêhesina herî bilind e.",
      pl: "To najwyższa kolej.",
      ro: "Este cea mai înaltă cale ferată.",
      sq: "Është hekurudha më e lartë."
    },
    actual: {
      en: "It is really time; we must hurry now.",
      fa: "دیگر واقعاً وقتش است؛ باید عجله کنیم.",
      ar: "حان الوقت فعلًا؛ يجب أن نسرع الآن.",
      tr: "Artık gerçekten zamanı geldi; şimdi acele etmeliyiz.",
      ru: "Уже действительно пора; нужно поторопиться.",
      ckb: "ئێستا بەڕاستی کاتە؛ دەبێت پەلە بکەین.",
      kmr: "Êdî bi rastî wext e; divê niha bilezînin.",
      pl: "Naprawdę już czas; musimy się spieszyć.",
      ro: "Chiar este timpul; trebuie să ne grăbim acum.",
      sq: "Tani është vërtet koha; duhet të nxitojmë."
    },
    usage: {
      en: "Use it when something has become urgent, often before leaving or finishing a task.",
      fa: "وقتی کاری فوری شده، مخصوصاً قبل از رفتن یا تمام کردن کاری، به کار می‌رود.",
      ar: "تُستخدم عندما يصبح الأمر عاجلًا، غالبًا قبل المغادرة أو إنهاء مهمة.",
      tr: "Bir şey acil hale geldiğinde, özellikle çıkmadan ya da işi bitirmeden önce kullanılır.",
      ru: "Используется, когда дело стало срочным, часто перед уходом или завершением задачи.",
      ckb: "کاتێک شتێک پەلەدار بووە، زۆرجار پێش ڕۆیشتن یان تەواوکردنی کارێک بەکار دێت.",
      kmr: "Dema tiştek lezgîn bibe, gelek caran berî çûnê an qedandina karekî tê bikaranîn.",
      pl: "Używa się tego, gdy coś stało się pilne, często przed wyjściem albo zakończeniem zadania.",
      ro: "Se folosește când ceva a devenit urgent, adesea înainte de plecare sau de terminarea unei sarcini.",
      sq: "Përdoret kur diçka është bërë urgjente, shpesh para nisjes ose përfundimit të një pune."
    },
    reason: "It is a time-pressure idiom whose literal railway image is not the actual meaning.",
    examples: [
      {
        de: "Es ist höchste Eisenbahn, der Bus fährt in fünf Minuten.",
        tr: {
          en: "We really have to hurry; the bus leaves in five minutes.",
          fa: "واقعاً باید عجله کنیم؛ اتوبوس پنج دقیقه دیگر حرکت می‌کند.",
          ar: "علينا أن نسرع فعلًا؛ الحافلة تغادر بعد خمس دقائق.",
          tr: "Gerçekten acele etmeliyiz; otobüs beş dakika içinde kalkıyor.",
          ru: "Нам правда нужно спешить; автобус отправляется через пять минут.",
          ckb: "بەڕاستی دەبێت پەلە بکەین؛ پاسەکە لە پێنج خولەکدا دەڕوات.",
          kmr: "Bi rastî divê em bilezînin; otobus di pênc xulekên din de diçe.",
          pl: "Naprawdę musimy się spieszyć; autobus odjeżdża za pięć minut.",
          ro: "Chiar trebuie să ne grăbim; autobuzul pleacă în cinci minute.",
          sq: "Vërtet duhet të nxitojmë; autobusi niset pas pesë minutash."
        }
      },
      {
        de: "Für die Bewerbung ist es höchste Eisenbahn.",
        tr: {
          en: "For the application, it is really time to act.",
          fa: "برای درخواست، واقعاً وقت اقدام کردن است.",
          ar: "بالنسبة إلى الطلب، حان الوقت فعلًا للتصرف.",
          tr: "Başvuru için artık gerçekten harekete geçme zamanı.",
          ru: "Для заявления уже действительно пора действовать.",
          ckb: "بۆ داواکارییەکە، بەڕاستی کاتی هەنگاونانە.",
          kmr: "Ji bo daxwaznameyê, êdî bi rastî wextê tevgerê ye.",
          pl: "W sprawie aplikacji naprawdę najwyższy czas działać.",
          ro: "Pentru aplicație chiar este momentul să acționezi.",
          sq: "Për aplikimin, tani është vërtet koha për të vepruar."
        }
      }
    ]
  },
  {
    slug: "der-groschen-ist-gefallen",
    text: "Der Groschen ist gefallen.",
    level: "B1",
    type: "idiom",
    transparency: "non-literal",
    register: "colloquial",
    category: "education-and-training",
    topics: ["education-and-training", "everyday-life"],
    literal: {
      en: "The coin has fallen.",
      fa: "سکه افتاده است.",
      ar: "سقطت العملة.",
      tr: "Bozuk para düştü.",
      ru: "Монета упала.",
      ckb: "پارە بچووکەکە کەوت.",
      kmr: "Pereyê biçûk ket.",
      pl: "Moneta spadła.",
      ro: "Moneda a căzut.",
      sq: "Monedha ra."
    },
    actual: {
      en: "Now I finally understand.",
      fa: "بالاخره فهمیدم.",
      ar: "الآن فهمت أخيرًا.",
      tr: "Sonunda anladım.",
      ru: "Теперь я наконец понял.",
      ckb: "ئێستا لە کۆتاییدا تێگەیشتم.",
      kmr: "Niha dawî ez fêm kirim.",
      pl: "Teraz wreszcie zrozumiałem.",
      ro: "Acum am înțeles în sfârșit.",
      sq: "Tani më në fund e kuptova."
    },
    usage: {
      en: "Use it when understanding comes after a delay, especially after an explanation.",
      fa: "وقتی بعد از کمی تأخیر، مخصوصاً پس از توضیح، مطلب را می‌فهمی به کار می‌رود.",
      ar: "تُستخدم عندما يأتي الفهم بعد تأخير، خاصة بعد شرح.",
      tr: "Anlama biraz gecikince, özellikle bir açıklamadan sonra kullanılır.",
      ru: "Используется, когда понимание приходит с задержкой, особенно после объяснения.",
      ckb: "کاتێک تێگەیشتن دوای کەمێک دواخستن دێت، بە تایبەتی دوای ڕوونکردنەوە، بەکار دێت.",
      kmr: "Dema têgihiştin piştî derengiyekê bê, bi taybetî piştî ravekirinê, tê bikaranîn.",
      pl: "Używa się tego, gdy zrozumienie przychodzi z opóźnieniem, zwłaszcza po wyjaśnieniu.",
      ro: "Se folosește când înțelegerea vine cu întârziere, mai ales după o explicație.",
      sq: "Përdoret kur kuptimi vjen me vonesë, sidomos pas një shpjegimi."
    },
    reason: "It is a common learning and conversation idiom for delayed understanding.",
    examples: [
      {
        de: "Nach dem dritten Beispiel ist bei mir der Groschen gefallen.",
        tr: {
          en: "After the third example, I finally understood.",
          fa: "بعد از مثال سوم، بالاخره فهمیدم.",
          ar: "بعد المثال الثالث فهمت أخيرًا.",
          tr: "Üçüncü örnekten sonra sonunda anladım.",
          ru: "После третьего примера я наконец понял.",
          ckb: "دوای نموونەی سێیەم، لە کۆتاییدا تێگەیشتم.",
          kmr: "Piştî mînaka sêyem, dawî ez fêm kirim.",
          pl: "Po trzecim przykładzie wreszcie zrozumiałem.",
          ro: "După al treilea exemplu am înțeles în sfârșit.",
          sq: "Pas shembullit të tretë, më në fund e kuptova."
        }
      },
      {
        de: "Jetzt ist der Groschen gefallen: Wir müssen zuerst den Antrag senden.",
        tr: {
          en: "Now I get it: first we have to send the application.",
          fa: "حالا فهمیدم: اول باید درخواست را بفرستیم.",
          ar: "الآن فهمت: علينا أولًا إرسال الطلب.",
          tr: "Şimdi anladım: önce başvuruyu göndermeliyiz.",
          ru: "Теперь понятно: сначала нужно отправить заявление.",
          ckb: "ئێستا تێگەیشتم: سەرەتا دەبێت داواکارییەکە بنێرین.",
          kmr: "Niha fêm kirim: pêşî divê em daxwaznameyê bişînin.",
          pl: "Teraz rozumiem: najpierw musimy wysłać wniosek.",
          ro: "Acum am înțeles: mai întâi trebuie să trimitem cererea.",
          sq: "Tani e kuptova: së pari duhet ta dërgojmë kërkesën."
        }
      }
    ]
  },
  {
    slug: "jemandem-unter-die-arme-greifen",
    text: "Jemandem unter die Arme greifen.",
    level: "B1",
    type: "idiom",
    transparency: "semi-idiomatic",
    register: "neutral",
    category: "asking-for-help",
    topics: ["social-and-relationships", "work-and-jobs"],
    literal: {
      en: "To grab under someone's arms.",
      fa: "زیر بازوهای کسی را گرفتن.",
      ar: "الإمساك تحت ذراعي شخص ما.",
      tr: "Birinin kollarının altından tutmak.",
      ru: "Подхватить кого-то под руки.",
      ckb: "لە ژێر باڵی کەسێک گرتن.",
      kmr: "Di bin milên kesekî de girtin.",
      pl: "Chwycić kogoś pod ramiona.",
      ro: "A prinde pe cineva de sub brațe.",
      sq: "Ta kapësh dikë poshtë krahëve."
    },
    actual: {
      en: "To help or support someone.",
      fa: "به کسی کمک یا پشتیبانی کردن.",
      ar: "مساعدة شخص أو دعمه.",
      tr: "Birine yardım etmek ya da destek olmak.",
      ru: "Помочь или поддержать кого-то.",
      ckb: "یارمەتی یان پشتگیری کەسێک کردن.",
      kmr: "Alîkarî an piştgirîya kesekî kirin.",
      pl: "Pomóc komuś albo go wesprzeć.",
      ro: "A ajuta sau a sprijini pe cineva.",
      sq: "Të ndihmosh ose mbështesësh dikë."
    },
    usage: {
      en: "Use it for practical support at work, school, family, or administration.",
      fa: "برای کمک عملی در کار، مدرسه، خانواده یا کارهای اداری به کار می‌رود.",
      ar: "تُستخدم للدعم العملي في العمل أو المدرسة أو الأسرة أو الإدارة.",
      tr: "İş, okul, aile ya da resmi işler konusunda pratik destek için kullanılır.",
      ru: "Используется для практической поддержки на работе, в школе, семье или административных делах.",
      ckb: "بۆ پشتگیریی کرداری لە کار، خوێندنگە، خێزان یان کاروباری ئیداریدا بەکار دێت.",
      kmr: "Ji bo piştgirîya pratîk di kar, dibistan, malbat an karên îdarî de tê bikaranîn.",
      pl: "Używa się tego przy praktycznym wsparciu w pracy, szkole, rodzinie albo sprawach urzędowych.",
      ro: "Se folosește pentru sprijin practic la muncă, școală, familie sau administrație.",
      sq: "Përdoret për mbështetje praktike në punë, shkollë, familje ose administratë."
    },
    reason: "It is a frequent support idiom that learners may misread as a physical action only.",
    examples: [
      {
        de: "Kannst du mir beim Formular unter die Arme greifen?",
        tr: {
          en: "Can you help me with the form?",
          fa: "می‌توانی در مورد فرم کمکم کنی؟",
          ar: "هل يمكنك مساعدتي في الاستمارة؟",
          tr: "Form konusunda bana yardım edebilir misin?",
          ru: "Можешь помочь мне с формой?",
          ckb: "دەتوانیت لە فۆرمەکەدا یارمەتیم بدەیت؟",
          kmr: "Tu dikarî di derbarê formê de alîkariya min bikî?",
          pl: "Czy możesz mi pomóc z formularzem?",
          ro: "Mă poți ajuta cu formularul?",
          sq: "A mund të më ndihmosh me formularin?"
        }
      },
      {
        de: "Das Team hat ihr in der ersten Woche unter die Arme gegriffen.",
        tr: {
          en: "The team supported her during the first week.",
          fa: "تیم در هفتهٔ اول از او حمایت کرد.",
          ar: "دعمها الفريق في الأسبوع الأول.",
          tr: "Ekip ilk haftada ona destek oldu.",
          ru: "Команда поддержала ее в первую неделю.",
          ckb: "تیمەکە لە هەفتەی یەکەمدا پشتگیری کرد.",
          kmr: "Tîmê di hefteya yekem de piştgirî da wê.",
          pl: "Zespół wsparł ją w pierwszym tygodniu.",
          ro: "Echipa a sprijinit-o în prima săptămână.",
          sq: "Ekipi e mbështeti në javën e parë."
        }
      }
    ]
  },
  {
    slug: "in-die-gaenge-kommen",
    text: "In die Gänge kommen.",
    level: "B1",
    type: "colloquial-phrase",
    transparency: "semi-idiomatic",
    register: "colloquial",
    category: "planning-and-projects",
    topics: ["planning-and-projects", "everyday-life"],
    literal: {
      en: "To get into the gears.",
      fa: "وارد دنده‌ها شدن.",
      ar: "الدخول في التروس.",
      tr: "Viteslere girmek.",
      ru: "Войти в передачи.",
      ckb: "چوونە ناو گێڕەکان.",
      kmr: "Ketina nav gêrên makîneyê.",
      pl: "Wejść w biegi.",
      ro: "A intra în viteze.",
      sq: "Të hysh në marshe."
    },
    actual: {
      en: "To finally get started or become active.",
      fa: "بالاخره راه افتادن یا فعال شدن.",
      ar: "أن تبدأ أخيرًا أو تصبح نشيطًا.",
      tr: "Sonunda harekete geçmek ya da aktif olmak.",
      ru: "Наконец начать или активизироваться.",
      ckb: "لە کۆتاییدا دەستپێکردن یان چالاک بوون.",
      kmr: "Dawî dest pê kirin an çalak bûn.",
      pl: "Wreszcie ruszyć z miejsca albo się uaktywnić.",
      ro: "A porni în sfârșit sau a deveni activ.",
      sq: "Më në fund të fillosh ose të bëhesh aktiv."
    },
    usage: {
      en: "Use it when a person, team, or process was slow and now needs momentum.",
      fa: "وقتی شخص، تیم یا روندی کند بوده و حالا باید حرکت بگیرد به کار می‌رود.",
      ar: "تُستخدم عندما يكون شخص أو فريق أو إجراء بطيئًا ويحتاج الآن إلى حركة.",
      tr: "Bir kişi, ekip ya da süreç yavaş kaldığında ve artık hızlanması gerektiğinde kullanılır.",
      ru: "Используется, когда человек, команда или процесс были медленными и теперь должны набрать темп.",
      ckb: "کاتێک کەس، تیم یان پرۆسەیەک هێواش بووە و ئێستا پێویستی بە جووڵەیە، بەکار دێت.",
      kmr: "Dema kesek, tîmek an pêvajoyek hêdî bûbe û niha divê lez bigire, tê bikaranîn.",
      pl: "Używa się tego, gdy osoba, zespół albo proces były powolne i teraz muszą nabrać tempa.",
      ro: "Se folosește când o persoană, o echipă sau un proces a fost lent și acum trebuie să prindă ritm.",
      sq: "Përdoret kur një person, ekip ose proces ka qenë i ngadalshëm dhe tani duhet të marrë ritëm."
    },
    reason: "It is a common momentum phrase; the gear image is not literal movement.",
    examples: [
      {
        de: "Wir müssen mit dem Projekt endlich in die Gänge kommen.",
        tr: {
          en: "We finally have to get the project moving.",
          fa: "بالاخره باید پروژه را راه بیندازیم.",
          ar: "علينا أخيرًا أن نحرك المشروع.",
          tr: "Projeyi sonunda hareketlendirmemiz gerekiyor.",
          ru: "Нам наконец нужно сдвинуть проект с места.",
          ckb: "دەبێت لە کۆتاییدا پرۆژەکە بخەینە جووڵە.",
          kmr: "Divê em dawî projeyê bixin tevgerê.",
          pl: "Musimy wreszcie ruszyć projekt z miejsca.",
          ro: "Trebuie în sfârșit să punem proiectul în mișcare.",
          sq: "Më në fund duhet ta vëmë projektin në lëvizje."
        }
      },
      {
        de: "Nach dem Kaffee kam die ganze Gruppe in die Gänge.",
        tr: {
          en: "After the coffee, the whole group got going.",
          fa: "بعد از قهوه، کل گروه راه افتاد.",
          ar: "بعد القهوة بدأت المجموعة كلها تتحرك.",
          tr: "Kahveden sonra bütün grup hareketlendi.",
          ru: "После кофе вся группа оживилась.",
          ckb: "دوای قاوە، هەموو گرووپەکە چووە جووڵە.",
          kmr: "Piştî qehweyê, hemû kom çalak bû.",
          pl: "Po kawie cała grupa ruszyła z miejsca.",
          ro: "După cafea, tot grupul s-a pus în mișcare.",
          sq: "Pas kafesë, i gjithë grupi u aktivizua."
        }
      }
    ]
  },
  {
    slug: "nicht-ohne-sein",
    text: "Nicht ohne sein.",
    level: "B2",
    type: "colloquial-phrase",
    transparency: "semi-idiomatic",
    register: "neutral",
    category: "quality-and-risk",
    topics: ["quality-and-risk", "work-and-jobs"],
    literal: {
      en: "To be not without.",
      fa: "بی‌چیز نبودن.",
      ar: "أن يكون ليس بلا شيء.",
      tr: "Bir şeysiz olmamak.",
      ru: "Быть не без чего-то.",
      ckb: "بێ شت نەبوون.",
      kmr: "Bê tiştekî nebûn.",
      pl: "Nie być bez czegoś.",
      ro: "A nu fi fără ceva.",
      sq: "Të mos jesh pa diçka."
    },
    actual: {
      en: "It is demanding, risky, or more serious than it looks.",
      fa: "سخت، پرریسک یا جدی‌تر از چیزی است که به نظر می‌رسد.",
      ar: "الأمر صعب أو محفوف بالمخاطر أو أكثر جدية مما يبدو.",
      tr: "Zor, riskli ya da göründüğünden daha ciddi.",
      ru: "Это сложно, рискованно или серьезнее, чем кажется.",
      ckb: "قورس، مەترسیدار یان جدیترە لەوەی دەردەکەوێت.",
      kmr: "Ew dijwar, metirsîdar an ji ya xuya dike girîngtir e.",
      pl: "To trudne, ryzykowne albo poważniejsze, niż wygląda.",
      ro: "Este dificil, riscant sau mai serios decât pare.",
      sq: "Është e vështirë, me rrezik ose më serioze sesa duket."
    },
    usage: {
      en: "Use it to warn that a task, price, illness, or situation should not be underestimated.",
      fa: "برای هشدار دادن به اینکه کار، قیمت، بیماری یا موقعیت را نباید دست‌کم گرفت به کار می‌رود.",
      ar: "تُستخدم للتنبيه إلى أن مهمة أو سعرًا أو مرضًا أو موقفًا لا ينبغي الاستهانة به.",
      tr: "Bir işin, fiyatın, hastalığın ya da durumun hafife alınmaması gerektiğini belirtir.",
      ru: "Используется, чтобы предупредить: задачу, цену, болезнь или ситуацию не стоит недооценивать.",
      ckb: "بۆ ئاگادارکردنەوەی ئەوە بەکار دێت کە کار، نرخ، نەخۆشی یان دۆخ نابێت بچووک بکرێتەوە.",
      kmr: "Ji bo hişyarkirinê tê bikaranîn ku kar, bihâ, nexweşî an rewş neyê piçûkxistin.",
      pl: "Używa się tego, by ostrzec, że zadania, ceny, choroby albo sytuacji nie należy lekceważyć.",
      ro: "Se folosește pentru a avertiza că o sarcină, un preț, o boală sau o situație nu trebuie subestimată.",
      sq: "Përdoret për të paralajmëruar se një punë, çmim, sëmundje ose situatë nuk duhet nënvlerësuar."
    },
    reason: "It is a compact risk-assessment phrase whose actual meaning is stronger than the literal negation.",
    examples: [
      {
        de: "Die Prüfung ist nicht ohne, du solltest früh anfangen.",
        tr: {
          en: "The exam is demanding; you should start early.",
          fa: "امتحان ساده نیست؛ بهتر است زود شروع کنی.",
          ar: "الامتحان ليس سهلًا؛ من الأفضل أن تبدأ مبكرًا.",
          tr: "Sınav kolay değil; erken başlamalısın.",
          ru: "Экзамен непростой; тебе стоит начать заранее.",
          ckb: "تاقیکردنەوەکە ئاسان نییە؛ باشترە زوو دەست پێ بکەیت.",
          kmr: "Ezmûn hêsan nîne; baştir e zû dest pê bikî.",
          pl: "Egzamin jest wymagający; powinieneś zacząć wcześnie.",
          ro: "Examenul nu este ușor; ar trebui să începi devreme.",
          sq: "Provimi nuk është i lehtë; duhet të fillosh herët."
        }
      },
      {
        de: "Die Reparatur ist nicht ohne, weil mehrere Teile fehlen.",
        tr: {
          en: "The repair is more difficult than it looks because several parts are missing.",
          fa: "تعمیر کار ساده‌ای نیست، چون چند قطعه کم است.",
          ar: "الإصلاح أصعب مما يبدو لأن عدة قطع ناقصة.",
          tr: "Onarım göründüğünden zor, çünkü birkaç parça eksik.",
          ru: "Ремонт сложнее, чем кажется, потому что не хватает нескольких деталей.",
          ckb: "چاککردنەوەکە لەوەی دەردەکەوێت قورسترە، چونکە چەند پارچەیەک کەمە.",
          kmr: "Çêkirin ji ya xuya dike dijwartir e, ji ber ku çend perçe kêm in.",
          pl: "Naprawa jest trudniejsza, niż wygląda, bo brakuje kilku części.",
          ro: "Reparația este mai grea decât pare, pentru că lipsesc mai multe piese.",
          sq: "Riparimi është më i vështirë sesa duket, sepse mungojnë disa pjesë."
        }
      }
    ]
  },
  {
    slug: "im-eifer-des-gefechts",
    text: "Im Eifer des Gefechts.",
    level: "B2",
    type: "idiom",
    transparency: "non-literal",
    register: "neutral",
    category: "social-and-relationships",
    topics: ["social-and-relationships", "business-communication"],
    literal: {
      en: "In the zeal of the battle.",
      fa: "در شور و حرارت نبرد.",
      ar: "في حماسة المعركة.",
      tr: "Çarpışmanın hararetinde.",
      ru: "В пылу сражения.",
      ckb: "لە گەرمی شەڕدا.",
      kmr: "Di germiya şer de.",
      pl: "W zapale walki.",
      ro: "În ardoarea luptei.",
      sq: "Në zjarrin e betejës."
    },
    actual: {
      en: "In the heat of the moment, without thinking calmly.",
      fa: "در لحظهٔ هیجان یا فشار، بدون فکر آرام.",
      ar: "في حرارة اللحظة، من دون تفكير هادئ.",
      tr: "Anın hararetiyle, sakin düşünmeden.",
      ru: "Сгоряча, без спокойного обдумывания.",
      ckb: "لە گەرمی ساتەکەدا، بێ بیرکردنەوەی ئارام.",
      kmr: "Di germiya kêliyê de, bê fikirandina aram.",
      pl: "W ferworze chwili, bez spokojnego namysłu.",
      ro: "În fierbințeala momentului, fără gândire calmă.",
      sq: "Në nxehtësinë e momentit, pa menduar qetë."
    },
    usage: {
      en: "Use it to soften or explain a mistake made during stress, argument, or hurry.",
      fa: "برای نرم کردن یا توضیح اشتباهی که در فشار، بحث یا عجله رخ داده به کار می‌رود.",
      ar: "تُستخدم لتخفيف أو تفسير خطأ حدث تحت ضغط أو في نقاش أو عجلة.",
      tr: "Stres, tartışma ya da acele sırasında yapılan hatayı yumuşatmak ya da açıklamak için kullanılır.",
      ru: "Используется, чтобы смягчить или объяснить ошибку, сделанную в стрессе, споре или спешке.",
      ckb: "بۆ نەرمکردن یان ڕوونکردنەوەی هەڵەیەک بەکار دێت کە لە فشار، مشتومڕ یان پەلەدا ڕوویداوە.",
      kmr: "Ji bo nermkirin an ravekirina xeletiyekê ku di stres, nîqaş an lezê de çêbûye tê bikaranîn.",
      pl: "Używa się tego, by złagodzić albo wyjaśnić błąd popełniony w stresie, sporze albo pośpiechu.",
      ro: "Se folosește pentru a atenua sau explica o greșeală făcută sub stres, într-o ceartă sau în grabă.",
      sq: "Përdoret për të zbutur ose shpjeguar një gabim të bërë nën stres, debat ose nxitim."
    },
    reason: "It is a common social-repair idiom for responsibility and context, not a literal battle phrase.",
    examples: [
      {
        de: "Im Eifer des Gefechts habe ich deinen Namen vergessen.",
        tr: {
          en: "In the heat of the moment, I forgot your name.",
          fa: "در آن لحظهٔ پرتنش، اسمت را فراموش کردم.",
          ar: "في حرارة اللحظة نسيت اسمك.",
          tr: "O anın hararetiyle adını unuttum.",
          ru: "Сгоряча я забыл твое имя.",
          ckb: "لە گەرمی ساتەکەدا ناوت لەبیرم چوو.",
          kmr: "Di germiya kêliyê de navê te ji bîr kirim.",
          pl: "W ferworze chwili zapomniałem twoje imię.",
          ro: "În fierbințeala momentului ți-am uitat numele.",
          sq: "Në nxehtësinë e momentit harrova emrin tënd."
        }
      },
      {
        de: "Er hat im Eifer des Gefechts zu schnell zugesagt.",
        tr: {
          en: "In the pressure of the moment, he agreed too quickly.",
          fa: "در فشار آن لحظه، خیلی سریع موافقت کرد.",
          ar: "تحت ضغط اللحظة وافق بسرعة كبيرة.",
          tr: "O anın baskısıyla çok hızlı kabul etti.",
          ru: "Под давлением момента он слишком быстро согласился.",
          ckb: "لە فشاری ئەو ساتەدا زۆر خێرا ڕازی بوو.",
          kmr: "Di zexta wê kêliyê de pir zû qebûl kir.",
          pl: "Pod presją chwili zgodził się zbyt szybko.",
          ro: "Sub presiunea momentului a acceptat prea repede.",
          sq: "Nën presionin e momentit pranoi shumë shpejt."
        }
      }
    ]
  },
  {
    slug: "jemandem-einen-korb-geben",
    text: "Jemandem einen Korb geben.",
    level: "B2",
    type: "idiom",
    transparency: "non-literal",
    register: "colloquial",
    category: "social-and-relationships",
    topics: ["social-and-relationships"],
    literal: {
      en: "To give someone a basket.",
      fa: "به کسی یک سبد دادن.",
      ar: "إعطاء شخص سلة.",
      tr: "Birine sepet vermek.",
      ru: "Дать кому-то корзину.",
      ckb: "سەبەتەیەک بە کەسێک دان.",
      kmr: "Sepetekê dayîna kesekî.",
      pl: "Dać komuś kosz.",
      ro: "A da cuiva un coș.",
      sq: "T’i japësh dikujt një shportë."
    },
    actual: {
      en: "To reject someone's romantic interest or invitation.",
      fa: "درخواست عاشقانه یا دعوت کسی را رد کردن.",
      ar: "رفض اهتمام عاطفي أو دعوة من شخص.",
      tr: "Birinin duygusal ilgisini ya da davetini reddetmek.",
      ru: "Отклонить чью-то романтическую заинтересованность или приглашение.",
      ckb: "داواکاریی سۆزداری یان بانگهێشتی کەسێک ڕەتکردنەوە.",
      kmr: "Eleqeya evînî an vexwendina kesekî red kirin.",
      pl: "Odrzucić czyjeś romantyczne zainteresowanie albo zaproszenie.",
      ro: "A respinge interesul romantic sau invitația cuiva.",
      sq: "Të refuzosh interesin romantik ose ftesën e dikujt."
    },
    usage: {
      en: "Use it mainly in informal social contexts; it can sound personal, so it needs tact.",
      fa: "بیشتر در موقعیت‌های اجتماعی غیررسمی به کار می‌رود؛ چون شخصی است، باید با احتیاط استفاده شود.",
      ar: "تُستخدم غالبًا في سياقات اجتماعية غير رسمية؛ لأنها شخصية وتحتاج إلى لباقة.",
      tr: "Daha çok samimi sosyal bağlamlarda kullanılır; kişisel olduğu için dikkat ister.",
      ru: "Используется в основном в неформальных социальных ситуациях; тема личная, поэтому нужен такт.",
      ckb: "زیاتر لە دۆخە کۆمەڵایەتییە نافەرمییەکاندا بەکار دێت؛ چونکە کەسییە، پێویستی بە وردی هەیە.",
      kmr: "Zêdetir di rewşên civakî yên nefermî de tê bikaranîn; ji ber ku kesane ye, hişyari dixwaze.",
      pl: "Używa się tego głównie w nieformalnych sytuacjach społecznych; jest osobiste, więc wymaga taktu.",
      ro: "Se folosește mai ales în contexte sociale informale; fiind personal, cere tact.",
      sq: "Përdoret kryesisht në situata shoqërore joformale; është personale, prandaj kërkon takt."
    },
    reason: "It is a non-literal social idiom that learners would misunderstand as an object exchange.",
    isRisky: true,
    warnings: [warning],
    examples: [
      {
        de: "Er wollte mit ihr ausgehen, aber sie hat ihm einen Korb gegeben.",
        tr: {
          en: "He wanted to go out with her, but she rejected him.",
          fa: "او می‌خواست با او بیرون برود، اما او درخواستش را رد کرد.",
          ar: "أراد الخروج معها، لكنها رفضته.",
          tr: "Onunla dışarı çıkmak istedi ama kadın onu reddetti.",
          ru: "Он хотел с ней встретиться, но она ему отказала.",
          ckb: "ویستی لەگەڵیدا بچێتە دەرەوە، بەڵام ئەو ڕەتی کردەوە.",
          kmr: "Wî xwest bi wê re derkeve, lê wê ew red kir.",
          pl: "Chciał się z nią umówić, ale ona go odrzuciła.",
          ro: "El voia să iasă cu ea, dar ea l-a refuzat.",
          sq: "Ai donte të dilte me të, por ajo e refuzoi."
        }
      },
      {
        de: "Nach dem Korb war die Stimmung kurz etwas komisch.",
        tr: {
          en: "After the rejection, the mood was a bit awkward for a moment.",
          fa: "بعد از رد شدن، فضا برای لحظه‌ای کمی معذب شد.",
          ar: "بعد الرفض أصبح الجو محرجًا قليلًا للحظة.",
          tr: "Reddedilmeden sonra ortam bir süre biraz tuhaflaştı.",
          ru: "После отказа атмосфера на мгновение стала немного неловкой.",
          ckb: "دوای ڕەتکردنەوەکە، کەشەکە بۆ ماوەیەکی کورت کەمێک نامۆ بوو.",
          kmr: "Piştî redkirinê, rewş demek kurt hinek nerehet bû.",
          pl: "Po odmowie atmosfera przez chwilę była trochę niezręczna.",
          ro: "După refuz, atmosfera a fost pentru scurt timp puțin stânjenitoare.",
          sq: "Pas refuzimit, atmosfera për pak kohë u bë pak e sikletshme."
        }
      }
    ]
  },
  {
    slug: "die-faeden-ziehen",
    text: "Die Fäden ziehen.",
    level: "C1",
    type: "idiom",
    transparency: "non-literal",
    register: "neutral",
    category: "management-and-leadership",
    topics: ["management-and-leadership", "business-communication"],
    literal: {
      en: "To pull the strings.",
      fa: "نخ‌ها را کشیدن.",
      ar: "سحب الخيوط.",
      tr: "İpleri çekmek.",
      ru: "Дергать за нити.",
      ckb: "داوەکان ڕاکێشان.",
      kmr: "Têlan kişandin.",
      pl: "Pociągać za sznurki.",
      ro: "A trage sforile.",
      sq: "Të tërheqësh fijet."
    },
    actual: {
      en: "To control events or decisions from the background.",
      fa: "از پشت صحنه اتفاقات یا تصمیم‌ها را کنترل کردن.",
      ar: "التحكم في الأحداث أو القرارات من خلف الكواليس.",
      tr: "Olayları ya da kararları arka plandan kontrol etmek.",
      ru: "Контролировать события или решения из-за кулис.",
      ckb: "لە پشت پەردەوە ڕووداو یان بڕیارەکان کۆنترۆڵ کردن.",
      kmr: "Ji pişt perdeyê bûyer an biryaran kontrol kirin.",
      pl: "Kontrolować wydarzenia albo decyzje z tylnego planu.",
      ro: "A controla evenimente sau decizii din culise.",
      sq: "Të kontrollosh ngjarje ose vendime nga prapaskena."
    },
    usage: {
      en: "Use it for hidden influence in organizations, politics, teams, or family situations.",
      fa: "برای نفوذ پنهان در سازمان، سیاست، تیم یا موقعیت خانوادگی به کار می‌رود.",
      ar: "تُستخدم للتأثير الخفي في المؤسسات أو السياسة أو الفرق أو المواقف العائلية.",
      tr: "Kurum, siyaset, ekip ya da aile durumlarında gizli etki için kullanılır.",
      ru: "Используется для скрытого влияния в организациях, политике, командах или семье.",
      ckb: "بۆ کاریگەریی شاراوە لە ڕێکخراو، سیاسەت، تیم یان دۆخی خێزانیدا بەکار دێت.",
      kmr: "Ji bo bandora veşartî di rêxistin, siyaset, tîm an rewşa malbatî de tê bikaranîn.",
      pl: "Używa się tego przy ukrytym wpływie w organizacjach, polityce, zespołach albo rodzinie.",
      ro: "Se folosește pentru influență ascunsă în organizații, politică, echipe sau familie.",
      sq: "Përdoret për ndikim të fshehtë në organizata, politikë, ekipe ose familje."
    },
    reason: "It is an advanced influence idiom; the literal puppet-like image carries the social meaning.",
    examples: [
      {
        de: "Offiziell entscheidet der Ausschuss, aber im Hintergrund zieht sie die Fäden.",
        tr: {
          en: "Officially the committee decides, but behind the scenes she controls things.",
          fa: "رسمی کمیته تصمیم می‌گیرد، اما پشت صحنه او همه‌چیز را کنترل می‌کند.",
          ar: "رسميًا تقرر اللجنة، لكنها في الخلفية تتحكم في الأمور.",
          tr: "Resmen komite karar veriyor ama arka planda işleri o yönetiyor.",
          ru: "Официально решает комитет, но за кулисами она всем управляет.",
          ckb: "بە فەرمی لێژنەکە بڕیار دەدات، بەڵام لە پشت پەردەوە ئەو کۆنترۆڵ دەکات.",
          kmr: "Bi fermî komîte biryar dide, lê li pişt perdeyê ew kontrol dike.",
          pl: "Oficjalnie decyduje komisja, ale za kulisami to ona pociąga za sznurki.",
          ro: "Oficial decide comisia, dar din culise ea controlează lucrurile.",
          sq: "Zyrtarisht vendos komisioni, por në prapaskenë ajo i kontrollon gjërat."
        }
      },
      {
        de: "Niemand wusste genau, wer bei dem Projekt die Fäden zieht.",
        tr: {
          en: "No one knew exactly who was really steering the project.",
          fa: "هیچ‌کس دقیق نمی‌دانست چه کسی واقعاً پروژه را هدایت می‌کند.",
          ar: "لم يعرف أحد بدقة من يدير المشروع فعليًا.",
          tr: "Projeyi gerçekte kimin yönettiğini kimse tam bilmiyordu.",
          ru: "Никто точно не знал, кто на самом деле руководит проектом.",
          ckb: "هیچ کەس بە وردی نەیدەزانی کێ بەڕاستی پرۆژەکە بەڕێوە دەبات.",
          kmr: "Kes bi rastî nedizanî kî projeyê rêve dibe.",
          pl: "Nikt dokładnie nie wiedział, kto naprawdę steruje projektem.",
          ro: "Nimeni nu știa exact cine conduce de fapt proiectul.",
          sq: "Askush nuk e dinte saktësisht kush e drejtonte vërtet projektin."
        }
      }
    ]
  },
  {
    slug: "zwischen-den-stuehlen-sitzen",
    text: "Zwischen den Stühlen sitzen.",
    level: "C1",
    type: "idiom",
    transparency: "non-literal",
    register: "neutral",
    category: "business-communication",
    topics: ["business-communication", "social-and-relationships"],
    literal: {
      en: "To sit between the chairs.",
      fa: "بین صندلی‌ها نشستن.",
      ar: "الجلوس بين الكراسي.",
      tr: "Sandalyelerin arasında oturmak.",
      ru: "Сидеть между стульями.",
      ckb: "لە نێوان کورسییەکاندا دانیشتن.",
      kmr: "Di navbera kursiyan de rûniştin.",
      pl: "Siedzieć między krzesłami.",
      ro: "A sta între scaune.",
      sq: "Të ulesh mes karrigeve."
    },
    actual: {
      en: "To be caught between two sides or interests.",
      fa: "بین دو طرف یا دو منفعت گیر افتادن.",
      ar: "أن تكون عالقًا بين طرفين أو مصلحتين.",
      tr: "İki taraf ya da çıkar arasında kalmak.",
      ru: "Оказаться между двумя сторонами или интересами.",
      ckb: "لە نێوان دوو لایەن یان دوو بەرژەوەندیدا گیر بوون.",
      kmr: "Di navbera du alî an du berjewendiyan de asê bûn.",
      pl: "Utknąć między dwiema stronami albo interesami.",
      ro: "A fi prins între două părți sau interese.",
      sq: "Të mbetesh mes dy palëve ose interesave."
    },
    usage: {
      en: "Use it for conflicts of loyalty or responsibility in family, work, politics, or administration.",
      fa: "برای تعارض وفاداری یا مسئولیت در خانواده، کار، سیاست یا اداره به کار می‌رود.",
      ar: "تُستخدم لصراع الولاء أو المسؤولية في الأسرة أو العمل أو السياسة أو الإدارة.",
      tr: "Aile, iş, siyaset ya da resmi işlerde sadakat veya sorumluluk çatışması için kullanılır.",
      ru: "Используется при конфликте лояльности или ответственности в семье, работе, политике или администрации.",
      ckb: "بۆ ناکۆکیی وەفاداری یان بەرپرسیارێتی لە خێزان، کار، سیاسەت یان ئیدارەدا بەکار دێت.",
      kmr: "Ji bo nakokiya dilsozî an berpirsiyariyê di malbat, kar, siyaset an îdareyê de tê bikaranîn.",
      pl: "Używa się tego przy konflikcie lojalności albo odpowiedzialności w rodzinie, pracy, polityce albo administracji.",
      ro: "Se folosește pentru conflicte de loialitate sau responsabilitate în familie, muncă, politică sau administrație.",
      sq: "Përdoret për konflikt besnikërie ose përgjegjësie në familje, punë, politikë ose administratë."
    },
    reason: "It is a nuanced conflict idiom for situations where no side is comfortable.",
    examples: [
      {
        de: "Als Teamleiter sitze ich zwischen den Stühlen: Die Geschäftsführung will sparen, das Team braucht mehr Zeit.",
        tr: {
          en: "As team lead, I am caught between two sides: management wants savings, the team needs more time.",
          fa: "به عنوان سرپرست تیم، بین دو طرف گیر کرده‌ام: مدیریت می‌خواهد صرفه‌جویی کند، تیم زمان بیشتری می‌خواهد.",
          ar: "كقائد فريق أنا عالق بين طرفين: الإدارة تريد التوفير، والفريق يحتاج إلى وقت أكثر.",
          tr: "Ekip lideri olarak iki taraf arasında kaldım: yönetim tasarruf istiyor, ekip daha fazla zamana ihtiyaç duyuyor.",
          ru: "Как руководитель команды, я оказался между двух сторон: руководство хочет экономить, команде нужно больше времени.",
          ckb: "وەک سەرپەرشتیاری تیم، لە نێوان دوو لایەندا گیرم: بەڕێوەبەرایەتی دەیەوێت پاشەکەوت بکات، تیمەکە کاتی زیاتر دەوێت.",
          kmr: "Wek rêberê tîmê, ez di navbera du aliyan de me: rêveberî dixwaze pars bike, tîmê demek zêdetir divê.",
          pl: "Jako lider zespołu jestem między dwiema stronami: zarząd chce oszczędzać, zespół potrzebuje więcej czasu.",
          ro: "Ca lider de echipă sunt prins între două părți: conducerea vrea economii, echipa are nevoie de mai mult timp.",
          sq: "Si drejtues ekipi jam mes dy palëve: drejtimi kërkon kursime, ekipi ka nevojë për më shumë kohë."
        }
      },
      {
        de: "In dem Streit zwischen Eltern und Schule sitzt das Kind zwischen den Stühlen.",
        tr: {
          en: "In the conflict between parents and school, the child is caught in the middle.",
          fa: "در اختلاف بین والدین و مدرسه، کودک وسط گیر افتاده است.",
          ar: "في الخلاف بين الوالدين والمدرسة، الطفل عالق في الوسط.",
          tr: "Velilerle okul arasındaki anlaşmazlıkta çocuk ortada kalıyor.",
          ru: "В конфликте между родителями и школой ребенок оказывается между двух сторон.",
          ckb: "لە ناکۆکیی نێوان دایک‌وباوک و قوتابخانەدا، منداڵەکە لە ناوەڕاستدا گیر دەبێت.",
          kmr: "Di nakokiya di navbera dêûbav û dibistanê de, zarok di navê de dimîne.",
          pl: "W sporze między rodzicami a szkołą dziecko jest między dwiema stronami.",
          ro: "În conflictul dintre părinți și școală, copilul este prins la mijloc.",
          sq: "Në konfliktin mes prindërve dhe shkollës, fëmija mbetet në mes."
        }
      }
    ]
  },
  {
    slug: "aus-dem-groebsten-raus-sein",
    text: "Aus dem Gröbsten raus sein.",
    level: "C1",
    type: "colloquial-phrase",
    transparency: "semi-idiomatic",
    register: "colloquial",
    category: "planning-and-projects",
    topics: ["planning-and-projects", "quality-and-risk"],
    literal: {
      en: "To be out of the roughest part.",
      fa: "از خشن‌ترین بخش بیرون بودن.",
      ar: "الخروج من الجزء الأخشن.",
      tr: "En kaba bölümden çıkmış olmak.",
      ru: "Выйти из самой грубой части.",
      ckb: "لە قورس‌ترین بەش دەرچوون.",
      kmr: "Ji beşa herî dijwar derketin.",
      pl: "Być poza najtrudniejszą częścią.",
      ro: "A fi ieșit din partea cea mai grea.",
      sq: "Të kesh dalë nga pjesa më e vështirë."
    },
    actual: {
      en: "The worst or most difficult part is over.",
      fa: "بدترین یا سخت‌ترین بخش تمام شده است.",
      ar: "انتهى الجزء الأسوأ أو الأصعب.",
      tr: "En kötü ya da en zor kısım geride kaldı.",
      ru: "Самая плохая или трудная часть уже позади.",
      ckb: "خراپترین یان قورسترین بەش تەواو بووە.",
      kmr: "Beşa herî xirab an herî dijwar êdî derbas bûye.",
      pl: "Najgorsza albo najtrudniejsza część już minęła.",
      ro: "Partea cea mai rea sau cea mai grea a trecut.",
      sq: "Pjesa më e keqe ose më e vështirë ka kaluar."
    },
    usage: {
      en: "Use it when a crisis, illness, project, or stressful phase is not finished but the worst part has passed.",
      fa: "وقتی بحران، بیماری، پروژه یا دورهٔ سخت هنوز تمام نشده اما بدترین بخش گذشته، به کار می‌رود.",
      ar: "تُستخدم عندما لا تكون الأزمة أو المرض أو المشروع أو المرحلة الصعبة قد انتهت، لكن الأسوأ قد مر.",
      tr: "Kriz, hastalık, proje ya da stresli dönem bitmemiş olsa da en kötü kısmı geçince kullanılır.",
      ru: "Используется, когда кризис, болезнь, проект или стрессовый этап еще не закончены, но худшее уже позади.",
      ckb: "کاتێک قەیران، نەخۆشی، پرۆژە یان قۆناغی پڕفشار تەواو نەبووە بەڵام خراپترین بەش تێپەڕیوە، بەکار دێت.",
      kmr: "Dema qeyran, nexweşî, proje an qonaxa stresê hîn neqediya be lê beşa herî xirab derbas bûbe, tê bikaranîn.",
      pl: "Używa się tego, gdy kryzys, choroba, projekt albo stresujący etap jeszcze trwa, ale najgorsze minęło.",
      ro: "Se folosește când o criză, boală, proiect sau perioadă stresantă nu s-a terminat, dar partea cea mai grea a trecut.",
      sq: "Përdoret kur një krizë, sëmundje, projekt ose fazë stresuese nuk ka mbaruar, por pjesa më e keqe ka kaluar."
    },
    reason: "It is a nuanced relief phrase for partial recovery or progress, not just a literal spatial expression.",
    examples: [
      {
        de: "Der Umzug ist noch nicht vorbei, aber wir sind aus dem Gröbsten raus.",
        tr: {
          en: "The move is not over yet, but the worst part is behind us.",
          fa: "اسباب‌کشی هنوز تمام نشده، اما سخت‌ترین بخش را پشت سر گذاشته‌ایم.",
          ar: "الانتقال لم ينته بعد، لكن أصعب جزء أصبح خلفنا.",
          tr: "Taşınma henüz bitmedi ama en zor kısmı geride bıraktık.",
          ru: "Переезд еще не закончен, но самое трудное уже позади.",
          ckb: "کۆچکردنەکە هێشتا تەواو نەبووە، بەڵام قورسترین بەشمان تێپەڕاندووە.",
          kmr: "Koçberî hîn neqediya ye, lê me beşa herî dijwar derbas kiriye.",
          pl: "Przeprowadzka jeszcze się nie skończyła, ale najtrudniejsze już za nami.",
          ro: "Mutarea nu s-a terminat încă, dar partea cea mai grea a trecut.",
          sq: "Shpërngulja nuk ka mbaruar ende, por pjesa më e vështirë ka kaluar."
        }
      },
      {
        de: "Nach zwei Wochen intensiver Arbeit ist das Projekt aus dem Gröbsten raus.",
        tr: {
          en: "After two weeks of intense work, the project is past the hardest phase.",
          fa: "بعد از دو هفته کار فشرده، پروژه از سخت‌ترین مرحله عبور کرده است.",
          ar: "بعد أسبوعين من العمل المكثف تجاوز المشروع أصعب مرحلة.",
          tr: "İki haftalık yoğun çalışmadan sonra proje en zor aşamayı geçti.",
          ru: "После двух недель интенсивной работы проект прошел самый трудный этап.",
          ckb: "دوای دوو هەفتە کاری چڕ، پرۆژەکە قورسترین قۆناغی تێپەڕاندووە.",
          kmr: "Piştî du hefte karê giran, proje qonaxa herî dijwar derbas kiriye.",
          pl: "Po dwóch tygodniach intensywnej pracy projekt ma najtrudniejszy etap za sobą.",
          ro: "După două săptămâni de muncă intensă, proiectul a trecut de faza cea mai grea.",
          sq: "Pas dy javësh pune intensive, projekti e ka kaluar fazën më të vështirë."
        }
      }
    ]
  }
];

function makeEntry(entry, index) {
  const base = {
    slug: entry.slug,
    expressionText: entry.text,
    literalMeaningText: entry.literal.en,
    actualMeaningText: entry.actual.en,
    usageExplanation: entry.usage.en,
    teachingReason: entry.reason,
    cefrLevel: entry.level,
    expressionType: entry.type,
    register: entry.register,
    category: entry.category,
    region: "de",
    topics: entry.topics,
    isPublished: true,
    sortOrder: (index + 1) * 10,
    meaningTransparency: entry.transparency,
    safetyRating: entry.isRisky ? "mild-rude" : "general",
    minimumAge: entry.isRisky ? 16 : 0,
    requiresAdultAccess: false,
    meanings: languages.map(language => ({
      language,
      actualMeaningText: entry.actual[language],
      literalMeaningText: entry.literal[language],
      usageExplanation: entry.usage[language]
    })),
    examples: entry.examples.map((example, exampleIndex) => ({
      germanText: example.de,
      translations: languages.map(language => ({
        language,
        text: example.tr[language]
      })),
      sortOrder: (exampleIndex + 1) * 10
    }))
  };

  if (entry.isRisky) {
    base.isRisky = true;
    base.warnings = entry.warnings;
  }

  return base;
}

const packageJson = {
  packageVersion: "1.0",
  packageId: "expressions-mixed-supplement-01-v1",
  packageName: "Expressions Mixed Supplement 01 v1",
  source: "learning-portal-expressions",
  defaultMeaningLanguages: languages,
  entries: [],
  expressionEntries: entries.map(makeEntry)
};

fs.mkdirSync(path.dirname(outputPath), { recursive: true });
fs.writeFileSync(outputPath, `${JSON.stringify(packageJson, null, 2)}\n`, "utf8");
console.log(`Wrote ${outputPath} with ${packageJson.expressionEntries.length} expressions.`);
